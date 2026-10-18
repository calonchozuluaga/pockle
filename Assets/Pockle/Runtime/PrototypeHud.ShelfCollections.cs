using System;
using System.Collections.Generic;
using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// Shelf tab, under the live shelf: one card per collection with its cover, box and progress, and a row of still
    /// slots (found toys first, then the ones still to find). Nothing here animates in 3D; only the live shelf moves.
    /// </summary>
    public sealed partial class PrototypeHud
    {
        private const int CardSlots = 5;
        private sealed class ShelfCard
        {
            public CollectionDefinition Collection;
            public RectTransform Root, Banner, SlotRow;
            public RawImage Cover, Box;
            public Text Name, Count;
            public Image Fill;
            public readonly Button[] Slots = new Button[CardSlots];
            public readonly RawImage[] Pictures = new RawImage[CardSlots];
            public readonly CharacterSilhouetteGraphic[] Outlines = new CharacterSilhouetteGraphic[CardSlots];
            public readonly RectTransform[] LiveBadges = new RectTransform[CardSlots], SpareBadges = new RectTransform[CardSlots];
            public readonly Text[] SpareLabels = new Text[CardSlots];
            public readonly string[] Ids = new string[CardSlots];
            public float BoxAspect = 1f;
        }
        private readonly List<ShelfCard> shelfCards = new List<ShelfCard>();
        private RectTransform shelfCollectionsHeading;
        private readonly Dictionary<string, Texture> stillPortraits = new Dictionary<string, Texture>();

        /// <summary>Collections with toys you can find today, newest first; planned characters stay under "All collections".</summary>
        private static List<CollectionDefinition> ShelfCollections()
        {
            var list = new List<CollectionDefinition>();
            foreach (var collection in ToyCatalog.Collections)
            {
                bool available = false;
                foreach (string id in collection.Members) if (ToyCatalog.TryGetCollectible(id, out var toy) && toy.Available) { available = true; break; }
                if (available) list.Add(collection);
            }
            list.Sort((a, b) => a.SeriesNumber > 0 && b.SeriesNumber > 0 ? b.SeriesNumber.CompareTo(a.SeriesNumber) : b.SeriesNumber.CompareTo(a.SeriesNumber));
            return list;
        }

        private void BuildShelfCollections()
        {
            shelfCollectionsHeading = Heading("Your collections", shelfRoot);
            foreach (var collection in ShelfCollections())
            {
                string id = collection.Id;
                var card = new ShelfCard { Collection = collection };
                card.Root = Rect("Shelf collection card · " + id, shelfRoot); Surface(card.Root, Color.white, 26);
                card.Root.gameObject.AddComponent<Mask>().showMaskGraphic = true;
                // Banner: the collection's cover with its box; tapping it opens the collection.
                var banner = CreateButton(collection.DisplayName, card.Root, Vector2.zero, new Vector2(300, 92), CatalogUnityColor(collection.FieldColor),
                    () => OpenCollection(id), out var bannerFace, 20, false);
                SetRadius(bannerFace, 1);
                banner.name = "Shelf collection · " + id;
                card.Banner = banner.GetComponent<RectTransform>();
                var cover = Rect("Cover", card.Banner); Stretch(cover); cover.SetAsFirstSibling();
                card.Cover = cover.gameObject.AddComponent<RawImage>(); card.Cover.raycastTarget = false;
                var fade = Rect("Fade", card.Banner); Stretch(fade); fade.SetSiblingIndex(1); fade.anchorMax = new Vector2(.82f, 1);
                var fadeImage = fade.gameObject.AddComponent<RawImage>(); fadeImage.texture = UiShapes.FadeRight; fadeImage.raycastTarget = false;
                fadeImage.color = new Color(PockleTheme.Milk.r, PockleTheme.Milk.g, PockleTheme.Milk.b, .95f);
                var boxRect = Rect("Box", card.Banner);
                card.Box = boxRect.gameObject.AddComponent<RawImage>(); card.Box.raycastTarget = false;
                BoxHype.Add(card.Box, Color.Lerp(CollectionLook.Accent(collection), Color.white, .25f), shelfCards.Count * 1.1f);
                card.Name = banner.GetComponentInChildren<Text>(); card.Name.alignment = TextAnchor.UpperLeft; card.Name.color = Ink;
                var name = card.Name.rectTransform; name.anchorMin = name.anchorMax = name.pivot = new Vector2(0, 1);
                name.anchoredPosition = new Vector2(16, -14); name.sizeDelta = new Vector2(190, 28);
                var track = Rect("Progress track", card.Banner); Surface(track, new Color(1, 1, 1, .9f), 5);
                track.anchorMin = track.anchorMax = track.pivot = new Vector2(0, 1); track.anchoredPosition = new Vector2(16, -54); track.sizeDelta = new Vector2(140, 10);
                card.Fill = ProgressFill(track, CatalogUnityColor(collection.AccentColor));
                card.Count = Label("", card.Banner, PockleTheme.SmallSize, CatalogUnityColor(collection.InkColor), FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(60, 20));
                card.Count.rectTransform.anchorMin = card.Count.rectTransform.anchorMax = card.Count.rectTransform.pivot = new Vector2(0, 1);
                card.Count.rectTransform.anchoredPosition = new Vector2(164, -49); card.Count.rectTransform.sizeDelta = new Vector2(60, 20);

                // Slots: still portraits on their colour fields, rounded by a mask so corners never square out.
                card.SlotRow = Rect("Slots", card.Root);
                for (int s = 0; s < CardSlots; s++)
                {
                    int index = s;
                    var slot = CreateButton("", card.SlotRow, Vector2.zero, new Vector2(60, 64), PockleTheme.Mystery,
                        () => { string toy = card.Ids[index]; if (toy != null) ShowToy(toy); }, out var face, 12, false);
                    SetRadius(face, 16);
                    slot.gameObject.AddComponent<Mask>().showMaskGraphic = true;
                    var colours = slot.colors; colours.disabledColor = Color.white; slot.colors = colours;
                    var outlineHolder = Rect("Outline", slot.transform);
                    outlineHolder.anchorMin = new Vector2(.15f, .08f); outlineHolder.anchorMax = new Vector2(.85f, .9f); outlineHolder.offsetMin = outlineHolder.offsetMax = Vector2.zero;
                    card.Outlines[s] = outlineHolder.gameObject.AddComponent<CharacterSilhouetteGraphic>();
                    card.Outlines[s].color = new Color(PockleTheme.PlumSoft.r, PockleTheme.PlumSoft.g, PockleTheme.PlumSoft.b, .5f);
                    card.Outlines[s].Configure(collection.CharacterId);
                    var picture = Rect("Portrait", slot.transform); Stretch(picture);
                    card.Pictures[s] = picture.gameObject.AddComponent<RawImage>(); card.Pictures[s].raycastTarget = false;
                    card.Slots[s] = slot;
                    TintMirror.Follow(slot, card.Pictures[s]);
                    // A star marks toys that live on the live shelf; ×N counts spares.
                    card.LiveBadges[s] = Rect("On the live shelf", slot.transform); Surface(card.LiveBadges[s], Ink, 9);
                    var star = Rect("Star", card.LiveBadges[s]); Stretch(star); star.offsetMin = new Vector2(3, 3); star.offsetMax = new Vector2(-3, -3);
                    var starImage = star.gameObject.AddComponent<RawImage>(); starImage.texture = UiShapes.Sparkle; starImage.color = PockleTheme.FieldGold; starImage.raycastTarget = false;
                    card.SpareBadges[s] = Rect("Spares", slot.transform); Surface(card.SpareBadges[s], Ink, 9);
                    card.SpareLabels[s] = Label("", card.SpareBadges[s], 11, PockleTheme.OnPlum, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
                    Stretch(card.SpareLabels[s].rectTransform);
                }
                shelfCards.Add(card);
            }
        }

        /// <summary>A found toy's still portrait on its colour field (Pips reuse the shared shelf renders).</summary>
        private Texture StillPortrait(string id)
        {
            if (PipVariants.TryFromCollectibleId(id, out var legacy)) return previews[(int)legacy];
            if (lineupPortraits.TryGetValue(id, out var cached)) return cached;
            Texture texture = null;
            try { texture = ToyPortrait.Render(id, PockleTheme.FieldFor(id)); }
            catch (Exception error) { Debug.LogWarning("Pockle shelf slot portrait: " + error.Message); }
            lineupPortraits[id] = texture;
            return texture;
        }

        private void RefreshShelfCollections()
        {
            if (shelfCards.Count == 0 || session == null) return;
            bool visible = page == AppPage.Shelf;
            foreach (var card in shelfCards)
            {
                var collection = card.Collection;
                int found = FoundCount(collection), total = collection.Members.Count;
                card.Count.text = found + "/" + total;
                card.Fill.fillAmount = total == 0 ? 0 : (float)found / total;
                if (visible && card.Cover.texture == null)
                {
                    var cover = CollectionLook.Banner(collection);
                    card.Cover.texture = cover; card.Cover.enabled = cover != null;
                    var box = CollectionLook.Box(collection, out card.BoxAspect);
                    card.Box.texture = box; card.Box.enabled = box != null;
                }
                // Found toys first (catalog order), then the ones still to find.
                var order = new List<string>();
                foreach (string id in collection.Members) if (session.GetOwnedCount(id) > 0) order.Add(id);
                foreach (string id in collection.Members)
                    if (session.GetOwnedCount(id) == 0 && ToyCatalog.TryGetCollectible(id, out var toy) && toy.Available && id != collection.SecretId) order.Add(id);
                for (int s = 0; s < CardSlots; s++)
                {
                    string id = s < order.Count ? order[s] : null;
                    card.Ids[s] = id;
                    card.Slots[s].gameObject.SetActive(id != null);
                    if (id == null) continue;
                    int owned = session.GetOwnedCount(id);
                    bool playable = session.IsOwnedAndPlayable(id);
                    card.Slots[s].name = "Slot · " + id;
                    card.Slots[s].interactable = playable;
                    card.Slots[s].GetComponent<Image>().color = owned > 0 ? PockleTheme.FieldFor(id) : PockleTheme.Mystery;
                    var picture = owned > 0 && visible ? StillPortrait(id) : null;
                    card.Pictures[s].texture = picture; card.Pictures[s].enabled = owned > 0 && picture != null;
                    card.Outlines[s].enabled = owned == 0;
                    card.LiveBadges[s].gameObject.SetActive(liveShelf.Contains(id));
                    card.SpareBadges[s].gameObject.SetActive(owned > 1);
                    card.SpareLabels[s].text = "×" + Mathf.Min(owned, 99);
                }
            }
        }

        private float LayoutShelfCollections(float width, float y)
        {
            LeftLabel(shelfCollectionsHeading, new Vector2(0, -y), new Vector2(width, 30)); y += 40;
            const float bannerHeight = 92, slotHeight = 64, pad = 10;
            foreach (var card in shelfCards)
            {
                LeftLabel(card.Root, new Vector2(0, -y), new Vector2(width, bannerHeight + slotHeight + pad * 2));
                LeftLabel(card.Banner, Vector2.zero, new Vector2(width, bannerHeight));
                card.Cover.uvRect = CollectionLook.Cover(card.Cover.texture, width / bannerHeight, .5f, .55f);
                var box = card.Box.rectTransform;
                box.anchorMin = box.anchorMax = box.pivot = new Vector2(1, 0);
                box.sizeDelta = new Vector2(96 * card.BoxAspect, 96); box.anchoredPosition = new Vector2(-8, -8);
                LeftLabel(card.SlotRow, new Vector2(pad, -(bannerHeight + pad)), new Vector2(width - pad * 2, slotHeight));
                float slot = (width - pad * 2 - 6 * (CardSlots - 1)) / CardSlots;
                for (int s = 0; s < CardSlots; s++)
                {
                    LeftLabel(card.Slots[s].GetComponent<RectTransform>(), new Vector2(s * (slot + 6), 0), new Vector2(slot, slotHeight));
                    var live = card.LiveBadges[s]; live.anchorMin = live.anchorMax = live.pivot = new Vector2(1, 1);
                    live.anchoredPosition = new Vector2(-4, -4); live.sizeDelta = new Vector2(18, 18);
                    var spare = card.SpareBadges[s]; spare.anchorMin = spare.anchorMax = spare.pivot = new Vector2(0, 1);
                    spare.anchoredPosition = new Vector2(4, -4); spare.sizeDelta = new Vector2(26, 18);
                }
                y += bannerHeight + slotHeight + pad * 2 + 12;
            }
            RefreshShelfCollections();
            return y;
        }
    }
}
