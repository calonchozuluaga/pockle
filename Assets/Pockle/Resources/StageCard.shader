Shader "Pockle/Stage Card"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Cull Off ZWrite On
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct Input { float4 vertex:POSITION; fixed4 color:COLOR; };
            struct Output { float4 vertex:SV_POSITION; fixed4 color:COLOR; };
            Output vert(Input i) { Output o; o.vertex=UnityObjectToClipPos(i.vertex); o.color=i.color; return o; }
            fixed4 frag(Output i):SV_Target { return i.color; }
            ENDCG
        }
    }
}
