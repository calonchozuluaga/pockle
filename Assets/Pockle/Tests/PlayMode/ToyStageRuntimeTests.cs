using System.Collections;
using System.Linq;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;

namespace Pockle.Tests
{
    public sealed class ToyStageRuntimeTests
    {
        [UnityTest]
        public IEnumerator PaintedMapsBindToTheToyAndGlowSpillResetsWhenSwitching()
        {
            Color ambient=RenderSettings.ambientLight;
            PrototypeStage.Create(out var camera,out var mount,out _,out _,out _);
            var root=camera.transform.parent.gameObject;
            var obj=new GameObject("Painted world toy");obj.transform.SetParent(mount,false);
            var toy=obj.AddComponent<JellyToy>();toy.Initialize();
            try
            {
                Assert.That(PaintedWorldAsset.Load(StageWorld.Studio,"studio"),Is.Null);
                foreach(var world in new[]{StageWorld.Home,StageWorld.Garden,StageWorld.Midnight})
                {
                    var asset=PaintedWorldAsset.Load(world,"fox-hollow");
                    Assert.That(asset,Is.Not.Null);Assert.That(asset.Background,Is.Not.Null);
                    Assert.That(asset.Reflections.width,Is.EqualTo(128));
                    Assert.That(toy.TrySetCollectible(ToyCatalog.FoxAuroraId),Is.True);
                    PrototypeStage.SetWorld(camera,world,toy.CollectibleId);PrototypeStage.SetLight(camera,StageLight.Night);
                    Assert.That(toy.BodyRenderer.sharedMaterial.GetTexture("_StageEnvironment"),Is.SameAs(asset.Reflections));
                    Assert.That(camera.GetComponent<ToyGlowAura>().Strength,Is.GreaterThan(.2f));
                    Assert.That(root.GetComponentsInChildren<Collider>().Length,Is.EqualTo(1), "Only the toy may add a collider.");
                    yield return null;
                }
                Assert.That(toy.TrySetCollectible(ToyCatalog.GoldId),Is.True);
                PrototypeStage.SetWorld(camera,StageWorld.Midnight,toy.CollectibleId);PrototypeStage.SetLight(camera,StageLight.Night);
                Assert.That(camera.GetComponent<ToyGlowAura>().Strength,Is.InRange(.01f,.1f));
                Assert.That(toy.TrySetCollectible(ToyCatalog.MintId),Is.True);
                PrototypeStage.SetWorld(camera,StageWorld.Studio,toy.CollectibleId);PrototypeStage.SetLight(camera,StageLight.Day);
                Assert.That(toy.BodyRenderer.sharedMaterial.GetFloat("_StageEnvironmentStrength"),Is.Zero);
                Assert.That(camera.GetComponent<ToyGlowAura>().Strength,Is.Zero);
                Assert.That(root.GetComponentsInChildren<ReflectionProbe>(true).Single().enabled,Is.False);
            }
            finally {Object.Destroy(root);RenderSettings.ambientLight=ambient;}
            yield return null;
        }
        [UnityTest]
        public IEnumerator WorldsAreBoundedCalmWithoutAPlate()
        {
            Color ambient=RenderSettings.ambientLight;
            PrototypeStage.Create(out var camera,out var mount,out var plateRoot,out var plate,out _);
            var root=camera.transform.parent.gameObject;
            var environment=camera.GetComponent<ToyStageEnvironment>();
            try
            {
                Assert.That(PrototypeStage.SetWorld(camera,StageWorld.Home,ToyCatalog.FoxId),Is.True);
                Assert.That(environment.HomeWorld,Is.EqualTo("fox-hollow"));
                Assert.That(root.GetComponentsInChildren<Collider>(),Is.Empty);
                Assert.That(plate,Is.Null);
                var leaf=root.GetComponentsInChildren<Transform>().First(t=>t.name=="Falling leaf");
                environment.Calm=true;var rest=leaf.localPosition;
                yield return new WaitForSeconds(.15f);Assert.That(leaf.localPosition,Is.EqualTo(rest));
                environment.Calm=false;yield return new WaitForSeconds(.15f);Assert.That(leaf.localPosition,Is.Not.EqualTo(rest));
                for(int i=0;i<12;i++)
                {
                    Assert.That(PrototypeStage.SetWorld(camera,(StageWorld)(i%4),ToyCatalog.FoxId),Is.True);
                    yield return null;
                    Assert.That(root.GetComponentsInChildren<MeshRenderer>().Length,Is.LessThan(55));

                }
                Assert.That(PrototypeStage.SetWorld(camera,StageWorld.Home,ToyCatalog.PeachId),Is.True);
                Assert.That(environment.HomeWorld,Is.EqualTo("pip-jelly-garden"));
                Assert.That(root.GetComponentsInChildren<Transform>().Any(t=>t.name=="Falling leaf"&&t.gameObject.activeInHierarchy),Is.False);
                Assert.That(PrototypeStage.SetWorld(camera,(StageWorld)99,ToyCatalog.FoxId),Is.False);
            }
            finally { Object.Destroy(root); RenderSettings.ambientLight=ambient; }
            yield return null;
        }
        [UnityTest]
        public IEnumerator LightingAffectsOnlyTheSelectedToyAndAddsNightGlow()
        {
            Color ambient=RenderSettings.ambientLight;
            PrototypeStage.Create(out var camera,out var mount,out _,out _,out _);
            var root=camera.transform.parent.gameObject;
            var toyObject=new GameObject("Stage glow test");toyObject.transform.SetParent(mount,false);
            var toy=toyObject.AddComponent<JellyToy>();toy.Initialize();
            var portraitObject=new GameObject("Unrelated portrait");var portrait=portraitObject.AddComponent<JellyToy>();portrait.Initialize();
            try
            {
                foreach(string id in new[]{ToyCatalog.FoxAuroraId,ToyCatalog.PipSpecialId})
                {
                    Assert.That(toy.TrySetCollectible(id),Is.True);
                    PrototypeStage.SetWorld(camera,StageWorld.Midnight,id);
                    PrototypeStage.SetLight(camera,StageLight.Night);
                    Assert.That(toy.BodyRenderer.sharedMaterial.GetFloat("_StageEmission"),Is.GreaterThan(0));
                    Assert.That(portrait.BodyRenderer.sharedMaterial.GetColor("_StageTint"),Is.EqualTo(Color.white));
                    PrototypeStage.SetLight(camera,StageLight.Day);
                    Assert.That(toy.BodyRenderer.sharedMaterial.GetFloat("_StageEmission"),Is.EqualTo(0));
                    Assert.That(toy.BodyRenderer.sharedMaterial.GetColor("_StageTint"),Is.EqualTo(Color.white));
                }
                Assert.That(PrototypeStage.SetLight(camera,(StageLight)99),Is.False);
            }
            finally { Object.Destroy(root);Object.Destroy(portraitObject);RenderSettings.ambientLight=ambient; }
            yield return null;
        }
    }
}
