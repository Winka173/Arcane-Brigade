# Nhịp tuyển companion — cập nhật 16/10/2026

Giữ trần cũ **3 lượt mua companion ở shop thường mỗi round**, gồm cả bản trùng mua để nâng sao. Bổ sung quota loài mới bên trong trần này:

| Round | Tổng lượt companion thường | Trong đó, tối đa loài mới |
| --- | --- | --- |
| 1–3 | 3 | 1 |
| 4 trở đi, gồm endless | 3 | 1 |

“Loài mới” là ID loài chưa có trong roster hiện tại. Khác vũ khí hoặc khác sao nhưng cùng loài không phải loài mới. Với hero hấp thụ companion, memory đã sở hữu cũng được tính là loài đã có.

Ví dụ round 5: mua một loài mới rồi mua hai bản trùng để ghép là đủ ba lượt. Không mua được loài mới thứ hai và cũng không mua thêm bản trùng thứ tư. Nếu chỉ hết quota loài mới mà vẫn còn lượt tổng, shop tiếp tục cho bản trùng. Sau ba lượt tổng, shop thường chỉ roll item. Quà companion thường từ relic dùng cùng các quota, nên không vòng qua giới hạn.

Companion mua từ **shop đặc biệt** không dùng quota tổng và không dùng quota loài mới; vẫn mua được khi shop thường đã hết lượt. Ngoại lệ tính theo nguồn offer, không theo rarity hay loại tiền. Giới hạn cơ thể **15**, giá, unlock, theme, vũ khí không trùng và quy tắc hero vẫn áp dụng. Quà phụ từ talent/altar/sự kiện giữ quy tắc riêng hiện có. Theo phản hồi mới nhất, ba round đầu cũng chỉ được một loài mới mỗi round.

## Xác suất tìm loài mới

Chỉ nhánh cho phép loài mới chịu đường giảm theo quân số. Không áp đường giảm quân số lên slot companion/bản trùng. Nhịp giảm slot cũ sau lần mua thường thứ hai vẫn giữ: cơ hội nhánh companion còn 15%; sau lần thứ ba là 0. Shop đặc biệt không bị nhịp giảm do số lượt mua thường này.

- Round 1–3: gate loài mới 85% khi dưới 6 cơ thể; từ 6 cơ thể giữ tối thiểu 70% để giai đoạn mở đầu vẫn dễ tuyển.
- Round 4+: dưới 6 cơ thể giữ 85%; từ đúng 6 bắt đầu giảm.
- Với `t = clamp01((quân_số − 6) / 9)`, gate từ 6 cơ thể là `lerp(0.55, 0.02, 1 − (1 − t)²)`.

| Cơ thể hiện có | Gate gốc round 4+ |
| --- | --- |
| 5 | 85% |
| 6 | 55% |
| 8 | khoảng 34,06% |
| 10 | khoảng 18,36% |
| 12 | khoảng 7,89% |
| 15 | 2% |

Đây là **gate cho phép đưa loài mới vào pool**, trước Luck, trọng số chọn ứng viên, ưu tiên ghép, theme và giới hạn đội. Không phải tỉ lệ cuối mỗi ô shop. Nếu lượt mới bị chặn, loài đang có vẫn nằm trong pool nâng sao cho đến khi đủ ba lượt mua thường.

## Ghép sao, lưu game và UI

Giữ công thức ghép ba, cấp offer, kế thừa ngẫu nhiên vũ khí thật từ nguyên liệu, ưu tiên ghép và bảo vệ chuỗi reroll không gặp mảnh. Quota loài mới không ngăn mua bản trùng; trần ba lượt tổng vẫn ngăn tăng sao quá nhanh.

Hai counter lưu trong save, reroll không reset. Hoàn thành trận thường và mở endless reset counter theo vòng shop hiện có. Save cũ còn counter tổng 0–3 vẫn tải được; counter loài mới chưa có dữ liệu sẽ mặc định 0, trần tổng vẫn bảo vệ lượt shop đang dở.

Mô tả máy reroll hiển thị cả lượt tổng và lượt loài mới còn lại, nhắc shop đặc biệt được miễn. Khi mua bị chặn, bảng từ chối phân biệt **hết ba lượt companion** với **hết quota loài mới**. Description relic tặng companion được cập nhật cùng luật.

## Kiểm tra

[Verification.json](../PreviewExports/Recruitment/Verification.json) và [Checks.txt](../PreviewExports/Recruitment/Checks.txt): bộ kiểm tra round 1/2/3/4/8/20/30, offer cũ, bản trùng/ghép sao, shop đặc biệt trước và sau quota, memory hấp thụ, quà relic, reroll, save/load, reset qua trận thắng thật, đường xác suất và roll có seed. Đợt cập nhật quota một loài mới có **799 kiểm tra đạt**; số 802 thuộc luật hai loài ở ba round đầu trước đây.

Trong 400 lần roll mỗi trường hợp ở cùng seed, đội 5/6/12 người cho **303/182/25 offer loài mới**. Đây là kiểm tra xu hướng trên mẫu cố định, không phải công bố tỉ lệ toàn game. Bộ **2.330 kiểm tra hero/description/shop feedback** cũng được chạy lại khi build.

Đợt này chỉ đổi nhịp tuyển; chưa áp đề xuất đội 12 người, cách tính bond theo loài hoặc mechanic companion mới.

Cập nhật trần đội và bond: [Roster15_And_Bond_Balance.md](Roster15_And_Bond_Balance.md).
