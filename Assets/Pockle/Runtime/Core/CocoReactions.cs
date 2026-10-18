using System;
namespace Pockle.Core
{
    public enum CocoAction { HandleHello, MarshmallowBounce, CreamWobble, HappyHop, CozySway }
    public readonly struct CocoActionPose
    {
        public readonly float Lift,Pitch,Yaw,Roll,Cream,Marshmallows;
        public readonly PeachExpressionPose Face;
        public CocoActionPose(float lift,float pitch,float yaw,float roll,float cream,float marshmallows,PeachExpressionPose face)
        {Lift=lift;Pitch=pitch;Yaw=yaw;Roll=roll;Cream=cream;Marshmallows=marshmallows;Face=face;}
    }
    public static class CocoReactions
    {
        public const float Duration=2;
        public static CocoAction Choose(int previous,int draw)=>(CocoAction)(int)PipReactions.Choose(previous,draw);
        public static CocoActionPose Sample(CocoAction action,float progress,bool calm)
        {
            if(!Enum.IsDefined(typeof(CocoAction),action)||float.IsNaN(progress)||float.IsInfinity(progress)||progress<=0||progress>=1)return default;
            float e=(float)Math.Pow(Math.Sin(progress*Math.PI),2),wave=(float)Math.Sin(progress*Math.PI*6)*e;
            if(calm)return new CocoActionPose(0,0,0,0,0,0,new PeachExpressionPose(0,.25f*e,0));
            switch(action)
            {
                case CocoAction.HandleHello:return new CocoActionPose(.025f*e,0,-14*e,-7*e,.15f*wave,0,new PeachExpressionPose(0,.8f*e,0));
                case CocoAction.MarshmallowBounce:return new CocoActionPose(0,0,0,0,.15f*wave,wave,new PeachExpressionPose(0,.25f*e,.7f*e));
                case CocoAction.CreamWobble:return new CocoActionPose(0,0,0,2*wave,wave,.15f*wave,new PeachExpressionPose(0,.8f*e,0));
                case CocoAction.HappyHop:return new CocoActionPose(.23f*e,0,0,0,.3f*wave,.45f*wave,new PeachExpressionPose(0,e,.35f*e));
                default:return new CocoActionPose(0,3*e,0,-4*e,.15f*wave,0,new PeachExpressionPose(Math.Min(1,e*1.4f),.3f*e,0));
            }
        }
    }
}
