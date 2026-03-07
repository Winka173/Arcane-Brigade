# Kiểm tra idle / walk riêng của hero — 18/10/2026

**31/31 playable đã có và đang sử dụng pose idle/walk riêng. Không tìm thấy hero cần vẽ bù hai trạng thái này trong catalog hiện tại.** Mỗi hero có hai hình idle và ba hình đi riêng biệt: tổng 62 pose idle + 93 pose walk. Đây là art đã có từ các đợt trước, không phải 155 hình mới được vẽ trong lần kiểm tra này.

LanternKnight dùng `AuthoredMotionSet` riêng tại `Art/FinalQuality/LanternKnightMotion.asset`. Ba mươi hero còn lại dùng entry `locomotion` theo đúng ID trong `ExpandedPerformanceLibrary`, gồm hai frame idle và ba frame walk. `UnitVisual.Bind` lấy entry theo ID hero và `UnitVisual.Display` thực sự chọn các sprite này khi đứng/đi. Một số motion asset nền cũ chứa frame lặp nhưng đã bị bộ locomotion ghi đè khi chơi; chỉ đếm asset nền sẽ kết luận sai rằng hero chưa có animation.

Idle hiện tại chủ yếu là đứng và chớp mắt/đổi biểu cảm, có chuyển động trình diễn bằng code; walk là ba pose vẽ riêng. Không gọi đây là bộ bốn idle/tám walk, animation nhiều hướng hoặc chuyển động được vẽ ở mọi frame. RootOracle vẫn đứng yên trong combat theo mechanic, nhưng có pose walk cho nơi cho phép di chuyển như shop. Reduced Motion chủ động giữ một pose ổn định.

## Bằng chứng kiểm tra mới

- [Coverage theo từng hero và đường dẫn sprite](../PreviewExports/HeroLocomotion/Coverage.tsv).
- [280 kiểm tra PASS](../PreviewExports/HeroLocomotion/Checks.txt), [kết quả máy đọc](../PreviewExports/HeroLocomotion/Verification.json).
- Bảng pose: [hero 1–8](../PreviewExports/HeroLocomotion/Hero_Poses_0.png), [9–16](../PreviewExports/HeroLocomotion/Hero_Poses_1.png), [17–24](../PreviewExports/HeroLocomotion/Hero_Poses_2.png), [25–31](../PreviewExports/HeroLocomotion/Hero_Poses_3.png).

Kiểm tra mới tạo `UnitVisual` thật từ prefab và lấy `BodyRenderer.sprite` trong hơn một chu kỳ idle/walk. Xác nhận mọi pose được chọn, đi dùng hình khác đứng, các hero không dùng chung sprite locomotion, dừng trả về idle, thời gian pause không đổi frame và Reduced Motion giữ một pose đầy đủ. Probe chỉ vô hiệu hóa diễn xuất tò mò trong lúc đo locomotion cơ sở; vũ khí được clone thành probe để tránh khởi tạo hiệu ứng vũ khí trong edit mode. Không thay source asset, gameplay hoặc mechanic.

Bốn bảng hình render các sprite được đo bằng camera Unity để duyệt hình dáng/chân/tư thế. Đã xem đủ bốn bảng. Đây là nghiệm thu body và lựa chọn sprite trong Unity Editor, không phải bản Windows mới hoặc chứng nhận mọi tình huống combat/camera/ánh sáng.

Chạy lại bằng Unity CLI với `-executeMethod ArcaneBrigade.Editor.HeroLocomotionAudit.Run` (có graphics, không thêm `-nographics`). Công cụ chỉ xuất bằng chứng tại `PreviewExports/HeroLocomotion/` và không sửa catalog.

## Worklist nâng cấp chưa thực hiện

`Docs/Bizarre_Hero_Animation_Worklist.json` là bộ **372 job khác**, gồm bốn idle và tám walk cho mỗi hero. Các file riêng `_Idle_fN` / `_Walk_fN` trong worklist đó hiện chưa được tạo. Không đánh dấu worklist ấy hoàn thành chỉ vì bộ hai idle/ba walk hiện hành đã đủ coverage. Yêu cầu lần này là bổ sung hero còn thiếu idle/walk; kết quả kiểm tra không có hero thiếu, nên không tạo thêm art trùng nhiệm vụ hoặc thay hàng loạt art đã có.
