import {handlePaid} from './handler.ts';
const uid='11111111-1111-4111-8111-111111111111',sid='22222222-2222-4222-8222-222222222222';
const token='header.'+btoa(JSON.stringify({session_id:sid}))+'.signature';
const env={url:'https://db.test',anon:'public',service:'server',revenuecat:'sk_server_only'};
const assert=(value:unknown)=>{if(!value)throw Error('assertion failed');};
function req(action='recover',payload:unknown={}){return new Request(env.url,{method:'POST',headers:{Authorization:'Bearer '+token},body:JSON.stringify({action,payload,actor:'spoofed'})});}
const user={id:uid,is_anonymous:false,identities:[{provider:'google'}]};
function mock(transactions:unknown,log:{action:string,payload:Record<string,unknown>}[],original=uid):typeof fetch {
 return async(input,init)=>{
  if(String(input).endsWith('/auth/v1/user'))return Response.json(user);
  if(String(input).startsWith('https://api.revenuecat.com/')){assert(String(input).endsWith(uid));assert(new Headers(init?.headers).get('Authorization')==='Bearer sk_server_only');return Response.json({subscriber:{original_app_user_id:original,non_subscriptions:transactions}});}
  assert(String(input).endsWith('/rpc/pockle_paid_box_api'));
  assert(new Headers(init?.headers).get('apikey')==='server');
  const b=JSON.parse(String(init?.body));assert(b.actor===uid&&b.session_id===sid);log.push(b);
  return Response.json(b.action==='status'?{available:true,environment:'SANDBOX'}:b.action==='deliveries'?{deliveries:[{receipt_id:'saved'}]}:{receipt_id:'saved'});
 };
}
Deno.test('purchase endpoint rejects missing auth, anonymous and unsupported providers',async()=>{
 assert((await handlePaid(new Request(env.url,{method:'POST'}),env)).status===401);
 for(const invalid of [{id:uid,is_anonymous:true,identities:user.identities},{id:uid,identities:[{provider:'email'}]}])
  assert((await handlePaid(req(),env,async()=>Response.json(invalid))).status===401);
});
Deno.test('client claims cannot mint an award; verified RevenueCat transaction owns all grant fields',async()=>{
 const log:{action:string,payload:Record<string,unknown>}[]=[];
 const response=await handlePaid(req('recover',{transaction_id:'forged',product_id:'pockle.mix.5.v1',items:['forged']}),env,mock({'pockle.box.pip.v1':[{id:'rc_verified',store:'play_store',is_sandbox:true,purchase_date:'2026-11-04T12:00:00Z'}]},log));
 assert(response.status===200);const grants=log.filter(c=>c.action==='deliver');assert(grants.length===1);
 assert(grants[0].payload.product_id==='pockle.box.pip.v1'&&grants[0].payload.transaction_id==='rc_verified');
 assert(grants[0].payload.environment==='SANDBOX'&&!('items' in grants[0].payload));
});
Deno.test('production, promotional, refunds, unapproved products and multiple quantities never grant',async()=>{
 const log:{action:string,payload:Record<string,unknown>}[]=[];
 const valid={id:'rc',store:'app_store',is_sandbox:true,purchase_date:'2026-11-04T12:00:00Z'};
 await handlePaid(req(),env,mock({'pockle.box.pip.v1':[{...valid,is_sandbox:false},{...valid,store:'promotional'},{...valid,is_refunded:true},{...valid,refunded_at:'2026-11-04'},{...valid,quantity:2}],other:[valid]},log));
 assert(!log.some(c=>c.action==='deliver'));
});
Deno.test('aliased ownership, unavailable credentials and failed verification stop before grants',async()=>{
 const log:{action:string,payload:Record<string,unknown>}[]=[];
 assert((await handlePaid(req(),env,mock({},log,'other'))).status===409);
 assert((await handlePaid(req(),{...env,revenuecat:''},mock({},log))).status===503);
 const failed:typeof fetch=async input=>String(input).endsWith('/auth/v1/user')?Response.json(user):String(input).startsWith('https://api.revenuecat.com/')?new Response('',{status:500}):Response.json({available:true});
 assert((await handlePaid(req(),env,failed)).status===503);assert(!log.some(c=>c.action==='deliver'));
});
Deno.test('unknown actions and invalid product IDs never reach grant RPC',async()=>{
 const log:{action:string,payload:Record<string,unknown>}[]=[];
 assert((await handlePaid(req('deliver',{items:['forged']}),env,mock({},log))).status===400);
 assert((await handlePaid(req('quote',{product_id:'pockle.retired.box'}),env,mock({},log))).status===400);
 assert(!log.some(c=>c.action==='deliver'));
});

Deno.test('quote contents come only from the authenticated server quote RPC',async()=>{
 let quoted=false;
 const response=await handlePaid(req('quote',{product_id:'pockle.box.fox.v1',contents:[{collectible_id:'forged',percent:100}]}),env,async(input,init)=>{
  if(String(input).endsWith('/auth/v1/user'))return Response.json(user);
  const b=JSON.parse(String(init?.body));assert(b.actor===uid&&b.session_id===sid);
  if(b.action==='status')return Response.json({available:true});
  assert(String(input).endsWith('/rpc/pockle_paid_box_quote'));
  assert(JSON.stringify(b.payload)===JSON.stringify({product_id:'pockle.box.fox.v1'}));quoted=true;
  return Response.json({quote_id:'server',odds:[{contents:[{collectible_id:'fox.mochi-foam',percent:11}]}]});
 });
 assert(response.status===200&&quoted);const result=await response.json();assert(result.odds[0].contents[0].percent===11);
});

Deno.test('Mix quotes omit only the absent top-level collection for installed Unity clients',async()=>{
 for(const [product,count] of [['pockle.mix.3.v1',3],['pockle.mix.5.v1',5]] as const) {
  const saved={quote_id:'fixture',product_id:product,box_count:count,collection_id:null,
   odds:[{collection_id:'fox',collection_percent:100/11,rows:[{tier:0,percent:88}],contents:[{collectible_id:'fox.clear-jelly',tier:0,percent:88/9/11,within_collection_percent:88/9}]}],
   notes:['server notes']};
  const calls:string[]=[];
  const response=await handlePaid(req('quote',{product_id:product}),env,async(input,init)=>{
   if(String(input).endsWith('/auth/v1/user'))return Response.json(user);
   const b=JSON.parse(String(init?.body));calls.push(b.action);
   assert(b.actor===uid&&b.session_id===sid);
   if(b.action==='status')return Response.json({available:true});
   assert(String(input).endsWith('/rpc/pockle_paid_box_quote'));
   assert(b.action==='quote'&&b.payload.product_id===product);return Response.json(saved);
  });
  assert(response.status===200);const wire=await response.json();
  assert(!Object.hasOwn(wire,'collection_id'));
  const {collection_id:_absent,...expected}=saved;
  assert(JSON.stringify(wire)===JSON.stringify(expected));
  // Compare fields explicitly, independent of JSON object-key order.
  assert(wire.quote_id===saved.quote_id&&wire.product_id===product&&wire.box_count===count);
  assert(JSON.stringify(wire.odds)===JSON.stringify(saved.odds)&&JSON.stringify(wire.notes)===JSON.stringify(saved.notes));
  assert(JSON.stringify(calls)==='["status","quote"]');
 }
});

Deno.test('compatibility omission never hides a wrong Mix identity or a real collection identity',async()=>{
 for(const [requested,returned,collection] of [
  ['pockle.box.fox.v1','pockle.box.fox.v1','fox'],
  ['pockle.mix.3.v1','pockle.mix.3.v1','fox'],
  ['pockle.mix.3.v1','pockle.mix.5.v1',null],
 ]) {
  const saved={quote_id:'fixture',product_id:returned,collection_id:collection,box_count:3};
  const response=await handlePaid(req('quote',{product_id:requested}),env,async(input,init)=>{
   if(String(input).endsWith('/auth/v1/user'))return Response.json(user);
   return Response.json(JSON.parse(String(init?.body)).action==='status'?{available:true}:saved);
  });
  assert(response.status===200);assert(JSON.stringify(await response.json())===JSON.stringify(saved));
 }
});
