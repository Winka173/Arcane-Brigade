# Arcane Brigade — UI, chữ, gameplay và cá tính giấc mơ

**Ngày:** 2026-10-13. **Trạng thái:** chủ dự án duyệt **toàn bộ brainstorm bổ sung**; tài liệu này chốt phạm vi thiết kế S6–S10, nối tiếp S1–S5 trong `Signature_Gameplay_Expansion.md`.

**AUTHORED / APPROVED DESIGN — chưa implement runtime, font asset, animation, audio hoặc balance.** Không phải mô tả tính năng đã xác minh của Bizarre Brigade. Việc duyệt hướng không đồng nghĩa mọi con số/asset đã nghiệm thu.

**Bổ sung đã duyệt:** N16–N18 (thử vũ khí trong bảo tàng, delta sau mua/ghép, chỉ báo loot ngoài viewport). Thứ tự triển khai và cổng nghiệm thu ở `Implementation_Roadmap.md`; đây là kế hoạch thực thi mới, không đổi trạng thái runtime.

## 1. Scope và giới hạn

Hướng nhận diện: **cửa hàng đồ thất lạc trong giấc mơ**. Đây là lớp nhận diện trong thế giới Arcane Brigade, không đổi tên game, source ID hoặc phá vòng chơi đã chốt.

- Giữ movement-only trong combat, không formation command, manual skill/knockback/dash/aim/đổi vũ khí.
- Shop rest có ba lựa chọn vật lý, gần quầy rồi Space/USE; không thay bằng bảng mua đồ toàn màn hình.
- Đồng minh bất tử, roam trong leash, không arm/forearm; blocking chỉ nhờ item cho phép.
- Không slot cap/upkeep/sell/lock, không permanent stat grind, quảng cáo/IAP/gacha trả tiền.
- Cosmetic không thay semantic màu đạn nguy hiểm, collider, timing hoặc khả năng đọc gameplay.
- Độc lạ ở hình/cơ chế/cá tính; không đảo điều khiển, xoay camera, giấu dữ liệu quan trọng hoặc làm UI chuyển động liên tục.

## 2. S6 — Giao diện là một phần thế giới

### N01 — Details dưới dạng hồ sơ đội quân

Giữ ba tab quen thuộc Stats / Companions / Relics; phần trang trí biến chúng thành bảng kiểm định, thẻ nhân viên và ngăn kéo kiểm kê.

- Companions: body portrait, vũ khí **thực tế đã roll**, tier, passive, bond và vai trò. Mini animation có thể dừng ở reduced-motion mode.
- Relics: icon riêng, số stack, rarity, scope, effect/trigger, cooldown và lợi/hại; không dùng body ally thay icon production của mọi item.
- Stats: tổng thực tế, base/bonus, nguồn đóng góp, Champion/companion/brigade. Nhãn trang trí không đổi vị trí số hoặc hide stat bất lợi.
- Frame/nav/scroll ổn định; không đẩy card lệch ngẫu nhiên như một đống giấy khó chạm. Đội không cap nên list phải chịu được số lượng lớn.
- Không biến inventory thành minigame sắp đồ hoặc thêm quyền bán/đổi vũ khí.

### N02 — Hiển thị các món đang nối build

Chọn một relic làm sáng các thành phần tương tác: nguồn → connector → payoff. Chỉ là read-only inspection, không kéo nối dây, không tự trang bị/thay effect.

- Graph lấy từ rule data, phân biệt **đang hoạt động**, **có thể phối hợp**, **chưa đủ điều kiện**. Không dùng tên/keyword giống nhau để invent synergy.
- Hiển thị lời giải thích ngắn và số thật khi tính được; không khẳng định bonus của món chưa sở hữu đang được áp dụng.
- Chuỗi dài gộp thành nhóm, mở chi tiết theo nhu cầu. Không render dây chằng chịt lên màn combat.
- Keyboard/touch/controller đều chọn/xem/đóng được, không bắt hover bằng chuột.
- Không đổi RNG, cooldown, stats, roster hoặc pause ownership khi chọn một node.

### N03 — Biên lai nhỏ sau khi mua

Biên lai có món nhận, giá/currency, vũ khí vừa roll, bond tăng và merge xảy ra. Nó xuất hiện sau **transaction thành công**, rồi thu gọn; không popup confirm thêm lần nữa.

- Failed purchase có phản hồi riêng, không in biên lai thành công. Save rollback không để receipt nói rằng đã sở hữu món mất đi.
- Mua nhanh nhiều lần dùng hàng đợi/gộp thông báo; lịch sử chi tiết xem được ở rest/Details, không che quầy hoặc cap số giao dịch.
- Receipt không là source of truth: skip, đóng UI, chuyển scene không trừ tiền/roll/tạo vật phẩm thêm lần nữa.
- Giá trị có scope rõ: ví dụ bond body trước/sau ghép, không quảng cáo ba body vẫn còn sau merge thành một.

### N04 — Hóa đơn kết quả run

Results có phong cách hóa đơn dài, với đội/vũ khí/relic, score/record, contribution và nguyên nhân chết thật. Các nhãn hài như “quái đã xử lý” hoặc “chuông đã làm phiền hàng xóm” chỉ bổ sung tính cách.

- Contribution phải đo từ gameplay; damage dùng policy rõ cho overkill/proc/zone/prop, không suy từ rarity/tier hoặc chia đều cho cả đội.
- Cause-of-death vẫn hiện nguồn, loại và damage; humor không chế giễu player hoặc thay phần giúp học từ thất bại.
- Hóa đơn có scroll/page và quick summary; không bắt xem animation cuộn dài mới được replay/menu.
- Results đọc trạng thái đã chốt; mở lại không credit record/unlock hai lần.
- Export ảnh hóa đơn là tiện ích cosmetic tùy chọn, mặc định local; không tự upload dữ liệu hoặc tích hợp mạng trong milestone solo.

## 3. S7 / N05 — Hệ chữ ba vai trò

Chốt **vai trò và phong cách**, chưa chốt tên font, license hay file font. Font thật phải có quyền dùng/nhúng/phân phối cho Android, iOS, PC trước khi đưa vào build.

| Vai trò | Hướng thiết kế | Điều không được làm |
| --- | --- | --- |
| Logo, boss, banner | Chữ khối hơi méo như biển hiệu tự dựng; silhouette mạnh | Không dùng cho tooltip dài hoặc ép méo tới mức đọc sai tên |
| Menu, mô tả, tooltip | Chữ tròn gọn, nét rõ, đầy đủ dấu tiếng Việt | Không all-caps toàn đoạn, không chữ lắc hoặc dấu bị cắt |
| Damage, giá, timer | Chữ số chắc, độ rộng ổn định, phân biệt rõ ký tự | Không giá/timer đổi độ rộng mỗi tick, không khiến 0/O hoặc 1/l dễ nhầm |

Các accent đã duyệt:

- Boss name có animation như con dấu đóng xuống.
- SOLD dùng dấu mực trên quầy; vẫn có trạng thái disabled/tên dễ đọc.
- Crit bật/nghiêng nhẹ rồi hồi, không rung liên tục.
- Banner merge có một chữ lệch rồi được nắn lại; giảm motion thì dùng banner tĩnh.
- Tooltip, giá và timer không animation letter-by-letter; animation không thay logical text value.

### Font/UI contract

- Bộ glyph tiếng Việt đủ, including Đ/đ và dấu kết hợp; fallback có cùng khả năng đọc. Kiểm tra cả chuỗi có nhiều dấu, không chỉ ASCII.
- Chuỗi kiểm tra: **“Đội quân thất lạc — Ống kính ngái ngủ | 1.234 vàng / 00:59 | Đã mua”**.
- Số tiền lớn, số âm, %, tier, tên weapon dài, stack cao và text localization phải wrap/scale có giới hạn, không tràn safe area.
- Dấu không bị cắt ở line-height nhỏ; đối chiếu desktop và mobile landscape. Body font không chỉ đẹp trong logo screenshot.
- Weight/outline/shadow dùng có chọn lọc: ưu tiên tương phản và danger readability, không tô viền dày mọi chữ.
- Font bitmap/SDF/vector pipeline lựa chọn theo chất lượng thực và renderer hiện có; không tự migrate URP/UI toàn dự án chỉ vì brainstorm font.
- Hoàn thiện glyph/kerning/license/previews là asset task riêng; tài liệu này không khẳng định đã tạo custom font.

## 4. S8 — Gameplay bất ngờ nhưng có thể học được

### N06 — Đồ vật có tật

Weapon cadence có quy luật: máy ảnh chớp mắt trước khi chụp, búa nặng có cú cuối chậm, hộp nhạc hụt nhịp rồi bù bằng accent mạnh.

- Tật có anticipation đọc được và được ghi trong description; **không RNG giấu kín làm miss**, không input mới.
- Nếu một “hụt nhịp” chỉ là animation, không mất logical damage. Nếu thật sự đổi cadence/damage, data/tuning/tooltip phải nói rõ.
- Attack-speed modifier tác động nhất quán lên windup/recovery; không làm shot xuất hiện trước muzzle, bỏ impact hoặc reset cooldown khi target đổi.
- Đặc tính cơ bản nhận ra ở tier đầu; tier twist nâng bản sắc, không biến mọi món thành cùng multi-shot explosion.

### N07 — Relic nối hai build

| Relic concept | Kết nối | Guardrail |
| --- | --- | --- |
| Mực chịu nhiệt | Dấu trên quái tương tác với vũng trà | Một reaction theo cooldown; zone tick không tự nhân reaction vô hạn |
| Dây đàn bằng giấy | Mục tiêu được cung nối truyền một nhịp chuông | Target list hữu hạn; nhịp được truyền không tự truyền lại |
| Ống kính thu ngân | Nhặt coin tích năng lượng cho lần chụp kế tiếp | Coin logical credit một lần; chụp không sinh coin từ coin |

Đồ nối phải hữu ích với một phần build, không chỉ hoạt động khi đủ hai bộ hiếm. Details cho biết nhánh nào active/tiềm năng; không tự gắn perk nếu player chỉ đang xem codex. Rarity tăng cơ hội kết nối không phải luôn tăng DPS vượt mọi item thường.

### N08 — Enemy family có hành vi kỳ quặc

| Enemy concept | Quyết định vị trí tạo ra | Fairness và readability |
| --- | --- | --- |
| Kẻ đội cửa | Che phía trước, đội đánh từ góc khác hiệu quả hơn | Hướng chắn nhìn thấy; không bất tử toàn bộ hoặc yêu cầu manual aim |
| Quái nam châm | Kéo coin/loot tới quanh nó, trở thành mục tiêu đáng săn | Không xóa/ăn mất loot; scope/radius có giới hạn, không hút vô hạn toàn map |
| Kẻ giả ngủ | Thức dậy tạo đợt áp lực có báo hiệu | Telegraph trước activation, không xuất hiện dưới player rồi one-hit |
| Quái kéo rèm | Đổi lớp nền không khí trong vùng | Không che Champion/đạn/telegraph/UI; accessibility giảm hiệu ứng vẫn thấy threat |

Các family phải có silhouette/attack khác nhau thật, không chỉ đổi màu và HP. Drop/elite/difficulty configuration riêng; không force spawn mọi loại từ wave đầu. Nam châm giữ logical pickup identity/value, không tạo bản sao khi nhả loot hoặc bị giết.

### N09 — Boss thay đổi sân theo phase

- **Boss văn phòng:** đánh dấu khu vực sắp bị thu hồi; vùng báo trước rõ và còn đường né.
- **Boss đồ chơi:** cho đường ray xuất hiện, xe chạy trên lane có warning/cooldown.
- **Boss sân khấu:** chuyển vị trí các chuông giữa phase, không teleport player hoặc ally để đẹp VFX.

Đây là modifier arena gắn boss, không thay camera/điều khiển. Transition bảo đảm no-spawn-on-player, không wall kẹt, không lấp toàn sân bằng hazard. Damage từng nguồn phải giữ cap/fairness đã chốt, kể cả nhiều nguồn hợp lệ cùng tick. Boss chết dọn hazard nguy hiểm theo rule, giữ death/loot presentation đủ lâu rồi mới results/rest.

## 5. S9 — Nhân vật, âm thanh và vật liệu có cá tính

### N10 — Phản ứng nhỏ của đồng minh

Các idle/rest beat đã duyệt: ấm trà nhìn túi tiền; hộp thư nhả rồi nuốt thư; chuông nứt rung thử; đồng minh mới ghép thử pose với weapon thật.

- Chỉ idle/rest hoặc animation không chặn combat. Không đứng diễn khi cần attack/target, không thay logical leash/velocity/collider để làm trò.
- Hand vẫn gắn weapon, không mọc arm trong clip mới. Hộp thư/ấm trà/chuông vẫn đọc silhouette khi thu nhỏ.
- Trigger ít và có cooldown, dùng cosmetic RNG riêng hoặc timing ổn định; không đổi combat/drop/shop/weapon RNG.
- Animation priority: death/hit/attack/telegraph trước idle. Reduced motion vẫn giữ role/tier/weapon readability.

### N11 — Đội quân như một dàn máy móc

Chuông, đóng dấu, hộp nhạc và coin có texture riêng; grouped accents khiến những chuỗi thành công nghe có nhịp, thay vì hàng trăm voice cùng lúc.

- Không delay impact gameplay để ép vào beat nhạc; grouping/pitch/voice selection chỉ presentation.
- Music dùng layer phụ nhẹ theo intensity/payoff/boss, không bắt thay cả soundtrack khi build đổi.
- Danger/low-health/telegraph SFX có priority; đánh dấu/merge/coin không che cue cần phản ứng.
- Cadence/cooldown/voice grouping không cap damage hoặc số ally. Combat logs không dùng số audio voice làm số proc.
- Settings cần music/SFX volume phù hợp, reduced-audio density nếu cần; toàn bộ nhạc/SFX tạo riêng hoặc có license.
- Audition/mixing vẫn là việc cần làm; audio chưa nghe được không được ghi PASS chỉ vì waveform không rỗng.

### N12 — Chết theo vật liệu

Quái mực tan thành giọt, đồ sứ rạn/vỡ, vải xẹp, máy móc rã; mỗi family có animation silhouette, fragment shape và SFX riêng.

- Material cue nằm trong death presentation data; không chỉ một puff đổi tint.
- Logical death tắt hurtbox ngay, credit kill/drop một lần; mảnh vỡ không là projectile hoặc actor gameplay.
- Enemy/elite/boss/player có scale và sequence khác nhau; không dùng nổ lớn cho mọi target.
- Quái Nam châm nhả loot đang giữ bằng identity thật, không credit mới. Boss hazard cleanup không bị death clip trì hoãn trái rule.
- Effect fade/finish được khi wave vừa kết thúc; không tồn dư cả đống fragment trong rest hoặc chiếm VFX budget che danger.

## 6. S10 — Meta không tăng sức mạnh

### N13 — Bảo tàng đồ thất lạc

Codex được trình bày như phòng trưng bày: weapon đã gặp, biến thể tier đã ghép, chain đã kích hoạt và boss đã đánh bại. Mở khóa hình/ghi chú/preview/cosmetic, không tăng stat.

- Discovery dựa trên event thật, ID ổn định; xem codex không tự đánh dấu đã sở hữu/ghép/clear.
- Preview animation không ghi vào run, RNG, score hoặc unlock; lựa chọn locked có tiến độ/điều kiện rõ.
- UI phục vụ keyboard/touch/controller, không bắt player đi qua một museum 3D để tìm thông tin.
- Collection hỗ trợ save migration và profile backup. Thành tích/record credit đúng một lần, không bị tăng lại khi load results.

### N14 — Biệt danh riêng cho run

Tên gợi chuyện dựa trên build/contribution thật: “Phòng hoàn trả không nghỉ”, “Tiệc trà cháy bếp”, “Dàn chuông đòi nợ”. Lưu cùng kết quả/lịch sử run; tên chỉ trang trí, không là rule ID hay nguồn thưởng.

- Rule đặt tên dùng tags/evidence rõ; không gọi run là build trà khi không có trà.
- Tie-breaking ổn định, có tên fallback nếu build hỗn hợp; reload/history không roll tên mới hoặc đổi score.
- Tên khác locale vẫn trỏ cùng run identity; không dùng tên hiển thị để deduplicate record.
- Không cần network để tạo tên; tránh dependency account/cloud và nội dung sinh ngẫu nhiên khó kiểm soát.

### N15 — Cosmetic đồng bộ cả trải nghiệm

Bundle ví dụ “văn phòng đêm” đổi stamp mua đồ, receipt paper, UI frame và trang trí. Các motif sân khấu/đồ chơi có thể nối với S4, nhưng không đổi semantic danger.

- Unlock qua chơi/challenge/discovery, không gacha/IAP hoặc stat buff. Không cần sở hữu bundle để hiểu game.
- Không đổi hitbox, opacity/shape của enemy projectile, telegraph timings, text-size floor hoặc color coding quan trọng.
- Cosmetic layer nằm ngoài combat/save rule data; đổi bundle không restart wave, roll shop hoặc hồi HP.
- Có default theme đọc tốt và high-readability/reduced-motion options; bundle mới phải qua cùng QA với default.

## 7. Ba bổ sung cuối — N16–N18

### N16 — Thử vũ khí trong bảo tàng

Chọn weapon đã khám phá để xem đồng minh tự đánh mục tiêu mẫu. Có cadence, target behavior và tier preview; không yêu cầu điều khiển đánh, không thưởng tài nguyên.

- Sandbox dùng bản sao data và RNG riêng; không chạy vào run đang lưu, không tăng discovery/score/challenge/contribution.
- Dùng cùng rule/pose/VFX pipeline với gameplay để demo không quảng cáo sai. Có nhãn rõ cho tier/modifier đang preview, không giả bộ player đã sở hữu tier đó.
- Reset/đổi mẫu/thoát không thay profile/build/currency/RNG; không trao item hoặc stat bonus từ lượt thử.
- Keyboard/touch/controller đều dùng được; mục tiêu đứng yên/di chuyển chỉ là preset, không thêm skill combat.
- Nền demo gọn, phép thử không phải arena riêng để farm. Có thể triển khai sandbox cô lập trước khi museum shell hoàn chỉnh.

### N17 — Tóm tắt delta sau mua/ghép

Mở rộng receipt N03: nêu **được gì, mất gì, chuỗi nào vừa hoạt động**, dựa trên snapshot trước/sau transaction.

- Hiển thị gold/eggs/HP thực chi, item stack, weapon thực roll, tier/roster và bond trước/sau. Ghép ba body thành một không được quảng cáo giữ nguyên cả ba body.
- Chỉ báo chain mới active từ rule data N02; không invent synergy hoặc nhầm effect tiềm năng thành active.
- Failed purchase/rollback không có delta thành công; không tính lại giao dịch khi mở receipt hoặc resume.
- Gộp thông báo mua/merge liên tiếp, xem lại chi tiết tại rest; không thêm modal hoặc nút confirm bắt buộc. Các khoản âm/đánh đổi rõ, không giấu sau nhãn hài.

### N18 — Chỉ báo loot ngoài góc nhìn

Chỉ báo nhỏ cho **rương/trứng đáng chú ý**, không đánh dấu mọi đồng xu. Mục tiêu dẫn hướng trong bounded map lớn hơn viewport mà không phủ arena bằng mũi tên.

- Tính từ vị trí loot thật và camera viewport hiện tại, không từ kích thước toàn Screen; tôn trọng letterbox/safe area/HUD.
- Dùng icon/shape phân biệt chest/egg, gộp gần hướng, ưu tiên và giới hạn marker **hiển thị**, không xóa/cap logical loot.
- Fade/hide khi loot vào viewport, đã lấy/mất hiệu lực hoặc phase không cần chỉ báo. Camera clamp, đổi aspect và nam châm kéo loot vẫn cập nhật đúng.
- Không auto-path hoặc tự kéo người chơi; không marker xuyên map cho mọi quái/xu. Có tùy chọn giảm/tắt indicator.
- Nếu sau này có loot discovery restriction, marker phải dùng đúng eligibility; không vô tình lộ vật phẩm ẩn.

## 8. Coverage và dependency

| ID | Hạng mục đã duyệt | Hệ signature | Milestone nối vào |
| --- | --- | --- | --- |
| N01 | Details hồ sơ | S6 | R1, R6 |
| N02 | Build links read-only | S6 / S2 | R1, R2, R6 |
| N03 | Purchase receipt | S6 / S5 | R2, R5 |
| N04 | Results invoice | S6 / S5 | R6 |
| N05 | Typography ba vai trò + accent | S7 | R1, R5, R7 |
| N06 | Weapon quirks | S8 / S1 | R2, R4, R5 |
| N07 | Cross-build connectors | S8 / S2 | R2, R4, R6 |
| N08 | Enemy behavior families | S8 | R3, R4, R5 |
| N09 | Boss arena phases | S8 / S4 | R1, R4, R5 |
| N10 | Companion idle personality | S9 | R5 |
| N11 | Audio ensemble/layers | S9 / S5 | R7 |
| N12 | Material death | S9 / S5 | R3, R5, R7 |
| N13 | Lost-object museum | S10 | R6 |
| N14 | Run nicknames | S10 | R6 |
| N15 | Cosmetic bundles | S10 | R6, R7 |
| N16 | Museum weapon sandbox | S10 / S1 | R4, R6 |
| N17 | Purchase/merge delta summary | S6 / S2 / S3 | R1, R2, R5 |
| N18 | Offscreen chest/egg indicators | S6 / S4 | R1, R3, R7 |

Toàn bộ 18 hạng mục giữ trong scope đã duyệt. Triển khai từng gói để đạt chất lượng không có nghĩa bỏ các phần còn lại; không ghi DONE chỉ vì có theme màu hoặc vài label hài.

R1 dựng interaction/Details/main menu cùng typography cơ bản; R2 tạo truth data cho graph/receipt/proc; R3/R4 đưa build, loot, enemy/boss thành gameplay thật; R5 làm motion/death/personality/reveal; R6 nối contribution/results/museum/history/cosmetic; R7 nghiệm thu audio/font/device/readability. Không dựng build graph đẹp nhưng không khớp rule data, hoặc invoice đẹp với contribution bịa.

## 9. Nghiệm thu phần bổ sung

- **Coverage:** N01–N18 đều có owner milestone; không mất một mục vì chỉ nằm trong brainstorm.
- **Sandbox/delta/indicators:** demo không farm/mutate run; delta khớp committed transaction; marker chỉ chest/egg đủ điều kiện, không thay loot identity/value hoặc che danger.
- **Read-only UI:** mở/chọn/đóng hồ sơ/graph/codex không đổi combat/RNG/timer/currency/unlock ngoài hành động thật.
- **Transaction:** receipt/results/discovery chỉ phản ánh trạng thái committed, rollback/skip/reload không dup.
- **Typography:** tiếng Việt đủ glyph/dấu; giá/timer/crit rõ ở mobile, mọi theme; font license ghi nhận trước ship.
- **Weapon quirks:** timing/power đúng data và tooltip; animation không miss/skip logical attack vì lỗi pose.
- **Enemy/boss:** nguy hiểm báo trước, loot identity đúng, không one-hit do base damage, không che đạn hoặc đổi control/camera.
- **Personality:** idle không cản attack/roam, không arm/forearm, cosmetic RNG không ảnh hưởng combat.
- **Material/audio:** death đúng family, no duplicate drop; SFX grouping không ảnh hưởng event count; audition thật trước approval.
- **Meta:** museum/nickname/cosmetic không tăng permanent stats hoặc đổi gameplay; history/record ID ổn định qua migration.
- **Quality:** default + cosmetic + reduced-motion/readability modes kiểm tra bằng actual Unity frames/playtest và frame-time; tài liệu không thay build/device validation.
