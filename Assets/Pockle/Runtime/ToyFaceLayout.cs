using System;
using System.Collections.Generic;
using UnityEngine;
namespace Pockle.Runtime
{
    public static class ToyFaceLayout
    {
        public readonly struct Layout
        {
            public readonly Vector4 Left,Right,Mouth;
            public readonly float Front;
            public Layout(Vector4 left,Vector4 right,Vector4 mouth,float front=-.3f){Left=left;Right=right;Mouth=mouth;Front=front;}
        }
        private static readonly Dictionary<string,Vector2[]> skinUvs=new Dictionary<string,Vector2[]>();
        public static bool TryGet(string character,out Layout layout)
        {
            switch(character)
            {
                case "reiny":layout=new Layout(new Vector4(-.275f,.19f,.068f,.09f),new Vector4(.275f,.19f,.068f,.09f),new Vector4(0,.01f,.073f,.026f),-.12f);return true;
                case "guin":layout=new Layout(new Vector4(-.283f,.284f,.068f,.09f),new Vector4(.283f,.284f,.068f,.09f),new Vector4(0,.129f,.075f,.026f),-.12f);return true;
                case "bundy":layout=new Layout(new Vector4(-.325f,-.285f,.08f,.10f),new Vector4(.325f,-.285f,.08f,.10f),new Vector4(0,-.361f,.073f,.027f),-.12f);return true;
                case "fox":layout=new Layout(new Vector4(-.18f,.05f,.105f,.13f),new Vector4(.38f,.04f,.105f,.13f),new Vector4(.085f,-.173f,.073f,.028f));return true;
                case "axolotl":layout=new Layout(new Vector4(-.27f,.34f,.075f,.085f),new Vector4(.25f,.34f,.075f,.085f),new Vector4(0,.23f,.09f,.035f),-.12f);return true;
                case "firb":layout=new Layout(new Vector4(-.225f,-.08f,.075f,.09f),new Vector4(.225f,-.08f,.075f,.09f),new Vector4(0,-.13f,.07f,.03f),-.12f);return true;
                case "coco":layout=new Layout(new Vector4(-.58f,-.09f,.085f,.105f),new Vector4(.11f,-.09f,.085f,.105f),new Vector4(-.22f,-.145f,.08f,.032f),-.12f);return true;
                case "snowy":layout=new Layout(new Vector4(-.22f,.20f,.065f,.08f),new Vector4(.22f,.20f,.065f,.08f),new Vector4(0,.04f,.075f,.025f),-.12f);return true;
                case "ginger":layout=new Layout(new Vector4(-.30f,.18f,.085f,.095f),new Vector4(.30f,.18f,.085f,.095f),new Vector4(0,.10f,.075f,.026f),-.12f);return true;
                default:layout=default;return false;
            }
        }
        public static void Apply(Material material,PipCharacterAsset asset)
        {
            if(!TryGet(asset.CharacterId,out var layout))return;
            if(!skinUvs.TryGetValue(asset.CharacterId,out var uv))
            {
                var mesh=asset.BodyMesh;var vertices=mesh.vertices;var indices=mesh.triangles;var source=mesh.uv;
                // Sample unpainted skin above each eye and just below the smile, preserving noses and patterned faces.
                uv=new[]{Project(asset.CharacterId=="fox"?new Vector2(layout.Left.x-.20f,layout.Left.y-.01f):new Vector2(layout.Left.x,layout.Left.y+layout.Left.w*1.8f),vertices,indices,source),
                    Project(asset.CharacterId=="fox"?new Vector2(layout.Right.x+.20f,layout.Right.y-.01f):new Vector2(layout.Right.x,layout.Right.y+layout.Right.w*1.8f),vertices,indices,source),
                    Project(new Vector2(layout.Mouth.x,layout.Mouth.y-layout.Mouth.w*2),vertices,indices,source)};
                if(asset.CharacterId=="fox"){uv[0]=uv[2];uv[1]=uv[2];}
                if(asset.CharacterId=="snowy"){var white=Project(new Vector2(0,.33f),vertices,indices,source);uv[0]=white;uv[1]=white;uv[2]=white;}
                skinUvs.Add(asset.CharacterId,uv);
            }
            material.SetVector("_TemperLeft",layout.Left);material.SetVector("_TemperRight",layout.Right);material.SetVector("_TemperMouth",layout.Mouth);
            material.SetFloat("_TemperFront",layout.Front);
            // Axolotl's baked eye halos extend well beyond the painted dark ovals.
            float cleanup=asset.CharacterId=="axolotl"?2.2f:asset.CharacterId=="snowy"?1.6f:1.35f;
            var leftClean=layout.Left;leftClean.z*=cleanup;leftClean.w*=cleanup;
            var rightClean=layout.Right;rightClean.z*=cleanup;rightClean.w*=cleanup;
            material.SetVector("_TemperLeftClean",leftClean);material.SetVector("_TemperRightClean",rightClean);
            material.SetFloat("_TemperKeepEyePatch",asset.CharacterId=="fox"?1:0);
            material.SetFloat("_TemperSkinDetail",asset.CharacterId=="ginger"?.06f:asset.CharacterId=="coco"?.035f:0);
            var clean=layout.Mouth;
            clean.z=asset.CharacterId=="snowy"?.14f:asset.CharacterId=="ginger"?.14f:asset.CharacterId=="coco"?.13f:asset.CharacterId=="firb"?.11f:asset.CharacterId=="fox"?.12f:.16f;
            clean.w=asset.CharacterId=="snowy"?.075f:asset.CharacterId=="ginger"?.065f:asset.CharacterId=="coco"?.075f:asset.CharacterId=="firb"?.065f:asset.CharacterId=="fox"?.05f:.065f;
            material.SetVector("_TemperMouthClean",clean);
            Vector4 nose=asset.CharacterId=="reiny"?new Vector4(0,.12f,.095f,.065f):asset.CharacterId=="guin"?new Vector4(0,.23f,.105f,.065f):asset.CharacterId=="fox"?new Vector4(.085f,-.095f,.075f,.045f):asset.CharacterId=="ginger"?new Vector4(0,.175f,.06f,.035f):asset.CharacterId=="snowy"?new Vector4(0,.13f,.072f,.068f):new Vector4(5,5,.01f,.01f);
            material.SetVector("_TemperNose",nose);
            material.SetVector("_TemperMouthColor",asset.CharacterId=="ginger"?new Vector4(1,.94f,.68f,2.3f):new Vector4(.035f,.008f,.02f,1));
            material.SetVector("_TemperSkinLeft",uv[0]);material.SetVector("_TemperSkinRight",uv[1]);material.SetVector("_TemperSkinMouth",uv[2]);
        }
        private static Vector2 Project(Vector2 point,Vector3[] vertices,int[] triangles,Vector2[] uv)
        {
            float nearest=float.PositiveInfinity;Vector2 result=default;bool found=false;
            for(int i=0;i<triangles.Length;i+=3)
            {
                int ia=triangles[i],ib=triangles[i+1],ic=triangles[i+2];var a=vertices[ia];var b=vertices[ib];var c=vertices[ic];
                float det=(b.y-c.y)*(a.x-c.x)+(c.x-b.x)*(a.y-c.y);if(Mathf.Abs(det)<.0000001f)continue;
                float u=((b.y-c.y)*(point.x-c.x)+(c.x-b.x)*(point.y-c.y))/det;
                float v=((c.y-a.y)*(point.x-c.x)+(a.x-c.x)*(point.y-c.y))/det;float w=1-u-v;
                if(u<0||v<0||w<0)continue;float z=u*a.z+v*b.z+w*c.z;if(z>=nearest)continue;
                nearest=z;result=uv[ia]*u+uv[ib]*v+uv[ic]*w;found=true;
            }
            if(!found)throw new InvalidOperationException("Face skin anchor must hit the native front surface");
            return result;
        }
    }
}
