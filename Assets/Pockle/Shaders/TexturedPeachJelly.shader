Shader "Pockle/Textured Peach Jelly"
{
    Properties
    {
        _StageTint ("Stage illumination", Color) = (1,1,1,1)
        _StageEmission ("Night glow", Float) = 0
        _FinishTint ("Collection tint", Color) = (1,1,1,1)
        _TintStrength ("Collection tint amount", Range(0,1)) = 0
        _GlitterStrength ("Glitter flakes", Range(0,1)) = 0
        _PearlSheen ("Pearl sheen", Range(0,1)) = 0
        _Fuzz ("Velvet pile", Range(0,1)) = 0
        _Metallic ("Metal coating", Range(0,1)) = 0
        _Iridescence ("Iridescence", Range(0,1)) = 0
        _Glow ("Inner glow", Range(0,1)) = 0
        _ZWrite ("Opaque finish depth", Float) = 0
        _MainTex ("Owner painted Peach", 2D) = "white" {}
        _StudioCube ("Studio reflections", Cube) = "black" {}
        _Roughness ("Wet roughness", Range(0.08,0.4)) = 0.16
        _ReflectionStrength ("Moving studio reflection", Range(0,1)) = 0.80
        _Opacity ("Jelly shell opacity", Range(0.25,1)) = 0.62
        _Transmission ("Backlight transmission", Range(0,1)) = 0.90
        _Subsurface ("Soft scattering", Range(0,0.2)) = 0.05
        _SubsurfaceColor ("Warm peach scattering", Color) = (1,0.62,0.38,1)
        _SpeckEmission ("Painted gold glow", Range(0,0.5)) = 0.20
        _Blink ("Blink", Range(0,1)) = 0
        _Happy ("Happy squint", Range(0,1)) = 0
        _Surprise ("Surprised", Range(0,1)) = 0
        _LeftEye ("Left eye region", Vector) = (-0.284,-0.06,0.15,0.17)
        _RightEye ("Right eye region", Vector) = (0.32,-0.06,0.15,0.17)
        _Mouth ("Mouth region", Vector) = (0.02,-0.125,0.12,0.09)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        CGINCLUDE
        #include "UnityCG.cginc"
    half4 _StageTint; half _StageEmission;
        sampler2D _MainTex;
        samplerCUBE _StudioCube;
        half _Roughness, _ReflectionStrength, _Opacity, _Transmission, _Subsurface, _SpeckEmission;
        half _Blink, _Happy, _Surprise;
        half4 _SubsurfaceColor, _FinishTint;
        half _TintStrength, _GlitterStrength, _PearlSheen, _Fuzz, _Metallic, _Iridescence, _Glow;
        float4 _LeftEye, _RightEye, _Mouth;
        struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; float2 uv : TEXCOORD0; };
        struct v2f { float4 vertex : SV_POSITION; float3 world : TEXCOORD0; half3 normal : TEXCOORD1; float2 uv : TEXCOORD2; float3 local : TEXCOORD3; };
        float Region(float2 position, float4 feature)
        {
            float2 d = (position - feature.xy) / feature.zw;
            return 1 - smoothstep(.45,1,dot(d,d));
        }
        float EyeShift(float2 p, float4 eye)
        {
            float closure = max(_Blink, _Happy * .78);
            float dy = p.y - eye.y;
            float dx = (p.x - eye.x) / eye.z;
            return Region(p,eye) * (-dy * closure * .92 + _Happy * (.018 - .035*dx*dx) + _Surprise * dy * .12);
        }
        v2f vert(appdata v)
        {
            v2f o; o.local = v.vertex.xyz;
            float front = smoothstep(.45,.62,-v.vertex.z);
            float eyeShift = EyeShift(v.vertex.xy,_LeftEye) + EyeShift(v.vertex.xy,_RightEye);
            float mouthShift = Region(v.vertex.xy,_Mouth) * (v.vertex.y-_Mouth.y) * _Surprise * 1.1;
            v.vertex.y += front * (eyeShift + mouthShift);
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.world = mul(unity_ObjectToWorld,v.vertex).xyz;
            o.normal = UnityObjectToWorldNormal(v.normal); o.uv = v.uv;
            return o;
        }
        half FacePigment(v2f i, half3 paint)
        {
            half brightness=max(paint.r,max(paint.g,paint.b));
            half face=saturate(Region(i.local.xy,_LeftEye)+Region(i.local.xy,_RightEye)+Region(i.local.xy,_Mouth))*smoothstep(.45,.62,-i.local.z);
            return face*(1-smoothstep(.15,.45,brightness));
        }
        half3 FinishPaint(v2f i,half3 paint)
        {
            half brightness=max(paint.r,max(paint.g,paint.b));
            half luminance=dot(paint,half3(.299,.587,.114));
            half minimum=min(paint.r,min(paint.g,paint.b));
            half whiteHighlight=smoothstep(.90,.98,brightness)*(1-smoothstep(.04,.15,brightness-minimum));
            return lerp(paint,_FinishTint.rgb*(.55+.45*luminance),_TintStrength*(1-FacePigment(i,paint))*(1-whiteHighlight));
        }
        fixed4 fragBack(v2f i) : SV_Target
        {
            half3 paint = tex2D(_MainTex,i.uv).rgb;
            clip(.999-_Opacity);
            return fixed4(FinishPaint(i,paint)*.8, _Opacity*.16);
        }
        fixed4 fragFront(v2f i) : SV_Target
        {
            half3 paint = tex2D(_MainTex,i.uv).rgb;
            half pigment = FacePigment(i,paint);
            paint = FinishPaint(i,paint);
            half3 n = normalize(i.normal);
            half3 view = normalize(_WorldSpaceCameraPos.xyz-i.world);
            half3 key = normalize(half3(.64,.70,-.66));
            half3 back = normalize(half3(.60,.18,.70));
            half facing = saturate(dot(n,view)); half rim = (1-facing)*(1-facing);
            half wrapped = saturate((dot(n,key)+.45)/1.45);
            half brightness = max(paint.r,max(paint.g,paint.b));
            half body = smoothstep(.12,.35,brightness);
            half3 color = paint*(.80+.20*wrapped);
            color += paint*_SubsurfaceColor.rgb*_Subsurface*(1-wrapped)*body;
            color += paint*_Transmission*saturate(dot(n,back))*(.035+.09*rim)*body;
            half3 reflection = reflect(-view,n);
            half3 across = normalize(cross(key,half3(0,1,0))), along = cross(across,key);
            half rx = dot(reflection,across), ry = dot(reflection,along);
            half spread = lerp(60,18,saturate((_Roughness-.08)/.32));
            half softbox = exp2(-rx*rx*spread-ry*ry*spread*.48)*smoothstep(.5,.85,dot(reflection,key));
            half3 studio = texCUBE(_StudioCube,reflection).rgb;
            color += (studio*(.17+.33*rim) + half3(1,.98,.94)*softbox*.5)*_ReflectionStrength;
            half gold = smoothstep(.55,.8,paint.r)*smoothstep(.45,.65,paint.g)
                *(1-smoothstep(.16,.3,paint.b))*smoothstep(.68,.86,paint.g/max(paint.r,.001));
            color += half3(1,.65,.16)*gold*_SpeckEmission;
            half noise = frac(sin(dot(floor(i.local*100),float3(127.1,311.7,74.7)))*43758.5453);
            color += _GlitterStrength*smoothstep(.98,.997,noise)*(.3+.7*softbox)*half3(1,.81,.31)*(1-pigment);
            half3 pearl = .5+.5*cos(half3(0,2,4)+facing*6);
            color += _PearlSheen*pearl*(.05+.12*rim)*(1-pigment);
            color *= 1 - _Fuzz*(noise-.5)*.10*(1-pigment);
            color += _Fuzz*rim*half3(.10,.08,.12)*(1-pigment);
            color += _Metallic*studio*_FinishTint.rgb*.45*(1-pigment);
            half3 iridescent=.5+.5*cos(half3(.5,2.4,4.4)+facing*9+i.local.y*1.8);
            color=lerp(color,color*.7+iridescent*.35,_Iridescence*(.25+.75*rim)*(1-pigment));
            color += _Glow*paint*(.045+.13*rim)*(1-pigment);
            // Dark painted eyes/mouth remain readable; the rest is a translucent shell.
            half alpha = lerp(saturate(_Opacity+.22*rim),.98,pigment);
            half reflectedAlpha = _ReflectionStrength*(max(studio.r,max(studio.g,studio.b))*.55+softbox*.65);
            alpha = max(alpha,saturate(reflectedAlpha));
            return fixed4(saturate(color * _StageTint.rgb + _StageEmission * half3(.23,.65,.64)),alpha);
        }
        ENDCG
        Pass
        {
            Name "JellyBack" ZWrite Off Cull Front
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragBack
            #pragma target 3.0
            ENDCG
        }
        Pass
        {
            Name "JellyFront" ZWrite [_ZWrite] Cull Back
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment fragFront
            #pragma target 3.0
            ENDCG
        }
    }
    Fallback "Unlit/Texture"
}
