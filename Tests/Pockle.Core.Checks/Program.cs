using System;
using Pockle.Core;

internal static class Program
{
    private static int checks;

    private static int Main()
    {
        try
        {
            CheckSpringConvergenceAndStability();
            CheckSpringElapsedTimeAndReset();
            CheckSpringInvalidInputs();
            CheckRestShapeAndIdentity();
            CheckSquashStretchAndAnchor();
            CheckContactAndVolume();
            CheckFiniteDeformation();
            Console.WriteLine("PASS: " + checks + " core assertions (spring stability, timing, and jelly geometry).");
            AuthoredMeshChecks.Run();
            InteractionChecks.Run();
            ManipulationChecks.Run();
            RevealChecks.Run();
            CollectionChecks.Run();
            WeightChecks.Run();
            MenuChecks.Run();
            CatalogChecks.Run();
            MaterialHandlingChecks.Run();
            SeriesMotionChecks.Run();
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine("FAIL: " + exception.Message);
            return 1;
        }
    }

    private static void CheckSpringConvergenceAndStability()
    {
        foreach (float damping in new[] { .2f, .65f, 1f, 2f })
        {
            foreach (float step in new[] { 1f / 240f, 1f / 60f, 1f / 15f, .25f })
            {
                var spring = new Spring1D(3.5f, damping);
                spring.Target = 1f;
                for (int frame = 0; frame < (int)Math.Ceiling(8d / step); frame++)
                {
                    spring.Step(step);
                    Assert(Finite(spring.Value) && Finite(spring.Velocity), "Spring became nonfinite.");
                    Assert(spring.Value >= -.001f && spring.Value <= 2.001f, "Spring energy grew at low frame rate.");
                }
                Near(spring.Value, 1f, .0001f, "Spring did not converge.");
                Near(spring.Velocity, 0f, .0002f, "Spring did not settle.");
            }
        }

        var undamped = new Spring1D(2f, 0f);
        undamped.Target = 1f;
        for (int index = 0; index < 1000; index++)
            undamped.Step(.071f);
        double energy = (undamped.Value - 1d) * (undamped.Value - 1d) +
            undamped.Velocity * undamped.Velocity / (16d * Math.PI * Math.PI);
        Assert(Math.Abs(energy - 1d) < .0001d, "Undamped spring did not preserve its bounded energy.");
    }

    private static void CheckSpringElapsedTimeAndReset()
    {
        foreach (float damping in new[] { .65f, 1f, 2f })
        {
            var oneStep = new Spring1D(3.5f, damping, -.3f);
            var variable = new Spring1D(3.5f, damping, -.3f);
            oneStep.Target = variable.Target = .8f;
            float[] intervals = { .011f, .073f, .019f, .16f, .037f };
            float elapsed = 0f;
            foreach (float interval in intervals)
            {
                variable.Step(interval);
                elapsed += interval;
            }
            oneStep.Step(elapsed);
            Near(variable.Value, oneStep.Value, .000002f, "Variable frame timing changed spring position.");
            Near(variable.Velocity, oneStep.Velocity, .00003f, "Variable frame timing changed spring velocity.");
            variable.Step(20f);
            Near(variable.Value, variable.Target, .000001f, "A hitch did not consume elapsed time.");
            Near(variable.Velocity, 0f, .000001f, "A hitch left unstable velocity.");
            variable.Reset(-.75f);
            Near(variable.Value, -.75f, 0f, "Reset did not set position.");
            Near(variable.Target, -.75f, 0f, "Reset did not clear target.");
            Near(variable.Velocity, 0f, 0f, "Reset did not clear velocity.");
            variable.Step(.1f);
            Near(variable.Value, -.75f, 0f, "Reset spring moved without input.");
        }
    }

    private static void CheckSpringInvalidInputs()
    {
        var spring = new Spring1D(float.NaN, float.NegativeInfinity, float.PositiveInfinity);
        Near(spring.Value, 0f, 0f, "Invalid initial position was not repaired.");
        spring.Target = 1f;
        foreach (float invalid in new[] { 0f, -.1f, float.NaN, float.PositiveInfinity })
        {
            spring.Step(invalid);
            Near(spring.Value, 0f, 0f, "Invalid delta time mutated spring state.");
        }
        spring.Target = float.NaN;
        Near(spring.Target, 1f, 0f, "Invalid target replaced the last valid input.");
        spring.Step(float.MaxValue);
        Near(spring.Value, 1f, 0f, "An extreme hitch did not settle safely.");
        Assert(Finite(spring.Velocity), "An extreme hitch produced nonfinite velocity.");
        spring.Reset(float.NaN);
        Near(spring.Value, 0f, 0f, "Invalid reset was not repaired.");
    }

    private static void CheckRestShapeAndIdentity()
    {
        Point3 crown = JellyShape.Rest(0f, 0f);
        Point3 basePoint = JellyShape.Rest((float)Math.PI, 0f);
        Near(crown.Y, 1f, .000001f, "Rest crown height differs from the model contract.");
        Near(basePoint.Y, -1f, .000001f, "Rest base height differs from the model contract.");
        float maxX = 0f;
        float maxZ = 0f;
        for (int latitude = 0; latitude <= 32; latitude++)
        {
            for (int longitude = 0; longitude < 64; longitude++)
            {
                Point3 rest = JellyShape.Rest((float)Math.PI * latitude / 32f, (float)Math.PI * 2f * longitude / 64f);
                Point3 idle = JellyShape.Deform(rest, 0f, 0f, 0f, 0f, 0f, 0f);
                Near(idle.X, rest.X, .000001f, "Idle deformation moved X.");
                Near(idle.Y, rest.Y, .000001f, "Idle deformation moved Y.");
                Near(idle.Z, rest.Z, .000001f, "Idle deformation moved Z.");
                maxX = Math.Max(maxX, Math.Abs(rest.X));
                maxZ = Math.Max(maxZ, Math.Abs(rest.Z));
            }
        }
        Near(maxX, .85f, .015f, "Rest width is outside its specified range.");
        Near(maxZ, .72f, .015f, "Rest depth is outside its specified range.");
    }

    private static void CheckSquashStretchAndAnchor()
    {
        Point3 side = JellyShape.Rest((float)Math.PI * .5f, 0f);
        Point3 top = JellyShape.Rest(0f, 0f);
        Point3 squashed = JellyShape.Deform(side, .42f, 0f, 0f, 0f, 0f, 0f);
        Point3 stretched = JellyShape.Deform(side, 0f, .6f, 0f, 0f, 0f, 0f);
        Assert(squashed.X > side.X && squashed.Y < side.Y, "Squash did not widen and lower the body.");
        Assert(stretched.X < side.X && stretched.Y > side.Y, "Stretch did not narrow and raise the body.");
        Assert(JellyShape.Deform(top, .42f, 0f, 0f, 0f, 0f, 0f).Y < top.Y, "Squash did not lower the crown.");
        Near(JellyShape.Deform(top, 0f, .6f, 0f, 0f, 0f, 0f).Y, 2.2f, .000001f, "Stretch crown height is wrong.");
        Point3 anchor = new Point3(.1f, -1f, -.12f);
        Point3 result = JellyShape.Deform(anchor, .42f, .6f, 1f, -1f, .5f, -.5f);
        Near(result.X, anchor.X, 0f, "Base X moved.");
        Near(result.Y, -1f, 0f, "Base height moved.");
        Near(result.Z, anchor.Z, 0f, "Base Z moved.");
    }

    private static void CheckContactAndVolume()
    {
        Point3 crown = JellyShape.Rest(0f, 0f);
        Point3 centered = JellyShape.Deform(crown, .35f, 0f, 0f, 0f, 0f, 0f);
        Point3 distant = JellyShape.Deform(crown, .35f, 0f, 0f, 0f, .8f, .6f);
        Assert(centered.Y < distant.Y - .02f, "Press contact did not localize the dent.");
        Point3 tilted = JellyShape.Deform(crown, 0f, 0f, .7f, -.4f, 0f, 0f);
        Assert(tilted.X > 0f && tilted.Z < 0f, "Tilt did not follow both input axes.");

        double volume = MeshVolume(0f, 0f);
        Assert(volume > 2d && volume < 3d, "Rest mesh volume is implausible.");
        foreach (float[] input in new[] { new[] { .42f, 0f }, new[] { 0f, .6f }, new[] { .25f, .3f } })
        {
            double ratio = MeshVolume(input[0], input[1]) / volume;
            Assert(ratio > .90d && ratio < 1.10d, "Squash/stretch lost bulk volume: ratio " + ratio);
        }
    }

    private static double MeshVolume(float compression, float stretch)
    {
        const int rings = 32;
        const int segments = 64;
        double volume = 0d;
        for (int ring = 0; ring < rings; ring++)
        {
            for (int segment = 0; segment < segments; segment++)
            {
                Point3 a = Sample(ring, segment, compression, stretch);
                Point3 b = Sample(ring + 1, segment, compression, stretch);
                Point3 c = Sample(ring + 1, segment + 1, compression, stretch);
                Point3 d = Sample(ring, segment + 1, compression, stretch);
                volume += SignedTetrahedron(a, b, c) + SignedTetrahedron(a, c, d);
            }
        }
        return Math.Abs(volume);
    }

    private static Point3 Sample(int ring, int segment, float compression, float stretch)
    {
        Point3 rest = JellyShape.Rest((float)Math.PI * ring / 32f, (float)Math.PI * 2f * segment / 64f);
        return JellyShape.Deform(rest, compression, stretch, 0f, 0f, 0f, 0f);
    }

    private static double SignedTetrahedron(Point3 a, Point3 b, Point3 c)
    {
        return ((double)a.X * (b.Y * c.Z - b.Z * c.Y) +
                (double)a.Y * (b.Z * c.X - b.X * c.Z) +
                (double)a.Z * (b.X * c.Y - b.Y * c.X)) / 6d;
    }

    private static void CheckFiniteDeformation()
    {
        var random = new Random(418);
        for (int index = 0; index < 1000; index++)
        {
            Point3 rest = JellyShape.Rest((float)(random.NextDouble() * Math.PI), (float)(random.NextDouble() * Math.PI * 2d));
            Point3 result = JellyShape.Deform(rest, (float)random.NextDouble(), (float)random.NextDouble(),
                (float)(random.NextDouble() * 4d - 2d), (float)(random.NextDouble() * 4d - 2d),
                (float)(random.NextDouble() * 4d - 2d), (float)(random.NextDouble() * 4d - 2d));
            Assert(Finite(result.X) && Finite(result.Y) && Finite(result.Z), "Valid deformation became nonfinite.");
            Assert(result.Y >= -1.000001f && result.Y <= 2.200001f, "Deformation escaped vertical bounds.");
            Assert(Math.Abs(result.X) < 1.6f && Math.Abs(result.Z) < 1.4f, "Deformation escaped lateral bounds.");
        }
        Point3 invalid = JellyShape.Deform(new Point3(float.NaN, float.PositiveInfinity, float.NegativeInfinity),
            float.NaN, float.PositiveInfinity, float.NegativeInfinity, float.NaN, float.NaN, float.PositiveInfinity);
        Assert(Finite(invalid.X) && Finite(invalid.Y) && Finite(invalid.Z), "Malformed deformation was not repaired.");
        Point3 invalidRest = JellyShape.Rest(float.NaN, float.PositiveInfinity);
        Assert(Finite(invalidRest.X) && Finite(invalidRest.Y) && Finite(invalidRest.Z), "Malformed surface angles were not repaired.");
    }

    private static bool Finite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }

    private static void Near(float actual, float expected, float tolerance, string message)
    {
        Assert(Finite(actual) && Math.Abs(actual - expected) <= tolerance, message + " Actual: " + actual + "; expected: " + expected);
    }

    private static void Assert(bool condition, string message)
    {
        checks++;
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
