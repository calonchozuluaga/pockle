using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
    public sealed class PipFusionMaterialTests
    {
        [Test]
        public void OwnerGrantPersistsBeforeNotificationAndSurvivesRestart()
        {
            const string saveKey="pockle.collection.v1";string grantKey=CollectionSession.PipFusionStudiesGrantKey;
            bool hadSave=PlayerPrefs.HasKey(saveKey),hadGrant=PlayerPrefs.HasKey(grantKey);
            string previousSave=PlayerPrefs.GetString(saveKey);int previousGrant=PlayerPrefs.GetInt(grantKey);
            const string legacyKey="pockle.owner-test.pip-fusion-studies.v1";bool hadLegacy=PlayerPrefs.HasKey(legacyKey);int previousLegacy=PlayerPrefs.GetInt(legacyKey);
            var obj=new GameObject("Fusion owner grant persistence");
            try
            {
                PlayerPrefs.SetString(saveKey,JsonUtility.ToJson(new CollectionSave{Version=2,Day=CollectionSession.Today,Inventory=new List<OwnedCollectible>{new OwnedCollectible(ToyCatalog.PeachId,4),new OwnedCollectible(PipFusionStudies.DuskId,1),new OwnedCollectible(PipFusionStudies.SorbetId,1)}}));
                PlayerPrefs.SetInt("pockle.owner-test.pip-fusion-studies.v1",1); // An owner with the old two-study award must receive the expansion.
                PlayerPrefs.DeleteKey(grantKey);var session=obj.AddComponent<CollectionSession>();session.Initialize();
                bool notified=false;session.Changed+=()=>{
                    var saved=JsonUtility.FromJson<CollectionSave>(PlayerPrefs.GetString(saveKey));
                    var restored=new CollectionProgress(saved,CollectionSession.Today);
                    Assert.That(restored.GetOwnedCount(PipFusionStudies.DuskId),Is.EqualTo(1));
                    foreach(string id in PipFusionStudies.Ids)Assert.That(restored.GetOwnedCount(id),Is.EqualTo(1));notified=true;
                };
                Assert.That(session.TryApplyOwnerPipFusionStudiesGrant(),Is.True);Assert.That(notified,Is.True);
                Assert.That(session.TryApplyOwnerPipFusionStudiesGrant(),Is.False);
                Assert.That(session.GetOwnedCount(ToyCatalog.PeachId),Is.EqualTo(4));
            }
            finally
            {
                Object.DestroyImmediate(obj);
                if(hadSave)PlayerPrefs.SetString(saveKey,previousSave);else PlayerPrefs.DeleteKey(saveKey);
                if(hadGrant)PlayerPrefs.SetInt(grantKey,previousGrant);else PlayerPrefs.DeleteKey(grantKey);
                if(hadLegacy)PlayerPrefs.SetInt(legacyKey,previousLegacy);else PlayerPrefs.DeleteKey(legacyKey);PlayerPrefs.Save();
            }
        }
        [UnityTest]
        public IEnumerator EclipseHasWarmNightHaloAndHoneydewRemainsSubtle()
        {
            PrototypeStage.Create(out var camera,out var mount,out _,out _,out _);var root=camera.transform.parent.gameObject;
            var obj=new GameObject("Eclipse night test");obj.transform.SetParent(mount,false);var toy=obj.AddComponent<JellyToy>();toy.Initialize();
            try
            {
                toy.TrySetCollectible(PipFusionStudies.EclipseId);PrototypeStage.SetWorld(camera,StageWorld.Midnight,toy.CollectibleId);PrototypeStage.SetLight(camera,StageLight.Night);yield return null;
                var aura=camera.GetComponent<ToyGlowAura>();Assert.That(aura.Strength,Is.EqualTo(.5f));
                var tint=mount.Find("Toy emitted halo").GetComponent<Renderer>().sharedMaterial.color;Assert.That(tint.r,Is.GreaterThan(tint.g));Assert.That(tint.g,Is.GreaterThan(tint.b));
                toy.TrySetCollectible(PipFusionStudies.HoneydewId);PrototypeStage.SetWorld(camera,StageWorld.Midnight,toy.CollectibleId);yield return null;Assert.That(aura.Strength,Is.EqualTo(.07f));
            }
            finally{Object.Destroy(root);}
            yield return null;
        }
        [UnityTest]
        public IEnumerator StudiesUseApprovedMeshDistinctSurfacesAndKeepExpressionsAndGestures()
        {
            var obj=new GameObject("Fusion material study test");var toy=obj.AddComponent<JellyToy>();toy.Initialize();
            try
            {
                Assert.That(CharacterArt.TryLoad(ToyCatalog.PeachId,out var approved),Is.True);
                var source=(Vector3[])approved.BodyMesh.vertices.Clone();
                foreach(string id in PipFusionStudies.Ids)
                {
                    Assert.That(CharacterArt.TryLoad(id,out var asset),Is.True);Assert.That(asset,Is.SameAs(approved));
                    Assert.That(toy.TrySetCollectible(id),Is.True);Assert.That(toy.UsesMeshyPeach,Is.True);
                    var mat=toy.BodyRenderer.sharedMaterial;bool dusk=id==PipFusionStudies.DuskId;
                    int mode=id==PipFusionStudies.DuskId?1:id==PipFusionStudies.SorbetId?2:id==PipFusionStudies.HoneydewId?3:4;
                    bool sorbet=id==PipFusionStudies.SorbetId;
                    Assert.That(mat.GetFloat("_FusionMode"),Is.EqualTo(mode));
                    Assert.That(mat.GetFloat("_Opacity"),sorbet?Is.EqualTo(1):Is.LessThan(1));
                    Assert.That(mat.GetFloat("_ReflectionStrength"),Is.EqualTo(dusk?.64f:sorbet?.20f:mode==3?.34f:.72f).Within(.001));
                    Assert.That(mat.GetFloat("_Glow"),Is.EqualTo(mode==4?.60f:0));
                    Assert.That(toy.GetComponentInChildren<PeachExpressionDriver>(),Is.Not.Null);
                    var driver=obj.GetComponent<PipReactionDriver>()??obj.AddComponent<PipReactionDriver>();driver.Initialize(toy);
                    Assert.That(driver.TryPlay(PipAction.CuriousTilt),Is.True);driver.ApplyPose(PipReactions.Sample(PipAction.CuriousTilt,.4f,toy.Handling==MaterialHandling.Firm,false));driver.Cancel();
                    Assert.That(toy.Handling,Is.SameAs(id==PipFusionStudies.SorbetId?MaterialHandling.Foam:MaterialHandling.Gel));
                    PipFinishMaterial.Apply(mat,ToyCatalog.PeachId);Assert.That(mat.GetFloat("_FusionMode"),Is.Zero);Assert.That(mat.GetFloat("_PearlSheen"),Is.Zero);
                    PipFinishMaterial.Apply(mat,id);Assert.That(mat.GetFloat("_FusionMode"),Is.EqualTo(mode));
                }
                CollectionAssert.AreEqual(source,approved.BodyMesh.vertices);
            }
            finally{Object.Destroy(obj);}
            yield return null;
        }
    }
}
