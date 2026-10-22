#nullable enable
using System;
using System.Security.Cryptography;
using System.Text;
namespace Pockle.Core
{
    /// <summary>Local PKCE and receipt primitives. No provider/service secrets belong in the client.</summary>
    public static class AccountProtocol
    {
        public const string Redirect = "com.playpockle.app://auth/callback";
        public static string RandomToken() { var bytes=new byte[32];using(var rng=RandomNumberGenerator.Create())rng.GetBytes(bytes);return Base64Url(bytes); }
        public static string Challenge(string verifier) { using(var hash=SHA256.Create())return Base64Url(hash.ComputeHash(Encoding.UTF8.GetBytes(verifier))); }
        public static string Digest(string text) { using(var hash=SHA256.Create())return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(text))).Replace("-","").ToLowerInvariant(); }
        public static string Base64Url(byte[] bytes)=>Convert.ToBase64String(bytes).TrimEnd('=').Replace('+','-').Replace('/','_');
        public static string? CallbackCode(string callback,string request)
        {
            if(string.IsNullOrEmpty(request)||!Uri.TryCreate(callback,UriKind.Absolute,out var uri)||uri.Scheme!="com.playpockle.app"||uri.Host!="auth"||uri.AbsolutePath!="/callback"||uri.Fragment.Length>0)return null;
            string? code=null,state=null;int codes=0,states=0;
            foreach(string item in uri.Query.TrimStart('?').Split('&')) { var pair=item.Split(new[]{'='},2);if(pair.Length!=2)continue;
                string value;try{value=Uri.UnescapeDataString(pair[1]);}catch{return null;}
                if(pair[0]=="code"){code=value;codes++;}if(pair[0]=="request"){state=value;states++;} }
            return codes==1&&states==1&&state==request&&!string.IsNullOrEmpty(code)?code:null;
        }
        public static bool Publishable(string value)=>value!=null&&value.StartsWith("sb_publishable_",StringComparison.Ordinal)&&value.Length>20&&value.IndexOfAny(new[]{' ','\n','\r','\t'})<0;
    }
}
