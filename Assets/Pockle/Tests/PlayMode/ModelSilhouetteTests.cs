using System.Collections;
using System.Linq;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Pockle.Tests
{
    public sealed class ModelSilhouetteTests
    {
        [UnityTest]
        public IEnumerator EveryActiveBodyAndRareUseTheActualModelGeometry()
        {
            var root = new GameObject("Model silhouette test", typeof(Canvas));
            var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var obj = new GameObject("Outline", typeof(RectTransform), typeof(CharacterSilhouetteGraphic));
            obj.transform.SetParent(root.transform, false);
            var outline = obj.GetComponent<CharacterSilhouetteGraphic>();
            outline.rectTransform.sizeDelta = new Vector2(240, 240);
            outline.color = Color.black;
            try
            {
                foreach (var character in ToyCatalog.Characters)
                {
                    var collection = ToyCatalog.Collections.Single(c => c.Id == character.Id);
                    Assert.IsTrue(CharacterArt.TryLoad(collection.Members[0], out var asset), character.Id);
                    foreach (bool rare in new[] { false, true })
                    {
                        outline.Configure(character.Id, rare);
                        Canvas.ForceUpdateCanvases(); yield return null;
                        var rendered = outline.canvasRenderer.GetMesh();
                        Assert.IsNotNull(rendered, character.Id);
                        Assert.AreEqual(asset.BodyMesh.vertexCount, rendered.vertexCount, character.Id + " body vertex count");
                        CollectionAssert.AreEqual(asset.BodyMesh.triangles, rendered.triangles, character.Id + " body topology");
                        var body = asset.BodyMesh.vertices; var actual = rendered.vertices;
                        var bounds = asset.BodyMesh.bounds;
                        float scale = 216 / Mathf.Max(bounds.size.x, bounds.size.y);
                        for (int i = 0; i < body.Length; i++)
                        {
                            Assert.That(actual[i].x, Is.EqualTo((body[i].x - bounds.center.x) * scale).Within(.001f), character.Id);
                            Assert.That(actual[i].y, Is.EqualTo((body[i].y - bounds.center.y) * scale).Within(.001f), character.Id);
                        }
                        Assert.AreEqual(rare, outline.ConcealsSecret);
                        Assert.IsFalse(outline.raycastTarget);
                        Assert.IsFalse(obj.GetComponentsInChildren<RawImage>().Any(), "No finish, face or portrait may be revealed.");
                    }
                }
            }
            finally { Object.Destroy(root); }
        }
    }
}
