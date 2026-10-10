#nullable enable
using System;
using System.Collections.Generic;

namespace Pockle.Core
{
    public readonly struct CharacterIdlePose
    {
        public float Height { get; }
        public float PitchDegrees { get; }
        public float YawDegrees { get; }
        public float RollDegrees { get; }
        public float Compression { get; }
        public float Stretch { get; }
        internal CharacterIdlePose(float height, float pitch, float yaw, float roll, float compression, float stretch)
        { Height = height; PitchDegrees = pitch; YawDegrees = yaw; RollDegrees = roll; Compression = compression; Stretch = stretch; }
    }

    public sealed class CharacterIdleDefinition
    {
        public string Id { get; }
        public string Description { get; }
        public float IdleDurationSeconds { get; }
        public float EagerDurationSeconds { get; }
        internal float Squish { get; }
        internal float Pitch { get; }
        internal float Turn { get; }
        internal float Sway { get; }
        internal float Hop { get; }
        internal CharacterIdleDefinition(string id, string description, float period, float eagerPeriod,
            float squish, float pitch, float turn, float sway, float hop)
        { Id = id; Description = description; IdleDurationSeconds = period; EagerDurationSeconds = eagerPeriod;
            Squish = squish; Pitch = pitch; Turn = turn; Sway = sway; Hop = hop; }
    }

    /// <summary>Allocation-free, closed motion loops; a lineup presentation separate from tactile physics.</summary>
    public static class CharacterMotion
    {
        private static readonly CharacterIdleDefinition[] profiles = {
            new CharacterIdleDefinition("pip", "Jelly breath and curious bounce", 4f, 3.6f, .020f, 0, 3, 2, .10f),
            new CharacterIdleDefinition("dew", "Sleepy breath and drowsy nod", 5.8f, 4.8f, .018f, 3, 0, 1, .065f),
            new CharacterIdleDefinition("tula", "Slow nod and tiny lift", 6.4f, 5.2f, .012f, 4, 0, 1, .035f),
            new CharacterIdleDefinition("ripple", "Float and gentle fin-like bank", 4.8f, 3.8f, .008f, 2, 0, 4, .08f),
            new CharacterIdleDefinition("moss", "Shy lean and garden peek", 5.4f, 4.5f, .012f, 3, 4, 2, .045f),
            new CharacterIdleDefinition("nook", "Cozy stuffed breath and soft rock", 6f, 4.8f, .025f, 1, 0, 2, .04f),
            new CharacterIdleDefinition("wisp", "Comet bob and curious turn", 3.8f, 3.1f, .008f, 1, 6, 3, .11f),
            new CharacterIdleDefinition("loop", "Bow sway and cheerful bounce", 4.2f, 3.4f, .014f, 0, 4, 4, .09f),
            new CharacterIdleDefinition("bop", "Firm show-off turn and top rock", 4.6f, 3.3f, 0, 0, 9, 3, .055f),
            new CharacterIdleDefinition("rolo", "Firm robot nod and scan", 5.2f, 4f, 0, 4, 7, 0, .025f),
            new CharacterIdleDefinition("mallow", "Moon sway and quiet peek", 6.2f, 5f, 0, 1, 3, 3, .04f),
            new CharacterIdleDefinition("sprig", "Seed bob and sprout-like sway", 4.9f, 3.9f, 0, 3, 4, 2, .065f)
        };
        private static readonly Dictionary<string, CharacterIdleDefinition> byId = BuildLookup();
        public static IReadOnlyList<CharacterIdleDefinition> Profiles { get; } = Array.AsReadOnly(profiles);

        public static bool TryGetProfile(string? id, out CharacterIdleDefinition profile)
        { profile = null!; return id != null && byId.TryGetValue(id, out profile!); }

        public static CharacterIdlePose Sample(string? profileId, double elapsedSeconds,
            bool eager = false, bool calm = false, float phaseOffset = 0)
        {
            if (calm || double.IsNaN(elapsedSeconds) || double.IsInfinity(elapsedSeconds) || elapsedSeconds < 0 ||
                !Numeric.IsFinite(phaseOffset) || !TryGetProfile(profileId, out var profile)) return default;
            double period = eager ? profile.EagerDurationSeconds : profile.IdleDurationSeconds;
            // Reduce time before adding phase, preserving bounds even after long sessions.
            double cycle = ((elapsedSeconds % period) / period + phaseOffset % 1d) % 1d;
            if (cycle < 0) cycle += 1d;
            // Eager motion is a short greeting followed by a resting beat, not constant jumping.
            if (eager) { cycle *= 3d; if (cycle >= 1d) return default; }
            double angle = cycle * Math.PI * 2d;
            float pulse = (float)((1d - Math.Cos(angle)) * .5d);
            float wave = (float)Math.Sin(angle) * pulse;
            return new CharacterIdlePose(eager ? profile.Hop * pulse : 0,
                profile.Pitch * wave, profile.Turn * wave, profile.Sway * wave,
                profile.Squish * pulse * (eager ? 1.5f : 1f),
                eager ? profile.Squish * .5f * pulse : 0);
        }

        private static Dictionary<string, CharacterIdleDefinition> BuildLookup()
        {
            var result = new Dictionary<string, CharacterIdleDefinition>(StringComparer.Ordinal);
            foreach (var profile in profiles) result.Add(profile.Id, profile);
            return result;
        }
    }
}
