// Shapes 40-49 (worker C): 40 gear, 41 page / book, 42 rock / spike / foot / monolith / boulder, 43 ink rune / stamp mark / banner, 44 cooking pot / lid,
// 45 pie crust / filling, 46 clock face, 47 lantern glass / lantern / light cone, 48 spring coil / pendulum rod / bob, 49 loaf / apple.
// Objects are SOLID (owner 29/10/2026: pot, rocks, book, gears read as glowing light): on the matter layer the silhouette is opaque,
// shaded from a light at the top-left and outlined in dark ink; on the additive light layer only a small accent glows (a rim catch-light,
// a lit pane, hot broth). Shapes that turn pass their rotation in q (degrees / 360) so the light stays at the top-left of the screen.
#ifndef FX_SHAPES_C
#define FX_SHAPES_C
float segC(float2 p, float2 a, float2 b) { float2 pa = p - a, ba = b - a; float h = saturate(dot(pa, ba) / max(dot(ba, ba), .0001)); return length(pa - ba * h); }
// the screen's top-left light seen from inside a quad turned by q turns
float2 LightC(float q) { float a = q * 6.2831853, c = cos(a), s = sin(a); float2 L = float2(-.6, .8); return float2(L.x * c + L.y * s, -L.x * s + L.y * c); }
// matter layer: opaque fill, colour = .82 tint + .18 hot (FxProc), so hot is pushed far: tone 0 deep shadow .. 1 bright, ink = black line.
// light layer: only the accent glows.
void MatC(float fill, float tone, float ink, float accent, float3 accentCol, inout float I, inout float3 hot)
{
    if (_Matter > .5) { float t = lerp(-1.9, 2.7, saturate(tone)); I = saturate(fill); hot = lerp(float3(t, t, t), float3(-5, -5, -5), saturate(ink)); }
    else { I = saturate(accent); hot = accentCol; }
}
// an ink line just inside a silhouette edge, sd = distance inside (positive inside)
float InkC(float sd, float w) { return smoothstep(w, w * .35, sd) * step(-.02, sd); }

void ShapeC(float mode, float2 u, float q, float seed, float par, float r, inout float I, inout float3 hot)
{
    I = 0;
    float a = atan2(u.y, u.x);
    float2 L = LightC(q);
    if (mode < 40.5)                                                  // 40 brass gear, par = teeth, q = turn
    {
        float teeth = par < 3 ? 8 : par;
        float tw = smoothstep(-.35, .35, sin(a * teeth));
        float R = .7 + .2 * tw;
        float dOut = R - r, dHub = r - .17;
        float win = smoothstep(.05, .2, sin(a * 5 + seed)) * smoothstep(.31, .34, r) * smoothstep(.5, .47, r);
        float fill = smoothstep(-.015, .015, dOut) * smoothstep(-.015, .015, dHub) * (1 - win);
        // a bevelled rim and hub: the faces tilt toward the light on the top-left side
        float2 nrm = normalize(u + .0001);
        float bevel = smoothstep(.12, 0, dOut) - smoothstep(.1, 0, abs(r - .5)) * .5 + smoothstep(.1, 0, dHub) * -.6;
        float tone = .55 + .38 * bevel * dot(nrm, L) + .1 * fbm(u * 4 + seed) - .12 * smoothstep(.2, .7, r) * (1 - dot(nrm, L)) * .5;
        float winEdge = smoothstep(.05, .2, sin(a * 5 + seed)) * (exp(-pow((r - .325) / .02, 2)) + exp(-pow((r - .485) / .02, 2)));
        float ink = max(max(InkC(dOut, .05), InkC(dHub, .045)), saturate(winEdge) * .8) + exp(-pow((r - .5) / .015, 2)) * .5;
        float rim = exp(-pow((dOut - .035) / .025, 2)) * saturate(dot(nrm, L) * 1.3) + exp(-pow((dHub - .03) / .02, 2)) * saturate(-dot(nrm, L)) * .6;
        MatC(fill, tone, ink, rim * fill * .9 + fill * .06, float3(1, .92, .66), I, hot);
    }
    else if (mode < 41.5)                                             // 41 paper page (0), leather cover (1), closed book (2), half of an open book (3)
    {
        if (par < .5)
        {
            float2 d2 = abs(u) - float2(.8, .9); float sd = -max(d2.x, d2.y);
            float fill = smoothstep(-.02, .02, sd);
            float row = floor(u.y * 6 + 3), len = .35 + .55 * hash(float2(row, seed));
            float band = smoothstep(.22, .3, frac(u.y * 6 + 3)) * smoothstep(.62, .54, frac(u.y * 6 + 3));
            float text = band * step(abs(u.x + .08), len * .7) * step(abs(u.y + .03), .72) * (hash(float2(floor(u.x * 18), row + seed)) > .22 ? 1 : .3);
            float curl = smoothstep(.5, .95, u.x) * .25;
            float tone = .7 + .12 * (u.y - u.x) * .5 - curl + .06 * fbm(u * 5 + seed);
            MatC(fill, tone, max(InkC(sd, .05), text * .55), fill * .1, float3(1, .95, .8), I, hot);
        }
        else if (par < 1.5)
        {
            float2 d2 = abs(u) - float2(.82, .92); float sd = -max(d2.x, d2.y);
            float fill = smoothstep(-.02, .02, sd);
            float tool = exp(-pow((sd - .12) / .022, 2));
            float emblem = exp(-pow((r - .3) / .03, 2)) + exp(-pow((abs(u.x) + abs(u.y) - .2) / .03, 2));
            float tone = .45 + .2 * dot(normalize(u + .001), L) * smoothstep(.0, .5, r) + .14 * fbm(u * 6 + seed) + .3 * smoothstep(.1, 0, sd) * saturate(dot(sign(u) * step(.7, abs(u) / float2(.82, .92)), L));
            MatC(fill, tone, max(InkC(sd, .05), (tool + emblem) * .35), (tool + emblem) * fill * .75, float3(1, .82, .4), I, hot);
        }
        else if (par < 2.5)
        {
            // a closed book from above-front: the cover board, the cream page block below it (thickness), the rounded spine on the left
            float2 c0 = abs(u - float2(.04, .14)) - float2(.8, .72); float sdC = -max(c0.x, c0.y);
            float2 c1 = abs(u - float2(.06, -.66)) - float2(.76, .14); float sdP = -max(c1.x, c1.y);
            float2 c2 = abs(u - float2(-.82, -.08)) - float2(.12, .86); float sdS = -max(c2.x, c2.y);
            float cover = smoothstep(-.02, .02, sdC), pages = smoothstep(-.02, .02, sdP) * (1 - cover), spine = smoothstep(-.02, .02, sdS) * (1 - cover);
            float fill = max(cover, max(pages, spine));
            float lines = .5 + .5 * sin(u.y * 140);
            float tool = exp(-pow((sdC - .1) / .02, 2)) * cover;
            float boss = exp(-pow((length(u - float2(.06, .16)) - .24) / .03, 2)) * cover;
            float bands = (exp(-pow((u.y - .45) / .03, 2)) + exp(-pow((u.y + .3) / .03, 2))) * spine;
            float tone = cover * (.5 + .18 * (u.y - u.x) * .5 + .12 * fbm(u * 6 + seed)) + pages * (.86 - .2 * lines) + spine * (.32 + .3 * smoothstep(-.94, -.8, u.x));
            float ink = max(max(InkC(sdC, .05), InkC(sdP, .04) * (1 - cover)), InkC(sdS, .04) * (1 - cover)) + bands * .6 + (tool + boss) * .3;
            MatC(fill, tone, ink, (tool + boss) * .8 + bands * .5 + pages * .05, float3(1, .82, .4), I, hot);
            // the page block is cream whatever the leather tint (matter colour = .67 tint + .18 hot)
            if (_Matter > .5) hot = lerp(hot, float3(3, 3.9, 3.6) * (.8 + .2 * lines) * (1 - saturate(ink)) + float3(-5, -5, -5) * saturate(ink), pages);
        }
        else
        {
            // half of an open book, spine at u.x = -1: a page that bows up from the gutter, the page block's edge below, the cover's lip
            float bow = .1 * sin(saturate((u.x + 1) * .5) * 3.14159265);
            float top = .78 + bow, bot = -.66 + bow * .6;
            float sdPage = min(min(top - u.y, u.y - bot), min(u.x + .98, .9 - u.x));
            float sdBlock = min(min(bot - u.y, u.y - (bot - .14)), min(u.x + .98, .93 - u.x));
            float sdCov = min(min(top + .06 - u.y, u.y - (bot - .2)), min(u.x + 1.0, .98 - u.x));
            float page = smoothstep(-.02, .02, sdPage), block = smoothstep(-.02, .02, sdBlock) * (1 - page), cov = smoothstep(-.02, .02, sdCov) * (1 - page) * (1 - block);
            float fill = max(page, max(block, cov));
            float row = floor((u.y - bot) / (top - bot) * 9), fy = frac((u.y - bot) / (top - bot) * 9);
            float text = smoothstep(.3, .4, fy) * smoothstep(.62, .52, fy) * step(-.82, u.x) * step(u.x, .72 - .4 * hash(float2(row, seed))) * step(.5, row) * step(row, 7.5) * (hash(float2(floor(u.x * 16), row + seed)) > .2 ? 1 : .3);
            float gutter = smoothstep(-.6, -1, u.x);
            float tone = page * (.8 - .35 * gutter + .08 * fbm(u * 5 + seed)) + block * (.78 - .25 * (.5 + .5 * sin(u.x * 90))) + cov * .35;
            float inkH = max(max(InkC(sdPage, .035) * page, InkC(sdCov, .04)), text * .5 * page);
            MatC(fill, tone, inkH, page * .08, float3(1, .95, .8), I, hot);
            // the cover's lip is leather whatever the paper tint
            if (_Matter > .5) hot = lerp(hot, float3(-1.5, -2.3, -2.3) * (1 - saturate(inkH)) + float3(-5, -5, -5) * saturate(inkH), cov);
        }
    }
    else if (mode < 42.5)                                             // 42 rock chunk (0) / spike (1) / stone foot (2) / monolith (3) / boulder (4)
    {
        if (par < .5 || (par > 3.5 && par < 4.5))
        {
            bool boulder = par > 3.5;
            float n = boulder ? 11 : 7; float ang = (a / 6.2831853 + .5) * n; float i0 = floor(ang), f = frac(ang);
            float vr = boulder ? .2 : .32, base = boulder ? .72 : .6;
            float r0 = base + vr * hash(float2(fmod(i0, n) + seed, 1.7)), r1 = base + vr * hash(float2(fmod(i0 + 1, n) + seed, 1.7));
            float R = lerp(r0, r1, f);
            float sd = (R - r) * .9;
            float fill = smoothstep(-.015, .015, sd);
            // a lit, faceted lump: each facet tilts its own way, shaded from the top-left light
            float rr = saturate(r / R), ac = (i0 + .5) / n * 6.2831853 - 3.14159265, tone0 = hash(float2(i0, seed + 9));
            float3 nrm = normalize(float3(lerp(u / R, float2(cos(ac), sin(ac)) * rr, .55), sqrt(1 - rr * rr * .9) + .15));
            float diff = saturate(dot(nrm, normalize(float3(L, .7))));
            float grain = fbm(u * 6 + seed), crack = exp(-abs(fbm(u * (boulder ? 2.4 : 3.2) + seed) - .5) * 34) * smoothstep(.25, .5, rr + .3);
            float strata = boulder ? .06 * sin((u.x * L.y - u.y * L.x) * 18 + grain * 4) : 0;
            float tone = .08 + .78 * diff * (.8 + .4 * tone0) + .12 * grain + strata;
            float edgeLine = smoothstep(.04, 0, min(f, 1 - f)) * smoothstep(.3, .7, rr);
            float ink = max(InkC(sd, .07), crack * .75) + edgeLine * .35;
            float rim = exp(-pow((sd - .05) / .03, 2)) * saturate(dot(normalize(u + .001), L) * 1.4);
            MatC(fill, tone, ink, rim * .5, float3(1, .95, .85), I, hot);
        }
        else if (par < 1.5)
        {
            float h = saturate((u.y + 1) * .5);
            float jag = .08 * (vnoise(float2(u.y * 7, seed)) - .5);
            float w = (1 - pow(h, .9)) * .78 + jag * (1 - h);
            float x = u.x - jag * .6, ax = abs(x);
            float sd = min(w - ax, (u.y + .96) * .5);
            float fill = smoothstep(-.015, .015, sd);
            float side = x / max(w, .05);
            float facet = smoothstep(-.15, .15, side - .2 * sin(u.y * 5 + seed));
            float strata = .08 * sin(u.y * 16 + u.x * 3 + seed) + .1 * fbm(u * float2(3, 8) + seed);
            float tone = lerp(.82, .22, facet) + strata + .1 * h;
            float ridge = exp(-pow((side - .2 * sin(u.y * 5 + seed)) / .08, 2)) * .4;
            float rim = exp(-pow((sd - .03) / .03, 2)) * step(x, 0) * .5 + exp(-pow(length(u - float2(0, .95)) / .12, 2)) * .5;
            MatC(fill, tone, max(InkC(sd, .06), ridge), rim, float3(1, .95, .85), I, hot);
        }
        else if (par < 2.5)
        {
            // a giant stone foot seen from above, toes toward the viewer: a domed sole, five chiselled toes, the cut stone of the ankle on top
            float2 fp = (u - float2(0, .2)) / float2(.46 + .16 * saturate(.6 - u.y), .68); float sdSole = (1 - length(fp)) * .5;
            float toeSd = -1, toeId = 0;
            for (int k = 0; k < 5; k++)
            {
                float ts = k == 0 ? .19 : .145 - k * .008;
                float2 tc = float2(-.44 + k * .22 + (k == 0 ? -.04 : 0), -.66 + abs(k - 1.2) * .06);
                float d = ts - length((u - tc) / float2(1, 1.2));
                if (d > toeSd) { toeSd = d; toeId = k; }
            }
            float sdAnk = .36 - length((u - float2(.02, .5)) / float2(1, .85));
            float all = max(max(sdSole, toeSd), sdAnk);
            float fill = smoothstep(-.015, .015, all);
            float onToe = step(max(sdSole, sdAnk), toeSd), onAnk = step(max(sdSole, toeSd), sdAnk) ;
            float2 tc2 = float2(-.44 + toeId * .22 + (toeId < .5 ? -.04 : 0), -.66 + abs(toeId - 1.2) * .06);
            float2 nn = onToe > .5 ? (u - tc2) / .18 : onAnk > .5 ? (u - float2(.02, .5)) / .36 : fp;
            float3 n3 = normalize(float3(nn * .8, sqrt(saturate(1 - dot(nn, nn) * .6)) + .2));
            float tone = .04 + .62 * saturate(dot(n3, normalize(float3(L, .6)))) + .12 * fbm(u * 5 + seed) + .05 * sin(u.y * 26 + fbm(u * 3) * 5) + onAnk * .12;
            float nail = onToe * exp(-pow(length((u - tc2 - float2(0, -.03)) / float2(1, 1.3)) / .06, 2));
            float crack = exp(-abs(fbm(u * 2.6 + seed) - .5) * 30) * .7;
            float ink = max(InkC(all, .06), max(max(InkC(toeSd, .035) * onToe, InkC(sdAnk, .04) * onAnk), crack)) + nail * .35;
            MatC(fill, tone, ink, exp(-pow((all - .04) / .03, 2)) * saturate(dot(normalize(nn + .001), L)) * .45, float3(1, .92, .78), I, hot);
        }
        else
        {
            // a carved standing monolith: a tall slab with chamfered top, chisel marks, a column of runes that glow as the accent
            float2 p = u / float2(.56, .96);
            float chamfer = (1.0 - p.y) * .8 + (.56 - abs(u.x)) ;
            float sd = min(min(.56 - abs(u.x), .96 - abs(u.y)), (1.22 - abs(u.x) * .6 - u.y) * .5);
            float fill = smoothstep(-.015, .015, sd);
            float face = smoothstep(-.1, .1, u.x - .25);
            float tone = lerp(.66, .3, face) + .12 * fbm(u * float2(4, 7) + seed) + .07 * sin(u.y * 30 + fbm(u * 2) * 6) - .15 * smoothstep(.6, .96, u.y) * face;
            float rune = 0;
            for (int k = 0; k < 4; k++)
            {
                float y0 = .55 - k * .38; float h1 = hash(float2(k, seed)), h2 = hash(float2(k + 5, seed));
                rune = max(rune, smoothstep(.05, .02, segC(u, float2(-.12, y0 + .1), float2(.08, y0 - .12 + .1 * h1))));
                rune = max(rune, smoothstep(.05, .02, segC(u, float2(-.16 + .1 * h2, y0 - .02), float2(.14, y0 + .04))));
            }
            float ink = max(InkC(sd, .06), exp(-pow((u.x - .25) / .02, 2)) * .5) + rune * .8;
            MatC(fill, tone, ink, rune * fill * 1.2, float3(1, .78, .35), I, hot);
        }
    }
    else if (mode < 43.5 && par > 1.5)                                // 43 par 2: a rubber-stamp imprint; par 3: a fluttering swallowtail banner (q = time)
    {
        if (par < 2.5)
        {
            float2 d2 = abs(u) - float2(.86, .74); float sd = -max(d2.x, d2.y);
            float frame = smoothstep(.0, .02, sd) * smoothstep(.13, .1, sd) + smoothstep(.15, .17, sd) * smoothstep(.22, .2, sd);
            // a crown glyph and two lines of lettering, worn by the grain of the rubber
            float crown = smoothstep(.05, .02, min(min(segC(u, float2(-.4, .05), float2(.4, .05)), min(segC(u, float2(-.4, .05), float2(-.45, .45)), segC(u, float2(.4, .05), float2(.45, .45)))),
                          min(min(segC(u, float2(-.45, .45), float2(-.2, .2)), segC(u, float2(-.2, .2), float2(0, .5))), min(segC(u, float2(0, .5), float2(.2, .2)), segC(u, float2(.2, .2), float2(.45, .45))))));
            float row = step(abs(u.y + .28), .06) * step(abs(u.x), .55) * step(.3, hash(float2(floor(u.x * 14), seed))) + step(abs(u.y + .5), .04) * step(abs(u.x), .4) * step(.3, hash(float2(floor(u.x * 14), seed + 3)));
            float wear = smoothstep(.25, .55, fbm(u * 9 + seed));
            float ink = saturate(frame + crown + row) * wear * smoothstep(1, .6, q);
            MatC(ink, .45 + .2 * fbm(u * 4 + seed), 0, ink * .25, float3(1, .5, .45), I, hot);
        }
        else
        {
            float y01 = 1 - (u.y * .5 + .5);                               // 0 at the bar, 1 at the tails
            float wave = sin(u.y * 4.2 + q * 7 + seed) * .14 * y01 + sin(u.y * 9 - q * 11 + seed * 2) * .04 * y01;
            float x = u.x - wave, w = .62 * (1 - .12 * y01), xn = x / w;
            float bottom = -.94 + .42 * (1 - abs(xn));
            float sd = min(w - abs(x), min(.97 - u.y, (u.y - bottom) * .7));
            float fill = smoothstep(-.012, .012, sd);
            float fold = cos(u.y * 4.2 + q * 7 + seed) * y01;
            float tone = .55 + .3 * fold * sign(xn + .001) * .6 - .2 * xn * .5 + .08 * fbm(u * 5 + seed) + .15 * cos(u.y * 9 - q * 11 + seed * 2) * y01;
            float trim = smoothstep(.13, .09, sd) * smoothstep(.04, .07, sd);
            float2 ep = float2(x, u.y - .1);
            float emblem = smoothstep(.06, .03, abs(length(ep) - .22)) + smoothstep(.22, .18, abs(ep.x) + abs(ep.y) * .8) * smoothstep(.1, .14, abs(ep.x) + abs(ep.y) * .8) ;
            MatC(fill, tone, max(InkC(sd, .04), trim * .3), (trim + saturate(emblem)) * fill * .9, float3(1, .85, .4), I, hot);
        }
    }
    else if (mode < 43.5)                                             // 43 ink rune, q = how much is drawn (light only)
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
        // par 1: the glyph strokes alone in wet dark ink, a gold sheen on the light layer
        if (par > .5) { float gf = saturate(glyph * 1.6); MatC(gf, .5 + .3 * smoothstep(.2, .9, u.y - u.x + .5) + .1 * fbm(u * 6 + seed), smoothstep(.75, .3, gf) * gf * 1.2, gf * (.3 + .4 * smoothstep(.2, .9, u.y - u.x + .5)), float3(1, .9, .55), I, hot); }
    }
    else if (mode < 44.5)                                             // 44 cast-iron cooking pot (par 0) or its lid (par 2)
    {
        if (par < 1)
        {
            float2 p = float2(u.x / .74, (u.y + .16) / .58);
            float bodyD = 1 - (pow(abs(p.x), 3.0) + pow(abs(p.y), 2.2));
            float2 rp = float2(u.x / .82, (u.y - .38) / .17);
            float rimD = 1 - length(rp);
            float2 e1 = float2(abs(u.x) - .86, u.y - .22);
            float earD = .05 - abs(length(e1) - .12);
            float2 lg = float2(abs(u.x) - .42, u.y + .76);
            float legD = min(.07 - abs(lg.x + lg.y * .2), .12 - abs(lg.y));
            float sd = max(max(bodyD * .3, rimD * .16), max(earD, legD));
            float fill = smoothstep(-.012, .012, sd);
            float2 sp = float2(u.x / .66, (u.y - .38) / .1);
            float inner = smoothstep(1.02, .9, length(sp));
            float onBody = step(rimD * .16, bodyD * .3);
            // round iron belly lit from the top-left, a bright lip on the rim, the inside of the rim in shadow
            float3 nrm = normalize(float3(p.x * .9, p.y * .5, sqrt(saturate(1 - p.x * p.x * .7)) + .2));
            float tone = onBody * (.12 + .7 * saturate(dot(nrm, normalize(float3(L, .6))))) + (1 - onBody) * (.5 + .3 * saturate(-rp.y + .3)) - inner * .45 + .08 * fbm(u * 7 + seed);
            float ink = max(InkC(sd, .05), exp(-pow((rimD) / .06, 2)) * .7 * step(-.1, rimD) * (1 - inner));
            float shine = exp(-pow((u.x + .4) / .06, 2)) * step(u.y, .2) * step(-.45, u.y) * onBody;
            MatC(fill, tone + shine * .4, ink, shine * .7 + exp(-pow((rimD - .12) / .08, 2)) * step(0, rp.y) * .4, float3(1, .9, .75), I, hot);
        }
        else
        {
            float2 p = float2(u.x / .82, (u.y + .3) / .52);
            float sdDome = min((1 - length(p)) * .4, (u.y + .3) * .6);
            float sdKnob = .15 - length((u - float2(0, .28)) / float2(1.2, 1));
            float sdLip = min(.06 - abs(u.y + .32), .86 - abs(u.x));
            float sd = max(max(sdDome, sdKnob), sdLip);
            float fill = smoothstep(-.012, .012, sd);
            float3 nrm = normalize(float3(p.x, p.y, .7));
            float tone = .2 + .65 * saturate(dot(nrm, normalize(float3(L, .7)))) + .06 * fbm(u * 6 + seed) + step(sdDome, sdKnob) * .15;
            float shine = exp(-pow(length((u - float2(-.38, .05)) / float2(.6, 1)) / .12, 2));
            MatC(fill, tone + shine * .3, InkC(sd, .05), shine * .6, float3(1, .92, .8), I, hot);
        }
    }
    else if (mode < 45.5)                                             // 45 pie seen from above: par 0 crust (gaps open), par 1 the filling, par 2 a wedge of crust
    {
        if (par < .5)
        {
            float R = .82 + .04 * sin(a * 26);
            float sd = (R - r);
            float barD = min(abs(frac((u.x + u.y) * 2.4 + seed * .1) - .5), abs(frac((u.x - u.y) * 2.4) - .5));
            float isBar = smoothstep(.15, .1, barD);
            float rimBand = smoothstep(.58, .64, r);
            float crust = max(isBar, rimBand);
            float fill = smoothstep(-.015, .015, sd) * crust;
            float crimp = .5 + .5 * sin(a * 26);
            float bake = fbm(u * 5 + seed);
            float tone = rimBand * (.45 + .3 * crimp * saturate(dot(normalize(u + .001), L) + .6)) + (1 - rimBand) * isBar * (.55 + .35 * (.5 - barD) * 2) + .14 * bake - .12 * smoothstep(.35, .7, bake);
            float ink = max(InkC(sd, .05), smoothstep(.15, .12, barD) * smoothstep(.1, .13, barD) * (1 - rimBand) * .7) + smoothstep(.58, .6, r) * smoothstep(.62, .6, r) * .6;
            float sugar = step(.92, hash(floor(u * 30) + seed)) * crust;
            MatC(fill, tone + sugar * .3, ink, sugar * .5 * fill, float3(1, .95, .8), I, hot);
        }
        else if (par < 1.5)
        {
            float R = .7 + .05 * sin(a * 7 + seed);
            float sd = R - r; float fill = smoothstep(-.015, .015, sd);
            float blob = fbm(u * 4 + seed + q * .5);
            float tone = .35 + .45 * smoothstep(.45, .7, blob) + .3 * exp(-pow(length(u - float2(-.25, .25)) / .2, 2));
            float bubble = smoothstep(.06, .03, abs(length(frac(u * 3 + seed) - .5) - .18)) * .5;
            MatC(fill, tone, InkC(sd, .04) * .7 + bubble * .4, fill * (.25 + .4 * smoothstep(.55, .75, blob)), float3(1, .45, .3), I, hot);
        }
        else
        {
            float2 v = u - float2(-.7, 0); float ang = atan2(v.y, v.x), rr = length(v);
            float sd = min(min(1.5 - rr, (.42 - abs(ang)) * rr), rr - .02);
            float fill = smoothstep(-.015, .015, sd);
            float rimB = smoothstep(1.25, 1.35, rr);
            float tone = .5 + .3 * rimB * (.5 + .5 * sin(ang * 60)) + .15 * fbm(u * 6 + seed) - .2 * smoothstep(.2, -.2, v.y);
            MatC(fill, tone, InkC(sd, .05), 0, float3(1, 1, 1), I, hot);
        }
    }
    else if (mode < 46.5)                                             // 46 clock face, par = minute-hand angle in radians (light only)
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
    else if (mode < 47.5)                                             // 47 honey glass prism (0), brass lantern (1), lighthouse light cone (10, light only)
    {
        if (par > 9.5)
        {
            // a cone of light from the lamp at u.x = -1, widening to the far end; zero on every quad edge
            float x01 = (u.x + 1) * .5, w = .1 + .9 * x01;
            float across = abs(u.y) / w;
            float body = exp(-across * across * 2.2) * smoothstep(1, .7, across);
            float n = .75 + .25 * vnoise(float2(u.x * 4 - q * 3 + seed, u.y * 3 / w));
            float core = exp(-across * across * 30) * (1 - x01 * .6);
            I = (body * (.45 + .55 * (1 - x01)) * n + core * .8) * smoothstep(1, .55, x01) * smoothstep(0, .06, x01) * smoothstep(1, .85, abs(u.y)) * (_Matter > .5 ? 0 : 1);
            hot = float3(1, .95, .8);
        }
        else if (par > .5)
        {
            // lantern: a ring handle, a domed cap, four glass panes in a brass cage, a footed base; the panes and flame glow
            float sdBody = min(.42 - abs(u.x), .42 - abs(u.y + .05));
            float sdCap = min((.5 - abs(u.x) * 1.0 - (u.y - .37) * 1.6) * .6, (u.y - .37 + .03));
            float sdBase = min(.5 - abs(u.x), .08 - abs(u.y + .52));
            float sdRing = .045 - abs(length((u - float2(0, .78)) / float2(1, 1.1)) - .14);
            float sdKnob = .07 - length(u - float2(0, .6));
            float sd = max(max(max(sdBody, sdCap), max(sdBase, sdRing)), sdKnob);
            float pane = smoothstep(.0, .02, sdBody) * (1 - smoothstep(.05, .02, abs(u.x)) * 1) * smoothstep(.06, .09, sdBody);
            float bars = (1 - pane) * step(sdCap, sdBody + .001) * smoothstep(-.01, .01, sdBody);
            float metal = smoothstep(-.012, .012, sd) * (1 - pane);
            float fill = metal + pane * .55;
            float tone = metal * (.38 + .35 * saturate(dot(normalize(u + .001), L)) + .1 * fbm(u * 8 + seed)) + pane * .95;
            float flame = exp(-pow(length((u - float2(0, -.08)) / float2(.6, 1.2)) / .12, 2));
            MatC(fill, tone, InkC(sd, .045) * (1 - pane) + bars * .2, pane * (.55 + .45 * flame) + flame * 1.2 + metal * exp(-pow((sd - .03) / .03, 2)) * .3, float3(1, .9, .6), I, hot);
        }
        else
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
    }
    else if (mode < 48.5)                                             // 48 spring coil along u.x (par = turns < 20), pendulum rod (20), pendulum bob (21)
    {
        if (par > 20.5)
        {
            // a heavy brass bob: a domed disc with an engraved sun ring, a raised boss and a bright rim catching the light
            float sd = .86 - r; float fill = smoothstep(-.012, .012, sd);
            float3 nrm = normalize(float3(u * .9, sqrt(saturate(1 - r * r * .9)) + .25));
            float tone = .18 + .74 * saturate(dot(nrm, normalize(float3(L, .65)))) + .06 * fbm(u * 6 + seed);
            float engr = exp(-pow((r - .58) / .02, 2)) + exp(-pow((r - .66) / .015, 2)) * .6 + smoothstep(.04, .015, abs(sin(a * 8)) * r) * smoothstep(.2, .3, r) * smoothstep(.55, .5, r) * .6;
            float boss = exp(-pow((r - .16) / .025, 2));
            float spec = exp(-pow(length(u - L * .45) / .14, 2));
            MatC(fill, tone + spec * .4, max(InkC(sd, .05), (engr + boss) * .55), exp(-pow((sd - .04) / .03, 2)) * saturate(dot(normalize(u + .001), L)) * .8 + spec * .5, float3(1, .93, .7), I, hot);
        }
        else if (par > 19.5)
        {
            // the rod: a round brass bar along u.y with collars, shaded like a cylinder (lit on the left)
            float w = .34 + .22 * exp(-pow((u.y - .55) / .05, 2)) + .22 * exp(-pow((u.y + .7) / .05, 2));
            float sd = min(w - abs(u.x), .97 - abs(u.y)) * .5;
            float fill = smoothstep(-.012, .012, sd);
            float cyl = u.x / w;
            float tone = .6 - .45 * cyl + .25 * exp(-pow((cyl + .45) / .15, 2)) + .04 * sin(u.y * 40);
            MatC(fill, tone, InkC(sd, .06), exp(-pow((cyl + .5) / .15, 2)) * fill * .5, float3(1, .92, .7), I, hot);
        }
        else
        {
            float turns = par < 1 ? 5 : par;
            float ph = u.x * turns * 3.14159265;
            float d = abs(u.y - sin(ph) * .5);
            float wire = exp(-pow(d / .12, 2));
            float back = exp(-pow(abs(u.y - cos(ph) * .5) / .1, 2)) * .45;
            I = saturate((wire + back) * smoothstep(1, .85, abs(u.x)));
            hot = float3(1, .9, .6);
        }
    }
    else                                                              // 49 loaf of bread (par 0) or apple (par 1)
    {
        if (par < .5)
        {
            float2 p = float2(u.x / .85, u.y / .52);
            float sd = (1 - length(p)) * .45;
            float fill = smoothstep(-.012, .012, sd);
            float score = 0;
            for (int k = -1; k <= 1; k++) score = max(score, smoothstep(.06, .02, segC(u, float2(k * .36 - .1, -.25), float2(k * .36 + .08, .28))));
            float3 nrm = normalize(float3(p * .9, .6));
            float tone = .2 + .7 * saturate(dot(nrm, normalize(float3(L, .6)))) + .1 * fbm(u * 5 + seed);
            MatC(fill, tone + score * .25, InkC(sd, .045) + score * .3, score * fill * .25, float3(1, .9, .7), I, hot);
        }
        else
        {
            float sdB = .7 - length((u - float2(0, -.08)) / float2(1.05, 1)) - .12 * smoothstep(.25, 0, length(u - float2(0, .58)));
            float sdS = .045 - segC(u, float2(0, .5), float2(.08, .85));
            float sdL = .16 - length((u - float2(.24, .76)) * float2(1, 2.2));
            float sd = max(sdB, max(sdS, sdL));
            float fill = smoothstep(-.012, .012, sd);
            float3 nrm = normalize(float3(u * 1.2, .7));
            float tone = .18 + .72 * saturate(dot(nrm, normalize(float3(L, .65)))) + .08 * fbm(u * 4 + seed);
            float spec = exp(-pow(length(u - float2(-.3, .14)) / .12, 2));
            MatC(fill, tone + spec * .5, InkC(sd, .045), spec * .7, float3(1, .95, .9), I, hot);
        }
    }
}
#endif
