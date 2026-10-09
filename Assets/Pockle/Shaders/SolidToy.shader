Shader "Pockle/Solid Toy"
{
    Properties
    {
        _Color ("Body", Color) = (.48, .65, .36, 1)
        _TopColor ("Top", Color) = (.72, .82, .50, 1)
        _BottomColor ("Base", Color) = (.28, .45, .22, 1)
        _Flock ("Short flock", Range(0,1)) = 1
        _Glossiness ("Vinyl glaze", Range(0,1)) = .8
        _StudioCube ("Studio reflection", Cube) = "" {}
    }
    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        LOD 150
        ZWrite On
        Cull Back
        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            fixed4 _Color, _TopColor, _BottomColor;
            half _Flock, _Glossiness;
            samplerCUBE _StudioCube;
            struct appdata { float4 vertex : POSITION; float3 normal : NORMAL; float2 uv : TEXCOORD0; };
            struct v2f
            {
                float4 position : SV_POSITION;
                float3 world : TEXCOORD0;
                half3 normal : TEXCOORD1;
                float2 uv : TEXCOORD2;
                half height : TEXCOORD3;
            };
            v2f vert(appdata input)
            {
                v2f output;
                output.position = UnityObjectToClipPos(input.vertex);
                output.world = mul(unity_ObjectToWorld, input.vertex).xyz;
                output.normal = UnityObjectToWorldNormal(input.normal);
                output.uv = input.uv;
                output.height = saturate(input.vertex.y * .5h + .5h);
                return output;
            }
            fixed4 frag(v2f input) : SV_Target
            {
                half3 n = normalize(input.normal);
                half3 view = normalize(_WorldSpaceCameraPos.xyz - input.world);
                half3 key = normalize(half3(-.5h, .8h, -.65h));
                half facing = saturate(dot(n, view));
                half diffuse = .62h + .38h * saturate(dot(n, key));
                half3 tint = lerp(_BottomColor.rgb, _TopColor.rgb, input.height);
                tint = lerp(tint, _Color.rgb, .35h);
                float2 grid = input.uv * 180;
                float2 cell = floor(grid);
                float3 hash = frac(float3(cell.x, cell.y, cell.x) * .1031);
                hash += dot(hash, hash.yzx + 33.33);
                half noise = frac((hash.x + hash.y) * hash.z);
                // Fade subpixel fibers; no fur shells, alpha sorting, or strand geometry.
                float footprint = max(fwidth(grid.x), fwidth(grid.y));
                half resolved = 1 - smoothstep(.5, 2, footprint);
                half fiber = (noise - .5h) * resolved * .13h * _Flock;
                half clothRim = pow(1 - facing, 3) * .20h * _Flock;
                half3 color = tint * (diffuse + fiber) + half3(.85h, .93h, .72h) * clothRim;
                half3 reflection = reflect(-view, n);
                half3 studio = texCUBE(_StudioCube, reflection).rgb;
                half glaze = _Glossiness * (1 - _Flock);
                color += studio * glaze * .7h;
                color += pow(saturate(dot(n, normalize(key + view))), 64) * glaze * .2h;
                return fixed4(color, 1);
            }
            ENDCG
        }
    }
    Fallback "Diffuse"
}
