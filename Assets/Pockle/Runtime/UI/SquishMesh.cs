using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>Scales one graphic's vertices around a shared world-space centre.</summary>
    [DisallowMultipleComponent]
    public sealed class SquishMesh : BaseMeshEffect
    {
        private Vector3 worldCentre;
        private float scale = 1f;

        public void Set(Vector3 centre, float value)
        {
            if (Mathf.Approximately(value, scale) && centre == worldCentre) return;
            worldCentre = centre; scale = value;
            if (graphic != null) graphic.SetVerticesDirty();
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive() || Mathf.Approximately(scale, 1f)) return;
            Vector3 centre = graphic.rectTransform.InverseTransformPoint(worldCentre);
            var vertex = new UIVertex();
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vertex, i);
                vertex.position = centre + (vertex.position - centre) * scale;
                vh.SetUIVertex(vertex, i);
            }
        }
    }
}
