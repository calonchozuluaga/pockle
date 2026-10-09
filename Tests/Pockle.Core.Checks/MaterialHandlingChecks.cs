using System;
using Pockle.Core;

internal static class MaterialHandlingChecks
{
    private static int assertions;
    private static void Check(bool condition, string message)
    { assertions++; if (!condition) throw new InvalidOperationException(message); }

    internal static void Run()
    {
        var gel = MaterialHandling.Gel;
        var flock = MaterialHandling.Flock;
        var firm = MaterialHandling.Firm;
        Check(.42f * flock.Compression >= .15f && .42f * flock.Compression < .25f, "Flock should give under a press without flattening like gel.");
        Check(.6f * firm.Stretch < .01f && .55f * firm.Pinch < .01f, "Vinyl can stretch like jelly.");
        Check(firm.Sag == 0 && firm.RockDegrees > 0, "Firm toy must rock rather than hang as gel.");
        Check(flock.Sag > 0 && flock.Sag < gel.Sag / 2, "Flock stuffing should sag gently.");
        foreach (var finish in ToyCatalog.Finishes)
        {
            var profile = MaterialHandling.ForFinish(finish.Id);
            foreach (float step in new[] { 1f/240, 1f/60, 1f/15, .25f })
            {
                var spring = new Spring1D(profile.RecoveryFrequency, profile.RecoveryDamping, .4f);
                spring.Target = 0;
                float min = .4f;
                for (int i = 0; i < (int)Math.Ceiling(3 / step); i++)
                {
                    spring.Step(step);
                    min = Math.Min(min, spring.Value);
                    Check(Numeric.IsFinite(spring.Value) && Math.Abs(spring.Value) <= .400001f, "Material recovery became unstable.");
                }
                Check(Math.Abs(spring.Value) < .00001f && Math.Abs(spring.Velocity) < .0001f, "Material did not settle.");
                // A 250ms sample can skip an entire rebound peak. Check its amplitude
                // only at interactive frame rates; coarse steps still test stability/settling.
                if (profile == gel)
                {
                    if (step <= 1f / 60) Check(min < -.025f, "Gel lost its noticeable rebound.");
                }
                else Check(min > -.025f, "Non-gel material rebounds too dramatically.");
                var shake = new Spring1D(profile.ShakeFrequency, profile.ShakeDamping, .3f);
                shake.Target = 0; shake.Step(5);
                Check(Math.Abs(shake.Value) < .00001f && Math.Abs(shake.Velocity) < .00001f, "Material shake did not settle after a pause.");
            }
        }
        Check(MaterialHandling.ForFinish("gloss-vinyl") == firm && MaterialHandling.ForFinish("matte-vinyl") == firm && MaterialHandling.ForFinish("coated-metallic") == firm, "Hard finishes disagree on compliance.");
        Check(MaterialHandling.ForFinish("velvet-flock") == flock, "Flock dispatch is incorrect.");
        Check(MaterialHandling.ForFinish("boucle-plush") == MaterialHandling.Plush &&
            MaterialHandling.ForFinish("mochi-foam") == MaterialHandling.Foam, "Nook finish dispatch is incorrect.");
        foreach (float step in new[] { 1f/240, 1f/60, 1f/15, .25f })
        {
            var plush = new Spring1D(MaterialHandling.Plush.RecoveryFrequency, MaterialHandling.Plush.RecoveryDamping, .4f);
            var foam = new Spring1D(MaterialHandling.Foam.RecoveryFrequency, MaterialHandling.Foam.RecoveryDamping, .4f);
            plush.Target = foam.Target = 0;
            float elapsed = 0;
            while (elapsed < .2f)
            {
                float dt = Math.Min(step, .2f - elapsed);
                plush.Step(dt); foam.Step(dt); elapsed += dt;
            }
            Check(foam.Value > plush.Value * 1.5f, "Foam should recover more slowly than plush after release.");
            Check(.42f * MaterialHandling.Foam.Compression > .42f * MaterialHandling.Plush.Compression,
                "Foam should give more deeply than a stuffed pillow.");
            Check(MaterialHandling.Plush.Sag > MaterialHandling.Foam.Sag,
                "Plush stuffing should hang more softly than resilient foam.");
            plush.Step(5); foam.Step(5);
            Check(Math.Abs(plush.Value) < .00001f && Math.Abs(foam.Value) < .00001f,
                "A pause leaves a Nook finish stuck away from rest.");
        }
        Console.WriteLine("PASS: " + assertions + " material handling assertions (compliance, rebound, settling, frame timing).");
    }
}
