# Hồi 1 — đèn cổ, ánh sáng đồng hành và sương đen

**Bản hình ảnh đã được thay thế:** theo phản hồi mới nhất, 13 asset Miền Chờ đã được vẽ lại theo theme gốc và loại tài nguyên Blender khỏi Player. Mô tả mesh/Point Light trong báo cáo này là lịch sử; trạng thái hiện hành dùng đèn sprite, halo và field Unity: [Mien_Cho_Painted_Art_Pass.md](Mien_Cho_Painted_Art_Pass.md).

Phạm vi theo yêu cầu mới ngày 16/10/2026: hoàn thiện **hồi 1, chiến dịch 20 wave**. Hồi 2 và 3 để phát triển sau; bản này không có nút sang hồi sau hoặc boss Người Luôn Ở Đó. Last Customer và hai bình minh thật vẫn kết thúc hành trình hiện tại.

## Đèn và bóng tối

Blender MCP dựng đèn đường cổ bằng sắt uốn, đồng cũ, kính mật ong, mái chóp và ngọn lửa bên trong. Có ba trạng thái hình học riêng: **nguyên, nứt, vỡ**; xuất FBX để dùng trong Unity và ảnh alpha Cycles cho chế độ giảm hiệu ứng. Nguồn chỉnh sửa: [MienCho_AntiqueLanterns.blend](../Tools/Blender/MienCho_AntiqueLanterns.blend), [script dựng](../Tools/Blender/create_night_lanterns.py).

Tám đèn có điểm neo cố định trên các lối đi và rìa map; thân đèn nay **lơ lửng, nhấp nhô và nghiêng**, có bóng tách sàn. Theo phản hồi mới nhất đã bỏ khói/vệt gió dưới đồ vật. Halo/Point Light đi theo ngọn lửa, gồm cả khi nghiêng; đèn vỡ vẫn nổi nhưng không phát lửa. Không thêm collider làm vướng đường hoặc giá trị damage. Có đèn trong vùng camera trung tâm để người chơi thấy được quá trình mất ánh sáng khi quay lại cùng một vị trí. Đợt làm lại toàn hệ và bằng chứng mới: [Dream_Levitation_02.md](Dream_Levitation_02.md).

| Wave | Đèn vỡ trong combat | Cảm giác không gian |
| --- | --- | --- |
| 1–4 | 0/8 | Còn nhiều điểm sáng ấm; một số đèn bắt đầu nứt |
| 5–8 | 2/8 | Những khoảng tối đầu tiên xuất hiện |
| 9–12 | 4/8 | Các vùng sáng bị chia cắt |
| 13–16 | 6/8 | Đoàn trở thành nguồn sáng chủ yếu |
| 17+ | 8/8 | Khung đèn còn đó, lửa tắt; ánh sáng đoàn giữ đường đi |

Đèn nứt còn nửa cường độ, biến thiên rất nhẹ; Reduced Flashes bỏ biến thiên. Khi vỡ, vùng sáng rút trong một khoảng ngắn thay vì chớp mạnh. Mảnh kính vỡ và vỏ đèn không còn emission. Tăng độ tối theo wave, không theo thời gian người chơi đứng yên. Bình minh phục hồi ánh sáng môi trường; không giả rằng mọi đèn tự sửa lại.

Trong shop chỉ đặt hai đèn thực dụng ở mép ngoài để giữ quầy, cửa và hàng hóa dễ đọc. Chúng không dùng quota mua hàng. Các đèn đường khác được ẩn trong bố cục shop.

Hero và từng companion vẫn có nguồn sáng riêng trong field đội hình, tối đa hero + 15 bạn. Nguồn gần nhau hợp bằng giá trị lớn nhất, không cộng thành ban ngày. Người chơi solo vẫn có vùng nhìn đủ chiến đấu.

## Cách ánh sáng và sương hoạt động

- Mesh đèn sử dụng vật liệu Standard và tối đa **4 Point Light thật trong Unity**, ưu tiên các đèn còn sáng gần người chơi. Tối đa hai nguồn dùng soft shadow; giảm hiệu ứng bỏ shadow và dùng ảnh đèn Cycles. Mesh được gộp theo vật liệu để giảm draw call.
- Nền sprite hiện có nhận một texture ánh sáng **256×192** với độ rơi mềm và vùng che ánh sáng từ các vật cảnh đã biết. Đây là bóng che tính gần đúng cho nền 2D, không phải toàn bộ map chuyển sang vật liệu PBR/geometry 3D.
- Sương đen tích phân hấp thụ qua **8 lớp lấy mẫu**, noise ba chiều nhiều tần số, dòng trôi và ánh sáng tán xạ từ đèn/đoàn. Vùng quanh đèn vỡ tăng mật độ; ánh sáng đoàn làm loãng phần đặc nhất. Render ở **320×240**, lọc mềm khi phủ lên world để giữ chi phí ổn định. Reduced Effects còn bốn mẫu và mật độ thấp hơn.
- Sương nằm sau actor, đạn, telegraph và HUD. Thân địch xa vẫn tối theo hệ visibility; mắt hoặc ember gắn vào đúng địch đang tiến lại. Boss và đòn báo trước có giới hạn độ tối riêng.
- Pause giữ nguyên clock, texture sương và biến thiên đèn. Reduced Motion dừng dòng sương nhưng giữ ánh sáng thiết yếu; High Readability giảm mật độ và giữ silhouette.
- Thêm sáu clip PCM nguyên bản: gió đêm, tiếng kính đèn, bước chân và ba nền âm cho phòng khám phá. [Script âm thanh](../Tools/Story/create_night_audio.py). Đây là âm thanh tổng hợp, không phải voice acting hoặc bản thu nhạc cụ.

Giải pháp kết hợp mesh/đèn thật với ánh sáng và sương shader dành cho game sprite. Không mô tả đây là ray tracing, mô phỏng khói chất lưu hoặc volumetric GI đầy đủ. Unity MCP không được cung cấp trong phiên; phần Unity thực hiện bằng Editor cài trên máy và Player Windows D3D11. Blender được điều khiển qua MCP.

## Chiều sâu hồi 1 đi cùng đợt hình ảnh

Ba phòng khám phá riêng có thể đi lại bằng sprite 2D: **nhà ga** sau wave 8–9, **hành lang bước chân** sau 14–15 và **căn phòng hoàn hảo** sau 16–17. Mở từ **PATHS & PEOPLE / K / Y** trong shop; có thể rời bất cứ lúc nào. Mỗi phòng có ba điểm chứng cứ và hai cách phản hồi; lưu lựa chọn xuống save, không phát vàng/stat hoặc lấy mất companion. Bối cảnh vẽ từ bố cục Blender, không phải ba vùng thế giới 3D mới.

Sáu tuyến companion có ba bước, mở từ shop sau wave 4/10/16 khi bạn đó có trong đội. Hai phản hồi mỗi bước dẫn tới lời kể hậu quả riêng, xuất hiện trong nhật ký, phòng khám phá và lời kết. Các quyết định không thay quota, tốc độ lên sao hoặc quyền rời đi của nhân vật. Không yêu cầu hoàn thành quest để nhận ending.

Ba nền phòng được tạo bằng **skill painting**, chế độ công cụ tích hợp, dùng bố cục Blender và art của dự án làm tham khảo. Đã lưu trong `Assets/ArcaneBrigade/Resources/MienCho/Journey/*_Painted_v1.png`; prompt đầy đủ tại [Act_One_Image_Prompts.json](Act_One_Image_Prompts.json). Nguồn và SHA-256 của asset, Blender và script: [Act_One_Art_Provenance.json](Act_One_Art_Provenance.json).

## Kiểm tra

Bản Windows: `Builds/HeroIdentity/ArcaneBrigade.exe`. Gate cuối **5142 kiểm tra qua / 82 ảnh / 0 lỗi build / 0 cảnh báo**: [Gate_Summary.md](../PreviewExports/ActOne/Gate_Summary.md), [Verification.json](../PreviewExports/ActOne/Verification.json). Đã đối chiếu kiểm tra Editor, ảnh thường/giảm hiệu ứng, sự biến đổi texture sương trên GPU, pause, solo/15 bạn, shop, lưu/tải phòng và hai ending. Các ảnh và lượt chạy đều thuộc bản build được ghi hash trong báo cáo. Không dùng gate animation của đợt khác để thay bằng chứng cho ánh sáng mới.

Số đo 120 frame là fixture có giới hạn spawn, bao gồm frame cap và tải máy lúc chạy; không thay cho benchmark mọi cấu hình hoặc chơi cân bằng toàn hành trình. Chưa nghiệm thu tay cầm vật lý.
