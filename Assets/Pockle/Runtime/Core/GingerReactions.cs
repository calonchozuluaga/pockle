using System;
namespace Pockle.Core
{
    public enum GingerAction { CookieWave, EarWiggle, ButtonDance, HappyHop, SweetBow }
    public readonly struct GingerActionPose
    {
        public readonly float Lift,Pitch,Yaw,Roll,Paw,Ears,Head;
        public readonly PeachExpressionPose Face;
        public GingerActionPose(float lift,float pitch,float yaw,float roll,float paw,float ears,float head,PeachExpressionPose face)
        {Lift=lift;Pitch=pitch;Yaw=yaw;Roll=roll;Paw=paw;Ears=ears;Head=head;Face=face;}
    }
    public static class GingerReactions
    {
        public const float Duration=2;
        public static GingerAction Choose(int previous,int draw)=>(GingerAction)(int)PipReactions.Choose(previous,draw);
        public static GingerActionPose Sample(GingerAction action,float progress,bool calm)
        {
            if(!Enum.IsDefined(typeof(GingerAction),action)||float.IsNaN(progress)||float.IsInfinity(progress)||progress<=0||progress>=1)return default;
            float e=(float)Math.Pow(Math.Sin(progress*Math.PI),2),wave=(float)Math.Sin(progress*Math.PI*6)*e;
            if(calm)return new GingerActionPose(0,0,0,0,0,0,0,new PeachExpressionPose(0,.25f*e,0));
            switch(action)
            {
                case GingerAction.CookieWave:return new GingerActionPose(0,0,0,-2*e,e*(.72f+.28f*(float)Math.Sin(progress*Math.PI*8)),.15f*wave,0,new PeachExpressionPose(0,.8f*e,0));
                case GingerAction.EarWiggle:return new GingerActionPose(0,0,0,0,0,wave,0,new PeachExpressionPose(0,.2f*e,.7f*e));
                case GingerAction.ButtonDance:return new GingerActionPose(0,0,9*wave,4*wave,.3f*wave,.35f*wave,.15f*wave,new PeachExpressionPose(0,.9f*e,0));
                case GingerAction.HappyHop:return new GingerActionPose(.23f*e,0,0,0,.4f*e,.4f*wave,0,new PeachExpressionPose(0,e,.35f*e));
                default:return new GingerActionPose(0,0,0,0,0,.15f*e,e,new PeachExpressionPose(Math.Min(1,e*1.3f),.5f*e,0));
            }
        }
    }
}
