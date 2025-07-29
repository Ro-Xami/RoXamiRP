#ifndef ROXAMIRP_TOONLITPASS_INCLUDE
#define ROXAMIRP_TOONLITPASS_INCLUDE
#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
#include "Assets/RoXamiRP/ShaderLibrary/ToonLighting.hlsl"

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

CBUFFER_START(UnityPerMaterial)
	float4 _BaseMap_ST;
	float4 _BaseColor;
	float _cutout;
	float _roughness;
	float _metallic;
	float _ao;
	float3 _emissive;
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
	float4 base = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * IN.color;
	OUT.albedo = base.rgb;
	OUT.normal = IN.normalWS;
	OUT.roughness = _roughness;
	OUT.metallic = _metallic;
	OUT.emissive = _emissive;
	OUT.ao = _ao;
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

    return color;
}

#endif