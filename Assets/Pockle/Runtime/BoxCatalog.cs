namespace Pockle.Runtime
{
    /// <summary>Test catalog. Storefront prices must replace proposed USD labels when billing is connected.</summary>
    public sealed class BoxOffer
    {
        public readonly string ProductId, Name, ProposedPrice, Contents;
        public BoxOffer(string id, string name, string price, string contents)
        { ProductId = id; Name = name; ProposedPrice = price; Contents = contents; }
    }

    public static class BoxCatalog
    {
        // Proposed Google Play consumable IDs, to be configured before enabling checkout.
        public static readonly BoxOffer[] Offers = {
            new BoxOffer("pockle.jelly_garden.box", "Jelly Garden", "$0.99", "Peach / Mint · 50% each"),
            new BoxOffer("pockle.midnight_glow.box", "Midnight Glow", "$2.99", "Moon Jelly · 100%"),
            new BoxOffer("pockle.gold_confetti.box", "Gold Confetti", "$2.99", "Gold Glitter · 100%")
        };
    }

    public interface IBoxCheckout
    {
        // A real adapter must use store prices, validate purchases with the backend,
        // and persist each unique transaction once before consuming the purchase.
        bool Available { get; }
        string UnavailableReason { get; }
        void Begin(BoxOffer offer);
    }

    public sealed class UnconfiguredBoxCheckout : IBoxCheckout
    {
        public bool Available => false;
        public string UnavailableReason => "Purchases aren't available in this test build. Nothing will be charged.\n\nYou can earn a Jelly Garden box by walking 1,000 steps.";
        public void Begin(BoxOffer offer) { } // No SDK, charge, or inventory grant in this build.
    }
}
