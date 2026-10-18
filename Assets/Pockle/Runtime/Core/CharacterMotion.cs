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
            new CharacterIdleDefinition("tula", "Slow nod and tiny lift", 6.4f, 5.2f, .012f, 4, 0, 1, .035f),
            new CharacterIdleDefinition("fox", "Alert fox glance and gentle rock", 4.7f, 3.7f, 0, 2, 8, 1, .045f),
            new CharacterIdleDefinition("axolotl", "Soft plush sway", 5.8f, 4.2f, 0, 1, 5, .5f, .025f),
            new CharacterIdleDefinition("firb", "Evergreen branch sway", 5.3f, 4.1f, 0, 1, 2, 2.5f, .035f),
            new CharacterIdleDefinition("coco", "Cozy cocoa wobble", 6.1f, 4.6f, 0, 1.5f, 3, 1.5f, .022f),
            new CharacterIdleDefinition("snowy", "Snowy mitten greeting", 5.6f, 4.3f, 0, 2, 3.5f, 1.2f, .028f),
            new CharacterIdleDefinition("ginger", "Gingerbread cheerful bounce", 4.5f, 3.5f, 0, .5f, 2.5f, 3.2f, .06f),
            new CharacterIdleDefinition("reiny", "Reindeer curious nod", 5.4f, 3.8f, 0, 2, 4, 1.1f, .05f),
            new CharacterIdleDefinition("guin", "Penguin cozy waddle", 5.8f, 4.2f, 0, .8f, 2.2f, 3.4f, .035f),
            new CharacterIdleDefinition("bundy", "Pudding gentle wobble", 6.1f, 4.5f, 0, 1, 1.5f, 2, .045f)
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
