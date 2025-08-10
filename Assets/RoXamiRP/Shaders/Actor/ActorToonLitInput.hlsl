#ifndef ACTORTOON_LIT_INPUT_INCLUDE
#define ACTORTOON_LIT_INPUT_INCLUDE

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);
TEXTURE2D(_MraMap);
SAMPLER(sampler_MraMap);
TEXTURE2D(_NormalMap);
SAMPLER(sampler_NormalMap);
TEXTURE2D(_EmissionMap);
SAMPLER(sampler_EmissionMap);
TEXTURE2D(_LutMap);
SAMPLER(sampler_LutMap);

CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
float4 _BaseColor;
float _cutout;
float _roughness;
float _metallic;
float _ao;
float _normalStrength;
float3 _emissive;
CBUFFER_END

#endif