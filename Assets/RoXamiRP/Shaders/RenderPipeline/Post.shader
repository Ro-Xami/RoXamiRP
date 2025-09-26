Shader "RoXami RP/Hide/Post"
{
	SubShader
	{
		Cull Off
		ZTest Always
		ZWrite Off
		
		HLSLINCLUDE
		#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
		#include "PostPass.hlsl"
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
				#pragma multi_compile _Bloom
				#pragma multi_compile _ _ACES_Film_ToneMapping _ACES_Simple_ToneMapping _GT_ToneMapping
				#pragma multi_compile _ColorAdjustments
				#pragma multi_compile _DepthOfFeild
				
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