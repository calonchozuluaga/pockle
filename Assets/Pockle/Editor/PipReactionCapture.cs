using System.IO;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
namespace Pockle.Editor
{
    public static class PipReactionCapture
    {
        public static void Capture()
        {
            string output="/private/tmp/pockle-pip-reaction-frames";Directory.CreateDirectory(output);
            foreach(PipAction action in System.Enum.GetValues(typeof(PipAction)))
            {
                string folder=Path.Combine(output,action.ToString());Directory.CreateDirectory(folder);
                var obj=new GameObject("Pip animated preview");var toy=obj.AddComponent<JellyToy>();toy.Initialize();toy.TrySetCollectible(ToyCatalog.PeachId);
                var driver=obj.AddComponent<PipReactionDriver>();driver.Initialize(toy);
                var cameraObj=new GameObject("Pip reaction camera",typeof(Camera));var camera=cameraObj.GetComponent<Camera>();
                camera.enabled=false;camera.orthographic=true;camera.orthographicSize=1.5f;camera.backgroundColor=new Color(.973f,.851f,.776f);
                camera.clearFlags=CameraClearFlags.SolidColor;camera.transform.position=new Vector3(0,.18f,-5);camera.transform.LookAt(new Vector3(0,.12f,0));
                var lamp=cameraObj.AddComponent<Light>();lamp.type=LightType.Directional;lamp.intensity=1.1f;RenderSettings.ambientLight=Color.white;
                var target=new RenderTexture(480,480,24);target.Create();camera.targetTexture=target;
                for(int frame=0;frame<48;frame++)
                {
                    // 1.8 seconds of action followed by a short neutral hold.
                    float progress=frame/40f;driver.ApplyPose(PipReactions.Sample(action,progress,false,false));camera.Render();
                    var previous=RenderTexture.active;RenderTexture.active=target;var pixels=new Texture2D(480,480,TextureFormat.RGB24,false);
                    pixels.ReadPixels(new Rect(0,0,480,480),0,0);pixels.Apply();RenderTexture.active=previous;
                    File.WriteAllBytes(Path.Combine(folder,frame.ToString("D3")+".png"),pixels.EncodeToPNG());Object.DestroyImmediate(pixels);
                }
                Object.DestroyImmediate(obj);Object.DestroyImmediate(cameraObj);target.Release();Object.DestroyImmediate(target);
            }
        }
    }
}
