using System.Collections;
using System.Linq;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
    public sealed class LatestPipCollectionTests
    {
        [UnityTest]
        public IEnumerator LiveShelfAcceptsOwnedNewPipFinishWithoutSubstitutingPeach()
        {
            var root = new GameObject("New finish shelf routing",typeof(RectTransform));
            var picture = new GameObject("Pip picture",typeof(RectTransform),typeof(UnityEngine.UI.RawImage));
            var outline = new GameObject("Pip outline",typeof(RectTransform),typeof(CharacterSilhouetteGraphic));
            picture.transform.SetParent(root.transform,false); outline.transform.SetParent(root.transform,false);
            try
            {
                var runtime=root.AddComponent<ShelfPreviewRuntime>(); runtime.Initialize(root.GetComponent<RectTransform>());
                var binding=runtime.Register(picture.GetComponent<UnityEngine.UI.RawImage>(),outline.GetComponent<CharacterSilhouetteGraphic>(),"pip.glow-jelly");
                runtime.SetFound(binding,true);
                Assert.That(picture.activeSelf,Is.True,"An owned available new finish must show its actual art.");
                Assert.That(outline.activeSelf,Is.False);
                runtime.SetFound(binding,false);
                Assert.That(picture.activeSelf,Is.False); Assert.That(outline.activeSelf,Is.True);
            }
            finally { Object.Destroy(root); }
            yield return null;
        }
        [UnityTest]
        public IEnumerator UndiscoveredPipSilhouetteProjectsTheLatestApprovedModel()
        {
            var root = new GameObject("Approved Pip silhouette",typeof(Canvas));
            var child = new GameObject("Pip silhouette",typeof(RectTransform),typeof(CanvasRenderer));
            child.transform.SetParent(root.transform,false);
            Mesh mesh = null;
            try
            {
                root.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
                var graphic=child.AddComponent<CharacterSilhouetteGraphic>(); graphic.Configure("pip");
                yield return null; Canvas.ForceUpdateCanvases();
                mesh = child.GetComponent<CanvasRenderer>().GetMesh();
                Assert.That(mesh,Is.Not.Null);
                Assert.That(mesh.vertexCount,Is.EqualTo(Resources.Load<PipCharacterAsset>(PipCharacterAsset.MeshyResourcePath).BodyMesh.vertexCount));
                Assert.That(mesh.triangles.Length,Is.EqualTo(Resources.Load<PipCharacterAsset>(PipCharacterAsset.MeshyResourcePath).BodyMesh.triangles.Length));
            }
            finally { Object.Destroy(root); Object.Destroy(mesh); }
            yield return null;
        }
        [UnityTest]
        public IEnumerator TenFinishesUseTheSameApprovedMeshFaceAndDistinctResettableMaterials()
        {
            bool had = PlayerPrefs.HasKey(PipCharacterAsset.MeshyPreference);
            int pref = PlayerPrefs.GetInt(PipCharacterAsset.MeshyPreference);
            PlayerPrefs.SetInt(PipCharacterAsset.MeshyPreference,1);
            var root = new GameObject("Latest Pip collection regression");
            try
            {
                var source = Resources.Load<PipCharacterAsset>(PipCharacterAsset.MeshyResourcePath);
                var original = source.BodyMesh.vertices;
                var toy = root.AddComponent<JellyToy>();
                var members = ToyCatalog.Collectibles.Where(c => c.CollectionId == "pip").ToArray();
                Assert.That(members.Length,Is.EqualTo(10));
                foreach (var item in members.Concat(members.Reverse()))
                {
                    Assert.That(item.Available,Is.True);
                    Assert.That(CharacterArt.TryLoad(item.Id,out var asset),Is.True);
                    Assert.That(asset,Is.SameAs(source));
                    Assert.That(toy.TrySetCollectible(item.Id),Is.True);
                    Assert.That(toy.CollectibleId,Is.EqualTo(item.Id));
                    Assert.That(toy.UsesMeshyPeach,Is.True);
                    toy.ResetToy();
                    CollectionAssert.AreEqual(original,toy.BodyRenderer.GetComponent<MeshFilter>().sharedMesh.vertices);
                    var material=toy.BodyRenderer.sharedMaterial;
                    Assert.That(material.mainTexture,Is.SameAs(source.BaseColorTexture));
                    Assert.That(material.shader.isSupported,Is.True);
                    Assert.That(toy.VisualRoot.GetComponent<PeachExpressionDriver>(),Is.Not.Null);
                    Assert.That(material.GetFloat("_TintStrength"),Is.EqualTo(item.Id==ToyCatalog.PeachId ? 0 : 1));
                    Assert.That(material.GetFloat("_GlitterStrength"),Is.EqualTo(item.Id==ToyCatalog.GoldId ? .9f : 0));
                    Assert.That(material.GetFloat("_PearlSheen"),Is.EqualTo(item.Id==ToyCatalog.MoonId ? .72f : 0));
                    bool opaque=item.Id==ToyCatalog.MintId || item.FinishId=="velvet-flock" || item.FinishId=="boucle-plush" || item.FinishId=="matte-vinyl" || item.FinishId=="gloss-vinyl" || item.FinishId=="coated-metallic";
                    Assert.That(material.GetFloat("_ZWrite"),Is.EqualTo(opaque ? 1 : 0));
                    Assert.That(material.GetFloat("_Opacity"),opaque ? Is.EqualTo(1) : Is.LessThan(1));
                    if(item.Id==ToyCatalog.MoonId)
                    {
                        var stars=toy.VisualRoot.Cast<Transform>().Where(t=>t.name.StartsWith("Suspended silver star")).ToArray();
                        Assert.That(stars.Length,Is.EqualTo(8));
                        Assert.That(stars.All(t=>t.GetComponent<MeshFilter>().sharedMesh != null),Is.True,"Moon stars must have actual geometry.");
                    }
                    toy.SetDeformation(.18f,.2f,.1f,-.1f,0,0,.15f,Vector3.up,.12f);
                    foreach(var v in toy.BodyRenderer.GetComponent<MeshFilter>().sharedMesh.vertices) Assert.That(float.IsNaN(v.x)||float.IsInfinity(v.y),Is.False);
                    if(toy.Handling==MaterialHandling.Firm) CollectionAssert.AreEqual(original,toy.BodyRenderer.GetComponent<MeshFilter>().sharedMesh.vertices);
                    yield return null;
                }
                CollectionAssert.AreEqual(original,source.BodyMesh.vertices,"Variant changes edited the approved source asset.");
                toy.TrySetCollectible("pip.glow-jelly"); toy.SetVariant(PipVariant.PeachJelly);
                Assert.That(toy.CollectibleId,Is.EqualTo(ToyCatalog.PeachId));
                Assert.That(toy.BodyRenderer.sharedMaterial.GetFloat("_Glow"),Is.Zero);
                Assert.That(toy.BodyRenderer.sharedMaterial.GetFloat("_TintStrength"),Is.Zero);
                Assert.That(toy.TrySetCollectible(ToyCatalog.FoxAuroraId),Is.True);
                Assert.That(toy.TrySetCollectible(ToyCatalog.MoonId),Is.True);
                Assert.That(toy.UsesMeshyPeach,Is.True);
            }
            finally { Object.Destroy(root); if(had)PlayerPrefs.SetInt(PipCharacterAsset.MeshyPreference,pref);else PlayerPrefs.DeleteKey(PipCharacterAsset.MeshyPreference); }
            yield return null;
        }
    }
}
