# K0–K2 — bàn giao runtime

Ngày 2026-10-13. Unity 6000.4.8f1, Windows x64/D3D11. **Functional/presentation candidate; chưa nghiệm thu production hoặc reference parity hoàn chỉnh.** K2 được bổ sung theo yêu cầu trong lúc đang triển khai K0/K1.

## K0

- Baseline source ZIP, SHA-256 manifest, catalog IDs và regression 96 PASS: `PreviewExports/K0`. Không rewrite/reset catalog hoặc commit.
- Contract/decision register: `K0_Data_Contract.md`. Những công thức chưa có evidence vẫn mở, không tự đổi altar thành luật đoán.
- SchemaRevision 1 nhận legacy 0/missing, normalize SpecialOffers, reject future schema. Không đổi ID hoặc catalog Version hiện có.
- Diagnostic saves dùng root riêng chung frontend/arena. Primary checksum/backup fixtures; failed transaction rollback vàng/stock/RNG. NextWave/Endless/restart không chuyển tiếp nếu lưu thất bại.
- New-run menu xác nhận trước, backup primary + backup vào ArchivedRuns; backup lỗi không launch. Save lỗi hiện UI; không navigate khỏi run nếu SaveRun thất bại.

## K1

- World 34×22; combat view cao 10.8, aspect 16:9 cố định; smooth follow 0.16s và clamp. Rest view cao 12, giữ quầy x±7 trong frame. Bốn góc có movement/clamp fixtures.
- Crop phần sàn nội thất từ art hiện có thành Sprite asset; boundary là layer riêng, không kéo giãn viền cũ theo floor. Boundary block là nền K1, chưa phải bộ prop/environment final K5.
- Menu landing/roster có locked states/resume summary/settings/help/records/overwrite confirmation. Volume và cosmetic lưu có rollback nếu lỗi.
- Details read-only Stats/Companions/Relics; nguồn damage/CD/passive, weapon đã roll, tier, stack và bond theo body count. Hai scroll panes; Tab đổi mục, Up/Down hoặc click/tap chọn dòng. I/ESC/ĐÓNG thoát.
- Pause độc lập manual/Details/navigation/focus/background; đóng overlay không nhả pause khác. Timer/HP/snapshot/RNG không đổi trong capture Details. Movement và SPACE không chạy khi paused.
- Safe-area transform cho UI; Noto body/Black heading/Mono numbers, glyph tiếng Việt đã kiểm, OFL đi kèm Windows build. Provenance: `K1_Font_Provenance.md`.
- Không fake synergy graph, receipt/delta hoặc marker chest/egg. Contract trước, data thật ở K3.

## K2

- Logical death lập tức tắt attack/hurt eligibility, emit một subject-ID event. Body/grip chuyển thành remnant pool độc lập: impact trắng, kick, collapse, nghiêng/rơi hand+weapon cùng grip, paper chips, shadow/alpha fade. Enemy 0.65s, player 0.95s, boss 1.1s là AUTHORED tuning.
- Giữ VFX qua wave boundary. Boss hold 1.15s, hall thường 0.45s; player results chờ 1s. Terminal presentation clock hoàn tất attack/hit pose mà không tăng combat time hoặc RNG. Projectile/halo/streak fade 0.25s, không thành ghost bất động.
- Enemy còn sống khi hết giờ và coin chưa nhặt chỉ fade/despawn; không phát kill/pickup giả hoặc credit tiền ngoài radius. Heal có event/ring xanh riêng, không giả glint coin.
- Một coin logical có stable ID/value: scatter 0.26s với velocity derived từ ID, không consume combat RNG; clamp bound → attraction lock khi vào pickup radius → collect gần champion → credit một lần. Không tự lấy coin nằm ngoài pickup radius.
- Visual coin: arc/bounce landing, spin giữ scale dương, shadow theo ground, glint, pickup star/spark và wallet pulse. UI/animation không tự cộng tiền; pause không hút/credit.
- Body/projectile/coin position và attack age interpolated; sửa discontinuity sword/thrust/slam/cast tại impact, hand tiếp tục nằm trong grip. Flash/squash chỉ presentation, không invulnerability/global hitstop.
- Death puff đặt dưới silhouette để không che body; VFX/remnant pool cap chỉ presentation, không cap đội quân. Player/boss death ưu tiên khi remnant pool đầy.
- Player/boss remnant ở lớp ưu tiên trên đồng minh trong lúc collapse, để đội đông không che mất điểm nhấn cảnh chết.
- Damage numbers dùng font số, outline; hai death cue synth nguyên bản bổ sung để kiểm timing. Đây là cue study, **không gọi là soundtrack/SFX production đã duyệt**.
- Chỉ mẫu death paper/collapse nền; material families/merge personality/boss scene motifs còn K4/K5. Coin art hiện có được tái sử dụng, chưa phải một đợt redraw toàn bộ content.

## Kiểm tra / artifacts

- `K2Checks.Run` chạy regression **96 PASS**, K0/K1 **27 PASS**, K2 **22 PASS**. Check source: `Assets/ArcaneBrigade/Editor/K1Builder.cs`, `K2Checks.cs`. Reports ở `PreviewExports/FullRun_Checks.txt`, `PreviewExports/K1/K0_K1_Checks.txt`, `PreviewExports/K2/K2_Checks.txt`.
- Windows build: `Builds/FullRun/ArcaneBrigade.exe`; giữ toàn bộ directory. `PreviewExports/FullRun_Build.txt`: Succeeded, 0 errors, 0 warnings. Không dùng baseline content builder để kiểm/bổ sung.
- Capture K1: `PreviewExports/K1/Player`: menu pages, combat, Details, camera corners, shop, populated large roster first/last và relic stack fixtures; `Capture_Notes.txt` nêu setup/kết quả.
- Capture K2: `PreviewExports/K2/Player`: Enemy_Death_Coin, Boss_Death_Transition, Champion_Death_Results và Results_After_Death. PNG frames có timestamp CSV, không gán FPS giả. `Capture_Report.txt` ghi native-player runtime outcome và sample frame-time riêng không chụp.
- Mở `PreviewExports/K2/Player/index.html` để phát/seek các sequence theo timestamp thực (không cần server). Cảnh death/coin đầu kéo dài ~0.7s, boss/player ~1.2s. Sample early-wave không chụp riêng 3s ghi median/p95 trong capture report; không certify stress/mobile. Fixture đi tới coin qua movement bình thường, không assume mọi drop tự nằm trong pickup radius.
- Trong Unity dùng menu **Arcane Brigade → K0-K2 → Check and build existing content**, hoặc executeMethod `ArcaneBrigade.Editor.K2Checks.BuildAndCheck`; không reset art/tuning.
- Capture flags chỉ diagnostic, không ghi save thật. Inject enemy/HP/roster và camera corners là fixtures, **không bằng chứng thắng full run tự nhiên**. Native GPU readback/PNG encoding ảnh hưởng capture; không dùng các frames này certify FPS.

## Còn mở

- Art/feel cần chủ dự án duyệt bằng chơi thật; pass compile/fixtures không đủ xác nhận chất lượng “cực cao”.
- Android/iOS module/device/certificate và Steam release/controller chưa kiểm. Safe-area/focus ownership desktop không thay mobile lifecycle QA.
- Chưa có stress/full-run balance/Endless performance hoặc audio audition; frame-time early-wave chỉ baseline desktop.
- K3: modifiers/proc/cause registry, shop weighted economy, altar HP/chance, receipt/delta và chest/egg indicators bằng data thật. Không tự thêm skill/manual combat, upkeep/sell/lock/permanent stat grind.
