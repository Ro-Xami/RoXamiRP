Shader "RoXamiRP/Scene/ToonLit"
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
		
		[Enum(Off, 0, Front, 1, Back, 2)] _cullMode ("Cull Mode", Int) = 2
	}
	
	SubShader
	{
		Pass
		{
			Name "ToonGBuffer"
			Tags{"LightMode" = "ToonGBuffer"}
			
			Stencil
			{
				Ref 100
				Pass Replace
			}
			
			ColorMask RGB
			
			Cull [_cullMode]
			HLSLPROGRAM
			#pragma vertex ToonGBufferPassVertex
			#pragma fragment ToonGBufferPassFragment
			#pragma multi_compile_instancing
			#pragma shader_feature_local _ALPHACLIP_ON
			#pragma shader_feature_local _MRA_MAP_ON
			#pragma shader_feature_local _NORMAL_MAP_ON
			#pragma shader_feature_local _EMISSIVE_MAP_ON
			#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
			#include "Assets/RoXamiRP/Shaders/ToonLit/ToonLitInput.hlsl"
			#include "Assets/RoXamiRP/Shaders/ToonLit/ToonLitPass.hlsl"
			
			ENDHLSL
		}

		Pass
		{
			Name "ShadowCaster"
			Tags {"LightMode" = "ShadowCaster"}
			
			ColorMask 0
			Cull [_cullMode]
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex ShadowCasterPassVertex
			#pragma fragment ShadowCasterPassFragment
			#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
			#include "Assets/RoXamiRP/Shaders/ToonLit/ToonLitInput.hlsl"
			#include_with_pragmas "Assets/RoXamiRP/Shaders/Common/ShadowCasterPass.hlsl"

			ENDHLSL
		}
	}
}
