# K3.1 — Giao dịch, biên lai và migration

Báo cáo tiếp nối K3.2a: `K3_Modifier_Economy.md`; phần chưa làm bên dưới phản ánh thời điểm bàn giao K3.1.

Ngày 2026-10-13. Phạm vi: lát cắt đầu của K3, Windows candidate; không đánh dấu toàn bộ K3/N03/N17 hoàn tất.

## Runtime
- `RunTransaction.TryCommit` dùng before snapshot, thực hiện operation, validate, xây biên lai before/after, rồi persist. False/exception/save lỗi phục hồi snapshot, hai RNG, roster/material, tiền, stock, reroll counter và biên lai; không dựng lại simulation hoặc báo mua thành công.
- ArenaController chỉ giao dịch khi rest hợp lệ, không pause/transition. WorldShop trả kết quả commit thật cho mua/reroll/recipe; bước qua hall trả phase thực, không mặc định thành công.
- Schema 2 thêm counter reroll và tám biên lai gần nhất. Schema 0/1 được nhận; schema tương lai bị từ chối. IDs, catalog version, random streams, checksum/atomic replace/backup giữ nguyên. Baseline K0 và manifest K0–K2 giữ nguyên như artifact lịch sử.
- Biên lai lưu cùng gameplay snapshot: gold/egg, các body/weapon/tier thêm hoặc mất, item stack, từng stat Champion/đồng minh thay đổi, bonds và dự báo lãi. Chỉ ghi stat thực từ read model, không dựng contribution graph giả; giá trị hiển thị có precision hữu hạn.
- Shop vẫn là ba quầy world-space. Biên lai là nút mở panel nhỏ tại rest, không biến mua đồ thành modal shop. Panel co theo nội dung, scroll nếu dài; Details/Vật phẩm có lịch sử read-only, mới nhất trước, trước phần bonds.

## Kiểm chứng
- Recheck trước K3: checkpoint 17, squad 12, full-run 96, K0/K1 27, K2 22 PASS.
- Regression sau K3.1: các bộ trên giữ PASS; 36 kiểm tra K3 PASS trong `PreviewExports/K3/K3_Transaction_Checks.txt`.
- K3 bao gồm failure injection operation false/throw, persistence false/throw, rollback JSON, mua gold/egg, chống mua lại, merge ba thành một, recipe material/eggs/RNG rollback, stat HP UI/combat, delta companion-only, checksum resume, schema 1 migration, lịch sử rollover và chặn mua trong combat. K0/K1 giữ test revision 0 và future schema.
- Unity Windows build: Succeeded, 0 errors, 0 warnings; `PreviewExports/FullRun_Build.txt`. Menu kiểm tra/build: **Arcane Brigade > K3 > Check transactions and build**. Không dùng baseline builder reset content.
- Capture native: `PreviewExports/K3/Player/World_Stand_Receipt_Fixture.png`, `Details_Receipt_History_Fixture.png`, `Capture_Notes.txt`. Fixture seed shop/wallet và vị trí đứng, nhưng mua qua `WorldShop.Interact`, giao dịch thật và save thật ở root diagnostic riêng. Player kiểm snapshot trên disk, exit 0/PASS. Không gọi đây là chơi hết run hoặc kiểm mobile.
- Capture được xem trực tiếp để kiểm chữ Việt, panel/safe viewport, không chồng nút USE; chỉ hiện stat thay đổi, tránh lặp cả bảng stat không đổi.

## Tiếp theo / chưa hoàn tất
K3.2: modifier/scope/rarity/stack, weighted pools, build links và research economy 1.5+ có mục tiêu. Reroll counter đã lưu nhưng giá vẫn cố định theo tuning hiện có. Altar hiện vẫn recipe egg-only, chưa có HP/activation chance. K3.3: cause/proc guard, chest/egg/heart, luck/detection và indicator ngoài viewport. K3.4: end-to-end/device/art gate. Chưa nghiệm thu Android/iOS/Steam hoặc art/audio production.
