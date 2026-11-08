using Pockle.Core;
using UnityEngine;
namespace Pockle.Runtime
{
    /// <summary>Palette and rarity layers shared across the approved meshes. Wally retains its approved pilot recipe.</summary>
    public static class RosterFinishMaterial
    {
        public static void Apply(Material m, CollectibleDefinition toy)
        {
            if (toy.CharacterId == "wally" || toy.CollectionId == PipFusionStudies.CollectionId) return;
            var swatch = RosterFinishDesign.Swatch(toy.Id, toy.CharacterId, toy.FinishId);
            var tint = new Color(swatch.Red, swatch.Green, swatch.Blue);
            bool pip = toy.CharacterId == "pip";
            m.SetColor(pip ? "_FinishTint" : "_BodyColor", tint);
            m.SetFloat("_RosterPalette", 1);
            m.SetFloat("_EditionEffect", 0);
            m.SetColor("_EditionColor", tint);
            // Preserve each uploaded Classic's paint; a new matte variety must still accept its new pigment.
            bool classic = toy.FinishId == "matte-vinyl" && !RosterFinishDesign.IsAddition(toy.Id);
            if (!pip && toy.CharacterId != "fox") m.SetFloat("_TintStrength", (classic && HolidayCatalog.IsCharacter(toy.CharacterId)) || toy.Id == ToyCatalog.AxolotlId ? 0 : 1);
            if (m.HasProperty("_TemperClassic") && toy.CharacterId == "fox") m.SetFloat("_TemperClassic", toy.Id == ToyCatalog.FoxId ? 1 : 0);
            if (!pip && HolidayCatalog.IsCharacter(toy.CharacterId))
            {
                if (toy.FinishId == "velvet-flock") { m.SetFloat("_Fuzz", 1); m.SetFloat("_SurfaceRelief", .28f); m.SetFloat("_Gloss", .015f); }
                if (toy.FinishId == "boucle-plush") { m.SetFloat("_LoopPile", 1); m.SetFloat("_Fuzz", .65f); m.SetFloat("_Gloss", .025f); }
            }
            if (toy.Tier == ToyTier.Rare)
            {
                m.SetFloat("_EditionEffect", toy.Id.EndsWith("." + RosterFinishDesign.Leaf) ? 2 : 1);
                SetOpaque(m, pip, .99f);
                m.SetFloat("_Metallic", 1);
            }
            else if (toy.Id.EndsWith("." + RosterFinishDesign.Diamond))
            {
                m.SetFloat("_EditionEffect", 3); SetOpaque(m, pip, .97f);
            }
            else if (toy.Tier == ToyTier.Secret)
            {
                m.SetFloat("_EditionEffect", 3 + RosterFinishDesign.SecretEffect(toy.CharacterId));
                SetOpaque(m, pip, .92f);
                m.SetFloat("_Metallic", 0);
            }
            else if (toy.Id.EndsWith("." + RosterFinishDesign.Signature)) SetOpaque(m, pip, .10f);
        }
        static void SetOpaque(Material m, bool pip, float gloss)
        {
            m.SetFloat("_Opacity", 1); m.renderQueue = pip ? 2001 : 2000;
            if (pip)
            {
                m.SetFloat("_Roughness", 1-gloss); m.SetFloat("_ReflectionStrength", .98f);
                m.SetFloat("_Transmission", .04f); m.SetFloat("_ZWrite", 1);
            }
            else m.SetFloat("_Gloss", gloss);
        }
    }
}
