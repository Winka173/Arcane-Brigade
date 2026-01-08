# K7B — night market / living squad / icon HUD

2026-10-14. Windows candidate thay UI shop tím/viền vàng K7. Không chờ confirm giữa bước; không tự ghi nhận human art/feel approval.

## Đã triển khai

- UI vé hội chợ: giấy kem, dấu đỏ/mint, giá bằng currency icon + số; không mesh box ghi giá cũ. Tag dịch sang bên khi focus để không che người mua. Menu/button dùng ticket skin và hover tween.
- Merchant vàng và merchant giấc mơ có4frame riêng + sway/breath/blink/cheer. Main merchant cao3.15 world-units, gần4 lần offer actor0.8; đã hạ tâm xuống2.7 để không cắt đầu. Header wave ở shop dịch sang trái, không đè lên merchant.
- Bóng mềm + bóng tiếp xúc merchant; ground contact cho pedestal, máy reroll và cửa. Actor depth-sort theo vị trí, icon vũ khí được giới hạn cả chiều rộng lẫn cao.
- Máy reroll2frame và cửa xuống2frame là art độc lập, không dùng chung bàn offer. Exit ở(0,-4.3), player tự bước xuống0.9s, curtain chuyển rồi fade-in vào wave tiếp theo; chỉ advance một lần, giữ persist/rollback.
- Shop actor/item có idle; item mua thành công bay vào người, merchant/fixture cheer. Thiếu tiền shake + flash đỏ + caption nhỏ, không consume stock/currency/receipt. Recruit/merge committed feedback trước đó vẫn giữ.
- Item Uncommon xanh lam, Rare tím, Mythic vàng: soft aura + light shafts có pulse, không vòng tròn mục tiêu. Common không glow. Shop đặc biệt dùng trứng hoặc HP theo món; icon trái tim và mô tả HP hiện tại, không giảm maxHP, luôn để lại ít nhất1HP.
- Góc phải chỉ icon Hearth / Archive / Clockwork / Frost cùng số body/ngưỡng, không tên thường trực; tên/thưởng chỉ trong hover/click tooltip.4icon nguyên gốc mới, không dùng chữ thay icon.
- Idle thân trên rõ hơn, blink độc lập, breathing4.5%/sway2.4°; không tính lắc vũ khí là body idle. Khi champion đứng yên, mỗi ally tự chọn nghỉ0.6–1.8s hoặc roam; vẫn separation/leash/auto-attack, RNG roam riêng.
- Wave thường:35s đầu, +2s mỗi wave tới55s. Wave1 chặn toàn bộ enemy-projectile spawn routes, wave2 mở ranged pool. Đạn thường≤2.4 units/s; boss pattern1.8–2.4, ally projectile không giảm. Boss giữ luật phải hạ, **không tự coi timer0 là thắng**; boss HP multiplier0.55 bù nhịp/economy ngắn hơn. Vì vậy boss vẫn có thể vượt thời gian wave thường.

## Research / provenance

Trực tiếp đối chiếu [Belle1.5](https://www.youtube.com/watch?v=70OufN07abE) tại29–31s và46/58s; [Fortress1.5](https://www.youtube.com/watch?v=tqvQPCo51xc) tại24–26s. Quan sát merchant lớn, goods có motion, hệ bằng icon/threshold, price/currency theo offer. Source footage chỉ ở`.utmp`, không ship/import.

Tốc độ2.4 là tuning của Arcane dựa trên pacing quan sát, **không khẳng định exact số game gốc** hoặc suy hitbox/projectile ownership từ một frame. Không sao chép art/âm thanh của Bizarre Brigade.

Built-in painting tạo `NightMarket_v1.png`1254×1254 và `BondIcons_v1.png`2172×724. Source/prompt/mapping: `K7B_Art_Manifest.json`, `K7B_Bond_Art_Manifest.json`. Đây không phải PNG native4K hoặc dozens of bespoke rig poses; Unity render/capture mới ở3840×2160.

## Gate / tái tạo

- `Builds/FullRun/ArcaneBrigade.exe`, giữ nguyên folder và ThirdPartyNotices. Fingerprints candidate: `PreviewExports/K7B/Artifact_SHA256.txt`.
- Gallery và clips: `PreviewExports/K7B/index.html`. Shop38s1920×1080 framebuffer, seeded demo, timeScale1; không desktop/microphone, CFR không thay perf measurement. Combat15s riêng. Audio giữ licensed K7, audition ở`PreviewExports/K7/Audition/index.html`.
- Unity6000.4.8f1: `ArcaneBrigade.Editor.K7BBuilder.Build` import art/apply/check/regression/bot/build. Final recheck giữ content: `ArcaneBrigade.Editor.K7BBuilder.FinalizeCandidate`.
- Native: `-arcane-k7-capture -arcane-k7b -capturePath <absolute> -force-d3d11 -screen-fullscreen 0`. Save root GUID temp riêng. Tools: `Tools/Record-K7Window.ps1 -Phase K7B -Mode shop` / `combat`.
- K7B18focused checks + K7/K5/K6/regression PASS. Native77assertions /25PNG (22native4K +3desktop-aspect) và actual item/deny/HP/egg/reroll/downward wave transition. Windows0errors/0warnings.
- Perf12s/case sau3s warmup, không encoder/PNG trong đo:~60FPS, p95~16.7ms, CPU main p95 dưới4ms trong các fixture1/56/100allies trên RTX5070Ti / Ryzen9700X. Release GC counter NA, không zero-GC; số cụ thể/raw metrics trong`Player/`.
- Movement bot15runs:14VICTORY20hall,1TIMEOUT tại hall8, normal combat của wins~12.6–15phút. Không gọi cân bằng xong bằng bot thắng; long boss/Endless và human decisions còn cần tuning thực tế. CSV là nhịp mới, không dùng báo cáo84s cũ để chứng nhận.

Android/iOS device gate, physical controller/Steam và human art/audio/feel vẫn chưa được chứng nhận. K7 gallery/manifest trước là snapshot lịch sử, không fingerprint cho candidate K7B hiện tại.
