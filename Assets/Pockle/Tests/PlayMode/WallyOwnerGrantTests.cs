using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
    public sealed class WallyOwnerGrantTests
    {
        private const string SaveKey="pockle.collection.v1";
        private readonly Dictionary<string,string> saved=new Dictionary<string,string>();
        private readonly string[] keys={SaveKey,SaveKey+".backup",SaveKey+".before-v2",SaveKey+".before-v3",SaveKey+".before-v4"};
        private GameObject root;
        private bool hadMarker;private int marker;
        [SetUp]public void SetUp()
        {
            foreach(string key in keys){if(PlayerPrefs.HasKey(key))saved[key]=PlayerPrefs.GetString(key);PlayerPrefs.DeleteKey(key);}
            hadMarker=PlayerPrefs.HasKey(CollectionSession.WallyTestGrantKey);marker=PlayerPrefs.GetInt(CollectionSession.WallyTestGrantKey);PlayerPrefs.DeleteKey(CollectionSession.WallyTestGrantKey);
            PlayerPrefs.SetString(SaveKey,JsonUtility.ToJson(new CollectionSave{Version=CollectionProgress.SaveVersion,Day=CollectionSession.Today,Steps=521,Claimed=true,PendingRevealId=ToyCatalog.MoonId,
                Inventory=new List<OwnedCollectible>{new OwnedCollectible(ToyCatalog.PeachId,5),new OwnedCollectible(WallyCatalog.ClassicId,7)}}));
        }
        [TearDown]public void TearDown()
        {
            if(root!=null)UnityEngine.Object.DestroyImmediate(root);
            foreach(string key in keys){if(saved.TryGetValue(key,out var json))PlayerPrefs.SetString(key,json);else PlayerPrefs.DeleteKey(key);}saved.Clear();
            if(hadMarker)PlayerPrefs.SetInt(CollectionSession.WallyTestGrantKey,marker);else PlayerPrefs.DeleteKey(CollectionSession.WallyTestGrantKey);
            PlayerPrefs.Save();FusionCatalog.Restore(null);
        }
        private CollectionSession Start()
        {root=new GameObject("Wally owner test session");var session=root.AddComponent<CollectionSession>();session.Initialize();return session;}
        [UnityTest]public IEnumerator CopiesAreDurableBeforeRefreshAndConsumedSparesNeverRefillOnRestart()
        {
            var session=Start();bool refreshed=false;
            session.Changed+=()=>{
                var stored=new CollectionProgress(JsonUtility.FromJson<CollectionSave>(PlayerPrefs.GetString(SaveKey)),CollectionSession.Today);
                Assert.That(stored.GetOwnedCount(WallyCatalog.GlowId),Is.EqualTo(3));Assert.That(PlayerPrefs.GetInt(CollectionSession.WallyTestGrantKey),Is.EqualTo(1));refreshed=true;
            };
            Assert.That(session.TryApplyOwnerWallyTestGrant(),Is.True);Assert.That(refreshed,Is.True);Assert.That(session.TryApplyOwnerWallyTestGrant(),Is.False);
            Assert.That(session.Progress.GetOwnedCount(ToyCatalog.PeachId),Is.EqualTo(5));Assert.That(session.Progress.GetOwnedCount(WallyCatalog.ClassicId),Is.EqualTo(7));
            Assert.That(session.Progress.OwnedCounts.Count(x=>x.Key.StartsWith("wally.")&&x.Value>0),Is.EqualTo(10));
            Assert.That(session.Progress.Save.Steps,Is.EqualTo(521));Assert.That(session.Progress.Save.Claimed,Is.True);Assert.That(session.Progress.Save.PendingRevealId,Is.EqualTo(ToyCatalog.MoonId));
            UnityEngine.Object.DestroyImmediate(root);root=null;
            var consumed=JsonUtility.FromJson<CollectionSave>(PlayerPrefs.GetString(SaveKey));consumed.Inventory.Single(x=>x.Id=="wally.clear-jelly").Count=2;
            PlayerPrefs.SetString(SaveKey,JsonUtility.ToJson(consumed));PlayerPrefs.Save();session=Start();
            Assert.That(session.TryApplyOwnerWallyTestGrant(),Is.False);Assert.That(session.Progress.GetOwnedCount("wally.clear-jelly"),Is.EqualTo(2));
            yield return null;
        }
        [UnityTest]public IEnumerator FailedSavePublishesNoGrantOrMarkerAndCanRetrySafely()
        {
            var session=Start();int changed=0;session.Changed+=()=>changed++;
            Assert.That(session.TryApplyOwnerWallyTestGrant(_=>throw new System.IO.IOException("injected test write failure")),Is.False);
            Assert.That(changed,Is.Zero);Assert.That(PlayerPrefs.GetInt(CollectionSession.WallyTestGrantKey),Is.Zero);
            Assert.That(session.Progress.GetOwnedCount(WallyCatalog.GlowId),Is.Zero);Assert.That(session.Progress.GetOwnedCount(WallyCatalog.ClassicId),Is.EqualTo(7));
            Assert.That(session.TryApplyOwnerWallyTestGrant(),Is.True);Assert.That(changed,Is.EqualTo(1));Assert.That(session.Progress.GetOwnedCount(WallyCatalog.GlowId),Is.EqualTo(3));
            yield return null;
        }
        [UnityTest]public IEnumerator FutureReadOnlySaveCannotReceiveOrMarkTheGrant()
        {
            var future=JsonUtility.FromJson<CollectionSave>(PlayerPrefs.GetString(SaveKey));future.Version=CollectionProgress.SaveVersion+1;string json=JsonUtility.ToJson(future);PlayerPrefs.SetString(SaveKey,json);
            var session=Start();Assert.That(session.TryApplyOwnerWallyTestGrant(),Is.False);Assert.That(PlayerPrefs.GetString(SaveKey),Is.EqualTo(json));Assert.That(PlayerPrefs.GetInt(CollectionSession.WallyTestGrantKey),Is.Zero);
            yield return null;
        }
    }
}
