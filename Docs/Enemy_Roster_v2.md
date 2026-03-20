# Danh sách quái bản 2 (chốt 19/10/2026)

Rà soát theo theme Miền Chờ (đồ thất lạc, thư chưa gửi, đèn lồng, sân ga đêm, Sương Đen). Mọi cơ chế chỉ nhắm hero hoặc mặt sàn;
companion bất tử nên không có quái "giết đồng minh". Art: `Tools/Art/enemies_art.py` → `Docs/Expansion_Art_Worklist.json`
(job `Enemy_*`, lưu ở `Assets/ArcaneBrigade/Art/Expansion/Enemies/`, atlas 4×4 như K22/K25: 2 hàng dạng thường, 2 hàng dạng tiến hóa).

## 1. Tám quái mới

| Quái | Khu · wave | Vai trò | Cơ chế (đọc được, né được) |
|---|---|---|---|
| Spare Chair | Platform · 6–10 | Khống chế vị trí | Đi tới, bung ra, mực vẽ vòng hình chỗ ngồi dưới hero (1.0 s báo trước, ánh hổ phách dưới ghế); còn trong vòng khi ghế gập thì bị ghim 0.6 s. |
| Waking Sitter | Chỉ trận ẩn SIT CLOSER | Trói / làm chậm | Đứng dậy, ném dây mực vào hero; khi dây căng hero chậm 30%, sát thương thấp; đi xa quá 5 đơn vị là dây tuột. Không xuất hiện ở wave thường ("họ không phải quái"). |
| Ink Notary | Perfect Room · 16–20 | Hỗ trợ quái | Đi sau quái khác, đóng dấu PAID: quái được đóng giảm 50% đòn kế tiếp nhận; in vết mực CLOSED trên sàn làm hero chậm 25% trong 3 s. |
| Sewn Letter | Platform · House · 6–15 | Tách đôi | Chết thì bung chỉ thành 2 nửa phong thư (HP 35%), mỗi nửa chết thành 2 con tem nhỏ (HP 12%); con tem không tách tiếp. |
| Party Line | Platform · 8–14 | Đứng yên, tạo vùng | Không di chuyển; reo 0.9 s rồi căng dây điện thoại ngang sàn tới một điểm gần hero, chạm vào bị thương, dây giữ 3 s. |
| Door Knocker | House · 11–15 | Sóng theo nhịp | Đáp xuống, gõ 3 nhịp (0.7 s/nhịp) mỗi nhịp một sóng lan, nhịp 3 to nhất; tiếng gõ là tín hiệu. |
| Lamp Snuffer | 9–20 | Nhắm đèn | Trôi tới đèn lồng gần nhất và làm đèn tối dần (giảm vùng sáng) đến khi bị hạ; mọi cảnh báo vẫn vẽ trên lớp sương. |
| Ledger Hound | Perfect Room · 16–20 | Chém có báo trước | Chạy vòng quanh hero, vạch 4 gạch đếm trên sàn, gạch thứ 5 là một nhát chém chéo theo đường đã vạch. |

## 2. Sáu sửa đổi

1. **Coin Leech → Toll Tick**: thu phí thay vì cướp; để lại hóa đơn, hạ nó thì hoàn lại số vàng đã thu + 10%. (Luật thế giới: không gì bị lấy, chỉ được mua.)
2. **Spore Bishop → Coat Rack**: giá treo áo phủ bụi; vũng nổ chậm giữ nguyên, nay là bụi lắng xuống.
3. **Smoking Coffin → Unclaimed Trunk**: rương thất lạc, thả ra tất lẻ và bướm đêm thay cho triệu hồi cũ (giữ hành vi).
4. **Hopper Keg → Hatbox Hopper**: hộp mũ trên lò xo (giữ hành vi nhảy vào điểm đánh dấu).
5. **Tên và mô tả**: thêm dấu cách cho 10 tên dính liền (HushMite → Hush Mite...); RuneWisp → Wick Wisp; SirenSeal → Lullaby Seal (đợt sóng thứ hai ngắt sớm như bài ru wave 7); BristleImp → Splinter Imp (dằm gỗ, bản lề cửa); sửa mô tả The Last Customer (đang chép của Undelivered King).
6. **Affix và elite**: lửa của affix là "lửa trộm từ đèn vỡ" (lời mô tả); elite "Gilded X" → "Indebted X" có dấu mực hóa đơn (`Enemy_IndebtedStamp`); quái mới chia theo khu để wave 11–20 có quái riêng.

## 3. Thứ tự làm

1. Không cần art: tên, mô tả, elite Indebted (dùng dấu vẽ mực cho tới khi có art), Toll Tick hoàn tiền.
2. Có art rồi mới bật: 8 quái mới và 5 hình mới (Toll Tick, Coat Rack, Unclaimed Trunk, Hatbox Hopper, Splinter Imp). Quái thiếu art thì ở ngoài game.
