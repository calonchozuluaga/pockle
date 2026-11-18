// Tones the page painting (calmer colour, a milk lift so plum words read on it) and blurs its small copy for frosted glass.
Shader "Pockle/UI/Ground"
{
    Properties
    {
        [PerRendererData] _MainTex("Painting", 2D) = "white" {}
        _Saturation("Saturation", Float) = .82
        _Lift("Milk lift", Float) = .12
        _Spread("Blur spread, texels", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Cull Off Lighting Off ZWrite Off ZTest Always Blend Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _MainTex; float4 _MainTex_TexelSize; float _Saturation, _Lift, _Spread;
            struct Input { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct Output { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; };
            Output vert(Input i) { Output o; o.vertex = UnityObjectToClipPos(i.vertex); o.uv = i.uv; return o; }
            fixed4 frag(Output i):SV_Target
            {
                float2 s = _MainTex_TexelSize.xy * _Spread;
                float3 c = tex2D(_MainTex, i.uv).rgb;
                if (_Spread > 0)
                {
                    c = c * .25
                        + (tex2D(_MainTex, i.uv + float2(s.x, 0)).rgb + tex2D(_MainTex, i.uv - float2(s.x, 0)).rgb
                        + tex2D(_MainTex, i.uv + float2(0, s.y)).rgb + tex2D(_MainTex, i.uv - float2(0, s.y)).rgb) * .125
                        + (tex2D(_MainTex, i.uv + s).rgb + tex2D(_MainTex, i.uv - s).rgb
                        + tex2D(_MainTex, i.uv + float2(s.x, -s.y)).rgb + tex2D(_MainTex, i.uv + float2(-s.x, s.y)).rgb) * .0625;
                }
                float grey = dot(c, float3(.2126, .7152, .0722));
                c = lerp(grey.xxx, c, _Saturation);
                c = lerp(c, float3(1, .955, .915), _Lift);
                return fixed4(c, 1);
            }
            ENDCG
        }
    }
}
