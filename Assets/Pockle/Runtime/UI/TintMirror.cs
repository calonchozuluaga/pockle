using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Pockle.Runtime
{
    /// <summary>
    /// A button's colour tint (pressed, highlighted, disabled) only touches its own background. Toy portraits carry
    /// their own baked background in the tile's colour, so without this they show as a lighter square while the tile
    /// is pressed. This copies the background's current tint onto the pictures inside the tile every frame.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TintMirror : MonoBehaviour
    {
        private Graphic source;
        private readonly List<Graphic> targets = new List<Graphic>();

        /// <summary>Makes <paramref name="picture"/> take the same press tint as <paramref name="button"/>'s background.</summary>
        public static void Follow(Selectable button, Graphic picture)
        {
            if (button == null || picture == null) return;
            var mirror = button.GetComponent<TintMirror>() ?? button.gameObject.AddComponent<TintMirror>();
            mirror.source = button.targetGraphic;
            if (!mirror.targets.Contains(picture)) mirror.targets.Add(picture);
        }

        private void LateUpdate()
        {
            if (source == null) return;
            Color tint = source.canvasRenderer.GetColor();
            for (int i = 0; i < targets.Count; i++)
                if (targets[i] != null && targets[i].isActiveAndEnabled) targets[i].canvasRenderer.SetColor(tint);
        }
    }
}
