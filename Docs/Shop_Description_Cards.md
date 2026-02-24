# Mô tả shop có icon và thông tin companion

Cập nhật 17/10/2026 theo hai ảnh tham khảo Bizarre Brigade. Bố cục dùng phong cách kính tối/ánh đèn của Miền Chờ; tên, số liệu và cơ chế lấy từ catalog Arcane Brigade hiện tại.

## Nội dung đã triển khai

- Thẻ companion có portrait, rarity, cấp sao, giá và icon tiền tương ứng. Hàng đầu hiển thị icon vũ khí, khoảng damage/cycle/reach, tất cả icon bond, số cơ thể hiện có/mốc kế tiếp, phạm vi tác dụng và thưởng tích lũy ở mốc đó. Không giới hạn ở hai bond.
- Vũ khí vẫn roll khi tuyển. Khoảng tấn công được biên dịch từ các vũ khí còn hợp lệ với build hiện tại, gồm bước cân bằng và talent hiện có; không tiêu RNG, không hứa một vũ khí/chỉ số cố định. Đây là thông số trước giao dịch; tuyển/ghép và talent nhận khi mua có thể thay đổi kết quả sau đó.
- Phần thân có đầy đủ hỗ trợ theo cấp sao và scope, talent, passive cá nhân, điều kiện, giới hạn và quy tắc ghép. INFO thêm tiểu sử, nguồn nhận, giải thích vũ khí và các mốc thưởng bond. Có cuộn để đọc hết các mô tả dài.
- Item có icon, loại DRINK/FOOD/CURIO, rarity, phạm vi và số liệu từng modifier; giữ hiệu ứng wave kế tiếp, trigger, chance, cooldown, stack limit, điều kiện vũ khí và phản ứng mua theo đội hiện tại.
- Giá trị có lợi màu xanh, bất lợi màu cam đỏ dịu; số điều kiện/thời lượng màu vàng. Giá trị modifier có scope được tô theo ý nghĩa thực, kể cả giảm stat địch. Chữ số đậm, giữ phần lẻ cần thiết của tooltip. Quy tắc số nguyên của thông báo stat nổi không thay đổi.
- Bộ StatGlyph bao phủ cả 49 ItemEffect; bổ sung hình dễ nhận biết cho tim, cung, giày, rương, cỏ may mắn, né tránh, giảm giá, thu nhập, rarity, va chạm và các hệ số correction. Dùng sprite bond/vật phẩm/vũ khí có sẵn; không thêm emoji phụ thuộc font.
- Click vào hàng trên map mở cùng mô hình nội dung, có cuộn và đóng; thẻ hàng nằm trên bóng thoại merchant để nút đóng đọc/bấm được. Các từ stat vẫn mở giải thích.

## Sửa nội dung sai hoặc dễ hiểu nhầm

- Đếm bản trùng theo đúng cấp sao đang bán, thay vì luôn đếm sao 1.
- Phân biệt tuyển thẳng một bạn sao cao với giao dịch thật sự ghép nâng sao.
- Không mô tả sao 3 có thể lên sao 4; Evil giữ ngoại lệ không ghép.
- Bỏ việc chỉ lấy hai dòng modifier ở bản mô tả delta cũ.
- Glossary bond dùng threshold riêng của catalog; bỏ khẳng định chung mọi bond đều có mốc 2/4/6.
- Bỏ câu mô tả Thorn Treasurer còn bị giới hạn số cơ thể.

Không sao chép con số của ảnh tham khảo: ví dụ Gilt Orbit dùng flat ranged damage và Treasure Detection của chính game. Tiểu sử companion xuất hiện trong INFO; không tự tạo thêm lore vật phẩm khi catalog chỉ có mô tả cơ chế.

## Kiểm tra

- [Kiểm tra dữ liệu](../PreviewExports/ShopDescriptions/Checks.txt): 1.701 kiểm tra qua, gồm mọi companion ở sao 1/2/3 và mọi item, icon, tất cả modifier/scope, talent, tiểu sử, threshold bond, tính bất biến của save/RNG khi xem và các trường hợp sao cao/quota.
- [Windows build](../PreviewExports/ShopDescriptions/Build.txt): thành công, 0 lỗi. Có 20 warning từ các thành phần hiện có của dự án.
- [Windows 1080p](../PreviewExports/ShopDescriptions/Native/Run.json) và [Windows 720p giảm hiệu ứng](../PreviewExports/ShopDescriptions/Reduced720/Run.json): mỗi lượt 23 kiểm tra và 13 ảnh; shop thường/đặc biệt, companion sao 1/3, item có đổi chác, HP cost, INFO đầu/cuối, thẻ click và toàn bộ icon.
- Ảnh: [companion và ba bond](../PreviewExports/ShopDescriptions/Native/Regular-0-summary.png), [item](../PreviewExports/ShopDescriptions/Native/Regular-1-summary.png), [companion sao 3](../PreviewExports/ShopDescriptions/Native/Dream-three-star-companion.png), [720p](../PreviewExports/ShopDescriptions/Reduced720/Regular-0-summary.png).
- [Bản xuất nội dung catalog](../PreviewExports/ShopDescriptions/Catalog_Descriptions.md) lưu nội dung và token icon để đối chiếu. Token được renderer chuyển thành icon/số màu, không hiện nguyên dạng trong game.

Player kiểm tra: `Builds/ShopDescriptions/ArcaneBrigade.exe`. Chạy lại bằng `ArcaneBrigade.Editor.ShopDescriptionChecks.VerifyAndBuild`, sau đó `Tools/Art/Capture_Descriptions.ps1`. Diagnostic dùng thư mục save riêng; không ghi đè profile người chơi. Các kiểm tra xác nhận mô tả và trạng thái được chạy, không thay thế kiểm thử toàn bộ cân bằng dài hạn.
