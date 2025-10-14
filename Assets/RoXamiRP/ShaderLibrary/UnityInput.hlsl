#ifndef ROXAMIRP_UNITY_INPUT_INCLUDED
#define ROXAMIRP_UNITY_INPUT_INCLUDED

CBUFFER_START(UnityPerDraw)
	float4x4 unity_ObjectToWorld;
	float4x4 unity_WorldToObject;
	float4 unity_LODFade;
	real4 unity_WorldTransformParams;

	//GI
	// float4 _RoXamiRP_SHAr;
	// float4 _RoXamiRP_SHAg;
	// float4 _RoXamiRP_SHAb;
	// float4 _RoXamiRP_SHBr;
	// float4 _RoXamiRP_SHBg;
	// float4 _RoXamiRP_SHBb;
	// float4 _RoXamiRP_SHC;
CBUFFER_END

float4x4 unity_MatrixVP;
float4x4 unity_MatrixV;
float4x4 unity_MatrixInvV;
float4x4 unity_prev_MatrixM;
float4x4 unity_prev_MatrixIM;
float4x4 glstate_matrix_projection;

float3 _WorldSpaceCameraPos;

float4 _ProjectionParams;

// Values used to linearize the Z buffer (http://www.humus.name/temp/Linearize%20depth.txt)
// x = 1-far/near
// y = far/near
// z = x/far
// w = y/far
// or in case of a reversed depth buffer (UNITY_REVERSED_Z is 1)
// x = -1+far/near
// y = 1
// z = x/far
// w = 1/far
float4 _ZBufferParams;

#endif