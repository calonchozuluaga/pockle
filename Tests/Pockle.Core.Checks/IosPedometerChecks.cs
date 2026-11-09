using System;
using System.Text.Json;
using Pockle.Core;
internal static class IosPedometerChecks
{
    private static void Require(bool ok, string why) { if (!ok) throw new InvalidOperationException(why); }
    public static void Run()
    {
        // Core Motion returns today's cumulative count, not a delta or a count since the app started.
        var p = new CollectionProgress(null, 1000);
        p.ObserveSteps(1000, 500, 100, 12345);
        Require(p.DailyWalkingSteps == 0, "First authorized snapshot must not award earlier steps.");
        p.ObserveSteps(1000, 510, 110, 12345); p.ObserveSteps(1000, 510, 111, 12345);
        Require(p.DailyWalkingSteps == 10, "Repeated native snapshots must not count twice.");
        var options = new JsonSerializerOptions { IncludeFields = true };
        var saved = JsonSerializer.Deserialize<CollectionSave>(JsonSerializer.Serialize(p.Save, options), options);
        p = new CollectionProgress(saved, 1000);
        p.ObserveSteps(1000, 620, 210, 12345);
        Require(p.DailyWalkingSteps == 120, "Same-day history after reopening must preserve baseline and add only new steps.");
        p.ObserveSteps(1000, 630, 1, 12346);
        Require(p.DailyWalkingSteps == 120, "Reboot changes must reset baseline, not award an interval.");
        p.ObserveSteps(1000, 640, 11, 12346);
        Require(p.DailyWalkingSteps == 130, "New steps after reboot should resume normally.");
        p.ObserveSteps(1001, 2, 40, 12346);
        Require(p.DailyWalkingSteps == 0, "Midnight must not assign the cross-day interval.");
        p.ObserveSteps(1001, 7, 45, 12346);
        Require(p.DailyWalkingSteps == 5, "Daily counter reset must count new steps once.");
        p.ObserveSteps(1001, 9999, double.NaN, 12346);
        Require(p.DailyWalkingSteps == 5 && p.Save.SensorTotal == 7, "Invalid native snapshots must not alter the baseline.");
        // Local midnight can happen within the same UTC reward day. Today's first reading
        // may already exceed yesterday's last reading after the phone was offline overnight.
        p.ObserveSteps(1001, 1500, 36450, 13786);
        Require(p.DailyWalkingSteps == 5 && p.Save.SensorTotal == 1500,
            "A changed Core Motion query window must baseline even when its total increased and UTC day did not change.");
        p.ObserveSteps(1001, 1510, 36460, 13786);
        Require(p.DailyWalkingSteps == 15, "Only new steps within the new local-day query window count.");
        // Moving time zones changes the start-of-day instant without necessarily changing either date.
        p.ObserveSteps(1001, 2000, 37060, 13726);
        Require(p.DailyWalkingSteps == 15, "Time-zone query-window changes must also reset the baseline.");
        p.ObserveSteps(1001, 2005, 37065, 13726);
        Require(p.DailyWalkingSteps == 20, "Counting resumes after a time-zone query-window reset.");
        Console.WriteLine("PASS: iOS cumulative pedometer baseline, duplicate snapshots, save/reopen history, reboot, local/UTC midnight, time-zone query windows and malformed snapshots.");
    }
}
