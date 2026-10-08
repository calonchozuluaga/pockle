using System;

namespace Pockle.Core
{
    /// <summary>
    /// An analytically integrated damped spring. Step consumes the whole elapsed time,
    /// so a slow frame or application hitch cannot destabilize an Euler integrator.
    /// Frequency is in cycles per second; damping ratio 1 is critical damping.
    /// </summary>
    public sealed class Spring1D
    {
        private readonly double angularFrequency;
        private readonly double dampingRatio;
        private float target;

        public float Value { get; private set; }
        public float Velocity { get; private set; }

        public float Target
        {
            get { return target; }
            set
            {
                // A malformed pointer event must not poison subsequent frames.
                if (Numeric.IsFinite(value))
                    target = value;
            }
        }

        public Spring1D(float frequency = 3.5f, float dampingRatio = .65f, float initialValue = 0f)
        {
            if (!Numeric.IsFinite(frequency) || frequency <= 0f)
                frequency = 3.5f;
            if (!Numeric.IsFinite(dampingRatio) || dampingRatio < 0f)
                dampingRatio = .65f;

            // Keep accidental settings within a useful interactive range.
            angularFrequency = 2d * Math.PI * Numeric.Clamp(frequency, .01f, 60f);
            this.dampingRatio = Numeric.Clamp(dampingRatio, 0f, 10f);
            Reset(initialValue);
        }

        public void Reset(float value = 0f)
        {
            Value = Numeric.FiniteOr(value, 0f);
            target = Value;
            Velocity = 0f;
        }

        public void Step(float deltaTime)
        {
            if (!Numeric.IsFinite(deltaTime) || deltaTime <= 0f)
                return;

            double time = deltaTime;
            double displacement = (double)Value - target;
            double velocity = Velocity;
            double frequency = angularFrequency;
            double ratio = dampingRatio;
            double nextDisplacement;
            double nextVelocity;

            if (ratio < 1d - .00001d)
            {
                double decay = ratio * frequency;
                double dampedFrequency = frequency * Math.Sqrt(1d - ratio * ratio);
                double attenuation = Math.Exp(-decay * time);
                double cosine = Math.Cos(dampedFrequency * time);
                double sine = Math.Sin(dampedFrequency * time);
                nextDisplacement = attenuation * (displacement * cosine +
                    (velocity + decay * displacement) / dampedFrequency * sine);
                nextVelocity = attenuation * (velocity * cosine -
                    (decay * velocity + frequency * frequency * displacement) / dampedFrequency * sine);
            }
            else if (ratio > 1d + .00001d)
            {
                double root = Math.Sqrt(ratio * ratio - 1d);
                double slowRate = -frequency * (ratio - root);
                double fastRate = -frequency * (ratio + root);
                double slowAmplitude = (velocity - fastRate * displacement) / (slowRate - fastRate);
                double fastAmplitude = displacement - slowAmplitude;
                double slow = slowAmplitude * Math.Exp(slowRate * time);
                double fast = fastAmplitude * Math.Exp(fastRate * time);
                nextDisplacement = slow + fast;
                nextVelocity = slowRate * slow + fastRate * fast;
            }
            else
            {
                double attenuation = Math.Exp(-frequency * time);
                double coefficient = velocity + frequency * displacement;
                nextDisplacement = attenuation * (displacement + coefficient * time);
                nextVelocity = attenuation * (velocity - frequency * coefficient * time);
            }

            Value = Numeric.ToFiniteFloat(target + nextDisplacement);
            Velocity = Numeric.ToFiniteFloat(nextVelocity);
        }
    }
}
