# Tools/Audio

Build-time tổng hợp âm thanh Arcane Brigade (Python 3, numpy, scipy, ffmpeg có libvorbis).

```
pip install numpy scipy
python3 Tools/Audio/build_sfx.py        # wav + cues.json   (~1 phút)
python3 Tools/Audio/build_music.py      # 15 bản x 4 stem ogg + music.json (~15 phút; có thể truyền id: build_music.py shop_gold)
python3 Tools/Audio/write_meta.py       # .meta với GUID ổn định + import settings
python3 Tools/Audio/verify_audio.py     # kiểm kỹ thuật (KHÔNG phải nghe)
python3 Tools/Audio/render_previews.py  # PreviewExports/Audio
```
Chạy lại `build_*` sau khi sửa script sẽ ghi đè wav/ogg nhưng giữ nguyên `.meta`. Đừng chạy khi Unity đang import thư mục này. Các file wav/ogg được Git LFS theo `.gitattributes`.

`python3 Tools/Audio/gen_contract.py` sinh lại `Docs/Audio/Audio_Integration_Contract.md` (chú thích tay nằm trong script và `contract_head.md`/`contract_tail.md`).
`build_sfx.py <cue.id> ...` chỉ ghi lại các cue được nêu (manifest luôn đầy đủ), để không làm đổi các binary LFS khác.
