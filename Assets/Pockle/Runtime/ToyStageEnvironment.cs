using System;
using System.Collections.Generic;
using Pockle.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pockle.Runtime
{
    /// <summary>Bounded local scenery. No colliders, real-time shadows, network or inventory changes.</summary>
    public sealed class ToyStageEnvironment : MonoBehaviour
    {
        public StageWorld World { get; private set; } = StageWorld.Studio;
        public StageLight Lighting { get; private set; } = StageLight.Day;
        public bool Calm { get; set; }
        public string HomeWorld { get; private set; } = "studio";
        private Camera viewer;
        private Transform toyMount, scenery;
        private Light key, fill;
        private Material plateBase, plateTop, cards;
        private readonly List<Mesh> meshes = new List<Mesh>();
        private readonly List<Transform> leaves = new List<Transform>();
        private readonly List<Vector3> leafRest = new List<Vector3>();
        private string collectibleId = "";
        private float elapsed;
        private Mesh gradient;
        private Transform gradientPlane;
        private Camera background;

        public void Initialize(Camera camera, Transform mount, Light keyLight, Light fillLight, Material lower, Material upper)
        {
            viewer = camera; toyMount = mount; key = keyLight; fill = fillLight; plateBase = lower; plateTop = upper;
            cards = new Material(Shader.Find("Pockle/Stage Card")) { name = "Stage · unlit scenery" };
            // Shelf portraits use their own layer and lights.
            key.cullingMask = fill.cullingMask = ~(1 << 30);
            background=transform.parent.Find("Full-screen background").GetComponent<Camera>();
            background.orthographic=true; background.orthographicSize=1; background.nearClipPlane=.1f; background.farClipPlane=3;
            background.cullingMask=1<<29; viewer.cullingMask &= ~(1<<29);
            gradient=new Mesh{name="Stage gradient"};
            gradient.vertices=new[]{new Vector3(-1,-1,0),new Vector3(1,-1,0),new Vector3(-1,1,0),new Vector3(1,1,0)};
            gradient.triangles=new[]{0,2,1,1,2,3}; gradient.RecalculateBounds();
            var plane=new GameObject("World gradient",typeof(MeshFilter),typeof(MeshRenderer));plane.layer=29;
            gradientPlane=plane.transform;gradientPlane.SetParent(background.transform,false);gradientPlane.localPosition=Vector3.forward;
            plane.GetComponent<MeshFilter>().sharedMesh=gradient;plane.GetComponent<MeshRenderer>().sharedMaterial=cards;
            plane.GetComponent<MeshRenderer>().shadowCastingMode=ShadowCastingMode.Off;
            plane.SetActive(false);

        }

        public bool SetWorld(StageWorld world, string id)
        {
            if (!Enum.IsDefined(typeof(StageWorld), world) || !ToyCatalog.TryGetCollectible(id, out var toy)) return false;
            if (World == world && collectibleId == id && scenery != null) return true;
            World = world; collectibleId = id;
            gradientPlane.gameObject.SetActive(true);
            HomeWorld = ToyCatalog.TryGetCollection(toy.CollectionId, out var collection) ? collection.HomeWorld : "studio";
            ClearScenery(); elapsed = 0;
            scenery = new GameObject("Stage world · " + world).transform;
            scenery.SetParent(transform.parent, false);
            bool hollow = world == StageWorld.Home && HomeWorld == "fox-hollow";
            Color rim = new Color(.91f,.857f,.79f), top = new Color(1,.985f,.957f);
            if (hollow)
            {
                rim = new Color(.44f,.27f,.19f); top = new Color(.72f,.49f,.30f);
                for (int i = 0; i < 8; i++)
                {
                    float x = (i - 3.5f) * .67f;
                    Pine(new Vector3(x, .22f + (i % 3) * .12f, 2.4f), .7f + (i % 3) * .3f,
                        i % 2 == 0 ? new Color(.38f,.51f,.39f) : new Color(.51f,.62f,.46f));
                }
                for (int i = 0; i < 7; i++)
                {
                    var leaf = Card("Falling leaf", new Vector3((i-3)*.66f, 1.1f+(i%3)*.48f, 1.9f), .07f,.13f,
                        new Color(.81f,.43f,.21f), 4);
                    leaves.Add(leaf); leafRest.Add(leaf.localPosition);
                }
                for (int i = 1; i <= 5; i++) Ring("Stump growth ring", .19f*i, .1515f, new Color(.55f,.34f,.22f), .012f);
            }
            else if (world == StageWorld.Garden)
            {
                rim = new Color(.86f,.44f,.62f); top = new Color(1,.68f,.80f);
                for (int i = 0; i < 5; i++) Card("Pink hill", new Vector3((i-2)*1.1f,-.10f,2.8f+i*.05f),1.3f,.9f,
                    i%2==0 ? new Color(.92f,.61f,.77f) : new Color(.97f,.73f,.83f),32);
                for (int i = 0; i < 6; i++)
                {
                    float x=(i-2.5f)*.82f; float y=.30f+(i%2)*.4f;
                    Card("Gummy stem",new Vector3(x,y-.12f,2.1f),.025f,.22f,new Color(.44f,.70f,.57f),4);
                    for (int petal=0;petal<5;petal++)
                    {
                        float angle=petal*Mathf.PI*2/5;
                        Card("Gummy flower",new Vector3(x+Mathf.Cos(angle)*.09f,y+Mathf.Sin(angle)*.09f,2),.09f,.09f,new Color(1,.82f,.52f),12);
                    }
                    Card("Flower centre",new Vector3(x,y,1.98f),.055f,.055f,new Color(.89f,.47f,.64f),12);
                }
            }
            else if (world == StageWorld.Midnight)
            {
                rim=new Color(.39f,.30f,.60f); top=new Color(.62f,.50f,.80f);
                Card("Moon",new Vector3(1.52f,2.75f,2.8f),.33f,.33f,new Color(.98f,.90f,.68f),40);
                for(int i=0;i<22;i++) Card("Midnight star",new Vector3(Mathf.Sin(i*2.399f)*2.5f,.75f+(i%7)*.38f,2.7f),
                    i%3==0 ? .035f : .018f,i%3==0 ? .065f : .025f,new Color(.92f,.86f,1),4);
                Ring("Lavender luminous rim",1.21f,.152f,new Color(.83f,.72f,1),.035f);
            }
            plateBase.color=rim; plateTop.color=top;
            ApplyLighting();
            return true;
        }

        public bool SetLight(StageLight light)
        {
            if (!Enum.IsDefined(typeof(StageLight),light)) return false;
            Lighting=light; ApplyLighting(); return true;
        }

        private void ApplyLighting()
        {
            if (key == null) return;
            bool night=Lighting==StageLight.Night, golden=Lighting==StageLight.Golden;
            key.transform.rotation=Quaternion.Euler(night?48:golden?68:38,golden?-65:-32,0);
            key.color=night?new Color(.90f,.82f,1):golden?new Color(1,.77f,.51f):new Color(1,.955f,.905f);
            key.intensity=night?.55f:golden?.95f:1.05f;
            fill.color=night?new Color(.48f,.56f,.90f):new Color(.9f,.935f,1);
            fill.intensity=night?.18f:.34f;
            RenderSettings.ambientLight=night?new Color(.28f,.27f,.41f):golden?new Color(.72f,.58f,.48f):new Color(.69f,.65f,.66f);
            if(gradient!=null)
            {
                Color field=viewer.backgroundColor;
                Color lower=Color.Lerp(field,Color.white,night?.02f:.13f);
                Color upper=Color.Lerp(field,night?new Color(.11f,.10f,.24f):new Color(.85f,.76f,.82f),night?.23f:.16f);
                gradient.colors=new[]{lower,lower,upper,upper};
            }
            Color tint=night?new Color(.53f,.56f,.75f):golden?new Color(1,.86f,.68f):Color.white;
            bool glow=ToyCatalog.TryGetCollectible(collectibleId,out var item)&&item.FinishId=="glow-jelly";
            foreach(var renderer in toyMount.GetComponentsInChildren<Renderer>(true))
                foreach(var material in renderer.sharedMaterials)
                {
                    if(material==null) continue;
                    if(material.HasProperty("_StageTint")) material.SetColor("_StageTint",tint);
                    if(material.HasProperty("_StageEmission")) material.SetFloat("_StageEmission",night&&glow?.23f:0);
                }
        }

        private Transform Card(string name,Vector3 position,float rx,float ry,Color colour,int segments)
        {
            var vertices=new Vector3[segments+1]; var colours=new Color[vertices.Length]; var triangles=new int[segments*3];
            colours[0]=colour;
            for(int i=0;i<segments;i++)
            {
                float a=i*Mathf.PI*2/segments;
                vertices[i+1]=new Vector3(Mathf.Cos(a)*rx,Mathf.Sin(a)*ry,0); colours[i+1]=colour;
                triangles[i*3]=0; triangles[i*3+1]=i+1; triangles[i*3+2]=(i+1)%segments+1;
            }
            return MeshObject(name,position,vertices,colours,triangles);
        }
        private void Pine(Vector3 position,float height,Color colour)
        {
            Card("Pine trunk",position+Vector3.up*.22f,.055f,.25f,new Color(.43f,.34f,.28f),4);
            for(int j=0;j<3;j++)
            {
                float y=height*(.25f+j*.23f), w=height*(.34f-j*.07f);
                MeshObject("Pine silhouette",position,new[]{new Vector3(-w,y,0),new Vector3(w,y,0),new Vector3(0,y+height*.48f,0)},
                    new[]{colour,colour,colour},new[]{0,1,2});
            }
        }
        private void Ring(string name,float radius,float y,Color colour,float thickness)
        {
            const int n=64; var v=new Vector3[n*2];var c=new Color[v.Length];var t=new int[n*6];
            for(int i=0;i<n;i++)
            {
                float a=i*Mathf.PI*2/n;
                for(int j=0;j<2;j++){float r=radius+j*thickness;v[i*2+j]=new Vector3(Mathf.Cos(a)*r,0,Mathf.Sin(a)*r);c[i*2+j]=colour;}
                int next=(i+1)%n*2; int k=i*6;t[k]=i*2;t[k+1]=next;t[k+2]=i*2+1;t[k+3]=i*2+1;t[k+4]=next;t[k+5]=next+1;
            }
            var ring=MeshObject(name,new Vector3(0,y,0),v,c,t);
            // Plate decoration follows the plate; scenery does not rotate with the toy.
            ring.SetParent(toyMount.parent,false);
            ring.localPosition=new Vector3(0,y,0);
            plateDecorations.Add(ring.gameObject);
        }
        private readonly List<GameObject> plateDecorations=new List<GameObject>();
        private Transform MeshObject(string name,Vector3 position,Vector3[] vertices,Color[] colours,int[] triangles)
        {
            var mesh=new Mesh{name=name};mesh.vertices=vertices;mesh.colors=colours;mesh.triangles=triangles;mesh.RecalculateBounds();meshes.Add(mesh);
            var obj=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));obj.transform.SetParent(scenery,false);obj.transform.localPosition=position;
            obj.GetComponent<MeshFilter>().sharedMesh=mesh;var r=obj.GetComponent<MeshRenderer>();r.sharedMaterial=cards;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;
            return obj.transform;
        }
        private void Update()
        {
            if(gradientPlane!=null && background!=null) gradientPlane.localScale=new Vector3(background.aspect,1,1);
            if(Calm||viewer==null||!viewer.enabled||leaves.Count==0) return;
            elapsed+=Time.deltaTime;
            for(int i=0;i<leaves.Count;i++)
            {
                Vector3 p=leafRest[i];p.y=.4f+Mathf.Repeat(p.y-.4f-elapsed*.12f,2.5f);p.x+=Mathf.Sin(elapsed*.7f+i)*.16f;
                leaves[i].localPosition=p;leaves[i].localRotation=Quaternion.Euler(0,0,Mathf.Sin(elapsed+i)*28);
            }
        }
        private void ClearScenery()
        {
            if(scenery!=null){scenery.gameObject.SetActive(false);Destroy(scenery.gameObject);}
            foreach(var obj in plateDecorations) if(obj!=null){obj.SetActive(false);Destroy(obj);}
            plateDecorations.Clear();leaves.Clear();leafRest.Clear();
            foreach(var mesh in meshes) if(mesh!=null) Destroy(mesh);meshes.Clear();
        }
        private void OnDestroy(){ClearScenery();if(gradient!=null)Destroy(gradient);if(gradientPlane!=null)Destroy(gradientPlane.gameObject);if(cards!=null)Destroy(cards);}
    }
}
