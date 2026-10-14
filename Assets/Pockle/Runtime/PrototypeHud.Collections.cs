using System;
using System.Collections.Generic;
using System.Globalization;
using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>Series browser and bounded animated lineup. Planned art never substitutes an owned Pip.</summary>
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
        private readonly List<CharacterSilhouetteGraphic> lineupMysteries = new List<CharacterSilhouetteGraphic>();
        private readonly List<ShelfPreviewRuntime.Binding> lineupBindings = new List<ShelfPreviewRuntime.Binding>();
        private readonly List<RawImage> lineupPictures = new List<RawImage>();
        private Button collectionBoxes, collectionWalking;
        private Text collectionProgress, collectionNote;
        // v2 canvas: list cards with a portrait; a coloured hero, "Who's inside", and a secret card on each collection.
        private readonly List<RawImage> collectionThumbs = new List<RawImage>();
        private readonly List<CharacterSilhouetteGraphic> collectionThumbShapes = new List<CharacterSilhouetteGraphic>();
        private readonly List<Text> collectionCardStatus = new List<Text>();
        private readonly Dictionary<string, Texture> collectionArt = new Dictionary<string, Texture>();
        private RectTransform collectionHero, lineupHeading, secretCard;
        private Text heroTitle, heroCaption, heroCount, secretTitle, secretBody;
        private Image heroFill;
        private RawImage heroPortrait;
        private CharacterSilhouetteGraphic heroShape, secretShape;

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
                // A neutral row with the character on its colour field, the series line, and progress.
                var card = CreateButton(collection.DisplayName, collectionsRoot, Vector2.zero, new Vector2(300, 96), Quiet, () => OpenCollection(id), out var face, 18, false);
                SetRadius(face, 24);
                card.name = "Collection · " + id;
                var title = card.GetComponentInChildren<Text>(); title.alignment = TextAnchor.LowerLeft;
                title.rectTransform.offsetMin = new Vector2(100, 50); title.rectTransform.offsetMax = new Vector2(-40, -14);
                var thumb = Rect("Collection portrait field", card.transform); Surface(thumb, field, 20);
                thumb.anchorMin = thumb.anchorMax = thumb.pivot = new Vector2(0, .5f); thumb.anchoredPosition = new Vector2(10, 0); thumb.sizeDelta = new Vector2(76, 76);
                var shapeRect = Rect("Collection silhouette", thumb); Stretch(shapeRect); shapeRect.offsetMin = new Vector2(10, 6); shapeRect.offsetMax = new Vector2(-10, -10);
                var shape = shapeRect.gameObject.AddComponent<CharacterSilhouetteGraphic>(); shape.Configure(collection.CharacterId); shape.color = new Color(Ink.r, Ink.g, Ink.b, .18f);
                var portraitRect = Rect("Collection portrait", thumb); Stretch(portraitRect);
                var portrait = portraitRect.gameObject.AddComponent<RawImage>(); portrait.raycastTarget = false; portrait.enabled = false;
                var detail = Label("", card.transform, PockleTheme.SmallSize, MutedInk, FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
                Stretch(detail.rectTransform); detail.rectTransform.offsetMin = new Vector2(100, 30); detail.rectTransform.offsetMax = new Vector2(-40, -48);
                var status = Label("", card.transform, PockleTheme.SmallSize, MutedInk, FontStyle.Bold, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
                Stretch(status.rectTransform); status.rectTransform.offsetMin = new Vector2(100, 10); status.rectTransform.offsetMax = new Vector2(-40, -68);
                var next = AddIcon(card.transform, "Next", 18, MutedInk);
                next.anchorMin = next.anchorMax = next.pivot = new Vector2(1, .5f); next.anchoredPosition = new Vector2(-16, 0);
                collectionCards.Add(card); collectionCardDetails.Add(detail); collectionCardStatus.Add(status);
                collectionThumbs.Add(portrait); collectionThumbShapes.Add(shape);
            }

            // One collection: a hero on the collection's colour, like the canvas.
            collectionHero = Rect("Collection hero", collectionRoot); Surface(collectionHero, Quiet, 32);
            heroTitle = Label("", collectionHero, 34, Ink, FontStyle.Bold, TextAnchor.LowerLeft, Vector2.zero, new Vector2(200, 80));
            heroTitle.resizeTextMinSize = 24;
            heroCaption = Label("", collectionHero, PockleTheme.CaptionSize + 1, MutedInk, FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, new Vector2(200, 44));
            var heroArt = Rect("Collection hero art", collectionHero);
            var heroShapeRect = Rect("Hero silhouette", heroArt); Stretch(heroShapeRect);
            heroShape = heroShapeRect.gameObject.AddComponent<CharacterSilhouetteGraphic>(); heroShape.color = new Color(Ink.r, Ink.g, Ink.b, .16f);
            var heroPortraitRect = Rect("Hero portrait", heroArt); Stretch(heroPortraitRect);
            heroPortrait = heroPortraitRect.gameObject.AddComponent<RawImage>(); heroPortrait.raycastTarget = false; heroPortrait.enabled = false;
            var track = Rect("Collection progress track", collectionHero); Surface(track, new Color(1, 1, 1, .6f), 5);
            heroFill = ProgressFill(track, Primary);
            heroCount = Label("", collectionHero, PockleTheme.CaptionSize + 1, Ink, FontStyle.Bold, TextAnchor.MiddleRight, Vector2.zero, new Vector2(120, 24));
            lineupHeading = Heading("Who's inside", collectionRoot);
            secretCard = Rect("Secret card", collectionRoot); Surface(secretCard, Primary, 24);
            var secretArt = Rect("Secret silhouette", secretCard);
            secretArt.anchorMin = secretArt.anchorMax = secretArt.pivot = new Vector2(0, .5f); secretArt.anchoredPosition = new Vector2(18, 0); secretArt.sizeDelta = new Vector2(64, 64);
            secretShape = secretArt.gameObject.AddComponent<CharacterSilhouetteGraphic>(); secretShape.color = new Color(1, 1, 1, .2f);
            secretTitle = Label("", secretCard, PockleTheme.BodySize + 2, PockleTheme.OnPlum, FontStyle.Bold, TextAnchor.LowerLeft, Vector2.zero, Vector2.zero);
            Stretch(secretTitle.rectTransform); secretTitle.rectTransform.offsetMin = new Vector2(98, 52); secretTitle.rectTransform.offsetMax = new Vector2(-16, -14);
            secretBody = Label("", secretCard, PockleTheme.CaptionSize, PockleTheme.OnPlumMuted, FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
            Stretch(secretBody.rectTransform); secretBody.rectTransform.offsetMin = new Vector2(98, 10); secretBody.rectTransform.offsetMax = new Vector2(-16, -54);
            collectionProgress = Label("", collectionRoot, PockleTheme.HeadingSize, Ink, FontStyle.Bold,
                TextAnchor.MiddleLeft, Vector2.zero, new Vector2(300, 32));
            collectionProgress.gameObject.SetActive(false);
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
            // Recycle visible actors; the grid never owns one camera/target per collectible.
            foreach (var binding in lineupBindings) shelfPreviews.Unregister(binding);
            lineupBindings.Clear();
            foreach (var tile in lineupTiles) { tile.gameObject.SetActive(false); Destroy(tile.gameObject); }
            lineupTiles.Clear(); lineupNames.Clear(); lineupStates.Clear(); lineupMysteries.Clear(); lineupPictures.Clear();
            foreach (string member in SelectedCollection.Members)
            {
                string id = member;
                var tile = CreateButton("???", collectionRoot, Vector2.zero, new Vector2(100, 144), Quiet,
                    () => ShowToy(id), out _, 14, false);
                tile.name = "Collectible · " + id;
                var colours = tile.colors; colours.disabledColor = Color.white; tile.colors = colours;
                lineupTiles.Add(tile);
                var title = tile.GetComponentInChildren<Text>(); title.alignment = TextAnchor.MiddleCenter;
                lineupNames.Add(title);
                lineupStates.Add(Label("", tile.transform, PockleTheme.SmallSize, MutedInk, FontStyle.Normal,
                    TextAnchor.MiddleCenter, Vector2.zero, new Vector2(100, 24)));
                var silhouetteRect = Rect("Undiscovered silhouette", tile.transform);
                var silhouette = silhouetteRect.gameObject.AddComponent<CharacterSilhouetteGraphic>();
                silhouette.color = PockleTheme.PlumSoft; lineupMysteries.Add(silhouette);
                var portraitRect = Rect("Lineup portrait", tile.transform);
                var picture = portraitRect.gameObject.AddComponent<RawImage>(); picture.raycastTarget = false;
                if (PipVariants.TryFromCollectibleId(id, out var variant)) picture.texture = previews[(int)variant];
                lineupPictures.Add(picture);
                lineupBindings.Add(shelfPreviews.Register(picture, silhouette, id));
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
            bool showingList = page == AppPage.Collections, showingOne = page == AppPage.Collection;
            for (int i = 0; i < browsingCollections.Count; i++)
            {
                var collection = browsingCollections[i];
                int found = FoundCount(collection);
                collectionCardDetails[i].text = CollectionCaption(collection);
                bool complete = found == collection.Members.Count && found > 0;
                collectionCardStatus[i].text = complete ? "Complete, " + found + " of " + found : found == 0 ? "Not started" : found + " of " + collection.Members.Count + " found";
                collectionCardStatus[i].color = complete ? PockleTheme.Success : MutedInk;
                if (showingList) ShowCollectionArt(collection, collectionThumbs[i], collectionThumbShapes[i]);
            }
            var selected = SelectedCollection;
            int selectedFound = FoundCount(selected);
            collectionProgress.text = selectedFound + " of " + selected.Members.Count + " found";
            collectionHero.GetComponent<Image>().color = CatalogUnityColor(selected.FieldColor);
            heroTitle.text = selected.DisplayName;
            heroCaption.text = CollectionCaption(selected) + ". " + selected.Members.Count + " to find" + (selected.HasSecret ? ", plus one secret." : ".");
            heroCount.text = selectedFound + " of " + selected.Members.Count + " found";
            heroFill.fillAmount = selected.Members.Count == 0 ? 0 : (float)selectedFound / selected.Members.Count;
            if (showingOne) ShowCollectionArt(selected, heroPortrait, heroShape);
            secretCard.gameObject.SetActive(selected.HasSecret);
            if (selected.HasSecret)
            {
                bool foundSecret = session != null && session.GetOwnedCount(selected.SecretId) > 0;
                secretShape.Configure(selected.CharacterId, !foundSecret);
                string secretName = ToyCatalog.TryGetCollectible(selected.SecretId, out var secretToy) ? FinishName(secretToy) : "the secret";
                secretTitle.text = foundSecret ? "You found " + secretName : "A secret hides here";
                secretBody.text = foundSecret ? "It's the rarest one in " + selected.DisplayName + "." : "It turns up rarely. Keep opening " + selected.DisplayName + " boxes.";
            }
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
                bool playable = session != null && session.IsOwnedAndPlayable(id);
                lineupTiles[i].interactable = playable;
                lineupNames[i].text = owned && known ? collectible.FinishDisplayName : "???";
                lineupStates[i].text = !known || !collectible.Available ? "Coming soon" : owned ? "Yours to play" : "Not found yet";
                shelfPreviews.SetFound(lineupBindings[i], playable);
                lineupTiles[i].GetComponent<Image>().color = playable ? PockleTheme.FieldFor(id) : PockleTheme.Mystery;
            }
        }

        private float LayoutCollections(float width)
        {
            float y = 0;
            foreach (var card in collectionCards)
            {
                LeftLabel(card.GetComponent<RectTransform>(), new Vector2(0, -y), new Vector2(width, 96));
                y += 106;
            }
            TopCentered(collectionsRoot, Vector2.zero, new Vector2(width, y));
            return y;
        }

        private float LayoutCollection(float width)
        {
            // Hero: title and caption on the left, the character on the right, progress along the bottom.
            const float heroHeight = 236;
            LeftLabel(collectionHero, Vector2.zero, new Vector2(width, heroHeight));
            float heroArtSize = Mathf.Min(150, width * .4f);
            var heroArt = heroPortrait.rectTransform.parent as RectTransform;
            heroArt.anchorMin = heroArt.anchorMax = heroArt.pivot = new Vector2(1, 1); heroArt.anchoredPosition = new Vector2(-12, -18); heroArt.sizeDelta = new Vector2(heroArtSize, heroArtSize);
            float text = width - heroArtSize - 34;
            LeftLabel(heroTitle.rectTransform, new Vector2(22, -24), new Vector2(text, 84));
            LeftLabel(heroCaption.rectTransform, new Vector2(22, -112), new Vector2(text, 64));
            var track = heroFill.rectTransform.parent as RectTransform;
            LeftLabel(track, new Vector2(22, -(heroHeight - 32)), new Vector2(width - 44 - 128, 10));
            LeftLabel(heroCount.rectTransform, new Vector2(width - 22 - 120, -(heroHeight - 40)), new Vector2(120, 24));
            float y = heroHeight + 20;
            LeftLabel(lineupHeading, new Vector2(0, -y), new Vector2(width, 30)); y += 40;
            LeftLabel(collectionNote.rectTransform, new Vector2(0, -y), new Vector2(width, 0));
            collectionNote.gameObject.SetActive(false);
            int columns = width >= 450 ? 4 : width >= 300 ? 3 : 2;
            const float gap = 10;
            float tileWidth = (width - (columns - 1) * gap) / columns;
            float tileHeight = tileWidth + 64;
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
            y += rows * (tileHeight + gap) + 4;
            if (secretCard.gameObject.activeSelf) { LeftLabel(secretCard, new Vector2(0, -y), new Vector2(width, 104)); y += 116; }
            LeftLabel(collectionBoxes.GetComponent<RectTransform>(), new Vector2(0, -y), new Vector2(width, 52)); y += 64;
            LeftLabel(collectionWalking.GetComponent<RectTransform>(), new Vector2(0, -y), new Vector2(width, 48)); y += 60;
            TopCentered(collectionRoot, Vector2.zero, new Vector2(width, y));
            return y;
        }

        /// <summary>The collection's face: an owned member's portrait, else the first member with art, else its silhouette.</summary>
        private void ShowCollectionArt(CollectionDefinition collection, RawImage portrait, CharacterSilhouetteGraphic shape)
        {
            if (!collectionArt.TryGetValue(collection.Id, out var art))
            {
                art = null;
                string lead = null;
                foreach (string id in collection.Members) if (session != null && session.IsOwnedAndPlayable(id) && id != collection.SecretId) { lead = id; break; }
                if (lead == null) foreach (string id in collection.Members) if (id != collection.SecretId && ToyCatalog.TryGetCollectible(id, out var toy) && toy.Available) { lead = id; break; }
                // Pips reuse the shelf's portraits; other characters get one small render each.
                if (lead != null && PipVariants.TryFromCollectibleId(lead, out var legacy)) art = previews[(int)legacy];
                else if (lead != null)
                {
                    try { art = ToyPortrait.Render(lead, CatalogUnityColor(collection.FieldColor)); }
                    catch (Exception error) { Debug.LogWarning("Pockle collection portrait: " + error.Message); }
                }
                collectionArt[collection.Id] = art;
            }
            portrait.texture = art; portrait.enabled = art != null;
            shape.Configure(collection.CharacterId); shape.enabled = art == null;
        }

        private static Color CatalogUnityColor(CatalogColor colour) => new Color(colour.Red, colour.Green, colour.Blue, 1f);
    }
}
