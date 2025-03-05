Shader "RoXami RP/Unlit"
{
	Properties
	{
		_BaseColor ("BaseColor" , color) = (1,1,1,1)
	}
	
	SubShader
	{
		HLSLINCLUDE
		#include "../ShaderLibrary/Common.hlsl"
		ENDHLSL

		Pass
		{
			HLSLPROGRAM
			#pragma vertex UnlitPassVertex
			#pragma fragment UnlitPassFragment
			#pragma multi_compile_instancing

			#include "UnlitPass.hlsl"

			ENDHLSL
		}
	}
}