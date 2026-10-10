using System;
using System.Collections.Generic;
using System.Globalization;
using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>Series browser and bounded static lineup. Planned art never substitutes an owned Pip.</summary>
    public sealed partial class PrototypeHud
    {
        private string selectedCollectionId = "pip";
        private CollectionDefinition SelectedCollection => ToyCatalog.TryGetCollection(selectedCollectionId, out var selected)
            ? selected : ToyCatalog.Collections[0];
        private readonly List<CollectionDefinition> browsingCollections = new List<CollectionDefinition>();
        private readonly List<Button> collectionCards = new List<Button>();
        private readonly List<Text> collectionCardDetails = new List<Text>();
        private readonly List<Button> lineupTiles = new List<Button>();
        private readonly List<Text> lineupNames = new List<Text>();
        private readonly List<Text> lineupStates = new List<Text>();
        private readonly List<Text> lineupMysteries = new List<Text>();
        private readonly List<RawImage> lineupPictures = new List<RawImage>();
        private Button collectionBoxes, collectionWalking;
        private Text collectionProgress, collectionNote;

        private static string CollectionCaption(CollectionDefinition collection)
        {
            if (collection.SeriesNumber == 0) return "Coming soon";
            string caption = "Series " + collection.SeriesNumber;
            if (collection.ReleaseDateUtc.HasValue)
                caption += " · " + collection.ReleaseDateUtc.Value.ToString("MMMM yyyy", CultureInfo.CurrentCulture);
            return caption;
        }

        private void BuildCollections()
        {
            browsingCollections.AddRange(ToyCatalog.Collections);
            // Approved dated series first, newest first; numbered prototypes, then unassigned concepts.
            browsingCollections.Sort((a, b) =>
            {
                int scheduled = b.HasPublishedSchedule.CompareTo(a.HasPublishedSchedule);
                if (scheduled != 0) return scheduled;
                int dated = Nullable.Compare(b.ReleaseDateUtc, a.ReleaseDateUtc);
                if (dated != 0) return dated;
                int numbered = (b.SeriesNumber > 0).CompareTo(a.SeriesNumber > 0);
                if (numbered != 0) return numbered;
                int series = b.SeriesNumber.CompareTo(a.SeriesNumber);
                return series != 0 ? series : string.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal);
            });
            foreach (var collection in browsingCollections)
            {
                string id = collection.Id;
                Color field = CatalogUnityColor(collection.FieldColor);
                var card = RowButton(collection.DisplayName, "", "Shelf", collectionsRoot, () => OpenCollection(id), out Text detail, true);
                card.name = "Collection · " + id;
                card.GetComponent<Image>().color = field;
                collectionCards.Add(card); collectionCardDetails.Add(detail);
            }
            collectionProgress = Label("", collectionRoot, PockleTheme.HeadingSize, Ink, FontStyle.Bold,
                TextAnchor.MiddleLeft, Vector2.zero, new Vector2(300, 32));
            collectionNote = Label("", collectionRoot, PockleTheme.BodySize, MutedInk, FontStyle.Normal,
                TextAnchor.UpperLeft, Vector2.zero, new Vector2(300, 60));
            collectionBoxes = CreateButton("View boxes", collectionRoot, Vector2.zero, new Vector2(300, 52), Primary,
                () => Navigate(AppPage.Boxes), out _);
            collectionWalking = CreateButton("Walk for one", collectionRoot, Vector2.zero, new Vector2(300, 48), Quiet,
                () => Navigate(AppPage.Rewards), out _);
            RebuildLineup(); RefreshCollections();
        }

        private void OpenCollection(string id)
        {
            if (!ToyCatalog.TryGetCollection(id, out _)) return;
            bool changed = selectedCollectionId != id;
            selectedCollectionId = id;
            if (changed) RebuildLineup();
            Navigate(AppPage.Collection);
            if (changed) { navigator.SaveScroll(0); content.anchoredPosition = Vector2.zero; }
            RefreshCollections();
        }

        private void RebuildLineup()
        {
            // No render targets are created here: every owned playable tile shares the four shelf portraits.
            foreach (var tile in lineupTiles) { tile.gameObject.SetActive(false); Destroy(tile.gameObject); }
            lineupTiles.Clear(); lineupNames.Clear(); lineupStates.Clear(); lineupMysteries.Clear(); lineupPictures.Clear();
            foreach (string member in SelectedCollection.Members)
            {
                string id = member;
                var tile = CreateButton("???", collectionRoot, Vector2.zero, new Vector2(100, 144), Quiet,
                    () => ShowToy(id), out _, 14, false);
                tile.name = "Collectible · " + id;
                lineupTiles.Add(tile);
                var title = tile.GetComponentInChildren<Text>(); title.alignment = TextAnchor.MiddleCenter;
                lineupNames.Add(title);
                lineupStates.Add(Label("", tile.transform, PockleTheme.SmallSize, MutedInk, FontStyle.Normal,
                    TextAnchor.MiddleCenter, Vector2.zero, new Vector2(100, 24)));
                lineupMysteries.Add(Label("?", tile.transform, 40, PockleTheme.FillDeep, FontStyle.Bold,
                    TextAnchor.MiddleCenter, Vector2.zero, new Vector2(80, 80)));
                var portraitRect = Rect("Lineup portrait", tile.transform);
                var picture = portraitRect.gameObject.AddComponent<RawImage>(); picture.raycastTarget = false;
                if (PipVariants.TryFromCollectibleId(id, out var variant)) picture.texture = previews[(int)variant];
                lineupPictures.Add(picture);
            }
        }

        private int FoundCount(CollectionDefinition collection)
        {
            int count = 0;
            if (session != null) foreach (string id in collection.Members) if (session.GetOwnedCount(id) > 0) count++;
            return count;
        }

        private void RefreshCollections()
        {
            if (collectionProgress == null) return;
            for (int i = 0; i < browsingCollections.Count; i++)
            {
                var collection = browsingCollections[i];
                collectionCardDetails[i].text = CollectionCaption(collection) + " · " + FoundCount(collection) + " of " + collection.Members.Count + " found";
            }
            var selected = SelectedCollection;
            collectionProgress.text = FoundCount(selected) + " of " + selected.Members.Count + " found";
            bool offers = false;
            foreach (var offer in BoxCatalog.Offers) if (offer.CollectionId == selected.Id) offers = true;
            collectionBoxes.interactable = offers;
            collectionWalking.interactable = ToyCatalog.TryGetCollectionForPool(ToyCatalog.DailyPoolId, out var walking)
                && walking.Id == selected.Id;
            collectionNote.text = offers ? "More varieties are on the way. Explore today's boxes to see what you can find."
                : "This character is coming later. Your collection will be waiting here.";
            for (int i = 0; i < lineupTiles.Count; i++)
            {
                string id = selected.Members[i];
                bool owned = session != null && session.GetOwnedCount(id) > 0;
                bool known = ToyCatalog.TryGetCollectible(id, out var collectible);
                bool playable = session != null && session.TryGetPlayableVariant(id, out _);
                lineupTiles[i].interactable = playable;
                lineupNames[i].text = owned && known ? collectible.FinishDisplayName : "???";
                lineupStates[i].text = !known || !collectible.Available ? "Coming soon" : owned ? "Yours to play" : "Not found yet";
                lineupPictures[i].gameObject.SetActive(playable);
                lineupMysteries[i].gameObject.SetActive(!playable);
                lineupTiles[i].GetComponent<Image>().color = playable ? PockleTheme.FieldFor(id) : PockleTheme.Mystery;
            }
        }

        private float LayoutCollections(float width)
        {
            float y = 0;
            foreach (var card in collectionCards)
            {
                LeftLabel(card.GetComponent<RectTransform>(), new Vector2(0, -y), new Vector2(width, 76));
                y += 88;
            }
            TopCentered(collectionsRoot, Vector2.zero, new Vector2(width, y));
            return y;
        }

        private float LayoutCollection(float width)
        {
            LeftLabel(collectionProgress.rectTransform, Vector2.zero, new Vector2(width, 32));
            LeftLabel(collectionNote.rectTransform, new Vector2(0, -40), new Vector2(width, 64));
            int columns = width >= 450 ? 4 : width >= 300 ? 3 : 2;
            const float gap = 10;
            float tileWidth = (width - (columns - 1) * gap) / columns;
            float tileHeight = tileWidth + 64;
            float y = 116;
            for (int i = 0; i < lineupTiles.Count; i++)
            {
                LeftLabel(lineupTiles[i].GetComponent<RectTransform>(), new Vector2((i % columns) * (tileWidth + gap),
                    -y - (i / columns) * (tileHeight + gap)), new Vector2(tileWidth, tileHeight));
                float art = tileWidth - 16;
                TopCentered(lineupPictures[i].rectTransform, new Vector2(0, -8), new Vector2(art, art));
                TopCentered(lineupMysteries[i].rectTransform, new Vector2(0, -8), new Vector2(art, art));
                var name = lineupNames[i].rectTransform;
                name.anchorMin = name.anchorMax = name.pivot = new Vector2(.5f, 0);
                name.anchoredPosition = new Vector2(0, 30); name.sizeDelta = new Vector2(tileWidth - 12, 24);
                TopCentered(lineupStates[i].rectTransform, new Vector2(0, -tileHeight + 28), new Vector2(tileWidth - 12, 24));
            }
            int rows = (lineupTiles.Count + columns - 1) / columns;
            y += rows * (tileHeight + gap) + 8;
            LeftLabel(collectionBoxes.GetComponent<RectTransform>(), new Vector2(0, -y), new Vector2(width, 52)); y += 64;
            LeftLabel(collectionWalking.GetComponent<RectTransform>(), new Vector2(0, -y), new Vector2(width, 48)); y += 60;
            TopCentered(collectionRoot, Vector2.zero, new Vector2(width, y));
            return y;
        }

        private static Color CatalogUnityColor(CatalogColor colour) => new Color(colour.Red, colour.Green, colour.Blue, 1f);
    }
}
