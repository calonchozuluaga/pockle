using Pockle.Core;
using UnityEngine;

namespace Pockle.Runtime
{
    /// <summary>Where a toy is shown on its stage (docs/VISUAL_SYSTEM.md, "Toy stage"). Home is the collection's own world.</summary>
    public enum StageWorld { Home, Studio, Garden, Midnight }

    /// <summary>The stage's lighting mood.</summary>
    public enum StageLight { Day, Golden, Night }

    /// <summary>
    /// Names and fallback colours for the stage tray. The 3D stage owns the real scenery and lights; until it
    /// handles <see cref="StageWorld"/> and <see cref="StageLight"/>, the HUD tints the stage backdrop with these colours.
    /// </summary>
    public static class StageLook
    {
        public const string WorldKey = "pockle.stage.world.v1";
        public const string LightKey = "pockle.stage.light.v1";

        public static readonly StageWorld[] Worlds = { StageWorld.Home, StageWorld.Studio, StageWorld.Garden, StageWorld.Midnight };
        public static readonly StageLight[] Lights = { StageLight.Day, StageLight.Golden, StageLight.Night };

        /// <summary>The collection's own world name. Placeholder names until each collection's world is approved.</summary>
        public static string HomeWorldName(string characterId)
        {
            switch (characterId)
            {
                case "fox": return "Fox hollow";
                case "pip": return "Pip patch";
                case "axolotl": return "Axolotl pond";
                default: return "Home";
            }
        }

        public static string Name(StageWorld world, string characterId)
        {
            switch (world)
            {
                case StageWorld.Home: return HomeWorldName(characterId);
                case StageWorld.Studio: return "Studio";
                case StageWorld.Garden: return "Jelly Garden";
                default: return "Midnight";
            }
        }

        public static string Name(StageLight light)
        {
            switch (light)
            {
                case StageLight.Golden: return "Golden hour";
                case StageLight.Night: return "Night lamp";
                default: return "Daylight";
            }
        }

        public static string Note(StageLight light, bool glows)
        {
            switch (light)
            {
                case StageLight.Golden: return "Warm and low, with long shadows.";
                case StageLight.Night: return glows ? "This finish glows in the dark." : "One warm lamp on your toy.";
                default: return "Soft, even light.";
            }
        }

        /// <summary>The swatch shown for a world in the tray.</summary>
        public static Color Swatch(StageWorld world, string characterId, Color toyField)
        {
            switch (world)
            {
                case StageWorld.Home: return HomeColour(characterId, toyField);
                case StageWorld.Studio: return toyField;
                case StageWorld.Garden: return PockleTheme.Jelly;
                default: return PockleTheme.Midnight;
            }
        }

        /// <summary>The flat backdrop colour used until the stage draws the world itself.</summary>
        public static Color Backdrop(StageWorld world, StageLight light, string characterId, Color toyField)
        {
            Color colour = Swatch(world, characterId, toyField);
            switch (light)
            {
                case StageLight.Golden: return new Color(colour.r, colour.g * .93f, colour.b * .82f, 1f);
                case StageLight.Night: return Color.Lerp(colour, new Color(.15f, .13f, .34f), .55f);
                default: return colour;
            }
        }

        /// <summary>True when light text reads better than plum over the stage.</summary>
        public static bool IsDark(StageWorld world, StageLight light) => world == StageWorld.Midnight || light == StageLight.Night;

        private static Color HomeColour(string characterId, Color toyField)
        {
            switch (characterId)
            {
                case "fox": return new Color(1f, .79f, .64f);
                case "axolotl": return new Color(.80f, .90f, .98f);
                default: return Color.Lerp(toyField, new Color(.95f, .85f, .75f), .35f);
            }
        }
    }
}
