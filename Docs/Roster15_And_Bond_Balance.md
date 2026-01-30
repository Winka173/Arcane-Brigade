# Trần 15 companion và giảm nhẹ bond

Ngày 16/10/2026: trần tuyển mới giảm từ 20 xuống **15 cơ thể**. Giữ nguyên hai quota shop thường: tối đa ba lượt companion/round gồm bản trùng; trong đó loài mới tối đa hai ở round 1–3, sau đó một. Shop đặc biệt miễn quota lượt mua, nhưng vẫn chịu trần cơ thể 15.

Mọi đường thêm roster — shop thường/đặc biệt, quà talent, quà relic, quà Spiky Yoga Ball, Sprout/Grand Sprout, Keeper và giải cứu bí mật — dùng cùng hằng giới hạn. Khi đầy vẫn được mua bản trùng **nếu ghép ngay** và làm giảm quân số, trong quota mua còn lại. Hai bản 3 sao hoặc talent Evil không được giả là có thể ghép để lách trần.

Save cũ đang có 16–20 người giữ nguyên bạn đã sở hữu, không xóa hay cắt đội tự động. Chặn mọi lần thêm cơ thể; vẫn cho mua để ghép ngay, giảm dần về trần mới. Memory hấp thụ không phải cơ thể và giữ quy tắc riêng của hero.

Contract Dream Lamb/Borrowed Shadow không tạo cơ thể thứ 16; khi đội đầy, dùng phương án echo +20% hero damage hiện có cho solo. Mô tả contract nêu rõ điều này. Thưởng giải cứu khi đầy đổi sang relic tương ứng (Wooden Heart/Dead-Letter Key); nếu relic đã đủ stack thì hiển thị và trả 25 vàng. Tránh kẹt màn nhận thưởng. Keeper vẫn có quà relic để chọn, nút nhận companion bị khóa và giải thích khi đủ 15 người.

## Bond

Giảm **10% reward dương có lợi**, gồm reward lặp theo level và extra milestone. Ví dụ +8% damage thành +7,2%; +15% thành +13,5%; +8 HP thành +7,2 HP trước làm tròn và giới hạn stat. Hiệu ứng bất lợi tăng enemy count/health/speed và giá trị âm giữ nguyên. Các tooltip, bảng nguồn stat, preview và compile cùng đọc reward đã cân bằng.

Giữ nguyên 29 bond, tag, cách đếm từng cơ thể, threshold, level và các cơ chế hero/talent dùng level bond. Đợt này giảm các reward stat trực tiếp; không viết lại mọi tương tác talent theo bond.

Hằng `BondDefinition.PositiveRewardScale=0.9` áp lúc đọc reward, không nhân lặp vào asset mỗi lần chạy importer. Trường Value/ExtraBonuses gốc vẫn có trong dữ liệu; xem [Bond_Changes.tsv](../PreviewExports/Roster15/Bond_Changes.tsv) để đối chiếu giá trị trước/sau.

Đường gate loài mới vẫn bắt đầu giảm từ 6 cơ thể, nay tiến tới 2% ở 15 thay vì 20. Ba round đầu giữ mức cao và Luck tiếp tục tham gia như trước. Không đổi công thức ghép, vũ khí kế thừa hay bond để ép quân số thấp hơn 15.

Achievement quân số đổi mục tiêu thành **8/12/15**, giữ ID cũ để không xóa achievement người chơi đã đạt. Vẫn có đủ 104 achievement nhật ký.

## Kiểm tra

- [Verification.json](../PreviewExports/Roster15/Verification.json): **1.021 kiểm tra** về trần, shop/quà, ghép khi đầy, save cũ, contract, thưởng bí mật/Keeper, achievement và tất cả 29 bond ở count 0–16.
- Chạy lại [802 kiểm tra tuyển quân](../PreviewExports/Recruitment/Verification.json) và [2.330 kiểm tra hero/description/shop](../PreviewExports/HeroIdentity/Verification.json).
- [Build.txt](../PreviewExports/Roster15/Build.txt) ghi kết quả build cuối. Bản Windows cập nhật nằm ở `Builds/HeroIdentity/ArcaneBrigade.exe`.

Đây là thay đổi cân bằng theo yêu cầu, chưa thay thế việc chơi thử dài hạn để đánh giá độ khó.
