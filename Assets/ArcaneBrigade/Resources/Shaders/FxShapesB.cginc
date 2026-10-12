// Shapes 30-39 of the FxProc shader (worker B: Venom, Starlight, Dusk, Carnival, Court).
// 30 bubble (par 0) / balloon (par 1)   31 star (par = points, default 5)   32 teardrop (par 0) / potion flask (par 1)
// 33 wax seal rosette                    34 crown                            35 rift (lens slit, q = how open)
// 36 noisy ring (par 0..1 width), corona rays (par 1..5), crescent moon (par >= 5)
// 37 wavy ribbon (par = waves)           38 card (par 0) / unrolling parchment (par 1, q = unrolled) / heraldic banner cloth (par 2) / its gold trim (par 3)
// 39 comet streak: head at u.x = .62, tail fades towards -x
#ifndef FX_SHAPES_B
#define FX_SHAPES_B
// soft-edged box: 1 inside the half-extent h, fading over e at the border (no hard step edges)
float fxbBox(float2 p, float2 h, float e) { float2 d = abs(p) - h; return smoothstep(e, -e, max(d.x, d.y)); }
void ShapeB(float mode, float2 u, float q, float seed, float par, float r, inout float I, inout float3 hot)
{
    I = 0;
    float a = atan2(u.y, u.x);
    if (mode < 30.5)
    {
        if (par < .5)
        {
            float rim = exp(-pow((r - .8) / .055, 2)), irid = .55 + .45 * sin(a * 3 + seed * 2 + q * 5);
            float2 s1 = u - float2(-.3, .3), s2 = u - float2(.35, -.35);
            I = (rim * irid * .95 + smoothstep(.8, 0, r) * .16 + exp(-dot(s1, s1) * 70) * .9 + exp(-dot(s2, s2) * 120) * .5) * smoothstep(.98, .82, r);
        }
        else
        {
            float2 p = (u - float2(0, .12)) / float2(.6, .74); float d = length(p);
            float body = smoothstep(1, .93, d) * (.4 + .18 * (1 - d * .7));
            float2 s1 = u - float2(-.25, .4);
            float w = .1 * saturate((-.6 - u.y) / .18), knot = smoothstep(w + .03, w - .03, abs(u.x)) * smoothstep(-.83, -.77, u.y) * smoothstep(-.56, -.62, u.y);
            I = body + exp(-dot(s1, s1) * 50) * .7 + knot * .55 + exp(-pow((d - .96) / .03, 2)) * .2;
        }
    }
    else if (mode < 31.5)
    {
        float n = par < 2.5 ? 5 : par, k = abs(frac(a / (6.2831853 / n) + .5) - .5) * 2, edge = lerp(.92, .3, k);
        float body = smoothstep(edge + .05, edge - .05, r);
        I = body * (.62 + .38 * (1 - r / edge)) + exp(-r * r * 6) * .5 + exp(-r * r * 3) * .2 + exp(-pow((r - edge) / .035, 2)) * .45;
        I *= smoothstep(1, .88, r);
    }
    else if (mode < 32.5)
    {
        float2 p = u;
        if (par < .5)
        {
            float y = p.y, w = y < -.25 ? sqrt(max(0, .3025 - (y + .25) * (y + .25))) : .55 * pow(saturate(1 - (y + .25) / 1.15), 1.6);
            float inside = smoothstep(w + .04, w - .04, abs(p.x)) * smoothstep(-.84, -.76, y) * smoothstep(.92, .86, y);
            float2 s = p - float2(-.2, -.1);
            I = inside * (.6 + .4 * smoothstep(-.8, .5, y)) + exp(-dot(s, s) * 60) * .8 * inside;
        }
        else
        {
            float2 c = p - float2(0, -.3); float bl = length(c);
            float bulb = smoothstep(.55, .5, bl), neck = fxbBox(p - float2(0, .14), float2(.17, .44), .025);
            float lip = fxbBox(p - float2(0, .64), float2(.27, .06), .025), cork = fxbBox(p - float2(0, .79), float2(.19, .09), .025);
            float glass = saturate(bulb + neck + lip), level = -.22 + .07 * sin(p.x * 9 + q * 10 + seed);
            float2 s = p - float2(-.25, -.2);
            I = glass * .28 + glass * smoothstep(level + .03, level - .03, p.y) * .6 + cork * .9 + exp(-pow((bl - .55) / .03, 2)) * .6 * smoothstep(-.1, -.3, p.y) + exp(-dot(s, s) * 50) * .7 * glass;
        }
    }
    else if (mode < 33.5)
    {
        float nz = fbm(u * 3 + seed), R = .84 + .045 * cos(a * 18) + (nz - .5) * .06;
        float disc = smoothstep(R + .03, R - .03, r), ring = exp(-pow((r - .68) / .025, 2)) * .9, ring2 = exp(-pow((r - .6) / .015, 2)) * .5;
        float k = abs(frac(a / 1.0471976 + .5) - .5) * 2, edge = lerp(.5, .22, k), star = smoothstep(edge + .03, edge - .03, r);
        float beads = pow(saturate(cos(a * 24) * .5 + .5), 6) * exp(-pow((r - .76) / .035, 2)) * .7;
        I = (disc * (.42 + .2 * nz) + ring + ring2 + star * .85 + beads) * smoothstep(1, .92, r);
    }
    else if (mode < 34.5)
    {
        float x = u.x, y = u.y, tri = abs(frac(x / .45 + .5) - .5) * 2, top = -.1 + .75 * (1 - tri) * (1 - .25 * abs(x));
        float body = smoothstep(top + .04, top - .04, y) * smoothstep(-.64, -.56, y) * smoothstep(.95, .88, abs(x));
        float jew = 0;
        for (int j = -2; j <= 2; j++) { float h = -.1 + .75 * (1 - .25 * abs(j * .45)); float2 d = u - float2(j * .45, h + .06); jew += exp(-dot(d, d) * 380); }
        float2 g = u - float2(0, -.47);
        float band = smoothstep(-.61, -.55, y) * smoothstep(-.35, -.41, y) * .28;
        I = body * (.45 + .18 * (y + .6)) + band * body + jew * 1.0 + exp(-dot(g, g) * 300) * 1.1 + exp(-pow((y + .6) / .03, 2)) * .35 * smoothstep(.95, .85, abs(x));
    }
    else if (mode < 35.5)
    {
        float open = saturate(q), w = .3 * pow(saturate(1 - abs(u.x)), .7) * open, nz = fbm(float2(u.x * 5 + seed, u.y * 3)) * .1;
        float e = abs(abs(u.y) - w + nz * .5);
        I = (exp(-e * e / (.0025 + .003 * par)) * .95 + smoothstep(w, 0, abs(u.y)) * .22 * step(abs(u.x), .98)) * smoothstep(1, .75, abs(u.x));
    }
    else if (mode < 36.5)
    {
        float nz = fbm(float2(a * 3 + seed + q * 3, r * 4 - q * 2));
        if (par >= 5)
        {
            float outer = smoothstep(.86, .82, r), inner = smoothstep(.8, .76, length(u - float2(.16, .05)));
            I = (saturate(outer - inner) * (.6 + .4 * (1 - r)) + exp(-pow((r - .86) / .03, 2)) * .5) * smoothstep(.97, .88, r);
        }
        else if (par > 1)
        {
            float rays = pow(vnoise(float2(a * (6 + par * 2) + seed + q * 2, 0)), 2.5);
            I = (exp(-pow((r - .72) / .05, 2)) * (.6 + .7 * nz) + rays * exp(-(r - .72) * 3.5) * step(.72, r) * 1.4) * smoothstep(1, .85, r);
        }
        else
        {
            float wd = .05 + .1 * par;
            I = (exp(-pow((r - .78) / wd, 2)) * (.5 + .9 * nz) + exp(-abs(r - .78) * 5) * .12) * smoothstep(1, .88, r);
        }
    }
    else if (mode < 37.5)
    {
        float f = 3.14159 * (1.5 + par), c = .45 * sin(u.x * f + q * 10 + seed) * (1 - u.x * u.x * .3), w = .26 * (1 - .3 * abs(u.x)), d = abs(u.y - c);
        I = smoothstep(w, w * .65, d) * (.5 + .14 * sin(u.x * 24 + seed)) * smoothstep(1, .82, abs(u.x)) + exp(-pow((d - w * .9) / .03, 2)) * .25 * smoothstep(1, .85, abs(u.x));
    }
    else if (mode < 38.5)
    {
        if (par < .5)
        {
            float m = max(abs(u.x), abs(u.y));
            I = smoothstep(.9, .78, m) * (.5 + .1 * smoothstep(-.4, .4, sin((u.x + u.y) * 7 + seed)) + .08 * u.x);
        }
        else if (par < 1.5)
        {
            // warm aged parchment: tan paper, darker burnt rim, rows of brown ink writing, rolled edges
            float W = max(.05, q * .86), inside = smoothstep(W, W - .05, abs(u.x)) * smoothstep(.82, .75, abs(u.y));
            float grain = vnoise(u * 6 + seed) * .6 + vnoise(u * 17 + seed * 1.3) * .4, e = min(.76 - abs(u.y), W - .05 - abs(u.x));
            float ly = frac(u.y * 7 + .5), row = floor(u.y * 7 + .5);
            float words = smoothstep(.35, .6, vnoise(float2(u.x * 7 + row * 3.1 + seed, row)));
            float lines = smoothstep(.3, .42, ly) * smoothstep(.62, .5, ly) * smoothstep(W - .12, W - .2, abs(u.x)) * smoothstep(.62, .55, abs(u.y)) * words;
            float rim = smoothstep(.0, .14, e);
            float paper = (.3 + .12 * grain) * (.55 + .45 * rim) - lines * .16;
            float roll = exp(-pow((abs(u.x) - W + .02) / .05, 2)) * smoothstep(.86, .78, abs(u.y));
            I = inside * paper + roll * .5;
            I *= smoothstep(.98, .9, abs(u.x));
            hot = float3(1, .78, .46);
        }
        else
        {
            // heraldic banner hanging from a bar at the top: swallowtail foot, cloth folds that ripple more towards the foot
            float sway = (.92 - u.y) * .55, x = u.x + .07 * sin(u.y * 4.5 + q * 7 + seed) * sway;
            float foot = -.9 + .42 * saturate(1 - abs(x) / .62);
            float inside = smoothstep(.7, .65, abs(x)) * smoothstep(.93, .88, u.y) * smoothstep(foot - .03, foot + .03, u.y);
            float e = min(min(.68 - abs(x), .9 - u.y), (u.y - foot) * .8);
            if (par < 2.5)
            {
                float fold = .62 + .38 * (sin(x * 8.5 + q * 5 + seed + u.y * .8) * .5 + .5), shade = .75 + .25 * smoothstep(-.9, .9, u.y);
                I = inside * (.42 * fold * shade + .05 * vnoise(u * 12 + seed)) + inside * exp(-pow((x + .35) / .12, 2)) * .1;
            }
            else
            {
                float border = exp(-pow((e - .085) / .022, 2)), hem = exp(-pow((u.y - .78) / .03, 2)) * smoothstep(.68, .6, abs(x));
                float2 dm = float2(x, u.y - .05); float diamond = exp(-pow((abs(dm.x) + abs(dm.y) * .8 - .3) / .025, 2));
                I = inside * (border * .9 + hem * .7 + diamond * .45);
            }
        }
    }
    else
    {
        float2 d = (u - float2(.62, 0)) * float2(1, 1.3); float dh = length(d);
        float head = exp(-dh * dh * 28) * 1.2 + exp(-dh * dh * 6) * .4;
        float s = saturate((.62 - u.x) / 1.62), w = .2 * (1 - s * .9) + .012, core = exp(-u.y * u.y / (w * w));
        float nz = vnoise(float2(u.x * 16 + q * 30 + seed, u.y * 6));
        float tail = (core * (.5 + .8 * nz) + exp(-u.y * u.y / (w * w * 5)) * .3) * (1 - s) * smoothstep(.72, .56, u.x);
        I = (head + tail) * smoothstep(1, .9, abs(u.x)) * smoothstep(.95, .5, abs(u.y));
    }
}
#endif
