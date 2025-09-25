Shader "RoXami RP/ToonLit_Opaque"
{
	Properties
	{
		_BaseColor ("Base Color" , color) = (1,1,1,1)
		[NoScaleOffest] _BaseMap ("Base Map" , 2D) = "white" {}
		
		[Space(10)][Header(Metallic Roughtness Ao)]
		[NoScaleOffest] _MraMap ("MRA Map", 2D) = "white" {}
		_roughness ("Roughness" , Range(0 , 1)) = 0.5
		_metallic ("Metallic" , Range(0 , 1)) = 0
		_ao ("AO" , Range(0 , 1)) = 1
		
		[Space(10)][Header(Normal)]
		[NoScaleOffest] _NormalMap ("Normal Map", 2D) = "bump" {}
		_normalStrength ("Normal Strength", Float) = 0
		
		[Space(10)][Header(Emission)]
		[HDR]_emissive ("Emissive" , Color) = (0,0,0,0)
		[NoScaleOffest] _EmissionMap ("Emission Map", 2D) = "black" {}
		
		[Space(10)][Header(Alpha Clip)]
		[Toggle(_ALPHACLIP_ON)] _alphaClip ("Alpha Clip" , float) = 0
		_cutout ("Cut Out" , Range(0 , 1)) = 0.5
		
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
			
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex ToonLitPassVertex
			#pragma fragment ToonLitPassFragment
			#pragma multi_compile _ SCREENSPACE_SHADOWS
			#pragma multi_compile _instancing
			#pragma shader_feature_local _ALPHACLIP_ON
			#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
			#include "Assets/RoXamiRP/Shaders/ToonLitInput.hlsl"
			#include "Assets/RoXamiRP/Shaders/ToonLitPass.hlsl"

			ENDHLSL
		}

		Pass
		{
			Name "ToonGBuffer"
			Tags{"LightMode" = "ToonGBuffer"}
			
			Stencil
			{
				Ref 0
				Pass Replace
			}
			
			HLSLPROGRAM
			#pragma vertex ToonGBufferPassVertex
			#pragma fragment ToonGBufferPassFragment
			#pragma multi_compile_instancing
			#pragma shader_feature_local _ALPHACLIP_ON
			#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
			#include "Assets/RoXamiRP/Shaders/ToonLitInput.hlsl"
			#include "Assets/RoXamiRP/Shaders/ToonGBufferPass.hlsl"

			ENDHLSL
		}

		Pass
		{
			Name "ShadowCaster"
			Tags {"LightMode" = "ShadowCaster"}
			ColorMask 0
			HLSLPROGRAM
			#pragma target 3.5
			#pragma multi_compile_instancing
			#pragma shader_feature_local _ALPHACLIP_ON
			#pragma vertex ShadowCasterPassVertex
			#pragma fragment ShadowCasterPassFragment
			#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
			#include "Assets/RoXamiRP/Shaders/ToonLitInput.hlsl"
			#include "Assets/RoXamiRP/Shaders/ShadowCasterPass.hlsl"

			ENDHLSL
		}
	}
}