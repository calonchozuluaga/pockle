using Pockle.Core;
using UnityEngine;
namespace Pockle.Runtime
{
    /// <summary>Patterned appearance for both existing and new fusion receipts. Personality never participates in color or surface resolution.</summary>
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
            if (toy.CharacterId == "wally") return WallyFinishMaterial.Create(id, paint, studio);
            if (toy.CharacterId == "fox") return FoxFinishMaterial.Create(id, paint, studio);
            if (toy.CharacterId == "axolotl") return AxolotlFinishMaterial.Create(id, paint, studio);
            if (HolidayCatalog.IsCharacter(toy.CharacterId)) return HolidayFinishMaterial.Create(id, paint, studio);
            var result = new Material(Shader.Find("Pockle/Textured Peach Jelly")) { mainTexture = paint };
            result.SetTexture("_StudioCube", studio); PipFinishMaterial.Apply(result, id); return result;
        }
        public static void Apply(Material material, FusedToy record)
        {
            bool reverse = string.CompareOrdinal(record.First, record.Second) > 0;
            var a = Parent(reverse ? record.Second : record.First, material.mainTexture, material.GetTexture("_StudioCube") as Cubemap);
            var b = Parent(reverse ? record.First : record.Second, material.mainTexture, material.GetTexture("_StudioCube") as Cubemap);
            try
            {
                foreach (string property in Floats)
                    if (material.HasProperty(property) && a.HasProperty(property) && b.HasProperty(property))
                        material.SetFloat(property, (a.GetFloat(property) + b.GetFloat(property)) * .5f);
                if (material.HasProperty("_BodyColor")) material.SetColor("_BodyColor", a.GetColor("_BodyColor"));
                bool approvedStudy = Study(record) != null;
                material.SetFloat("_FusionPattern", approvedStudy ? 0 : FusionPattern.Kind(record.First, record.Second));
                string pigment = material.HasProperty("_BodyColor") ? "_BodyColor" : "_FinishTint";
                Color colorA = a.GetColor(pigment), colorB = b.GetColor(pigment);
                // Same/near-identical pigments still produce a visible tonal pattern.
                if (Vector3.Distance(new Vector3(colorA.r,colorA.g,colorA.b), new Vector3(colorB.r,colorB.g,colorB.b)) < .18f)
                    colorB = Color.Lerp(colorB, colorB.maxColorComponent < .25f ? Color.white : Color.black, .38f);
                material.SetColor("_FusionPigmentA", colorA); material.SetColor("_FusionPigmentB", colorB);
                string shine = material.HasProperty("_Gloss") ? "_Gloss" : "_Roughness";
                Vector4 Surface(Material m) => new Vector4(m.GetFloat(shine), m.GetFloat("_Metallic"), m.GetFloat("_Fuzz"), m.GetFloat("_Opacity"));
                material.SetVector("_FusionSurfaceA", Surface(a)); material.SetVector("_FusionSurfaceB", Surface(b));
                float Read(Material m, string p) => m.HasProperty(p) ? m.GetFloat(p) : 0;
                bool pip = material.HasProperty("_FinishTint");
                Vector4 Detail(Material m) => pip
                    ? new Vector4(Read(m,"_ReflectionStrength"),Read(m,"_PearlSheen"),Read(m,"_Iridescence"),Read(m,"_GlitterStrength"))
                    : new Vector4(Read(m,"_Grain"),Read(m,"_Pearl"),Read(m,"_Iridescence"),Read(m,"_Glitter"));
                Vector4 Accent(Material m) => pip
                    ? new Vector4(Read(m,"_Transmission"),Read(m,"_SpeckEmission"),Read(m,"_Glow"),0)
                    : new Vector4(Read(m,"_SurfaceRelief"),Read(m,"_LoopPile"),Read(m,"_Crystal"),0);
                material.SetVector("_FusionDetailA",Detail(a)); material.SetVector("_FusionDetailB",Detail(b));
                material.SetVector("_FusionAccentA",Accent(a)); material.SetVector("_FusionAccentB",Accent(b));
                if (!approvedStudy) {
                    if (material.HasProperty("_TintStrength")) material.SetFloat("_TintStrength", 1);
                    if (material.HasProperty("_TemperClassic")) material.SetFloat("_TemperClassic", 0);
                }
                if (material.HasProperty("_FusionMode"))
                {
                    string study = Study(record);
                    if (study != null) PipFinishMaterial.Apply(material, study);
                    else
                    {
                        material.SetFloat("_FusionMode", 0); material.SetFloat("_TintStrength", 1);
                        ToyCatalog.TryGetCollectible(record.First, out var first); ToyCatalog.TryGetCollectible(record.Second, out var second);
                        material.SetColor("_FusionColorA", ColorOf(first.SwatchColor)); material.SetColor("_FusionColorB", ColorOf(second.SwatchColor));
                    }
                }
                float opacity = material.HasProperty("_Opacity") ? material.GetFloat("_Opacity") : 1;
                material.renderQueue = opacity < .995f ? 3000 : 2001;
                if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", opacity < .995f ? 0 : 1);
                material.name = record.Name + " · patterned appearance v2";
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
