# K9 — coverage animation và feedback

Đây là coverage luồng tương tác, không phải tuyên bố có clip vẽ tay riêng cho mọi state. Sprite gốc hiện có bốn frame cho nhiều actor; body puppet, camera và UI dùng procedural animation. Không đưa art/audio Bizarre Brigade vào game.

| Ngữ cảnh | Animation/feedback | Tình trạng K9 |
| --- | --- | --- |
| Main menu, mở trang | Alpha + scale reveal ngắn, có giảm chuyển động | Mới, dùng chung cho roster/settings/records/help/museum/sandbox |
| Chọn/nút menu | Hover drift, nhấn co rồi trả lại, âm xác nhận | Nâng cấp; identity theo vị trí điều hướng để các nút không chữ không dùng chung hover |
| Roster chân dung | Chớp mắt, bước ngắn khi focus, trạng thái khóa | Có từ K8; không tự thêm chuyển động cho state khóa chưa có frame riêng |
| Settings | Hover/press cho button; đổi bus âm và giảm motion thật | Nâng cấp button; slider/toggle dùng hệ thống hiện có |
| Sổ đội quân mở | Fade/scale, chuyển tab/selection đổi viền màu | Mới; đóng vẫn tức thì để không kéo dài pause |
| Sổ đồng minh | Lợi/hại và nguồn modifier thật | Mới, thông tin không bị thay bằng animation |
| Inventory/relic/link | Selection/scroll và giải thích trong cùng panel reveal | Nâng cấp panel; không tự cộng buff theo animation |
| Kết quả/hóa đơn | Chân dung đội lần lượt hiện; death/victory transition hiện có | Mới ở hóa đơn; điểm luôn hiển thị đúng, không thay score thật bằng số đếm đang chạy |
| Resume/overwrite/error | Page reveal + nút press | Nâng cấp; lỗi giao dịch không giả success |
| Thủ lĩnh đứng yên | Blink, upper/lower breathing, trọng tâm và vũ khí idle | Có; tắt wobble khi reduced motion |
| Thủ lĩnh di chuyển | Bước, lean, bụi, vũ khí theo hướng | Có |
| Đồng minh tuần tra/nghỉ | Chu kỳ roam/pause độc lập, idle body/breath/blink | Có; không buộc cả đội di chuyển đồng thời |
| Nội tại hồi máu | Feedback tại người nhận và glyph/ngẩng thân tại nguồn nếu thật sự hồi | Mới tại nguồn |
| Nội tại tìm vàng | Glyph/ngẩng thân khi thật sự tăng vàng | Mới; vàng đạt cap không giả nhận thêm |
| Nội tại chill | Glyph lạnh/ngẩng thân khi có địch trong vùng | Mới; không chớp khi không có mục tiêu |
| Địch thường | Idle/move, hit tint/squash, death theo vật liệu | Có; silhouette và màu phe đã phân biệt |
| Boss | Windup, telegraph, motif, hit/death, weakspot | Có; không đổi simulation vì thêm animation |
| Kiếm/quét/đâm | Anticipation, cung đánh, recovery, slash | Có từ K8 |
| Cung | Kéo dây/tay, nhả, recovery | Có từ K8 |
| Phép/đạn nhiều tia | Cast/recoil/muzzle, projectile, impact | Có; số projectile do logic thực, không vẽ giả tia thêm |
| Boomerang/signature | Bay về, trail, mark/pulse/burst và chain giới hạn | Có; không tự nhận mọi loại vũ khí đều nảy/chia nhánh |
| Loot rơi/nhặt | Bounce, bóng, hút trong vùng, bay/biến mất | Có |
| Wallet vàng/trứng | Icon pop khi số thật thay đổi | Mới; số ví không bị lerp thành giá trị sai |
| Thanh máu | Damage trail theo sau; chữ HP đúng ngay | Mới; đứng yên khi pause, reduced motion cập nhật ngay |
| Icon hệ góc phải | Bounce damped khi thưởng hệ tăng; flash hữu hạn; hover/click tooltip | Mới bounce; bỏ tooltip tự bật khi kích hoạt |
| Nút pause/sổ | PC ẩn trên HUD, giữ Esc/I/pad; touch đặt bên trái | Mới, trả góc phải cho hệ |
| Hàng shop idle/focus | Body blink/idle, spring phóng lớn, giá/rarity glow | Có từ K8 |
| Merchant vào quầy | Hạ xuống, settle, bóng, confetti | Có từ K8 |
| Merchant nói | Text reveal + frame 2/3 + rung thân nhẹ + murmurs theo chữ | Mới; hai giọng/pitch; không phải thu âm diễn viên |
| Merchant mua/thiếu tiền/máu/reroll/fuse/rời | Dialogue riêng + pose/reject/celebrate + SFX transaction | Có, nối speaking mới |
| Mua item | Bay về người, reveal/receipt sau commit | Có |
| Mua đồng minh | Nhảy xuống, bước khỏi quầy, tier reveal | Có |
| Merge/altar | Combine/reveal, đồng minh result, hiệu ứng hữu hạn | Có; K9 cập nhật HP khi nguồn buff đổi |
| Reroll | Hàng cũ nghỉ, hàng mới vào so le | Có |
| Xuống màn mới | Cửa riêng, hướng xuống, curtain transition | Có; camera zoom nay chuyển mượt |
| Map/quầy | Palette theo chặng, bóng props/rug; pennants gió nhẹ | Mới pennants; không cho mọi prop đung đưa vô nghĩa |
| Âm thoại bị pause/background | Dừng voice; không phát từ OnGUI | Mới, lane riêng không chiếm danger voices |

## Giới hạn/gate còn mở

- Người chơi duyệt cảm giác hình/âm vẫn là gate riêng. Audio murmur tạo thủ tục có thể cần thu/thiết kế giọng riêng sau nghe thử; waveform hay test tự động không chứng minh hay.
- Không gọi pose merchant 2/3 là lip-sync phoneme; đây là gibberish cadence.
- Chưa vẽ hàng chục clip riêng cho từng đồng minh/boss. Nếu cần chất lượng animation frame-by-frame, phải làm thêm sprite states có art direction và nghiệm thu riêng.
- Device gate Android/iOS, controller thật và chứng nhận Steam vẫn mở theo quyết định trước đó.
- So sánh chỉ số chi tiết: `Docs/Bizarre_Brigade_15_Stat_Comparison.md`. Chưa âm thầm thêm toàn bộ stat còn thiếu.
