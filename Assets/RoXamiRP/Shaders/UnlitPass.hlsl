#ifndef ROXAMIRP_UNLITPASS_INCLUDE
#define ROXAMIRP_UNLITPASS_INCLUDE

#pragma multi_compile_instancing
#pragma shader_feature_local _ALPHACLIP_ON

TEXTURE2D(_TestMap);
SAMPLER(sampler_TestMap);
TEXTURE2D(_TestMap0);
SAMPLER(sampler_TestMap0);
TEXTURE2D(_TestMap1);
SAMPLER(sampler_TestMap1);

CBUFFER_START(UnityPerMaterial)
	float4 _BaseColor;
	float4 _TestMap_ST;
	float _cutout;
CBUFFER_END

#ifdef INSTANCING_ON
    UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
        UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
    UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

#define _BaseColor              UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor)
#endif

struct Attributes {
	float4 positionOS : POSITION;
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

Varyings UnlitPassVertex(Attributes IN)
{
	Varyings OUT = (Varyings)0;
	UNITY_SETUP_INSTANCE_ID(IN);
	UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

	half3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
	OUT.positionCS = TransformWorldToHClip(positionWS);
	OUT.color = IN.color * _BaseColor;
	OUT.uv = TRANSFORM_TEX(IN.uv , _TestMap);

	return OUT;
}

float4 UnlitPassFragment (Varyings IN) : SV_TARGET
{
	UNITY_SETUP_INSTANCE_ID(IN);

	half4 albedo = SAMPLE_TEXTURE2D(_TestMap, sampler_TestMap, IN.uv) * _BaseColor * IN.color;

	#ifdef _ALPHACLIP_ON
	clip(albedo.a - _cutout);
	#endif

	return albedo;
}

#endif