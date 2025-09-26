#ifndef ROXAMIRP_FULLSCREENTRIANGLE_INCLUDE
#define ROXAMIRP_FULLSCREENTRIANGLE_INCLUDE

#include "Assets/RoXamiRP/ShaderLibrary/Common.hlsl"
#include "Assets/RoXamiRP/Shaders/RenderPipeline/SamplePostSource.hlsl"
#include "Assets/RoXamiRP/Shaders/RenderPipeline/FullScreenTriangle.hlsl"

float4 CopyPassFragment (Varyings IN) : SV_TARGET
{
    return GetSource0(IN.uv);
}

//====================================================================================================
//Bloom
float4 _bloomParam;
#define threshold _bloomParam.x
#define thresholdKnee _bloomParam.y
#define clampMax _bloomParam.z
#define scatter _bloomParam.w
float _bloomIntensity;

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

    return  high + low;// * scatter;
    return lerp(high , low , scatter);
}

//====================================================================================================
//Color Grading
float3 HSV2RGB( float3 c )
{
    float3 rgb = clamp( abs(fmod(c.x*6.0+float3(0.0,4.0,2.0),6)-3.0)-1.0, 0, 1);
    rgb = rgb*rgb*(3.0-2.0*rgb);
    return c.z * lerp( float3(1,1,1), rgb, c.y);
}

float3 RGB2HSV(float3 c)
{
    float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
    float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

float4 _colorAdjustmentFactor;
#define _hue _colorAdjustmentFactor.x
#define _saturation _colorAdjustmentFactor.y
#define _exposure _colorAdjustmentFactor.z
#define _contrast _colorAdjustmentFactor.w
float3 ColorAdjustments(float3 color)
{
    float3 hsv = RGB2HSV(color);

    float h = hsv.x + _hue;
    h = h < 0? h + 1: h > 1? h - 1 : h;
    hsv.x = h;
    hsv.y = saturate(hsv.y + _saturation);
    hsv.z = max(0, hsv.z * max(1, _exposure + 1));
    
    return HSV2RGB(hsv);
}

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
}

//====================================================================================================
//Combine
float4 CombineFragment(Varyings IN) : SV_TARGET
{
    float4 colorAttachment = GetSource0(IN.uv);
    
    float4 col = colorAttachment;

#if defined(_Bloom)
    float4 bloom = GetSource1(IN.uv);
    col.rgb += bloom.rgb * _bloomIntensity;
#endif

#if defined(_ColorAdjustments)
    col.rgb = ColorAdjustments(col);
#endif

#if defined(_ACES_Film_ToneMapping)
    col.rgb = ACES_ToneMapping(col.rgb);
#endif

#if defined(_ACES_Simple_ToneMapping)
    col.rgb = ACES_ToneMapping(col.rgb);
#endif

#if defined(_GT_ToneMapping)
    col.rgb = GT_ToneMapping(col.rgb);
#endif

    return col;
}

float4 FinalBlitFragment(Varyings IN) : SV_TARGET
{
    return GetSource0(IN.uv);
}

#endif
