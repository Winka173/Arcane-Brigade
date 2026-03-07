# Cân bằng hero, companion và hướng đầu tư — 17–18/10/2026

## Mục tiêu và đối chiếu nguồn

Yêu cầu: mua buff phải là hướng chơi đủ sức cạnh tranh với tuyển quân; kiểm tra playable, companion, các hướng build, boss và độ khó. Đánh giá bằng trận thật trong simulation và shop sản xuất, phân biệt với loadout được cấp ngân sách trong phòng thử.

**Đã hoàn tất lượt cân bằng và đo ngày 18/10/2026.** Ưu tiên buff với đội sáu người đạt **47/98** campaign, tuyển quân đạt **55/98** trong cùng bộ hero/độ khó/seed; trung vị wave cuối đều 20. Hướng buff đã cạnh tranh hơn, nhưng chưa ngang mọi hero/độ khó. Đây là kết quả bot chạy Unity headless, chưa phải playtest của người chơi hay một bản Windows mới.

Đã xem lại nguồn Bizarre Brigade:

- [Thông báo chính thức 1.5](https://zenless.hoyoverse.com/m/en-us/news/129572) xác nhận đợt mở rộng và Shared Dream; không cung cấp công thức damage nội bộ để chép sang Arcane.
- [Relics, dữ liệu được công cụ tìm kiếm lập chỉ mục](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Relics): có hướng máu chuyển thành damage, va chạm/giáp, đánh đổi giữa melee/ranged và tương tác hồi phục/né tránh. Đây là cơ sở chọn archetype, không phải xác minh mọi hệ số của phiên bản hiện hành.
- [Companions, dữ liệu được lập chỉ mục](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Companions): cấp sao, tag, buff và điều kiện kích hoạt làm giá trị một bạn vượt ngoài DPS. Do đó báo cáo giữ cột support/heal/coin bên cạnh damage.
- [Dream Seekers](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Dream_Seekers) và hai footage được ghi trong [audit 1.5+](Bizarre_Brigade_15_Reference_Audit.md) nhấn mạnh khác biệt theo nhân vật. Không giả định mọi hero nên chơi cùng một hướng.

Trang Fandom đầy đủ không mở được trong lượt này; chỉ dùng nội dung search trả về, đối chiếu tài liệu nghiên cứu cũ và code địa phương. Không tuyên bố đã trích xuất hoặc chơi thử trực tiếp toàn bộ Bizarre Brigade. Giữ luật Arcane đã chốt: companion bất tử, không phí duy trì, không trần cơ thể, quota mua hiện tại và chiến dịch 20 wave.

## Phương pháp

Bot mới tiếp cận theo tầm vũ khí và bán kính mục tiêu, tránh đạn dự đoán gần và telegraph. Bot cũ tránh boss ở khoảng cách 3.2, làm việc đo hero cận chiến dễ lệch. Cùng seed và cùng bot giữa các hướng; mua khác nhau dẫn đến nhánh RNG shop khác nhau, nên đây không phải một chuỗi shop cố định được phát lại.

- Mẫu đối chiếu trước/sau: sáu hero đại diện × ba hướng Buff/Recruit/Hybrid × độ khó 0/5 × một seed = 36 campaign mỗi bản.
- Ma trận campaign cuối: cả 31 hero ở approach 0, ba hướng mua và độ khó 0/5; sáu hero đại diện mở rộng cả sáu độ khó và seed thứ hai ở 0/5. Tổng 294 campaign. Mẫu khám phá trước khi bổ sung shop nền tảng đã dừng sau 252 campaign, giữ riêng ở `PreFoundation`, không nhận là ma trận hoàn tất.
- Đo bổ sung FocusedBuff: cùng 98 trường hợp hero/độ khó/seed của mỗi hướng chính, ưu tiên item và đội nhỏ. Chính sách giữ tối đa bốn body tự mua để còn chỗ tích bản trùng ghép sao; không xóa quà phát sinh và vẫn cho ghép khi đủ người. Đây là hạn chế của bot để thử hướng chơi, không phải trần roster trong game.
- Đo riêng FocusedBuff6: thêm 98 campaign với cùng seed, cách di chuyển và ưu tiên item của FocusedBuff, tăng mục tiêu đội từ bốn lên sáu người. Giữ nguyên kết quả đội bốn, không lấy kết quả tốt hơn giữa hai chính sách cho từng hero. Quà phát sinh vẫn được giữ. Tổng ma trận cuối là **598 campaign**: 294 chính + 108 archetype + 98 đội bốn + 98 đội sáu, chưa cộng các lượt hiệu chỉnh/chạy lại.
- Cả 93 approach: 31 hero × ba approach × wave 5/10/15 × hai seed, so với các trận tắt identity tương ứng; 558 trận bật và 186 trận đối chứng. Dùng loadout phòng thử gồm bốn bạn hợp theme và 2/4/6 relic theo wave; riêng LoneJelly chỉ có ký ức hấp thụ hợp lệ, không bơm bạn vào roster. Kết quả ở `PreviewExports/HeroMechanicBalance/BuildBalanceFinal`.
- Sáu hướng chuyên biệt bổ sung: melee, ranged, critical, collision, sustain, economy; sáu hero ở độ khó 0/2/5 = 108 campaign.
- Companion: 66 loài × ba cấp sao × wave 5/15 = 396 trận. Hero không gây damage, không có story party; có đo modifier hỗ trợ. Đây là fixture được cấp đội hình, không phải companion nhận được qua shop.
- Vũ khí: toàn bộ 98 weapon definition trong catalog × ba cấp sao × wave 5/15 = 588 trận, cùng một companion mang vũ khí. Bao gồm thế hệ cũ và arsenal mới; không tương đương 98 vũ khí mới.
- Boss: 504 fixture ngân sách shop cố định, gồm mười thân boss thường, boss cốt truyện, Keeper và Bedside. Tách khỏi probe Keeper/Bedside dùng bản sao đội hình thực kiếm được trong campaign.

Campaign dùng tiền/loot/giá/quota/merge thật, có starter và story companions thật, không bơm HP hay ép thắng. Buff không mua companion; Recruit ưu tiên companion rồi dùng tiền dư mua item có ích; Hybrid mua cả hai. Với hero hấp thụ bạn, mua companion vẫn tuân theo hấp thụ của production, không gọi đó là đội đông. Ngân sách phòng thử được ghi riêng, không giả thành thu nhập tự nhiên.

Simulation bước 0.05s. Wave thường dừng kiểm tra ở 65s, boss ở 180s; còn Combat được ghi TIMEOUT, không tính là thắng. Không dùng tỷ lệ thắng bot như tỷ lệ thắng người chơi. Không có ma trận hữu hạn nào chứng minh mọi tổ hợp 170 item với mọi vũ khí/đội hình đều cân bằng.

## Quan sát nền

36 campaign nền: Buff thắng 2/12; Recruit 4/12; Hybrid 5/12. 23 lượt thua và hai lượt chưa kết thúc trong thời hạn. Xem CSV và Summary để đối chiếu wave cuối theo hero và độ khó.

Ví dụ cùng seed 77138: Postmaster Buff ở độ khó 5 thua wave 5; Recruit thắng wave 20. LoneJelly thua ngay wave 1 ở độ khó 5 trong cả ba chính sách. Các fixture companion/vũ khí đều có damage ghi nhận; chưa thấy mẫu damage bằng 0.

## Thay đổi cân bằng

Tăng sức tấn công nền của hero tại bước compile; tăng nhẹ hiệu quả phần damage/attack speed vượt ngưỡng mềm cho riêng hero. Companion giữ đường tăng hiện có. Chỉ số hiển thị lấy từ compiled unit, có giải thích hệ số hero. Không sửa file asset vũ khí, không nhân vào save stack, không đổi giá hoặc quota.

| Tham số | Trước | Sau |
| --- | --- | --- |
| Hệ số weapon damage của hero | 1 | 3; RootOracle/GlassDice 3.6; PatchworkHost 4.5; LoneJelly 6.5 |
| Phần damage bonus vượt +120% | Hiệu quả 30% | Hero 60%; companion vẫn 30% |
| Phần attack speed vượt +60% | Hiệu quả 12% | Hero 30%; companion vẫn 12% |
| Trần hệ số attack speed trước xử lý cooldown | 2.2 | Hero 4; companion 2.2; cooldown cuối vẫn tối thiểu 0.28s |
| LoneJelly | HP 145, armor +0.3, flat melee +4 | HP 225, armor +0.6, flat melee/ranged +4, hồi 3 HP/s trước correction/cap |
| Patchwork Host: tăng HP/số địch | +35% / +20% ngay từ đầu | +12% / +6% qua wave 8; tăng tuyến tính đến mức cũ ở wave 20 |

Theo công thức đang có, base weapon + flat damage đã làm mềm được nhân với damage multiplier, gồm hệ số hero mới, rồi áp correction và các hệ số riêng. Skill lấy damage từ weapon đã compile cũng hưởng thay đổi đó; không sửa riêng hệ số/cooldown/cách hoạt động của skill. Collision và mọi modifier khác không bị nhân đồng loạt. LoneJelly cần cả ranged lẫn melee vì loadout hiện dùng vũ khí tầm xa nhưng innate cũ chỉ buff cận chiến. Mô tả SoloMaw sửa lại đúng việc hấp thụ companion đã mua; không còn nói sai rằng shop chỉ có relic. Patchwork Host được giảm hình phạt mở màn sau khi cả sáu campaign mẫu đều thất bại; vẫn giữ đánh đổi đầy đủ ở cuối trận.

Năm wave đầu, ô item thứ hai của shop vàng chọn từ các common/uncommon hợp lệ có chỉ số chiến đấu dương phù hợp hero và không có modifier âm/tăng sức mạnh địch. Phần lọc pool/theme vẫn chạy trước; nếu không có ứng viên thì dùng pool cũ. Không ép HP vào Card Queen/Periwinky, hoặc armor vào Card Queen, vì các stat đó không đi qua công thức riêng của họ. Sau wave 5 dùng phân phối cũ. Slot companion, shop đặc biệt, giá và quota giữ luật hiện có.

Bot cho Card Queen bỏ qua giá trị HP/armor không áp dụng; Periwinky giữ một phần vàng để duy trì máu theo vàng. Có probe bổ sung thu nhặt coin phù hợp tầm đánh của Periwinky và probe collision chủ động chạm địch khi còn đủ máu. Những điều chỉnh bot được ghi riêng, không gọi là buff gameplay.

## Kết quả cuối

| Chính sách mua | Campaign | Thắng | Thua | Chưa kết thúc trong hạn | Trung vị wave cuối |
| --- | ---: | ---: | ---: | ---: | ---: |
| Ba hướng chính, gộp 31 hero | 294 | 140 | 148 | 6 | 19.5 |
| Sáu archetype | 108 | 67 | 39 | 2 | 20 |
| Ưu tiên buff, đội bốn | 98 | 41 | 54 | 3 | 17.5 |
| Ưu tiên buff, đội sáu | 98 | 47 | 47 | 4 | 20 |
| Tuyển quân, đối chiếu 98 cặp với buff | 98 | 55 | 41 | 2 | 20 |

Hai dòng ưu tiên buff là hai chính sách riêng; dòng tuyển quân đã nằm trong 294 campaign chính, không cộng lần nữa. Tất cả bảng dùng bản kiểm tra lại đã chỉ định, kể cả khi kết quả kém hơn lượt trước. Thay đổi cách bot chọn đồ/thu vàng được phân biệt với thay đổi gameplay.

| Độ khó (index trong code) | Số cặp | Buff đội sáu thắng | Tuyển quân thắng |
| --- | ---: | ---: | ---: |
| 0 | 37 | 25 | 27 |
| 1 | 6 | 3 | 4 |
| 2 | 6 | 4 | 3 |
| 3 | 6 | 4 | 4 |
| 4 | 6 | 3 | 4 |
| 5 | 37 | 8 | 13 |

Khoảng cách tổng là 8/98 lượt, khoảng 8.2 điểm phần trăm. Độ khó cao nhất còn nghiêng về tuyển quân; các mức giữa chỉ có sáu mẫu mỗi mức, chưa đủ kết luận xác suất thắng. Trung vị đội hình cuối là 6 so với 17 người. Trung vị chi vàng ròng cho item/tuyển quân là 45,279 / 500.5 ở hướng buff, so với 33,553 / 11,415.5 ở hướng tuyển quân. Đây là hai cách phân bổ tài nguyên khác nhau, không phải cùng tổng tiền kiếm được ở mọi run.

Ở 36 campaign cố định trước/sau: tổng clear tăng **11 → 21**; chỉ mua item **2/12 → 4/12**, tuyển quân **4/12 → 9/12**, hỗn hợp **5/12 → 8/12**. Đội bốn/sáu không có ma trận baseline tương ứng, nên không dùng chúng để công bố mức tăng trước/sau. Patchwork Host sau tuning cuối clear cả normal và độ khó 5 với Recruit/Hybrid; các mẫu chỉ mua item và FocusedBuff đội bốn của hero này vẫn thất bại. Không lấy lượt FocusedBuff cũ từng thắng để thay kết quả mới.

396 mẫu companion và 588 mẫu vũ khí cuối đều có direct damage lớn hơn 0. Các bảng vẫn giữ heal, coin và modifier hỗ trợ. 504 fixture boss là phép thử thiếu ngân sách tương đối; ngoài ra có **292 probe boss đặc biệt** từ run thực kiếm được: Bedside bị hạ 92/160, Keeper bị hạ 126/132 và rút lui 6/132. Không tính rút lui là hạ boss. Các probe gồm nhiều chiến lược/độ khó, không đại diện tỷ lệ thắng người chơi.

93 approach được đo lại đủ 558 lượt bật và 186 đối chứng. **28/93** còn nằm ngoài dải cảnh báo giá trị mechanic 0.6–1.6 lần trung vị. Đây là danh sách cần đọc cùng điều kiện/kỹ năng hỗ trợ, không phải 28 lỗi đã xác nhận; không tuyên bố mọi skill và tổ hợp item đã cân bằng hoàn hảo.

Kiểm tra hồi quy cuối: **2,636** build/shop/compile/save-load + **4,229** mode/difficulty + **125** boss relief + **17** shop flow + **820** recruitment = **7,827 PASS**. Script tổng hợp cũng xác nhận đủ ID, không trùng campaign, số hữu hạn và các runner đã hoàn tất. Giữ nerf boss đã có từ lượt trước (HP ×0.6, damage địch từ boss ×0.9); đợt này không sửa thêm bộ chiêu boss hay vũ khí.

## Dữ liệu và cách đọc

Dữ liệu ở `PreviewExports/BuildBalance/`; `Assembly.txt` ghi hash assembly thực được chạy. `Final` giữ ma trận chính; `FinalFocused` giữ hướng buff/đội nhỏ, collision chủ động và lần kiểm tra lại Periwinky/Card Queen/Patchwork Host. `HostFinal` đo riêng tám campaign của Patchwork Host sau khi tăng thêm sức đánh, rồi đo lại toàn bộ 93 approach và hồi quy. `CrewSixFinal` giữ 98 campaign đội sáu, cùng tuning gameplay cuối. Các file `Reviewed_Natural.csv`, `Reviewed_Archetypes.csv`, `Reviewed_Focused.csv` ghép thay thế theo đúng ID, có cột `source_pass`; không chọn kết quả tốt hơn giữa các lần. Bản chính chạy trước thay đổi riêng Patchwork Host, nên sáu dòng của hero này được thay bằng lượt cuối.

Các folder Baseline/Candidate1/Candidate2/SoloRepair/SoloRepair2/FoundationPilot/PreFoundation giữ lịch sử đo; không coi bản thử hoặc lượt chưa hoàn tất là kết quả cuối. `Tools/Balance/summarize_builds.py` tái tạo Summary, so sánh 36 trường hợp trước/sau và 98 cặp FocusedBuff/Recruit.

| Lượt hiệu chỉnh | Tham số khác biệt | Phạm vi / ý nghĩa |
| --- | --- | --- |
| Baseline | Hero power 1; damage slope .3, speed slope .12, speed cap 2.2; LoneJelly HP 145, chỉ flat melee +4, armor .3 | 36 campaign và bộ fixture; mốc trước thay đổi |
| Candidate1 | Hero power 1.8, Root 2.1, Dice 2.2, Lone 3.2; damage slope .45, speed slope .2, cap 2.2 | 36 campaign, bản thử đã bỏ |
| Candidate2 | Hero power 3, Root/Dice 3.6, Lone 5.5; damage slope .6, speed slope .3, cap 4; Lone vẫn stats cũ | 36 campaign, bản thử đã bỏ |
| SoloRepair | Lone power 5.5, HP 165, armor .3, hồi 1, thêm flat ranged +4 | Sáu campaign; gate chưa đạt |
| SoloRepair2 | Lone dùng thông số cuối trong bảng tuning | 12 campaign / hai seed; có clear solo buff ở normal |
| PreFoundation | Thông số cuối, chưa có ô item nền tảng | Dừng có ghi nhận sau 252 campaign để sửa stock đầu trận |
| FoundationPilot / Final | Thông số cuối và ô item nền tảng; Patchwork vẫn phạt cũ trong assembly này | 36 campaign gate, rồi ma trận chính |
| FinalFocused | Thêm ramp Patchwork và sửa bot collision/thu vàng/Card Queen; sửa fixture solo không có roster | Chỉ thay thế những mẫu đã chỉ định, không chạy lại để chọn seed thắng |
| HostFinal | Patchwork power 3 → 4.5 | Tám campaign cùng seed/độ khó/chính sách cũ; đo lại HMB và các kiểm tra hồi quy |
| CrewSixFinal | Không thay gameplay; bot FocusedBuff6 dùng mục tiêu sáu người | 98 campaign riêng, giữ nguyên dữ liệu đội bốn |

Tái tạo báo cáo cuối từ thư mục repo:

```powershell
python Tools/Balance/summarize_builds.py PreviewExports/BuildBalance/Final PreviewExports/BuildBalance/Baseline PreviewExports/BuildBalance/FinalFocused PreviewExports/BuildBalance/HostFinal PreviewExports/BuildBalance/CrewSixFinal --verify
```

Các cột chi tiêu là **chênh lệch ví vàng ròng**, không phải tổng giá niêm yết: vật phẩm/refresh có thể trả lại vàng nên có thể âm; không cộng tiền health/egg vào vàng. Fixture boss dùng 65 + 5×shop vàng mỗi shop, không có tiền từ combat; thường nghèo hơn run kiếm nhiều loot, nên không dùng tỷ lệ thắng fixture làm tỷ lệ thắng campaign. Keeper rút lui vẫn tách khỏi giết boss. Không ép tỷ lệ thắng tăng đều trên một seed khi shop, cách mua và enemy density làm chuỗi RNG thay đổi; hệ số HP/damage/density theo độ khó được kiểm riêng.

Trong CSV campaign, tên cột lịch sử `ally_damage` là **tất cả damage ngoài entry mang ID hero**, gồm cả nguồn `ARENA` của relic/hiệu ứng không còn nguồn truy nguyên; không phải DPS thuần của companion. Vì vậy LoneJelly có thể có số ở cột này dù roster bằng 0. Không dùng cột đó để kết luận đoàn quân mạnh hơn hero. Bảng `Companions.csv` và `Weapons.csv` lọc đúng ID companion mang vũ khí; so sánh hướng đầu tư dựa trên kết quả wave, sống sót và chi tiêu, không dựa vào phép chia hero/ally này.

Việc mọi mẫu vũ khí gây damage không chứng minh mọi modifier projectile tương tác đúng với mọi pattern Arsenal. Các nhánh pattern có số lượng/tốc độ/tầm cố định cần phối hợp agent đang phụ trách vũ khí; đợt này không sửa cơ chế hoặc asset vũ khí của agent đó. Các cờ LOW/HIGH trong báo cáo 93 approach dùng để tìm điểm cần xem tiếp; hồi phục, kinh tế và điều kiện riêng không thể quy hoàn toàn về DPS. Không thay skill chỉ để ép mọi cờ về cùng một con số.
