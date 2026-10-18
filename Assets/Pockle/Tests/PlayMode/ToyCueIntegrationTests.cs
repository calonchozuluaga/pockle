using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
namespace Pockle.Tests
{
    public sealed class ToyCueIntegrationTests
    {
        private static object Field(object o,string name)=>o.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(o);
        private static void Call(object o,string name,params object[] args)=>o.GetType().GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(o,args);
        [UnityTest]
        public IEnumerator EveryPlayableCharacterFeedsAcceptedReactionsToClaudeSpeechWithoutChangingInventory()
        {
            const string key="pockle.collection.v1";bool had=PlayerPrefs.HasKey(key);string prior=PlayerPrefs.GetString(key);
            bool hadVariant=PlayerPrefs.HasKey(PipVariants.Preference);int variant=PlayerPrefs.GetInt(PipVariants.Preference);
            var ambient=RenderSettings.ambientLight;var mode=RenderSettings.ambientMode;bool fog=RenderSettings.fog;
            int fps=Application.targetFrameRate,vsync=QualitySettings.vSyncCount;
            var root=new GameObject("Combined toy and HUD integration");
            try
            {
                var save=new CollectionSave{Version=CollectionProgress.SaveVersion,Day=CollectionSession.Today,
                    Inventory=ToyCatalog.Collectibles.Where(t=>t.Available).Select(t=>new OwnedCollectible(t.Id,1)).ToList()};
                PlayerPrefs.SetString(key,JsonUtility.ToJson(save));PlayerPrefs.SetInt(PipVariants.Preference,0);
                var runtime=root.AddComponent<TactilePrototype>();yield return null;yield return null;
                var hud=(PrototypeHud)Field(runtime,"hud");var toy=(JellyToy)Field(runtime,"toy");var input=root.GetComponent<ToyViewGesture>();
                var cues=new List<ReactionCue>();var heads=new List<Vector2?>();runtime.ToyReactionStarted+=(cue,head)=>{cues.Add(cue);heads.Add(head);};
                foreach(string id in new[]{ToyCatalog.PeachId,ToyCatalog.FoxId,ToyCatalog.AxolotlId,"firb.matte-vinyl","coco.matte-vinyl","ginger.matte-vinyl","snowy.matte-vinyl","reiny.matte-vinyl","guin.matte-vinyl","bundy.matte-vinyl"})
                {
                    Assert.That(hud.ShowToy(id),Is.True);yield return null;yield return null;
                    int before=cues.Count;Call(runtime,"PlayTapReaction");Assert.That(cues.Count,Is.EqualTo(before+1));Assert.That(cues.Last(),Is.EqualTo(ReactionCue.Poke));
                    Call(runtime,"PlayTapReaction");Assert.That(cues.Count,Is.EqualTo(before+1),"Ignored taps must not advance the expression picker.");
                    Assert.That(heads.Last().HasValue,Is.True);Assert.That(float.IsNaN(heads.Last().Value.x),Is.False);
                }
                hud.ShowToy(ToyCatalog.PeachId);yield return null;
                input.SoftManipulationStarted(Vector2.zero);input.SoftManipulated(new SoftToyPose(.12f,.24f,0),Vector3.up);
                int squishes=cues.Count;input.SoftManipulated(new SoftToyPose(.13f,.25f,0),Vector3.up);Assert.That(cues.Count,Is.EqualTo(squishes));
                Assert.That(cues.Last(),Is.EqualTo(ReactionCue.Squish));
                input.SoftManipulated(new SoftToyPose(0,.18f,.2f),Vector3.up);Assert.That(cues.Last(),Is.EqualTo(ReactionCue.PickUp));
                input.SoftManipulationEnded(false);input.TurnStarted();Assert.That(cues.Last(),Is.EqualTo(ReactionCue.Turn));
                Call(runtime,"SetLight",StageLight.Night);Assert.That(cues.Last(),Is.EqualTo(ReactionCue.Night));
                runtime.GetType().GetField("lastToyActivity",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(runtime,Time.unscaledTime-20);
                Call(runtime,"ReadInput");Assert.That(cues.Last(),Is.EqualTo(ReactionCue.LeftAlone));int idle=cues.Count;Call(runtime,"ReadInput");Assert.That(cues.Count,Is.EqualTo(idle));
                var session=root.GetComponent<CollectionSession>();foreach(var item in ToyCatalog.Collectibles.Where(t=>t.Available))Assert.That(session.GetOwnedCount(item.Id),Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(root);
                if(had)PlayerPrefs.SetString(key,prior);else PlayerPrefs.DeleteKey(key);
                if(hadVariant)PlayerPrefs.SetInt(PipVariants.Preference,variant);else PlayerPrefs.DeleteKey(PipVariants.Preference);
                PlayerPrefs.Save();RenderSettings.ambientLight=ambient;RenderSettings.ambientMode=mode;RenderSettings.fog=fog;
                Application.targetFrameRate=fps;QualitySettings.vSyncCount=vsync;
            }
            yield return null;yield return null;
        }
    }
}
