# Đèn nhỏ và bố trí lệch nhịp — 16/10/2026

Tám đèn cổ rời trên map đã giảm chiều cao từ 2,7 xuống **1,6–1,9 đơn vị**, tức nhỏ hơn khoảng **30–41%**. Mỗi đèn có kích thước, độ nghiêng nền và pha lơ lửng riêng. Góc nền 4,5–10,5° chia cả hai hướng; chuyển động chậm cộng thêm độ đung đưa nhẹ. Giảm chuyển động giữ góc bố trí ban đầu và độ nổi, dừng trôi/đung đưa.

Vị trí xê dịch theo seed của lượt chơi trong tám khu vực có khoảng cách an toàn, thay cho các cặp nằm đối xứng tuyệt đối. Cùng save giữ cùng vị trí khi tải lại hoặc qua wave; đèn hỏng vẫn ở chỗ cũ. Lượt mới có bố cục khác. Bộ tạo biến thiên riêng không dùng random của combat hoặc shop. Hai đèn shop có biên xê dịch nhỏ hơn, nằm ngoài khu bán hàng và được kéo vào trong khung hình.

Điểm lửa và ánh quầng đi theo sprite sau khi thu nhỏ/nghiêng; độ chiếu xuống sàn, bóng và khoảng cách hạt cũng được thu theo. Bán kính soi sáng giữ nguyên để việc thu nhỏ hình đèn không làm giảm khả năng đọc combat. Tiến trình đèn nứt/vỡ giữ nguyên.

Phạm vi là tám đèn cổ sprite độc lập, gồm hai đèn dùng trong shop. Những cụm đồ vật đã vẽ liền trong ảnh nền không được tách ra trong đợt này.

Kiểm tra: 64 seed kiểm tra vị trí, khoảng cách, giới hạn map, hai hướng nghiêng, save/load và vùng shop. Bộ chụp Windows riêng kiểm tra đèn nguyên/nứt/vỡ, vị trí khác nhau giữa hai lượt, khôi phục lượt cũ, nguồn sáng, bóng sàn, pause và giảm chuyển động. Bằng chứng cập nhật: [LanternVariation](../PreviewExports/LanternVariation/Gate_Summary.md). Chạy lại bằng `Tools/Story/Capture_Lantern_Variation.ps1` với bản Windows tại `Builds/HeroIdentity/ArcaneBrigade.exe`.

Kết quả bản cuối: **512 kiểm tra editor, 260 kiểm tra Windows thường + 260 giảm chuyển động, 10 ảnh mới**; build 0 lỗi/cảnh báo. Đèn shop bên phải được nâng khỏi vùng nút nhật ký. Bản `FullRun` đang mở được giữ nguyên; mở bản `HeroIdentity` để xem thay đổi này.
