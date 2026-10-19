using System;
namespace Pockle.Core
{
    /// <summary>Wally's ocean finishes. Availability never changes ownership or daily reward pools.</summary>
    public static class WallyCatalog
    {
        public const string ClassicId = "wally.gloss-vinyl";
        public const string SpecialId = "wally.glow-jelly";
        private static readonly string[] labels = { "Lagoon Jelly", "Moon Pearl", "Coral Glitter", "Seafoam Mochi", "Lavender Flock", "Cloud Plush", "Reef Matte", "Classic Blue", "Sunken Gold", "Deep Sea Glow · Ultra rare" };
        public static string Label(int index) => labels[index];
        public static CatalogColor Swatch(string finish)
        {
            switch(finish)
            {
                case "clear-jelly": return new CatalogColor(0x27c6d2);
                case "pearl-jelly": return new CatalogColor(0xaaa5ee);
                case "confetti-gel": return new CatalogColor(0xf982a5);
                case "mochi-foam": return new CatalogColor(0x99ddbe);
                case "velvet-flock": return new CatalogColor(0x9865c5);
                case "boucle-plush": return new CatalogColor(0xf2bf8e);
                case "matte-vinyl": return new CatalogColor(0x2d91a7);
                case "coated-metallic": return new CatalogColor(0xe2ad4d);
                case "glow-jelly": return new CatalogColor(0x477ce7);
                default: return new CatalogColor(0x69bfe7);
            }
        }
    }
}
