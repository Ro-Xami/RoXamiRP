Shader "RoXami RP/ToonLit"
{
	Properties
	{
		_BaseColor ("Base Color" , color) = (1,1,1,1)
		_BaseMap ("Base Map" , 2D) = "white" {}
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 0
		[Enum(Off, 0, On, 1)] _ZWrite ("Z Write", Float) = 1

		[Toggle] _alphaClip ("Alpha Clip" , float) = 0
		_cutout ("Cut Out" , Range(0 , 1)) = 0.5
	}
	
	SubShader
	{
		HLSLINCLUDE
		#include "../ShaderLibrary/Common.hlsl"
		ENDHLSL

		Pass
		{
			Name "ToonLit"
			Tags {"LightMode" = "ToonLit"}
			ZWrite [_ZWrite]
			Blend [_SrcBlend] [_DstBlend]
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex ToonLitPassVertex
			#pragma fragment ToonLitPassFragment

			#include_with_pragmas "ToonLitPass.hlsl"

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

			#include_with_pragmas "ShadowCasterPass.hlsl"

			ENDHLSL
		}
	}
}