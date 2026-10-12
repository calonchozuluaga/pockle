#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using NUnit.Framework;
using Pockle.Runtime;
using Pockle.Core;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Pockle.Tests
{
    public sealed class CollectionUiTests
    {
        private const string SaveKey = "pockle.collection.v1";
        private GameObject root;
        private PrototypeHud hud;
        private CollectionSession session;
        private string oldSave, oldBackup;
        private bool hadSave, hadBackup;
        // String keys hold names/IDs; the legacy avatar/favorite keys are ints. All are restored after each test.
        private readonly string[] profileKeys = { "pockle.profile.local.name", "pockle.profile.local.avatar", "pockle.profile.local.favorite",
            "pockle.profile.local.avatarId", "pockle.profile.local.favoriteId" };
        private static bool IsIntKey(int index) => index == 1 || index == 2;
        private readonly string[] previousProfile = new string[5];
        private readonly bool[] hadProfile = new bool[5];
        private bool hadVolume;
        private float previousVolume, observedVolume;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            hadSave = PlayerPrefs.HasKey(SaveKey); oldSave = PlayerPrefs.GetString(SaveKey);
            hadBackup = PlayerPrefs.HasKey(SaveKey + ".backup"); oldBackup = PlayerPrefs.GetString(SaveKey + ".backup");
            PlayerPrefs.DeleteKey(SaveKey);
            for (int i = 0; i < profileKeys.Length; i++)
            {
                hadProfile[i] = PlayerPrefs.HasKey(profileKeys[i]);
                previousProfile[i] = IsIntKey(i) ? PlayerPrefs.GetInt(profileKeys[i]).ToString() : PlayerPrefs.GetString(profileKeys[i]);
                PlayerPrefs.DeleteKey(profileKeys[i]);
            }
            hadVolume = PlayerPrefs.HasKey("pockle.prototype.soundVolume"); previousVolume = PlayerPrefs.GetFloat("pockle.prototype.soundVolume");
            PlayerPrefs.DeleteKey("pockle.prototype.soundVolume");
            root = new GameObject("Collection UI test");
            session = root.AddComponent<CollectionSession>(); session.Initialize();
            hud = root.AddComponent<PrototypeHud>();
            hud.Initialize(_ => { }, _ => { }, _ => { }, value => observedVolume = value); hud.Bind(session);
            hud.SetSettings(true, false, false);
            yield return null;
            Canvas.ForceUpdateCanvases();
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Object.Destroy(root); yield return null; // Session persists before we restore the original save.
            if (hadSave) PlayerPrefs.SetString(SaveKey, oldSave); else PlayerPrefs.DeleteKey(SaveKey);
            if (hadBackup) PlayerPrefs.SetString(SaveKey + ".backup", oldBackup); else PlayerPrefs.DeleteKey(SaveKey + ".backup");
            for (int i = 0; i < profileKeys.Length; i++)
            {
                if (!hadProfile[i]) PlayerPrefs.DeleteKey(profileKeys[i]);
                else if (IsIntKey(i)) PlayerPrefs.SetInt(profileKeys[i], int.Parse(previousProfile[i]));
                else PlayerPrefs.SetString(profileKeys[i], previousProfile[i]);
            }
            if (hadVolume) PlayerPrefs.SetFloat("pockle.prototype.soundVolume", previousVolume); else PlayerPrefs.DeleteKey("pockle.prototype.soundVolume");
            PlayerPrefs.Save();
        }

        [UnityTest]
        public IEnumerator ShelfAndLineupRenderForReview()
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
                Assert.Ignore("Shelf review captures require a graphics device.");
            int oldWidth = Screen.width, oldHeight = Screen.height;
            FullScreenMode oldMode = Screen.fullScreenMode;
            Screen.SetResolution(720, 1280, FullScreenMode.Windowed);
            yield return null; yield return null; yield return null;
            hud.ShelfPreviews.SendMessage("OnApplicationFocus", true);
            var canvas = root.GetComponentInChildren<Canvas>();
            var scaler = canvas.GetComponent<CanvasScaler>();
            var oldScaleMode = scaler.uiScaleMode; float oldScaleFactor = scaler.scaleFactor;
            // Editor Game View may ignore SetResolution. The camera target and
            // logical portrait scale still exercise the real responsive HUD.
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize; scaler.scaleFactor = 1280f / 844f;
            var cameraObject = new GameObject("Shelf UI review camera", typeof(Camera));
            var camera = cameraObject.GetComponent<Camera>(); camera.enabled = false;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = PrototypeStage.Background;
            camera.cullingMask = 1 << 29; camera.orthographic = true; camera.orthographicSize = 5;
            camera.nearClipPlane = .1f; camera.farClipPlane = 10;
            var target = new RenderTexture(720, 1280, 24, RenderTextureFormat.ARGB32); target.Create();
            camera.targetTexture = target;
            var layers = canvas.GetComponentsInChildren<Transform>(true).ToDictionary(child => child, child => child.gameObject.layer);
            foreach (var child in layers.Keys) child.gameObject.layer = 29;
            canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
            try
            {
                ButtonWithText("Shelf").onClick.Invoke();
                var shelfScroll = root.GetComponentsInChildren<ScrollRect>().Single();
                shelfScroll.content.anchoredPosition = new Vector2(0, 186);
                for (int frame = 0; frame < 8; frame++)
                { yield return new WaitForSecondsRealtime(.3f); CaptureShelfUi(canvas, camera, target, "shelf-idle-" + frame); }
                ButtonWithText("Browse collections").onClick.Invoke(); ButtonNamed("Collection · pip").onClick.Invoke();
                yield return new WaitForSecondsRealtime(.2f); CaptureShelfUi(canvas, camera, target, "pip-lineup");
                var scroll = root.GetComponentsInChildren<ScrollRect>().Single();
                FocusFirstSilhouette(scroll);
                for (int frame = 0; frame < 8; frame++)
                { yield return new WaitForSecondsRealtime(.3f); CaptureShelfUi(canvas, camera, target, "silhouettes-eager-" + frame); }
                Assert.That(hud.ShelfPreviews.ActiveSilhouetteCount, Is.GreaterThan(0), "Visible found toys must leave motion slots for the missing silhouettes.");
                hud.SetSettings(true, false, true);
                yield return null; CaptureShelfUi(canvas, camera, target, "motion-calm");
                hud.GoBack(); ButtonNamed("Collection · loop").onClick.Invoke();
                yield return null; FocusFirstSilhouette(scroll); yield return null; CaptureShelfUi(canvas, camera, target, "planned-loop");
                Assert.That(hud.ShelfPreviews.AllocatedTargetCount, Is.LessThanOrEqualTo(ShelfPreviewRuntime.Capacity));
                Debug.Log("SHELF_REVIEW_CAPTURES " + SystemInfo.graphicsDeviceName);
            }
            finally
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.worldCamera = null;
                foreach (var pair in layers) if (pair.Key != null) pair.Key.gameObject.layer = pair.Value;
                camera.targetTexture = null; target.Release(); Object.Destroy(target); Object.Destroy(cameraObject);
                scaler.uiScaleMode = oldScaleMode; scaler.scaleFactor = oldScaleFactor;
                Screen.SetResolution(oldWidth, oldHeight, oldMode);
            }
        }

        private static void FocusFirstSilhouette(ScrollRect scroll)
        {
            Canvas.ForceUpdateCanvases();
            var outline = scroll.content.GetComponentsInChildren<CharacterSilhouetteGraphic>().First();
            var bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(scroll.content, outline.rectTransform);
            scroll.content.anchoredPosition = new Vector2(0, Mathf.Clamp(-bounds.center.y - scroll.viewport.rect.height * .5f,
                0, Mathf.Max(0, scroll.content.rect.height - scroll.viewport.rect.height)));
        }

        private static void CaptureShelfUi(Canvas canvas, Camera camera, RenderTexture target, string name)
        {
            foreach (Transform child in canvas.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = 29;
            Canvas.ForceUpdateCanvases(); camera.Render();
            var previous = RenderTexture.active;
            var image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            try
            {
                RenderTexture.active = target; image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); image.Apply();
                string directory = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.dataPath), "test-results", "shelf-motion");
                System.IO.Directory.CreateDirectory(directory); System.IO.File.WriteAllBytes(System.IO.Path.Combine(directory, name + ".png"), image.EncodeToPNG());
                const int width = 360; int height = Mathf.RoundToInt((float)target.height / target.width * width);
                var pixels = image.GetPixels32(); var rgb = new byte[width * height * 3];
                for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
                {
                    int sx = x * target.width / width, sy = target.height - 1 - y * target.height / height;
                    Color32 pixel = pixels[sy * target.width + sx]; int index = (y * width + x) * 3;
                    rgb[index] = pixel.r; rgb[index + 1] = pixel.g; rgb[index + 2] = pixel.b;
                }
                using (var output = new System.IO.MemoryStream())
                {
                    using (var gzip = new System.IO.Compression.GZipStream(output, System.IO.Compression.CompressionLevel.Fastest, true)) gzip.Write(rgb, 0, rgb.Length);
                    Debug.Log("SHELF_REVIEW_PREVIEW " + name + " " + width + " " + height + " " + System.Convert.ToBase64String(output.ToArray()));
                }
                // The screen centre can be a grid gutter. Sample the full UI rather than
                // rejecting a valid capture because two background pixels match.
                var background = pixels[0]; bool varied = false;
                for (int i = 0; i < pixels.Length; i += Mathf.Max(1, pixels.Length / 1024))
                    if (Mathf.Abs(pixels[i].r - background.r) + Mathf.Abs(pixels[i].g - background.g) + Mathf.Abs(pixels[i].b - background.b) > 30)
                    { varied = true; break; }
                Assert.IsTrue(varied, "UI review capture is empty.");
            }
            finally { RenderTexture.active = previous; Object.Destroy(image); }
        }

        [UnityTest]
        public IEnumerator AnimatedShelfUsesRealMeshesAndStopsForCalmNavigationAndPause()
        {
            ButtonWithText("Shelf").onClick.Invoke();
            yield return null; yield return null; yield return null;
            var previews = hud.ShelfPreviews;
            Assert.That(previews.ActiveCount, Is.InRange(1, ShelfPreviewRuntime.Capacity));
            Assert.That(previews.CameraCount, Is.EqualTo(1));
            Assert.That(previews.AllocatedTargetCount, Is.InRange(1, ShelfPreviewRuntime.Capacity));
            var animator = root.GetComponentsInChildren<ToyLineupAnimator>().First(actor => actor.IsAnimating);
            var mesh = animator.GetComponent<JellyToy>().BodyRenderer.GetComponent<MeshFilter>().sharedMesh;
            var before = mesh.vertices;
            yield return new WaitForSecondsRealtime(.3f);
            var after = mesh.vertices;
            Assert.IsTrue(before.Where((vertex, index) => Vector3.SqrMagnitude(vertex - after[index]) > 1e-10f).Any(),
                "Found toys must deform their actual mesh, not just move a still portrait.");
            int frames = previews.RenderedFrameCount;
            hud.SetSettings(true, false, true);
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(previews.ActiveCount, Is.Zero);
            Assert.That(previews.RenderedFrameCount, Is.EqualTo(frames), "Motion calm must stop live rendering.");
            Assert.IsFalse(root.GetComponentsInChildren<ToyLineupAnimator>().Any(actor => actor.IsAnimating));
            hud.SetSettings(true, false, false);
            yield return null; yield return null;
            Assert.That(previews.ActiveCount, Is.GreaterThan(0));
            previews.SendMessage("OnApplicationPause", true);
            Assert.That(previews.ActiveCount, Is.Zero);
            previews.SendMessage("OnApplicationPause", false);
            yield return null; yield return null;
            Assert.That(previews.ActiveCount, Is.GreaterThan(0));
            ButtonWithText("Boxes").onClick.Invoke();
            yield return null;
            Assert.That(previews.ActiveCount, Is.Zero, "Leaving the shelf must release every actor.");
            ButtonWithText("Shelf").onClick.Invoke();
            yield return null; yield return null;
            hud.enabled = false;
            Assert.That(previews.ActiveCount, Is.Zero, "Disabling the HUD must stop its preview component too.");
            hud.enabled = true;
            yield return new WaitForSecondsRealtime(.15f);
            Assert.That(previews.ActiveCount, Is.GreaterThan(0));
            var targets = root.GetComponentsInChildren<RawImage>().Select(image => image.texture).OfType<RenderTexture>().ToArray();
            previews.Dispose();
            Assert.That(previews.ActiveCount, Is.Zero);
            Assert.That(previews.AllocatedTargetCount, Is.Zero);
            Assert.That(previews.CameraCount, Is.Zero);
            Assert.IsTrue(targets.Any(target => !target.IsCreated()), "Disposal must release native live targets.");
        }

        [UnityTest]
        public IEnumerator MissingLineupHasEagerSilhouettesWithoutFacesOrOwnershipChanges()
        {
            var inventory = session.OwnedCounts.ToDictionary(pair => pair.Key, pair => pair.Value);
            ButtonWithText("Browse collections").onClick.Invoke(); ButtonNamed("Collection · pip").onClick.Invoke();
            var missing = ButtonNamed("Collectible · pip.glow-jelly");
            Assert.IsFalse(missing.interactable);
            var outline = missing.GetComponentInChildren<CharacterSilhouetteGraphic>();
            Assert.That(outline.CharacterId, Is.EqualTo("pip"));
            Assert.IsFalse(missing.GetComponentsInChildren<RawImage>().Any(), "A missing toy must expose no rendered face or finish.");
            var scroll = root.GetComponentsInChildren<ScrollRect>().Single();
            Canvas.ForceUpdateCanvases();
            FocusFirstSilhouette(scroll);
            yield return null; yield return null;
            var silhouettes = root.GetComponentsInChildren<CharacterSilhouetteGraphic>();
            var positions = silhouettes.Select(graphic => graphic.rectTransform.anchoredPosition).ToArray();
            yield return new WaitForSecondsRealtime(.35f);
            Assert.That(hud.ShelfPreviews.ActiveCount, Is.InRange(1, ShelfPreviewRuntime.Capacity));
            Assert.IsTrue(silhouettes.Where((graphic, index) => Vector2.Distance(graphic.rectTransform.anchoredPosition, positions[index]) > .01f).Any(),
                "Visible missing silhouettes should play the eager greeting.");
            hud.SetSettings(true, false, true);
            var calmPositions = silhouettes.Select(graphic => graphic.rectTransform.anchoredPosition).ToArray();
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(hud.ShelfPreviews.ActiveCount, Is.Zero);
            Assert.IsTrue(silhouettes.Select((graphic, index) => graphic.rectTransform.anchoredPosition == calmPositions[index]).All(value => value));
            outline.Configure("sprig", secret: true);
            Assert.IsTrue(outline.ConcealsSecret, "Secret placeholders must not use the secret's character outline.");
            Assert.IsFalse(hud.ShowToy("pip.glow-jelly"));
            CollectionAssert.AreEquivalent(inventory, session.OwnedCounts);
        }

        [UnityTest]
        public IEnumerator ShelfSelectionAndUnavailableCheckoutDoNotGrantToys()
        {
            Assert.AreEqual(AppPage.Home, hud.CurrentPage);
            ButtonWithText("See all").onClick.Invoke();
            var portraits = root.GetComponentsInChildren<RawImage>().Where(image => image.texture is RenderTexture).ToArray();
            Assert.AreEqual(4, portraits.Length, "Shelf must show four separate rendered toys.");
            Assert.IsTrue(portraits.All(image => ((RenderTexture)image.texture).IsCreated()));
            PipVariant selected = PipVariant.PeachJelly;
            hud.VariantChanged += choice => selected = choice;
            ButtonWithText("Moon Jelly").onClick.Invoke();
            Assert.AreEqual(PipVariant.MoonJelly, selected);
            Assert.IsFalse(hud.StoreVisible, "Selected toy must be available for direct manipulation.");
            hud.GoBack();
            ButtonWithText("Boxes").onClick.Invoke();
            var before = (int[])session.Progress.Save.Counts.Clone();
            ButtonNamed("Buy · Jelly Garden").onClick.Invoke();
            yield return null;
            CollectionAssert.AreEqual(before, session.Progress.Save.Counts, "Unavailable checkout must never grant a toy.");
            Assert.IsTrue(root.GetComponentsInChildren<Text>().Any(text => text.text.Contains("Nothing will be charged")));
            hud.GoBack();
            Assert.IsTrue(hud.StoreVisible, "Closing purchase dialog must retain box browsing.");
        }

        [UnityTest]
        public IEnumerator DailyBoxUiUnlocksAndOnlyGrantsOnce()
        {
            ButtonWithText("Boxes").onClick.Invoke();
            Assert.IsFalse(ButtonWithText("Walk to unlock").interactable);
            // The Shelf tab is reachable from every tab page, and Back from it returns Home.
            ButtonWithText("Shelf").onClick.Invoke();
            Assert.AreEqual(AppPage.Shelf, hud.CurrentPage);
            Assert.IsTrue(hud.GoBack()); Assert.AreEqual(AppPage.Home, hud.CurrentPage);
            ButtonWithText("Boxes").onClick.Invoke();
            int grants = 0;
            hud.DailyBoxRequested += () => { if (session.ClaimDaily(out _)) grants++; };
            session.SimulateDailyWalk();
            yield return null;
            var open = ButtonWithText("Open your box");
            Assert.IsTrue(open.interactable);
            open.onClick.Invoke();
            yield return null;
            Assert.AreEqual(1, grants);
            Assert.AreEqual(5, session.Progress.Save.Counts.Sum());
            Assert.IsFalse(ButtonWithText("Opened today").interactable);
            open.onClick.Invoke(); // Even stale/replayed callbacks cannot duplicate ownership.
            Assert.AreEqual(1, grants);
        }

        [UnityTest]
        public IEnumerator PlayRoundTripRestoresShelfScrollAndSettingsKeepVolume()
        {
            ButtonWithText("See all").onClick.Invoke();
            yield return null; Canvas.ForceUpdateCanvases();
            var scroll = root.GetComponentInChildren<ScrollRect>();
            float position = Mathf.Max(0, scroll.content.rect.height - scroll.viewport.rect.height) * .5f;
            scroll.content.anchoredPosition = new Vector2(0, position);
            ButtonWithText("Moon Jelly").onClick.Invoke();
            Assert.AreEqual(AppPage.Play, hud.CurrentPage); Assert.IsFalse(hud.StoreVisible);
            hud.GoBack(); Assert.AreEqual(AppPage.Shelf, hud.CurrentPage);
            Assert.AreEqual(position, scroll.content.anchoredPosition.y, .1f, "Play round trip lost the shelf position.");
            hud.GoBack(); Assert.AreEqual(AppPage.Home, hud.CurrentPage);
            // Settings opens from the gear on You (v2).
            ButtonWithText("You").onClick.Invoke();
            ButtonWithText("Settings").onClick.Invoke();
            Assert.AreEqual(AppPage.Settings, hud.CurrentPage); Assert.IsTrue(hud.StoreVisible);
            var volume = root.GetComponentInChildren<Slider>(); volume.value = .42f;
            Assert.AreEqual(.42f, observedVolume, .001f); Assert.AreEqual(.42f, PlayerPrefs.GetFloat("pockle.prototype.soundVolume"), .001f);
            ButtonNamed("Sound switch").onClick.Invoke();
            Assert.IsFalse(volume.interactable); Assert.AreEqual(.42f, volume.value, .001f, "Muting erased the chosen volume.");
            Assert.IsTrue(VisibleText("Muted"));
            hud.GoBack(); Assert.AreEqual(AppPage.Profile, hud.CurrentPage);
            ButtonWithText("Home").onClick.Invoke(); Assert.AreEqual(AppPage.Home, hud.CurrentPage);
            Assert.IsFalse(hud.GoBack(), "Home Back must hand root exit to the platform.");
        }

        [UnityTest]
        public IEnumerator LocalProfilePersistsWithoutPublishingOrGrantingToys()
        {
            var inventory = (int[])session.Progress.Save.Counts.Clone();
            ButtonWithText("You").onClick.Invoke();
            var field = root.GetComponentInChildren<InputField>();
            field.text = "  Lucía 李  "; field.onEndEdit.Invoke(field.text);
            Assert.AreEqual("Lucía 李", PlayerPrefs.GetString("pockle.profile.local.name"));
            Assert.IsFalse(field.textComponent.supportRichText);
            ButtonNamed("Avatar · MOON JELLY").onClick.Invoke();
            Assert.AreEqual(1, PlayerPrefs.GetInt("pockle.profile.local.avatar"));
            // The favorite is set with the heart while playing a toy (v2).
            Assert.IsTrue(hud.ShowToy(PipVariants.CollectibleId(PipVariant.MoonJelly)));
            ButtonNamed("Favorite").onClick.Invoke();
            Assert.AreEqual(1, PlayerPrefs.GetInt("pockle.profile.local.favorite"));
            hud.GoBack(); hud.GoBack();
            ButtonWithText("Home").onClick.Invoke();
            Assert.IsTrue(Greets("Lucía 李"));
            ButtonWithText("Friends").onClick.Invoke();
            Assert.AreEqual(AppPage.Social, hud.CurrentPage);
            Assert.IsTrue(root.GetComponentsInChildren<Text>().Any(text => text.text.Contains("No public profiles are connected yet")));
            ButtonWithText("Your friends").onClick.Invoke();
            Assert.IsTrue(root.GetComponentsInChildren<Text>().Any(text => text.text.Contains("when accounts are ready")));
            CollectionAssert.AreEqual(inventory, session.Progress.Save.Counts);
            Object.Destroy(root); yield return null;
            root = new GameObject("Restarted local profile test");
            hud = root.AddComponent<PrototypeHud>(); hud.Initialize(_ => { }, _ => { }, _ => { });
            yield return null;
            Assert.IsTrue(Greets("Lucía 李"));
            ButtonWithText("You").onClick.Invoke();
            Assert.AreEqual("Lucía 李", root.GetComponentInChildren<InputField>().text);
        }

        [UnityTest]
        public IEnumerator HomeAndShelfReflectProgressAndKeepUndiscoveredToysASurprise()
        {
            // Restart with a saved ID inventory: Moon not yet discovered, three Mint duplicates.
            Object.Destroy(root); yield return null;
            var save = new CollectionSave { Version = CollectionProgress.SaveVersion, Day = CollectionSession.Today,
                Inventory = new System.Collections.Generic.List<OwnedCollectible> {
                    new OwnedCollectible(PipVariants.CollectibleId(PipVariant.PeachJelly), 1),
                    new OwnedCollectible(PipVariants.CollectibleId(PipVariant.GoldGlitter), 1),
                    new OwnedCollectible(PipVariants.CollectibleId(PipVariant.MintSoft), 3) } };
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(save));
            root = new GameObject("Partial collection UI test");
            session = root.AddComponent<CollectionSession>(); session.Initialize();
            Assert.AreEqual(0, session.GetOwnedCount(PipVariants.CollectibleId(PipVariant.MoonJelly)));
            hud = root.AddComponent<PrototypeHud>(); hud.Initialize(_ => { }, _ => { }, _ => { }); hud.Bind(session);
            session.SimulateDailyWalk(); // Ready box; also raises Changed so the HUD refreshes.
            yield return null;
            Assert.IsTrue(ButtonWithText("Open your box").interactable, "Home must offer the ready box as its main action.");
            Assert.IsTrue(VisibleText("Your box is ready!"), "Rewards tile must reflect today's ready box.");
            Assert.IsTrue(VisibleText("3 of 4 finishes · 5 toys on your shelf"));
            ButtonWithText("See all").onClick.Invoke();
            yield return null; Canvas.ForceUpdateCanvases();
            Assert.IsTrue(VisibleText("3 of 4 discovered"));
            Assert.IsTrue(VisibleText("???"), "An undiscovered toy must keep its name a surprise.");
            Assert.IsFalse(VisibleText("Moon Jelly"), "An undiscovered toy's name must not be shown.");
            var moon = root.GetComponentsInChildren<Button>().Single(button => button.name == "MOON JELLY · shelf toy");
            Assert.IsFalse(moon.interactable, "An undiscovered toy cannot be played.");
            Assert.IsTrue(VisibleText("×3"), "Duplicates show a count badge.");
            var portraits = root.GetComponentsInChildren<RawImage>().Where(image => image.texture is RenderTexture).ToArray();
            Assert.AreEqual(3, portraits.Length, "Only discovered toys show their rendered portrait.");
            ButtonWithText("Missing").onClick.Invoke();
            Assert.IsTrue(moon.gameObject.activeInHierarchy, "Missing shows the undiscovered toy.");
            Assert.AreEqual(7, root.GetComponentsInChildren<CharacterSilhouetteGraphic>().Length,
                "Missing includes Moon and six planned Pip catalog slots, without granting them.");
            Assert.IsFalse(ButtonNamed("pip.glow-jelly · shelf toy").interactable);
            Assert.IsFalse(ButtonNamed("PEACH JELLY · shelf toy", true).gameObject.activeInHierarchy, "Missing hides owned toys.");
            ButtonWithText("Duplicates").onClick.Invoke();
            Assert.IsTrue(ButtonNamed("MINT SOFT · shelf toy", true).gameObject.activeInHierarchy, "Duplicates shows the toy you have three of.");
            Assert.IsFalse(moon.gameObject.activeInHierarchy);
            ButtonWithText("Everything").onClick.Invoke();
            Assert.IsTrue(moon.gameObject.activeInHierarchy);
            Assert.IsFalse(hud.ShowToy(PipVariants.CollectibleId(PipVariant.MoonJelly)), "An unowned ID must not open play.");
            Assert.IsFalse(hud.ShowToy("moss.velvet-flock"), "A planned collectible without art must not open play.");
            Assert.AreEqual(AppPage.Shelf, hud.CurrentPage);
            Assert.IsTrue(hud.ShowToy(PipVariants.CollectibleId(PipVariant.MintSoft)), "An owned, available ID opens play.");
            Assert.AreEqual(AppPage.Play, hud.CurrentPage);
        }

        [UnityTest]
        public IEnumerator CollectionsKeepLineupContextAndUseTheDisabledViewerBackdrop()
        {
            var before = session.OwnedCounts.ToDictionary(pair => pair.Key, pair => pair.Value);
            var oldAmbient = RenderSettings.ambientLight;
            var oldAmbientMode = RenderSettings.ambientMode;
            bool oldFog = RenderSettings.fog;
            PrototypeStage.Create(out Camera viewer, out _, out _, out _, out _);
            var stageRoot = viewer.transform.parent.gameObject;
            viewer.enabled = false; // Runtime hides this camera while menus are open.
            System.Action<Color> backdrop = colour => PrototypeStage.SetBackdrop(viewer, colour);
            hud.BackdropChanged += backdrop;
            try
            {
                ButtonWithText("Browse collections").onClick.Invoke();
                Assert.AreEqual(AppPage.Collections, hud.CurrentPage);
                Assert.AreEqual(12, root.GetComponentsInChildren<Button>().Count(button => button.name.StartsWith("Collection · ")));
                var scroll = root.GetComponentsInChildren<ScrollRect>().Single();
                scroll.content.anchoredPosition = new Vector2(0, 90);
                ButtonNamed("Collection · pip").onClick.Invoke();
                Assert.AreEqual(AppPage.Collection, hud.CurrentPage);
                var slots = root.GetComponentsInChildren<Button>().Where(button => button.name.StartsWith("Collectible · ")).ToArray();
                Assert.AreEqual(10, slots.Length);
                Assert.AreEqual(4, slots.Count(button => button.interactable));
                Assert.IsFalse(ButtonNamed("Collectible · pip.glow-jelly").interactable);
                Assert.IsTrue(VisibleText("Coming soon"));
                var textures = root.GetComponentsInChildren<RawImage>().Where(image => image.texture is RenderTexture)
                    .Select(image => image.texture).Distinct().ToArray();
                Assert.AreEqual(4, textures.Length, "Lineup must reuse the shelf portraits, not allocate ten live cameras.");

                scroll.content.anchoredPosition = new Vector2(0, 120);
                ButtonNamed("Collectible · " + ToyCatalog.MoonId).onClick.Invoke();
                Assert.AreEqual(AppPage.Play, hud.CurrentPage);
                Assert.AreEqual(PockleTheme.FieldMoon, viewer.backgroundColor, "A disabled viewer still needs its backdrop set before play.");
                Assert.AreEqual(PockleTheme.FieldMoon, stageRoot.GetComponentsInChildren<Camera>().Single(camera => camera != viewer).backgroundColor);
                hud.SetVariant(PipVariant.MintSoft);
                Assert.AreEqual(PockleTheme.FieldMint, viewer.backgroundColor);
                hud.GoBack();
                Assert.AreEqual(AppPage.Collection, hud.CurrentPage);
                Assert.AreEqual(120, scroll.content.anchoredPosition.y, .1f);
                Assert.AreEqual(PrototypeStage.Background, viewer.backgroundColor);
                hud.GoBack();
                Assert.AreEqual(AppPage.Collections, hud.CurrentPage);
                Assert.AreEqual(90, scroll.content.anchoredPosition.y, .1f);

                ButtonNamed("Collection · moss").onClick.Invoke();
                Assert.AreEqual(AppPage.Collection, hud.CurrentPage);
                Assert.IsFalse(root.GetComponentsInChildren<Button>().Any(button => button.name.StartsWith("Collectible · ") && button.interactable));
                Assert.IsFalse(ButtonWithText("View boxes").interactable);
                Assert.IsFalse(ButtonWithText("Walk for one").interactable);
                Assert.AreEqual(0, scroll.content.anchoredPosition.y, .1f, "A different series should start at its top.");
                Assert.IsFalse(hud.ShowToy("moss.velvet-flock"));
                ButtonWithText("Home").onClick.Invoke();
                Assert.AreEqual(AppPage.Home, hud.CurrentPage);
                Assert.IsFalse(hud.GoBack(), "Selecting Home must clear the collection history.");
                CollectionAssert.AreEquivalent(before, session.OwnedCounts, "Browsing cannot grant toys or change the save.");
                yield return null;
            }
            finally
            {
                hud.BackdropChanged -= backdrop;
                Object.Destroy(stageRoot);
                RenderSettings.ambientLight = oldAmbient;
                RenderSettings.ambientMode = oldAmbientMode;
                RenderSettings.fog = oldFog;
            }
            yield return null;
        }

        private bool VisibleText(string value)
        {
            return root.GetComponentsInChildren<Text>().Any(text => text.text == value && text.enabled);
        }

        private Button ButtonNamed(string name, bool includeHidden = false)
        {
            return root.GetComponentsInChildren<Button>(includeHidden).First(button => button.name == name);
        }

        /// <summary>Home greets by time of day ("Morning, Lucía").</summary>
        private bool Greets(string name)
        {
            return root.GetComponentsInChildren<Text>().Any(text => text.enabled && text.text.EndsWith(", " + name)
                && (text.text.StartsWith("Morning") || text.text.StartsWith("Afternoon") || text.text.StartsWith("Evening")));
        }

        private Button ButtonWithText(string value)
        {
            return root.GetComponentsInChildren<Button>().First(button =>
                button.GetComponentsInChildren<Text>().Any(text => text.text == value));
        }
    }
}
#endif
