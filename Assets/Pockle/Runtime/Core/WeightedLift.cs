using System;

namespace Pockle.Core
{
    /// <summary>A damped hand constraint while held; analytic gravity after release.</summary>
    public sealed class WeightedLift
    {
        public const float Maximum = .75f;
        public const float Gravity = 5.5f;
        private readonly Spring1D hand = new Spring1D(2.6f, .9f);
        private bool wasHeld;
        private float target;
        public float Value { get; private set; }
        public float Velocity { get; private set; }
        public float LandingSpeed { get; private set; }
        public float Target
        {
            get => target;
            set { if (Numeric.IsFinite(value)) target = Numeric.Clamp(value, 0, Maximum); }
        }

        public void Reset(float height = 0)
        {
            Value = target = Numeric.Clamp(Numeric.FiniteOr(height, 0), 0, Maximum);
            Velocity = LandingSpeed = 0;
            wasHeld = false;
            hand.Reset(Value);
        }

        public void ConstrainHeight(float maximum)
        {
            maximum = Numeric.Clamp(Numeric.FiniteOr(maximum, 0), 0, Maximum);
            if (Value <= maximum) return;
            Value = maximum;
            if (Velocity > 0) Velocity = 0;
            hand.Reset(Value);
        }

        public void Step(float dt, bool held, bool calm)
        {
            LandingSpeed = 0;
            if (!Numeric.IsFinite(dt) || dt <= 0) return;
            if (calm)
            {
                Value = held ? target : 0;
                Velocity = 0; hand.Reset(Value); wasHeld = held;
                return;
            }
            if (held)
            {
                if (!wasHeld) hand.Reset(Value); // Regrab exactly where the falling body is.
                hand.Target = target;
                hand.Step(dt);
                Value = Numeric.Clamp(hand.Value, 0, Maximum);
                Velocity = Numeric.Clamp(hand.Velocity, -2, 2);
            }
            else if (Value > 0)
            {
                if (wasHeld) Velocity = Numeric.Clamp(Velocity, -2, .55f);
                // Solve time-to-contact, so impact strength does not depend on frame rate.
                double v = Velocity;
                double impactTime = (v + Math.Sqrt(v * v + 2d * Gravity * Value)) / Gravity;
                if (dt >= impactTime)
                {
                    LandingSpeed = (float)Math.Sqrt(v * v + 2d * Gravity * Value);
                    Value = Velocity = 0;
                }
                else
                {
                    float next = (float)(Value + v * dt - .5d * Gravity * dt * dt);
                    Velocity = (float)(v - Gravity * dt);
                    Value = Math.Min(Maximum, next);
                    if (next > Maximum && Velocity > 0) Velocity = 0;
                }
            }
            else Velocity = 0;
            wasHeld = held;
        }
    }
}
