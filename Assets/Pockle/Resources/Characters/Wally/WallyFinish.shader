Shader "Pockle/Wally Finish"
{
    Properties
    {
        _BrushTrailWeights ("Recovering stroke trail", Vector) = (0,0,0,0)
        _SurfaceKind ("Touch response: none / wobble / contents / nap", Float) = 0
        _ContentsPhase ("Interior flow angle", Float) = 0
        _BrushAmount ("Recovering nap", Range(0,1)) = 0
        _BrushFrom ("Brush segment start", Vector) = (0,0,0,0)
        _BrushTo ("Brush segment end", Vector) = (0,0,0,0)
        _BrushDirection ("Nap direction", Vector) = (0,1,0,0)

        _FusionDetailA ("First parent detail", Vector) = (0,0,0,0)
        _FusionDetailB ("Second parent detail", Vector) = (0,0,0,0)
        _FusionAccentA ("First parent accent", Vector) = (0,0,0,0)
        _FusionAccentB ("Second parent accent", Vector) = (0,0,0,0)
        _FusionPattern ("Fusion pattern: none / gradient / swirl / spots / ribbons", Float) = 0
        _FusionPigmentA ("First parent pigment", Color) = (1,1,1,1)
        _FusionPigmentB ("Second parent pigment", Color) = (1,1,1,1)
        _FusionSurfaceA ("First parent surface", Vector) = (0,0,0,1)
        _FusionSurfaceB ("Second parent surface", Vector) = (0,0,0,1)

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
        _MainTex ("Approved Wally paint", 2D) = "white" {}
        _StudioCube ("Studio reflections", Cube) = "black" {}
        _BodyColor ("Finish pigment", Color) = (1,.5,.2,1)
        _TintStrength ("Finish tint amount", Range(0,1)) = 0
        _WallyTail ("Tail splash", Float) = 0
        _WallyFlipper ("Flipper wave", Float) = 0
        _WallyIdle ("Gentle swimming", Float) = 0
        _GoldLeaf ("Broken gold leaf", Range(0,1)) = 0
        _Diamond ("Prismatic ice facets", Range(0,1)) = 0
        _Molten ("Obsidian neon veins", Range(0,1)) = 0
        _RarePulse ("Calm-aware vein luminance", Float) = 1
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
    #include "Assets/Pockle/Shaders/SurfacePlay.cginc"
    #include "Assets/Pockle/Shaders/StageGlow.cginc"
    #include "Assets/Pockle/Shaders/FusionPattern.cginc"
    half4 _StageTint; half _StageEmission; samplerCUBE _StageEnvironment; half _StageEnvironmentStrength;
    sampler2D _MainTex; samplerCUBE _StudioCube;
    half4 _BodyColor; half _TintStrength;
    half _GoldLeaf, _Diamond, _Molten, _RarePulse;
    half _WallyTail, _WallyFlipper, _WallyIdle;
    half _WinterPalette, _SurfaceRelief, _LoopPile;
    half _Gloss, _Opacity, _Fuzz, _Grain, _Metallic, _Pearl, _Crystal, _Iridescence, _Glitter;
    #include "Assets/Pockle/Shaders/TemperamentFace.cginc"
    struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; float2 uv:TEXCOORD0; float3 faceRest:TEXCOORD1; };
    struct v2f { float4 vertex:SV_POSITION; float3 world:TEXCOORD0; float3 local:TEXCOORD1; half3 normal:TEXCOORD2; float2 uv:TEXCOORD3; float3 face:TEXCOORD4; };
    v2f vert(appdata v)
    {
        float3 rest=v.vertex.xyz;
        // Broad tail mask behind the body; face and belly remain anchored.
        float tail=smoothstep(.15,.95,rest.z);
        float angle=(_WallyTail*.45+_WallyIdle*.035)*tail;
        float sn=sin(angle),cs=cos(angle);
        float2 fin=v.vertex.yz-float2(-.30,.15);
        v.vertex.yz=float2(cs*fin.x-sn*fin.y,sn*fin.x+cs*fin.y)+float2(-.30,.15);
        v.normal.yz=float2(cs*v.normal.y-sn*v.normal.z,sn*v.normal.y+cs*v.normal.z);
        // One flipper greets from its shoulder, with a soft joint mask on the single mesh.
        float flipper=smoothstep(.61,.88,rest.x)*(1-smoothstep(-.42,-.17,rest.y))
            *(1-smoothstep(.0,.40,rest.z));
        angle=_WallyFlipper*.65*flipper;sn=sin(angle);cs=cos(angle);
        float2 hand=v.vertex.xy-float2(.60,-.57);
        v.vertex.xy=float2(cs*hand.x-sn*hand.y,sn*hand.x+cs*hand.y)+float2(.60,-.57);
        v.normal.xy=float2(cs*v.normal.x-sn*v.normal.y,sn*v.normal.x+cs*v.normal.y);
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
        half visible=smoothstep(.10,.25,max(paint.r,max(paint.g,paint.b)));
        // Recolor ocean-blue skin, retaining the cream belly, blush and face ink.
        return visible*smoothstep(.04,.16,paint.b-paint.r)*smoothstep(-.01,.08,paint.b-paint.g);
    }

    fixed4 finish(v2f i, bool back)
    {
        half4 fusionSurface=FusionSurface(i.face,half4(_Gloss,_Metallic,_Fuzz,_Opacity));
            fusionSurface.z*=1-BrushMask(i.face)*.8;
            half4 fusionDetail=FusionDetail(i.face,half4(_Grain,_Pearl,_Iridescence,_Glitter));
            half4 fusionAccent=FusionAccent(i.face,half4(_SurfaceRelief,_LoopPile,_Crystal,0));
        half3 fusionPigment=FusionPigment(i.face,_BodyColor.rgb);

        half3 paint=TemperClean(i.face,tex2D(_MainTex,i.uv).rgb);
        // Clean the stray baked oval outside the native left eye without erasing the lower cheek.
        half stray=(1-smoothstep(1,1.2,length((i.face.xy-float2(-.55,-.16))/float2(.08,.18))))*TemperFront(i.face);
        paint=lerp(paint,TemperSkin(i.face,_TemperSkinLeft.xy),stray);
        half body=bodyMask(paint);
        if(back) { clip(.999-fusionSurface.w); clip(body-.3); }
        half3 n=TemperNormal(i.face,normalize(i.normal));
        half3 cleanSide=UnityObjectToWorldNormal(normalize(half3((i.face.x-.08)*.7,(i.face.y+.19)*.7,-1)));
        n=normalize(lerp(n,cleanSide,stray)); if(back) n=-n;
        // Body-local fibers/pile stay attached during rotation and animation.
        half surface = _WinterPalette > .5 ? 1-TemperFeatures(i.face).a : body;
        half3 fiber=0,curls=0;
        half poreAmount=0;
        // Uniform branches keep the additional surface work off smooth vinyl/jelly/metal.
        if (fusionAccent.x > .001)
        {
            if (_WinterPalette > .5 && fusionDetail.x > .5)
            {
                float4 pores=foamNoise(i.local*72);
                fiber=pores.yzw*.4;
                poreAmount=smoothstep(.55,.78,pores.x);
            }
            else
            {
                half fiberScale=lerp(110,48,fusionDetail.x);
                fiber=half3(hash(floor(i.local*fiberScale)),hash(floor(i.local.yzx*fiberScale)+17),hash(floor(i.local.zxy*fiberScale)+39))-.5;
            }
        }
        if (fusionAccent.y > .001) curls=sin(i.local.yzx*145+sin(i.local.zxy*81)*1.8);
        half3 relief = fiber*fusionAccent.x*.8+curls*fusionAccent.y*.20;
        n=normalize(n+surface*(relief-n*dot(n,relief)));
        half3 view=normalize(_WorldSpaceCameraPos.xyz-i.world);
        half facing=saturate(dot(n,view)); half rim=1-facing;
        half3 key=normalize(half3(.64,.70,-.66));
        half wrapped=saturate((dot(n,key)+.45)/1.45);
        half grain=hash(floor(i.local*480));
        half brush=sin(i.local.y*680+i.local.x*8)*.5+.5;
        half detail=dot(paint,half3(.3,.5,.2));
        half gill=saturate((paint.r-paint.g)*2.0);
        half3 pigment=fusionPigment*lerp((.65+.35*detail)*(1-gill*.23),.88+.12*detail,_WinterPalette);
        half3 base=lerp(paint,pigment,body*_TintStrength);
        base*=.70+.30*wrapped;
        base*=1-surface*((grain-.5)*(fusionSurface.z*.08+fusionDetail.x*.07)+fusionSurface.y*brush*.035);
        half3 reflected=reflect(-view,n);
        half3 studio=lerp(texCUBE(_StudioCube,reflected).rgb,texCUBE(_StageEnvironment,reflected).rgb,_StageEnvironmentStrength);
        half3 across=normalize(cross(key,half3(0,1,0))); half3 along=cross(across,key);
        half rx=dot(reflected,across), ry=dot(reflected,along);
        half spread=lerp(8,95,fusionSurface.x);
        half softbox=exp2(-rx*rx*spread-ry*ry*spread*.48)*smoothstep(.5,.85,dot(reflected,key));
        half glaze=(softbox*.52+max(studio.r,max(studio.g,studio.b))*.16)*lerp(.08,1,fusionSurface.x);
        half3 spec=lerp(half3(1,.98,.95),fusionPigment,fusionSurface.y*body*.8);
        base+=spec*glaze*(1-TemperFeatures(i.face).a);
        base+=body*fusionSurface.y*(studio*fusionPigment*.55+spec*softbox*.20);
        half loops = dot(curls,curls)/3;
        base *= 1-surface*(fusionAccent.y*(.10+.20*loops)+fusionAccent.x*fusionDetail.x*(.05+.10*abs(fiber.x)));
        base+=surface*fusionSurface.z*pow(rim,2)*lerp(half3(.25,.18,.14),fusionPigment*.30+half3(.12,.12,.12),_WinterPalette);
        base+=surface*fusionAccent.y*pow(saturate(1-loops),3)*.10;
        base*=1-surface*fusionDetail.x*_WinterPalette*poreAmount*.12;
        half3 pearl=.5+.5*cos(half3(0,2,4)+facing*6);
        base+=body*fusionDetail.y*pearl*(.06+.12*rim);
        half3 aurora=.5+.5*cos(half3(.5,2.4,4.4)+facing*9+i.local.y*1.8);
        base=lerp(base,base*.6+aurora*.45,body*fusionDetail.z*(.35+.65*rim));
        half facet=hash(floor(ContentsCoordinate(i.local)*24));
        base+=body*fusionAccent.z*smoothstep(.92,.99,facet)*(.06+.16*glaze)*half3(1,.7,.85);
        half fleck=smoothstep(.996,.999,hash(floor(ContentsCoordinate(i.local)*480)));
        base+=body*fusionDetail.w*fleck*half3(.8,.55,.16);
        base+=body*texCUBE(_StageEnvironment,refract(-view,n,.74)).rgb*_StageEnvironmentStrength*(1-fusionSurface.w)*.24;
        half rareMask=body*(1-TemperFeatures(i.face).a);
        if(_GoldLeaf>.001)
        {
            float leaf=foamNoise(i.face*25).x;
            half patch=smoothstep(.40,.53,leaf);
            base=lerp(base,half3(.70,.40,.08)*(.8+.2*wrapped),rareMask*(1-patch)*.50);
            base+=rareMask*patch*(studio*.22+softbox*.25)*half3(1,.78,.28);
        }
        if(_Diamond>.001)
        {
            float2 facetUV=i.face.xy*14+i.face.z*float2(2.3,1.7);
            float3 tile=float3(floor(facetUV),step(1,frac(facetUV.x)+frac(facetUV.y)));
            half3 facetNormal=normalize(n+half3(hash(tile)-.5,hash(tile+7)-.5,hash(tile+13)-.5)*.65);
            half glint=pow(saturate(dot(reflect(-view,facetNormal),key)),48);
            half facetTint=hash(tile+31);
            half3 prism=.65+.35*cos(half3(0,2.1,4.2)+facetTint*6.28+facing*4);
            base=lerp(base,base*(.92+.08*facetTint)+prism*(glint*.7+pow(rim,3)*.08),rareMask);
        }
        if(_Molten>.001)
        {
            // Nine-cell Voronoi edge distance: stable local-space cracks, no animated UV crawl.
            float2 p=i.face.xy*7+i.face.z*float2(2.3,1.7);
            float2 cell=floor(p),f=frac(p);float first=8,second=8;
            [unroll] for(int y=-1;y<=1;y++) [unroll] for(int x=-1;x<=1;x++)
            {
                float2 neighbour=float2(x,y);
                float2 seed=float2(hash(float3(cell+neighbour,1)),hash(float3(cell+neighbour,17)));
                float2 v=neighbour+seed-f;float d=dot(v,v);
                if(d<first){second=first;first=d;}else second=min(second,d);
            }
            half crack=1-smoothstep(.018,.085,second-first);
            half3 neon=lerp(half3(.08,.93,1),half3(1,.12,.58),smoothstep(-.5,.8,i.face.x+i.face.z*.5));
            half3 obsidian=half3(.075,.09,.14)*(.7+.3*wrapped)+studio*.14+softbox*.26;
            base=lerp(base,obsidian+neon*crack*_RarePulse*(.85+_StageEmission),rareMask);
        }
        half alpha=lerp(1,fusionSurface.w+(1-fusionSurface.w)*rim*.42,body);
        base=BrushNap(i.face,base,smoothstep(.05,.20,max(paint.r,max(paint.g,paint.b)))*(1-TemperFeatures(i.face).a));base+=ContentsLight(i.face,view)*body;
        if(back) alpha*=.32;
        else {
            base=TemperFinish(i.face,base);half4 faceInk=TemperFeatures(i.face);alpha=max(alpha,faceInk.a);
            // Pale mouth ink keeps the existing animated smile/frown readable on obsidian.
            float2 mouthDistance=(i.face.xy-_TemperMouth.xy)/max(_TemperMouth.zw*float2(2.1,2.5),.001);
            half mouthRegion=1-smoothstep(.7,1,dot(mouthDistance,mouthDistance));
            base=lerp(base,half3(.60,.94,1),faceInk.a*mouthRegion*_Molten);
        }
        return fixed4(saturate(base * _StageTint.rgb + PockleStageGlow(fusionPigment, _StageEmission, body * (1-TemperFeatures(i.face).a), facing)),alpha);
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
