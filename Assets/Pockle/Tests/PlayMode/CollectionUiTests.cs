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
        private bool hadVolume, hadMoments, hadWorld, hadLight;
        private int previousWorld, previousLight;
        // Live shelf picks and per-collection worlds are presentation preferences; restored after each test.
        private static readonly string[] stringPrefs = { PrototypeHud.LiveShelfKey, PrototypeHud.LiveShelfSeenKey,
            "pockle.stage.environment.v1.pip", "pockle.stage.environment.v1.fox", "pockle.stage.environment.v1.axolotl", LocalActivityFeed.LogKey, ShelfFriendships.Key };
        private readonly System.Collections.Generic.Dictionary<string, string> savedPrefs = new System.Collections.Generic.Dictionary<string, string>();
        private string previousMoments;
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
            hadMoments = PlayerPrefs.HasKey(PrototypeHud.MomentsKey); previousMoments = PlayerPrefs.GetString(PrototypeHud.MomentsKey);
            PlayerPrefs.DeleteKey(PrototypeHud.MomentsKey);
            hadWorld = PlayerPrefs.HasKey(StageLook.WorldKey); previousWorld = PlayerPrefs.GetInt(StageLook.WorldKey);
            hadLight = PlayerPrefs.HasKey(StageLook.LightKey); previousLight = PlayerPrefs.GetInt(StageLook.LightKey);
            PlayerPrefs.DeleteKey(StageLook.WorldKey); PlayerPrefs.DeleteKey(StageLook.LightKey);
            savedPrefs.Clear();
            foreach (var key in stringPrefs) { if (PlayerPrefs.HasKey(key)) savedPrefs[key] = PlayerPrefs.GetString(key); PlayerPrefs.DeleteKey(key); }
            ShelfFriendships.Reload();
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
            if (hadMoments) PlayerPrefs.SetString(PrototypeHud.MomentsKey, previousMoments); else PlayerPrefs.DeleteKey(PrototypeHud.MomentsKey);
            if (hadWorld) PlayerPrefs.SetInt(StageLook.WorldKey, previousWorld); else PlayerPrefs.DeleteKey(StageLook.WorldKey);
            if (hadLight) PlayerPrefs.SetInt(StageLook.LightKey, previousLight); else PlayerPrefs.DeleteKey(StageLook.LightKey);
            foreach (var key in stringPrefs) { if (savedPrefs.TryGetValue(key, out var value)) PlayerPrefs.SetString(key, value); else PlayerPrefs.DeleteKey(key); }
            PlayerPrefs.Save();
            ShelfFriendships.Reload();
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
                ButtonWithText("All collections").onClick.Invoke(); ButtonNamed("Collection · pip").onClick.Invoke();
                yield return new WaitForSecondsRealtime(.2f); CaptureShelfUi(canvas, camera, target, "pip-lineup");
                var scroll = root.GetComponentsInChildren<ScrollRect>().Single();
                FocusFirstSilhouette(scroll);
                for (int frame = 0; frame < 8; frame++)
                { yield return new WaitForSecondsRealtime(.3f); CaptureShelfUi(canvas, camera, target, "silhouettes-eager-" + frame); }
                Assert.IsTrue(root.GetComponentsInChildren<PersonalityMotion>().Any(motion => motion.name == "Eager outline"),
                    "Missing silhouettes hop and wave in 2D.");
                Assert.That(hud.ShelfPreviews.ActiveSilhouetteCount, Is.Zero, "Silhouettes take no 3D actor slot.");
                hud.SetSettings(true, false, true);
                yield return null; CaptureShelfUi(canvas, camera, target, "motion-calm");
                hud.GoBack(); ButtonNamed("Collection · tula").onClick.Invoke();
                yield return null; FocusFirstSilhouette(scroll); yield return null; CaptureShelfUi(canvas, camera, target, "planned-tula");
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

        private static IEnumerator WaitForPreviewFrame(ShelfPreviewRuntime previews, int previousFrame)
        {
            float deadline = Time.realtimeSinceStartup + 2f;
            while (previews.RenderedFrameCount <= previousFrame && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(previews.RenderedFrameCount, Is.GreaterThan(previousFrame), "An exposed found toy must produce a live preview frame.");
        }

        private static void FocusFirstSilhouette(ScrollRect scroll)
        {
            Canvas.ForceUpdateCanvases();
            // The lineup's eager outlines, not the collection hero's character shape.
            var outline = scroll.content.GetComponentsInChildren<CharacterSilhouetteGraphic>().First(graphic => graphic.name == "Eager outline");
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
            // The Shelf tab is still pictures with 2D motion; the live meshes are the found toys on a collection's lineup.
            OpenPipLineup();
            var scroll = root.GetComponentsInChildren<ScrollRect>().First(rect => rect.vertical);
            Canvas.ForceUpdateCanvases();
            var firstTile = ButtonNamed("Collectible · " + ToyCatalog.PeachId).GetComponent<RectTransform>();
            var tileBounds = RectTransformUtility.CalculateRelativeRectTransformBounds(scroll.content, firstTile);
            scroll.content.anchoredPosition = new Vector2(0, Mathf.Clamp(-tileBounds.max.y - 8, 0, Mathf.Max(0, scroll.content.rect.height - scroll.viewport.rect.height)));
            Canvas.ForceUpdateCanvases();
            yield return null; yield return null; yield return null;
            var previews = hud.ShelfPreviews;
            yield return WaitForPreviewFrame(previews, 0);
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
            var cover = new GameObject("Whole viewport occlusion", typeof(RectTransform));
            var coverRect = cover.GetComponent<RectTransform>(); coverRect.SetParent(scroll.viewport, false);
            coverRect.anchorMin = Vector2.zero; coverRect.anchorMax = Vector2.one; coverRect.offsetMin = coverRect.offsetMax = Vector2.zero;
            previews.SetOcclusion(coverRect);
            yield return null;
            Assert.That(previews.ActiveCount, Is.Zero, "Covered toy cards must not animate.");
            frames = previews.RenderedFrameCount;
            yield return new WaitForSecondsRealtime(.1f);
            Assert.That(previews.RenderedFrameCount, Is.EqualTo(frames));
            previews.SetOcclusion(root.GetComponentsInChildren<RectTransform>().Single(rect => rect.name == "Navigation"));
            Object.Destroy(cover);
            yield return null; yield return null;
            Assert.That(previews.ActiveCount, Is.GreaterThan(0));
            ButtonWithText("Boxes").onClick.Invoke();
            yield return null;
            Assert.That(previews.ActiveCount, Is.Zero, "Leaving the lineup must release every actor.");
            OpenPipLineup();
            scroll.content.anchoredPosition = new Vector2(0, Mathf.Clamp(-tileBounds.max.y - 8, 0, Mathf.Max(0, scroll.content.rect.height - scroll.viewport.rect.height)));
            yield return null; yield return null;
            hud.enabled = false;
            Assert.That(previews.ActiveCount, Is.Zero, "Disabling the HUD must stop its preview component too.");
            int previousFrame = previews.RenderedFrameCount;
            hud.enabled = true;
            yield return WaitForPreviewFrame(previews, previousFrame);
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
            ButtonNamed("Home collections · all").onClick.Invoke(); ButtonNamed("Collection · pip").onClick.Invoke();
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
            Assert.That(hud.ShelfPreviews.ActiveSilhouetteCount, Is.Zero, "Silhouettes move in 2D, without a 3D actor slot.");
            Assert.IsTrue(silhouettes.Where((graphic, index) => Vector2.Distance(graphic.rectTransform.anchoredPosition, positions[index]) > .01f).Any(),
                "Visible missing silhouettes should play the eager greeting.");
            hud.SetSettings(true, false, true);
            yield return null; // Motion calm returns every outline to rest on the next frame, then holds it there.
            var calmPositions = silhouettes.Select(graphic => graphic.rectTransform.anchoredPosition).ToArray();
            yield return new WaitForSecondsRealtime(.2f);
            Assert.That(hud.ShelfPreviews.ActiveCount, Is.Zero);
            Assert.IsTrue(silhouettes.Select((graphic, index) => graphic.rectTransform.anchoredPosition == calmPositions[index]).All(value => value));
            outline.Configure("tula", secret: true);
            Assert.IsTrue(outline.ConcealsSecret, "Secret placeholders must not use the secret's character outline.");
            Assert.IsFalse(hud.ShowToy("pip.glow-jelly"));
            CollectionAssert.AreEquivalent(inventory, session.OwnedCounts);
        }

        [UnityTest]
        public IEnumerator ShelfSelectionAndUnavailableCheckoutDoNotGrantToys()
        {
            Assert.AreEqual(AppPage.Home, hud.CurrentPage);
            ButtonWithText("See all").onClick.Invoke();
            yield return null;
            var portraits = ShelfTilePortraits();
            Assert.AreEqual(4, portraits.Length, "The live shelf shows four separate rendered toys.");
            Assert.IsTrue(portraits.All(image => image.texture.width > 0), "Each live-shelf toy has its cut-out portrait.");
            PipVariant selected = PipVariant.PeachJelly;
            hud.VariantChanged += choice => selected = choice;
            ButtonNamed("Slot · " + ToyCatalog.MoonId).onClick.Invoke();
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
            ButtonNamed("Slot · " + ToyCatalog.MoonId).onClick.Invoke(); // a collection-card slot plays the toy
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
            ButtonNamed("Home activity · see all").onClick.Invoke();
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
            Assert.IsTrue(VisibleText("3/10"), "The Pip card counts the toys found.");
            Assert.IsFalse(VisibleText("Moon Jelly"), "An undiscovered toy's name must not be shown.");
            var moon = ButtonNamed("Slot · " + ToyCatalog.MoonId);
            Assert.IsFalse(moon.interactable, "An undiscovered toy cannot be played.");
            Assert.IsFalse(moon.GetComponentsInChildren<RawImage>().Any(image => image.enabled && image.texture != null), "A missing toy shows no face.");
            Assert.IsTrue(VisibleText("×3"), "Spares show a count badge on their slot.");
            var portraits = ShelfTilePortraits();
            Assert.AreEqual(3, portraits.Length, "Only discovered toys sit on the live shelf.");
            Assert.AreEqual(3, hud.LiveShelf.Count);
            Assert.IsTrue(ButtonNamed("Open spot 3").gameObject.activeInHierarchy, "Free spots invite you to add a toy.");
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
                ButtonNamed("Home collections · all").onClick.Invoke();
                Assert.AreEqual(AppPage.Collections, hud.CurrentPage);
                Assert.AreEqual(ToyCatalog.Collections.Count, root.GetComponentsInChildren<Button>().Count(button => button.name.StartsWith("Collection · ")));
                var scroll = root.GetComponentsInChildren<ScrollRect>().Single();
                scroll.content.anchoredPosition = new Vector2(0, 90);
                ButtonNamed("Collection · pip").onClick.Invoke();
                Assert.AreEqual(AppPage.Collection, hud.CurrentPage);
                var slots = root.GetComponentsInChildren<Button>().Where(button => button.name.StartsWith("Collectible · ")).ToArray();
                Assert.AreEqual(10, slots.Length);
                Assert.AreEqual(4, slots.Count(button => button.interactable));
                Assert.IsFalse(ButtonNamed("Collectible · pip.glow-jelly").interactable);
                Assert.IsTrue(VisibleText("???"), "Available but unowned Pip finishes remain undiscovered.");
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

                ButtonNamed("Collection · tula").onClick.Invoke();
                Assert.AreEqual(AppPage.Collection, hud.CurrentPage);
                Assert.IsFalse(root.GetComponentsInChildren<Button>().Any(button => button.name.StartsWith("Collectible · ") && button.interactable));
                Assert.IsFalse(ButtonNamed("Collection boxes").interactable, "A collection without boxes on sale offers none.");
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

        /// <summary>Rendered portraits on the shelf's toy tiles (not the living-shelf scene above them).</summary>
        [UnityTest]
        public IEnumerator LivingShelfShowsMomentsAndOpensAPersonality()
        {
            var inventory = session.OwnedCounts.ToDictionary(pair => pair.Key, pair => pair.Value);
            ButtonWithText("Shelf").onClick.Invoke();
            ButtonNamed("Shelf time · day").onClick.Invoke();
            yield return null;
            // Gold, Moon and Peach share the top shelf; Mint starts the bottom one. Each shelf shows its first pair's moment.
            CollectionAssert.AreEqual(new[] { ToyCatalog.GoldId, ToyCatalog.MoonId, ToyCatalog.PeachId, ToyCatalog.MintId }, hud.LiveShelf.ToArray());
            Assert.IsTrue(VisibleText("Gold Glitter shows off for Moon Jelly"));
            ButtonNamed("Shelf time · night").onClick.Invoke();
            yield return null;
            Assert.IsTrue(VisibleText("Moon Jelly stays up stargazing"));
            Assert.IsTrue(VisibleText("2 shelf moments found"), "Seen moments are remembered.");
            Assert.IsTrue(ButtonNamed("Open spot 4").gameObject.activeInHierarchy && ButtonNamed("Open spot 5").gameObject.activeInHierarchy);
            ButtonNamed("Shelf toy · " + ToyCatalog.GoldId).onClick.Invoke();
            Assert.AreEqual(AppPage.Personality, hud.CurrentPage);
            Assert.IsTrue(VisibleText("Gold Glitter"));
            Assert.IsTrue(VisibleText("Strikes a pose"));
            Assert.IsTrue(VisibleText("Shows off for Moon"), "A found moment names its shelf friend.");
            Assert.IsTrue(VisibleText("Moment to find") && VisibleText("Seat them side by side"), "A friend whose moment is still hidden still shows itself, with a hint.");
            Assert.IsTrue(root.GetComponentsInChildren<Text>().Any(text => text.enabled && text.text.StartsWith("1 of ") && text.text.EndsWith(" found")), "The friends heading counts moments found.");
            Assert.IsTrue(root.GetComponentsInChildren<RectTransform>().Any(rect => rect.name.StartsWith("Meter · Energy · ")), "The personality sliders show.");
            Assert.IsTrue(root.GetComponentsInChildren<RectTransform>().Any(rect => rect.name.StartsWith("Meter · Mood · ")), "Mood is the fourth bar.");
            Assert.IsTrue(VisibleText("Show-off"), "Gold Glitter's temperament badge.");
            Assert.IsTrue(VisibleText("Best buds · Lv 1"), "Neighbours today are level-1 best buds.");
            Assert.AreEqual(1, ShelfFriendships.Level(ToyCatalog.GoldId, ToyCatalog.MoonId), "Sitting side by side on the open shelf counts a day.");
            Assert.AreEqual(0, ShelfFriendships.Level(ToyCatalog.GoldId, ToyCatalog.PeachId), "Only neighbours become friends.");
            Assert.AreEqual(10, root.GetComponentsInChildren<Button>().Count(button => button.name.StartsWith("Family · ")), "The family strip shows every Pip.");
            ButtonWithText("Play with Gold Glitter").onClick.Invoke();
            Assert.AreEqual(AppPage.Play, hud.CurrentPage);
            hud.GoBack(); Assert.AreEqual(AppPage.Personality, hud.CurrentPage);
            hud.GoBack(); Assert.AreEqual(AppPage.Shelf, hud.CurrentPage);
            // A restart keeps the moments that were found.
            Object.Destroy(root); yield return null;
            root = new GameObject("Restarted moments test");
            session = root.AddComponent<CollectionSession>(); session.Initialize();
            hud = root.AddComponent<PrototypeHud>(); hud.Initialize(_ => { }, _ => { }, _ => { }); hud.Bind(session);
            yield return null;
            ButtonWithText("Shelf").onClick.Invoke();
            Assert.IsTrue(VisibleText("2 shelf moments found"), "A restart keeps the moments found.");
            CollectionAssert.AreEquivalent(inventory, session.OwnedCounts, "Personalities never change the inventory.");
        }

        [UnityTest]
        public IEnumerator FusionPreviewShowsTheResultWithoutUsingSpares()
        {
            Object.Destroy(root); yield return null;
            var save = new CollectionSave { Version = CollectionProgress.SaveVersion, Day = CollectionSession.Today,
                Inventory = new System.Collections.Generic.List<OwnedCollectible> {
                    new OwnedCollectible(ToyCatalog.PeachId, 2), new OwnedCollectible(ToyCatalog.MoonId, 3),
                    new OwnedCollectible(ToyCatalog.GoldId, 2), new OwnedCollectible(ToyCatalog.MintId, 1) } };
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(save));
            root = new GameObject("Fusion preview test");
            session = root.AddComponent<CollectionSession>(); session.Initialize();
            hud = root.AddComponent<PrototypeHud>(); hud.Initialize(_ => { }, _ => { }, _ => { }); hud.Bind(session);
            yield return null;
            var inventory = session.OwnedCounts.ToDictionary(pair => pair.Key, pair => pair.Value);
            ButtonWithText("Shelf").onClick.Invoke();
            ButtonWithText("Fuse spares").onClick.Invoke();
            Assert.AreEqual(AppPage.Fusion, hud.CurrentPage);
            var fuse = ButtonNamed("Fuse");
            Assert.IsFalse(fuse.interactable, "Fusing needs two spares.");
            Assert.IsFalse(root.GetComponentsInChildren<Button>().Any(button => button.name == "Spare · " + ToyCatalog.MintId),
                "A single copy is never offered as a spare.");
            ButtonNamed("Spare · " + ToyCatalog.GoldId).onClick.Invoke();
            Assert.IsTrue(VisibleText("Gold Glitter") && VisibleText("Show-off"), "The pick stands in the fusion card with its temperament.");
            Assert.IsTrue(VisibleText("1 spare"), "A tile counts its spares, not what's left after picking.");
            ButtonNamed("Spare · " + ToyCatalog.MoonId).onClick.Invoke();
            Assert.IsTrue(fuse.interactable);
            Assert.IsTrue(VisibleText("This pair hides a secret recipe"));
            ButtonNamed("Fusion slot · Spare 1").onClick.Invoke();
            ButtonNamed("Spare · " + ToyCatalog.PeachId).onClick.Invoke();
            Assert.IsFalse(VisibleText("This pair hides a secret recipe"));
            Assert.IsTrue(VisibleText("Sunny + Dreamy"), "The personality hint names both temperaments.");
            Assert.IsTrue(VisibleText("A named hybrid?"));
            Assert.AreEqual("Fuse Peach + Moon", fuse.GetComponentInChildren<Text>().text);
            fuse.onClick.Invoke();
            yield return null;
            // The fusion moment plays first (unless motion is calm); a tap skips straight to the reveal.
            if (hud.FusionMomentPlaying) { ButtonNamed("Fusion moment").onClick.Invoke(); yield return null; }
            Assert.IsFalse(hud.FusionMomentPlaying);
            Assert.AreEqual(AppPage.FusionReveal, hud.CurrentPage);
            Assert.IsTrue(VisibleText("Dusk Pip"));
            Assert.IsTrue(VisibleText("Moon blue to peach"));
            // Peach (Sunny) + Moon (Dreamy) make the Stargazer hybrid (docs/PERSONALITY_SYSTEM.md section 5).
            Assert.IsTrue(VisibleText("Stargazer"));
            Assert.IsTrue(VisibleText("It's… a Stargazer! Points everyone at the night sky."));
            Assert.IsTrue(VisibleText("Preview: fusion isn't switched on yet, so your spares weren't used."));
            Assert.IsFalse(ButtonWithText("Add to shelf").interactable, "A preview can't add a toy to the shelf.");
            CollectionAssert.AreEquivalent(inventory, session.OwnedCounts, "A fusion preview must never use or grant toys.");
            ButtonWithText("Fuse again").onClick.Invoke();
            Assert.AreEqual(AppPage.Fusion, hud.CurrentPage);
        }

        /// <summary>Rendered portraits on the live shelf (one per seated toy).</summary>
        private RawImage[] ShelfTilePortraits()
        {
            return root.GetComponentsInChildren<Button>().Where(button => button.name.StartsWith("Shelf toy · "))
                .SelectMany(button => button.GetComponentsInChildren<RawImage>()).Where(image => image.name == "Toy" && image.enabled && image.texture != null).ToArray();
        }

        private void OpenPipLineup()
        {
            ButtonWithText("Shelf").onClick.Invoke();
            ButtonNamed("Shelf collection · pip").onClick.Invoke();
            Assert.AreEqual(AppPage.Collection, hud.CurrentPage);
        }

        [UnityTest]
        public IEnumerator ToyStageShowsPersonalityWarningsAndSpeechBubbles()
        {
            var inventory = session.OwnedCounts.ToDictionary(pair => pair.Key, pair => pair.Value);
            Assert.IsTrue(hud.ShowToy(ToyCatalog.GoldId));
            yield return null; Canvas.ForceUpdateCanvases();
            Assert.AreEqual(AppPage.Play, hud.CurrentPage);
            Assert.IsTrue(VisibleText("Show-off"), "The temperament badge sits under the name.");
            var warnings = ToyExpressions.Warnings(ToyCatalog.GoldId);
            Assert.IsTrue(VisibleText(warnings[0]), "The first 'handle with care' line shows.");
            ButtonNamed("Handle with care · 0").onClick.Invoke();
            yield return null;
            Assert.IsTrue(VisibleText(warnings[1]), "Tapping the chip shows the next line.");
            // A reaction gets a short sound or a sticker, never a sentence; picking up always speaks.
            var expression = hud.ShowToyExpression(ReactionCue.PickUp);
            Assert.IsFalse(expression.IsEmpty);
            Assert.LessOrEqual(expression.Sound.Length, 16);
            Assert.AreEqual(expression.Sound, hud.ToyBubbleText);
            hud.GoBack(); yield return null;
            Assert.AreEqual("", hud.ToyBubbleText, "Leaving the toy hides its bubble.");
            CollectionAssert.AreEquivalent(inventory, session.OwnedCounts, "Personality UI never changes the inventory.");
        }

        [UnityTest]
        public IEnumerator ToyStageShowsCollectionAndSwitchesWorldAndLight()
        {
            var backdrops = new System.Collections.Generic.List<Color>();
            var worlds = new System.Collections.Generic.List<StageWorld>();
            var lights = new System.Collections.Generic.List<StageLight>();
            hud.BackdropChanged += backdrops.Add; hud.WorldChanged += worlds.Add; hud.LightChanged += lights.Add;
            Assert.IsTrue(hud.ShowToy(ToyCatalog.MoonId));
            yield return null; Canvas.ForceUpdateCanvases();
            Assert.AreEqual(AppPage.Play, hud.CurrentPage);
            Assert.AreEqual(StageWorld.Studio, hud.CurrentWorld, "New players start in the studio, on the toy's own colour.");
            Assert.AreEqual(PockleTheme.FieldMoon, backdrops.Last());
            var pill = ButtonNamed("Stage collection");
            StringAssert.Contains(" collection · ", pill.GetComponentInChildren<Text>().text);
            Assert.IsTrue(root.GetComponentsInChildren<Text>().Any(text => text.enabled && text.text.Contains("NO. ")), "The eyebrow numbers the toy within its collection.");
            var closedArea = hud.StageViewport;
            Assert.That(closedArea.height, Is.GreaterThan(0), "The stage reports the free area between the title and the tray.");
            ButtonNamed("Stage tab · Collection").onClick.Invoke();
            Assert.IsTrue(root.GetComponentsInChildren<Button>().Any(button => button.name.StartsWith("Stage finish · ")), "The collection tab lists every member.");
            Assert.That(hud.StageViewport.height, Is.LessThan(closedArea.height), "An open panel shrinks the toy's area instead of covering it.");

            ButtonNamed("Stage tab · World").onClick.Invoke();
            var worldTiles = root.GetComponentsInChildren<Button>().Where(button => button.name.StartsWith("Stage world · ")).ToArray();
            Assert.AreEqual(1 + 5, worldTiles.Length, "Studio plus the Pip collection's five worlds.");
            var environments = new System.Collections.Generic.List<string>(); hud.EnvironmentChanged += environments.Add;
            var pipWorld = worldTiles[2]; string pipWorldId = pipWorld.name.Substring("Stage world · ".Length);
            pipWorld.onClick.Invoke();
            Assert.AreEqual(StageWorld.Home, hud.CurrentWorld);
            Assert.AreEqual(pipWorldId, hud.CurrentEnvironmentId);
            CollectionAssert.AreEqual(new[] { StageWorld.Home }, worlds);
            CollectionAssert.AreEqual(new[] { pipWorldId }, environments);
            Assert.AreNotEqual(PockleTheme.FieldMoon, backdrops.Last());
            Assert.AreEqual("env:" + pipWorldId, PlayerPrefs.GetString("pockle.stage.environment.v1.pip"), "The chosen world is remembered per collection.");
            hud.EnvironmentChanged -= environments.Add;

            ButtonNamed("Stage tab · Light").onClick.Invoke();
            ButtonNamed("Stage light · Night").onClick.Invoke();
            Assert.AreEqual(StageLight.Night, hud.CurrentLight);
            CollectionAssert.AreEqual(new[] { StageLight.Night }, lights);
            Assert.IsTrue(VisibleText("Night lamp"));
            Assert.IsTrue(hud.GoBack());
            Assert.AreEqual(AppPage.Play, hud.CurrentPage, "The phone's Back closes the open card before leaving the toy.");
            Assert.IsFalse(ButtonNamed("Stage tray close", true).gameObject.activeInHierarchy);

            ButtonNamed("Stage tab · About").onClick.Invoke();
            Assert.IsTrue(VisibleText("Meet Moon Jelly"), "The About tab introduces the toy.");
            Assert.IsTrue(VisibleText("Pip by nature · Moon touch"));
            ButtonNamed("Stage meet").onClick.Invoke();
            Assert.AreEqual(AppPage.Personality, hud.CurrentPage);
            hud.GoBack();
            Assert.AreEqual(AppPage.Play, hud.CurrentPage);

            pill.onClick.Invoke();
            Assert.AreEqual(AppPage.Collection, hud.CurrentPage, "The collection pill opens the toy's collection.");
            Assert.IsFalse(root.GetComponentsInChildren<Button>().Any(button => button.name.StartsWith("Stage tab · ")), "The tray belongs to the stage only.");
            hud.GoBack();
            Assert.AreEqual(AppPage.Play, hud.CurrentPage);
            Assert.AreEqual(StageWorld.Home, hud.CurrentWorld);
            hud.BackdropChanged -= backdrops.Add; hud.WorldChanged -= worlds.Add; hud.LightChanged -= lights.Add;
        }

        [UnityTest]
        public IEnumerator LiveShelfPickerTakesToysOffAndOnWithoutChangingInventory()
        {
            var inventory = session.OwnedCounts.ToDictionary(pair => pair.Key, pair => pair.Value);
            ButtonWithText("Shelf").onClick.Invoke();
            yield return null;
            Assert.AreEqual(4, hud.LiveShelf.Count, "New toys take free spots automatically.");
            ButtonNamed("Edit shelf").onClick.Invoke();
            Assert.AreEqual(AppPage.ShelfPick, hud.CurrentPage);
            Assert.IsTrue(VisibleText("4 of 6"));
            ButtonNamed("Pick · " + ToyCatalog.MoonId).onClick.Invoke();
            Assert.IsFalse(hud.LiveShelf.Contains(ToyCatalog.MoonId), "Tapping a seated toy takes it off.");
            Assert.IsTrue(VisibleText("3 of 6"));
            ButtonNamed("Pick · " + ToyCatalog.MoonId).onClick.Invoke();
            Assert.AreEqual(ToyCatalog.MoonId, hud.LiveShelf.Last(), "A toy put back takes the next free spot.");
            ButtonNamed("Pick · " + ToyCatalog.MoonId).onClick.Invoke();
            ButtonNamed("Shelf pick done").onClick.Invoke();
            Assert.AreEqual(AppPage.Shelf, hud.CurrentPage);
            // A restart keeps the picks, and a toy taken off isn't re-added just because a spot is free.
            Object.Destroy(root); yield return null;
            root = new GameObject("Restarted live shelf test");
            session = root.AddComponent<CollectionSession>(); session.Initialize();
            hud = root.AddComponent<PrototypeHud>(); hud.Initialize(_ => { }, _ => { }, _ => { }); hud.Bind(session);
            yield return null;
            ButtonWithText("Shelf").onClick.Invoke();
            Assert.AreEqual(3, hud.LiveShelf.Count);
            Assert.IsFalse(hud.LiveShelf.Contains(ToyCatalog.MoonId));
            CollectionAssert.AreEquivalent(inventory, session.OwnedCounts, "Arranging the shelf never changes the inventory.");
        }

        [Test]
        public void EveryCollectibleHasALayeredPersonality()
        {
            Assert.IsTrue(PersonalityBook.TryGet(ToyCatalog.FoxAuroraId, out var aurora));
            Assert.AreEqual("fox", aurora.CharacterId);
            Assert.AreEqual("Alert and curious", aurora.Nature);
            Assert.AreEqual("Aurora touch", aurora.TouchName);
            Assert.AreEqual("Dreamy", aurora.TouchTrait);
            Assert.AreEqual(PersonalityMotionStyle.Dream, aurora.Motion);
            Assert.IsTrue(aurora.Reactions.Any(r => r.Key == "At night" && r.Value == "Glows and stargazes"));
            foreach (var toy in ToyCatalog.Collectibles.Where(c => c.Available))
                Assert.IsTrue(PersonalityBook.TryGet(toy.Id, out var personality) && personality.TouchName.Length > 0, toy.Id);
            Assert.IsTrue(PersonalityBook.TryGet(ToyCatalog.GoldId, out var gold));
            Assert.AreEqual("Gold touch", gold.TouchName);
            // Mixed neighbours get a trait moment, the same whichever side each sits on.
            var mixed = PersonalityBook.FindMoment(ToyCatalog.GoldId, ToyCatalog.FoxAuroraId, false);
            Assert.IsNotNull(mixed);
            Assert.AreEqual(mixed.Id, PersonalityBook.FindMoment(ToyCatalog.FoxAuroraId, ToyCatalog.GoldId, false).Id);
            StringAssert.Contains("shows off", mixed.Caption);
        }

        [Test]
        public void TemperamentsRelationshipsAndFriendshipFollowThePersonalitySystem()
        {
            // Authored dials land each original Pip on its approved temperament (docs/PERSONALITY_SYSTEM.md section 2).
            Assert.AreEqual(Temperament.Sunny, PersonalityBook.TemperamentOf(ToyCatalog.PeachId));
            Assert.AreEqual(Temperament.Dreamy, PersonalityBook.TemperamentOf(ToyCatalog.MoonId));
            Assert.AreEqual(Temperament.ShowOff, PersonalityBook.TemperamentOf(ToyCatalog.GoldId));
            Assert.AreEqual(Temperament.Calm, PersonalityBook.TemperamentOf(ToyCatalog.MintId));
            Assert.AreEqual(Temperament.Dreamy, PersonalityBook.TemperamentOf(ToyCatalog.FoxAuroraId));
            Assert.AreEqual(Temperament.Grumpy, PersonalityBook.TemperamentOf("axolotl.matte-vinyl"));
            Assert.AreEqual(Temperament.Shy, PersonalityBook.TemperamentOf("fox.confetti-gel"));
            foreach (var toy in ToyCatalog.Collectibles.Where(c => c.Available))
            {
                var dials = PersonalityBook.Dials(toy.Id);
                for (int i = 0; i < 4; i++) Assert.That(dials[i], Is.InRange(0, 100), toy.Id);
            }

            Assert.AreEqual(Relationship.WarmingUp, PersonalityBook.RelationshipOf(ToyCatalog.PeachId, "axolotl.matte-vinyl", false));
            Assert.AreEqual(Relationship.Stargazers, PersonalityBook.RelationshipOf(ToyCatalog.PeachId, ToyCatalog.MoonId, true));
            Assert.AreEqual(new[] { 0, 1, 1, 2, 2, 3 }, new[] { 0, 1, 2, 3, 6, 7 }.Select(PersonalityBook.FriendshipLevel).ToArray());

            // Level 1 keeps the authored Pip moment and its id; deeper levels are new moments, the same either way round.
            var first = PersonalityBook.FindMoment(ToyCatalog.GoldId, ToyCatalog.MoonId, false, 1);
            Assert.AreEqual("Gold Glitter shows off for Moon Jelly", first.Caption);
            var second = PersonalityBook.FindMoment(ToyCatalog.GoldId, ToyCatalog.MoonId, false, 2);
            Assert.AreNotEqual(first.Id, second.Id);
            Assert.AreEqual(second.Id, PersonalityBook.FindMoment(ToyCatalog.MoonId, ToyCatalog.GoldId, false, 2).Id);
            foreach (var a in new[] { ToyCatalog.PeachId, ToyCatalog.GoldId, "axolotl.matte-vinyl", "fox.confetti-gel", "axolotl.gloss-vinyl" })
                foreach (var b in new[] { ToyCatalog.MoonId, ToyCatalog.MintId, ToyCatalog.FoxAuroraId, "axolotl.clear-jelly" })
                    for (int level = 1; level <= 3; level++)
                        foreach (bool night in new[] { false, true })
                        {
                            var moment = PersonalityBook.FindMoment(a, b, night, level);
                            Assert.IsNotNull(moment, a + "+" + b);
                            Assert.IsFalse(moment.Caption.Contains("{") || moment.FirstSays.Contains("{") || moment.SecondSays.Contains("{"), moment.Caption);
                        }

            // A day counts once per pair, and friendship never goes down.
            Assert.AreEqual(0, ShelfFriendships.Level(ToyCatalog.PeachId, ToyCatalog.MintId));
            Assert.IsTrue(ShelfFriendships.RecordTogether(ToyCatalog.PeachId, ToyCatalog.MintId, 100));
            Assert.IsFalse(ShelfFriendships.RecordTogether(ToyCatalog.MintId, ToyCatalog.PeachId, 100));
            ShelfFriendships.RecordTogether(ToyCatalog.PeachId, ToyCatalog.MintId, 101);
            Assert.IsTrue(ShelfFriendships.RecordTogether(ToyCatalog.PeachId, ToyCatalog.MintId, 140));
            Assert.AreEqual(2, ShelfFriendships.Level(ToyCatalog.MintId, ToyCatalog.PeachId));
            ShelfFriendships.Save(); ShelfFriendships.Reload();
            Assert.AreEqual(3, ShelfFriendships.Days(ToyCatalog.PeachId, ToyCatalog.MintId));

            // Fusion reveal copy: named hybrids first, otherwise the blended temperament.
            Assert.AreEqual("Secretly Sweet", PersonalityBook.HybridOf(Temperament.Grumpy, Temperament.Sunny).Name);
            StringAssert.StartsWith("It's… a Stargazer!", PersonalityBook.DescribeFusion(ToyCatalog.PeachId, ToyCatalog.MoonId).PersonalityLine);
        }

        [UnityTest]
        public IEnumerator HomeActivitySwitchesScopeAndCheersWithoutGrantingToys()
        {
            var inventory = session.OwnedCounts.ToDictionary(pair => pair.Key, pair => pair.Value);
            Assert.AreEqual(AppPage.Home, hud.CurrentPage);
            Assert.IsTrue(root.GetComponentsInChildren<Button>().Any(button => button.name.StartsWith("Home collection · ")), "Keep collecting lists collections.");
            ButtonNamed("Home activity scope · Everyone").onClick.Invoke();
            yield return null;
            Assert.IsTrue(VisibleText("Everyone"));
            ButtonNamed("Home activity scope · Friends").onClick.Invoke();
            yield return null;
            Assert.IsTrue(VisibleText("Friends' activity"));
            // The editor shows labelled sample posts; cheering one changes only its count, never the inventory.
            var cheer = root.GetComponentsInChildren<Button>().First(button => button.name.StartsWith("Cheer · sample:"));
            string before = cheer.GetComponentInChildren<Text>().text;
            cheer.onClick.Invoke();
            var again = ButtonNamed(cheer.name);
            Assert.AreNotEqual(before, again.GetComponentInChildren<Text>().text, "Cheering updates the count.");
            Assert.IsTrue(VisibleText("SAMPLE"), "Sample posts are always labelled.");
            ButtonNamed("Home activity · see all").onClick.Invoke();
            Assert.AreEqual(AppPage.Social, hud.CurrentPage);
            CollectionAssert.AreEquivalent(inventory, session.OwnedCounts);
            PlayerPrefs.DeleteKey("pockle.home.activityScope.v1");
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
