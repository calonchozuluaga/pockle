#nullable enable
using System;
using System.Collections.Generic;
namespace Pockle.Core
{
    /// <summary>Versioned local fusion receipt: each record is one consumption of both parents and one personal toy.</summary>
    [Serializable]
    public sealed class FusedToy
    {
        public string Id = "", First = "", Second = "", Name = "", Material = "";
        public int RecipeVersion = 1;
        public string Personality = "", PersonalityLine = "", HybridId = "";
        public int Energy, Mood, Boldness, Bedtime, Temperament;
    }

    /// <summary>Definitions for the active local save only; never added to finite collections or reward pools.</summary>
    public static class FusionCatalog
    {
        private static readonly Dictionary<string, CollectibleDefinition> definitions = new Dictionary<string, CollectibleDefinition>(StringComparer.Ordinal);
        private static readonly Dictionary<string, FusedToy> records = new Dictionary<string, FusedToy>(StringComparer.Ordinal);
        public static bool TryGet(string? id, out FusedToy record)
        { record = null!; return id != null && records.TryGetValue(id, out record!); }
        public static void Restore(IEnumerable<FusedToy>? saved)
        {
            records.Clear(); definitions.Clear();
            if (saved == null) return;
            foreach (var record in saved) if (Valid(record) && !records.ContainsKey(record.Id)) records.Add(record.Id, record);
        }
        public static bool IsSource(string? id) => id != null && !id.StartsWith("fusion.", StringComparison.Ordinal)
            && ToyCatalog.TryGetCollectible(id, out var toy) && toy.Available && toy.CollectionId == toy.CharacterId;
        public static bool Valid(FusedToy? record)
        {
            if (record == null || string.IsNullOrEmpty(record.Id) || !record.Id.StartsWith("fusion.", StringComparison.Ordinal) || !ToyCatalog.IsValidId(record.Id)
                || record.RecipeVersion != 1 || string.IsNullOrEmpty(record.Name) || record.Name.Length > 100
                || !IsSource(record.First) || !IsSource(record.Second)) return false;
            ToyCatalog.TryGetCollectible(record.First, out var first); ToyCatalog.TryGetCollectible(record.Second, out var second);
            return first.CharacterId == second.CharacterId && record.Temperament >= 0 && record.Temperament < 8
                && record.Energy >= 0 && record.Energy <= 100 && record.Mood >= 0 && record.Mood <= 100
                && record.Boldness >= 0 && record.Boldness <= 100 && record.Bedtime >= 0 && record.Bedtime <= 100;
        }
        public static CatalogColor Swatch(FusedToy record)
        {
            ToyCatalog.TryGetCollectible(record.First, out var a); ToyCatalog.TryGetCollectible(record.Second, out var b);
            var x = a.SwatchColor; var y = b.SwatchColor;
            uint red = (uint)((x.Red + y.Red) * 127.5f), green = (uint)((x.Green + y.Green) * 127.5f), blue = (uint)((x.Blue + y.Blue) * 127.5f);
            return new CatalogColor((red << 16) | (green << 8) | blue);
        }
        internal static bool TryGetDefinition(string? id, out CollectibleDefinition toy)
        {
            toy = null!;
            if (id != null && definitions.TryGetValue(id, out toy!)) return true;
            if (!TryGet(id, out var record) || !ToyCatalog.TryGetCollectible(record.First, out var parent)
                || !ToyCatalog.TryGetCharacter(parent.CharacterId, out var character)) return false;
            var finish = new FinishDefinition(parent.FinishId, record.Material, "fusion");
            toy = new CollectibleDefinition(record.Id, character, finish, record.Name, true, parent.CollectionId);
            definitions[record.Id] = toy;
            return true;
        }
    }
}
