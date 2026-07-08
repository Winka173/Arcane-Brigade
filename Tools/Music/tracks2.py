"""Sixteen more Arcane Brigade pieces (music v5 "storybook", second set, 26/10/2026): the same palette, rules and renderer as
tracks.py, new tunes. They do not touch the first sixteen. Three menus, the Glasshouse and the Theatre (until now borrowing),
one more fight for each of the older four maps, two more boss pieces, two shops and one endless loop.

Stems follow tracks.py: base = bass + harmony bed, rhythm = percussion + ostinati, melody = lead (menus, shops and a boss's
intro), intensity = an extra layer (heard in the shops). Fights and endless play base + rhythm only, so they carry no lead there.
"""
from compose import *  # noqa: F401,F403
from tracks import sc, MINOR, MAJOR, HARM, _counter, walking  # noqa: F401

# a pit band for the Theatre and a nylon guitar for the shop: more General MIDI programs of GeneralUser GS
I.update(horn=(60, 0), trombone=(57, 0), tuba=(58, 0), mutedtrp=(59, 0), nylon=(24, 0), harpsichord=(6, 0))


def prog(s, *chunks):
    """Joins 8-bar chunks into the timeline and checks the song has exactly s.bars bars."""
    text = ' | '.join(chunks)
    n = len([b for b in text.split('|') if b.strip()])
    assert n == s.bars, (s.id, n, s.bars)
    return s.tl(text)


# =============================================================================================== menus
def menu_moth():
    s = Song('menu_moth', 'Moth Lullaby', 'G major', ['menu'], qbpm=66, qpb=4, bars=32, seed=101, scale=sc(7, MAJOR),
             desc='Slow 4/4 lullaby: music box and harp over a string bed, felt piano and a cello; a Em middle.')
    A = 'G | Em7 | Cmaj7 | D7sus4 D7 | G | Bm7 | Cmaj7 | Am7 D7'
    A2 = 'G | Em7 | Cmaj7 | D7sus4 D7 | G | Bm7 | Cmaj7 D7 | G'
    B = 'Em | C | G/B | D | Em | Am7 | Cmaj7 | D7sus4 D7'
    tl = prog(s, A, A2, B, A2)
    head = 'B4:h D5:q G5:q | G5:h E5:h | E5:q G5:q C6:h | B5:q A5:q F#5:h | '
    mA = head + 'D5:q G5:q B5:h | A5:h F#5:h | G5:q E5:q C5:q E5:q | D5:h r:h'
    mA2 = head + 'D5:q G5:q B5:h | C6:h A5:h | G5:q E5:q C5:q E5:q | G5:w'
    mB = 'E5:q G5:q B5:h | C6:q B5:q G5:h | D5:q G5:q B5:q D6:q | A5:q F#5:q D5:h | G5:q B5:q E6:h | E6:q C6:q A5:h | G5:q E5:q C5:h | D5:q G5:q A5:q B5:q'
    box = s.part('musicbox', 'melody', 'musicbox', level=-20, send=0.35, tj=12, pan=0.1)
    box.play(mel(mA, 0), 76)
    box.play(mel(mA2, 32), 78)
    box.play(mel(mA2, 96), 78)
    hm = s.part('harpmel', 'melody', 'harp', level=-25, send=0.4, pan=-0.15)
    hm.play(mel(mB, 64), 72)
    cel = s.part('celesta', 'melody', 'celesta', level=-27, send=0.4, pan=0.2)
    cel.play(mel(mA2, 96), 56, octave=1)
    st = s.part('strings', 'base', 'strings', level=-25, send=0.4, lp=5000)
    pad(st, seg(tl, 4, 0, 8), 50, 69, 3, 44)
    pad(st, seg(tl, 4, 8, 32), 50, 72, 4, 52)
    vc = s.part('cello', 'base', 'cello', level=-26, send=0.3, lp=4000, pan=-0.15)
    roots(vc, tl, 38, [(0, None, 'R', 1.0)], 56)
    cb = s.part('contrabass', 'base', 'contrabass', level=-30, send=0.25, lp=2500)
    roots(cb, seg(tl, 4, 16, 32), 28, [(0, None, 'R', 1.0)], 48)
    hp = s.part('harp', 'rhythm', 'harp', level=-25, send=0.4, pan=0.25)
    arp(hp, tl, 50, 79, '0 2 4 5 4 2 1 3', 0.5, 46, gate=2.0)
    pno = s.part('feltpiano', 'rhythm', 'piano', level=-27, send=0.35, lp=2400, tj=12)
    roots(pno, tl, 38, [(0, 2, 'R', 1.0), (2, 2, '5', 0.8)], 42)
    clk = s.kit('clock', 'rhythm', 'standard', level=-33, send=0.2, tj=4, lp=6000)
    drums(clk, s, 0, 32, {WB_LO: 'x...o...', TRI_O: '........|....g...'}, 68)
    cl = s.part('clarinet', 'intensity', 'clarinet', level=-26, send=0.35, pan=-0.2)
    guide_line(cl, tl, 58, 72, 56, s.scale, s.rng)
    gl = s.part('glock', 'intensity', 'glock', level=-30, send=0.4, pan=0.3)
    sprinkle(gl, s, tl, 0, 32, 79, 96, 48, n=3, step=0.5, every=4, phase=3)
    return s


def select_lanternroll():
    s = Song('select_lanternroll', 'Choosing Hour', 'C major', ['select'], qbpm=92, qpb=4, bars=32, seed=103, scale=sc(0, MAJOR),
             desc='Bright 4/4 stroll for the hero select: flute and marimba over pizzicato bass, harp and a ticking clave.')
    A = 'C | Am | F | G | C | Em | F | G7'
    A2 = 'C | Am | F | G | C | E7 | Am F | G7sus4 G7'
    B = 'F | Dm | Bb | G7 | C | Am | Dm7 | G7'
    tl = prog(s, A, A2, B, A2)
    mA = 'E5:q G5:q C6:h | A5:q C6:q E6:h | C6:q A5:q F5:q A5:q | G5:h D5:h | E5:q G5:q C6:q E6:q | B5:h G5:h | A5:q F5:q C6:q A5:q | B5:h D6:h'
    mA2 = 'E5:q G5:q C6:h | A5:q C6:q E6:h | C6:q A5:q F5:q A5:q | G5:h D5:h | E5:q G5:q C6:q E6:q | G#5:h B5:h | A5:h C6:h | D6:h B5:h'
    mB = 'A5:q C6:q F6:h | D6:q F6:q A5:h | Bb5:q D6:q F6:h | D6:q B5:q G5:h | E6:q C6:q G5:h | C6:q E6:q A5:h | F5:q A5:q C6:q D6:q | B5:w'
    fl = s.part('flute', 'melody', 'flute', level=-21, send=0.3, pan=0.1)
    fl.play(mel(mA, 0), 76)
    fl.play(mel(mA2, 32), 76)
    fl.play(mel(mB, 64), 78)
    fl.play(mel(mA2, 96), 78)
    mr = s.part('marimba_mel', 'melody', 'marimba', level=-26, send=0.3, pan=-0.2)
    mr.play(mel(mA2, 96), 60, octave=-1)
    st = s.part('strings', 'base', 'strings', level=-27, send=0.4, lp=5000)
    pad(st, tl, 52, 72, 3, 42)
    pzb = s.part('pizzbass', 'base', 'pizz', level=-24, send=0.25, lp=3200)
    roots(pzb, tl, 36, [(0, 1, 'R', 1.0), (2, 1, '5', 0.8)], 68)
    hp = s.part('harp', 'rhythm', 'harp', level=-25, send=0.35, pan=0.25)
    arp(hp, tl, 52, 81, '0 2 4 2 5 4 2 1', 0.5, 48, gate=1.8)
    pz = s.part('pizzchords', 'rhythm', 'pizz', level=-29, send=0.3, pan=-0.25)
    comp(pz, seg(tl, 4, 8, 32), '.x.o', 55, 70, 3, 1.0, 46, gate=0.6)
    tk = s.kit('ticks', 'rhythm', 'standard', level=-34, send=0.2, tj=4)
    drums(tk, s, 0, 32, {CLAVES: 'x.o.....', WB_HI: '..g...g.'}, 66)
    ce = s.part('celesta', 'intensity', 'celesta', level=-27, send=0.4)
    guide_line(ce, tl, 72, 88, 54, s.scale, s.rng)
    return s


def hub_corridor():
    s = Song('hub_corridor', 'The Long Corridor', 'B-flat major / G minor', ['hub'], qbpm=66, qpb=3, bars=32, seed=105,
             scale=sc(7, HARM), desc='Hushed 3/4 hall theme: oboe and celesta over cello and harp, a clock far down the corridor.')
    A = 'Gm | Eb | Bb | F | Gm | Cm7 | D7 | Gm'
    A2 = 'Gm | Eb | Bb | F | Cm | Gm/Bb | D7sus4 | D7'
    B = 'Eb | Bb/D | Cm7 | F7 | Dm7 | Gm | Eb | D7'
    tl = prog(s, A, A2, B, A)
    mA = 'D5:h. | Eb5:h G5:q | F5:h. | A5:h C6:q | Bb5:h. | G5:h Eb5:q | F#5:h A5:q | G5:h.'
    mA2 = 'D5:h. | Eb5:h G5:q | F5:h. | A5:h C6:q | G5:h Eb5:q | D5:h Bb4:q | A5:h G5:q | F#5:h.'
    mB = 'Eb5:q G5:q Bb5:q | D6:h F5:q | Eb5:h G5:q | A5:h. | F5:q A5:q D6:q | D6:h Bb5:q | G5:h Bb5:q | A5:h F#5:q'
    ob = s.part('oboe', 'melody', 'oboe', level=-21, send=0.45, pan=-0.1, tj=12)
    ob.play(mel(mA, 0), 70)
    ob.play(mel(mA2, 24), 72)
    ob.play(mel(mB, 48), 74)
    ob.play(mel(mA, 72), 70)
    ce = s.part('celesta', 'melody', 'celesta', level=-26, send=0.5, pan=0.2)
    ce.play(mel(mB, 48), 56, octave=1)
    sprinkle(ce, s, seg(tl, 3, 0, 16), 0, 16, 76, 91, 44, n=3, step=0.5, every=4, phase=3)
    st = s.part('strings', 'base', 'strings', level=-25, send=0.55, lp=4500)
    pad(st, tl, 50, 72, 4, 46)
    vc = s.part('cello', 'base', 'cello', level=-27, send=0.4, lp=3000)
    roots(vc, tl, 36, [(0, None, 'R', 1.0)], 52)
    hp = s.part('harp', 'rhythm', 'harp', level=-24, send=0.5, pan=0.2)
    arp(hp, tl, 53, 81, '0 2 4 5 4 2', 0.5, 42, gate=2.5)
    clk = s.kit('clock', 'rhythm', 'standard', level=-33, send=0.6, lp=3000, tj=3)
    drums(clk, s, 0, 32, {WB_LO: 'x..'}, 62)
    sh = s.part('shimmer', 'intensity', 'trem', level=-30, send=0.6, lp=5000)
    pad(sh, tl, 65, 79, 2, 34)
    return s


# =============================================================================================== fights (no lead in base + rhythm)
def combat_queue():
    s = Song('combat_queue', 'Number Called', 'E minor', ['combat_early', 'combat_map_waitingroom'], qbpm=118, qpb=4, bars=40, seed=111,
             scale=sc(4), desc='Waiting-room shuffle: bassoon and plucked low strings, brushes, a ticket-punch clave; no lead.')
    tl = prog(s, 'Em | Em | C | D | Em | Em | Am | B7', 'Em | Em | C | D | Em | Em | Am | B7',
              'C | D | Em | G | Am | B7 | Em | Em', 'C | D | Bm | Em | Am | D | B7 | B7', 'Em | C | Am | B7 | Em | C | B7sus4 | B7')
    vc = s.part('cello', 'base', 'cello', level=-25, send=0.3, lp=3500)
    roots(vc, tl, 40, [(0, None, 'R', 1.0)], 58)
    cb = s.part('contrabass', 'base', 'contrabass', level=-29, send=0.2, lp=2200)
    roots(cb, seg(tl, 4, 8, 32), 28, [(0, None, 'R', 1.0)], 52)
    st = s.part('strings', 'base', 'strings', level=-29, send=0.4, lp=4000)
    pad(st, tl, 52, 69, 3, 40)
    bn = s.part('bassoon', 'base', 'bassoon', level=-26, send=0.25, pan=0.15)
    ostinato(bn, seg(tl, 4, 8, 32), 'R! . R . 5 . R .', 40, 60, 0.5, gate=0.45)
    ostinato(bn, seg(tl, 4, 32, 40), 'R! . . . 5 . . .', 40, 56, 0.5, gate=0.45)
    pz = s.part('pizz', 'rhythm', 'pizz', level=-23, send=0.25, pan=-0.15)
    ostinato(pz, seg(tl, 4, 0, 16), 'R! . 3 5 . 5 3 . R! . 3 5 8 5 3 .', 40, 62, 0.5, gate=0.7)
    ostinato(pz, seg(tl, 4, 16, 32), 'R! 5 . 8 5 . 3 . R! 5 . 8 10 . 5 .', 40, 64, 0.5, gate=0.7)
    ostinato(pz, seg(tl, 4, 32, 40), 'R! . 5 . 8 . 5 .', 40, 58, 0.5, gate=0.8)
    cl = s.kit('punch', 'rhythm', 'standard', level=-30, send=0.15, tj=4, pan=0.2)
    drums(cl, s, 0, 40, {CLAVES: 'x..o..x...o.....', WB_HI: '....g.......g...'}, 76)
    dr = s.kit('brushes', 'rhythm', 'brush', level=-26, send=0.2, tj=6)
    drums(dr, s, 0, 8, {SWIRL: 'o.......o.......', TAP: '............o...'}, 88)
    drums(dr, s, 8, 32, {KICK: 'x.......x.o.....', TAP: '....x.......x...', SWIRL: 'o.......o.......', HATP: '..g...g...g...g.'}, 92,
          fill={TAP: '....x...x.oxoxo.'})
    drums(dr, s, 32, 40, {KICK: 'x...............', TAP: '........x.......', SWIRL: 'o...o...o...o...'}, 86)
    ce = s.part('celesta', 'rhythm', 'celesta', level=-30, send=0.45, pan=0.3)
    sprinkle(ce, s, tl, 8, 32, 76, 93, 46)
    _counter(s, tl, 'oboe', 64, 79)
    sh = s.kit('shaker', 'intensity', 'standard', level=-31, send=0.15)
    drums(sh, s, 0, 40, {SHAKER: 'xgogxgogxgogxgog'}, 70)
    vb = s.part('vibes', 'intensity', 'vibes', level=-28, send=0.3, pan=-0.3)
    comp(vb, tl, '..x...x.', 64, 76, 2, 0.5, 50, gate=1.2)
    return s


def combat_airmail():
    s = Song('combat_airmail', 'Air Mail', 'G major', ['combat_mid', 'combat_map_postoffice'], qbpm=126, qpb=4, bars=40, seed=113,
             scale=sc(7, MAJOR), desc='Sorting-room chug: upright bass, flute staccato, brushed snare train, tambourine and glock; no lead.')
    tl = prog(s, 'G | G | Em | C | G | G | Am7 | D', 'G | G | Em | C | G | G | Am7 | D',
              'C | D | Bm7 | Em | C | D | G | D7', 'C | D | Bm7 | Em | Am7 | D7 | G | D7', 'G | Em | C | D | G | Em | C D | D7')
    bs = s.part('upright', 'base', 'upright', level=-23, send=0.15, lp=3500, tj=6)
    ostinato(bs, seg(tl, 4, 0, 32), 'R! . . R . 5 . R R! . . R . 5 . 8', 31, 80, 0.5, gate=0.75)
    ostinato(bs, seg(tl, 4, 32, 40), 'R! . . . . . 5 .', 31, 76, 0.5, gate=0.9)
    st = s.part('strings', 'base', 'strings', level=-30, send=0.4, lp=4200)
    pad(st, tl, 55, 72, 3, 38)
    bn = s.part('bassoon', 'base', 'bassoon', level=-28, send=0.25)
    guide_line(bn, seg(tl, 4, 8, 40), 46, 58, 52, s.scale, s.rng, pass_prob=0.4)
    fl = s.part('fluteStaccato', 'rhythm', 'flute', level=-27, send=0.25, pan=0.2)
    ostinato(fl, seg(tl, 4, 0, 32), 'R . 5 . 8 . 5 . R . 5 . 10 . 8 .', 67, 56, 0.5, gate=0.3)
    ostinato(fl, seg(tl, 4, 32, 40), 'R . . . 5 . . .', 67, 52, 0.5, gate=0.4)
    dr = s.kit('brushes', 'rhythm', 'brush', level=-25, send=0.2, tj=5)
    drums(dr, s, 0, 8, {SWIRL: 'o.o.o.o.o.o.o.o.', KICK: 'x...............'}, 86)
    drums(dr, s, 8, 32, {SWIRL: 'o.o.o.o.o.o.o.o.', KICK: 'x.......x.......', TAP: '....x.......x...', HATP: 'g.g.g.g.g.g.g.g.'}, 92,
          fill={TAP: 'x.x.x.x.x.xxxxxx'})
    drums(dr, s, 32, 40, {KICK: 'x...............', TAP: '........x.......', SWIRL: 'o...o...o...o...'}, 86)
    pz = s.part('pizz', 'rhythm', 'pizz', level=-28, send=0.3, pan=-0.25)
    comp(pz, seg(tl, 4, 8, 32), '.x.x.x.o', 57, 72, 3, 0.5, 50, gate=0.5)
    _counter(s, tl, 'clarinet', 62, 77)
    tb = s.kit('tamb', 'intensity', 'standard', level=-31)
    drums(tb, s, 0, 40, {TAMB: 'g.g.o.g.g.g.o.g.'}, 70)
    gl = s.part('glock', 'intensity', 'glock', level=-30, send=0.4, pan=0.3)
    comp(gl, tl, '...x...x', 79, 91, 2, 0.5, 46, gate=1.5)
    return s


def combat_wick():
    s = Song('combat_wick', 'Wick Trimmer', 'F-sharp minor', ['combat_late', 'combat_map_lamphall'], qbpm=122, qpb=4, bars=40, seed=115,
             scale=sc(6), desc='Late lamplit chase: plucked harp ostinato, accordion offbeats, low bassoon, soft taiko; no lead.')
    tl = prog(s, 'F#m | F#m | D | D | Bm | Bm | C#7sus4 | C#7', 'F#m | A | D | E | F#m | D | Bm7 | C#7',
              'A | E | F#m | D | A | E | D | C#7sus4', 'F#m | F#m | D | E | Bm | Bm | C#7sus4 | C#7', 'F#m | D | Bm | C#7 | F#m | D | C#7sus4 | C#7')
    vc = s.part('cellodrone', 'base', 'cello', level=-25, send=0.3, lp=3000)
    pedal(vc, s, 0, 16, pn('F#2'), 60, every=4)
    roots(vc, seg(tl, 4, 16, 40), 42, [(0, None, 'R', 1.0)], 58)
    cb = s.part('contrabass', 'base', 'contrabass', level=-28, send=0.2, lp=2000)
    roots(cb, tl, 30, [(0, None, 'R', 1.0)], 52)
    bn = s.part('bassoon', 'base', 'bassoon', level=-27, send=0.25, pan=0.1)
    ostinato(bn, seg(tl, 4, 8, 40), 'R! . . R . . 5 .', 42, 58, 0.5, gate=0.5)
    st = s.part('strings', 'base', 'strings', level=-29, send=0.4, lp=3200)
    pad(st, tl, 49, 66, 3, 40)
    hp = s.part('harpost', 'rhythm', 'harp', level=-24, send=0.25, pan=-0.2)
    ostinato(hp, seg(tl, 4, 0, 32), 'R! 5 8 5 10 8 5 3', 54, 62, 0.5, gate=0.8, accent=1.25)
    ostinato(hp, seg(tl, 4, 32, 40), 'R! . 5 . 8 . 5 .', 54, 58, 0.5, gate=1.0)
    ac = s.part('accordion', 'rhythm', 'accordion', level=-28, send=0.3, lp=3500, pan=0.25)
    comp(ac, seg(tl, 4, 8, 32), '..x...x.', 54, 69, 3, 0.5, 48, gate=0.5)
    tk = s.part('taiko', 'rhythm', 'taiko', level=-27, send=0.3)
    drums(tk, s, 0, 40, {41: 'X.......o.......|X.......o.....g.'}, 70)
    wb = s.kit('woodblock', 'rhythm', 'standard', level=-31, send=0.12, tj=3, pan=0.25)
    drums(wb, s, 0, 8, {WB_HI: 'x...o...x...o...'}, 72)
    drums(wb, s, 8, 40, {WB_HI: 'xgogxgogxgogxgog', WB_LO: '................|..............o.'}, 74)
    gk = s.part('glock', 'rhythm', 'glock', level=-31, send=0.4, pan=0.3)
    comp(gk, seg(tl, 4, 16, 32), '...x............', 78, 90, 2, 0.5, 46, gate=1.0)
    p = _counter(s, tl, 'clarinet', 54, 69)
    p.level = -25
    tp = s.part('timpani', 'intensity', 'timpani', level=-29, send=0.35)
    for b in range(0, 40, 4):
        tp.n(b * 4, 2, bass_pitch(chord_at(tl, b * 4).root, 42), 70)
    tr = s.part('tremolo', 'intensity', 'trem', level=-30, send=0.4, lp=4500)
    pad(tr, tl, 60, 72, 2, 40)
    return s


def combat_cardindex():
    s = Song('combat_cardindex', 'Card Index', 'B minor', ['combat_mid', 'combat_map_archivevault'], qbpm=114, qpb=4, bars=40, seed=117,
             scale=sc(11), desc='Archive typewriter groove: marimba and celesta ostinato, upright bass, clave and wood-block keys; no lead.')
    tl = prog(s, 'Bm | Bm | G | G | Em7 | Em7 | F#7sus4 | F#7', 'Bm | Bm | G | A | Bm | D | Em7 | F#7',
              'G | A | Bm | F#m | G | A | F#7sus4 | F#7', 'Bm | G | Em7 | F#7 | Bm | G | Em7 | A7', 'Bm | Bm | G | A | Em7 | G | F#7sus4 | F#7')
    bs = s.part('upright', 'base', 'upright', level=-23, send=0.15, lp=3200, tj=6)
    ostinato(bs, seg(tl, 4, 0, 32), 'R! . . R . . 5 . R! . . R . 5 . 8', 35, 80, 0.5, gate=0.7)
    ostinato(bs, seg(tl, 4, 32, 40), 'R! . . . . . 5 .', 35, 76, 0.5, gate=0.9)
    vc = s.part('cellodrone', 'base', 'cello', level=-27, send=0.3, lp=2800)
    pedal(vc, s, 0, 16, pn('B1'), 58, every=4)
    st = s.part('strings', 'base', 'strings', level=-30, send=0.4, lp=4000)
    pad(st, tl, 50, 68, 3, 38)
    mr = s.part('marimba', 'rhythm', 'marimba', level=-24, send=0.25, pan=0.2)
    ostinato(mr, seg(tl, 4, 0, 32), 'R 5 8 5 R 5 10 5 R 5 8 12 10 8 5 3', 59, 54, 0.5, gate=0.85, accent=1.2)
    ostinato(mr, seg(tl, 4, 32, 40), 'R . 5 . 8 . 5 .', 59, 50, 0.5)
    ce = s.part('celesta', 'rhythm', 'celesta', level=-29, send=0.4, pan=-0.3)
    sprinkle(ce, s, tl, 8, 32, 76, 93, 46)
    ty = s.kit('typewriter', 'rhythm', 'standard', level=-29, send=0.12, tj=3, pan=-0.2)
    drums(ty, s, 0, 8, {CLAVES: 'x..x..x.x.......', WB_HI: '....g...g.......'}, 74)
    drums(ty, s, 8, 40, {CLAVES: 'x..x..x.x..x..x.', WB_HI: 'g.g.g.g.g.g.g.g.', WB_LO: '................|...............o'}, 76)
    dr = s.kit('brushes', 'rhythm', 'brush', level=-28, send=0.2, tj=6)
    drums(dr, s, 8, 40, {KICK: 'x.......x.......', SWIRL: 'o...o...o...o...', TAP: '....x.......x...'}, 88)
    _counter(s, tl, 'clarinet', 59, 74)
    tb = s.kit('tamb', 'intensity', 'standard', level=-31)
    drums(tb, s, 0, 40, {TAMB: 'g.g.o.g.g.g.o.g.'}, 68)
    gl = s.part('glock', 'intensity', 'glock', level=-30, send=0.4, pan=0.3)
    comp(gl, tl, '...x...x', 79, 91, 2, 0.5, 46, gate=1.5)
    return s


def combat_pollen():
    s = Song('combat_pollen', 'Pollen Count', 'F major', ['combat_mid', 'combat_map_glasshouse'], qbpm=120, qpb=4, bars=40, seed=119,
             scale=sc(5, MAJOR), desc='Greenhouse bounce: harp and marimba arpeggios over an upright bass, shaker and pizzicato; no lead.')
    tl = prog(s, 'F | F | Dm | Dm | Bb | Bb | C | C', 'F | Am | Bb | C | Dm | Bb | Gm7 | C7',
              'F | F/A | Bb | Gm7 | F | Dm | Bb | C7sus4', 'Dm | Dm | Bb | F | Gm7 | Gm7 | C7 | C7', 'F | Am | Bb | C | F | Bb | C7sus4 | C7')
    bs = s.part('upright', 'base', 'upright', level=-22, send=0.15, lp=3500, tj=6)
    ostinato(bs, seg(tl, 4, 0, 32), 'R! . . R . . 5 . R! . . R . 5 . 8', 29, 80, 0.5, gate=0.75)
    ostinato(bs, seg(tl, 4, 32, 40), 'R! . . . . . 5 .', 29, 76, 0.5, gate=0.9)
    st = s.part('strings', 'base', 'strings', level=-29, send=0.4, lp=4500)
    pad(st, tl, 53, 72, 3, 38)
    cl = s.part('clarinetbase', 'base', 'clarinet', level=-29, send=0.3, pan=-0.1)
    guide_line(cl, seg(tl, 4, 8, 40), 52, 64, 50, s.scale, s.rng, pass_prob=0.4)
    hp = s.part('harp', 'rhythm', 'harp', level=-23, send=0.3, pan=-0.25)
    arp(hp, seg(tl, 4, 0, 32), 53, 84, '0 2 4 2 5 4 2 1', 0.5, 56, gate=1.6)
    arp(hp, seg(tl, 4, 32, 40), 53, 84, '0 . 4 . 5 . 4 .', 0.5, 50, gate=1.8)
    mr = s.part('marimba', 'rhythm', 'marimba', level=-26, send=0.25, pan=0.25)
    ostinato(mr, seg(tl, 4, 8, 32), 'R . 5 . 8 . 5 . R . 5 . 10 . 8 .', 53, 52, 0.5, gate=0.6)
    pz = s.part('pizz', 'rhythm', 'pizz', level=-28, send=0.3, pan=0.1)
    comp(pz, seg(tl, 4, 8, 32), '.x.x.x.o', 57, 72, 3, 0.5, 48, gate=0.5)
    dr = s.kit('brushes', 'rhythm', 'brush', level=-27, send=0.2, tj=5)
    drums(dr, s, 0, 8, {SWIRL: 'o.......o.......', KICK: 'x...............'}, 86)
    drums(dr, s, 8, 32, {KICK: 'x.......x.o.....', TAP: '....x.......x...', SWIRL: 'o...o...o...o...', HATP: 'g.g.g.g.g.g.g.g.'}, 90,
          fill={TAP: '....x...x.oxoxo.'})
    drums(dr, s, 32, 40, {KICK: 'x...............', TAP: '........x.......', SWIRL: 'o...o...o...o...'}, 84)
    sh = s.kit('shaker', 'rhythm', 'standard', level=-31, send=0.15)
    drums(sh, s, 8, 40, {SHAKER: 'xgogxgogxgogxgog'}, 66)
    _counter(s, tl, 'flute', 67, 83)
    tb = s.kit('tamb', 'intensity', 'standard', level=-31)
    drums(tb, s, 0, 40, {TAMB: 'g.g.o.g.g.g.o.g.'}, 68)
    gl = s.part('glock', 'intensity', 'glock', level=-30, send=0.4, pan=0.3)
    comp(gl, tl, '...x...x', 81, 93, 2, 0.5, 46, gate=1.5)
    return s


def combat_rain():
    s = Song('combat_rain', 'Rain on Glass', 'G minor', ['combat_late', 'combat_map_glasshouse'], qbpm=126, qpb=4, bars=40, seed=121,
             scale=sc(7, HARM), desc='Late glass-house storm: sixteenth-note celesta and vibes patter, tremolo strings, soft taiko; no lead.')
    tl = prog(s, 'Gm | Gm | Eb | Eb | Cm | Cm | D | D', 'Gm | Bb | Eb | F | Gm | Eb | Cm7 | D7',
              'Cm | D | Gm | Eb | Cm | D | Eb | D7sus4', 'Gm | Gm | Bb | Bb | Eb | Eb | D7sus4 | D7', 'Gm | Eb | Cm | D | Gm | Eb | D7sus4 | D7')
    vc = s.part('cellodrone', 'base', 'cello', level=-25, send=0.3, lp=3000)
    pedal(vc, s, 0, 16, pn('G1'), 60, every=4)
    roots(vc, seg(tl, 4, 16, 40), 43, [(0, None, 'R', 1.0)], 58)
    cb = s.part('contrabass', 'base', 'contrabass', level=-28, send=0.2, lp=2000)
    roots(cb, tl, 31, [(0, None, 'R', 1.0)], 52)
    tr = s.part('tremolo', 'base', 'trem', level=-29, send=0.45, lp=3500)
    pad(tr, tl, 50, 67, 3, 40)
    ce = s.part('celesta', 'rhythm', 'celesta', level=-26, send=0.4, pan=0.25)
    ostinato(ce, seg(tl, 4, 0, 32), 'R! . 5 . 8 5 . 5 R . 5 8 . 8 5 .', 67, 54, 0.25, gate=1.2, accent=1.2)
    ostinato(ce, seg(tl, 4, 32, 40), 'R! . . . 5 . . .', 67, 50, 0.25, gate=2.0)
    vb = s.part('vibes', 'rhythm', 'vibes', level=-28, send=0.4, pan=-0.25)
    ostinato(vb, seg(tl, 4, 8, 32), '. . 5 . . 8 . . . 5 . . 10 . . 5', 55, 52, 0.25, gate=1.5)
    tk = s.part('taiko', 'rhythm', 'taiko', level=-27, send=0.3)
    drums(tk, s, 0, 40, {41: 'X.......o.......|X.....g.o.......'}, 68)
    dr = s.kit('brushes', 'rhythm', 'brush', level=-28, send=0.2, tj=5)
    drums(dr, s, 8, 40, {SWIRL: 'o...o...o...o...', TAP: '....x.......x...', HATP: 'g.g.g.g.g.g.g.g.'}, 86)
    wb = s.kit('woodblock', 'rhythm', 'standard', level=-32, send=0.12, tj=3, pan=0.25)
    drums(wb, s, 8, 40, {WB_HI: 'xgogxgogxgogxgog'}, 70)
    p = _counter(s, tl, 'oboe', 62, 77)
    p.level = -25
    tp = s.part('timpani', 'intensity', 'timpani', level=-29, send=0.35)
    for b in range(0, 40, 4):
        tp.n(b * 4, 2, bass_pitch(chord_at(tl, b * 4).root, 43), 70)
    gl = s.part('glock', 'intensity', 'glock', level=-31, send=0.4, pan=0.3)
    comp(gl, tl, '...x...x', 79, 91, 2, 0.5, 44, gate=1.5)
    return s


def combat_housedown():
    s = Song('combat_housedown', 'House Lights Down', 'E-flat major', ['combat_mid', 'combat_map_theatre'], qbpm=118, qpb=4, bars=40, seed=123,
             scale=sc(3, MAJOR), desc='Pit-band stomp: tuba and upright, horn offbeats, snare and brushes, glock sparkle; no lead.')
    tl = prog(s, 'Eb | Eb | Cm | Cm | Ab | Ab | Bb | Bb', 'Eb | Gm | Ab | Bb | Cm | Ab | Fm7 | Bb7',
              'Eb | Eb/G | Ab | Fm7 | Eb | Cm | Ab | Bb7sus4', 'Cm | Bb | Ab | Bb | Cm | Fm | Ab | Bb7', 'Eb | Cm | Ab | Bb | Eb | Ab | Bb7sus4 | Bb7')
    tu = s.part('tuba', 'base', 'tuba', level=-24, send=0.2, lp=2500)
    ostinato(tu, seg(tl, 4, 0, 32), 'R! . . . 5 . . . R! . . . 5 . . 5', 27, 74, 0.5, gate=0.6)
    ostinato(tu, seg(tl, 4, 32, 40), 'R! . . . . . . .', 27, 70, 0.5, gate=1.5)
    bs = s.part('upright', 'base', 'upright', level=-26, send=0.15, lp=3200, tj=6)
    ostinato(bs, seg(tl, 4, 8, 40), '. . R . . . 5 . . . R . . . 5 .', 39, 66, 0.5, gate=0.6)
    st = s.part('strings', 'base', 'strings', level=-30, send=0.4, lp=4200)
    pad(st, tl, 51, 70, 3, 38)
    hn = s.part('horn', 'rhythm', 'horn', level=-26, send=0.3, pan=0.2)
    comp(hn, seg(tl, 4, 8, 40), '..x...x.', 51, 68, 3, 0.5, 52, gate=0.5)
    pf = s.part('piano', 'rhythm', 'honky', level=-27, send=0.25, lp=4500, pan=-0.2)
    comp(pf, seg(tl, 4, 0, 32), '.x.x.x.x', 53, 69, 3, 0.5, 52, gate=0.4)
    dr = s.kit('snare', 'rhythm', 'brush', level=-25, send=0.2, tj=5)
    drums(dr, s, 0, 8, {KICK: 'x...............', SWIRL: 'o...o...o...o...'}, 86)
    drums(dr, s, 8, 32, {KICK: 'x.......x.......', TAP: '....x.......x...', SWIRL: 'o...o...o...o...', HATP: 'g.g.g.g.g.g.g.g.'}, 92,
          fill={TAP: '....x...x.xxxxxx'})
    drums(dr, s, 32, 40, {KICK: 'x...............', TAP: '........x.......', SWIRL: 'o...o...o...o...'}, 86)
    gk = s.part('glock', 'rhythm', 'glock', level=-30, send=0.4, pan=0.3)
    sprinkle(gk, s, tl, 8, 32, 79, 93, 48)
    _counter(s, tl, 'mutedtrp', 58, 73)
    tb = s.kit('tamb', 'intensity', 'standard', level=-31)
    drums(tb, s, 0, 40, {TAMB: 'g.g.o.g.g.g.o.g.'}, 70)
    tr = s.part('trombone', 'intensity', 'trombone', level=-29, send=0.3)
    pad(tr, tl, 48, 63, 2, 40)
    return s


def combat_secondact():
    s = Song('combat_secondact', 'Second Act', 'D minor', ['combat_late', 'combat_map_theatre'], qbpm=130, qpb=4, bars=40, seed=125,
             scale=sc(2, HARM), desc='Late stage drama: spiccato strings, horn and timpani, taiko hits, snare roll fills; no lead.')
    tl = prog(s, 'Dm | Dm | Bb | Bb | Gm | Gm | A7 | A7', 'Dm | F | Bb | C | Dm | Bb | Gm | A7',
              'Gm | Gm | Dm | Dm | Bb | C | A7sus4 | A7', 'Dm | A | Bb | F | Gm | Dm/F | Em7b5 | A7', 'Dm | Bb | Gm | A7 | Dm | Bb | A7sus4 | A7')
    vc = s.part('cellodrone', 'base', 'cello', level=-25, send=0.3, lp=3000)
    pedal(vc, s, 0, 16, pn('D2'), 62, every=4)
    roots(vc, seg(tl, 4, 16, 40), 38, [(0, None, 'R', 1.0)], 60)
    hn = s.part('horn', 'base', 'horn', level=-28, send=0.3, pan=0.15)
    roots(hn, seg(tl, 4, 8, 40), 38, [(0, None, 'R', 1.0)], 52)
    st = s.part('strings', 'base', 'strings', level=-29, send=0.4, lp=3200)
    pad(st, tl, 50, 66, 3, 40)
    os_ = s.part('spiccato', 'rhythm', 'faststr', level=-23, send=0.25, lp=5000)
    ostinato(os_, seg(tl, 4, 0, 32), 'R! R 5 R! R 3 R 5 R! R 5 R! R 8 7 5', 50, 64, 0.5, gate=0.35, accent=1.3)
    ostinato(os_, seg(tl, 4, 32, 40), 'R! . R . R! . 5 .', 50, 60, 0.5, gate=0.35)
    tk = s.part('taiko', 'rhythm', 'taiko', level=-25, send=0.3)
    drums(tk, s, 0, 40, {41: 'X.......o.......|X.....X.o.......'}, 74)
    tp = s.part('timpani_r', 'rhythm', 'timpani', level=-28, send=0.3)
    ostinato(tp, seg(tl, 4, 8, 32), 'R! . . . . . R . R! . . . . . . .', 38, 62, 0.5, gate=1.2)
    sn = s.kit('snare', 'rhythm', 'orch', level=-30, send=0.15, tj=4)
    drums(sn, s, 8, 40, {TAP: '................|............o.xo'}, 70)
    wb = s.kit('woodblock', 'rhythm', 'standard', level=-32, send=0.12, tj=3, pan=0.25)
    drums(wb, s, 8, 40, {WB_HI: 'xgogxgogxgogxgog'}, 72)
    p = _counter(s, tl, 'mutedtrp', 55, 70)
    p.level = -25
    tr = s.part('tremolo', 'intensity', 'trem', level=-30, send=0.4, lp=4500)
    pad(tr, tl, 58, 72, 2, 40)
    tb = s.part('trombone', 'intensity', 'trombone', level=-29, send=0.3)
    pad(tb, tl, 46, 60, 2, 40)
    return s


# =============================================================================================== bosses
def boss_marionette():
    s = Song('boss_marionette', 'Strings Attached', 'A minor', ['boss'], qbpm=99, qpb=3, bars=40, json_bpm=66, json_bpb=2, seed=131,
             scale=sc(9, HARM), desc='Stiff 3/4 puppet march: violin and glock motif, harpsichord and pizzicato, bass drum, wood-block taps.')
    tl = prog(s, 'Am | E7 | Am | E7 | Dm | Am | Bm7b5 E7 | Am', 'F | E7 | Am | Am/C | Dm | Am | Bm7b5 | E7',
              'Am | E7 | Am | E7 | Dm | Dm | Am | E7', 'F | G | Am | C | Dm | F | E7sus4 | E7', 'Am | E7 | Dm | Am | F | E7 | Am | Am')
    motif = 'A4:q C5:q E5:q | G#5:h A5:q | E5:q C5:q A4:q | G#4:h.'
    var = 'D5:q F5:q A5:q | E5:h C5:q | D5:q F5:q B4:q | G#4:h.'
    vn = s.part('violin', 'melody', 'violin', level=-21, send=0.3, tj=10, pan=-0.1)
    gl = s.part('glock', 'melody', 'glock', level=-25, send=0.4, tj=20, pan=0.2)
    for bar in (0, 8, 28):
        vn.play(mel(motif, bar * 3), 80, shape=False)
        gl.play(mel(motif, bar * 3 + 0.04), 68, shape=False, octave=1)
    vn.play(mel(var, 4 * 3), 78, shape=False)
    gl.play(mel(var, 4 * 3), 66, shape=False, octave=1)
    xy = s.part('xylo', 'melody', 'xylo', level=-28, send=0.3, pan=0.3)
    xy.play(mel(motif, 16 * 3), 64, shape=False, octave=1)
    cb = s.part('contrabass', 'base', 'contrabass', level=-26, send=0.25, lp=2000)
    roots(cb, tl, 28, [(0, None, 'R', 1.0)], 58)
    bn = s.part('bassoon', 'base', 'bassoon', level=-26, send=0.25)
    ostinato(bn, tl, 'R! . . 5 . .', 33, 66, 0.5, gate=0.5)
    st = s.part('strings', 'base', 'strings', level=-29, send=0.4, lp=3200)
    pad(st, tl, 48, 65, 3, 40)
    hc = s.part('harpsichord', 'rhythm', 'harpsichord', level=-25, send=0.3, lp=5000, pan=0.15)
    comp(hc, tl, 'x.xx.o', 55, 71, 3, 0.5, 56, gate=0.4)
    pz = s.part('pizz', 'rhythm', 'pizz', level=-28, send=0.3, pan=-0.25)
    ostinato(pz, seg(tl, 3, 8, 32), '. . . . 5 . | . . . . . 8', 45, 56, 0.5, gate=0.6)
    bd = s.kit('bassdrum', 'rhythm', 'orch', level=-26, send=0.3, tj=4)
    drums(bd, s, 0, 40, {36: 'X..o..'}, 84)
    wb = s.kit('taps', 'rhythm', 'standard', level=-30, send=0.2, tj=5)
    drums(wb, s, 0, 40, {WB_HI: '..g...|g.....|..g..o|......', CLAVES: 'o.....|......|......|..o...'}, 76)
    tr = s.part('tremolo', 'intensity', 'trem', level=-30, send=0.4)
    pad(tr, tl, 60, 74, 2, 40)
    xr = s.part('xyloruns', 'intensity', 'xylo', level=-29, send=0.3, pan=0.3)
    for b in range(3, 40, 4):
        t0 = b * 3 + 1.5
        top = bass_pitch(chord_at(tl, (b + 1) * 3 % (40 * 3)).root, 72)
        for k in range(3):
            xr.n(t0 + k * 0.5, 0.5, top - 3 + k, 60 + 6 * k)
    return s


def boss_pipes():
    s = Song('boss_pipes', 'The Howling Pipes', 'C-sharp minor', ['boss'], qbpm=120, qpb=4, bars=40, json_bpm=60, json_bpb=2, seed=133,
             scale=sc(1, HARM), desc='Grave organ march: reed organ and tubular bells over a pedal, low strings, timpani and taiko, slow ticks.')
    tl = prog(s, 'C#m | C#m | A | G#7 | C#m | B | A | G#7', 'F#m | C#m | A | E | F#m | D | G#7sus4 | G#7',
              'C#m | A | B | G#7 | C#m | A | F#m | G#7', 'A | B | C#m | C#m/G# | F#m | D | G#7sus4 | G#7', 'C#m | C#m | A | B | F#m | G#7 | C#m | C#m')
    motif = 'G#4:q C#5:q E5:q F5:q | E5:h C#5:h | B4:q A4:q G#4:q B4:q | A4:w'
    var = 'E5:q F#5:q A5:q G#5:q | F#5:h D5:h | C#5:q B4:q A4:q C5:q | G#4:w'
    og = s.part('organ', 'melody', 'reedorgan', level=-21, send=0.35, tj=8, pan=-0.1)
    bl = s.part('bells', 'melody', 'tubular', level=-25, send=0.5, tj=14, pan=0.2)
    for bar in (0, 8, 32):
        og.play(mel(motif, bar * 4), 80, shape=False)
        bl.play(mel(motif, bar * 4 + 0.04), 66, shape=False)
    og.play(mel(var, 4 * 4), 78, shape=False)
    bl.play(mel(var, 4 * 4), 64, shape=False)
    cb = s.part('contrabass', 'base', 'contrabass', level=-26, send=0.25, lp=1800)
    pedal(cb, s, 0, 8, pn('C#2'), 62, every=4)
    roots(cb, seg(tl, 4, 8, 40), 25, [(0, None, 'R', 1.0)], 58)
    vc = s.part('cello', 'base', 'cello', level=-27, send=0.3, lp=3000)
    roots(vc, seg(tl, 4, 8, 40), 37, [(0, None, 'R', 1.0)], 54)
    st = s.part('strings', 'base', 'strings', level=-29, send=0.45, lp=3000)
    pad(st, tl, 47, 64, 3, 40)
    ob = s.part('organbed', 'rhythm', 'reedorgan', level=-30, send=0.4, lp=2500)
    comp(ob, seg(tl, 4, 8, 40), 'x...............|x.......o.......', 49, 66, 3, 0.5, 46, gate=3.0)
    tp = s.part('timpani', 'rhythm', 'timpani', level=-26, send=0.35)
    ostinato(tp, tl, 'R! . . . . . R . R! . . . . . . .', 37, 66, 0.5, gate=1.4, accent=1.25)
    tk = s.part('taiko', 'rhythm', 'taiko', level=-28, send=0.3)
    drums(tk, s, 8, 40, {41: 'X.......o.......|X.......o.....g.'}, 70)
    wb = s.kit('ticks', 'rhythm', 'standard', level=-32, send=0.25, lp=3500, tj=4)
    drums(wb, s, 0, 40, {WB_LO: 'x...o...x...o...'}, 70)
    tr = s.part('tremolo', 'intensity', 'trem', level=-30, send=0.4)
    pad(tr, tl, 59, 73, 2, 40)
    bell = s.part('bell', 'intensity', 'tubular', level=-31, send=0.6, pan=0.3)
    for b in range(0, 40, 8):
        bell.n(b * 4, 4, pn('C#4'), 46)
    return s


# =============================================================================================== shops
def shop_receipts():
    s = Song('shop_receipts', 'Receipts and Rain', 'C major', ['shop_gold'], qbpm=104, qpb=4, bars=32, seed=135,
             scale=sc(0, MAJOR), desc='Rainy-window bossa: vibes tune, flute answers, nylon guitar and upright bass, cross-stick brushes.')
    A = 'Dm7 | G7 | Cmaj7 | Am7 | Dm7 | G7 | Em7 A7 | Dm7 G7'
    A2 = 'Dm7 | G7 | Cmaj7 | A7 | Dm7 | G7 | Cmaj7 A7 | Dm7 G7'
    B = 'Fmaj7 | Fm6 | Em7 | A7 | Dm7 | G7 | Em7 | A7'
    tl = prog(s, A, A2, B, A2)
    mA = 'F5:q A5:q D6:h | D6:q B5:q G5:h | E5:q G5:q B5:h | C6:q A5:q E5:h | F5:q A5:q C6:q D6:q | B5:q G5:q D5:h | G5:q B5:q C#6:q A5:q | D6:h B5:h'
    mB = 'A5:q C6:q F6:h | Ab5:q C6:q F6:h | G5:q B5:q E6:h | E6:q C#6:q A5:h | D6:q F6:q A5:h | B5:q D6:q G5:h | G5:q E5:q B5:h | C#6:q E6:q A5:h'
    vb = s.part('vibes', 'melody', 'vibes', level=-21, send=0.4, tj=10, pan=0.1)
    vb.play(mel(mA, 0), 76)
    vb.play(mel(mA, 32), 78)
    vb.play(mel(mB, 64), 78)
    vb.play(mel(mA, 96), 78)
    fl = s.part('flute', 'melody', 'flute', level=-24, send=0.35, pan=-0.2)
    for bar, a in ((3, 'r:h. E5:e G5:e'), (7, 'r:h C6:e A5:e F5:e E5:e'), (11, 'r:h. G5:e B5:e'), (27, 'r:h. E5:e G5:e')):
        fl.play(mel(a, bar * 4), 68)
    bs = s.part('upright', 'base', 'upright', level=-23, send=0.15, lp=3500, tj=6)
    ostinato(bs, tl, 'R! . . 5 . . R .', 36, 74, 0.5, gate=0.8)
    ng = s.part('nylon', 'base', 'nylon', level=-26, send=0.3, lp=4500, pan=-0.15)
    comp(ng, tl, 'x..x..x.', 53, 69, 3, 0.5, 50, gate=0.9)
    st = s.part('strings', 'base', 'strings', level=-31, send=0.4, lp=4500)
    pad(st, seg(tl, 4, 8, 32), 55, 72, 3, 36)
    dr = s.kit('brushes', 'rhythm', 'brush', level=-27, send=0.2, tj=5)
    drums(dr, s, 0, 32, {SIDE: 'x..x..x...x..x..', SWIRL: 'o.......o.......', KICK: 'x.....x.x.....x.', HATP: 'g.g.g.g.g.g.g.g.'}, 84)
    sh = s.kit('shaker', 'rhythm', 'standard', level=-32, send=0.15)
    drums(sh, s, 0, 32, {SHAKER: 'xgogxgogxgogxgog'}, 64)
    pz = s.part('pizz', 'rhythm', 'pizz', level=-29, send=0.3, pan=0.3)
    comp(pz, tl, '.......x|...x...x', 60, 76, 3, 0.5, 48, gate=0.5)
    sp = s.part('strings_i', 'intensity', 'strings', level=-28, send=0.4, lp=5000)
    pad(sp, tl, 60, 79, 4, 44)
    mb = s.part('musicbox', 'intensity', 'musicbox', level=-27, send=0.4, pan=0.35)
    sprinkle(mb, s, tl, 0, 32, 81, 98, 52, n=4, step=0.25, every=2, phase=1)
    return s


def shop_harbour():
    s = Song('shop_harbour', 'Pillow Fort Harbour', 'A lydian', ['shop_dream'], qbpm=76, qpb=3, bars=32, seed=137,
             scale=[9, 11, 1, 3, 4, 6, 8], desc='Drifting 3/4: marimba and celesta tune over harp arpeggios, a warm string bed and a low flute.')
    A = 'Amaj7 | B/A | Amaj7 | B/A | F#m7 | Dmaj7 | Bm7 | E7sus4'
    A2 = 'Amaj7 | B/A | Amaj7 | B/A | C#m7 | F#m7 | Dmaj7 | E7sus4'
    B = 'Dmaj7 | E/D | C#m7 | F#m7 | Bm7 | E7 | Amaj7 | E7sus4'
    tl = prog(s, A, A2, B, A)
    mA = 'E5:h. | D#5:h F#5:q | A5:h E5:q | D#5:h. | C#5:q E5:q A5:q | F#5:h D5:q | B4:q D5:q F#5:q | E5:h.'
    mA2 = 'E5:h. | D#5:h F#5:q | A5:h E5:q | D#5:h. | E5:q G#5:q C#6:q | A5:h F#5:q | D5:q F#5:q A5:q | E5:h.'
    mB = 'F#5:q A5:q D6:q | E6:h B5:q | E5:q G#5:q C#6:q | A5:h F#5:q | D6:q B5:q F#5:q | G#5:q B5:q D6:q | C#6:h E6:q | B5:h.'
    mr = s.part('marimba', 'melody', 'marimba', level=-21, send=0.45, tj=12)
    mr.play(mel(mA, 0), 74)
    mr.play(mel(mA2, 24), 76)
    mr.play(mel(mB, 48), 78)
    mr.play(mel(mA, 72), 76)
    ce = s.part('celesta', 'melody', 'celesta', level=-25, send=0.45, pan=0.2)
    ce.play(mel(mA, 72), 56, octave=1)
    st = s.part('strings', 'base', 'strings', level=-24, send=0.5, lp=4500)
    pad(st, tl, 49, 73, 4, 48)
    vc = s.part('cello', 'base', 'cello', level=-28, send=0.35, lp=3000)
    roots(vc, tl, 33, [(0, None, 'R', 1.0)], 50)
    fl = s.part('lowflute', 'base', 'flute', level=-29, send=0.4, pan=-0.2)
    guide_line(fl, seg(tl, 3, 8, 32), 64, 76, 48, s.scale, s.rng)
    hp = s.part('harp', 'rhythm', 'harp', level=-23, send=0.45, pan=-0.2)
    arp(hp, tl, 49, 82, '0 2 4 6 4 2 1 3 5 7 5 3', 0.5, 48, gate=2.5)
    tk = s.kit('ticks', 'rhythm', 'standard', level=-35, send=0.35, tj=5)
    drums(tk, s, 0, 32, {WB_HI: 'g..', TRI_O: 'o..|...'}, 62)
    mb = s.part('musicbox', 'intensity', 'musicbox', level=-25, send=0.5, pan=0.3)
    guide_line(mb, tl, 76, 90, 54, s.scale, s.rng)
    gl = s.part('glock', 'intensity', 'glock', level=-30, send=0.5, pan=-0.3)
    sprinkle(gl, s, tl, 0, 32, 81, 97, 46, n=3, step=0.5, every=4, phase=3)
    return s


# =============================================================================================== endless
def endless_lastorders():
    s = Song('endless_lastorders', 'Last Orders', 'C-sharp minor', ['endless'], qbpm=112, qpb=4, bars=40, seed=139,
             scale=sc(1, HARM), desc='Dark late-night loop: upright bass, cello pedal, plucked ostinato, soft taiko and a slow bell; no lead.')
    tl = prog(s, 'C#m | C#m | A | A | F#m | F#m | G#7 | G#7', 'C#m | B | A | G#7 | C#m | E | F#m | G#7sus4',
              'A | E | B | G#7 | A | F#m | D | G#7', 'C#m | A | F#m | G#7 | C#m | A | D | G#7', 'C#m | C#m | A | B | F#m | D | G#7sus4 | G#7')
    vc = s.part('cellodrone', 'base', 'cello', level=-25, send=0.35, lp=2500)
    pedal(vc, s, 0, 16, pn('C#2'), 58, every=4)
    roots(vc, seg(tl, 4, 16, 32), 37, [(0, None, 'R', 1.0)], 56)
    pedal(vc, s, 32, 40, pn('C#2'), 58, every=4)
    bs = s.part('upright', 'base', 'upright', level=-25, send=0.15, lp=3000, tj=6)
    ostinato(bs, seg(tl, 4, 8, 32), 'R! . . R . . 5 . R! . . R . 5 . 8', 37, 72, 0.5, gate=0.7)
    tr = s.part('tremlow', 'base', 'trem', level=-28, send=0.45, lp=2200)
    pad(tr, tl, 46, 63, 3, 40)
    pz = s.part('pizz', 'rhythm', 'pizz', level=-24, send=0.3, lp=3500)
    ostinato(pz, seg(tl, 4, 0, 32), 'R! R . R 3 . 5 R R! R . R 3 . -5 R', 37, 60, 0.5, gate=0.75)
    ostinato(pz, seg(tl, 4, 32, 40), 'R! . . R . . 5 .', 37, 56, 0.5)
    pn_ = s.part('pianostab', 'rhythm', 'piano', level=-29, send=0.35, lp=3000, pan=-0.15)
    comp(pn_, seg(tl, 4, 8, 32), '..x...x.........', 49, 66, 3, 0.5, 46, gate=0.8)
    tp = s.part('timpani', 'rhythm', 'timpani', level=-26, send=0.35)
    ostinato(tp, tl, 'R! . . . . . R . R! . . . . . . .', 37, 62, 0.5, gate=1.5, accent=1.25)
    wb = s.kit('ticks', 'rhythm', 'standard', level=-32, send=0.2, lp=3500, tj=4)
    drums(wb, s, 0, 40, {WB_LO: 'x...o...x...o...'}, 70)
    dr = s.kit('brushes', 'rhythm', 'brush', level=-28, send=0.2, tj=5)
    drums(dr, s, 8, 32, {KICK: 'x.....x.........', SWIRL: 'o...............', TAP: '............g...'}, 84)
    bell = s.part('bell', 'rhythm', 'tubular', level=-33, send=0.6, pan=0.3)
    for b in range(0, 40, 8):
        bell.n(b * 4, 4, pn('C#4'), 46)
    _counter(s, tl, 'clarinet', 54, 68)
    ce = s.part('celesta', 'intensity', 'celesta', level=-30, send=0.5)
    sprinkle(ce, s, tl, 0, 40, 70, 86, 42, every=4)
    return s


ALL = [menu_moth, select_lanternroll, hub_corridor, combat_queue, combat_airmail, combat_wick, combat_cardindex, combat_pollen,
       combat_rain, combat_housedown, combat_secondact, boss_marionette, boss_pipes, shop_receipts, shop_harbour, endless_lastorders]
