using Pockle.Core;
using UnityEngine;
namespace Pockle.Runtime
{
    /// <summary>Local face paint for approved Pip. Temperament never changes finish or saved inventory.</summary>
    [DefaultExecutionOrder(200)]
    public sealed class ToyTemperamentDriver : MonoBehaviour
    {
        private Material material;
        private double elapsed;
        private float pressure,lift;
        private bool paused;
        private float reactionTime,reactionDuration;
        private bool reacting;
        private PeachExpressionPose motionFace;
        private int testTap=-1;
        public PipTemperament CurrentTemperament { get; private set; }
        public bool CycleEnabled { get; private set; }
        private static readonly int Blink=Shader.PropertyToID("_TemperBlink");
        private static readonly int Happy=Shader.PropertyToID("_TemperHappy");
        private static readonly int Surprise=Shader.PropertyToID("_TemperSurprise");
        private static readonly int Lids=Shader.PropertyToID("_TemperLids");
        private static readonly int Frown=Shader.PropertyToID("_TemperFrown");
        private static readonly int GlanceX=Shader.PropertyToID("_TemperGlanceX");
        private static readonly int GlanceY=Shader.PropertyToID("_TemperGlanceY");
        private static readonly int Blush=Shader.PropertyToID("_TemperBlush");
        private static readonly int Wink=Shader.PropertyToID("_TemperWink");
        private static readonly int LidAngle=Shader.PropertyToID("_TemperLidAngle");
        private static readonly int SleepCurve=Shader.PropertyToID("_TemperSleepCurve");
        private static readonly int MouthOpen=Shader.PropertyToID("_TemperMouthOpen");
        private static readonly int MouthTilt=Shader.PropertyToID("_TemperMouthTilt");
        private static readonly int HighlightStyle=Shader.PropertyToID("_TemperHighlightStyle");
        private static readonly int MouthScale=Shader.PropertyToID("_TemperMouthScale");
        public void Initialize(Material target,string collectibleId=null)
        {
            material=target;
            var resource=(Application.isEditor||Debug.isDebugBuild)?Resources.Load<TextAsset>("PipTemperamentOwnerTest"):null;
            CycleEnabled=resource!=null&&resource.text.Trim()=="enabled";
            CurrentTemperament=CycleEnabled?PipTemperament.Sunny:Assigned(collectibleId);
            Apply(default);
        }
        private static PipTemperament Assigned(string id)
        {
            switch(PersonalityBook.TemperamentOf(id))
            {
                case Temperament.Grumpy:return PipTemperament.Grumpy;
                case Temperament.Shy:return PipTemperament.Shy;
                case Temperament.ShowOff:return PipTemperament.ShowOff;
                case Temperament.Dreamy:return PipTemperament.Dreamy;
                case Temperament.Sleepy:return PipTemperament.Sleepy;
                case Temperament.Mischief:return PipTemperament.Mischief;
                case Temperament.Calm:return PipTemperament.Calm;
                default:return PipTemperament.Sunny;
            }
        }
        public void SetCollectible(string id)
        {
            if(!CycleEnabled)CurrentTemperament=Assigned(id);
            Apply(default);
        }
        public void AdvanceTestTemperament()
        {
            if(!CycleEnabled)return;
            CurrentTemperament=PipFaces.TestTemperament(++testTap);
            Debug.Log("Toy face test: "+CurrentTemperament);
        }
        public void SetContext(float compression,float heldHeight){pressure=compression;lift=heldHeight;}
        public void Apply(PeachExpressionPose motion) => ApplyWithMotion(PipFaces.Pose(CurrentTemperament,0),motion);
        public void ApplyReaction(float progress,PeachExpressionPose motion)
        {
            ApplyWithMotion(PipFaces.Reaction(CurrentTemperament,progress),motion);
        }
        private void ApplyWithMotion(PipFacePose p,PeachExpressionPose motion)
        {
            ApplyFace(p);
            if(material==null)return;
            material.SetFloat(Blink,Mathf.Max(p.Blink,motion.Blink));
            material.SetFloat(Happy,Mathf.Max(p.Happy,motion.Happy));
            material.SetFloat(Surprise,Mathf.Max(p.Surprise,motion.Surprise));
            material.SetFloat(MouthOpen,Mathf.Max(p.MouthOpen,motion.Surprise*.8f));
        }
        public void ApplyFace(PipFacePose p)
        {
            if(material==null)return;
            material.SetFloat(Blink,p.Blink);
            material.SetFloat(Happy,p.Happy);
            material.SetFloat(Surprise,p.Surprise);
            material.SetFloat(Lids,p.Lids);
            material.SetFloat(Frown,p.Frown);
            material.SetFloat(GlanceX,p.GlanceX);
            material.SetFloat(GlanceY,p.GlanceY);
            material.SetFloat(Blush,p.Blush);
            material.SetFloat(Wink,p.Wink);
            material.SetFloat(LidAngle,p.LidAngle);
            material.SetFloat(SleepCurve,p.SleepCurve);
            material.SetFloat(MouthOpen,p.MouthOpen);
            material.SetFloat(MouthTilt,p.MouthTilt);
            material.SetFloat(HighlightStyle,p.HighlightStyle);
            material.SetFloat(MouthScale,p.MouthScale>0?p.MouthScale:1);
        }
        public void BeginTap(float duration)
        {
            AdvanceTestTemperament();reactionDuration=float.IsNaN(duration)||float.IsInfinity(duration)?1.8f:Mathf.Clamp(duration,.1f,5);reactionTime=0;reacting=true;
        }
        public void SetMotionFace(PeachExpressionPose pose){motionFace=pose;if(reacting)ApplyReaction(reactionTime/reactionDuration,pose);else Apply(pose);}
        public void CancelTap(){reacting=false;reactionTime=0;motionFace=default;Apply(default);}
        private void LateUpdate()
        {
            if(paused||material==null)return;
            elapsed+=Mathf.Max(0,Time.unscaledDeltaTime);
            var touch=PeachExpression.Sample(elapsed,pressure,lift,PlayerPrefs.GetInt("pockle.prototype.reducedMotion",0)==1);
            var motion=new PeachExpressionPose(Mathf.Max(touch.Blink,motionFace.Blink),Mathf.Max(touch.Happy,motionFace.Happy),Mathf.Max(touch.Surprise,motionFace.Surprise));
            if(reacting)
            {
                reactionTime+=Mathf.Max(0,Time.unscaledDeltaTime);
                if(reactionTime>=reactionDuration){reacting=false;motionFace=default;Apply(touch);}
                else ApplyReaction(reactionTime/reactionDuration,motion);
            }
            else Apply(motion);
        }
        public bool IsReacting => reacting;
        private void OnDisable(){CancelTap();}
        private void OnApplicationFocus(bool value){if(!value)CancelTap();}
        private void OnApplicationPause(bool value){paused=value;if(value)CancelTap();}
    }
}
