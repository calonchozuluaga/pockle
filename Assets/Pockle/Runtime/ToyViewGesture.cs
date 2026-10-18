using System;
using Pockle.Core;
using UnityEngine;
using UnityEngine.EventSystems;
namespace Pockle.Runtime
{
    /// <summary>Viewer gestures own their starting fingers; UI touches never join the gesture.</summary>
    public sealed class ToyViewGesture : MonoBehaviour
    {
        public ToyOrbit Orbit { get; } = new ToyOrbit();
        public bool IsHolding => holding||softPair;
        public bool Calm;
        public bool Suspended;
        private Camera viewer;
        private Transform pivot;
        private JellyToy toy;
        private Action<bool> press;
        private Action reset;
        public Action Tapped;
        public Action GestureStarted;
        public Action TurnStarted;
        public Action<Vector2> SoftManipulationStarted;
        public Action<SoftToyPose,Vector3> SoftManipulated;
        public Action<bool> SoftManipulationEnded;
        private bool softPair;
        private float pendingTap=-1;
        private int first=-999,second=-999;
        private Vector2 start,previous,pairCentre,lastTapPosition,screen;
        private float began,pairDistance,pairZoom,pairX,pairY,pairUnits,lastTap=-10;
        private bool moved,holding,paired,onBody,tapOnBody,suppressUntilLift;
        private Vector3 home;
        public void Initialize(Camera camera,Transform root,JellyToy body,Action<bool> onPress,Action onReset)
        { viewer=camera;pivot=root;toy=body;press=onPress;reset=onReset;home=root.position;screen=new Vector2(Screen.width,Screen.height); }
        private bool Allowed(Vector2 p,int id) => new Rect(0,0,Screen.width,Screen.height).Contains(p)&&
            (EventSystem.current==null||!(id<0?EventSystem.current.IsPointerOverGameObject():EventSystem.current.IsPointerOverGameObject(id)));
        private float Slop => Mathf.Max(8,Mathf.Min(Screen.width,Screen.height)*.012f);
        private void Begin(int id,Vector2 p)
        {
            if(!Allowed(p,id))return;
            GestureStarted?.Invoke();
            first=id;start=previous=p;began=Time.unscaledTime;moved=paired=holding=false;
            tapOnBody=toy.RaycastBody(viewer.ScreenPointToRay(p),out _);
            onBody=toy.Handling!=MaterialHandling.Firm&&tapOnBody;Orbit.Stop();
        }
        private void Move(Vector2 p)
        {
            bool wasMoved=moved;
            if((p-start).sqrMagnitude>Slop*Slop)moved=true;
            if(moved&&!wasMoved&&!holding)TurnStarted?.Invoke();
            if(moved&&!holding)
            {
                Vector2 d=p-previous;
                Orbit.Drag(-d.x/Mathf.Max(1,viewer.pixelWidth)*260,d.y/Mathf.Max(1,viewer.pixelHeight)*260,Mathf.Max(.001f,Time.unscaledDeltaTime));
            }
            else if(!moved&&!holding&&onBody&&!paired&&Time.unscaledTime-began>=.35f)
            {holding=true;Orbit.Stop();press?.Invoke(true);}
            previous=p;
        }
        private void End(bool completed)
        {
            bool tap=completed&&!moved&&!paired&&!holding&&Time.unscaledTime-began<.35f;
            if(holding)press?.Invoke(false);
            if(softPair)SoftManipulationEnded?.Invoke(completed);
            softPair=false;
            if(!completed||holding||paired||Time.unscaledTime-began>.35f&&(previous-start).sqrMagnitude<Slop*Slop)Orbit.Stop();
            first=second=-999;holding=false;
            if(tap)
            {
                if(Time.unscaledTime-lastTap<.3f&&(previous-lastTapPosition).sqrMagnitude<Slop*Slop*9)
                {lastTap=-10;pendingTap=-1;reset?.Invoke();}
                else {lastTap=Time.unscaledTime;lastTapPosition=previous;pendingTap=tapOnBody?Time.unscaledTime+.31f:-1;}
            }
            else {lastTap=-10;pendingTap=-1;}
        }
        public void Cancel(){if(holding)press?.Invoke(false);if(softPair)SoftManipulationEnded?.Invoke(false);softPair=false;first=second=-999;holding=false;suppressUntilLift=true;Orbit.Stop();lastTap=-10;pendingTap=-1;}
        public void ResetView(){Cancel();Orbit.Reset();viewer?.GetComponent<ToyStageEnvironment>()?.ResetView();Apply();}
        public void ReadInput()
        {
            if(Suspended||viewer==null)return;
            Vector2 size=new Vector2(Screen.width,Screen.height);
            if(size!=screen){Cancel();screen=size;}
            if(suppressUntilLift)
            {if(Input.touchCount==0&&!Input.GetMouseButton(0))suppressUntilLift=false;return;}
            if(Input.touchCount>0||first>=0)
            {
                if(first==-999)
                    for(int i=0;i<Input.touchCount&&first==-999;i++){var t=Input.GetTouch(i);if(t.phase==TouchPhase.Began)Begin(t.fingerId,t.position);}
                if(first==-999)return;
                if(!Find(first,out var a)||a.phase==TouchPhase.Canceled){End(false);suppressUntilLift=true;return;}
                if(a.phase==TouchPhase.Ended){End(true);suppressUntilLift=true;return;}
                if(second==-999)
                    for(int i=0;i<Input.touchCount;i++)
                    {
                        var b=Input.GetTouch(i);
                        if(b.fingerId==first||b.phase!=TouchPhase.Began||!Allowed(b.position,b.fingerId))continue;
                        float distance=Vector2.Distance(a.position,b.position);if(distance<Slop)continue;
                        if(SoftToyGesture.UsesDeformation(toy.Handling)&&!tapOnBody)continue;
                        BeginPair(a.position,b.position,b.fingerId);break;
                    }
                if(second!=-999)
                {
                    if(!Find(second,out var b)||b.phase==TouchPhase.Ended||b.phase==TouchPhase.Canceled)
                    {End(b.phase==TouchPhase.Ended);suppressUntilLift=true;return;}
                    MovePair(a.position,b.position);
                }
                else Move(a.position);
                return;
            }
            if(Input.GetMouseButtonDown(0))Begin(-1,Input.mousePosition);
            if(first==-1){if(Input.GetMouseButtonUp(0))End(true);else if(Input.GetMouseButton(0))Move(Input.mousePosition);else End(false);}
            float scroll=Input.mouseScrollDelta.y;
            if(scroll!=0&&Allowed(Input.mousePosition,-1))Orbit.SetFrame(Orbit.Zoom*Mathf.Exp(-scroll*.1f),Orbit.PanX,Orbit.PanY);
        }
        private void BeginPair(Vector2 a,Vector2 b,int id)
        {
            if(holding){holding=false;press?.Invoke(false);}
            second=id;paired=true;Orbit.Stop();pendingTap=-1;
            pairDistance=Mathf.Max(Slop,Vector2.Distance(a,b));pairCentre=(a+b)*.5f;
            pairZoom=Orbit.Zoom;pairX=Orbit.PanX;pairY=Orbit.PanY;
            pairUnits=2*viewer.orthographicSize/Mathf.Max(1,viewer.pixelHeight);
            softPair=SoftToyGesture.UsesDeformation(toy.Handling);
            if(softPair)SoftManipulationStarted?.Invoke(pairCentre);
        }
        private void MovePair(Vector2 a,Vector2 b)
        {
            Vector2 separation=b-a,delta=(a+b)*.5f-pairCentre;
            float distance=Mathf.Max(Slop,separation.magnitude);
            if(softPair)
            {
                Vector3 axis=toy.transform.InverseTransformDirection(viewer.transform.right*separation.x+viewer.transform.up*separation.y).normalized;
                SoftManipulated?.Invoke(SoftToyGesture.Sample(distance/pairDistance,delta.y*pairUnits),axis);
            }
            else Orbit.SetFrame(pairZoom*pairDistance/distance,pairX+delta.x*pairUnits,pairY+delta.y*pairUnits);
        }
        private static bool Find(int id,out Touch touch)
        {for(int i=0;i<Input.touchCount;i++){touch=Input.GetTouch(i);if(touch.fingerId==id)return true;}touch=default;return false;}
        private void LateUpdate()
        {
            if(viewer==null)return;
            if(pendingTap>=0&&Time.unscaledTime>=pendingTap&&!Suspended&&first==-999)
            {pendingTap=-1;Tapped?.Invoke();}
            Orbit.Step(Time.unscaledDeltaTime,first!=-999||Suspended,Calm);Apply();
        }
        private void Apply()
        {
            if(pivot==null)return;
            pivot.rotation=Quaternion.AngleAxis(Orbit.Pitch,viewer.transform.right)*Quaternion.AngleAxis(Orbit.Yaw,Vector3.up);
            Vector3 offset=viewer.transform.right*Orbit.PanX+viewer.transform.up*Orbit.PanY;
            pivot.position=home+offset;
            var shadow=pivot.parent.Find("Pip · contact shadow");
            if(shadow!=null)shadow.position=new Vector3(0,.151f,.04f)+offset;
            PrototypeStage.SetSoftFraming(viewer,toy!=null&&toy.Handling!=Pockle.Core.MaterialHandling.Firm);
            PrototypeStage.SetZoom(viewer,Orbit.Zoom);
            viewer.GetComponent<ToyStageEnvironment>().SetViewAngles(Orbit.Yaw,Orbit.Pitch);
        }
        private void OnApplicationFocus(bool focus){if(!focus)Cancel();}
        private void OnApplicationPause(bool paused){if(paused)Cancel();}
        private void OnDisable(){Cancel();}
    }
}
