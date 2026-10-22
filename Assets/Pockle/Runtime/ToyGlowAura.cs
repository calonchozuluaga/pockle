using Pockle.Core;
using UnityEngine;
using UnityEngine.Rendering;
namespace Pockle.Runtime
{
    /// <summary>Two bounded additive cards emulate light spill without post-processing or dynamic shadows.</summary>
    [DefaultExecutionOrder(200)]
    public sealed class ToyGlowAura : MonoBehaviour
    {
        public float Strength { get; private set; }
        private Transform halo, pool, mount;
        private Camera viewer;
        private JellyToy body;
        private Material haloMaterial,poolMaterial;
        private Mesh quad;
        public void Initialize(Transform toyMount,Camera camera)
        {
            mount=toyMount;viewer=camera;body=mount.GetComponentInChildren<JellyToy>(true);
            quad=new Mesh{name="Toy glow quad",vertices=new[]{new Vector3(-1,-1,0),new Vector3(1,-1,0),new Vector3(-1,1,0),new Vector3(1,1,0)},
                uv=new[]{Vector2.zero,Vector2.right,Vector2.up,Vector2.one},triangles=new[]{0,2,1,1,2,3}};
            quad.RecalculateBounds();var shader=Shader.Find("Pockle/Toy Glow");
            haloMaterial=new Material(shader){name="Toy halo"};poolMaterial=new Material(shader){name="Toy ground light spill"};
            halo=Make("Toy emitted halo",haloMaterial,mount);
            halo.localPosition=new Vector3(0,0,.75f);halo.localScale=new Vector3(1.65f,1.65f,1);
            pool=Make("Toy emitted ground light",poolMaterial,mount.parent.parent);
            pool.localPosition=new Vector3(0,.154f,0);pool.localRotation=Quaternion.Euler(90,0,0);pool.localScale=new Vector3(1.16f,1.16f,1);
            Configure("",StageLight.Day);
        }
        private Transform Make(string name,Material material,Transform parent)
        {
            var obj=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));obj.transform.SetParent(parent,false);
            obj.GetComponent<MeshFilter>().sharedMesh=quad;var renderer=obj.GetComponent<MeshRenderer>();renderer.sharedMaterial=material;
            renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;return obj.transform;
        }
        public void Configure(string id,StageLight light)
        {
            bool glow=ToyCatalog.TryGetCollectible(id,out var toy)&&(toy.FinishId=="glow-jelly"||WallyCatalog.EmitsLight(id));
            bool glitter=toy!=null&&(toy.FinishId=="confetti-gel"||id=="fox.clear-jelly");
            string appearance = FusionMaterial.AppearanceId(id);
            if (FusionCatalog.TryGet(id, out var fused))
            {
                ToyCatalog.TryGetCollectible(fused.First, out var first); ToyCatalog.TryGetCollectible(fused.Second, out var second);
                glow = first.FinishId == "glow-jelly" || second.FinishId == "glow-jelly" || appearance == PipFusionStudies.EclipseId;
                glitter = first.FinishId == "confetti-gel" || second.FinishId == "confetti-gel";
            }
            Strength=light==StageLight.Night?(glow?.50f:glitter?.07f:0):glow?.08f:0;
            Color colour=toy!=null&&HolidayCatalog.IsCharacter(toy.CharacterId)?HolidayFinishMaterial.GlowTint(toy.CharacterId):glitter?new Color(1,.70f,.25f):toy!=null&&toy.CharacterId=="fox"?new Color(.65f,.44f,1):new Color(.25f,.88f,1);
            if(appearance==PipFusionStudies.EclipseId)colour=new Color(1,.65f,.23f);
            colour.a=Strength*.22f;haloMaterial.color=colour;colour.a=Strength*.8f;poolMaterial.color=colour;
            halo.gameObject.SetActive(Strength>0);pool.gameObject.SetActive(Strength>0);
        }
        private void LateUpdate()
        {
            if(halo!=null&&viewer!=null&&mount!=null)
            {
                Bounds bounds = body != null && body.BodyRenderer != null
                    ? body.SurfaceBounds : new Bounds(mount.position, Vector3.one * 2);
                Vector3 right = viewer.transform.right, up = viewer.transform.up, forward = viewer.transform.forward;
                Vector3 extents = bounds.extents;
                float width = Vector3.Dot(new Vector3(Mathf.Abs(right.x), Mathf.Abs(right.y), Mathf.Abs(right.z)), extents);
                float height = Vector3.Dot(new Vector3(Mathf.Abs(up.x), Mathf.Abs(up.y), Mathf.Abs(up.z)), extents);
                float depth = Vector3.Dot(new Vector3(Mathf.Abs(forward.x), Mathf.Abs(forward.y), Mathf.Abs(forward.z)), extents);
                // The quad stays behind the entire shell at every orbit angle, centred on the visible body.
                halo.position = bounds.center + forward * (depth + .08f);
                halo.rotation = viewer.transform.rotation;
                Vector3 parentScale = mount.lossyScale;
                halo.localScale = new Vector3(Mathf.Max(.05f, width) * 1.3f / Mathf.Max(.001f, Mathf.Abs(parentScale.x)),
                    Mathf.Max(.05f, height) * 1.3f / Mathf.Max(.001f, Mathf.Abs(parentScale.y)), 1);
            }
            if(pool!=null&&mount!=null)
            {
                Vector3 centre = body != null && body.BodyRenderer != null ? body.SurfaceBounds.center : mount.position;
                pool.position = new Vector3(centre.x, mount.position.y - .996f, centre.z);
                pool.rotation=Quaternion.Euler(90,0,0);
                pool.localScale=new Vector3(1.16f,1.16f,1);
            }
        }
        private void OnDestroy()
        {
            if(halo!=null)Destroy(halo.gameObject);if(pool!=null)Destroy(pool.gameObject);
            if(quad!=null)Destroy(quad);if(haloMaterial!=null)Destroy(haloMaterial);if(poolMaterial!=null)Destroy(poolMaterial);
        }
    }
}
