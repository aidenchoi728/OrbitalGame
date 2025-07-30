Shader "Custom/DoubleSidedTransparentURP"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,0.2)
        _Smoothness ("Smoothness", Range(0,1)) = 0.4
        _Metallic ("Metallic", Range(0,1)) = 0
    }
    SubShader
    {
        Tags {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
        }

        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off // <-- This makes it double-sided

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
            };

            float4 _BaseColor;
            float _Metallic;
            float _Smoothness;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                return _BaseColor;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}
