Shader "RoXami RP/Hide/DepthToPositionWS"
{
	Properties
	{
		
	}
	
	SubShader
	{
		Cull Off
		ZWrite Off
		ZTest Always
		
		HLSLINCLUDE
			#include "Assets/RoXamiRP/Shaders/FullScreenTriangle.hlsl"
			#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
			#include "Assets/RoXamiRP/ShaderLibrary/CameraAttachment.hlsl"
		ENDHLSL

		Pass
		{
			Name "DepthToPositionWS"
			Tags {"LightMode" = "DepthToPositionWS"}
			
			HLSLPROGRAM
			#pragma vertex FullScreenTriangle
			#pragma fragment DepthToPositionWSFragment

			float4 DepthToPositionWSFragment (Varyings IN) : SV_TARGET
			{
				float2 screenSpaceUV = IN.uv;
				float depth = SampleCameraDepth(screenSpaceUV);
				depth = GetReverseDepth(depth);
				float3 positionWS = CalculateDepthToPositionWS(depth, screenSpaceUV);
				
			    return float4(positionWS, 1);
			}

			ENDHLSL
		}
	}
}