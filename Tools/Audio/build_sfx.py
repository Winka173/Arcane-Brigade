"""Builds every SFX / voice / ambience clip and the cue manifest (cues.json).

Run:  python3 Tools/Audio/build_sfx.py            (from repo root)
Output: Assets/ArcaneBrigade/Resources/Audio/Arcane/{Sfx,Voice,Amb}/<cue>_<nn>.wav
        Assets/ArcaneBrigade/Resources/Audio/Arcane/cues.json  (TextAsset read by ArcaneCueLibrary)

Manifest fields per cue: id, bus, priority (Unity: 0 loudest-priority .. 256), gainDb, cooldown (s),
maxVoices (concurrent for this cue), pitchJitter (+-semitone fraction), danger, clips[].
"""
import json
import os
import sys

import numpy as np

sys.path.insert(0, os.path.dirname(__file__))
import sfx_gen as g
from synth import SR, write_wav, _stable

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
OUT = os.path.join(ROOT, 'Assets', 'ArcaneBrigade', 'Resources', 'Audio', 'Arcane')

MATS = ['paper', 'metal', 'wax', 'fabric', 'glass', 'wood']

# (id, generator, kwargs, variations, bus, priority, gainDb, cooldown, maxVoices, pitchJitter, flags)
CUES = []


def C(cid, fn, nvar, bus, prio, gain, cd, vox, jit=0.0, loop=False, danger=False, **kw):
    CUES.append(dict(id=cid, fn=fn, kw=kw, n=nvar, bus=bus, prio=prio, gain=gain, cd=cd, vox=vox, jit=jit, loop=loop, danger=danger))


# --- combat: swings, hits (frequent => 4-8 variations; high-traffic cues get short cooldown + low voice count)
for kind, nv in (('blade', 6), ('spear', 5), ('hammer', 4), ('scissors', 4), ('umbrella', 4), ('whip', 4)):
    C('cmb.swing.' + kind, g.swing, nv, 'Combat', 150, -14, 0.05, 3, 0.03, kind=kind)
C('cmb.windup', g.windup, 3, 'Combat', 150, -17, 0.12, 2, 0.02)
C('cmb.miss', g.miss, 4, 'Combat', 190, -22, 0.08, 2, 0.04)
for m in MATS:
    C('cmb.hit.' + m, g.impact, 6, 'Combat', 140, -15, 0.04, 3, 0.03, material=m)
    C('enemy.die.' + m, g.enemy_die, 3, 'Combat', 120, -13, 0.07, 3, 0.08)  # wide pitch spread: a falling crowd never repeats one clip
CUES[-1]  # (enemy.die.* share generator; variations differ by index -> material cycle below)
C('cmb.crit', g.crit_sting, 5, 'Combat', 100, -13, 0.1, 2, 0.0, material='wood')

# weapons / signature
C('wpn.bow.draw', g.bow_draw, 3, 'Combat', 160, -20, 0.2, 1, 0.02)
C('wpn.bow.release', g.bow_release, 5, 'Combat', 150, -15, 0.06, 3, 0.03)
C('wpn.gun.pop', g.pop_gun, 4, 'Combat', 150, -15, 0.06, 3, 0.03)
C('wpn.launcher', g.pop_gun, 3, 'Combat', 130, -16, 0.12, 2, 0.02, big=True)
C('wpn.cast.fire', g.fire_cast, 4, 'Combat', 150, -19, 0.08, 2, 0.03)
C('wpn.cast.ice', g.ice_cast, 4, 'Combat', 150, -17, 0.08, 2, 0.02)
C('wpn.cast.arcane', g.arcane_cast, 4, 'Combat', 150, -17, 0.08, 2, 0.02)
C('wpn.boomerang.fly', g.boomerang_fly, 3, 'Combat', 170, -20, 0.3, 1, 0.02)
C('wpn.boomerang.return', g.boomerang_return, 3, 'Combat', 150, -16, 0.2, 1, 0.02)
C('wpn.pierce', g.pierce, 4, 'Combat', 160, -18, 0.07, 2, 0.03)
C('wpn.split', g.split, 3, 'Combat', 160, -18, 0.1, 2, 0.02)
C('wpn.bounce', g.bounce, 4, 'Combat', 170, -19, 0.06, 2, 0.04)
C('sig.bell', g.bell_ring, 4, 'Combat', 110, -14, 0.25, 1, 0.0, big=True)
C('sig.umbrella', g.umbrella_pop, 3, 'Combat', 120, -15, 0.2, 1, 0.02)
C('sig.stamp', g.stamp, 3, 'Combat', 120, -16, 0.2, 1, 0.02)
C('sig.scissors', g.snip, 4, 'Combat', 130, -17, 0.1, 2, 0.02)
C('sig.camera', g.camera_flash, 3, 'Combat', 120, -15, 0.3, 1, 0.0)
C('sig.musicbox', g.music_box_note, 3, 'Combat', 130, -18, 0.2, 1, 0.0)
C('sig.tea', g.tea_splash, 3, 'Combat', 150, -17, 0.12, 2, 0.03)
C('sig.fish', g.fish_slap, 3, 'Combat', 150, -15, 0.1, 2, 0.03)
C('sig.alarm', g.alarm_ring, 2, 'Combat', 140, -18, 0.3, 1, 0.0)
C('sig.iron', g.iron_press, 3, 'Combat', 130, -15, 0.15, 1, 0.02)
C('sig.hammer', g.hammer_slam, 3, 'Combat', 120, -13, 0.15, 1, 0.02)
C('fx.burn', g.burn_tick, 4, 'Combat', 200, -24, 0.12, 2, 0.05)
C('fx.chill', g.chill_tick, 4, 'Combat', 200, -24, 0.12, 2, 0.03)
C('fx.knockback', g.knockback, 4, 'Combat', 170, -19, 0.08, 2, 0.04)
C('fx.reaction.steam', g.reaction, 2, 'Combat', 130, -16, 0.2, 1, 0.0, kind='steam')
C('fx.reaction.shatter', g.reaction, 2, 'Combat', 130, -16, 0.2, 1, 0.0, kind='shatter')
C('fx.reaction.arcane', g.reaction, 2, 'Combat', 130, -16, 0.2, 1, 0.0, kind='arcane')

# --- characters
C('chr.step', g.step, 6, 'Foley', 220, -26, 0.16, 2, 0.05)
C('chr.dash', g.dash_whoosh, 4, 'Foley', 190, -20, 0.2, 2, 0.04)
C('chr.jump', g.jump, 3, 'Foley', 200, -22, 0.2, 2, 0.03)
C('chr.land', g.land, 3, 'Foley', 200, -22, 0.2, 2, 0.03)
C('plr.dodge', g.dodge, 3, 'Foley', 100, -18, 0.2, 1, 0.02)
C('plr.hurt', g.hurt, 5, 'Combat', 20, -9, 0.15, 1, 0.0, danger=True)
C('plr.lowhp', g.heartbeat, 2, 'Combat', 30, -11, 0.7, 1, 0.0, danger=True)
C('plr.heal', g.heal, 3, 'Foley', 90, -14, 0.25, 1, 0.0)
C('plr.death', g.player_death, 2, 'Combat', 10, -8, 1.0, 1, 0.0, danger=True)

# --- enemies / bosses
C('enemy.spit.windup', g.spit_windup, 4, 'Combat', 40, -14, 0.1, 2, 0.04, danger=True)
C('enemy.spit.fire', g.spit_fire, 5, 'Combat', 40, -12, 0.06, 3, 0.04, danger=True)
C('enemy.lunge', g.enemy_lunge, 4, 'Combat', 50, -14, 0.12, 2, 0.04, danger=True)
C('enemy.hurt', g.enemy_hurt, 6, 'Combat', 170, -20, 0.05, 3, 0.04)
C('enemy.big.step', g.big_step, 3, 'Combat', 80, -14, 0.3, 1, 0.02)
C('enemy.big.roar', g.big_roar, 3, 'Combat', 60, -11, 1.0, 1, 0.0)
C('enemy.die.large', g.enemy_die, 3, 'Combat', 70, -11, 0.2, 2, 0.02, size='large')
C('boss.enter', g.boss_enter, 2, 'Combat', 5, -8, 5.0, 1, 0.0, danger=True)
C('boss.telegraph', g.telegraph, 3, 'Combat', 10, -9, 0.3, 1, 0.0, danger=True)
C('boss.attack', g.boss_attack, 3, 'Combat', 10, -9, 0.2, 1, 0.0, danger=True)
C('boss.phase', g.phase_change, 2, 'Combat', 5, -8, 3.0, 1, 0.0, danger=True)
C('boss.defeat', g.boss_defeat, 2, 'Combat', 5, -8, 3.0, 1, 0.0)

# --- integration-contract additions (weapon payoffs, hazards, mimic, negative UI)
C('sig.alarm.boom', g.alarm_boom, 3, 'Combat', 120, -12, 0.15, 2, 0.03)
C('fx.field.zipper', g.zipper_link, 3, 'Combat', 160, -20, 0.1, 2, 0.03)
C('fx.hazard.warn', g.hazard_warn, 2, 'Combat', 40, -12, 0.4, 1, 0.0, danger=True)
C('fx.hazard.hit', g.hazard_hit, 3, 'Combat', 60, -12, 0.2, 2, 0.02)
C('enemy.mimic.wake', g.mimic_wake, 2, 'Combat', 60, -13, 0.4, 1, 0.0)
C('hero.pulse.ember', g.hero_pulse, 2, 'Combat', 90, -15, 1.0, 1, 0.0, element='ember')
C('hero.pulse.frost', g.hero_pulse, 2, 'Combat', 90, -12, 1.0, 1, 0.0, element='frost')
for _kind in ('locked', 'error'):
    C('ui.' + _kind, g.ui_negative, 2, 'UI', 64, -17, 0.15, 1, 0.0, kind=_kind)

# --- loot
C('loot.coin.drop', g.coin, 4, 'Foley', 190, -22, 0.04, 3, 0.03, mode='drop')
C('loot.coin.bounce', g.coin, 5, 'Foley', 200, -25, 0.05, 3, 0.04, mode='bounce')
C('loot.coin.vacuum', g.coin, 3, 'Foley', 200, -27, 0.05, 2, 0.04, mode='vacuum')
C('loot.coin.pickup', g.coin, 6, 'Foley', 160, -19, 0.035, 3, 0.02, mode='pickup')
C('loot.egg', g.egg_pickup, 4, 'Foley', 150, -17, 0.06, 2, 0.02)
C('loot.heart', g.heart_pickup, 3, 'Foley', 100, -15, 0.1, 1, 0.0)
C('loot.chest.appear', g.chest_appear, 2, 'Foley', 110, -14, 0.5, 1, 0.0)
C('loot.chest.open', g.chest_open, 3, 'Foley', 100, -13, 0.5, 1, 0.0)

# --- upgrades
C('up.merge', g.merge, 3, 'Foley', 80, -13, 0.4, 1, 0.0)
C('up.merge.gather', g.merge_gather, 3, 'Foley', 90, -16, 0.5, 1, 0.0)
C('up.star2', g.star_up, 2, 'Foley', 70, -11, 0.5, 1, 0.0, stars=1)
C('up.star3', g.star_up, 2, 'Foley', 70, -10, 0.5, 1, 0.0, stars=3)
C('up.ally.land', g.ally_land, 4, 'Foley', 130, -17, 0.12, 2, 0.03)
C('up.bond.t1', g.bond_tier, 2, 'Foley', 90, -14, 0.5, 1, 0.0, tier=1)
C('up.bond.t2', g.bond_tier, 2, 'Foley', 90, -13, 0.5, 1, 0.0, tier=2)
C('up.chain', g.chain_trigger, 4, 'Foley', 120, -17, 0.2, 1, 0.0)
for tier in ('blue', 'purple', 'gold'):
    C('up.rarity.' + tier, g.rarity, 2, 'Foley', 100, -14, 0.3, 1, 0.0, tier=tier)

# --- shop
C('shop.keeper.enter.gold', g.keeper_enter, 2, 'Foley', 60, -11, 1.0, 1, 0.0, special=False)
C('shop.keeper.enter.dream', g.keeper_enter, 2, 'Foley', 60, -11, 1.0, 1, 0.0, special=True)
C('shop.keeper.exit.gold', g.keeper_exit, 2, 'Foley', 70, -15, 1.0, 1, 0.0, special=False)
C('shop.keeper.exit.dream', g.keeper_exit, 2, 'Foley', 70, -15, 1.0, 1, 0.0, special=True)
C('shop.carpet.open', g.carpet, 2, 'Foley', 90, -15, 0.5, 1, 0.0, mode='open')
C('shop.carpet.roll', g.carpet, 2, 'Foley', 90, -15, 0.5, 1, 0.0, mode='roll')
C('shop.stand.appear', g.stand_appear, 4, 'Foley', 130, -18, 0.07, 2, 0.03)
C('shop.item.pop', g.item_pop, 4, 'Foley', 130, -18, 0.07, 2, 0.03)
C('shop.approach', g.approach, 4, 'UI', 80, -20, 0.12, 1, 0.0)
C('shop.focus', g.focus, 4, 'UI', 80, -22, 0.06, 1, 0.0)
C('shop.buy.gold', g.buy_gold, 4, 'UI', 40, -12, 0.15, 1, 0.0)
C('shop.buy.dream', g.buy_dream, 3, 'UI', 40, -12, 0.15, 1, 0.0)
for cur in ('gold', 'egg', 'hp'):
    C('shop.deny.' + cur, g.deny, 2, 'UI', 40, -14, 0.25, 1, 0.0, currency=cur)
C('shop.invalid', g.invalid, 3, 'UI', 60, -19, 0.12, 1, 0.0)
C('shop.reroll', g.reroll, 3, 'UI', 60, -14, 0.3, 1, 0.0)
C('shop.fusion', g.fusion, 2, 'UI', 40, -10, 0.5, 1, 0.0)
C('shop.descend', g.descend, 2, 'UI', 50, -16, 0.8, 1, 0.0)

# --- ui
for kind, nv, gain, cd in (('hover', 4, -24, 0.04), ('select', 4, -18, 0.05), ('confirm', 3, -15, 0.1), ('back', 3, -17, 0.1),
                           ('open', 2, -18, 0.2), ('close', 2, -18, 0.2), ('page', 3, -22, 0.1), ('tooltip', 2, -26, 0.15),
                           ('tab', 3, -21, 0.05), ('pause', 1, -17, 0.3), ('resume', 1, -17, 0.3), ('setting', 3, -23, 0.04),
                           ('unlock', 2, -13, 0.5), ('achievement', 2, -12, 0.5), ('receipt', 2, -17, 0.5)):
    C('ui.' + kind, g.ui, nv, 'UI', 64, gain, cd, 1, 0.0, kind=kind)

# --- stings
C('sting.waveclear', g.sting, 2, 'Foley', 30, -12, 1.0, 1, 0.0, kind='waveclear')
C('sting.victory', g.sting, 1, 'Foley', 5, -9, 5.0, 1, 0.0, kind='victory')
C('sting.defeat', g.sting, 1, 'Foley', 5, -9, 5.0, 1, 0.0, kind='defeat')

# --- ambience (seamless loops, played on Ambience bus under combat)
for kind in ('stage', 'office', 'lostshop', 'goldshop', 'dreamshop', 'toys'):
    C('amb.' + kind, g.ambience, 1, 'Ambience', 240, -30, 0.0, 1, 0.0, loop=True, kind=kind)
C('amb.transition', g.ambience, 2, 'Ambience', 200, -20, 1.0, 1, 0.0, kind='transition')

# --- shopkeeper voice (gibberish; runtime paces by words/punctuation, never per glyph)
for keeper in ('gold', 'dream'):
    C('voice.%s.syl' % keeper, g.syllable, 8, 'Voice', 100, -15, 0.09, 1, 0.04, keeper=keeper)
    for kind, nv in (('laugh', 2), ('hmm', 2), ('gasp', 2), ('farewell', 2)):
        C('voice.%s.%s' % (keeper, kind), g.vocal_gesture, nv, 'Voice', 90, -14, 0.8, 1, 0.0, keeper=keeper, kind=kind)


def folder(c):
    if c['bus'] == 'Voice': return 'Voice'
    if c['bus'] == 'Ambience': return 'Amb'
    return 'Sfx'


def render(c, i):
    kw = dict(c['kw'])
    if c['id'].startswith('enemy.die.') and c['id'] != 'enemy.die.large':
        mat = c['id'].split('.')[-1]
        kw['size'] = 'small'
        rng = np.random.default_rng(_stable((c['id'], i)))
        # force this cue's material: shift variation index onto the material's slot in the 6-cycle
        idx = MATS_ENEMY.index(mat) + 6 * i
        return c['fn'](idx, rng, **kw)
    rng = np.random.default_rng(_stable((c['id'], i)))
    return c['fn'](i, rng, **kw)


MATS_ENEMY = ['paper', 'wax', 'fabric', 'wood', 'metal', 'glass']  # enemy_die cycles in this order


def main(only=None):
    manifest = []
    total = 0
    for c in CUES:
        clips = []
        for i in range(c['n']):
            rel = '%s/%s_%02d' % (folder(c), c['id'].replace('.', '_'), i)
            if only and c['id'] not in only:
                clips.append(rel)   # manifest stays complete; untouched binaries are not rewritten
                continue
            x = render(c, i).astype(np.float64)
            if c['loop']:
                y = x * 10 ** (-3 / 20) / max(np.max(np.abs(x)), 1e-6) * 0.6
            else:
                y = x * 10 ** (-3 / 20) / max(np.max(np.abs(x)), 1e-6)
            rel = '%s/%s_%02d' % (folder(c), c['id'].replace('.', '_'), i)
            path = os.path.join(OUT, rel + '.wav')
            os.makedirs(os.path.dirname(path), exist_ok=True)
            write_wav(path, y)
            clips.append(rel)
            total += 1
        manifest.append(dict(id=c['id'], bus=c['bus'], priority=c['prio'], gainDb=c['gain'], cooldown=c['cd'],
                             maxVoices=c['vox'], pitchJitter=c['jit'], loop=c['loop'], danger=c['danger'], clips=clips))
    with open(os.path.join(OUT, 'cues.json'), 'w') as f:
        json.dump({'version': 1, 'cues': manifest}, f, indent=1)
    print('cues:', len(manifest), 'clips:', total)


if __name__ == '__main__':
    main(set(sys.argv[1:]) or None)
