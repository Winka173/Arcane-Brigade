# Đèn bị bóng tối làm lu mờ

Cập nhật 17/10/2026: thân đèn nhận độ sáng tại vị trí thật trên map thay vì giữ màu sáng cố định. Đèn vỡ ở xa đoàn chìm vào nền tối; ánh sáng đồng hành soi rõ thân đèn khi đến gần nhưng không tự thắp lại đèn vỡ.

Vùng tối quanh đèn vỡ giảm cả ánh sáng phát ra của đèn lân cận, quầng sáng và hạt sáng quanh đèn. Tính các vùng tối trước, cập nhật toàn bộ nguồn sáng rồi mới tô sprite để kết quả không phụ thuộc thứ tự đèn. Shop giữ ánh sáng ấm; bình minh xóa suy giảm do vùng tối.

Theo yêu cầu chủ dự án, chốt nhanh và commit/push, **không chạy kiểm tra/test cho bản sửa cuối này**. Không dùng kết quả nghiệm thu icon/thoại trước đó để xác nhận thay đổi ánh sáng.
