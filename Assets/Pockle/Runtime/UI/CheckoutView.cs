using System;
using System.Collections.Generic;
using System.Globalization;

namespace Pockle.Runtime
{
    // Checkout screens, Claude's side (owner, November 3: paid boxes are in the first release; three offers: a Collection
    // Box the player picks, Mystery Mix 3 and Mystery Mix 5). Codex's runtime owns store products, localized prices,
    // server quotes, odds, payment and delivery. The HUD only shows what it is handed and reports what the player chose.
    //
    // Flow:
    //   1. Codex: hud.SetCheckoutCatalog(catalog). Until then the Boxes page keeps the old test cards (nothing is sold).
    //   2. Player taps an offer -> hud.CheckoutQuoteRequested(offerId, collectionId). Codex asks the server for a fresh quote.
    //   3. Codex: hud.ShowCheckoutQuote(quote) -> review sheet: localized price, contents, the quote's own odds, Buy.
    //      (While waiting, the sheet shows "Checking what's left…"; Codex may also send a Failed/SoldOut state instead.)
    //   4. Player taps Buy -> hud.CheckoutConfirmed(quoteId). Codex opens the native store sheet.
    //   5. Codex: hud.SetCheckoutState(quoteId, state). Delivered closes the sheet; Codex then saves and calls
    //      hud.ShowBoxReveal. Nothing in the HUD grants a toy, and no client success message implies a saved toy.
    //   6. Paid-but-not-yet-saved purchases: hud.SetPendingDeliveries(count). The Boxes page shows a banner with "Check
    //      again" -> hud.CheckoutRecoveryRequested. Recovery never asks the player to buy again.

    public enum CheckoutOfferKind { CollectionBox, MysteryMix }

    public enum CheckoutCatalogStatus { Loading, Ready, Unavailable }

    public enum CheckoutState
    {
        /// <summary>The quote is on screen and Buy is enabled.</summary>
        Ready,
        /// <summary>A fresh quote is being fetched.</summary>
        LoadingQuote,
        /// <summary>The native store sheet is open.</summary>
        WaitingForStore,
        /// <summary>The player backed out of the store sheet. No charge.</summary>
        Cancelled,
        /// <summary>The store or server refused before any charge.</summary>
        Failed,
        /// <summary>Deferred payment (Ask to Buy, cash/bank pending). No toy until the store confirms.</summary>
        PaymentPending,
        /// <summary>Paid; the server is still saving the box. Recovery continues without another charge.</summary>
        DeliveryPending,
        /// <summary>Saved. The sheet closes; Codex shows the reveal.</summary>
        Delivered,
        /// <summary>Not enough supply left for this offer.</summary>
        SoldOut
    }

    /// <summary>One shop card. Prices are the store's own localized strings; never built from dollars in the client.</summary>
    public sealed class CheckoutOffer
    {
        public string OfferId;
        public CheckoutOfferKind Kind;
        /// <summary>1 for a Collection Box, 3 or 5 for a Mystery Mix.</summary>
        public int BoxCount = 1;
        /// <summary>The collection a Collection Box opens. Null for a Mystery Mix.</summary>
        public string CollectionId;
        /// <summary>The store's localized price, e.g. "$0.99" or "0,99 €". Null: not loaded, and the offer cannot be bought.</summary>
        public string LocalizedPrice;
        /// <summary>False when it can't be bought right now (sold out, not in this store yet).</summary>
        public bool Available = true;
        /// <summary>Shown on the card when not available, e.g. "Sold out".</summary>
        public string UnavailableReason;
        /// <summary>Optional, from the store's real local prices only (e.g. "Saves 48¢ vs. 3 singles"). Never computed in the HUD.</summary>
        public string SavingsNote;

        public bool Buyable => Available && !string.IsNullOrEmpty(LocalizedPrice);
    }

    public sealed class CheckoutCatalog
    {
        public CheckoutCatalogStatus Status = CheckoutCatalogStatus.Loading;
        /// <summary>Shown when Unavailable, e.g. "Purchases aren't available on this device."</summary>
        public string Message;
        public IReadOnlyList<CheckoutOffer> Offers = Array.Empty<CheckoutOffer>();
    }

    /// <summary>One tier's chance inside one collection, exactly as the accepted quote computed it.</summary>
    public sealed class CheckoutOddsRow
    {
        public RarityTier Tier;
        /// <summary>Percent, e.g. 77.187. Shown to two decimals.</summary>
        public double Percent;
        /// <summary>Optional real remaining copies for this tier, from the server. Null: not shown (never invented).</summary>
        public long? Remaining;

        public string PercentText => Percent <= 0 ? "0%" : Percent.ToString(Percent < 10 ? "0.00" : "0.0#", CultureInfo.InvariantCulture) + "%";
        /// <summary>"about 1 in 73" for chances under 5%, else null.</summary>
        public string OneInText => Percent > 0 && Percent < 5 ? "about 1 in " + Math.Round(100 / Percent).ToString("N0", CultureInfo.InvariantCulture) : null;
    }

    /// <summary>A collection's odds. For a Mystery Mix, <see cref="CollectionPercent"/> is the chance each box picks it.</summary>
    public sealed class CheckoutOddsGroup
    {
        public string CollectionId;
        /// <summary>Mystery Mix only: chance each box picks this collection. Null for a Collection Box.</summary>
        public double? CollectionPercent;
        public IReadOnlyList<CheckoutOddsRow> Rows = Array.Empty<CheckoutOddsRow>();
    }

    /// <summary>The server's accepted quote: the only source for the prices and odds shown before paying.</summary>
    public sealed class CheckoutQuote
    {
        public string QuoteId;
        public string OfferId;
        public CheckoutOfferKind Kind;
        public int BoxCount = 1;
        /// <summary>Collection Box only.</summary>
        public string CollectionId;
        public string LocalizedPrice;
        public IReadOnlyList<CheckoutOddsGroup> Odds = Array.Empty<CheckoutOddsGroup>();
        /// <summary>Server-written rule lines, e.g. the Common completion help. Shown as written.</summary>
        public IReadOnlyList<string> Notes = Array.Empty<string>();

        /// <summary>A quote can only be bought with a price and odds for every group.</summary>
        public bool Complete
        {
            get
            {
                if (string.IsNullOrEmpty(QuoteId) || string.IsNullOrEmpty(LocalizedPrice) || Odds == null || Odds.Count == 0) return false;
                foreach (var group in Odds) if (group == null || group.Rows == null || group.Rows.Count == 0) return false;
                return Kind != CheckoutOfferKind.CollectionBox || !string.IsNullOrEmpty(CollectionId);
            }
        }
    }
}
