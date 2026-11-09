#if UNITY_EDITOR
using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Pockle.Tests
{
    /// <summary>UX-052 server walking-box cards, the UX-050 cold-start sign-in gate and the October 21 rarity looks.</summary>
    public sealed class WalkingBoxUiTests
    {
        private static readonly string[] Keys = { "pockle.onboarding.v1.ageConfirmed", "pockle.onboarding.v1.ageLocked", "pockle.onboarding.v1.completed" };
        private readonly int[] previous = new int[3];
        private readonly bool[] had = new bool[3];
        private GameObject root;
        private PrototypeHud hud;
        private int opened;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            for (int i = 0; i < Keys.Length; i++) { had[i] = PlayerPrefs.HasKey(Keys[i]); previous[i] = PlayerPrefs.GetInt(Keys[i]); PlayerPrefs.DeleteKey(Keys[i]); }
            root = new GameObject("Walking box UI test");
            var session = root.AddComponent<CollectionSession>(); session.Initialize();
            hud = root.AddComponent<PrototypeHud>();
            hud.Initialize(_ => { }, _ => { }, _ => { }, _ => { }); hud.Bind(session);
            opened = 0; hud.DailyBoxRequested += () => opened++;
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            UnityEngine.Object.Destroy(root); yield return null;
            RarityLook.TierSource = null;
            for (int i = 0; i < Keys.Length; i++) { if (had[i]) PlayerPrefs.SetInt(Keys[i], previous[i]); else PlayerPrefs.DeleteKey(Keys[i]); }
            PlayerPrefs.Save();
        }

        private static WalkingBoxView View(params string[] banked) => new WalkingBoxView
        { BankedCollectionIds = banked, EarnedToday = banked.Length, StepCredit = 400, CanOpen = banked.Length > 0, SecondsUntilNextBox = null };

        [UnityTest]
        public IEnumerator BankedBoxesShowTheirSavedCollectionAndOpenThroughTheServerEvent()
        {
            hud.SetWalkingBoxes(View("wally", "fox")); yield return null;
            Assert.That(Text("Today's box").Contains("2 BOXES READY"), Is.True);
            Assert.That(Text("Today's box").Contains("Wally box"), Is.True, "the first banked box's own collection, as saved");
            Assert.That(Text("Banked boxes").Contains("Today"), Is.True);
            var dots = root.GetComponentsInChildren<Image>(true).Where(i => i.name.StartsWith("Today ") && i.gameObject.activeInHierarchy).ToList();
            Assert.That(dots.Count, Is.EqualTo(4), "one dot per box a day");
            Assert.That(dots.Count(d => d.color.a > .9f), Is.EqualTo(2), "two earned today");
            Assert.That(root.GetComponentsInChildren<RawImage>(true).Count(r => r.transform.parent.name.StartsWith("Bank slot") && r.enabled), Is.EqualTo(2), "each banked box shows its own box art");
            Button("Open your box").onClick.Invoke();
            Assert.That(opened, Is.EqualTo(1));
            Assert.That(root.GetComponentsInChildren<Button>().Any(b => b.name.IndexOf("reroll", StringComparison.OrdinalIgnoreCase) >= 0
                || b.name.IndexOf("choose", StringComparison.OrdinalIgnoreCase) >= 0), Is.False, "the walking page exposes no way to pick or reroll a box's collection");
        }

        [UnityTest]
        public IEnumerator UnknownCollectionClearsThePreviousBoxPictureUntilTheServerAssignsOne()
        {
            hud.SetWalkingBoxes(View("wally"));
            Button("Shop").onClick.Invoke(); yield return null;
            var card = root.GetComponentsInChildren<Transform>(true).First(t => t.name == "Daily walking box");
            var art = card.Find("Closed box").GetComponent<RawImage>();
            var assigned = art.texture;
            Assert.That(art.enabled && assigned != null, Is.True);
            hud.SetWalkingBoxes(View()); yield return null;
            Assert.That(art.enabled, Is.False, "An unknown collection must not retain Wally or the legacy Pip carton.");
            Assert.That(art.texture, Is.Null);
            Assert.That(Visible("Unknown walking collection"), Is.True);
            hud.SetWalkingBoxes(View("fox")); yield return null;
            Assert.That(art.enabled && art.texture != null, Is.True);
            Assert.That(art.texture, Is.Not.SameAs(assigned), "The next saved collection gets its own carton.");
            Assert.That(Visible("Unknown walking collection"), Is.False);
            hud.ClearWalkingBoxes(); yield return null;
            Assert.That(art.enabled && art.texture != null, Is.True, "Leaving server mode restores the local preview.");
            Assert.That(Visible("Unknown walking collection"), Is.False);
        }

        [UnityTest]
        public IEnumerator OfflineAndReconnectingShowSavedBoxesButNeverOpen()
        {
            foreach (var state in new[] { WalkingBoxConnection.Offline, WalkingBoxConnection.Reconnecting })
            {
                var v = View("wally"); v.Connection = state; hud.SetWalkingBoxes(v); yield return null;
                Assert.That(Button("Open your box").interactable, Is.False, state + " must not open");
                Button("Open your box").onClick.Invoke();
                Assert.That(opened, Is.Zero);
                Assert.That(Text("Today's box").Contains("Wally box"), Is.True, "the saved box stays visible");
            }
            Assert.That(Text("Today's box").Contains("Reconnecting"), Is.True);
        }

        [UnityTest]
        public IEnumerator FullBankAndFinishedDayPauseEarningWithoutAMidnightCountdown()
        {
            var full = View("wally", "fox"); full.SecondsUntilNextBox = 3600; hud.SetWalkingBoxes(full); yield return null;
            Assert.That(Text("Today's box").Contains("bank is full"), Is.True);
            Assert.That(Visible("Today's steps"), Is.False, "earning is paused while two boxes wait");
            var done = View(); done.EarnedToday = 4; hud.SetWalkingBoxes(done); yield return null;
            Assert.That(Text("Today's box").Contains("all 4 boxes for today"), Is.True);
            Assert.That(Visible("Next box countdown"), Is.False);
        }

        [UnityTest]
        public IEnumerator CountdownComesFromTheServerNotThePhoneClock()
        {
            float started = Time.realtimeSinceStartup;
            var v = View(); v.SecondsUntilNextBox = 2 * 3600 + 5 * 60 + 9; hud.SetWalkingBoxes(v);
            yield return null; yield return null;
            string shown = Text("Today's box");
            const string prefix = "Next box in ";
            int start = shown.IndexOf(prefix, StringComparison.Ordinal);
            Assert.That(start, Is.GreaterThanOrEqualTo(0), shown);
            string clock = shown.Substring(start + prefix.Length).Split(' ')[0];
            Assert.That(TimeSpan.TryParse(clock, out var left), Is.True, shown);
            double expected = v.SecondsUntilNextBox.Value - (Time.realtimeSinceStartup - started);
            Assert.That(left.TotalSeconds, Is.InRange(expected - 1.5, expected + 1.5), "server seconds minus local elapsed time");
            var walking = View(); walking.StepCredit = 640; hud.SetWalkingBoxes(walking); yield return null;
            Assert.That(Text("Today's box").Contains("360 more steps"), Is.True);
            Assert.That(Text("Today's box").Contains("640"), Is.True);
        }

        [UnityTest]
        public IEnumerator WalkingRulesUseTheOneHourDefaultAndRefreshFromTheServerSnapshot()
        {
            hud.SetWalkingBoxes(View());
            Button("Shop").onClick.Invoke(); yield return null;
            Assert.That(Text("Walking box rules"), Does.Contain("1,000 steps earns a box · one every hour · up to 4 a day · 2 can wait"));
            Button("Rules & odds").onClick.Invoke();
            Assert.That(root.GetComponentsInChildren<Text>().Any(t => t.text.Contains("Every 1,000 steps earns a box, one every hour, up to 4 a day. Up to 2 can wait for you")), Is.True);
            hud.GoBack();
            var changed = View(); changed.CooldownHours = 2; changed.StepsPerBox = 2000; changed.MaxPerDay = 3; changed.MaxBanked = 1;
            hud.SetWalkingBoxes(changed); yield return null;
            Assert.That(Text("Walking box rules"), Does.Contain("2,000 steps earns a box · one every 2 hours · up to 3 a day · 1 can wait"));
            Button("Rules & odds").onClick.Invoke();
            Assert.That(root.GetComponentsInChildren<Text>().Any(t => t.text.Contains("Every 2,000 steps earns a box, one every 2 hours, up to 3 a day. Up to 1 can wait for you")), Is.True);
            Assert.That(opened, Is.Zero, "Rule copy never awards or opens a box.");
        }

        [UnityTest]
        public IEnumerator CountdownRepairsTextOverwrittenDuringTheSameSecond()
        {
            var node = new GameObject("Countdown regression", typeof(RectTransform), typeof(Text));
            node.transform.SetParent(root.transform);
            var text = node.GetComponent<Text>();
            var countdown = node.AddComponent<CountdownText>();
            countdown.Target = text; countdown.Source = () => TimeSpan.FromSeconds(7509); countdown.Prefix = "Next box in ";
            countdown.SendMessage("Update"); Assert.That(text.text, Is.EqualTo("Next box in 2:05:09"));
            text.text = "Next box in"; countdown.SendMessage("Update");
            Assert.That(text.text, Is.EqualTo("Next box in 2:05:09"));
            countdown.Source = () => null; countdown.SendMessage("Update");
            Assert.That(text.text, Is.EqualTo("Checking…"));
            yield return null;
        }

        [UnityTest]
        public IEnumerator ClearingTheServerSnapshotRestoresTheLocalCard()
        {
            hud.SetWalkingBoxes(View("wally")); yield return null;
            hud.ClearWalkingBoxes(); yield return null;
            Assert.That(hud.ServerBoxes, Is.False);
            Assert.That(Visible("Banked boxes"), Is.False);
        }

        [UnityTest]
        public IEnumerator SignedOutReturningPlayerMustSignInAndKeepsTheirAgeAndTutorial()
        {
            OnboardingState.ConfirmAge(); OnboardingState.Complete();
            var accounts = new SignedOut();
            hud.SetAccountService(accounts);
            hud.StartOnboardingIfNeeded(); yield return null;
            Assert.That(Visible("Onboarding · SignIn"), Is.True);
            Assert.That(OnboardingState.AgeConfirmed && OnboardingState.Completed, Is.True);
            Assert.That(hud.GoBack(), Is.False, "Back from the sign-in gate leaves the app instead of reopening the splash");
            accounts.SignedIn = true;
            UnityEngine.Object.Destroy(root); yield return null;
            // A fresh launch, now signed in (prefs untouched).
            root = new GameObject("Walking box UI test · relaunch");
            var session = root.AddComponent<CollectionSession>(); session.Initialize();
            hud = root.AddComponent<PrototypeHud>(); hud.Initialize(_ => { }, _ => { }, _ => { }, _ => { }); hud.Bind(session);
            hud.SetAccountService(accounts); hud.StartOnboardingIfNeeded(); yield return null;
            Assert.That(hud.OnboardingVisible, Is.False, "signed in: straight to the game");
        }

        [Test]
        public void LadderTiersComeFromTheRuntimeAndOddsAreNeverInvented()
        {
            RarityLook.TierSource = id => id == "wally.molten" ? RarityTier.Secret : id == "wally.diamond" ? RarityTier.Epic : (RarityTier?)null;
            Assert.That(RarityLook.For(RarityTier.Epic, false, "Epic").Shown, Is.True);
            Assert.That(RarityLook.For(RarityTier.Secret, true, "Secret").Secret, Is.True);
            Assert.That(RarityLook.Words(RarityTier.Rare), Is.EqualTo("Rare"));
            Assert.That(RarityLook.FromTier(Pockle.Core.ToyTier.Epic), Is.EqualTo(RarityTier.Epic));
            Assert.That(RarityLook.FromTier(Pockle.Core.ToyTier.Secret), Is.EqualTo(RarityTier.Secret));
            Assert.That(TierOdds.Format(.15), Is.EqualTo("15%"));
            Assert.That(TierOdds.Format(1 / 72.0), Is.EqualTo("about 1 in 72"));
            Assert.That(new WalkingBoxView().Odds, Is.Null, "the UI shows odds only when the server sends them");
        }

        [UnityTest]
        public IEnumerator WallyLineupShowsItsLadderTiersOnFoundAndUnfoundSlots()
        {
            RarityLook.TierSource = id => id.StartsWith("wally.") ? RarityLook.FromTier(Pockle.Core.WallyCatalog.Tier(id)) : (RarityTier?)null;
            root.GetComponentsInChildren<Button>().First(b => b.GetComponentsInChildren<Text>().Any(t => t.text == "Shelf")).onClick.Invoke(); yield return null;
            root.GetComponentsInChildren<Button>().First(b => b.GetComponentsInChildren<Text>().Any(t => t.text == "All collections")).onClick.Invoke(); yield return null;
            root.GetComponentsInChildren<Button>().First(b => b.name == "Collection · wally").onClick.Invoke(); yield return null;
            string Badge(string id)
            {
                var tile = root.GetComponentsInChildren<Button>(true).First(b => b.name == "Collectible · " + id);
                var badge = tile.transform.Find("Tier badge");
                return badge.gameObject.activeSelf ? badge.GetComponentInChildren<Text>().text : "";
            }
            Assert.That(Badge(Pockle.Core.WallyCatalog.SpecialId), Is.EqualTo("Secret"));
            Assert.That(Badge(Pockle.Core.WallyCatalog.DiamondId), Is.EqualTo("Epic"));
            Assert.That(Badge(Pockle.Core.WallyCatalog.GlowId), Is.EqualTo("Epic"), "Deep Sea Glow moved to Epic");
            Assert.That(Badge(Pockle.Core.WallyCatalog.LeafId), Is.EqualTo("Rare"));
            Assert.That(Badge(Pockle.Core.WallyCatalog.ClassicId), Is.EqualTo("Common"), "Every slot shows its rarity, including hidden Common toys.");
        }

        private sealed class SignedOut : IAccountService
        {
            public bool SignedIn;
            public bool IsSignedIn => SignedIn;
            public string Username => SignedIn ? "Comet_Mochi" : null;
            public void SignIn(AccountProvider provider, Action<AccountResult> done) { SignedIn = true; done(new AccountResult(AccountStatus.SignedIn, "", true)); }
            public void ClaimUsername(string username, Action<UsernameCheck> done) => done(UsernameCheck.Available);
            public void SignOut(Action done) { SignedIn = false; done(); }
            public void RequestDeletion(Action<bool> done) => done(true);
        }

        private string Text(string container)
        {
            var node = root.GetComponentsInChildren<Transform>(true).First(t => t.name == container);
            return string.Join(" | ", node.GetComponentsInChildren<Text>(false).Where(t => t.gameObject.activeInHierarchy).Select(t => t.text));
        }
        private bool Visible(string name) => root.GetComponentsInChildren<Transform>(false).Any(t => t.name == name && t.gameObject.activeInHierarchy);
        private Button Button(string name) => root.GetComponentsInChildren<Button>(true).First(b => b.name == name);
    }
}
#endif
