using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Pockle.Tests
{
    public sealed class FoxOwnerGrantTests
    {
        private readonly string[] keys = { "pockle.collection.v1", "pockle.collection.v1.backup", "pockle.collection.v1.before-v2", CollectionSession.FoxTestGrantKey };
        private readonly Dictionary<string, string> saved = new Dictionary<string, string>();
        private GameObject root;
        [SetUp]
        public void Setup()
        {
            foreach (var key in keys) if (PlayerPrefs.HasKey(key)) saved[key] = key == CollectionSession.FoxTestGrantKey ? PlayerPrefs.GetInt(key).ToString() : PlayerPrefs.GetString(key);
            PlayerPrefs.DeleteKey(CollectionSession.FoxTestGrantKey);
            var inventory = new List<OwnedCollectible>();
            for (int i = 0; i < 4; i++) inventory.Add(new OwnedCollectible(ToyCatalog.LegacyCollectibleId(i), i + 1));
            PlayerPrefs.SetString(keys[0], JsonUtility.ToJson(new CollectionSave { Version = 2, Inventory = inventory, Day = CollectionSession.Today, Steps = 521 }));
            root = new GameObject("Owner Fox grant test");
        }
        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            Object.Destroy(root); yield return null;
            foreach (var key in keys)
            {
                if (!saved.ContainsKey(key)) PlayerPrefs.DeleteKey(key);
                else if (key == CollectionSession.FoxTestGrantKey) PlayerPrefs.SetInt(key, int.Parse(saved[key]));
                else PlayerPrefs.SetString(key, saved[key]);
            }
            saved.Clear(); PlayerPrefs.Save();
        }
        [UnityTest]
        public IEnumerator FoxGrantPersistsOnceAndOwnedCollectionTileOpensItsId()
        {
            var session = root.AddComponent<CollectionSession>(); session.Initialize();
            Assert.That(session.TryApplyOwnerFoxTestGrant(), Is.True);
            Assert.That(session.GetOwnedCount(ToyCatalog.FoxId), Is.EqualTo(1));
            Assert.That(session.TryApplyOwnerFoxTestGrant(), Is.False);
            var persisted = JsonUtility.FromJson<CollectionSave>(PlayerPrefs.GetString(keys[0]));
            Assert.That(persisted.Inventory.Single(c => c.Id == ToyCatalog.FoxId).Count, Is.EqualTo(1));
            Assert.That(persisted.Steps, Is.EqualTo(521));
            for (int i = 0; i < 4; i++) Assert.That(session.GetOwnedCount(ToyCatalog.LegacyCollectibleId(i)), Is.EqualTo(i + 1));
            var hud = root.AddComponent<PrototypeHud>(); hud.Initialize(_ => { }, _ => { }, _ => { }, _ => { }); hud.Bind(session);
            string selected = null; hud.CollectibleChanged += id => selected = id;
            yield return null;
            root.GetComponentsInChildren<Button>().Single(b => b.name == "Browse collections").onClick.Invoke();
            root.GetComponentsInChildren<Button>().Single(b => b.name == "Collection · fox").onClick.Invoke();
            var tile = root.GetComponentsInChildren<Button>().Single(b => b.name == "Collectible · " + ToyCatalog.FoxId);
            Assert.That(tile.interactable, Is.True);
            tile.onClick.Invoke(); Assert.That(hud.CurrentPage, Is.EqualTo(AppPage.Play));
            Assert.That(selected, Is.EqualTo(ToyCatalog.FoxId));
            Assert.That(root.GetComponentsInChildren<Text>().Any(t => t.text == "Fox · Gloss Vinyl"), Is.True);
            Assert.That(hud.ShowToy("fox.clear-jelly"), Is.False);
        }
    }
}
