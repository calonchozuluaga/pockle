export type Env = { url: string; anon: string; service: string };
type Json = Record<string, unknown>;
const headers = { 'Content-Type': 'application/json', 'Cache-Control': 'no-store' };
const actions = new Set(['profile','username','legacy_upload','legacy_latest','delete_request','sign_out','boxes','walking_report','box_open','box_finish']);
const uuid = /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;
function response(body: unknown, status = 200) { return new Response(JSON.stringify(body), { status, headers }); }
export async function readBody(req: Request): Promise<Json> {
  const reader = req.body?.getReader(); if (!reader) throw Error('invalid_body');
  let length = 0; const chunks: Uint8Array[] = [];
  while (true) { const { value, done } = await reader.read(); if (done) break;
    length += value.length; if (length > 2200000) { await reader.cancel(); throw Error('body_too_large'); } chunks.push(value); }
  const bytes = new Uint8Array(length); let at=0; for (const c of chunks) { bytes.set(c,at);at+=c.length; }
  const body=JSON.parse(new TextDecoder().decode(bytes));
  if (!body || typeof body !== 'object' || Array.isArray(body)) throw Error('invalid_body'); return body;
}
export function sessionId(token: string): string {
  const chunk=token.split('.')[1]; if (!chunk) throw Error('invalid_session');
  const value=JSON.parse(atob(chunk.replace(/-/g,'+').replace(/_/g,'/'))).session_id;
  if (typeof value !== 'string' || !uuid.test(value)) throw Error('invalid_session');return value;
}
export async function handle(req: Request, env: Env, request: typeof fetch = fetch): Promise<Response> {
  if (req.method !== 'POST') return response({ error:'method_not_allowed' },405);
  const auth=req.headers.get('Authorization') ?? ''; if (!/^Bearer [^\s]+$/.test(auth)) return response({error:'unauthenticated'},401);
  try {
    const token=auth.slice(7);
    // Auth verifies the signed JWT; decoded claims alone are NEVER authorization.
    const verified=await request(env.url+'/auth/v1/user',{headers:{apikey:env.anon,Authorization:auth}});
    if (!verified.ok) return response({error:'unauthenticated'},401);
    const user=await verified.json();
    if (!uuid.test(user.id) || user.is_anonymous || !user.identities?.some((i: Json)=>i.provider==='apple'||i.provider==='google'))
      return response({error:'provider_not_allowed'},403);
    const sid=sessionId(token), body=await readBody(req), action=body.action;
    if (typeof action!=='string' || !actions.has(action)) return response({error:'unknown_action'},400);
    // The current Auth session is checked again in SQL, and actor is always taken from the verified user.
    const rpc=async(name: string, action: string, payload: unknown) => await request(env.url+'/rest/v1/rpc/'+name,{
      method:'POST',headers:{apikey:env.service,Authorization:'Bearer '+env.service,'Content-Type':'application/json'},
      body:JSON.stringify({actor:user.id,session_id:sid,action,payload})});
    const isBox=['boxes','walking_report','box_open','box_finish'].includes(action);
    const result=await rpc(isBox?'pockle_box_api':'pockle_account_api',action==='sign_out'?'profile':action,body.payload??{});
    if (!result.ok) {
      const failure=await result.json().catch(()=>({}));
      const known=['invalid_session','account_disabled','invalid_snapshot','snapshot_limit','username_required','walking_device_change_requires_review','invalid_counter','stale_counter','counter_rate_rejected','box_not_owned','box_not_opened','box_policy_unconfigured'];
      const error=known.find(e=>failure.message===e)??'request_failed';
      return response({error},['invalid_session','account_disabled'].includes(error)?401:400);
    }
    const value=await result.json();
    if (action==='delete_request'||action==='sign_out') {
      // On deletion SQL already disables all sensitive operations, even before Auth revocation finishes.
      const logout=await request(env.url+'/auth/v1/logout?scope='+(action==='delete_request'?'global':'local'),{method:'POST',headers:{apikey:env.anon,Authorization:auth}});
      if(action==='sign_out'&&!logout.ok)return response({error:'sign_out_failed'},503);
    }
    return response(value,value.status==='rate_limited'?429:200);
  } catch (e) { const code=e instanceof Error?e.message:'';return response({error:code==='body_too_large'?code:'invalid_request'},code==='body_too_large'?413:400); }
}
