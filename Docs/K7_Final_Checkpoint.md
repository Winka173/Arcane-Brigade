# K7 — Windows candidate / quality rework

2026-10-14. **Bản Windows rework đã bàn giao theo functional/capture gate. Không đánh dấu toàn K7 production DONE.** Chủ dự án yêu cầu tự quyết định/sửa không đợi confirm; đã triển khai và kiểm tra, không dùng test để tự ghi human approval. Android/iOS device gate vẫn OPEN theo quyết định trước.

## Những điểm đã sửa

- HUD chỉ portrait/HP/currency/wave/timer/small controls; bỏ footer/bonds/ally-count/difficulty/income banners và biên lai nổi. Details/results giữ dữ liệu thật. Main menu art-led, logo Coiny OFL, ít bảng/chữ hơn; smooth gradient, cache styles và normalize tiếng Việt.
- Quầy3option vẫn ở map: original wood/altar furniture, price strips nhỏ, spring focus/nảy; sold-empty không ĐÃ MUA. Chỉ committed transaction mới mở actual rolled companion drop/landing/walkout và merge gather/result/stars/painted payoff. Rollback không diễn. Animation clock giới hạn visual delta khi hitch, không sửa combat timer.
- 48 original character source states +16 painted dust/slash/magic/death frames +4shop props. BodyPuppet tách upper/lower sprite regions, thở/tilt/blink/feet riêng; tay-vũ khí vẫn cùng grip, không cánh tay. Enemy dark/jagged/red eyes khác ally. Mapping explicit theo unit ID, không fallback tất cả sang pillow.
- Melee sway/turn/windup/recovery, boomerang spin/return có target dedupe; magic2ray/pierce1, bow pierce2; base coin magnet1.7 và modifier vẫn cộng. Bỏ hit/buff/proc/merge target-ring outlines; hazard telegraphs còn để né, F3leash opt-in debug.
- Default33Foley Kenney + composed jazz Julie Damsgaard/Spring Spring (CC0) thay beep/music study cũ;3bus/danger duck/priority/density. Full credits/source/license đi kèm; procedural study chỉ fallback. Không dùng âm thanh reference.
- Keyboard/pad logical navigation/deadzone, pause ownership/background audio và profile1/2→3 migration. SFX mute áp dụng cả voice đang phát; gameplay RNG/economy/health/damage không bị accessibility sửa.

## Chạy và tái tạo

- Playable: `Builds/FullRun/ArcaneBrigade.exe`; giữ nguyên cả folder và ThirdPartyNotices. Source/build fingerprints trong `PreviewExports/K7/Artifact_SHA256.txt`.
- Gallery /2clip: `PreviewExports/K7/index.html`; nghe riêng `PreviewExports/K7/Audition/index.html`; checklist `K7_Playtest_Checklist.md`.
- Unity6000.4.8f1: menu **Arcane Brigade > K7 > Quality rework apply check and build**, batch method `ArcaneBrigade.Editor.K7QualityBuilder.GateAndBuild`. Không chạy baseline hoặc K5Apply cũ ghi đè animation art.
- Final fast recheck/build giữ content: `ArcaneBrigade.Editor.K7Builder.RecheckCandidate`; full regression trước đó đã PASS. Native: `-arcane-k7-capture -capturePath <absolute path> -force-d3d11 -screen-fullscreen 0`. Save root luôn GUID temp riêng.
- Video: `Tools/Record-K7Window.ps1 -Mode combat` / `-Mode shop`, optional `-FFmpegPath`. Direct Unity framebuffer/AsyncGPUReadback → local RGBA pipe; không GDI/desktop/mic. Clip15s900frames và12s720frames,1920×1080 CFR60; readbacks/timing trong `.mp4.txt`. CFR không chứng minh render60FPS, không accelerate gameplay; video silent, audition riêng.

## Kiểm tra đã chạy

`Editor_Gates.txt`:73K7 focused +47quality +72K6 +139K5, K0–K4/full-run regression PASS. Final Windows build0errors/0warnings. Native54assertions /18PNG,15frames3840×2160 +3desktop-aspect frames. Actual menu submit bỏ nút Resume disabled, real shop commit/merge/recruit, screenshots reject blank black. Trước bàn giao đã bắt/sửa alias model, price-panel che actor, hidden/DPI capture và constructor MaterialPropertyBlock trong BodyPuppet/ExpandedVisual.

## Expected / observed performance

Target desktop fixture: actual3840×2160, timeScale1, no ally cap, framep95≤20ms, encoder/screenshots ngoài measurement. Observed trên RTX5070Ti / Ryzen7 9700X /64GB Windows11, Direct3D11,12s mỗi case sau3s warmup:

| Allies seeded | Release FPS | Frame p95 | CPU main p95 | Unity allocated memory peak |
|---|---:|---:|---:|---:|
|1|~60|~16.68ms|~0.7ms|~147MiB|
|56|~60|~16.68ms|~2ms|~132MiB|
|100|~60|~16.68ms|~3.2ms|~147MiB|

Raw CSV/metadata in `Player/`. GPU counter reported roughly15–18ms, includes presentation/driver effects; không gọi là shader-only GPU cost. Memory là Unity allocated memory, không phải toàn process RAM/VRAM. Release GC counter unavailable ghiNA, không zero-cost. Development probe riêng counterVALID: mean khoảng9/38/67KiB per frame, peak24/75/83KiB cho1/56/100allies, memory≤151MiB. Đây chưa phải zero-GC, long-session stability hoặc mobile budget approval. Development sample khác release instrumentation; menu gradient cuối không thuộc combat measurement.

## Balance diagnostics / giới hạn

15movement-only bot runs:3champions×5difficulty, real84s authored waves, actual purchase/reroll, không HP cheat/enemy skip. Rework tăng projectile/pickup power; cả15clear20hall và3Endless hall, combat khoảng21.5–22.4phút. **Không ghi balanced bởi bot thắng**, chưa bao phủ full challenge/long-Endless/performance/human decision space. See `Movement_Bot_Balance.csv`.

PNG/clip squad/gold/price seeded là visual stress fixtures, không tự nhiên mua100ally hoặc chỉ mất1vàng ngoài game thật. Atlas nguồn không native4K;4character states/rig parts không phải dozens of hand-drawn directional/attack/death poses.7companions chia6base forms, bosses reuse enemy forms; không claim mọi model bespoke.

## Gate mở

- Art/audio/feel/balance: không tự coi approval đã có; reference audit/source/prompt/license ở `K7_Reference_Rework_Audit.md`, `K7_Art_Manifest.json`, `K7_Audio_Provenance.md`.
- Physical controller/full focus traversal và long-session GC/performance vẫn cần nghiệm thu trên hardware thực; logical/native fixture không thay mọi thao tác người thật.
- Android/iOS: editor này chỉ Windows/WebGL modules; adb không có device online. Chưa Mac/Xcode/signing/iPhone; touch/notch/thermal/background-device chưa test.
- Steam: chưa AppID/account/depot/Steam Input/Deck QA; không dùng AppID giả hoặc gọi executable Windows là Steam release. Không cấp firewall/network permission để lấy clip; GDI overlay recordings đã loại, bàn giao chỉ framebuffer game.
