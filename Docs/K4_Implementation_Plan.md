# K4 — kế hoạch thực thi signature

Ngày 2026-10-13. Dependency K3 Windows đạt; HD/4K pass đã push. Font/UI redesign để K6, final polish/device ở K7 theo feedback chủ dự án.

**Trạng thái cuối: K4 DONE theo functional gate runtime Windows.** Báo cáo nghiệm thu và giới hạn: `K4_Final_Checkpoint.md`. Không coi DONE là mobile/production art/audio/balance approval; device gate K7 đã được chủ dự án duyệt tách riêng.

| Bước | Phạm vi | Gate |
| --- | --- | --- |
| K4.1 — DONE Windows | Ô/BellFork và DreamStamp: cadence/target/mark/payoff/tier; art dấu riêng, pose/cue/SFX và Details từ data | 40 checks + regression PASS; native autocombat capture PASS |
| K4.2 — DONE Windows | Máy thu hoạch / Văn phòng hoàn trả, Carbon Ribbon connector, graph hữu hạn và contribution hiệu lực | Hai thành phần hoạt động, ledger không overkill/overheal/double-settle; native graph/bonus PASS |
| K4.3 — DONE Windows | Post-save merge/altar reveal queue, tier silhouette, idle personality và reduced motion | Rollback/duplicate/skip/reload checks; native purchase→merge finish/skip PASS |
| K4.4 — DONE Windows | Chuông sân khấu / Xe đồ chơi, seed/wave layout, boss half-health phase đổi sân | 49 checks; native impact→warning→payoff và boss→shift→rest PASS |
| K4.5 — DONE Windows | Discovered-only weapon sandbox, cloned data/RNG/render scene, tier/target preset, lifecycle | 174 checks; native menu preview/reset/exit và save bytes unchanged PASS |

## K4.1 contract

- Không đổi ID cũ, không thay silhouette HD vừa duyệt; thêm `DreamStamp` vào pool tuyển ngẫu nhiên. Không shop chọn weapon, không đổi merge/inheritance, không đổi save schema.
- Umbrella giữ thrust/Frost gốc; mỗi ba đòn bung phía trước, push enemy có giới hạn/boss resistance; không chặn đạn. Tier ≥2 chỉ rộng hơn 25%.
- Bell giữ projectile/Arcane gốc; mỗi ba đòn thêm sóng tròn trễ. Tier ≥2 thêm đúng một echo yếu, không echo từ echo.
- Stamp chỉ đánh mục tiêu đã lock khi windup nếu vẫn trong range. Ba dấu cùng source-target mới burst; dấu hết nếu bỏ mục tiêu quá lâu. Tier ≥2 truyền một dấu sang một enemy gần, không burst từ truyền dấu.
- Payoff `CombatCause.Depth=1`, không kích hoạt loot procs. Một logical kill tạo loot đúng một lần. Mark/pulse là combat state reset tại boundary như projectile, không pretend resume mid-wave.
- Animation/particle không là source of truth. Nhịp đổi gameplay ngay cả khi tắt VFX. SFX tổng hợp nguyên bản chỉ là cue study, chưa nghiệm thu soundtrack.
