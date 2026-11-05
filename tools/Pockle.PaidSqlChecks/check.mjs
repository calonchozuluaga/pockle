import {PGlite} from '@electric-sql/pglite';
import fs from 'node:fs/promises';
const pg=new PGlite();
await pg.exec(`create schema pockle_private;create role anon;create role authenticated;create role service_role;
create table pockle_private.players(id uuid primary key,status text default 'active');
create table pockle_private.legacy_snapshots(player_id uuid,save_text text);
create function pockle_private.actor(uid uuid,sid uuid) returns uuid language sql as $$ select uid $$;`);
let sql=await fs.readFile(new URL('../../supabase/migrations/20261105033943_paid_box_sandbox_ledger.sql',import.meta.url),'utf8');
// The existing account backup function is tested separately against Supabase; this local fixture exercises the new paid ledger.
const start=sql.indexOf('-- Preserve the existing legacy backup contract');const seed=sql.indexOf('insert into pockle_private.paid_box_finishes',start);
sql=sql.slice(0,start)+sql.slice(seed);
await pg.exec(sql);
await pg.exec(await fs.readFile(new URL('../../supabase/migrations/20261105141903_paid_box_contents_quote.sql',import.meta.url),'utf8'));
await pg.exec(`insert into pockle_private.players(id) values('11111111-1111-4111-8111-111111111111'),('22222222-2222-4222-8222-222222222222');`);
const a='11111111-1111-4111-8111-111111111111',b='22222222-2222-4222-8222-222222222222';
async function call(action,payload={},uid=a){return(await pg.query('select public.pockle_paid_box_api($1::uuid,$1::uuid,$2,$3::jsonb) as result',[uid,action,JSON.stringify(payload)])).rows[0].result;}
const assert=(ok,msg)=>{if(!ok)throw Error(msg);};
assert(!(await call('status')).available,'Default policy must be off');
try{await call('quote',{product_id:'pockle.box.wally.v1'});throw Error('Disabled quote succeeded');}catch(e){assert(e.message.includes('checkout_unavailable'),'Unexpected quote failure '+e.message);}
await pg.exec('update pockle_private.paid_box_policy set sandbox_enabled=true');
for(const id of ['pockle.box.wally.v1','pockle.box.pip.v1','pockle.mix.5.v1']){
 const quote=await call('quote',{product_id:id});assert(quote.quote_id&&quote.odds.length>0,'Missing odds');
 for(const g of quote.odds)assert(Math.abs(g.rows.reduce((s,r)=>s+r.percent,0)-100)<1e-7,'Tier odds do not sum to100');
 if(id.includes('mix'))assert(Math.abs(quote.odds.reduce((s,g)=>s+g.collection_percent,0)-100)<1e-7,'Collection odds do not sum to100');
 if(id.includes('wally'))assert(JSON.stringify(quote.odds[0].rows.map(r=>r.percent))==='[88,8,3,1]','Wrong Wally odds');
}
// Every disclosed finish is an actual eligible server finish, with per-box odds including collection choice.
for(const id of ['pockle.box.fox.v1','pockle.box.wally.v1','pockle.mix.3.v1','pockle.mix.5.v1']){
 const q=(await pg.query('select public.pockle_paid_box_quote($1::uuid,$1::uuid,$2,$3::jsonb) result',[a,'quote',JSON.stringify({product_id:id})])).rows[0].result;
 const all=q.odds.flatMap(g=>g.contents);assert(all.length===(id.includes('mix')?115:id.includes('wally')?15:10),'Wrong contents count');
 assert(new Set(all.map(x=>x.collectible_id)).size===all.length,'Duplicate disclosed finish');
 assert(Math.abs(all.reduce((sum,x)=>sum+x.percent,0)-100)<1e-7,'Per-box contents odds must sum to100');
 for(const g of q.odds){
  assert(Math.abs(g.contents.reduce((sum,x)=>sum+x.within_collection_percent,0)-100)<1e-7,'Conditional contents odds must sum to100');
  for(const row of g.rows)assert(Math.abs(g.contents.filter(x=>x.tier===row.tier).reduce((sum,x)=>sum+x.within_collection_percent,0)-row.percent)<1e-7,'Contents tier mismatch');
  if(id.includes('mix'))assert(g.contents.every(x=>Math.abs(x.percent-x.within_collection_percent*g.collection_percent/100)<1e-7),'Mix must include collection chance');
 }
}
assert(!(await pg.query("select has_function_privilege('authenticated','public.pockle_paid_box_quote(uuid,uuid,text,jsonb)','execute') allowed")).rows[0].allowed,'Quote RPC exposed');
const payload={store:'play_store',environment:'SANDBOX',product_id:'pockle.mix.5.v1',transaction_id:'verified-1',purchased_at:'2026-11-04T00:00:00Z'};
const first=await call('deliver',payload),second=await call('deliver',payload);assert(first.receipt_id===second.receipt_id,'Replay changed receipt');
let deliveries=(await call('deliveries')).deliveries;assert(deliveries.length===1&&deliveries[0].items.length===5,'Pack did not save exactly five');const original=JSON.stringify(deliveries[0].items);
await call('progress',{receipt_id:first.receipt_id,revealed:3,finished:false});await call('progress',{receipt_id:first.receipt_id,revealed:1,finished:false});
deliveries=(await call('deliveries')).deliveries;assert(deliveries[0].revealed===3&&JSON.stringify(deliveries[0].items)===original,'Progress changed outcome or moved backwards');
for(const [casePayload,uid,message] of [[{...payload,environment:'PRODUCTION'},a,'production_checkout_disabled'],[payload,b,'receipt_owner_mismatch'],[{...payload,product_id:'pockle.box.pip.v1'},a,'receipt_owner_mismatch']]){
 try{await call('deliver',casePayload,uid);throw Error('Unsafe delivery accepted');}catch(e){assert(e.message.includes(message),'Unexpected reject '+e.message);}
}
assert((await call('deliveries',{},b)).deliveries.length===0,'Other account could read delivery');
const checks=await pg.query(`select not has_function_privilege('authenticated','public.pockle_paid_box_api(uuid,uuid,text,jsonb)','execute') as protected, (select bool_and(relrowsecurity) from pg_class c join pg_namespace n on n.oid=c.relnamespace where n.nspname='pockle_private' and c.relkind='r' and relname like 'paid_box_%') as rls`);
assert(checks.rows[0].protected&&checks.rows[0].rls,'Client access or RLS failure');
console.log('PASS: local Postgres migration, disabled policy, exact quote odds, five-child commit, replay, ownership isolation, no production grants, monotonic reveal, RLS/private RPC. Actor fixture mocked; live session validation not exercised here.');
await pg.close();
