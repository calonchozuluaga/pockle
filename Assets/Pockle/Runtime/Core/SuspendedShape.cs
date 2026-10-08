using System;

namespace Pockle.Core
{
    /// <summary>Bounded gravity droop around a supported grip; shared by shell, face and filling.</summary>
    public readonly struct SuspendedShape
    {
        private readonly float sag, floor;
        private readonly Point3 grip;

        public SuspendedShape(float sag, Point3 grip, float clearance)
        {
            this.sag = Numeric.Clamp(Numeric.FiniteOr(sag, 0), 0, .24f);
            this.grip = new Point3(Numeric.Clamp(Numeric.FiniteOr(grip.X, 0), -1.5f, 1.5f),
                Numeric.Clamp(Numeric.FiniteOr(grip.Y, 0), -1.5f, 2f), Numeric.Clamp(Numeric.FiniteOr(grip.Z, 0), -1.5f, 1.5f));
            floor = -1f - Numeric.Clamp(Numeric.FiniteOr(clearance, 0), 0, 1.05f);
        }

        public Point3 Apply(Point3 point)
        {
            point = new Point3(Numeric.FiniteOr(point.X, 0), Numeric.FiniteOr(point.Y, 0), Numeric.FiniteOr(point.Z, 0));
            if (sag == 0) return point;
            float dx = point.X - grip.X, dy = point.Y - grip.Y, dz = point.Z - grip.Z;
            float away = Numeric.SmoothStep((dx * dx + dy * dy + dz * dz) / .36f);
            float below = Numeric.SmoothStep(-dy / 1.2f);
            float drop = sag * (.18f + .82f * below) * away;
            float belly = sag * .16f * below * away;
            return new Point3(point.X + dx * belly, Math.Max(floor, point.Y - drop), point.Z + dz * belly);
        }
    }
}
