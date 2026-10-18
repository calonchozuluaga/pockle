using System;
using System.IO;
using Pockle.Core;
using Pockle.Runtime;
using UnityEditor;
using UnityEngine;
namespace Pockle.Editor
{
    public static class ToyTemperamentCapture
    {
        public static void Capture()
        {
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-toyFaces");if(index<0)throw new ArgumentException("Supply -toyFaces folder");
            string output=args[index+1];Directory.CreateDirectory(output);var obj=new GameObject("Native temperament review");var cameraObject=new GameObject("Camera",typeof(Camera));
            var toy=obj.AddComponent<JellyToy>();toy.Initialize();var camera=cameraObject.GetComponent<Camera>();camera.enabled=false;camera.orthographic=true;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.973f,.851f,.776f);
            int count=0;
            try
            {
                foreach(string character in new[]{"fox","axolotl","firb","coco","ginger","snowy"})
                {
                    string id=null;foreach(var item in ToyCatalog.Collectibles)if(item.CharacterId==character&&item.FinishId=="matte-vinyl")id=item.Id;
                    if(id==null||!toy.TrySetCollectible(id))throw new InvalidOperationException("Missing native model "+character);
                    if(!ToyFaceLayout.TryGet(character,out var layout))throw new InvalidOperationException("Missing face layout "+character);
                    var driver=toy.GetComponentInChildren<ToyTemperamentDriver>();if(driver==null)throw new InvalidOperationException("Missing face driver "+character);
                    var material=toy.BodyRenderer.sharedMaterial;
                    float centreX=(layout.Left.x+layout.Right.x)*.5f,centreY=(layout.Left.y+layout.Mouth.y)*.5f;
                    camera.orthographicSize=Mathf.Max(.43f,(layout.Right.x-layout.Left.x)*.80f);camera.transform.position=new Vector3(centreX,centreY,-5);camera.transform.LookAt(new Vector3(centreX,centreY,0));
                    material.SetFloat("_CharacterTemperament",0);Save(camera,640,Path.Combine(output,character+"-source.png"));count++;material.SetFloat("_CharacterTemperament",1);
                    foreach(PipTemperament style in Enum.GetValues(typeof(PipTemperament)))
                        for(int state=0;state<3;state++){driver.ApplyFace(PipFaces.Pose(style,state));Save(camera,640,Path.Combine(output,character+"-"+style+"-"+state+".png"));count++;}
                    camera.orthographicSize=1.25f;camera.transform.position=new Vector3(0,0,-5);camera.transform.LookAt(Vector3.zero);driver.ApplyFace(PipFaces.Pose(PipTemperament.Sunny,0));
                    Save(camera,512,Path.Combine(output,character+"-body.png"));count++;
                    if(ShaderUtil.ShaderHasError(material.shader))throw new InvalidOperationException("Face shader failed "+character);
                }
                foreach(var item in ToyCatalog.Collectibles)
                {
                    if(!item.Available||!ToyFaceLayout.TryGet(item.CharacterId,out _))continue;
                    toy.TrySetCollectible(item.Id);var driver=toy.GetComponentInChildren<ToyTemperamentDriver>();driver.ApplyFace(PipFaces.Pose(PipTemperament.ShowOff,2));
                    camera.orthographicSize=1.25f;camera.transform.position=new Vector3(0,0,-5);camera.transform.LookAt(Vector3.zero);
                    Save(camera,384,Path.Combine(output,item.Id+"-front.png"));toy.transform.rotation=Quaternion.Euler(0,65,0);Save(camera,384,Path.Combine(output,item.Id+"-side.png"));toy.transform.rotation=Quaternion.identity;count+=2;
                }
                Debug.Log("TOY_TEMPERAMENT_CAPTURE_OK "+count+" native views");
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
