# Ghép hai bản và nhịp tuyển late — 22/10/2026

Luật hiện hành sau hai yêu cầu mới của chủ dự án. Thay thế các con số ghép ba bản và lịch tuyển hai wave xuyên suốt trong báo cáo cũ.

## Nâng sao

- **Hai bản cùng loài, cùng sao → một bản cao hơn một sao.** Hai bản 1★ thành 2★; hai bản 2★ thành 3★. Nếu bắt đầu toàn 1★ thì cần bốn bản để đạt 3★.
- Kết quả chọn ngẫu nhiên một vũ khí trong **hai nguyên liệu đã dùng**, mỗi nguyên liệu có xác suất 50%. Không roll ngoài nguyên liệu.
- Một loài chỉ có một người ra sân; nguyên liệu đang giữ không đánh, không cộng thành viên bond hoặc passive. Chưa ghép thì vũ khí của người đang ra sân giữ nguyên.
- Shop ưu tiên sao của nguyên liệu còn thiếu: đã có một bản 2★ thì offer bản trùng có thể là 2★ để hoàn tất 3★, với giá offer sao cao hiện hành. Preview, tiến độ, thoại và hiệu ứng ghép đã đổi theo hai nguyên liệu.
- Giữ ngoại lệ không ghép của Evil và cơ chế hero hấp thụ. Little Sprout dùng ngưỡng hai bản mới; phần thưởng Grand Sprout đi kèm vẫn giữ. Công thức altar và Splice Kit dùng ba **loài khác nhau** giữ quy tắc riêng.

## Shop thường

Lượt loài mới tích lũy ở các wave **1, 3, 5, 7, 9, 12, 15, 18, 21…**. Từ wave 10, kiếm thêm một lượt mỗi ba wave. Lượt chưa dùng không mất; khi mua bù vẫn tối đa một loài mới mỗi wave. Passive cộng lượt của hero vẫn cộng vào ngân sách.

| Wave của shop | Tổng lượt cơ bản đã kiếm |
| --- | --- |
| 6 | 3 |
| 9 | 5 |
| 10–11 | 5 |
| 12–14 | 6 |
| 15–17 | 7 |
| 18–20 | 8 |

Để tập trung nâng chất lượng đội, từ wave 10:

- Tăng quota mua bản trùng ở shop thường từ **một lên hai giao dịch mỗi wave**.
- Thêm ô thứ hai ưu tiên nguyên liệu của loài đã sở hữu, nếu có loài hợp lệ và còn quota. Hai ô ưu tiên các loài khác nhau để tránh mua xong một ô lên max sao rồi ô còn lại trở thành vô dụng.
- Ô loài mới vẫn bảo đảm offer khi còn ngân sách và pool có ứng viên. Không tăng quota loài mới, không tăng giới hạn cơ thể và không tăng trực tiếp stat chiến đấu.

Quota mua ở shop đặc biệt độc lập: **một loài mới mỗi lần vào**, bản trùng không dùng quota thường.

## Shop đặc biệt

Chuyển từ xác suất dựa vào số trứng sang lịch **mỗi ba hoặc bốn wave**, khoảng cách được chọn theo RNG có lưu. Shop đầu ở sau wave 3 hoặc 4; sau khi tạo shop tiếp tục hẹn ba/bốn wave kế tiếp. Nhịp này tính theo shop được mở, không theo việc người chơi có ghé mua hay không.

Giữ lịch qua save/reload/reroll. Trứng dùng mua hàng, không làm shop xuất hiện sớm hơn. Luật nhịp áp dụng chung, gồm hero trước đây có dòng “Dream shops always open”; pact cấm dream shop vẫn có hiệu lực. Shop đã hiện trong save cũ được giữ nguyên.

## Save và phạm vi kiểm tra

Save revision **5** lưu lịch shop mới và phần ngân sách tuyển chuyển tiếp. Khi nạp save cũ:

- Giữ mọi entry, vũ khí và cấp sao hiện có; bộ nguyên liệu đủ điều kiện sẽ ghép theo ngưỡng hai bản mới.
- Giữ tổng lượt tuyển đã kiếm theo lịch cũ bằng khoản chuyển tiếp, nên không thu hồi lượt đã mua hoặc làm mất lượt chưa dùng.
- Hẹn shop đặc biệt kế tiếp sau ba/bốn wave từ mốc save; giữ hàng và quota hiện có nếu đang trong shop.

**Không chạy test, mô phỏng hoặc build trong đợt này theo yêu cầu chủ dự án.** Chỉ rà mã nguồn, các chuỗi mô tả và diff. Các kết quả test trong báo cáo cũ không phải nghiệm thu của luật mới này.
