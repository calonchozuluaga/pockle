using System;
using System.IO;
using Pockle.Core;
using Pockle.Runtime;
using UnityEditor;
using UnityEngine;
namespace Pockle.Editor
{
    public static class WallyPilotCapture
    {
        public static void Capture()
        {
            string output=Path.Combine(Directory.GetCurrentDirectory(),"test-results","wally-pilot");Directory.CreateDirectory(output);
            var obj=new GameObject("Wally pilot");var view=new GameObject("Pilot camera",typeof(Camera));
            var toy=obj.AddComponent<JellyToy>();toy.Initialize();var camera=view.GetComponent<Camera>();
            camera.enabled=false;camera.orthographic=true;camera.orthographicSize=1.35f;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.89f,.95f,.97f);
            camera.transform.position=new Vector3(0,.04f,-5);camera.transform.LookAt(new Vector3(0,-.12f,0));
            try
            {
                int count=0;
                foreach(var item in ToyCatalog.Collectibles)
                {
                    if(item.CharacterId!="wally")continue;
                    if(!toy.TrySetCollectible(item.Id))throw new Exception("Unavailable: "+item.Id);
                    var material=toy.BodyRenderer.sharedMaterial;
                    if(ShaderUtil.ShaderHasError(material.shader))
                    {
                        foreach(var message in ShaderUtil.GetShaderMessages(material.shader))Debug.LogError(message.message+" at line "+message.line);
                        throw new Exception("Shader error: "+item.Id);
                    }
                    Save(camera,Path.Combine(output,item.Id+".png"));count++;
                }
                camera.orthographicSize=1.65f;
                foreach(var id in new[]{"wally.coated-metallic",WallyCatalog.LeafId,WallyCatalog.RoseId,WallyCatalog.DiamondId,WallyCatalog.SpecialId})
                {
                    toy.TrySetCollectible(id);toy.transform.rotation=Quaternion.Euler(0,35,0);
                    Save(camera,Path.Combine(output,id+"-angle.png"));
                }
                camera.orthographicSize=1.35f;
                toy.transform.rotation=Quaternion.identity;toy.TrySetCollectible(WallyCatalog.SpecialId);
                toy.BodyRenderer.sharedMaterial.SetFloat("_RarePulse",.86f);Save(camera,Path.Combine(output,"molten-low.png"));
                toy.BodyRenderer.sharedMaterial.SetFloat("_RarePulse",1.14f);Save(camera,Path.Combine(output,"molten-high.png"));
                if(count!=15)throw new Exception("Expected15 Wally members");
                Debug.Log("WALLY_PILOT_CAPTURE_OK: 15 finishes, five reflective angle views, two pulse endpoints");
            }
            finally{UnityEngine.Object.DestroyImmediate(obj);UnityEngine.Object.DestroyImmediate(view);}
        }
        private static void Save(Camera camera,string path)
        {
            var rt=new RenderTexture(640,640,24);rt.Create();var image=new Texture2D(640,640,TextureFormat.RGB24,false);var previous=RenderTexture.active;
            try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;image.ReadPixels(new Rect(0,0,640,640),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());}
            finally{camera.targetTexture=null;RenderTexture.active=previous;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(image);}
        }
    }
}
