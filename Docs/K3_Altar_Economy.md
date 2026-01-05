# K3.2b — Altar HP và activation

Ngày 2026-10-13. Windows candidate. Cơ chế chạy thật nhưng công thức **AUTHORED**, chưa coi là Bizarre Brigade 1.5+ parity. Merge inheritance giữ nguyên random weapon hiện có; chưa sửa theo suy đoán.

## Triển khai
- Thêm ShopCurrency.Health sau Gold/Eggs, không đổi ordinal cũ hoặc schema 2. Mua HP trừ **HP hiện tại**, không giảm max HP, không dùng gold/eggs thay thế. Kiểm affordability trước nhận item; giao dịch phải chừa ít nhất 1 HP, kể cả item được mua có bonus health.
- Modifier health của item vẫn áp dụng bình thường sau affordability; receipt ghi delta HP thực, không gọi delta net là giá item. Giá HP hiện rõ tại quầy, tooltip và preview HP sau trả; sold không tiếp tục dự báo như đang mua lại.
- Item altar có currency/price riêng. Tuning hiện tại: rarity Rare/Mythic trả ceil(30% giá vàng), minimum 5 HP; item khác trả 2 eggs. Đây không phải bảng giá reference. Chưa có altar-exclusive content/pool.
- AltarRevision 1 dùng một activation roll từ ShopRandom tại lúc tạo rest mới, chance clamp(10% + eggs hiện giữ × 5%). Kích hoạt không tiêu eggs. Chance dùng eggs sau settle; không có phí nuôi/sell/lock. Ba offers altar được lưu, load không roll lại; reroll vàng không đổi stock altar.
- Catalog revision 0 vẫn hỗ trợ lịch SpecialShopEvery prototype để regression/legacy authoring. Migration Apply một lần, giữ IDs/catalog version và offers save đang có. Hiệu lực mới áp dụng ở rest được tạo tiếp theo, không tự thêm quầy khi load shop cũ.
- Details có dự báo chance theo số eggs hiện tại, ghi rõ đây là dự báo, không phải kết quả roll của rest đang đứng. Altar/receipt đọc dữ liệu thật, không tiêu RNG khi mở UI.
- Atomic pipeline K3.1 bảo vệ HP, item, offers, receipts và hai RNG nếu operation/persist lỗi; current HP được đưa vào combat khi Begin lại sau purchase thành công.

## Kiểm tra
- Regression giữ checkpoint 17, squad 12, full-run 96, K0/K1 27, K2 22, transaction 36, modifier/economy 29 PASS.
- 14 altar checks: save-failure rollback, HP-only spend, receipt, max HP bất biến, lethal purchase guard, còn đúng 1 HP, chance clamp/giá trị, zero/certain activation, saved RNG reproducibility, resume không roll lại, reroll failure, checksum round-trip Health currency, idempotent authoring.
- Reports: `PreviewExports/K3/AltarEconomy/Checks.txt`; build `PreviewExports/FullRun_Build.txt`. Menu: **Arcane Brigade > K3 > Apply altar slice and build**, sau lần nâng đầu dùng **Check altar economy and build**.
- Native diagnostic `-arcane-altar-capture`: seeded shop/wallet/item/position và mixed-currency stock fixtures; mua HP qua WorldShop.Interact, kiểm HP giảm nhưng gold/eggs không đổi, SaveRun thật trong root riêng. PNG/notes tại `PreviewExports/K3/AltarEconomy/Player/`. Không phải chơi hết run hay kiểm Android/iOS.

## Research / còn thiếu
Nguồn đối chiếu lịch sử có trong `Bizarre_Brigade_15_Reference_Audit.md`: [footage 1.5](https://www.youtube.com/watch?v=70OufN07abE), [activation tutorial](https://zzz.honeyhunterworld.com/999130701-tutorial/?lang=EN), [purchase/formula tutorial](https://zzz.honeyhunterworld.com/999130702-tutorial/?lang=EN). Lượt này search có mục tiêu không trả evidence hữu ích để xác nhận current/max HP, chance hay bảng giá. Không lấy kết quả game khác làm thông tin Bizarre Brigade.

K3.2 vẫn còn xác minh parity/inheritance, altar-exclusive pool/recipe previews và quyết định tuning qua playtest. K3.3 tiếp theo là cause/proc và chest/egg/heart/luck/indicator; K3.4 end-to-end/device gate. Không gọi toàn bộ K3 hoàn tất, không tự chuyển K4.

## Source control
Source đầy đủ gồm Assets (code, data, model/sprite, prefab, animation, scene, fonts và .meta), Packages, ProjectSettings, Docs, README và PreviewExports. Git LFS theo .gitattributes cho ảnh/font/archive. Build executable, Library/Temp/UserSettings và diagnostic .utmp không đưa vào Git; chúng được tạo lại bằng Unity/menu, không phải thiếu source. Clone bằng git + git lfs pull trước mở Unity 6000.4.8f1.
