# K7 — bản thử Windows và checklist duyệt

Chủ game đã chọn chốt Windows trước; Android/iOS device gate để OPEN sang bước có thiết bị. K7 không tự đánh dấu art/audio/feel đạt chỉ vì bot hoặc test PASS.

## Mở bản thử

- Chạy `Builds/FullRun/ArcaneBrigade.exe`; giữ nguyên cả thư mục build và ThirdPartyNotices cạnh exe.
- Xem `PreviewExports/K7/index.html`; nghe riêng tại `PreviewExports/K7/Audition/index.html`. Video chỉ ghi cửa sổ game, không có audio; nghe mix thật trong game.
- Profile thật tách khỏi diagnostic. Muốn giữ tiến độ cũ, không xóa profile; migration giữ unlock/history và thêm các tùy chọn mới.
- Một lượt bình thường thắng của bot dài khoảng22–24 phút combat, chưa tính đứng mua đồ. Dành30–40 phút để chơi/đọc UI; đây chưa phải số liệu trải nghiệm người thật.

## Điều khiển

- WASD / phím mũi tên: di chuyển. Nhân vật tự đánh; không thêm kỹ năng chủ động.
- Sau wave, đi tới quầy rồi Space để mua; shop vẫn ở trong map. Xem hướng dẫn trong game cho quầy altar và tiền riêng.
- I: xem đồ / Details; Esc: pause hoặc quay lại. Tab / mũi tên rồi Enter: chọn nút menu.
- Mapping tay cầm dự kiến kiểu Xbox: analog trái di chuyển/menu; A mua/xác nhận; B quay lại; Start pause; Y xem đồ; LB/RB chuyển tab. Chưa chứng nhận tay cầm vật lý hay Steam Input.

## Ghi nhận ưu tiên

- [ ] 3 champion có cá tính, silhouette và tay cầm vũ khí không bị lẫn; zoom/map đủ rộng nhưng không cần căng mắt tìm main.
- [ ] Chạy vòng ally có bán kính tự nhiên, không dính chặt main; ở56+ ally vẫn đọc được đạn, hướng đánh và main.
- [ ] Quơ kiếm, kéo cung, phép, kill/death, rơi/nhặt xu, merge, boss warning đều dễ hiểu, mượt và thỏa mãn; ghi hall + vũ khí gây khó chịu.
- [ ] UI Việt không thiếu dấu/chồng chữ; giá/currency/điều kiện mua đọc rõ; pause/inventory/receipt/museum/history không gây bấm nhầm.
- [ ] Cài đặt tổng/nhạc/SFX mute thật, lưu sau mở lại; giảm dồn âm thanh không che cảnh báo nguy hiểm; nhạc boss/quầy không bật quá gắt.
- [ ] Nghe ít nhất một lượt: melody có quá lặp, thiếu bass hay mệt tai không? Rework dùng composed jazz CC0 + Foley Kenney; không tự coi việc thay asset là duyệt soundtrack/mix.
- [ ] Giảm chuyển động: ít bob/jitter/flash cơ thể, vẫn nhìn rõ telegraph; không hứa chế độ chống nhạy sáng tuyệt đối cho mọi hiệu ứng.
- [ ] Alt-Tab và quay lại không tự mở pause do người chơi bật; đóng/mở game ở boundary giữ đúng lượt và đồ, không cấp lại thắng/score.
- [ ] Chơi ít nhất difficulty0 và1, rồi tier cao sau unlock; đi20hall và Endless. Ghi champion, difficulty, seed, hall chết, lượng vàng và món làm build quá mạnh/yếu.
- [ ] Nếu có tay cầm: thử toàn landing/roster/settings/help/museum/sandbox/history/pause/invoice và mua đồ không chuột. Báo mapping không đúng / drift / mất focus; logical input test không thay bước này.

## Mẫu phản hồi

`Champion / difficulty / seed / hall — đã đạt hoặc chưa — mô tả — ảnh/video nếu có.`

Duyệt riêng: **art**, **animation/feel**, **UI/font**, **music**, **SFX/mix**, **balance**, **controller/lifecycle**. Có thể duyệt từng phần và trả danh sách sửa theo mức độ; chưa có phản hồi nghĩa là gate vẫn OPEN.

## Gate ngoài Windows

Android: module + thiết bị thật + USB debugging; iOS: module + Mac/Xcode/signing + iPhone thật. Kiểm touch một tay, safe-area, foreground/background, nhiệt, RAM và FPS trên máy yếu. Steam: AppID/Steamworks chính thức, depot và Steam Input/Deck phải kiểm riêng; không dùng AppID giả để ghi PASS.
