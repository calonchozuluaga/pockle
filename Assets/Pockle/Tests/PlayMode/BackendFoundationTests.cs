using System.Collections;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
    public sealed class BackendFoundationTests
    {
        [UnityTest] public IEnumerator LegacyExportPreservesExactJsonAndUnknownFields()
        {
            const string key="pockle.collection.v1";bool had=PlayerPrefs.HasKey(key);string old=PlayerPrefs.GetString(key);var root=new GameObject("Legacy backup test");
            try
            {
                PlayerPrefs.DeleteKey(key);var session=root.AddComponent<CollectionSession>();session.Initialize();
                const string raw="{\"Version\":4, \"Inventory\":[{\"Id\":\"future.unknown\",\"Count\":23}],\"Fusions\":[{\"Id\":\"future.fused\",\"Extra\":\"unchanged\"}],\"FutureField\":{\"a\":17}}";
                PlayerPrefs.SetString(key,raw);Assert.That(session.ExportLegacySnapshot(),Is.EqualTo(raw));
                Assert.That(PlayerPrefs.GetString(key),Is.EqualTo(raw));
            }
            finally{Object.DestroyImmediate(root);if(had)PlayerPrefs.SetString(key,old);else PlayerPrefs.DeleteKey(key);PlayerPrefs.Save();}
            yield return null;
        }
        [Test] public void NotificationsCheckPreferenceAtScheduleAndDisplayAndCancelPending()
        {
            const string prefix="pockle.notifications.v1.";int[] old=new int[4];bool[] had=new bool[4];int cancelled=0,scheduled=0,shown=0;
            for(int i=0;i<4;i++){string key=prefix+NotificationPrefs.Kinds[i];had[i]=PlayerPrefs.HasKey(key);old[i]=PlayerPrefs.GetInt(key);}
            try
            {
                using(var gate=new NotificationGate(_=>cancelled++))foreach(string kind in NotificationPrefs.Kinds)
                {
                    NotificationPrefs.Set(kind,true);Assert.IsTrue(gate.Schedule(kind,()=>scheduled++));
                    NotificationPrefs.Set(kind,false);Assert.IsFalse(gate.Schedule(kind,()=>scheduled++));Assert.IsFalse(gate.Show(kind,()=>shown++));
                    NotificationPrefs.Set(kind,true);Assert.IsTrue(gate.Show(kind,()=>shown++));
                }
                Assert.That(cancelled,Is.EqualTo(4));Assert.That(scheduled,Is.EqualTo(4));Assert.That(shown,Is.EqualTo(4));
            }
            finally{for(int i=0;i<4;i++){string key=prefix+NotificationPrefs.Kinds[i];if(had[i])PlayerPrefs.SetInt(key,old[i]);else PlayerPrefs.DeleteKey(key);}PlayerPrefs.Save();}
        }
        [UnityTest] public IEnumerator AccountAdapterRequiresAgeGateAndNeverInventsAGuestSession()
        {
            const string age="pockle.onboarding.v1.ageConfirmed",locked="pockle.onboarding.v1.ageLocked";
            bool ha=PlayerPrefs.HasKey(age),hl=PlayerPrefs.HasKey(locked);int a=PlayerPrefs.GetInt(age),l=PlayerPrefs.GetInt(locked);
            var root=new GameObject("Backend adapter age test");
            try
            {
                PlayerPrefs.SetInt(age,0);PlayerPrefs.SetInt(locked,0);var collection=root.AddComponent<CollectionSession>();collection.Initialize();
                var account=root.AddComponent<SupabaseAccountService>();bool initialized=false;account.Initialize(collection,()=>initialized=true);
                Assert.IsTrue(initialized);Assert.IsFalse(account.IsSignedIn);bool called=false;
                account.SignIn(AccountProvider.Google,result=>{called=true;Assert.That(result.Status,Is.EqualTo(AccountStatus.Failed));});
                Assert.IsTrue(called);Assert.IsFalse(account.IsSignedIn);
            }
            finally{Object.DestroyImmediate(root);if(ha)PlayerPrefs.SetInt(age,a);else PlayerPrefs.DeleteKey(age);if(hl)PlayerPrefs.SetInt(locked,l);else PlayerPrefs.DeleteKey(locked);PlayerPrefs.Save();}
            yield return null;
        }
    }
}
