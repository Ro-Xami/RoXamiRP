Shader "RoxamiRP/Scene/ToonLit_Grass"
{
    Properties
    {
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1,1,1,1)

        [Space(10)]
        [Header(Normal Settings)]
        [Toggle(_NORMALMAP)] _BumpMapON ("Enable Normal Map", Float) = 0
        _BumpScale("Scale", Float) = 1.0
        _BumpMap("Normal Map", 2D) = "bump" {}
        
        [Space(10)]
        [Header(Metallic Smoothness Occlusion Settings)]
        [Toggle(_METALLICSPECGLOSSMAP)] _MetallicGlossMapON ("Enable MSA Map", Float) = 0
        _MetallicGlossMap("MSA Map", 2D) = "white" {}
        _Metallic("Metallic", Range(0.0, 1.0)) = 0.0
        _Smoothness("Smoothness", Range(0.0, 1.0)) = 0.0
        _OcclusionStrength("Occlusion Strength", Range(0.0, 1.0)) = 1.0

        [Space(10)]
        [Header(Emission Settings)]
        [Toggle(_EMISSION)] _EmissionMapON ("Enable Emission Map", Float) = 0
        _EmissionMap("Emission", 2D) = "white" {}
        [HDR] _EmissionColor("Color", Color) = (0,0,0)
        
        [Space(10)]
        [Header(Grass Settings)]
        [KeywordEnum(VColor, Mask, PositionOSY, UV0)] _WindWeightInput ("Wind Weight Input", Float) = 0
        _WindWeightMask ("Wind Weight Mask", 2D) = "black" {}
        _WindWeightFactor ("Wind Weight Factor", Range(0, 1)) = 1
        _GrassColor ("Grass Color", Color) = (1,1,1,1)
       
        [Space(10)]
        [Header(Rendering Settings)]
        [Toggle(_ALPHATEST_ON)] _AlphaClip("Alpha Clip", Float) = 1
        _Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

        [Enum(Off, 0, Front, 1, Back, 2)] _Cull("Cull Mode", Float) = 2.0
    }

    SubShader
    {
        // Universal Pipeline tag is required. If Universal render pipeline is not set in the graphics settings
        // this Subshader will fail. One can add a subshader below or fallback to Standard built-in to make this
        // material work with both Universal Render Pipeline and Builtin Unity Pipeline
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Lit"
            "IgnoreProjector" = "True"
        }
        LOD 300

        Pass
        {
            // Lightmode matches the ShaderPassName set in UniversalRenderPipeline.cs. SRPDefaultUnlit and passes with
            // no LightMode tag are also rendered by Universal Render Pipeline
            Name "GBuffer"
            Tags
            {
                "LightMode" = "UniversalGBuffer"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ZTest LEqual
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 4.5

            #define _APPLY_GLOBAL_WIND
            #pragma shader_feature_local _WINDWEIGHTINPUT_VCOLOR _WINDWEIGHTINPUT_MASK _WINDWEIGHTINPUT_POSITIONOS _WINDWEIGHTINPUT_UV

            // Deferred Rendering Path does not support the OpenGL-based graphics API:
            // Desktop OpenGL, OpenGL ES 3.0, WebGL 2.0.
            #pragma exclude_renderers gles3 glcore

            // -------------------------------------
            // Shader Stages
            #pragma vertex LitGBufferPassVertex
            #pragma fragment LitGBufferPassFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _NORMALMAP
            #pragma shader_feature_local_fragment _ALPHATEST_ON
            #pragma shader_feature_local_fragment _EMISSION
            #pragma shader_feature_local_fragment _METALLICSPECGLOSSMAP

            #define _SPECULARHIGHLIGHTS_OFF
            #define _ENVIRONMENTREFLECTIONS_OFF
            #pragma shader_feature_local _RECEIVE_SHADOWS_OFF

            // -------------------------------------
            // Universal Pipeline keywords
            // #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            // #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            #pragma multi_compile_fragment _ _RENDER_PASS_ENABLED

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE
            #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            //#pragma instancing_options renderinglayer
            //#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Toon/RoxamiLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Toon/RoxamiLitGBufferPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags
            {
                "LightMode" = "ShadowCaster"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            #define _APPLY_GLOBAL_WIND
            #pragma shader_feature_local _WINDWEIGHTINPUT_VCOLOR _WINDWEIGHTINPUT_MASK _WINDWEIGHTINPUT_POSITIONOS _WINDWEIGHTINPUT_UV

            // -------------------------------------
            // Shader Stages
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ALPHATEST_ON
            //#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            //#include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Universal Pipeline keywords

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            // This is used during shadow map generation to differentiate between directional and punctual light shadows, as they use different formulas to apply Normal Bias
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Toon/RoxamiLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Toon/RoxamiShadowCasterPass.hlsl"
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags
            {
                "LightMode" = "DepthOnly"
            }

            // -------------------------------------
            // Render State Commands
            ZWrite On
            ColorMask R
            Cull[_Cull]

            HLSLPROGRAM
            #pragma target 2.0

            #define _APPLY_GLOBAL_WIND
            #pragma shader_feature_local _WINDWEIGHTINPUT_VCOLOR _WINDWEIGHTINPUT_MASK _WINDWEIGHTINPUT_POSITIONOS _WINDWEIGHTINPUT_UV

            // -------------------------------------
            // Shader Stages
            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature_local _ALPHATEST_ON
            //#pragma shader_feature_local_fragment _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fragment _ LOD_FADE_CROSSFADE

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DOTS.hlsl"

            // -------------------------------------
            // Includes
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Toon/RoxamiLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/Toon/RoxamiDepthOnlyPass.hlsl"
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
    //CustomEditor "UnityEditor.Rendering.Universal.ShaderGUI.LitShader"
}
