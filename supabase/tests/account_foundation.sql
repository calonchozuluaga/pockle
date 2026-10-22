begin;
create temp table pockle_test_ids as select gen_random_uuid() u,gen_random_uuid() s,gen_random_uuid() u2,gen_random_uuid() s2,gen_random_uuid() source;
grant select on pockle_test_ids to service_role;
insert into auth.users(id,is_anonymous) select u,false from pockle_test_ids union all select u2,false from pockle_test_ids;
insert into auth.identities(user_id,provider,provider_id,identity_data)
 select u,'google',u::text,jsonb_build_object('sub',u::text) from pockle_test_ids union all
 select u2,'apple',u2::text,jsonb_build_object('sub',u2::text) from pockle_test_ids;
insert into auth.sessions(id,user_id) select s,u from pockle_test_ids union all select s2,u2 from pockle_test_ids;
set local role service_role;
do $$
declare x record; a jsonb; b jsonb; raw text; receipt jsonb; begin
 select * into x from pockle_test_ids;
 a:=public.pockle_account_api(x.u,x.s,'profile');
 assert a->>'player_id' is not null;
 assert public.pockle_account_api(x.u,x.s,'username','{"username":"Admin"}')->>'status'='not_allowed';
 assert public.pockle_account_api(x.u,x.s,'username','{"username":"f_u_c_k"}')->>'status'='not_allowed';
 assert public.pockle_account_api(x.u,x.s,'username','{"username":"Moon_Fixture"}')->>'status'='available';
 assert public.pockle_account_api(x.u2,x.s2,'username','{"username":"moon_fixture"}')->>'status'='taken';
 assert public.pockle_account_api(x.u2,x.s2,'legacy_latest')='{}'::jsonb;
 raw:=jsonb_build_object('Version',4,'Inventory',(select jsonb_agg(jsonb_build_object('Id','future.'||n,'Count',n)) from generate_series(1,89)n),
 'Fusions',(select jsonb_agg(jsonb_build_object('Id','fusion.'||n,'UnknownFutureField','kept')) from generate_series(1,15)n),'UnknownFutureProperty','preserved')::text;
 receipt:=public.pockle_account_api(x.u,x.s,'legacy_upload',jsonb_build_object('source_id',x.source,'save_json',raw));
 assert (receipt->>'inventory_rows')::int=89 and (receipt->>'fusion_rows')::int=15;
 assert receipt=public.pockle_account_api(x.u,x.s,'legacy_upload',jsonb_build_object('source_id',x.source,'save_json',raw));
 assert public.pockle_account_api(x.u,x.s,'legacy_latest')->>'save_json'=raw;
 assert public.pockle_account_api(x.u2,x.s2,'legacy_latest')='{}'::jsonb;
 for amount in 1..105 loop
  perform public.pockle_account_api(x.u,x.s,'legacy_upload',jsonb_build_object('source_id',x.source,'save_json',(raw::jsonb||jsonb_build_object('BackupRevision',amount))::text));
 end loop;
 assert (select count(*) from pockle_private.legacy_snapshots where player_id=(a->>'player_id')::uuid)=100;
 assert receipt=public.pockle_account_api(x.u,x.s,'legacy_upload',jsonb_build_object('source_id',x.source,'save_json',raw)),'First import receipt retained through pruning';
 begin perform public.pockle_account_api(x.u,x.s2,'profile');raise exception 'wrong session accepted';exception when invalid_authorization_specification then null;end;
 assert not has_function_privilege('anon','public.pockle_account_api(uuid,uuid,text,jsonb)','EXECUTE');
 assert not has_function_privilege('authenticated','public.pockle_account_api(uuid,uuid,text,jsonb)','EXECUTE');
 assert not has_table_privilege('authenticated','pockle_private.legacy_snapshots','SELECT');
 assert public.pockle_account_api(x.u,x.s,'delete_request')->>'accepted'='true';
 begin perform public.pockle_account_api(x.u,x.s,'profile');raise exception 'disabled account accepted';exception when invalid_authorization_specification then null;end;
end $$;
reset role;
do $$ declare p uuid; x record; begin
 select * into x from pockle_test_ids;select player_id into p from pockle_private.player_accounts where auth_user_id=x.u;
 insert into pockle_private.purchase_records(player_id,platform,transaction_hash,purchased_at) values(p,'google','test-purchase-'||p,now());
 perform pockle_private.purge_requested_accounts();
 assert not exists(select 1 from auth.users where id=x.u);
 assert not exists(select 1 from auth.sessions where id=x.s);
 assert not exists(select 1 from pockle_private.legacy_snapshots where player_id=p);
 assert exists(select 1 from pockle_private.purchase_records where player_id=p);
 assert exists(select 1 from pockle_private.account_deletions where player_id=p and completed_at<=deadline);
end $$;
rollback;
select 'PASS: account identity, names, 89/15 exact snapshot, replay, isolation, live session, erasure and retained purchase' result;
