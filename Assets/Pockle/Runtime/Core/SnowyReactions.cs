using System;
namespace Pockle.Core
{
    public enum SnowyAction { MittenWave, PomWobble, FrostyShuffle, SnowHop, CozyNod }
    public readonly struct SnowyActionPose
    {
        public readonly float Lift,Pitch,Yaw,Roll,Mitten,Pom,Head;
        public readonly PeachExpressionPose Face;
        public SnowyActionPose(float lift,float pitch,float yaw,float roll,float mitten,float pom,float head,PeachExpressionPose face)
        {Lift=lift;Pitch=pitch;Yaw=yaw;Roll=roll;Mitten=mitten;Pom=pom;Head=head;Face=face;}
    }
    public static class SnowyReactions
    {
        public const float Duration=2;
        public static SnowyAction Choose(int previous,int draw)=>(SnowyAction)(int)PipReactions.Choose(previous,draw);
        public static SnowyActionPose Sample(SnowyAction action,float progress,bool calm)
        {
            if(!Enum.IsDefined(typeof(SnowyAction),action)||float.IsNaN(progress)||float.IsInfinity(progress)||progress<=0||progress>=1)return default;
            float e=(float)Math.Pow(Math.Sin(progress*Math.PI),2),wave=(float)Math.Sin(progress*Math.PI*6)*e;
            if(calm)return new SnowyActionPose(0,0,0,0,0,0,0,new PeachExpressionPose(0,.25f*e,0));
            switch(action)
            {
                case SnowyAction.MittenWave:return new SnowyActionPose(0,0,0,-2*e,e*(.7f+.3f*(float)Math.Sin(progress*Math.PI*8)),.25f*wave,0,new PeachExpressionPose(0,.8f*e,0));
                case SnowyAction.PomWobble:return new SnowyActionPose(0,0,0,0,0,wave,0,new PeachExpressionPose(0,.15f*e,.7f*e));
                case SnowyAction.FrostyShuffle:return new SnowyActionPose(.025f*Math.Abs(wave),0,12*wave,4*wave,.25f*wave,.6f*wave,.15f*wave,new PeachExpressionPose(0,.9f*e,0));
                case SnowyAction.SnowHop:return new SnowyActionPose(.22f*e,0,0,0,.5f*e,.5f*wave,0,new PeachExpressionPose(0,e,.4f*e));
                default:return new SnowyActionPose(0,0,0,0,0,.15f*wave,e,new PeachExpressionPose(Math.Min(1,e*1.3f),.4f*e,0));
            }
        }
    }
}
