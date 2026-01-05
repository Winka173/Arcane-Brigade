# Bizarre Brigade 1.5+ — đối chiếu toàn hệ thống

Ngày rà soát: 2026-10-13. Chủ dự án đã chọn **bản mở rộng 1.5+** làm chuẩn. Đây là **research + gap audit**, không phải báo cáo các tính năng đã implement. Build hiện tại chưa thay đổi trong lượt research này.

## 1. Cách đọc và độ tin cậy

- **V**: đã nhìn thấy trong footage gameplay, kèm mốc thời gian. Không suy ra công thức chỉ từ hình.
- **T**: nội dung tutorial của game được database tái hiện; cần chú ý phiên bản database, không tự coi mọi giá trị là bản 1.5.
- **O**: thông báo chính thức.
- **S**: nguồn cộng đồng; chỉ dùng bổ trợ, không dùng làm chuẩn tuning khi mâu thuẫn footage.
- **U**: chưa xác minh đủ; cần footage/thử nghiệm bổ sung trước khi ghi là giống reference.
- **D**: khác biệt có chủ đích do chủ dự án chốt; không được tự sửa để chạy theo reference.

Không có quyền truy cập trực tiếp phiên bản ZZZ đang chạy để test mọi item. Research này dùng hai run 1.5 khác hướng build, một video 1.2 để so sánh, tutorial, patch notes và nguồn cộng đồng. Sampling theo mốc và các chuỗi frame ngắn **không đồng nghĩa đã xem liên tục từng frame của tất cả run**, cũng không bao phủ toàn bộ roster/relic hay mọi update sau 1.5.

## 2. Nguồn và evidence ledger

### Footage đã tải và kiểm tra hình

| ID | Nguồn | Phiên bản / ngày video | Mốc hữu ích đã kiểm tra |
| --- | --- | --- | --- |
| V1 | [Belle, Difficulty 5](https://www.youtube.com/watch?v=70OufN07abE) | Tiêu đề ghi 1.5; 2025-01-28; 968 giây; 720p/60 fps | 5s chọn nhân vật; 15–30s đầu run; 30.05–30.40s hit/death/coins; 45–60s shop; 120/180/300s build/shop; 240s sát rìa; 480s altar; 720–930s late combat; 945s kết quả |
| V2 | [Boo Fortress, Difficulty 5](https://www.youtube.com/watch?v=tqvQPCo51xc) | Tiêu đề ghi 1.5; 2025-01-30; 1118 giây; bản tải 360p/30 fps | 5s luật nhân vật; 15–30s mortar; 60s projectile bounce; 120s companion; 470–495s shop, Stats, Relics; 960s trade-off kinh tế; 1080s late combat; 1100s kết quả |
| V0 | [Video reference cũ](https://www.youtube.com/watch?v=0RcLBt0125A) | 2024-10-12, trước 1.5; 1080p/60 fps | 0s chọn nhân vật; 3s chọn độ khó; 45s shop; 135s giá trứng và HP trên cùng altar; 215s defeat; 400s Details; 430s altar |

Frame nghiên cứu ở `.utmp/art-reference/frames/`, prefix `belle15_`, `fortress15_`, `reference_`. Video và frame **không nằm trong Assets, không đóng gói vào game**. Bộ decode không chỉnh sửa hình; chỉ lấy frame từ video. V2 có chữ nhỏ khó đọc ở 360p: không dùng nó để chốt các con số mờ.

### Tutorial và thông báo

- T1: [Altar activation](https://zzz.honeyhunterworld.com/999130701-tutorial/?lang=EN): lượng trứng giữ ảnh hưởng khả năng altar xuất hiện.
- T2: [Altar purchases and formula](https://zzz.honeyhunterworld.com/999130702-tutorial/?lang=EN): có giao dịch bằng **trứng hoặc HP** và công thức đồng minh đặc biệt. Search trả nội dung; mở trực tiếp bị 403 trong lượt này.
- T3: [Rest purchases](https://zzz.honeyhunterworld.com/999130601-tutorial/?lang=EN): mua đồng minh/item trong rest.
- T4: [Companion upkeep](https://zzz.honeyhunterworld.com/999130603-tutorial/?lang=EN): reference có phí đồng minh; **Arcane Brigade không áp dụng** theo yêu cầu.
- T5: [Shared Dream coin sharing](https://zzz.honeyhunterworld.com/999130803-tutorial/?lang=EN): hai người nhận cùng lượng coin khi một người nhặt; không phải chung ví chi tiêu.
- O1: [Thông báo chính thức Shared Dream](https://x.com/ZZZ_EN/status/1883738031372611623), 2025-01-27. URL xác định được, nhưng web không trả body/ảnh trong lượt này. Không đánh dấu đã đọc toàn bộ manual.
- O2: [Patch notes 1.4](https://www.hoyolab.com/article/35654082): autosave/resume ở rest point. Trang mở trực tiếp chỉ hiện Loading; đoạn gameplay được search index trả lại.
- S1: [Game8 guide](https://game8.co/games/Zenless-Zone-Zero/archives/475381), cập nhật ghi 2025-01-28: difficulty/mode, thao tác reroll, merge. Nội dung search đọc được; open bị 402. Không dùng bảng passive lẫn phiên bản để sao chép số liệu.
- S2: [Companions](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Companions): merge và trường hợp riêng; dùng bổ trợ cho V1. Open bị chặn, đọc qua search.
- S3: [Relics](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Relics): nhiều hướng trigger/projectile và trade-off; đọc qua search, không sao chép toàn bộ catalog.
- S4: [Endless trải nghiệm 1.5](https://www.hoyolab.com/article/36751336): nguồn người chơi, không phải spec chính thức về điều kiện hay giới hạn Endless.

## 3. Các hiểu nhầm cần sửa trước khi thêm content

1. **Không thể lấy footage 1.2 làm spec 1.5.** Passive, companion đặc biệt, pool shop và các hướng build đã khác. Số liệu trong GDD gốc chưa phải dữ liệu tham chiếu đã xác minh.
2. **Altar không chỉ là shop trứng.** V0 135s có giá bằng trứng và HP trên cùng ba bệ; V1 480s có altar, giá HP và reroll bằng vàng. T1/T2 xác nhận vai trò trứng và công thức. `SpecialShopEvery = 2` hiện tại là giả định, không phải luật reference.
3. **Merge không luôn chỉ nâng hệ số damage.** V1 600s cho thấy companion có phần thưởng/luật riêng khi merge; S2 hỗ trợ các ngoại lệ. Luật ba bản cùng loại/cấp chủ dự án đã chốt vẫn giữ, nhưng cần event và recipe data riêng.
4. **Build không chỉ là cộng các buff dương.** V1/V2 có item nhiều chỉ số, điều kiện, lợi/hại và effect theo sự kiện; struct một `Effect/Value` không đủ.
5. **Main menu có, nhưng bản hiện tại mới là prototype.** Không gọi phần đã tồn tại là hoàn toàn thiếu; cần làm đủ flow, thông tin, unlock, input và phản hồi.
6. **Một arena giới hạn không có nghĩa camera phải chứa toàn arena.** Chủ dự án yêu cầu mới: map rộng hơn viewport, camera theo người chơi và clamp ở rìa. Điều này thay thế quy tắc cũ “sân nằm trọn trong một màn hình”.
7. **Video Unity xuất 12 fps không dùng để kết luận game mượt/không mượt.** Cần bản chạy thật và capture 60 fps đúng thời gian; báo riêng frame-time gameplay và chất lượng animation.

## 4. Ma trận toàn bộ feature surface

Trạng thái dưới đây từ source hiện tại, không phải lời hứa “đã hoàn thiện”. ID nối với backlog ở `Reference_Parity_Backlog.md`.

### A. Frontend, tiến trình và trạng thái run

| ID | Reference / evidence | Source hiện tại / phần cần làm |
| --- | --- | --- |
| A01 | V1/V2: chọn nhân vật qua roster và mô tả luật chơi | `RunMenu` chỉ 3 nhân vật, chọn bằng mũi tên; cần grid, selection feedback, locked state và trang thông tin rõ |
| A02 | V0 3s: chọn difficulty; S1 hỗ trợ 5 difficulty | Chưa có difficulty trong snapshot/catalog/launch; cần difficulty data, lưu lựa chọn, record theo difficulty; không tự rút run 20 hall đã chốt |
| A03 | V1 5s và V2 5s: nhân vật thay đổi cả cách xây đội/đạn/shop | 3 Champion còn gần cùng khung; cần archetype thật, không chỉ đổi sprite hoặc một passive |
| A04 | V1/V2: unlock/progress hiển thị ở selection | Hiện unlock theo BestWave; thiếu challenge/milestone cụ thể và feedback unlock |
| A05 | O2: resume/abandon ở ranh giới rest | Atomic save đã có; thiếu màn xác nhận ghi đè/abandon, thông tin save và UX báo lỗi có lựa chọn |
| A06 | V1 945s, V2 1100s: results, record, đội và relic | Hiện chỉ text hall/kills/boss/time; thiếu breakdown đóng góp và build recap có icon |
| A07 | V1 HUD: score đang tăng; S1: high-score loop | Chưa có score, score multiplier, best score theo Champion/difficulty |
| A08 | S4: Endless là hệ riêng; điều kiện cụ thể còn U | Hiện bấm tiếp tục sau victory luôn được; cần chốt điều kiện, presentation và scaling riêng; không sao chép bug/lag của reference |
| A09 | O1/T5: Shared Dream hai người | Chưa có multiplayer; đưa vào nhánh mở rộng riêng, không tự dựng mạng khi chưa chốt phạm vi/hosting |
| A10 | D: standalone Android/iOS/Steam, free | Không copy city launcher, event currency, account reward/gacha của ZZZ; device/store QA vẫn chưa thực hiện |

### B. Details, HUD và interaction

| ID | Reference / evidence | Source hiện tại / phần cần làm |
| --- | --- | --- |
| B01 | V2 480/495s: Details có Stats / Companions / Relics | Chưa có inventory/Details trong run; codex ngoài menu không thay được menu build đang có |
| B02 | V2 480s: stat kèm giải thích, scroll | Cần tổng stat đúng từ nguồn build, scope Champion/brigade, base/bonus và tooltip; không chỉ hiển thị số bonus |
| B03 | V1 945s, S2: companion khác cấp, vai trò, bond | Cần danh sách đầy đủ không giới hạn slot, icon, vũ khí đã roll, tier, passive, bond, điều kiện merge |
| B04 | V2 495s: relic đã sở hữu có số stack và effect | Cần icon/stack/mô tả, lợi/hại, trigger và phạm vi tác dụng của từng món |
| B05 | V1 rest: bond icon và tiến độ threshold | HUD hiện text tóm tắt; thiếu icon, trạng thái active và giải thích hiệu ứng khi xem build |
| B06 | V1 rest: tooltip theo vị trí người chơi | Đã có proximity và Space/USE; thiếu rarity/scope/so sánh stat, feedback thiếu tài nguyên và thông tin fusion tại chỗ |
| B07 | V1/V2: context input hints; D: mobile một tay | Cần focus navigation, scroll/touch, safe area, đủ cỡ chữ, close/back nhất quán; chưa có controller hỗ trợ thật |
| B08 | V1/V2: có Details trong combat; pause timing còn U | Cần overlay không làm lệch wave timer, đóng không gỡ focus pause, không chạy combat phía sau nếu chọn pause; phải test lifecycle |
| B09 | V0 215s, V1 945s: transition không chỉ đổi text | Thiếu intro wave/boss, rest arrival, victory/defeat sequencing và input lock ngắn trong transition |

### C. Combat và hướng build

| ID | Reference / evidence | Source hiện tại / phần cần làm |
| --- | --- | --- |
| C01 | V1/V2: auto-combat với đội đi quanh; D chỉ movement | Có core movement/auto attack/leash; vẫn phải cải thiện target/facing và readability khi đông |
| C02 | V2 5–30s: stationary mortar và luật đạn riêng | Hiện tất cả projectile bay cùng khung; thiếu đặt bom, fuse, nổ vùng, cap/exception theo archetype |
| C03 | V1/V2/S3: projectile count, bounce, division, pierce, size/range | Có multi-shot; chưa có bounce/division/pierce. Cần swept collision và hit-history tránh lặp hit vô hạn |
| C04 | V2 480s: collision, melee, ranged, crit, range khác nhau | Hiện damage generic, 3 element; cần phân biệt damage scope, flat/percent scaling, collision build, range thực và tooltip đúng |
| C05 | V1/V2/S3: item có nhiều lợi/hại | `ItemDefinition` một Effect/Value; cần modifier list, rarity, category, scope, trigger, stack policy |
| C06 | V1 300/480s, V2 120/490s: chest/coin/crit/wave-start trigger | Chỉ vài `PassiveKind`; thiếu event-driven proc registry, điều kiện/cooldown và hiệu ứng theo trigger |
| C07 | V2 Stats: armor/recovery; S3: dodge/conditional survival | Có armor và healing đơn giản; thiếu dodge/hearts/heal event, negative stat semantics và cap có giải thích |
| C08 | D: không one-hit do base damage; vẫn nhận nhiều hit hợp lệ | Hiện cap theo nguồn; cần regression source attribution và cap theo difficulty/build, không thêm hurt invulnerability toàn cục |
| C09 | V1 600s / S2: merge và special companion ngoại lệ | Có 3-copy merge/tier/bond count; thiếu merge event/reward data, preview và animation merge; không giả định toàn bộ reference cùng inheritance policy |
| C10 | D: immortal allies, interceptor chỉ nhờ item | Core đã có; cần pickup/tooltip/animation bật interceptor, test projectile behavior khi đồ thay đổi sau rest |
| C11 | D: manual knockback ở reference không đưa vào game | Footage có nút knockback; không thêm nút combat hoặc lén biến Space thành skill |

### D. Kinh tế, shop và altar

| ID | Reference / evidence | Source hiện tại / phần cần làm |
| --- | --- | --- |
| D01 | V1/V2: ba bệ vật lý, merchant, sign, exit glyph | Cơ chế gần đúng; art dùng companion phóng lớn và ellipse tạm; thiếu merchant/stand/exit artwork và animation riêng |
| D02 | V1/V2: rarity, giá và pool theo run/build | Hiện luôn 1 companion + 2 item, uniform RNG; thiếu weighted rarity/pool eligibility và các nguồn discount |
| D03 | V2 470–490s / S1: reroll tăng giá; V1 có modifier discount | Hiện phí cố định 4; cần counter trong save, bảng giá theo hall/lượt/modifier và feedback sign; công thức số cụ thể còn U |
| D04 | V1 480s + T1/T2: altar chance theo eggs, HP hoặc eggs | Hiện mở cố định mỗi 2 hall, chỉ eggs; đây là gap luật chứ không chỉ thiếu giao diện |
| D05 | V1/V2: quyết định build và món conditional | SpecialOffers hiện dùng cùng pool item thường; thiếu altar-exclusive effects, trade-off và eligibility |
| D06 | T2/S2: công thức đặc biệt theo material companion | Có 2 recipe tự đặt; cần recipe rules/matching/tier/material preview, bảo vệ rollback và art fusion |
| D07 | D: income/interest nhưng không upkeep, sell, lock | Có interest/income; không đưa upkeep của T4 trở lại. Công thức lãi reference chưa xác minh; cần UI dự báo và economic playtest |
| D08 | V1 480s: HP trade-off; exact current/max cost còn U | Cần currency Health riêng, chỉ rõ giá tác động HP hiện tại hay HP tối đa; không tự chọn công thức rồi gọi là reference |

### E. Loot, enemy, wave và arena

| ID | Reference / evidence | Source hiện tại / phần cần làm |
| --- | --- | --- |
| E01 | V1 30.05–30.40s: đồng xu bung ra và còn trên sân | `PickupState` tĩnh; chưa có scatter/landing/spin/glint và shadow |
| E02 | V1/V2: đi thu loot; S3 có pickup-trigger build | Pickup hiện tăng vàng và xóa ngay; cần hút/collect trail, wallet pulse, phản hồi theo loại, không credit hai lần |
| E03 | T1/S1/S3: trứng, rương, Treasure Detection | Chưa có egg drop entity, chest entity hay chest quality. Trứng hiện phát sau mỗi hall — không phải hệ drop đã đối chiếu |
| E04 | V1/V2/S3: hearts và chest/loot healing | Chưa có heart pickup và loot reward presentation; cần drop eligibility và full-health behavior |
| E05 | V2 480s, V1 120s: Treasure Detection và Luck | Thiếu hai stat, shop weighting và drop weighting; không chỉ thêm hai label không có tác dụng |
| E06 | V1 15/240/720/930s: silhouettes/behavior tăng đa dạng | Có 6 enemy definitions nhưng dùng nhiều body đồng minh; thiếu enemy family art, elite đặc trưng, pattern/role thật |
| E07 | V1 360/930s: boss lớn, attack khác nhau | Có 5 boss đổi body/pattern; thiếu intro, phase change, riêng art/death và tuning pattern. Lịch boss/formula scale cần đối chiếu thêm |
| E08 | V1 đầu/giữa/cuối: mật độ và thời lượng wave khác | Wave hiện 84s cố định; cần wave table/curves, tempo, spawn composition và cao trào, giữ run length chủ dự án yêu cầu |
| E09 | V1 15/30/240s: center follow, người chơi lệch gần wall; D yêu cầu camera nhỏ hơn map | `ArenaCamera` chỉ letterbox/aspect, camera không follow; map hiện fit toàn sân. Cần world size khác view size, clamp và smooth follow |
| E10 | V1 floor: flat shapes, debris thưa, edge riêng | Một PNG vừa nền/vừa wall hiện bị stretch; cần center floor/tile/debris và boundary lớp riêng cho map rộng |

### F. Animation, VFX, âm thanh và độ mượt

| ID | Reference / evidence | Source hiện tại / phần cần làm |
| --- | --- | --- |
| F01 | V1 30.05–30.40s: flash/fragment/death, loot bật ra | Unit chết bị bỏ khỏi Render ngay; puff có nhưng thiếu body death sequence, silhouette fragments và hand/weapon cleanup |
| F02 | V0 215s: player collapse và defeat banner | Champion biến mất ngay khi Alive=false; thiếu death presentation/timing trước results. Exact 1.5 defeat chưa có footage đủ |
| F03 | V1/V2: hand/weapon gắn sát; attack mang tính cách | `UnitVisual` có bob/windup nhưng angle nhảy ở impact và khi đổi hướng; cần pose continuity, easing curve và VFX bám phase |
| F04 | V1 combat / V2 mortar: hit cues khác nổ, melee, magic | Có slash/ring/star; hình/curve dùng chung nhiều, thiếu impact/muzzle/trail theo từng weapon family |
| F05 | V1 945s/rest: purchase/merge/build feedback | Chưa có purchase chime/receipt animation, merge burst, tier reveal hoặc relic arrival riêng |
| F06 | V1 combat: màu số damage/crit và silhouette nguy hiểm | Có crit/white flash; cần layering, number density budget, danger priority và screen-space legibility không giảm damage thực |
| F07 | Source/preview audit: 60Hz sim nhưng preview 12fps | Cần render-time pose/VFX interpolation, actual 60fps capture và frame-time profiling; không dùng fake slow-mo/hitstop làm simulation mất thời gian |
| F08 | Music/SFX reference: **U, chưa audition được** | Có nhạc synth study, chưa phải soundtrack; cần audition, cue inventory/menu/death/loot/shop/boss, mixing/ducking và volume buses |
| F09 | D: cực cao trên mobile và PC | Cần VFX pooling + allocation budget, stress ở đội đông/Endless, GPU/CPU/device tests; existing simulation checks không nghiệm thu cảm giác |

## 5. Sơ đồ trigger cần có để build không còn nông

Đây là kiến trúc đề xuất cho Arcane Brigade, không phải mô tả code nội bộ của ZZZ.

| Trigger | Mục tiêu |
| --- | --- |
| RunStarted / WaveStarted / WaveEnded | Buff reset theo wave, income, difficulty, stage modifiers |
| AttackStarted / ProjectileSpawned / HitResolved | Modifier theo scope, proc cooldown, split/bounce/pierce |
| UnitKilled | Đóng góp damage, coin/egg/heart/chest drops, kill-count passives |
| LootCollected / ChestOpened | Healing, growth trong run, coin-trigger damage, Treasure Detection build |
| OfferPurchased / ShopRefreshed | Discount, category-specific effects, reroll price, purchase presentation |
| CompanionMerged / RecipeCrafted | Reward riêng, roster/bond refresh, animation merge/fusion |
| PlayerDamaged / PlayerHealed / PlayerDied | Conditional build, clear cause-of-death, death/results sequencing |

Proc phải tách gameplay/presentation; chống recursion và double-credit; RNG proc/drop/shop/weapon phải độc lập và khôi phục từ save. Chỉ số đang tăng trong run không được vô tình ghi thành meta stat vĩnh viễn.

## 6. Các khác biệt giữ nguyên theo chủ dự án

- Chỉ movement trong combat, không manual knockback/skill/dash/aim.
- Không phí nuôi, không sell/lock, không cap số đồng minh.
- Đồng minh bất tử, default không chặn đạn, đi quanh bán kính; item mở rộng/chặn đạn.
- Ba copy cùng loại/cấp ghép; không trùng unit + weapon; bond đếm body, không nhân sao.
- Không import candy tăng stat vĩnh viễn của reference; meta unlock/codex/record/cosmetic giữ nguyên.
- Landscape, mobile một tay; bounded map lớn hơn camera theo yêu cầu mới.
- Run length và Endless của Arcane Brigade không mặc định bằng 3/6/10 wave của reference.
- Asset/nhạc tạo riêng; không đưa artwork, logo, nhạc hoặc clip ZZZ vào shipping Assets.

## 7. Chưa được phép nói “research mọi thứ đã xong”

Các lỗ evidence còn lại: toàn roster và relic theo đúng build 1.5; companion weapon eligibility; merge inheritance/exceptions từng loại; thuật toán shop rarity/pricing/interest; HP hiện tại hay max HP khi trả giá; xác suất altar theo eggs và lúc tiêu trứng; lịch/drop rate/loot table từng enemy; exact boss schedule; điều kiện Endless; input/pause của Details trong mọi mode; 1.5 player-death sequence; full Shared Dream manual/flow; audio audition và timings soundtrack.

Cần dữ liệu bổ sung có chủ đích, không tìm kiếm chung rồi tự lấp bằng số bịa. Nguồn wiki/data có thể đã cập nhật sau 1.5 hoặc bị chặn; không gộp nó vào footage 1.5 như một spec đồng nhất. Bản audit đã rà **57 hạng mục feature surface**, nhưng chưa là reverse-engineering đầy đủ của game thương mại.
