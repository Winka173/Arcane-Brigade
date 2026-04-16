# Điều chỉnh đàn quái, xu, cận chiến và record — 22/10/2026

Đây là trạng thái cuối sau các phản hồi tiếp nối trong ngày; các mức đề xuất trung gian được thay bằng các mức dưới đây.

- Wave 1–9 giữ nguyên nhịp và thành phần đàn quái. Từ wave 10, giữ nguyên thời điểm ra đàn, khoảng cách an toàn và các chu kỳ cao/thấp; mỗi đàn thường có hai quái trung/bự và khoảng nửa số quái nhỏ cũ (làm tròn lên). Ví dụ 1 dẫn + 19 nhỏ → 2 trung/bự + 10 nhỏ. Chọn loài lớn hơn từ pool đã mở, không chỉ phóng to quái nhỏ. Đợt surge cũng còn nửa số nhỏ và thêm hai hộ tống. Nhóm bù giờ boss vẫn giữ giới hạn riêng tối đa bốn cơ thể.
- Xu thưởng hạ địch được gom theo 4 lần hạ ở wave 1–9, 8 lần ở wave 10–14 và 12 lần từ wave 15. Boss/elite chi thưởng ngay; phần chưa đủ một nhóm được xử lý khi thắng wave. Các phần trăm lẻ giữ lại giữa các lần rơi trong wave; dưới một đơn vị xu còn dư ở cuối wave bị làm tròn xuống.
- Tối đa **32 pickup xu** đang tồn tại, kể cả đang hút. Xu phát sinh thêm gộp giá trị vào xu sẵn có; không bỏ tiền khi chạm trần. Giữ hình xu nhỏ hơn 40% đã chốt ở đợt trước.
- Thưởng hạ địch còn **90%**. Quái hộ tống mang thêm phần thưởng của các quái nhỏ bị bỏ đi, để tổng thưởng raw của một đàn đầy đủ giữ nguyên trước khi giảm 10%, thay vì vô tình giảm gần nửa kinh tế. Rương, coin từ skill và hoàn trả tiền bị lấy không bị áp dụng thêm mức giảm thưởng hạ quái. Các bonus dựa trên số lần hạ vẫn đi theo số địch thực tế; không bảo đảm tổng vàng của mọi build giảm chính xác 10%.
- Nhặt xu trong trận và gom xu cuối wave dùng chung CoinValueScale, hệ số hero và phần tiền lẻ. Sửa lỗi cũ: sweep cộng thẳng raw value, bỏ qua hệ số .75, dẫn đến để xu trên sàn được nhiều tiền hơn đi nhặt. Vì sửa lỗi này, người trước đây thường để tiền được gom cuối wave có thể thấy mức giảm lớn hơn 10%.
- Cận chiến: damage multiplier **0.9 → 1.05**, tăng lại khoảng 16.7% sau lần nerf trước (vẫn thấp hơn 30% so với mức 1.5 cũ). Cooldown, windup và recovery ×**1.25**, nhịp đánh khoảng −20% nếu không bị các giới hạn/ngoại lệ riêng chi phối. Hệ số di chuyển companion cận chiến **1.15 → 0.9**, khoảng −21.7%; hero và ranged không nhận hệ số giảm di chuyển này.
- Record sau trận và lịch sử trận: bỏ gấp top 5/More cho đội hình, thêm danh sách toàn bộ bond có thành viên trong đội, gồm cả bond chưa kích hoạt. Hiển thị icon, tên, số thành viên và cấp; dùng vùng cuộn chung. Việc đọc bond restore từ bản sao tách rời, không sửa record lưu.

## Kiểm tra

Unity 6000.4.8f1 batchmode biên dịch và chạy qua:

| Bộ kiểm tra | Qua |
| --- | ---: |
| LateCrowdCoinChecks | 63 |
| Sandbox vũ khí K4 | 1470 |
| Xu/animation K2 | 131 |
| Độ khó/lưu tiến độ K6 | 72 |
| Tổng | 1736 |

Kiểm tra tập trung dùng simulation thật để đếm đàn ở wave 1/6/9/10/11/15/20, kiểm tra hộ tống là loài lớn hơn, giữ ngân sách thưởng từng đàn, surge 20 nhỏ → 10 nhỏ + 2 hộ tống, gom 960 lần hạ ở từng mức batch, trần 32 khi mọi xu đang hút, sweep lặp không trả hai lần, reset/nhóm còn dở và record có hơn năm bond không bị cắt/mutate.

960 lần hạ với raw reward 1, CoinValueScale .75: tổng trả **648 vàng** thay vì **720 vàng**. Bài kiểm tra này xác nhận tỷ lệ thưởng, không đại diện cho tổng thu nhập mọi lượt chơi.

Bằng chứng: [kiểm tra tập trung](../PreviewExports/LateCrowdCoins-20261022/Focused_Checks.txt), [sandbox](../PreviewExports/LateCrowdCoins-20261022/WeaponSandbox_Checks.txt), [xu/animation](../PreviewExports/LateCrowdCoins-20261022/CoinAnimation_Checks.txt), [độ khó](../PreviewExports/LateCrowdCoins-20261022/Difficulty_Checks.txt).

Chưa nghiệm thu nghe/nhìn hoặc đo frame time trên bản Windows mới. Hiệu ứng kích hoạt theo mỗi lần nhặt vật lý có thể kích hoạt ít hơn do xu được gom thành ít pickup; không nhân giả số sự kiện nhặt.
