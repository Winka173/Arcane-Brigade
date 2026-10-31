# Đổi đội hình và sao thứ 5 — 31/10/2026

Luật tuyển quân hiện hành (commit a23c4552): bạn mới ở wave 1, 3, 6, 9, 12, 15 và shop trước wave 20; bạn đặc biệt tối đa 3 tới wave 20; bản sao luôn 1 sao, mỗi bản +0,5 sao tới 4 sao. File này thêm bốn cách đổi đội hình.

## 1A. Tiễn về nhà (bến đò trong shop)

- Chọn một bạn: cả loài rời đội.
- Nhận lại một nửa số vàng đã tiêu (giá hôm nay × giá trị sao).
- Trước wave 20, bạn thường trả lại 1 suất bạn mới. Bạn đặc biệt thì không trả suất, nhưng bớt một bạn đặc biệt nên chợ mộng lại mời bạn mới với tỉ lệ cao hơn.
- Bạn để lại một lá thư. Cuối run, mỗi lá thư cho 10 Lamp Oil, tính tối đa 3 lá.
- Mỗi shop được tiễn một lần. Không tiễn được bạn cuối cùng, cũng không tiễn bốn bạn đồng hành của cốt truyện.

## 1B. Đò qua sông (bến đò trong shop, từ wave 2)

- Chọn một bạn, rồi chọn 1 trong 3 bạn mới đang chờ bên kia sông. Bạn mới cùng loại shop: bạn thường đổi lấy bạn thường, bạn đặc biệt đổi lấy bạn đặc biệt.
- Bạn mới giữ nguyên số sao. Mỗi bản của bạn cũ được thay bằng một bản của bạn mới với vũ khí riêng.
- Giá là 3 × hệ số giá chợ mộng, trả bằng trứng.
- Mỗi shop được chèo một lần. Ba lựa chọn cố định trong cùng một shop.
- Đò không dùng suất bạn mới. Sau wave 20, đây là cách duy nhất để đổi loài.

## 1C. Khách Cuối mua ký ức

- Từ shop 9, Khách Cuối ghé khoảng 20% số shop, ngoài lần ghé của Act I ở shop 8.
- Ông mua đứt một bạn và trả đủ số vàng bạn ấy đã tốn, tức gấp đôi khi tiễn về nhà.
- Bán cho ông thì không có thư và không được trả suất.
- Từ chối thì ông đi. Thư của ông xếp gọn trên HUD và mở lại được.

## 2D. Sợi Chỉ Sao ở chợ mộng

- Từ wave 15, mỗi lần ghé chợ mộng có khoảng 10% bán một Sợi Chỉ Sao, với giá 16 × hệ số giá chợ mộng, trả bằng trứng.
- Kết quả của mỗi lần ghé được quyết một lần, nên đổi hàng không làm tăng tỉ lệ.
- Chợ chỉ bày sợi chỉ khi bạn có một bạn từ 3 sao trở lên và chưa giữ sợi chỉ nào.
- Mua xong, một bạn 4 sao lên 5 sao ngay. Nếu chưa có bạn nào 4 sao, sợi chỉ chờ tới khi có.

## Art

`Resources/Shop/FerryStation.png`, `Resources/Shop/HomeLight.png`, `Resources/UI/Items/{StarlightThread, FerryIcon, SeeHomeIcon, FarewellLetter, MemorySale}.png`.

## Code và kiểm tra

- Code: `Runtime/Core/FarewellRules.cs`, `Runtime/Presentation/FerryDock.cs`, `CustomerOffer.cs` (thẻ của Khách Cuối có thêm chế độ mua đứt).
- Kiểm tra: `Editor/FarewellChecks.cs` đạt 124/124. Ảnh chụp mới là Item33_* trong lượt chụp playtest.

> Ảnh chụp cũ nhắc trong tài liệu này đã được xóa. Toàn bộ màn hình mới nhất nằm trong `Docs/Evidence/UIAudit/full_2026-10-31/`.
