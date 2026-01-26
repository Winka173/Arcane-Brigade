# Midnight Toybox Band — bàn giao audio

Ngày sản xuất: 2026-10-15. Creative brief chính: `Music_Prompts_Toybox.md`.

## Phạm vi

- 15 bản nhạc gốc, 60 stem stereo đồng bộ ở 44.1 kHz. Cùng motif sol–đô–rê–mi, trả lời mi–rê–đô; mỗi bản có hook, bass, bridge và cách phối riêng.
- 471 clip cũ được thay một-một, giữ tên file, GUID Unity, số biến thể và số sample. Bao gồm SFX, tiếng nhân vật không lời và ambience.
- Bổ sung đủ 111 clip cho 53 cue trong phần NEW: tổng cộng 206 cue / 582 clip ngoài nhạc.
- Không dùng, cắt, phối lại hoặc phân tích nội dung của các MP3 trong `generate audio/`. Chúng thuộc theme khác và được giữ nguyên.
- Không dùng dịch vụ tạo nhạc/audio. Nhạc được soạn bằng score trong `toybox_music.py`, render bằng nhạc cụ sample, kết hợp sound design vật lý/tổng hợp bằng code.

## Nhạc và mastering

`Assets/ArcaneBrigade/Resources/Audio/Arcane/Music/music.json` là manifest đang dùng trong game. ID và scene routing giữ nguyên; tempo, meter, tiêu đề và số sample được cập nhật theo Toybox. Thời lượng ghi bằng giây tròn trong prompt là số gần đúng: số sample được tính bằng `round(bars × beatsPerBar × 60 / BPM × 44100)`. Không kéo giãn toàn bản để ép vào số giây làm tròn.

Mỗi bản gồm base / rhythm / melody / intensity. Hook chính nằm trong melody, percussion chính nằm trong rhythm; không phụ thuộc intensity để nghe đầy đủ ở menu/select hay combat thưa. Bridge vẫn có trống. Nhạc cụ gồm music box, celesta, glockenspiel, honky-tonk piano, calliope, harmonica/melodica-like reeds, brass, bass và drums; percussion gỗ, lò xo, kim loại tạo nhận diện đồ vật.

Reverb và lọc được xử lý vòng; tiếng nhả phím từ vòng trước được render trước khi lấy loop cuối. Chênh lệch pha nhỏ tại ranh giới block của sampler được sửa bằng một splice Hermite 3 ms quanh điểm nối, giữ nguyên số sample, không chèn khoảng im và không fade-out toàn bản. Bốn stem nhận chung gain và envelope kiểm soát transient để giữ cân bằng adaptive và stereo. Runtime Vorbis q8; master WAV PCM 24-bit. SFX/voice/ambience là mono WAV PCM 16-bit, peak −3 dBFS (ambience loop −7 dBFS), lọc DC/hạ tầng và đầu/cuối sạch. Các mốc merchant 0.98 s, ally 0.405 s, gather/reveal được giữ.

## Điểm gameplay đã nối

- Sáu vũ khí YoYo / Magnet / Sewing / SpringHammer / Projector / RibbonWheel và tiếng YoYo quay về.
- Mười cơ chế hero: SignaturePulse theo HeroMechanic, riêng Parcel Express theo nguyên nhân Fire. Charge-ready phát theo cạnh trạng thái khi có thể giữ charge chờ mục tiêu.
- Mười boss: tiếng riêng khi `BossLanded`, cùng entrance/telegraph/music sẵn có.
- Cut-in boss/showstopper/encore/bond phát khi banner thực sự lấy khỏi queue, wave stamp và tutorial bubble phát khi hiện.
- Sáu đồ vật ở menu phát cue riêng khi rê chuột vào. Điều hướng bàn phím/controller vẫn có tiếng focus chung hiện có.
- Ticket SOLD, sway khi tiếp cận stand và hint khi focus offer có thể merge.
- Midwave spectacle: warn lúc xuất hiện, start sau 2 giây, reward khi số lượt reward tăng, end ở cuối vòng đời. Observer chỉ đọc simulation, không thay RNG/gameplay.
- Hostile Hook → enemy.shot.arc; Weave → enemy.shot.spiral; volley từ 3 projectile trở lên → enemy.shot.burst. Đây là phân loại pattern có sẵn; không thêm kiểu projectile mới.

`ui.encore.ready` có đủ clip nhưng **reserved**: game chưa có Encore meter. Không phát giả ở thời điểm khác. Các cue cũ vốn reserved vẫn giữ asset thay thế; không tạo mechanic mới chỉ để phát chúng. Clip không đảm bảo được phát mọi lần: cooldown, giới hạn voice, culling và Reduced Audio tiếp tục áp dụng.

## Nghe và kiểm tra

Mở `PreviewExports/Audio/Toybox/index.html` để nghe 15 bản và từng biến thể SFX, hoặc dùng `playlist.m3u`. Thư mục `Masters/` chứa 60 master stem 24-bit và 15 full mix. `combat_mix_simulation.ogg` là bản phối kiểm thử 24 giây bằng script, **không phải recording trong game**.

Audio runtime, preview, báo cáo và source pipeline được lưu trong Git. `Masters/` là export lossless sinh lại được (khoảng 1 GB), giữ ở local và được gitignore; chạy pipeline để tái tạo trên máy checkout mới.

`QA_Report.json` ghi kết quả kiểm tra âm thanh sau encode: số lượng/file, định dạng, sample alignment, thay hết 471 file cũ, trùng nội dung, DC, peak, seam và true peak oversample 4× trong các tổ hợp stem. `ToyboxAudioChecks.cs` kiểm tra import/routing thực trong Unity, kết quả được chép vào `Unity_Checks.txt` khi chạy thành công.

**Giới hạn cần nghe duyệt:** kiểm tra kỹ thuật không chứng minh người chơi thích âm thanh, không thay thế nghe trong game, và không phải xác nhận đã nghe trên tai nghe/loa thực. Cần playtest combat đông quân, chuyển scene, pause/resume và phiên chơi dài để chốt mức nhạc/SFX, độ mệt tai, độ dễ nhận biết danger. General MIDI samples và giọng tổng hợp vẫn có giới hạn âm sắc; không coi đây là thu âm dàn nhạc/người thật hay bản master đã được chuyên gia nghe duyệt.

## Nguồn và quyền

- Nhạc, score, object synthesis và cách phối được tạo cho Arcane Brigade; không chép nhạc/cue từ game khác.
- Sample instruments: **GeneralUser GS 2.0.3**, S. Christian Collins, [trang tác giả](https://schristiancollins.com/generaluser), [license upstream](https://github.com/mrbumpy409/GeneralUser-GS/blob/main/documentation/LICENSE.txt). License cho phép tạo nhạc thương mại. Bản license cũng ghi tác giả không thể xác nhận tuyệt đối nguồn gốc mọi sample kế thừa; giữ nguyên toàn văn tại `ThirdParty/GeneralUser-GS-LICENSE.txt` để người phát hành đánh giá.
- Renderer offline: FluidSynth 2.6.1 (LGPL), NumPy/SciPy, FFmpeg/libvorbis, python-soundfile. Không đưa DLL, executable renderer hay soundfont vào player; game chỉ nhận audio đã render. File download được pin URL/commit và SHA-256 trong `Tools/Audio/toybox_dependencies.json`.
- Provenance từng file, cue/piece ID, số sample, SHA-256 và nguồn nằm trong `Toybox_Asset_Manifest.json`; score/timeline nằm trong `Toybox_Scores.json`.
- Các asset CC0 fallback ở `Audio/Licensed` vẫn mang credit cũ và được giữ nguyên.

## Tái tạo

Trên Windows x64, Python 3.11 với numpy, scipy, soundfile >= 0.13 và imageio-ffmpeg:

```powershell
python Tools/Audio/setup_toybox.py
python Tools/Audio/build_toybox.py --install
python Tools/Audio/toybox_preview.py
python Tools/Audio/verify_toybox.py
```

`build_toybox.py` dựng vào staging trước, mastering và QA toàn bộ rồi mới chép vào Unity. Có thể rebuild một bản bằng `--music <id>` hoặc chỉ SFX bằng `--sfx`, sau đó `--install-only` để xác minh và cài bộ staging đầy đủ. `toybox_baseline.json` lưu manifest/hash/độ dài cũ để kiểm chứng thay thế. Không chạy legacy `build_music.py` / `build_sfx.py` để rebuild bộ Toybox: hai script đó dựng palette v2.
