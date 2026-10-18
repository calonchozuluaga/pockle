Shader "Pockle/Toy Glow"
{
    Properties { _Color("Light tint",Color)=(.3,.8,1,.2) }
    SubShader
    {
        Tags { "Queue"="Transparent+10" "RenderType"="Transparent" }
        Blend SrcAlpha One ZWrite Off Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            fixed4 _Color;
            struct Input { float4 vertex:POSITION; float2 uv:TEXCOORD0; };
            struct Output { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; };
            Output vert(Input i){Output o;o.vertex=UnityObjectToClipPos(i.vertex);o.uv=i.uv*2-1;return o;}
            fixed4 frag(Output i):SV_Target {half fade=saturate(1-dot(i.uv,i.uv));return fixed4(_Color.rgb,_Color.a*fade*fade*fade);}
            ENDCG
        }
    }
}
