#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Pockle.Tests
{
    public sealed class AvatarUiTests
    {
        private const string First = "11111111-1111-4111-8111-111111111111", Second = "22222222-2222-4222-8222-222222222222";
        private GameObject root; private PrototypeHud hud; private ProfileAvatarService service;
        private string directory, verified;
        private const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
        private sealed class Account : IAccountService
        {
            public bool IsSignedIn { get; private set; } = true;
            public string Username => "AvatarTester";
            public bool DeleteSucceeds;
            public void SignIn(AccountProvider provider, Action<AccountResult> done) => done(new AccountResult(AccountStatus.SignedIn, hasUsername: true));
            public void ClaimUsername(string name, Action<UsernameCheck> done) => done(UsernameCheck.Available);
            public void SignOut(Action done) { IsSignedIn = false; done(); }
            public void RequestDeletion(Action<bool> done) { if (DeleteSucceeds) IsSignedIn = false; done(DeleteSucceeds); }
        }
        [UnitySetUp] public IEnumerator SetUp()
        {
            directory = Path.Combine(Application.temporaryCachePath, "AvatarUi-" + Guid.NewGuid().ToString("N"));
            root = new GameObject("Avatar UI isolated fixture");
            service = root.AddComponent<ProfileAvatarService>(); Set(service, "storageRoot", directory);
            hud = root.AddComponent<PrototypeHud>(); hud.Initialize(_ => { }, _ => { }, _ => { }, _ => { });
            verified = null; hud.SetAvatarService(service, () => verified);
            yield return null;
            Named("Your profile").onClick.Invoke(); Canvas.ForceUpdateCanvases();
        }
        [UnityTearDown] public IEnumerator TearDown()
        { Object.Destroy(root); yield return null; if (Directory.Exists(directory)) Directory.Delete(directory, true); }
        private static void Set(object target, string field, object value) => target.GetType().GetField(field, Private).SetValue(target, value);
        private T Field<T>(string name) => (T)typeof(PrototypeHud).GetField(name, Private).GetValue(hud);
        private object Call(string name, params object[] args) => typeof(PrototypeHud).GetMethod(name, Private).Invoke(hud, args);
        private Button Named(string name) => root.GetComponentsInChildren<Button>(true).Single(b => b.name == name);
        private void Bind(string id) { verified = id; hud.SyncAvatarAccount(); }
        private void Photo()
        {
            Directory.CreateDirectory(directory); string request = Guid.NewGuid().ToString("N"), path = Path.Combine(directory, request + ".png");
            var photo = new Texture2D(8, 4, TextureFormat.RGB24, false);
            photo.SetPixels(Enumerable.Range(0, 32).Select(i => i % 8 < 4 ? Color.red : Color.blue).ToArray()); photo.Apply();
            File.WriteAllBytes(path, photo.EncodeToPNG()); Object.DestroyImmediate(photo);
            Set(service, "request", request); Set(service, "requestPath", path);
            typeof(ProfileAvatarService).GetProperty("Stage").SetValue(service, AvatarPhotoStage.Picking);
            typeof(ProfileAvatarService).GetMethod("ReceivePickerResult", Private).Invoke(service,
                new object[] { "{\"request\":\"" + request + "\",\"status\":\"ok\"}" });
        }
        [Test] public void TwelveApprovedChoicesRequireVerifiedAccountAndPersistSeparately()
        {
            var choices = Field<Button[]>("profileAvatarChoices"); Assert.That(choices.Length, Is.EqualTo(12));
            Assert.That(choices.All(b => !b.interactable), Is.True);
            foreach (var id in AvatarCatalog.Ids) Assert.That(Resources.Load<Texture2D>(AvatarCatalog.Resource(id)), Is.Not.Null, id);
            Named("Avatar · fox").onClick.Invoke(); Assert.That(service.CanEdit, Is.False);
            Bind(First); Named("Avatar · fox").onClick.Invoke(); Assert.That(service.PresetId, Is.EqualTo("fox"));
            Assert.That(Field<RawImage>("profilePortrait").texture, Is.SameAs(service.Texture));
            Bind(Second); Assert.That(service.PresetId, Is.EqualTo(AvatarCatalog.DefaultId));
            Bind(First); Assert.That(service.PresetId, Is.EqualTo("fox"));
            Assert.That(Field<Text>("profileName"), Is.Not.Null);
            Assert.That(Field<RectTransform>("profileIdentity").GetComponentInChildren<InputField>(), Is.Null);
        }
        [Test] public void SelectionUsesABorderWithoutResizingTheAvatarCrop()
        {
            Bind(First); var choices=Field<Button[]>("profileAvatarChoices");
            var picture=choices[0].GetComponentInChildren<RawImage>();
            Vector2 min=picture.rectTransform.offsetMin,max=picture.rectTransform.offsetMax;Rect uv=picture.uvRect;
            Named("Avatar · pip-peach").onClick.Invoke();
            Assert.That(picture.rectTransform.offsetMin,Is.EqualTo(min));Assert.That(picture.rectTransform.offsetMax,Is.EqualTo(max));
            Assert.That(picture.uvRect,Is.EqualTo(uv));Assert.That(choices[0].GetComponentInChildren<AvatarSelectionRing>(true).enabled,Is.True);
            Named("Avatar · fox").onClick.Invoke();
            Assert.That(choices[0].GetComponentInChildren<AvatarSelectionRing>(true).enabled,Is.False);
            Assert.That(picture.rectTransform.offsetMin,Is.EqualTo(min));
        }
        [Test] public void CropPreviewPanZoomSaveAndCancelUseTheSamePixels()
        {
            Bind(First); service.SelectPreset("wally"); Photo(); Assert.That(hud.AvatarOverlayVisible, Is.True);
            Field<Slider>("avatarCropZoom").value = 2;
            Call("PanAvatarCrop", new Vector2(-1, 0));
            Assert.That(Field<RawImage>("avatarCropImage").uvRect.x, Is.GreaterThan(.5f));
            Call("CancelAvatarEditor"); Assert.That(hud.AvatarOverlayVisible, Is.False); Assert.That(service.PresetId, Is.EqualTo("wally")); Assert.That(service.HasPhoto, Is.False);
            Photo(); Field<Slider>("avatarCropZoom").value = 2; Call("PanAvatarCrop", new Vector2(-1, 0)); Call("SaveAvatarEditor");
            Assert.That(service.HasPhoto, Is.True); Assert.That(hud.AvatarOverlayVisible, Is.False);
            Assert.That(service.Texture.width, Is.EqualTo(512)); Assert.That(((Texture2D)service.Texture).GetPixel(256, 256).b, Is.GreaterThan(.9f));
            Assert.That(Field<Text>("profileScope").text, Is.EqualTo("Saved on this device"));
        }
        [Test] public void RegistrationDraftOnlyCommitsOnContinueAndSkipDiscards()
        {
            int continued = 0; Action done = () => continued++;
            Assert.That(Call("OfferRegistrationAvatar", done), Is.EqualTo(false));
            Bind(First); Assert.That(Call("OfferRegistrationAvatar", done), Is.EqualTo(true));
            Call("SelectProfileAvatar", "fox"); Assert.That(service.PresetId, Is.EqualTo(AvatarCatalog.DefaultId));
            Call("CancelAvatarEditor"); Assert.That(continued, Is.EqualTo(1)); Assert.That(service.PresetId, Is.EqualTo(AvatarCatalog.DefaultId));
            Call("OfferRegistrationAvatar", done); Call("SelectProfileAvatar", "wally"); Call("SaveAvatarEditor");
            Assert.That(continued, Is.EqualTo(2)); Assert.That(service.PresetId, Is.EqualTo("wally"));
        }
        [Test] public void AccountSwitchClosesCropAndCannotContinueOldRegistration()
        {
            Bind(First); int continued = 0; Call("OfferRegistrationAvatar", (Action)(() => continued++)); Photo();
            Bind(Second); Assert.That(hud.AvatarOverlayVisible, Is.False); Assert.That(service.PendingPhoto, Is.Null);
            Call("SaveAvatarEditor"); Assert.That(service.HasPhoto, Is.False); Assert.That(continued, Is.Zero);
        }
        [Test] public void SignOutHidesPhotoAndDeletionRemovesOnlyConfirmedScope()
        {
            Bind(First); Photo(); Call("SaveAvatarEditor"); var account = new Account(); hud.SetAccountService(account);
            Call("DeleteAccount"); Assert.That(service.HasPhoto, Is.True); Assert.That(Directory.Exists(Path.Combine(directory, First)), Is.True);
            Call("CloseModal"); Bind(Second); service.SelectPreset("ginger"); Bind(First);
            account.DeleteSucceeds = true; Call("DeleteAccount"); Assert.That(service.CanEdit, Is.False);
            Assert.That(Directory.Exists(Path.Combine(directory, First)), Is.False); Assert.That(Directory.Exists(Path.Combine(directory, Second)), Is.True);
            hud.SetAccountService(new Account()); Bind(Second); Call("SignOut"); Assert.That(service.CanEdit, Is.False);
            Assert.That(hud.ProfileAvatarTexture, Is.SameAs(Resources.Load<Texture2D>(AvatarCatalog.Resource(AvatarCatalog.DefaultId))));
        }
        [Test] public void ShortSafeAreaScrollsWithoutShrinkingActionTargets()
        {
            Bind(First); Call("OfferRegistrationAvatar", (Action)(() => { }));
            var safe = Field<RectTransform>("safeRoot"); safe.anchorMin = safe.anchorMax = new Vector2(.5f, .5f); safe.sizeDelta = new Vector2(620, 290);
            Call("LayoutAvatarOverlay");
            var card = Field<RectTransform>("avatarEditCard"); Assert.That(card.rect.height, Is.LessThanOrEqualTo(266));
            Assert.That(card.GetComponent<ScrollRect>().content.rect.height, Is.GreaterThan(card.rect.height));
            Assert.That(Field<Button>("avatarEditSave").GetComponent<RectTransform>().rect.height, Is.GreaterThanOrEqualTo(44));
        }
    }
}
#endif
