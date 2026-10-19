using Pockle.Core;
using UnityEngine;
namespace Pockle.Runtime
{
    public sealed partial class JellyToy
    {
        private float contentsPhase;
        private float surfaceRockX,surfaceRockY;
        public void SetSurfaceRock(float x,float y)
        {
            Initialize();
            bool rigid=Handling==MaterialHandling.Firm&&MaterialTouch.ForToy(collectibleId)==MaterialTouchKind.Wobble;
            x=rigid?SafeClamp(x,-1,1):0;y=rigid?SafeClamp(y,-1,1):0;
            if(Mathf.Abs(x-surfaceRockX)>ChangeThreshold||Mathf.Abs(y-surfaceRockY)>ChangeThreshold)haveDeformation=false;
            surfaceRockX=x;surfaceRockY=y;
        }
        private readonly Vector4[] brushTrailFrom=new Vector4[4],brushTrailTo=new Vector4[4];
        private readonly float[] brushTrailBorn=new float[4];
        private int brushTrailNext;
        private string surfaceToyId="";
        private Vector3 lastTrailPoint;
        private bool haveTrail;
        private Vector3 SurfaceRest(Vector3 rest,bool isSurface)
        {
            if(isSurface||contentsPhase==0)return rest;
            return Quaternion.AngleAxis(contentsPhase*Mathf.Rad2Deg,Vector3.forward)*(rest+Vector3.up*.12f)-Vector3.up*.12f;
        }
        public void SetSurfacePlay(float phase,Vector3 from,Vector3 to,Vector3 direction,float nap)
        {
            Initialize();
            if(surfaceToyId!=collectibleId) {surfaceToyId=collectibleId;haveTrail=false;System.Array.Clear(brushTrailBorn,0,4);}
            var kind=MaterialTouch.ForToy(collectibleId);
            contentsPhase=kind==MaterialTouchKind.Swirl&& !float.IsNaN(phase)&&!float.IsInfinity(phase)?phase:0;
            if(bodyMaterial!=null&&bodyMaterial.HasProperty("_SurfaceKind")) {
                bodyMaterial.SetFloat("_SurfaceKind",(float)kind);
                bodyMaterial.SetFloat("_ContentsPhase",contentsPhase);
                bodyMaterial.SetVector("_BrushFrom",from);bodyMaterial.SetVector("_BrushTo",to);
                bodyMaterial.SetVector("_BrushDirection",direction);
                bodyMaterial.SetFloat("_BrushAmount",kind==MaterialTouchKind.Brush?Mathf.Clamp01(nap):0);
                if(kind==MaterialTouchKind.Brush&&nap>.01f) {
                    if(!haveTrail||(to-lastTrailPoint).sqrMagnitude>.012f) {
                        brushTrailFrom[brushTrailNext]=haveTrail?lastTrailPoint:from;brushTrailTo[brushTrailNext]=to;
                        brushTrailBorn[brushTrailNext]=Time.unscaledTime+.001f;brushTrailNext=(brushTrailNext+1)%4;lastTrailPoint=to;haveTrail=true;
                    }
                    Vector4 weights=Vector4.zero;for(int i=0;i<4;i++)weights[i]=brushTrailBorn[i]>0?Mathf.Exp(-.65f*Mathf.Max(0,Time.unscaledTime-brushTrailBorn[i])):0;
                    bodyMaterial.SetVectorArray("_BrushTrailFrom",brushTrailFrom);bodyMaterial.SetVectorArray("_BrushTrailTo",brushTrailTo);bodyMaterial.SetVector("_BrushTrailWeights",weights);
                } else if(nap<=0) {haveTrail=false;bodyMaterial.SetVector("_BrushTrailWeights",Vector4.zero);}
            }
            if(kind!=MaterialTouchKind.Swirl)return;
            var h=Handling;
            var directional=new DirectionalStretch(previousPinch*h.Pinch,new Point3(previousPinchAxis.x,previousPinchAxis.y,previousPinchAxis.z));
            var suspension=new SuspendedShape(previousSag*h.Sag,new Point3(previousGrip.x,previousGrip.y,previousGrip.z),previousClearance);
            foreach(var accent in accents) if(!accent.IsSurface&&accent.Transform!=null)
                accent.Transform.localPosition=Deform(SurfaceRest(accent.Rest,false),previousCompression*h.Compression,previousStretch*h.Stretch,
                    previousTiltX*h.Bend,previousTiltZ*h.Bend,previousContactX,previousContactZ,directional,suspension);
        }
    }
}
