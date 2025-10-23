#ifndef ROXAMIRP_ACTOR_LIT_PASS_INCLUDE
#define ROXAMIRP_ACTOR_LIT_PASS_INCLUDE

#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
#include "Assets/RoXamiRP/ShaderLibrary/Surface.hlsl"
#include "Assets/RoXamiRP/ShaderLibrary/Light.hlsl"

TEXTURE2D(_SdfFaceMap);
SAMPLER(sampler_SdfFaceMap);

struct Attributes
{
    float4 positionOS : POSITION;
    float2 uv : TEXCOORD0;
    float3 normalOS : NORMAL;
    float4 tangentOS : TANGENT;
    float4 color : COLOR;

    UNITY_VERTEX_INPUT_INSTANCE_ID
};
			 
struct Varyings {
    float4 positionCS : SV_POSITION;
    float2 uv : TEXCOORD0;
    float3 normalWS : TEXCOORD1;
    float3 tangentWS : TEXCOORD2;
    float3 biTangentWS : TEXCOORD3;
    float4 color : COLOR;

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings ActorLitGBufferPassVertex(Attributes IN)
{
    Varyings OUT = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(IN);
    UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

    OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
    OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
    OUT.tangentWS = TransformObjectToWorldNormal(IN.tangentOS.xyz);
    OUT.biTangentWS = GetBiTangent(OUT.normalWS, OUT.tangentWS, IN.tangentOS.w);
    OUT.color = IN.color * _BaseColor;
    OUT.uv = TRANSFORM_TEX(IN.uv , _BaseMap);

    return OUT;
}

Surface GetSurfaceData(Varyings IN)
{
    Surface OUT = (Surface)0;
	
    float4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * IN.color;
				
    float3 mra = float3(_metallic , _roughness , _ao);
    #if defined(_MRA_MAP_ON)
    mra *= SAMPLE_TEXTURE2D(_MRAMap, sampler_MRAMap, IN.uv);
    #endif

    float3 emission = _emissive;
    #if defined(_EMISSIVE_MAP_ON)
    emission *= SAMPLE_TEXTURE2D(_EmissionMap, sampler_EmissionMap, IN.uv).rgb;
    #endif

    #if defined(_NORMAL_MAP_ON)
    float4 normalMap = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, IN.uv);
    float3 normal = TransformNormalMapToNormal(
        normalMap, _normalStrength,
        IN.normalWS, IN.tangentWS, IN.biTangentWS);
    #else
    float3 normal = IN.normalWS;
    #endif
	
    OUT.albedo = albedo.rgb;
    OUT.normal = normal;
    OUT.metallic = mra.r;
    OUT.roughness = mra.g;
    OUT.ao = mra.b;
    OUT.emissive = emission;
    OUT.alpha = albedo.a;

    return OUT;
}

void ActorLitGBufferPassFragment (Varyings IN,
    out float4 GT0 : SV_Target0,
    out float4 GT1 : SV_Target1,
    out float4 GT2 : SV_Target2,
    out float4 GT3 : SV_Target3)
{
    UNITY_SETUP_INSTANCE_ID(IN);
    
    Surface surfaceData = GetSurfaceData(IN);

    #ifdef _ALPHACLIP_ON
    clip(surfaceData.alpha - _cutout);
    #endif

    GT0 = float4(surfaceData.albedo, 1);
    GT1 = float4(surfaceData.normal, 1);
    GT2 = float4(surfaceData.metallic, surfaceData.roughness, surfaceData.ao, 1);
    GT3 = float4(surfaceData.emissive, 1);
}

float SDF_NoL(float2 uv, float3 lightDir)
{
    float4 leftLightShadow = SAMPLE_TEXTURE2D(_SdfFaceMap, sampler_SdfFaceMap, uv);
    float4 rightLightShadow = SAMPLE_TEXTURE2D(_SdfFaceMap, sampler_SdfFaceMap, float2(1 - uv.x , uv.y));
    float2 rightDir_XZ = normalize(half2(1,0));
    float2 lightDir_XZ = normalize(lightDir.xz);
    float2 frontDir_XZ = normalize(_faceFrontDir.xz);
    float isFront = dot(lightDir_XZ , frontDir_XZ);
    float isRight = dot(lightDir.xz , rightDir_XZ);
    float4 sdf_LightShadow = isRight > 0 ? rightLightShadow : leftLightShadow;
    float NoL = (sdf_LightShadow.r - 0.5) * 2 + isFront;
    // NoL = min(0.95, NoL);
    // NoL = max(0.05, NoL);

    return NoL;
}

void ActorLitFaceGBufferPassFragment (Varyings IN,
    out float4 GT0 : SV_Target0,
    out float4 GT1 : SV_Target1,
    out float4 GT2 : SV_Target2,
    out float4 GT3 : SV_Target3)
{
    UNITY_SETUP_INSTANCE_ID(IN);
    
    Surface surfaceData = GetSurfaceData(IN);

    #ifdef _ALPHACLIP_ON
    clip(surfaceData.alpha - _cutout);
    #endif
    
    Light mainLight = GetMainLight();
    float3 faceNormal = lerp(mainLight.direction, - mainLight.direction, SDF_NoL(IN.uv, mainLight.direction));

    GT0 = SDF_NoL(IN.uv, mainLight.direction);//float4(surfaceData.albedo, 1);
    GT1 = float4(faceNormal, 1);
    GT2 = float4(surfaceData.metallic, surfaceData.roughness, surfaceData.ao, 1);
    GT3 = float4(surfaceData.emissive, 1);
}

#endif