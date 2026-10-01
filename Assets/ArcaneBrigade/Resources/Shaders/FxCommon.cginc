// Noise helpers shared by every FxProc shape file. Sin-free hash (no blocky precision artefacts on large inputs), and a
// rotated five-octave fbm so no grid shows through when an effect is drawn big.
#ifndef FX_COMMON
#define FX_COMMON
float hash(float2 p) { float3 p3 = frac(float3(p.xyx) * .1031); p3 += dot(p3, p3.yzx + 33.33); return frac((p3.x + p3.y) * p3.z); }
float vnoise(float2 x)
{
    float2 i = floor(x), f = frac(x); f = f * f * f * (f * (f * 6 - 15) + 10);
    return lerp(lerp(hash(i), hash(i + float2(1, 0)), f.x), lerp(hash(i + float2(0, 1)), hash(i + float2(1, 1)), f.x), f.y);
}
float fbm(float2 x)
{
    const float2x2 m = float2x2(.8, .6, -.6, .8); float v = 0, a = .5;
    for (int k = 0; k < 5; k++) { v += a * vnoise(x); x = mul(m, x) * 2.02 + 3.1; a *= .5; }
    return v / .97;
}
// fbm through itself: billowing, curling smoke and dust
float warped(float2 x, float t) { float2 w = float2(fbm(x + float2(0, t)), fbm(x + float2(5.2, 1.3 - t))); return fbm(x + 1.6 * w); }
#endif
