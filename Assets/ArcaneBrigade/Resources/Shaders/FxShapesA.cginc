// Shapes 20-29 of the FxProc shader (family A: Windborne, Ember, Frost, Tide, Garden).
// 20 crescent blade (horns point +x), 21 leaf / feather (par>=1), 22 crystal shard (tip +x), 23 water drop (tip +y), 24 bubble,
// 25 spiral streaks (par: arms, sign = turn), 26 snowflake, 27 vine curl (q = growth), 28 blossom (par >= 10: bark grain), 29 root / tentacle (+y, q = growth).
// 24 with par >= 10 is a flame tongue.
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
        if (par >= 10)
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
        float y01 = u.y * .5 + .5, x0 = .22 * (1 + par) * sin(u.y * 4.2 + seed * 1.3) * y01;
        float w = (.34 * pow(max(1 - y01, 0), .8) + .03) * (1 + .28 * pow(.5 + .5 * sin(u.y * 18.85 + seed * 6.28), 8));
        float d = u.x - x0, body = smoothstep(w, w - .05, abs(d)) * smoothstep(q, q - .03, y01);
        I = body * (.55 + .45 * vnoise(float2(u.x * 6 + seed, u.y * 14))) * (.7 + .3 * saturate(-d / max(w, .03)));
        hot = float3(.75, .6, .35);
    }
}
#endif
