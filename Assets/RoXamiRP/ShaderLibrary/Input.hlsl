#ifndef ROXAMIRP_INPUT_INCLUDE
#define ROXAMIRP_INPUT_INCLUDE

struct Input
{
	float3  positionWS;
    float4  positionCS;
    float3  normalWS;
    float3  viewWS;
    float2  screenSpaceUV;
};

#endif