# Arcane Brigade — Audio audit và bảng event → cue

Cập nhật: 2026-10-14. Base: `7ab96d6` (checkpoint K12). Trạng thái: **đã tích hợp trong code nhưng CHƯA chạy Unity và CHƯA nghe** (xem `Audio_QA_Checklist.md`).

## 1. Hệ thống trước khi thay đổi (clip nào thực sự phát)

| Thành phần | File | Thực tế phát |
| --- | --- | --- |
| Nhạc | `ArcadeMusic.cs` | 4 track CC0 của bên thứ ba (`K8-Menu-FusionJazz`, `K8-Shop-CoffeeHouse`, `K8-Combat-JazzClub`, `K8-Boss-BattleTheme`), 1 track/phase, crossfade volume, không cùng nhịp/hòa âm, không layer. Menu và Victory/Defeat dùng chung track 0. |
| SFX | `CheckpointAudio.cs` + `ArcadeSound.cs` | 39 cue index (0–38), 8 AudioSource. Clip = Kenney CC0 `Cue-00…32` (hit theo chất liệu mượn lại 6 clip), nếu thiếu thì `NightCabaretSound.Foley` tạo procedural. `CombatSound.cs`/`NightShift_*.wav`/`CombatStudy.wav`/`ShopStudy.wav` **không** được game phát (study cũ). |
| Giọng shopkeeper | `MerchantVoice.cs` | 1 syllable sin+formant procedural, phát gần như mỗi glyph (khoảng 0,105 s). |
| Ambience | — | Không có. |
| Mixer | — | Không có AudioMixer; 8 lane: 0–1 danger, 2–3 UI, 4–7 cosmetic (2 khi Reduced Audio). |

Lưu ý checkout: các file audio cũ trong repo là con trỏ Git LFS (130 byte); không xác minh được nội dung ở môi trường này.

## 2. Hệ thống sau thay đổi

`Resources/Audio/Arcane/` (sinh bởi `Tools/Audio`): **143 cue / 447 clip** + **9 bản nhạc × 4 stem**. `ArcaneCueLibrary` đọc `cues.json`/`music.json`. Nếu thiếu manifest hoặc clip, game **tự rơi về hệ thống cũ** (không crash, không im lặng).

Phân bố: Combat 65 cue/238 clip, Foley 35/101, UI 26/68, Voice 10/32, Ambience 7/8.
(Vượt mốc 250 clip vì chọn phủ rộng; frequent event 4–6 biến thể, hiếm 2–3.)

## 3. Event gameplay → cue (đã nối)

Trạng thái: **W** = đã nối vào runtime; **A** = chỉ là asset, chưa có điểm phát; **P** = đề xuất hook (xem `Audio_Cue_Contract.md`).

### Combat (`CombatEvent` → `ArcaneCueLibrary.EventId`, gọi từ `CheckpointAudio.Play(CombatEvent)`)

| Event | Điều kiện | Cue | |
| --- | --- | --- | --- |
| Windup | boss | `boss.telegraph` | W |
| Windup | địch có đạn | `enemy.spit.windup` | W |
| Windup | địch cận chiến | `cmb.windup` | W |
| Fire | boss | `boss.attack` | W |
| Fire | địch bắn / lao | `enemy.spit.fire` / `enemy.lunge` | W |
| Fire | Family Tea/Fish/Alarm/TetherBow | `sig.tea` / `sig.fish` / `sig.alarm` / `wpn.bow.release` | W |
| Fire | Motion Slash/Thrust/Slam | `cmb.swing.blade` / `.spear` / `.hammer` | W |
| Fire | Motion Recoil | `wpn.gun.pop` (đạn chậm <6: `wpn.launcher`) | W |
| Fire | Motion Bow / Cast(Ember/Frost/khác) | `wpn.bow.release` / `wpn.cast.fire/ice/arcane` | W |
| Fire | Bell/Stamp/Scissors/Camera/Iron/Hammer/Music | `sig.bell/stamp/scissors/camera/iron/hammer/musicbox` | W |
| Hit | người chơi bị trúng | `plr.hurt` (danger lane) | W |
| Hit | crit | `cmb.crit` | W |
| Hit | quái, theo `BodyMaterial` | `cmb.hit.paper/metal/wax/fabric/glass/wood` | W |
| Kill | người chơi | `plr.death` | W |
| Kill | boss | `boss.defeat` | W |
| Kill | quái `BodyScale ≥ 1.8` | `enemy.die.large` | W (ngưỡng 1.8 là giả định, chưa đối chiếu roster) |
| Kill | quái khác | `enemy.die.<material>` | W |
| Pickup | Coin / Egg / Heart | `loot.coin.pickup` / `loot.egg` / `loot.heart` | W |
| Heal / ChestOpened / Dodge | — | `plr.heal` / `loot.chest.open` / `plr.dodge` | W |
| SignaturePulse/Burst | Bell/Stamp/Umbrella | `sig.bell/stamp/umbrella` | W |
| Proc | Ember/Frost/Arcane | `fx.burn` / `fx.chill` / `fx.reaction.arcane` | W (ý nghĩa `Proc` chưa kiểm chứng) |

### Giao diện / shop / transition (gọi số nguyên cũ → `LegacyId`, hoặc `PlayInterfaceId`)

| Nguồn | Cue |
| --- | --- |
| `ArcadeNavigation` chọn (19) | `ui.select` |
| `ArenaController.Transaction` thành công (20) | `shop.buy.gold` |
| `Transaction` thất bại (22) | `shop.invalid` |
| `MergeReveal` (23) | `up.merge` |
| cảnh báo boss (24) / máu thấp (25) | `boss.telegraph` / `plr.lowhp` (danger) |
| `WorldShop` shopkeeper xuất hiện | `shop.keeper.enter.gold` / `.dream` (đổi từ cue 20) |
| Phase Combat→Shop | `sting.waveclear` (trong `ArcadeMusic.SetPhase`) |
| Boss bắt đầu | `boss.enter` |
| Victory / Defeat | `sting.victory` / `sting.defeat`, nhạc im ~3,7 s rồi về menu |
| Hội thoại shopkeeper | `voice.gold.syl` / `voice.dream.syl`, nhịp theo **từ** và dấu câu, tối đa 14 syllable/câu |

### Chỉ là asset, chưa có điểm phát (cần hook từ agent gameplay, xem contract)

`loot.coin.drop/bounce/vacuum`, `loot.chest.appear`, `up.star2/star3`, `up.ally.land`, `up.bond.t1/t2`, `up.chain`, `up.rarity.blue/purple/gold`, `chr.step/dash/jump/land`, `enemy.hurt`, `enemy.big.step/roar`, `boss.phase`, `wpn.boomerang.*`, `wpn.pierce/split/bounce`, `fx.knockback`, `fx.reaction.steam/shatter`, `shop.carpet.*`, `shop.stand.appear`, `shop.item.pop`, `shop.approach`, `shop.focus`, `shop.buy.dream`, `shop.deny.*`, `shop.reroll`, `shop.fusion`, `shop.descend`, `shop.keeper.exit.*`, `ui.hover/back/open/close/page/tooltip/tab/pause/resume/setting/unlock/achievement/receipt`, `voice.*.laugh/hmm/gasp/farewell` (`MerchantVoice.Gesture` có sẵn), `amb.office/lostshop/toys/transition`.

## 4. Ambience hiện nối

`ArcadeMusic` phát một lớp nền: combat/boss/endless → `amb.stage`; shop vàng → `amb.goldshop`; shop giấc mơ → `amb.dreamshop`; menu/select/victory/defeat → không. Chọn theo theme map (`office`, `toys`, `lostshop`) cần API theme từ gameplay — chưa có.
