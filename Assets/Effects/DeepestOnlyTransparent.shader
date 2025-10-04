Shader "Custom/NearestOnlyLit"
{
    Properties
    {
        _BaseMap("Base Map", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1,1,1,0.5)
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }

        // PASS 0: write NEAREST depth
        Pass
        {
            Name "NearestOnlyDepth"
            Tags { "LightMode"="NearestOnlyDepth" }

            ZWrite Off
            ZTest Always
            Cull Back
            ColorMask R
            Blend One One
            BlendOp Min

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings   { float4 positionCS : SV_POSITION; };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                return o;
            }

            float frag (Varyings i) : SV_Target
            {
                float ndc = i.positionCS.z / i.positionCS.w;
                float lin = LinearEyeDepth(ndc, _ZBufferParams);
                return min(lin, 1e9);
            }
            ENDHLSL
        }

        // PASS 1: forward lit with nearest-only discard
        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode"="UniversalForward" }
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D_X(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D_X_FLOAT(_NearestOnlyTex); SAMPLER(sampler_NearestOnlyTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float4 screenPos  : TEXCOORD1;
            };

            Varyings vert(Attributes v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv         = v.uv;
                o.screenPos  = ComputeScreenPos(o.positionCS);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float ndc = i.positionCS.z / i.positionCS.w;
                float lin = LinearEyeDepth(ndc, _ZBufferParams);

                float2 uvScreen = i.screenPos.xy / i.screenPos.w;
                float stored = SAMPLE_TEXTURE2D_X(_NearestOnlyTex, sampler_NearestOnlyTex, uvScreen).r;

                if (lin > stored + 1e-3) discard; // discard if farther than nearest

                float4 baseTex = SAMPLE_TEXTURE2D_X(_BaseMap, sampler_BaseMap, i.uv);
                return baseTex * _BaseColor;
            }
            ENDHLSL
        }
    }
    FallBack Off
}
