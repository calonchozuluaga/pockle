using System.Collections;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace Pockle.Tests
{
    public sealed class FoxCollectionTests
    {
        [UnityTest]
        public IEnumerator UploadedFoxLoadsWithPaintAndFirmHandlingWithoutPipFace()
        {
            Assert.That(CharacterArt.TryLoad(ToyCatalog.FoxId, out var asset), Is.True);
            Assert.That(asset.CharacterId, Is.EqualTo("fox"));
            Assert.That(asset.BakedSurface, Is.True);
            Assert.That(asset.BaseColorTexture, Is.Not.Null);
            Assert.That(asset.BodyMesh.isReadable, Is.True);
            Assert.That(asset.BodyMesh.triangles.Length / 3, Is.EqualTo(30000));
            var root = new GameObject("Fox collection test");
            try
            {
                var toy = root.AddComponent<JellyToy>();
                Assert.That(toy.TrySetCollectible(ToyCatalog.FoxId), Is.True);
                Assert.That(toy.CollectibleId, Is.EqualTo(ToyCatalog.FoxId));
                Assert.That(toy.Handling, Is.SameAs(MaterialHandling.Firm));
                Assert.That(toy.UsesMeshyPeach, Is.False);
                Assert.That(toy.VisualRoot.GetComponentInChildren<MeshFilter>().sharedMesh.vertexCount, Is.EqualTo(asset.BodyMesh.vertexCount));
                var visual = toy.VisualRoot;
                Assert.That(visual.childCount, Is.EqualTo(1), "Baked eyes/ears must not gain Pip's procedural features.");
                var material = visual.GetComponentInChildren<MeshRenderer>().sharedMaterial;
                Assert.That(material.shader.name, Is.EqualTo("Standard"));
                Assert.That(material.mainTexture, Is.SameAs(asset.BaseColorTexture));
                var resting = toy.VisualRoot.GetComponentInChildren<MeshFilter>().sharedMesh.vertices;
                toy.SetDeformation(.25f, .3f, .5f, .5f, 0, 0, .4f, Vector3.up, .3f);
                CollectionAssert.AreEqual(resting, toy.VisualRoot.GetComponentInChildren<MeshFilter>().sharedMesh.vertices, "Vinyl Fox must remain rigid.");
                foreach (var vertex in toy.VisualRoot.GetComponentInChildren<MeshFilter>().sharedMesh.vertices)
                    Assert.That(float.IsNaN(vertex.x) || float.IsInfinity(vertex.y), Is.False);
                Assert.That(toy.TrySetCollectible("fox.clear-jelly"), Is.True);
                Assert.That(toy.BodyRenderer.sharedMaterial.shader.name, Is.EqualTo("Pockle/Fox Finish"));
                Assert.That(toy.TrySetCollectible(ToyCatalog.PeachId), Is.True);
                Assert.That(toy.TrySetCollectible(ToyCatalog.FoxId), Is.True);
            }
            finally { Object.Destroy(root); }
            yield return null;
        }
    }
}
