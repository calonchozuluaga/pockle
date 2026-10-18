using Pockle.Core;
using UnityEngine;
namespace Pockle.Runtime
{
    [DefaultExecutionOrder(120)]
    public sealed class WinterTrioReactionDriver : MonoBehaviour
    {
        public bool IsPlaying{get;private set;}
        public WinterTrioAction CurrentAction{get;private set;}
        private JellyToy toy;private Transform offset;private float elapsed;private int last=-1;private string playingId,character,lastCharacter;private bool paused;
        public void Initialize(JellyToy body){toy=body;if(offset!=null)return;offset=new GameObject("Winter signature reaction offset").transform;offset.SetParent(toy.transform,false);}
        public bool TryPlay(WinterTrioAction action)
        {
            if(toy==null||!gameObject.activeInHierarchy||!ToyCatalog.TryGetCollectible(toy.CollectibleId,out var item)||!HolidayCatalog.IsWinterTrio(item.CharacterId)||!System.Enum.IsDefined(typeof(WinterTrioAction),action))return false;
            if(toy.VisualRoot.parent!=offset)toy.VisualRoot.SetParent(offset,false);
            toy.GetComponentInChildren<ToyTemperamentDriver>()?.BeginTap(WinterTrioReactions.Duration);
            character=item.CharacterId;lastCharacter=character;CurrentAction=action;last=(int)action;elapsed=0;playingId=toy.CollectibleId;IsPlaying=true;return true;
        }
        public void PlayRandom()
        {
            if(IsPlaying||!ToyCatalog.TryGetCollectible(toy.CollectibleId,out var item)||!HolidayCatalog.IsWinterTrio(item.CharacterId))return;
            if(lastCharacter!=item.CharacterId)last=-1;
            TryPlay(WinterTrioReactions.Choose(last,Random.Range(0,last<0?5:4)));
        }
        public void ApplyPose(WinterTrioPose pose)
        {
            if(offset==null||toy==null)return;
            offset.localPosition=Vector3.up*pose.Lift;offset.localRotation=Quaternion.Euler(pose.Pitch,pose.Yaw,pose.Roll);offset.localScale=Vector3.one;
            toy.GetComponentInChildren<ToyTemperamentDriver>()?.SetMotionFace(pose.Face);
            toy.GetComponentInChildren<HolidayExpressionDriver>()?.Apply(pose.Face,0);
            var material=toy.BodyRenderer==null?null:toy.BodyRenderer.sharedMaterial;
            if(material!=null&&material.HasProperty("_WinterWave")){material.SetFloat("_WinterWave",pose.Wave);material.SetFloat("_WinterFeature",pose.Feature);material.SetFloat("_WinterNod",pose.Nod);}
        }
        public void Cancel(){if(toy!=null&&ToyCatalog.TryGetCollectible(toy.CollectibleId,out var item)&&HolidayCatalog.IsWinterTrio(item.CharacterId)){toy.GetComponentInChildren<ToyTemperamentDriver>()?.CancelTap();ApplyPose(default);}IsPlaying=false;elapsed=0;if(offset!=null){offset.localPosition=Vector3.zero;offset.localRotation=Quaternion.identity;}}
        private void LateUpdate()
        {
            if(!IsPlaying||paused)return;if(toy.CollectibleId!=playingId){Cancel();return;}
            elapsed+=Mathf.Max(0,Time.unscaledDeltaTime);if(elapsed>=WinterTrioReactions.Duration){Cancel();return;}
            ApplyPose(WinterTrioReactions.Sample(character,CurrentAction,elapsed/WinterTrioReactions.Duration,PlayerPrefs.GetInt("pockle.prototype.reducedMotion",0)==1));
        }
        private void OnDisable(){Cancel();}private void OnApplicationPause(bool value){paused=value;if(value)Cancel();}private void OnApplicationFocus(bool focus){if(!focus)Cancel();}
    }
}
