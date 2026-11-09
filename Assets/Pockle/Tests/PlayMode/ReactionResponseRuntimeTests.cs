using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using Pockle.Core;
using Pockle.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace Pockle.Tests
{
    public sealed class ReactionResponseRuntimeTests
    {
        [Test] public void EveryResponseUsesAnImportedRecordingForItsCharacter()
        {
            foreach(var pair in new[]{("pip",typeof(PipAction)),("fox",typeof(FoxAction)),("axolotl",typeof(AxolotlAction)),("firb",typeof(FirbAction)),("coco",typeof(CocoAction)),("snowy",typeof(SnowyAction)),("ginger",typeof(GingerAction)),("reiny",typeof(WinterTrioAction)),("guin",typeof(WinterTrioAction)),("bundy",typeof(WinterTrioAction)),("wally",typeof(WallyAction))})
            {
                foreach(Enum action in Enum.GetValues(pair.Item2)) Assert.NotNull(Resources.Load<AudioClip>(ToyReactionResponses.Action(pair.Item1,action).AudioResource),pair.Item1+action);
                foreach(PipTemperament face in Enum.GetValues(typeof(PipTemperament))) Assert.NotNull(Resources.Load<AudioClip>(ToyReactionResponses.Temperament(pair.Item1,face).AudioResource),pair.Item1+face);
            }
        }
        [UnityTest] public IEnumerator GrumpyResponseOverridesHappyMotionForPipAndNativeFacesWithoutChangingFinish()
        {
            bool had=PlayerPrefs.HasKey(PipCharacterAsset.MeshyPreference);int old=PlayerPrefs.GetInt(PipCharacterAsset.MeshyPreference);
            PlayerPrefs.SetInt(PipCharacterAsset.MeshyPreference,1);
            var root=new GameObject("Coherent faces");var toy=root.AddComponent<JellyToy>();toy.Initialize();
            try
            {
                foreach(string id in new[]{ToyCatalog.PeachId,"fox.gloss-vinyl"})
                {
                    Assert.IsTrue(toy.TrySetCollectible(id)); var native=toy.GetComponentInChildren<ToyTemperamentDriver>();var pip=toy.GetComponentInChildren<PeachExpressionDriver>();
                    IToyResponseFace face=(IToyResponseFace)native??pip; Assert.NotNull(face);
                    string character=id.StartsWith("fox")?"fox":"pip";var material=toy.BodyRenderer.sharedMaterial;
                    var paint=material.mainTexture;var response=ToyReactionResponses.Temperament(character,PipTemperament.Grumpy);
                    face.BeginResponse(response,3);
                    face.GetType().GetField("responseStarted",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(face,Time.unscaledTime-1.2f);
                    if(native!=null){native.CancelTap();native.Apply(new PeachExpressionPose(0,1,1));}
                    else pip.Apply(new PeachExpressionPose(0,1,1));
                    string prefix=native!=null?"_Temper":"_";
                    Assert.Greater(material.GetFloat(prefix+"Frown"),.5f);Assert.Less(material.GetFloat(prefix+"Happy"),.1f);
                    Assert.AreSame(paint,material.mainTexture);
                }
            }
            finally{UnityEngine.Object.Destroy(root);if(had)PlayerPrefs.SetInt(PipCharacterAsset.MeshyPreference,old);else PlayerPrefs.DeleteKey(PipCharacterAsset.MeshyPreference);}
            yield return null;
        }
        [Test] public void BubbleHasOnePaintedSilhouetteAndKeepsTheFullResponseDuration()
        {
            var root=new GameObject("Response bubble",typeof(RectTransform),typeof(CanvasGroup),typeof(SpeechBubbleGraphic));
            var pop=root.AddComponent<BubblePop>();
            try
            {
                pop.Play(0,false,3.5f);
                Assert.That(root.GetComponentsInChildren<Graphic>().Length,Is.EqualTo(1));
                Assert.That((float)typeof(BubblePop).GetField("hold",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(pop),Is.GreaterThanOrEqualTo(3.5f-BubblePop.In-BubblePop.Out));
            }
            finally{UnityEngine.Object.DestroyImmediate(root);}
        }
    }
}
