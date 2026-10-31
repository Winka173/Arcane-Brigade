# Quy chuẩn UI

Bản quy chuẩn này áp dụng cho mọi màn của Arcane Brigade. Nó dựa trên bản rà soát `Docs/UI_Audit_2026-10-31.md` và các quy tắc UI đã thống nhất trước đó. Màn nào làm lại cũng phải qua danh sách kiểm tra ở cuối tài liệu.

## 1. Khung vẽ

- **Khung thiết kế 1920×1080.** Mọi số đo trong tài liệu này là đơn vị của khung đó (gọi tắt là u).
  - `ArcadeTypography.BeginViewport` co khung vào vùng an toàn `Screen.safeArea` của từng máy.
- **Điện thoại là chuẩn đo.** Trên điện thoại 6,1 inch nằm ngang, 1 pt của hệ điều hành bằng khoảng 2,8 u. Mọi cỡ chữ và vùng bấm dưới đây đã tính theo con số đó.
- **Lề ngoài 48 u** ở cả bốn cạnh. Không đặt chữ, nút hay icon nào sát mép hơn 48 u.
- **Thang khoảng cách:** 8, 16, 24, 32, 48, 64, 96. Không dùng số lẻ ngoài thang này.

## 2. Chữ

### Phông: hai họ và một kiểu chữ tay

| Vai | Phông | Dùng cho |
| --- | --- | --- |
| Tiêu đề | Fraunces (Heading, Display) | Tên màn, tên thẻ, tên nhân vật |
| Nội dung | Alegreya Sans, kể cả bản số | Mọi chữ còn lại, kể cả số. Số dùng chữ số đều cột. |
| Chữ tay | Caveat | Chỉ cho lời thoại và nội dung thư |

- **Bỏ** Lilita One (Punch), Be Vietnam Pro, Noto Sans, Coiny và Young Serif khỏi UI.
- **Riêng số sát thương trong trận** được giữ một kiểu chữ đậm, vì đó là hiệu ứng chứ không phải chữ để đọc.

### Thang cỡ: chỉ năm cỡ

| Tên | Cỡ (u) | Khoảng bằng | Dùng cho |
| --- | --- | --- | --- |
| Ghi chú | 28 | 10 pt | Chú thích phụ, số đếm nhỏ. Không có chữ nào nhỏ hơn cỡ này. |
| Nội dung | 34 | 12 pt | Mô tả, dòng chỉ số, nhãn nút phụ |
| Mục | 44 | 16 pt | Tên thẻ, tên mục, nhãn nút chính |
| Tiêu đề | 64 | 23 pt | Tên màn, tên popup |
| Trưng bày | 110 | 39 pt | Tên game, con số lớn ở giữa màn (wave, đếm ngược) |

- **Mỗi màn chỉ dùng tối đa ba cỡ:** một cỡ lớn, một cỡ nội dung, một cỡ ghi chú.
- **Viết hoa đầu câu**, không viết hoa cả dòng. Chữ hoa toàn bộ chỉ dùng cho nhãn ngắn trên nút, tối đa hai từ.
- **Không đặt nhãn chữ hoa nhỏ phía trên tiêu đề** (kiểu "THE WAITING PLACE · NEW JOURNEY"). Thông tin cần thiết đưa vào chính tiêu đề, còn lại thì bỏ.
- **Mỗi khối chữ tối đa khoảng 25 từ, ba dòng.** Dài hơn thì chia mục hoặc chuyển vào màn chi tiết.
- **Chữ nằm thẳng trên tranh** phải có bóng tối (`ShadowLabel`) hoặc một nền.

## 3. Màu

Màu lấy từ `ArcadeSkin`. Không viết mã màu trực tiếp trong code màn.

| Token | Vai |
| --- | --- |
| `Ink`, `Deep`, `Surface` | Nền và panel |
| `Cream` | Chữ chính trên nền tối |
| `Gold` | Màu nhấn. Chỉ dùng cho nút chính, phần thưởng và thứ được chọn. Mỗi màn chỉ một vùng vàng nổi bật. |
| `Teal` | Thông tin phụ, companion |
| `Danger` | Chỉ cho kẻ địch và thứ có hại |
| `Better`, `Worse` | Chỉ số tăng, giảm |
| Màu độ hiếm | Chỉ dùng trên viền và nhãn độ hiếm |

- **Chữ nội dung** luôn dùng màu chữ chính. Không dùng xám nhạt cho chữ cần đọc.
- **Không dùng màu làm tín hiệu duy nhất.** Tăng giảm phải kèm dấu + hoặc −, trạng thái khóa phải kèm icon khóa.

## 4. Thành phần chung

### Tiêu đề màn: một kiểu

Dùng dải ruy băng đỏ vẽ tay của Waiting Room, đặt ở giữa phía trên, chữ Fraunces cỡ 64. Áp dụng cho mọi màn toàn trang và mọi popup lớn: menu, Settings, Collection, Hero select, Combos, tổng kết run.

### Khung: hai kiểu

1. **Panel hệ thống:** nền xanh đậm, viền vàng vẽ tay. Dùng cho danh sách, cài đặt, chi tiết.
2. **Giấy da:** dùng cho thứ có thật trong thế giới game, như thư, thẻ vật phẩm, hóa đơn, tổng kết run.

Bỏ các khung tím và gỗ nâu. Khung vẽ tay không được tô màu, không vẽ hình chữ nhật đè lên; nội dung phải theo đúng hình của khung.

### Nút: ba kiểu, một nút chính mỗi màn

| Kiểu | Hình | Dùng cho |
| --- | --- | --- |
| Chính | Vàng | Hành động chính. Mỗi màn chỉ có một nút chính. |
| Phụ | Gỗ | Quay lại, chi tiết, lựa chọn khác |
| Khóa | Tối, có icon khóa | Kèm một dòng ghi chú nói cách mở. Không bao giờ để trống. |

- **Nút đỏ (Danger)** chỉ dùng khi trả bằng máu.
- **Vùng bấm tối thiểu cao 110 u.** Nút icon nhỏ có thể vẽ nhỏ hơn, nhưng vùng bấm vẫn phải đủ 96 × 96 u.
- **Mỗi nút có đủ năm trạng thái:** thường, rê chuột, nhấn, khóa, đang chọn.
- **Nút đóng (X)** nằm trong góc phải trên của khung, cách mép trong 24 u.

### Popup

- **Mở popup** bằng `LivingUI.Pop`, kèm một lớp nền tối phủ phía sau.
- **Mỗi lúc chỉ một popup.**
- **Popup co theo nội dung.** Không có khung lớn mà bên trong trống.
- **Mục đã đóng hoặc đã từ chối** vẫn phải mở lại được.

### Lời thoại và gợi ý

- **Bong bóng lời thoại** tự co giãn theo chữ, không bao giờ cắt chữ, luôn bám theo người nói.
- **Gợi ý có một vùng riêng** ở giữa phía trên, ngay dưới số wave. Mỗi lúc chỉ một gợi ý.
- **Gợi ý không đè lên** HUD, bong bóng lời thoại hay biển tên map. Gợi ý mới thay gợi ý cũ, không chồng lên nhau.
- **Không có gợi ý về phím bấm.** Game chơi trên điện thoại trước.

### Điều chỉnh giá trị

- **Không dùng nút bấm để xoay vòng giá trị.**
- **Lựa chọn ít giá trị** dùng một hàng nút chia đoạn, nút đang chọn có viền vàng.
- **Âm lượng** dùng thanh kéo có núm to, cao ít nhất 96 u.

### Số trong trận

Số sát thương gần nhau gộp thành một số, hoặc xếp lệch nhau. Không để chồng lên nhau thành một cục không đọc được.

### Lỗi và trạng thái

- **Không hiện thông báo kỹ thuật** (exception) cho người chơi. Mỗi lỗi có một câu dễ hiểu và một lựa chọn rõ.
- **Màn trống** phải nói vì sao trống và cách làm cho có.
- **Mục khóa** luôn có icon khóa và điều kiện mở.

## 5. Vùng HUD trong trận (1920×1080)

| Vùng | Nội dung |
| --- | --- |
| Trái trên | Máu, vàng, vé. Chữ cỡ 34. Không đặt dòng phụ nào bên dưới. |
| Giữa trên | Số wave và thời gian, ngay dưới là vùng gợi ý |
| Phải trên | Bảng bond, chữ cỡ 28. Mặc định thu gọn thành icon cùng số đếm, chạm vào để mở. |
| Đáy | Thẻ shop và thanh boss. Cách mép đáy 48 u. |

Dòng "THE GUIDING LAMP / COMPANY LIGHT · LEVEL" chuyển vào HUD trái, bỏ chữ hoa nhỏ.

## 6. Danh sách kiểm tra cho mỗi màn

Một màn chỉ được tính là xong khi chụp ở 1920×1080 và ở kích thước điện thoại mà vẫn đạt đủ các điểm sau:

- [ ] Không có chữ nào nhỏ hơn 28 u. Màn dùng tối đa ba cỡ chữ.
- [ ] Không chữ nào bị cắt, xuống dòng giữa từ hay chồng lên chữ khác.
- [ ] Lề ngoài 48 u. Icon và chữ không dính sát mép.
- [ ] Đúng một tiêu đề theo kiểu chung và đúng một nút chính.
- [ ] Chỉ dùng hai kiểu khung, ba kiểu nút và màu theo token.
- [ ] Không có nhãn chữ hoa nhỏ phía trên tiêu đề, không có gợi ý phím.
- [ ] Popup mở bằng `LivingUI.Pop`. Khung co theo nội dung.
- [ ] Mục khóa có icon và điều kiện. Lỗi có câu dễ hiểu.
- [ ] Vùng bấm cao ít nhất 110 u (nút icon 96 u).
