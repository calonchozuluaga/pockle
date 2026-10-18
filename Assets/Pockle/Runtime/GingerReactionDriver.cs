using Pockle.Core;
using UnityEngine;
namespace Pockle.Runtime
{
    [DefaultExecutionOrder(120)]
    public sealed class GingerReactionDriver : MonoBehaviour
    {
        public bool IsPlaying{get;private set;}
        public GingerAction CurrentAction{get;private set;}
        private JellyToy toy;private Transform offset;private float elapsed;private int last=-1;private string playingId;private bool paused;
        public void Initialize(JellyToy body){toy=body;if(offset!=null)return;offset=new GameObject("Ginger signature reaction offset").transform;offset.SetParent(toy.transform,false);}
        public bool TryPlay(GingerAction action)
        {
            if(toy==null||!gameObject.activeInHierarchy||!ToyCatalog.TryGetCollectible(toy.CollectibleId,out var item)||item.CharacterId!="ginger"||!System.Enum.IsDefined(typeof(GingerAction),action))return false;
            if(toy.VisualRoot.parent!=offset)toy.VisualRoot.SetParent(offset,false);
            toy.GetComponentInChildren<ToyTemperamentDriver>()?.BeginTap(GingerReactions.Duration);
            CurrentAction=action;last=(int)action;elapsed=0;playingId=toy.CollectibleId;IsPlaying=true;return true;
        }
        public void PlayRandom(){if(!IsPlaying)TryPlay(GingerReactions.Choose(last,Random.Range(0,last<0?5:4)));}
        public void ApplyPose(GingerActionPose pose)
        {
            if(offset==null||toy==null)return;
            offset.localPosition=Vector3.up*pose.Lift;offset.localRotation=Quaternion.Euler(pose.Pitch,pose.Yaw,pose.Roll);offset.localScale=Vector3.one;
            toy.GetComponentInChildren<ToyTemperamentDriver>()?.SetMotionFace(pose.Face);
            var face=toy.GetComponentInChildren<HolidayExpressionDriver>();if(face!=null)face.Apply(pose.Face,0);
            var material=toy.BodyRenderer.sharedMaterial;
            if(material!=null&&material.HasProperty("_GingerPaw")){material.SetFloat("_GingerPaw",pose.Paw);material.SetFloat("_GingerEars",pose.Ears);material.SetFloat("_GingerHead",pose.Head);}
        }
        public void Cancel(){toy?.GetComponentInChildren<ToyTemperamentDriver>()?.CancelTap();if(IsPlaying||offset!=null&&offset.childCount>0)ApplyPose(default);IsPlaying=false;elapsed=0;}
        private void LateUpdate()
        {
            if(!IsPlaying||paused)return;if(toy.CollectibleId!=playingId){Cancel();return;}
            elapsed+=Mathf.Max(0,Time.unscaledDeltaTime);if(elapsed>=GingerReactions.Duration){Cancel();return;}
            ApplyPose(GingerReactions.Sample(CurrentAction,elapsed/GingerReactions.Duration,PlayerPrefs.GetInt("pockle.prototype.reducedMotion",0)==1));
        }
        private void OnDisable(){Cancel();}private void OnApplicationPause(bool value){paused=value;}private void OnApplicationFocus(bool focus){if(!focus)Cancel();}
    }
}
