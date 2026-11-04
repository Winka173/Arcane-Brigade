# Kế hoạch kiếm tiền — 03/11/2026

Nguyên tắc: không bán sức mạnh. Hero, Lamp Tree, Lamp Oil giữ nguyên như hiện nay (kiếm bằng cách chơi). Không bán tiền trong game, không bán hộp ngẫu nhiên.

## Theo nền tảng

| Nền tảng | Cách bán | Giá |
| --- | --- | --- |
| Steam (PC) | Bán thẳng, có sẵn mọi thứ của Deluxe, không quảng cáo | 10 USD |
| Điện thoại | Tải miễn phí, phần chính không khoá; gói Deluxe mua một lần | Deluxe khoảng 4,99 USD |

## Phần miễn phí trên điện thoại

Cả 30 hero, đủ 20 wave của Act I, độ khó Peaceful, Wandering, Nightmare, Waiting Room, Daily Dream, Museum.

## Gói Deluxe

**Khi phát hành:**
- Endless (chơi tiếp sau Dawn, giữ đội và relic).
- Hai độ khó cao nhất: Sleepless, The Last Night (vẫn mở dần theo tiến độ như hiện nay).
- Night Pact (tuỳ chọn; nếu xem là phần cốt lõi thì để miễn phí).
- Khung và huy hiệu Deluxe (màn tổng kết, main menu).
- Phòng tranh trong Museum (art nhân vật, boss, map đã gặp).

**Về sau:**
- Act II (có sẵn trong Deluxe; chỉ hứa Act II, không hứa mọi bản mở rộng sau này).
- Trang phục hero (cần vẽ thêm; chủ dự án quyết).
- Nhạc phim (khi có bộ nhạc riêng).

Mục bị khoá vẫn hiện, có ổ khoá và một dòng ngắn "Cần Deluxe". Màn mời mua chỉ ở ba chỗ: khi chạm mục bị khoá, ở màn chiến thắng cạnh nút Endless, và một mục trong main menu. Không chen vào giữa trận.

## Quảng cáo

Bỏ hẳn (quyết định 4/11/2026): không quảng cáo có thưởng, không quảng cáo bắt buộc, không banner, trên mọi nền tảng.

## Việc kỹ thuật

- Unity IAP (Google Play Billing; sau này App Store), nút "Khôi phục giao dịch", cờ Deluxe lưu cùng save và kiểm tra lại với store.
- Màn mời mua, phần khoá nội dung.
- Bản Steam: Steamworks (thành tựu, lưu đám mây), không có IAP và quảng cáo.
- Khai báo Data safety trên Google Play (game không thu thập dữ liệu, không quảng cáo; chỉ có giao dịch qua Google Play) và chọn nhóm tuổi.

## Cần chủ dự án

- Tài khoản Google Play Console (có hồ sơ thanh toán), tài khoản Steamworks.
- Upload keystore cho Google Play.
- Chốt giá Deluxe.

## Đã làm trong game (4/11/2026)

- **Unity IAP 5.4.4** (Google Play Billing). Code: `Runtime/Core/Deluxe.cs` (luật: cái gì bán, cái gì khoá, đã mua chưa), `Runtime/Presentation/DeluxeStore.cs` (kết nối store, giá, mua, khôi phục, hỏi lại store mỗi lần mở game), `Runtime/UI/DeluxeOffer.cs` (thẻ mời mua).
- **Chỉ bán trên điện thoại.** Trên PC (Steam) Deluxe có sẵn. Cờ `-arcane-store` bật cửa hàng điện thoại trên PC để kiểm tra.
- **Deluxe gồm:** Endless, hai độ khó Sleepless và The Last Night (The Last Night vẫn cần cả Lamp Tree), Act II khi có.
- **Quyết định:** Night Pact để miễn phí (là phần cốt lõi của cách chơi). Khung, huy hiệu Deluxe và phòng tranh Museum để sau (cần vẽ thêm); chỉ hứa những gì đã có.
- **Thẻ mời mua mở ở ba chỗ:** chạm độ khó bị khoá (thanh độ khó hoặc danh sách), nút Endless ở màn bình minh, và mục "DELUXE · giá" trong main menu (chỉ hiện khi chưa mua). Không bao giờ trong trận.
- **Thẻ:** những gì Deluxe thêm, dòng "Không bán thứ gì làm lượt chơi mạnh hơn", nút vàng "GET DELUXE · giá" (giá theo store của người chơi), nút "RESTORE PURCHASE", nút đóng; chạm ra ngoài cũng đóng.
- **Đã mua:** lưu trên máy và được store xác nhận lại mỗi lần mở game; mua trên máy khác hoặc cài lại vẫn có lại; bị hoàn tiền thì mất.

## Việc cần làm trong Google Play Console

1. Tải lên một bản `.aab` (kênh Internal testing là đủ) để mở mục sản phẩm.
2. Monetize › Products › **In-app products** › Create product:
   - Product ID: **`deluxe`** (phải đúng như trong `Deluxe.ProductId`).
   - Tên: Arcane Brigade Deluxe. Mô tả: Endless và hai độ khó cao nhất; Act II khi ra mắt.
   - Giá: khoảng 4,99 USD (Play tự quy đổi theo nước). Activate.
3. Setup › **License testing**: thêm email người thử để mua thử không mất tiền.
4. Không cần quyền nào thêm: gói Unity IAP tự thêm quyền BILLING vào bản Android.
