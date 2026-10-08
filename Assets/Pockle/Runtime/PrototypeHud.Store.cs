using System;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    public sealed partial class PrototypeHud
    {
        public event Action<bool> StoreVisibilityChanged;
        public bool StoreVisible { get; private set; }

        private RectTransform storeRoot;
        private RectTransform storeSafeRoot;
        private RectTransform storeBrand;
        private RectTransform storeScrollRoot;
        private RectTransform storeContent;
        private readonly RectTransform[] storeCards = new RectTransform[3];
        private readonly RectTransform[] storePictures = new RectTransform[3];
        private readonly RectTransform[] storeNames = new RectTransform[3];
        private readonly RectTransform[] storePrices = new RectTransform[3];

        public void SetStoreVisible(bool visible)
        {
            if (storeRoot == null || StoreVisible == visible) return;
            StoreVisible = visible;
            storeRoot.gameObject.SetActive(visible);
            // Keep the same viewer behind the store, including its variant and angle.
            if (visible) storeRoot.SetAsLastSibling();
            StoreVisibilityChanged?.Invoke(visible);
        }

        private void BuildStore(Transform parent)
        {
            storeRoot = Rect("Pockle store", parent);
            Stretch(storeRoot);
            var backdrop = storeRoot.gameObject.AddComponent<Image>();
            backdrop.color = new Color(.972f, .956f, .928f);
            backdrop.raycastTarget = true; // Background touches cannot reach the viewer.
            storeSafeRoot = Rect("Store safe area", storeRoot);
            Stretch(storeSafeRoot);

            Image backBackground;
            CreateButton("Back to Pip", storeSafeRoot, new Vector2(18f, 18f),
                new Vector2(110f, 44f), Paper, () => SetStoreVisible(false), out backBackground, 12);
            storeBrand = Label("Pockle", storeSafeRoot, 30, Ink, FontStyle.Bold,
                TextAnchor.MiddleRight, Vector2.zero, new Vector2(180f, 44f)).rectTransform;
            Label("Surprise boxes", storeSafeRoot, 24, Ink, FontStyle.Bold,
                TextAnchor.MiddleCenter, new Vector2(0f, -80f), new Vector2(260f, 32f));
            Label("Little boxes. Big surprises.", storeSafeRoot, 12, MutedInk, FontStyle.Normal,
                TextAnchor.MiddleCenter, new Vector2(0f, -116f), new Vector2(260f, 22f));

            storeScrollRoot = Rect("Box browsing", storeSafeRoot);
            var scroll = storeScrollRoot.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.inertia = true;
            var viewport = Rect("Box viewport", storeScrollRoot);
            Stretch(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();
            // A transparent graphic provides a reliable drag target over gaps.
            var scrollTarget = viewport.gameObject.AddComponent<Image>();
            scrollTarget.color = Color.clear;
            scrollTarget.raycastTarget = true;
            storeContent = Rect("Closed collection boxes", viewport);
            storeContent.anchorMin = new Vector2(0f, 1f);
            storeContent.anchorMax = new Vector2(1f, 1f);
            storeContent.pivot = new Vector2(.5f, 1f);
            scroll.viewport = viewport;
            scroll.content = storeContent;

            // Sample the existing folded concept artwork as a shared UI atlas.
            // UV rectangles omit the presentation's heading and captions.
            Texture2D artwork = Resources.Load<Texture2D>("Store/CollectionBoxes");
            if (artwork == null) Debug.LogWarning("Store box artwork is missing; showing mystery silhouettes.", this);
            string[] names = { "Jelly Garden", "Midnight Glow", "Gold Confetti" };
            string[] prices = { "$0.99", "$2.99", "$2.99" };
            Rect[] crops = {
                new Rect(.04f, .09f, .30f, .74f),
                new Rect(.35f, .06f, .29f, .73f),
                new Rect(.655f, .09f, .325f, .74f) };
            for (int i = 0; i < storeCards.Length; i++)
            {
                storeCards[i] = Rect(names[i] + " · closed box", storeContent);
                storePictures[i] = Rect("Closed mystery box", storeCards[i]);
                if (artwork != null)
                {
                    var picture = storePictures[i].gameObject.AddComponent<RawImage>();
                    picture.texture = artwork;
                    picture.uvRect = crops[i];
                    picture.raycastTarget = false;
                }
                else
                {
                    Surface(storePictures[i], Quiet);
                    var mystery = Label("?", storePictures[i], 50, Ink, FontStyle.Bold,
                        TextAnchor.MiddleCenter, Vector2.zero, Vector2.one * 120f);
                    Stretch(mystery.rectTransform);
                }
                storeNames[i] = Label(names[i], storeCards[i], 13, Ink, FontStyle.Bold,
                    TextAnchor.MiddleCenter, Vector2.zero, new Vector2(160f, 24f)).rectTransform;
                storePrices[i] = Label(prices[i], storeCards[i], 20, Ink, FontStyle.Bold,
                    TextAnchor.MiddleCenter, Vector2.zero, new Vector2(160f, 30f)).rectTransform;
            }

            var note = Label("STORE PREVIEW · USD", storeSafeRoot, 9, MutedInk, FontStyle.Normal,
                TextAnchor.MiddleCenter, Vector2.zero, new Vector2(260f, 22f)).rectTransform;
            note.anchorMin = note.anchorMax = new Vector2(.5f, 0f);
            note.pivot = new Vector2(.5f, 0f);
            note.anchoredPosition = new Vector2(0f, 8f);
            storeRoot.gameObject.SetActive(false);
        }

        private void AdaptStoreLayout(Vector2 safeSize)
        {
            if (storeRoot == null) return;
            storeBrand.anchorMin = storeBrand.anchorMax = new Vector2(1f, 1f);
            storeBrand.pivot = new Vector2(1f, 1f);
            storeBrand.anchoredPosition = new Vector2(-18f, -18f);
            storeBrand.sizeDelta = new Vector2(Mathf.Max(1f, Mathf.Min(260f, safeSize.x - 164f)), 44f);

            float width = Mathf.Max(1f, Mathf.Min(900f, safeSize.x - 36f));
            float viewportHeight = Mathf.Max(40f, safeSize.y - 194f);
            TopCentered(storeScrollRoot, new Vector2(0f, -154f), new Vector2(width, viewportHeight));
            int columns = width >= 620f ? 3 : width >= 280f ? 2 : 1;
            const float gap = 14f;
            float cellWidth = (width - gap * (columns - 1)) / columns;
            float imageHeight = Mathf.Min(170f, cellWidth * .84f);
            float cellHeight = imageHeight + 82f;
            int rows = (storeCards.Length + columns - 1) / columns;
            float height = rows * cellHeight + (rows - 1) * gap;
            storeContent.sizeDelta = new Vector2(0f, Mathf.Max(viewportHeight, height));
            storeContent.anchoredPosition = new Vector2(0f,
                Mathf.Clamp(storeContent.anchoredPosition.y, 0f, Mathf.Max(0f, height - viewportHeight)));
            for (int i = 0; i < storeCards.Length; i++)
            {
                LeftLabel(storeCards[i], new Vector2((i % columns) * (cellWidth + gap),
                    -(i / columns) * (cellHeight + gap)), new Vector2(cellWidth, cellHeight));
                TopCentered(storePictures[i], new Vector2(0f, -4f), new Vector2(cellWidth, imageHeight));
                TopCentered(storeNames[i], new Vector2(0f, -imageHeight - 12f), new Vector2(cellWidth, 26f));
                TopCentered(storePrices[i], new Vector2(0f, -imageHeight - 42f), new Vector2(cellWidth, 30f));
            }
        }
    }
}
