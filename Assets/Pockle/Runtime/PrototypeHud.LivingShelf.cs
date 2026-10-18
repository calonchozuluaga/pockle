using System;
using System.Collections.Generic;
using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// Live shelf (owner decisions October 14; canvas row "Live shelf"): six toys the player picks, on two shelves of
    /// three. They idle in character and neighbours share moments; every other toy lives as a still slot under its
    /// collection, so only these six move. New toys take a free spot automatically. Each toy is a still portrait
    /// moved by <see cref="PersonalityMotion"/>, measured once so its feet stand on the plank. The picks and the moments
    /// found are presentation preferences, never inventory.
    /// </summary>
    public sealed partial class PrototypeHud
    {
        public const string MomentsKey = "pockle.shelf.moments.v1";
        public const string LiveShelfKey = "pockle.shelf.live.v1", LiveShelfSeenKey = "pockle.shelf.seen.v1";
        public const int LiveShelfSpots = 6;
        private const int ShelfColumns = 3;
        private static readonly Color SceneDay = new Color32(0xFF, 0xF1, 0xE6, 0xFF), SceneNight = PockleTheme.Midnight;
        private static readonly Color PlankDay = new Color32(0xE9, 0xCD, 0xB2, 0xFF), PlankNight = new Color32(0x4A, 0x44, 0x80, 0xFF);
        /// <summary>First-fill order: show-offs and dreamers first, as on the approved canvas; then everything else in catalog order.</summary>
        private static readonly string[] SceneOrder = { PersonalityBook.Gold, PersonalityBook.Moon, PersonalityBook.Peach, PersonalityBook.Mint };
        private const float SceneBase = .14f; // Fallback: where a toy's base sits inside its square portrait, from the bottom.
        /// <summary>Where the two planks sit in a shelf backdrop image, as fractions of its height (Codex's art follows this).</summary>
        private const float TopPlank = .42f, BottomPlank = .88f;
        // Codex's shelf art (Resources/UI/Shelves/LiveShelfBackground, October 14): measured plank tops, and its
        // aspect, which the scene keeps so the planks never stretch or crop.
        private const string ShelfBackdropPath = "UI/Shelves/LiveShelfBackground";
        private const float BackdropTopPlank = .392f, BackdropBottomPlank = .828f;
        private float TopPlankAt => shelfBackdrop != null && shelfBackdrop.texture != null ? BackdropTopPlank : TopPlank;
        private float BottomPlankAt => shelfBackdrop != null && shelfBackdrop.texture != null ? BackdropBottomPlank : BottomPlank;

        private bool shelfNight;
        private RectTransform timeSwitch, scene, sceneMoon, momentsCard, momentsTrack, sceneEmpty, spotsChip;
        private Image sceneFace, momentsFill, dayFace, nightFace;
        private Button dayButton, nightButton, fuseRow, editShelf;
        private Text momentsLabel, spotsLabel;
        private RawImage shelfBackdrop;
        private readonly RectTransform[] planks = new RectTransform[2];
        private readonly RectTransform[] chips = new RectTransform[2];
        private readonly Text[] chipLabels = new Text[2];
        private readonly Button[] sceneSlots = new Button[LiveShelfSpots];
        private readonly Button[] emptySpots = new Button[LiveShelfSpots];
        private readonly PersonalityMotion[] sceneMotion = new PersonalityMotion[LiveShelfSpots];
        private readonly RawImage[] scenePictures = new RawImage[LiveShelfSpots];
        private readonly string[] sceneIds = new string[LiveShelfSpots];
        private readonly RectTransform[] sceneZs = new RectTransform[LiveShelfSpots];
        private readonly List<RectTransform> sparkles = new List<RectTransform>();
        private RectTransform sceneHeart;
        private readonly Dictionary<string, Texture2D> scenePortraits = new Dictionary<string, Texture2D>();
        private readonly Dictionary<string, float> sceneFeet = new Dictionary<string, float>();
        private readonly float[] sceneCentres = new float[LiveShelfSpots], sceneShelfY = new float[LiveShelfSpots];
        private readonly HashSet<string> foundMoments = new HashSet<string>();
        private readonly List<string> liveShelf = new List<string>();
        private readonly HashSet<string> liveShelfSeen = new HashSet<string>();
        private float sceneHeight, sceneSlot;

        private static bool NightByClock()
        {
            int hour = DateTime.Now.Hour;
            return hour >= 21 || hour < 6;
        }

        /// <summary>The toys on the live shelf, in seat order (left to right, top shelf first).</summary>
        public IReadOnlyList<string> LiveShelf => liveShelf;

        private void BuildLivingShelf()
        {
            shelfNight = NightByClock();
            foreach (string id in (PlayerPrefs.GetString(MomentsKey, "") ?? "").Split(','))
                if (!string.IsNullOrEmpty(id)) foundMoments.Add(id);
            foreach (string id in (PlayerPrefs.GetString(LiveShelfKey, "") ?? "").Split(','))
                if (!string.IsNullOrEmpty(id) && liveShelf.Count < LiveShelfSpots && !liveShelf.Contains(id)) liveShelf.Add(id);
            foreach (string id in (PlayerPrefs.GetString(LiveShelfSeenKey, "") ?? "").Split(','))
                if (!string.IsNullOrEmpty(id)) liveShelfSeen.Add(id);

            // Day / Night switch and Edit shelf.
            timeSwitch = Rect("Shelf time", shelfRoot); Surface(timeSwitch, Quiet, 22);
            dayButton = CreateButton(Greeting(), timeSwitch, Vector2.zero, new Vector2(100, 38), Paper, () => SetShelfNight(false), out dayFace, 15);
            dayButton.name = "Shelf time · day";
            nightButton = CreateButton("Night", timeSwitch, Vector2.zero, new Vector2(100, 38), Paper, () => SetShelfNight(true), out nightFace, 15);
            nightButton.name = "Shelf time · night";
            editShelf = CreateButton("Edit shelf", shelfRoot, Vector2.zero, new Vector2(120, 46), Quiet, () => Navigate(AppPage.ShelfPick), out _, 15);
            editShelf.name = "Edit shelf";

            // The scene: a shelf (Codex's backdrop when present, else a colour wall and two planks), six spots, marks.
            scene = Rect("Living shelf", shelfRoot); sceneFace = Surface(scene, SceneDay, 32);
            scene.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var backdrop = Rect("Shelf backdrop", scene); Stretch(backdrop);
            shelfBackdrop = backdrop.gameObject.AddComponent<RawImage>(); shelfBackdrop.raycastTarget = false;
            shelfBackdrop.texture = Resources.Load<Texture2D>(ShelfBackdropPath) ?? Resources.Load<Texture2D>("UI/Shelf/Backdrop");
            shelfBackdrop.enabled = shelfBackdrop.texture != null;
            sceneMoon = Rect("Moon", scene);
            var moonImage = sceneMoon.gameObject.AddComponent<RawImage>(); moonImage.texture = UiShapes.Moon; moonImage.color = PockleTheme.FieldGold;
            moonImage.raycastTarget = false;
            for (int i = 0; i < planks.Length; i++) { planks[i] = Rect("Shelf " + (i + 1), scene); Surface(planks[i], PlankDay, 6); }
            for (int i = 0; i < LiveShelfSpots; i++)
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
                sceneMotion[i].Configure(PersonalityMotion.Kind.Toy, PersonalityMotionStyle.Calm, i * .37f);
                sceneZs[i] = SceneMark("z", scene, i * .6f);
                // An open spot: a soft "+" that opens the picker.
                emptySpots[i] = CreateButton("+", scene, Vector2.zero, new Vector2(56, 56), new Color(1, 1, 1, .55f), () => Navigate(AppPage.ShelfPick), out _, 24);
                emptySpots[i].name = "Open spot " + i;
                emptySpots[i].GetComponentInChildren<Text>().color = MutedInk;
            }
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
            spotsChip = Rect("Spots", scene); Surface(spotsChip, new Color(1, 1, 1, .7f), 14);
            spotsLabel = Label("", spotsChip, PockleTheme.SmallSize - 1, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            Stretch(spotsLabel.rectTransform);
            sceneEmpty = Label("Your toys will live here. Walk or open a box to find one.", scene, PockleTheme.BodySize, MutedInk, FontStyle.Normal,
                TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero).rectTransform;

            // Moments found.
            momentsCard = Rect("Shelf moments", shelfRoot); Surface(momentsCard, Quiet, 22);
            var star = Rect("Moments icon", momentsCard);
            var starImage = star.gameObject.AddComponent<RawImage>(); starImage.texture = UiShapes.Sparkle; starImage.color = Ink; starImage.raycastTarget = false;
            LeftLabel(star, new Vector2(16, -22), new Vector2(24, 24));
            momentsLabel = Label("", momentsCard, PockleTheme.CaptionSize + 1, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(200, 22));
            momentsTrack = Rect("Moments hint", momentsCard);
            var hint = Label("Mix collections: new neighbours share new moments", momentsCard, PockleTheme.SmallSize, MutedInk, FontStyle.Normal,
                TextAnchor.MiddleLeft, Vector2.zero, new Vector2(200, 20));
            hint.rectTransform.SetParent(momentsTrack, false); Stretch(hint.rectTransform);
            momentsFill = null;

            fuseRow = RowButton("Fuse spares", "Make a one-of-a-kind toy", "Box", shelfRoot, () => Navigate(AppPage.Fusion), out _, true);
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

        // ----- Which toys live on the shelf -----

        /// <summary>Owned, playable toys in first-fill order.</summary>
        private List<string> ShelfCandidates()
        {
            var list = new List<string>();
            if (session == null) return list;
            foreach (string id in SceneOrder) if (session.IsOwnedAndPlayable(id)) list.Add(id);
            foreach (var toy in ToyCatalog.Collectibles) if (!list.Contains(toy.Id) && session.IsOwnedAndPlayable(toy.Id)) list.Add(toy.Id);
            return list;
        }

        /// <summary>
        /// Drops toys that are no longer yours, then seats toys you've never had on the shelf while a spot is free.
        /// A toy the player took off stays off: only newly found toys are added automatically.
        /// </summary>
        private void SyncLiveShelf()
        {
            if (session == null) return;
            bool changed = liveShelf.RemoveAll(id => !session.IsOwnedAndPlayable(id)) > 0;
            foreach (string id in ShelfCandidates())
            {
                if (liveShelfSeen.Contains(id)) continue;
                liveShelfSeen.Add(id); changed = true;
                if (liveShelf.Count < LiveShelfSpots && !liveShelf.Contains(id)) liveShelf.Add(id);
            }
            if (changed) SaveLiveShelf();
        }

        private void SaveLiveShelf()
        {
            PlayerPrefs.SetString(LiveShelfKey, string.Join(",", liveShelf));
            PlayerPrefs.SetString(LiveShelfSeenKey, string.Join(",", liveShelfSeen));
            PlayerPrefs.Save();
        }

        /// <summary>Puts a toy on the shelf in the next free spot, or takes it off. Returns false when the shelf is full.</summary>
        public bool ToggleLiveShelf(string id)
        {
            if (liveShelf.Remove(id)) { SaveLiveShelf(); RefreshLivingShelf(); RefreshShelfPick(); RefreshShelf(); return true; }
            if (liveShelf.Count >= LiveShelfSpots || session == null || !session.IsOwnedAndPlayable(id)) return false;
            liveShelf.Add(id); liveShelfSeen.Add(id); SaveLiveShelf();
            RefreshLivingShelf(); RefreshShelfPick(); RefreshShelf();
            return true;
        }

        // ----- Portraits -----

        /// <summary>
        /// A cut-out portrait (transparent background), so a toy tilting with its personality never shows the square of
        /// its picture over the plank or its neighbours (owner, October 14). Measured once so its feet rest on the plank.
        /// </summary>
        private Texture ScenePortrait(string id, bool night) => CutoutPortrait(id);

        private Texture CutoutPortrait(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (scenePortraits.TryGetValue(id, out var texture) && texture != null) return texture;
            try
            {
                scenePortraits[id] = texture = PortraitCutout.Render(id, out float feet);
                sceneFeet[id] = feet >= 0 ? feet : SceneBase;
            }
            catch (Exception error) { Debug.LogWarning("Pockle shelf portrait: " + error.Message); return StillPortrait(id); }
            return texture;
        }

        private float FeetOf(string id) => id != null && sceneFeet.TryGetValue(id, out var feet) ? feet : SceneBase;

        /// <summary>Stands a spot's portrait so its measured feet rest on the top edge of its plank.</summary>
        private void PlaceSceneSlot(int i)
        {
            if (sceneSlot <= 0) return;
            float feet = FeetOf(sceneIds[i]);
            float top = sceneShelfY[i] - (1 - feet) * sceneSlot;
            LeftLabel(sceneSlots[i].GetComponent<RectTransform>(), new Vector2(sceneCentres[i] - sceneSlot / 2, -top), new Vector2(sceneSlot, sceneSlot));
            sceneMotion[i].GetComponent<RectTransform>().pivot = new Vector2(.5f, feet);
            LeftLabel(sceneZs[i], new Vector2(sceneCentres[i] + sceneSlot * .22f, -(top + sceneSlot * .08f)), new Vector2(24, 24));
            LeftLabel(emptySpots[i].GetComponent<RectTransform>(), new Vector2(sceneCentres[i] - 28, -(sceneShelfY[i] - 64)), new Vector2(56, 56));
        }

        // ----- Layout and refresh -----

        /// <summary>Lays out the switch, scene, moments card and fuse row from <paramref name="y"/>; returns the new y.</summary>
        private float LayoutLivingShelf(float width, float y)
        {
            float editWidth = Mathf.Min(130, editShelf.GetComponentInChildren<Text>().preferredWidth + 36);
            float switchWidth = width - editWidth - 10, half = (switchWidth - 12) / 2;
            LeftLabel(timeSwitch, new Vector2(0, -y), new Vector2(switchWidth, 46));
            LeftLabel(dayFace.rectTransform, new Vector2(4, -4), new Vector2(half, 38));
            LeftLabel(nightFace.rectTransform, new Vector2(8 + half, -4), new Vector2(half, 38));
            LeftLabel(editShelf.GetComponent<RectTransform>(), new Vector2(width - editWidth, -y), new Vector2(editWidth, 46));
            y += 46 + 14;

            float k = width / 350f;
            var art = shelfBackdrop.texture;
            sceneHeight = art != null ? Mathf.Round(width * art.height / Mathf.Max(1f, art.width)) : Mathf.Round(430 * k);
            sceneSlot = Mathf.Round(Mathf.Min(118 * k, (width - 24) / ShelfColumns));
            LeftLabel(scene, new Vector2(0, -y), new Vector2(width, sceneHeight));
            shelfBackdrop.uvRect = new Rect(0, 0, 1, 1); // the scene takes the art's own shape
            LeftLabel(sceneMoon, new Vector2(width - 62 * k, -54 * k), new Vector2(40 * k, 40 * k));
            float topShelf = Mathf.Round(TopPlankAt * sceneHeight), bottomShelf = Mathf.Round(BottomPlankAt * sceneHeight);
            LeftLabel(planks[0], new Vector2(14 * k, -topShelf), new Vector2(width - 28 * k, 12 * k));
            LeftLabel(planks[1], new Vector2(14 * k, -bottomShelf), new Vector2(width - 28 * k, 12 * k));
            for (int i = 0; i < LiveShelfSpots; i++)
            {
                int column = i % ShelfColumns;
                sceneCentres[i] = width * (column + .5f) / ShelfColumns;
                sceneShelfY[i] = i < ShelfColumns ? topShelf : bottomShelf;
                PlaceSceneSlot(i);
            }
            // Bottom right, under the lower plank, so it never sits on the top shelf's moment label.
            spotsChip.anchorMin = spotsChip.anchorMax = spotsChip.pivot = new Vector2(1, 0);
            spotsChip.anchoredPosition = new Vector2(-14, 12); spotsChip.sizeDelta = new Vector2(Mathf.Max(96, spotsLabel.preferredWidth + 24), 28);
            LeftLabel(sceneHeart, new Vector2(width / 2 - 20, -(bottomShelf - sceneSlot * 1.15f)), new Vector2(40, 30));
            TopCentered(sceneEmpty, Vector2.zero, new Vector2(width - 48, sceneHeight));
            y += sceneHeight + 14;

            LeftLabel(momentsCard, new Vector2(0, -y), new Vector2(width, 64));
            LeftLabel(momentsLabel.rectTransform, new Vector2(52, -12), new Vector2(width - 64, 22));
            LeftLabel(momentsTrack, new Vector2(52, -34), new Vector2(width - 64, 20));
            y += 64 + 12;
            LeftLabel(fuseRow.GetComponent<RectTransform>(), new Vector2(0, -y), new Vector2(width, 60));
            y += 60 + 24;
            RefreshLivingShelf();
            return y;
        }

        private void RefreshLivingShelf()
        {
            if (scene == null) return;
            SyncLiveShelf();
            bool backdrop = shelfBackdrop.texture != null;
            sceneFace.color = shelfNight ? SceneNight : SceneDay;
            shelfBackdrop.color = shelfNight ? new Color(.45f, .45f, .7f, 1f) : Color.white;
            foreach (var plank in planks) { plank.GetComponent<Image>().color = shelfNight ? PlankNight : PlankDay; plank.gameObject.SetActive(!backdrop); }
            sceneMoon.gameObject.SetActive(shelfNight);
            dayFace.color = shelfNight ? new Color(1, 1, 1, 0) : Color.white;
            nightFace.color = shelfNight ? Color.white : new Color(1, 1, 1, 0);
            dayButton.GetComponentInChildren<Text>().text = Greeting();
            spotsLabel.text = liveShelf.Count + " of " + LiveShelfSpots + " spots";
            bool anyOwned = ShelfCandidates().Count > 0;

            for (int i = 0; i < LiveShelfSpots; i++) AssignSceneSlot(i, i < liveShelf.Count ? liveShelf[i] : null, anyOwned);
            sceneEmpty.gameObject.SetActive(!anyOwned);
            spotsChip.gameObject.SetActive(anyOwned);

            bool sparkled = false;
            foreach (var sparkle in sparkles) sparkle.gameObject.SetActive(false);
            for (int i = 0; i < LiveShelfSpots; i++)
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
                    LeftLabel(sparkles[0], slot.anchoredPosition + new Vector2(sceneSlot * .06f, -sceneSlot * .10f), new Vector2(22, 22));
                    LeftLabel(sparkles[1], slot.anchoredPosition + new Vector2(sceneSlot * .80f, -sceneSlot * .18f), new Vector2(14, 14));
                    sparkles[0].gameObject.SetActive(true); sparkles[1].gameObject.SetActive(true); sparkled = true;
                }
            }

            // Neighbours on the open shelf count a day together; friendship deepens at 1, 3 and 7 days and never drops.
            bool befriended = false;
            if (page == AppPage.Shelf)
                for (int row = 0; row < 2; row++)
                    for (int pair = 0; pair < ShelfColumns - 1; pair++)
                    {
                        string a = sceneIds[row * ShelfColumns + pair], b = sceneIds[row * ShelfColumns + pair + 1];
                        if (a != null && b != null && ShelfFriendships.RecordTogether(a, b, CollectionSession.Today)) befriended = true;
                    }
            if (befriended) ShelfFriendships.Save();

            // Each shelf shows one moment its neighbours share, at their friendship level; seeing it records it as found.
            bool changed = false;
            for (int row = 0; row < 2; row++)
            {
                ShelfMoment moment = null;
                for (int pair = 0; pair < ShelfColumns - 1 && moment == null; pair++)
                {
                    string a = sceneIds[row * ShelfColumns + pair], b = sceneIds[row * ShelfColumns + pair + 1];
                    if (a != null && b != null) moment = PersonalityBook.FindMoment(a, b, shelfNight, ShelfFriendships.Level(a, b));
                }
                chips[row].gameObject.SetActive(moment != null);
                if (moment == null) continue;
                chipLabels[row].text = moment.Caption;
                float chipWidth = Mathf.Min(scene.sizeDelta.x - 24, chipLabels[row].preferredWidth + 24);
                float shelfY = row == 0 ? TopPlankAt * sceneHeight : BottomPlankAt * sceneHeight;
                TopCentered(chips[row], new Vector2(0, -(shelfY - sceneSlot * 1.12f - 34)), new Vector2(chipWidth, 30));
                chips[row].GetComponent<Image>().color = shelfNight ? new Color(1, 1, 1, .16f) : Color.white;
                chipLabels[row].color = shelfNight ? PockleTheme.OnPlum : Ink;
                if (page == AppPage.Shelf && foundMoments.Add(moment.Id)) changed = true;
            }
            sceneHeart.gameObject.SetActive(!shelfNight && chips[1].gameObject.activeSelf);
            if (changed) { PlayerPrefs.SetString(MomentsKey, string.Join(",", foundMoments)); PlayerPrefs.Save(); }
            int found = foundMoments.Count;
            momentsLabel.text = found == 1 ? "1 shelf moment found" : found + " shelf moments found";
        }

        private int FoundMomentCount() => foundMoments.Count;

        private void AssignSceneSlot(int slot, string id, bool anyOwned)
        {
            sceneSlots[slot].gameObject.SetActive(id != null);
            sceneSlots[slot].name = id != null ? "Shelf toy · " + id : "Shelf toy slot " + slot;
            emptySpots[slot].gameObject.SetActive(id == null && anyOwned);
            sceneIds[slot] = id;
            scenePictures[slot].texture = id != null ? ScenePortrait(id, shelfNight) : null;
            PlaceSceneSlot(slot);
        }

        private void ReleaseScenePortraits()
        {
            foreach (var texture in scenePortraits.Values) if (texture != null) Destroy(texture);
            scenePortraits.Clear();
        }
    }
}
