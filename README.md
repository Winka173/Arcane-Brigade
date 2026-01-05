# Arcane Brigade

**K4 DONE — functional gate Windows:** hai build Máy thu hoạch/Văn phòng hoàn trả và Carbon Ribbon, contribution đo HP thật, merge reveal sau save, tier/idle/reduced motion, Chuông sân khấu/Xe đồ chơi và boss phase đổi sân, sandbox chỉ weapon đã khám phá. Regression + native combat→loot→shop/merge→boss/rest→sandbox PASS. Báo cáo: `Docs/K4_Final_Checkpoint.md`; ảnh Unity4K: `PreviewExports/K4/Final/Player/index.html`. Menu build/check: `Arcane Brigade > K4 > Apply full signature slice check and build`. K5 tiếp theo, font/UI K6, production/mobile K7; DONE không phải nghiệm thu release.

Checkpoint K4.1: Ô bung đẩy enemy phía trước, Chuông tích nhịp phát sóng trễ, **DreamStamp** đánh một mục tiêu/tích dấu/burst; tier ≥2 có twist hữu hạn riêng. Thêm weapon vào pool tuyển ngẫu nhiên, không cho chọn/đổi weapon. 40 checks + regression PASS, native Windows capture: `PreviewExports/K4/SignaturePlayer/index.html`. Báo cáo lịch sử: `Docs/K4_Signature_Checkpoint.md`; bản tích hợp mới dùng menu full signature slice ở trên.

Visual pass theo **hướng 1**: model/vũ khí HD, nền map lát tile, zoom combat +38% / shop +25%, màu rõ hơn, hiệu ứng viền mềm và bảng giá dễ đọc. Ảnh Unity **3840×2160**: `PreviewExports/VisualQuality/Player4K/index.html`; báo cáo và giới hạn: `Docs/Visual_Quality_4K_Pass.md`. PNG nguồn 1672×941, không gọi là native 4K. Dùng menu `Arcane Brigade > Visual Quality > Apply HD assets map and check build`; không dựng lại baseline. K4 chưa triển khai trong visual pass này.

K3 hoàn tất theo gate Windows đã duyệt: proc/cause, chest/egg/heart/Luck/Detection, offscreen chest/egg indicators, item delta preview, recipe preview/altar reroll. Chủ dự án chuyển device gate sang K7. Báo cáo: `Docs/K3_Final_Checkpoint.md`; build: `Arcane Brigade > K3 > Check final checkpoint and build`. Motion native: `PreviewExports/K3/Final/Player/index.html`. Mobile/parity chưa nghiệm thu, không gọi candidate là production.

K3.2b mới: altar mua bằng HP hiện tại hoặc eggs, activation chance theo eggs, giữ save/rollback; báo cáo `Docs/K3_Altar_Economy.md`. Giá/chance là tuning riêng chưa xác nhận công thức reference. Clone source cần `git lfs pull`; mở bằng Unity 6000.4.8f1, dùng `Arcane Brigade > K3 > Check altar economy and build`. Build/cache/save diagnostic không commit vào Git.

Plan triển khai hiện hành: `Docs/Implementation_Roadmap.md` (K0–K7). K0–K2 có baseline, camera/map, main menu/Details và death/coin/pose runtime; báo cáo `Docs/K0_K2_Implementation.md`. Chơi bản mới tại `Builds/FullRun/ArcaneBrigade.exe`; giữ thư mục đi kèm. Ảnh Unity: `PreviewExports/K1/Player`; motion/capture K2: `PreviewExports/K2/Player`. N16–N18 vẫn chờ pipeline K3/K4/K6, không demo bằng dữ liệu giả.

K0–K2 đã recheck; đang làm K3 theo `Docs/K3_Implementation_Plan.md`. K3.1 thêm giao dịch atomic, biên lai sau save và lịch sử trong Details/Vật phẩm; nhận save schema 0/1 lên schema 2. Dùng menu `Arcane Brigade > K3 > Check transactions and build`, không dựng lại baseline. Giá/chance altar HP, proc và loot mở rộng vẫn chưa hoàn tất.

K3.2a tiếp tục modifier đa hiệu ứng/scope, rarity/weighted item pools, reroll 4/6/8… và tab Details/LIÊN KẾT từ nguồn thật. Echo Postage/Long Atlas có trade-off hiện rõ trong tooltip và receipt. Số cân bằng là tuning riêng, không tự nhận copy đúng công thức reference. Kiểm tra/build: `Arcane Brigade > K3 > Check modifier economy and build`. Báo cáo: `Docs/K3_Modifier_Economy.md`; ảnh Unity mới: `PreviewExports/K3/ModifierEconomy/Player/`. Altar HP/chance, proc và loot mở rộng vẫn chờ phần tiếp theo.

Research theo **Bizarre Brigade 1.5+**: `Docs/Bizarre_Brigade_15_Reference_Audit.md` (57 hạng mục, nguồn và phần thiếu), `Docs/Reference_Parity_Backlog.md` (R1–R8). Details/menu/camera đã có runtime K1. Shop vẫn thiếu giao dịch HP/altar chance và nhiều nền tảng build; không coi prototype là reference parity hoàn chỉnh.

Ý tưởng nguyên bản 2–6 đã duyệt và mở rộng trong `Docs/Signature_Gameplay_Expansion.md`: 12 concept vũ khí, 6 hướng build, biến hình/biến thể tier, 6 đạo cụ map và pipeline hit/death/coin/merge. Đã nối vào backlog; chưa implement các tính năng này và không thêm formation command.

Brainstorm UI/font/gameplay bổ sung **đã duyệt hết**: `Docs/Nightmare_Identity_UI_Gameplay.md` (S6–S10, 15 hạng mục N01–N15). Bao gồm hồ sơ/build links, receipt/results, font ba vai trò, enemy/boss/personality/audio/material death và museum/nickname/cosmetic; đã map vào R1–R7, chưa thay runtime/build hoặc thêm font asset.

Unity 6000.4.8f1. Bản mới: **Full Run — functional candidate P3–P6**. Đã nối combat, shop, build, save, run 20 hall và Endless; chưa nghiệm thu chất lượng production.

## Chơi bản mới

1. Chạy `Builds/FullRun/ArcaneBrigade.exe`, giữ nguyên toàn bộ thư mục đi kèm; hoặc mở `Assets/ArcaneBrigade/Scenes/RunBoot.unity` rồi Play trong Unity.
2. WASD/mũi tên để di chuyển, ESC pause, F3 xem leash. Không có nút đánh/skill. Touch kéo vùng dưới bên trái; chưa kiểm thử trên điện thoại.
3. Sau hall có **3 quầy trong sân**, không mở bảng shop: di chuyển tới quầy, nhấn **Space** hoặc nút **USE** trên màn hình để mua. Reroll và cửa hall kế tiếp cũng tương tác tại chỗ. Shop đặc biệt dùng **EGGS**, không trừ vàng; lịch xuất hiện và giá hiện là tuning thử nghiệm. Đồng minh tự đi trong bán kính, bất tử; cần Guardian Canopy để chặn đạn.
4. Resume khôi phục shop đã lưu hoặc **bắt đầu lại hall hiện tại từ ranh giới**; không khôi phục projectile giữa trận. Meta chỉ unlock/codex/record/cosmetic, không tăng stat vĩnh viễn.

Nội dung hiện tại: 3 Champion, 7 loại đồng minh, 9 vũ khí (gồm cung, trượng phép và DreamStamp), 29 item, 4 bond, 6 enemy definitions và 5 boss variants; hai đạo cụ sân và weapon sandbox. Một số nhân vật/địch/boss dùng chung body; không phải 21 model độc lập.

Presentation mới: ảnh Unity và video motion tại `PreviewExports/Presentation/`. Chi tiết shop: `Docs/World_Shop_Implementation.md`; đối chiếu hình ảnh, asset và giới hạn âm nhạc: `Docs/Presentation_Reference_Pass.md`. Nhạc hiện là bản study tổng hợp, chưa phải soundtrack hoàn chỉnh. Dùng **Presentation > Apply presentation and check** để áp dụng art/map và chạy kiểm tra; không dùng menu dựng baseline Full Run nếu muốn giữ presentation mới.

Ảnh Unity: `PreviewExports/Unity_FullRun_Model_Gallery.png`, `Unity_FullRun_Combat_UI.png`, `Unity_FullRun_Shop_UI.png`, `Unity_FullRun_Boss_UI.png`. Shop/boss UI là fixture kiểm tra, không phải bằng chứng thắng run tự nhiên.

Menu **Arcane Brigade > Full Run > Build content, scenes and checks** dựng lại baseline, **có ghi đè tuning trong Data/Run**. Chỉnh gameplay tại `Assets/ArcaneBrigade/Resources/RunCatalog.asset` và `Assets/ArcaneBrigade/Data/Run/`; build Windows bằng menu tương ứng. Báo cáo và giới hạn: `Docs/Full_Run_Implementation.md`.

Muốn kiểm tra mà giữ tuning: **Full Run > Check existing content without resetting**. Footage đã đối chiếu và các điểm còn chưa xác nhận: `Docs/P3_P6_Worklog.md`.

Xem trực tiếp sáu bộ model mới trong `Assets/ArcaneBrigade/Scenes/RunModelGallery.unity` (gallery tĩnh; animation chạy trong RunArena).

## Chơi checkpoint

1. Mở `Assets/ArcaneBrigade/Scenes/Boot.unity` trong Unity, nhấn Play rồi chọn **PLAY 60-SECOND WAVE**. Có thể mở `Arena.unity` để vào trận ngay.
2. Di chuyển bằng WASD/phím mũi tên; nhân vật và đồng minh tự đánh. ESC tạm dừng, F3 bật/tắt vòng bán kính đồng minh; có chơi lại và về menu.
3. Touch dùng kéo vùng dưới bên trái; chưa kiểm thử trên Android/iOS thật. Chưa hỗ trợ tay cầm.
4. Bản Windows mới nằm tại `Builds/Checkpoint02/ArcaneBrigade.exe`; phải giữ nguyên các file/thư mục đi kèm. Bản đầu tiên vẫn giữ trong `Builds/Checkpoint01/`.

Menu `Arcane Brigade > Checkpoint > Build assets, check and capture` dựng catalog, prefab, scene, chạy smoke checks và chụp camera Unity. `Build Windows playable` build bản PC.

Với phase đội quân hiện tại dùng menu **`Arcane Brigade > Squad > Upgrade, check and capture`** và **`Build Windows playable`** trong nhóm Squad. Menu dựng baseline Checkpoint cũ có thể ghi lại tuning/scene; không dùng để capture squad đã chỉnh.

Đồng minh tuần tra độc lập, tách nhau nhẹ, tăng tốc khi đi xa champion và giữ trong bán kính 3.2 world units. Chỉnh radius/tốc độ/thời gian đổi hướng trong `Assets/ArcaneBrigade/Data/Checkpoint/CheckpointCatalog.asset`. Bóng chân champion vàng, ally xanh ngọc để dễ phân biệt.

Ảnh mới: `PreviewExports/Unity_Checkpoint02_Squad.png`; ảnh đứng yên 2/6/10 giây: `Unity_Checkpoint02_Roam_02s.png`, `Unity_Checkpoint02_Roam_06s.png`, `Unity_Checkpoint02_Roam_10s.png` trong `PreviewExports/`. Báo cáo: `Docs/Checkpoint_02.md`.

Ảnh: `PreviewExports/Unity_Checkpoint01_Arena.png` và `PreviewExports/Unity_Checkpoint01_Modular.png`. Báo cáo phạm vi/kiểm thử: `Docs/Checkpoint_01.md`.

## Xem mẫu

1. Add thư mục repository này trong Unity Hub, mở bằng Editor 6000.4.8f1.
2. Mở `Assets/ArcaneBrigade/StylePreview/Scenes/MinimalArcadePreview.unity`.
3. Nhấn Play để xem các pose đổi tự động. Đây là animation thử, không phải gameplay hoặc rig production.
4. Dùng menu `Arcane Brigade > Style Preview > Build scene and capture` để dựng lại prefab, clip, scene và chụp ảnh bằng camera Unity.

Prefab và `.anim` nằm trong `Assets/ArcaneBrigade/StylePreview/`. Gói `.unitypackage`, ảnh chụp và báo cáo render nằm trong `PreviewExports/`.

Hướng Minimal Arcade đã được duyệt. Prefab gameplay tại `Assets/ArcaneBrigade/Prefabs/Checkpoint/ModularUnit.prefab` tách body, tay và vũ khí; tay–vũ khí cùng `GripRoot`, không có cánh tay. Animation gameplay hiện là bob/squash/swing/recoil procedural, chưa phải animation vẽ tay hoàn thiện.

Xem `Docs/Minimal_Arcade_Art_Direction.md` về style và các giới hạn, `Docs/Production_Plan_2D.md` về gameplay và `Docs/Implementation_Plan.md` về checkpoint implement P0–P7.
