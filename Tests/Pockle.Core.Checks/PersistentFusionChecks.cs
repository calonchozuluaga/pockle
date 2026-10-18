using System;
using System.Linq;
using System.Text.Json;
using Pockle.Core;
internal static class PersistentFusionChecks
{
    private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true };
    private static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
    private static FusedToy Result(string first, string second, string suffix = "a") => new FusedToy {
        Id = "fusion.test-" + suffix, First = first, Second = second, Name = "Test fusion", Material = "Mixed", Personality = "Calm",
        Energy = 40, Mood = 70, Boldness = 45, Bedtime = 40, Temperament = 7 };
    public static void Run()
    {
        int pairs = 0;
        foreach (var collection in ToyCatalog.Collections.Where(c => c.CharacterId == c.Id))
        {
            var sources = collection.Members.Where(FusionCatalog.IsSource).ToArray();
            for (int i = 0; i < sources.Length; i++) for (int j = i; j < sources.Length; j++)
            {
                var progress = new CollectionProgress(new CollectionSave { Version = 2, Day = 42, Steps = 681,
                    Claimed = true, Inventory = sources.Select(id => new OwnedCollectible(id, 3)).ToList() }, 42);
                var record = Result(sources[i], sources[j]);
                Check(progress.TryFuse(record), "A valid same-character spare pair was rejected.");
                Check(progress.GetOwnedCount(sources[i]) == (i == j ? 1 : 2) && progress.GetOwnedCount(sources[j]) == (i == j ? 1 : 2), "Wrong parent consumption.");
                Check(progress.GetOwnedCount(record.Id) == 1 && progress.FusionsMade == 1 && progress.Save.Fusions!.Count == 1, "Missing single result/receipt.");
                Check(!progress.TryFuse(record), "Repeated commit consumed twice.");
                string json = JsonSerializer.Serialize(progress.Save, Json);
                progress = new CollectionProgress(JsonSerializer.Deserialize<CollectionSave>(json, Json), 43);
                Check(progress.GetOwnedCount(record.Id) == 1 && progress.Save.PendingFusionId == record.Id, "Interrupted reveal or day rollover lost fusion.");
                Check(FusionCatalog.TryGet(record.Id, out var saved) && saved.Personality == "Calm" && saved.Energy == 40, "Personality snapshot lost.");
                Check(ToyCatalog.TryGetCollectible(record.Id, out var toy) && toy.CharacterId == collection.CharacterId, "Fused definition body changed.");
                Check(!ToyCatalog.IsDailyEligible(record.Id) && !collection.Members.Contains(record.Id), "Fusion affected reward odds or finite completion.");
                Check(progress.FinishFusion(record.Id) && !progress.FinishFusion(record.Id), "Reveal acknowledgment was not idempotent.");
                Check(!progress.TryFuse(Result(record.Id, sources[i], "recursive")), "Recursive fusion must not use personal treasures.");
                pairs++;
            }
        }
        var bad = new CollectionProgress(null, 42); bad.EnsureFusionReviewSpares();
        var state = JsonSerializer.Serialize(bad.Save, Json);
        Check(!bad.TryFuse(Result(ToyCatalog.PeachId, ToyCatalog.FoxId)), "Cross-character fusion accepted.");
        Check(!bad.TryFuse(Result(ToyCatalog.MoonId, ToyCatalog.MoonId)), "Last copies consumed.");
        Check(!bad.TryFuse(Result("unknown.toy", ToyCatalog.PeachId)), "Unknown source accepted.");
        Check(JsonSerializer.Serialize(bad.Save, Json) == state, "Failed fusion changed the save.");
        var v1 = new CollectionProgress(new CollectionSave { Counts = new[] { 4, 7, 2, 9 }, Day = 42, Steps = 123 }, 42);
        Check(v1.Save.Version == CollectionProgress.SaveVersion && v1.GetOwnedCount(ToyCatalog.MoonId) == 7, "Legacy migration lost copies.");
        FusionCatalog.Restore(null);
        Console.WriteLine("PASS: " + pairs + " same-character fusion pairs, last-copy and recursive protection, receipts, restart, v1/v2 migration and fixed reward pools.");
    }
}
