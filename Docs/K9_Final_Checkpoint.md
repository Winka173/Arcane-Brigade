# K9 — living UI, thoại và đặc tính đồng minh

2026-10-14. Candidate Windows mới; không phải chứng nhận chất lượng thương mại hoặc parity công thức Bizarre Brigade.

## Đã nối

- PC ẩn nút pause/sổ trên HUD, vẫn dùng Esc/I/pad; touch chuyển sang trái. Góc phải là icon hệ với bounce damped khi reward tăng, không tự bật tooltip chữ. Pause đóng băng nhịp; clock reset khi qua wave xóa flash cũ.
- Merchant có murmurs theo nhịp chữ trên audio lane riêng, chuyển pose/body trong lúc nói; không phát từ OnGUI, không tranh danger voices, dừng khi pause/background và im khi reduced audio. Đây là âm thủ tục gốc, không phải diễn viên hay phoneme lip-sync.
- Menu/page, nút hover/press, sổ/tab, hóa đơn roster, wallet/HP trail, pennants và chuyển zoom được bổ sung motion. Nội tại hồi máu/tìm vàng/chill phản hồi tại nguồn chỉ khi có tác dụng thật. Các motion combat/loot/shop/merge/death từ K8 giữ lại; coverage ghi rõ từng nơi và giới hạn.
- Bảy đồng minh có `CompanionModifiers` dương/âm, scope thật và tier ×1/2/4. Buy/merge/altar tính lại từ roster, giữ lượng HP đã mất; HP trong shop tối thiểu 1. Restore không nhân bonus lần nữa và clamp HP theo maximum hiện hành, không sửa snapshot truyền vào. Save cũ dùng balance hiện hành khi tiếp tục, không được cộng heal miễn phí khi restore.
- Đặc tính hiện trước mua và trong sổ, chỉ số có truy nguồn. Không cho chọn hoặc preview như đã chốt vũ khí recruit ngẫu nhiên.

## Research

`Docs/Bizarre_Brigade_15_Stat_Comparison.md` kiểm kê 17 nhóm công khai: 8 nền tảng tương đương, 4 một phần, 5 còn thiếu; thêm bảng thuộc tính đạn/body/shop/địch/điều kiện. Footage 1.5 là nguồn chính; các tên chưa nhìn rõ trong panel được đánh dấu nguồn cộng đồng. Không khẳng định biết toàn bộ enum/cap/công thức nội bộ.

Không âm thầm thêm stat né tránh, va chạm gây damage, damage cận/xa cộng thẳng hoặc hệ số điểm chỉ để báo đủ tên. Không phí nuôi, nút kỹ năng hay nâng stat vĩnh viễn theo luật đã chốt.

## Kiểm tra và bản thử

- Editor: 33 focused checks K9, cộng toàn bộ regression K8 và các gate K3–K7; log lọc tại `PreviewExports/K9/Editor_Gates.txt`.
- Windows build: `Builds/FullRun/ArcaneBrigade.exe`; giữ nguyên folder player đi kèm. Báo build tại `PreviewExports/K9/Windows_Build.txt`.
- Native: 107 assertions, 34 PNG; 31 framebuffer 3840×2160 và 3 desktop aspect-resize. Có kiểm tra dispatch nhịp thoại và silence lúc mở sổ/pause, không xem đó là duyệt chất lượng nghe.
- Probe 12 giây ở mỗi roster 1/56/100 trên RTX 5070 Ti / Ryzen 9700X: dữ liệu thật tại `Player/Native_Metrics.txt` và CSV. GC counter release không có (NA), không gọi zero-GC; desktop aspect không phải device safe-area.
- Balance diagnostic 15 seed/mode ở pipeline companion traits K9 đầu tiên: 15 thắng; chỉ là bot, không chứng minh cân bằng tự nhiên hoặc mọi seed. Các fix sau đó giới hạn HP khi restore và reset visual clock; không đổi giá/hệ số traits trong CSV.
- Gallery/checklist: `PreviewExports/K9/index.html`. Movies gameplay15s và shop38s, 1920×1080, silent Unity framebuffer, không ghi desktop/microphone. CFR60 là thông số encode, không chứng minh game luôn render60FPS. Stock/roster/vàng trong fixture được seed.
- Fingerprints source/data/evidence và exe cục bộ: `PreviewExports/K9/Artifact_SHA256.txt`. Build folder bị Git ignore; source, data, meta và evidence được đưa vào repository.
- Reproduce: Unity batch `ArcaneBrigade.Editor.K9Builder.Build`; native player thêm `-arcane-k7-capture -arcane-k7b -arcane-k8 -arcane-k9 -capturePath <absolute-directory> -force-d3d11 -screen-fullscreen 0`; movie `Tools/Record-K7Window.ps1 -Phase K9 -Mode shop` hoặc `combat`.

## Gate vẫn mở

- Duyệt art/cảm giác/nhạc/giọng thực tế của người chơi; murmurs có thể cần âm giọng thu riêng sau nghe thử.
- Sprite clip riêng đầy đủ cho mọi nhân vật/vũ khí/hướng, soundtrack độc quyền/cùng art direction, biome gameplay và nội dung mở rộng không được giả là đã xong.
- Android/iOS device, controller thật, Steam/Deck và co-op vẫn theo gate/defer đã chốt. Nguồn art không native 4K.
