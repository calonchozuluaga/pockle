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
    public sealed class PlayControlsUiTests
    {
        private const string SaveKey = "pockle.collection.v1", FusionId = "fusion.play-picker";
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
                Name = "Picker fusion", Material = "Jelly", Temperament = 2 };
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(new CollectionSave { Version = CollectionProgress.SaveVersion,
                Day = CollectionSession.Today, Inventory = inventory, Fusions = new List<FusedToy> { fusion } }));
        }

        private void BuildHud()
        {
            root = new GameObject("Play controls UI test");
            session = root.AddComponent<CollectionSession>(); session.Initialize();
            hud = root.AddComponent<PrototypeHud>();
            hud.Initialize(_ => { }, _ => { }, _ => { }); hud.Bind(session); hud.SetSettings(true, false, true);
            Assert.IsTrue(hud.ShowToy(ToyCatalog.PeachId));
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
        public IEnumerator LabeledControlsOpenCompactPickerForAllOwnedCollectionsAndSavedFusions()
        {
            BuildHud(); yield return null;
            foreach (string label in new[] { "Pockles", "Backdrop", "Light", "About" })
            {
                var button = ButtonNamed("Stage tab · " + label);
                var text = button.GetComponentInChildren<Text>();
                Assert.IsTrue(text.enabled, label + " is a visible label, not an icon-only control.");
                Assert.AreEqual(label, text.text);
            }
            Assert.IsFalse(Tray.gameObject.activeInHierarchy, "No picker wall is permanently displayed over the toy.");
            ButtonNamed("Stage tab · Pockles").onClick.Invoke();
            yield return null; Canvas.ForceUpdateCanvases();
            string[] expected = session.OwnedCounts.Where(p => p.Value > 0 && session.IsOwnedAndPlayable(p.Key)).Select(p => p.Key).ToArray();
            var buttons = root.GetComponentsInChildren<Button>().Where(b => b.name.StartsWith("Stage pockle · ")).ToArray();
            CollectionAssert.AreEquivalent(expected, buttons.Select(b => b.name.Substring("Stage pockle · ".Length)));
            Assert.That(expected, Does.Contain(FusionId));
            Assert.That(expected, Does.Not.Contain(unownedId));
            Assert.That(expected, Does.Not.Contain("future.unplayable"));
            Assert.That(expected.Where(id => id != FusionId).Select(id => { ToyCatalog.TryGetCollectible(id, out var toy); return toy.CollectionId; }).Distinct().Count(), Is.EqualTo(11));
            var scroll = root.GetComponentsInChildren<ScrollRect>().Single(s => s.name == "Pockle picker");
            Assert.IsTrue(scroll.vertical); Assert.IsFalse(scroll.horizontal);
            Assert.LessOrEqual(scroll.viewport.rect.height, 244, "The optional sheet remains compact.");
            Assert.Less(buttons.Count(b => b.GetComponentInChildren<RawImage>(true).texture != null), buttons.Length,
                "Opening the picker does not synchronously render portraits for the entire roster.");
        }

        [UnityTest]
        public IEnumerator OneTapSwitchesCollectionOrSavedFusionAndClosesWithoutGrantingCopies()
        {
            BuildHud(); yield return null;
            var before = session.OwnedCounts.ToDictionary(p => p.Key, p => p.Value);
            var changes = new List<string>(); hud.CollectibleChanged += changes.Add;
            string fox = before.Keys.First(id => id.StartsWith("fox.") && session.IsOwnedAndPlayable(id));
            foreach (string target in new[] { fox, FusionId })
            {
                ButtonNamed("Stage tab · Pockles").onClick.Invoke();
                ButtonNamed("Stage pockle · " + target).onClick.Invoke();
                yield return null;
                Assert.AreEqual(AppPage.Play, hud.CurrentPage);
                Assert.AreEqual(target, changes.Last());
                Assert.IsFalse(Tray.gameObject.activeInHierarchy, "One selection returns straight to play.");
                CollectionAssert.AreEquivalent(before, session.OwnedCounts);
                Assert.AreEqual(1, session.Progress.Save.Fusions.Count, "Selecting a fusion does not create another receipt.");
            }
            var persisted = JsonUtility.FromJson<CollectionSave>(PlayerPrefs.GetString(SaveKey));
            foreach (var pair in before) Assert.AreEqual(pair.Value, persisted.Inventory.Single(p => p.Id == pair.Key).Count);
        }

        [UnityTest]
        public IEnumerator LightSheetOffersDaylightAndNightAndPreservesNightValue()
        {
            BuildHud(); yield return null;
            var changes = new List<StageLight>(); hud.LightChanged += changes.Add;
            ButtonNamed("Stage tab · Light").onClick.Invoke();
            var buttons = root.GetComponentsInChildren<Button>().Where(b => b.name.StartsWith("Stage light · ")).ToArray();
            CollectionAssert.AreEquivalent(new[] { "Daylight", "Night" }, buttons.Select(b => b.GetComponentInChildren<Text>().text));
            ButtonNamed("Stage light · Night").onClick.Invoke();
            Assert.AreEqual(StageLight.Night, hud.CurrentLight); Assert.AreEqual(2, PlayerPrefs.GetInt(StageLook.LightKey));
            ButtonNamed("Stage light · Day").onClick.Invoke();
            Assert.AreEqual(StageLight.Day, hud.CurrentLight); Assert.AreEqual(0, PlayerPrefs.GetInt(StageLook.LightKey));
            CollectionAssert.AreEqual(new[] { StageLight.Night, StageLight.Day }, changes);
        }

        [UnityTest]
        public IEnumerator SavedGoldenHourMigratesToDaylightWithoutTouchingTheCollection()
        {
            PlayerPrefs.SetInt(StageLook.LightKey, 1);
            BuildHud(); yield return null;
            Assert.AreEqual(1, (int)StageLight.Golden); Assert.AreEqual(2, (int)StageLight.Night);
            Assert.AreEqual(StageLight.Day, hud.CurrentLight); Assert.AreEqual(0, PlayerPrefs.GetInt(StageLook.LightKey));
            Assert.AreEqual(StageLook.Name(StageLight.Day), StageLook.Name(StageLight.Golden));
            Assert.AreEqual(StageLook.Backdrop(StageWorld.Studio, StageLight.Day, "pip", Color.red),
                StageLook.Backdrop(StageWorld.Studio, StageLight.Golden, "pip", Color.red));
            Assert.AreEqual(1, session.GetOwnedCount(FusionId));
        }

        [UnityTest]
        public IEnumerator ShortLandscapeKeepsSpeechAndNamedControlsUsable()
        {
            BuildHud(); yield return null;
            // Exercise the supported landscape reference size independently of the Editor Game View dimensions.
            var safe = root.GetComponentsInChildren<RectTransform>(true).Single(t => t.name == "Safe area");
            safe.anchorMin = safe.anchorMax = new Vector2(.5f, .5f); safe.sizeDelta = new Vector2(844, 390);
            Assert.IsTrue(hud.ShowToy(ToyCatalog.GoldId));
            yield return null; Canvas.ForceUpdateCanvases();
            var expression = hud.ShowToyExpression(ReactionCue.PickUp);
            Assert.IsFalse(expression.IsEmpty);
            Assert.AreEqual(expression.Sound, hud.ToyBubbleText, "The controls must leave room for a real reaction bubble in landscape.");
            Assert.GreaterOrEqual(hud.StageViewport.height / root.GetComponentInChildren<Canvas>().scaleFactor, 62);
            Assert.IsFalse(root.GetComponentsInChildren<RectTransform>(true).Single(t => t.name == "Ways to play").gameObject.activeInHierarchy,
                "The optional reminder yields before the named controls or speech bubble.");
            foreach (string label in new[] { "Pockles", "Backdrop", "Light", "About" })
                Assert.IsTrue(ButtonNamed("Stage tab · " + label).interactable);
        }

        [UnityTest]
        public IEnumerator SavedNightRemainsNightAfterRemovingGoldenChoice()
        {
            PlayerPrefs.SetInt(StageLook.LightKey, 2);
            BuildHud(); yield return null;
            Assert.AreEqual(StageLight.Night, hud.CurrentLight); Assert.AreEqual(2, PlayerPrefs.GetInt(StageLook.LightKey));
            CollectionAssert.AreEqual(new[] { StageLight.Day, StageLight.Night }, StageLook.Lights);
            Assert.AreEqual(StageLight.Day, StageLook.NormalizeLight((StageLight)999));
        }

        private Button ButtonNamed(string name) => root.GetComponentsInChildren<Button>().Single(b => b.name == name);
        private RectTransform Tray => root.GetComponentsInChildren<RectTransform>(true).Single(t => t.name == "Stage tray");
    }
}
#endif
