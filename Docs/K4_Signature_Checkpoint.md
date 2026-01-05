# K4.1 — Signature weapon Windows candidate

Ngày: 2026-10-13. Bắt đầu K4 theo roadmap đã duyệt. **K4.1 đạt functional gate Windows**, không phải toàn K4 DONE hoặc production art/audio/balance/mobile acceptance.

## Runtime thật

| Family | Hành vi cơ bản | Tier ≥2 |
| --- | --- | --- |
| UmbrellaLance | Giữ thrust/Frost cũ. Ba impact tích một bung hướng trước, trễ 0.18s, damage 45% base, radius 2.1; push tối đa 0.35, boss ×0.2 | Radius bung ×1.25, không tự chặn đạn |
| BellFork | Giữ projectile/Arcane cũ. Ba impact tạo sóng quanh vị trí nguồn khi tích đủ, trễ 0.18s, damage 65% base, radius 2.6 | Đúng một echo sau thêm 0.3s, damage 35% sóng; không echo từ echo |
| DreamStamp | Một target lock tại windup; impact miss nếu target chết/ra range. Damage 9, range 1.65, cooldown 0.75s. Ba dấu cùng source-target → burst 110% base; bỏ mục tiêu >2.4s mất dấu | Burst truyền tối đa một dấu sang một enemy gần radius 1.7; truyền dấu không tự burst |

Số là tuning khởi điểm riêng của Arcane Brigade, không phải thông số Bizarre Brigade được xác minh. Damage base/tier/item vẫn dùng modifier runtime hiện có; không nhân thêm tier damage trong payoff. Cadence tích theo impact đã phát, không theo số target hit; stamp tích dấu chỉ khi hit target sống. Đây là auto-combat, không thêm nút skill hoặc movement combo.

ID Ô/Chuông và asset HD cũ giữ nguyên. `DreamStamp` là ID mới, nằm trong pool roll weapon khi tuyển/merge theo luật cũ; không show weapon cố định trước tuyển hoặc thay inheritance. Pool có chín món nên các roll **tương lai** có thể khác bản tám món cùng seed; stock/roster đã lưu không bị roll lại. Save schema vẫn 2. Tier clone mang `CombatTier` để đọc twist; không tự nâng tier trên asset nguồn.

## Bảo vệ và presentation

- Mark tách từng source-target; prune khi hết hạn/chết. Snapshot pulse giữ vị trí/hướng/damage/radius khi lên lịch, không theo camera. Pause không tiến timer.
- Burst/wave/echo dùng cause depth 1, không kích hoạt loot procs. Damage/kill/loot qua core thật, không visual hit hoặc duplicate coin. Echo không tự schedule echo. Ally vẫn bất tử; Ô không cấp interceptor.
- Cadence/mark/pulse reset ở start/rest boundary như projectiles. Resume combat bắt đầu lại hall theo save contract đã có; không giả hỗ trợ mid-tick state restore.
- Stamp có sprite nguyên bản transparent **1774×887**, không native 4K. Nguồn: `Assets/ArcaneBrigade/Art/QualityHD/DreamStamp_v1.png`; asset: `Data/Run/DreamStamp.asset`. Import lossless, max4096, bilinear, alpha thật. Giữ bộ weapon atlas cũ, không regenerate toàn bộ art.
- Stamp có thrust-compression pose liên tục qua impact, dấu nhỏ trên enemy, seal burst; Ô dùng forward arc, Chuông dùng vòng sóng. Ba cue âm tổng hợp riêng, có throttle/voice budget; chưa có audition/license music production mới.
- Details đọc threshold, expiry, scale và tier thật từ data; không đổi RNG. Font/UI layout chưa redesign theo yêu cầu để sau K6/K7. Tier **silhouette riêng/merge reveal** còn K4.3; không nhận các chấm dấu là final art.

## Gate và bằng chứng

`PreviewExports/K4/Signature_Checks.txt`: **40 PASS** gồm cadence/delay/range/front hemisphere, finite echo, boss resistance, pause/rest/restart, source-separated marks, expiry/dead cleanup, single-target/range miss, loot-once, secondary kill không proc, save/Details và reapply.

Regression: Checkpoint 17, Squad 12, FullRun 97, K0/K1 27, K2 23 (thêm pose continuity cho DreamStamp), transaction 36, modifier 29, altar 14, K3 final 31, HD 31. Build Windows thành công 0 errors / 0 warnings. Test phát hiện discontinuity ở pose stamp đã được sửa bằng anticipation blend; test bow dùng windup thực thay vì giả định chung 0.13s.

Native Windows capture: `PreviewExports/K4/SignaturePlayer/index.html`, five PNGs **3840×2160**. Máy chạy live1920×1080, Unity supersize2; không resize ngoài Unity hoặc claim realtime4K60FPS. Seed13104, roster tier2/HP/vị trí diagnostic; enemy AI/attack và ally patrol đóng băng để quan sát. **Target/windup/hit/mark/pulse/tier tự chạy simulation thật, không bơm proc bằng fixture helper**. Ba counter payoff phải >0 mới PASS. Capture có thể làm elapsed attack count khác; không dùng counter này làm contribution/DPS. Save diagnostic tách riêng. Notes ghi kết quả thực.

Binary local: `Builds/FullRun/ArcaneBrigade.exe`, Unity6000.4.8f1; build/cache không commit. Source/art/meta/scene/catalog/test/capture đưa Git/LFS. Hash snapshot: `PreviewExports/K4/Artifact_SHA256.txt`.

## Còn lại

K4.2: hai build chains và connector/contribution thật. K4.3: merge reveal/tier silhouette/idle. K4.4: props theo seed và boss phase cảnh báo công bằng. K4.5: sandbox cô lập và full vertical slice. Không dùng screenshot fixture này để nhận combat→shop→boss full run đã nghiệm thu. Mobile gate K7; font/UI K6–K7.

## Art generation prompt

Mode built-in image generation với atlas riêng `Weapons_v2.png` làm style reference; không dùng CLI/API riêng. Final prompt:

> Generate ONE new original game weapon sprite matching the reference sheet's simple clean rounded hand-painted cel arcade style. A whimsical dream-office rubber stamp weapon, horizontally oriented for 2D auto-combat: dark plum chunky cylindrical grip on LEFT, short golden collar, coral-red squat spring-loaded stamp block at RIGHT, broad cream rectangular stamping face at far RIGHT with one simple red four-point seal motif, tiny crooked golden crown nub. Strong readable silhouette, two or three large cel color regions per part, no fine busy detail. Full weapon entirely visible, centered, occupies 75% width, generous transparent padding. True transparent background, NO ground shadow, NO baked glow, NO arm, NO hand, NO text, NO letters, NO additional weapons or variations. This is a single usable HD sprite, not a concept presentation. Keep the long axis horizontal left to right and the left grip unobstructed for a floating hand attachment. Preserve the reference palette but do not reproduce any existing reference weapon.

Alpha/shape đã kiểm tra trước khi import; bản chọn đã copy vào repo, không chỉ nằm trong thư mục generated_images ngoài project.
