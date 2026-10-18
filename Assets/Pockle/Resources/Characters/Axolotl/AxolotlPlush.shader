Shader "Pockle/Axolotl Plush"
{
    Properties
    {
        _CharacterTemperament ("Local temperament face", Float) = 1
        _TemperBlink ("Temperament Blink", Float) = 0
        _TemperHappy ("Temperament Happy", Float) = 0
        _TemperSurprise ("Temperament Surprise", Float) = 0
        _TemperLids ("Temperament Lids", Float) = 0
        _TemperFrown ("Temperament Frown", Float) = 0
        _TemperGlanceX ("Temperament GlanceX", Float) = 0
        _TemperGlanceY ("Temperament GlanceY", Float) = 0
        _TemperBlush ("Temperament Blush", Float) = 0
        _TemperWink ("Temperament Wink", Float) = 0
        _TemperLidAngle ("Temperament LidAngle", Float) = 0
        _TemperSleepCurve ("Temperament SleepCurve", Float) = 0
        _TemperMouthOpen ("Temperament MouthOpen", Float) = 0
        _TemperMouthTilt ("Temperament MouthTilt", Float) = 0
        _TemperHighlightStyle ("Temperament HighlightStyle", Float) = 0
        _TemperMouthScale ("Temperament MouthScale", Float) = 1
        _TemperLeft ("Face layout Left", Vector) = (0,0,.1,.1)
        _TemperRight ("Face layout Right", Vector) = (0,0,.1,.1)
        _TemperMouth ("Face layout Mouth", Vector) = (0,0,.1,.1)
        _TemperSkinLeft ("Face layout SkinLeft", Vector) = (0,0,.1,.1)
        _TemperSkinRight ("Face layout SkinRight", Vector) = (0,0,.1,.1)
        _TemperSkinMouth ("Face layout SkinMouth", Vector) = (0,0,.1,.1)
        _TemperLeftClean ("Old left eye footprint", Vector) = (0,0,.1,.1)
        _TemperRightClean ("Old right eye footprint", Vector) = (0,0,.1,.1)
        _TemperKeepEyePatch ("Preserve cream eye pattern", Float) = 0
        _TemperSkinDetail ("Native skin detail scale", Float) = 0
        _TemperMouthClean ("Old smile footprint", Vector) = (0,0,.12,.06)
        _TemperNose ("Preserve native nose", Vector) = (5,5,.01,.01)
        _TemperMouthColor ("Mouth ink and thickness", Vector) = (.035,.008,.02,1)
        _TemperFront ("Face depth gate", Float) = -.3

        _StageEnvironment ("Painted environment", Cube) = "black" {}
        _StageEnvironmentStrength ("Environment amount", Range(0,1)) = 0
        _StageTint ("Stage illumination", Color) = (1,1,1,1)
        _StageEmission ("Night glow", Float) = 0
        _MainTex ("Approved Axolotl paint", 2D) = "white" {}
        _StudioCube ("Studio reflections", Cube) = "black" {}
        _BodyColor ("Finish pigment", Color) = (1,.5,.2,1)
        _TintStrength ("Finish tint amount", Range(0,1)) = 0
        _Blink ("Blink", Range(0,1)) = 0
        _Happy ("Happy", Range(0,1)) = 0
        _Surprise ("Surprise", Range(0,1)) = 0
        _IdleWave ("Gill and arm wave", Float) = 0
        _ArmWave ("Greeting arm", Float) = 0
        _GillFlutter ("Gill flutter", Float) = 0
        _TailSwish ("Tail swish", Float) = 0
        _Gloss ("Glaze", Range(0,1)) = .3
        _Opacity ("Body opacity", Range(0,1)) = 1
        _Fuzz ("Velvet pile", Range(0,1)) = 0
        _Grain ("Foam grain", Range(0,1)) = 0
        _Metallic ("Brushed copper", Range(0,1)) = 0
        _Pearl ("Pearl sheen", Range(0,1)) = 0
        _Crystal ("Crystal facets", Range(0,1)) = 0
        _Iridescence ("Aurora coating", Range(0,1)) = 0
        _Glitter ("Fine gold inclusions", Range(0,1)) = 0
    }
    CGINCLUDE
    #include "UnityCG.cginc"
    half4 _StageTint; half _StageEmission; samplerCUBE _StageEnvironment; half _StageEnvironmentStrength;
    sampler2D _MainTex; samplerCUBE _StudioCube;
    half4 _BodyColor; half _TintStrength, _Blink, _Happy, _Surprise, _IdleWave, _ArmWave, _GillFlutter, _TailSwish;
    half _Gloss, _Opacity, _Fuzz, _Grain, _Metallic, _Pearl, _Crystal, _Iridescence, _Glitter;
    #include "Assets/Pockle/Shaders/TemperamentFace.cginc"
    struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; float2 uv:TEXCOORD0; float3 faceRest:TEXCOORD1; };
    struct v2f { float4 vertex:SV_POSITION; float3 world:TEXCOORD0; float3 local:TEXCOORD1; half3 normal:TEXCOORD2; float2 uv:TEXCOORD3; float3 face:TEXCOORD4; };
    v2f vert(appdata v)
    {
        float3 rest=v.vertex.xyz;
        float gill=smoothstep(.58,.85,abs(rest.x))*smoothstep(.03,.28,rest.y);
        float arm=smoothstep(.40,.65,abs(rest.x))*(1-smoothstep(-.07,.10,rest.y))*smoothstep(-.66,-.35,rest.y);
        v.vertex.y+=_IdleWave*(gill*.018+arm*.025);
        float greeting=arm*smoothstep(0,.30,rest.x);
        float angle=_ArmWave*.65;
        float2 limb=rest.xy-float2(.40,-.20);
        float2 raised=float2(limb.x*cos(angle)-limb.y*sin(angle),limb.x*sin(angle)+limb.y*cos(angle));
        v.vertex.xy+=(raised-limb)*greeting;
        v.vertex.y+=gill*_GillFlutter*.065;
        v.vertex.z+=gill*_GillFlutter*sign(rest.x)*.045;
        float tail=smoothstep(.15,.55,rest.z)*(1-smoothstep(-.25,.10,rest.y));
        v.vertex.x+=tail*_TailSwish*.11;
        v2f o; o.vertex=UnityObjectToClipPos(v.vertex); o.world=mul(unity_ObjectToWorld,v.vertex).xyz;
        o.local=v.vertex.xyz; o.normal=UnityObjectToWorldNormal(v.normal); o.uv=v.uv; o.face=v.faceRest; return o;
    }
    float hash(float3 p) { return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453); }
    half bodyMask(half3 paint)
    { return smoothstep(.12,.36,max(paint.r,max(paint.g,paint.b))); }
    fixed4 finish(v2f i, bool back)
    {
        half3 paint=TemperClean(i.face,tex2D(_MainTex,i.uv).rgb); half body=bodyMask(paint);
        if(back) { clip(.999-_Opacity); clip(body-.3); }
        half3 n=TemperNormal(i.face,normalize(i.normal)); if(back) n=-n;
        half3 view=normalize(_WorldSpaceCameraPos.xyz-i.world);
        half facing=saturate(dot(n,view)); half rim=1-facing;
        half3 key=normalize(half3(.64,.70,-.66));
        half wrapped=saturate((dot(n,key)+.45)/1.45);
        half grain=hash(floor(i.local*480));
        half brush=sin(i.local.y*680+i.local.x*8)*.5+.5;
        half detail=dot(paint,half3(.3,.5,.2));
        half gill=saturate((paint.r-paint.g)*2.0);
        half3 pigment=_BodyColor.rgb*(.65+.35*detail)*(1-gill*.23);
        half3 base=lerp(paint,pigment,body*_TintStrength);
        base*=.70+.30*wrapped;
        base*=1-body*((grain-.5)*(_Fuzz*.22+_Grain*.16)+_Metallic*brush*.035);
        half3 reflected=reflect(-view,n);
        half3 studio=lerp(texCUBE(_StudioCube,reflected).rgb,texCUBE(_StageEnvironment,reflected).rgb,_StageEnvironmentStrength);
        half3 across=normalize(cross(key,half3(0,1,0))); half3 along=cross(across,key);
        half rx=dot(reflected,across), ry=dot(reflected,along);
        half spread=lerp(8,95,_Gloss);
        half softbox=exp2(-rx*rx*spread-ry*ry*spread*.48)*smoothstep(.5,.85,dot(reflected,key));
        half glaze=(softbox*.52+max(studio.r,max(studio.g,studio.b))*.16)*lerp(.08,1,_Gloss);
        half3 spec=lerp(half3(1,.98,.95),_BodyColor.rgb,_Metallic*body*.8);
        base+=spec*glaze*(1-TemperFeatures(i.face).a);
        base+=body*_Metallic*(studio*_BodyColor.rgb*.55+spec*softbox*.20);
        base+=body*_Fuzz*pow(rim,2)*half3(.25,.18,.14);
        half3 pearl=.5+.5*cos(half3(0,2,4)+facing*6);
        base+=body*_Pearl*pearl*(.06+.12*rim);
        half3 aurora=.5+.5*cos(half3(.5,2.4,4.4)+facing*9+i.local.y*1.8);
        base=lerp(base,base*.6+aurora*.45,body*_Iridescence*(.35+.65*rim));
        half facet=hash(floor(i.local*24));
        base+=body*_Crystal*smoothstep(.92,.99,facet)*(.06+.16*glaze)*half3(1,.7,.85);
        half fleck=smoothstep(.996,.999,grain);
        base+=body*_Glitter*fleck*half3(.8,.55,.16);
        base+=body*texCUBE(_StageEnvironment,refract(-view,n,.74)).rgb*_StageEnvironmentStrength*(1-_Opacity)*.24;
        half alpha=lerp(1,_Opacity+(1-_Opacity)*rim*.42,body);
        if(back) alpha*=.32;
        else { base=TemperFinish(i.face,base);alpha=max(alpha,TemperFeatures(i.face).a); }
        return fixed4(saturate(base * _StageTint.rgb + _StageEmission * half3(.23,.65,.64) * body),alpha);
    }
    fixed4 fragBack(v2f i):SV_Target { return finish(i,true); }
    fixed4 fragFront(v2f i):SV_Target { return finish(i,false); }
    ENDCG
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Cull Back ZWrite On ColorMask 0
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment fragDepth
            fixed4 fragDepth(v2f i):SV_Target { return 0; }
            ENDCG
        }
        Pass
        {
            Cull Front ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment fragBack
            ENDCG
        }
        Pass
        {
            Cull Back ZWrite On Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment fragFront
            ENDCG
        }
    }
    Fallback "Unlit/Texture"
}
