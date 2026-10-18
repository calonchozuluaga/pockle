using System;
using Pockle.Core;
internal static class FirbReactionChecks
{
    private static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
    public static void Run()
    {
        foreach(FirbAction action in Enum.GetValues(typeof(FirbAction)))
        {
            for(int i=0;i<=100;i++)
            {
                var p=FirbReactions.Sample(action,i/100f,false);Check(p.Lift>=0&&p.Lift<=.241f&&Math.Abs(p.Branch)<=1.001f&&Math.Abs(p.Star)<=1.001f&&Math.Abs(p.Ornaments)<=1.001f,"Bounded local Firb articulation.");
                var c=FirbReactions.Sample(action,i/100f,true);Check(c.Lift==0&&c.Pitch==0&&c.Yaw==0&&c.Roll==0&&c.Branch==0&&c.Star==0&&c.Ornaments==0&&c.Face.Blink==0,"Motionless Calm alternative.");
            }
            foreach(float t in new[]{-1f,0,1,float.NaN,float.PositiveInfinity})
            {var p=FirbReactions.Sample(action,t,false);Check(p.Lift==0&&p.Branch==0&&p.Star==0&&p.Face.Happy==0,"Invalid time and endpoints are neutral.");}
        }
        for(int last=-1;last<5;last++)for(int draw=0;draw<(last<0?5:4);draw++)Check((int)FirbReactions.Choose(last,draw)!=last,"No consecutive Firb repeat.");
        Check(FirbReactions.Sample(FirbAction.BranchWave,.5f,false).Branch>.6f,"Branch greeting is distinct.");
        Check(FirbReactions.Sample(FirbAction.StarNod,.25f,false).Star!=0,"Star nod has independent movement.");
        Check(FirbReactions.Sample(FirbAction.OrnamentShimmy,.25f,false).Ornaments!=0,"Ornament shimmy has independent movement.");
        Check(FirbReactions.Sample(FirbAction.HappyHop,.5f,false).Lift>.23f,"Happy hop leaves the ground.");
        Console.WriteLine("PASS: five Firb reactions, bounded branch/star/ornament and facial poses, Calm and random selection.");
    }
}
