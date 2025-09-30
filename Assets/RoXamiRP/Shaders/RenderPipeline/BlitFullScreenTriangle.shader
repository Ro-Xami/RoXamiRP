Shader "RoXami RP/Hide/BlitFullScreenTriangle"
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
			Name "BlitFullScreenTriangle"
			
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex FullScreenTriangle
			#pragma fragment BlitFullScreenTriangleFragment

			#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
			#include "Assets/RoXamiRP/Shaders/RenderPipeline/FullScreenTriangle.hlsl"

			TEXTURE2D(_TempRtSource0);
			SAMPLER(sampler_linear_clamp);

			float4 BlitFullScreenTriangleFragment(Varyings IN) : SV_Target
			{
				return float4(SAMPLE_TEXTURE2D(_TempRtSource0, sampler_linear_clamp, IN.uv).rgb, 1);
			}
			ENDHLSL
		}
	}
}