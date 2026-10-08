using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>Collection-first UI. Toy manipulation stays on the toy and its plate.</summary>
    public sealed partial class PrototypeHud : MonoBehaviour
    {
        private static readonly Color Ink = new Color(.29f, .21f, .29f);
        private static readonly Color MutedInk = new Color(.48f, .41f, .43f);
        private static readonly Color Peach = new Color(.965f, .72f, .61f);
        private static readonly Color Quiet = new Color(.947f, .922f, .89f);
        private static readonly Color Paper = new Color(1f, .99f, .971f);
        private enum Page { Shelf, Boxes, Play }
        private Page page = Page.Shelf;
        public bool StoreVisible => page != Page.Play || modal != null && modal.gameObject.activeSelf;
        public event Action<PipVariant> VariantChanged;
        public event Action<bool> StoreVisibilityChanged;
        public event Action DailyBoxRequested;
        private RectTransform safeRoot, browsing, content, navigation, modal, modalSafeRoot, modalCard;
        private Text heading, subtitle, playHint, modalTitle, modalText;
        private Button settingsButton, backButton;
        private Image shelfTab, boxesTab;
        private RectTransform shelfRoot, boxesRoot;
        private Font font;
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
        private RectTransform preferenceRow;
        private Text soundLabel, hapticsLabel, motionLabel;
        private bool initialized;

        public void Initialize(Action<bool> sound, Action<bool> haptics, Action<bool> reducedMotion)
        {
            if (initialized) return;
            initialized = true;
            soundCallback = sound; hapticsCallback = haptics; motionCallback = reducedMotion;
            font = LoadFont(); roundedSprite = CreateRoundedSprite();
            var root = new GameObject("Pockle · game UI", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            root.transform.SetParent(transform, false);
            canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 20;
            scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(390, 844); scaler.matchWidthOrHeight = 1;
            safeRoot = Rect("Safe area", root.transform); Stretch(safeRoot);
            var brand = Label("Pockle", safeRoot, 32, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(180, 44));
            LeftLabel(brand.rectTransform, new Vector2(22, -14), new Vector2(180, 44));
            settingsButton = CreateButton("Settings", safeRoot, Vector2.zero, new Vector2(88, 44), Quiet, ShowSettings, out _, 12);
            backButton = CreateButton("Shelf", safeRoot, new Vector2(22, 66), new Vector2(74, 44), Paper, () => SetPage(Page.Shelf), out _, 12);
            heading = Label("Your little collection", safeRoot, 23, Ink, FontStyle.Bold, TextAnchor.MiddleCenter,
                new Vector2(0, -70), new Vector2(340, 34));
            subtitle = Label("Four Pips. Four little personalities.", safeRoot, 12, MutedInk, FontStyle.Normal,
                TextAnchor.MiddleCenter, new Vector2(0, -108), new Vector2(350, 26));
            browsing = Rect("Collection browsing", safeRoot);
            var scroll = browsing.gameObject.AddComponent<ScrollRect>(); scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped;
            var viewport = Rect("Viewport", browsing); Stretch(viewport); viewport.gameObject.AddComponent<RectMask2D>();
            var dragTarget = viewport.gameObject.AddComponent<Image>(); dragTarget.color = Color.clear;
            content = Rect("Collection content", viewport); content.anchorMin = new Vector2(0, 1); content.anchorMax = new Vector2(1, 1); content.pivot = new Vector2(.5f, 1);
            scroll.content = content; scroll.viewport = viewport;
            shelfRoot = Rect("Your shelf", content); boxesRoot = Rect("Walking and shop", content);
            BuildShelf(); BuildStore();
            navigation = Rect("Navigation", safeRoot); Surface(navigation, Paper);
            CreateButton("Shelf", navigation, Vector2.zero, new Vector2(150, 48), Peach, () => SetPage(Page.Shelf), out shelfTab);
            CreateButton("Boxes", navigation, new Vector2(158, 0), new Vector2(150, 48), Quiet, () => SetPage(Page.Boxes), out boxesTab);
            playHint = Label("Make yourself at home.", safeRoot, 12, MutedInk, FontStyle.Normal, TextAnchor.MiddleCenter,
                Vector2.zero, new Vector2(350, 40));
            playHint.rectTransform.anchorMin = playHint.rectTransform.anchorMax = new Vector2(.5f, 0);
            playHint.rectTransform.pivot = new Vector2(.5f, 0); playHint.rectTransform.anchoredPosition = new Vector2(0, 20);
            BuildModal();
            if (EventSystem.current == null)
            {
                ownedEventSystem = new GameObject("Pockle event system", typeof(EventSystem), typeof(StandaloneInputModule));
                ownedEventSystem.transform.SetParent(transform, false);
            }
            ApplySafeArea(); Canvas.ForceUpdateCanvases(); AdaptLayout(); SetPage(Page.Shelf);
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
            if (page == Page.Play) { heading.text = "Pip"; subtitle.text = PipVariants.Label(variant); }
        }
        public void ShowToy(PipVariant choice)
        {
            choice = PipVariants.FromSaved((int)choice);
            if (session != null && session.Progress.Save.Counts[(int)choice] <= 0) return;
            SetVariant(choice); SetPage(Page.Play); VariantChanged?.Invoke(choice);
        }
        public void GoBack()
        {
            if (modal.gameObject.activeSelf) CloseModal();
            else if (backButton.interactable) SetPage(Page.Shelf);
        }
        private void SetPage(Page next)
        {
            bool blocked = StoreVisible;
            page = next;
            browsing.gameObject.SetActive(page != Page.Play);
            navigation.gameObject.SetActive(page != Page.Play);
            playHint.gameObject.SetActive(page == Page.Play);
            backButton.gameObject.SetActive(page == Page.Play);
            shelfRoot.gameObject.SetActive(page == Page.Shelf); boxesRoot.gameObject.SetActive(page == Page.Boxes);
            heading.text = page == Page.Shelf ? "Your little collection" : page == Page.Boxes ? "A little surprise awaits" : "Pip";
            subtitle.text = page == Page.Shelf ? "Tap a toy. Make yourself at home." : page == Page.Boxes ? "Walk for a box, or pick one to buy." : PipVariants.Label(variant);
            shelfTab.color = page == Page.Shelf ? Peach : Quiet; boxesTab.color = page == Page.Boxes ? Peach : Quiet;
            content.anchoredPosition = Vector2.zero;
            if (lastLayoutSize.x > 0) AdaptLayout();
            if (blocked != StoreVisible) StoreVisibilityChanged?.Invoke(StoreVisible);
        }
        public void SetSettings(bool sound, bool haptics, bool reducedMotion)
        {
            soundEnabled = sound; hapticsEnabled = haptics; reducedMotionEnabled = reducedMotion;
            if (soundLabel != null) soundLabel.text = sound ? "Sound on" : "Sound off";
            if (hapticsLabel != null) hapticsLabel.text = haptics ? "Haptics on" : "Haptics off";
            if (motionLabel != null) motionLabel.text = reducedMotion ? "Motion calm" : "Motion full";
        }
        private void BuildModal()
        {
            modal = Rect("Dialog backdrop", canvas.transform); Stretch(modal);
            var backdrop = modal.gameObject.AddComponent<Image>(); backdrop.color = new Color(.23f, .17f, .22f, .35f);
            modalSafeRoot = Rect("Dialog safe area", modal); Stretch(modalSafeRoot);
            modalCard = Rect("Dialog", modalSafeRoot); modalCard.anchorMin = modalCard.anchorMax = new Vector2(.5f, .5f);
            modalCard.pivot = new Vector2(.5f, .5f); Surface(modalCard, Paper);
            modalTitle = Label("", modalCard, 22, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, new Vector2(0, -24), new Vector2(290, 36));
            modalText = Label("", modalCard, 14, MutedInk, FontStyle.Normal, TextAnchor.UpperCenter, new Vector2(0, -76), new Vector2(290, 132));
            preferenceRow = Rect("Comfort preferences", modalCard); TopCentered(preferenceRow, new Vector2(0, -204), new Vector2(284, 50));
            var b = CreateButton("", preferenceRow, Vector2.zero, new Vector2(88, 48), Quiet,
                () => { SetSettings(!soundEnabled, hapticsEnabled, reducedMotionEnabled); soundCallback?.Invoke(soundEnabled); }, out _, 11);
            soundLabel = b.GetComponentInChildren<Text>();
            b = CreateButton("", preferenceRow, new Vector2(98, 0), new Vector2(88, 48), Quiet,
                () => { SetSettings(soundEnabled, !hapticsEnabled, reducedMotionEnabled); hapticsCallback?.Invoke(hapticsEnabled); }, out _, 11);
            hapticsLabel = b.GetComponentInChildren<Text>();
            b = CreateButton("", preferenceRow, new Vector2(196, 0), new Vector2(88, 48), Quiet,
                () => { SetSettings(soundEnabled, hapticsEnabled, !reducedMotionEnabled); motionCallback?.Invoke(reducedMotionEnabled); }, out _, 11);
            motionLabel = b.GetComponentInChildren<Text>();
            b = CreateButton("Done", modalCard, Vector2.zero, new Vector2(140, 48), Peach, CloseModal, out _);
            var close = b.GetComponent<RectTransform>(); close.anchorMin = close.anchorMax = new Vector2(.5f, 0);
            close.pivot = new Vector2(.5f, 0); close.anchoredPosition = new Vector2(0, 22);
            modal.gameObject.SetActive(false);
        }
        private void ShowSettings() { ShowDialog("Make it yours", "Set the sound, haptics, and motion to whatever feels comfortable.", true); }
        private void ShowDialog(string title, string message, bool settings = false)
        {
            bool wasBlocked = StoreVisible;
            modalTitle.text = title; modalText.text = message; preferenceRow.gameObject.SetActive(settings);
            LayoutModal(settings);
            modal.gameObject.SetActive(true); modal.SetAsLastSibling();
            if (!wasBlocked) StoreVisibilityChanged?.Invoke(true);
        }
        private void CloseModal()
        {
            modal.gameObject.SetActive(false);
            if (page == Page.Play) StoreVisibilityChanged?.Invoke(false);
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
            settings.pivot = new Vector2(1, 1); settings.anchoredPosition = new Vector2(-22, -14);
            float width = Mathf.Max(1, Mathf.Min(820, size.x - 32));
            heading.rectTransform.sizeDelta = new Vector2(Mathf.Max(1, width - (page == Page.Play ? 152 : 0)), 34);
            subtitle.rectTransform.sizeDelta = new Vector2(width, 26);
            float viewportHeight = Mathf.Max(30, size.y - 230);
            TopCentered(browsing, new Vector2(0, -148), new Vector2(width, viewportHeight));
            navigation.anchorMin = navigation.anchorMax = new Vector2(.5f, 0); navigation.pivot = new Vector2(.5f, 0);
            navigation.anchoredPosition = new Vector2(0, 20); navigation.sizeDelta = new Vector2(Mathf.Min(340, width), 52);
            float navWidth = navigation.sizeDelta.x;
            LeftLabel(shelfTab.rectTransform, Vector2.zero, new Vector2((navWidth - 8) / 2, 52));
            LeftLabel(boxesTab.rectTransform, new Vector2((navWidth + 8) / 2, 0), new Vector2((navWidth - 8) / 2, 52));
            playHint.rectTransform.sizeDelta = new Vector2(width, 40);
            float height = page == Page.Boxes ? LayoutStore(width) : LayoutShelf(width);
            content.sizeDelta = new Vector2(0, Mathf.Max(viewportHeight, height));
            content.anchoredPosition = new Vector2(0, Mathf.Clamp(content.anchoredPosition.y, 0, Mathf.Max(0, height - viewportHeight)));
            if (modal.gameObject.activeSelf) LayoutModal(preferenceRow.gameObject.activeSelf);
        }

        private void LayoutModal(bool settings)
        {
            float width = Mathf.Max(1, Mathf.Min(340, lastLayoutSize.x - 24));
            float height = Mathf.Min(settings ? 340 : 280, lastLayoutSize.y - 16);
            modalCard.sizeDelta = new Vector2(width, height);
            modalTitle.rectTransform.sizeDelta = new Vector2(width - 24, 36);
            modalText.rectTransform.sizeDelta = new Vector2(width - 32, settings ? Mathf.Max(44, height - 240) : Mathf.Max(44, height - 150));
            preferenceRow.anchoredPosition = new Vector2(0, -height + 136);
            float rowWidth = Mathf.Min(284, width - 24);
            preferenceRow.sizeDelta = new Vector2(rowWidth, 48);
            for (int i = 0; i < 3; i++)
                LeftLabel((RectTransform)preferenceRow.GetChild(i), new Vector2(i * (rowWidth + 10) / 3, 0), new Vector2((rowWidth - 20) / 3, 48));
        }
        private static void LeftLabel(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private Button CreateButton(string title, Transform parent, Vector2 position, Vector2 size,
            Color colour, Action click, out Image background, int fontSize = 13)
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
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = colour;
            text.alignment = alignment;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.raycastTarget = false;
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

        private static Font LoadFont()
        {
#if UNITY_2022_2_OR_NEWER
            return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
            return Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
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

        private void OnDestroy()
        {
            if (session != null) session.Changed -= RefreshCollection;
            foreach (var preview in previews) if (preview != null) { preview.Release(); Destroy(preview); }
            if (roundedSprite != null) Destroy(roundedSprite);
            if (roundedTexture != null) Destroy(roundedTexture);
            if (canvas != null) Destroy(canvas.gameObject);
            if (ownedEventSystem != null) Destroy(ownedEventSystem);
        }
    }
}
