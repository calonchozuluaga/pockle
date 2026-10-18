using System;
using System.IO;
using Pockle.Runtime;
using Pockle.Core;
using UnityEngine;
namespace Pockle.Editor
{
    /// <summary>Offline review renders only. Does not register outcomes, change preferences or grant inventory.</summary>
    public static class PipFusionGalleryCapture
    {
        [Serializable] public sealed class Manifest { public Recipe[] recipes; }
        [Serializable] public sealed class Recipe
        {
            public string key,specialId;
            public float[] colorA,colorB;
            public float mode,roughness,reflection,opacity,transmission,pearl,glitter,fuzz,metal,glow,iridescence,zwrite;
        }
        private static Color Tint(float[] c)=>new Color(c[0],c[1],c[2],1);
        public static void Capture()
        {
            var args=Environment.GetCommandLineArgs();int index=Array.IndexOf(args,"-fusionGallery");
            if(index<0||index+1>=args.Length)throw new ArgumentException("Supply -fusionGallery <output folder>.");
            string output=args[index+1];var manifest=JsonUtility.FromJson<Manifest>(File.ReadAllText(Path.Combine(output,"recipes.json")));
            if(manifest.recipes==null||manifest.recipes.Length!=55)throw new InvalidOperationException("Expected exactly55 review recipes.");
            string folder=Path.Combine(output,"renders");Directory.CreateDirectory(folder);
            foreach(var r in manifest.recipes)
            {
                var obj=new GameObject("Offline fusion review "+r.key);
                var camObj=new GameObject("Review camera",typeof(Camera));RenderTexture target=null;
                try
                {
                    var toy=obj.AddComponent<JellyToy>();toy.Initialize();
                    if(!toy.TrySetCollectible(string.IsNullOrEmpty(r.specialId)?PipFusionStudies.SorbetId:r.specialId))throw new InvalidOperationException("Missing Pip reference mesh.");
                    var m=toy.BodyRenderer.sharedMaterial;
                    if(string.IsNullOrEmpty(r.specialId))
                    {
                        m.SetFloat("_FusionMode",r.mode);m.SetColor("_FinishTint",Tint(r.colorA));m.SetFloat("_TintStrength",1);
                        m.SetColor("_FusionColorA",Tint(r.colorA));m.SetColor("_FusionColorB",Tint(r.colorB));
                        m.SetFloat("_Roughness",r.roughness);m.SetFloat("_ReflectionStrength",r.reflection);
                        m.SetFloat("_Opacity",r.opacity);m.SetFloat("_Transmission",r.transmission);
                        m.SetFloat("_PearlSheen",r.pearl);m.SetFloat("_GlitterStrength",r.glitter);
                        m.SetFloat("_Fuzz",r.fuzz);m.SetFloat("_Metallic",r.metal);m.SetFloat("_Glow",r.glow);m.SetFloat("_Iridescence",r.iridescence);
                        m.SetFloat("_ZWrite",r.zwrite);m.SetFloat("_SpeckEmission",0);m.renderQueue=r.zwrite>.5f?2001:3000;
                    }
                    var camera=camObj.GetComponent<Camera>();camera.enabled=false;camera.orthographic=true;camera.orthographicSize=1.2f;
                    camera.clearFlags=CameraClearFlags.SolidColor;camera.transform.position=new Vector3(0,.1f,-5);camera.transform.LookAt(Vector3.zero);
                    RenderSettings.ambientLight=Color.white;target=new RenderTexture(512,512,24);target.Create();camera.targetTexture=target;
                    foreach(string view in new[]{"front","side","night"})
                    {
                        bool night=view=="night";obj.transform.rotation=Quaternion.Euler(0,view=="side"?65:0,0);
                        camera.backgroundColor=night?new Color(.035f,.045f,.09f):new Color(.973f,.851f,.776f);
                        m.SetColor("_StageTint",night?new Color(.50f,.55f,.70f):Color.white);m.SetFloat("_StageEmission",night&&r.glow>0?.04f:0);
                        camera.Render();var previous=RenderTexture.active;RenderTexture.active=target;var pixels=new Texture2D(512,512,TextureFormat.RGB24,false);
                        try{pixels.ReadPixels(new Rect(0,0,512,512),0,0);pixels.Apply();File.WriteAllBytes(Path.Combine(folder,r.key+"-"+view+".png"),pixels.EncodeToPNG());}
                        finally{RenderTexture.active=previous;UnityEngine.Object.DestroyImmediate(pixels);}
                    }
                }
                finally{UnityEngine.Object.DestroyImmediate(obj);UnityEngine.Object.DestroyImmediate(camObj);if(target!=null){target.Release();UnityEngine.Object.DestroyImmediate(target);}}
            }
            Debug.Log("PIP_FUSION_GALLERY_OK 55 recipes / 165 renders; no runtime catalog or inventory changes.");
        }
    }
}
