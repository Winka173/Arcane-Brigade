# Tools/Audio

## Current production entry: Midnight Toybox Band

Use `setup_toybox.py`, then `build_toybox.py --install`, then `toybox_preview.py`.
See `Docs/Audio/Toybox_Delivery.md` for the complete reproducible workflow,
offline dependency licenses, new hooks, validation and listening limitations.
The legacy scripts below produce the preceding v2 palette.

## Previous palette: Neon Carnival Funk v2

Build-time tổng hợp âm thanh Arcane Brigade — bộ "Neon Carnival Funk" v2 (Python 3, numpy, scipy, soundfile; không cần ffmpeg). Nhạc cụ dùng chung ở `kit.py`; hướng âm thanh ở `Docs/Audio/Audio_Direction_Palette.md`.

```
pip install numpy scipy soundfile
python3 Tools/Audio/build_sfx.py        # wav + cues.json   (~15 giây)
python3 Tools/Audio/build_music.py      # 15 bản x 4 stem ogg + music.json (~2–3 phút; truyền id để render một bài, -j N để chạy song song)
python3 Tools/Audio/audit_harmony.py    # liệt kê nốt giai điệu nghịch với hợp âm (kiểm tra bản phổ, không phải nghe)
python3 Tools/Audio/write_meta.py       # .meta với GUID ổn định + import settings
python3 Tools/Audio/verify_audio.py     # kiểm kỹ thuật (KHÔNG phải nghe)
python3 Tools/Audio/render_previews.py  # PreviewExports/Audio
```
Chạy lại `build_*` sau khi sửa script sẽ ghi đè wav/ogg nhưng giữ nguyên `.meta`. Đừng chạy khi Unity đang import thư mục này. Các file wav/ogg được Git LFS theo `.gitattributes`.

`python3 Tools/Audio/gen_contract.py` sinh lại `Docs/Audio/Audio_Integration_Contract.md` (chú thích tay nằm trong script và `contract_head.md`/`contract_tail.md`).
`build_sfx.py <cue.id> ...` chỉ ghi lại các cue được nêu (manifest luôn đầy đủ), để không làm đổi các binary LFS khác.
