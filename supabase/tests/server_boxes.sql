begin;
create temp table pockle_test_ids as select gen_random_uuid() u,gen_random_uuid() s,gen_random_uuid() device,
 gen_random_uuid() u2,gen_random_uuid() s2,gen_random_uuid() source;
grant select on pockle_test_ids to service_role;
insert into auth.users(id,is_anonymous) select u,false from pockle_test_ids union all select u2,false from pockle_test_ids;
insert into auth.identities(user_id,provider,provider_id,identity_data)
 select u,'google',u::text,jsonb_build_object('sub',u::text) from pockle_test_ids union all
 select u2,'google',u2::text,jsonb_build_object('sub',u2::text) from pockle_test_ids;
insert into auth.sessions(id,user_id) select s,u from pockle_test_ids union all select s2,u2 from pockle_test_ids;
set local role service_role;
do $$
declare x record; p uuid; r jsonb; ids jsonb; delivery jsonb; original jsonb; collection text; raw text; n int; seen text[]:='{}';
begin
 select * into x from pockle_test_ids; p:=(public.pockle_account_api(x.u,x.s,'profile')->>'player_id')::uuid;
 r:=public.pockle_box_api(x.u,x.s,'walking_report',jsonb_build_object('device_id',x.device,'sequence',1,'sensor_total',5000,'boot','a'));
 assert (r->>'step_credit')::int=0,'A new sensor baseline cannot mint historical steps';
 assert r=public.pockle_box_api(x.u,x.s,'walking_report',jsonb_build_object('device_id',x.device,'sequence',1,'sensor_total',5000,'boot','a'));
 assert r->>'cooldown_seconds'='3600' and r->>'steps_per_box'='1000' and r->>'daily_limit'='4' and r->>'bank_limit'='2';
 begin perform public.pockle_box_api(x.u,x.s,'walking_report',jsonb_build_object('device_id',x.device,'sequence',2,'sensor_total',9000,'boot','a'));raise exception 'rate accepted';exception when raise_exception then if sqlerrm<>'counter_rate_rejected' then raise;end if;end;
 update pockle_private.box_wallets set next_box_at=now()-interval '2 hours',step_credit=6000 where player_id=p;
 r:=public.pockle_box_api(x.u,x.s,'boxes'); assert (r->>'banked')::int=2 and (r->>'earned_today')::int=2; ids:=r->'boxes';
 assert r->>'step_credit'='4000' and abs(extract(epoch from ((r->>'next_box_at')::timestamptz-now()))-3600)<1,'Catch-up costs steps and leaves one-hour timer';
 assert r->'odds'='[{"tier":0,"chance":0.88},{"tier":1,"chance":0.08},{"tier":2,"chance":0.03},{"tier":3,"chance":0.01}]'::jsonb;
 r:=public.pockle_box_api(x.u,x.s,'boxes','{"now":"2999-01-01","steps":99999999,"collection_id":"pip"}');
 assert r->'boxes'=ids and r->>'banked'='2','Client clock/steps/collection cannot alter saved boxes';
 begin perform public.pockle_box_api(x.u,x.s,'box_finish',jsonb_build_object('box_id',ids->0->>'id'));raise exception 'finished unopened';exception when raise_exception then if sqlerrm<>'box_not_opened' then raise;end if;end;
 r:=public.pockle_box_api(x.u,x.s,'box_open',jsonb_build_object('box_id',ids->0->>'id')); assert r->>'banked'='1';
 delivery:=r->'delivery'; original:=delivery->'item';
 assert delivery->>'box_id'=ids->0->>'id' and delivery->>'collection_id'=ids->0->>'collection_id';
 assert (delivery->>'earned_day')::int=(now() at time zone 'UTC')::date-date '1970-01-01';
 assert exists(select 1 from pockle_private.paid_box_finishes where id=original->>'collectible_id'
   and collection_id=original->>'collection_id' and tier=(original->>'tier')::int),'Saved result must be eligible';
 assert (select result_item from pockle_private.box_reservations where id=(ids->0->>'id')::uuid)=original,'Result saved before response';
 r:=public.pockle_box_api(x.u,x.s,'box_open',jsonb_build_object('box_id',ids->0->>'id'));
 assert r->>'banked'='1' and r->'delivery'=delivery and jsonb_array_length(r->'deliveries')=1,'Open replay cannot grant twice or reroll';
 r:=public.pockle_box_api(x.u,x.s,'box_finish',jsonb_build_object('box_id',ids->0->>'id'));
 assert r->'delivery'->>'finished'='true' and r->'delivery'->'item'=original and jsonb_array_length(r->'deliveries')=1;
 r:=public.pockle_box_api(x.u,x.s,'box_finish',jsonb_build_object('box_id',ids->0->>'id'));
 assert r->'delivery'->>'finished'='true' and r->'delivery'->'item'=original,'Finish replay changed result';
 r:=public.pockle_box_api(x.u,x.s,'boxes'); assert r->'deliveries'->0->'item'=original and r->'deliveries'->0->>'finished'='true','Completed receipts needed to restore older saves';
 r:=public.pockle_box_api(x.u,x.s,'box_open',jsonb_build_object('box_id',ids->0->>'id'));
 assert r->'delivery'->'item'=original and r->'delivery'->>'finished'='true','Finished open must not reroll';
 begin update pockle_private.box_reservations set result_item=result_item||'{"tier":99}' where id=(ids->0->>'id')::uuid;raise exception 'mutated result';exception when raise_exception then if sqlerrm<>'walking_result_immutable' then raise;end if;end;
 begin update pockle_private.box_reservations set reveal_finished=false where id=(ids->0->>'id')::uuid;raise exception 'rewound finish';exception when raise_exception then if sqlerrm<>'walking_result_immutable' then raise;end if;end;
 begin update pockle_private.box_reservations set collection_id=case when collection_id='pip' then 'fox' else 'pip' end where id=(ids->1->>'id')::uuid;raise exception 'changed collection';exception when raise_exception then if sqlerrm<>'walking_result_immutable' then raise;end if;end;
 begin perform public.pockle_box_api(x.u2,x.s2,'box_open',jsonb_build_object('box_id',ids->0->>'id'));raise exception 'other owner opened';exception when raise_exception then if sqlerrm<>'box_not_owned' then raise;end if;end;
 begin perform public.pockle_box_api(x.u2,x.s2,'box_finish',jsonb_build_object('box_id',ids->0->>'id'));raise exception 'other owner finished';exception when raise_exception then if sqlerrm<>'box_not_owned' then raise;end if;end;
 assert public.pockle_box_api(x.u2,x.s2,'boxes')->'deliveries'='[]'::jsonb;
 begin perform public.pockle_box_api(x.u,x.s2,'boxes');raise exception 'wrong session';exception when invalid_authorization_specification then null;end;
 update pockle_private.box_wallets set next_box_at=now()-interval '1 second',earned_today=4 where player_id=p;
 assert public.pockle_box_api(x.u,x.s,'boxes')->>'banked'='1','UTC daily cap';
 update pockle_private.box_wallets set day=(now() at time zone 'UTC')::date-1 where player_id=p;
 r:=public.pockle_box_api(x.u,x.s,'boxes'); assert r->>'banked'='2' and r->>'earned_today'='1';
 update pockle_private.walking_counters set observed_at=now()-interval '10 minutes' where player_id=p;
 r:=public.pockle_box_api(x.u,x.s,'walking_report',jsonb_build_object('device_id',x.device,'sequence',2,'sensor_total',6000,'boot','a'));
 assert r->>'step_credit'='4000','Accepted delta counted once';
 r:=public.pockle_box_api(x.u,x.s,'walking_report',jsonb_build_object('device_id',x.device,'sequence',2,'sensor_total',6000,'boot','a'));
 assert r->>'step_credit'='4000','Sensor replay does not add credit';
 r:=public.pockle_box_api(x.u,x.s,'walking_report',jsonb_build_object('device_id',x.device,'sequence',3,'sensor_total',10,'boot','b'));
 assert r->>'step_credit'='4000','Reboot establishes baseline without granting historical steps';
 -- iOS uses the local-day Core Motion query window as its counter epoch. The
 -- new day's first total can be larger than yesterday's last total after being offline.
 update pockle_private.walking_counters set observed_at=now()-interval '10 minutes' where player_id=p;
 r:=public.pockle_box_api(x.u,x.s,'walking_report',jsonb_build_object('device_id',x.device,'sequence',4,'sensor_total',1000,'boot','next-local-day'));
 assert r->>'step_credit'='4000','Changed query window must not difference unrelated day totals';
 r:=public.pockle_box_api(x.u,x.s,'walking_report',jsonb_build_object('device_id',x.device,'sequence',5,'sensor_total',1010,'boot','next-local-day'));
 assert r->>'step_credit'='4010','New steps in the same query window count once';
 r:=public.pockle_box_api(x.u,x.s,'walking_report',jsonb_build_object('device_id',x.device,'sequence',5,'sensor_total',1010,'boot','next-local-day'));
 assert r->>'step_credit'='4010','New-day report replay must not add credit';
 assert r->>'production_grants_enabled'='false';

 -- All enabled collections can be selected; disabled collections never receive new reservations.
 -- Fixture wallet resets isolate selection from real-world cooldown/day rules, tested above.
 update pockle_private.box_wallets set step_credit=0 where player_id=p;
 for ids in select jsonb_build_object('id',id) from pockle_private.box_reservations where player_id=p and status='banked' loop
   perform public.pockle_box_api(x.u,x.s,'box_open',jsonb_build_object('box_id',ids->>'id'));
 end loop;
 for collection in select id from pockle_private.box_collections order by position loop
   update pockle_private.box_collections set enabled=(id=collection);
   update pockle_private.box_wallets set next_box_at=now(),earned_today=0,step_credit=1000 where player_id=p;
   r:=public.pockle_box_api(x.u,x.s,'boxes'); assert r->>'banked'='1' and r->'boxes'->0->>'collection_id'=collection;
   r:=public.pockle_box_api(x.u,x.s,'box_open',jsonb_build_object('box_id',r->'boxes'->0->>'id'));
   assert r->'delivery'->'item'->>'collection_id'=collection;
 end loop;
 update pockle_private.box_collections set enabled=true;
 perform setseed(0.37);
 for n in 1..20 loop
   update pockle_private.box_wallets set next_box_at=now(),earned_today=0,step_credit=1000 where player_id=p;
   r:=public.pockle_box_api(x.u,x.s,'boxes');seen:=array_append(seen,r->'boxes'->0->>'collection_id');
   perform public.pockle_box_api(x.u,x.s,'box_open',jsonb_build_object('box_id',r->'boxes'->0->>'id'));
 end loop;
 assert (select count(distinct id) from unnest(seen) id)>1,'Collection draws must vary';

 perform public.pockle_account_api(x.u,x.s,'username',jsonb_build_object('username','W_'||substr(replace(x.source::text,'-',''),1,10)));
 raw:=jsonb_build_object('Version',6,'Inventory',jsonb_build_array(jsonb_build_object('Id',original->>'collectible_id','Count',2)),
   'Fusions','[]'::jsonb,'PaidDeliveryIds','[]'::jsonb,'WalkingDeliveryIds',jsonb_build_array(delivery->>'box_id'))::text;
 r:=public.pockle_account_api(x.u,x.s,'legacy_upload',jsonb_build_object('source_id',x.source,'save_json',raw));
 assert r->>'inventory_rows'='1' and public.pockle_account_api(x.u,x.s,'legacy_latest')->>'save_json'=raw,'v6 backup must preserve receipt IDs and exact save';
 assert not has_function_privilege('authenticated','public.pockle_box_api(uuid,uuid,text,jsonb)','EXECUTE');
 assert not has_table_privilege('authenticated','pockle_private.box_reservations','SELECT');
end $$;
rollback;
select 'PASS: 1-hour server clock, 1000 steps, bank2, max4/day, random saved collections, all11 enabled pools, immutable saved results, open/finish replay, ownership/session isolation, completed recovery, baseline/rate/reboot checks and exact v6 backup. Fixtures rolled back.' result;
