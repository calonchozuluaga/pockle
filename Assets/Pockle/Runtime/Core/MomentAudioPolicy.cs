using System;
using System.Collections.Generic;

namespace Pockle.Core
{
    public enum MomentCue { None, FusionComplete, BoxReveal, SpecialReveal, Steps1000, WalkingBoxReady, TradeComplete, PurchaseSaved }

    /// <summary>Local presentation only: never changes ownership, walking credit, or notification preferences.</summary>
    public static class MomentAudioPolicy
    {
        public static string Resource(MomentCue cue)
        {
            string name;
            switch (cue)
            {
                case MomentCue.FusionComplete: name = "fusion-complete"; break;
                case MomentCue.BoxReveal: name = "box-reveal"; break;
                case MomentCue.SpecialReveal: name = "special-reveal"; break;
                case MomentCue.Steps1000: name = "steps-1000"; break;
                case MomentCue.WalkingBoxReady: name = "walking-box-ready"; break;
                case MomentCue.TradeComplete: name = "trade-complete"; break;
                case MomentCue.PurchaseSaved: name = "purchase-saved"; break;
                default: return "";
            }
            return "Audio/Moments/" + name;
        }
        public static bool Passive(MomentCue cue) => cue == MomentCue.Steps1000 || cue == MomentCue.WalkingBoxReady || cue == MomentCue.PurchaseSaved;
        public static int Priority(MomentCue cue)
        {
            switch (cue)
            {
                case MomentCue.FusionComplete: case MomentCue.TradeComplete: return 5;
                case MomentCue.SpecialReveal: return 4;
                case MomentCue.BoxReveal: return 3;
                case MomentCue.WalkingBoxReady: return 2;
                case MomentCue.Steps1000: case MomentCue.PurchaseSaved: return 1;
                default: return 0;
            }
        }
        public static float Volume(MomentCue cue) => cue == MomentCue.PurchaseSaved ? .35f : Passive(cue) ? .5f : .65f;
    }

    /// <summary>One bounded pending cue; a later notification cannot obscure a reveal or cut its tail.</summary>
    public sealed class MomentCueQueue
    {
        public const double MaximumDelay = 4;
        private MomentCue pending;
        private double pendingAt;
        public bool HasPending => pending != MomentCue.None;
        public bool Offer(MomentCue cue, double now)
        {
            if (!Finite(now) || MomentAudioPolicy.Priority(cue) == 0) return false;
            Expire(now);
            if (HasPending && MomentAudioPolicy.Priority(cue) < MomentAudioPolicy.Priority(pending)) return false;
            pending = cue; pendingAt = now; return true;
        }
        public bool Take(double now, bool busy, bool passiveAllowed, out MomentCue cue)
        {
            cue = MomentCue.None;
            if (!Finite(now)) return false;
            Expire(now);
            if (MomentAudioPolicy.Passive(pending) && !passiveAllowed) Clear();
            if (busy || !HasPending) return false;
            cue = pending; Clear(); return true;
        }
        private void Expire(double now) { if (HasPending && (now < pendingAt || now - pendingAt > MaximumDelay)) Clear(); }
        public void Clear() { pending = MomentCue.None; }
        private static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
    }

    public readonly struct WalkingMomentSnapshot
    {
        public readonly string Owner;
        public readonly int UtcDay, EarnedToday, StepCredit;
        public readonly IReadOnlyList<string> BankedIds;
        public WalkingMomentSnapshot(string owner, int day, int earned, int credit, IReadOnlyList<string> bankedIds)
        { Owner = owner; UtcDay = day; EarnedToday = earned; StepCredit = credit; BankedIds = bankedIds; }
    }

    /// <summary>In-memory high-water marks. First sync, account changes and foreground recovery are always silent.</summary>
    public sealed class WalkingMomentGate
    {
        private string owner = "";
        private int day, milestones;
        private bool initialized;
        private readonly HashSet<string> seenBoxes = new HashSet<string>(StringComparer.Ordinal);
        public void Rebaseline() { initialized = false; }
        public MomentCue Observe(WalkingMomentSnapshot value, bool audible)
        {
            if (string.IsNullOrEmpty(value.Owner) || value.BankedIds == null || value.UtcDay < 0 ||
                value.EarnedToday < 0 || value.EarnedToday > 4 || value.StepCredit < 0 || value.StepCredit > 100000) return MomentCue.None;
            bool newOwner = owner != value.Owner;
            if (newOwner) { owner = value.Owner; seenBoxes.Clear(); initialized = false; }
            bool newBox = false;
            foreach (string id in value.BankedIds)
                if (!string.IsNullOrEmpty(id) && seenBoxes.Add(id)) newBox = true;
            int next = value.EarnedToday + value.StepCredit / 1000;
            bool stepEdge = initialized && value.UtcDay == day && next > milestones;
            bool wasInitialized = initialized;
            if (!initialized || day != value.UtcDay) milestones = next;
            else milestones = Math.Max(milestones, next);
            day = value.UtcDay; initialized = true;
            if (!wasInitialized || !audible) return MomentCue.None;
            if (newBox) return MomentCue.WalkingBoxReady;
            return stepEdge ? MomentCue.Steps1000 : MomentCue.None;
        }
    }
}
