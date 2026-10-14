using System.Collections;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests {
 public sealed class AxolotlCollectionTests {
  [UnityTest] public IEnumerator AxolotlUsesUploadedPlushArtAndSwitchesWithoutGrantingOwnership() {
   Assert.That(CharacterArt.TryLoad(ToyCatalog.AxolotlId,out var asset),Is.True);
   Assert.That(asset.CharacterId,Is.EqualTo("axolotl"));
   Assert.That(asset.BodyMesh.triangles.Length/3,Is.EqualTo(30000));
   Assert.That(asset.BodyMesh.vertexCount,Is.LessThan(65000));
   Assert.That(asset.BodyMesh.bounds.min.y,Is.EqualTo(-1).Within(.001));
   Assert.That(asset.BodyMesh.bounds.size.x,Is.LessThanOrEqualTo(2.051f));
   var root = new GameObject("Axolotl collection regression");
   try {
    var toy=root.AddComponent<JellyToy>();
    Assert.That(toy.TrySetCollectible(ToyCatalog.AxolotlId),Is.True);
    Assert.That(toy.Handling,Is.SameAs(MaterialHandling.Plush));
    Assert.That(toy.UsesMeshyPeach,Is.False);
    Assert.That(toy.VisualRoot.childCount,Is.EqualTo(1),"Uploaded face/gills must not gain procedural Pip features.");
    Assert.That(toy.BodyRenderer.sharedMaterial.shader.name,Is.EqualTo("Pockle/Axolotl Plush"));
    Assert.That(toy.BodyRenderer.sharedMaterial.mainTexture,Is.SameAs(asset.BaseColorTexture));
    toy.SetDeformation(.2f,.1f,.1f,0,0,0);
    foreach(var v in toy.BodyRenderer.GetComponent<MeshFilter>().sharedMesh.vertices) Assert.That(float.IsNaN(v.x)||float.IsInfinity(v.y),Is.False);
    Assert.That(toy.TrySetCollectible(ToyCatalog.FoxAuroraId),Is.True);
    Assert.That(toy.TrySetCollectible(ToyCatalog.AxolotlId),Is.True);
    Assert.That(toy.TrySetCollectible("axolotl.clear-jelly"),Is.False);
    var fresh=new CollectionProgress(null,CollectionSession.Today);
    Assert.That(fresh.GetOwnedCount(ToyCatalog.AxolotlId),Is.Zero);
    fresh.EnsureFoxFinishTestCopies(); Assert.That(fresh.GetOwnedCount(ToyCatalog.AxolotlId),Is.Zero);
   } finally { Object.Destroy(root); }
   yield return null;
  }
 }
}
