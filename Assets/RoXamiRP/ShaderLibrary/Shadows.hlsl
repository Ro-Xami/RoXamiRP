#ifndef ROXAMIRP_SHADOWS_INCLUDED
#define ROXAMIRP_SHADOWS_INCLUDED

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

//最大级联数
#define MAX_CASCADE_COUNT 4

//阴影图集
TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(RoXamiShadows)
	float4x4 _DirectionalShadowMatrices[MAX_CASCADE_COUNT];
	float4 _DirectionalLightShadowData;//shadowStrength normaliBias cascadeCount atlasSize
	float4 _CascadeCullingSpheres[MAX_CASCADE_COUNT];
	float4 _ShadowDistanceFade;
CBUFFER_END

struct DirectionalShadowData
{
	float strength;
	float3 normalBias;
};

struct ShadowCascadeData 
{
	int cascadeIndex;
	float cascadeCull;
	float cascadeBlend;
};

DirectionalShadowData GetDirectionalShadowData(ShadowCascadeData shadowData , float3 normalWS) 
{
	DirectionalShadowData data;
	data.strength = _DirectionalLightShadowData.x * shadowData.cascadeCull;
	data.normalBias = _DirectionalLightShadowData.y;

	return data;
}

//计算平方距离，用于计算是否超过了级联包围球
float DistanceSquared(float3 pA, float3 pB) 
{
	return dot(pA - pB, pA - pB);
}

//剔除过渡
float FadedShadowStrength (float distance, float scale, float fade) {
	return saturate((1.0 - distance / scale) / fade);
}

ShadowCascadeData GetShadowCascadeData (float3 positionWS) 
{
	ShadowCascadeData data = (ShadowCascadeData) 0;

	data.cascadeBlend = 1;
	//淡化裁剪
	float depth = -TransformWorldToView(positionWS).z;
	data.cascadeCull = FadedShadowStrength(depth, _ShadowDistanceFade.x, _ShadowDistanceFade.y);

	int i;//计算级联包围球的范围
	for (i = 0; i < _DirectionalLightShadowData.z; i++) 
	{
		float4 sphere = _CascadeCullingSpheres[i];
		float distanceSqr = DistanceSquared(positionWS, sphere.xyz);

		if (distanceSqr < sphere.w) 
		{
			//淡化最后一个级联
			if (i == _DirectionalLightShadowData.z - 1)
			{
				data.cascadeCull *= FadedShadowStrength(distanceSqr , sphere.w , _ShadowDistanceFade.z);
			}
			break;
		}
	}

	if (i == _DirectionalLightShadowData.z) {data.cascadeCull = 0.0; }//剔除超过级联包围球的部分

	data.cascadeIndex = i;

	return data;
}

//采样阴影图集
float SampleDirectionalShadowAtlas (float3 positionSTS)
{
	return SAMPLE_TEXTURE2D_SHADOW(_DirectionalShadowAtlas, SHADOW_SAMPLER, positionSTS);
}

//PCF软化阴影
float FilterDirectionalShadow (float3 positionSTS) {
	#if defined(DIRECTIONAL_FILTER_SETUP)
		float weights[DIRECTIONAL_FILTER_SAMPLES];
		float2 positions[DIRECTIONAL_FILTER_SAMPLES];
		float atlasSize = _DirectionalLightShadowData.w;
		float dAtlasSize = 1 / atlasSize;
		float4 size = float4(dAtlasSize , dAtlasSize , atlasSize , atlasSize);
		DIRECTIONAL_FILTER_SETUP(size, positionSTS.xy, weights, positions);
		float shadow = 0;
		for (int i = 0; i < DIRECTIONAL_FILTER_SAMPLES; i++) {
			shadow += weights[i] * SampleDirectionalShadowAtlas(
				float3(positions[i].xy, positionSTS.z)
			);
		}
		return shadow;
	#else
		return SampleDirectionalShadowAtlas(positionSTS);
	#endif
}

//获取阴影
float GetDirectionalShadowAttenuation (float3 positionWS , float3 normalWS) 
{
	ShadowCascadeData cascadeData = GetShadowCascadeData(positionWS);
	DirectionalShadowData data = GetDirectionalShadowData(cascadeData , normalWS);

	float shadow = 1;
	if (data.strength != 0)
	{
		//根据包围球半径和级联贴图大小做偏移
		float3 normalBias = 2 * _CascadeCullingSpheres[cascadeData.cascadeIndex].w / (_DirectionalLightShadowData.w * 1.4142136f);
		normalBias *= normalWS * data.normalBias;
		float3 positionSTS = mul(_DirectionalShadowMatrices[cascadeData.cascadeIndex] , float4(positionWS + normalBias, 1.0)).xyz;
		shadow = FilterDirectionalShadow(positionSTS);
		shadow = lerp(1.0, shadow, data.strength);
	}
	return shadow;
}

#endif