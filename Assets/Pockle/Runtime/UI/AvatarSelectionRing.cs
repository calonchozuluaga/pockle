using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>Selection decorates the original crop; it never changes the photo's geometry.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class AvatarSelectionRing : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear(); var r = rectTransform.rect; float radius = Mathf.Min(r.width, r.height) / 2 - 1;
            for (int i = 0; i <= 64; i++)
            {
                float a = i * Mathf.PI * 2 / 64; var direction = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                mesh.AddVert(r.center + direction * radius, color, Vector2.zero);
                mesh.AddVert(r.center + direction * Mathf.Max(0, radius - 3), color, Vector2.zero);
                if (i == 0) continue; int v = i * 2;
                mesh.AddTriangle(v - 2, v - 1, v); mesh.AddTriangle(v, v - 1, v + 1);
            }
        }
    }
}
