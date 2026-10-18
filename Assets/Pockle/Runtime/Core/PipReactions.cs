using System;
namespace Pockle.Core
{
    public enum PipAction { Hop, CuriousTilt, HappyWiggle, Peekaboo, ThankYouBow }
    public readonly struct PipActionPose
    {
        public readonly float Lift, Pitch, Yaw, Roll, Squash;
        public readonly PeachExpressionPose Face;
        public PipActionPose(float lift,float pitch,float yaw,float roll,float squash,PeachExpressionPose face)
        { Lift=lift;Pitch=pitch;Yaw=yaw;Roll=roll;Squash=squash;Face=face; }
    }
    /// <summary>Five authored, finite local reactions; every action returns exactly to rest.</summary>
    public static class PipReactions
    {
        public const float Duration=1.8f;
        /// <summary>Map a uniform draw to all five actions initially, then the four other actions.</summary>
        public static PipAction Choose(int previous,int draw)
        {
            if(previous < -1 || previous > 4)throw new ArgumentOutOfRangeException(nameof(previous));
            int count=previous < 0 ? 5 : 4;
            if(draw < 0 || draw >= count)throw new ArgumentOutOfRangeException(nameof(draw));
            return (PipAction)(previous>=0&&draw>=previous ? draw+1 : draw);
        }
        public static PipActionPose Sample(PipAction action,float progress,bool firm,bool calm)
        {
            if(!Enum.IsDefined(typeof(PipAction),action)||float.IsNaN(progress)||float.IsInfinity(progress)||progress<=0||progress>=1)return default;
            float t=progress, e=(float)Math.Pow(Math.Sin(t*Math.PI),2);
            if(calm)return new PipActionPose(0,0,0,0,0,new PeachExpressionPose(0,.35f*e,0));
            float wave=(float)Math.Sin(t*Math.PI*6)*e;
            switch(action)
            {
                case PipAction.Hop:return new PipActionPose(.34f*e,0,0,0,firm?0:-.055f*e,new PeachExpressionPose(0,.4f*e,.75f*e));
                case PipAction.CuriousTilt:return new PipActionPose(0,0,10*e,18*e,0,new PeachExpressionPose(0,0,.65f*e));
                case PipAction.HappyWiggle:return new PipActionPose(.025f*e,0,8*wave,12*wave,firm?0:.035f*e,new PeachExpressionPose(0,e,0));
                case PipAction.Peekaboo:return new PipActionPose(-.035f*e,5*e,0,0,firm?0:.025f*e,new PeachExpressionPose(Math.Min(1,e*1.35f),.5f*e,0));
                default:return new PipActionPose(-.045f*e,24*e,0,0,0,new PeachExpressionPose(0,.75f*e,0));
            }
        }
    }
}
