using Pockle.Core;
using UnityEngine;

namespace Pockle.Runtime
{
    /// <summary>Local identity only; ID references preserve future selections across catalog changes.</summary>
    public sealed class GuestProfile
    {
        private const string Prefix = "pockle.profile.local.";
        public string Name { get; private set; }
        public string AvatarId { get; private set; }
        public string FavoriteId { get; private set; }
        // Compatibility view for the current four-Pip HUD; unknown future choices remain stored by ID.
        public PipVariant Avatar => (PipVariant)ProfileSelection.LegacyIndex(AvatarId);
        public PipVariant Favorite => (PipVariant)ProfileSelection.LegacyIndex(FavoriteId);
        public GuestProfile()
        {
            Name = ProfileName.Normalize(PlayerPrefs.GetString(Prefix + "name", "Collector"));
            string savedAvatar = PlayerPrefs.GetString(Prefix + "avatarId", "");
            string savedFavorite = PlayerPrefs.GetString(Prefix + "favoriteId", "");
            AvatarId = ProfileSelection.MigrateId(savedAvatar, PlayerPrefs.GetInt(Prefix + "avatar", 0));
            FavoriteId = ProfileSelection.MigrateId(savedFavorite, PlayerPrefs.GetInt(Prefix + "favorite", 0));
            if (savedAvatar != AvatarId || savedFavorite != FavoriteId) Save();
        }
        public void SetName(string name) { Name = ProfileName.Normalize(name); Save(); }
        public void SetAvatar(PipVariant avatar) { AvatarId = PipVariants.CollectibleId(avatar); Save(); }
        public void SetFavorite(PipVariant favorite) { FavoriteId = PipVariants.CollectibleId(favorite); Save(); }
        public bool SetAvatar(string id)
        { if (!ProfileSelection.CanSelect(id)) return false; AvatarId = id; Save(); return true; }
        // The caller additionally checks ownership before offering a favorite choice.
        public bool SetFavorite(string id)
        { if (!ProfileSelection.CanSelect(id)) return false; FavoriteId = id; Save(); return true; }
        private void Save()
        {
            PlayerPrefs.SetString(Prefix + "name", Name);
            PlayerPrefs.SetString(Prefix + "avatarId", AvatarId);
            PlayerPrefs.SetString(Prefix + "favoriteId", FavoriteId);
            PlayerPrefs.SetInt(Prefix + "avatar", (int)Avatar);
            PlayerPrefs.SetInt(Prefix + "favorite", (int)Favorite);
            PlayerPrefs.Save();
        }
    }
}
