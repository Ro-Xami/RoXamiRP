#ifndef ROXAMIRP_TOONLIT_PASS_INCLUDE
#define ROXAMIRP_TOONLIT_PASS_INCLUDE

#include "Assets/RoXamiRP/ShaderLibrary/Surface.hlsl"

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
    float4 color : COLOR;

    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings ToonGBufferPassVertex(Attributes IN)
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
    mra *= SAMPLE_TEXTURE2D(_MraMap, sampler_MraMap, IN.uv);
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

    GT0 = float4(surfaceData.albedo, 0);
    GT1 = float4(surfaceData.normal, 0);
    GT2 = float4(surfaceData.metallic, surfaceData.roughness, surfaceData.ao, 0);
    GT3 = float4(surfaceData.emissive, 0);
}

#endif