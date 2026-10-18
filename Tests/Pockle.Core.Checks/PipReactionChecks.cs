using System;
using Pockle.Core;
internal static class PipReactionChecks
{
    private static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
    public static void Run()
    {
        for(int previous=-1;previous<5;previous++)
        {
            var seen=new bool[5];
            for(int draw=0;draw<(previous<0?5:4);draw++)
            {
                int selected=(int)PipReactions.Choose(previous,draw);
                Check(selected!=previous,"Consecutive reactions cannot repeat.");
                Check(!seen[selected],"Each allowed outcome has one equal-probability draw.");
                seen[selected]=true;
            }
            for(int action=0;action<5;action++)Check(seen[action]==(action!=previous),"All allowed actions remain selectable.");
        }
        foreach(PipAction action in Enum.GetValues(typeof(PipAction)))
        {
            foreach(bool firm in new[]{false,true})
            for(int i=0;i<=100;i++)
            {
                var a=PipReactions.Sample(action,i/100f,firm,false);
                Check(!float.IsNaN(a.Lift)&&Math.Abs(a.Lift)<.35f,"Bounded reaction height.");
                Check(Math.Abs(a.Roll)<=18&&Math.Abs(a.Pitch)<=24,"Bounded rotation.");
                if(firm)Check(a.Squash==0,"Firm toys never squash in reactions.");
                var calm=PipReactions.Sample(action,i/100f,firm,true);
                Check(calm.Lift==0&&calm.Pitch==0&&calm.Yaw==0&&calm.Roll==0&&calm.Squash==0&&calm.Face.Blink==0,"Calm has no body motion or blink.");
            }
            foreach(float t in new[]{-1f,0,1,2,float.NaN,float.PositiveInfinity})
            {var a=PipReactions.Sample(action,t,false,false);Check(a.Lift==0&&a.Roll==0&&a.Face.Happy==0,"Rest and invalid-time samples neutral.");}
        }
        Check(PipReactions.Sample(PipAction.Hop,.5f,false,false).Lift>.3f,"Hop visible.");
        Check(PipReactions.Sample(PipAction.CuriousTilt,.5f,true,false).Roll>15,"Tilt distinct.");
        Check(PipReactions.Sample(PipAction.HappyWiggle,.5f,true,false).Face.Happy>.9f,"Happy face visible.");
        Check(PipReactions.Sample(PipAction.Peekaboo,.5f,true,false).Face.Blink==1,"Peekaboo closes eyes.");
        Check(PipReactions.Sample(PipAction.ThankYouBow,.5f,true,false).Pitch>20,"Bow distinct.");
        Console.WriteLine("PASS: five Pip reactions, rigid compliance, bounded timing and Calm checks.");
    }
}
