"""Summarise completed native runs and retain hashes of the tested player and implementation."""
import hashlib,json,re
from datetime import datetime
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'PreviewExports/ActOne'
def read(path):return (ROOT/path).read_text(encoding='utf-8-sig')
def sha(path):return hashlib.sha256((ROOT/path).read_bytes()).hexdigest()
build=read('PreviewExports/ActOne/Build.txt').strip()
assert 'Result: Succeeded' in build and 'Errors: 0' in build
run_sets=[('PreviewExports/ActOne/Capture_Runs.json',2),('PreviewExports/MienCho/Capture_Runs.json',4)]
runs=[]
for path,count in run_sets:
    data=json.loads(read(path));assert isinstance(data,list) and len(data)==count,(path,'incomplete runs')
    base=(ROOT/path).parent
    for case in data:
        assert case['ExitCode']==0
        start=datetime.fromisoformat(case['Started']).timestamp();finish=datetime.fromisoformat(case['Finished']).timestamp()
        assert start>=(OUT/'Build.txt').stat().st_mtime,'Capture predates current build'
        checks=base/case['Name']/'Native_Checks.txt';assert checks.stat().st_mtime>=start
        lines=checks.read_text(encoding='utf-8-sig').splitlines();assert len(lines)==case['Checks'] and all(x.startswith('PASS ') for x in lines),checks
        pictures=list((base/case['Name']).glob('*.png'))
        assert all(start<=p.stat().st_mtime<=finish+2 for p in pictures),'Stale picture'
        runs.append(dict(case,ChecksFile=checks.relative_to(ROOT).as_posix(),Images=len(pictures)))
checks={}
for name,path in [('act_one_editor','PreviewExports/ActOne/Checks.txt'),('story_editor','PreviewExports/MienCho/Checks.txt'),('roster15','PreviewExports/Roster15/Checks.txt'),('recruitment','PreviewExports/Recruitment/Checks.txt'),('hero_identity','PreviewExports/HeroIdentity/Checks.txt')]:
    lines=read(path).splitlines();assert lines and all(x.startswith('PASS ') for x in lines),path;checks[name]=len(lines)
for case in runs:checks[case['ChecksFile']]=case['Checks']
sources=[
 'Assets/ArcaneBrigade/Runtime/Presentation/NightLanternField.cs',
 'Assets/ArcaneBrigade/Runtime/Presentation/SharedLightField.cs',
 'Assets/ArcaneBrigade/Runtime/Presentation/NightmarePresence.cs',
 'Assets/ArcaneBrigade/Runtime/Presentation/NightmarePresentation.cs',
 'Assets/ArcaneBrigade/Runtime/Presentation/WaitingJourney.cs',
 'Assets/ArcaneBrigade/Runtime/Presentation/WaitingRoomController.cs',
 'Assets/ArcaneBrigade/Runtime/Presentation/MienChoPresentation.cs',
 'Assets/ArcaneBrigade/Runtime/Presentation/NightmareDialogue.cs',
 'Assets/ArcaneBrigade/Runtime/Data/WaitingCampaign.cs',
 'Assets/ArcaneBrigade/Runtime/Data/WaitingRooms.cs',
 'Assets/ArcaneBrigade/Runtime/Data/WaitingCompanionQuests.cs',
 'Assets/ArcaneBrigade/Runtime/Data/MienChoStory.cs',
 'Assets/ArcaneBrigade/Runtime/Core/MienChoCapture.cs',
 'Assets/ArcaneBrigade/Runtime/Core/ArenaController.cs',
 'Assets/ArcaneBrigade/Art/Presentation/NightLanternField.shader',
 'Assets/ArcaneBrigade/Art/Presentation/NightFogVolume.shader',
 'Assets/ArcaneBrigade/Art/Presentation/JourneyRoomLight.shader',
 'Assets/ArcaneBrigade/Art/QualityHD/FloorHD.shader',
 'Assets/ArcaneBrigade/Scenes/WaitingRoom.unity',
 'Assets/ArcaneBrigade/Editor/WaitingActOneBuilder.cs',
 'Assets/ArcaneBrigade/Editor/WaitingActOneChecks.cs',
 'ProjectSettings/EditorBuildSettings.asset',
 'Tools/Story/Capture_ActOne.ps1','Tools/Story/Capture_ActOne_All.ps1','Tools/Story/verify_act_one.py']
content=['Builds/HeroIdentity/ArcaneBrigade.exe','Builds/HeroIdentity/ArcaneBrigade_Data/Managed/ArcaneBrigade.Runtime.dll','Builds/HeroIdentity/ArcaneBrigade_Data/globalgamemanagers','Builds/HeroIdentity/ArcaneBrigade_Data/sharedassets0.assets','Builds/HeroIdentity/ArcaneBrigade_Data/resources.assets']
report={'status':'PASS','scope':'Act I / 20 waves. Painted sprites replace Blender art. Later acts deferred.','build':build,'checks':checks,'totalChecks':sum(checks.values()),'totalImages':sum(c['Images'] for c in runs),'captureRuns':runs,'sourceSha256':{p:sha(p) for p in sources},'playerContentSha256':{p:sha(p) for p in content},'limitations':['Drawn sprite lamps and floor use a Unity analytical light/occlusion field. No Blender lamp meshes, full-scene ray tracing or fluid simulation.','Fog integrates eight depth samples at 320x240; reduced effects use four samples and lower density.','Timing covers a diagnostic fixture with limited spawn and includes frame cap; not a hardware-independent full-battle benchmark.','No physical gamepad or subjective full-campaign playtest is claimed.'],'verifiedAtVietnam':max(c['Finished'] for c in runs)}
(OUT/'Verification.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps({k:report[k] for k in ('status','totalChecks','totalImages','build')},indent=2))
