#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
namespace Pockle.Core
{
    [Serializable]
    public sealed class BadgeRuntimeSave
    {
        public long LifetimeSteps;
        public int DailyWalkingSteps, BestDailySteps, HistoryStartDay;
        public List<int> WalkingDays = new List<int>();
        public List<int> WalkingBoxDays = new List<int>();
        public List<OwnedCollectible> EarnedCopies = new List<OwnedCollectible>();
        public string EarnedBadges = "";
    }

    public sealed partial class CollectionProgress
    {
        private readonly Dictionary<string, int> earnedCopies = new Dictionary<string, int>(StringComparer.Ordinal);
        public IReadOnlyDictionary<string, int> BadgeEligibleOwnedCounts { get; private set; } = null!;
        public long LifetimeSteps => Save.BadgeState!.LifetimeSteps;
        public int DailyWalkingSteps => Save.BadgeState!.DailyWalkingSteps;
        public int BestDailySteps => Save.BadgeState!.BestDailySteps;
        public IReadOnlyList<int> WalkingDays => Save.BadgeState!.WalkingDays.AsReadOnly();
        public IReadOnlyList<int> WalkingBoxDays => Save.BadgeState!.WalkingBoxDays.AsReadOnly();
        public int LongestWalkingStreak => LongestStreak(Save.BadgeState!.WalkingDays);
        public int LongestWalkingBoxStreak => LongestStreak(Save.BadgeState!.WalkingBoxDays);

        private void InitializeBadgeFacts(int today)
        {
            bool migrated = Save.BadgeState == null;
            if (migrated)
            {
                // Only the old day's capped validated progress is known; never invent the missing past.
                Save.BadgeState = new BadgeRuntimeSave { LifetimeSteps = Save.Steps, DailyWalkingSteps = Save.Steps,
                    BestDailySteps = Save.Steps, HistoryStartDay = Save.Day > 0 ? Save.Day : today };
                if (Save.Steps > 0) Save.BadgeState.WalkingDays.Add(Save.Day);
                if (Save.Claimed) Save.BadgeState.WalkingBoxDays.Add(Save.Day);
                // A v3 receipt proves an action; fixed study/test grants have no such receipt.
                if (Save.Fusions != null) foreach (var fused in Save.Fusions)
                    if (FusionCatalog.Valid(fused) && GetOwnedCount(fused.Id) > 0)
                        earnedCopies[fused.Id] = 1;
            }
            var state = Save.BadgeState!;
            state.DailyWalkingSteps = Math.Max(Save.Steps, state.DailyWalkingSteps);
            state.BestDailySteps = Math.Max(state.DailyWalkingSteps, state.BestDailySteps);
            state.LifetimeSteps = Math.Max(state.DailyWalkingSteps, state.LifetimeSteps);
            state.WalkingDays = NormalizeDays(state.WalkingDays);
            state.WalkingBoxDays = NormalizeDays(state.WalkingBoxDays);
            if (state.EarnedCopies != null) foreach (var entry in state.EarnedCopies)
                if (entry != null && ToyCatalog.IsValidId(entry.Id))
                    earnedCopies[entry.Id] = Math.Max(earnedCopies.TryGetValue(entry.Id, out int old) ? old : 0,
                        Math.Min(GetOwnedCount(entry.Id), BoundedCount(entry.Count)));
            BadgeEligibleOwnedCounts = new ReadOnlyDictionary<string, int>(earnedCopies);
            state.EarnedBadges = BadgeSaveLedger.Merge(state.EarnedBadges, "");
        }

        private void SynchronizeBadgeFacts()
        {
            if (Save.BadgeState == null) return;
            var ids = new List<string>(earnedCopies.Keys); ids.Sort(StringComparer.Ordinal);
            Save.BadgeState.EarnedCopies = new List<OwnedCollectible>();
            foreach (string id in ids)
            {
                earnedCopies[id] = Math.Min(GetOwnedCount(id), earnedCopies[id]);
                if (earnedCopies[id] > 0) Save.BadgeState.EarnedCopies.Add(new OwnedCollectible(id, earnedCopies[id]));
            }
        }
        private void MarkEarned(string id)
        { earnedCopies[id] = Math.Min(GetOwnedCount(id), (earnedCopies.TryGetValue(id, out int count) ? count : 0) + 1); }
        private void ConsumeSpareProvenance(string id)
        {
            int earned = earnedCopies.TryGetValue(id, out int count) ? count : 0;
            if (GetOwnedCount(id) <= earned && earned > 0) earnedCopies[id] = earned - 1;
        }
        private void RecordWalking(long delta)
        {
            var state = Save.BadgeState!;
            state.DailyWalkingSteps = delta >= int.MaxValue - state.DailyWalkingSteps ? int.MaxValue : state.DailyWalkingSteps + (int)delta;
            state.BestDailySteps = Math.Max(state.BestDailySteps, state.DailyWalkingSteps);
            state.LifetimeSteps = state.LifetimeSteps > long.MaxValue - delta ? long.MaxValue : state.LifetimeSteps + delta;
            if (!state.WalkingDays.Contains(Save.Day)) state.WalkingDays.Add(Save.Day);
        }
        private static List<int> NormalizeDays(List<int>? days)
        {
            var sorted = new SortedSet<int>();
            if (days != null) foreach (int day in days) if (day >= 0 && day <= 2932896) sorted.Add(day);
            return new List<int>(sorted);
        }
        private static int LongestStreak(List<int> days)
        {
            int best = 0, current = 0, last = int.MinValue;
            foreach (int day in days) { current = (long)day - last == 1 ? current + 1 : 1; best = Math.Max(best, current); last = day; }
            return best;
        }
    }

    /// <summary>Stable union of local badge dates; unknown valid IDs survive for a compatible future cabinet.</summary>
    public static class BadgeSaveLedger
    {
        public static string Merge(string saved, string local)
        {
            var dates = new Dictionary<string, int>(StringComparer.Ordinal); var order = new List<string>();
            foreach (string data in new[] { saved ?? "", local ?? "" }) foreach (string part in data.Split(','))
            {
                int split = part.IndexOf('=');
                if (split <= 0) continue;
                string id = part.Substring(0, split);
                if (!ToyCatalog.IsValidId(id) || !int.TryParse(part.Substring(split + 1), NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out int day) || day < 0 || day > 2932896) continue;
                if (!dates.ContainsKey(id)) { order.Add(id); dates[id] = day; }
                else dates[id] = Math.Min(dates[id], day);
            }
            var parts = new List<string>();
            foreach (string id in order) parts.Add(id + "=" + dates[id].ToString(CultureInfo.InvariantCulture));
            return string.Join(",", parts);
        }
    }
}
