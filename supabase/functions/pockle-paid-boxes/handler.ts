import { readBody, sessionId } from '../pockle-api/handler.ts';
type Env = { url: string; anon: string; service: string; revenuecat: string };
const headers = {'Content-Type':'application/json','Cache-Control':'no-store'};
const reply = (body: unknown,status=200) => new Response(JSON.stringify(body),{status,headers});
const collections = ['pip','fox','axolotl','firb','coco','snowy','ginger','reiny','guin','bundy','wally'];
const products = new Set([...collections.map(c=>`pockle.box.${c}.v1`),'pockle.mix.3.v1','pockle.mix.5.v1']);
export async function handlePaid(req: Request,env: Env,request: typeof fetch=fetch): Promise<Response> {
 if(req.method!=='POST')return reply({error:'method_not_allowed'},405);
 const auth=req.headers.get('Authorization')??'';
 if(!/^Bearer [^\s]+$/.test(auth))return reply({error:'unauthenticated'},401);
 try {
  const userResponse=await request(env.url+'/auth/v1/user',{headers:{apikey:env.anon,Authorization:auth},signal:AbortSignal.timeout(15000)});
  if(!userResponse.ok)return reply({error:'unauthenticated'},401);
  const user=await userResponse.json();
  if(typeof user.id!=='string'||user.is_anonymous||!Array.isArray(user.identities)||!user.identities.some((i:{provider:string})=>['apple','google'].includes(i.provider)))return reply({error:'unauthenticated'},401);
  const sid=sessionId(auth.slice(7));const body=await readBody(req);
  const action=body.action;
  if(!['status','quote','recover','progress'].includes(action as string))return reply({error:'invalid_action'},400);
  const payload=body.payload as Record<string,unknown>??{};
  async function rpc(action: string,payload: unknown) {
   const endpoint=action==='quote'?'pockle_paid_box_quote':'pockle_paid_box_api';
   const result=await request(env.url+'/rest/v1/rpc/'+endpoint,{method:'POST',headers:{apikey:env.service,Authorization:'Bearer '+env.service,'Content-Type':'application/json'},
    body:JSON.stringify({actor:user.id,session_id:sid,action,payload}),signal:AbortSignal.timeout(20000)});
   if(!result.ok)throw Error('delivery_unavailable');
   return await result.json();
  }
  const status=await rpc('status',{});
  if(action==='status')return reply({...status,available:status.available===true&&env.revenuecat.startsWith('sk_')});
  if(!env.revenuecat.startsWith('sk_')||status.available!==true)return reply({error:'checkout_unavailable'},503);
  if(action==='quote') {
   if(!products.has(payload.product_id as string))return reply({error:'invalid_product'},400);
   return reply(await rpc('quote',{product_id:payload.product_id}));
  }
  if(action==='progress')return reply(await rpc('progress',{receipt_id:payload.receipt_id,revealed:payload.revealed,finished:payload.finished===true}));
  // Never accept a client's success flag, transaction, store, environment or awarded toy.
  const customer=await request('https://api.revenuecat.com/v1/subscribers/'+encodeURIComponent(user.id),{
   headers:{Authorization:'Bearer '+env.revenuecat},signal:AbortSignal.timeout(20000)});
  if(!customer.ok)return reply({error:'purchase_verification_unavailable'},503);
  const subscriber=(await customer.json()).subscriber;
  if(!subscriber||subscriber.original_app_user_id!==user.id)return reply({error:'purchase_identity_mismatch'},409);
  const purchases=subscriber.non_subscriptions;
  if(!purchases||typeof purchases!=='object'||Array.isArray(purchases))return reply({error:'invalid_purchase_response'},503);
  let pending=0;
  for(const [product,transactions] of Object.entries(purchases)) {
   if(!products.has(product)||!Array.isArray(transactions))continue;
   for(const t of transactions) {
    if(t.is_sandbox!==true||!['app_store','play_store'].includes(t.store)||t.refunded_at||t.is_refunded===true||(t.quantity!=null&&t.quantity!==1)){pending++;continue;}
    if(typeof t.id!=='string'||t.id.length<1||t.id.length>200||typeof t.purchase_date!=='string'||!Number.isFinite(Date.parse(t.purchase_date)))return reply({error:'invalid_transaction'},503);
    await rpc('deliver',{product_id:product,transaction_id:t.id,store:t.store,environment:'SANDBOX',purchased_at:t.purchase_date});
   }
  }
  return reply({...await rpc('deliveries',{}),pending});
 } catch { return reply({error:'checkout_request_failed'},503); }
}
