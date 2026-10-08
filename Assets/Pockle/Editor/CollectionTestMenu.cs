using UnityEditor;
using UnityEngine;

namespace Pockle.Editor
{
    public static class CollectionTestMenu
    {
        [MenuItem("Pockle/Testing/Complete today's walk (Play Mode only)")]
        public static void CompleteWalk()
        {
            var session = Object.FindFirstObjectByType<Runtime.CollectionSession>();
            if (!Application.isPlaying || session == null)
            { Debug.Log("Enter Play Mode before simulating a walk."); return; }
            session.SimulateDailyWalk();
        }
    }
}
