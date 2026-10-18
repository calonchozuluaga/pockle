using System.IO;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
namespace Pockle.Editor
{
    public static class ToyStageReviewCapture
    {
        public static void Capture()
        {
            Directory.CreateDirectory("/private/tmp/pockle-stage-review");
            foreach(var world in new[]{StageWorld.Studio,StageWorld.Home,StageWorld.Garden,StageWorld.Midnight})
            {
                PrototypeStage.Create(out var camera,out var mount,out _,out _,out _);
                var root=camera.transform.parent.gameObject;
                var obj=new GameObject("Stage review fox");obj.transform.SetParent(mount,false);
                var toy=obj.AddComponent<JellyToy>();toy.Initialize();toy.TrySetCollectible(world==StageWorld.Midnight?ToyCatalog.FoxAuroraId:ToyCatalog.FoxId);
                PrototypeStage.SetBackdrop(camera,StageLook.Backdrop(world,world==StageWorld.Midnight?StageLight.Night:StageLight.Day,"fox",new Color(1,.88f,.80f)));
                PrototypeStage.SetWorld(camera,world,toy.CollectibleId);PrototypeStage.SetLight(camera,world==StageWorld.Midnight?StageLight.Night:StageLight.Day);
                var target=new RenderTexture(720,960,24);target.Create();camera.targetTexture=target;
                camera.rect=new Rect(0,0,1,1);camera.orthographicSize=2.55f;camera.transform.LookAt(new Vector3(0,1.53f,0));
                var bg=root.transform.Find("Full-screen background").GetComponent<Camera>();bg.targetTexture=target;
                bg.transform.Find("World gradient").localScale=new Vector3(.75f,1,1);
                camera.GetComponent<ToyStageEnvironment>().SendMessage("Update");
                bg.Render();camera.clearFlags=CameraClearFlags.Depth;camera.Render();
                var previous=RenderTexture.active;RenderTexture.active=target;var pixels=new Texture2D(720,960,TextureFormat.RGB24,false);
                pixels.ReadPixels(new Rect(0,0,720,960),0,0);pixels.Apply();RenderTexture.active=previous;
                File.WriteAllBytes("/private/tmp/pockle-stage-review/"+world+".png",pixels.EncodeToPNG());
                Object.DestroyImmediate(pixels);target.Release();Object.DestroyImmediate(target);Object.DestroyImmediate(root);
            }
        }
    }
}
