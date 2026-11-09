using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Pockle.Tests
{
    public sealed class ProfileAvatarTests
    {
        private const string First = "11111111-1111-4111-8111-111111111111", Second = "22222222-2222-4222-8222-222222222222";
        private string directory; private GameObject root; private ProfileAvatarService avatar;
        [SetUp] public void SetUp()
        {
            directory = Path.Combine(Application.temporaryCachePath, "AvatarTests-" + Guid.NewGuid().ToString("N"));
            root = new GameObject("Private avatar fixture"); avatar = root.AddComponent<ProfileAvatarService>(); Set("storageRoot", directory);
        }
        [TearDown] public void TearDown()
        { Object.DestroyImmediate(root); if (Directory.Exists(directory)) Directory.Delete(directory, true); }
        private void Set(string name, object value) => typeof(ProfileAvatarService).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(avatar, value);
        private void Receive(string request, string status) => typeof(ProfileAvatarService).GetMethod("ReceivePickerResult", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(avatar, new object[] { "{\"request\":\"" + request + "\",\"status\":\"" + status + "\"}" });
        private string PreparePhoto()
        {
            Directory.CreateDirectory(directory); string request = Guid.NewGuid().ToString("N"), file = Path.Combine(directory, request + ".jpg");
            var image = new Texture2D(4, 2, TextureFormat.RGB24, false);
            image.SetPixels(new[] { Color.red, Color.green, Color.blue, Color.white, Color.red, Color.green, Color.blue, Color.white }); image.Apply();
            File.WriteAllBytes(file, image.EncodeToPNG()); Object.DestroyImmediate(image);
            Set("request", request); Set("requestPath", file);
            typeof(ProfileAvatarService).GetProperty("Stage").SetValue(avatar, AvatarPhotoStage.Picking);
            Receive(request, "ok"); return request;
        }
        [Test] public void PresetsPersistPerAccountAndNeverExposeAnotherAccountsPhoto()
        {
            Assert.IsFalse(avatar.SelectPreset("fox")); avatar.BindAccount(First);
            Assert.IsTrue(avatar.SelectPreset("fox")); PreparePhoto(); Assert.AreEqual(AvatarPhotoStage.Cropping, avatar.Stage);
            Assert.IsTrue(avatar.CommitPhotoCrop()); Assert.IsTrue(avatar.HasPhoto);
            var saved = Directory.GetFiles(Path.Combine(directory, First), "*.png"); Assert.AreEqual(1, saved.Length);
            var picture = (Texture2D)avatar.Texture; Assert.AreEqual(512, picture.width); Assert.AreEqual(512, picture.height);
            Assert.IsFalse(avatar.IsCloudSaved); Assert.AreEqual("Saved on this device", avatar.SaveScopeLabel);
            avatar.BindAccount(Second); Assert.IsFalse(avatar.HasPhoto); Assert.AreEqual(AvatarCatalog.DefaultId, avatar.PresetId);
            Assert.IsTrue(avatar.SelectPreset("wally")); avatar.BindAccount(null); Assert.IsFalse(avatar.CanEdit); Assert.IsFalse(avatar.HasPhoto);
            avatar.BindAccount(First); Assert.IsTrue(avatar.HasPhoto); Assert.AreEqual("fox", avatar.PresetId);
            Assert.IsTrue(avatar.DeleteAccountData(First)); Assert.IsFalse(Directory.Exists(Path.Combine(directory, First)));
            avatar.BindAccount(Second); Assert.AreEqual("wally", avatar.PresetId);
        }
        [Test] public void CancelAndLatePickerResultsNeverChangeTheSavedSelection()
        {
            avatar.BindAccount(First); avatar.SelectPreset("ginger"); string old = PreparePhoto();
            avatar.CancelPhoto(); Assert.IsNull(avatar.PendingPhoto); Assert.AreEqual("ginger", avatar.PresetId); Assert.IsFalse(avatar.HasPhoto);
            Receive(old, "ok"); Assert.AreEqual(AvatarPhotoStage.Idle, avatar.Stage);
            old = PreparePhoto(); avatar.BindAccount(Second); Receive(old, "ok");
            Assert.IsNull(avatar.PendingPhoto); Assert.IsFalse(avatar.HasPhoto); Assert.AreEqual(AvatarCatalog.DefaultId, avatar.PresetId);
            avatar.BindAccount(First); Assert.AreEqual("ginger", avatar.PresetId);
        }
        [Test] public void ChangingToPresetRemovesPhotoButInvalidCropKeepsItUnchanged()
        {
            avatar.BindAccount(First); PreparePhoto(); Assert.IsFalse(avatar.CommitPhotoCrop(float.NaN));
            Assert.IsFalse(avatar.HasPhoto); Assert.IsNotNull(avatar.PendingPhoto);
            Assert.IsTrue(avatar.CommitPhotoCrop()); Assert.IsTrue(avatar.HasPhoto);
            Assert.IsTrue(avatar.SelectPreset("bundy")); Assert.IsFalse(avatar.HasPhoto);
            Assert.AreEqual(0, Directory.GetFiles(Path.Combine(directory, First), "*.png").Length);
            Assert.IsFalse(avatar.SelectPreset("../wrong")); Assert.AreEqual("bundy", avatar.PresetId);
        }
        [Test] public void DuplicateCallbacksAndDisableCannotReuseAPhotoSelection()
        {
            avatar.BindAccount(First); string token = PreparePhoto(); var pending = avatar.PendingPhoto;
            Receive(token, "ok"); Assert.AreSame(pending, avatar.PendingPhoto); Assert.AreEqual(AvatarPhotoStage.Cropping, avatar.Stage);
            avatar.enabled = false; Assert.IsNull(avatar.PendingPhoto); Assert.AreEqual(AvatarPhotoStage.Idle, avatar.Stage);
            avatar.enabled = true; Receive(token, "ok"); Assert.IsNull(avatar.PendingPhoto); Assert.IsFalse(avatar.HasPhoto);
        }
        [Test] public void NotificationFailureAfterCommitCannotRemoveSavedPhoto()
        {
            avatar.BindAccount(First); PreparePhoto();
            Action brokenUi = () => throw new IOException("Test subscriber failure"); avatar.Changed += brokenUi;
            try { Assert.Throws<IOException>(() => avatar.CommitPhotoCrop()); }
            finally { avatar.Changed -= brokenUi; }
            Assert.IsTrue(avatar.HasPhoto);
            avatar.BindAccount(null); avatar.BindAccount(First); Assert.IsTrue(avatar.HasPhoto, "Committed image survives failed UI notification and reopen.");
        }
        [Test] public void InterruptedWritesAreCleanedWithoutReplacingTheSelectedPhoto()
        {
            avatar.BindAccount(First); PreparePhoto(); Assert.IsTrue(avatar.CommitPhotoCrop());
            string folder = Path.Combine(directory, First), orphan = Path.Combine(folder, Guid.NewGuid().ToString("N") + ".png");
            File.WriteAllBytes(orphan, new byte[] { 1, 2, 3 });
            File.WriteAllText(Path.Combine(folder, "selection.json.interrupted.tmp"), "partial");
            Directory.CreateDirectory(Path.Combine(folder, "pending")); File.WriteAllText(Path.Combine(folder, "pending", "unfinished.jpg"), "partial");
            avatar.BindAccount(null); avatar.BindAccount(First);
            Assert.IsTrue(avatar.HasPhoto); Assert.IsFalse(File.Exists(orphan));
            Assert.IsFalse(Directory.Exists(Path.Combine(folder, "pending")));
            Assert.AreEqual(0, Directory.GetFiles(folder, "*.tmp").Length);
        }
    }
}
