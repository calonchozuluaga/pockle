using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    public sealed partial class PrototypeHud
    {
        private RectTransform walkCard;
        private Text walkProgress, walkStatus;
        private Image walkFill;
        private Button openDaily;
        private readonly RectTransform[] storeCards = new RectTransform[3];
        private readonly RectTransform[] storePictures = new RectTransform[3];
        private readonly RectTransform[] storeNames = new RectTransform[3];
        private readonly RectTransform[] storePrices = new RectTransform[3];
        private readonly RectTransform[] storeBuy = new RectTransform[3];
        private readonly RectTransform[] storeOdds = new RectTransform[3];
        private RectTransform shopHeading;
        private RectTransform catalogNote;
        private RectTransform walkButtons;
        private readonly IBoxCheckout checkout = new UnconfiguredBoxCheckout();

        private void BuildStore()
        {
            walkCard = Rect("Daily walking box", boxesRoot); Surface(walkCard, Paper);
            Label("YOUR DAILY BOX", walkCard, 10, MutedInk, FontStyle.Bold, TextAnchor.MiddleCenter,
                new Vector2(0, -16), new Vector2(250, 20));
            Label("A little walk. A little wonder.", walkCard, 18, Ink, FontStyle.Bold, TextAnchor.MiddleCenter,
                new Vector2(0, -40), new Vector2(290, 28));
            walkProgress = Label("0 / 1,000 steps", walkCard, 24, Ink, FontStyle.Bold, TextAnchor.MiddleCenter,
                new Vector2(0, -76), new Vector2(250, 32));
            var track = Rect("Step progress", walkCard); TopCentered(track, new Vector2(0, -119), new Vector2(250, 8)); Surface(track, Quiet);
            var fill = Rect("Earned steps", track); Stretch(fill); walkFill = fill.gameObject.AddComponent<Image>();
            walkFill.sprite = roundedSprite; walkFill.color = Peach; walkFill.type = Image.Type.Filled; walkFill.fillMethod = Image.FillMethod.Horizontal;
            walkFill.fillOrigin = 0; walkFill.raycastTarget = false; walkFill.fillAmount = 0;
            walkStatus = Label("Enable walking to get started.", walkCard, 11, MutedInk, FontStyle.Normal,
                TextAnchor.MiddleCenter, new Vector2(0, -140), new Vector2(280, 42));
            walkButtons = Rect("Walking actions", walkCard); TopCentered(walkButtons, new Vector2(0, -194), new Vector2(290, 48));
            CreateButton("Enable walking", walkButtons, Vector2.zero, new Vector2(140, 48), Quiet, () => session?.EnableWalking(), out _, 12);
            openDaily = CreateButton("Walk to unlock", walkButtons, new Vector2(150, 0), new Vector2(140, 48), Peach, () => DailyBoxRequested?.Invoke(), out _, 12);
            Label("Jelly Garden: Peach or Mint, equal chance.", walkCard, 10, MutedInk, FontStyle.Normal,
                TextAnchor.MiddleCenter, new Vector2(0, -250), new Vector2(290, 22));
            shopHeading = Label("Or pick a surprise box", boxesRoot, 20, Ink, FontStyle.Bold, TextAnchor.MiddleCenter,
                Vector2.zero, new Vector2(300, 30)).rectTransform;
            var artwork = Resources.Load<Texture2D>("Store/CollectionBoxes");
            Rect[] crops = { new Rect(.04f, .09f, .30f, .74f), new Rect(.35f, .06f, .29f, .73f), new Rect(.655f, .09f, .325f, .74f) };
            for (int i = 0; i < 3; i++)
            {
                int index = i;
                BoxOffer offer = BoxCatalog.Offers[i];
                storeCards[i] = Rect(offer.Name + " · shop", boxesRoot); Surface(storeCards[i], Paper);
                storePictures[i] = Rect("Closed collection box", storeCards[i]);
                if (artwork != null)
                {
                    var image = storePictures[i].gameObject.AddComponent<RawImage>(); image.texture = artwork;
                    image.uvRect = crops[i]; image.raycastTarget = false;
                }
                else Surface(storePictures[i], Quiet);
                storeNames[i] = Label(offer.Name, storeCards[i], 14, Ink, FontStyle.Bold, TextAnchor.MiddleCenter,
                    Vector2.zero, new Vector2(160, 26)).rectTransform;
                storePrices[i] = Label(offer.ProposedPrice, storeCards[i], 21, Ink, FontStyle.Bold, TextAnchor.MiddleCenter,
                    Vector2.zero, new Vector2(160, 30)).rectTransform;
                storeOdds[i] = Label(offer.Contents, storeCards[i], 10, MutedInk, FontStyle.Normal, TextAnchor.MiddleCenter,
                    Vector2.zero, new Vector2(160, 32)).rectTransform;
                var buy = CreateButton("Buy box", storeCards[i], Vector2.zero, new Vector2(150, 48), Peach,
                    () => BuyBox(BoxCatalog.Offers[index]), out _, 12);
                storeBuy[i] = buy.GetComponent<RectTransform>();
            }
            catalogNote = Label("Test catalog · proposed USD prices. Checkout coming next.", boxesRoot, 10, MutedInk,
                FontStyle.Normal, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(330, 30)).rectTransform;
        }

        private void BuyBox(BoxOffer offer)
        {
            if (!checkout.Available) { ShowDialog(offer.Name, checkout.UnavailableReason); return; }
            checkout.Begin(offer);
        }

        private void LayoutWalkingCard(float width)
        {
            TopCentered(walkCard, Vector2.zero, new Vector2(Mathf.Min(width, 480), 284));
            float buttonWidth = Mathf.Min(width - 24, 290);
            walkButtons.sizeDelta = new Vector2(buttonWidth, 48);
            for (int i = 0; i < 2; i++) LeftLabel((RectTransform)walkButtons.GetChild(i), new Vector2(i * (buttonWidth + 10) / 2, 0), new Vector2((buttonWidth - 10) / 2, 48));
        }

        private float LayoutStore(float width)
        {
            LayoutWalkingCard(width);
            TopCentered(shopHeading, new Vector2(0, -308), new Vector2(width, 30));
            const float gap = 14;
            int columns = width >= 620 ? 3 : width >= 280 ? 2 : 1;
            float cellWidth = (width - gap * (columns - 1)) / columns;
            float imageHeight = Mathf.Min(170, cellWidth * .84f);
            float cellHeight = imageHeight + 166;
            int rows = (3 + columns - 1) / columns;
            float height = 358 + rows * (cellHeight + gap);
            TopCentered(boxesRoot, Vector2.zero, new Vector2(width, height + 42));
            for (int i = 0; i < 3; i++)
            {
                LeftLabel(storeCards[i], new Vector2((i % columns) * (cellWidth + gap), -358 - (i / columns) * (cellHeight + gap)), new Vector2(cellWidth, cellHeight));
                TopCentered(storePictures[i], new Vector2(0, -4), new Vector2(cellWidth - 8, imageHeight));
                TopCentered(storeNames[i], new Vector2(0, -imageHeight - 10), new Vector2(cellWidth - 10, 26));
                TopCentered(storePrices[i], new Vector2(0, -imageHeight - 38), new Vector2(cellWidth - 10, 30));
                TopCentered(storeOdds[i], new Vector2(0, -imageHeight - 72), new Vector2(cellWidth - 10, 32));
                TopCentered(storeBuy[i], new Vector2(0, -imageHeight - 111), new Vector2(cellWidth - 20, 48));
            }
            TopCentered(catalogNote, new Vector2(0, -height), new Vector2(width, 32));
            return height + 42;
        }
    }
}
