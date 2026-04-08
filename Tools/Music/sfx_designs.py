"""SFX v6 designs: 75 boss cues (15 bosses x entrance / windup / hit / phase / defeat) and 30 weapon / pickup sounds
(10 families x a/b/c), after Docs/Audio_Prompts_ALL.txt #9-#113. Procedural layers + CC0 Kenney foley."""
from sfx_lib import *  # noqa: F401,F403

D = {}


def sfx(name, dur, wet=0.12, peak=-3.0):
    def deco(fn):
        D[name] = (fn, dur, wet, peak)
        return fn
    return deco


def Z(d):
    return np.zeros(N(d)).view(Sig)


def K(pat, r, ratio=1.0, gain=1.0):
    x = sample(pat, rng=r)
    return np.asarray((pitch(x, ratio) if ratio != 1.0 else x) * gain).view(Sig)


def grunt(d, r, f=(95, 70)):
    return voiced(d, line(d, *f), r, ((380, 6), (800, 7), (2300, 6)), breath=0.25) * bell_env(d, 0.25, 1.4)


# ================================================================================================ Chair Porter (wood, chairs)
@sfx('boss_chairporter_entrance', 3.0)
def _(r):
    d = 3.0
    o = Z(d)
    place(o, scrape(2.6, r, 700, 1.0, 30) * bell_env(2.6, 0.5, 1.2), 0.1, 0.5)
    place(o, creak(2.4, r, (14, 26), ((160, 10), (420, 12), (900, 12))), 0.3, 0.9)
    for t in (0.0, 0.9, 1.7):
        place(o, K('creak*', r, 0.7), t, 0.35)
        place(o, K('impactWood_light*', r, 0.8), t + 0.4, 0.3)
    place(o, grunt(0.8, r), 2.0, 0.55)
    return o


@sfx('boss_chairporter_windup', 1.5)
def _(r):
    d = 1.5
    o = Z(d)
    for t in np.arange(0, 1.2, 0.17):
        place(o, scrape(0.14, r, 1100, 1.4, 70) * bell_env(0.14, 0.3), t, 0.6)
    place(o, creak(1.4, r, (20, 70), ((300, 10), (700, 12), (1600, 12)), amp_env=line(1.4, 0.2, 1.0, 1.5)), 0.05, 1.0)
    return o


@sfx('boss_chairporter_hit', 1.0)
def _(r):
    d = 1.0
    o = Z(d)
    place(o, K('impactWood_heavy*', r, 0.8), 0, 1.0)
    place(o, thump(70, 0.6, r, tau=0.12, lpf=500), 0, 0.9)
    place(o, K('impactSoft_heavy*', r, 0.85), 0.005, 0.6)
    place(o, creak(0.4, r, (30, 15)), 0.12, 0.25)
    return o


@sfx('boss_chairporter_phase', 2.5)
def _(r):
    d = 2.5
    o = Z(d)
    for t in sorted(r.uniform(0, 1.5, 13)):
        place(o, K('impactWood_*', r, r.uniform(0.85, 1.25)), t, r.uniform(0.3, 0.7))
    place(o, creak(1.3, r, (12, 6), ((140, 12), (360, 12), (800, 10)), amp_env=bell_env(1.3, 0.3, 1.0)), 1.15, 1.0)
    return o


@sfx('boss_chairporter_defeat', 4.0)
def _(r):
    d = 4.0
    o = Z(d)
    place(o, creak(1.2, r, (10, 22), ((150, 12), (380, 12))), 0.0, 0.8)
    t = 1.0
    for k in range(9):
        place(o, K('impactWood_*', r, r.uniform(0.75, 1.1)), t, 0.9 - k * 0.06)
        t += r.uniform(0.08, 0.25)
    place(o, thump(60, 0.7, r), 1.1, 0.7)
    place(o, K('creak*', r, 0.85), 3.1, 0.25)
    return o


# ================================================================================================ The Door That Stayed Shut
@sfx('boss_doorthatstayedshut_entrance', 3.0)
def _(r):
    d = 3.0
    o = Z(d)
    for t in (0.0, 0.62, 1.24):
        place(o, K('impactWood_heavy*', r, 0.7), t, 0.9)
        place(o, thump(65, 0.5, r, tau=0.1), t, 0.8)
    for k in range(6):
        place(o, K('metalClick*', r, r.uniform(0.9, 1.15)), 2.0 + k * 0.08 + r.uniform(0, 0.03), 0.4)
    place(o, K('metalLatch*', r), 2.15, 0.5)
    return o


@sfx('boss_doorthatstayedshut_windup', 1.5)
def _(r):
    d = 1.5
    o = Z(d)
    grind = bp(noise(1.3, r) * np.clip(lp(noise(1.3, r), 25) * 10, 0, 2), 1800, 2.5)
    place(o, grind * line(1.3, 0.3, 1.0), 0.05, 0.5)
    place(o, creak(1.3, r, (25, 60), ((900, 18), (1900, 16), (3200, 14)), amp_env=line(1.3, 0.2, 1, 1.4)), 0.05, 0.7)
    place(o, K('metalClick*', r, 0.8), 1.3, 0.6)
    return o


@sfx('boss_doorthatstayedshut_hit', 1.0, wet=0.2)
def _(r):
    d = 1.0
    o = Z(d)
    place(o, K('doorClose_*', r, 0.8), 0, 1.0)
    place(o, thump(55, 0.8, r, tau=0.18, lpf=400), 0, 1.0)
    place(o, K('impactWood_heavy*', r, 0.65), 0.002, 0.6)
    return o


@sfx('boss_doorthatstayedshut_phase', 2.5)
def _(r):
    d = 2.5
    o = Z(d)
    place(o, creak(1.4, r, (60, 140), ((700, 25), (1500, 22), (2900, 18)), jitter=0.1, amp_env=bell_env(1.4, 0.3, 1.2)), 0, 0.9)
    place(o, K('doorOpen_*', r, 0.9), 0.1, 0.6)
    place(o, wind(2.1, r, 600, 0.8, 0.9) * env_ar(2.1, 0.4, 0.8), 0.4, 0.9)
    return o


@sfx('boss_doorthatstayedshut_defeat', 4.0, wet=0.15)
def _(r):
    d = 4.0
    o = Z(d)
    place(o, creak(1.5, r, (14, 8), ((250, 12), (600, 12))), 0.0, 0.4)
    place(o, K('doorClose_*', r, 0.95), 1.5, 0.5)
    place(o, K('metalLatch*', r, 1.0), 2.3, 0.7)
    return o


# ================================================================================================ The Bedside (clock, lullaby)
LULL = [69, 72, 76, 74, 72, 69, 71, 72]


@sfx('boss_bedside_entrance', 3.0)
def _(r):
    d = 3.0
    o = Z(d)
    place(o, ticks(list(np.arange(0, 3.0, 0.5)), r, (2200, 700)), 0, 0.8)
    times = np.cumsum([0] + [0.3 * (1.12 ** k) for k in range(7)])
    notes = [(t, m - 0.3 * k / 7) for k, (t, m) in enumerate(zip(times, LULL))]
    mb = np.zeros(N(d))
    for k, (t, m) in enumerate(notes):
        place(mb, tine_notes([(0, m)], 1.4, r, 0.8), t, 0.7)
    place(o, mb, 0.1, 0.7)
    hum = voiced(2.6, 220 * 2 ** ((np.interp(tt(2.6), times, LULL) - 69) / 12) * 0.5, r, ((300, 8), (800, 8)), 0.1)
    place(o, hum * bell_env(2.6, 0.3), 0.1, 0.18)
    return o


@sfx('boss_bedside_windup', 1.5)
def _(r):
    d = 1.5
    o = Z(d)
    place(o, whoosh(0.9, r, 300, 1400, 1.2), 0.0, 0.8)
    place(o, K('cloth*', r, 0.9), 0.15, 0.6)
    place(o, whoosh(0.5, r, 500, 2000, 1.4), 0.7, 0.5)
    place(o, K('metalClick*', r, 1.4), 1.25, 0.5)
    place(o, click(0.003, r, 4500), 1.27, 0.4)
    return o


@sfx('boss_bedside_hit', 1.0)
def _(r):
    d = 1.0
    o = Z(d)
    place(o, K('impactSoft_heavy*', r, 0.8), 0, 1.0)
    place(o, thump(80, 0.4, r, tau=0.09), 0, 0.6)
    place(o, lp(bell(1046, 0.8, 'brass', 0.5, r), 2500), 0.03, 0.25)
    return o


@sfx('boss_bedside_phase', 2.5)
def _(r):
    d = 2.5
    o = Z(d)
    ring = np.zeros(N(2.0))
    for k, t in enumerate(np.arange(0, 1.9, 1 / 24)):
        place(ring, bell(1750 if k % 2 else 1680, 0.12, 'brass', 0.08, r) + click(0.002, r, 3000) * 0.5, t)
    place(o, ring * (0.85 + 0.15 * np.sin(2 * np.pi * 3 * tt(2.0))), 0, 0.9)
    place(o, K('metalClick*', r, 0.8), 1.98, 0.8)
    place(o, lp(bell(1750, 0.4, 'brass', 0.25, r), 3000), 1.99, 0.3)
    return o


@sfx('boss_bedside_defeat', 4.0)
def _(r):
    d = 4.0
    o = Z(d)
    yd = 1.8
    place(o, voiced(yd, line(yd, 230, 140, 0.8), r, ((700, 6), (1100, 7), (2600, 6)), 0.5) * bell_env(yd, 0.35, 1.2), 0.0, 0.35)
    place(o, breath(yd, r, (600, 1000, 2400)) * bell_env(yd, 0.6), 0.1, 0.3)
    times, t, gap = [], 1.2, 0.45
    while t < 3.7:
        times.append(t)
        t += gap
        gap *= 1.22
    place(o, ticks([x - 1.2 for x in times], r, (2200, 700)), 1.2, 0.8)
    return o


# ================================================================================================ The Last Customer (shop, cane, coins)
@sfx('boss_lastcustomer_entrance', 3.0)
def _(r):
    d = 3.0
    o = Z(d)
    place(o, bell(1320, 2.0, 'brass', 0.7, r) + bell(1980, 2.0, 'brass', 0.5, r) * 0.6, 0.0, 0.6)
    place(o, bell(1320, 1.6, 'brass', 0.5, r), 0.18, 0.35)
    for t in (1.1, 1.55):
        place(o, K('impactPlate_light*', r, 1.3), t, 0.5)
        place(o, click(0.004, r, 2500), t, 0.4)
    ch = Z(0.9)
    for k, t in enumerate((0, 0.17, 0.33)):
        place(ch, voiced(0.13, 150 - k * 6, r, ((600, 6), (1150, 7), (2500, 6)), 0.6) * bell_env(0.13, 0.2), t, 0.9 - 0.2 * k)
    place(o, ch, 2.0, 0.3)
    return o


@sfx('boss_lastcustomer_windup', 1.5)
def _(r):
    d = 1.5
    o = Z(d)
    sp = Z(1.3)
    for t in np.arange(0, 1.25, 1 / 28):
        place(sp, coin_clink(r, 3600, 0.05) * (0.3 + 0.7 * t / 1.3), t)
    place(o, sp, 0, 0.5)
    place(o, K('chips-handle-*', r), 0.05, 0.3)
    place(o, whoosh(0.35, r, 600, 2800, 2.0), 0.6, 0.7)
    place(o, whoosh(0.3, r, 700, 3000, 2.0), 1.0, 0.6)
    return o


@sfx('boss_lastcustomer_hit', 1.0)
def _(r):
    d = 1.0
    o = Z(d)
    place(o, K('impactMining*', r, 1.0), 0, 0.9)
    place(o, click(0.006, r, 1800, 2), 0, 0.6)
    for k in range(7):
        place(o, coin_clink(r), 0.05 + r.uniform(0, 0.6), r.uniform(0.2, 0.5))
    place(o, K('chips-collide-*', r), 0.04, 0.5)
    return o


@sfx('boss_lastcustomer_phase', 2.5)
def _(r):
    d = 2.5
    o = Z(d)
    place(o, K('bookClose*', r, 0.75), 0, 1.0)
    place(o, thump(90, 0.3, r, tau=0.06), 0, 0.5)
    place(o, breath(1.6, r, (500, 900, 2200), 5, line(1.6, 0, 1, 1.6) * env_ar(1.6, 0.05, 0.15)), 0.6, 0.6)
    return o


@sfx('boss_lastcustomer_defeat', 4.0)
def _(r):
    d = 4.0
    o = Z(d)
    place(o, K('dropLeather*', r, 0.9), 0.0, 0.6)
    place(o, K('impactSoft_medium*', r, 0.9), 0.05, 0.4)
    place(o, spin(2.8, r, (9, 26), 3000, (0.5, 1.0)), 0.6, 0.7)
    place(o, coin_clink(r, 3300, 0.6), 3.4, 0.6)
    return o


# ================================================================================================ The Keeper (stone hand, wind, deep chime)
@sfx('boss_keeper_entrance', 3.0, wet=0.2)
def _(r):
    d = 3.0
    o = Z(d)
    place(o, rumble(2.9, r, 70) * bell_env(2.9, 0.4, 1.3), 0, 1.0)
    grind = bp(noise(2.0, r) * np.clip(lp(noise(2.0, r), 18) * 12, 0, 2), 350, 1.2)
    place(o, grind * bell_env(2.0, 0.5), 0.3, 0.6)
    place(o, wind(3.0, r, 450, 0.7, 0.3) * env_ar(3.0, 0.8, 0.8), 0, 0.5)
    place(o, K('impactMining*', r, 0.5), 1.0, 0.25)
    return o


@sfx('boss_keeper_windup', 1.5, wet=0.2)
def _(r):
    d = 1.5
    o = Z(d)
    place(o, (bell(110, 1.5, 'church', 3.0, r, 0.4) + bell(220, 1.5, 'church', 2.0, r, 0.6) * 0.5) * line(1.5, 0, 1, 2.0), 0, 1.0)
    place(o, bp(noise(1.5, r, 'pink'), 320, 0.8) * line(1.5, 0.1, 1, 1.5) + rumble(1.5, r, 60) * line(1.5, 0.2, 1, 1.5) * 0.5, 0, 0.5)
    return o


@sfx('boss_keeper_hit', 1.0, wet=0.2)
def _(r):
    d = 1.0
    o = Z(d)
    place(o, K('impactMining*', r, 0.55), 0, 1.0)
    place(o, thump(45, 0.8, r, 0.8, 0.22, 0.5, 300), 0, 1.0)
    place(o, bell(880, 0.9, 'glass', 0.6, r) * 0.2, 0.12, 0.6)
    return o


@sfx('boss_keeper_phase', 2.5, wet=0.2)
def _(r):
    d = 2.5
    o = Z(d)
    for t in sorted(r.uniform(0, 0.9, 8)):
        place(o, K('impactMining*', r, r.uniform(0.6, 0.9)), t, r.uniform(0.4, 0.8))
    place(o, crackle(1.0, r, 80, (800, 4000)) * line(1.0, 1, 0.2), 0, 0.6)
    place(o, wind(2.2, r, 380, 0.5, 1.2) * line(2.2, 0.1, 1.0, 1.3) * env_ar(2.2, 0.1, 0.3), 0.3, 1.0)
    return o


@sfx('boss_keeper_defeat', 4.0, wet=0.22)
def _(r):
    d = 4.0
    o = Z(d)
    place(o, breath(2.2, r, (400, 800, 1800), 3, bell_env(2.2, 0.25, 1.2)), 0, 0.8)
    place(o, rumble(2.0, r, 50) * bell_env(2.0, 0.3), 0, 0.4)
    place(o, bell(98, 2.6, 'church', 2.2, r, 0.7), 1.4, 0.9)
    return o


# ================================================================================================ Undelivered King (paper letters, crown)
@sfx('boss_undeliveredking_entrance', 3.0)
def _(r):
    d = 3.0
    o = Z(d)
    place(o, flutter(3.0, r, 220, (1800, 7000), env=bell_env(3.0, 0.45, 1.0)), 0, 0.9)
    place(o, K('card-shuffle*', r, 0.9), 0.4, 0.5)
    place(o, K('card-fan-*', r), 1.1, 0.4)
    for t, f in ((1.6, 392), (1.85, 392), (2.05, 523)):
        place(o, kazoo(f, 0.22 if t < 2 else 0.7, r) * env_ar(0.22 if t < 2 else 0.7, 0.01, 0.06), t, 0.45)
    return o


@sfx('boss_undeliveredking_windup', 1.5)
def _(r):
    d = 1.5
    o = Z(d)
    t = 0.0
    gap = 0.28
    while t < 1.3:
        place(o, bp(noise(0.12, r) * env_ar(0.12, 0.01, 0.05), 3500, 1.2) + K('cards-pack-open-*', r, r.uniform(0.95, 1.2)) * 0.6, t, 0.6)
        t += gap
        gap *= 0.8
    place(o, flutter(1.5, r, 160, env=line(1.5, 0.1, 1.0, 1.4)), 0, 0.6)
    return o


@sfx('boss_undeliveredking_hit', 1.0)
def _(r):
    d = 1.0
    o = Z(d)
    place(o, K('bookPlace*', r, 0.8), 0, 1.0)
    place(o, K('impactPunch_medium*', r, 0.9), 0.003, 0.5)
    place(o, flutter(0.35, r, 120, env=env_exp(0.35, 0.1)), 0.02, 0.5)
    return o


@sfx('boss_undeliveredking_phase', 2.5)
def _(r):
    d = 2.5
    o = Z(d)
    tear = bp(noise(0.45, r) * np.clip(lp(noise(0.45, r), 60) * 8, 0, 2), 1600, 0.8) * env_ar(0.45, 0.02, 0.1)
    place(o, tear, 0, 0.9)
    place(o, whoosh(1.4, r, 300, 2200, 1.0, peak=0.25), 0.35, 0.8)
    place(o, flutter(2.0, r, 260, env=bell_env(2.0, 0.2, 1.3)), 0.35, 0.9)
    return o


@sfx('boss_undeliveredking_defeat', 4.0)
def _(r):
    d = 4.0
    o = Z(d)
    place(o, flutter(3.6, r, 45, (1500, 5000), (0.01, 0.04), env=line(3.6, 1, 0.3)), 0, 0.8)
    for t in sorted(r.uniform(0.2, 3.0, 9)):
        place(o, K('card-place-*', r, r.uniform(0.9, 1.1)), t, 0.18)
    crum = crackle(0.9, r, 140, (1500, 5000), (0.2, 0.8)) + bp(noise(0.9, r), 2500, 0.8) * 0.3
    place(o, crum * bell_env(0.9, 0.3), 2.9, 0.6)
    return o


# ================================================================================================ Scarlet Index (index cards, ink)
@sfx('boss_scarletindex_entrance', 3.0)
def _(r):
    d = 3.0
    o = Z(d)
    place(o, scrape(0.8, r, 900, 1.3, 90) * bell_env(0.8, 0.6), 0, 0.6)
    place(o, flutter(0.8, r, 140, (2500, 6000), env=bell_env(0.8, 0.6)), 0, 0.6)
    place(o, K('impactWood_light*', r, 1.1), 0.78, 0.5)
    q = Z(1.2)
    for t in np.arange(0, 1.1, 0.075):
        place(q, bp(noise(0.06, r) * env_ar(0.06, 0.005, 0.03), r.uniform(3000, 5500), 3), t, r.uniform(0.4, 1.0))
    place(o, q, 1.0, 0.55)
    for t in (2.3, 2.7):
        place(o, drip(r, 800), t, 0.5)
    return o


@sfx('boss_scarletindex_windup', 1.5)
def _(r):
    d = 1.5
    o = Z(d)
    t, gap = 0.0, 0.12
    while t < 1.4:
        place(o, K('card-slide-*', r, r.uniform(1.0, 1.3)) * 0.5 + K('card-place-*', r, 1.1) * 0.3, t, 0.55)
        t += gap
        gap = max(0.035, gap * 0.85)
    place(o, flutter(1.5, r, 200, env=line(1.5, 0.1, 1, 1.5)), 0, 0.5)
    return o


@sfx('boss_scarletindex_hit', 1.0)
def _(r):
    d = 1.0
    o = Z(d)
    place(o, K('impactPunch_heavy*', r, 1.0), 0, 0.8)
    place(o, K('bookPlace*', r, 1.0), 0.002, 0.6)
    sq = bp(noise(0.25, r) * np.clip(lp(noise(0.25, r), 200) * 8, 0, 2), 1100, 1.5) * env_exp(0.25, 0.06)
    place(o, sq, 0.02, 0.5)
    place(o, drip(r, 600, 0.08), 0.06, 0.35)
    return o


@sfx('boss_scarletindex_phase', 2.5)
def _(r):
    d = 2.5
    o = Z(d)
    place(o, K('metalPot*', r, 0.7), 0, 0.6)
    place(o, scrape(0.4, r, 700, 1.0, 120) * env_exp(0.4, 0.15), 0, 0.8)
    place(o, K('impactMetal_heavy*', r, 0.8), 0.25, 0.6)
    place(o, flutter(2.1, r, 320, (1800, 6500), env=bell_env(2.1, 0.15, 1.2)), 0.28, 1.0)
    for t in sorted(r.uniform(0.3, 1.8, 10)):
        place(o, K('card-place-*', r, r.uniform(0.9, 1.2)), t, 0.25)
    return o


@sfx('boss_scarletindex_defeat', 4.0)
def _(r):
    d = 4.0
    o = Z(d)
    place(o, scrape(1.8, r, 800, 1.2, 50) * line(1.8, 0.8, 0.2), 0, 0.5)
    place(o, flutter(1.8, r, 50, env=line(1.8, 0.6, 0.1)), 0, 0.4)
    place(o, K('impactWood_light*', r, 0.95), 1.8, 0.6)
    place(o, drip(r, 700, 0.08), 3.0, 0.6)
    return o


# ================================================================================================ Sleep Collector (pillows, snoring)
def snore(d, r, f=55):
    return voiced(d, line(d, f, f * 0.9), r, ((250, 5), (600, 6), (1300, 5)), breath=0.6, shimmer=0.08) * bell_env(d, 0.45, 1.2)


@sfx('boss_sleepcollector_entrance', 3.0)
def _(r):
    d = 3.0
    o = Z(d)
    place(o, snore(1.4, r), 0.0, 0.8)
    place(o, breath(0.9, r, (500, 900, 2000)) * 0.6, 1.4, 0.4)
    for t in (0.5, 1.2):
        place(o, K('impactSoft_medium*', r, 0.8), t, 0.4)
    times = [0.0, 0.32, 0.68, 1.1, 1.62]
    place(o, tine_notes([(t, m) for t, m in zip(times, [76, 74, 72, 69, 67])], 2.0, r, 0.9), 1.0, 0.45)
    return o


@sfx('boss_sleepcollector_windup', 1.5)
def _(r):
    return breath(1.5, r, (450, 850, 1900), 4, line(1.5, 0.05, 1.0, 1.4) * env_ar(1.5, 0.05, 0.08)) + \
        lp(noise(1.5, r, 'pink'), 300) * line(1.5, 0, 0.6, 2)


@sfx('boss_sleepcollector_hit', 1.0)
def _(r):
    d = 1.0
    o = Z(d)
    place(o, K('impactSoft_heavy*', r, 0.85), 0, 1.0)
    place(o, K('cloth*', r, 1.1), 0.01, 0.5)
    place(o, flutter(0.6, r, 90, (2000, 5000), (0.01, 0.03), env=env_exp(0.6, 0.2)), 0.03, 0.4)
    return o


@sfx('boss_sleepcollector_phase', 2.5)
def _(r):
    d = 2.5
    o = Z(d)
    place(o, voiced(0.35, line(0.35, 140, 90), r, ((400, 5), (900, 6)), 0.9) * bell_env(0.35, 0.2), 0, 0.9)
    for k, t in enumerate((0.45, 0.85, 1.2, 1.55)):
        place(o, boing(0.6, 140 * (1.06 ** k), r) * (1 - 0.15 * k), t, 0.5)
        place(o, K('impactSoft_medium*', r, 1.1), t, 0.25)
    return o


@sfx('boss_sleepcollector_defeat', 4.0)
def _(r):
    d = 4.0
    o = Z(d)
    place(o, breath(1.5, r, (500, 950, 2100), 4, bell_env(1.5, 0.15, 1.1)), 0, 0.9)
    for k, t in enumerate((1.7, 2.6, 3.4)):
        place(o, breath(0.7, r, (450, 900, 2000), 4) * (0.6 - 0.15 * k), t, 1.0)
    return o


# ================================================================================================ Wax Admiral (ship bell, timbers, wax)
@sfx('boss_waxadmiral_entrance', 3.0)
def _(r):
    d = 3.0
    o = Z(d)
    for t in (0.0, 0.5):
        place(o, bell(740, 2.4, 'brass', 1.2, r, 0.8), t, 0.6)
    place(o, creak(2.2, r, (10, 18), ((120, 12), (320, 12), (750, 10))), 0.7, 0.8)
    place(o, sizzle(1.2, r) * bell_env(1.2, 0.3), 1.6, 0.25)
    for t in (1.7, 2.3):
        place(o, drip(r, 500, 0.07), t, 0.4)
    return o


@sfx('boss_waxadmiral_windup', 1.5)
def _(r):
    d = 1.5
    o = Z(d)
    for t in sorted(r.uniform(0, 1.3, 26)):
        place(o, drip(r, r.uniform(250, 600), 0.05) * (0.3 + t / 1.5), t, 0.5)
    place(o, sizzle(1.5, r, 1500) * line(1.5, 0.2, 1.0), 0, 0.3)
    place(o, creak(1.0, r, (40, 90), ((500, 15), (1200, 15)), amp_env=line(1.0, 0.1, 1, 1.4)), 0.45, 0.6)
    return o


@sfx('boss_waxadmiral_hit', 1.0)
def _(r):
    d = 1.0
    o = Z(d)
    place(o, K('impactSoft_heavy*', r, 0.75), 0, 1.0)
    place(o, thump(75, 0.4, r, tau=0.08), 0, 0.5)
    spl = lp(noise(0.3, r), 1500) * env_exp(0.3, 0.07) * np.clip(lp(noise(0.3, r), 300) * 6, 0, 2)
    place(o, spl, 0.005, 0.5)
    place(o, sizzle(0.8, r) * env_exp(0.8, 0.3), 0.05, 0.35)
    return o


@sfx('boss_waxadmiral_phase', 2.5)
def _(r):
    d = 2.5
    o = Z(d)
    place(o, creak(0.5, r, (30, 80), ((300, 10), (800, 10))), 0, 0.6)
    place(o, K('impactWood_heavy*', r, 0.6), 0.45, 1.0)
    place(o, crackle(0.5, r, 120, (600, 3000)) * env_exp(0.5, 0.15), 0.45, 0.7)
    place(o, roar(1.8, r, 700) * env_ar(1.8, 0.15, 0.6), 0.6, 0.9)
    return o


@sfx('boss_waxadmiral_defeat', 4.0, wet=0.18)
def _(r):
    d = 4.0
    o = Z(d)
    place(o, crackle(2.2, r, 25, (1500, 5000), (0.1, 0.5)) * line(2.2, 1, 0.2), 0, 0.6)
    place(o, lp(bell(740, 2.0, 'brass', 1.0, r, 0.8), 2200), 2.0, 0.45)
    return o


# ================================================================================================ Last Lantern (glass, flame, chain)
def chain(d, r, rate=10):
    out = Z(d)
    for t in np.arange(0, d, 1 / rate):
        place(out, K('metalClick*', r, r.uniform(1.4, 1.8)) * 0.3 + coin_clink(r, r.uniform(2000, 3000), 0.05) * 0.3, t + r.uniform(0, 0.03))
    return out


@sfx('boss_lastlantern_entrance', 3.0)
def _(r):
    d = 3.0
    o = Z(d)
    sw = 0.6 + 0.4 * np.sin(2 * np.pi * 0.6 * tt(3.0)) ** 2
    place(o, roar(3.0, r, 500) * sw, 0, 0.6)
    place(o, crackle(3.0, r, 30, (1200, 4000)), 0, 0.4)
    for t in (0.3, 1.1, 1.9):
        place(o, creak(0.5, r, (40, 30), ((1800, 30), (3500, 25)), jitter=0.1), t, 0.25)
        place(o, chain(0.3, r), t + 0.4, 0.5)
    return o


@sfx('boss_lastlantern_windup', 1.5)
def _(r):
    d = 1.5
    o = Z(d)
    place(o, roar(1.5, r, 900) * line(1.5, 0.2, 1.0, 1.3), 0, 0.8)
    place(o, whoosh(1.4, r, 300, 1800, 1.0, peak=0.85), 0.05, 0.7)
    return o


@sfx('boss_lastlantern_hit', 1.0)
def _(r):
    d = 1.0
    o = Z(d)
    place(o, K('impactGlass_heavy*', r), 0, 0.9)
    place(o, thump(90, 0.3, r, tau=0.06), 0, 0.5)
    for k in range(6):
        place(o, bell(r.uniform(3000, 6000), 0.3, 'glass', 0.08, r), 0.03 + r.uniform(0, 0.4), 0.15)
    place(o, roar(0.5, r, 1200) * env_exp(0.5, 0.15, 0.01), 0.01, 0.6)
    return o


@sfx('boss_lastlantern_phase', 2.5)
def _(r):
    d = 2.5
    o = Z(d)
    for t in (0.0, 0.18, 0.3):
        place(o, crackle(0.08, r, 300, (2500, 7000)), t, 0.6)
    place(o, K('impactGlass_medium*', r, 0.9), 0.32, 0.6)
    place(o, roar(2.1, r, 800) * env_ar(2.1, 0.3, 0.7, 1.5), 0.35, 1.0)
    place(o, crackle(2.0, r, 40, (1000, 3500)), 0.4, 0.4)
    return o


@sfx('boss_lastlantern_defeat', 4.0)
def _(r):
    d = 4.0
    o = Z(d)
    place(o, roar(1.0, r, 500) * line(1.0, 0.8, 0.0), 0, 0.6)
    place(o, sizzle(0.7, r) * env_exp(0.7, 0.25), 0.9, 0.4)
    o2 = Z(2.6)
    for k, t in enumerate(np.cumsum([0.4 * (0.82 ** i) for i in range(8)])):
        place(o2, chain(0.12, r, 20) * (0.8 ** k), t)
    place(o, o2, 1.2, 0.7)
    return o


# ================================================================================================ Marquee Maw (theatre lights, curtain)
def clunk(r):
    return K('impactMetal_heavy*', r, 0.6) * 0.6 + thump(70, 0.3, r, tau=0.05) * 0.6 + K('metalLatch*', r, 0.8) * 0.3


@sfx('boss_marqueemaw_entrance', 3.0, wet=0.18)
def _(r):
    d = 3.0
    o = Z(d)
    for t in (0.0, 0.35, 0.7):
        place(o, clunk(r), t, 0.7)
    place(o, whoosh(1.4, r, 250, 900, 0.8, 'pink', 0.4) + K('cloth*', r, 0.7) * 0.3, 1.0, 0.8)
    murmur = sum(voiced(2.0, 120 + 40 * r.random() + 15 * np.sin(2 * np.pi * r.uniform(0.3, 1) * tt(2.0)), r,
                        ((r.uniform(400, 700), 4), (r.uniform(1000, 1700), 4)), 0.4) for _ in range(5))
    place(o, lp(murmur, 900) * env_ar(2.0, 0.6, 0.6), 1.0, 0.12)
    return o


@sfx('boss_marqueemaw_windup', 1.5)
def _(r):
    d = 1.5
    o = Z(d)
    t = 0.0
    while t < 1.42:
        prog = t / 1.42
        place(o, snare_tap(r, 0.35 + 0.65 * prog ** 1.5), t)
        t += 0.075 - 0.035 * prog
    place(o, snare_tap(r, 1.2, 0.2), 1.42)
    return o


@sfx('boss_marqueemaw_hit', 1.0, wet=0.18)
def _(r):
    d = 1.0
    o = Z(d)
    place(o, K('impactPunch_heavy*', r, 0.7), 0, 0.8)
    place(o, thump(55, 0.8, r, 0.7, 0.2, 0.5, 400), 0, 1.0)
    place(o, K('impactPlank_medium*', r, 0.75), 0.003, 0.5)
    return o


@sfx('boss_marqueemaw_phase', 2.5)
def _(r):
    d = 2.5
    o = Z(d)
    for k, t in enumerate((0.0, 0.3, 0.55, 0.85, 1.1, 1.4)):
        place(o, click(0.01, r, 1500, 1) * 2 + K('impactGlass_light*', r, r.uniform(0.9, 1.2)) * 0.4, t, 0.7)
        place(o, sizzle(0.4, r, 2500) * env_exp(0.4, 0.12), t + 0.01, 0.3)
    return o


@sfx('boss_marqueemaw_defeat', 4.0, wet=0.18)
def _(r):
    d = 4.0
    o = Z(d)
    place(o, whoosh(2.0, r, 220, 700, 0.8, 'pink', 0.5) + K('cloth*', r, 0.6) * 0.25, 0, 0.8)
    for k, t in enumerate((2.0, 2.6, 3.2)):
        place(o, clunk(r) * (1 - 0.2 * k), t, 0.6)
    return o


# ================================================================================================ Mother Umbra (shadow, whispers, fabric)
@sfx('boss_motherumbra_entrance', 3.0, wet=0.2)
def _(r):
    d = 3.0
    o = Z(d)
    place(o, wind(3.0, r, 700, 0.5, 0.4) * env_ar(3.0, 0.7, 0.8), 0, 0.6)
    place(o, whispers(2.6, r, 7, 5) * env_ar(2.6, 0.5, 0.6), 0.3, 0.7)
    place(o, scrape(2.0, r, 400, 0.8, 15, 0.5) * bell_env(2.0, 0.5), 0.6, 0.3)
    return o


@sfx('boss_motherumbra_windup', 1.5, wet=0.2)
def _(r):
    d = 1.5
    o = Z(d)
    place(o, breath(1.5, r, (400, 800, 1600), 3, line(1.5, 0.05, 1.0, 1.5)), 0, 0.8)
    place(o, whispers(1.5, r, 6, 7) * line(1.5, 0.1, 1.0, 1.5), 0, 0.6)
    return o


@sfx('boss_motherumbra_hit', 1.0)
def _(r):
    d = 1.0
    o = Z(d)
    place(o, K('impactSoft_heavy*', r, 0.65), 0, 1.0)
    place(o, thump(50, 0.6, r, 0.5, 0.15, 0.4, 300), 0, 0.8)
    place(o, whoosh(0.3, r, 200, 500, 0.8), 0, 0.3)
    return o


@sfx('boss_motherumbra_phase', 2.5, wet=0.2)
def _(r):
    d = 2.5
    o = Z(d)
    place(o, whispers(1.6, r, 9, 7) * line(1.6, 0.2, 1.0, 1.4), 0, 0.7)
    place(o, wind(1.3, r, 650, 0.6, 1.5) * env_ar(1.3, 0.2, 0.6), 1.2, 1.0)
    return o


@sfx('boss_motherumbra_defeat', 4.0, wet=0.2)
def _(r):
    d = 4.0
    o = Z(d)
    place(o, whispers(2.4, r, 6, 5) * line(2.4, 1.0, 0.0, 0.7), 0, 0.6)
    place(o, breath(1.6, r, (450, 850, 1800), 3, bell_env(1.6, 0.15, 1.3)), 2.0, 0.7)
    return o


# ================================================================================================ Coin Widow (coins, veil, marble)
def pour(d, r, rate=60, env=None):
    out = Z(d)
    for t in sorted(r.uniform(0, d, int(rate * d))):
        place(out, coin_clink(r, None, 0.2) * r.uniform(0.2, 0.7), t)
    return out * (env if env is not None else 1)


@sfx('boss_coinwidow_entrance', 3.0)
def _(r):
    d = 3.0
    o = Z(d)
    place(o, pour(1.8, r, 70, bell_env(1.8, 0.3)), 0, 0.7)
    place(o, K('chips-stack-*', r) * 0.5 + K('handleCoins*', r) * 0.5, 0.2, 0.6)
    place(o, whoosh(1.0, r, 1500, 5000, 1.0) * 0.6 + K('cloth*', r, 1.2) * 0.3, 1.6, 0.6)
    place(o, K('metalClick*', r, 1.2) + click(0.003, r, 3500), 2.6, 0.7)
    return o


@sfx('boss_coinwidow_windup', 1.5)
def _(r):
    return spin(1.4, r, (8, 40), 3300, (0.4, 1.0)) + bp(noise(1.5, r), 3000, 1) * line(1.5, 0.01, 0.06)


@sfx('boss_coinwidow_hit', 1.0)
def _(r):
    d = 1.0
    o = Z(d)
    place(o, K('impactPunch_heavy*', r, 0.8) + K('dropLeather*', r, 0.8) * 0.6, 0, 0.8)
    place(o, pour(0.5, r, 120, env_exp(0.5, 0.15)), 0.01, 0.8)
    place(o, K('chips-collide-*', r), 0.02, 0.6)
    return o


@sfx('boss_coinwidow_phase', 2.5)
def _(r):
    d = 2.5
    o = Z(d)
    place(o, K('impactGlass_heavy*', r, 0.9), 0, 0.8)
    place(o, pour(2.2, r, 110, env_ar(2.2, 0.02, 1.2, 1.2)), 0.03, 1.0)
    place(o, K('chips-handle-*', r), 0.2, 0.4)
    return o


@sfx('boss_coinwidow_defeat', 4.0)
def _(r):
    d = 4.0
    o = Z(d)
    place(o, spin(3.0, r, (22, 8), 3200, (0.9, 0.35)), 0, 0.8)
    wob = Z(0.5)
    for k, t in enumerate(np.cumsum([0.12 * 0.7 ** i for i in range(6)])):
        place(wob, coin_clink(r, 3200, 0.1) * 0.6 ** k, t)
    place(o, wob, 3.0, 0.7)
    place(o, coin_clink(r, 3200, 0.7), 3.35, 0.7)
    return o


# ================================================================================================ Howling Organ (pipe organ, bellows)
def bellows(d, r, pumps=3):
    env = 0.5 + 0.5 * np.sin(np.pi * pumps * np.linspace(0, 1, N(d))) ** 2
    return lp(noise(d, r, 'pink'), 900) * env + creak(d, r, (12, 12), ((200, 8), (500, 8))) * 0.2


@sfx('boss_howlingorgan_entrance', 3.0, wet=0.22)
def _(r):
    d = 3.0
    o = Z(d)
    place(o, bellows(3.0, r, 3), 0, 0.5)
    wheeze = organ([110, 165], 2.4, r, 0.012, 0.4, env=line(2.4, 0, 1, 1.8) * env_ar(2.4, 0.01, 0.4))
    wheeze *= 1 + 0.3 * np.sin(2 * np.pi * 4 * tt(2.4))
    place(o, wheeze, 0.5, 0.7)
    place(o, wind(2.2, r, 900, 0.5, 0.8) * env_ar(2.2, 0.5, 0.5), 0.6, 0.4)
    return o


@sfx('boss_howlingorgan_windup', 1.5, wet=0.2)
def _(r):
    d = 1.5
    o = Z(d)
    f = line(1.5, 180, 420, 1.3)
    pipes = sum(np.sin(2 * np.pi * np.cumsum(f * h) / SR) / h for h in (1, 2, 3, 4)) * env_ar(1.5, 0.1, 0.08)
    place(o, lp(pipes, 3500) + bp(noise(1.5, r), 1200, 0.8) * 0.3, 0, 0.7)
    place(o, bellows(1.5, r, 5), 0, 0.5)
    return o


@sfx('boss_howlingorgan_hit', 1.0, wet=0.22)
def _(r):
    d = 1.0
    o = Z(d)
    place(o, organ([98, 147, 196, 233], 0.45, r, 0.006, 0.2, env=env_ar(0.45, 0.01, 0.2)), 0, 0.8)
    place(o, K('impactWood_heavy*', r, 0.7), 0, 0.6)
    place(o, thump(65, 0.4, r, tau=0.08), 0, 0.5)
    return o


@sfx('boss_howlingorgan_phase', 2.5, wet=0.22)
def _(r):
    d = 2.5
    o = Z(d)
    shriek = organ([523, 554, 784], 1.4, r, 0.02, 0.5, env=env_ar(1.4, 0.05, 0.4))
    place(o, shriek * (1 + 0.4 * np.sin(2 * np.pi * 7 * tt(1.4))), 0, 0.6)
    tear = bp(noise(0.5, r) * np.clip(lp(noise(0.5, r), 50) * 8, 0, 2), 900, 0.8) * env_ar(0.5, 0.01, 0.2)
    place(o, tear, 1.1, 0.8)
    place(o, wind(1.4, r, 800, 0.4, 1.5) * env_ar(1.4, 0.05, 0.8), 1.1, 0.9)
    return o


@sfx('boss_howlingorgan_defeat', 4.0, wet=0.22)
def _(r):
    d = 4.0
    t = tt(3.6)
    f = line(3.6, 220, 70, 0.7)
    pipes = sum(np.sin(2 * np.pi * np.cumsum(f * h) / SR) / h for h in (1, 2, 3)) * line(3.6, 0.9, 0.0, 0.8)
    o = Z(d)
    place(o, lp(pipes, 2500) * 0.5 + lp(noise(3.6, r, 'pink'), 800) * line(3.6, 1, 0, 1.2), 0, 0.8)
    place(o, creak(2.5, r, (14, 6), ((180, 8), (450, 8))) * 0.4, 0.5, 0.6)
    return o


# ================================================================================================ Suitcase Leviathan (latches, leather)
def spin(d, r, rates, f=3400, amp=(0.4, 1.0)):
    """A coin spinning / rolling: one short coin ring per contact, contact rate moving from rates[0] to rates[1]."""
    out = Z(d)
    t, k = 0.0, 0
    while t < d - 0.05:
        p = t / d
        place(out, coin_clink(r, f * r.uniform(0.97, 1.03), 0.06) * (amp[0] + (amp[1] - amp[0]) * p), t)
        t += 1.0 / (rates[0] + (rates[1] - rates[0]) * p)
    return out


def latch(r, ratio=1.0):
    return K('metalLatch*', r, ratio) * 0.7 + K('metalClick*', r, ratio * 1.1) * 0.5


@sfx('boss_suitcaseleviathan_entrance', 3.0)
def _(r):
    d = 3.0
    o = Z(d)
    t = 0.0
    while t < 1.9:
        place(o, latch(r, r.uniform(0.85, 1.2)), t, r.uniform(0.4, 0.8))
        t += r.uniform(0.05, 0.13)
    place(o, creak(2.0, r, (10, 20), ((200, 8), (500, 8), (1100, 8))) * 0.7 + K('handleSmallLeather*', r, 0.8) * 0.4, 0.8, 0.7)
    return o


@sfx('boss_suitcaseleviathan_windup', 1.5)
def _(r):
    d = 1.5
    rate = line(1.5, 35, 70)
    ph = np.cumsum(rate) / SR
    exc = (np.diff(np.floor(ph), prepend=0) > 0).astype(float) * (0.6 + 0.4 * r.random(N(1.5)))
    teeth = sum(reson(exc, f, 6) for f in (1800, 3200, 5200)) * 0.4 + bp(noise(1.5, r), 2500, 1) * 0.05
    return teeth * line(1.5, 0.3, 1.0, 1.3)


@sfx('boss_suitcaseleviathan_hit', 1.0)
def _(r):
    d = 1.0
    o = Z(d)
    place(o, K('impactWood_heavy*', r, 0.75), 0, 1.0)
    place(o, thump(60, 0.5, r, tau=0.1), 0, 0.7)
    place(o, latch(r, 0.8), 0.03, 0.5)
    place(o, K('dropLeather*', r, 0.8), 0.005, 0.4)
    return o


@sfx('boss_suitcaseleviathan_phase', 2.5)
def _(r):
    d = 2.5
    o = Z(d)
    t = 0.0
    for k in range(8):
        place(o, K('impactWood_medium*', r, r.uniform(0.75, 0.95)) + K('dropLeather*', r, 0.9) * 0.4, t, 0.8 - k * 0.05)
        t += 0.17 + r.uniform(0, 0.08)
    place(o, thump(60, 0.6, r), t, 0.7)
    return o


@sfx('boss_suitcaseleviathan_defeat', 4.0)
def _(r):
    d = 4.0
    o = Z(d)
    t = 0.2
    gap = 0.32
    for k in range(7):
        place(o, latch(r, 1.0 + 0.03 * k), t, 0.8 * (0.85 ** k))
        t += gap
        gap *= 1.15
    return o


# ================================================================================================ weapons / pickups (a, b, c genuinely different)
@sfx('sfx_ink_shot_a', 0.5)
def _(r):
    o = Z(0.5)
    place(o, click(0.004, r, 4200, 3), 0, 0.8)
    place(o, whoosh(0.22, r, 900, 3200, 2.0, peak=0.3), 0.0, 0.7)
    for t in (0.1, 0.13, 0.17):
        place(o, drip(r, r.uniform(700, 1100), 0.05), t, 0.4)
    return o


@sfx('sfx_ink_shot_b', 0.5)
def _(r):
    o = Z(0.5)
    place(o, bp(noise(0.05, r) * env_exp(0.05, 0.012), 2600, 2), 0, 0.9)
    spl = lp(noise(0.25, r), 2200) * np.clip(lp(noise(0.25, r), 400) * 8, 0, 2) * env_exp(0.25, 0.06)
    place(o, spl, 0.06, 0.8)
    place(o, whoosh(0.3, r, 500, 1800, 1.5, peak=0.25), 0, 0.5)
    return o


@sfx('sfx_ink_shot_c', 0.5)
def _(r):
    o = Z(0.5)
    sc = bp(noise(0.08, r) * np.clip(lp(noise(0.08, r), 300) * 8, 0, 2), 5000, 3) * env_ar(0.08, 0.01, 0.04)
    place(o, sc, 0, 0.8)
    place(o, whoosh(0.18, r, 1400, 4200, 2.5, peak=0.35), 0.03, 0.6)
    place(o, drip(r, 500, 0.08), 0.16, 0.6)
    return o


@sfx('sfx_paper_shot_a', 0.5)
def _(r):
    o = Z(0.5)
    place(o, K('card-place-*', r, 1.2), 0, 0.8)
    place(o, flutter(0.2, r, 140, (3000, 7000), env=env_exp(0.2, 0.06)), 0.02, 0.5)
    return o


@sfx('sfx_paper_shot_b', 0.5)
def _(r):
    o = Z(0.5)
    snap = hp(noise(0.02, r), 2500) * env_exp(0.02, 0.004, 0.0003)
    place(o, snap, 0, 1.0)
    place(o, whoosh(0.2, r, 1500, 5000, 2.0, peak=0.2), 0.0, 0.5)
    place(o, flutter(0.12, r, 250, (2500, 6000)), 0.04, 0.5)
    return o


@sfx('sfx_paper_shot_c', 0.5)
def _(r):
    o = Z(0.5)
    place(o, K('card-slide-*', r, 1.4), 0, 0.8)
    place(o, click(0.003, r, 3500, 2), 0.0, 0.5)
    return o


def cork(r, f=420, d=0.12, drop=0.35):
    fc = f * (1 + drop * np.exp(-tt(d) / 0.008))
    return svf(noise(d, r) * env_exp(d, 0.012, 0.0004), fc, 7, 'bp') * 3 + osc(fc, d) * env_exp(d, 0.015) * 0.6


@sfx('sfx_toy_gun_a', 0.5)
def _(r):
    o = Z(0.5)
    place(o, cork(r, 420), 0, 1.0)
    place(o, K('impactWood_light*', r, 1.4), 0.004, 0.3)
    return o


@sfx('sfx_toy_gun_b', 0.5)
def _(r):
    o = Z(0.5)
    place(o, click(0.004, r, 2500, 2), 0, 0.6)
    place(o, cork(r, 300, 0.16, 0.6), 0.008, 1.0)
    place(o, whoosh(0.15, r, 800, 2000, 2), 0.02, 0.3)
    return o


@sfx('sfx_toy_gun_c', 0.5)
def _(r):
    o = Z(0.5)
    place(o, cork(r, 560, 0.09, 0.2), 0, 0.9)
    place(o, K('impactWood_light*', r, 1.7), 0.0, 0.5)
    place(o, cork(r, 900, 0.05, 0.1), 0.05, 0.25)
    return o


@sfx('sfx_thrown_a', 0.6)
def _(r):
    o = Z(0.6)
    place(o, whoosh(0.45, r, 700, 2400, 3.5, 'white', 0.5), 0.0, 1.0)
    place(o, K('impactWood_light*', r, 1.3), 0.0, 0.2)
    return o


@sfx('sfx_thrown_b', 0.6)
def _(r):
    o = Z(0.6)
    d = 0.5
    am = 0.6 + 0.4 * np.sin(2 * np.pi * 22 * tt(d))
    place(o, whoosh(d, r, 500, 1600, 2.5, 'pink', 0.4) * am, 0.0, 1.0)
    return o


@sfx('sfx_thrown_c', 0.6)
def _(r):
    o = Z(0.6)
    d = 0.5
    wh = osc(line(d, 1700, 2400), d) * bell_env(d, 0.4, 1.5) * 0.15
    place(o, whoosh(d, r, 1000, 3200, 5.0, 'white', 0.45) + wh, 0.0, 1.0)
    return o


@sfx('sfx_slash_a', 0.5)
def _(r):
    o = Z(0.5)
    place(o, whoosh(0.28, r, 600, 3000, 1.6, 'white', 0.35), 0, 1.0)
    place(o, K('cloth*', r, 1.4), 0.0, 0.4)
    return o


@sfx('sfx_slash_b', 0.5)
def _(r):
    o = Z(0.5)
    place(o, whoosh(0.22, r, 900, 4000, 2.2, 'white', 0.45), 0, 0.9)
    place(o, K('clothBelt*', r, 1.3), 0.02, 0.5)
    return o


@sfx('sfx_slash_c', 0.5)
def _(r):
    o = Z(0.5)
    place(o, whoosh(0.35, r, 400, 1800, 1.2, 'pink', 0.3), 0, 0.8)
    place(o, whoosh(0.15, r, 2000, 5000, 3.0, 'white', 0.5), 0.08, 0.5)
    place(o, K('cloth*', r, 1.1), 0.05, 0.3)
    return o


def drumhead(r, f=150, d=0.4, tau=0.09):
    return thump(f, d, r, 0.25, tau, 0.25, 1200) + bp(noise(d, r), 900, 1.2) * env_exp(d, 0.02) * 0.4


@sfx('sfx_blunt_a', 0.5)
def _(r):
    o = Z(0.5)
    place(o, drumhead(r, 150), 0, 1.0)
    place(o, K('impactSoft_medium*', r, 1.0), 0, 0.5)
    return o


@sfx('sfx_blunt_b', 0.5)
def _(r):
    o = Z(0.5)
    place(o, drumhead(r, 115, 0.45, 0.12), 0, 1.0)
    place(o, K('impactWood_medium*', r, 0.9), 0.002, 0.35)
    return o


@sfx('sfx_blunt_c', 0.5)
def _(r):
    o = Z(0.5)
    place(o, drumhead(r, 190, 0.3, 0.06), 0, 1.0)
    place(o, click(0.006, r, 1800, 2), 0, 0.5)
    place(o, K('impactPunch_medium*', r, 1.2), 0.0, 0.3)
    return o


@sfx('sfx_impact_a', 0.4)
def _(r):
    o = Z(0.4)
    place(o, K('impactSoft_medium*', r, 1.1), 0, 0.9)
    place(o, crackle(0.08, r, 300, (2000, 6000)), 0.005, 0.5)
    return o


@sfx('sfx_impact_b', 0.4)
def _(r):
    o = Z(0.4)
    place(o, K('impactPunch_medium*', r, 1.15), 0, 0.8)
    place(o, K('card-place-*', r, 1.0), 0.002, 0.5)
    return o


@sfx('sfx_impact_c', 0.4)
def _(r):
    o = Z(0.4)
    place(o, thump(160, 0.25, r, 0.3, 0.04, 0.6, 1500), 0, 0.8)
    place(o, crackle(0.12, r, 200, (1500, 5000), (0.3, 1)) + bp(noise(0.12, r), 3000, 1) * env_exp(0.12, 0.03) * 0.5, 0.003, 0.6)
    return o


@sfx('sfx_big_impact_a', 0.8)
def _(r):
    o = Z(0.8)
    place(o, lp(K('impactSoft_heavy*', r, 0.8), 3500), 0, 1.0)
    place(o, thump(60, 0.5, r, tau=0.12), 0, 0.8)
    place(o, flutter(0.4, r, 90, env=env_exp(0.4, 0.15)), 0.08, 0.4)
    return o


@sfx('sfx_big_impact_b', 0.8)
def _(r):
    o = Z(0.8)
    place(o, lp(K('impactPunch_heavy*', r, 0.75), 3500), 0, 0.9)
    place(o, thump(48, 0.6, r, 0.6, 0.15, 0.4, 400), 0, 0.9)
    place(o, K('bookFlip*', r, 1.1), 0.12, 0.35)
    return o


@sfx('sfx_big_impact_c', 0.8)
def _(r):
    o = Z(0.8)
    place(o, K('bookPlace*', r, 0.6), 0, 0.9)
    place(o, thump(70, 0.5, r, 0.4, 0.1, 0.5, 600), 0, 0.8)
    place(o, crackle(0.4, r, 60, (1500, 5000)) * env_exp(0.4, 0.15), 0.1, 0.5)
    return o


@sfx('sfx_coin_pickup_a', 0.4)
def _(r):
    o = Z(0.4)
    place(o, coin_clink(r, 3800, 0.35), 0, 0.9)
    place(o, bell(7600, 0.3, 'glass', 0.1, r) * 0.25, 0.04, 0.6)
    return o


@sfx('sfx_coin_pickup_b', 0.4)
def _(r):
    o = Z(0.4)
    place(o, K('chip-lay-*', r, 1.3), 0, 0.6)
    place(o, coin_clink(r, 3100, 0.3), 0.01, 0.7)
    place(o, coin_clink(r, 4650, 0.25), 0.06, 0.4)
    return o


@sfx('sfx_coin_pickup_c', 0.4)
def _(r):
    o = Z(0.4)
    for k, f in enumerate((2637, 3520, 5274)):
        place(o, bell(f, 0.3, 'glass', 0.08, r), k * 0.035, 0.6 - 0.12 * k)
    place(o, click(0.002, r, 7000), 0, 0.4)
    return o


@sfx('sfx_egg_pickup_a', 0.5)
def _(r):
    o = Z(0.5)
    place(o, bell(1568, 0.5, 'porcelain', 0.18, r), 0, 0.8)
    place(o, bell(2349, 0.45, 'glass', 0.3, r), 0.06, 0.4)
    return o


@sfx('sfx_egg_pickup_b', 0.5)
def _(r):
    o = Z(0.5)
    place(o, K('impactGlass_light*', r, 1.5), 0, 0.4)
    place(o, bell(1319, 0.5, 'porcelain', 0.12, r), 0.004, 0.8)
    place(o, bell(1976, 0.45, 'glass', 0.25, r), 0.09, 0.4)
    return o


@sfx('sfx_egg_pickup_c', 0.5)
def _(r):
    o = Z(0.5)
    place(o, click(0.003, r, 3500, 4), 0, 0.5)
    place(o, bell(1760, 0.5, 'porcelain', 0.1, r), 0, 0.7)
    for k, f in enumerate((2093, 2637)):
        place(o, bell(f, 0.4, 'glass', 0.2, r), 0.07 + 0.06 * k, 0.35)
    return o
