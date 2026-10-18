using System;
namespace Pockle.Core
{
    public enum PipTemperament { Sunny, Grumpy, Shy, ShowOff, Dreamy, Sleepy, Mischief, Calm }
    /// <summary>Bounded local face controls; independent from material, color and inventory.</summary>
    public readonly struct PipFacePose
    {
        public readonly float Blink, Happy, Surprise, Lids, Frown, GlanceX, GlanceY, Blush, Wink, LidAngle, SleepCurve, MouthOpen, MouthTilt, HighlightStyle, MouthScale;
        public PipFacePose(float blink=0,float happy=0,float surprise=0,float lids=0,float frown=0,float glanceX=0,float glanceY=0,float blush=0,float wink=0,float lidAngle=0,float sleepCurve=0,float mouthOpen=0,float mouthTilt=0,float highlightStyle=0,float mouthScale=1)
        {
            Blink=Numeric.Clamp(Numeric.FiniteOr(blink,0),0f,1f);
            Happy=Numeric.Clamp(Numeric.FiniteOr(happy,0),0f,1f);
            Surprise=Numeric.Clamp(Numeric.FiniteOr(surprise,0),0f,1f);
            Lids=Numeric.Clamp(Numeric.FiniteOr(lids,0),0f,1f);
            Frown=Numeric.Clamp(Numeric.FiniteOr(frown,0),0f,1f);
            GlanceX=Numeric.Clamp(Numeric.FiniteOr(glanceX,0),-1f,1f);
            GlanceY=Numeric.Clamp(Numeric.FiniteOr(glanceY,0),-1f,1f);
            Blush=Numeric.Clamp(Numeric.FiniteOr(blush,0),0f,1f);
            Wink=Numeric.Clamp(Numeric.FiniteOr(wink,0),0f,1f);
            LidAngle=Numeric.Clamp(Numeric.FiniteOr(lidAngle,0),-1f,1f);
            SleepCurve=Numeric.Clamp(Numeric.FiniteOr(sleepCurve,0),0f,1f);
            MouthOpen=Numeric.Clamp(Numeric.FiniteOr(mouthOpen,0),0f,1f);
            MouthTilt=Numeric.Clamp(Numeric.FiniteOr(mouthTilt,0),-1f,1f);
            HighlightStyle=Numeric.Clamp(Numeric.FiniteOr(highlightStyle,0),0f,2f);
            MouthScale=Numeric.Clamp(Numeric.FiniteOr(mouthScale,1),0.5f,1.5f);
        }
        public static PipFacePose Blend(PipFacePose a,PipFacePose b,float t)
        {
            t=Numeric.Clamp(Numeric.FiniteOr(t,0),0,1);t=t*t*(3-2*t);
            return new PipFacePose(blink:a.Blink+(b.Blink-a.Blink)*t,happy:a.Happy+(b.Happy-a.Happy)*t,surprise:a.Surprise+(b.Surprise-a.Surprise)*t,lids:a.Lids+(b.Lids-a.Lids)*t,frown:a.Frown+(b.Frown-a.Frown)*t,glanceX:a.GlanceX+(b.GlanceX-a.GlanceX)*t,glanceY:a.GlanceY+(b.GlanceY-a.GlanceY)*t,blush:a.Blush+(b.Blush-a.Blush)*t,wink:a.Wink+(b.Wink-a.Wink)*t,lidAngle:a.LidAngle+(b.LidAngle-a.LidAngle)*t,sleepCurve:a.SleepCurve+(b.SleepCurve-a.SleepCurve)*t,mouthOpen:a.MouthOpen+(b.MouthOpen-a.MouthOpen)*t,mouthTilt:a.MouthTilt+(b.MouthTilt-a.MouthTilt)*t,highlightStyle:a.HighlightStyle+(b.HighlightStyle-a.HighlightStyle)*t,mouthScale:a.MouthScale+(b.MouthScale-a.MouthScale)*t);
        }
    }
    /// <summary>Approved pass02 face sets; no network, mutable shared state or per-frame allocations.</summary>
    public static class PipFaces
    {
        private static readonly PipFacePose[,] poses = {
            { new PipFacePose(happy:0.15f,blush:0.12f), new PipFacePose(happy:0.8f,lids:0.1f,blush:0.25f), new PipFacePose(happy:0.55f,glanceX:0.2f,blush:0.2f) }, // Sunny
            { new PipFacePose(lids:0.55f,frown:1f,glanceX:0.55f,lidAngle:0.65f), new PipFacePose(lids:0.7f,frown:1f,glanceX:0.85f,lidAngle:0.8f), new PipFacePose(happy:0.1f,lids:0.35f,glanceX:0.65f,blush:0.4f,lidAngle:0.5f,mouthScale:0.68f) }, // Grumpy
            { new PipFacePose(lids:0.15f,glanceX:-0.55f,glanceY:-0.55f,blush:0.55f,mouthScale:0.72f), new PipFacePose(lids:0.35f,glanceX:-0.9f,glanceY:-0.7f,blush:0.9f,mouthScale:0.65f), new PipFacePose(happy:0.3f,glanceX:0.25f,glanceY:0.15f,blush:0.75f,mouthScale:0.8f) }, // Shy
            { new PipFacePose(happy:0.75f,highlightStyle:1f,mouthScale:1.15f), new PipFacePose(happy:1f,blush:0.2f,highlightStyle:1f,mouthScale:1.25f), new PipFacePose(happy:0.6f,glanceX:0.15f,blush:0.15f,wink:1f,highlightStyle:1f,mouthScale:1.15f) }, // Show-off
            { new PipFacePose(lids:0.3f,glanceY:0.85f,mouthScale:0.65f), new PipFacePose(surprise:0.3f,lids:0.15f,glanceY:1f,mouthOpen:1f,mouthScale:0.7f), new PipFacePose(happy:0.15f,lids:0.2f,glanceY:1f,highlightStyle:2f,mouthScale:0.7f) }, // Dreamy
            { new PipFacePose(lids:0.8f,lidAngle:-0.65f,mouthScale:0.65f), new PipFacePose(surprise:0.65f,lids:0.05f,lidAngle:-0.15f,mouthOpen:1f), new PipFacePose(blink:1f,lids:0.8f,lidAngle:-0.65f,sleepCurve:1f,mouthScale:0.65f) }, // Sleepy
            { new PipFacePose(happy:0.35f,lids:0.28f,glanceX:0.85f,lidAngle:0.65f,mouthTilt:0.8f), new PipFacePose(happy:0.65f,lids:0.4f,glanceX:1f,blush:0.1f,lidAngle:0.75f,mouthTilt:0.85f), new PipFacePose(happy:0.35f,lids:0.2f,glanceX:0.85f,wink:1f,lidAngle:0.55f,mouthTilt:0.8f) }, // Mischief
            { new PipFacePose(happy:0.06f,lids:0.2f,mouthScale:0.9f), new PipFacePose(happy:0.28f,lids:0.2f,blush:0.12f,mouthScale:0.95f), new PipFacePose(blink:1f,happy:0.18f,lids:0.2f,mouthScale:0.9f) }, // Calm
        };
        public static PipFacePose Pose(PipTemperament temperament,int state)
        {
            if((int)temperament<0||(int)temperament>7)temperament=PipTemperament.Sunny;
            return poses[(int)temperament,Math.Max(0,Math.Min(2,state))];
        }
        public static PipFacePose Reaction(PipTemperament temperament,float progress)
        {
            var rest=Pose(temperament,0);progress=Numeric.FiniteOr(progress,0);
            if(progress<=0||progress>=1)return rest;
            if(progress<.30f)return PipFacePose.Blend(rest,Pose(temperament,1),progress/.30f);
            if(progress<.50f)return Pose(temperament,1);
            if(progress<.72f)return PipFacePose.Blend(Pose(temperament,1),Pose(temperament,2),(progress-.50f)/.22f);
            if(progress<.84f)return Pose(temperament,2);
            return PipFacePose.Blend(Pose(temperament,2),rest,(progress-.84f)/.16f);
        }
        public static PipTemperament TestTemperament(int tapIndex) => (PipTemperament)((tapIndex%8+8)%8);
    }
}
