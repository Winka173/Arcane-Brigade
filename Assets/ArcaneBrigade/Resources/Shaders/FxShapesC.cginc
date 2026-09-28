// Shapes 40-49 (worker C): 40 gear, 41 page / book cover, 42 rock / stone spike, 43 ink rune, 44 cooking pot / lid, 45 pie,
// 46 clock face, 47 glass lantern prism, 48 spring coil, 49 loaf / apple.
#ifndef FX_SHAPES_C
#define FX_SHAPES_C
float segC(float2 p, float2 a, float2 b) { float2 pa = p - a, ba = b - a; float h = saturate(dot(pa, ba) / max(dot(ba, ba), .0001)); return length(pa - ba * h); }
void ShapeC(float mode, float2 u, float q, float seed, float par, float r, inout float I, inout float3 hot)
{
    I = 0;
    float a = atan2(u.y, u.x);
    if (mode < 40.5)                                                  // 40 brass gear, par = teeth
    {
        float teeth = par < 3 ? 8 : par;
        float R = .68 + .2 * smoothstep(-.3, .3, sin(a * teeth));
        float body = smoothstep(R + .035, R - .035, r) * smoothstep(.2, .26, r);
        float groove = exp(-pow((r - .5) / .035, 2));
        float win = smoothstep(.05, .2, sin(a * 5 + seed)) * step(.31, r) * step(r, .46);
        float rim = exp(-pow((r - R) / .05, 2)) + exp(-pow((r - .22) / .03, 2)) * .7;
        float shade = .5 + .22 * (u.y * .5 + .5) + .12 * fbm(u * 3 + seed);
        I = saturate(body * shade * (1 - win * .7) + rim * .75 + groove * .18 * body);
        hot = float3(1, .86, .5);
    }
    else if (mode < 41.5)                                             // 41 page (par 0) or leather cover (par 1)
    {
        float2 d2 = abs(u) - float2(.8, .9); float d = max(d2.x, d2.y);
        float inside = smoothstep(.04, -.04, d);
        float edge = exp(-pow(d / .035, 2));
        if (par < .5)
        {
            float row = floor(u.y * 6 + 3), len = .35 + .55 * hash(float2(row, seed));
            float band = step(.28, frac(u.y * 6 + 3)) * step(frac(u.y * 6 + 3), .62);
            float text = band * step(abs(u.x + .08), len * .7) * step(abs(u.y + .03), .75) * (hash(float2(floor(u.x * 18), row + seed)) > .22 ? 1 : .4);
            float fold = step(1.3, u.x + u.y);
            float curl = .78 + .22 * u.x * (1 - .5 * fold);
            I = saturate(inside * curl * (1 - text * .3) * (1 - fold * .5) + edge * .55 + fold * edge * .4);
            hot = float3(1, .93, .72);
        }
        else
        {
            float inner = exp(-pow((d + .13) / .03, 2));
            float emblem = exp(-pow((r - .28) / .035, 2)) + exp(-pow((abs(u.x) + abs(u.y) - .22) / .035, 2));
            float grain = .5 + .2 * fbm(u * 5 + seed);
            I = saturate(inside * grain * .9 + edge * .8 + inner * .9 + emblem * .8 * inside);
            hot = float3(1, .8, .45);
        }
    }
    else if (mode < 42.5)                                             // 42 rock chunk (par 0) or stalagmite spike (par 1)
    {
        if (par < .5)
        {
            float n = 7; float ang = (a / 6.2831853 + .5) * n; float i0 = floor(ang), f = frac(ang);
            float r0 = .6 + .32 * hash(float2(fmod(i0, n) + seed, 1.7)), r1 = .6 + .32 * hash(float2(fmod(i0 + 1, n) + seed, 1.7));
            float R = lerp(r0, r1, f);
            float fill = smoothstep(R + .035, R - .035, r);
            float lit = .2 + .3 * hash(float2(i0, seed + 9)) + .14 * u.y;
            float edgeLine = smoothstep(.06, 0, min(f, 1 - f)) * smoothstep(.15, .55, r);
            float crack = exp(-abs(fbm(u * 3.2 + seed) - .5) * 26);
            float rim = exp(-pow((r - R) / .06, 2)) * .85;
            I = saturate(fill * (lit + .12 * fbm(u * 6 + seed)) * (1 - crack * .35) + edgeLine * fill * .45 + rim);
            hot = float3(.95, .76, .5);
        }
        else
        {
            float h = saturate((u.y + 1) * .5);
            float jag = .08 * (hash(float2(floor(u.y * 7), seed)) - .5);
            float w = (1 - pow(h, .9)) * .78 + jag * (1 - h);
            float ax = abs(u.x - jag * .6);
            float inside = smoothstep(.05, -.05, ax - w) * smoothstep(-1.02, -.88, u.y);
            float side = u.x - jag * .6 < 0 ? .5 : .22;
            float strata = .12 * sin(u.y * 16 + u.x * 3 + seed);
            float rim = exp(-pow((ax - w) / .045, 2)) * smoothstep(-1, -.8, u.y);
            I = saturate(inside * (side + strata) + rim * .9);
            hot = float3(.95, .74, .48);
        }
    }
    else if (mode < 43.5)                                             // 43 ink rune, q = how much is drawn
    {
        float a01 = a / 6.2831853 + .5;
        float ring = exp(-pow((r - .82) / .04, 2)) * step(a01, q * 1.25);
        float ring2 = exp(-pow((r - .6) / .03, 2)) * step(frac(a01 * 3 + seed), .72) * step(.15, q);
        float tick = pow(max(0, sin(a * 14)), 24) * exp(-pow((r - .9) / .05, 2)) * step(.5, q);
        float glyph = 0;
        for (int k = 0; k < 5; k++)
        {
            float2 p0 = (float2(hash(float2(k, seed)), hash(float2(k + 7, seed))) * 2 - 1) * .5;
            float2 p1 = (float2(hash(float2(k + 3, seed + 1)), hash(float2(k + 11, seed + 2))) * 2 - 1) * .5;
            float dd = segC(u, p0, lerp(p0, p1, saturate(q * 5 - k * .55)));
            glyph = max(glyph, exp(-pow(dd / .05, 2)) * step(k * .11, q));
            glyph = max(glyph, exp(-pow(length(u - p0) / .09, 2)) * step(k * .11, q));
        }
        I = saturate(ring + ring2 * .7 + tick * .7 + glyph + .1 * smoothstep(.9, .5, r) * q);
        hot = float3(1, .88, .55);
    }
    else if (mode < 44.5)                                             // 44 cooking pot (par 0) or its lid (par 2)
    {
        if (par < 1)
        {
            float2 p = float2(u.x / .74, (u.y + .18) / .56);
            float body = smoothstep(1.06, .94, pow(abs(p.x), 3.0) + pow(abs(p.y), 2.2));
            float2 rp = float2(u.x / .8, (u.y - .38) / .2);
            float rimEll = smoothstep(1.06, .94, dot(rp, rp));
            float2 sp = float2(u.x / .68, (u.y - .38) / .13);
            float surface = smoothstep(1.06, .9, dot(sp, sp));
            float rimLine = exp(-pow((length(rp) - 1) / .12, 2));
            float2 e1 = float2(abs(u.x) - .88, u.y - .2);
            float ear = exp(-pow((length(e1) - .14) / .045, 2));
            float shine = exp(-pow((u.x + .42) / .07, 2)) * body * step(u.y, .25) * step(-.4, u.y);
            I = saturate(body * (.5 + .12 * u.x) * (1 - surface * .1) + rimEll * .35 + rimLine * .8 + ear * .9 + shine * .5 + surface * .85);
            hot = float3(1, .78, .4);
        }
        else
        {
            float2 p = float2(u.x / .8, (u.y + .35) / .5);
            float dome = step(0, p.y) * smoothstep(1.06, .94, dot(p, p));
            float knob = smoothstep(.17, .13, length(u - float2(0, .22)));
            float rimL = exp(-pow((u.y + .35) / .05, 2)) * step(abs(u.x), .8);
            float shine = exp(-pow((u.x + .35) / .09, 2)) * dome * .5;
            I = saturate(dome * .55 + knob * .9 + rimL * .6 + shine);
            hot = float3(1, .85, .5);
        }
    }
    else if (mode < 45.5)                                             // 45 pie seen from above
    {
        float R = .8 + .045 * sin(a * 24);
        float disc = smoothstep(R + .03, R - .03, r);
        float bar = max(smoothstep(.13, .06, abs(frac((u.x + u.y) * 2.6) - .5)), smoothstep(.13, .06, abs(frac((u.x - u.y) * 2.6) - .5)));
        float lattice = r < .62 ? bar : 1;
        float crust = smoothstep(.62, .68, r);
        float fillGap = (1 - lattice) * smoothstep(.62, .5, r);
        float edge = exp(-pow((r - R) / .05, 2));
        float gloss = exp(-pow(length(u - float2(-.3, .3)) / .2, 2)) * .35;
        I = saturate(disc * (.55 + .3 * lattice) + crust * disc * .2 - fillGap * .15 + edge * .5 + gloss * disc);
        hot = float3(1, .8, .45);
    }
    else if (mode < 46.5)                                             // 46 clock face, par = minute-hand angle in radians
    {
        float ring = exp(-pow((r - .88) / .045, 2));
        float tk = 0;
        for (int k = 0; k < 12; k++)
        {
            float ka = k * .5235988;
            float2 d = float2(cos(ka), sin(ka));
            float len = (k % 3 == 0) ? .26 : .13;
            tk = max(tk, exp(-pow(segC(u, d * (.8 - len), d * .8) / .035, 2)));
        }
        float2 mh = float2(cos(1.5707963 - par), sin(1.5707963 - par)), hh = float2(cos(1.5707963 - par / 12), sin(1.5707963 - par / 12));
        float hands = max(exp(-pow(segC(u, float2(0, 0), mh * .66) / .04, 2)), exp(-pow(segC(u, float2(0, 0), hh * .44) / .06, 2)));
        float boss = exp(-pow(r / .09, 2));
        I = saturate(ring + tk * .9 + hands + boss + .1 * smoothstep(.9, .3, r));
        hot = float3(.9, 1, 1);
    }
    else if (mode < 47.5)                                             // 47 honey glass prism (lantern pane / shard)
    {
        float hexR = .82 / cos(fmod(abs(a) + 3.14159265, 1.0471976) - .5235988);
        float fill = smoothstep(hexR + .03, hexR - .03, r);
        float facet = .55 + .4 * sin(floor((a + 3.14159265) / 1.0471976) * 2.3 + seed);
        float edge = exp(-pow((r - hexR) / .045, 2));
        float spokes = exp(-pow(abs(sin(a * 3)) * r / .04, 2)) * .35;
        float core = exp(-r * r * 7);
        I = saturate(fill * facet * .5 + edge * .9 + spokes * fill + core * .9);
        hot = float3(1, .9, .6);
    }
    else if (mode < 48.5)                                             // 48 spring coil along u.x, par = turns
    {
        float turns = par < 1 ? 5 : par;
        float ph = u.x * turns * 3.14159265;
        float d = abs(u.y - sin(ph) * .5);
        float wire = exp(-pow(d / .12, 2));
        float back = exp(-pow(abs(u.y - cos(ph) * .5) / .1, 2)) * .45;
        I = saturate((wire + back) * smoothstep(1, .85, abs(u.x)));
        hot = float3(1, .9, .6);
    }
    else                                                              // 49 loaf of bread (par 0) or apple (par 1)
    {
        if (par < .5)
        {
            float2 p = float2(u.x / .85, u.y / .5);
            float body = smoothstep(1.06, .94, dot(p, p));
            float score = 0;
            for (int k = -1; k <= 1; k++) score = max(score, exp(-pow(segC(u, float2(k * .36 - .1, -.25), float2(k * .36 + .08, .28)) / .035, 2)));
            float gloss = exp(-pow(length(u - float2(-.3, .22)) / .22, 2)) * .4;
            float rim = exp(-pow((length(p) - 1) / .09, 2));
            I = saturate(body * (.55 + .15 * fbm(u * 5 + seed)) + score * body * .5 + gloss * body + rim * .5);
            hot = float3(1, .82, .5);
        }
        else
        {
            float body = smoothstep(.72, .66, length(u - float2(0, -.08)));
            float dent = smoothstep(.2, 0, length(u - float2(0, .6)));
            float stem = exp(-pow(segC(u, float2(0, .55), float2(.08, .85)) / .035, 2));
            float leaf = exp(-pow(length((u - float2(.22, .78)) * float2(1, 2.2)) / .15, 2));
            float gloss = exp(-pow(length(u - float2(-.28, .12)) / .17, 2)) * .5;
            I = saturate(body * (.6 - dent * .3 + .15 * fbm(u * 4 + seed)) + stem + leaf * .7 + gloss * body);
            hot = float3(1, .8, .6);
        }
    }
}
#endif
