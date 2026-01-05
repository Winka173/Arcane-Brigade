# Backlog triển khai sau audit Bizarre Brigade 1.5+

Ngày 2026-10-13. Chuẩn tham chiếu do chủ dự án chọn: **1.5+**. Nguồn, mốc footage, mức xác nhận và khác biệt chủ đích ở `Bizarre_Brigade_15_Reference_Audit.md`.

Trạng thái mới: R1 có Windows candidate K1; death/coin/pose thuộc R3/R5 đã có nền K2. Các mục còn lại vẫn PLANNED; không đánh dấu toàn R3/R5 hoàn tất. Báo cáo thực chạy: `K0_K2_Implementation.md`. P3–P6 trước đó là functional candidate, không phải completion của backlog này.

**Scope nguyên bản mới đã được duyệt:** năm hướng S1–S5 trong `Signature_Gameplay_Expansion.md`: vũ khí đồ vật, build dây chuyền, biến hình khi ghép, đạo cụ arena và phản hồi combat/loot nhiều lớp. Đây là AUTHORED design, không phải reference parity. Không đưa ý tưởng formation/chỉ huy bằng movement vào scope; combat vẫn chỉ di chuyển.

**Duyệt toàn bộ brainstorm bổ sung:** `Nightmare_Identity_UI_Gameplay.md` chốt S6–S10, coverage N01–N15. Các tính năng vẫn PLANNED; theme mới không thay core rules hoặc mở rộng multiplayer scope.

**Bổ sung cuối đã duyệt:** N16 weapon sandbox, N17 delta mua/ghép, N18 offscreen chest/egg indicators; tổng coverage N01–N18. Thứ tự thực thi chính: K0–K7 trong `Implementation_Roadmap.md`; R1–R8 giữ vai trò mapping phạm vi, không là lịch implement bắt buộc theo thứ tự nguyên khối.

## R1 — Sửa nền tảng không gian và đọc build

Phạm vi: E09/E10, B01–B08, A01/A05.

- Arena giới hạn nhưng rộng hơn view; camera theo nhân vật, smooth/clamp, shop framing riêng; edge props không stretch cùng sàn.
- Details ba tab: Stats, Companions, Relics; data thật, tier/weapon/stack/scope/bond, scroll và context tooltips.
- Main menu tách landing/roster/resume/records/settings/help; focus/back/confirm, cảnh báo overwrite và save error.
- Giữ movement-only và proximity shop; không để nút overlay rò input vào mua đồ/combat.
- N01/N02/N05/N09: Details hồ sơ, graph synergy read-only, typography ba vai trò và camera/bound hỗ trợ boss arena phase; không để ornament cản navigation/readability.
- N17/N18: UI contract delta committed và marker theo viewport/safe area; không receipt/marker giả trước khi có data thật.

Nghiệm thu: di chuyển từ tâm đến wall trong cả bốn hướng, camera không vượt bound; mở/đóng Details ở combat/rest/paused/focus loss không đổi timer/gold/RNG; touch không mua nhầm; inventory đủ đội lớn không bị cắt mất.

## R2 — Event, stat và giao dịch có trade-off

Phạm vi: C04–C10, D02–D08, E05.

- Stat scope Champion/companion/brigade; flat/percent modifiers, rarity/category, modifier list và trigger registry.
- Reroll tăng giá theo counter/config, eligibility và discount; trạng thái nằm trong save.
- Altar: chance gắn eggs, offer có currency eggs/HP, exclusive pool; recipe/material/tier thật; không giữ lịch cứng hai hall như “giống BB”.
- Chốt semantics HP bằng evidence trước coding; không tự đổi sang trả max HP/current HP mà thiếu nhãn rõ.
- Negative stats, conditional buffs, stacking/unique policy; chống proc recursion, economy exploit và rollback lỗi.
- S1/S2/S3: data weapon family/tier twist, trigger graph hữu hạn, source attribution và item nhiều modifier; merge reveal không thay inheritance policy hoặc roll lại transaction.
- N02/N03/N06/N07: truth data cho build links/receipt, cadence quirks và ba connector relic concepts; graph đọc rule thật, receipt sau commit, proc chain hữu hạn.
- N17: diff trước/sau giao dịch gồm cost/stack/weapon/tier/roster/bond/chain mới active; merge giảm body phải hiện đúng, rollback không báo thành công.

Nghiệm thu: bảng stat khớp gameplay sau mua/ghép/resume; gold không thay thế eggs/HP; failed purchase không đổi stats/RNG; probability và price tables phải ghi nguồn hoặc ghi AUTHORED.

## R3 — Loot và mục tiêu di chuyển

Phạm vi: E01–E05, C06, F05.

- Coin scatter/bounce/landing/glint, pickup attraction và collect trail/wallet pulse; credit đúng một lần.
- Egg drop, heart pickup, chest entity/open/rewards/quality; Treasure Detection và Luck có tác dụng gameplay.
- Chest/coin/kill/heal triggers cho build; khác biệt từng loại loot có cả art và sound.
- Chest interaction phù hợp movement-only: không thêm nút combat; quy tắc tự mở/đi qua cần nhất quán với luật đã chốt.
- S2/S5: pickup nối build Máy thu hoạch/Thợ săn kho báu, scatter → landing → attraction → collect → wallet; coin-trigger không tự sinh coin, đủ cue riêng cho chest/heart/egg.
- N08/N12: loot bị Quái nam châm kéo giữ identity/value, nhả không duplicate; death theo vật liệu không tạo actor/drop thêm.
- N18: chỉ báo rương/trứng ngoài viewport, gộp/ưu tiên/fade, không marker từng coin hoặc thay số logical loot.

Nghiệm thu: game pause không hút/credit loot; coin ngoài screen không mất; collected/discarded/settled không dup; drop seed ổn định; có lý do thật để người chơi chạy đi lấy rương/trứng/hearts.

## R4 — Combat và archetype thực sự khác nhau

Phạm vi: A03, C01–C04/C06, E06–E08.

- Archetype melee, bow/magic, mortar/trap và economic/chest engine; không làm thêm Champion bằng đổi màu cùng attack.
- Bounce/division/pierce/size/range, explosion AoE/fuse, exceptions/caps theo weapon, target behavior theo role.
- Enemy family/elite/boss có silhouette và pattern riêng; wave table/curves, stage modifiers và boss intro/phases.
- Tối thiểu ba hướng build có loop khác nhau, có điểm yếu và trade-off; giữ nhiều hit đồng thời và không base one-hit.
- S1/S2/S4: lát cắt Ô/Chuông/Đóng dấu; Máy thu hoạch/Văn phòng hoàn trả; Chuông sân khấu/Xe đồ chơi. Mở rộng 12 concept weapon, 6 build và 6 prop theo quality gate, không chỉ thêm skin.
- N06–N09: weapon quirks/connector relics, bốn enemy behavior concepts và ba boss arena motifs; telegraph/fairness, không manual aim/skill hoặc đổi control/camera.
- N16: weapon sandbox cùng rule/pose pipeline, clone data/RNG riêng và preset target/tier; không nhận thưởng hoặc ghi vào run/profile.

Nghiệm thu: high-speed swept collision, không re-hit cùng target ngoài luật; count proc có giới hạn an toàn không cap roster; damage attribution và VFX không thêm hit; run đầu/giữa/cuối có cadence khác nhau.

## R5 — Animation, death và cảm giác chuyển động

Phạm vi: B09, F01–F07, D01.

- Body death lifetime riêng khỏi logical Alive; flash/collapse/dissolve/fragments, cleanup weapon/hand và coin release timing.
- Player defeat và boss death có sequence; results không che mất khoảnh khắc kết liễu.
- Pose continuity ở anticipation/impact/recovery, smooth facing, trails theo weapon family; bow/magic/melee/mortar không chung một ring.
- Merchant/stand/altar/exit art nguyên bản, rarity glow, purchase/merge/tier reveal và UI transitions.
- Capture motion 60fps đúng realtime, không kết luận qua clip 12fps; không sửa simulation bằng render easing.
- S3/S5: tier silhouette/behavior, merge reveal, pose continuity, family-specific VFX và toàn chuỗi hit/death/loot; không Clear cue ngay wave-end hoặc credit theo animation hai lần.
- N03/N05/N06/N08/N09/N10/N12: purchase receipt, stamp/merge/crit chữ, personality và material death; idle không cản combat, death không trì hoãn hurtbox cleanup.

Nghiệm thu: frame-by-frame theo từng weapon, kiểm tra cả đánh trái/phải/lên/xuống và cỡ nhỏ; particles/death vẫn kết thúc khi wave vừa clear; camera/VFX không gây chóng mặt, che đạn nguy hiểm hay drop FPS.

## R6 — Progression, difficulty và kết quả

Phạm vi: A02/A04/A06–A08, B03–B05.

- Difficulty/challenge records, unlock conditions, progress preview và notifications; không candy permanent stats.
- Live score/scoring policy rõ; damage/heal/kill contribution theo đơn vị, result build grid và records theo Champion/mode.
- Endless gate/flow và scaling riêng sau khi chốt reference; save migration và replay seed diagnostics.
- S2/S3/S5: build recap, proc contribution, weapon/tier thật và trigger explanations trong Details/codex/results; không suy diễn số contribution chưa đo.
- N01/N02/N04/N07/N13/N14/N15: invoice results, build recap, bảo tàng đồ thất lạc, nickname ổn định và cosmetic bundles; discovery/contribution dựa event thật, không permanent stat grind.
- N16: tích hợp weapon sandbox vào museum đã khám phá, hỗ trợ preview tier không claim đã sở hữu, đóng/demo/reset không mutate save.

Nghiệm thu: unlock/record credit đúng một lần; resume giữ difficulty/build; score không double-count overkill/chain hit; endless không reset nhầm build hay trộn record run thường.

## R7 — Audio, accessibility và production QA

Phạm vi: B07/B08, F08/F09, A10.

- Audition reference bằng phương tiện nghe được; compose/licence soundtrack nguyên bản, SFX đủ menu/shop/loot/death/boss/merge; audio buses/ducking.
- Settings/control hints, readability, touch safe areas, controller navigation, options giảm số/flash/shake chỉ presentation.
- Full-run playtests, frame-time/memory/GC/GPU trên cấu hình thực; Android/iOS lifecycle, save recovery, landscape ratios và Steam build/release prerequisites.
- S5: danger-first visual/audio priority, coin sound grouping, local presentation accents thay global hitstop; giảm VFX không được cap ally hoặc silently bỏ damage.
- N05/N11/N12/N15: font license/glyph tiếng Việt, audio ensemble/layers/material cues, QA cosmetic/reduced-motion/readability; cosmetic không đổi semantic danger hoặc gameplay.
- N18: options/readability và mobile safe-area cho indicators; giảm marker chỉ presentation, không xóa loot hoặc auto-path player.

Nghiệm thu: báo cáo actual vs expected và video realtime; compile/check count không thay thế sound/art approval hoặc device testing. Không tự ghi Android/iOS/Steam PASS từ Windows.

## R8 — Shared Dream nếu mở rộng phạm vi

Phạm vi: A09 và T5.

- Hiện chỉ là nhánh nghiên cứu mở rộng, chưa được xác nhận là milestone release của Arcane Brigade.
- Cần chốt local hay online, số người, nền tảng crossplay/hosting, pause/disconnect/resume và authority của currency/build/proc.
- Không đặt multiplayer lên vòng chơi chưa đạt chất lượng solo.

## Definition of done cho toàn backlog

Parity cố ý không áp dụng: **C11** (manual knockback), upkeep T4 và candy permanent stats. Giữ guardrail và regression để các cơ chế này không bị thêm trở lại chỉ vì xuất hiện trong footage. A09/R8 vẫn là nhánh scope cần chốt, không phải missing solo feature có thể lấp bằng một nút menu.

Mỗi feature cần: evidence/source hoặc nhãn AUTHORED; spec/data; runtime; art/motion/audio/UI liên quan; save/migration; test; capture Unity; playtest/approval. Một hàng thiếu bất kỳ phần cần thiết nào chỉ là PARTIAL.

Thứ tự dependency: R1 → R2 → R3/R4 → R5 → R6 → R7. R8 không được chen vào dependency solo nếu chưa chốt scope. Toàn bộ ID trong audit phải nằm trong milestone hoặc mục intentionally different/out-of-scope; không bỏ quên vì chưa được người dùng liệt kê.

Lát cắt signature đầu tiên đi cùng R2–R5, sau nền R1; không trì hoãn death/coin/Details chỉ để vẽ thêm weapon. Coverage: S1 → R2/R4/R5; S2 → R2/R3/R4/R6; S3 → R2/R5/R6; S4 → R1/R4/R5; S5 → R3/R5/R7. Các concept còn lại giữ trong backlog mở rộng, không ghi DONE sau lát cắt đầu tiên.
