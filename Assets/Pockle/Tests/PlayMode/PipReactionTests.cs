using System.Collections;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
    public sealed class PipReactionTests
    {
        [UnityTest]
        public IEnumerator FiveActionsKeepRigidShapeAndResetAfterSwitchingOrCancellation()
        {
            var obj=new GameObject("Pip reactions test");var toy=obj.AddComponent<JellyToy>();toy.Initialize();
            var driver=obj.AddComponent<PipReactionDriver>();driver.Initialize(toy);
            try
            {
                Assert.That(toy.TrySetCollectible("pip.matte-vinyl"),Is.True);
                foreach(PipAction action in System.Enum.GetValues(typeof(PipAction)))
                {
                    Assert.That(driver.TryPlay(action),Is.True);
                    driver.ApplyPose(PipReactions.Sample(action,.5f,true,false));
                    Assert.That(toy.VisualRoot.parent.localScale,Is.EqualTo(Vector3.one),"Rigid bodies retain their shape.");
                    driver.Cancel();Assert.That(toy.VisualRoot.parent.localPosition,Is.EqualTo(Vector3.zero));
                    Assert.That(toy.VisualRoot.parent.localRotation,Is.EqualTo(Quaternion.identity));
                }
                Assert.That(toy.TrySetCollectible(ToyCatalog.PeachId),Is.True);
                Assert.That(driver.TryPlay(PipAction.Peekaboo),Is.True);
                driver.ApplyPose(PipReactions.Sample(PipAction.Peekaboo,.5f,false,false));
                Assert.That(toy.BodyRenderer.sharedMaterial.GetFloat("_Blink"),Is.EqualTo(1));
                driver.Cancel();Assert.That(toy.BodyRenderer.sharedMaterial.GetFloat("_Blink"),Is.Zero);
                int previous=-1;
                for(int i=0;i<100;i++)
                {
                    driver.PlayRandom();Assert.That(driver.IsPlaying,Is.True);
                    int action=(int)driver.CurrentAction;Assert.That(action,Is.Not.EqualTo(previous));
                    previous=action;driver.Cancel();
                }
                driver.TryPlay(PipAction.Hop);yield return new WaitForSeconds(2);
                Assert.That(driver.IsPlaying,Is.False);Assert.That(toy.VisualRoot.parent.localPosition,Is.EqualTo(Vector3.zero));
                toy.TrySetCollectible(ToyCatalog.FoxId);Assert.That(driver.TryPlay(PipAction.Hop),Is.False);
            }
            finally{Object.Destroy(obj);}
            yield return null;
        }
    }
}
