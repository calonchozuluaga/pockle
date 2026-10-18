using System;
namespace Pockle.Core
{
    public enum WinterTrioAction { Greeting, FeatureWiggle, Shuffle, Jump, Bow }
    public readonly struct WinterTrioPose
    {
        public readonly float Lift,Pitch,Yaw,Roll,Wave,Feature,Nod;
        public readonly PeachExpressionPose Face;
        public WinterTrioPose(float lift,float pitch,float yaw,float roll,float wave,float feature,float nod,PeachExpressionPose face)
        {Lift=lift;Pitch=pitch;Yaw=yaw;Roll=roll;Wave=wave;Feature=feature;Nod=nod;Face=face;}
    }
    /// <summary>Character-specific local actions, independent of finish and personality paint.</summary>
    public static class WinterTrioReactions
    {
        public const float Duration=2f;
        public static WinterTrioAction Choose(int previous,int draw)=>(WinterTrioAction)(int)PipReactions.Choose(previous,draw);
        public static string Name(string character,WinterTrioAction action)
        {
            if(!HolidayCatalog.IsWinterTrio(character)||!Enum.IsDefined(typeof(WinterTrioAction),action))return "";
            string[] names=character=="reiny"?new[]{"Hoof hello","Antler nod","Happy prance","Reindeer leap","Shy bow"}:
                character=="guin"?new[]{"Flipper hello","Earmuff wiggle","Cozy waddle","Happy hop","Cozy dip"}:
                new[]{"Little wave","Holly wobble","Pudding jiggle","Sweet hop","Thank-you bow"};
            return names[(int)action];
        }
        public static WinterTrioPose Sample(string character,WinterTrioAction action,float progress,bool calm)
        {
            if(!HolidayCatalog.IsWinterTrio(character)||!Enum.IsDefined(typeof(WinterTrioAction),action)||float.IsNaN(progress)||float.IsInfinity(progress)||progress<=0||progress>=1)return default;
            float e=(float)Math.Pow(Math.Sin(progress*Math.PI),2),wave=(float)Math.Sin(progress*Math.PI*6)*e;
            if(calm)return new WinterTrioPose(0,0,0,0,0,0,0,new PeachExpressionPose(0,.25f*e,0));
            bool deer=character=="reiny",penguin=character=="guin";
            switch(action)
            {
                case WinterTrioAction.Greeting:return new WinterTrioPose(0,0,0,(deer?-2:penguin?3:1)*e,e*(.7f+.3f*(float)Math.Sin(progress*Math.PI*8)),.15f*wave,0,new PeachExpressionPose(0,.8f*e,0));
                case WinterTrioAction.FeatureWiggle:return new WinterTrioPose(0,deer?2*wave:0,0,0,0,wave,0,new PeachExpressionPose(0,.2f*e,.7f*e));
                case WinterTrioAction.Shuffle:return new WinterTrioPose((deer?.045f:penguin?.015f:.025f)*Math.Abs(wave),0,(deer?9:penguin?6:3)*wave,(deer?3:penguin?7:4)*wave,.20f*wave,.45f*wave,0,new PeachExpressionPose(0,.9f*e,0));
                case WinterTrioAction.Jump:return new WinterTrioPose((deer?.26f:penguin?.16f:.21f)*e,0,0,0,.5f*e,.45f*wave,0,new PeachExpressionPose(0,e,.4f*e));
                default:return new WinterTrioPose(0,0,0,0,0,.1f*wave,e,new PeachExpressionPose(Math.Min(1,e*1.3f),.4f*e,0));
            }
        }
    }
}
