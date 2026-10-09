using UnityEngine;

namespace Pockle.Runtime
{
    /// <summary>
    /// UX-002 design tokens: colour, type, spacing, and shared UI resources.
    /// See docs/VISUAL_SYSTEM.md. Screens should take values from here instead of hard-coding them.
    /// </summary>
    public static class PockleTheme
    {
        // Colour. Warm cream surfaces, plum ink, and the peach of the Jelly Garden box.
        public static readonly Color Ink = Hex(0x4A364A);
        public static readonly Color MutedInk = Hex(0x7A666C);
        public static readonly Color Paper = Hex(0xFFFDF8);
        public static readonly Color Quiet = Hex(0xF3ECE4);
        public static readonly Color Peach = Hex(0xF7B092);
        public static readonly Color PeachDeep = Hex(0xE3876B);
        public static readonly Color TilePeach = Hex(0xFCDCC5);
        public static readonly Color TileLavender = Hex(0xDCE2FA);
        public static readonly Color TileMint = Hex(0xDBF0E2);
        public static readonly Color ShelfFace = Hex(0xFAF5EF);
        public static readonly Color ShelfPlank = Hex(0xE6D7C9);
        public static readonly Color ShelfEdge = Hex(0xD6C4B4);
        public static readonly Color Backdrop = new Color(.23f, .17f, .22f, .38f);
        /// <summary>Soft plum drop shadow under cards.</summary>
        public static readonly Color CardShadow = new Color(.29f, .21f, .29f, .10f);
        /// <summary>The "lip" under neutral buttons; primary buttons use <see cref="PeachDeep"/>.</summary>
        public static readonly Color ButtonLip = new Color(.29f, .21f, .29f, .13f);

        // Type scale, in reference pixels (390 x 844 portrait).
        public const int TitleSize = 23;
        public const int HeadingSize = 19;
        public const int ButtonSize = 15;
        public const int BodySize = 14;
        public const int CaptionSize = 12;
        public const int EyebrowSize = 11;
        /// <summary>Text never best-fits below this size.</summary>
        public const int MinimumTextSize = 10;

        // Spacing and shape.
        public const float Gutter = 16f;
        public const float TouchTarget = 48f;
        public const float CornerRadius = 16f;
        public static readonly Vector2 CardShadowOffset = new Vector2(0f, -4f);
        public static readonly Vector2 ButtonLipOffset = new Vector2(0f, -4f);

        // Motion.
        public const float PressScale = .93f;
        public const float CalmPressScale = .97f;

        private static Font display, body, fallback;
        private static bool loaded;

        /// <summary>Rounded display face (Fredoka SemiBold) for titles, eyebrows, and buttons.</summary>
        public static Font DisplayFont { get { Load(); return display != null ? display : fallback; } }
        /// <summary>Readable body face (Nunito SemiBold) for descriptions and status text.</summary>
        public static Font BodyFont { get { Load(); return body != null ? body : fallback; } }
        /// <summary>True when the bundled fonts loaded, so weights come from the font rather than synthesized bold.</summary>
        public static bool HasBrandFonts { get { Load(); return display != null && body != null; } }

        public static Texture2D Wordmark => Resources.Load<Texture2D>("UI/PockleWordmark");
        public static Texture2D Icon(string name) => Resources.Load<Texture2D>("UI/Icons/" + name);

        private static void Load()
        {
            if (loaded) return;
            loaded = true;
            display = Resources.Load<Font>("UI/Fonts/Fredoka-SemiBold");
            body = Resources.Load<Font>("UI/Fonts/Nunito-SemiBold");
#if UNITY_2022_2_OR_NEWER
            fallback = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
            fallback = Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
            if (display == null || body == null)
                Debug.LogWarning("Pockle UI fonts are missing from Resources/UI/Fonts; using the built-in font.");
        }

        private static Color Hex(int rgb)
        {
            return new Color(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f, 1f);
        }
    }
}
