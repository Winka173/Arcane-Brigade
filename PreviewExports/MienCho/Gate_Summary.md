# Miền Chờ — gate Windows 16/10/2026

**Bản art mới nhất:** [gate bộ tranh thay Blender](../PaintedMienCho/Gate_Summary.md), 13 asset, 0 lỗi/0 cảnh báo build. Hồi quy truyện/shop ở bảng dưới đã được chạy lại trên Player này và cập nhật hash; tổng gate hồi 1 hiện là **5834 qua / 84 ảnh**, cùng gate chuyển động/shop thành **29836 qua / 102 ảnh**. Con số 5142 ở phần lịch sử dưới đây thuộc đợt trước khi đổi art.

**Gate tổng mới hơn:** [Hồi 1 / đèn cổ và sương đen](../ActOne/Gate_Summary.md), 5142 kiểm tra và 82 ảnh. Bảng dưới là phạm vi hồi quy truyện/shop; các ảnh đã được chạy lại trên bản Player mới. Kết quả build và hash hiện tại xem gate ActOne.

**PASS cho phạm vi bản triển khai 20 wave và ba cảnh khám phá shop.** Build `Builds/HeroIdentity/ArcaneBrigade.exe`: **Succeeded, 0 error, 0 warning**. [Build.txt](Build.txt) và [Verification.json](Verification.json) lưu kết quả hồi quy trên bản Player mới, hash player/content và source liên quan.

| Gate | Kết quả |
| --- | --- |
| Truyện, dữ liệu, save và cấu hình ánh sáng | 272 kiểm tra Editor qua |
| Hồi quy trần 15/bond | 1021 qua |
| Hồi quy nhịp tuyển | 802 qua |
| Hồi quy hero/mechanic/description/giao dịch | 2330 qua |
| Windows D3D11 thường | 58 qua, 21 ảnh |
| Windows Reduced Motion/Effects/Flashes + High Readability | 58 qua, 21 ảnh |
| Ba cảnh shop, Windows thường | 71 qua, 4 ảnh |
| Ba cảnh shop, Windows giảm hiệu ứng/dễ nhìn | 71 qua, 4 ảnh |
| Tổng gate trên | **4683 qua, 50 ảnh**, không có dòng FAIL |

Các ảnh được chụp từ frame của Unity Player, không phải mockup. Scene thường chạy simulation; không dùng menu pause che cảnh để nghiệm thu ánh sáng. Các fixture giới hạn số quái, tăng giáp và tắt spawn để giữ điều kiện so sánh, không phải playtest cân bằng. Có vòng 120 frame với 15 companion; trung bình **17.36 ms thường / 17.64 ms reduced**, bao gồm cap/VSync và tải trên máy lúc chụp. Không coi đây là benchmark độc lập phần cứng hoặc chứng minh mọi trận đông đều giữ cùng FPS.

Đã kiểm tra: solo đầu/giữa/cuối; đúng số nguồn hero + companion; 15 bạn không nhân cường độ quá trần; silhouette xa, windup hiện đầy đủ, địch tới gần được hiện trước contact; mắt/ember thuộc actor thật; boss không bị bóng tối che; shop và giá; pause; journal 1080p/720p, tiểu sử/arc; lưu xuống đĩa; dawn; kết liễu Customer qua simulation thật và hai nhánh farewell đến kết quả; intro tự mở và đọc sau không lặp lại.

Gate cảnh shop kiểm tra cửa sổ xuất hiện theo wave; tới gần mới Examine; pause dừng hoạt cảnh/chuông; không đổi vàng hoặc roster; lưu cờ chứng cứ và trang từng chương xuống đĩa rồi tải lại scene; đọc sau khác hoàn thành; xác nhận trong mục companion quay về chương; gương dùng companion thật hoặc hero solo; vật thể bị ẩn khi vào combat. Fixture chờ diễn hoạt mở shop kết thúc trước khi thử tương tác. Chưa nghiệm thu tay cầm vật lý hoặc giả lập lỗi ghi ổ đĩa. [Capture_Runs.json](Capture_Runs.json) ghi thời gian và exit code của cả bốn lượt Player trên bản này.

Ảnh tiêu biểu:

- [Đêm sâu, solo](Native/03-endless-night-solo.png), [đội 15](Native/04-endless-night-fifteen-lights.png), [boss](Native/05-customer-boss-readable.png).
- [Shop](Native/06-warm-shop-prices-readable.png), [thư bị giữ lại](Native/07-intercepted-letter-1080.png), [tiểu sử companion 720p](Native/09b-companion-life-and-arc-720.png).
- [Intro tự mở](Native/00-automatic-arrival.png), [bình minh với đội hình thật](Native/ending-CloseShop-3.png), [đèn để lại](Native/ending-LeaveLight-4.png).
- [Đêm ở chế độ dễ nhìn/giảm hiệu ứng](Reduced/03-endless-night-solo.png).
- [Bàn ăn đặt trước](Encounters/Native/encounter-4-world.png), [điện thoại](Encounters/Native/encounter-6-world.png), [gương 720p giảm hiệu ứng](Encounters/Reduced/encounter-mirror-720.png).

Nội dung cốt truyện xuất trực tiếp từ dữ liệu Unity: [Story_Text.md](Story_Text.md). Phạm vi và giới hạn: [Mien_Cho_Runtime_Pass.md](../../Docs/Mien_Cho_Runtime_Pass.md). Chưa có ba chiến dịch riêng, boss entity riêng, 66 quest phân nhánh hoặc mọi cảnh kể thành phòng 3D. Không sử dụng gate animation của agent khác để suy ra kết quả này.
