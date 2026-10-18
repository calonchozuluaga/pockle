using System.Collections;
using System.Reflection;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
    public sealed class MaterialGestureTests
    {
        private static void Call(object o,string name,params object[] args)=>o.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(o,args);
        [UnityTest]
        public IEnumerator SoftPairsDeformWithoutZoomAndRigidPairsZoomWithoutDeforming()
        {
            PrototypeStage.Create(out var camera,out var mount,out var pivot,out _,out _);
            var root=camera.transform.parent.gameObject;var body=new GameObject("Material gesture toy");body.transform.SetParent(mount,false);
            var toy=body.AddComponent<JellyToy>();toy.Initialize();
            var input=root.AddComponent<ToyViewGesture>();input.Initialize(camera,pivot,toy,_=>{},()=>{});
            int changes=0,ends=0;SoftToyPose pose=default;Vector3 axis=Vector3.zero;
            input.SoftManipulated=(p,a)=>{pose=p;axis=a;changes++;};input.SoftManipulationEnded=_=>ends++;
            try
            {
                yield return null;
                foreach(string id in new[]{ToyCatalog.PeachId,ToyCatalog.MintId,"pip.boucle-plush","pip.matte-vinyl","pip.gloss-vinyl","pip.coated-metallic",ToyCatalog.PipSpecialId})
                {
                    input.Cancel();input.ResetView();Assert.That(toy.TrySetCollectible(id),Is.True,id);changes=0;
                    Call(input,"BeginPair",new Vector2(100,200),new Vector2(300,200),2);
                    Call(input,"MovePair",new Vector2(150,170),new Vector2(250,170));
                    if(toy.Handling==MaterialHandling.Firm)
                    {Assert.That(changes,Is.Zero);Assert.That(input.Orbit.Zoom,Is.GreaterThan(1));Assert.That(input.Orbit.PanY,Is.LessThan(0));}
                    else
                    {
                        Assert.That(changes,Is.EqualTo(1));Assert.That(input.Orbit.Zoom,Is.EqualTo(1));Assert.That(input.Orbit.PanY,Is.Zero);
                        Assert.That(pose.Pinch,Is.LessThan(0));Assert.That(pose.Compression,Is.GreaterThan(.18f));Assert.That(axis.sqrMagnitude,Is.EqualTo(1).Within(.001f));
                        Call(input,"MovePair",new Vector2(50,240),new Vector2(350,240));
                        Assert.That(pose.Pinch,Is.GreaterThan(0));Assert.That(pose.LiftDelta,Is.GreaterThan(0));
                        Assert.That(input.IsHolding,Is.True);int before=ends;input.Cancel();Assert.That(ends,Is.EqualTo(before+1));Assert.That(input.IsHolding,Is.False);
                    }
                }
                toy.TrySetCollectible(ToyCatalog.PipSpecialId);
                PrototypeStage.SetWorld(camera,StageWorld.Midnight,toy.CollectibleId);
                toy.transform.localPosition=mount.InverseTransformVector(Vector3.up*.4f);
                yield return null;yield return null;
                var halo=mount.Find("Toy emitted halo");
                Assert.That(Vector3.Distance(halo.position,toy.VisualRoot.position+camera.transform.forward*.85f),Is.LessThan(.001f),"Halo follows a lifted toy without rotating its offset.");
            }
            finally{Object.Destroy(root);}
            yield return null;
        }
    }
}
