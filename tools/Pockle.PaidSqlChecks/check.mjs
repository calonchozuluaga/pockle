import {PGlite} from '@electric-sql/pglite';
import fs from 'node:fs/promises';
import {execFileSync} from 'node:child_process';
import {fileURLToPath} from 'node:url';

const assert = (ok, message) => { if (!ok) throw Error(message); };
const readMigration = name => fs.readFile(new URL(`../../supabase/migrations/${name}`, import.meta.url), 'utf8');
const dotnetOutput = execFileSync(process.env.POCKLE_DOTNET || 'dotnet', [
  'run', '--project', fileURLToPath(new URL('CatalogExport.csproj', import.meta.url)),
  '--verbosity', 'quiet', '-p:UseSharedCompilation=false',
], {encoding:'utf8', env:{...process.env, DOTNET_CLI_TELEMETRY_OPTOUT:'1'}});
const client = JSON.parse(dotnetOutput.trim().split('\n').findLast(line => line.startsWith('{')));
const clientById = new Map(client.finishes.map(f => [f.id, f]));
assert(client.finishes.length === 165 && client.products.length === 13, 'Unexpected current launch roster');

const pg = new PGlite();
try {
  await pg.exec(`create schema pockle_private; create role anon; create role authenticated; create role service_role;
    create table pockle_private.players(id uuid primary key,status text default 'active');
    create table pockle_private.legacy_snapshots(player_id uuid,save_text text);
    create function pockle_private.actor(uid uuid,sid uuid) returns uuid language sql as $$ select uid $$;`);
  let sql = await readMigration('20261105033943_paid_box_sandbox_ledger.sql');
  // Auth/session and full save backup checks need Supabase; this fixture tests the purchase SQL itself.
  const start = sql.indexOf('-- Preserve the existing legacy backup contract');
  const seed = sql.indexOf('insert into pockle_private.paid_box_finishes', start);
  assert(start > 0 && seed > start, 'Ledger fixture extraction markers changed');
  await pg.exec(sql.slice(0, start) + sql.slice(seed));
  await pg.exec(await readMigration('20261105143457_paid_box_contents_quote.sql'));
  const a = '11111111-1111-4111-8111-111111111111', b = '22222222-2222-4222-8222-222222222222';
  await pg.query('insert into pockle_private.players(id) values($1),($2)', [a,b]);
  const call = async (action, payload = {}, uid = a) => (await pg.query(
    'select public.pockle_paid_box_api($1::uuid,$1::uuid,$2,$3::jsonb) result',
    [uid,action,JSON.stringify(payload)])).rows[0].result;
  const quote = async product_id => (await pg.query(
    'select public.pockle_paid_box_quote($1::uuid,$1::uuid,$2,$3::jsonb) result',
    [a,'quote',JSON.stringify({product_id})])).rows[0].result;
  const rejected = async (fn, message) => {
    try { await fn(); } catch(e) { assert(e.message.includes(message), `Wrong rejection: ${e.message}`); return; }
    throw Error(`Expected rejection: ${message}`);
  };
  const matchesClient = (item, collectionId = item.collection_id) => {
    const f = clientById.get(item.collectible_id);
    return f && f.collection_id === collectionId && f.tier === item.tier;
  };

  assert(!(await call('status')).available, 'Default checkout policy must stay off');
  await rejected(() => quote('pockle.box.wally.v1'), 'checkout_unavailable');
  await pg.exec('update pockle_private.paid_box_policy set sandbox_enabled=true');
  const stale = await quote('pockle.box.fox.v1');
  assert(stale.odds[0].contents.length === 10 && stale.odds[0].contents.some(i => !matchesClient(i, 'fox')),
    'Regression fixture must reproduce the stale non-Wally quote rejected by the client');
  const staleWally = await quote('pockle.box.wally.v1');
  assert(staleWally.odds[0].contents.every(i => matchesClient(i, 'wally')), 'Original Wally quote should match');

  // A historical glow already bought under the old Secret tier is an immutable snapshot.
  // Its saved tier, receipt and reveal progress must survive the catalog update and retry.
  const oldPayload = {store:'play_store', environment:'SANDBOX', product_id:'pockle.box.fox.v1',
    transaction_id:'pre-roster-sync', purchased_at:'2026-11-04T00:00:00Z'};
  await pg.query(`insert into pockle_private.paid_box_deliveries
    (player_id,store,environment,transaction_id,product_id,purchased_at,items,revealed,finished)
    values($1,'play_store','SANDBOX','pre-roster-sync','pockle.box.fox.v1','2026-11-04T00:00:00Z',
      '[{"collectible_id":"fox.glow-jelly","collection_id":"fox","tier":3,"is_new":true}]',1,true)`, [a]);
  const before = (await call('deliveries')).deliveries;
  const sync = await readMigration('20261109052434_paid_box_roster_catalog_sync.sql');
  await pg.exec(sync);
  await pg.exec(sync); // Safe replay: no reset of checkout policy or purchase history.
  assert((await call('status')).available, 'Migration changed checkout policy');
  assert(JSON.stringify((await call('deliveries')).deliveries) === JSON.stringify(before), 'Migration rewrote purchase snapshot');
  const oldReplay = await call('deliver',oldPayload);
  assert(oldReplay.receipt_id === before[0].receipt_id, 'Old transaction rerolled after migration');
  assert(JSON.stringify((await call('deliveries')).deliveries) === JSON.stringify(before), 'Receipt replay changed old item or progress');

  const server = (await pg.query('select id,collection_id,tier from pockle_private.paid_box_finishes order by id')).rows;
  assert(JSON.stringify(server) === JSON.stringify(client.finishes), 'Server catalog must match actual ToyCatalog IDs, collections and tiers');
  assert(JSON.stringify((await pg.query('select id from pockle_private.paid_box_products order by id')).rows.map(p => p.id))
    === JSON.stringify(client.products), 'Product IDs diverged from client');
  // Unknown rows must abort the entire migration instead of deleting or hiding earlier data.
  await pg.exec("insert into pockle_private.paid_box_finishes values('retired.test','fox',0); update pockle_private.paid_box_finishes set tier=3 where id='fox.glow-jelly'");
  await rejected(() => pg.exec(sync), 'paid_box_catalog_sync_unexpected_roster');
  assert((await pg.query("select tier from pockle_private.paid_box_finishes where id='fox.glow-jelly'")).rows[0].tier === 3,
    'Unexpected catalog failure must roll back all updates');
  await pg.exec("delete from pockle_private.paid_box_finishes where id='retired.test'");
  await pg.exec(sync);

  for (const id of client.products) {
    const q = await quote(id), all = q.odds.flatMap(g => g.contents);
    assert(q.quote_id && all.length === (id.includes('.mix.') ? 165 : 15), `Incomplete contents: ${id}`);
    assert(q.odds.every(g => g.contents.every(i => matchesClient(i, g.collection_id))), `Client would reject quote: ${id}`);
    assert(new Set(all.map(i => i.collectible_id)).size === all.length, 'Duplicate disclosed finish');
    assert(Math.abs(all.reduce((sum,i) => sum+i.percent,0)-100) < 1e-7, 'Per-box probability total');
    for (const g of q.odds) {
      assert(JSON.stringify(g.rows.map(r => r.percent)) === '[88,8,3,1]', 'Wrong launch tier odds');
      assert(JSON.stringify([0,1,2,3].map(t => g.contents.filter(i => i.tier === t).length)) === '[9,3,2,1]', 'Wrong tier counts');
      assert(Math.abs(g.contents.reduce((sum,i) => sum+i.within_collection_percent,0)-100) < 1e-7, 'Conditional total');
      for (const r of g.rows) assert(Math.abs(g.contents.filter(i => i.tier === r.tier)
        .reduce((sum,i) => sum+i.within_collection_percent,0)-r.percent) < 1e-7, 'Contents tier probability mismatch');
      if (id.includes('.mix.')) assert(g.contents.every(i => Math.abs(i.percent-i.within_collection_percent*g.collection_percent/100) < 1e-7), 'Mix collection chance missing');
    }
  }
  assert((await call('deliveries')).deliveries.length === 1, 'Browsing granted a toy');
  const payload = {...oldPayload, product_id:'pockle.mix.5.v1', transaction_id:'verified-1'};
  const first = await call('deliver',payload), second = await call('deliver',payload);
  assert(first.receipt_id === second.receipt_id, 'Replay changed receipt');
  let delivery = (await call('deliveries')).deliveries.find(d => d.receipt_id === first.receipt_id);
  assert(delivery.items.length === 5 && delivery.items.every(i => matchesClient(i)), 'Pack must save exactly five valid current finishes');
  const original = JSON.stringify(delivery.items);
  await call('progress',{receipt_id:first.receipt_id,revealed:3,finished:false});
  await call('progress',{receipt_id:first.receipt_id,revealed:1,finished:false});
  delivery = (await call('deliveries')).deliveries.find(d => d.receipt_id === first.receipt_id);
  assert(delivery.revealed === 3 && JSON.stringify(delivery.items) === original, 'Progress changed outcome or moved backwards');
  await rejected(() => call('deliver',{...payload,environment:'PRODUCTION'}),'production_checkout_disabled');
  await rejected(() => call('deliver',payload,b),'receipt_owner_mismatch');
  await rejected(() => call('deliver',{...payload,product_id:'pockle.box.pip.v1'}),'receipt_owner_mismatch');
  assert((await call('deliveries',{},b)).deliveries.length === 0, 'Other account read delivery');
  const checks = (await pg.query(`select
    not has_function_privilege('authenticated','public.pockle_paid_box_api(uuid,uuid,text,jsonb)','execute') protected,
    not has_function_privilege('authenticated','public.pockle_paid_box_quote(uuid,uuid,text,jsonb)','execute') quote_protected,
    (select bool_and(relrowsecurity) from pg_class c join pg_namespace n on n.oid=c.relnamespace
      where n.nspname='pockle_private' and c.relkind='r' and relname like 'paid_box_%') rls`)).rows[0];
  assert(checks.protected && checks.quote_protected && checks.rls,'Client access or RLS failure');
  console.log('PASS: actual Core/server parity for all165finishes and13products; stale quote reproduced; all fresh quotes valid at88/8/3/1; historical snapshot/reveal and receipt replay preserved; unknown roster rolls back; five-child commit, ownership isolation, production rejection, monotonic reveal, RLS/private RPC. Actor fixture mocked; real sessions and store purchases not exercised.');
} finally { await pg.close(); }
