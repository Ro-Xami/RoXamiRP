Shader "RoXamiRP/Hide/Post"
{
	SubShader
	{
		Cull Off
		ZTest Always
		ZWrite Off
		
		HLSLINCLUDE
		#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
		#include "Assets/RoXamiRP/Shaders/RenderPass/PostPass.hlsl"
		ENDHLSL

		Pass
		{
			Name "Copy"
			
			HLSLPROGRAM
				#pragma target 3.5
				#pragma vertex FullScreenTriangle
				#pragma fragment CopyPassFragment
			ENDHLSL
		}

		Pass
		{
			Name "Bloom Filter"
			
			HLSLPROGRAM
				#pragma target 3.5
				#pragma vertex FullScreenTriangle
				#pragma fragment BloomPrefilterPassFragment
			ENDHLSL
		}

		Pass
		{
			Name "Bloom Horizontal"
			
			HLSLPROGRAM
				#pragma target 3.5
				#pragma vertex FullScreenTriangle
				#pragma fragment BloomHorizontalPassFragment
			ENDHLSL
		}

		Pass
		{
			Name "Bloom Vertical"
			
			HLSLPROGRAM
				#pragma target 3.5
				#pragma vertex FullScreenTriangle
				#pragma fragment BloomVerticalPassFragment
			ENDHLSL
		}

		Pass
		{
			Name "Bloom UpSample"
			
			HLSLPROGRAM
				#pragma target 3.5
				#pragma vertex FullScreenTriangle
				#pragma fragment BloomUpSamplePassFragment
			ENDHLSL
		}

		Pass
		{
			Name "Combine"
			
			HLSLPROGRAM
				#pragma multi_compile _ _Post_Bloom_ON
				#pragma multi_compile _ _Post_ColorAdjustments_ON
				#pragma multi_compile _ _Post_WhiteBalance_ON
				#pragma multi_compile _ _Post_AcesFilm_ON _Post_AcesSimple_ON _Post_GT_ON
				#pragma multi_compile _ _Post_DepthOfFeild_ON
				
				#pragma target 3.5
				#pragma vertex FullScreenTriangle
				#pragma fragment CombineFragment
			ENDHLSL
		}

		Pass
		{
			Name "FinalBlit"
			
			HLSLPROGRAM
				#pragma target 3.5
				#pragma vertex FullScreenTriangle
				#pragma fragment FinalBlitFragment
			ENDHLSL
		}
	}
}