"""Read-only audit of delivered Unity captures and original generated alpha sources."""
from pathlib import Path
from PIL import Image
import hashlib
import json

root = Path(__file__).resolve().parents[2]
output = root / 'PreviewExports/BuildMorphs'
report = {'focused_checks': {}, 'native': {}, 'artwork': {}}
focused = (output / 'Focused_Checks.txt').read_text().splitlines()
assert len(focused) == 470 and all(line.startswith('PASS ') for line in focused)
report['focused_checks'] = {'passed': len(focused), 'failed': 0}
for mode in ['Standard', 'Reduced']:
    directory = output / mode
    checks = (directory / 'Native_Checks.txt').read_text().splitlines()
    assert len(checks) == 99 and all(line.startswith('PASS ') for line in checks)
    shots = sorted(directory.glob('*.png'))
    assert len(shots) == 25
    hashes = []
    for shot in shots:
        with Image.open(shot) as image:
            assert image.size == (1920, 1080), shot
        hashes.append(hashlib.sha256(shot.read_bytes()).hexdigest())
    assert len(set(hashes)) == 25
    report['native'][mode] = {'passed': len(checks), 'failed': 0, 'captures': [
        {'path': shot.relative_to(root).as_posix(), 'sha256': digest, 'width': 1920, 'height': 1080}
        for shot, digest in zip(shots, hashes)]}
for name in ['BackAttachments_v3.png', 'Charms_v2.png']:
    path = root / 'Assets/ArcaneBrigade/Art/BuildMorphs' / name
    with Image.open(path) as image:
        assert image.mode == 'RGBA'
        histogram = image.getchannel('A').histogram()
        assert histogram[0] > image.width * image.height * .4
        report['artwork'][name] = {'path': path.relative_to(root).as_posix(), 'size': image.size,
            'fully_transparent_pixels': histogram[0], 'sha256': hashlib.sha256(path.read_bytes()).hexdigest()}
(output / 'Capture_Audit.json').write_text(json.dumps(report, indent=2) + '\n')
print('BUILD_MORPH_CAPTURE_AUDIT: 470 focused checks; 198 native checks; 50 distinct 1920x1080 captures; two original RGBA sources.')
