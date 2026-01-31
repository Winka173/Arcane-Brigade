# Sửa chiến đấu, cửa shop và đồ cảnh — 16/10/2026

Đợt này xử lý phản hồi về nhân vật không đánh, melee hụt tầm, địch rút lui, nhịp tuyển, hiệu ứng mechanic và kiến trúc bám camera. Đã build và kiểm tra bản Windows mới; bằng chứng nằm trong `PreviewExports/CombatRepair`.

## Chiến đấu và nhịp tuyển

- Bỏ các khóa tấn công cũ trên hero và một số companion; hero dùng vũ khí đã trang bị cùng mechanic riêng. Giữ các đánh đổi được relic mô tả rõ như Collision Only và Solo Overdrive.
- Melee xét mép thân mục tiêu, hướng vũ khí tiếp tục bám mục tiêu trong windup. Điểm dừng của companion nằm trong tầm đánh; companion gây sát thương va chạm có thể tiến đủ gần để chạm thật.
- Bỏ hành vi lùi khi hero tới gần của địch ranged. Đòn ram trước đây bước lùi được đổi thành bước tiến; vẫn giữ di chuyển ngang, đòn lao và knockback chiến đấu.
- Máu địch thường, boss, Keeper, Last Customer và boss sự kiện nhân thêm **0,8**. Damage vũ khí nhân thêm **1,08**; contact nhân thêm **1,08 × 1,6**, thời gian giữa hai lần contact nhân **0,9**, tối thiểu 0,45 giây. Hệ số wave, độ khó, mutation và giới hạn riêng vẫn áp dụng; các thay đổi cân bằng mode đang được thực hiện riêng xem `Mode_Balance_Pass.md`.
- Companion thêm **8% damage vũ khí/va chạm, 5% tốc độ di chuyển**, vẫn chịu trần tốc độ 10 và vẫn bất tử. Không tăng HP vốn không dùng để mất máu.
- Tối đa **một loài mới mỗi round từ round 1**, giữ ba lượt mua companion thường gồm bản trùng. Shop đặc biệt miễn quota mua; trần đội 15 giữ nguyên. Save cũ đã dùng hai lượt loài mới vẫn tải được nhưng không mua thêm loài mới trong round đó.

Kiểm tra đánh thật dùng mô phỏng, đếm `Fire` và `Hit` có đúng nguồn sát thương; không tự thêm damage để cho test qua. Bảng [Attack_Audit.tsv](../PreviewExports/CombatRepair/Attack_Audit.tsv) gồm 31 hero, 66 companion và 57 vũ khí. Bảng `Attack_Audit_Before.tsv` là ảnh chụp đợt audit ban đầu sau sửa tầm melee, phát hiện tám trường hợp còn bị khóa; không phải baseline nguyên trạng trước mọi sửa đổi. Kiểm tra bổ sung để companion tự đuổi tới một mục tiêu cố định cách 3,5 đơn vị.

## Cửa và cảnh rơi khi vào shop

Cửa ở giữa phía dưới, ngưỡng cố định `(0, -4)` sau layout lift. Bán kính tương tác **1,85**. Đã sửa cả bộ chọn `Nearby`: trước đây nó dùng khoảng cách tối đa 1,1 dù station có bán kính lớn hơn. Trong vùng chồng nhau, station gần nhân vật hơn được chọn. Sprite cửa lơ lửng không kéo ngưỡng tương tác theo nó.

Theo câu trả lời xác nhận của chủ dự án, **chỉ đồ cảnh rơi khi vừa vào shop, đèn được loại trừ**. Đồ cảnh hạ từ trên xuống, chạm mặt sàn neo cố định, nảy nhẹ rồi trở lại lơ lửng. Áp dụng cho landmark không phải đèn, kiến trúc và các vật chứng truyện đang hiển thị; không áp dụng lên loot/nhân vật. Quầy hàng giữ animation xuất hiện vốn có. Reduced motion giữ tư thế ổn định. Đồng hồ presentation dừng cùng pause. Giữ quyết định bỏ khói/gió dưới chân đồ vật.

## Kiến trúc đứng trên map và vỡ dần

Lớp `NightmareGrandStage` cũ dùng tâm camera ×0,82 và kích thước khung hình để đặt kiến trúc khổng lồ ở viền. Vì vậy người chơi thấy đồ vỡ di chuyển theo màn hình ở các wave sâu.

Đã thay lớp đó bằng **12 đồ cảnh neo theo tọa độ map và seed**, rải hai bên, trên/dưới và hai cụm bên trong; không đọc vị trí camera. Bốn loại art vẽ mới: vòm cửa, tủ thư, bình phong và ghế dài; mỗi loại có ba trạng thái nguyên/nứt/vỡ, tổng 12 sprite. Có từ wave 1; nứt ở wave 5–8 và vỡ ở wave 12–16 tùy món/seed. Thay sprite tại chính vị trí cũ; chỉ thân nổi/nghiêng nhẹ, bóng giữ trên sàn. Những món ngoài khung hình vẫn ở vị trí thế giới của chúng.

Nguồn/prompt: [Waiting_Scenery_Stages_Prompt.json](Waiting_Scenery_Stages_Prompt.json). Đây là art raster vẽ mới, không dựng Blender. Đèn tiếp tục dùng hệ nguyên/nứt/vỡ và bố cục riêng của `NightLanternField`.

## Hiệu ứng mechanic

Thêm ba atlas vẽ mới: **31 motif hero**, **12 motif build** và bốn nét hỗ trợ chém/đâm/va chạm/hồi phục. `HeroIdentityVisual`, `BuildMorphVisual` và field đồng minh/vũ khí dùng sprite nhiều lớp, nét kéo theo hướng đòn, mảnh dư, chuyển tỷ lệ và fade. Signature/support/proc dùng art cùng bộ; hiệu ứng đánh thường đã có sheet vẽ giữ thư viện animation đó.

Màu sprite gốc được giữ phần lớn; opacity, số lớp và pooling theo ngân sách hiệu ứng/reduced settings. Geometry gây damage vẫn thuộc simulation; art không thay hitbox. Cảnh báo nguy hiểm của địch tiếp tục ưu tiên ranh giới vùng gây damage rõ ràng. Đợt này không tuyên bố đã thay toàn bộ telegraph địch, UI hoặc mọi hiệu ứng phụ trong game.

Nguồn/prompt: [Mechanic_Effects_Prompts.json](Mechanic_Effects_Prompts.json). Ảnh gallery runtime được dùng để kiểm tra atlas và renderer; đây là bố cục kiểm tra art, không phải ảnh đánh thật của 43 mechanic kích hoạt cùng lúc.

## Bằng chứng và giới hạn

Các gate mới: [Checks.txt](../PreviewExports/CombatRepair/Checks.txt), [Build.txt](../PreviewExports/CombatRepair/Build.txt), kiểm tra Windows thường/giảm hiệu ứng tại [Capture_Runs.json](../PreviewExports/CombatRepair/Capture_Runs.json). Quota/hero/shop còn chạy lại các bộ kiểm tra hiện có. Số kiểm tra và hash DLL cuối được ghi theo bản chạy thực tế, không lấy nghiệm thu animation/cốt truyện cũ thay thế.

- Audit **154/154** hero/companion/vũ khí gây sát thương thật; **133 kiểm tra** art, tầm melee, đuổi mục tiêu, rút lui, cửa và đồ cảnh đạt. Bao gồm đủ 66 companion tự đuổi mục tiêu cố định.
- Chạy lại **799** kiểm tra tuyển, **2.330** hero/approach/description/shop feedback và **575** shop presentation: đều đạt.
- Windows **109 kiểm tra / 11 ảnh mỗi chế độ**, tổng **218 kiểm tra / 22 ảnh** thường và giảm chuyển động/hiệu ứng. Xem [wave 1](../PreviewExports/CombatRepair/Native/04-world-scenery-intact.png), [sau khi camera đổi vị trí](../PreviewExports/CombatRepair/Native/05-world-scenery-after-camera-move.png), [wave 11](../PreviewExports/CombatRepair/Native/06-world-scenery-cracked.png), [wave 17](../PreviewExports/CombatRepair/Native/07-world-scenery-broken.png), [cửa mới](../PreviewExports/CombatRepair/Native/03-bottom-centre-door-wide-focus.png) và [gallery mechanic](../PreviewExports/CombatRepair/Native/08-mechanic-art-page-3.png).
- Build Windows thành công, **0 lỗi / 0 cảnh báo**. DLL runtime của bản được chụp: `038B07D3BD19A5381C6FFD126F670CF2B5206B0684396EECB505B4F14A3697DF`.

Các phép kiểm tra xác nhận tấn công, quota, vị trí thế giới, sprite theo wave, cửa và trạng thái render được kiểm tra. Chưa thay thế chơi thử cân bằng dài hạn hoặc bảo đảm mọi góc camera đều đạt mức thẩm mỹ người chơi mong muốn. Bản mới xuất riêng tại `Builds/HeroIdentity/ArcaneBrigade.exe` để không ghi đè executable FullRun người dùng đang mở.
