using System;
using UnityEngine;
using UnityEngine.UI;
namespace Pockle.Runtime
{
    /// <summary>Only renders a contents thumbnail when it scrolls into view; never creates 115 cameras up front.</summary>
    public sealed class CheckoutContentsPortrait : MonoBehaviour
    {
        private string id;
        private RectTransform viewport;
        private RawImage image;
        private Func<string, Texture> render;
        private readonly Vector3[] viewCorners = new Vector3[4], corners = new Vector3[4];
        private static int renderedFrame = -1;
        public void Initialize(string collectible, RectTransform view, RawImage target, Func<string, Texture> renderer)
        { id = collectible; viewport = view; image = target; render = renderer; }
        private void LateUpdate()
        {
            if (viewport == null || image == null || render == null || Time.frameCount == renderedFrame) return;
            viewport.GetWorldCorners(viewCorners); ((RectTransform)transform).GetWorldCorners(corners);
            if (corners[2].y <= viewCorners[0].y || corners[0].y >= viewCorners[2].y || viewport.rect.width < 1) return;
            renderedFrame = Time.frameCount;
            image.texture = render(id); image.color = image.texture != null ? Color.white : Color.clear;
            render = null; enabled = false;
        }
    }
}
