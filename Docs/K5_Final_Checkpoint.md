# K5 final checkpoint

**DONE theo functional gate runtime Windows — 2026-10-14 (Asia/Saigon).** K5.1–K5.5 hoàn tất trong phạm vi đã duyệt; chưa nhận production/mobile gate K7 và chưa làm redesign font/UI K6.

## Đã nối vào game

- Đủ12 concept vũ khí: ô, chuông, dấu, trà, cung zipper, kéo, camera, cá hộp, bàn ủi, búa cao su, đồng hồ mortar, hộp nhạc. Giữ9 ID cũ, thêm7 ID;16 biến thể vẫn roll ngẫu nhiên sau mua. Không kỹ năng chủ động, bán/nuôi/cap đồng minh.
- Sáu nhánh build với payoff từ hai thành phần, sáu relic mới có trade-off thật; Thermal/Chorus/Treasure/Toy bridge hữu hạn. Bonus cửa sổ rương và heal đo lượng hiệu lực, không cộng damage hai lần. Proc từ primary kill của kéo/mortar trễ đã sửa đúng thứ tự xử lý.
- Sáu đạo cụ: chuông, xe đồ chơi, bình mực vùng chậm, búa treo, gương copy một projectile, hộp nhạc hỗ trợ cooldown. Seed/layout, warning, swept hit, root dedup và boss layout shift vẫn qua pipeline thật.
- Enemy cửa/nam châm/giả ngủ/rèm với unlock theo wave; composition nhấn swarm đầu, door/magnet giữa, special cuối. Ba motif boss nửa máu có warning và damage nhỏ, vị trí khóa để né; không khóa lối đi hoặc đổi điều khiển.
- Atlas gốc riêng cho weapon/enemy/prop/relic; không có cánh tay, bàn tay đi cùng grip. Pose/cadence/tier, material death, zone/zipper/fish/note VFX và9 nhóm âm thanh mới. Sandbox cùng runtime có mô tả luật và zoom đọc mẫu rõ hơn.

Đặc tả luật/tuning/giới hạn: `K5_Content_Guide.md`. Nguồn ảnh/prompt và alpha: `K5_Art_Manifest.json`. Đây là art và tuning riêng, không trích asset/âm nhạc hoặc xác nhận thông số Bizarre Brigade.

## Nghiệm thu

- K5:139 assertions PASS (family/tier, field timing/evade, finite effects, guard/magnet/mimic/curtain, props thực, partial builds, contribution, delayed primary proc, save/migration).
- Hồi quy: Checkpoint17, Squad12, FullRun104, K0/K127, K230, transaction36, modifier29, altar14, K3Final31, HD31, K4Signature40, mergePASS, arena49, sandbox258, K4Final36. Những suite phụ thuộc có chạy lại baseline; không cộng trùng để phóng đại số test.
- Build Windows64: Succeeded,0errors,0warnings,168232167bytes. Binary local: `Builds/FullRun/ArcaneBrigade.exe`; build không đưa vào Git. Báo cáo: `PreviewExports/K5/Windows_Build.txt`.
- Native player:40 assertions PASS,15 PNG3840×2160; live Unity1920×1080, screenshot supersampling2×, không resize bằng công cụ ngoài. Đã rà hình combat, prop, boss và sandbox; sửa atlas cung bị cắt trước gate cuối. Mô tả sandbox hiện nằm trong viewport, không bị cắt dòng cuối.
- Capture có fixture: seed5505/hall10, roster12/relic/chest định sẵn, ambient spawn tắt, enemy contact/đòn thường tắt vàHP1500, movement behavior vẫn chạy. Để chụp từng prop, Champion được dời vị trí và primary root charge hợp lệ được bơm qua NotifyArenaToyHit. Boss hall12HP49%, bullets thường tắt; phase/warning thật. Sandbox discovery là danh sách chỉ đọc của fixture. Không gọi đây là run tự nhiên/playtest cân bằng.
- Save/profile của người chơi không bị dùng: diagnostic root riêng. Native captures không chứng minh Android/iOS/Steam SDK acceptance hoặc music/motion audition bằng video.

Gallery: `PreviewExports/K5/index.html`. Chi tiết assertions: `PreviewExports/K5/Checks.txt`, `PreviewExports/K5/Player/CaptureNotes.txt`. Summary gate không chứa thông tin license/machine: `PreviewExports/K5/Editor_Checks.txt`; raw log ở ignored`Logs/K5-ReleaseGate.log`. SHA256 candidate source/catalog/art/build/capture: `PreviewExports/K5/Artifact_SHA256.txt`.

## Tiếp theo

K6: hệ font/UI đồng nhất, HUD/menu/Details/shop/results, museum/meta và contribution presentation; giữ shop vật lý ba lựa chọn và currency đã chốt. K7: natural playtest/balance/readability, texture/performance mobile, audio/video motion review và thiết bị thật. Nguồn PNG không phải native4K; lựa chọn của chủ dự án là ưu tiên game render sắc ở4K. Không giảm cap đồng minh để né performance gate.

Reproduce: Unity6000.4.8f1, `ArcaneBrigade.Editor.K5Builder.GateAndBuild`; không chạy baseline builder để reset tuning. Native player flags: `-arcane-k5-capture -capturePath <absolute-folder> -force-d3d11 -screen-width 1920 -screen-height 1080 -screen-fullscreen 0`.
