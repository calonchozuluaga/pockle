using System;

namespace Pockle.Core
{
    /// <summary>Stretch along an arbitrary local axis, with inverse radial scaling and a planted foot.</summary>
    public readonly struct DirectionalStretch
    {
        private readonly float amount;
        private readonly Point3 axis;
        private readonly float radial;
        private readonly float difference;

        public DirectionalStretch(float amount, Point3 direction)
        {
            this.amount = Numeric.Clamp(Numeric.FiniteOr(amount, 0f), -.35f, .55f);
            double length = Math.Sqrt((double)direction.X * direction.X +
                (double)direction.Y * direction.Y + (double)direction.Z * direction.Z);
            axis = double.IsNaN(length) || double.IsInfinity(length) || length < .000001d
                ? new Point3(0f, 1f, 0f)
                : new Point3((float)(direction.X / length), (float)(direction.Y / length), (float)(direction.Z / length));
            float along = 1f + this.amount;
            radial = (float)(1d / Math.Sqrt(along));
            difference = along - radial;
        }

        public Point3 Apply(Point3 point)
        {
            if (amount == 0f || point.Y <= -1f) return point;
            float y = point.Y + 1f;
            float projection = point.X * axis.X + y * axis.Y + point.Z * axis.Z;
            float weight = Numeric.SmoothStep(y / .3f);
            return new Point3(
                point.X + (point.X * (radial - 1f) + axis.X * projection * difference) * weight,
                Math.Max(-1f, point.Y + (y * (radial - 1f) + axis.Y * projection * difference) * weight),
                point.Z + (point.Z * (radial - 1f) + axis.Z * projection * difference) * weight);
        }

        public Point3 FeatureScale
        {
            get { return amount == 0f ? new Point3(1f, 1f, 1f) : new Point3(
                radial + difference * axis.X * axis.X,
                radial + difference * axis.Y * axis.Y,
                radial + difference * axis.Z * axis.Z); }
        }
    }
}
