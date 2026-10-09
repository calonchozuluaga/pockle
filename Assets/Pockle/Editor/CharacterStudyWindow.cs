using Pockle.Runtime;
using UnityEditor;
using UnityEngine;

namespace Pockle.Editor
{
    /// <summary>Separate authoring controls; does not add prototype controls to the game UI.</summary>
    public sealed class CharacterStudyWindow : EditorWindow
    {
        [MenuItem("Pockle/Character studies")]
        private static void Open() { GetWindow<CharacterStudyWindow>("Character studies"); }
        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Press Play in the Pockle scene, then choose a draft model. Drag to lift, release to drop, or press J for a simulated shake. Review previews do not grant toys or write inventory.", MessageType.Info);
            EditorGUILayout.LabelField("The current HUD still labels the viewer Pip.");
            using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
            {
                if (GUILayout.Button("Preview Moss · Velvet Flock")) Preview(CharacterArt.MossStudyId);
                if (GUILayout.Button("Preview Bop · Gloss Vinyl")) Preview(CharacterArt.BopStudyId);
                if (GUILayout.Button("Preview Nook · Oat Bouclé Plush")) Preview(CharacterArt.NookPlushStudyId);
                if (GUILayout.Button("Preview Nook · Lilac Mochi Foam")) Preview(CharacterArt.NookFoamStudyId);
            }
        }
        private static void Preview(string id)
        {
            TactilePrototype viewer = Object.FindFirstObjectByType<TactilePrototype>();
            if (viewer == null || !viewer.PreviewCharacterStudy(id))
                Debug.LogWarning("Start the Pockle scene with an owned Pip, finish any box reveal, and check character import errors before previewing.");
        }
    }
}
