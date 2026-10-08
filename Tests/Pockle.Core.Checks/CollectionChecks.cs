using System;
using System.Text.Json;
using Pockle.Core;

internal static class CollectionChecks
{
    private static int checks;
    public static void Run()
    {
        const int day = 20734;
        var progress = new CollectionProgress(null, day);
        Assert(progress.Save.Counts.Length == 4 && Array.TrueForAll(progress.Save.Counts, n => n == 1), "Starter collection did not seed four independent toys.");
        Assert(!progress.TryClaimDaily(day, 0), "An unearned walking box was granted.");
        progress.ObserveSteps(day, 5000, 100, 1);
        Assert(progress.Save.Steps == 0, "Historical steps were credited on first enable.");
        for (int i = 101; i < 160; i++) progress.ObserveSteps(day, 5000, i, 1);
        progress.ObserveSteps(day, 5100, 160, 1);
        Assert(progress.Save.Steps == 100, "Batched sensor updates lost their elapsed time.");
        progress.ObserveSteps(day, 5100, 160, 1);
        progress.ObserveSteps(day, 5100, 200, 1);
        Assert(progress.Save.Steps == 100, "A duplicate sample awarded steps.");
        progress.ObserveSteps(day, 6000, 201, 1);
        Assert(progress.Save.Steps == 100, "An implausible rapid batch was awarded.");
        progress.ObserveSteps(day, 6900, 700, 1);
        Assert(progress.Save.Steps == 1000 && progress.CanClaim, "Valid walking did not unlock the daily box.");
        Assert(!progress.TryClaimDaily(day, 1) && progress.Save.Counts[1] == 1, "Daily box granted a variant outside its pool.");
        Assert(progress.TryClaimDaily(day, 3), "Earned box was not claimable.");
        Assert(progress.Save.Counts[3] == 2 && progress.Save.Claimed && progress.Save.PendingReveal == 3, "Claim did not atomically preserve inventory and reveal.");
        for (int i = 0; i < 100; i++) Assert(!progress.TryClaimDaily(day, 0), "Repeated claim duplicated a reward.");

        var options = new JsonSerializerOptions { IncludeFields = true };
        string json = JsonSerializer.Serialize(progress.Save, options);
        var restored = new CollectionProgress(JsonSerializer.Deserialize<CollectionSave>(json, options), day);
        Assert(restored.Save.Counts[3] == 2 && restored.Save.PendingReveal == 3 && !restored.CanClaim, "Restart lost a claim or interrupted reveal.");
        restored.ObserveSteps(day, 6950, 760, 1);
        Assert(restored.Save.Steps == 1000, "Progress exceeded the cap.");
        restored.AdvanceDay(day - 1);
        Assert(restored.Save.Day == day && restored.Save.Claimed, "Clock rollback reset the claimed day.");
        Assert(!restored.TryClaimDaily(day - 1, 0), "Clock rollback granted another reward.");
        restored.AdvanceDay(day + 1);
        Assert(restored.Save.Steps == 0 && !restored.Save.Claimed && restored.Save.PendingReveal == 3, "New day lost pending ownership or retained yesterday's progress.");
        restored.ObserveSteps(day + 1, 10000, 1000, 1);
        Assert(restored.Save.Steps == 0, "An interval spanning midnight was credited.");
        restored.ObserveSteps(day + 1, 11000, 1500, 1);
        Assert(!restored.TryClaimDaily(day + 1, 0), "A pending reveal was overwritten by another reward.");
        restored.FinishReveal();
        Assert(restored.TryClaimDaily(day + 1, 0) && restored.Save.Counts[0] == 2, "Next day's box did not become available.");

        var reset = new CollectionProgress(null, day);
        reset.ObserveSteps(day, 1000, 100, 1);
        reset.ObserveSteps(day, 20, 200, 2);
        Assert(reset.Save.Steps == 0, "Reboot credited a hardware counter reset.");
        reset.ObserveSteps(day, 50, 220, 2);
        Assert(reset.Save.Steps == 30, "Walking after reboot did not resume.");
        reset.ObserveSteps(day, 100, 10, 2);
        Assert(reset.Save.Steps == 30, "Uptime rollback awarded steps.");
        reset.ObserveSteps(day, -1, 100, 2);
        reset.ObserveSteps(day, long.MaxValue, double.NaN, 2);
        reset.ObserveSteps(day, long.MaxValue, double.PositiveInfinity, 2);
        Assert(reset.Save.Steps == 30, "Invalid samples changed progress.");
        var corrupted = new CollectionProgress(new CollectionSave { Day = day, Steps = -99, Counts = new[] { -3, 99999, 1, 0 }, PendingReveal = 99 }, day);
        Assert(corrupted.Save.Steps == 0 && corrupted.Save.Counts[0] == 0 && corrupted.Save.Counts[1] == 9999 && corrupted.Save.PendingReveal == -1, "Damaged save values escaped their bounds.");
        Assert(CollectionProgress.UtcDay(new DateTime(1970, 1, 1, 23, 59, 0, DateTimeKind.Utc)) == 0, "UTC daily boundary is incorrect.");
        Console.WriteLine("PASS: " + checks + " collection assertions (walking batches, restart, day rollover, claim idempotency, inventory, pending reveals).");
    }
    private static void Assert(bool condition, string message)
    { checks++; if (!condition) throw new InvalidOperationException(message); }
}
