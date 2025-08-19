Shader "RoXami RP/Hide/FXAA"
{
	SubShader
	{
		Cull Off
		ZTest Always
		ZWrite Off
		
		HLSLINCLUDE
		
		ENDHLSL

		Pass
		{
			Name "FXAA_Quality"
			
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex FullScreenTriangle
			#pragma fragment FXAA_Quality
			#pragma multi_compile _AA_HIGH _AA_MIDDLE _AA_LOW

			#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
			#include "Assets/RoXamiRP/Shaders/RenderPipeline/FullScreenTriangle.hlsl"
			#include "Assets/RoXamiRP/Shaders/RenderPipeline/FXAAFragmentPass.hlsl"
			ENDHLSL
		}

		Pass
		{
			Name "FXAA_Console"
			
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex FullScreenTriangle
			#pragma fragment FXAA_Console
			#pragma multi_compile _AA_HIGH _AA_MIDDLE _AA_LOW

			#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
			#include "Assets/RoXamiRP/Shaders/RenderPipeline/FullScreenTriangle.hlsl"
			#include "Assets/RoXamiRP/Shaders/RenderPipeline/FXAAFragmentPass.hlsl"
			ENDHLSL
		}
	}
}