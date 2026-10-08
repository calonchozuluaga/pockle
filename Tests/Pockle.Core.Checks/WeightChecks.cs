using System;
using System.IO;
using System.Text.Json;
using Pockle.Core;

internal static class WeightChecks
{
    private static int checks;
    public static void Run()
    {
        foreach (int fps in new[] { 15, 30, 60, 120, 240 })
        {
            var lift = new WeightedLift { Target = .6f };
            float dt = 1f / fps;
            lift.Step(dt, true, false);
            Assert(lift.Value > 0 && lift.Value < .3f, "Weighted lift should trail the hand, not snap to it.");
            for (int i = 0; i < fps * 2; i++) lift.Step(dt, true, false);
            Near(lift.Value, .6f, .0001f, "Held toy did not settle at the requested grip height.");
            float elapsed = 0, impact = 0;
            int landings = 0;
            for (int i = 0; i < fps; i++)
            {
                lift.Step(dt, false, false); elapsed += dt;
                Assert(lift.Value >= 0 && lift.Value <= .75f, "Falling body escaped the floor or lift limit.");
                if (lift.LandingSpeed > 0)
                {
                    impact = lift.LandingSpeed; landings++;
                    Assert(elapsed <= .48f + dt, "Gravity drop took too long.");
                }
            }
            Assert(landings == 1 && lift.Value == 0 && lift.Velocity == 0, "Landing repeated or body did not settle on the plate.");
            Near(impact, (float)Math.Sqrt(2 * WeightedLift.Gravity * .6f), .001f, "Impact depends on frame rate rather than drop energy.");
            lift.Reset(.6f); lift.Step(.08f, false, false);
            float falling = lift.Value;
            lift.Target = falling;
            lift.Step(dt, true, false);
            Near(lift.Value, falling, .00001f, "Regrabbing a falling toy caused a jump.");
            for (int i = 0; i < fps; i++) lift.Step(dt, true, false);
            Near(lift.Value, falling, .0001f, "Stable one/two-finger handoff drifted vertically.");
            lift.Reset(.6f); lift.Step(dt, false, true);
            Assert(lift.Value == 0 && lift.LandingSpeed == 0, "Calm mode generated a gravity fall or landing pulse.");
            lift.Target = .5f; lift.Step(dt, true, true);
            Assert(lift.Value == .5f, "Calm mode lost direct lift control.");
        }
        var constrained = new WeightedLift();
        constrained.Reset(.7f); constrained.ConstrainHeight(.2f); constrained.Step(10, false, false);
        Near(constrained.LandingSpeed, (float)Math.Sqrt(2 * WeightedLift.Gravity * .2f), .00001f, "Camera clipping retained invisible drop energy.");
        constrained.Reset(); constrained.Target = float.NaN;
        constrained.Step(float.PositiveInfinity, true, false);
        Assert(constrained.Value == 0, "Malformed pointer/timing poisoned the lift.");
        constrained.Target = 999; constrained.Step(99, true, false);
        Assert(constrained.Value <= .75f, "An extreme target escaped the safe limit.");
        var small = new WeightedLift(); small.Reset(.1f); small.Step(10, false, false);
        var large = new WeightedLift(); large.Reset(.7f); large.Step(10, false, false);
        Assert(large.LandingSpeed > small.LandingSpeed * 2, "Higher drops did not carry greater landing weight.");

        var grip = new Point3(.25f, .3f, -.5f);
        var suspended = new SuspendedShape(.18f, grip, .6f);
        Point3 support = suspended.Apply(grip);
        Near(support.X, grip.X, 0, "Sag shifted the grip X."); Near(support.Y, grip.Y, 0, "Sag shifted the grip Y.");
        var bottom = new Point3(0, -1, 0);
        Assert(suspended.Apply(bottom).Y < -1.1f, "Suspended gel did not sag beneath its grip.");
        Assert(new SuspendedShape(.18f, grip, 0).Apply(bottom).Y >= -1, "Sag penetrated the plate.");
        Assert(new SuspendedShape(0, grip, .6f).Apply(bottom).Y == -1, "Disabling sag changed the original mesh.");
        Point3 invalid = new SuspendedShape(float.NaN, new Point3(float.NaN, 0, 0), float.NaN).Apply(new Point3(float.NaN, 0, 0));
        Assert(Finite(invalid.X) && Finite(invalid.Y) && Finite(invalid.Z), "Malformed sag produced nonfinite geometry.");

        string path = "Assets/Pockle/Resources/Pip/Pip.pocklemesh";
        using var mesh = JsonDocument.Parse(File.ReadAllText(path));
        var positions = mesh.RootElement.GetProperty("positions");
        foreach (float clearance in new[] { 0f, .04f, .2f, .75f })
        foreach (float pinch in new[] { -.35f, 0, .55f })
        foreach (float compression in new[] { 0f, .42f })
        {
            var sag = new SuspendedShape(.24f, grip, clearance);
            var direction = new DirectionalStretch(pinch, new Point3(1, 1, .2f));
            for (int i = 0; i < positions.GetArrayLength(); i += 3)
            {
                var rest = new Point3(positions[i].GetSingle(), positions[i + 1].GetSingle(), positions[i + 2].GetSingle());
                Point3 deformed = direction.Apply(JellyShape.Deform(rest, compression, .3f, .15f, -.15f, .2f, -.2f));
                Point3 result = sag.Apply(deformed);
                Assert(Finite(result.X) && Finite(result.Y) && Finite(result.Z) && result.Y + clearance >= -1.00001f,
                    "Combined sag, pinch and squish escaped finite bounds or crossed the plate.");
                Assert(Math.Abs(result.X - deformed.X) < .12f && Math.Abs(result.Z - deformed.Z) < .12f,
                    "Suspension caused excessive lateral distortion.");
            }
        }
        Console.WriteLine("PASS: " + checks + " weight assertions (hand lag, analytic falls, impact timing, regrab, calm mode, and authored sag clearance).");
    }
    private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    private static void Near(float a, float b, float tolerance, string message) => Assert(Math.Abs(a - b) <= tolerance, message);
    private static void Assert(bool condition, string message)
    { checks++; if (!condition) throw new InvalidOperationException(message); }
}
