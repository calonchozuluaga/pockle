#!/usr/bin/env python3
"""Read-only/negative smoke checks. Uses only the checked-in publishable configuration; creates no accounts/data."""
import json,urllib.request,urllib.error,pathlib
config=json.loads((pathlib.Path(__file__).resolve().parents[1]/'Assets/Pockle/Resources/Backend/PockleBackend.json').read_text())
def request(path,payload=None,token=None):
 headers={'apikey':config['publishableKey'],'Content-Type':'application/json'}
 if token:headers['Authorization']='Bearer '+token
 req=urllib.request.Request(config['url']+path,data=json.dumps(payload).encode() if payload is not None else None,headers=headers)
 try:
  with urllib.request.urlopen(req,timeout=20) as result:return result.status,json.loads(result.read())
 except urllib.error.HTTPError as e:
  try:body=json.loads(e.read())
  except Exception:body={}
  return e.code,body
status,body=request('/functions/v1/pockle-api',{'action':'profile'});assert status==401,(status,body)
status,body=request('/functions/v1/pockle-api',{'action':'profile'},'invalid-session');assert status==401,(status,body)
status,body=request('/rest/v1/rpc/pockle_account_api',{'actor':'11111111-1111-4111-8111-111111111111','session_id':'22222222-2222-4222-8222-222222222222','action':'profile'});assert status in (401,403,404),(status,body)
status,body=request('/auth/v1/settings');assert status==200
providers=body.get('external',{});assert providers.get('anonymous_users',False) is False;assert providers.get('email',False) is False;assert providers.get('phone',False) is False
print('PASS: deployed Edge denies missing/invalid sessions; direct server RPC unavailable to client; anonymous/email/phone disabled.')
print('Provider switches:',{k:providers.get(k,False) for k in ('apple','google')})
