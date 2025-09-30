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

		Pass
		{
			Name "FXAA_NVIDIA_LUMA"
			
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex FullScreenTriangle
			#pragma fragment FXAA_LUMA
			#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
			#include "Assets/RoXamiRP/Shaders/RenderPipeline/FullScreenTriangle.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			TEXTURE2D(_TempRtSource0);
			SAMPLER(sampler_TempRtSource0);

			float4 FXAA_LUMA(Varyings IN) : SV_Target
			{
				float4 color = SAMPLE_TEXTURE2D(_TempRtSource0, sampler_TempRtSource0, IN.uv);
				float luma = Luminance(color.rgb);
				return float4(color.rgb, luma);
			}
			ENDHLSL
		}

		Pass
		{
			Name "FXAA_NVIDIA_Quality"
			
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex FullScreenTriangle
			#pragma fragment FXAA_Quality_NVIDIA
			#define FXAA_PC 1
			#define FXAA_HLSL_3 1
			#define FXAA_GREEN_AS_LUMA 1
			#define FXAA_QUALITY__PRESET 39
			#pragma multi_compile _AA_HIGH _AA_MIDDLE _AA_LOW
			#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
			#include "Assets/RoXamiRP/Shaders/RenderPipeline/FullScreenTriangle.hlsl"
			#include "Assets/RoXamiRP/ShaderLibrary/NVIDA/FXAA3_11.hlsl"
			
			#if defined(_AA_HIGH)
				#define _contrastThreshold_Quality 0.0312
				#define _relativeThreshold_Quality 0.125
			#elif defined(_AA_MIDDLE)
				#define _contrastThreshold_Quality 0.0625
				#define _relativeThreshold_Quality 0.166
			#elif defined(_AA_LOW)
				#define _contrastThreshold_Quality 0.0833
				#define _relativeThreshold_Quality 0.250
			#endif
			
			sampler2D _TempRtSource0;
			float4 _TempRtSource0_TexelSize;
			
			float4 FXAA_Quality_NVIDIA(Varyings IN) : SV_Target
			{
				return FxaaPixelShader
				(
				    IN.uv,
				    0,
				    _TempRtSource0,
				    _TempRtSource0,
				    _TempRtSource0,
				    _TempRtSource0_TexelSize.xy,
				    0,
				    0,
				    0,
				    0.75,
				    _contrastThreshold_Quality,
				    _relativeThreshold_Quality,
				    0,
				    0,
				    0,
				    0
				);
			}
			ENDHLSL
		}

		Pass
		{
			Name "FXAA_NVIDIA_Console"
			
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex FullScreenTriangle
			#pragma fragment FXAA_Console_NVIDIA
			#define FXAA_PC_CONSOLE 1
			#define FXAA_HLSL_3 1
			#define FXAA_QUALITY__PRESET 20
			#pragma multi_compile _AA_HIGH _AA_MIDDLE _AA_LOW
			#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
			#include "Assets/RoXamiRP/Shaders/RenderPipeline/FullScreenTriangle.hlsl"
			#include "Assets/RoXamiRP/ShaderLibrary/NVIDA/FXAA3_11.hlsl"

			#define scale 0.5
			#define sharpness 8
			#if defined(_AA_HIGH)
				#define _contrastThreshold_Console 0.04
				#define _relativeThreshold_Console 0.125
			#elif defined(_AA_MIDDLE)
				#define _contrastThreshold_Console 0.05
				#define _relativeThreshold_Console 0.25
			#elif defined(_AA_LOW)
				#define _contrastThreshold_Console 0.06
				#define _relativeThreshold_Console 0.25
			#endif

			sampler2D _TempRtSource0;
			float4 _TempRtSource0_TexelSize;

			float4 FXAA_Console_NVIDIA(Varyings IN) : SV_Target
			{
				float2 uv = IN.uv;
				float4 pos = float4(uv, uv) + float4(-_TempRtSource0_TexelSize.x, -_TempRtSource0_TexelSize.y, _TempRtSource0_TexelSize.x, _TempRtSource0_TexelSize.y) * 0.5f;
				float4 RcpFrame = float4(-_TempRtSource0_TexelSize.x, -_TempRtSource0_TexelSize.y, _TempRtSource0_TexelSize.x, _TempRtSource0_TexelSize.y);
				float4 RcpFrameOpt = RcpFrame * 0.5f;
				float4 RcpFrameOpt2 = RcpFrame * 2.0f;
				
				return FxaaPixelShader
				(
					uv,
				    pos,
				    _TempRtSource0,
				    _TempRtSource0,
				    _TempRtSource0,
				    _TempRtSource0_TexelSize.xy,
				    RcpFrameOpt,
				    RcpFrameOpt2,
				    0,
				    0,
				    0,
				    0,
				    sharpness,
				    _contrastThreshold_Console,
				    _relativeThreshold_Console,
				    0
				);
			}
			ENDHLSL
		}
	}
}