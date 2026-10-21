using System;
using System.Collections.Generic;
using Pockle.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// UX-050 first-run flow (owner, October 21): splash, birthday (13+ gate), sign in with Apple or Google (no guest mode),
    /// collector name, then a four-page picture-first tutorial that uses the real toys. Full screen, above every page.
    /// Accounts are Codex's runtime through <see cref="IAccountService"/>; until one is set, development builds offer
    /// "Continue (test build)" and release builds cannot get past sign-in.
    /// </summary>
    public sealed partial class PrototypeHud
    {
        private enum OnboardingStep { Splash, Birthday, SignIn, Username, Tutorial, Blocked }
        private const int TutorialPages = 4;

        private RectTransform onboarding, onboardingSafe;
        private Image onboardingGround;
        private readonly Dictionary<OnboardingStep, RectTransform> obSteps = new Dictionary<OnboardingStep, RectTransform>();
        private readonly RectTransform[] obPages = new RectTransform[TutorialPages];
        private readonly Image[] obDots = new Image[TutorialPages];
        private readonly List<KeyValuePair<RawImage, int>> obToys = new List<KeyValuePair<RawImage, int>>();
        private readonly List<string> obHeroes = new List<string>();
        private OnboardingStep obStep;
        private int obPage;
        private bool obTutorialOnly, obBusy, obToysReady;
        private IAccountService account;
        private readonly System.Random obRandom = new System.Random();

        // Birthday
        private readonly Button[] obMonths = new Button[12];
        private int obMonth;
        private InputField obDay, obYear, obName;
        private Text obBirthdayError, obSignInError, obNameError, obNextLabel;
        private Button obBirthdayNext, obApple, obGoogle, obTestContinue, obClaim;

        public bool OnboardingVisible => onboarding != null && onboarding.gameObject.activeSelf;

        /// <summary>Codex: call once the account runtime exists. Null keeps the test-build path.</summary>
        public void SetAccountService(IAccountService service)
        {
            account = service;
            if (obTestContinue != null) obTestContinue.gameObject.SetActive(TestBuildSignIn);
        }

        private bool TestBuildSignIn => account == null && (Debug.isDebugBuild || Application.isEditor);

        private static readonly Color[] PageFields = { PockleTheme.FieldGold, PockleTheme.Jelly, PockleTheme.FieldMint, PockleTheme.FieldMoon };

        private void BuildOnboarding()
        {
            onboarding = Rect("First-run onboarding", canvas.transform); Stretch(onboarding);
            onboardingGround = onboarding.gameObject.AddComponent<Image>(); onboardingGround.color = PockleTheme.Jelly;
            onboardingGround.raycastTarget = true; // nothing underneath can be touched
            onboardingSafe = Rect("Onboarding safe area", onboarding); Stretch(onboardingSafe);
            onboarding.gameObject.AddComponent<OnboardingSwipe>().Swiped = OnboardingSwiped;

            BuildSplash(); BuildBirthday(); BuildBlocked(); BuildSignIn(); BuildUsername(); BuildTutorial();
            onboarding.gameObject.SetActive(false);
        }

        // ----- Shared pieces -----

        private RectTransform ObStep(OnboardingStep step)
        {
            var root = Rect("Onboarding · " + step, onboardingSafe); Stretch(root); obSteps[step] = root; return root;
        }

        /// <summary>Anchors a rect to a fraction of its parent, inset by pixels (left, bottom, right, top).</summary>
        private static RectTransform Place(RectTransform rect, float x0, float y0, float x1, float y1, float l = 0, float b = 0, float r = 0, float t = 0)
        {
            rect.anchorMin = new Vector2(x0, y0); rect.anchorMax = new Vector2(x1, y1); rect.pivot = new Vector2(.5f, .5f);
            rect.offsetMin = new Vector2(l, b); rect.offsetMax = new Vector2(-r, -t); return rect;
        }

        private Text ObTitle(string value, Transform parent, float y0, float y1)
        {
            var text = Label(value, parent, PockleTheme.TitleSize, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            Place(text.rectTransform, 0, y0, 1, y1, 28, 0, 28, 0); return text;
        }

        private Text ObBody(string value, Transform parent, float y0, float y1, int size = PockleTheme.BodySize + 1)
        {
            var text = Label(value, parent, size, MutedInk, FontStyle.Normal, TextAnchor.UpperCenter, Vector2.zero, Vector2.zero);
            Place(text.rectTransform, 0, y0, 1, y1, 32, 0, 32, 0); return text;
        }

        /// <summary>The big action at the bottom of each step.</summary>
        private Button ObPrimary(string title, Transform parent, Action click, float bottom = 34)
        {
            var button = CreateButton(title, parent, Vector2.zero, new Vector2(300, 60), Primary, click, out _, PockleTheme.ButtonSize + 2);
            var rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0); rect.anchorMax = new Vector2(1, 0); rect.pivot = new Vector2(.5f, 0);
            rect.offsetMin = new Vector2(24, bottom); rect.offsetMax = new Vector2(-24, bottom + 60);
            return button;
        }

        private Text ObError(Transform parent, float y0, float y1)
        {
            var text = Label("", parent, PockleTheme.CaptionSize, PockleTheme.Heart, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            Place(text.rectTransform, 0, y0, 1, y1, 28, 0, 28, 0); return text;
        }

        /// <summary>A real toy, cut out, floating in a slot. <paramref name="hero"/> indexes the hero list filled on first show.</summary>
        private RectTransform ObToy(Transform parent, int hero, float cx, float cy, float size, float phase, float tilt = 4)
        {
            var slot = Rect("Toy slot", parent);
            slot.anchorMin = slot.anchorMax = new Vector2(cx, cy); slot.pivot = new Vector2(.5f, .5f); slot.sizeDelta = new Vector2(size, size);
            var motion = Rect("Float", slot); Stretch(motion);
            var bob = motion.gameObject.AddComponent<OnboardingFloat>(); bob.Phase = phase; bob.Tilt = tilt; bob.Amplitude = size * .045f;
            var raw = motion.gameObject.AddComponent<RawImage>(); raw.raycastTarget = false; raw.color = new Color(1, 1, 1, 0);
            obToys.Add(new KeyValuePair<RawImage, int>(raw, hero));
            return slot;
        }

        private RectTransform ObIcon(Transform parent, string icon, float cx, float cy, float size, Color colour, float phase = -1)
        {
            var slot = Rect(icon + " slot", parent);
            slot.anchorMin = slot.anchorMax = new Vector2(cx, cy); slot.pivot = new Vector2(.5f, .5f); slot.sizeDelta = new Vector2(size, size);
            var art = AddIcon(slot, icon, size, colour);
            if (phase >= 0) { var bob = art.gameObject.AddComponent<OnboardingFloat>(); bob.Phase = phase; bob.Amplitude = size * .08f; bob.Tilt = 10; }
            return slot;
        }

        private void ObPicture(Transform parent, Texture texture, float cx, float cy, float height, float aspect, float phase = -1)
        {
            if (texture == null) return;
            var slot = Rect("Picture", parent);
            slot.anchorMin = slot.anchorMax = new Vector2(cx, cy); slot.pivot = new Vector2(.5f, .5f); slot.sizeDelta = new Vector2(height * aspect, height);
            var raw = slot.gameObject.AddComponent<RawImage>(); raw.texture = texture; raw.raycastTarget = false;
            if (phase >= 0) { var bob = slot.gameObject.AddComponent<OnboardingFloat>(); bob.Phase = phase; bob.Amplitude = height * .02f; bob.Tilt = 2; }
        }

        /// <summary>A soft milk disc behind the hero art, so cut-outs read on every colour field.</summary>
        private void ObHalo(Transform parent, float cx, float cy, float size)
        {
            var disc = Rect("Halo", parent);
            disc.anchorMin = disc.anchorMax = new Vector2(cx, cy); disc.pivot = new Vector2(.5f, .5f); disc.sizeDelta = new Vector2(size, size);
            Surface(disc, new Color(1, 1, 1, .45f), size / 2);
        }

        // ----- Steps -----

        private void BuildSplash()
        {
            var root = ObStep(OnboardingStep.Splash);
            ObHalo(root, .5f, .70f, 300);
            var mark = Rect("Pockle wordmark", root);
            mark.anchorMin = mark.anchorMax = new Vector2(.5f, .70f); mark.pivot = new Vector2(.5f, .5f); mark.sizeDelta = new Vector2(84 * WordmarkAspect, 84);
            if (PockleTheme.Wordmark != null) { var image = mark.gameObject.AddComponent<RawImage>(); image.texture = PockleTheme.Wordmark; image.raycastTarget = false; }
            else { var word = Label("Pockle", mark, 64, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero); Stretch(word.rectTransform); }
            ObBody("Squishy little toys to find, play with and trade.", root, .54f, .61f, PockleTheme.HeadingSize - 2);
            // Five real toys in a gentle arc, each floating on its own beat.
            float[] xs = { .13f, .32f, .5f, .68f, .87f }, ys = { .36f, .31f, .29f, .31f, .36f }, sizes = { 104, 124, 150, 124, 104 };
            for (int i = 0; i < 5; i++) ObToy(root, i, xs[i], ys[i], sizes[i], i * .9f, i % 2 == 0 ? 5 : -5);
            ObPrimary("Let's go!", root, () => { ShowOnboardingStep(OnboardingState.AgeConfirmed ? AfterAge() : OnboardingStep.Birthday); });
        }

        private void BuildBirthday()
        {
            var root = ObStep(OnboardingStep.Birthday);
            ObHalo(root, .5f, .855f, 150);
            ObToy(root, 2, .5f, .855f, 150, .4f);
            ObIcon(root, "StickerStar", .68f, .92f, 34, PockleTheme.Heart, .2f);
            ObTitle("When's your birthday?", root, .70f, .77f);
            ObBody("We only use it to check Pockle is right for you. We don't save it.", root, .64f, .70f, PockleTheme.CaptionSize + 1);

            var months = Rect("Month", root); Place(months, 0, .40f, 1, .625f, 24, 0, 24, 0);
            string[] names = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            for (int i = 0; i < 12; i++)
            {
                int month = i + 1;
                obMonths[i] = CreateButton(names[i], months, Vector2.zero, new Vector2(60, 44), Paper, () => PickMonth(month), out _, PockleTheme.BodySize);
                var rect = obMonths[i].GetComponent<RectTransform>();
                int col = i % 4, row = i / 4;
                Place(rect, col / 4f, 1 - (row + 1) / 3f, (col + 1) / 4f, 1 - row / 3f, 4, 4, 4, 4);
            }
            var parts = Rect("Day and year", root); Place(parts, 0, .29f, 1, .38f, 24, 0, 24, 0);
            obDay = ObNumberField("Day", parts, 2, 0, .38f);
            obYear = ObNumberField("Year", parts, 4, .42f, 1);
            obDay.onValueChanged.AddListener(_ => RefreshBirthday()); obYear.onValueChanged.AddListener(_ => RefreshBirthday());
            obBirthdayError = ObError(root, .21f, .28f);
            obBirthdayNext = ObPrimary("Next", root, SubmitBirthday);
        }

        private InputField ObNumberField(string placeholder, Transform parent, int limit, float x0, float x1)
        {
            var rect = Rect(placeholder, parent); Place(rect, x0, 0, x1, 1, 4, 0, 4, 0);
            var background = Surface(rect, Paper, 18); background.raycastTarget = true;
            var text = Label("", rect, PockleTheme.HeadingSize + 3, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            text.resizeTextForBestFit = false; Stretch(text.rectTransform);
            var hint = Label(placeholder, rect, PockleTheme.HeadingSize, MutedInk, FontStyle.Normal, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            Stretch(hint.rectTransform);
            var field = rect.gameObject.AddComponent<InputField>(); field.targetGraphic = background;
            field.textComponent = text; field.placeholder = hint; field.characterLimit = limit;
            field.contentType = InputField.ContentType.IntegerNumber; field.lineType = InputField.LineType.SingleLine;
            field.keyboardType = TouchScreenKeyboardType.NumberPad;
            return field;
        }

        private void PickMonth(int month) { obMonth = month; RefreshBirthday(); }

        private bool TryBirthday(out int year, out int month, out int day)
        {
            month = obMonth;
            bool parsed = int.TryParse(obDay.text, out day) & int.TryParse(obYear.text, out year);
            return parsed && month > 0 && obYear.text.Length == 4;
        }

        private void RefreshBirthday()
        {
            for (int i = 0; i < 12; i++)
            {
                bool on = obMonth == i + 1;
                obMonths[i].targetGraphic.color = on ? Primary : Paper;
                obMonths[i].GetComponentInChildren<Text>().color = on ? PockleTheme.OnPlum : Ink;
            }
            obBirthdayNext.interactable = TryBirthday(out _, out _, out _);
            obBirthdayError.text = "";
        }

        private void SubmitBirthday()
        {
            if (!TryBirthday(out int year, out int month, out int day)) return;
            var today = DateTime.Today;
            if (!OnboardingRules.IsRealBirthday(today, year, month, day)) { obBirthdayError.text = "That date doesn't look quite right."; return; }
            // Only the result is kept. A failed check locks this phone, so a different date can't be tried.
            if (OnboardingRules.AgeOn(today, year, month, day) < OnboardingRules.MinimumAge) { OnboardingState.LockAge(); ShowOnboardingStep(OnboardingStep.Blocked); return; }
            OnboardingState.ConfirmAge();
            obDay.text = obYear.text = ""; obMonth = 0;
            ShowOnboardingStep(AfterAge());
        }

        private OnboardingStep AfterAge() => account != null && account.IsSignedIn ? OnboardingStep.Username : OnboardingStep.SignIn;

        private void BuildBlocked()
        {
            var root = ObStep(OnboardingStep.Blocked);
            ObHalo(root, .5f, .66f, 220);
            ObToy(root, 1, .5f, .66f, 200, 0, 2);
            ObIcon(root, "StickerZzz", .67f, .76f, 44, MutedInk, .5f);
            ObTitle("Pockle isn't for you yet", root, .43f, .51f);
            ObBody("Pockle is for people 13 and older. We'd love to see you when you're a little older!", root, .32f, .42f);
        }

        private void BuildSignIn()
        {
            var root = ObStep(OnboardingStep.SignIn);
            // A tiny shelf: three toys standing on a plank.
            var plank = Rect("Plank", root); plank.anchorMin = plank.anchorMax = new Vector2(.5f, .64f); plank.pivot = new Vector2(.5f, .5f);
            plank.sizeDelta = new Vector2(310, 16); Surface(plank, new Color(1, 1, 1, .7f), 8);
            ObToy(root, 3, .25f, .73f, 120, .3f); ObToy(root, 0, .5f, .745f, 140, 1.1f); ObToy(root, 4, .75f, .73f, 120, 2f);
            ObIcon(root, "Heart", .5f, .90f, 30, PockleTheme.Heart, .7f);
            ObTitle("Save your shelf", root, .49f, .57f);
            ObBody("Sign in so your toys follow you to any new phone.", root, .43f, .49f);
            obSignInError = ObError(root, .37f, .43f);
            bool apple = Application.platform == RuntimePlatform.IPhonePlayer;
            // Codex: swap these for the platform's official sign-in buttons when wiring the providers.
            obApple = ObProviderButton("Continue with Apple", root, Color.black, Color.white, apple ? 2 : 1, () => StartSignIn(AccountProvider.Apple));
            obGoogle = ObProviderButton("Continue with Google", root, Color.white, Ink, apple ? 1 : 2, () => StartSignIn(AccountProvider.Google));
            obTestContinue = CreateButton("Continue (test build)", root, Vector2.zero, new Vector2(240, 40), new Color(1, 1, 1, .35f), FinishTestSignIn, out _, PockleTheme.CaptionSize + 1);
            var test = obTestContinue.GetComponent<RectTransform>(); test.anchorMin = test.anchorMax = test.pivot = new Vector2(1, 1); test.anchoredPosition = new Vector2(-16, -16);
            obTestContinue.gameObject.SetActive(TestBuildSignIn);
            var terms = ObBody("By continuing you agree to Pockle's Terms and Privacy Policy.", root, .045f, .085f, PockleTheme.SmallSize);
            terms.alignment = TextAnchor.MiddleCenter;
        }

        /// <summary>Provider buttons stack above the terms; <paramref name="slot"/> 2 is the upper one.</summary>
        private Button ObProviderButton(string title, Transform parent, Color fill, Color ink, int slot, Action click)
        {
            var button = CreateButton(title, parent, Vector2.zero, new Vector2(300, 56), fill, click, out var face, PockleTheme.ButtonSize + 1);
            button.GetComponentInChildren<Text>().color = ink;
            if (fill == Color.white) { var outline = face.gameObject.AddComponent<Outline>(); outline.effectColor = PockleTheme.FillDeep; outline.effectDistance = new Vector2(1.5f, -1.5f); }
            var rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, .09f); rect.anchorMax = new Vector2(1, .09f); rect.pivot = new Vector2(.5f, 0);
            float y = 16 + (slot - 1) * 68;
            rect.offsetMin = new Vector2(24, y); rect.offsetMax = new Vector2(-24, y + 56);
            return button;
        }

        private void StartSignIn(AccountProvider provider)
        {
            if (obBusy) return;
            if (account == null) { obSignInError.text = "Sign-in isn't ready in this build yet."; return; }
            SetSignInBusy(true); obSignInError.text = "";
            account.SignIn(provider, result =>
            {
                SetSignInBusy(false);
                if (!OnboardingVisible) return;
                if (result.Status == AccountStatus.SignedIn) ShowOnboardingStep(result.HasUsername ? OnboardingStep.Tutorial : OnboardingStep.Username);
                else if (result.Status == AccountStatus.Failed) obSignInError.text = string.IsNullOrEmpty(result.Message) ? "Couldn't sign in. Try again." : result.Message;
            });
        }

        private void SetSignInBusy(bool busy)
        {
            obBusy = busy; obApple.interactable = !busy; obGoogle.interactable = !busy;
            if (busy) obSignInError.text = "Signing in…";
        }

        private void FinishTestSignIn() { if (TestBuildSignIn) ShowOnboardingStep(OnboardingStep.Username); }

        private void BuildUsername()
        {
            var root = ObStep(OnboardingStep.Username);
            ObHalo(root, .5f, .80f, 190);
            ObToy(root, 0, .5f, .80f, 180, .6f);
            ObIcon(root, "StickerSparkle", .70f, .88f, 34, PockleTheme.Heart, 1.2f);
            ObTitle("Pick a collector name", root, .61f, .68f);
            ObBody("Friends see it when you trade. Don't use your real name.", root, .55f, .61f, PockleTheme.CaptionSize + 1);
            var row = Rect("Name row", root); Place(row, 0, .44f, 1, .53f, 24, 0, 24, 0);
            var fieldRect = Rect("Collector name", row); Place(fieldRect, 0, 0, 1, 1, 0, 0, 64, 0);
            var background = Surface(fieldRect, Paper, 18); background.raycastTarget = true;
            var text = Label("", fieldRect, PockleTheme.HeadingSize + 1, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            text.resizeTextForBestFit = false; Stretch(text.rectTransform); text.rectTransform.offsetMin = new Vector2(18, 2); text.rectTransform.offsetMax = new Vector2(-12, -2);
            obName = fieldRect.gameObject.AddComponent<InputField>(); obName.targetGraphic = background; obName.textComponent = text;
            obName.characterLimit = OnboardingRules.UsernameMax; obName.lineType = InputField.LineType.SingleLine;
            obName.onValidateInput = (_, __, c) => c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c >= '0' && c <= '9' || c == '_' ? c : '\0';
            obName.onValueChanged.AddListener(_ => { obNameError.text = ""; obNameError.color = PockleTheme.Heart; });
            // Shuffle: a fresh friendly name.
            var shuffle = CreateButton("New idea", row, Vector2.zero, new Vector2(56, 56), Paper, () => obName.text = OnboardingRules.SuggestUsername(obRandom), out _, 12);
            IconOnly(shuffle, "StickerSparkle", 26);
            var shuffleRect = shuffle.GetComponent<RectTransform>(); shuffleRect.anchorMin = shuffleRect.anchorMax = shuffleRect.pivot = new Vector2(1, .5f);
            shuffleRect.anchoredPosition = Vector2.zero; shuffleRect.sizeDelta = new Vector2(56, 56);
            obNameError = ObError(root, .37f, .43f);
            obClaim = ObPrimary("That's me!", root, ClaimName);
        }

        private void ClaimName()
        {
            if (obBusy) return;
            string name = obName.text.Trim();
            string problem = OnboardingRules.UsernameProblem(name);
            if (problem != null) { obNameError.text = problem; return; }
            if (account == null) { AcceptName(name); return; }
            obBusy = true; obClaim.interactable = false; obNameError.color = MutedInk; obNameError.text = "Checking…";
            account.ClaimUsername(name, check =>
            {
                obBusy = false; obClaim.interactable = true; obNameError.color = PockleTheme.Heart;
                if (!OnboardingVisible) return;
                switch (check)
                {
                    case UsernameCheck.Available: AcceptName(name); break;
                    case UsernameCheck.Taken: obNameError.text = "That one's taken. Try another!"; break;
                    case UsernameCheck.NotAllowed: obNameError.text = "Let's pick a different name."; break;
                    default: obNameError.text = "Couldn't check that name. Try again."; break;
                }
            });
        }

        private void AcceptName(string name)
        {
            profile.SetName(name);
            if (profileName != null) profileName.SetTextWithoutNotify(profile.Name);
            ShowOnboardingStep(OnboardingStep.Tutorial);
        }

        private void BuildTutorial()
        {
            var root = ObStep(OnboardingStep.Tutorial);
            string[] titles = { "Open a box", "Fill your shelf", "Walk for free boxes", "Trade with friends" };
            string[] lines =
            {
                "Every box hides a surprise toy. Some are super rare!",
                "Your toys live on your shelf. Squish them, spin them, play!",
                "Walking unlocks a free box every few hours.",
                "Swap your spares with friends to finish a collection.",
            };
            for (int i = 0; i < TutorialPages; i++)
            {
                var page = obPages[i] = Rect("Tutorial page " + (i + 1), root); Stretch(page);
                var art = Rect("Picture", page); Place(art, 0, .36f, 1, .94f);
                BuildTutorialArt(i, art);
                ObTitle(titles[i], page, .27f, .35f);
                ObBody(lines[i], page, .19f, .27f);
            }
            var dots = Rect("Page dots", root); dots.anchorMin = dots.anchorMax = new Vector2(.5f, 0); dots.pivot = new Vector2(.5f, 0);
            dots.anchoredPosition = new Vector2(0, 112); dots.sizeDelta = new Vector2(TutorialPages * 22, 10);
            for (int i = 0; i < TutorialPages; i++)
            {
                var dot = Rect("Dot", dots); dot.anchorMin = dot.anchorMax = new Vector2(0, .5f); dot.pivot = new Vector2(0, .5f);
                dot.anchoredPosition = new Vector2(i * 22 + 6, 0); dot.sizeDelta = new Vector2(10, 10); obDots[i] = Surface(dot, Ink, 5);
            }
            var next = ObPrimary("Next", root, NextTutorialPage); obNextLabel = next.GetComponentInChildren<Text>();
            var skip = CreateButton("Skip", root, Vector2.zero, new Vector2(72, 44), new Color(1, 1, 1, .45f), FinishOnboarding, out _, PockleTheme.CaptionSize + 1);
            var skipRect = skip.GetComponent<RectTransform>(); skipRect.anchorMin = skipRect.anchorMax = skipRect.pivot = new Vector2(1, 1);
            skipRect.anchoredPosition = new Vector2(-16, -16);
        }

        private void BuildTutorialArt(int page, RectTransform art)
        {
            switch (page)
            {
                case 0: // A toy pops out of a real collection box.
                {
                    ObHalo(art, .5f, .46f, 280);
                    var shown = ShownCollections();
                    float aspect = 1;
                    var box = shown.Count > 0 ? CollectionLook.Box(shown[0], out aspect) : null;
                    if (box == null) { box = Resources.Load<Texture2D>("UI/Boxes/JellyGarden"); aspect = 380f / 342f; }
                    aspect = Mathf.Clamp(aspect, .3f, 3f);
                    ObToy(art, 0, .5f, .62f, 170, 0, 6);
                    ObPicture(art, box, .5f, .30f, 170, aspect);
                    ObIcon(art, "StickerSparkle", .26f, .72f, 40, PockleTheme.Heart, .3f);
                    ObIcon(art, "StickerStar", .76f, .64f, 34, Ink, 1.4f);
                    break;
                }
                case 1: // Three toys on a shelf.
                {
                    var plank = Rect("Plank", art); plank.anchorMin = plank.anchorMax = new Vector2(.5f, .30f); plank.pivot = new Vector2(.5f, .5f);
                    plank.sizeDelta = new Vector2(330, 18); Surface(plank, new Color(1, 1, 1, .75f), 9);
                    ObToy(art, 1, .22f, .47f, 130, .2f); ObToy(art, 2, .5f, .50f, 150, 1.4f); ObToy(art, 3, .78f, .47f, 130, 2.6f);
                    ObIcon(art, "Heart", .5f, .82f, 34, PockleTheme.Heart, .9f);
                    break;
                }
                case 2: // A toy out for a walk.
                {
                    ObHalo(art, .5f, .50f, 260);
                    ObToy(art, 4, .5f, .50f, 190, .5f, 9);
                    ObIcon(art, "Steps", .20f, .26f, 46, Ink, .1f);
                    ObIcon(art, "Steps", .80f, .74f, 40, Ink, 1.3f);
                    ObIcon(art, "Box", .80f, .26f, 46, PockleTheme.Heart, 2.1f);
                    break;
                }
                default: // Two toys swapping.
                {
                    ObToy(art, 0, .26f, .50f, 150, .4f); ObToy(art, 3, .74f, .50f, 150, 1.9f);
                    var right = ObIcon(art, "Next", .5f, .57f, 34, Ink);
                    var left = ObIcon(art, "Next", .5f, .43f, 34, Ink); left.localScale = new Vector3(-1, 1, 1);
                    ObIcon(art, "Friends", .5f, .82f, 40, PockleTheme.Heart, .8f);
                    break;
                }
            }
        }

        private void NextTutorialPage()
        {
            if (obPage >= TutorialPages - 1) { FinishOnboarding(); return; }
            ShowTutorialPage(obPage + 1);
        }

        private void ShowTutorialPage(int index)
        {
            obPage = Mathf.Clamp(index, 0, TutorialPages - 1);
            for (int i = 0; i < TutorialPages; i++)
            {
                obPages[i].gameObject.SetActive(i == obPage);
                obDots[i].color = i == obPage ? Ink : new Color(Ink.r, Ink.g, Ink.b, .22f);
                obDots[i].rectTransform.sizeDelta = new Vector2(i == obPage ? 10 : 8, i == obPage ? 10 : 8);
            }
            onboardingGround.color = PageFields[obPage];
            obNextLabel.text = obPage == TutorialPages - 1 ? (obTutorialOnly ? "Got it!" : "Start collecting!") : "Next";
        }

        private void OnboardingSwiped(int direction)
        {
            if (obStep != OnboardingStep.Tutorial) return;
            if (direction > 0) { if (obPage < TutorialPages - 1) ShowTutorialPage(obPage + 1); }
            else if (obPage > 0) ShowTutorialPage(obPage - 1);
        }

        // ----- Flow -----

        /// <summary>The app calls this after Initialize/Bind: shows the flow until this phone has finished it once. Scenes
        /// that build the HUD on their own (tests, captures) don't call it, so they never see onboarding.</summary>
        public void StartOnboardingIfNeeded()
        {
            if (OnboardingState.Completed) return;
            obTutorialOnly = false;
            ShowOnboardingStep(OnboardingState.AgeLocked ? OnboardingStep.Blocked : OnboardingStep.Splash);
        }

        /// <summary>Settings › How to play: the tutorial pages on their own; nothing about the account changes.</summary>
        private void ShowTutorialAgain()
        {
            obTutorialOnly = true; ShowOnboardingStep(OnboardingStep.Tutorial);
        }

        /// <summary>Settings (test builds): forget the first-run flow and start it again.</summary>
        private void RestartOnboardingForTesting()
        {
            OnboardingState.ResetForTesting(); obTutorialOnly = false; ShowOnboardingStep(OnboardingStep.Splash);
        }

        private void ShowOnboardingStep(OnboardingStep step)
        {
            obStep = step;
            onboarding.gameObject.SetActive(true);
            onboarding.SetAsLastSibling(); modal.SetAsLastSibling();
            foreach (var entry in obSteps) entry.Value.gameObject.SetActive(entry.Key == step);
            Color field = PockleTheme.Jelly;
            switch (step)
            {
                case OnboardingStep.Birthday: field = PockleTheme.FieldPeach; RefreshBirthday(); break;
                case OnboardingStep.SignIn: field = PockleTheme.FieldMoon; obSignInError.text = ""; obTestContinue.gameObject.SetActive(TestBuildSignIn); break;
                case OnboardingStep.Username:
                    field = PockleTheme.FieldMint;
                    if (string.IsNullOrEmpty(obName.text)) obName.text = profile.Name != "Collector" && OnboardingRules.UsernameProblem(profile.Name) == null ? profile.Name : OnboardingRules.SuggestUsername(obRandom);
                    obNameError.text = ""; break;
                case OnboardingStep.Blocked: field = PockleTheme.Fill; break;
            }
            onboardingGround.color = field;
            if (step == OnboardingStep.Tutorial) ShowTutorialPage(0);
            LayoutOnboarding();
        }

        private void FinishOnboarding()
        {
            if (!obTutorialOnly) OnboardingState.Complete();
            onboarding.gameObject.SetActive(false);
            if (!obTutorialOnly && page != AppPage.Home) SelectTab(AppPage.Home);
            obTutorialOnly = false;
        }

        /// <summary>The phone's Back inside onboarding. Returns false only on the splash, so the app can close.</summary>
        private bool OnboardingBack()
        {
            switch (obStep)
            {
                case OnboardingStep.Splash: case OnboardingStep.Blocked: return false;
                case OnboardingStep.Birthday: case OnboardingStep.SignIn: if (!obBusy) ShowOnboardingStep(OnboardingStep.Splash); return true;
                case OnboardingStep.Tutorial:
                    if (obPage > 0) ShowTutorialPage(obPage - 1);
                    else if (obTutorialOnly) FinishOnboarding();
                    return true;
                default: return true; // signed in: the name is next, there's no going back to sign-in
            }
        }

        /// <summary>Mirrors the HUD's safe area and fills in the toy portraits once, after the stage exists.</summary>
        private void LayoutOnboarding()
        {
            if (onboarding == null) return;
            onboardingSafe.anchorMin = safeRoot.anchorMin; onboardingSafe.anchorMax = safeRoot.anchorMax;
            onboardingSafe.offsetMin = onboardingSafe.offsetMax = Vector2.zero;
            // Small phones: shrink the art rather than letting it collide with the text.
            float scale = Mathf.Clamp(safeRoot.rect.height / 760f, .72f, 1.15f);
            foreach (var entry in obToys) entry.Key.transform.parent.localScale = Vector3.one * scale;
        }

        private void UpdateOnboarding()
        {
            if (!OnboardingVisible) return;
            OnboardingFloat.Calm = reducedMotionEnabled;
            if (obToysReady) return;
            obToysReady = true;
            obHeroes.Clear();
            foreach (var collection in ShownCollections())
            {
                if (obHeroes.Count >= 5) break;
                foreach (var id in collection.Members)
                    if (ToyCatalog.TryGetCollectible(id, out var toy) && toy.Available && !toy.IsSecret) { obHeroes.Add(id); break; }
            }
            foreach (var entry in obToys)
            {
                if (obHeroes.Count == 0) break;
                var texture = CutoutPortrait(obHeroes[entry.Value % obHeroes.Count]);
                entry.Key.texture = texture; entry.Key.color = texture != null ? Color.white : new Color(1, 1, 1, 0);
            }
            LayoutOnboarding();
        }
    }

    /// <summary>A gentle float and tilt for onboarding art. Holds still under Motion: Calm.</summary>
    internal sealed class OnboardingFloat : MonoBehaviour
    {
        public static bool Calm;
        public float Phase, Amplitude = 6, Tilt = 4, Speed = 1.6f;
        private RectTransform rect;
        private void Awake() { rect = (RectTransform)transform; }
        private void Update()
        {
            if (Calm) { rect.anchoredPosition = Vector2.zero; rect.localRotation = Quaternion.identity; return; }
            float t = Time.unscaledTime * Speed + Phase;
            rect.anchoredPosition = new Vector2(0, Mathf.Sin(t) * Amplitude);
            rect.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * .7f + 1) * Tilt);
        }
    }

    /// <summary>Reports a horizontal swipe: +1 for next (swipe left), -1 for back.</summary>
    internal sealed class OnboardingSwipe : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Action<int> Swiped;
        private Vector2 start;
        public void OnBeginDrag(PointerEventData data) { start = data.position; }
        public void OnDrag(PointerEventData data) { }
        public void OnEndDrag(PointerEventData data)
        {
            var delta = data.position - start;
            float threshold = Screen.width * .12f;
            if (Mathf.Abs(delta.x) > threshold && Mathf.Abs(delta.x) > Mathf.Abs(delta.y) * 1.5f) Swiped?.Invoke(delta.x < 0 ? 1 : -1);
        }
    }
}
