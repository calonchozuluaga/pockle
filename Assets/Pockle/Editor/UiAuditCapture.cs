using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Pockle.Core;
using Pockle.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Editor
{
    // UI audit: every screen of the real HUD at phone size (390 x 845 pt, iPhone-style safe area), drawn from offline
    // fixtures. Never connects accounts or stores. Long pages are captured again at each scroll stop.
    // Run: Unity -batchmode -projectPath . -executeMethod Pockle.Editor.UiAuditCapture.Capture
    // Output folder: POCKLE_UI_AUDIT_OUT (default docs/ui-audit/01/screenshots). POCKLE_UI_AUDIT_ONLY=01,12 limits the run to
    // shots whose names start with those prefixes. POCKLE_UI_AUDIT_CUTOUTS=<folder> exports every owned toy's transparent
    // portrait there instead of taking screenshots.
    [InitializeOnLoad]
    public static class UiAuditCapture
    {
        private const string Active = "Pockle.UiAuditCapture";
        private const int ToyLayer = 28, MaxScrollStops = 5;
        private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

        private sealed class Shot
        {
            public string Name; public Action Prepare; public bool Scroll, Stage, End; public double Delay = .4;
        }

        private static PrototypeHud hud; private static GameObject root, stage; private static Camera camera, toyCamera, background;
        private static RenderTexture target; private static JellyToy previewToy;
        private static ProfileAvatarService avatarService; private static string avatarScope;
        private static List<Shot> shots; private static readonly List<string> written = new List<string>();
        private static int index, wait, last = -1, stop; private static double readyAt;
        private static bool initialized, prepared;

        private static string Output => Environment.GetEnvironmentVariable("POCKLE_UI_AUDIT_OUT")
            ?? Path.Combine(Directory.GetCurrentDirectory(), "docs/ui-audit/01/screenshots");

        static UiAuditCapture() { if (SessionState.GetBool(Active, false)) EditorApplication.update += Tick; }

        // ---- saved preferences: everything a fixture can touch is put back afterwards ----
        private static string[] StringKeys()
        {
            var keys = new List<string>
            {
                "pockle.collection.v1", "pockle.collection.v1.before-v5", "pockle.collection.v1.before-v6", "pockle.collection.v1.backup",
                LocalActivityFeed.LogKey, ShelfFriendships.Key, BadgeCabinet.Key, CollectionSession.BadgePreferenceKey,
                PrototypeHud.LiveShelfKey, PrototypeHud.LiveShelfSeenKey, PrototypeHud.MomentsKey,
                "pockle.profile.local.name", "pockle.profile.local.avatarId", "pockle.profile.local.favoriteId"
            };
            foreach (var collection in ToyCatalog.Collections) keys.Add("pockle.stage.environment.v1." + collection.Id);
            return keys.ToArray();
        }
        private static readonly string[] IntKeys =
        {
            StageLook.LightKey, StageLook.WorldKey, PipCharacterAsset.MeshyPreference, "pockle.profile.local.avatar", "pockle.profile.local.favorite",
            "pockle.onboarding.v1.ageConfirmed", "pockle.onboarding.v1.ageLocked", "pockle.onboarding.v1.completed"
        };

        public static void Capture()
        {
            if (Application.unityVersion != "6000.6.5f1") throw new InvalidOperationException("Pinned Unity required");
            foreach (var key in StringKeys()) { SessionState.SetBool(Active + key, PlayerPrefs.HasKey(key)); SessionState.SetString(Active + key + "value", PlayerPrefs.GetString(key, "")); }
            foreach (var key in IntKeys) { SessionState.SetBool(Active + key, PlayerPrefs.HasKey(key)); SessionState.SetInt(Active + key + "int", PlayerPrefs.GetInt(key)); }
            PlayerPrefs.SetInt(PipCharacterAsset.MeshyPreference, 1);

            // A mid-game collector: some collections nearly done, one finished, two untouched, spares for fusion.
            var want = new Dictionary<string, int> { { "pip", 9 }, { "fox", 6 }, { "axolotl", 4 }, { "firb", 99 }, { "coco", 3 }, { "snowy", 2 }, { "reiny", 1 }, { "guin", 5 }, { "wally", 8 } };
            var save = new CollectionSave { Version = CollectionProgress.SaveVersion, Inventory = new List<OwnedCollectible>(), Day = CollectionSession.Today };
            var counts = new Dictionary<string, int>();
            foreach (var collection in ToyCatalog.Collections)
            {
                if (collection.Id == PipFusionStudies.CollectionId || !want.TryGetValue(collection.Id, out int take)) continue;
                int i = 0;
                foreach (var toy in ToyCatalog.Collectibles)
                {
                    if (toy.CollectionId != collection.Id || !toy.Available) continue;
                    if (i >= take) break;
                    counts[toy.Id] = i % 3 == 0 ? 3 : i % 3 == 1 ? 2 : 1; i++;
                }
            }
            foreach (var id in new[] { ToyCatalog.PeachId, ToyCatalog.MoonId }) counts[id] = 3;
            foreach (var entry in counts) save.Inventory.Add(new OwnedCollectible(entry.Key, entry.Value));
            PlayerPrefs.SetString("pockle.collection.v1", JsonUtility.ToJson(save));
            PlayerPrefs.SetString("pockle.profile.local.name", "Collector");
            var live = new List<string> { ToyCatalog.PeachId, First("fox"), First("axolotl"), First("wally"), First("firb"), ToyCatalog.MoonId };
            PlayerPrefs.SetString(PrototypeHud.LiveShelfKey, string.Join(",", live.Where(id => id != null && counts.ContainsKey(id))));
            PlayerPrefs.DeleteKey(PrototypeHud.MomentsKey);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Directory.CreateDirectory(Output); SessionState.SetBool(Active, true); EditorApplication.isPlaying = true;
        }

        private static string First(string collectionId)
        {
            foreach (var toy in ToyCatalog.Collectibles) if (toy.CollectionId == collectionId && toy.Available) return toy.Id;
            return null;
        }

        private static void Restore()
        {
            if (avatarService != null && avatarScope != null) avatarService.DeleteAccountData(avatarScope);
            if (stage != null) UnityEngine.Object.DestroyImmediate(stage);
            if (root != null) UnityEngine.Object.DestroyImmediate(root);
            foreach (var key in StringKeys()) { if (SessionState.GetBool(Active + key, false)) PlayerPrefs.SetString(key, SessionState.GetString(Active + key + "value", "")); else PlayerPrefs.DeleteKey(key); }
            foreach (var key in IntKeys) { if (SessionState.GetBool(Active + key, false)) PlayerPrefs.SetInt(key, SessionState.GetInt(Active + key + "int", 0)); else PlayerPrefs.DeleteKey(key); }
            PlayerPrefs.Save(); SessionState.SetBool(Active, false);
            Debug.Log("POCKLE_UI_AUDIT_PREFS_RESTORED");
        }

        // ---- reflection into the HUD's private pages ----
        private static object Call(string name, params object[] args)
        {
            foreach (var method in typeof(PrototypeHud).GetMethods(Private))
            {
                if (method.Name != name) continue;
                var parameters = method.GetParameters();
                if (parameters.Length != args.Length) continue;
                bool fits = true;
                for (int i = 0; i < args.Length && fits; i++) fits = args[i] == null || parameters[i].ParameterType.IsInstanceOfType(args[i]);
                if (fits) return method.Invoke(hud, args);
            }
            throw new MissingMethodException("PrototypeHud." + name + "/" + args.Length);
        }
        private static T Field<T>(string name) => (T)typeof(PrototypeHud).GetField(name, Private).GetValue(hud);
        private static object Step(string name) => Enum.Parse(typeof(PrototypeHud).GetNestedType("OnboardingStep", BindingFlags.NonPublic), name);
        private static Button Find(string name)
        {
            var buttons = root.GetComponentsInChildren<Button>(true);
            var named = buttons.FirstOrDefault(b => b.name == name && b.gameObject.activeInHierarchy) ?? buttons.FirstOrDefault(b => b.name == name);
            if (named != null) return named;
            named = buttons.FirstOrDefault(b => b.GetComponentsInChildren<Text>(true).Any(t => t.text == name));
            if (named == null) throw new Exception("No button named " + name);
            return named;
        }

        // ---- fixtures ----
        private static WalkingBoxView Banked() => new WalkingBoxView { BankedCollectionIds = new[] { "wally" }, EarnedToday = 1, StepCredit = 320, CanOpen = true };
        private static WalkingBoxView Earning() => new WalkingBoxView { StepCredit = 680, EarnedToday = 1, SecondsUntilNextBox = 1800 };

        private static CheckoutQuote WithContents(CheckoutQuote quote)
        {
            foreach (var group in quote.Odds)
            {
                var contents = new List<CheckoutContentsItem>();
                foreach (var row in group.Rows)
                {
                    var toys = ToyCatalog.Collectibles.Where(t => t.CollectionId == group.CollectionId && RarityLook.FromTier(t.Tier) == row.Tier).ToArray();
                    foreach (var toy in toys) contents.Add(new CheckoutContentsItem { CollectibleId = toy.Id, Tier = row.Tier,
                        WithinCollectionPercent = row.Percent / toys.Length, Percent = row.Percent / toys.Length * (group.CollectionPercent ?? 100) / 100 });
                }
                group.Contents = contents;
            }
            return quote;
        }
        private static CheckoutQuote WallyQuote() => WithContents(new CheckoutQuote
        {
            QuoteId = "audit-wally", OfferId = "pockle.box.wally.v1", Kind = CheckoutOfferKind.CollectionBox, CollectionId = "wally", LocalizedPrice = "$0.99",
            Odds = new[] { new CheckoutOddsGroup { CollectionId = "wally", Rows = new[] {
                new CheckoutOddsRow { Tier = RarityTier.Common, Percent = 88 }, new CheckoutOddsRow { Tier = RarityTier.Rare, Percent = 8 },
                new CheckoutOddsRow { Tier = RarityTier.Epic, Percent = 3 }, new CheckoutOddsRow { Tier = RarityTier.Secret, Percent = 1 } } } },
            Notes = new[] { "Offline review fixture." }
        });

        private static void ShowPack(params string[] ids)
        {
            var items = new List<BoxRevealItem>();
            for (int i = 0; i < ids.Length; i++)
            {
                ToyCatalog.TryGetCollectible(ids[i], out var toy);
                items.Add(new BoxRevealItem { CollectibleId = ids[i], CollectionId = toy.CollectionId, Tier = RarityLook.FromTier(toy.Tier), IsNew = i % 2 == 0 });
            }
            var view = new BoxRevealView { ReceiptId = "audit-" + ids.Length + "-" + ids[0], Source = ids.Length > 1 ? BoxRevealSource.MysteryMix : BoxRevealSource.CollectionBox, Items = items };
            if (!hud.ShowBoxReveal(view)) throw new Exception("Reveal fixture refused");
        }
        private static string Tiered(string collectionId, RarityTier tier)
        {
            foreach (var toy in ToyCatalog.Collectibles)
                if (toy.CollectionId == collectionId && toy.Available && RarityLook.FromTier(toy.Tier) == tier) return toy.Id;
            return First(collectionId);
        }

        private static void EnsureStage(string id, StageWorld world, StageLight light)
        {
            hud.ShowToy(id);
            if (stage == null)
            {
                PrototypeStage.Create(out toyCamera, out var mount, out _, out _, out _); stage = toyCamera.transform.parent.gameObject;
                var obj = new GameObject("Audit toy"); obj.transform.SetParent(mount, false); previewToy = obj.AddComponent<JellyToy>(); previewToy.Initialize();
                var framing = toyCamera.GetComponent("StageFraming") as Behaviour; if (framing != null) framing.enabled = false;
                toyCamera.clearFlags = CameraClearFlags.Depth; toyCamera.targetTexture = target; toyCamera.rect = new Rect(0, 0, 1, 1); toyCamera.aspect = (float)target.width / target.height;
                background = stage.transform.Find("Full-screen background").GetComponent<Camera>(); background.targetTexture = target;
                camera.clearFlags = CameraClearFlags.Depth; camera.cullingMask = 1 << ToyLayer;
                foreach (var t in root.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = ToyLayer;
            }
            previewToy.TrySetCollectible(id); PrototypeStage.SetWorld(toyCamera, world, id); PrototypeStage.SetLight(toyCamera, light);
            hud.SetStatus("Tap · drag to turn");
        }
        // Same framing calculation as StageFraming, using the capture target rather than editor Screen dimensions.
        private static void FrameToy()
        {
            var p = hud.StageViewport;
            var frame = new Rect(p.x / target.width, p.y / target.height, p.width / target.width, p.height / target.height);
            float aspect = (float)target.width / target.height;
            float halfHeight = ToyOrbit.FrameSize(aspect * frame.width / frame.height, 1, true) / frame.height;
            float halfWidth = halfHeight * aspect;
            var offset = new Vector2((1 - 2 * frame.center.x) * halfWidth, (1 - 2 * frame.center.y) * halfHeight);
            toyCamera.orthographicSize = halfHeight; toyCamera.transform.LookAt(new Vector3(0, 1.15f, 0));
            toyCamera.projectionMatrix = Matrix4x4.Ortho(offset.x - halfWidth, offset.x + halfWidth, offset.y - halfHeight, offset.y + halfHeight, toyCamera.nearClipPlane, toyCamera.farClipPlane);
        }

        private static List<Shot> BuildShots()
        {
            var list = new List<Shot>();
            void Add(string name, Action prepare, bool scroll = false, double delay = .4, bool onStage = false, bool end = false)
                => list.Add(new Shot { Name = name, Prepare = prepare, Scroll = scroll, Delay = delay, Stage = onStage, End = end });
            string peach = ToyCatalog.PeachId, moon = ToyCatalog.MoonId, fox = First("fox");

            // Main tabs
            Add("01-home-box-ready", () => { hud.SetWalkingBoxes(Banked()); Call("SelectTab", AppPage.Home); }, true, .8);
            Add("02-home-earning", () => { hud.SetWalkingBoxes(Earning()); Call("SelectTab", AppPage.Home); });
            Add("03-home-signed-out", () => { hud.ClearWalkingBoxes(); Call("SelectTab", AppPage.Home); });
            Add("04-shelf", () => { hud.SetWalkingBoxes(Banked()); Call("SelectTab", AppPage.Shelf); }, true, .8);
            Add("05-shelf-live", () => Call("OpenLivingShelfView"), true, .8);
            Add("06-shelf-pick", () => Call("Navigate", AppPage.ShelfPick), true);
            Add("07-home-feature-picker", () => { Call("SelectTab", AppPage.Home); Call("BeginHomeFeatureChoice"); });
            Add("08-walk", () => Call("SelectTab", AppPage.Rewards), true);
            Add("09-community-a", () => { Call("SelectTab", AppPage.Social); Call("SelectCommunitySection", 0); }, true);
            Add("10-community-b", () => Call("SelectCommunitySection", 1), true);
            Add("11-community-c", () => Call("SelectCommunitySection", 2), true);
            Add("12-shop", () => Call("SelectTab", AppPage.Boxes), true, .8);
            Add("12-shop-end", () => Call("SelectTab", AppPage.Boxes), false, .8, false, true);
            Add("13-checkout-quote", () => { Find("Buy · Wally").onClick.Invoke(); if (!hud.ShowCheckoutQuote(WallyQuote())) throw new Exception("Quote refused"); });
            Add("14-walking-rules-dialog", () => { hud.GoBack(); Call("ShowBoxRules"); });

            // Collections and toys
            Add("15-collections", () => { Call("CloseModal"); Call("SelectTab", AppPage.Shelf); Call("Navigate", AppPage.Collections); }, true, .8);
            Add("16-collection-wally", () => Call("OpenCollection", "wally"), true, .8);
            Add("17-collection-pip", () => { hud.GoBack(); Call("OpenCollection", "pip"); }, true, .8);
            Add("18-toy-details", () => Call("OpenPersonality", peach), true, .8);
            Add("19-fuse-empty", () => { Call("SelectTab", AppPage.Shelf); Call("Navigate", AppPage.Fusion); }, true);
            Add("20-fuse-picked", () => { Call("PickSpare", peach); Call("PickSpare", moon); }, true);
            Add("21-fusion-moment", () => Call("Fuse"), false, 2.3);
            Add("22-fusion-reveal", () => { if (hud.FusionMomentPlaying) hud.GoBack(); }, true, .8);

            // You
            Add("23-you", () => { Call("SelectTab", AppPage.Home); Call("Navigate", AppPage.Profile); }, true);
            Add("24-badges", () => Call("OpenBadges"), true);
            Add("25-settings", () => { hud.GoBack(); Call("ShowSettings"); }, true);
            Add("26-confirm-sign-out", () => Find("Sign out").onClick.Invoke());

            // Box opening
            Add("27-reveal-waiting", () => { Call("CloseConfirm"); Call("SelectTab", AppPage.Home); ShowPack(peach, fox, First("axolotl")); });
            Add("28-reveal-first", () => hud.CompleteBoxRevealStep(), false, 1.2);
            Add("29-reveal-summary", () => hud.SkipBoxReveal(), false, .8);
            Add("30-reveal-epic", () => { hud.FinishBoxReveal(); ShowPack(Tiered("wally", RarityTier.Epic)); hud.CompleteBoxRevealStep(); }, false, 1.2);
            Add("31-reveal-secret", () => { hud.FinishBoxReveal(); ShowPack(Tiered("wally", RarityTier.Secret)); hud.CompleteBoxRevealStep(); }, false, 1.2);

            // First run
            Add("32-onboarding-splash", () => { hud.FinishBoxReveal(); Call("ShowOnboardingStep", Step("Splash")); }, false, .8);
            Add("33-onboarding-birthday", () => Call("ShowOnboardingStep", Step("Birthday")));
            Add("34-onboarding-sign-in", () => Call("ShowOnboardingStep", Step("SignIn")));
            Add("35-onboarding-name", () => Call("ShowOnboardingStep", Step("Username")));
            Add("36-onboarding-tutorial-1", () => Call("ShowOnboardingStep", Step("Tutorial")));
            Add("37-onboarding-tutorial-2", () => Call("ShowTutorialPage", 1));
            Add("38-onboarding-tutorial-3", () => Call("ShowTutorialPage", 2));
            Add("39-onboarding-tutorial-4", () => Call("ShowTutorialPage", 3));
            Add("40-onboarding-blocked", () => Call("ShowOnboardingStep", Step("Blocked")));

            // Play (needs the 3D stage; these come last)
            Add("41-play", () => { Field<RectTransform>("onboarding").gameObject.SetActive(false); hud.SetSettings(true, true, false); EnsureStage(peach, StageWorld.Garden, StageLight.Day); }, false, .8, true);
            Add("42-play-pockles", () => Call("SetTrayTab", 0), false, .6, true);
            Add("43-play-backdrop", () => Call("SetTrayTab", 1), false, .6, true);
            Add("44-play-light", () => Call("SetTrayTab", 2), false, .6, true);
            Add("45-play-about", () => Call("SetTrayTab", 3), false, .6, true);
            Add("46-play-fox-bubble", () =>
            {
                Call("SetTrayTab", -1); EnsureStage(fox, StageWorld.Home, StageLight.Golden);
                var response = ToyReactionResponses.Temperament("fox", PipTemperament.Grumpy);
                hud.ShowToyResponse(new ToyExpression(response.Text), null, 0, 3);
            }, false, .5, true);
            string only = Environment.GetEnvironmentVariable("POCKLE_UI_AUDIT_ONLY");
            if (!string.IsNullOrEmpty(only))
            {
                var prefixes = only.Split(',');
                list = list.Where(shot => prefixes.Any(prefix => shot.Name.StartsWith(prefix.Trim()))).ToList();
            }
            return list;
        }

        // Design reference: every owned toy's transparent portrait, exactly as the HUD draws it, with a manifest.
        private static int ExportCutouts(string folder)
        {
            Directory.CreateDirectory(folder);
            var session = root.GetComponent<CollectionSession>(); var lines = new List<string>(); int count = 0;
            foreach (var entry in session.OwnedCounts)
            {
                if (entry.Value <= 0 || !ToyCatalog.TryGetCollectible(entry.Key, out var toy)) continue;
                var picture = Call("CutoutPortrait", entry.Key) as Texture2D;
                if (picture == null) continue;
                string file = entry.Key.Replace('.', '_') + ".png";
                File.WriteAllBytes(Path.Combine(folder, file), picture.EncodeToPNG());
                lines.Add(string.Join("\t", entry.Key, file, toy.DisplayName, toy.CollectionId, RarityLook.FromTier(toy.Tier).ToString(), entry.Value.ToString()));
                count++;
            }
            File.WriteAllLines(Path.Combine(folder, "manifest.tsv"), lines);
            return count;
        }

        private static void ApplySafeArea()
        {
            foreach (var name in new[] { "safeRoot", "modalSafeRoot" })
            {
                var r = Field<RectTransform>(name);
                r.anchorMin = new Vector2(0, 34f / 844); r.anchorMax = new Vector2(1, 1 - 44f / 844); r.offsetMin = r.offsetMax = Vector2.zero;
            }
        }

        private static void Tick()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || last == Time.frameCount) return;
            last = Time.frameCount;
            try
            {
                if (!initialized)
                {
                    root = new GameObject("UI audit fixtures"); var session = root.AddComponent<CollectionSession>(); session.Initialize();
                    hud = root.AddComponent<PrototypeHud>(); hud.Initialize(_ => { }, _ => { }, _ => { }, _ => { }); hud.Bind(session); hud.SetFusionService(new LocalFusionService(session));
                    avatarScope = Guid.NewGuid().ToString(); avatarService = root.AddComponent<ProfileAvatarService>(); hud.SetAvatarService(avatarService, () => avatarScope); avatarService.SelectPreset("fox");
                    hud.SetWalkingBoxes(Banked());
                    var offers = new List<CheckoutOffer>();
                    foreach (var p in PaidBoxProducts.All)
                        offers.Add(new CheckoutOffer { OfferId = p.Id, CollectionId = p.CollectionId, BoxCount = p.Count, Kind = p.CollectionId == null ? CheckoutOfferKind.MysteryMix : CheckoutOfferKind.CollectionBox, Available = true, LocalizedPrice = p.Count == 1 ? "$0.99" : p.Count == 3 ? "$2.49" : "$3.99" });
                    hud.SetCheckoutCatalog(new CheckoutCatalog { Status = CheckoutCatalogStatus.Ready, Offers = offers });
                    var c = new GameObject("Portrait capture camera"); c.transform.SetParent(root.transform); camera = c.AddComponent<Camera>();
                    camera.orthographic = true; camera.transform.position = new Vector3(0, 0, -10); camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = PockleTheme.Page;
                    target = new RenderTexture(1080, 2340, 24); target.Create(); camera.targetTexture = target;
                    foreach (var canvas in root.GetComponentsInChildren<Canvas>(true))
                    {
                        canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
                        var scaler = canvas.GetComponent<CanvasScaler>(); if (scaler != null) scaler.enabled = false; canvas.scaleFactor = 1080f / 390;
                    }
                    shots = BuildShots(); initialized = true; wait = 30; return;
                }
                if (wait-- > 0 || EditorApplication.timeSinceStartup < readyAt) return;
                string cutouts = Environment.GetEnvironmentVariable("POCKLE_UI_AUDIT_CUTOUTS");
                if (!string.IsNullOrEmpty(cutouts))
                {
                    int exported = ExportCutouts(cutouts);
                    Restore(); Debug.Log("POCKLE_UI_AUDIT_CUTOUTS_OK:" + exported); EditorApplication.Exit(0); return;
                }
                if (index >= shots.Count)
                {
                    File.WriteAllLines(Path.Combine(Output, "shots.txt"), written);
                    Restore(); Debug.Log("POCKLE_UI_AUDIT_CAPTURE_OK:" + written.Count); EditorApplication.Exit(0); return;
                }
                var shot = shots[index];
                ApplySafeArea();
                if (!prepared)
                {
                    // One screen failing to open must not cost the rest of the audit.
                    try { shot.Prepare(); }
                    catch (Exception e)
                    {
                        var cause = e is TargetInvocationException && e.InnerException != null ? e.InnerException : e;
                        Debug.LogWarning("POCKLE_UI_AUDIT_SKIPPED:" + shot.Name + ": " + cause.GetType().Name + ": " + cause.Message + "\n" + cause.StackTrace);
                        written.Add("# skipped " + shot.Name + ": " + cause.Message.Replace('\n', ' '));
                        index++; wait = 2; return;
                    }
                    ApplySafeArea(); Call("AdaptLayout");
                    if (!shot.Stage)
                    {
                        // Pages remember their scroll; every capture starts from the top (or the very end when asked).
                        Canvas.ForceUpdateCanvases();
                        var page = Field<RectTransform>("content"); var window = (RectTransform)page.parent;
                        page.anchoredPosition = new Vector2(0, shot.End ? Mathf.Max(0, page.rect.height - window.rect.height) : 0);
                    }
                    prepared = true; stop = 0; wait = 14; readyAt = EditorApplication.timeSinceStartup + shot.Delay; return;
                }
                Canvas.ForceUpdateCanvases();
                if (shot.Stage)
                {
                    // UI built after the stage appeared (tray tiles, bubbles) must be on the layer the UI camera draws.
                    foreach (var t in root.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = ToyLayer;
                    toyCamera.targetTexture = target; background.targetTexture = target; FrameToy(); background.Render(); toyCamera.Render();
                }
                camera.Render(); var previous = RenderTexture.active; RenderTexture.active = target;
                var image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false); image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); image.Apply();
                string file = shot.Name + (stop == 0 ? "" : "-" + (char)('a' + stop)) + ".png";
                File.WriteAllBytes(Path.Combine(Output, file), image.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(image); RenderTexture.active = previous;
                written.Add(file);
                if (shot.Scroll && stop < MaxScrollStops - 1)
                {
                    var content = Field<RectTransform>("content"); var viewport = (RectTransform)content.parent;
                    float max = content.rect.height - viewport.rect.height, step = viewport.rect.height * .86f;
                    float next = Mathf.Min(max, (stop + 1) * step);
                    if (max > 24 && next > content.anchoredPosition.y + 24)
                    {
                        stop++; content.anchoredPosition = new Vector2(0, next);
                        wait = 12; readyAt = EditorApplication.timeSinceStartup + .45; return;
                    }
                }
                index++; prepared = false; wait = 4;
            }
            catch (Exception e)
            {
                Debug.LogError("POCKLE_UI_AUDIT_FAILED_AT:" + (shots != null && index < shots.Count ? shots[index].Name : "setup"));
                if (written.Count > 0) File.WriteAllLines(Path.Combine(Output, "shots.txt"), written);
                Restore(); Debug.LogException(e); EditorApplication.Exit(1);
            }
        }
    }
}
