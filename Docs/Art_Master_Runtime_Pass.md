# Art Hồi 1 — tranh mới và tích hợp runtime

Cập nhật 17/10/2026: đã giao đủ **261/261 PNG**, gồm **260 ảnh mới hiện hành từ painting pass** và một mặt trống Customer giữ từ art đã duyệt. Bộ này bám hai ảnh tham chiếu `furniture-00_v1.png` / `NightMarket_v1.png`, không dùng Blender. Prompt, nguồn gốc, kích thước và SHA-256 của từng ảnh nằm trong [manifest](Act_One_Story_Image_Prompts.json); danh sách đủ ở [worklist](Art_Master_Worklist.json).

Đã thử lại công cụ thành công theo yêu cầu: vẽ đủ **24 frame HD Customer còn thiếu**, sửa **hai atlas Hob/Tally** để toàn bộ cốc/cánh nằm trong ô và có khoảng trống ngoài. Đã rà và bổ sung cuộn biên nhận vào hai pose Pleasant bị mất vật cầm. [Danh sách chờ](../PreviewExports/ArtMaster/Pending_Deliveries.json) hiện không còn mục thiếu hoặc cần sửa; không dùng fallback atlas nhỏ cho Customer nữa.

## Đã đưa vào game

- Ghế gỗ ấm và ba biến thể; năm lữ khách ngồi phủ bụi, một lần quay đầu; bốn pose người bán hàng hiền và mệt mỏi.
- Hob, Latch, Postscript và Tally có art riêng cho slot truyện. Postscript là sinh vật phong thư. Đồng minh mua cùng loài vẫn giữ hình của catalog.
- Chair Porter, Door That Stayed Shut và The Bedside có **16 ảnh độ phân giải riêng mỗi boss**. The Bedside được sửa để nhận ra chiếc ghế gỗ ấm, thay hình ngai bọc nệm không đúng nguồn gốc.
- Customer có **hai bộ 16 pose HD riêng**, Pleasant và BorrowedFace, cùng mặt trống cũ sau thất bại. Tổng ba boss và hai mặt Customer là **80 frame độ phân giải riêng**, mỗi bộ giữ 16 texture độc lập; atlas cũ giữ làm nguồn tham khảo.
- Năm sàn theo vùng/trận ẩn, 12 đồ cảnh, năm vật phẩm chứng cứ, bảy biểu tượng ký ức, sáu tranh journal, menu ghế quay và tranh Hob. Đồ cảnh neo tọa độ thế giới, theo hệ lơ lửng hiện hành; không bám camera. Cảnh kéo Hob dùng nhân vật sống đặt lên ghế, tránh vẽ chồng Hob đã nằm trong tranh nền.
- **11 token hero**; **102 PNG VFX trong 62 nhóm**: đạn phe ta/thù địch, đột biến, chớp nòng, nhận đòn, hazard, trường tác động và vụ nổ. Đạn quái đỏ ember; lửa xanh giữ lõi xanh nhưng có viền đỏ rõ. Hazard có tranh báo trước và tác động, khớp bán kính/làn gây sát thương thật.
- Đèn bùng sáng và chuông nhặt trong trận có hình riêng. Vệt trà hổ phách `Form_TeaSpill` được giao vào thư viện. Các vùng mechanic phe ta giữ cách vẽ hình học trên sàn theo đợt sửa mới nhất, token nhỏ dùng art; không khôi phục các tranh phủ kín vùng đánh.

Importer tạo hai thư viện `ActOneArtwork` / `CombatVfxArtwork`, đăng ký biên mực thật để căn chân, tâm VFX và kích thước. Cả năm bộ boss dùng ảnh HD riêng thay các ô atlas nhỏ bị mờ khi phóng lớn. Người bán hàng không bị clip mua hàng cũ đổi trở lại hình quái nhiều mắt. Tên boss dài có khung HUD rộng theo chữ.

Giữ các asset được ghi GIỮ trong master list. Nổ chết quái thường thuộc đợt golden explosion đang được cập nhật riêng; không ép quay lại mảnh gốm. Bộ chip gốm vẫn được giao theo yêu cầu art nhưng không được chọn vào hiệu ứng nổ runtime hiện hành.

## Kiểm tra và giới hạn

Unity 6000.4.8f1: **216 kiểm tra art**, 399 Miền Chờ, 512 Hồi 1 và 2702 hero identity qua. Windows build **Succeeded, 0 lỗi, 19 cảnh báo** (chủ yếu API tìm object đã bị đánh dấu cũ, cùng cảnh báo có sẵn); [Build.txt](../PreviewExports/ArtMaster/Build.txt). Bản chạy: `Builds/ArtMaster/ArcaneBrigade.exe`.

Chạy lại Windows D3D11 sau khi đủ HD, ở 1920×1080 trên RTX 5070 Ti: cả thường và giảm chuyển động/hiệu ứng/chớp đều **77 kiểm tra qua, 0 lỗi, 36 ảnh mỗi chế độ**. [Kết quả thường](../PreviewExports/ArtMaster/Native/Run.json), [giảm hiệu ứng](../PreviewExports/ArtMaster/Reduced/Run.json), [tổng hợp](../PreviewExports/ArtMaster/Verification.json). DLL chạy ở cả hai có SHA-256 `9FD91863643D1BA42BDF29DC2C4CD854AFF76060476CF4966A00D81C0D684812`.

Đã xem ảnh nguồn toàn bộ qua chín contact sheet và các ảnh chạy thật trọng tâm: [Customer HD](../PreviewExports/ArtMaster/Native/Customer-second-mask.png), [gallery pose Customer](../PreviewExports/ArtMaster/Native/Gallery-09.png), [boss cửa](../PreviewExports/ArtMaster/Native/Boss-15.png), [hazard tác động](../PreviewExports/ArtMaster/Native/Painted-hazards-active.png), [đồ cảnh vùng sâu](../PreviewExports/ArtMaster/Native/Zone-props-17.png), [người bán hàng](../PreviewExports/ArtMaster/Native/Shop-kind-eyes.png), [Hob ngồi](../PreviewExports/ArtMaster/Native/Hob-sitting.png) và gallery pose. Ảnh preview cũ được chuyển ra thư mục tạm; không dùng ảnh render đen của lần thử D3D12 làm bằng chứng. Chưa đo frame time riêng của đợt art này.

[Asset_Audit.json](../PreviewExports/ArtMaster/Asset_Audit.json): đủ 261 ảnh, đúng hash/nguồn, có alpha khi yêu cầu; không còn lỗi thiếu ảnh. Cờ `complete` là true. Kiểm tra riêng hai atlas đã sửa không chạm mép ngoài hoặc đường chia ô.

Chạy `ArtMasterChecks.VerifyAndBuild` với đầy đủ yêu cầu HD; capture không dùng `-art-available`. Kiểm tra bổ sung xác nhận bốn companion có đủ bốn pose riêng và tư thế chuẩn bị đánh khác tư thế ra đòn.

Còn 19 cảnh báo để xem hình: 16 cảnh báo đường chia ô của năm atlas boss cũ không còn dùng cho animation, và ba cảnh báo nét sát biên ngoài ở ảnh mặt trống gốc, BorrowedFace gốc và một pose BorrowedFace. Đã xem ảnh: hình thể vẫn đầy đủ; không coi các cảnh báo này là bằng chứng mọi asset có đúng 12% padding. Importer căn chân theo biên mực thật; các frame HD không bị cắt theo ô atlas.

Các kiểm tra xác nhận liên kết dữ liệu, hiển thị và trạng thái đã chụp; không chứng minh mọi bố cục hoặc chuyển động đều đẹp ở mọi tình huống. Phạm vi chỉ là Hồi 1; không triển khai Hồi 2/3.

Nguồn ảnh nằm trong `Assets/ArcaneBrigade/Resources/MienCho/ActOne/`, `Assets/ArcaneBrigade/Art/ActOneBosses/`, `Assets/ArcaneBrigade/Art/CombatVFX/` và `Assets/ArcaneBrigade/Art/MechanicEffects/Tokens/`; hai ảnh menu/phòng nằm tại đường dẫn riêng ghi trong master list. Mọi bản cuối đều được chép vào workspace, không chỉ giữ trong thư mục ảnh sinh mặc định.
