Shader "RoXamiRP/Actor/ActorToonSkin"
{
	Properties
	{
		[HDR] _BaseColor ("Base Color" , color) = (1,1,1,1)
		[NoScaleOffset] _BaseMap ("Base Map" , 2D) = "white" {}
		
		[Space(10)][Header(Face)]
		[Toggle(_ACTOR_FACE_ON)] _actorFaceON ("Is Face", Float) = 0
		[NoScaleOffset] _SdfFaceMap ("Sdf Face Map", 2D) = "white" {}
		
		[Space(10)][Header(Alpha Clip)]
		[Toggle(_ALPHACLIP_ON)] _alphaClip ("Alpha Clip" , float) = 0
		_cutout ("Cut Out" , Range(0 , 1)) = 0.5
		
		
		
		[Space(10)]
		[Header(Metallic Roughtness Ao)]
		_roughness ("Roughness" , Range(0 , 1)) = 0.5
		_metallic ("Metallic" , Range(0 , 1)) = 0
		_ao ("AO" , Range(0 , 1)) = 1
		
		[HideInInspector] [NoScaleOffset] _MraMap ("MRA Map", 2D) = "white" {}
		[HideInInspector] _faceFrontRightDir ("Face Front Dir", Vector) = (1,1,1,1)
		
		//[Space(10)][Header(Normal)]
		[HideInInspector] [NoScaleOffset] _NormalMap ("Normal Map", 2D) = "bump" {}
		[HideInInspector] _normalStrength ("Normal Strength", Float) = 0
		
		//[Space(10)][Header(Emission)]
		[HideInInspector] [HDR]_emissive ("Emissive" , Color) = (0,0,0,0)
		[HideInInspector] [NoScaleOffset] _EmissionMap ("Emission Map", 2D) = "white" {}
	}
	
	SubShader
	{
		HLSLINCLUDE
		
		ENDHLSL

		Pass
		{
			Name "ToonGBuffer"
			Tags{"LightMode" = "ToonGBuffer"}
			
			Stencil
			{
				Ref 102
				Pass Replace
			}
			
			HLSLPROGRAM
			#pragma vertex ActorLitGBufferPassVertex
			#pragma fragment ActorLitSkinGBufferPassFragment
			#pragma multi_compile _ _ACTOR_FACE_ON
			#pragma shader_feature_local _ALPHACLIP_ON
			#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
			#include "Assets/RoXamiRP/Shaders/Actor/ActorLitInput.hlsl"
			#include "Assets/RoXamiRP/Shaders/Actor/ActorLitPass.hlsl"

			ENDHLSL
		}

		Pass
		{
			Name "ActorOutline"
			Tags{"LightMode" = "ActorOutline"}
			
			Cull Front
			
			HLSLPROGRAM
			#pragma vertex ActorOutlinePassVertex
			#pragma fragment ActorOutlinePassFragment
			#pragma multi_compile_instancing
			#pragma shader_feature_local _ALPHACLIP_ON
			#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
			#include "Assets/RoXamiRP/Shaders/Actor/ActorLitInput.hlsl"
			#include "Assets/RoXamiRP/Shaders/Actor/ActorOutLinePass.hlsl"

			ENDHLSL
		}

		Pass
		{
			Name "ShadowCaster"
			Tags {"LightMode" = "ShadowCaster"}
			ColorMask 0
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex ShadowCasterPassVertex
			#pragma fragment ShadowCasterPassFragment
			#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
			#include "Assets/RoXamiRP/Shaders/Actor/ActorLitInput.hlsl"
			#include_with_pragmas "Assets/RoXamiRP/Shaders/Common/ShadowCasterPass.hlsl"

			ENDHLSL
		}
	}
}