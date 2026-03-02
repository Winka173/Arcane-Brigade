# Đồng bộ theme Hồi 1 — bản sửa 17/10/2026

**Cập nhật sau rà soát lần hai:** đã sửa source/asset cho 10 phát hiện, bổ sung 8 pose/sprite và xuất lại atlas Keeper. Chưa chạy build/test/runtime mới. Chi tiết: [Theme_Consistency_Completion.md](Theme_Consistency_Completion.md). Bản [rà soát](Theme_Consistency_Reaudit.md) giữ làm lịch sử phát hiện.

Phạm vi được chủ dự án duyệt: sửa các điểm trong [audit](Theme_Consistency_Audit.md), **giữ nguyên vũ khí và mechanic vì agent khác đang phụ trách**. Canon là [Hồi 1](Mien_Cho_Act_One_Story.md). Không triển khai Hồi 2–3.

## Art và ánh sáng

- Thêm **70 PNG** bằng công cụ `painted`: 49 icon stat riêng, 10 hình Keeper/bộ phận/lăng kính, một thân modular bổ sung và 10 đạo cụ. Mỗi sprite có metadata Unity; không dùng Blender. [Provenance](Theme_Consistency_Art_Provenance.json), [prompt nguyên văn](Theme_Consistency_Art_Prompts.json), [script tích hợp](../Tools/Art/slice_theme_consistency.py). Python chỉ tách mực và thêm lề trong suốt, không vẽ thay công cụ ảnh.
- Keeper dùng thân modular không in sẵn bàn tay/thảm/mắt để tránh nhân đôi khi lắp bộ phận. Các bộ phận nguyên/vỡ dùng bộ painted mới; ba cảnh vận chuyển dùng cùng thiết kế; silhouette và prism nạp qua đường resource hiện hành. Giữ nguyên vị trí vùng đánh, độ bền bộ phận, moveset, damage và phần thưởng. Trạng thái quỳ có art riêng; đây không phải bộ 16 pose mới cho mỗi bộ phận.
- `PaintedStatIcons` ưu tiên sprite chuyên dụng cho toàn bộ 49 `ItemEffect`, không phối badge nhỏ lên relic/vũ khí nữa; bộ cũ chỉ là fallback nếu asset không tải được. Dùng chung cho description, glossary và popup stat.
- Ánh sáng đội chia thành các lượt GPU tối đa 16 nguồn, hợp bằng giá trị lớn nhất. **16 là kích thước một lượt, không phải trần số đồng minh**. CPU và GPU nhận mọi bạn đang sống; cường độ không cộng vô hạn làm sáng trắng map. Màu nguồn sáng giới hạn trong hổ phách, teal, xanh trăng và kem.
- Hạ nhẹ độ bão hòa thân nhân vật để hòa với cảnh painted; nhánh material dùng cho vũ khí/prop giữ nguyên. Đây là chỉnh trình bày chung, **không phải vẽ lại 31 hero và 66 loài**. Những sprite còn đơn giản vẫn giữ silhouette gốc.

## Canon và diễn xuất

- Hai đoàn ending thêm các diễn viên từ `CombatCatalog.StoryCompanions`; giữ tên, art và idle/walk của Hob/Latch/Postscript/Tally thực sự đang đi cùng, không biến bản mua cùng loài thành nhân vật truyện. Cảnh chia tay vẫn xử lý bạn cứu hộ như trước.
- Bố cục bình minh co giãn hàng theo quân số; phòng cứu hộ cũng không cắt danh sách hiển thị ở 15 bạn. Đèn trong phòng gộp theo ô, mọi bạn đóng góp trong giới hạn 16 nguồn shader của phòng.
- Màn lựa chọn ending thay hình người đàn ông cũ bằng `LastCustomer_Blank` và hợp đồng cháy. Không ngầm trả lại khuôn mặt đã bán. Hai lối vẫn dẫn tới bình minh thật.
- Mặt Customer giữ trạng thái dễ nhận ra, chuyển sang mặt vay mượn khi đe dọa/ra đòn hoặc bị thương nặng; không đảo mặt theo đồng hồ. Đây là phản ứng trình bày trên trạng thái trận có sẵn.
- Câu thì thầm về thư, khóa, bài ru, tên trong sổ và chuyến đi chỉ được nhắc sau mốc tương ứng đã đọc xong. Lời merchant kiểm tra cũng lấy bằng chứng đã hoàn thành, không lấy chương vừa mở khóa. Trước đó dùng dấu hiệu không tiết lộ lời giải.
- Điện thoại thật xuất hiện tại shop 8; chuông và thao tác nhấc máy mở đúng chương điện thoại, giữ lựa chọn Answer / Let it ring. Không khôi phục bàn/gương của canon cũ.
- Có vật chứng trên map ở shop 12 (thư khâu), 15 (ô ướt), 17 (ghế đẩu) và 18 (chỗ Tally bỏ lại). Chúng dùng vị trí thế giới, tham gia bố trí shop, lơ lửng và mở đúng chương khi lại gần bấm Space/click. Bố trí không dựng lại khi mua hoặc reroll.
- Shop 17 đặt **Hob đang có trong đoàn** lên ghế đẩu bằng tư thế trình bày, không tạo bản sao hay đổi simulation. Ghế giữ lơ lửng; ánh sáng đi theo vị trí nhìn thấy. Tally cầm tờ giấy rồi gấp/cất khi ta đến gần; shop 18 vẫn vắng mặt theo canon.
- Trong phòng cứu Hob, hero và bạn dùng idle/walk, bước nhấc chân, quay hướng, nghiêng người kéo và nhịp đứng vững/rời ghế. Một lần bấm Space bắt đầu cả đoàn kéo; không cần giữ phím 2.4 giây. Đạo cụ phòng dùng đúng ghế thường/cửa sổ/vật chứng, không lấy thân hero làm cửa sổ.
- Trận Bedside dùng motif ru bỏ lửng lặp riêng, nhạc combat thường hạ xuống; rời trận trả lại nhạc theo scene. Đây vẫn là nhạc tổng hợp, không phải thu âm hoặc voice acting mới.

## UI và nội dung cũ

- Bỏ đường phím J còn sót và hướng dẫn J/ghế mở tuyến cũ. Journal mở bằng tủ hoặc tờ ghi chú. Cổng/sự kiện nhận Space theo lần bấm, không còn giữ 0.85 giây; có chặn phím từ màn trước lọt qua.
- Tên hiển thị bond `Toy Knights` → **Crossing Knights**, `Playtime` → **Lantern Revel**. Giữ ID `knight`/`ball`, tag, save và mọi bonus.
- Mô tả dự phòng của companion gọi họ là cư dân Miền Chờ có cuộc đời riêng, thay cách gọi chung là đồ chơi; không đổi passive hoặc support.
- **Sáu tuyến lựa chọn và phòng Station/FootstepHall đời cũ được lưu làm nội dung retired**, không quảng bá là sáu quest đang chơi trong canon. Không nối lại bốn route của loài mua với bốn diễn viên canon: họ có danh tính riêng. `WaitingCompanionQuests.Futures` cũng không được chèn vào ending Hồi 1 như thể những lựa chọn đó đã diễn ra. Tiểu sử loài và hành trình canon trong journal vẫn có.

## Ngoại lệ và giới hạn

Các mục 02, phần màu/tỷ lệ vũ khí và token mechanic ở 01/03, cùng tên `Toybox Orchestra`/nguồn damage trong file mechanic ở 13 được để cho agent đang phụ trách. Không sửa hình vũ khí, animation đòn, collision, chỉ số, AI hoặc cơ chế hero. Bộ art gốc 261 PNG vẫn là đợt giao trước, không gộp thành một lần nghiệm thu mới.

Đạo cụ vạch chiều cao/dấu chân/đòn bẩy đã có mapping cho các phòng lưu trữ cũ; điều đó không mở lại các phòng đó trong Hồi 1. Áo mưa là asset dự phòng minh họa; không tự thêm một cảnh gặp người đón ngoài canon. `Keeper_Whole_Body` là bản đủ hình dùng làm mẫu, trận dùng `Keeper_BodyShell`.

**Theo yêu cầu chốt nhanh trước đó, đợt này không chạy build, test, đo hiệu năng, nghe nhạc hoặc nghiệm thu ảnh trong Windows.** Chỉ ghi nhận source/asset đã sửa; các ảnh và gate cũ không chứng minh kết quả của đợt này. Cần một lượt xem trong player để quyết định thẩm mỹ cuối của Keeper, tư thế trên ghế và độ đọc icon ở 24 px.
