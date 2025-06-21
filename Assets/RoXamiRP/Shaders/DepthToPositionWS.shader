Shader "RoXami RP/Hide/DepthToPositionWS"
{
    Properties
    {
    }
    
    SubShader
    {
        Pass
        {
            HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex DepthToPositionWSPassVertex
			#pragma fragment DepthToPositionWSPassFragment
			#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
			#include "Assets/RoXamiRP/ShaderLibrary/CameraAttachment.hlsl"
			
			struct Attributes
			{
				float4 positionOS : POSITION;
			};
	 
			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float4 srcPos : TEXCOORD6;
			};

			Varyings DepthToPositionWSPassVertex(Attributes IN)
			{
				Varyings OUT = (Varyings)0;
				
				OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
				OUT.srcPos = ComputeScreenPos(OUT.positionCS);

				return OUT;
			}

			float4 DepthToPositionWSPassFragment (Varyings IN) : SV_TARGET
			{
				float2 screenSpaceUV = IN.srcPos.xy / IN.srcPos.w;
				float depth = SampleCameraOpaqueDepth(screenSpaceUV);
				depth = GetReverseDepth(depth);
				
				float4 ndc = float4(screenSpaceUV * 2 - 1, depth, 1);
				#if UNITY_UV_STARTS_AT_TOP
				ndc.y = 1 - ndc.y;
				#endif

				float4 positionWS = mul(MATRIX_I_VP, ndc);
				positionWS.xyz = positionWS.xyz / positionWS.w;
				
			    return float4(positionWS.xyz, 1);
			}
	        ENDHLSL
        }
    }
}