using UnityEngine;
using UnityEngine.UI;
namespace Pockle.Runtime
{
    /// <summary>A neutral branded carton teaser before the server assigns a collection.</summary>
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class MysteryCartonGraphic : MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear(); var r = rectTransform.rect;
            Quad(mesh, r, new Vector2(.14f,.13f),new Vector2(.70f,.13f),new Vector2(.70f,.70f),new Vector2(.14f,.70f),PockleTheme.Milk);
            Quad(mesh, r, new Vector2(.70f,.13f),new Vector2(.90f,.29f),new Vector2(.90f,.86f),new Vector2(.70f,.70f),PockleTheme.JellyInk);
            Quad(mesh, r, new Vector2(.14f,.70f),new Vector2(.70f,.70f),new Vector2(.90f,.86f),new Vector2(.34f,.86f),PockleTheme.Jelly);
        }
        private static void Quad(VertexHelper mesh, Rect r, Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color color)
        {
            int index=mesh.currentVertCount;
            foreach(var p in new[]{a,b,c,d}) mesh.AddVert(new Vector3(r.xMin+p.x*r.width,r.yMin+p.y*r.height),color,Vector2.zero);
            mesh.AddTriangle(index,index+1,index+2);mesh.AddTriangle(index,index+2,index+3);
        }
    }
}
