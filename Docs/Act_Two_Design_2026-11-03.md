# Act II “The Kept Rooms” — thiết kế đề xuất, 03/11/2026

Bản nháp để chủ dự án duyệt. Tên riêng (boss, hero, map) là tên tạm.

## 1. Vị trí trong game

- Mở sau khi thắng Act I lần đầu. Nằm trong gói Deluxe (xem `Monetization_Plan_2026-11-03.md`); bản Steam có sẵn.
- Phát hành làm bản cập nhật lớn đầu tiên sau Act I.
- Cùng khung với Act I: 20 wave, boss mỗi 4 wave, shop sau mỗi wave, cùng Waiting Room, cùng 30 hero và Lamp Tree. Độ khó đủ 5 mức.

## 2. Mạch truyện

Nối từ `Mien_Cho_Act_One_Story.md` §9: *Hồi 2 — Những căn phòng bị giữ lại: đi tìm người khách đã quay lại tám lần; tên của họ nằm trong ngọn đèn cuối. Ta tự quay lại Miền Chờ vì phong bì ngày thứ Năm, không bị kéo về.*

- **Mở đầu:** sáng thứ Năm, phong bì không tem trên thảm chùi chân. Trong phong bì là một chiếc chìa khoá cũ. Đêm đó ta tự bước vào Miền Chờ, lần đầu không ai gọi.
- **Sự thật của hồi:** tám lần người khách quay lại, Người Luôn Ở Đó không chỉ giấu đi mà còn **cất mỗi lần vào một căn phòng** và khoá lại. Act I thắp lại tám ngọn đèn (giọng nói, màu áo mưa, dáng người, mùi mưa, khuôn mặt, cách gõ cửa, nét chữ, cái tên). Act II mở từng căn phòng để lấy lại **câu chuyện** của mỗi lần đến: người khách đã làm gì, nói gì, chờ bao lâu.
- **Kẻ đối đầu của hồi: THE HOUSEKEEPER**, tay sai của Người Luôn Ở Đó. Nó dọn sạch mọi dấu vết người khách để lại: lau dấu chân, gấp thư, phủ khăn lên đồ đạc. Người Luôn Ở Đó vẫn chỉ thì thầm, để dành cho Act III.
- **Đồng hành chính:** bốn người của Act I (Hob, Latch, Postscript, Tally) đi tiếp. Tally trở về sau khi biến mất ở Shop 18 của Act I; vết khắc trên thân nó là manh mối đầu tiên dẫn tới các căn phòng.
- **Kết hồi:** mở căn phòng cuối, ta biết người khách là ai và vì sao họ quay lại tám lần. Ta chọn mang câu chuyện đó theo hay để lại cho Những Người Đã Ngồi Xuống nghe (hai kết, đều là tự do thật, như Act I). Mở đường sang Act III: cánh cửa cuối có dấu tay của Người Luôn Ở Đó.

## 3. Cấu trúc một lượt

Mỗi chặng 4 wave, kết thúc bằng một boss. Sau mỗi boss, **chọn một trong hai cánh cửa** dẫn sang hai căn phòng khác nhau. Một lượt đi qua 5 phòng trong số 8, mỗi lượt một đường khác nhau. Đây là cách sửa điểm thứ tự map cố định của Act I.

| Chặng | Wave | Phòng (chọn 1 trong 2) | Boss |
| --- | --- | --- | --- |
| I | 1–4 | Hành lang ngày thứ Năm (cố định, mở đầu) | THE COAT STAND |
| II | 5–8 | Chiếu nghỉ cầu thang / Gác xép | THE STAIR THAT COUNTS |
| III | 9–12 | Phòng giặt / Vườn sau | THE WASH THAT NEVER DRIES / THE GATE OF HEDGES |
| IV | 13–16 | Phòng khách phủ khăn / Phòng ngủ thời nhỏ | THE DUST SHEET / THE SMALL CHAIR'S SHADOW |
| V | 17–20 | Căn phòng cuối (cố định) | THE HOUSEKEEPER |

Mỗi phòng gắn với một lần người khách quay lại, tức một ngọn đèn của Act I. Thắng một phòng thì nhận một **trang chuyện** của lần đến đó.

## 4. Tám căn phòng (map)

Mỗi phòng có **một cơ chế chính luôn bật và hai đến ba cơ chế phụ luân phiên**, một cơ chế phụ mỗi wave, cơ chế phụ im khi boss hoặc elite ra. Đúng hệ thống `MapPacing` đã có.

| Phòng | Đèn gắn với | Cơ chế chính | Cơ chế phụ |
| --- | --- | --- | --- |
| Hành lang ngày thứ Năm | Cách gõ cửa | Các cánh cửa dọc tường lần lượt rung rồi mở; đứng trước cửa đang mở được tăng sức | Thư rơi qua khe; áo mưa treo rỏ nước làm trơn sàn |
| Chiếu nghỉ cầu thang | Dáng người | Sàn chia bậc, đứng trên bậc cao bắn xa hơn | Tiếng bước chân lăn xuống thành sóng đẩy; đèn cầu thang chớp tắt |
| Gác xép | Giọng nói | Hộp nhạc giữa gác: đứng gần khi nó hát thì quái chậm lại | Rương bật nắp thả quái nhỏ; bụi rơi theo đường xà |
| Phòng giặt | Mùi mưa | Hơi nước dâng theo đợt, che tầm nhìn quái | Dây phơi kéo ngang chặn đường; máy giặt rung đẩy mọi thứ ra xa |
| Vườn sau | Màu áo mưa | Mưa theo đợt, vũng nước lớn dần | Hàng rào cây mọc chặn lối; chiếc ô gập rơi làm tấm khiên tạm |
| Phòng khách phủ khăn | Khuôn mặt | Khăn phủ đồ đạc: kéo khăn ra (đi qua) lộ đồ vật cho thưởng hoặc quái | Đồng hồ quả lắc dừng thời gian trong một vùng; ảnh trên tường nhìn theo |
| Phòng ngủ thời nhỏ | Nét chữ | Vạch đo chiều cao trên khung cửa: mỗi wave một vạch, vạch càng cao quái càng mạnh, đổi lại thưởng nhiều hơn | Đồ chơi lên dây cót chạy lung tung; ngăn kéo bàn mở ra thư cũ |
| Căn phòng cuối | Cái tên | Ngọn đèn cuối giữa phòng: sáng dần theo wave, vùng sáng là vùng an toàn | Housekeeper lau mất một phần sàn (vùng tối); những trang chuyện bay quanh |

## 5. Năm boss chặng và boss cuối

Quy tắc chung (giữ như Act I): chỉ boss có vùng cảnh báo đòn đánh, vùng cảnh báo là một mảng màu đặc, đòn lớn có thời gian lấy đà rõ, mỗi boss có một tiếng lấy đà nghe rõ.

| Boss | Hình dạng | Đòn nổi bật |
| --- | --- | --- |
| THE COAT STAND | Giá treo áo cao, treo tám chiếc áo mưa | Ném áo mưa thành vùng trơn; xoay người quét móc áo theo vòng cung |
| THE STAIR THAT COUNTS | Cầu thang gỗ biết đi, mỗi bậc là một con số | Gọi số: bậc đó sụp thành hố; đếm ngược rồi lăn cả tầng bậc xuống |
| THE WASH THAT NEVER DRIES | Máy giặt cổ, dây phơi làm tay | Phun hơi theo hình nón; kéo dây phơi quét ngang sân |
| THE GATE OF HEDGES | Cổng vườn mọc gai | Mọc hàng rào thành mê cung tạm; bắn gai theo hàng |
| THE DUST SHEET | Tấm khăn phủ khổng lồ | Trùm xuống một vùng rồi kéo đi; chia thành nhiều khăn nhỏ |
| THE SMALL CHAIR'S SHADOW | Bóng của chiếc ghế gỗ nhỏ | Kéo dài bóng thành đường đánh; ngồi xuống làm cả phòng tối lại |
| THE HOUSEKEEPER (wave 20) | Người hầu cao gầy, khăn lau và chùm chìa khoá | Lau sạch vùng sàn (mất ánh sáng); khoá các cửa phòng quanh sân; pha cuối đem tám trang chuyện ra đốt |

## 6. Tiến trình qua nhiều lượt

- **Trang chuyện:** mỗi phòng thắng lần đầu cho một trang; tám trang kể lại tám lần người khách đến. Xem trong Postbox (thêm một ngăn "Thư người khách").
- **Mở kết hồi:** cần thắng boss cuối ít nhất một lần; đủ tám trang thì mở thêm một đoạn kết dài hơn.
- Không có phần thưởng sức mạnh mới ngoài Lamp Tree hiện có. Nếu cần thêm nhánh Lamp Tree thì dùng Lamp Oil như cũ.

## 7. Nội dung mới khác

- **Quái mới (8–10 loại):** đồ vật trong nhà bị cất đi, như chổi lông, móc áo, ấm nước, chìa khoá chạy, khăn bay, đèn ngủ, giày đi trong nhà. Không dùng lại hình của quái Act I. Mỗi loại một kiểu di chuyển khác.
- **Hero mới (4):** gắn với các lần người khách đến, ví dụ chiếc ô gập, thước đo chiều cao, tấm lịch ngày thứ Năm, chiếc thảm chùi chân. Mỗi hero một vũ khí riêng từ hình dạng của nó.
- **Đồng hành mới (6–8)** và vài bond mới theo cơ chế 2/4/6 hiện có.
- **Waiting Room:** ngăn "Thư người khách" trong Postbox; cửa mới trong phòng dẫn vào Act II.

## 8. Quy tắc thiết kế phải giữ

Lấy từ các quy tắc đang dùng cho Act I:
- Kỹ năng và đồ vật của mỗi nhân vật lấy từ chính hình dạng của nó, để dễ vẽ.
- Kỹ năng không nhắm một quái đơn lẻ (cuối trận có hàng trăm quái): đánh theo vùng, theo hàng, theo cụm, hoặc tăng cho hero và đồng đội.
- Đồng hành bất tử; buff cho đội không hồi máu, không thêm giáp, không né.
- Không dùng lại animation: mỗi kỹ năng, mỗi cơ chế có hình riêng.
- Hiệu ứng to, rõ, đẹp; mờ nhạt là lỗi.
- Mỗi map một cơ chế chính, các cơ chế phụ luân phiên (MapPacing).
- Kiểm tra cả bố cục điện thoại (`-arcane-phone`).

## 9. Danh sách art cần vẽ

| Nhóm | Số lượng ước tính |
| --- | --- |
| Nền và sàn 8 phòng | 8 |
| Đồ vật và cơ chế trên map | khoảng 40–50 |
| Boss (thân, các pha, đòn đánh) | 7 boss, mỗi boss vài khung hình |
| Quái mới | 8–10 loại, mỗi loại khung đi và khung đánh |
| Hero mới | 4, kèm vũ khí và vật của kỹ năng |
| Đồng hành mới | 6–8 |
| Trang truyện (mở đầu, kết, tám trang chuyện) | khoảng 12 |
| Âm thanh | nhạc cho 8 phòng và boss, tiếng lấy đà mỗi boss (theo `Docs/Audio_Prompts_2026-10-20.md`) |

## 10. Thứ tự làm đề xuất

1. Chốt truyện và danh sách phòng, boss (tài liệu này).
2. Khung chọn cửa sau boss và tiến trình trang chuyện (code, dùng map Act I làm chỗ thử).
3. Vẽ và dựng từng phòng: nền, cơ chế chính, cơ chế phụ.
4. Quái mới, rồi boss theo thứ tự chặng.
5. Hero và đồng hành mới.
6. Trang truyện, kết hồi, cân bằng bằng bot (`BalanceAudit`) và chơi thử trên điện thoại.

## 11. Câu hỏi cho chủ dự án

- Duyệt kẻ đối đầu THE HOUSEKEEPER và cấu trúc chọn cửa?
- Muốn 8 phòng ngay, hay phát hành 6 phòng trước rồi thêm sau?
- Số hero mới: 4 có đủ không?
