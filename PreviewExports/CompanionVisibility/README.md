# Sửa đồng minh chớp/mất thân và thu gọn HUD bond

## Nguyên nhân chớp

`BodyPuppet` vẽ thân trên và chân bằng các sprite/frame riêng. Khi đang đuổi theo mục tiêu hoặc đang đánh, chúng có thể nằm ở các ô khác nhau trong atlas. Property block shader trước đây chép `_UvRect` của sprite tổng sang cả hai phần. Shader `UnitFlash` có bước cắt alpha ngoài `_UvRect`, nên thân bị cắt mất khi các ô không trùng nhau, dù `_Flash` đã bằng 0.

Đã giữ các thuộc tính ánh sáng/chuyển động, đồng thời đặt `_UvRect` theo frame nguồn thực của từng phần thân. Các animation di chuyển, tấn công và phản ứng vẫn hoạt động.

- [Tái hiện cách cũ trên GPU: thân mất hoàn toàn](Native/TeapotPilgrim-legacy-UV-reproduction.png)
- [Cùng trạng thái sau sửa](Native/TeapotPilgrim-attack-fixed.png)
- [Kiểm tra trước sửa](Before_Fix.txt): Teapot Pilgrim, cấp 1, đang di chuyển và chuẩn bị đánh, cả hai phần thân đều bị clip.

Kiểm tra edit mode: 66 loài đồng minh × 3 cấp sao + 31 champion, mỗi trường hợp có 12 trạng thái di chuyển, chuẩn bị đánh, nhả đòn, pursuit và hurt. Tổng 2.748 trạng thái; thêm 2 kiểm tra HUD bond. Có 2.944 mẫu renderer thân tách được kiểm tra.

Kiểm tra Windows render thực: so số pixel hiện hữu trên GPU với ảnh đối chứng có vùng UV hợp lệ, tách biến dạng bề mặt khỏi phép đo alpha. Kiểm tra cả chế độ thường và giảm hiệu ứng. Bản tái hiện UV cũ cho Teapot Pilgrim trả về **0 pixel**, sau sửa có **2.511 pixel**.

[Kết quả edit mode](Focused_Checks.txt), [Windows thường](Native/Native_Checks.txt), [Windows giảm hiệu ứng](NativeReduced/Native_Checks.txt).

## HUD bond

HUD góc phải chỉ hiện tối đa 5 bond, cao tối đa 224px ở bố cục 1080p. Ưu tiên cấp đang kích hoạt, rồi tiến độ tới ngưỡng kế tiếp. Mỗi dòng có icon, tên, bộ đếm, cấp và thanh tiến độ mảnh. Header cho biết tổng số bond đang kích hoạt; footer hiện số mục còn lại và nút View all.

View all mở tab Links ở bond đầu bảng và tạm dừng game. Danh sách đầy đủ vẫn có thể cuộn để xem từng hiệu ứng. Tooltip từng bond trên HUD được giữ lại.

- [29 bond được thu gọn ở 720p](../BondHud/Native/05c-bonds-720.png)
- [Danh sách và mô tả đầy đủ ở 720p](../BondHud/Native/05d-bonds-full-list-720.png)
- [Kiểm tra Windows HUD](../BondHud/Native/Native_Checks.txt)

Build: `Builds/FullRun/ArcaneBrigade.exe`. Các capture dùng profile chẩn đoán riêng.
