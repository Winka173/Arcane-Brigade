"""Verify the replacement art and the fresh native gates from the same Windows player."""
import hashlib,json
from pathlib import Path
from PIL import Image
ROOT=Path(__file__).resolve().parents[2]
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
manifest=json.loads((ROOT/'Docs/Mien_Cho_Painted_Art_Prompts.json').read_text(encoding='utf-8'))
assert len(manifest['assets'])==13
for a in manifest['assets']:
    p=ROOT/a['path'];assert sha(p)==a['sha256'],a['name']
    im=Image.open(p);assert list(im.size)==a['size']
    if a['transparent']:assert im.mode=='RGBA' and im.getchannel('A').histogram()[0]>im.width*im.height*.08,a['name']
lanterns=ROOT/'Assets/ArcaneBrigade/Resources/MienCho/Lanterns'
assert not list(lanterns.glob('*.fbx')) and not list(lanterns.glob('*.prefab'))
assert len(list(lanterns.glob('*.png')))==3
journey=ROOT/'Assets/ArcaneBrigade/Resources/MienCho/Journey'
assert len(list(journey.glob('*.png')))==3
gate=json.loads((ROOT/'PreviewExports/Levitation02/Verification.json').read_text(encoding='utf-8'))
assert gate['status']=='PASS'
for p,digest in gate['playerContentSha256'].items():assert sha(ROOT/p)==digest,p
checks=(ROOT/'PreviewExports/ActOne/Checks.txt').read_text(encoding='utf-8').splitlines()
assert sum('Retired Blender lamp prefab excluded' in s for s in checks)==3
for profile in ['Native','Reduced']:
    lines=(ROOT/'PreviewExports/ActOne'/profile/'Native_Checks.txt').read_text(encoding='utf-8').splitlines()
    assert all(s.startswith('PASS ') for s in lines)
    assert any('Only drawn lamp sprites' in s for s in lines)
    assert any('No under-object wind' in s for s in lines)
out=ROOT/'PreviewExports/PaintedMienCho';out.mkdir(parents=True,exist_ok=True)
gate.update(scope='13 illustrated replacement assets; no Blender mesh lamp resources; shared levitation without under-object wind.',artAssets=manifest['assets'],promptManifest='Docs/Mien_Cho_Painted_Art_Prompts.json')
(out/'Verification.json').write_text(json.dumps(gate,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps(dict(status='PASS',assets=13,checks=gate['totalChecks'],images=gate['totalImages']),indent=2))
