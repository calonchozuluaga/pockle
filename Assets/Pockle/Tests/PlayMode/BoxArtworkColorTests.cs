using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
    public sealed class BoxArtworkColorTests
    {
        [UnityTest]
        public IEnumerator PackagingPrintKeepsItsColorAcrossFaceNormals()
        {
            var root=new GameObject("Box print color regression");
            var cameraRoot=new GameObject("Box print color camera",typeof(Camera));
            var mesh=new Mesh{vertices=new[]{new Vector3(-1,-1,0),new Vector3(1,-1,0),new Vector3(-1,1,0),new Vector3(1,1,0)},uv=new[]{Vector2.zero,Vector2.right,Vector2.up,Vector2.one},triangles=new[]{0,2,1,2,3,1}};
            var paint=new Texture2D(2,2,TextureFormat.RGBA32,false);
            paint.SetPixels(new[]{new Color(.85f,.14f,.46f),new Color(.85f,.14f,.46f),new Color(.85f,.14f,.46f),new Color(.85f,.14f,.46f)});paint.Apply();
            var material=new Material(Shader.Find("Pockle/Reveal Box")){mainTexture=paint,color=Color.white};
            var target=new RenderTexture(32,32,24);var pixels=new Texture2D(32,32,TextureFormat.RGBA32,false);
            try
            {
                root.layer=28;root.transform.position=new Vector3(1000,0,0);
                root.AddComponent<MeshFilter>().sharedMesh=mesh;root.AddComponent<MeshRenderer>().sharedMaterial=material;
                var camera=cameraRoot.GetComponent<Camera>();camera.enabled=false;camera.cullingMask=1<<28;camera.orthographic=true;camera.orthographicSize=1.2f;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;
                camera.transform.position=new Vector3(1000,0,-5);camera.transform.rotation=Quaternion.identity;camera.targetTexture=target;target.Create();
                Color[] colors=new Color[2];int i=0;
                foreach(var normal in new[]{Vector3.up,Vector3.right})
                {
                    mesh.normals=new[]{normal,normal,normal,normal};camera.Render();var previous=RenderTexture.active;
                    try{RenderTexture.active=target;pixels.ReadPixels(new Rect(0,0,32,32),0,0);pixels.Apply();colors[i++]=pixels.GetPixel(16,16);}
                    finally{RenderTexture.active=previous;}
                }
                Assert.That(colors[0].r,Is.GreaterThan(.65f),"The print must remain bright.");
                Assert.That(Mathf.Abs(colors[0].r-colors[1].r)+Mathf.Abs(colors[0].g-colors[1].g)+Mathf.Abs(colors[0].b-colors[1].b),Is.LessThan(.005f),"Native carton normals must not wash out or darken the artwork.");
            }
            finally
            {
                Object.Destroy(root);Object.Destroy(cameraRoot);Object.Destroy(mesh);Object.Destroy(material);Object.Destroy(paint);Object.Destroy(pixels);target.Release();Object.Destroy(target);
            }
            yield return null;
        }
    }
}
