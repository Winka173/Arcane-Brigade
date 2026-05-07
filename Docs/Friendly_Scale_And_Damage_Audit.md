# Rà kích thước và sát thương hero/companion — 23/10/2026

Phủ catalog hiện tại: **36 hero, 96 companion, 117 vũ khí**. Sửa presentation và sát thương nền của một số vũ khí dùng chung; không sửa vũ khí nguyên liệu khi nâng sao, quota tuyển, máu địch hoặc bộ kỹ năng hero.

## Kích thước

Trước đây `UnitVisual` dùng chiều cao cả ô sprite / `ReferenceHeight` của sheet. Lề trong tranh và cách import khác nhau khiến cùng chỉ số BodyHeight nhưng phần nhân vật thực nhìn khác nhau. `SpriteInkMetricsBuilder` trước đó không thu đầy đủ hero và các bộ pose thay thế. `K8Motion` có trần companion nhưng không có sàn.

- Đo lại **2.025 sprite**: thêm atlas hero, motion, diễn xuất và locomotion vào dữ liệu painted bounds.
- Dùng phần hình thực của một pose tham chiếu cố định trong mỗi bộ để scale mọi frame; không tự fit từng frame làm animation phình/thu theo tư thế.
- Companion một sao: chiều cao mục tiêu **0,80–0,92 world units**. Hero: **0,95–1,05**, nhân thêm ×1,1 nếu bật nhận diện hero. Giới hạn bề ngang thân dài/rộng theo tỷ lệ để đuôi/cánh không chiếm màn hình.
- Sao vẫn tăng hình ×1 / ×1,15 / ×1,30. Relic tăng/giảm kích thước vẫn nhân sau chuẩn hóa. Căn chân theo phần tranh thực. Đây là scale hiển thị; không đổi radius va chạm hoặc chỉ số tốc độ.
- Yo-yo Squirrel được đưa về thân đọc được ở mức 0,80 một sao, thay cho mức nominal cũ 0,7125 trước ảnh hưởng lề của bộ pose. Không vẽ lại hoặc kéo giãn tranh.

## Sát thương

Đo vũ khí trên cùng Jelly Swordsman để loại khác biệt loài/rarity; cả ba sao, hai seed. Đo thêm từng companion ở ba sao và từng hero ở cấp gốc. So vũ khí **cùng loại**; một đòn đánh nhiều mục tiêu không được coi là DPS đơn mục tiêu.

Các vũ khí sau cao bền vững trong nhiều mức sao của fixture được hạ sát thương nền:

| Vũ khí | Hệ số mới |
| --- | --- |
| TeapotPopgun | 0,60 |
| BambooVase | 0,65 |
| WateringCan, MoonseedOrb | 0,75 |
| DreamOrbStaff | 0,80 |
| StampHammer, PizzaWheel, MaracaHammer | 0,85 |
| PaperLanternBomb, ParasolNeedle | 0,90 |

`FriendlyWeaponBalance` áp dụng cho **hero lẫn companion đang cầm vũ khí**, kể cả vũ khí đổi sau nâng sao. Các asset nền được giữ nguyên để builder không ghi đè đợt tuning. `BuildUnit` hỗ trợ catalog cũ StatRevision=0; khi StatRevision>0, `K10StatRules` tính lại từ damage nền đã tuning, vì vậy chỉ nhân một lần. Flat damage của relic được cộng sau tuning nền.

Giữ hệ số sao sát thương hiện có **×1 / ×2,7 / ×7,29**, cùng thay đổi số đạn/phạm vi/đòn riêng theo sao. Giữ hệ số rarity companion **1 + 0,18 × chỉ số rarity**: Common ×1; Uncommon ×1,18; Rare ×1,36; Mythic ×1,54; Crimson ×1,72; Rainbow ×1,90. Không biến mọi hero hoặc loài cùng sao thành cùng sát thương; các buff cá nhân, kỹ năng, passives và special recruit vẫn có vai trò.

Ví dụ cùng build, wave 10: Teapot Pilgrim trước tuning có compiled hit 14,954 / 40,376 / 109,014; sau là **8,972 / 24,225 / 65,409**. Damage nền Root Oracle 202,562 → **131,665**, Drowsy Seer 57,880 → **43,410**; Lantern Knight và Yo-yo Squirrel không đổi damage nền. Số này là damage đã compile, chưa phải tổng sát thương theo thời gian.

UI bảng build, bảo tàng và sandbox dùng damage nền mới; shop tiếp tục lấy chỉ số từ bản compile thật.

## Bằng chứng và giới hạn

- [Tổng hợp](../PreviewExports/FriendlyScaleDamage/Summary.json).
- [Kích thước](../PreviewExports/FriendlyScaleDamage/After/Sizes.tsv), [2.592 trạng thái đứng/đi/chuẩn bị/ra đòn](../PreviewExports/FriendlyScaleDamage/Presentation/Poses.tsv): cả thường/giảm chuyển động, đủ sao; kiểm tra giá trị hữu hạn, giới hạn kích thước và giữ size bonus.
- Bảng so cùng tỷ lệ: [36 hero](../PreviewExports/FriendlyScaleDamage/Presentation/hero_Scale_ContactSheet.png), [96 companion](../PreviewExports/FriendlyScaleDamage/Presentation/companion_Scale_ContactSheet.png). Ghép trực tiếp sprite đang được Unity chọn với scale đo được, 80 px/world-unit; chỉ minh họa thân đứng, không phải ảnh combat hoặc nghiệm thu ánh sáng.
- Damage từng hero/companion: [trước](../PreviewExports/FriendlyScaleDamage/Current/Damage.tsv), [sau](../PreviewExports/FriendlyScaleDamage/After/Damage.tsv), **648 lượt mỗi đợt**.
- Damage từng vũ khí cùng người cầm: [trước](../PreviewExports/FriendlyScaleDamage/Weapons/Damage.tsv), [sau](../PreviewExports/FriendlyScaleDamage/WeaponsAfter/Damage.tsv), **702 lượt mỗi đợt**.
- Đối chiếu 1.350 compiled-hit trước/sau đúng hệ số một lần, gồm các vũ khí không thay đổi.

Fixture: wave 10, 18 giây, seed 77138/33071, spawn và steering production. Tắt hero identity/mechanic trong lượt cô lập để đo nền, giữ xử lý vũ khí và kỹ năng companion. Không phải tỉ lệ thắng người chơi, không phải nghiệm thu tất cả awakening/approach hoặc mọi build relic ở mọi độ khó. Damage diện rộng cộng nhiều mục tiêu và thay đổi phân bố địch sau khi hạ damage, nên tổng damage ghi nhận không giảm tuyến tính đúng hệ số. Kiểm tra geometry sprite không thay cho duyệt thẩm mỹ cả trận đông. Chưa build lại Windows trong task này.
