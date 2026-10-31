# Danh sách vẽ lại và sửa luồng UI

Ngày 31/10/2026. Danh sách dựa trên ba nguồn:

- Ảnh chụp 62 màn sau đợt sửa UI: `Docs/Evidence/UIAudit/after_*.jpg`.
- Kho art UI hiện có: 124 file trong `Resources/UI`.
- Ba bộ nguyên tắc UI/UX:
  - **Taste**: nhìn một giây phải thấy một điểm nhấn; một màu nhấn duy nhất; không nhãn chữ hoa nhỏ; không khung trống; luôn có nút chính rõ ràng.
  - **Hallmark**: không nhãn chữ hoa nhỏ đè lên tiêu đề; mỗi màn khoảng 80 chữ; không lồng thẻ trong thẻ; nút nào cũng có đủ trạng thái.
  - **Frontend UI engineering**: có đủ trạng thái đang tải, trống và lỗi; vùng bấm đủ lớn; màu không phải tín hiệu duy nhất.

Tài liệu này ban đầu chỉ liệt kê.

**Tình trạng:**

- **Đợt 1 (commit 699b6d2e):** đã vẽ và gắn A1–A5.
- **Đợt 2 (31/10/2026):** đã vẽ và gắn A6–A12:
  - A6: bảng Bonds giấy da, có con dấu sáp Lv và con dấu vàng lá New.
  - A7: đế đồng hồ wave.
  - A8: tấm nhãn nhung dưới chân dung anh hùng.
  - A9: khung 9 mảnh cho Common, Rare, Mythic và Crimson. Uncommon và Rainbow vẫn giữ khung cũ. Bỏ vệt sáng chạy theo cạnh thẳng vì cắt ngang viền vẽ tay.
  - A10: băng gợi ý có ổ tròn để đặt icon.
  - A11: bong bóng lời thoại, tách thành thân và tab tên co giãn theo độ dài tên, kèm đuôi vẽ tay. Lời thoại tông Boss và Dream vẫn giữ giấy vẽ bằng code, vì không được tô màu lên art.
  - A12: một ổ khóa vẽ tay dùng chung, thay `ArcadeSkin.Lock`.
- **Chứng cứ:** `Docs/Evidence/UIAudit/kit_wave2.jpg`.
- **Đợt 3 (31/10/2026):** đã vẽ và gắn A13–A16:
  - A13: ô tủ trưng bày trong Collection, kính sáng khi đã gặp, kính bụi khi chưa gặp; icon to hơn.
  - A14: tờ tổng kết giấy da, thanh tỉ lệ rãnh gỗ có phần lấp vàng, ổ huy chương cho ba thành tích.
  - A15: tranh màn loading, sảnh đèn lồng.
  - A16: biển gỗ treo xích cho tên các trạm.
- **Chứng cứ đợt 3:** `Docs/Evidence/UIAudit/kit_wave3.jpg`.
- **Sửa thêm:**
  - C6: khi không có lượt chơi dở, nút "Nothing to continue" là một tấm mảnh dưới New Journey.
  - B4: chú thích độ khó ở Hero select còn một dòng.

## A. Art UI cần vẽ lại hoặc vẽ thêm

Ưu tiên theo mức người chơi thấy thường xuyên.

| # | Thành phần | Hiện tại | Cần vẽ |
| --- | --- | --- | --- |
| A1 | Nút kit: Primary, Secondary, Danger | Đầu nút trang trí to, ăn mất chỗ của nút hẹp. Chữ "−10%" và "Smaller" từng bị co nhỏ. | Một bộ nút gọn có đầu nút mảnh cho nút nhỏ (rộng 140–220 u), đủ 4 trạng thái: thường, rê, nhấn, khóa. |
| A2 | Hàng nút chia đoạn (Settings, Lamp Tree) | Dùng nút thường, lựa chọn đang chọn chỉ có vạch vàng vẽ bằng code. | Ba mảnh trái, giữa, phải, mỗi mảnh có trạng thái thường và đang chọn (viền và nền vàng). |
| A3 | Tab (`TabActive`, `TabInactive`, `TabHover`, `TabDisabled`) | Đầu tab thò ra ngoài. Tab hẹp thì chữ co nhỏ, như 9 tab của Lamp Tree. Tab đang chọn khó phân biệt. | Tab 3 mảnh có đầu mảnh, tab đang chọn khác hẳn (nền đỏ hoặc vàng, nhô lên). |
| A4 | Tấm bảng tiêu đề (`Panels/Plaque`) | Dùng cho mọi tiêu đề trang, nhưng là một ảnh duy nhất bị kéo giãn theo độ dài tiêu đề. | Bảng 3 mảnh: hai đầu cố định và phần giữa lặp được, để tiêu đề dài không bị méo hoa văn. |
| A5 | Thanh trượt âm lượng (`SliderTrack`, `SliderKnob`) | Núm quá nhỏ cho ngón tay. | Núm to khoảng 64 u trên khung 720 (khoảng 96 u ở 1080), rãnh dày hơn. |
| A6 | Bảng bond trong HUD | Giấy vẽ bằng code (`Ticket`), nhãn Lv và New là hình xiên đơn giản. | Tấm giấy da có dải đầu, đường chia hàng, con dấu Lv và New vẽ tay. |
| A7 | Đồng hồ wave | Tấm `Plate` vẽ bằng code. | Đế đồng hồ vẽ tay, ăn khớp với `WaveBanner`. |
| A8 | Dòng tên hero và gauge ở HUD trái | Chữ nằm thẳng trên sàn, chỉ có bóng đổ. | Một tấm nhãn nhỏ vẽ tay phía sau hai dòng, hợp với ô chân dung. |
| A9 | Khung thẻ theo độ hiếm (`RarityDescriptionFrame`) | Khung vẽ bằng code. Ở màn chi tiết, khung tím rất lớn. | Khung 9 mảnh vẽ tay cho từng độ hiếm (Common, Rare, Mythic, Crimson), co giãn được. |
| A10 | Băng gợi ý | `Ticket` vẽ bằng code. | Băng giấy vẽ tay có chỗ cho icon, co giãn ngang được. |
| A11 | Bong bóng lời thoại | Vẽ bằng code, tên người nói nằm trong bong bóng. | Bong bóng 9 mảnh có tab tên riêng, đuôi bong bóng vẽ tay. |
| A12 | Ô khóa | `ArcadeSkin.Lock` vẽ bằng code, nhìn khác ổ khóa trên lá thư Postbox. | Một icon khóa vẽ tay dùng chung cho Museum, Lamp Tree, Hero select, Postbox. |
| A13 | Ô vật phẩm trong Collection | Ô tối vẽ bằng code, icon nhỏ giữa ô to. | Ô tủ trưng bày vẽ tay (đã thấy và chưa thấy), icon to hơn. |
| A14 | Bảng tổng kết run | Giấy và thanh tỉ lệ vẽ bằng code. | Tấm giấy da có dải tiêu đề, thanh tỉ lệ vẽ tay, ba ô thành tích. |
| A15 | Màn loading | Nền tối, tên game, một nhân vật, hàng chấm. | Một tranh nền loading (đèn lồng thắp dần), giữ đúng không khí game. |
| A16 | Biển tên các trạm trong Waiting Room | Nhãn `Tag` nhỏ trên sàn. | Biển gỗ treo vẽ tay, chữ lớn hơn. |

Ghi chú khi vẽ:

- Giữ đúng style tranh cũ: viền mực ấm, đổ bóng vẽ tay, màu gỗ, đồng thau, đỏ vang, giấy da.
- Mọi mảnh có thể co giãn (A1–A4, A9–A11) cần vẽ theo dạng 3 mảnh hoặc 9 mảnh, có ghi rõ số đo lề.
- Không tô màu đè lên tranh vẽ, không vẽ hình chữ nhật đè lên khung có hình dạng riêng.

## B. Phần UI cần sửa thêm bằng code

1. **Chữ trên các màn phụ:** nhãn, chip và tag đang ở 18–24 u trên khung 1920, chưa đạt 28. Phải bố cục lại từng khung cố định: vé giá shop, thẻ khách ghé, thẻ xem nhanh, phòng bí mật, bàn thờ, trang chi tiết đội hình.
2. **Lỗi lưu trong trận:** `owner.SaveError` vẫn hiện nguyên văn câu kỹ thuật ở đầu màn chiến đấu. Cần đổi sang câu dễ hiểu và một lựa chọn, như đã làm ở menu.
3. **Main menu có hai nút vàng** (New Journey và Waiting Room). Quy chuẩn chỉ cho một nút chính mỗi màn, nên Waiting Room phải chuyển sang nút phụ.
4. **Hero select:**
   - Chân dung hero nhỏ trong ô to.
   - Phần mô tả bên phải vượt khoảng 80 chữ và phải cuộn.
   - Chú thích độ khó xuống hai dòng.
5. **How to Play:** mũi tên chuyển trang và các chấm trang nhỏ, chưa đủ cho ngón tay.
6. **Records và trang tạm dừng (trừ Settings):** chưa có ảnh chụp, chưa rà soát. Cần thêm vào công cụ chụp.
7. **Trạng thái trống:** cần kiểm tra Collection, Records, Museum khi chưa có gì. Theo quy chuẩn, mỗi màn trống phải nói vì sao trống và làm gì để có.

## C. Luồng chơi (flow) cần sửa

1. **Từ Waiting Room bắt đầu một đêm mới mất ba bước:** cửa ghi "MAIN MENU ▶", về main menu, rồi bấm New Journey. Mũi tên ▶ lại gợi ý "đi tiếp" chứ không phải "quay về".
   - Đề xuất: cửa đưa thẳng sang Hero select với nhãn "New journey".
   - Nút quay lại góc trái vẫn về main menu.
2. **Hết run, muốn tiêu Lamp Oil:** bảng tổng kết bảo "Spend them in the Waiting Room, at the Lamp Tree", nhưng chỉ có nút Play again và Main menu.
   - Đề xuất: thêm nút "Waiting Room", mở thẳng Lamp Tree khi có thứ mua được.
3. **Mở Collection từ Museum rồi bấm Back thì về main menu**, không về Museum. Back phải quay lại nơi đã mở.
4. **Lamp Tree:** mở lần đầu là cả cây với nút nhỏ. Trên điện thoại nên mở thẳng nhánh vừa dùng hoặc nhánh có thứ mua được.
5. **Nhãn có nút ▶:** "MAIN MENU ▶" và "STEP THROUGH ▶" dùng cùng ký hiệu nhưng một nút là quay về, một nút là đi tiếp. Chỉ nút đi tiếp mới có ▶.
6. **Continue khi chưa có lượt chơi dở:** nút "NOTHING TO CONTINUE" chiếm vị trí thứ hai trên main menu. Giữ cho trung thực nhưng thu nhỏ và hạ xuống dưới New Journey; hoặc khi có lượt dở thì Continue thành nút chính.

## D. Thứ tự đề xuất

1. **Sửa bằng code, không cần vẽ:**
   - B2 (lỗi lưu trong trận);
   - B3 (hai nút vàng ở main menu);
   - C1, C2, C3, C5 (luồng chơi).

   Nhỏ, rủi ro thấp, người chơi thấy ngay.
2. **Vẽ:** đợt 1 gồm A1–A5, là các thành phần dùng ở mọi màn. Đợt 2 gồm A6–A12. Đợt 3 gồm A13–A16.
3. **Sau khi có art:** làm B1 (chữ 28 trên các màn phụ) cùng lúc thay khung mới, vì đằng nào cũng phải bố cục lại.

> Ảnh chụp cũ nhắc trong tài liệu này đã được xóa. Toàn bộ màn hình mới nhất nằm trong `Docs/Evidence/UIAudit/full_2026-10-31/`.
