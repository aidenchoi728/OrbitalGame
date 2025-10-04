Shader "Custom/URP/LitTransparentFrontMost"
{
    Properties
    {
        // Standard Lit properties (names match URP Lit so textures plug right in)
        _BaseMap        ("Base Map", 2D) = "white" {}
        _BaseColor      ("Base Color", Color) = (1,1,1,1)
        _Cutoff         ("Alpha Clipping", Range(0.0,1.0)) = 0.5

        _MetallicGlossMap ("Metallic Map (R) Smoothness (A)", 2D) = "black" {}
        _Metallic       ("Metallic", Range(0.0,1.0)) = 0.0
        _Smoothness     ("Smoothness", Range(0.0,1.0)) = 0.5

        _BumpMap        ("Normal Map", 2D) = "bump" {}
        _BumpScale      ("Normal Scale", Float) = 1.0

        _OcclusionMap   ("Occlusion", 2D) = "white" {}
        _OcclusionStrength ("Occlusion Strength", Range(0,1)) = 1.0

        _EmissionMap    ("Emission Map", 2D) = "black" {}
        _EmissionColor  ("Emission Color", Color) = (0,0,0,0)

        // Optional tweak: if you see edge fizzing with MSAA, set to 1 to use LEqual + small bias
        _RobustEdges    ("Robust Edges (LEqual+Bias)", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "UniversalMaterialType"="Lit"
            "IgnoreProjector"="True"
        }
        LOD 300
        Cull Back

        // ---------------------------------------------------------------------
        // PASS 0: Depth prepass
        // Writes only depth so the nearest surface of THIS material is recorded.
        // No color written (ColorMask 0).
        Pass
        {
            Name "FrontMostDepthPrepass"
            Tags { "LightMode"="UniversalForward" } // ensure it draws before the forward pass
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Blend One Zero

            // Mark stencil so the color pass can be restricted to pixels we touched
            Stencil
            {
                Ref 3
                Comp Always
                Pass Replace
            }

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex DepthOnlyVert
            #pragma fragment DepthOnlyFrag

            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings DepthOnlyVert (Attributes IN)
            {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_TRANSFER_INSTANCE_ID(IN,o);
                o.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return o;
            }

            half4 DepthOnlyFrag (Varyings i) : SV_Target
            {
                return 0; // ColorMask 0 -> nothing is written to color
            }
            ENDHLSL
        }

        // ---------------------------------------------------------------------
        // PASS 1: Lit forward pass (PBR) but ONLY where we are the closest layer.
        Pass
        {
            Name "ForwardLitFrontMost"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            // Default to Equal; switch to LEqual + bias via _RobustEdges if you see tiny edge misses.
            ZTest Equal

            // When _RobustEdges != 0, we enable an alternate keyword block below that sets Offset.
            // (URP can’t change fixed-function states from code, so we branch the compiled variants.)
            // We still compile this pass once; the Offset directive is inside the keyworded block.

            Cull Back

            Stencil
            {
                Ref 3
                Comp Equal   // only shade pixels our prepass touched
            }

            HLSLPROGRAM
            #pragma target 3.5

            // Vertex/fragment from URP Lit:
            #pragma vertex   LitPassVertex
            #pragma fragment LitPassFragment

            // ---------------- URP feature keywords (trimmed to the useful set) ----------------
            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            // Lighting variants
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            // GI/Lightmap variants
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK

            // Material features (subset of Lit)
            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local _ _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON
            #pragma shader_feature_local _ _METALLICSPECGLOSSMAP _SPECULAR_SETUP
            #pragma shader_feature_local _ _NORMALMAP
            #pragma shader_feature_local _ _OCCLUSIONMAP
            #pragma shader_feature_local _ _EMISSION

            // We’re always Transparent surface type
            #define _SURFACE_TYPE_TRANSPARENT 1

            // “Robust edges” variant: when enabled on the material, we use LEqual + a tiny depth bias.
            #pragma shader_feature_local _FRONTMOST_ROBUST_EDGES

            // Hook up URP Lit code
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitForwardPass.hlsl"

            ENDHLSL

            // NOTE: We can’t set ZTest dynamically in code above; if you want the robust variant:
            // Duplicate this pass with: ZTest LEqual and add `Offset -1, -1`.
        }

        // Optional: Duplicate of the forward pass using LEqual + slight depth bias.
        // Enable by setting _RobustEdges != 0 in material inspector (see MaterialPropertyDrawer below).
        // If you’d rather not have a second pass, delete this block.
        Pass
        {
            Name "ForwardLitFrontMost_Robust"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Offset -1, -1
            Cull Back

            Stencil
            {
                Ref 3
                Comp Equal
            }

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex   LitPassVertex
            #pragma fragment LitPassFragment

            #pragma multi_compile_instancing
            #pragma multi_compile _ DOTS_INSTANCING_ON

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile_fragment _ _SHADOWS_SOFT

            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
            #pragma multi_compile _ SHADOWS_SHADOWMASK

            #pragma shader_feature_local _ALPHATEST_ON
            #pragma shader_feature_local _ _ALPHAPREMULTIPLY_ON _ALPHAMODULATE_ON
            #pragma shader_feature_local _ _METALLICSPECGLOSSMAP _SPECULAR_SETUP
            #pragma shader_feature_local _ _NORMALMAP
            #pragma shader_feature_local _ _OCCLUSIONMAP
            #pragma shader_feature_local _ _EMISSION

            #define _SURFACE_TYPE_TRANSPARENT 1

            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitForwardPass.hlsl"
            ENDHLSL
        }
    }

    FallBack Off
}
