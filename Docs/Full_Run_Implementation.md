# Full Run — functional candidate P3–P6

**Follow-up hiện tại:** đã thay bảng shop bên dưới bằng 3 quầy mua trong sân + shop EGGS riêng, bổ sung cung/trượng (8 vũ khí), motion/VFX/UI/map và nhạc study. Xem `Docs/World_Shop_Implementation.md` và `Docs/Presentation_Reference_Pass.md`. Các số liệu/checks và mô tả shop 6 ô trong báo cáo baseline bên dưới là lịch sử, không phải giao diện hiện tại.

Ngày 2026-10-13; Unity 6000.4.8f1, built-in renderer, catalog `run-0.1`, simulation 60 Hz. Đây là bản implement chơi được, không phải chứng nhận hoàn tất chất lượng P5/P6 hoặc release Android/iOS/Steam.

## Chơi / chỉnh

- Windows: `Builds/FullRun/ArcaneBrigade.exe`; không tách exe khỏi Data/MonoBleedingEdge và các file DLL đi kèm.
- Unity: `Assets/ArcaneBrigade/Scenes/RunBoot.unity` -> RunFrontend -> RunArena. Mở RunArena trực tiếp để thử Champion đầu tiên.
- Model preview tĩnh đủ sáu bộ body/weapon: `Assets/ArcaneBrigade/Scenes/RunModelGallery.unity`. Không có simulation trong gallery; motion được chạy trong RunArena. Menu `Capture model gallery without resetting` dựng gallery/capture, không ghi lại tuning.
- Input: WASD/arrows/touch kéo dưới trái; ESC pause, F3 vòng leash. Trong combat chỉ di chuyển, tất cả đánh/passive tự động. Shop mới nhận mua/nâng cấp.
- Catalog chung: `Assets/ArcaneBrigade/Resources/RunCatalog.asset`; stat/unit/weapon trong `Assets/ArcaneBrigade/Data/Run/`.
- Menu `Arcane Brigade > Full Run > Build content, scenes and checks` **ghi lại baseline data/scene**, không dùng sau tuning thủ công nếu không muốn reset. Menu Build Windows dùng dữ liệu đang có.
- Menu `Check existing content without resetting` chỉ chạy diagnostic/regression, không dựng lại data/scene; một số checks cố ý khóa scope baseline và cần cập nhật khi mở rộng content.

## Những phần đã nối

- 3 Champion: LanternKnight (rally theo quân số), Postmaster (bắn thư + nhặt tiền tự động), DrowsySeer (bell/arcane + tự hồi). Hai Champion sau mở ở hall 5/10.
- 7 companion identities: TeapotPilgrim, CandleSnail, MailboxImp, PillowOracle, MushroomLibrarian, HoodWisp, JellySwordsman; immortal, không slot cap/upkeep. Không trùng unit + weapon, vẫn cho cùng unit với weapon khác.
- 6 modular prop weapons: KeyCleaver/slam, TeapotPopgun/recoil, UmbrellaLance/thrust, MailboxLauncher/recoil, BellFork/bell, BookPaddle/slash. Body, hand và weapon độc lập; hand cùng GripRoot, không có arm/forearm.
- 4 multi-tag bonds đếm **đồng minh hiện hữu**, không đếm sao; 20 item có stack và mô tả cap. Damage/attack-speed chung và riêng Champion/companion tạo đường build khác; armor, crit, projectile, roam, pickup, heal, interest và discount dùng cùng data pipeline.
- Shop 6 offers (3 recruit + 3 item), random weapon sau khi mua, shared gold, reroll, interest, future-roll discount. Không bán/lock. Sold/token guard; nếu save run thất bại giao dịch rollback snapshot.
- Ghép 3 cùng loại/cùng tier qua các weapon -> tier cao hơn, tối đa tier 3. Weapon kết quả roll từ các variant không trùng với roster **sau tiêu thụ**, có thể dùng lại weapon của nguyên liệu. Bond/stat được tính lại sau merge.
- Baseline thử nghiệm tier damage ×3.6/cấp, support potency ×3/cấp (heal/scavenge/rally; chill tăng radius, không kéo dài slow boss). Tránh ghép ba support thành một unit mất phần lớn hồi máu/thu nhập. Bond vẫn đếm một body sau ghép; sức mạnh từng weapon khác nhau nên ghép không đảm bảo mọi đội hình đều tăng DPS chính xác ×1.2.
- 2 altar recipes tiêu thụ hai unit và 3 eggs, tạo tier 2. Chọn nguyên liệu tier thấp nhất, UI hiển thị tier sẽ tiêu thụ; không tự lấy unit không có.
- Physical/Ember/Frost/Arcane: burn, slow, một chain hit không recursive; Ember/Frost phản ứng tiêu thụ status và thêm một hit. Chưa phải toàn bộ matrix trong design; tick burn hiện là số tune riêng, không sao chép công thức reference.
- Ally mặc định không chặn đạn; Guardian Canopy bật interceptor. Wandering Map/Long Atlas mở rộng leash thật. Spatial grid dùng cho target/projectile/separation; swept projectile còn kiểm tra đoạn từ body đến muzzle để không bắn xuyên địch quá gần mà không gây damage.
- 20 hall, boss mỗi 4 hall. Hall thường 84 giây; hall boss kết thúc khi boss chết, không tự hết timer. 5 boss variants: radial burst, fan, cross trains, fast narrow volley và two-speed rings; telegraph 0.85 giây có danger lanes khớp hướng bắn.
- Wave modifiers tự động: Clockstorm mỗi 7 hall, Lost Treasury mỗi 5, Quiet Stacks mỗi 11 (ưu tiên theo thứ tự catalog nếu trùng). Không thêm nút skill.
- Victory cho chọn Endless, giữ build/currency/wave; score segment có RunId riêng để profile không cộng lại kills của run đã thắng. Unlock/codex/achievements/best hall/volume/cosmetic marker được lưu, không permanent combat stats. Recap ghi nguồn và damage **đòn cuối**, chưa có toàn bộ chuỗi nguyên nhân chết.

## Art / animation / provenance

Nguồn raster: `Assets/ArcaneBrigade/Art/Run/Characters_v1.png` và `Weapons_v1.png`, mỗi atlas 1536×1024, 6 sprites với alpha thật. Prompt nằm tại `Docs/P6_Character_Prompt.txt`, `P6_Character_Edit_Prompt.txt`, `P6_Weapon_Prompt.txt`. Unity importer trim theo alpha, PPU 256, pivot body ở chân và pivot weapon riêng theo handle (tay cầm dưới của hai popgun/launcher); muzzle offset theo hình nòng, cùng giá trị cho marker và projectile launch. Prefab shared `Assets/ArcaneBrigade/Prefabs/Checkpoint/ModularUnit.prefab`.

Sáu silhouette mới là lantern/teapot/candle-snail/mailbox/pillow/mushroom-book. Enemy definitions còn reuse một số body với tint; boss là body lớn và pattern khác, **chưa có năm boss artworks riêng**. Animation hiện là procedural bob/tilt/squash, windup/impact/recovery, slash/thrust/slam/recoil/bell; có body attack lean riêng theo weapon và số damage lấy sau armor từ event simulation, tối đa 64 labels (ưu tiên damage người chơi). Giới hạn labels/VFX không giảm damage/unit. Không phải frame-by-frame animation vẽ tay hoàn thiện.

Đã xem gameplay/shop screenshots, guide và sample footage của Bizarre Brigade, gồm chuỗi 12:00–12:00.6 cách nhau 0.1 giây; audit/URL ở `Docs/P3_P6_Worklog.md`. Body tilt và detached-hand weapon movement được quan sát thật, nhưng chưa phục dựng chính xác toàn bộ curves/timing. Artwork độc lập, không import asset game kia; **không tuyên bố đã copy đúng từng model/animation**. Merge inheritance, max tier, interest formula và altar vẫn `NEEDS_REFERENCE`; chính sách tác giả nêu trên là thử nghiệm và có nhãn trong shop.

## Save / lifecycle

- Thư mục `Application.persistentDataPath`; `run.json`/`profile.json`, SHA-256 checksum, ghi temporary + flush rồi replace, backup `.bak`. Checksum phát hiện hỏng file, không phải chống cheat.
- Save đầu hall, sau settlement và sau mua/reroll/altar. Resume shop giữ gold, SOLD offers và RNG. Resume combat **bắt đầu lại hall từ boundary**, không trả lại gold/damage/time đã phát sinh giữa trận vì chúng chưa commit.
- Corrupt primary có thể khôi phục **boundary cũ trong backup**, không đảm bảo giao dịch cuối. Catalog không tương thích bị từ chối và file giữ nguyên; chưa có migration giữa version.
- Save run và profile là hai file riêng. Nếu run đã commit nhưng profile write lỗi, không rollback run đã commit; UI báo lỗi profile. Pause/focus không chạy simulation. Diagnostic player dùng thư mục temp riêng, không đụng save thật.
- Currency/kill/egg/profile counters bão hòa ở Int32.MaxValue thay vì wrap; hall guard ở Int32.MaxValue, upgrade stack guard 100000, crit 100%, discount 50%, interceptor boolean. Đây là giới hạn kỹ thuật/stat công khai, không phải giới hạn slot đồng minh. Roster thực tế vẫn bị số identity × weapon trong content hiện tại ràng buộc.

## Validation

Kết quả cuối được ghi trong `PreviewExports/FullRun_Checks.txt`, `FullRun_Build.txt`, `FullRun_Simulation_Benchmark.txt`; log Editor/player nằm trong `.utmp/FullRun_*.log`. Checks là diagnostic harness, không phải Unity Test Framework và không thay thế chơi thật.

Editor cuối: **87 Full Run checks PASS + 17 Checkpoint + 12 Squad regression PASS** (116 assertions). Seed chính 131026; combat fixture 42, schedule 71, load benchmark 92. Benchmark desktop Editor: 25/50/100/200 allies với 100 enemies duy trì lần lượt **0.338 / 0.777 / 2.087 / 6.242 ms/step** trong lượt đo cuối; không gọi là FPS render hoặc mục tiêu mobile. Con số tham khảo phụ thuộc máy/tải nền.

Windows cuối: **Succeeded, 0 build errors, 0 build warnings**, output 110064588 bytes theo Unity BuildReport. Native headless smoke `ARCANE_PLAYER_SMOKE_PASSED`, exit 0; rendered D3D11 menu/combat/shop/boss capture `ARCANE_UI_CAPTURE_PASSED`, exit 0, ảnh đã mở để kiểm tra. Shop có hai purchase qua API và boss fixture giữ save temp riêng. Unity licensing/network/D3D diagnostic messages có trong Editor log; không gọi toàn bộ log sạch chỉ vì BuildReport báo 0 warnings.

Các nhóm: catalog/assets, settlement một lần, thiếu vàng/SOLD guard, RNG restore, stack, merge/bond, duplicate-save rejection, atomic save/backup corruption, profile dedup, altar, element reactions, interceptor/armor, riêng Champion damage, events, boss telegraph, muzzle segment, accelerated schedule 1–20 -> Endless 21, uncapped simulation fixtures và regression Checkpoint/Squad.

Capture ban đầu dùng batch mode/hidden D3D12 không có backbuffer hữu ích (failed/black screenshots); không dùng làm evidence. Chuyển sang rendered standalone window D3D11, `WaitForEndOfFrame` + `CaptureScreenshotAsTexture`, kiểm tra pixel variation trước ghi/PASS và xem ảnh bằng mắt. Các ảnh UI cuối là từ đường capture này; thông báo PASS cũ không đủ nếu ảnh blank.

Schedule dùng timer 0.1 giây và fixture kill boss: chứng minh state/reward flow, **không chứng minh thắng tự nhiên hoặc thời lượng 30–40 phút**. Benchmark 25/50/100/200 allies + 100 enemies (50 contact/50 ranged), 600 ticks; tăng HP Champion **và enemies** để duy trì combat, bypass shop identity để đo. Chưa đo GPU/GC/điện thoại, không suy ra FPS thực tế từ milliseconds/step.

Screenshots 1920×1080: `Unity_FullRun_Model_Gallery.png` (Camera.Render), `Unity_FullRun_World.png` (world-only), `Unity_FullRun_Menu_UI.png`, `Unity_FullRun_Combat_UI.png`, `Unity_FullRun_Shop_UI.png`, `Unity_FullRun_Boss_UI.png` (rendered native Unity player). Shop được dựng fixture gold, có mua thật qua transaction API; boss fixture chuyển hall và thêm roster. Không phải ảnh mockup, cũng không phải một run đã chơi thắng.

## Chưa nghiệm thu

1. Chủ dự án duyệt art/motion/feel/build depth/UI; reference motion và quy tắc chi tiết cần evidence bổ sung. Không dùng compile/build PASS để gọi P5 chất lượng cao đã hoàn tất.
2. Chơi tự nhiên nhiều run, cân bằng 3 đường build, xác nhận pacing 30–40 phút, loss spikes, readability trong đội đông; benchmark sustained load/run dài và memory/GC.
3. Production enemy/boss art, merge/spawn/passive cues, animation vẽ tay, SFX/music/mix; UI hiện là IMGUI functional, chưa gamepad navigation/mobile safe-area production, localization/accessibility.
4. Catalog migration, suspend/device validation, Android/iOS builds, Steam SDK/AppID/platform features chưa làm. Máy hiện thiếu Android/iOS build modules, iOS còn cần macOS/Xcode. Windows executable không đồng nghĩa Steam release-ready.
