Shader "Pockle/Soft Accent"
{
    Properties { _Color ("Accent color", Color) = (1,1,1,1) }
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        LOD 100
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"
            fixed4 _Color;
            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; };
            struct v2f { float4 position : SV_POSITION; half3 normal : TEXCOORD0; };
            v2f vert(appdata input)
            {
                v2f output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.normal = UnityObjectToWorldNormal(input.normal);
                return output;
            }
            fixed4 frag(v2f input) : SV_Target
            {
                half softness = saturate(dot(normalize(input.normal), normalize(half3(-0.3h, 0.8h, -0.7h))));
                return fixed4(_Color.rgb * (0.88h + softness * 0.12h), _Color.a);
            }
            ENDCG
        }
    }
    Fallback "Unlit/Color"
}
