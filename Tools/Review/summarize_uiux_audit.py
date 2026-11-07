"""Summarize synthetic native hover observations, without controlling any device input."""
import json
from pathlib import Path
from PIL import Image, ImageChops, ImageStat

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'PreviewExports/UIUXAuditSynthetic'
OUT = ROOT / 'Docs/Verification/UIUXAuditMetrics.json'


def main():
    screens = []
    for path in sorted(SOURCE.glob('*-baseline.json')):
        if path.name.startswith('probe-'):
            continue
        if path.stem.removesuffix('-baseline') in {'vi-shop-offer', 'vi-shop-detail', 'vi-hub-ledger', 'vi-hub-clock'}:
            continue  # Replaced shop setup; inactive stations are not independent working screens.
        state = json.loads(path.read_text(encoding='utf-8'))
        image = Image.open(path.with_suffix('.png')).convert('RGB')
        name = path.stem.removesuffix('-baseline')
        observations = []
        for index, control in enumerate(state['controls']):
            hover_json = SOURCE / f'{name}-hover-{index:03}.json'
            if not hover_json.exists():
                continue
            hover = json.loads(hover_json.read_text(encoding='utf-8'))
            hovered_image = Image.open(hover_json.with_suffix('.png')).convert('RGB')
            rect = control['rect']
            x, y, w, h = (rect[k] for k in ('x', 'y', 'width', 'height'))
            box = (max(0, round(x)), max(0, round(y)), min(image.width, round(x + w)), min(image.height, round(y + h)))
            if box[2] <= box[0] or box[3] <= box[1]:
                continue
            difference = ImageChops.difference(image.crop(box), hovered_image.crop(box))
            mean_difference = sum(ImageStat.Stat(difference).mean) / 3
            pointer_matched = any(c['over'] and abs(c['rect']['x'] - x) < 3 and abs(c['rect']['y'] - y) < 3 for c in hover['controls'])
            observations.append({
                'index': index, 'text': control['text'], 'enabled': control['enabled'], 'rect': rect,
                'virtualPointerMatched': pointer_matched,
                'meanPixelDelta': round(mean_difference, 5), 'unchangedPixels': difference.getbbox() is None,
                'caller': control['caller'], 'declaredHoverStyle': control['hoverStyle']
            })
        clipped = [{'text': e['text'], 'fullText': e['tooltip'], 'rect': e['rect']} for e in state['labels'] if e['tooltip']]
        screens.append({
            'screen': name, 'language': state['language'], 'width': state['width'], 'height': state['height'],
            'controls': len(observations), 'enabledControls': sum(c['enabled'] for c in observations),
            'unchangedEnabledRegions': sum(c['enabled'] and c['virtualPointerMatched'] and c['unchangedPixels'] for c in observations),
            'pointerMatchedControls': sum(c['virtualPointerMatched'] for c in observations),
            'shortenedLabels': clipped, 'observations': observations
        })
    result = {
        'method': 'Native Unity render with virtual GUI pointer state only; no OS mouse or keyboard control.',
        'caveat': 'Pixel deltas are observations, not automatic failures. Disabled/selected controls and moving backgrounds require source and screenshot review.',
        'screens': len(screens), 'hoverSamples': sum(s['controls'] for s in screens),
        'pointerMatchedSamples': sum(s['pointerMatchedControls'] for s in screens),
        'unchangedEnabledRegions': sum(s['unchangedEnabledRegions'] for s in screens),
        'results': screens
    }
    OUT.parent.mkdir(parents=True, exist_ok=True)
    OUT.write_text(json.dumps(result, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(json.dumps({k: v for k, v in result.items() if k != 'results'}, ensure_ascii=True))


if __name__ == '__main__':
    main()
