import { handlePaid } from './handler.ts';
Deno.serve(req=>handlePaid(req,{
 url:Deno.env.get('SUPABASE_URL')??'',anon:Deno.env.get('SUPABASE_ANON_KEY')??'',
 service:Deno.env.get('SUPABASE_SERVICE_ROLE_KEY')??'',revenuecat:Deno.env.get('POCKLE_REVENUECAT_SECRET_KEY')??''
}));
