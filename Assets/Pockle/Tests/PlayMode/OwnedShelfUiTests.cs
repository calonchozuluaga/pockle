#if UNITY_EDITOR
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
    public sealed class OwnedShelfUiTests
    {
        private const string SaveKey = "pockle.collection.v1", FusionId = "fusion.owned-shelf";
        private readonly Dictionary<string, string> oldStrings = new Dictionary<string, string>();
        private readonly Dictionary<string, int> oldInts = new Dictionary<string, int>();
        private readonly List<string> stringKeys = new List<string>();
        private readonly string[] intKeys = { StageLook.LightKey, StageLook.WorldKey, "pockle.profile.local.avatar", "pockle.profile.local.favorite" };
        private GameObject root;
        private CollectionSession session;
        private PrototypeHud hud;
        private string unownedId;

        [SetUp]
        public void SetUp()
        {
            stringKeys.Clear(); oldStrings.Clear(); oldInts.Clear();
            stringKeys.AddRange(new[] { SaveKey, SaveKey + ".backup", SaveKey + ".before-v2", SaveKey + ".before-v3",
                SaveKey + ".before-v4", SaveKey + ".before-v5", SaveKey + ".before-v6",
                "pockle.profile.local.name", "pockle.profile.local.avatarId", "pockle.profile.local.favoriteId",
                PrototypeHud.MomentsKey, PrototypeHud.LiveShelfKey, PrototypeHud.LiveShelfSeenKey,
                LocalActivityFeed.LogKey, ShelfFriendships.Key, BadgeCabinet.Key });
            stringKeys.AddRange(ToyCatalog.Collections.Select(c => "pockle.stage.environment.v1." + c.Id));
            foreach (string key in stringKeys) { if (PlayerPrefs.HasKey(key)) oldStrings[key] = PlayerPrefs.GetString(key); PlayerPrefs.DeleteKey(key); }
            foreach (string key in intKeys) { if (PlayerPrefs.HasKey(key)) oldInts[key] = PlayerPrefs.GetInt(key); PlayerPrefs.DeleteKey(key); }
            ShelfFriendships.Reload();
            var ids = ToyCatalog.Collections.Where(c => c.Id == c.CharacterId).SelectMany(c => c.Members.Take(2)).ToList();
            if (!ids.Contains(ToyCatalog.PeachId)) ids.Add(ToyCatalog.PeachId);
            if (!ids.Contains(ToyCatalog.GoldId)) ids.Add(ToyCatalog.GoldId);
            unownedId = ToyCatalog.Collectibles.First(t => t.Available && !ids.Contains(t.Id)).Id;
            var inventory = ids.Distinct().Select(id => new OwnedCollectible(id, 2)).ToList();
            inventory.Add(new OwnedCollectible(unownedId, 0));
            inventory.Add(new OwnedCollectible("future.unplayable", 1));
            inventory.Add(new OwnedCollectible(FusionId, 1));
            var fusion = new FusedToy { Id = FusionId, First = ToyCatalog.PeachId, Second = ToyCatalog.MoonId,
                Name = "Shelf fusion", Material = "Jelly", Temperament = 2 };
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(new CollectionSave { Version = CollectionProgress.SaveVersion,
                Day = CollectionSession.Today, Inventory = inventory, Fusions = new List<FusedToy> { fusion } }));
        }

        private void BuildHud()
        {
            root = new GameObject("Owned shelf UI test");
            session = root.AddComponent<CollectionSession>(); session.Initialize();
            hud = root.AddComponent<PrototypeHud>();
            hud.Initialize(_ => { }, _ => { }, _ => { }); hud.Bind(session); hud.SetSettings(true, false, true);
            root.GetComponentsInChildren<Button>().First(b => b.GetComponentInChildren<Text>()?.text == "Shelf").onClick.Invoke();
            Canvas.ForceUpdateCanvases();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (root != null) Object.Destroy(root);
            yield return null;
            foreach (string key in stringKeys) { if (oldStrings.TryGetValue(key, out var value)) PlayerPrefs.SetString(key, value); else PlayerPrefs.DeleteKey(key); }
            foreach (string key in intKeys) { if (oldInts.TryGetValue(key, out int value)) PlayerPrefs.SetInt(key, value); else PlayerPrefs.DeleteKey(key); }
            PlayerPrefs.Save(); ShelfFriendships.Reload(); FusionCatalog.Restore(null);
        }

        [UnityTest]
        public IEnumerator OwnedGridIncludesEveryPlayableCollectionAndFusionWithExactExtraCounts()
        {
            BuildHud(); yield return null;
            string[] expected = session.OwnedCounts.Where(p => p.Value > 0 && session.IsOwnedAndPlayable(p.Key)).Select(p => p.Key).ToArray();
            CollectionAssert.AreEquivalent(expected, VisibleIds());
            Assert.That(VisibleIds(), Does.Contain(FusionId));
            Assert.That(VisibleIds(), Does.Not.Contain(unownedId));
            Assert.That(VisibleIds(), Does.Not.Contain("future.unplayable"));
            Assert.AreEqual(11, expected.Where(id => id != FusionId).Select(id => { ToyCatalog.TryGetCollectible(id, out var toy); return toy.CharacterId; }).Distinct().Count());
            foreach (string id in expected)
            {
                string count = ButtonNamed("Owned toy · " + id).GetComponentsInChildren<Text>().Single(t => t.transform.parent.name == "Copies").text;
                Assert.AreEqual(id == FusionId ? "Fused" : "+1 extra", count, "The first owned copy is not an extra.");
            }
            Assert.AreEqual(0, hud.ShelfPreviews.ActiveCount, "The owned grid must not allocate live actors per finish.");
            Assert.IsFalse(root.GetComponentsInChildren<RectTransform>(true).Single(t => t.name == "Living shelf").gameObject.activeInHierarchy);
        }

        [UnityTest]
        public IEnumerator FiltersAndSearchComposeWithoutChangingOwnershipAndTapPlaysImmediately()
        {
            BuildHud(); yield return null;
            var before = session.OwnedCounts.ToDictionary(p => p.Key, p => p.Value);
            ButtonNamed("Owned shelf filter · Fusions").onClick.Invoke();
            CollectionAssert.AreEqual(new[] { FusionId }, VisibleIds());
            ButtonNamed("Owned shelf filter · Extras").onClick.Invoke();
            CollectionAssert.AreEquivalent(before.Where(p => p.Value > 1 && session.IsOwnedAndPlayable(p.Key)).Select(p => p.Key), VisibleIds());
            var search = root.GetComponentsInChildren<InputField>().Single(f => f.name == "Search your shelf");
            search.text = "  FOX  ";
            Assert.IsTrue(VisibleIds().Length > 0); Assert.IsTrue(VisibleIds().All(id => id.StartsWith("fox.")));
            search.text = "no such toy"; Assert.IsEmpty(VisibleIds());
            search.text = ""; ButtonNamed("Owned shelf filter · All owned").onClick.Invoke();
            var changes = new List<string>(); hud.CollectibleChanged += changes.Add;
            ButtonNamed("Owned toy · " + FusionId).onClick.Invoke(); yield return null;
            Assert.AreEqual(AppPage.Play, hud.CurrentPage); Assert.AreEqual(FusionId, changes.Last());
            CollectionAssert.AreEquivalent(before, session.OwnedCounts);
            Assert.AreEqual(1, session.Progress.Save.Fusions.Count);
            hud.GoBack(); Assert.AreEqual(AppPage.Shelf, hud.CurrentPage);
            CollectionAssert.AreEquivalent(before.Where(p => p.Value > 0 && session.IsOwnedAndPlayable(p.Key)).Select(p => p.Key), VisibleIds());
            var saved = JsonUtility.FromJson<CollectionSave>(PlayerPrefs.GetString(SaveKey));
            foreach (var pair in before) Assert.AreEqual(pair.Value, saved.Inventory.Single(p => p.Id == pair.Key).Count);
        }

        [UnityTest]
        public IEnumerator DiscoveryAndLiveShelfAreSecondaryRoutesWithoutInventoryGrants()
        {
            BuildHud(); yield return null;
            var before = session.OwnedCounts.ToDictionary(p => p.Key, p => p.Value);
            ButtonNamed("Owned shelf · Walk").onClick.Invoke(); Assert.AreEqual(AppPage.Rewards, hud.CurrentPage);
            root.GetComponentsInChildren<Button>().First(b => b.GetComponentInChildren<Text>()?.text == "Shelf").onClick.Invoke();
            ButtonNamed("Owned shelf · Shop").onClick.Invoke(); Assert.AreEqual(AppPage.Boxes, hud.CurrentPage);
            root.GetComponentsInChildren<Button>().First(b => b.GetComponentInChildren<Text>()?.text == "Shelf").onClick.Invoke();
            ButtonNamed("Owned shelf · Fuse").onClick.Invoke(); Assert.AreEqual(AppPage.Fusion, hud.CurrentPage);
            hud.GoBack(); Assert.AreEqual(AppPage.Shelf, hud.CurrentPage);
            ButtonNamed("Owned shelf · Live shelf").onClick.Invoke();
            Assert.IsEmpty(VisibleIds()); Assert.IsNotNull(ButtonNamed("Edit shelf"));
            ButtonNamed("Live shelf · All my Pockles").onClick.Invoke(); Assert.IsNotEmpty(VisibleIds());
            ButtonNamed("Owned shelf · Live shelf").onClick.Invoke();
            root.GetComponentsInChildren<Button>().First(b => b.GetComponentInChildren<Text>()?.text == "Shelf").onClick.Invoke();
            Assert.IsNotEmpty(VisibleIds(), "The Shelf tab resets to the owned grid even when already viewing the living scene.");
            ButtonNamed("All collections").onClick.Invoke(); Assert.AreEqual(AppPage.Collections, hud.CurrentPage);
            CollectionAssert.AreEquivalent(before, session.OwnedCounts);
        }

        [UnityTest]
        public IEnumerator SpendingSparesUpdatesExtrasImmediatelyAndProtectsTheFirstCopies()
        {
            BuildHud(); yield return null;
            ButtonNamed("Owned shelf filter · Extras").onClick.Invoke();
            CollectionAssert.Contains(VisibleIds(), ToyCatalog.PeachId);
            CollectionAssert.Contains(VisibleIds(), ToyCatalog.GoldId);
            var service = new LocalFusionService(session);
            Assert.IsTrue(service.TryFuse(ToyCatalog.PeachId, ToyCatalog.GoldId)); yield return null;
            var result = session.Progress.Save.Fusions.Last();
            Assert.AreEqual(1, session.GetOwnedCount(ToyCatalog.PeachId)); Assert.AreEqual(1, session.GetOwnedCount(ToyCatalog.GoldId));
            CollectionAssert.DoesNotContain(VisibleIds(), ToyCatalog.PeachId);
            CollectionAssert.DoesNotContain(VisibleIds(), ToyCatalog.GoldId);
            ButtonNamed("Owned shelf filter · All owned").onClick.Invoke();
            foreach (string id in new[] { ToyCatalog.PeachId, ToyCatalog.GoldId, result.Id }) CollectionAssert.Contains(VisibleIds(), id);
            ButtonNamed("Owned shelf filter · Fusions").onClick.Invoke();
            CollectionAssert.AreEquivalent(new[] { FusionId, result.Id }, VisibleIds());
            Assert.IsFalse(service.TryFuse(ToyCatalog.PeachId, ToyCatalog.GoldId), "Spent spares cannot be reused or take the protected first copies.");
            Assert.AreEqual(1, session.GetOwnedCount(result.Id));
        }

        [UnityTest]
        public IEnumerator HomeFeatureChooserUsesOwnedToysAndPreservesInventory()
        {
            BuildHud();
            root.GetComponentsInChildren<Button>(true).Single(b=>b.GetComponentInChildren<Text>()?.text=="Home").onClick.Invoke();
            yield return null;
            var before=session.OwnedCounts.ToDictionary(p=>p.Key,p=>p.Value);
            string selected=before.Keys.First(id=>id.StartsWith("fox.")&&session.IsOwnedAndPlayable(id));
            var choose=root.GetComponentsInChildren<Button>(true).Single(b=>b.name=="Change featured toy");choose.onClick.Invoke();yield return null;
            Assert.That(hud.CurrentPage,Is.EqualTo(AppPage.Shelf));
            root.GetComponentsInChildren<Button>(true).Single(b=>b.name=="Owned toy · "+selected).onClick.Invoke();yield return null;
            Assert.That(hud.CurrentPage,Is.EqualTo(AppPage.Home));
            Assert.That(new GuestProfile().FavoriteId,Is.EqualTo(selected));
            Assert.That(root.GetComponentsInChildren<Text>(true).Any(t=>t.text=="Play with "+PersonalityBook.ShortName(selected)),Is.True);
            CollectionAssert.AreEquivalent(before,session.OwnedCounts);
        }
        [UnityTest]
        public IEnumerator LargeRosterRendersOnlyVisibleStillPortraitsAndKeepsTheLastRowReachable()
        {
            var save = JsonUtility.FromJson<CollectionSave>(PlayerPrefs.GetString(SaveKey));
            save.Inventory = ToyCatalog.Collections.Where(c => c.Id == c.CharacterId).SelectMany(c => c.Members).Take(125)
                .Select(id => new OwnedCollectible(id, 2)).ToList();
            save.Inventory.Add(new OwnedCollectible(FusionId, 1));
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(save));
            BuildHud(); yield return null; Canvas.ForceUpdateCanvases();
            Assert.That(VisibleIds().Length, Is.GreaterThan(120));
            var pictures = root.GetComponentsInChildren<RawImage>().Where(p => p.name == "Owned portrait").ToArray();
            Assert.That(pictures.Count(p => p.texture != null), Is.LessThan(20), "No eager render of 120+ meshes on shelf entry.");
            var scroll = root.GetComponentsInChildren<ScrollRect>().Single();
            scroll.verticalNormalizedPosition = 0; yield return null; Canvas.ForceUpdateCanvases();
            var fuse = ButtonNamed("Owned shelf · Fuse").GetComponent<RectTransform>();
            var corners = new Vector3[4]; fuse.GetWorldCorners(corners);
            Assert.GreaterOrEqual(scroll.viewport.InverseTransformPoint(corners[0]).y, scroll.viewport.rect.yMin - 1,
                "The last actions must scroll fully above the fixed navbar.");
            Assert.AreEqual(0, hud.ShelfPreviews.ActiveCount);
        }

        private string[] VisibleIds() => root.GetComponentsInChildren<Button>().Where(b => b.name.StartsWith("Owned toy · ")).Select(b => b.name.Substring("Owned toy · ".Length)).ToArray();
        private Button ButtonNamed(string name) => root.GetComponentsInChildren<Button>().Single(b => b.name == name);
    }
}
#endif
