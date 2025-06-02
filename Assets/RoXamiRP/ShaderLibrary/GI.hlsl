#ifndef ROXAMIRP_GI_INCLUDE
#define ROXAMIRP_GI_INCLUDE
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/EntityLighting.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/ImageBasedLighting.hlsl"
#include "Input.hlsl"
#include "Surface.hlsl"

TEXTURECUBE(unity_SpecCube0);
SAMPLER(samplerunity_SpecCube0);

CBUFFER_START(RoXamiGI)
	float4 unity_SHAr;
	float4 unity_SHAg;
	float4 unity_SHAb;
	float4 unity_SHBr;
	float4 unity_SHBg;
	float4 unity_SHBb;
	float4 unity_SHC;

	float4 unity_SpecCube0_HDR;
CBUFFER_END

struct GI
{
	float3 diffuse;
	float3 specular;
};

float3 SampleLightProbe (float3 normal) 
{
	float4 coefficients[7];
	coefficients[0] = unity_SHAr;
	coefficients[1] = unity_SHAg;
	coefficients[2] = unity_SHAb;
	coefficients[3] = unity_SHBr;
	coefficients[4] = unity_SHBg;
	coefficients[5] = unity_SHBb;
	coefficients[6] = unity_SHC;
	return max(0.0, SampleSH9(coefficients, normal));
}

float3 SampleEnvironment (float3 normalWS , float3 viewWS , float roughness) {
	float3 uvw = reflect(-viewWS, normalWS);
	float mip = PerceptualRoughnessToMipmapLevel(roughness);
	float4 environment = SAMPLE_TEXTURECUBE_LOD(
		unity_SpecCube0, samplerunity_SpecCube0, uvw, mip
	);
	return DecodeHDREnvironment(environment, unity_SpecCube0_HDR);
}

GI GetGI(Input inputData , Surface surfaceData)
{
	GI OUT = (GI)0;
	float mip = PerceptualRoughnessToMipmapLevel(surfaceData.roughness);
	float3 reflectionDir = reflect(-inputData.viewWS , inputData.normalWS);
	OUT.diffuse = SampleLightProbe(inputData.normalWS);
	OUT.specular = SampleEnvironment(inputData.normalWS , inputData.viewWS , mip);

	return OUT;
}

#endif