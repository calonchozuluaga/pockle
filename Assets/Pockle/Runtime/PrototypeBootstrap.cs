using UnityEngine;

namespace Pockle.Runtime
{
    /// <summary>Scene entry point. The prototype needs no imported art or editor-generated objects.</summary>
    public sealed class PrototypeBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            if (FindFirstObjectByType<TactilePrototype>() == null)
                gameObject.AddComponent<TactilePrototype>();
        }
    }
}
