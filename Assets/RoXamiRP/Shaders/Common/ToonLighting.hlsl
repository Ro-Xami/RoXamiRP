#ifndef ROXAMIRP_SHADOWCASTERPASS_INCLUDE
#define ROXAMIRP_SHADOWCASTERPASS_INCLUDE
#include "Assets//RoXamiRP/ShaderLibrary/Surface.hlsl"
#include "Assets//RoXamiRP/ShaderLibrary/Input.hlsl"
#include "Assets//RoXamiRP/ShaderLibrary/Light.hlsl"
#include "Assets//RoXamiRP/ShaderLibrary/GI.hlsl"

#define linear_F0 0.04

//=================================================Math Function=============================================================
float LinearStep(float minValue, float maxValue, float In)
{
    return saturate((In-minValue) / (maxValue - minValue));
}

float LinearStep_Max(float minValue, float maxValue, float In)
{
    return max(0 , (In-minValue) / (maxValue - minValue));
}

//====================================================BRDF Function=============================================================
float Distribution (float NoH , float roughness)
{
    float roughness2 = pow(roughness, 2);
    return roughness2 / (3.141592654 * pow(pow(NoH, 2) * (roughness2 - 1) + 1, 2));
    //return NoH * NoH * roughness2 + 1;
}

float Sub_Geometry (float DotTerm , float k)
{
    return DotTerm / lerp(DotTerm, 1, k);
}

float Combine_Geometry (float NoL , float NoV , float roughness)
{
    float k = pow((1.0 + roughness), 2) / 0.5;
    return Sub_Geometry(NoL, k) * Sub_Geometry(NoV, k);
}

float3 GGX_Spec (float HoL , float NoH , float3 F0 ,  float roughness)
{
    float roughness2 = pow(roughness , 2);
    float HoL2 = pow(HoL , 2);
    float d = NoH * NoH * (roughness2 - 1) + 1.00001f;
    float nor = roughness * 4 + 2;
    float Spec = roughness2 / ((d * d) * max(0.1 , HoL2) * nor);

    return F0 * Spec;
}

float3 Fresnel_Light (float HoL, float3 F0)
{
    float fresnel = exp2((-5.55473 * HoL - 6.98316) * HoL);
    return lerp(fresnel, 1.0, F0);
}

float3 Fresnel_InLight (float NoV, float roughness, float3 F0)
{
    float fresnel = exp2((-5.55473 * NoV - 6.98316) * NoV);   
    fresnel = pow(fresnel * 2 , 2);
    float3 fresnelCol = F0 + fresnel * saturate(1 - roughness - F0);
    return fresnelCol;
}

float2 LUT_Approx (float roughness, float NoV)
{
    const float4 c0 = { -1, -0.0275, -0.572, 0.022 };
    const float4 c1 = { 1, 0.0425, 1.04, -0.04 };
    float4 r = roughness * c0 + c1;
    float a004 = min(r.x * r.x, exp2(-9.28 * NoV)) * r.x + r.y;
    float2 AB = float2(-1.04, 1.04) * a004 + r.zw;
    return saturate(AB);
}

//=================================================Combine Direct InDirect==================================================
float3 DirectionalLight (float HoL , float NoL , float NoV , float NoH , float3 albedo , float roughness , float metallic , float3 F0 , float3 lightColor , float3 viewDir)
{ 
    float3 Ks = Fresnel_Light(HoL, F0);
    float3 Kd = saturate((1 - Ks)) * (1 - metallic);
    
    //BRDF
    float3 BRDFSpec = GGX_Spec(HoL , NoH , F0 , roughness);
    //BRDFSpec = d * g * f / (4 * NoL * NoV);

    return (Kd * albedo * NoL + BRDFSpec * NoL) * lightColor;
    //return BRDFSpec;
}

float3 InDirectionalLight(float NoV , float3 normalWS, float3 viewWS , float3 albedo , float metallic , float roughness, float occlusion, float3 F0 , GI gi)
{
    float3 SHColor = gi.diffuse;
    float3 Ks = Fresnel_InLight(NoV, roughness, F0);
    float3 Kd = saturate((1 - Ks)) * (1 - metallic);
    float3 InDiffuse = SHColor * Kd * albedo;
    
    float3 F_IndirectionLight = Ks;
    float3 SpecCubeColor = gi.specular;
    float2 LUT = LUT_Approx(roughness, NoV);
    float3 InSpec = SpecCubeColor * (F_IndirectionLight * LUT.r + LUT.g);
    return (InDiffuse + InSpec) * occlusion;
    //return InSpec;
}

//===========================================================================================================================
float4 CalculateToonLighting(Input inputData , Surface surfaceData)
{
    float3 albedo = surfaceData.albedo;
    float3 normalDir = surfaceData.normal;
    float occlusion = surfaceData.ao;
    float roughness = LinearStep( 0.003 , 1 , surfaceData.roughness);
    float metallic = surfaceData.metallic;
    float3 emission = surfaceData.emissive;
    
	Light light = GetMainLight(inputData);
	GI gi = GetGI(inputData , surfaceData);

	float3 additionalLightColor = float3(0 , 0 , 0);
	int additionalLightCount = GetAdditionalLightCount();
	
	UNITY_LOOP
	for (int additionLightIndex = 0 ; additionLightIndex < additionalLightCount ; additionLightIndex++)
	{
		Light additionalLight = GetAdditionalLight(additionLightIndex , inputData);
		float NoL = saturate(dot(additionalLight.direction , normalDir));
		additionalLightColor += additionalLight.color * NoL * additionalLight.shadowAttenuation;
	}
    additionalLightColor *= albedo;

    float3 lightColor = light.color;
    float3 lightDir = normalize(light.direction);
    float3 halfDir = SafeNormalize(inputData.viewWS + lightDir);
    float NoH = max(saturate(dot(normalDir, halfDir)), 0.0001);
    float NoL = max(saturate(dot(normalDir, lightDir)) , 0.0001);
    float NoV = max(saturate(dot(normalDir, inputData.viewWS)), 0.01);
    float HoV = max(saturate(dot(inputData.viewWS, lightDir)), 0.0001);
    float HoL = max(saturate(dot(halfDir, lightDir)), 0.0001);

    NoL *= light.shadowAttenuation;
    float3 F0 = lerp(linear_F0, albedo, metallic);

    //return float4(gi.diffuse, 1);
    
    float4 finalColor = float4(0, 0, 0, 0);
    finalColor.rgb =
        DirectionalLight(HoL, NoL, NoV, NoH, albedo, roughness, metallic, F0 , lightColor, inputData.viewWS) +
        InDirectionalLight(NoV, normalDir, inputData.viewWS, albedo, metallic, roughness, occlusion , F0 , gi) +
        emission +
        additionalLightColor;
    finalColor.a = surfaceData.alpha;

    //return float4(NoL.xxx, 1);
    return finalColor;
}

#endif