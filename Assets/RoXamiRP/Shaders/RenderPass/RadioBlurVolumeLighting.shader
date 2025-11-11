Shader "RoXamiRP/Hide/RadioBlurVolumeLighting"
{
	Properties
	{
		
	}
	
	SubShader
	{
		HLSLINCLUDE
		#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
		#include "Assets/RoXamiRP/Shaders/RenderPipeline/FullScreenTriangle.hlsl"
		#include "Assets/RoXamiRP/Shaders/Common/CameraColorAttachment.hlsl"
		#include "Assets/RoXamiRP/Shaders/Common/CameraDepthAttachment.hlsl"
		#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
		#include "Assets/RoXamiRP/Shaders/RenderPipeline/SampleTempRtSource.hlsl"
		#include "Assets/RoXamiRP/ShaderLibrary/Light.hlsl"

		float _VolumeLighting_RadioBlur_Intensity;
		
		float4 _VolumeLighting_RadioBlur_FilterParams;
		#define _threshold _VolumeLighting_RadioBlur_FilterParams.x
		#define _thresholdKnee _VolumeLighting_RadioBlur_FilterParams.y
		#define _clampMax _VolumeLighting_RadioBlur_FilterParams.z

		float4 _VolumeLighting_RadioBlur_BlurParams;
		#define _blurCount _VolumeLighting_RadioBlur_BlurParams.x
		#define _blurSize _VolumeLighting_RadioBlur_BlurParams.y
		#define _blurCenter _VolumeLighting_RadioBlur_BlurParams.zw

		float4 _VolumeLighting_TexelSize;
		#define _texelSize _VolumeLighting_TexelSize

		float3 RadioBlurVolumeLighting(float2 uv)
		{
			float2 lightDistanceXY = uv - _blurCenter;
			lightDistanceXY.y *= _texelSize.z / _texelSize.w;
			float lightDistance = saturate(length(lightDistanceXY));
			lightDistance = smoothstep(0, 0.3, lightDistance);
			
			float2 blurDir = (_blurCenter - uv) * _blurSize * _texelSize.w * lightDistance;

			float3 color = 0;
			UNITY_LOOP
			for (int i = 0; i < _blurCount; i++)
			{
				half4 sample = GetSource0(saturate(uv + blurDir * i));
				color += sample.rgb;
			}
			color /= _blurCount;

			return color;
		}
		
		ENDHLSL

		Pass
		{
			Name "RadioBlur Type Filter"
			
			Cull Off
			ZWrite Off
			ZTest Always
			
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex FullScreenTriangle
			#pragma fragment RadioBlurTypeFilterFragment

			float3 ApplyThreshold (float3 color)
			{
			    // User controlled clamp to limit crazy high broken spec
			    color = min(_clampMax, color);

			    // Thresholding,soft the _threshold
			    half brightness = Max3(color.r, color.g, color.b);
			    half softness = clamp(brightness - _threshold + _thresholdKnee, 0.0, 2.0 * _thresholdKnee);
			    softness = (softness * softness) / (4.0 * _thresholdKnee + 1e-4);
			    half multiplier = max(brightness - _threshold, softness) / max(brightness, 1e-4);
			    color *= multiplier;

			    // Clamp colors to positive once in prefilter. Encode can have a sqrt, and sqrt(-x) == NaN. Up/Downsample passes would then spread the NaN.
			    color = max(color, 0);

			    return color;
			    //return EncodeHDR(color);
			}

			float4 RadioBlurTypeFilterFragment (Varyings IN) : SV_TARGET
			{
			    float3 color = GetSource0(IN.uv).rgb;
				float3 filter = ApplyThreshold(color);

				float depth = SampleCameraDepth(IN.uv);
				float skybox = 0;
			    #if defined(UNITY_REVERSED_Z)
			        if (depth <= FLT_MIN)
			        {
			            skybox = 1;
			        }
			    #else
			        depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, depth);
			        if (depth >= 1)
			        {
			            skybox = 1;
			        }
			    #endif
				
			    return float4(filter * skybox, 1);
			}
			ENDHLSL
		}

		Pass
		{
			Name "RadioBlur Type Blur"
			
			Cull Off
			ZWrite Off
			ZTest Always
			
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex FullScreenTriangle
			#pragma fragment RadioBlurTypeBlurFragment

			float4 RadioBlurTypeBlurFragment (Varyings IN) : SV_TARGET
			{
				return float4(RadioBlurVolumeLighting(IN.uv), 1);
			}
			ENDHLSL
		}

		Pass
		{
			Name "Combine"
			
			Cull Off
			ZWrite Off
			ZTest Always
			Blend One One
			
			HLSLPROGRAM
			#pragma target 3.5
			#pragma vertex FullScreenTriangle
			#pragma fragment CombineFragment

			float4 CombineFragment (Varyings IN) : SV_TARGET
			{
				float3 volumeLight = RadioBlurVolumeLighting(IN.uv);

				Light mainLight = GetMainLight();
				float3 color = volumeLight * mainLight.color * _VolumeLighting_RadioBlur_Intensity;

			    return float4(color, 1);
			}
			ENDHLSL
		}
	}
}