using Pockle.Core;
using UnityEngine;
namespace Pockle.Runtime
{
    [DefaultExecutionOrder(120)]
    public sealed class CocoReactionDriver : MonoBehaviour
    {
        public bool IsPlaying{get;private set;}
        public CocoAction CurrentAction{get;private set;}
        private JellyToy toy;private Transform offset;private float elapsed;private int last=-1;private string playingId;private bool paused;
        public void Initialize(JellyToy body){toy=body;if(offset!=null)return;offset=new GameObject("Coco signature reaction offset").transform;offset.SetParent(toy.transform,false);}
        public bool TryPlay(CocoAction action)
        {
            if(toy==null||!gameObject.activeInHierarchy||!ToyCatalog.TryGetCollectible(toy.CollectibleId,out var item)||item.CharacterId!="coco"||!System.Enum.IsDefined(typeof(CocoAction),action))return false;
            if(toy.VisualRoot.parent!=offset)toy.VisualRoot.SetParent(offset,false);
            toy.GetComponentInChildren<ToyTemperamentDriver>()?.BeginTap(CocoReactions.Duration);
            CurrentAction=action;last=(int)action;elapsed=0;playingId=toy.CollectibleId;IsPlaying=true;return true;
        }
        public void PlayRandom(){if(!IsPlaying)TryPlay(CocoReactions.Choose(last,Random.Range(0,last<0?5:4)));}
        public void ApplyPose(CocoActionPose pose)
        {
            if(offset==null||toy==null)return;
            offset.localPosition=Vector3.up*pose.Lift;offset.localRotation=Quaternion.Euler(pose.Pitch,pose.Yaw,pose.Roll);offset.localScale=Vector3.one;
            toy.GetComponentInChildren<ToyTemperamentDriver>()?.SetMotionFace(pose.Face);
            var face=toy.GetComponentInChildren<HolidayExpressionDriver>();if(face!=null)face.Apply(pose.Face,0);
            var material=toy.BodyRenderer.sharedMaterial;
            if(material!=null&&material.HasProperty("_CocoCream")){material.SetFloat("_CocoCream",pose.Cream);material.SetFloat("_CocoMarshmallows",pose.Marshmallows);}
        }
        public void Cancel(){toy?.GetComponentInChildren<ToyTemperamentDriver>()?.CancelTap();if(IsPlaying||offset!=null&&offset.childCount>0)ApplyPose(default);IsPlaying=false;elapsed=0;}
        private void LateUpdate()
        {
            if(!IsPlaying||paused)return;if(toy.CollectibleId!=playingId){Cancel();return;}
            elapsed+=Mathf.Max(0,Time.unscaledDeltaTime);if(elapsed>=CocoReactions.Duration){Cancel();return;}
            ApplyPose(CocoReactions.Sample(CurrentAction,elapsed/CocoReactions.Duration,PlayerPrefs.GetInt("pockle.prototype.reducedMotion",0)==1));
        }
        private void OnDisable(){Cancel();}private void OnApplicationPause(bool value){paused=value;}private void OnApplicationFocus(bool focus){if(!focus)Cancel();}
    }
}
