#ifndef ACTORTOON_FACE_FORWARDPASS_INCLUDE
#define ACTORTOON_FACE_FORWARDPASS_INCLUDE

#include "Assets/RoXamiRP/Shaders/Actor/ActorLighting.hlsl"

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
	float3 biTangentWS : TEXCOORD4;
	float3 viewWS : TEXCOORD5;
	float4 srcPos : TEXCOORD6;
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
	OUT.tangentWS = TransformObjectToWorldNormal(IN.tangentOS.xyz);
	OUT.biTangentWS = GetBiTangent(OUT.normalWS, OUT.tangentWS, IN.tangentOS.w);
	
	OUT.viewWS = GetViewDirWS(OUT.positionWS);
	OUT.color = IN.color * _BaseColor;
	OUT.uv = TRANSFORM_TEX(IN.uv , _BaseMap);
	OUT.srcPos = ComputeScreenPos(OUT.positionCS);

	return OUT;
}

Input GetInputData(Varyings IN)
{
	Input OUT = (Input)0;
	OUT.positionWS = IN.positionWS;
    OUT.positionCS = IN.positionCS;
    OUT.normalWS = IN.normalWS;
    OUT.viewWS = IN.viewWS;
    OUT.screenSpaceUV = IN.srcPos.xy / IN.srcPos.w;

	return OUT;
}

Surface GetSurfaceData(Varyings IN)
{
	Surface OUT = (Surface)0;
	float4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * IN.color;

	float3 albedo = baseMap.rgb;
	float alpha = baseMap.a;
	
	OUT.albedo = albedo;
	OUT.normal = IN.normalWS;
	OUT.metallic = 0;
	OUT.roughness = 1;
	OUT.ao = 1;
	OUT.emissive = 0;
	OUT.alpha = alpha;

	return OUT;
}

float4 ToonLitPassFragment (Varyings IN) : SV_TARGET
{
	UNITY_SETUP_INSTANCE_ID(IN);
	
	Input inputData = GetInputData(IN);
	Surface surfaceData = GetSurfaceData(IN);

	float4 color = CalculateActorFace(inputData , surfaceData, IN.uv);

	#ifdef _ALPHACLIP_ON
		clip(surfaceData.alpha - _cutout);
	#endif

    return color;
}

#endif