using UnityEngine;
namespace Pockle.Runtime
{
    /// <summary>Baked LDR panorama and its small reflection map; no runtime captures.</summary>
    public sealed class PaintedWorldAsset : ScriptableObject
    {
        public Texture2D Background;
        public Cubemap Reflections;
        public static PaintedWorldAsset Load(StageWorld world, string homeWorld)
        {
            string name=world==StageWorld.Garden?"jelly-garden":world==StageWorld.Midnight?"moonlit-pond":
                world==StageWorld.Home&&homeWorld!="studio"?homeWorld:null;
            return name==null?null:Resources.Load<PaintedWorldAsset>("Worlds/"+name);
        }
    }
}
