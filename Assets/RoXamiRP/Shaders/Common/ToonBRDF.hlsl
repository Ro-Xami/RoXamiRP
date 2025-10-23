#ifndef ROXAMIRP_TOON_BRDF_INCLUDE
#define ROXAMIRP_TOON_BRDF_INCLUDE

#include "Assets/RoXamiRP/ShaderLibrary/Input.hlsl"
#include "Assets/RoXamiRP/ShaderLibrary/Surface.hlsl"
#include "Assets/RoXamiRP/ShaderLibrary/Light.hlsl"

#define linear_F0 0.04
#define safeDataMin 0.01f

TEXTURE2D(_ToonLitLut);
SAMPLER(sampler_ToonLitLut);

struct ToonBRDF
{
    float3 halfDir;
    float NoH;
    float NoL;
    float NoV;
    float HoV;
    float HoL;
    float3 F0;
    float3 toonDiffuse;
};

ToonBRDF GetToonBRDF(Input inputData, Surface surfaceData, Light light)
{
    ToonBRDF toonBRDF = (ToonBRDF) 0;

    toonBRDF.halfDir = SafeNormalize(inputData.viewWS + light.direction);
    toonBRDF.NoH = max(safeDataMin ,saturate(dot(surfaceData.normal, toonBRDF.halfDir)));
    float NoL = dot(surfaceData.normal, light.direction);
    toonBRDF.NoL = max(safeDataMin ,saturate(NoL * light.shadowAttenuation));
    toonBRDF.NoV = max(safeDataMin ,saturate(dot(surfaceData.normal, inputData.viewWS)));
    toonBRDF.HoV = max(safeDataMin ,saturate(dot(inputData.viewWS,   light.direction)));
    toonBRDF.HoL = max(safeDataMin ,saturate(dot(toonBRDF.halfDir,   light.direction)));
    toonBRDF.F0 = lerp(linear_F0, surfaceData.albedo, surfaceData.metallic);

    // float3 shadowColor = SAMPLE_TEXTURE2D(_ToonLitLut, sampler_ToonLitLut, 0.5).rgb;
    // float3 toonShadow = step(0.999f, light.shadowAttenuation) + lerp(0, half3(0,0,1), light.shadowAttenuation) * light.shadowAttenuation;
    // toonShadow = saturate(toonShadow);
    
    float clampedNoL = clamp((NoL * light.shadowAttenuation * 0.5f + 0.5f), 0.05f, 0.95f);
    toonBRDF.toonDiffuse = SAMPLE_TEXTURE2D(_ToonLitLut, sampler_ToonLitLut, clampedNoL).rgb;
    //toonBRDF.toonDiffuse *= toonShadow;

    return toonBRDF;
}

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

#endif