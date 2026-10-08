#nullable enable
using System;

namespace Pockle.Core
{
    [Serializable]
    public sealed class CollectionSave
    {
        public int Version = 1;
        public int[] Counts = new int[4];
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
        public bool CanClaim => Save.Steps >= DailyTarget && !Save.Claimed;

        public CollectionProgress(CollectionSave? saved, int day)
        {
            Save = saved ?? new CollectionSave { Counts = new[] { 1, 1, 1, 1 } };
            if (Save.Version != 1) throw new ArgumentException("Unsupported collection save version.");
            if (Save.Counts == null || Save.Counts.Length != 4) Save.Counts = new[] { 1, 1, 1, 1 };
            for (int i = 0; i < 4; i++) Save.Counts[i] = Math.Max(0, Math.Min(9999, Save.Counts[i]));
            Save.Steps = Math.Max(0, Math.Min(DailyTarget, Save.Steps));
            if (Save.PendingReveal < -1 || Save.PendingReveal > 3) Save.PendingReveal = -1;
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
            AdvanceDay(day);
            if (day != Save.Day || !CanClaim || variant != 0 && variant != 3 || Save.PendingReveal >= 0)
                return false;
            Save.Claimed = true;
            Save.Counts[variant] = Math.Min(9999, Save.Counts[variant] + 1);
            Save.PendingReveal = variant; // Save ownership before the animation so interruption loses nothing.
            return true;
        }

        public void FinishReveal() { Save.PendingReveal = -1; }
        public static int UtcDay(DateTime utc) => (int)(utc.Date - new DateTime(1970, 1, 1)).TotalDays;
    }
}
