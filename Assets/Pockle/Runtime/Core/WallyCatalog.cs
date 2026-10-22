using System;
using System.Collections.Generic;
namespace Pockle.Core
{
    /// <summary>Wally's ocean finishes. Availability never changes ownership or daily reward pools.</summary>
    public static class WallyCatalog
    {
        public static IReadOnlyList<CollectionWorldDefinition> Worlds { get; } = Array.AsReadOnly(new[] {
            new CollectionWorldDefinition("wally-coral-cove","Coral Cove"),
            new CollectionWorldDefinition("wally-pearl-grotto","Pearl Grotto"),
            new CollectionWorldDefinition("wally-kelp-garden","Kelp Garden"),
            new CollectionWorldDefinition("wally-sunset-shallows","Sunset Shallows"),
            new CollectionWorldDefinition("wally-midnight-trench","Midnight Trench")
        });
        public const string ClassicId = "wally.gloss-vinyl";
        public const string GlowId = "wally.glow-jelly";
        public const string KelpId = "wally.kelp-mint";
        public const string LeafId = "wally.treasure-leaf";
        public const string RoseId = "wally.rose-gold-shell";
        public const string DiamondId = "wally.diamond-frost";
        public const string SpecialId = "wally.molten-abyss";
        public static ToyTier Tier(string id)
        {
            if(id==SpecialId)return ToyTier.Secret;
            if(id==GlowId||id==DiamondId)return ToyTier.Epic;
            if(id=="wally.coated-metallic"||id==LeafId||id==RoseId)return ToyTier.Rare;
            return ToyTier.Common;
        }
        public static bool EmitsLight(string id) => id==GlowId||id==SpecialId;
        public static CatalogColor SwatchFor(string id,string finish)
        {
            switch(id)
            {
                case KelpId:return new CatalogColor(0x57bb86);
                case LeafId:return new CatalogColor(0xf5ce61);
                case RoseId:return new CatalogColor(0xd28c93);
                case DiamondId:return new CatalogColor(0xace5fa);
                case SpecialId:return new CatalogColor(0x182238);
                default:return Swatch(finish);
            }
        }
        private static readonly string[] labels = { "Lagoon Jelly", "Moon Pearl", "Coral Glitter", "Seafoam Mochi", "Lavender Flock", "Cloud Plush", "Reef Matte", "Classic Blue", "Sunken Gold", "Deep Sea Glow" };
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
