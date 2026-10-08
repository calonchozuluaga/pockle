Shader "Pockle/Soft Blush"
{
    Properties { _Color ("Blush tint", Color) = (1, .3, .26, .62) }
    SubShader
    {
        Tags { "Queue"="Transparent+20" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #include "UnityCG.cginc"
            fixed4 _Color;
            struct appdata { float4 vertex : POSITION; };
            struct v2f { float4 position : SV_POSITION; half2 disk : TEXCOORD0; };
            v2f vert(appdata input)
            {
                v2f output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.disk = input.vertex.xy;
                return output;
            }
            fixed4 frag(v2f input) : SV_Target
            {
                half fade = saturate(1.0h - dot(input.disk, input.disk));
                return fixed4(_Color.rgb, _Color.a * fade * fade);
            }
            ENDCG
        }
    }
    Fallback "Unlit/Color"
}
