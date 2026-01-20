# Arcane Brigade — hướng âm thanh và palette

## 1. Nghiên cứu Bizarre Brigade (giới hạn trung thực)
Môi trường làm việc này **không phát hay nghe được audio và không xem được video có tiếng**. Vì vậy **chưa có** quan sát âm thanh trực tiếp nào về Bizarre Brigade, và tài liệu này **không** ghi điều gì như "đã nghe". Việc còn lại cho người có tai:
- Footage đã dùng ở `Bizarre_Brigade_15_Reference_Audit.md` (V1 Belle 1.5, V2 Boo Fortress 1.5, V0 bản cũ). Nghe lại các mốc: V1 5s (chọn nhân vật), 30.05–30.40s (hit/death/coins), 45–60s và 120/180/300s (shop), 480s (altar), 720–930s (late combat đông), 945s (kết quả); V2 60s (đạn nảy), 470–495s (shop), 1080s (late combat).
- Ghi ra: nhạc có layer theo mật độ hay không; mức duck khi đông quái; âm hit nào được ưu tiên; shop có nhạc riêng không; cách xử lý coin dồn dập; độ lớn tương đối của UI so với combat.
- Chỉ tham khảo cách phối/feedback. Không trích xuất hay sao chép nhạc/SFX (bản audit F08 cũng ghi "U, chưa audition được").

Các quyết định bên dưới là **giả thuyết thiết kế**, không phải "giống reference".

## 2. Palette thống nhất — "Neon Carnival Funk" (soundtrack v2, 2026-10-14)
Bản v1 (dream cabaret / toy orchestra: clarinet, marimba, celesta) đã được **thay toàn bộ**. Hướng mới: groove đô thị kiểu Zenless Zone Zero (funk, breakbeat, electro, drum & bass, phonk, lo-fi hip-hop — dải thể loại mà bản phát hành soundtrack ZZZ tự mô tả gồm EDM, lo-fi, "laid-back grooves") trộn màu lễ hội ma quái của Arcane Brigade (calliope organ, cowbell, đồ chơi). Tham khảo thêm về *năng lượng và cách phản hồi*: Hi-Fi Rush (funk/rock bám nhịp), Persona 5 (acid jazz), Vampire Survivors (âm nhặt đồ/lên cấp gây "nghiện"). **Chỉ tham khảo thể loại, cách phối và chức năng**; không chép giai điệu, tiến trình đặc trưng, sample hay âm thanh của game nào.

| Vai trò | Nhạc cụ (tổng hợp trong `Tools/Audio/kit.py`, không sample) |
| --- | --- |
| Bass | slap / funk finger, acid 303, reese, wobble, saw méo, 808 sub có glide |
| Hòa âm | Rhodes FM (9th/11th), clav, drawbar organ, pad supersaw, chord stab disco/pluck |
| Giai điệu | square lead, supersaw, sax formant, whistle, guitar méo, calliope, cowbell phonk, vibes/glock |
| Giọng | vocal chop formant ("yeah", "oh", "hey", "woo", "ooh"), scratch turntable |
| Nhịp | kick punch/break/808/boom, snare, clap, hat kim loại 808, ride, crash, rim, conga, tamb, tick |
| FX | riser, downlifter, impact boom, gong, tape-stop, bitcrush, vinyl crackle, wow băng từ, sidechain pump |

Nguyên tắc: hook ngắn 2 ô nhịp lặp lại có biến thể (dễ nhớ), câu hỏi–đáp giữa lead và brass/vox; section A–A–B–A (B là breakdown/bridge, A cuối dày nhất); stem `intensity` chứa brass stab, arp, double octave, vox chop, scratch, riser để mật độ quái "mở" thêm năng lượng; sidechain theo kick (bơm kiểu electro) trên base/intensity. SFX dùng cùng palette: hit = thump + crack + màu vật liệu, crit có "ting" kỹ thuật số, nhặt đồ = chime/ka-ching theo pentatonic, UI kiểu CRT/băng cassette, sting = jingle funk ngắn.

## 3. Thư viện nhạc (15 bản, ID/scene giữ nguyên để runtime không đổi)
Mỗi bản = 4 stem (base / rhythm / melody / intensity), cùng tempo, độ dài mẫu chính xác, đuôi nốt/reverb/wow/sidechain vòng về đầu loop.

| ID | Tên mới | Scene | Thể loại | BPM | Nhịp | Âm | Loop |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `menu_curtain` | Neon Marquee | menu | lo-fi neo-soul hip-hop, vinyl | 88 | 4/4 swing | F dorian | 87,3 s |
| `select_dressing` | Pick Your Crew | select | city-pop / chill funk | 96 | 4/4 | Eb major | 40,0 s |
| `combat_toys` | Wind-Up Riot | combat đầu | funk breakbeat + calliope | 120 | 4/4 | A dorian | 64,0 s |
| `combat_spoons` | Spoon Bandits | combat đầu (xoay) | street funk, horn section | 116 | 4/4 | D mixolydian | 66,2 s |
| `combat_clockwork` | Gearbox Groove | combat giữa | electro-funk, acid bass | 124 | 4/4 floor | E dorian | 61,9 s |
| `combat_carousel` | Carousel Heist | combat giữa (xoay) | electro-swing | 124 | 4/4 shuffle | A harmonic minor | 61,9 s |
| `combat_overtime` | Overtime Rush | combat cuối | drum & bass, reese | 174 | 4/4 | G minor | 44,1 s |
| `combat_lasttrain` | Last Train Breakbeat | combat cuối (xoay) | jungle / breakbeat | 166 | 4/4 | C minor | 46,3 s |
| `boss_ringmaster` | Ringmaster Breakdown | boss | big-beat electro-rock, gong | 150 | 4/4 | D phrygian-dominant | 51,2 s |
| `boss_clown` | Clown Kingpin | boss (xoay) | drift phonk, cowbell hook | 150 | 4/4 half-time | E phrygian | 51,2 s |
| `shop_gold` | Cash Rules the Carnival | shop vàng | acid-jazz boom-bap | 100 | 4/4 swing | C mixolydian | 76,8 s |
| `shop_layaway` | Layaway Lounge | shop vàng (xoay) | city-pop jazz-funk | 104 | 4/4 | F major | 73,8 s |
| `shop_dream` | Velvet Daydream | shop giấc mơ | lo-fi dream waltz, tape wobble | 84 | 3/4 | Ab lydian | 51,4 s |
| `endless_neverclose` | Neon Never Closes | endless | future-funk / filter house | 128 | 4/4 floor | Bb dorian | 60,0 s |
| `endless_encore` | Encore Overdrive | endless (xoay) | hard electro-funk, supersaw | 138 | 4/4 floor | G dorian | 55,7 s |

Menu và select có `intensity` = 0 ở runtime nên mọi thứ quan trọng của hai bài này nằm ở base/rhythm/melody.

**Kiểm tra không cần tai:** `python Tools/Audio/audit_harmony.py` liệt kê nốt giai điệu dài rơi vào quãng nghịch với hợp âm; hiện chỉ còn nốt b2 phrygian cố ý của `boss_ringmaster`. Engine còn tự "snap" nốt dài trên phách nằm nửa cung trên một nốt hợp âm.

**Rủi ro nghe:** vẫn **chưa ai nghe**. Tổng hợp bằng code sẽ có chỗ nghe "synth/giả" (sax formant, guitar méo, giọng formant); độ cuốn của hook, độ dày bass và mức sidechain cần chỉnh bằng tai. Chất lượng ngang soundtrack thương mại cần nhạc sĩ/sample library có license.

## 4. Mixing
- Peak mọi clip SFX −3 dBFS; stem nhạc chuẩn RMS base −22 / rhythm −24 / melody −22,5 / intensity −25,5 dB, tổng 4 stem ≈ −17,2 dB RMS, đỉnh tổng 0,68–0,93 (runtime nhân SceneLevel 0,5–0,62 và bus nên không clip).
- Ưu tiên: danger (đạn địch, telegraph boss, player damage, chết) lane riêng + duck nhạc; swing/hit hàng loạt bị cooldown + gate.
- Reduced Audio: cosmetic ×0,65, cue priority ≥180 bị bỏ, cooldown ×2, nhạc ×0,75, shopkeeper im.
- Mọi mức (SceneLevel, gainDb, BusTrim) là **giá trị khởi điểm chưa nghe**; chỉnh bằng tai trong game.
