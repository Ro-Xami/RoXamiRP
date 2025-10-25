#ifndef ACTORTOON_LIT_INPUT_INCLUDE
#define ACTORTOON_LIT_INPUT_INCLUDE

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);
TEXTURE2D(_MRAMap);
SAMPLER(sampler_MRAMap);
TEXTURE2D(_NormalMap);
SAMPLER(sampler_NormalMap);
TEXTURE2D(_EmissionMap);
SAMPLER(sampler_EmissionMap);
TEXTURE2D(_MatcapMap);
SAMPLER(sampler_MatcapMap);

CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
float4 _BaseColor;
float _cutout;
float _roughness;
float _metallic;
float _ao;
float _normalStrength;
float3 _emissive;

//Matcap
float _matcapIntensity;

//Face
float4 _faceFrontRightDir;
#define _faceFrontDirXZ _faceFrontRightDir.xy
#define _faceRightDirXZ _faceFrontRightDir.zw

CBUFFER_END

#ifdef INSTANCING_ON
    UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
        UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
    UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

#define _BaseColor              UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor)
#endif

#endif