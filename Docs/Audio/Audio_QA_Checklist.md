# Audio QA checklist và trạng thái bàn giao

## Cập nhật tích hợp offline — 2026-10-14

- Đã import bộ handoff: 153 cue / 471 clip; 15 bản nhạc / 60 stem.
- Hợp đồng sinh lại từ source: 141 cue có hook, 0 pending, 12 reserved có lý do.
- Unity compile/build và kiểm tra nguồn/import đã chạy tại máy Windows;
  xem `PreviewExports/AudioHandoff` cho kết quả thực tế.
- Playback tự động kiểm clip mới, simulation pause không dùng timeScale,
  delayed UI/world, hủy lịch cũ, bốn stem select đồng bộ và crossfade dream shop.
- Người dùng đã duyệt preview nhạc trước đó. Agent chưa nghe các phối hợp mới
  trong game; không có video có tiếng, chưa nghiệm thu cảm giác/mix mới.
- Các mục bên dưới là bản checklist bàn giao lịch sử, không phải số liệu mới.

Quy ước: ✅ đã làm và kiểm bằng máy · 🟡 đã viết/tích hợp nhưng **chưa chạy Unity** · ⬜ **chưa nghe/chưa test** (cần người).

Môi trường tạo ra bản này: Linux headless, không Unity, không loa, không quay được video gameplay. **Không có video in-game có tiếng trong bản bàn giao này.** Thay vào đó có preview offline ở `PreviewExports/Audio/` (mix từng bản nhạc + sfx_reel có mốc thời gian).

## 0. Cảnh báo bàn giao: file nhị phân chưa được push
`git push` bị chặn ở bước Git LFS (`lfs.github.com … /verify: Forbidden`, chính sách mạng của môi trường cloud). Vì vậy commit **không chứa** wav/ogg (Sfx/Voice/Amb/Music stems, PreviewExports) và `.meta` của chúng. Manifest (`cues.json`, `music.json`) có mặt; thiếu clip thì game **tự rơi về audio cũ**.
Để có đủ asset: trên máy có Unity chạy `Tools/Audio/README.md` (build_sfx ~1 phút, build_music ~10 phút, write_meta, render_previews; kết quả xác định) rồi `git add` (LFS tự theo `.gitattributes`). Hoặc cho phép host LFS trong Network access của environment và push lại.

## A. Đã tích hợp (code)
- 🟡 `ArcaneCueLibrary` + `cues.json`/`music.json`; fallback về audio cũ nếu thiếu.
- 🟡 `CheckpointAudio`: phát theo id, cooldown/maxVoices theo cue, chiếm lane theo priority, pitch jitter, không lặp biến thể liền kề.
- 🟡 `ArcadeMusic`: 4 stem đồng bộ dspTime, crossfade theo vạch nhịp, intensity theo mật độ, pause/focus/reduced, ambience bed, sting wave clear/victory/defeat/boss.enter.
- 🟡 `MerchantVoice`: gibberish theo từ/dấu câu, cap 14 syllable/câu; `Gesture()` có sẵn nhưng chưa ai gọi.
- 🟡 Một dòng hook audio-only ở `ArenaController.Update`; một dòng ở `WorldShop` (shopkeeper xuất hiện).
- 🟡 `K8AudioChecks` đặt `ArcaneCueLibrary.Enabled=false` để vẫn kiểm đường audio cũ.
- ✅ Cú pháp C# đã parse (tree-sitter) không lỗi; **chưa biên dịch** (không có Unity/dotnet) nên lỗi kiểu/tham chiếu vẫn có thể có.

## B. Chỉ là asset (đã sinh, đã kiểm kỹ thuật, chưa nối)
Danh sách cue ở mục "Chỉ là asset" của `Audio_Audit_Event_Cue_Map.md`.

## C. Kiểm tra máy đã chạy (`Tools/Audio/verify_audio.py`)
- ✅ 447 clip: không clip (peak <0,999), không im lặng, không trùng nội dung, DC offset, cảnh báo cạnh/click, độ chói trung bình.
- ✅ Loop ambience: không nhảy mẫu ở điểm nối.
- ✅ 36 stem nhạc: độ dài giải mã đúng `loopSamples` ±1, không nhảy ở điểm loop, không clip.
- ⬜ Cái này **không** chứng minh nghe hay.

## D. Cần chạy trong Unity (🟡 → ✅)
1. `Arcane Brigade/Audio/Arcane audio checks` (`ArcaneAudioChecks.cs`): manifest, import, độ dài stem, scene mapping.
2. `Arcane Brigade/K8/Audio source checks` và Playback: đảm bảo đường legacy không vỡ.
3. Mở `Resources/Audio/Arcane` trong Editor để Unity nhận `.meta` đã sinh (import settings: SFX ≤0,4 s DecompressOnLoad Vorbis 0,8; dài hơn CompressedInMemory; ambience CompressedInMemory 0,6; nhạc Streaming 0,85, stereo; SFX/voice mono).
4. Git LFS: kiểm `git lfs ls-files` thấy wav/ogg mới.

## E. Nghe và test trong game (⬜ tất cả)
- ⬜ Menu, chọn nhân vật (cần `ForceScene("select")`).
- ⬜ Combat đông quân wave 4–5+: tiếng đánh hàng loạt có bị dày/mệt không; `cmb.swing.*`, `cmb.hit.*` cooldown/gate có nghe "bị cắt" không.
- ⬜ Boss: `boss.enter`, telegraph, nhạc vào ngay, intensity.
- ⬜ Shop vàng, shop giấc mơ: nhạc riêng, ambience, giọng, chuyển combat → shop → combat trên cùng map (bar-quantized).
- ⬜ Merge, loot, chest, rarity (nhiều cue chưa nối), defeat/victory (im 3,7 s rồi menu).
- ⬜ Clipping thực tế khi nhiều layer + SFX; loop seam nghe thật; pause/resume; Reduced Audio; slider Music/SFX/master.
- ⬜ Cân bằng: `SceneLevel`, `gainDb`, `BusTrim` toàn bộ là khởi điểm.
- ⬜ Hiệu năng/RAM mobile: 36 stem streaming (4–8 stream cùng lúc khi crossfade), 447 clip nhỏ; `Resources.Load` lười lần đầu có thể giật (cân nhắc preload ở màn tải).
- ⬜ Video có âm thanh in-game + playlist preview (playlist offline đã có; video phải quay trên máy có Unity).

## F. Hạn chế đã biết
- Mô hình hóa tổng hợp: chất lượng "nhạc cụ thật" không đạt được; thay bằng thu âm/sample library có license nếu cần.
- 9/12–18 bản nhạc; chưa có theme/map variants, elite, intro/outro rời.
- 447 clip vượt khoảng 150–250 mục tiêu vì phủ rộng; có thể cắt biến thể sau khi nghe.
- Không có AudioMixer asset: "bus" là hệ số phần mềm; ducking/limiter hệ thống chưa có (chỉ duck nhạc khi danger).
- Bizarre Brigade audio: **chưa research bằng tai** (xem Direction §1).
