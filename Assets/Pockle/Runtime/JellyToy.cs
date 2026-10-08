using System.Collections.Generic;
using Pockle.Core;
using UnityEngine;

namespace Pockle.Runtime
{
    /// <summary>
    /// Pip's authored art pass, with the procedural baseline retained for comparison.
    /// Blender supplies the rest mesh and anchors; interaction remains entirely local.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class JellyToy : MonoBehaviour
    {
        private const int LatitudeSegments = 28;
        private const int LongitudeSegments = 36;
        private const float ChangeThreshold = 0.0001f;

        private readonly List<Mesh> ownedMeshes = new List<Mesh>();
        private readonly List<Material> ownedMaterials = new List<Material>();
        private readonly List<SurfaceAccent> accents = new List<SurfaceAccent>();
        private readonly Vector3[] mouthRest = new Vector3[13];
        private readonly Vector3[] mouthPositions = new Vector3[13];
        private Mesh bodyMesh;
        private PipCharacterAsset characterAsset;
        private Vector3[] groupNormals;
        private Mesh accentMesh;
        private Mesh sparkleMesh;
        private Mesh starMesh;
        private Cubemap studioReflection;
        private Vector3[] restVertices;
        private Vector3[] vertices;
        private Vector3[] normals;
        private int[] triangles;
        private LineRenderer mouth;
        private Transform visualRoot;
        private Material bodyMaterial;
        private Material eyeMaterial;
        private Material smileMaterial;
        private Material highlightMaterial;
        private Material cheekMaterial;
        private Material bubbleMaterial;
        private Material sparkleMaterial;
        private Material silverMaterial;
        private PipVariant variant;
        private bool initialized;
        private bool haveDeformation;
        private float previousCompression;
        private float previousStretch;
        private float previousTiltX;
        private float previousTiltZ;
        private float previousContactX;
        private float previousContactZ;
        private float previousPinch;
        private Vector3 previousPinchAxis;
        private float previousSag, previousClearance;
        private Vector3 previousGrip;
        public float DeformedTop { get; private set; }

        public Transform VisualRoot { get { Initialize(); return visualRoot; } }
        public Renderer BodyRenderer { get; private set; }
        public bool UsesAuthoredMesh { get { return characterAsset != null; } }
        public PipVariant Variant { get { Initialize(); return variant; } }

        public void SetVariant(PipVariant choice)
        {
            Initialize();
            choice = PipVariants.FromSaved((int)choice);
            if (choice == variant) return;
            variant = choice;
            ApplyVariantMaterial();
            // Remove only filling pieces. Mesh, face, pose and turntable survive.
            for (int i = accents.Count - 1; i >= 0; i--)
            {
                if (accents[i].IsSurface) continue;
                accents[i].Transform.gameObject.SetActive(false);
                Destroy(accents[i].Transform.gameObject);
                accents.RemoveAt(i);
            }
            BuildSuspendedAccents();
            haveDeformation = false;
            SetDeformation(previousCompression, previousStretch, previousTiltX, previousTiltZ, previousContactX, previousContactZ, previousPinch, previousPinchAxis, previousSag, previousGrip, previousClearance);
            if (Application.isEditor || Debug.isDebugBuild)
                Debug.Log("Pip variant: " + PipVariants.Label(variant), this);
        }

        /// <summary>Pick the visible shell only at pointer-down, without recooking a collider.</summary>
        public bool RaycastBody(Ray worldRay, out Vector3 point)
        {
            Initialize();
            point = Vector3.zero;
            if (!gameObject.activeInHierarchy) return false;
            Matrix4x4 inverse = visualRoot.worldToLocalMatrix;
            var ray = new Ray(inverse.MultiplyPoint3x4(worldRay.origin), inverse.MultiplyVector(worldRay.direction));
            if (!bodyMesh.bounds.IntersectRay(ray)) return false;
            var origin = new Point3(ray.origin.x, ray.origin.y, ray.origin.z);
            var direction = new Point3(ray.direction.x, ray.direction.y, ray.direction.z);
            float nearest = float.PositiveInfinity;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 a = vertices[triangles[i]], b = vertices[triangles[i + 1]], c = vertices[triangles[i + 2]];
                float distance;
                if (SurfaceRaycast.TryTriangle(origin, direction, new Point3(a.x, a.y, a.z),
                    new Point3(b.x, b.y, b.z), new Point3(c.x, c.y, c.z), out distance) && distance < nearest)
                    nearest = distance;
            }
            if (float.IsInfinity(nearest)) return false;
            point = visualRoot.TransformPoint(ray.GetPoint(nearest));
            return true;
        }

        private sealed class SurfaceAccent
        {
            public Transform Transform;
            public Vector3 Rest;
            public Vector3 Scale;
            public bool IsSurface;
        }

        private void Awake()
        {
            Initialize();
        }

        /// <summary>Safe to call immediately after AddComponent and more than once.</summary>
        public void Initialize()
        {
            if (initialized) return;
            initialized = true;
            variant = PipVariants.FromSaved(PlayerPrefs.GetInt(PipVariants.Preference, 0));

            if (PlayerPrefs.GetInt(PipCharacterAsset.BaselinePreference, 0) == 0)
            {
                characterAsset = Resources.Load<PipCharacterAsset>(PipCharacterAsset.ResourcePath);
                if (characterAsset != null && (characterAsset.BodyMesh == null || !characterAsset.BodyMesh.isReadable))
                {
                    Debug.LogWarning("Pip's authored mesh is unreadable; using procedural baseline.", this);
                    characterAsset = null;
                }
                if (characterAsset == null)
                    Debug.LogWarning("Authored Pip is unavailable. Check the .pocklemesh import errors; using the baseline.", this);
            }
            visualRoot = new GameObject(characterAsset != null ? "Pip • authored peach jelly" : "Pip • procedural baseline").transform;
            visualRoot.SetParent(transform, false);
            if (characterAsset != null) studioReflection = StudioReflection.Create();
            bodyMaterial = CreateBodyMaterial();
            eyeMaterial = CreateFlatMaterial("Pip eyes", new Color(0.20f, 0.12f, 0.17f, 1f));
            smileMaterial = eyeMaterial;
            if (characterAsset != null)
            {
                eyeMaterial = CreateCandyMaterial("Pip glazed plum eyes", new Color(.15f, .025f, .075f, 1f), .85f);
                if (eyeMaterial.HasProperty("_TopColor")) eyeMaterial.SetColor("_TopColor", new Color(.24f, .045f, .12f, 1f));
                if (eyeMaterial.HasProperty("_BottomColor")) eyeMaterial.SetColor("_BottomColor", new Color(.075f, .008f, .035f, 1f));
                if (eyeMaterial.HasProperty("_RimColor")) eyeMaterial.SetColor("_RimColor", new Color(.27f, .07f, .13f, 1f));
                if (eyeMaterial.HasProperty("_ReflectionStrength")) eyeMaterial.SetFloat("_ReflectionStrength", .32f);
                // Opaque eyes write depth before the translucent shell. They stay
                // crisp in front and receive its peach tint when viewed from behind.
                if (eyeMaterial.HasProperty("_ZWrite")) eyeMaterial.SetFloat("_ZWrite", 1f);
                eyeMaterial.renderQueue = 2000;
            }
            highlightMaterial = CreateFlatMaterial("Pip eye glints", new Color(1f, 0.95f, 0.86f, 1f));
            cheekMaterial = CreateFlatMaterial("Pip blush", new Color(1f, 0.42f, 0.40f, 1f));
            if (characterAsset != null)
            {
                Shader blushShader = Shader.Find("Pockle/Soft Blush");
                if (blushShader != null && blushShader.isSupported)
                {
                    cheekMaterial = new Material(blushShader) { name = "Pip diffused blush", color = new Color(1f, .30f, .26f, .62f) };
                    ownedMaterials.Add(cheekMaterial);
                }
            }
            bubbleMaterial = CreateCandyMaterial("Suspended pearls", new Color(1f, 0.88f, 0.69f, 0.36f), 0.62f);
            // Draw suspended pearls before the shell so its peach tint covers them.
            bubbleMaterial.renderQueue = 2990;
            sparkleMaterial = CreateFlatMaterial("Apricot flecks", new Color(1f, 0.82f, 0.57f, 1f));
            silverMaterial = CreateCandyMaterial("Moon Jelly silver stars", new Color(.86f, .94f, 1f, 1f), .82f);
            if (silverMaterial.HasProperty("_ZWrite")) silverMaterial.SetFloat("_ZWrite", 1f);
            silverMaterial.renderQueue = 2000;
            ApplyVariantMaterial();

            if (characterAsset != null) BuildAuthoredBody();
            else BuildBody();
            accentMesh = BuildSphere("Shared accent sphere", characterAsset != null ? 16 : 12, characterAsset != null ? 12 : 10);
            sparkleMesh = BuildSparkle();
            starMesh = BuildStar();
            BuildFace();
            BuildCrown();
            BuildSuspendedAccents();

            var touchCollider = GetComponent<SphereCollider>();
            if (touchCollider == null) touchCollider = gameObject.AddComponent<SphereCollider>();
            touchCollider.center = Vector3.zero;
            touchCollider.radius = 1.03f;
            touchCollider.isTrigger = false;
            ResetToy();
            if (Application.isEditor || Debug.isDebugBuild)
                Debug.Log("Pip: " + (characterAsset != null ? "authored mesh" : "procedural baseline") + " · " + bodyMesh.vertexCount + " vertices", this);
            if (Application.isEditor || Debug.isDebugBuild)
                Debug.Log("Pip variant: " + PipVariants.Label(variant), this);
        }

        public void ResetToy()
        {
            Initialize();
            haveDeformation = false;
            SetDeformation(0f, 0f, 0f, 0f, 0f, 0f);
        }

        /// <summary>
        /// Updates preallocated mesh buffers. Normals and feature positions only change
        /// when the deformation changes; an idle toy has no per-frame work or allocations.
        /// </summary>
        public void SetDeformation(float compression, float stretch, float tiltX,
            float tiltZ, float contactX, float contactZ, float pinch = 0f, Vector3 pinchAxis = default(Vector3),
            float sag = 0f, Vector3 grip = default(Vector3), float clearance = 0f)
        {
            Initialize();
            // Match the portable shape's safe range so feature scale follows its shell,
            // including when a spring briefly overshoots zero on release.
            compression = SafeClamp(compression, 0f, 0.42f);
            stretch = SafeClamp(stretch, 0f, 0.6f);
            tiltX = SafeClamp(tiltX, -1f, 1f);
            tiltZ = SafeClamp(tiltZ, -1f, 1f);
            contactX = SafeClamp(contactX, -1.2f, 1.2f);
            contactZ = SafeClamp(contactZ, -1.2f, 1.2f);
            pinch = SafeClamp(pinch, -.35f, .55f);
            sag = SafeClamp(sag, 0f, .24f);
            clearance = SafeClamp(clearance, 0f, 1.05f);
            grip = new Vector3(SafeClamp(grip.x, -1.5f, 1.5f), SafeClamp(grip.y, -1.5f, 2f), SafeClamp(grip.z, -1.5f, 1.5f));
            if (!float.IsNaN(pinchAxis.sqrMagnitude) && !float.IsInfinity(pinchAxis.sqrMagnitude) && pinchAxis.sqrMagnitude > .000001f)
                pinchAxis.Normalize();
            else pinchAxis = Vector3.up;
            if (haveDeformation &&
                Mathf.Abs(compression - previousCompression) < ChangeThreshold &&
                Mathf.Abs(stretch - previousStretch) < ChangeThreshold &&
                Mathf.Abs(tiltX - previousTiltX) < ChangeThreshold &&
                Mathf.Abs(tiltZ - previousTiltZ) < ChangeThreshold &&
                Mathf.Abs(contactX - previousContactX) < ChangeThreshold &&
                Mathf.Abs(contactZ - previousContactZ) < ChangeThreshold &&
                Mathf.Abs(pinch - previousPinch) < ChangeThreshold &&
                Mathf.Abs(sag - previousSag) < ChangeThreshold &&
                (sag == 0f || Mathf.Abs(clearance - previousClearance) < ChangeThreshold &&
                    (grip - previousGrip).sqrMagnitude < ChangeThreshold * ChangeThreshold) &&
                (pinchAxis - previousPinchAxis).sqrMagnitude < ChangeThreshold * ChangeThreshold) return;

            previousCompression = compression;
            previousStretch = stretch;
            previousTiltX = tiltX;
            previousTiltZ = tiltZ;
            previousContactX = contactX;
            previousContactZ = contactZ;
            previousPinch = pinch;
            previousPinchAxis = pinchAxis;
            previousSag = sag; previousGrip = grip; previousClearance = clearance;
            haveDeformation = true;
            var directional = new DirectionalStretch(pinch, new Point3(pinchAxis.x, pinchAxis.y, pinchAxis.z));
            var suspension = new SuspendedShape(sag, new Point3(grip.x, grip.y, grip.z), clearance);

            DeformedTop = -1f;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = Deform(restVertices[i], compression, stretch, tiltX, tiltZ, contactX, contactZ, directional, suspension);
                DeformedTop = Mathf.Max(DeformedTop, vertices[i].y);
            }

            RebuildNormals();
            bodyMesh.vertices = vertices;
            bodyMesh.normals = normals;
            // Deliberately fixed conservative bounds avoid RecalculateBounds every touch.
            bodyMesh.bounds = new Bounds(new Vector3(0f, 0.5f, 0f), new Vector3(6f, 6f, 6f));

            float vertical = 1f - compression + stretch;
            float spread = 1f / Mathf.Sqrt(vertical);
            var featureScale = new Vector3(spread, vertical, spread);
            Point3 pinchScale = directional.FeatureScale;
            featureScale = Vector3.Scale(featureScale, new Vector3(pinchScale.X, pinchScale.Y, pinchScale.Z));
            var featureRotation = Quaternion.Euler(tiltZ * 8f, 0f, -tiltX * 8f);
            for (int i = 0; i < accents.Count; i++)
            {
                var accent = accents[i];
                accent.Transform.localPosition = Deform(accent.Rest, compression, stretch, tiltX, tiltZ, contactX, contactZ, directional, suspension);
                accent.Transform.localScale = Vector3.Scale(accent.Scale, featureScale);
                if (accent.IsSurface) accent.Transform.localRotation = featureRotation;
            }

            for (int i = 0; i < mouthRest.Length; i++)
                mouthPositions[i] = Deform(mouthRest[i], compression, stretch, tiltX, tiltZ, contactX, contactZ, directional, suspension);
            mouth.SetPositions(mouthPositions);
        }

        private static float SafeClamp(float value, float minimum, float maximum)
        {
            return float.IsNaN(value) || float.IsInfinity(value) ? 0f : Mathf.Clamp(value, minimum, maximum);
        }

        private static Vector3 Deform(Vector3 rest, float compression, float stretch,
            float tiltX, float tiltZ, float contactX, float contactZ, DirectionalStretch directional, SuspendedShape suspension)
        {
            Point3 deformed = JellyShape.Deform(new Point3(rest.x, rest.y, rest.z),
                compression, stretch, tiltX, tiltZ, contactX, contactZ);
            deformed = directional.Apply(deformed);
            deformed = suspension.Apply(deformed);
            return new Vector3((float)deformed.X, (float)deformed.Y, (float)deformed.Z);
        }

        private void BuildBody()
        {
            int row = LongitudeSegments + 1;
            int count = (LatitudeSegments + 1) * row;
            restVertices = new Vector3[count];
            vertices = new Vector3[count];
            normals = new Vector3[count];
            var uv = new Vector2[count];
            triangles = new int[LatitudeSegments * LongitudeSegments * 6];
            for (int latitude = 0; latitude <= LatitudeSegments; latitude++)
            {
                float v = latitude / (float)LatitudeSegments;
                for (int longitude = 0; longitude <= LongitudeSegments; longitude++)
                {
                    float u = longitude / (float)LongitudeSegments;
                    int index = latitude * row + longitude;
                    Point3 rest = JellyShape.Rest(v * Mathf.PI, u * Mathf.PI * 2f);
                    restVertices[index] = new Vector3((float)rest.X, (float)rest.Y, (float)rest.Z);
                    uv[index] = new Vector2(u, v);
                }
            }
            int triangle = 0;
            for (int latitude = 0; latitude < LatitudeSegments; latitude++)
            {
                for (int longitude = 0; longitude < LongitudeSegments; longitude++)
                {
                    int a = latitude * row + longitude;
                    int b = a + row;
                    triangles[triangle++] = a;
                    triangles[triangle++] = a + 1;
                    triangles[triangle++] = b;
                    triangles[triangle++] = a + 1;
                    triangles[triangle++] = b + 1;
                    triangles[triangle++] = b;
                }
            }
            // Rest is supplied by the portable simulation. Correct winding if its
            // latitude convention changes, without coupling art to that convention.
            int sample = (LatitudeSegments / 2) * row + LongitudeSegments / 4;
            Vector3 surfaceNormal = Vector3.Cross(restVertices[sample + 1] - restVertices[sample],
                restVertices[sample + row] - restVertices[sample]);
            if (Vector3.Dot(surfaceNormal, restVertices[sample]) < 0f)
            {
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    int swap = triangles[i + 1];
                    triangles[i + 1] = triangles[i + 2];
                    triangles[i + 2] = swap;
                }
            }

            bodyMesh = new Mesh { name = "Pip jelly • 28×36" };
            bodyMesh.MarkDynamic();
            bodyMesh.vertices = restVertices;
            bodyMesh.triangles = triangles;
            bodyMesh.uv = uv;
            ownedMeshes.Add(bodyMesh);
            AttachBodyRenderer();
        }

        private void BuildAuthoredBody()
        {
            bodyMesh = Instantiate(characterAsset.BodyMesh);
            bodyMesh.name = "Pip authored deformation instance";
            bodyMesh.MarkDynamic();
            ownedMeshes.Add(bodyMesh);
            restVertices = bodyMesh.vertices;
            vertices = new Vector3[restVertices.Length];
            normals = new Vector3[restVertices.Length];
            triangles = bodyMesh.triangles;
            int groupCount = 0;
            foreach (int group in characterAsset.NormalGroups) groupCount = Mathf.Max(groupCount, group + 1);
            groupNormals = new Vector3[groupCount];
            AttachBodyRenderer();
        }

        private void AttachBodyRenderer()
        {
            var body = new GameObject("Jelly body", typeof(MeshFilter), typeof(MeshRenderer));
            body.transform.SetParent(visualRoot, false);
            body.GetComponent<MeshFilter>().sharedMesh = bodyMesh;
            BodyRenderer = body.GetComponent<MeshRenderer>();
            BodyRenderer.sharedMaterial = bodyMaterial;
            BodyRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            BodyRenderer.receiveShadows = false;
        }

        private void RebuildNormals()
        {
            System.Array.Clear(normals, 0, normals.Length);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int a = triangles[i];
                int b = triangles[i + 1];
                int c = triangles[i + 2];
                Vector3 normal = Vector3.Cross(vertices[b] - vertices[a], vertices[c] - vertices[a]);
                normals[a] += normal;
                normals[b] += normal;
                normals[c] += normal;
            }
            if (characterAsset != null)
            {
                System.Array.Clear(groupNormals, 0, groupNormals.Length);
                for (int i = 0; i < normals.Length; i++) groupNormals[characterAsset.NormalGroups[i]] += normals[i];
                for (int i = 0; i < normals.Length; i++)
                {
                    Vector3 normal = groupNormals[characterAsset.NormalGroups[i]];
                    normals[i] = normal.sqrMagnitude > 0.000001f ? normal.normalized : Vector3.up;
                }
                return;
            }
            int row = LongitudeSegments + 1;
            for (int latitude = 0; latitude <= LatitudeSegments; latitude++)
            {
                int first = latitude * row;
                int last = first + LongitudeSegments;
                Vector3 seam = normals[first] + normals[last];
                normals[first] = seam;
                normals[last] = seam;
            }
            // Shared normals at each pole prevent a pinwheel highlight at the tips.
            SmoothPole(0, row);
            SmoothPole(LatitudeSegments * row, row);
            for (int i = 0; i < normals.Length; i++)
                normals[i] = normals[i].sqrMagnitude > 0.000001f ? normals[i].normalized : Vector3.up;
        }

        private void SmoothPole(int start, int count)
        {
            Vector3 combined = Vector3.zero;
            for (int i = 0; i < count; i++) combined += normals[start + i];
            for (int i = 0; i < count; i++) normals[start + i] = combined;
        }

        private void BuildFace()
        {
            // Face is deliberately on negative Z, facing the default portrait camera.
            for (int side = -1; side <= 1; side += 2)
            {
                int index = side < 0 ? 0 : 1;
                Vector3 eye = characterAsset != null ? characterAsset.Eyes[index] : SurfacePoint(side * 0.225f, 0.18f, 0.034f);
                AddAccent(side < 0 ? "Left soft eye" : "Right soft eye", eye,
                    characterAsset != null ? new Vector3(0.092f, 0.127f, 0.042f) : new Vector3(0.068f, 0.103f, 0.041f), eyeMaterial, accentMesh, true);
                AddAccent("Cream eye glint", characterAsset != null ? characterAsset.EyeGlints[index] : eye + new Vector3(-0.016f, 0.034f, -0.037f),
                    characterAsset != null ? new Vector3(0.020f, 0.024f, 0.012f) : new Vector3(0.018f, 0.021f, 0.011f), highlightMaterial, accentMesh, true);
                AddAccent("Warm cheek", characterAsset != null ? characterAsset.Cheeks[index] : SurfacePoint(side * 0.355f, 0.022f, 0.022f),
                    characterAsset != null ? new Vector3(0.15f, 0.087f, 0.018f) : new Vector3(0.075f, 0.038f, 0.024f), cheekMaterial, accentMesh, true);
            }
            var smile = new GameObject("Tiny smile", typeof(LineRenderer));
            smile.transform.SetParent(visualRoot, false);
            mouth = smile.GetComponent<LineRenderer>();
            mouth.sharedMaterial = smileMaterial;
            mouth.useWorldSpace = false;
            mouth.widthMultiplier = 0.017f;
            mouth.numCapVertices = 4;
            mouth.numCornerVertices = 3;
            mouth.positionCount = mouthRest.Length;
            mouth.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mouth.receiveShadows = false;
            for (int i = 0; i < mouthRest.Length; i++)
            {
                float x = (i / (float)(mouthRest.Length - 1) - 0.5f) * 0.16f;
                float y = -0.04f + 0.055f * Mathf.Pow(x / 0.08f, 2f);
                mouthRest[i] = characterAsset != null ? characterAsset.Mouth[i] : SurfacePoint(x, y, 0.052f);
            }
        }

        private static Vector3 SurfacePoint(float x, float y, float outward)
        {
            // Match the unit seed proportions, keeping facial marks above the shell.
            float latitude = Mathf.Acos(Mathf.Clamp(y, -1f, 1f));
            float bestDistance = float.MaxValue;
            Vector3 best = new Vector3(x, y, -0.7f);
            for (int i = 0; i <= 72; i++)
            {
                Point3 point = JellyShape.Rest(latitude, i * Mathf.PI * 2f / 72f);
                if (point.Z > 0) continue;
                float distance = Mathf.Abs((float)point.X - x);
                if (distance >= bestDistance) continue;
                bestDistance = distance;
                best = new Vector3(x, (float)point.Y, (float)point.Z);
            }
            best.z -= outward;
            return best;
        }

        private void BuildCrown()
        {
            if (characterAsset != null && characterAsset.IntegratedCrown) return;
            if (characterAsset != null)
            {
                for (int i = 0; i < characterAsset.Crowns.Length; i++)
                    AddAccent("Authored crown lobe " + i, characterAsset.Crowns[i], characterAsset.CrownScales[i], bodyMaterial, accentMesh, true);
                return;
            }
            // Two small rounded seed lobes give the placeholder an original silhouette.
            AddAccent("Left crown lobe", new Vector3(-0.14f, 0.91f, 0.015f),
                new Vector3(0.20f, 0.235f, 0.17f), bodyMaterial, accentMesh, true);
            AddAccent("Right crown lobe", new Vector3(0.15f, 0.915f, 0.015f),
                new Vector3(0.18f, 0.20f, 0.155f), bodyMaterial, accentMesh, true);
        }

        private void BuildSuspendedAccents()
        {
            // Glitter is shaded as dense microflakes; opaque soft gel hides filling.
            // Neither needs extra interior objects or per-flake draw calls.
            if (variant == PipVariant.GoldGlitter || variant == PipVariant.MintSoft) return;
            if (variant == PipVariant.MoonJelly)
            {
                Vector3[] moonPearls = { new Vector3(-.38f, -.43f, -.24f), new Vector3(.14f, -.59f, -.26f), new Vector3(.40f, -.29f, -.13f) };
                for (int i = 0; i < moonPearls.Length; i++)
                    AddAccent("Moon pearl " + (i + 1), moonPearls[i], Vector3.one * (.044f + i * .013f), bubbleMaterial, accentMesh, false);
                Vector3[] stars = {
                    new Vector3(-.36f, -.65f, -.26f), new Vector3(.28f, -.45f, -.30f),
                    new Vector3(.49f, -.12f, -.21f), new Vector3(-.24f, .29f, -.22f),
                    new Vector3(.32f, .24f, -.26f), new Vector3(-.46f, -.06f, -.22f),
                    new Vector3(.05f, -.76f, -.24f), new Vector3(.08f, .39f, -.10f) };
                for (int i = 0; i < stars.Length; i++)
                {
                    Transform star = AddAccent("Suspended silver star " + (i + 1), stars[i],
                        Vector3.one * (.046f + (i % 3) * .009f), silverMaterial, starMesh, false);
                    star.localRotation = Quaternion.Euler(10f * i, 17f * i, 31f * i);
                }
                return;
            }
            Vector3[] pearls =
            {
                new Vector3(-0.37f, 0.48f, -0.21f), new Vector3(0.43f, 0.35f, -0.12f),
                new Vector3(-0.45f, -0.28f, -0.17f), new Vector3(0.36f, -0.40f, -0.12f),
                new Vector3(0.10f, 0.65f, -0.11f), new Vector3(-0.13f, -0.58f, -0.22f)
            };
            if (characterAsset != null) pearls = characterAsset.Pearls;
            for (int i = 0; i < pearls.Length; i++)
            {
                float radius = (characterAsset != null ? 0.032f : 0.026f) + (i % 3) * 0.011f;
                AddAccent("Suspended pearl " + (i + 1), pearls[i], Vector3.one * radius,
                    bubbleMaterial, accentMesh, false);
            }
            Vector3[] flecks =
            {
                new Vector3(-0.22f, 0.54f, -0.38f), new Vector3(0.34f, 0.53f, -0.29f),
                new Vector3(-0.54f, 0.03f, -0.27f), new Vector3(0.48f, -0.15f, -0.32f),
                new Vector3(-0.25f, -0.47f, -0.34f), new Vector3(0.19f, -0.62f, -0.21f)
            };
            if (characterAsset != null) flecks = characterAsset.Flecks;
            for (int i = 0; i < flecks.Length; i++)
            {
                var fleck = AddAccent("Apricot fleck " + (i + 1), flecks[i],
                    Vector3.one * (0.023f + (i % 2) * 0.008f), sparkleMaterial, sparkleMesh, false);
                fleck.localRotation = Quaternion.Euler(12f * i, 27f * i, 19f * i);
            }
        }

        private Transform AddAccent(string objectName, Vector3 rest, Vector3 scale,
            Material material, Mesh mesh, bool isSurface)
        {
            var accent = new GameObject(objectName, typeof(MeshFilter), typeof(MeshRenderer));
            accent.transform.SetParent(visualRoot, false);
            accent.transform.localPosition = rest;
            accent.transform.localScale = scale;
            accent.GetComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = accent.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            accents.Add(new SurfaceAccent { Transform = accent.transform, Rest = rest, Scale = scale, IsSurface = isSurface });
            return accent.transform;
        }

        private Material CreateBodyMaterial()
        {
            if (characterAsset != null)
            {
                Color shell = characterAsset.BodyColor;
                shell.a = .60f;
                Material authored = CreateCandyMaterial("Pip clear peach jelly shell", shell, .92f);
                if (authored.HasProperty("_BottomColor")) authored.SetColor("_BottomColor", characterAsset.BottomColor);
                if (authored.HasProperty("_TopColor")) authored.SetColor("_TopColor", characterAsset.TopColor);
                return authored;
            }
            var material = CreateCandyMaterial("Pip coral jelly", new Color(1f, 0.43f, 0.34f, 0.91f), 0.48f);
            if (material.HasProperty("_BottomColor")) material.SetColor("_BottomColor", new Color(0.89f, 0.20f, 0.23f, 1f));
            if (material.HasProperty("_TopColor")) material.SetColor("_TopColor", new Color(1f, 0.71f, 0.49f, 1f));
            return material;
        }

        private void ApplyVariantMaterial()
        {
            // Restore every finish-specific property before applying another preset.
            bool soft = variant == PipVariant.MintSoft;
            SetFloat(bodyMaterial, "_Softness", soft ? 1f : 0f);
            SetFloat(bodyMaterial, "_GlitterStrength", variant == PipVariant.GoldGlitter ? .9f : 0f);
            SetFloat(bodyMaterial, "_GlitterDensity", 96f);
            SetColor(bodyMaterial, "_GlitterColor", new Color(1f, .87f, .44f));
            SetFloat(bodyMaterial, "_StudioStrength", !soft && studioReflection != null ? 1f : 0f);
            SetFloat(bodyMaterial, "_ZWrite", soft ? 1f : 0f);
            SetFloat(bodyMaterial, "_PearlSheen", 0f);
            bodyMaterial.renderQueue = soft ? 2001 : 3000;
            bool moon = variant == PipVariant.MoonJelly;
            if (variant == PipVariant.GoldGlitter)
            {
                bodyMaterial.color = new Color(.96f, .72f, .19f, .58f);
                SetColor(bodyMaterial, "_TopColor", new Color(1f, .94f, .58f));
                SetColor(bodyMaterial, "_BottomColor", new Color(.82f, .46f, .08f));
                SetColor(bodyMaterial, "_RimColor", new Color(1f, .93f, .65f));
                SetFloat(bodyMaterial, "_Glossiness", .88f);
                SetFloat(bodyMaterial, "_ReflectionStrength", .85f);
                cheekMaterial.color = new Color(1f, .35f, .30f, .55f);
            }
            else if (soft)
            {
                bodyMaterial.color = new Color(.52f, .82f, .64f, 1f);
                SetColor(bodyMaterial, "_TopColor", new Color(.75f, .93f, .77f));
                SetColor(bodyMaterial, "_BottomColor", new Color(.35f, .64f, .48f));
                SetColor(bodyMaterial, "_RimColor", new Color(.67f, .89f, .73f));
                SetFloat(bodyMaterial, "_Glossiness", .15f);
                SetFloat(bodyMaterial, "_ReflectionStrength", .16f);
                cheekMaterial.color = new Color(.95f, .43f, .57f, .48f);
            }
            else if (moon)
            {
                bodyMaterial.color = new Color(.46f, .78f, .96f, .54f);
                SetColor(bodyMaterial, "_TopColor", new Color(.77f, .92f, 1f, 1f));
                SetColor(bodyMaterial, "_BottomColor", new Color(.35f, .40f, .82f, 1f));
                SetColor(bodyMaterial, "_RimColor", new Color(.85f, .92f, 1f, 1f));
                SetFloat(bodyMaterial, "_Glossiness", .58f);
                SetFloat(bodyMaterial, "_ReflectionStrength", .62f);
                SetFloat(bodyMaterial, "_PearlSheen", .72f);
                bubbleMaterial.color = new Color(.88f, .97f, 1f, .64f);
                SetColor(bubbleMaterial, "_TopColor", new Color(.97f, .99f, 1f, 1f));
                SetColor(bubbleMaterial, "_BottomColor", new Color(.62f, .81f, .95f, 1f));
                cheekMaterial.color = new Color(.92f, .40f, .67f, .48f);
            }
            else
            {
                bodyMaterial.color = characterAsset != null
                    ? new Color(characterAsset.BodyColor.r, characterAsset.BodyColor.g, characterAsset.BodyColor.b, .60f)
                    : new Color(1f, .43f, .34f, .91f);
                SetColor(bodyMaterial, "_TopColor", characterAsset != null ? characterAsset.TopColor : new Color(1f, .71f, .49f, 1f));
                SetColor(bodyMaterial, "_BottomColor", characterAsset != null ? characterAsset.BottomColor : new Color(.89f, .20f, .23f, 1f));
                SetColor(bodyMaterial, "_RimColor", new Color(1f, .86f, .67f, 1f));
                SetFloat(bodyMaterial, "_Glossiness", characterAsset != null ? .92f : .48f);
                SetFloat(bodyMaterial, "_ReflectionStrength", 1f);
                SetFloat(bodyMaterial, "_PearlSheen", 0f);
                bubbleMaterial.color = new Color(1f, .88f, .69f, .36f);
                SetColor(bubbleMaterial, "_TopColor", Color.Lerp(bubbleMaterial.color, Color.white, .34f));
                SetColor(bubbleMaterial, "_BottomColor", bubbleMaterial.color * new Color(.92f, .74f, .79f, 1f));
                cheekMaterial.color = characterAsset != null ? new Color(1f, .30f, .26f, .62f) : new Color(1f, .42f, .40f, 1f);
            }
            if (visualRoot != null) visualRoot.name = "Pip · " + PipVariants.Label(variant);
            bodyMaterial.name = "Pip · " + PipVariants.Label(variant) + " shell";
        }

        private static void SetColor(Material material, string property, Color color)
        { if (material.HasProperty(property)) material.SetColor(property, color); }
        private static void SetFloat(Material material, string property, float value)
        { if (material.HasProperty(property)) material.SetFloat(property, value); }

        private Material CreateCandyMaterial(string materialName, Color color, float gloss)
        {
            Shader shader = Shader.Find("Pockle/Jelly Candy");
            if (shader == null || !shader.isSupported) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            var material = new Material(shader) { name = materialName };
            material.color = color;
            if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", gloss);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
            if (studioReflection != null && material.HasProperty("_StudioCube"))
            {
                material.SetTexture("_StudioCube", studioReflection);
                material.SetFloat("_StudioStrength", 1f);
            }
            if (material.HasProperty("_BottomColor")) material.SetColor("_BottomColor", color * new Color(0.92f, 0.74f, 0.79f, 1f));
            if (material.HasProperty("_TopColor")) material.SetColor("_TopColor", Color.Lerp(color, Color.white, 0.34f));
            ownedMaterials.Add(material);
            return material;
        }

        private Material CreateFlatMaterial(string materialName, Color color)
        {
            Shader shader = Shader.Find("Pockle/Soft Accent");
            if (shader == null || !shader.isSupported) shader = Shader.Find("Unlit/Color");
            if (shader == null) shader = Shader.Find("Standard");
            var material = new Material(shader) { name = materialName, color = color };
            ownedMaterials.Add(material);
            return material;
        }

        private Mesh BuildSphere(string meshName, int longitudeCount, int latitudeCount)
        {
            int row = longitudeCount + 1;
            var sphereVertices = new Vector3[(latitudeCount + 1) * row];
            var sphereNormals = new Vector3[sphereVertices.Length];
            var indices = new int[latitudeCount * longitudeCount * 6];
            for (int latitude = 0; latitude <= latitudeCount; latitude++)
            {
                float theta = latitude * Mathf.PI / latitudeCount;
                for (int longitude = 0; longitude <= longitudeCount; longitude++)
                {
                    float phi = longitude * Mathf.PI * 2f / longitudeCount;
                    int index = latitude * row + longitude;
                    sphereVertices[index] = new Vector3(Mathf.Sin(theta) * Mathf.Cos(phi), Mathf.Cos(theta), Mathf.Sin(theta) * Mathf.Sin(phi));
                    sphereNormals[index] = sphereVertices[index];
                }
            }
            int triangle = 0;
            for (int latitude = 0; latitude < latitudeCount; latitude++)
            {
                for (int longitude = 0; longitude < longitudeCount; longitude++)
                {
                    int a = latitude * row + longitude;
                    int b = a + row;
                    indices[triangle++] = a;
                    indices[triangle++] = a + 1;
                    indices[triangle++] = b;
                    indices[triangle++] = a + 1;
                    indices[triangle++] = b + 1;
                    indices[triangle++] = b;
                }
            }
            var mesh = new Mesh { name = meshName, vertices = sphereVertices, normals = sphereNormals, triangles = indices };
            mesh.RecalculateBounds();
            ownedMeshes.Add(mesh);
            return mesh;
        }

        private Mesh BuildSparkle()
        {
            var mesh = new Mesh { name = "Original six-point apricot fleck" };
            mesh.vertices = new[] { Vector3.up, Vector3.down, Vector3.left * 0.64f,
                Vector3.right * 0.64f, Vector3.forward * 0.50f, Vector3.back * 0.50f };
            mesh.triangles = new[] { 0, 4, 3, 0, 3, 5, 0, 5, 2, 0, 2, 4,
                1, 3, 4, 1, 5, 3, 1, 2, 5, 1, 4, 2 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            ownedMeshes.Add(mesh);
            return mesh;
        }

        private Mesh BuildStar()
        {
            var points = new Vector3[22];
            var faces = new int[120];
            points[20] = new Vector3(0f, 0f, -.18f);
            points[21] = new Vector3(0f, 0f, .18f);
            for (int i = 0; i < 10; i++)
            {
                float angle = i * Mathf.PI / 5f;
                float radius = i % 2 == 0 ? 1f : .46f;
                points[i] = new Vector3(Mathf.Sin(angle) * radius, Mathf.Cos(angle) * radius, -.18f);
                points[i + 10] = new Vector3(points[i].x, points[i].y, .18f);
                int next = (i + 1) % 10;
                int t = i * 12;
                faces[t] = 20; faces[t + 1] = i; faces[t + 2] = next;
                faces[t + 3] = 21; faces[t + 4] = next + 10; faces[t + 5] = i + 10;
                faces[t + 6] = i; faces[t + 7] = i + 10; faces[t + 8] = next + 10;
                faces[t + 9] = i; faces[t + 10] = next + 10; faces[t + 11] = next;
            }
            var mesh = new Mesh { name = "Five-point suspended star", vertices = points, triangles = faces };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            ownedMeshes.Add(mesh);
            return mesh;
        }

        private void OnDestroy()
        {
            if (visualRoot != null) Destroy(visualRoot.gameObject);
            for (int i = 0; i < ownedMeshes.Count; i++)
                if (ownedMeshes[i] != null) Destroy(ownedMeshes[i]);
            for (int i = 0; i < ownedMaterials.Count; i++)
                if (ownedMaterials[i] != null) Destroy(ownedMaterials[i]);
            if (studioReflection != null) Destroy(studioReflection);
            ownedMeshes.Clear();
            ownedMaterials.Clear();
        }
    }
}
