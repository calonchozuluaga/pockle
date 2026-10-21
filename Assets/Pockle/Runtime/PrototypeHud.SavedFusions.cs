using System.Collections.Generic;
using Pockle.Core;
using UnityEngine;
using UnityEngine.UI;
namespace Pockle.Runtime
{
    public sealed partial class PrototypeHud
    {
        private RectTransform savedFusionsRoot;
        private readonly List<Button> savedFusionTiles = new List<Button>();
        private string savedFusionKeys = "";
        private float LayoutSavedFusions(float width, float y)
        {
            if (savedFusionsRoot == null) savedFusionsRoot = Rect("Saved fusions", shelfRoot);
            // Every fused toy you own: saved fusions, then the Pip Fusion Lab studies (they live here, not as a collection).
            var ids = new List<string>();
            var records = session != null ? session.Progress.Save.Fusions : null;
            if (records != null) foreach (var record in records)
                    if (record != null && FusionCatalog.TryGet(record.Id, out _) && session.IsOwnedAndPlayable(record.Id)) ids.Add(record.Id);
            if (session != null) foreach (string study in PipFusionStudies.Ids) if (session.IsOwnedAndPlayable(study)) ids.Add(study);
            string keys = string.Join("|", ids);
            savedFusionsRoot.gameObject.SetActive(keys.Length > 0);
            if (keys.Length == 0) return y;
            if (keys != savedFusionKeys)
            {
                foreach (Transform child in savedFusionsRoot) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
                savedFusionTiles.Clear(); savedFusionKeys = keys;
                var title = Heading("Fusions · yours to keep", savedFusionsRoot);
                LeftLabel(title, Vector2.zero, new Vector2(width, 30));
                foreach (string fusedId in ids)
                {
                    string id = fusedId;
                    var tile = CreateButton(PersonalityBook.ShortName(id), savedFusionsRoot, Vector2.zero, Vector2.zero, Paper, () => ShowToy(id), out _, PockleTheme.SmallSize);
                    tile.name = "Saved fusion · " + id;
                    // The name gets its own strip under the picture: one or two short lines that shrink to fit, never over the toy.
                    var label = tile.GetComponentInChildren<Text>();
                    label.alignment = TextAnchor.MiddleCenter; label.horizontalOverflow = HorizontalWrapMode.Wrap; label.verticalOverflow = VerticalWrapMode.Truncate;
                    label.resizeTextForBestFit = true; label.resizeTextMinSize = 11; label.resizeTextMaxSize = PockleTheme.SmallSize + 1;
                    var lr = label.rectTransform; lr.anchorMin = Vector2.zero; lr.anchorMax = new Vector2(1, 0); lr.pivot = new Vector2(.5f, 0);
                    lr.offsetMin = new Vector2(4, 4); lr.offsetMax = new Vector2(-4, 40);
                    var image = Rect("Fused portrait", tile.transform);
                    image.anchorMin = Vector2.zero; image.anchorMax = Vector2.one; image.offsetMin = new Vector2(4, 44); image.offsetMax = new Vector2(-4, -2);
                    var portrait = image.gameObject.AddComponent<RawImage>(); portrait.texture = CutoutPortrait(id); portrait.raycastTarget = false;
                    savedFusionTiles.Add(tile);
                }
            }
            const int columns = 3; const float gap = 8;
            float tileWidth = (width - gap * (columns - 1)) / columns, height = tileWidth + 48;
            for (int i = 0; i < savedFusionTiles.Count; i++)
                LeftLabel(savedFusionTiles[i].GetComponent<RectTransform>(), new Vector2(i % columns * (tileWidth + gap), -40 - i / columns * (height + gap)), new Vector2(tileWidth, height));
            float total = 40 + ((savedFusionTiles.Count + columns - 1) / columns) * (height + gap);
            LeftLabel(savedFusionsRoot, new Vector2(0, -y), new Vector2(width, total));
            return y + total + 20;
        }
    }
}
