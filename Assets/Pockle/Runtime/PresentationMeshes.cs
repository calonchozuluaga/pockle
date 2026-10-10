using UnityEngine;

namespace Pockle.Runtime
{
    /// <summary>Presentation geometry without CreatePrimitive's implicit physics components.</summary>
    internal static class PresentationMeshes
    {
        // Match Unity's unit cylinder: radius .5, height 2. Split cap normals.
        public static Mesh CreateDisk()
        {
            const int sides = 32;
            var vertices = new Vector3[sides * 4 + 2];
            var normals = new Vector3[vertices.Length];
            var uv = new Vector2[vertices.Length];
            var triangles = new int[sides * 12];
            int topCenter = sides * 4, bottomCenter = topCenter + 1;
            vertices[topCenter] = Vector3.up; vertices[bottomCenter] = Vector3.down;
            normals[topCenter] = Vector3.up; normals[bottomCenter] = Vector3.down;
            uv[topCenter] = uv[bottomCenter] = new Vector2(.5f, .5f);
            for (int i = 0; i < sides; i++)
            {
                float angle = i * Mathf.PI * 2f / sides;
                var radial = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                vertices[i * 2] = radial * .5f + Vector3.up;
                vertices[i * 2 + 1] = radial * .5f + Vector3.down;
                normals[i * 2] = normals[i * 2 + 1] = radial;
                uv[i * 2] = new Vector2(i / (float)sides, 1f);
                uv[i * 2 + 1] = new Vector2(i / (float)sides, 0f);
                int top = sides * 2 + i, bottom = sides * 3 + i;
                vertices[top] = vertices[i * 2]; vertices[bottom] = vertices[i * 2 + 1];
                normals[top] = Vector3.up; normals[bottom] = Vector3.down;
                uv[top] = uv[bottom] = new Vector2(radial.x * .5f + .5f, radial.z * .5f + .5f);
                int next = (i + 1) % sides, offset = i * 12;
                triangles[offset] = i * 2; triangles[offset + 1] = next * 2; triangles[offset + 2] = i * 2 + 1;
                triangles[offset + 3] = next * 2; triangles[offset + 4] = next * 2 + 1; triangles[offset + 5] = i * 2 + 1;
                triangles[offset + 6] = topCenter; triangles[offset + 7] = sides * 2 + next; triangles[offset + 8] = top;
                triangles[offset + 9] = bottomCenter; triangles[offset + 10] = bottom; triangles[offset + 11] = sides * 3 + next;
            }
            var mesh = new Mesh { name = "Pockle presentation disk", vertices = vertices, normals = normals, uv = uv, triangles = triangles };
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh CreateBox()
        {
            var vertices = new Vector3[24];
            var uv = new Vector2[24];
            var triangles = new int[36];
            var faces = new[] { Vector3.forward, Vector3.back, Vector3.up, Vector3.down, Vector3.right, Vector3.left };
            for (int face = 0; face < faces.Length; face++)
            {
                Vector3 normal = faces[face];
                Vector3 across = face < 2 ? Vector3.right : Vector3.forward;
                Vector3 along = Vector3.Cross(normal, across);
                int first = face * 4;
                vertices[first] = (normal - across - along) * .5f;
                vertices[first + 1] = (normal + across - along) * .5f;
                vertices[first + 2] = (normal + across + along) * .5f;
                vertices[first + 3] = (normal - across + along) * .5f;
                uv[first] = Vector2.zero; uv[first + 1] = Vector2.right;
                uv[first + 2] = Vector2.one; uv[first + 3] = Vector2.up;
                int offset = face * 6;
                triangles[offset] = first; triangles[offset + 1] = first + 1; triangles[offset + 2] = first + 2;
                triangles[offset + 3] = first; triangles[offset + 4] = first + 2; triangles[offset + 5] = first + 3;
            }
            var mesh = new Mesh { name = "Pockle carton floor", vertices = vertices, uv = uv, triangles = triangles };
            mesh.RecalculateNormals(); mesh.RecalculateBounds();
            return mesh;
        }
    }
}
