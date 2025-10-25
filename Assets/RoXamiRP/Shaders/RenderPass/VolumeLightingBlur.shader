Shader "RoXamiRP/Hide/VolumeLightingBlur"
{
	Properties
	{
		
	}
	
	SubShader
	{
		Pass
		{
			Name "VolumeLighting Blur"
			
			Cull Off
			ZWrite Off
			ZTest Always
			//Blend SrcAlpha OneMinusSrcAlpha
			Blend One One
			
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex FullScreenTriangle
			#pragma fragment VolumeLightingBlurFragment
			#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
			#include "Assets/RoXamiRP/Shaders/RenderPipeline/FullScreenTriangle.hlsl"
			#include "Assets/RoXamiRP/ShaderLibrary/Light.hlsl"

			TEXTURE2D(_VolumeLightingTexture);
			SAMPLER(sampler_VolumeLightingTexture);

			float _volumeLightIntensity;
			float _volumeLightPower;
			float3 _volumeLightColor;
			float4 _volumeLightBlurTexelSize;
			#define _texelSize _volumeLightBlurTexelSize

			float SampleVolumeLight(float2 uv)
			{
				return SAMPLE_TEXTURE2D(_VolumeLightingTexture, sampler_VolumeLightingTexture, uv).r;
			}

			float4 VolumeLightingBlurFragment (Varyings IN) : SV_TARGET
			{
				float volumeLight = 0;
				volumeLight += SampleVolumeLight(IN.uv + float2(0.0, 0.0)) * 0.147716f;
				
                volumeLight += SampleVolumeLight(IN.uv + float2(_texelSize.z, 0.0)) * 0.118318f;
                volumeLight += SampleVolumeLight(IN.uv + float2(0.0, -_texelSize.w)) * 0.118318f;
                volumeLight += SampleVolumeLight(IN.uv + float2(0.0, _texelSize.w)) * 0.118318f;
                volumeLight += SampleVolumeLight(IN.uv + float2(-_texelSize.z, 0.0)) * 0.118318f;

                volumeLight += SampleVolumeLight(IN.uv + float2(_texelSize.z, _texelSize.w)) * 0.0947416f;
                volumeLight += SampleVolumeLight(IN.uv + float2(-_texelSize.z, -_texelSize.w)) * 0.0947416f;
                volumeLight += SampleVolumeLight(IN.uv + float2(_texelSize.z, -_texelSize.w)) * 0.0947416f;
                volumeLight += SampleVolumeLight(IN.uv + float2(-_texelSize.z, _texelSize.w)) * 0.0947416f;

				volumeLight = pow(volumeLight, max(0.0001f, _volumeLightPower));

				Light mainLight = GetMainLight();
				float3 color = volumeLight * _volumeLightColor * mainLight.color * _volumeLightIntensity;

			    return float4(color, 1);
			}

			ENDHLSL
		}
	}
}