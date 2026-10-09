using Pockle.Core;
using UnityEngine;

namespace Pockle.Runtime
{
    /// <summary>Explicit art registry. Model studies are loadable for review, but remain unavailable in the production catalog.</summary>
    public static class CharacterArt
    {
        public const string MossStudyId = "moss.velvet-flock";
        public const string BopStudyId = "bop.gloss-vinyl";
        public const string NookPlushStudyId = "nook.boucle-plush";
        public const string NookFoamStudyId = "nook.mochi-foam";

        public static bool IsStudy(string id)
        { return id == MossStudyId || id == BopStudyId || id == NookPlushStudyId || id == NookFoamStudyId; }

        public static bool TryLoad(string id, out PipCharacterAsset asset)
        {
            asset = null;
            if (!ToyCatalog.TryGetCollectible(id, out var definition)) return false;
            string path;
            if (ToyCatalog.TryGetLegacyIndex(id, out _)) path = PipCharacterAsset.ResourcePath;
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
