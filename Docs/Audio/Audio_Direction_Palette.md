# Arcane Brigade — hướng âm thanh và palette

## 1. Nghiên cứu Bizarre Brigade (giới hạn trung thực)
Môi trường làm việc này **không phát hay nghe được audio và không xem được video có tiếng**. Vì vậy **chưa có** quan sát âm thanh trực tiếp nào về Bizarre Brigade, và tài liệu này **không** ghi điều gì như "đã nghe". Việc còn lại cho người có tai:
- Footage đã dùng ở `Bizarre_Brigade_15_Reference_Audit.md` (V1 Belle 1.5, V2 Boo Fortress 1.5, V0 bản cũ). Nghe lại các mốc: V1 5s (chọn nhân vật), 30.05–30.40s (hit/death/coins), 45–60s và 120/180/300s (shop), 480s (altar), 720–930s (late combat đông), 945s (kết quả); V2 60s (đạn nảy), 470–495s (shop), 1080s (late combat).
- Ghi ra: nhạc có layer theo mật độ hay không; mức duck khi đông quái; âm hit nào được ưu tiên; shop có nhạc riêng không; cách xử lý coin dồn dập; độ lớn tương đối của UI so với combat.
- Chỉ tham khảo cách phối/feedback. Không trích xuất hay sao chép nhạc/SFX (bản audit F08 cũng ghi "U, chưa audition được").

Các quyết định bên dưới là **giả thuyết thiết kế**, không phải "giống reference".

## 2. Palette thống nhất
Dream cabaret / haunted toy orchestra: kỳ quái nhưng vui.

| Vai trò | Nhạc cụ (tổng hợp mô hình hóa — modal/additive, không sample) |
| --- | --- |
| Bass | upright pizzicato, bass clarinet (ostinato cho late/boss) |
| Giai điệu | clarinet (chính), marimba, vibraphone, celesta, music-box |
| Hòa âm | pizzicato chord, pad saw detune nhẹ (dream shop detune mạnh hơn) |
| Nhịp | felt kick, brush, woodblock, shaker, tick đồng hồ, toy tom, gong cho boss/phase |
| Vật thể | chuông, đồng xu kim loại, giấy, vải, gỗ, thủy tinh, sáp (SFX theo chất liệu) |

Nguyên tắc: không beep sine thuần cho feedback chính (mọi cue pha marimba/celesta/vibes/bell hoặc noise có hình bao), không sawtooth đơn lẻ; transient mềm (attack ≥0,5 ms, lọc <14 kHz); tương phản: stem `intensity` và `rhythm` có thể tắt để còn base+melody.

Shop vàng: C major swing, marimba + upright đi bộ + brush + chuông quầy thu ngân (ấm, lạch cạch, tham). Shop giấc mơ: Ab lydian 3/4, vibes/celesta/music-box, pad detune, reverb dài hơn (lung linh, hơi méo, bí ẩn).

## 3. Thư viện nhạc (15 bản, đạt mục tiêu 12–18)
Mỗi bản = 4 stem (base / rhythm / melody / intensity), cùng tempo, độ dài mẫu chính xác, đuôi nốt và reverb vòng quanh về đầu loop (không click, không cắt câu). Không dùng một BPM chung.

| ID | Scene | BPM | Nhịp | Âm | Loop | Họ |
| --- | --- | --- | --- | --- | --- | --- |
| `menu_curtain` "Curtain Call" | menu | 88 | 4/4 swing | D dorian | 87,3 s | stage |
| `select_dressing` "Dressing Room" | select | 76 | 4/4 | F major | 50,5 s | stage |
| `combat_toys` "Toys on Parade" | combat đầu | 112 | 4/4 hành khúc | A minor | 68,6 s | toyroom |
| `combat_clockwork` "Clockwork Conga" | combat giữa | 124 | 4/4 | E dorian | 61,9 s | workshop |
| `combat_overtime` "Midnight Overtime" | combat cuối | 138 | 4/4 | G minor | 55,7 s | office |
| `boss_ringmaster` "Ringmaster of Drawers" | boss | 148 | 4/4 | D phrygian-dominant | 51,9 s | all |
| `shop_gold` "Cash Register Rag" | shop vàng | 104 | 4/4 swing | C major | 73,8 s | toyroom |
| `shop_dream` "Lullaby Till Open" | shop giấc mơ | 80 | 3/4 | Ab lydian | 54,0 s | dream |
| `endless_neverclose` "Never-Closing Time" | endless | 132 | 4/4 | Bb dorian | 58,2 s | all |
| `combat_spoons` "Spoon Brigade" | combat đầu (xoay theo wave) | 118 | 4/4 hành khúc | D major | 65,1 s | toyroom |
| `combat_carousel` "Haunted Carousel" | combat giữa (xoay) | 126 | 4/4 | A harmonic minor | 61,0 s | toyroom |
| `combat_lasttrain` "Last Train Overtime" | combat cuối (xoay) | 142 | 4/4 | C minor | 54,1 s | office |
| `boss_clown` "Clown at the Cash Register" | boss (xoay) | 152 | 4/4 | E phrygian-dominant | 50,5 s | all |
| `shop_layaway` "Layaway Shuffle" | shop vàng (xoay) | 98 | 4/4 shuffle | F major | 78,4 s | toyroom |
| `endless_encore` "Encore Forever" | endless (xoay) | 136 | 4/4 | G dorian | 56,5 s | all |

Bản thứ hai của mỗi scene dùng motif hook ngắn lặp lại ở đầu mỗi section (2 nốt nhấp nhô, riff bass clarinet, câu hỏi–đáp clarinet), nhân đôi bằng celesta/music-box khi section cuối. Cùng scene thì `PieceForScene` xoay theo số wave.

Quan hệ chuyển cảnh: combat đầu (A minor) → shop vàng (C major, relative major) cùng họ toyroom; chuyển diễn ra ở vạch nhịp của bản đang phát với crossfade 1,4 s.

**Chưa làm:** biến thể theo theme/map (cần API theme), nhạc elite riêng, intro/outro tách file (hiện chỉ có loop + stings `sting.waveclear/victory/defeat` + `boss.enter/phase/defeat`), Mỗi bản mới chỉ cần một `add(...)` trong `build_music.py`.

**Rủi ro nghe:** giai điệu được viết bằng bảng nốt thủ công và "snap" vào nốt hợp âm ở phách mạnh; **chưa ai nghe** nên chưa biết có nhớ/dễ chịu không. Chất liệu là tổng hợp mô hình hóa nên có thể nghe "MIDI/giả" ở marimba/clarinet; chất lượng "nhạc sống" chỉ đạt được bằng nhạc sĩ hoặc sample library có license.

## 4. Mixing
- Peak mọi clip SFX −3 dBFS; stem nhạc chuẩn RMS ≈ −22…−25,5 dB mỗi stem, tổng 4 stem ≈ −17 dB RMS, đỉnh tổng ≤1,08 (runtime nhân SceneLevel 0,5–0,62 và bus nên không clip; `select_dressing` đỉnh tổng 1,08 chỉ khi cả 4 stem đầy).
- Ưu tiên: danger (đạn địch, telegraph boss, player damage, chết) lane riêng + duck nhạc; swing/hit hàng loạt bị cooldown + gate.
- Reduced Audio: cosmetic ×0,65, cue priority ≥180 bị bỏ, cooldown ×2, nhạc ×0,75, shopkeeper im.
- Mọi mức (SceneLevel, gainDb, BusTrim) là **giá trị khởi điểm chưa nghe**; chỉnh bằng tai trong game.
