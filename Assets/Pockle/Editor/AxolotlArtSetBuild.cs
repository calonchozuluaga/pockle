using System;
using Pockle.Runtime;
using UnityEditor;
using UnityEngine;
namespace Pockle.Editor
{
    public static class AxolotlArtSetBuild
    {
        public static void Build()
        {
            foreach(string path in new[]{"Assets/Pockle/Resources/Collections/Axolotl/Cover.png","Assets/Pockle/Resources/UI/Boxes/Collection-axolotl.png","Assets/Pockle/Resources/UI/Boxes/Collection-axolotl-open.png","Assets/Pockle/Resources/Boxes/AxolotlFront.png","Assets/Pockle/Resources/Boxes/AxolotlSide.png","Assets/Pockle/Resources/Boxes/AxolotlLid.png"})
            {
                var textureImporter=(TextureImporter)AssetImporter.GetAtPath(path);
                textureImporter.textureType=TextureImporterType.Default;textureImporter.textureShape=TextureImporterShape.Texture2D;
                textureImporter.npotScale=TextureImporterNPOTScale.None;textureImporter.maxTextureSize=4096;
                textureImporter.alphaIsTransparency=true;textureImporter.SaveAndReimport();
            }
            const string modelPath="Assets/Pockle/Art/Models/AxolotlBox/AxolotlCollectionBox.fbx";
            var importer=(ModelImporter)AssetImporter.GetAtPath(modelPath);
            importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.importAnimation=true;importer.animationType=ModelImporterAnimationType.Legacy;
            importer.SaveAndReimport();
            const string folder="Assets/Pockle/Resources/Boxes";
            var paper=Material(folder+"/AxolotlPaper.mat",new Color(1,.84f,.74f),null);
            var front=Material(folder+"/AxolotlFront.mat",Color.white,AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"/AxolotlFront.png"));
            var side=Material(folder+"/AxolotlSide.mat",Color.white,AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"/AxolotlSide.png"));
            var lid=Material(folder+"/AxolotlLid.mat",Color.white,AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"/AxolotlLid.png"));
            var model=AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);var instance=UnityEngine.Object.Instantiate(model);instance.name="AxolotlCollectionBox";
            try
            {
                foreach(var renderer in instance.GetComponentsInChildren<Renderer>())
                {
                    string name=renderer.name;
                    renderer.sharedMaterial=name.Contains("mystery print")?front:name.Contains("pattern print")?side:name.Contains("brand print")?lid:paper;
                }
                AnimationClip imported=null;
                foreach(var asset in AssetDatabase.LoadAllAssetsAtPath(modelPath))if(asset is AnimationClip clip&&!clip.name.StartsWith("__")){imported=clip;break;}
                if(imported==null)throw new InvalidOperationException("Box opening animation missing.");
                const string clipPath=folder+"/AxolotlBox_Open.anim";
                var opening=AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                if(opening==null){opening=UnityEngine.Object.Instantiate(imported);opening.name="AxolotlBox_Open";AssetDatabase.CreateAsset(opening,clipPath);}
                else {EditorUtility.CopySerialized(imported,opening);opening.name="AxolotlBox_Open";EditorUtility.SetDirty(opening);}
                var native=instance.AddComponent<NativeCollectionBox>();native.OpeningClip=opening;
                var animation=instance.GetComponent<Animation>();if(animation!=null){animation.playAutomatically=false;animation.clip=opening;}
                PrefabUtility.SaveAsPrefabAsset(instance,folder+"/AxolotlCollectionBox.prefab");AssetDatabase.SaveAssets();
                Debug.Log("AXOLOTL_ART_BOX_READY "+opening.length+"s opening, "+instance.GetComponentsInChildren<Renderer>().Length+" renderers");
            }
            finally{UnityEngine.Object.DestroyImmediate(instance);}
        }
        private static Material Material(string path,Color color,Texture texture)
        {
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null){material=new Material(Shader.Find("Pockle/Reveal Box"));AssetDatabase.CreateAsset(material,path);}
            material.color=color;material.mainTexture=texture;EditorUtility.SetDirty(material);return material;
        }
    }
}
