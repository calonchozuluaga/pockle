using UnityEngine;
using UnityEngine.Rendering;

namespace Pockle.Runtime
{
    /// <summary>A deliberately small, entirely procedural presentation stage.</summary>
    public static class PrototypeStage
    {
        public static readonly Color Background = new Color(0.973f, 0.957f, 0.933f, 1f);

        /// <summary>Set both stage cameras to an opaque colour field. False if no Pockle stage is active.</summary>
        public static bool SetBackdrop(Color colour) => SetBackdrop(Camera.main, colour);

        /// <summary>Explicit viewer overload for isolated stages; never changes portrait or unrelated cameras.</summary>
        public static bool SetBackdrop(Camera viewer, Color colour)
        {
            if (viewer == null) return false;
            var framing = viewer.GetComponent<StageFraming>();
            return framing != null && framing.SetBackdrop(colour);
        }

        public static bool SetWorld(StageWorld world) => SetWorld(Camera.main, world, "pip.peach-jelly");
        public static bool SetWorld(Camera viewer, StageWorld world, string collectibleId)
        {
            var environment = viewer == null ? null : viewer.GetComponent<ToyStageEnvironment>();
            return environment != null && environment.SetWorld(world, collectibleId);
        }
        public static bool SetEnvironment(Camera viewer, string worldId, string collectibleId)
        {
            var environment = viewer == null ? null : viewer.GetComponent<ToyStageEnvironment>();
            return environment != null && environment.SetEnvironment(worldId, collectibleId);
        }

        public static bool SetLight(StageLight light) => SetLight(Camera.main, light);
        public static bool SetLight(Camera viewer, StageLight light)
        {
            var environment = viewer == null ? null : viewer.GetComponent<ToyStageEnvironment>();
            return environment != null && environment.SetLight(light);
        }

        public static void SetViewport(Camera viewer, Rect pixels) => viewer?.GetComponent<StageFraming>()?.SetViewport(pixels);
        public static void SetSoftFraming(Camera viewer,bool soft) { var f=viewer?.GetComponent<StageFraming>();if(f!=null)f.Soft=soft; }
        public static void SetZoom(Camera viewer, float zoom) { var f=viewer?.GetComponent<StageFraming>(); if(f!=null)f.Zoom=zoom; }

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

            // A full-screen background camera holds the painted world independently of toy framing.
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

            turntable = new GameObject("Toy orbit pivot").transform;
            turntable.SetParent(stage.transform, false);
            turntable.position = new Vector3(0,1.15f,0);
            var mount = new GameObject("Pip mount");
            mount.transform.SetParent(turntable, false);
            mount.transform.position = new Vector3(0f, 1.15f, 0f);
            toyMount = mount.transform;

            var baseMaterial = MakeMaterial("Ceramic · lower rim", new Color(0.91f, 0.857f, 0.79f), 0.18f);
            var topMaterial = MakeMaterial("Ceramic · warm ivory", new Color(1f, 0.985f, 0.957f), 0.24f);
            var shadowMaterial = MakeMaterial("Soft contact shadow", new Color(0.91f, 0.835f, 0.762f), 0f);
            Shader softShadow = Shader.Find("Pockle/Soft Blush");
            if (softShadow != null && softShadow.isSupported)
            {
                Object.Destroy(shadowMaterial);
                shadowMaterial = new Material(softShadow) { name = "Pip · radial contact shadow", color = new Color(.54f, .36f, .43f, .20f) };
                shadowMaterial.SetFloat("_GroundPlane", 1f);
                shadowMaterial.renderQueue = 2990; // Behind transparent Pip, over the opaque plate.
            }
            resources.Materials = new[] { baseMaterial, topMaterial, shadowMaterial };
            Mesh diskMesh = PresentationMeshes.CreateDisk();
            resources.Mesh = diskMesh;
            // Ground contact stays in world space while the toy orbits around its centre.
            contactShadow = MakeDisk(stage.transform, "Pip · contact shadow", new Vector3(0f, .151f, .04f),
                new Vector3(1.88f, .0005f, 1.58f), shadowMaterial, diskMesh);
            plateCollider = null; // Kept as a compatibility output; the viewer has no plate or picking surface.

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
            cameraObject.AddComponent<ToyStageEnvironment>().Initialize(camera, toyMount, key, fill, baseMaterial, topMaterial);

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

        private static Transform MakeDisk(Transform parent, string name, Vector3 position, Vector3 scale, Material material, Mesh mesh)
        {
            var disk = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            disk.GetComponent<MeshFilter>().sharedMesh = mesh;
            disk.transform.SetParent(parent, false);
            disk.transform.localPosition = position;
            disk.transform.localScale = scale;
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
        public Mesh Mesh;

        private void OnDestroy()
        {
            if (Mesh != null) Destroy(Mesh);
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
        private Rect requested;
        private Rect frame;
        private bool initialized;
        public float Zoom = 1;
        public bool Soft;
        public void SetViewport(Rect pixels)
        {
            if(float.IsNaN(pixels.x)||float.IsNaN(pixels.y)||float.IsNaN(pixels.width)||float.IsNaN(pixels.height)||
                float.IsInfinity(pixels.x)||float.IsInfinity(pixels.y)||float.IsInfinity(pixels.width)||float.IsInfinity(pixels.height)||pixels.width<=0||pixels.height<=0)return;
            requested=pixels;
        }

        public void Initialize(Camera viewerCamera, Camera backgroundCamera)
        {
            viewer = viewerCamera;
            background = backgroundCamera;
        }

        public bool SetBackdrop(Color colour)
        {
            if (viewer == null || background == null) return false;
            if (float.IsNaN(colour.r) || float.IsInfinity(colour.r)
                || float.IsNaN(colour.g) || float.IsInfinity(colour.g)
                || float.IsNaN(colour.b) || float.IsInfinity(colour.b)) return false;
            colour = new Color(Mathf.Clamp01(colour.r), Mathf.Clamp01(colour.g), Mathf.Clamp01(colour.b), 1f);
            viewer.backgroundColor = colour;
            background.backgroundColor = colour;
            return true;
        }

        private void LateUpdate()
        {
            if (viewer == null || Screen.width < 1 || Screen.height < 1) return;

            width = Screen.width;
            height = Screen.height;
            safeArea = Screen.safeArea;
            var landscape = (float)width / height > 1.15f;
            background.enabled = true;
            viewer.clearFlags = CameraClearFlags.Depth;
            // The play screen reserves only its identity header and a short gesture hint.
            var pixelsPerUnit = height / (landscape ? 390f : 844f);
            var top = 144f * pixelsPerUnit;
            var bottom = 120f * pixelsPerUnit;
            var viewportHeight = Mathf.Max(1f, safeArea.height - top - bottom);
            var viewportWidth = safeArea.width;
            Rect pixels=requested.width>0?requested:new Rect(safeArea.xMin,safeArea.yMin+bottom,viewportWidth,viewportHeight);
            float left=Mathf.Clamp(pixels.xMin,safeArea.xMin,safeArea.xMax-1);
            float low=Mathf.Clamp(pixels.yMin,safeArea.yMin,safeArea.yMax-1);
            float right=Mathf.Clamp(pixels.xMax,left+1,safeArea.xMax);
            float high=Mathf.Clamp(pixels.yMax,low+1,safeArea.yMax);
            Rect target=new Rect(left/width,low/height,(right-left)/width,(high-low)/height);
            float t=initialized?1-Mathf.Exp(-Time.unscaledDeltaTime/ .07f):1;
            var r=frame;
            frame=new Rect(Mathf.Lerp(r.x,target.x,t),Mathf.Lerp(r.y,target.y,t),Mathf.Lerp(r.width,target.width,t),Mathf.Lerp(r.height,target.height,t));
            initialized=true;
            // Frame the toy in the available HUD area without clipping rendering to that area.
            viewer.rect = new Rect(0, 0, 1, 1);
            float screenAspect = (float)width / height;
            float frameAspect = screenAspect * frame.width / frame.height;
            float halfHeight = Pockle.Core.ToyOrbit.FrameSize(frameAspect, Zoom, Soft) / frame.height;
            float halfWidth = halfHeight * screenAspect;
            viewer.orthographicSize = halfHeight;
            Vector2 offset = new Vector2((1 - 2 * frame.center.x) * halfWidth, (1 - 2 * frame.center.y) * halfHeight);
            viewer.transform.LookAt(new Vector3(0f,1.15f,0f));
            viewer.projectionMatrix = Matrix4x4.Ortho(offset.x - halfWidth, offset.x + halfWidth,
                offset.y - halfHeight, offset.y + halfHeight, viewer.nearClipPlane, viewer.farClipPlane);
        }
    }
}
