using System.IO;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
namespace Pockle.Editor
{
    public static class SnowyReactionCapture
    {
        public static void Capture()
        {
            string output="/private/tmp/pockle-snowy-reaction-frames";Directory.CreateDirectory(output);
            foreach(SnowyAction action in System.Enum.GetValues(typeof(SnowyAction)))
            {
                string folder=Path.Combine(output,action.ToString());Directory.CreateDirectory(folder);
                var obj=new GameObject("Snowy animated preview");var toy=obj.AddComponent<JellyToy>();toy.Initialize();toy.TrySetCollectible("snowy.matte-vinyl");
                var driver=obj.AddComponent<SnowyReactionDriver>();driver.Initialize(toy);driver.TryPlay(action);
                var cameraObj=new GameObject("Snowy reaction camera",typeof(Camera));var camera=cameraObj.GetComponent<Camera>();
                camera.enabled=false;camera.orthographic=true;camera.orthographicSize=1.45f;camera.backgroundColor=new Color(.973f,.851f,.776f);
                camera.clearFlags=CameraClearFlags.SolidColor;camera.transform.position=new Vector3(0,.15f,-5);camera.transform.LookAt(new Vector3(0,.1f,0));
                var lamp=cameraObj.AddComponent<Light>();lamp.type=LightType.Directional;lamp.intensity=1.1f;RenderSettings.ambientLight=Color.white;
                var target=new RenderTexture(480,480,24);target.Create();camera.targetTexture=target;
                for(int frame=0;frame<52;frame++)
                {
                    driver.ApplyPose(SnowyReactions.Sample(action,frame/44f,false));camera.Render();
                    var previous=RenderTexture.active;RenderTexture.active=target;var pixels=new Texture2D(480,480,TextureFormat.RGB24,false);
                    pixels.ReadPixels(new Rect(0,0,480,480),0,0);pixels.Apply();RenderTexture.active=previous;
                    File.WriteAllBytes(Path.Combine(folder,frame.ToString("D3")+".png"),pixels.EncodeToPNG());Object.DestroyImmediate(pixels);
                }
                Object.DestroyImmediate(obj);Object.DestroyImmediate(cameraObj);target.Release();Object.DestroyImmediate(target);
            }
        }
    }
}
