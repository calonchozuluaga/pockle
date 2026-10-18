using System;
namespace Pockle.Core
{
    /// <summary>Local viewer state. Angles stay unwrapped so panorama motion has no seam jump.</summary>
    public sealed class ToyOrbit
    {
        public float Yaw { get; private set; }
        public float Pitch { get; private set; }
        public float YawSpeed { get; private set; }
        public float PitchSpeed { get; private set; }
        public float Zoom { get; private set; } = 1;
        public float PanX { get; private set; }
        public float PanY { get; private set; }
        private static float Clamp(float x,float a,float b) => Math.Max(a,Math.Min(b,x));
        private static bool Finite(float x) => !float.IsNaN(x)&&!float.IsInfinity(x);
        public void Drag(float x,float y,float dt)
        {
            if(!Finite(x)||!Finite(y)||!Finite(dt)||dt<=0) return;
            Yaw+=x; Pitch+=y;
            YawSpeed=Clamp(x/dt,-480,480); PitchSpeed=Clamp(y/dt,-480,480);
        }
        public void Stop() { YawSpeed=PitchSpeed=0; }
        public void Step(float dt,bool held,bool calm)
        {
            if(calm) { Stop();return; }
            if(held||!Finite(dt)||dt<=0) return;
            dt=Math.Min(dt,.1f);
            float decay=(float)Math.Exp(-8*dt), travel=(1-decay)/8;
            Yaw+=YawSpeed*travel;Pitch+=PitchSpeed*travel;
            YawSpeed*=decay;PitchSpeed*=decay;
            if(Math.Abs(YawSpeed)<.05f)YawSpeed=0;
            if(Math.Abs(PitchSpeed)<.05f)PitchSpeed=0;
        }
        public void SetFrame(float zoom,float x,float y)
        {
            if(!Finite(zoom)||!Finite(x)||!Finite(y))return;
            Zoom=Clamp(zoom,.65f,1.8f);PanX=Clamp(x,-.7f,.7f);PanY=Clamp(y,-.65f,.65f);
        }
        /// <summary>Soft toys fill more horizontal room; retain vertical clearance for hops and turns.</summary>
        public static float FrameSize(float aspect,float zoom,bool soft)
        {
            if(!Finite(aspect)||aspect<=0)aspect=1;
            if(!Finite(zoom))zoom=1;
            return Math.Max(1.6f,(soft?1.2f:1.5f)/Math.Max(.1f,aspect))*Clamp(zoom,.65f,1.8f);
        }
        public void Reset(){Yaw=Pitch=PanX=PanY=0;Zoom=1;Stop();}
        public static float PanoramaCentre(float angle,float crop,float home,float fraction)
        {
            if(!Finite(angle)||!Finite(crop)||!Finite(home)||!Finite(fraction))return .5f;
            crop=Clamp(crop,0,1);
            return Clamp(home+angle/180f*(1-crop)*fraction,crop*.5f,1-crop*.5f);
        }
    }
}
