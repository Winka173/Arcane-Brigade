# Hồi 1 — nghiệm thu đèn cổ và sương đen, 16/10/2026

**Gate hiện hành sau đổi art:** [13 asset vẽ thay Blender](../PaintedMienCho/Gate_Summary.md). Bản Player mới **0 lỗi/0 cảnh báo**, gate hồi 1 **5834 qua / 84 ảnh**; thêm kiểm tra chuyển động/shop thành **29836 qua / 102 ảnh**. Đèn hiện là sprite vẽ với field Unity, không còn mesh/Point Light Blender. Hai lượt đèn/phòng hiện có **480 kiểm tra và 17 ảnh mỗi lượt**, fixture 15 companion đo **17,50 / 17,77 ms** thường/giảm hiệu ứng. [Verification.json](Verification.json) đã cập nhật hash và lượt chạy; bảng và mô tả bên dưới ghi lại gate mesh cũ.

**PASS.** Windows D3D11, `Builds/HeroIdentity/ArcaneBrigade.exe`: **Succeeded / 0 error / 0 warning**. [Build.txt](Build.txt), [Build_Messages.txt](Build_Messages.txt), [Verification.json](Verification.json).

| Phần kiểm tra | Kết quả |
| --- | --- |
| Đèn, save, phòng và tuyến companion mới — Editor | 191 qua |
| Truyện/journal/ánh sáng nền — Editor | 272 qua |
| Trần 15 và bond | 1021 qua |
| Nhịp tuyển và quota | 802 qua |
| Hero, mechanic, description và giao dịch | 2330 qua |
| Đèn/sương/phòng Windows thường | 134 qua, 16 ảnh |
| Đèn/sương/phòng Windows giảm hiệu ứng/dễ nhìn | 134 qua, 16 ảnh |
| Hồi quy cảnh shop — thường/giảm hiệu ứng | 71 + 71 qua, 8 ảnh |
| Hồi quy truyện, journal và hai ending — thường/giảm hiệu ứng | 58 + 58 qua, 42 ảnh |
| Tổng | **5142 qua, 82 ảnh**, không có dòng FAIL |

[Lượt chạy mới](Capture_Runs.json) và [lượt hồi quy](../MienCho/Capture_Runs.json) ghi thời gian/exit code. Verifier kiểm tra các lượt chạy và ảnh đều mới hơn bản build; hash player, runtime DLL, dữ liệu và source nằm trong Verification.json. Có thể chạy lại bằng `Tools/Story/Capture_ActOne_All.ps1`, sau đó `python Tools/Story/verify_act_one.py`.

## Ảnh thật từ Player

- [Wave 1: các đèn còn sáng](Native/lamps-and-fog-wave-1.png).
- [Wave 9: vùng sáng bị chia cắt](Native/lamps-and-fog-wave-9.png).
- [Wave 19: đèn vỡ và ánh sáng đoàn](Native/lamps-and-fog-wave-19.png).
- [Đêm sâu solo](Native/deep-night-solo.png), [đội 15](Native/deep-night-fifteen.png), [boss](Native/customer-in-deep-fog.png).
- [Chế độ dễ nhìn/giảm hiệu ứng](Reduced/lamps-and-fog-wave-19.png).
- [Shop không bị đèn che cửa/quầy](../MienCho/Encounters/Native/encounter-4-world.png).
- [Nhà ga](Native/room-8-world.png), [hành lang](Native/room-14-world.png), [căn phòng](Native/room-16-world.png).
- [Lựa chọn companion và hậu quả đã lưu](Native/companion-decisions.png).
- [Bình minh thật với đội hình hiện tại](../MienCho/Native/ending-CloseShop-3.png), [ngọn đèn để lại](../MienCho/Native/ending-LeaveLight-4.png).

Kiểm tra GPU xác nhận texture sương có mật độ thay đổi theo không gian, biến đổi trong cảnh đang chạy và giữ nguyên khi pause. Đếm đúng đèn nguyên/nứt/vỡ theo wave; số Point Light có giới hạn. Kiểm tra solo và 15 companion, boss, tiếp cận và windup của địch; projectiles/HUD không bị sương che. Cảnh shop, nhật ký và hai farewell đều chạy lại trên cùng bản build.

Ba phòng được vào bằng luồng thật từ shop, lưu trước khi chuyển scene, đi tới điểm chứng cứ mới Examine, chọn phản hồi, tải lại scene từ save rồi trở lại shop. Vàng và quân số giữ nguyên. Sáu tuyến companion lưu lựa chọn và chuyển chúng sang văn bản hậu quả/epilogue. Save cũ không có dữ liệu mới vẫn được tải. Chưa nghiệm thu tay cầm vật lý hoặc giả lập lỗi I/O.

Trong fixture 15 companion có sương, 120 frame trung bình **17.22 ms thường / 17.36 ms reduced**. Số này bao gồm cap/VSync và tải máy, fixture giới hạn spawn; không phải benchmark mọi trận đông hoặc bằng chứng cân bằng toàn chiến dịch.

## Phạm vi hình ảnh

Đèn là mesh Blender có Point Light/soft shadow Unity. Nền sprite dùng field ánh sáng và bóng che gần đúng; sương tích phân tám mẫu ở 320×240, giảm còn bốn mẫu khi giảm hiệu ứng. Không phải toàn map PBR 3D, ray tracing hoặc mô phỏng chất lưu. Phòng khám phá là không gian đi lại bằng sprite 2D với nền vẽ; không gọi đây là phòng 3D đầy đủ. Chỉ hồi 1 được triển khai; hồi 2–3 và boss thực thể để sau.

[Chi tiết triển khai](../../Docs/Act_One_Lanterns_And_Black_Fog.md), [nguồn art](../../Docs/Act_One_Art_Provenance.json), [prompt hình ảnh](../../Docs/Act_One_Image_Prompts.json). Không lấy gate animation của đợt khác làm nghiệm thu cho phần này.
