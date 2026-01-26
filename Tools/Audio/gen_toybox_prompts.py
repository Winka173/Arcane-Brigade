"""Writes Docs/Audio/Music_Prompts_Toybox.md — the "Midnight Toybox Band" identity (our own, not reference-led).
Timestamps are computed from tempo/meter so every prompt's sections land on bar lines.
Then run gen_agent_brief.py to append the matching SFX prompts.   python Tools/Audio/gen_toybox_prompts.py"""
import os

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
OUT = os.path.join(ROOT, 'Docs', 'Audio', 'Music_Prompts_Toybox.md')

WORLD = ('Original instrumental music for the game "Arcane Brigade": at midnight the objects of a forgotten curio shop wake up '
         'and play as a band — the instruments ARE the objects (music box, toy piano, glockenspiel, calliope, melodica, wind-up keys, '
         'clock ticks, typewriter, cash-register drawers, coins) over a modern, punchy, catchy beat. Cool and stylish, not a kids song or nursery rhyme.')
MOTIF = ('Weave in the Brigade motif: a bright rising 4-note call (sol-do-re-mi in the key of the track) answered by a falling 3-note reply (mi-re-do).')
RULES = ('Instrumental only: no lead vocals, no lyrics (short crowd shouts allowed). Lead melody always clear and louder than the bass, crisp highs, punchy drums. Drums never stop, no silence, no build-up, '
         'no drop, no fade-out, no ending — the last bar leads straight back to the start. Not lo-fi, not chill, not ambient.')

# id, title, scene line, genre/palette, key, bpm, meter, bars, sections(sting, hook, answer, bridge, return)
TRACKS = [
    ('menu_curtain', 'Midnight Overture', 'main menu — the shop wakes up',
     'toy-pop overture: music box and toy piano lead, plucky bass, glockenspiel sparkles, light four-on-the-floor with tick-tock percussion, warm calliope chords',
     'F major', 112, 4, 32,
     ('over the running beat, a music box plays the Brigade motif and the whole toy band answers with one big chord',
      'toy piano + glockenspiel play the main theme (built on the Brigade motif), catchy and upfront',
      'calliope answers each phrase, wind-up ratchet fills between phrases',
      'a new sweeping melody on melodica over the same groove, clock chimes on accents',
      'main theme returns with everyone doubling it')),
    ('select_dressing', 'Roll Call', 'champion select — the brigade lines up',
     'toy-march pop: snare-drum roll pattern, toy trumpets, glockenspiel, handclaps, bouncy bass',
     'Bb major', 120, 4, 16,
     ('toy trumpet fanfare quoting the Brigade motif + snare roll',
      'glockenspiel + toy trumpets play a proud marching hook',
      'handclaps and calliope answer the hook',
      'new cheeky melody on melodica, march keeps going',
      'hook returns with full band')),
    ('combat_toys', 'Wind-Up Parade', 'early combat — the toys go to war',
     'energetic toy-pop / electro-march: punchy electronic drums with toy snare, wind-up ratchets as hi-hats, bouncy synth bass, toy piano + calliope lead, toy-brass stabs',
     'A minor', 132, 4, 32,
     ('wind-up key cranks, then a big band hit + crowd of toys shouting "hey!"',
      'toy piano + calliope play a bouncy, mischievous hook built on the Brigade motif',
      'toy-brass stabs answer every phrase, rattles and clacks in the gaps',
      'new marching melody on glockenspiel, drums keep driving',
      'hook returns an octave higher')),
    ('combat_spoons', 'Cutlery Brigade', 'early combat, alternate — the kitchen drawer joins in',
     'funky toy-pop: tight drums with spoon and pot-lid percussion, slap bass, whistled lead, toy-brass section',
     'D major', 128, 4, 32,
     ('pot-lid crash + spoon drum fill',
      'whistled lead + toy brass play a cheeky hook in unison',
      'spoons and toy brass trade phrases (call and response)',
      'new sneaky melody on toy piano over the same groove',
      'hook returns with extra harmony')),
    ('combat_clockwork', 'Gearworks', 'mid combat — the clocks and machines fight back',
     'driving electro-pop with clockwork percussion: four-on-the-floor kick, ticking clock hi-hats, gear ratchets, cuckoo-clock calls, analog synth bass, bright square-wave + glockenspiel lead',
     'E minor', 140, 4, 32,
     ('a cuckoo clock strikes, gears engage, kick hits',
      'square-wave + glockenspiel lead plays a relentless hook built on the Brigade motif',
      'cuckoo calls and typewriter dings answer each phrase',
      'new mechanical melody on music box over a pounding beat',
      'hook returns with a faster arpeggio layer')),
    ('combat_carousel', 'Carousel Chase', 'mid combat, alternate — the carousel spins out of control',
     'fast swing-pop with carnival colour: driving kick, swung hats, walking bass, calliope + accordion-like lead, glockenspiel, carousel bells',
     'A harmonic minor', 140, 4, 32,
     ('carousel bell rings + band hit',
      'calliope plays a dizzying, catchy hook',
      'accordion-like synth answers, carousel bells on accents',
      'new spinning melody on toy piano, groove keeps going',
      'hook returns with bell harmony')),
    ('combat_overtime', 'Midnight Stampede', 'late combat — the whole shop stampedes',
     'high-energy breakbeat / drum & bass with toy orchestra: rapid breakbeats, deep bass, toy piano runs, orchestral hits, glockenspiel lead, typewriter and stapler percussion',
     'G minor', 170, 4, 32,
     ('an alarm-clock bell rings into a hard drum hit',
      'glockenspiel + toy piano play an urgent hook built on the Brigade motif',
      'orchestral hits and typewriter rolls answer each phrase',
      'new heroic melody on calliope over rolling breaks',
      'hook returns at full strength')),
    ('combat_lasttrain', 'Toy Train Express', 'late combat, alternate — a runaway toy train',
     'fast breakbeat with toy train percussion: chugging rhythm, train whistle synth, toy piano lead, rolling bass, bells',
     'C minor', 168, 4, 32,
     ('toy train whistle blows over the already-running chugging beat',
      'toy piano plays a racing, catchy hook',
      'train-whistle synth answers each phrase',
      'new melody on glockenspiel, chugging breaks keep rolling',
      'hook returns with full band')),
    ('boss_ringmaster', 'The Ringmaster', 'boss fight — the headliner of the night',
     'dark theatrical toy-orchestra rock: heavy drums, distorted bass, sinister calliope + organ lead, brass hits, timpani and gong, music-box quotes of the Brigade motif turned minor',
     'D minor', 150, 4, 32,
     ('timpani roll + gong + evil calliope chord',
      'organ + distorted guitar play a menacing hook',
      'brass hits and timpani answer every phrase',
      'a creepy music-box version of the Brigade motif over driving toms',
      'hook returns with the whole orchestra')),
    ('boss_clown', 'Jack-in-the-Box', 'boss fight, alternate — something terrible springs out',
     'aggressive electro with sinister toy colour: hard clap, heavy 808, twisted jack-in-the-box melody on detuned music box and calliope, spring "boing" accents, distorted synth stabs',
     'E minor', 150, 4, 32,
     ('jack-in-the-box crank melody over the beat ends in a POP! and a heavy hit',
      'detuned music box + calliope play a twisted, catchy hook',
      'distorted synth stabs answer, spring boings on accents',
      'new eerie melody on toy piano, beat keeps going',
      'hook returns with extra distorted layer')),
    ('shop_gold', 'Ka-Ching Swing', 'gold shop — a greedy, cheerful merchant',
     'bouncy swing-pop: shuffle drums, upright-style bass, honky-tonk toy piano lead, cash-register drawer and coin percussion, muted toy trumpet',
     'C major', 116, 4, 32,
     ('cash register "ka-ching!" + coin shower',
      'honky-tonk toy piano plays a cheeky, greedy hook',
      'muted toy trumpet answers, coin jingles on every 4th bar',
      'new playful melody on glockenspiel, swing keeps going',
      'hook returns with trumpet harmony')),
    ('shop_layaway', 'Window Shopping', 'gold shop, alternate — browsing the shelves',
     'bright city-pop with toy colour: punchy drums, slap bass, electric piano, toy glockenspiel lead, handclaps, shop bell',
     'F major', 112, 4, 32,
     ('shop door bell jingles + chord hit',
      'glockenspiel + electric piano play a sunny, catchy hook',
      'handclaps and whistled answers',
      'new melody on toy piano, groove keeps going',
      'hook returns with full band')),
    ('shop_dream', 'The Dream Counter', 'dream shop — the only truly dreamy room',
     'enchanted waltz: oom-pah-pah soft bass, music box and glass harp lead, celesta, airy choir pads, gentle shimmer; mysterious and beautiful but with a clear melody and steady rhythm',
     'Ab major', 138, 3, 32,
     ('glass harp glissando over the running waltz, music box states the Brigade motif',
      'music box + celesta play a hypnotic, memorable waltz melody',
      'airy choir answers each phrase',
      'new floating melody on glass harp, waltz keeps turning',
      'melody returns with everyone')),
    ('endless_neverclose', 'Open All Night', 'endless mode — the show never ends',
     'euphoric dance-pop: four-on-the-floor, pumping chords, octave bass, huge toy-piano + synth lead, glockenspiel sparkles, crowd cheers',
     'Bb major', 128, 4, 32,
     ('crowd cheer + chord stab',
      'toy-piano + synth lead play a euphoric hook built on the Brigade motif',
      'crowd "hey!" and glockenspiel answer each phrase',
      'new soaring melody on calliope, beat keeps pumping',
      'hook returns with everyone')),
    ('endless_encore', 'One More Encore', 'endless mode, alternate — the final bow, again and again',
     'triumphant pop-rock with toy orchestra: driving drums, bass, power chords, toy brass, glockenspiel and calliope lead',
     'G major', 140, 4, 32,
     ('drum fill + full band hit + applause',
      'calliope + toy brass play a triumphant hook (the Brigade motif in full glory)',
      'power chords and glockenspiel answer each phrase',
      'new heroic melody on toy piano, drums keep driving',
      'hook returns with applause')),
]


def stamp(seconds):
    seconds = int(round(seconds))
    return '%d:%02d' % (seconds // 60, seconds % 60)


def main():
    lines = ['# Music prompts — "Midnight Toybox Band" (our own identity)\n\n',
             'The objects of the curio shop play the soundtrack themselves over modern, punchy beats. One shared leitmotif (the Brigade motif) ties all 15 tracks together. '
             'Dreamy colour is reserved for the dream shop. Structure: STING → HOOK → ANSWER → BRIDGE → HOOK RETURN, no drop, never silent. '
             'Set the generation length to the stated duration. Track ids/scenes are unchanged.\n']
    for index, (pid, title, scene, palette, key, bpm, bpb, bars, sec) in enumerate(TRACKS, 1):
        bar = bpb * 60.0 / bpm
        cuts = [0, 1, 5, 9, 13, 16] if bars == 16 else [0, 2, 10, 18, 26, 32]
        names = ['STING', 'HOOK', 'ANSWER', 'BRIDGE', 'HOOK RETURN']
        total = bars * bar
        body = ['%s %s' % (WORLD, MOTIF),
                'Track: "%s" — %s.' % (title, scene),
                'Style: %s.' % palette,
                '%s, %d BPM, %d/4, %d bars, %d seconds.' % (key, bpm, bpb, bars, round(total)),
                'Structure (follow strictly):']
        for k, name in enumerate(names):
            body.append('%s-%s %s: %s.' % (stamp(cuts[k] * bar), stamp(cuts[k + 1] * bar), name, sec[k]))
        body.append(RULES)
        lines.append('\n## %d. %s — "%s"\n```\n%s\n```\n' % (index, pid, title, '\n'.join(body)))
    with open(OUT, 'w', encoding='utf-8', newline='\n') as f:
        f.write(''.join(lines))
    print('written', OUT)


if __name__ == '__main__':
    main()
