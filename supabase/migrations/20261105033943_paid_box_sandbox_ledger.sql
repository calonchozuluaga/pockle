-- Internal store testing only. Production transactions are explicitly rejected.
create table pockle_private.paid_box_policy (
 singleton boolean primary key default true check(singleton), sandbox_enabled boolean not null default false
);
insert into pockle_private.paid_box_policy values(true,false);
create table pockle_private.paid_box_finishes (
 id text primary key, collection_id text not null, tier integer not null check(tier between 0 and 3)
);
create table pockle_private.paid_box_products (
 id text primary key, collection_id text, box_count integer not null check(box_count in (1,3,5)),
 check((collection_id is not null and box_count=1) or (collection_id is null and box_count in (3,5)))
);
create table pockle_private.paid_box_deliveries (
 id uuid primary key default gen_random_uuid(), player_id uuid not null references pockle_private.players(id),
 store text not null check(store in ('app_store','play_store')), environment text not null check(environment='SANDBOX'),
 transaction_id text not null check(length(transaction_id) between 1 and 200),
 product_id text not null references pockle_private.paid_box_products(id), purchased_at timestamptz not null,
 items jsonb not null, revealed integer not null default 0 check(revealed between 0 and 5),
 finished boolean not null default false, created_at timestamptz not null default now(),
 unique(store,environment,transaction_id)
);
create index paid_box_deliveries_player on pockle_private.paid_box_deliveries(player_id,created_at,id);
do $$ declare t text; begin
 foreach t in array array['paid_box_policy','paid_box_finishes','paid_box_products','paid_box_deliveries'] loop
  execute format('alter table pockle_private.%I enable row level security',t);
  execute format('revoke all on pockle_private.%I from public,anon,authenticated',t);
  execute format('grant select,insert,update on pockle_private.%I to service_role',t);
 end loop;
end $$;

-- This is a service-only boundary. uid/sid come from Auth, verified transactions from RevenueCat.
create function public.pockle_paid_box_api(actor uuid,session_id uuid,action text,payload jsonb default '{}') returns jsonb
language plpgsql security invoker set search_path='' as $$
declare p uuid; product pockle_private.paid_box_products; delivery pockle_private.paid_box_deliveries;
 groups jsonb; result_items jsonb:='[]'; chosen text; cid text; selected_tier int; roll float8; n int; ids text[]; prior bool;
begin
 p:=pockle_private.actor(actor,session_id);
 if action='status' then
  return jsonb_build_object('available',(select sandbox_enabled from pockle_private.paid_box_policy where singleton),'environment','SANDBOX');
 end if;
 if not (select sandbox_enabled from pockle_private.paid_box_policy where singleton) then raise exception 'checkout_unavailable';end if;
 if action='quote' then
  select * into product from pockle_private.paid_box_products where id=payload->>'product_id';
  if not found then raise exception 'invalid_product';end if;
  select jsonb_agg(jsonb_build_object('collection_id',c.collection_id,'collection_percent',case when product.collection_id is null then 100.0/c.total else null end,
   'rows',(select jsonb_agg(jsonb_build_object('tier',tier,'percent',weight) order by tier) from (
    select tier,case tier when 0 then 100 - coalesce(sum(case tier when 1 then 8 when 2 then 3 when 3 then 1 else 0 end) over(),0)
     when 1 then 8 when 2 then 3 when 3 then 1 end as weight
    from (select distinct tier from pockle_private.paid_box_finishes f where f.collection_id=c.collection_id) t
   ) weights))) into groups
  from (select collection_id,count(*) over() as total from (select distinct collection_id from pockle_private.paid_box_finishes where product.collection_id is null or collection_id=product.collection_id) collections) c;
  return jsonb_build_object('quote_id',gen_random_uuid(),'product_id',product.id,'box_count',product.box_count,'collection_id',product.collection_id,'odds',groups,
   'notes',jsonb_build_array('Internal sandbox test. No production editions are issued.','Collections in a Mystery Mix are equally likely; duplicates are possible.','No completion boost is applied in this test.'));
 elsif action='deliver' then
  if payload->>'environment' is distinct from 'SANDBOX' then raise exception 'production_checkout_disabled';end if;
  if payload->>'store' not in ('app_store','play_store') or payload->>'store' is null then raise exception 'invalid_store';end if;
  select * into product from pockle_private.paid_box_products where id=payload->>'product_id';
  if not found then raise exception 'invalid_product';end if;
  -- Shared lock also serializes a transaction racing across accounts. No overwrite on conflict.
  perform pg_advisory_xact_lock(hashtextextended('pockle:paid:sandbox',0));
  select * into delivery from pockle_private.paid_box_deliveries where store=payload->>'store' and environment='SANDBOX' and transaction_id=payload->>'transaction_id';
  if found then
   if delivery.player_id<>p or delivery.product_id<>product.id then raise exception 'receipt_owner_mismatch';end if;
   return jsonb_build_object('receipt_id',delivery.id);
  end if;
  for n in 1..product.box_count loop
   cid:=product.collection_id;
   if cid is null then select array_agg(collection_id order by collection_id) into ids from (select distinct collection_id from pockle_private.paid_box_finishes) c;cid:=ids[1+floor(random()*array_length(ids,1))::int];end if;
   roll:=random()*100;selected_tier:=0;
   -- Absent tier weights return to Common; Wally is 88/8/3/1, two-tier collections 99/1.
   if roll<1 and exists(select 1 from pockle_private.paid_box_finishes where collection_id=cid and tier=3) then selected_tier:=3;
   elsif roll>=1 and roll<4 and exists(select 1 from pockle_private.paid_box_finishes where collection_id=cid and tier=2) then selected_tier:=2;
   elsif roll>=4 and roll<12 and exists(select 1 from pockle_private.paid_box_finishes where collection_id=cid and tier=1) then selected_tier:=1;end if;
   select array_agg(id order by id) into ids from pockle_private.paid_box_finishes where collection_id=cid and tier=selected_tier;
   if coalesce(array_length(ids,1),0)=0 then raise exception 'catalog_unconfigured';end if;
   chosen:=ids[1+floor(random()*array_length(ids,1))::int];
   select exists(select 1 from pockle_private.paid_box_deliveries d cross join lateral jsonb_array_elements(d.items) i where player_id=p and i->>'collectible_id'=chosen)
    or exists(select 1 from jsonb_array_elements(result_items) i where i->>'collectible_id'=chosen)
    or exists(select 1 from pockle_private.legacy_snapshots l cross join lateral jsonb_array_elements(l.save_text::jsonb->'Inventory') i where l.player_id=p and i->>'Id'=chosen and (i->>'Count')::int>0) into prior;
   result_items:=result_items||jsonb_build_array(jsonb_build_object('collectible_id',chosen,'collection_id',cid,'tier',selected_tier,'is_new',not prior));
  end loop;
  insert into pockle_private.paid_box_deliveries(player_id,store,environment,transaction_id,product_id,purchased_at,items)
   values(p,payload->>'store','SANDBOX',payload->>'transaction_id',product.id,(payload->>'purchased_at')::timestamptz,result_items) returning * into delivery;
  return jsonb_build_object('receipt_id',delivery.id);
 elsif action='progress' then
  update pockle_private.paid_box_deliveries set revealed=greatest(revealed,least(jsonb_array_length(items),greatest(0,(payload->>'revealed')::int))),
   finished=finished or coalesce((payload->>'finished')::boolean,false)
  where id=(payload->>'receipt_id')::uuid and player_id=p;
  if not found then raise exception 'receipt_not_owned';end if;
 elsif action<>'deliveries' then raise exception 'unknown_action';end if;
 return jsonb_build_object('deliveries',coalesce((select jsonb_agg(jsonb_build_object('receipt_id',id,'product_id',product_id,'items',items,'revealed',revealed,'finished',finished) order by created_at,id)
  from pockle_private.paid_box_deliveries where player_id=p),'[]'));
end $$;
revoke all on function public.pockle_paid_box_api(uuid,uuid,text,jsonb) from public,anon,authenticated;
grant execute on function public.pockle_paid_box_api(uuid,uuid,text,jsonb) to service_role;

-- Preserve the existing legacy backup contract while accepting the additive v5 receipt list.
create or replace function pockle_private.account_api(uid uuid,sid uuid,action text,payload jsonb) returns jsonb
language plpgsql security invoker set search_path='' as $$
declare p uuid; n text; normalized text; raw text; snap jsonb; d text; src uuid; receipt uuid; inv int; fus int; amount int; begin
 p:=pockle_private.actor(uid,sid);
 if action='profile' then
  return (select jsonb_build_object('player_id',id,'username',username) from pockle_private.players where id=p);
 elsif action='username' then
  insert into pockle_private.name_attempts values(p,now(),1) on conflict(player_id) do update
  set attempts=case when name_attempts.window_at<now()-interval '1 minute' then 1 else name_attempts.attempts+1 end,
      window_at=case when name_attempts.window_at<now()-interval '1 minute' then now() else name_attempts.window_at end
  returning attempts into amount;
  if amount>12 then return '{"status":"rate_limited"}'::jsonb; end if;
  n:=payload->>'username';normalized:=regexp_replace(translate(lower(n),'013457','oieast'),'[^a-z]','','g');
  if n is null or n !~ '^[A-Za-z0-9_]{3,16}$' or exists(select 1 from pockle_private.reserved_names where name=lower(n))
   or normalized ~ '(fuck|shit|cunt|nigger|nigga|faggot|hitler|nazi|porn|pussy)' then return '{"status":"not_allowed"}'::jsonb; end if;
  begin update pockle_private.players set username=n where id=p;
  exception when unique_violation then return '{"status":"taken"}'::jsonb; end;
  return jsonb_build_object('status','available','username',n);
 elsif action='legacy_upload' then
  if (select username is null from pockle_private.players where id=p) then raise exception 'username_required'; end if;
  raw:=payload->>'save_json';src:=(payload->>'source_id')::uuid;
  if raw is null or octet_length(raw)>2097152 or src is null then raise exception 'invalid_snapshot'; end if;
  snap:=raw::jsonb;
  if jsonb_typeof(snap)!='object' or ((snap->>'Version')::int between 1 and 5) is not true
   or jsonb_typeof(snap->'Inventory') is distinct from 'array' or jsonb_typeof(coalesce(nullif(snap->'Fusions','null'::jsonb),'[]'))!='array'
   then raise exception 'invalid_snapshot'; end if;
  inv:=jsonb_array_length(snap->'Inventory');fus:=jsonb_array_length(coalesce(nullif(snap->'Fusions','null'::jsonb),'[]'));
  d:=encode(extensions.digest(convert_to(raw,'UTF8'),'sha256'),'hex');
  select id into receipt from pockle_private.legacy_snapshots where player_id=p and source_id=src and digest=d;
  if receipt is null then
   -- Retain the immutable first import plus the latest99 backups. Never exhaust cloud backup during ordinary walking.
   delete from pockle_private.legacy_snapshots where id in (
    select id from pockle_private.legacy_snapshots where player_id=p and id<>(
     select id from pockle_private.legacy_snapshots where player_id=p order by created_at,id limit 1)
    order by created_at desc,id desc offset 98);
   insert into pockle_private.legacy_snapshots(player_id,source_id,digest,save_text,inventory_rows,fusion_rows)
    values(p,src,d,raw,inv,fus) returning id into receipt;
  end if;
  return jsonb_build_object('receipt_id',receipt,'digest',d,'inventory_rows',inv,'fusion_rows',fus,'tag','legacy_beta_unverified');
 elsif action='legacy_latest' then
  return coalesce((select jsonb_build_object('receipt_id',id,'digest',digest,'save_json',save_text,'tag',tag)
   from pockle_private.legacy_snapshots where player_id=p order by created_at desc,id desc limit 1),'{}'::jsonb);
 elsif action='delete_request' then
  insert into pockle_private.account_deletions(player_id) values(p) on conflict do nothing;
  update pockle_private.players set status='deletion_requested',username=null,deletion_requested_at=now() where id=p;
  -- Remove the collection backup immediately. A scheduled private worker removes the Auth record and all remaining personal data.
  delete from pockle_private.legacy_snapshots where player_id=p;
  return jsonb_build_object('accepted',true,'deadline',(select deadline from pockle_private.account_deletions where player_id=p));
 else raise exception 'unknown_action'; end if;
end $$;


insert into pockle_private.paid_box_finishes(id,collection_id,tier) values
('pip.peach-jelly','pip',0),
('pip.moon-pearl','pip',0),
('pip.gold-confetti','pip',0),
('pip.mint-mochi','pip',0),
('pip.velvet-flock','pip',0),
('pip.boucle-plush','pip',0),
('pip.matte-vinyl','pip',0),
('pip.gloss-vinyl','pip',0),
('pip.coated-metallic','pip',0),
('pip.glow-jelly','pip',3),
('fox.clear-jelly','fox',0),
('fox.pearl-jelly','fox',0),
('fox.confetti-gel','fox',0),
('fox.mochi-foam','fox',0),
('fox.velvet-flock','fox',0),
('fox.boucle-plush','fox',0),
('fox.matte-vinyl','fox',0),
('fox.gloss-vinyl','fox',0),
('fox.coated-metallic','fox',0),
('fox.glow-jelly','fox',3),
('axolotl.clear-jelly','axolotl',0),
('axolotl.pearl-jelly','axolotl',0),
('axolotl.confetti-gel','axolotl',0),
('axolotl.mochi-foam','axolotl',0),
('axolotl.velvet-flock','axolotl',0),
('axolotl.boucle-plush','axolotl',0),
('axolotl.matte-vinyl','axolotl',0),
('axolotl.gloss-vinyl','axolotl',0),
('axolotl.coated-metallic','axolotl',0),
('axolotl.glow-jelly','axolotl',3),
('firb.clear-jelly','firb',0),
('firb.pearl-jelly','firb',0),
('firb.confetti-gel','firb',0),
('firb.mochi-foam','firb',0),
('firb.velvet-flock','firb',0),
('firb.boucle-plush','firb',0),
('firb.matte-vinyl','firb',0),
('firb.gloss-vinyl','firb',0),
('firb.coated-metallic','firb',0),
('firb.glow-jelly','firb',3),
('coco.clear-jelly','coco',0),
('coco.pearl-jelly','coco',0),
('coco.confetti-gel','coco',0),
('coco.mochi-foam','coco',0),
('coco.velvet-flock','coco',0),
('coco.boucle-plush','coco',0),
('coco.matte-vinyl','coco',0),
('coco.gloss-vinyl','coco',0),
('coco.coated-metallic','coco',0),
('coco.glow-jelly','coco',3),
('snowy.clear-jelly','snowy',0),
('snowy.pearl-jelly','snowy',0),
('snowy.confetti-gel','snowy',0),
('snowy.mochi-foam','snowy',0),
('snowy.velvet-flock','snowy',0),
('snowy.boucle-plush','snowy',0),
('snowy.matte-vinyl','snowy',0),
('snowy.gloss-vinyl','snowy',0),
('snowy.coated-metallic','snowy',0),
('snowy.glow-jelly','snowy',3),
('ginger.clear-jelly','ginger',0),
('ginger.pearl-jelly','ginger',0),
('ginger.confetti-gel','ginger',0),
('ginger.mochi-foam','ginger',0),
('ginger.velvet-flock','ginger',0),
('ginger.boucle-plush','ginger',0),
('ginger.matte-vinyl','ginger',0),
('ginger.gloss-vinyl','ginger',0),
('ginger.coated-metallic','ginger',0),
('ginger.glow-jelly','ginger',3),
('reiny.clear-jelly','reiny',0),
('reiny.pearl-jelly','reiny',0),
('reiny.confetti-gel','reiny',0),
('reiny.mochi-foam','reiny',0),
('reiny.velvet-flock','reiny',0),
('reiny.boucle-plush','reiny',0),
('reiny.matte-vinyl','reiny',0),
('reiny.gloss-vinyl','reiny',0),
('reiny.coated-metallic','reiny',0),
('reiny.glow-jelly','reiny',3),
('guin.clear-jelly','guin',0),
('guin.pearl-jelly','guin',0),
('guin.confetti-gel','guin',0),
('guin.mochi-foam','guin',0),
('guin.velvet-flock','guin',0),
('guin.boucle-plush','guin',0),
('guin.matte-vinyl','guin',0),
('guin.gloss-vinyl','guin',0),
('guin.coated-metallic','guin',0),
('guin.glow-jelly','guin',3),
('bundy.clear-jelly','bundy',0),
('bundy.pearl-jelly','bundy',0),
('bundy.confetti-gel','bundy',0),
('bundy.mochi-foam','bundy',0),
('bundy.velvet-flock','bundy',0),
('bundy.boucle-plush','bundy',0),
('bundy.matte-vinyl','bundy',0),
('bundy.gloss-vinyl','bundy',0),
('bundy.coated-metallic','bundy',0),
('bundy.glow-jelly','bundy',3),
('wally.clear-jelly','wally',0),
('wally.pearl-jelly','wally',0),
('wally.confetti-gel','wally',0),
('wally.mochi-foam','wally',0),
('wally.velvet-flock','wally',0),
('wally.boucle-plush','wally',0),
('wally.matte-vinyl','wally',0),
('wally.gloss-vinyl','wally',0),
('wally.coated-metallic','wally',1),
('wally.glow-jelly','wally',2),
('wally.kelp-mint','wally',0),
('wally.treasure-leaf','wally',1),
('wally.rose-gold-shell','wally',1),
('wally.diamond-frost','wally',2),
('wally.molten-abyss','wally',3);
insert into pockle_private.paid_box_products(id,collection_id,box_count) values
('pockle.box.axolotl.v1','axolotl',1),
('pockle.box.bundy.v1','bundy',1),
('pockle.box.coco.v1','coco',1),
('pockle.box.firb.v1','firb',1),
('pockle.box.fox.v1','fox',1),
('pockle.box.ginger.v1','ginger',1),
('pockle.box.guin.v1','guin',1),
('pockle.box.pip.v1','pip',1),
('pockle.box.reiny.v1','reiny',1),
('pockle.box.snowy.v1','snowy',1),
('pockle.box.wally.v1','wally',1),
('pockle.mix.3.v1',null,3),
('pockle.mix.5.v1',null,5);
