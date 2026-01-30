# Lơ lửng 02 — nghiệm thu hệ mới và bỏ gió dưới chân

**PASS**, Windows D3D11, 0 lỗi/0 cảnh báo build. Bộ art hiện hành đã chuyển từ Blender sang sprite vẽ theo game; xem [gate bộ tranh](../PaintedMienCho/Gate_Summary.md).

- 23234 kiểm tra tự động về khoảng nổi, độ nghiêng, lệch pha và Reduced Motion.
- Hồi 1: 480 + 480 kiểm tra thường/giảm hiệu ứng, gồm pose đèn ở nhiều thời điểm, pause, bóng neo sàn, nguồn sáng bám sprite, đèn vỡ, các vật chứng và ghế.
- Shop cũ: 432 + 336 kiểm tra, 9 ảnh mỗi chế độ; chuyển động merchant/bệ, hàng bám bệ, aura rarity, mua và qua cửa đều chạy lại.
- Hồi quy truyện, cảnh bàn ăn/điện thoại/gương và hai ending: 258 kiểm tra, 50 ảnh.

Cộng cả kiểm tra Editor dữ liệu: **29836 qua / 102 ảnh**, giữ hash Player và source trong [Verification.json](Verification.json). 18 ảnh shop được kiểm tra nonblack sau khi cửa sổ chụp được đặt ngoài màn hình; không dùng ảnh đen của lần thử trước làm bằng chứng.

Đã bỏ toàn bộ renderer khói/vệt gió dưới đồ vật. Giữ nhấp nhô, nghiêng chậm lệch nhịp, bóng tách sàn và hạt sáng nhẹ phía trên đáy vật. Các đèn mới, ghế Miền Chờ và vật chứng từng bị sót đã được nối vào hệ chung. Reduced Motion giữ khoảng nổi đứng yên; Reduced Effects bỏ hạt. Điểm tương tác cố định.

[Phạm vi và giới hạn](../../Docs/Dream_Levitation_02.md), [ảnh shop](Shop/Native/suspended-shop-later.png), [đèn](../ActOne/Native/lamps-and-fog-wave-1.png). Vật vẽ liền nền phòng không thể nghiêng riêng; hero/địch/đạn và sàn không thuộc chuyển động đồ cảnh. Các con số là kiểm tra tự động, không thay thế duyệt thẩm mỹ của người chơi.
