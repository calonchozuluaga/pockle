-- Clients have no direct table/RPC access. The Edge boundary verifies Apple/Google + a live session.
create schema if not exists pockle_private;
revoke all on schema pockle_private from public, anon, authenticated;
grant usage on schema pockle_private to service_role;
alter default privileges in schema pockle_private revoke execute on functions from public;

create table pockle_private.players (
 id uuid primary key default gen_random_uuid(), username text,
 username_key text generated always as (lower(username)) stored unique,
 status text not null default 'active' check(status in ('active','deletion_requested','deleted')),
 created_at timestamptz not null default now(), deletion_requested_at timestamptz, deleted_at timestamptz,
 check(username is null or username ~ '^[A-Za-z0-9_]{3,16}$')
);
create table pockle_private.player_accounts (
 auth_user_id uuid primary key references auth.users(id) on delete cascade,
 player_id uuid not null unique references pockle_private.players(id)
);
create table pockle_private.legacy_snapshots (
 id uuid primary key default gen_random_uuid(), player_id uuid not null references pockle_private.players(id),
 source_id uuid not null, tag text not null default 'legacy_beta_unverified' check(tag='legacy_beta_unverified'),
 digest text not null, save_text text not null check(octet_length(save_text)<=2097152),
 inventory_rows integer not null, fusion_rows integer not null,
 created_at timestamptz not null default clock_timestamp(),
 unique(player_id,source_id,digest)
);
create index legacy_snapshots_latest on pockle_private.legacy_snapshots(player_id,created_at desc,id);
create table pockle_private.account_deletions (
 player_id uuid primary key references pockle_private.players(id), requested_at timestamptz not null default now(),
 deadline timestamptz not null default now()+interval '30 days', completed_at timestamptz
);
-- Billing is not implemented. This retained, server-only table provides the deletion boundary now.
create table pockle_private.purchase_records (
 id uuid primary key default gen_random_uuid(), player_id uuid references pockle_private.players(id),
 platform text not null check(platform in ('apple','google')), transaction_hash text not null unique,
 purchased_at timestamptz not null, retained_reason text not null default 'accounting_and_refunds'
);
create index purchase_records_player on pockle_private.purchase_records(player_id);
create table pockle_private.name_attempts (
 player_id uuid primary key references pockle_private.players(id), window_at timestamptz not null, attempts int not null
);
create table pockle_private.reserved_names (name text primary key);
insert into pockle_private.reserved_names values ('admin'),('administrator'),('support'),('moderator'),('pockle'),('playpockle'),('official'),('null'),('undefined');

-- No client policies are intentional: only server functions can read/write private data.
do $$ declare t text; begin
 foreach t in array array['players','player_accounts','legacy_snapshots','account_deletions','purchase_records','name_attempts','reserved_names'] loop
  execute format('alter table pockle_private.%I enable row level security',t);
  execute format('revoke all on pockle_private.%I from public,anon,authenticated',t);
  execute format('grant select,insert,update,delete on pockle_private.%I to service_role',t);
 end loop;
end $$;
grant select(id,user_id,not_after) on auth.sessions to service_role;
grant select(id,is_anonymous,banned_until) on auth.users to service_role;
grant select(user_id,provider) on auth.identities to service_role;

create function pockle_private.actor(uid uuid,sid uuid) returns uuid
language plpgsql security invoker set search_path='' as $$
declare p uuid; begin
 if uid is null or sid is null or not exists(
  select 1 from auth.sessions s join auth.users u on u.id=s.user_id
  where s.id=sid and s.user_id=uid and not coalesce(u.is_anonymous,false)
  and (s.not_after is null or s.not_after>now()) and (u.banned_until is null or u.banned_until<now())
  and exists(select 1 from auth.identities i where i.user_id=uid and i.provider in ('apple','google'))
 ) then raise exception 'invalid_session' using errcode='28000'; end if;
 -- The auth subject is serialized before creating its stable player mapping.
 perform pg_advisory_xact_lock(hashtextextended(uid::text,0));
 select player_id into p from pockle_private.player_accounts where auth_user_id=uid;
 if p is null then
  insert into pockle_private.players default values returning id into p;
  insert into pockle_private.player_accounts values(uid,p);
 end if;
 perform 1 from pockle_private.players where id=p and status='active' for update;
 if not found then raise exception 'account_disabled' using errcode='28000'; end if;
 return p;
end $$;

create function pockle_private.account_api(uid uuid,sid uuid,action text,payload jsonb) returns jsonb
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
  if jsonb_typeof(snap)!='object' or (snap->>'Version')::int not between 1 and 4
   or jsonb_typeof(snap->'Inventory')!='array' or jsonb_typeof(coalesce(nullif(snap->'Fusions','null'::jsonb),'[]'))!='array'
   then raise exception 'invalid_snapshot'; end if;
  inv:=jsonb_array_length(snap->'Inventory');fus:=jsonb_array_length(coalesce(nullif(snap->'Fusions','null'::jsonb),'[]'));
  d:=encode(extensions.digest(convert_to(raw,'UTF8'),'sha256'),'hex');
  select id into receipt from pockle_private.legacy_snapshots where player_id=p and source_id=src and digest=d;
  if receipt is null then
   if (select count(*) from pockle_private.legacy_snapshots where player_id=p)>=100 then raise exception 'snapshot_limit'; end if;
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

-- One narrow exposed wrapper, callable by the Edge server role ONLY. It never accepts client-chosen actor IDs directly.
create function public.pockle_account_api(actor uuid,session_id uuid,action text,payload jsonb default '{}') returns jsonb
language sql security invoker set search_path='' as $$ select pockle_private.account_api(actor,session_id,action,payload); $$;
revoke all on function public.pockle_account_api(uuid,uuid,text,jsonb) from public,anon,authenticated;
grant execute on function public.pockle_account_api(uuid,uuid,text,jsonb) to service_role;
grant execute on all functions in schema pockle_private to service_role;

-- Sole definer function: private, input-free and only purges already requested accounts.
-- Auth deletion requires the database owner, not broad client/Edge DELETE access to auth.users.
create function pockle_private.purge_requested_accounts() returns integer
language plpgsql security definer set search_path='' as $$
declare r record; affected int:=0; begin
 for r in select p.id,a.auth_user_id from pockle_private.players p
  join pockle_private.player_accounts a on a.player_id=p.id where p.status='deletion_requested' for update of p loop
  delete from pockle_private.legacy_snapshots where player_id=r.id;
  delete from pockle_private.name_attempts where player_id=r.id;
  -- Purchase records retain only a pseudonymous player reference, never identity/profile/token payloads.
  delete from auth.users where id=r.auth_user_id;
  update pockle_private.players set username=null,status='deleted',deleted_at=now(),deletion_requested_at=null where id=r.id;
  update pockle_private.account_deletions set completed_at=now() where player_id=r.id;
  affected:=affected+1;
 end loop;
 return affected;
end $$;
revoke all on function pockle_private.purge_requested_accounts() from public,anon,authenticated,service_role;
create extension if not exists pg_cron with schema pg_catalog;
select cron.schedule('pockle-account-erasure','*/5 * * * *','select pockle_private.purge_requested_accounts()');
