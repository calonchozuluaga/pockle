using Pockle.Core;
using UnityEngine;
namespace Pockle.Runtime
{
    [DefaultExecutionOrder(110)]
    public sealed class FoxReactionDriver : MonoBehaviour
    {
        public bool IsPlaying { get; private set; }
        public FoxAction CurrentAction { get; private set; }
        private JellyToy toy;
        private Transform offset;
        private float elapsed;
        private int lastAction=-1;
        private string playingId;
        private bool paused;
        public void Initialize(JellyToy body){toy=body;if(offset!=null)return;offset=new GameObject("Fox signature reaction offset").transform;offset.SetParent(toy.transform,false);}
        public bool TryPlay(FoxAction action)
        {
            if(toy==null||!gameObject.activeInHierarchy||!ToyCatalog.IsFoxCollectible(toy.CollectibleId)||!System.Enum.IsDefined(typeof(FoxAction),action))return false;
            if(toy.VisualRoot.parent!=offset)toy.VisualRoot.SetParent(offset,false);
            toy.GetComponentInChildren<ToyTemperamentDriver>()?.BeginTap(FoxReactions.Duration);
            CurrentAction=action;lastAction=(int)action;elapsed=0;playingId=toy.CollectibleId;IsPlaying=true;return true;
        }
        public void PlayRandom(){if(!IsPlaying)TryPlay(FoxReactions.Choose(lastAction,Random.Range(0,lastAction<0?5:4)));}
        public void ApplyPose(FoxActionPose pose)
        {
            if(toy==null||offset==null)return;
            offset.localPosition=Vector3.up*pose.Lift;offset.localRotation=Quaternion.Euler(pose.Pitch,pose.Yaw,pose.Roll);offset.localScale=Vector3.one;
            toy.SetFoxReaction(pose);
            toy.GetComponentInChildren<ToyTemperamentDriver>()?.SetMotionFace(new PeachExpressionPose(pose.Blink,pose.Happy,pose.Surprise));
        }
        public void Cancel(){toy?.GetComponentInChildren<ToyTemperamentDriver>()?.CancelTap();if(IsPlaying||offset!=null&&offset.childCount>0)ApplyPose(default);IsPlaying=false;elapsed=0;}
        private void LateUpdate()
        {
            if(!IsPlaying||paused)return;
            if(toy.CollectibleId!=playingId){Cancel();return;}
            elapsed+=Mathf.Max(0,Time.unscaledDeltaTime);
            if(elapsed>=FoxReactions.Duration){Cancel();return;}
            ApplyPose(FoxReactions.Sample(CurrentAction,elapsed/FoxReactions.Duration,PlayerPrefs.GetInt("pockle.prototype.reducedMotion",0)==1));
        }
        private void OnDisable(){Cancel();}
        private void OnApplicationPause(bool value){paused=value;}
        private void OnApplicationFocus(bool focus){if(!focus)Cancel();}
    }
}
