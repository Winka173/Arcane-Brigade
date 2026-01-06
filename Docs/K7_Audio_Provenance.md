# K7 — audio rework, nguồn và giấy phép

2026-10-14: chủ dự án từ chối music/SFX tổng hợp ban đầu. Không ghi quality approved. Rework chuyển default sang sampled Foley và composed track có CC0; synth mới chỉ là study/fallback, không mô tả bản licensed là nhạc độc quyền của project.

## Default runtime

- 33 cue: từ [Kenney Impact Sounds](https://kenney.nl/assets/impact-sounds), [RPG Audio](https://kenney.nl/assets/rpg-audio), [Interface Sounds](https://kenney.nl/assets/interface-sounds). Trang tác giả xác nhận CC0; License.txt nguyên bản của mỗi pack nằm trong Resources/Audio/Licensed, được copy vào ThirdPartyNotices.
- Nhạc: [(Basically not) Fusion Jazz](https://opengameart.org/content/basically-not-fusion-jazz), Julie Damsgaard / Spring Spring / Spring Enterprises. Trang upload của tác giả ghi CC0. File fusion jazz_0.ogg giữ nguyên bytes dưới tên FusionJazz.ogg, stream stereo trong game; shop pitch0.9, boss1.035. Không claim live band hoặc composition riêng cho Arcane Brigade.
- Không trích soundtrack/SFX của Bizarre Brigade. Không ghép stems của hòa âm khác lên licensed track. Voice danger/ducking/density và3bus vẫn chạy trên actual clips.

## Mapping cue sang file tác giả

00 knifeSlice;01 bookFlip1;02 glass_004;03 impactPunch_medium_000;04 handleCoins2;05 impactSoft_heavy_000;06 impactPlate_heavy_000;07 impactBell_heavy_000;08 chop;09 glass_002;10 metalPot1;11 drawKnife2;12 knifeSlice2;13 click_003;14 cloth2;15 bookPlace1;16 impactWood_heavy_000;17 impactBell_heavy_002;18 pluck_001;19 click_001;20 confirmation_001;21 switch_002;22 error_002;23 confirmation_002;24 question_003;25 error_005;26 bong_001;27 cloth3;28 impactMetal_medium_000;29 impactSoft_medium_000;30 cloth1;31 impactGlass_light_000;32 impactWood_medium_000.

Nguồn OGG được decode bằng FFmpeg thành PCM16 mono44.1kHz để nhập/editor audition. Runtime SFX Vorbis/decompress-on-load, music streaming. Source33WAV/meta, track OGG và credit/license đều nằm trong repo, không chỉ default cache.

## Study / fallback

NightCabaretSound: progression/motif/pluck/brush/filtered-noise nguyên gốc;32bars128beats116BPM, shop100BPM.4studyWAV trong Resources/Audio và audition riêng, **không phải mix mặc định** nếu licensed resources hiện diện. Runtime merge confirmation dùng bus chung, không còn beep MergeConfirmationStudy làm phản hồi mặc định.

## Kiểm tra và giới hạn

Finite/energy/no sample overflow chỉ là kỹ thuật. Không thay việc nghe mix ingame, độ phù hợp/đỡ mệt tai hoặc duyệt soundtrack. Licensed jazz dài hơn study nhưng không phải mỗi phase có một soundtrack bespoke. Art/audio/feel gate không tự PASS do đổi asset.

Font: Be Vietnam Pro / Noto Sans Mono / Coiny (Marcelo Magalhaes) giữ OFL nguyên bản và copy license vào build. Coiny dùng logo/menu display, không thay body nhỏ hoặc số kinh tế. Nguồn [Google Fonts Coiny](https://github.com/google/fonts/tree/main/ofl/coiny).

FFmpeg: qua Windows link của [FFmpeg download](https://ffmpeg.org/download.html#build-windows), chỉ là diagnostic tool trong Logs, không ship binary trong game. GDI HWND đã thử nhưng có OS overlay trong frame, nên clip đó bị loại. Bản bàn giao đọc framebuffer Unity bằng ScreenCapture/AsyncGPUReadback, gửi RGBA qua pipe local sang [rawvideo](https://ffmpeg.org/ffmpeg-formats.html#rawvideo); không dùng desktop capture/microphone. CFR60 giữ wall-clock bằng lặp slot thiếu, báo unique readbacks riêng và không dùng FPS video làm perf proof. Video im lặng, audition riêng.
