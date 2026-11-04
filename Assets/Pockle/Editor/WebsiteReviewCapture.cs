using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Pockle.Core;
using Pockle.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pockle.Editor
{
    // Editorial staging of real runtime assets. Does not touch inventory or saved scenes.
    public static class WebsiteReviewCapture
    {
        [Serializable] private sealed class Shot
        {
            public string id, file, alt, world, composition, status = "composition-review";
            public string[] toys;
            public int width, height;
            public bool transparent;
        }
        [Serializable] private sealed class Manifest { public string renderer = "Unity 6000.6.5f1, Built-in"; public Shot[] images; }
        private static readonly List<Shot> shots = new List<Shot>();
        private static readonly Color Milk = new Color(251/255f,248/255f,245/255f);
        private static string Output => Path.Combine(Directory.GetCurrentDirectory(), "ArtSource", "Website");

        public static void Capture()
        {
            if (Application.unityVersion != "6000.6.5f1") throw new InvalidOperationException("Pinned Unity required");
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            shots.Clear(); Directory.CreateDirectory(Output);
            bool had = PlayerPrefs.HasKey(PipCharacterAsset.MeshyPreference);
            int previous = PlayerPrefs.GetInt(PipCharacterAsset.MeshyPreference);
            PlayerPrefs.SetInt(PipCharacterAsset.MeshyPreference,1);
            try
            {
                Hero(false); Hero(true);
                Walk(); Squish();
                Open("common",ToyCatalog.PeachId,new Color(1,.92f,.77f));
                Open("rare",WallyCatalog.LeafId,new Color(1,.75f,.23f));
                Open("epic",WallyCatalog.DiamondId,new Color(.55f,.85f,1));
                Series(false); Series(true);
                string[] tierIds={WallyCatalog.KelpId,WallyCatalog.LeafId,WallyCatalog.DiamondId,WallyCatalog.SpecialId};
                string[] tiers={"common","rare","epic","secret"};
                for(int i=0;i<tierIds.Length;i++)
                {
                    using(var s=new Stage("wally",tierIds[i],true))
                    {
                        s.Toy(tierIds[i],Vector3.zero,2.65f,-12);
                        s.Frame(new Vector3(0,1.48f,-7),new Vector3(0,1.34f,0),2.75f);
                        s.Save("SE2",tiers[i],1600,1600,"Wally in "+tiers[i]+" finish.",new[]{tierIds[i]},"Same pose and camera; transparent background.",true);
                    }
                }
                int card=0;
                foreach(var character in ToyCatalog.Characters)
                {
                    string id=character.Id=="pip"?ToyCatalog.PeachId:character.Id+"."+character.LeadFinishId;
                    using(var s=new Stage(character.Id,id,false))
                    {
                        s.Box(character.Id,new Vector3(-1.05f,0,.35f),2.0f,false,-16);
                        s.Toy(id,new Vector3(1.13f,0,-.15f),1.95f,-12);
                        s.Frame(new Vector3(0,2.7f,-8),new Vector3(0,1.1f,0),2.05f);
                        s.Save("C"+(++card),character.Id,1600,1200,character.DisplayName+" Pockle beside its collection box.",new[]{id},"Aligned tabletop pair in the character's home world.");
                    }
                }
                File.WriteAllText(Path.Combine(Output,"manifest.json"),JsonUtility.ToJson(new Manifest{images=shots.ToArray()},true));
                Debug.Log("POCKLE_WEBSITE_CAPTURE_OK: "+shots.Count+" real-asset images");
            }
            finally { if(had)PlayerPrefs.SetInt(PipCharacterAsset.MeshyPreference,previous);else PlayerPrefs.DeleteKey(PipCharacterAsset.MeshyPreference); }
        }

        private static void Hero(bool mobile)
        {
            string[] ids={ToyCatalog.PeachId,"fox.boucle-plush",WallyCatalog.ClassicId,"axolotl.clear-jelly",ToyCatalog.GoldId,ToyCatalog.PipSpecialId};
            using(var s=new Stage("pip",ToyCatalog.PeachId,true))
            {
                if(!mobile)
                {
                    s.Box("pip",new Vector3(2.8f,0,1.4f),2.0f,true,-12);
                    Vector3[] p={new Vector3(.6f,0,-1f),new Vector3(2.6f,0,-1.0f),new Vector3(4.55f,0,-.65f),new Vector3(1.0f,.85f,1.5f),new Vector3(4.6f,.85f,1.7f),new Vector3(2.6f,1.6f,2.3f)};
                    s.Riser(new Vector3(1,.35f,1.5f),new Vector3(1.25f,.9f,1.25f));
                    s.Riser(new Vector3(4.6f,.35f,1.7f),new Vector3(1.25f,.9f,1.25f));
                    for(int i=0;i<ids.Length;i++) {var t=s.Toy(ids[i],p[i],i==0?1.8f:1.35f,i%2==0?-12:12);if(i==0)t.SetDeformation(.12f,0,0,0,0,0);}
                    s.Frame(new Vector3(.1f,3.7f,-11),new Vector3(.1f,1.1f,.5f),3.25f);
                }
                else
                {
                    s.Box("pip",new Vector3(0,0,1.4f),2.0f,true,-12);
                    Vector3[] p={new Vector3(-1.05f,0,-1.3f),new Vector3(1.02f,0,-1.3f),new Vector3(-1.1f,0,.35f),new Vector3(1.08f,0,.35f),new Vector3(-1.05f,1.3f,1.8f),new Vector3(1.05f,1.3f,1.8f)};
                    for(int i=0;i<ids.Length;i++)s.Toy(ids[i],p[i],i==2?1.0f:1.25f,i%2==0?-10:10);
                    s.Frame(new Vector3(0,5.4f,-11),new Vector3(0,2.45f,.4f),3.6f);
                }
                s.Save("H1",mobile?"mobile":"desktop",mobile?1080:2560,mobile?1350:1440,"A group of Pockle toys in jelly, plush and vinyl finishes around an open surprise box.",ids,mobile?"Recomposed two-column group; upper35% reserved for copy.":"Right-aligned group; left40% reserved for copy.");
            }
        }
        private static void Walk()
        {
            using(var s=new Stage("pip",ToyCatalog.PeachId,false))
            {
                s.Toy(ToyCatalog.PeachId,new Vector3(-.55f,.35f,-.3f),1.8f,-12);
                s.Box("pip",new Vector3(.95f,.65f,.6f),1.18f,false,18);
                for(int i=0;i<8;i++)s.Orb(new Vector3(-1.7f+i*.22f,.12f,.15f+Mathf.Sin(i)*.1f),.035f,new Color(1,.78f,.35f));
                s.Frame(new Vector3(0,2.6f,-8),new Vector3(0,1.35f,0),2.05f);
                s.Save("S1","walk",1600,1600,"Pip hopping along a path toward a surprise box.",new[]{ToyCatalog.PeachId},"Static authored mid-hop editorial pose in Pip's garden world.");
            }
        }
        private static void Squish()
        {
            using(var s=new Stage("pip",ToyCatalog.PeachId,true))
            {
                var t=s.Toy(ToyCatalog.PeachId,Vector3.zero,2.4f,0);t.SetDeformation(.34f,0,.08f,0,.15f,0);
                s.Frame(new Vector3(0,2.0f,-7),new Vector3(0,1.02f,0),1.65f);
                s.Save("S3","play",1600,1600,"A translucent jelly Pockle being squished.",new[]{ToyCatalog.PeachId},"Real JellyToy deformation; no hand or fake UI.");
            }
        }
        private static void Open(string variant,string toy,Color light)
        {
            string collection=toy.StartsWith("wally.")?"wally":"pip";
            using(var s=new Stage(collection,toy,true))
            {
                s.Box(collection,new Vector3(0,0,.3f),2.0f,true,-8);
                s.Toy(toy,new Vector3(0,1.35f,-.1f),1.6f,-8);
                for(int i=0;i<12;i++){float a=i*Mathf.PI*2/12;s.Orb(new Vector3(Mathf.Cos(a)*1.12f,1.8f+Mathf.Sin(a)*.9f,.4f),i%3==0?.052f:.028f,light);}
                s.Frame(new Vector3(0,3,-8),new Vector3(0,1.65f,0),2.05f);
                s.Save("S2","open-"+variant,1600,1600,"A Pockle box bursting open as a toy pops out.",new[]{toy},"Actual hinged carton sampled open with staged rising toy; editorial still, not a captured paid transaction.");
            }
        }
        private static void Series(bool mobile)
        {
            var ids=ToyCatalog.Collectibles.Where(x=>x.CharacterId=="wally").OrderBy(x=>(int)WallyCatalog.Tier(x.Id)).Select(x=>x.Id).ToArray();
            using(var s=new Stage("wally",WallyCatalog.ClassicId,false,"wally-pearl-grotto"))
            {
                int n=0;
                int[] counts={5,4,3,2,1};
                for(int row=0;row<counts.Length;row++)
                {
                    float spacing=mobile?1.06f:1.9f;
                    float z=row*(mobile?.7f:1.35f);float y=row*(mobile?.62f:.48f);
                    s.Riser(new Vector3(0,y*.5f-.05f,z),new Vector3(counts[row]*spacing,.12f+y,.98f));
                    for(int col=0;col<counts[row];col++)s.Toy(ids[n++],new Vector3((col-(counts[row]-1)*.5f)*spacing,y,z),mobile?.95f:1.5f,0);
                }
                if(mobile)s.Frame(new Vector3(0,6.4f,-12),new Vector3(0,2.15f,1.7f),4.3f);
                else s.Frame(new Vector3(0,6.6f,-13),new Vector3(0,2.15f,2.5f),4.1f);
                s.Save("SE1",mobile?"mobile":"desktop",mobile?1080:2560,mobile?1350:1100,"All fifteen Wally Pockles arranged on a display, from common to secret.",ids,"Actual15 finishes on five ascending display rows; Pearl Grotto world.");
            }
        }

        private sealed class Stage : IDisposable
        {
            private readonly GameObject root;
            private readonly Camera camera, background;
            private readonly Transform mount;
            private readonly bool studio;
            private readonly string world;
            private readonly List<Material> materials=new List<Material>();
            private readonly List<Mesh> meshes=new List<Mesh>();
            public Stage(string collection,string toy,bool studio,string worldOverride=null)
            {
                this.studio=studio;
                PrototypeStage.Create(out camera,out mount,out _,out _,out var shadow);
                root=camera.transform.parent.gameObject;shadow.gameObject.SetActive(false);
                background=root.transform.Find("Full-screen background").GetComponent<Camera>();
                ToyCatalog.TryGetCollection(collection,out var c);world=worldOverride??c.HomeWorld;
                if(!PrototypeStage.SetEnvironment(camera,world,toy))throw new InvalidOperationException("Missing world "+world);
                foreach(var t in root.GetComponentsInChildren<Transform>())if(t.name=="Falling leaf")t.gameObject.SetActive(false);
                PrototypeStage.SetLight(camera,StageLight.Day);
                camera.enabled=background.enabled=false;camera.nearClipPlane=.1f;camera.farClipPlane=60;
                camera.backgroundColor=Milk;camera.rect=new Rect(0,0,1,1);
                if(studio)Floor();
            }
            public JellyToy Toy(string id,Vector3 position,float height,float yaw)
            {
                var go=new GameObject("Website toy "+id);go.transform.SetParent(mount,false);
                var toy=go.AddComponent<JellyToy>();toy.Initialize();if(!toy.TrySetCollectible(id))throw new InvalidOperationException("Unavailable "+id);
                go.transform.rotation=Quaternion.Euler(0,yaw,0);
                Bounds b=toy.SurfaceBounds;go.transform.localScale=Vector3.one*(height/Mathf.Max(.01f,b.size.y));
                b=toy.SurfaceBounds;go.transform.position+=position-new Vector3(b.center.x,b.min.y,b.center.z);
                foreach(var r in go.GetComponentsInChildren<Renderer>())foreach(var m in r.sharedMaterials)
                {if(m==null)continue;if(m.HasProperty("_Blink"))m.SetFloat("_Blink",0);if(m.HasProperty("_Happy"))m.SetFloat("_Happy",.2f);if(ShaderUtil.ShaderHasError(m.shader))throw new InvalidOperationException("Shader error "+m.shader.name);}
                PrototypeStage.SetLight(camera,StageLight.Day);
                ContactShadow(position,height);
                return toy;
            }
            public void Box(string collection,Vector3 position,float height,bool open,float yaw)
            {
                string name=char.ToUpperInvariant(collection[0])+collection.Substring(1);
                var prefab=Resources.Load<GameObject>("Boxes/"+name+"CollectionBox");if(prefab==null)throw new InvalidOperationException("Missing box "+name);
                var box=UnityEngine.Object.Instantiate(prefab,root.transform);box.GetComponent<NativeCollectionBox>().ApplyPose(new RevealPose(open?.55f:0,1,1,0,0,false,false),false);
                box.transform.rotation=Quaternion.Euler(0,180+yaw,0);var b=BoundsOf(box);box.transform.localScale*=height/b.size.y;
                b=BoundsOf(box);box.transform.position+=position-new Vector3(b.center.x,b.min.y,b.center.z);
            }
            private static Bounds BoundsOf(GameObject go){var rs=go.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);return b;}
            private Material Mat(Color color,bool unlit=false)
            {var m=new Material(Shader.Find(unlit?"Unlit/Color":"Standard"));m.color=color;if(!unlit){m.SetFloat("_Glossiness",.32f);m.SetFloat("_Metallic",.05f);}materials.Add(m);return m;}
            private void Floor(){var p=GameObject.CreatePrimitive(PrimitiveType.Plane);p.transform.SetParent(root.transform,false);p.transform.position=new Vector3(0,-.10f,3);p.transform.localScale=Vector3.one*8;p.GetComponent<Renderer>().sharedMaterial=Mat(Milk,true);}
            private void ContactShadow(Vector3 position,float height)
            {
                var p=GameObject.CreatePrimitive(PrimitiveType.Quad);p.name="Website contact shadow";p.transform.SetParent(root.transform,false);
                p.transform.position=new Vector3(position.x,-.075f,position.z);p.transform.rotation=Quaternion.Euler(90,0,0);p.transform.localScale=new Vector3(height*.60f,height*.38f,1);
                var mesh=UnityEngine.Object.Instantiate(p.GetComponent<MeshFilter>().sharedMesh);var vertices=mesh.vertices;for(int i=0;i<vertices.Length;i++)vertices[i]*=2;mesh.vertices=vertices;mesh.RecalculateBounds();meshes.Add(mesh);p.GetComponent<MeshFilter>().sharedMesh=mesh;
                var m=new Material(Shader.Find("Pockle/Soft Blush"));m.color=new Color(.3f,.18f,.26f,.16f);m.SetFloat("_GroundPlane",0);m.renderQueue=2990;materials.Add(m);p.GetComponent<Renderer>().sharedMaterial=m;
            }
            public void Riser(Vector3 position,Vector3 size){var p=GameObject.CreatePrimitive(PrimitiveType.Cube);p.transform.SetParent(root.transform,false);p.transform.position=position;p.transform.localScale=size;p.GetComponent<Renderer>().sharedMaterial=Mat(new Color(.89f,.96f,.97f));}
            public void Orb(Vector3 position,float radius,Color color){var p=GameObject.CreatePrimitive(PrimitiveType.Sphere);p.transform.SetParent(root.transform,false);p.transform.position=position;p.transform.localScale=Vector3.one*radius;p.GetComponent<Renderer>().sharedMaterial=Mat(color,true);}
            public void Frame(Vector3 position,Vector3 look,float size){camera.transform.position=position;camera.transform.LookAt(look);camera.orthographicSize=size;}
            public void Save(string id,string name,int width,int height,string alt,string[] toys,string composition,bool alpha=false)
            {
                var folder=Path.Combine(Output,id);Directory.CreateDirectory(folder);
                var rt=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32);rt.antiAliasing=4;rt.Create();
                var image=new Texture2D(width,height,alpha?TextureFormat.RGBA32:TextureFormat.RGB24,false);
                var previous=RenderTexture.active;
                try
                {
                    camera.targetTexture=rt;camera.aspect=(float)width/height;camera.ResetProjectionMatrix();
                    // Marketing stills use the real world probe at lower exposure so gold/ice retain detail.
                    foreach(var toy in mount.GetComponentsInChildren<JellyToy>())foreach(var r in toy.GetComponentsInChildren<Renderer>())foreach(var m in r.sharedMaterials)
                        if(m!=null&&m.HasProperty("_StageEnvironmentStrength"))m.SetFloat("_StageEnvironmentStrength",.20f);
                    if(!studio&&!alpha){background.targetTexture=rt;background.aspect=camera.aspect;camera.GetComponent<ToyStageEnvironment>().SendMessage("Update");background.Render();camera.clearFlags=CameraClearFlags.Depth;}
                    else {camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=alpha?Color.clear:Milk;}
                    if(alpha)foreach(var r in root.GetComponentsInChildren<Renderer>())if(!r.gameObject.name.StartsWith("Website")&&!r.transform.GetComponentsInParent<JellyToy>().Any())r.enabled=false;
                    camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();
                    string file=name+".png";File.WriteAllBytes(Path.Combine(folder,file),image.EncodeToPNG());
                    var shot=new Shot{id=id,file=id+"/"+file,width=width,height=height,alt=alt,toys=toys,world=studio?"Studio lighting; collection reflection environment "+world:world,composition=composition,transparent=alpha};shots.Add(shot);
                    File.WriteAllText(Path.Combine(folder,name+".manifest.json"),JsonUtility.ToJson(shot,true));
                    Debug.Log("WEBSITE_SHOT "+shot.file);
                }
                finally{camera.targetTexture=background.targetTexture=null;RenderTexture.active=previous;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(image);}
            }
            public void Dispose(){UnityEngine.Object.DestroyImmediate(root);foreach(var m in materials)UnityEngine.Object.DestroyImmediate(m);foreach(var m in meshes)UnityEngine.Object.DestroyImmediate(m);}
        }
    }
}
