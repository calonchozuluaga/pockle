using System;
using Pockle.Core;
internal static class AccountProtocolChecks
{
    private static void Check(bool value,string message){if(!value)throw new Exception(message);}
    public static void Run()
    {
        Check(AccountProtocol.Challenge("dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk")=="E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM","RFC7636 S256 vector");
        Check(AccountProtocol.Digest("abc")=="ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad","SHA256 receipt vector");
        string a=AccountProtocol.RandomToken(),b=AccountProtocol.RandomToken();Check(a.Length==43&&a!=b,"Fresh 256-bit PKCE/request values");
        string callback=AccountProtocol.Redirect+"?request="+a+"&code=accepted";
        Check(AccountProtocol.CallbackCode(callback,a)=="accepted","Only the matching request accepts a code");
        foreach(string bad in new[]{callback.Replace("com.playpockle.app","https"),callback.Replace("auth/","attacker/"),callback.Replace("callback?","callback/extra?"),callback+"&code=other",callback+"&request="+a,callback+"#access_token=bad",callback.Replace(a,b)})
            Check(AccountProtocol.CallbackCode(bad,a)==null,"Reject callback replay/route/query/fragment confusion");
        Check(!AccountProtocol.Publishable("sb_secret_do_not_embed")&&!AccountProtocol.Publishable("legacy.jwt.token")&&AccountProtocol.Publishable("sb_publishable_public_configuration"),"Only publishable keys may enter client config");
        Console.WriteLine("PASS: account PKCE, receipt SHA256, callback binding and publishable-key boundaries.");
    }
}
