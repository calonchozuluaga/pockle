using System;
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
    // Editor-only visual fixtures. No store, server or account service is connected.
    [InitializeOnLoad]
    public static class CheckoutReviewCapture
    {
        private const string ActiveKey = "Pockle.CheckoutReview.Active";
        private static readonly string[] FixtureShots = { "shop-collections", "shop-mixes", "loading-wally", "ready-wally", "waiting-for-store", "payment-pending", "delivery-pending", "sold-out", "ready-mix", "mix-last-contents", "wally-secret" };
        private static bool ComingSoon => SessionState.GetBool(ActiveKey + ".ComingSoon", false);
        private static string[] Shots => ComingSoon ? new[] { "coming-soon-collections", "coming-soon-more-collections", "coming-soon-mixes" } : FixtureShots;
        private static PrototypeHud hud;
        private static GameObject root, cameraObject;
        private static Camera camera;
        private static RenderTexture target;
        private static int index, wait, lastFrame = -1;
        private static bool prepared, initialized;
        private static double readyAt;
        private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

        static CheckoutReviewCapture()
        {
            if (SessionState.GetBool(ActiveKey, false)) EditorApplication.update += Tick;
        }

        public static void Capture()
        {
            SessionState.SetBool(ActiveKey + ".ComingSoon", false);
            BeginCapture();
        }

        public static void CaptureComingSoon()
        {
            SessionState.SetBool(ActiveKey + ".ComingSoon", true);
            BeginCapture();
        }

        private static void BeginCapture()
        {
            Directory.CreateDirectory(Output);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            SessionState.SetBool(ActiveKey, true);
            EditorApplication.isPlaying = true;
        }

        private static string Output => Path.Combine(Directory.GetCurrentDirectory(), "test-results", ComingSoon ? "checkout-coming-soon" : "checkout-ui");

        public static void BuildComingSoonAndroid()
        {
            // Match the owner's existing Android beta. Production identity remains an unapproved proposal.
            var platform = UnityEditor.Build.NamedBuildTarget.Android;
            string original = PlayerSettings.GetApplicationIdentifier(platform);
            try
            {
                PlayerSettings.SetApplicationIdentifier(platform, "com.PocklePrototype.Pockle");
                WallyPilotPhoneBuild.Build();
                var path = Path.Combine(Directory.GetCurrentDirectory(), "Builds", "Pockle-ComingSoon-Checkout.apk");
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.Copy("/private/tmp/Pockle-Wally-Pilot.apk", path, true);
                Debug.Log("POCKLE_COMING_SOON_ANDROID_OK:" + path);
            }
            finally { PlayerSettings.SetApplicationIdentifier(platform, original); }
        }
        private static T Field<T>(string name) => (T)typeof(PrototypeHud).GetField(name, Private).GetValue(hud);
        private static void Invoke(string name) => typeof(PrototypeHud).GetMethod(name, Private).Invoke(hud, null);
        private static Button Button(string name)
        {
            var buttons = root.GetComponentsInChildren<Button>();
            var named = buttons.FirstOrDefault(b => b.name == name);
            return named != null ? named : buttons.First(b => b.GetComponentsInChildren<Button>().Length == 1 && b.GetComponentsInChildren<Text>().Any(t => t.text == name));
        }

        private static void Tick()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || lastFrame == Time.frameCount) return;
            lastFrame = Time.frameCount;
            try
            {
                if (!initialized)
                {
                    // Preserve the editor save: HUD creation can initialize/persist CollectionSession.
                    const string saveKey = "pockle.collection.v1";
                    SessionState.SetBool(ActiveKey + ".HadSave", PlayerPrefs.HasKey(saveKey));
                    SessionState.SetString(ActiveKey + ".Save", PlayerPrefs.GetString(saveKey));
                    root = new GameObject("Checkout review fixtures");
                    var session = root.AddComponent<CollectionSession>(); session.Initialize();
                    hud = root.AddComponent<PrototypeHud>(); hud.Initialize(_ => { }, _ => { }, _ => { }, _ => { }); hud.Bind(session);
                    cameraObject = new GameObject("Checkout portrait camera"); camera = cameraObject.AddComponent<Camera>();
                    camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.985f, .973f, .961f);
                    camera.transform.position = new Vector3(0, 0, -10); camera.orthographic = true;
                    target = new RenderTexture(1080, 2340, 24); target.Create(); camera.targetTexture = target;
                    foreach (var canvas in root.GetComponentsInChildren<Canvas>(true))
                    {
                        canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1;
                        var scaler = canvas.GetComponent<CanvasScaler>(); if (scaler != null) scaler.enabled = false;
                        canvas.scaleFactor = 1080f / 390;
                    }
                    Canvas.ForceUpdateCanvases(); ResetSafeArea(); Invoke("AdaptLayout");
                    Button("Boxes").onClick.Invoke();
                    if (!ComingSoon) { hud.SetCheckoutCatalog(Catalog()); hud.SetPendingDeliveries(2); }
                    initialized = true; wait = 8; return;
                }
                if (wait-- > 0 || EditorApplication.timeSinceStartup < readyAt) return;
                if (index >= Shots.Length)
                {
                    SessionState.SetBool(ActiveKey, false); EditorApplication.update -= Tick;
                    RestoreSave();
                    Debug.Log("POCKLE_CHECKOUT_CAPTURE_OK:" + Shots.Length);
                    EditorApplication.Exit(0); return;
                }
                if (!prepared) { Prepare(index); prepared = true; wait = 4; readyAt = EditorApplication.timeSinceStartup + .25; return; }
                ResetSafeArea(); Canvas.ForceUpdateCanvases();
                camera.Render(); var previous = RenderTexture.active; RenderTexture.active = target;
                var image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
                image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); image.Apply();
                File.WriteAllBytes(Path.Combine(Output, Shots[index] + ".png"), image.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(image); RenderTexture.active = previous;
                index++; prepared = false; wait = 2;
            }
            catch (Exception e)
            {
                SessionState.SetBool(ActiveKey, false); RestoreSave(); Debug.LogException(e); EditorApplication.Exit(1);
            }
        }

        private static void RestoreSave()
        {
            if (root != null) UnityEngine.Object.DestroyImmediate(root);
            const string saveKey = "pockle.collection.v1";
            if (SessionState.GetBool(ActiveKey + ".HadSave", false)) PlayerPrefs.SetString(saveKey, SessionState.GetString(ActiveKey + ".Save", "")); else PlayerPrefs.DeleteKey(saveKey);
            PlayerPrefs.Save();
        }

        private static void ResetSafeArea()
        {
            foreach (var name in new[] { "safeRoot", "modalSafeRoot" })
            {
                var rect = Field<RectTransform>(name); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            }
        }

        private static void Prepare(int shot)
        {
            if (ComingSoon)
            {
                if (hud.LaunchShopActive || hud.CheckoutVisible) throw new Exception("Coming-soon fixture unexpectedly bound a catalog or opened checkout");
                var offers = root.GetComponentsInChildren<Button>().Where(b => b.name.StartsWith("Buy · ")).ToArray();
                if (offers.Length != 13 || offers.Any(b => b.interactable || b.GetComponentInChildren<Text>().text != "Coming soon"))
                    throw new Exception("Expected13disabled Coming soon offers");
                var content = Field<RectTransform>("content"); var launch = Field<RectTransform>("launchShop"); var viewport = Field<RectTransform>("browsing");
                float y = shot == 0 ? -launch.anchoredPosition.y : shot == 1 ? -launch.anchoredPosition.y + 636 : content.rect.height - viewport.rect.height;
                content.anchoredPosition = new Vector2(0, Mathf.Clamp(y, 0, Mathf.Max(0, content.rect.height - viewport.rect.height)));
                return;
            }
            if (shot < 2)
            {
                var content = Field<RectTransform>("content"); var launch = Field<RectTransform>("launchShop");
                var viewport = Field<RectTransform>("browsing");
                float y = shot == 0 ? -launch.anchoredPosition.y : content.rect.height - viewport.rect.height;
                content.anchoredPosition = new Vector2(0, Mathf.Clamp(y, 0, Mathf.Max(0, content.rect.height - viewport.rect.height)));
                return;
            }
            if (shot == 2) { Button("Buy · Wally").onClick.Invoke(); return; }
            if (shot == 3) { if (!hud.ShowCheckoutQuote(WallyQuote())) throw new Exception("Fixture Wally quote rejected"); return; }
            if (shot == 4) { Button("Buy for 0,99 €").onClick.Invoke(); return; }
            if (shot == 5) { hud.SetCheckoutState("review-wally", CheckoutState.PaymentPending); return; }
            if (shot == 6) { hud.SetCheckoutState("review-wally", CheckoutState.DeliveryPending); return; }
            if (shot == 7) { hud.SetCheckoutState("review-wally", CheckoutState.SoldOut); return; }
            if (shot == 9)
            {
                var odds = Field<RectTransform>("checkoutOddsContent");
                odds.anchoredPosition = new Vector2(0, Mathf.Max(0, odds.rect.height - Field<RectTransform>("checkoutOdds").rect.height)); return;
            }
            if (shot == 10)
            {
                hud.GoBack(); Button("Buy · Wally").onClick.Invoke(); hud.ShowCheckoutQuote(WallyQuote());
                Field<RectTransform>("checkoutOddsContent").anchoredPosition = new Vector2(0, 26 + 7 * 198); return;
            }
            hud.GoBack(); Button("Buy · Mystery Mix 3").onClick.Invoke();
            hud.ShowCheckoutQuote(WithContents(new CheckoutQuote
            {
                QuoteId = "review-mix", OfferId = "review-mix3", Kind = CheckoutOfferKind.MysteryMix, BoxCount = 3, LocalizedPrice = "2,49 €",
                Odds = ToyCatalog.Collections.Where(c => c.Id != PipFusionStudies.CollectionId).OrderBy(c => c.Id == "wally" ? 0 : 1).Select(c => new CheckoutOddsGroup
                {
                    CollectionId = c.Id, CollectionPercent = 100.0 / 11,
                    Rows = c.Id == "wally" ? WallyQuote().Odds[0].Rows : new[] { new CheckoutOddsRow { Tier = RarityTier.Common, Percent = 99 }, new CheckoutOddsRow { Tier = RarityTier.Secret, Percent = 1 } }
                }).ToArray()
            }));
        }

        private static CheckoutCatalog Catalog() => new CheckoutCatalog
        {
            Status = CheckoutCatalogStatus.Ready,
            Offers = new[]
            {
                new CheckoutOffer { OfferId = "review-single", Kind = CheckoutOfferKind.CollectionBox, CollectionId = "wally", LocalizedPrice = "0,99 €" },
                new CheckoutOffer { OfferId = "review-soldout", Kind = CheckoutOfferKind.CollectionBox, CollectionId = "fox", LocalizedPrice = "0,99 €", Available = false, UnavailableReason = "Sold out" },
                new CheckoutOffer { OfferId = "review-mix3", Kind = CheckoutOfferKind.MysteryMix, BoxCount = 3, LocalizedPrice = "2,49 €" },
                new CheckoutOffer { OfferId = "review-mix5", Kind = CheckoutOfferKind.MysteryMix, BoxCount = 5, LocalizedPrice = "3,99 €" }
            }
        };

        private static CheckoutQuote WithContents(CheckoutQuote quote)
        {
            foreach (var group in quote.Odds)
            {
                var contents = new System.Collections.Generic.List<CheckoutContentsItem>();
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
            QuoteId = "review-wally", OfferId = "review-single", Kind = CheckoutOfferKind.CollectionBox, CollectionId = "wally", LocalizedPrice = "0,99 €",
            Odds = new[] { new CheckoutOddsGroup { CollectionId = "wally", Rows = new[] {
                new CheckoutOddsRow { Tier = RarityTier.Common, Percent = 88 }, new CheckoutOddsRow { Tier = RarityTier.Rare, Percent = 8 },
                new CheckoutOddsRow { Tier = RarityTier.Epic, Percent = 3 }, new CheckoutOddsRow { Tier = RarityTier.Secret, Percent = 1 } } } },
            Notes = new[] { "Internal sandbox fixture. No completion boost is applied." }
        });
    }
}
