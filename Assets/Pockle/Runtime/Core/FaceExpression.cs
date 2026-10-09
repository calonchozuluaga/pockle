using System;

namespace Pockle.Core
{
    public enum FaceExpression { Happy, Curious, Sleepy, Delighted }

    /// <summary>Normalized blend weights and face curves; independent of rendering and body motion.</summary>
    public struct FacePose
    {
        public readonly float Curious, Sleepy, Delighted, Blink;
        public float Happy { get { return Math.Max(0f, 1f - Curious - Sleepy - Delighted); } }
        public float EyeOpen { get { return Math.Max(0f, 1f - Sleepy - Delighted) * (1f - Blink); } }
        public float EyeArc { get { return Delighted - Sleepy - Blink * Happy; } }
        public float MouthOpen { get { return Curious + Delighted; } }
        internal FacePose(float curious, float sleepy, float delighted, float blink)
        { Curious = curious; Sleepy = sleepy; Delighted = delighted; Blink = blink; }

        public static FacePose For(FaceExpression expression)
        {
            return new FacePose(expression == FaceExpression.Curious ? 1 : 0,
                expression == FaceExpression.Sleepy ? 1 : 0,
                expression == FaceExpression.Delighted ? 1 : 0, 0);
        }

        public Point3 MouthPoint(float fraction)
        {
            fraction = float.IsNaN(fraction) || float.IsInfinity(fraction) ? 0 : Math.Max(0, Math.Min(1, fraction));
            float x = 2 * fraction - 1;
            double angle = Math.PI + fraction * Math.PI * 2;
            float ringX = (float)Math.Cos(angle), ringY = (float)Math.Sin(angle);
            return new Point3(Happy * x + Sleepy * x * .48f + Curious * ringX * .32f + Delighted * ringX,
                Happy * x * x + Sleepy * (.3f + .3f * x * x) +
                Curious * (.45f + .60f * ringY) + Delighted * (.30f + .90f * ringY), 0);
        }
    }

    /// <summary>One local face per viewer. Manual previews never change inventory or preferences.</summary>
    public sealed class FaceExpressionController
    {
        private float curious, sleepy, delighted, idle, blinkClock, reactionHold;
        private FaceExpression reaction = FaceExpression.Happy;
        private FaceExpression? preview;
        public FaceExpression Expression { get; private set; }
        public bool IsPreview { get { return preview.HasValue; } }
        public FacePose Pose { get; private set; }
        public const float SleepAfterSeconds = 12f;

        public void Reset()
        {
            curious = sleepy = delighted = idle = blinkClock = reactionHold = 0;
            Expression = FaceExpression.Happy;
            Pose = FacePose.For(Expression);
            preview = null;
        }

        public void Preview(FaceExpression? expression)
        {
            if (expression.HasValue && !Enum.IsDefined(typeof(FaceExpression), expression.Value))
                throw new ArgumentOutOfRangeException(nameof(expression));
            preview = expression;
            idle = reactionHold = blinkClock = 0;
        }

        public FacePose Step(float deltaTime, bool held, float compression, float stretch,
            float height, float pinch, float shake, bool landed, bool calm)
        {
            float dt = Positive(deltaTime);
            // Hitches/background returns cannot jump straight to sleep or consume a reaction.
            dt = Math.Min(dt, .1f);
            compression = Positive(compression); stretch = Positive(stretch); height = Positive(height);
            pinch = Finite(pinch) ? pinch : 0; shake = Positive(shake);
            bool curiousNow = held && (height > .06f || stretch > .10f || pinch > .08f);
            bool delightedNow = held && (compression > .10f || pinch < -.06f) || shake > .12f || landed;
            bool active = held || shake > .04f || landed;
            idle = active ? 0 : Math.Min(SleepAfterSeconds, idle + dt);
            reactionHold = Math.Max(0, reactionHold - dt);
            if (curiousNow || delightedNow)
            {
                reaction = curiousNow ? FaceExpression.Curious : FaceExpression.Delighted;
                reactionHold = .75f;
            }
            Expression = preview ?? (reactionHold > 0 ? reaction :
                idle >= SleepAfterSeconds ? FaceExpression.Sleepy : FaceExpression.Happy);
            FacePose target = FacePose.For(Expression);
            float blend = calm ? 1f : (float)(1 - Math.Exp(-12 * dt));
            curious += (target.Curious - curious) * blend;
            sleepy += (target.Sleepy - sleepy) * blend;
            delighted += (target.Delighted - delighted) * blend;
            float blink = 0;
            if (!calm && !preview.HasValue && Expression == FaceExpression.Happy && !active && reactionHold == 0)
            {
                blinkClock = (blinkClock + dt) % 4.8f;
                if (blinkClock > 4.52f)
                    blink = (float)Math.Sin((blinkClock - 4.52f) / .28f * Math.PI);
            }
            else blinkClock = 0;
            Pose = new FacePose(curious, sleepy, delighted, Math.Max(0, Math.Min(1, blink)));
            return Pose;
        }

        private static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
        private static float Positive(float value) { return Finite(value) ? Math.Max(0, value) : 0; }
    }
}
