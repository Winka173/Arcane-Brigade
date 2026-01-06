# K6 — Final checkpoint

**2026-10-14: DONE theo functional gate runtime Windows.** K6.1–K6.5 đã triển khai; không tuyên bố production/mobile, exact-reference parity hoặc full-run balance đã nghiệm thu. K7 là phase tiếp theo.

## Deliverables

- **K6.1 DONE:** năm difficulty và ba challenge fixed-seed local; live score, records riêng Champion/difficulty/mode/challenge; unlock không permanent stat. Schema3/profile2 migration, profile disk-error rollback/retry, terminal-credit recovery từ menu và lifetime RunId dedupe. Resume cả Victory/Defeat mở hóa đơn cũ, không tạo run mới; Victory còn tiếp tục Endless sau khi đóng game. Endless có ParentRunId/StartHall/ledger riêng, giữ build.
- **K6.2 DONE:** invoice scroll/grid thể hiện roster/weapon/tier/relic thật, ledger damage/heal/coin/kill/activation hiệu lực và death source/kind/damage. Nickname deterministic theo contribution, frozen cùng score/build trong50 history. Export frame PNG local tùy chọn; không upload, không claim export cả scroll.
- **K6.3 DONE:** museum weapon/tier/relic/companion/chain/boss, locked conditions, sandbox actual gameplay pipeline. Journal giữ variant thực cả khi merge/craft tiêu body; rollback không mở discovery. Chain cần ledger evidence, boss cần kill identity, preview không ghi profile/run.
- **K6.4 DONE:** shared UI cho HUD/menu/Details/quầy vật lý/receipt/invoice/museum/history; font Việt Be Vietnam Pro Black/Medium + Noto Sans Mono, license đi kèm. Menu backdrop original bằng painting pass; ba cosmetic frame/paper/stamp bundles, không đổi combat/danger semantics. 4K là render/capture quality, không mỗi PNG nguồn native4K.
- **K6.5 DONE:**72 focused checks + K0–K5 regression PASS; build Windows0errors/0warnings;34 native assertions +24 UnityPNG3840×2160. Hash/source/art/license/checkpoint có đầy đủ trong repo, binary local không commit.

## Evidence và chạy thử

- Build local: `Builds/FullRun/ArcaneBrigade.exe`; report size173271671 bytes (toàn build report, không phải riêng exe launcher).
- Gallery: `PreviewExports/K6/index.html`; raw frames `PreviewExports/K6/Player/`.
- Focused gate `PreviewExports/K6/Checks.txt`; sanitized summary `PreviewExports/K6/Editor_Checks.txt`; player assertions/fixture limits `PreviewExports/K6/Player/CaptureNotes.txt`; fingerprints `PreviewExports/K6/Artifact_SHA256.txt`.
- Reproduce focused gate: Unity batch `-executeMethod ArcaneBrigade.Editor.K6Builder.Check`; full gate/build `ArcaneBrigade.Editor.K6Builder.GateAndBuild`. Không gọi baseline BuildAndCheck reset catalog.
- Native fixture: player `-arcane-k6-capture -capturePath <absolute folder> -force-d3d11 -screen-width 1920 -screen-height 1080 -screen-fullscreen 0`; Root riêng qua RunPaths, không persistent profile thật.
- Ghi chú kỹ thuật/tuning/migration: `K6_Meta_Rules.md`; font/art provenance và full prompt: `K6_Art_Manifest.json`, `K1_Font_Provenance.md` và font OFL trong Resources/Fonts.

## Phạm vi nghiệm thu

Ảnh HUD/Details lấy combat native với squad/relic seeded; contribution lấy simulation thật. Victory/death/wave/boss discovery của ảnh là terminal fixtures được gán để kiểm tra layout. Native ContinueEndless/storage/history/museum/sandbox dùng runtime thật, nhưng **không lấy capture làm bằng chứng chơi tự nhiên thắng20hall**. Screenshot là Unity live1920×1080 supersample2× ra3840×2160, không chứng minh4K60FPS hay motion/audio chất lượng production.

K7 còn balance mọi difficulty/challenge/full-run/Endless, audio audition và music/SFX mix, frame-time/GC/GPU/memory, combining glyph/kerning và mọi landscape aspect, controller toàn menu, Android/iOS device+lifecycle/safe area và Steam prerequisites. Những gate này không bị chuyển thành PASS bởi check simulation hoặc ảnh desktop.
