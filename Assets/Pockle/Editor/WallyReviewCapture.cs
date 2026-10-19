using System;
using System.IO;
using Pockle.Core;
using Pockle.Runtime;
using UnityEditor;
using UnityEngine;
namespace Pockle.Editor
{
    public static class WallyReviewCapture
    {
        public static void BuildAndCapture(){WallyCollectionBuild.Build();Capture();}
        public static void Capture()
        {
            string output=Path.Combine(Directory.GetCurrentDirectory(),"test-results","wally");Directory.CreateDirectory(output);
            var obj=new GameObject("Wally review");var cameraObj=new GameObject("Wally review camera",typeof(Camera));
            var toy=obj.AddComponent<JellyToy>();toy.Initialize();var driver=obj.AddComponent<WallyReactionDriver>();driver.Initialize(toy);
            var camera=cameraObj.GetComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.orthographicSize=1.3f;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.89f,.95f,.97f);
            camera.transform.position=new Vector3(0,.08f,-5);camera.transform.LookAt(new Vector3(0,-.1f,0));
            try
            {
                foreach(var item in ToyCatalog.Collectibles)
                {
                    if(item.CharacterId!="wally")continue;toy.TrySetCollectible(item.Id);
                    var m=toy.BodyRenderer.sharedMaterial;if(ShaderUtil.ShaderHasError(m.shader))throw new InvalidOperationException("Wally shader failed");
                    Save(camera,512,Path.Combine(output,item.Id+".png"));
                }
                toy.TrySetCollectible(WallyCatalog.ClassicId);var face=toy.GetComponentInChildren<ToyTemperamentDriver>();
                var material=toy.BodyRenderer.sharedMaterial;material.SetFloat("_CharacterTemperament",0);
                Save(camera,640,Path.Combine(output,"wally-source.png"));material.SetFloat("_CharacterTemperament",1);
                foreach(PipTemperament temperament in Enum.GetValues(typeof(PipTemperament)))
                {face.ApplyFace(PipFaces.Pose(temperament,0));Save(camera,512,Path.Combine(output,"face-"+temperament+".png"));}
                foreach(WallyAction action in Enum.GetValues(typeof(WallyAction)))
                {
                    string folder=Path.Combine(output,action.ToString());Directory.CreateDirectory(folder);driver.TryPlay(action);
                    for(int frame=0;frame<50;frame++)
                    {
                        var pose=WallyReactions.Sample(action,frame/44f,false);driver.ApplyPose(pose);
                        face.ApplyReaction(frame/44f,pose.Face);Save(camera,400,Path.Combine(folder,frame.ToString("D3")+".png"));
                    }
                    driver.Cancel();
                }
                toy.transform.rotation=Quaternion.Euler(0,180,0);Save(camera,512,Path.Combine(output,"wally-back.png"));
                Debug.Log("WALLY_NATIVE_REVIEW_OK: ten finishes, eight faces, five animated actions and back view.");
            }
            finally{UnityEngine.Object.DestroyImmediate(obj);UnityEngine.Object.DestroyImmediate(cameraObj);}
        }
        private static void Save(Camera camera,int size,string file)
        {
            var target=new RenderTexture(size,size,24);target.Create();var image=new Texture2D(size,size,TextureFormat.RGB24,false);var active=RenderTexture.active;
            try{camera.targetTexture=target;camera.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,size,size),0,0);image.Apply();File.WriteAllBytes(file,image.EncodeToPNG());}
            finally{camera.targetTexture=null;RenderTexture.active=active;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(image);}
        }
    }
}
