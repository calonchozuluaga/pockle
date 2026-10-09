using System;
using System.Collections.Generic;
using System.Linq;
using Pockle.Core;

internal static class SeriesMotionChecks
{
    private static int checks;
    public static void Run()
    {
        Metadata(); PoolValidation(); Secrets(); Motion(); Budget();
        Console.WriteLine("PASS: " + checks + " series/motion assertions (UTC schedules, immutable odds, secret rates, loop bounds, calm and visible budgets).");
    }

    private static void Metadata()
    {
        Assert(ToyCatalog.Collections.Count == 12, "Collections are not one per distinct character.");
        foreach (var series in ToyCatalog.Collections)
        {
            Assert(ToyCatalog.TryGetCollection(series.Id, out var found) && ReferenceEquals(found, series), "Collection lookup differs from presentation.");
            Assert(series.SeriesNumber == (series.CharacterId == "pip" ? 1 : 0) && series.ReleaseDateUtc == null && series.RetireDateUtc == null, "Pip Series 1 was not preserved, or a date/other public series number was invented.");
            Assert(!series.HasPublishedSchedule && !series.IsReleasedAt(Utc(2026, 10, 9)), "Unscheduled prototype masqueraded as a public release.");
            Assert(!series.HasSecret && series.Odds.All(o => !o.IsSecret), "Existing regular figures acquired secret status.");
            Assert(series.Members.Count >= 10 && series.Members.All(id => ToyCatalog.TryGetCollectible(id, out var item) && item.CharacterId == series.CharacterId && item.CollectionId == series.Id), "Collection lacks ten varieties or mixes distinct characters.");
            Assert(!series.HasApprovedOdds && series.Pool == null && series.Odds.Count == 0, "A draft collection invented full-series draw odds.");
            Assert(series.ProductId.Length == 0 && series.BoxPriceTier.Length == 0, "Unapproved full-collection pricing was invented.");
            foreach (var odds in series.Odds)
                Assert(series.TryGetOdds(odds.CollectibleId, out var item) && ReferenceEquals(odds, item), "Odds lookup disagrees with lineup order.");
            Assert(!series.TryGetOdds("future.toy", out _) && !series.TryGetOdds(null, out _), "Unknown identity received display odds.");
        }
        Assert(!ToyCatalog.TryGetCollection(null, out _) && !ToyCatalog.TryGetCollection("jelly-garden", out _), "Legacy variant theme was treated as a character collection.");
        Assert(ToyCatalog.Collectibles.All(c => !c.IsSecret), "A prototype collectible was silently made secret.");
        Assert(ToyCatalog.TryGetCollection("pip", out var pip) && pip.SeriesNumber == 1 &&
            pip.Members.Contains(ToyCatalog.PeachId) && pip.Members.Contains(ToyCatalog.MoonId) && pip.Members.Contains(ToyCatalog.GoldId),
            "Existing Pip finishes did not join one Series 1 collection.");
        Assert(ToyCatalog.BoxPools.All(pool => ToyCatalog.TryGetCollectionForPool(pool.Id, out var owner) && owner.Id == "pip"),
            "Legacy Pip offers became separate character collections.");
        Reject(() => new CollectionDefinition("mixed", "Mixed", "pip", new[] { ToyCatalog.PeachId, "nook.mochi-foam" }),
            "A collection accepted a different character.");
        Reject(() => new CollectionDefinition("duplicate", "Duplicate", "pip", new[] { ToyCatalog.PeachId, ToyCatalog.PeachId }),
            "Duplicate membership padded the variety count.");
        var memberCopy = new[] { ToyCatalog.PeachId, ToyCatalog.MintId };
        var draft = new CollectionDefinition("draft", "Draft", "pip", memberCopy);
        memberCopy[0] = ToyCatalog.GoldId;
        Assert(draft.Members[0] == ToyCatalog.PeachId, "Caller array mutated the collection lineup.");
        Reject(() => new CollectionDefinition("partial", "Partial", "pip", new[] { ToyCatalog.PeachId, ToyCatalog.MintId },
            pool: new BoxPoolDefinition("partial.pool", "Partial pool", new BoxPoolEntry(ToyCatalog.PeachId, 1))),
            "Approved collection odds silently excluded one of its varieties.");
        var schedule = FixtureCollection(ToyCatalog.BoxPools[0], 1, Utc(2027, 1, 1));
        Assert(!schedule.IsReleasedAt(Utc(2026, 12, 31)) && schedule.IsReleasedAt(Utc(2027, 1, 1)), "Release boundary is not inclusive.");
        Assert(schedule.IsReleasedAt(Utc(2099, 1, 1)), "Permanent collection expired unexpectedly.");
        var limitedFixture = FixtureCollection(ToyCatalog.BoxPools[0], 1, Utc(2027, 1, 1), Utc(2027, 2, 1));
        Assert(limitedFixture.IsReleasedAt(Utc(2027, 1, 31)) && !limitedFixture.IsReleasedAt(Utc(2027, 2, 1)), "Optional retirement schema boundary is not exclusive.");
        Reject(() => FixtureCollection(schedule.Pool!, -1), "Negative series number accepted.");
        Reject(() => FixtureCollection(schedule.Pool!, releaseDateUtc: Utc(2027, 1, 1)), "Scheduled release has no public number.");
        Reject(() => FixtureCollection(schedule.Pool!, 1, DateTime.SpecifyKind(Utc(2027, 1, 1), DateTimeKind.Unspecified)), "Ambiguous release timezone accepted.");
        Reject(() => FixtureCollection(schedule.Pool!, 1, Utc(2027, 1, 1).AddHours(1)), "Non-date release time accepted.");
        Reject(() => FixtureCollection(schedule.Pool!, 1, retireDateUtc: Utc(2027, 2, 1)), "Retirement without a release accepted.");
        Reject(() => FixtureCollection(schedule.Pool!, 1, Utc(2027, 1, 1), Utc(2027, 1, 1)), "Empty release interval accepted.");
        Reject(() => schedule.IsReleasedAt(DateTime.SpecifyKind(Utc(2027, 1, 1), DateTimeKind.Local)), "Display scheduling accepted a local clock.");
        RejectMutation(() => ((IList<string>)schedule.Members)[0] = "future.toy");
        RejectMutation(() => ((IList<CollectionOdds>)schedule.Odds).Clear());
        Assert(schedule.Members[0] == ToyCatalog.PeachId && schedule.Odds.Count == 2 && Math.Abs(schedule.Odds.Sum(o => o.Probability) - 1d) < 1e-12, "Lineup mutation changed reward odds.");
    }

    private static void PoolValidation()
    {
        Reject(() => new BoxPoolEntry("invalid id", 1), "Invalid prize identity accepted.");
        Reject(() => new BoxPoolEntry("valid.id", 0), "Zero weight accepted.");
        Reject(() => new BoxPoolEntry("valid.id", -1), "Negative weight accepted.");
        Reject(() => new BoxPoolDefinition("pool", "Pool"), "Empty draw pool accepted.");
        Reject(() => new BoxPoolDefinition("pool", "Pool", new BoxPoolEntry("a", 1), new BoxPoolEntry("a", 2)), "Duplicate identity double-counted odds.");
        Reject(() => new BoxPoolDefinition("pool", "Pool", new BoxPoolEntry[] { null! }), "Null prize entry accepted.");
        var entries = new[] { new BoxPoolEntry("a", int.MaxValue), new BoxPoolEntry("b", int.MaxValue) };
        var pool = new BoxPoolDefinition("fixture", "Fixture", entries);
        entries[0] = new BoxPoolEntry("changed", 1);
        Assert(pool.TotalWeight == 2L * int.MaxValue && pool.Entries[0].CollectibleId == "a", "Pool aliased caller array or overflowed total weight.");
        Assert(pool.TryChoose(.49999f, out var a) && a == "a" && pool.TryChoose(.5f, out var b) && b == "b", "Large pool changed cumulative draw boundaries.");
        Assert(pool.TryChoose(1, out var last) && last == "b", "Inclusive RNG endpoint stopped selecting the final outcome.");
        foreach (float invalid in new[] { float.NaN, float.PositiveInfinity, -1f, 1.1f })
            Assert(!pool.TryChoose(invalid, out _), "Malformed roll awarded a figure.");
    }

    private static void Secrets()
    {
        // Fixture only: no new secret identity or 1-in-72 pool is published into the live beta catalog.
        var pool = new BoxPoolDefinition("fixture.series", "Fixture series",
            new BoxPoolEntry("fixture.regular", 71), new BoxPoolEntry("fixture.secret", 1));
        var series = FixtureCollection(pool, 7, Utc(2027, 7, 1), secretId: "fixture.secret");
        Assert(series.HasSecret && series.TryGetOdds(series.SecretId, out var secret) && secret.IsSecret &&
            Math.Abs(secret.Probability - 1d / 72d) < 1e-12, "Secret disclosure differs from approved base rate.");
        Assert(series.Odds.Count(o => o.IsSecret) == 1, "More than one secret slot appeared.");
        int secretDraws = 0;
        for (int i = 0; i < 72000; i++)
        {
            Assert(pool.TryChoose((i + .5f) / 72000f, out var id), "Valid draw failed.");
            if (id == series.SecretId) secretDraws++;
        }
        Assert(secretDraws == 1000, "Weighted draws do not match displayed 1-in-72 odds.");
        Reject(() => FixtureCollection(pool, secretId: "fixture.unknown"), "A secret outside the pool received misleading odds.");
    }

    private static void Motion()
    {
        Assert(CharacterMotion.Profiles.Count == ToyCatalog.Characters.Count, "Some roster characters lack motion recipes.");
        var signatures = new HashSet<string>();
        foreach (var character in ToyCatalog.Characters)
        {
            Assert(CharacterMotion.TryGetProfile(character.IdleProfileId, out var profile), "Missing character idle profile.");
            Assert(profile.IdleDurationSeconds > 0 && profile.EagerDurationSeconds > 0, "Invalid clip duration.");
            foreach (bool eager in new[] { false, true })
            {
                double period = eager ? profile.EagerDurationSeconds : profile.IdleDurationSeconds;
                Neutral(CharacterMotion.Sample(profile.Id, 0, eager));
                Neutral(CharacterMotion.Sample(profile.Id, period, eager));
                Neutral(CharacterMotion.Sample(profile.Id, period * .5, eager, calm: true));
                for (int i = 0; i <= 240; i++)
                {
                    var pose = CharacterMotion.Sample(profile.Id, period * i / 240d, eager);
                    Bounded(pose);
                    var repeat = CharacterMotion.Sample(profile.Id, period * i / 240d + period, eager);
                    Near(pose.Height, repeat.Height, .000001f); Near(pose.RollDegrees, repeat.RollDegrees, .00001f);
                    Near(pose.Compression, repeat.Compression, .000001f);
                }
                var beforeEnd = CharacterMotion.Sample(profile.Id, period - .00001, eager);
                Assert(Math.Abs(beforeEnd.RollDegrees) < .00001f && beforeEnd.Height < .00001f, "Motion loop snaps at its boundary.");
                if (eager) Neutral(CharacterMotion.Sample(profile.Id, period * .75, eager));
            }
            var sample = CharacterMotion.Sample(profile.Id, profile.IdleDurationSeconds * .25);
            signatures.Add(sample.PitchDegrees + "/" + sample.YawDegrees + "/" + sample.RollDegrees + "/" + sample.Compression);
            foreach (double bad in new[] { double.NaN, double.PositiveInfinity, -1d }) Neutral(CharacterMotion.Sample(profile.Id, bad));
            Bounded(CharacterMotion.Sample(profile.Id, double.MaxValue, true, phaseOffset: -.2f));
            Neutral(CharacterMotion.Sample(profile.Id, 1, phaseOffset: float.NaN));
            if (character.LeadFinishId.Contains("vinyl") || character.LeadFinishId == "coated-metallic")
                Assert(sample.Compression == 0 && sample.Stretch == 0, "Firm lead character received jelly breathing.");
        }
        Assert(signatures.Count == 12, "Distinct personalities collapsed into the same motion.");
        Neutral(CharacterMotion.Sample("unknown", 1)); Neutral(CharacterMotion.Sample(null, 1));
        // Warm up the static definitions and JIT before measuring the frame path.
        for (int i = 0; i < 1000; i++) CharacterMotion.Sample("pip", i / 60d);
        long start = GC.GetAllocatedBytesForCurrentThread();
        float value = 0;
        for (int i = 0; i < 10000; i++) value += CharacterMotion.Sample("pip", i / 60d).Compression;
        long allocated = GC.GetAllocatedBytesForCurrentThread() - start;
        Assert(allocated == 0 && value > 0, "Motion sampling allocated on the per-frame path.");
    }

    private static void Budget()
    {
        Reject(() => new LineupMotionBudget(0), "Zero motion budget accepted.");
        Reject(() => new LineupMotionBudget(7), "More than a handful of actors can animate.");
        var budget = new LineupMotionBudget(2);
        var first = budget.TryAcquire()!; var second = budget.TryAcquire()!;
        Assert(budget.ActiveCount == 2 && budget.TryAcquire() == null, "Visible actors exceeded the shared budget.");
        first.Dispose(); first.Dispose();
        Assert(budget.ActiveCount == 1, "Hidden actor cleanup was not idempotent.");
        var replacement = budget.TryAcquire()!;
        Assert(budget.ActiveCount == 2 && budget.TryAcquire() == null, "Freed slot could not serve a new visible toy.");
        second.Dispose(); replacement.Dispose();
        Assert(budget.ActiveCount == 0, "Lineup dismissal leaked a motion slot.");
    }

    private static void Bounded(CharacterIdlePose p)
    {
        foreach (float v in new[] { p.Height, p.PitchDegrees, p.YawDegrees, p.RollDegrees, p.Compression, p.Stretch })
            Assert(!float.IsNaN(v) && !float.IsInfinity(v), "Motion became nonfinite.");
        Assert(p.Height >= 0 && p.Height <= .12f && Math.Abs(p.YawDegrees) <= 9 && Math.Abs(p.RollDegrees) <= 4 &&
            Math.Abs(p.PitchDegrees) <= 4 && p.Compression >= 0 && p.Compression <= .04f && p.Stretch >= 0 && p.Stretch <= .013f,
            "Motion exceeded lineup bounds.");
    }
    private static void Neutral(CharacterIdlePose p)
    { Assert(p.Height == 0 && p.PitchDegrees == 0 && p.YawDegrees == 0 && p.RollDegrees == 0 && p.Compression == 0 && p.Stretch == 0, "Calm, rest, or invalid motion did not stay neutral."); }
    private static void Near(float a, float b, float tolerance) { Assert(Math.Abs(a - b) <= tolerance, "Looped sampling drifted."); }
    private static CollectionDefinition FixtureCollection(BoxPoolDefinition pool, int seriesNumber = 0,
        DateTime? releaseDateUtc = null, DateTime? retireDateUtc = null, string secretId = "")
    {
        string characterId = pool.Entries[0].CollectibleId.Split('.')[0];
        return new CollectionDefinition("fixture", "Fixture", characterId,
            pool.Entries.Select(e => e.CollectibleId).ToArray(), seriesNumber, releaseDateUtc, retireDateUtc, secretId, pool: pool);
    }
    private static DateTime Utc(int year, int month, int day) => new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
    private static void Reject(Action action, string message)
    { bool rejected = false; try { action(); } catch (ArgumentException) { rejected = true; } Assert(rejected, message); }
    private static void RejectMutation(Action action)
    { bool rejected = false; try { action(); } catch (NotSupportedException) { rejected = true; } Assert(rejected, "Presentation data mutated a shared definition."); }
    private static void Assert(bool value, string message) { checks++; if (!value) throw new InvalidOperationException(message); }
}
