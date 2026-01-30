# Miền Chờ — thay art Blender bằng tranh vẽ

Theo yêu cầu mới ngày 16/10/2026, toàn bộ nhóm art Miền Chờ từng dựng/render từ Blender được thay bằng **13 asset vẽ mới**. Dùng công cụ **painted tích hợp**, qua skill painting, với hai sheet có sẵn của game làm tham chiếu nét vẽ: `FinalQuality/furniture-00_v1.png` và `AnimationHD/NightMarket_v1.png`. Không dùng bố cục hoặc ảnh render Blender làm đầu vào cho bộ mới.

## Bộ thay thế

| Nhóm | Asset trong Resources/MienCho |
| --- | --- |
| Đèn đường, cùng một thiết kế ở ba trạng thái | `Lanterns/Intact.png`, `Cracked.png`, `Broken.png` |
| Ghế trống | `EmptyChair.png` |
| Bàn, ba ghế, hai tách và chỗ ngồi thừa | `Encounters/ReservedTable.png` |
| Điện thoại quay số | `Encounters/BreathingPhone.png` |
| Gương đứng, mặt kính trống để ghép companion thật | `Encounters/LateReflection.png` |
| Ba panel chờ đợi, thư bị giữ và bình minh thật | `WaitingRoom.png`, `WithheldDawn.png`, `OpenDawn.png` |
| Ba nền phòng khám phá vẽ lại từ đầu | `Journey/Station_Painted_v1.png`, `PerfectRoom_Painted_v1.png`, `FootstepHall_Painted_v1.png` |

Nét viền đậm, hình khối tròn lệch nhẹ, màu nhung đỏ, vàng cổ và xanh đêm bám sheet gốc. Bảy sprite rời giữ alpha thật; sáu nền/panel là ảnh đục. Hai trạng thái đèn hỏng được sửa trực tiếp từ thiết kế nguyên để giữ dáng và điểm neo.

## Unity và chuyển động

Đèn dùng sprite vẽ ở cả chế độ thường và giảm hiệu ứng. Loại bỏ nhánh tải FBX/prefab và các Point Light dành cho mesh cũ. Ánh sáng động tiếp tục dùng field Unity 256×192, halo mềm và vùng sáng đội hình; tâm lửa được đặt trên chính sprite rồi biến đổi theo độ nghiêng/nhấp nhô. Sương môi trường tiếp tục bắt sáng qua shader nhiều mẫu. Các pass ngoài màn hình khôi phục render target sau khi vẽ, tránh để texture sương trở thành đích của bước render sau.

Giữ lơ lửng, nghiêng chậm lệch pha và bóng neo sàn. **Không có khói/vệt gió dưới đồ vật.** Gương vẫn ghép sprite companion thật và nghiêng phần phản chiếu theo khung. Điểm tương tác, lựa chọn, save, quota và chiến dịch 20 wave giữ nguyên. Nền phòng là tranh ghép liền; các vật vẽ liền trong nền không chuyển động độc lập.

FBX, prefab, vật liệu/mesh bake và ảnh render/concept cũ được lưu tại `ArtArchive/MienCho_Blender_20261016`, ngoài `Assets`; không còn được gói vào Player qua Resources. Các file `.blend` trong `Tools/Blender` chỉ còn là nguồn lịch sử, không dùng để tạo hoặc chạy bộ art mới. Hồi 2–3 tiếp tục để sau.

## Nguồn và nghiệm thu

Đã chạy đủ tám lượt Windows thường/giảm hiệu ứng: hồi 1/đèn/phòng, ba encounter, truyện/hai ending và shop cũ. **0 lỗi / 0 cảnh báo build**, 102 ảnh có thời gian khớp Player được ghi hash. Gate tổng 29836 kiểm tra, trong đó 23234 là mẫu giới hạn chuyển động tự động; không diễn giải thành từng ấy cảnh duyệt tay. Đã xem ảnh đèn, đèn vỡ, gương/companion, bàn ăn, nhà ga và shop có hàng. Nguồn Blender cũ được lưu làm lịch sử ngoài tài nguyên Player.

- [Prompt đầy đủ, đường dẫn ảnh gốc, kích thước và SHA-256 của 13 asset](Mien_Cho_Painted_Art_Prompts.json).
- [Gate Windows và ảnh trong game](../PreviewExports/PaintedMienCho/Gate_Summary.md).
- [Chuyển động lơ lửng](Dream_Levitation_02.md).

