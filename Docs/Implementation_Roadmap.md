# Arcane Brigade — kế hoạch chuẩn bị triển khai

**Mốc mới nhất 2026-10-14: K5 DONE theo functional gate Windows.** Xem `K5_Final_Checkpoint.md`;12 concept/6 build/6 prop,139 checks K5 + regression và40 native assertions đạt. Phase kế tiếp K6, production/mobile K7. Mốc này có ưu tiên hơn mọi trạng thái lịch sử phía dưới.

**K4 cập nhật cuối (2026-10-13): DONE theo functional gate runtime Windows.** Ba weapon, hai build/connector/contribution, post-save merge/tier/idle, hai đạo cụ/boss phase và sandbox cô lập đã nối; regression + native vertical slice PASS. Xem `K4_Final_Checkpoint.md`. Phase kế tiếp K5; font/UI redesign K6, device/production polish K7. Những trạng thái K4.1/chưa triển khai cũ bên dưới là lịch sử, cập nhật này có ưu tiên.

**Visual pass trước K4:** chủ dự án chọn hướng 1 (render sắc ở 4K, không yêu cầu mỗi PNG native 4K). Model/map/zoom/effects và ảnh Unity được nâng cấp riêng, giữ contract gameplay K3. Xem `Visual_Quality_4K_Pass.md`; không tính là hoàn tất K4 hoặc device gate K7.

**K3 cập nhật cuối:** DONE theo gate runtime Windows; báo cáo `K3_Final_Checkpoint.md`. Chủ dự án xác nhận chuyển device gate sang K7. Có thể tiếp tục K4; chưa triển khai K4 trong lượt này hoặc tuyên bố production/mobile/exact-reference parity DONE. Các dòng trạng thái cũ bên dưới là lịch sử, cập nhật này có ưu tiên.

Cập nhật K3.2b: HP currency/activation/receipt Windows candidate theo tuning riêng trong `K3_Altar_Economy.md`. Parity/inheritance và K3.3/K3.4 còn lại; chưa qua K4.

**Ngày:** 2026-10-13. **Trạng thái:** K0 đã khóa baseline/contract; K1 và K2 có runtime Windows candidate và đã recheck. K3 đang triển khai theo `K3_Implementation_Plan.md`: K3.1 giao dịch/biên lai/schema 2 và K3.2a modifier/weighted shop/reroll/build links. Báo cáo mới: `K3_Modifier_Economy.md`. Báo cáo K0–K2: `K0_K2_Implementation.md`. Chưa nghiệm thu production hoặc thiết bị mobile.

Đây là **thứ tự thực thi chính** cho backlog đã duyệt, không thay audit/spec: `Bizarre_Brigade_15_Reference_Audit.md` → `Reference_Parity_Backlog.md`, `Signature_Gameplay_Expansion.md` (S1–S5), `Nightmare_Identity_UI_Gameplay.md` (S6–S10, N01–N18). P3–P6 cũ chỉ là functional candidate; K0–K7 bên dưới không kế thừa nhãn hoàn tất hoặc số PASS cũ.

## 1. Cách triển khai

- Kế thừa project/runtime/assets hiện có; giữ ID và save thật. Không rewrite framework, migrate renderer hoặc reset catalog qua baseline builder không cần thiết.
- Mỗi checkpoint là một gói chơi/xem được, có code + data + presentation + UI liên quan + tests + capture. Không để toàn bộ art/feel tới giai đoạn cuối.
- Làm lát cắt hoàn chỉnh rồi mới nhân content. Tất cả ý tưởng đã duyệt vẫn nằm trong backlog, không bỏ vì lát cắt đầu nhỏ.
- Không thêm combat input, formation, upkeep, slot cap, sell/lock hoặc permanent stat grind.
- Desktop validation trước; Android/iOS/Steam cần build/device/lifecycle và prerequisites riêng. Không suy ra device PASS từ Windows.

## 2. Các checkpoint

### K0 — Khóa spec và bảo vệ baseline

**Phạm vi:** chuẩn bị chung cho R1–R7. **Dependency:** không có.

- Lưu manifest source/build hiện có, cảnh/catalog/asset IDs và bộ regression; diagnostic saves cô lập, không ghi đè save player.
- Chốt data contract: unit/weapon/item/loot/prop IDs, scope/modifier/trigger, cause attribution, schema migration và gameplay/presentation boundaries.
- Lập decision register cho các điểm chưa xác minh: HP hiện tại hay max khi mua altar; chance theo eggs; merge weapon inheritance; rarity/pricing/interest; boss/Endless rules.
- Chỉ research bổ sung có mục tiêu cho những quyết định sắp implement. Công thức AUTHORED phải có nhãn/tuning rõ, không gọi là reference; không chặn K1/K2 vì một công thức altar chưa đủ evidence.
- Chốt sample typography/art/animation contract; font license/glyph cần kiểm trước khi ship, không tự coi custom font đã có.

**Đạt khi:** có baseline chạy được, snapshot/schema/rollback fixtures và danh sách rule còn mở; biết phần nào giữ nguyên, phần nào sửa, không mất thay đổi/save hiện tại.

### K1 — Camera, map, main menu và Details

**Phạm vi:** R1; N01/N05, khung N02/N17/N18. **Dependency:** K0.

- Map rộng hơn viewport, smooth follow/clamp, floor/boundary tách lớp; giữ vùng nhìn nhất quán theo aspect.
- Main menu đầy đủ landing/roster/resume/settings/help; warning overwrite và save error; font ba vai trò với glyph tiếng Việt.
- Details Stats/Companions/Relics kiểu hồ sơ: weapon/tier/stack/bond thật, scroll, pause/focus/back ownership; không selling/equipping.
- Dựng UI contract cho receipt/delta/build links và offscreen marker; chưa có rule/loot thật thì không hiển thị graph hoặc marker giả.

**Đạt khi:** đi tới cả bốn rìa map, camera không vượt bound; Details mở/đóng không đổi timer/RNG/build; menu/touch không rò input sang mua/combat; chữ tiếng Việt, giá/timer và list đội lớn đọc được.

### K2 — Death, coin và độ mượt nền

**Phạm vi:** phần R3/R5; S5, N12 cơ bản. **Dependency:** K1.

- Unit logical death tách lifetime body VFX; enemy/player/boss có sequence, hand/weapon cleanup và results timing phù hợp.
- Coin scatter/landing/spin/glint → attraction/collect → wallet pulse; credit đúng một lần, không fake coin để quay preview.
- Pose continuity qua impact, render interpolation, hit/muzzle đúng vị trí, fade VFX khi wave clear; không global hitstop làm sai simulation.
- Một mẫu death theo vật liệu làm đủ pipeline trước khi nhân sang các enemy family.

**Đạt khi:** kill/death/loot không dup; hurtbox tắt đúng tick; pause không credit; wave-end không cắt cue quan trọng; capture motion realtime và frame-time giúp phân biệt animation giật với FPS thấp.

### K3 — Stat, trigger, shop và giao dịch

**Phạm vi:** R2 và loot gameplay R3; S2, N02/N03/N07/N17/N18. **Dependency:** K0–K2.

- Modifier list/scope/rarity/stack rules, event/cause registry, deterministic RNG và save migration.
- Shop ba quầy, weighted pools, reroll counter/discount, altar eggs/HP/recipe theo decision register; rollback tiền/material/roster/RNG.
- Rương/trứng/hearts, Treasure Detection/Luck, pickup/chest events; ngoài viewport vẫn có loot logical hợp lệ.
- Receipt N03 + delta N17 từ before/after committed state; build links N02 từ rule data thật.
- Indicator N18 chỉ chest/egg đủ điều kiện, gộp/ưu tiên/fade và safe-area, không đánh dấu mọi coin.

**Đạt khi:** stat UI khớp combat sau mua/merge/resume; HP/eggs không bị thay bằng gold; failed transaction không đổi RNG; proc không recurse; marker đúng sau pickup/camera clamp/loot bị kéo; không giấu trade-off.

### K4 — Lát cắt signature hoàn chỉnh

**Tiến độ cuối:** K4.1–K4.5 đạt functional gate Windows theo `K4_Final_Checkpoint.md`. Plan/checkpoint chi tiết: `K4_Implementation_Plan.md`. Font/UI redesign vẫn để K6–K7; không tự nhận mobile, final art/audio/balance hoặc exact-reference parity đã nghiệm thu.

**Phạm vi:** R4/R5; S1–S4, N06/N07/N09/N10 và sandbox N16. **Dependency:** K3.

- Hoàn thiện Ô / Chuông / Đóng dấu: target, cadence/tật, pose, tier twist, VFX/SFX/UI riêng; regression cung/phép cũ.
- Hai chuỗi Máy thu hoạch / Văn phòng hoàn trả; ít nhất một connector, graph/contribution rõ và cooldown hữu hạn.
- Merge reveal/biến hình theo rule đã chốt, không tự đổi inheritance hoặc cho chọn weapon.
- Hai prop Chuông sân khấu / Xe đồ chơi, layout theo seed, không manual skill; một boss phase đổi sân có warning/fairness.
- Weapon sandbox N16 dùng cùng runtime rule nhưng data/RNG/save cô lập; preview read-only trước khi museum shell hoàn chỉnh.

**Đạt khi:** một lượt combat → loot → mua/ghép → build payoff → boss/rest chơi được và hiểu được; ba weapon khác nhau cả khi tắt particle; sandbox không farm hoặc đổi run/profile.

### K5 — Mở rộng gameplay theo quality gate

**Cập nhật 2026-10-14: DONE theo functional gate runtime Windows.** Đủ12 concept/6 build/6 prop,4 enemy behavior và3 boss motif;139 checks K5 + regression PASS,40 native assertions/15 ảnh Unity3840×2160, build0errors/0warnings. Xem `K5_Final_Checkpoint.md`. Phase kế tiếp K6/font/UI/museum; natural playtest, production/device gate vẫn K7. Các trạng thái lịch sử bên dưới không ghi đè checkpoint mới này.

**Phạm vi:** phần còn lại R4/R5; S1–S4, N06–N12. **Dependency:** K4 được kiểm chứng.

- Phủ đủ 12 weapon concept, 6 hướng build, 6 prop đã duyệt; giữ ID/assets cũ hợp lệ, không ép mọi concept thành cùng một ring/nổ.
- Bốn enemy behavior: đội cửa, nam châm, giả ngủ, kéo rèm; ba boss arena motif: văn phòng, đồ chơi, sân khấu.
- Archetype melee/bow/magic/mortar/economic khác luật thật; wave composition/curves, early/mid/late cadence và boss phases.
- Material death, idle personality, tier silhouette và art riêng cho family; không để ally đứng diễn thay vì đánh.

**Đạt khi:** từng family có sample/pattern/tooltip/art đủ; mỗi build có tác dụng khi mới có hai thành phần, có trade-off/điểm yếu; không item bắt buộc cho mọi build, không one-hit do base damage, không cap đội để né lag.

### K6 — Kết quả, museum và meta

**Feedback đã nhận (2026-10-13):** font và UI hiện tại chưa phù hợp chất lượng art. Chủ dự án đồng ý chưa redesign trong visual pass. K6 làm bộ UI đồng nhất (HUD/menu/Details/shop/results), font có cá tính arcade nhưng rõ dấu tiếng Việt, phân cấp chữ/số, spacing, icon và bảng giá gọn; giữ các thao tác và dữ liệu thật. K7 chốt kerning/glyph, transitions và nghiệm thu đa nền tảng. Đổi palette/bảng giá nhỏ trong visual pass không được tính là hoàn tất redesign này.

**Phạm vi:** R6; N04/N13–N16, contribution N02/N07. **Dependency:** K3/K4; content K5 bổ sung theo ID.

- Difficulty/challenge/unlock/score/records, Endless flow theo rule đã chốt; bảo vệ credit đúng một lần.
- Invoice results: đội/weapon/relic, contribution đo thật, cause-of-death; recap và nickname ổn định theo build.
- Museum discovery/collection/tier/chain/boss, tích hợp sandbox N16; preview không tự unlock hoặc tăng stat.
- Cosmetic bundle UI/receipt/stamp, history và profile migration; không đổi threat semantics hoặc combat values.

**Đạt khi:** resume/results/history không dup hoặc roll nickname; contribution không double-count overkill/chain; museum/sandbox/cosmetic không tạo power progression vĩnh viễn.

### K7 — Production polish và ba nền tảng

**Phạm vi:** R7, nghiệm thu toàn R1–R6/S1–S10/N01–N18. **Dependency:** các checkpoint liên quan đủ functionality; profile xuyên suốt từ K2.

- Audition và soundtrack/SFX nguyên bản có license, audio ensemble/layers/grouping/danger priority; không gọi waveform test là musical approval.
- Hoàn thiện font/kerning/glyph/license, UI transitions, art consistency; default/cosmetic/reduced-motion/readability đều đạt.
- Full-run/Endless balance và feel playtests, actual FPS/frame-time/memory/GC/GPU; capture đúng realtime, không video 12fps giả làm perf evidence.
- Android/iOS touch/safe areas/lifecycle/save/background audio; PC keyboard/controller/settings và Steam release prerequisites.
- Nếu thiếu module/device/certificate/account, ghi dependency cụ thể và kết quả phần đã chạy; không tự ghi release complete.

**Đạt khi:** báo cáo expected/observed, artifact/build có version/seed, các lỗi và dependencies còn mở minh bạch; art/audio/feel được duyệt riêng khỏi compile/test PASS.

## 3. Thứ tự và bàn giao

**K0 → K1 → K2 → K3 → K4 → K5 → K6 → K7.** K6 có thể làm nền contribution/profile sau K4 rồi nhận content K5; audio/perf/readability kiểm xuyên suốt, không đợi K7 mới phát hiện vấn đề. R8/co-op vẫn là nhánh scope riêng, không chen vào dependency solo.

Mỗi checkpoint bàn giao: danh sách file/data đổi; migration/backward compatibility; tests/regression thực chạy; Windows build chơi được khi runtime đổi; ảnh/clip Unity đúng setup; expected vs observed và phần còn thiếu. Không commit/branch hoặc overwrite real save nếu không được yêu cầu.

Không đặt deadline theo số lượng feature chưa đo. Nếu checkpoint chưa đạt quality gate, sửa/giảm độ phức tạp trong nội dung của nó trước khi nhân rộng, không silently bỏ tính năng đã duyệt.

## 4. Coverage và điểm bắt đầu

- Audit 57 mục giữ mapping R1–R8 trong `Reference_Parity_Backlog.md`; intentional differences vẫn giữ nguyên.
- S1–S5: K3–K5, presentation nền K2. S6–S10: K1/K3–K7.
- N01–N05: K1/K3/K6/K7; N06–N12: K2/K4/K5/K7; N13–N16: K4/K6/K7; N17: K1/K3; N18: K1/K3/K7.
- N16/N17/N18 đã duyệt đầy đủ; shell/demo/receipt placeholder không đủ đánh dấu DONE.

**Đã triển khai:** K0 baseline/contract, K1 camera/map/Details/main-menu, K2 death/coin/pose; đã recheck trước khi bắt đầu K3. K3.1 có giao dịch rollback/biên lai; K3.2a có additive modifier/scope/rarity/weighted shop/reroll/build links. Chưa hoàn tất toàn bộ K3/N17. Altar/economy reference, proc/loot và gate tiếp tục theo `K3_Implementation_Plan.md`; không tự mở rộng content K4/K5 trước khi K3 đạt gate. Windows build/capture không xác nhận platform release.
