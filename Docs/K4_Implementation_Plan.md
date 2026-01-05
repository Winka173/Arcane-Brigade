# K4 — kế hoạch thực thi signature

Ngày 2026-10-13. Dependency K3 Windows đạt; HD/4K pass đã push. Font/UI redesign để K6, final polish/device ở K7 theo feedback chủ dự án.

| Bước | Phạm vi | Gate |
| --- | --- | --- |
| K4.1 — Windows candidate đạt | Ô/BellFork và DreamStamp: cadence/target/mark/payoff/tier; art dấu riêng, pose/cue/SFX và Details từ data | 40 checks + regression PASS; native Windows autocombat capture PASS. Art/audio/balance vẫn cần playtest, không gọi toàn K4 DONE |
| K4.2 — tiếp theo | Máy thu hoạch / Văn phòng hoàn trả, connector, graph hữu hạn và contribution đo thật | Hai thành phần đã có payoff, không proc recurse/farm tiền |
| K4.3 | Merge reveal queue, tier silhouette, idle personality ngắn | Commit trước reveal; skip/rollback/load không roll hoặc tiêu thêm |
| K4.4 | Chuông sân khấu / Xe đồ chơi, layout seed, một boss phase đổi sân | Không manual skill; warning đủ, không gây kẹt hoặc one-hit |
| K4.5 | Weapon sandbox read-only, data/RNG/save riêng; nghiệm thu lát cắt xuyên run | Combat → loot → shop/merge → build → boss/rest; không farm meta/run |

## K4.1 contract

- Không đổi ID cũ, không thay silhouette HD vừa duyệt; thêm `DreamStamp` vào pool tuyển ngẫu nhiên. Không shop chọn weapon, không đổi merge/inheritance, không đổi save schema.
- Umbrella giữ thrust/Frost gốc; mỗi ba đòn bung phía trước, push enemy có giới hạn/boss resistance; không chặn đạn. Tier ≥2 chỉ rộng hơn 25%.
- Bell giữ projectile/Arcane gốc; mỗi ba đòn thêm sóng tròn trễ. Tier ≥2 thêm đúng một echo yếu, không echo từ echo.
- Stamp chỉ đánh mục tiêu đã lock khi windup nếu vẫn trong range. Ba dấu cùng source-target mới burst; dấu hết nếu bỏ mục tiêu quá lâu. Tier ≥2 truyền một dấu sang một enemy gần, không burst từ truyền dấu.
- Payoff `CombatCause.Depth=1`, không kích hoạt loot procs. Một logical kill tạo loot đúng một lần. Mark/pulse là combat state reset tại boundary như projectile, không pretend resume mid-wave.
- Animation/particle không là source of truth. Nhịp đổi gameplay ngay cả khi tắt VFX. SFX tổng hợp nguyên bản chỉ là cue study, chưa nghiệm thu soundtrack.
