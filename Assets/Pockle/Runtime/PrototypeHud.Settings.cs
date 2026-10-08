using System;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    public sealed partial class PrototypeHud
    {
        private RectTransform comfortCard, walkingSettings, aboutCard;
        private Button soundButton, hapticsButton, motionButton;
        private Slider soundVolume;
        private Text volumeLabel, permissionStatus;
        private Action<float> volumeCallback;
        public const string UiBuild = "Home hub 01";

        private void BuildSettings()
        {
            comfortCard = Rect("Comfort settings", settingsRoot); Surface(comfortCard, Paper);
            Label("YOUR COMFORT", comfortCard, 11, MutedInk, FontStyle.Bold, TextAnchor.MiddleCenter,
                new Vector2(0, -12), new Vector2(270, 24));
            soundButton = CreateButton("", comfortCard, Vector2.zero, new Vector2(280, 48), Quiet,
                () => { SetSettings(!soundEnabled, hapticsEnabled, reducedMotionEnabled); soundCallback?.Invoke(soundEnabled); RefreshSettings(); }, out _);
            soundLabel = soundButton.GetComponentInChildren<Text>();
            volumeLabel = Label("", comfortCard, 12, MutedInk, FontStyle.Normal, TextAnchor.MiddleCenter,
                new Vector2(0, -106), new Vector2(270, 24));
            soundVolume = BuildVolumeSlider(comfortCard);
            soundVolume.onValueChanged.AddListener(value =>
            {
                PlayerPrefs.SetFloat("pockle.prototype.soundVolume", value);
                volumeCallback?.Invoke(value); RefreshSettings();
            });
            hapticsButton = CreateButton("", comfortCard, Vector2.zero, new Vector2(280, 48), Quiet,
                () => { SetSettings(soundEnabled, !hapticsEnabled, reducedMotionEnabled); hapticsCallback?.Invoke(hapticsEnabled); RefreshSettings(); }, out _);
            hapticsLabel = hapticsButton.GetComponentInChildren<Text>();
            motionButton = CreateButton("", comfortCard, Vector2.zero, new Vector2(280, 48), Quiet,
                () => { SetSettings(soundEnabled, hapticsEnabled, !reducedMotionEnabled); motionCallback?.Invoke(reducedMotionEnabled); RefreshSettings(); }, out _);
            motionLabel = motionButton.GetComponentInChildren<Text>();
            walkingSettings = Rect("Walking access", settingsRoot); Surface(walkingSettings, Paper);
            Label("WALKING ACCESS", walkingSettings, 11, MutedInk, FontStyle.Bold, TextAnchor.MiddleCenter,
                new Vector2(0, -12), new Vector2(270, 24));
            permissionStatus = Label("", walkingSettings, 13, MutedInk, FontStyle.Normal, TextAnchor.MiddleCenter,
                new Vector2(0, -44), new Vector2(290, 80));
            CreateButton("Enable walking", walkingSettings, Vector2.zero, new Vector2(280, 48), Peach,
                () => session?.EnableWalking(), out _);
            Label("If activity access is denied, allow Physical activity in your phone's app settings, then return here. Walking is counted while Pockle is open for now.",
                walkingSettings, 11, MutedInk, FontStyle.Normal, TextAnchor.UpperCenter,
                new Vector2(0, -194), new Vector2(290, 78));
            aboutCard = Rect("About and help", settingsRoot); Surface(aboutCard, Paper);
            Label("ABOUT POCKLE", aboutCard, 11, MutedInk, FontStyle.Bold, TextAnchor.MiddleCenter,
                new Vector2(0, -12), new Vector2(270, 24));
            Label("Version " + Application.version + " · " + UiBuild, aboutCard, 12, MutedInk, FontStyle.Normal,
                TextAnchor.MiddleCenter, new Vector2(0, -40), new Vector2(290, 28));
            CreateButton("Play & walking help", aboutCard, Vector2.zero, new Vector2(280, 48), Quiet,
                () => ShowDialog("A few little tips", "Drag Pip to lift. Use two fingers to stretch or squish. Drag the plate to turn it. A gentle phone shake jiggles it in Motion full. Walk 1,000 steps with Pockle open to earn a daily box."), out _);
            CreateButton("Your save & account", aboutCard, Vector2.zero, new Vector2(280, 48), Quiet,
                () => ShowDialog("Saved on this device", "Your shelf, walking progress, and profile are saved here. Accounts and cloud save are coming later. Purchases aren't available yet, and no charges can be made."), out _);
            SetSoundVolume(PlayerPrefs.GetFloat("pockle.prototype.soundVolume", 1));
        }
        private Slider BuildVolumeSlider(Transform parent)
        {
            var root = Rect("Sound volume", parent);
            var hit = root.gameObject.AddComponent<Image>(); hit.color = Color.clear;
            var track = Rect("Volume track", root); Stretch(track); track.offsetMin = new Vector2(12, 18); track.offsetMax = new Vector2(-12, -18); Surface(track, Quiet);
            var fillArea = Rect("Volume fill area", root); Stretch(fillArea); fillArea.offsetMin = new Vector2(12, 18); fillArea.offsetMax = new Vector2(-12, -18);
            var fill = Rect("Volume fill", fillArea); Stretch(fill); Surface(fill, Peach);
            var handleArea = Rect("Volume handle area", root); Stretch(handleArea); handleArea.offsetMin = new Vector2(12, 0); handleArea.offsetMax = new Vector2(-12, 0);
            var handle = Rect("Volume handle", handleArea); handle.sizeDelta = new Vector2(24, 24);
            handle.anchorMin = handle.anchorMax = new Vector2(0, .5f); handle.pivot = new Vector2(.5f, .5f);
            var image = Surface(handle, Ink); image.raycastTarget = true;
            var slider = root.gameObject.AddComponent<Slider>(); slider.minValue = 0; slider.maxValue = 1;
            slider.fillRect = fill; slider.handleRect = handle; slider.targetGraphic = image; slider.direction = Slider.Direction.LeftToRight;
            return slider;
        }
        public void SetSoundVolume(float value)
        {
            if (soundVolume == null) return;
            soundVolume.SetValueWithoutNotify(float.IsNaN(value) || float.IsInfinity(value) ? 1 : Mathf.Clamp01(value));
            RefreshSettings();
        }
        private void RefreshSettings()
        {
            if (soundVolume == null) return;
            volumeLabel.text = "Volume " + Mathf.RoundToInt(soundVolume.value * 100) + "%" + (soundEnabled ? "" : " · muted");
            soundVolume.interactable = soundEnabled;
            permissionStatus.text = session == null ? "Choose walking to request activity access." : session.WalkingStatus;
        }
        private float LayoutSettings(float width)
        {
            float w = Mathf.Min(width, 480);
            TopCentered(settingsRoot, Vector2.zero, new Vector2(width, 874));
            TopCentered(comfortCard, Vector2.zero, new Vector2(w, 314));
            TopCentered(soundButton.GetComponent<RectTransform>(), new Vector2(0, -44), new Vector2(w - 32, 48));
            TopCentered(soundVolume.GetComponent<RectTransform>(), new Vector2(0, -132), new Vector2(w - 40, 44));
            TopCentered(hapticsButton.GetComponent<RectTransform>(), new Vector2(0, -186), new Vector2(w - 32, 48));
            TopCentered(motionButton.GetComponent<RectTransform>(), new Vector2(0, -246), new Vector2(w - 32, 48));
            TopCentered(walkingSettings, new Vector2(0, -332), new Vector2(w, 280));
            TopCentered(permissionStatus.rectTransform, new Vector2(0, -44), new Vector2(w - 32, 80));
            TopCentered(walkingSettings.GetComponentsInChildren<Button>()[0].GetComponent<RectTransform>(), new Vector2(0, -132), new Vector2(w - 32, 48));
            var walkingHelp = walkingSettings.GetComponentsInChildren<Text>();
            walkingHelp[walkingHelp.Length - 1].rectTransform.sizeDelta = new Vector2(w - 32, 78);
            TopCentered(aboutCard, new Vector2(0, -630), new Vector2(w, 226));
            var aboutButtons = aboutCard.GetComponentsInChildren<Button>();
            for (int i = 0; i < aboutButtons.Length; i++)
                TopCentered(aboutButtons[i].GetComponent<RectTransform>(), new Vector2(0, -82 - i * 60), new Vector2(w - 32, 48));
            return 874;
        }
    }
}
