# Hướng hình ảnh sửa lại — Minimal Arcade

Ngày 2026-10-13. Chủ dự án yêu cầu xem kỹ Bizarre Brigade, bỏ các mẫu anime chibi fantasy nhiều chi tiết.

**Đã duyệt hướng:** chủ dự án đánh giá mẫu Minimal Arcade nhìn tốt. Bản implement cần thêm một vài chi tiết và cá tính độc lạ, nhưng giữ nền đơn giản. **Không có cánh tay/cẳng tay: chỉ có bàn tay cầm vũ khí, gắn và chuyển động cùng vũ khí.** Đây là yêu cầu thiết kế của chủ dự án; không suy rộng thành kết luận đã kiểm chứng mọi nhân vật của bản tham chiếu.

## Đối chiếu hình ảnh thực tế

Đã mở và nhìn trực tiếp ảnh gameplay và shop từ [Sportskeeda](https://www.sportskeeda.com/esports/zenless-zone-zero-zzz-don-t-walk-alone-event-guide), [ảnh giao diện có mod tooltip](https://gamebanana.com/mods/546288) và [Icy Veins](https://www.icy-veins.com/zenless-zone-zero/bizarre-brigade). Ảnh tham khảo chỉ dùng nghiên cứu, không đưa vào Assets hoặc gói phát hành.

Nhận xét thị giác: silhouette phẳng, đầu lớn/thân nhỏ hoặc nguyên một khối blob, mặt rất ít nét, ít mảng màu, chi tiết vũ khí đơn giản. Không phải nhân vật fantasy nhiều lớp giáp, mắt anime, tóc vẽ từng lọn, chất liệu kim loại, gouache hay hatch dày. Tên gọi "vẽ tay nét cao" ở đây là đường cong/biên sạch, không phải tăng mật độ chi tiết.

## Luật art mới

- Ưu tiên nhận diện ở 32–48 px và trong đội quân đông hơn độ đẹp khi phóng lớn.
- Mỗi nhân vật có một silhouette nổi bật và một đặc điểm nhận diện chính; mặt dùng chấm/gạch ngắn.
- Có thể thêm một hoặc hai chi tiết lớn để tăng nhận diện và sự độc lạ; không quay lại kiểu chibi fantasy nhiều giáp/hoa văn.
- Ít màu, ít bóng đổ, không texture/phụ kiện li ti, không giáp nhiều mảnh, không mắt anime.
- Vũ khí đọc được bằng hình khối. Mẫu này gộp vũ khí vào sprite để thử hình ảnh, chưa phải hệ vũ khí thay độc lập.
- Prefab implement tách body với cụm grip: bàn tay đi cùng vũ khí khi swing/aim/recoil, không dùng rig tay dài hoặc vẽ bộ phận nối về vai. Đổi vũ khí không cần vẽ lại body.
- Animation đầu tiên là squash/lean và bước chân nhỏ; không yêu cầu key pose chiến đấu phức tạp ở bản style thử.
- Không sao chép nhân vật hoặc asset của ZZZ; chỉ theo ngôn ngữ đơn giản và dễ đọc.

## Mẫu hiện tại

Ba nhân vật gốc: SleepyKnight, HoodWisp và GrumpyJelly. Mỗi nhân vật có bốn pose raster: idle, bước trái, bước phải, action. Sinh bằng công cụ image generation tích hợp, sau đó Unity import/slice thành sprite và tạo prefab/clip.

Đây là **style prototype**, chưa phải bộ animation production hoàn chỉnh, chưa có rig nhiều lớp, nhiều hướng nhìn hoặc game combat. Có thể còn lệch nhẹ hình dáng/chi tiết giữa các frame; phải chỉnh production sau khi chọn style. Không dùng bốn sheet fantasy trước đó làm chuẩn art.

Được duyệt style không đồng nghĩa các sprite gộp vũ khí này đã đạt hợp đồng prefab mới. Bước tiếp theo là mẫu tách body/bàn tay/vũ khí theo [Implementation_Plan.md](Implementation_Plan.md), kiểm tra motion, flip và nhiều tổ hợp vũ khí trong Unity.

Scene: `Assets/ArcaneBrigade/StylePreview/Scenes/MinimalArcadePreview.unity`.

Menu dựng lại: `Arcane Brigade > Style Preview > Build scene and capture`.

Ảnh ở `PreviewExports/` do Unity Camera.Render xuất, không phải ảnh giả lập gameplay. Preview dùng built-in renderer để kiểm tra sprite với ít dependency; không tuyên bố đã cấu hình URP hoặc build Android/iOS.
