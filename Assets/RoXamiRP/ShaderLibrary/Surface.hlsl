#ifndef ROXAMIRP_SURFACE_INCLUDE
#define ROXAMIRP_SURFACE_INCLUDE

struct Surface
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