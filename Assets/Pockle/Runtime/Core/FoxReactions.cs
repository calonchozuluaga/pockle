using System;
namespace Pockle.Core
{
    public enum FoxAction { Jump, TailWag, CuriousTilt, SleepyBlink, GreetingBow }
    public readonly struct FoxActionPose
    {
        public readonly float Lift,Pitch,Yaw,Roll,HeadRoll,Tail,Ears,Blink,Happy,Surprise,HeadPitch;
        public FoxActionPose(float lift,float pitch,float yaw,float roll,float headRoll,float tail,float ears,float blink,float happy,float surprise,float headPitch=0)
        {Lift=lift;Pitch=pitch;Yaw=yaw;Roll=roll;HeadRoll=headRoll;Tail=tail;Ears=ears;Blink=blink;Happy=happy;Surprise=surprise;HeadPitch=headPitch;}
    }
    public static class FoxReactions
    {
        public const float Duration=2;
        public static FoxAction Choose(int previous,int draw) => (FoxAction)(int)PipReactions.Choose(previous,draw);
        public static FoxActionPose Sample(FoxAction action,float progress,bool calm)
        {
            if(!Enum.IsDefined(typeof(FoxAction),action)||float.IsNaN(progress)||float.IsInfinity(progress)||progress<=0||progress>=1)return default;
            float e=(float)Math.Pow(Math.Sin(progress*Math.PI),2),wave=(float)Math.Sin(progress*Math.PI*6)*e;
            if(calm)return new FoxActionPose(0,0,0,0,0,0,0,0,.25f*e,0);
            switch(action)
            {
                case FoxAction.Jump:return new FoxActionPose(.32f*e,0,0,0,0,14*wave,.8f*e,0,.4f*e,.7f*e);
                case FoxAction.TailWag:return new FoxActionPose(0,0,0,2*wave,0,28*wave,.5f*e,0,.85f*e,0);
                case FoxAction.CuriousTilt:return new FoxActionPose(0,0,-8*e,0,14*e,5*e,.7f*e,0,.1f*e,.6f*e);
                case FoxAction.SleepyBlink:return new FoxActionPose(0,0,0,-3*e,-5*e,-10*e,-.7f*e,Math.Min(1,e*1.4f),.25f*e,0);
                default:return new FoxActionPose(0,0,0,0,0,10*wave,.6f*e,0,.8f*e,0,-18*e);
            }
        }
    }
    /// <summary>Procedural articulation of the approved Fox v2 mesh, with a smoothly pinned tail root.</summary>
    public readonly struct FoxDeformation
    {
        private readonly FoxActionPose pose;
        private readonly float tailCos,tailSin,headCos,headSin,pitchCos,pitchSin;
        public FoxDeformation(FoxActionPose value)
        {
            pose=value;double tail=value.Tail*Math.PI/180,head=value.HeadRoll*Math.PI/180;
            tailCos=(float)Math.Cos(tail);tailSin=(float)Math.Sin(tail);headCos=(float)Math.Cos(head);headSin=(float)Math.Sin(head);
            double pitch=value.HeadPitch*Math.PI/180;pitchCos=(float)Math.Cos(pitch);pitchSin=(float)Math.Sin(pitch);
        }
        private static float Smooth(float t){t=Numeric.Clamp(t,0,1);return t*t*(3-2*t);}
        private static float Region(Point3 p,float x,float y,float rx,float ry)
        {float dx=(p.X-x)/rx,dy=(p.Y-y)/ry;return Smooth((1-dx*dx-dy*dy)*3)*Smooth((-p.Z-.30f)/.15f);}
        public static float TailWeight(Point3 p)=>Smooth((p.Z-.10f)/.20f)*Smooth((-p.X-.22f)/.20f)*Smooth((-.30f-p.Y)/.20f);
        public Point3 Apply(Point3 p)
        {
            float x=p.X,y=p.Y,z=p.Z;
            if(p.Z<-.30f && p.Y>-.26f && p.Y<.30f && (pose.Blink!=0||pose.Happy!=0||pose.Surprise!=0))
            {
                float closure=Math.Max(pose.Blink,pose.Happy*.65f);
                float left=Region(p,-.18f,.05f,.16f,.18f),right=Region(p,.38f,.04f,.17f,.18f);
                y-=(left*(y-.05f)+right*(y-.04f))*closure*.96f;
                y+=(left+right)*pose.Surprise*(p.Y-.045f)*.13f;
                float mouth=Region(p,.085f,-.173f,.12f,.065f),mx=(p.X-.085f)/.12f;
                y+=mouth*(pose.Happy*(.02f*mx*mx-.006f)+pose.Surprise*(p.Y+.173f)*.75f);
            }
            if(pose.Ears!=0 && p.Y>.43f)
            {
                float ear=Smooth((p.Y-.43f)/.28f);
                z+=ear*pose.Ears*.045f;x+=ear*pose.Ears*(p.X<.1f?-.018f:.018f);
            }
            float weight=(pose.HeadRoll!=0||pose.HeadPitch!=0||pose.Tail!=0)?TailWeight(p):0;
            if((pose.HeadRoll!=0||pose.HeadPitch!=0) && p.Y>-.36f)
            {
                float head=Smooth((p.Y+.36f)/.28f)*(1-weight),hx=x-.10f,hy=y+.25f;
                x+=(hx*headCos-hy*headSin-hx)*head;y+=(hx*headSin+hy*headCos-hy)*head;
                hy=y+.25f;float hz=z+.08f;
                y+=(hy*pitchCos-hz*pitchSin-hy)*head;z+=(hy*pitchSin+hz*pitchCos-hz)*head;
            }
            if(pose.Tail!=0 && weight>0)
            {
                float tx=p.X+.35f,tz=p.Z-.22f;
                x+=(tx*tailCos+tz*tailSin-tx)*weight;z+=(-tx*tailSin+tz*tailCos-tz)*weight;
            }
            return new Point3(x,y,z);
        }
    }
}
