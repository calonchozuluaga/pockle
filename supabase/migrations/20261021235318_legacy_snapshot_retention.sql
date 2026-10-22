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
  if jsonb_typeof(snap)!='object' or ((snap->>'Version')::int between 1 and 4) is not true
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

