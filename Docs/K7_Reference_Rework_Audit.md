# K7 — research phục vụ quality rework

2026-10-14. Đây là đối chiếu presentation/motion, không tuyên bố đã test mọi nhân vật/item của ZZZ hoặc sao chép exact công thức.

## Footage kiểm trực tiếp

- [Belle difficulty5 v1.5](https://www.youtube.com/watch?v=70OufN07abE): local video720p60fps. Frame30s cho HUD HP/currency/wave/timer nhỏ, địch blob tối mắt đỏ khác nhân vật. Chuỗi45–48s4samples/sec cho empty pedestal sau mua, companion xuất hiện trên quầy, tooltip theo khoảng cách và merge. Đây không phải dữ liệu đo exact spring curve/giá/weapon odds.
- [Boo Fortress difficulty5 v1.5](https://www.youtube.com/watch?v=tqvQPCo51xc): local360p30fps. Chuỗi25–26.5s8samples/sec cho nhiều projectile kéo vệt theo hướng bay;25s7timer→26s6timer và coins nằm ở arena.120s là shop, không gắn nhãn nhầm là combat. Quan sát không đủ xác nhận mọi bounce/pierce/boomerang stat.
- Nguồn/frame cục bộ chỉ tại `.utmp/art-reference`, bị Git ignore; không import vào Assets. Thuật ngữ rộng/luật hệ thống xem audit1.5 trước đó.

## Menu ngoại tham khảo

[Hades main menu screenshot](https://interfaceingame.com/screenshots/hades-main-menu/) và [Supergiant Hades page](https://www.supergiantgames.com/games/hades/): lấy hierarchy art-led và short primary actions, không lấy artwork/font/logo của Hades. Arcane Brigade dùng LostShop_v1 nguyên gốc đã có + Coiny OFL, giữ roster/museum/records/settings/help nhưng bớt chrome và diễn giải trên landing.

## Quyết định trong rework

- HUD combat: portrait/HP,2currency, wave/timer,2small controls. Bỏ bonds/difficulty/ally-count/tutorial/footer/income banners; dữ liệu sâu ở Details/result, không xóa mechanic hoặc save.
- Shop:3option trong map, ghế trưng bày/altar art riêng, price strips nhỏ; proximity spring + settle, sold-empty. Actual commit mới mở arrival/merge; rollback không mở animation. Ally nhảy xuống rồi đi tới squad; merge gom material silhouettes → actual result +stars +painted sparkle, không ĐÃ MUA.
- Friendly: round/brighter silhouettes; enemies: dark/jagged + red eyes.48 character states + runtime upper/lower segmentation, breathing, blink/feet selection;16painted dust/slash/magic/death frames. Không gọi4frames là hàng chục bespoke attack/death poses hoặc full bone rig.
- Không vẽ target/hit rings hoặc circle cho buff/proc/merge mặc định. Boss/arena hazard telegraphs giữ để né; F3 leash chỉ là debug opt-in. Bullet core/halo là projectile, không reticle.
- Melee: slow idle weapon sway/turn, windup/strike/recovery và painted slash; attack pose giữ đúng hướng damage, không smoothing làm đòn đánh lệch. TinFish quay khi bay, quay về sau hit/0.7s, có thể trúng target khác trên đường về; cùng target chỉ primaryhit một lần mỗi throw. Mage2ray mặc định, pierce1; bow pierce2; modifiers vẫn cộng ray như trước. Đây là tuning riêng, không ghi exact-reference stats. Arcane secondary chain vẫn là cơ chế riêng, không bị xóa bởi primary victim dedupe.
- Coins: baseline1.7world-unit hút vào main, itemPickup vẫn cộng bán kính; landing/spin/glint/pickup feedback. Không auto-pickup mọi khoảng cách hoặc thaychest interaction.
- Audio: researched licensed Kenney Foley và composed jazz CC0 thay default procedural beep; không rip âm thanh reference. Source/license/default-vs-fallback rõ trong K7_Audio_Provenance.

## Quality gate

Không chờ confirm để sửa và rebuild. Việc agent xem PNG/test không bằng chủ dự án duyệt audio/feel; Windows runtime/capture/perf báo riêng. Android/iOS hardware, controller vật lý và Steam vẫn OPEN theo quyết định trước.
