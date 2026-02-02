# Shop, tương tác trên map và đêm sâu hơn — 16/10/2026

Đợt này tiếp tục phản hồi sau bản `750181f0`. Chủ dự án đã yêu cầu commit/push toàn bộ đợt sửa và đặt mặc định tự commit/push sau khi hoàn tất, kiểm tra; quy tắc được lưu tại [AGENTS.md](../AGENTS.md). Bản Windows nằm ở đường dẫn cuối báo cáo, không đưa build/cache vào Git.

## Nhịp shop và phản hồi stat

Đồ cảnh chỉ hạ xuống lúc thực sự vào một lượt shop mới. Mua/reroll vẫn tạo lại mô phỏng để biên dịch stat, nhưng không khởi động lại animation đồ cảnh. Pause, nhật ký và màn truyện cũng không tính là lượt vào mới. Đèn tiếp tục được loại khỏi animation rơi.

Thông báo stat bỏ hộp nền và tên dài: có icon stat, chữ đậm màu tăng/giảm, nảy nhẹ khi xuất hiện, trôi lên và mờ dần trong khoảng 1,65 giây mỗi cặp dòng. Reduced motion giữ fade nhưng bỏ nảy/trôi. Theo lựa chọn xác nhận, số hiển thị làm tròn thành số nguyên, thay đổi dưới 1 ghi `+<1` / `-<1`, không có dấu phẩy/phần thập phân. Stat thật và bảng chi tiết không bị làm tròn theo popup. Gold, eggs, giá reroll và số lượng relic không xếp vào hàng thông báo trên đầu nhân vật; chúng đã có ở HUD/chi tiết. Vào combat xóa hàng chờ như trước.

## Vật thể mở nội dung

Bỏ nút HUD và phím tắt K/J, bỏ shortcut Y trên tay cầm. Tủ trưng bày/lưu trữ xanh bên trái shop mở journal; ghế lớn bên phải mở Paths & People. Đây là hai đồ cảnh đã có art vẽ và chuyển động lơ lửng. Chúng không bị dựng lại mỗi giao dịch.

Đi tới trong khoảng 2 đơn vị từ điểm neo sàn rồi click trực tiếp vào hình vật thể. Vùng click dùng bounds của sprite đang nổi/nghiêng, cộng padding; khoảng cách tới vật dùng bóng neo trên map. Có thể dùng Space/A tại vật để hỗ trợ bàn phím/tay cầm; không dùng K/Y/J. Nhãn hành động chỉ xuất hiện khi đứng gần. Đóng nội dung không làm cùng input lọt sang mua hàng. Các mốc truyện tự mở vẫn giữ.

Giữ hệ lơ lửng cho quầy/bệ, hàng, cửa, đèn và vật chứng. Tăng biên độ chung: thành phần trôi ngang từ 0,055 + 0,02 lên 0,12 + 0,04; thành phần nhấp nhô từ 0,08 + 0,02 lên 0,16 + 0,04, trước hệ số riêng từng vật. Chu kỳ chậm, pha riêng và điểm neo giữ nguyên. Reduced motion vẫn ổn định. Không thêm lại gió/khói dưới chân vật.

## Quota tuyển đã chốt

“Mới” tiếp tục là loài chưa có trong roster/memory hiện tại. Khác sao hoặc vũ khí của loài đang có là bản trùng nâng sao.

| Wave | Shop thường | Shop đặc biệt |
| --- | --- | --- |
| 1–9 | Tối đa 1 loài mới/wave; tổng 3 lượt companion gồm bản trùng | Tối đa 1 loài mới/wave; bản trùng không dùng quota mua thường |
| 10+ | Cả hai shop **gộp chung 1 loài mới mỗi 2 wave**; tổng mua thường vẫn 3/wave | Chịu cùng cooldown chung; bản trùng không dùng quota mua thường |

Ví dụ mua loài mới ở wave 10 bằng bất kỳ shop nào: cả hai shop chặn loài mới ở phần còn lại của wave 10 và wave 11; wave 12 mở lại. Không chia cặp wave cố định; mua ở 11 thì lần kế là 13. Counter mới và wave mua cuối lưu trong save, reroll không reset. Hai nguồn chỉ áp dụng quota mới cho loài mới; mua bản trùng vẫn theo luật cũ. Pool reroll đặc biệt bỏ loài mới khi quota hết nhưng giữ loài đã có. Quà companion thường từ relic cũng dùng gate chung để không lách bằng mua item.

Theo xác nhận tiếp theo, **bỏ trần 15 cơ thể cho toàn đội**, giữ quota mua. Đã gỡ các chặn cơ thể ở shop, quà relic/talent, Sprout, contract, cứu hộ và phần thưởng Keeper; companion cầu vồng nhận được dù đội đã hơn 15. Giữ quy tắc hero solo và vũ khí không trùng. Mốc 15 chỉ còn trong đường giảm trọng số loài mới, không chặn thêm thành viên. Quà talent/altar/Keeper/cứu hộ không tự đổi thành giao dịch mua loài mới. Save cũ không có lịch sử cooldown mặc định 0; không đoán ngược wave mua chưa được lưu. Counter mua thường cũ vẫn giữ khi tải, cooldown bắt đầu lưu từ lần mua loài mới theo hệ mới.

## Keeper và khung mô tả

Điểm thách đấu chuyển về `(5,2; -3,3)` phía dưới bên phải, dưới vùng HUD bond. Giữ bàn tay lơ lửng/quầng cầu vồng, thêm nhãn boss tùy chọn và giải thích phần thưởng/rủi ro khi tới gần. Từ đây chỉ cần **bấm Space/A một lần**; không giữ 0,85 giây. Sau chiến thắng, bước tới món quà rồi bấm Space/A một lần để nhận, bỏ nút Claim riêng. Mỗi món chỉ nhận một lần.

Keeper tăng scale thân từ 1,75 lên **2,45** (+40% so với bản trước); hitbox thân tăng 40%; HP nhân thêm **1,8** ở cùng wave/độ khó. Sau sáu giây bắt đầu triệu hồi nhóm 3–4 quái từ hai bên, lặp khoảng 8–11 giây theo HP; tối đa 12 vệ binh sống, có thời gian xuất hiện trước khi đánh. Giữ báo trước, miễn knockback và đòn chữ ký. Đây là tăng sức mạnh theo yêu cầu, chưa là nghiệm thu độ khó dài hạn.

Camera trận Keeper nới từ half-height tối thiểu 5,5 lên 7 để giữ thân và bộ phận lớn trong khung; capture dùng vị trí spawn thật `(0; 2)` của Keeper. Khung mô tả trong shop và chọn quà dùng màu rarity thật, kể cả Crimson/Rainbow; nền nhuộm nhẹ và viền màu. Có nét sáng trôi chậm trên viền; giảm chuyển động/hiệu ứng/chớp giữ viền tĩnh. Nội dung và giá không bị phủ bởi animation.

## Tối dần

Ambient theo năm depth đổi thành **0,92 → 0,58 → 0,27 → 0,075 → 0,018**. Wave 13–16 rút sáng nền mạnh hơn; wave 17+ giữ mức thấp có giới hạn, kể cả endless. Props ngoài nguồn sáng có floor 0,025 thay vì 0,1; sương đen cuối chặng tăng density từ 0,92 lên 1,12. Đây là hệ số render, không phải công bố phần trăm độ sáng cảm nhận trên màn hình.

Giữ bán kính ánh sáng hero/companion, nhận diện mắt địch thật, reveal gần/contact/windup, telegraph và đạn. Shop vẫn có ambient tối thiểu 0,5; High Readability giữ các mức sàn riêng; bình minh vẫn hồi sáng thật. Không thêm damage từ bóng tối hoặc nhiên liệu.

## Kiểm tra

Đã chạy 820 kiểm tra tuyển, 133 kiểm tra combat/art, audit 154 actor/vũ khí, 2.330 hero/description/shop, 575 shop, 272 Miền Chờ và 34 nightmare visuals. Thêm **17 kiểm tra** số stat/đội 15–20–40/nhận cầu vồng/save/boss triệu hồi và chạy lại **48 kiểm tra Keeper**. Các kiểm tra tuyển mới xác nhận cooldown hai hướng shop, bản trùng ở wave chờ, save/load và reroll.

Kiểm tra Windows theo thao tác thật ở [Capture_Runs.json](../PreviewExports/ShopFlow/Capture_Runs.json), ảnh và kết quả tại [Native](../PreviewExports/ShopFlow/Native/Native_Checks.txt) / [Reduced](../PreviewExports/ShopFlow/Reduced/Native_Checks.txt): reroll ba lần, mua tăng/giảm stat, pause, mở/đóng hai nội dung từ vật thể, click bounds theo sprite, shop lượt mới và so sánh ánh sáng đầu/cuối chặng. Thêm khung rarity, điểm thách đấu mới, boss lớn có vệ binh và nhận companion cầu vồng vào đội 20 người. Handler tương tác được gọi trực tiếp, không tuyên bố đã mô phỏng phần cứng chuột/phím bằng các kiểm tra này. Ảnh chữ và map được duyệt trực quan sau khi chạy.

Lượt chạy cuối có **56 kiểm tra / 12 ảnh mỗi chế độ**, tổng **112 kiểm tra / 24 ảnh**, gồm cả xác nhận các bộ phận Keeper lớn nằm trong khung camera. Runtime SHA-256: `570FED132455A7BE2F8FF4EA40627B4E382E4EE46D2DE4F02852B2DA2E8620AE`.

Capture boss dùng arena thật và triệu hồi thật nhưng đặt armor cao/tắt đòn boss để chụp, không phải chứng minh cân bằng hoặc chiến thắng bằng chơi thường. Capture nhận quà dùng fixture reward-state/đội 20 người, rồi gọi cùng handler Space; chiến thắng Keeper thật được kiểm tra riêng trong bộ Editor có fixture sát thương kết thúc boss.

Bản Windows mới ở `Builds/HeroIdentity/ArcaneBrigade.exe`, tránh ghi đè bản FullRun chủ dự án đang mở. Build thành công, **0 lỗi / 0 cảnh báo**; build report: [Build.txt](../PreviewExports/CombatRepair/Build.txt). Chưa có nghiệm thu cân bằng dài hạn, thiết bị mobile hoặc P95 mới trong đợt này.
