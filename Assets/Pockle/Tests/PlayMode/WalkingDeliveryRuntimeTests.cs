using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
namespace Pockle.Tests
{
    public sealed class WalkingDeliveryRuntimeTests
    {
        private const string Key="pockle.collection.v1";
        private readonly Dictionary<string,string> previous=new Dictionary<string,string>();
        private readonly string[] keys={Key,Key+".backup",Key+".before-v2",Key+".before-v3",Key+".before-v4",Key+".before-v5",Key+".before-v6",CollectionSession.BadgePreferenceKey};
        private GameObject root;private CollectionSession session;
        [SetUp] public void Setup()
        {
            foreach(var key in keys){if(PlayerPrefs.HasKey(key))previous[key]=PlayerPrefs.GetString(key);PlayerPrefs.DeleteKey(key);}
            PlayerPrefs.SetString(Key,JsonUtility.ToJson(new CollectionSave{Version=5,Day=CollectionSession.Today,
                Inventory=new List<OwnedCollectible>{new OwnedCollectible(ToyCatalog.FoxId,3),new OwnedCollectible(ToyCatalog.PeachId,5)}}));
            Start();
        }
        private void Start(){root=new GameObject("Walking receipt fixture");session=root.AddComponent<CollectionSession>();session.Initialize();}
        private bool Commit(string receipt,Action<string> persist=null)=>(bool)typeof(CollectionSession).GetMethod("CommitWalkingDelivery",BindingFlags.Instance|BindingFlags.NonPublic)
            .Invoke(session,new object[]{receipt,"fox",ToyCatalog.FoxId,CollectionSession.Today,persist});
        [TearDown] public void Cleanup()
        {
            if(root!=null)UnityEngine.Object.DestroyImmediate(root);
            foreach(var key in keys){PlayerPrefs.DeleteKey(key);if(previous.TryGetValue(key,out var value))PlayerPrefs.SetString(key,value);}PlayerPrefs.Save();
        }
        [Test] public void WalkingSavePrecedesObserverAndReplayAfterRestartDoesNotGrantAgain()
        {
            string receipt=Guid.NewGuid().ToString("D");bool observed=false;
            session.Changed+=()=>{Assert.That(JsonUtility.FromJson<CollectionSave>(PlayerPrefs.GetString(Key)).WalkingDeliveryIds,Does.Contain(receipt));observed=true;};
            Assert.That(Commit(receipt),Is.True);Assert.That(observed,Is.True);Assert.That(session.GetOwnedCount(ToyCatalog.FoxId),Is.EqualTo(4));
            UnityEngine.Object.DestroyImmediate(root);Start();
            Assert.That(Commit(receipt),Is.True);Assert.That(session.GetOwnedCount(ToyCatalog.FoxId),Is.EqualTo(4));
            Assert.That(session.GetOwnedCount(ToyCatalog.PeachId),Is.EqualTo(5));
        }
        [Test] public void WalkingFailedSaveDoesNotMutateOrRevealAndRetryIsSafe()
        {
            string receipt=Guid.NewGuid().ToString("D"),before=PlayerPrefs.GetString(Key);bool observed=false;
            session.Changed+=()=>observed=true;
            Assert.That(Commit(receipt,_=>throw new InvalidOperationException("disk fixture")),Is.False);
            Assert.That(observed,Is.False);Assert.That(PlayerPrefs.GetString(Key),Is.EqualTo(before));
            Assert.That(session.GetOwnedCount(ToyCatalog.FoxId),Is.EqualTo(3));Assert.That(Commit(receipt),Is.True);
        }
        [Test] public void V5BackupSurvivesAdditiveWalkingReceiptMigration()
        {
            var backup=JsonUtility.FromJson<CollectionSave>(PlayerPrefs.GetString(Key+".before-v6"));
            Assert.That(backup.Version,Is.EqualTo(5));Assert.That(backup.Inventory.Count,Is.EqualTo(2));
            Assert.That(session.Progress.Save.Version,Is.EqualTo(6));
        }
        [Test] public void ServerCountdownOnlyCountsFutureWindowsAndNeverUsesPhoneTime()
        {
            var method=typeof(ServerWalkingBoxService).GetMethod("Countdown",BindingFlags.Static|BindingFlags.NonPublic);
            object Run(string next,bool paused=false)=>method.Invoke(null,new object[]{"2026-11-09T10:00:00Z",next,paused});
            Assert.That(Run("2026-11-09T09:59:59Z"),Is.Null,"An open earning window must show remaining steps, not Checking forever.");
            Assert.That(Run("2026-11-09T10:00:00Z"),Is.Null);
            Assert.That(Run("2026-11-09T11:00:00Z"),Is.EqualTo(3600d));
            Assert.That(Run("2026-11-09T11:00:00Z",true),Is.Null,"Full banks/daily limits pause countdown.");
            Assert.That(Run("malformed"),Is.Null);
        }
    }
}
