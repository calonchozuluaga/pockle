using System.Collections.Generic;
using Pockle.Core;
using UnityEngine;
namespace Pockle.Runtime
{
    /// <summary>Imported hinged carton; samples its authored opening against the existing reveal pose.</summary>
    public sealed class NativeCollectionBox : MonoBehaviour
    {
        public AnimationClip OpeningClip;
        private readonly List<Material> materials=new List<Material>();
        private bool initialized;
        public void Initialize()
        {
            if(initialized)return;initialized=true;
            foreach(var renderer in GetComponentsInChildren<Renderer>())
            {
                var replacements=renderer.sharedMaterials;
                for(int i=0;i<replacements.Length;i++)
                {
                    if(replacements[i]==null)continue;
                    var clone=new Material(replacements[i]);materials.Add(clone);replacements[i]=clone;
                }
                renderer.sharedMaterials=replacements;
            }
            var animation=GetComponent<Animation>();if(animation!=null){animation.Stop();animation.playAutomatically=false;}
        }
        public void ApplyPose(RevealPose pose, bool reducedMotion)
        {
            Initialize();
            if(OpeningClip!=null)OpeningClip.SampleAnimation(gameObject,Mathf.Lerp(11f/24,35f/24,Mathf.Clamp01(pose.Opening)*(reducedMotion?18f/108:1)));
            foreach(var material in materials)
            {
                Color colour=material.color;colour.a=Mathf.Clamp01(pose.BoxAlpha);material.color=colour;
                material.SetFloat("_ZWrite",pose.BoxAlpha>=.999f?1:0);material.renderQueue=pose.BoxAlpha>=.999f?2995:3120;
            }
        }
        private void OnDestroy(){foreach(var material in materials)if(material!=null)Destroy(material);}
    }
}
