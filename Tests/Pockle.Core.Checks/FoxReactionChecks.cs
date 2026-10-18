using System;
using Pockle.Core;
internal static class FoxReactionChecks
{
    private static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
    public static void Run()
    {
        foreach(FoxAction action in Enum.GetValues(typeof(FoxAction)))
        {
            for(int i=0;i<=100;i++)
            {
                var p=FoxReactions.Sample(action,i/100f,false);
                Check(p.Lift>=-.0251f&&p.Lift<=.321f&&Math.Abs(p.Tail)<=28.01f,"Fox pose bounded.");
                var calm=FoxReactions.Sample(action,i/100f,true);
                Check(calm.Lift==0&&calm.Pitch==0&&calm.HeadRoll==0&&calm.HeadPitch==0&&calm.Tail==0&&calm.Ears==0&&calm.Blink==0,"Calm removes articulation/body motion.");
                var mesh=new FoxDeformation(p);
                foreach(var point in new[]{new Point3(-.65f,-.6f,.65f),new Point3(0,-1,-.2f),new Point3(-.18f,.05f,-.54f),new Point3(.38f,.8f,0)})
                {var d=mesh.Apply(point);Check(!float.IsNaN(d.X)&&!float.IsNaN(d.Y)&&!float.IsNaN(d.Z),"Fox deformation finite.");}
            }
            foreach(float time in new[]{0f,1f,float.NaN,float.PositiveInfinity})
            {
                var p=FoxReactions.Sample(action,time,false);Check(p.Lift==0&&p.Tail==0&&p.Blink==0,"Exact rest endpoints.");
            }
        }
        for(int previous=-1;previous<5;previous++)for(int draw=0;draw<(previous<0?5:4);draw++)
            Check((int)FoxReactions.Choose(previous,draw)!=previous,"Fox tap cannot repeat.");
        Check(FoxDeformation.TailWeight(new Point3(-.65f,-.6f,.65f))>.9f,"Tail tip selected.");
        Check(FoxDeformation.TailWeight(new Point3(-.18f,.05f,-.54f))==0,"Face excluded from tail.");
        var foot=new Point3(.25f,-1,-.25f);var pose=FoxReactions.Sample(FoxAction.TailWag,.4f,false);var moved=new FoxDeformation(pose).Apply(foot);
        Check(foot.X==moved.X&&foot.Y==moved.Y&&foot.Z==moved.Z,"Tail wag keeps feet rigid.");
        Console.WriteLine("PASS: five Fox actions, no-repeat selection, pinned tail/feet, facial bounds and Calm.");
    }
}
