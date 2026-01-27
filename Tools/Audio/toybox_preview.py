"""Offline listening desk and a clearly labelled simulated combat mix, not a game capture."""
import html
import json
from pathlib import Path

import numpy as np
import soundfile as sf

from toybox_engine import ROOT, SR, add
from toybox_music import encode_vorbis

A=ROOT/'Assets/ArcaneBrigade/Resources/Audio/Arcane'
P=ROOT/'PreviewExports/Audio/Toybox'


def main():
    from foley_preview import build as build_foley_previews
    foley_buttons,foley_designs=build_foley_previews()
    pieces=json.loads((A/'Music/music.json').read_text())['pieces']
    cues=json.loads((A/'cues.json').read_text())['cues']
    byid={c['id']:c for c in cues}
    # Synthetic arrangement to audition approximate runtime headroom/density.
    # It is explicitly labelled; it is not offered as in-game listening evidence.
    music,_=sf.read(str(P/'combat_toys.ogg'),always_2d=True)
    n=SR*24; mix=music[np.arange(n)%len(music)]*.3
    events=[]
    for k in range(80):
        at=k*.28
        events.append(('cmb.swing.blade',at,k))
        events.append(('cmb.hit.'+['wood','paper','metal','fabric','wax','glass'][k%6],at+.09,k))
        if k%3==0: events.append(('loot.coin.pickup',at+.17,k))
        if k%7==0: events.append(('enemy.die.wood',at+.21,k))
    events += [('event.warn',5,0),('enemy.shot.burst',8,0),('plr.hurt',12,0),('up.merge.gather',16,0),('up.merge',16.54,0),('up.star3',16.61,0)]
    for cid,at,i in events:
        c=byid[cid]; rel=c['clips'][i%len(c['clips'])]
        x,_=sf.read(str(A/(rel+'.wav')))
        add(mix,x,at,10**(c['gainDb']/20)*.7,pan=(i%3-1)*.2)
    sf.write(str(P/'Masters/combat_mix_simulation.wav'),mix,SR,subtype='PCM_24')
    encode_vorbis(P/'Masters/combat_mix_simulation.wav',P/'combat_mix_simulation.ogg')
    ui=[]
    scores=json.loads((ROOT/'Docs/Audio/Toybox_Scores.json').read_text(encoding='utf-8'))
    for pc in pieces:
        identity=scores.get(pc['id'],{}).get('identity','')
        ui.append(f'<button class="track" data-src="{pc["id"]}.ogg"><b>{html.escape(pc["title"])}</b><small>{pc["bpm"]} BPM · {pc["beatsPerBar"]}/4 · {pc["bars"]} bars · {pc["loopSeconds"]:.2f}s · {html.escape(pc["scenes"][0])}</small><small>{html.escape(identity)}</small></button>')
    # Quick sequential comparisons make samey writing easier to notice. Every
    # excerpt is taken from its own new scene mix, with 0.4s between songs.
    reel=[];reel_sections=[];cursor=0
    for pc in pieces:
        x,_=sf.read(str(P/(pc['id']+'.ogg')),always_2d=True)
        excerpt=x[:SR*8].copy()
        ramp=np.linspace(0,1,round(SR*.015))[:,None]
        excerpt[:len(ramp)]*=ramp;excerpt[-len(ramp):]*=ramp[::-1]
        reel_sections.append({'piece':pc['id'],'title':pc['title'],'at':cursor})
        reel.extend([excerpt,np.zeros((round(SR*.4),2))]);cursor+=len(excerpt)/SR+.4
    sf.write(str(P/'Masters/individual_themes_comparison.wav'),np.concatenate(reel),SR,subtype='PCM_24')
    encode_vorbis(P/'Masters/individual_themes_comparison.wav',P/'individual_themes_comparison.ogg')
    (P/'comparison_timeline.json').write_text(json.dumps(reel_sections,indent=2)+'\n')
    cue_data=[{'id':c['id'],'bus':c['bus'],'danger':c['danger'],'clips':c['clips'],
               'identity':foley_designs[c['id']]['identity']} for c in cues]
    page='''<!doctype html><html lang="vi"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
<title>Midnight Toybox Band · Listening Desk</title><style>
*{box-sizing:border-box}body{margin:0;background:#10121c;color:#eee8da;font:16px system-ui,sans-serif}main{max-width:1140px;margin:auto;padding:40px 24px 100px}h1{font-size:42px;margin:8px 0 16px}h2{margin-top:34px}p{color:#b7bbc8;line-height:1.6}button,input,select{font:inherit}button{cursor:pointer;color:inherit;background:#222638;border:1px solid #3a4056;border-radius:10px;padding:12px;text-align:left}button:hover,button.active{border-color:#f1bd65;background:#303044}.grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(245px,1fr));gap:10px}small{display:block;color:#b7bbc8;margin-top:8px}.player{position:sticky;top:0;background:#181c2af5;border:1px solid #44475d;padding:18px;border-radius:12px;z-index:3}.player audio{width:100%;margin-top:10px}.gold{color:#f1bd65}.tools{display:flex;gap:12px;flex-wrap:wrap;margin:18px 0}input,select{background:#222638;color:inherit;padding:10px;border:1px solid #44475d;border-radius:8px}#q{flex:1;min-width:220px}.cue{display:flex;align-items:center;justify-content:space-between;padding:12px;border-bottom:1px solid #303448;gap:12px}.cue button{padding:6px 10px;margin:3px}label{color:#c7c9d2}a{color:#f1bd65}.note{border-left:3px solid #806d4a;padding-left:15px}.badge{font-size:12px;color:#f1bd65;letter-spacing:2px}#fx{width:100%}</style>
<main><span class="badge">ARCANE BRIGADE · ORIGINAL SOUND COLLECTION</span><h1>Midnight <span class="gold">Toybox Band</span></h1>
<p>Revision 4 · 15 bài soạn riêng từ đầu · 60 stem đồng bộ. Mỗi bài có hòa âm, bass, câu giai điệu, pattern trống và cấu trúc riêng. Không sử dụng các MP3 studio trong repo.</p>
<div class="player"><b id="now">Chọn một bản nhạc bên dưới</b><audio id="music" controls preload="none" loop></audio><label><input id="loop" type="checkbox" checked> Lặp nhạc để kiểm tra điểm nối</label><small>Âm lượng khởi điểm 65%. Hãy kiểm tra thêm bằng tai nghe và loa ở mức nghe quen thuộc.</small></div>
<h2>So sánh 15 chủ đề</h2><button class="track" data-src="individual_themes_comparison.ogg"><b>Nghe liên tiếp 15 hook mới · 8 giây mỗi bài</b><small>Theo thứ tự playlist bên dưới; nghỉ 0,4 giây giữa các bài. Bỏ chọn lặp nếu muốn nghe một lượt.</small></button>
<h2>Soundtrack · từng bài hoàn chỉnh</h2><div class="grid">TRACKS</div>
<p class="note">Preview dùng mix theo scene; menu/select không có intensity. Mỗi bài có bản phổ JSON riêng trong Tools/Audio/Scores, bản MIDI chỉnh sửa được trong MIDI và master WAV 24-bit trong Masters (local). Tempo và độ dài mới đã cập nhật vào manifest game.</p>
<button class="track" data-src="combat_mix_simulation.ogg"><b>Combat mix simulation · 24 giây</b><small>Mô phỏng nhạc + SFX ở mật độ cao; đây không phải bản ghi âm trong game.</small></button>
<h2>Sound effects & voices</h2><audio id="fx" controls preload="none"></audio><div class="tools"><input id="q" type="search" placeholder="Tìm cue: merge, coin, boss, voice…" aria-label="Tìm cue"><select id="bus" aria-label="Audio bus"><option value="">Tất cả bus</option><option>Combat</option><option>UI</option><option>Foley</option><option>Voice</option><option>Ambience</option></select><label><input id="fxloop" type="checkbox"> Lặp clip</label></div><p id="count"></p><div id="cues"></div>
<h2>Kiểm tra</h2><p><a href="QA_Report.json">QA audio</a> · <a href="Composition_Audit.json">Đối chiếu 105 cặp bản phổ</a> · <a href="../../../Docs/Audio/Toybox_Individual_Compositions.md">Thiết kế từng bài</a> · <a href="../../../Docs/Audio/Toybox_Delivery.md">Bàn giao</a></p><p>Đối chiếu bản phổ kiểm tra các chuỗi quãng nốt và nhịp, hòa âm, chart trống. Các phép đo không chứng minh bài nhạc hấp dẫn hoặc hoàn toàn khác nhau khi nghe. Cần nghe thử trong Unity để đánh giá cân bằng thực tế và độ mệt tai khi chơi lâu.</p>
</main><script>const cues=CUE_DATA;
const player=document.querySelector('#music'),fx=document.querySelector('#fx');player.volume=.65;fx.volume=.5;
document.querySelectorAll('.track').forEach(b=>b.onclick=()=>{document.querySelectorAll('.track').forEach(x=>x.classList.remove('active'));b.classList.add('active');player.src=b.dataset.src;document.querySelector('#now').textContent=b.querySelector('b').textContent;player.play().catch(()=>{});});
document.querySelector('#loop').onchange=e=>player.loop=e.target.checked;document.querySelector('#fxloop').onchange=e=>fx.loop=e.target.checked;
function show(){let q=document.querySelector('#q').value.toLowerCase(),bus=document.querySelector('#bus').value;let list=cues.filter(c=>c.id.includes(q)&&(!bus||bus===c.bus));document.querySelector('#count').textContent=list.length+' cue';let box=document.querySelector('#cues');box.replaceChildren();for(let c of list){let row=document.createElement('div');row.className='cue';let label=document.createElement('div');let title=document.createElement('b');title.textContent=c.id;let sub=document.createElement('small');sub.textContent=c.bus+' · '+c.clips.length+' biến thể'+(c.danger?' · DANGER':'');label.append(title,sub);let controls=document.createElement('div');c.clips.forEach((src,i)=>{let b=document.createElement('button');b.textContent=String(i+1).padStart(2,'0');b.ariaLabel=c.id+' variation '+(i+1);b.onclick=()=>{fx.src='../../../Assets/ArcaneBrigade/Resources/Audio/Arcane/'+src+'.wav';fx.play().catch(()=>{});};controls.append(b);});row.append(label,controls);box.append(row);}}
document.querySelector('#q').oninput=show;document.querySelector('#bus').onchange=show;show();</script></html>'''
    page=page.replace('TRACKS',''.join(ui)).replace('CUE_DATA',json.dumps(cue_data))
    page=page.replace('<h2>Sound effects & voices</h2>',
        '<h2>Foley v5 · 582 clip mới</h2><p>Thiết kế riêng 206 cue; mô hình vật liệu, dây, tiếp xúc, hơi và giọng formant. Không dùng audio mẫu hoặc studio. Chọn một nhóm để nghe lần lượt, hoặc nghe từng biến thể bên dưới.</p><div class="grid">'+foley_buttons+'</div><p><a href="Foley_Timelines.json">Mốc từng cue trong reel</a> · <a href="Foley_Audit.json">Kiểm tra 582 clip thay thế</a></p><h2>Sound effects & voices</h2>')
    page=page.replace("label.append(title,sub);", "let design=document.createElement('small');design.textContent=c.identity;label.append(title,sub,design);")
    (P/'index.html').write_text(page,encoding='utf-8')
    (P/'playlist.m3u').write_text('#EXTM3U\n'+'\n'.join(p['id']+'.ogg' for p in pieces)+'\n',encoding='utf-8')
    print('Listening desk:',P/'index.html')


if __name__=='__main__': main()
