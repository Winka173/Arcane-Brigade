# Lơ lửng — đợt sửa toàn hệ, 16/10/2026

**Cập nhật art tiếp theo:** các đèn và đồ cảnh Miền Chờ đã chuyển sang sprite vẽ mới; không còn nhánh mesh Blender/Point Light cho đèn. Halo và field Unity bám điểm lửa trên sprite sau khi nghiêng. Nguồn và nghiệm thu hiện hành: [Mien_Cho_Painted_Art_Pass.md](Mien_Cho_Painted_Art_Pass.md). Mô tả mesh bên dưới ghi lại bước sửa trước khi đổi art.

Đợt rà lại phát hiện hệ cũ đã áp dụng cho đồ cảnh gốc nhưng **tám đèn cổ mới, ghế Miền Chờ và vật chứng trong phòng khám phá còn đứng yên**. Bản này sửa các phần bị sót và làm lại chuyển động dùng chung. Theo phản hồi mới nhất, **bỏ toàn bộ khói và vệt gió dưới đồ vật**, kể cả ở hệ cũ; giữ khoảng nổi, nghiêng nhẹ, bóng sàn và hạt sáng nhỏ quanh mép trên.

## Chuyển động và hiệu ứng

- Các vật có chu kỳ và pha riêng, trôi ngang chậm, nhấp nhô hai nhịp và nghiêng có hướng; không cùng lên xuống như một hàng piston. Khoảng nổi gốc khoảng 0,26–0,59 đơn vị world, độ nghiêng tối đa 4,35°, trước hệ số riêng theo loại vật.
- Không tạo renderer khói hoặc các đường cong dưới chân. Hai hạt sáng nhỏ trôi hai bên, cao hơn đáy vật; tắt khi giảm hiệu ứng hoặc detail budget thấp. Asset khói cũ vẫn được giữ trong dự án làm lịch sử, không render trong hệ lơ lửng.
- Bóng riêng neo tại điểm sàn, độ mềm thay đổi theo khoảng nổi. Độ nghiêng không xoay điểm tương tác; hàng hóa vẫn bám bệ đang di chuyển.
- Đèn nguyên/nứt/vỡ cùng nổi. Với mesh Blender, halo và Point Light bám tâm hình học ngọn lửa sau khi nghiêng; field nền đi theo nguồn đó. Bóng và vùng sương đen do đèn vỡ vẫn neo tại vị trí sàn. Shop chỉ hiện hai đèn mép ngoài, ẩn cả hạt sáng/bóng của sáu đèn còn lại. Sương đen môi trường của Miền Chờ vẫn là hệ riêng.
- Bàn/điện thoại/gương được tăng độ nổi và nghiêng để đọc rõ chủ ý; hình phản chiếu và giọt sáng nghiêng cùng gương.

## Phạm vi rà lại

| Nhóm | Xử lý |
| --- | --- |
| Tám landmark gốc, gồm tủ, ghế, đèn cũ, rèm/cửa sổ và kỷ vật | Dùng chuyển động mới qua hệ chung, bỏ gió dưới chân |
| Merchant, bệ hàng, reroll, thảm cuộn và cửa | Giữ choreography, nâng chất lượng hệ chung; hàng và rarity theo bệ |
| Invitation, artifact, phần thưởng Keeper/rainbow | Giữ anchor tương tác, dùng hệ chung |
| Rương, tim, trứng chờ nhặt | Dùng hệ chung; hút/nhặt vẫn theo simulation |
| Kiến trúc ngoại vi, cờ và đồ vật farewell | Thêm phần hiệu ứng bao quanh còn thiếu |
| Tám đèn cổ Blender | Thêm toàn bộ pose nổi/nghiêng, bóng và đồng bộ nguồn sáng |
| Ghế cốt truyện | Thêm bóng và chuyển động nổi |
| Ba vật chứng trong mỗi phòng, ghế phòng hoàn hảo | Thêm hệ nổi; lựa chọn đổi điểm neo ghế, đọc chứng cứ dừng animation |

Nền sàn và đồ vật đã vẽ liền trong ảnh nền phòng không phải các object rời có thể nghiêng độc lập. Hero, companion, địch, đạn, telegraph và dấu dẫn đường giữ cơ chế thể hiện riêng. Xu giữ bounce/spin/hút có sẵn. Người ở cảnh kết thúc vẫn đứng sàn theo chủ ý kể chuyện.

Pause dừng chuyển động, hạt và ánh đèn. Reduced Motion giữ khoảng nổi cố định, bỏ trôi/nghiêng; Reduced Effects bỏ hạt. Không thêm collider hoặc thay giá, quota, damage, đường đi.

## Bằng chứng

Build và ảnh mới: [nghiệm thu Levitation-02](../PreviewExports/Levitation02/Gate_Summary.md). Không dùng số nghiệm thu của bản levitation cũ để xác nhận các đèn mới. Art đèn vẫn là mesh/ảnh Cycles đã dựng qua Blender MCP; đợt sửa này thay runtime Unity, không tạo thêm asset Blender hoặc ảnh.
