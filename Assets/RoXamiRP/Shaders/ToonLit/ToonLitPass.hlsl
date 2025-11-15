#ifndef ROXAMIRP_TOONLIT_PASS_INCLUDE
#define ROXAMIRP_TOONLIT_PASS_INCLUDE

#include "Assets/RoXamiRP/ShaderLibrary/Surface.hlsl"
#include "Assets/RoXamiRP/Shaders/Common/CameraDepthAttachment.hlsl"

#ifdef INSTANCING_ON
    UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
        UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
    UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

#define _BaseColor              UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor)
#endif

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
    float4 scrPos : TEXCOORD4;
    float4 color : COLOR;

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

//===================================================================//
//============================Function===============================//
//===================================================================//

void GetDecal(float4 scrPos, out float2 decalUV, out float decalFade)
{
    float2 screenSpaceUV = scrPos.xy / scrPos.w;
    float depth = SampleCameraDepth(screenSpaceUV);
    depth = GetReverseDepth(depth);
    
    float3 positionWS = CalculateDepthToPositionWS(depth, screenSpaceUV);
    float3 positionOS = TransformWorldToObject(positionWS);

    float3 box = abs(positionOS);
    clip(0.5 - box);

    decalUV = positionOS.xz + 0.5;
    decalUV = TRANSFORM_TEX(decalUV , _BaseMap);
    
    half2 fade = smoothstep(0 , _decalFade , 1 - box.xz * 2);
    decalFade = fade.x * fade.y;
}

//===================================================================//
//============================ pass   ===============================//
//===================================================================//

Varyings ToonGBufferPassVertex(Attributes IN)
{
    Varyings OUT = (Varyings)0;
    UNITY_SETUP_INSTANCE_ID(IN);
    UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

    OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
#ifdef _DECAL_GBUFFER
    OUT.normalWS = TransformObjectToWorldNormal(float3(0, 1, 0));
#else
    OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
#endif
    OUT.tangentWS = TransformObjectToWorldNormal(IN.tangentOS.xyz);
    OUT.biTangentWS = GetBiTangent(OUT.normalWS, OUT.tangentWS, IN.tangentOS.w);

    OUT.scrPos = ComputeScreenPos(OUT.positionCS);
    
    OUT.color = IN.color * _BaseColor;
    OUT.uv = TRANSFORM_TEX(IN.uv , _BaseMap);

    return OUT;
}

Surface GetSurfaceData(Varyings IN)
{
    Surface OUT = (Surface)0;

#ifdef _DECAL_GBUFFER
    float decalFade = 0;
    GetDecal(IN.scrPos, IN.uv, decalFade);
#endif
	
    float4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * IN.color;
    
#ifdef _DECAL_GBUFFER
    albedo.a *= decalFade;
#ifdef _DECAL_MASK_MAP_ON
    float decalMask = SAMPLE_TEXTURE2D(_DecalMaskMap, sampler_DecalMaskMap, IN.uv).r;
    albedo.a *= decalMask;
#endif
    
#endif
			
    float3 mra = float3(_metallic , _roughness , _ao);
#if defined(_MRA_MAP_ON)
    mra *= SAMPLE_TEXTURE2D(_MraMap, sampler_MraMap, IN.uv).rgb;
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

void ToonGBufferPassFragment (Varyings IN,
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

    GT0 = float4(surfaceData.albedo, surfaceData.alpha);
    GT1 = float4(surfaceData.normal, surfaceData.alpha);
    GT2 = float4(surfaceData.metallic, surfaceData.roughness, surfaceData.ao, surfaceData.alpha);
    GT3 = float4(surfaceData.emissive, surfaceData.alpha);
}

#endif
