#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Pockle.Tests
{
    /// <summary>Launch checkout (owner, November 3): collection boxes and Mystery Mix, server odds before paying, honest states.</summary>
    public sealed class CheckoutUiTests
    {
        private static readonly string[] Keys = { "pockle.onboarding.v1.ageConfirmed", "pockle.onboarding.v1.ageLocked", "pockle.onboarding.v1.completed" };
        private readonly int[] previous = new int[3];
        private readonly bool[] had = new bool[3];
        private GameObject root;
        private PrototypeHud hud;
        private CollectionSession session;
        private string wally, other;
        private readonly List<(string offer, string collection)> quotes = new List<(string, string)>();
        private readonly List<string> confirmed = new List<string>();
        private readonly List<(string quote, CheckoutState state)> closed = new List<(string, CheckoutState)>();
        private int recoveries;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            for (int i = 0; i < Keys.Length; i++) { had[i] = PlayerPrefs.HasKey(Keys[i]); previous[i] = PlayerPrefs.GetInt(Keys[i]); PlayerPrefs.DeleteKey(Keys[i]); }
            root = new GameObject("Checkout UI test");
            session = root.AddComponent<CollectionSession>(); session.Initialize();
            hud = root.AddComponent<PrototypeHud>();
            hud.Initialize(_ => { }, _ => { }, _ => { }, _ => { }); hud.Bind(session);
            quotes.Clear(); confirmed.Clear(); closed.Clear(); recoveries = 0;
            hud.CheckoutQuoteRequested += (offer, collection) => quotes.Add((offer, collection));
            hud.CheckoutConfirmed += quote => confirmed.Add(quote);
            hud.CheckoutClosed += (quote, state) => closed.Add((quote, state));
            hud.CheckoutRecoveryRequested += () => recoveries++;
            ToyCatalog.TryGetCollectible(WallyCatalog.KelpId, out var kelp); wally = kelp.CollectionId;
            other = ToyCatalog.Collections.First(c => c.Id != wally).Id;
            yield return null;
            ButtonWithText("Boxes").onClick.Invoke();
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(root); yield return null;
            for (int i = 0; i < Keys.Length; i++) { if (had[i]) PlayerPrefs.SetInt(Keys[i], previous[i]); else PlayerPrefs.DeleteKey(Keys[i]); }
            PlayerPrefs.Save();
        }

        private Button ButtonWithText(string value) => root.GetComponentsInChildren<Button>().First(b => b.GetComponentsInChildren<Text>().Any(t => t.text == value));
        private Button Named(string name) => root.GetComponentsInChildren<Button>().First(b => b.name == name);
        private bool HasButton(string name) => root.GetComponentsInChildren<Button>().Any(b => b.name == name);
        private IEnumerable<string> Texts => root.GetComponentsInChildren<Text>().Where(t => t.enabled).Select(t => t.text);
        private string Wally => ToyCatalog.TryGetCollection(wally, out var c) ? c.DisplayName : wally;
        private string Other => ToyCatalog.TryGetCollection(other, out var c) ? c.DisplayName : other;

        private CheckoutCatalog Catalog(bool otherSoldOut = false) => new CheckoutCatalog
        {
            Status = CheckoutCatalogStatus.Ready,
            Offers = new[]
            {
                new CheckoutOffer { OfferId = "single." + wally, Kind = CheckoutOfferKind.CollectionBox, CollectionId = wally, LocalizedPrice = "0,99 €" },
                new CheckoutOffer { OfferId = "single." + other, Kind = CheckoutOfferKind.CollectionBox, CollectionId = other, LocalizedPrice = "0,99 €",
                    Available = !otherSoldOut, UnavailableReason = otherSoldOut ? "Sold out" : null },
                new CheckoutOffer { OfferId = "mix3", Kind = CheckoutOfferKind.MysteryMix, BoxCount = 3, LocalizedPrice = "2,49 €" },
                new CheckoutOffer { OfferId = "mix5", Kind = CheckoutOfferKind.MysteryMix, BoxCount = 5, LocalizedPrice = "3,99 €" }
            }
        };

        private CheckoutQuote WallyQuote(string id = "q1") => new CheckoutQuote
        {
            QuoteId = id, OfferId = "single." + wally, Kind = CheckoutOfferKind.CollectionBox, CollectionId = wally, LocalizedPrice = "0,99 €",
            Odds = new[] { new CheckoutOddsGroup { CollectionId = wally, Rows = new[] {
                new CheckoutOddsRow { Tier = RarityTier.Common, Percent = 77.187 }, new CheckoutOddsRow { Tier = RarityTier.Rare, Percent = 15.4374 },
                new CheckoutOddsRow { Tier = RarityTier.Epic, Percent = 6.0034 }, new CheckoutOddsRow { Tier = RarityTier.Secret, Percent = 1.3722, Remaining = 1598 } } } },
            Notes = new[] { "Common toys you're missing are a little more likely. Rare, Epic and Secret chances stay the same." }
        };

        [UnityTest]
        public IEnumerator WithoutALiveCatalogTheOldTestCardsStayAndNothingIsSold()
        {
            Assert.That(hud.LaunchShopActive, Is.False);
            Assert.That(HasButton("Buy · Jelly Garden"), Is.True, "the unconnected test shop is unchanged until Codex binds products");
            Assert.That(Texts.Any(t => t.Contains("Pick a collection")), Is.False);
            yield return null;
        }

        [UnityTest]
        public IEnumerator TheLiveCatalogShowsStorePricesForEachCollectionAndBothMixes()
        {
            hud.SetCheckoutCatalog(Catalog(otherSoldOut: true)); yield return null;
            Assert.That(HasButton("Buy · Jelly Garden"), Is.False, "test cards hide once real products exist");
            Assert.That(Texts, Has.Member("Pick a collection")); Assert.That(Texts, Has.Member("Mystery Mix"));
            Assert.That(Named("Buy · " + Wally).GetComponentInChildren<Text>().text, Is.EqualTo("0,99 €"), "the store's own localized price");
            Assert.That(Named("Buy · Mystery Mix 3").GetComponentInChildren<Text>().text, Is.EqualTo("2,49 €"));
            Assert.That(Named("Buy · Mystery Mix 5").GetComponentInChildren<Text>().text, Is.EqualTo("3,99 €"));
            var soldOut = Named("Buy · " + Other);
            Assert.That(soldOut.interactable, Is.False); Assert.That(soldOut.GetComponentInChildren<Text>().text, Is.EqualTo("Sold out"));
            Assert.That(Texts, Has.Member("Still tradable"));
            Assert.That(Texts.Any(t => t.Contains("$")), Is.False, "no hand-built dollar prices");
        }

        [UnityTest]
        public IEnumerator LoadingAndUnavailableCataloguesSellNothing()
        {
            hud.SetCheckoutCatalog(new CheckoutCatalog { Status = CheckoutCatalogStatus.Loading }); yield return null;
            Assert.That(Texts, Has.Member("Loading prices…"));
            Assert.That(root.GetComponentsInChildren<Button>().Any(b => b.name.StartsWith("Buy · ")), Is.False);
            hud.SetCheckoutCatalog(new CheckoutCatalog { Status = CheckoutCatalogStatus.Unavailable, Message = "Purchases aren't available on this device." }); yield return null;
            Assert.That(Texts, Has.Member("Purchases aren't available on this device."));
            var noPrice = Catalog(); ((CheckoutOffer)noPrice.Offers[0]).LocalizedPrice = null;
            hud.SetCheckoutCatalog(noPrice); yield return null;
            Assert.That(Named("Buy · " + Wally).interactable, Is.False, "no store price, no checkout");
        }

        [UnityTest]
        public IEnumerator BuyIsOnlyEnabledOnceTheServerQuoteAndItsOddsArrive()
        {
            hud.SetCheckoutCatalog(Catalog()); yield return null;
            Named("Buy · " + Wally).onClick.Invoke(); yield return null;
            Assert.That(hud.CheckoutVisible, Is.True);
            Assert.That(quotes, Is.EqualTo(new[] { ("single." + wally, wally) }));
            Assert.That(hud.CheckoutCurrentState, Is.EqualTo(CheckoutState.LoadingQuote));
            Assert.That(Named("Buy for 0,99 €").interactable, Is.False, "no Buy before the quote");
            Assert.That(Texts, Has.Member("Checking what's left…"));
            Assert.That(hud.ShowCheckoutQuote(WallyQuote()), Is.True); yield return null;
            Assert.That(Named("Buy for 0,99 €").interactable, Is.True);
            Assert.That(Texts, Has.Member("77.19%")); Assert.That(Texts, Has.Member("15.44%"));
            Assert.That(Texts, Has.Member("6.00%")); Assert.That(Texts, Has.Member("1.37%"));
            Assert.That(Texts, Has.Member("about 1 in 73 · 1,598 left"), "real remaining copies only when the server sends them");
            Assert.That(Texts.Any(t => t.StartsWith("Common toys you're missing")), Is.True, "server rule lines are shown as written");
            Named("Buy for 0,99 €").onClick.Invoke(); yield return null;
            Assert.That(confirmed, Is.EqualTo(new[] { "q1" }));
            Assert.That(hud.CheckoutCurrentState, Is.EqualTo(CheckoutState.WaitingForStore));
            Assert.That(hud.GoBack(), Is.True); Assert.That(hud.CheckoutVisible, Is.True, "Back can't close the sheet while the store window is open");
        }

        [UnityTest]
        public IEnumerator AnIncompleteOrMismatchedQuoteNeverEnablesBuy()
        {
            hud.SetCheckoutCatalog(Catalog()); yield return null;
            Named("Buy · " + Wally).onClick.Invoke(); yield return null;
            var wrongOffer = WallyQuote(); wrongOffer.OfferId = "mix3";
            Assert.That(hud.ShowCheckoutQuote(wrongOffer), Is.False, "a quote for another offer is ignored");
            Assert.That(hud.CheckoutCurrentState, Is.EqualTo(CheckoutState.LoadingQuote));
            var empty = WallyQuote(); empty.Odds = new CheckoutOddsGroup[0];
            Assert.That(hud.ShowCheckoutQuote(empty), Is.False);
            Assert.That(hud.CheckoutCurrentState, Is.EqualTo(CheckoutState.Failed));
            Assert.That(Texts.Any(t => t.StartsWith("We couldn't load the odds")), Is.True);
            Named("Try again").onClick.Invoke(); yield return null;
            Assert.That(quotes.Count, Is.EqualTo(2), "Try again asks for a fresh quote");
            Assert.That(confirmed, Is.Empty);
        }

        [UnityTest]
        public IEnumerator MysteryMixShowsTheChanceOfEachCollectionThenItsOdds()
        {
            hud.SetCheckoutCatalog(Catalog()); yield return null;
            Named("Buy · Mystery Mix 3").onClick.Invoke(); yield return null;
            Assert.That(quotes.Last(), Is.EqualTo(("mix3", (string)null)), "a mix never sends a collection");
            hud.ShowCheckoutQuote(new CheckoutQuote
            {
                QuoteId = "m1", OfferId = "mix3", Kind = CheckoutOfferKind.MysteryMix, BoxCount = 3, LocalizedPrice = "2,49 €",
                Odds = new[]
                {
                    new CheckoutOddsGroup { CollectionId = wally, CollectionPercent = 50, Rows = new[] { new CheckoutOddsRow { Tier = RarityTier.Common, Percent = 100 } } },
                    new CheckoutOddsGroup { CollectionId = other, CollectionPercent = 50, Rows = new[] { new CheckoutOddsRow { Tier = RarityTier.Common, Percent = 100 } } }
                }
            }); yield return null;
            Assert.That(Texts, Has.Member("Mystery Mix · 3 boxes"));
            Assert.That(Texts, Has.Member(Wally + " · 50.0% of boxes")); Assert.That(Texts, Has.Member(Other + " · 50.0% of boxes"));
            Assert.That(Texts.Any(t => t.Contains("No guaranteed rare")), Is.True);
            Assert.That(Named("Buy for 2,49 €").interactable, Is.True);
        }

        [UnityTest]
        public IEnumerator PaymentStatesAreHonestAndNeverAskToPayAgain()
        {
            hud.SetCheckoutCatalog(Catalog()); yield return null;
            Named("Buy · " + Wally).onClick.Invoke(); hud.ShowCheckoutQuote(WallyQuote()); yield return null;
            Named("Buy for 0,99 €").onClick.Invoke();
            hud.SetCheckoutState("q1", CheckoutState.Cancelled); yield return null;
            Assert.That(Texts, Has.Member("Purchase cancelled. You weren't charged."));
            Assert.That(Named("Buy for 0,99 €").interactable, Is.True, "they can change their mind");
            Named("Buy for 0,99 €").onClick.Invoke();
            hud.SetCheckoutState("stale", CheckoutState.Failed); yield return null;
            Assert.That(hud.CheckoutCurrentState, Is.EqualTo(CheckoutState.WaitingForStore), "a stale quote's callback is ignored");
            hud.SetCheckoutState("q1", CheckoutState.PaymentPending); yield return null;
            Assert.That(HasButton("Buy for 0,99 €"), Is.False, "no second purchase while one is pending");
            Assert.That(Texts.Any(t => t.Contains("no need to buy again")), Is.True);
            hud.SetCheckoutState("q1", CheckoutState.DeliveryPending); yield return null;
            Assert.That(Texts.Any(t => t.Contains("You won't be charged again")), Is.True);
            Named("Close").onClick.Invoke(); yield return null;
            Assert.That(hud.CheckoutVisible, Is.False);
            Assert.That(closed.Last(), Is.EqualTo(("q1", CheckoutState.DeliveryPending)), "Codex knows not to release a paid hold");
        }

        [UnityTest]
        public IEnumerator DeliveredClosesTheSheetSoTheRevealCanShow()
        {
            hud.SetCheckoutCatalog(Catalog()); yield return null;
            Named("Buy · " + Wally).onClick.Invoke(); hud.ShowCheckoutQuote(WallyQuote()); yield return null;
            Named("Buy for 0,99 €").onClick.Invoke();
            hud.SetCheckoutState("q1", CheckoutState.Delivered); yield return null;
            Assert.That(hud.CheckoutVisible, Is.False);
            Assert.That(closed, Is.Empty, "a delivery is not a cancellation");
            Assert.That(hud.ShowBoxReveal(new BoxRevealView { ReceiptId = "r", Items = new[] { new BoxRevealItem { CollectibleId = WallyCatalog.KelpId, IsNew = true } } }), Is.True);
        }

        [UnityTest]
        public IEnumerator ChangedOddsAndSellingOutAreShownBeforeAnyCharge()
        {
            hud.SetCheckoutCatalog(Catalog()); yield return null;
            Named("Buy · " + Wally).onClick.Invoke(); hud.ShowCheckoutQuote(WallyQuote("q1")); yield return null;
            hud.ShowCheckoutQuote(WallyQuote("q2")); yield return null;
            Assert.That(hud.CheckoutQuoteId, Is.EqualTo("q2"));
            Assert.That(Texts.Any(t => t.StartsWith("What's left changed")), Is.True);
            var soldOut = Catalog(); ((CheckoutOffer)soldOut.Offers[0]).Available = false;
            hud.SetCheckoutCatalog(soldOut); yield return null;
            Assert.That(hud.CheckoutCurrentState, Is.EqualTo(CheckoutState.SoldOut));
            Assert.That(HasButton("Buy for 0,99 €"), Is.False);
            Assert.That(Texts.Any(t => t.Contains("can still be traded")), Is.True);
            Assert.That(confirmed, Is.Empty);
        }

        [UnityTest]
        public IEnumerator PaidBoxesOnTheWayShowABannerThatOnlyChecksAgain()
        {
            hud.SetPendingDeliveries(2); yield return null;
            Assert.That(Texts, Has.Member("2 paid boxes are on their way"));
            Named("Check again").onClick.Invoke();
            Assert.That(recoveries, Is.EqualTo(1));
            Assert.That(HasButton("Buy · Jelly Garden"), Is.True, "the banner works before the live catalog too");
            hud.SetCheckoutCatalog(Catalog()); hud.SetPendingDeliveries(1); yield return null;
            Assert.That(Texts, Has.Member("1 paid box is on its way"));
            hud.SetPendingDeliveries(0); yield return null;
            Assert.That(HasButton("Check again"), Is.False);
        }

        [UnityTest]
        public IEnumerator NotNowAndBackCloseTheSheetAndReleaseTheQuote()
        {
            hud.SetCheckoutCatalog(Catalog()); yield return null;
            Named("Buy · " + Wally).onClick.Invoke(); hud.ShowCheckoutQuote(WallyQuote()); yield return null;
            Named("Not now").onClick.Invoke(); yield return null;
            Assert.That(closed.Last(), Is.EqualTo(("q1", CheckoutState.Ready)));
            Named("Buy · " + Wally).onClick.Invoke(); yield return null;
            Assert.That(hud.GoBack(), Is.True); Assert.That(hud.CheckoutVisible, Is.False);
            Assert.That(closed.Last(), Is.EqualTo(((string)null, CheckoutState.LoadingQuote)));
        }

        [Test]
        public void OddsTextIsInvariantAndOneInOnlyForSmallChances()
        {
            Assert.That(new CheckoutOddsRow { Percent = 77.187 }.PercentText, Is.EqualTo("77.19%"));
            Assert.That(new CheckoutOddsRow { Percent = 15.4374 }.PercentText, Is.EqualTo("15.44%"));
            Assert.That(new CheckoutOddsRow { Percent = 1.3722 }.OneInText, Is.EqualTo("about 1 in 73"));
            Assert.That(new CheckoutOddsRow { Percent = 15 }.OneInText, Is.Null);
            Assert.That(new CheckoutOddsRow { Percent = 0 }.PercentText, Is.EqualTo("0%"));
        }
    }
}
#endif
