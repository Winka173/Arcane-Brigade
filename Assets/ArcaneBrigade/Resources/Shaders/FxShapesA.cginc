// Shapes 20-29 of the FxProc shader (family A: Windborne, Ember, Frost, Tide, Garden).
// 20 crescent blade (horns point +x), 21 leaf / feather (par>=1), 22 crystal shard (tip +x), 23 water drop (tip +y), 24 bubble,
// 25 spiral streaks (par: arms, sign = turn; par 10 tornado funnel body, 11 its light), 26 snowflake, 27 vine curl (q = growth),
// 28 blossom (par >= 10: bark grain), 29 tube segment of a chain (u.x along, capsule, q = straight fraction; par 0 wood, 1 vine,
// 2 serpent scales, 9 flat ink). 23 with par >= 10 is a faceted ice chunk.
// 24 with par >= 10 is a flame tongue; par >= 20 a storybook cel flame (20 one tongue, 21 three, 22 five).
#ifndef FX_SHAPES_A
#define FX_SHAPES_A
void ShapeA(float mode, float2 u, float q, float seed, float par, float r, inout float I, inout float3 hot)
{
    I = 0;
    if (mode < 20.5)
    {
        float ra = length(u - float2(-.12, 0)) / .88, rb = length(u - float2(.34, 0)) / .84;
        float body = smoothstep(1, .92, ra) * smoothstep(.94, 1.03, rb);
        float edge = exp(-pow((rb - 1) / .07, 2)) * smoothstep(1, .9, ra);
        float n = vnoise(float2(u.y * 9 + seed, u.x * 1.6));
        I = (body * (.5 + .4 * n) + edge * .9) * smoothstep(1.02, .88, r);
        hot = float3(.9, 1, .95);
    }
    else if (mode < 21.5)
    {
        float x01 = u.x * .5 + .5, yy = u.y + .16 * u.x * u.x, s = sin(3.14159 * saturate(x01));
        float w = par < .5 ? .5 * pow(s, .85) : .2 * pow(s, .45) + .02;
        float inside = smoothstep(w, w - .07, abs(yy)) * step(0, x01) * step(x01, 1);
        float rib = exp(-yy * yy / .0014);
        float vein = par < .5 ? pow(.5 + .5 * sin((abs(yy) * 15 - u.x * 11)), 3) * .35 : (.5 + .5 * sin((u.x - abs(yy) * .9) * 46)) * .4;
        I = inside * (.55 + vein) + rib * inside * .6;
        hot = float3(.9, 1, .75);
    }
    else if (mode < 22.5)
    {
        float x = u.x, w = .46 * min(saturate((x + .75) / .9), saturate((1 - x) / .85));
        float inside = smoothstep(w, w - .05, abs(u.y)) * step(-.75, x) * step(x, 1);
        float side = u.y > 0 ? 1 : .55, ridge = exp(-u.y * u.y / .004), line2 = exp(-pow((abs(u.y) - w * .5) / .025, 2)) * .5;
        float rim = smoothstep(w - .1, w, abs(u.y)) * .4;
        I = inside * (.42 * side + .25 * (x * .5 + .5) + ridge * .5 + line2 + rim);
        hot = float3(.95, 1, 1);
    }
    else if (mode < 23.5 && par >= 10)
    {
        // faceted ice chunk: an irregular polygon (radius jittered per corner), flat facets lit from the top-left, a pale lit rim
        float a = atan2(u.y, u.x), n = 6, f = (a + 3.14159) / 6.28318 * n, k0 = floor(f), fr = frac(f);
        float r0 = .58 + .4 * hash(float2(k0, seed)), r1 = .58 + .4 * hash(float2(fmod(k0 + 1, n), seed));
        float R = lerp(r0, r1, fr), inside = smoothstep(R, R - .05, r);
        float ac = (k0 + .5) / n * 6.28318 - 3.14159; float2 fn = float2(cos(ac), sin(ac));
        float facet = .5 + .4 * dot(fn, float2(-.7071, .7071)) + .1 * hash(float2(k0, seed + 3));
        float top = smoothstep(R * .48, R * .43, r);
        float shade = lerp(facet, .95, top) + exp(-pow((r - R * .45) / .03, 2)) * .25 * step(0, dot(normalize(u + 1e-4), float2(-.7071, .7071)));
        I = inside * saturate(shade) * (_Matter > .5 ? 1 : .18);
        hot = float3(.85, .97, 1);
    }
    else if (mode < 23.5)
    {
        float body, d = length(u - float2(0, -.15)) / .6;
        if (u.y < -.15) body = smoothstep(1, .93, d);
        else { float h = saturate((u.y + .15) / 1.1), w = .6 * pow(1 - h, 1.4); body = smoothstep(w, w - .05, abs(u.x)) * step(u.y, .95); }
        float2 dh = u - float2(-.22, -.3);
        I = body * (.5 + .2 * (u.y * .5 + .5) + .3 * smoothstep(.6, .95, d)) + exp(-dot(dh, dh) / .012) * .8;
        hot = float3(.8, .95, 1);
    }
    else if (mode < 24.5)
    {
        if (par >= 20)
        {
            // storybook flame (par 20 + v; v 0: one tongue, 1: three, 2: five): a round bulb with pointed licking tongues, a crisp
            // flat cel silhouette along +y. Layered by the caller (dark ink, red, orange, yellow). q = flicker time.
            float v = par - 20, ins = smoothstep(1, .93, length((u - float2(0, -.4)) * float2(1, 1.08)) / .46);
            float n = v < .5 ? 1 : (v < 1.5 ? 3 : 5);
            for (int k = 0; k < 5; k++)
            {
                if (k >= n) break;
                float side = k == 0 ? 0 : (k % 2 == 1 ? -1 : 1) * (k < 3 ? 1 : 1.85);
                float top = k == 0 ? .84 : (k < 3 ? .4 : .08) + .1 * sin(seed * 3.7 + k * 2.1 + q * 1.3);
                float y01 = (u.y + .5) / (top + .5);
                float x0 = side * .28 + side * (abs(side) > 1.5 ? .18 : .3) * y01 * y01 + (.07 + .03 * abs(side)) * sin(y01 * 5.2 - q * 10 + seed + k * 1.9) * y01;
                float w = (k == 0 ? .42 : .3) * pow(saturate(1 - y01), 1.15);
                ins = max(ins, smoothstep(w, w - .035, abs(u.x - x0)) * step(0, y01) * step(y01, 1));
            }
            // solid on the matter layer; only a faint sheen on the additive light layer so the cel colours never burn to white
            I = ins * (.92 + .08 * smoothstep(.5, -.6, u.y)) * (_Matter > .5 ? 1 : .2);
            hot = float3(1, .9, .55);
        }
        else if (par >= 10)
        {
            float y01 = u.y * .5 + .5, x0 = (.16 + .05 * (par - 10)) * sin(u.y * 3.2 - q * 9 + seed) * smoothstep(-.3, 1, u.y);
            float w = .62 * pow(max(1 - y01, 0), .85) * pow(saturate(y01 * 6 + .3), .5);
            w *= .8 + .45 * fbm(float2(u.x * 2.2 + seed, u.y * 2.6 - q * 5));
            float d = abs(u.x - x0) / max(w, .001), body = smoothstep(1, .55, d);
            I = body * (.5 + .6 * (1 - d) * (1 - d)) * (1 - .35 * y01) * smoothstep(1, .9, y01 + .04) * smoothstep(0, .08, y01);
            hot = float3(1, .95, .7);
        }
        else
        {
        float2 dh = u - float2(-.3, .3);
        I = (exp(-pow((r - .78) / .07, 2)) * .85 + smoothstep(.8, 0, r) * .12 + exp(-dot(dh, dh) / .02) * .7) * smoothstep(1, .86, r);
        hot = float3(1, 1, 1);
        }
    }
    else if (mode < 25.5 && par >= 10)
    {
        // tornado funnel along +y: narrow ploughing foot, wide leaning top, snaking; par 10 its dusty body (solid, alpha layer),
        // par 11 its light: a bright rim and streaks of wind wrapping round it (q = time)
        float y01 = saturate(u.y * .5 + .5);
        float x0 = .22 * y01 * y01 - .08 + .07 * sin(u.y * 4.5 - q * 2.6 + seed);
        float w = lerp(.11, .86, pow(y01, 1.35)) * (1 + .06 * sin(u.y * 13 - q * 9));
        float d = (u.x - x0) / w, ad = abs(d), en = .14 * (vnoise(float2(u.y * 7 - q * 3, seed)) - .5);
        float inside = smoothstep(1 + en, .8 + en, ad) * smoothstep(1, .88, y01) * smoothstep(0, .05, y01);
        float phi = asin(clamp(d, -1, 1));
        float sw = vnoise(float2(phi * 2.5 + q * 8 + y01 * 7, y01 * 16 + seed)), sw2 = vnoise(float2(phi * 4 - q * 5 + y01 * 3, y01 * 30 + seed * 1.7));
        float bands = smoothstep(.4, .85, sw) * .7 + smoothstep(.5, .9, sw2) * .5;
        if (par < 10.5)
        {
            I = inside * (.78 + .22 * sw2 - .25 * (1 - ad) * (1 - y01 * .5)) * (_Matter > .5 ? 1 : .06);
            hot = float3(.8, .75, .6);
        }
        else
        {
            float rim = smoothstep(.5, .93, ad) * (d < 0 ? 1 : .55);
            I = inside * (rim * (.55 + .45 * sw) + bands * (.25 + .5 * ad)) * (_Matter > .5 ? 0 : 1);
            hot = float3(1, 1, .95);
        }
    }
    else if (mode < 25.5)
    {
        float arms = 3 + floor(abs(par)), dir = par < 0 ? -1 : 1, a = atan2(u.y, u.x);
        float s = frac(a / 6.2832 * arms + dir * (log(r + .1) * 1.6 - q) + seed * .1);
        float band = smoothstep(0, .16, s) * smoothstep(.62, .3, s), n = vnoise(float2(a * 3 + seed, r * 6));
        I = band * (.45 + .75 * n) * smoothstep(1, .7, r) * smoothstep(0, .08, r);
        hot = float3(1, 1, 1);
    }
    else if (mode < 26.5)
    {
        float a = atan2(u.y, u.x), m = fmod(a + 12.566, 1.0472), b = min(m, 1.0472 - m);
        float perp = r * sin(b), along = r * cos(b);
        float mn = exp(-perp * perp / .0012) * smoothstep(.97, .88, along);
        float d1 = perp - (along - .38), d2 = perp - (along - .6);
        float br1 = exp(-d1 * d1 / .0012) * smoothstep(.36, .4, along) * smoothstep(.7, .64, along), br2 = exp(-d2 * d2 / .0012) * smoothstep(.58, .62, along) * smoothstep(.9, .84, along);

        I = (mn + (br1 + br2) * .8 + smoothstep(.17, .1, r) * .8 + exp(-r * r * 30) * .6) * smoothstep(1, .85, r);
        hot = float3(1, 1, 1);
    }
    else if (mode < 27.5)
    {
        float rr = r / .95, dirn = par < 0 ? -1 : 1, a = atan2(u.y, u.x) * dirn;
        float diff = a - rr * 2.4 * 6.2832; diff = diff - 6.2832 * floor(diff / 6.2832 + .5);
        float dist = abs(diff) * r, w = .075 * (1 - rr * .65) + .012;
        I = exp(-dist * dist / (w * w)) * smoothstep(q, q - .05, rr) * (.7 + .5 * vnoise(float2(r * 14, seed))) * smoothstep(1, .9, r);
        hot = float3(.8, 1, .6);
    }
    else if (mode < 28.5 && par >= 10)
    {
        // bark grain (par >= 10): long wavy streaks along +y with knots, laid faintly over painted wood
        float x = u.x * 3.2 + .35 * sin(u.y * 2.3 + seed) + .5 * fbm(float2(u.x * 1.5 + seed, u.y * .8));
        float g = abs(frac(x) - .5), ln = exp(-g * g / .006);
        float2 kd = (u - float2(.3 * sin(seed * 3.1), .4 * cos(seed * 1.7))) * float2(3, 1.4); float knot = exp(-pow(length(kd) - .25, 2) / .004);
        I = (ln * (.45 + .55 * vnoise(float2(x * 3, u.y * 4 + seed))) + knot * .8) * smoothstep(1, .65, abs(u.x)) * smoothstep(1, .75, abs(u.y));
        hot = float3(1, .82, .55);
    }
    else if (mode < 28.5)
    {
        float a = atan2(u.y, u.x), R = .35 + .6 * pow(.5 + .5 * cos(5 * a), .6);
        float inside = smoothstep(R, R - .06, r);
        I = inside * (.5 + .5 * smoothstep(.15, .7, r / R)) * (.8 + .2 * cos(5 * a)) + exp(-r * r * 40) * .8;
        hot = float3(1, .95, .65);
    }
    else
    {
        // tube segment: a capsule along u.x whose straight middle is q of the half-length (round caps overlap the neighbours,
        // so a chain of them reads as one smooth curving body). Lit along +y (the caller turns +y to the top-left), darker
        // towards -y so the ink copy drawn under it shows through. The light layer only gets a faint sheen.
        float k = saturate(q), cap = length(float2(max(abs(u.x) - k, 0) / max(1 - k, .05), u.y));
        float body = smoothstep(1, .9, cap), lit = smoothstep(-1, .75, u.y), m = par;
        float shade = 1; hot = float3(.6, .6, .6);
        if (m < .5)
        {
            float gx = u.y * 3.4 + .55 * fbm(float2(u.x * .45 + seed, u.y * 1.5));
            float g = abs(frac(gx) - .5), grain = exp(-g * g / .01) * (.55 + .45 * vnoise(float2(u.x * 2.5 + seed, u.y * 4)));
            shade = lerp(.52, 1, lit) - grain * .3;
            hot = float3(1, .78, .5);
        }
        else if (m < 1.5)
        {
            shade = lerp(.55, 1, lit) - exp(-pow((u.y + .2) / .1, 2)) * .12;
            hot = float3(.8, 1, .55);
        }
        else if (m < 2.5)
        {
            float2 sc = float2(u.x * 2.6 + seed, u.y * 1.9 + .5 * frac(floor(u.x * 2.6 + seed) * .5) * 2);
            float2 c = frac(sc) - float2(.5, 0); float scale = smoothstep(.06, 0, abs(length(c * float2(1, 1.3)) - .55));
            float belly = smoothstep(-.2, -.6, u.y);
            shade = lerp(.5, 1, lit) - scale * .28 * (1 - belly) + belly * .25;
            hot = float3(.75, 1, 1);
        }
        // the light layer gets no light from the flat ink/highlight passes and only a faint sheen from the body
        I = body * saturate(shade) * (_Matter > .5 ? 1 : (m > 8.5 ? 0 : .07));
    }
}
#endif
