Shader "RoXami RP/DeferredToonLit"
{
	Properties
	{
		
	}
	
	SubShader
	{
		HLSLINCLUDE
		
		ENDHLSL

		Pass
		{
//			Name "Deff"
//			Tags {"LightMode" = "ToonLit"}
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex FullScreenTriangle
			#pragma fragment ToonLitPassFragment

			#pragma multi_compile _instancing
			#pragma multi_compile _DIRECTIONAL_PCF3 _DIRECTIONAL_PCF5 _DIRECTIONAL_PCF7
			#pragma shader_feature_local _ALPHACLIP_ON

			#include "Assets/RoXamiRP/Shaders/FullScreenTriangle.hlsl"
			#include "../ShaderLibrary/Common.hlsl"
			#include "../ShaderLibrary/ToonLighting.hlsl"

			TEXTURE2D(Gbuffer0);
			SAMPLER(sampler_Gbuffer0);
			TEXTURE2D(Gbuffer1);
			SAMPLER(sampler_Gbuffer1);
			TEXTURE2D(Gbuffer2);
			SAMPLER(sampler_Gbuffer2);
			TEXTURE2D(Gbuffer3);
			SAMPLER(sampler_Gbuffer3);

			CBUFFER_START(UnityPerMaterial)

			CBUFFER_END

			Input GetInputData(Varyings IN)
			{
				Input OUT = (Input)0;
				OUT.positionWS = 0;
			    OUT.positionCS = IN.positionCS;
			    OUT.normalWS = 0;
			    OUT.viewWS = 0;
			    OUT.screenSpaceUV = 0;

				return OUT;
			}

			Surface GetSurfaceData(Varyings IN)
			{
				Surface OUT = (Surface)0;
				float3 base = SAMPLE_TEXTURE2D(Gbuffer0, sampler_Gbuffer0, IN.uv);
				float3 nomral = SAMPLE_TEXTURE2D(Gbuffer1, sampler_Gbuffer1, IN.uv);
				float3 MRA = SAMPLE_TEXTURE2D(Gbuffer2, sampler_Gbuffer2, IN.uv);
				float3 emission = SAMPLE_TEXTURE2D(Gbuffer3, sampler_Gbuffer3, IN.uv);
				OUT.albedo = base.rgb;
				OUT.normal = nomral;
				OUT.roughness = MRA.g;
				OUT.metallic = MRA.r;
				OUT.ao = MRA.b;
				OUT.emissive = emission;
				OUT.alpha = 1;

				return OUT;
			}

			float4 ToonLitPassFragment (Varyings IN) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);

				#ifdef _ALPHACLIP_ON
				clip(albedo.a - _cutout);
				#endif

				Input inputData = GetInputData(IN);
				Surface surfaceData = GetSurfaceData(IN);

				float4 color = CalculateToonLighting(inputData , surfaceData);

			    return float4(surfaceData.normal , 1);
			}

			ENDHLSL
		}
	}
}