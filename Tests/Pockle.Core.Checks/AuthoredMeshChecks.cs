using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Pockle.Core;

internal static class AuthoredMeshChecks
{
    private static int assertions;
    private static void Check(bool pass, string label)
    {
        assertions++;
        if (!pass) throw new InvalidDataException(label);
    }

    internal static int Run()
    {
        string path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory,
            "../../../../../Assets/Pockle/Resources/Pip/Pip.pocklemesh"));
        using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
        JsonElement source = document.RootElement;
        Check(source.GetProperty("schemaVersion").GetInt32() == 1, "Authored mesh schema mismatch.");
        Check(source.GetProperty("integratedCrown").GetBoolean(), "Crown must share the deformable body surface.");
        float[] positions = Floats(source, "positions");
        float[] uv = Floats(source, "uv");
        int[] indices = Ints(source, "triangles");
        int[] groups = Ints(source, "normalGroups");
        Check(positions.Length % 3 == 0 && positions.Length >= 12, "Malformed mesh positions.");
        int count = positions.Length / 3;
        Check(count <= 4000, "Authored body exceeds the mobile vertex budget.");
        Check(count < 65000 && groups.Length == count && uv.Length == count * 2, "Mesh buffer dimensions mismatch.");
        Check(indices.Length > 0 && indices.Length % 3 == 0, "Malformed mesh indices.");
        Point3[] rest = new Point3[count];
        for (int i = 0; i < count; i++)
        {
            rest[i] = new Point3(positions[i * 3], positions[i * 3 + 1], positions[i * 3 + 2]);
            Check(Finite(rest[i]) && rest[i].Y >= -1 && rest[i].Y <= 1, "Mesh outside finite deformation range.");
            Check(groups[i] >= 0 && groups[i] < count, "Normal weld group invalid.");
        }
        foreach (float coordinate in uv) Check(!float.IsNaN(coordinate) && coordinate >= 0 && coordinate <= 1, "UV outside 0..1.");
        Dictionary<(int, int), (int count, int direction)> edges = new Dictionary<(int, int), (int count, int direction)>();
        int[] parents = new int[count];
        for (int i = 0; i < count; i++) parents[i] = i;
        for (int face = 0; face < indices.Length; face += 3)
        {
            for (int corner = 0; corner < 3; corner++) Check(indices[face + corner] >= 0 && indices[face + corner] < count, "Invalid vertex index.");
            Point3 a = rest[indices[face]], b = rest[indices[face + 1]], c = rest[indices[face + 2]];
            Point3 normal = Cross(Subtract(b, a), Subtract(c, a));
            Check(Dot(normal, normal) > 1e-12, "Degenerate authored triangle.");
            for (int corner = 0; corner < 3; corner++)
            {
                int from = groups[indices[face + corner]], to = groups[indices[face + (corner + 1) % 3]];
                parents[Root(parents, from)] = Root(parents, to);
                Check(from != to, "Collapsed logical mesh edge.");
                var key = (Math.Min(from, to), Math.Max(from, to));
                edges.TryGetValue(key, out var edge);
                edges[key] = (edge.count + 1, edge.direction + (from < to ? 1 : -1));
            }
        }
        foreach (var edge in edges.Values) Check(edge.count == 2 && edge.direction == 0, "Body is not a consistently wound closed surface.");
        int component = Root(parents, groups[0]);
        foreach (int group in groups)
            Check(Root(parents, group) == component, "Crown/body contains disconnected shells.");
        double restVolume = Volume(rest, indices);
        Check(restVolume > 1, "Body winding or volume incorrect.");
        Point3[] transformed = new Point3[count];
        foreach (var pose in new[] { (0f, 0f), (.42f, 0f), (0f, .6f), (.23f, .3f) })
        {
            for (int i = 0; i < count; i++)
            {
                transformed[i] = JellyShape.Deform(rest[i], pose.Item1, pose.Item2, .25f, -.25f, .4f, -.4f);
                Check(Finite(transformed[i]), "Authored deformation became nonfinite.");
                Check(1.15 + transformed[i].Y < 3.6, "Integrated body/crown exceeds viewer framing.");
                if (rest[i].Y == -1)
                    Check(transformed[i].X == rest[i].X && transformed[i].Y == -1 && transformed[i].Z == rest[i].Z,
                        "Authored flat base moved.");
                Point3 identity = JellyShape.Deform(rest[i], 0, 0, 0, 0, 0, 0);
                Check(Math.Abs(identity.X - rest[i].X) < 1e-6 && Math.Abs(identity.Y - rest[i].Y) < 1e-6 && Math.Abs(identity.Z - rest[i].Z) < 1e-6,
                    "Reset does not recover authored rest shape.");
            }
            double ratio = Volume(transformed, indices) / restVolume;
            Check(ratio > .88 && ratio < 1.12, "Authored deformation loses excessive bulk volume.");
        }
        float[] crowns = Floats(source, "crowns"), scales = Floats(source, "crownScales");
        Check(crowns.Length == 6 && scales.Length == 6, "Crown dimensions invalid.");
        for (int i = 0; i < 2; i++)
        {
            Point3 point = JellyShape.Deform(new Point3(crowns[i * 3], crowns[i * 3 + 1], crowns[i * 3 + 2]), 0, .6f, 0, 0, 0, 0);
            Check(1.15 + point.Y + scales[i * 3 + 1] * 1.6 < 3.6, "Authored crown exceeds viewer framing.");
        }
        Check(Floats(source, "eyes").Length == 6 && Floats(source, "mouth").Length == 39, "Facial anchors missing.");
        Console.WriteLine("PASS: " + assertions + " authored mesh assertions; " + count + " vertices / " + indices.Length / 3 + " triangles.");
        return assertions;
    }

    private static float[] Floats(JsonElement source, string key)
    {
        JsonElement array = source.GetProperty(key);
        float[] values = new float[array.GetArrayLength()];
        for (int i = 0; i < values.Length; i++) values[i] = array[i].GetSingle();
        return values;
    }
    private static int Root(int[] parents, int vertex)
    {
        while (parents[vertex] != vertex)
        {
            parents[vertex] = parents[parents[vertex]];
            vertex = parents[vertex];
        }
        return vertex;
    }
    private static int[] Ints(JsonElement source, string key)
    {
        JsonElement array = source.GetProperty(key);
        int[] values = new int[array.GetArrayLength()];
        for (int i = 0; i < values.Length; i++) values[i] = array[i].GetInt32();
        return values;
    }
    private static bool Finite(Point3 v) { return !float.IsNaN(v.X + v.Y + v.Z) && !float.IsInfinity(v.X + v.Y + v.Z); }
    private static Point3 Subtract(Point3 a, Point3 b) { return new Point3(a.X - b.X, a.Y - b.Y, a.Z - b.Z); }
    private static Point3 Cross(Point3 a, Point3 b) { return new Point3(a.Y * b.Z - a.Z * b.Y, a.Z * b.X - a.X * b.Z, a.X * b.Y - a.Y * b.X); }
    private static double Dot(Point3 a, Point3 b) { return (double)a.X * b.X + (double)a.Y * b.Y + (double)a.Z * b.Z; }
    private static double Volume(Point3[] vertices, int[] indices)
    {
        double volume = 0;
        for (int i = 0; i < indices.Length; i += 3) volume += Dot(vertices[indices[i]], Cross(vertices[indices[i + 1]], vertices[indices[i + 2]])) / 6;
        return volume;
    }
}
