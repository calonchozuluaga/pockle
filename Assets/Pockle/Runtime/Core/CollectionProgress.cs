#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Pockle.Core
{
    [Serializable]
    public sealed class OwnedCollectible
    {
        public string Id = "";
        public int Count;
        public OwnedCollectible() { }
        public OwnedCollectible(string id, int count) { Id = id; Count = count; }
    }

    [Serializable]
    public sealed class CollectionSave
    {
        public int Version = 1;
        // Version-1 compatibility projections. ID inventory is authoritative in version 2.
        public int[] Counts = new int[4];
        public List<OwnedCollectible>? Inventory;
        public string PendingRevealId = "";
        public int Day;
        public int Steps;
        public bool Claimed;
        public long SensorTotal = -1;
        public double SensorUptime = -1;
        public int SensorBoot = -1;
        public int PendingReveal = -1;
    }

    /// <summary>Local beta inventory. Production rewards need server validation.</summary>
    public sealed class CollectionProgress
    {
        public const int DailyTarget = 1000;
        public CollectionSave Save { get; }
        public const int SaveVersion = 2;
        private readonly Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.Ordinal);
        public IReadOnlyDictionary<string, int> OwnedCounts { get; }
        public bool CanClaim => Save.Steps >= DailyTarget && !Save.Claimed && Save.PendingRevealId.Length == 0;
        public int GetOwnedCount(string? id) => id != null && counts.TryGetValue(id, out int count) ? count : 0;

        public CollectionProgress(CollectionSave? saved, int day)
        {
            Save = saved ?? StarterSave();
            if (Save.Version != 1 && Save.Version != SaveVersion) throw new ArgumentException("Unsupported collection save version.");
            if (Save.Version == 1)
            {
                for (int i = 0; i < 4; i++)
                    counts.Add(ToyCatalog.LegacyCollectibleId(i), BoundedCount(Save.Counts != null && i < Save.Counts.Length ? Save.Counts[i] : 0));
                Save.PendingRevealId = Save.PendingReveal >= 0 && Save.PendingReveal < 4 ? ToyCatalog.LegacyCollectibleId(Save.PendingReveal) : "";
            }
            else if (Save.Inventory != null)
            {
                foreach (var entry in Save.Inventory)
                {
                    if (entry == null || !ToyCatalog.IsValidId(entry.Id)) continue;
                    // Duplicate serialized rows are not additional rewards; preserve the largest valid count.
                    counts[entry.Id] = Math.Max(GetOwnedCount(entry.Id), BoundedCount(entry.Count));
                }
            }
            Save.Version = SaveVersion;
            Save.PendingRevealId = ToyCatalog.IsValidId(Save.PendingRevealId) ? Save.PendingRevealId : "";
            Save.Steps = Math.Max(0, Math.Min(DailyTarget, Save.Steps));
            OwnedCounts = new ReadOnlyDictionary<string, int>(counts);
            SynchronizeInventory();
            AdvanceDay(day);
        }

        // Days only move forward: rolling the device clock backward cannot reset a claim.
        public bool AdvanceDay(int day)
        {
            if (day <= Save.Day) return false;
            Save.Day = day;
            Save.Steps = 0;
            Save.Claimed = false;
            Save.SensorTotal = -1; // Never assign an interval across midnight to either day.
            Save.SensorUptime = -1;
            return true;
        }

        public bool ObserveSteps(int day, long total, double uptime, int boot)
        {
            bool changed = AdvanceDay(day);
            if (day != Save.Day || total < 0 || double.IsNaN(uptime) || double.IsInfinity(uptime) || uptime < 0)
                return changed;
            long previous = Save.SensorTotal;
            double elapsed = uptime - Save.SensorUptime;
            bool reset = previous < 0 || total < previous || elapsed < 0 ||
                (boot >= 0 && Save.SensorBoot >= 0 && boot != Save.SensorBoot);
            if (!reset && total == previous) return changed; // Retain elapsed time for batched sensor updates.
            if (!reset && elapsed == 0) return changed;
            Save.SensorTotal = total;
            Save.SensorUptime = uptime;
            Save.SensorBoot = boot;
            if (!reset)
            {
                long delta = total - previous;
                // Reject implausible batches, but tolerate sensor batching and pocket walks.
                if (delta > 0 && delta <= Math.Max(20d, elapsed * 4d))
                    Save.Steps = (int)Math.Min(DailyTarget, Save.Steps + delta);
            }
            return changed || total != previous || reset;
        }

        public bool TryClaimDaily(int day, int variant)
        {
            if (variant < 0 || variant >= 4) return false;
            return TryClaimDaily(day, ToyCatalog.LegacyCollectibleId(variant));
        }

        public bool TryClaimDaily(int day, string? collectibleId)
        {
            AdvanceDay(day);
            if (day != Save.Day || !CanClaim || !ToyCatalog.IsDailyEligible(collectibleId)) return false;
            Save.Claimed = true;
            string id = collectibleId!;
            counts[id] = BoundedCount(GetOwnedCount(id) + 1);
            Save.PendingRevealId = id; // Persist ownership before animation; interruption cannot grant twice.
            SynchronizeInventory();
            return true;
        }

        public void FinishReveal() { FinishReveal(Save.PendingRevealId); }
        public bool FinishReveal(string? expectedId)
        {
            if (string.IsNullOrEmpty(expectedId) || expectedId != Save.PendingRevealId) return false;
            Save.PendingRevealId = ""; Save.PendingReveal = -1; return true;
        }
        private static int BoundedCount(int count) => Math.Max(0, Math.Min(9999, count));
        private static CollectionSave StarterSave()
        {
            var saved = new CollectionSave { Version = SaveVersion, Inventory = new List<OwnedCollectible>() };
            for (int i = 0; i < 4; i++) saved.Inventory.Add(new OwnedCollectible(ToyCatalog.LegacyCollectibleId(i), 1));
            return saved;
        }
        private void SynchronizeInventory()
        {
            var ids = new List<string>(counts.Keys); ids.Sort(StringComparer.Ordinal);
            Save.Inventory = new List<OwnedCollectible>(ids.Count);
            foreach (string id in ids) Save.Inventory.Add(new OwnedCollectible(id, counts[id]));
            Save.Counts = new int[4];
            for (int i = 0; i < 4; i++) Save.Counts[i] = GetOwnedCount(ToyCatalog.LegacyCollectibleId(i));
            Save.PendingReveal = GetOwnedCount(Save.PendingRevealId) > 0 && ToyCatalog.TryGetLegacyIndex(Save.PendingRevealId, out int index) ? index : -1;
        }

        public static int UtcDay(DateTime utc) => (int)(utc.Date - new DateTime(1970, 1, 1)).TotalDays;
    }
}
