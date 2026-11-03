using System;
using Pockle.Core;
using UnityEngine;

namespace Pockle.Runtime
{
    /// <summary>
    /// Values keep their numbers. The October 21 ladder (owner-approved: Common, Rare gold, Epic diamond, Secret molten/cyan) uses
    /// Common/Rare/Epic/Secret; SuperRare/UltraRare remain for the current catalog's labels until the runtime retags toys.
    /// </summary>
    public enum RarityTier { Common, Rare, SuperRare, UltraRare, Fusion, Epic, Secret }

    /// <summary>
    /// The rarity colour index (owner request, October 17): rarity reads as a solid, coloured badge with an icon, so it never
    /// looks like the glassy personality chips beside it. Each step up the ladder gets a warmer colour and a richer icon:
    /// Rare = mint + star, Super rare = rose + sparkle, Ultra rare = gold + gem, Fusion = plum + two joined rings.
    /// Common toys show no badge. A secret keeps its tier colour and adds "Secret" to the words.
    /// </summary>
    public readonly struct RarityLook
    {
        public readonly RarityTier Tier;
        public readonly bool Secret;
        public readonly string Text;
        public readonly Color Fill, Ink, Ring;
        /// <summary>Icon in Resources/UI/Icons.</summary>
        public readonly string Icon;

        private RarityLook(RarityTier tier, bool secret, string text, Color fill, Color ink, Color ring, string icon)
        { Tier = tier; Secret = secret; Text = text; Fill = fill; Ink = ink; Ring = ring; Icon = icon; }

        public bool Shown => Tier != RarityTier.Common && Text.Length > 0;

        private static Color Hex(uint rgb, float a = 1) => new Color(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f, a);

        public static RarityLook For(RarityTier tier, bool secret, string words)
        {
            switch (tier)
            {
                // October 21 ladder: Rare is gold, Epic is diamond, Secret is molten (cracked obsidian, glowing veins).
                case RarityTier.Rare: return new RarityLook(tier, secret, words, Hex(0xFFDB7A), Hex(0x5C3A00), Hex(0xFFF1B8), "StickerStar");
                case RarityTier.Epic: return new RarityLook(tier, secret, words, Hex(0xD6F1FF), Hex(0x15486E), Hex(0x9FDCFF), "RarityGem");
                // Matches the approved Molten Abyss art (October 22): black obsidian with cyan-lit fissures.
                case RarityTier.Secret: return new RarityLook(tier, true, words, Hex(0x0E1418), Hex(0x7FF3FF), Hex(0x22D3F0), "StickerSparkle");
                case RarityTier.SuperRare: return new RarityLook(tier, secret, words, Hex(0xFFB8D9), Hex(0x7A1F4E), Hex(0xF27BB2), "StickerSparkle");
                case RarityTier.UltraRare: return new RarityLook(tier, secret, words, Hex(0xFFD66B), Hex(0x5C3A00), Hex(0xFFF1B8), "RarityGem");
                case RarityTier.Fusion: return new RarityLook(tier, secret, words, Hex(0x2A1430), Hex(0xF1E6FF), Hex(0xB99CDE), "RarityFusion");
                default: return new RarityLook(RarityTier.Common, false, "", Color.clear, Color.clear, Color.clear, "");
            }
        }

        /// <summary>
        /// The runtime's tier for a collectible ID (Codex: set this once tiers are in the catalog/server). When it returns a
        /// tier, that wins over the label parsing below. Null, or a null result, keeps today's label-based tiers.
        /// </summary>
        public static Func<string, RarityTier?> TierSource;

        /// <summary>Core's ladder tier as a UI tier. Codex's binding can be one line, e.g.
        /// <c>RarityLook.TierSource = id => id.StartsWith("wally.") ? RarityLook.FromTier(WallyCatalog.Tier(id)) : (RarityTier?)null;</c></summary>
        public static RarityTier FromTier(ToyTier tier) => tier == ToyTier.Rare ? RarityTier.Rare : tier == ToyTier.Epic ? RarityTier.Epic
            : tier == ToyTier.Secret ? RarityTier.Secret : RarityTier.Common;

        /// <summary>Words for a ladder tier: "Rare", "Epic", "Secret".</summary>
        public static string Words(RarityTier tier) => tier == RarityTier.Rare ? "Rare" : tier == RarityTier.Epic ? "Epic"
            : tier == RarityTier.Secret ? "Secret" : tier == RarityTier.SuperRare ? "Super rare" : tier == RarityTier.UltraRare ? "Ultra rare" : "";

        /// <summary>The colour a box reveal glows for this tier (Common gets a soft white).</summary>
        public Color RevealGlow => Tier == RarityTier.Common ? new Color(1, 1, 1, .7f) : Ring;

        /// <summary>Reads the tier from the runtime (TierSource), else the toy's label ("Starlight Glow · Ultra rare") or fusion.</summary>
        public static RarityLook Of(CollectibleDefinition toy)
        {
            if (toy == null) return For(RarityTier.Common, false, "");
            if (FusionCatalog.TryGet(toy.Id, out _)) return For(RarityTier.Fusion, false, "Fusion · One of a kind");
            var given = TierSource?.Invoke(toy.Id);
            if (given.HasValue) return For(given.Value, given.Value == RarityTier.Secret || toy.IsSecret, Words(given.Value));
            string name = toy.FinishDisplayName ?? "";
            int dot = name.IndexOf(" · ", StringComparison.Ordinal);
            string label = dot > 0 ? name.Substring(dot + 3) : "";
            var tier = TierOf(label);
            if (label.IndexOf("fusion", StringComparison.OrdinalIgnoreCase) >= 0) return For(RarityTier.Fusion, false, "Fusion study");
            if (toy.IsSecret && tier == RarityTier.Common) tier = RarityTier.UltraRare; // a secret is never shown as common
            string words = tier == RarityTier.Common ? "" : (label.Length > 0 ? Capital(label) : "Ultra rare") + (toy.IsSecret ? " · Secret" : "");
            return For(tier, toy.IsSecret, words);
        }

        public static RarityTier TierOf(string label)
        {
            string l = (label ?? "").ToLowerInvariant();
            if (l.Contains("ultra")) return RarityTier.UltraRare;
            if (l.Contains("super")) return RarityTier.SuperRare;
            if (l.Contains("rare")) return RarityTier.Rare;
            return RarityTier.Common;
        }

        private static string Capital(string s) => s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s.Substring(1);
    }
}
