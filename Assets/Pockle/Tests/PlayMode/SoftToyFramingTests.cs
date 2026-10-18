using System.Collections;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
    public sealed class SoftToyFramingTests
    {
        [UnityTest]
        public IEnumerator SoftFramingFollowsMaterialAcrossResetAndPanelSizes()
        {
            PrototypeStage.Create(out var camera,out var mount,out var pivot,out _,out _);
            var root=camera.transform.parent.gameObject;
            var obj=new GameObject("Soft framing test");obj.transform.SetParent(mount,false);
            var toy=obj.AddComponent<JellyToy>();toy.Initialize();var input=root.AddComponent<ToyViewGesture>();input.Initialize(camera,pivot,toy,_=>{},()=>{});
            try
            {
                foreach(float panelHeight in new[]{.7f,.4f})
                {
                    PrototypeStage.SetViewport(camera,new Rect(Screen.width*.15f,Screen.height*.15f,Screen.width*.7f,Screen.height*panelHeight));
                    foreach(string id in new[]{PipFusionStudies.DuskId,PipFusionStudies.SorbetId,"pip.gloss-vinyl",ToyCatalog.PeachId})
                    {
                        toy.TrySetCollectible(id);input.ResetView();yield return new WaitForSeconds(.5f);
                        float aspect=Screen.width*.7f/(Screen.height*panelHeight);
                        bool soft=toy.Handling!=MaterialHandling.Firm;
                        Assert.That(camera.orthographicSize,Is.EqualTo(ToyOrbit.FrameSize(aspect,1,soft)/panelHeight).Within(.005));
                        Assert.That(input.Orbit.Zoom,Is.EqualTo(1));
                    }
                }
            }
            finally{Object.Destroy(root);}
            yield return null;
        }
    }
}
