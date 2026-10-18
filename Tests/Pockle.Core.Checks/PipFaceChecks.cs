using System;
using Pockle.Core;
internal static class PipFaceChecks
{
    private static void Check(bool ok,string text){if(!ok)throw new Exception(text);}
    public static void Run()
    {
        var bad=new PipFacePose(blink:float.NaN,glanceX:float.PositiveInfinity,lids:9,lidAngle:-9,mouthScale:float.NaN,highlightStyle:9);
        Check(bad.Blink==0&&bad.GlanceX==0&&bad.Lids==1&&bad.LidAngle==-1&&bad.MouthScale==1&&bad.HighlightStyle==2,"Face controls must sanitize invalid input");
        foreach(PipTemperament style in Enum.GetValues(typeof(PipTemperament)))
        {
            var rest=PipFaces.Pose(style,0);
            foreach(float t in new[]{-1f,0,1,2,float.NaN,float.PositiveInfinity})Check(PipFaces.Reaction(style,t).Equals(rest),"Invalid/end timing must return exact rest");
            for(int i=0;i<=1000;i++)
            {
                var p=PipFaces.Reaction(style,i/1000f);
                Check(float.IsFinite(p.Blink)&&p.Blink>=0&&p.Blink<=1&&float.IsFinite(p.GlanceX)&&Math.Abs(p.GlanceX)<=1&&float.IsFinite(p.MouthScale)&&p.MouthScale>=.5&&p.MouthScale<=1.5,"Face interpolation left bounds");
            }
            Check(PipFaces.Reaction(style,.4f).Equals(PipFaces.Pose(style,1)),"Reaction must hold approved tap face");
            Check(PipFaces.Reaction(style,.8f).Equals(PipFaces.Pose(style,2)),"Reaction must hold approved signature face");
        }
        for(int i=0;i<32;i++)Check((int)PipFaces.TestTemperament(i)==i%8,"Owner test must cycle all eight in order");
        Check(PipFaces.Pose(PipTemperament.Grumpy,2).Lids>=.35f,"Reluctant Grumpy smile must keep lids");
        Check(PipFaces.Pose(PipTemperament.Mischief,0).MouthTilt>0&&PipFaces.Pose(PipTemperament.Mischief,0).LidAngle>0,"Mischief needs a crooked grin and inward lids");
        Check(PipFaces.Pose(PipTemperament.Sleepy,2).SleepCurve==1&&PipFaces.Pose(PipTemperament.ShowOff,2).Wink==1,"Sleep and wink closures must differ");
        Check(PipFaces.Pose(PipTemperament.Dreamy,1).MouthOpen==1&&PipFaces.Pose(PipTemperament.Sleepy,1).MouthOpen==1,"Surprise mouths must be round");
        Console.WriteLine("PASS: eight Pip face sets, bounded timing/controls, exact reset, signature differences and owner test cycle.");
    }
}
