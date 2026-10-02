// Shapes 50-59 of the FxProc shader (worker D: Bulwark, Spirit, Melody, Gravity and the minor bonds).
// 50 hex (par 0 plate, 1 outline, 2 honeycomb)   51 music note (par 0 quarter, 1 eighth, 2 beamed pair, 3 sharp)
// 52 sound arcs (par = half angle, 0 = .9)       53 scythe crescent (sweeps with q)
// 54 ghost wisp (heads +x)                       55 black hole (par 0 accretion, 1 lensing ring, 2 streak swirl)
// 56 shield (par 0 heater shield, 1 shard, 2 stone slab)  57 (par 0 clover, -1 leaf, 3..50 star, >=100 gear)
// 58 dashed pulse ring (par = dashes)            59 glyph (par 0 strokes, 1 rune circle, 2 letter card)
#ifndef FX_SHAPES_D
#define FX_SHAPES_D
float dd_seg(float2 p, float2 a, float2 b) { float2 pa = p - a, ba = b - a; float h = saturate(dot(pa, ba) / dot(ba, ba)); return length(pa - ba * h); }
float dd_hex(float2 p) { p = abs(p); return max(p.x * .866025 + p.y * .5, p.y) / .866025; }
float dd_ell(float2 p, float2 c, float2 rad, float ang) { p -= c; float cs = cos(ang), sn = sin(ang); p = float2(cs * p.x + sn * p.y, -sn * p.x + cs * p.y); return length(p / rad); }
float dd_stroke(float d, float w) { return smoothstep(w, w * .35, d); }

void ShapeD(float mode, float2 u, float q, float seed, float par, float r, inout float I, inout float3 hot)
{
    I = 0;
    float box = smoothstep(1, .86, max(abs(u.x), abs(u.y)));
    float m = mode - 50;
    if (m < .5)                                                                    // 50 hexagon
    {
        if (par < 1.5)
        {
            float h = dd_hex(u), edge = exp(-pow((h - .84) / (par < .5 ? .06 : .09), 2));
            float inner = par < .5 ? exp(-pow((dd_hex(u * 1.7) - .84) / .08, 2)) * .65 : exp(-pow((dd_hex(u * 1.35) - .84) / .06, 2)) * .5;
            float fill = par < .5 ? smoothstep(.9, 0, h) * (.3 + .25 * fbm(u * 3 + seed)) : 0;
            I = (edge + inner + fill) * smoothstep(.97, .88, h);
        }
        else
        {
            float2 g = u * 3.2, s = float2(1, 1.7320508);
            float2 a = g - s * floor(g / s) - s * .5, b = (g - s * .5) - s * floor((g - s * .5) / s) - s * .5;
            bool pa = dot(a, a) < dot(b, b); float2 gv = pa ? a : b; float2 id = pa ? floor(g / s) : floor((g - s * .5) / s) + .5;
            float2 p = abs(gv); float d = max(dot(p, float2(.5, .866025)), p.x);
            float lit = pow(.5 + .5 * sin(q * 9 + hash(id + seed) * 6.28), 3);
            I = (exp(-pow((.5 - d) / .045, 2)) * (.55 + .8 * lit) + smoothstep(.5, 0, d) * lit * .5) * smoothstep(1, .55, r);
        }
    }
    else if (m < 1.5)                                                              // 51 music note
    {
        float head = 0, stem = 0, flag = 0;
        if (par < 2) { head = smoothstep(1.05, .85, dd_ell(u, float2(-.28, -.52), float2(.36, .25), .5)); stem = dd_stroke(dd_seg(u, float2(.06, -.5), float2(.06, .78)), .06); }
        if (par > .5 && par < 1.5) flag = dd_stroke(min(dd_seg(u, float2(.08, .76), float2(.42, .5)), min(dd_seg(u, float2(.42, .5), float2(.5, .2)), dd_seg(u, float2(.5, .2), float2(.3, -.06)))), .075);
        if (par > 1.5 && par < 2.5)
        {
            head = max(smoothstep(1.05, .85, dd_ell(u, float2(-.55, -.5), float2(.3, .22), .5)), smoothstep(1.05, .85, dd_ell(u, float2(.4, -.66), float2(.3, .22), .5)));
            stem = max(dd_stroke(dd_seg(u, float2(-.27, -.5), float2(-.27, .62)), .055), dd_stroke(dd_seg(u, float2(.68, -.66), float2(.68, .42)), .055));
            flag = dd_stroke(dd_seg(u, float2(-.28, .62), float2(.69, .42)), .1);
        }
        if (par > 2.5) { stem = max(max(dd_stroke(dd_seg(u, float2(-.2, -.7), float2(-.2, .7)), .06), dd_stroke(dd_seg(u, float2(.2, -.7), float2(.2, .7)), .06)), max(dd_stroke(dd_seg(u, float2(-.55, -.12), float2(.55, .12)), .1), dd_stroke(dd_seg(u, float2(-.55, .3), float2(.55, .54)), .1))); }
        float body = max(max(head, stem), flag);
        I = (body + exp(-r * r * 3) * .22) * box;
    }
    else if (m < 2.5)                                                              // 52 sound arcs travelling along +x from the left edge
    {
        float2 p = u - float2(-.95, 0); float rr = length(p), a = atan2(p.y, p.x), hw = par > .01 ? par : .9;
        float wave = pow(.5 + .5 * cos((rr - q * .9) * 14), 5);
        float env = smoothstep(hw, hw * .55, abs(a)) * smoothstep(1.7, .9, rr) * smoothstep(.1, .35, rr);
        I = wave * env * box * 1.5 + exp(-pow(rr - q * 1.4, 2) * 30) * env * box * .3;
    }
    else if (m < 3.5)                                                              // 53 scythe crescent sweeping counter-clockwise
    {
        float a = atan2(u.y, u.x), hd = lerp(-2.3, 2.3, saturate(q)), L = 1.9, behind = hd - a;
        float tr = saturate(1 - behind / L), on = smoothstep(-.08, -.02, behind) * smoothstep(L, L - .1, behind) * smoothstep(-2.6, -2.2, a) * smoothstep(2.6, 2.2, a);
        float R = .93, ri = R - .5 * pow(tr, .7) - .03, tip = smoothstep(-.06, .1, behind);
        float band = smoothstep(ri - .02, ri + .03, r) * smoothstep(R + .02, R - .03, r);
        float edge = exp(-pow((r - R) / .035, 2)) * 1.1;
        I = (band * (.35 + .65 * pow(tr, 1.2)) + edge * tr) * on * tip;
    }
    else if (m < 4.5)                                                              // 54 ghost wisp heading +x
    {
        float2 hc = float2(.45, 0); float he = length((u - hc) / float2(.3, .27));
        float head = smoothstep(1, .55, he);
        float yc = .13 * sin(u.x * 8 - q * 16 + seed) * saturate(.45 - u.x);
        float w = .27 * smoothstep(-1, .45, u.x), tail = smoothstep(w, w * .25, abs(u.y - yc)) * smoothstep(-.96, -.25, u.x) * smoothstep(.55, .4, u.x) * .8;
        float eyes = smoothstep(.08, .04, length(u - float2(.52, .1))) + smoothstep(.08, .04, length(u - float2(.52, -.1)));
        float mouth = smoothstep(.1, .05, length((u - float2(.66, 0)) / float2(.7, 1)));
        I = saturate(max(head, tail) * (1 - eyes * .95) * (1 - mouth * .6) + exp(-he * he * 1.4) * .22) * box;
    }
    else if (m < 5.5)                                                              // 55 black hole
    {
        float a = atan2(u.y, u.x);
        if (par < .5)
        {
            float sp = .5 + .5 * sin(a * 2 + 11 * log(r + .05) - q * 14 + seed), disc = smoothstep(.2, .3, r) * smoothstep(.95, .45, r) * pow(.26 / max(r, .26), .9);
            float ring = exp(-pow((r - .26) / .03, 2)) * 1.8;
            I = (disc * (.2 + .9 * pow(sp, 2.2)) + ring) * smoothstep(1, .85, r);
            hot = float3(1, .92, 1);
        }
        else if (par < 1.5)
        {
            float n = .5 + .5 * vnoise(float2(a * 5 + seed, q * 3));
            float rr = r + .03 * sin(a * 6 + seed);
            I = (exp(-pow((rr - .68) / .03, 2)) * (.4 + .9 * n) + exp(-pow((rr - .5) / .02, 2)) * .5 * n + exp(-pow((rr - .84) / .02, 2)) * .3 * (1 - n)) * smoothstep(.97, .88, r);
        }
        else
        {
            float st = vnoise(float2(a * 26 + seed, r * 2.2 - q * 3)); float spin = a + r * 3.0 - q * 6;
            I = pow(saturate(vnoise(float2(spin * 7, r * 5 + seed)) * 1.5 - .45), 2) * smoothstep(.2, .35, r) * smoothstep(1, .55, r) * (.5 + st);
        }
    }
    else if (m < 6.5)                                                              // 56 shield / shard / slab
    {
        if (par < .5)
        {
            float w = u.y > -.1 ? .72 : .72 * sqrt(saturate((u.y + .92) / .82));
            float de = min(w - abs(u.x), min(.82 - u.y, (u.y + .94) * 1.2));
            float inside = smoothstep(-.015, .02, de), rim = exp(-pow((de - .06) / .05, 2)), band = exp(-pow((de - .2) / .035, 2)) * .5;
            float emblem = max(dd_stroke(dd_seg(u, float2(0, -.5), float2(0, .5)), .07), dd_stroke(dd_seg(u, float2(-.36, .12), float2(.36, .12)), .07));
            I = inside * (.28 + .12 * fbm(u * 4 + seed) + rim * 1.1 + band + emblem * .8 + exp(-pow(length(u - float2(0, .12)) / .12, 2)) * .6);
        }
        else if (par < 1.5)
        {
            float d1 = -dot(u - float2(.95, 0), normalize(float2(.5, 1.6))), d2 = -dot(u - float2(.95, 0), normalize(float2(.5, -1.6))), d3 = (u.x + .7);
            float de = min(min(d1, d2), d3 * .8); float inside = smoothstep(-.015, .02, de);
            I = inside * (.35 + exp(-pow((de - .05) / .05, 2)) * 1.1 + .2 * fbm(u * 5 + seed)) * box;
        }
        else
        {
            float2 p = abs(u) / float2(.5, .9); float de = min(1 - p.x, 1 - p.y) * .5; float inside = smoothstep(-.012, .015, de);
            float rune = dd_stroke(dd_seg(u, float2(0, -.65), float2(0, .45)), .05) + dd_stroke(dd_seg(u, float2(-.26, .15), float2(.26, .15)), .045) + dd_stroke(dd_seg(u, float2(-.18, -.35), float2(.18, -.35)), .04);
            float pulse = .6 + .4 * sin(q * 12 + seed);
            I = inside * (.33 + .1 * fbm(u * 6 + seed) + exp(-pow((de - .035) / .035, 2)) * 1.1 + rune * pulse * 1.1);
        }
    }
    else if (m < 7.5)                                                              // 57 clover / leaf / star / gear
    {
        float a = atan2(u.y, u.x);
        if (par < -.5) { float w = .46 * (1 - u.x * u.x); float d = abs(u.y); I = (smoothstep(w, w - .12, d) * (.45 + .2 * fbm(u * 4 + seed)) + dd_stroke(dd_seg(u, float2(-.8, 0), float2(.8, 0)), .03) * .6 + exp(-pow((d - w) / .04, 2)) * .5 * smoothstep(.97, .85, abs(u.x))) * smoothstep(1, .85, abs(u.x)); }
        else if (par < .5)
        {
            float d = 9;
            d = min(d, length(u - float2(.34, 0)) / .4); d = min(d, length(u + float2(.34, 0)) / .4); d = min(d, length(u - float2(0, .34)) / .4); d = min(d, length(u + float2(0, .34)) / .4);
            float seam = exp(-pow(abs(u.x) * abs(u.y) * 6, 2) * 6);
            float stem = dd_stroke(dd_seg(u, float2(.1, -.1), float2(.45, -.7)), .045) * .9;
            I = (smoothstep(1, .55, d) * (.55 - seam * .3) + exp(-pow((d - .88) / .08, 2)) * .9 + stem) * smoothstep(.97, .8, r);
        }
        else if (par < 99)
        {
            float R = lerp(.38, .95, pow(.5 + .5 * cos(par * a), 1.6));
            I = (smoothstep(R, R - .08, r) * (.35 + .25 * fbm(u * 4 + seed)) + exp(-pow((r - R) / .05, 2)) * .9 + exp(-r * r * 12) * .8) * smoothstep(1, .92, r);
        }
        else
        {
            float N = par - 100, R = .7 + .2 * smoothstep(-.2, .2, sin(N * a));
            float body = smoothstep(R, R - .06, r) * smoothstep(.2, .26, r);
            I = (body * .42 + exp(-pow((r - R) / .04, 2)) * .9 + exp(-pow((r - .3) / .04, 2)) * .7) * smoothstep(1, .93, r);
        }
    }
    else if (m < 8.5)                                                              // 58 dashed pulse ring
    {
        float a = atan2(u.y, u.x), N = par > .5 ? par : 12, R = .2 + .72 * (1 - pow(1 - saturate(q), 2));
        float dash = smoothstep(-.1, .25, sin(a * N + q * 6 + seed));
        float w = lerp(.1, .035, q);
        I = (exp(-pow((r - R) / w, 2)) * (.35 + .9 * dash) + smoothstep(R, 0, r) * .16 * (1 - q) + exp(-pow((r - R * .72) / (w * .6), 2)) * .4) * (1 - smoothstep(.65, 1, q)) * smoothstep(.98, .88, r);
    }
    else                                                                           // 59 glyph
    {
        if (par > 1.5)
        {
            float2 p = abs(u) / float2(.78, .56); float de = min(1 - p.x, 1 - p.y) * .4; float inside = smoothstep(-.012, .015, de);
            float lines = 0;
            for (int k = 0; k < 3; k++) { float y = -.28 + k * .26; lines += dd_stroke(dd_seg(u, float2(-.5, y), float2(.5 - .24 * hash(float2(seed, k)), y)), .03); }
            I = inside * (.5 + exp(-pow((de - .03) / .03, 2)) * .9 + lines * .7) * box;
        }
        else
        {
            float d = 9, ring = 0;
            for (int k = 0; k < 4; k++)
            {
                float ia = floor(hash(float2(seed, k)) * 9), ib = floor(hash(float2(seed * .3 + 1.7, k * 3.3)) * 9);
                float2 pa = float2(fmod(ia, 3) - 1, floor(ia / 3) - 1) * .42, pb = float2(fmod(ib, 3) - 1, floor(ib / 3) - 1) * .42;
                if (ia != ib) d = min(d, dd_seg(u, pa, pb));
            }
            if (par > .5) ring = exp(-pow((r - .86) / .035, 2)) * .8 + exp(-pow((r - .7) / .02, 2)) * .45;
            I = (dd_stroke(d, .075) + ring + exp(-d * d * 60) * .5) * box;
        }
    }
}
#endif
