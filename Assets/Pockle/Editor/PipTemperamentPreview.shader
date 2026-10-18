Shader "Pockle/Editor/Pip Temperament Preview"
{
    Properties
    {
        _StageEnvironment ("Painted environment", Cube) = "black" {}
        _StageEnvironmentStrength ("Environment amount", Range(0,1)) = 0
        _StageTint ("Stage illumination", Color) = (1,1,1,1)
        _StageEmission ("Night glow", Float) = 0
        _FusionMode ("Fusion study: none / dusk / sorbet / honeydew / eclipse", Float) = 0
        _FusionColorA ("Fusion first color", Color) = (1,1,1,1)
        _FusionColorB ("Fusion second color", Color) = (1,1,1,1)
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
        _PreviewFace ("Canonical front paint bake", 2D) = "white" {}
        _Lids ("Preview lids", Range(0,1)) = 0
        _Frown ("Preview frown", Range(0,1)) = 0
        _GlanceX ("Preview horizontal gaze", Range(-1,1)) = 0
        _GlanceY ("Preview vertical gaze", Range(-1,1)) = 0
        _Blush ("Preview cheek blush", Range(0,1)) = 0
        _Wink ("Preview left-eye wink", Range(0,1)) = 0
        _MouthScale ("Preview mouth size", Range(.5,1.5)) = 1
        _LidAngle ("Preview lid angle: inner-down positive", Range(-1,1)) = 0
        _SleepCurve ("Preview sleeping closure", Range(0,1)) = 0
        _MouthOpen ("Preview round surprise mouth", Range(0,1)) = 0
        _MouthTilt ("Preview crooked smile", Range(-1,1)) = 0
        _HighlightStyle ("Preview glint: normal / double / star", Range(0,2)) = 0
        _LeftEye ("Left eye region", Vector) = (-0.284,-0.06,0.15,0.17)
        _RightEye ("Right eye region", Vector) = (0.32,-0.06,0.15,0.17)
        _Mouth ("Mouth region", Vector) = (0.02,-0.125,0.12,0.09)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        CGINCLUDE
        #include "UnityCG.cginc"
    half4 _StageTint; half _StageEmission; samplerCUBE _StageEnvironment; half _StageEnvironmentStrength;
        sampler2D _MainTex, _PreviewFace;
        samplerCUBE _StudioCube;
        half _Roughness, _ReflectionStrength, _Opacity, _Transmission, _Subsurface, _SpeckEmission;
        half _Blink, _Happy, _Surprise;
        half _Lids, _Frown, _GlanceX, _GlanceY, _Blush, _Wink, _MouthScale;
        half4 _SubsurfaceColor, _FinishTint, _FusionColorA, _FusionColorB;
        half _FusionMode;
        half _TintStrength, _GlitterStrength, _PearlSheen, _Fuzz, _Metallic, _Iridescence, _Glow;
        half _LidAngle, _SleepCurve, _MouthOpen, _MouthTilt, _HighlightStyle;
        float4 _LeftEye, _RightEye, _Mouth;
        struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; float2 uv : TEXCOORD0; };
        struct v2f { float4 vertex : SV_POSITION; float3 world : TEXCOORD0; half3 normal : TEXCOORD1; float2 uv : TEXCOORD2; float3 local : TEXCOORD3; };
        float Region(float2 position, float4 feature)
        {
            float2 d = (position - feature.xy) / feature.zw;
            return 1 - smoothstep(.45,1,dot(d,d));
        }
        v2f vert(appdata v)
        {
            v2f o; o.local = v.vertex.xyz;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.world = mul(unity_ObjectToWorld,v.vertex).xyz;
            o.normal = UnityObjectToWorldNormal(v.normal); o.uv = v.uv;
            return o;
        }
        // Pass02 authors clean local face paint instead of scaling baked atlas islands.
        // The source model/texture and production shaders are never modified.
        half3 FaceSample(float2 target) { return tex2D(_PreviewFace,target/2.4+.5).rgb; }
        half FaceMask(float2 p,float4 feature)
        {
            float2 d=(p-feature.xy)/feature.zw;
            return 1-smoothstep(1.10,1.75,dot(d,d));
        }
        half Stroke(float distance,float width)
        {
            float aa=max(fwidth(distance),.0012);
            return 1-smoothstep(width-aa,width+aa,abs(distance));
        }
        half Disc(float2 p,float radius)
        {
            float aa=max(fwidth(length(p)),.0012);
            return 1-smoothstep(radius-aa,radius+aa,length(p));
        }
        half Star(float2 p)
        {
            float angle=atan2(p.y,p.x);
            float radius=.008+.020*pow(abs(cos(angle*2)),10);
            return Disc(p,radius);
        }
        half3 PreviewEye(v2f i,half3 paint,float4 eye,half wink,half side)
        {
            // 0.09 local units is 6.4% of the approximately 1.4-unit face width.
            float2 centre=eye.xy+float2(_GlanceX*.09,_GlanceY*.09);
            float2 d=i.local.xy-centre;
            float2 radius=float2(.098,.125)*(1+_Surprise*.08);
            float ellipse=length(d/radius);
            float aa=max(fwidth(ellipse),.01);
            half open=1-smoothstep(1-aa,1+aa,ellipse);
            float lid=radius.y*(1-2*_Lids)-_LidAngle*side*d.x*.65;
            open*=1-smoothstep(lid-.002,lid+.002,d.y);
            half closure=max(_Blink,wink);
            half closed=smoothstep(.45,.90,closure);
            float x=d.x/radius.x;
            float curve=lerp(-.026+.038*x*x,.020-.035*x*x,_SleepCurve);
            half closedStroke=Stroke(d.y-curve,.007)*(1-smoothstep(.88,1.03,abs(x)));
            half coverage=lerp(open,closedStroke,closed)*smoothstep(.25,.50,-i.local.z);
            half3 ink=lerp(half3(.045,.012,.030),half3(.22,.075,.14),saturate(-d.y/radius.y));
            float2 glint=d-float2(.028,.043);
            half normal=Disc(glint,.021);
            half doubleGlint=max(normal,Disc(d-float2(-.028,-.020),.010));
            half starGlint=Star(glint);
            half sparkle=lerp(lerp(normal,doubleGlint,saturate(_HighlightStyle)),starGlint,saturate(_HighlightStyle-1));
            sparkle*=1-smoothstep(.70,.82,max(_Lids,closure));
            ink=lerp(ink,half3(1,.97,.91),sparkle);
            return lerp(paint,ink,coverage);
        }
        half3 PreviewPaint(v2f i)
        {
            half3 paint=tex2D(_MainTex,i.uv).rgb;
            half front=smoothstep(.25,.50,-i.local.z);
            half frontSkin=front*(1-smoothstep(.62,.90,abs(i.local.x)))*(1-smoothstep(.35,.65,abs(i.local.y+.06)));
            paint=lerp(paint,FaceSample(float2(0,.25)),frontSkin);
            paint=PreviewEye(i,paint,_LeftEye,_Wink,1);
            paint=PreviewEye(i,paint,_RightEye,0,-1);
            float2 d=i.local.xy-(_Mouth.xy+float2(0,-.055));
            float width=.075*_MouthScale*(1+_Happy*.65);
            float x=d.x/max(width,.01);
            float curve=lerp(-.035+.034*x*x,.022-.034*x*x,_Frown)+_MouthTilt*d.x*.38;
            half smile=Stroke(d.y-curve,.007)*(1-smoothstep(.88,1.04,abs(x)));
            float2 o=d-float2(0,-.018);
            half roundMouth=Disc(o/float2(.032,.041),1);
            half mouth=lerp(smile,roundMouth,_MouthOpen)*front;
            return lerp(paint,half3(.04,.008,.022),mouth);
        }
        half FacePigment(v2f i, half3 paint)
        {
            half brightness=max(paint.r,max(paint.g,paint.b));
            half face=smoothstep(.25,.50,-i.local.z)*(1-smoothstep(.55,.80,abs(i.local.x)))*(1-smoothstep(.30,.50,abs(i.local.y)));
            return face*(1-smoothstep(.15,.45,brightness));
        }
        half3 FinishPaint(v2f i,half3 paint)
        {
            half brightness=max(paint.r,max(paint.g,paint.b));
            half luminance=dot(paint,half3(.299,.587,.114));
            half minimum=min(paint.r,min(paint.g,paint.b));
            half whiteHighlight=smoothstep(.90,.98,brightness)*(1-smoothstep(.04,.15,brightness-minimum));
            half3 tint=_FinishTint.rgb;
            if(_FusionMode>.5)
            {
                half mixAmount=smoothstep(-.75,.75,i.local.y);
                if(_FusionMode>1.5&&_FusionMode<2.5)mixAmount=smoothstep(-.65,.65,sin(i.local.y*7+i.local.x*4+sin(i.local.z*5+i.local.x*3)*1.2));
                if(_FusionMode>2.5&&_FusionMode<3.5)
                    mixAmount=saturate(smoothstep(.38,.90,i.local.y)*.75+(1-smoothstep(-.85,-.45,i.local.y))*.60+pow(sin(i.local.x*4+i.local.y*3+i.local.z*2)*.5+.5,8)*.18);
                tint=lerp(_FusionColorA.rgb,_FusionColorB.rgb,mixAmount);
            }
            return lerp(paint,tint*(.55+.45*luminance),_TintStrength*(1-FacePigment(i,paint))*(1-whiteHighlight));
        }
        fixed4 fragBack(v2f i) : SV_Target
        {
            half3 paint = PreviewPaint(i);
            clip(.999-_Opacity);
            return fixed4(FinishPaint(i,paint)*.8, _Opacity*.16);
        }
        fixed4 fragFront(v2f i) : SV_Target
        {
            half3 paint = PreviewPaint(i);
            half pigment = FacePigment(i,paint);
            paint = FinishPaint(i,paint);
            half3 n = normalize(i.normal);
            half frontNormal=smoothstep(.25,.50,-i.local.z)*(1-smoothstep(.62,.90,abs(i.local.x)))*(1-smoothstep(.35,.65,abs(i.local.y+.06)));
            n=normalize(lerp(n,normalize(half3(i.local.x*.8,i.local.y*.8,-1)),frontNormal));
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
            half3 studio = lerp(texCUBE(_StudioCube,reflection).rgb,texCUBE(_StageEnvironment,reflection).rgb,_StageEnvironmentStrength);
            color += (studio*(.17+.33*rim) + half3(1,.98,.94)*softbox*.5)*_ReflectionStrength*(1-pigment);
            half3 transmitted=texCUBE(_StageEnvironment,refract(-view,n,.74)).rgb;
            color+=transmitted*_StageEnvironmentStrength*_Transmission*(1-_Opacity)*.28*(1-pigment);
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
            if(_FusionMode>3.5)
            {
                // A ray-offset local sample suggests a ribbon suspended beneath the shell, without extra draw calls.
                float3 localView=normalize(mul((float3x3)unity_WorldToObject,view));
                float3 inside=i.local-localView*.16;
                half ribbon=pow(saturate(1-abs(sin(inside.y*5+inside.x*1.7+inside.z*2.2+sin(inside.x*3+inside.z*2)*.75))),10);
                half3 gold=half3(1,.66,.18);
                color=lerp(color,color*.40+gold*.72,ribbon*.72*(1-pigment));
                color+=gold*ribbon*_Glow*.32*(1-pigment);
            }
            float4 cheekA=float4(-.22,-.29,.13,.075),cheekB=float4(.25,-.29,.13,.075);
            half cheeks=max(Region(i.local.xy,cheekA),Region(i.local.xy,cheekB))*smoothstep(.45,.62,-i.local.z);
            color=lerp(color,half3(1,.21,.37),cheeks*_Blush*.72);
            // Dark painted eyes/mouth remain readable; the rest is a translucent shell.
            half alpha = lerp(saturate(_Opacity+.22*rim),.98,pigment);
            half reflectedAlpha = _ReflectionStrength*(max(studio.r,max(studio.g,studio.b))*.55+softbox*.65);
            alpha = max(alpha,saturate(reflectedAlpha));
            return fixed4(saturate(color * _StageTint.rgb + _StageEmission * half3(.23,.65,.64) * (1-pigment)),alpha);
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
