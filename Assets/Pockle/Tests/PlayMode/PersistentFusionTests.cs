using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Pockle.Tests
{
    public sealed class PersistentFusionTests
    {
        private const string SaveKey = "pockle.collection.v1";
        private readonly Dictionary<string, string> previous = new Dictionary<string, string>();
        private GameObject root;
        private static readonly string[] Keys = { SaveKey, SaveKey + ".backup", SaveKey + ".before-v2", SaveKey + ".before-v3" };
        private static object Field(object target, string name) => target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
        private static void Call(object target, string name, params object[] args) => target.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, args);
        [SetUp] public void SetUp()
        {
            foreach (string key in Keys) { if (PlayerPrefs.HasKey(key)) previous[key] = PlayerPrefs.GetString(key); PlayerPrefs.DeleteKey(key); }
            var inventory = ToyCatalog.Collectibles.Where(t => t.Available).Select(t => new OwnedCollectible(t.Id, 3)).ToList();
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(new CollectionSave { Version = 2, Day = CollectionSession.Today,
                Steps = 731, Claimed = true, Inventory = inventory }));
        }
        [UnityTearDown] public IEnumerator TearDown()
        {
            if (root != null) UnityEngine.Object.Destroy(root); yield return null;
            foreach (string key in Keys) { if (previous.TryGetValue(key, out var value)) PlayerPrefs.SetString(key, value); else PlayerPrefs.DeleteKey(key); }
            previous.Clear(); PlayerPrefs.Save(); FusionCatalog.Restore(null);
        }
        [UnityTest] public IEnumerator EveryCharacterSavesPlayableBlendedMaterialAndIndependentPersonality()
        {
            root = new GameObject("Persistent fusion material test");
            var session = root.AddComponent<CollectionSession>(); session.Initialize();
            var service = new LocalFusionService(session);
            var toy = new GameObject("Fused toy").AddComponent<JellyToy>(); toy.transform.SetParent(root.transform);
            foreach (var collection in ToyCatalog.Collections.Where(c => c.Id == c.CharacterId && c.Members.Any(FusionCatalog.IsSource)))
            {
                string first = collection.Members.Single(id => ToyCatalog.TryGetCollectible(id, out var t) && t.FinishId == "gloss-vinyl");
                string second = collection.Members.Single(id => ToyCatalog.TryGetCollectible(id, out var t) && t.FinishId == "coated-metallic");
                Assert.IsTrue(service.TryFuse(second, first), collection.Id);
                string id = session.Progress.Save.PendingFusionId;
                Assert.IsTrue(FusionCatalog.TryGet(id, out var record));
                Assert.That(JsonUtility.FromJson<CollectionSave>(PlayerPrefs.GetString(SaveKey)).Fusions.Last().Id, Is.EqualTo(id));
                Assert.That(session.GetOwnedCount(first), Is.EqualTo(2)); Assert.That(session.GetOwnedCount(second), Is.EqualTo(2));
                Assert.IsFalse(service.TryFuse(first, second), "Unacknowledged reveal blocks duplicate input.");
                Assert.IsTrue(toy.TrySetCollectible(id)); Assert.AreEqual(id, toy.CollectibleId);
                var material = toy.BodyRenderer.sharedMaterial;
                Assert.That(material.GetFloat("_Metallic"), Is.EqualTo(.5f).Within(.01f));
                Assert.That(PersonalityBook.Dials(id).Energy, Is.EqualTo(record.Energy));
                Assert.That((int)PersonalityBook.TemperamentOf(id), Is.EqualTo(record.Temperament));
                var face = toy.GetComponentInChildren<ToyTemperamentDriver>();
                if (face != null) Assert.IsFalse(face.CycleEnabled, "Saved fusion expressions use inherited temperament.");
                CaptureNativeFusion(id, collection.Id);
                Assert.IsTrue(session.FinishFusion(id)); yield return null;
            }
            Assert.That(session.Progress.Save.Steps, Is.EqualTo(731)); Assert.IsTrue(session.Progress.Save.Claimed);
            Assert.That(PlayerPrefs.GetString(SaveKey + ".before-v3"), Does.Contain("\"Version\":2"));
        }
        [UnityTest] public IEnumerator ApprovedPipPairsKeepTheirReviewedMaterialsAndContents()
        {
            root = new GameObject("Approved Pip recipe test");
            var session = root.AddComponent<CollectionSession>(); session.Initialize(); var service = new LocalFusionService(session);
            var toy = new GameObject("Pip recipe toy").AddComponent<JellyToy>(); toy.transform.SetParent(root.transform);
            string[] first = { ToyCatalog.PeachId, ToyCatalog.PeachId, ToyCatalog.GoldId, ToyCatalog.MoonId };
            string[] second = { ToyCatalog.MoonId, ToyCatalog.MintId, ToyCatalog.MintId, ToyCatalog.GoldId };
            string[] appearance = { PipFusionStudies.DuskId, PipFusionStudies.SorbetId, PipFusionStudies.HoneydewId, PipFusionStudies.EclipseId };
            for (int i = 0; i < first.Length; i++)
            {
                Assert.IsTrue(service.TryFuse(first[i], second[i])); string id = session.Progress.Save.PendingFusionId;
                Assert.AreEqual(appearance[i], FusionMaterial.AppearanceId(id)); Assert.IsTrue(toy.TrySetCollectible(id));
                Assert.That(toy.BodyRenderer.sharedMaterial.GetFloat("_FusionMode"), Is.EqualTo(i + 1));
                if (i == 0) Assert.That(toy.GetComponentsInChildren<Transform>().Count(t => t.name.StartsWith("Dusk blue star")), Is.EqualTo(4));
                if (i == 3) {
                    Assert.That(toy.GetComponentsInChildren<Transform>().Count(t => t.name.StartsWith("Eclipse gold star")), Is.EqualTo(4));
                    Assert.That(toy.BodyRenderer.sharedMaterial.GetFloat("_Glow"), Is.GreaterThan(.5f));
                }
                Assert.IsTrue(session.FinishFusion(id)); yield return null;
            }
            foreach (string id in new[] { ToyCatalog.PeachId, ToyCatalog.MoonId, ToyCatalog.GoldId, ToyCatalog.MintId })
                Assert.That(session.GetOwnedCount(id), Is.EqualTo(1), "Four creations must retain every parent's last copy.");
        }

        private static void CaptureNativeFusion(string id, string character)
        {
            var target = (RenderTexture)typeof(JellyToy).Assembly.GetType("Pockle.Runtime.ToyPortrait").GetMethod("Render", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(string), typeof(Color) }, null).Invoke(null, new object[] { id, new Color(.96f, .94f, .92f) });
            var old = RenderTexture.active; var image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            try
            {
                RenderTexture.active = target; image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); image.Apply();
                var pixels = image.GetPixels32(); var background = pixels[0];
                Assert.That(pixels.Count(c => Math.Abs(c.r - background.r) + Math.Abs(c.g - background.g) + Math.Abs(c.b - background.b) > 35),
                    Is.GreaterThan(1000), "The persisted toy must actually render, not only resolve to a definition.");
                string folder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.dataPath), "test-results", "persistent-fusion");
                System.IO.Directory.CreateDirectory(folder); System.IO.File.WriteAllBytes(System.IO.Path.Combine(folder, character + ".png"), image.EncodeToPNG());
            }
            finally { RenderTexture.active = old; target.Release(); UnityEngine.Object.Destroy(target); UnityEngine.Object.Destroy(image); }
        }

        [UnityTest] public IEnumerator FailedWriteKeepsParentsAndCreatesNoReceipt()
        {
            root = new GameObject("Failed fusion write"); var session = root.AddComponent<CollectionSession>(); session.Initialize();
            string before = PlayerPrefs.GetString(SaveKey);
            var record = new FusedToy { Id = "fusion.failure-test", First = ToyCatalog.PeachId, Second = ToyCatalog.MoonId,
                Name = "Dusk Pip", Material = "Pearl jelly", Temperament = 7 };
            Action<string> fail = _ => throw new System.IO.IOException("Test write failure");
            LogAssert.Expect(LogType.Warning, "Fusion was not committed: IOException");
            bool committed = (bool)typeof(CollectionSession).GetMethod("CommitFusion", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(session, new object[] { record, fail });
            Assert.IsFalse(committed); Assert.AreEqual(before, PlayerPrefs.GetString(SaveKey));
            Assert.That(session.GetOwnedCount(ToyCatalog.PeachId), Is.EqualTo(3)); Assert.IsFalse(FusionCatalog.TryGet(record.Id, out _));
            yield return null;
        }
        [UnityTest] public IEnumerator HudCommitsBeforeRevealRecoversAfterRestartAndPlaysSavedToy()
        {
            var state = JsonUtility.FromJson<CollectionSave>(PlayerPrefs.GetString(SaveKey));
            foreach (var item in state.Inventory) if (item.Id != ToyCatalog.FoxId && item.Id != "fox.coated-metallic") item.Count = 1;
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(state));
            root = new GameObject("Persistent fusion runtime"); var runtime = root.AddComponent<TactilePrototype>();
            yield return null; yield return null;
            var hud = (PrototypeHud)Field(runtime, "hud"); var session = root.GetComponent<CollectionSession>();
            hud.SetSettings(true, false, true); Call(hud, "Navigate", AppPage.Fusion);
            Call(hud, "PickSpare", ToyCatalog.FoxId); Call(hud, "PickSpare", "pip.coated-metallic");
            Assert.IsNull(Field(hud, "fuseSecond"), "Mixed characters must never fill the second slot.");
            Call(hud, "PickSpare", "fox.coated-metallic"); Call(hud, "Fuse");
            Assert.That(hud.CurrentPage, Is.EqualTo(AppPage.FusionReveal));
            string id = session.Progress.Save.PendingFusionId;
            Assert.That(session.GetOwnedCount(id), Is.EqualTo(1)); Assert.That(session.GetOwnedCount(ToyCatalog.FoxId), Is.EqualTo(2));
            var committed = JsonUtility.FromJson<CollectionSave>(PlayerPrefs.GetString(SaveKey));
            Assert.That(committed.Fusions.Single().Id, Is.EqualTo(id));
            UnityEngine.Object.Destroy(root); root = null; yield return null;
            root = new GameObject("Restarted fusion runtime"); runtime = root.AddComponent<TactilePrototype>(); yield return null; yield return null;
            hud = (PrototypeHud)Field(runtime, "hud"); session = root.GetComponent<CollectionSession>();
            Assert.That(hud.CurrentPage, Is.EqualTo(AppPage.FusionReveal), "Interrupted reveals recover the committed toy.");
            Assert.That(session.GetOwnedCount(ToyCatalog.FoxId), Is.EqualTo(2));
            var add = hud.GetComponentsInChildren<Button>().Single(b => b.GetComponentInChildren<Text>()?.text == "View on shelf"); add.onClick.Invoke();
            yield return null; Canvas.ForceUpdateCanvases();
            var saved = hud.GetComponentsInChildren<Button>().Single(b => b.name == "Saved fusion · " + id); saved.onClick.Invoke();
            Assert.That(hud.CurrentPage, Is.EqualTo(AppPage.Play));
            Assert.That(((JellyToy)Field(runtime, "toy")).CollectibleId, Is.EqualTo(id));
            Assert.That(session.Progress.Save.PendingFusionId, Is.Empty);
            var candidates = (List<string>)typeof(PrototypeHud).GetMethod("ShelfCandidates", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(hud, null);
            CollectionAssert.Contains(candidates, id, "Saved fusions can join the live shelf and its social moments.");
        }
    }
}
