Shader "RoXamiRP/Hide/FullScreenGlobalFog"
{
	Properties
	{
		
	}
	
	SubShader
	{
		Pass
		{
			Name "FullScreenGlobalFog"
			
			Cull Off
			ZWrite Off
			ZTest Always
			Blend SrcAlpha OneMinusSrcAlpha
			
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex FullScreenTriangle
			#pragma fragment FullScreenGlobalFogFragment
			#pragma multi_compile _ _GlobalFog_None _GlobalFog_Linear _GlobalFog_EXP _GlobalFog_EXP2
			#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
			#include "Assets/RoXamiRP/Shaders/RenderPipeline/FullScreenTriangle.hlsl"
			#include "Assets/RoXamiRP/Shaders/Common/ApplyFog.hlsl"

			float4 FullScreenGlobalFogFragment (Varyings IN) : SV_TARGET
			{
				float4 color = 1;
				
				float fog = ComputeFogIntensityWithUV(IN.uv);
				
				color.rgb = _GlobalFogColor;
				color.a = fog;

			    return color;
			}

			ENDHLSL
		}
	}
}