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
            foreach(string path in new[]{"Assets/Pockle/Resources/Collections/Fox/Cover.png","Assets/Pockle/Resources/UI/Boxes/Collection-fox.png","Assets/Pockle/Resources/UI/Boxes/Collection-fox-open.png"})
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
            var paper=Material(folder+"/FoxPaper.mat",new Color(.96f,.89f,.76f),null);
            var ink=Material(folder+"/FoxInk.mat",new Color(.10f,.22f,.15f),null);
            var accent=Material(folder+"/FoxAccent.mat",new Color(.77f,.24f,.09f),null);
            var forest=Material(folder+"/FoxForest.mat",Color.white,AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Pockle/Resources/Worlds/fox-hollow.png"));
            var cover=Material(folder+"/FoxCover.mat",Color.white,AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Pockle/Resources/Collections/Fox/Cover.png"));
            var model=AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);var instance=UnityEngine.Object.Instantiate(model);instance.name="FoxCollectionBox";
            try
            {
                foreach(var renderer in instance.GetComponentsInChildren<Renderer>())
                {
                    string name=renderer.name;
                    renderer.sharedMaterial=name.Contains("forest print")?forest:name.Contains("cover print")?cover:name=="Printed FOX"?accent:name.StartsWith("Printed ")?ink:paper;
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
