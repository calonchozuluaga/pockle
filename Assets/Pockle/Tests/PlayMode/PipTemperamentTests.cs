using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
    public sealed class PipTemperamentTests
    {
        [UnityTest]
        public IEnumerator FacesFollowDeformationCycleEightAndNeverChangeFinishes()
        {
            bool had=PlayerPrefs.HasKey(PipCharacterAsset.MeshyPreference);int previous=PlayerPrefs.GetInt(PipCharacterAsset.MeshyPreference);
            PlayerPrefs.SetInt(PipCharacterAsset.MeshyPreference,1);
            var obj=new GameObject("Runtime face test");var toy=obj.AddComponent<JellyToy>();toy.Initialize();
            var reaction=obj.AddComponent<PipReactionDriver>();reaction.Initialize(toy);
            try
            {
                foreach(string id in new[]{ToyCatalog.PeachId,ToyCatalog.MoonId,ToyCatalog.GoldId,ToyCatalog.MintId,"pip.velvet-flock","pip.boucle-plush","pip.matte-vinyl","pip.gloss-vinyl","pip.coated-metallic","pip.glow-jelly",PipFusionStudies.DuskId,PipFusionStudies.SorbetId,PipFusionStudies.HoneydewId,PipFusionStudies.EclipseId})
                {
                    Assert.That(toy.TrySetCollectible(id),Is.True);
                    var face=toy.GetComponentInChildren<PeachExpressionDriver>();Assert.That(face,Is.Not.Null);
                    var mesh=toy.BodyRenderer.GetComponent<MeshFilter>().sharedMesh;
                    var rest=new List<Vector3>();mesh.GetUVs(1,rest);Assert.That(rest.Count,Is.EqualTo(mesh.vertexCount));
                    var before=mesh.vertices;
                    for(int i=0;i<before.Length;i++)Assert.That(rest[i],Is.EqualTo(before[i]),"Rest coordinates must match neutral vertices");
                    toy.SetDeformation(.2f,.08f,.04f,0,0,0,0);
                    var after=new List<Vector3>();mesh.GetUVs(1,after);CollectionAssert.AreEqual(rest,after,"Face coordinates stay attached through deformation");
                    toy.ResetToy();
                    var material=toy.BodyRenderer.sharedMaterial;var paint=material.mainTexture;
                    var tint=material.GetColor("_FinishTint");float opacity=material.GetFloat("_Opacity"),glow=material.GetFloat("_Glow"),fusion=material.GetFloat("_FusionMode");
                    foreach(PipTemperament style in System.Enum.GetValues(typeof(PipTemperament)))
                    {
                        face.ApplyFace(PipFaces.Pose(style,2));
                        Assert.That(material.GetFloat("_Blink"),Is.EqualTo(PipFaces.Pose(style,2).Blink));
                        Assert.That(material.GetColor("_FinishTint"),Is.EqualTo(tint));Assert.That(material.GetFloat("_Opacity"),Is.EqualTo(opacity));
                        Assert.That(material.GetFloat("_Glow"),Is.EqualTo(glow));Assert.That(material.GetFloat("_FusionMode"),Is.EqualTo(fusion));Assert.That(material.mainTexture,Is.SameAs(paint));
                    }
                }
                toy.TrySetCollectible(ToyCatalog.PeachId);
                var pipFace=toy.GetComponentInChildren<PeachExpressionDriver>();Assert.That(pipFace.CycleEnabled,Is.True,"This development build includes the owner test resource");
                for(int i=0;i<16;i++)
                {
                    Assert.That(reaction.TryPlay(PipAction.HappyWiggle),Is.True);Assert.That(pipFace.CurrentTemperament,Is.EqualTo(PipFaces.TestTemperament(i)));
                    pipFace.ApplyReaction(PipAction.HappyWiggle,.8f,default);
                    Assert.That(toy.BodyRenderer.sharedMaterial.GetFloat("_Wink"),Is.EqualTo(PipFaces.Pose(pipFace.CurrentTemperament,2).Wink));
                    reaction.Cancel();Assert.That(toy.BodyRenderer.sharedMaterial.GetFloat("_Blink"),Is.Zero);
                }
                CollectionAssert.IsEmpty(Resources.Load<PipCharacterAsset>(PipCharacterAsset.MeshyResourcePath).BodyMesh.uv2,"Imported source mesh remains untouched");
            }
            finally{Object.Destroy(obj);if(had)PlayerPrefs.SetInt(PipCharacterAsset.MeshyPreference,previous);else PlayerPrefs.DeleteKey(PipCharacterAsset.MeshyPreference);}
            yield return null;
        }
    }
}
