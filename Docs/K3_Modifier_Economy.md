# K3.2a — Modifier và shop economy

Tiếp nối K3.2b: `K3_Altar_Economy.md`. Các dòng "chưa thêm HP/chance" bên dưới phản ánh thời điểm K3.2a, không phải trạng thái runtime mới nhất.

Ngày 2026-10-13. Windows candidate, chưa hoàn tất toàn bộ K3.2/K3. Không chuyển qua K4 chỉ vì lát cắt này đã có build.

## Đã triển khai
- Item có modifier list, scope Team/Champion/Companions/Run, rarity, trọng số, pool gold/egg, stack policy và stack limit. List rỗng vẫn đọc Effect/Value cũ; list có nội dung thay thế, không cộng lại legacy effect. Hiện hỗ trợ additive modifiers, chưa có flat/multiply/trigger registry.
- Bonus theo target được dùng thống nhất trong combat, Details, receipt. Damage/attack speed/movement/crit/projectile hỗ trợ scope đội/Champion/đồng minh; health/armor/regen chỉ Champion; economic/radius/interceptor thuộc run. Scope chưa hỗ trợ bị từ chối thay vì quảng cáo hiệu ứng không tồn tại.
- Stack additive có limit data; Unique có guard mua và effective count. Tất cả item trong catalog bàn giao vẫn additive với limit kỹ thuật cũ 100000; không áp Unique lên item đang chơi hoặc đổi luật cộng dồn đã chốt. Legacy ownership dư limit không bị xóa.
- Weighted item pool lọc wave, currency pool, weight và stack limit. Recruit hết biến thể weapon không vào pool. Khi một nhóm hết offer dùng nhóm còn hợp lệ, giữ ba quầy. Chưa có weighting theo build hoặc companion-specific weight; duplicate item offers vẫn có thể xuất hiện, không tự nhận đây là parity reference.
- Reroll dùng counter đã lưu: giá gốc 4, tăng 2 mỗi lần tại cùng rest; reset tại rest mới. Counter/price/pools/RNG cùng rollback qua pipeline K3.1. Coupon chỉ giảm giá offer vàng roll tương lai, cap 50%, không giảm reroll fee; stock đã lưu không được repricing hoặc reroll khi load.
- Rarity hiện gán theo wave/price, weights 100/60/25/8. Đây là **AUTHORED tuning**, không phải rarity/drop table hoặc công thức giá của Bizarre Brigade.
- Hai item có lợi/hại thật: Long Atlas giữ +1 leash, thêm -4% movement toàn đội; Echo Postage giữ +2 projectile, thêm -8% attack speed toàn đội. Receipt và Details ghi cả trade-off; hệ số damage/speed/cooldown có floor kỹ thuật hiện hiển thị trong read model.
- Tab thứ tư **LIÊN KẾT** có ba rule ID: engine-clockwork, engine-economy, engine-patrol. Tiến độ lấy từ item IDs/count thật, hiển thị nguồn/scope/stack; không có bonus set ẩn hoặc graph contribution giả. Đây là nền đọc build N02, không phải proc synergy hoàn chỉnh.
- Tooltip proximity có rarity, phạm vi, per-stack và tổng; nút USE tắt nếu offer không mua được. Rarity label world-space dùng Noto tiếng Việt và anchor riêng tránh chồng tên/giá; caption reroll có giá tiếp theo ngay cạnh USE. Không thêm shop modal, sell/lock/upkeep hoặc combat input.

## Data/save và kiểm chứng
- `K32Builder.Apply` chỉ nâng catalog EconomyRevision từ 0 lên 1 một lần, không reset scene/model/IDs/catalog.Version; gọi lại không overwrite tuning. Runtime save vẫn schema 2, không thêm RNG stream hoặc sửa offer đã lưu khi resume.
- Hai trade-off trên là thay đổi cân bằng có chủ ý cho cả run đang resume dùng catalog mới; không tuyên bố stats của hai item đó giữ nguyên. Những item khác chuyển legacy thành modifier tương đương.
- Menu: **Arcane Brigade > K3 > Apply modifier economy slice and build** để nâng catalog cũ; sau đó **Check modifier economy and build** để kiểm tra/build không reset authoring.
- Regression: checkpoint 17, squad 12, full-run 96, K0/K1 27, K2 22, transaction K3.1 36; checks mới tại `PreviewExports/K3/ModifierEconomy/Checks.txt`. Distribution test lấy 600 item offers qua 300 rolls, tính là một assertion trọng số chứ không báo thành 300 feature tests.
- K3.2a: 29 checks PASS; Unity Windows build Succeeded, 0 errors/0 warnings; native purchase/reroll/save-boundary capture PASS, player exit 0. Không tính những giá trị này thành mobile/full-run playtest PASS.
- Native capture dùng `-arcane-k32-capture`: seed wave/shop/wallet/owned items và vị trí là fixture; mua và reroll qua WorldShop.Interact, SaveRun thật ở root diagnostic. Capture kiểm disk snapshot và reroll charge/sign, không phải full-run playtest. PNG và notes: `PreviewExports/K3/ModifierEconomy/Player/`.
- Xem trực tiếp capture để sửa rarity/name overlap, tooltip trade-off, receipt và tab liên kết. Windows build/capture không xác nhận Android/iOS/Steam hoặc art/audio production.

## Research và phần còn lại
Đối chiếu đã có trong [audit nội bộ](Bizarre_Brigade_15_Reference_Audit.md): [footage 1.5](https://www.youtube.com/watch?v=70OufN07abE), tutorial trích từ game về [altar activation](https://zzz.honeyhunterworld.com/999130701-tutorial/?lang=EN) và [altar purchase/formula](https://zzz.honeyhunterworld.com/999130702-tutorial/?lang=EN). Lượt này thử truy cập lại: video không fetch được, tutorial 403; không có evidence mới xác nhận công thức số. Không suy ra số từ Reddit hoặc guide của game khác có tên giống nhau.

K3.2b còn: targeted visual verification và quyết định altar HP current/max, chance theo eggs, price/rarity/inheritance, altar-exclusive pools, flat/multiply nếu cần. Altar đang giữ recipe egg-only và lịch special shop prototype; chưa thêm HP currency hoặc tự chọn xác suất rồi gọi là reference. K3.3/K3.4 còn cause/proc guard, chest/egg/heart/luck/detection/marker và gate end-to-end/device.
