Shader "RoXamiRP/Scene/ToonLit_Decal"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        [NoScaleOffset] _BaseMap ("Base Map", 2D) = "white" {}
        
        [Space(10)] [Header(Decal Settings)]
        [Toggle(_DECAL_MASK_MAP_ON)] _enableDecalMask ("Enable Decal Mask", Float) = 0
        _DecalMaskMap ("Decal Mask Map", 2D) = "white" {}
        _decalFade ("Decal Fade", Range(0, 1)) = 0
        
        [Space(10)][Header(Metallic Roughtness Ao)]
        [Toggle(_MRA_MAP_ON)] _enableMraMap ("Enable MRA Map", Float) = 0
        [NoScaleOffset] _MraMap ("MRA Map", 2D) = "white" {}
        _roughness ("Roughness", Range(0, 1)) = 0.5
        _metallic ("Metallic", Range(0, 1)) = 0
        _ao ("AO", Range(0, 1)) = 1
        
        [Space(10)][Header(Normal)]
        [Toggle(_NORMAL_MAP_ON)] _enableNormalMap ("Enable Normal Map", Float) = 0
        [NoScaleOffset] _NormalMap ("Normal Map", 2D) = "bump" {}
        _normalStrength ("Normal Strength", Float) = 0
        
        [Space(10)][Header(Emission)]
        [Toggle(_EMISSIVE_MAP_ON)] _enableEmissionMap ("Enable Emission Map", Float) = 0
        [HDR]_emissive ("Emissive", Color) = (0,0,0,0)
        [NoScaleOffset] _EmissionMap ("Emission Map", 2D) = "black" {}
        
        [Space(10)][Header(Alpha Clip)]
        [Toggle(_ALPHACLIP_ON)] _alphaClip ("Alpha Clip", float) = 0
        _cutout ("Cut Out", Range(0, 1)) = 0.5
    }
    
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
        }
        Pass
        {
            Name "ToonGBuffer"
            Tags{"LightMode" = "ToonGBuffer"}
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ZTest LEqual
            Cull Off
            ColorMask RGB
            
            HLSLPROGRAM
            #define _DECAL_GBUFFER
            #pragma shader_feature_local _DECAL_MASK_MAP_ON

            #pragma shader_feature_local _ALPHACLIP_ON
            #pragma shader_feature_local _MRA_MAP_ON
            #pragma shader_feature_local _NORMAL_MAP_ON
            #pragma shader_feature_local _EMISSIVE_MAP_ON

            #pragma multi_compile_instancing
            #pragma vertex ToonGBufferPassVertex
            #pragma fragment ToonGBufferPassFragment
            #include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
            #include "Assets/RoXamiRP/Shaders/ToonLit/ToonLitInput.hlsl"
            #include "Assets/RoXamiRP/Shaders/ToonLit/ToonLitPass.hlsl"
            
            ENDHLSL
        }
    }
}
