using Pockle.Core;
using UnityEngine;
namespace Pockle.Runtime
{
    /// <summary>Local face paint for approved Pip. Temperament never changes finish or saved inventory.</summary>
    public sealed class PeachExpressionDriver : MonoBehaviour
    {
        private Material material;
        private double elapsed;
        private float pressure,lift;
        private bool paused;
        private int testTap=-1;
        public PipTemperament CurrentTemperament { get; private set; }
        public bool CycleEnabled { get; private set; }
        private static readonly int Blink=Shader.PropertyToID("_Blink");
        private static readonly int Happy=Shader.PropertyToID("_Happy");
        private static readonly int Surprise=Shader.PropertyToID("_Surprise");
        private static readonly int Lids=Shader.PropertyToID("_Lids");
        private static readonly int Frown=Shader.PropertyToID("_Frown");
        private static readonly int GlanceX=Shader.PropertyToID("_GlanceX");
        private static readonly int GlanceY=Shader.PropertyToID("_GlanceY");
        private static readonly int Blush=Shader.PropertyToID("_Blush");
        private static readonly int Wink=Shader.PropertyToID("_Wink");
        private static readonly int LidAngle=Shader.PropertyToID("_LidAngle");
        private static readonly int SleepCurve=Shader.PropertyToID("_SleepCurve");
        private static readonly int MouthOpen=Shader.PropertyToID("_MouthOpen");
        private static readonly int MouthTilt=Shader.PropertyToID("_MouthTilt");
        private static readonly int HighlightStyle=Shader.PropertyToID("_HighlightStyle");
        private static readonly int MouthScale=Shader.PropertyToID("_MouthScale");
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
            Debug.Log("Pip face test: "+CurrentTemperament);
        }
        public void SetContext(float compression,float heldHeight){pressure=compression;lift=heldHeight;}
        // Legacy anchors are retained for compatibility; rest UV coordinates now follow tactile deformation.
        public void SetCenters(Vector3 left,Vector3 right,Vector3 mouth)
        {
            if(material==null||material.HasProperty("_PipFace"))return;
            material.SetVector("_LeftEye",new Vector4(left.x,left.y,.15f,.17f));
            material.SetVector("_RightEye",new Vector4(right.x,right.y,.15f,.17f));
            material.SetVector("_Mouth",new Vector4(mouth.x,mouth.y,.12f,.09f));
        }
        public void Apply(PeachExpressionPose motion) => ApplyWithMotion(PipFaces.Pose(CurrentTemperament,0),motion);
        public void ApplyReaction(PipAction action,float progress,PeachExpressionPose motion)
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
        private void LateUpdate()
        {
            if(paused||material==null)return;
            elapsed+=Mathf.Max(0,Time.unscaledDeltaTime);
            Apply(PeachExpression.Sample(elapsed,pressure,lift,PlayerPrefs.GetInt("pockle.prototype.reducedMotion",0)==1));
        }
        private void OnApplicationPause(bool value){paused=value;}
    }
}
