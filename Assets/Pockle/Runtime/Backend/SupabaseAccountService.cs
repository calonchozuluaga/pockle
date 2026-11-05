using System;
using System.Collections;
using System.Text;
using Pockle.Core;
using UnityEngine;
using UnityEngine.Networking;
namespace Pockle.Runtime
{
    /// <summary>Apple/Google PKCE via the system browser. The cloud stores unverified legacy backups, never client-minted serials.</summary>
    public sealed class SupabaseAccountService : MonoBehaviour,IAccountService
    {
        [Serializable] private sealed class Config { public string url="",publishableKey=""; }
        [Serializable] private sealed class User { public string id=""; }
        [Serializable] private sealed class Session { public string access_token="",refresh_token="";public long expires_at;public int expires_in;public User user; }
        [Serializable] private sealed class Stored { public Session session;public string verifier="",request="";public long started; }
        [Serializable] private sealed class Profile { public string player_id="",username=""; }
        [Serializable] private sealed class Receipt { public string digest="",receipt_id="";public int inventory_rows,fusion_rows; }
        [Serializable] private sealed class NameReply { public string status="",username=""; }
        [Serializable] private sealed class TokenRequest { public string auth_code="",code_verifier="",refresh_token=""; }
        [Serializable] private sealed class NamePayload { public string username=""; }
        [Serializable] private sealed class SnapshotPayload { public string source_id="",save_json=""; }
        private Config config;private Stored stored=new Stored();private Profile profile;private CollectionSession collection;
        private bool busy,exchanging,ready;private Action<AccountResult> signInDone;
        private string lastDigest="";private float backupAt=-1;
        public string LastError { get;private set; }
        public bool IsSignedIn=>ready&&stored.session!=null;
        public string UserId => IsSignedIn ? stored.session.user.id : null;
        // Checkout uses the same verified account/session; tokens never leave this service.
        public IEnumerator AuthorizedRequest(string function, string body, Action<string,bool> done)
        {
            if (function != "pockle-paid-boxes" || !IsSignedIn) { done(null,false); yield break; }
            yield return RefreshIfNeeded();
            if (!IsSignedIn) { done(null,false); yield break; }
            yield return Send("/functions/v1/" + function, body, true, done);
        }
        public string Username=>IsSignedIn?profile?.username:null;
        private static long Now=>DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public void Initialize(CollectionSession session,Action initialized=null)
        {
            collection=session;collection.Changed+=CollectionChanged;
            var asset=Resources.Load<TextAsset>("Backend/PockleBackend");config=asset!=null?Parse<Config>(asset.text):null;
            if(config==null||config.url!="https://azdzafotawbrplukeqcl.supabase.co"||!AccountProtocol.Publishable(config.publishableKey)) { LastError="Account configuration is unavailable.";initialized?.Invoke();return; }
            try{stored=Parse<Stored>(AccountTokenStore.Read())??new Stored();}catch{stored=new Stored();LastError="The saved session could not be opened. Sign in again.";}
            Application.deepLinkActivated+=OnDeepLink;
            if(!string.IsNullOrEmpty(Application.absoluteURL))OnDeepLink(Application.absoluteURL);
            if(stored.session!=null&&OnboardingState.AgeConfirmed&&!OnboardingState.AgeLocked)StartCoroutine(Restore(initialized));
            else initialized?.Invoke();
        }
        public void SignIn(AccountProvider provider,Action<AccountResult> done)
        {
            if(busy){done(new AccountResult(AccountStatus.Failed,"A sign-in is already in progress."));return;}
            if(config==null||!AccountProtocol.Publishable(config.publishableKey)||!OnboardingState.AgeConfirmed||OnboardingState.AgeLocked)
            {done(new AccountResult(AccountStatus.Failed,"Complete the age check before signing in."));return;}
            if(provider!=AccountProvider.Apple&&provider!=AccountProvider.Google){done(new AccountResult(AccountStatus.Failed,"Choose Apple or Google."));return;}
            busy=true;ready=false;signInDone=done;stored=new Stored{verifier=AccountProtocol.RandomToken(),request=AccountProtocol.RandomToken(),started=Now};
            if(!SaveSecure()){FinishSignIn(false,"Secure session storage is unavailable.");return;}
            string redirect=AccountProtocol.Redirect+"?request="+stored.request;
            string url=config.url+"/auth/v1/authorize?provider="+(provider==AccountProvider.Apple?"apple":"google")+"&redirect_to="+Uri.EscapeDataString(redirect)
                +"&code_challenge="+AccountProtocol.Challenge(stored.verifier)+"&code_challenge_method=s256";
            Application.OpenURL(url);
        }
        private void OnDeepLink(string url)
        {
            if(exchanging||string.IsNullOrEmpty(stored.verifier)||Now-stored.started>300)return;
            string code=AccountProtocol.CallbackCode(url,stored.request);if(code==null)return;
            exchanging=true;busy=true;StartCoroutine(Exchange(code));
        }
        private IEnumerator Exchange(string code)
        {
            string reply=null;bool ok=false;
            yield return Send("/auth/v1/token?grant_type=pkce",JsonUtility.ToJson(new TokenRequest{auth_code=code,code_verifier=stored.verifier}),false,(r,s)=>{reply=r;ok=s;});
            stored.verifier="";stored.request="";exchanging=false;
            if(!ok||!AcceptToken(reply)){FinishSignIn(false,"Sign-in could not be completed. Please try again.");yield break;}
            yield return LoadProfileAndBackup();FinishSignIn(ready||(profile!=null&&string.IsNullOrEmpty(profile.username)),LastError);
        }
        private IEnumerator Restore(Action initialized)
        {
            busy=true;yield return LoadProfileAndBackup();busy=false;initialized?.Invoke();
        }
        private IEnumerator LoadProfileAndBackup()
        {
            ready=false;LastError=null;profile=null;
            yield return RefreshIfNeeded();if(stored.session==null)yield break;
            string reply=null;bool ok=false;yield return Api("profile","{}",(r,s)=>{reply=r;ok=s;});
            if(!ok||(profile=Parse<Profile>(reply))==null){LastError="Could not verify your account. Your phone save is unchanged.";yield break;}
            if(!string.IsNullOrEmpty(profile.username))yield return Backup();
        }
        public void ClaimUsername(string username,Action<UsernameCheck> done)
        {
            if(busy||stored.session==null){done(UsernameCheck.Failed);return;}
            if(OnboardingRules.UsernameProblem(username)!=null){done(UsernameCheck.NotAllowed);return;}
            busy=true;StartCoroutine(ClaimName(username,done));
        }
        private IEnumerator ClaimName(string name,Action<UsernameCheck> done)
        {
            yield return RefreshIfNeeded();string reply=null;bool ok=false;
            if(stored.session!=null)yield return Api("username",JsonUtility.ToJson(new NamePayload{username=name}),(r,s)=>{reply=r;ok=s;});
            var result=ok?Parse<NameReply>(reply):null;var check=result?.status=="taken"?UsernameCheck.Taken:result?.status=="not_allowed"?UsernameCheck.NotAllowed:UsernameCheck.Failed;
            if(result?.status=="available") {profile=new Profile{username=result.username};yield return Backup();if(ready)check=UsernameCheck.Available;}
            busy=false;done(check);
        }
        private IEnumerator Backup()
        {
            if(collection==null||stored.session?.user==null||profile==null)yield break;
            string owner=PlayerPrefs.GetString("pockle.legacy.owner.v1","");
            if(owner.Length>0&&owner!=stored.session.user.id){LastError="This phone’s collection is linked to another account. Use that account to preserve it.";yield break;}
            string raw=collection.ExportLegacySnapshot();var local=Parse<CollectionSave>(raw);
            if(local==null||local.Version<1||local.Version>CollectionProgress.SaveVersion){LastError="This save needs a compatible app before backup.";yield break;}
            string digest=AccountProtocol.Digest(raw);
            if(digest==lastDigest){ready=true;yield break;}
            string source=PlayerPrefs.GetString("pockle.legacy.source.v1","");
            if(source.Length==0){source=Guid.NewGuid().ToString();PlayerPrefs.SetString("pockle.legacy.source.v1",source);PlayerPrefs.Save();}
            string reply=null;bool ok=false;yield return Api("legacy_upload",JsonUtility.ToJson(new SnapshotPayload{source_id=source,save_json=raw}),(r,s)=>{reply=r;ok=s;});
            var receipt=ok?Parse<Receipt>(reply):null;
            if(receipt==null||receipt.digest!=digest||receipt.inventory_rows!=(local.Inventory?.Count??0)||receipt.fusion_rows!=(local.Fusions?.Count??0))
            {LastError="Cloud backup is not confirmed. Your full phone save is still safe here.";yield break;}
            // Mark ownership only after the server commits the exact full snapshot; never replace the local collection.
            PlayerPrefs.SetString("pockle.legacy.owner.v1",stored.session.user.id);PlayerPrefs.SetString("pockle.legacy.receipt.v1",receipt.receipt_id);PlayerPrefs.SetString("pockle.legacy.digest.v1",digest);PlayerPrefs.Save();
            lastDigest=digest;ready=true;
        }
        public void SignOut(Action done) {StopAllCoroutines();exchanging=false;var pending=signInDone;signInDone=null;pending?.Invoke(new AccountResult(AccountStatus.Cancelled));busy=true;StartCoroutine(EndSession(false,_=>done()));}
        public void RequestDeletion(Action<bool> done) {if(busy||stored.session==null){done(false);return;}busy=true;StartCoroutine(EndSession(true,done));}
        private IEnumerator EndSession(bool deletion,Action<bool> done)
        {
            yield return RefreshIfNeeded();bool ok=false;
            if(stored.session!=null)yield return Api(deletion?"delete_request":"sign_out","{}",(_,s)=>ok=s);
            if(deletion&&!ok){busy=false;done(false);yield break;}
            ready=false;profile=null;stored=new Stored();SaveSecure();lastDigest="";busy=false;done(true);
        }
        private IEnumerator RefreshIfNeeded()
        {
            if(stored.session==null||stored.session.expires_at>Now+60)yield break;
            string reply=null;bool ok=false;yield return Send("/auth/v1/token?grant_type=refresh_token",JsonUtility.ToJson(new TokenRequest{refresh_token=stored.session.refresh_token}),false,(r,s)=>{reply=r;ok=s;});
            if(!ok||!AcceptToken(reply)){ready=false;stored=new Stored();SaveSecure();LastError="Sign in again to reconnect your cloud save.";}
        }
        private bool AcceptToken(string json)
        {
            var token=Parse<Session>(json);if(token==null||token.user==null||string.IsNullOrEmpty(token.access_token)||string.IsNullOrEmpty(token.refresh_token))return false;
            if(token.expires_at==0)token.expires_at=Now+token.expires_in;stored.session=token;return SaveSecure();
        }
        private IEnumerator Api(string action,string payload,Action<string,bool> done)
        {
            // Payload is already JSON, never serialized through object (JsonUtility omits object members).
            yield return Send("/functions/v1/pockle-api","{\"action\":\""+action+"\",\"payload\":"+payload+"}",true,done);
        }
        private IEnumerator Send(string path,string body,bool authorized,Action<string,bool> done)
        {
            if(config==null||authorized&&stored.session==null){done(null,false);yield break;}
            using(var request=new UnityWebRequest(config.url+path,"POST"))
            {
                request.uploadHandler=new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));request.downloadHandler=new DownloadHandlerBuffer();request.timeout=30;
                request.SetRequestHeader("Content-Type","application/json");request.SetRequestHeader("apikey",config.publishableKey);
                if(authorized)request.SetRequestHeader("Authorization","Bearer "+stored.session.access_token);
                yield return request.SendWebRequest();done(request.downloadHandler.text,request.result==UnityWebRequest.Result.Success);
            }
        }
        private bool SaveSecure(){try{AccountTokenStore.Write(JsonUtility.ToJson(stored));return true;}catch{ready=false;return false;}}
        private static T Parse<T>(string json) where T:class {try{return string.IsNullOrEmpty(json)?null:JsonUtility.FromJson<T>(json);}catch{return null;}}
        private void FinishSignIn(bool ok,string message){busy=false;var done=signInDone;signInDone=null;done?.Invoke(new AccountResult(ok?AccountStatus.SignedIn:AccountStatus.Failed,message??"",ok&&!string.IsNullOrEmpty(profile?.username)));}
        private void CollectionChanged(){if(IsSignedIn&&backupAt<0)backupAt=Time.unscaledTime+60;}
        private void Update()
        {
            if(!string.IsNullOrEmpty(stored.verifier)&&Now-stored.started>300){stored.verifier="";stored.request="";SaveSecure();FinishSignIn(false,"Sign-in timed out. Please try again.");}
            if(backupAt>0&&Time.unscaledTime>=backupAt&&!busy&&IsSignedIn){backupAt=-1;busy=true;StartCoroutine(BackupChanged());}
        }
        private IEnumerator BackupChanged(){yield return RefreshIfNeeded();yield return Backup();busy=false;}
        private void OnDestroy(){Application.deepLinkActivated-=OnDeepLink;if(collection!=null)collection.Changed-=CollectionChanged;}
    }
}
