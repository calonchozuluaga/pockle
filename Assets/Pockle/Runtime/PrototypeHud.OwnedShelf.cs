using System;
using System.Collections.Generic;
using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    public sealed partial class PrototypeHud
    {
        private enum OwnedShelfFilter { All, Fusions, Extras }
        private sealed class OwnedShelfTile
        {
            public string Id, Search;
            public bool Fusion;
            public Button Button;
            public RawImage Portrait;
            public Text Name, Detail, Count;
            public RectTransform Picture, Badge;
        }
        private RectTransform ownedShelfRoot, ownedShelfGrid, ownedShelfFilters;
        private Text ownedShelfSummary, ownedShelfEmpty;
        private InputField ownedShelfSearch;
        private Button ownedShelfWalk, ownedShelfShop, ownedShelfFuse, ownedShelfLive, ownedShelfReturn;
        private readonly Image[] ownedShelfFilterFaces = new Image[3];
        private readonly List<OwnedShelfTile> ownedShelfTiles = new List<OwnedShelfTile>();
        private readonly Dictionary<string, Texture2D> ownedShelfPortraits = new Dictionary<string, Texture2D>();
        private readonly LinkedList<string> ownedShelfPortraitOrder = new LinkedList<string>();
        private readonly Vector3[] ownedShelfCorners = new Vector3[4];
        private const int OwnedShelfPortraitLimit = 36;
        private OwnedShelfFilter ownedShelfFilter;
        private bool ownedShelfLiveView;
        private string ownedShelfKeys = "";
        private int ownedShelfRenderedFrame = -1;

        private void BuildOwnedShelf()
        {
            ownedShelfRoot = Rect("Owned shelf", shelfRoot);
            ownedShelfSummary = Label("", ownedShelfRoot, 14, MutedInk, FontStyle.Normal, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            ownedShelfWalk = OwnedShelfAction("Go for a walk", "Earn your next box", "Owned shelf · Walk", PockleTheme.FieldMint,
                () => SelectTab(AppPage.Rewards));
            ownedShelfShop = OwnedShelfAction("Find a new friend", "Explore the shop", "Owned shelf · Shop", PockleTheme.FieldPeach,
                () => SelectTab(AppPage.Boxes));
            var search = Rect("Search your shelf", ownedShelfRoot);
            var face = Surface(search, new Color(1, 1, 1, .8f), 18); face.raycastTarget = true;
            var text = Label("", search, 15, Ink, FontStyle.Normal, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            text.resizeTextForBestFit = false; text.supportRichText = false;
            Stretch(text.rectTransform); text.rectTransform.offsetMin = new Vector2(16, 2); text.rectTransform.offsetMax = new Vector2(-16, -2);
            var placeholder = Label("Find a toy or character", search, 15, MutedInk, FontStyle.Normal, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            Stretch(placeholder.rectTransform); placeholder.rectTransform.offsetMin = new Vector2(16, 2); placeholder.rectTransform.offsetMax = new Vector2(-16, -2);
            ownedShelfSearch = search.gameObject.AddComponent<InputField>(); ownedShelfSearch.targetGraphic = face;
            ownedShelfSearch.textComponent = text; ownedShelfSearch.placeholder = placeholder; ownedShelfSearch.characterLimit = 80;
            ownedShelfSearch.lineType = InputField.LineType.SingleLine;
            ownedShelfSearch.onValueChanged.AddListener(_ => OwnedShelfFilterChanged());
            ownedShelfFilters = Rect("Owned shelf filters", ownedShelfRoot);
            string[] labels = { "All owned", "Fusions", "Extras" };
            for (int i = 0; i < labels.Length; i++)
            {
                var filter = (OwnedShelfFilter)i;
                var button = CreateButton(labels[i], ownedShelfFilters, Vector2.zero, Vector2.zero, Quiet,
                    () => { ownedShelfFilter = filter; OwnedShelfFilterChanged(); }, out ownedShelfFilterFaces[i], 14);
                button.name = "Owned shelf filter · " + labels[i];
            }
            ownedShelfGrid = Rect("Owned toy grid", ownedShelfRoot);
            ownedShelfEmpty = Label("", ownedShelfRoot, 17, MutedInk, FontStyle.Normal, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            ownedShelfFuse = OwnedShelfAction("Make something new", "Fuse two spares of the same character", "Owned shelf · Fuse", PockleTheme.FieldMoon,
                () => Navigate(AppPage.Fusion));
            ownedShelfLive = CreateButton("Live shelf", ownedShelfRoot, Vector2.zero, Vector2.zero, Quiet, OpenLivingShelfView, out _, 15);
            ownedShelfLive.name = "Owned shelf · Live shelf";
            ownedShelfReturn = CreateButton("All my Pockles", shelfRoot, Vector2.zero, Vector2.zero, Quiet,
                () => { ResetOwnedShelfView(); content.anchoredPosition = Vector2.zero; AdaptLayout(); }, out _, 15);
            ownedShelfReturn.name = "Live shelf · All my Pockles";
        }

        private Button OwnedShelfAction(string title, string caption, string name, Color color, Action action)
        {
            var button = CreateButton(title, ownedShelfRoot, Vector2.zero, Vector2.zero, color, action, out _, 15);
            button.name = name;
            var words = button.GetComponentInChildren<Text>(); words.alignment = TextAnchor.MiddleLeft;
            words.rectTransform.anchorMin = new Vector2(0, .48f); words.rectTransform.anchorMax = new Vector2(1, 1);
            words.rectTransform.offsetMin = new Vector2(12, 0); words.rectTransform.offsetMax = new Vector2(-12, -8);
            var sub = Label(caption, button.transform, 12, MutedInk, FontStyle.Normal, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            sub.rectTransform.anchorMin = Vector2.zero; sub.rectTransform.anchorMax = new Vector2(1, .48f);
            sub.rectTransform.offsetMin = new Vector2(12, 8); sub.rectTransform.offsetMax = new Vector2(-12, 0);
            return button;
        }

        private void ResetOwnedShelfView() { ownedShelfLiveView = false; }
        private void OpenLivingShelfView()
        {
            ownedShelfLiveView = true;
            if (page == AppPage.Shelf) AdaptLayout();
            else Navigate(AppPage.Shelf);
            // This is a different shelf presentation; a deep owned-grid scroll must not hide its scene on entry.
            content.anchoredPosition = Vector2.zero;
            navigator.SaveScroll(0);
        }
        private void OwnedShelfFilterChanged()
        {
            if (page != AppPage.Shelf) return;
            content.anchoredPosition = Vector2.zero;
            AdaptLayout();
        }

        private void RefreshOwnedShelf()
        {
            if (ownedShelfRoot == null || session == null) return;
            var ids = new List<string>();
            foreach (var pair in session.OwnedCounts)
                if (pair.Value > 0 && session.IsOwnedAndPlayable(pair.Key)) ids.Add(pair.Key);
            ids.Sort(StringComparer.Ordinal);
            string keys = string.Join("|", ids);
            if (keys != ownedShelfKeys)
            {
                foreach (var tile in ownedShelfTiles) { tile.Button.gameObject.SetActive(false); Destroy(tile.Button.gameObject); }
                ownedShelfTiles.Clear(); ownedShelfKeys = keys;
                foreach (string id in ids) BuildOwnedShelfTile(id);
            }
            long extras = 0;
            foreach (var tile in ownedShelfTiles)
            {
                int count = session.GetOwnedCount(tile.Id); extras += Math.Max(0, count - 1);
                tile.Count.text = count > 1 ? "+" + (count - 1) + (count == 2 ? " extra" : " extras") : tile.Fusion ? "Fused" : "Owned";
                tile.Button.interactable = session.IsOwnedAndPlayable(tile.Id);
            }
            ownedShelfSummary.text = ids.Count + (ids.Count == 1 ? " little friend" : " little friends") + " · " + extras + (extras == 1 ? " extra copy" : " extra copies");
        }

        private void BuildOwnedShelfTile(string id)
        {
            if (!ToyCatalog.TryGetCollectible(id, out var toy)) return;
            bool fused = FusionCatalog.TryGet(id, out _) || toy.CollectionId == PipFusionStudies.CollectionId;
            string character = ToyCatalog.TryGetCharacter(toy.CharacterId, out var c) ? c.DisplayName : toy.CharacterId;
            string tier = fused ? "Fusion" : toy.Tier.ToString();
            var tile = new OwnedShelfTile { Id = id, Fusion = fused, Search = toy.FinishDisplayName + " " + character + " " + tier };
            tile.Button = CreateButton("", ownedShelfGrid, Vector2.zero, Vector2.zero, Color.clear, () =>
            {
                // Recheck current inventory at the point of use, including after a fusion spends an extra copy.
                if (session != null && session.IsOwnedAndPlayable(id)) ShowToy(id);
            }, out _, 14);
            tile.Button.name = "Owned toy · " + id;
            tile.Picture = Rect("Toy portrait field", tile.Button.transform); Surface(tile.Picture, PockleTheme.FieldFor(id), 18);
            tile.Picture.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var image = Rect("Owned portrait", tile.Picture); Stretch(image);
            tile.Portrait = image.gameObject.AddComponent<RawImage>(); tile.Portrait.raycastTarget = false;
            tile.Portrait.enabled = false;
            tile.Badge = Rect("Copies", tile.Picture); Surface(tile.Badge, new Color(1, 1, 1, .94f), 10);
            tile.Count = Label("", tile.Badge, 11, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero); Stretch(tile.Count.rectTransform);
            tile.Name = Label(PersonalityBook.FinishName(toy), tile.Button.transform, 14, Ink, FontStyle.Bold, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
            tile.Name.supportRichText = false; tile.Name.resizeTextMinSize = 11;
            tile.Detail = Label(character + " · " + tier, tile.Button.transform, 11, MutedInk, FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
            tile.Detail.resizeTextMinSize = 10;
            ownedShelfTiles.Add(tile);
        }

        private bool MatchesOwnedShelf(OwnedShelfTile tile)
        {
            if (ownedShelfFilter == OwnedShelfFilter.Fusions && !tile.Fusion) return false;
            if (ownedShelfFilter == OwnedShelfFilter.Extras && session.GetOwnedCount(tile.Id) < 2) return false;
            string query = ownedShelfSearch.text.Trim();
            return query.Length == 0 || tile.Search.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private float LayoutOwnedShelf(float width)
        {
            RefreshOwnedShelf();
            ownedShelfRoot.gameObject.SetActive(!ownedShelfLiveView); ownedShelfReturn.gameObject.SetActive(ownedShelfLiveView);
            if (ownedShelfLiveView)
            {
                LeftLabel(ownedShelfReturn.GetComponent<RectTransform>(), Vector2.zero, new Vector2(width, 44));
                return LayoutLivingShelf(width, 58);
            }
            float y = 0;
            LeftLabel(ownedShelfSummary.rectTransform, new Vector2(0, -y), new Vector2(width, 24)); y += 34;
            float half = (width - 10) / 2;
            LeftLabel(ownedShelfWalk.GetComponent<RectTransform>(), new Vector2(0, -y), new Vector2(half, 74));
            LeftLabel(ownedShelfShop.GetComponent<RectTransform>(), new Vector2(half + 10, -y), new Vector2(half, 74)); y += 88;
            LeftLabel(ownedShelfSearch.GetComponent<RectTransform>(), new Vector2(0, -y), new Vector2(width, 46)); y += 58;
            LeftLabel(ownedShelfFilters, new Vector2(0, -y), new Vector2(width, 44));
            float chip = (width - 16) / 3;
            for (int i = 0; i < 3; i++)
            {
                bool selected = (int)ownedShelfFilter == i;
                LeftLabel(ownedShelfFilterFaces[i].rectTransform, new Vector2(i * (chip + 8), 0), new Vector2(chip, 44));
                ownedShelfFilterFaces[i].color = selected ? Ink : Quiet;
                ownedShelfFilterFaces[i].GetComponentInChildren<Text>().color = selected ? Paper : Ink;
            }
            y += 58;
            const float gap = 10;
            int columns = width < 300 ? 2 : 3, shown = 0;
            float tileWidth = (width - gap * (columns - 1)) / columns, tileHeight = tileWidth + 58;
            foreach (var tile in ownedShelfTiles)
            {
                bool visible = session != null && MatchesOwnedShelf(tile); tile.Button.gameObject.SetActive(visible);
                if (!visible) continue;
                LeftLabel(tile.Button.GetComponent<RectTransform>(), new Vector2(shown % columns * (tileWidth + gap), -shown / columns * (tileHeight + gap)), new Vector2(tileWidth, tileHeight));
                LeftLabel(tile.Picture, Vector2.zero, new Vector2(tileWidth, tileWidth));
                LeftLabel(tile.Name.rectTransform, new Vector2(1, -tileWidth - 7), new Vector2(tileWidth - 2, 27));
                LeftLabel(tile.Detail.rectTransform, new Vector2(1, -tileWidth - 36), new Vector2(tileWidth - 2, 20));
                tile.Badge.anchorMin = tile.Badge.anchorMax = tile.Badge.pivot = new Vector2(1, 1);
                tile.Badge.anchoredPosition = new Vector2(-5, -5); tile.Badge.sizeDelta = new Vector2(Mathf.Min(tileWidth - 10, Mathf.Max(48, tile.Count.preferredWidth + 14)), 21);
                shown++;
            }
            float gridHeight = ((shown + columns - 1) / columns) * (tileHeight + gap);
            LeftLabel(ownedShelfGrid, new Vector2(0, -y), new Vector2(width, gridHeight));
            ownedShelfEmpty.gameObject.SetActive(shown == 0);
            ownedShelfEmpty.text = ownedShelfTiles.Count == 0 ? "Your first little friend is waiting.\nWalk for a box or explore the shop."
                : ownedShelfSearch.text.Trim().Length > 0 ? "No toys match that search.\nTry another name or filter."
                : ownedShelfFilter == OwnedShelfFilter.Fusions ? "Your fusions will live here.\nCombine two spares of the same character."
                : "No extra copies yet.\nYour first copy always stays on your shelf.";
            LeftLabel(ownedShelfEmpty.rectTransform, new Vector2(8, -y), new Vector2(width - 16, 100));
            y += shown == 0 ? 112 : gridHeight + 8;
            LeftLabel(ownedShelfFuse.GetComponent<RectTransform>(), new Vector2(0, -y), new Vector2(width, 76)); y += 88;
            LeftLabel(ownedShelfLive.GetComponent<RectTransform>(), new Vector2(0, -y), new Vector2(width, 44)); y += 56;
            LeftLabel(ownedShelfRoot, Vector2.zero, new Vector2(width, y));
            return y;
        }

        /// <summary>One new still per frame, only for visible tiles. No live actor per owned toy; the cache is bounded.</summary>
        private void RefreshVisibleOwnedShelfPortraits()
        {
            if (page != AppPage.Shelf || ownedShelfLiveView || ownedShelfRoot == null || session == null || browsing == null) return;
            var viewport = browsing.GetComponent<ScrollRect>().viewport;
            bool rendered = ownedShelfRenderedFrame == Time.frameCount;
            foreach (var tile in ownedShelfTiles)
            {
                if (!tile.Button.gameObject.activeInHierarchy) continue;
                tile.Picture.GetWorldCorners(ownedShelfCorners);
                float bottom = viewport.InverseTransformPoint(ownedShelfCorners[0]).y;
                float top = viewport.InverseTransformPoint(ownedShelfCorners[1]).y;
                if (top < viewport.rect.yMin || bottom > viewport.rect.yMax) continue;
                if (ownedShelfPortraits.TryGetValue(tile.Id, out var cached))
                {
                    tile.Portrait.texture = cached; tile.Portrait.enabled = cached != null;
                    ownedShelfPortraitOrder.Remove(tile.Id); ownedShelfPortraitOrder.AddLast(tile.Id);
                    continue;
                }
                if (rendered) continue;
                rendered = true; ownedShelfRenderedFrame = Time.frameCount;
                Texture2D picture = null; RenderTexture source = null, small = null; var previous = RenderTexture.active;
                try
                {
                    source = ToyPortrait.Render(tile.Id, PockleTheme.FieldFor(tile.Id));
                    small = RenderTexture.GetTemporary(256, 256, 0, RenderTextureFormat.ARGB32);
                    Graphics.Blit(source, small); RenderTexture.active = small;
                    picture = new Texture2D(256, 256, TextureFormat.RGBA32, false) { name = "Owned shelf · " + tile.Id, filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
                    picture.ReadPixels(new Rect(0, 0, 256, 256), 0, 0); picture.Apply(false, true);
                }
                catch (Exception error) { if (picture != null) Destroy(picture); picture = null; Debug.LogWarning("Pockle owned portrait: " + error.Message); }
                finally { RenderTexture.active = previous; if (small != null) RenderTexture.ReleaseTemporary(small); if (source != null) { source.Release(); Destroy(source); } }
                ownedShelfPortraits[tile.Id] = picture; ownedShelfPortraitOrder.AddLast(tile.Id);
                tile.Portrait.texture = picture; tile.Portrait.enabled = picture != null;
                while (ownedShelfPortraits.Count > OwnedShelfPortraitLimit)
                {
                    string oldest = ownedShelfPortraitOrder.First.Value; ownedShelfPortraitOrder.RemoveFirst();
                    var expired = ownedShelfPortraits[oldest]; ownedShelfPortraits.Remove(oldest);
                    foreach (var oldTile in ownedShelfTiles) if (oldTile.Id == oldest) { oldTile.Portrait.texture = null; oldTile.Portrait.enabled = false; }
                    if (expired != null) Destroy(expired);
                }
            }
        }

        private void ReleaseOwnedShelfPortraits()
        {
            foreach (var portrait in ownedShelfPortraits.Values) if (portrait != null) Destroy(portrait);
            ownedShelfPortraits.Clear(); ownedShelfPortraitOrder.Clear();
        }
    }
}
