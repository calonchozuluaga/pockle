using Pockle.Core;
using UnityEngine;
namespace Pockle.Runtime
{
    /// <summary>Local whale articulation plus whole-body reactions; no scale deformation on rigid finishes.</summary>
    [DefaultExecutionOrder(120)]
    public sealed class WallyReactionDriver : MonoBehaviour
    {
        public bool IsPlaying { get; private set; }
        public WallyAction CurrentAction { get; private set; }
        private JellyToy toy;
        private Transform offset;
        private float elapsed,idle;
        private int last=-1;
        private string playingId;
        private bool paused,focused=true;
        public void Initialize(JellyToy body)
        {
            toy=body;
            if(offset==null){offset=new GameObject("Wally signature reaction offset").transform;offset.SetParent(toy.transform,false);}
        }
        private bool IsWally => toy!=null && ToyCatalog.TryGetCollectible(toy.CollectibleId,out var item) && item.CharacterId=="wally";
        public bool TryPlay(WallyAction action)
        {
            if(!IsWally||!gameObject.activeInHierarchy||paused||!focused||!System.Enum.IsDefined(typeof(WallyAction),action))return false;
            if(toy.VisualRoot.parent!=offset)toy.VisualRoot.SetParent(offset,false);
            toy.GetComponentInChildren<ToyTemperamentDriver>()?.BeginTap(WallyReactions.Duration);
            CurrentAction=action;last=(int)action;elapsed=0;playingId=toy.CollectibleId;IsPlaying=true;return true;
        }
        public void PlayRandom(){if(!IsPlaying)TryPlay(WallyReactions.Choose(last,Random.Range(0,last<0?5:4)));}
        public void ApplyPose(WallyActionPose pose)
        {
            if(offset==null||toy==null)return;
            offset.localPosition=Vector3.up*pose.Lift;offset.localRotation=Quaternion.Euler(pose.Pitch,pose.Yaw,pose.Roll);offset.localScale=Vector3.one;
            if(!IsWally)return;
            toy.GetComponentInChildren<ToyTemperamentDriver>()?.SetMotionFace(pose.Face);
            var m=toy.BodyRenderer.sharedMaterial;
            if(m!=null&&m.HasProperty("_WallyTail")){m.SetFloat("_WallyTail",pose.Tail);m.SetFloat("_WallyFlipper",pose.Flipper);}
        }
        public void Cancel()
        {
            if(IsWally)toy.GetComponentInChildren<ToyTemperamentDriver>()?.CancelTap();
            ApplyPose(default);IsPlaying=false;elapsed=0;
        }
        private void LateUpdate()
        {
            if(paused||!focused||!IsWally){if(IsPlaying)Cancel();return;}
            bool calm=PlayerPrefs.GetInt("pockle.prototype.reducedMotion",0)==1;
            idle+=Mathf.Max(0,Time.unscaledDeltaTime);
            toy.BodyRenderer.sharedMaterial.SetFloat("_WallyIdle",calm?0:Mathf.Sin(idle*1.3f));
            if(!IsPlaying)return;
            if(toy.CollectibleId!=playingId){Cancel();return;}
            elapsed+=Mathf.Max(0,Time.unscaledDeltaTime);
            if(elapsed>=WallyReactions.Duration){Cancel();return;}
            ApplyPose(WallyReactions.Sample(CurrentAction,elapsed/WallyReactions.Duration,calm));
        }
        private void OnDisable(){Cancel();}
        private void OnApplicationPause(bool value){paused=value;if(value)Cancel();}
        private void OnApplicationFocus(bool value){focused=value;if(!value)Cancel();}
    }
}
