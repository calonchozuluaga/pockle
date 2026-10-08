using UnityEngine;

namespace Pockle.Runtime
{
    /// <summary>Four one-time 256px renders, using the actual toy mesh and finish. No idle cameras.</summary>
    internal static class ToyPortrait
    {
        public static RenderTexture Render(PipVariant variant)
        {
            var root = new GameObject("Shelf preview · " + variant);
            root.transform.position = new Vector3(1000, 0, 0);
            var toyObject = new GameObject("Shelf toy"); toyObject.transform.SetParent(root.transform, false);
            var toy = toyObject.AddComponent<JellyToy>(); toy.Initialize(); toy.SetVariant(variant);
            foreach (Transform child in toyObject.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = 30;
            var cameraObject = new GameObject("Shelf portrait camera", typeof(Camera)); cameraObject.transform.SetParent(root.transform, false);
            var camera = cameraObject.GetComponent<Camera>(); camera.enabled = false;
            camera.cullingMask = 1 << 30; camera.orthographic = true; camera.orthographicSize = 1.13f;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.978f, .962f, .943f);
            camera.nearClipPlane = .1f; camera.farClipPlane = 10; camera.allowHDR = false; camera.allowMSAA = false;
            camera.transform.localPosition = new Vector3(0, .18f, -5);
            camera.transform.LookAt(root.transform.position + new Vector3(0, .05f, 0));
            var texture = new RenderTexture(256, 256, 16, RenderTextureFormat.ARGB32)
            { name = "Shelf · " + variant, filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            texture.Create(); camera.targetTexture = texture; camera.Render(); camera.targetTexture = null;
            root.SetActive(false); Object.Destroy(root);
            return texture;
        }
    }
}
