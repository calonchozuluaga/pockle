Shader "Pockle/Textured Peach Jelly"
{
    Properties
    {
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
        sampler2D _MainTex;
        samplerCUBE _StudioCube;
        half _Roughness, _ReflectionStrength, _Opacity, _Transmission, _Subsurface, _SpeckEmission;
        half _Blink, _Happy, _Surprise;
        half4 _SubsurfaceColor;
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
        fixed4 fragBack(v2f i) : SV_Target
        {
            half3 paint = tex2D(_MainTex,i.uv).rgb;
            return fixed4(paint*.8, _Opacity*.16);
        }
        fixed4 fragFront(v2f i) : SV_Target
        {
            half3 paint = tex2D(_MainTex,i.uv).rgb;
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
            // Dark painted eyes/mouth remain readable; the rest is a translucent shell.
            half face = saturate(Region(i.local.xy,_LeftEye)+Region(i.local.xy,_RightEye)+Region(i.local.xy,_Mouth))
                * smoothstep(.45,.62,-i.local.z);
            half pigment = face*(1-smoothstep(.15,.45,brightness));
            half alpha = lerp(saturate(_Opacity+.22*rim),.98,pigment);
            half reflectedAlpha = _ReflectionStrength*(max(studio.r,max(studio.g,studio.b))*.55+softbox*.65);
            alpha = max(alpha,saturate(reflectedAlpha));
            return fixed4(saturate(color),alpha);
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
            Name "JellyFront" ZWrite Off Cull Back
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
