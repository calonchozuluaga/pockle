using System;
using Pockle.Core;

internal static class MomentAudioChecks
{
    private static void Check(bool value, string message) { if (!value) throw new Exception(message); }
    private static WalkingMomentSnapshot S(string owner, int day, int earned, int credit, params string[] ids)
        => new WalkingMomentSnapshot(owner, day, earned, credit, ids);
    public static void Run()
    {
        var gate = new WalkingMomentGate();
        Check(gate.Observe(S("a", 50, 0, 990), true) == MomentCue.None, "Startup walking snapshot must be silent.");
        Check(gate.Observe(S("a", 50, 0, 1000), true) == MomentCue.Steps1000, "Qualifying server step boundary must signal once.");
        Check(gate.Observe(S("a", 50, 0, 1000), true) == MomentCue.None, "Refreshing steps must not repeat audio.");
        Check(gate.Observe(S("a", 50, 1, 0, "box1"), true) == MomentCue.WalkingBoxReady, "Banked box edge is separate from already celebrated steps.");
        Check(gate.Observe(S("a", 50, 2, 0, "box1", "box2"), true) == MomentCue.WalkingBoxReady, "Box ready must replace simultaneous step milestone.");
        Check(gate.Observe(S("a", 50, 2, 0, "box2"), true) == MomentCue.None, "Opening a box must not celebrate the other saved box again.");
        Check(gate.Observe(S("a", 50, 2, 0, "box1", "box2"), true) == MomentCue.None, "Old bank snapshot must not rearm a box cue.");
        Check(gate.Observe(S("a", 50, 1, 0), true) == MomentCue.None, "Regressed counts must stay silent.");
        Check(gate.Observe(S("a", 50, 2, 0), true) == MomentCue.None, "Returning to high-water mark must not repeat milestone.");
        Check(gate.Observe(S("a", 51, 0, 5000), true) == MomentCue.None, "UTC rollover must baseline carried credit.");
        Check(gate.Observe(S("a", 51, 0, 6000), false) == MomentCue.None, "Muted/background progress must be consumed silently.");
        Check(gate.Observe(S("a", 51, 0, 6000), true) == MomentCue.None, "Unmuting cannot replay an old milestone.");
        gate.Rebaseline();
        Check(gate.Observe(S("a", 51, 1, 6000, "box3"), true) == MomentCue.None, "Returning foreground must not autoplay caught-up rewards.");
        Check(gate.Observe(S("b", 51, 1, 6000, "otherBox"), true) == MomentCue.None, "Account switches must not autoplay another player's progress.");
        Check(gate.Observe(S("b", 51, 9, 6000, "invalidBox"), true) == MomentCue.None, "Invalid snapshots cannot produce cues.");

        var queue = new MomentCueQueue();
        Check(queue.Offer(MomentCue.Steps1000, 1), "Step cue should queue.");
        Check(queue.Offer(MomentCue.WalkingBoxReady, 1), "Ready cue should coalesce a pending step cue.");
        Check(!queue.Take(1, true, true, out _), "Busy voice/effect must retain its full tail.");
        Check(queue.Take(2, false, true, out var cue) && cue == MomentCue.WalkingBoxReady, "Only ready cue should play for simultaneous walking events.");
        Check(!queue.Take(2, false, true, out _), "One queued event cannot play twice.");
        queue.Offer(MomentCue.BoxReveal, 3); queue.Offer(MomentCue.SpecialReveal, 3);
        Check(!queue.Offer(MomentCue.PurchaseSaved, 3), "Save notification cannot replace a special reveal.");
        Check(queue.Take(3, false, true, out cue) && cue == MomentCue.SpecialReveal, "Special must replace standard, not stack.");
        queue.Offer(MomentCue.Steps1000, 4);
        Check(!queue.Take(4, false, false, out _) && !queue.HasPending, "Modal/reveal suppresses passive cues without stale queue.");
        queue.Offer(MomentCue.BoxReveal, 5);
        Check(!queue.Take(10, false, true, out _), "Expired audio must not play late.");
        Check(!queue.Offer(MomentCue.None, 10) && !queue.Offer(MomentCue.BoxReveal, double.NaN), "Invalid cue/time rejected.");
        foreach (MomentCue c in Enum.GetValues(typeof(MomentCue)))
            if (c != MomentCue.None) Check(MomentAudioPolicy.Resource(c).StartsWith("Audio/Moments/") && MomentAudioPolicy.Volume(c) <= .65f, "Every approved cue needs a local route with headroom.");
        Console.WriteLine("PASS: moment audio edges, priorities, background/account baselines and full-tail queue checks.");
    }
}
