#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Pockle.Core
{
    public readonly struct CatalogColor
    {
        public float Red { get; }
        public float Green { get; }
        public float Blue { get; }
        public CatalogColor(uint rgb)
        { Red = ((rgb >> 16) & 255) / 255f; Green = ((rgb >> 8) & 255) / 255f; Blue = (rgb & 255) / 255f; }
    }

    public sealed class CharacterDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public CatalogColor AccentColor { get; }
        public string LeadFinishId { get; }
        public string IdleProfileId => Id;
        internal CharacterDefinition(string id, string name, uint accent, string lead)
        { Id = id; DisplayName = name; AccentColor = new CatalogColor(accent); LeadFinishId = lead; }
    }

    public sealed class FinishDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        /// <summary>Intended material behavior; new profiles are not applied by this catalog-only pass.</summary>
        public string HandlingProfileId { get; }
        internal FinishDefinition(string id, string name, string handling)
        { Id = id; DisplayName = name; HandlingProfileId = handling; }
    }

    public sealed class CollectibleDefinition
    {
        public string Id { get; }
        public string CharacterId { get; }
        public string FinishId { get; }
        public string FinishDisplayName { get; }
        public string DisplayName { get; }
        /// <summary>An asset is playable in this build; this does not establish reward eligibility.</summary>
        public bool Available { get; }
        /// <summary>The character collection; membership does not grant art availability or reward eligibility.</summary>
        public string CollectionId { get; }
        /// <summary>Derived from the character collection's explicit secret identity.</summary>
        public bool IsSecret => ToyCatalog.TryGetCollection(CollectionId, out var collection) && collection.SecretId == Id;
        internal CollectibleDefinition(string id, CharacterDefinition character, FinishDefinition finish,
            string label, bool available, string collection)
        {
            Id = id; CharacterId = character.Id; FinishId = finish.Id;
            FinishDisplayName = label; DisplayName = character.DisplayName + " · " + label;
            Available = available; CollectionId = collection;
        }
    }

    public sealed class BoxPoolEntry
    {
        public string CollectibleId { get; }
        public int Weight { get; }
        public BoxPoolEntry(string id, int weight)
        {
            if (!ToyCatalog.IsValidId(id)) throw new ArgumentException("Invalid collectible ID.", nameof(id));
            if (weight <= 0) throw new ArgumentOutOfRangeException(nameof(weight));
            CollectibleId = id; Weight = weight;
        }
    }
    public sealed class BoxPoolDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public IReadOnlyList<BoxPoolEntry> Entries { get; }
        public long TotalWeight { get; }
        public BoxPoolDefinition(string id, string name, params BoxPoolEntry[] entries)
        {
            if (!ToyCatalog.IsValidId(id)) throw new ArgumentException("Invalid pool ID.", nameof(id));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("A pool needs a name.", nameof(name));
            if (entries == null || entries.Length == 0) throw new ArgumentException("A pool needs explicit outcomes.", nameof(entries));
            var copy = (BoxPoolEntry[])entries.Clone();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var entry in copy)
            {
                if (entry == null || !ids.Add(entry.CollectibleId)) throw new ArgumentException("Null or duplicate pool entry.", nameof(entries));
                TotalWeight += entry.Weight;
            }
            Id = id; DisplayName = name; Entries = Array.AsReadOnly(copy);
        }
        public bool TryChoose(float unitRoll, out string collectibleId)
        {
            collectibleId = "";
            if (!Numeric.IsFinite(unitRoll) || unitRoll < 0 || unitRoll > 1 || Entries.Count == 0) return false;
            double threshold = (double)unitRoll * TotalWeight; long cumulative = 0;
            foreach (var entry in Entries)
            {
                cumulative += entry.Weight;
                if (threshold < cumulative) { collectibleId = entry.CollectibleId; return true; }
            }
            collectibleId = Entries[Entries.Count - 1].CollectibleId; return true; // Inclusive RNG endpoint.
        }
        public bool ContainsCollectible(string collectibleId)
        { foreach (var entry in Entries) if (entry.CollectibleId == collectibleId) return true; return false; }
    }

    /// <summary>Immutable roster definitions. Concepts remain unavailable until their runtime art exists.</summary>
    public static class ToyCatalog
    {
        public const string DailyPoolId = "jelly-garden";
        public const string PeachId = "pip.peach-jelly";
        public const string MoonId = "pip.moon-pearl";
        public const string GoldId = "pip.gold-confetti";
        public const string MintId = "pip.mint-mochi";
        private static readonly string[] legacyIds = { PeachId, MoonId, GoldId, MintId };
        private static readonly Dictionary<string, CharacterDefinition> charactersById = new Dictionary<string, CharacterDefinition>(StringComparer.Ordinal);
        private static readonly Dictionary<string, FinishDefinition> finishesById = new Dictionary<string, FinishDefinition>(StringComparer.Ordinal);
        private static readonly Dictionary<string, CollectibleDefinition> collectiblesById = new Dictionary<string, CollectibleDefinition>(StringComparer.Ordinal);
        private static readonly Dictionary<string, BoxPoolDefinition> poolsById = new Dictionary<string, BoxPoolDefinition>(StringComparer.Ordinal);
        private static readonly Dictionary<string, CollectionDefinition> collectionsById = new Dictionary<string, CollectionDefinition>(StringComparer.Ordinal);
        public static IReadOnlyList<CollectionDefinition> Collections { get; }
        public static IReadOnlyList<BoxPoolDefinition> BoxPools { get; }
        public static IReadOnlyList<CharacterDefinition> Characters { get; }
        public static IReadOnlyList<FinishDefinition> Finishes { get; }
        public static IReadOnlyList<CollectibleDefinition> Collectibles { get; }

        static ToyCatalog()
        {
            var finishes = new[] {
                new FinishDefinition("clear-jelly", "Clear Jelly", "gel"),
                new FinishDefinition("pearl-jelly", "Pearl Jelly", "pearl-gel"),
                new FinishDefinition("confetti-gel", "Confetti Gel", "filled-gel"),
                new FinishDefinition("mochi-foam", "Mochi Foam", "foam"),
                new FinishDefinition("velvet-flock", "Velvet Flock", "flock"),
                new FinishDefinition("boucle-plush", "Bouclé Plush", "plush"),
                new FinishDefinition("matte-vinyl", "Matte Vinyl", "firm"),
                new FinishDefinition("gloss-vinyl", "Gloss Vinyl", "firm"),
                new FinishDefinition("coated-metallic", "Coated Metallic", "firm-coated"),
                new FinishDefinition("glow-jelly", "Glow Jelly", "gel")
            };
            var characters = new[] {
                new CharacterDefinition("pip", "Pip", 0xf5b89c, "clear-jelly"),
                new CharacterDefinition("dew", "Dew", 0x9cdbdc, "clear-jelly"),
                new CharacterDefinition("tula", "Tula", 0xabcaae, "mochi-foam"),
                new CharacterDefinition("ripple", "Ripple", 0xa9bfeb, "pearl-jelly"),
                new CharacterDefinition("moss", "Moss", 0x94ba8a, "velvet-flock"),
                new CharacterDefinition("nook", "Nook", 0xd4bba5, "boucle-plush"),
                new CharacterDefinition("wisp", "Wisp", 0xc7b6df, "velvet-flock"),
                new CharacterDefinition("loop", "Loop", 0xeabccf, "boucle-plush"),
                new CharacterDefinition("bop", "Bop", 0xf1c47b, "gloss-vinyl"),
                new CharacterDefinition("rolo", "Rolo", 0x96bbc7, "matte-vinyl"),
                new CharacterDefinition("mallow", "Mallow", 0xd3c5e5, "matte-vinyl"),
                new CharacterDefinition("sprig", "Sprig", 0xaecba0, "coated-metallic")
            };
            Characters = Array.AsReadOnly(characters); Finishes = Array.AsReadOnly(finishes);
            foreach (var character in characters) charactersById.Add(character.Id, character);
            foreach (var finish in finishes) finishesById.Add(finish.Id, finish);
            var collectibles = new List<CollectibleDefinition>(characters.Length * finishes.Length);
            string[] pipLabels = { "Peach Jelly", "Moon Jelly", "Gold Glitter", "Mint Soft" };
            foreach (var character in characters)
            {
                for (int index = 0; index < finishes.Length; index++)
                {
                    var finish = finishes[index];
                    bool existing = character.Id == "pip" && index < legacyIds.Length;
                    string id = existing ? legacyIds[index] : character.Id + "." + finish.Id;
                    var collectible = new CollectibleDefinition(id, character, finish,
                        existing ? pipLabels[index] : finish.DisplayName, existing, character.Id);
                    collectibles.Add(collectible); collectiblesById.Add(id, collectible);
                }
            }
            Collectibles = new ReadOnlyCollection<CollectibleDefinition>(collectibles);
            var pools = new[] {
                new BoxPoolDefinition(DailyPoolId, "Jelly Garden", new BoxPoolEntry(PeachId, 50), new BoxPoolEntry(MintId, 50)),
                new BoxPoolDefinition("midnight-glow", "Midnight Glow", new BoxPoolEntry(MoonId, 100)),
                new BoxPoolDefinition("gold-confetti", "Gold Confetti", new BoxPoolEntry(GoldId, 100))
            };
            BoxPools = Array.AsReadOnly(pools);
            foreach (var pool in pools) poolsById.Add(pool.Id, pool);
            // A collection is one character with >=10 varieties. Pip belongs to Series 1;
            // dates/full collection odds and other characters' public numbering remain unapproved.
            var collections = new List<CollectionDefinition>(characters.Length);
            foreach (var character in characters)
            {
                var members = new List<string>(finishes.Length);
                foreach (var item in collectibles) if (item.CharacterId == character.Id) members.Add(item.Id);
                uint field = character.Id == "pip" ? 0xf8d9c6u : 0xfaf5eeu;
                collections.Add(new CollectionDefinition(character.Id, character.DisplayName, character.Id, members,
                    seriesNumber: character.Id == "pip" ? 1 : 0, fieldColor: field));
            }
            Collections = new ReadOnlyCollection<CollectionDefinition>(collections);
            foreach (var collection in collections) collectionsById.Add(collection.Id, collection);
        }

        public static bool TryGetCharacter(string? id, out CharacterDefinition character)
        { character = null!; return id != null && charactersById.TryGetValue(id, out character!); }
        public static bool TryGetFinish(string? id, out FinishDefinition finish)
        { finish = null!; return id != null && finishesById.TryGetValue(id, out finish!); }
        public static bool TryGetCollectible(string? id, out CollectibleDefinition collectible)
        { collectible = null!; return id != null && collectiblesById.TryGetValue(id, out collectible!); }
        public static string LegacyCollectibleId(int index)
        {
            if (index < 0 || index >= legacyIds.Length) throw new ArgumentOutOfRangeException(nameof(index));
            return legacyIds[index];
        }
        public static bool TryGetLegacyIndex(string? id, out int index)
        {
            for (index = 0; index < legacyIds.Length; index++)
                if (string.Equals(id, legacyIds[index], StringComparison.Ordinal)) return true;
            index = -1; return false;
        }
        public static bool TryGetBoxPool(string? id, out BoxPoolDefinition pool)
        { pool = null!; return id != null && poolsById.TryGetValue(id, out pool!); }
        public static bool TryGetCollection(string? id, out CollectionDefinition collection)
        { collection = null!; return id != null && collectionsById.TryGetValue(id, out collection!); }
        /// <summary>Legacy beta offers are Pip sub-pools, not distinct character collections.</summary>
        public static bool TryGetCollectionForPool(string? poolId, out CollectionDefinition collection)
        {
            collection = null!;
            if (!TryGetBoxPool(poolId, out var pool) || !TryGetCollectible(pool.Entries[0].CollectibleId, out var first) ||
                !TryGetCollection(first.CollectionId, out collection)) return false;
            foreach (var entry in pool.Entries)
                if (!TryGetCollectible(entry.CollectibleId, out var item) || item.CollectionId != collection.Id)
                { collection = null!; return false; }
            return true;
        }
        public static bool IsDailyEligible(string? id)
        {
            if (!TryGetCollectible(id, out var collectible) || !collectible.Available) return false;
            foreach (var entry in poolsById[DailyPoolId].Entries) if (entry.CollectibleId == id) return true;
            return false;
        }
        /// <summary>Validate persisted IDs without requiring this client to know a future collectible.</summary>
        public static bool IsValidId(string? id)
        {
            if (string.IsNullOrEmpty(id) || id.Length > 128) return false;
            foreach (char c in id)
                if (!(c >= 'a' && c <= 'z') && !(c >= '0' && c <= '9') && c != '.' && c != '-' && c != '_') return false;
            return true;
        }
    }
}
