#nullable enable
namespace Pockle.Core
{
    /// <summary>Stable profile references survive client catalog changes without rewriting unknown IDs.</summary>
    public static class ProfileSelection
    {
        public static string MigrateId(string? savedId, int legacyIndex)
        {
            if (ToyCatalog.IsValidId(savedId)) return savedId!;
            return ToyCatalog.LegacyCollectibleId(legacyIndex >= 0 && legacyIndex < 4 ? legacyIndex : 0);
        }
        public static bool CanSelect(string? id) => ToyCatalog.TryGetCollectible(id, out var definition) && definition.Available;
        public static int LegacyIndex(string? id) => ToyCatalog.TryGetLegacyIndex(id, out int index) ? index : 0;
    }
}
