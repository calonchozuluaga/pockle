using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Pockle.Core;

internal static class CatalogChecks
{
    private static int checks;
    private static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true };
    public static void Run()
    {
        Definitions(); Migration(); FutureIdentity(); ProfileReferences(); PreviewBudget();
        Console.WriteLine("PASS: " + checks + " catalog assertions (definitions, legacy migration, unknown IDs, profiles, bounded preview lifetimes).");
    }
    private static void Definitions()
    {
        Assert(ToyCatalog.Characters.Count == 12 && ToyCatalog.Finishes.Count == 10 && ToyCatalog.Collectibles.Count == 120, "Roster is not twelve distinct characters with ten finishes.");
        Assert(ToyCatalog.Collectibles.Select(c => c.Id).Distinct(StringComparer.Ordinal).Count() == 120, "Collectible IDs collide.");
        Assert(ToyCatalog.Collectibles.Count(c => c.Available) == 4, "Planned art was marked playable.");
        foreach (var definition in ToyCatalog.Collectibles)
        {
            Assert(ToyCatalog.IsValidId(definition.Id), "Catalog has an invalid persistence ID.");
            Assert(ToyCatalog.TryGetCollectible(definition.Id, out var found) && ReferenceEquals(found, definition), "Lookup disagrees with read-only catalog.");
            Assert(ToyCatalog.TryGetCharacter(definition.CharacterId, out var character), "Collectible references a missing character.");
            Assert(ToyCatalog.TryGetFinish(definition.FinishId, out _), "Collectible references a missing finish.");
            Assert(character.AccentColor.Red >= 0 && character.AccentColor.Red <= 1, "Accent RGB is outside UI color bounds.");
            Assert(ToyCatalog.TryGetCollection(definition.CollectionId, out var collection) && collection.CharacterId == definition.CharacterId, "Variety was placed outside its character collection.");
        }
        foreach (var character in ToyCatalog.Characters)
            Assert(ToyCatalog.Collectibles.Count(c => c.CharacterId == character.Id) == 10, "A character is merely a finish entry rather than its own silhouette.");
        string[] oldIds = { "pip.peach-jelly", "pip.moon-pearl", "pip.gold-confetti", "pip.mint-mochi" };
        for (int i = 0; i < oldIds.Length; i++)
        {
            Assert(ToyCatalog.LegacyCollectibleId(i) == oldIds[i], "Legacy save mapping changed.");
            Assert(ToyCatalog.TryGetLegacyIndex(oldIds[i], out int index) && index == i, "Legacy bridge is not reversible.");
        }
        Assert(!ToyCatalog.TryGetCollectible(null, out _) && !ToyCatalog.TryGetCollectible("PIP.PEACH-JELLY", out _), "ID lookup ignored canonical identity.");
        Assert(!ToyCatalog.TryGetLegacyIndex("moss.velvet-flock", out _) && !ToyCatalog.TryGetLegacyIndex("unknown.toy", out _), "An unknown toy fell back to a playable Pip.");
        Assert(ToyCatalog.IsDailyEligible(ToyCatalog.PeachId) && ToyCatalog.IsDailyEligible(ToyCatalog.MintId), "Current daily pool changed.");
        Assert(!ToyCatalog.IsDailyEligible(ToyCatalog.MoonId) && !ToyCatalog.IsDailyEligible("moss.velvet-flock"), "Expanded roster leaked into the daily pool.");
        Assert(ToyCatalog.TryGetBoxPool(ToyCatalog.DailyPoolId, out var pool), "Daily reward has no explicit ID pool.");
        Assert(pool.Entries.Count == 2 && pool.Entries.All(e => e.Weight == 50), "Daily odds changed during catalog migration.");
        foreach (float roll in new[] { 0f, .1f, .49999f, .5f, .8f, 1f })
        {
            Assert(pool.TryChoose(roll, out string chosen) && chosen == (roll < .5f ? ToyCatalog.PeachId : ToyCatalog.MintId), "Weighted ID pool changed boundary behavior.");
        }
        foreach (float invalid in new[] { float.NaN, float.PositiveInfinity, -1f, 1.001f })
            Assert(!pool.TryChoose(invalid, out _), "Invalid reward roll selected a collectible.");
        foreach (var box in ToyCatalog.BoxPools)
            Assert(ToyCatalog.TryGetCollectionForPool(box.Id, out var collection) && collection.Id == "pip" && box.Entries.All(e => ToyCatalog.TryGetCollectible(e.CollectibleId, out var item) && item.Available && item.CollectionId == collection.Id), "Legacy Pip sub-pool includes an unapproved/mismatched concept.");
        bool immutable = false;
        try { ((IList<CollectibleDefinition>)ToyCatalog.Collectibles).Clear(); } catch (NotSupportedException) { immutable = true; }
        Assert(immutable && ToyCatalog.Collectibles.Count == 120, "A HUD consumer mutated the shared catalog.");
    }
    private static void Migration()
    {
        const int day = 20734;
        for (int pending = -1; pending < 4; pending++)
        {
            var old = new CollectionSave { Version = 1, Counts = new[] { 0, 17, 3, 9 }, Day = day, Steps = 837,
                Claimed = true, SensorTotal = 4421, SensorUptime = 2500, SensorBoot = 3, PendingReveal = pending };
            // A version-1 payload may have no new fields at all, as JsonUtility will see during real upgrades.
            string raw = "{\"Version\":1,\"Counts\":[0,17,3,9],\"Day\":" + day + ",\"Steps\":837,\"Claimed\":true,\"SensorTotal\":4421,\"SensorUptime\":2500,\"SensorBoot\":3,\"PendingReveal\":" + pending + "}";
            var migrated = new CollectionProgress(JsonSerializer.Deserialize<CollectionSave>(raw, Json), day);
            Assert(migrated.Save.Version == 2 && migrated.Save.Inventory!.Count == 4, "Legacy schema was not migrated to ID inventory.");
            Assert(migrated.Save.Counts.SequenceEqual(old.Counts), "Migration changed counts or gave missing starter toys.");
            for (int i = 0; i < 4; i++) Assert(migrated.GetOwnedCount(ToyCatalog.LegacyCollectibleId(i)) == old.Counts[i], "Legacy ID received another toy's count.");
            Assert(migrated.Save.Steps == 837 && migrated.Save.Day == day && migrated.Save.Claimed, "Migration reset daily progress or eligibility.");
            Assert(migrated.Save.SensorTotal == 4421 && migrated.Save.SensorUptime == 2500 && migrated.Save.SensorBoot == 3, "Migration broke hardware tracking baseline.");
            Assert(migrated.Save.PendingRevealId == (pending < 0 ? "" : ToyCatalog.LegacyCollectibleId(pending)), "Interrupted reveal identity was lost.");
            Assert(migrated.Save.PendingReveal == (pending >= 0 && old.Counts[pending] > 0 ? pending : -1), "Legacy viewer attempted an unavailable/unowned reveal.");
            var restored = RoundTrip(migrated, day);
            Assert(restored.Save.Counts.SequenceEqual(old.Counts) && restored.Save.PendingRevealId == migrated.Save.PendingRevealId, "Migration replay or restart changed ownership/pending state.");
            Assert(!restored.TryClaimDaily(day, ToyCatalog.PeachId), "Migrated claimed box could be awarded again.");
        }
        var partial = new CollectionProgress(new CollectionSave { Counts = new[] { 7, 0 }, Day = day }, day);
        Assert(partial.Save.Counts.SequenceEqual(new[] { 7, 0, 0, 0 }), "Short/damaged arrays discarded recoverable counts or gifted extras.");
        var empty = new CollectionProgress(new CollectionSave { Counts = null!, Day = day }, day);
        Assert(empty.OwnedCounts.Values.Sum() == 0, "Missing old inventory created starter gifts.");
        var fresh = new CollectionProgress(null, day);
        Assert(fresh.OwnedCounts.Count == 4 && fresh.OwnedCounts.Values.Sum() == 4, "Fresh beta starter behavior changed or seeded all planned toys.");
        fresh.ObserveSteps(day, 0, 0, 1); fresh.ObserveSteps(day, 1000, 600, 1);
        Assert(!fresh.TryClaimDaily(day, "moss.velvet-flock") && !fresh.TryClaimDaily(day, "future.toy"), "Concept/unknown reward was granted.");
        Assert(fresh.TryClaimDaily(day, ToyCatalog.MintId), "Earned legacy daily reward did not work by ID.");
        Assert(fresh.GetOwnedCount(ToyCatalog.MintId) == 2 && fresh.Save.Counts[3] == 2, "Legacy HUD projection differs from authoritative inventory.");
        Assert(!fresh.FinishReveal(ToyCatalog.PeachId) && fresh.Save.PendingRevealId == ToyCatalog.MintId, "Wrong reveal acknowledgment cleared a pending toy.");
        Assert(fresh.FinishReveal(ToyCatalog.MintId) && !fresh.FinishReveal(ToyCatalog.MintId), "Reveal completion was not idempotent.");
        bool immutable = false;
        try { ((IDictionary<string, int>)fresh.OwnedCounts)["future.toy"] = 999; } catch (NotSupportedException) { immutable = true; }
        Assert(immutable && fresh.GetOwnedCount("future.toy") == 0, "HUD inventory view could grant ownership.");
    }
    private static void FutureIdentity()
    {
        const int day = 20734;
        var saved = new CollectionSave { Version = 2, Day = day, Steps = 1000, PendingRevealId = "future.character.finish",
            Counts = new[] { 9999, 9999, 9999, 9999 }, Inventory = new List<OwnedCollectible> {
                new OwnedCollectible("future.character.finish", 8), new OwnedCollectible(ToyCatalog.MoonId, 5),
                new OwnedCollectible(ToyCatalog.MoonId, 3), new OwnedCollectible("moss.velvet-flock", 0),
                new OwnedCollectible("invalid id", 9999), new OwnedCollectible("invalid\n", 8), null! } };
        var progress = new CollectionProgress(saved, day);
        Assert(progress.GetOwnedCount("future.character.finish") == 8, "A valid future ID was discarded.");
        Assert(progress.GetOwnedCount(ToyCatalog.MoonId) == 5, "Duplicate serialized rows granted extra inventory.");
        Assert(progress.Save.Counts.SequenceEqual(new[] { 0, 5, 0, 0 }), "Version-2 legacy projection overwrote ID inventory.");
        Assert(!progress.CanClaim && !progress.TryClaimDaily(day, ToyCatalog.PeachId), "A future pending reveal was overwritten by a new claim.");
        for (int i = 0; i < 20; i++)
        {
            progress = RoundTrip(progress, day);
            Assert(progress.GetOwnedCount("future.character.finish") == 8 && progress.Save.PendingRevealId == "future.character.finish", "Unknown IDs did not round-trip across client saves.");
            Assert(progress.Save.Inventory!.Count == 3, "Normalization dropped legitimate zero entries or retained invalid rows.");
        }
        progress.AdvanceDay(day + 1);
        Assert(progress.Save.PendingRevealId == "future.character.finish" && progress.GetOwnedCount("future.character.finish") == 8, "Day rollover erased future ownership/reveal.");
        var rng = new Random(29);
        for (int iteration = 0; iteration < 200; iteration++)
        {
            int[] amounts = Enumerable.Range(0, 4).Select(_ => rng.Next(0, 9999)).ToArray();
            int pending = rng.Next(-1, 4);
            var migrated = new CollectionProgress(new CollectionSave { Counts = amounts, Day = day, PendingReveal = pending }, day);
            migrated.Save.Inventory!.Reverse();
            migrated = RoundTrip(migrated, day);
            Assert(migrated.Save.Counts.SequenceEqual(amounts), "Reordered inventory changed index-to-ID identity.");
            Assert(migrated.Save.PendingRevealId == (pending < 0 ? "" : ToyCatalog.LegacyCollectibleId(pending)), "Reordered inventory changed reveal identity.");
        }
        bool rejected = false;
        try { _ = new CollectionProgress(new CollectionSave { Version = 3 }, day); } catch (ArgumentException) { rejected = true; }
        Assert(rejected, "Unknown schemas were silently migrated as current saves.");
    }
    private static void ProfileReferences()
    {
        for (int i = 0; i < 4; i++)
            Assert(ProfileSelection.MigrateId(null, i) == ToyCatalog.LegacyCollectibleId(i), "Legacy avatar/favorite was assigned another finish.");
        Assert(ProfileSelection.MigrateId("future.avatar.finish", 2) == "future.avatar.finish", "Future profile reference was overwritten by its compatibility index.");
        Assert(ProfileSelection.MigrateId("", 999) == ToyCatalog.PeachId && ProfileSelection.MigrateId("bad id", 3) == ToyCatalog.MintId, "Invalid profile references broke legacy fallback.");
        Assert(ProfileSelection.CanSelect(ToyCatalog.MoonId) && !ProfileSelection.CanSelect("moss.velvet-flock") && !ProfileSelection.CanSelect("future.avatar.finish"), "Missing/planned profile art was offered as playable.");
        Assert(ProfileSelection.LegacyIndex(ToyCatalog.GoldId) == 2, "Old profile UI cannot resolve migrated Gold.");
    }
    private static void PreviewBudget()
    {
        int live = 0, peak = 0, builds = 0, destroyed = 0;
        var cache = new BoundedLeaseCache<string>(2, id => { builds++; if (id == "missing") return null; peak = Math.Max(peak, ++live); return id; }, _ => { live--; destroyed++; });
        var a = cache.TryAcquire("a")!; var aAgain = cache.TryAcquire("a")!;
        Assert(builds == 1 && a.Value == "a" && aAgain.Value == "a", "Duplicate visible items created duplicate portraits.");
        var b = cache.TryAcquire("b")!;
        Assert(cache.TryAcquire("c") == null && builds == 2 && live == 2, "Pinned visible items exceeded budget or were evicted.");
        a.Dispose();
        Assert(cache.TryAcquire("c") == null, "A portrait was evicted while another image still held it.");
        aAgain.Dispose(); var c = cache.TryAcquire("c")!;
        Assert(destroyed == 1 && c.Value == "c" && b.Value == "b" && live == 2 && peak == 2, "Eviction broke visible textures or exceeded capacity during allocation.");
        cache.Dispose();
        Assert(live == 2 && b.Value == "b", "Cache disposal invalidated still-visible images.");
        b.Dispose(); c.Dispose(); c.Dispose(); cache.Dispose();
        Assert(live == 0 && destroyed == 3 && cache.CachedCount == 0, "Portrait cleanup leaked or double-destroyed resources.");
        bool rejected = false; try { cache.TryAcquire("a"); } catch (ObjectDisposedException) { rejected = true; }
        Assert(rejected, "A closed cache allocated more images.");
        rejected = false; try { _ = a.Value; } catch (ObjectDisposedException) { rejected = true; }
        Assert(rejected, "Released lease exposed a destroyed texture.");
        var evicted = new List<string>();
        var lru = new BoundedLeaseCache<string>(2, id => id == "missing" ? null : id, id => evicted.Add(id));
        lru.TryAcquire("old")!.Dispose(); lru.TryAcquire("new")!.Dispose(); lru.TryAcquire("old")!.Dispose();
        lru.TryAcquire("third")!.Dispose(); Assert(lru.CachedCount == 2 && evicted.SequenceEqual(new[] { "new" }), "Cache did not evict the least recently used unpinned portrait.");
        Assert(lru.TryAcquire("missing") == null && lru.CachedCount == 1, "Missing art was cached as a playable portrait."); lru.Dispose();
    }
    private static CollectionProgress RoundTrip(CollectionProgress progress, int day)
    { return new CollectionProgress(JsonSerializer.Deserialize<CollectionSave>(JsonSerializer.Serialize(progress.Save, Json), Json), day); }
    private static void Assert(bool value, string message)
    { checks++; if (!value) throw new InvalidOperationException(message); }
}
