using Pockle.Core;
using UnityEngine;

namespace Pockle.Runtime
{
    public sealed partial class JellyToy
    {
        private readonly Transform[] faceEyes = new Transform[2], faceGlints = new Transform[2];
        private readonly SurfaceAccent[] eyeAccents = new SurfaceAccent[2], glintAccents = new SurfaceAccent[2];
        private readonly LineRenderer[] closedEyes = new LineRenderer[2];
        private readonly Vector3[][] eyeDown = { new Vector3[9], new Vector3[9] };
        private readonly Vector3[][] eyeUp = { new Vector3[9], new Vector3[9] };
        private readonly Vector3[] eyePositions = new Vector3[9];
        private readonly Vector3[][] expressionMouth = { new Vector3[13], new Vector3[13], new Vector3[13], new Vector3[13] };
        private readonly Vector3[] interiorPositions = new Vector3[14];
        private Mesh mouthInteriorMesh;
        private MeshRenderer mouthInterior;
        private Transform tongue;
        private Vector3 tongueRest, tongueScale;
        private FacePose facePose;
        private float faceCompression, faceStretch, faceTiltX, faceTiltZ, faceContactX, faceContactZ;
        private DirectionalStretch faceDirectional;
        private SuspendedShape faceSuspension;
        public FacePose ExpressionPose { get { return facePose; } }

        /// <summary>Viewer-owned clock. Portraits retain the static Happy rest pose.</summary>
        public void SetFacePose(FacePose pose)
        {
            Initialize();
            if (pose.Curious == facePose.Curious && pose.Sleepy == facePose.Sleepy &&
                pose.Delighted == facePose.Delighted && pose.Blink == facePose.Blink) return;
            facePose = pose;
            ApplyFacePose(faceCompression, faceStretch, faceTiltX, faceTiltZ, faceContactX, faceContactZ,
                faceDirectional, faceSuspension);
        }

        private LineRenderer CreateFaceLine(string name, int count)
        {
            var root = new GameObject(name, typeof(LineRenderer));
            root.transform.SetParent(visualRoot, false);
            var line = root.GetComponent<LineRenderer>();
            line.sharedMaterial = smileMaterial;
            line.useWorldSpace = false;
            line.positionCount = count;
            line.numCapVertices = 4;
            line.numCornerVertices = 3;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            return line;
        }

        private void BuildExpressionFeatures()
        {
            for (int side = 0; side < 2; side++)
            {
                // BuildFace adds eye, glint, cheek, in that order. Keep their authored anchors immutable.
                eyeAccents[side] = accents[side * 3];
                glintAccents[side] = accents[side * 3 + 1];
                eyeAccents[side].IsFaceFeature = glintAccents[side].IsFaceFeature = true;
                closedEyes[side] = CreateFaceLine(side == 0 ? "Left closed eyelid" : "Right closed eyelid", 9);
                Vector3 eye = eyeAccents[side].Rest;
                for (int i = 0; i < 9; i++)
                {
                    float x = i / 4f - 1;
                    float arc = (1 - x * x) * eyeAccents[side].Scale.y * .38f;
                    float px = eye.x + x * eyeAccents[side].Scale.x * .87f;
                    eyeDown[side][i] = ProjectFacePoint(px, eye.y - arc, .018f);
                    eyeUp[side][i] = ProjectFacePoint(px, eye.y + arc, .018f);
                }
            }
            float minX = mouthRest[0].x, maxX = mouthRest[12].x;
            float baseY = mouthRest[6].y;
            float height = Mathf.Max(.025f, (mouthRest[0].y + mouthRest[12].y) * .5f - baseY);
            float centerX = (minX + maxX) * .5f;
            float halfWidth = (maxX - minX) * .5f;
            for (int expression = 0; expression < 4; expression++)
                for (int i = 0; i < 13; i++)
                {
                    Point3 point = FacePose.For((FaceExpression)expression).MouthPoint(i / 12f);
                    expressionMouth[expression][i] = expression == 0 ? mouthRest[i] :
                        ProjectFacePoint(centerX + halfWidth * point.X, baseY + height * point.Y, .028f);
                }
            var interior = new GameObject("Open smile interior", typeof(MeshFilter), typeof(MeshRenderer));
            interior.transform.SetParent(visualRoot, false);
            mouthInterior = interior.GetComponent<MeshRenderer>();
            mouthInterior.sharedMaterial = smileMaterial;
            mouthInterior.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mouthInterior.receiveShadows = false;
            int[] indices = new int[36];
            Vector3[] faceNormals = new Vector3[14];
            for (int i = 0; i < 14; i++) faceNormals[i] = Vector3.back;
            for (int i = 0; i < 12; i++) { indices[i * 3] = 0; indices[i * 3 + 1] = i + 2; indices[i * 3 + 2] = i + 1; }
            mouthInteriorMesh = new Mesh { name = "Reusable open smile", vertices = interiorPositions, triangles = indices, normals = faceNormals };
            mouthInteriorMesh.MarkDynamic();
            ownedMeshes.Add(mouthInteriorMesh);
            interior.GetComponent<MeshFilter>().sharedMesh = mouthInteriorMesh;
            var tongueObject = new GameObject("Little pink tongue", typeof(MeshFilter), typeof(MeshRenderer));
            tongue = tongueObject.transform;
            tongue.SetParent(visualRoot, false);
            tongueObject.GetComponent<MeshFilter>().sharedMesh = accentMesh;
            var tongueRenderer = tongueObject.GetComponent<MeshRenderer>();
            tongueRenderer.sharedMaterial = CreateFlatMaterial("Pip tongue", new Color(.95f, .36f, .48f));
            tongueRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            tongueRenderer.receiveShadows = false;
            tongueRest = ProjectFacePoint(centerX, baseY - height * .22f, .037f);
            tongueScale = new Vector3(halfWidth * .47f, height * .23f, .006f);
        }

        // One-time construction only: project onto the actual front of the rest mesh, including opaque finishes.
        private Vector3 ProjectFacePoint(float x, float y, float outward)
        {
            float front = float.PositiveInfinity;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 a = restVertices[triangles[i]], b = restVertices[triangles[i + 1]], c = restVertices[triangles[i + 2]];
                float determinant = (b.y - c.y) * (a.x - c.x) + (c.x - b.x) * (a.y - c.y);
                if (Mathf.Abs(determinant) < .0000001f) continue;
                float u = ((b.y - c.y) * (x - c.x) + (c.x - b.x) * (y - c.y)) / determinant;
                float v = ((c.y - a.y) * (x - c.x) + (a.x - c.x) * (y - c.y)) / determinant;
                if (u < -.00001f || v < -.00001f || u + v > 1.00001f) continue;
                front = Mathf.Min(front, a.z * u + b.z * v + c.z * (1 - u - v));
            }
            return float.IsInfinity(front) ? SurfacePoint(x, y, outward) : new Vector3(x, y, front - outward);
        }

        private void ApplyFacePose(float compression, float stretch, float tiltX, float tiltZ,
            float contactX, float contactZ, DirectionalStretch directional, SuspendedShape suspension)
        {
            faceCompression = compression; faceStretch = stretch; faceTiltX = tiltX; faceTiltZ = tiltZ;
            faceContactX = contactX; faceContactZ = contactZ; faceDirectional = directional; faceSuspension = suspension;
            float vertical = 1 - compression + stretch;
            float spread = 1 / Mathf.Sqrt(vertical);
            Point3 ds = directional.FeatureScale;
            var scale = Vector3.Scale(new Vector3(spread, vertical, spread), new Vector3(ds.X, ds.Y, ds.Z));
            var rotation = Quaternion.Euler(tiltZ * 8, 0, -tiltX * 8);
            float open = facePose.EyeOpen;
            for (int side = 0; side < 2; side++)
            {
                SurfaceAccent eye = eyeAccents[side], glint = glintAccents[side];
                faceEyes[side].localPosition = Deform(eye.Rest, compression, stretch, tiltX, tiltZ, contactX, contactZ, directional, suspension);
                faceEyes[side].localRotation = rotation;
                faceEyes[side].localScale = Vector3.Scale(eye.Scale, Vector3.Scale(scale, new Vector3(1, Mathf.Max(.015f, open), 1)));
                faceEyes[side].gameObject.SetActive(open > .08f);
                Vector3 glintRest = eye.Rest + Vector3.Scale(glint.Rest - eye.Rest, new Vector3(1, open, 1));
                faceGlints[side].localPosition = Deform(glintRest, compression, stretch, tiltX, tiltZ, contactX, contactZ, directional, suspension);
                faceGlints[side].localRotation = rotation;
                faceGlints[side].localScale = Vector3.Scale(glint.Scale, scale) * open;
                faceGlints[side].gameObject.SetActive(open > .35f);
                closedEyes[side].enabled = open < .45f;
                closedEyes[side].widthMultiplier = .017f * (1 - open) * spread;
                for (int i = 0; i < 9; i++)
                {
                    Vector3 rest = Vector3.Lerp(eyeDown[side][i], eyeUp[side][i], (facePose.EyeArc + 1) * .5f);
                    eyePositions[i] = Deform(rest, compression, stretch, tiltX, tiltZ, contactX, contactZ, directional, suspension);
                }
                closedEyes[side].SetPositions(eyePositions);
            }
            Vector3 center = Vector3.zero;
            for (int i = 0; i < 13; i++)
            {
                Vector3 rest = expressionMouth[0][i] * facePose.Happy + expressionMouth[1][i] * facePose.Curious +
                    expressionMouth[2][i] * facePose.Sleepy + expressionMouth[3][i] * facePose.Delighted;
                mouthPositions[i] = Deform(rest, compression, stretch, tiltX, tiltZ, contactX, contactZ, directional, suspension);
                if (i < 12) center += mouthPositions[i] / 12f;
                interiorPositions[i + 1] = mouthPositions[i] + Vector3.forward * .002f;
            }
            mouth.SetPositions(mouthPositions);
            mouth.widthMultiplier = Mathf.Lerp(.017f, .012f, facePose.MouthOpen) * spread;
            mouthInterior.enabled = facePose.MouthOpen > .97f;
            if (mouthInterior.enabled)
            {
                interiorPositions[0] = center + Vector3.forward * .002f;
                mouthInteriorMesh.vertices = interiorPositions;
                mouthInteriorMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 6);
            }
            tongue.gameObject.SetActive(facePose.Delighted > .97f);
            tongue.localPosition = Deform(tongueRest, compression, stretch, tiltX, tiltZ, contactX, contactZ, directional, suspension);
            tongue.localScale = Vector3.Scale(tongueScale, scale);
            tongue.localRotation = rotation;
        }
    }
}
