"""The 15 Arcane Brigade pieces (music v5, "storybook"): cozy-dark, acoustic, lantern-lit toy shop / dream waiting room.

Stems: base = bass + harmony bed, rhythm = percussion + ostinati, melody = lead (shop / menus / boss intro only),
intensity = an extra layer (heard in the shops). Combat plays base + rhythm only, so combat base + rhythm carry no lead.
"""
from compose import *  # noqa: F401,F403

MINOR = [0, 2, 3, 5, 7, 8, 10]


def sc(root_pc, mode=MINOR):
    return [(root_pc + i) % 12 for i in mode]


MAJOR = [0, 2, 4, 5, 7, 9, 11]
HARM = [0, 2, 3, 5, 7, 8, 11]


# =============================================================================================== menus
MENU_A = 'Dm | Bb | Gm | A7 | Dm | F/C | Gm6/Bb | A'
MENU_A2 = 'Dm | Bb | Gm | A7 | Dm | F/C | A7 | Dm'
MENU_MA = 'A4:q D5:q F5:q | F5:h D5:q | G5:q Bb5:q A5:q | G5:q E5:h | F5:q D5:q A4:q | C5:q F5:q A5:q | G5:q E5:q D5:q | C#5:h.'
MENU_MA2 = 'A4:q D5:q F5:q | F5:h D5:q | G5:q Bb5:q A5:q | G5:q E5:h | F5:q A5:q D6:q | C6:h A5:q | G5:q E5:q C#5:q | D5:h.'


def menu_curtain():
    s = Song('menu_curtain', 'Lantern Waltz', 'D minor', ['menu'], qbpm=72, qpb=3, bars=32, seed=11, scale=sc(2, HARM),
             desc='Tender 3/4 lullaby waltz: music box and celesta over warm strings, felt piano and a grandfather-clock tick.')
    B = 'Bb | F/A | Gm | Dm/F | Gm | Eb | A7sus4 | A7'
    mB = 'F5:q Bb5:q D6:q | C6:h A5:q | Bb5:q A5:q G5:q | A5:h. | G5:q Bb5:q D6:q | Eb6:h Bb5:q | A5:h G5:q | E5:h C#5:q'
    tl = s.tl(' | '.join([MENU_A, MENU_A2, B, MENU_A2]))
    # melody
    box = s.part('musicbox', 'melody', 'musicbox', level=-20, send=0.35, tj=12, pan=0.1)
    box.play(mel(MENU_MA, 0), 78)
    box.play(mel(MENU_MA2, 24), 80)
    box.play(mel(MENU_MA2, 72), 80)
    cel = s.part('celesta', 'melody', 'celesta', level=-22, send=0.35, tj=10, pan=-0.1)
    cel.play(mel(mB, 48), 74)
    cel.play(mel(MENU_MA2, 72), 58, octave=-1)
    # base
    st = s.part('strings', 'base', 'strings', level=-24, send=0.4, lp=5000)
    pad(st, seg(tl, 3, 0, 8), 50, 69, 2, 46)
    pad(st, seg(tl, 3, 8, 32), 50, 74, 4, 54)
    vc = s.part('cello', 'base', 'cello', level=-26, send=0.3, lp=4000, pan=-0.15)
    roots(vc, tl, 38, [(0, None, 'R', 1.0)], 58)
    cb = s.part('contrabass', 'base', 'contrabass', level=-30, send=0.25, lp=2500)
    roots(cb, seg(tl, 3, 16, 32), 28, [(0, None, 'R', 1.0)], 50)
    hp = s.part('harp', 'base', 'harp', level=-28, send=0.4, pan=0.25)
    strum(hp, seg(tl, 3, 8, 32), 50, 79, 5, 50, spread=0.06, ring=3, qpb=3)
    # rhythm
    pno = s.part('feltpiano', 'rhythm', 'piano', level=-25, send=0.35, lp=2400, tj=12)
    roots(pno, seg(tl, 3, 0, 8), 38, [(0, 3, 'R', 1.0), (0, 3, '5', 0.8)], 44)
    roots(pno, seg(tl, 3, 8, 32), 38, [(0, 1, 'R', 1.0)], 48)
    comp(pno, seg(tl, 3, 8, 32), '.xo', 53, 69, 3, 1.0, 46, gate=0.85)
    clk = s.kit('clock', 'rhythm', 'standard', level=-32, send=0.2, tj=4, lp=6000)
    drums(clk, s, 0, 32, {WB_LO: 'xo o', TRI_O: '...|...|...|g..'}, 70)
    # intensity
    cl = s.part('clarinet', 'intensity', 'clarinet', level=-25, send=0.35, pan=-0.2)
    guide_line(cl, tl, 58, 72, 58, s.scale, s.rng)
    gl = s.part('glock', 'intensity', 'glock', level=-30, send=0.4, pan=0.3)
    sprinkle(gl, s, tl, 0, 32, 79, 96, 50, n=3, step=0.5, every=4, phase=3)
    return s


def select_dressing():
    s = Song('select_dressing', 'Pocket Heroes', 'D minor / F major', ['select'], qbpm=84, qpb=3, bars=32, seed=13, scale=sc(2, HARM),
             desc='The menu waltz, lighter: clarinet and flute over harp arpeggios and pizzicato, a warm F-major middle.')
    B = 'F | C/E | Dm | Bb | Gm7 | C7 | F | A7'
    mB = 'A5:q C6:q A5:q | G5:h E5:q | F5:q A5:q F5:q | D5:h. | Bb5:q A5:q G5:q | E5:q G5:q Bb5:q | A5:h F5:q | E5:h C#5:q'
    tl = s.tl(' | '.join([MENU_A, MENU_A2, B, MENU_A2]))
    cl = s.part('clarinet', 'melody', 'clarinet', level=-21, send=0.3, pan=-0.1)
    cl.play(mel(MENU_MA, 0), 76, octave=-1)
    fl = s.part('flute', 'melody', 'flute', level=-21, send=0.32, pan=0.1)
    fl.play(mel(MENU_MA2, 24), 74)
    fl.play(mel(mB, 48), 76)
    fl.play(mel(MENU_MA2, 72), 76)
    gk = s.part('glock_mel', 'melody', 'glock', level=-28, send=0.4, pan=0.25)
    gk.play(mel(MENU_MA2, 72), 52)
    st = s.part('strings', 'base', 'strings', level=-26, send=0.4, lp=5000)
    pad(st, tl, 50, 70, 3, 44)
    pzb = s.part('pizzbass', 'base', 'pizz', level=-25, send=0.25, lp=3000)
    roots(pzb, tl, 38, [(0, 1, 'R', 1.0)], 70)
    hp = s.part('harp', 'rhythm', 'harp', level=-24, send=0.35, pan=0.2)
    arp(hp, tl, 50, 79, '0 2 4 5 4 2 1 3 5 6 5 3', 0.5, 50, gate=2.0)
    pz = s.part('pizzchords', 'rhythm', 'pizz', level=-29, send=0.3, pan=-0.2)
    comp(pz, seg(tl, 3, 8, 32), '.xo', 55, 70, 3, 1.0, 44, gate=0.6)
    clk = s.kit('ticks', 'rhythm', 'standard', level=-34, send=0.2, tj=4)
    drums(clk, s, 0, 32, {CLAVES: 'o..', WB_HI: '.gg'}, 64)
    ce = s.part('celesta', 'intensity', 'celesta', level=-26, send=0.4)
    guide_line(ce, tl, 72, 86, 56, s.scale, s.rng)
    return s


# =============================================================================================== shops
def shop_gold():
    s = Song('shop_gold', 'Brass Bell Counter', 'F major', ['shop_gold'], qbpm=96, qpb=4, bars=32, swing=0.64, seed=17,
             scale=sc(5, MAJOR), desc='Light jazz swing: toy piano tune, clarinet answers, walking upright bass, brushes, pizzicato.')
    A = 'Fmaj7 | Dm7 | Gm7 | C7 | Am7 | D7 | Gm7 | C7'
    A2 = 'Fmaj7 | Dm7 | Gm7 | C7 | Am7 | D7 | Gm7 C7 | F6'
    B = 'Bbmaj7 | Bbm6 | Am7 | D7 | Gm7 | C7 | Am7 D7 | Gm7 C7'
    tl = s.tl(' | '.join([A, A2, B, A2]))
    head = 'C5:q F5:q A5:q G5:e F5:e | E5:h r:q D5:e E5:e | F5:q D5:q Bb4:q r:q | r:w | C5:q E5:q G5:q A5:e G5:e | F#5:h r:q A5:e r:e | '
    mA = head + 'Bb5:q A5:q G5:q F5:q | E5:h r:h'
    mA2 = head + 'Bb5:q A5:e G5:e E5:q C5:q | F5:h. r:q'
    mB = 'D6:q. C6:e A5:q F5:q | Db6:h Bb5:q G5:q | C6:q. A5:e E5:h | F#5:q A5:q C6:h | Bb5:q. A5:e G5:q F5:q | E5:h. r:q | C5:q E5:q F#5:q A5:q | G5:q F5:q E5:q C5:q'
    toy = s.part('toypiano', 'melody', 'bellpiano', level=-20, send=0.3, swing=True, pan=0.1)
    for start, m in ((0, mA), (32, mA2), (64, mB), (96, mA2)):
        toy.play(mel(m, start), 78)
    cl = s.part('clarinet', 'melody', 'clarinet', level=-22, send=0.3, swing=True, pan=-0.25)
    ans1 = 'r:e E4:e G4:e Bb4:e D5:q C5:q'
    ans2 = 'r:q G4:e Bb4:e D5:e C5:e Bb4:e G4:e'
    ans3 = 'r:h. Bb4:e C5:e'
    for bar, a in ((3, ans1), (7, ans2), (11, ans1), (21, ans3), (27, ans1)):
        cl.play(mel(a, bar * 4), 70)
    bs = s.part('upright', 'base', 'upright', level=-23, send=0.15, lp=3500, tj=7)
    walking(bs, s, tl, 29, 50, 84)
    pno = s.part('piano', 'base', 'piano', level=-26, send=0.3, lp=3200, swing=True, pan=-0.1)
    comp(pno, seg(tl, 4, 0, 16), 'x..x....', 52, 67, 3, 0.5, 52, gate=1.6)
    comp(pno, seg(tl, 4, 16, 24), '..x...x.', 52, 67, 3, 0.5, 50, gate=1.4)
    comp(pno, seg(tl, 4, 24, 32), 'x..x....', 52, 67, 3, 0.5, 52, gate=1.6)
    dr = s.kit('brushes', 'rhythm', 'brush', level=-27, send=0.2, swing=True, tj=6)
    drums(dr, s, 0, 32, {SWIRL: 'o.......o.......', TAP: '....x.......x...', RIDE: 'o...g.go....g.go', KICK: 'g.......g.......'}, 92,
          fill={TAP: '....x...o.g.x.oo'})
    pz = s.part('pizz', 'rhythm', 'pizz', level=-28, send=0.3, swing=True, pan=0.3)
    comp(pz, tl, '.......x|...x...x', 60, 76, 3, 0.5, 50, gate=0.5)
    tick = s.kit('tick', 'rhythm', 'standard', level=-35, send=0.15, swing=True, tj=4)
    drums(tick, s, 0, 32, {WB_HI: 'g...g...g...g...'}, 70)
    sp = s.part('strings', 'intensity', 'strings', level=-27, send=0.4, lp=5000)
    pad(sp, tl, 60, 79, 4, 46)
    mb = s.part('musicbox', 'intensity', 'musicbox', level=-27, send=0.4, pan=0.35)
    sprinkle(mb, s, tl, 0, 32, 81, 98, 54, n=4, step=0.25, every=2, phase=1)
    return s


def walking(p, s, tl, lo, hi, vel):
    rng = s.rng
    cur = None
    for i, (t, d, c) in enumerate(tl):
        nxt = tl[(i + 1) % len(tl)][2]
        r = bass_pitch(c.bass, lo)
        if cur is not None and abs(r + 12 - cur) < abs(r - cur) and r + 12 <= hi:
            r += 12
        tgt = bass_pitch(nxt.bass, lo)
        if abs(tgt + 12 - r) < abs(tgt - r) and tgt + 12 <= hi:
            tgt += 12
        beats = int(round(d))
        line = [r]
        if beats >= 4:
            up = tgt >= r
            mid1 = r + (c.off('3') if up else c.off('5') - 12 if r + c.off('5') - 12 >= lo else c.off('5'))
            mid2 = r + c.off('5') if up else scale_step(mid1, -1, s.scale)
            line += [mid1, mid2]
        elif beats >= 2:
            pass
        appr = tgt + (rng.choice([-1, 1]) if beats >= 2 else 0)
        if beats >= 2:
            line.append(appr if lo <= appr <= hi else tgt - 1)
        for k, x in enumerate(line[:beats]):
            while x > hi:
                x -= 12
            while x < lo:
                x += 12
            p.n(t + k, 0.92, x, vel * (1.0 if k % 2 == 0 else 0.86))
        cur = line[-1]


def shop_layaway():
    s = Song('shop_layaway', 'Parlour on Layaway', 'B-flat major', ['shop_gold'], qbpm=92, qpb=4, bars=32, seed=19,
             scale=sc(10, MAJOR), desc='Gentle parlour stride: flute tune with clarinet, upright bass in two, brushes and a shaker.')
    A = 'Bb | Gm | Cm7 | F7 | Dm7 | G7 | Cm7 F7 | Bb F7'
    A2 = 'Bb | Gm | Cm7 | F7 | Dm7 | G7 | Cm7 F7 | Bb'
    B = 'Eb | Edim7 | Bb/F | G7 | C7 | C7 | F7sus4 | F7'
    tl = s.tl(' | '.join([A, A2, B, A2]))
    head = 'D5:e F5:e Bb5:q A5:e G5:e F5:q | G5:q. F5:e D5:h | Eb5:e G5:e C6:q Bb5:e G5:e Eb5:q | F5:h. r:q | F5:e A5:e D6:q C6:e A5:e F5:q | G5:q. F5:e D5:q B4:q | '
    mA = head + 'C5:q Eb5:q A4:q C5:q | Bb4:h r:h'
    mA2 = head + 'Eb5:q G5:q F5:q Eb5:q | D5:h. r:q'
    mB = 'G5:q Bb5:q Eb6:h | D6:q. C#6:e Bb5:h | D6:q C6:q Bb5:q F5:q | B5:h. G5:q | Bb5:q. A5:e G5:q E5:q | C5:h. r:q | Bb4:q C5:q Eb5:q F5:q | A5:h. r:q'
    fl = s.part('flute', 'melody', 'flute', level=-20, send=0.3, pan=0.1)
    fl.play(mel(mA, 0), 76)
    fl.play(mel(mA2, 32), 78)
    fl.play(mel(mA2, 96), 78)
    cl = s.part('clarinet', 'melody', 'clarinet', level=-22, send=0.3, pan=-0.2)
    cl.play(mel(mB, 64), 76, octave=-1)
    cl.play(harmonize_below(mel(mA2, 96), s.scale), 62)
    for bar, a in ((3, 'r:h. Eb5:e C5:e'), (7, 'r:h C5:e A4:e F4:e Eb4:e'), (11, 'r:h. Eb5:e C5:e')):
        cl.play(mel(a, bar * 4), 66)
    bs = s.part('upright', 'base', 'upright', level=-23, send=0.15, lp=3500)
    roots(bs, tl, 34, [(0, 1.8, 'R', 1.0), (2, 1.8, '5', 0.85)], 80)
    st = s.part('strings', 'base', 'strings', level=-29, send=0.4, lp=4500)
    pad(st, seg(tl, 4, 8, 32), 55, 72, 3, 40)
    pno = s.part('piano', 'rhythm', 'piano', level=-25, send=0.3, lp=4200, pan=-0.1)
    comp(pno, tl, '..x...x.', 58, 74, 3, 0.5, 52, gate=1.2)
    dr = s.kit('brushes', 'rhythm', 'brush', level=-28, send=0.2, tj=6)
    drums(dr, s, 0, 32, {TAP: '....o.......o...', KICK: 'g.......g.......', SHAKER: 'o.g.o.g.o.g.o.gg'}, 90,
          fill={TAP: '....o.....g.o.go'})
    mb = s.part('musicbox', 'intensity', 'musicbox', level=-26, send=0.4, pan=0.3)
    guide_line(mb, tl, 74, 88, 56, s.scale, s.rng)
    gl = s.part('glock', 'intensity', 'glock', level=-30, send=0.4, pan=-0.3)
    sprinkle(gl, s, tl, 0, 32, 81, 96, 50, n=3, step=0.5, every=4, phase=3)
    return s


def shop_dream():
    s = Song('shop_dream', 'Lanterns Under Water', 'E-flat major (lydian)', ['shop_dream'], qbpm=80, qpb=3, bars=32, seed=23,
             scale=[3, 5, 7, 9, 10, 0, 2], desc='Floating 3/4: vibraphone and celesta over harp arpeggios and a soft string bed.')
    A = 'Ebmaj7 | F/Eb | Ebmaj7 | F/Eb | Cm7 | Abmaj7 | Fm7 | Bb7sus4'
    A2 = 'Ebmaj7 | F/Eb | Ebmaj7 | F/Eb | Gm7 | Cm7 | Abm6 | Bb7sus4'
    B = 'Abmaj7 | Bb/Ab | Gm7 | Cm7 | Fm7 | Bb7 | Ebmaj7 | Bb7sus4'
    tl = s.tl(' | '.join([A, A2, B, A]))
    mA = 'Bb5:h. | A5:h C6:q | D6:h Bb5:q | A5:h. | G5:q Bb5:q Eb6:q | C6:h G5:q | Ab5:q G5:q F5:q | Eb5:h.'
    mA2 = 'Bb5:h. | A5:h C6:q | D6:h Bb5:q | A5:h. | D6:h Bb5:q | G5:h. | B5:q Ab5:q F5:q | Eb5:h.'
    mB = 'Eb6:q C6:q G5:q | F5:q. Bb5:e D6:q | D6:h. | Eb6:q D6:q Bb5:q | Ab5:h C6:q | D6:q. C6:e Ab5:q | G5:h. | F5:h Eb5:q'
    vb = s.part('vibes', 'melody', 'vibes', level=-20, send=0.45, tj=12)
    vb.play(mel(mA, 0), 74)
    vb.play(mel(mA2, 24), 76)
    vb.play(mel(mB, 48), 78)
    vb.play(mel(mA, 72), 76)
    ce = s.part('celesta', 'melody', 'celesta', level=-26, send=0.45, pan=0.2)
    ce.play(mel(mA, 72), 56, octave=1)
    st = s.part('strings', 'base', 'strings', level=-24, send=0.5, lp=4500)
    pad(st, tl, 51, 75, 4, 48)
    vc = s.part('cello', 'base', 'cello', level=-28, send=0.35, lp=3000)
    roots(vc, tl, 36, [(0, None, 'R', 1.0)], 50)
    hp = s.part('harp', 'rhythm', 'harp', level=-23, send=0.45, pan=-0.2)
    arp(hp, tl, 51, 84, '0 2 4 6 4 2 1 3 5 7 5 3', 0.5, 48, gate=2.5)
    tk = s.kit('ticks', 'rhythm', 'standard', level=-35, send=0.35, tj=5)
    drums(tk, s, 0, 32, {WB_HI: 'g..', TRI_O: 'o..|...'}, 64)
    mb = s.part('musicbox', 'intensity', 'musicbox', level=-25, send=0.5, pan=0.3)
    guide_line(mb, tl, 79, 91, 56, s.scale, s.rng)
    gl = s.part('glock', 'intensity', 'glock', level=-30, send=0.5, pan=-0.3)
    sprinkle(gl, s, tl, 0, 32, 84, 98, 46, n=3, step=0.5, every=4, phase=3)
    return s


# =============================================================================================== combat (no lead in base + rhythm)
def _counter(s, tl, inst, lo, hi, level=-24):
    p = s.part('counter', 'melody', inst, level=level, send=0.35, pan=-0.15)
    guide_line(p, tl, lo, hi, 62, s.scale, s.rng)
    return p


def combat_toys():
    s = Song('combat_toys', 'Paper Corridors', 'A minor', ['combat_early'], qbpm=112, qpb=4, bars=40, seed=31, scale=sc(9),
             desc='Lantern-lit chase: plucked low-string ostinato, bassoon pulse, brushes and a ticking woodblock; no lead.')
    P = ('Am | Am | Fmaj7 | Fmaj7 | Dm7 | Dm7 | Esus4 | E | Am | Am | Fmaj7 | Fmaj7 | Dm7 | Dm7 | Esus4 | E | '
         'F | G | Em7 | Am | Dm7 | G | Cmaj7 | E7 | F | G | Em7 | Am | Dm7 | Bm7b5 | Esus4 | E7 | '
         'Am | Fmaj7 | Dm7 | E7 | Am | Fmaj7 | Bm7b5 | E7')
    tl = s.tl(P)
    vc = s.part('cello', 'base', 'cello', level=-25, send=0.3, lp=3500)
    roots(vc, tl, 36, [(0, None, 'R', 1.0)], 58)
    cb = s.part('contrabass', 'base', 'contrabass', level=-29, send=0.2, lp=2200)
    roots(cb, seg(tl, 4, 8, 32), 28, [(0, None, 'R', 1.0)], 52)
    st = s.part('strings', 'base', 'strings', level=-29, send=0.4, lp=4000)
    pad(st, tl, 52, 69, 3, 40)
    bn = s.part('bassoon', 'base', 'bassoon', level=-26, send=0.25, pan=0.15)
    ostinato(bn, seg(tl, 4, 8, 32), 'R! . R . 5 . R .', 40, 60, 0.5, gate=0.45)
    ostinato(bn, seg(tl, 4, 32, 40), 'R! . . . 5 . . .', 40, 56, 0.5, gate=0.45)
    pz = s.part('pizz', 'rhythm', 'pizz', level=-23, send=0.25, pan=-0.15)
    ostinato(pz, seg(tl, 4, 0, 16), 'R! 8 5 8 3 8 5 8 R! 8 5 8 10 8 5 8', 40, 62, 0.5, gate=0.7)
    ostinato(pz, seg(tl, 4, 16, 32), 'R! . 5 8 . 8 5 . R! . 5 8 10 . 5 .', 40, 64, 0.5, gate=0.7)
    ostinato(pz, seg(tl, 4, 32, 40), 'R! . 5 . 8 . 5 .', 40, 58, 0.5, gate=0.8)
    wb = s.kit('woodblock', 'rhythm', 'standard', level=-31, send=0.15, tj=4, pan=0.2)
    drums(wb, s, 0, 40, {WB_HI: 'x...o...x...o...', WB_LO: '..g...g...g...g.'}, 76)
    dr = s.kit('brushes', 'rhythm', 'brush', level=-26, send=0.2, tj=6)
    drums(dr, s, 0, 8, {SWIRL: 'o.......o.......', TAP: '............o...'}, 88)
    drums(dr, s, 8, 32, {KICK: 'x.....o...x.....', TAP: '....x.......x...', SWIRL: 'o.......o.......', HATP: '..g...g...g...g.'}, 92,
          fill={TAP: '....x...x.o.xoxo'})
    drums(dr, s, 32, 40, {KICK: 'x...............', TAP: '........x.......', SWIRL: 'o...o...o...o...'}, 86)
    ce = s.part('celesta', 'rhythm', 'celesta', level=-30, send=0.45, pan=0.3)
    sprinkle(ce, s, tl, 8, 32, 76, 93, 48)
    _counter(s, tl, 'oboe', 64, 79)
    sh = s.kit('shaker', 'intensity', 'standard', level=-31, send=0.15)
    drums(sh, s, 0, 40, {SHAKER: 'xgogxgogxgogxgog'}, 70)
    mr = s.part('marimba', 'intensity', 'marimba', level=-28, send=0.3, pan=-0.3)
    comp(mr, tl, '.x.x.x.o', 64, 76, 2, 0.5, 52, gate=0.8)
    return s


def combat_spoons():
    s = Song('combat_spoons', 'Cutlery Drawer', 'D minor', ['combat_early'], qbpm=116, qpb=4, bars=40, seed=37, scale=sc(2),
             desc='3+3+2 groove for castanets, claves, marimba and pizzicato over an upright bass; no lead.')
    P = ('Dm | Dm | Bb | C | Dm | Dm | Gm | A7 | Dm | Dm | Bb | C | Dm | Dm | Gm | A7 | '
         'Gm | C | F | Bb | Gm | A | Dm | A7 | Gm | C | F | Bb | Em7b5 | A7 | Dm | A7 | '
         'Bb | Bb | A | A | Gm | Gm | A7sus4 | A7')
    tl = s.tl(P)
    bs = s.part('upright', 'base', 'upright', level=-22, send=0.15, lp=3500, tj=6)
    ostinato(bs, seg(tl, 4, 0, 32), 'R! . . R . . 5 . R! . . R . . 8 5', 28, 82, 0.5, gate=0.8)
    ostinato(bs, seg(tl, 4, 32, 40), 'R! . . . . . 5 .', 28, 78, 0.5, gate=0.9)
    bn = s.part('bassoon', 'base', 'bassoon', level=-27, send=0.3)
    guide_line(bn, seg(tl, 4, 8, 40), 46, 58, 52, s.scale, s.rng, pass_prob=0.3)
    st = s.part('strings', 'base', 'strings', level=-30, send=0.4, lp=4000)
    pad(st, tl, 55, 70, 3, 36)
    mr = s.part('marimba', 'rhythm', 'marimba', level=-24, send=0.25, pan=0.2)
    ostinato(mr, seg(tl, 4, 0, 32), 'R 5 8 5 10 8 5 3 R 5 8 12 10 8 5 8', 50, 56, 0.5, gate=0.9, accent=1.25)
    ostinato(mr, seg(tl, 4, 32, 40), 'R . 5 . 8 . 5 .', 50, 52, 0.5)
    pz = s.part('pizz', 'rhythm', 'pizz', level=-27, send=0.3, pan=-0.25)
    comp(pz, seg(tl, 4, 8, 32), 'x..x..x.', 57, 72, 3, 0.5, 50, gate=0.6)
    pc = s.kit('spoons', 'rhythm', 'standard', level=-27, send=0.15, tj=5)
    drums(pc, s, 0, 8, {CAST: 'x..o.x..o.x.o.g.', CLAVES: 'x.....x.....x...'}, 80)
    drums(pc, s, 8, 32, {CAST: 'x..o.x..o.x.o.g.', CLAVES: 'x.....x.....x...', KICK: 'x.....x.........', SIDE: '............o...'}, 84,
          fill={CAST: 'x..o.x..o.x.xxxx'})
    drums(pc, s, 32, 40, {CLAVES: 'x.....x.....x...', KICK: 'x...............'}, 78)
    _counter(s, tl, 'clarinet', 62, 77)
    tb = s.kit('tamb', 'intensity', 'standard', level=-31)
    drums(tb, s, 0, 40, {TAMB: 'g.g.o.g.g.g.o.g.'}, 70)
    gl = s.part('glock', 'intensity', 'glock', level=-30, send=0.4, pan=0.3)
    comp(gl, tl, '...x...x', 79, 91, 2, 0.5, 46, gate=1.5)
    return s


def combat_clockwork():
    s = Song('combat_clockwork', 'Escapement', 'E minor', ['combat_mid'], qbpm=120, qpb=4, bars=40, seed=41, scale=sc(4),
             desc='Clockwork: tick-tock woodblocks, celesta escapement figures, syncopated cello pizzicato, soft timpani; no lead.')
    P = ('Em | Em/D | Cmaj7 | B7 | Em | Em/D | Am7 | B7 | Em | Em/D | Cmaj7 | B7 | Em | Em/D | Am7 | B7 | '
         'Am | D | Gmaj7 | Em | Am | F#m7b5 | B7sus4 | B7 | Am | D | Gmaj7 | Cmaj7 | Am | F#m7b5 | B7sus4 | B7 | '
         'Cmaj7 | Cmaj7 | Am7 | Am7 | F#m7b5 | F#m7b5 | B7sus4 | B7')
    tl = s.tl(P)
    cb = s.part('contrabass', 'base', 'contrabass', level=-26, send=0.2, lp=2500)
    roots(cb, tl, 28, [(0, None, 'R', 1.0)], 56)
    st = s.part('strings', 'base', 'strings', level=-28, send=0.4, lp=4000)
    pad(st, tl, 52, 67, 3, 40)
    bn = s.part('bassoon', 'base', 'bassoon', level=-27, send=0.25, pan=0.2)
    ostinato(bn, seg(tl, 4, 8, 40), 'R! . . . 5 . . .', 40, 56, 0.5, gate=0.5)
    tk = s.kit('ticktock', 'rhythm', 'standard', level=-28, send=0.15, tj=3)
    drums(tk, s, 0, 40, {WB_HI: 'X...x...X...x...', WB_LO: '..g...g...g...g.'}, 78)
    vc = s.part('cellopizz', 'rhythm', 'pizz', level=-23, send=0.25, pan=-0.2)
    ostinato(vc, seg(tl, 4, 0, 32), 'R! . R 8 . R 5 . R! . R 8 . 5 3 .', 40, 64, 0.5, gate=0.7)
    ostinato(vc, seg(tl, 4, 32, 40), 'R! . . . R . . .', 40, 60, 0.5)
    ce = s.part('celesta', 'rhythm', 'celesta', level=-28, send=0.4, pan=0.3)
    esc = '. . . . . . . . R! 5 8 5 10 5 8 5 . . . . . . . . . . . . . . . .'
    ostinato(ce, seg(tl, 4, 8, 32), esc, 72, 44, 0.25, gate=1.4)
    dr = s.kit('brushes', 'rhythm', 'brush', level=-27, send=0.2, tj=5)
    drums(dr, s, 0, 8, {KICK: 'x.......x.......', SWIRL: 'g...g...g...g...'}, 88)
    drums(dr, s, 8, 32, {KICK: 'x.......x.....o.', TAP: '....o.......o...', SWIRL: 'g...g...g...g...', HATP: '..g...g...g...g.'}, 92,
          fill={TAP: '....o...o.g.o.oo'})
    drums(dr, s, 32, 40, {KICK: 'x...............', SWIRL: 'o.......o.......'}, 84)
    tp = s.part('timpani', 'rhythm', 'timpani', level=-30, send=0.35)
    for b in (0, 8, 16, 24, 32):
        tp.n(b * 4, 2, bass_pitch(chord_at(tl, b * 4).root, 40), 76)
    for b in (16, 32):
        roll(tp, b * 4, bass_pitch(chord_at(tl, b * 4 - 1).root, 40), 2, 30, 70)
    _counter(s, tl, 'oboe', 64, 79)
    gl = s.part('glock', 'intensity', 'glock', level=-30, send=0.4, pan=0.35)
    comp(gl, tl, '.o.o.o.o', 79, 91, 1, 0.5, 44, gate=1.2)
    sh = s.kit('shaker', 'intensity', 'standard', level=-32)
    drums(sh, s, 0, 40, {SHAKER: 'ogxgogxgogxgogxg'}, 70)
    return s


def combat_carousel():
    s = Song('combat_carousel', 'Crooked Carousel', 'G minor', ['combat_mid'], qbpm=118, qpb=4, bars=40, seed=47, scale=sc(7, HARM),
             desc='A turning carousel: rotating harp figure, pizzicato galop bass and off-beat chords, triangle and brushes; no lead.')
    P = ('Gm | D7 | Gm | D7 | Cm | Gm | Am7b5 | D7 | Gm | D7 | Gm | D7 | Cm | Gm | Am7b5 | D7 | '
         'Eb | Bb | Cm | Gm | Eb | F | Bb | D7 | Eb | Bb | Cm | Gm | Cm | Am7b5 | D7sus4 | D7 | '
         'Cm | Gm | Eb | D | Cm | Gm | Am7b5 | D7')
    tl = s.tl(P)
    bs = s.part('pizzbass', 'base', 'upright', level=-22, send=0.15, lp=3500)
    ostinato(bs, seg(tl, 4, 0, 32), 'R! . . . 5 . . . R! . . . -5 . . .', 31, 80, 0.5, gate=0.85)
    ostinato(bs, seg(tl, 4, 32, 40), 'R! . . . . . . . 5 . . . . . . .', 31, 74, 0.5, gate=0.9)
    st = s.part('strings', 'base', 'strings', level=-29, send=0.4, lp=4000)
    pad(st, tl, 53, 69, 3, 38)
    bn = s.part('bassoon', 'base', 'bassoon', level=-28, send=0.3, pan=0.2)
    guide_line(bn, seg(tl, 4, 16, 40), 46, 58, 50, s.scale, s.rng, pass_prob=0.4)
    hp = s.part('harp', 'rhythm', 'harp', level=-24, send=0.35, pan=0.25)
    arp(hp, seg(tl, 4, 0, 32), 55, 79, '0 2 4 5 7 5 4 2 1 3 5 6 8 6 5 3', 0.5, 46, gate=1.8)
    arp(hp, seg(tl, 4, 32, 40), 55, 79, '0 2 4 5 7 . . . . . . . . . . .', 0.5, 44, gate=2.5)
    pz = s.part('pizzchords', 'rhythm', 'pizz', level=-27, send=0.3, pan=-0.25)
    comp(pz, seg(tl, 4, 0, 32), '..x...x...x...o.', 55, 70, 3, 0.25, 50, gate=1.6)
    dr = s.kit('perc', 'rhythm', 'brush', level=-27, send=0.2, tj=5)
    drums(dr, s, 0, 8, {TRI_O: 'o...............', KICK: 'g.......g.......'}, 86)
    drums(dr, s, 8, 32, {TRI_O: 'o...............', KICK: 'x.......o.......', TAP: '..o...o...o...o.', SWIRL: 'g.......g.......'}, 90,
          fill={TAP: '..o...o.o.o.ooxo'})
    drums(dr, s, 32, 40, {TRI_O: 'o.......g.......', KICK: 'x...............'}, 82)
    _counter(s, tl, 'clarinet', 62, 77)
    gl = s.part('glock', 'intensity', 'glock', level=-30, send=0.4)
    sprinkle(gl, s, tl, 0, 40, 79, 94, 48, n=4, step=0.25)
    tb = s.kit('tamb', 'intensity', 'standard', level=-32)
    drums(tb, s, 0, 40, {TAMB: '..g...o...g...o.'}, 70)
    return s


def combat_overtime():
    s = Song('combat_overtime', 'Overtime', 'C minor', ['combat_late'], qbpm=124, qpb=4, bars=40, seed=53, scale=sc(0, HARM),
             desc='Late waves: staccato low-string ostinato, cello drone, soft taiko on 1 and 3, fast woodblock, celesta stabs; no lead.')
    P = ('Cm | Cm | Ab | Ab | Fm | Fm | G7sus4 | G | Cm | Cm | Ab | Ab | Fm | Fm | G7sus4 | G | '
         'Cm | Bb | Ab | G | Fm | Ab | Dm7b5 | G7 | Cm | Bb | Ab | G | Fm | Db | Dm7b5 | G7 | '
         'Db | Db | Cm | Cm | Ab | Fm | G7sus4 | G7')
    tl = s.tl(P)
    vc = s.part('cellodrone', 'base', 'cello', level=-25, send=0.3, lp=3000)
    pedal(vc, s, 0, 16, pn('C2'), 60, every=4)
    roots(vc, seg(tl, 4, 16, 40), 36, [(0, None, 'R', 1.0)], 58)
    cb = s.part('contrabass', 'base', 'contrabass', level=-28, send=0.2, lp=2000)
    roots(cb, tl, 28, [(0, None, 'R', 1.0)], 52)
    st = s.part('strings', 'base', 'strings', level=-29, send=0.4, lp=3000)
    pad(st, tl, 48, 63, 3, 40)
    os_ = s.part('stacstrings', 'rhythm', 'faststr', level=-23, send=0.25, lp=5000)
    ostinato(os_, seg(tl, 4, 0, 32), 'R! R 5 R! R 3 R! 5 R! R 5 R! R 8 7 5', 43, 66, 0.5, gate=0.35, accent=1.3)
    ostinato(os_, seg(tl, 4, 32, 40), 'R! . R . R! . 5 .', 43, 60, 0.5, gate=0.35)
    tk = s.part('taiko', 'rhythm', 'taiko', level=-26, send=0.3)
    drums(tk, s, 0, 40, {41: 'X.......o.......|X.......o.....g.'}, 72)
    wb = s.kit('woodblock', 'rhythm', 'standard', level=-31, send=0.12, tj=3, pan=0.25)
    drums(wb, s, 0, 8, {WB_HI: 'x...o...x...o...'}, 72)
    drums(wb, s, 8, 40, {WB_HI: 'xgogxgogxgogxgog', WB_LO: '................|..............o.'}, 74)
    ce = s.part('celesta', 'rhythm', 'celesta', level=-30, send=0.4, pan=-0.3)
    comp(ce, seg(tl, 4, 8, 32), '...x..........x.|................', 72, 86, 3, 0.5, 48, gate=0.5)
    p = _counter(s, tl, 'clarinet', 55, 70)
    p.level = -25
    tp = s.part('timpani', 'intensity', 'timpani', level=-29, send=0.35)
    for b in range(0, 40, 4):
        tp.n(b * 4, 2, bass_pitch(chord_at(tl, b * 4).root, 41), 72)
    tr = s.part('tremolo', 'intensity', 'trem', level=-30, send=0.4, lp=4500)
    pad(tr, tl, 60, 72, 2, 40)
    return s


def combat_lasttrain():
    s = Song('combat_lasttrain', 'Last Train Home', 'B minor', ['combat_late'], qbpm=128, qpb=4, bars=40, seed=59, scale=sc(11),
             desc='Night train: brushed chugging snare, low-string ostinato, off-beat bassoon, a distant train bell; no lead.')
    P = ('Bm | Bm | G | G | Em | Em | F#7sus4 | F#7 | Bm | Bm | G | G | Em | Em | F#7sus4 | F#7 | '
         'Bm | A | G | F# | Em | G | C | F#7 | Bm | A | G | F# | Em | Em/D | C | F#7 | '
         'G | A | Bm | Bm | Em | C | F#7sus4 | F#7')
    tl = s.tl(P)
    vc = s.part('cello', 'base', 'cello', level=-26, send=0.3, lp=3000)
    roots(vc, tl, 35, [(0, None, 'R', 1.0)], 58)
    st = s.part('strings', 'base', 'strings', level=-28, send=0.4, lp=3500)
    pad(st, tl, 50, 66, 3, 40)
    cb = s.part('contrabass', 'base', 'contrabass', level=-29, send=0.2, lp=2000)
    roots(cb, seg(tl, 4, 8, 32), 28, [(0, None, 'R', 1.0)], 52)
    os_ = s.part('lowstrings', 'rhythm', 'faststr', level=-23, send=0.25, lp=4500)
    ostinato(os_, seg(tl, 4, 0, 32), 'R! . R R . R R . R! . R R . 5 3 .', 40, 64, 0.5, gate=0.4)
    ostinato(os_, seg(tl, 4, 32, 40), 'R! . . . R . . .', 40, 58, 0.5, gate=0.5)
    dr = s.kit('chug', 'rhythm', 'brush', level=-25, send=0.2, tj=4)
    drums(dr, s, 0, 8, {TAP: 'ogogogogogogogog', KICK: 'x.......x.......'}, 80)
    drums(dr, s, 8, 32, {TAP: 'xgogXgogxgogXgog', KICK: 'x.......x.......', SHAKER: '..g...g...g...g.'}, 88,
          fill={TAP: 'xgogXgogxgxgXXXX'})
    drums(dr, s, 32, 40, {TAP: 'o.g.o.g.o.g.o.g.', KICK: 'x...............'}, 80)
    bn = s.part('bassoon', 'rhythm', 'bassoon', level=-28, send=0.3, pan=0.25)
    ostinato(bn, seg(tl, 4, 8, 32), '. 5 . 5 . 5 . 5', 46, 50, 0.5, gate=0.4)
    bell = s.part('bell', 'rhythm', 'tubular', level=-32, send=0.5, pan=-0.3)
    for b in range(0, 40, 8):
        bell.n(b * 4, 4, bass_pitch(chord_at(tl, b * 4).root, 64), 50)
    _counter(s, tl, 'clarinet', 59, 74)
    ce = s.part('celesta', 'intensity', 'celesta', level=-29, send=0.4)
    sprinkle(ce, s, tl, 0, 40, 74, 91, 48)
    tr = s.part('tremolo', 'intensity', 'trem', level=-30, send=0.4, lp=4500)
    pad(tr, tl, 62, 74, 2, 40)
    return s


def endless_neverclose():
    s = Song('endless_neverclose', 'The Shop Never Closes', 'D phrygian minor', ['endless'], qbpm=116, qpb=4, bars=40, seed=61,
             scale=[2, 3, 5, 7, 9, 10, 0], desc='Darker endless loop: low pizzicato, timpani pulse, slow muted ticks, tremolo strings and a bell.')
    P = ('Dm | Eb | Dm | Eb | Bb | Eb | Gm | A7 | Dm | Eb | Dm | C | Bb | Eb/G | Gm | A7 | '
         'Gm | Gm | Eb | Eb | Bb | Bb | A7sus4 | A7 | Gm | Eb | Bb | Dm | Gm | Eb | A7sus4 | A7 | '
         'Dm | Dm | Eb | Eb | Dm | Dm | A7sus4 | A7')
    tl = s.tl(P)
    vc = s.part('cellodrone', 'base', 'cello', level=-25, send=0.35, lp=2500)
    pedal(vc, s, 0, 16, pn('D2'), 58, every=4)
    roots(vc, seg(tl, 4, 16, 32), 36, [(0, None, 'R', 1.0)], 56)
    pedal(vc, s, 32, 40, pn('D2'), 58, every=4)
    cb = s.part('contrabass', 'base', 'contrabass', level=-28, send=0.25, lp=1800)
    roots(cb, tl, 28, [(0, None, 'R', 1.0)], 54)
    tr = s.part('tremlow', 'base', 'trem', level=-28, send=0.45, lp=2200)
    pad(tr, tl, 45, 62, 3, 40)
    pz = s.part('pizz', 'rhythm', 'pizz', level=-24, send=0.3, lp=3500)
    ostinato(pz, seg(tl, 4, 0, 32), 'R! R . R 3 . 5 R R! R . R 3 . -5 R', 38, 60, 0.5, gate=0.75)
    ostinato(pz, seg(tl, 4, 32, 40), 'R! . . R . . 5 .', 38, 56, 0.5)
    tp = s.part('timpani', 'rhythm', 'timpani', level=-26, send=0.35)
    ostinato(tp, tl, 'R! . . . . . R . R! . . . . . . .', 38, 64, 0.5, gate=1.5, accent=1.25)
    wb = s.kit('ticks', 'rhythm', 'standard', level=-32, send=0.2, lp=3500, tj=4)
    drums(wb, s, 0, 40, {WB_LO: 'x...o...x...o...'}, 70)
    dr = s.kit('brushes', 'rhythm', 'brush', level=-28, send=0.2, tj=5)
    drums(dr, s, 8, 32, {KICK: 'x.....x.........', SWIRL: 'o...............', TAP: '............g...'}, 86)
    bell = s.part('bell', 'rhythm', 'tubular', level=-33, send=0.6, pan=0.3)
    for b in range(0, 40, 8):
        bell.n(b * 4, 4, pn('D4'), 46)
    _counter(s, tl, 'clarinet', 55, 69)
    ce = s.part('celesta', 'intensity', 'celesta', level=-30, send=0.5)
    sprinkle(ce, s, tl, 0, 40, 70, 86, 42, every=4)
    return s


def endless_encore():
    s = Song('endless_encore', 'Encore After Hours', 'E minor', ['endless'], qbpm=124, qpb=4, bars=40, seed=67, scale=sc(4, HARM),
             desc='Darker carnival loop: distant accordion off-beats, low strings, muffled bass drum and ratchet; no lead.')
    P = ('Em | C | Am | B7 | Em | C | F#m7b5 | B7 | Em | C | Am | B7 | Em | C | F#m7b5 | B7 | '
         'Am | Em | C | B7 | Am | Em | F | B7 | Am | Em | C | B7 | Am | Em | F | B7 | '
         'C | C | Em | Em | Am | Am | B7sus4 | B7')
    tl = s.tl(P)
    cb = s.part('contrabass', 'base', 'contrabass', level=-25, send=0.25, lp=2200)
    roots(cb, tl, 28, [(0, None, 'R', 1.0)], 58)
    st = s.part('lowstrings', 'base', 'strings', level=-27, send=0.4, lp=3000)
    pad(st, tl, 47, 62, 3, 42)
    org = s.part('organ', 'base', 'reedorgan', level=-31, send=0.5, lp=1600)
    pad(org, tl, 55, 67, 3, 36)
    acc = s.part('accordion', 'rhythm', 'accordion', level=-27, send=0.45, lp=2600, pan=0.25)
    comp(acc, seg(tl, 4, 0, 32), '.x.x.x.x', 55, 69, 3, 0.5, 46, gate=0.45)
    pz = s.part('pizz', 'rhythm', 'pizz', level=-24, send=0.3, lp=3500, pan=-0.2)
    ostinato(pz, seg(tl, 4, 0, 32), 'R! . 5 . R . 5 8', 40, 62, 0.5, gate=0.7)
    ostinato(pz, seg(tl, 4, 32, 40), 'R! . . . 5 . . .', 40, 58, 0.5)
    bd = s.kit('bassdrum', 'rhythm', 'orch', level=-27, send=0.3, tj=4)
    drums(bd, s, 0, 40, {36: 'X.......o.......', TRI_O: '................|................|................|o...............'}, 84)
    rt = s.kit('ratchet', 'rhythm', 'standard', level=-32, send=0.3, tj=4)
    drums(rt, s, 0, 40, {GUIRO_L: '................|................|................|........o.......', WB_HI: '..g...g...g...g.'}, 76)
    p = s.part('counter', 'melody', 'musicbox', level=-25, send=0.45)
    guide_line(p, tl, 76, 88, 60, s.scale, s.rng)
    tp = s.part('timpani', 'intensity', 'timpani', level=-29, send=0.35)
    for b in range(0, 40, 4):
        tp.n(b * 4, 2, bass_pitch(chord_at(tl, b * 4).root, 40), 70)
    gl = s.part('glock', 'intensity', 'glock', level=-31, send=0.45)
    sprinkle(gl, s, tl, 0, 40, 79, 94, 44, every=4)
    return s


# =============================================================================================== bosses (6/8)
def boss_ringmaster():
    s = Song('boss_ringmaster', 'The Ringmaster Bows', 'E minor', ['boss'], qbpm=99, qpb=3, bars=40, json_bpm=66, json_bpb=2,
             seed=71, scale=sc(4, HARM), desc='6/8 barrel-organ menace: organ oom-pah-pah, heavy low strings and bassoon, muffled drum, ratchet; music-box motif.')
    P = ('Em | Am | B7 | Em | Em | Am | B7 | Em | Am | Em | F#m7b5 | B7 | C | Am | B7 | Em | '
         'Em | Am | B7 | Em | C | C | B7 | B7 | Am | Em | Am | B7 | Em | Am | B7 | Em | '
         'C | Am | F#m7b5 | B7 | Em | C | B7sus4 | B7')
    tl = s.tl(P)
    motif = 'E5:e G5:e B5:e A#5:q B5:e | C6:q. B5:e A5:e G5:e | F#5:e A5:e C6:e B5:q. | E5:q. r:q.'
    var = 'A5:e C6:e E6:e D#6:q E6:e | B5:q. G5:e F#5:e E5:e | A5:e C6:e E6:e D#6:q. | B5:q. r:q.'
    mb = s.part('musicbox', 'melody', 'musicbox', level=-21, send=0.35, tj=26, pan=0.15)
    for bar in (0, 4, 16, 28):
        mb.play(mel(motif, bar * 3), 80, shape=False)
    mb.play(mel(var, 24 * 3), 78, shape=False)
    org = s.part('barrelorgan', 'melody', 'reedorgan', level=-22, send=0.35, tj=14, pan=-0.1)
    pic = s.part('pipes', 'melody', 'piccolo', level=-28, send=0.35, tj=14, detune=14, pan=-0.2)
    for bar in (0, 28):
        org.play(mel(motif, bar * 3), 76, shape=False, octave=-1)
        pic.play(mel(motif, bar * 3), 60, shape=False)
    org.play(mel(var, 24 * 3), 74, shape=False, octave=-1)
    cb = s.part('contrabass', 'base', 'contrabass', level=-24, send=0.25, lp=2200)
    roots(cb, tl, 28, [(0, None, 'R', 1.0)], 62)
    vc = s.part('cello', 'base', 'cello', level=-25, send=0.3, lp=3000)
    roots(vc, tl, 40, [(0, 1.4, 'R', 1.1), (1.5, 1.4, '5', 0.85)], 60)
    bn = s.part('bassoon', 'base', 'bassoon', level=-26, send=0.3, pan=0.2)
    ostinato(bn, tl, 'R! . . 5 . . R! . . -5 . 3', 34, 62, 0.5, gate=0.55)
    st = s.part('strings', 'base', 'strings', level=-29, send=0.4, lp=3000)
    pad(st, tl, 47, 64, 3, 40)
    ob = s.part('organbass', 'rhythm', 'reedorgan', level=-26, send=0.3, lp=2500)
    ostinato(ob, tl, 'R! . . 5 . .', 40, 64, 0.5, gate=0.55)
    acc = s.part('accordion', 'rhythm', 'accordion', level=-25, send=0.35, lp=3500, pan=0.2)
    comp(acc, tl, '.xo.xo', 55, 71, 3, 0.5, 54, gate=0.5)
    bd = s.kit('bassdrum', 'rhythm', 'orch', level=-26, send=0.3, tj=4)
    drums(bd, s, 0, 40, {36: 'X..o..'}, 86)
    rt = s.kit('ratchet', 'rhythm', 'standard', level=-30, send=0.3, tj=4)
    drums(rt, s, 0, 40, {GUIRO_L: '......|......|......|...o..', TRI_O: 'o.....|......|......|......', WB_LO: '.....g'}, 78)
    tp = s.part('timpani', 'intensity', 'timpani', level=-28, send=0.35)
    for b in range(0, 40, 4):
        tp.n(b * 3, 1.5, bass_pitch(chord_at(tl, b * 3).root, 40), 76)
        roll(tp, b * 3 + 12, bass_pitch(chord_at(tl, b * 3 + 9).root, 40), 1.5, 28, 64)
    tr = s.part('tremolo', 'intensity', 'trem', level=-30, send=0.4)
    pad(tr, tl, 64, 76, 2, 40)
    return s


def boss_clown():
    s = Song('boss_clown', 'The Painted Grin', 'C minor', ['boss'], qbpm=108, qpb=3, bars=40, json_bpm=72, json_bpb=2,
             seed=73, scale=sc(0, HARM), desc='6/8 crooked clown waltz: accordion and bassoon oom-pah, muffled drum, ratchet; chromatic clarinet/music-box motif.')
    P = ('Cm | G7 | Ab | G7 | Cm | G7 | Ab | G7 | Fm | Cm | Db | G7 | Ab | Fm | Dm7b5 | G7 | '
         'Cm | G7 | Ab | G7 | Fm | Fm | Cm | Cm | Db | Db | G7 | G7 | Cm | G7 | Ab | G7 | '
         'Fm | Cm | Dm7b5 | G7 | Ab | Ab | G7sus4 | G7')
    tl = s.tl(P)
    motif = 'G4:e Ab4:e G4:e F#4:e G4:e Eb5:e | D5:q. B4:e C5:e D5:e | Eb5:e D5:e Eb5:e C5:q. | B4:q. G4:q.'
    var = 'C5:e Db5:e C5:e B4:e C5:e Ab5:e | G5:q. Eb5:e D5:e C5:e | F5:e Eb5:e F5:e Db5:q. | B4:q. D5:q.'
    cl = s.part('clarinet', 'melody', 'clarinet', level=-21, send=0.3, tj=10, pan=-0.1)
    mb = s.part('musicbox', 'melody', 'musicbox', level=-24, send=0.4, tj=30, pan=0.2)
    for bar in (0, 28):
        cl.play(mel(motif, bar * 3), 80, shape=False)
        mb.play(mel(motif, bar * 3 + 0.04), 70, shape=False, octave=1)
    mb.play(mel(motif, 4 * 3), 74, shape=False, octave=1)
    cl.play(mel(var, 8 * 3), 78, shape=False)
    xy = s.part('xylo', 'melody', 'xylo', level=-27, send=0.3, pan=0.3)
    xy.play(mel(motif, 16 * 3), 66, shape=False, octave=1)
    bn = s.part('bassoon', 'base', 'bassoon', level=-24, send=0.25)
    ostinato(bn, tl, 'R! . . 5 . .', 34, 70, 0.5, gate=0.5)
    cb = s.part('contrabass', 'base', 'contrabass', level=-27, send=0.25, lp=2000)
    roots(cb, tl, 28, [(0, None, 'R', 1.0)], 56)
    st = s.part('strings', 'base', 'strings', level=-29, send=0.4, lp=3200)
    pad(st, tl, 48, 65, 3, 40)
    acc = s.part('accordion', 'rhythm', 'accordion', level=-24, send=0.35, lp=3500, pan=0.2)
    comp(acc, tl, '.xx.xo', 55, 70, 3, 0.5, 52, gate=0.45)
    pz = s.part('pizz', 'rhythm', 'pizz', level=-29, send=0.3, pan=-0.25)
    ostinato(pz, seg(tl, 3, 8, 32), '. . . . . 8 | . . . . 5 8', 48, 54, 0.5, gate=0.6)
    bd = s.kit('bassdrum', 'rhythm', 'orch', level=-26, send=0.3, tj=4)
    drums(bd, s, 0, 40, {36: 'X..o..'}, 84)
    fx = s.kit('slapstick', 'rhythm', 'standard', level=-30, send=0.3, tj=5)
    drums(fx, s, 0, 40, {GUIRO_L: '......|......|......|...o..', WB_HI: '.....o|......|..g..o|......', TRI_O: 'o.....|......|......|......'}, 78)
    xr = s.part('xyloruns', 'intensity', 'xylo', level=-29, send=0.3, pan=0.3)
    for b in range(3, 40, 4):
        t0 = b * 3 + 1.5
        top = bass_pitch(chord_at(tl, (b + 1) * 3 % (40 * 3)).root, 72)
        for k in range(3):
            xr.n(t0 + k * 0.5, 0.5, top - 3 + k, 60 + 6 * k)
    tr = s.part('tremolo', 'intensity', 'trem', level=-30, send=0.4)
    pad(tr, tl, 60, 74, 2, 40)
    return s


# =============================================================================================== the Waiting Room hub
def hub_waitingroom():
    s = Song('hub_waitingroom', 'The Waiting Room', 'F major / D minor', ['hub'], qbpm=78, qpb=4, bars=32, seed=29, scale=sc(5, MAJOR),
             desc='Calm 4/4 waiting-room theme: slow felt piano, soft clarinet, celesta, a distant clock, a music-box quote of the menu waltz.')
    A = 'F | C/E | Dm7 | Bbmaj7 | Gm7 | Am7 | Bbmaj7 | C7sus4'
    A2 = 'F | C/E | Dm7 | Bbmaj7 | Gm7 | C7 | F | F7'
    B = 'Bbmaj7 | A7 | Dm | Dm/C | Gm7 | C7 | Am7 | Dm7'
    C = 'Dm | Bb | Gm | A7 | Dm | Bbmaj7 | Gm7 | C7sus4'
    tl = s.tl(' | '.join([A, A2, B, C]))
    head = 'A4:h C5:q F5:q | E5:h. D5:q | C5:h A4:h | D5:w | '
    mA = head + 'Bb4:q D5:q F5:h | E5:h. C5:q | D5:h F5:q A5:q | G5:w'
    mA2 = head + 'Bb4:q D5:q G5:q F5:q | E5:h. G4:q | A4:w | r:h Eb5:q C5:q'
    mB = 'D5:h. F5:q | E5:h C#5:h | D5:q. E5:e F5:h | A5:w | Bb5:h A5:q G5:q | E5:h G5:h | C5:h E5:h | F5:w'
    quote = 'A4:q D5:q F5:h | F5:h. D5:q | G5:q Bb5:q A5:h | G5:q E5:h.'
    tail = 'F5:q A5:q D6:h | D6:h. A5:q | Bb5:q A5:q G5:q F5:q | G5:w'
    cl = s.part('clarinet', 'melody', 'clarinet', level=-21, send=0.45, pan=-0.1, tj=12)
    cl.play(mel(mA, 0), 70)
    cl.play(mel(mA2, 32), 72)
    cl.play(mel(mB, 64), 74)
    mb = s.part('musicbox', 'melody', 'musicbox', level=-22, send=0.5, pan=0.2, tj=14)
    mb.play(mel(quote, 96), 74)
    ce = s.part('celesta', 'melody', 'celesta', level=-24, send=0.5, pan=0.15)
    ce.play(mel(tail, 112), 64)
    sprinkle(ce, s, seg(tl, 4, 0, 24), 0, 24, 76, 91, 46, n=3, step=0.5, every=4, phase=3)
    st = s.part('strings', 'base', 'strings', level=-25, send=0.55, lp=4500)
    pad(st, tl, 50, 72, 4, 46)
    vc = s.part('cello', 'base', 'cello', level=-28, send=0.4, lp=3000)
    roots(vc, tl, 36, [(0, None, 'R', 1.0)], 52)
    pno = s.part('feltpiano', 'rhythm', 'piano', level=-23, send=0.5, lp=2300, tj=14)
    roots(pno, tl, 36, [(0, 2, 'R', 1.0)], 46)
    arp(pno, tl, 53, 77, '. 1 2 3 . 2 3 4 . 1 2 4 . 3 2 1', 0.5, 40, gate=3.0)
    clk = s.kit('clock', 'rhythm', 'standard', level=-33, send=0.6, lp=3000, tj=3)
    drums(clk, s, 0, 32, {WB_LO: 'x...o...x...o...'}, 64)
    sh = s.part('shimmer', 'intensity', 'trem', level=-30, send=0.6, lp=5000)
    pad(sh, tl, 65, 79, 2, 36)
    return s


ALL = [menu_curtain, select_dressing, shop_gold, shop_layaway, shop_dream, combat_toys, combat_spoons, combat_clockwork,
       combat_carousel, combat_overtime, combat_lasttrain, endless_neverclose, endless_encore, boss_ringmaster, boss_clown, hub_waitingroom]
