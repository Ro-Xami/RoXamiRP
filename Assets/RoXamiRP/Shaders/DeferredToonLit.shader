Shader "RoXami RP/Hide/DeferredToonLit"
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
			#pragma vertex ToonLitPassVertex
			#pragma fragment ToonLitPassFragment

			#pragma multi_compile _instancing
			#pragma multi_compile _DIRECTIONAL_PCF3 _DIRECTIONAL_PCF5 _DIRECTIONAL_PCF7
			#pragma shader_feature_local _ALPHACLIP_ON

			//#include "Assets/RoXamiRP/Shaders/FullScreenTriangle.hlsl"
			#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
			#include "Assets/RoXamiRP/ShaderLibrary/ToonLighting.hlsl"

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

			struct Attributes {
	float4 positionOS : POSITION;
	float3 normalOS : NORMAL;
	float4 tangentOS : TANGENT;
	float2 uv : TEXCOORD0;
	float4 color : COLOR;

	UNITY_VERTEX_INPUT_INSTANCE_ID
};
 
struct Varyings {
	float4 positionCS : SV_POSITION;
	float2 uv : TEXCOORD0;
	float3 positionWS : TEXCOORD1;
	float3 normalWS : TEXCOORD2;
	float3 tangentWS : TEXCOORD3;
	float3 bitangentWS : TEXCOORD4;
	float3 viewWS : TEXCOORD5;
	float2 screenSpaceUV : TEXCOORD6;
	float4 color : COLOR;

	UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings ToonLitPassVertex(Attributes IN)
{
	Varyings OUT = (Varyings)0;
	UNITY_SETUP_INSTANCE_ID(IN);
	UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

	OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
	OUT.positionCS = TransformWorldToHClip(OUT.positionWS);
	OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
	OUT.viewWS = GetViewDirWS(OUT.positionWS);
	OUT.color = IN.color;
	OUT.uv = IN.uv;

	return OUT;
}

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
				float4 base = SAMPLE_TEXTURE2D(Gbuffer0, sampler_Gbuffer0, IN.uv);
				float4 nomral = SAMPLE_TEXTURE2D(Gbuffer1, sampler_Gbuffer1, IN.uv);
				float4 MRA = SAMPLE_TEXTURE2D(Gbuffer2, sampler_Gbuffer2, IN.uv);
				float4 emission = SAMPLE_TEXTURE2D(Gbuffer3, sampler_Gbuffer3, IN.uv);
				OUT.albedo = base.rgb;
				OUT.normal = nomral.xyz;
				OUT.roughness = MRA.g;
				OUT.metallic = MRA.r;
				OUT.ao = MRA.b;
				OUT.emissive = emission.rgb;
				OUT.alpha = base.a;

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

			    return float4(surfaceData.normal , surfaceData.alpha);
			}

			ENDHLSL
		}
	}
}