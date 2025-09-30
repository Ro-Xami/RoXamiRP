#ifndef ROXAMIRP_POST_PASS_INCLUDE
#define ROXAMIRP_POST_PASS_INCLUDE

#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
#include "Assets/RoXamiRP/Shaders/RenderPipeline/SampleTempRtSource.hlsl"
#include "Assets/RoXamiRP/Shaders/RenderPipeline/FullScreenTriangle.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

float4 CopyPassFragment (Varyings IN) : SV_TARGET
{
    return GetSource0(IN.uv);
}

//====================================================================================================
//Bloom
float4 _PostBloomParams;
#define threshold _PostBloomParams.x
#define thresholdKnee _PostBloomParams.y
#define clampMax _PostBloomParams.z
#define scatter _PostBloomParams.w
float _PostBloomIntensity;

float3 ApplyBloomThreshold (float3 color)
{
    // User controlled clamp to limit crazy high broken spec
    color = min(clampMax, color);

    // Thresholding,soft the threshold
    half brightness = Max3(color.r, color.g, color.b);
    half softness = clamp(brightness - threshold + thresholdKnee, 0.0, 2.0 * thresholdKnee);
    softness = (softness * softness) / (4.0 * thresholdKnee + 1e-4);
    half multiplier = max(brightness - threshold, softness) / max(brightness, 1e-4);
    color *= multiplier;

    // Clamp colors to positive once in prefilter. Encode can have a sqrt, and sqrt(-x) == NaN. Up/Downsample passes would then spread the NaN.
    color = max(color, 0);

    return color;
    //return EncodeHDR(color);
}

float4 BloomPrefilterPassFragment (Varyings IN) : SV_TARGET
{
    float3 color = ApplyBloomThreshold(GetSource0(IN.uv).rgb);
    return float4(color, 1.0);
}

float4 BloomHorizontalPassFragment (Varyings IN) : SV_TARGET
{
    float3 color = 0.0;
    float offsets[] = {
        -3.23076923, -1.38461538, 0.0, 1.38461538, 3.23076923
    };
    float weights[] = {
        0.07027027, 0.31621622, 0.22702703, 0.31621622, 0.07027027
    };
    for (int i = 0; i < 5; i++) {
        float offset = offsets[i] * 2.0 * GetSourceTexelSize().x;
        color += GetSource0(IN.uv + float2(offset, 0.0)).rgb * weights[i];
    }
    return float4(color, 1.0);
}

float4 BloomVerticalPassFragment (Varyings IN) : SV_TARGET
{
    float3 color = 0.0;
    float offsets[] = {
        -4.0, -3.0, -2.0, -1.0, 0.0, 1.0, 2.0, 3.0, 4.0
    };
    float weights[] = {
        0.01621622, 0.05405405, 0.12162162, 0.19459459, 0.22702703,
        0.19459459, 0.12162162, 0.05405405, 0.01621622
    };
    for (int i = 0; i < 9; i++) {
        float offset = offsets[i] * GetSourceTexelSize().y;
        color += GetSource0(IN.uv + float2(0.0, offset)).rgb * weights[i];
    }
    return float4(color, 1.0);
}

float4 BloomUpSamplePassFragment (Varyings IN) : SV_TARGET
{
    float4 low = GetSourceBicubic(IN.uv);
    float4 high = GetSource1(IN.uv);

    //return  high + low;// * scatter;
    return lerp(high , low , scatter);
}

//====================================================================================================
//Color Adjustments
#define ACEScc_MID_GRAY 0.4135884
float3 _PostColorFilter;
float4 _PostColorAdjustmentsParams;
#define _hue _PostColorAdjustmentsParams.x
#define _saturation _PostColorAdjustmentsParams.y
#define _exposure _PostColorAdjustmentsParams.z
#define _contrast _PostColorAdjustmentsParams.w

float3 ColorAdjustments(float3 color)
{
    //Exposure
    color *= max(1, _exposure + 1);
    //Contrast
    color = LinearToLogC(color);
    color = (color - ACEScc_MID_GRAY) * (_contrast + 1) + ACEScc_MID_GRAY;
    color = LogCToLinear(color);
    //ColorFilter
    color *= _PostColorFilter;

    color = max(color, 0.0);
    
    //Hue
    float3 hsv = RgbToHsv(color);
    float h = hsv.x + _hue;
    h = h < 0? h + 1: h > 1? h - 1 : h;
    hsv.x = h;
    color = HsvToRgb(hsv);
    //Saturation
    float luma = Luminance(color);
    color = (color - luma) * (_saturation + 1) + luma;

    color = max(0.0, color);
    
    return color;
}

//====================================================================================================
//White Balance
float4 _Post_WhiteBalanceParams;

float3 WhiteBalance(float3 color)
{
    color = LinearToLMS(color);
    color *= _Post_WhiteBalanceParams.rgb;
    return LMSToLinear(color);
}

//====================================================================================================
//Tone Mapping
//https://knarkowicz.wordpress.com/2016/01/06/aces-filmic-tone-mapping-curve/
float3 ACESFilm(float3 x)
{
    float a = 2.51f;
    float b = 0.03f;
    float c = 2.43f;
    float d = 0.59f;
    float e = 0.14f;
    return saturate((x*(a*x+b))/(x*(c*x+d)+e));
}

//https://www.desmos.com/calculator/zygyam5cg3?lang=zh-CN
float3 ACES_ToneMapping(float3 x)
{
    float a = 1.36f;
    float b = 0.047f;
    float c = 0.93f;
    float d = 0.56f;
    float e = 0.14f;
    return saturate((x*(a*x+b))/(x*(c*x+d)+e));
}

//https://www.desmos.com/calculator/lojift1sad?lang=zh-CN
float W_f(float x, float e0, float e1)
{
    if (x <= e0)
        return 0;
    if (x >= e1)
        return 1;
    float a = (x - e0) / (e1 - e0);
    return a * a * (3 - 2 * a);
}

float H_f(float x, float e0, float e1)
{
    if (x <= e0)
        return 0;
    if (x >= e1)
        return 1;
    return (x - e0) / (e1 - e0);
}

float3 GT_ToneMapping(float3 x)
{
    return x;
    /*
    float P = 1; // Maximum brightness
    float a = 1; // Contrast
    float m = 0.22; // Linear section start
    float l = 0.4; // Linear section length
    float c = 1.33; // Black pow  def 1 
    float b = 0; // Black min
    float3 l0 = (P - m) * l / a;
    float3 L0 = m - m / a;
    float3 L1 = m + (1 - m) / a;
    float3 L_x = m + a * (x - m);
    float3 T_x = m * pow(x / m, c) + b;
    float3 S0 = m + l0;
    float3 S1 = m + a * l0;
    float3 C2 = a * P / (P - S1);
    float3 S_x = P - (P - S1) * exp(-(C2 * (x - S0) / P));
    float3 w0_x = 1 - W_f(x, 0, m);
    float3 w2_x = H_f(x, m + l0, m + l0);
    float3 w1_x = 1 - w0_x - w2_x;
    float3 f_x = T_x * w0_x + L_x * w1_x + S_x * w2_x;
    
    return f_x;
    */
}

//====================================================================================================
//Combine
float4 CombineFragment(Varyings IN) : SV_TARGET
{
    float4 colorAttachment = GetSource0(IN.uv);
    
    float4 col = colorAttachment;

#if defined(_Post_Bloom_ON)
    float4 bloom = GetSource1(IN.uv);
    col.rgb += bloom.rgb * _PostBloomIntensity;
#endif

#if defined(_Post_ColorAdjustments_ON)
    col.rgb = ColorAdjustments(col.rgb);
#endif

#if defined(_Post_WhiteBalance_ON)
    col.rgb = WhiteBalance(col.rgb);
#endif

#if defined(_Post_AcesFilm_ON)
    col.rgb = ACES_ToneMapping(col.rgb);
#endif

#if defined(_Post_AcesSimple_ON)
    col.rgb = ACES_ToneMapping(col.rgb);
#endif

#if defined(_Post_GT_ON)
    col.rgb = GT_ToneMapping(col.rgb);
#endif

    return col;
}

float4 FinalBlitFragment(Varyings IN) : SV_TARGET
{
    return GetSource0(IN.uv);
}

#endif
