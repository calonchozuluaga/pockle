using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>A single, non-overlapping silhouette: the body and pointer share one fade.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class SpeechBubbleGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear(); var r = rectTransform.rect;
            float radius = Mathf.Min(20, Mathf.Min(r.width, r.height) / 2);
            mesh.AddVert(r.center, color, Vector2.zero);
            Point(mesh, r.xMin + radius, r.yMin);
            Point(mesh, r.center.x - 7, r.yMin);
            Point(mesh, r.center.x, r.yMin - 9);
            Point(mesh, r.center.x + 7, r.yMin);
            Arc(mesh, new Vector2(r.xMax - radius, r.yMin + radius), radius, -90);
            Arc(mesh, new Vector2(r.xMax - radius, r.yMax - radius), radius, 0);
            Arc(mesh, new Vector2(r.xMin + radius, r.yMax - radius), radius, 90);
            Arc(mesh, new Vector2(r.xMin + radius, r.yMin + radius), radius, 180);
            int count = mesh.currentVertCount - 1;
            for (int i = 1; i <= count; i++) mesh.AddTriangle(0, i, i == count ? 1 : i + 1);
        }
        private void Point(VertexHelper mesh, float x, float y) => mesh.AddVert(new Vector3(x, y), color, Vector2.zero);
        private void Arc(VertexHelper mesh, Vector2 center, float radius, float start)
        {
            for (int i = 0; i <= 8; i++)
            {
                float a = (start + i * 90f / 8) * Mathf.Deg2Rad;
                Point(mesh, center.x + Mathf.Cos(a) * radius, center.y + Mathf.Sin(a) * radius);
            }
        }
    }
}
