// Jelly glass (UX-053): every rounded UI surface in one pass. GlassSurface supplies a single quad per surface with
// its size, corner radius and style, so the shape, light rim, gloss and soft shadow are drawn here at any resolution.
// Frosted surfaces show a blurred copy of the page painting (PockleGlass.SetBackdrop) at their place on screen.
Shader "Pockle/UI/Glass"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite (unused)", 2D) = "white" {}
        _Color("Tint", Color) = (1,1,1,1)
        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255
        _ColorMask("Color Mask", Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "CanUseSpriteAtlas"="True" }
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        ColorMask [_ColorMask]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct Input { float4 vertex:POSITION; fixed4 color:COLOR; float4 uv0:TEXCOORD0; float4 uv1:TEXCOORD1; float4 uv2:TEXCOORD2; };
            struct Output
            {
                float4 vertex:SV_POSITION; fixed4 color:COLOR; float2 local:TEXCOORD0; float4 shape:TEXCOORD1; float4 style:TEXCOORD2;
                float4 canvas:TEXCOORD3; float2 ground:TEXCOORD4; float4 mask:TEXCOORD5;
            };

            fixed4 _Color; float4 _ClipRect; float _UIMaskSoftnessX, _UIMaskSoftnessY;
            sampler2D _PockleBackdrop;      // blurred, toned page painting
            float4 _PockleGround;           // canvas-space rect of the page ground: min.xy, 1 / size.xy
            float4 _PockleGroundUV;         // the painting's cover crop: offset.xy, scale.xy
            float _PockleBackdropOn;        // 1 while the painting really is what lies behind the page

            Output vert(Input i)
            {
                Output o;
                o.canvas = i.vertex;
                o.vertex = UnityObjectToClipPos(i.vertex);
                o.color = i.color * _Color;
                o.local = i.uv0.xy; o.shape = i.uv1; o.style = i.uv2;
                float2 n = (i.vertex.xy - _PockleGround.xy) * _PockleGround.zw;
                o.ground = _PockleGroundUV.xy + n * _PockleGroundUV.zw;
                // RectMask2D clipping with soft edges, as in Unity's UI/Default.
                float2 pixel = o.vertex.w; pixel /= abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));
                float4 clip = clamp(_ClipRect, -2e10, 2e10);
                o.mask = float4(i.vertex.xy * 2 - clip.xy - clip.zw, .25 / (.25 * half2(_UIMaskSoftnessX, _UIMaskSoftnessY) + abs(pixel.xy)));
                return o;
            }

            float Box(float2 p, float2 halfSize, float r)
            {
                float2 q = abs(p) - halfSize + r;
                return length(max(q, 0)) + min(max(q.x, q.y), 0) - r;
            }

            fixed4 frag(Output i):SV_Target
            {
                float2 halfSize = i.shape.xy;
                float r = min(i.shape.z, min(halfSize.x, halfSize.y));
                float d = Box(i.local, halfSize, r);
                float aa = max(fwidth(d), .0001);
                float inside = saturate(.5 - d / aa);
                float rim = i.style.x, gloss = i.style.y, shadow = i.style.z, sheen = i.style.w;
                fixed4 tint = i.color;
                float t = saturate(i.local.y / max(halfSize.y, .001) * .5 + .5); // 0 at the bottom edge, 1 at the top

                // Body: frost shows the blurred painting through the tint; everything else is the tint itself.
                float frost = step(.5, i.shape.w);
                float over = frost * _PockleBackdropOn;
                float3 behind = tex2D(_PockleBackdrop, i.ground).rgb;
                float3 rgb = lerp(tint.rgb, lerp(behind, tint.rgb, tint.a), over);
                // Frost with nothing known behind it (the toy's stage, a dialog) turns nearly solid so words stay readable.
                float alpha = lerp(lerp(tint.a, 1 - (1 - tint.a) * .12, frost), max(tint.a, step(.004, tint.a)), over);
                float seen = step(.004, tint.a);

                // Light from above: a brighter top, a slightly deeper bottom.
                rgb *= 1 + gloss * (t - .55) * .22;
                // Jelly highlight: a soft band of light under the top edge.
                float band = smoothstep(.50, .98, t) * smoothstep(-1.5, -5.5, d);
                rgb = lerp(rgb, 1, band * gloss * .20);
                // A faint diagonal sheen on clear glass.
                float diagonal = i.local.x / max(halfSize.x, .001) * .35 + i.local.y / max(halfSize.y, .001);
                rgb = lerp(rgb, 1, exp(-pow((diagonal - .45) * 2.4, 2)) * sheen * .10);
                // Inner glow and the bright rim that catches light, strongest along the top.
                rgb = lerp(rgb, 1, exp(d / 5) * rim * .10);
                float edge = smoothstep(-1.7, -.9, d) * rim * lerp(.22, .85, t);
                rgb = lerp(rgb, 1, edge);
                alpha = max(alpha, edge * .9) * seen;

                // Soft shadow below, drawn in the margin GlassSurface adds around the surface.
                float below = Box(i.local + float2(0, 3), halfSize, r);
                float cast = shadow * .20 * pow(saturate(1 - max(below, 0) / 11), 2) * seen;
                fixed4 colour;
                colour.rgb = lerp(float3(.045, .016, .04), rgb, inside);
                colour.a = lerp(cast, alpha, inside);
                #ifdef UNITY_UI_CLIP_RECT
                half2 m = saturate((_ClipRect.zw - _ClipRect.xy - abs(i.mask.xy)) * i.mask.zw);
                colour.a *= m.x * m.y;
                #endif
                #ifdef UNITY_UI_ALPHACLIP
                clip(inside - .5);
                #endif
                return colour;
            }
            ENDCG
        }
    }
}
