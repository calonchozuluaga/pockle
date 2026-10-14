using System;
using System.Collections.Generic;
using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// Toy stage (canvas row "Toy stage"): collection pill in the header, series/number eyebrow, rarity badge, and a
    /// bottom tray with Collection, World and Light. The tray is a closed strip so the plate stays visible; a tab opens it.
    /// </summary>
    public sealed partial class PrototypeHud
    {
        public event Action<StageWorld> WorldChanged;
        public event Action<StageLight> LightChanged;
        public StageWorld CurrentWorld => stageWorld;
        public StageLight CurrentLight => stageLight;

        private const int TrayClosed = -1;
        private static readonly string[] TrayTabNames = { "Collection", "World", "Light" };
        private Button stageCollectionPill;
        private Text stageCollectionText, stageEyebrow, stageRarityText, stageCount, stageCollectionTitle, stageWorldNote, stageLightNote;
        private RectTransform stageRarity, stageTray, stagePanel;
        private readonly RectTransform[] stagePanels = new RectTransform[3];
        private readonly Button[] stageTabs = new Button[3];
        private readonly Image[] stageTabFaces = new Image[3];
        private readonly List<Button> finishDots = new List<Button>();
        private readonly List<Image> finishDotFaces = new List<Image>();
        private readonly List<Image> finishDotRings = new List<Image>();
        private readonly List<string> finishDotIds = new List<string>();
        private Button stageFindMore;
        private readonly Button[] worldTiles = new Button[4];
        private readonly Image[] worldFaces = new Image[4];
        private readonly Image[] worldRings = new Image[4];
        private readonly Button[] lightTiles = new Button[3];
        private readonly Image[] lightFaces = new Image[3];
        private int trayTab = TrayClosed;
        private StageWorld stageWorld = StageWorld.Studio;
        private StageLight stageLight = StageLight.Day;
        private string stageCollectionId = "";

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

            stageTray = Rect("Stage tray", safeRoot); Surface(stageTray, new Color(PockleTheme.Milk.r, PockleTheme.Milk.g, PockleTheme.Milk.b, .94f), 28);
            stageTray.GetComponent<Image>().raycastTarget = true; // touches on the tray never reach the toy
            for (int i = 0; i < 3; i++)
            {
                int tab = i;
                stageTabs[i] = CreateButton(TrayTabNames[i], stageTray, Vector2.zero, new Vector2(100, 40), Quiet,
                    () => SetTrayTab(trayTab == tab ? TrayClosed : tab), out stageTabFaces[i], PockleTheme.CaptionSize);
                stageTabs[i].name = "Stage tab · " + TrayTabNames[i];
            }
            stagePanel = Rect("Stage panel", stageTray);
            for (int i = 0; i < 3; i++) stagePanels[i] = Rect(TrayTabNames[i] + " panel", stagePanel);

            // Collection: the members as dots, then the way to find the rest.
            var collectionPanel = stagePanels[0];
            stageCollectionTitle = Label("", collectionPanel, PockleTheme.BodySize, Ink, FontStyle.Bold, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(200, 24));
            stageCount = Label("", collectionPanel, PockleTheme.SmallSize, MutedInk, FontStyle.Normal, TextAnchor.MiddleRight, Vector2.zero, new Vector2(120, 24));
            stageFindMore = CreateButton("Find the rest in boxes", collectionPanel, Vector2.zero, new Vector2(300, 44), Primary,
                () => Navigate(AppPage.Boxes), out _, PockleTheme.CaptionSize + 1);
            stageFindMore.name = "Stage find more";

            // World: four swatches with names.
            for (int i = 0; i < 4; i++)
            {
                var world = StageLook.Worlds[i];
                var ringRect = Rect("World ring", stagePanels[1]); worldRings[i] = Surface(ringRect, Ink, 20);
                worldTiles[i] = CreateButton(world.ToString(), stagePanels[1], Vector2.zero, new Vector2(72, 92), Quiet,
                    () => SetStageWorld(world), out worldFaces[i], PockleTheme.SmallSize - 1, false);
                worldTiles[i].name = "Stage world · " + world;
                var caption = worldTiles[i].GetComponentInChildren<Text>().rectTransform;
                caption.anchorMin = new Vector2(0, 0); caption.anchorMax = new Vector2(1, 0); caption.pivot = new Vector2(.5f, 0);
                caption.offsetMin = new Vector2(4, 6); caption.offsetMax = new Vector2(-4, 30);
            }
            stageWorldNote = Label("", stagePanels[1], PockleTheme.SmallSize - 1, MutedInk, FontStyle.Normal, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(300, 20));

            // Light: three moods.
            for (int i = 0; i < 3; i++)
            {
                var light = StageLook.Lights[i];
                lightTiles[i] = CreateButton(StageLook.Name(light), stagePanels[2], Vector2.zero, new Vector2(100, 56), Quiet,
                    () => SetStageLight(light), out lightFaces[i], PockleTheme.CaptionSize, false);
                lightTiles[i].name = "Stage light · " + light;
            }
            stageLightNote = Label("", stagePanels[2], PockleTheme.SmallSize - 1, MutedInk, FontStyle.Normal, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(300, 20));
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
            for (int i = 0; i < 3; i++)
            {
                bool on = i == tab;
                stagePanels[i].gameObject.SetActive(on);
                stageTabFaces[i].color = on ? Primary : new Color(0, 0, 0, 0);
                stageTabs[i].GetComponentInChildren<Text>().color = on ? PockleTheme.OnPlum : Ink;
            }
            stagePanel.gameObject.SetActive(tab != TrayClosed);
            if (lastLayoutSize.x > 0 && page == AppPage.Play) AdaptLayout();
        }

        private void SetStageWorld(StageWorld world)
        {
            stageWorld = world; PlayerPrefs.SetInt(StageLook.WorldKey, (int)world);
            RefreshStage(); WorldChanged?.Invoke(world); BackdropChanged?.Invoke(StageBackdrop);
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
            stageTray.gameObject.SetActive(play && known);
            string rarity = known ? RarityText(toy) : "";
            stageRarity.gameObject.SetActive(play && rarity.Length > 0);
            if (!play) { heading.color = Ink; return; }

            bool dark = StageLook.IsDark(stageWorld, stageLight);
            heading.color = dark ? PockleTheme.OnPlum : Ink;
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

            if (stageCollectionId != collection.Id) RebuildFinishDots(collection);
            Color accent = ToyCatalog.TryGetCharacter(collection.CharacterId, out var character) ? CatalogUnityColor(character.AccentColor) : PockleTheme.Jelly;
            for (int i = 0; i < finishDots.Count; i++)
            {
                string id = finishDotIds[i];
                bool owned = session != null && session.GetOwnedCount(id) > 0;
                bool playable = session != null && session.IsOwnedAndPlayable(id);
                bool secret = collection.SecretId == id;
                finishDots[i].interactable = playable;
                finishDotFaces[i].color = !owned ? PockleTheme.Mystery : secret ? PockleTheme.Confetti : accent;
                finishDots[i].GetComponentInChildren<Text>().text = owned ? "" : "?";
                finishDotRings[i].gameObject.SetActive(id == selectedCollectibleId);
                string label = ToyCatalog.TryGetCollectible(id, out var member) && owned ? FinishName(member) : "Not found yet";
                finishDots[i].name = "Stage finish · " + id + " · " + label;
            }

            for (int i = 0; i < 4; i++)
            {
                var world = StageLook.Worlds[i];
                Color swatch = StageLook.Swatch(world, toy.CharacterId, PockleTheme.FieldFor(toy.Id));
                worldFaces[i].color = swatch;
                worldTiles[i].GetComponentInChildren<Text>().text = StageLook.Name(world, toy.CharacterId);
                worldTiles[i].GetComponentInChildren<Text>().color = world == StageWorld.Midnight ? PockleTheme.OnPlum : Ink;
                worldRings[i].gameObject.SetActive(world == stageWorld);
            }
            stageWorldNote.text = StageLook.HomeWorldName(toy.CharacterId) + " comes with the " + collection.DisplayName + " collection.";
            for (int i = 0; i < 3; i++)
            {
                bool on = StageLook.Lights[i] == stageLight;
                lightFaces[i].color = on ? Primary : Quiet;
                lightTiles[i].GetComponentInChildren<Text>().color = on ? PockleTheme.OnPlum : Ink;
            }
            stageLightNote.text = StageLook.Note(stageLight, toy.Id == ToyCatalog.FoxAuroraId);
        }

        private void RebuildFinishDots(CollectionDefinition collection)
        {
            foreach (var dot in finishDots) Destroy(dot.gameObject);
            foreach (var ring in finishDotRings) Destroy(ring.gameObject);
            finishDots.Clear(); finishDotFaces.Clear(); finishDotRings.Clear(); finishDotIds.Clear();
            stageCollectionId = collection.Id;
            foreach (string member in collection.Members)
            {
                string id = member;
                var ringRect = Rect("Finish ring", stagePanels[0]);
                finishDotRings.Add(Surface(ringRect, Ink, 18));
                var dot = CreateButton("?", stagePanels[0], Vector2.zero, new Vector2(28, 28), PockleTheme.Mystery,
                    () => { if (selectedCollectibleId != id) ShowToy(id); }, out var face, PockleTheme.SmallSize);
                var colours = dot.colors; colours.disabledColor = Color.white; dot.colors = colours;
                dot.GetComponentInChildren<Text>().color = MutedInk;
                finishDots.Add(dot); finishDotFaces.Add(face); finishDotIds.Add(id);
            }
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
            playHintPill.anchorMin = playHintPill.anchorMax = playHintPill.pivot = new Vector2(0, 1);
            playHintPill.anchoredPosition = new Vector2(left, stageRarity.anchoredPosition.y < -120 ? -178 : -144);
            playHintPill.sizeDelta = new Vector2(Mathf.Min(width, playHint.preferredWidth + 32), 34);

            // Tray: a closed strip of three tabs; an open tab adds its panel above the strip.
            float trayWidth = Mathf.Min(width + 16, 380);
            float inner = trayWidth - 16;
            float panelHeight = trayTab == 0 ? 132 : trayTab == 1 ? 138 : trayTab == 2 ? 100 : 0;
            float trayHeight = 56 + panelHeight;
            stageTray.anchorMin = stageTray.anchorMax = stageTray.pivot = new Vector2(.5f, 0);
            stageTray.anchoredPosition = new Vector2(0, 12); stageTray.sizeDelta = new Vector2(trayWidth, trayHeight);
            float tabWidth = (inner - 8) / 3;
            for (int i = 0; i < 3; i++) LeftLabel(stageTabs[i].GetComponent<RectTransform>(), new Vector2(8 + i * (tabWidth + 4), -(panelHeight + 8)), new Vector2(tabWidth, 40));
            LeftLabel(stagePanel, new Vector2(8, -8), new Vector2(inner, panelHeight));
            foreach (var panel in stagePanels) LeftLabel(panel, Vector2.zero, new Vector2(inner, panelHeight));

            float pad = 8, row = inner - pad * 2;
            LeftLabel(stageCollectionTitle.rectTransform, new Vector2(pad, -4), new Vector2(row * .6f, 24));
            LeftLabel(stageCount.rectTransform, new Vector2(pad + row * .6f, -4), new Vector2(row * .4f, 24));
            int dots = finishDots.Count;
            float step = dots > 1 ? Mathf.Min(40, (row - 28) / (dots - 1)) : 0;
            float start = pad + (row - (step * (dots - 1) + 28)) / 2;
            for (int i = 0; i < dots; i++)
            {
                LeftLabel(finishDots[i].GetComponent<RectTransform>(), new Vector2(start + i * step, -40), new Vector2(28, 28));
                LeftLabel(finishDotRings[i].rectTransform, new Vector2(start + i * step - 3, -37), new Vector2(34, 34));
            }
            LeftLabel(stageFindMore.GetComponent<RectTransform>(), new Vector2(pad, -82), new Vector2(row, 44));

            float tile = Mathf.Min(80, (row - 24) / 4);
            for (int i = 0; i < 4; i++)
            {
                float x = pad + i * (row - tile) / 3;
                LeftLabel(worldTiles[i].GetComponent<RectTransform>(), new Vector2(x, -6), new Vector2(tile, 96));
                LeftLabel(worldRings[i].rectTransform, new Vector2(x - 3, -3), new Vector2(tile + 6, 102));
            }
            LeftLabel(stageWorldNote.rectTransform, new Vector2(pad, -110), new Vector2(row, 20));
            float lightWidth = (row - 16) / 3;
            for (int i = 0; i < 3; i++) LeftLabel(lightTiles[i].GetComponent<RectTransform>(), new Vector2(pad + i * (lightWidth + 8), -6), new Vector2(lightWidth, 56));
            LeftLabel(stageLightNote.rectTransform, new Vector2(pad, -72), new Vector2(row, 20));
        }
    }
}
