#ifndef ROXAMIRP_COMMON_INCLUDE
#define ROXAMIRP_COMMON_INCLUDE

//====================================CoreRP Include================================
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "Assets/RoXamiRP/ShaderLibrary/UnityInput.hlsl"

#define UNITY_MATRIX_M unity_ObjectToWorld
#define UNITY_MATRIX_I_M unity_WorldToObject
#define UNITY_MATRIX_V unity_MatrixV
#define UNITY_MATRIX_I_V unity_MatrixInvV
#define UNITY_MATRIX_VP unity_MatrixVP
#define UNITY_PREV_MATRIX_M unity_prev_MatrixM
#define UNITY_PREV_MATRIX_I_M unity_prev_MatrixIM
#define UNITY_MATRIX_P glstate_matrix_projection

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

//====================================RoXamiRP Include=================================s
#include "Assets/RoXamiRP/ShaderLibrary/RoXamiInput.hlsl"
#define MATRIX_I_VP _RoXamiRP_MatrixInvVP
//#define ROXAMIRP_UV_STARTS_AT_TOP _ProjectionParams.x

float Square(float x) { return x * x;} 
float2 Square(float2 x) { return x * x;} 
float3 Square(float3 x) { return x * x;} 
float4 Square(float4 x) { return x * x;} 

float3 GetViewDirWS(float3 positionWS)
{
	return normalize(_WorldSpaceCameraPos - positionWS);
}

// Deprecated: A confusingly named and duplicate function that scales clipspace to unity NDC range. (-w < x(-y) < w --> 0 < xy < w)
// Use GetVertexPositionInputs().positionNDC instead for vertex shader
// Or a similar function in Common.hlsl, ComputeNormalizedDeviceCoordinatesWithZ()
float4 ComputeScreenPos(float4 positionCS)
{
	float4 o = positionCS * 0.5f;
	o.xy = float2(o.x, o.y * _ProjectionParams.x) + o.w;
	o.zw = positionCS.zw;
	return o;
}

#endif