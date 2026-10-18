using System;
using Pockle.Core;
internal static class SoftToyGestureChecks
{
    private static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
    public static void Run()
    {
        foreach(string finish in new[]{"clear-jelly","pearl-jelly","confetti-gel","mochi-foam","velvet-flock","boucle-plush","glow-jelly"})
            Check(SoftToyGesture.UsesDeformation(MaterialHandling.ForFinish(finish)),"Soft finish must deform instead of zoom.");
        foreach(string finish in new[]{"matte-vinyl","gloss-vinyl","coated-metallic"})
            Check(!SoftToyGesture.UsesDeformation(MaterialHandling.ForFinish(finish)),"Rigid finish must retain zoom.");
        Check(SoftToyGesture.Sample(.5f,0).Pinch<0,"Pinch in squeezes.");
        Check(SoftToyGesture.Sample(1.5f,0).Pinch>0,"Pulling apart stretches.");
        Check(SoftToyGesture.Sample(1,-.5f).Compression>SoftToyGesture.Sample(1,0).Compression,"Two-finger downward drag presses.");
        Check(SoftToyGesture.Sample(1,.5f).LiftDelta>.4f,"Upward drag lifts.");
        foreach(float ratio in new[]{float.NaN,float.PositiveInfinity,-100f,0,.5f,1,100})
        foreach(float drag in new[]{float.NaN,float.NegativeInfinity,-100f,0,100})
        {var p=SoftToyGesture.Sample(ratio,drag);Check(p.Pinch>=-.35f&&p.Pinch<=.55f&&p.Compression>=.04f&&p.Compression<=.42f&&Math.Abs(p.LiftDelta)<=2,"Soft inputs remain finite and bounded.");}
        Console.WriteLine("PASS: material-aware pinch, stretch, press and lift checks.");
    }
}
