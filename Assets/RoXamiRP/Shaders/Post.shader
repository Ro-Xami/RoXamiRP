Shader "Hidden/RoXamiRP/Post"
{
	SubShader
	{
		Cull Off
		ZTest Always
		ZWrite Off
		
		HLSLINCLUDE
		#include "../ShaderLibrary/Common.hlsl"
		#include "FullScreenTriangle.hlsl"
		ENDHLSL

		Pass {
			Name "Copy"
			
			HLSLPROGRAM
				#pragma target 3.5
				#pragma vertex FullScreenTriangle
				#pragma fragment CopyPassFragment
			ENDHLSL
		}
	}
}