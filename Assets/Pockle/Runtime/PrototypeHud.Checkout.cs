using System;
using System.Collections.Generic;
using System.Globalization;
using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// Launch checkout (owner, November 3). Once Codex hands over a live catalog, the Boxes page shows "Pick a collection"
    /// (one $0.99-class box per collection, the store's own price) and "Mystery Mix" (3 and 5 boxes), plus a banner for
    /// paid boxes still being saved. Tapping an offer opens a review sheet that shows the server quote's own odds before
    /// any purchase, then follows Codex's payment states. Contract in <see cref="CheckoutCatalog"/>.
    /// </summary>
    public sealed partial class PrototypeHud
    {
        private const float CheckoutTileHeight = 200, CheckoutMixHeight = 118;

        private CheckoutCatalog checkoutCatalog;
        private RectTransform launchShop, pendingBanner;
        private Text pendingTitle;
        private int pendingDeliveries;
        private float launchShopHeight, storeWidth = 350, launchShopBuiltWidth;

        private RectTransform checkoutRoot, checkoutSheet, checkoutOdds, checkoutOddsContent;
        private RawImage checkoutArt;
        private Text checkoutTitle, checkoutSubtitle, checkoutPrice, checkoutStatus;
        private Button checkoutPrimary, checkoutSecondary;
        private Text checkoutPrimaryText, checkoutSecondaryText;
        private CheckoutOffer checkoutOffer;
        private CheckoutQuote checkoutQuote;
        private CheckoutState checkoutState;

        /// <summary>Codex: fetch a fresh server quote for this offer (collectionId set for a Collection Box).</summary>
        public event Action<string, string> CheckoutQuoteRequested;
        /// <summary>The player pressed Buy on this quote: open the native store sheet.</summary>
        public event Action<string> CheckoutConfirmed;
        /// <summary>"Check again" on the paid-box banner: look for saved deliveries. Never a new purchase.</summary>
        public event Action CheckoutRecoveryRequested;
        /// <summary>The player closed the sheet: (quote id or null, the state it was in). Release a hold only for an unpaid
        /// state; PaymentPending/DeliveryPending keep going and show on the paid-box banner.</summary>
        public event Action<string, CheckoutState> CheckoutClosed;

        public bool CheckoutVisible => checkoutRoot != null && checkoutRoot.gameObject.activeSelf;
        public CheckoutState CheckoutCurrentState => checkoutState;
        public string CheckoutQuoteId => checkoutQuote?.QuoteId;
        /// <summary>True once Codex has handed over a live catalog. Before that the Boxes page shows the launch lineup as
        /// "Coming soon" (owner, November 4: the old Jelly Garden / Midnight Glow / Gold Confetti test boxes are retired).</summary>
        public bool LaunchShopActive => checkoutCatalog != null;

        /// <summary>The launch lineup with nothing for sale: one Collection Box per shown collection, then Mystery Mix 3 and 5.</summary>
        private static CheckoutCatalog PreviewCatalog()
        {
            var offers = new List<CheckoutOffer>();
            foreach (var collection in ShownCollections())
                offers.Add(new CheckoutOffer { OfferId = "preview.box." + collection.Id, Kind = CheckoutOfferKind.CollectionBox, CollectionId = collection.Id,
                    Available = false, ComingSoon = true, UnavailableReason = "Coming soon" });
            foreach (int count in new[] { 3, 5 })
                offers.Add(new CheckoutOffer { OfferId = "preview.mix." + count, Kind = CheckoutOfferKind.MysteryMix, BoxCount = count,
                    Available = false, ComingSoon = true, UnavailableReason = "Coming soon" });
            return new CheckoutCatalog { Status = CheckoutCatalogStatus.Ready, Offers = offers };
        }

        // ---------- The Boxes page ----------

        /// <summary>Codex: the live offers (store prices, availability). Replaces the old test cards.</summary>
        public void SetCheckoutCatalog(CheckoutCatalog catalog)
        {
            checkoutCatalog = catalog;
            if (boxesRoot == null) return;
            BuildLaunchShop();
            if (CheckoutVisible && checkoutOffer != null && !(checkoutState == CheckoutState.WaitingForStore || checkoutState == CheckoutState.DeliveryPending || checkoutState == CheckoutState.PaymentPending))
            {
                // The offer on screen went away or sold out while the player was reading.
                var fresh = FindCheckoutOffer(checkoutOffer.OfferId);
                if (fresh == null || !fresh.Available) SetCheckoutState(checkoutQuote?.QuoteId, CheckoutState.SoldOut);
            }
            AdaptLayout();
        }

        /// <summary>Codex: paid boxes not yet saved for this account (0 hides the banner).</summary>
        public void SetPendingDeliveries(int count)
        {
            pendingDeliveries = Mathf.Max(0, count);
            BuildLaunchShop(); // the banner changes the page's height
            AdaptLayout();
        }

        private CheckoutOffer FindCheckoutOffer(string offerId)
        {
            if (checkoutCatalog?.Offers == null) return null;
            foreach (var offer in checkoutCatalog.Offers) if (offer != null && offer.OfferId == offerId) return offer;
            return null;
        }

        private void BuildLaunchShop()
        {
            if (boxesRoot == null) return;
            if (launchShop != null) { launchShop.gameObject.SetActive(false); Destroy(launchShop.gameObject); }
            launchShop = null; pendingBanner = null; pendingTitle = null;
            bool preview = checkoutCatalog == null;
            var catalog = checkoutCatalog ?? PreviewCatalog();
            // The old test cards never show any more: the launch lineup replaces them, "Coming soon" until products exist.
            foreach (var card in storeCards) if (card != null) card.gameObject.SetActive(false);
            if (shopHeading != null) shopHeading.gameObject.SetActive(false);
            if (catalogNote != null) catalogNote.gameObject.SetActive(false);

            launchShop = Rect("Launch shop", boxesRoot);
            float w = Mathf.Min(storeWidth, 480), y = 0; launchShopBuiltWidth = w;

            // Paid but not yet saved: always first, never a buy button.
            pendingBanner = Rect("Paid boxes on the way", launchShop); LeftLabel(pendingBanner, new Vector2(0, -y), new Vector2(w, 96));
            Surface(pendingBanner, PockleTheme.FieldGold, 22);
            pendingTitle = Label("", pendingBanner, PockleTheme.BodySize, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            LeftLabel(pendingTitle.rectTransform, new Vector2(16, -12), new Vector2(w - 32, 22));
            var pendingText = Label("Your payment went through. It'll show up here once it's saved, and you won't be charged again.", pendingBanner,
                PockleTheme.SmallSize, Ink, FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
            LeftLabel(pendingText.rectTransform, new Vector2(16, -36), new Vector2(w - 150, 50));
            var recheck = CreateButton("Check again", pendingBanner, Vector2.zero, new Vector2(116, 40), PockleTheme.Plum, () => CheckoutRecoveryRequested?.Invoke(), out _, PockleTheme.CaptionSize);
            var recheckRect = (RectTransform)recheck.transform; recheckRect.anchorMin = recheckRect.anchorMax = recheckRect.pivot = new Vector2(1, 0);
            recheckRect.anchoredPosition = new Vector2(-14, 14);
            pendingTitle.text = pendingDeliveries == 1 ? "1 paid box is on its way" : pendingDeliveries + " paid boxes are on their way";
            pendingBanner.gameObject.SetActive(pendingDeliveries > 0);
            if (pendingDeliveries > 0) y += 96 + 20;

            if (catalog.Status != CheckoutCatalogStatus.Ready)
            {
                string text = catalog.Status == CheckoutCatalogStatus.Loading ? "Loading prices…"
                    : string.IsNullOrEmpty(catalog.Message) ? "Boxes aren't for sale on this device right now." : catalog.Message;
                var note = Label(text, launchShop, PockleTheme.BodySize, MutedInk, FontStyle.Normal, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
                note.name = "Shop status"; LeftLabel(note.rectTransform, new Vector2(0, -y), new Vector2(w, 60));
                launchShopHeight = y + 60; return;
            }

            var singles = new List<CheckoutOffer>(); var mixes = new List<CheckoutOffer>();
            foreach (var offer in catalog.Offers)
            {
                if (offer == null || string.IsNullOrEmpty(offer.OfferId)) continue;
                if (offer.Kind == CheckoutOfferKind.MysteryMix) mixes.Add(offer);
                else if (!string.IsNullOrEmpty(offer.CollectionId) && ToyCatalog.TryGetCollection(offer.CollectionId, out _)) singles.Add(offer);
            }

            if (singles.Count > 0)
            {
                y = ShopHeading("Pick a collection", "One box, one random toy from the collection you pick.", w, y);
                float tileW = (w - 12) / 2;
                for (int i = 0; i < singles.Count; i++)
                    BuildCollectionTile(singles[i], new Vector2(i % 2 * (tileW + 12), -(y + i / 2 * (CheckoutTileHeight + 12))), new Vector2(tileW, CheckoutTileHeight));
                y += Mathf.CeilToInt(singles.Count / 2f) * (CheckoutTileHeight + 12) + 14;
            }
            if (mixes.Count > 0)
            {
                y = ShopHeading("Mystery Mix", "Random collections and toys. Repeats can happen.", w, y);
                foreach (var mix in mixes) { BuildMixCard(mix, new Vector2(0, -y), new Vector2(w, CheckoutMixHeight)); y += CheckoutMixHeight + 12; }
                y += 2;
            }
            string foot = preview ? "Boxes go on sale when Pockle launches. Until then, walk to earn free ones."
                : "You'll see the odds before you pay. Extras become spares for fusing and trading.";
            var footnote = Label(foot, launchShop, PockleTheme.SmallSize, MutedInk,
                FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
            LeftLabel(footnote.rectTransform, new Vector2(4, -y), new Vector2(w - 8, 36));
            launchShopHeight = y + 40;
        }

        private float ShopHeading(string title, string subtitle, float w, float y)
        {
            var heading = Label(title, launchShop, PockleTheme.HeadingSize, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            LeftLabel(heading.rectTransform, new Vector2(4, -y), new Vector2(w - 8, 30));
            var sub = Label(subtitle, launchShop, PockleTheme.CaptionSize, MutedInk, FontStyle.Normal, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            LeftLabel(sub.rectTransform, new Vector2(4, -(y + 30)), new Vector2(w - 8, 20));
            return y + 60;
        }

        private void BuildCollectionTile(CheckoutOffer offer, Vector2 position, Vector2 size)
        {
            ToyCatalog.TryGetCollection(offer.CollectionId, out var collection);
            Color field = PockleTheme.CollectionField(offer.CollectionId, out Color ink, out _);
            var tile = Rect(collection.DisplayName + " · offer", launchShop); LeftLabel(tile, position, size); Card(tile, field);
            var tap = tile.gameObject.AddComponent<Button>(); tap.transition = Selectable.Transition.None; tap.interactable = offer.Buyable && !offer.ComingSoon;
            tap.onClick.AddListener(() => OpenCheckout(offer)); tap.name = "View contents · " + collection.DisplayName;
            var art = Rect("Box", tile); art.anchorMin = art.anchorMax = art.pivot = new Vector2(.5f, 1);
            var texture = CollectionLook.Box(collection, out float aspect);
            var raw = art.gameObject.AddComponent<RawImage>(); raw.texture = texture; raw.raycastTarget = false; raw.color = texture != null ? Color.white : Color.clear;
            float ah = 92; art.sizeDelta = new Vector2(Mathf.Min(size.x - 24, ah * Mathf.Max(.4f, aspect)), ah); art.anchoredPosition = new Vector2(0, -12);
            if (texture != null && offer.Available) BoxHype.Add(raw, Color.white, position.x * .01f);
            var name = Label(collection.DisplayName, tile, PockleTheme.BodySize + 1, ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            LeftLabel(name.rectTransform, new Vector2(8, -110), new Vector2(size.x - 16, 24));
            string label = offer.ComingSoon ? "Coming soon" : offer.Buyable ? offer.LocalizedPrice : !offer.Available ? (string.IsNullOrEmpty(offer.UnavailableReason) ? "Sold out" : offer.UnavailableReason) : "Price unavailable";
            Color button = ink == PockleTheme.OnPlum ? PockleTheme.Jelly : PockleTheme.Plum;
            var buy = CreateButton(label, tile, new Vector2(14, size.y - 14 - 44), new Vector2(size.x - 28, 44), offer.Buyable ? button : new Color(1, 1, 1, .45f),
                () => OpenCheckout(offer), out _, PockleTheme.BodySize);
            buy.name = "Buy · " + collection.DisplayName; buy.interactable = offer.Buyable && !offer.ComingSoon;
            if (!buy.interactable) buy.GetComponentInChildren<Text>().color = ink;
            if (!offer.Available && !offer.ComingSoon)
            {
                var traded = Label("Still tradable", tile, PockleTheme.SmallSize, ink, FontStyle.Normal, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
                LeftLabel(traded.rectTransform, new Vector2(8, -134), new Vector2(size.x - 16, 18));
            }
        }

        private void BuildMixCard(CheckoutOffer offer, Vector2 position, Vector2 size)
        {
            int count = Mathf.Max(1, offer.BoxCount);
            var card = Rect("Mystery Mix " + count + " · offer", launchShop); LeftLabel(card, position, size); Card(card, PockleTheme.Confetti);
            var tap = card.gameObject.AddComponent<Button>(); tap.transition = Selectable.Transition.None; tap.interactable = offer.Buyable && !offer.ComingSoon;
            tap.onClick.AddListener(() => OpenCheckout(offer)); tap.name = "View contents · Mystery Mix " + count;
            // A small fanned stack of real collection boxes (1024px renders): the mix's "any collection" picture.
            var fan = new List<Texture2D>();
            foreach (var c in ToyCatalog.Collections)
            {
                if (fan.Count >= Mathf.Min(count, 3)) break;
                var art = CollectionLook.Box(c, out _); if (art != null) fan.Add(art);
            }
            for (int i = fan.Count - 1; i >= 0; i--)
            {
                var box = Rect("Mix box", card); box.anchorMin = box.anchorMax = box.pivot = new Vector2(1, .5f);
                var raw = box.gameObject.AddComponent<RawImage>(); raw.texture = fan[i]; raw.raycastTarget = false;
                box.sizeDelta = new Vector2(96, 96); box.anchoredPosition = new Vector2(-6 - i * 38, (i - 1) * 6); box.localEulerAngles = new Vector3(0, 0, (i - 1) * 8);
            }
            var title = Label("Mystery Mix · " + count + " boxes", card, PockleTheme.BodySize + 2, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            LeftLabel(title.rectTransform, new Vector2(16, -14), new Vector2(size.x - 170, 26));
            string sub = string.IsNullOrEmpty(offer.SavingsNote) ? count + " random toys" : count + " random toys · " + offer.SavingsNote;
            var line = Label(sub, card, PockleTheme.SmallSize, PockleTheme.PlumSoft, FontStyle.Normal, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            LeftLabel(line.rectTransform, new Vector2(16, -40), new Vector2(size.x - 170, 20));
            string label = offer.ComingSoon ? "Coming soon" : offer.Buyable ? offer.LocalizedPrice : !offer.Available ? (string.IsNullOrEmpty(offer.UnavailableReason) ? "Unavailable" : offer.UnavailableReason) : "Price unavailable";
            var buy = CreateButton(label, card, new Vector2(16, size.y - 14 - 44), new Vector2(124, 44), offer.Buyable ? PockleTheme.Plum : new Color(1, 1, 1, .6f),
                () => OpenCheckout(offer), out _, PockleTheme.BodySize);
            buy.name = "Buy · Mystery Mix " + count; buy.interactable = offer.Buyable && !offer.ComingSoon;
            if (!buy.interactable) buy.GetComponentInChildren<Text>().color = Ink;
        }

        /// <summary>Called from LayoutStore: places the launch section at y and returns its height (0 when not live).</summary>
        private float LayoutLaunchShop(float width, float y)
        {
            storeWidth = width;
            float w = Mathf.Min(width, 480);
            if (launchShop == null || Mathf.Abs(w - launchShopBuiltWidth) > .5f) BuildLaunchShop(); // first layout or a new width
            if (launchShop == null) return 0;
            TopCentered(launchShop, new Vector2(0, -y), new Vector2(w, launchShopHeight));
            return launchShopHeight;
        }

        // ---------- The review sheet ----------

        private void OpenCheckout(CheckoutOffer offer)
        {
            if (offer == null || !offer.Buyable || offer.ComingSoon || canvas == null) return;
            if (checkoutRoot == null) BuildCheckoutSheet();
            checkoutOffer = offer; checkoutQuote = null;
            checkoutRoot.gameObject.SetActive(true); checkoutRoot.SetAsLastSibling();
            LayoutCheckoutSheet();
            FillCheckoutHeader(offer.Kind, offer.BoxCount, offer.CollectionId, offer.LocalizedPrice);
            ClearCheckoutOdds();
            ApplyCheckoutState(CheckoutState.LoadingQuote, null);
            CheckoutQuoteRequested?.Invoke(offer.OfferId, offer.Kind == CheckoutOfferKind.CollectionBox ? offer.CollectionId : null);
        }

        /// <summary>
        /// Codex: the accepted quote for the open offer. Returns false (and shows "couldn't load the odds") for an
        /// incomplete quote or one for a different offer: Buy is never enabled without the quote's own price and odds.
        /// </summary>
        public bool ShowCheckoutQuote(CheckoutQuote quote)
        {
            if (!CheckoutVisible || quote == null || checkoutOffer == null || quote.OfferId != checkoutOffer.OfferId) return false;
            if (checkoutState == CheckoutState.WaitingForStore || checkoutState == CheckoutState.PaymentPending || checkoutState == CheckoutState.DeliveryPending) return false;
            if (!quote.Complete) { checkoutQuote = null; ClearCheckoutOdds(); ApplyCheckoutState(CheckoutState.Failed, "We couldn't load the odds for this box, so it can't be bought right now. You weren't charged."); return false; }
            bool changed = checkoutQuote != null && checkoutQuote.QuoteId != quote.QuoteId;
            checkoutQuote = quote;
            FillCheckoutHeader(quote.Kind, quote.BoxCount, quote.CollectionId, quote.LocalizedPrice);
            FillCheckoutOdds(quote);
            ApplyCheckoutState(CheckoutState.Ready, changed ? "What's left changed, so the odds were updated. Please check them again." : null);
            return true;
        }

        /// <summary>Codex: payment progress for the quote on screen (a null quote id is accepted before any quote arrived).</summary>
        public void SetCheckoutState(string quoteId, CheckoutState state, string message = null)
        {
            if (!CheckoutVisible) return;
            if (checkoutQuote != null && quoteId != null && quoteId != checkoutQuote.QuoteId) return; // a stale callback
            if (checkoutQuote == null && state == CheckoutState.Ready) return; // Ready needs a quote: use ShowCheckoutQuote
            if (state == CheckoutState.Delivered) { CloseCheckout(false); return; } // Codex shows the reveal after saving
            ApplyCheckoutState(state, message);
        }

        private void BuildCheckoutSheet()
        {
            checkoutRoot = Rect("Checkout", canvas.transform); Stretch(checkoutRoot);
            var backdrop = checkoutRoot.gameObject.AddComponent<Image>(); backdrop.color = PockleTheme.Backdrop;
            var dismiss = checkoutRoot.gameObject.AddComponent<Button>(); dismiss.transition = Selectable.Transition.None;
            dismiss.onClick.AddListener(() => { if (CheckoutDismissible) CloseCheckout(true); });

            checkoutSheet = Rect("Checkout sheet", checkoutRoot);
            checkoutSheet.anchorMin = new Vector2(.5f, 0); checkoutSheet.anchorMax = new Vector2(.5f, 0); checkoutSheet.pivot = new Vector2(.5f, 0);
            Surface(checkoutSheet, PockleTheme.Milk, 28).raycastTarget = true; // taps on the sheet stay on the sheet

            var art = Rect("Box", checkoutSheet); LeftLabel(art, new Vector2(20, -20), new Vector2(72, 72));
            checkoutArt = art.gameObject.AddComponent<RawImage>(); checkoutArt.raycastTarget = false;
            checkoutTitle = Label("", checkoutSheet, PockleTheme.HeadingSize, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            checkoutSubtitle = Label("", checkoutSheet, PockleTheme.CaptionSize, MutedInk, FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
            checkoutPrice = Label("", checkoutSheet, PockleTheme.HeadingSize, Ink, FontStyle.Bold, TextAnchor.MiddleRight, Vector2.zero, Vector2.zero);

            checkoutOdds = Rect("Odds", checkoutSheet);
            checkoutOdds.gameObject.AddComponent<RectMask2D>();
            var hit = checkoutOdds.gameObject.AddComponent<Image>(); hit.color = new Color(0, 0, 0, 0); // drag area
            checkoutOddsContent = Rect("Odds content", checkoutOdds);
            checkoutOddsContent.anchorMin = new Vector2(0, 1); checkoutOddsContent.anchorMax = new Vector2(1, 1); checkoutOddsContent.pivot = new Vector2(.5f, 1);
            var scroll = checkoutOdds.gameObject.AddComponent<ScrollRect>(); scroll.horizontal = false; scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.content = checkoutOddsContent; scroll.viewport = checkoutOdds;

            checkoutStatus = Label("", checkoutSheet, PockleTheme.CaptionSize, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            checkoutStatus.name = "Checkout status";
            checkoutPrimary = CreateButton("Buy", checkoutSheet, Vector2.zero, new Vector2(100, 54), PockleTheme.Plum, CheckoutPrimaryTapped, out _);
            checkoutPrimaryText = checkoutPrimary.GetComponentInChildren<Text>();
            checkoutSecondary = CreateButton("Not now", checkoutSheet, Vector2.zero, new Vector2(100, 48), PockleTheme.Fill, () => CloseCheckout(true), out _, PockleTheme.CaptionSize + 1);
            checkoutSecondaryText = checkoutSecondary.GetComponentInChildren<Text>();
            LayoutCheckoutSheet();
            checkoutRoot.gameObject.SetActive(false);
        }

        private void LayoutCheckoutSheet()
        {
            float screenW = ((RectTransform)canvas.transform).rect.width, screenH = ((RectTransform)canvas.transform).rect.height;
            float w = Mathf.Min(screenW, 480), h = Mathf.Min(screenH * .86f, 680);
            float bottomInset = safeRoot != null ? Mathf.Max(0, safeRoot.anchorMin.y * screenH) : 0;
            checkoutSheet.sizeDelta = new Vector2(w, h + bottomInset); checkoutSheet.anchoredPosition = new Vector2(0, -28); // round top, square-looking bottom
            float pad = 20, textX = pad + 72 + 14;
            LeftLabel(checkoutTitle.rectTransform, new Vector2(textX, -22), new Vector2(w - textX - pad - 96, 30));
            LeftLabel(checkoutSubtitle.rectTransform, new Vector2(textX, -54), new Vector2(w - textX - pad, 40));
            LeftLabel(checkoutPrice.rectTransform, new Vector2(w - pad - 96, -22), new Vector2(96, 30));
            const float footer = 54 + 10 + 48 + 34 + 28;
            LeftLabel(checkoutOdds, new Vector2(pad, -108), new Vector2(w - pad * 2, h - 108 - footer));
            LeftLabel(checkoutStatus.rectTransform, new Vector2(pad, -(h - footer + 4)), new Vector2(w - pad * 2, 30));
            LeftLabel((RectTransform)checkoutPrimary.transform, new Vector2(pad, -(h - footer + 38)), new Vector2(w - pad * 2, 54));
            LeftLabel((RectTransform)checkoutSecondary.transform, new Vector2(pad, -(h - footer + 38 + 54 + 10)), new Vector2(w - pad * 2, 48));
            checkoutStatus.horizontalOverflow = HorizontalWrapMode.Wrap;
        }

        private string CheckoutCollectionName(string id) => ToyCatalog.TryGetCollection(id, out var c) ? c.DisplayName : "Pockle";

        private void FillCheckoutHeader(CheckoutOfferKind kind, int boxCount, string collectionId, string price)
        {
            bool mix = kind == CheckoutOfferKind.MysteryMix; int count = Mathf.Max(1, boxCount);
            string name = CheckoutCollectionName(collectionId);
            checkoutTitle.text = mix ? "Mystery Mix · " + count + " boxes" : name + " box";
            checkoutSubtitle.text = mix ? count + " random toys from random collections. No guaranteed rare, and repeats can happen."
                : "1 random " + name + " toy. Repeats can happen.";
            checkoutPrice.text = price ?? "";
            float aspect = 1; Texture2D art = null;
            if (!mix && ToyCatalog.TryGetCollection(collectionId, out var collection)) art = CollectionLook.Box(collection, out aspect);
            if (art == null) foreach (var c in ToyCatalog.Collections) { art = CollectionLook.Box(c, out aspect); if (art != null) break; } // a real 1024px box, not the old 380px shop art
            checkoutArt.texture = art; checkoutArt.color = art != null ? Color.white : Color.clear;
        }

        private void ClearCheckoutOdds()
        {
            for (int i = checkoutOddsContent.childCount - 1; i >= 0; i--) { var child = checkoutOddsContent.GetChild(i).gameObject; child.SetActive(false); Destroy(child); }
            checkoutOddsContent.sizeDelta = new Vector2(0, 0);
        }

        private void FillCheckoutOdds(CheckoutQuote quote)
        {
            ClearCheckoutOdds();
            float w = checkoutOdds.rect.width, y = 0;
            bool mix = quote.Kind == CheckoutOfferKind.MysteryMix;
            y = OddsLine(mix ? "Possible contents · odds per box" : "All possible contents", PockleTheme.BodySize, FontStyle.Bold, Ink, w, y, 26);
            if (mix) y = OddsLine("Each box picks a collection first, then a toy using that collection's odds.", PockleTheme.SmallSize, FontStyle.Normal, MutedInk, w, y, 36);
            foreach (var group in quote.Odds)
            {
                if (mix || quote.Odds.Count > 1)
                {
                    string head = CheckoutCollectionName(group.CollectionId);
                    if (group.CollectionPercent.HasValue) head += " · " + new CheckoutOddsRow { Percent = group.CollectionPercent.Value }.PercentText + " of boxes";
                    y = OddsLine(head, PockleTheme.CaptionSize, FontStyle.Bold, Ink, w, y + 6, 22);
                }
                for (int i = 0; i < group.Contents.Count; i++)
                    BuildCheckoutContentsTile(group.Contents[i], w, y, i);
                y += Mathf.CeilToInt(group.Contents.Count / 2f) * 198 + 8;
                y = OddsLine(mix ? "Rarity odds within this collection" : "Rarity odds", PockleTheme.CaptionSize, FontStyle.Bold, Ink, w, y, 24);
                foreach (var row in group.Rows) y = OddsRow(row, w, y);
            }
            if (quote.Notes != null)
                foreach (var note in quote.Notes) if (!string.IsNullOrEmpty(note)) y = OddsLine(note, PockleTheme.SmallSize, FontStyle.Normal, MutedInk, w, y + 6, 36);
            y = OddsLine("Chances shown are per box and rounded. Each box draws independently; repeats are possible. Extras stay as spares for fusing or trading.",
                PockleTheme.SmallSize, FontStyle.Normal, MutedInk, w, y + 8, 52);
            checkoutOddsContent.sizeDelta = new Vector2(0, y);
            checkoutOddsContent.anchoredPosition = Vector2.zero;
        }

        private void BuildCheckoutContentsTile(CheckoutContentsItem item, float w, float y, int index)
        {
            if (!ToyCatalog.TryGetCollectible(item.CollectibleId, out var toy)) return;
            float width = (w - 10) / 2;
            var tile = Rect("Possible toy · " + item.CollectibleId, checkoutOddsContent);
            LeftLabel(tile, new Vector2((index % 2) * (width + 10), -(y + index / 2 * 198)), new Vector2(width, 188));
            Surface(tile, PockleTheme.Fill, 16);
            var picture = Rect("Contents portrait", tile);
            LeftLabel(picture, new Vector2((width - 100) / 2, -4), new Vector2(100, 100));
            var raw = picture.gameObject.AddComponent<RawImage>(); raw.raycastTarget = false;
            raw.color = Color.clear;
            picture.gameObject.AddComponent<CheckoutContentsPortrait>().Initialize(item.CollectibleId, checkoutOdds, raw, CutoutPortrait);
            var name = Label(toy.FinishDisplayName, tile, PockleTheme.CaptionSize, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            name.horizontalOverflow = HorizontalWrapMode.Wrap;
            LeftLabel(name.rectTransform, new Vector2(6, -104), new Vector2(width - 12, 36));
            var rarity = Label(item.Tier == RarityTier.Common ? "Common" : RarityLook.Words(item.Tier), tile, PockleTheme.SmallSize, MutedInk, FontStyle.Normal, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            LeftLabel(rarity.rectTransform, new Vector2(6, -140), new Vector2(width - 12, 18));
            var chance = Label(item.PercentText + " per box", tile, PockleTheme.CaptionSize, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            LeftLabel(chance.rectTransform, new Vector2(6, -160), new Vector2(width - 12, 22));
        }

        private float OddsLine(string text, int size, FontStyle style, Color colour, float w, float y, float h)
        {
            var label = Label(text, checkoutOddsContent, size, colour, style, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            LeftLabel(label.rectTransform, new Vector2(0, -y), new Vector2(w, h));
            return y + h;
        }

        private float OddsRow(CheckoutOddsRow row, float w, float y)
        {
            const float h = 34;
            var line = Rect(RarityLook.Words(row.Tier) + " odds", checkoutOddsContent); LeftLabel(line, new Vector2(0, -y), new Vector2(w, h - 4));
            Surface(line, PockleTheme.Fill, 12);
            var look = RarityLook.For(row.Tier, row.Tier == RarityTier.Secret, RarityLook.Words(row.Tier));
            var dot = Rect("Tier", line); dot.anchorMin = dot.anchorMax = dot.pivot = new Vector2(0, .5f); dot.anchoredPosition = new Vector2(10, 0); dot.sizeDelta = new Vector2(12, 12);
            Surface(dot, row.Tier == RarityTier.Common ? PockleTheme.FillDeep : look.Fill, 6);
            var tier = Label(row.Tier == RarityTier.Common ? "Common" : RarityLook.Words(row.Tier), line, PockleTheme.CaptionSize, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            LeftLabel(tier.rectTransform, new Vector2(30, 0), new Vector2(w * .35f, h - 4));
            string detail = row.OneInText ?? "";
            if (row.Remaining.HasValue) detail = (detail.Length > 0 ? detail + " · " : "") + row.Remaining.Value.ToString("N0", CultureInfo.InvariantCulture) + " left";
            var extra = Label(detail, line, PockleTheme.SmallSize, MutedInk, FontStyle.Normal, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            LeftLabel(extra.rectTransform, new Vector2(30 + w * .3f, 0), new Vector2(w * .45f, h - 4));
            var percent = Label(row.PercentText, line, PockleTheme.CaptionSize, Ink, FontStyle.Bold, TextAnchor.MiddleRight, Vector2.zero, Vector2.zero);
            LeftLabel(percent.rectTransform, new Vector2(w - 90, 0), new Vector2(80, h - 4));
            return y + h;
        }

        private bool CheckoutDismissible => checkoutState != CheckoutState.WaitingForStore;

        private void ApplyCheckoutState(CheckoutState state, string message)
        {
            checkoutState = state;
            string price = checkoutQuote?.LocalizedPrice ?? checkoutOffer?.LocalizedPrice ?? "";
            string status; bool primaryShown = true, primaryOn = false, secondaryShown = true;
            string primary = "Buy for " + price, secondary = "Not now";
            switch (state)
            {
                case CheckoutState.LoadingQuote: status = "Checking what's left…"; secondary = "Cancel"; break;
                case CheckoutState.Ready: status = ""; primaryOn = true; break;
                case CheckoutState.WaitingForStore: status = "Finish in the store window."; secondaryShown = false; break;
                case CheckoutState.Cancelled: status = "Purchase cancelled. You weren't charged."; primaryOn = checkoutQuote != null; secondary = "Close"; break;
                case CheckoutState.Failed: status = "Something went wrong. You weren't charged."; primary = "Try again"; primaryOn = true; secondary = "Close"; break;
                case CheckoutState.PaymentPending: status = "Waiting for the payment to be approved. Your box arrives once it goes through, so there's no need to buy again."; primaryShown = false; secondary = "Close"; break;
                case CheckoutState.DeliveryPending: status = "Payment received. Saving your box… You won't be charged again."; primaryShown = false; secondary = "Close"; break;
                case CheckoutState.SoldOut: status = "This box just sold out. Sold-out toys can still be traded."; primaryShown = false; secondary = "Close"; break;
                default: status = ""; break;
            }
            checkoutStatus.text = string.IsNullOrEmpty(message) ? status : message;
            checkoutStatus.color = state == CheckoutState.Failed || state == CheckoutState.SoldOut ? PockleTheme.Heart : Ink;
            checkoutPrimary.gameObject.SetActive(primaryShown);
            checkoutPrimary.interactable = primaryOn;
            checkoutPrimaryText.text = primary; checkoutPrimary.name = primary;
            checkoutSecondary.gameObject.SetActive(secondaryShown);
            checkoutSecondaryText.text = secondary; checkoutSecondary.name = secondary;
        }

        private void CheckoutPrimaryTapped()
        {
            if (checkoutOffer == null) return;
            if (checkoutState == CheckoutState.Failed) { var offer = checkoutOffer; checkoutQuote = null; OpenCheckout(offer); return; } // a fresh quote
            if ((checkoutState != CheckoutState.Ready && checkoutState != CheckoutState.Cancelled) || checkoutQuote == null || !checkoutQuote.Complete) return;
            ApplyCheckoutState(CheckoutState.WaitingForStore, null);
            CheckoutConfirmed?.Invoke(checkoutQuote.QuoteId);
        }

        /// <summary>Closes the sheet. A delivery keeps going in the background; the banner shows it.</summary>
        private void CloseCheckout(bool byPlayer)
        {
            if (!CheckoutVisible) return;
            string quote = checkoutQuote?.QuoteId; var state = checkoutState;
            checkoutRoot.gameObject.SetActive(false);
            checkoutOffer = null; checkoutQuote = null;
            if (byPlayer) CheckoutClosed?.Invoke(quote, state);
        }

        /// <summary>The phone's Back on the sheet: closes it, except while the store window is open.</summary>
        private void CheckoutBack() { if (CheckoutDismissible) CloseCheckout(true); }
    }
}
