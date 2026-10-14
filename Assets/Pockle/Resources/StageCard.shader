Shader "Pockle/Stage Card"
{
    Properties { _MainTex("Painted world",2D)="white" {} _UseTexture("Use image",Float)=0 _Crop("Portrait crop",Float)=1 _Pan("Panorama centre",Float)=.5 _Tint("Image light",Color)=(1,1,1,1) }
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
            sampler2D _MainTex; half _UseTexture,_Crop,_Pan; half4 _Tint;
            struct Input { float4 vertex:POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0; };
            struct Output { float4 vertex:SV_POSITION; fixed4 color:COLOR; float2 uv:TEXCOORD0; };
            Output vert(Input i) { Output o; o.vertex=UnityObjectToClipPos(i.vertex); o.color=i.color; o.uv=i.uv; return o; }
            fixed4 frag(Output i):SV_Target { return lerp(i.color,tex2D(_MainTex,float2((i.uv.x-.5)*_Crop+_Pan,i.uv.y))*_Tint,_UseTexture); }
            ENDCG
        }
    }
}
