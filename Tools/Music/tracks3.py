"""Fourteen story pieces (music v5 "storybook", third set, 27/10/2026): one piece for each of five bosses (Coin Widow, Chair Porter,
the Door That Stayed Shut, the Marquee Maw, the Last Customer), one for each of the eight secret rooms, and the ending. Same
palette, library and renderer as tracks.py and tracks2.py; the first thirty-two pieces are not touched.

A boss piece plays like the other boss pieces (its melody only in the first 8 s, then base + rhythm). A secret room's piece plays
with its melody while the room's choices are open (the fight inside the room keeps the boss music). The ending plays like a menu.
"""
from compose import *  # noqa: F401,F403
from tracks import sc, MINOR, MAJOR, HARM, _counter, walking  # noqa: F401
from tracks2 import prog, I  # noqa: F401  (tracks2 adds the pit-band and guitar programs to I)


# =============================================================================================== bosses
def boss_coinwidow():
    s = Song('boss_coinwidow', 'Interest Compounds', 'F minor', ['boss_coinwidow'], qbpm=102, qpb=3, bars=40, json_bpm=68, json_bpb=2,
             seed=141, scale=sc(5, HARM), desc='Lace-and-coins waltz: music box and glock motif, harpsichord stabs, coin chimes, a bass drum on one.')
    tl = prog(s, 'Fm | C7 | Fm | C7 | Db | Fm | Bbm | C7', 'Fm | Db | Ab | C7 | Fm | Bbm | Gdim | C7',
              'Bbm | Fm | Db | C7 | Bbm | Fm | Gdim | C7', 'Fm | C7 | Fm | C7 | Db | Ab | Bbm | C7', 'Fm | Db | Bbm | C7 | Fm | Db | C7sus4 | C7')
    motif = 'C5:q F5:q Ab5:q | G5:h E5:q | F5:q Ab5:q C6:q | B5:h.'
    var = 'Db6:q C6:q Ab5:q | G5:h E5:q | F5:h. | r:h.'
    mb = s.part('musicbox', 'melody', 'musicbox', level=-21, send=0.4, tj=10, pan=-0.1)
    gl = s.part('glock', 'melody', 'glock', level=-25, send=0.4, tj=20, pan=0.2)
    for bar in (0, 8, 28):
        mb.play(mel(motif, bar * 3), 80, shape=False)
        gl.play(mel(motif, bar * 3 + 0.04), 68, shape=False, octave=1)
    mb.play(mel(var, 4 * 3), 78, shape=False)
    gl.play(mel(var, 4 * 3), 66, shape=False, octave=1)
    vl = s.part('violin', 'melody', 'violin', level=-27, send=0.3, pan=0.25)
    vl.play(mel(motif, 16 * 3), 66, shape=False)
    cb = s.part('contrabass', 'base', 'contrabass', level=-26, send=0.25, lp=2000)
    roots(cb, tl, 29, [(0, None, 'R', 1.0)], 58)
    vc = s.part('cello', 'base', 'cello', level=-27, send=0.3, lp=3200)
    roots(vc, seg(tl, 3, 8, 40), 41, [(0, 1, 'R', 1.0), (2, 1, '5', 0.8)], 54)
    st = s.part('strings', 'base', 'strings', level=-29, send=0.4, lp=3200)
    pad(st, tl, 48, 65, 3, 40)
    hc = s.part('harpsichord', 'rhythm', 'harpsichord', level=-25, send=0.3, lp=5000, pan=0.15)
    comp(hc, tl, '.xx.xo', 55, 71, 3, 0.5, 54, gate=0.4)
    pz = s.part('pizz', 'rhythm', 'pizz', level=-28, send=0.3, pan=-0.25)
    ostinato(pz, seg(tl, 3, 8, 32), '. . . . 5 . | . . . . . 8', 41, 56, 0.5, gate=0.6)
    bd = s.kit('bassdrum', 'rhythm', 'orch', level=-26, send=0.3, tj=4)
    drums(bd, s, 0, 40, {36: 'X..o..'}, 82)
    cn = s.kit('coins', 'rhythm', 'standard', level=-30, send=0.4, tj=5, pan=0.3)
    drums(cn, s, 0, 40, {TRI_O: '..g...|......|..o...|......', TRI_M: '......|..g...|......|....g.', CAST: '.....g|......|......|..g...'}, 74)
    tr = s.part('tremolo', 'intensity', 'trem', level=-30, send=0.4)
    pad(tr, tl, 60, 74, 2, 40)
    xr = s.part('xyloruns', 'intensity', 'xylo', level=-29, send=0.3, pan=0.3)
    for b in range(3, 40, 4):
        t0 = b * 3 + 1.5
        top = bass_pitch(chord_at(tl, (b + 1) * 3 % (40 * 3)).root, 72)
        for k in range(3):
            xr.n(t0 + k * 0.5, 0.5, top - 3 + k, 60 + 6 * k)
    return s


def boss_chairporter():
    s = Song('boss_chairporter', 'Heavy Load', 'C minor', ['boss_chairporter'], qbpm=88, qpb=4, bars=40, json_bpm=40, json_bpb=2,
             seed=143, scale=sc(0, HARM), desc='Slow weary march: bassoon and horn tune, tuba and contrabass tread, timpani and taiko, a creaking guiro.')
    tl = prog(s, 'Cm | Cm | Ab | Ab | Fm | Fm | G7 | G7', 'Cm | Eb | Ab | Bb | Cm | Fm | G7sus4 | G7',
              'Fm | Fm | Cm | Cm | Ab | Bb | G7sus4 | G7', 'Cm | Bb | Ab | Eb | Fm | Cm/Eb | Dm7b5 | G7', 'Cm | Ab | Fm | G7 | Cm | Ab | G7sus4 | G7')
    motif = 'C4:h Eb4:q G4:q | Ab4:h. G4:q | F4:h Ab4:q C5:q | B4:w'
    var = 'Eb5:h D5:q C5:q | C5:h Ab4:h | C5:h Ab4:q F4:q | D4:w'
    bs = s.part('bassoon', 'melody', 'bassoon', level=-21, send=0.3, tj=10, pan=-0.1)
    hn = s.part('horn', 'melody', 'horn', level=-24, send=0.35, tj=14, pan=0.2)
    for bar in (0, 8, 32):
        bs.play(mel(motif, bar * 4), 80, shape=False)
        hn.play(mel(motif, bar * 4 + 0.04), 66, shape=False, octave=1)
    bs.play(mel(var, 4 * 4), 78, shape=False)
    hn.play(mel(var, 4 * 4), 64, shape=False)
    tu = s.part('tuba', 'base', 'tuba', level=-24, send=0.2, lp=2500)
    ostinato(tu, tl, 'R! . . . . . . . 5 . . . . . . .', 24, 74, 0.5, gate=1.6, accent=1.15)
    cb = s.part('contrabass', 'base', 'contrabass', level=-26, send=0.25, lp=2000)
    roots(cb, tl, 24, [(0, None, 'R', 1.0)], 58)
    st = s.part('strings', 'base', 'strings', level=-29, send=0.45, lp=3000)
    pad(st, tl, 48, 63, 3, 40)
    tp = s.part('timpani', 'rhythm', 'timpani', level=-25, send=0.35)
    ostinato(tp, tl, 'R! . . . R . . . R! . . . R . . .', 36, 68, 0.5, gate=1.4, accent=1.25)
    tk = s.part('taiko', 'rhythm', 'taiko', level=-27, send=0.3)
    drums(tk, s, 8, 40, {41: 'X.......o.......|X.......o.....g.'}, 70)
    gu = s.kit('creak', 'rhythm', 'standard', level=-31, send=0.3, tj=6, pan=0.2)
    drums(gu, s, 0, 40, {GUIRO_L: '................|........o.......', WB_LO: 'x...o...x...o...'}, 70)
    tr = s.part('tremolo', 'intensity', 'trem', level=-30, send=0.4)
    pad(tr, tl, 58, 72, 2, 40)
    bell = s.part('bell', 'intensity', 'tubular', level=-31, send=0.6, pan=0.3)
    for b in range(0, 40, 8):
        bell.n(b * 4, 4, pn('C4'), 46)
    return s


def boss_doorthatstayedshut():
    s = Song('boss_doorthatstayedshut', 'Nobody Home', 'B minor', ['boss_doorthatstayedshut'], qbpm=84, qpb=4, bars=40, json_bpm=38,
             json_bpb=2, seed=145, scale=sc(11, HARM), desc='Knocking that never stops: reed organ and bells over a pedal, wood-block and timpani knocks, low strings.')
    tl = prog(s, 'Bm | Bm | Gmaj7 | Gmaj7 | Em | Em | F#7sus4 | F#7', 'Bm | D | G | A | Bm | Em | F#7sus4 | F#7',
              'G | G | Bm | Bm | Em | F#7 | Bm | Bm', 'Em | Em | Bm | Bm | G | A | F#7sus4 | F#7', 'Bm | G | Em | F#7 | Bm | G | F#7sus4 | F#7')
    motif = 'F#4:h B4:q D5:q | C#5:h. r:q | B4:q D5:q F#5:q G5:q | F#5:h D5:h'
    var = 'E5:q G5:q B5:h | A5:h G5:h | F#5:q A5:q C#6:h | B5:w'
    og = s.part('organ', 'melody', 'reedorgan', level=-21, send=0.35, tj=8, pan=-0.1)
    bl = s.part('bells', 'melody', 'tubular', level=-25, send=0.5, tj=14, pan=0.2)
    for bar in (0, 8, 32):
        og.play(mel(motif, bar * 4), 80, shape=False)
        bl.play(mel(motif, bar * 4 + 0.04), 66, shape=False)
    og.play(mel(var, 4 * 4), 78, shape=False)
    bl.play(mel(var, 4 * 4), 64, shape=False)
    cb = s.part('contrabass', 'base', 'contrabass', level=-26, send=0.25, lp=1800)
    pedal(cb, s, 0, 8, pn('B1'), 62, every=4)
    roots(cb, seg(tl, 4, 8, 40), 23, [(0, None, 'R', 1.0)], 58)
    vc = s.part('cello', 'base', 'cello', level=-27, send=0.3, lp=3000)
    roots(vc, seg(tl, 4, 8, 40), 35, [(0, None, 'R', 1.0)], 54)
    st = s.part('strings', 'base', 'strings', level=-29, send=0.45, lp=3000)
    pad(st, tl, 47, 64, 3, 40)
    ob = s.part('organbed', 'rhythm', 'reedorgan', level=-30, send=0.4, lp=2500)
    comp(ob, seg(tl, 4, 8, 40), 'x...............|x.......o.......', 47, 64, 3, 0.5, 46, gate=3.0)
    kn = s.kit('knock', 'rhythm', 'standard', level=-26, send=0.3, tj=6)
    drums(kn, s, 0, 40, {WB_LO: 'x.x.x...........|x.x.x.....x.x...'}, 84)
    tp = s.part('timpani', 'rhythm', 'timpani', level=-26, send=0.35)
    ostinato(tp, tl, 'R! . . . . . . . R! . . . R . . .', 35, 66, 0.5, gate=1.4, accent=1.25)
    tr = s.part('tremolo', 'intensity', 'trem', level=-30, send=0.4)
    pad(tr, tl, 59, 73, 2, 40)
    bell = s.part('bell', 'intensity', 'tubular', level=-31, send=0.6, pan=0.3)
    for b in range(0, 40, 8):
        bell.n(b * 4, 4, pn('B3'), 46)
    return s


def boss_marqueemaw():
    s = Song('boss_marqueemaw', 'Tonight Only', 'G minor', ['boss_marqueemaw'], qbpm=132, qpb=4, bars=40, json_bpm=66, json_bpb=2,
             seed=147, scale=sc(7, HARM), desc='Pit-band fanfare: horn and muted trumpet tune, tuba oom-pah, snare and brushes, timpani rolls.')
    tl = prog(s, 'Gm | Gm | Eb | Eb | Cm | Cm | D7 | D7', 'Gm | Bb | Eb | F | Gm | Cm | D7sus4 | D7',
              'Eb | Eb | Gm | Gm | Cm | Dm7b5 | D7 | D7', 'Gm | F | Eb | D7 | Gm | Eb | Cm | D7', 'Gm | Eb | Cm | D7 | Gm | Eb | D7sus4 | D7')
    motif = 'D5:q G5:q Bb5:q D6:q | C6:h Bb5:h | G5:q Bb5:q Eb6:h | D6:h. r:q'
    var = 'Bb5:q D6:q G6:q F6:q | Eb6:h C6:h | Eb6:q C6:q G5:q Bb5:q | A5:w'
    hn = s.part('horn', 'melody', 'horn', level=-21, send=0.3, tj=10, pan=-0.1)
    tp_ = s.part('trumpet', 'melody', 'mutedtrp', level=-25, send=0.35, tj=12, pan=0.2)
    for bar in (0, 8, 32):
        hn.play(mel(motif, bar * 4), 82, shape=False)
        tp_.play(mel(motif, bar * 4 + 0.04), 70, shape=False, octave=1)
    hn.play(mel(var, 4 * 4), 80, shape=False)
    tp_.play(mel(var, 4 * 4), 68, shape=False)
    tu = s.part('tuba', 'base', 'tuba', level=-24, send=0.2, lp=2500)
    ostinato(tu, tl, 'R! . . . 5 . . . R! . . . 5 . . 5', 27, 76, 0.5, gate=0.6)
    bs = s.part('upright', 'base', 'upright', level=-27, send=0.15, lp=3200, tj=6)
    ostinato(bs, seg(tl, 4, 8, 40), '. . R . . . 5 . . . R . . . 5 .', 43, 64, 0.5, gate=0.6)
    st = s.part('strings', 'base', 'strings', level=-30, send=0.4, lp=4200)
    pad(st, tl, 50, 67, 3, 38)
    hb = s.part('hornoff', 'rhythm', 'horn', level=-27, send=0.3, pan=0.2)
    comp(hb, seg(tl, 4, 8, 40), '..x...x.', 50, 67, 3, 0.5, 54, gate=0.5)
    pf = s.part('piano', 'rhythm', 'honky', level=-27, send=0.25, lp=4500, pan=-0.2)
    comp(pf, seg(tl, 4, 0, 32), '.x.x.x.x', 53, 69, 3, 0.5, 54, gate=0.4)
    dr = s.kit('snare', 'rhythm', 'brush', level=-24, send=0.2, tj=5)
    drums(dr, s, 0, 8, {KICK: 'x...............', SWIRL: 'o...o...o...o...'}, 88)
    drums(dr, s, 8, 40, {KICK: 'x.......x.......', TAP: '....x.......x...', SWIRL: 'o...o...o...o...', HATP: 'g.g.g.g.g.g.g.g.'}, 94,
         fill={TAP: '....x...x.xxxxxx'})
    tm = s.part('timpani', 'rhythm', 'timpani', level=-28, send=0.35)
    ostinato(tm, seg(tl, 4, 8, 40), 'R! . . . . . R . R! . . . . . . .', 43, 62, 0.5, gate=1.2)
    tr = s.part('trombone', 'intensity', 'trombone', level=-29, send=0.3)
    pad(tr, tl, 46, 62, 2, 40)
    gk = s.part('glock', 'intensity', 'glock', level=-30, send=0.4, pan=0.3)
    sprinkle(gk, s, tl, 0, 40, 79, 93, 48)
    return s


def boss_lastcustomer():
    s = Song('boss_lastcustomer', 'Always Open', 'A minor', ['boss_lastcustomer'], qbpm=80, qpb=4, bars=40, json_bpm=36, json_bpb=2,
             seed=149, scale=sc(9, HARM), desc='Smooth lounge that is slightly wrong: music box and vibes motif, upright bass, brushes, a muted bell.')
    tl = prog(s, 'Am | Am | Dm7 | Dm7 | Bm7b5 | E7 | Am | Am', 'Am | F | Dm7 | G7 | Cmaj7 | Bm7b5 E7 | Am | E7',
              'Dm7 | G7 | Cmaj7 | Fmaj7 | Bm7b5 | E7b9 | Am | Am', 'Am | Am/G | F | E7 | Am | Dm7 | Bm7b5 | E7', 'Am | F | Dm7 | E7 | Am | Dm7 | E7sus4 | E7')
    motif = 'A4:q C5:q E5:h | D5:q C5:q B4:h | D5:q F5:q A5:q C6:q | A5:h F5:h'
    var = 'E5:q A5:q C6:h | B5:q A5:q G#5:h | A5:q F5:q D5:q F5:q | E5:w'
    mb = s.part('musicbox', 'melody', 'musicbox', level=-21, send=0.4, tj=16, pan=-0.1)
    vb = s.part('vibes', 'melody', 'vibes', level=-24, send=0.4, tj=12, pan=0.2)
    for bar in (0, 8, 32):
        mb.play(mel(motif, bar * 4), 78, shape=False)
        vb.play(mel(motif, bar * 4 + 0.04), 66, shape=False)
    mb.play(mel(var, 4 * 4), 76, shape=False)
    vb.play(mel(var, 4 * 4), 64, shape=False)
    bs = s.part('upright', 'base', 'upright', level=-23, send=0.15, lp=3500, tj=7)
    walking(bs, s, tl, 33, 52, 80)
    st = s.part('strings', 'base', 'strings', level=-30, send=0.5, lp=4000)
    pad(st, tl, 52, 70, 3, 38)
    pno = s.part('piano', 'rhythm', 'piano', level=-27, send=0.3, lp=3200, pan=-0.1)
    comp(pno, tl, 'x..x....|..x...x.', 52, 67, 3, 0.5, 50, gate=1.6)
    dr = s.kit('brushes', 'rhythm', 'brush', level=-27, send=0.2, tj=6)
    drums(dr, s, 0, 40, {SWIRL: 'o.......o.......', TAP: '....x.......x...', RIDE: 'o...g.go....g.go', KICK: 'g.......g.......'}, 88,
          fill={TAP: '....x...o.g.x.oo'})
    bl = s.part('bell', 'rhythm', 'tubular', level=-32, send=0.6, pan=0.3)
    for b in range(0, 40, 8):
        bl.n(b * 4 + 3, 4, pn('A4'), 44)
    cl = s.part('clarinet', 'intensity', 'clarinet', level=-26, send=0.35, pan=-0.2)
    guide_line(cl, tl, 58, 72, 54, s.scale, s.rng)
    gl = s.part('glock', 'intensity', 'glock', level=-30, send=0.4, pan=0.3)
    sprinkle(gl, s, tl, 0, 40, 79, 96, 46, n=3, step=0.5, every=4, phase=3)
    return s


# =============================================================================================== secret rooms
def room(pid, title, key, qbpm, qpb, seed, scale, A, A2, B, mA, mA2, mB, lead, second, desc, bass='cello', arp_pat='0 2 4 5 4 2',
         kit=None, extra=None, level=-20):
    """A 32-bar secret-room piece in the shops' shape: tune on A, A2, B, A2; a string bed, a bass, a harp or piano figure, soft
    ticks and an extra layer. kit = {note: pattern} for the soft percussion; extra(s, tl) adds anything of the piece's own."""
    s = Song(pid, title, key, ['secret_' + pid.split('_', 1)[1]], qbpm=qbpm, qpb=qpb, bars=32, seed=seed, scale=scale, desc=desc)
    tl = prog(s, A, A2, B, A2)
    bar = qpb
    ld = s.part('lead', 'melody', lead, level=level, send=0.35, tj=10, pan=-0.1)
    ld.play(mel(mA, 0), 76)
    ld.play(mel(mA2, 8 * bar), 78)
    ld.play(mel(mB, 16 * bar), 78)
    ld.play(mel(mA2, 24 * bar), 78)
    sd = s.part('second', 'melody', second, level=level - 6, send=0.4, pan=0.2)
    sd.play(mel(mB, 16 * bar), 58, octave=1)
    sd.play(mel(mA2, 24 * bar), 58, octave=1)
    st = s.part('strings', 'base', 'strings', level=-26, send=0.45, lp=4500)
    pad(st, tl, 50, 70, 3, 44)
    bs = s.part('bass', 'base', bass, level=-27, send=0.3, lp=3200)
    roots(bs, tl, 36, [(0, None, 'R', 1.0)], 54)
    hp = s.part('figure', 'rhythm', 'harp', level=-24, send=0.4, pan=0.22)
    arp(hp, tl, 50, 80, arp_pat, 0.5, 46, gate=2.2)
    pz = s.part('pizz', 'rhythm', 'pizz', level=-30, send=0.3, pan=-0.22)
    comp(pz, seg(tl, qpb, 8, 32), '.xo' if qpb == 3 else '.x.o', 55, 70, 3, 1.0 if qpb == 3 else 0.5, 44, gate=0.6)
    tk = s.kit('ticks', 'rhythm', 'standard', level=-33, send=0.25, tj=4)
    drums(tk, s, 0, 32, kit or {WB_LO: 'x..' if qpb == 3 else 'x...o...'}, 64)
    ce = s.part('celesta', 'intensity', 'celesta', level=-26, send=0.4)
    guide_line(ce, tl, 72, 88, 54, s.scale, s.rng)
    mb = s.part('musicbox', 'intensity', 'musicbox', level=-29, send=0.45, pan=0.3)
    sprinkle(mb, s, tl, 0, 32, 79, 96, 48, n=3, step=0.5, every=4, phase=3)
    if extra:
        extra(s, tl)
    return s


def secret_lostproperty():
    A = 'D | Bm | G | A7 | D | F#m | G | A7'
    A2 = 'D | Bm | G | A7 | D | Bm | Em7 A7 | D'
    B = 'G | D/F# | Em7 | A7 | Bm | G | Em7 | A7'
    mA = 'F#5:h. | D5:h B4:q | D5:q G5:q B5:q | A5:h. | F#5:h D5:q | C#5:q F#5:q A5:q | B5:q G5:q D5:q | C#5:h.'
    mA2 = 'F#5:h. | D5:h B4:q | D5:q G5:q B5:q | A5:h. | F#5:h D5:q | D5:q F#5:q B5:q | G5:q E5:q C#5:q | D5:h.'
    mB = 'B5:h. | A5:h F#5:q | G5:q B5:q E6:q | C#6:h A5:q | D6:h. | G5:h B5:q | E5:q G5:q D6:q | C#6:h.'
    return room('secret_lostproperty', 'Lost and Found', 'D major', 84, 3, 151, sc(2, MAJOR), A, A2, B, mA, mA2, mB, 'clarinet', 'glock',
                'Gentle 3/4 for the lost-property vault: clarinet and glock over strings, cello and harp.')


def secret_impossibletailor():
    A = 'Bb | G7 | Cm7 | F7 | Bb | D7 | Gm7 | C7 F7'
    A2 = 'Bb | G7 | Cm7 | F7 | Bb | D7 | Gm7 C7 | Bb'
    B = 'Eb | Ebm6 | Bb/D | G7 | Cm7 | F7 | Dm7 | G7'
    mA = 'D5:q F5:q Bb5:h | B5:q D6:q G5:h | Eb5:q G5:q C6:h | A5:q F5:q C5:h | D5:q F5:q Bb5:q D6:q | C#6:q A5:q F#5:h | G5:q Bb5:q D6:h | E5:q G5:q F5:h'
    mA2 = 'D5:q F5:q Bb5:h | B5:q D6:q G5:h | Eb5:q G5:q C6:h | A5:q F5:q C5:h | D5:q F5:q Bb5:q D6:q | C#6:q A5:q F#5:h | G5:q Bb5:q E5:q G5:q | D6:w'
    mB = 'G5:q Bb5:q Eb6:h | Gb5:q Bb5:q Eb6:h | F5:q Bb5:q D6:h | D6:q B5:q G5:h | Eb5:q G5:q C6:h | A5:q C6:q F6:h | F5:q A5:q D6:h | B5:q D6:q G5:h'

    def sew(s, tl):
        wb = s.kit('sewing', 'rhythm', 'standard', level=-32, send=0.15, tj=3, pan=0.3)
        drums(wb, s, 0, 32, {WB_HI: 'xgxgxgxgxgxgxgxg', CLAVES: '..............o.'}, 60)
    return room('secret_impossibletailor', 'Thread and Needle', 'B-flat major', 108, 4, 153, sc(10, MAJOR), A, A2, B, mA, mA2, mB, 'bellpiano',
                'clarinet', 'Bouncy ragtime for the tailor: toy piano and clarinet, a wood-block sewing machine.', bass='upright',
                arp_pat='0 2 4 2 5 4 2 1', extra=sew)


def secret_understudy():
    A = 'Em | C | G | D | Em | C | Am7 | B7'
    A2 = 'Em | C | G | D | Em | Am7 | B7sus4 | B7'
    B = 'C | G/B | Am7 | Em | C | D | G | B7'
    mA = 'B4:q E5:q G5:h | E5:q G5:q C6:h | D5:q G5:q B5:h | A5:q F#5:q D5:h | G5:q B5:q E6:h | E6:q C6:q G5:h | E5:q A5:q C6:h | D#5:q F#5:q B5:h'
    mA2 = 'B4:q E5:q G5:h | E5:q G5:q C6:h | D5:q G5:q B5:h | A5:q F#5:q D5:h | G5:q B5:q E6:h | E5:q A5:q C6:h | F#5:q B5:q E6:h | D#6:h B5:h'
    mB = 'E5:q G5:q C6:h | D5:q G5:q B5:h | C6:q A5:q E5:h | G5:q B5:q E6:h | G5:q C6:q E6:h | F#5:q A5:q D6:h | B4:q D5:q G5:h | F#5:q A5:q D#6:h'
    return room('secret_understudy', 'Waiting in the Wings', 'E minor', 88, 4, 155, sc(4, HARM), A, A2, B, mA, mA2, mB, 'flute', 'clarinet',
                'Mirrored lines for the understudy: flute answered by clarinet over strings and harp.', arp_pat='0 2 4 6 4 2 1 3')


def secret_contrabandtrain():
    A = 'Gm | Gm | Cm | Cm | Gm | Eb | D7 | D7'
    A2 = 'Gm | Gm | Cm | Cm | Gm | Eb | D7sus4 | D7'
    B = 'Bb | Bb | Eb | Eb | Cm | D7 | Gm | D7'
    mA = 'G4:q Bb4:q D5:h | G5:q F5:q D5:h | C5:q Eb5:q G5:h | F5:q Eb5:q C5:h | D5:q G5:q Bb5:q D6:q | C6:h G5:h | A5:q F#5:q D5:h | C5:w'
    mA2 = 'G4:q Bb4:q D5:h | G5:q F5:q D5:h | C5:q Eb5:q G5:h | F5:q Eb5:q C5:h | D5:q G5:q Bb5:q D6:q | C6:h G5:h | G5:q A5:q D6:h | F#5:w'
    mB = 'D5:q F5:q Bb5:h | D6:q Bb5:q F5:h | G5:q Bb5:q Eb6:h | D6:q Bb5:q G5:h | C6:q Eb6:q G5:h | A5:q C6:q F#5:h | G5:q Bb5:q D6:h | A5:q F#5:q C5:h'

    def rails(s, tl):
        br = s.kit('rails', 'rhythm', 'brush', level=-28, send=0.2, tj=5)
        drums(br, s, 0, 32, {SWIRL: 'x.x.x.x.x.x.x.x.', KICK: 'x.......x.......', HATP: 'g.g.g.g.g.g.g.g.'}, 84)
    return room('secret_contrabandtrain', 'Night Express', 'G minor', 100, 4, 157, sc(7, HARM), A, A2, B, mA, mA2, mB, 'accordion', 'clarinet',
                'Steady rails for the contraband train: accordion tune, brushes chugging, upright bass.', bass='upright', extra=rails)


def secret_thirteenthhour():
    A = 'F#m | D | Bm | C#7 | F#m | A | Bm | C#7'
    A2 = 'F#m | D | Bm | C#7 | F#m | A | D C#7 | F#m'
    B = 'A | E | D | C#7 | F#m | Bm | D | C#7sus4'
    mA = 'A5:h. | F#5:h D5:q | D5:q F#5:q B5:q | C#6:h F5:q | A5:q C#6:q F#6:q | E6:h C#6:q | D6:q B5:q F#5:q | F5:h.'
    mA2 = 'A5:h. | F#5:h D5:q | D5:q F#5:q B5:q | C#6:h F5:q | A5:q C#6:q F#6:q | E6:h C#6:q | D6:q F#5:q F5:q | F#5:h.'
    mB = 'C#6:h. | B5:h G#5:q | A5:q F#5:q D5:q | G#5:h F5:q | A5:q F#5:q C#5:q | B5:q D6:q F#6:q | A5:h F#5:q | F#5:h.'

    def chimes(s, tl):
        bl = s.part('chimes', 'rhythm', 'tubular', level=-31, send=0.6, pan=0.3)
        for b in range(0, 32, 4):
            bl.n(b * 3, 3, pn('F#4'), 44)
    return room('secret_thirteenthhour', 'Thirteenth Chime', 'F-sharp minor', 70, 3, 159, sc(6, HARM), A, A2, B, mA, mA2, mB, 'musicbox', 'celesta',
                'Hushed 3/4 for the hour that is not on the clock: music box, celesta, a slow bell and ticking.',
                kit={WB_LO: 'x..', WB_HI: '.g.'}, arp_pat='0 2 4 5 4 2', extra=chimes, level=-19)


def secret_sleepinggarden():
    A = 'Cmaj7 | D/C | Cmaj7 | D/C | Am7 | Fmaj7 | Dm7 | G7sus4'
    A2 = 'Cmaj7 | D/C | Cmaj7 | D/C | Em7 | Am7 | Fmaj7 | G7sus4'
    B = 'Fmaj7 | G/F | Em7 | Am7 | Dm7 | G7 | Cmaj7 | G7sus4'
    mA = 'G5:h. | F#5:h A5:q | E5:h G5:q | F#5:h. | E5:q A5:q C6:q | A5:h F5:q | F5:q A5:q D6:q | D6:h.'
    mA2 = 'G5:h. | F#5:h A5:q | E5:h G5:q | F#5:h. | G5:q B5:q E6:q | C6:h A5:q | A5:q C6:q F6:q | D6:h.'
    mB = 'A5:q C6:q F6:q | D6:q G5:q B5:q | E6:h G5:q | C6:h E6:q | F6:q D6:q A5:q | B5:q D6:q F5:q | E6:h. | D6:h.'
    return room('secret_sleepinggarden', 'Sleepers', 'C lydian', 66, 3, 161, [0, 2, 4, 6, 7, 9, 11], A, A2, B, mA, mA2, mB, 'flute', 'vibes',
                'Drifting 3/4 lullaby for the garden: flute and vibes over harp arpeggios and a string bed.', arp_pat='0 2 4 6 4 2 1 3 5 7 5 3',
                level=-21)


def secret_emptytheatre():
    A = 'Eb | Cm7 | Abmaj7 | Bb7 | Eb | Gm7 | Abmaj7 | Bb7sus4 Bb7'
    A2 = 'Eb | Cm7 | Abmaj7 | Bb7 | Eb | Eb7 | Abmaj7 Bb7 | Eb'
    B = 'Abmaj7 | Gm7 | Cm7 | Fm7 | Bb7 | Eb | Fm7 | Bb7'
    mA = 'G5:q Bb5:q Eb6:h | Eb6:q C6:q G5:h | Eb5:q G5:q C6:h | D6:q F5:q Bb5:h | G5:q Bb5:q Eb6:q G6:q | F6:q D6:q Bb5:h | C6:q Eb6:q G5:h | F5:h D5:h'
    mA2 = 'G5:q Bb5:q Eb6:h | Eb6:q C6:q G5:h | Eb5:q G5:q C6:h | D6:q F5:q Bb5:h | G5:q Bb5:q Eb6:q G6:q | Db6:q Bb5:q G5:h | C6:q Eb6:q D6:q F5:q | Eb6:w'
    mB = 'C6:q Eb6:q Ab5:h | D6:q Bb5:q G5:h | Eb6:q C6:q G5:h | C6:q Ab5:q F5:h | D6:q F6:q Ab5:h | G6:q Bb5:q Eb6:h | C6:q Ab5:q F5:h | F5:q Ab5:q D6:h'
    return room('secret_emptytheatre', 'Empty Seats', 'E-flat major', 78, 4, 163, sc(3, MAJOR), A, A2, B, mA, mA2, mB, 'mutedtrp', 'celesta',
                'Slow ballad for the empty theatre: muted trumpet and celesta, strings, felt-piano warmth.', arp_pat='0 2 4 5 4 2 1 3', level=-21)


def secret_deadletters():
    A = 'Am | F | Dm | E7 | Am | C | Dm | E7'
    A2 = 'Am | F | Dm | E7 | Am | C | Dm E7 | Am'
    B = 'F | C/E | Dm7 | Am | Bm7b5 | E7 | Am | E7sus4'
    mA = 'E5:h. | F5:h A5:q | D5:q F5:q A5:q | G#5:h B5:q | C6:h E5:q | E5:q G5:q C6:q | D5:q F5:q A5:q | B4:h G#4:q'
    mA2 = 'E5:h. | F5:h A5:q | D5:q F5:q A5:q | G#5:h B5:q | C6:h E5:q | E5:q G5:q C6:q | F5:q A5:q G#5:q | A5:h.'
    mB = 'A5:q C6:q F6:q | G5:q C6:q E6:q | F5:q A5:q D6:q | E6:h C6:q | D6:q F6:q B5:q | G#5:q B5:q D6:q | C6:q E6:q A5:q | B5:h.'

    def keys(s, tl):
        ty = s.kit('typewriter', 'rhythm', 'standard', level=-32, send=0.12, tj=3, pan=-0.2)
        drums(ty, s, 0, 32, {WB_HI: 'xgg', CLAVES: '..o|...'}, 62)
    return room('secret_deadletters', 'Return to Sender', 'A minor', 76, 3, 165, sc(9, HARM), A, A2, B, mA, mA2, mB, 'oboe', 'celesta',
                'Faded 3/4 for the dead-letter office: oboe and celesta, a far typewriter, strings and harp.', extra=keys, level=-21)


# =============================================================================================== ending
def ending_dawn():
    s = Song('ending_dawn', 'First Light', 'D major', ['ending'], qbpm=58, qpb=4, bars=32, seed=167, scale=sc(2, MAJOR),
             desc='Slow warm ending: flute and music box over strings and harp, a quiet piano and a soft clock; no drums.')
    A = 'D | A/C# | Bm | G | D | A | Em7 | A7sus4 A7'
    A2 = 'D | A/C# | Bm | G | D | Bm | G A | D'
    B = 'G | D/F# | Em | A | Bm | G | Em7 | A7'
    tl = prog(s, A, A2, B, A2)
    mA = 'F#5:h A5:h | E5:h A5:h | D5:q F#5:q B5:h | B5:q D6:q G5:h | F#5:q A5:q D6:h | C#6:h E5:h | G5:q B5:q E6:h | E6:q C#6:q A5:h'
    mA2 = 'F#5:h A5:h | E5:h A5:h | D5:q F#5:q B5:h | B5:q D6:q G5:h | F#5:q A5:q D6:h | D6:h B5:h | B5:q G5:q E5:q A5:q | D6:w'
    mB = 'B5:q D6:q G6:h | A5:q D6:q F#6:h | G5:q B5:q E6:h | E6:q C#6:q A5:h | D6:q B5:q F#5:h | G5:q B5:q D6:h | G5:q B5:q E6:h | C#6:q E6:q G5:h'
    fl = s.part('flute', 'melody', 'flute', level=-21, send=0.45, pan=-0.1, tj=12)
    fl.play(mel(mA, 0), 72)
    fl.play(mel(mA2, 32), 74)
    fl.play(mel(mB, 64), 76)
    fl.play(mel(mA2, 96), 76)
    mb = s.part('musicbox', 'melody', 'musicbox', level=-25, send=0.5, pan=0.2, tj=14)
    mb.play(mel(mA2, 96), 62, octave=1)
    vl = s.part('violin', 'melody', 'violin', level=-27, send=0.5, pan=0.15)
    vl.play(mel(mB, 64), 58)
    st = s.part('strings', 'base', 'strings', level=-24, send=0.55, lp=4500)
    pad(st, tl, 50, 72, 4, 48)
    vc = s.part('cello', 'base', 'cello', level=-27, send=0.4, lp=3000)
    roots(vc, tl, 38, [(0, None, 'R', 1.0)], 52)
    hp = s.part('harp', 'rhythm', 'harp', level=-23, send=0.5, pan=-0.2)
    arp(hp, tl, 50, 81, '0 2 4 5 4 2 1 3', 0.5, 46, gate=2.8)
    pno = s.part('feltpiano', 'rhythm', 'piano', level=-27, send=0.5, lp=2300, tj=14)
    roots(pno, tl, 38, [(0, 2, 'R', 1.0)], 40)
    clk = s.kit('clock', 'rhythm', 'standard', level=-34, send=0.6, lp=3000, tj=3)
    drums(clk, s, 0, 32, {WB_LO: 'x...o...'}, 60)
    ce = s.part('celesta', 'intensity', 'celesta', level=-27, send=0.5)
    guide_line(ce, tl, 74, 88, 52, s.scale, s.rng)
    sh = s.part('shimmer', 'intensity', 'trem', level=-31, send=0.6, lp=5000)
    pad(sh, tl, 65, 79, 2, 34)
    return s


ALL = [boss_coinwidow, boss_chairporter, boss_doorthatstayedshut, boss_marqueemaw, boss_lastcustomer, secret_lostproperty,
       secret_impossibletailor, secret_understudy, secret_contrabandtrain, secret_thirteenthhour, secret_sleepinggarden,
       secret_emptytheatre, secret_deadletters, ending_dawn]
