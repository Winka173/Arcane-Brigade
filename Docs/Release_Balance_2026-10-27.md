# Release balance — khởi tạo audit 27/10/2026

Đây là vòng khởi tạo đo lường và sửa lỗi chặn mô phỏng trên source hiện tại. **Chưa đạt gate cân bằng 30%, chưa nghiệm thu release.** Không lấy số đo hoặc báo cáo checkpoint cũ làm bằng chứng cho build hiện tại.

Cập nhật theo yêu cầu đo lại năm mức: [Release_Five_Difficulties_2026-10-27.md](Release_Five_Difficulties_2026-10-27.md) là báo cáo mới hơn, có 210 campaign, kiểm chứng migration và nguồn dữ liệu riêng. Phần dưới giữ snapshot tám mức ban đầu.

**Ranh giới phiên bản:** lượt đo bắt đầu từ commit `c68e5999` cộng bản sửa traversal/fixture của audit này, khi game có tám độ khó. Trong lúc đang chạy, workspace có thay đổi song song hạ còn năm độ khó, đổi unlock/migration và đóng tuyển quân mới ở cuối trận. Các thay đổi song song được giữ nguyên, không đưa vào commit audit này. Số liệu tám độ khó và campaign index 1/3 bên dưới thuộc phiên bản đo có hash trong `Assembly.txt`; **không phải chứng nhận cho cấu hình năm độ khó đang sửa trong workspace**. Sau khi tích hợp nhánh đó cần chạy lại theo tên/mapping độ khó mới và quy tắc tuyển quân mới.

## Phạm vi source

Đã lập inventory source/tài liệu/công cụ và rà các luồng liên quan trực tiếp đến cân bằng. Inventory có hash và số dòng, không đồng nghĩa đã kiểm chứng từng dòng hoặc duyệt toàn bộ art/audio nhị phân.

| Hệ thống | Nguồn chính |
| --- | --- |
| Catalog và dữ liệu thật | `Resources/RunCatalog.asset`, `Data/Run/`, `Runtime/Data/` |
| Biên dịch stat, shop, thu nhập, merge | `RunSession`, `RunModifierRules`, `RunTransaction`, `CompanionDeployment`, `RecruitmentRules` |
| Damage và nguồn đóng góp | `CombatSimulation`, `ContributionLedger`, `PlayerBuildBalance`, `FriendlyWeaponBalance` |
| Hero, biến hình, approach | `HeroKits`, `HeroIdentity*`, `HeroAwakening`, `HeroGauge`, `October*`, `AwakenTuning` |
| Vũ khí và companion | `ArsenalCombat`, `ExpandedCombat`, `CarnivalCombat`, `CompanionSkills*`, `CompanionTierScaling` |
| Nhịp địch, độ khó, Endless | `JourneyBalance`, `CarnivalBalance`, `WaveCadence`, `SwarmPacing`, `EnemyVariety` |
| Boss, boss đặc biệt, trận cuối | `BossEncounterRules`, `BossSignatureCombat`, `OutsideStage*`, `LastCustomerCombat`, `ActOneRun` |
| Event và meta | `SecretEventRun`, `RunVisitors`, `RareVisitors`, `MetaRun`, `LampTree` |

Đọc cùng các báo cáo lịch sử: `Hero_DPS_And_Reach_Audit.md`, `Build_Balance_Pass.md`, `Mode_Balance_Pass.md`, `Balance_Pacing_2026-10-22.md`. Các thông số và kỳ vọng của chúng có thể đã bị yêu cầu gameplay sau đó thay thế.

## Quy ước gate damage

Chủ dự án đã chốt **giữ vai trò, so sức mạnh tổng đội**. Gate dùng **DPS cao nhất / thấp nhất ≤ 1,30**, trong cùng bài đo, cùng wave và cùng mức đầu tư. Đây chặt hơn cách so từng hero với trung vị. Hero hỗ trợ được đánh giá theo tổng đội; hero solo vẫn không được gán đội giả. Damage riêng vẫn được lưu để phát hiện outlier, không thêm đòn thường cho hero vốn không tự đánh chỉ để làm đẹp bảng.

Fixture có 36 hero × 3 approach × wave 10/20 × 1/8 mục tiêu × solo/đội × đứng/di chuyển/áp lực = **2.592 mẫu** mỗi mức đầu tư, **5.184 mẫu** tổng. Seed 77138; đội chuẩn bốn companion 2 sao như audit trước; Lone Jelly có ba ký ức mẫu. Mỗi mẫu 60 giây, mục tiêu 1.200 HP được thay sau khi chết. Cấu hình đầu tư cộng giống nhau team damage +50%, crit +35 điểm phần trăm, attack speed +25%; đây là fixture, không phải item mua tự nhiên.

Sửa fixture để bù HP tối thiểu 75% trước mỗi bước ở mọi chế độ, vì hero tự rút máu có thể chết ngay trong bài không có địch đánh. Bài contact vẫn nhận hit thật. **Không dùng fixture này để kết luận sống sót hoặc win rate.** Giá trị theo phần trăm HP có thể khác khi đánh quái ít máu.

Baseline đầy đủ trước sửa chung vòng duyệt:

| Mục tiêu | Wave | Cơ bản: max/min | Có đầu tư: max/min |
| --- | --- | ---: | ---: |
| 1 | 10 | 3,6734 | 2,4520 |
| 1 | 20 | 3,7173 | 2,5037 |
| 8 | 10 | 8,9818 | 3,8273 |
| 8 | 20 | 9,7141 | 3,2811 |

Cả tám nhóm đều chưa đạt 1,30. Ở cấu hình cơ bản, Glass Dice cao nhất và Tin Virtuoso thấp nhất trong cả bốn nhóm. Khi có đầu tư, outlier thay đổi: Soap Skater thấp nhất ở đơn mục tiêu; Lone Jelly hoặc Tin Virtuoso thấp nhất trong đám đông; Velvet Regent thường cao nhất. Vì vậy chỉ nhân một hệ số damage cho mọi hero sẽ không giải quyết được cả hai mức đầu tư và cả hai loại mục tiêu.

`teamDps` đã bao gồm damage hero; **không cộng thêm** `teamBonusDps`. Cột lịch sử `allyDamage` trong campaign là toàn bộ damage ngoài ID hero, gồm `ARENA`, không phải damage thuần companion. Không so tổng damage campaign có số wave/thời gian sống khác nhau để kết luận chênh DPS 30%.

## Sửa lỗi chặn release audit

Campaign tái hiện `Collection was modified` khi quái Splitter chết và sinh hai con ngay trong vòng duyệt mục tiêu. Đã gặp thực tế ở sát thương lan của địch, Fizz và đòn vùng mở rộng/Echo Blade. Đây là lỗi runtime có thể làm gián đoạn trận.

Thêm traversal chốt số phần tử ban đầu, không cấp phát mảng; áp dụng cho các vòng `foreach` trên danh sách unit nội bộ của simulation. Việc xóa unit vẫn ở bước cleanup. Quái con không bị đòn vừa giết mẹ đánh tiếp trong cùng traversal; chúng tham gia các lượt xử lý sau. Không đổi damage asset, giá, thu nhập hoặc bảng độ khó trong vòng này.

`ReleaseCombatChecks` kiểm tra hai Splitter cùng chết và sinh bốn con còn nguyên máu ở năm đường: hostile collateral, friendly field, Arsenal Fizz, expanded melee và Echo Blade. Đồng thời sửa bot nhận quà prism đã kiếm được trước khi mua/đi tiếp; trước đó audit ghi `BLOCKED_SHOP` sai về khả năng hoàn thành campaign.

## Campaign, kinh tế, boss và event

Runner dùng `RunSession`/`CombatSimulation` thật: tự kiếm tiền, mua theo giá thật, reroll, quota, merge, story party và quà kiếm được. Ma trận mặc định là toàn bộ 36 hero × difficulty index 1/3 × seed 271026 × Hybrid, tổng 72 campaign. Có cờ chọn hero, seed, difficulty và Buff/Recruit/Hybrid để mở rộng. Không bơm HP/vàng/item, không bật Lamp Tree. Thăm dò compile bao phủ 36 hero × 8 độ khó × 15 mốc wave 1–50 = 4.320 cấu hình.

Campaign bỏ qua giao dịch event tùy chọn. Vì vậy kiểm tra transaction event có qua cũng **chưa chứng minh cân bằng lợi ích/rủi ro event**. Độ khó 6/7 cần meta để mở trong game, chỉ được compile probe trong lượt này; không dùng run không meta để kết luận win rate endgame. Timeout 90 giây ở wave thường/240 giây ở wave boss là chưa phân giải, không gán thành thắng hoặc thua. Một seed và một bot không đủ ước lượng tỷ lệ thắng người chơi hoặc đảm bảo độ khó thắng giảm đều.

Các kiểm tra lịch sử ghi nhận sáu gate cần triage: BuildBalance (drawback Patchwork), HeroIdentity (read-model row), HeroAwakening (hết biến hình), Visitors (copy/icon Waker), RareVisitors (upgrade/merge), Modes (hệ số Peaceful và special currency). Recruitment và SecretEvents qua. Một số kỳ vọng rõ ràng đã cũ, ví dụ thời lượng biến hình được tăng ×1,55 và special shop có ramp; vẫn giữ FAIL nguyên trạng để không che phần chưa điều tra. Không sửa gameplay về thông số cũ chỉ để test xanh.

## Tái tạo và đọc bằng chứng

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Balance/Run-ReleaseBalance.ps1 -Label MyUniquePass
```

Dùng label mới khi source/assembly thay đổi. Runner lưu hash hai assembly và từ chối nối dữ liệu khác phiên bản. `Method.txt` ghi ma trận thực được chọn. `Checks.tsv` ghi PASS/FAIL riêng; hoàn thành runner không đồng nghĩa các gate đều qua. `summarize_release.py` kiểm tra trùng ID, đủ mẫu DPS, số hữu hạn, lịch boss và cấu hình compile; không tính wave dang dở của campaign chưa hoàn tất.

Không dùng các folder `Baseline20261027`, `Release20261027`, `ReleaseStable20261027`, `ReleaseSweep20261027`, hoặc DPS `ReleaseMeasured20261027` làm kết quả cuối: đó là lượt chẩn đoán bị dừng. Các nhánh lỗi và stack trace ngắn nằm trong `PreviewExports/ReleaseBalance/DiagnosticHistory.txt`.

## Kết quả campaign đã hoàn tất

`PreviewExports/ReleaseBalance/ReleaseFinal20261027/`: **72 campaign, 1.017 wave**, không còn `BLOCKED_SHOP` hoặc exception. Đây là baseline sau sửa lỗi runtime, **chưa có tuning số học mới**.

| Difficulty index | Lượt | Thắng campaign | Thua | Timeout chưa phân giải | Wave cuối trung bình |
| --- | ---: | ---: | ---: | ---: | ---: |
| 1 | 36 | 16 | 16 | 4 | 16,08 |
| 3 | 36 | 10 | 26 | 0 | 12,17 |

Không đồng nhất tỷ lệ này với win rate người chơi. Cả hai difficulty dùng cùng seed và cùng chính sách Hybrid, nhưng shop/loot có thể rẽ nhánh khác nhau khi combat khác. Tin Virtuoso thua cả hai lượt ở wave 8. Root Oracle thắng cả hai; cuối lượt difficulty 1 có **332 stack item và 287.973 vàng**, difficulty 3 có 135 stack và 10.425 vàng: cần điều tra vòng tăng sức mạnh/thu nhập của riêng build này.

| Wave shop | Mẫu thắng tới shop | Ví trước mua / giá offer TB: trung vị | Cùng chỉ số: trung bình | Số món mua TB |
| --- | ---: | ---: | ---: | ---: |
| 1–3 | 216 | 3,53 | 3,84 | 3,34 |
| 4–7 | 276 | 5,51 | 6,12 | 5,78 |
| 8–11 | 209 | 3,12 | 3,82 | 4,18 |
| 12–15 | 136 | 3,10 | 4,81 | 4,56 |
| 16–19 | 108 | 2,73 | 8,04 | 5,08 |

Trung bình cuối trận bị kéo lên bởi outlier, trong khi trung vị sức mua giảm. **Chưa có cơ sở nerf thu nhập hoặc tăng giá toàn game.** Các shop cuối chỉ thuộc nhóm còn sống tới đó; không so trực tiếp như cùng một quần thể.

Boss tại wave 4/8/12/16/20 lần lượt bị hạ **70/72, 55/66, 38/51, 27/31, 26/27** lượt đã tới trận đó. Có ba timeout ở wave 16 và một ở wave 20. Đây là đếm boss bị hạ, có thể khác kết quả thắng wave nếu hero chết sau đó. Cột thời gian hiện tại là **thời gian cả encounter**, không phải thời điểm boss mất HP cuối cùng: trận có thể đợi hết thời lượng dù boss đã chết. Chưa dùng các số này để điều chỉnh TTK; vòng đo tiếp cần xuất `BossFellAt` riêng.

Compile probe đủ **4.320 cấu hình**, số hữu hạn/hợp lệ và thang HP/damage tăng nghiêm ngặt theo cả tám độ khó. Điều này không thay thế sáu gate lịch sử còn FAIL, độ khó challenge, nội dung event hoặc frame time trên player.

Hồi quy mới: **5/5** đường chết-tách đôi qua. Recruitment: **433** kiểm tra qua; SecretEvents qua transaction/save checks. DPS/reach baseline hoàn tất **1.080** kiểm tra tầm/shot pattern/crit encore. Giữ riêng kết quả của từng bộ, không công bố tất cả hồi quy production đã xanh.

## Gate tiếp theo

1. Triage sáu nhóm kiểm tra đang FAIL; giữ phân biệt lỗi test và lỗi gameplay.
2. Tune outlier damage theo tổng đội, kiểm lại cả đơn/đám đông và hai mức đầu tư; giữ một seed khác để đối chứng.
3. Đo Buff/Recruit/Hybrid với cùng seed, ngân sách thật; kiểm wallet/mean offer, lượng mua/refresh, lãi và thời điểm đạt sao; điều tra build có tiền/item tăng đột biến.
4. Tune địch/độ khó/boss theo các mốc 4/8/12/16/20, thời gian trận, damage nhận và tử vong; tách Endless và meta endgame.
5. Đo từng lựa chọn của tám secret room và visitor theo chi phí/cơ hội/rủi ro, gồm chiến đấu phụ, boss Keeper/Bedside và reload/transaction.
6. Playtest cảm giác và độ mượt ở mật độ cuối trận trên Windows player trước khi chốt release.
