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
        private readonly List<RectTransform> lineupMysteryHolders = new List<RectTransform>();
        private readonly List<RectTransform> lineupBubbles = new List<RectTransform>();
        private readonly List<ShelfPreviewRuntime.Binding> lineupBindings = new List<ShelfPreviewRuntime.Binding>();
        private readonly List<RawImage> lineupPictures = new List<RawImage>();
        private Button collectionBoxes, collectionWalking;
        private Text collectionProgress, collectionNote;
        // Box-first collections (canvas row "Collections, box first"): each card is the collection's world painting with
        // its box; each collection page wears the collection's banner, accent and ink.
        private readonly List<RawImage> collectionThumbs = new List<RawImage>();
        private readonly List<CharacterSilhouetteGraphic> collectionThumbShapes = new List<CharacterSilhouetteGraphic>();
        private readonly List<Text> collectionCardStatus = new List<Text>();
        private readonly List<Text> collectionCardTitles = new List<Text>();
        private readonly List<RawImage> collectionBanners = new List<RawImage>();
        private readonly List<RawImage> collectionBoxArt = new List<RawImage>();
        private readonly List<RectTransform> collectionStandIns = new List<RectTransform>();
        private readonly List<Image> collectionFills = new List<Image>();
        private readonly List<RawImage> collectionFades = new List<RawImage>();
        private readonly List<Image> collectionTracks = new List<Image>();
        private readonly Dictionary<string, Texture> collectionArt = new Dictionary<string, Texture>();
        private RectTransform collectionHero, lineupHeading, secretCard, progressCard, heroStandIn, halfwayTick;
        private Text heroTitle, heroCaption, heroCount, heroNote, secretTitle, secretBody;
        private Image heroFill;
        private RawImage heroPortrait, heroBanner, heroBox, heroFade;
        private BoxHype heroHype;
        private CharacterSilhouetteGraphic heroShape, secretShape;
        private float heroBoxAspect = 1f;
        // Still portraits for lineup toys that aren't Pips, so a slot never shows blank white while the four live actors are busy.
        private readonly Dictionary<string, Texture> lineupPortraits = new Dictionary<string, Texture>();

        private static string CollectionCaption(CollectionDefinition collection)
        {
            if (collection.SeriesNumber == 0) return IsPlayable(collection) ? "New collection" : "Coming soon";
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
                var card = CreateButton(collection.DisplayName, collectionsRoot, Vector2.zero, new Vector2(300, 200), field, () => OpenCollection(id), out var face, 34, false);
                SetRadius(face, 30);
                card.gameObject.AddComponent<Mask>().showMaskGraphic = true; // rounded corners clip the painting
                card.name = "Collection · " + id;
                // Painting, then a soft see-through plum shade on the left for white words, then the box (owner, October 14:
                // the old milk fade hid too much of the cover art). Cards without a painting keep dark words on their colour.
                var banner = Rect("Collection banner", card.transform); Stretch(banner); banner.SetAsFirstSibling();
                var bannerImage = banner.gameObject.AddComponent<RawImage>(); bannerImage.raycastTarget = false;
                var fade = Rect("Collection fade", card.transform); Stretch(fade); fade.SetSiblingIndex(1);
                var fadeImage = fade.gameObject.AddComponent<RawImage>(); fadeImage.texture = UiShapes.FadeRight; fadeImage.raycastTarget = false;
                fade.anchorMax = new Vector2(.72f, 1);
                var title = card.GetComponentInChildren<Text>(); title.alignment = TextAnchor.UpperLeft; title.color = Ink;
                title.rectTransform.anchorMin = new Vector2(0, 1); title.rectTransform.anchorMax = new Vector2(0, 1); title.rectTransform.pivot = new Vector2(0, 1);
                title.rectTransform.anchoredPosition = new Vector2(20, -42); title.rectTransform.sizeDelta = new Vector2(190, 42);
                // The box, or until its render exists, the character on the collection's colour.
                var boxRect = Rect("Collection box", card.transform);
                var boxImage = boxRect.gameObject.AddComponent<RawImage>(); boxImage.raycastTarget = false;
                BoxHype.Add(boxImage, Glowing(CollectionLook.Accent(collection)), browsingCollections.IndexOf(collection) * 1.3f);
                var standIn = Rect("Collection portrait field", card.transform); Surface(standIn, field, 24);
                standIn.gameObject.AddComponent<Mask>().showMaskGraphic = true;
                var shapeRect = Rect("Collection silhouette", standIn); Stretch(shapeRect); shapeRect.offsetMin = new Vector2(14, 8); shapeRect.offsetMax = new Vector2(-14, -14);
                var shape = shapeRect.gameObject.AddComponent<CharacterSilhouetteGraphic>(); shape.Configure(collection.CharacterId); shape.color = new Color(Ink.r, Ink.g, Ink.b, .18f);
                var portraitRect = Rect("Collection portrait", standIn); Stretch(portraitRect);
                var portrait = portraitRect.gameObject.AddComponent<RawImage>(); portrait.raycastTarget = false; portrait.enabled = false;
                var detail = Label("", card.transform, PockleTheme.SmallSize - 1, Ink, FontStyle.Bold, TextAnchor.UpperLeft, Vector2.zero, new Vector2(190, 18));
                LeftLabel(detail.rectTransform, new Vector2(20, -20), new Vector2(190, 18));
                var status = Label("", card.transform, PockleTheme.SmallSize, Ink, FontStyle.Bold, TextAnchor.LowerLeft, Vector2.zero, new Vector2(170, 20));
                status.rectTransform.anchorMin = status.rectTransform.anchorMax = status.rectTransform.pivot = new Vector2(0, 0);
                status.rectTransform.anchoredPosition = new Vector2(20, 38); status.rectTransform.sizeDelta = new Vector2(170, 20);
                var track = Rect("Collection progress track", card.transform); var trackFace = Surface(track, new Color(1, 1, 1, .85f), 6);
                track.anchorMin = track.anchorMax = track.pivot = new Vector2(0, 0); track.anchoredPosition = new Vector2(20, 18); track.sizeDelta = new Vector2(170, 12);
                var fill = ProgressFill(track, CollectionLook.Accent(collection));
                collectionCards.Add(card); collectionCardDetails.Add(detail); collectionCardStatus.Add(status); collectionCardTitles.Add(title);
                collectionThumbs.Add(portrait); collectionThumbShapes.Add(shape);
                collectionBanners.Add(bannerImage); collectionBoxArt.Add(boxImage); collectionStandIns.Add(standIn); collectionFills.Add(fill);
                collectionFades.Add(fadeImage); collectionTracks.Add(trackFace);
                foreach (var words in new[] { title, detail, status }) Soften(words);
            }

            // One collection: its painting as a banner with the box over it and the title on the art.
            collectionHero = Rect("Collection hero", collectionRoot); Surface(collectionHero, Quiet, 32);
            collectionHero.gameObject.AddComponent<Mask>().showMaskGraphic = true;
            var heroBannerRect = Rect("Collection hero banner", collectionHero); Stretch(heroBannerRect);
            heroBanner = heroBannerRect.gameObject.AddComponent<RawImage>(); heroBanner.raycastTarget = false;
            var heroFadeRect = Rect("Collection hero fade", collectionHero); Stretch(heroFadeRect);
            heroFade = heroFadeRect.gameObject.AddComponent<RawImage>(); heroFade.texture = UiShapes.FadeDown; heroFade.raycastTarget = false;
            heroStandIn = Rect("Collection hero art", collectionHero); Surface(heroStandIn, Quiet, 26);
            heroStandIn.gameObject.AddComponent<Mask>().showMaskGraphic = true; // round the portrait's corners
            var heroShapeRect = Rect("Hero silhouette", heroStandIn); Stretch(heroShapeRect);
            heroShape = heroShapeRect.gameObject.AddComponent<CharacterSilhouetteGraphic>(); heroShape.color = new Color(Ink.r, Ink.g, Ink.b, .16f);
            var heroPortraitRect = Rect("Hero portrait", heroStandIn); Stretch(heroPortraitRect);
            heroPortrait = heroPortraitRect.gameObject.AddComponent<RawImage>(); heroPortrait.raycastTarget = false; heroPortrait.enabled = false;
            // The box gives a little show-off hop now and then (motion moves the inner picture, layout moves the holder).
            var heroBoxHolder = Rect("Collection hero box", collectionHero);
            var heroBoxRect = Rect("Box", heroBoxHolder); Stretch(heroBoxRect); heroBoxRect.pivot = new Vector2(.5f, 0);
            heroBox = heroBoxRect.gameObject.AddComponent<RawImage>(); heroBox.raycastTarget = false;
            heroHype = BoxHype.Add(heroBox, Color.white, .5f);
            heroBoxRect.gameObject.AddComponent<PersonalityMotion>().Configure(PersonalityMotion.Kind.Toy, PersonalityMotionStyle.Show, scale: .6f);
            heroCaption = Label("", collectionHero, PockleTheme.SmallSize - 1, PockleTheme.OnPlum, FontStyle.Bold, TextAnchor.LowerLeft, Vector2.zero, new Vector2(200, 18));
            heroTitle = Label("", collectionHero, 44, PockleTheme.OnPlum, FontStyle.Bold, TextAnchor.LowerLeft, Vector2.zero, new Vector2(200, 52));
            heroTitle.resizeTextMinSize = 28;
            // Progress card in the collection's accent, tucked under the banner.
            progressCard = Rect("Collection progress", collectionRoot); Surface(progressCard, Primary, 26);
            heroCount = Label("", progressCard, 22, PockleTheme.OnPlum, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(200, 30));
            heroNote = Label("", progressCard, PockleTheme.SmallSize, PockleTheme.OnPlum, FontStyle.Bold, TextAnchor.MiddleRight, Vector2.zero, new Vector2(150, 30));
            var track2 = Rect("Collection progress track", progressCard); Surface(track2, new Color(1, 1, 1, .3f), 7);
            heroFill = ProgressFill(track2, Color.white);
            halfwayTick = Rect("Halfway", track2); Surface(halfwayTick, Color.white, 2);
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
            collectionBoxes = CreateButton("View boxes", collectionRoot, Vector2.zero, new Vector2(300, 56), Primary,
                () => Navigate(AppPage.Boxes), out _);
            collectionBoxes.name = "Collection boxes";
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
            lineupMysteryHolders.Clear(); lineupBubbles.Clear();
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
                int index = lineupMysteries.Count;
                var silhouette = EagerOutline(silhouetteRect, index);
                lineupMysteries.Add(silhouette); lineupMysteryHolders.Add(silhouetteRect);
                // A little "!" or "?" pops up over toys still to find (canvas "Collection: who's inside").
                var bubble = Rect("Eager bubble", tile.transform);
                var bubbleFace = Rect("Bubble", bubble); Stretch(bubbleFace); Surface(bubbleFace, Color.white, 13);
                var mark = Label(index % 2 == 0 ? "!" : "?", bubbleFace, 17, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
                Stretch(mark.rectTransform);
                bubbleFace.gameObject.AddComponent<PersonalityMotion>().Configure(PersonalityMotion.Kind.Pop, phaseSeconds: index * .23f);
                lineupBubbles.Add(bubble);
                var portraitRect = Rect("Lineup portrait", tile.transform);
                var picture = portraitRect.gameObject.AddComponent<RawImage>(); picture.raycastTarget = false;
                if (PipVariants.TryFromCollectibleId(id, out var variant)) picture.texture = previews[(int)variant];
                lineupPictures.Add(picture);
                TintMirror.Follow(tile, picture);
                lineupBindings.Add(shelfPreviews.Register(picture, silhouette, id));
            }
        }

        /// <summary>The glow behind a box: the collection's accent, lifted toward white so it reads as light.</summary>
        private static Color Glowing(Color accent) => Color.Lerp(accent, Color.white, .25f);

        /// <summary>A collection without a series number is only "coming soon" if none of its toys can be had yet.</summary>
        private static bool IsPlayable(CollectionDefinition collection)
        {
            foreach (string id in collection.Members)
                if (ToyCatalog.TryGetCollectible(id, out var toy) && toy.Available) return true;
            return false;
        }

        /// <summary>A faint plum shadow under white words, so they stay readable over bright patches of a painting.</summary>
        private static void Soften(Text words)
        {
            var shadow = words.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(.10f, .04f, .12f, .35f); shadow.effectDistance = new Vector2(0, -1); shadow.enabled = false;
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
                int found = FoundCount(collection), total = collection.Members.Count;
                bool complete = found == total && found > 0;
                collectionCardDetails[i].text = (collection.SeriesNumber > 0 ? "Series " + collection.SeriesNumber : IsPlayable(collection) ? "New collection" : "Coming soon").ToUpperInvariant();
                collectionCardStatus[i].text = (complete ? "Complete" : found == 0 ? "Not started" : found + " found") + "  ·  " + found + "/" + total;
                bool painted = CollectionLook.Banner(collection) != null;
                collectionFades[i].enabled = painted;
                collectionFades[i].color = new Color(Ink.r, Ink.g, Ink.b, .58f);
                collectionCardTitles[i].color = painted ? Color.white : Ink;
                collectionCardDetails[i].color = painted ? new Color(1, 1, 1, .86f) : CollectionLook.Ink(collection);
                collectionCardStatus[i].color = painted ? (complete ? new Color(.74f, .96f, .80f) : Color.white) : complete ? PockleTheme.Success : CollectionLook.Ink(collection);
                collectionTracks[i].color = painted ? new Color(1, 1, 1, .32f) : new Color(1, 1, 1, .85f);
                foreach (var words in new Text[] { collectionCardTitles[i], collectionCardDetails[i], collectionCardStatus[i] })
                    words.GetComponent<Shadow>().enabled = painted;
                collectionFills[i].fillAmount = total == 0 ? 0 : (float)found / total;
                if (!showingList) continue;
                var banner = CollectionLook.Banner(collection);
                collectionBanners[i].texture = banner; collectionBanners[i].enabled = banner != null;
                var box = CollectionLook.Box(collection, out float aspect);
                collectionBoxArt[i].texture = box; collectionBoxArt[i].enabled = box != null;
                collectionBoxArt[i].rectTransform.sizeDelta = new Vector2(150 * aspect, 150); // positioned in LayoutCollections
                collectionStandIns[i].gameObject.SetActive(box == null);
                if (box == null) ShowCollectionArt(collection, collectionThumbs[i], collectionThumbShapes[i]);
            }
            var selected = SelectedCollection;
            int selectedFound = FoundCount(selected), selectedTotal = selected.Members.Count;
            Color accent = CollectionLook.Accent(selected), deep = CollectionLook.Ink(selected), field = CatalogUnityColor(selected.FieldColor);
            collectionProgress.text = selectedFound + " of " + selectedTotal + " found";
            collectionHero.GetComponent<Image>().color = field;
            var heroArt = CollectionLook.Banner(selected);
            heroBanner.texture = heroArt; heroBanner.enabled = heroArt != null;
            heroFade.color = new Color(deep.r, deep.g, deep.b, heroArt != null ? .62f : .35f);
            var heroBoxArt = CollectionLook.Box(selected, out heroBoxAspect);
            heroHype.SetColour(Glowing(CollectionLook.Accent(selected)));
            heroBox.texture = heroBoxArt; heroBox.enabled = heroBoxArt != null; heroBox.transform.parent.gameObject.SetActive(heroBoxArt != null);
            heroStandIn.gameObject.SetActive(heroBoxArt == null);
            heroStandIn.GetComponent<Image>().color = field;
            heroTitle.text = selected.DisplayName;
            heroCaption.text = (selected.SeriesNumber > 0 ? "Series " + selected.SeriesNumber + " · " : "") + selectedTotal + " to find" + (selected.HasSecret ? " + a secret" : "");
            heroCaption.text = heroCaption.text.ToUpperInvariant();
            progressCard.GetComponent<Image>().color = accent;
            heroCount.text = selectedFound + " of " + selectedTotal + " found";
            int missing = selectedTotal - selectedFound;
            heroNote.text = missing == 0 ? "Complete!" : missing == 1 ? "1 more to find" : missing + " more to find";
            heroFill.fillAmount = selectedTotal == 0 ? 0 : (float)selectedFound / selectedTotal;
            lineupHeading.GetComponent<Text>().color = deep;
            if (showingOne && heroBoxArt == null) ShowCollectionArt(selected, heroPortrait, heroShape);
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
            collectionBoxes.GetComponent<Image>().color = offers ? deep : Quiet;
            string price = "";
            foreach (var offer in BoxCatalog.Offers) if (offer.CollectionId == selected.Id && price.Length == 0) price = offer.ProposedPrice;
            var ctaLabel = collectionBoxes.GetComponentInChildren<Text>();
            ctaLabel.text = offers ? "Open a " + selected.DisplayName + " box" + (price.Length > 0 ? ", " + price : "") : selected.DisplayName + " boxes are coming soon";
            ctaLabel.color = offers ? PockleTheme.OnPlum : MutedInk;
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
                lineupBubbles[i].gameObject.SetActive(!playable && known && collectible.Available);
                // The slot is the portrait's own colour field, so the toy sits on one continuous background (no square inside the tile).
                lineupTiles[i].GetComponent<Image>().color = playable ? PockleTheme.FieldFor(id) : PockleTheme.Mystery;
                if (playable && lineupPictures[i].texture == null) ShowLineupPortrait(i, id);
                lineupNames[i].color = playable ? deep : Ink;
            }
        }

        private float LayoutCollections(float width)
        {
            float y = 0;
            const float cardHeight = 200;
            for (int i = 0; i < collectionCards.Count; i++)
            {
                LeftLabel(collectionCards[i].GetComponent<RectTransform>(), new Vector2(0, -y), new Vector2(width, cardHeight));
                var banner = collectionBanners[i];
                banner.uvRect = CollectionLook.Cover(banner.texture, width / cardHeight, .5f, .45f);
                var box = collectionBoxArt[i].rectTransform;
                box.anchorMin = box.anchorMax = box.pivot = new Vector2(1, 0);
                float boxHeight = Mathf.Min(152, (width * .46f) / Mathf.Max(.5f, box.sizeDelta.x / Mathf.Max(1, box.sizeDelta.y)));
                box.sizeDelta = new Vector2(boxHeight * box.sizeDelta.x / Mathf.Max(1, box.sizeDelta.y), boxHeight);
                box.anchoredPosition = new Vector2(-6, 8);
                var standIn = collectionStandIns[i];
                standIn.anchorMin = standIn.anchorMax = standIn.pivot = new Vector2(1, 0);
                standIn.anchoredPosition = new Vector2(-16, 16); standIn.sizeDelta = new Vector2(128, 128);
                var track = collectionFills[i].rectTransform.parent as RectTransform;
                track.sizeDelta = new Vector2(Mathf.Min(190, width * .5f), 12);
                y += cardHeight + 12;
            }
            TopCentered(collectionsRoot, Vector2.zero, new Vector2(width, y));
            return y;
        }

        private float LayoutCollection(float width)
        {
            // Banner with the box over it and the title on the art; the accent progress card tucks under it.
            // Full-bleed: the painting runs edge to edge from the top, under the back button. The hero starts above the
            // viewport so only its bottom corners are rounded; everything below keeps the usual gutters.
            const float heroHeight = 330, overhang = 40;
            float g = PockleTheme.Gutter, full = width;
            width = full - g * 2;
            LeftLabel(collectionHero, new Vector2(0, overhang), new Vector2(full, heroHeight + overhang));
            heroBanner.uvRect = CollectionLook.Cover(heroBanner.texture, full / (heroHeight + overhang), .4f, .5f);
            float boxHeight = Mathf.Min(186, full * .5f / Mathf.Max(.5f, heroBoxAspect));
            var boxRect = (RectTransform)heroBox.rectTransform.parent;
            boxRect.anchorMin = boxRect.anchorMax = boxRect.pivot = new Vector2(1, 0);
            boxRect.anchoredPosition = new Vector2(-g + 8, 36); boxRect.sizeDelta = new Vector2(boxHeight * heroBoxAspect, boxHeight);
            heroStandIn.anchorMin = heroStandIn.anchorMax = heroStandIn.pivot = new Vector2(1, 0);
            heroStandIn.anchoredPosition = new Vector2(-g, 40); heroStandIn.sizeDelta = new Vector2(150, 150);
            float text = full - Mathf.Min(boxHeight * heroBoxAspect, full * .5f) - g * 2;
            heroTitle.rectTransform.anchorMin = heroTitle.rectTransform.anchorMax = heroTitle.rectTransform.pivot = new Vector2(0, 0);
            heroTitle.rectTransform.anchoredPosition = new Vector2(g + 4, 40); heroTitle.rectTransform.sizeDelta = new Vector2(text, 52);
            heroCaption.rectTransform.anchorMin = heroCaption.rectTransform.anchorMax = heroCaption.rectTransform.pivot = new Vector2(0, 0);
            heroCaption.rectTransform.anchoredPosition = new Vector2(g + 4, 94); heroCaption.rectTransform.sizeDelta = new Vector2(text, 18);
            const float cardHeight = 92;
            LeftLabel(progressCard, new Vector2(g, -(heroHeight - 22)), new Vector2(width, cardHeight));
            float inner = width - 36;
            LeftLabel(heroCount.rectTransform, new Vector2(18, -12), new Vector2(inner * .6f, 30));
            LeftLabel(heroNote.rectTransform, new Vector2(18 + inner * .6f, -12), new Vector2(inner * .4f, 30));
            var progressTrack = heroFill.rectTransform.parent as RectTransform;
            LeftLabel(progressTrack, new Vector2(18, -56), new Vector2(inner, 14));
            LeftLabel(halfwayTick, new Vector2(inner * .5f - 2, 3), new Vector2(4, 20));
            float y = heroHeight - 22 + cardHeight + 22;
            LeftLabel(lineupHeading, new Vector2(g, -y), new Vector2(width, 30)); y += 40;
            LeftLabel(collectionNote.rectTransform, new Vector2(g, -y), new Vector2(width, 0));
            collectionNote.gameObject.SetActive(false);
            int columns = width >= 450 ? 4 : width >= 300 ? 3 : 2;
            const float gap = 10;
            float tileWidth = (width - (columns - 1) * gap) / columns;
            float tileHeight = tileWidth + 64;
            for (int i = 0; i < lineupTiles.Count; i++)
            {
                LeftLabel(lineupTiles[i].GetComponent<RectTransform>(), new Vector2(g + (i % columns) * (tileWidth + gap),
                    -y - (i / columns) * (tileHeight + gap)), new Vector2(tileWidth, tileHeight));
                float art = tileWidth - 16;
                TopCentered(lineupPictures[i].rectTransform, new Vector2(0, -8), new Vector2(art, art));
                TopCentered(lineupMysteryHolders[i], new Vector2(0, -8), new Vector2(art, art));
                var bubbleRect = lineupBubbles[i];
                bubbleRect.anchorMin = bubbleRect.anchorMax = bubbleRect.pivot = new Vector2(1, 1);
                bubbleRect.anchoredPosition = new Vector2(-8, -8); bubbleRect.sizeDelta = new Vector2(28, 28);
                var name = lineupNames[i].rectTransform;
                name.anchorMin = name.anchorMax = name.pivot = new Vector2(.5f, 0);
                name.anchoredPosition = new Vector2(0, 30); name.sizeDelta = new Vector2(tileWidth - 12, 24);
                TopCentered(lineupStates[i].rectTransform, new Vector2(0, -tileHeight + 28), new Vector2(tileWidth - 12, 24));
            }
            int rows = (lineupTiles.Count + columns - 1) / columns;
            y += rows * (tileHeight + gap) + 4;
            if (secretCard.gameObject.activeSelf) { LeftLabel(secretCard, new Vector2(g, -y), new Vector2(width, 104)); y += 116; }
            LeftLabel(collectionBoxes.GetComponent<RectTransform>(), new Vector2(g, -y), new Vector2(width, 56)); y += 68;
            LeftLabel(collectionWalking.GetComponent<RectTransform>(), new Vector2(g, -y), new Vector2(width, 48)); y += 60;
            TopCentered(collectionRoot, Vector2.zero, new Vector2(full, y));
            return y;
        }

        /// <summary>
        /// An undiscovered toy's outline that hops or waves to be noticed, drawn and moved in 2D (smooth at any frame
        /// rate, no 3D actor). <paramref name="holder"/> is the laid-out box; the outline fills it and moves inside it.
        /// </summary>
        private CharacterSilhouetteGraphic EagerOutline(RectTransform holder, int index)
        {
            var outline = Rect("Eager outline", holder); Stretch(outline); outline.pivot = new Vector2(.5f, 0);
            var graphic = outline.gameObject.AddComponent<CharacterSilhouetteGraphic>(); graphic.color = PockleTheme.PlumSoft;
            outline.gameObject.AddComponent<PersonalityMotion>().Configure(index % 2 == 0 ? PersonalityMotion.Kind.Hop : PersonalityMotion.Kind.Wave,
                phaseSeconds: index * .23f, scale: .7f);
            return graphic;
        }

        /// <summary>A found toy's still portrait on its colour field, used whenever its live preview isn't running.</summary>
        private void ShowLineupPortrait(int index, string id)
        {
            if (!lineupPortraits.TryGetValue(id, out var texture))
            {
                texture = null;
                try { texture = ToyPortrait.Render(id, PockleTheme.FieldFor(id)); }
                catch (Exception error) { Debug.LogWarning("Pockle lineup portrait: " + error.Message); }
                lineupPortraits[id] = texture;
            }
            if (texture == null) return;
            lineupPictures[index].texture = texture;
            shelfPreviews.SetPresentation(lineupBindings[index], texture, PockleTheme.FieldFor(id));
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
