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

CBUFFER_START(_CustomShadows)
	float4x4 _DirectionalShadowMatrices[MAX_CASCADE_COUNT];
	float4 _DirectionalLightShadowData;
	int _CascadeCount;
	float4 _CascadeCullingSpheres[MAX_CASCADE_COUNT];
	float4 _ShadowDistanceFade;
CBUFFER_END

struct DirectionalShadowData
{
	float strength;
	int tileIndex;
	float normalBias;
};

struct ShadowData 
{
	int cascadeIndex;
	float cascadeCull;

};

DirectionalShadowData GetDirectionalShadowData(ShadowData shadowData) 
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

ShadowData GetShadowData (float3 positionWS) 
{
	ShadowData data;

	//�����ü�
	float depth = -TransformWorldToView(positionWS).z;
	data.cascadeCull = FadedShadowStrength(depth, _ShadowDistanceFade.x, _ShadowDistanceFade.y);

	int i;//���㼶����Χ��ķ�Χ
	for (i = 0; i < _CascadeCount; i++) 
	{
		float4 sphere = _CascadeCullingSpheres[i];
		float distanceSqr = DistanceSquared(positionWS, sphere.xyz);
		if (distanceSqr < sphere.w) {break;}
	}
	if (i == _CascadeCount) {data.cascadeCull = 0.0;}//�޳�����������Χ��Ĳ���
	data.cascadeIndex = i;

	return data;
}

//������Ӱͼ��
float SampleDirectionalShadowAtlas (float3 positionSTS)
{
	return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowAtlas, SHADOW_SAMPLER, positionSTS);
}

//��ȡ��Ӱ
float GetDirectionalShadowAttenuation (DirectionalShadowData data, float3 positionWS) 
{
	if (data.strength != 0)
	{
		float3 positionSTS = mul(_DirectionalShadowMatrices[data.tileIndex] , float4(positionWS, 1.0)).xyz;
		float shadow = SampleDirectionalShadowAtlas(positionSTS);

		return lerp(1.0, shadow, data.strength);
	}
	return 1;
}

#endif