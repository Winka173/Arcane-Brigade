"""Build the reviewable image-generation work list from the approved art documents.

This only prepares prompts and file destinations. Images are generated with the
painting pass, never a procedural stand-in or a separate API runner.
"""
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
STORY = 'Assets/ArcaneBrigade/Resources/MienCho/ActOne/'
REFS = ['Assets/ArcaneBrigade/Art/FinalQuality/furniture-00_v1.png',
        'Assets/ArcaneBrigade/Art/AnimationHD/NightMarket_v1.png']
STYLE = ('Use case: style-transfer. Production raster art for Arcane Brigade. '
         'Both input sheets are STYLE REFERENCES ONLY, not edit targets. '
         'Match confident dark ink contours, rounded slightly crooked asymmetric forms, '
         'rich painterly storybook shading, warm antique brass, burgundy velvet, deep teal '
         'and midnight blue, cream highlights. Readable silhouette at gameplay size. '
         'Clearly drawn illustration, never photo, 3D render, PBR or plastic. ')
ISOLATED = (' Elevated front three-quarter game view. ONE complete isolated subject '
            'on genuine transparent RGBA background, centered with 12% empty margin. '
            'No ground plane, cast shadow, external halo, wind/smoke underneath, '
            'lettering, labels, UI, grid or watermark. No gore. Travellers are living '
            'dream creatures, not toys; seated travellers are not enemies. ')
jobs = []

def add(name, path, subject, transparent=True, refs=None, dep=None, mode='single'):
    jobs.append(dict(name=name, path=path, prompt=subject, transparent=transparent,
                     refs=refs or REFS, depends=dep, mode=mode))

master = (ROOT / 'Docs/Art_Master_List.md').read_text(encoding='utf-8-sig')
for line in master.splitlines():
    if not line.startswith('| `'): continue
    cols = [c.strip() for c in line.split('|')[1:-1]]
    match = re.search(r'`([^`]+\.png)`', cols[0])
    if not match: continue
    file = match[1]
    if '/' in file: continue
    name = file[:-4]
    subject = cols[-1]
    if name.startswith('WarmChair_') or name == 'SeatedOne_A_Turned': continue
    if name.startswith('LastCustomer_') or name in ('Hob', 'Latch', 'Tally'): continue
    if name.startswith('Floor_'):
        prompt = STYLE + 'Subject: ' + subject + (' Output a seamless tileable square floor texture, '
            '2048x2048. Flat overhead surface at the same camera angle as existing game arena '
            'floors. Fill the entire opaque canvas, consistent soft ambient illumination. '
            'No furniture, characters, text, cast shadows, perspective vanishing point or vignette. '
            'Edges must tile seamlessly. Materials remain painterly, not photographic.')
        add(name, STORY+file, prompt, False)
    elif name.startswith('Panel_'):
        add(name, STORY+file, STYLE+'Subject: '+subject+(' Wide 16:9 opaque narrative illustration, '
            '1672x941 landscape. No text or UI. Keep the lower middle calm for runtime characters. '
            'Implied psychological horror, no violence against children, no gore.'), False)
    else:
        folder = 'Assets/ArcaneBrigade/Art/ActOneBosses/' if name in ('ChairPorter','DoorThatStayedShut','TheBedside') else STORY
        add(name, folder+file, STYLE+'Subject: '+subject+ISOLATED)

for suffix, detail in [
    ('Coins','Add exactly three small antique coins of different sizes on the folded blanket.'),
    ('Jar','Add one small stoppered glass jar on the blanket, containing a faint pale curl of whisper.'),
    ('Phone','Add one detached old ivory-and-teal telephone receiver on the blanket; loose curled cord ends at the side.')]:
    add('WarmChair_'+suffix, STORY+'WarmChair_'+suffix+'.png',
        'Use case: precise-object-edit. Edit the approved single Warm Chair. '+detail+
        ' Change only that addition. Preserve exact chair, blanket, repaired leg, style, camera, '
        'scale, canvas placement and foot anchor. One sprite, transparent RGBA, no ground, '
        'cast shadow, exterior glow, smoke, text or extra object.', refs=[STORY+'WarmChair.png'], dep='WarmChair')
add('SeatedOne_A_Turned', STORY+'SeatedOne_A_Turned.png',
    'Use case: precise-object-edit. Edit the seated traveller A. Turn ONLY the head over '
    'the shoulder toward the viewer. Face is a soft featureless dusty oval, two faint '
    'dark hollows. Preserve exact body, chair, hands, dust, style, camera, scale, canvas '
    'placement and floor anchor. One complete isolated sprite, genuine transparent background. '
    'No ground, shadow, text, glow, horror teeth, blood or violence.',
    refs=[STORY+'SeatedOne_A.png'], dep='SeatedOne_A')

merchant = next(line for line in master.splitlines() if line.startswith('| 1 (idle)'))
add('Shopkeeper_Idle', STORY+'Shopkeeper_Idle.png',
    STYLE+'Subject: '+merchant.split('|')[-2].strip()+ISOLATED)
for name, subject in [('Pour','The same gentle shopkeeper pouring tea from a small brass kettle.'),
                      ('Wipe','The same shopkeeper wiping a glass with a cloth, tired kind eyes still turned away.'),
                      ('Warn','The same shopkeeper with a worried look, one hand raised slightly as if to say not that one.')]:
    add('Shopkeeper_'+name, STORY+'Shopkeeper_'+name+'.png',
        'Use case: precise-object-edit. Edit the approved shopkeeper sprite. '+subject+
        ' Preserve exact creature design, spectacles, counter belly, clothing, scale, camera, '
        'canvas and foot anchor. Change only the requested gesture/expression. Complete '
        'isolated sprite on genuine transparent RGBA. No floor, shadow, lettering, extra '
        'creature, oversized toothy grin or billowing tongue.',
        refs=[STORY+'Shopkeeper_Idle.png'], dep='Shopkeeper_Idle')

token_text = (ROOT/'Docs/Hero_Mechanic_Art_Requests.md').read_text(encoding='utf-8-sig')
for match in re.finditer(r'Target: `([^`]+)`.*?```[^\n]*\n(.*?)```', token_text, re.S):
    path, prompt = match[1], match[2].strip()
    if path.endswith('.png'): add(Path(path).stem, path, prompt)

# The VFX document contains one full prompt per base asset. Preserve those prompts.
vfx = (ROOT / 'Docs/Combat_VFX_Art_Requests.md').read_text(encoding='utf-8-sig')
for block in re.finditer(r'```[^\n]*\n(.*?)```', vfx, re.S):
    prompt = block[1].strip()
    if 'Subject:' not in prompt or not prompt.startswith('Create a 2D'): continue
    before = vfx[:block.start()]
    heading = list(re.finditer(r'^### ([HAJ]\w*)[^\n]*', before, re.M))
    if not heading: continue
    section = before[heading[-1].start():]
    # The first target line owns the asset. Later variant snippets must not
    # accidentally turn the destination into a bare "f3_v1.png".
    target_lines = re.findall(r'^Target: `([^`]+)`', section, re.M)
    targets = target_lines or re.findall(r'`([^`]+\.png[^`]*)`', section)
    if not targets: continue
    raw = targets[0] if target_lines else targets[-1]
    special_names = re.findall(r'\*\*[^\n]+\*\*\s*\(`([^`]+\.png)`\)', section)
    if special_names: raw = special_names[-1]
    if '{' in raw:
        families = re.findall(r'`(Mutation_[0-7]_a_v1\.png)`', section)
        if families: raw = families[-1]
    raw = re.sub(r'_f0\.\.f[0-9]', '_f0', raw)
    files = re.findall(r'[A-Za-z][A-Za-z0-9_]*\.png', raw)
    if not files: continue
    file = files[0]
    full_targets = re.findall(r'Assets/ArcaneBrigade/Art/CombatVFX/(?:Hostile|Allied|Juice)/', section)
    if not full_targets: continue
    folder = full_targets[-1]
    if '{' in raw: continue
    add(file[:-4], folder+file, prompt)

# Optional lane telegraph has a separate subject rather than a full repeated block.
circle = next(j for j in jobs if j['name'] == 'TelegraphCircle_v1')
lane = re.search(r'Lane variant .*?Subject\): ([^\n]+)', vfx)
if lane:
    prompt = re.sub(r'Subject:.*?\n\nStyle:', 'Subject: '+lane[1]+'\n\nStyle:', circle['prompt'], flags=re.S)
    add('TelegraphLane_v1', circle['path'].replace('TelegraphCircle','TelegraphLane'), prompt)

# Stable IDs prevent duplicate requests when the manifest is regenerated.
unique = {j['path']: j for j in jobs}
out = ROOT/'Docs/Art_Master_Worklist.json'
if out.exists():
    previous = json.loads(out.read_text(encoding='utf-8-sig'))
    if isinstance(previous, dict): previous = previous['jobs']
    # Approved pose edits and reviewed prompts take precedence over another parsing pass.
    unique.update({job['path']: job for job in previous})
out.write_text(json.dumps(sorted(unique.values(), key=lambda j: j['path']), ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
print('Prepared/preserved', len(unique), 'explicit art requests, including approved animation edits.')
