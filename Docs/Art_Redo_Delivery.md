# Bàn giao art vẽ lại — 24/10/2026

Đã hoàn thành toàn bộ [Art_Redo_Requests.md](Art_Redo_Requests.md), gồm cả phần acting của 10 hero. Vẽ bằng công cụ **painted**, theo nhận diện cũ và reference chất liệu của Hồi 1; tám companion từng dùng lại hình hero đã có thiết kế riêng.

| Nhóm | Ảnh PNG | Sprite riêng |
| --- | ---: | ---: |
| 10 hero và 44 companion, tám tư thế mỗi nhân vật | 54 | 432 |
| Vũ khí cầm tay | 23 | 23 |
| Acting 10 hero: tò mò, vui, bị đánh, thất bại | 10 | 120 |
| Tổng | **87** | **575** |

Ảnh cuối nằm trong `Assets/ArcaneBrigade/Art/Playtest/Redo/`. Sheet nhân vật 1024×512, acting 1024×768, ô 256×256; vũ khí 700×300. Tổng PNG: **23.455.724 byte**, khoảng **22,37 MiB**.

## Các bản sửa khi duyệt

- **Balloon Mallet:** vẽ lại tám tư thế; vòi mọc từ mặt, cuộn lên phía trước khi lấy đà rồi duỗi ra trước khi đánh. Bỏ tư thế vòi xuyên thân hoặc mọc ra phía lưng.
- **Comet Kite:** thiết kế lại mặt bằng mắt và miệng thêu trên chính lớp vải navy. Bỏ mảng mặt người sáng màu, mũi và chi tiết trông như dán từ hình khác.
- **Pillow Oracle, Brass Acorn, Comet Kite, Mailbox Imp, Mirror Newt, Bubble Koi, Jelly Swordsman, Dice Crab:** thiết kế riêng, tách nhận diện khỏi hero tương ứng.
- Sửa các bản bị cắt rìa, chồng ô, đổi hướng trái/phải hoặc có hơi thở dạng mây ngoài yêu cầu. Drowsy Seer acting chỉ giữ một ngôi sao treo ở mũ trong từng frame.

## Bảng ảnh để review

- [10 hero](../PreviewExports/ArtRedo/Heroes.jpg).
- [Companion, phần A](../PreviewExports/ArtRedo/Companions_A.jpg), [phần B](../PreviewExports/ArtRedo/Companions_B.jpg).
- [23 vũ khí](../PreviewExports/ArtRedo/Weapons.jpg).
- [120 tư thế acting](../PreviewExports/ArtRedo/Acting.jpg).

## Nhập vào Unity và kiểm tra

Unity **6000.4.8f1** chạy batch import thành công, thoát với mã 0. Kết quả: **54 character sheet, 23 weapon, 10 acting sheet; 87 ảnh / 575 sprite**. [Bằng chứng kiểm tra Unity](Art_Redo_Unity_Verification.txt).

`ArtRedoBuilder.BuildAndVerify` dùng Sprite Editor Data Provider để cắt ô, giữ sprite ID theo tên khi nhập lại và kiểm tra các liên kết. Đã cập nhật Body, AuthoredMotion, AnimationFrames, expanded gait và acting; cập nhật cả định nghĩa Run/Checkpoint trùng ID. Các alias cũ khiến companion dùng hình hero được gỡ. SpriteInkMetrics được dựng lại để đo phần hình thật thay vì lề canvas. Texture dùng alpha, bilinear, clamp và không mipmap; chính sách build của dự án áp dụng BC7/ASTC theo nền tảng.

Kiểm tra PNG độc lập: cả 87 ảnh là RGBA, bốn góc alpha 0; không cắt chủ thể; tất cả ô nhân vật có lề trong suốt tối thiểu **32 px**. Chỉ dùng xử lý ảnh để căn ô, resize và dọn alpha vụn theo quyền đã được chấp thuận; không vẽ thay nhân vật bằng code. [Kết quả PNG](Art_Redo_Verification.json), [tiến độ](Art_Redo_Progress.txt).

Các kiểm tra này xác nhận dữ liệu ảnh, cắt frame và liên kết Unity. Chưa chạy lại toàn chiến dịch hoặc tạo bản Windows mới trong đợt bàn giao này; bảng ảnh review là ảnh ghép từ PNG, không phải screenshot combat.

## Prompt và cách lặp lại

[Manifest](Art_Redo_Manifest.json) lưu prompt, reference, vùng crop nhận diện, nguồn ảnh và các prompt sửa khi duyệt. Reference crop dưới `Library/ArtRedoRefs/` là dữ liệu tạm có thể dựng lại từ `identity_reference` và `reference_crop`, không đưa cache vào Git.

`python Tools/Art/prepare_art_redo.py --refs --full --review` dựng lại crop, căn các nguồn ảnh đã tạo và xuất báo cáo/bảng review. Nguồn painted gốc được giữ tại đường dẫn ghi trong manifest trên máy vẽ; PNG cuối đã được lưu đầy đủ trong repo. Nhập art bằng menu **Arcane Brigade → Art → Put repainted characters and weapons in**, hoặc chạy `ArcaneBrigade.Editor.ArtRedoBuilder.BuildAndVerify` để nhập kèm kiểm tra liên kết.
