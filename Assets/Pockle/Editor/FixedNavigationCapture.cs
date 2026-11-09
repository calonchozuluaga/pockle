using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Pockle.Core;
using Pockle.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
namespace Pockle.Editor
{
    /// <summary>Real HUD portrait evidence with offline inventory and restored editor preferences.</summary>
    [InitializeOnLoad] public static class FixedNavigationCapture
    {
        const string Active = "Pockle.FixedNavigationCapture";
        static readonly string[] Keys = { "pockle.collection.v1", "pockle.collection.v1.before-v5", "pockle.collection.v1.before-v6", "pockle.collection.v1.backup", CollectionSession.BadgePreferenceKey, PrototypeHud.LiveShelfKey, PrototypeHud.LiveShelfSeenKey, PrototypeHud.MomentsKey, "pockle.profile.local.name", "pockle.profile.local.avatarId", "pockle.profile.local.favoriteId" };
        static readonly string[] Shots = { "01-home", "02-owned-shelf", "03-walk", "04-community-feed", "05-community-friends", "06-community-exchange", "07-profile", "08-shop", "09-small-shelf" };
        static readonly AppPage[] Pages = { AppPage.Home, AppPage.Shelf, AppPage.Rewards, AppPage.Social, AppPage.Social, AppPage.Social, AppPage.Profile, AppPage.Boxes, AppPage.Shelf };
        static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        static PrototypeHud hud; static ProfileAvatarService avatarService; static string avatarScope; static GameObject root; static Camera camera; static RenderTexture target;
        static int index, wait, last = -1; static bool initialized, prepared;
        static string Output => Path.Combine(Directory.GetCurrentDirectory(), "docs/fixed-navigation-build/01/screenshots");
        static FixedNavigationCapture() { if (SessionState.GetBool(Active, false)) EditorApplication.update += Tick; }
        public static void Capture()
        {
            if (Application.unityVersion != "6000.6.5f1") throw new InvalidOperationException("Pinned Unity required");
            foreach (var key in Keys) { SessionState.SetBool(Active + key, PlayerPrefs.HasKey(key)); SessionState.SetString(Active + key + "value", PlayerPrefs.GetString(key, "")); }
            var save = new CollectionSave { Version = CollectionProgress.SaveVersion, Inventory = new List<OwnedCollectible>(), Day = CollectionSession.Today };
            foreach (var id in new[] { ToyCatalog.PeachId, ToyCatalog.MoonId, ToyCatalog.GoldId, ToyCatalog.MintId, "fox.gloss-vinyl", "axolotl.clear-jelly", "firb.matte-vinyl", WallyCatalog.ClassicId })
                if (ToyCatalog.TryGetCollectible(id, out var toy) && toy.Available) save.Inventory.Add(new OwnedCollectible(id, 3));
            PlayerPrefs.SetString("pockle.collection.v1", JsonUtility.ToJson(save));
            PlayerPrefs.SetString("pockle.profile.local.name", "Collector");
            PlayerPrefs.SetString(PrototypeHud.LiveShelfKey, string.Join(",", save.Inventory.ConvertAll(x => x.Id).GetRange(0, Math.Min(6, save.Inventory.Count))));
            PlayerPrefs.DeleteKey(PrototypeHud.MomentsKey);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Directory.CreateDirectory(Output); SessionState.SetBool(Active, true); EditorApplication.isPlaying = true;
        }
        static void Call(string method, params object[] args) => typeof(PrototypeHud).GetMethod(method, Private).Invoke(hud, args);
        static T Field<T>(string name) => (T)typeof(PrototypeHud).GetField(name, Private).GetValue(hud);
        static void Restore()
        {
            if (avatarService != null && avatarScope != null && !avatarService.DeleteAccountData(avatarScope)) throw new Exception("Capture avatar cleanup failed");
            if (root != null) UnityEngine.Object.DestroyImmediate(root);
            foreach (var key in Keys) { if (SessionState.GetBool(Active + key, false)) PlayerPrefs.SetString(key, SessionState.GetString(Active + key + "value", "")); else PlayerPrefs.DeleteKey(key); }
            PlayerPrefs.Save(); SessionState.SetBool(Active, false);
            foreach (var key in Keys) if (PlayerPrefs.HasKey(key) != SessionState.GetBool(Active + key, false) || (PlayerPrefs.HasKey(key) && PlayerPrefs.GetString(key) != SessionState.GetString(Active + key + "value", ""))) throw new Exception("Capture preference restoration failed: " + key);
            Debug.Log("POCKLE_FIXED_NAV_PREFS_RESTORED");
        }
        static void Tick()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || last == Time.frameCount) return; last = Time.frameCount;
            try
            {
                if (!initialized)
                {
                    root = new GameObject("Offline fixed navigation capture"); var session = root.AddComponent<CollectionSession>(); session.Initialize();
                    hud = root.AddComponent<PrototypeHud>(); hud.Initialize(_ => { }, _ => { }, _ => { }, _ => { }); hud.Bind(session); hud.SetFusionService(new LocalFusionService(session));
                    avatarScope = Guid.NewGuid().ToString(); avatarService = root.AddComponent<ProfileAvatarService>(); hud.SetAvatarService(avatarService, () => avatarScope); avatarService.SelectPreset("pip-peach");
                    hud.SetWalkingBoxes(new WalkingBoxView { StepCredit = 680, EarnedToday = 1, SecondsUntilNextBox = 1800 });
                    var offers = new List<CheckoutOffer>(); foreach (var p in PaidBoxProducts.All) offers.Add(new CheckoutOffer { OfferId = p.Id, CollectionId = p.CollectionId, BoxCount = p.Count, Kind = p.CollectionId == null ? CheckoutOfferKind.MysteryMix : CheckoutOfferKind.CollectionBox, Available = true, LocalizedPrice = p.Count == 1 ? "$0.99" : p.Count == 3 ? "$2.49" : "$3.99" }); hud.SetCheckoutCatalog(new CheckoutCatalog { Status = CheckoutCatalogStatus.Ready, Offers = offers });
                    var c = new GameObject("Portrait capture camera"); c.transform.SetParent(root.transform); camera = c.AddComponent<Camera>(); camera.orthographic = true; camera.transform.position = new Vector3(0, 0, -10); camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = PockleTheme.Page;
                    target = new RenderTexture(1080, 2340, 24); target.Create(); camera.targetTexture = target;
                    foreach (var canvas in root.GetComponentsInChildren<Canvas>(true)) { canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = 1; var scaler = canvas.GetComponent<CanvasScaler>(); if (scaler != null) scaler.enabled = false; canvas.scaleFactor = 1080f / 390; }
                    initialized = true; wait = 30; return;
                }
                if (wait-- > 0) return;
                if (index >= Shots.Length) { Restore(); Debug.Log("POCKLE_FIXED_NAV_CAPTURE_OK:" + Shots.Length); EditorApplication.Exit(0); return; }
                if (!prepared)
                {
                    if (index == 8)
                    {
                        target.Release(); UnityEngine.Object.DestroyImmediate(target); target = new RenderTexture(960, 1704, 24); target.Create(); camera.targetTexture = target;
                        foreach (var canvas in root.GetComponentsInChildren<Canvas>(true)) canvas.scaleFactor = 3;
                    }
                    Canvas.ForceUpdateCanvases();
                    foreach (var name in new[] { "safeRoot", "modalSafeRoot" })
                    {
                        var r = Field<RectTransform>(name); r.anchorMin = new Vector2(0, index == 8 ? 0 : 34f / 844); r.anchorMax = new Vector2(1, 1 - (index == 8 ? 20f / 568 : 44f / 844)); r.offsetMin = r.offsetMax = Vector2.zero;
                    }
                    Canvas.ForceUpdateCanvases(); Call("Navigate", Pages[index]);
                    if (index >= 3 && index <= 5) Call("SelectCommunitySection", index - 3);
                    Call("AdaptLayout"); prepared = true; wait = 16; return;
                }
                Canvas.ForceUpdateCanvases(); camera.Render(); var previous = RenderTexture.active; RenderTexture.active = target;
                var image = new Texture2D(target.width, target.height, TextureFormat.RGB24, false); image.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0); image.Apply();
                File.WriteAllBytes(Path.Combine(Output, Shots[index] + ".png"), image.EncodeToPNG()); UnityEngine.Object.DestroyImmediate(image); RenderTexture.active = previous;
                index++; prepared = false; wait = 4;
            }
            catch (Exception e) { Restore(); Debug.LogException(e); EditorApplication.Exit(1); }
        }
    }
}
