# K3 — Triển khai theo lát cắt

**Cập nhật cuối:** K3.1–K3.3 và gate Windows K3.4 hoàn tất; checklist `K3_Final_Checkpoint.md`. Chủ dự án chốt runtime Windows, device gate sang K7. K3 DONE theo gate này, có thể qua K4; không gọi hoàn tất production/mobile/parity. Các trạng thái bên dưới giữ lịch sử bàn giao.

K0–K2 recheck ngày 2026-10-13: checkpoint 17, squad 12, full-run 96, K0/K1 27, K2 22 PASS. Đây là kiểm tra tự động Windows, không phải chứng nhận art/mobile.

## K3.1 — Giao dịch và biên lai (Windows candidate)
- Một pipeline cho mua gold/eggs, reroll và recipe hiện có. Thất bại/exception phục hồi tiền, roster, material, offers, hai RNG và lịch sử; chỉ hiển thị thành công sau khi save thành công.
- Schema 2 nhận schema 0/1, giữ IDs và RNG. Lưu reroll counter, tám biên lai gần nhất cùng snapshot. Không đổi catalog version hay sửa baseline K0.
- Biên lai ghi chi phí thật, weapon/tier thêm và mất, stack, stat Champion, stat weapon của đồng minh còn giữ và bond before/after từ read model hiện có. Nút xem tại rest + lịch sử trong Details/Vật phẩm; vẫn mua bằng ba quầy world-space.
- Chưa phải N17 hoàn chỉnh: số hiển thị theo precision của Details; chưa có preview trước mua, source breakdown cho mọi stat hoặc contribution graph.

Kiểm tra/phạm vi bàn giao: `K3_Implementation.md`. K3.2a đã tiếp tục bên dưới; chưa qua K4.

## K3.2 — Modifier và economy (K3.2a Windows candidate)
- Tách scope/rarity/stack rules, weighted pools và data-driven build links.
- Đối chiếu Bizarre Brigade 1.5+ trước khi chốt curve tăng giá reroll, altar HP/eggs, activation chance và inheritance. Giá reroll hiện vẫn là tuning prototype, counter không tự chứng minh công thức reference.
- Kiểm tra stat UI/combat sau mua, merge, recipe và resume; không thêm điều khiển ngoài di chuyển/tương tác tại rest.

K3.2a có additive modifier list/scope/rarity/stack, weighted item pools, reroll giá tăng với tuning riêng và tab liên kết từ nguồn thật. Báo cáo: `K3_Modifier_Economy.md`. K3.2b còn altar HP/chance/inheritance và những công thức reference chưa xác minh; chưa đánh dấu toàn bộ K3.2 hoàn tất.

K3.2b đã có runtime HP hiện tại, activation theo eggs và receipt/rollback bằng tuning riêng; báo cáo `K3_Altar_Economy.md`. Công thức parity/inheritance, exclusive pool và recipe preview còn lại; chưa hoàn tất toàn bộ K3.2. Có thể tiếp tục nền K3.3 trong khi giữ các decision chưa xác minh.

## K3.3 — Cause/proc và loot
- Cause registry và proc guard; random gameplay tách khỏi presentation.
- Chest/egg/heart, Treasure Detection/Luck, chỉ báo ngoài viewport ưu tiên/gộp/fade. Không biến mọi coin thành marker; loot logical vẫn tồn tại ngoài camera.

Runtime đã có hai proc root-only, RNG riêng, loot bốn loại và indicators thật. Final checks/capture PASS; xem báo cáo cuối.

## K3.4 — Gate hoàn tất
- Regression, failure injection, end-to-end mua tại quầy và save/resume.
- Capture native kiểm đọc UI, touch safe-area, scene motion và các trade-off; kiểm device Android/iOS riêng.
- Chỉ qua K4 sau khi toàn bộ K3 đạt gate. K3.1 không được ghi thành K3 hoàn tất.

Windows gate PASS. Theo xác nhận của chủ dự án, Android/iOS device gate chuyển K7; chưa chạy vì module/device chưa sẵn sàng. K3 chốt Windows, giữ AUTHORED tuning và không chứng nhận exact-reference parity.
