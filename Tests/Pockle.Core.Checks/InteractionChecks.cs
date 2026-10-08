using System;
using System.IO;
using System.Text.Json;
using Pockle.Core;

internal static class InteractionChecks
{
    private static int assertions;
    private static void Check(bool pass, string message)
    {
        assertions++;
        if (!pass) throw new InvalidDataException(message);
    }

    internal static void Run()
    {
        var gesture = new PointerGesture();
        Check(!gesture.TryBegin(0, PointerTarget.None), "Background captured a gesture.");
        foreach (int first in new[] { -1, 0, 7 })
        foreach (PointerTarget target in new[] { PointerTarget.Toy, PointerTarget.Plate })
        {
            Check(gesture.TryBegin(first, target), "Valid touch failed to begin.");
            for (int i = 0; i < 20; i++)
            {
                Check(!gesture.TryBegin(first, target == PointerTarget.Toy ? PointerTarget.Plate : PointerTarget.Toy),
                    "Crossing surfaces switched gesture target.");
                Check(!gesture.TryBegin(first + 10, PointerTarget.Plate), "Second finger stole gesture.");
                Check(!gesture.End(first + 10), "Second finger ended gesture.");
                Check(gesture.Target == target && gesture.PointerId == first, "Capture changed before release.");
            }
            Check(gesture.End(first) && !gesture.IsActive, "Release did not clear capture.");
            Check(gesture.TryBegin(first, PointerTarget.Plate), "New surface failed after release.");
            gesture.Cancel();
            Check(gesture.PointerId == PointerGesture.NoPointer && !gesture.IsActive, "Lifecycle cancellation left a capture.");
        }

        Point3 a = new Point3(-1, -1, 0), b = new Point3(0, 1, 0), c = new Point3(1, -1, 0);
        float distance;
        Check(SurfaceRaycast.TryTriangle(new Point3(0, 0, -2), new Point3(0, 0, 1), a, b, c, out distance)
            && Math.Abs(distance - 2) < 1e-6, "Front-face pick has wrong depth.");
        Check(!SurfaceRaycast.TryTriangle(new Point3(0, 0, 2), new Point3(0, 0, -1), a, b, c, out distance), "Back face was picked.");
        Check(!SurfaceRaycast.TryTriangle(new Point3(2, 0, -2), new Point3(0, 0, 1), a, b, c, out distance), "Outside triangle was picked.");
        Check(!SurfaceRaycast.TryTriangle(new Point3(0, 0, -2), new Point3(1, 0, 0), a, b, c, out distance), "Parallel ray was picked.");
        Check(!SurfaceRaycast.TryTriangle(new Point3(0, 0, -2), new Point3(0, 0, 1), a, a, a, out distance), "Degenerate triangle was picked.");
        Check(!SurfaceRaycast.TryTriangle(new Point3(float.NaN, 0, -2), new Point3(0, 0, 1), a, b, c, out distance), "Invalid ray was picked.");

        string path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "../../../../../Assets/Pockle/Resources/Pip/Pip.pocklemesh"));
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
        JsonElement positions = document.RootElement.GetProperty("positions");
        JsonElement triangles = document.RootElement.GetProperty("triangles");
        Point3[] rest = new Point3[positions.GetArrayLength() / 3];
        for (int i = 0; i < rest.Length; i++)
            rest[i] = new Point3(positions[i * 3].GetSingle(), positions[i * 3 + 1].GetSingle(), positions[i * 3 + 2].GetSingle());
        int[] indices = new int[triangles.GetArrayLength()];
        for (int i = 0; i < indices.Length; i++) indices[i] = triangles[i].GetInt32();
        Point3[] vertices = new Point3[rest.Length];
        foreach (var pose in new[] { (0f, 0f), (.42f, 0f), (0f, .6f) })
        foreach (bool rotated in new[] { false, true })
        {
            for (int i = 0; i < rest.Length; i++)
            {
                Point3 p = JellyShape.Deform(rest[i], pose.Item1, pose.Item2, 0, 0, 0, 0);
                vertices[i] = rotated ? new Point3(p.Z, p.Y, -p.X) : p;
            }
            float depth = Pick(vertices, indices, new Point3(0, -.4f, -3), new Point3(0, 0, 1));
            Check(depth > 1 && depth < 3, "Deformed/rotated shell failed front pick.");
            depth = Pick(vertices, indices, new Point3(0, -.4f, 3), new Point3(0, 0, -1));
            Check(depth > 1 && depth < 3, "Deformed/rotated shell failed back pick.");
            Check(float.IsInfinity(Pick(vertices, indices, new Point3(1.5f, -.4f, -3), new Point3(0, 0, 1))),
                "Plate rim outside the shell was mistaken for Pip.");
        }
        Check(float.IsInfinity(Pick(rest, indices, new Point3(.94f, -.4f, -3), new Point3(0, 0, 1))),
            "Old spherical hit region captured empty space beside the visible shell.");
        Check(Pick(rest, indices, new Point3(.095f, .85f, -3), new Point3(0, 0, 1)) < 3,
            "Integrated crown cannot be pressed.");
        Console.WriteLine("PASS: " + assertions + " interaction assertions (capture, cancellation, and visible-shell picking).");
    }

    private static float Pick(Point3[] vertices, int[] indices, Point3 origin, Point3 direction)
    {
        float nearest = float.PositiveInfinity;
        for (int i = 0; i < indices.Length; i += 3)
            if (SurfaceRaycast.TryTriangle(origin, direction, vertices[indices[i]], vertices[indices[i + 1]],
                vertices[indices[i + 2]], out float distance) && distance < nearest) nearest = distance;
        return nearest;
    }
}
