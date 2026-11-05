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
declare x record; q jsonb; product record; g jsonb; total numeric; conditional numeric; actual numeric; r jsonb; before_count bigint;
begin
 select * into x from pockle_paid_test_ids;
 update pockle_private.paid_box_policy set sandbox_enabled=true where singleton;
 select count(*) into before_count from pockle_private.paid_box_deliveries;
 for product in select * from pockle_private.paid_box_products loop
  q:=public.pockle_paid_box_quote(x.u,x.s,'quote',jsonb_build_object('product_id',product.id));
  total:=0;
  for g in select value from jsonb_array_elements(q->'odds') loop
   assert jsonb_array_length(g->'contents')=(select count(*) from pockle_private.paid_box_finishes where collection_id=g->>'collection_id'),'Incomplete contents';
   select sum((i->>'percent')::numeric),sum((i->>'within_collection_percent')::numeric)
    into actual,conditional from jsonb_array_elements(g->'contents') i;
   total:=total+actual;assert abs(conditional-100)<.00001,'Conditional total changed';
   assert abs(actual-coalesce((g->>'collection_percent')::numeric,100))<.00001,'Mix collection probability missing';
   for r in select value from jsonb_array_elements(g->'rows') loop
    select sum((i->>'within_collection_percent')::numeric) into actual from jsonb_array_elements(g->'contents') i where i->>'tier'=r->>'tier';
    assert abs(actual-(r->>'percent')::numeric)<.00001,'Tier probability mismatch';
   end loop;
  end loop;
  assert abs(total-100)<.00001,'Contents probability total changed';
 end loop;
 assert (select count(*) from pockle_private.paid_box_deliveries)=before_count,'Browsing granted a toy';
 begin perform public.pockle_paid_box_quote(x.u,x.s2,'quote','{"product_id":"pockle.box.fox.v1"}');raise exception 'Wrong session accepted';exception when invalid_authorization_specification then null;end;
 assert not has_function_privilege('authenticated','public.pockle_paid_box_quote(uuid,uuid,text,jsonb)','execute');
end $$;
rollback;
select 'PASS: all13products disclose complete per-finish contents, exact tier/collection/per-box totals, no grants, real session enforcement and private RPC. Fixtures rolled back.' result;
