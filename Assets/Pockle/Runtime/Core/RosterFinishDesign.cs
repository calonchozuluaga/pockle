using System;
namespace Pockle.Core
{
    /// <summary>Collection-specific pigments and additive IDs. Personality never changes appearance.</summary>
    public static class RosterFinishDesign
    {
        public const string Signature = "signature-matte", Leaf = "gilded-leaf", Rose = "rose-gold", Diamond = "diamond-prism", Secret = "secret-edition";
        public static string SecretId(string character) => character == "wally" ? WallyCatalog.SpecialId : character + "." + Secret;
        public static bool IsAddition(string id) => id.EndsWith("." + Signature, StringComparison.Ordinal) || id.EndsWith("." + Leaf, StringComparison.Ordinal) || id.EndsWith("." + Rose, StringComparison.Ordinal) || id.EndsWith("." + Diamond, StringComparison.Ordinal) || id.EndsWith("." + Secret, StringComparison.Ordinal);
        public static ToyTier Tier(string id, string character, string finish, string collection)
        {
            if (collection == PipFusionStudies.CollectionId) return ToyTier.Common;
            if (character == "wally") return WallyCatalog.Tier(id);
            if (id == SecretId(character)) return ToyTier.Secret;
            if (finish == "glow-jelly" || id.EndsWith("." + Diamond, StringComparison.Ordinal)) return ToyTier.Epic;
            if (finish == "coated-metallic") return ToyTier.Rare;
            return ToyTier.Common;
        }
        public static int CommonIndex(string finish) => finish == "clear-jelly" ? 0 : finish == "pearl-jelly" ? 1 : finish == "confetti-gel" ? 2 : finish == "mochi-foam" ? 3 : finish == "velvet-flock" ? 4 : finish == "boucle-plush" ? 5 : finish == "matte-vinyl" ? 6 : 7;
        // Nine readable hues per character. Classic (index 6) keeps the original uploaded paint where applicable.
        public static CatalogColor Pigment(string character, int index)
        {
            uint[] colors;
            switch (character)
            {
                case "pip": colors = new uint[] {0xf5b89c,0x73b8f5,0xffb02e,0x8cd9a8,0xb08ade,0xf2a2be,0xcce6fa,0xf65e92,0xffdaa0}; break;
                case "fox": colors = new uint[] {0xf4b13c,0xabcce9,0xe25c92,0xf2aaa4,0x9067c6,0x2d6571,0xf4ae72,0xff742f,0x80bd91}; break;
                case "axolotl": colors = new uint[] {0xf5bd53,0xc3c0f4,0xee6ca3,0x85d3ae,0x8b6cc4,0xf5b8c7,0xb5ddf2,0xf7647e,0x64bdc7}; break;
                case "firb": colors = new uint[] {0xd63267,0xc4b4ed,0x26baaa,0xffb192,0x8146b0,0x9cd5b3,0x548b66,0x3c73d6,0xf1ce79}; break;
                case "coco": colors = new uint[] {0xde743a,0xc7b9e8,0x39b6a1,0xf3c890,0x9b396c,0xe6a8b9,0x965334,0x4e8aca,0x9fc781}; break;
                case "snowy": colors = new uint[] {0x75d3ee,0xd2b9ed,0xe760a0,0xffc39e,0x7972c4,0x92d2b7,0xe8f5f6,0x397dcc,0xf5d77c}; break;
                case "ginger": colors = new uint[] {0xe99031,0xc5b4ef,0xe4457c,0x9cdab6,0x7455a7,0xeab4b9,0xd69b64,0x48a5bb,0xf0dc9c}; break;
                case "reiny": colors = new uint[] {0xe52e55,0xf2bacf,0x18b981,0xff9d83,0x833c9e,0x70c9c0,0xc39365,0x355ce4,0xeace6d}; break;
                case "guin": colors = new uint[] {0x29d9e8,0xaaa0f2,0xff775f,0xffdc80,0xa63379,0x8fdab1,0x7397b5,0x6639cc,0xf6b9d4}; break;
                default: colors = new uint[] {0xf3a329,0xf5d5b6,0xdd388c,0xffefb5,0x237b60,0xb09be2,0x99674d,0xd82c43,0x66b9d0}; break;
            }
            return new CatalogColor(colors[Math.Max(0, Math.Min(8, index))]);
        }
        public static string CommonLabel(string character, int index)
        {
            string[] labels;
            switch(character)
            {
                case "pip": labels = new[]{"Peach Jelly","Moon Jelly","Gold Glitter","Mint Soft","Lavender Flock","Rose Plush","Cloud Vinyl","Berry Gloss","Vanilla Custard"};break;
                case "fox": labels = new[]{"Amber Jelly","Bluebell Pearl","Berry Confetti","Mochi Pink","Violet Flock","Juniper Plush","Apricot Vinyl","Classic Orange","Meadow Mint"};break;
                case "axolotl": labels = new[]{"Honey Jelly","Lilac Pearl","Petal Confetti","Mint Mochi","Orchid Flock","Blush Plush","Ice Vinyl","Coral Gloss","Lagoon Teal"};break;
                case "firb": labels = new[]{"Cranberry Jelly","Sugarplum Pearl","Tinsel Teal","Peach Mochi","Amethyst Flock","Pistachio Plush","Classic Evergreen","Blue Spruce Gloss","Vanilla Fir"};break;
                case "coco": labels = new[]{"Caramel Jelly","Lavender Cream","Peppermint Confetti","Butterscotch Mochi","Berry Truffle Flock","Strawberry Plush","Classic Cocoa","Blueberry Glaze","Pistachio Cream"};break;
                case "snowy": labels = new[]{"Glacier Jelly","Lilac Pearl","Berry Snow Confetti","Peach Snow Mochi","Twilight Flock","Mint Cloud Plush","Classic Snow","Winter Blue Gloss","Lemon Snow"};break;
                case "ginger": labels = new[]{"Honey Jelly","Lavender Icing","Cherry Confetti","Mint Cookie Mochi","Sugarplum Flock","Strawberry Plush","Classic Ginger","Blue Icing Gloss","Vanilla Biscuit"};break;
                case "reiny": labels = new[]{"Cranberry Jelly","Rose Pearl","Holly Confetti","Peach Mochi","Plum Flock","Frosted Mint Plush","Classic Reiny","Blue Ribbon Gloss","Vanilla Antler"};break;
                case "guin": labels = new[]{"Ice Lagoon Jelly","Lilac Pearl","Coral Confetti","Lemon Mochi","Raspberry Flock","Mint Plush","Classic Guin","Violet Gloss","Rose Snow"};break;
                default: labels = new[]{"Caramel Jelly","Vanilla Pearl","Berry Confetti","Custard Mochi","Holly Flock","Lavender Plush","Classic Bundy","Cherry Glaze","Blue Sugar"};break;
            }
            return labels[index];
        }
        public static string SecretLabel(string character) => character == "pip" ? "Cosmic Core" : character == "fox" ? "Sunfire Spirit" : character == "axolotl" ? "Biolume Bloom" : character == "firb" ? "Aurora Evergreen" : character == "coco" ? "Midnight Ganache" : character == "snowy" ? "Polar Opal" : character == "ginger" ? "Stained Sugar" : character == "reiny" ? "Solstice Spirit" : character == "guin" ? "Polar Night" : "Sugar Nebula";
        public static CatalogColor SecretColor(string character) => new CatalogColor(character == "fox" || character == "reiny" ? 0xffa13bu : character == "coco" ? 0xffcb65u : character == "ginger" || character == "bundy" ? 0xf880cfu : character == "axolotl" ? 0x6bffd2u : 0x75dbffu);
        public static int SecretEffect(string character) => character == "pip" || character == "guin" || character == "bundy" ? 1 : character == "fox" || character == "firb" || character == "reiny" ? 2 : character == "axolotl" ? 3 : character == "coco" ? 4 : character == "snowy" ? 5 : 6;
        public static CatalogColor Swatch(string id, string character, string finish)
        {
            if (id == SecretId(character)) return SecretColor(character);
            if (id.EndsWith("." + Signature, StringComparison.Ordinal)) return Pigment(character, 8);
            if (id.EndsWith("." + Diamond, StringComparison.Ordinal)) return new CatalogColor(0xb7e6ff);
            if (id.EndsWith("." + Rose, StringComparison.Ordinal)) return new CatalogColor(0xdb94a0);
            if (id.EndsWith("." + Leaf, StringComparison.Ordinal)) return new CatalogColor(0xf4cd65);
            if (finish == "coated-metallic") return new CatalogColor(0xeab84c);
            if (finish == "glow-jelly") return SecretColor(character);
            return Pigment(character, CommonIndex(finish));
        }
    }
}
