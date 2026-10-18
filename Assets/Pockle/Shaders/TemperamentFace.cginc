#ifndef POCKLE_TEMPERAMENT_FACE
#define POCKLE_TEMPERAMENT_FACE
float4 _TemperLeft,_TemperRight,_TemperMouth,_TemperSkinLeft,_TemperSkinRight,_TemperSkinMouth;
float4 _TemperLeftClean,_TemperRightClean; half _TemperSkinDetail,_TemperKeepEyePatch;
float4 _TemperMouthClean,_TemperNose,_TemperMouthColor;
float _TemperFront; half _CharacterTemperament;
half _TemperBlink,_TemperHappy,_TemperSurprise,_TemperLids,_TemperFrown,_TemperGlanceX,_TemperGlanceY,_TemperBlush,_TemperWink,_TemperLidAngle,_TemperSleepCurve,_TemperMouthOpen,_TemperMouthTilt,_TemperHighlightStyle,_TemperMouthScale;
half TemperFront(float3 p){return (1-smoothstep(_TemperFront-.02,_TemperFront+.08,p.z))*_CharacterTemperament;}
half TemperRegion(float2 p,float4 feature)
{
    return 1-smoothstep(1.08,1.65,length((p-feature.xy)/feature.zw));
}
half TemperKeepNose(float2 p){return smoothstep(.95,1.12,length((p-_TemperNose.xy)/_TemperNose.zw));}
half TemperSurface(float3 p)
{
    return max(max(TemperRegion(p.xy,_TemperLeftClean),TemperRegion(p.xy,_TemperRightClean)),TemperRegion(p.xy,_TemperMouthClean))*TemperFront(p)*TemperKeepNose(p.xy);
}
half3 TemperNormal(float3 p,half3 n)
{
    float cx=(_TemperLeft.x+_TemperRight.x)*.5,cy=(_TemperLeft.y+_TemperRight.y)*.5;
    half3 clean=UnityObjectToWorldNormal(normalize(half3((p.x-cx)*.7,(p.y-cy)*.7,-1)));
    float my=_TemperMouth.y;
    float2 centre=float2(cx,(cy+my)*.5);
    float2 radii=float2(abs(_TemperRight.x-_TemperLeft.x)*.5+_TemperLeft.z*1.7,abs(cy-my)*.5+_TemperLeft.w*1.7);
    half face=1-smoothstep(1.12,1.65,length((p.xy-centre)/radii));
    face*=TemperFront(p)*TemperKeepNose(p.xy);
    return normalize(lerp(n,clean,max(face,TemperSurface(p))));
}
half3 TemperSkin(float3 p,float2 uv)
{
    float noise=frac(sin(dot(floor(p.xy*380),float2(127.1,311.7)))*43758.5453);
    return tex2D(_MainTex,uv).rgb*(1+(noise-.5)*_TemperSkinDetail);
}
half3 TemperClean(float3 p,half3 paint)
{
    half front=TemperFront(p)*TemperKeepNose(p.xy);
    paint=lerp(paint,TemperSkin(p,_TemperSkinLeft.xy),TemperRegion(p.xy,lerp(_TemperLeftClean,_TemperLeft,_TemperKeepEyePatch))*front);
    paint=lerp(paint,TemperSkin(p,_TemperSkinRight.xy),TemperRegion(p.xy,lerp(_TemperRightClean,_TemperRight,_TemperKeepEyePatch))*front);
    paint=lerp(paint,TemperSkin(p,_TemperSkinMouth.xy),TemperRegion(p.xy,_TemperMouthClean)*front);
    return paint;
}
half TemperStroke(float distance,float width)
{
    float aa=max(fwidth(distance),.0008);
    return 1-smoothstep(width-aa,width+aa,abs(distance));
}
half TemperDisc(float2 p,float radius)
{
    float aa=max(fwidth(length(p)),.0008);
    return 1-smoothstep(radius-aa,radius+aa,length(p));
}
half4 TemperEye(float2 p,float4 eye,half wink,half side)
{
    float spacing=abs(_TemperRight.x-_TemperLeft.x);
    float2 centre=eye.xy+float2(_TemperGlanceX*spacing*.15,_TemperGlanceY*eye.w*.65);
    float2 d=p-centre;
    float2 radius=eye.zw*(1+_TemperSurprise*.08);
    float ellipse=length(d/radius),aa=max(fwidth(ellipse),.01);
    half open=1-smoothstep(1-aa,1+aa,ellipse);
    float lid=radius.y*(1-2*_TemperLids)-_TemperLidAngle*side*d.x*.65;
    open*=1-smoothstep(lid-.0015,lid+.0015,d.y);
    half closure=max(_TemperBlink,wink),closed=smoothstep(.45,.90,closure);
    float x=d.x/radius.x;
    float curve=lerp(-.21+.30*x*x,.16-.28*x*x,_TemperSleepCurve)*eye.w;
    half closedStroke=TemperStroke(d.y-curve,eye.w*.055)*(1-smoothstep(.88,1.03,abs(x)));
    half coverage=lerp(open,closedStroke,closed);
    half3 ink=lerp(half3(.035,.012,.025),half3(.16,.055,.095),saturate(-d.y/radius.y));
    float2 h=(d-float2(radius.x*.28,radius.y*.34))/eye.w;
    half normal=TemperDisc(h,.17);
    half doubleGlint=max(normal,TemperDisc((d-float2(-radius.x*.28,-radius.y*.16))/eye.w,.08));
    half starGlint=0;
    if(_TemperHighlightStyle>1)
    {
        float angle=atan2(h.y,h.x);starGlint=TemperDisc(h,.065+.16*pow(abs(cos(angle*2)),10));
    }
    half glint=lerp(lerp(normal,doubleGlint,saturate(_TemperHighlightStyle)),starGlint,saturate(_TemperHighlightStyle-1));
    glint*=1-smoothstep(.70,.82,max(_TemperLids,closure));
    return half4(lerp(ink,half3(1,.97,.91),glint),coverage);
}
half4 TemperFeatures(float3 p)
{
    half4 left=TemperEye(p.xy,_TemperLeft,_TemperWink,1),right=TemperEye(p.xy,_TemperRight,0,-1);
    half4 result=lerp(left,right,right.a);
    result.a=max(left.a,right.a);
    float2 d=p.xy-_TemperMouth.xy;
    float width=_TemperMouth.z*max(.5,_TemperMouthScale)*(1+_TemperHappy*.65);
    float x=d.x/max(width,.001);
    float curve=lerp(-.4+.7*x*x,.35-.7*x*x,_TemperFrown)*_TemperMouth.w+_TemperMouthTilt*d.x*.38;
    half smile=TemperStroke(d.y-curve,_TemperMouth.w*.17*_TemperMouthColor.w)*(1-smoothstep(.88,1.04,abs(x)));
    float2 roundPosition=d/(_TemperMouth.zw*float2(.40,.8));
    half roundMouth=TemperDisc(roundPosition,1);
    if(_TemperMouthColor.w>1.5)roundMouth=TemperStroke(length(roundPosition)-.75,.22);
    half mouth=lerp(smile,roundMouth,_TemperMouthOpen);
    result.rgb=lerp(result.rgb,_TemperMouthColor.rgb,mouth);result.a=max(result.a,mouth);
    result.a*=TemperFront(p)*TemperKeepNose(p.xy);return result;
}
half3 TemperFinish(float3 p,half3 color)
{
    float gap=abs(_TemperRight.x-_TemperLeft.x);
    float eyeHeight=(_TemperLeft.y+_TemperRight.y)*.5;
    float cheekY=_TemperMouth.y-_TemperMouth.w*1.6;
    float2 radii=float2(gap*.17,_TemperMouth.w*1.15);
    float2 a=(p.xy-float2(_TemperMouth.x-gap*.28,cheekY))/radii;
    float2 b=(p.xy-float2(_TemperMouth.x+gap*.28,cheekY))/radii;
    half cheek=max(1-smoothstep(.35,1.4,dot(a,a)),1-smoothstep(.35,1.4,dot(b,b)));
    color=lerp(color,half3(1,.32,.43),cheek*_TemperBlush*.6*TemperFront(p));
    half4 face=TemperFeatures(p);return lerp(color,face.rgb,face.a);
}
#endif
