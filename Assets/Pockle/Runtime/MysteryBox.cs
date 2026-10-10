using Pockle.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Pockle.Runtime
{
    /// <summary>An open-top carton with printed outer panels and real lid/flap hinges.</summary>
    public sealed class MysteryBox : MonoBehaviour
    {
        private const float Width = 1.85f, Height = 1.55f, Depth = 1.50f;
        private Mesh boardMesh;
        private Mesh floorMesh;
        private Material printed, lining;
        private Color printedColor = Color.white;
        private readonly Color liningColor = new Color(1f, .94f, .84f);
        private Transform lidHinge, leftFlap, rightFlap;
        private bool initialized;

        public void Initialize()
        {
            if (initialized) return;
            initialized = true;
            Shader shader = Shader.Find("Pockle/Reveal Box");
            if (shader == null || !shader.isSupported) shader = Shader.Find("Pockle/Soft Accent");
            printed = new Material(shader) { name = "Jelly Garden · printed carton", color = Color.white };
            lining = new Material(shader) { name = "Jelly Garden · cream lining", color = liningColor };
            Texture2D artwork = Resources.Load<Texture2D>("Store/JellyGardenPanel");
            if (artwork != null && printed.HasProperty("_MainTex")) printed.mainTexture = artwork;
            else
            {
                printedColor = new Color(1f, .67f, .49f);
                Debug.LogWarning("Jelly Garden panel artwork is unavailable; check texture import and the reveal shader.", this);
            }
            boardMesh = BuildBoard();
            AddBoard("Front panel", transform, new Vector3(0f, Height * .5f, -Depth * .5f), Quaternion.identity, Width, Height, true);
            AddBoard("Back panel", transform, new Vector3(0f, Height * .5f, Depth * .5f), Quaternion.Euler(0f, 180f, 0f), Width, Height, true);
            AddBoard("Left panel", transform, new Vector3(-Width * .5f, Height * .5f, 0f), Quaternion.Euler(0f, 90f, 0f), Depth, Height, true);
            AddBoard("Right panel", transform, new Vector3(Width * .5f, Height * .5f, 0f), Quaternion.Euler(0f, -90f, 0f), Depth, Height, true);
            lidHinge = Hinge("Back lid hinge", new Vector3(0f, Height + .025f, Depth * .5f));
            AddBoard("Printed lid", lidHinge, new Vector3(0f, 0f, -Depth * .5f), Quaternion.Euler(90f, 0f, 0f), Width + .05f, Depth + .04f, true);
            leftFlap = Hinge("Left dust flap hinge", new Vector3(-Width * .5f, Height, 0f));
            rightFlap = Hinge("Right dust flap hinge", new Vector3(Width * .5f, Height, 0f));
            AddBoard("Left inner flap", leftFlap, new Vector3(.21f, 0f, 0f), Quaternion.Euler(90f, 0f, 0f), .42f, Depth - .06f, false);
            AddBoard("Right inner flap", rightFlap, new Vector3(-.21f, 0f, 0f), Quaternion.Euler(90f, 0f, 0f), .42f, Depth - .06f, false);
            var floor = new GameObject("Carton floor", typeof(MeshFilter), typeof(MeshRenderer));
            floorMesh = PresentationMeshes.CreateBox();
            floor.GetComponent<MeshFilter>().sharedMesh = floorMesh;
            floor.transform.SetParent(transform, false);
            floor.transform.localPosition = new Vector3(0f, .02f, 0f);
            floor.transform.localScale = new Vector3(Width, .04f, Depth);
            var renderer = floor.GetComponent<Renderer>();
            renderer.sharedMaterial = lining;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            ApplyPose(RevealSequence.Sample(0f, false), false);
        }

        public void ApplyPose(RevealPose pose, bool reducedMotion)
        {
            Initialize();
            float angle = pose.Opening * (reducedMotion ? 18f : 108f);
            lidHinge.localRotation = Quaternion.Euler(angle, 0f, 0f);
            leftFlap.localRotation = Quaternion.Euler(0f, 0f, angle * .88f);
            rightFlap.localRotation = Quaternion.Euler(0f, 0f, -angle * .88f);
            SetAlpha(printed, printedColor, pose.BoxAlpha);
            SetAlpha(lining, liningColor, pose.BoxAlpha);
        }

        private static void SetAlpha(Material material, Color tint, float alpha)
        {
            tint.a = alpha;
            material.color = tint;
            bool opaque = alpha >= .999f;
            if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", opaque ? 1f : 0f);
            material.renderQueue = opaque ? 2995 : 3120;
        }

        private Transform Hinge(string name, Vector3 position)
        {
            var hinge = new GameObject(name).transform;
            hinge.SetParent(transform, false);
            hinge.localPosition = position;
            return hinge;
        }

        private void AddBoard(string name, Transform parent, Vector3 position, Quaternion rotation,
            float width, float height, bool branded)
        {
            var board = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
            board.transform.SetParent(parent, false);
            board.transform.localPosition = position;
            board.transform.localRotation = rotation;
            board.transform.localScale = new Vector3(width, height, 1f);
            board.GetComponent<MeshFilter>().sharedMesh = boardMesh;
            var renderer = board.GetComponent<MeshRenderer>();
            renderer.sharedMaterials = new[] { branded ? printed : lining, lining };
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        private static Mesh BuildBoard()
        {
            var mesh = new Mesh { name = "Printed carton board · two faces" };
            mesh.vertices = new[] {
                new Vector3(-.5f, -.5f, -.012f), new Vector3(-.5f, .5f, -.012f),
                new Vector3(.5f, .5f, -.012f), new Vector3(.5f, -.5f, -.012f),
                new Vector3(-.5f, -.5f, .012f), new Vector3(-.5f, .5f, .012f),
                new Vector3(.5f, .5f, .012f), new Vector3(.5f, -.5f, .012f) };
            mesh.uv = new[] { new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f),
                new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0f) };
            mesh.subMeshCount = 2;
            mesh.SetTriangles(new[] { 0, 1, 2, 0, 2, 3 }, 0);
            mesh.SetTriangles(new[] { 4, 6, 5, 4, 7, 6 }, 1);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private void OnDestroy()
        {
            if (boardMesh != null) Destroy(boardMesh);
            if (floorMesh != null) Destroy(floorMesh);
            if (printed != null) Destroy(printed);
            if (lining != null) Destroy(lining);
        }
    }
}
