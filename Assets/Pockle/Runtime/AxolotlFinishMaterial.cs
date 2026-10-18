using Pockle.Core;
using UnityEngine;
namespace Pockle.Runtime
{
    /// <summary>Ten local recipes share the approved mesh, original face and gill paint.</summary>
    public static class AxolotlFinishMaterial
    {
        public static Material Create(string id, Texture paint, Cubemap studio)
        {
            if (!ToyCatalog.TryGetCollectible(id, out var toy) || toy.CharacterId != "axolotl")
                throw new System.ArgumentException("Expected an Axolotl finish.", nameof(id));
            var template = Resources.Load<Material>("Characters/Axolotl/AxolotlPlush");
            if (template == null) throw new System.InvalidOperationException("Axolotl material missing.");
            var m = new Material(template) { name = toy.DisplayName, mainTexture = paint };
            m.SetTexture("_StudioCube", studio);
            m.SetColor("_BodyColor", new Color(toy.SwatchColor.Red, toy.SwatchColor.Green, toy.SwatchColor.Blue));
            m.SetFloat("_TintStrength", id == ToyCatalog.AxolotlId ? 0 : 1);
            float gloss = .12f, opacity = 1;
            switch (toy.FinishId)
            {
                case "clear-jelly": gloss = .85f; opacity = .52f; break;
                case "pearl-jelly": gloss = .67f; m.SetFloat("_Pearl", .85f); break;
                case "confetti-gel": gloss = .80f; opacity = .62f; m.SetFloat("_Glitter", 1); break;
                case "mochi-foam": m.SetFloat("_Grain", .6f); break;
                case "velvet-flock": gloss = .04f; m.SetFloat("_Fuzz", 1); break;
                case "boucle-plush": gloss = .03f; m.SetFloat("_Fuzz", .65f); break;
                case "gloss-vinyl": gloss = .9f; break;
                case "coated-metallic": gloss = .82f; m.SetFloat("_Metallic", 1); break;
                case "glow-jelly": gloss = .87f; opacity = .55f; m.SetFloat("_Iridescence", .65f); m.SetFloat("_Glitter", .5f); break;
            }
            m.SetFloat("_Gloss", gloss); m.SetFloat("_Opacity", opacity);
            m.renderQueue = opacity < 1 ? 3000 : 2000;
            return m;
        }
    }
}
