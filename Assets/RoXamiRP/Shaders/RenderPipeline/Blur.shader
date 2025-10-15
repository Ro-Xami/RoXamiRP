Shader "RoXami RP/Hidden/Blur"
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
			Name "Gaussian Blur"
			
			HLSLPROGRAM
			
			#pragma target 3.5
			#pragma vertex FullScreenTriangle
			#pragma fragment GaussianBlurPass
			#define _Post_BlurPass
			#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
			#include "Assets/RoXamiRP/Shaders/RenderPipeline/FullScreenTriangle.hlsl"
			#include "Assets/RoXamiRP/Shaders/RenderPipeline/BlurFragmentPass.hlsl"
			
			ENDHLSL
		}

		Pass
		{
			Name "Box Blur"
			
			HLSLPROGRAM
			
			#pragma target 3.5
			#pragma vertex FullScreenTriangle
			#pragma fragment BoxBlurPass
			#define _Post_BlurPass
			#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
			#include "Assets/RoXamiRP/Shaders/RenderPipeline/FullScreenTriangle.hlsl"
			#include "Assets/RoXamiRP/Shaders/RenderPipeline/BlurFragmentPass.hlsl"
			
			ENDHLSL
		}
	}
}