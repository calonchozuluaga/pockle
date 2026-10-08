using Pockle.Core;
using UnityEngine;

namespace Pockle.Runtime
{
    /// <summary>Local identity only; no public account or cloud ownership is implied.</summary>
    internal sealed class GuestProfile
    {
        private const string Prefix = "pockle.profile.local.";
        public string Name { get; private set; } = ProfileName.Normalize(PlayerPrefs.GetString(Prefix + "name", "Collector"));
        public PipVariant Avatar { get; private set; } = PipVariants.FromSaved(PlayerPrefs.GetInt(Prefix + "avatar", 0));
        public PipVariant Favorite { get; private set; } = PipVariants.FromSaved(PlayerPrefs.GetInt(Prefix + "favorite", 0));
        public void SetName(string name) { Name = ProfileName.Normalize(name); Save(); }
        public void SetAvatar(PipVariant avatar) { Avatar = PipVariants.FromSaved((int)avatar); Save(); }
        public void SetFavorite(PipVariant favorite) { Favorite = PipVariants.FromSaved((int)favorite); Save(); }
        private void Save()
        {
            PlayerPrefs.SetString(Prefix + "name", Name);
            PlayerPrefs.SetInt(Prefix + "avatar", (int)Avatar);
            PlayerPrefs.SetInt(Prefix + "favorite", (int)Favorite);
            PlayerPrefs.Save();
        }
    }
}
