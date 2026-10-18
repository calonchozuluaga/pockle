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
    public sealed class FoxFinishesTests
    {
        [UnityTest]
        public IEnumerator AllFinishesLoadAndKeepTheUprightRigidMesh()
        {
            var root = new GameObject("Fox finish regression");
            try
            {
                var toy = root.AddComponent<JellyToy>();
                foreach (var item in ToyCatalog.Collectibles.Where(c => c.CharacterId == "fox"))
                {
                    Assert.That(CharacterArt.TryLoad(item.Id, out var asset), Is.True);
                    Assert.That(toy.TrySetCollectible(item.Id), Is.True);
                    Assert.That(toy.Handling, Is.SameAs(MaterialHandling.Firm));
                    var material = toy.BodyRenderer.sharedMaterial;
                    Assert.That(material.mainTexture, Is.SameAs(asset.BaseColorTexture));
                    Assert.That(material.shader.isSupported, Is.True);
                    if (item.Id != ToyCatalog.FoxId) Assert.That(material.shader.name, Is.EqualTo("Pockle/Fox Finish"));
                    var resting = toy.BodyRenderer.GetComponent<MeshFilter>().sharedMesh.vertices;
                    CollectionAssert.AreEqual(asset.BodyMesh.vertices, resting);
                    toy.SetDeformation(.42f,.6f,.8f,.8f,0,0,.5f,Vector3.up,.4f);
                    CollectionAssert.AreEqual(resting, toy.BodyRenderer.GetComponent<MeshFilter>().sharedMesh.vertices);
                    Assert.That(toy.VisualRoot.localRotation, Is.EqualTo(Quaternion.identity));
                    bool inclusions = item.Id == ToyCatalog.FoxAuroraId || item.Id == "fox.clear-jelly";
                    Assert.That(toy.VisualRoot.childCount, Is.EqualTo(inclusions ? 13 : 1));
                    if (inclusions) Assert.That(material.GetFloat("_Opacity"), Is.LessThan(.6f));
                }
                Assert.That(toy.TrySetCollectible(ToyCatalog.PeachId), Is.True);
                Assert.That(toy.TrySetCollectible(ToyCatalog.FoxAuroraId), Is.True);
                Assert.That(toy.BodyRenderer.sharedMaterial.GetFloat("_Iridescence"), Is.EqualTo(1));
            }
            finally { Object.Destroy(root); }
            yield return null;
        }

        [UnityTest]
        public IEnumerator FinishAwardPersistsBeforeRefreshAndSurvivesRestartWithoutDuplicates()
        {
            string[] keys = { "pockle.collection.v1", "pockle.collection.v1.backup", "pockle.collection.v1.before-v2" };
            var saved = new Dictionary<string,string>();
            foreach (string key in keys) if (PlayerPrefs.HasKey(key)) saved[key] = PlayerPrefs.GetString(key);
            bool hadMarker = PlayerPrefs.HasKey(CollectionSession.FoxFinishesGrantKey);
            int marker = PlayerPrefs.GetInt(CollectionSession.FoxFinishesGrantKey);
            var root = new GameObject("Fox finishes grant");
            try
            {
                PlayerPrefs.DeleteKey(CollectionSession.FoxFinishesGrantKey);
                PlayerPrefs.SetString(keys[0], JsonUtility.ToJson(new CollectionSave { Version = 2, Day = CollectionSession.Today,
                    Steps = 521, PendingRevealId = ToyCatalog.MoonId,
                    Inventory = new List<OwnedCollectible> { new OwnedCollectible(ToyCatalog.FoxId, 3), new OwnedCollectible(ToyCatalog.PeachId, 7) } }));
                var session = root.AddComponent<CollectionSession>(); session.Initialize();
                bool refreshed = false;
                session.Changed += () => {
                    var disk = JsonUtility.FromJson<CollectionSave>(PlayerPrefs.GetString(keys[0]));
                    Assert.That(disk.Inventory.Count(c => c.Id.StartsWith("fox.")), Is.EqualTo(10));
                    refreshed = true;
                };
                Assert.That(session.TryApplyOwnerFoxFinishesGrant(), Is.True);
                Assert.That(refreshed, Is.True);
                Assert.That(session.GetOwnedCount(ToyCatalog.FoxId), Is.EqualTo(3));
                Assert.That(session.GetOwnedCount(ToyCatalog.PeachId), Is.EqualTo(7));
                foreach (var item in ToyCatalog.Collectibles.Where(c => c.CharacterId == "fox"))
                    Assert.That(session.IsOwnedAndPlayable(item.Id), Is.True);
                Assert.That(session.Progress.Save.Steps, Is.EqualTo(521));
                Assert.That(session.Progress.Save.PendingRevealId, Is.EqualTo(ToyCatalog.MoonId));
                var hud = root.AddComponent<PrototypeHud>(); hud.Initialize(_ => { }, _ => { }, _ => { }, _ => { }); hud.Bind(session);
                foreach (var item in ToyCatalog.Collectibles.Where(c => c.CharacterId == "fox"))
                {
                    Assert.That(hud.ShowToy(item.Id), Is.True);
                    string title = item.FinishDisplayName.Split(new[] { " · " }, System.StringSplitOptions.None)[0];
                    Assert.That(root.GetComponentsInChildren<UnityEngine.UI.Text>().Any(t => t.text == title), Is.True, item.Id);
                }
                Assert.That(session.TryApplyOwnerFoxFinishesGrant(), Is.False);
                // A crash after inventory persist but before marker persist must also be harmless.
                PlayerPrefs.DeleteKey(CollectionSession.FoxFinishesGrantKey);
                Assert.That(session.TryApplyOwnerFoxFinishesGrant(), Is.False);
                var restored = new CollectionProgress(JsonUtility.FromJson<CollectionSave>(PlayerPrefs.GetString(keys[0])), CollectionSession.Today);
                Assert.That(restored.EnsureFoxFinishTestCopies(), Is.False);
                Assert.That(restored.GetOwnedCount(ToyCatalog.FoxId), Is.EqualTo(3));
            }
            finally
            {
                Object.Destroy(root);
                foreach (var key in keys) { if (saved.ContainsKey(key)) PlayerPrefs.SetString(key,saved[key]); else PlayerPrefs.DeleteKey(key); }
                if (hadMarker) PlayerPrefs.SetInt(CollectionSession.FoxFinishesGrantKey,marker); else PlayerPrefs.DeleteKey(CollectionSession.FoxFinishesGrantKey);
                PlayerPrefs.Save();
            }
            yield return null;
        }
    }
}
