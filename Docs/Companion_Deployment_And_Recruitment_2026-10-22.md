# Companion, quota tuyển và độ khó — 22/10/2026

**Đã có luật mới hơn:** ghép hai bản; từ wave 10 tuyển loài mới mỗi ba wave, mua tối đa hai bản trùng/wave và thêm ô nguyên liệu; shop đặc biệt mỗi ba/bốn wave. Xem [Recruitment_Two_Copy_And_Late_Pacing.md](Recruitment_Two_Copy_And_Late_Pacing.md). Nội dung và bằng chứng bên dưới lưu trạng thái trước thay đổi này.

## Đội hình và nâng sao

Mỗi loài có **một companion ra sân**. Bản trùng mua thêm được giữ làm nguyên liệu nâng sao, không sinh thêm người đánh, không cộng thành viên bond, passive, talent hoặc các bonus theo quân số. Bản có sao cao nhất ra sân; nếu bằng sao thì giữ bản sở hữu trước, tránh tự đổi vũ khí khi vừa mua nguyên liệu.

Ba nguyên liệu cùng loài, cùng sao ghép thành một bản cao hơn: 3 bản 1★ thành 2★; 3 bản 2★ thành 3★. Có thể tiếp tục gom bản 1★ sau khi đã có 2★. Các vũ khí nguyên liệu được giữ đến lúc ghép; kết quả chọn một vũ khí trong ba nguyên liệu đã dùng. Shop ưu tiên nguyên liệu cuối còn thiếu ở cả bước lên 2★ và 3★, với bảo vệ hạn kéo dài khi reroll.

Mô tả mua và bảng đội hình ghi tiến độ, số nguyên liệu đang giữ và việc bản trùng không tạo thêm người đánh. Preview bond dùng kết quả giao dịch thật: chỉ thêm nguyên liệu sẽ không báo thêm một thành viên bond. Các talent kích hoạt **khi mua** vẫn hoạt động theo cơ chế riêng.

Shop ngừng bán nguyên liệu của loài đã đạt 3★. Quà/sự kiện trao trực tiếp vẫn được nhận và giữ; không có trần tổng số companion. Ngoại lệ có sẵn của loài tham chiếu **Evil** vẫn giữ: không ghép, nên không bán thêm bản trùng vô dụng. Hero hấp thụ companion vẫn hấp thụ kỹ năng thay vì triển khai thêm người.

## Shop thường: lượt tích lũy

- Wave 1 có một lượt loài mới; thêm một lượt ở wave 3, 5, 7… Vì vậy đến wave 6 đã kiếm được ba lượt.
- Lượt chưa dùng được giữ lại, nhưng mỗi wave mua tối đa **một loài mới**. Nếu bỏ qua cả sáu wave đầu thì vẫn có thể mua bù từng wave sau đó.
- Khi còn lượt và pool có loài mới hợp lệ, shop bảo đảm có offer loài mới để người chơi mua bù.
- Mỗi wave vẫn có tối đa **một giao dịch nguyên liệu của loài đã sở hữu**, độc lập với lượt loài mới.
- Passive cộng lượt tuyển của hero cộng vào ngân sách tích lũy; không cho mua hai loài mới trong cùng wave.
- Reload/reroll không khôi phục lượt đã dùng. Quà companion thường từ relic mua trong shop giữ quy tắc quota giao dịch hiện có.

## Shop đặc biệt: độc lập

Mỗi lần vào shop đặc biệt mua tối đa **một loài mới**. Lượt này không tiêu ngân sách shop thường; mua ở một shop không khóa lượt của shop còn lại. Bản trùng ở shop đặc biệt giữ miễn quota nguyên liệu của shop thường. Không còn cooldown chung giữa hai shop từ wave 10. Reroll/reload không mở lại lượt trong cùng lần vào; game hiện cho vào một lần mỗi chặng shop.

## Save và độ khó

Save revision **4** giữ toàn bộ kho nguyên liệu và vũ khí, cùng tổng lượt loài mới đã dùng ở shop thường. Save cũ được chuyển khi nạp: không xóa tài sản, tự ghép bộ đủ điều kiện và chỉ triển khai một bản mỗi loài. Vì save cũ không ghi sổ nguồn mua/quà, số lượt thường đã dùng được ước tính bằng số loài đang sở hữu trừ loài khởi đầu; ước tính này có thể tính cả loài từng nhận từ quà hoặc shop đặc biệt.

**Mở sẵn cả sáu độ khó**, từ Peaceful đến Beyond the Lamp, cho cả profile mới và cũ. Các hệ số chiến đấu, challenge và thành tích thắng từng độ khó giữ quy tắc riêng.

## Kiểm tra

Bằng chứng được lưu tại [CompanionDeployment-20261022](../PreviewExports/CompanionDeployment-20261022/Verification.json): mua liên tiếp chín nguyên liệu qua transaction thật, một cơ thể mỗi loài, bond/passive, tiến độ 3★, vũ khí kế thừa, lưu/nạp đĩa, chuyển save cũ, ngân sách mua bù, hai shop độc lập và sáu độ khó. Chạy trong Unity batchmode; đợt này chưa build hoặc nghiệm thu hình ảnh Windows mới, cũng không thay thế playtest cân bằng dài hạn.
