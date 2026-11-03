# Rà soát Act I trước khi phát hành — 03/11/2026

Đối chiếu ba nguồn: lời truyện trong game (`Runtime/Data/ActOneCanon.cs`, 20 chương), map và boss trong game (`MapIdentity`, `RunSession.StoryBoss`), và tài liệu truyện `Mien_Cho_Act_One_Story.md`. Bảng nội dung từng wave sinh bằng `ArcaneBrigade.Editor.ActOneContentDump.RunBatch` (`PreviewExports/ActOneContent/Waves.md`).

## Game đang có gì

| Wave | Map | Khu vực truyện (tiêu đề nhật ký) | Boss |
| --- | --- | --- | --- |
| 1–4 | The Waiting Room | THE CROSSING | 4: THE COIN WIDOW |
| 5–8 | The Post Office | 5: THE CROSSING; 6–8: THE LAST PLATFORM | 8: THE CHAIR PORTER |
| 9–11 | The Glasshouse | 9–10: THE LAST PLATFORM; 11: THE HOUSE WITH INWARD WINDOWS | |
| 12–14 | The Lamp Hall | THE HOUSE WITH INWARD WINDOWS | 12: THE DOOR THAT STAYED SHUT |
| 15–17 | The Forgotten Theatre | 15: HOUSE…; 16–17: THE PERFECT ROOM | 16: The Marquee Maw |
| 18–20 | The Archive Vault | THE PERFECT ROOM | 20: The Last Customer |

## Lỗi, xếp theo mức nặng

### 1. Truyện kể trận boss muộn một đến ba wave (nặng)
Boss đã đổi từ wave 5/10/15/20 sang 4/8/12/16/20 (26/10/2026) nhưng lời truyện chưa đổi theo:
- Coin Widow đánh ở wave 4, nhưng trang "When she falls…" nằm ở shop 5.
- Chair Porter đánh ở wave 8, nhưng trang "The Chair Porter came down the platform… When it fell" nằm ở shop 10.
- Door That Stayed Shut đánh ở wave 12, nhưng trang "The front door fought you… When it fell" nằm ở shop 15. Tệ hơn: cửa đã bị hạ ở wave 12, nhưng chương 14 vẫn kể có người gõ cửa và hỏi trốn hay mở.
- The Marquee Maw (wave 16) không có trong truyện.

### 2. Lời truyện tả nơi chốn khác với map người chơi đang thấy (nặng)
- Truyện tả sân ga ("The platform has no end"), trong khi người chơi đang ở Post Office và Glasshouse.
- Truyện tả ngôi nhà thời thơ ấu, trong khi người chơi đang ở Glasshouse và Lamp Hall.
- Truyện tả phòng ngủ hoàn hảo, trong khi người chơi đang ở Theatre và Archive Vault.
- Ranh giới khu vực truyện (wave 6/11/16) không trùng ranh giới map (5/9/12/15/18). Ở wave 5, người chơi đã ở Post Office, nhưng tới shop 5 truyện mới kể bàn tay Keeper bế cửa tiệm sang "rìa một sân ga".

### 3. Bốn đồng hành truyện được kể nhưng không xuất hiện (vừa)
Hob, Latch, Postscript, Tally không còn đi cùng đội (`StoryAlliesTravel = false`, play-test 20/10). Lời truyện vẫn kể họ có mặt: "Latch đứng bên trái ta", "Latch chặn cửa", "Tally ngồi dưới quầy", "Latch, Postscript, Tally kéo ta dậy". Người chơi đọc thấy họ gia nhập mà không bao giờ thấy họ.

### 4. Shop 8 dồn quá nhiều thứ (vừa)
Lời mời phòng bí mật đặt ở shop 3, 8, 13, 18 (`wave % 5 == 3`), theo luật "không bao giờ ở shop sau đêm boss", viết từ hồi boss ở 5/10/15. Giờ shop 8 nằm ngay sau boss Chair Porter, và cùng lúc có: điện thoại (chọn nghe hay không), lời Customer mua ký ức, mở trận ẩn SIT CLOSER, và lời mời phòng bí mật.

### 5. Kẻ thù chưa ra chất từng map (vừa)
- Glasshouse không có quái riêng kiểu cây cỏ. Nhóm quái của nó (Spore Bishop, Blast Puffer, Umbrella Stalker, Ink Toad…) là quái chung, và trong 2 seed thử không có con nào hợp nhà kính.
- Số loài trong một wave quá nhiều: wave 2 đã có 9 loài, wave 12–14 có 19–22 loài. Ngược với mục tiêu "bớt rối" vừa làm cho cơ chế map.
- Một vài con lạc chỗ, ví dụ InkMoth (quái kiểu thư viện) xuất hiện ngay wave 1 ở Waiting Room.

### 6. Tài liệu lệch code (nhẹ)
- `Mien_Cho_Act_One_Story.md` vẫn ghi boss THE KEEPER ở wave 5 và các chặng 1–5/6–10/11–15/16–20.
- Ghi chú trong `RunSession.StoryBoss` và `MapIdentity.EliteWave` vẫn nói boss ở bội số của 5 và Chair Porter "mượn thân" (art riêng đã có trong `Art/ActOneBosses`).

### Phần đã ổn
- Mạch truyện chính chặt: tám ngọn đèn vỡ theo wave, mỗi đèn mất một chi tiết về người khách; Customer trả tiền lên ghế chứ không lên quầy; hai lựa chọn (điện thoại, cánh cửa); kết hồi và hai lựa chọn cuối.
- Boss cuối wave 20 đúng là The Last Customer, ở Archive Vault là nơi có sổ cái của hắn, rất hợp.
- Theatre có khán giả ma, hợp với "những người đã ngồi xuống nhìn về phía giường".
- Tên quái hiển thị đúng ("Hush Mite"…) nhờ `RunUICopy`, dù dữ liệu gốc viết liền.

## Đề xuất sửa

**Hướng chính: cho truyện đi theo 6 map**, vì map, art, cơ chế và nhạc đã xong. Chỉ phải sửa lời và vài con số.

| Map | Wave | Khu vực truyện mới | Truyện đặt ở đây |
| --- | --- | --- | --- |
| Waiting Room | 1–4 | THE CROSSING | Cửa tiệm, ghế ấm, Keeper bị xích; boss Coin Widow ở wave 4, Keeper quỳ ngay shop 4 |
| Post Office | 5–8 | THE DEAD LETTER OFFICE (thay sân ga) | Những Người Đã Ngồi Xuống chờ thư không bao giờ tới; Postscript; bài ru trong máy hát của bưu điện; điện thoại (đúng chỗ của quái Party Line); boss Chair Porter wave 8 cõng tháp ghế qua sảnh phân thư |
| Glasshouse | 9–11 | THE HOUSE WITH INWARD WINDOWS | Ngôi nhà thời nhỏ dựng lại bằng kính, mọi cửa sổ nhìn vào trong; lá thư bị khâu; tiếng gõ cửa và lựa chọn trốn hay mở (chuyển từ chương 14 sang shop 11) |
| Lamp Hall | 12–14 | THE FRONT HALL | Boss Door That Stayed Shut wave 12, cửa mở ra hành lang đèn; Customer mang khuôn mặt (đèn 5, wave 13) |
| Theatre | 15–17 | THE PERFECT ROOM | Phòng ngủ hoàn hảo dựng thành sân khấu, khán giả là Những Người Đã Ngồi Xuống; boss Marquee Maw wave 16 là màn mời "ở lại xem hết vở"; người bán hàng thú nhận |
| Archive Vault | 18–20 | THE LEDGER | Quầy và sổ của Customer; Tally biến mất rồi trở lại; boss Last Customer |

Kèm theo:
1. Chuyển trang kể trận boss về đúng shop sau đêm boss (4, 8, 12, 16), viết thêm trang cho Marquee Maw.
2. Bàn tay Keeper bế cửa tiệm sang map mới ở shop cuối của mỗi map (4, 8, 11, 14, 17), thay vì 6/11/16.
3. Đổi tiêu đề khu vực trong nhật ký theo 6 map (`ZoneNames`, `Zone(wave)`).
4. Lời mời phòng bí mật dời sang shop 3, 7, 13, 18 (không còn ngay sau boss).
5. Bốn đồng hành truyện: cho họ đứng trong shop làm nhân vật (không đánh), từ shop họ gia nhập, dùng thân có sẵn (Teapot Pilgrim, Hood Wisp, Shell Postman, Paper Devil). Hoặc sửa lời để họ chỉ hiện trong nhật ký.
6. Quái: bớt còn khoảng 6–8 loài mỗi wave; cho Glasshouse một nhóm quái cây cỏ (cần vẽ thêm) hoặc lấy các quái có sẵn hợp nhất.
7. Cập nhật `Mien_Cho_Act_One_Story.md` và các ghi chú cũ trong code.

## Cần chủ dự án quyết

- Đồng ý cho truyện đi theo 6 map như bảng trên, kể cả tên khu vực mới?
- Bốn đồng hành truyện: đứng trong shop, hay chỉ có trong nhật ký?
- Glasshouse: vẽ quái cây cỏ mới, hay dùng quái có sẵn?
