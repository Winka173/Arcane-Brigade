# Art Hồi 1 — tranh mới và tích hợp runtime

Cập nhật 17/10/2026. Đã giao **237 PNG**, gồm **236 ảnh tạo mới bằng painting pass** và một mặt trống Customer giữ từ art đã duyệt. Bộ này bám hai ảnh tham chiếu `furniture-00_v1.png` / `NightMarket_v1.png`, không dùng Blender. Prompt, nguồn gốc, kích thước và SHA-256 của từng ảnh nằm trong [manifest](Act_One_Story_Image_Prompts.json); kế hoạch đủ 261 ảnh ở [worklist](Art_Master_Worklist.json).

**Chưa hoàn tất toàn bộ:** painted trả `usage_limit_reached`. Còn **24 frame HD bổ sung của hai mặt Customer**, cùng **hai atlas Hob/Tally cần sửa mép ngoài**. Mốc mở lại được công cụ trả về: **02:02 ngày 18/10/2026, giờ Việt Nam**. Danh sách đường dẫn và prompt sửa đã lưu ở [Pending_Deliveries.json](../PreviewExports/ArtMaster/Pending_Deliveries.json). Không tính các mục này là đã nghiệm thu.

## Đã đưa vào game

- Ghế gỗ ấm và ba biến thể; năm lữ khách ngồi phủ bụi, một lần quay đầu; bốn pose người bán hàng hiền và mệt mỏi.
- Hob, Latch, Postscript và Tally có art riêng cho slot truyện. Postscript là sinh vật phong thư. Đồng minh mua cùng loài vẫn giữ hình của catalog.
- Chair Porter, Door That Stayed Shut và The Bedside có **16 ảnh độ phân giải riêng mỗi boss**. The Bedside được sửa để nhận ra chiếc ghế gỗ ấm, thay hình ngai bọc nệm không đúng nguồn gốc.
- Customer có hai bộ **16 pose atlas** và mặt trống cũ sau thất bại. Sáu frame HD bổ sung đã giao nhưng chưa ghép một bộ thiếu vào runtime; hai bộ atlas hoàn chỉnh tiếp tục được dùng cho đến khi đủ HD.
- Năm sàn theo vùng/trận ẩn, 12 đồ cảnh, năm vật phẩm chứng cứ, bảy biểu tượng ký ức, sáu tranh journal, menu ghế quay và tranh Hob. Đồ cảnh neo tọa độ thế giới, theo hệ lơ lửng hiện hành; không bám camera. Cảnh kéo Hob dùng nhân vật sống đặt lên ghế, tránh vẽ chồng Hob đã nằm trong tranh nền.
- **11 token hero**; **102 PNG VFX trong 62 nhóm**: đạn phe ta/thù địch, đột biến, chớp nòng, nhận đòn, hazard, trường tác động và vụ nổ. Đạn quái đỏ ember; lửa xanh giữ lõi xanh nhưng có viền đỏ rõ. Hazard có tranh báo trước và tác động, khớp bán kính/làn gây sát thương thật.
- Đèn bùng sáng và chuông nhặt trong trận có hình riêng. Trường trà dùng vệt trà hổ phách `Form_TeaSpill`, không dùng bát đĩa xanh.

Importer tạo hai thư viện `ActOneArtwork` / `CombatVfxArtwork`, đăng ký biên mực thật để căn chân, tâm VFX và kích thước. Ba boss lớn dùng ảnh HD riêng thay các ô atlas nhỏ bị mờ khi phóng lớn. Người bán hàng không bị clip mua hàng cũ đổi trở lại hình quái nhiều mắt. Tên boss dài có khung HUD rộng theo chữ.

Giữ các asset được ghi GIỮ trong master list. Nổ chết quái thường thuộc đợt golden explosion đang được cập nhật riêng; không ép quay lại mảnh gốm. Bộ chip gốm vẫn được giao theo yêu cầu art nhưng không được chọn vào hiệu ứng nổ runtime hiện hành.

## Kiểm tra và giới hạn

Unity 6000.4.8f1: **208 kiểm tra art**, 399 Miền Chờ, 512 Hồi 1 và 2702 hero identity qua. Windows build **Succeeded, 0 lỗi, 20 cảnh báo** (chủ yếu API tìm object đã bị đánh dấu cũ, cùng cảnh báo có sẵn); [Build.txt](../PreviewExports/ArtMaster/Build.txt). Bản chạy: `Builds/ArtMaster/ArcaneBrigade.exe`.

Chạy Windows D3D11 ở 1920×1080 trên RTX 5070 Ti: cả thường và giảm chuyển động/hiệu ứng/chớp đều **77 kiểm tra qua, 0 lỗi, 36 ảnh mỗi chế độ**. [Kết quả thường](../PreviewExports/ArtMaster/Native/Run.json), [giảm hiệu ứng](../PreviewExports/ArtMaster/Reduced/Run.json), [tổng hợp](../PreviewExports/ArtMaster/Verification.json). DLL chạy ở cả hai có SHA-256 `33FF56C712B288F25638642C84DA5558F68B53C141FCB4AAF8CDFC5CD0AE38C8`.

Đã xem ảnh nguồn toàn bộ qua tám contact sheet và các ảnh chạy thật trọng tâm: [boss cửa](../PreviewExports/ArtMaster/Native/Boss-15.png), [hazard tác động](../PreviewExports/ArtMaster/Native/Painted-hazards-active.png), [đồ cảnh vùng sâu](../PreviewExports/ArtMaster/Native/Zone-props-17.png), [người bán hàng](../PreviewExports/ArtMaster/Native/Shop-kind-eyes.png), [Hob ngồi](../PreviewExports/ArtMaster/Native/Hob-sitting.png) và gallery pose. Ảnh preview cũ được chuyển ra thư mục tạm; không dùng ảnh render đen của lần thử D3D12 làm bằng chứng. Chưa đo frame time riêng của đợt art này.

[Asset_Audit.json](../PreviewExports/ArtMaster/Asset_Audit.json) giữ nguyên danh sách 24 ảnh còn thiếu; chế độ kiểm tra phần hiện có chỉ chấp nhận đúng danh sách chờ đã ghi, vẫn báo lỗi với ảnh thiếu khác, hash sai hoặc alpha không hợp lệ. Cờ `complete` còn là false.

`ArtMasterChecks.VerifyAndBuild` vẫn yêu cầu đủ HD cho cả hai mặt Customer. Bản có thể chơi hiện tại dùng `VerifyAvailableAndBuild`, kiểm tra riêng ba boss HD và hai atlas Customer; báo phạm vi này trong Build.txt. Capture Windows dùng `-art-available` để ghi đúng phạm vi, không coi atlas Customer là ảnh HD.

Hai atlas Hob/Tally chạm mép ở cup/wing ngoài cùng: đã kiểm tra bằng ảnh nguồn, giữ trong danh sách sửa. Các cảnh báo đường biên ô ở atlas boss là lịch sử của bản sheet; ba boss hiện dùng PNG riêng. Importer cắt theo cụm mực cho atlas companion/Customer để không cắt tay ở đường lưới toán học. Điều đó không sửa được nét đã chạm mép ngoài của ảnh nguồn.

Các kiểm tra xác nhận liên kết dữ liệu, hiển thị và trạng thái đã chụp; không chứng minh mọi bố cục hoặc chuyển động đều đẹp ở mọi tình huống. Phạm vi chỉ là Hồi 1; không triển khai Hồi 2/3.

Khi quota mở lại: dùng painting pass cho đúng 24 đường dẫn thiếu và hai prompt sửa trong Pending_Deliveries; lưu bằng `store_master_art.py` để cập nhật hash/nguồn. Sau đó chạy audit không có `--available`, `ArtMasterChecks.VerifyAndBuild` và capture không có `-Available`. Chỉ xóa trạng thái chờ khi ảnh mới, bộ HD và mép atlas đã được kiểm tra thật. Không tạo lại 237 PNG đã giao.
