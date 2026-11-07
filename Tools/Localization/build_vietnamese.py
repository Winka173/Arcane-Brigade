"""Refresh the offline Vietnamese phrase catalogue from authored English game text.

Translation downloads happen ONLY when explicitly running this authoring tool.
The game itself has no network translation dependency. Reviewed overrides always win.
Python standard library only; cached downloads allow interrupted runs to resume.
"""
import concurrent.futures
import json
import pathlib
import re
import time
import urllib.parse
import urllib.request
import sys

ROOT = pathlib.Path(__file__).resolve().parents[2]
OUTPUT = ROOT / 'Assets/ArcaneBrigade/Resources/Localization/vi.json'
CACHE = ROOT / 'Logs/Localization/draft.json'
OVERRIDES = pathlib.Path(__file__).with_name('vi_overrides.json')
TOKEN = re.compile(r'//[^\n]*|/\*[\s\S]*?\*/|@"(?:""|[^"])*"|"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'')


def literal(token):
    try:
        if token.startswith('@"'):
            return token[2:-1].replace('""', '"')
        return json.loads(token.replace('\\0', '\\u0000').replace('\\v', '\\u000b').replace('\\a', '\\u0007'))
    except (ValueError, TypeError):
        return ''


def unity_copy(text):
    """Read the string fields in Unity's YAML, including folded multi-line descriptions."""
    lines = text.splitlines()
    for index, line in enumerate(lines):
        match = re.match(r'^(\s*)(?:Name|DisplayName|Description|Text|Title|Line|Hint):[ \t]*(.*)$', line)
        if not match:
            continue
        indent, value = len(match[1]), match[2]
        continuation = []
        for next_line in lines[index + 1:]:
            if next_line.strip() and len(next_line) - len(next_line.lstrip()) <= indent:
                break
            continuation.append(next_line.strip())
        if continuation:
            value += '\n' + '\n'.join(continuation)
        value = value.rstrip()
        # A single line break folds to a space; an empty line is a paragraph break.
        value = re.sub(r'\n+', lambda m: ' ' if len(m[0]) == 1 else '\n' * (len(m[0]) - 1), value)
        if value.startswith('"'):
            # JSON shares Unity's escaped double-quote syntax; literal paragraph breaks must be escaped first.
            value = literal(value.replace('\n', '\\n'))
        elif value.startswith("'") and value.endswith("'"):
            value = value[1:-1].replace("''", "'")
        if value:
            yield value


def candidates():
    found = set()

    def add(value):
        value = value.strip()
        if not re.search('[A-Za-z]{2}', value) or any(ord(c) > 127 and c.isalpha() for c in value):
            return
        if re.fullmatch(r'[\w./:#\\-]+', value):
            # Keep normal words / all-caps headings, never paths, CamelCase identifiers or format specifiers.
            if not value.isalpha() or not (value.islower() or value.isupper() or value.istitle()):
                return
            if len(value) < 3:
                return
        if re.search(r'\.(png|jpg|wav|json|asset|unity|csv|txt|mp4)|[A-Z_]{8,}_|https?://', value):
            return
        found.add(value)

    for path in (ROOT / 'Assets/ArcaneBrigade/Runtime').rglob('*.cs'):
        if 'Capture' in path.name or path.name in ('GameLocalization.cs', 'LocalizedGUI.cs', 'UIUXAuditProbe.cs'):
            continue
        for match in TOKEN.finditer(path.read_text(encoding='utf-8-sig')):
            if match[0].startswith(('"', '@"')):
                value = literal(match[0])
                if path.name == 'VoiceLines.cs' and '|' in value:
                    # VoiceLine.Text is only the spoken field. Delivery notes are authoring metadata.
                    add(value.split('|', 1)[0])
                    continue
                add(value)
                # Hero approach names are authored as a pipe-separated list and rendered individually.
                if '|' in value and 2 <= len(value.split('|')) <= 4 and '\n' not in value and all(len(part) < 40 for part in value.split('|')):
                    for fragment in value.split('|'):
                        add(fragment)
    for path in (ROOT / 'Assets/ArcaneBrigade').rglob('*.asset'):
        text = path.read_text(encoding='utf-8-sig')
        for value in unity_copy(text):
            add(value)
    overrides = json.loads(OVERRIDES.read_text(encoding='utf-8'))
    approaches = json.loads(OVERRIDES.with_name('vi_approaches.json').read_text(encoding='utf-8'))
    for source, target in approaches.items():
        overrides[source] = target
        overrides[source.upper()] = target.upper()
    found.update(overrides)
    return sorted(found), overrides


def translate(batch):
    # Numbered boundaries keep multiline descriptions and short sentence fragments together.
    query = '\n'.join(f'⟦{i:04d}⟧ {text}' for i, text in enumerate(batch))
    url = 'https://translate.googleapis.com/translate_a/single?' + urllib.parse.urlencode(
        {'client': 'gtx', 'sl': 'en', 'tl': 'vi', 'dt': 't', 'q': query})
    for attempt in range(5):
        try:
            request = urllib.request.Request(url, headers={'User-Agent': 'ArcaneBrigade-TranslationAuthoring/1.0'})
            with urllib.request.urlopen(request, timeout=60) as response:
                data = json.load(response)
            translated = ''.join(part[0] for part in data[0] if part[0])
            parts = re.split(r'⟦\s*(\d{4})\s*⟧\s*', translated)
            results = {int(parts[i]): parts[i + 1].strip() for i in range(1, len(parts) - 1, 2)}
            if set(results) != set(range(len(batch))) or not all(results.values()):
                if len(batch) > 1:
                    middle = len(batch) // 2
                    return {**translate(batch[:middle]), **translate(batch[middle:])}
                # Very short snippets can cause the translator to drop the marker.
                plain_url = 'https://translate.googleapis.com/translate_a/single?' + urllib.parse.urlencode(
                    {'client': 'gtx', 'sl': 'en', 'tl': 'vi', 'dt': 't', 'q': batch[0]})
                with urllib.request.urlopen(plain_url, timeout=30) as response:
                    plain = json.load(response)
                return {batch[0]: ''.join(part[0] for part in plain[0] if part[0]).strip()}
            return dict(zip(batch, (results[i] for i in range(len(batch)))))
        except Exception:
            if attempt == 4:
                raise
            time.sleep(2 ** attempt)


def main():
    source, overrides = candidates()
    cache = json.loads(CACHE.read_text(encoding='utf-8')) if CACHE.exists() else {}
    if OUTPUT.exists():
        cache.update({entry['source']: entry['target'] for entry in json.loads(OUTPUT.read_text(encoding='utf-8'))['entries']})
    # Migrate existing drafts that were keyed by "spoken text|delivery note".
    # Split both sides only when their field counts agree; never guess a missing separator.
    voice_source = ROOT / 'Assets/ArcaneBrigade/Runtime/Presentation/VoiceLines.cs'
    for match in TOKEN.finditer(voice_source.read_text(encoding='utf-8-sig')):
        if not match[0].startswith(('"', '@"')):
            continue
        value = literal(match[0])
        if '|' not in value or not value.split('|', 1)[0].strip():
            continue
        draft = cache.get(value, '')
        spoken = value.split('|', 1)[0]
        if spoken not in cache and draft.count('|') == value.count('|'):
            caption = draft.split('|', 1)[0].strip()
            if caption:
                cache[spoken] = caption
    pending = [text for text in source if text not in cache and text not in overrides]
    if '--offline' in sys.argv:
        cache.update(overrides)
        entries = [{'source': text, 'target': cache[text]} for text in sorted(set(source) | set(cache)) if text in cache]
        OUTPUT.write_text(json.dumps({'entries': entries}, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
        print(f'Wrote {len(entries)} offline entries; {len(pending)} unseeded source strings retain the normal English fallback', flush=True)
        return
    batches, batch, size = [], [], 0
    for text in pending:
        if batch and (size + len(text) > 2700 or len(batch) >= 24):
            batches.append(batch)
            batch, size = [], 0
        batch.append(text)
        size += len(text) + 10
    if batch:
        batches.append(batch)
    print(f'{len(source)} phrases; {len(pending)} pending in {len(batches)} batches', flush=True)
    CACHE.parent.mkdir(parents=True, exist_ok=True)
    with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:
        futures = {pool.submit(translate, batch): batch for batch in batches}
        for count, future in enumerate(concurrent.futures.as_completed(futures), 1):
            cache.update(future.result())
            CACHE.write_text(json.dumps(cache, ensure_ascii=False, indent=2), encoding='utf-8')
            if count % 10 == 0 or count == len(batches):
                print(f'{count}/{len(batches)} batches completed', flush=True)
    cache.update(overrides)
    entries = [{'source': text, 'target': cache[text]} for text in source]
    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    OUTPUT.write_text(json.dumps({'entries': entries}, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(f'Wrote {len(entries)} offline translations to {OUTPUT}', flush=True)


if __name__ == '__main__':
    main()
