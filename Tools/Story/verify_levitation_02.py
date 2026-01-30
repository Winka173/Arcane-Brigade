"""Check fresh Windows captures against the exact tested player, including old shop choreography."""
import hashlib,json
from datetime import datetime
from pathlib import Path
from PIL import Image
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'PreviewExports/Levitation02'
def read(p):return (ROOT/p).read_text(encoding='utf-8-sig')
def sha(p):return hashlib.sha256((ROOT/p).read_bytes()).hexdigest()
act=json.loads(read('PreviewExports/ActOne/Verification.json'))
assert act['status']=='PASS'
for p,digest in act['playerContentSha256'].items():assert sha(p)==digest,p
build_time=(ROOT/'PreviewExports/ActOne/Build.txt').stat().st_mtime
cases=json.loads(read('PreviewExports/Levitation02/Capture_Runs.json'))
assert len(cases)==2
for c in cases:
    start=datetime.fromisoformat(c['Started']).timestamp();finish=datetime.fromisoformat(c['Finished']).timestamp()
    assert c['ExitCode']==0 and start>=build_time
    folder=OUT/'Shop'/c['Name'];checks=folder/'Native_Checks.txt'
    lines=checks.read_text(encoding='utf-8-sig').splitlines()
    assert len(lines)==c['Checks'] and all(x.startswith('PASS ') for x in lines)
    assert checks.stat().st_mtime>=start
    assert lines[-1]=='PASS Shop presentation native review complete'
    images=list(folder.glob('*.png'));assert images and all(start<=p.stat().st_mtime<=finish+2 for p in images)
    for p in images:
        with Image.open(p) as im:
            assert im.size==(1920,1080),p
            pixels=im.convert('RGB')
            visible=sum(max(pixels.getpixel((x,y)))>30 for y in range(0,im.height,24) for x in range(0,im.width,24))
            assert visible>60,('Black native capture',p)
    c['Images']=len(images)
focused=read('PreviewExports/Levitation/Focused_Checks.txt').splitlines()
assert focused and all(x.startswith('PASS ') for x in focused)
assert build_time-1800<=(ROOT/'PreviewExports/Levitation/Focused_Checks.txt').stat().st_mtime<=build_time
names=['DreamLevitation','FloatingStoryProp','NightLanternField','SharedLightField','WaitingRealmStage','WaitingRoomController','WaitingShopEncounter','LivingScenery','NightmareGrandStage','NightmareFarewellRoom','NightmarePresentation','ShopDisplayActor','OctoberScenery','CoinVisual','OutsideStageChoice','SuspendedScenery']
sources=['Assets/ArcaneBrigade/Runtime/Presentation/'+n+'.cs' for n in names]
sources+=['Assets/ArcaneBrigade/Runtime/Core/ShopPresentationCapture.cs','Assets/ArcaneBrigade/Runtime/Core/MienChoCapture.cs','Tools/Story/Capture_Levitation_02.ps1','Tools/Story/verify_levitation_02.py']
report=dict(status='PASS',scope='Shared levitation revision, missing Act I props, under-object wind removed.',build=act['build'],totalChecks=act['totalChecks']+len(focused)+sum(c['Checks'] for c in cases),totalImages=act['totalImages']+sum(c['Images'] for c in cases),focusedMotionChecks=len(focused),actOneChecks=act['totalChecks'],shopRuns=cases,playerContentSha256=act['playerContentSha256'],sourceSha256={p:sha(p) for p in sources},limitations=['Independent runtime props float. Objects painted into room backdrops cannot move independently.','No under-object smoke or wind ribbons are instantiated. Environmental nightmare fog is a separate system.','Native timing includes capture work and frame cap; no full-campaign subjective art approval is claimed.'])
(OUT/'Verification.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps({k:report[k] for k in ('status','totalChecks','totalImages')},indent=2))
