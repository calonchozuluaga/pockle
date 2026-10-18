using System;
using System.IO;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
namespace Pockle.Editor
{
    public static class WinterTrioArtReviewCapture
    {
        public static void Build()
        {
            WinterTrioWorldImport.Build();WinterTrioArtSetBuild.Build();
            CaptureWorlds();
        }
        public static void CaptureWorlds()
        {
            const string output="/private/tmp/winter-trio-art-review";Directory.CreateDirectory(output);
            foreach(string id in new[]{"reiny","guin","bundy"})
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
            Debug.Log("WINTER_TRIO_ART_READY: 15 worlds, 3 covers and 3 opening cartons.");
        }
    }
}
