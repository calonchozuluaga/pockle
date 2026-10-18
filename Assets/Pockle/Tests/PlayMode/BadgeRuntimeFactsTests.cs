using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
    public sealed class BadgeRuntimeFactsTests
    {
        private const string SaveKey = "pockle.collection.v1";
        private readonly Dictionary<string,string> previous = new Dictionary<string,string>();
        private readonly string[] keys = { SaveKey, SaveKey+".backup", SaveKey+".before-v2", SaveKey+".before-v3", SaveKey+".before-v4", CollectionSession.BadgePreferenceKey };
        private GameObject root;
        [SetUp] public void SetUp()
        {
            foreach (string key in keys) { if (PlayerPrefs.HasKey(key)) previous[key] = PlayerPrefs.GetString(key); PlayerPrefs.DeleteKey(key); }
        }
        [UnityTearDown] public IEnumerator TearDown()
        {
            if (root != null) Object.Destroy(root); yield return null;
            foreach (string key in keys) { if (previous.TryGetValue(key,out var value)) PlayerPrefs.SetString(key,value); else PlayerPrefs.DeleteKey(key); }
            previous.Clear(); PlayerPrefs.Save(); FusionCatalog.Restore(null);
        }
        private CollectionSession StartSession()
        {
            root = new GameObject("Badge runtime facts"); var session = root.AddComponent<CollectionSession>(); session.Initialize(); return session;
        }
        [UnityTest] public IEnumerator VersionThreeMigrationPreservesInventoryAndRestoresBadgeDatesFromSavedSnapshot()
        {
            var save = new CollectionSave { Version=3,Day=CollectionSession.Today,Steps=321,Inventory=new List<OwnedCollectible>{new OwnedCollectible(ToyCatalog.PeachId,5),new OwnedCollectible("future.toy",2)} };
            string original = JsonUtility.ToJson(save); PlayerPrefs.SetString(SaveKey,original);
            PlayerPrefs.SetString(CollectionSession.BadgePreferenceKey,"first-fusion=201,secret-finder=202");
            var session = StartSession();
            Assert.AreEqual(4,session.Progress.Save.Version); Assert.AreEqual(original,PlayerPrefs.GetString(SaveKey+".before-v4"));
            Assert.AreEqual(5,session.GetOwnedCount(ToyCatalog.PeachId)); Assert.AreEqual(2,session.GetOwnedCount("future.toy"));
            Assert.AreEqual(321,session.LifetimeSteps); Assert.AreEqual(321,session.DailyWalkingSteps);
            Assert.IsEmpty(session.BadgeEligibleOwnedCounts,"Old copies have unknown provenance, not invented achievements.");
            Assert.AreEqual("first-fusion=201,secret-finder=202",JsonUtility.FromJson<CollectionSave>(PlayerPrefs.GetString(SaveKey)).BadgeState.EarnedBadges);
            Object.Destroy(root); root=null; yield return null;
            PlayerPrefs.DeleteKey(CollectionSession.BadgePreferenceKey);
            session = StartSession();
            Assert.AreEqual("first-fusion=201,secret-finder=202",PlayerPrefs.GetString(CollectionSession.BadgePreferenceKey),"Restoring the collection restores badge dates before the HUD loads.");
            PlayerPrefs.SetString(CollectionSession.BadgePreferenceKey,"first-fusion=205,new-badge=203");
            float deadline=Time.realtimeSinceStartup+2.5f;
            while (Time.realtimeSinceStartup<deadline && !JsonUtility.FromJson<CollectionSave>(PlayerPrefs.GetString(SaveKey)).BadgeState.EarnedBadges.Contains("new-badge=203")) yield return null;
            Assert.That(JsonUtility.FromJson<CollectionSave>(PlayerPrefs.GetString(SaveKey)).BadgeState.EarnedBadges, Does.Contain("new-badge=203"), "New dates reach the main save without waiting for pause or a walking change.");
            Object.Destroy(root); root=null; yield return null;
            var persisted = JsonUtility.FromJson<CollectionSave>(PlayerPrefs.GetString(SaveKey));
            Assert.AreEqual("first-fusion=201,secret-finder=202,new-badge=203",persisted.BadgeState.EarnedBadges,"Save merges dates and never revokes earlier badges.");
        }
        [UnityTest] public IEnumerator ValidatedHistoryAndEarnedRewardProvenanceSurviveSessionRestart()
        {
            var session = StartSession(); int day = CollectionSession.Today;
            session.Progress.ObserveSteps(day,0,0,1); session.Progress.ObserveSteps(day,10000,2500,1);
            Assert.AreEqual(10000,session.DailyWalkingSteps); Assert.AreEqual(1000,session.Progress.Save.Steps);
            Assert.IsTrue(session.Progress.TryClaimDaily(day,ToyCatalog.PeachId)); session.FinishReveal();
            Assert.AreEqual(1,session.BadgeEligibleOwnedCounts[ToyCatalog.PeachId]); Assert.AreEqual(1,session.LongestWalkingBoxStreak);
            Object.Destroy(root); root=null; yield return null;
            session = StartSession();
            Assert.AreEqual(10000,session.LifetimeSteps); Assert.AreEqual(10000,session.BestDailySteps);
            Assert.AreEqual(1,session.BadgeEligibleOwnedCounts[ToyCatalog.PeachId]); Assert.AreEqual(1,session.WalkingDays.Count);
        }
        [UnityTest] public IEnumerator FutureSaveNeverChangesPrimarySaveOrLocalBadgeLedger()
        {
            string raw = "{\"Version\":99,\"Counts\":[1,2,3,4],\"unknown\":true}";
            PlayerPrefs.SetString(SaveKey,raw); PlayerPrefs.SetString(CollectionSession.BadgePreferenceKey,"first-fusion=203");
            var session=StartSession(); Assert.IsTrue(session.IsReadOnlySave);
            Object.Destroy(root);root=null;yield return null;
            Assert.AreEqual(raw,PlayerPrefs.GetString(SaveKey)); Assert.AreEqual("first-fusion=203",PlayerPrefs.GetString(CollectionSession.BadgePreferenceKey));
        }
    }
}
