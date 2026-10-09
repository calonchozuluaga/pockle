namespace Pockle.Core
{
    /// <summary>Visual compliance and recovery for a finish, independent of its character mesh.</summary>
    public sealed class MaterialHandling
    {
        public string Id { get; }
        public float Compression { get; }
        public float Stretch { get; }
        public float Pinch { get; }
        public float Sag { get; }
        public float Bend { get; }
        public float RockDegrees { get; }
        public float RecoveryFrequency { get; }
        public float RecoveryDamping { get; }
        public float ShakeFrequency { get; }
        public float ShakeDamping { get; }

        private MaterialHandling(string id, float compression, float stretch, float pinch,
            float sag, float bend, float rock, float frequency, float damping, float shakeFrequency, float shakeDamping)
        {
            Id = id; Compression = compression; Stretch = stretch; Pinch = pinch;
            Sag = sag; Bend = bend; RockDegrees = rock; RecoveryFrequency = frequency;
            RecoveryDamping = damping; ShakeFrequency = shakeFrequency; ShakeDamping = shakeDamping;
        }

        public static readonly MaterialHandling Gel = new MaterialHandling("gel", 1, 1, 1, 1, 1, 0, 3.8f, .45f, ToyFeel.ShakeFrequency, ToyFeel.ShakeDamping);
        public static readonly MaterialHandling Foam = new MaterialHandling("foam", .95f, .18f, .35f, .20f, .40f, 0, 1.8f, 1, 2.8f, .95f);
        public static readonly MaterialHandling Flock = new MaterialHandling("flock", .45f, .20f, .28f, .28f, .40f, 0, 2.7f, .95f, 3.5f, .90f);
        public static readonly MaterialHandling Plush = new MaterialHandling("plush", .75f, .28f, .40f, .42f, .55f, 0, 2.3f, .90f, 3, .90f);
        public static readonly MaterialHandling Firm = new MaterialHandling("firm", .035f, .012f, .015f, 0, .035f, 7, 6, .80f, 5.5f, .85f);

        public static MaterialHandling ForFinish(string finishId)
        {
            switch (finishId)
            {
                case "mochi-foam": return Foam;
                case "velvet-flock": return Flock;
                case "boucle-plush": return Plush;
                case "matte-vinyl": case "gloss-vinyl": case "coated-metallic": return Firm;
                default: return Gel;
            }
        }
    }
}
