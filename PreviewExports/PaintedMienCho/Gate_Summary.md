# Miền Chờ — nghiệm thu bộ tranh thay Blender, 16/10/2026

**PASS.** Đã thay **13 asset** bằng tranh mới bám sheet gốc của game, đưa FBX/prefab/material đèn Blender và các ảnh render cũ khỏi Resources. Bản Windows D3D11: **Succeeded / 0 lỗi / 0 cảnh báo**, `Builds/HeroIdentity/ArcaneBrigade.exe`. [Build](../ActOne/Build.txt), [Verification.json](Verification.json), [prompt và SHA-256](../../Docs/Mien_Cho_Painted_Art_Prompts.json).

| Phạm vi | Kết quả trên cùng Player |
| --- | --- |
| Asset vẽ mới | 13/13 đúng hash/kích thước; 7 sprite alpha thật và 6 panel/nền đục |
| Editor hồi 1, truyện, roster, tuyển và hero | 4616 kiểm tra qua |
| Chuyển động dùng chung | 23234 mẫu kiểm tra giới hạn, pha và Reduced Motion qua |
| Đèn, ánh sáng/sương, phòng khám phá — thường / giảm hiệu ứng | 480 + 480 qua; 34 ảnh |
| Ba cảnh bàn ăn, điện thoại và gương — thường / giảm hiệu ứng | 71 + 71 qua; 8 ảnh |
| Truyện/journal/hai kết thúc — thường / giảm hiệu ứng | 58 + 58 qua; 42 ảnh |
| Shop cũ: bệ, hàng, rarity, mua và cửa — thường / giảm hiệu ứng | 432 + 336 qua; 18 ảnh |

Tổng **29836 kiểm tra / 102 ảnh**; phần lớn số kiểm tra là mẫu giới hạn chuyển động tự động, không phải từng ấy cảnh duyệt tay. Tất cả ảnh được đối chiếu thời gian với lượt chạy và Player; 18 ảnh shop được kiểm tra thêm nội dung pixel để loại ảnh đen. [Gate chuyển động](../Levitation02/Verification.json), [gate hồi 1](../ActOne/Verification.json).

Ảnh tiêu biểu: [đèn đầu hành trình](../ActOne/Native/lamps-and-fog-wave-1.png), [đèn vỡ trong đêm](../ActOne/Native/lamps-and-fog-wave-19.png), [bàn ăn](../MienCho/Encounters/Native/encounter-4-world.png), [gương có companion thật](../MienCho/Encounters/Native/encounter-10-world.png), [shop có hàng](../Levitation02/Shop/Native/suspended-shop-later.png), [nhà ga](../ActOne/Native/room-8-world.png), [phòng hoàn hảo](../ActOne/Native/room-16-world.png), [panel bình minh và đội hình](../MienCho/Native/ending-LeaveLight-3.png).

Đèn sprite nguyên/nứt/vỡ dùng cùng thiết kế và điểm neo. Halo/field ánh sáng Unity đi theo lửa khi nghiêng; không còn mesh/Point Light của đèn Blender. Giữ khoảng nổi, nghiêng chậm, bóng sàn và pause; không tạo khói/vệt gió dưới đồ vật. Gương giữ phản chiếu companion thật. Các cảnh khám phá lưu/tải lựa chọn và quay lại shop đúng vàng/quân số; hai ending dẫn tới bình minh thật.

Fixture 15 companion đo 120 frame: **17,50 ms thường / 17,77 ms giảm hiệu ứng**. Bao gồm cap và tải chụp, có giới hạn spawn; không phải benchmark trận đông mọi cấu hình. Nền phòng là tranh 2D, đồ vẽ liền nền không chuyển động độc lập. Chưa tuyên bố nghiệm thu thẩm mỹ của chủ dự án, chơi cân bằng cả chiến dịch hoặc tay cầm vật lý. Hồi 2–3 chưa triển khai.

[Chi tiết thay art và đường dẫn lưu](../../Docs/Mien_Cho_Painted_Art_Pass.md). Blender chỉ còn nguồn lịch sử ngoài tài nguyên Player; bộ mới dùng công cụ painted tích hợp với art gốc làm tham chiếu.
