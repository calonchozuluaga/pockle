using System;
using Pockle.Core;
internal static class AxolotlReactionChecks
{
    private static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
    public static void Run()
    {
        foreach(AxolotlAction action in Enum.GetValues(typeof(AxolotlAction)))
        {
            for(int i=0;i<=100;i++)
            {
                var p=AxolotlReactions.Sample(action,i/100f,false);Check(p.Lift>=0&&p.Lift<=.221f&&Math.Abs(p.Arm)<=1.001f&&Math.Abs(p.Gills)<=1.001f&&Math.Abs(p.Tail)<=1.001f,"Bounded local Axolotl articulation.");
                var c=AxolotlReactions.Sample(action,i/100f,true);Check(c.Lift==0&&c.Pitch==0&&c.Yaw==0&&c.Roll==0&&c.Arm==0&&c.Gills==0&&c.Tail==0&&c.Face.Blink==0,"Motionless Calm alternative.");
            }
            foreach(float t in new[]{-1f,0,1,float.NaN,float.PositiveInfinity})
            {var p=AxolotlReactions.Sample(action,t,false);Check(p.Lift==0&&p.Arm==0&&p.Gills==0&&p.Face.Happy==0,"Invalid time and endpoints are neutral.");}
        }
        for(int last=-1;last<5;last++)for(int draw=0;draw<(last<0?5:4);draw++)Check((int)AxolotlReactions.Choose(last,draw)!=last,"No consecutive Axolotl repeat.");
        Console.WriteLine("PASS: five Axolotl reactions, bounded gill/arm/tail and facial poses, Calm and random selection.");
    }
}
