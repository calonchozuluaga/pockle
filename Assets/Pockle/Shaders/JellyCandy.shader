Shader "Pockle/Jelly Candy"
{
    Properties
    {
        _Color ("Coral tint / opacity", Color) = (1, 0.43, 0.34, 0.91)
        _TopColor ("Peach light", Color) = (1, 0.71, 0.49, 1)
        _BottomColor ("Warm base", Color) = (0.89, 0.20, 0.23, 1)
        _Glossiness ("Soft glaze", Range(0,1)) = 0.48
        _RimColor ("Candy edge", Color) = (1, 0.86, 0.67, 1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        LOD 150
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
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

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            struct v2f
            {
                float4 position : SV_POSITION;
                float3 worldPosition : TEXCOORD0;
                half3 normal : TEXCOORD1;
                half height : TEXCOORD2;
            };

            v2f vert(appdata input)
            {
                v2f output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.worldPosition = mul(unity_ObjectToWorld, input.vertex).xyz;
                output.normal = UnityObjectToWorldNormal(input.normal);
                output.height = saturate(input.vertex.y * 0.42h + 0.50h);
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                half3 normal = normalize(input.normal);
                half3 view = normalize(_WorldSpaceCameraPos.xyz - input.worldPosition);
                // A quiet studio key keeps the little toy welcoming under any scene light.
                // This cheap transmitted-light approximation uses no scene color grab,
                // cubemap, metallic reflection, or additional lighting passes.
                half3 key = normalize(half3(-0.48h, 0.80h, -0.58h));
                half3 fill = normalize(half3(0.60h, 0.18h, 0.70h));
                half light = saturate(dot(normal, key)) * 0.24h + 0.76h;
                half softFill = saturate(dot(normal, fill)) * 0.12h;
                half facing = saturate(dot(normal, view));
                half rim = 1.0h - facing;
                rim = rim * rim;
                half3 halfVector = normalize(key + view);
                half sheen = pow(saturate(dot(normal, halfVector)), 24.0h + _Glossiness * 40.0h);
                half3 candy = lerp(_BottomColor.rgb, _TopColor.rgb, input.height);
                candy = lerp(candy, _Color.rgb, 0.20h);
                candy *= light;
                candy += _RimColor.rgb * (rim * 0.20h + softFill * 0.22h);
                candy += half3(1.0h, 0.94h, 0.82h) * sheen * (0.21h + _Glossiness * 0.26h);
                half alpha = saturate(_Color.a + rim * (1.0h - _Color.a));
                return fixed4(candy, alpha);
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}
