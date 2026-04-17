# Khách hiếm trao quà — 22/10/2026

Ba cư dân mới có thể ghé khi kết thúc wave và chuyển sang shop. Họ không đòi tiền hoặc điều kiện đổi chác. Người chơi tới điểm khách trên map rồi mở thẻ và nhận quà; đọc sau vẫn giữ khách đến hết shop. Nhận một lần, không xuất hiện lại bằng cách reroll/reload.

| Khách | Quà | Xác suất thử mỗi shop đủ điều kiện |
| --- | --- | --- |
| **Người Vá Sao** — Star Mender | Chọn ngẫu nhiên một companion 1★ đang ra sân, nâng thẳng lên 2★ | 4% |
| **Người Giữ Tiếng Chuông Thứ Ba** — Third Bell Keeper | Chọn ngẫu nhiên một companion dưới 3★ đang ra sân, nâng thẳng lên 3★ | 0,75% |
| **Người Giữ Quà Thất Lạc** — Unclaimed Gift Keeper | Một relic ngẫu nhiên, cao nhất Mythic | 2,25% |

Mỗi lần thử chỉ có tối đa một khách hiếm; phần xác suất của khách không phù hợp không chuyển cho khách khác. Bắt đầu từ shop chuẩn bị wave 3 (sau khi qua wave 2); khách 3★ bắt đầu ở shop chuẩn bị wave 9. Hai khách hiếm cách nhau ít nhất ba mốc shop, dù người chơi nhận hay bỏ qua. Không chen vào lúc đang có lựa chọn Customer, relight hoặc phần thưởng sân khấu. Khách thường vẫn dùng nhịp hiện có, thay bằng khách hiếm khi roll trúng; không chất thêm nhiều khách cùng lúc.

Nâng sao chỉ chọn trong **đội đang triển khai**, không chọn nguyên liệu dự trữ, companion truyện hoặc loài không ghép Evil. Giữ vũ khí hiện tại và mọi nguyên liệu đã lưu, không tạo thêm người đánh và không tiêu quota tuyển. Đây là quà nâng trực tiếp, không phải giao dịch ghép ba nguyên liệu: luật ngẫu nhiên vũ khí nguyên liệu của ghép thông thường vẫn giữ nguyên. Nếu không có đối tượng hợp lệ thì không xuất hiện khách đó; nếu đội thay đổi sau khi khách đã đến, thẻ sẽ khóa nhận khi không còn ai phù hợp.

Relic dùng trọng số **Common 45 / Uncommon 30 / Rare 20 / Mythic 5**; chỉ tính các bậc có vật phẩm hợp lệ trong pool hiện tại. Tôn trọng unlock wave, theme hero, giới hạn stack và pool shop vàng/trứng. Không trao Crimson hoặc Rainbow. Loại relic sinh thêm companion khỏi pool quà này để quà không âm thầm tiêu quota tuyển; các hiệu ứng riêng của relic được nhận vẫn chạy theo hệ hiện có.

## Art và nhân vật

- Người Vá Sao là một bà chuột sóc sống, áo choàng teal thêu đồng, kính nhỏ và cuộn chỉ hổ phách. Lời chào: “Con không cần phải kiếm lấy mọi sự tử tế.”
- Người Giữ Tiếng Chuông Thứ Ba là một cư dân cú lợn buồn ngủ, mặc áo nhân viên sân ga, khăn teal và mang chùm ba chuông đồng.
- Người Giữ Quà Thất Lạc là một cụ rùa đường sông, áo khoác xanh đêm, kính đồng, cầm hộp gỗ sơn cùng ruy băng đỏ rượu. Hộp chờ người nhận, không có hợp đồng đòi lại.

Cả ba dùng tranh mới tạo bằng `painted`, nét mực ấm và nhiều lớp màu kiểu sách truyện; không Blender, không glow đỏ phe ta. Mỗi PNG 1536×1024 chứa hai tư thế chờ/trao quà, tổng sáu tư thế. File RGBA giữ nguyên từ công cụ, không key nền; có vài pixel alpha 1/255 tại mép sheet rùa, không có phần thân chạm mép. Unity cắt cùng một khung cục bộ cho cả hai tư thế và dùng chung điểm chân. Tranh dùng cả ở điểm tương tác ngoài map và thẻ khách.

Nguồn, prompt và các lần chỉnh: [Rare_Visitor_Art_Prompts_2026-10-22.md](Rare_Visitor_Art_Prompts_2026-10-22.md). Asset nằm trong `Assets/ArcaneBrigade/Resources/RareVisitors/`. Import lại bằng menu **Arcane Brigade → Art → Import rare visitors**.

## Kiểm tra

[Bằng chứng kiểm tra](../PreviewExports/RareVisitors-20261022/Verification.json): 1.340 kiểm tra khách hiếm/art, 835 kiểm tra hệ khách và 22 kiểm tra Windows mỗi chế độ thường 1080p/giảm hiệu ứng 720p. Đã build Windows, nhận quà thật qua transaction, chặn nhận lặp và render cả ba khách trong cảnh shop. Giữ sáu ảnh cảnh đại diện trong báo cáo. Vì cửa sổ chạy ẩn cho ảnh màn hình đen, capture dùng camera render vào texture: ảnh có hậu tố `camera-only`, **không bao gồm lớp GUI của thẻ thoại**. Chưa nghiệm thu thị giác lớp thẻ thoại trong đợt này và chưa coi đây là bằng chứng cân bằng dài hạn.
