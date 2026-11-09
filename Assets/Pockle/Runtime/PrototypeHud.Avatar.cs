using System;
using Pockle.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    public sealed partial class PrototypeHud
    {
        private ProfileAvatarService avatarService;
        private Func<string> avatarVerifiedAccount;
        private RectTransform avatarOverlay, avatarEditCard, avatarEditContent, avatarSignupGrid, avatarCropCircle;
        private Text avatarEditTitle, avatarEditHint, avatarEditStatus;
        private Button avatarEditSave, avatarEditCancel, avatarSignupPhoto;
        private Button[] avatarSignupChoices;
        private RawImage avatarCropImage;
        private Slider avatarCropZoom;
        private Action avatarRegistrationDone;
        private string avatarRegistrationPreset;
        private bool avatarCropMode, avatarUiDisposed;
        private float avatarCenterX = .5f, avatarCenterY = .5f;
        public bool AvatarOverlayVisible => avatarOverlay != null && avatarOverlay.gameObject.activeSelf;
        public Texture ProfileAvatarTexture => avatarService != null && avatarService.CanEdit ? avatarService.Texture :
            Resources.Load<Texture2D>(AvatarCatalog.Resource(AvatarCatalog.DefaultId));

        /// <summary>Only the account runtime may supply the verified auth UUID. No email/name-based binding.</summary>
        public void SetAvatarService(ProfileAvatarService service, Func<string> verifiedAccountId)
        {
            if (avatarService != null) avatarService.Changed -= AvatarChanged;
            CloseAvatarEditor(false);
            avatarService = service; avatarVerifiedAccount = verifiedAccountId; avatarUiDisposed = false;
            if (avatarService != null) avatarService.Changed += AvatarChanged;
            SyncAvatarAccount(); AvatarChanged();
        }
        public void SyncAvatarAccount()
        {
            if (avatarUiDisposed || avatarService == null) return;
            string verified = AvatarCatalog.AccountScope(avatarVerifiedAccount?.Invoke());
            if (avatarService.AccountId == verified) return;
            CloseAvatarEditor(false); // Discard a crop and its continuation before switching accounts.
            avatarService.BindAccount(verified);
        }
        private void AvatarChanged()
        {
            if (avatarUiDisposed) return;
            if (avatarService != null && avatarService.Stage == AvatarPhotoStage.Cropping && !avatarCropMode)
            {
                BuildAvatarEditor(); avatarCropMode = true; avatarCenterX = avatarCenterY = .5f;
                avatarCropZoom.SetValueWithoutNotify(1); avatarCropImage.texture = avatarService.PendingPhoto;
                avatarOverlay.gameObject.SetActive(true); avatarOverlay.SetAsLastSibling(); RefreshAvatarCrop();
            }
            if (avatarCropMode && (avatarService == null || avatarService.Stage != AvatarPhotoStage.Cropping))
            {
                avatarCropMode = false;
                if (avatarCropImage != null) avatarCropImage.texture = null;
                if (avatarRegistrationDone == null && avatarOverlay != null) avatarOverlay.gameObject.SetActive(false);
            }
            RefreshProfile(); RefreshAvatarButton(); RefreshAvatarChoices(avatarSignupChoices);
            RefreshAvatarEditor();
        }
        private static string AvatarLabel(string id)
        { if (id == "pip-peach") return "Peach Pip"; if (id == "pip-moon") return "Moon Pip"; return char.ToUpperInvariant(id[0]) + id.Substring(1); }
        private RawImage AvatarImage(Transform parent, Texture texture)
        {
            var rect = Rect("Avatar artwork", parent); Stretch(rect);
            var raw = rect.gameObject.AddComponent<RawImage>(); raw.texture = texture; raw.raycastTarget = false; return raw;
        }
        private Button[] BuildAvatarChoices(RectTransform parent)
        {
            var choices = new Button[AvatarCatalog.Ids.Count];
            for (int i = 0; i < choices.Length; i++)
            {
                string id = AvatarCatalog.Ids[i];
                choices[i] = CreateButton(AvatarLabel(id), parent, Vector2.zero, new Vector2(72, 72), Quiet,
                    () => SelectProfileAvatar(id), out var face, 14);
                choices[i].gameObject.name = "Avatar · " + id;
                choices[i].GetComponentInChildren<Text>().enabled = false;
                choices[i].gameObject.AddComponent<Mask>().showMaskGraphic = true;
                AvatarImage(choices[i].transform, Resources.Load<Texture2D>(AvatarCatalog.Resource(id)));
                SetRadius(face, 36);
            }
            return choices;
        }
        private void SelectProfileAvatar(string id)
        {
            SyncAvatarAccount(); if (avatarService == null || !avatarService.CanEdit || avatarService.Stage != AvatarPhotoStage.Idle) return;
            if (avatarRegistrationDone != null) { avatarRegistrationPreset = id; RefreshAvatarChoices(avatarSignupChoices); return; }
            avatarService.SelectPreset(id);
        }
        private void RefreshAvatarChoices(Button[] choices)
        {
            if (choices == null) return;
            bool editable = avatarService != null && avatarService.CanEdit && avatarService.Stage == AvatarPhotoStage.Idle;
            for (int i = 0; i < choices.Length; i++)
            {
                choices[i].interactable = editable;
                bool drafting = choices == avatarSignupChoices && avatarRegistrationPreset != null;
                bool selected = avatarService != null && avatarService.CanEdit && (drafting ? avatarRegistrationPreset == AvatarCatalog.Ids[i] : !avatarService.HasPhoto && avatarService.PresetId == AvatarCatalog.Ids[i]);
                choices[i].GetComponent<Image>().color = selected ? Ink : Quiet;
                var image = choices[i].GetComponentInChildren<RawImage>().rectTransform;
                image.offsetMin = Vector2.one * (selected ? 4 : 2); image.offsetMax = -image.offsetMin;
            }
        }
        private float LayoutAvatarChoices(Button[] choices, float width)
        {
            const float gap = 12; float size = Mathf.Min(94, (width - gap * 3) / 4), left = (width - size * 4 - gap * 3) / 2;
            for (int i = 0; i < choices.Length; i++)
            {
                LeftLabel(choices[i].GetComponent<RectTransform>(), new Vector2(left + i % 4 * (size + gap), -i / 4 * (size + gap)), new Vector2(size, size));
                SetRadius(choices[i].GetComponent<Image>(), size / 2);
            }
            return size * 3 + gap * 2;
        }
        private void BeginAvatarPhoto()
        { SyncAvatarAccount(); if (avatarService != null) avatarService.PickPhoto(); }

        /// <summary>Optional step only after username claim + legacy backup succeeded. Never bypasses auth gates.</summary>
        private bool OfferRegistrationAvatar(Action done)
        {
            SyncAvatarAccount(); if (avatarService == null || !avatarService.CanEdit) return false;
            BuildAvatarEditor(); avatarRegistrationDone = done; avatarRegistrationPreset = null; avatarCropMode = false;
            avatarOverlay.gameObject.SetActive(true); avatarOverlay.SetAsLastSibling(); RefreshAvatarEditor(); return true;
        }
        private void BuildAvatarEditor()
        {
            if (avatarOverlay != null) return;
            avatarOverlay = Rect("Private avatar editor", canvas.transform); Stretch(avatarOverlay);
            var backdrop = avatarOverlay.gameObject.AddComponent<Image>(); backdrop.color = PockleTheme.Backdrop;
            avatarEditCard = Rect("Avatar editor card", avatarOverlay); Surface(avatarEditCard, new Color(1f, .957f, .914f), 30);
            avatarEditCard.anchorMin = avatarEditCard.anchorMax = avatarEditCard.pivot = new Vector2(.5f, .5f);
            var viewport = Rect("Avatar editor viewport", avatarEditCard); Stretch(viewport); viewport.gameObject.AddComponent<RectMask2D>();
            avatarEditContent = Rect("Avatar editor scroll content", viewport);
            var scroll = avatarEditCard.gameObject.AddComponent<ScrollRect>(); scroll.viewport = viewport; scroll.content = avatarEditContent;
            scroll.horizontal = false; scroll.vertical = true; scroll.movementType = ScrollRect.MovementType.Clamped; scroll.scrollSensitivity = 24;
            avatarEditTitle = Label("Make it feel like you", avatarEditContent, 25, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            avatarEditHint = Label("Pick a Pockle or choose a photo.", avatarEditContent, 14, MutedInk, FontStyle.Normal, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            avatarSignupGrid = Rect("Registration avatar choices", avatarEditContent); avatarSignupChoices = BuildAvatarChoices(avatarSignupGrid);
            avatarSignupPhoto = CreateButton("Choose a photo", avatarEditContent, Vector2.zero, new Vector2(300, 48), Quiet, BeginAvatarPhoto, out _, 17);
            avatarCropCircle = Rect("Circular photo crop", avatarEditContent); Surface(avatarCropCircle, Quiet, 128);
            avatarCropCircle.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            avatarCropImage = AvatarImage(avatarCropCircle, null); avatarCropImage.raycastTarget = true;
            avatarCropImage.gameObject.AddComponent<AvatarCropDrag>().Moved = PanAvatarCrop;
            avatarCropZoom = BuildVolumeSlider(avatarEditContent); avatarCropZoom.name = "Avatar crop zoom";
            avatarCropZoom.minValue = 1; avatarCropZoom.maxValue = 4; avatarCropZoom.SetValueWithoutNotify(1);
            avatarCropZoom.onValueChanged.AddListener(_ => RefreshAvatarCrop());
            avatarEditStatus = Label("Saved on this device", avatarEditContent, 14, MutedInk, FontStyle.Normal, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            avatarEditSave = CreateButton("Save photo", avatarEditContent, Vector2.zero, new Vector2(140, 50), Primary, SaveAvatarEditor, out _, 17);
            avatarEditCancel = CreateButton("Cancel", avatarEditContent, Vector2.zero, new Vector2(140, 50), Quiet, CancelAvatarEditor, out _, 17);
            avatarOverlay.gameObject.SetActive(false);
        }
        private void PanAvatarCrop(Vector2 delta)
        {
            var photo = avatarService != null ? avatarService.PendingPhoto : null;
            if (photo == null || !AvatarCrop.TryCreate(photo.width, photo.height, avatarCenterX, avatarCenterY, avatarCropZoom.value, out var crop)) return;
            avatarCenterX -= delta.x * (float)crop.Size / photo.width; avatarCenterY -= delta.y * (float)crop.Size / photo.height;
            RefreshAvatarCrop();
        }
        private void RefreshAvatarCrop()
        {
            var photo = avatarService != null ? avatarService.PendingPhoto : null;
            if (photo == null || avatarCropImage == null || !AvatarCrop.TryCreate(photo.width, photo.height, avatarCenterX, avatarCenterY, avatarCropZoom.value, out var crop)) return;
            avatarCenterX = (float)((crop.X + crop.Size / 2) / photo.width); avatarCenterY = (float)((crop.Y + crop.Size / 2) / photo.height);
            avatarCropImage.uvRect = new Rect((float)crop.X / photo.width, (float)crop.Y / photo.height, (float)crop.Size / photo.width, (float)crop.Size / photo.height);
        }
        private void RefreshAvatarEditor()
        {
            if (!AvatarOverlayVisible) return;
            avatarEditTitle.text = avatarCropMode ? "Your little circle" : "Make it feel like you";
            avatarEditHint.text = avatarCropMode ? "Drag to move. Slide to zoom." : "Pick a little face. Or bring your own.";
            avatarSignupGrid.gameObject.SetActive(!avatarCropMode); avatarSignupPhoto.gameObject.SetActive(!avatarCropMode);
            avatarCropCircle.gameObject.SetActive(avatarCropMode); avatarCropZoom.gameObject.SetActive(avatarCropMode);
            avatarEditSave.GetComponentInChildren<Text>().text = avatarCropMode ? "Save photo" : "Continue";
            avatarEditCancel.GetComponentInChildren<Text>().text = avatarCropMode ? "Cancel" : "Skip for now";
            avatarEditStatus.text = avatarService != null && !string.IsNullOrEmpty(avatarService.Message) ? avatarService.Message : "Saved on this device · private";
            bool picking = avatarService != null && avatarService.Stage == AvatarPhotoStage.Picking;
            avatarSignupPhoto.interactable = !picking; avatarEditSave.interactable = !picking;
            RefreshAvatarChoices(avatarSignupChoices); LayoutAvatarOverlay();
        }
        private void SaveAvatarEditor()
        {
            SyncAvatarAccount(); if (!AvatarOverlayVisible || avatarService == null || !avatarService.CanEdit) return;
            if (avatarCropMode)
            {
                if (avatarService.CommitPhotoCrop(avatarCenterX, avatarCenterY, avatarCropZoom.value))
                { avatarRegistrationPreset = null; RefreshAvatarChoices(avatarSignupChoices); }
                return;
            }
            if (avatarRegistrationPreset != null && !avatarService.SelectPreset(avatarRegistrationPreset)) return;
            CloseAvatarEditor(true);
        }
        private void CancelAvatarEditor()
        {
            if (avatarCropMode || avatarService != null && avatarService.Stage == AvatarPhotoStage.Picking)
            { avatarService.CancelPhoto(); return; }
            CloseAvatarEditor(true);
        }
        private void CloseAvatarEditor(bool continueRegistration)
        {
            var done = continueRegistration ? avatarRegistrationDone : null; avatarRegistrationDone = null; avatarRegistrationPreset = null;
            avatarCropMode = false;
            if (avatarOverlay != null) avatarOverlay.gameObject.SetActive(false);
            if (avatarCropImage != null) avatarCropImage.texture = null;
            if (avatarService != null && avatarService.Stage != AvatarPhotoStage.Idle) avatarService.CancelPhoto();
            done?.Invoke();
        }
        private bool CloseAvatarOverlayForBack()
        {
            if (AvatarOverlayVisible) { CancelAvatarEditor(); return true; }
            if (avatarService != null && avatarService.Stage == AvatarPhotoStage.Picking) { avatarService.CancelPhoto(); return true; }
            return false;
        }
        private void DisposeAvatarUi()
        {
            avatarUiDisposed = true;
            if (avatarService != null) avatarService.Changed -= AvatarChanged;
            CloseAvatarEditor(false); avatarVerifiedAccount = null; avatarService = null;
        }
        private void HideAccountAvatar()
        { CloseAvatarEditor(false); if (avatarService != null) avatarService.BindAccount(null); }
        private bool DeleteAccountAvatar(string verifiedId)
        { CloseAvatarEditor(false); return avatarService == null || avatarService.DeleteAccountData(verifiedId); }
        private void LayoutAvatarOverlay()
        {
            if (!AvatarOverlayVisible || safeRoot == null) return;
            float width = Mathf.Min(470, safeRoot.rect.width - 24), inner = width - 40;
            float picture = Mathf.Min(272, inner), grid = LayoutAvatarChoices(avatarSignupChoices, inner);
            float body = avatarCropMode ? picture + 58 : grid + 64, height = 196 + body;
            avatarEditCard.sizeDelta = new Vector2(width, Mathf.Min(height, Mathf.Max(160, safeRoot.rect.height - 24)));
            LeftLabel(avatarEditContent, Vector2.zero, new Vector2(width, height));
            // Center in the safe area, including asymmetric camera/sensor insets.
            avatarEditCard.position = safeRoot.TransformPoint(safeRoot.rect.center);
            LeftLabel(avatarEditTitle.rectTransform, new Vector2(20, -20), new Vector2(inner, 36));
            LeftLabel(avatarEditHint.rectTransform, new Vector2(20, -58), new Vector2(inner, 28));
            LeftLabel(avatarSignupGrid, new Vector2(20, -96), new Vector2(inner, grid));
            LeftLabel(avatarSignupPhoto.GetComponent<RectTransform>(), new Vector2(20, -106 - grid), new Vector2(inner, 48));
            LeftLabel(avatarCropCircle, new Vector2((width - picture) / 2, -96), new Vector2(picture, picture)); SetRadius(avatarCropCircle.GetComponent<Image>(), picture / 2);
            LeftLabel(avatarCropZoom.GetComponent<RectTransform>(), new Vector2(20, -104 - picture), new Vector2(inner, 44));
            LeftLabel(avatarEditStatus.rectTransform, new Vector2(20, -100 - body), new Vector2(inner, 30));
            LeftLabel(avatarEditCancel.GetComponent<RectTransform>(), new Vector2(20, -height + 66), new Vector2((inner - 12) / 2, 48));
            LeftLabel(avatarEditSave.GetComponent<RectTransform>(), new Vector2(26 + inner / 2, -height + 66), new Vector2((inner - 12) / 2, 48));
        }
    }

    /// <summary>Only the crop image receives the drag; never hands an in-progress gesture to the shelf/toy.</summary>
    public sealed class AvatarCropDrag : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        public Action<Vector2> Moved;
        private Vector2 previous;
        public void OnBeginDrag(PointerEventData e) => RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)transform, e.position, e.pressEventCamera, out previous);
        public void OnDrag(PointerEventData e)
        {
            var rect = (RectTransform)transform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, e.position, e.pressEventCamera, out var now)) return;
            if (rect.rect.width > 0 && rect.rect.height > 0) Moved?.Invoke(new Vector2((now.x - previous.x) / rect.rect.width, (now.y - previous.y) / rect.rect.height));
            previous = now;
        }
    }
}
