using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace Pockle.Tests
{
    public sealed class AccountRefreshCaller : MonoBehaviour { }

    public sealed class AccountRefreshRuntimeTests
    {
        private const string A="11111111-1111-4111-8111-111111111111",B="22222222-2222-4222-8222-222222222222";
        private sealed class Pending
        {
            public string path,reply; public bool released,success; public Action<string,bool> callback;
        }
        private GameObject root,callerRoot;
        private SupabaseAccountService account;
        private AccountRefreshCaller caller;
        private readonly List<Pending> requests=new List<Pending>();
        private readonly List<string> writes=new List<string>();
        private static FieldInfo Field(string name)=>typeof(SupabaseAccountService).GetField(name,BindingFlags.Instance|BindingFlags.NonPublic);
        private void Set(string name,object value)=>Field(name).SetValue(account,value);
        private static string Token(string user,string tag,long expires)=>"{\"access_token\":\"test-access-"+tag+"\",\"refresh_token\":\"test-refresh-"+tag+"\",\"expires_at\":"+expires+",\"user\":{\"id\":\""+user+"\"}}";
        private void Session(string user,string tag="old",bool expired=true)
        {
            string json="{\"session\":"+Token(user,tag,expired?1:DateTimeOffset.UtcNow.ToUnixTimeSeconds()+3600)+"}";
            Set("stored",JsonUtility.FromJson(json,Field("stored").FieldType));Set("ready",true);
        }
        private void ReplaceToken(string user,string tag)
        {
            object owner=Field("stored").GetValue(account);
            var session=owner.GetType().GetField("session");
            session.SetValue(owner,JsonUtility.FromJson(Token(user,tag,DateTimeOffset.UtcNow.ToUnixTimeSeconds()+3600),session.FieldType));
        }
        [SetUp] public void Setup()
        {
            requests.Clear();writes.Clear();root=new GameObject("Account refresh fixture");callerRoot=new GameObject("External refresh callers");
            account=root.AddComponent<SupabaseAccountService>();caller=callerRoot.AddComponent<AccountRefreshCaller>();
            Set("sendOverride",new Func<string,string,bool,Action<string,bool>,IEnumerator>(Transport));
            Set("secureWriteOverride",new Action<string>(json=>writes.Add(json)));
            Session(A);
        }
        [TearDown] public void Cleanup()
        {
            UnityEngine.Object.DestroyImmediate(callerRoot);UnityEngine.Object.DestroyImmediate(root);
        }
        private IEnumerator Transport(string path,string body,bool authorized,Action<string,bool> done)
        {
            var pending=new Pending{path=path,callback=done};requests.Add(pending);
            while(!pending.released)yield return null;
            done(pending.reply,pending.success);
        }
        private IEnumerator Refresh(Action done)
        {
            yield return (IEnumerator)typeof(SupabaseAccountService).GetMethod("RefreshIfNeeded",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(account,null);
            done();
        }
        private static void Release(Pending pending,bool success,string reply=null)
        {pending.success=success;pending.reply=reply;pending.released=true;}
        private static IEnumerator Until(Func<bool> done)
        {
            for(int i=0;i<30&&!done();i++)yield return null;
            Assert.That(done(),Is.True,"Coroutine did not finish; a cancelled refresh may have stranded its waiters.");
        }
        [UnityTest] public IEnumerator ConcurrentWalkingCheckoutAndBackupWaitForOneRotation()
        {
            int finished=0;
            for(int i=0;i<3;i++)caller.StartCoroutine(Refresh(()=>finished++));
            yield return Until(()=>requests.Count==1);
            Assert.That(requests[0].path,Does.EndWith("grant_type=refresh_token"));
            yield return null;Assert.That(requests.Count,Is.EqualTo(1));
            Release(requests[0],true,Token(A,"new",DateTimeOffset.UtcNow.ToUnixTimeSeconds()+3600));
            yield return Until(()=>finished==3);
            Assert.That(writes.Count,Is.EqualTo(1));Assert.That(writes[0],Does.Contain("test-refresh-new"));
            Assert.That(account.UserId,Is.EqualTo(A));Assert.That(Field("refresh").GetValue(account),Is.Null);
            caller.StartCoroutine(Refresh(()=>finished++));yield return Until(()=>finished==4);
            Assert.That(requests.Count,Is.EqualTo(1),"Fresh token should not rotate again.");
        }
        [UnityTest] public IEnumerator LateRefreshSuccessCannotReplaceNewerAccountSession()
        {
            bool finished=false;caller.StartCoroutine(Refresh(()=>finished=true));yield return Until(()=>requests.Count==1);
            Session(B,"newer",false);
            Release(requests[0],true,Token(A,"late",DateTimeOffset.UtcNow.ToUnixTimeSeconds()+3600));
            yield return Until(()=>finished&&Field("refresh").GetValue(account)==null);
            Assert.That(account.UserId,Is.EqualTo(B));Assert.That(writes,Is.Empty);
        }
        [UnityTest] public IEnumerator CancellingOneCallerDoesNotCancelSharedRefresh()
        {
            bool first=false,second=false;
            Coroutine cancelled=caller.StartCoroutine(Refresh(()=>first=true));
            caller.StartCoroutine(Refresh(()=>second=true));yield return Until(()=>requests.Count==1);
            caller.StopCoroutine(cancelled);
            Release(requests[0],true,Token(A,"survived",DateTimeOffset.UtcNow.ToUnixTimeSeconds()+3600));
            yield return Until(()=>second);
            Assert.That(first,Is.False);Assert.That(writes.Count,Is.EqualTo(1));Assert.That(account.UserId,Is.EqualTo(A));
        }
        [UnityTest] public IEnumerator LateRefreshFailureCannotClearNewerSameAccountSession()
        {
            bool finished=false;caller.StartCoroutine(Refresh(()=>finished=true));yield return Until(()=>requests.Count==1);
            // Same account with a newer session still requires identity-of-session checking.
            ReplaceToken(A,"newer");Release(requests[0],false);
            yield return Until(()=>finished&&Field("refresh").GetValue(account)==null);
            Assert.That(account.UserId,Is.EqualTo(A));Assert.That(account.LastError,Is.Null);Assert.That(writes,Is.Empty);
        }
        [UnityTest] public IEnumerator SignOutReleasesExternalWaitersAndDoesNotStartSecondRotation()
        {
            int finished=0;bool signedOut=false;
            caller.StartCoroutine(Refresh(()=>finished++));caller.StartCoroutine(Refresh(()=>finished++));
            yield return Until(()=>requests.Count==1);
            account.SignOut(()=>signedOut=true);
            yield return Until(()=>finished==2&&requests.Count==2);
            Assert.That(requests[1].path,Does.EndWith("/functions/v1/pockle-api"));
            Assert.That(account.IsSignedIn,Is.False);Assert.That(Field("refresh").GetValue(account),Is.Null);
            // A late transport callback from the cancelled request cannot mutate the account.
            requests[0].callback(Token(A,"cancelled",DateTimeOffset.UtcNow.ToUnixTimeSeconds()+3600),true);
            Release(requests[1],true,"{}");yield return Until(()=>signedOut);
            Assert.That(account.IsSignedIn,Is.False);Assert.That(writes.Count,Is.EqualTo(1));
            Assert.That(writes[0],Does.Not.Contain("test-refresh-cancelled"));
            Session(A,"again");caller.StartCoroutine(Refresh(()=>finished++));yield return Until(()=>requests.Count==3);
            Release(requests[2],true,Token(A,"again-new",DateTimeOffset.UtcNow.ToUnixTimeSeconds()+3600));
            yield return Until(()=>finished==3);Assert.That(account.UserId,Is.EqualTo(A));
        }
        [UnityTest] public IEnumerator CancelledFailureAndLogoutCannotEraseNewlyInstalledSession()
        {
            bool finished=false,signedOut=false;caller.StartCoroutine(Refresh(()=>finished=true));yield return Until(()=>requests.Count==1);
            account.SignOut(()=>signedOut=true);yield return Until(()=>finished&&requests.Count==2);
            Session(B,"newer",false);requests[0].callback(null,false);Release(requests[1],true,"{}");
            yield return Until(()=>signedOut);
            Assert.That(account.UserId,Is.EqualTo(B));Assert.That(writes,Is.Empty);
        }
    }
}
