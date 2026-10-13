using System.Collections.Generic;
using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// Toy personality page (owner-approved canvas "Toy personality", October 12): the toy poses on its colour field,
    /// and a sheet lists its nature, how it reacts, and the shelf friends it has met. Full-bleed: the page draws its own
    /// gutters. All content is authored (<see cref="PersonalityBook"/>); nothing is generated at runtime.
    /// </summary>
    public sealed partial class PrototypeHud
    {
        private const float PersonalitySheetTop = 330f;
        private string personalityId = PersonalityBook.Peach;
        private RectTransform personalityToy, personalityShadow, personalitySheet, personalityChips, personalityTable, personalityFriendsRow;
        private RawImage personalityPicture;
        private PersonalityMotion personalityMotion;
        private Button personalityVoice, personalityPlay;
        private Text personalityName, personalityLine, personalityVoiceLabel;
        private RectTransform personalityReactsHeading, personalityFriendsHeading;
        private readonly List<RectTransform> personalitySparkles = new List<RectTransform>();
        private readonly List<Text> chipTexts = new List<Text>();
        private readonly List<Image> chipFaces = new List<Image>();
        private readonly Text[] reactionWhen = new Text[4], reactionDoes = new Text[4];
        private readonly RectTransform[] reactionRows = new RectTransform[4];
        private readonly Image[] friendFaces = new Image[3];
        private readonly RawImage[] friendPictures = new RawImage[3];
        private readonly CharacterSilhouetteGraphic[] friendOutlines = new CharacterSilhouetteGraphic[3];
        private readonly Text[] friendLabels = new Text[3];
        private AudioSource voiceSource;

        private void BuildPersonality()
        {
            personalityVoice = CreateButton("Voice", personalityRoot, Vector2.zero, new Vector2(140, 44), new Color(1, 1, 1, .6f), PlayVoice, out _, 14);
            personalityVoice.name = "Personality voice";
            personalityVoiceLabel = personalityVoice.GetComponentInChildren<Text>();

            personalityShadow = Rect("Contact shadow", personalityRoot); Surface(personalityShadow, new Color(.59f, .39f, .08f, .18f), 15);
            personalityToy = Rect("Personality toy", personalityRoot);
            var motion = Rect("Personality motion", personalityToy); Stretch(motion); motion.pivot = new Vector2(.5f, SceneBase);
            personalityMotion = motion.gameObject.AddComponent<PersonalityMotion>();
            personalityMotion.Configure(PersonalityMotion.Kind.Toy, PersonalityMotionStyle.Show);
            var picture = Rect("Portrait", motion); Stretch(picture);
            personalityPicture = picture.gameObject.AddComponent<RawImage>(); personalityPicture.raycastTarget = false;
            personalitySparkles.Add(Sparkle(personalityRoot, Color.white, 0f));
            personalitySparkles.Add(Sparkle(personalityRoot, new Color32(0xE0, 0xA9, 0x2E, 0xFF), .4f));

            personalitySheet = Rect("Personality sheet", personalityRoot); Surface(personalitySheet, Paper, 32);
            personalityName = Label("", personalitySheet, 30, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(300, 36));
            personalityLine = Label("", personalitySheet, PockleTheme.CaptionSize + 1, MutedInk, FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, new Vector2(300, 40));
            personalityChips = Rect("Traits", personalitySheet);
            for (int i = 0; i < 4; i++)
            {
                var chip = Rect("Trait", personalityChips); chipFaces.Add(Surface(chip, Quiet, 15));
                var text = Label("", chip, PockleTheme.SmallSize, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
                Stretch(text.rectTransform); text.resizeTextForBestFit = false; text.horizontalOverflow = HorizontalWrapMode.Overflow;
                chipTexts.Add(text);
            }
            personalityReactsHeading = Label("How it reacts", personalitySheet, 18, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(300, 26)).rectTransform;
            personalityTable = Rect("Reactions", personalitySheet); Surface(personalityTable, Quiet, 20);
            for (int i = 0; i < reactionRows.Length; i++)
            {
                reactionRows[i] = Rect("Reaction", personalityTable);
                reactionWhen[i] = Label("", reactionRows[i], PockleTheme.CaptionSize, MutedInk, FontStyle.Normal, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
                Stretch(reactionWhen[i].rectTransform);
                reactionDoes[i] = Label("", reactionRows[i], PockleTheme.CaptionSize + 1, Ink, FontStyle.Bold, TextAnchor.MiddleRight, Vector2.zero, Vector2.zero);
                Stretch(reactionDoes[i].rectTransform);
                if (i < reactionRows.Length - 1) Divider(reactionRows[i]);
            }
            personalityFriendsHeading = Label("Shelf friends", personalitySheet, 18, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(300, 26)).rectTransform;
            personalityFriendsRow = Rect("Shelf friends", personalitySheet);
            for (int i = 0; i < friendFaces.Length; i++)
            {
                var tile = Rect("Shelf friend", personalityFriendsRow); friendFaces[i] = Surface(tile, PockleTheme.Mystery, 18);
                var art = Rect("Friend", tile);
                friendPictures[i] = art.gameObject.AddComponent<RawImage>(); friendPictures[i].raycastTarget = false;
                var outline = Rect("Not found yet", tile);
                friendOutlines[i] = outline.gameObject.AddComponent<CharacterSilhouetteGraphic>(); friendOutlines[i].color = PockleTheme.PlumSoft;
                friendOutlines[i].Configure("pip");
                friendLabels[i] = Label("", tile, 12, Ink, FontStyle.Bold, TextAnchor.LowerCenter, Vector2.zero, Vector2.zero);
            }
            personalityPlay = CreateButton("Play", personalitySheet, Vector2.zero, new Vector2(300, 52), PockleTheme.Plum, () => ShowToy(personalityId), out _);
        }

        private void OpenPersonality(string id)
        {
            if (!PersonalityBook.TryGet(id, out _) || session == null || session.GetOwnedCount(id) <= 0) return;
            personalityId = id;
            Navigate(AppPage.Personality);
            RefreshPersonality();
        }

        private void RefreshPersonality()
        {
            if (personalityName == null || !PersonalityBook.TryGet(personalityId, out var personality)) return;
            bool legacy = PipVariants.TryFromCollectibleId(personalityId, out var variant);
            string name = legacy ? ToyName(variant) : personalityId;
            personalityName.text = name;
            personalityLine.text = (legacy ? SeriesLine(variant) : "Pip") + ". " + personality.Nature + ", " + personality.Flavor + ".";
            personalityVoiceLabel.text = personality.Voice;
            personalityVoice.name = "Voice · " + personality.Voice;
            personalityPicture.texture = legacy ? previews[(int)variant] : null;
            personalityMotion.SetStyle(personality.Motion);
            foreach (var sparkle in personalitySparkles) sparkle.gameObject.SetActive(personality.Motion == PersonalityMotionStyle.Show);
            personalityPlay.GetComponentInChildren<Text>().text = "Play with " + name;
            personalityPlay.interactable = session != null && session.IsOwnedAndAvailable(personalityId);

            // Two of the character's traits, then the variety's tint on its own colour.
            for (int i = 0; i < chipTexts.Count; i++)
            {
                bool tint = i == Mathf.Min(2, personality.Traits.Count);
                string text = i < 2 && i < personality.Traits.Count ? personality.Traits[i] : tint ? personality.Tint : "";
                chipTexts[i].text = text;
                chipFaces[i].color = tint ? PockleTheme.FieldFor(personalityId) : Quiet;
                chipFaces[i].gameObject.SetActive(!string.IsNullOrEmpty(text));
            }
            for (int i = 0; i < reactionRows.Length; i++)
            {
                bool has = i < personality.Reactions.Count;
                reactionRows[i].gameObject.SetActive(has);
                if (!has) continue;
                reactionWhen[i].text = personality.Reactions[i].Key; reactionDoes[i].text = personality.Reactions[i].Value;
            }

            // Shelf friends: the day moments this toy can share, found or still a surprise.
            int slot = 0;
            foreach (var moment in PersonalityBook.Moments)
            {
                if (moment.Night || !moment.Involves(personalityId) || slot >= friendFaces.Length) continue;
                string partner = moment.Partner(personalityId);
                bool found = PipVariants.TryFromCollectibleId(partner, out var partnerIndex) && foundMoments.Contains(moment.Id);
                friendFaces[slot].color = found ? PockleTheme.FieldFor(partner) : PockleTheme.Mystery;
                friendPictures[slot].texture = found ? previews[(int)partnerIndex] : null;
                friendPictures[slot].gameObject.SetActive(found);
                friendOutlines[slot].gameObject.SetActive(!found);
                friendLabels[slot].text = found ? moment.LabelFor(personalityId) : "Not found yet";
                friendLabels[slot].color = found ? Ink : MutedInk;
                friendFaces[slot].gameObject.name = "Shelf friend · " + (found ? partner : "unknown");
                slot++;
            }
            for (int i = slot; i < friendFaces.Length; i++) friendFaces[i].gameObject.SetActive(false);
            for (int i = 0; i < slot; i++) friendFaces[i].gameObject.SetActive(true);
        }

        /// <summary>Full-bleed layout: the toy on its field, then the sheet. Returns the content height.</summary>
        private float LayoutPersonality(float width)
        {
            const float gutter = 22f;
            float inner = Mathf.Min(width - gutter * 2, 440f), left = (width - inner) / 2;
            var voice = personalityVoice.GetComponent<RectTransform>();
            float voiceWidth = Mathf.Max(120, personalityVoiceLabel.preferredWidth + 32);
            LeftLabel(voice, new Vector2(width - 20 - voiceWidth, -16), new Vector2(voiceWidth, 44));

            const float toy = 220f, baseY = 300f;
            LeftLabel(personalityToy, new Vector2(width / 2 - toy / 2, -(baseY + SceneBase * toy - toy)), new Vector2(toy, toy));
            LeftLabel(personalityShadow, new Vector2(width / 2 - 85, -(baseY - 12)), new Vector2(170, 30));
            LeftLabel(personalitySparkles[0], new Vector2(width / 2 - 98, -108), new Vector2(26, 26));
            LeftLabel(personalitySparkles[1], new Vector2(width / 2 + 78, -140), new Vector2(18, 18));

            float y = 24;
            LeftLabel(personalityName.rectTransform, new Vector2(left, -y), new Vector2(inner, 36)); y += 40;
            LeftLabel(personalityLine.rectTransform, new Vector2(left, -y), new Vector2(inner, 40)); y += 48;
            LeftLabel(personalityChips, new Vector2(left, -y), new Vector2(inner, 30));
            float x = 0;
            for (int i = 0; i < chipTexts.Count; i++)
            {
                if (!chipFaces[i].gameObject.activeSelf) continue;
                float chipWidth = chipTexts[i].preferredWidth + 24;
                LeftLabel(chipFaces[i].rectTransform, new Vector2(x, 0), new Vector2(chipWidth, 30));
                x += chipWidth + 8;
            }
            y += 30 + 20;
            LeftLabel(personalityReactsHeading, new Vector2(left, -y), new Vector2(inner, 26)); y += 34;
            int rows = 0; foreach (var row in reactionRows) if (row.gameObject.activeSelf) rows++;
            LeftLabel(personalityTable, new Vector2(left, -y), new Vector2(inner, rows * 44));
            for (int i = 0; i < reactionRows.Length; i++)
            {
                LeftLabel(reactionRows[i], new Vector2(14, -i * 44), new Vector2(inner - 28, 44));
            }
            y += rows * 44 + 18;
            LeftLabel(personalityFriendsHeading, new Vector2(left, -y), new Vector2(inner, 26)); y += 34;
            LeftLabel(personalityFriendsRow, new Vector2(left, -y), new Vector2(inner, 92));
            float tile = (inner - 16) / 3;
            for (int i = 0; i < friendFaces.Length; i++)
            {
                var face = friendFaces[i].rectTransform;
                LeftLabel(face, new Vector2(i * (tile + 8), 0), new Vector2(tile, 92));
                TopCentered(friendPictures[i].rectTransform, new Vector2(0, -6), new Vector2(54, 54));
                TopCentered(friendOutlines[i].rectTransform, new Vector2(0, -6), new Vector2(54, 54));
                var label = friendLabels[i].rectTransform; Stretch(label); label.offsetMin = new Vector2(6, 8); label.offsetMax = new Vector2(-6, -60);
            }
            y += 92 + 16;
            LeftLabel(personalityPlay.GetComponent<RectTransform>(), new Vector2(left, -y), new Vector2(inner, 52)); y += 52 + 28;

            LeftLabel(personalitySheet, new Vector2(0, -PersonalitySheetTop), new Vector2(width, y + 400));
            float height = PersonalitySheetTop + y;
            TopCentered(personalityRoot, Vector2.zero, new Vector2(width, height));
            return height;
        }

        private void PlayVoice()
        {
            if (!soundEnabled || !PersonalityBook.TryGet(personalityId, out var personality)) return;
            if (voiceSource == null) { voiceSource = gameObject.AddComponent<AudioSource>(); voiceSource.playOnAwake = false; }
            voiceSource.volume = Mathf.Clamp01(PlayerPrefs.GetFloat("pockle.prototype.soundVolume", 1));
            voiceSource.PlayOneShot(VoiceClip(personality.VoicePitch));
        }

        private readonly Dictionary<float, AudioClip> voiceClips = new Dictionary<float, AudioClip>();

        /// <summary>A tiny synthesized two-note chirp, pitched per personality. Local and placeholder, like the toy sounds.</summary>
        private AudioClip VoiceClip(float pitch)
        {
            if (voiceClips.TryGetValue(pitch, out var clip) && clip != null) return clip;
            const int rate = 22050; int length = (int)(rate * .26f);
            var samples = new float[length];
            double phase = 0;
            for (int i = 0; i < length; i++)
            {
                float t = i / (float)rate;
                float note = t < .12f ? 660f : 880f;
                phase += 2 * Mathf.PI * note * pitch / rate;
                float local = t < .12f ? t / .12f : (t - .12f) / .14f;
                float envelope = Mathf.Sin(Mathf.PI * Mathf.Clamp01(local)) * .35f;
                samples[i] = (float)System.Math.Sin(phase) * envelope;
            }
            clip = AudioClip.Create("Pockle voice " + pitch, length, 1, rate, false);
            clip.SetData(samples, 0);
            voiceClips[pitch] = clip;
            return clip;
        }
    }
}
