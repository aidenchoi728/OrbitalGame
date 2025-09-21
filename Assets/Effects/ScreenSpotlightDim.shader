// Assets/Effects/ScreenSpotlightDim.shader
Shader "Hidden/ScreenSpotlightDim"
{
    Properties
    {
        _DimAmount    ("Dim Amount", Range(0,1)) = 0.7
        _SpotRect     ("Spot Rect (cx,cy,hx,hy)", Vector) = (0.5,0.5,0.2,0.1)
        _SpotFeather  ("Feather (0-0.25)", Range(0,0.25)) = 0.01
        [Toggle]_FlipSampleY ("Flip Sample Y", Float) = 0
        [Toggle]_FlipMaskY   ("Flip Mask Y", Float) = 0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            Name "ScreenSpotlightDim"
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc" // _ScreenParams, sampler2D, tex2D

            struct VSOut { float4 pos : SV_Position; };

            // Fullscreen triangle (procedural)
            VSOut Vert(uint vid : SV_VertexID)
            {
                float2 p = float2((vid << 1) & 2, vid & 2); // (0,0),(2,0),(0,2)
                VSOut o;
                o.pos = float4(p * 2.0 - 1.0, 0.0, 1.0);
                return o;
            }

            // Bound by Full Screen Pass (camera color)
            sampler2D _BlitTexture;

            // Material params
            float  _DimAmount;                // 0..1
            float4 _SpotRect;                 // (cx, cy, halfW, halfH) in normalized screen UV
            float  _SpotFeather;              // normalized feather width
            float  _FlipSampleY;              // 0/1
            float  _FlipMaskY;                // 0/1

            float RectSDF(float2 uv, float2 c, float2 h)
            {
                float2 d = abs(uv - c) - h;
                return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
            }

            fixed4 Frag(VSOut i) : SV_Target
            {
                // 0..1 screen UV from SV_Position (robust to RTHandles/render-scale)
                float2 uvScreen = i.pos.xy / _ScreenParams.xy;

                // Sampling UV (what we use to read the camera color)
                float2 uvSample = uvScreen;
                if (_FlipSampleY > 0.5) uvSample.y = 1.0 - uvSample.y;

                float4 col = tex2D(_BlitTexture, uvSample);

                // Mask UV (where you define the spotlight rect)
                float2 uvMask = uvScreen;
                if (_FlipMaskY > 0.5) uvMask.y = 1.0 - uvMask.y;

                // Spotlight mask
                float  sd   = RectSDF(uvMask, _SpotRect.xy, _SpotRect.zw);
                float  mask = 1.0 - smoothstep(0.0, max(1e-5, _SpotFeather), sd); // 1 = inside rect

                // Dim outside
                float3 dimmed = col.rgb * (1.0 - saturate(_DimAmount));
                col.rgb = lerp(dimmed, col.rgb, mask);
                return col;
            }
            ENDCG
        }
    }
    FallBack Off
}
