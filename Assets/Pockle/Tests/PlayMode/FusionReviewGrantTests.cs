using System.Collections;
using System.Linq;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
    public sealed class FusionReviewGrantTests
    {
        [UnityTest]
        public IEnumerator SparesPersistOnceAndDoNotRefillAfterRestartOrTouchFutureSaves()
        {
            const string saveKey = "pockle.collection.v1";
            string[] keys = { saveKey, saveKey + ".backup", CollectionSession.FusionReviewSparesKey };
            var had = keys.Select(PlayerPrefs.HasKey).ToArray();
            string oldSave = PlayerPrefs.GetString(saveKey), oldBackup = PlayerPrefs.GetString(keys[1]);
            int oldMarker = PlayerPrefs.GetInt(keys[2]);
            GameObject root = null;
            try
            {
                var save = new CollectionProgress(null, CollectionSession.Today).Save;
                save.Steps = 571; save.PendingRevealId = ToyCatalog.MoonId;
                PlayerPrefs.SetString(saveKey, JsonUtility.ToJson(save)); PlayerPrefs.DeleteKey(keys[2]);
                root = new GameObject("Fusion review grant");
                var session = root.AddComponent<CollectionSession>(); session.Initialize();
                bool changed = false;
                session.Changed += () => {
                    var persisted = JsonUtility.FromJson<CollectionSave>(PlayerPrefs.GetString(saveKey));
                    Assert.That(persisted.Inventory.Count(i => i.Count >= 3), Is.EqualTo(18));
                    changed = true;
                };
                Assert.IsTrue(session.TryApplyOwnerFusionReviewSpares()); Assert.IsTrue(changed);
                Assert.That(PlayerPrefs.GetInt(keys[2]), Is.EqualTo(1));
                Object.DestroyImmediate(root); root = null;
                // Simulate using spare copies, then restart: the one-time grant must never refill them.
                save = JsonUtility.FromJson<CollectionSave>(PlayerPrefs.GetString(saveKey));
                save.Inventory.Single(i => i.Id == ToyCatalog.FoxId).Count = 1;
                PlayerPrefs.SetString(saveKey, JsonUtility.ToJson(save));
                root = new GameObject("Restarted review"); session = root.AddComponent<CollectionSession>(); session.Initialize();
                Assert.IsFalse(session.TryApplyOwnerFusionReviewSpares());
                Assert.That(session.GetOwnedCount(ToyCatalog.FoxId), Is.EqualTo(1));
                Assert.That(session.Progress.Save.Steps, Is.EqualTo(571)); Assert.That(session.PendingRevealId, Is.EqualTo(ToyCatalog.MoonId));
                Object.DestroyImmediate(root); root = null;
                save.Version = CollectionProgress.SaveVersion + 1;
                string future = JsonUtility.ToJson(save); PlayerPrefs.SetString(saveKey, future); PlayerPrefs.DeleteKey(keys[2]);
                root = new GameObject("Future save review"); session = root.AddComponent<CollectionSession>(); session.Initialize();
                Assert.IsTrue(session.IsReadOnlySave); Assert.IsFalse(session.TryApplyOwnerFusionReviewSpares());
                Assert.That(PlayerPrefs.GetString(saveKey), Is.EqualTo(future)); Assert.IsFalse(PlayerPrefs.HasKey(keys[2]));
            }
            finally
            {
                if (root != null) Object.DestroyImmediate(root);
                if (had[0]) PlayerPrefs.SetString(saveKey, oldSave); else PlayerPrefs.DeleteKey(saveKey);
                if (had[1]) PlayerPrefs.SetString(keys[1], oldBackup); else PlayerPrefs.DeleteKey(keys[1]);
                if (had[2]) PlayerPrefs.SetInt(keys[2], oldMarker); else PlayerPrefs.DeleteKey(keys[2]);
                PlayerPrefs.Save();
            }
            yield return null;
        }
    }
}
