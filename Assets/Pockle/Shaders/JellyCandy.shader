Shader "Pockle/Jelly Candy"
{
    Properties
    {
        _Color ("Tint / opacity", Color) = (1, 0.43, 0.34, 0.91)
        _TopColor ("Top tint", Color) = (1, 0.71, 0.49, 1)
        _BottomColor ("Base tint", Color) = (0.89, 0.20, 0.23, 1)
        _Glossiness ("Soft glaze", Range(0,1)) = 0.48
        _RimColor ("Candy edge", Color) = (1, 0.86, 0.67, 1)
        _ReflectionStrength ("Studio reflections", Range(0,1)) = 1
        [Toggle] _ZWrite ("Write depth for opaque accents", Float) = 0
        _StudioCube ("Studio reflection", Cube) = "" {}
        _StudioStrength ("Studio reflection strength", Range(0,1)) = 0
        _PearlSheen ("Pearlescent finish", Range(0,1)) = 0
        _Softness ("Soft opaque finish", Range(0,1)) = 0
        _GlitterStrength ("Microflake glitter", Range(0,1)) = 0
        _GlitterDensity ("Glitter density", Range(24,160)) = 96
        _GlitterColor ("Glitter tint", Color) = (1, .87, .44, 1)
        _Transmission ("Soft transmitted light", Range(0,1)) = 0
        _EdgeLight ("Saturated thin edges", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 150
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite [_ZWrite]
        Cull Back
        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"

            fixed4 _Color;
            fixed4 _TopColor;
            fixed4 _BottomColor;
            fixed4 _RimColor;
            half _Glossiness;
            half _ReflectionStrength;
            samplerCUBE _StudioCube;
            half _StudioStrength;
            half _PearlSheen;
            half _Softness;
            half _GlitterStrength;
            float _GlitterDensity;
            fixed4 _GlitterColor;
            half _Transmission;
            half _EdgeLight;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };
            struct v2f
            {
                float4 position : SV_POSITION;
                float3 worldPosition : TEXCOORD0;
                half3 normal : TEXCOORD1;
                half height : TEXCOORD2;
                float2 uv : TEXCOORD3;
            };

            v2f vert(appdata input)
            {
                v2f output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.worldPosition = mul(unity_ObjectToWorld, input.vertex).xyz;
                output.normal = UnityObjectToWorldNormal(input.normal);
                output.height = saturate((input.vertex.y + 1.0h) / 1.68h);
                output.uv = input.uv;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                half3 normal = normalize(input.normal);
                half3 view = normalize(_WorldSpaceCameraPos.xyz - input.worldPosition);
                // A quiet studio key keeps the little toy welcoming under any scene light.
                // This cheap transmitted-light approximation uses no scene color grab,
                // live scene capture or additional lighting passes.
                half3 key = normalize(half3(0.64h, 0.70h, -0.66h));
                half3 fill = normalize(half3(0.60h, 0.18h, 0.70h));
                half light = saturate(dot(normal, key)) * 0.18h + 0.82h;
                light = lerp(light, .64h + .36h * saturate(dot(normal, key)), _Softness);
                half softFill = saturate(dot(normal, fill)) * 0.12h;
                half facing = saturate(dot(normal, view));
                half rim = 1.0h - facing;
                rim = rim * rim;
                // Elliptical studio softbox reflections give a wet glaze instead
                // of a tiny plastic hotspot. Directions are in world space, so
                // the reflections move over the surface when Pip is rotated.
                half3 reflection = reflect(-view, normal);
                half3 across = normalize(cross(key, half3(0, 1, 0)));
                half3 along = cross(across, key);
                half rx = dot(reflection, across);
                half ry = dot(reflection, along);
                half gate = smoothstep(0.45h, 0.85h, dot(reflection, key));
                half broad = exp2(-rx * rx * lerp(8.0h, 5.0h, _Softness) - ry * ry * lerp(7.0h, 4.0h, _Softness)) * gate;
                half glaze = exp2(-rx * rx * 22.0h - ry * ry * 18.0h) * gate;
                // The coral filling gathers in the lower third; shoulders and
                // crown keep the pale peach tint instead of a uniform orange.
                half gradient = smoothstep(.04h, .78h, input.height);
                half3 candy = lerp(_BottomColor.rgb, _TopColor.rgb, gradient);
                candy = lerp(candy, _Color.rgb, 0.10h);
                candy *= light;
                candy += _RimColor.rgb * softFill * 0.12h;
                // A bounded studio back-light approximation brightens thin edges.
                // The view-dependent thickness proxy needs no texture or extra pass.
                half backlight = saturate(dot(normal, fill)) * (.30h + .70h * rim);
                candy += _Color.rgb * backlight * _Transmission;
                half3 thinTint = lerp(_Color.rgb, _RimColor.rgb, .35h);
                candy = lerp(candy, thinTint, rim * _EdgeLight * .22h);
                half shine = (broad * 0.42h + glaze * 0.18h * (1.0h - _Softness)) * _Glossiness * _ReflectionStrength;
                candy = lerp(candy, half3(1.0h, 0.98h, 0.91h), saturate(shine));
                half3 studio = texCUBE(_StudioCube, reflection).rgb * _StudioStrength;
                half coat = _Glossiness * _ReflectionStrength;
                // Blend a bounded reflection instead of adding HDR white to an
                // already lit shell, which blew out the outline and gradient.
                half reflected = saturate(max(studio.r, max(studio.g, studio.b)) * coat);
                candy = lerp(candy, half3(1.0h, .98h, .94h), reflected * .72h);
                half pearl = _PearlSheen * (1.0h - facing) * (1.0h - facing);
                half3 pearlColor = lerp(half3(.64h, .86h, 1.0h), half3(.91h, .73h, 1.0h), saturate(normal.y * .5h + .5h));
                candy = lerp(candy, pearlColor, pearl * .38h);
                if (_GlitterStrength > .001h)
                {
                    // Rest UVs attach the microflakes to the deforming gel. A cheap
                    // cell hash replaces hundreds of meshes; view direction lights them.
                    float2 grid = input.uv * _GlitterDensity;
                    float2 cell = floor(grid);
                    float3 hash = frac(float3(cell.x, cell.y, cell.x) * .1031);
                    hash += dot(hash, hash.yzx + 33.33);
                    float seed = frac((hash.x + hash.y) * hash.z);
                    float seed2 = frac(seed * 13.71);
                    float2 spot = abs(frac(grid) - .5 - (float2(seed, seed2) - .5) * .35);
                    half flake = (1.0h - smoothstep(.035, .14, spot.x)) * (1.0h - smoothstep(.06, .23, spot.y));
                    flake *= step(.18, seed) * _GlitterStrength;
                    half3 facet = normalize(half3(seed - .5, .9, seed2 - .5));
                    half catchLight = pow(saturate(dot(reflection, facet)), 8.0h);
                    candy = lerp(candy, _GlitterColor.rgb * (.65h + catchLight * .65h), flake * .75h);
                    candy += half3(1.0h, .98h, .83h) * flake * catchLight * .65h;
                }
                // Keep the peach shell visible at grazing angles, with a soft
                // transmitted center. One pass; no screen grab or extra lights.
                // Reflections are opaque even where the peach gel is translucent.
                half alpha = saturate(_Color.a + rim * (1.0h - _Color.a) * .72h + reflected * 0.32h);
                return fixed4(candy, alpha);
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}
