using Pockle.Core;
using UnityEngine;
namespace Pockle.Runtime
{
    /// <summary>Version-one appearance recipe. Personality never participates in color or surface resolution.</summary>
    public static class FusionMaterial
    {
        private static readonly string[] Floats = { "_Gloss", "_Opacity", "_Metallic", "_Pearl", "_Iridescence", "_Glitter", "_Crystal",
            "_Fuzz", "_Grain", "_SurfaceRelief", "_LoopPile", "_TintStrength", "_Roughness", "_ReflectionStrength", "_Transmission",
            "_SpeckEmission", "_GlitterStrength", "_PearlSheen", "_Glow", "_TemperClassic" };
        public static Material Create(FusedToy record, Texture paint, Cubemap studio)
        {
            var material = Parent(record.First, paint, studio);
            Apply(material, record);
            return material;
        }
        private static Material Parent(string id, Texture paint, Cubemap studio)
        {
            ToyCatalog.TryGetCollectible(id, out var toy);
            if (toy.CharacterId == "fox") return FoxFinishMaterial.Create(id, paint, studio);
            if (toy.CharacterId == "axolotl") return AxolotlFinishMaterial.Create(id, paint, studio);
            if (HolidayCatalog.IsCharacter(toy.CharacterId)) return HolidayFinishMaterial.Create(id, paint, studio);
            var result = new Material(Shader.Find("Pockle/Textured Peach Jelly")) { mainTexture = paint };
            result.SetTexture("_StudioCube", studio); PipFinishMaterial.Apply(result, id); return result;
        }
        public static void Apply(Material material, FusedToy record)
        {
            var a = Parent(record.First, material.mainTexture, material.GetTexture("_StudioCube") as Cubemap);
            var b = Parent(record.Second, material.mainTexture, material.GetTexture("_StudioCube") as Cubemap);
            try
            {
                foreach (string property in Floats)
                    if (material.HasProperty(property) && a.HasProperty(property) && b.HasProperty(property))
                        material.SetFloat(property, (a.GetFloat(property) + b.GetFloat(property)) * .5f);
                if (material.HasProperty("_BodyColor")) material.SetColor("_BodyColor", Color.Lerp(a.GetColor("_BodyColor"), b.GetColor("_BodyColor"), .5f));
                if (material.HasProperty("_FusionMode"))
                {
                    string study = Study(record);
                    if (study != null) PipFinishMaterial.Apply(material, study);
                    else
                    {
                        material.SetFloat("_FusionMode", 1); material.SetFloat("_TintStrength", 1);
                        ToyCatalog.TryGetCollectible(record.First, out var first); ToyCatalog.TryGetCollectible(record.Second, out var second);
                        material.SetColor("_FusionColorA", ColorOf(first.SwatchColor)); material.SetColor("_FusionColorB", ColorOf(second.SwatchColor));
                    }
                }
                float opacity = material.HasProperty("_Opacity") ? material.GetFloat("_Opacity") : 1;
                material.renderQueue = opacity < .995f ? 3000 : 2001;
                if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", opacity < .995f ? 0 : 1);
                material.name = record.Name + " · fused recipe v1";
            }
            finally { Object.Destroy(a); Object.Destroy(b); }
        }
        public static string AppearanceId(string id) => FusionCatalog.TryGet(id, out var fused) ? Study(fused) ?? id : id;
        private static Color ColorOf(CatalogColor c) => new Color(c.Red, c.Green, c.Blue);
        private static string Study(FusedToy r)
        {
            bool Pair(string x, string y) => (r.First == x && r.Second == y) || (r.First == y && r.Second == x);
            if (Pair(ToyCatalog.PeachId, ToyCatalog.MoonId)) return PipFusionStudies.DuskId;
            if (Pair(ToyCatalog.PeachId, ToyCatalog.MintId)) return PipFusionStudies.SorbetId;
            if (Pair(ToyCatalog.GoldId, ToyCatalog.MintId)) return PipFusionStudies.HoneydewId;
            if (Pair(ToyCatalog.MoonId, ToyCatalog.GoldId)) return PipFusionStudies.EclipseId;
            return null;
        }
    }
}
