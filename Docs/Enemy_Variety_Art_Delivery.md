# Bàn giao ba bộ art — 24/10/2026

Đủ các file trong cả ba yêu cầu. Đợt này vẽ mới **22 sheet quái / 352 frame**, đổi **Pillow Oracle** sang tông ấm, và kiểm tra lại sự hiện diện, RGBA, kích thước và alpha góc của hai bộ đã giao trước đó.

| Yêu cầu | Kết quả |
| --- | --- |
| [23_Enemy_Variety.txt](Playtest_Art_Prompts/23_Enemy_Variety.txt) | 22/22 PNG: 10 loài vẽ lại, 8 loài mới, 4 tinh anh; mỗi sheet 4×4, 1536×1536, 384 px/ô |
| [Art_Redo_Requests.md](Art_Redo_Requests.md) | Đã có đủ 77 món chính: 54 sheet nhân vật và 23 vũ khí; thêm 10 sheet acting hero đã giao. Pillow Oracle đã đổi màu ở commit `b1f1f50f` |
| [Hero_Effect_Art_Requests_2.md](Hero_Effect_Art_Requests_2.md) | Đã có đủ 24 sheet / 78 frame, mỗi ô 256×256; đã nhập Unity trong đợt trước |

Các bộ đã hoàn tất được giữ lại; đợt này không vẽ lại 10 hero hoặc 24 vật thể hiệu ứng lần nữa. [Bàn giao art redo](Art_Redo_Delivery.md), [bàn giao hiệu ứng hero phần 2](Hero_Effect_Art_Delivery_2.md).

## Bộ quái mới

Tất cả lưu đúng tên `Enemy_<Id>.png` trong `Assets/ArcaneBrigade/Art/Playtest/MapEnemies/`.

| Nhóm | ID |
| --- | --- |
| Vẽ lại | MoonMagnet, ClockBat, RuneWisp, InkMoth, SirenSeal, SleepMimic, SporeBishop, BroomSkater, CurtainWisp, HushMite |
| Loài mới | FireScreenWarden, TwinWickLantern, CandleShade, SpyglassSniper, DrawerBroodmother, LedgerTortoise, BoundBookends, RubberStampTitan |
| Tinh anh | HeadUsher, DeadLetterCourier, WickWarden, ArchiveSentinel |

Hai hàng trên là tám tư thế dạng thường, hai hàng dưới là tám tư thế dạng biến dị. Đã duyệt thay đổi cấu trúc cơ thể giữa hai dạng: thêm cực nam châm, bánh răng/cánh, vòng rune, loa, miệng, nhánh móc, bấc, đầu, ngăn kéo, giáp, mắt kính hoặc tay tùy loài.

Các bản đã sửa khi duyệt:

- **ClockBat:** bỏ số lịch trên cánh giấy; giữ mặt đồng hồ không số và căn lại lề.
- **SirenSeal:** dạng biến dị có hai loa gramophone riêng trên lưng, bỏ sừng động vật do bản đầu hiểu nhầm.
- **CandleShade:** vẽ lại thành bóng nến dài, đầu sáp và mắt khe; dạng biến dị hai đầu. Bỏ bản thân lông tròn giống HushMite. Căn riêng 16 cơ thể độc lập để giữ nguyên các ngón tay dài khi chúng vượt ranh hàng danh nghĩa nhưng không chạm nhau.
- **DrawerBroodmother:** vẽ lại thành tủ thép thấp, rộng, có bánh xe và ấu trùng kẹp giấy; bỏ bản giống tủ gỗ CabinetMimic.

## Màu tím từ quái cũ

Đã xem atlas quái cũ, gồm `Art/K22/HushMite_v1.png`, `Art/K22/ClockBat_v1.png` và bảng `PreviewExports/ArtAudit/enemy_hostile_shift.jpg`; lấy mẫu RGB trực tiếp từ pixel tím đục của HushMite:

`#12111A` · `#1A192A` · `#202032` · `#2A294A` · `#313153` · `#453C6E` · `#615697`

Đây là dải bóng, thân và vùng sáng tham chiếu. Kim loại, giấy, sáp, vải và gỗ trong ảnh mới đều được xử lý bằng tím/chàm/tím mận, với đỏ ở mắt và điểm nhấn nhỏ. Đã đối chiếu thêm `HostileCast` trong `UnitVisual` (`#B89EFF`) và màu bóng/viền trong `SpriteLighting`; các giá trị shader này được ghi riêng trong manifest, không thay bằng một lớp phủ màu đồng nhất lên tranh.

## Bảng ảnh duyệt và bằng chứng

- [10 loài vẽ lại](../PreviewExports/EnemyVariety/Repaints.jpg).
- [8 loài mới](../PreviewExports/EnemyVariety/NewEnemies.jpg).
- [4 tinh anh](../PreviewExports/EnemyVariety/Elites.jpg).
- [Prompt, reference, crop màu và nguồn ảnh](Enemy_Variety_Art_Manifest.json).
- [Kiểm tra PNG, bounds từng ô, SHA-256](Enemy_Variety_Art_Verification.json).
- [Tiến độ 22/22](Enemy_Variety_Art_Progress.txt).

Đọc lại toàn bộ 22 PNG cuối: **RGBA thật, 1536×1536, 16 ô có hình, cả bốn cạnh canvas trong suốt hoàn toàn**, lề mỗi ô tối thiểu **48 px**. Tổng PNG **32.176.023 byte**, khoảng **30,69 MiB**. Không còn job thất bại hoặc cần sửa trong báo cáo.

Art được tạo bằng **painted**. Xử lý sau vẽ chỉ gồm căn ô, resize/padding và dọn alpha vụn theo quyền đã được chấp thuận; không dùng code để vẽ thay các loài quái. `Tools/Art/prepare_enemy_variety.py` ghi kiểm tra theo từng món; `--review-only` xuất bảng ảnh duyệt. Reference trong `Library/EnemyVarietyRefs/` là dữ liệu tạm: nguồn crop và tọa độ hoặc mã màu đã được lưu trong manifest.

## Nhập vào game

Mỗi PNG quái và Pillow Oracle có commit riêng; art đã được đẩy lên `origin/main` theo từng đợt giao. Bên code chạy các menu đã nêu:

1. **Arcane Brigade → Art → Put enemy variety in** cho 22 sheet mới.
2. **Arcane Brigade → Art → Put repainted characters and weapons in** để nhận màu Pillow Oracle mới.
3. Khi cần dựng lại thư viện hiệu ứng: **Import hero effect sheets**, rồi **Build hero effect art library**.

Đợt này bàn giao ảnh và kiểm tra file; không chạy lại các menu nhập, không nghiệm thu combat runtime hoặc tạo bản Windows mới. Các bảng duyệt là ảnh ghép từ PNG. Bằng chứng nhập Unity của hai bộ cũ nằm trong các báo cáo bàn giao tương ứng.
