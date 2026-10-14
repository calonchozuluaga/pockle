using Pockle.Core;
using UnityEngine;

namespace Pockle.Runtime
{
    /// <summary>Explicit art registry for available toys and editor model studies; art loading never grants ownership.</summary>
    public static class CharacterArt
    {
        public const string MossStudyId = "moss.velvet-flock";
        public const string BopStudyId = "bop.gloss-vinyl";
        public const string NookPlushStudyId = "nook.boucle-plush";
        public const string NookFoamStudyId = "nook.mochi-foam";

        public static bool IsStudy(string id)
        { return id == ToyCatalog.AxolotlId || id == ToyCatalog.FoxId || id == MossStudyId || id == BopStudyId || id == NookPlushStudyId || id == NookFoamStudyId; }

        public static bool TryLoad(string id, out PipCharacterAsset asset)
        {
            asset = null;
            if (!ToyCatalog.TryGetCollectible(id, out var definition)) return false;
            string path;
            if (definition.CharacterId == "pip")
            {
                path = PipCharacterAsset.ResourcePath;
                if (PlayerPrefs.GetInt(PipCharacterAsset.MeshyPreference, 1) != 0 || !ToyCatalog.TryGetLegacyIndex(id,out _))
                {
                    asset = Resources.Load<PipCharacterAsset>(PipCharacterAsset.MeshyResourcePath);
                    if (asset != null && asset.CharacterId == "pip" && asset.BakedSurface &&
                        asset.BaseColorTexture != null && asset.BodyMesh != null && asset.BodyMesh.isReadable)
                        return true;
                    if (!ToyCatalog.TryGetLegacyIndex(id,out _)) return false;
                    // The reviewed authored model remains a safe fallback on import failure.
                }
            }
            else if (definition.CharacterId == "axolotl" && definition.Available) path = "Characters/Axolotl/Axolotl";
            else if (ToyCatalog.IsFoxCollectible(id)) path = "Characters/Fox/Fox";
            else if (id == MossStudyId) path = "Characters/Moss/Moss";
            else if (id == BopStudyId) path = "Characters/Bop/Bop";
            else if (id == NookPlushStudyId || id == NookFoamStudyId) path = "Characters/Nook/Nook";
            else return false;
            asset = Resources.Load<PipCharacterAsset>(path);
            return asset != null && asset.CharacterId == definition.CharacterId &&
                asset.BodyMesh != null && asset.BodyMesh.isReadable;
        }
    }
}
