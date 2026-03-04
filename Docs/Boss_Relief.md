# Giảm độ khó boss — 17/10/2026

Theo phản hồi boss quá khó, giảm thêm **40% HP** và **10% sát thương thực nhận từ boss** so với bản trước đợt này, trên mọi độ khó.

- HP nhân ×0.6 sau khi thiết lập encounter và các hệ số hiện có. Áp dụng boss thường, boss cốt truyện, Last Customer, Bedside, Keeper và boss phòng bí mật. Các tham chiếu trùng chỉ áp dụng một lần.
- Damage nhân ×0.9 ở bước nhận sát thương, sau trần mỗi hit và giảm sát thương chung. Áp dụng đòn chạm, đạn và vùng kỹ năng có nguồn là boss; giữ nguồn của đòn còn tồn tại sau khi boss chết. Chi phí HP của nhân vật không chịu hệ số này.
- Quái thường và lính boss triệu hồi giữ chỉ số trước đợt này. Không đổi vũ khí, nhịp đánh, bộ đòn hay số lính triệu hồi.

Chỉ số được tính lại khi chuẩn bị combat; trận đang chạy cần bắt đầu lại để nhận HP mới.

Kiểm tra Unity: `ArcaneBrigade.Editor.BossReliefChecks.Run`; kết quả ở `PreviewExports/BossRelief/Checks.txt`. Bộ kiểm tra bao phủ sáu độ khó, boss cốt truyện/endless, các override HP đặc biệt, tính lại combat không cộng dồn, quái thường và sát thương boss trước/sau trần hit. Đây là kiểm tra cơ chế, chưa đo cảm nhận cân bằng của người chơi.

Kết quả: **125/125** kiểm tra boss và **17/17** kiểm tra ShopFlow qua trong Unity 6000.4.8f1 batch mode, exit code 0. Đợt này chưa build lại bản Windows.
