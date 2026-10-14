Shader "Pockle/Axolotl Plush" {
 Properties { _MainTex ("Uploaded plush paint", 2D) = "white" {} }
 SubShader { Tags { "RenderType"="Opaque" }
 Pass { Cull Back ZWrite On
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma target 2.0
 #include "UnityCG.cginc"
 sampler2D _MainTex;
 struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; float2 uv:TEXCOORD0; };
 struct v2f { float4 vertex:SV_POSITION; half3 normal:TEXCOORD0; float2 uv:TEXCOORD1; float3 world:TEXCOORD2; };
 v2f vert(appdata v) { v2f o; o.vertex=UnityObjectToClipPos(v.vertex); o.normal=UnityObjectToWorldNormal(v.normal); o.uv=v.uv; o.world=mul(unity_ObjectToWorld,v.vertex).xyz; return o; }
 fixed4 frag(v2f i):SV_Target {
  half3 paint=tex2D(_MainTex,i.uv).rgb; half3 n=normalize(i.normal); half3 view=normalize(_WorldSpaceCameraPos.xyz-i.world);
  half light=saturate((dot(n,normalize(half3(.6,.7,-.65)))+.5)/1.5);
  half rim=1-saturate(dot(n,view));
  half mask=smoothstep(.10,.35,max(paint.r,max(paint.g,paint.b)));
  half3 color=paint*(.78+.22*light)+mask*pow(rim,3)*half3(.12,.07,.09);
  return fixed4(saturate(color),1);
 }
 ENDCG
 } } Fallback "Unlit/Texture"
}
