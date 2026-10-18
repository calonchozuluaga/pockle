using System;
namespace Pockle.Core
{
    public enum AxolotlAction { HelloWave, GillFlutter, WiggleDance, HappyBounce, SleepySway }
    public readonly struct AxolotlActionPose
    {
        public readonly float Lift,Pitch,Yaw,Roll,Arm,Gills,Tail;
        public readonly PeachExpressionPose Face;
        public AxolotlActionPose(float lift,float pitch,float yaw,float roll,float arm,float gills,float tail,PeachExpressionPose face)
        {Lift=lift;Pitch=pitch;Yaw=yaw;Roll=roll;Arm=arm;Gills=gills;Tail=tail;Face=face;}
    }
    public static class AxolotlReactions
    {
        public const float Duration=2;
        public static AxolotlAction Choose(int previous,int draw)=>(AxolotlAction)(int)PipReactions.Choose(previous,draw);
        public static AxolotlActionPose Sample(AxolotlAction action,float progress,bool calm)
        {
            if(!Enum.IsDefined(typeof(AxolotlAction),action)||float.IsNaN(progress)||float.IsInfinity(progress)||progress<=0||progress>=1)return default;
            float e=(float)Math.Pow(Math.Sin(progress*Math.PI),2),wave=(float)Math.Sin(progress*Math.PI*6)*e;
            if(calm)return new AxolotlActionPose(0,0,0,0,0,0,0,new PeachExpressionPose(0,.25f*e,0));
            switch(action)
            {
                case AxolotlAction.HelloWave:return new AxolotlActionPose(0,0,0,-3*e,e*(.7f+.3f*(float)Math.Sin(progress*Math.PI*8)),.25f*e,0,new PeachExpressionPose(0,.8f*e,0));
                case AxolotlAction.GillFlutter:return new AxolotlActionPose(0,0,0,0,0,wave,.15f*wave,new PeachExpressionPose(0,.15f*e,.6f*e));
                case AxolotlAction.WiggleDance:return new AxolotlActionPose(0,0,9*wave,5*wave,.35f*wave,.7f*wave,wave,new PeachExpressionPose(0,.8f*e,0));
                case AxolotlAction.HappyBounce:return new AxolotlActionPose(.22f*e,0,0,0,.6f*e,.6f*e,.35f*wave,new PeachExpressionPose(0,e,.4f*e));
                default:return new AxolotlActionPose(0,0,0,-5*e,0,-.25f*e,.2f*wave,new PeachExpressionPose(Math.Min(1,e*1.4f),.25f*e,0));
            }
        }
    }
}
