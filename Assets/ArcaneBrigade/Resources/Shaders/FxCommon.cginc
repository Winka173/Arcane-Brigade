// Noise helpers shared by every FxProc shape file.
#ifndef FX_COMMON
#define FX_COMMON
float hash(float2 x) { return frac(sin(dot(x, float2(127.1, 311.7))) * 43758.5453); }
float vnoise(float2 x)
{
    float2 i = floor(x), f = frac(x); f = f * f * (3 - 2 * f);
    return lerp(lerp(hash(i), hash(i + float2(1, 0)), f.x), lerp(hash(i + float2(0, 1)), hash(i + float2(1, 1)), f.x), f.y);
}
float fbm(float2 x) { return vnoise(x) * .5 + vnoise(x * 2.03 + 7.1) * .25 + vnoise(x * 4.1 + 3.7) * .125 + vnoise(x * 8.3 + 1.3) * .0625; }

#endif
