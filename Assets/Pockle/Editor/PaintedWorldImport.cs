using Pockle.Runtime;
using UnityEditor;
using UnityEngine;
namespace Pockle.Editor
{
    public static class PaintedWorldImport
    {
        [MenuItem("Pockle/Art/Build painted world reflection maps")]
        public static void Build()
        {
            foreach(string name in new[]{"fox-hollow","jelly-garden","moonlit-pond","fox-riverbank","fox-autumn-orchard","fox-snowy-den","fox-aurora-grove","pip-jelly-garden","pip-mint-spring","pip-pearl-lagoon","pip-confetti-meadow","pip-starlight-grove"})
            {
                string prefix="Assets/Pockle/Resources/Worlds/"+name;
                var importer=(TextureImporter)AssetImporter.GetAtPath(prefix+".png");
                importer.textureType=TextureImporterType.Default;importer.textureShape=TextureImporterShape.Texture2D;
                importer.isReadable=true;importer.mipmapEnabled=true;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;
                importer.wrapMode=TextureWrapMode.Clamp;importer.SaveAndReimport();
                var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(prefix+".png");
                const int size=128;var cube=new Cubemap(size,TextureFormat.RGBA32,true){name=name+" · baked LDR environment",filterMode=FilterMode.Trilinear};
                var pixels=new Color[size*size];
                for(int face=0;face<6;face++)
                {
                    for(int y=0;y<size;y++)for(int x=0;x<size;x++)
                    {
                        var d=Direction(face,2f*(x+.5f)/size-1,2f*(y+.5f)/size-1).normalized;
                        // Camera faces +Z; the panorama centre is directly behind the toy.
                        float u=.5f+Mathf.Atan2(d.x,d.z)/(2*Mathf.PI),v=.5f+Mathf.Asin(d.y)/Mathf.PI;
                        pixels[y*size+x]=texture.GetPixelBilinear(u,v);
                    }
                    cube.SetPixels(pixels,(CubemapFace)face);
                }
                cube.Apply(true,true);
                string cubePath=prefix+"-reflection.asset";
                var existingCube=AssetDatabase.LoadAssetAtPath<Cubemap>(cubePath);
                if(existingCube==null)AssetDatabase.CreateAsset(cube,cubePath);
                else {EditorUtility.CopySerialized(cube,existingCube);Object.DestroyImmediate(cube);cube=existingCube;EditorUtility.SetDirty(cube);}
                var asset=AssetDatabase.LoadAssetAtPath<PaintedWorldAsset>(prefix+".asset");
                if(asset==null){asset=ScriptableObject.CreateInstance<PaintedWorldAsset>();AssetDatabase.CreateAsset(asset,prefix+".asset");}
                asset.Background=texture;asset.Reflections=cube;EditorUtility.SetDirty(asset);
                importer.isReadable=false;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Compressed;
                importer.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();
        }
        private static Vector3 Direction(int face,float u,float v)
        {
            switch((CubemapFace)face)
            {
                case CubemapFace.PositiveX:return new Vector3(1,-v,-u);
                case CubemapFace.NegativeX:return new Vector3(-1,-v,u);
                case CubemapFace.PositiveY:return new Vector3(u,1,v);
                case CubemapFace.NegativeY:return new Vector3(u,-1,-v);
                case CubemapFace.PositiveZ:return new Vector3(u,-v,1);
                default:return new Vector3(-u,-v,-1);
            }
        }
    }
}
