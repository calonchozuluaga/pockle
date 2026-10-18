using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
    public sealed class ToyTemperamentTests
    {
        [UnityTest]
        public IEnumerator EveryAvailableNativeFinishHasEightFacesAndPreservesItsArtAndHandling()
        {
            var obj=new GameObject("All-character face test");var toy=obj.AddComponent<JellyToy>();toy.Initialize();int count=0;
            var tested=new HashSet<string>();
            try
            {
                foreach(var item in ToyCatalog.Collectibles)
                {
                    if(!item.Available||!ToyFaceLayout.TryGet(item.CharacterId,out _))continue;
                    Assert.That(CharacterArt.TryLoad(item.Id,out var source),Is.True);var original=source.BodyMesh.vertices;
                    Assert.That(toy.TrySetCollectible(item.Id),Is.True);var face=toy.GetComponentInChildren<ToyTemperamentDriver>();Assert.That(face,Is.Not.Null);
                    Assert.That(face.CycleEnabled,Is.True);var material=toy.BodyRenderer.sharedMaterial;
                    Assert.That(material.mainTexture,Is.SameAs(source.BaseColorTexture));Assert.That(material.shader.isSupported,Is.True);
                    var mesh=toy.BodyRenderer.GetComponent<MeshFilter>().sharedMesh;var rest=new List<Vector3>();mesh.GetUVs(1,rest);Assert.That(rest.Count,Is.EqualTo(mesh.vertexCount));
                    toy.SetDeformation(.2f,.05f,.04f,0,0,0,0);var deformedRest=new List<Vector3>();mesh.GetUVs(1,deformedRest);CollectionAssert.AreEqual(rest,deformedRest);toy.ResetToy();
                    var color=material.GetColor("_BodyColor");float opacity=material.GetFloat("_Opacity"),gloss=material.GetFloat("_Gloss");
                    foreach(PipTemperament style in System.Enum.GetValues(typeof(PipTemperament)))
                        for(int state=0;state<3;state++)
                        {
                            var pose=PipFaces.Pose(style,state);face.ApplyFace(pose);
                            Assert.That(material.GetFloat("_TemperFrown"),Is.EqualTo(pose.Frown));Assert.That(material.GetFloat("_TemperWink"),Is.EqualTo(pose.Wink));
                            Assert.That(material.GetColor("_BodyColor"),Is.EqualTo(color));Assert.That(material.GetFloat("_Opacity"),Is.EqualTo(opacity));Assert.That(material.GetFloat("_Gloss"),Is.EqualTo(gloss));
                        }
                    if(tested.Add(item.CharacterId))
                    {
                        for(int tap=0;tap<16;tap++){face.BeginTap(.2f);Assert.That(face.CurrentTemperament,Is.EqualTo(PipFaces.TestTemperament(tap)));face.CancelTap();Assert.That(face.IsReacting,Is.False);}
                        if(item.CharacterId=="ginger")Assert.That(material.GetVector("_TemperMouthColor").y,Is.GreaterThan(.9f),"Ginger keeps a cream icing mouth");
                        if(item.CharacterId=="fox"||item.CharacterId=="ginger"||item.CharacterId=="snowy"||item.CharacterId=="reiny"||item.CharacterId=="guin")Assert.That(Mathf.Abs(material.GetVector("_TemperNose").x),Is.LessThan(1),"Native nose must be protected");
                    }
                    CollectionAssert.AreEqual(original,source.BodyMesh.vertices,"Imported model stays immutable");count++;
                }
                Assert.That(count,Is.EqualTo(90));Assert.That(tested.Count,Is.EqualTo(9));
                var last=toy.GetComponentInChildren<ToyTemperamentDriver>();last.BeginTap(.1f);yield return new WaitForSecondsRealtime(.25f);yield return null;Assert.That(last.IsReacting,Is.False);
            }
            finally{Object.Destroy(obj);}
            yield return null;
        }
    }
}
