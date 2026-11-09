using System;
using System.IO;
using Pockle.Core;
using UnityEngine;

namespace Pockle.Runtime
{
    public enum AvatarPhotoStage { Idle, Picking, Cropping }

    /// <summary>Account-separated device avatars. No cloud upload or public publication is implemented here.</summary>
    [DisallowMultipleComponent]
    public sealed class ProfileAvatarService : MonoBehaviour
    {
        [Serializable] private sealed class Selection { public int version = 1; public string presetId = AvatarCatalog.DefaultId, photoId = ""; }
        [Serializable] private sealed class PickerReply { public string request = "", status = ""; }
        private string storageRoot, owner = "", request = "", requestPath = "";
        private Selection selection = new Selection();
        private Texture2D photo, pending;
        public event Action Changed;
        public string AccountId => owner;
        public string PresetId => selection.presetId;
        public bool HasPhoto => photo != null;
        public bool CanEdit => owner.Length > 0;
        public bool IsCloudSaved => false;
        public string SaveScopeLabel => "Saved on this device";
        public string Message { get; private set; } = "";
        public AvatarPhotoStage Stage { get; private set; }
        public Texture2D PendingPhoto => pending;
        public Texture Texture => photo != null ? photo : Resources.Load<Texture2D>(AvatarCatalog.Resource(PresetId));
        private string Root => storageRoot ?? (storageRoot = Path.Combine(Application.persistentDataPath, "ProfileAvatars", "v1"));
        private string Folder => Path.Combine(Root, owner);

        /// <summary>Caller supplies Supabase's verified UserId, or null immediately on sign-out. Never use a name/email.</summary>
        public void BindAccount(string verifiedUserId)
        {
            string next = AvatarCatalog.AccountScope(verifiedUserId);
            if (next == owner) return;
            CancelPhoto(false); DisposeTexture(ref photo); owner = next; selection = new Selection(); Message = "";
            if (CanEdit)
            {
                try
                {
                    string file = Path.Combine(Folder, "selection.json");
                    byte[] manifest = File.Exists(file) ? AvatarFiles.ReadBounded(file, AvatarFiles.MaximumManifestBytes) : null;
                    var saved = manifest != null ? JsonUtility.FromJson<Selection>(System.Text.Encoding.UTF8.GetString(manifest)) : null;
                    if (saved != null && saved.version == 1 && AvatarCatalog.Contains(saved.presetId) &&
                        (string.IsNullOrEmpty(saved.photoId) || AvatarCatalog.PhotoId(saved.photoId)))
                    {
                        selection = saved;
                        if (!string.IsNullOrEmpty(saved.photoId)) photo = ReadPhoto(Path.Combine(Folder, saved.photoId + ".png"));
                        if (!string.IsNullOrEmpty(saved.photoId) && photo == null)
                            Message = "Your saved photo could not be opened. You can choose it again.";
                        CleanOrphans(Folder, saved.photoId);
                    }
                    else if (File.Exists(file)) Message = "Your avatar selection could not be opened. Choose an avatar to replace it.";
                    else CleanOrphans(Folder, "");
                }
                catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException)
                { Message = "Your saved photo could not be opened. You can choose it again."; }
                CleanPending(Folder);
            }
            Changed?.Invoke();
        }
        public bool SelectPreset(string id)
        {
            if (!CanEdit || !AvatarCatalog.Contains(id)) return false;
            CancelPhoto(false);
            bool saved = Commit(new Selection { presetId = id }, null); Changed?.Invoke(); return saved;
        }
        public bool ClearPhoto() => SelectPreset(AvatarCatalog.DefaultId);

        public bool PickPhoto()
        {
            if (!CanEdit || Stage != AvatarPhotoStage.Idle) return false;
            try
            {
                request = Guid.NewGuid().ToString("N");
                string scratch = Path.Combine(Folder, "pending"); Directory.CreateDirectory(scratch);
                requestPath = Path.Combine(scratch, request + ".jpg");
                if (!NativeAvatarPicker.Begin(request, requestPath))
                { CancelPhoto(false); Message = "Photo selection is not available on this device. Choose a Pockle avatar instead."; Changed?.Invoke(); return false; }
                Stage = AvatarPhotoStage.Picking; Message = "";
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is InvalidOperationException)
            { CancelPhoto(false); Message = "Photos could not be opened. Your avatar has not changed."; Changed?.Invoke(); return false; }
            Changed?.Invoke(); return true;
        }

        private void Update()
        {
            if (Stage == AvatarPhotoStage.Picking) ReceivePickerResult(NativeAvatarPicker.TakeResult(request));
        }
        private void ReceivePickerResult(string reply)
        {
            if (string.IsNullOrEmpty(reply) || reply.Length > 1024) return;
            PickerReply result;
            try { result = JsonUtility.FromJson<PickerReply>(reply); } catch (ArgumentException) { return; }
            if (result == null || result.request != request || request.Length == 0 || !CanEdit || Stage != AvatarPhotoStage.Picking) return;
            if (result.status == "ok")
            {
                DisposeTexture(ref pending); pending = ReadPhoto(requestPath); DeleteFile(requestPath); requestPath = ""; request = "";
                if (pending != null) { Stage = AvatarPhotoStage.Cropping; Message = "Adjust the photo, then save. It stays on this device."; Changed?.Invoke(); return; }
            }
            bool cancelled = result.status == "cancel";
            CancelPhoto(false); Message = cancelled ? "" : "That photo could not be opened. Your avatar has not changed."; Changed?.Invoke();
        }

        public bool CommitPhotoCrop(float centerX = .5f, float centerY = .5f, float zoom = 1)
        {
            if (!CanEdit || Stage != AvatarPhotoStage.Cropping || pending == null ||
                !AvatarCrop.TryCreate(pending.width, pending.height, centerX, centerY, zoom, out var crop)) return false;
            var square = new Texture2D(AvatarCatalog.OutputSize, AvatarCatalog.OutputSize, TextureFormat.RGB24, false);
            try
            {
                var pixels = new Color[AvatarCatalog.OutputSize * AvatarCatalog.OutputSize];
                for (int y = 0; y < AvatarCatalog.OutputSize; y++)
                    for (int x = 0; x < AvatarCatalog.OutputSize; x++)
                        pixels[y * AvatarCatalog.OutputSize + x] = pending.GetPixelBilinear(
                            (float)((crop.X + (x + .5) * crop.Size / AvatarCatalog.OutputSize) / pending.width),
                            (float)((crop.Y + (y + .5) * crop.Size / AvatarCatalog.OutputSize) / pending.height));
                square.SetPixels(pixels); square.Apply(false, false);
                // Re-encoding these new pixels strips source EXIF/GPS and preserves no camera filename.
                byte[] bytes = square.EncodeToPNG();
                if (bytes == null || bytes.Length > 1200000) { Message = "That photo could not be saved."; return false; }
                bool saved = Commit(new Selection { presetId = selection.presetId, photoId = Guid.NewGuid().ToString("N") }, bytes);
                if (saved) CancelPhoto(false);
                Changed?.Invoke();
                return saved;
            }
            finally { Destroy(square); }
        }

        public void CancelPhoto() => CancelPhoto(true);
        private void CancelPhoto(bool notify)
        {
            if (request.Length > 0) NativeAvatarPicker.Cancel(request);
            request = ""; DeleteFile(requestPath); requestPath = ""; DisposeTexture(ref pending); Stage = AvatarPhotoStage.Idle;
            if (notify) Changed?.Invoke();
        }

        private bool Commit(Selection next, byte[] bytes)
        {
            string newPhoto = bytes != null ? Path.Combine(Folder, next.photoId + ".png") : "";
            string temporary = ""; Texture2D candidate = null;
            try
            {
                Directory.CreateDirectory(Folder);
                if (bytes != null)
                {
                    AvatarFiles.WriteNewDurable(newPhoto, bytes);
                    candidate = ReadPhoto(newPhoto);
                    if (candidate == null) throw new IOException("Avatar decode failed before commit.");
                }
                string file = Path.Combine(Folder, "selection.json"); temporary = file + "." + Guid.NewGuid().ToString("N") + ".tmp";
                AvatarFiles.WriteNewDurable(temporary, System.Text.Encoding.UTF8.GetBytes(JsonUtility.ToJson(next)));
                if (File.Exists(file)) File.Replace(temporary, file, null); else File.Move(temporary, file);
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is NotSupportedException)
            { DisposeTexture(ref candidate); DeleteFile(temporary); DeleteFile(newPhoto); Message = "Your avatar could not be saved. Try again."; return false; }
            // File-write recovery ends at the manifest commit. UI callbacks must never roll back saved bytes.
            string oldPhoto = selection.photoId;
            selection = next; DisposeTexture(ref photo); photo = candidate;
            if (AvatarCatalog.PhotoId(oldPhoto)) DeleteFile(Path.Combine(Folder, oldPhoto + ".png"));
            Message = SaveScopeLabel; return true;
        }

        /// <summary>Call only after the account service confirms this account's deletion request. Other scopes are untouched.</summary>
        public bool DeleteAccountData(string verifiedUserId)
        {
            string scope = AvatarCatalog.AccountScope(verifiedUserId); if (scope.Length == 0) return false;
            if (scope == owner) BindAccount(null);
            try { string path = Path.Combine(Root, scope); if (Directory.Exists(path)) Directory.Delete(path, true); return true; }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException) { return false; }
        }

        private static Texture2D ReadPhoto(string path)
        {
            Texture2D texture = null;
            try
            {
                byte[] bytes = AvatarFiles.ReadBounded(path, AvatarCatalog.MaximumInputBytes); if (!AvatarImageHeader.Allowed(bytes)) return null;
                texture = new Texture2D(2, 2, TextureFormat.RGB24, false) { name = "Private profile photo", wrapMode = TextureWrapMode.Clamp };
                if (texture.LoadImage(bytes, false)) return texture;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is ArgumentException) { }
            if (texture != null) Destroy(texture); return null;
        }
        private static void DeleteFile(string path)
        { try { if (!string.IsNullOrEmpty(path) && File.Exists(path)) File.Delete(path); } catch (IOException) { } catch (UnauthorizedAccessException) { } }
        private static void DisposeTexture(ref Texture2D texture) { if (texture != null) Destroy(texture); texture = null; }
        private static void CleanPending(string folder)
        { try { string path = Path.Combine(folder, "pending"); if (Directory.Exists(path)) Directory.Delete(path, true); } catch (IOException) { } catch (UnauthorizedAccessException) { } }
        private static void CleanOrphans(string folder, string selected)
        {
            try
            {
                if (!Directory.Exists(folder)) return;
                foreach (string path in Directory.EnumerateFiles(folder, "*.png"))
                { string id = Path.GetFileNameWithoutExtension(path); if (AvatarCatalog.PhotoId(id) && id != selected) DeleteFile(path); }
                foreach (string path in Directory.EnumerateFiles(folder, "selection.json.*.tmp")) DeleteFile(path);
            }
            catch (IOException) { } catch (UnauthorizedAccessException) { }
        }
        private void OnDisable() { CancelPhoto(); }
        private void OnDestroy() { CancelPhoto(false); DisposeTexture(ref photo); }
    }
}
