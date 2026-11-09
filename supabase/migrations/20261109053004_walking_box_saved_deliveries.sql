-- Server-saved beta walking rewards. This does not allocate finite production editions.
-- Keep reservation IDs/collections and past results stable across retries, reconnects and saves.
alter table pockle_private.box_reservations
  add column result_item jsonb,
  add column reveal_finished boolean not null default false,
  add constraint box_result_shape check(result_item is null or
    (jsonb_typeof(result_item)='object' and result_item ?& array['collectible_id','collection_id','tier','is_new']
      and result_item->>'collection_id'=collection_id)),
  add constraint box_finished_has_result check(not reveal_finished or (status='opened' and result_item is not null));

create function pockle_private.protect_walking_result() returns trigger
language plpgsql security invoker set search_path='' as $$
begin
  if new.collection_id is distinct from old.collection_id
    or (old.result_item is not null and new.result_item is distinct from old.result_item)
    or (old.reveal_finished and not new.reveal_finished)
    or (old.status='opened' and new.status<>'opened') then
    raise exception 'walking_result_immutable';
  end if;
  return new;
end $$;
revoke all on function pockle_private.protect_walking_result() from public,anon,authenticated;
grant execute on function pockle_private.protect_walking_result() to service_role;
create trigger box_reservation_result_immutable before update on pockle_private.box_reservations
  for each row execute function pockle_private.protect_walking_result();

-- Preserve collection enable/disable choices; update only their supported finish lists.
update pockle_private.box_collections c set collectible_ids=f.ids
from (select collection_id,array_agg(id order by id) ids from pockle_private.paid_box_finishes group by collection_id) f
where c.id=f.collection_id;
-- A six-hour timer already running becomes the equivalent one-hour timer.
-- Do this once in migration, never on reconnect or repeated API calls.
update pockle_private.box_wallets set next_box_at=greatest(now()-interval '1 hour',next_box_at-interval '5 hours')
where next_box_at>now();

create or replace function pockle_private.box_api(uid uuid,sid uuid,action text,payload jsonb) returns jsonb
language plpgsql security invoker set search_path='' as $$
declare
  p uuid; w pockle_private.box_wallets; c pockle_private.walking_counters;
  opened pockle_private.box_reservations; bank int; selected text; candidates text[];
  dev uuid; seq bigint; total bigint; boot_id text; delta bigint; box uuid;
  chosen text; selected_tier int; roll float8; prior bool; result jsonb; odds jsonb;
begin
  p:=pockle_private.actor(uid,sid); -- real session and player lock serialize reports, earns and opens
  insert into pockle_private.box_wallets(player_id) values(p) on conflict do nothing;
  select * into w from pockle_private.box_wallets where player_id=p for update;
  if action='walking_report' then
    dev:=(payload->>'device_id')::uuid; seq:=(payload->>'sequence')::bigint;
    total:=(payload->>'sensor_total')::bigint; boot_id:=payload->>'boot';
    if dev is null or seq<1 or seq is null or total<0 or total is null
      or length(boot_id) not between 1 and 80 or boot_id is null then raise exception 'invalid_counter'; end if;
    select * into c from pockle_private.walking_counters where player_id=p;
    if c.player_id is null then
      insert into pockle_private.walking_counters(player_id,device_id,boot,sequence,sensor_total)
        values(p,dev,boot_id,seq,total);
    elsif c.device_id<>dev then raise exception 'walking_device_change_requires_review';
    elsif seq=c.sequence and total=c.sensor_total and boot_id=c.boot then null;
    elsif seq<=c.sequence then raise exception 'stale_counter';
    else
      delta:=case when boot_id=c.boot and total>=c.sensor_total then total-c.sensor_total else 0 end;
      if delta>least(100000,extract(epoch from (now()-c.observed_at))*6+30) then raise exception 'counter_rate_rejected'; end if;
      w.step_credit:=least(100000,w.step_credit+delta);
      update pockle_private.walking_counters set boot=boot_id,sequence=seq,sensor_total=total,observed_at=now() where player_id=p;
    end if;
  elsif action in ('box_open','box_finish') then
    box:=(payload->>'box_id')::uuid;
    select * into opened from pockle_private.box_reservations where id=box and player_id=p for update;
    if not found then raise exception 'box_not_owned'; end if;
    if action='box_finish' then
      if opened.result_item is null or opened.status<>'opened' then raise exception 'box_not_opened'; end if;
      update pockle_private.box_reservations set reveal_finished=true where id=box returning * into opened;
    elsif opened.result_item is null then
      -- Fixed approved rarity weights. Missing tiers fall back to Common as in paid draws.
      roll:=random()*100; selected_tier:=0;
      if roll<1 and exists(select 1 from pockle_private.paid_box_finishes where collection_id=opened.collection_id and tier=3) then selected_tier:=3;
      elsif roll>=1 and roll<4 and exists(select 1 from pockle_private.paid_box_finishes where collection_id=opened.collection_id and tier=2) then selected_tier:=2;
      elsif roll>=4 and roll<12 and exists(select 1 from pockle_private.paid_box_finishes where collection_id=opened.collection_id and tier=1) then selected_tier:=1; end if;
      select array_agg(id order by id) into candidates from pockle_private.paid_box_finishes
        where collection_id=opened.collection_id and tier=selected_tier;
      if coalesce(array_length(candidates,1),0)=0 then raise exception 'box_policy_unconfigured'; end if;
      chosen:=candidates[1+floor(random()*array_length(candidates,1))::int];
      select exists(select 1 from pockle_private.box_reservations r
        where r.player_id=p and r.result_item->>'collectible_id'=chosen)
        or exists(select 1 from pockle_private.paid_box_deliveries d cross join lateral jsonb_array_elements(d.items) i
          where d.player_id=p and i->>'collectible_id'=chosen)
        or exists(select 1 from pockle_private.legacy_snapshots l
          cross join lateral jsonb_array_elements(l.save_text::jsonb->'Inventory') i
          where l.player_id=p and i->>'Id'=chosen and (i->>'Count')::int>0) into prior;
      update pockle_private.box_reservations set status='opened',opened_at=coalesce(opened_at,now()),
        result_item=jsonb_build_object('collectible_id',chosen,'collection_id',collection_id,'tier',selected_tier,'is_new',not prior)
        where id=box returning * into opened;
    end if;
  elsif action<>'boxes' then raise exception 'unknown_action'; end if;

  if w.day<>(now() at time zone 'UTC')::date then w.day:=(now() at time zone 'UTC')::date; w.earned_today:=0; end if;
  select count(*) into bank from pockle_private.box_reservations where player_id=p and status='banked';
  -- Bound catch-up to two saved slots. The UTC day cap and step cost still apply.
  if w.next_box_at<now()-interval '1 hour' then w.next_box_at:=now()-interval '1 hour'; end if;
  while action<>'box_finish' and w.next_box_at<=now() and w.step_credit>=1000 and w.earned_today<4 and bank<2 loop
    select array_agg(bc.id order by bc.position) into candidates from pockle_private.box_collections bc
      where bc.enabled and exists(select 1 from pockle_private.paid_box_finishes f where f.collection_id=bc.id);
    if coalesce(array_length(candidates,1),0)=0 then raise exception 'box_policy_unconfigured'; end if;
    -- Select once at earning, uniformly across enabled collections, then persist the choice.
    selected:=candidates[1+floor(random()*array_length(candidates,1))::int];
    insert into pockle_private.box_reservations(player_id,collection_id) values(p,selected);
    w.step_credit:=w.step_credit-1000; w.earned_today:=w.earned_today+1;
    bank:=bank+1; w.next_box_at:=w.next_box_at+interval '1 hour';
  end loop;
  update pockle_private.box_wallets set next_box_at=w.next_box_at,day=w.day,
    earned_today=w.earned_today,step_credit=w.step_credit where player_id=p;

  select collection_id into selected from pockle_private.box_reservations
    where player_id=p and status='banked' order by ordinal limit 1;
  select jsonb_agg(jsonb_build_object('tier',tier,'chance',weight/100.0) order by tier) into odds from (
    select tier,case tier when 0 then 100-coalesce(sum(case tier when 1 then 8 when 2 then 3 when 3 then 1 else 0 end) over(),0)
      when 1 then 8 when 2 then 3 when 3 then 1 end weight
    from (select distinct tier from pockle_private.paid_box_finishes where collection_id=selected) tiers
  ) weights;
  result:=jsonb_build_object('server_time',now(),'next_box_at',w.next_box_at,'earned_today',w.earned_today,
    'step_credit',w.step_credit,'banked',bank,'cooldown_seconds',3600,'steps_per_box',1000,'daily_limit',4,'bank_limit',2,
    'verification','client_reported_beta','production_grants_enabled',false,'odds',coalesce(odds,'[]'),
    'boxes',coalesce((select jsonb_agg(jsonb_build_object('id',id,'collection_id',collection_id,'earned_at',earned_at) order by ordinal)
      from pockle_private.box_reservations where player_id=p and status='banked'),'[]'),
    -- Include completed receipts too: an older cloud snapshot can replay its missing IDs safely.
    'deliveries',coalesce((select jsonb_agg(jsonb_build_object('box_id',id,'collection_id',collection_id,'item',result_item,
      'finished',reveal_finished,'earned_day',(earned_at at time zone 'UTC')::date-date '1970-01-01') order by ordinal)
      from pockle_private.box_reservations where player_id=p and result_item is not null),'[]'));
  if opened.id is not null then
    result:=result||jsonb_build_object('delivery',jsonb_build_object('box_id',opened.id,'collection_id',opened.collection_id,
      'item',opened.result_item,'finished',opened.reveal_finished,'earned_day',(opened.earned_at at time zone 'UTC')::date-date '1970-01-01'));
  end if;
  return result;
end $$;
revoke all on function pockle_private.box_api(uuid,uuid,text,jsonb) from public,anon,authenticated;
grant execute on function pockle_private.box_api(uuid,uuid,text,jsonb) to service_role;

-- Accept v6 additive WalkingDeliveryIds without rewriting any original save or purchase receipts.
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
  if jsonb_typeof(snap)!='object' or ((snap->>'Version')::int between 1 and 6) is not true
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
