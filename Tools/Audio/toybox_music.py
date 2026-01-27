"""Render independently authored scores, replacing the shared v3 composition template.

Each song is written separately in Scores/<piece>.json: its own harmony,
phrases, bass parts, drum charts, instrumentation, form and mix. Shared code
handles notation/encoding only. No common motif or bridge is injected.
"""
import subprocess

import imageio_ffmpeg

from score_player import STEMS, load_score, render

IDS=('menu_curtain','select_dressing','combat_toys','combat_spoons','combat_clockwork',
     'combat_carousel','combat_overtime','combat_lasttrain','boss_ringmaster',
     'boss_clown','shop_gold','shop_layaway','shop_dream','endless_neverclose','endless_encore')


def encode_vorbis(source,dest):
    result=subprocess.run([imageio_ffmpeg.get_ffmpeg_exe(),'-hide_banner','-loglevel','error','-y',
                           '-i',str(source),'-map_metadata','-1','-c:a','libvorbis','-q:a','8',str(dest)],
                          capture_output=True,text=True,creationflags=getattr(subprocess,'CREATE_NO_WINDOW',0))
    if result.returncode: raise RuntimeError(result.stderr)


def parse_tracks():
    configs=[]
    for pid in IDS:
        score=load_score(pid)
        configs.append((pid,score['title'],score['key'],score['bpm'],score['meter'],sum(s['bars'] for s in score['form'])))
    return configs


def render_piece(sampler,config,out,preview,original):
    return render(sampler,load_score(config[0]),out,preview,original,encode_vorbis)
