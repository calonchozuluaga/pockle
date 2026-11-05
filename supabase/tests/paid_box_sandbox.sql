begin;
create temp table pockle_paid_test_ids as select gen_random_uuid() u,gen_random_uuid() s,gen_random_uuid() u2,gen_random_uuid() s2,gen_random_uuid() source;
grant select on pockle_paid_test_ids to service_role;
insert into auth.users(id,is_anonymous) select u,false from pockle_paid_test_ids union all select u2,false from pockle_paid_test_ids;
insert into auth.identities(user_id,provider,provider_id,identity_data)
 select u,'google',u::text,jsonb_build_object('sub',u::text) from pockle_paid_test_ids union all
 select u2,'apple',u2::text,jsonb_build_object('sub',u2::text) from pockle_paid_test_ids;
insert into auth.sessions(id,user_id) select s,u from pockle_paid_test_ids union all select s2,u2 from pockle_paid_test_ids;
set local role service_role;
do $$
declare x record; q jsonb; d jsonb; e jsonb; first_items jsonb; body jsonb; raw text; receipt uuid; begin
 select * into x from pockle_paid_test_ids;
 assert public.pockle_paid_box_api(x.u,x.s,'status')->>'available'='false','Test policy must default off';
 update pockle_private.paid_box_policy set sandbox_enabled=true where singleton;
 q:=public.pockle_paid_box_api(x.u,x.s,'quote','{"product_id":"pockle.box.wally.v1"}');
 assert q->'odds'->0->'rows'='[{"tier":0,"percent":88},{"tier":1,"percent":8},{"tier":2,"percent":3},{"tier":3,"percent":1}]'::jsonb;
 body:=jsonb_build_object('store','play_store','environment','SANDBOX','transaction_id','fixture-'||x.source,'product_id','pockle.mix.5.v1','purchased_at',now());
 d:=public.pockle_paid_box_api(x.u,x.s,'deliver',body);receipt:=(d->>'receipt_id')::uuid;
 assert d=public.pockle_paid_box_api(x.u,x.s,'deliver',body),'Transaction replay changed receipt';
 e:=public.pockle_paid_box_api(x.u,x.s,'deliveries');
 assert jsonb_array_length(e->'deliveries')=1 and jsonb_array_length(e->'deliveries'->0->'items')=5,'Pack size changed';
 first_items:=e->'deliveries'->0->'items';
 perform public.pockle_paid_box_api(x.u,x.s,'progress',jsonb_build_object('receipt_id',receipt,'revealed',3));
 perform public.pockle_paid_box_api(x.u,x.s,'progress',jsonb_build_object('receipt_id',receipt,'revealed',1));
 e:=public.pockle_paid_box_api(x.u,x.s,'deliveries');assert e->'deliveries'->0->>'revealed'='3' and e->'deliveries'->0->'items'=first_items;
 begin perform public.pockle_paid_box_api(x.u2,x.s2,'deliver',body);raise exception 'cross-account receipt accepted';exception when others then if sqlerrm<>'receipt_owner_mismatch' then raise;end if;end;
 begin perform public.pockle_paid_box_api(x.u,x.s,'deliver',body||'{"environment":"PRODUCTION"}');raise exception 'production accepted';exception when others then if sqlerrm<>'production_checkout_disabled' then raise;end if;end;
 begin perform public.pockle_paid_box_api(x.u,x.s2,'status');raise exception 'wrong session accepted';exception when invalid_authorization_specification then null;end;
 assert public.pockle_paid_box_api(x.u2,x.s2,'deliveries')->'deliveries'='[]'::jsonb;
 perform public.pockle_account_api(x.u,x.s,'username',jsonb_build_object('username','Q_'||substr(replace(x.source::text,'-',''),1,10)));
 raw:=jsonb_build_object('Version',5,'Inventory',(select jsonb_agg(jsonb_build_object('Id','future.'||n,'Count',n)) from generate_series(1,89)n),
 'Fusions',(select jsonb_agg(jsonb_build_object('Id','fusion.'||n)) from generate_series(1,15)n),'PaidDeliveryIds',jsonb_build_array(receipt))::text;
 e:=public.pockle_account_api(x.u,x.s,'legacy_upload',jsonb_build_object('source_id',x.source,'save_json',raw));
 assert (e->>'inventory_rows')::int=89 and (e->>'fusion_rows')::int=15;
 assert public.pockle_account_api(x.u,x.s,'legacy_latest')->>'save_json'=raw,'v5 exact backup changed';
 assert not has_function_privilege('authenticated','public.pockle_paid_box_api(uuid,uuid,text,jsonb)','EXECUTE');
 assert not has_table_privilege('authenticated','pockle_private.paid_box_deliveries','SELECT');
end $$;
rollback;
select 'PASS: sandbox receipt replay, five saved children, exact Wally odds, monotonic reveal, account/session isolation, production rejection and v5 89/15 exact legacy backup. All fixtures rolled back.' result;
