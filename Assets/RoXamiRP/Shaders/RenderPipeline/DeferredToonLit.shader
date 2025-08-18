Shader "RoXami RP/Hide/DeferredToonLit"
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
			#include "Assets/RoXamiRP/Shaders/RenderPipeline/FullScreenTriangle.hlsl"
			#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
			#include "Assets/RoXamiRP/ShaderLibrary/ToonLighting.hlsl"
			#include "Assets/RoXamiRP/ShaderLibrary/CameraAttachment.hlsl"
		ENDHLSL

		Pass
		{
			Name "DeferredTooLit"
			Tags {"LightMode" = "DeferredToonLit"}
			
			Stencil
			{
				Ref 100
				Comp Equal
			}
			
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex FullScreenTriangle
			#pragma fragment DeferredToonLitPassFragment
			#pragma multi_compile _ SCREENSPACE_SHADOWS
			#pragma multi_compile _DIRECTIONAL_PCF3 _DIRECTIONAL_PCF5 _DIRECTIONAL_PCF7

			TEXTURE2D(_GBuffer0);
			SAMPLER(sampler_GBuffer0);
			TEXTURE2D(_GBuffer1);
			SAMPLER(sampler_GBuffer1);
			TEXTURE2D(_GBuffer2);
			SAMPLER(sampler_GBuffer2);
			TEXTURE2D(_GBuffer3);
			SAMPLER(sampler_GBuffer3);
			
			
			Input GetInputData(Varyings IN)
			{
				Input OUT = (Input)0;
				float depth = SampleCameraDepth(IN.uv);
				depth = GetReverseDepth(depth);
				
				OUT.positionWS = CalculateDepthToPositionWS(depth, IN.uv);
			    OUT.normalWS = SAMPLE_TEXTURE2D(_GBuffer1, sampler_GBuffer1, IN.uv).xyz;
			    OUT.viewWS = GetViewDirWS(OUT.positionWS);
			    OUT.screenSpaceUV = IN.uv;

				return OUT;
			}

			Surface GetSurfaceData(Varyings IN)
			{
				Surface OUT = (Surface)0;
				float4 base = SAMPLE_TEXTURE2D(_GBuffer0, sampler_GBuffer0, IN.uv);
				float4 nomral = SAMPLE_TEXTURE2D(_GBuffer1, sampler_GBuffer1, IN.uv);
				float4 MRA = SAMPLE_TEXTURE2D(_GBuffer2, sampler_GBuffer2, IN.uv);
				float4 emission = SAMPLE_TEXTURE2D(_GBuffer3, sampler_GBuffer3, IN.uv);
				OUT.albedo = base.rgb;
				OUT.normal = nomral.xyz;
				OUT.roughness = MRA.g;
				OUT.metallic = MRA.r;
				OUT.ao = MRA.b;
				OUT.emissive = emission.rgb;
				OUT.alpha = base.a;

				return OUT;
			}

			float4 DeferredToonLitPassFragment (Varyings IN) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);

				#ifdef _ALPHACLIP_ON
				clip(albedo.a - _cutout);
				#endif

				Input inputData = GetInputData(IN);
				Surface surfaceData = GetSurfaceData(IN);

				float4 color = CalculateToonLighting(inputData , surfaceData);

			    return float4(color.rgb, 1);
			}

			ENDHLSL
		}
	}
}