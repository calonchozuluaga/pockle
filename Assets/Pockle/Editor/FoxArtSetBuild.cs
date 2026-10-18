using System;
using Pockle.Runtime;
using UnityEditor;
using UnityEngine;
namespace Pockle.Editor
{
    public static class FoxArtSetBuild
    {
        public static void Build()
        {
            foreach(string path in new[]{"Assets/Pockle/Resources/Collections/Fox/Cover.png","Assets/Pockle/Resources/UI/Boxes/Collection-fox.png","Assets/Pockle/Resources/UI/Boxes/Collection-fox-open.png","Assets/Pockle/Resources/Boxes/FoxFront.png","Assets/Pockle/Resources/Boxes/FoxSide.png","Assets/Pockle/Resources/Boxes/FoxLid.png"})
            {
                var textureImporter=(TextureImporter)AssetImporter.GetAtPath(path);
                textureImporter.textureType=TextureImporterType.Default;textureImporter.textureShape=TextureImporterShape.Texture2D;
                textureImporter.npotScale=TextureImporterNPOTScale.None;textureImporter.maxTextureSize=4096;
                textureImporter.alphaIsTransparency=true;textureImporter.SaveAndReimport();
            }
            const string modelPath="Assets/Pockle/Art/Models/FoxBox/FoxCollectionBox.fbx";
            var importer=(ModelImporter)AssetImporter.GetAtPath(modelPath);
            importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.importAnimation=true;importer.animationType=ModelImporterAnimationType.Legacy;
            importer.SaveAndReimport();
            const string folder="Assets/Pockle/Resources/Boxes";
            var paper=Material(folder+"/FoxPaper.mat",new Color(1,.84f,.74f),null);
            var front=Material(folder+"/FoxFront.mat",Color.white,AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"/FoxFront.png"));
            var side=Material(folder+"/FoxSide.mat",Color.white,AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"/FoxSide.png"));
            var lid=Material(folder+"/FoxLid.mat",Color.white,AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"/FoxLid.png"));
            var model=AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);var instance=UnityEngine.Object.Instantiate(model);instance.name="FoxCollectionBox";
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
                const string clipPath=folder+"/FoxBox_Open.anim";
                var opening=AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                if(opening==null){opening=UnityEngine.Object.Instantiate(imported);opening.name="FoxBox_Open";AssetDatabase.CreateAsset(opening,clipPath);}
                else {EditorUtility.CopySerialized(imported,opening);opening.name="FoxBox_Open";EditorUtility.SetDirty(opening);}
                var native=instance.AddComponent<NativeCollectionBox>();native.OpeningClip=opening;
                var animation=instance.GetComponent<Animation>();if(animation!=null){animation.playAutomatically=false;animation.clip=opening;}
                PrefabUtility.SaveAsPrefabAsset(instance,folder+"/FoxCollectionBox.prefab");AssetDatabase.SaveAssets();
                Debug.Log("FOX_ART_BOX_READY "+opening.length+"s opening, "+instance.GetComponentsInChildren<Renderer>().Length+" renderers");
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
