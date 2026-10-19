using System;
namespace Pockle.Core
{
    public enum WallyAction { BreachJump, TailSplash, FlipperWave, CuriousRoll, SleepyBob }
    public readonly struct WallyActionPose
    {
        public readonly float Lift,Pitch,Yaw,Roll,Tail,Flipper;
        public readonly PeachExpressionPose Face;
        public WallyActionPose(float lift,float pitch,float yaw,float roll,float tail,float flipper,PeachExpressionPose face)
        { Lift=lift;Pitch=pitch;Yaw=yaw;Roll=roll;Tail=tail;Flipper=flipper;Face=face; }
    }
    public static class WallyReactions
    {
        public const float Duration=2.2f;
        public static WallyAction Choose(int previous,int draw)=>(WallyAction)(int)PipReactions.Choose(previous,draw);
        public static WallyActionPose Sample(WallyAction action,float progress,bool calm)
        {
            if(!Enum.IsDefined(typeof(WallyAction),action)||float.IsNaN(progress)||float.IsInfinity(progress)||progress<=0||progress>=1)return default;
            float e=(float)Math.Pow(Math.Sin(progress*Math.PI),2),wave=(float)Math.Sin(progress*Math.PI*6)*e;
            if(calm)return new WallyActionPose(0,0,0,0,0,0,new PeachExpressionPose(0,.25f*e,0));
            switch(action)
            {
                case WallyAction.BreachJump:return new WallyActionPose(.36f*e,-16*e,0,5*wave,.4f*wave,.4f*e,new PeachExpressionPose(0,.55f*e,.7f*e));
                case WallyAction.TailSplash:return new WallyActionPose(.035f*e,3*wave,0,0,wave,0,new PeachExpressionPose(0,.9f*e,.2f*e));
                case WallyAction.FlipperWave:return new WallyActionPose(0,0,0,-3*e,.12f*wave,e*(.7f+.3f*(float)Math.Sin(progress*Math.PI*8)),new PeachExpressionPose(0,.85f*e,0));
                case WallyAction.CuriousRoll:return new WallyActionPose(.025f*e,0,12*e,18*e,.18f*wave,.3f*e,new PeachExpressionPose(0,.15f*e,.55f*e));
                default:return new WallyActionPose(.045f*e,7*e,0,3*wave,.15f*wave,0,new PeachExpressionPose(Math.Min(1,1.25f*e),.35f*e,0));
            }
        }
    }
}
