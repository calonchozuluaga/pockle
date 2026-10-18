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
            bool glow=ToyCatalog.TryGetCollectible(id,out var toy)&&toy.FinishId=="glow-jelly";
            bool glitter=toy!=null&&(toy.FinishId=="confetti-gel"||id=="fox.clear-jelly");
            Strength=light==StageLight.Night?(glow?.50f:glitter?.07f:0):glow?.08f:0;
            Color colour=toy!=null&&HolidayCatalog.IsCharacter(toy.CharacterId)?HolidayFinishMaterial.GlowTint(toy.CharacterId):glitter?new Color(1,.70f,.25f):toy!=null&&toy.CharacterId=="fox"?new Color(.65f,.44f,1):new Color(.25f,.88f,1);
            if(id==PipFusionStudies.EclipseId)colour=new Color(1,.65f,.23f);
            colour.a=Strength;haloMaterial.color=colour;colour.a=Strength*.8f;poolMaterial.color=colour;
            halo.gameObject.SetActive(Strength>0);pool.gameObject.SetActive(Strength>0);
        }
        private void LateUpdate()
        {
            if(halo!=null&&viewer!=null&&mount!=null)
            {
                halo.position=(body!=null?body.VisualRoot.position:mount.position)+viewer.transform.forward*.85f;
                halo.rotation=viewer.transform.rotation;
                halo.localScale=new Vector3(1.65f,1.65f,1);
            }
            if(pool!=null&&mount!=null)
            {
                pool.position=mount.position+new Vector3(0,-.996f,0);
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
