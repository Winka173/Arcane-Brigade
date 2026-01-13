"""Writes Unity .meta files (stable GUIDs derived from the asset path) with import settings per asset class,
so imports are identical on every machine without opening the editor. Never overwrites an existing .meta."""
import hashlib, os, wave, sys
ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
A = os.path.join(ROOT, 'Assets', 'ArcaneBrigade', 'Resources', 'Audio', 'Arcane')

def guid(rel): return hashlib.md5(('arcane-audio:' + rel).encode()).hexdigest()

def folder_meta(rel): return ("fileFormatVersion: 2\nguid: %s\nfolderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n" % guid(rel))
def text_meta(rel): return ("fileFormatVersion: 2\nguid: %s\nTextScriptImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n" % guid(rel))

def audio_meta(rel, load, quality, background, note):
    # loadType: 0 DecompressOnLoad, 1 CompressedInMemory, 2 Streaming. compressionFormat 1 = Vorbis.
    return ("fileFormatVersion: 2\nguid: %s\nAudioImporter:\n  externalObjects: {}\n  serializedVersion: 8\n  defaultSettings:\n"
            "    serializedVersion: 2\n    loadType: %d\n    sampleRateSetting: 0\n    sampleRateOverride: 44100\n    compressionFormat: 1\n"
            "    quality: %s\n    conversionMode: 0\n    preloadAudioData: %d\n  platformSettingOverrides: {}\n  forceToMono: 0\n  normalize: 0\n"
            "  loadInBackground: %d\n  ambisonic: 0\n  3D: 0\n  userData: %s\n  assetBundleName: \n  assetBundleVariant: \n"
            % (guid(rel), load, quality, 0 if load == 2 else 1, background, note))

def put(path, text):
    if os.path.exists(path + '.meta'): return
    open(path + '.meta', 'w', newline='\n').write(text)

def main():
    put(A, folder_meta('Arcane'))
    for d, _, files in os.walk(A):
        rel_d = os.path.relpath(d, A)
        if rel_d != '.': put(d, folder_meta('Arcane/' + rel_d))
        for f in files:
            if f.endswith('.meta'): continue
            p = os.path.join(d, f); rel = 'Arcane/' + os.path.relpath(p, A).replace(os.sep, '/')
            if f.endswith('.json'): put(p, text_meta(rel)); continue
            top = rel.split('/')[1]
            if top == 'Music': put(p, audio_meta(rel, 2, '0.85', 1, 'Arcane music stem; see Docs/Audio/Provenance.md'))
            elif top == 'Amb': put(p, audio_meta(rel, 1, '0.6', 1, 'Arcane ambience loop; seamless'))
            else:
                with wave.open(p) as w: dur = w.getnframes() / w.getframerate()
                put(p, audio_meta(rel, 0 if dur < 0.4 else 1, '0.8', 0, 'Arcane SFX/voice; see Docs/Audio/Provenance.md'))
if __name__ == '__main__': main()
