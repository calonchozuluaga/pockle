using Pockle.Core;
using UnityEngine;
namespace Pockle.Runtime
{
    public static class HolidayFinishMaterial
    {
        public static Material Create(string id, Texture paint, Cubemap studio)
        {
            if (!ToyCatalog.TryGetCollectible(id, out var toy) || !HolidayCatalog.IsCharacter(toy.CharacterId))
                throw new System.ArgumentException("Expected a Christmas character finish.", nameof(id));
            var shader=Resources.Load<Shader>("Characters/HolidayFinish");
            if(shader==null || !shader.isSupported) throw new System.InvalidOperationException("Christmas finish shader unavailable.");
            var m=new Material(shader){name=toy.DisplayName,mainTexture=paint};
            m.SetTexture("_StudioCube",studio);
            Color tint=BaseTint(toy.CharacterId);
            float gloss=.12f,opacity=1;
            switch(toy.FinishId)
            {
                case "clear-jelly": gloss=.86f;opacity=.70f;break;
                case "pearl-jelly": tint=Color.Lerp(tint,new Color(.85f,.8f,.96f),.65f);gloss=.68f;m.SetFloat("_Pearl",1);break;
                case "confetti-gel": gloss=.82f;opacity=.76f;m.SetFloat("_Glitter",1);break;
                case "mochi-foam": tint=Color.Lerp(tint,Color.white,.28f);m.SetFloat("_Grain",.35f);break;
                case "velvet-flock": tint=Color.Lerp(tint,new Color(.70f,.58f,.85f),.45f);m.SetFloat("_Fuzz",.75f);break;
                case "boucle-plush": tint=Color.Lerp(tint,Color.white,.2f);m.SetFloat("_Fuzz",.45f);break;
                case "matte-vinyl": break;
                case "gloss-vinyl": gloss=.94f;break;
                case "coated-metallic": tint=(toy.CharacterId=="snowy"||toy.CharacterId=="guin")?new Color(.66f,.8f,.9f):new Color(.90f,.63f,.20f);gloss=.86f;m.SetFloat("_Metallic",1);break;
                case "glow-jelly": tint=GlowTint(toy.CharacterId);gloss=.88f;opacity=.78f;m.SetFloat("_Iridescence",.55f);m.SetFloat("_Glitter",.65f);break;
            }
            m.SetColor("_BodyColor",tint);m.SetFloat("_Gloss",gloss);m.SetFloat("_Opacity",opacity);
            m.SetFloat("_TintStrength",toy.FinishId=="matte-vinyl"?0:1);
            m.SetFloat("_PigmentKind",toy.CharacterId=="guin"?3:toy.CharacterId=="firb"?0:toy.CharacterId=="snowy"?2:1);
            m.SetFloat("_MotionKind",toy.CharacterId=="reiny"?4:toy.CharacterId=="guin"?5:toy.CharacterId=="bundy"?6:toy.CharacterId=="firb"?0:toy.CharacterId=="coco"?1:toy.CharacterId=="snowy"?2:3);
            // Face centres are in normalized native mesh space, preserving the uploaded painted features.
            Vector4 left,right,mouth;
            if(toy.CharacterId=="firb") {left=new Vector4(-.225f,-.08f,.15f,.15f);right=new Vector4(.225f,-.08f,.15f,.15f);mouth=new Vector4(0,-.13f,.11f,.08f);}
            else if(toy.CharacterId=="coco") {left=new Vector4(-.58f,-.09f,.16f,.17f);right=new Vector4(.11f,-.09f,.16f,.17f);mouth=new Vector4(-.22f,-.145f,.13f,.09f);}
            else if(toy.CharacterId=="snowy") {left=new Vector4(-.22f,.20f,.12f,.14f);right=new Vector4(.22f,.20f,.12f,.14f);mouth=new Vector4(0,.04f,.12f,.07f);}
            else {left=new Vector4(-.30f,.18f,.16f,.16f);right=new Vector4(.30f,.18f,.16f,.16f);mouth=new Vector4(0,.10f,.13f,.08f);}
            m.SetVector("_LeftEyeCenter",left);m.SetVector("_RightEyeCenter",right);m.SetVector("_MouthCenter",mouth);
            m.renderQueue=opacity<1?3000:2000;return m;
        }
        public static Color GlowTint(string id) => id=="reiny"?new Color(1,.68f,.28f):id=="guin"?new Color(.40f,.78f,1):id=="bundy"?new Color(.55f,.95f,.58f): id=="ginger"?new Color(.7f,.42f,.82f):id=="coco"?new Color(1,.55f,.20f):new Color(.4f,.85f,.72f);
        private static Color BaseTint(string id) => id=="reiny"?new Color(.76f,.52f,.30f):id=="guin"?new Color(.38f,.59f,.76f):id=="bundy"?new Color(.56f,.30f,.18f): id=="firb"?new Color(.37f,.66f,.46f):id=="coco"?new Color(.79f,.43f,.28f):id=="snowy"?new Color(.81f,.94f,.97f):new Color(.87f,.58f,.31f);
    }
}
