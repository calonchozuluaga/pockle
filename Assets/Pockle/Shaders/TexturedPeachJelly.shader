Shader "Pockle/Textured Peach Jelly"
{
    Properties
    {
        _MainTex ("Owner painted Peach", 2D) = "white" {}
        _StudioCube ("Studio reflections", Cube) = "black" {}
        _Roughness ("Wet roughness", Range(0.15,0.25)) = 0.20
        _Transmission ("Backlight transmission", Range(0,1)) = 0.90
        _Subsurface ("Soft scattering", Range(0,0.2)) = 0.05
        _SubsurfaceColor ("Warm peach scattering", Color) = (1,0.62,0.38,1)
        _SpeckEmission ("Painted gold glow", Range(0,0.5)) = 0.20
    }
    SubShader
    {
        // Opaque depth keeps a painted face readable and avoids sorting artifacts
        // during squish. Transmission is a lighting approximation, not alpha or
        // Blender refraction. One pass, no scene copy, no additional shell.
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            ZWrite On
            Cull Back
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"
            sampler2D _MainTex;
            samplerCUBE _StudioCube;
            half _Roughness, _Transmission, _Subsurface, _SpeckEmission;
            half4 _SubsurfaceColor;
            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; float2 uv : TEXCOORD0; };
            struct v2f { float4 vertex : SV_POSITION; float3 world : TEXCOORD0; half3 normal : TEXCOORD1; float2 uv : TEXCOORD2; };
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.world = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.uv = v.uv;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                half3 paint = tex2D(_MainTex, i.uv).rgb;
                half3 n = normalize(i.normal);
                half3 view = normalize(_WorldSpaceCameraPos.xyz - i.world);
                half3 key = normalize(half3(.64,.70,-.66));
                half3 back = normalize(half3(.60,.18,.70));
                half facing = saturate(dot(n,view));
                half rim = (1-facing)*(1-facing);
                half wrapped = saturate((dot(n,key)+.45)/1.45);
                half body = smoothstep(.12,.35,max(paint.r,max(paint.g,paint.b)));
                half3 color = paint * (.86 + .14*wrapped);
                color += paint * _SubsurfaceColor.rgb * _Subsurface * (1-wrapped) * body;
                color += paint * _Transmission * saturate(dot(n,back)) * (.035+.09*rim) * body;
                half3 reflection = reflect(-view,n);
                half3 across = normalize(cross(key,half3(0,1,0)));
                half3 along = cross(across,key);
                half rx = dot(reflection,across), ry = dot(reflection,along);
                half spread = lerp(65,38,saturate((_Roughness-.15)*10));
                half softbox = exp2(-rx*rx*spread-ry*ry*spread*.48) * smoothstep(.5,.85,dot(reflection,key));
                half3 studio = texCUBE(_StudioCube,reflection).rgb;
                half studioLight = max(studio.r,max(studio.g,studio.b));
                // The uploaded texture already contains highlights; use a small,
                // moving glaze so they do not bleach the painted peach crown.
                half glaze = saturate(softbox*.22 + studioLight*(.045+.055*rim));
                color = lerp(color,half3(1,.98,.94),glaze);
                half gold = smoothstep(.55,.8,paint.r) * smoothstep(.45,.65,paint.g)
                    * (1-smoothstep(.16,.3,paint.b)) * smoothstep(.68,.86,paint.g/max(paint.r,.001));
                color += half3(1,.65,.16)*gold*_SpeckEmission;
                return fixed4(saturate(color),1);
            }
            ENDCG
        }
    }
    Fallback "Unlit/Texture"
}
