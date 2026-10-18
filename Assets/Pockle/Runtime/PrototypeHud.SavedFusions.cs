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
            var records = session != null ? session.Progress.Save.Fusions : null;
            string keys = records == null ? "" : string.Join("|", records.FindAll(f => f != null && session.IsOwnedAndPlayable(f.Id)).ConvertAll(f => f.Id));
            savedFusionsRoot.gameObject.SetActive(keys.Length > 0);
            if (keys.Length == 0) return y;
            if (keys != savedFusionKeys)
            {
                foreach (Transform child in savedFusionsRoot) { child.gameObject.SetActive(false); Destroy(child.gameObject); }
                savedFusionTiles.Clear(); savedFusionKeys = keys;
                var title = Heading("Fusions · yours to keep", savedFusionsRoot);
                LeftLabel(title, Vector2.zero, new Vector2(width, 30));
                foreach (var record in records)
                {
                    if (record == null || !FusionCatalog.TryGet(record.Id, out _)) continue;
                    string id = record.Id;
                    if (!session.IsOwnedAndPlayable(id)) continue;
                    var tile = CreateButton(record.Name, savedFusionsRoot, Vector2.zero, Vector2.zero, Paper, () => ShowToy(id), out _, PockleTheme.SmallSize);
                    tile.name = "Saved fusion · " + id;
                    var label = tile.GetComponentInChildren<Text>();
                    label.alignment = TextAnchor.LowerCenter;
                    var image = Rect("Fused portrait", tile.transform);
                    image.anchorMin = new Vector2(0, .23f); image.anchorMax = Vector2.one; image.offsetMin = image.offsetMax = Vector2.zero;
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
