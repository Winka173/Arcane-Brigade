# Audio provenance — thư viện `Resources/Audio/Arcane`

## Nhạc hiện tại: storybook v5, 2026-10-20

Music revision 5 thay toàn bộ 15 bài (60 stem) bằng các bản soạn mới cho game trong `Tools/Music/tracks.py` (nốt nhạc viết tay, không chép, không sample), render bằng FluidSynth 2.6.1 + GeneralUser GS 2.0.3, mỗi scene mix ở −18 LUFS, loop liền mạch. Công cụ, giấy phép và bảng từng bài: [Music_Credits.md](../Music_Credits.md). Phần nhạc của revision 4 bên dưới chỉ còn là lịch sử.

## Hiện tại: Midnight Toybox Band, 2026-10-15

Music revision 4 thay toàn bộ 15 bài bằng các bản soạn độc lập trong `Tools/Audio/Scores/`; không dùng khuôn motif/hòa âm/bridge của revision trước. Nhạc runtime gồm 60 stem mới. [Thiết kế từng bài](Toybox_Individual_Compositions.md) và báo cáo `PreviewExports/Audio/Toybox/Composition_Audit.json` mô tả phạm vi đối chiếu bản phổ.

Foley revision 5 tiếp tục thay **582/582 clip ngoài nhạc** bằng tổng hợp nguyên gốc trong `foley_engine.py`, từ 206 thiết kế trong `Tools/Audio/SfxDesigns.json`. Không dùng sample, soundfont, đầu vào studio hay cách dựng SFX v3. Giọng là âm thanh tổng hợp không lời, không phải bản thu của người thật. Nguồn/hash thiết kế và renderer được ghi cho từng WAV trong manifest; chi tiết ở [Toybox_Foley_Designs.md](Toybox_Foley_Designs.md). Nhạc vẫn dùng GeneralUser GS như mô tả bên dưới.

Thư viện đang dùng đã được thay bằng 582 clip SFX/voice/ambience và 60 stem của 15 bản Toybox. Nhạc gốc được soạn/render bằng code với sample instruments **GeneralUser GS 2.0.3** cùng object synthesis; không dùng các file studio trong repo. Chi tiết nguồn, license, giới hạn, pipeline và kiểm tra: [Toybox_Delivery.md](Toybox_Delivery.md). Provenance/SHA-256 từng file: [Toybox_Asset_Manifest.json](Toybox_Asset_Manifest.json). Toàn văn license sample: [GeneralUser-GS-LICENSE.txt](ThirdParty/GeneralUser-GS-LICENSE.txt).

Các tuyên bố bên dưới chỉ mô tả bộ cũ v2; không áp dụng cho audio Toybox có sample bên thứ ba.

## Lịch sử: Neon Carnival Funk v2, 2026-10-14

- **Nguồn:** toàn bộ 471 clip SFX/voice/ambience và 60 stem nhạc (cùng `cues.json`, `music.json`, preview) — bộ "Neon Carnival Funk" v2 thay hoàn toàn bản v1 ngày 2026-10-14 — được **tổng hợp bằng code** trong `Tools/Audio` (`synth.py`, `kit.py`, `sfx_gen.py`, `sfx_kit_extra.py`, `build_sfx.py`, `build_music.py`) bằng numpy/scipy, encode Vorbis bằng libsndfile (soundfile); không dùng sample, không trích xuất từ game khác (kể cả Bizarre Brigade / Zenless Zone Zero — chỉ tham khảo thể loại và năng lượng, không chép giai điệu hay âm thanh), không dùng thư viện âm thanh bên thứ ba.
- **Tái lập:** seed xác định theo (id cue, biến thể); chạy lại script cho kết quả giống hệt (trừ khác biệt encoder libvorbis).
- **Quyền:** tác phẩm tạo ra cho dự án Arcane Brigade; chủ dự án quyết định giấy phép phát hành. Không có điều khoản bên thứ ba ràng buộc output, nên dùng thương mại trên Steam/mobile không bị chặn bởi nguồn gốc audio. numpy/scipy (BSD) chỉ là công cụ build, không đóng gói vào game. *Đây là ghi nhận kỹ thuật về provenance, không phải tư vấn pháp lý.*
- **Âm thanh cũ giữ lại làm fallback** (không bị xóa, vẫn đi kèm credit gốc): Kenney Impact/RPG/Interface Sounds (CC0) trong `Licensed/Cue-*.wav`; 4 track CC0 `K8-*` (xem `Resources/Audio/Licensed/K8-Audio-Credits.txt`, `Docs/K7_Audio_Provenance.md`, `Docs/K8_Audio_Provenance.md`). Khi bản mới được duyệt có thể xóa để giảm dung lượng và credit.
- **Lưu ý checkout:** các file cũ là con trỏ Git LFS ở môi trường này; không xác minh lại hash ở đây.
