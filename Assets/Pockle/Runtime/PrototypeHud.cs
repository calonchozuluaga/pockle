using System;
using Pockle.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>Home hub and collection UI. Toy manipulation stays on the toy and its plate.</summary>
    public sealed partial class PrototypeHud : MonoBehaviour
    {
        // Short aliases for the UX-002 theme tokens used throughout the HUD partials.
        private static Color Ink => PockleTheme.Ink;
        private static Color MutedInk => PockleTheme.MutedInk;
        private static Color Peach => PockleTheme.Peach;
        private static Color Quiet => PockleTheme.Quiet;
        private static Color Paper => PockleTheme.Paper;
        private readonly MenuNavigation navigator = new MenuNavigation();
        private AppPage page => navigator.Current;
        public AppPage CurrentPage => page;
        private GuestProfile profile;
        public bool StoreVisible => page != AppPage.Play || modal != null && modal.gameObject.activeSelf;
        public event Action<PipVariant> VariantChanged;
        public event Action<bool> StoreVisibilityChanged;
        public event Action DailyBoxRequested;
        private RectTransform safeRoot, browsing, content, navigation, modal, modalSafeRoot, modalCard;
        private RectTransform brand;
        private Text heading, subtitle, playHint, modalTitle, modalText;
        private Button settingsButton, backButton;
        private Image homeTab, boxesTab, youTab;
        private RectTransform backIcon;
        private RectTransform shelfRoot, boxesRoot, homeRoot, rewardsRoot, socialRoot, profileRoot, settingsRoot;
        private Sprite roundedSprite;
        private Texture2D roundedTexture;
        private Canvas canvas;
        private CanvasScaler scaler;
        private GameObject ownedEventSystem;
        private Action<bool> soundCallback, hapticsCallback, motionCallback;
        private bool soundEnabled, hapticsEnabled, reducedMotionEnabled;
        private Rect lastSafeArea;
        private Vector2 lastScreenSize, lastLayoutSize;
        private PipVariant variant;
        private CollectionSession session;
        private readonly RenderTexture[] previews = new RenderTexture[4];
        private readonly Button[] toyButtons = new Button[4];
        private readonly Text[] toyCounts = new Text[4];
        private readonly RectTransform[] toyTiles = new RectTransform[4];
        private readonly RectTransform[] toyPlanks = new RectTransform[4];
        private readonly RectTransform[] toyPictures = new RectTransform[4];
        private readonly RectTransform[] toyNames = new RectTransform[4];
        private Text soundLabel, hapticsLabel, motionLabel;
        private bool initialized;

        public void Initialize(Action<bool> sound, Action<bool> haptics, Action<bool> reducedMotion, Action<float> volume = null)
        {
            if (initialized) return;
            initialized = true;
            profile = new GuestProfile();
            soundCallback = sound; hapticsCallback = haptics; motionCallback = reducedMotion; volumeCallback = volume;
            roundedSprite = CreateRoundedSprite();
            var root = new GameObject("Pockle · game UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 20;
            scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(390, 844); scaler.matchWidthOrHeight = 1;
            safeRoot = Rect("Safe area", root.transform); Stretch(safeRoot);
            brand = BuildWordmark(safeRoot);
            settingsButton = CreateButton("Settings", safeRoot, Vector2.zero, new Vector2(48, 48), Paper, ShowSettings, out _, 12);
            IconOnly(settingsButton, "Settings", 26);
            backButton = CreateButton("Shelf", safeRoot, new Vector2(22, 14), new Vector2(98, 48), Paper, () => GoBack(), out _, 14);
            backIcon = AddIcon(backButton.transform, "Back", 18, MutedInk);
            backIcon.anchorMin = backIcon.anchorMax = new Vector2(0, .5f); backIcon.pivot = new Vector2(0, .5f); backIcon.anchoredPosition = new Vector2(12, 0);
            var backLabel = backButton.GetComponentInChildren<Text>().rectTransform; backLabel.offsetMin = new Vector2(30, 2);
            heading = Label("Your little collection", safeRoot, PockleTheme.TitleSize, Ink, FontStyle.Bold, TextAnchor.MiddleCenter,
                new Vector2(0, -70), new Vector2(340, 34));
            heading.resizeTextMinSize = 14;
            subtitle = Label("Four Pips. Four little personalities.", safeRoot, 13, MutedInk, FontStyle.Normal,
                TextAnchor.MiddleCenter, new Vector2(0, -106), new Vector2(350, 26));
            browsing = Rect("Collection browsing", safeRoot);
            var scroll = browsing.gameObject.AddComponent<ScrollRect>(); scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped;
            var viewport = Rect("Viewport", browsing); Stretch(viewport); viewport.gameObject.AddComponent<RectMask2D>();
            var dragTarget = viewport.gameObject.AddComponent<Image>(); dragTarget.color = Color.clear;
            content = Rect("Collection content", viewport); content.anchorMin = new Vector2(0, 1); content.anchorMax = new Vector2(1, 1); content.pivot = new Vector2(.5f, 1);
            scroll.content = content; scroll.viewport = viewport;
            shelfRoot = Rect("Your shelf", content); boxesRoot = Rect("Walking and shop", content);
            homeRoot = Rect("Home hub", content); rewardsRoot = Rect("Rewards", content);
            socialRoot = Rect("Friends and discovery", content); profileRoot = Rect("Local profile", content);
            settingsRoot = Rect("Settings page", content);
            BuildShelf(); BuildStore(); BuildHome(); BuildRewards(); BuildProfile(); BuildSocial(); BuildSettings();
            navigation = Rect("Navigation", safeRoot); Card(navigation, Paper);
            BuildTab("Home", "Home", () => SelectTab(AppPage.Home), out homeTab);
            BuildTab("Boxes", "Box", () => SelectTab(AppPage.Boxes), out boxesTab);
            BuildTab("You", "You", () => SelectTab(AppPage.Profile), out youTab);
            playHint = Label("Make yourself at home.", safeRoot, 13, MutedInk, FontStyle.Normal, TextAnchor.MiddleCenter,
                Vector2.zero, new Vector2(350, 40));
            playHint.rectTransform.anchorMin = playHint.rectTransform.anchorMax = new Vector2(.5f, 0);
            playHint.rectTransform.pivot = new Vector2(.5f, 0); playHint.rectTransform.anchoredPosition = new Vector2(0, 20);
            BuildModal();
            if (EventSystem.current == null)
            {
                ownedEventSystem = new GameObject("Pockle event system", typeof(EventSystem), typeof(StandaloneInputModule));
                ownedEventSystem.transform.SetParent(transform, false);
            }
            ApplySafeArea(); Canvas.ForceUpdateCanvases(); AdaptLayout(); ApplyPage(true);
        }

        public void Bind(CollectionSession collection)
        {
            session = collection; session.Changed += RefreshCollection; RefreshCollection();
        }
        public void SetStatus(string message) { if (playHint != null) playHint.text = message; }
        public void SetRevealAvailable(bool available)
        { backButton.interactable = available; settingsButton.interactable = available; }
        public void SetVariant(PipVariant choice)
        {
            variant = PipVariants.FromSaved((int)choice);
            if (page == AppPage.Play) { heading.text = "Pip"; subtitle.text = PipVariants.Label(variant); }
        }
        public void ShowToy(PipVariant choice)
        {
            choice = PipVariants.FromSaved((int)choice);
            if (session != null && session.Progress.Save.Counts[(int)choice] <= 0) return;
            SetVariant(choice);
            if (page != AppPage.Shelf && page != AppPage.Play) Navigate(AppPage.Shelf);
            Navigate(AppPage.Play); VariantChanged?.Invoke(choice);
        }
        public bool GoBack()
        {
            if (modal.gameObject.activeSelf) { CloseModal(); return true; }
            if (!backButton.interactable) return true;
            var selected = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
            var editing = selected != null ? selected.GetComponent<InputField>() : null;
            if (TouchScreenKeyboard.visible || editing != null && editing.isFocused)
            {
                CommitProfile();
                if (editing != null) editing.DeactivateInputField();
                if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
                return true;
            }
            bool blocked = StoreVisible;
            SavePage();
            if (!navigator.Back()) return false;
            ApplyPage(blocked); return true;
        }
        private void Navigate(AppPage next)
        {
            bool blocked = StoreVisible; SavePage();
            if (navigator.Push(next)) ApplyPage(blocked);
        }
        private void SelectTab(AppPage next)
        {
            if (page == next) return;
            bool blocked = StoreVisible; SavePage(); navigator.SelectTab(next); ApplyPage(blocked);
        }
        private void SavePage()
        {
            navigator.SaveScroll(content.anchoredPosition.y);
            browsing.GetComponent<ScrollRect>().StopMovement();
            CommitProfile(); PlayerPrefs.Save();
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }
        private void ApplyPage(bool wasBlocked)
        {
            browsing.gameObject.SetActive(page != AppPage.Play);
            navigation.gameObject.SetActive(page != AppPage.Play);
            playHint.gameObject.SetActive(page == AppPage.Play);
            brand.gameObject.SetActive(page == AppPage.Home);
            backButton.gameObject.SetActive(page != AppPage.Home);
            backButton.GetComponentInChildren<Text>().text = page == AppPage.Play ? "Shelf" : "Back";
            shelfRoot.gameObject.SetActive(page == AppPage.Shelf); boxesRoot.gameObject.SetActive(page == AppPage.Boxes);
            homeRoot.gameObject.SetActive(page == AppPage.Home); rewardsRoot.gameObject.SetActive(page == AppPage.Rewards);
            socialRoot.gameObject.SetActive(page == AppPage.Social); profileRoot.gameObject.SetActive(page == AppPage.Profile);
            settingsRoot.gameObject.SetActive(page == AppPage.Settings);
            // The daily card is shared, so both destinations always show the same claim state.
            walkCard.SetParent(page == AppPage.Rewards ? rewardsRoot : boxesRoot, false);
            switch (page)
            {
                case AppPage.Home: heading.text = "Hey, " + profile.Name; subtitle.text = "A little wonder, waiting for you."; break;
                case AppPage.Shelf: heading.text = "Your little collection"; subtitle.text = "Tap a toy. Make yourself at home."; break;
                case AppPage.Rewards: heading.text = "Little walks, little rewards"; subtitle.text = "One daily Jelly Garden box. 1,000 steps."; break;
                case AppPage.Social: heading.text = "More shelves to explore"; subtitle.text = "Discover collectors and connect with friends."; break;
                case AppPage.Profile: heading.text = "Your corner of Pockle"; subtitle.text = "Make your local profile feel like you."; break;
                case AppPage.Settings: heading.text = "Make it yours"; subtitle.text = "Small comforts for your little companion."; break;
                case AppPage.Boxes: heading.text = "A little surprise awaits"; subtitle.text = "Walk for a box, or pick one to buy."; break;
                default: heading.text = "Pip"; subtitle.text = PipVariants.Label(variant); break;
            }
            StyleTab(homeTab, page == AppPage.Home || page == AppPage.Shelf || page == AppPage.Rewards || page == AppPage.Social);
            StyleTab(boxesTab, page == AppPage.Boxes);
            StyleTab(youTab, page == AppPage.Profile || page == AppPage.Settings);
            RefreshProfile(); RefreshSettings();
            content.anchoredPosition = new Vector2(0, navigator.Scroll);
            if (lastLayoutSize.x > 0) AdaptLayout();
            if (wasBlocked != StoreVisible) StoreVisibilityChanged?.Invoke(StoreVisible);
        }
        public void SetSettings(bool sound, bool haptics, bool reducedMotion)
        {
            soundEnabled = sound; hapticsEnabled = haptics; reducedMotionEnabled = reducedMotion;
            SquishFeedback.Calm = reducedMotion;
            if (soundLabel != null) soundLabel.text = sound ? "Sound on" : "Sound off";
            if (hapticsLabel != null) hapticsLabel.text = haptics ? "Haptics on" : "Haptics off";
            if (motionLabel != null) motionLabel.text = reducedMotion ? "Motion calm" : "Motion full";
            RefreshSettings();
        }
        private void BuildModal()
        {
            modal = Rect("Dialog backdrop", canvas.transform); Stretch(modal);
            var backdrop = modal.gameObject.AddComponent<Image>(); backdrop.color = PockleTheme.Backdrop;
            modalSafeRoot = Rect("Dialog safe area", modal); Stretch(modalSafeRoot);
            modalCard = Rect("Dialog", modalSafeRoot); modalCard.anchorMin = modalCard.anchorMax = new Vector2(.5f, .5f);
            modalCard.pivot = new Vector2(.5f, .5f); Card(modalCard, Paper);
            modalTitle = Label("", modalCard, 22, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0, -24), new Vector2(290, 36));
            modalText = Label("", modalCard, 14, MutedInk, FontStyle.Normal, TextAnchor.UpperCenter, new Vector2(0, -76), new Vector2(290, 132));
            var b = CreateButton("Done", modalCard, Vector2.zero, new Vector2(150, 48), Peach, CloseModal, out _);
            var close = b.GetComponent<RectTransform>(); close.anchorMin = close.anchorMax = new Vector2(.5f, 0);
            close.pivot = new Vector2(.5f, 0); close.anchoredPosition = new Vector2(0, 22);
            modal.gameObject.SetActive(false);
        }
        private void ShowSettings() { Navigate(AppPage.Settings); }
        private void ShowDialog(string title, string message)
        {
            bool wasBlocked = StoreVisible;
            modalTitle.text = title; modalText.text = message;
            LayoutModal();
            modal.gameObject.SetActive(true); modal.SetAsLastSibling();
            if (!wasBlocked) StoreVisibilityChanged?.Invoke(true);
        }
        private void CloseModal()
        {
            modal.gameObject.SetActive(false);
            if (page == AppPage.Play) StoreVisibilityChanged?.Invoke(false);
        }
        private void Update()
        {
            if (!initialized) return;
            if (lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height || lastSafeArea != Screen.safeArea) ApplySafeArea();
            if (lastLayoutSize != safeRoot.rect.size) AdaptLayout();
        }
        private void ApplySafeArea()
        {
            if (Screen.width < 1 || Screen.height < 1) return;
            lastScreenSize = new Vector2(Screen.width, Screen.height); lastSafeArea = Screen.safeArea;
            scaler.referenceResolution = (float)Screen.width / Screen.height > 1.15f ? new Vector2(844, 390) : new Vector2(390, 844);
            safeRoot.anchorMin = new Vector2(lastSafeArea.xMin / Screen.width, lastSafeArea.yMin / Screen.height);
            safeRoot.anchorMax = new Vector2(lastSafeArea.xMax / Screen.width, lastSafeArea.yMax / Screen.height);
            safeRoot.offsetMin = safeRoot.offsetMax = Vector2.zero;
            modalSafeRoot.anchorMin = safeRoot.anchorMin; modalSafeRoot.anchorMax = safeRoot.anchorMax;
            modalSafeRoot.offsetMin = modalSafeRoot.offsetMax = Vector2.zero;
        }
        private void AdaptLayout()
        {
            var size = safeRoot.rect.size; if (size.x < 1 || size.y < 1) return; lastLayoutSize = size;
            var settings = settingsButton.GetComponent<RectTransform>(); settings.anchorMin = settings.anchorMax = new Vector2(1, 1);
            settings.pivot = new Vector2(1, 1); settings.anchoredPosition = new Vector2(-18, -12); settings.sizeDelta = new Vector2(48, 48);
            float width = Mathf.Max(1, Mathf.Min(820, size.x - 32));
            heading.rectTransform.sizeDelta = new Vector2(width, 34);
            subtitle.rectTransform.sizeDelta = new Vector2(width, 26);
            float viewportHeight = Mathf.Max(30, size.y - 244);
            TopCentered(browsing, new Vector2(0, -148), new Vector2(width, viewportHeight));
            navigation.anchorMin = navigation.anchorMax = new Vector2(.5f, 0); navigation.pivot = new Vector2(.5f, 0);
            navigation.anchoredPosition = new Vector2(0, 16); navigation.sizeDelta = new Vector2(Mathf.Min(340, width), 64);
            float navWidth = navigation.sizeDelta.x;
            float tabWidth = (navWidth - 12 - 8) / 3;
            LeftLabel(homeTab.rectTransform, new Vector2(6, -6), new Vector2(tabWidth, 52));
            LeftLabel(boxesTab.rectTransform, new Vector2(6 + tabWidth + 4, -6), new Vector2(tabWidth, 52));
            LeftLabel(youTab.rectTransform, new Vector2(6 + (tabWidth + 4) * 2, -6), new Vector2(tabWidth, 52));
            playHint.rectTransform.sizeDelta = new Vector2(width, 40);
            float height;
            switch (page)
            {
                case AppPage.Home: height = LayoutHome(width); break;
                case AppPage.Boxes: height = LayoutStore(width); break;
                case AppPage.Rewards: height = LayoutRewards(width); break;
                case AppPage.Profile: height = LayoutProfile(width); break;
                case AppPage.Settings: height = LayoutSettings(width); break;
                case AppPage.Social: height = LayoutSocial(width); break;
                default: height = LayoutShelf(width); break;
            }
            content.sizeDelta = new Vector2(0, Mathf.Max(viewportHeight, height));
            content.anchoredPosition = new Vector2(0, Mathf.Clamp(content.anchoredPosition.y, 0, Mathf.Max(0, height - viewportHeight)));
            if (modal.gameObject.activeSelf) LayoutModal();
        }

        private void LayoutModal()
        {
            float width = Mathf.Max(1, Mathf.Min(340, lastLayoutSize.x - 24));
            float height = Mathf.Min(280, lastLayoutSize.y - 16);
            modalCard.sizeDelta = new Vector2(width, height);
            modalTitle.rectTransform.sizeDelta = new Vector2(width - 24, 36);
            modalText.rectTransform.sizeDelta = new Vector2(width - 32, Mathf.Max(44, height - 150));
        }
        private static void LeftLabel(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private Button CreateButton(string title, Transform parent, Vector2 position, Vector2 size,
            Color colour, Action click, out Image background, int fontSize = PockleTheme.ButtonSize, bool raised = true)
        {
            var rect = Rect(title, parent);
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(position.x, -position.y);
            rect.sizeDelta = size;
            background = Surface(rect, colour);
            background.raycastTarget = true;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            var colours = button.colors;
            colours.normalColor = Color.white;
            colours.highlightedColor = new Color(0.96f, 0.94f, 0.94f);
            colours.pressedColor = new Color(0.88f, 0.84f, 0.83f);
            colours.selectedColor = Color.white;
            colours.disabledColor = new Color(1f, 1f, 1f, 0.4f);
            colours.fadeDuration = 0.08f;
            button.colors = colours;
            button.onClick.AddListener(() => click?.Invoke());
            if (raised)
            {
                // A darker "lip" under the button gives it a soft, pressable candy shape.
                var lip = rect.gameObject.AddComponent<Shadow>();
                lip.effectColor = colour == Peach ? PockleTheme.PeachDeep : PockleTheme.ButtonLip;
                lip.effectDistance = PockleTheme.ButtonLipOffset; lip.useGraphicAlpha = true;
            }
            rect.gameObject.AddComponent<SquishFeedback>();
            var text = Label(title, rect, fontSize, Ink, FontStyle.Bold, TextAnchor.MiddleCenter,
                Vector2.zero, size);
            Stretch(text.rectTransform);
            text.rectTransform.offsetMin = new Vector2(4f, 2f);
            text.rectTransform.offsetMax = new Vector2(-4f, -2f);
            return button;
        }

        private Text Label(string value, Transform parent, int size, Color colour, FontStyle style,
            TextAnchor alignment, Vector2 position, Vector2 dimensions)
        {
            var rect = Rect(value, parent);
            TopCentered(rect, position, dimensions);
            var text = rect.gameObject.AddComponent<Text>();
            // Bold labels use the rounded display face; everything else uses the body face.
            text.font = style == FontStyle.Bold ? PockleTheme.DisplayFont : PockleTheme.BodyFont;
            text.text = value;
            text.fontSize = size;
            text.fontStyle = PockleTheme.HasBrandFonts ? FontStyle.Normal : style;
            // Shrink rather than clip when a translation, a long name, or a small phone runs out of room.
            text.resizeTextForBestFit = true;
            text.resizeTextMaxSize = size;
            text.resizeTextMinSize = Mathf.Min(size, Mathf.Max(PockleTheme.MinimumTextSize, size - 5));
            text.color = colour;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
            text.supportRichText = false;
            return text;
        }

        private Image Surface(RectTransform rect, Color colour)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = roundedSprite;
            image.type = Image.Type.Sliced;
            image.color = colour;
            image.raycastTarget = false;
            return image;
        }

        private static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        private static void TopCentered(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        /// <summary>A surface with a soft drop shadow, for cards and floating bars.</summary>
        private Image Card(RectTransform rect, Color colour)
        {
            var image = Surface(rect, colour);
            var shadow = rect.gameObject.AddComponent<Shadow>();
            shadow.effectColor = PockleTheme.CardShadow; shadow.effectDistance = PockleTheme.CardShadowOffset;
            shadow.useGraphicAlpha = true;
            return image;
        }

        private RectTransform BuildWordmark(Transform parent)
        {
            var mark = Rect("Pockle wordmark", parent);
            LeftLabel(mark, new Vector2(18, -12), new Vector2(136, 48));
            var art = PockleTheme.Wordmark;
            if (art != null)
            {
                var image = mark.gameObject.AddComponent<RawImage>(); image.texture = art; image.raycastTarget = false;
                float aspect = art.height > 0 ? (float)art.width / art.height : 2.8f;
                mark.sizeDelta = new Vector2(48 * aspect, 48);
            }
            else
            {
                var text = Label("Pockle", mark, 32, PockleTheme.PeachDeep, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
                Stretch(text.rectTransform);
            }
            return mark;
        }

        /// <summary>Adds a tinted icon from Resources/UI/Icons. Returns null when the icon is missing.</summary>
        private RectTransform AddIcon(Transform parent, string name, float size, Color colour)
        {
            var texture = PockleTheme.Icon(name);
            if (texture == null) return Rect(name + " icon (missing)", parent);
            var rect = Rect(name + " icon", parent);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.sizeDelta = new Vector2(size, size);
            var image = rect.gameObject.AddComponent<RawImage>(); image.texture = texture; image.color = colour; image.raycastTarget = false;
            return rect;
        }

        /// <summary>Shows an icon instead of the caption. The caption stays on the button (disabled) as its name.</summary>
        private void IconOnly(Button button, string icon, float size)
        {
            if (PockleTheme.Icon(icon) == null) return;
            button.GetComponentInChildren<Text>().enabled = false;
            AddIcon(button.transform, icon, size, Ink);
        }

        private void BuildTab(string title, string icon, Action click, out Image background)
        {
            var button = CreateButton(title, navigation, Vector2.zero, new Vector2(100, 52), Paper, click, out background, 12, false);
            var label = button.GetComponentInChildren<Text>().rectTransform;
            label.anchorMin = new Vector2(0, 0); label.anchorMax = new Vector2(1, 0); label.pivot = new Vector2(.5f, 0);
            label.offsetMin = new Vector2(4, 5); label.offsetMax = new Vector2(-4, 23);
            var glyph = AddIcon(button.transform, icon, 24, MutedInk);
            glyph.anchorMin = glyph.anchorMax = new Vector2(.5f, 1); glyph.pivot = new Vector2(.5f, 1); glyph.anchoredPosition = new Vector2(0, -6);
        }

        private void StyleTab(Image tab, bool selected)
        {
            tab.color = selected ? Peach : Paper;
            var tint = selected ? Ink : MutedInk;
            tab.GetComponentInChildren<Text>().color = tint;
            var glyph = tab.GetComponentInChildren<RawImage>();
            if (glyph != null) glyph.color = tint;
        }

        private Sprite CreateRoundedSprite()
        {
            const int size = 64;
            const float radius = 15f;
            roundedTexture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Pockle · generated rounded UI surface",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var colours = new Color32[size * size];
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var centreX = Mathf.Clamp(x + 0.5f, radius, size - radius);
                var centreY = Mathf.Clamp(y + 0.5f, radius, size - radius);
                var distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(centreX, centreY));
                colours[y * size + x] = new Color32(255, 255, 255, (byte)(255f * Mathf.Clamp01(radius - distance + 0.5f)));
            }
            roundedTexture.SetPixels32(colours);
            roundedTexture.Apply(false, true);
            return Sprite.Create(roundedTexture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f),
                100f, 0, SpriteMeshType.FullRect, new Vector4(16f, 16f, 16f, 16f));
        }

        private void OnApplicationPause(bool paused)
        { if (paused) { CommitProfile(); PlayerPrefs.Save(); } }
        private void OnApplicationFocus(bool focused)
        { if (!focused) { CommitProfile(); PlayerPrefs.Save(); } }
        private void OnDestroy()
        {
            CommitProfile();
            if (session != null) session.Changed -= RefreshCollection;
            foreach (var preview in previews) if (preview != null) { preview.Release(); Destroy(preview); }
            if (roundedSprite != null) Destroy(roundedSprite);
            if (roundedTexture != null) Destroy(roundedTexture);
            if (canvas != null) Destroy(canvas.gameObject);
            if (ownedEventSystem != null) Destroy(ownedEventSystem);
        }
    }
}
