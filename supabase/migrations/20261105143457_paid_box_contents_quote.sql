-- Add per-finish disclosure to the existing server quote without changing draws or grants.
create function public.pockle_paid_box_quote(actor uuid,session_id uuid,action text,payload jsonb default '{}') returns jsonb
language plpgsql security invoker set search_path='' as $$
declare quote jsonb; g jsonb; contents jsonb; groups jsonb:='[]';
begin
 if action is distinct from 'quote' then raise exception 'invalid_action';end if;
 -- Retains the existing real-account/session, product and enabled-policy checks.
 quote:=public.pockle_paid_box_api(actor,session_id,'quote',payload);
 for g in select value from jsonb_array_elements(quote->'odds') loop
  select jsonb_agg(jsonb_build_object(
   'collectible_id',f.id,'tier',f.tier,
   'within_collection_percent',(r->>'percent')::numeric/f.finish_count,
   'percent',((r->>'percent')::numeric/f.finish_count)*coalesce((g->>'collection_percent')::numeric,100)/100
  ) order by f.tier,f.id) into contents
  from (select id,tier,count(*) over(partition by tier) as finish_count
   from pockle_private.paid_box_finishes where collection_id=g->>'collection_id') f
  join lateral jsonb_array_elements(g->'rows') r on (r->>'tier')::int=f.tier;
  groups:=groups||jsonb_build_array(g||jsonb_build_object('contents',contents));
 end loop;
 return jsonb_set(quote,'{odds}',groups);
end $$;
revoke all on function public.pockle_paid_box_quote(uuid,uuid,text,jsonb) from public,anon,authenticated;
grant execute on function public.pockle_paid_box_quote(uuid,uuid,text,jsonb) to service_role;
