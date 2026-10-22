create function pockle_private.box_api(uid uuid,sid uuid,action text,payload jsonb) returns jsonb
language plpgsql security invoker set search_path='' as $$
declare p uuid; w pockle_private.box_wallets; c pockle_private.walking_counters; bank int; selected text;
 pos int; latest jsonb; candidates text[]; dev uuid; seq bigint; total bigint; boot_id text; delta bigint; box uuid;
begin
 p:=pockle_private.actor(uid,sid); -- shared player lock makes reports/earns/opens serial and atomic
 insert into pockle_private.box_wallets(player_id) values(p) on conflict do nothing;
 select * into w from pockle_private.box_wallets where player_id=p for update;
 if action='walking_report' then
  dev:=(payload->>'device_id')::uuid;seq:=(payload->>'sequence')::bigint;total:=(payload->>'sensor_total')::bigint;boot_id:=payload->>'boot';
  if dev is null or seq<1 or seq is null or total<0 or total is null or length(boot_id) not between 1 and 80 or boot_id is null then raise exception 'invalid_counter';end if;
  select * into c from pockle_private.walking_counters where player_id=p;
  if c.player_id is null then
   insert into pockle_private.walking_counters(player_id,device_id,boot,sequence,sensor_total) values(p,dev,boot_id,seq,total);
  elsif c.device_id<>dev then raise exception 'walking_device_change_requires_review';
  elsif seq=c.sequence and total=c.sensor_total and boot_id=c.boot then null; -- exact replay
  elsif seq<=c.sequence then raise exception 'stale_counter';
  else
   delta:=case when boot_id=c.boot and total>=c.sensor_total then total-c.sensor_total else 0 end;
   if delta>least(100000,extract(epoch from (now()-c.observed_at))*6+30) then raise exception 'counter_rate_rejected';end if;
   w.step_credit:=least(100000,w.step_credit+delta);
   update pockle_private.walking_counters set boot=boot_id,sequence=seq,sensor_total=total,observed_at=now() where player_id=p;
  end if;
 elsif action='box_open' then
  box:=(payload->>'box_id')::uuid;
  update pockle_private.box_reservations set status='opened',opened_at=coalesce(opened_at,now()) where id=box and player_id=p;
  if not found then raise exception 'box_not_owned';end if;
 elsif action<>'boxes' then raise exception 'unknown_action';end if;
 if w.day<>(now() at time zone 'UTC')::date then w.day:=(now() at time zone 'UTC')::date;w.earned_today:=0;end if;
 select count(*) into bank from pockle_private.box_reservations where player_id=p and status='banked';
 -- No clock inputs accepted. Old slots can bank, but never more than two, four per UTC day, or one per six-hour boundary.
 if w.next_box_at<now()-interval '6 hours' then w.next_box_at:=now()-interval '6 hours';end if;
 while w.next_box_at<=now() and w.step_credit>=1000 and w.earned_today<4 and bank<2 loop
  select save_text::jsonb into latest from pockle_private.legacy_snapshots where player_id=p order by created_at desc,id desc limit 1;
  select array_agg(id order by position) into candidates from pockle_private.box_collections bc where enabled and exists(
   select 1 from unnest(bc.collectible_ids) needed where not exists(
    select 1 from jsonb_array_elements(coalesce(latest->'Inventory','[]')) item where item->>'Id'=needed and (item->>'Count')::int>0));
  if coalesce(array_length(candidates,1),0)=0 then select array_agg(id order by position) into candidates from pockle_private.box_collections where enabled;end if;
  select id,position into selected,pos from pockle_private.box_collections where id=any(candidates) and position>w.rotation_position order by position limit 1;
  if selected is null then select id,position into selected,pos from pockle_private.box_collections where id=any(candidates) order by position limit 1;end if;
  if selected is null then raise exception 'box_policy_unconfigured';end if;
  insert into pockle_private.box_reservations(player_id,collection_id) values(p,selected);
  w.rotation_position:=pos;selected:=null;w.step_credit:=w.step_credit-1000;w.earned_today:=w.earned_today+1;bank:=bank+1;w.next_box_at:=w.next_box_at+interval '6 hours';
 end loop;
 update pockle_private.box_wallets set next_box_at=w.next_box_at,day=w.day,earned_today=w.earned_today,step_credit=w.step_credit,rotation_position=w.rotation_position where player_id=p;
 return jsonb_build_object('server_time',now(),'next_box_at',w.next_box_at,'earned_today',w.earned_today,'step_credit',w.step_credit,'banked',bank,
 'verification','client_reported_beta','production_grants_enabled',false,
 'boxes',coalesce((select jsonb_agg(jsonb_build_object('id',id,'collection_id',collection_id,'earned_at',earned_at) order by earned_at,id) from pockle_private.box_reservations where player_id=p and status='banked'),'[]'::jsonb));
end $$;
create function public.pockle_box_api(actor uuid,session_id uuid,action text,payload jsonb default '{}') returns jsonb
language sql security invoker set search_path='' as $$select pockle_private.box_api(actor,session_id,action,payload);$$;
revoke all on function public.pockle_box_api(uuid,uuid,text,jsonb) from public,anon,authenticated;
grant execute on function public.pockle_box_api(uuid,uuid,text,jsonb) to service_role;
grant execute on function pockle_private.box_api(uuid,uuid,text,jsonb) to service_role;

create or replace function pockle_private.purge_requested_accounts() returns integer
language plpgsql security definer set search_path='' as $$
declare r record; affected int:=0; begin
 for r in select p.id,a.auth_user_id from pockle_private.players p join pockle_private.player_accounts a on a.player_id=p.id where p.status='deletion_requested' for update of p loop
  delete from pockle_private.legacy_snapshots where player_id=r.id;
  delete from pockle_private.name_attempts where player_id=r.id;
  delete from pockle_private.walking_counters where player_id=r.id;
  delete from pockle_private.box_wallets where player_id=r.id;
  delete from pockle_private.box_reservations where player_id=r.id;
  delete from auth.users where id=r.auth_user_id;
  update pockle_private.players set username=null,status='deleted',deleted_at=now(),deletion_requested_at=null where id=r.id;
  update pockle_private.account_deletions set completed_at=now() where player_id=r.id;
  affected:=affected+1;
 end loop;return affected;
end $$;
