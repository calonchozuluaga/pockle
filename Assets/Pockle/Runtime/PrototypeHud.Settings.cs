using System;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// v2 Settings (UX-032): grouped Feel / Walking / About panels with switches, a volume slider, and a
    /// Full / Calm motion choice. Reached from the gear on You.
    /// </summary>
    public sealed partial class PrototypeHud
    {
        private RectTransform feelHeading, feelGroup, walkingHeading, walkingSettings, aboutHeading, aboutCard;
        private Button soundSwitch, hapticsSwitch, motionFull, motionCalm, walkingButton;
        private Image soundTrack, hapticsTrack, motionFullFace, motionCalmFace;
        private RectTransform soundKnob, hapticsKnob, motionGroup;
        private Slider soundVolume;
        private Text volumeLabel, permissionStatus, walkingHelp;
        private Action<float> volumeCallback;
        private readonly RectTransform[] feelRows = new RectTransform[4];
        public const string UiBuild = "v2 screens 01";

        private void BuildSettings()
        {
            feelHeading = GroupLabel("Feel", settingsRoot);
            feelGroup = Rect("Comfort settings", settingsRoot); Surface(feelGroup, Quiet, 24);
            feelRows[0] = SwitchRow("Sound", feelGroup, () => { SetSettings(!soundEnabled, hapticsEnabled, reducedMotionEnabled); soundCallback?.Invoke(soundEnabled); },
                out soundSwitch, out soundTrack, out soundKnob);
            feelRows[1] = Rect("Volume row", feelGroup);
            volumeLabel = Label("Volume", feelRows[1], PockleTheme.CaptionSize + 1, MutedInk, FontStyle.Normal, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(64, 24));
            soundVolume = BuildVolumeSlider(feelRows[1]);
            soundVolume.onValueChanged.AddListener(value =>
            {
                PlayerPrefs.SetFloat("pockle.prototype.soundVolume", value);
                volumeCallback?.Invoke(value); RefreshSettings();
            });
            feelRows[2] = SwitchRow("Vibration", feelGroup, () => { SetSettings(soundEnabled, !hapticsEnabled, reducedMotionEnabled); hapticsCallback?.Invoke(hapticsEnabled); },
                out hapticsSwitch, out hapticsTrack, out hapticsKnob);
            feelRows[3] = Rect("Motion row", feelGroup);
            var motionTitle = Label("Motion", feelRows[3], 17, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(200, 24));
            LeftLabel(motionTitle.rectTransform, new Vector2(0, -14), new Vector2(200, 24));
            var motionHelp = Label("Calm keeps squishing but skips bounces and shakes.", feelRows[3], PockleTheme.CaptionSize, MutedInk, FontStyle.Normal,
                TextAnchor.UpperLeft, Vector2.zero, new Vector2(300, 38));
            LeftLabel(motionHelp.rectTransform, new Vector2(0, -40), new Vector2(300, 38));
            motionGroup = Rect("Motion choice", feelRows[3]); Surface(motionGroup, PockleTheme.FillDeep, 22);
            motionFull = CreateButton("Full", motionGroup, Vector2.zero, new Vector2(100, 40), Paper, () => SetMotion(false), out motionFullFace, 15);
            motionCalm = CreateButton("Calm", motionGroup, Vector2.zero, new Vector2(100, 40), Paper, () => SetMotion(true), out motionCalmFace, 15);

            walkingHeading = GroupLabel("Walking", settingsRoot);
            walkingSettings = Rect("Walking access", settingsRoot); Surface(walkingSettings, Quiet, 24);
            permissionStatus = Label("", walkingSettings, PockleTheme.BodySize, Ink, FontStyle.Bold, TextAnchor.UpperLeft, Vector2.zero, new Vector2(280, 44));
            walkingHelp = Label("Steps count while Pockle is open. Keep it open on your walk for now. If activity access is denied, allow Physical activity in your phone's app settings.",
                walkingSettings, PockleTheme.CaptionSize, MutedInk, FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, new Vector2(300, 60));
            walkingButton = CreateButton("Enable walking", walkingSettings, Vector2.zero, new Vector2(280, 48), PockleTheme.Plum, () => session?.EnableWalking(), out _);

            aboutHeading = GroupLabel("About", settingsRoot);
            aboutCard = Rect("About and help", settingsRoot); Surface(aboutCard, Quiet, 24);
            LinkRow("How to play", aboutCard, () => ShowDialog("A few little tips",
                "Press Pip to squish. Drag up to lift. Use two fingers to stretch. Drag the plate to turn it. A gentle phone shake jiggles Pip in Full motion. Walk 1,000 steps with Pockle open to earn a daily box."));
            LinkRow("Your save and account", aboutCard, () => ShowDialog("Saved on this device",
                "Your shelf, walking progress, and profile are saved here. Accounts and cloud save are coming later. Purchases aren't available yet, and no charges can be made."));
            var version = Rect("Version row", aboutCard);
            var versionTitle = Label("Version", version, 17, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero); Stretch(versionTitle.rectTransform);
            var versionValue = Label(Application.version + " · " + UiBuild, version, PockleTheme.CaptionSize + 1, MutedInk, FontStyle.Normal,
                TextAnchor.MiddleRight, Vector2.zero, Vector2.zero);
            Stretch(versionValue.rectTransform);
            SetSoundVolume(PlayerPrefs.GetFloat("pockle.prototype.soundVolume", 1));
        }

        private void SetMotion(bool calm)
        {
            if (calm == reducedMotionEnabled) return;
            SetSettings(soundEnabled, hapticsEnabled, calm); motionCallback?.Invoke(reducedMotionEnabled);
        }

        private RectTransform GroupLabel(string title, Transform parent)
        {
            return Label(title, parent, PockleTheme.CaptionSize + 1, MutedInk, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(300, 22)).rectTransform;
        }

        /// <summary>A 60px row with a title and an on/off switch. The switch is the button; the title names it.</summary>
        private RectTransform SwitchRow(string title, Transform parent, Action toggle, out Button button, out Image track, out RectTransform knob)
        {
            var row = Rect(title + " row", parent);
            var label = Label(title, row, 17, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            Stretch(label.rectTransform); label.rectTransform.offsetMax = new Vector2(-64, 0);
            button = CreateButton(title, row, Vector2.zero, new Vector2(52, 32), PockleTheme.FillDeep, toggle, out track, 12);
            button.gameObject.name = title + " switch";
            button.GetComponentInChildren<Text>().enabled = false;
            var rect = button.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(1, .5f); rect.anchoredPosition = Vector2.zero;
            knob = Rect("Knob", rect); Surface(knob, Paper, 13);
            knob.anchorMin = knob.anchorMax = knob.pivot = new Vector2(0, .5f); knob.sizeDelta = new Vector2(26, 26);
            Divider(row);
            return row;
        }

        private static void StyleSwitch(Image track, RectTransform knob, bool on)
        {
            track.color = on ? PockleTheme.Plum : PockleTheme.FillDeep;
            knob.anchoredPosition = new Vector2(on ? 23 : 3, 0);
        }

        private void LinkRow(string title, Transform parent, Action click)
        {
            var button = CreateButton(title, parent, Vector2.zero, new Vector2(300, 56), Color.clear, click, out _, 17, false);
            var label = button.GetComponentInChildren<Text>(); label.alignment = TextAnchor.MiddleLeft;
            label.rectTransform.offsetMin = new Vector2(0, 2); label.rectTransform.offsetMax = new Vector2(-30, -2);
            var next = AddIcon(button.transform, "Next", 18, MutedInk);
            next.anchorMin = next.anchorMax = next.pivot = new Vector2(1, .5f); next.anchoredPosition = Vector2.zero;
            Divider(button.GetComponent<RectTransform>());
        }

        private static void Divider(RectTransform row)
        {
            var line = Rect("Divider", row);
            line.anchorMin = new Vector2(0, 0); line.anchorMax = new Vector2(1, 0); line.pivot = new Vector2(.5f, 0);
            line.offsetMin = Vector2.zero; line.offsetMax = new Vector2(0, 1);
            var image = line.gameObject.AddComponent<Image>(); image.color = PockleTheme.FillDeep; image.raycastTarget = false;
        }

        private Slider BuildVolumeSlider(Transform parent)
        {
            var root = Rect("Sound volume", parent);
            var hit = root.gameObject.AddComponent<Image>(); hit.color = Color.clear;
            var track = Rect("Volume track", root); Stretch(track); track.offsetMin = new Vector2(12, 19); track.offsetMax = new Vector2(-12, -19); Surface(track, PockleTheme.FillDeep, 3);
            var fillArea = Rect("Volume fill area", root); Stretch(fillArea); fillArea.offsetMin = new Vector2(12, 19); fillArea.offsetMax = new Vector2(-12, -19);
            var fill = Rect("Volume fill", fillArea); Stretch(fill); Surface(fill, Ink, 3);
            var handleArea = Rect("Volume handle area", root); Stretch(handleArea); handleArea.offsetMin = new Vector2(12, 0); handleArea.offsetMax = new Vector2(-12, 0);
            var handle = Rect("Volume handle", handleArea); handle.sizeDelta = new Vector2(24, 24);
            handle.anchorMin = handle.anchorMax = new Vector2(0, .5f); handle.pivot = new Vector2(.5f, .5f);
            var image = Surface(handle, Ink, 12); image.raycastTarget = true;
            var slider = root.gameObject.AddComponent<Slider>(); slider.minValue = 0; slider.maxValue = 1;
            slider.fillRect = fill; slider.handleRect = handle; slider.targetGraphic = image; slider.direction = Slider.Direction.LeftToRight;
            Divider(parent as RectTransform);
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
            StyleSwitch(soundTrack, soundKnob, soundEnabled);
            StyleSwitch(hapticsTrack, hapticsKnob, hapticsEnabled);
            motionFullFace.color = reducedMotionEnabled ? new Color(1, 1, 1, 0) : Paper;
            motionCalmFace.color = reducedMotionEnabled ? Paper : new Color(1, 1, 1, 0);
            volumeLabel.text = soundEnabled ? "Volume" : "Muted";
            soundVolume.interactable = soundEnabled;
            permissionStatus.text = session == null ? "Choose walking to request activity access." : session.WalkingStatus;
        }

        private float LayoutSettings(float width)
        {
            float w = Mathf.Min(width, 480);
            float left = (width - w) / 2, inner = w - 32;
            float y = 4;
            LeftLabel(feelHeading, new Vector2(left + 4, -y), new Vector2(w, 22)); y += 30;
            float[] heights = { 60, 56, 60, 108 };
            float rowY = 0;
            for (int i = 0; i < feelRows.Length; i++) { LeftLabel(feelRows[i], new Vector2(16, -rowY), new Vector2(inner, heights[i])); rowY += heights[i]; }
            LeftLabel(volumeLabel.rectTransform, new Vector2(0, -16), new Vector2(64, 24));
            LeftLabel(soundVolume.GetComponent<RectTransform>(), new Vector2(70, -6), new Vector2(inner - 70, 44));
            LeftLabel(motionGroup, new Vector2(0, -60), new Vector2(inner, 48));
            float half = (inner - 12) / 2;
            LeftLabel(motionFullFace.rectTransform, new Vector2(4, -4), new Vector2(half, 40));
            LeftLabel(motionCalmFace.rectTransform, new Vector2(8 + half, -4), new Vector2(half, 40));
            rowY += 12;
            LeftLabel(feelGroup, new Vector2(left, -y), new Vector2(w, rowY)); y += rowY + 22;

            LeftLabel(walkingHeading, new Vector2(left + 4, -y), new Vector2(w, 22)); y += 30;
            LeftLabel(permissionStatus.rectTransform, new Vector2(16, -14), new Vector2(inner, 44));
            LeftLabel(walkingHelp.rectTransform, new Vector2(16, -60), new Vector2(inner, 56));
            LeftLabel(walkingButton.GetComponent<RectTransform>(), new Vector2(16, -122), new Vector2(inner, 48));
            LeftLabel(walkingSettings, new Vector2(left, -y), new Vector2(w, 186)); y += 186 + 22;

            LeftLabel(aboutHeading, new Vector2(left + 4, -y), new Vector2(w, 22)); y += 30;
            for (int i = 0; i < aboutCard.childCount; i++)
                LeftLabel((RectTransform)aboutCard.GetChild(i), new Vector2(16, -i * 56), new Vector2(inner, 56));
            float about = aboutCard.childCount * 56;
            LeftLabel(aboutCard, new Vector2(left, -y), new Vector2(w, about)); y += about;
            TopCentered(settingsRoot, Vector2.zero, new Vector2(width, y));
            return y;
        }
    }
}
