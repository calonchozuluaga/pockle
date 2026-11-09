// Run SQL suites on a fresh local PostgreSQL engine with minimal Auth tables.
// No remote project or user data is accessed; Auth JWT and pg_cron execution are not exercised.
import {PGlite} from '@electric-sql/pglite';
import {pgcrypto} from '@electric-sql/pglite/contrib/pgcrypto';
import fs from 'node:fs/promises';
const pg = new PGlite({extensions:{pgcrypto}});
try {
  await pg.exec(`create schema auth; create schema extensions;
    create role anon; create role authenticated; create role service_role bypassrls;
    grant usage on schema auth,extensions to service_role;
    create extension pgcrypto with schema extensions;
    create table auth.users(id uuid primary key,is_anonymous boolean default false,banned_until timestamptz);
    create table auth.identities(user_id uuid references auth.users on delete cascade,provider text,provider_id text,identity_data jsonb);
    create table auth.sessions(id uuid primary key,user_id uuid references auth.users on delete cascade,not_after timestamptz);`);
  const migrations = new URL('../../supabase/migrations/',import.meta.url);
  for (const name of (await fs.readdir(migrations)).filter(n => n.endsWith('.sql')).sort()) {
    const walkingMigration = name === '20261109053004_walking_box_saved_deliveries.sql';
    if (walkingMigration) {
      // Model a real six-hour timer and existing reservations before the upgrade.
      await pg.exec(`insert into pockle_private.players(id) values('99999999-9999-4999-8999-999999999999');
        insert into pockle_private.box_wallets(player_id,next_box_at,step_credit,earned_today)
          values('99999999-9999-4999-8999-999999999999',now()+interval '6 hours',450,2);
        insert into pockle_private.box_reservations(player_id,collection_id,status)
          values('99999999-9999-4999-8999-999999999999','fox','banked'),
          ('99999999-9999-4999-8999-999999999999','pip','opened');`);
    }
    let sql = await fs.readFile(new URL(name,migrations),'utf8');
    // Cron is unavailable in embedded Postgres. Keep the purge function; remove scheduling only.
    sql = sql.replace(/^create extension if not exists pg_cron.*\n/m,'')
      .replace(/^select cron\.schedule.*\n/m,'');
    await pg.exec(sql);
    if (walkingMigration) {
      await pg.exec(`do $$ begin
        assert (select abs(extract(epoch from (next_box_at-now()))-3600)<2 and step_credit=450 and earned_today=2
          from pockle_private.box_wallets where player_id='99999999-9999-4999-8999-999999999999'), 'Existing timer migration';
        assert (select count(*)=2 and bool_and(result_item is null and not reveal_finished)
          from pockle_private.box_reservations where player_id='99999999-9999-4999-8999-999999999999'), 'Migration cannot mint historical results';
        assert exists(select 1 from pockle_private.box_reservations
          where player_id='99999999-9999-4999-8999-999999999999' and collection_id='fox' and status='banked');
      end $$;
      delete from pockle_private.box_reservations where player_id='99999999-9999-4999-8999-999999999999';
      delete from pockle_private.box_wallets where player_id='99999999-9999-4999-8999-999999999999';
      delete from pockle_private.players where id='99999999-9999-4999-8999-999999999999';`);
    }
  }
  for (const name of ['account_foundation.sql','paid_box_sandbox.sql','paid_box_contents.sql','server_boxes.sql']) {
    const results = await pg.exec(await fs.readFile(new URL(`../../supabase/tests/${name}`,import.meta.url),'utf8'));
    console.log(name + ': ' + results.at(-1).rows[0].result);
  }
  console.log('PASS: full local migration chain and four SQL suites. Actual SQL actor/session validation; local Auth-table fixtures only, no live Auth service, cron worker or store purchase.');
} catch (error) {
  console.error(error.message, error.where || '', error.internalQuery || '');
  process.exitCode = 1;
} finally { await pg.close(); }
