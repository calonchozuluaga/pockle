Shader "Pockle/Holiday Finish"
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
        _MainTex ("Approved Christmas paint", 2D) = "white" {}
        _StudioCube ("Studio reflections", Cube) = "black" {}
        _BodyColor ("Finish pigment", Color) = (1,.5,.2,1)
        _PigmentKind ("Pigment region", Float) = 0
        _MotionKind ("Character motion", Float) = 0
        _LeftEyeCenter ("Left eye", Vector) = (-.25,0,.12,.15)
        _RightEyeCenter ("Right eye", Vector) = (.25,0,.12,.15)
        _MouthCenter ("Mouth", Vector) = (0,-.1,.12,.08)
        _TintStrength ("Finish tint amount", Range(0,1)) = 0
        _Blink ("Blink", Range(0,1)) = 0
        _Happy ("Happy", Range(0,1)) = 0
        _Surprise ("Surprise", Range(0,1)) = 0
        _IdleWave ("Gill and arm wave", Float) = 0
        _FirbBranch ("Firb branch greeting", Float) = 0
        _FirbStar ("Firb star nod", Float) = 0
        _FirbOrnaments ("Firb ornament shimmy", Float) = 0
        _CocoCream ("Coco cream wobble", Float) = 0
        _CocoMarshmallows ("Coco marshmallow bounce", Float) = 0
        _GingerPaw ("Ginger paw greeting", Float) = 0
        _GingerEars ("Ginger ear wiggle", Float) = 0
        _GingerHead ("Ginger head bow", Float) = 0
        _SnowyMitten ("Snowy mitten greeting", Float) = 0
        _SnowyPom ("Snowy pom wobble", Float) = 0
        _SnowyHead ("Snowy cozy nod", Float) = 0
        _WinterWave ("Winter greeting", Float) = 0
        _WinterFeature ("Antler earmuff or holly wiggle", Float) = 0
        _WinterNod ("Winter bow", Float) = 0
        _Gloss ("Glaze", Range(0,1)) = .3
        _Opacity ("Body opacity", Range(0,1)) = 1
        _WinterPalette ("Winter finish pigment mask", Float) = 0
        _SurfaceRelief ("Phone-scale surface relief", Range(0,1)) = 0
        _LoopPile ("Curled boucle pile", Range(0,1)) = 0
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
    half _PigmentKind, _MotionKind; float4 _LeftEyeCenter,_RightEyeCenter,_MouthCenter; half4 _BodyColor; half _TintStrength, _Blink, _Happy, _Surprise, _IdleWave;
    half _FirbBranch, _FirbStar, _FirbOrnaments;
    half _CocoCream, _CocoMarshmallows;
    half _GingerPaw, _GingerEars, _GingerHead;
    half _SnowyMitten, _SnowyPom, _SnowyHead;
    half _WinterWave, _WinterFeature, _WinterNod;
    half _WinterPalette, _SurfaceRelief, _LoopPile;
    half _Gloss, _Opacity, _Fuzz, _Grain, _Metallic, _Pearl, _Crystal, _Iridescence, _Glitter;
    #include "Assets/Pockle/Shaders/TemperamentFace.cginc"
    struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; float2 uv:TEXCOORD0; float3 faceRest:TEXCOORD1; };
    struct v2f { float4 vertex:SV_POSITION; float3 world:TEXCOORD0; float3 local:TEXCOORD1; half3 normal:TEXCOORD2; float2 uv:TEXCOORD3; float3 face:TEXCOORD4; };
    v2f vert(appdata v)
    {
        float3 rest=v.vertex.xyz;
        float gill=smoothstep(.58,.85,abs(rest.x))*smoothstep(.03,.28,rest.y);
        float arm=smoothstep(.44,.70,abs(rest.x))*(1-smoothstep(-.38,-.10,rest.y))*smoothstep(-.65,-.42,rest.y);
        float height=saturate((rest.y+1)/1.94);
        if(_MotionKind<.5)
        {
            // Spatial masks keep the face and feet anchored; the painted details travel with each tier.
            float branch=smoothstep(.27,.58,rest.x)*(1-smoothstep(.15,.38,rest.y))*smoothstep(-.62,-.38,rest.y);
            v.vertex.y+=branch*_FirbBranch*.15;
            v.vertex.x-=branch*_FirbBranch*.035;
            float star=smoothstep(.52,.71,rest.y);
            float angle=_FirbStar*.20*star, sn=sin(angle), cs=cos(angle);
            float2 tip=v.vertex.xy-float2(0,.55);
            v.vertex.xy=float2(cs*tip.x-sn*tip.y,sn*tip.x+cs*tip.y)+float2(0,.55);
            v.normal.xy=float2(cs*v.normal.x-sn*v.normal.y,sn*v.normal.x+cs*v.normal.y);
            float tiers=smoothstep(.28,.55,abs(rest.x))*smoothstep(-.68,-.42,rest.y)*(1-smoothstep(.30,.55,rest.y));
            v.vertex.z+=_FirbOrnaments*tiers*sign(rest.x)*.055;
            v.vertex.x+=_IdleWave*height*height*.022;
        }
        else if(_MotionKind<1.5)
        {
            // Above the cocoa rim only: the cup, heart, feet and handle stay rigid.
            float topping=smoothstep(.27,.39,rest.y);
            float cream=topping*smoothstep(.39,.57,rest.y);
            float marshmallow=topping*(1-smoothstep(.43,.61,rest.y));
            float angle=_CocoCream*.22*cream, sn=sin(angle), cs=cos(angle);
            float2 tip=v.vertex.xy-float2(-.22,.38);
            v.vertex.xy=float2(cs*tip.x-sn*tip.y,sn*tip.x+cs*tip.y)+float2(-.22,.38);
            v.normal.xy=float2(cs*v.normal.x-sn*v.normal.y,sn*v.normal.x+cs*v.normal.y);
            v.vertex.y+=_CocoMarshmallows*marshmallow*.065;
            v.vertex.x+=_IdleWave*height*.014;
        }
        else if(_MotionKind<2.5)
        {
            // Mitten pivots below the scarf; the head nod leaves the scarf, torso and feet planted.
            float mitten=smoothstep(.31,.53,rest.x)*smoothstep(-.65,-.48,rest.y)*(1-smoothstep(-.23,-.10,rest.y));
            float angle=_SnowyMitten*.65*mitten, sn=sin(angle), cs=cos(angle);
            float2 hand=v.vertex.xy-float2(.32,-.25);
            v.vertex.xy=float2(cs*hand.x-sn*hand.y,sn*hand.x+cs*hand.y)+float2(.32,-.25);
            v.normal.xy=float2(cs*v.normal.x-sn*v.normal.y,sn*v.normal.x+cs*v.normal.y);
            float hat=smoothstep(.43,.64,rest.y), pom=smoothstep(.66,.80,rest.y);
            angle=_SnowyPom*.10*hat; sn=sin(angle); cs=cos(angle);
            float2 cap=v.vertex.xy-float2(0,.43);
            v.vertex.xy=float2(cs*cap.x-sn*cap.y,sn*cap.x+cs*cap.y)+float2(0,.43);
            v.normal.xy=float2(cs*v.normal.x-sn*v.normal.y,sn*v.normal.x+cs*v.normal.y);
            v.vertex.x+=_SnowyPom*pom*.035;
            float head=smoothstep(-.08,.11,rest.y);
            angle=-_SnowyHead*.10*head; sn=sin(angle); cs=cos(angle);
            float2 nod=v.vertex.yz-float2(-.08,0);
            v.vertex.yz=float2(cs*nod.x-sn*nod.y,sn*nod.x+cs*nod.y)+float2(-.08,0);
            v.normal.yz=float2(cs*v.normal.y-sn*v.normal.z,sn*v.normal.y+cs*v.normal.z);
            v.vertex.y+=_IdleWave*smoothstep(.35,.60,abs(rest.x))*(1-smoothstep(-.2,.1,rest.y))*.025;
        }
        else if(_MotionKind<3.5)
        {
            // Local joints keep the torso, iced buttons and planted feet out of the articulation masks.
            float paw=smoothstep(.32,.54,rest.x)*smoothstep(-.66,-.48,rest.y)*(1-smoothstep(-.22,-.08,rest.y));
            float angle=_GingerPaw*.65*paw, sn=sin(angle), cs=cos(angle);
            float2 limb=v.vertex.xy-float2(.34,-.25);
            v.vertex.xy=float2(cs*limb.x-sn*limb.y,sn*limb.x+cs*limb.y)+float2(.34,-.25);
            v.normal.xy=float2(cs*v.normal.x-sn*v.normal.y,sn*v.normal.x+cs*v.normal.y);
            float ear=smoothstep(.40,.57,rest.y)*smoothstep(.30,.47,abs(rest.x));
            angle=_GingerEars*.18*ear*sign(rest.x); sn=sin(angle); cs=cos(angle);
            float2 crown=v.vertex.xy-float2(sign(rest.x)*.42,.52);
            v.vertex.xy=float2(cs*crown.x-sn*crown.y,sn*crown.x+cs*crown.y)+float2(sign(rest.x)*.42,.52);
            v.normal.xy=float2(cs*v.normal.x-sn*v.normal.y,sn*v.normal.x+cs*v.normal.y);
            float head=smoothstep(-.18,.04,rest.y);
            angle=-_GingerHead*.24*head; sn=sin(angle); cs=cos(angle);
            float2 nod=v.vertex.yz-float2(-.18,0);
            v.vertex.yz=float2(cs*nod.x-sn*nod.y,sn*nod.x+cs*nod.y)+float2(-.18,0);
            v.normal.yz=float2(cs*v.normal.y-sn*v.normal.z,sn*v.normal.y+cs*v.normal.z);
            v.vertex.y+=_IdleWave*smoothstep(.4,.65,abs(rest.x))*.02;
        }
        else
        {
            bool deer=_MotionKind<4.5, penguin=_MotionKind>=4.5&&_MotionKind<5.5;
            float lower=deer?-.65:penguin?-.60:-.76, upper=deer?-.25:penguin?-.04:-.25;
            float limb=smoothstep(deer?.31:penguin?.53:.64,deer?.48:penguin?.72:.82,rest.x)
                *smoothstep(lower-.10,lower,rest.y)*(1-smoothstep(upper,upper+.12,rest.y));
            float angle=_WinterWave*.48*limb, sn=sin(angle), cs=cos(angle);
            float2 joint=float2(deer?.37:penguin?.56:.67,upper);
            float2 arm=v.vertex.xy-joint;
            v.vertex.xy=float2(cs*arm.x-sn*arm.y,sn*arm.x+cs*arm.y)+joint;
            v.normal.xy=float2(cs*v.normal.x-sn*v.normal.y,sn*v.normal.x+cs*v.normal.y);
            // Separate high/side masks keep face and feet still while the character's feature moves.
            float feature=deer?smoothstep(.53,.70,rest.y):penguin?
                smoothstep(.45,.63,abs(rest.x))*smoothstep(.05,.24,rest.y):smoothstep(.39,.56,rest.y);
            angle=_WinterFeature*(deer?.11:penguin?.07:.12)*feature;
            sn=sin(angle);cs=cos(angle);
            float2 crown=v.vertex.xy-float2(0,deer?.53:penguin?.30:.39);
            v.vertex.xy=float2(cs*crown.x-sn*crown.y,sn*crown.x+cs*crown.y)+float2(0,deer?.53:penguin?.30:.39);
            v.normal.xy=float2(cs*v.normal.x-sn*v.normal.y,sn*v.normal.x+cs*v.normal.y);
            float head=smoothstep(deer?-.19:penguin?-.55:-.68,deer?.02:penguin?-.18:-.35,rest.y);
            angle=-_WinterNod*(deer?.18:penguin?.12:.10)*head;sn=sin(angle);cs=cos(angle);
            float2 nod=v.vertex.yz-float2(deer?-.19:penguin?-.55:-.68,0);
            v.vertex.yz=float2(cs*nod.x-sn*nod.y,sn*nod.x+cs*nod.y)+float2(deer?-.19:penguin?-.55:-.68,0);
            v.normal.yz=float2(cs*v.normal.y-sn*v.normal.z,sn*v.normal.y+cs*v.normal.z);
            v.vertex.y+=_IdleWave*limb*.012;
        }
        v2f o; o.vertex=UnityObjectToClipPos(v.vertex); o.world=mul(unity_ObjectToWorld,v.vertex).xyz;
        o.local=v.vertex.xyz; o.normal=UnityObjectToWorldNormal(v.normal); o.uv=v.uv; o.face=v.faceRest; return o;
    }
    float hash(float3 p) { return frac(sin(dot(p,float3(127.1,311.7,74.7)))*43758.5453); }
    float4 foamNoise(float3 p)
    {
        float3 cell=floor(p),f=frac(p),u=f*f*(3-2*f),du=6*f*(1-f);
        float a=hash(cell),b=hash(cell+float3(1,0,0)),c=hash(cell+float3(0,1,0)),d=hash(cell+float3(1,1,0));
        float e=hash(cell+float3(0,0,1)),g=hash(cell+float3(1,0,1)),h=hash(cell+float3(0,1,1)),j=hash(cell+float3(1,1,1));
        float x00=lerp(a,b,u.x),x10=lerp(c,d,u.x),x01=lerp(e,g,u.x),x11=lerp(h,j,u.x);
        float z0=lerp(x00,x10,u.y),z1=lerp(x01,x11,u.y);
        float3 gradient=float3(lerp(lerp(b-a,d-c,u.y),lerp(g-e,j-h,u.y),u.z)*du.x,
            lerp(x10-x00,x11-x01,u.z)*du.y,(z1-z0)*du.z);
        return float4(lerp(z0,z1,u.z),gradient);
    }
    half bodyMask(half3 paint)
    {
        half bright=smoothstep(.13,.36,max(paint.r,max(paint.g,paint.b)));
        if (_WinterPalette > .5)
        {
            // Preserve cream bibs/muzzles, dark features and saturated red accessories.
            half visible = smoothstep(.055,.20,max(paint.r,max(paint.g,paint.b)));
            if (_PigmentKind > 2.5)
                return visible * smoothstep(.16,.38,(paint.b-paint.r)/(paint.b+.03));
            half warmth = (paint.r-paint.g)/(paint.r+.03);
            return visible * smoothstep(.14,.31,warmth) * (1-smoothstep(.72,.9,warmth))
                * smoothstep(.10,.27,(paint.r-paint.b)/(paint.r+.03));
        }
        if(_PigmentKind<.5)return bright*smoothstep(.015,.12,paint.g-paint.r)*smoothstep(-.02,.10,paint.g-paint.b);
        if(_PigmentKind<1.5)return bright*smoothstep(.04,.18,paint.r-paint.g)*smoothstep(.03,.15,paint.r-paint.b)*(1-smoothstep(.65,.88,paint.r));
        if(_PigmentKind>2.5)return bright*smoothstep(.02,.12,paint.b-paint.r)*smoothstep(-.01,.06,paint.b-paint.g);
        return bright*(1-smoothstep(.08,.22,max(paint.r,max(paint.g,paint.b))-min(paint.r,min(paint.g,paint.b))))*smoothstep(.42,.65,min(paint.r,min(paint.g,paint.b)));
    }
    fixed4 finish(v2f i, bool back)
    {
        half3 paint=TemperClean(i.face,tex2D(_MainTex,i.uv).rgb); half body=bodyMask(paint);
        if (_WinterPalette > .5)
        {
            half front = 1-smoothstep(_TemperFront-.02,_TemperFront+.08,i.face.z);
            body *= lerp(1,TemperKeepNose(i.face.xy),front);
            if (_MotionKind < 4.5) body *= smoothstep(-.83,-.70,i.face.y);
            if (_MotionKind > 5.5) body *= 1-smoothstep(.34,.43,i.face.y);
        }
        if(back) { clip(.999-_Opacity); clip(body-.3); }
        half3 n=TemperNormal(i.face,normalize(i.normal)); if(back) n=-n;
        // Body-local fibers/pile stay attached during rotation and animation.
        half surface = _WinterPalette > .5 ? 1-TemperFeatures(i.face).a : body;
        half3 fiber=0,curls=0;
        half poreAmount=0;
        // Uniform branches keep the additional surface work off smooth vinyl/jelly/metal.
        if (_SurfaceRelief > .001)
        {
            if (_WinterPalette > .5 && _Grain > .5)
            {
                float4 pores=foamNoise(i.local*72);
                fiber=pores.yzw*.4;
                poreAmount=smoothstep(.55,.78,pores.x);
            }
            else
            {
                half fiberScale=lerp(110,48,_Grain);
                fiber=half3(hash(floor(i.local*fiberScale)),hash(floor(i.local.yzx*fiberScale)+17),hash(floor(i.local.zxy*fiberScale)+39))-.5;
            }
        }
        if (_LoopPile > .001) curls=sin(i.local.yzx*145+sin(i.local.zxy*81)*1.8);
        half3 relief = fiber*_SurfaceRelief*.8+curls*_LoopPile*.20;
        n=normalize(n+surface*(relief-n*dot(n,relief)));
        half3 view=normalize(_WorldSpaceCameraPos.xyz-i.world);
        half facing=saturate(dot(n,view)); half rim=1-facing;
        half3 key=normalize(half3(.64,.70,-.66));
        half wrapped=saturate((dot(n,key)+.45)/1.45);
        half grain=hash(floor(i.local*480));
        half brush=sin(i.local.y*680+i.local.x*8)*.5+.5;
        half detail=dot(paint,half3(.3,.5,.2));
        half gill=saturate((paint.r-paint.g)*2.0);
        half3 pigment=_BodyColor.rgb*lerp((.65+.35*detail)*(1-gill*.23),.88+.12*detail,_WinterPalette);
        half3 base=lerp(paint,pigment,body*_TintStrength);
        base*=.70+.30*wrapped;
        base*=1-surface*((grain-.5)*(_Fuzz*.08+_Grain*.07)+_Metallic*brush*.035);
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
        half loops = dot(curls,curls)/3;
        base *= 1-surface*(_LoopPile*(.10+.20*loops)+_SurfaceRelief*_Grain*(.05+.10*abs(fiber.x)));
        base+=surface*_Fuzz*pow(rim,2)*lerp(half3(.25,.18,.14),_BodyColor.rgb*.30+half3(.12,.12,.12),_WinterPalette);
        base+=surface*_LoopPile*pow(saturate(1-loops),3)*.10;
        base*=1-surface*_Grain*_WinterPalette*poreAmount*.12;
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
        return fixed4(saturate(base * _StageTint.rgb + _StageEmission * _BodyColor.rgb * body),alpha);
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
