namespace Pockle.Core
{
    public readonly struct RevealPose
    {
        public readonly float Opening, BoxAlpha, ToyScale, ToyLift, Compression;
        public readonly bool ToyVisible, Finished;

        public RevealPose(float opening, float alpha, float scale, float lift,
            float compression, bool visible, bool finished)
        {
            Opening = opening; BoxAlpha = alpha; ToyScale = scale;
            ToyLift = lift; Compression = compression; ToyVisible = visible; Finished = finished;
        }
    }

    public static class RevealSequence
    {
        public static float Duration(bool reducedMotion) { return reducedMotion ? .85f : 2.6f; }

        public static RevealPose Sample(float elapsed, bool reducedMotion)
        {
            float progress = Numeric.Clamp(Numeric.FiniteOr(elapsed, 0f) / Duration(reducedMotion), 0f, 1f);
            float opening = Ease(progress, .10f, .35f);
            float alpha = 1f - Ease(progress, reducedMotion ? .28f : .50f, .75f);
            bool visible = progress >= .30f;
            float scale = reducedMotion ? 1f : .45f + .55f * Ease(progress, .30f, .62f);
            float lift = reducedMotion ? 0f : 1.05f * Ease(progress, .30f, .60f) * (1f - Ease(progress, .64f, .90f));
            float compression = reducedMotion ? 0f : .12f * Ease(progress, .84f, .90f) * (1f - Ease(progress, .90f, 1f));
            return new RevealPose(opening, alpha, scale, lift, compression, visible, progress >= 1f);
        }

        private static float Ease(float value, float start, float end)
        { return Numeric.SmoothStep((value - start) / (end - start)); }
    }
}
