using UnityEngine;
namespace Pockle.Runtime
{
    public sealed partial class JellyToy
    {
        private float editionClock;
        private static readonly int EditionClockProperty = Shader.PropertyToID("_EditionClock");
        private static readonly int EditionEffectProperty = Shader.PropertyToID("_EditionEffect");
        private void LateUpdate()
        {
            if (bodyMaterial == null || !bodyMaterial.HasProperty(EditionClockProperty)
                || bodyMaterial.GetFloat(EditionEffectProperty) < 3.5f) return;
            if (PlayerPrefs.GetInt("pockle.prototype.reducedMotion", 0) != 1) editionClock += Time.deltaTime;
            bodyMaterial.SetFloat(EditionClockProperty, editionClock);
        }
    }
}
