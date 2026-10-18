using System;
using System.Linq;
using Pockle.Core;
internal static class FusionReviewSpareChecks
{
    public static void Run()
    {
        var progress = new CollectionProgress(null, 42);
        progress.Save.Steps = 571; progress.Save.Claimed = true; progress.Save.PendingRevealId = ToyCatalog.MoonId;
        var untouched = progress.OwnedCounts.ToDictionary(p => p.Key, p => p.Value);
        if (!progress.EnsureFusionReviewSpares()) throw new Exception("Missing review spares.");
        var targets = progress.OwnedCounts.Where(p => p.Value == 3).ToArray();
        if (targets.Length != 18 || targets.Select(p => ToyCatalog.Collectibles.Single(t => t.Id == p.Key).CollectionId).Distinct().Count() != 6)
            throw new Exception("Expected three finishes in six collections.");
        if (progress.EnsureFusionReviewSpares()) throw new Exception("Review minimum duplicated.");
        progress.Save.Inventory!.Single(p => p.Id == ToyCatalog.FoxId).Count = 7;
        var restarted = new CollectionProgress(progress.Save, 42);
        if (restarted.EnsureFusionReviewSpares() || restarted.GetOwnedCount(ToyCatalog.FoxId) != 7)
            throw new Exception("Grant reduced an existing larger stack or repeated after reload.");
        foreach (var pair in untouched.Where(p => !targets.Any(t => t.Key == p.Key)))
            if (restarted.GetOwnedCount(pair.Key) != pair.Value) throw new Exception("Unrelated inventory changed.");
        if (restarted.Save.Steps != 571 || !restarted.Save.Claimed || restarted.Save.PendingRevealId != ToyCatalog.MoonId)
            throw new Exception("Review spares changed reward/reveal state.");
        Console.WriteLine("PASS: fusion review minimums, six collections, 18 finishes, idempotency and preserved larger stacks/rewards.");
    }
}
