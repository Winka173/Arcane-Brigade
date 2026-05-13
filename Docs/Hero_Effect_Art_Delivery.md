# Bàn giao art bộ phận hiệu ứng hero — 24/10/2026

Đã vẽ đủ yêu cầu trong [Hero_Effect_Art_Requests.md](Hero_Effect_Art_Requests.md) bằng **painted**, đối chiếu từng tranh Awakening đang có. Có **14 PNG / 40 frame**; hai lớp Petal Citadel và chùy/quả bóng Juggling Jester tách sheet riêng.

Tất cả nằm trong `Assets/ArcaneBrigade/Art/Playtest/HeroEffects/`. Mỗi ô **256×256**, một hàng trái sang phải, RGBA thật, bốn góc alpha 0, lề trong suốt ít nhất 30 px mỗi ô sau khi xuất. Tổng PNG **1.553.517 byte**, khoảng 1,55 MB. Kích thước gốc công cụ được chuẩn hóa bằng resize, căn ô và dọn alpha mờ; không dùng code vẽ thay tranh hoặc chroma key.

| File | Frame | Kích thước | Nội dung |
| --- | ---: | --- | --- |
| `TreasureAcorn_Crank.png` | 4 | 1024×256 | Tay quay lên/phải/xuống/trái, vỏ hạt rỗng bật ra ở frame 3. |
| `Postmaster_Barrels.png` | 3 | 768×256 | Cụm nòng giật lệch nhịp và hơi mực nhỏ tại miệng nòng. |
| `CopperCauldron_PistonValve.png` | 4 | 1024×256 | Piston vào/ra; van mở ở frame 2–3. |
| `CitrusMarshal_Coupling.png` | 3 | 768×256 | Móc nối chùng, căng, lệch theo cua. |
| `MirrorKeeper_Pane.png` | 3 | 768×256 | Kính thường, lõm khi bắt đạn, lóe bạc khi trả. |
| `PetalCitadel_WallFront.png` | 3 | 768×256 | Lớp cánh thấp phía trước: đóng, hé khe, mở. |
| `PetalCitadel_WallBack.png` | 3 | 768×256 | Lớp cánh cao phía sau, dùng chung mốc và tỉ lệ với lớp trước. |
| `JugglingJester_Pin.png` | 2 | 512×256 | Một chùy hai hướng khi lộn đầu. |
| `JugglingJester_Ball.png` | 2 | 512×256 | Bóng tròn và bóng nén khi đáp, chung mốc đáy. |
| `KiteAdmiral_FleetKite.png` | 3 | 768×256 | Một diều hộp: thăng bằng, nghiêng trái, nghiêng phải. |
| `BellDiver_DivingBell.png` | 3 | 768×256 | Chuông thường, ép mặt trước khi húc, thoát bọt tại vành. |
| `BonsaiClocksmith_GearBranch.png` | 2 | 512×256 | Cành cố định, bánh răng xoay nửa răng. |
| `SupperMortar_PotLid.png` | 2 | 512×256 | Nắp yên trên bệ cổ nồi; nắp rung mở hé và thoát hơi. |
| `HourglassStag_Hourglass.png` | 3 | 768×256 | Thẳng, nghiêng, lật ngược; không vẽ cát bên trong. |

## Kiểm tra và chuẩn bị cho Unity

- [Bảng ảnh xem nhanh](../PreviewExports/HeroEffects/ContactSheet.jpg) được ghép để review, không dùng làm texture trong game. Chữ tên file chỉ xuất hiện trên bảng review; các PNG sprite không có chữ.
- [Manifest](Hero_Effect_Art_Manifest.json) lưu prompt, ảnh tham chiếu, nguồn do painted tạo, thứ tự frame, mốc căn và đường dẫn cuối. Treasure Acorn có cả prompt chỉnh tay quay, vì bản đầu có hai tư thế bị trùng.
- [PNG verification](Hero_Effect_Art_Verification.json) lưu mode, kích thước, alpha góc, bounding box từng ô và dung lượng.
- Unity đã nhập **14 sheet và tạo đúng 40 sprite**, tên `<tên sheet>_f0`, `_f1`… Mỗi rect 256×256, pivot giữa, pixels-per-unit 256; không mipmap, không ép kích thước NPOT, bilinear/clamp và max texture 1024. [Kết quả](Hero_Effect_Art_Unity_Verification.txt).
- Có thể nhập lại bằng menu **Arcane Brigade → Art → Import hero effect sheets**, hoặc batch method `ArcaneBrigade.Editor.HeroEffectArtImporter.Run`. Importer chỉ xử lý 14 sheet trong manifest.
- Script `Tools/Art/prepare_hero_effect_sheets.py` xử lý bố trí/alpha từ ảnh nguồn. Nếu công cụ đặt ô lệch, nó tìm khoảng trống thật giữa các sprite để tránh cắt bộ phận. Những pose cùng animation dùng một tỉ lệ chung và mốc bộ phận cố định. Hai lớp Petal Citadel dùng chung nhóm căn.

Phạm vi bàn giao là **tranh và sprite đã cắt**, sẵn sàng cho code gắn vào hiệu ứng. Chưa thay token runtime, chưa dựng clip animation/đo frame time trong combat và chưa build Windows mới. Ánh sáng, hơi nước, điện, mưa, sonar và các hiệu ứng đã vẽ bằng code tiếp tục thuộc hệ runtime; bộ art này cung cấp phần vật thể theo yêu cầu.
