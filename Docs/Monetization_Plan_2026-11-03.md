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
- Tắt quảng cáo bắt buộc.

**Về sau:**
- Act II (có sẵn trong Deluxe; chỉ hứa Act II, không hứa mọi bản mở rộng sau này).
- Trang phục hero (cần vẽ thêm; chủ dự án quyết).
- Nhạc phim (khi có bộ nhạc riêng).

Mục bị khoá vẫn hiện, có ổ khoá và một dòng ngắn "Cần Deluxe". Màn mời mua chỉ ở ba chỗ: khi chạm mục bị khoá, ở màn chiến thắng cạnh nút Endless, và một mục trong main menu. Không chen vào giữa trận.

## Quảng cáo (chỉ điện thoại)

**Có thưởng, người chơi tự bấm:**
- Màn tổng kết: nhân đôi Lamp Oil của lượt vừa xong.
- Daily Dream: thêm một lần chơi trong ngày.
- Không hồi sinh, không thưởng gì trong trận (giữ độ khó và Records đúng).

**Bắt buộc, chỉ ở màn tổng kết (sau khi chết hoặc thắng):**
- Tối đa 1 lần mỗi 20–30 phút chơi.
- Không trong 2 lượt đầu và ngày đầu tiên.
- Không sau lượt chết trước wave 3.
- Bỏ qua được sau vài giây.
- Người có Deluxe không bao giờ thấy.

**Không bao giờ:** giữa trận, giữa các wave, khi vào shop, khi mở app, khi Pause; không banner.

## Việc kỹ thuật

- Unity IAP (Google Play Billing; sau này App Store), nút "Khôi phục giao dịch", cờ Deluxe lưu cùng save và kiểm tra lại với store.
- Quảng cáo qua AdMob hoặc Unity LevelPlay, hộp hỏi đồng ý quyền riêng tư (GDPR/UMP).
- Màn mời mua, phần khoá nội dung, bộ đếm giới hạn quảng cáo.
- Bản Steam: Steamworks (thành tựu, lưu đám mây), không có IAP và quảng cáo.
- Khai báo Data safety trên Google Play; đặt độ tuổi 13+ (art dễ thương dễ bị xếp vào nhóm trẻ em, khi đó luật quảng cáo chặt hơn).

## Cần chủ dự án

- Tài khoản Google Play Console (có hồ sơ thanh toán), tài khoản AdMob, tài khoản Steamworks.
- Upload keystore cho Google Play.
- Chốt giá Deluxe.
