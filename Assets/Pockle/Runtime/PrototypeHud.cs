using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>Safe-area UI, generated locally without a scene or imported assets.</summary>
    public sealed partial class PrototypeHud : MonoBehaviour
    {
        private static readonly Color Ink = new Color(0.29f, 0.21f, 0.29f);
        private static readonly Color MutedInk = new Color(0.48f, 0.41f, 0.43f);
        private static readonly Color Peach = new Color(0.965f, 0.72f, 0.61f);
        private static readonly Color Quiet = new Color(0.947f, 0.922f, 0.89f);
        private static readonly Color Paper = new Color(1f, 0.99f, 0.971f);

        public event Action<bool> SoundChanged;
        public event Action<bool> HapticsChanged;
        public event Action<bool> ReducedMotionChanged;
        public event Action<PipVariant> VariantChanged;

        private RectTransform safeRoot;
        private RectTransform header;
        private RectTransform companion;
        private RectTransform card;
        private RectTransform variantRow;
        private RectTransform actionRow;
        private RectTransform preferenceRow;
        private Text status;
        private Text guide;
        private Text soundLabel;
        private Text hapticsLabel;
        private Text motionLabel;
        private Text brandLabel;
        private Text taglineLabel;
        private Text companionKind;
        private Text companionName;
        private Text companionCaption;
        private Text footer;
        private Image peachBackground;
        private Image moonBackground;
        private Image soundBackground;
        private Image hapticsBackground;
        private Image motionBackground;
        private Button revealButton;
        private Font font;
        private Sprite roundedSprite;
        private Texture2D roundedTexture;
        private Canvas canvas;
        private CanvasScaler scaler;
        private GameObject ownedEventSystem;
        private Action<bool> soundCallback;
        private Action<bool> hapticsCallback;
        private Action<bool> motionCallback;
        private bool soundEnabled = true;
        private bool hapticsEnabled = true;
        private bool reducedMotionEnabled;
        private bool initialized;
        private PipVariant variant;
        private Rect lastSafeArea;
        private Vector2 lastScreenSize;
        private Vector2 lastLayoutSize;

        public void Initialize(Action reveal, Action reset, Action<bool> sound,
            Action<bool> haptics, Action<bool> reducedMotion)
        {
            if (initialized) return;
            initialized = true;
            soundCallback = sound;
            hapticsCallback = haptics;
            motionCallback = reducedMotion;
            font = LoadFont();
            roundedSprite = CreateRoundedSprite();

            var canvasObject = new GameObject("Pockle · interface", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(390f, 844f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 1f;
            safeRoot = Rect("Safe area", canvasObject.transform);
            Stretch(safeRoot);

            header = Rect("Brand", safeRoot);
            brandLabel = Label("Pockle", header, 34, Ink, FontStyle.Bold, TextAnchor.MiddleLeft,
                new Vector2(0f, 0f), new Vector2(360f, 44f));
            taglineLabel = Label("A little wonder, in your hands.", header, 13, MutedInk, FontStyle.Normal,
                TextAnchor.UpperLeft, new Vector2(0f, -45f), new Vector2(360f, 34f));

            companion = Rect("Companion identity", safeRoot);
            companionKind = Label("PEACH JELLY", companion, 10, MutedInk, FontStyle.Bold,
                TextAnchor.MiddleRight, new Vector2(0f, -35f), new Vector2(120f, 18f));
            companionName = Label("Pip", companion, 27, Ink, FontStyle.Bold, TextAnchor.MiddleRight,
                Vector2.zero, new Vector2(120f, 34f));
            companionCaption = Label("01 / COMPANION", companion, 9, MutedInk, FontStyle.Normal,
                TextAnchor.MiddleRight, new Vector2(0f, -53f), new Vector2(120f, 16f));

            card = Rect("Interaction card", safeRoot);
            Surface(card, Paper);
            status = Label("Make yourself at home.", card, 15, Ink, FontStyle.Bold,
                TextAnchor.MiddleCenter, new Vector2(0f, -12f), new Vector2(328f, 25f));
            guide = Label("Drag Pip to lift. Two fingers to squish/stretch.", card, 11, MutedInk,
                FontStyle.Normal, TextAnchor.MiddleCenter, new Vector2(0f, -37f), new Vector2(328f, 18f));
            guide.resizeTextForBestFit = true;
            guide.resizeTextMinSize = 9;
            guide.resizeTextMaxSize = 11;
            status.resizeTextForBestFit = true;
            status.resizeTextMinSize = 12;
            status.resizeTextMaxSize = 15;

            variantRow = Rect("Pip variants", card);
            TopCentered(variantRow, new Vector2(0f, -58f), new Vector2(328f, 44f));
            CreateButton("Peach", variantRow, Vector2.zero, new Vector2(158f, 44f), Peach,
                () => ChangeVariant(PipVariant.PeachJelly), out peachBackground);
            CreateButton("Moon", variantRow, new Vector2(170f, 0f), new Vector2(158f, 44f), Quiet,
                () => ChangeVariant(PipVariant.MoonJelly), out moonBackground);

            actionRow = Rect("Toy actions", card);
            TopCentered(actionRow, new Vector2(0f, -112f), new Vector2(328f, 44f));
            Image revealBackground;
            revealButton = CreateButton("Reveal again", actionRow, Vector2.zero, new Vector2(101f, 44f),
                Peach, () => reveal?.Invoke(), out revealBackground, 11);
            Image resetBackground;
            CreateButton("Reset", actionRow, new Vector2(110f, 0f), new Vector2(101f, 44f),
                Quiet, () => reset?.Invoke(), out resetBackground);
            Image storeBackground;
            CreateButton("Store", actionRow, new Vector2(220f, 0f), new Vector2(108f, 44f),
                Quiet, () => SetStoreVisible(true), out storeBackground);

            preferenceRow = Rect("Comfort settings", card);
            TopCentered(preferenceRow, new Vector2(0f, -168f), new Vector2(328f, 44f));
            var soundButton = CreateButton("Sound · on", preferenceRow, Vector2.zero, new Vector2(101f, 44f),
                Quiet, ChangeSound, out soundBackground, 11);
            soundLabel = soundButton.GetComponentInChildren<Text>();
            var hapticsButton = CreateButton("Haptics · on", preferenceRow, new Vector2(110f, 0f),
                new Vector2(108f, 44f), Quiet, ChangeHaptics, out hapticsBackground, 11);
            hapticsLabel = hapticsButton.GetComponentInChildren<Text>();
            var motionButton = CreateButton("Motion · full", preferenceRow, new Vector2(227f, 0f),
                new Vector2(101f, 44f), Quiet, ChangeMotion, out motionBackground, 11);
            motionLabel = motionButton.GetComponentInChildren<Text>();
            footer = Label("PEACH JELLY · TACTILE PROTOTYPE", card, 8, MutedInk, FontStyle.Normal,
                TextAnchor.MiddleCenter, new Vector2(0f, -216f), new Vector2(328f, 15f));

            if (EventSystem.current == null)
            {
                ownedEventSystem = new GameObject("Pockle event system", typeof(EventSystem), typeof(StandaloneInputModule));
                ownedEventSystem.transform.SetParent(transform, false);
            }
            SetSettings(true, true, false);
            SetVariant(PipVariant.PeachJelly);
            BuildStore(canvasObject.transform);
            ApplySafeArea();
            Canvas.ForceUpdateCanvases();
            AdaptLayout();
        }

        public void SetStatus(string message)
        {
            if (status != null) status.text = string.IsNullOrEmpty(message) ? "Make yourself at home." : message;
        }

        public void SetVariant(PipVariant choice)
        {
            variant = PipVariants.FromSaved((int)choice);
            bool moon = variant == PipVariant.MoonJelly;
            companionKind.text = PipVariants.Label(variant);
            companionCaption.text = moon ? "02 / VARIANT" : "01 / VARIANT";
            footer.text = PipVariants.Label(variant) + " · TACTILE PROTOTYPE";
            peachBackground.color = moon ? Quiet : Peach;
            moonBackground.color = moon ? new Color(.69f, .82f, .94f) : Quiet;
        }

        private void ChangeVariant(PipVariant choice)
        {
            if (variant == choice) return;
            SetVariant(choice);
            VariantChanged?.Invoke(choice);
        }

        public void SetRevealAvailable(bool available)
        {
            if (revealButton != null) revealButton.interactable = available;
        }

        /// <summary>Synchronize persisted preferences without invoking user callbacks.</summary>
        public void SetSettings(bool sound, bool haptics, bool reducedMotion)
        {
            soundEnabled = sound;
            hapticsEnabled = haptics;
            reducedMotionEnabled = reducedMotion;
            if (soundLabel != null) soundLabel.text = sound ? "Sound · on" : "Sound · off";
            if (hapticsLabel != null) hapticsLabel.text = haptics ? "Haptics · on" : "Haptics · off";
            if (motionLabel != null) motionLabel.text = reducedMotion ? "Motion · calm" : "Motion · full";
            if (soundBackground != null) soundBackground.color = sound ? new Color(0.97f, 0.87f, 0.8f) : Quiet;
            if (hapticsBackground != null) hapticsBackground.color = haptics ? new Color(0.97f, 0.87f, 0.8f) : Quiet;
            if (motionBackground != null) motionBackground.color = reducedMotion ? new Color(0.97f, 0.87f, 0.8f) : Quiet;
        }

        private void ChangeSound()
        {
            SetSettings(!soundEnabled, hapticsEnabled, reducedMotionEnabled);
            soundCallback?.Invoke(soundEnabled);
            SoundChanged?.Invoke(soundEnabled);
        }

        private void ChangeHaptics()
        {
            SetSettings(soundEnabled, !hapticsEnabled, reducedMotionEnabled);
            hapticsCallback?.Invoke(hapticsEnabled);
            HapticsChanged?.Invoke(hapticsEnabled);
        }

        private void ChangeMotion()
        {
            SetSettings(soundEnabled, hapticsEnabled, !reducedMotionEnabled);
            motionCallback?.Invoke(reducedMotionEnabled);
            ReducedMotionChanged?.Invoke(reducedMotionEnabled);
        }

        private void Update()
        {
            if (!initialized) return;
            if (lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height || lastSafeArea != Screen.safeArea)
                ApplySafeArea();
            if (safeRoot != null && lastLayoutSize != safeRoot.rect.size) AdaptLayout();
        }

        private void ApplySafeArea()
        {
            if (safeRoot == null || Screen.width < 1 || Screen.height < 1) return;
            lastSafeArea = Screen.safeArea;
            lastScreenSize = new Vector2(Screen.width, Screen.height);
            scaler.referenceResolution = (float)Screen.width / Screen.height > 1.15f
                ? new Vector2(844f, 390f) : new Vector2(390f, 844f);
            safeRoot.anchorMin = new Vector2(lastSafeArea.xMin / Screen.width, lastSafeArea.yMin / Screen.height);
            safeRoot.anchorMax = new Vector2(lastSafeArea.xMax / Screen.width, lastSafeArea.yMax / Screen.height);
            safeRoot.offsetMin = Vector2.zero;
            safeRoot.offsetMax = Vector2.zero;
            if (storeSafeRoot != null)
            {
                storeSafeRoot.anchorMin = safeRoot.anchorMin;
                storeSafeRoot.anchorMax = safeRoot.anchorMax;
                storeSafeRoot.offsetMin = storeSafeRoot.offsetMax = Vector2.zero;
            }
        }

        private void AdaptLayout()
        {
            if (safeRoot == null) return;
            var size = safeRoot.rect.size;
            if (size.x < 1f || size.y < 1f) return;
            lastLayoutSize = size;
            var landscape = (float)Screen.width / Mathf.Max(1, Screen.height) > 1.15f;
            var columnWidth = Mathf.Max(280f, size.x * 0.35f);
            var width = landscape ? Mathf.Min(360f, columnWidth - 20f) : Mathf.Min(360f, size.x - 24f);
            // Very narrow phones keep 44-point buttons by scaling only horizontal dimensions.
            var contentWidth = Mathf.Max(1f, width - 32f);
            ResizeButtons(variantRow, contentWidth, 12f, false);
            ResizeButtons(actionRow, contentWidth, 9f, false);
            ResizeButtons(preferenceRow, contentWidth, 9f, false);
            status.rectTransform.sizeDelta = new Vector2(contentWidth, 25f);
            guide.rectTransform.sizeDelta = new Vector2(contentWidth, 18f);
            if (landscape)
            {
                // Match StageFraming; wide phones use 35%, tablets reserve enough
                // actual width for readable labels and 44-point control targets.
                var viewerWidth = size.x - columnWidth;
                var headerWidth = Mathf.Min(360f, viewerWidth - 28f);
                TopCentered(header, new Vector2(-columnWidth * 0.5f, -14f), new Vector2(headerWidth, 78f));
                LeftLabel(brandLabel.rectTransform, Vector2.zero, new Vector2(headerWidth, 44f));
                LeftLabel(taglineLabel.rectTransform, new Vector2(0f, -45f), new Vector2(headerWidth, 34f));
                TopCentered(companion, new Vector2(viewerWidth * 0.5f, -14f), new Vector2(width, 70f));
                companionName.alignment = companionKind.alignment = companionCaption.alignment = TextAnchor.MiddleCenter;
                card.anchorMin = card.anchorMax = new Vector2(0.5f, 0f);
                card.pivot = new Vector2(0.5f, 0f);
                card.anchoredPosition = new Vector2(viewerWidth * 0.5f, 14f);
                card.sizeDelta = new Vector2(width, 238f);
            }
            else
            {
                TopCentered(header, new Vector2(0f, -24f), new Vector2(width, 78f));
                LeftLabel(brandLabel.rectTransform, Vector2.zero, new Vector2(width * 0.66f, 44f));
                LeftLabel(taglineLabel.rectTransform, new Vector2(0f, -45f), new Vector2(width * 0.66f, 34f));
                TopCentered(companion, new Vector2(width * 0.34f, -24f), new Vector2(width * 0.32f, 70f));
                companionName.alignment = companionKind.alignment = companionCaption.alignment = TextAnchor.MiddleRight;
                card.anchorMin = card.anchorMax = new Vector2(0.5f, 0f);
                card.pivot = new Vector2(0.5f, 0f);
                card.anchoredPosition = new Vector2(0f, 14f);
                card.sizeDelta = new Vector2(width, 238f);
            }
            var companionWidth = companion.sizeDelta.x;
            companionName.rectTransform.sizeDelta = new Vector2(companionWidth, 34f);
            companionKind.rectTransform.sizeDelta = new Vector2(companionWidth, 18f);
            companionCaption.rectTransform.sizeDelta = new Vector2(companionWidth, 16f);
            AdaptStoreLayout(size);
        }

        private static void ResizeButtons(RectTransform row, float width, float gap, bool weighted)
        {
            row.sizeDelta = new Vector2(width, row.sizeDelta.y);
            var count = row.childCount;
            var available = width - gap * (count - 1);
            var x = 0f;
            for (var i = 0; i < count; i++)
            {
                var button = row.GetChild(i) as RectTransform;
                if (button == null) continue;
                var buttonWidth = weighted ? available * (i == 0 ? 0.65f : 0.35f) : available / count;
                button.anchoredPosition = new Vector2(x, 0f);
                button.sizeDelta = new Vector2(buttonWidth, button.sizeDelta.y);
                x += buttonWidth + gap;
            }
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
            if (roundedSprite != null) Destroy(roundedSprite);
            if (roundedTexture != null) Destroy(roundedTexture);
            if (canvas != null) Destroy(canvas.gameObject);
            if (ownedEventSystem != null) Destroy(ownedEventSystem);
        }
    }
}
