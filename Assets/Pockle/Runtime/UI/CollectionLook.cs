using Pockle.Core;
using UnityEngine;

namespace Pockle.Runtime
{
    /// <summary>
    /// How a collection presents itself (canvas row "Collections, box first", owner-approved October 14): its box as the
    /// main art, its own world painting as the banner, and an accent and ink colour carried through its page. Art is
    /// looked up by convention so new renders need no code: a box at Resources/UI/Boxes/Collection-{id}, a banner from the
    /// collection's world. Collections without art fall back to the character portrait on the collection's colour.
    /// </summary>
    public static class CollectionLook
    {
        /// <summary>The world painting (Resources/Worlds) that is this collection's banner, or null.</summary>
        public static string BannerWorld(string collectionId)
        {
            switch (collectionId)
            {
                case "pip": return "jelly-garden";
                case "fox": return "fox-hollow";
                case "axolotl": return "moonlit-pond";
                default: return null;
            }
        }

        /// <summary>The collection's 3:1 cover (catalog CoverArt), else its home world painting, else the legacy banner world.</summary>
        public static Texture2D Banner(CollectionDefinition collection)
        {
            if (!string.IsNullOrEmpty(collection.CoverArt))
            {
                var cover = Resources.Load<Texture2D>(collection.CoverArt);
                if (cover != null) return cover;
            }
            if (collection.Worlds.Count > 0)
            {
                var home = Resources.Load<PaintedWorldAsset>(collection.Worlds[0].EnvironmentArt);
                if (home != null && home.Background != null) return home.Background;
            }
            return Banner(collection.Id);
        }

        public static Texture2D Banner(string collectionId)
        {
            string world = BannerWorld(collectionId);
            if (world == null) return null;
            var asset = Resources.Load<PaintedWorldAsset>("Worlds/" + world);
            return asset != null ? asset.Background : null;
        }

        /// <summary>The closed box picture (catalog BoxArt, else Resources/UI/Boxes/Collection-{id}) and its width/height ratio.</summary>
        public static Texture2D Box(CollectionDefinition collection, out float aspect)
        {
            if (!string.IsNullOrEmpty(collection.BoxArt))
            {
                var art = Resources.Load<Texture2D>(collection.BoxArt);
                if (art != null) { aspect = (float)art.width / Mathf.Max(1, art.height); return art; }
            }
            return Box(collection.Id, out aspect);
        }

        /// <summary>The closed box picture and its width/height ratio, or null while the render doesn't exist yet.</summary>
        public static Texture2D Box(string collectionId, out float aspect)
        {
            var box = Resources.Load<Texture2D>("UI/Boxes/Collection-" + collectionId);
            if (box != null) { aspect = (float)box.width / Mathf.Max(1, box.height); return box; }
            if (collectionId == "pip")
            {
                aspect = 380f / 342f;
                return Resources.Load<Texture2D>("UI/Boxes/JellyGarden");
            }
            aspect = 1f;
            return null;
        }

        /// <summary>The collection's bright colour: progress card, filled bars, tile tint (catalog AccentColor).</summary>
        public static Color Accent(CollectionDefinition collection)
        {
            var c = collection.AccentColor; return new Color(c.Red, c.Green, c.Blue, 1f);
        }

        /// <summary>The collection's deep colour: headings and the main button on its page (catalog InkColor).</summary>
        public static Color Ink(CollectionDefinition collection)
        {
            var c = collection.InkColor; return new Color(c.Red, c.Green, c.Blue, 1f);
        }

        /// <summary>A uvRect that fills a box of <paramref name="aspect"/> (width/height) without stretching, like CSS object-fit: cover.</summary>
        public static Rect Cover(Texture texture, float aspect, float focusX = .5f, float focusY = .5f)
        {
            if (texture == null || aspect <= 0) return new Rect(0, 0, 1, 1);
            float source = (float)texture.width / Mathf.Max(1, texture.height);
            if (aspect < source) { float w = aspect / source; return new Rect((1 - w) * focusX, 0, w, 1); }
            float h = source / aspect; return new Rect(0, (1 - h) * focusY, 1, h);
        }
    }
}
