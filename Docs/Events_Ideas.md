# Vị khách ghé shop — ý tưởng (18/10/2026, bản 4, để làm sau)

Trạng thái: **chưa làm** — chủ dự án chọn danh sách cuối. Theo góp ý: bỏ luật môi trường và bỏ biến cố giữa trận; chỉ làm
vị khách ghé shop (một nhân vật xuất hiện trong shop với một lời đề nghị, nhận hay bỏ qua).

## Luật chung

- Sau wave 3, cứ 2–3 shop có một vị khách ghé, rút ngẫu nhiên (theo seed của run), không lặp liền, không rơi vào wave boss.
- Chỉ tạm thời: trong trận đó hoặc wave kế. Không sự kiện nào cho chỉ số vĩnh viễn.
- Thưởng nhỏ: vàng 5–20, 1 trứng, hoặc 1 item Common (sự kiện khó: Uncommon từ wave 10). Nhiều sự kiện không có thưởng.
- Mỗi vị khách là một nhân vật vẽ mới đứng trong shop (một tranh); hiệu ứng vẽ bằng mực trong code.

## B. Vị khách ghé shop (nhận hay bỏ qua) — đã chốt cả 45 (18/10/2026)

Rà soát cuối: không vị khách nào hồi máu hay đổi máu lấy vàng (máu đã là tiền tệ ở quầy mua bằng máu); không hai vị khách
nào làm cùng một việc (một vị cá cược, một vị bán đồ, một vị đổi đồ, một vị hoàn lượt đổi, một vị đổi trứng).

| # | Vị khách | Đổi chác | Cần vẽ |
|---|---|---|---|
| B1 | Lời Thách Của Người Gác Cửa | wave sau +20% quái và 2 quái mạ vàng; thắng: 1 item Common (Uncommon từ wave 12) | — |
| B2 | Bưu Tá Lạc Đường | mang thư qua wave sau, bị đánh không quá 5 lần: 15 vàng | bưu tá |
| B3 | Ông Già Xúc Xắc | cược tối đa 20 vàng; qua wave sau trên 50% máu: x1.5 | ông già |
| B4 | Tiệm Cầm Đồ Ven Đường | gửi 1 item lấy nửa giá; chuộc lại trong 2 wave đúng giá đó, quá hạn thì mất | quầy |
| B5 | Bưu Kiện Không Tên | 10 vàng: 15 vàng, 1 trứng, 1 item Common, hoặc hộp rỗng kèm một mẩu thư | hộp |
| B6 | Gánh Hát Rong | chọn một vở cho wave sau: đồng đội +10% tốc đánh / quái −10% tốc độ / xu +1 | xe gánh hát |
| B7 | Người Thắp Đèn | 6 vàng: 4 đèn góc sân cháy wave sau, quái đi qua bén lửa nhẹ | — |
| B8 | Món Đồ Chơi Lạc Nhà | wave sau nó đi theo hero; giữ trong tầm 3 ô đủ 20s: 1 trứng và một câu chuyện ngắn | đồ chơi |
| B9 | Thợ Vá Đồ Chơi | 8 vàng: vá áo cho một đồng đội; wave sau đòn của nó làm chậm quái 20% | thợ vá |
| B10 | Hộp Nhạc Lên Dây | miễn phí: wave sau mọi thứ nhanh hơn 15% (cả quái), xu +1 | hộp nhạc |
| B11 | Người Đổi Chác | đổi 1 item đang có lấy 1 item ngẫu nhiên cùng bậc | người đổi chác |
| B12 | Người Gom Đồ Cũ | bán 1 item đang có lấy 60% giá | xe đồ cũ |
| B13 | Người Cho Mượn | cho mượn 1 item Rare đúng 1 wave, rồi lấy lại | người cho mượn |
| B14 | Bà Kể Chuyện | ngồi nghe (miễn phí): một trang truyện về Miền Chờ, và 20s đầu wave sau mỗi quái hạ rơi thêm 1 xu | bà kể chuyện |
| B15 | Chú Hề Tráo Hàng | tráo cả quầy shop sang hàng bậc cao hơn một bậc nhưng giá +30% | chú hề |
| B16 | Thợ Đồng Hồ Quay Ngược | 4 vàng: trả lại tiền lượt đổi shop vừa rồi và đưa quầy về như cũ | thợ đồng hồ |
| B17 | Đứa Trẻ Mất Ngủ | hát ru giúp: wave sau hạ 80 quái thì nhận 1 trứng | đứa trẻ |
| B18 | Phù Thủy Kẹo | 6 vàng một túi kẹo: wave sau cứ 10s một viên kẹo rơi cạnh hero, quái xúm vào tranh kẹo 2s | phù thủy kẹo |
| B19 | Người Bán Bóng Bay | 5 vàng một quả bóng: hết wave sau bóng mang về 1 item Common ngẫu nhiên (có thể rỗng) | người bán bóng |
| B20 | Kẻ Bán Lời Hứa | miễn phí: hứa không mua đồng đội ở shop kế; giữ lời thì shop sau nữa giảm 15% | kẻ bán lời hứa |
| B21 | Người Đưa Đò | chở một đồng đội 1★ sang bờ bên kia: đổi nó lấy một đồng đội 1★ khác cùng chủ đề trong bộ của hero | người đưa đò |
| B22 | Cô Bé Bán Diêm | 3 vàng một que diêm: 3 quái đầu tiên bị hạ ở wave sau bùng lửa nhỏ đốt quái bên cạnh | cô bé |
| B23 | Người Sưu Tầm Lạ | đặt hàng một loài quái: wave sau hạ 10 con loài đó thì nhận 12 vàng | người sưu tầm |
| B24 | Thợ Khắc Tên | 6 vàng khắc tên một đồng đội: wave sau nó +15% sát thương | thợ khắc |
| B25 | Người Đánh Thức | 6 vàng: wave sau thanh biến hình của hero bắt đầu ở 1/3 | người đánh thức |
| B26 | Người Bán Mộng | chọn 1 trong 3 giấc mơ niêm phong (miễn phí): wave sau một hiệu ứng nhỏ ngẫu nhiên, đa phần tốt, đôi khi kỳ quặc | người bán mộng |
| B27 | Hai Chị Em Sinh Đôi | đồng đội tiếp theo bạn mua ở shop này: bản thứ hai chỉ nửa giá | hai chị em |
| B28 | Người Gánh Nước Giếng | 5 vàng: wave sau bốn góc sân có vũng nước, quái đi qua bị ướt (đòn băng đóng băng được) | người gánh nước |
| B29 | Kẻ Trộm Đêm | rời shop với hơn 60 vàng thì hắn lấy mất 10%; trả 5 vàng để đuổi hắn đi | kẻ trộm |
| B30 | Bác Sĩ Búp Bê | 10 vàng: khâu áo choàng cho hero; wave sau lần đầu máu tụt dưới 25%, quái quanh hero bị hất văng mạnh (một lần) | bác sĩ búp bê |
| B31 | Người Giữ Chìa Khóa | 10 vàng một chiếc chìa: rương đầu tiên ở wave sau cho thêm 1 item Common | người giữ chìa |
| B32 | Nhà Ảo Thuật | 6 vàng: "biến" một món ngẫu nhiên trên quầy thành miễn phí | nhà ảo thuật |
| B33 | Người Đi Tìm Bạn | mượn một đồng đội đi cùng 1 wave (nó vắng mặt); quay về mang theo 1 trứng và 10 vàng | người đi tìm bạn |
| B34 | Thợ Rèn Mộng | 8 vàng: đổi vũ khí của một đồng đội, chọn 1 trong 3 | thợ rèn |
| B35 | Người Bán Bản Đồ | 6 vàng: wave sau đánh dấu sẵn chỗ rương sẽ rơi | người bán bản đồ |
| B36 | Người Đổi Trứng | 2 trứng lấy 1 item Uncommon, hoặc 1 item lấy 2 trứng | người đổi trứng |
| B37 | Người Đoán Thời Tiết | báo trước những loại quái của wave sau; bán chiếc ô 6 vàng: đạn địch wave sau giảm 20% sát thương | người đoán thời tiết |
| B38 | Cụ Bà Đan Len | 8 vàng một chiếc khăn: wave sau hero chạy nhanh hơn 15% | cụ bà |
| B39 | Họa Sĩ Vẽ Chân Dung | vẽ chân dung đội hình (miễn phí): bức tranh lưu vào bảo tàng làm kỷ niệm của run | họa sĩ |
| B40 | Kẻ Mượn Bóng | 8 vàng: hắn giữ cái bóng của hero, 5s đầu wave sau quái không nhắm vào hero | kẻ mượn bóng |
| B41 | Người Rắc Cát Ngủ | 8 vàng: 20s đầu wave sau quái ra chậm hơn 15% | người rắc cát |
| B42 | Người Lạ Đổi Màu | đổi quầy miễn phí, nhưng hàng mới thấp hơn một bậc và rẻ hơn 30% | người lạ |
| B43 | Bưu Cục Gửi Tương Lai | gửi 10 vàng hôm nay, 3 wave sau nhận lại 15 | quầy bưu cục |
| B44 | Người Sửa Đồng Hồ Cát | lật đồng hồ cát: wave sau dài thêm 10s, thêm quái và thêm xu | thợ sửa |
| B45 | Ca Sĩ Hát Rong | nghe một bài (miễn phí): bond đang gần lên cấp nhất được +1 thành viên trong đúng 1 wave | ca sĩ |
Code đã soạn sẵn cho B1 (quầy thử thách + quái tinh anh) trong scratchpad, cần hạ thưởng theo bản này khi làm.
