Shader "RoXami RP/Hide/AntiAliasing"
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
			Name "FXAA_Quality"
			
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex FullScreenTriangle
			#pragma fragment FXAA_Quality
			#pragma multi_compile _AA_HIGH _AA_MIDDLE _AA_LOW
			#include "Assets/RoXamiRP/Shaders/RenderPipeline/AntiAliasingFragmentPass.hlsl"
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
			#include "Assets/RoXamiRP/Shaders/RenderPipeline/AntiAliasingFragmentPass.hlsl"
			ENDHLSL
		}
	}
}