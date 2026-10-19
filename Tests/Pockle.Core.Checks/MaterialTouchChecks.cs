using System;
using Pockle.Core;
internal static class MaterialTouchChecks
{
    private static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
    public static void Run()
    {
        Check(MaterialTouch.ForFinish("boucle-plush")==MaterialTouchKind.Brush,"Plush should brush.");
        Check(MaterialTouch.ForFinish("velvet-flock")==MaterialTouchKind.Brush,"Flock should brush.");
        Check(MaterialTouch.ForFinish("mochi-foam")==MaterialTouchKind.None,"Foam retains existing squish.");
        Check(MaterialTouch.ForToy("pip.gloss-vinyl")==MaterialTouchKind.Wobble,"Vinyl should rock.");
        Check(MaterialTouch.ForToy("axolotl.clear-jelly")==MaterialTouchKind.Swirl,"Jelly should stir.");
        var fusion=new FusedToy {Id="fusion.touch-check",First="axolotl.clear-jelly",Second="axolotl.coated-metallic",Name="Honey Copper"};
        FusionCatalog.Restore(new[]{fusion});Check(MaterialTouch.ForToy(fusion.Id)==MaterialTouchKind.Swirl,"Saved jelly-metal fusion should stir.");
        FusionCatalog.Restore(null);
        var state=new MaterialTouch();state.Begin(MaterialTouchKind.Wobble);state.Stroke(.12f,-.1f,0,0);
        for(int i=0;i<30;i++)state.Step(1f/60,false);
        Check(state.RockX>.4f&&state.RockY<-.3f,"Held rocking follows the stroke.");state.End();bool rebound=false;
        for(int i=0;i<480;i++){state.Step(1f/60,false);rebound|=state.RockX<-.01f;Check(Math.Abs(state.RockX)<=1&&Math.Abs(state.RockY)<=1,"Rock escaped bounds.");}
        Check(rebound&&Math.Abs(state.RockX)<.001f,"Rigid rocking must rebound and settle.");
        state.Reset();state.Begin(MaterialTouchKind.Swirl);
        for(int i=0;i<72;i++){
            float a=i*(float)Math.PI/36,b=(i+1)*(float)Math.PI/36;
            state.Stroke((float)(Math.Cos(b)-Math.Cos(a))*.4f,(float)(Math.Sin(b)-Math.Sin(a))*.4f,(float)Math.Cos(b)*.4f,(float)Math.Sin(b)*.4f);state.Step(1f/60,false);
        }
        Check(state.Spin>0&&state.Phase!=0,"Circular strokes should stir contents.");state.End();float phase=state.Phase;state.Step(.1f,false);
        Check(state.Phase!=phase,"Contents should coast after release.");for(int i=0;i<600;i++)state.Step(1f/60,false);
        Check(Math.Abs(state.Spin)<.001f,"Contents must settle.");
        state.Reset();state.Begin(MaterialTouchKind.Brush);state.Stroke(.1f,.1f,0,0);Check(state.Nap>.9f,"Stroke should lay the pile.");
        state.End();for(int i=0;i<600;i++)state.Step(1f/60,false);Check(state.Nap<.001f,"Pile should recover.");
        state.Begin(MaterialTouchKind.Wobble);state.Stroke(float.NaN,float.PositiveInfinity,0,0);state.Step(float.NaN,false);state.Step(100,false);
        Check(!float.IsNaN(state.RockX),"Malformed input poisoned response.");
        state.End(false);Check(state.Nap==0&&state.Phase==0&&!state.Held&&state.Kind==MaterialTouchKind.None,"Cancellation must clear every response.");
        state.Begin(MaterialTouchKind.Swirl);state.Stroke(.1f,.1f,1,0);state.Step(.1f,true);Check(state.Spin==0&&state.Phase==0,"Calm disables inertial swirling.");
        Console.WriteLine("PASS: material touch routing, rigid rebound, circular stirring/coast, recovering plush nap, cancellation, Calm and malformed inputs.");
    }
}
