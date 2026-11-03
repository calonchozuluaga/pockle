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
    /// <summary>UX-050 first-run flow: age gate, sign-in (test build path), collector name and tutorial.</summary>
    public sealed class OnboardingUiTests
    {
        private static readonly string[] Keys = { "pockle.onboarding.v1.ageConfirmed", "pockle.onboarding.v1.ageLocked", "pockle.onboarding.v1.completed" };
        private const string NameKey = "pockle.profile.local.name";
        private readonly int[] previous = new int[3];
        private readonly bool[] had = new bool[3];
        private bool hadName; private string previousName;
        private GameObject root;
        private PrototypeHud hud;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            for (int i = 0; i < Keys.Length; i++) { had[i] = PlayerPrefs.HasKey(Keys[i]); previous[i] = PlayerPrefs.GetInt(Keys[i]); PlayerPrefs.DeleteKey(Keys[i]); }
            hadName = PlayerPrefs.HasKey(NameKey); previousName = PlayerPrefs.GetString(NameKey); PlayerPrefs.DeleteKey(NameKey);
            root = new GameObject("Onboarding UI test");
            var session = root.AddComponent<CollectionSession>(); session.Initialize();
            hud = root.AddComponent<PrototypeHud>();
            hud.Initialize(_ => { }, _ => { }, _ => { }, _ => { }); hud.Bind(session);
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            UnityEngine.Object.Destroy(root); yield return null;
            for (int i = 0; i < Keys.Length; i++) { if (had[i]) PlayerPrefs.SetInt(Keys[i], previous[i]); else PlayerPrefs.DeleteKey(Keys[i]); }
            if (hadName) PlayerPrefs.SetString(NameKey, previousName); else PlayerPrefs.DeleteKey(NameKey);
            PlayerPrefs.Save();
        }

        [Test]
        public void AgeAndUsernameRules()
        {
            var today = new DateTime(2026, 10, 21);
            Assert.That(OnboardingRules.AgeOn(today, 2013, 10, 21), Is.EqualTo(13));
            Assert.That(OnboardingRules.AgeOn(today, 2013, 10, 22), Is.EqualTo(12));
            Assert.That(OnboardingRules.IsRealBirthday(today, 2010, 2, 30), Is.False);
            Assert.That(OnboardingRules.IsRealBirthday(today, 2027, 1, 1), Is.False);
            Assert.That(OnboardingRules.IsRealBirthday(today, 2008, 2, 29), Is.True);
            Assert.That(OnboardingRules.UsernameProblem("ab"), Is.Not.Null);
            Assert.That(OnboardingRules.UsernameProblem("Comet Mochi"), Is.Not.Null);
            Assert.That(OnboardingRules.UsernameProblem("Comet_Mochi42"), Is.Null);
            Assert.That(OnboardingRules.UsernameProblem(OnboardingRules.SuggestUsername(new System.Random(7))), Is.Null);
        }

        [UnityTest]
        public IEnumerator HudBuiltWithoutTheAppNeverShowsOnboarding()
        {
            yield return null;
            Assert.That(hud.OnboardingVisible, Is.False, "tests and captures that build the HUD directly must not be covered");
        }

        [UnityTest]
        public IEnumerator UnderThirteenIsBlockedAndCannotRetry()
        {
            hud.StartOnboardingIfNeeded(); yield return null;
            Assert.That(hud.OnboardingVisible, Is.True);
            Click("Let's go!");
            EnterBirthday(DateTime.Today.Year - 10);
            Click("Next");
            Assert.That(Visible("Onboarding · Blocked"), Is.True);
            Assert.That(OnboardingState.AgeLocked, Is.True);
            Assert.That(hud.GoBack(), Is.False, "no way back to the birthday from the blocked screen");
            // A fresh launch goes straight back to the blocked screen.
            hud.StartOnboardingIfNeeded(); yield return null;
            Assert.That(Visible("Onboarding · Blocked"), Is.True);
        }

        [UnityTest]
        public IEnumerator AdultSignsInPicksANameAndFinishesTheTutorial()
        {
            hud.StartOnboardingIfNeeded(); yield return null;
            Click("Let's go!");
            Assert.That(Button("Next").interactable, Is.False, "Next waits for a full birthday");
            EnterBirthday(DateTime.Today.Year - 30);
            Assert.That(Button("Next").interactable, Is.True);
            Click("Next");
            Assert.That(Visible("Onboarding · SignIn"), Is.True);
            Assert.That(OnboardingState.AgeConfirmed, Is.True);
            Click("Continue (test build)");
            Assert.That(Visible("Onboarding · Username"), Is.True);
            var field = Field("Collector name");
            Assert.That(OnboardingRules.UsernameProblem(field.text), Is.Null, "a valid suggestion is prefilled");
            field.text = "ab"; Click("That's me!");
            Assert.That(Visible("Onboarding · Username"), Is.True, "too short stays on the name step");
            field.text = "CometMochi42"; Click("That's me!");
            Assert.That(Visible("Onboarding · Tutorial"), Is.True);
            for (int i = 0; i < 3; i++) Click("Next");
            Assert.That(Button("Next").GetComponentInChildren<Text>().text, Is.EqualTo("Start collecting!"));
            Click("Next");
            Assert.That(hud.OnboardingVisible, Is.False);
            Assert.That(OnboardingState.Completed, Is.True);
            Assert.That(PlayerPrefs.GetString(NameKey), Is.EqualTo("CometMochi42"));
            hud.StartOnboardingIfNeeded(); yield return null;
            Assert.That(hud.OnboardingVisible, Is.False, "finished once, never shown again");
        }

        [UnityTest]
        public IEnumerator FakeAccountServiceDrivesSignIn()
        {
            var service = new FakeAccounts();
            hud.SetAccountService(service);
            hud.StartOnboardingIfNeeded(); yield return null;
            Click("Let's go!"); EnterBirthday(DateTime.Today.Year - 20); Click("Next");
            Assert.That(Visible("Continue (test build)"), Is.False, "no test path once accounts exist");
            service.Next = new AccountResult(AccountStatus.Failed, "No connection.");
            Click("Continue with Google");
            Assert.That(Visible("Onboarding · SignIn"), Is.True);
            service.Next = new AccountResult(AccountStatus.SignedIn);
            Click("Continue with Apple");
            Assert.That(service.LastProvider, Is.EqualTo(AccountProvider.Apple));
            Field("Collector name").text = "Taken_Name"; service.Claim = UsernameCheck.Taken; Click("That's me!");
            Assert.That(Visible("Onboarding · Username"), Is.True);
            service.Claim = UsernameCheck.Available; Click("That's me!");
            Assert.That(Visible("Onboarding · Tutorial"), Is.True);
        }

        [UnityTest]
        public IEnumerator ProviderButtonsFollowAppleAndGoogleStyling()
        {
            hud.SetAccountService(new FakeAccounts());
            hud.StartOnboardingIfNeeded(); yield return null;
            Click("Let's go!"); EnterBirthday(DateTime.Today.Year - 25); Click("Next"); yield return null;
            var apple = Button("Continue with Apple"); var google = Button("Continue with Google");
            Assert.That(apple.GetComponent<Image>().color, Is.EqualTo(Color.black));
            Assert.That(apple.GetComponentInChildren<Text>().color, Is.EqualTo(Color.white));
            Assert.That(google.targetGraphic.color, Is.EqualTo(Color.white), "white fill inside the border");
            var border = google.GetComponent<Image>().color;
            Assert.That(Mathf.Abs(border.r - 0x74 / 255f) < .01f && Mathf.Abs(border.g - 0x77 / 255f) < .01f, Is.True, "Google's #747775 border");
            foreach (var b in new[] { apple, google })
            {
                var label = b.GetComponentInChildren<Text>();
                Assert.That(label.font, Is.Not.EqualTo(PockleTheme.DisplayFont), "titles use the system font, not Pockle's display font");
                Assert.That(label.text.StartsWith("Continue with "), Is.True);
            }
            // Logos are the brands' own files; without them the buttons stay text-only rather than showing a stand-in.
            bool appleFile = Resources.Load<Texture2D>("UI/Brand/AppleLogoWhite") != null, googleFile = Resources.Load<Texture2D>("UI/Brand/GoogleG") != null;
            Assert.That(apple.transform.Find("Apple logo").gameObject.activeSelf, Is.EqualTo(appleFile));
            Assert.That(google.transform.Find("Google logo").gameObject.activeSelf, Is.EqualTo(googleFile));
        }

        [UnityTest]
        public IEnumerator HowToPlayReplaysOnlyTheTutorial()
        {
            OnboardingState.Complete();
            hud.SetSettings(true, false, false);
            hud.SendMessage("ShowTutorialAgain"); yield return null;
            Assert.That(Visible("Onboarding · Tutorial"), Is.True);
            Assert.That(hud.GoBack(), Is.True); // first page: Back closes the replay
            Assert.That(hud.OnboardingVisible, Is.False);
        }

        [UnityTest]
        public IEnumerator SettingsCanRenameSignOutAndDeleteWithConfirmation()
        {
            OnboardingState.ConfirmAge(); OnboardingState.Complete();
            var service = new FakeAccounts(); hud.SetAccountService(service);
            service.SignIn(AccountProvider.Google, _ => { }); service.ClaimUsername("Comet_Mochi", _ => { });
            hud.SendMessage("ShowSettings"); yield return null;
            Click("Change collector name");
            Assert.That(Visible("Onboarding · Username"), Is.True);
            Field("Collector name").text = "Sunny_Jelly7"; Click("That's me!");
            Assert.That(hud.OnboardingVisible, Is.False, "renaming returns to Settings, not the tutorial");
            Assert.That(PlayerPrefs.GetString(NameKey), Is.EqualTo("Sunny_Jelly7"));
            Click("Delete account");
            Assert.That(Visible("Confirm backdrop"), Is.True, "deleting always asks first");
            Assert.That(hud.GoBack(), Is.True); // Back cancels the confirmation
            Assert.That(service.DeletionRequested, Is.False);
            Click("Delete account"); Click("Confirm");
            Assert.That(service.DeletionRequested, Is.True);
            Assert.That(Visible("Onboarding · SignIn"), Is.True, "after deletion the player is signed out");
        }

        [Test]
        public void NotificationChoicesDefaultOnAndPersist()
        {
            const string key = "pockle.notifications.v1.trades";
            bool had = PlayerPrefs.HasKey(key); int prior = PlayerPrefs.GetInt(key);
            try
            {
                PlayerPrefs.DeleteKey(key);
                Assert.That(NotificationPrefs.IsOn(NotificationPrefs.Trades), Is.True);
                NotificationPrefs.Set(NotificationPrefs.Trades, false);
                Assert.That(NotificationPrefs.IsOn(NotificationPrefs.Trades), Is.False);
            }
            finally { if (had) PlayerPrefs.SetInt(key, prior); else PlayerPrefs.DeleteKey(key); }
        }

        private sealed class FakeAccounts : IAccountService
        {
            public AccountResult Next = new AccountResult(AccountStatus.SignedIn);
            public UsernameCheck Claim = UsernameCheck.Available;
            public AccountProvider LastProvider;
            public bool IsSignedIn { get; private set; }
            public string Username { get; private set; }
            public bool DeletionRequested;
            public void SignOut(Action done) { IsSignedIn = false; done(); }
            public void RequestDeletion(Action<bool> done) { DeletionRequested = true; IsSignedIn = false; done(true); }
            public void SignIn(AccountProvider provider, Action<AccountResult> done)
            { LastProvider = provider; IsSignedIn = Next.Status == AccountStatus.SignedIn; done(Next); }
            public void ClaimUsername(string username, Action<UsernameCheck> done) { if (Claim == UsernameCheck.Available) Username = username; done(Claim); }
        }

        private void EnterBirthday(int year)
        {
            Click("Mar");
            Field("Day").text = "14"; Field("Year").text = year.ToString();
        }

        private bool Visible(string name) => root.GetComponentsInChildren<Transform>(false).Any(t => t.name == name && t.gameObject.activeInHierarchy);
        private Button Button(string name) => root.GetComponentsInChildren<Button>(false).First(b => b.name == name && b.gameObject.activeInHierarchy);
        private void Click(string name) => Button(name).onClick.Invoke();
        private InputField Field(string name) => root.GetComponentsInChildren<InputField>(false).First(f => f.name == name && f.gameObject.activeInHierarchy);
    }
}
#endif
