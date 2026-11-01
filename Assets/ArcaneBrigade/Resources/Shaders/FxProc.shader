// Procedural effect shapes drawn entirely in the fragment shader (no textures): one quad becomes a fireball, a vortex, a
// shock dome, a light shaft, a puff of smoke... Everything is driven by the quad's mode, progress, seed and a free parameter,
// so one batch of quads draws a whole effect. Additive for light, alpha for smoke and dust (set by _Src/_Dst).
Shader "ArcaneBrigade/FxProc"
{
    Properties { [HideInInspector] _Src ("Src", Float) = 5 [HideInInspector] _Dst ("Dst", Float) = 1 [HideInInspector] _Gain ("Gain", Float) = 1 [HideInInspector] _Matter ("Matter", Float) = 0 }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }
        Blend [_Src] [_Dst]
        ZWrite Off
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex : POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; float4 p : TEXCOORD1; };
            struct v2f { float4 pos : SV_POSITION; float4 color : COLOR; float2 uv : TEXCOORD0; float4 p : TEXCOORD1; };
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.color = v.color; o.uv = v.uv; o.p = v.p; return o; }

            float _Gain, _Matter;
            #include "FxCommon.cginc"
            // a beam's soft halo shifted down so it reaches exactly zero at the quad's long sides (it used to stop at ~5% there,
            // which on wide searchlights and lighthouse cones read as a hard straight edge)
            float FxBeamHalo(float y, float k) { float e = exp(-1 / k); return saturate((exp(-y * y / k) - e) / (1 - e)) * smoothstep(1, .8, abs(y)); }
            #include "FxShapesA.cginc"
            #include "FxShapesB.cginc"
            #include "FxShapesC.cginc"
            #include "FxShapesD.cginc"
            #include "FxShapesE.cginc"

            // p.x = mode, p.y = progress 0..1, p.z = seed, p.w = free parameter
            float4 frag (v2f i) : SV_Target
            {
                float2 u = i.uv; float mode = i.p.x, q = i.p.y, seed = i.p.z, par = i.p.w;
                float r = length(u), I = 0; float3 hot = float3(1, 1, 1);
                if (r > 1.45 && mode != 2) return 0;
                if (mode < .5) { I = exp(-r * r * 3.6) * smoothstep(1, .7, r); }                                  // 0 soft glow
                else if (mode < 1.5)                                                                              // 1 flame tongue (rises along +y)
                {
                    float y01 = u.y * .5 + .5, w = lerp(.95, .04, pow(saturate(y01), .85));
                    float n = fbm(float2(u.x * 2.2 + seed, u.y * 2.0 - q * (3.5 + par)) + seed * .37);
                    float d = abs(u.x) / max(w, .02);
                    I = smoothstep(1.05 + (n - .5) * .9, .15, d) * smoothstep(1, .55, y01) * smoothstep(0, .12, y01);
                }
                else if (mode < 2.5)                                                                              // 2 beam: u.x along, u.y across
                {
                    float thick = lerp(1, .35, par), ends = smoothstep(1, .55, abs(u.x));
                    float n = vnoise(float2(u.x * 7 - q * 22 + seed, u.y * 3));
                    float core = exp(-u.y * u.y / (.02 * thick + .001)), halo = FxBeamHalo(u.y, .32 * thick + .01);
                    I = saturate((core * 1.4 + halo * (.55 + .45 * n)) * ends);
                }
                else if (mode > 10.5 && mode < 11.5)                                                             // 11 link: a beam segment with no end fade (bolts)
                {
                    float thick = lerp(1, .35, par), ends = smoothstep(1, .88, abs(u.x));
                    float core = exp(-u.y * u.y / (.02 * thick + .001)), halo = FxBeamHalo(u.y, .32 * thick + .01);
                    I = saturate((core * 1.4 + halo * .6) * ends);
                }
                else if (mode > 19.5)                                                                             // 20+: shapes of the effect families (FxShapes*.cginc)
                {
                    if (mode < 29.5) ShapeA(mode, u, q, seed, par, r, I, hot);
                    else if (mode < 39.5) ShapeB(mode, u, q, seed, par, r, I, hot);
                    else if (mode < 49.5) ShapeC(mode, u, q, seed, par, r, I, hot);
                    else if (mode < 59.5) ShapeD(mode, u, q, seed, par, r, I, hot);
                    else ShapeE(mode, u, q, seed, par, r, I, hot);
                }
                else if (mode < 3.5)                                                                              // 3 vortex: spiral arms around the centre
                {
                    float a = atan2(u.y, u.x), arms = 5 + floor(par);
                    float s = sin(a * arms + r * 9 - q * 18 + seed) * .5 + .5, n = fbm(float2(a * 2 + seed, r * 5 - q * 4));
                    I = pow(s, 1.8) * (.45 + n) * smoothstep(1, .35, r) * smoothstep(0, .12, r);
                }
                else if (mode < 4.5)                                                                              // 4 shock dome: bright rim that runs outward
                {
                    float R = 1 - pow(1 - saturate(q), 2.2) ; R = R * .74 + .05;
                    float w = lerp(.045, .13, q), rim = exp(-pow((r - R) / w, 2));
                    float n = fbm(float2(atan2(u.y, u.x) * 3 + seed, r * 4));
                    I = (rim * (.55 + .5 * n) + smoothstep(R, 0, r) * (1 - q) * .22) * (1 - smoothstep(.7, 1, q)) * smoothstep(1, .8, r);
                }
                else if (mode < 5.5)                                                                              // 5 four-point sparkle
                {
                    // each arm tapers to nothing at its tip (a wide sparkle used to fill its quad and show as a square)
                    float c = exp(-abs(u.x) * 18 / (par + 1) - abs(u.y) * 2.2) * smoothstep(1, .3, abs(u.y)) * smoothstep(1, .25, abs(u.x))
                            + exp(-abs(u.y) * 18 / (par + 1) - abs(u.x) * 2.2) * smoothstep(1, .3, abs(u.x)) * smoothstep(1, .25, abs(u.y));
                    I = saturate(c * .8 + exp(-r * r * 14));
                }
                else if (mode < 6.5)                                                                              // 6 smoke puff: billowing, lit from above-left
                {
                    float d = warped(u * 1.7 + seed, q * .8);
                    // the outer half of the quad is a wide soft fade: a puff on a squashed quad no longer shows a straight top/bottom edge
                    float body = smoothstep(.95, .3, r + (d - .5) * .9) * smoothstep(1, .35, r);
                    float lit = saturate((d - warped(u * 1.7 + seed + float2(-.12, .16), q * .8)) * 4 + .5);
                    I = body * (.6 + .4 * d);
                    hot = i.color.rgb * lerp(.45, 1.35, lit);
                }
                else if (mode < 7.5)                                                                              // 7 god rays out of the centre
                {
                    float a = atan2(u.y, u.x);
                    float n = vnoise(float2(a * (4 + par) + seed + q * 2, 0)) * .6 + vnoise(float2(a * (9 + par) - q * 3, 1)) * .4;
                    I = pow(saturate(n * 1.25 - .15), 2.2) * exp(-r * 1.8) * smoothstep(1.05, .6, r);
                }
                else if (mode < 8.5)                                                                              // 8 cracked glow: bright veins on a dark floor
                {
                    float2 g = u * (2.6 + par * .6) + seed; float2 gi = floor(g), gf = frac(g); float dmin = 8, dmin2 = 8;
                    for (int y = -1; y <= 1; y++) for (int x = -1; x <= 1; x++)
                    {
                        float2 o = float2(x, y), c = o + float2(hash(gi + o), hash((gi + o).yx + 3.1)) - gf; float d = dot(c, c);
                        if (d < dmin) { dmin2 = dmin; dmin = d; } else if (d < dmin2) dmin2 = d;
                    }
                    float edge = sqrt(dmin2) - sqrt(dmin), vein = exp(-edge * 26) * 1.7 + exp(-edge * 6) * .12;
                    I = vein * smoothstep(1, .15, r) * smoothstep(0, .08, q);
                }
                else if (mode > 11.5 && mode < 12.5)                                                              // 12 dust: grainy, low, drifting
                {
                    float d = warped(u * float2(1.4, 2.4) + seed, q * .5);
                    float grain = vnoise(u * 70 + seed) * .35 + .65;
                    I = smoothstep(.95, .35, r + (d - .5) * 1.1) * smoothstep(1, .3, r) * (.5 + .5 * d) * grain;
                    // dust is lifted toward a pale haze so brown grit still reads on the red carpet and dark boards
                    hot = lerp(i.color.rgb, float3(1, .94, .82), .38) * lerp(.8, 1.35, d);
                    I = saturate(I * 1.25);
                }
                else if (mode < 9.5) { I = smoothstep(1, .92, r); }                                              // 9 hard disc (flat colour fill, soft rim)
                else { I = smoothstep(1.0, .35, r); hot = i.color.rgb; }                                          // 10 shade: a soft dark wash (alpha layer)
                // never a hard quad edge: everything fades out before the corner of its quad
                if (mode < 1.5 || (mode > 2.5 && mode < 10.5) || mode > 11.5) I *= smoothstep(1, .9, max(abs(u.x), abs(u.y)));
                I *= i.color.a * (_Matter > .5 ? 1 : _Gain);
                bool matter = (mode > 5.5 && mode < 6.5) || (mode > 11.5 && mode < 12.5);
                float3 col = matter ? hot : lerp(i.color.rgb, hot, saturate(I * I) * (_Matter > .5 ? .18 : mode > 8.5 && mode < 9.5 ? .3 : .55));
                // a touch of noise against banding in big soft glows on the dark floor
                float dither = (hash(i.pos.xy) - .5) / 128;
                return float4(col, saturate(I + dither * step(.01, I)));
            }
            ENDCG
        }
    }
}
