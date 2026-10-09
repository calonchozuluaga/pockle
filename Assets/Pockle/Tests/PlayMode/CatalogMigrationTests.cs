#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace Pockle.Tests
{
    public sealed class CatalogMigrationTests
    {
        private const string SaveKey = "pockle.collection.v1";
        private readonly string[] textKeys = { SaveKey, SaveKey + ".backup", SaveKey + ".before-v2",
            "pockle.profile.local.name", "pockle.profile.local.avatarId", "pockle.profile.local.favoriteId" };
        private readonly string[] intKeys = { "pockle.profile.local.avatar", "pockle.profile.local.favorite" };
        private readonly Dictionary<string, string> oldText = new Dictionary<string, string>();
        private readonly Dictionary<string, int> oldInts = new Dictionary<string, int>();
        private GameObject root;

        [SetUp]
        public void SetUp()
        {
            oldText.Clear(); oldInts.Clear();
            foreach (string key in textKeys)
            { if (PlayerPrefs.HasKey(key)) oldText.Add(key, PlayerPrefs.GetString(key)); PlayerPrefs.DeleteKey(key); }
            foreach (string key in intKeys)
            { if (PlayerPrefs.HasKey(key)) oldInts.Add(key, PlayerPrefs.GetInt(key)); PlayerPrefs.DeleteKey(key); }
        }
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (root != null) Object.Destroy(root);
            yield return null;
            foreach (string key in textKeys)
            { if (oldText.TryGetValue(key, out string value)) PlayerPrefs.SetString(key, value); else PlayerPrefs.DeleteKey(key); }
            foreach (string key in intKeys)
            { if (oldInts.TryGetValue(key, out int value)) PlayerPrefs.SetInt(key, value); else PlayerPrefs.DeleteKey(key); }
            PlayerPrefs.Save();
        }
        private CollectionSession StartSession()
        {
            root = new GameObject("Catalog migration test");
            var session = root.AddComponent<CollectionSession>(); session.Initialize(); return session;
        }

        [UnityTest]
        public IEnumerator JsonUtilityMigrationKeepsInventoryDailyAndInterruptedReveal()
        {
            int today = CollectionSession.Today;
            string old = "{\"Version\":1,\"Counts\":[0,17,3,9],\"Day\":" + today + ",\"Steps\":837,\"Claimed\":true,\"SensorTotal\":4421,\"SensorUptime\":2500,\"SensorBoot\":3,\"PendingReveal\":1}";
            PlayerPrefs.SetString(SaveKey, old);
            var session = StartSession();
            Assert.AreEqual(2, session.Progress.Save.Version);
            Assert.AreEqual(17, session.GetOwnedCount(ToyCatalog.MoonId));
            CollectionAssert.AreEqual(new[] { 0, 17, 3, 9 }, session.Progress.Save.Counts);
            Assert.AreEqual(ToyCatalog.MoonId, session.PendingRevealId);
            Assert.AreEqual(837, session.Progress.Save.Steps); Assert.IsTrue(session.Progress.Save.Claimed);
            Assert.AreEqual(old, PlayerPrefs.GetString(SaveKey + ".before-v2"));
            Assert.IsFalse(session.ClaimDaily(out _));
            Object.Destroy(root); yield return null;
            session = StartSession();
            Assert.AreEqual(17, session.GetOwnedCount(ToyCatalog.MoonId)); Assert.AreEqual(ToyCatalog.MoonId, session.PendingRevealId);
            Assert.IsFalse(session.FinishReveal(ToyCatalog.GoldId));
            Assert.IsTrue(session.FinishReveal(ToyCatalog.MoonId));
        }

        [UnityTest]
        public IEnumerator FutureIdsSurviveButUnknownSchemasAreNeverOverwritten()
        {
            var saved = new CollectionSave { Version = 2, Day = CollectionSession.Today, Inventory = new List<OwnedCollectible> {
                new OwnedCollectible("future.character.finish", 8), new OwnedCollectible(ToyCatalog.MoonId, 2) }, PendingRevealId = "future.character.finish" };
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(saved));
            var session = StartSession();
            Assert.AreEqual(8, session.GetOwnedCount("future.character.finish"));
            Assert.IsFalse(session.TryGetPlayableVariant("future.character.finish", out _));
            Assert.IsFalse(session.FinishReveal("future.character.finish"));
            Assert.AreEqual("future.character.finish", session.PendingRevealId);
            Object.Destroy(root); yield return null;
            var serialized = JsonUtility.FromJson<CollectionSave>(PlayerPrefs.GetString(SaveKey));
            Assert.IsTrue(serialized.Inventory.Exists(entry => entry.Id == "future.character.finish" && entry.Count == 8));
            string futureSchema = "{\"Version\":99,\"Counts\":[1,2,3,4],\"unknownField\":\"keep me\"}";
            PlayerPrefs.SetString(SaveKey, futureSchema);
            session = StartSession(); Assert.IsTrue(session.IsReadOnlySave); Assert.IsFalse(session.ClaimDaily(out _));
            Object.Destroy(root); yield return null;
            Assert.AreEqual(futureSchema, PlayerPrefs.GetString(SaveKey));
            string malformed = "{broken json"; PlayerPrefs.SetString(SaveKey, malformed);
            session = StartSession(); Assert.IsTrue(session.IsReadOnlySave);
            Assert.AreEqual(0, session.GetOwnedCount(ToyCatalog.PeachId), "Unreadable saved data should not grant fresh starter toys.");
            Object.Destroy(root); yield return null;
            Assert.AreEqual(malformed, PlayerPrefs.GetString(SaveKey));
        }

        [Test]
        public void ProfilePrefsMigrateAndFutureSelectionsSurviveNameChanges()
        {
            PlayerPrefs.SetInt("pockle.profile.local.avatar", 2); PlayerPrefs.SetInt("pockle.profile.local.favorite", 1);
            var profile = new GuestProfile();
            Assert.AreEqual(ToyCatalog.GoldId, profile.AvatarId); Assert.AreEqual(ToyCatalog.MoonId, profile.FavoriteId);
            Assert.AreEqual(ToyCatalog.MoonId, PlayerPrefs.GetString("pockle.profile.local.favoriteId"));
            PlayerPrefs.SetString("pockle.profile.local.favoriteId", "future.character.finish");
            profile = new GuestProfile(); profile.SetName("Collector 2");
            Assert.AreEqual("future.character.finish", PlayerPrefs.GetString("pockle.profile.local.favoriteId"));
            Assert.IsFalse(profile.SetFavorite("moss.velvet-flock")); Assert.AreEqual("future.character.finish", profile.FavoriteId);
            Assert.IsTrue(profile.SetFavorite(ToyCatalog.MintId));
            Assert.AreEqual(PipVariant.MintSoft, new GuestProfile().Favorite);
        }
    }
}
#endif
