# K3 — Runtime và gate cuối

Ngày 2026-10-13. **K3 hoàn tất theo gate runtime Windows đã được chủ dự án chốt. Device gate chuyển sang K7 theo xác nhận trực tiếp.** Đây vẫn là Windows candidate, không phải production/đa nền tảng hoặc chứng nhận exact-reference parity. Chưa triển khai K4.

## Runtime đã có
- Atomic mua/reroll/recipe, rollback tiền/HP/roster/material/stock và hai RNG, receipt committed, schema 2 nhận save 0/1, checksum/backup giữ nguyên.
- Additive modifier list/scope/rarity/stack, weighted eligible item pools gold/altar; companion uniform trong pool còn weapon variant. Reroll giá tăng theo rest; discount chỉ offer roll tương lai. Gold/eggs/current HP riêng, shop ba quầy world-space. Không sell/lock/upkeep hoặc nút đánh/skill.
- Altar exclusive **Altar Ember**: +18% Champion damage/-3% team movement. Altar reroll dùng gold, chỉ đổi stock altar, không roll lại activation hoặc đổi gold stock; hai shop dùng chung counter rest theo tuning riêng.
- Recipe có minimum material/result tier data, preview body/weapon/tier bị tiêu thật, affordable/unique-result guard và rollback. Merge giữ ba cùng loại/cấp; weapon kết quả random trong variant hợp lệ, không cho chọn. Giữ hành vi AUTHORED, không gọi là inheritance reference đã xác minh.
- N17 item preview clone snapshot/read model, không đổi live state/RNG; hiện HP cost và stat Champion/đồng minh/proc tăng giảm. Companion không preview một rolled weapon như đã chốt. Tooltip scroll; Details có item/bond source breakdown trước clamp/tier/passive. N02 links từ ID/count, thêm Kho Báu Thì Thầm, không bonus set/contribution graph giả.
- Cause root/source/item/depth trên damage/hit/kill/pickup/chest/proc và attack cues; projectile mang source ID, burn/reaction/chain giữ child cause. Proc registry trigger/action/chance/CD/power/radius; root-only và dedup rule/root trong batch, child không kích proc.
- **Coin Salve** hồi HP khi nhặt coin, **Chest Thunder** damage pulse khi mở rương. Details hiển thị trigger/power/stack/CD/chance thực, không gọi proc healing là regen.
- Coin/egg/heart/chest có stable runtime ID, type/value/scatter/attraction/collect và event. Chest mở một lần khi đi vào radius 0.85, không auto-attract hoặc credit trước pickup; reward là drops thật. Heart hồi actual HP, egg settle một lần cộng pickup + hall reward cũ.
- RNG loot/proc tách combat/spawn/shop/weapon; presentation không tiêu RNG gameplay. Resume vẫn boundary/seed, không giả vờ restore projectile/proc cooldown giữa combat.
- Luck tăng egg/heart/chest reward chance clamp; Detection giảm chest interval, tối đa bốn chest đồng thời. Icon vector-derived 256px gọn, anti-alias, bob/pop/shadow/glint; chest open ring/flash/audio và proc cue. Đây là art/audio candidate, không final production.
- N18 chỉ chest/egg ngoài camera: group kind/direction, chest/near priority, cap sáu, tránh overlap, fade pause-aware. Marker design safe rect nằm trong viewport/safe-area; không coin/heart markers, không xóa logical loot hoặc auto-path người chơi.

## Gate đã chạy
| Bộ kiểm | Kết quả |
| --- | --- |
| Checkpoint / Squad / Full-run regression | 17 / 12 / 96 PASS |
| K0/K1 / K2 | 27 / 22 PASS |
| Transaction / Modifier economy / Altar | 36 / 29 / 14 PASS |
| K3 final | 31 PASS |
| Unity Windows build | Succeeded, 0 errors, 0 warnings |
| Native K3 final capture | PASS, player exit 0 |

Final checks: pause mọi loot/proc; collect một lần/đúng currency; attributed proc kill; cooldown/chance/không recurse; chest ngoài viewport; marker group/priority/cap/visibility/pickup/safe rect; atomic altar reroll; live-RNG read-only preview; recipe material/tier; egg settle một lần; proc reconstruction sau resume; treasure determinism/detection; loot RNG không đổi crit combat. Reports: `PreviewExports/K3/Final/Checks.txt` và các bộ checkpoint trước.

Native capture dùng seeded wallet/items/loot positions, disabled ambient spawning, rest/offer và scripted move-target fixtures ở root save riêng. Movement → mở chest → collect reward → proc/cue chạy normal runtime; mua physical stand rồi checksum disk resume. **Không phải chơi hết run tự nhiên.** 72 motion frames buffer trước encode, timestamp CSV thật; viewer `PreviewExports/K3/Final/Player/index.html`. PNG marker/proc/preview đã xem trực tiếp và chỉnh layout. Viewer ~60 FPS chỉ để xem, không dùng đo hiệu năng.

`FullRun_Simulation_Benchmark.txt` chỉ đo Editor simulation, không đo GPU/mobile. Không công bố FPS release/stress/device PASS từ kết quả này.

## Chơi / build
- Unity 6000.4.8f1, scene `Assets/ArcaneBrigade/Scenes/RunBoot.unity`; player `Builds/FullRun/ArcaneBrigade.exe`, giữ thư mục đi kèm.
- **Arcane Brigade > K3 > Check final checkpoint and build** không reset content. Nâng catalog cũ bằng **Apply final loot proc slice and build**; idempotent, giữ IDs/catalog version.
- Chỉ move trong combat; tại rest Space/USE. Chest mở bằng đi tới, không thêm skill/input.
- Source, assets/.meta, docs/capture được đưa vào Git/LFS; Build/Library/Temp/UserSettings/.utmp không commit.

## Acceptance còn mở
- **Mobile — K7:** Unity chỉ có WindowsStandalone/WebGL modules; adb trả emulator-5554 offline, không có device online. Chưa build/test Android/iOS hoặc prerequisites macOS/iOS. Chủ dự án đã chọn "Chốt runtime Windows, device gate sang K7"; đây là thay đổi gate đã được xác nhận, không phải quyết định tự suy ra.
- **Reference parity:** current-vs-max HP, chance/pricing/drop tables/inheritance chưa có evidence đủ. Giữ AUTHORED tuning và nêu rõ UI/docs; không lấy số từ game khác rồi gọi reference. Runtime dùng được nhưng không nhận nhãn parity hoàn chỉnh.
- Merchant/stand artwork hoàn thiện, signature sâu, contribution/results, mobile lifecycle/controller/Steam release thuộc các gate K4–K7 còn lại; không suy ra hoàn tất từ Windows.

**Trạng thái:** K3 DONE theo scope/gate Windows được duyệt; có thể tiếp tục K4. Mobile acceptance tại K7 và exact-reference/art/audio production vẫn chưa nghiệm thu. AUTHORED tuning là baseline runtime hiện tại, không tự nhận số liệu reference đã xác minh.
