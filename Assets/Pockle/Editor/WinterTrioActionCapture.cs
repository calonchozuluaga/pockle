using System.IO;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
namespace Pockle.Editor
{
    public static class WinterTrioActionCapture
    {
        public static void Capture()
        {
            string output="/private/tmp/winter-trio-action-frames";Directory.CreateDirectory(output);
            foreach(string character in new[]{"reiny","guin","bundy"})
            foreach(WinterTrioAction action in System.Enum.GetValues(typeof(WinterTrioAction)))
            {
                string folder=Path.Combine(output,character,action.ToString());Directory.CreateDirectory(folder);
                var obj=new GameObject("Winter animated preview");var toy=obj.AddComponent<JellyToy>();toy.Initialize();toy.TrySetCollectible(character+".matte-vinyl");
                var driver=obj.AddComponent<WinterTrioReactionDriver>();driver.Initialize(toy);driver.TryPlay(action);
                var cameraObj=new GameObject("Winter reaction camera",typeof(Camera));var camera=cameraObj.GetComponent<Camera>();
                camera.enabled=false;camera.orthographic=true;camera.orthographicSize=1.45f;camera.backgroundColor=new Color(.973f,.851f,.776f);
                camera.clearFlags=CameraClearFlags.SolidColor;camera.transform.position=new Vector3(0,.15f,-5);camera.transform.LookAt(new Vector3(0,.1f,0));
                var lamp=cameraObj.AddComponent<Light>();lamp.type=LightType.Directional;lamp.intensity=1.1f;RenderSettings.ambientLight=Color.white;
                var target=new RenderTexture(480,480,24);target.Create();camera.targetTexture=target;
                for(int frame=0;frame<30;frame++)
                {
                    driver.ApplyPose(WinterTrioReactions.Sample(character,action,frame/24f,false));camera.Render();
                    var previous=RenderTexture.active;RenderTexture.active=target;var pixels=new Texture2D(480,480,TextureFormat.RGB24,false);
                    pixels.ReadPixels(new Rect(0,0,480,480),0,0);pixels.Apply();RenderTexture.active=previous;
                    File.WriteAllBytes(Path.Combine(folder,frame.ToString("D3")+".png"),pixels.EncodeToPNG());Object.DestroyImmediate(pixels);
                }
                Object.DestroyImmediate(obj);Object.DestroyImmediate(cameraObj);target.Release();Object.DestroyImmediate(target);
            }
        }
    }
}
