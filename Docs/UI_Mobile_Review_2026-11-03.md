# Rà soát UI/UX cho điện thoại — 03/11/2026

Mục tiêu: phát hành trên điện thoại trước. Rà lại toàn bộ màn hình theo hai bộ tiêu chí *frontend-ui-engineering* (vùng chạm, cỡ chữ, trạng thái, vùng tai thỏ) và *design-taste-frontend* (thứ bậc, mật độ, nút tự giải thích), rồi sửa những gì chặn việc chơi trên điện thoại.

## Cách kiểm tra

| Bước | Kết quả |
| --- | --- |
| Bộ chụp 61 màn trong trận ở 5 hình dạng màn: 2400×1080 (20:9), 1600×720, 2532×1170 (iPhone), 2048×1536 (iPad 4:3), 2560×1600 (16:10) | Bố cục giữ được ở mọi hình dạng: khung 16:9 nằm giữa, thế giới phủ kín hai bên, không có gì nằm dưới tai thỏ |
| Chế độ điện thoại trên PC (`-arcane-phone`): 61 màn trận và 23 màn menu với nút cảm ứng, cỡ và trang Settings của điện thoại | Đạt, không mục nào báo lỗi |
| Bộ kiểm tra PC chuẩn ở 2560×1440 | 115/115 |
| APK Android (Unity 6000.6, ARM64) trên máy ảo Pixel 7, Android 15 | Mở game, menu, chọn hero, cốt truyện, vào trận, joystick, nút Pause, thoát ra rồi vào lại: đều chạy |

Quy đổi: canvas 1920×1080 trên điện thoại 6,5" nằm ngang thì 1 px ≈ 0,063 mm. Cỡ 28 ≈ 11 pt (mức tối thiểu của iOS/Android), nút 64 px ≈ 4 mm (mức tối thiểu cho ngón tay là 7–9 mm).

## Đã sửa

1. **Vùng chạm (mọi màn):** mọi nút đi qua `TouchTargets`. Trên điện thoại, nút nhận lần chạm trong vòng 8 mm quanh hình vẽ; chạm trúng nút nào thì là nút đó, chạm giữa các nút thì nút gần nhất nhận. Hình vẽ không đổi. Trước đó phần lớn nút X, tab, ±10%, DETAILS, icon bond chỉ 2–4 mm.
2. **Nút Pause và nút Đội hình trong trận:** từ 64 lên 96 px; dòng gauge của hero lùi xuống dưới hai nút (trước đây hai nút đè lên chữ và vùng bấm của gauge).
3. **Bảng bond:** ô từ 64 lên 76 px trên điện thoại.
4. **Trang Pause:** nút từ 250×64 lên 310×76, chữ về cỡ chuẩn 28 (trước "HOW TO PLAY" và "START OVER" co còn khoảng 1,4 mm).
5. **Main menu:** 4 nút phụ rộng 290 (chữ co tới cỡ 15 khi rộng 230); các nút lớn rộng theo cho thẳng mép.
6. **Records:** nhãn "Best night / Wins / Foes / Difficulties" viết thường ở cỡ chuẩn thay cho chữ in hoa cỡ 15.
7. **Tag độ hiếm, tag sao trên thẻ shop và trong Đội hình:** cỡ 16–18 lên 20–22.
8. **Settings trên điện thoại:** chỉ còn Frame rate và Sharpness, bỏ Fullscreen/Resolution/Vsync vốn chỉ có nghĩa trên PC.
9. **Lỗi chỉ có trên Android — tấm nút vẽ tay bị dẹt, chữ rơi xuống mép dưới:** điện thoại nhập ảnh kit nhỏ hơn (128 px) nhưng code đo theo pixel của ảnh gốc. Thêm bảng kích thước gốc `ArcadeSkinKitSizes.cs` (sinh từ PNG) và quy mọi phép đo về pixel gốc. Trên PC hệ số là 1 nên không đổi gì.
10. **Bộ chụp menu (`-arcane-menu-review`)** chưa được cách ly khỏi save thật; đã thêm vào `RunPaths.Diagnostic`.

## Còn lại, xếp theo mức chặn phát hành

| Mức | Việc | Ghi chú |
| --- | --- | --- |
| Chặn | **Dung lượng APK 288 MB** (dữ liệu 258 MB) | Google Play giới hạn gói cơ bản 200 MB nén: cần Play Asset Delivery (chia asset thành gói tải sau) hoặc giảm tiếp art |
| Chặn | **Chưa chạy trên máy ARM thật** | Máy ảo x86 chạy mã ARM qua lớp dịch: crash trong thư viện điều nhịp khung hình Swappy và đứng ở Vulkan. Bản thử trên máy ảo tắt Swappy và ép OpenGL ES. Cần một điện thoại Android thật để xác nhận Swappy/Vulkan và đo FPS |
| Chặn | **Build phát hành cần Unity 6000.6** (bản có module Android); dự án đang mở bằng 6000.4.8 | Build Android hiện chạy trong bản sao `D:/ABMobile/android` |
| Cao | **RAM khoảng 540 MB** | Máy 2 GB RAM sẽ chật; máy tầm trung ổn |
| Vừa | Chữ cỡ 20–26 ở các thẻ phụ: Visitor (chip "next wave · first 20s" co tới 14), Secret Event (DO/LOSE/GET), giá vé trong shop (co tới 12), số damage xám | Đọc được nhưng nhỏ; nên đưa về 28 khi làm lại các thẻ đó |
| Vừa | Lamp Tree xem cả cây: nút nhánh khoảng 2 mm, đứng sát nhau | Vùng chạm mới giúp chạm trúng nút gần nhất, nhưng trên điện thoại nên mở thẳng từng nhánh (đang làm vậy khi có nhánh mua được) |
| Thấp | iPad 4:3: Waiting Room có dải tối trên và dưới (phòng phóng to và tối đi, có chủ đích) | Có thể vẽ phòng cao hơn nếu phát hành iPad |
| Thấp | Thẻ "a new page / TAP TO READ" cạnh HUD trong shop dùng chữ nhỏ | |

## Một bố cục hay hai?

Một bố cục chung vẫn dùng được cho cả hai vì game luôn nằm ngang và khung 16:9 giữ nguyên trên mọi máy. Phần khác nhau nằm ở công tắc `RunUIHints.Phone`:
- nút cảm ứng trong trận;
- ô bond lớn hơn;
- vùng chạm rộng;
- Settings của điện thoại;
- ẩn nút EXIT.

Màn nào cần tách hẳn bố cục điện thoại về sau (ví dụ cả cây Lamp Tree) thì đặt sau công tắc này.
