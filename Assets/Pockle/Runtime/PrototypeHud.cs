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
        // Short aliases for the v2 theme tokens used throughout the HUD partials.
        private static Color Ink => PockleTheme.Plum;
        private static Color MutedInk => PockleTheme.PlumSoft;
        private static Color Primary => PockleTheme.Plum;
        private static Color Quiet => PockleTheme.Fill;
        private static Color Paper => PockleTheme.Milk;
        private readonly MenuNavigation navigator = new MenuNavigation();
        private AppPage page => navigator.Current;
        public AppPage CurrentPage => page;
        private GuestProfile profile;
        public bool StoreVisible => page != AppPage.Play || modal != null && modal.gameObject.activeSelf;
        public event Action<PipVariant> VariantChanged;
        public event Action<bool> StoreVisibilityChanged;
        public event Action DailyBoxRequested;
        public event Action<Color> BackdropChanged;
        private RectTransform safeRoot, browsing, content, navigation, modal, modalSafeRoot, modalCard;
        private RectTransform brand, avatarButton, pageGround;
        private RawImage avatarPortrait;
        private Button favoriteToggle;
        private RawImage favoriteGlyph;
        private Text heading, subtitle, playHint, modalTitle, modalText;
        private Button settingsButton, backButton;
        private Image homeTab, shelfTab, boxesTab, youTab;
        private RectTransform backIcon;
        private RectTransform shelfRoot, boxesRoot, homeRoot, rewardsRoot, socialRoot, profileRoot, settingsRoot, collectionsRoot, collectionRoot;
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
        private RectTransform playHintPill;
        private bool initialized;
        private const float WordmarkAspect = 824f / 295f;

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
            // Menus sit on a milk ground; toy play shows the 3D stage instead.
            pageGround = Rect("Page ground", root.transform); Stretch(pageGround);
            var ground = pageGround.gameObject.AddComponent<Image>(); ground.color = Paper; ground.raycastTarget = false;
            safeRoot = Rect("Safe area", root.transform); Stretch(safeRoot);
            brand = BuildWordmark(safeRoot);
            avatarButton = BuildAvatarButton(safeRoot);
            settingsButton = CreateButton("Settings", safeRoot, Vector2.zero, new Vector2(44, 44), Quiet, ShowSettings, out _, 12);
            IconOnly(settingsButton, "Settings", 22);
            backButton = CreateButton("Back", safeRoot, Vector2.zero, new Vector2(44, 44), Quiet, () => GoBack(), out _, 12);
            IconOnly(backButton, "Back", 20);
            favoriteToggle = CreateButton("Favorite", safeRoot, Vector2.zero, new Vector2(44, 44), new Color(1, 1, 1, .6f), ToggleFavorite, out _, 12);
            IconOnly(favoriteToggle, "Heart", 20);
            favoriteGlyph = favoriteToggle.GetComponentInChildren<RawImage>();
            heading = Label("Your shelf", safeRoot, PockleTheme.TitleSize, Ink, FontStyle.Bold, TextAnchor.MiddleLeft,
                Vector2.zero, new Vector2(340, 40));
            heading.resizeTextMinSize = 20;
            subtitle = Label("", safeRoot, PockleTheme.BodySize, MutedInk, FontStyle.Normal,
                TextAnchor.UpperLeft, Vector2.zero, new Vector2(350, 44));
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
            collectionsRoot = Rect("Collections page", content); collectionRoot = Rect("Collection lineup", content);
            BuildShelf(); BuildStore(); BuildHome(); BuildRewards(); BuildProfile(); BuildSocial(); BuildSettings(); BuildCollections();
            navigation = Rect("Navigation", safeRoot); Surface(navigation, Primary, 32);
            BuildTab("Home", "Home", () => SelectTab(AppPage.Home), out homeTab);
            BuildTab("Shelf", "Shelf", () => SelectTab(AppPage.Shelf), out shelfTab);
            BuildTab("Boxes", "Box", () => SelectTab(AppPage.Boxes), out boxesTab);
            BuildTab("You", "You", () => SelectTab(AppPage.Profile), out youTab);
            // Play hint: a soft milk pill near the bottom, over the toy's stage.
            playHintPill = Rect("Ways to play", safeRoot); Surface(playHintPill, new Color(1, 1, 1, .55f), 20);
            playHintPill.anchorMin = playHintPill.anchorMax = playHintPill.pivot = new Vector2(.5f, 0);
            playHintPill.anchoredPosition = new Vector2(0, 28);
            playHint = Label("Press to squish. Drag up to lift. Pinch to stretch.", playHintPill, PockleTheme.SmallSize, Ink, FontStyle.Bold,
                TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            Stretch(playHint.rectTransform); playHint.rectTransform.offsetMin = new Vector2(14, 2); playHint.rectTransform.offsetMax = new Vector2(-14, -2);
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
        { backButton.interactable = available; settingsButton.interactable = available; favoriteToggle.interactable = available; }
        public void SetVariant(PipVariant choice)
        {
            variant = PipVariants.FromSaved((int)choice);
            if (page == AppPage.Play)
            {
                heading.text = ToyName(variant); subtitle.text = SeriesLine(variant); RefreshFavoriteToggle();
                BackdropChanged?.Invoke(PockleTheme.FieldFor(PipVariants.CollectibleId(variant)));
            }
        }
        public void ShowToy(PipVariant choice)
        {
            choice = PipVariants.FromSaved((int)choice);
            if (session != null && Owned(choice) <= 0) return;
            SetVariant(choice);
            if (page != AppPage.Shelf && page != AppPage.Collection && page != AppPage.Play) Navigate(AppPage.Shelf);
            Navigate(AppPage.Play); VariantChanged?.Invoke(choice);
        }
        /// <summary>Plays an owned collectible by stable catalog ID. Unknown, planned, or unowned IDs do nothing.</summary>
        public bool ShowToy(string collectibleId)
        {
            if (session == null || !session.TryGetPlayableVariant(collectibleId, out var choice) || Owned(choice) <= 0) return false;
            ShowToy(choice);
            return page == AppPage.Play;
        }

        /// <summary>Owned count for one of the four playable Pip presets, read from ID inventory.</summary>
        private int Owned(PipVariant variant)
        {
            return session == null ? 0 : session.GetOwnedCount(PipVariants.CollectibleId(variant));
        }

        /// <summary>Discovered playable finishes and total owned toys (including preserved future IDs).</summary>
        private void CollectionTotals(out int discovered, out int total)
        {
            discovered = 0; total = 0;
            if (session == null) return;
            foreach (var entry in session.OwnedCounts) if (entry.Value > 0) total += entry.Value;
            for (int i = 0; i < PipVariants.Count; i++) if (Owned((PipVariant)i) > 0) discovered++;
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
            bool blocked = StoreVisible; SavePage();
            // MenuNavigation's tab roots are Home, Boxes, and You. The Shelf tab is Home's shelf with history reset,
            // which gives the same stack a tab would: Back from Shelf returns Home.
            if (next == AppPage.Shelf) { navigator.SelectTab(AppPage.Home); navigator.Push(AppPage.Shelf); }
            else navigator.SelectTab(next);
            ApplyPage(blocked);
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
            bool play = page == AppPage.Play;
            browsing.gameObject.SetActive(!play);
            pageGround.gameObject.SetActive(!play);
            // Tab pages show the tab bar; pages reached from them show a back button instead.
            bool tabRoot = page == AppPage.Home || page == AppPage.Shelf || page == AppPage.Boxes || page == AppPage.Profile;
            navigation.gameObject.SetActive(!play);
            playHintPill.gameObject.SetActive(play);
            // On the toy's stage, header buttons are translucent milk; on pages they're the neutral fill.
            backButton.GetComponent<Image>().color = play ? new Color(1, 1, 1, .6f) : Quiet;
            brand.gameObject.SetActive(page == AppPage.Home);
            avatarButton.gameObject.SetActive(page == AppPage.Home);
            settingsButton.gameObject.SetActive(page == AppPage.Profile);
            favoriteToggle.gameObject.SetActive(play);
            backButton.gameObject.SetActive(!tabRoot);
            shelfRoot.gameObject.SetActive(page == AppPage.Shelf); boxesRoot.gameObject.SetActive(page == AppPage.Boxes);
            homeRoot.gameObject.SetActive(page == AppPage.Home); rewardsRoot.gameObject.SetActive(page == AppPage.Rewards);
            socialRoot.gameObject.SetActive(page == AppPage.Social); profileRoot.gameObject.SetActive(page == AppPage.Profile);
            settingsRoot.gameObject.SetActive(page == AppPage.Settings);
            collectionsRoot.gameObject.SetActive(page == AppPage.Collections); collectionRoot.gameObject.SetActive(page == AppPage.Collection);
            // The daily card is shared, so both destinations always show the same claim state.
            walkCard.SetParent(page == AppPage.Rewards ? rewardsRoot : boxesRoot, false);
            switch (page)
            {
                case AppPage.Home: heading.text = Greeting() + ", " + profile.Name; subtitle.text = HomeLine(); break;
                case AppPage.Shelf: heading.text = "Your shelf"; subtitle.text = ShelfLine(); break;
                case AppPage.Rewards: heading.text = "Rewards"; subtitle.text = "Walk 1,000 steps for a free box every day."; break;
                case AppPage.Social: heading.text = "Friends"; subtitle.text = ""; break;
                case AppPage.Profile: heading.text = ""; subtitle.text = ""; break;
                case AppPage.Settings: heading.text = "Settings"; subtitle.text = ""; break;
                case AppPage.Boxes: heading.text = "Boxes"; subtitle.text = "Walk for a free box every day, or pick one to buy."; break;
                case AppPage.Collections: heading.text = "Collections"; subtitle.text = "Meet the little characters for your shelf."; break;
                case AppPage.Collection:
                    heading.text = SelectedCollection.DisplayName; subtitle.text = CollectionCaption(SelectedCollection); break;
                default: heading.text = ToyName(variant); subtitle.text = SeriesLine(variant); break;
            }
            RefreshFavoriteToggle(); RefreshAvatarButton();
            StyleTab(homeTab, page == AppPage.Home || page == AppPage.Rewards || page == AppPage.Social);
            // Collection browsing lives under the Shelf tab.
            StyleTab(shelfTab, page == AppPage.Shelf || page == AppPage.Collections || page == AppPage.Collection);
            StyleTab(boxesTab, page == AppPage.Boxes);
            StyleTab(youTab, page == AppPage.Profile || page == AppPage.Settings);
            RefreshShelf(); RefreshProfile(); RefreshSettings(); RefreshWalkingCard(); RefreshHome(); RefreshCollections();
            content.anchoredPosition = new Vector2(0, navigator.Scroll);
            if (lastLayoutSize.x > 0) AdaptLayout();
            BackdropChanged?.Invoke(play ? PockleTheme.FieldFor(PipVariants.CollectibleId(variant)) : PrototypeStage.Background);
            if (wasBlocked != StoreVisible) StoreVisibilityChanged?.Invoke(StoreVisible);
        }
        public void SetSettings(bool sound, bool haptics, bool reducedMotion)
        {
            soundEnabled = sound; hapticsEnabled = haptics; reducedMotionEnabled = reducedMotion;
            SquishFeedback.Calm = reducedMotion;
            RefreshSettings();
        }
        private void BuildModal()
        {
            modal = Rect("Dialog backdrop", canvas.transform); Stretch(modal);
            var backdrop = modal.gameObject.AddComponent<Image>(); backdrop.color = PockleTheme.Backdrop;
            modalSafeRoot = Rect("Dialog safe area", modal); Stretch(modalSafeRoot);
            modalCard = Rect("Dialog", modalSafeRoot); modalCard.anchorMin = modalCard.anchorMax = new Vector2(.5f, .5f);
            modalCard.pivot = new Vector2(.5f, .5f); Card(modalCard, Paper);
            modalTitle = Label("", modalCard, 24, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0, -24), new Vector2(290, 36));
            modalText = Label("", modalCard, 14, MutedInk, FontStyle.Normal, TextAnchor.UpperCenter, new Vector2(0, -76), new Vector2(290, 132));
            var b = CreateButton("Done", modalCard, Vector2.zero, new Vector2(150, 52), Primary, CloseModal, out _);
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
            const float gutter = PockleTheme.Gutter;
            float width = Mathf.Max(1, Mathf.Min(560, size.x - gutter * 2));
            float left = (size.x - width) / 2;
            // Header row: wordmark or back on the left; avatar, settings, or favorite on the right.
            LeftLabel(backButton.GetComponent<RectTransform>(), new Vector2(left, -16), new Vector2(44, 44));
            LeftLabel(brand, new Vector2(left, -14), brand.sizeDelta);
            foreach (var right in new[] { avatarButton, settingsButton.GetComponent<RectTransform>(), favoriteToggle.GetComponent<RectTransform>() })
            { right.anchorMin = right.anchorMax = right.pivot = new Vector2(1, 1); right.anchoredPosition = new Vector2(-left, -16); right.sizeDelta = new Vector2(44, 44); }
            // Title block: left-aligned, under the header row. You has no title (its profile header is the title).
            bool titled = !string.IsNullOrEmpty(heading.text);
            bool subtitled = !string.IsNullOrEmpty(subtitle.text);
            LeftLabel(heading.rectTransform, new Vector2(left, -74), new Vector2(width, 40));
            LeftLabel(subtitle.rectTransform, new Vector2(left, -116), new Vector2(width, 44));
            heading.gameObject.SetActive(titled); subtitle.gameObject.SetActive(subtitled);
            float top = !titled ? 68 : subtitled ? 156 : 124;
            bool tabs = navigation.gameObject.activeSelf;
            float viewportHeight = Mathf.Max(30, size.y - top - (tabs ? 0 : 0));
            TopCentered(browsing, new Vector2(0, -top), new Vector2(width, viewportHeight));
            // Floating plum tab bar; the scroll area runs beneath it with bottom padding.
            navigation.anchorMin = navigation.anchorMax = new Vector2(.5f, 0); navigation.pivot = new Vector2(.5f, 0);
            navigation.anchoredPosition = new Vector2(0, 20); navigation.sizeDelta = new Vector2(Mathf.Min(390 - gutter * 2, width), 68);
            float navWidth = navigation.sizeDelta.x;
            // Four tabs, each an icon over its label.
            var tabButtons = new[] { homeTab, shelfTab, boxesTab, youTab };
            float tabWidth = (navWidth - 12 - 4 * (tabButtons.Length - 1)) / tabButtons.Length;
            for (int i = 0; i < tabButtons.Length; i++)
                LeftLabel(tabButtons[i].rectTransform, new Vector2(6 + i * (tabWidth + 4), -6), new Vector2(tabWidth, 56));
            playHintPill.sizeDelta = new Vector2(Mathf.Min(width, 360), 40);
            float height;
            switch (page)
            {
                case AppPage.Home: height = LayoutHome(width); break;
                case AppPage.Boxes: height = LayoutStore(width); break;
                case AppPage.Rewards: height = LayoutRewards(width); break;
                case AppPage.Profile: height = LayoutProfile(width); break;
                case AppPage.Settings: height = LayoutSettings(width); break;
                case AppPage.Social: height = LayoutSocial(width); break;
                case AppPage.Collections: height = LayoutCollections(width); break;
                case AppPage.Collection: height = LayoutCollection(width); break;
                default: height = LayoutShelf(width); break;
            }
            // Leave room to scroll the last row clear of the floating tab bar.
            height += tabs ? 108 : 24;
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
            Color colour, Action click, out Image background, int fontSize = PockleTheme.ButtonSize, bool pill = true)
        {
            var rect = Rect(title, parent);
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(position.x, -position.y);
            rect.sizeDelta = size;
            // Pills by default: the corner radius is half the button's height.
            background = Surface(rect, colour, pill ? size.y / 2 : PockleTheme.TileRadius);
            background.raycastTarget = true;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = background;
            var colours = button.colors;
            colours.normalColor = Color.white;
            colours.highlightedColor = new Color(0.97f, 0.95f, 0.96f);
            colours.pressedColor = new Color(0.86f, 0.82f, 0.85f);
            colours.selectedColor = Color.white;
            colours.disabledColor = new Color(1f, 1f, 1f, 0.4f);
            colours.fadeDuration = 0.08f;
            button.colors = colours;
            button.onClick.AddListener(() => click?.Invoke());
            rect.gameObject.AddComponent<SquishFeedback>();
            var text = Label(title, rect, fontSize, InkOn(colour), FontStyle.Bold, TextAnchor.MiddleCenter,
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
            // Bold: large sizes use the display weight, smaller ones the label weight. Everything else uses body.
            text.font = style == FontStyle.Bold ? (size >= 24 ? PockleTheme.DisplayFont : PockleTheme.LabelFont) : PockleTheme.BodyFont;
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

        private Image Surface(RectTransform rect, Color colour, float radius = PockleTheme.TileRadius)
        {
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = roundedSprite;
            image.type = Image.Type.Sliced;
            SetRadius(image, radius);
            image.color = colour;
            image.raycastTarget = false;
            return image;
        }

        /// <summary>The sprite's corners are 64 px; scaling pixels-per-unit sets the drawn corner radius.</summary>
        private static void SetRadius(Image image, float radius)
        {
            image.pixelsPerUnitMultiplier = 64f / Mathf.Max(1f, radius);
        }

        /// <summary>Readable text colour on a fill: light on plum and midnight, plum on everything else.</summary>
        private static Color InkOn(Color fill)
        {
            return fill == PockleTheme.Plum || fill == PockleTheme.Midnight ? PockleTheme.OnPlum : PockleTheme.Plum;
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

        /// <summary>A large flat surface (v2 has no drop shadows).</summary>
        private Image Card(RectTransform rect, Color colour)
        {
            return Surface(rect, colour, PockleTheme.CardRadius);
        }

        private RectTransform BuildWordmark(Transform parent)
        {
            var mark = Rect("Pockle wordmark", parent);
            LeftLabel(mark, new Vector2(20, -14), new Vector2(120, 40));
            var art = PockleTheme.Wordmark;
            if (art != null)
            {
                var image = mark.gameObject.AddComponent<RawImage>(); image.texture = art; image.raycastTarget = false;
                // Use the source artwork's proportions (824 x 295). The imported texture can be resized to a power of two,
                // so its width/height can't be trusted for the aspect.
                mark.sizeDelta = new Vector2(40 * WordmarkAspect, 40);
            }
            else
            {
                var text = Label("Pockle", mark, 32, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
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

        /// <summary>A shared toy portrait (rendered once on its colour field) inside a parent.</summary>
        private RawImage Portrait(Transform parent, int index)
        {
            var rect = Rect("Portrait", parent);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            var raw = rect.gameObject.AddComponent<RawImage>(); raw.texture = previews[index]; raw.raycastTarget = false;
            return raw;
        }

        /// <summary>A horizontal fill inside a progress track.</summary>
        private Image ProgressFill(RectTransform track, Color colour)
        {
            var fill = Rect("Progress", track); Stretch(fill);
            var image = fill.gameObject.AddComponent<Image>(); image.sprite = roundedSprite; image.color = colour;
            image.type = Image.Type.Filled; image.fillMethod = Image.FillMethod.Horizontal; image.fillOrigin = 0;
            image.raycastTarget = false; image.fillAmount = 0;
            return image;
        }

        /// <summary>A section heading, such as "Your shelf" or "Favorite".</summary>
        private RectTransform Heading(string title, Transform parent)
        {
            return Label(title, parent, PockleTheme.HeadingSize, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(300, 30)).rectTransform;
        }

        /// <summary>A flat 60px row: icon, title, a detail line, and an optional chevron.</summary>
        private Button RowButton(string title, string detail, string icon, Transform parent, Action click, out Text detailText, bool chevron)
        {
            var button = CreateButton(title, parent, Vector2.zero, new Vector2(170, 60), Quiet, click, out var face, 15, false);
            SetRadius(face, PockleTheme.RowRadius);
            var label = button.GetComponentInChildren<Text>();
            label.alignment = TextAnchor.LowerLeft;
            label.rectTransform.offsetMin = new Vector2(46, 30); label.rectTransform.offsetMax = new Vector2(chevron ? -36 : -10, -8);
            var glyph = AddIcon(button.transform, icon, 22, Ink);
            glyph.anchorMin = glyph.anchorMax = glyph.pivot = new Vector2(0, .5f); glyph.anchoredPosition = new Vector2(14, 0);
            detailText = Label(detail, button.transform, PockleTheme.SmallSize, MutedInk, FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
            Stretch(detailText.rectTransform);
            detailText.rectTransform.offsetMin = new Vector2(46, 8); detailText.rectTransform.offsetMax = new Vector2(chevron ? -36 : -10, -31);
            if (chevron)
            {
                var next = AddIcon(button.transform, "Next", 18, MutedInk);
                next.anchorMin = next.anchorMax = next.pivot = new Vector2(1, .5f); next.anchoredPosition = new Vector2(-14, 0);
            }
            return button;
        }

        private void BuildTab(string title, string icon, Action click, out Image background)
        {
            var button = CreateButton(title, navigation, Vector2.zero, new Vector2(80, 56), PockleTheme.Jelly, click, out background, PockleTheme.SmallSize);
            SetRadius(background, 22);
            // Icon above, label below, so four tabs fit the bar.
            var glyph = AddIcon(button.transform, icon, 22, Ink);
            glyph.anchorMin = glyph.anchorMax = glyph.pivot = new Vector2(.5f, 1);
            glyph.anchoredPosition = new Vector2(0, -8);
            var label = button.GetComponentInChildren<Text>();
            var lr = label.rectTransform; lr.anchorMin = new Vector2(0, 0); lr.anchorMax = new Vector2(1, 0); lr.pivot = new Vector2(.5f, 0);
            lr.offsetMin = new Vector2(2, 7); lr.offsetMax = new Vector2(-2, 25);
        }

        private void StyleTab(Image tab, bool selected)
        {
            // Selected: a jelly-pink pill with plum ink. Others: no fill, soft light ink on the plum bar.
            tab.color = selected ? PockleTheme.Jelly : new Color(1, 1, 1, 0);
            var tint = selected ? Ink : PockleTheme.OnPlumMuted;
            tab.GetComponentInChildren<Text>().color = tint;
            var glyph = tab.GetComponentInChildren<RawImage>();
            if (glyph != null) glyph.color = tint;
        }

        /// <summary>Round avatar on Home that opens the profile.</summary>
        private RectTransform BuildAvatarButton(Transform parent)
        {
            var button = CreateButton("Your profile", parent, Vector2.zero, new Vector2(44, 44), PockleTheme.FieldPeach,
                () => SelectTab(AppPage.Profile), out var face, 12);
            button.GetComponentInChildren<Text>().enabled = false;
            button.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var picture = Rect("Avatar", button.transform); Stretch(picture);
            avatarPortrait = picture.gameObject.AddComponent<RawImage>(); avatarPortrait.raycastTarget = false;
            return button.GetComponent<RectTransform>();
        }

        private void RefreshAvatarButton()
        {
            if (avatarPortrait == null || profile == null) return;
            int index = (int)profile.Avatar;
            avatarPortrait.texture = index >= 0 && index < previews.Length ? previews[index] : null;
            avatarButton.GetComponent<Image>().color = PockleTheme.FieldFor(profile.AvatarId);
        }

        private void ToggleFavorite()
        {
            if (profile == null) return;
            profile.SetFavorite(variant);
            RefreshFavoriteToggle(); RefreshShelf(); RefreshProfile();
        }

        private void RefreshFavoriteToggle()
        {
            if (favoriteGlyph == null || profile == null) return;
            bool favorite = profile.FavoriteId == PipVariants.CollectibleId(variant);
            favoriteGlyph.color = favorite ? PockleTheme.Heart : MutedInk;
        }

        private static string Greeting()
        {
            int hour = DateTime.Now.Hour;
            return hour < 12 ? "Morning" : hour < 18 ? "Afternoon" : "Evening";
        }

        private Sprite CreateRoundedSprite()
        {
            const int size = 128;
            const float radius = 63.5f;
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
                100f, 0, SpriteMeshType.FullRect, new Vector4(64f, 64f, 64f, 64f));
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
