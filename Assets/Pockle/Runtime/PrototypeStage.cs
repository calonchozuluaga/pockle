using UnityEngine;
using UnityEngine.Rendering;

namespace Pockle.Runtime
{
    /// <summary>A deliberately small, entirely procedural presentation stage.</summary>
    public static class PrototypeStage
    {
        public static readonly Color Background = new Color(0.973f, 0.957f, 0.933f, 1f);

        public static void Create(out Camera camera, out Transform toyMount, out Transform turntable, out Collider plateCollider, out Transform contactShadow)
        {
            var stage = new GameObject("Pockle · presentation stage");
            var resources = stage.AddComponent<StageResources>();

            var cameraObject = new GameObject("Pockle camera", typeof(Camera), typeof(AudioListener));
            cameraObject.transform.SetParent(stage.transform, false);
            cameraObject.tag = "MainCamera";
            camera = cameraObject.GetComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 3.2f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 24f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Background;
            camera.allowHDR = false;
            camera.allowMSAA = false;
            camera.transform.position = new Vector3(0f, 2.3f, -6f);
            camera.transform.LookAt(new Vector3(0f, 1.2f, 0f));

            // A clear-only camera lets the landscape viewer use a smaller viewport
            // while keeping the whole screen the same warm background colour.
            var backgroundObject = new GameObject("Full-screen background", typeof(Camera));
            backgroundObject.transform.SetParent(stage.transform, false);
            var background = backgroundObject.GetComponent<Camera>();
            background.depth = -2f;
            background.cullingMask = 0;
            background.clearFlags = CameraClearFlags.SolidColor;
            background.backgroundColor = Background;
            background.allowHDR = false;
            background.allowMSAA = false;
            background.enabled = true;
            var framing = cameraObject.AddComponent<StageFraming>();
            framing.Initialize(camera, background);

            turntable = new GameObject("Pip and plate · turntable").transform;
            turntable.SetParent(stage.transform, false);
            var mount = new GameObject("Pip mount");
            mount.transform.SetParent(turntable, false);
            mount.transform.position = new Vector3(0f, 1.15f, 0f);
            toyMount = mount.transform;

            var baseMaterial = MakeMaterial("Ceramic · lower rim", new Color(0.91f, 0.857f, 0.79f), 0.18f);
            var topMaterial = MakeMaterial("Ceramic · warm ivory", new Color(1f, 0.985f, 0.957f), 0.24f);
            var shadowMaterial = MakeMaterial("Soft contact shadow", new Color(0.91f, 0.835f, 0.762f), 0f);
            resources.Materials = new[] { baseMaterial, topMaterial, shadowMaterial };
            Transform lower = MakeDisk(turntable, "Pedestal · lower rim", new Vector3(0f, 0.045f, 0f),
                new Vector3(2.68f, 0.035f, 2.68f), baseMaterial);
            MakeDisk(turntable, "Pedestal · ceramic top", new Vector3(0f, 0.1f, 0f),
                new Vector3(2.46f, 0.05f, 2.46f), topMaterial);
            contactShadow = MakeDisk(turntable, "Pip · contact shadow", new Vector3(0f, 0.151f, 0.04f),
                new Vector3(1.36f, 0.0005f, 1.22f), shadowMaterial);
            // A thin cylinder covers the exposed plate. CapsuleCollider cannot
            // represent this nonuniform scale: its radius would swallow the toy.
            var touchSurface = new GameObject("Plate touch surface", typeof(MeshCollider));
            touchSurface.transform.SetParent(turntable, false);
            touchSurface.transform.localPosition = new Vector3(0f, .075f, 0f);
            touchSurface.transform.localScale = new Vector3(2.68f, .075f, 2.68f);
            var meshCollider = touchSurface.GetComponent<MeshCollider>();
            meshCollider.sharedMesh = lower.GetComponent<MeshFilter>().sharedMesh;
            plateCollider = meshCollider;
            // Small inset marks make the circular plate's rotation visible.
            for (int i = -1; i <= 1; i++)
            {
                var radians = (i * 14f + 180f) * Mathf.Deg2Rad;
                MakeDisk(turntable, "Turntable rim mark", new Vector3(Mathf.Sin(radians) * 1.13f, .151f, Mathf.Cos(radians) * 1.13f),
                    new Vector3(.055f, .0005f, .055f), baseMaterial);
            }

            var keyObject = new GameObject("Soft studio key", typeof(Light));
            keyObject.transform.SetParent(stage.transform, false);
            keyObject.transform.rotation = Quaternion.Euler(38f, -32f, 0f);
            var key = keyObject.GetComponent<Light>();
            key.type = LightType.Directional;
            key.color = new Color(1f, 0.955f, 0.905f);
            key.intensity = 1.05f;
            key.shadows = LightShadows.None;

            var fillObject = new GameObject("Studio fill", typeof(Light));
            fillObject.transform.SetParent(stage.transform, false);
            fillObject.transform.rotation = Quaternion.Euler(16f, 145f, 0f);
            var fill = fillObject.GetComponent<Light>();
            fill.type = LightType.Directional;
            fill.color = new Color(0.9f, 0.935f, 1f);
            fill.intensity = 0.34f;
            fill.shadows = LightShadows.None;

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.69f, 0.65f, 0.66f);
            RenderSettings.fog = false;
        }

        private static Material MakeMaterial(string name, Color colour, float smoothness)
        {
            var shader = Shader.Find("Pockle/Soft Accent");
            if (shader == null) shader = Shader.Find("Legacy Shaders/Diffuse");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            var material = new Material(shader) { name = name, color = colour };
            if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", smoothness);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
            return material;
        }

        private static Transform MakeDisk(Transform parent, string name, Vector3 position, Vector3 scale, Material material)
        {
            var disk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            disk.name = name;
            disk.transform.SetParent(parent, false);
            disk.transform.localPosition = position;
            disk.transform.localScale = scale;
            var collider = disk.GetComponent<Collider>();
            if (collider != null) { collider.enabled = false; Object.Destroy(collider); }
            var renderer = disk.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return disk.transform;
        }
    }

    internal sealed class StageResources : MonoBehaviour
    {
        public Material[] Materials;

        private void OnDestroy()
        {
            if (Materials == null) return;
            foreach (var material in Materials)
                if (material != null) Destroy(material);
        }
    }

    internal sealed class StageFraming : MonoBehaviour
    {
        private Camera viewer;
        private Camera background;
        private int width;
        private int height;
        private Rect safeArea;

        public void Initialize(Camera viewerCamera, Camera backgroundCamera)
        {
            viewer = viewerCamera;
            background = backgroundCamera;
        }

        private void LateUpdate()
        {
            if (viewer == null || Screen.width < 1 || Screen.height < 1) return;
            if (width == Screen.width && height == Screen.height && safeArea == Screen.safeArea) return;
            width = Screen.width;
            height = Screen.height;
            safeArea = Screen.safeArea;
            var landscape = (float)width / height > 1.15f;
            background.enabled = true;
            viewer.clearFlags = CameraClearFlags.Depth;
            // The play screen reserves only its identity header and a short gesture hint.
            var pixelsPerUnit = height / (landscape ? 390f : 844f);
            var top = 144f * pixelsPerUnit;
            var bottom = 72f * pixelsPerUnit;
            var viewportHeight = Mathf.Max(1f, safeArea.height - top - bottom);
            var viewportWidth = safeArea.width;
            viewer.rect = new Rect(safeArea.xMin / width, (safeArea.yMin + bottom) / height,
                viewportWidth / width, viewportHeight / height);
            // Reserve at least 3.9 world units vertically and 3 horizontally, so
            // tall screens keep the full plate and stretched crown in view.
            viewer.orthographicSize = Mathf.Max(1.95f, 1.5f * viewportHeight / viewportWidth);
            viewer.transform.LookAt(new Vector3(0f, 1.72f, 0f));
        }
    }
}
