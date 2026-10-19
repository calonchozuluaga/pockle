#ifndef POCKLE_SURFACE_PLAY
#define POCKLE_SURFACE_PLAY
half _SurfaceKind, _BrushAmount;
float _ContentsPhase;
float4 _BrushFrom,_BrushTo,_BrushDirection,_BrushTrailWeights;
float4 _BrushTrailFrom[4],_BrushTrailTo[4];
float3 ContentsCoordinate(float3 p)
{
    if(_SurfaceKind<1.5||_SurfaceKind>2.5)return p;
    float s=sin(-_ContentsPhase),c=cos(_ContentsPhase);p.y+=.12;
    p.xy=float2(p.x*c-p.y*s,p.x*s+p.y*c);p.y-=.12;return p;
}
half BrushMask(float3 p)
{
    if(_SurfaceKind<2.5||_BrushAmount<.001)return 0;
    float3 strokeVector=_BrushTo.xyz-_BrushFrom.xyz;
    float t=saturate(dot(p-_BrushFrom.xyz,strokeVector)/max(dot(strokeVector,strokeVector),.0001));
    half mask=1-smoothstep(.08,.29,length(p-lerp(_BrushFrom.xyz,_BrushTo.xyz,t)));
    [unroll] for(int i=0;i<4;i++) {
        float3 a=_BrushTrailFrom[i].xyz,b=_BrushTrailTo[i].xyz,d=b-a;
        float q=saturate(dot(p-a,d)/max(dot(d,d),.0001));
        mask=max(mask,(1-smoothstep(.08,.29,length(p-lerp(a,b,q))))*_BrushTrailWeights[i]);
    }
    return mask*_BrushAmount;
}
half3 BrushNap(float3 p,half3 color,half mask)
{
    half nap=BrushMask(p)*mask;
    half stroke=dot(normalize(_BrushDirection.xyz+float3(.001,.001,0)),normalize(float3(.5,.7,-.5)));
    return color*(1+nap*(.16+stroke*.12))+nap*.035;
}
half ContentsLight(float3 p,float3 view)
{
    if(_SurfaceKind<1.5||_SurfaceKind>2.5)return 0;
    // A ray-offset sample moves beneath the membrane as the viewer rotates.
    float3 localView=normalize(mul((float3x3)unity_WorldToObject,view));
    p=ContentsCoordinate(p-localView*.14);
    float2 cell=frac(p.xy*3.1)-.5;
    float r=length(cell);
    half ring=(1-smoothstep(.075,.095,abs(r-.115)))*smoothstep(.11,.17,r);
    half dotLight=1-smoothstep(.026,.055,length(cell-float2(-.065,.065)));
    return (ring*.045+dotLight*.11)*(1-smoothstep(.5,.95,abs(p.z)));
}
#endif
