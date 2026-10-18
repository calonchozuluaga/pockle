using System;
using System.Collections.Generic;
using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// Toy stage (canvas row "Play screen: side rail"): collection pill in the header, series/number eyebrow, rarity badge,
    /// and a rail of four round buttons on the right (World, Light, Collection, About). The toy gets the whole screen; a
    /// rail button opens a small floating card at the bottom, and the toy's area shrinks above it (owner, October 15).
    /// </summary>
    public sealed partial class PrototypeHud
    {
        public event Action<StageWorld> WorldChanged;
        public event Action<StageLight> LightChanged;
        /// <summary>
        /// The part of the screen the toy and its plate may use on the play page, in screen pixels (origin bottom-left, like
        /// <see cref="Camera.pixelRect"/>): below the title block and above the tray, or above the open panel. Raised whenever it
        /// changes, including when a tray tab opens or closes, so the stage can frame the toy clear of the HUD.
        /// </summary>
        public event Action<Rect> StageViewportChanged;
        public Rect StageViewport { get; private set; }
        /// <summary>
        /// The collection world on show, by catalog ID ("fox-snowy-den"), or empty for Studio and the shared legacy worlds.
        /// Raised with the ID so the stage can call PrototypeStage.SetEnvironment; WorldChanged still fires for older code.
        /// </summary>
        public event Action<string> EnvironmentChanged;
        public string CurrentEnvironmentId => stageEnvironment;
        public StageWorld CurrentWorld => stageWorld;
        public StageLight CurrentLight => stageLight;

        private const int TrayClosed = -1;
        private static readonly string[] TrayTabNames = { "Collection", "World", "Light", "About" };
        private const int AboutTab = 3;
        private Button stageCollectionPill;
        private Text stageCollectionText, stageEyebrow, stageRarityText, stageCount, stageCollectionTitle, stageWorldNote, stageLightNote;
        private RectTransform stageRarity, stageTray, stagePanel, stageRail;
        // Rail order top to bottom, and each button's icon (Resources/UI/Icons).
        private static readonly int[] RailOrder = { 1, 2, 0, 3 };
        private static readonly string[] RailIcons = { "Collection", "World", "Light", "About" };
        private const float RailButton = 52, RailGap = 10, CardHeader = 44;
        private Text stageTrayTitle, stageTrayNote;
        private Button stageTrayClose;
        private RectTransform stageShade;
        private Shadow headingShadow;
        private bool stagePainted;
        private readonly RectTransform[] stagePanels = new RectTransform[4];
        private readonly Button[] stageTabs = new Button[4];
        private readonly Image[] stageTabFaces = new Image[4];
        private readonly List<Button> finishDots = new List<Button>();
        private readonly List<Image> finishDotFaces = new List<Image>();
        private readonly List<Image> finishDotRings = new List<Image>();
        private readonly List<string> finishDotIds = new List<string>();
        // The Collection tab shows small pictures of the toys (owner, October 14), not colour dots: found toys as still
        // portraits you can tap to play, the rest as silhouettes. The row scrolls sideways.
        private readonly List<RawImage> finishPictures = new List<RawImage>();
        private readonly List<CharacterSilhouetteGraphic> finishOutlines = new List<CharacterSilhouetteGraphic>();
        private RectTransform finishViewport, finishContent;
        private const float FinishTileWidth = 58, FinishTileHeight = 66, FinishTileRadius = 15;
        private Button stageFindMore;
        // World tray: Studio plus the collection's own worlds (five each), scrolling sideways.
        private sealed class WorldTile { public Button Button; public Image Face, Ring; public RawImage Art, Shade; public Text Caption; }
        private struct WorldChoice { public string Key, Label, EnvironmentId; public StageWorld World; public Texture2D Art; public Color Swatch; }
        private readonly List<WorldTile> worldTiles = new List<WorldTile>();
        private readonly List<WorldChoice> worldChoices = new List<WorldChoice>();
        private RectTransform worldViewport, worldContent;
        private string stageEnvironment = "";
        private const string EnvironmentKeyPrefix = "pockle.stage.environment.v1.";
        private const float WorldTileWidth = 84, WorldTileHeight = 104, WorldTileRadius = 18;
        // About tab.
        private Text aboutSummary, aboutLine, aboutVoiceLabel;
        private Button aboutVoice, aboutMeet;
        private readonly RectTransform[] aboutChips = new RectTransform[3];
        private readonly Text[] aboutChipTexts = new Text[3];
        private readonly RectTransform[] aboutCards = new RectTransform[3];
        private readonly Text[] aboutWhen = new Text[3], aboutDoes = new Text[3];
        private readonly Button[] lightTiles = new Button[3];
        private readonly Image[] lightFaces = new Image[3];
        private int trayTab = TrayClosed;
        private StageWorld stageWorld = StageWorld.Studio;
        private StageLight stageLight = StageLight.Day;
        private string stageCollectionId = "";
        private RectTransform stageViewportRect;
        private readonly Vector3[] stageCorners = new Vector3[4];

        private void BuildStage()
        {
            // Studio (the toy's own colour field) until the player picks a world.
            stageWorld = (StageWorld)Mathf.Clamp(PlayerPrefs.GetInt(StageLook.WorldKey, (int)StageWorld.Studio), 0, StageLook.Worlds.Length - 1);
            stageLight = (StageLight)Mathf.Clamp(PlayerPrefs.GetInt(StageLook.LightKey, 0), 0, StageLook.Lights.Length - 1);

            stageCollectionPill = CreateButton("Collection", safeRoot, Vector2.zero, new Vector2(200, 40), new Color(1, 1, 1, .6f),
                OpenStageCollection, out _, PockleTheme.CaptionSize);
            stageCollectionPill.name = "Stage collection";
            stageCollectionText = stageCollectionPill.GetComponentInChildren<Text>();

            stageEyebrow = Label("", safeRoot, PockleTheme.SmallSize, MutedInk, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(300, 20));
            stageRarity = Rect("Rarity", safeRoot); Surface(stageRarity, PockleTheme.Confetti, 14);
            stageRarityText = Label("", stageRarity, PockleTheme.SmallSize, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
            Stretch(stageRarityText.rectTransform);
            BuildStageTalk(); // temperament badge, traits, "handle with care" chip and the speech bubble

            // A soft dark shade behind the title when a painted world fills the screen, so white words stay readable.
            stageShade = Rect("Stage title shade", safeRoot); stageShade.SetAsFirstSibling();
            var shade = stageShade.gameObject.AddComponent<RawImage>(); shade.texture = UiShapes.FadeDown; shade.raycastTarget = false;
            shade.color = new Color(.16f, .08f, .19f, .42f); shade.rectTransform.localScale = new Vector3(1, -1, 1); // dark at the top
            stageShade.gameObject.SetActive(false);
            headingShadow = heading.gameObject.GetComponent<Shadow>() ?? heading.gameObject.AddComponent<Shadow>();
            headingShadow.effectColor = new Color(.10f, .04f, .12f, .35f); headingShadow.effectDistance = new Vector2(0, -1); headingShadow.enabled = false;

            // The card a rail button opens: a title, a note, a close button, then that button's panel.
            stageTray = Rect("Stage tray", safeRoot); Surface(stageTray, new Color(PockleTheme.Milk.r, PockleTheme.Milk.g, PockleTheme.Milk.b, .95f), 28);
            stageTray.GetComponent<Image>().raycastTarget = true; // touches on the card never reach the toy
            stageTrayTitle = Label("", stageTray, PockleTheme.BodySize + 1, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(200, 30));
            stageTrayNote = Label("", stageTray, PockleTheme.SmallSize - 1, MutedInk, FontStyle.Normal, TextAnchor.MiddleRight, Vector2.zero, new Vector2(140, 30));
            stageTrayClose = CreateButton("Close", stageTray, Vector2.zero, new Vector2(44, 44), Quiet, () => SetTrayTab(TrayClosed), out _, PockleTheme.SmallSize);
            stageTrayClose.name = "Stage tray close";
            IconOnly(stageTrayClose, "Close", 14);

            // The rail: round glass buttons on the right, low enough for a thumb.
            stageRail = Rect("Stage rail", safeRoot);
            foreach (int i in RailOrder)
            {
                int tab = i;
                stageTabs[i] = CreateButton(TrayTabNames[i], stageRail, Vector2.zero, new Vector2(RailButton, RailButton), new Color(1, 1, 1, .8f),
                    () => SetTrayTab(trayTab == tab ? TrayClosed : tab), out stageTabFaces[i], PockleTheme.SmallSize);
                stageTabs[i].name = "Stage tab · " + TrayTabNames[i];
                IconOnly(stageTabs[i], RailIcons[i], 24);
            }
            stagePanel = Rect("Stage panel", stageTray);
            for (int i = 0; i < TrayTabNames.Length; i++) stagePanels[i] = Rect(TrayTabNames[i] + " panel", stagePanel);

            // Collection: the members as dots, then the way to find the rest.
            var collectionPanel = stagePanels[0];
            // Its title and count show in the card header (kept as hidden labels for the header to copy).
            stageCollectionTitle = Label("", collectionPanel, PockleTheme.BodySize, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(200, 24));
            stageCount = Label("", collectionPanel, PockleTheme.SmallSize, MutedInk, FontStyle.Normal, TextAnchor.MiddleRight, Vector2.zero, new Vector2(120, 24));
            stageCollectionTitle.enabled = false; stageCount.enabled = false;
            stageFindMore = CreateButton("Find the rest in boxes", collectionPanel, Vector2.zero, new Vector2(300, 44), Primary,
                () => Navigate(AppPage.Boxes), out _, PockleTheme.CaptionSize + 1);
            stageFindMore.name = "Stage find more";

            // World: a sideways-scrolling row of painted thumbnails (tiles are created as a collection needs them).
            worldViewport = Rect("World row", stagePanels[1]);
            worldViewport.gameObject.AddComponent<RectMask2D>();
            var worldHit = worldViewport.gameObject.AddComponent<Image>(); worldHit.color = Color.clear;
            var worldScroll = worldViewport.gameObject.AddComponent<ScrollRect>(); worldScroll.vertical = false; worldScroll.movementType = ScrollRect.MovementType.Clamped;
            worldContent = Rect("World tiles", worldViewport);
            worldContent.anchorMin = worldContent.anchorMax = worldContent.pivot = new Vector2(0, 1);
            worldScroll.content = worldContent; worldScroll.viewport = worldViewport;
            stageWorldNote = Label("", stagePanels[1], PockleTheme.SmallSize - 1, MutedInk, FontStyle.Normal, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(300, 20));
            stageWorldNote.enabled = false; // the card header says how many worlds there are

            // Light: three moods.
            for (int i = 0; i < 3; i++)
            {
                var light = StageLook.Lights[i];
                lightTiles[i] = CreateButton(StageLook.Name(light), stagePanels[2], Vector2.zero, new Vector2(100, 56), Quiet,
                    () => SetStageLight(light), out lightFaces[i], PockleTheme.CaptionSize, false);
                lightTiles[i].name = "Stage light · " + light;
            }
            stageLightNote = Label("", stagePanels[2], PockleTheme.SmallSize - 1, MutedInk, FontStyle.Normal, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(300, 20));
            // About: the toy's personality at a glance (canvas "Toy stage: About tab").
            var about = stagePanels[AboutTab];
            aboutSummary = Label("", about, PockleTheme.BodySize + 1, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(220, 24));
            aboutLine = Label("", about, PockleTheme.SmallSize, MutedInk, FontStyle.Normal, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(220, 18));
            aboutVoice = CreateButton("Voice", about, Vector2.zero, new Vector2(110, 40), Quiet, () => { personalityId = selectedCollectibleId; PlayVoice(); }, out _, PockleTheme.SmallSize);
            aboutVoice.name = "Stage voice";
            aboutVoiceLabel = aboutVoice.GetComponentInChildren<Text>();
            for (int i = 0; i < 3; i++)
            {
                aboutChips[i] = Rect("Trait", about); Surface(aboutChips[i], Quiet, 14);
                aboutChipTexts[i] = Label("", aboutChips[i], PockleTheme.SmallSize - 1, Ink, FontStyle.Bold, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
                Stretch(aboutChipTexts[i].rectTransform); aboutChipTexts[i].resizeTextForBestFit = false; aboutChipTexts[i].horizontalOverflow = HorizontalWrapMode.Overflow;
                aboutCards[i] = Rect("Reaction", about); Surface(aboutCards[i], Quiet, 16);
                aboutWhen[i] = Label("", aboutCards[i], 11, MutedInk, FontStyle.Normal, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
                aboutDoes[i] = Label("", aboutCards[i], PockleTheme.SmallSize, Ink, FontStyle.Bold, TextAnchor.UpperLeft, Vector2.zero, Vector2.zero);
            }
            aboutMeet = CreateButton("Meet", about, Vector2.zero, new Vector2(300, 44), Primary, () => OpenPersonality(selectedCollectibleId), out _, PockleTheme.CaptionSize + 1);
            aboutMeet.name = "Stage meet";
            stageViewportRect = Rect("Stage viewport", safeRoot);
            SetTrayTab(TrayClosed);
        }

        private bool StageSelection(out CollectibleDefinition toy, out CollectionDefinition collection)
        {
            collection = null;
            return ToyCatalog.TryGetCollectible(selectedCollectibleId, out toy) && ToyCatalog.TryGetCollection(toy.CollectionId, out collection);
        }

        /// <summary>"Aurora · Super rare" becomes "Aurora"; the rarity moves to its own badge.</summary>
        private static string FinishName(CollectibleDefinition toy)
        {
            string name = toy.FinishDisplayName;
            int dot = name.IndexOf(" · ", StringComparison.Ordinal);
            return dot > 0 ? name.Substring(0, dot) : name;
        }

        private static string RarityText(CollectibleDefinition toy)
        {
            string name = toy.FinishDisplayName;
            int dot = name.IndexOf(" · ", StringComparison.Ordinal);
            string label = dot > 0 ? name.Substring(dot + 3) : "";
            if (toy.IsSecret) return label.Length > 0 ? "Secret · " + label : "Secret";
            return label;
        }

        private string StageTitle => StageSelection(out var toy, out _) ? FinishName(toy) : SelectedToyName;

        private Color StageBackdrop
        {
            get
            {
                string character = StageSelection(out var toy, out _) ? toy.CharacterId : "pip";
                return StageLook.Backdrop(stageWorld, stageLight, character, PockleTheme.FieldFor(selectedCollectibleId));
            }
        }

        private void OpenStageCollection()
        {
            if (StageSelection(out _, out var collection)) OpenCollection(collection.Id);
        }

        private void SetTrayTab(int tab)
        {
            trayTab = tab;
            for (int i = 0; i < TrayTabNames.Length; i++)
            {
                bool on = i == tab;
                stagePanels[i].gameObject.SetActive(on);
                stageTabFaces[i].color = on ? Primary : new Color(1, 1, 1, .8f);
                stageTabs[i].GetComponentInChildren<Text>(true).color = on ? PockleTheme.OnPlum : Ink;
                foreach (var icon in stageTabs[i].GetComponentsInChildren<RawImage>(true)) icon.color = on ? PockleTheme.OnPlum : Ink;
            }
            stagePanel.gameObject.SetActive(tab != TrayClosed);
            stageTray.gameObject.SetActive(tab != TrayClosed && page == AppPage.Play && StageSelection(out _, out _));
            RefreshTrayHeader();
            if (lastLayoutSize.x > 0 && page == AppPage.Play) AdaptLayout();
        }

        /// <summary>The open card's title and note: "Worlds · 5 Pip worlds", "Pip collection · 10 of 10", ...</summary>
        private void RefreshTrayHeader()
        {
            if (stageTrayTitle == null) return;
            bool known = StageSelection(out var toy, out var collection);
            switch (trayTab)
            {
                case 0: stageTrayTitle.text = known ? collection.DisplayName + " collection" : "Collection"; stageTrayNote.text = stageCount.text; break;
                case 1:
                    stageTrayTitle.text = "Worlds";
                    stageTrayNote.text = known && collection.Worlds.Count > 0 ? collection.Worlds.Count + " " + collection.DisplayName + " worlds" : "";
                    break;
                case 2: stageTrayTitle.text = "Light"; stageTrayNote.text = ""; break;
                case AboutTab: stageTrayTitle.text = known ? "About " + PersonalityBook.ShortName(toy.Id) : "About"; stageTrayNote.text = ""; break;
                default: stageTrayTitle.text = ""; stageTrayNote.text = ""; break;
            }
        }

        /// <summary>Closes the play card without a relayout, for when the page is about to change anyway.</summary>
        private void CloseStageCardQuietly()
        {
            if (page != AppPage.Play || trayTab == TrayClosed) return;
            trayTab = TrayClosed;
            for (int i = 0; i < stagePanels.Length; i++) stagePanels[i].gameObject.SetActive(false);
            stagePanel.gameObject.SetActive(false); stageTray.gameObject.SetActive(false);
            for (int i = 0; i < stageTabs.Length; i++)
            {
                stageTabFaces[i].color = new Color(1, 1, 1, .8f);
                foreach (var icon in stageTabs[i].GetComponentsInChildren<RawImage>(true)) icon.color = Ink;
            }
        }

        private void SetStageWorld(StageWorld world) => SetStageWorld(world, "");

        /// <summary>Shows a world: Studio, a shared legacy world, or one of the collection's own (by catalog ID).</summary>
        private void SetStageWorld(StageWorld world, string environmentId)
        {
            stageWorld = world; stageEnvironment = environmentId ?? "";
            PlayerPrefs.SetInt(StageLook.WorldKey, (int)world);
            if (stageCollectionId.Length > 0) PlayerPrefs.SetString(EnvironmentKeyPrefix + stageCollectionId, ChoiceKey(world, stageEnvironment));
            RefreshStage(); WorldChanged?.Invoke(world); EnvironmentChanged?.Invoke(stageEnvironment); BackdropChanged?.Invoke(StageBackdrop);
        }

        private static string ChoiceKey(StageWorld world, string environmentId) => environmentId.Length > 0 ? "env:" + environmentId : world.ToString();

        /// <summary>Studio, then the collection's own worlds; collections without worlds yet keep the shared Jelly Garden and Midnight.</summary>
        private void BuildWorldChoices(CollectibleDefinition toy, CollectionDefinition collection)
        {
            worldChoices.Clear();
            Color field = PockleTheme.FieldFor(toy.Id);
            worldChoices.Add(new WorldChoice { Key = ChoiceKey(StageWorld.Studio, ""), Label = "Studio", EnvironmentId = "", World = StageWorld.Studio, Swatch = field });
            if (collection.Worlds.Count > 0)
            {
                foreach (var world in collection.Worlds)
                {
                    var painting = Resources.Load<PaintedWorldAsset>(world.EnvironmentArt);
                    worldChoices.Add(new WorldChoice { Key = ChoiceKey(StageWorld.Home, world.Id), Label = world.DisplayName, EnvironmentId = world.Id, World = StageWorld.Home,
                        Art = painting != null ? painting.Background : null, Swatch = StageLook.Swatch(StageWorld.Home, toy.CharacterId, field) });
                }
            }
            else
            {
                foreach (var world in new[] { StageWorld.Garden, StageWorld.Midnight })
                {
                    var painting = PaintedWorldAsset.Load(world, collection.HomeWorld);
                    worldChoices.Add(new WorldChoice { Key = ChoiceKey(world, ""), Label = StageLook.Name(world, toy.CharacterId), EnvironmentId = "", World = world,
                        Art = painting != null ? painting.Background : null, Swatch = StageLook.Swatch(world, toy.CharacterId, field) });
                }
            }
            while (worldTiles.Count < worldChoices.Count) worldTiles.Add(BuildWorldTile(worldTiles.Count));
        }

        private WorldTile BuildWorldTile(int index)
        {
            var tile = new WorldTile();
            // The ring sits behind the tile, 3 px larger all round with a 3 px larger radius, so selected corners stay round.
            var ring = Rect("World ring", worldContent); tile.Ring = Surface(ring, Ink, WorldTileRadius + 3);
            tile.Button = CreateButton("World", worldContent, Vector2.zero, new Vector2(WorldTileWidth, WorldTileHeight), Quiet,
                () => { var choice = worldChoices[index]; SetStageWorld(choice.World, choice.EnvironmentId); }, out tile.Face, PockleTheme.SmallSize - 1, false);
            SetRadius(tile.Face, WorldTileRadius);
            tile.Button.gameObject.AddComponent<Mask>().showMaskGraphic = true; // the painting takes the tile's rounded corners
            var art = Rect("World painting", tile.Button.transform); Stretch(art); art.SetAsFirstSibling();
            tile.Art = art.gameObject.AddComponent<RawImage>(); tile.Art.raycastTarget = false;
            var shade = Rect("World shade", tile.Button.transform); Stretch(shade); shade.SetSiblingIndex(1); shade.anchorMax = new Vector2(1, .5f);
            tile.Shade = shade.gameObject.AddComponent<RawImage>(); tile.Shade.texture = UiShapes.FadeDown; tile.Shade.raycastTarget = false;
            tile.Shade.color = new Color(0, 0, 0, .45f);
            tile.Caption = tile.Button.GetComponentInChildren<Text>();
            var caption = tile.Caption.rectTransform;
            caption.anchorMin = new Vector2(0, 0); caption.anchorMax = new Vector2(1, 0); caption.pivot = new Vector2(.5f, 0);
            caption.offsetMin = new Vector2(4, 6); caption.offsetMax = new Vector2(-4, 34);
            LeftLabel(tile.Button.GetComponent<RectTransform>(), new Vector2(3 + index * (WorldTileWidth + 8), -3), new Vector2(WorldTileWidth, WorldTileHeight));
            LeftLabel(ring, new Vector2(index * (WorldTileWidth + 8), 0), new Vector2(WorldTileWidth + 6, WorldTileHeight + 6));
            return tile;
        }

        /// <summary>When the toy's collection changes, show the world last picked for that collection.</summary>
        private void RestoreCollectionWorld(CollectionDefinition collection)
        {
            string saved = PlayerPrefs.GetString(EnvironmentKeyPrefix + collection.Id, "");
            string current = ChoiceKey(stageWorld, stageEnvironment);
            WorldChoice? pick = null;
            foreach (var choice in worldChoices) if (choice.Key == (saved.Length > 0 ? saved : current)) pick = choice;
            if (pick == null) pick = worldChoices[0];
            if (pick.Value.Key == current) return;
            stageWorld = pick.Value.World; stageEnvironment = pick.Value.EnvironmentId;
            WorldChanged?.Invoke(stageWorld); EnvironmentChanged?.Invoke(stageEnvironment); BackdropChanged?.Invoke(StageBackdrop);
        }

        private void SetStageLight(StageLight light)
        {
            stageLight = light; PlayerPrefs.SetInt(StageLook.LightKey, (int)light);
            RefreshStage(); LightChanged?.Invoke(light); BackdropChanged?.Invoke(StageBackdrop);
        }

        /// <summary>Rebuilds the dots when the collection changes; recolours everything else.</summary>
        private void RefreshStage()
        {
            if (stageTray == null) return;
            bool play = page == AppPage.Play;
            bool known = StageSelection(out var toy, out var collection);
            stageCollectionPill.gameObject.SetActive(play && known);
            stageEyebrow.gameObject.SetActive(play);
            stageTray.gameObject.SetActive(play && known && trayTab != TrayClosed);
            stageRail.gameObject.SetActive(play && known);
            RefreshStageTalk(play && known, known ? toy.Id : null);
            string rarity = known ? RarityText(toy) : "";
            stageRarity.gameObject.SetActive(play && rarity.Length > 0);
            if (!play) { heading.color = Ink; headingShadow.enabled = false; stageShade.gameObject.SetActive(false); return; }

            // Over a painting the words turn white on a soft shade; over the flat studio colour they stay plum.
            stagePainted = false;
            string shownKey = ChoiceKey(stageWorld, stageEnvironment);
            foreach (var choice in worldChoices) if (choice.Key == shownKey && choice.Art != null) stagePainted = true;
            bool dark = stagePainted || StageLook.IsDark(stageWorld, stageLight);
            heading.color = dark ? PockleTheme.OnPlum : Ink;
            headingShadow.enabled = stagePainted;
            stageShade.gameObject.SetActive(stagePainted);
            StyleStageTalk(dark);
            stageEyebrow.color = dark ? PockleTheme.OnPlumMuted : MutedInk;
            playHint.color = Ink;
            if (!known) { stageEyebrow.text = SelectedSeriesLine.ToUpperInvariant(); return; }

            int number = 0;
            for (int i = 0; i < collection.Members.Count; i++) if (collection.Members[i] == toy.Id) number = i + 1;
            string series = collection.SeriesNumber > 0 ? " · Series " + collection.SeriesNumber : "";
            stageEyebrow.text = (collection.DisplayName + series + (number > 0 ? " · No. " + number : "")).ToUpperInvariant();
            stageRarityText.text = rarity;

            int found = FoundCount(collection);
            stageCollectionText.text = collection.DisplayName + " collection · " + found + " of " + collection.Members.Count;
            stageCollectionTitle.text = collection.DisplayName + (collection.SeriesNumber > 0 ? " · Series " + collection.SeriesNumber : " collection");
            stageCount.text = found + " of " + collection.Members.Count + " found";
            int missing = collection.Members.Count - found;
            bool offers = false;
            foreach (var offer in BoxCatalog.Offers) if (offer.CollectionId == collection.Id) offers = true;
            stageFindMore.GetComponentInChildren<Text>().text = missing == 0 ? "You found all " + collection.Members.Count
                : offers ? "Find the other " + missing + " in boxes" : "More " + collection.DisplayName + " boxes coming soon";
            stageFindMore.interactable = missing > 0 && offers;

            if (stageCollectionId != collection.Id) { RebuildFinishDots(collection); BuildWorldChoices(toy, collection); RestoreCollectionWorld(collection); }
            for (int i = 0; i < finishDots.Count; i++)
            {
                string id = finishDotIds[i];
                bool owned = session != null && session.GetOwnedCount(id) > 0;
                bool playable = session != null && session.IsOwnedAndPlayable(id);
                bool secret = collection.SecretId == id;
                finishDots[i].interactable = playable;
                finishDotFaces[i].color = owned ? PockleTheme.FieldFor(id) : PockleTheme.Mystery;
                var picture = owned ? StillPortrait(id) : null;
                finishPictures[i].texture = picture; finishPictures[i].enabled = picture != null;
                // A missing toy shows only its character's outline; the secret's stays hidden.
                finishOutlines[i].enabled = !owned;
                if (!owned) finishOutlines[i].Configure(collection.CharacterId, secret);
                finishDotRings[i].gameObject.SetActive(id == selectedCollectibleId);
                string label = ToyCatalog.TryGetCollectible(id, out var member) && owned ? FinishName(member) : "Not found yet";
                finishDots[i].name = "Stage finish · " + id + " · " + label;
            }

            string currentKey = ChoiceKey(stageWorld, stageEnvironment);
            for (int i = 0; i < worldTiles.Count; i++)
            {
                var tile = worldTiles[i];
                bool shown = i < worldChoices.Count;
                tile.Button.gameObject.SetActive(shown); tile.Ring.gameObject.SetActive(false);
                if (!shown) continue;
                var choice = worldChoices[i];
                tile.Button.name = "Stage world · " + (choice.EnvironmentId.Length > 0 ? choice.EnvironmentId : choice.World.ToString());
                tile.Face.color = choice.Swatch;
                tile.Caption.text = choice.Label;
                tile.Art.texture = choice.Art; tile.Art.enabled = choice.Art != null; tile.Shade.enabled = choice.Art != null;
                tile.Art.uvRect = CollectionLook.Cover(choice.Art, WorldTileWidth / WorldTileHeight, .5f, .45f);
                tile.Caption.color = choice.Art != null || choice.World == StageWorld.Midnight ? PockleTheme.OnPlum : Ink;
                tile.Ring.gameObject.SetActive(choice.Key == currentKey);
            }
            worldContent.sizeDelta = new Vector2(worldChoices.Count * (WorldTileWidth + 8) + 6, WorldTileHeight + 6);
            stageWorldNote.text = collection.Worlds.Count > 0 ? collection.Worlds.Count + " worlds come with the " + collection.DisplayName + " collection. Swipe for more."
                : collection.DisplayName + "'s own worlds are on the way.";

            // About: the personality at a glance.
            if (PersonalityBook.TryGet(toy.Id, out var personality))
            {
                aboutSummary.text = personality.Summary;
                aboutLine.text = collection.DisplayName + " by nature · " + personality.TouchName;
                aboutVoiceLabel.text = personality.Voice;
                for (int i = 0; i < 3; i++)
                {
                    bool touch = i == 2;
                    string text = touch ? personality.TouchTrait : i < personality.Traits.Count ? personality.Traits[i] : "";
                    aboutChipTexts[i].text = text;
                    aboutChips[i].gameObject.SetActive(text.Length > 0);
                    aboutChips[i].GetComponent<Image>().color = touch ? PockleTheme.Confetti : PockleTheme.FieldFor(toy.Id);
                    bool reaction = i < personality.Reactions.Count;
                    // Prefer the night reaction last, when a finish has one (it's what makes a glow finish special).
                    int index = i == 2 && personality.Reactions.Count > 3 ? personality.Reactions.Count - 1 : i;
                    aboutCards[i].gameObject.SetActive(reaction);
                    if (reaction) { aboutWhen[i].text = personality.Reactions[index].Key; aboutDoes[i].text = personality.Reactions[index].Value; }
                }
                aboutMeet.GetComponentInChildren<Text>().text = "Meet " + PersonalityBook.ShortName(toy.Id);
            }
            for (int i = 0; i < 3; i++)
            {
                bool on = StageLook.Lights[i] == stageLight;
                lightFaces[i].color = on ? Primary : Quiet;
                lightTiles[i].GetComponentInChildren<Text>().color = on ? PockleTheme.OnPlum : Ink;
            }
            stageLightNote.text = StageLook.Note(stageLight, toy.Id == ToyCatalog.FoxAuroraId);
            RefreshTrayHeader();
        }

        private void RebuildFinishDots(CollectionDefinition collection)
        {
            if (finishViewport == null)
            {
                finishViewport = Rect("Finish row", stagePanels[0]);
                finishViewport.gameObject.AddComponent<RectMask2D>();
                var hit = finishViewport.gameObject.AddComponent<Image>(); hit.color = Color.clear;
                var scroll = finishViewport.gameObject.AddComponent<ScrollRect>(); scroll.vertical = false; scroll.movementType = ScrollRect.MovementType.Clamped;
                finishContent = Rect("Finish tiles", finishViewport);
                finishContent.anchorMin = finishContent.anchorMax = finishContent.pivot = new Vector2(0, 1);
                scroll.content = finishContent; scroll.viewport = finishViewport;
            }
            foreach (var dot in finishDots) Destroy(dot.gameObject);
            foreach (var ring in finishDotRings) Destroy(ring.gameObject);
            finishDots.Clear(); finishDotFaces.Clear(); finishDotRings.Clear(); finishDotIds.Clear(); finishPictures.Clear(); finishOutlines.Clear();
            stageCollectionId = collection.Id;
            for (int i = 0; i < collection.Members.Count; i++)
            {
                string id = collection.Members[i];
                float x = 3 + i * (FinishTileWidth + 8);
                // The ring sits behind the tile, 3 px larger with a 3 px larger radius, so the selected corners stay round.
                var ringRect = Rect("Finish ring", finishContent);
                finishDotRings.Add(Surface(ringRect, Ink, FinishTileRadius + 3));
                LeftLabel(ringRect, new Vector2(x - 3, 0), new Vector2(FinishTileWidth + 6, FinishTileHeight + 6));
                var tile = CreateButton("", finishContent, Vector2.zero, new Vector2(FinishTileWidth, FinishTileHeight), PockleTheme.Mystery,
                    () => { if (selectedCollectibleId != id) ShowToy(id); }, out var face, PockleTheme.SmallSize, false);
                SetRadius(face, FinishTileRadius);
                tile.gameObject.AddComponent<Mask>().showMaskGraphic = true; // portraits take the tile's rounded corners
                var colours = tile.colors; colours.disabledColor = Color.white; tile.colors = colours;
                LeftLabel(tile.GetComponent<RectTransform>(), new Vector2(x, -3), new Vector2(FinishTileWidth, FinishTileHeight));
                var outline = Rect("Outline", tile.transform);
                outline.anchorMin = new Vector2(.14f, .1f); outline.anchorMax = new Vector2(.86f, .9f); outline.offsetMin = outline.offsetMax = Vector2.zero;
                var shape = outline.gameObject.AddComponent<CharacterSilhouetteGraphic>();
                shape.color = new Color(PockleTheme.PlumSoft.r, PockleTheme.PlumSoft.g, PockleTheme.PlumSoft.b, .45f);
                shape.Configure(collection.CharacterId);
                var picture = Rect("Portrait", tile.transform); Stretch(picture);
                var image = picture.gameObject.AddComponent<RawImage>(); image.raycastTarget = false; image.enabled = false;
                TintMirror.Follow(tile, image);
                finishDots.Add(tile); finishDotFaces.Add(face); finishDotIds.Add(id); finishPictures.Add(image); finishOutlines.Add(shape);
            }
            finishContent.sizeDelta = new Vector2(collection.Members.Count * (FinishTileWidth + 8) + 6, FinishTileHeight + 6);
        }

        /// <summary>Header pill, title block and tray positions on the play page.</summary>
        private void LayoutStage(Vector2 size, float left, float width)
        {
            if (stageTray == null || page != AppPage.Play) return;
            // Header: collection pill centred between back and favourite.
            var pill = stageCollectionPill.GetComponent<RectTransform>();
            float pillWidth = Mathf.Min(size.x - left * 2 - 120, stageCollectionText.preferredWidth + 32);
            pill.anchorMin = pill.anchorMax = pill.pivot = new Vector2(.5f, 1); pill.anchoredPosition = new Vector2(0, -18); pill.sizeDelta = new Vector2(pillWidth, 40);

            // Title block: eyebrow, finish name with its rarity badge, then the ways to play.
            LeftLabel(stageEyebrow.rectTransform, new Vector2(left, -72), new Vector2(width, 20));
            LeftLabel(heading.rectTransform, new Vector2(left, -94), new Vector2(width, 44));
            float nameWidth = Mathf.Min(width - 130, heading.preferredWidth);
            float badgeWidth = stageRarityText.preferredWidth + 24;
            LeftLabel(stageRarity, new Vector2(left + nameWidth + 12, -102), new Vector2(badgeWidth, 28));
            if (left + nameWidth + 12 + badgeWidth > size.x - left) LeftLabel(stageRarity, new Vector2(left, -142), new Vector2(badgeWidth, 28));
            float titleBottom = stageRarity.gameObject.activeSelf && stageRarity.anchoredPosition.y < -120 ? 170 : 140;
            titleBottom = LayoutStageTalk(left, width, titleBottom);

            // Title shade across the top of the screen.
            stageShade.anchorMin = new Vector2(0, 1); stageShade.anchorMax = new Vector2(1, 1); stageShade.pivot = new Vector2(.5f, .5f); // flipped about its centre
            stageShade.offsetMin = new Vector2(-left, -(titleBottom + 70)); stageShade.offsetMax = new Vector2(left, 60);

            // The card: header row, then the open button's panel. Only one is open at a time.
            float trayWidth = Mathf.Min(size.x - 24, 400);
            float inner = trayWidth - 16;
            float panelHeight = trayTab == 0 ? 130 : trayTab == 1 ? 116 : trayTab == 2 ? 100 : trayTab == AboutTab ? 196 : 0;
            bool open = trayTab != TrayClosed;
            float trayHeight = open ? 12 + CardHeader + panelHeight + 8 : 0;
            stageTray.anchorMin = stageTray.anchorMax = stageTray.pivot = new Vector2(.5f, 0);
            stageTray.anchoredPosition = new Vector2(0, 16); stageTray.sizeDelta = new Vector2(trayWidth, trayHeight);
            LeftLabel(stageTrayTitle.rectTransform, new Vector2(16, -14), new Vector2(inner * .6f, 30));
            LeftLabel(stageTrayClose.GetComponent<RectTransform>(), new Vector2(trayWidth - 10 - 44, -7), new Vector2(44, 44)); // 44 px: easy to hit
            LeftLabel(stageTrayNote.rectTransform, new Vector2(trayWidth - 10 - 44 - 8 - 150, -14), new Vector2(150, 30));
            LeftLabel(stagePanel, new Vector2(8, -(12 + CardHeader)), new Vector2(inner, panelHeight));
            foreach (var panel in stagePanels) LeftLabel(panel, Vector2.zero, new Vector2(inner, panelHeight));
            float cardTop = open ? 16 + trayHeight : 0;

            // The rail sits on the right, just above the hint, or just above the open card.
            float hintTop = 20 + (playHint.preferredWidth + 32 > Mathf.Min(width - RailButton - 12, playHint.preferredWidth + 32) ? 50 : 32);
            float railHeight = RailButton * 4 + RailGap * 3;
            stageRail.anchorMin = stageRail.anchorMax = stageRail.pivot = new Vector2(1, 0);
            stageRail.anchoredPosition = new Vector2(-Mathf.Max(12, left - 6), open ? cardTop + 12 : hintTop + 24);
            stageRail.sizeDelta = new Vector2(RailButton, railHeight);
            for (int r = 0; r < RailOrder.Length; r++)
                LeftLabel(stageTabs[RailOrder[r]].GetComponent<RectTransform>(), new Vector2(0, -r * (RailButton + RailGap)), new Vector2(RailButton, RailButton));

            float pad = 8, row = inner - pad * 2;
            if (finishViewport != null) LeftLabel(finishViewport, new Vector2(pad - 3, 0), new Vector2(row + 6, FinishTileHeight + 8));
            LeftLabel(stageFindMore.GetComponent<RectTransform>(), new Vector2(pad, -82), new Vector2(row, 44));

            LeftLabel(worldViewport, new Vector2(pad - 3, -2), new Vector2(row + 6, WorldTileHeight + 8));
            LeftLabel(stageWorldNote.rectTransform, new Vector2(pad, -116), new Vector2(row, 20));
            float lightWidth = (row - 16) / 3;
            for (int i = 0; i < 3; i++) LeftLabel(lightTiles[i].GetComponent<RectTransform>(), new Vector2(pad + i * (lightWidth + 8), -6), new Vector2(lightWidth, 56));
            LeftLabel(stageLightNote.rectTransform, new Vector2(pad, -72), new Vector2(row, 20));

            // About: summary and voice, trait chips, three reactions, then Meet.
            float voiceWidth = Mathf.Max(96, aboutVoiceLabel.preferredWidth + 28);
            LeftLabel(aboutSummary.rectTransform, new Vector2(pad, -2), new Vector2(row - voiceWidth - 8, 24));
            LeftLabel(aboutLine.rectTransform, new Vector2(pad, -26), new Vector2(row - voiceWidth - 8, 18));
            LeftLabel(aboutVoice.GetComponent<RectTransform>(), new Vector2(pad + row - voiceWidth, -4), new Vector2(voiceWidth, 40));
            float chipX = pad;
            for (int i = 0; i < 3; i++)
            {
                if (!aboutChips[i].gameObject.activeSelf) continue;
                float chipWidth = Mathf.Min(row - (chipX - pad), aboutChipTexts[i].preferredWidth + 20);
                LeftLabel(aboutChips[i], new Vector2(chipX, -52), new Vector2(chipWidth, 28)); chipX += chipWidth + 6;
            }
            float cardWidth = (row - 12) / 3;
            for (int i = 0; i < 3; i++)
            {
                LeftLabel(aboutCards[i], new Vector2(pad + i * (cardWidth + 6), -88), new Vector2(cardWidth, 50));
                LeftLabel(aboutWhen[i].rectTransform, new Vector2(10, -6), new Vector2(cardWidth - 16, 14));
                LeftLabel(aboutDoes[i].rectTransform, new Vector2(10, -22), new Vector2(cardWidth - 16, 24));
            }
            LeftLabel(aboutMeet.GetComponent<RectTransform>(), new Vector2(pad, -148), new Vector2(row, 44));

            // The ways-to-play hint sits at the bottom while nothing is open. The toy's area is the space between the title and
            // the hint (or the open card), kept clear of the rail on both sides so the toy stays centred.
            playHintPill.anchorMin = playHintPill.anchorMax = playHintPill.pivot = new Vector2(.5f, 0);
            playHintPill.anchoredPosition = new Vector2(0, 20);
            // The runtime hint can be long ("Tap Pip to play · Drag to turn · ..."), so it may wrap onto a second line.
            float hintWidth = Mathf.Min(width - RailButton - 12, playHint.preferredWidth + 32);
            playHint.horizontalOverflow = HorizontalWrapMode.Wrap;
            playHintPill.sizeDelta = new Vector2(hintWidth, 32);
            float hintHeight = playHint.preferredWidth + 32 > hintWidth ? 50 : 32;
            playHintPill.sizeDelta = new Vector2(hintWidth, hintHeight);
            playHintPill.gameObject.SetActive(!open);
            float bottom = open ? cardTop + 8 : hintTop + 8;
            // Full width: the toy renders behind the rail buttons (owner, October 16: no box around the toy). The rail sits
            // symmetrically on both sides, so the toy stays centred; touches on the buttons never reach the toy.
            float side = 0;
            stageViewportRect.anchorMin = new Vector2(0, 0); stageViewportRect.anchorMax = new Vector2(1, 1);
            stageViewportRect.offsetMin = new Vector2(side, bottom); stageViewportRect.offsetMax = new Vector2(-side, -(titleBottom + 8));
            PublishStageViewport();
            SayPendingHello(); // needs the toy's area, which is only known now
        }

        /// <summary>Converts the free area to screen pixels and tells the stage when it moves.</summary>
        private void PublishStageViewport()
        {
            if (stageViewportRect == null || page != AppPage.Play) return;
            stageViewportRect.GetWorldCorners(stageCorners);
            var cam = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            Vector2 low = RectTransformUtility.WorldToScreenPoint(cam, stageCorners[0]);
            Vector2 high = RectTransformUtility.WorldToScreenPoint(cam, stageCorners[2]);
            var area = new Rect(low.x, low.y, Mathf.Max(1, high.x - low.x), Mathf.Max(1, high.y - low.y));
            if (area == StageViewport) return;
            StageViewport = area;
            StageViewportChanged?.Invoke(area);
        }
    }
}
