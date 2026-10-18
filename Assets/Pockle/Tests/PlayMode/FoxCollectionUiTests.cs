using System.Collections;
using System.Linq;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Pockle.Tests
{
    public sealed class FoxCollectionUiTests
    {
        [UnityTest]
        public IEnumerator FoxCollectionIsReachableWithoutGrantingInventory()
        {
            const string saveKey = "pockle.collection.v1";
            bool hadSave = PlayerPrefs.HasKey(saveKey), hadBackup = PlayerPrefs.HasKey(saveKey + ".backup");
            string save = PlayerPrefs.GetString(saveKey), backup = PlayerPrefs.GetString(saveKey + ".backup");
            var root = new GameObject("Fox collection browser test");
            try
            {
                var session = root.AddComponent<CollectionSession>(); session.Initialize();
                var before = session.OwnedCounts.ToDictionary(p => p.Key, p => p.Value);
                var hud = root.AddComponent<PrototypeHud>();
                hud.Initialize(_ => { }, _ => { }, _ => { }, _ => { }); hud.Bind(session);
                yield return null;
                root.GetComponentsInChildren<Button>().Single(b => b.name == "Home collections · all").onClick.Invoke();
                Assert.That(hud.CurrentPage, Is.EqualTo(AppPage.Collections));
                root.GetComponentsInChildren<Button>().Single(b => b.name == "Collection · fox").onClick.Invoke();
                Assert.That(hud.CurrentPage, Is.EqualTo(AppPage.Collection));
                var slots = root.GetComponentsInChildren<Button>().Where(b => b.name.StartsWith("Collectible · fox.")).ToArray();
                Assert.That(slots.Length, Is.EqualTo(10));
                Assert.That(slots.Any(b => b.name == "Collectible · " + ToyCatalog.FoxId), Is.True);
                CollectionAssert.AreEquivalent(before, session.OwnedCounts, "Collection browsing granted a fox.");
            }
            finally { Object.Destroy(root); }
            yield return null;
            if (hadSave) PlayerPrefs.SetString(saveKey, save); else PlayerPrefs.DeleteKey(saveKey);
            if (hadBackup) PlayerPrefs.SetString(saveKey + ".backup", backup); else PlayerPrefs.DeleteKey(saveKey + ".backup");
            PlayerPrefs.Save();
        }
    }
}
