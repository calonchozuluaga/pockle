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
    /// <summary>The first-release box reveal (owner, November 3): single boxes, Mystery Mix packs, real serials only, resume.</summary>
    public sealed class BoxRevealUiTests
    {
        private static readonly string[] Keys = { "pockle.onboarding.v1.ageConfirmed", "pockle.onboarding.v1.ageLocked", "pockle.onboarding.v1.completed" };
        private readonly int[] previous = new int[3];
        private readonly bool[] had = new bool[3];
        private GameObject root;
        private PrototypeHud hud;
        private readonly List<int> progress = new List<int>();
        private readonly List<RarityTier> opened = new List<RarityTier>();
        private readonly List<string> finished = new List<string>(), played = new List<string>();

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            for (int i = 0; i < Keys.Length; i++) { had[i] = PlayerPrefs.HasKey(Keys[i]); previous[i] = PlayerPrefs.GetInt(Keys[i]); PlayerPrefs.DeleteKey(Keys[i]); }
            root = new GameObject("Box reveal UI test");
            var session = root.AddComponent<CollectionSession>(); session.Initialize();
            hud = root.AddComponent<PrototypeHud>();
            hud.Initialize(_ => { }, _ => { }, _ => { }, _ => { }); hud.Bind(session);
            progress.Clear(); opened.Clear(); finished.Clear(); played.Clear();
            RarityLook.TierSource = id => id.StartsWith("wally.") ? RarityLook.FromTier(WallyCatalog.Tier(id)) : (RarityTier?)null; // as TactilePrototype sets it
            hud.BoxRevealProgress += (_, count) => progress.Add(count);
            hud.BoxRevealOpened += tier => opened.Add(tier);
            hud.BoxRevealFinished += receipt => finished.Add(receipt);
            hud.BoxRevealPlayRequested += id => played.Add(id);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(root); yield return null;
            RarityLook.TierSource = null;
            for (int i = 0; i < Keys.Length; i++) { if (had[i]) PlayerPrefs.SetInt(Keys[i], previous[i]); else PlayerPrefs.DeleteKey(Keys[i]); }
            PlayerPrefs.Save();
        }

        private static BoxRevealItem Item(string id, bool isNew = true, long? serial = null, int? size = null, RarityTier? tier = null)
            => new BoxRevealItem { CollectibleId = id, IsNew = isNew, Serial = serial, EditionSize = size, Tier = tier };

        private static BoxRevealView View(string receipt, params BoxRevealItem[] items)
            => new BoxRevealView { ReceiptId = receipt, Items = items, Source = items.Length > 1 ? BoxRevealSource.MysteryMix : BoxRevealSource.CollectionBox };

        private Transform Reveal => root.GetComponentsInChildren<Transform>(true).First(t => t.name == "Box reveal");
        private IEnumerable<string> ShownTexts => Reveal.GetComponentsInChildren<Text>(false).Where(t => t.gameObject.activeInHierarchy).Select(t => t.text);
        private Button RevealButton(string name) => Reveal.GetComponentsInChildren<Button>(false).First(b => b.name == name && b.gameObject.activeInHierarchy);
        private bool Shown(string name) => Reveal.GetComponentsInChildren<Transform>(true).Any(t => t.name == name && t.gameObject.activeInHierarchy);

        [UnityTest]
        public IEnumerator SingleBoxWaitsForATapThenShowsTheRealSerialAndKeepCloses()
        {
            Assert.That(hud.ShowBoxReveal(View("r1", Item(WallyCatalog.KelpId, true, 412, 10000))), Is.True);
            yield return null;
            Assert.That(hud.BoxRevealWaitingForTap, Is.True, "the box waits for the player");
            Assert.That(Shown("Reveal card"), Is.False, "nothing about the toy shows before it opens");
            Assert.That(progress, Is.Empty);
            hud.CompleteBoxRevealStep(); yield return null;
            Assert.That(ShownTexts, Has.Member("Kelp Mint"));
            Assert.That(ShownTexts, Has.Member("#412 of 10,000"));
            Assert.That(ShownTexts, Has.Member("New!"));
            Assert.That(opened, Is.EqualTo(new[] { RarityTier.Common }), "one cue at the pop");
            Assert.That(progress, Is.EqualTo(new[] { 1 }));
            Assert.That(Shown("Skip"), Is.False, "a single box has no Skip");
            RevealButton("Keep").onClick.Invoke(); yield return null;
            Assert.That(hud.BoxRevealVisible, Is.False);
            Assert.That(finished, Is.EqualTo(new[] { "r1" }));
        }

        [UnityTest]
        public IEnumerator WithoutAServerSerialNoNumberIsShownAndASpareSaysExtra()
        {
            hud.ShowBoxReveal(View("r2", Item(WallyCatalog.KelpId, false))); yield return null;
            hud.CompleteBoxRevealStep(); yield return null;
            Assert.That(ShownTexts.Any(t => t.Contains("#")), Is.False, "never a placeholder serial");
            Assert.That(ShownTexts, Has.Member("Extra"));
            Assert.That(ShownTexts.Any(t => t.Contains("spare for fusing or trading")), Is.True);
            Assert.That(ShownTexts, Has.No.Member("New!"));
            var half = Item(WallyCatalog.KelpId, true, 412, null);
            Assert.That(half.SerialText, Is.Null, "a serial without its edition size is not shown either");
            Assert.That(Item(WallyCatalog.KelpId, true, 0, 10000).SerialText, Is.Null);
            Assert.That(Item(WallyCatalog.KelpId, true, 10001, 10000).SerialText, Is.Null);
        }

        [UnityTest]
        public IEnumerator PackOpensOneAtATimeAndSkipGoesToTheSummary()
        {
            hud.ShowBoxReveal(View("pack", Item(WallyCatalog.KelpId), Item(WallyCatalog.LeafId), Item(WallyCatalog.DiamondId, false),
                Item(WallyCatalog.KelpId, false), Item(WallyCatalog.SpecialId))); yield return null;
            Assert.That(ShownTexts, Has.Member("Box 1 of 5"));
            Assert.That(Shown("Skip"), Is.True);
            hud.CompleteBoxRevealStep(); yield return null;
            Assert.That(Shown("Next box"), Is.True);
            Assert.That(Shown("Play"), Is.True, "any toy can be played straight away");
            RevealButton("Next box").onClick.Invoke(); yield return null;
            Assert.That(ShownTexts, Has.Member("Box 2 of 5"));
            Assert.That(hud.BoxRevealWaitingForTap, Is.True);
            RevealButton("Skip").onClick.Invoke(); yield return null;
            Assert.That(hud.BoxRevealSummaryVisible, Is.True);
            Assert.That(progress.Last(), Is.EqualTo(5), "skipping counts everything as revealed so a restart won't replay it");
            Assert.That(opened, Is.EqualTo(new[] { RarityTier.Common, RarityTier.Secret }), "one cue for the skipped boxes, at the best tier among them");
            Assert.That(Reveal.GetComponentsInChildren<Button>(false).Count(b => b.name.StartsWith("Summary toy")), Is.EqualTo(5));
            Assert.That(ShownTexts, Has.Member("3 new, 2 extra"));
            RevealButton("Done").onClick.Invoke(); yield return null;
            Assert.That(finished, Is.EqualTo(new[] { "pack" }));
        }

        [UnityTest]
        public IEnumerator LastBoxOfAPackLeadsToTheSummary()
        {
            hud.ShowBoxReveal(View("pack3", Item(WallyCatalog.KelpId), Item(WallyCatalog.LeafId), Item(WallyCatalog.RoseId))); yield return null;
            for (int i = 0; i < 2; i++) { hud.CompleteBoxRevealStep(); yield return null; RevealButton("Next box").onClick.Invoke(); yield return null; }
            hud.CompleteBoxRevealStep(); yield return null;
            RevealButton("See all 3").onClick.Invoke(); yield return null;
            Assert.That(hud.BoxRevealSummaryVisible, Is.True);
            Assert.That(progress, Is.EqualTo(new[] { 1, 2, 3 }));
            Assert.That(opened.Count, Is.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator ResumeStartsAtTheSavedCountWithoutReplayingCues()
        {
            var view = View("resume", Item(WallyCatalog.KelpId), Item(WallyCatalog.LeafId), Item(WallyCatalog.RoseId));
            view.StartAt = 2;
            hud.ShowBoxReveal(view); yield return null;
            Assert.That(hud.BoxRevealIndex, Is.EqualTo(2), "opens at the first unseen box");
            Assert.That(ShownTexts, Has.Member("Box 3 of 3"));
            Assert.That(opened, Is.Empty);
            hud.FinishBoxReveal(); yield return null;
            progress.Clear(); opened.Clear();
            var seen = View("seen", Item(WallyCatalog.KelpId), Item(WallyCatalog.LeafId)); seen.StartAt = 2;
            hud.ShowBoxReveal(seen); yield return null;
            Assert.That(hud.BoxRevealSummaryVisible, Is.True, "a fully seen pack goes straight to its summary");
            Assert.That(opened, Is.Empty); Assert.That(progress, Is.Empty);
        }

        [UnityTest]
        public IEnumerator TheServerTierWinsAndASecretGetsTheObsidianNight()
        {
            hud.ShowBoxReveal(View("secret", Item(WallyCatalog.KelpId, true, 7, 1600, RarityTier.Secret))); yield return null;
            var background = Reveal.GetComponent<Image>().color;
            Assert.That(background.r + background.g + background.b, Is.LessThan(.3f), "obsidian, not the plum night");
            hud.CompleteBoxRevealStep(); yield return null;
            Assert.That(ShownTexts, Has.Member("Secret"));
            Assert.That(ShownTexts, Has.Member("#7 of 1,600"));
            Assert.That(opened, Is.EqualTo(new[] { RarityTier.Secret }));
        }

        [UnityTest]
        public IEnumerator CalmMotionIsAShortCrossfade()
        {
            hud.SetSettings(true, true, true);
            hud.ShowBoxReveal(View("calm", Item(WallyCatalog.DiamondId))); yield return null;
            var player = Reveal.GetComponent<BoxRevealPlayer>();
            Assert.That(player.Calm, Is.True);
            Reveal.GetComponent<Button>().onClick.Invoke(); // tap the box
            yield return new WaitForSecondsRealtime(BoxRevealPlayer.CalmDuration + .25f);
            Assert.That(player.Phase, Is.EqualTo(BoxRevealPlayer.State.Shown), "calm lands without waiting for the full opening");
            Assert.That(opened, Is.EqualTo(new[] { RarityTier.Epic }));
            Assert.That(root.GetComponentsInChildren<RawImage>(false).Count(r => r.name == "Sparkle"), Is.Zero, "no confetti with calm motion");
        }

        [UnityTest]
        public IEnumerator FullMotionLandsOnItsOwn()
        {
            hud.ShowBoxReveal(View("full", Item(WallyCatalog.LeafId))); yield return null;
            Reveal.GetComponent<Button>().onClick.Invoke();
            Assert.That(Reveal.GetComponent<BoxRevealPlayer>().Phase, Is.EqualTo(BoxRevealPlayer.State.Opening));
            yield return new WaitForSecondsRealtime(BoxRevealPlayer.FullDuration + .3f);
            Assert.That(Reveal.GetComponent<BoxRevealPlayer>().Phase, Is.EqualTo(BoxRevealPlayer.State.Shown));
            Assert.That(opened, Is.EqualTo(new[] { RarityTier.Rare }));
            Assert.That(progress, Is.EqualTo(new[] { 1 }));
        }

        [UnityTest]
        public IEnumerator BadViewsAreRefusedAndASecondReceiptWaits()
        {
            Assert.That(hud.ShowBoxReveal(null), Is.False);
            Assert.That(hud.ShowBoxReveal(View("", Item(WallyCatalog.KelpId))), Is.False, "a receipt is required");
            Assert.That(hud.ShowBoxReveal(View("empty")), Is.False);
            Assert.That(hud.ShowBoxReveal(View("bad", Item("nobody.nothing"))), Is.False, "unknown toys never show");
            Assert.That(hud.BoxRevealVisible, Is.False);
            Assert.That(hud.ShowBoxReveal(View("a", Item(WallyCatalog.KelpId))), Is.True);
            Assert.That(hud.ShowBoxReveal(View("a", Item(WallyCatalog.KelpId))), Is.True, "the same receipt again is a no-op");
            Assert.That(hud.ShowBoxReveal(View("b", Item(WallyCatalog.LeafId))), Is.False, "a different delivery waits for this one");
            yield return null;
        }

        [UnityTest]
        public IEnumerator BackLandsTheBoxThenCloses()
        {
            hud.ShowBoxReveal(View("back", Item(WallyCatalog.KelpId))); yield return null;
            Assert.That(hud.GoBack(), Is.True);
            Assert.That(hud.BoxRevealVisible, Is.True, "Back never throws away an unseen toy");
            Assert.That(ShownTexts, Has.Member("Kelp Mint"));
            Assert.That(hud.GoBack(), Is.True);
            Assert.That(hud.BoxRevealVisible, Is.False);
            Assert.That(finished, Is.EqualTo(new[] { "back" }));
        }

        [UnityTest]
        public IEnumerator PlayRequestsTheToyAndLeavesTheReveal()
        {
            hud.ShowBoxReveal(View("play", Item(WallyCatalog.KelpId))); yield return null;
            hud.CompleteBoxRevealStep(); yield return null;
            RevealButton("Play").onClick.Invoke(); yield return null;
            Assert.That(played, Is.EqualTo(new[] { WallyCatalog.KelpId }));
            Assert.That(finished, Is.EqualTo(new[] { "play" }));
            Assert.That(hud.BoxRevealVisible, Is.False);
        }
    }
}
#endif
