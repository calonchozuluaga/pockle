using System;
namespace Pockle.Core
{
    public enum FirbAction { BranchWave, StarNod, OrnamentShimmy, HappyHop, SleepySway }
    public readonly struct FirbActionPose
    {
        public readonly float Lift,Pitch,Yaw,Roll,Branch,Star,Ornaments;
        public readonly PeachExpressionPose Face;
        public FirbActionPose(float lift,float pitch,float yaw,float roll,float branch,float star,float ornaments,PeachExpressionPose face)
        {Lift=lift;Pitch=pitch;Yaw=yaw;Roll=roll;Branch=branch;Star=star;Ornaments=ornaments;Face=face;}
    }
    public static class FirbReactions
    {
        public const float Duration=2;
        public static FirbAction Choose(int previous,int draw)=>(FirbAction)(int)PipReactions.Choose(previous,draw);
        public static FirbActionPose Sample(FirbAction action,float progress,bool calm)
        {
            if(!Enum.IsDefined(typeof(FirbAction),action)||float.IsNaN(progress)||float.IsInfinity(progress)||progress<=0||progress>=1)return default;
            float e=(float)Math.Pow(Math.Sin(progress*Math.PI),2),wave=(float)Math.Sin(progress*Math.PI*6)*e;
            if(calm)return new FirbActionPose(0,0,0,0,0,0,0,new PeachExpressionPose(0,.25f*e,0));
            switch(action)
            {
                case FirbAction.BranchWave:return new FirbActionPose(0,0,0,-2*e,e*(.65f+.35f*(float)Math.Sin(progress*Math.PI*8)),.2f*wave,0,new PeachExpressionPose(0,.8f*e,0));
                case FirbAction.StarNod:return new FirbActionPose(0,4*e,0,0,0,wave,0,new PeachExpressionPose(0,.2f*e,.65f*e));
                case FirbAction.OrnamentShimmy:return new FirbActionPose(0,0,7*wave,3*wave,.25f*wave,.3f*wave,wave,new PeachExpressionPose(0,.9f*e,0));
                case FirbAction.HappyHop:return new FirbActionPose(.24f*e,0,0,0,.45f*e,.4f*wave,.25f*wave,new PeachExpressionPose(0,e,.35f*e));
                default:return new FirbActionPose(0,0,0,-5*e,0,-.25f*e,0,new PeachExpressionPose(Math.Min(1,e*1.4f),.3f*e,0));
            }
        }
    }
}
