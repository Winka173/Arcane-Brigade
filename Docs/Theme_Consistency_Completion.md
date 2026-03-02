# Hoàn thiện các điểm rà soát theme — 17/10/2026

Đã sửa source và tích hợp asset cho **10 phát hiện** trong [rà soát lần hai](Theme_Consistency_Reaudit.md). Phạm vi là hình ảnh, diễn xuất, điều kiện kể truyện và thao tác vật chứng Hồi 1. Vũ khí, chỉ số, luật chiến đấu và Hồi 2–3 thuộc phạm vi khác.

Theo yêu cầu chốt nhanh trước đó, **không chạy build, test, player hoặc đo hiệu năng** trong lượt này. Đây là bản ghi triển khai, chưa phải nghiệm thu runtime hay xác nhận toàn game không còn lỗi.

| Điểm | Đã sửa |
| --- | --- |
| 1 · Nhầm diễn viên khi mua/reroll | Cảnh ghế kiểm tra thế hệ simulation, danh tính, faction và tham chiếu UnitState hiện hành của Hob/Tally. Vị trí ánh sáng Hob cũng chỉ dùng cache của đúng diễn viên. |
| 2 · Resume sau cứu Hob | Phân biệt phòng cứu hộ ở shop 16 với cờ cứu thành công; resume dựng cảnh hậu cứu, giữ Hob đã đứng dậy và Người Đã Ngồi Xuống đã quay đầu. Không rơi vào nhánh Perfect Room retired. |
| 3 · Tiết lộ quá sớm | `ActOneKnowledge` dùng tiến độ hoàn thành trang truyện và kết quả cứu Hob. Journal Lives, Brigade, lời bình shop, banter và merchant dùng cùng điều kiện; Hob chưa kể chuyện ghế đẩu trước shop 17, Postscript chưa nhắc hồi âm trước khi đọc, Tally chưa kể chuyện biến mất trước shop 18. |
| 4 · Mảnh thừa trong atlas | Bộ tách Keeper chỉ nhận các cụm mực được chỉ định và viền khử răng cưa sát chúng; bỏ việc gán mảnh lạ theo khoảng cách và vệt alpha rất mờ ở xa. Xuất lại 12 sprite từ sheet gốc, giữ GUID asset. Không vẽ đè bằng Python. |
| 5 · Keeper thiếu pose | Thân modular quỳ khi `KneelUntil` còn hiệu lực. Tay/thảm có pose lấy đà và ra động tác riêng; mắt có pose tích lực. Hốc vỡ dùng hình rỗng, không chồng thêm một bộ phận đã vỡ. Chuyến chở shop cũng dùng các pose khác nhau. Chỉ đọc trạng thái combat sẵn có. |
| 6 · Footprint vô hình | Cảnh nghỉ, điện thoại, Customer và diễn viên cảnh nền trả lại chỗ trong ShopLayout khi ẩn, đổi wave, rời shop hoặc bị hủy. |
| 7 · Điện thoại chồng vật khác | Điện thoại xin vị trí/kích thước qua ShopLayout; hình, bóng, focus và hiệu ứng cùng dùng anchor đã bố trí. |
| 8 · Click và tay cầm | Vật chứng và điện thoại nhận click trực tiếp theo vùng mực của hình đang nghiêng/lơ lửng. Nhận Space/E/A; vẫn qua focus và điều kiện modal của chủ tương tác. |
| 9 · Các giao dịch Customer | Cảnh diễn tại ghế ở shop 2/7/8/12/13/19; shop 7 có khung cửa sổ giữa ta và Customer, tiền bay vào ghế; shop 12 có pose quỳ vẽ riêng; shop 19 tiền không đổi được thứ gì và lời thoại đổi nếu đã đứng dậy khỏi ghế. Shop 4/9/16 giữ tiền đã nằm sẵn theo canon, không dựng cảnh xuất hiện trái với lời kể. Timer giữ qua mua/reroll, dừng khi đọc/pause; tiền diễn cảnh không thêm vào ví hoặc loot. |
| 10 · Ending nói sai quân số | Kiểm tra cả bạn truyện và bạn thường khi chọn lời kết; câu dự phòng không còn khẳng định ta đi một mình. |

## Art mới và nguồn

Thêm **8 sprite** vẽ bằng `painted`: `Keeper_KneelShell`, `Keeper_Hand_Charged`, `Keeper_Hand_Release`, `Keeper_Carpet_Charged`, `Keeper_Carpet_Release`, `Keeper_Eye_Charged`, `Keeper_Socket`, `LastCustomer_Kneel`. Tổng bộ theme của hai lượt là **78 PNG = 70 trước + 8 mới**, không cộng thành 78 bộ animation.

- [Prompt và nguồn 8 ảnh](Theme_Consistency_Followup_Art_Prompts.json).
- [Manifest 78 asset và SHA-256](Theme_Consistency_Art_Provenance.json).
- [Bộ tách atlas/tích hợp sprite](../Tools/Art/slice_theme_consistency.py); nhận bốn sheet cũ và JSON các sprite đơn bổ sung. Metadata Unity đi kèm; GUID cũ được giữ.

Các giới hạn đã nêu trong audit vẫn cần nghiệm thu riêng: đọc icon ở kích thước nhỏ trong player, cảm giác chuyển pose/ánh sáng, đội hình đông, pause/resume thực tế và hiệu năng Windows. Chưa vẽ lại toàn bộ art thân của 31 hero và 66 loài companion; không coi việc sửa 10 phát hiện này là thay toàn bộ catalog.
