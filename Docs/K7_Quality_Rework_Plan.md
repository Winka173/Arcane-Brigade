# K7 — quality rework theo feedback 2026-10-14

Audio/music/UI/art hiện tại bị chủ dự án từ chối; không giữ trạng thái quality approved. Tự quyết định chi tiết và triển khai, không chờ confirm giữa chừng. Windows first; mobile/Steam gate vẫn mở.

1. IMPLEMENTED — research footage1.5+ frame/timing/menu, evidence và tuning riêng ở `K7_Reference_Rework_Audit.md`.
2. IMPLEMENTED — combat HUD tối giản, Details giữ dữ liệu sâu, art-led landing/Coiny/smooth gradient.
3. IMPLEMENTED — spring focus, empty sold pedestal, actual committed drop/walkout/merge; native assertions + framebuffer clip.
4. IMPLEMENTED — enemy dark/red-eye silhouette,48character frames +upper/lower rig +16painted VFX +4shop props; full prompts/meta trong manifest.
5. IMPLEMENTED — melee sway/turn, boomerang/2ray/pierce, baseline magnet1.7; bỏ cosmetic target rings, giữ hazard warnings.
6. IMPLEMENTED — sampled Foley/composed jazzCC0 thay beep default, credits/audition, regression/native/perf/video/Windows build. Handoff/source theo final checkpoint; không gọi implementation là human approval hoặc all-device production DONE.

Nguồn cục bộ: `.utmp/art-reference/bizarre15-belle.mp4`, `bizarre15-fortress.webm`, frames đã đối chiếu. Footage Belle 30s/45s: HUD HP + tiền + wave/timer + score nhỏ; địch blob tối mắt đỏ khác champion/ally, shop3pedestal trong map. Không suy ra mọi attack chỉ bằng screenshot; cần chuỗi frame.

Ngoại tham khảo: [Hades menu screenshot](https://interfaceingame.com/screenshots/hades-main-menu/) — art-led, short vertical actions, ít chrome; [Supergiant Hades](https://www.supergiantgames.com/games/hades/) — primary artwork/gameplay presentation. Thiết kế mới là adaptation nguyên gốc cho Arcane Brigade, không nhập asset của game khác.

Không tính test/render PASS là chủ dự án duyệt art/audio/feel; không dùng fixture20hall thắng làm playtest người thật. Raster/sprite mới phải có source/prompt/meta, ảnh Unity kiểm tra sau tích hợp.
