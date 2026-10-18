using Pockle.Core;
using UnityEngine;
namespace Pockle.Runtime
{
    /// <summary>Reaction offset sits above the existing visual pose, preserving touch and viewer transforms.</summary>
    [DefaultExecutionOrder(100)]
    public sealed class PipReactionDriver : MonoBehaviour
    {
        public bool IsPlaying { get; private set; }
        public PipAction CurrentAction { get; private set; }
        private JellyToy toy;
        private Transform offset;
        private float elapsed;
        private int lastAction=-1;
        private string playingId;
        private bool paused;
        public void Initialize(JellyToy body)
        {
            toy=body;
            if(offset!=null)return;
            offset=new GameObject("Pip signature reaction offset").transform;offset.SetParent(toy.transform,false);
            toy.VisualRoot.SetParent(offset,false);
        }
        public bool TryPlay(PipAction action)
        {
            if(toy==null||!gameObject.activeInHierarchy||!ToyCatalog.TryGetCollectible(toy.CollectibleId,out var item)||item.CharacterId!="pip"||
                !System.Enum.IsDefined(typeof(PipAction),action))return false;
            if(toy.VisualRoot.parent!=offset)toy.VisualRoot.SetParent(offset,false);
            toy.GetComponentInChildren<PeachExpressionDriver>()?.AdvanceTestTemperament();
            CurrentAction=action;lastAction=(int)action;elapsed=0;playingId=toy.CollectibleId;IsPlaying=true;return true;
        }
        public void PlayRandom()
        {
            if(IsPlaying)return;
            TryPlay(PipReactions.Choose(lastAction,Random.Range(0,lastAction<0?5:4)));
        }
        public void ApplyPose(PipActionPose pose)
        {
            if(offset==null)return;
            offset.localPosition=Vector3.up*pose.Lift;
            offset.localRotation=Quaternion.Euler(pose.Pitch,pose.Yaw,pose.Roll);
            float height=1-pose.Squash,spread=1/Mathf.Sqrt(height);
            offset.localScale=new Vector3(spread,height,spread);
            var face=toy.GetComponentInChildren<PeachExpressionDriver>();if(face!=null)face.Apply(pose.Face);
        }
        public void Cancel(){IsPlaying=false;elapsed=0;ApplyPose(default);}
        private void LateUpdate()
        {
            if(!IsPlaying||paused)return;
            if(toy.CollectibleId!=playingId){Cancel();return;}
            elapsed+=Mathf.Min(Time.unscaledDeltaTime,.1f);
            if(elapsed>=PipReactions.Duration){Cancel();return;}
            float progress=elapsed/PipReactions.Duration;
            var pose=PipReactions.Sample(CurrentAction,progress,toy.Handling==MaterialHandling.Firm,
                PlayerPrefs.GetInt("pockle.prototype.reducedMotion",0)==1);
            ApplyPose(pose);
            toy.GetComponentInChildren<PeachExpressionDriver>()?.ApplyReaction(CurrentAction,progress,pose.Face);
        }
        private void OnDisable(){Cancel();}
        private void OnApplicationPause(bool value){paused=value;}
        private void OnApplicationFocus(bool focus){if(!focus)Cancel();}
    }
}
