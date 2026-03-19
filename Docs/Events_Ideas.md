# Sự kiện ngẫu nhiên — ý tưởng (18/10/2026, để làm sau)

Trạng thái: **chưa làm** — chủ dự án sẽ chọn danh sách cuối cùng. Bản này viết lại từ đầu sau góp ý: hợp theme Miền Chờ,
không có tác dụng vĩnh viễn, phần thưởng thấp, không bắt vẽ lại quái.

## Luật chung

- Sau wave 3, cứ 3–4 wave rút ngẫu nhiên một sự kiện (theo seed của run), không lặp liền, không rơi vào wave boss.
- Chỉ tạm thời: trong trận đó hoặc wave kế. Không sự kiện nào cho chỉ số vĩnh viễn.
- Thưởng nhỏ: vàng 8–20, hoặc 1 trứng, hoặc 1 item Common (sự kiện khó: Uncommon từ wave 10). Nhiều sự kiện không có thưởng,
  chỉ là một khoảnh khắc chiến thuật. Tổng mỗi run khoảng 30–50 vàng và 1–2 item Common/Uncommon.
- Mỗi sự kiện cần tối đa một vật thể vẽ mới (gửi bên art); quái và đồng đội giữ nguyên hình; hiệu ứng vẽ bằng mực trong code.

## A. Giấc mơ chen vào giữa trận (bất ngờ)

| # | Sự kiện | Diễn ra | Thưởng | Cần vẽ |
|---|---|---|---|---|
| A1 | Va Li Thất Lạc | va li mọc chân chạy quanh sân; trúng 5 lần trước khi nó thoát | 1 item Common (Uncommon từ wave 10) | va li có chân |
| A2 | Thư Bay Theo Gió | vài lá thư trôi ngang sân; hero đi qua để nhặt | 2 vàng mỗi lá, tối đa 10 | — |
| A3 | Ngọn Đèn Tắt | 8s sân tối, chỉ còn vùng sáng quanh hero; quái ngoài vùng sáng chậm 30% | — | — |
| A4 | Đồng Hồ Báo Thức | đứng cạnh 2s để tắt chuông: quái trên màn hình ngủ 2s, rồi một đợt quái nhỏ ùa tới | — | đồng hồ |
| A5 | Bong Bóng Giờ Tắm | bong bóng xà phòng trôi qua; quái chạm vào bị nhốt 3s | — | — |
| A6 | Giờ Ngủ Gật | 6s đạn của quái bay chậm như mơ | — | — |
| A7 | Đoàn Diễu Hành Đồ Chơi | đoàn lính chì băng qua một làn, giẫm bẹp quái; hero phải tránh làn | — | lính chì |
| A8 | Diều Lạc Mang Bưu Phẩm | diều kéo túi quà bay qua; bắn rơi trước khi nó đi mất | 1 trứng | diều + túi |
| A9 | Bánh Sinh Nhật Bị Bỏ Quên | quái bị hút tới ăn bánh 4s; đánh vỡ bánh | 1 món ăn Common | bánh kem |
| A10 | Gối Rơi | hero bước lên gối: lông vũ bung ra hất lùi quái quanh gối | — | gối |
| A11 | Rèm Sân Khấu | tấm rèm kéo ngang sân, gạt quái nhỏ dồn về một phía | — | rèm |
| A12 | Ánh Đèn Sân Khấu | 10s luồng đèn đuổi theo hero; quái hạ trong luồng đèn rơi thêm 1 xu | xu nhỏ | — |

## B. Vị khách ghé shop (nhận hay bỏ qua)

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
| B9 | Thợ Vá Đồ Chơi | 10 vàng: wave sau 3 đòn đầu trúng hero không gây sát thương | thợ vá |
| B10 | Hộp Nhạc Lên Dây | miễn phí: wave sau mọi thứ nhanh hơn 15% (cả quái), xu +1 | hộp nhạc |

## C. Đêm lạ (báo trước, chỉ đúng wave đó)

| # | Đêm | Luật |
|---|---|---|
| C1 | Mưa Phùn | cả wave mọi thứ ướt: đòn băng đóng băng ngắn |
| C2 | Giấc Ngủ Nông | wave ngắn hơn 25% nhưng quái ra dồn dập |
| C3 | Đèn Chập Chờn | mỗi 10s đèn chớp tắt; quái đứng khựng 1s |
| C4 | Ngày Kiểm Kho | không có rương; cứ 25 quái bị hạ rơi 2 vàng |
| C5 | Đêm Đồ Chơi Thức Dậy | vài món đồ chơi trên sàn tự nhảy vào đánh quái gần đó rồi nằm im |

Gợi ý đợt đầu: A1, A4, A5, A8, A9, B1, B2, B5, B8, B10, C1, C3 (8 vật thể cần vẽ).
Code đã soạn sẵn cho B1 (quầy thử thách + quái tinh anh) trong scratchpad, cần hạ thưởng theo bản này khi làm.
