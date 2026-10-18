using System;
using System.IO;
using Pockle.Core;
using Pockle.Runtime;
using UnityEditor;
using UnityEngine;
namespace Pockle.Editor
{
    public static class PipRuntimeFaceCapture
    {
        public static void Capture()
        {
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-pipFaces");
            if(index<0)throw new ArgumentException("Supply -pipFaces <folder>");
            string output=args[index+1];Directory.CreateDirectory(output);
            var obj=new GameObject("Runtime Pip face capture");var cameraObject=new GameObject("Camera",typeof(Camera));
            var toy=obj.AddComponent<JellyToy>();toy.Initialize();toy.TrySetCollectible(ToyCatalog.PeachId);
            if(!toy.UsesMeshyPeach)throw new InvalidOperationException("Approved Pip missing");
            var driver=toy.GetComponentInChildren<PeachExpressionDriver>();
            var camera=cameraObject.GetComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.orthographicSize=.60f;
            camera.transform.position=new Vector3(.02f,-.12f,-5);camera.transform.LookAt(new Vector3(.02f,-.12f,0));
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.973f,.851f,.776f);
            try
            {
                foreach(PipTemperament style in Enum.GetValues(typeof(PipTemperament)))
                    for(int state=0;state<3;state++)
                    {
                        driver.ApplyFace(PipFaces.Pose(style,state));
                        Save(camera,640,Path.Combine(output,style+"-"+state+".png"));
                    }
                foreach(string id in new[]{ToyCatalog.PeachId,ToyCatalog.MoonId,ToyCatalog.GoldId,ToyCatalog.MintId,"pip.gloss-vinyl","pip.coated-metallic","pip.glow-jelly",PipFusionStudies.EclipseId})
                {
                    toy.TrySetCollectible(id);driver=toy.GetComponentInChildren<PeachExpressionDriver>();
                    camera.orthographicSize=1.25f;camera.transform.position=new Vector3(0,0,-5);camera.transform.LookAt(Vector3.zero);
                    driver.ApplyFace(PipFaces.Pose(PipTemperament.ShowOff,2));Save(camera,512,Path.Combine(output,id+"-front.png"));
                    toy.transform.rotation=Quaternion.Euler(0,70,0);Save(camera,512,Path.Combine(output,id+"-side.png"));
                    toy.transform.rotation=Quaternion.Euler(0,180,0);Save(camera,512,Path.Combine(output,id+"-back.png"));
                    toy.transform.rotation=Quaternion.identity;toy.SetDeformation(.22f,.06f,.05f,-.05f,0,0,0);Save(camera,512,Path.Combine(output,id+"-squish.png"));toy.ResetToy();
                }
                var shader=Shader.Find("Pockle/Textured Peach Jelly");
                if(ShaderUtil.ShaderHasError(shader))throw new InvalidOperationException("Runtime face shader failed");
                Debug.Log("PIP_RUNTIME_FACES_OK 24 expressions + 32 finish/rotation/deformation views");
            }
            finally{UnityEngine.Object.DestroyImmediate(obj);UnityEngine.Object.DestroyImmediate(cameraObject);}
        }
        private static void Save(Camera camera,int size,string file)
        {
            var target=new RenderTexture(size,size,24);target.Create();var pixels=new Texture2D(size,size,TextureFormat.RGB24,false);var old=RenderTexture.active;
            try{camera.targetTexture=target;camera.Render();RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,size,size),0,0);pixels.Apply();File.WriteAllBytes(file,pixels.EncodeToPNG());}
            finally{camera.targetTexture=null;RenderTexture.active=old;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(pixels);}
        }
    }
}
