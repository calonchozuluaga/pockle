using System.IO;
using Pockle.Core;
using Pockle.Runtime;
using UnityEditor;
using UnityEngine;
namespace Pockle.Editor
{
    public static class GlowBoundsReviewCapture
    {
        public static void Capture()
        {
            var ambient=RenderSettings.ambientLight;var mode=RenderSettings.ambientMode;bool fog=RenderSettings.fog;
            PrototypeStage.Create(out var camera,out var mount,out _,out _,out _);var root=camera.transform.parent.gameObject;
            var obj=new GameObject("Glow review body");obj.transform.SetParent(mount,false);var toy=obj.AddComponent<JellyToy>();toy.Initialize();
            string output=Path.Combine(Directory.GetCurrentDirectory(),"test-results","glow-bounds");Directory.CreateDirectory(output);
            try
            {
                toy.TrySetCollectible(WallyCatalog.SpecialId);ToyCatalog.TryGetCollection("wally",out var collection);
                PrototypeStage.SetEnvironment(camera,collection.Worlds[4].Id,toy.CollectibleId);PrototypeStage.SetLight(camera,StageLight.Night);
                var aura=camera.GetComponent<ToyGlowAura>();
                camera.rect=new Rect(0,0,1,1);camera.orthographic=true;camera.orthographicSize=2f;camera.ResetProjectionMatrix();
                camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.015f,.01f,.03f);
                foreach(int yaw in new[]{0,90,180,270})
                {
                    toy.transform.localRotation=Quaternion.Euler(0,yaw,0);
                    camera.transform.position=toy.SurfaceBounds.center+new Vector3(0,.1f,-5);camera.transform.LookAt(toy.SurfaceBounds.center);aura.SendMessage("LateUpdate");Save(camera,Path.Combine(output,"wally-night-"+yaw+".png"));
                }
                toy.transform.localRotation=Quaternion.identity;camera.transform.position=toy.SurfaceBounds.center+new Vector3(0,.1f,-5);camera.transform.LookAt(toy.SurfaceBounds.center);aura.SendMessage("LateUpdate");
                foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>())if(renderer.name.StartsWith("Toy emitted"))renderer.gameObject.SetActive(false);
                Save(camera,Path.Combine(output,"wally-surface-only.png"));
                toy.BodyRenderer.sharedMaterial.SetFloat("_StageEmission",0);Save(camera,Path.Combine(output,"wally-no-emission.png"));
                Debug.Log("GLOW_BOUNDS_REVIEW_OK: four native Wally orientations and isolated surface-emission comparison.");
            }
            finally {Object.DestroyImmediate(root);RenderSettings.ambientLight=ambient;RenderSettings.ambientMode=mode;RenderSettings.fog=fog;}
        }
        private static void Save(Camera camera,string path)
        {
            var rt=new RenderTexture(640,640,24);rt.Create();var image=new Texture2D(640,640,TextureFormat.RGB24,false);var previous=RenderTexture.active;
            try {camera.targetTexture=rt;camera.aspect=1;camera.projectionMatrix=Matrix4x4.Ortho(-2f,2f,-2f,2f,camera.nearClipPlane,camera.farClipPlane);camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,640,640),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());}
            finally {camera.targetTexture=null;RenderTexture.active=previous;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(image);}
        }
    }
}
