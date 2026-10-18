using System;
using Pockle.Core;
internal static class GingerReactionChecks
{
    private static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
    public static void Run()
    {
        foreach(GingerAction action in Enum.GetValues(typeof(GingerAction)))
        {
            for(int i=0;i<=100;i++)
            {
                var p=GingerReactions.Sample(action,i/100f,false);Check(p.Lift>=0&&p.Lift<=.241f&&Math.Abs(p.Paw)<=1.001f&&Math.Abs(p.Ears)<=1.001f&&Math.Abs(p.Head)<=1.001f,"Bounded local Ginger articulation.");
                var c=GingerReactions.Sample(action,i/100f,true);Check(c.Lift==0&&c.Pitch==0&&c.Yaw==0&&c.Roll==0&&c.Paw==0&&c.Ears==0&&c.Head==0&&c.Face.Blink==0,"Motionless Calm alternative.");
            }
            foreach(float t in new[]{-1f,0,1,float.NaN,float.PositiveInfinity})
            {var p=GingerReactions.Sample(action,t,false);Check(p.Lift==0&&p.Paw==0&&p.Ears==0&&p.Face.Happy==0,"Invalid time and endpoints are neutral.");}
        }
        for(int last=-1;last<5;last++)for(int draw=0;draw<(last<0?5:4);draw++)Check((int)GingerReactions.Choose(last,draw)!=last,"No consecutive Ginger repeat.");
        Check(GingerReactions.Sample(GingerAction.CookieWave,.5f,false).Paw>.7f,"Cookie wave articulates the paw.");
        Check(GingerReactions.Sample(GingerAction.EarWiggle,.25f,false).Ears!=0,"Ear wiggle has independent motion.");
        Check(GingerReactions.Sample(GingerAction.SweetBow,.5f,false).Head>.99f,"Bow moves the head instead of tipping the feet.");
        Check(GingerReactions.Sample(GingerAction.HappyHop,.5f,false).Lift>.22f,"Happy hop leaves the ground.");
        Console.WriteLine("PASS: five Ginger reactions, bounded paw/ear/head and facial poses, Calm and random selection.");
    }
}
