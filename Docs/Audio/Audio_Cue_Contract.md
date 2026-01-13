# Audio cue contract

## Thêm cue mới
1. Thêm một dòng `C('nhom.ten', generator, số_biến_thể, bus, priority, gainDb, cooldown, maxVoices, pitchJitter, ...)` trong `Tools/Audio/build_sfx.py`.
2. `python3 Tools/Audio/build_sfx.py && python3 Tools/Audio/write_meta.py` (sinh wav + `cues.json` + `.meta` với GUID ổn định).
3. Phát: `CheckpointAudio.PlayInterfaceId("nhom.ten")` (route tới owner đang active), hoặc thêm alias trong `ArcaneCueLibrary.EventId` nếu là `CombatEvent`.

Thêm biến thể cho cue có sẵn: tăng `số_biến_thể`; không cần sửa C#.

## Trường của một cue
| Trường | Ý nghĩa |
| --- | --- |
| `id` | ID ổn định, `nhóm.tên[.biến thể]`. Không đổi tên sau khi game đã dùng. |
| `bus` | Music, Combat, UI, Foley, Voice, Ambience (bus phần mềm; `CheckpointAudio.BusTrim`). Không dùng AudioMixer asset. |
| `priority` | Unity priority (0 = quan trọng nhất). ≤120 được phép **chiếm** lane cosmetic của cue kém quan trọng; ≥180 bị bỏ khi Reduced Audio. |
| `gainDb` | Mức phát so với clip đã chuẩn hóa peak −3 dBFS. |
| `cooldown` | Giây tối thiểu giữa hai lần phát cùng cue (nhân 2 khi Reduced Audio, trừ danger). |
| `maxVoices` | Số lần phát đồng thời tối đa của riêng cue đó. |
| `pitchJitter` | ± tỉ lệ pitch ngẫu nhiên (≤0,05; signature = 0). |
| `danger` | Dùng lane dành riêng 0–1, không bị density gate, duck nhạc 0,48 trong 0,5 s. |
| `loop` | Chỉ ambience. |

Chống lặp: `Pick` không bao giờ chọn lại biến thể vừa phát.
Chống spam: cooldown theo cue + gate cosmetic toàn cục (`CosmeticInterval`) + `maxVoices` + 4 lane cosmetic (2 khi Reduced).
Đồng minh không phát footsteps riêng: `chr.step` cooldown 0,16 s, 2 voice, priority 220 (bị bỏ đầu tiên); **gameplay nên chỉ gọi cho Champion**.

## Event hook đề xuất cho agent gameplay (chưa nối, giữ thay đổi nhỏ)
Mỗi dòng là một lời gọi `CheckpointAudio.PlayInterfaceId("…")`:

| Khi | Cue |
| --- | --- |
| vàng rơi / nảy / bị hút | `loot.coin.drop` / `loot.coin.bounce` / `loot.coin.vacuum` |
| rương xuất hiện | `loot.chest.appear` |
| merge ra sao 2 / sao 3 | `up.star2` / `up.star3` |
| đồng minh nhảy xuống | `up.ally.land` |
| bond đạt mốc 1 / 2 | `up.bond.t1` / `up.bond.t2` |
| chuỗi tương tác kích hoạt | `up.chain` |
| item xanh / tím / vàng xuất hiện hoặc mua | `up.rarity.blue/purple/gold` |
| shop: thảm mở / cuộn, bệ xuất hiện, hàng nhảy ra | `shop.carpet.open/roll`, `shop.stand.appear`, `shop.item.pop` |
| tới gần món / chuyển focus | `shop.approach` / `shop.focus` |
| mua bằng trứng / HP | `shop.buy.dream` (vàng đã được nối qua cue 20) |
| thiếu vàng / trứng / HP | `shop.deny.gold/egg/hp` |
| reroll / fusion / xuống wave | `shop.reroll` / `shop.fusion` / `shop.descend` |
| shopkeeper rời | `shop.keeper.exit.gold/dream` |
| boss đổi giai đoạn | `boss.phase` |
| quái lớn bước / gầm | `enemy.big.step` / `enemy.big.roar` |
| Champion nhảy / đáp / lướt / bước | `chr.jump/land/dash/step` |
| UI hover, back, mở/đóng menu, notebook (page), tooltip, tab, pause/resume, settings, unlock, achievement, receipt | `ui.*` (chỉ gọi khi trạng thái UI **thực sự** đổi) |

## Nhạc: hợp đồng với gameplay
`ArcadeMusic.SetContext(wave, normalWaves, endless, dreamShop, units)` được gọi mỗi frame từ `ArenaController.Update` (một dòng, audio-only). Chọn scene:
`menu`, `select` (qua `ForceScene("select")`, menu chọn nhân vật cần gọi; chưa nối), `combat_early/mid/late` (theo wave/normalWaves: <34% / <67% / còn lại), `boss`, `endless`, `shop_gold`, `shop_dream` (`WorldShop.Special`).
Mật độ = (số địch còn sống − 8)/50, kẹp 0–1, lên 0,35/s, xuống 0,08/s. Stem `intensity` bật theo mật độ 0,3→0,85.
Chuyển scene: crossfade đúng **vạch nhịp** của bản đang phát (1,4 s; boss/lần đầu: vào ngay 0,5 s). Pause giảm nhạc mượt (×0,28), Reduced Audio ×0,75, danger duck ×0,48.

Thêm bản nhạc: khai báo `add(...)` trong `Tools/Audio/build_music.py` với `scenes=[…]`; nhiều bản cùng scene sẽ xoay theo `wave`.
