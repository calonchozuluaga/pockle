using Pockle.Core;
using UnityEngine;
namespace Pockle.Runtime
{
    /// <summary>Recipes share the approved Pip mesh and paint; reset every finish property when switching.</summary>
    public static class PipFinishMaterial
    {
        private static void Opaque(Material material,float roughness,float reflection)
        {
            material.SetFloat("_Opacity",1); material.SetFloat("_ZWrite",1); material.SetFloat("_Transmission",.04f);
            material.SetFloat("_Roughness",roughness); material.SetFloat("_ReflectionStrength",reflection); material.renderQueue=2001;
        }
        public static void Apply(Material material, string id)
        {
            material.SetColor("_FinishTint", Color.white);
            material.SetFloat("_TintStrength", id == ToyCatalog.PeachId ? 0 : 1);
            material.SetFloat("_Roughness", .16f); material.SetFloat("_Opacity", .62f);
            material.SetFloat("_ReflectionStrength", .80f); material.SetFloat("_Transmission", .90f);
            material.SetFloat("_SpeckEmission", id == ToyCatalog.PeachId ? .20f : 0);
            foreach (string property in new[] { "_GlitterStrength", "_PearlSheen", "_Fuzz", "_Metallic", "_Iridescence", "_Glow", "_ZWrite" }) material.SetFloat(property,0);
            material.renderQueue = 3000;
            switch (id)
            {
                case "pip.velvet-flock":
                    material.SetColor("_FinishTint",new Color(.75f,.63f,.9f)); material.SetFloat("_Fuzz",1);
                    Opaque(material,.40f,.10f); break;
                case "pip.boucle-plush":
                    material.SetColor("_FinishTint",new Color(.96f,.70f,.77f)); material.SetFloat("_Fuzz",.7f);
                    Opaque(material,.38f,.14f); break;
                case "pip.matte-vinyl":
                    material.SetColor("_FinishTint",new Color(.80f,.90f,.98f)); Opaque(material,.36f,.18f); break;
                case "pip.gloss-vinyl":
                    material.SetColor("_FinishTint",new Color(.98f,.49f,.61f)); Opaque(material,.10f,.9f); break;
                case "pip.coated-metallic":
                    material.SetColor("_FinishTint",new Color(.86f,.62f,.23f)); material.SetFloat("_Metallic",1);
                    Opaque(material,.15f,.95f); break;
                case "pip.glow-jelly":
                    material.SetColor("_FinishTint",new Color(.36f,.78f,.82f)); material.SetFloat("_Glow",.7f);
                    material.SetFloat("_Iridescence",.45f); material.SetFloat("_Opacity",.48f); break;
                case ToyCatalog.MoonId:
                    material.SetColor("_FinishTint", new Color(.45f,.72f,.96f));
                    material.SetFloat("_Opacity",.54f); material.SetFloat("_PearlSheen",.72f); break;
                case ToyCatalog.GoldId:
                    material.SetColor("_FinishTint", new Color(1,.69f,.18f));
                    material.SetFloat("_Opacity",.58f); material.SetFloat("_GlitterStrength",.9f); break;
                case ToyCatalog.MintId:
                    material.SetColor("_FinishTint", new Color(.55f,.85f,.66f));
                    material.SetFloat("_Roughness",.38f); material.SetFloat("_ReflectionStrength",.22f);
                    material.SetFloat("_Transmission",.04f); material.SetFloat("_Opacity",1); material.SetFloat("_ZWrite",1);
                    material.renderQueue=2001; break;
            }
            if (ToyCatalog.TryGetCollectible(id,out var item)) material.name=item.DisplayName + " · approved Pip shell";
        }
    }
}
