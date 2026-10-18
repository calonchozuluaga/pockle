using System.Collections.Generic;
using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// "Pick your live shelf" (canvas board "Pick your live shelf"): the six spots in seat order, a filter per collection,
    /// and every toy you own. Tap to put a toy on the shelf or take it off; picks save immediately.
    /// </summary>
    public sealed partial class PrototypeHud
    {
        private RectTransform shelfPickRoot, pickSpotsRow, pickFilters, pickGrid;
        private Text pickCount;
        private Button pickDone;
        private readonly Button[] pickSpots = new Button[LiveShelfSpots];
        private readonly RawImage[] pickSpotPictures = new RawImage[LiveShelfSpots];
        private readonly Text[] pickSpotPlus = new Text[LiveShelfSpots];
        private readonly List<Button> pickFilterButtons = new List<Button>();
        private readonly List<string> pickFilterIds = new List<string>();
        private sealed class PickTile { public Button Button; public Image Ring; public RawImage Picture; public RectTransform Badge; public Text BadgeLabel; public string CollectionId; }
        private readonly Dictionary<string, PickTile> pickTiles = new Dictionary<string, PickTile>();
        private readonly List<string> pickOrder = new List<string>();
        private string pickFilter = "";

        private void BuildShelfPick()
        {
            pickCount = Label("", shelfPickRoot, PockleTheme.CaptionSize + 1, MutedInk, FontStyle.Bold, TextAnchor.MiddleRight, Vector2.zero, new Vector2(100, 24));
            pickSpotsRow = Rect("Your six spots", shelfPickRoot); Surface(pickSpotsRow, Quiet, 20);
            for (int i = 0; i < LiveShelfSpots; i++)
            {
                int spot = i;
                pickSpots[i] = CreateButton("", pickSpotsRow, Vector2.zero, new Vector2(48, 52), Paper,
                    () => { if (spot < liveShelf.Count) ToggleLiveShelf(liveShelf[spot]); }, out var face, 12, false);
                SetRadius(face, 14);
                pickSpots[i].name = "Spot " + (i + 1);
                pickSpots[i].gameObject.AddComponent<Mask>().showMaskGraphic = true;
                var picture = Rect("Toy", pickSpots[i].transform); Stretch(picture);
                pickSpotPictures[i] = picture.gameObject.AddComponent<RawImage>(); pickSpotPictures[i].raycastTarget = false;
                TintMirror.Follow(pickSpots[i], pickSpotPictures[i]);
                pickSpotPlus[i] = pickSpots[i].GetComponentInChildren<Text>(); pickSpotPlus[i].fontSize = 22; pickSpotPlus[i].color = MutedInk;
            }
            pickFilters = Rect("Collection filters", shelfPickRoot);
            pickGrid = Rect("Your toys", shelfPickRoot);
            pickDone = CreateButton("Done", shelfPickRoot, Vector2.zero, new Vector2(300, 56), Primary, () => GoBack(), out _);
            pickDone.name = "Shelf pick done";
        }

        private void RefreshShelfPick()
        {
            if (shelfPickRoot == null || session == null) return;
            SyncLiveShelf();
            pickCount.text = liveShelf.Count + " of " + LiveShelfSpots;
            pickCount.color = liveShelf.Count == LiveShelfSpots ? PockleTheme.Success : MutedInk;
            for (int i = 0; i < LiveShelfSpots; i++)
            {
                string id = i < liveShelf.Count ? liveShelf[i] : null;
                pickSpots[i].GetComponent<Image>().color = id != null ? PockleTheme.FieldFor(id) : Paper;
                pickSpotPictures[i].texture = id != null && page == AppPage.ShelfPick ? StillPortrait(id) : null;
                pickSpotPictures[i].enabled = pickSpotPictures[i].texture != null;
                pickSpotPlus[i].text = id == null ? "+" : "";
                pickSpots[i].name = id != null ? "Spot " + (i + 1) + " · " + id : "Spot " + (i + 1);
            }

            // Filters: All, then each collection you own toys from.
            var owned = ShelfCandidates();
            var collections = new List<string> { "" };
            foreach (string id in owned)
                if (ToyCatalog.TryGetCollectible(id, out var toy) && !collections.Contains(toy.CollectionId)) collections.Add(toy.CollectionId);
            while (pickFilterButtons.Count < collections.Count)
            {
                int index = pickFilterButtons.Count;
                var chip = CreateButton("", pickFilters, Vector2.zero, new Vector2(80, 36), Quiet, () => { pickFilter = pickFilterIds[index]; RefreshShelfPick(); AdaptLayout(); }, out _, 14);
                pickFilterButtons.Add(chip); pickFilterIds.Add("");
            }
            if (!collections.Contains(pickFilter)) pickFilter = "";
            for (int i = 0; i < pickFilterButtons.Count; i++)
            {
                bool shown = i < collections.Count;
                pickFilterButtons[i].gameObject.SetActive(shown);
                if (!shown) continue;
                pickFilterIds[i] = collections[i];
                string label = collections[i].Length == 0 ? "All" : ToyCatalog.TryGetCollection(collections[i], out var c) ? c.DisplayName : collections[i];
                bool on = collections[i] == pickFilter;
                pickFilterButtons[i].name = "Shelf filter · " + label;
                pickFilterButtons[i].GetComponentInChildren<Text>().text = label;
                pickFilterButtons[i].GetComponentInChildren<Text>().color = on ? PockleTheme.OnPlum : Ink;
                pickFilterButtons[i].GetComponent<Image>().color = on ? Primary : Quiet;
            }

            // Every toy you own, with its seat number when it's on the shelf.
            pickOrder.Clear();
            foreach (string id in owned)
            {
                if (!pickTiles.TryGetValue(id, out var tile)) pickTiles[id] = tile = BuildPickTile(id);
                bool shown = pickFilter.Length == 0 || tile.CollectionId == pickFilter;
                tile.Button.gameObject.SetActive(shown); tile.Ring.gameObject.SetActive(false);
                if (!shown) continue;
                pickOrder.Add(id);
                int seat = liveShelf.IndexOf(id);
                tile.Ring.gameObject.SetActive(seat >= 0);
                tile.Badge.gameObject.SetActive(seat >= 0);
                tile.BadgeLabel.text = (seat + 1).ToString();
                if (page == AppPage.ShelfPick && tile.Picture.texture == null) { tile.Picture.texture = StillPortrait(id); tile.Picture.enabled = tile.Picture.texture != null; }
                // A full shelf greys out toys that aren't on it, until a spot frees up.
                tile.Button.interactable = seat >= 0 || liveShelf.Count < LiveShelfSpots;
            }
            foreach (var pair in pickTiles) if (!owned.Contains(pair.Key)) { pair.Value.Button.gameObject.SetActive(false); pair.Value.Ring.gameObject.SetActive(false); }
        }

        private PickTile BuildPickTile(string id)
        {
            var tile = new PickTile { CollectionId = ToyCatalog.TryGetCollectible(id, out var toy) ? toy.CollectionId : "" };
            // The selection ring sits behind the tile, 3 px larger with a radius 3 px larger, so the corners stay round.
            var ring = Rect("Selected ring", pickGrid); tile.Ring = Surface(ring, Ink, 21);
            tile.Button = CreateButton(PersonalityBook.ShortName(id), pickGrid, Vector2.zero, new Vector2(80, 100), PockleTheme.FieldFor(id),
                () => ToggleLiveShelf(id), out var face, 11, false);
            SetRadius(face, 18);
            tile.Button.name = "Pick · " + id;
            tile.Button.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var colours = tile.Button.colors; colours.disabledColor = new Color(1, 1, 1, .45f); tile.Button.colors = colours;
            var label = tile.Button.GetComponentInChildren<Text>().rectTransform;
            label.anchorMin = new Vector2(0, 0); label.anchorMax = new Vector2(1, 0); label.pivot = new Vector2(.5f, 0);
            label.offsetMin = new Vector2(3, 6); label.offsetMax = new Vector2(-3, 24);
            var picture = Rect("Toy", tile.Button.transform); picture.SetAsFirstSibling();
            picture.anchorMin = new Vector2(.1f, .24f); picture.anchorMax = new Vector2(.9f, .96f); picture.offsetMin = picture.offsetMax = Vector2.zero;
            tile.Picture = picture.gameObject.AddComponent<RawImage>(); tile.Picture.raycastTarget = false; tile.Picture.enabled = false;
            TintMirror.Follow(tile.Button, tile.Picture);
            tile.Badge = Rect("Seat", tile.Button.transform); Surface(tile.Badge, Ink, 11);
            tile.Badge.anchorMin = tile.Badge.anchorMax = tile.Badge.pivot = new Vector2(1, 1); tile.Badge.anchoredPosition = new Vector2(-6, -6); tile.Badge.sizeDelta = new Vector2(22, 22);
            tile.BadgeLabel = Label("", tile.Badge, 12, PockleTheme.OnPlum, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            Stretch(tile.BadgeLabel.rectTransform);
            return tile;
        }

        private float LayoutShelfPick(float width)
        {
            float y = 0;
            LeftLabel(pickCount.rectTransform, new Vector2(width - 100, -y), new Vector2(100, 24)); y += 30;
            LeftLabel(pickSpotsRow, new Vector2(0, -y), new Vector2(width, 68));
            float spot = (width - 16 - 6 * (LiveShelfSpots - 1)) / LiveShelfSpots;
            for (int i = 0; i < LiveShelfSpots; i++) LeftLabel(pickSpots[i].GetComponent<RectTransform>(), new Vector2(8 + i * (spot + 6), -8), new Vector2(spot, 52));
            y += 68 + 14;
            LeftLabel(pickFilters, new Vector2(0, -y), new Vector2(width, 36));
            float x = 0;
            foreach (var chip in pickFilterButtons)
            {
                if (!chip.gameObject.activeSelf) continue;
                float w = chip.GetComponentInChildren<Text>().preferredWidth + 28;
                LeftLabel(chip.GetComponent<RectTransform>(), new Vector2(x, 0), new Vector2(w, 36)); x += w + 6;
            }
            y += 36 + 14;
            int columns = width >= 450 ? 5 : 4;
            const float gap = 8;
            float tileWidth = (width - gap * (columns - 1)) / columns, tileHeight = Mathf.Round(tileWidth * 1.22f);
            LeftLabel(pickGrid, new Vector2(0, -y), new Vector2(width, 0));
            for (int i = 0; i < pickOrder.Count; i++)
            {
                var tile = pickTiles[pickOrder[i]];
                var position = new Vector2((i % columns) * (tileWidth + gap), -(i / columns) * (tileHeight + gap));
                LeftLabel(tile.Button.GetComponent<RectTransform>(), position, new Vector2(tileWidth, tileHeight));
                LeftLabel(tile.Ring.rectTransform, position + new Vector2(-3, 3), new Vector2(tileWidth + 6, tileHeight + 6));
            }
            int rows = (pickOrder.Count + columns - 1) / columns;
            y += rows * (tileHeight + gap) + 12;
            LeftLabel(pickDone.GetComponent<RectTransform>(), new Vector2(0, -y), new Vector2(width, 56)); y += 68;
            TopCentered(shelfPickRoot, Vector2.zero, new Vector2(width, y));
            return y;
        }
    }
}
