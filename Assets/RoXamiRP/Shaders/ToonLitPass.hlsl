#ifndef ROXAMIRP_TOONLITPASS_INCLUDE
#define ROXAMIRP_TOONLITPASS_INCLUDE

#include "../ShaderLibrary/Light.hlsl"
#include "../ShaderLibrary/ToonLitSurface.hlsl"
#include "../ShaderLibrary/ToonLighting.hlsl"

#pragma multi_compile_instancing
#pragma multi_compile _ _DIRECTIONAL_PCF3 _DIRECTIONAL_PCF5 _DIRECTIONAL_PCF7
#pragma shader_feature_local _ALPHACLIP_ON

CBUFFER_START(UnityPerMaterial)
	float4 _BaseColor;
	float4 _BaseMap_ST;
	float _cutout;
CBUFFER_END

#ifdef INSTANCING_ON
    UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
        UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
    UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

#define _BaseColor              UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor)
#endif

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

struct Attributes {
	float4 positionOS : POSITION;
	float2 uv : TEXCOORD0;
	float3 normalOS : NORMAL;
	float4 color : COLOR;

	UNITY_VERTEX_INPUT_INSTANCE_ID
};
 
struct Varyings {
	float4 positionCS : SV_POSITION;
	float3 positionWS : TEXCOORD1;
	float3 normalWS : TEXCOORD2;
	float2 uv : TEXCOORD0;
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
	OUT.color = IN.color * _BaseColor;
	OUT.uv = TRANSFORM_TEX(IN.uv , _BaseMap);

	return OUT;
}

float4 ToonLitPassFragment (Varyings IN) : SV_TARGET
{
	UNITY_SETUP_INSTANCE_ID(IN);

	half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor * IN.color;

	#ifdef _ALPHACLIP_ON
	clip(albedo.a - _cutout);
	#endif

	//float shadow = 0;
	//for (int i = 0; i < 4 ; i++)
	//{
	//	Light light = GetMainLight(i , IN.positionWS);
	//	shadow += 1 - light.shadowAttenuation;
	//}
 //       shadow = shadow / 4;
	//	shadow = 1 - shadow;

	

	Light light = GetMainLight(0 , IN.positionWS , IN.normalWS);

	float4x4 m = _DirectionalShadowMatrices[0];
	float4 m0 = float4(m[0][0] , m[0][1] ,m[0][2] ,m[0][3]);
	float4 m1 = float4(m[1][0] , m[1][1] ,m[1][2] ,m[1][3]);
	float4 m2 = float4(m[2][0] , m[2][1] ,m[2][2] ,m[2][3]);
	float4 m3 = float4(m[3][0] , m[3][1] ,m[3][2] ,m[3][3]);
    return light.shadowAttenuation;
}

#endif