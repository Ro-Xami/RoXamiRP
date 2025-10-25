Shader "RoXamiRP/Scene/ToonLit_Transparent"
{
	Properties
	{
		_BaseColor ("Base Color" , color) = (1,1,1,1)
		[NoScaleOffset] _BaseMap ("Base Map" , 2D) = "white" {}
		
		[Space(10)][Header(Metallic Roughtness Ao)]
		[Toggle(_MRA_MAP_ON)] _enableMraMap ("Enable MRA Map", Float) = 0
		[NoScaleOffset] _MraMap ("MRA Map", 2D) = "white" {}
		_roughness ("Roughness" , Range(0 , 1)) = 0.5
		_metallic ("Metallic" , Range(0 , 1)) = 0
		_ao ("AO" , Range(0 , 1)) = 1
		
		[Space(10)][Header(Normal)]
		[Toggle(_NORMAL_MAP_ON)] _enableNormalMap ("Enable Normal Map", Float) = 0
		[NoScaleOffset] _NormalMap ("Normal Map", 2D) = "bump" {}
		_normalStrength ("Normal Strength", Float) = 0
		
		[Space(10)][Header(Emission)]
		[Toggle(_EMISSIVE_MAP_ON)] _enableEmissionMap ("Enable Emission Map", Float) = 0
		[HDR]_emissive ("Emissive" , Color) = (0,0,0,0)
		[NoScaleOffset] _EmissionMap ("Emission Map", 2D) = "black" {}
		
		[Space(10)][Header(Alpha Clip)]
		[Toggle(_ALPHACLIP_ON)] _alphaClip ("Alpha Clip" , float) = 0
		_cutout ("Cut Out" , Range(0 , 1)) = 0.5
		
		[Space(10)][Header(Render Settings)]
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 0
		[Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 1
		[Enum(Back, 0, Front, 1, Off, 2)] _cullMode ("Cull Mode", Int) = 0
	}
	
	SubShader
	{
		HLSLINCLUDE
		
		ENDHLSL

		Pass
		{
			Name "ToonLit"
			Tags {"LightMode" = "ToonLit"}
			
			ZWrite [_ZWrite]
			Blend [_SrcBlend] [_DstBlend]
			Cull [_cullMode]
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex ToonLitPassVertex
			#pragma fragment ToonLitPassFragment
			#pragma multi_compile _ SCREENSPACE_SHADOWS
			#pragma multi_compile _instancing
			#pragma shader_feature_local _ALPHACLIP_ON
			#pragma shader_feature_local _MRA_MAP_ON
			#pragma shader_feature_local _NORMAL_MAP_ON
			#pragma shader_feature_local _MRAMAP_ON
			#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
			#include "Assets/RoXamiRP/Shaders/ToonLit/ToonLitInput.hlsl"
			#include "Assets/RoXamiRP/Shaders/ToonLit/ToonLitTransparentPass.hlsl"

			ENDHLSL
		}
	}
}