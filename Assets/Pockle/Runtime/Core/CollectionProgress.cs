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
        public List<FusedToy>? Fusions;
        public string PendingFusionId = "";
        public BadgeRuntimeSave? BadgeState;
        public List<string>? PaidDeliveryIds;
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
    public sealed partial class CollectionProgress
    {
        public const int DailyTarget = 1000;
        public CollectionSave Save { get; }
        public int FusionsMade
        {
            get
            {
                var receipts = new HashSet<string>(StringComparer.Ordinal);
                if (Save.Fusions != null) foreach (var record in Save.Fusions)
                    if (FusionCatalog.Valid(record)) receipts.Add(record.Id);
                return receipts.Count;
            }
        }
        public const int SaveVersion = 5;
        private readonly Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.Ordinal);
        public IReadOnlyDictionary<string, int> OwnedCounts { get; }
        public bool CanClaim => Save.Steps >= DailyTarget && !Save.Claimed && Save.PendingRevealId.Length == 0 && string.IsNullOrEmpty(Save.PendingFusionId);
        public int GetOwnedCount(string? id) => id != null && counts.TryGetValue(id, out int count) ? count : 0;

        public CollectionProgress(CollectionSave? saved, int day)
        {
            Save = saved ?? StarterSave();
            if (Save.Version < 1 || Save.Version > SaveVersion) throw new ArgumentException("Unsupported collection save version.");
            FusionCatalog.Restore(Save.Fusions);
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
            Save.PendingFusionId = ToyCatalog.IsValidId(Save.PendingFusionId) ? Save.PendingFusionId : "";
            Save.Version = SaveVersion;
            Save.PendingRevealId = ToyCatalog.IsValidId(Save.PendingRevealId) ? Save.PendingRevealId : "";
            Save.Steps = Math.Max(0, Math.Min(DailyTarget, Save.Steps));
            InitializeBadgeFacts(day);
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
            Save.BadgeState!.DailyWalkingSteps = 0;
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
                {
                    Save.Steps = (int)Math.Min(DailyTarget, Save.Steps + Math.Min(delta, DailyTarget));
                    RecordWalking(delta);
                }
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
            int before = GetOwnedCount(id);
            counts[id] = BoundedCount(before + 1);
            if (counts[id] > before) MarkEarned(id);
            if (!Save.BadgeState!.WalkingBoxDays.Contains(day)) Save.BadgeState.WalkingBoxDays.Add(day);
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
        /// <summary>Owner-requested local test copy; never adds duplicates or changes box/reveal state.</summary>
        public bool EnsureFoxTestCopy()
        {
            if (GetOwnedCount(ToyCatalog.FoxId) > 0) return false;
            counts[ToyCatalog.FoxId] = 1;
            SynchronizeInventory();
            return true;
        }
        /// <summary>Add only missing owner test finishes; preserve duplicates and all reward state.</summary>
        public bool EnsureFoxFinishTestCopies()
        {
            bool changed = false;
            foreach (var toy in ToyCatalog.Collectibles)
                if (toy.CharacterId == "fox" && toy.Available && GetOwnedCount(toy.Id) == 0)
                { counts[toy.Id] = 1; changed = true; }
            if (changed) SynchronizeInventory();
            return changed;
        }
        /// <summary>Owner-requested Pip collection test copies; existing duplicates and rewards are preserved.</summary>
        public bool EnsurePipFinishTestCopies()
        {
            bool changed = false;
            foreach (var toy in ToyCatalog.Collectibles)
                if (toy.CollectionId == "pip" && toy.Available && GetOwnedCount(toy.Id) == 0)
                { counts[toy.Id] = 1; changed = true; }
            if (changed) SynchronizeInventory();
            return changed;
        }
        /// <summary>Owner-requested Axolotl copies; preserve existing duplicates and reward state.</summary>
        public bool EnsureAxolotlFinishTestCopies()
        {
            bool changed = false;
            foreach (var toy in ToyCatalog.Collectibles)
                if (toy.CharacterId == "axolotl" && toy.Available && GetOwnedCount(toy.Id) == 0)
                { counts[toy.Id] = 1; changed = true; }
            if (changed) SynchronizeInventory();
            return changed;
        }
        /// <summary>Add one missing copy of each approved owner study, preserving all existing state.</summary>
        public bool EnsurePipFusionStudyCopies()
        {
            bool changed=false;
            foreach(string id in PipFusionStudies.Ids)
                if(GetOwnedCount(id)==0){counts[id]=1;changed=true;}
            if(changed)SynchronizeInventory();return changed;
        }
        /// <summary>Development owner review: add only missing winter-trio copies, never consume or reset prior state.</summary>
        public bool EnsureWinterTrioTestCopies()
        {
            bool changed=false;
            foreach(var toy in ToyCatalog.Collectibles)
                if(HolidayCatalog.IsWinterTrio(toy.CharacterId)&&toy.Available&&GetOwnedCount(toy.Id)==0)
                {counts[toy.Id]=1;changed=true;}
            if(changed)SynchronizeInventory();return changed;
        }
        /// <summary>Owner fusion review: keep one display copy plus two spares of varied finishes in six collections.</summary>
        public bool EnsureFusionReviewSpares()
        {
            bool changed = false;
            foreach (var toy in ToyCatalog.Collectibles)
            {
                bool collection = toy.CollectionId == "pip" || toy.CollectionId == "fox" || toy.CollectionId == "axolotl"
                    || toy.CollectionId == "reiny" || toy.CollectionId == "guin" || toy.CollectionId == "bundy";
                bool finish = toy.FinishId == "clear-jelly" || toy.FinishId == "gloss-vinyl" || toy.FinishId == "coated-metallic";
                if (!collection || !finish || !toy.Available || GetOwnedCount(toy.Id) >= 3) continue;
                counts[toy.Id] = 3;
                changed = true;
            }
            if (changed) SynchronizeInventory();
            return changed;
        }

        /// <summary>Apply to a private candidate save; the session persists that candidate before publishing it.</summary>
        public bool TryFuse(FusedToy result)
        {
            if (!FusionCatalog.Valid(result) || Save.PendingRevealId.Length > 0 || Save.PendingFusionId.Length > 0
                || GetOwnedCount(result.Id) > 0 || (Save.Fusions != null && Save.Fusions.Exists(f => f != null && f.Id == result.Id))) return false;
            int needed = result.First == result.Second ? 3 : 2;
            if (GetOwnedCount(result.First) < needed || GetOwnedCount(result.Second) < needed) return false;
            ConsumeSpareProvenance(result.First); counts[result.First]--;
            ConsumeSpareProvenance(result.Second); counts[result.Second]--;
            counts[result.Id] = 1; MarkEarned(result.Id);
            if (Save.Fusions == null) Save.Fusions = new List<FusedToy>();
            Save.Fusions.Add(result);
            Save.PendingFusionId = result.Id;
            SynchronizeInventory();
            return true;
        }

        public bool FinishFusion(string id)
        {
            if (string.IsNullOrEmpty(id) || Save.PendingFusionId != id || GetOwnedCount(id) < 1) return false;
            Save.PendingFusionId = "";
            return true;
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
            SynchronizeBadgeFacts();
            Save.Counts = new int[4];
            for (int i = 0; i < 4; i++) Save.Counts[i] = GetOwnedCount(ToyCatalog.LegacyCollectibleId(i));
            Save.PendingReveal = GetOwnedCount(Save.PendingRevealId) > 0 && ToyCatalog.TryGetLegacyIndex(Save.PendingRevealId, out int index) ? index : -1;
        }

        public static int UtcDay(DateTime utc) => (int)(utc.Date - new DateTime(1970, 1, 1)).TotalDays;
    }
}
