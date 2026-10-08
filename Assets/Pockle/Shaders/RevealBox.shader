Shader "Pockle/Reveal Box"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Printed packaging", 2D) = "white" {}
        [Toggle] _ZWrite ("Solid carton depth", Float) = 1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite [_ZWrite]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"
            fixed4 _Color;
            sampler2D _MainTex;
            float4 _MainTex_ST;
            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; float2 uv : TEXCOORD0; };
            struct v2f { float4 position : SV_POSITION; float3 normal : TEXCOORD0; float2 uv : TEXCOORD1; };
            v2f vert(appdata v)
            {
                v2f o; o.position = UnityObjectToClipPos(v.vertex); o.normal = UnityObjectToWorldNormal(v.normal);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex); return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                float light = 0.72 + 0.28 * saturate(dot(normalize(i.normal), normalize(float3(-0.5, 1.0, -0.4))));
                fixed4 art = tex2D(_MainTex, i.uv) * _Color;
                return fixed4(art.rgb * light, art.a);
            }
            ENDCG
        }
    }
}
