using System;

namespace Pockle.Core
{
    /// <summary>Shared bounded tuning for the more expressive toy response.</summary>
    public static class ToyFeel
    {
        public const float ShakeLimit = .34f;
        public const float ShakeGain = .20f;
        public const float ShakeFrequency = 4.3f;
        public const float ShakeDamping = .36f;
        public const float HangingSag = .32f;
        public const float MaximumSag = .40f;

        private static float Speed(float value) => Numeric.Clamp(Numeric.FiniteOr(value, 0), 0, 4);
        public static float LandingCompression(float speed) => Math.Min(.32f, Speed(speed) * .115f);
        public static float LandingJiggle(float speed) => -Math.Min(.18f, Speed(speed) * .055f);
        public static float LandingLean(float speed, float gripX) =>
            Numeric.Clamp(Numeric.FiniteOr(gripX, 0) * Speed(speed) * .055f, -.13f, .13f);
        public static float LandingVolume(float speed) => .75f + Math.Min(.45f, Speed(speed) * .15f);
    }
}
