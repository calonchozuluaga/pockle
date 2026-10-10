using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// v2 Boxes (UX-032): today's free walking box as a jelly-pink card, then one colour-field card per box with
    /// its contents and a price button. Checkout stays unavailable; buying never grants a toy.
    /// </summary>
    public sealed partial class PrototypeHud
    {
        private RectTransform walkCard, walkArt, walkTrack;
        private Text walkTitle, walkProgress, walkStatus;
        private Image walkFill;
        private Button openDaily, enableWalking;
        private RectTransform shopHeading, catalogNote;
        private readonly RectTransform[] storeCards = new RectTransform[3];
        private readonly RectTransform[] storePictures = new RectTransform[3];
        private readonly Text[] storeNames = new Text[3];
        private readonly Text[] storeOdds = new Text[3];
        private readonly RectTransform[] storeBuy = new RectTransform[3];
        private readonly IBoxCheckout checkout = new UnconfiguredBoxCheckout();

        /// <summary>Transparent cutouts of the three closed boxes, in offer order, and their width/height ratios.</summary>
        private static readonly string[] BoxCutouts = { "UI/Boxes/JellyGarden", "UI/Boxes/MidnightGlow", "UI/Boxes/GoldConfetti" };
        private static readonly float[] BoxAspects = { 380f / 342f, 387f / 343f, 371f / 324f };
        /// <summary>Fallback crops of Resources/Store/CollectionBoxes if a cutout is missing.</summary>
        private static readonly Rect[] BoxCrops = { new Rect(.04f, .09f, .30f, .74f), new Rect(.35f, .06f, .29f, .73f), new Rect(.655f, .09f, .325f, .74f) };

        /// <summary>A closed box picture. Its aspect comes from the source image, not the imported texture, so it never stretches.</summary>
        private RectTransform BoxArt(Transform parent, int index)
        {
            index = Mathf.Clamp(index, 0, BoxCutouts.Length - 1);
            var rect = Rect("Closed box", parent);
            var image = rect.gameObject.AddComponent<RawImage>(); image.raycastTarget = false;
            var cutout = Resources.Load<Texture2D>(BoxCutouts[index]);
            if (cutout != null) image.texture = cutout;
            else
            {
                image.texture = Resources.Load<Texture2D>("Store/CollectionBoxes");
                image.uvRect = BoxCrops[index];
                if (image.texture == null) image.enabled = false;
            }
            return rect;
        }

        private static Vector2 BoxSize(int index, float height)
        {
            return new Vector2(height * BoxAspects[Mathf.Clamp(index, 0, BoxAspects.Length - 1)], height);
        }

        private void BuildStore()
        {
            // Today's free box. Shared with Rewards, so both always show the same claim state.
            walkCard = Rect("Daily walking box", boxesRoot); Surface(walkCard, PockleTheme.Jelly, 24);
            walkArt = BoxArt(walkCard, 0);
            walkTitle = Label("Today's free box", walkCard, PockleTheme.BodySize, Ink, FontStyle.Bold, TextAnchor.MiddleLeft,
                Vector2.zero, new Vector2(200, 24));
            walkProgress = Label("0 of 1,000 steps", walkCard, PockleTheme.CaptionSize, PockleTheme.JellyInk, FontStyle.Normal, TextAnchor.MiddleLeft,
                Vector2.zero, new Vector2(200, 20));
            walkTrack = Rect("Step progress", walkCard); Surface(walkTrack, new Color(1, 1, 1, .6f), 3);
            walkFill = ProgressFill(walkTrack, Ink);
            openDaily = CreateButton("Walk to unlock", walkCard, Vector2.zero, new Vector2(120, 40), PockleTheme.Plum, () => DailyBoxRequested?.Invoke(), out _, 15);
            walkStatus = Label("", walkCard, PockleTheme.SmallSize, PockleTheme.JellyInk, FontStyle.Normal, TextAnchor.UpperLeft,
                Vector2.zero, new Vector2(280, 36));
            enableWalking = CreateButton("Enable walking", walkCard, Vector2.zero, new Vector2(140, 40), new Color(1, 1, 1, .6f), () => session?.EnableWalking(), out _, 15);

            shopHeading = Label("Collections", boxesRoot, PockleTheme.HeadingSize, Ink, FontStyle.Bold, TextAnchor.MiddleLeft,
                Vector2.zero, new Vector2(300, 30)).rectTransform;
            for (int i = 0; i < storeCards.Length; i++)
            {
                int index = i;
                BoxOffer offer = BoxCatalog.Offers[i];
                Color field = PockleTheme.CollectionField(offer.PoolId, out Color ink, out _);
                storeCards[i] = Rect(offer.Name + " · shop", boxesRoot); Card(storeCards[i], field);
                storePictures[i] = BoxArt(storeCards[i], i);
                storePictures[i].localEulerAngles = new Vector3(0, 0, -6);
                storeNames[i] = Label(offer.Name, storeCards[i], 22, ink, FontStyle.Bold, TextAnchor.UpperLeft, Vector2.zero, new Vector2(180, 30));
                storeOdds[i] = Label(offer.Contents, storeCards[i], PockleTheme.CaptionSize, ink, FontStyle.Normal, TextAnchor.UpperLeft,
                    Vector2.zero, new Vector2(180, 40));
                storeOdds[i].color = new Color(ink.r, ink.g, ink.b, .8f);
                // The price is the button. A light button on dark fields, plum on light ones.
                Color button = ink == PockleTheme.OnPlum ? PockleTheme.Jelly : PockleTheme.Plum;
                var buy = CreateButton(offer.ProposedPrice, storeCards[i], Vector2.zero, new Vector2(110, 44), button,
                    () => BuyBox(BoxCatalog.Offers[index]), out _, PockleTheme.BodySize);
                buy.gameObject.name = "Buy · " + offer.Name;
                storeBuy[i] = buy.GetComponent<RectTransform>();
            }
            catalogNote = Label("Test catalog. Prices are proposals, and checkout isn't connected yet.", boxesRoot, PockleTheme.SmallSize, MutedInk,
                FontStyle.Normal, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(330, 36)).rectTransform;
        }

        private void BuyBox(BoxOffer offer)
        {
            if (!checkout.Available) { ShowDialog(offer.Name, checkout.UnavailableReason); return; }
            checkout.Begin(offer);
        }

        /// <summary>Lays out the shared daily card at the top of its page and returns its height.</summary>
        private float LayoutWalkingCard(float width)
        {
            float w = Mathf.Min(width, 480);
            const float height = 150;
            TopCentered(walkCard, Vector2.zero, new Vector2(w, height));
            LeftLabel(walkArt, new Vector2(10, -16), BoxSize(0, 54));
            float textX = 80, textWidth = w - textX - 16;
            LeftLabel(walkTitle.rectTransform, new Vector2(textX, -12), new Vector2(textWidth, 24));
            LeftLabel(walkProgress.rectTransform, new Vector2(textX, -36), new Vector2(textWidth, 20));
            LeftLabel(walkTrack, new Vector2(textX, -60), new Vector2(textWidth, 6));
            LeftLabel(walkStatus.rectTransform, new Vector2(textX, -70), new Vector2(textWidth, 20));
            float buttonWidth = (w - 32 - 10) / 2;
            LeftLabel(enableWalking.GetComponent<RectTransform>(), new Vector2(16, -height + 14 + 40), new Vector2(buttonWidth, 40));
            LeftLabel(openDaily.GetComponent<RectTransform>(), new Vector2(16 + buttonWidth + 10, -height + 14 + 40), new Vector2(buttonWidth, 40));
            return height;
        }

        private float LayoutStore(float width)
        {
            float w = Mathf.Min(width, 480);
            float y = LayoutWalkingCard(width) + 26;
            TopCentered(shopHeading, new Vector2(0, -y), new Vector2(w, 30));
            y += 42;
            const float cardHeight = 150, gap = 12;
            float art = Mathf.Min(130, w * .36f);
            for (int i = 0; i < storeCards.Length; i++)
            {
                TopCentered(storeCards[i], new Vector2(0, -y), new Vector2(w, cardHeight));
                storePictures[i].anchorMin = storePictures[i].anchorMax = storePictures[i].pivot = new Vector2(1, .5f);
                storePictures[i].anchoredPosition = new Vector2(-6, 0); storePictures[i].sizeDelta = BoxSize(i, art * .9f);
                float textWidth = w - 20 - art - 8;
                LeftLabel(storeNames[i].rectTransform, new Vector2(20, -18), new Vector2(textWidth, 30));
                LeftLabel(storeOdds[i].rectTransform, new Vector2(20, -50), new Vector2(textWidth, 40));
                LeftLabel(storeBuy[i], new Vector2(20, -cardHeight + 16 + 44), new Vector2(Mathf.Min(120, textWidth), 44));
                y += cardHeight + gap;
            }
            TopCentered(catalogNote, new Vector2(0, -y), new Vector2(w, 36));
            y += 36;
            TopCentered(boxesRoot, Vector2.zero, new Vector2(width, y));
            return y;
        }

        private void RefreshWalkingCard()
        {
            if (session == null || walkProgress == null) return;
            var saved = session.Progress.Save;
            int target = CollectionProgress.DailyTarget;
            bool currentDay = saved.Day == CollectionSession.Today;
            bool ready = currentDay && session.Progress.CanClaim;
            walkTitle.text = saved.Claimed ? "Today's box is yours" : ready ? "Today's free box is ready" : "Today's free box";
            walkProgress.text = Mathf.Min(saved.Steps, target).ToString("N0") + " of " + target.ToString("N0") + " steps";
            walkFill.fillAmount = Mathf.Clamp01(saved.Steps / (float)target);
            walkStatus.text = !currentDay ? "Check your device date to keep walking." : saved.Claimed ? NextBoxIn() : session.WalkingStatus;
            openDaily.interactable = ready;
            openDaily.GetComponentInChildren<Text>().text = saved.Claimed ? "Opened today" : ready ? "Open your box" : "Walk to unlock";
        }
    }
}
