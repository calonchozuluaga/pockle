using System;
using Pockle.Core;
internal static class CocoReactionChecks
{
    private static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
    public static void Run()
    {
        foreach(CocoAction action in Enum.GetValues(typeof(CocoAction)))
        {
            for(int i=0;i<=100;i++)
            {
                var p=CocoReactions.Sample(action,i/100f,false);Check(p.Lift>=0&&p.Lift<=.241f&&Math.Abs(p.Cream)<=1.001f&&Math.Abs(p.Marshmallows)<=1.001f,"Bounded local Coco articulation.");
                var c=CocoReactions.Sample(action,i/100f,true);Check(c.Lift==0&&c.Pitch==0&&c.Yaw==0&&c.Roll==0&&c.Cream==0&&c.Marshmallows==0&&c.Face.Blink==0,"Motionless Calm alternative.");
            }
            foreach(float t in new[]{-1f,0,1,float.NaN,float.PositiveInfinity})
            {var p=CocoReactions.Sample(action,t,false);Check(p.Lift==0&&p.Cream==0&&p.Marshmallows==0&&p.Face.Happy==0,"Invalid time and endpoints are neutral.");}
        }
        for(int last=-1;last<5;last++)for(int draw=0;draw<(last<0?5:4);draw++)Check((int)CocoReactions.Choose(last,draw)!=last,"No consecutive Coco repeat.");
        Check(CocoReactions.Sample(CocoAction.HandleHello,.5f,false).Yaw<-13,"Greeting presents the rigid handle.");
        Check(CocoReactions.Sample(CocoAction.MarshmallowBounce,.25f,false).Marshmallows!=0,"Marshmallow action has independent motion.");
        Check(CocoReactions.Sample(CocoAction.CreamWobble,.25f,false).Cream!=0,"Cream action has independent motion.");
        Check(CocoReactions.Sample(CocoAction.HappyHop,.5f,false).Lift>.22f,"Happy hop leaves the ground.");
        Console.WriteLine("PASS: five Coco reactions, bounded cream/marshmallow and facial poses, Calm and random selection.");
    }
}
