Shader "RoXamiRP/Hide/SMAA"
{
	SubShader
	{
		Cull Off
		ZTest Always
		ZWrite Off
		
		HLSLINCLUDE
		#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
		#include "Assets/RoXamiRP/Shaders/RenderPipeline/FullScreenTriangle.hlsl"
		ENDHLSL

		Pass
		{
			Name "SMAA_Edge"
			
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex FullScreenTriangle
			#pragma fragment SMAA_Edge
			#pragma multi_compile _AA_HIGH _AA_MIDDLE _AA_LOW
			#include "Assets/RoXamiRP/Shaders/RenderPipeline/SMAAFragmentPass.hlsl"
			ENDHLSL
		}

		Pass
		{
			Name "SMAA_Factor"
			
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex FullScreenTriangle
			#pragma fragment SMAA_Factor
			#pragma multi_compile _AA_HIGH _AA_MIDDLE _AA_LOW
			#include "Assets/RoXamiRP/Shaders/RenderPipeline/SMAAFragmentPass.hlsl"
			ENDHLSL
		}

		Pass
		{
			Name "SMAA_Blend"
			
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex FullScreenTriangle
			#pragma fragment SMAA_Blend
			#pragma multi_compile _AA_HIGH _AA_MIDDLE _AA_LOW
			#include "Assets/RoXamiRP/Shaders/RenderPipeline/SMAAFragmentPass.hlsl"
			ENDHLSL
		}
	}
}