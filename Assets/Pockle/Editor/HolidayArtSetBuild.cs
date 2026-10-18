using System;
using Pockle.Runtime;
using UnityEditor;
using UnityEngine;
namespace Pockle.Editor
{
    public static class HolidayArtSetBuild
    {
        public static void Build()
        { foreach(string id in new[]{"firb","coco","snowy","ginger"}) BuildOne(id); }
        private static void BuildOne(string id)
        {
            string title=Pockle.Core.HolidayCatalog.Title(id);
            foreach(string path in new[]{"Assets/Pockle/Resources/Collections/"+title+"/Cover.png","Assets/Pockle/Resources/UI/Boxes/Collection-"+id+".png","Assets/Pockle/Resources/UI/Boxes/Collection-"+id+"-open.png","Assets/Pockle/Resources/Boxes/"+title+"Front.png","Assets/Pockle/Resources/Boxes/"+title+"Side.png","Assets/Pockle/Resources/Boxes/"+title+"Lid.png"})
            {
                var textureImporter=(TextureImporter)AssetImporter.GetAtPath(path);
                textureImporter.textureType=TextureImporterType.Default;textureImporter.textureShape=TextureImporterShape.Texture2D;
                textureImporter.npotScale=TextureImporterNPOTScale.None;textureImporter.maxTextureSize=4096;
                textureImporter.alphaIsTransparency=true;textureImporter.SaveAndReimport();
            }
            string modelPath="Assets/Pockle/Art/Models/"+title+"Box/"+title+"CollectionBox.fbx";
            var importer=(ModelImporter)AssetImporter.GetAtPath(modelPath);
            importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.importAnimation=true;importer.animationType=ModelImporterAnimationType.Legacy;
            importer.SaveAndReimport();
            const string folder="Assets/Pockle/Resources/Boxes";
            var paper=Material(folder+"/"+title+"Paper.mat",new Color(1,.84f,.74f),null);
            var front=Material(folder+"/"+title+"Front.mat",Color.white,AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"/"+title+"Front.png"));
            var side=Material(folder+"/"+title+"Side.mat",Color.white,AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"/"+title+"Side.png"));
            var lid=Material(folder+"/"+title+"Lid.mat",Color.white,AssetDatabase.LoadAssetAtPath<Texture2D>(folder+"/"+title+"Lid.png"));
            var model=AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);var instance=UnityEngine.Object.Instantiate(model);instance.name=title+"CollectionBox";
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
                string clipPath=folder+"/"+title+"Box_Open.anim";
                var opening=AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
                if(opening==null){opening=UnityEngine.Object.Instantiate(imported);opening.name=title+"Box_Open";AssetDatabase.CreateAsset(opening,clipPath);}
                else {EditorUtility.CopySerialized(imported,opening);opening.name=title+"Box_Open";EditorUtility.SetDirty(opening);}
                var native=instance.AddComponent<NativeCollectionBox>();native.OpeningClip=opening;
                var animation=instance.GetComponent<Animation>();if(animation!=null){animation.playAutomatically=false;animation.clip=opening;}
                PrefabUtility.SaveAsPrefabAsset(instance,folder+"/"+title+"CollectionBox.prefab");AssetDatabase.SaveAssets();
                Debug.Log("HOLIDAY_ART_BOX_READY "+opening.length+"s opening, "+instance.GetComponentsInChildren<Renderer>().Length+" renderers");
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
