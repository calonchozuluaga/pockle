using System;
using System.IO;
using System.Text.Json;
using Pockle.Core;

internal static class ManipulationChecks
{
    private static int assertions;
    private static void Check(bool value, string message)
    {
        assertions++;
        if (!value) throw new InvalidDataException(message);
    }

    internal static void Run()
    {
        CheckCaptureTransitions();
        CheckSensorFiltering();
        CheckDirectionalMesh();
        Console.WriteLine("PASS: " + assertions + " manipulation assertions (two-finger handoff, gravity filtering, and directional mesh volume).");
    }

    private static void CheckCaptureTransitions()
    {
        var gesture = new PointerGesture();
        Check(!gesture.TryJoin(2, PointerTarget.Toy), "A lone second finger captured a pair.");
        Check(gesture.TryBegin(1, PointerTarget.Plate), "Plate did not capture.");
        Check(!gesture.TryJoin(2, PointerTarget.Toy) && gesture.Target == PointerTarget.Plate,
            "A plate gesture upgraded into a toy pinch.");
        gesture.Cancel();
        foreach (bool primaryLeaves in new[] { true, false })
        {
            Check(gesture.TryBegin(7, PointerTarget.Toy), "Toy failed to begin.");
            Check(!gesture.TryJoin(8, PointerTarget.Plate), "A touch starting on the plate joined a pinch.");
            Check(!gesture.TryJoin(7, PointerTarget.Toy), "The primary finger joined itself.");
            Check(gesture.TryJoin(8, PointerTarget.Toy) && gesture.IsPair, "Second toy touch failed to join.");
            Check(!gesture.TryJoin(9, PointerTarget.Toy), "A third finger replaced a captured finger.");
            Check(!gesture.End(9) && gesture.IsPair, "An unrelated release ended the pair.");
            Check(gesture.End(primaryLeaves ? 7 : 8), "Captured finger failed to release.");
            Check(!gesture.IsPair && gesture.PointerId == (primaryLeaves ? 8 : 7) && gesture.Target == PointerTarget.Toy,
                "Release failed to hand control to the surviving finger.");
            Check(gesture.TryJoin(10, PointerTarget.Toy), "Survivor could not start another pair.");
            gesture.Cancel();
            Check(gesture.PointerId == PointerGesture.NoPointer && gesture.SecondPointerId == PointerGesture.NoPointer,
                "Lifecycle cancellation left a captured finger.");
        }
    }

    private static void CheckSensorFiltering()
    {
        foreach (int fps in new[] { 15, 30, 60, 120 })
        {
            var filter = new MotionJiggle();
            var spring = new Spring1D(ToyFeel.ShakeFrequency, ToyFeel.ShakeDamping);
            Point3 drive = filter.Step(new Point3(0f, -1f, 0f), 1f / fps);
            Check(Length(drive) == 0d, "Initial gravity caused a shake.");
            for (int i = 0; i < fps * 2; i++)
            {
                drive = filter.Step(new Point3((float)Math.Sin(i) * .03f, -1f, 0f), 1f / fps);
                Check(Length(drive) == 0d, "Stationary sensor noise caused a jiggle.");
            }
            drive = filter.Step(new Point3(2f, -1f, 0f), 1f / fps);
            Check(drive.X > .1f && Math.Abs(drive.Y) < .00001f, "Shake direction or gravity rejection is wrong.");
            for (int i = 0; i < fps * 2; i++)
            {
                drive = filter.Step(new Point3(i % 2 == 0 ? float.MaxValue : -float.MaxValue, -1f, 0f), 1f / fps);
                Check(Length(drive) <= ToyFeel.ShakeLimit + .000001d, "Sensor spike escaped the drive bound.");
                spring.Target = drive.X;
                spring.Step(1f / fps);
                Check(Math.Abs(spring.Value) < .6f, "Sustained shaking grew spring motion without bound.");
            }
            for (int i = 0; i < fps * 4; i++)
            {
                drive = filter.Step(new Point3(0f, -1f, 0f), 1f / fps);
                spring.Target = drive.X;
                spring.Step(1f / fps);
            }
            Check(Math.Abs(spring.Value) < .001f && Length(drive) == 0d, "Shake failed to settle.");
            Check(Length(filter.Step(new Point3(float.NaN, 0f, 0f), 1f / fps)) == 0d,
                "Malformed acceleration poisoned the filter.");
            Check(Length(filter.Step(new Point3(1f, 0f, 0f), 1f / fps)) == 0d,
                "Filter recovery replayed a stale gravity vector.");
            filter.Reset();
            Check(Length(filter.Step(new Point3(0f, 1f, 0f), 1f / fps)) == 0d,
                "Reset/orientation change caused an initial shake.");
        }
        var gentle = new MotionJiggle();
        gentle.Step(new Point3(0, -1, 0), 1f / 60);
        Point3 response = gentle.Step(new Point3(.5f, -1, 0), 1f / 60);
        Check(response.X > .06f && response.X < .09f, "A small deliberate shake lost its more pronounced response.");
    }

    private static void CheckDirectionalMesh()
    {
        string path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "../../../../../Assets/Pockle/Resources/Pip/Pip.pocklemesh"));
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
        JsonElement positions = document.RootElement.GetProperty("positions");
        JsonElement faces = document.RootElement.GetProperty("triangles");
        var rest = new Point3[positions.GetArrayLength() / 3];
        var result = new Point3[rest.Length];
        var indices = new int[faces.GetArrayLength()];
        for (int i = 0; i < rest.Length; i++) rest[i] = new Point3(positions[i * 3].GetSingle(), positions[i * 3 + 1].GetSingle(), positions[i * 3 + 2].GetSingle());
        for (int i = 0; i < indices.Length; i++) indices[i] = faces[i].GetInt32();
        double baseline = Volume(rest, indices);
        foreach (Point3 axis in new[] { new Point3(1f, 0f, 0f), new Point3(0f, 1f, 0f),
            new Point3(0f, 0f, 1f), new Point3(1f, 1f, 0f), new Point3(-1f, 1f, .3f) })
        foreach (float amount in new[] { -.35f, 0f, .55f })
        {
            var deform = new DirectionalStretch(amount, axis);
            var reversed = new DirectionalStretch(amount, new Point3(-axis.X, -axis.Y, -axis.Z));
            for (int i = 0; i < rest.Length; i++)
            {
                result[i] = deform.Apply(rest[i]);
                Point3 opposite = reversed.Apply(rest[i]);
                Check(Finite(result[i]) && result[i].Y >= -1f && Math.Abs(result[i].X) < 2.5f && result[i].Y < 2.5f,
                    "Directional pinch escaped mesh bounds.");
                Check(Distance(result[i], opposite) < .000001d, "Finger ordering flipped deformation.");
                if (rest[i].Y <= -1f || amount == 0f)
                    Check(Distance(rest[i], result[i]) < .000001d, "Pinch moved the foot or failed identity.");
            }
            double ratio = Volume(result, indices) / baseline;
            Check(ratio > .85d && ratio < 1.15d, "Pinch lost bulk volume: " + ratio);
            // The runtime composes directional pinch with the existing press/wobble.
            foreach (float pressure in new[] { .23f, .42f })
            {
                bool bounded = true;
                for (int i = 0; i < rest.Length; i++)
                {
                    result[i] = deform.Apply(JellyShape.Deform(rest[i], pressure, .12f, .2f, -.2f, .1f, -.1f));
                    bounded &= Finite(result[i]) && result[i].Y >= -1f && result[i].Y < 3f && Math.Abs(result[i].X) < 2.5f;
                }
                Check(bounded, "Combined press, pinch and wobble escaped finite bounds.");
                ratio = Volume(result, indices) / baseline;
                Check(ratio > .80d && ratio < 1.20d, "Combined deformation lost bulk volume: " + ratio);
            }
        }
        Point3 side = new Point3(.8f, 0f, 0f);
        Check(new DirectionalStretch(.5f, new Point3(1f, 0f, 0f)).Apply(side).X > side.X,
            "Horizontal pull failed to widen Pip.");
        Check(new DirectionalStretch(-.3f, new Point3(1f, 0f, 0f)).Apply(side).X < side.X,
            "Horizontal pinch failed to narrow Pip.");
        Check(Finite(new DirectionalStretch(float.NaN, new Point3(float.PositiveInfinity, 0f, 0f)).Apply(side)),
            "Invalid pinch input produced a nonfinite point.");
    }

    private static bool Finite(Point3 p) { return !float.IsNaN(p.X + p.Y + p.Z) && !float.IsInfinity(p.X + p.Y + p.Z); }
    private static double Length(Point3 p) { return Math.Sqrt((double)p.X * p.X + (double)p.Y * p.Y + (double)p.Z * p.Z); }
    private static double Distance(Point3 a, Point3 b) { return Length(new Point3(a.X - b.X, a.Y - b.Y, a.Z - b.Z)); }
    private static double Volume(Point3[] points, int[] faces)
    {
        double volume = 0d;
        for (int i = 0; i < faces.Length; i += 3)
        {
            Point3 a = points[faces[i]], b = points[faces[i + 1]], c = points[faces[i + 2]];
            volume += (double)a.X * (b.Y * c.Z - b.Z * c.Y) +
                (double)a.Y * (b.Z * c.X - b.X * c.Z) + (double)a.Z * (b.X * c.Y - b.Y * c.X);
        }
        return Math.Abs(volume / 6d);
    }
}
