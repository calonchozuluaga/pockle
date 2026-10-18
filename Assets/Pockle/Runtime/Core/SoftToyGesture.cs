namespace Pockle.Core
{
    public readonly struct SoftToyPose
    {
        public readonly float Pinch,Compression,LiftDelta;
        public SoftToyPose(float pinch,float compression,float liftDelta)
        { Pinch=pinch;Compression=compression;LiftDelta=liftDelta; }
    }
    /// <summary>Pinch deforms soft finishes; moving both fingers down presses, up lifts.</summary>
    public static class SoftToyGesture
    {
        public static bool UsesDeformation(MaterialHandling handling) => handling!=null && handling!=MaterialHandling.Firm;
        public static SoftToyPose Sample(float distanceRatio,float verticalDelta)
        {
            distanceRatio=Numeric.Clamp(Numeric.FiniteOr(distanceRatio,1),.1f,4);
            verticalDelta=Numeric.Clamp(Numeric.FiniteOr(verticalDelta,0),-2,2);
            return new SoftToyPose(Numeric.Clamp((distanceRatio-1)*.75f,-.35f,.55f),
                Numeric.Clamp(.18f-verticalDelta*.3f,.04f,.42f),verticalDelta);
        }
    }
}
