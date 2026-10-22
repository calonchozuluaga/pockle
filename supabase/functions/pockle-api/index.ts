import { handle } from './handler.ts';
const url=Deno.env.get('SUPABASE_URL'),anon=Deno.env.get('SUPABASE_ANON_KEY'),service=Deno.env.get('SUPABASE_SERVICE_ROLE_KEY');
if (!url||!anon||!service) throw Error('Missing server environment');
Deno.serve(req=>handle(req,{url,anon,service}));
