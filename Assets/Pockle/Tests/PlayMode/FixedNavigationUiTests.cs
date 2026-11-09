#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Pockle.Tests
{
    public sealed class FixedNavigationUiTests
    {
        private GameObject root;
        private PrototypeHud hud;
        private readonly Dictionary<string, string> strings = new Dictionary<string, string>();
        private readonly Dictionary<string, int> ints = new Dictionary<string, int>();
        private readonly List<string> keys = new List<string>();
        private readonly string[] intKeys = { "pockle.profile.local.avatar", "pockle.profile.local.favorite", StageLook.WorldKey, StageLook.LightKey };

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            keys.Clear(); strings.Clear(); ints.Clear();
            keys.AddRange(new[] { "pockle.collection.v1", "pockle.collection.v1.backup", "pockle.profile.local.name",
                "pockle.profile.local.avatarId", "pockle.profile.local.favoriteId", PrototypeHud.MomentsKey,
                PrototypeHud.LiveShelfKey, PrototypeHud.LiveShelfSeenKey, LocalActivityFeed.LogKey, ShelfFriendships.Key, BadgeCabinet.Key });
            for (int i = 2; i <= CollectionProgress.SaveVersion; i++) keys.Add("pockle.collection.v1.before-v" + i);
            keys.AddRange(ToyCatalog.Collections.Select(c => "pockle.stage.environment.v1." + c.Id));
            foreach (string key in keys) { if (PlayerPrefs.HasKey(key)) strings[key] = PlayerPrefs.GetString(key); PlayerPrefs.DeleteKey(key); }
            foreach (string key in intKeys) { if (PlayerPrefs.HasKey(key)) ints[key] = PlayerPrefs.GetInt(key); PlayerPrefs.DeleteKey(key); }
            ShelfFriendships.Reload();
            root = new GameObject("Fixed navigation UI test");
            var session = root.AddComponent<CollectionSession>(); session.Initialize();
            hud = root.AddComponent<PrototypeHud>(); hud.Initialize(_ => { }, _ => { }, _ => { }); hud.Bind(session);
            hud.SetSettings(true, false, true);
            yield return null;
            Canvas.ForceUpdateCanvases();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            if (root != null) Object.Destroy(root);
            yield return null;
            foreach (string key in keys) { if (strings.TryGetValue(key, out string value)) PlayerPrefs.SetString(key, value); else PlayerPrefs.DeleteKey(key); }
            foreach (string key in intKeys) { if (ints.TryGetValue(key, out int value)) PlayerPrefs.SetInt(key, value); else PlayerPrefs.DeleteKey(key); }
            PlayerPrefs.Save(); ShelfFriendships.Reload(); FusionCatalog.Restore(null);
        }

        [UnityTest]
        public IEnumerator FiveEqualTabsRouteToApprovedDestinationsAndUseMatchingIconStates()
        {
            var buttons = RectNamed("Navigation").GetComponentsInChildren<Button>();
            CollectionAssert.AreEqual(new[] { "Home", "Shelf", "Walk", "Community", "Shop" }, buttons.Select(b => b.name).ToArray());
            var pages = new[] { AppPage.Home, AppPage.Shelf, AppPage.Rewards, AppPage.Social, AppPage.Boxes };
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].onClick.Invoke();
                Assert.AreEqual(pages[i], hud.CurrentPage);
                for (int j = 0; j < buttons.Length; j++)
                {
                    var selection = buttons[j].transform.Find("Selected icon").GetComponent<Image>();
                    var glyph = selection.GetComponentInChildren<RawImage>();
                    Assert.NotNull(glyph.texture, buttons[j].name + " must have its shipped icon asset.");
                    Assert.AreEqual(i == j, glyph.texture.name.EndsWith("-fill"));
                    Assert.AreEqual(i == j, selection.color.a > .5f);
                    Assert.AreEqual(0, buttons[j].GetComponent<Image>().color.a, "Selection must not fill the whole tab target.");
                }
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator AvatarOpensProfileAndBackReturnsToItsOriginWithoutSelectingAnotherTab()
        {
            NavButton("Community").onClick.Invoke();
            var avatar = root.GetComponentsInChildren<Button>().Single(b => b.name == "Your profile");
            avatar.onClick.Invoke();
            Assert.AreEqual(AppPage.Profile, hud.CurrentPage);
            foreach (var button in RectNamed("Navigation").GetComponentsInChildren<Button>())
                Assert.AreEqual(0, button.transform.Find("Selected icon").GetComponent<Image>().color.a);
            Assert.IsTrue(hud.GoBack());
            Assert.AreEqual(AppPage.Social, hud.CurrentPage);
            Assert.IsTrue(hud.GoBack());
            Assert.AreEqual(AppPage.Home, hud.CurrentPage);
            yield return null;
        }

        [UnityTest]
        public IEnumerator FullWidthBackgroundCoversBottomInsetWhileTargetsStaySafeAndContentStaysAbove()
        {
            hud.enabled = false; // Keep the synthetic safe-area fixture stable; this is not a device claim.
            var safe = RectNamed("Safe area");
            var canvasRect = root.GetComponentInChildren<Canvas>().transform as RectTransform;
            foreach (var fixture in new[] { new Vector4(0, 0, 1, 1), new Vector4(.08f, .045f, .94f, .95f) })
            {
                safe.anchorMin = new Vector2(fixture.x, fixture.y); safe.anchorMax = new Vector2(fixture.z, fixture.w);
                safe.offsetMin = safe.offsetMax = Vector2.zero;
                Canvas.ForceUpdateCanvases();
                Invoke("AdaptLayout"); Canvas.ForceUpdateCanvases();
                var bar = RectNamed("Navigation");
                Assert.AreSame(canvasRect, bar.parent, "Bar must span the canvas independently of the inset safe root.");
                var barBounds = Bounds(bar); var canvasBounds = Bounds(canvasRect); var safeBounds = Bounds(safe);
                Assert.That(barBounds.xMin, Is.EqualTo(canvasBounds.xMin).Within(.1f));
                Assert.That(barBounds.xMax, Is.EqualTo(canvasBounds.xMax).Within(.1f));
                Assert.That(barBounds.yMin, Is.EqualTo(canvasBounds.yMin).Within(.1f));
                Assert.IsNull(bar.GetComponent<Image>().sprite, "A flat rectangle must not reuse the old rounded pill sprite.");
                Assert.That(bar.GetComponent<Image>().color.a, Is.EqualTo(1).Within(.001f));
                var buttons = bar.GetComponentsInChildren<Button>(); float width = Bounds(buttons[0].GetComponent<RectTransform>()).width;
                foreach (var button in buttons)
                {
                    var bounds = Bounds(button.GetComponent<RectTransform>());
                    Assert.That(bounds.width, Is.EqualTo(width).Within(.1f));
                    Assert.That(bounds.xMin, Is.GreaterThanOrEqualTo(safeBounds.xMin - .1f));
                    Assert.That(bounds.xMax, Is.LessThanOrEqualTo(safeBounds.xMax + .1f));
                    Assert.That(bounds.yMin, Is.GreaterThanOrEqualTo(safeBounds.yMin));
                    Assert.That(button.GetComponent<RectTransform>().rect.height, Is.GreaterThanOrEqualTo(44));
                }
                var viewport = Bounds(RectNamed("Collection browsing"));
                Assert.That(viewport.yMin, Is.EqualTo(barBounds.yMax).Within(.1f), "Scroll viewport must end above the bar, not behind it.");
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator FocusedToyPlayHidesBarAndBackRestoresItsShelfRoute()
        {
            NavButton("Shelf").onClick.Invoke();
            Assert.IsTrue(hud.ShowToy(ToyCatalog.PeachId));
            Assert.AreEqual(AppPage.Play, hud.CurrentPage);
            Assert.IsFalse(RectNamed("Navigation").gameObject.activeSelf);
            Assert.IsTrue(hud.GoBack());
            Assert.AreEqual(AppPage.Shelf, hud.CurrentPage);
            Assert.IsTrue(RectNamed("Navigation").gameObject.activeSelf);
            yield return null;
        }

        private Button NavButton(string name) => RectNamed("Navigation").GetComponentsInChildren<Button>().Single(b => b.name == name);
        private RectTransform RectNamed(string name) => root.GetComponentsInChildren<RectTransform>(true).Single(r => r.name == name);
        private void Invoke(string name) => typeof(PrototypeHud).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(hud, null);
        private static Rect Bounds(RectTransform rect)
        {
            var corners = new Vector3[4]; rect.GetWorldCorners(corners);
            return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
        }
    }
}
#endif
