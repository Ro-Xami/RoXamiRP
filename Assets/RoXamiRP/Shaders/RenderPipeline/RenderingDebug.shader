Shader "RoXamiRP/Hide/RenderingDebug"
{
	SubShader
	{
		Cull Off
		ZTest Always
		ZWrite Off
		
		HLSLINCLUDE
		
		ENDHLSL

		Pass
		{
			Name "BlitFullScreenTriangle"
			
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex FullScreenTriangle
			#pragma fragment BlitFullScreenTriangleFragment
			#pragma multi_compile _ SCREENSPACE_SHADOWS
			#pragma multi_compile _ SCREENSPACE_REFLECTION
			#pragma multi_compile _ _Debug_Albedo _Debug_Normal _Debug_Metallic _Debug_Roughness _Debug_Ao _Debug_Emission _Debug_GiDiffuse _Debug_GiSpecular _Debug_Shadow

			#include "Assets/RoXamiRP/Shaders/DeferredLit/DeferredInput.hlsl"
			#include "Assets/RoXamiRP/Shaders/Common/ToonLighting.hlsl"

			Surface GetSurfaceData(Varyings IN)
			{
				Surface OUT = (Surface)0;
				float4 base = SampleAlbedo(IN.uv);
				float4 nomral = SampleNormal(IN.uv);
				float4 MRA = SampleMRA(IN.uv);
				float4 emission = SamplerEmission(IN.uv);
				OUT.albedo = base.rgb;
				OUT.normal = nomral.xyz;
				OUT.roughness = MRA.g;
				OUT.metallic = MRA.r;
				OUT.ao = MRA.b;
				OUT.emissive = emission.rgb;
				OUT.alpha = base.a;

				return OUT;
			}

			float4 BlitFullScreenTriangleFragment(Varyings IN) : SV_Target
			{
				Surface surface = GetSurfaceData(IN);
				float depth;
				Input inputData = GetInputData(IN, surface, depth);

				Light light = GetMainLight(inputData);
				GI gi = GetGI(inputData , surface);

				float3 finalColor;

				#ifdef _Debug_Albedo
			    finalColor = surface.albedo;
			    #endif

			    #ifdef _Debug_Normal
			    finalColor = surface.normal;
			    #endif

			    #ifdef _Debug_Metallic
			    finalColor = surface.metallic.xxx;
			    #endif

			    #ifdef _Debug_Roughness
			    finalColor = surface.roughness.xxx;
			    #endif

			    #ifdef _Debug_Ao
			    finalColor = surface.ao.xxx;
			    #endif

			    #ifdef _Debug_Emission
			    finalColor = surface.emissive;
			    #endif

			    #ifdef _Debug_GiDiffuse
			    finalColor = gi.diffuse;
			    #endif

			    #ifdef _Debug_GiSpecular
				surface.roughness = 0;
			    finalColor = gi.specular;
			    #endif

				return float4(finalColor, 1);
			}
			ENDHLSL
		}
	}
}