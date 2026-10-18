using System.IO;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
namespace Pockle.Editor
{
    public static class FreeToyViewCapture
    {
        public static void Capture()
        {
            string output="/private/tmp/pockle-free-view-captures";Directory.CreateDirectory(output);
            foreach(var id in new[]{ToyCatalog.FoxId,ToyCatalog.PipSpecialId,"axolotl.glow-jelly"})
            foreach(float yaw in new[]{0f,90f,180f})
            {
                PrototypeStage.Create(out var camera,out var mount,out var pivot,out _,out _);
                var root=camera.transform.parent.gameObject;
                var obj=new GameObject("Free viewer capture");obj.transform.SetParent(mount,false);
                var toy=obj.AddComponent<JellyToy>();toy.Initialize();toy.TrySetCollectible(id);
                PrototypeStage.SetWorld(camera,StageWorld.Home,id);
                PrototypeStage.SetLight(camera,id==ToyCatalog.FoxId?StageLight.Day:StageLight.Night);
                var target=new RenderTexture(720,960,24);target.Create();camera.targetTexture=target;
                camera.rect=new Rect(0,0,1,1);camera.orthographicSize=1.8f;camera.transform.LookAt(mount.position);
                pivot.rotation=Quaternion.AngleAxis(yaw==90?45:0,camera.transform.right)*Quaternion.Euler(0,yaw,0);
                var bg=root.transform.Find("Full-screen background").GetComponent<Camera>();bg.targetTexture=target;
                var env=camera.GetComponent<ToyStageEnvironment>();env.SetViewAngles(yaw,yaw==90?45:0);
                for(int i=0;i<120;i++)env.SendMessage("Update");
                camera.GetComponent<ToyGlowAura>().SendMessage("LateUpdate");
                camera.rect=new Rect(0,0,1,1);camera.orthographicSize=1.8f;
                bg.Render();camera.clearFlags=CameraClearFlags.Depth;camera.Render();

                var previous=RenderTexture.active;RenderTexture.active=target;var pixels=new Texture2D(720,960,TextureFormat.RGB24,false);
                pixels.ReadPixels(new Rect(0,0,720,960),0,0);pixels.Apply();RenderTexture.active=previous;
                File.WriteAllBytes(output+"/"+id+"-"+yaw+".png",pixels.EncodeToPNG());
                Object.DestroyImmediate(pixels);target.Release();Object.DestroyImmediate(target);Object.DestroyImmediate(root);
            }
        }
    }
}
