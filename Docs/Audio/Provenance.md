# Audio provenance — thư viện `Resources/Audio/Arcane`

- **Nguồn:** toàn bộ 447 clip SFX/voice/ambience và 36 stem nhạc (cùng `cues.json`, `music.json`, preview) được **tổng hợp bằng code** trong `Tools/Audio` (`synth.py`, `sfx_gen.py`, `build_sfx.py`, `build_music.py`) bằng numpy/scipy; không dùng sample, không trích xuất từ game khác (kể cả Bizarre Brigade), không dùng thư viện âm thanh bên thứ ba.
- **Tái lập:** seed xác định theo (id cue, biến thể); chạy lại script cho kết quả giống hệt (trừ khác biệt encoder libvorbis).
- **Quyền:** tác phẩm tạo ra cho dự án Arcane Brigade; chủ dự án quyết định giấy phép phát hành. Không có điều khoản bên thứ ba ràng buộc output, nên dùng thương mại trên Steam/mobile không bị chặn bởi nguồn gốc audio. numpy/scipy (BSD) chỉ là công cụ build, không đóng gói vào game. *Đây là ghi nhận kỹ thuật về provenance, không phải tư vấn pháp lý.*
- **Âm thanh cũ giữ lại làm fallback** (không bị xóa, vẫn đi kèm credit gốc): Kenney Impact/RPG/Interface Sounds (CC0) trong `Licensed/Cue-*.wav`; 4 track CC0 `K8-*` (xem `Resources/Audio/Licensed/K8-Audio-Credits.txt`, `Docs/K7_Audio_Provenance.md`, `Docs/K8_Audio_Provenance.md`). Khi bản mới được duyệt có thể xóa để giảm dung lượng và credit.
- **Lưu ý checkout:** các file cũ là con trỏ Git LFS ở môi trường này; không xác minh lại hash ở đây.
