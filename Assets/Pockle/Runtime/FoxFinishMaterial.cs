using Pockle.Core;
using UnityEngine;

namespace Pockle.Runtime
{
    /// <summary>One uploaded mesh/paint, local finish recipes. No texture copies or runtime downloads.</summary>
    public static class FoxFinishMaterial
    {
        public static Material Create(string id, Texture paint, Cubemap studio)
        {
            if (!ToyCatalog.IsFoxCollectible(id)) throw new System.ArgumentException("Expected a Fox finish.", nameof(id));
            if (id == ToyCatalog.FoxId)
            {
                var template = Resources.Load<Material>("Characters/Fox/FoxVinyl");
                var classic = template != null ? new Material(template) : new Material(Shader.Find("Standard"));
                classic.name = "Fox · Classic"; classic.mainTexture = paint;
                return classic;
            }
            var shader = Resources.Load<Shader>("Characters/Fox/FoxFinish");
            if (shader == null || !shader.isSupported) throw new System.InvalidOperationException("Fox finish shader unavailable.");
            var material = new Material(shader) { name = ToyCatalog.TryGetCollectible(id, out var item) ? item.DisplayName : id, mainTexture = paint };
            material.SetTexture("_StudioCube", studio);
            float opacity = 1, gloss = .35f;
            Color tint = new Color(1, .57f, .25f);
            switch (item.FinishId)
            {
                case "matte-vinyl": tint = new Color(1, .57f, .25f); gloss = .12f; break;
                case "velvet-flock": tint = new Color(.97f, .73f, .58f); gloss = .04f; material.SetFloat("_Fuzz", 1); break;
                case "mochi-foam": tint = new Color(.98f, .68f, .65f); gloss = .07f; material.SetFloat("_Grain", .45f); break;
                case "coated-metallic": tint = new Color(.82f, .38f, .17f); gloss = .78f; material.SetFloat("_Metallic", 1); break;
                case "pearl-jelly": tint = new Color(.97f, .77f, .72f); gloss = .66f; material.SetFloat("_Pearl", 1); break;
                case "clear-jelly": tint = new Color(1, .49f, .08f); gloss = .87f; opacity = .40f; material.SetFloat("_Glitter", .7f); break;
                case "confetti-gel": tint = new Color(.96f, .42f, .63f); gloss = .80f; opacity = .52f; material.SetFloat("_Crystal", 1); break;
                case "boucle-plush": tint = new Color(.12f, .075f, .25f); gloss = .38f; break;
                case "glow-jelly": tint = new Color(.48f, .32f, .72f); gloss = .86f; opacity = .43f; material.SetFloat("_Iridescence", 1); material.SetFloat("_Glitter", 1); break;
            }
            material.SetColor("_BodyColor", tint); material.SetFloat("_Gloss", gloss); material.SetFloat("_Opacity", opacity);
            material.renderQueue = opacity < 1 ? 3000 : 2000;
            return material;
        }
    }
}
