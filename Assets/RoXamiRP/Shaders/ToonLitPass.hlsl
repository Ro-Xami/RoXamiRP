#ifndef ROXAMIRP_TOONLITPASS_INCLUDE
#define ROXAMIRP_TOONLITPASS_INCLUDE

#pragma multi_compile _instancing
#pragma multi_compile _DIRECTIONAL_PCF3 _DIRECTIONAL_PCF5 _DIRECTIONAL_PCF7
#pragma shader_feature_local _ALPHACLIP_ON

#include "../ShaderLibrary/Common.hlsl"
#include "../ShaderLibrary/ToonLighting.hlsl"

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

TEXTURECUBE(_TestCube);
SAMPLER(sampler_TestCube);

CBUFFER_START(UnityPerMaterial)
	float4 _BaseMap_ST;
	float4 _BaseColor;
	float _cutout;
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
	OUT.color = IN.color * _BaseColor;
	OUT.uv = TRANSFORM_TEX(IN.uv , _BaseMap);

	return OUT;
}

Input GetInputData(Varyings IN)
{
	Input OUT = (Input)0;
	OUT.positionWS = IN.positionWS;
    OUT.positionCS = IN.positionCS;
    OUT.normalWS = IN.normalWS;
    OUT.viewWS = IN.viewWS;
    OUT.screenSpaceUV = IN.screenSpaceUV;

	return OUT;
}

Surface GetSurfaceData(Varyings IN)
{
	Surface OUT = (Surface)0;
	float4 base = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * IN.color;
	OUT.albedo = base.rgb;
	OUT.normal = IN.normalWS;

	return OUT;
}

float4 ToonLitPassFragment (Varyings IN) : SV_TARGET
{
	UNITY_SETUP_INSTANCE_ID(IN);

	//#ifdef _ALPHACLIP_ON
	//clip(albedo.a - _cutout);
	//#endif

	Input inputData = GetInputData(IN);
	Surface surfaceData = GetSurfaceData(IN);

	float4 color = CalculateToonLighting(inputData , surfaceData);

    return color;
}

#endif