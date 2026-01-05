# Checkpoint 02 — P3a: đội quân và roaming

Ngày: 2026-10-13. Unity 6000.4.8f1; catalog `checkpoint-0.2`; seed mặc định `131026`.

## Thay đổi gameplay

- Đồng minh **không bám một slot cố định**. Mỗi unit tự chọn điểm đi lại trong vòng quanh champion, đổi điểm sau 1.2–2.8 giây hoặc khi đến gần đích; vẫn đi khi champion đứng yên.
- Khoảng chọn điểm mặc định 0.9–2.7 world units; leash ngoài **3.2 world units**. Đi chậm lúc tuần tra, tăng tốc khi xa champion; giới hạn cuối cùng giữ unit trong sân và leash. Clamp ở rìa là ràng buộc gameplay, không phải teleport phục vụ camera.
- Gia tốc làm đổi hướng mượt, có soft separation với champion/ally. Đây không phải collider cứng: vẫn có thể chồng hình trong lúc lướt qua/đội đông, và không chặn di chuyển người chơi.
- Melee tiến tới địch ở gần trong leash; ranged tuần tra và auto-attack. Không tự đi săn ngoài phạm vi; không thêm nút đánh hoặc kỹ năng.
- RNG patrol riêng cho từng ally, không lấy từ RNG spawn/combat. Restart reset waypoint, velocity, timer và roster fixture.
- Ba ally đầu trận: Wisp/trượng, Knight/nỏ, Jelly/kiếm. Đây là **loadout thử được chỉ định**, không phải chính sách mua/roll chính thức.
- `AddCompanion` là API nền cho roster, không áp slot cap; chưa phải giao dịch tuyển dụng, chưa xử lý trùng/ghép. Unit và Weapon ID vẫn giữ riêng qua definition, chưa hoàn thiện identity/tier/passive/bond.
- Đồng minh không có HP hữu hạn, luôn bỏ qua damage và không chặn đạn. Bóng chân champion vàng, ally xanh ngọc, địch đen; màu là hỗ trợ đọc hình, chưa phải accessibility hoàn chỉnh.
- HUD hiển thị số ally và radius. F3 bật vòng kiểm tra radius (chỉ debug hiển thị, không ảnh hưởng combat).

## Chạy thử và tune

Chạy `Builds/Checkpoint02/ArcaneBrigade.exe` với các thư mục đi kèm, hoặc Play scene `Assets/ArcaneBrigade/Scenes/Boot.unity`. WASD/mũi tên; ESC pause; F3 radius. Đứng yên vài giây để quan sát ally đi quanh, sau đó di chuyển/đổi hướng để kiểm tra catch-up.

Trong `Assets/ArcaneBrigade/Data/Checkpoint/CheckpointCatalog.asset`:

| Field | Ý nghĩa |
|---|---|
| `StartingCompanions` | Roster fixture lúc bắt đầu |
| `CompanionLeash` | Bán kính hoạt động tối đa |
| `CompanionRoamInnerRadius/OuterRadius` | Vành chọn waypoint |
| `CompanionPatrolSpeed` | Tốc độ tuần tra; tốc độ bắt kịp lấy từ UnitDefinition |
| `CompanionAcceleration` | Gia tốc đổi hướng |
| `CompanionSeparation` | Khoảng tách mềm giữa ally; champion có khoảng riêng tối thiểu |
| `CompanionRetargetMin/Max` | Thời gian chọn điểm đi lại tiếp |

Menu `Arcane Brigade > Squad > Upgrade, check and capture` chỉ tạo/nâng loadout khi catalog chưa là `checkpoint-0.2`; chạy lại trên version này giữ tuning đã sửa. Nó mở scene Arena để capture, không ghi lại scene hoặc import lại atlas. Menu baseline Checkpoint cũ vẫn có thể ghi tuning/scene, chỉ dành dựng baseline mới.

`Arcane Brigade > Squad > Build Windows playable` xuất vào `Builds/Checkpoint02/`, giữ bản executable Checkpoint01 để so sánh.

## Bằng chứng

Đã chạy **17 regression + 12 squad checks PASS**. Capture Editor thoát code 0; Windows x64 build thành công với **0 errors/0 warnings**; executable headless smoke **PASS, exit 0**. Headless sử dụng NullGfxDevice, không chứng minh graphics/performance; ảnh camera được render riêng trong Unity trên GPU thật.

- `PreviewExports/Checkpoint01_Checks.txt`: regression combat, fixture melee được cô lập khỏi ally auto-attack để kiểm tra đúng một impact.
- `PreviewExports/Checkpoint02_Checks.txt`: roster, idle patrol/retarget, các điểm độc lập, khoảng cách champion, pause, leash/bounds trong suốt di chuyển/đảo hướng/góc sân, bất tử, seed repeatability trong Editor, auto-attack, API 67 ally không cap và restart.
- `PreviewExports/Checkpoint02_Build.txt`, `Checkpoint02_PlayerSmoke.txt`: kết quả build/kiểm thử executable riêng.
- `PreviewExports/Unity_Checkpoint02_Squad.png`: combat simulation bước thực 16 giây rồi Camera.Render.
- `PreviewExports/Unity_Checkpoint02_Roam_02s.png`, `Unity_Checkpoint02_Roam_06s.png`, `Unity_Checkpoint02_Roam_10s.png`: cùng seed, champion đứng yên, không địch, ba thời điểm khác nhau. Vòng và label chỉ là capture diagnostics, không mockup hoặc ảnh OnGUI.

## Phạm vi còn lại

Chưa hoàn thành toàn bộ P3: chưa bond/stat/passive/support, item mở rộng radius/chặn đạn, production spatial query, random recruitment, trùng/ghép, shop/save/run dài. Các luật chưa xác minh vẫn `NEEDS_REFERENCE`, không tự nhận là giống Bizarre Brigade.

Target và separation còn quét list, chưa chứng nhận performance đội lớn. Check 67 ally chỉ chứng minh không cap trong API, không phải benchmark 67 unit chiến đấu. Android/iOS chưa build/chạy máy thật, gamepad/input/UI production và art/animation polish vẫn còn thiếu.
