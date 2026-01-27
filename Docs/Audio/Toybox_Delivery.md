# Midnight Toybox Band — bàn giao audio

Ngày sản xuất: 2026-10-15. Music revision 4: soạn lại riêng từng bài theo phản hồi các bản cũ nghe giống nhau. Palette lấy từ `Music_Prompts_Toybox.md`; bản phổ hiện tại nằm trong `Tools/Audio/Scores/`.

Foley revision 5: làm lại tiếp toàn bộ **582 clip ngoài nhạc**, gồm SFX, giọng không lời và ambience. 206 thiết kế riêng nằm trong `Tools/Audio/SfxDesigns.json`; xem [danh mục thiết kế](Toybox_Foley_Designs.md). Toàn bộ 60 stem nhạc revision 4, manifest cue, đường dẫn và GUID giữ nguyên qua lần thay Foley này.

## Phạm vi

- 15 bản nhạc được soạn lại từ đầu trong 15 file bản phổ độc lập, 60 stem stereo đồng bộ ở 44.1 kHz. Không ép chung motif, chuỗi hòa âm hay câu bridge. Mỗi bài có câu giai điệu, đoạn tiếp nối, bass, drum chart, form và mix riêng; xem [thiết kế từng bài](Toybox_Individual_Compositions.md).
- 582/582 clip Toybox trước đã được thay mới, bao gồm cả 111 clip từng bổ sung cho 53 cue NEW. Giữ tên file, GUID Unity, số biến thể và số sample của tất cả clip; tổng cộng 206 cue.
- Không dùng, cắt, phối lại hoặc phân tích nội dung của các MP3 trong `generate audio/`. Chúng thuộc theme khác và được giữ nguyên.
- Không dùng dịch vụ tạo nhạc/audio. Bản phổ là `Tools/Audio/Scores/<piece>.json`; `score_player.py` chỉ đọc ký âm và sắp event, `toybox_music.py` render/encode. Nhạc cụ sample kết hợp sound design vật lý/tổng hợp bằng code. Foley v5 được dựng mới hoàn toàn bằng code, không dùng soundfont, sample bên ngoài hay renderer SFX cũ; các hook gameplay giữ nguyên.

## SFX, giọng và ambience v5

`foley_engine.py` dựng tại 88.2 kHz rồi lọc và hạ xuống 44.1 kHz. Các mô hình gồm vật liệu cộng hưởng nhiều mode, dây có suy hao, hạt tiếp xúc, dòng hơi, giọt nước và nguồn giọng glottal/formant. Đầu ra runtime vẫn mono WAV PCM 16-bit theo contract; master 24-bit nằm trong `PreviewExports/Audio/Toybox/Masters/Foley/` tại local.

Giấy, thiếc, sáp, vải, thủy tinh và gỗ có tiếp xúc/thân/đuôi riêng; tiếng vỡ có mảnh rơi nối tiếp. Vũ khí có cơ chế riêng: dây cung, nút chai, lưỡi kéo, trục yo-yo, cuộn nam châm, kim khâu, lò xo, bánh răng máy chiếu và lụa. Hiệu ứng thường xuyên được giữ khô và ngắn; phần thưởng dùng tines/porcelain có cao độ; danger dùng nhịp/reed/áp lực khác tiếng nhặt tiền.

Mỗi hero và 10 boss có phối lớp riêng trong catalogue. Hai chủ tiệm dùng tám âm tiết với nguyên âm khác nhau, laugh/hmm/gasp/farewell; toàn bộ là tiếng nhân vật tổng hợp không lời, không phải diễn viên lồng tiếng. Sáu ambience có nền nhiễu tuần hoàn và vật thể thưa riêng cho từng phòng; không chèn giai điệu lặp chung. Biến thể thay điểm tiếp xúc, độ suy hao và diễn tấu; cảnh báo giữ cấu trúc nhịp và cao độ hẹp.

`Foley_Audit.json` kiểm tra thay đủ 582 file so với snapshot trước lần làm lại, giữ độ dài, tính duy nhất và tương quan waveform giữa biến thể. Tương quan không đo độ khác biệt cảm nhận. `QA_Report.json` kiểm tra audio sau encode; mốc merchant 0.98 s và ally 0.405 s được kiểm riêng. `Foley_Timelines.json` ghi chính xác từng cue trong tám reel nghe thử; các loop dùng trích đoạn sáu giây, từng loop đủ 24 giây vẫn nghe được ở nút biến thể.

## Nhạc và mastering

`Assets/ArcaneBrigade/Resources/Audio/Arcane/Music/music.json` là manifest đang dùng trong game. ID và scene routing giữ nguyên; tempo, meter, tiêu đề, cấu trúc và số sample theo bản phổ riêng mới. Các chỉ dẫn motif/form/thời lượng của prompt ban đầu được thay theo phản hồi mới: mỗi bài được phát triển độc lập, dài khoảng 48–86 giây. Số sample tính bằng `round(bars × beatsPerBar × 60 / BPM × 44100)`, không kéo giãn toàn bản.

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

`individual_themes_comparison.ogg` nối tám giây đầu của mỗi bài, cách nhau 0,4 giây; thứ tự và mốc thời gian trong `comparison_timeline.json`. `MIDI/` chứa 15 bản ký âm chỉnh sửa được (âm sắc khi phát MIDI phụ thuộc soundfont trên máy). `Composition_Audit.json` đối chiếu 105 cặp: chuỗi hòa âm, drum chart và cửa sổ bảy nốt theo quãng/nhịp không phụ thuộc tông. Kết quả ít trùng không phải chứng nhận hấp dẫn hoặc hoàn toàn khác nhau khi nghe.

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
python Tools/Audio/render_song.py --remaining
python Tools/Audio/render_song.py --assemble
python Tools/Audio/build_toybox.py --music-only --install-only
python Tools/Audio/audit_individual_scores.py
python Tools/Audio/render_foley.py --install
python Tools/Audio/toybox_preview.py
python Tools/Audio/verify_toybox.py
```

`render_song.py <id>` dựng một bài riêng; `--remaining` chỉ dựng bản phổ đã thay đổi/chưa render, `--assemble` gom đủ 15 bài thành manifest staging. `build_toybox.py --music-only --install-only` mastering/QA rồi chỉ cài nhạc. Với checkout mới chưa có staging SFX, dùng `build_toybox.py --music-only --install` để dựng/cài nhạc; pipeline kiểm SFX trực tiếp từ project. Audit/export MIDI cần thêm package `mido`; LUFS dùng `pyloudnorm` nếu có. `toybox_baseline.json` lưu manifest/hash/độ dài cũ. Không chạy legacy `build_music.py` / `build_sfx.py` để dựng bộ hiện tại.

`render_foley.py --install` dựng và kiểm 582 clip từ `SfxDesigns.json` trước khi chép vào Unity; không cần FluidSynth/soundfont để làm lại Foley. `build_toybox.py --sfx --install` chuyển tới cùng pipeline này. `author_foley.py` là phần biên soạn để xuất catalogue khi muốn sửa thiết kế; không cần chạy lại khi chỉ tái tạo audio. Giọng, SFX và ambience v5 không còn dùng sampled accents của v3. License GeneralUser GS vẫn áp dụng cho 60 stem nhạc.
