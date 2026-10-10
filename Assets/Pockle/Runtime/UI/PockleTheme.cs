using UnityEngine;

namespace Pockle.Runtime
{
    /// <summary>
    /// v2 design tokens (UX-032): colour, type, shape, and shared UI resources, matching the owner-approved
    /// "Pockle screens v2" canvas. See docs/VISUAL_SYSTEM.md. Screens take values from here, never hard-coded.
    /// </summary>
    public static class PockleTheme
    {
        // Ground and ink.
        public static readonly Color Milk = Hex(0xFBF8F5);
        public static readonly Color Plum = Hex(0x2A1430);
        public static readonly Color PlumSoft = Hex(0x6E5A72);
        public static readonly Color OnPlum = Hex(0xFBF8F5);
        public static readonly Color OnPlumMuted = Hex(0xE8DCE6);
        /// <summary>Neutral fill for rows, chips, inputs, and secondary buttons.</summary>
        public static readonly Color Fill = Hex(0xF1ECEF);
        public static readonly Color FillDeep = Hex(0xE2D8DE);
        public static readonly Color Mystery = Hex(0xEFE9EC);

        // Brand and toy colour fields.
        public static readonly Color Jelly = Hex(0xFFC6D6);
        public static readonly Color JellyInk = Hex(0x7A2A4E);
        public static readonly Color FieldPeach = Hex(0xFFD8C4);
        public static readonly Color FieldMoon = Hex(0xD8DDFF);
        public static readonly Color FieldGold = Hex(0xFFE3A3);
        public static readonly Color FieldMint = Hex(0xCDEFDB);
        public static readonly Color Midnight = Hex(0x2E2A5C);
        public static readonly Color Confetti = Hex(0xF1E6FF);
        public static readonly Color Heart = Hex(0xC2416E);
        public static readonly Color Success = Hex(0x2E7A50);
        public static readonly Color Backdrop = new Color(.16f, .08f, .19f, .45f);

        // Type scale, in reference pixels (390 x 844 portrait).
        public const int TitleSize = 32;
        public const int HeadingSize = 21;
        public const int ButtonSize = 17;
        public const int BodySize = 16;
        public const int CaptionSize = 14;
        public const int SmallSize = 13;
        /// <summary>Text never best-fits below this size.</summary>
        public const int MinimumTextSize = 11;

        // Shape and spacing.
        public const float Gutter = 20f;
        public const float TouchTarget = 44f;
        public const float CardRadius = 28f;
        public const float TileRadius = 22f;
        public const float RowRadius = 20f;

        // Motion.
        public const float PressScale = .95f;
        public const float CalmPressScale = .98f;

        /// <summary>The colour field a collectible sits on, by its stable catalog ID.</summary>
        public static Color FieldFor(string collectibleId)
        {
            switch (collectibleId)
            {
                case "pip.moon-pearl": return FieldMoon;
                case "pip.gold-confetti": return FieldGold;
                case "pip.mint-mochi": return FieldMint;
                case "pip.peach-jelly": return FieldPeach;
                default: return Mystery;
            }
        }

        /// <summary>The colour field for a collection (series), by catalog collection ID.</summary>
        public static Color CollectionField(string collectionId, out Color ink, out Color dot)
        {
            switch (collectionId)
            {
                case "midnight-glow": ink = OnPlum; dot = Hex(0x5B53B8); return Midnight;
                case "gold-confetti": ink = Plum; dot = Hex(0xE0A92E); return Confetti;
                case "jelly-garden": ink = Plum; dot = Hex(0xFF8FB0); return Jelly;
                default: ink = Plum; dot = PlumSoft; return Fill;
            }
        }

        private static Font display, label, body, fallback;
        private static bool loaded;

        /// <summary>Bricolage Grotesque Bold, for screen titles and large numbers.</summary>
        public static Font DisplayFont { get { Load(); return display != null ? display : fallback; } }
        /// <summary>Bricolage Grotesque SemiBold, for buttons, names, and section headings.</summary>
        public static Font LabelFont { get { Load(); return label != null ? label : fallback; } }
        /// <summary>Bricolage Grotesque Regular, for descriptions and status text.</summary>
        public static Font BodyFont { get { Load(); return body != null ? body : fallback; } }
        /// <summary>True when the bundled fonts loaded, so weights come from the files rather than synthesized bold.</summary>
        public static bool HasBrandFonts { get { Load(); return display != null && label != null && body != null; } }

        public static Texture2D Wordmark => Resources.Load<Texture2D>("UI/PockleWordmark");
        public static Texture2D Icon(string name) => Resources.Load<Texture2D>("UI/Icons/" + name);

        private static void Load()
        {
            if (loaded) return;
            loaded = true;
            display = Resources.Load<Font>("UI/Fonts/Bricolage-Display");
            label = Resources.Load<Font>("UI/Fonts/Bricolage-Label");
            body = Resources.Load<Font>("UI/Fonts/Bricolage-Body");
#if UNITY_2022_2_OR_NEWER
            fallback = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
            fallback = Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
            if (display == null || label == null || body == null)
                Debug.LogWarning("Pockle UI fonts are missing from Resources/UI/Fonts; using the built-in font.");
        }

        private static Color Hex(int rgb)
        {
            return new Color(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f, 1f);
        }
    }
}
