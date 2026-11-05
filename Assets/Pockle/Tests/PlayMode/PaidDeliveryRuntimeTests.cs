using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
namespace Pockle.Tests
{
    public sealed class PaidDeliveryRuntimeTests
    {
        private const string Key="pockle.collection.v1";
        private readonly Dictionary<string,string> before=new Dictionary<string,string>();
        private GameObject root;
        private CollectionSession session;
        private static readonly string[] Keys={Key,Key+".backup",Key+".before-v2",Key+".before-v3",Key+".before-v4",Key+".before-v5"};
        [SetUp] public void Setup()
        {
            foreach(string key in Keys){if(PlayerPrefs.HasKey(key))before[key]=PlayerPrefs.GetString(key);PlayerPrefs.DeleteKey(key);}
            PlayerPrefs.SetString(Key,JsonUtility.ToJson(new CollectionSave{Version=4,Day=CollectionSession.Today,
                Inventory=new List<OwnedCollectible>{new OwnedCollectible(ToyCatalog.PeachId,3),new OwnedCollectible(ToyCatalog.MoonId,8)}}));
            root=new GameObject("Paid delivery fixture");session=root.AddComponent<CollectionSession>();session.Initialize();
        }
        private bool Commit(string receipt,Action<string> save=null)=>(bool)typeof(CollectionSession).GetMethod("CommitPaidDelivery",BindingFlags.Instance|BindingFlags.NonPublic)
            .Invoke(session,new object[]{receipt,"pockle.box.pip.v1",new[]{ToyCatalog.PeachId},save});
        [TearDown] public void Cleanup()
        {
            if(root!=null)UnityEngine.Object.DestroyImmediate(root);
            foreach(string key in Keys){PlayerPrefs.DeleteKey(key);if(before.TryGetValue(key,out string value))PlayerPrefs.SetString(key,value);}PlayerPrefs.Save();
        }
        [Test] public void PaidCopyPersistsBeforeObserversAndDoesNotReplayAfterRestart()
        {
            string receipt=Guid.NewGuid().ToString();bool observed=false;
            session.Changed+=()=>{var save=JsonUtility.FromJson<CollectionSave>(PlayerPrefs.GetString(Key));Assert.That(save.PaidDeliveryIds,Does.Contain(receipt));observed=true;};
            Assert.That(Commit(receipt),Is.True);Assert.That(observed,Is.True);
            Assert.That(session.GetOwnedCount(ToyCatalog.PeachId),Is.EqualTo(4));
            UnityEngine.Object.DestroyImmediate(root);root=new GameObject("Restart");session=root.AddComponent<CollectionSession>();session.Initialize();
            Assert.That(Commit(receipt),Is.True);Assert.That(session.GetOwnedCount(ToyCatalog.PeachId),Is.EqualTo(4));
            Assert.That(session.GetOwnedCount(ToyCatalog.MoonId),Is.EqualTo(8));
        }
        [Test] public void FailedPersistenceCannotRevealOrMarkReceiptAndRetryStillGrantsOneCopy()
        {
            string receipt=Guid.NewGuid().ToString();string old=PlayerPrefs.GetString(Key);bool observed=false;
            session.Changed+=()=>observed=true;
            Assert.That(Commit(receipt,_=>throw new InvalidOperationException("disk fixture")),Is.False);
            Assert.That(observed,Is.False);Assert.That(PlayerPrefs.GetString(Key),Is.EqualTo(old));
            Assert.That(session.GetOwnedCount(ToyCatalog.PeachId),Is.EqualTo(3));
            Assert.That(Commit(receipt),Is.True);Assert.That(session.GetOwnedCount(ToyCatalog.PeachId),Is.EqualTo(4));
        }
        [Test] public void V4BackupIsRetainedBeforeTheAdditiveReceiptMigration()
        {
            Assert.That(session.Progress.Save.Version,Is.EqualTo(5));
            var backup=JsonUtility.FromJson<CollectionSave>(PlayerPrefs.GetString(Key+".before-v5"));
            Assert.That(backup.Version,Is.EqualTo(4));Assert.That(backup.Inventory.Count,Is.EqualTo(2));
        }
    }
}
