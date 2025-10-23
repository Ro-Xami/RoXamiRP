Shader "RoXamiRP/Actor/ActorToonFace"
{
	Properties
	{
		_BaseColor ("Base Color" , color) = (1,1,1,1)
		[NoScaleOffest] _BaseMap ("Base Map" , 2D) = "white" {}
		
		[Space(10)][Header(SDF)]
		[NoScaleOffest] _SdfFaceMap ("Sdf Face Map", 2D) = "white" {}
		
		[Space(10)][Header(Lut)]
		[NoScaleOffest] _LutMap ("Lut Map", 2D) = "white" {}
		
		[Space(10)][Header(Alpha Clip)]
		[Toggle(_ALPHACLIP_ON)] _alphaClip ("Alpha Clip" , float) = 0
		_cutout ("Cut Out" , Range(0 , 1)) = 0.5
		
		[Space(10)][Header(Render Settings)]
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 0
		[Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 1
		
		[HideInInspector] _faceFrontDir ("Face Front Dir", Vector) = (1,1,1,1)
		
		//[Space(10)][Header(Metallic Roughtness Ao)]
		[HideInInspector] [NoScaleOffest] _MraMap ("MRA Map", 2D) = "white" {}
		[HideInInspector] _roughness ("Roughness" , Range(0 , 1)) = 0.5
		[HideInInspector] _metallic ("Metallic" , Range(0 , 1)) = 0
		[HideInInspector] _ao ("AO" , Range(0 , 1)) = 1
		
		//[Space(10)][Header(Normal)]
		[HideInInspector] [NoScaleOffest] _NormalMap ("Normal Map", 2D) = "bump" {}
		[HideInInspector] _normalStrength ("Normal Strength", Float) = 0
		
		//[Space(10)][Header(Emission)]
		[HideInInspector] [HDR]_emissive ("Emissive" , Color) = (0,0,0,0)
		[HideInInspector] [NoScaleOffest] _EmissionMap ("Emission Map", 2D) = "white" {}
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
				Ref 0
				Pass Replace
			}
			
			HLSLPROGRAM
			#pragma vertex ActorLitGBufferPassVertex
			#pragma fragment ActorLitFaceGBufferPassFragment
			#pragma multi_compile_instancing
			#pragma shader_feature_local _ALPHACLIP_ON
			#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
			#include "Assets/RoXamiRP/Shaders/Actor/ActorLitInput.hlsl"
			#include "Assets/RoXamiRP/Shaders/Actor/ActorLitPass.hlsl"

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