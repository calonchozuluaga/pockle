using System;
using Pockle.Core;
internal static class ToyOrbitChecks
{
    private static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
    public static void Run()
    {
        var a=new ToyOrbit();a.Drag(720,540,.016f);
        Check(a.Yaw==720&&a.Pitch==540,"Both axes must turn beyond 360 degrees.");
        Check(a.YawSpeed==480,"Fling speed must be bounded.");
        a.Step(.016f,true,false);Check(a.Yaw==720,"Held view must not drift.");
        a.Step(.1f,false,false);Check(a.Yaw>720&&a.YawSpeed<480,"Released view must ease out.");
        float yaw=a.Yaw;a.Step(.1f,false,true);Check(a.Yaw==yaw&&a.YawSpeed==0,"Calm must stop inertia immediately.");
        a.SetFrame(100,100,-100);Check(a.Zoom==1.8f&&a.PanX==.7f&&a.PanY==-.65f,"Frame must stay bounded.");
        a.SetFrame(float.NaN,0,0);Check(a.Zoom==1.8f,"Invalid frame ignored.");
        a.Drag(float.NaN,0,.1f);Check(a.Yaw==yaw,"Invalid drag ignored.");
        a.Reset();Check(a.Yaw==0&&a.Pitch==0&&a.PanX==0&&a.Zoom==1,"Reset all viewer state.");
        foreach(float crop in new[]{0f,.3f,.8f,1f})
        foreach(float angle in new[]{-10000f,-180f,0f,180f,10000f})
        {float centre=ToyOrbit.PanoramaCentre(angle,crop,.5f,.25f);Check(centre>=crop/2&&centre<=1-crop/2,"Pan cannot reveal an image edge.");}
        Check(Math.Abs(ToyOrbit.PanoramaCentre(180,.4f,.5f,.25f)-.65f)<.0001f,"Pan speed is quarter hidden width per half turn.");
        var b=new ToyOrbit();a.Drag(4,3,.016f);b.Drag(4,3,.016f);
        for(int i=0;i<60;i++)a.Step(1f/60,false,false);
        for(int i=0;i<30;i++)b.Step(1f/30,false,false);
        Check(Math.Abs(a.Yaw-b.Yaw)<.001f,"Inertia must be independent of frame rate.");
        foreach(float aspect in new[]{.3f,.5f,.75f,1f,2f,3f})
        {
            float soft=ToyOrbit.FrameSize(aspect,1,true),rigid=ToyOrbit.FrameSize(aspect,1,false);
            Check(soft>=1.6f&&soft<=rigid,"Soft framing keeps vertical reaction room and never shrinks the toy.");
            Check(Math.Abs(rigid-Math.Max(1.6f,1.5f/aspect))<.0001f,"Rigid framing must stay unchanged.");
            if(aspect<=.75f)Check(Math.Abs(rigid/soft-1.25f)<.0001f,"Portrait soft toys should be 25% larger.");
        }
        Check(ToyOrbit.FrameSize(float.NaN,float.NaN,true)==1.6f,"Invalid framing must stay finite.");
        Console.WriteLine("PASS: free toy orbit, bounded framing, Calm, reset and panorama checks.");
    }
}
