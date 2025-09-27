#ifndef ACTORTOON_OUTLINE_PASS_INCLUDE
#define ACTORTOON_OUTLINE_PASS_INCLUDE

#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"

float _ActorOutlineWidth;

struct Attributes {
	float4 positionOS : POSITION;
	float3 normalOS : NORMAL;
	float2 uv : TEXCOORD0;
	float4 color : COLOR;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};
 
struct Varyings {
	float4 positionCS : SV_POSITION;
	float2 uv : TEXCOORD0;
	float4 color : COLOR;

	UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings ActorOutlinePassVertex(Attributes IN)
{
	Varyings OUT = (Varyings)0;
	UNITY_SETUP_INSTANCE_ID(IN);
	UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

	float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
	positionWS += TransformObjectToWorldNormal(IN.normalOS) * _ActorOutlineWidth * 0.01;
	OUT.positionCS = TransformWorldToHClip(positionWS);
	OUT.color = IN.color * _BaseColor;
	OUT.uv = TRANSFORM_TEX(IN.uv , _BaseMap);

	return OUT;
}

float4 ActorOutlinePassFragment (Varyings IN) : SV_TARGET
{
	UNITY_SETUP_INSTANCE_ID(IN);

	float4 color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * 0.1f;

	#ifdef _ALPHACLIP_ON
		clip(color - _cutout);
	#endif

    return color;
}

#endif