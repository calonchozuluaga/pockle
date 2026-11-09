using System;
using System.Collections.Generic;
using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// Toy stage with four named controls and a compact optional bottom sheet.
    /// The Pockles sheet switches directly between owned toys across the roster, including saved fusions.
    /// </summary>
    public sealed partial class PrototypeHud
    {
        public event Action<StageWorld> WorldChanged;
        public event Action<StageLight> LightChanged;
        /// <summary>
        /// The part of the screen the toy may use on the play page, in screen pixels (origin bottom-left, like
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
        private static readonly string[] TrayTabNames = { "Pockles", "Backdrop", "Light", "About" };
        private const int AboutTab = 3;
        private Button stageCollectionPill;
        private Text stageCollectionText, stageEyebrow, stageRarityText, stageWorldNote, stageLightNote;
        private RectTransform stageRarity, stageTray, stagePanel, stageRail;
        private Image stageRarityFace, stageRarityRing;
        private RawImage stageRarityIcon;
        private const float RarityHeight = 26;
        private static readonly int[] RailOrder = { 0, 1, 2, 3 };
        private const float RailButton = 48, RailGap = 6, CardHeader = 44;
        private Text stageTrayTitle, stageTrayNote;
        private Button stageTrayClose;
        private RectTransform stageShade;
        private static Texture2D stageTitleFade;
        private Shadow headingShadow;
        private bool stagePainted;
        private readonly RectTransform[] stagePanels = new RectTransform[4];
        private readonly Button[] stageTabs = new Button[4];
        private readonly Image[] stageTabFaces = new Image[4];
        private readonly List<Button> finishDots = new List<Button>();
        private readonly List<Image> finishDotFaces = new List<Image>();
        private readonly List<Image> finishDotRings = new List<Image>();
        private readonly List<string> finishDotIds = new List<string>();
        // The optional Pockles sheet shows owned playable finishes and fusions. It never offers locked toys.
        private readonly List<RawImage> finishPictures = new List<RawImage>();
        private RectTransform finishViewport, finishContent;
        private const float FinishTileHeight = 116, FinishTileRadius = 15;
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
        private Text aboutSummary, aboutLine;
        private Button aboutMeet;
        private readonly RectTransform[] aboutChips = new RectTransform[3];
        private readonly Text[] aboutChipTexts = new Text[3];
        private readonly RectTransform[] aboutCards = new RectTransform[3];
        private readonly Text[] aboutWhen = new Text[3], aboutDoes = new Text[3];
        private readonly Button[] lightTiles = new Button[StageLook.Lights.Length];
        private readonly Image[] lightFaces = new Image[StageLook.Lights.Length];
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
            int savedLight = PlayerPrefs.GetInt(StageLook.LightKey, (int)StageLight.Day);
            stageLight = StageLook.NormalizeLight((StageLight)savedLight);
            if (savedLight != (int)stageLight) { PlayerPrefs.SetInt(StageLook.LightKey, (int)stageLight); PlayerPrefs.Save(); }

            stageCollectionPill = CreateButton("Collection", safeRoot, Vector2.zero, new Vector2(200, 40), new Color(1, 1, 1, .6f),
                OpenStageCollection, out _, PockleTheme.CaptionSize);
            stageCollectionPill.name = "Stage collection";
            stageCollectionText = stageCollectionPill.GetComponentInChildren<Text>();

            stageEyebrow = Label("", safeRoot, PockleTheme.SmallSize, MutedInk, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(300, 20));
            // Rarity: a solid badge in its tier colour with an icon and a thin bright rim (RarityLook), unlike the glassy chips.
            stageRarity = Rect("Rarity", safeRoot);
            var rim = Rect("Rarity rim", stageRarity); Stretch(rim); rim.offsetMin = new Vector2(-1.5f, -1.5f); rim.offsetMax = new Vector2(1.5f, 1.5f);
            stageRarityRing = Surface(rim, Color.white, RarityHeight / 2 + 1.5f);
            var face = Rect("Rarity face", stageRarity); Stretch(face);
            stageRarityFace = Surface(face, PockleTheme.Confetti, RarityHeight / 2);
            var icon = Rect("Rarity icon", stageRarity); LeftLabel(icon, new Vector2(8, -5), new Vector2(16, 16));
            stageRarityIcon = icon.gameObject.AddComponent<RawImage>(); stageRarityIcon.raycastTarget = false;
            stageRarityText = Label("", stageRarity, 12, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, Vector2.zero);
            stageRarityText.horizontalOverflow = HorizontalWrapMode.Overflow; stageRarityText.resizeTextForBestFit = false;
            Stretch(stageRarityText.rectTransform); stageRarityText.rectTransform.offsetMin = new Vector2(29, 0); stageRarityText.rectTransform.offsetMax = new Vector2(-10, 0);
            BuildStageTalk(); // temperament badge, traits, "handle with care" chip and the speech bubble

            // A soft dark shade behind the title when a painted world fills the screen, so white words stay readable.
            stageShade = Rect("Stage title shade", safeRoot); stageShade.SetAsFirstSibling();
            var shade = stageShade.gameObject.AddComponent<RawImage>(); shade.texture = StageTitleFade(); shade.raycastTarget = false;
            shade.color = new Color(.16f, .08f, .19f, .52f);
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

            // Explicit labels keep the four play controls understandable without guessing at icons.
            stageRail = Rect("Stage rail", safeRoot);
            foreach (int i in RailOrder)
            {
                int tab = i;
                stageTabs[i] = CreateButton(TrayTabNames[i], stageRail, Vector2.zero, new Vector2(RailButton, RailButton), new Color(1, 1, 1, .8f),
                    () => SetTrayTab(trayTab == tab ? TrayClosed : tab), out stageTabFaces[i], PockleTheme.SmallSize);
                stageTabs[i].name = "Stage tab · " + TrayTabNames[i];
            }
            stagePanel = Rect("Stage panel", stageTray);
            for (int i = 0; i < TrayTabNames.Length; i++) stagePanels[i] = Rect(TrayTabNames[i] + " panel", stagePanel);

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

            // Light: two clear choices, while the saved legacy enum values remain stable.
            for (int i = 0; i < StageLook.Lights.Length; i++)
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
            if (FusionNames.TryShort(toy.Id, out var fused)) return fused; // "Honey × Copper", never the whole recipe
            string name = toy.FinishDisplayName;
            int dot = name.IndexOf(" · ", StringComparison.Ordinal);
            return dot > 0 ? name.Substring(0, dot) : name;
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
            if (tab == 0) RefreshPocklePicker();
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

        /// <summary>The open sheet's title and its count or collection context.</summary>
        private void RefreshTrayHeader()
        {
            if (stageTrayTitle == null) return;
            bool known = StageSelection(out var toy, out var collection);
            switch (trayTab)
            {
                case 0: stageTrayTitle.text = "Your Pockles"; stageTrayNote.text = finishDotIds.Count + " ready to play"; break;
                case 1:
                    stageTrayTitle.text = "Backdrop";
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
                stageTabs[i].GetComponentInChildren<Text>(true).color = Ink;
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
            stageLight = StageLook.NormalizeLight(light); PlayerPrefs.SetInt(StageLook.LightKey, (int)stageLight);
            RefreshStage(); LightChanged?.Invoke(stageLight); BackdropChanged?.Invoke(StageBackdrop);
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
            var rarity = known ? RarityLook.Of(toy) : RarityLook.For(RarityTier.Common, false, "");
            stageRarity.gameObject.SetActive(play && rarity.Shown);
            if (rarity.Shown)
            {
                stageRarityFace.color = rarity.Fill; stageRarityRing.color = rarity.Ring; stageRarityText.color = rarity.Ink;
                stageRarityIcon.texture = PockleTheme.Icon(rarity.Icon); stageRarityIcon.color = rarity.Ink;
                stageRarityIcon.enabled = stageRarityIcon.texture != null;
                stageRarity.name = "Rarity · " + rarity.Tier;
            }
            if (!play) { heading.resizeTextForBestFit = false; heading.color = Ink; headingShadow.enabled = false; stageShade.gameObject.SetActive(false); return; }

            // Over a painting the words turn white on a soft shade; over the flat studio colour they stay plum.
            stagePainted = false;
            string shownKey = ChoiceKey(stageWorld, stageEnvironment);
            foreach (var choice in worldChoices) if (choice.Key == shownKey && choice.Art != null) stagePainted = true;
            bool dark = stagePainted || StageLook.IsDark(stageWorld, stageLight);
            heading.color = dark ? PockleTheme.OnPlum : Ink;
            heading.resizeTextForBestFit = known; // long names shrink to fit one line rather than run into the toy
            heading.resizeTextMinSize = 18; heading.resizeTextMaxSize = PockleTheme.TitleSize;
            headingShadow.enabled = dark;
            stageShade.gameObject.SetActive(dark);
            StyleStageTalk(dark);
            stageEyebrow.color = dark ? PockleTheme.OnPlumMuted : MutedInk;
            playHint.color = Ink;
            if (!known) { stageEyebrow.text = SelectedSeriesLine.ToUpperInvariant(); return; }

            int number = 0;
            for (int i = 0; i < collection.Members.Count; i++) if (collection.Members[i] == toy.Id) number = i + 1;
            string series = collection.SeriesNumber > 0 ? " · Series " + collection.SeriesNumber : "";
            bool fusedToy = FusionCatalog.TryGet(toy.Id, out _);
            stageEyebrow.text = (collection.DisplayName + (fusedToy ? " · Fusion" : series + (number > 0 ? " · No. " + number : ""))).ToUpperInvariant();
            stageRarityText.text = rarity.Text;

            int found = FoundCount(collection);
            stageCollectionText.text = collection.DisplayName + " collection · " + found + " of " + collection.Members.Count;
            if (stageCollectionId != collection.Id) { stageCollectionId = collection.Id; BuildWorldChoices(toy, collection); RestoreCollectionWorld(collection); }
            if (trayTab == 0) RefreshPocklePicker();

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
            for (int i = 0; i < StageLook.Lights.Length; i++)
            {
                bool on = StageLook.Lights[i] == stageLight;
                lightFaces[i].color = on ? Primary : Quiet;
                lightTiles[i].GetComponentInChildren<Text>().color = on ? PockleTheme.OnPlum : Ink;
            }
            stageLightNote.text = StageLook.Note(stageLight, toy.Id == ToyCatalog.FoxAuroraId);
            RefreshTrayHeader();
        }

        private void RefreshPocklePicker()
        {
            var owned = new List<string>();
            if (session != null)
                foreach (var entry in session.OwnedCounts)
                    if (entry.Value > 0 && session.IsOwnedAndPlayable(entry.Key)) owned.Add(entry.Key);
            owned.Sort(StringComparer.Ordinal);
            bool changed = owned.Count != finishDotIds.Count;
            if (!changed) for (int i = 0; i < owned.Count; i++) if (owned[i] != finishDotIds[i]) { changed = true; break; }
            bool rebuilt = changed || finishViewport == null;
            if (rebuilt) RebuildPocklePicker(owned);
            for (int i = 0; i < finishDots.Count; i++)
            {
                string id = finishDotIds[i];
                finishDots[i].interactable = session != null && session.IsOwnedAndPlayable(id);
                finishDotFaces[i].color = PockleTheme.FieldFor(id);
                finishDotRings[i].gameObject.SetActive(id == selectedCollectibleId);
            }
            if (!rebuilt) RefreshVisiblePocklePortraits(); // New tile positions are assigned by LayoutPocklePicker first.
        }

        private void RebuildPocklePicker(List<string> owned)
        {
            if (finishViewport == null)
            {
                finishViewport = Rect("Pockle picker", stagePanels[0]);
                finishViewport.gameObject.AddComponent<RectMask2D>();
                var hit = finishViewport.gameObject.AddComponent<Image>(); hit.color = Color.clear;
                var scroll = finishViewport.gameObject.AddComponent<ScrollRect>();
                scroll.horizontal = false; scroll.vertical = true; scroll.movementType = ScrollRect.MovementType.Clamped;
                finishContent = Rect("Owned Pockles", finishViewport);
                finishContent.anchorMin = finishContent.anchorMax = finishContent.pivot = new Vector2(0, 1);
                scroll.content = finishContent; scroll.viewport = finishViewport;
                scroll.onValueChanged.AddListener(_ => RefreshVisiblePocklePortraits());
            }
            foreach (var dot in finishDots) { dot.gameObject.SetActive(false); Destroy(dot.gameObject); }
            foreach (var ring in finishDotRings) { ring.gameObject.SetActive(false); Destroy(ring.gameObject); }
            finishDots.Clear(); finishDotFaces.Clear(); finishDotRings.Clear(); finishDotIds.Clear(); finishPictures.Clear();
            foreach (string id in owned)
            {
                ToyCatalog.TryGetCollectible(id, out var toy);
                ToyCatalog.TryGetCollection(toy.CollectionId, out var collection);
                var ringRect = Rect("Pockle selection", finishContent);
                finishDotRings.Add(Surface(ringRect, Ink, FinishTileRadius + 3));
                var tile = CreateButton("", finishContent, Vector2.zero, new Vector2(84, FinishTileHeight), PockleTheme.FieldFor(id),
                    () =>
                    {
                        if (session == null || !session.IsOwnedAndPlayable(id)) return;
                        SetTrayTab(TrayClosed);
                        if (selectedCollectibleId != id) ShowToy(id);
                    }, out var face, 12, false);
                tile.name = "Stage pockle · " + id;
                SetRadius(face, FinishTileRadius);
                tile.gameObject.AddComponent<Mask>().showMaskGraphic = true;
                var picture = Rect("Portrait", tile.transform);
                picture.anchorMin = new Vector2(0, 1); picture.anchorMax = new Vector2(1, 1);
                picture.offsetMin = new Vector2(3, -78); picture.offsetMax = new Vector2(-3, -3);
                var image = picture.gameObject.AddComponent<RawImage>(); image.raycastTarget = false; image.enabled = false;
                TintMirror.Follow(tile, image);
                var caption = tile.GetComponentInChildren<Text>();
                caption.text = (collection != null ? collection.DisplayName : toy.CharacterId) + "\n" + FinishName(toy);
                caption.alignment = TextAnchor.MiddleCenter; caption.raycastTarget = false;
                caption.resizeTextForBestFit = true; caption.resizeTextMinSize = 10; caption.resizeTextMaxSize = 12;
                caption.rectTransform.anchorMin = new Vector2(0, 0); caption.rectTransform.anchorMax = new Vector2(1, 0);
                caption.rectTransform.offsetMin = new Vector2(3, 3); caption.rectTransform.offsetMax = new Vector2(-3, 36);
                finishDots.Add(tile); finishDotFaces.Add(face); finishDotIds.Add(id); finishPictures.Add(image);
            }
            finishContent.anchoredPosition = Vector2.zero;
        }

        private void LayoutPocklePicker(float row, float height)
        {
            if (finishViewport == null) return;
            LeftLabel(finishViewport, new Vector2(5, 0), new Vector2(row + 6, height));
            int columns = Mathf.Clamp(Mathf.FloorToInt((row + 8) / 80), 2, 4);
            float tileWidth = (row - (columns - 1) * 8) / columns;
            for (int i = 0; i < finishDots.Count; i++)
            {
                float x = i % columns * (tileWidth + 8), y = i / columns * (FinishTileHeight + 8);
                LeftLabel(finishDotRings[i].rectTransform, new Vector2(x, -y), new Vector2(tileWidth + 6, FinishTileHeight + 6));
                LeftLabel(finishDots[i].GetComponent<RectTransform>(), new Vector2(x + 3, -y - 3), new Vector2(tileWidth, FinishTileHeight));
            }
            int rows = (finishDots.Count + columns - 1) / columns;
            finishContent.sizeDelta = new Vector2(row + 6, Mathf.Max(height, rows * (FinishTileHeight + 8)));
            RefreshVisiblePocklePortraits();
        }

        private void RefreshVisiblePocklePortraits()
        {
            if (trayTab != 0 || finishViewport == null || !finishViewport.gameObject.activeInHierarchy) return;
            float top = Mathf.Max(0, finishContent.anchoredPosition.y);
            float bottom = top + finishViewport.rect.height;
            for (int i = 0; i < finishDots.Count; i++)
            {
                var tile = finishDots[i].GetComponent<RectTransform>();
                float tileTop = -tile.anchoredPosition.y;
                if (tileTop > bottom || tileTop + tile.rect.height < top || finishPictures[i].texture != null) continue;
                // Portraits are cached, but their first render is real work. Render only the rows in this compact sheet.
                var picture = StillPortrait(finishDotIds[i]);
                finishPictures[i].texture = picture; finishPictures[i].enabled = picture != null;
            }
        }

        // Keep the title, eyebrow and personality lines over a consistent scrim, then fade below them.
        // This dedicated ramp leaves the shared collection/banner gradients unchanged.
        private static Texture2D StageTitleFade()
        {
            if (stageTitleFade != null) return stageTitleFade;
            const int height = 64;
            stageTitleFade = new Texture2D(1, height, TextureFormat.RGBA32, false)
            { name = "Stage header fade", filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var pixels = new Color32[height];
            for (int y = 0; y < height; y++)
            {
                float alpha = Mathf.SmoothStep(0, 1, Mathf.Clamp01(y / ((height - 1f) * .30f)));
                pixels[y] = new Color32(255, 255, 255, (byte)(255 * alpha));
            }
            stageTitleFade.SetPixels32(pixels);
            stageTitleFade.Apply(false, true);
            return stageTitleFade;
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
            // The rarity badge goes beside the name when it fits, else at the end of the eyebrow line, else on its own line
            // under the name. The block below always starts under it, so nothing overlaps.
            float titleBottom = 140;
            if (stageRarity.gameObject.activeSelf)
            {
                float badgeWidth = Mathf.Min(width, 29 + stageRarityText.preferredWidth + 12);
                float nameWidth = Mathf.Min(width, heading.preferredWidth);
                float eyebrowWidth = Mathf.Min(width, stageEyebrow.preferredWidth);
                bool fused = FusionCatalog.TryGet(selectedCollectibleId, out _);
                if (!fused && nameWidth + 12 + badgeWidth <= width) LeftLabel(stageRarity, new Vector2(left + nameWidth + 12, -(116 - RarityHeight / 2)), new Vector2(badgeWidth, RarityHeight));
                else if (eyebrowWidth + 10 + badgeWidth <= width) LeftLabel(stageRarity, new Vector2(left + eyebrowWidth + 10, -(82 - RarityHeight / 2)), new Vector2(badgeWidth, RarityHeight));
                else { LeftLabel(stageRarity, new Vector2(left, -142), new Vector2(badgeWidth, RarityHeight)); titleBottom = 142 + RarityHeight + 12; }
            }
            titleBottom = LayoutStageTalk(left, width, titleBottom);

            // Title shade across the top of the screen.
            stageShade.anchorMin = new Vector2(0, 1); stageShade.anchorMax = new Vector2(1, 1); stageShade.pivot = new Vector2(.5f, .5f);
            stageShade.offsetMin = new Vector2(-left, -(titleBottom + 120)); stageShade.offsetMax = new Vector2(left, 60);

            // The card: header row, then the open button's panel. Only one is open at a time.
            float trayWidth = Mathf.Min(size.x - 24, 400);
            float inner = trayWidth - 16;
            float pickerHeight = Mathf.Clamp(size.y * .27f, 172, 244);
            float panelHeight = trayTab == 0 ? pickerHeight : trayTab == 1 ? 116 : trayTab == 2 ? 100 : trayTab == AboutTab ? 196 : 0;
            bool open = trayTab != TrayClosed;
            float trayHeight = open ? 12 + CardHeader + panelHeight + 8 : 0;
            stageTray.anchorMin = stageTray.anchorMax = stageTray.pivot = new Vector2(.5f, 0);
            float controlsTop = 16 + RailButton;
            stageTray.anchoredPosition = new Vector2(0, controlsTop + 12); stageTray.sizeDelta = new Vector2(trayWidth, trayHeight);
            LeftLabel(stageTrayTitle.rectTransform, new Vector2(16, -14), new Vector2(inner * .6f, 30));
            LeftLabel(stageTrayClose.GetComponent<RectTransform>(), new Vector2(trayWidth - 10 - 44, -7), new Vector2(44, 44)); // 44 px: easy to hit
            LeftLabel(stageTrayNote.rectTransform, new Vector2(trayWidth - 10 - 44 - 8 - 150, -14), new Vector2(150, 30));
            LeftLabel(stagePanel, new Vector2(8, -(12 + CardHeader)), new Vector2(inner, panelHeight));
            foreach (var panel in stagePanels) LeftLabel(panel, Vector2.zero, new Vector2(inner, panelHeight));
            float cardTop = open ? controlsTop + 12 + trayHeight : 0;

            // The named controls stay in a single thumb-friendly row below the optional sheet.
            float hintTop = controlsTop + 12 + (playHint.preferredWidth + 32 > width ? 50 : 32);
            // Landscape has less vertical room. The optional gesture reminder yields before it crowds out
            // the toy and its speech bubble; all four labeled controls remain available.
            bool showHint = !open && size.y - titleBottom - hintTop - 16 >= 120;
            stageRail.anchorMin = stageRail.anchorMax = stageRail.pivot = new Vector2(.5f, 0);
            stageRail.anchoredPosition = new Vector2(0, 16);
            stageRail.sizeDelta = new Vector2(trayWidth, RailButton);
            float controlWidth = (trayWidth - RailGap * 3) / 4;
            for (int r = 0; r < RailOrder.Length; r++)
                LeftLabel(stageTabs[RailOrder[r]].GetComponent<RectTransform>(), new Vector2(r * (controlWidth + RailGap), 0), new Vector2(controlWidth, RailButton));

            float pad = 8, row = inner - pad * 2;
            LayoutPocklePicker(row, pickerHeight);

            LeftLabel(worldViewport, new Vector2(pad - 3, -2), new Vector2(row + 6, WorldTileHeight + 8));
            LeftLabel(stageWorldNote.rectTransform, new Vector2(pad, -116), new Vector2(row, 20));
            float lightWidth = (row - 8) / StageLook.Lights.Length;
            for (int i = 0; i < StageLook.Lights.Length; i++) LeftLabel(lightTiles[i].GetComponent<RectTransform>(), new Vector2(pad + i * (lightWidth + 8), -6), new Vector2(lightWidth, 56));
            LeftLabel(stageLightNote.rectTransform, new Vector2(pad, -72), new Vector2(row, 20));

            // About stays informational; character voices belong to the actual toy reactions.
            LeftLabel(aboutSummary.rectTransform, new Vector2(pad, -2), new Vector2(row, 24));
            LeftLabel(aboutLine.rectTransform, new Vector2(pad, -26), new Vector2(row, 18));
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

            // The ways-to-play hint sits above the controls while nothing is open. The toy remains above the hint or sheet.
            playHintPill.anchorMin = playHintPill.anchorMax = playHintPill.pivot = new Vector2(.5f, 0);
            playHintPill.anchoredPosition = new Vector2(0, controlsTop + 12);
            // The runtime hint can be long ("Tap Pip to play · Drag to turn · ..."), so it may wrap onto a second line.
            float hintWidth = Mathf.Min(width, playHint.preferredWidth + 32);
            playHint.horizontalOverflow = HorizontalWrapMode.Wrap;
            playHintPill.sizeDelta = new Vector2(hintWidth, 32);
            float hintHeight = playHint.preferredWidth + 32 > hintWidth ? 50 : 32;
            playHintPill.sizeDelta = new Vector2(hintWidth, hintHeight);
            playHintPill.gameObject.SetActive(showHint);
            float bottom = open ? cardTop + 8 : showHint ? hintTop + 8 : controlsTop + 8;
            // The toy uses the full width between its title and the controls or open sheet.
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
