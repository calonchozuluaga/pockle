using System;
using System.Collections.Generic;
using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// Living shelf (owner-approved canvas "Living shelf", October 12): up to four of your toys stand on two shelves and idle
    /// in character, neighbours share authored moments, and the shelf turns to night. Toys render as live meshes through
    /// <see cref="ShelfPreviewRuntime"/> on the scene colour; <see cref="PersonalityMotion"/> adds each personality's sway.
    /// Moments found are a presentation-only preference (pockle.shelf.moments.v1), never inventory.
    /// </summary>
    public sealed partial class PrototypeHud
    {
        public const string MomentsKey = "pockle.shelf.moments.v1";
        private static readonly Color SceneDay = new Color32(0xFF, 0xF1, 0xE6, 0xFF), SceneNight = PockleTheme.Midnight;
        private static readonly Color PlankDay = new Color32(0xF2, 0xDC, 0xC8, 0xFF), PlankNight = new Color32(0x4A, 0x44, 0x80, 0xFF);
        /// <summary>Show-offs and dreamers up top, as on the approved canvas.</summary>
        private static readonly string[] SceneOrder = { PersonalityBook.Gold, PersonalityBook.Moon, PersonalityBook.Peach, PersonalityBook.Mint };
        private const float SceneBase = .14f; // Where a toy's base sits inside its square portrait, from the bottom.

        private bool shelfNight;
        private RectTransform timeSwitch, scene, sceneMoon, momentsCard, momentsTrack, sceneEmpty, allToysHeading;
        private Image sceneFace, momentsFill, dayFace, nightFace;
        private Button dayButton, nightButton, fuseRow;
        private Text momentsLabel;
        private readonly RectTransform[] planks = new RectTransform[2];
        private readonly RectTransform[] chips = new RectTransform[2];
        private readonly Text[] chipLabels = new Text[2];
        private readonly Button[] sceneSlots = new Button[4];
        private readonly PersonalityMotion[] sceneMotion = new PersonalityMotion[4];
        private readonly RawImage[] scenePictures = new RawImage[4];
        private readonly CharacterSilhouetteGraphic[] sceneSilhouettes = new CharacterSilhouetteGraphic[4];
        private readonly ShelfPreviewRuntime.Binding[] sceneBindings = new ShelfPreviewRuntime.Binding[4];
        private readonly string[] sceneIds = new string[4];
        private readonly RectTransform[] sceneZs = new RectTransform[4];
        private readonly List<RectTransform> sparkles = new List<RectTransform>();
        private RectTransform sceneHeart;
        private readonly Dictionary<string, RenderTexture> scenePortraits = new Dictionary<string, RenderTexture>();
        private readonly HashSet<string> foundMoments = new HashSet<string>();
        private float sceneHeight, sceneSlot;

        private static bool NightByClock()
        {
            int hour = DateTime.Now.Hour;
            return hour >= 21 || hour < 6;
        }

        private void BuildLivingShelf()
        {
            shelfNight = NightByClock();
            foreach (string id in (PlayerPrefs.GetString(MomentsKey, "") ?? "").Split(','))
                if (!string.IsNullOrEmpty(id)) foundMoments.Add(id);

            // Day / Night switch.
            timeSwitch = Rect("Shelf time", shelfRoot); Surface(timeSwitch, Quiet, 22);
            dayButton = CreateButton(Greeting(), timeSwitch, Vector2.zero, new Vector2(100, 38), Paper, () => SetShelfNight(false), out dayFace, 15);
            dayButton.name = "Shelf time · day";
            nightButton = CreateButton("Night", timeSwitch, Vector2.zero, new Vector2(100, 38), Paper, () => SetShelfNight(true), out nightFace, 15);
            nightButton.name = "Shelf time · night";

            // The scene: a colour field, two shelves, four toy slots, and the marks that come and go.
            scene = Rect("Living shelf", shelfRoot); sceneFace = Surface(scene, SceneDay, 32);
            scene.gameObject.AddComponent<RectMask2D>();
            sceneMoon = Rect("Moon", scene);
            var moonImage = sceneMoon.gameObject.AddComponent<RawImage>(); moonImage.texture = UiShapes.Moon; moonImage.color = PockleTheme.FieldGold;
            moonImage.raycastTarget = false;
            for (int i = 0; i < sceneSlots.Length; i++)
            {
                int slot = i;
                var holder = Rect("Shelf toy slot " + i, scene);
                var hit = holder.gameObject.AddComponent<Image>(); hit.color = Color.clear;
                sceneSlots[i] = holder.gameObject.AddComponent<Button>(); sceneSlots[i].targetGraphic = hit;
                sceneSlots[i].onClick.AddListener(() => { if (sceneIds[slot] != null) OpenPersonality(sceneIds[slot]); });
                var motion = Rect("Personality motion", holder); Stretch(motion); motion.pivot = new Vector2(.5f, SceneBase);
                sceneMotion[i] = motion.gameObject.AddComponent<PersonalityMotion>();
                var picture = Rect("Toy", motion); Stretch(picture);
                scenePictures[i] = picture.gameObject.AddComponent<RawImage>(); scenePictures[i].raycastTarget = false;
                var silhouette = Rect("Unused outline", motion); Stretch(silhouette);
                sceneSilhouettes[i] = silhouette.gameObject.AddComponent<CharacterSilhouetteGraphic>(); silhouette.gameObject.SetActive(false);
                sceneMotion[i].Configure(PersonalityMotion.Kind.Toy, PersonalityMotionStyle.Calm, i * .37f);
                sceneZs[i] = SceneMark("z", scene, i * .6f);
            }
            // Shelves sit in front of the toys' bases.
            for (int i = 0; i < planks.Length; i++) { planks[i] = Rect("Shelf " + (i + 1), scene); Surface(planks[i], PlankDay, 6); }
            for (int i = 0; i < 2; i++) sparkles.Add(Sparkle(scene, i == 0 ? (Color)new Color32(0xE0, 0xA9, 0x2E, 0xFF) : Color.white, i * .3f));
            sceneHeart = Rect("Say hi", scene);
            var bubble = Rect("Bubble", sceneHeart); Stretch(bubble); Surface(bubble, Color.white, 14);
            var heart = AddIcon(bubble, "Heart", 18, PockleTheme.Heart); heart.anchoredPosition = Vector2.zero;
            bubble.gameObject.AddComponent<PersonalityMotion>().Configure(PersonalityMotion.Kind.Pop);
            for (int i = 0; i < chips.Length; i++)
            {
                chips[i] = Rect("Shelf moment " + (i + 1), scene); Surface(chips[i], Color.white, 15);
                chipLabels[i] = Label("", chips[i], PockleTheme.SmallSize, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
                Stretch(chipLabels[i].rectTransform); chipLabels[i].resizeTextForBestFit = false; chipLabels[i].horizontalOverflow = HorizontalWrapMode.Overflow;
            }
            sceneEmpty = Label("Your toys will live here. Walk or open a box to find one.", scene, PockleTheme.BodySize, MutedInk, FontStyle.Normal,
                TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero).rectTransform;

            // Moments found.
            momentsCard = Rect("Shelf moments", shelfRoot); Surface(momentsCard, Quiet, 22);
            var star = Rect("Moments icon", momentsCard);
            var starImage = star.gameObject.AddComponent<RawImage>(); starImage.texture = UiShapes.Sparkle; starImage.color = Ink; starImage.raycastTarget = false;
            LeftLabel(star, new Vector2(16, -22), new Vector2(24, 24));
            momentsLabel = Label("", momentsCard, PockleTheme.CaptionSize + 1, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(200, 22));
            momentsTrack = Rect("Moments progress", momentsCard); Surface(momentsTrack, PockleTheme.FillDeep, 3);
            momentsFill = ProgressFill(momentsTrack, Ink);
            var hint = Label("Try a shy toy next to a show-off", momentsCard, PockleTheme.SmallSize, MutedInk, FontStyle.Normal,
                TextAnchor.MiddleLeft, Vector2.zero, new Vector2(96, 40));
            hint.rectTransform.anchorMin = hint.rectTransform.anchorMax = hint.rectTransform.pivot = new Vector2(1, .5f);
            hint.rectTransform.anchoredPosition = new Vector2(-14, 0);

            fuseRow = RowButton("Fuse spares", "Make a one-of-a-kind toy", "Box", shelfRoot, () => Navigate(AppPage.Fusion), out _, true);
            allToysHeading = Heading("All your toys", shelfRoot);
        }

        private RectTransform SceneMark(string text, Transform parent, float phase)
        {
            var mark = Label(text, parent, 18, PockleTheme.OnPlumMuted, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(24, 24));
            var rect = Rect("Sleepy z", parent); rect.sizeDelta = new Vector2(24, 24);
            mark.rectTransform.SetParent(rect, false); Stretch(mark.rectTransform);
            mark.gameObject.AddComponent<PersonalityMotion>().Configure(PersonalityMotion.Kind.FloatUp, phaseSeconds: phase);
            return rect;
        }

        private RectTransform Sparkle(Transform parent, Color colour, float phase)
        {
            var holder = Rect("Sparkle", parent);
            var glyph = Rect("Glyph", holder); Stretch(glyph);
            var image = glyph.gameObject.AddComponent<RawImage>(); image.texture = UiShapes.Sparkle; image.color = colour; image.raycastTarget = false;
            glyph.gameObject.AddComponent<PersonalityMotion>().Configure(PersonalityMotion.Kind.Twinkle, phaseSeconds: phase);
            return holder;
        }

        private void SetShelfNight(bool night)
        {
            if (shelfNight == night) return;
            shelfNight = night;
            RefreshLivingShelf();
            if (page == AppPage.Shelf && subtitle != null) subtitle.text = ShelfMood();
        }

        private string ShelfMood()
        {
            return shelfNight ? "Late night. Everyone is dozing off." : Greeting() + ". Your toys are up to something.";
        }

        /// <summary>A still portrait on the scene colour, used whenever the live preview isn't running (calm, scrolled away).</summary>
        private Texture ScenePortrait(string id, bool night)
        {
            string key = id + (night ? "@night" : "@day");
            if (!scenePortraits.TryGetValue(key, out var texture) || texture == null)
                scenePortraits[key] = texture = ToyPortrait.Render(id, night ? SceneNight : SceneDay);
            return texture;
        }

        /// <summary>Lays out the time switch, scene, moments card and fuse row from <paramref name="y"/>; returns the new y.</summary>
        private float LayoutLivingShelf(float width, float y)
        {
            float half = (width - 12) / 2;
            TopCentered(timeSwitch, new Vector2(0, -y), new Vector2(width, 46));
            LeftLabel(dayFace.rectTransform, new Vector2(4, -4), new Vector2(half, 38));
            LeftLabel(nightFace.rectTransform, new Vector2(8 + half, -4), new Vector2(half, 38));
            y += 46 + 14;

            float k = width / 350f;
            sceneHeight = Mathf.Round(420 * k); sceneSlot = Mathf.Round(136 * k);
            TopCentered(scene, new Vector2(0, -y), new Vector2(width, sceneHeight));
            LeftLabel(sceneMoon, new Vector2(width - 62 * k, -18 * k), new Vector2(40 * k, 40 * k));
            float topShelf = 178 * k, bottomShelf = sceneHeight - 38 * k;
            LeftLabel(planks[0], new Vector2(18 * k, -topShelf), new Vector2(width - 36 * k, 12 * k));
            LeftLabel(planks[1], new Vector2(18 * k, -bottomShelf), new Vector2(width - 36 * k, 12 * k));
            float[] centres = { 96 * k, width - 101 * k, 108 * k, width - 108 * k };
            for (int i = 0; i < sceneSlots.Length; i++)
            {
                float baseY = i < 2 ? topShelf : bottomShelf;
                float top = baseY + SceneBase * sceneSlot - sceneSlot;
                LeftLabel(sceneSlots[i].GetComponent<RectTransform>(), new Vector2(centres[i] - sceneSlot / 2, -top), new Vector2(sceneSlot, sceneSlot));
                LeftLabel(sceneZs[i], new Vector2(centres[i] + sceneSlot * .22f, -(top + sceneSlot * .08f)), new Vector2(24, 24));
            }
            LeftLabel(sceneHeart, new Vector2((centres[2] + centres[3]) / 2 - 20, -(bottomShelf - 150 * k)), new Vector2(40, 30));
            TopCentered(sceneEmpty, Vector2.zero, new Vector2(width - 48, sceneHeight));
            y += sceneHeight + 14;

            TopCentered(momentsCard, new Vector2(0, -y), new Vector2(width, 68));
            LeftLabel(momentsLabel.rectTransform, new Vector2(52, -14), new Vector2(width - 52 - 120, 22));
            LeftLabel(momentsTrack, new Vector2(52, -42), new Vector2(width - 52 - 120, 6));
            y += 68 + 12;
            LeftLabel(fuseRow.GetComponent<RectTransform>(), new Vector2(0, -y), new Vector2(width, 60));
            y += 60 + 24;
            LeftLabel(allToysHeading, new Vector2(0, -y), new Vector2(width, 30));
            y += 42;
            RefreshLivingShelf();
            return y;
        }

        private void RefreshLivingShelf()
        {
            if (scene == null) return;
            sceneFace.color = shelfNight ? SceneNight : SceneDay;
            foreach (var plank in planks) plank.GetComponent<Image>().color = shelfNight ? PlankNight : PlankDay;
            sceneMoon.gameObject.SetActive(shelfNight);
            dayFace.color = shelfNight ? new Color(1, 1, 1, 0) : Color.white;
            nightFace.color = shelfNight ? Color.white : new Color(1, 1, 1, 0);
            dayButton.GetComponentInChildren<Text>().text = Greeting();

            // Fill the slots with owned toys, show-offs and dreamers first.
            int filled = 0;
            for (int i = 0; i < SceneOrder.Length; i++)
                if (session != null && session.GetOwnedCount(SceneOrder[i]) > 0 && session.TryGetPlayableVariant(SceneOrder[i], out _))
                    AssignSceneSlot(filled++, SceneOrder[i]);
            for (int i = filled; i < sceneSlots.Length; i++) AssignSceneSlot(i, null);
            sceneEmpty.gameObject.SetActive(filled == 0);

            bool sparkled = false;
            foreach (var sparkle in sparkles) sparkle.gameObject.SetActive(false);
            for (int i = 0; i < sceneSlots.Length; i++)
            {
                string id = sceneIds[i];
                PersonalityBook.TryGet(id, out var personality);
                var style = personality != null ? personality.Motion : PersonalityMotionStyle.Calm;
                // At night everyone dozes, except the dreamers, who stay up.
                if (shelfNight && style != PersonalityMotionStyle.Dream) style = PersonalityMotionStyle.Sleep;
                sceneMotion[i].SetStyle(style);
                sceneZs[i].gameObject.SetActive(id != null && style == PersonalityMotionStyle.Sleep);
                if (id != null && !shelfNight && style == PersonalityMotionStyle.Show && !sparkled)
                {
                    var slot = sceneSlots[i].GetComponent<RectTransform>();
                    var a = sparkles[0]; var b = sparkles[1];
                    LeftLabel(a, slot.anchoredPosition + new Vector2(sceneSlot * .06f, -sceneSlot * .10f), new Vector2(22, 22));
                    LeftLabel(b, slot.anchoredPosition + new Vector2(sceneSlot * .80f, -sceneSlot * .18f), new Vector2(14, 14));
                    a.gameObject.SetActive(true); b.gameObject.SetActive(true); sparkled = true;
                }
            }

            // Neighbours on each shelf share a moment; seeing one records it as found.
            bool changed = false;
            for (int row = 0; row < 2; row++)
            {
                string a = sceneIds[row * 2], b = sceneIds[row * 2 + 1];
                var moment = a != null && b != null ? PersonalityBook.FindMoment(a, b, shelfNight) : null;
                chips[row].gameObject.SetActive(moment != null);
                if (moment == null) continue;
                chipLabels[row].text = moment.Caption;
                float chipWidth = Mathf.Min(scene.sizeDelta.x - 24, chipLabels[row].preferredWidth + 24);
                float chipTop = row == 0 ? 22 * scene.sizeDelta.x / 350f : sceneHeight - (196 + 30) * scene.sizeDelta.x / 350f;
                TopCentered(chips[row], new Vector2(0, -chipTop), new Vector2(chipWidth, 30));
                chips[row].GetComponent<Image>().color = shelfNight ? new Color(1, 1, 1, .14f) : Color.white;
                chipLabels[row].color = shelfNight ? PockleTheme.OnPlum : Ink;
                if (page == AppPage.Shelf && foundMoments.Add(moment.Id)) changed = true;
            }
            sceneHeart.gameObject.SetActive(!shelfNight && chips[1].gameObject.activeSelf);
            if (changed) { PlayerPrefs.SetString(MomentsKey, string.Join(",", foundMoments)); PlayerPrefs.Save(); }
            int total = PersonalityBook.Moments.Count, found = FoundMomentCount();
            momentsLabel.text = found + " of " + total + " shelf moments found";
            momentsFill.fillAmount = total == 0 ? 0 : found / (float)total;
        }

        private int FoundMomentCount()
        {
            int found = 0;
            foreach (var moment in PersonalityBook.Moments) if (foundMoments.Contains(moment.Id)) found++;
            return found;
        }

        private void AssignSceneSlot(int slot, string id)
        {
            sceneSlots[slot].gameObject.SetActive(id != null);
            sceneSlots[slot].name = id != null ? "Shelf toy · " + id : "Shelf toy slot " + slot;
            if (sceneIds[slot] != id)
            {
                if (sceneBindings[slot] != null) { shelfPreviews.Unregister(sceneBindings[slot]); sceneBindings[slot] = null; }
                sceneIds[slot] = id;
                if (id != null)
                {
                    scenePictures[slot].texture = ScenePortrait(id, shelfNight);
                    sceneBindings[slot] = shelfPreviews.Register(scenePictures[slot], sceneSilhouettes[slot], id, shelfNight ? SceneNight : SceneDay);
                    shelfPreviews.SetFound(sceneBindings[slot], true);
                }
            }
            if (id != null && sceneBindings[slot] != null)
                shelfPreviews.SetPresentation(sceneBindings[slot], ScenePortrait(id, shelfNight), shelfNight ? SceneNight : SceneDay);
        }

        private void ReleaseScenePortraits()
        {
            foreach (var texture in scenePortraits.Values) if (texture != null) { texture.Release(); Destroy(texture); }
            scenePortraits.Clear();
        }
    }
}
