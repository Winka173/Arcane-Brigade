# Cố định vị trí chat đang đọc

Cập nhật 17/10/2026: mỗi câu thoại chỉ chọn vị trí một lần khi bắt đầu được vẽ, sau đó giữ nguyên khung và điểm đuôi trong tọa độ màn hình đến hết câu. Người chơi, người nói hoặc camera di chuyển không khiến câu đang đọc đổi góc. Khi người nói biến mất sau lần hiển thị đầu, câu vẫn ở vị trí đã chọn đến hết thời lượng.

Khung mới tránh vị trí các câu đã hiện; câu có độ ưu tiên cao hơn không đẩy câu cũ đi. Câu mới của cùng người nói vẫn thay câu trước theo hệ thống hiện có. Giữ animation xuất hiện/ẩn và chữ hiện dần; bỏ trượt ngang của lời thì thầm. Thông báo stat tiếp tục tránh khung chat đã cố định.

Theo yêu cầu chốt nhanh trước đó, không chạy test/build cho bản sửa này.
