using System;

namespace Pockle.Core
{
    /// <summary>Reject steady gravity, filter accelerometer noise, and bound the jelly drive.</summary>
    public sealed class MotionJiggle
    {
        private Point3 gravity;
        private bool initialized;

        public void Reset() { initialized = false; gravity = new Point3(0f, 0f, 0f); }

        public Point3 Step(Point3 acceleration, float deltaTime)
        {
            if (!Numeric.IsFinite(acceleration.X) || !Numeric.IsFinite(acceleration.Y) ||
                !Numeric.IsFinite(acceleration.Z) || !Numeric.IsFinite(deltaTime) || deltaTime <= 0f)
            { Reset(); return new Point3(0f, 0f, 0f); }
            var sample = new Point3(Numeric.Clamp(acceleration.X, -8f, 8f),
                Numeric.Clamp(acceleration.Y, -8f, 8f), Numeric.Clamp(acceleration.Z, -8f, 8f));
            if (!initialized) { gravity = sample; initialized = true; return new Point3(0f, 0f, 0f); }
            float alpha = (float)(1d - Math.Exp(-Math.Min(deltaTime, .1f) / .25d));
            gravity = new Point3(gravity.X + (sample.X - gravity.X) * alpha,
                gravity.Y + (sample.Y - gravity.Y) * alpha, gravity.Z + (sample.Z - gravity.Z) * alpha);
            float x = sample.X - gravity.X, y = sample.Y - gravity.Y, z = sample.Z - gravity.Z;
            float magnitude = (float)Math.Sqrt(x * x + y * y + z * z);
            if (magnitude <= .12f) return new Point3(0f, 0f, 0f);
            float drive = Math.Min(ToyFeel.ShakeLimit, (magnitude - .12f) * ToyFeel.ShakeGain) / magnitude;
            return new Point3(x * drive, y * drive, z * drive);
        }
    }
}
