using System.Collections;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
    public sealed class CharacterStudyTests
    {
        [Test]
        public void RetiredStudiesCannotLoadOrReturnToTheCatalog()
        {
            foreach(string id in new[]{"loop","mallow","moss","nook","ripple","rolo","sprig","wisp","bop","dew"})
            {
                Assert.That(ToyCatalog.TryGetCharacter(id,out _),Is.False);Assert.That(ToyCatalog.TryGetCollection(id,out _),Is.False);
                Assert.That(CharacterArt.IsStudy(id+".boucle-plush"),Is.False);
                foreach(var finish in ToyCatalog.Finishes)Assert.That(CharacterArt.TryLoad(id+"."+finish.Id,out _),Is.False);
            }
            foreach(string name in new[]{"Moss","Nook","Bop"})Assert.That(Resources.Load<PipCharacterAsset>("Characters/"+name+"/"+name),Is.Null);
        }
        [UnityTest]
        public IEnumerator CurrentCharactersSwitchWithoutRetainingOldMeshes()
        {
            var root=new GameObject("Current character fixture");var toy=root.AddComponent<JellyToy>();toy.Initialize();
            try
            {
                foreach(string id in new[]{ToyCatalog.FoxId,ToyCatalog.AxolotlId,"firb.matte-vinyl","coco.matte-vinyl","snowy.matte-vinyl","ginger.matte-vinyl",ToyCatalog.PeachId})
                {
                    var previous=toy.BodyRenderer.GetComponent<MeshFilter>().sharedMesh;
                    Assert.That(toy.TrySetCollectible(id),Is.True,id);yield return null;
                    Assert.That(previous==null,Is.True,"Previous instance mesh released.");
                    Assert.That(toy.BodyRenderer.GetComponent<MeshFilter>().sharedMesh.vertexCount,Is.LessThanOrEqualTo(65000));
                }
            }
            finally{Object.Destroy(root);}
            yield return null;
        }
    }
}
