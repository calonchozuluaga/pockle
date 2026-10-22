import {handle} from './handler.ts';
function assert(value:unknown){if(!value)throw Error('assertion failed');}
const uid='11111111-1111-4111-8111-111111111111',sid='22222222-2222-4222-8222-222222222222';
const token='header.'+btoa(JSON.stringify({session_id:sid}))+'.signature';
const env={url:'https://example.test',anon:'public',service:'server-only'};
function req(action='profile',extra:unknown={}){return new Request(env.url,{method:'POST',headers:{Authorization:'Bearer '+token},body:JSON.stringify({action,payload:extra,actor:'spoofed'})});}
Deno.test('Reject missing/invalid authentication and non-Apple/Google identities',async()=>{
 assert((await handle(new Request(env.url,{method:'POST'}),env)).status===401);
 assert((await handle(req(),env,()=>Promise.resolve(new Response('{}',{status:401})))).status===401);
 for(const user of [{id:uid,is_anonymous:true,identities:[{provider:'google'}]},{id:uid,identities:[{provider:'email'}]}])
  assert((await handle(req(),env,()=>Promise.resolve(Response.json(user)))).status===403);
});
Deno.test('Use verified actor and session; clients cannot choose server RPC or identity',async()=>{
 let calls=0;
 const mock:typeof fetch=async(input,init)=>{calls++;if(calls===1)return Response.json({id:uid,identities:[{provider:'google'}]});
  assert(String(input).endsWith('/rpc/pockle_account_api'));const b=JSON.parse(String(init?.body));assert(b.actor===uid&&b.session_id===sid);assert(new Headers(init?.headers).get('apikey')==='server-only');return Response.json({username:'Mochi'});};
 const r=await handle(req(),env,mock);assert(r.status===200&&calls===2);
});
Deno.test('Reject revoked sessions and unknown actions; delete requests revoke all sessions',async()=>{
 let calls=0,scope='';const mock:typeof fetch=async(input)=>{calls++;if(calls===1)return Response.json({id:uid,identities:[{provider:'apple'}]});if(calls===2)return Response.json({accepted:true});scope=String(input);return new Response(null,{status:204});};
 assert((await handle(req('delete_request'),env,mock)).status===200);assert(scope.endsWith('scope=global'));
 calls=0;assert((await handle(req('invented'),env,mock)).status===400&&calls===1);
 calls=0;const revoked:typeof fetch=async()=>++calls===1?Response.json({id:uid,identities:[{provider:'google'}]}):Response.json({message:'invalid_session'},{status:400});
 assert((await handle(req(),env,revoked)).status===401);
});
