using System.IO;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
namespace Pockle.Editor
{
    public static class PipFusionMaterialCapture
    {
        public static void Capture()
        {
            string output="/private/tmp/pockle-pip-fusion-material-renders";Directory.CreateDirectory(output);
            foreach(string id in PipFusionStudies.Ids)
            {
                var obj=new GameObject("Pip fusion material preview");var toy=obj.AddComponent<JellyToy>();toy.Initialize();toy.TrySetCollectible(id);
                var camObj=new GameObject("Fusion camera",typeof(Camera));var camera=camObj.GetComponent<Camera>();camera.enabled=false;
                camera.orthographic=true;camera.orthographicSize=1.2f;camera.backgroundColor=new Color(.973f,.851f,.776f);camera.clearFlags=CameraClearFlags.SolidColor;
                camera.transform.position=new Vector3(0,.1f,-5);camera.transform.LookAt(Vector3.zero);
                var lamp=camObj.AddComponent<Light>();lamp.type=LightType.Directional;lamp.intensity=1.1f;RenderSettings.ambientLight=Color.white;
                var target=new RenderTexture(768,768,24);target.Create();camera.targetTexture=target;
                foreach(int angle in new[]{0,65,180,-1})
                {
                    bool night=angle<0;camera.backgroundColor=night?new Color(.035f,.045f,.09f):new Color(.973f,.851f,.776f);
                    var material=toy.BodyRenderer.sharedMaterial;material.SetColor("_StageTint",night?new Color(.50f,.55f,.70f):Color.white);material.SetFloat("_StageEmission",night?.04f:0);
                    obj.transform.rotation=Quaternion.Euler(0,night?0:angle,0);camera.Render();
                    var previous=RenderTexture.active;RenderTexture.active=target;var pixels=new Texture2D(768,768,TextureFormat.RGB24,false);
                    pixels.ReadPixels(new Rect(0,0,768,768),0,0);pixels.Apply();RenderTexture.active=previous;
                    File.WriteAllBytes(Path.Combine(output,id+"-"+(night?"night":angle.ToString())+".png"),pixels.EncodeToPNG());Object.DestroyImmediate(pixels);
                }
                Object.DestroyImmediate(obj);Object.DestroyImmediate(camObj);target.Release();Object.DestroyImmediate(target);
            }
        }
    }
}
