#nullable enable
using System;
using System.Collections.Generic;

namespace Pockle.Core
{
    /// <summary>An exact weighted outcome for disclosure; secrets are collection-specific.</summary>
    public sealed class CollectionOdds
    {
        public string CollectibleId { get; }
        public int Weight { get; }
        public long TotalWeight { get; }
        public double Probability => (double)Weight / TotalWeight;
        public bool IsSecret { get; }
        internal CollectionOdds(BoxPoolEntry entry, long total, string secretId)
        { CollectibleId = entry.CollectibleId; Weight = entry.Weight; TotalWeight = total; IsSecret = CollectibleId == secretId; }
    }

    /// <summary>Read-only series presentation, separate from asset availability and ownership.</summary>
    public sealed class CollectionDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        /// <summary>A collection contains varieties of exactly one character.</summary>
        public string CharacterId { get; }
        /// <summary>Zero means the prototype has no approved public series number.</summary>
        public int SeriesNumber { get; }
        public DateTime? ReleaseDateUtc { get; }
        /// <summary>Optional schema support. Current product rules leave this unset: series do not retire.</summary>
        public DateTime? RetireDateUtc { get; }
        public bool HasPublishedSchedule => SeriesNumber > 0 && ReleaseDateUtc.HasValue;
        public IReadOnlyList<string> Members { get; }
        public string SecretId { get; }
        public bool HasSecret => SecretId.Length > 0;
        public IReadOnlyList<CollectionOdds> Odds { get; }
        /// <summary>Null until the complete collection's draw policy is approved.</summary>
        public BoxPoolDefinition? Pool { get; }
        public bool HasApprovedOdds => Pool != null;
        public string ProductId { get; }
        public string BoxPriceTier { get; }
        public CatalogColor FieldColor { get; }
        public CatalogColor InkColor { get; }

        public CollectionDefinition(string id, string displayName, string characterId, IReadOnlyList<string> members,
            int seriesNumber = 0,
            DateTime? releaseDateUtc = null, DateTime? retireDateUtc = null, string secretId = "",
            string productId = "", string boxPriceTier = "", uint fieldColor = 0xfaf5ee, uint inkColor = 0x49344c,
            BoxPoolDefinition? pool = null)
        {
            if (!ToyCatalog.IsValidId(id) || !ToyCatalog.IsValidId(characterId) || string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("A collection requires valid identity and a display name.");
            if (members == null || members.Count == 0) throw new ArgumentException("A collection needs ordered members.", nameof(members));
            if (seriesNumber < 0 || (releaseDateUtc.HasValue && seriesNumber == 0))
                throw new ArgumentOutOfRangeException(nameof(seriesNumber));
            RequireDate(releaseDateUtc, nameof(releaseDateUtc)); RequireDate(retireDateUtc, nameof(retireDateUtc));
            if (retireDateUtc.HasValue && (!releaseDateUtc.HasValue || retireDateUtc.Value <= releaseDateUtc.Value))
                throw new ArgumentException("Retirement must follow release.", nameof(retireDateUtc));
            if (secretId == null || productId == null || boxPriceTier == null) throw new ArgumentNullException();
            var copy = new string[members.Count];
            var unique = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < copy.Length; i++)
            {
                string member = members[i];
                if (!ToyCatalog.IsValidId(member) || !member.StartsWith(characterId + ".", StringComparison.Ordinal) || !unique.Add(member))
                    throw new ArgumentException("Members must be unique varieties of the collection's character.", nameof(members));
                copy[i] = member;
            }
            if (secretId.Length > 0 && !unique.Contains(secretId))
                throw new ArgumentException("Secret must be a collection member.", nameof(secretId));
            var odds = new CollectionOdds[pool?.Entries.Count ?? 0];
            if (pool != null)
            {
                if (pool.Entries.Count != copy.Length)
                    throw new ArgumentException("Approved collection odds must cover every member.", nameof(pool));
                for (int i = 0; i < odds.Length; i++)
                {
                    if (!unique.Contains(pool.Entries[i].CollectibleId)) throw new ArgumentException("Prize lies outside the character collection.", nameof(pool));
                    odds[i] = new CollectionOdds(pool.Entries[i], pool.TotalWeight, secretId);
                }
                if (secretId.Length > 0 && !pool.ContainsCollectible(secretId))
                    throw new ArgumentException("Approved pool must disclose the secret's odds.", nameof(pool));
            }
            Id = id; DisplayName = displayName; CharacterId = characterId; Pool = pool;
            SeriesNumber = seriesNumber; ReleaseDateUtc = releaseDateUtc; RetireDateUtc = retireDateUtc;
            SecretId = secretId; ProductId = productId; BoxPriceTier = boxPriceTier;
            FieldColor = new CatalogColor(fieldColor); InkColor = new CatalogColor(inkColor);
            Members = Array.AsReadOnly(copy); Odds = Array.AsReadOnly(odds);
        }

        public bool TryGetOdds(string? collectibleId, out CollectionOdds odds)
        {
            foreach (var item in Odds) if (item.CollectibleId == collectibleId) { odds = item; return true; }
            odds = null!; return false;
        }

        /// <summary>Display scheduling only; never erases owned toys or silently changes reward eligibility.</summary>
        public bool IsReleasedAt(DateTime utcNow)
        {
            if (utcNow.Kind != DateTimeKind.Utc) throw new ArgumentException("Supply UTC time.", nameof(utcNow));
            return HasPublishedSchedule && utcNow >= ReleaseDateUtc!.Value &&
                (!RetireDateUtc.HasValue || utcNow < RetireDateUtc.Value);
        }

        private static void RequireDate(DateTime? date, string name)
        {
            if (date.HasValue && (date.Value.Kind != DateTimeKind.Utc || date.Value.TimeOfDay != TimeSpan.Zero))
                throw new ArgumentException("Supply a midnight UTC calendar date.", name);
        }
    }
}
