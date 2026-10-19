using Pockle.Core;
using UnityEngine;
namespace Pockle.Runtime
{
    public static class WallyFinishMaterial
    {
        public static Material Create(string id, Texture paint, Cubemap studio)
        {
            if (!ToyCatalog.TryGetCollectible(id,out var toy) || toy.CharacterId != "wally")
                throw new System.ArgumentException("Expected a Wally finish.",nameof(id));
            var shader=Resources.Load<Shader>("Characters/Wally/WallyFinish");
            if(shader==null || !shader.isSupported) throw new System.InvalidOperationException("Wally finish shader unavailable.");
            var m=new Material(shader){name=toy.DisplayName,mainTexture=paint};
            m.SetTexture("_StudioCube",studio);
            var swatch=WallyCatalog.Swatch(toy.FinishId);
            m.SetColor("_BodyColor",new Color(swatch.Red,swatch.Green,swatch.Blue));
            float gloss=.12f,opacity=1;
            switch(toy.FinishId)
            {
                case "clear-jelly":gloss=.86f;opacity=.60f;break;
                case "pearl-jelly":gloss=.70f;m.SetFloat("_Pearl",1);break;
                case "confetti-gel":gloss=.85f;opacity=.68f;m.SetFloat("_Glitter",1.5f);break;
                case "mochi-foam":gloss=.035f;m.SetFloat("_Grain",.9f);m.SetFloat("_SurfaceRelief",.30f);break;
                case "velvet-flock":gloss=.015f;m.SetFloat("_Fuzz",1);m.SetFloat("_SurfaceRelief",.28f);break;
                case "boucle-plush":gloss=.025f;m.SetFloat("_Fuzz",.65f);m.SetFloat("_LoopPile",1);m.SetFloat("_SurfaceRelief",.14f);break;
                case "gloss-vinyl":gloss=.94f;break;
                case "coated-metallic":gloss=.97f;m.SetFloat("_Metallic",1);break;
                case "glow-jelly":gloss=.88f;opacity=.76f;m.SetFloat("_Iridescence",.7f);m.SetFloat("_Glitter",1);break;
            }
            m.SetFloat("_Gloss",gloss);m.SetFloat("_Opacity",opacity);
            m.SetFloat("_TintStrength",toy.FinishId=="gloss-vinyl"?0:1);
            m.SetFloat("_WinterPalette",1); // Same phone-scale pile and foam detail as the approved winter toys.
            m.renderQueue=opacity<1?3000:2000;
            return m;
        }
    }
}
