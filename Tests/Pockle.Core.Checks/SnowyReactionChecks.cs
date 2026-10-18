using System;
using Pockle.Core;
internal static class SnowyReactionChecks
{
    private static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
    public static void Run()
    {
        foreach(SnowyAction action in Enum.GetValues(typeof(SnowyAction)))
        {
            for(int i=0;i<=100;i++)
            {
                var p=SnowyReactions.Sample(action,i/100f,false);Check(p.Lift>=0&&p.Lift<=.241f&&Math.Abs(p.Mitten)<=1.001f&&Math.Abs(p.Pom)<=1.001f&&Math.Abs(p.Head)<=1.001f,"Bounded local Snowy articulation.");
                var c=SnowyReactions.Sample(action,i/100f,true);Check(c.Lift==0&&c.Pitch==0&&c.Yaw==0&&c.Roll==0&&c.Mitten==0&&c.Pom==0&&c.Head==0&&c.Face.Blink==0,"Motionless Calm alternative.");
            }
            foreach(float t in new[]{-1f,0,1,float.NaN,float.PositiveInfinity})
            {var p=SnowyReactions.Sample(action,t,false);Check(p.Lift==0&&p.Mitten==0&&p.Pom==0&&p.Face.Happy==0,"Invalid time and endpoints are neutral.");}
        }
        for(int last=-1;last<5;last++)for(int draw=0;draw<(last<0?5:4);draw++)Check((int)SnowyReactions.Choose(last,draw)!=last,"No consecutive Snowy repeat.");
        Check(SnowyReactions.Sample(SnowyAction.MittenWave,.5f,false).Mitten>.69f,"Greeting articulates the mitten.");
        Check(SnowyReactions.Sample(SnowyAction.PomWobble,.25f,false).Pom!=0,"Pom has independent movement.");
        Check(SnowyReactions.Sample(SnowyAction.CozyNod,.5f,false).Head>.99f,"Nod moves the head with planted feet.");
        Check(SnowyReactions.Sample(SnowyAction.SnowHop,.5f,false).Lift>.21f,"Snow hop leaves the ground.");
        Console.WriteLine("PASS: five Snowy reactions, bounded mitten/pom/head and facial poses, Calm and random selection.");
    }
}
