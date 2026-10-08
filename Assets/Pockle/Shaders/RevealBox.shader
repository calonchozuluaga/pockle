Shader "Pockle/Reveal Box"
{
    Properties { _Color ("Color", Color) = (0.8, 0.5, 0.5, 1) }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _Color;
            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; };
            struct v2f { float4 position : SV_POSITION; float3 normal : TEXCOORD0; };
            v2f vert(appdata v)
            {
                v2f o; o.position = UnityObjectToClipPos(v.vertex); o.normal = UnityObjectToWorldNormal(v.normal); return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                float light = 0.72 + 0.28 * saturate(dot(normalize(i.normal), normalize(float3(-0.5, 1.0, -0.4))));
                return fixed4(_Color.rgb * light, _Color.a);
            }
            ENDCG
        }
    }
}
