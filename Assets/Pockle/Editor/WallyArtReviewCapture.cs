using System;
using System.IO;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
namespace Pockle.Editor
{
    public static class WallyArtReviewCapture
    {
        public static void Build()
        {
            WallyWorldImport.Build();WallyArtSetBuild.Build();
            CaptureWorlds();CaptureBox();
        }
        public static void CaptureWorlds()
        {
            string output=Path.Combine(Directory.GetCurrentDirectory(),"test-results","wally-art");Directory.CreateDirectory(output);
            foreach(string id in new[]{"wally"})
            {
                ToyCatalog.TryGetCollection(id,out var collection);
                for(int index=0;index<collection.Worlds.Count;index++)
                {
                    var world=collection.Worlds[index];bool night=index==4;
                    PrototypeStage.Create(out var camera,out var mount,out _,out _,out _);
                    var root=camera.transform.parent.gameObject;var obj=new GameObject("World review toy");obj.transform.SetParent(mount,false);
                    RenderTexture target=null;Texture2D pixels=null;var previous=RenderTexture.active;
                    try
                    {
                        var toy=obj.AddComponent<JellyToy>();toy.Initialize();toy.TrySetCollectible(id+(night?".glow-jelly":".gloss-vinyl"));
                        if(!PrototypeStage.SetEnvironment(camera,world.Id,toy.CollectibleId))throw new InvalidOperationException("World not bound: "+world.Id);
                        PrototypeStage.SetLight(camera,night?StageLight.Night:StageLight.Day);
                        target=new RenderTexture(600,800,24);target.Create();camera.targetTexture=target;
                        camera.rect=new Rect(0,0,1,1);camera.orthographicSize=1.95f;camera.aspect=.75f;camera.ResetProjectionMatrix();camera.transform.LookAt(new Vector3(0,1.15f,0));
                        var bg=root.transform.Find("Full-screen background").GetComponent<Camera>();bg.targetTexture=target;
                        camera.GetComponent<ToyStageEnvironment>().SendMessage("Update");camera.GetComponent<ToyGlowAura>().SendMessage("LateUpdate");
                        bg.Render();camera.clearFlags=CameraClearFlags.Depth;camera.projectionMatrix=Matrix4x4.Ortho(-1.4625f,1.4625f,-1.95f,1.95f,.1f,24f);camera.Render();RenderTexture.active=target;
                        pixels=new Texture2D(600,800,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,600,800),0,0);pixels.Apply();
                        File.WriteAllBytes(Path.Combine(output,world.Id+".png"),pixels.EncodeToPNG());
                    }
                    finally
                    {
                        RenderTexture.active=previous;camera.targetTexture=null;UnityEngine.Object.DestroyImmediate(root);
                        if(pixels!=null)UnityEngine.Object.DestroyImmediate(pixels);if(target!=null){target.Release();UnityEngine.Object.DestroyImmediate(target);}
                    }
                }
            }
            Debug.Log("WALLY_ART_READY: five ocean worlds, cover and real opening carton.");
        }
        public static void CaptureBox()
        {
            string output=Path.Combine(Directory.GetCurrentDirectory(),"test-results","wally-art","box");Directory.CreateDirectory(output);
            var root=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Boxes/WallyCollectionBox"));
            var native=root.GetComponent<NativeCollectionBox>();var view=new GameObject("Wally carton camera",typeof(Camera));var camera=view.GetComponent<Camera>();
            camera.enabled=false;camera.orthographic=true;camera.orthographicSize=2.6f;camera.backgroundColor=new Color(.89f,.95f,.97f);camera.clearFlags=CameraClearFlags.SolidColor;
            camera.transform.position=new Vector3(3,2.8f,-4);camera.transform.LookAt(new Vector3(0,1.50f,0));
            try
            {
                for(int frame=0;frame<60;frame++)
                {
                    float opening=Mathf.SmoothStep(0,1,Mathf.Clamp01((frame-8)/36f));native.ApplyPose(new RevealPose(opening,1,1,0,0,false,false),false);
                    var target=new RenderTexture(600,600,24);target.Create();var image=new Texture2D(600,600,TextureFormat.RGB24,false);var active=RenderTexture.active;
                    try{camera.targetTexture=target;camera.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,600,600),0,0);image.Apply();File.WriteAllBytes(Path.Combine(output,frame.ToString("D3")+".png"),image.EncodeToPNG());}
                    finally{camera.targetTexture=null;RenderTexture.active=active;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(image);}
                }
            }
            finally{UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(view);}
        }

    }
}
