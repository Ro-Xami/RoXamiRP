Shader "Hidden/RoXamiRP/Post"
{
	SubShader
	{
		Cull Off
		ZTest Always
		ZWrite Off
		
		HLSLINCLUDE
		#include "../ShaderLibrary/Common.hlsl"
		#include_with_pragmas "PostPass.hlsl"
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
			Name "Bloom Combine"
			
			HLSLPROGRAM
				#pragma target 3.5
				#pragma vertex FullScreenTriangle
				#pragma fragment BloomCombineFragment
			ENDHLSL
		}
	}
}