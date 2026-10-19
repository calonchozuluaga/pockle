using System;
namespace Pockle.Core
{
    public enum MaterialTouchKind { None, Wobble, Swirl, Brush }
    /// <summary>Bounded local material response. No inventory or personality dependency.</summary>
    public sealed class MaterialTouch
    {
        private readonly Spring1D x = new Spring1D(2.1f,.28f), y = new Spring1D(2.1f,.28f);
        public float RockX { get; private set; }
        public float RockY { get; private set; }
        public float Phase { get; private set; }
        public float Spin { get; private set; }
        public float Nap { get; private set; }
        public bool Held { get; private set; }
        public MaterialTouchKind Kind { get; private set; }
        private static float Safe(float n,float min,float max) => Numeric.Clamp(Numeric.FiniteOr(n,0),min,max);
        public static MaterialTouchKind ForFinish(string finish)
        {
            switch(finish) {
                case "matte-vinyl": case "gloss-vinyl": case "coated-metallic": return MaterialTouchKind.Wobble;
                case "velvet-flock": case "boucle-plush": return MaterialTouchKind.Brush;
                case "clear-jelly": case "pearl-jelly": case "confetti-gel": case "glow-jelly": return MaterialTouchKind.Swirl;
                default: return MaterialTouchKind.None;
            }
        }
        public static MaterialTouchKind ForToy(string id)
        {
            if(FusionCatalog.TryGet(id,out var fusion)) {
                var a=ForToy(fusion.First); var b=ForToy(fusion.Second);
                if(a==MaterialTouchKind.Swirl||b==MaterialTouchKind.Swirl)return MaterialTouchKind.Swirl;
                if(a==MaterialTouchKind.Brush||b==MaterialTouchKind.Brush)return MaterialTouchKind.Brush;
                return a==MaterialTouchKind.Wobble&&b==a?a:MaterialTouchKind.None;
            }
            return ToyCatalog.TryGetCollectible(id,out var toy)?ForFinish(toy.FinishId):MaterialTouchKind.None;
        }
        public void Begin(MaterialTouchKind kind) { Kind=kind; Held=kind!=MaterialTouchKind.None; }
        public void Stroke(float dx,float dy,float px,float py)
        {
            if(!Held||!Numeric.IsFinite(dx)||!Numeric.IsFinite(dy)||!Numeric.IsFinite(px)||!Numeric.IsFinite(py))return;
            dx=Safe(dx,-.2f,.2f); dy=Safe(dy,-.2f,.2f);
            if(Kind==MaterialTouchKind.Wobble) {x.Target=Safe(x.Target+dx*7,-.9f,.9f);y.Target=Safe(y.Target+dy*7,-.9f,.9f);}
            if(Kind==MaterialTouchKind.Swirl)Spin=Safe(Spin+(px*dy-py*dx)*55,-7,7);
            if(Kind==MaterialTouchKind.Brush)Nap=Safe(Nap+(Math.Abs(dx)+Math.Abs(dy))*9,0,1);
        }
        public void End(bool completed=true) {Held=false;x.Target=y.Target=0;if(!completed)Reset();}
        public void Step(float dt,bool calm)
        {
            if(!Numeric.IsFinite(dt)||dt<=0)return;dt=Math.Min(dt,.1f);
            if(calm) {x.Reset(Held?x.Target:0); y.Reset(Held?y.Target:0);Spin=0;} else {x.Step(dt);y.Step(dt);}
            RockX=Safe(x.Value,-1,1)*(calm?.35f:1); RockY=Safe(y.Value,-1,1)*(calm?.35f:1);
            Phase=(Phase+Spin*dt)%(float)(Math.PI*2);
            Spin*= (float)Math.Exp(-1.7f*dt);Nap*=(float)Math.Exp(-.85f*dt);
        }
        public void Reset() {x.Reset();y.Reset();RockX=RockY=Phase=Spin=Nap=0;Held=false;Kind=MaterialTouchKind.None;}
    }
}
