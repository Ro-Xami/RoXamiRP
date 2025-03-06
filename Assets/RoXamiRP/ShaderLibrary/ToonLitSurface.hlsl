#ifndef ROXAMIRP_TOONLITSURFACE_INCLUDE
#define ROXAMIRP_TOONLITSURFACE_INCLUDE

struct ToonLitSurface
{
	float3 albedo;
    float3 normal;
    float ao;
    float roughness;
    float metallic;
    float3 emissive;
    float alpha;
};

#endif