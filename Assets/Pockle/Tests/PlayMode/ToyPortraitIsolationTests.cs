using System.Reflection;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;

namespace Pockle.Tests
{
    public sealed class ToyPortraitIsolationTests
    {
        [TestCase(ToyCatalog.FoxId)]
        [TestCase("axolotl.clear-jelly")]
        [TestCase("firb.matte-vinyl")]
        public void StillPortraitDoesNotPhotographTheLiveShelfActor(string id)
        {
            // Matches ShelfPreviewRuntime's first actor, which can remain active
            // until LateUpdate while a new screen requests its still portraits.
            var live = new GameObject("Live shelf portrait collision fixture");
            live.transform.position = new Vector3(1000, 0, 0);
            var toy = live.AddComponent<JellyToy>();
            Assert.That(toy.TrySetCollectible(ToyCatalog.PeachId), Is.True);
            foreach (Transform child in live.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = 30;
            RenderTexture baseline = null, withLive = null;
            try
            {
                live.SetActive(false);
                baseline = Render(id);
                var expected = Pixels(baseline);
                live.SetActive(true);
                withLive = Render(id);
                var actual = Pixels(withLive);
                long difference = 0;
                for (int i = 0; i < expected.Length; i++)
                {
                    difference += Mathf.Abs(expected[i].r - actual[i].r);
                    difference += Mathf.Abs(expected[i].g - actual[i].g);
                    difference += Mathf.Abs(expected[i].b - actual[i].b);
                }
                Assert.That(difference / (double)(expected.Length * 3), Is.LessThan(.1),
                    "An active live shelf must not alter the requested toy's still render.");
                Assert.That(live.activeInHierarchy, Is.True, "Still rendering must not hide the live shelf.");
                Assert.That(toy.BodyRenderer.enabled, Is.True);
            }
            finally
            {
                Release(baseline); Release(withLive);
                live.SetActive(false); Object.Destroy(live);
            }
        }

        private static RenderTexture Render(string id)
        {
            var type = typeof(JellyToy).Assembly.GetType("Pockle.Runtime.ToyPortrait");
            var method = type.GetMethod("Render", BindingFlags.Public | BindingFlags.Static,
                null, new[] { typeof(string), typeof(Color) }, null);
            return (RenderTexture)method.Invoke(null, new object[] { id, Color.black });
        }

        private static Color32[] Pixels(RenderTexture source)
        {
            var previous = RenderTexture.active;
            var copy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            try
            {
                RenderTexture.active = source;
                copy.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0, false);
                return copy.GetPixels32();
            }
            finally { RenderTexture.active = previous; Object.Destroy(copy); }
        }

        private static void Release(RenderTexture texture)
        { if (texture != null) { texture.Release(); Object.Destroy(texture); } }
    }
}
