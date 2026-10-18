using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Pockle.Core;
internal static class BadgeRuntimeChecks
{
    private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true };
    private static void Check(bool value, string message) { if (!value) throw new Exception(message); }
    private static CollectionProgress Reload(CollectionProgress progress) => new CollectionProgress(JsonSerializer.Deserialize<CollectionSave>(JsonSerializer.Serialize(progress.Save, Json), Json), progress.Save.Day);
    public static void Run()
    {
        var p = new CollectionProgress(null, 1000);
        p.ObserveSteps(1000, 100, 0, 1); p.ObserveSteps(1000, 1100, 250, 1); p.ObserveSteps(1000, 10100, 2500, 1);
        Check(p.Save.Steps == 1000 && p.DailyWalkingSteps == 10000 && p.LifetimeSteps == 10000 && p.BestDailySteps == 10000, "Uncapped walking history changed reward cap or lost a 10k day.");
        p.ObserveSteps(1000, 10100, 2500, 1); p.ObserveSteps(1000, 20100, 2501, 1);
        Check(p.LifetimeSteps == 10000, "Duplicate/implausible sensor batch counted.");
        p.ObserveSteps(1000, 20200, 2526, 1); p = Reload(p);
        p.ObserveSteps(1000, 20, 0, 2); p.ObserveSteps(1000, 70, 13, 2);
        Check(p.LifetimeSteps == 10150 && p.DailyWalkingSteps == 10150, "Restart/reboot repeated or lost a batch.");
        p.ObserveSteps(1001, 170, 40, 2);
        Check(p.DailyWalkingSteps == 0 && p.LifetimeSteps == 10150, "Midnight assigned an unknown cross-day batch.");
        p.ObserveSteps(1001, 220, 53, 2); p.ObserveSteps(1000, 240, 60, 2);
        Check(p.LifetimeSteps == 10200 && p.WalkingDays.SequenceEqual(new[] {1000,1001}), "Clock rollback or walking-day uniqueness failed.");
        Check(p.LongestWalkingStreak == 2 && p.BestDailySteps == 10150, "Best day/streak lost.");
        p = new CollectionProgress(null, 2000); p.EnsureFusionReviewSpares(); p.EnsureWinterTrioTestCopies();
        Check(!p.BadgeEligibleOwnedCounts.Any(v => v.Value > 0), "Starter/test gifts earned collection achievements.");
        p.ObserveSteps(2000, 0, 0, 1); p.ObserveSteps(2000, 1000, 250, 1);
        Check(p.TryClaimDaily(2000, ToyCatalog.PeachId), "Walking reward missing."); p.FinishReveal();
        Check(p.BadgeEligibleOwnedCounts[ToyCatalog.PeachId] == 1, "Walking reward provenance missing.");
        var result = new FusedToy { Id = "fusion.provenance", First = ToyCatalog.PeachId, Second = "pip.gloss-vinyl", Name = "Provenance fusion", Temperament = 7 };
        Check(p.TryFuse(result), "Provenance fusion rejected.");
        Check(p.BadgeEligibleOwnedCounts[ToyCatalog.PeachId] == 1 && p.BadgeEligibleOwnedCounts[result.Id] == 1, "Fusion consumed earned copies before test extras or lost result origin.");
        p.FinishFusion(result.Id); p = Reload(p);
        Check(p.BadgeEligibleOwnedCounts[ToyCatalog.PeachId] == 1 && p.FusionsMade == 1, "Provenance did not survive save.");
        for (int day = 2001; day < 2030; day++)
        {
            p.ObserveSteps(day, 0, 0, day); p.ObserveSteps(day, 1000, 250, day);
            Check(p.TryClaimDaily(day, ToyCatalog.PeachId), "Daily history claim failed."); p.FinishReveal();
        }
        Check(p.LongestWalkingBoxStreak == 30 && p.LongestWalkingStreak == 30 && p.WalkingBoxDays.Count == 30, "Thirty-day history repeated or missed dates.");
        p.AdvanceDay(2032); Check(p.LongestWalkingBoxStreak == 30, "Historical achievement was lost after a gap.");
        var legacy = new CollectionProgress(new CollectionSave { Version=3,Day=500,Steps=1000,Claimed=true,
            Inventory = new List<OwnedCollectible> { new OwnedCollectible(ToyCatalog.PeachId, 9) } }, 500);
        Check(legacy.LifetimeSteps == 1000 && legacy.WalkingDays.Count == 1 && legacy.LongestWalkingBoxStreak == 1, "Migration invented or lost known lower-bound history.");
        Check(!legacy.BadgeEligibleOwnedCounts.Any(v => v.Value > 0) && legacy.GetOwnedCount(ToyCatalog.PeachId) == 9, "Migration guessed old gift provenance or lost inventory.");
        legacy.Save.BadgeState!.LifetimeSteps=long.MaxValue-1;legacy.Save.BadgeState!.DailyWalkingSteps=int.MaxValue-1;
        legacy.ObserveSteps(500,0,0,1);legacy.ObserveSteps(500,10,3,1);
        Check(legacy.LifetimeSteps==long.MaxValue && legacy.DailyWalkingSteps==int.MaxValue, "History counter overflow.");
        string merged=BadgeSaveLedger.Merge("first-fusion=200,future-badge=201", "first-fusion=205,secret-finder=202,bad=oops,negative=-1,overflow=2147483647");
        Check(merged=="first-fusion=200,future-badge=201,secret-finder=202", "Badge ledger lost dates, unknown IDs or accepted corruption.");
        Check(BadgeSaveLedger.Merge(merged,merged)==merged, "Badge merge was not idempotent.");
        FusionCatalog.Restore(null);
        Console.WriteLine("PASS: uncapped validated walking, reset/midnight/clock/restart, 30-day history, gift provenance, fusion origins, legacy lower bounds, overflow and badge snapshot union.");
    }
}
