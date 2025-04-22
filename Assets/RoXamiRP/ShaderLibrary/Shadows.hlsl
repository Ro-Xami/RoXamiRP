#ifndef CUSTOM_SHADOWS_INCLUDED
#define CUSTOM_SHADOWS_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Shadow/ShadowSamplingTent.hlsl"

//PCF
#if defined(_DIRECTIONAL_PCF3)
	#define DIRECTIONAL_FILTER_SAMPLES 4
	#define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_3x3
#elif defined(_DIRECTIONAL_PCF5)
	#define DIRECTIONAL_FILTER_SAMPLES 9
	#define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_5x5
#elif defined(_DIRECTIONAL_PCF7)
	#define DIRECTIONAL_FILTER_SAMPLES 16
	#define DIRECTIONAL_FILTER_SETUP SampleShadow_ComputeSamples_Tent_7x7
#endif

//�������
#define MAX_CASCADE_COUNT 4

//��Ӱͼ��
TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_RoXamiShadows)
	float4x4 _DirectionalShadowMatrices[MAX_CASCADE_COUNT];
	float4 _DirectionalLightShadowData;//shadowStrength normaliBias cascadeCount
	float4 _CascadeCullingSpheres[MAX_CASCADE_COUNT];
	float4 _ShadowDistanceFade;
CBUFFER_END

struct DirectionalShadowData
{
	float strength;
	int tileIndex;
	float normalBias;
};

struct ShadowCascadeData 
{
	int cascadeIndex;
	float cascadeCull;
};

DirectionalShadowData GetDirectionalShadowData(ShadowCascadeData shadowData) 
{
	DirectionalShadowData data;
	data.strength = _DirectionalLightShadowData.x * shadowData.cascadeCull;
	data.tileIndex = shadowData.cascadeIndex;
	return data;
}

//����ƽ�����룬���ڼ����Ƿ񳬹��˼�����Χ��
float DistanceSquared(float3 pA, float3 pB) 
{
	return dot(pA - pB, pA - pB);
}

//�޳�����
float FadedShadowStrength (float distance, float scale, float fade) {
	return saturate((1.0 - distance * scale) * fade);
}

ShadowCascadeData GetShadowCascadeData (float3 positionWS) 
{
	ShadowCascadeData data = (ShadowCascadeData) 0;

	//�����ü�
	float depth = -TransformWorldToView(positionWS).z;
	data.cascadeCull = FadedShadowStrength(depth, _ShadowDistanceFade.x, _ShadowDistanceFade.y);

	int i;//���㼶����Χ��ķ�Χ
	for (i = 0; i < _DirectionalLightShadowData.z; i++) 
	{
		float4 sphere = _CascadeCullingSpheres[i];
		float distanceSqr = DistanceSquared(positionWS, sphere.xyz);
		if (distanceSqr < sphere.w) {break;}
	}
	if (i == _DirectionalLightShadowData.z) {data.cascadeCull = 0.0;}//�޳�����������Χ��Ĳ���
	data.cascadeIndex = i;

	return data;
}

//������Ӱͼ��
float SampleDirectionalShadowAtlas (float3 positionSTS)
{
	return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowAtlas, SHADOW_SAMPLER, positionSTS);
}

//��ȡ��Ӱ
float GetDirectionalShadowAttenuation (float3 positionWS , float3 normalWS) 
{
	ShadowCascadeData cascadeData = GetShadowCascadeData(positionWS);
	DirectionalShadowData data = GetDirectionalShadowData(cascadeData);

	float shadow = 1;
	if (data.strength != 0)
	{
		float3 normalBias = normalWS * _DirectionalLightShadowData.y;
		float3 positionSTS = mul(_DirectionalShadowMatrices[data.tileIndex] , float4(positionWS + normalBias, 1.0)).xyz;
		shadow = SampleDirectionalShadowAtlas(positionSTS);
		shadow = lerp(1.0, shadow, data.strength);
	}
	return shadow;
}

#endif