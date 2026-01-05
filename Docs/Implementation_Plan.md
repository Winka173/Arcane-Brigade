# Arcane Brigade — Kế hoạch chuẩn bị implement

**Ngày:** 2026-10-13  
**Trạng thái:** Đã implement functional candidate P3–P6 (Full Run); chưa nghiệm thu P5/P6 về chất lượng, pacing/reference và thiết bị thật. Xem `Docs/Full_Run_Implementation.md`.  
**Cập nhật K0–K2:** baseline/contract, camera/map, main menu/Details và death/coin/pose đã có runtime Windows candidate; xem `K0_K2_Implementation.md`. Các đoạn “chưa implement” bên dưới mô tả lịch sử brainstorm, không phủ nhận phần đã bàn giao; cũng không đánh dấu toàn S/N hoàn tất.
**Nguồn luật:** quyết định của chủ dự án và `Production_Plan_2D.md`.  
**Art được chọn:** Minimal Arcade, thêm chi tiết nhận diện có chọn lọc; không có cánh tay, bàn tay gắn và chuyển động cùng vũ khí.

**Đối chiếu mới:** chủ dự án chọn Bizarre Brigade **1.5+**, yêu cầu research toàn hệ thống trước khi bổ sung. `Bizarre_Brigade_15_Reference_Audit.md` ghi 57 hạng mục và evidence; `Reference_Parity_Backlog.md` là backlog R1–R8 cho lượt triển khai tiếp theo. Chưa implement backlog này; P3–P6 không có nghĩa reference parity đã hoàn tất.

**Khác biệt nguyên bản đã duyệt:** ý tưởng 2–6 được mở rộng thành S1–S5 trong `Signature_Gameplay_Expansion.md`; tích hợp vào R2–R7. Không thêm ý tưởng formation/chỉ huy bằng movement. Đây là thiết kế mới, chưa phải tính năng runtime hoặc tuning đã nghiệm thu.

**Brainstorm bổ sung duyệt hết:** `Nightmare_Identity_UI_Gameplay.md` chốt S6–S10 và N01–N15 cho UI/font/gameplay/personality/meta; đã nối vào R1–R7. Toàn bộ vẫn là APPROVED DESIGN, không khẳng định đã có asset/runtime/build mới.

**Kế hoạch thực thi hiện hành:** `Implementation_Roadmap.md` chia K0–K7 với dependency/deliverable/quality gate; thêm N16–N18 đã duyệt (sandbox, delta mua/ghép, chỉ báo chest/egg). Bắt đầu K0/K1, ưu tiên death/coin/độ mượt ở K2 trước nhân rộng content. Các checkpoint P cũ dưới đây là lịch sử baseline, không thay thứ tự mới.

## 1. Hiện trạng và điểm xuất phát

- Có project Unity **6000.4.8f1**, sprite import, ba prefab, chín clip pose thử, scene so sánh và ảnh render từ Unity.
- Project dùng **built-in renderer**. Đã có RunBoot/RunFrontend/RunArena, combat, shop và boundary save; chưa có URP, Input System mới hoặc tích hợp Steam.
- Sprite style preview cũ gộp người và vũ khí, chỉ là mẫu hướng style. Gameplay checkpoint mới có body/tay/vũ khí riêng, chưa phải bộ animation production hoàn chỉnh.
- Chưa đo FPS/memory trên mobile. Scene 54 đơn vị là mẫu nhìn hình, không phải benchmark gameplay.
- Bản ZIP/unitypackage trong `PreviewExports/` là snapshot style preview trước kế hoạch này, không phải bản game sắp implement.
- Giữ nguyên preview để đối chiếu; gameplay mới nằm ngoài `StylePreview/`. Không sửa hoặc xóa thay đổi đang có, không tự commit/push.

## 2. Những luật không được làm sai

1. Trong trận chỉ di chuyển; Champion có passive và auto-attack, không skill/ultimate/dash/aim/đổi vũ khí chủ động.
2. Một arena giới hạn rộng hơn viewport, camera theo người chơi và clamp ở rìa theo yêu cầu mới; mobile ngang, chơi một tay; PC dùng bàn phím và chuẩn bị tay cầm. Thay thế luật cũ fit toàn sân trong một màn hình.
3. Đồng minh bất tử, ở gần người chơi, không chặn đạn mặc định; item mới cho phép chặn đạn hoặc đi xa hơn.
4. Không giới hạn slot đồng minh, không phí nuôi. Giảm tải VFX không được giảm số đơn vị hoặc damage thực.
5. Vũ khí đồng minh ngẫu nhiên; luật mua, trùng và ghép theo tham chiếu, không tự đổi sang tích mảnh.
6. Bond tính theo số đơn vị hiện hữu, không nhân theo sao; một đơn vị có thể có nhiều tag.
7. Đồng minh và nâng cấp cộng dồn đều phải mua sau wave, cùng dùng vàng; có reroll/lãi, không bán/khóa món.
8. Có contact damage và nhiều đòn hợp lệ cùng lúc. Giới hạn sức mạnh từng đòn bằng cân bằng, không thêm bất tử toàn cục sau trúng đòn.
9. Giữ damage type/status/reaction tự động. Run thường mục tiêu 30–40 phút gồm shop; có Endless.
10. Art đơn giản và độc lạ, không anime chibi nhiều giáp. **Không có cánh tay/cẳng tay nối thân với bàn tay.**

## 3. Hợp đồng prefab/art cần làm trước

### 3.1. Hình dáng

- Thân/đầu là khối chính, chân nhỏ; mắt/mặt vài nét như hướng đã duyệt.
- Thêm một hoặc hai chi tiết lớn có ý nghĩa: hình mũ, vết khuyết, biểu cảm, biểu tượng, phụ kiện hoặc tỷ lệ kỳ quái. Không tăng chi tiết bằng hoa văn, tóc từng lọn hay giáp nhiều mảnh.
- Một roster phải khác nhau bằng silhouette và tính cách, không chỉ đổi màu cùng một thân.
- Giữ nét sạch ở cỡ lớn nhưng thiết kế cho 32–48 px. Tính độc lạ không được che người chơi, projectile hoặc telegraph.
- Mẫu mới phải có bàn tay đơn giản cầm vũ khí, không mọc tay dài khi đánh. Nhân vật không cầm vũ khí không bắt buộc có bàn tay trang trí.

### 3.2. Cấu trúc hiển thị

```text
UnitRoot                         vị trí/facing từ gameplay
  BodyVisual                     thân/đầu; bob/squash chỉ hiển thị
  FeetVisual                     tùy chọn nếu cần tách bước chân
  GripRoot                       cụm cầm vũ khí, không có arm bone
    WeaponVisual                 sprite vũ khí độc lập
    HandVisual                   bàn tay, khóa vào grip của vũ khí
    SecondaryHandVisual          chỉ khi vũ khí cần hai bàn tay
    MuzzleMarker                 marker để xem vị trí phát đạn
    TrailAnchor                  marker VFX
  ShadowVisual
  StatusVisualRoot
```

- Bàn tay và vũ khí phải dịch chuyển/xoay như một cụm: không rơi tay, không trượt grip, không cần rig khuỷu tay/vai.
- Định nghĩa vũ khí lưu grip, pivot, tư thế idle, điểm phát đạn, hướng forward, layer trước/sau thân và envelope của swing/recoil.
- Body asset tái sử dụng với nhiều vũ khí, không bake kiếm/nỏ vào mọi frame cơ thể. Bàn tay có thể dùng sprite riêng hoặc nằm trong asset cụm vũ khí, miễn không tách rời khi chuyển động.
- Gameplay không đọc damage từ marker/animation. Vị trí bắn mô phỏng và marker hiển thị cùng dùng thông số mount/facing, để thay sprite không đổi kết quả chiến đấu.
- Tách màu/sort của bóng, người, tay/vũ khí và VFX. Flip trái/phải phải giữ grip, muzzle và vũ khí đúng phía.
- Hướng nhìn khởi đầu: hình mặt trước có flip trái/phải; chưa sản xuất nhiều hướng nhìn trước khi duyệt mẫu trong trận.

### 3.3. Animation tối thiểu

| State | Mục tiêu |
|---|---|
| Idle | Nhún nhẹ thân, giữ tay/vũ khí ổn định; không rung jitter |
| Move | Lean/bob/bước chân nhỏ; tay và vũ khí theo nhân vật |
| Melee | Anticipation → impact → recovery trên cụm grip; không vẽ cánh tay nối thân |
| Ranged | Nhấc/aim tự động → fire → recoil/recovery; đạn khởi tạo từ luật combat |
| Support | Cast/heal/buff đọc được, vẫn không có skill button |
| Hurt/death | Phản hồi riêng cho người chơi/địch; đồng minh không có health/death gameplay |
| Spawn/merge | Hiệu ứng ngắn, có kết quả rõ, không khóa thao tác quá lâu |

Một attack instance có timeline dữ liệu duy nhất; simulation phát event impact/fire, hiển thị bám theo timeline. Không tính damage lại từ AnimationEvent. Attack speed thay đổi vẫn đúng nhịp.

**Nghiệm thu asset mẫu:** ba nhân vật và ba vũ khí phải ghép qua lại được; quay/flip/đánh/bắn không lộ cánh tay hoặc lệch grip; capture cả cỡ lớn, 32–48 px và đội quân đông. Đánh giá chất lượng bằng hình và motion thật trong Unity.

## 4. Nền tảng và dữ liệu

- Giữ Editor 6000.4.8f1 ở bước đầu vì preview đã import/render được. Khóa manifest/lockfile theo các package được kiểm tra; không tự nâng Editor giữa milestone.
- Tiếp tục built-in cho checkpoint đầu; đánh giá có cần URP 2D trước sản xuất art/VFX hàng loạt. Nếu đổi, phải giữ hình đã duyệt và có capture so sánh. Không ghi URP đã cài khi chưa cấu hình.
- Thống nhất keyboard/gamepad/touch qua `Move` intent; UI có action map riêng. Không tạo `Attack`, `Ultimate` hay `Aim` dành cho gameplay.
- Fixed-step simulation đề xuất khởi đầu 60 Hz, render nội suy; xác nhận đáp ứng/tải trước khi khóa. Không hứa replay bit-identical giữa platform dựa trên physics float.
- Stable entity ID, run generation, seed và luồng RNG riêng cho wave/shop/weapon/gameplay; VFX randomness không làm lệch roll shop.
- ScriptableObject catalog là nguồn tune chuẩn ban đầu, có ID/version/đơn vị; báo cáo JSON/CSV chỉ xuất đọc, không trở thành nguồn sửa song song.
- Stat hiển thị phải cùng pipeline stat dùng khi combat; stack order, cap và rounding được ghi trong dữ liệu.
- Thiết kế combat không phụ thuộc UI. Preview không được trở thành service quyết định damage, spawn, tiền hoặc roster.

Catalog cần có: `ChampionDefinition`, `CompanionDefinition`, `WeaponDefinition`, `ItemDefinition`, `BondDefinition`, `EnemyDefinition`, `WaveDefinition`, `BossPatternDefinition`, `MergeRecipeDefinition`, `RunRulesDefinition`.

Luật chưa kiểm chứng nằm trong policy/data được đánh dấu `NEEDS_REFERENCE`, không hardcode thành luật production.

## 5. Thứ tự implement và checkpoint

Các số content bên dưới là scope khởi đầu, không phải cam kết toàn bộ nội dung launch hoặc lịch theo ngày. Mỗi bước kết thúc bằng bản chơi được/capture và ghi rõ phần chưa hoàn thiện.

### P0 — Chuẩn bị đặc tả, catalog và hợp đồng asset

**Trạng thái:** một phần — catalog checkpoint và prefab modular đã có; reference ledger vẫn cần evidence trước shop/ghép.

- Lập ledger tham chiếu cho trùng/ghép, cấp vs rarity, preview vũ khí trước mua, vũ khí sau ghép, targeting và altar.
- Tạo baseline HP/damage/economy trong cùng catalog; không chép số damage cũ gây one-hit.
- Chốt đơn vị world, kích thước arena, collider so với hình và footprint grip/weapon.
- Tạo mẫu thân + bàn tay/vũ khí riêng như phần 3; lưu nguồn asset, import settings và metadata.

**Đạt khi:** có quy tắc đủ để bắt đầu phần độc lập; từng hệ phụ thuộc reference có trạng thái rõ; visual contract được thực thi trên mẫu. Nếu evidence chưa có, vẫn tiếp tục nền tảng/movement/art, nhưng chưa nghiệm thu ghép/shop là giống bản gốc.

### P1 — Bộ khung gameplay và movement

**Trạng thái:** một phần — Boot/Frontend/Arena, movement keyboard/touch, fixed-step, pause/restart đã có; gamepad, input production và kiểm thử thiết bị còn thiếu.

- Tạo runtime assembly boundaries và scene `Boot`, `Frontend`, `Arena` ngoài preview.
- Run state: `Preparing → Combat → Settlement → Shop → Combat`, thêm `Paused`, `Victory`, `Defeat`.
- Movement keyboard và joystick touch trong cùng sân; clamp theo collider, giữ vùng nhìn bằng nhau trên 16:9 và màn hình dài.
- Lắp body/grip visuals; mirror và movement animation; pause/focus/suspend không cho simulation tự chạy nền.
- Có menu start, restart và HUD nền tảng; không dựng menu production quá lớn trước core.

**Checkpoint:** người chơi di chuyển mượt trên PC/touch, chỉ cần một tay, không có thao tác combat ẩn; restart sạch ID/timer/subscription.

### P2 — Combat feel và một wave chơi được

**Trạng thái:** một phần — một wave 60 giây, melee/projectile/contact, pickup, VFX/audio thử và smoke checks đã có; telegraph, stat pipeline, spatial index và polish production chưa hoàn tất.

- Một Champion tự đánh, một địch melee và một ranged; một vũ khí melee và một projectile weapon.
- Damage pipeline, attack cooldown, contact damage theo từng nguồn, projectile sweep/hit/lifetime/pool, health/death và vàng/pickup.
- Auto-target policy có fixture để kiểm tra, không gọi policy tạm là luật Bizarre Brigade đã xác nhận.
- Impact/recoil, audio cơ bản, hit feedback, bóng và VFX gọn; spawn địch ngoài vùng an toàn của người chơi.
- Wave timer, settlement và kết quả chết/restart; không mở rộng đội quân trước khi cảm giác movement/đánh đạt.

**Checkpoint:** một wave từ start tới kết thúc; impact khớp visual, một attack không tính damage trùng, nhiều nguồn vẫn gây damage cùng tick; không có global hurt immunity và đòn thường không vô lý xóa đầy máu.

### P3 — Đội quân, vũ khí và bond

**Trạng thái:** chức năng candidate đã có — identity/random weapon, roaming/spacing/catch-up, bond/passive, item radius/interceptor, spatial query và benchmark simulation. Reference chi tiết và đo thiết bị production chưa xong.

- Roster runtime tách identity/passive/weapon/bond/tier. Các fixture thử có vũ khí chỉ định; tuyển dụng chính thức áp random policy đã xác nhận.
- Follow radius/leash và spacing nhẹ; item range thay đổi thật phạm vi hoạt động. Targeting không làm đồng minh tự bỏ người chơi đi săn khắp sân.
- Ally không có health/death hay collider chặn đạn mặc định. Chặn đạn là capability có provenance từ item; thu hồi đúng khi hiệu ứng hết.
- Support hồi người chơi, buff đội hoặc kiểm soát địch; không tạo HP giả cho đồng minh.
- Bond đếm đơn vị hiện hữu và nhiều tag, không đếm summon/bản sao ẩn/cấp sao ngoài luật đã xác nhận.
- Spatial queries/pooling từ đầu; kiểm tra nhóm đông để tránh thiết kế tìm target theo mọi cặp đơn vị.

**Checkpoint:** đội gần người chơi, giữ vũ khí/grip đúng khi quay/đánh; item radius/interceptor thể hiện rõ; không giới hạn slot để che vấn đề hiệu năng.

### P4 — Shop, kinh tế, ghép và save giao dịch

**Trạng thái:** chức năng candidate đã có — shop/income/interest/discount/reroll, merge/altar, transaction rollback khi save run lỗi, checksum/backup và RNG resume. Chính sách merge/economy là thử nghiệm, chưa xác nhận giống hệt reference.

- Ba loại lựa chọn tranh ngân sách: tuyển đồng minh, tăng sức mạnh Champion, tăng đội/build; vật phẩm stack theo dữ liệu.
- Gold, income, interest, reroll và item discount qua một transaction path; hiển thị tiền trước/sau và lãi dự kiến.
- Shop dừng combat; không sell/lock/upkeep. Double-click, thiếu vàng và stale offer không nhân giao dịch.
- Mua/ghép thường và một nhóm công thức altar theo reference. Sau ghép cập nhật roster, bond, passive, weapon và visuals atomically.
- Save tại ranh giới wave và sau giao dịch shop; catalog version, seed/RNG state, roster, gold, items và run progress đầy đủ. Có backup/recovery, không nhân vàng khi resume.

**Checkpoint:** ít nhất ba wave có shop nối nhau; quyết định mua có ý nghĩa; mua/ghép/restart/resume không làm lệch dữ liệu. UI chứng minh được stack và bond trước/sau.

### P5 — Vertical slice chất lượng

**Trạng thái:** có vertical-slice candidate chơi được và capture Unity; chưa đạt checkpoint duyệt chất lượng. Đã có 3 Champion, 7 companion, 6 weapon, 20 item, 6 enemy và 5 boss variants; art/motion/UI/audio còn cần polish và duyệt.

- Mục tiêu: 3 Champion khác passive, 6 loại đồng minh, 6–8 vũ khí, 6 địch, 1 boss, 1 arena và khoảng 20 item.
- Ít nhất ba build thật sự khác: Champion tự đánh, đội quân và economy/hybrid. Không đạt bằng thay màu cùng một bộ chỉ số.
- Boss có telegraph và pattern, địch/contact/projectile đa dạng; cân bằng từng đòn và độ dày nguồn damage đồng thời.
- Thêm một tập status/reaction đại diện; ràng buộc proc chain, resistance và source attribution trước khi thêm toàn bộ matrix.
- HUD/shop thực sự dùng được trên mobile và tay cầm, có âm thanh/nhạc và animation đạt hướng đã chọn.
- Vertical slice là run ngắn để đánh giá core, không phải bản 30–40 phút bị kéo dài bằng máu boss.

**Checkpoint:** chủ dự án chơi và duyệt cảm giác, motion, chiều sâu và UI; compile/capture không thay cho việc duyệt gameplay. Chỉ mở rộng content sau checkpoint này.

### P6 — Run đầy đủ, Endless và progression

**Trạng thái:** vòng run 20 hall, event modifiers, Endless, meta/codex/record/cosmetic và boundary save đã nối. Chưa nghiệm thu pacing 30–40 phút, replay dài, migration catalog và cảm giác/animation reference; không gọi P6 production hoàn tất.

- Tạo pacing 30–40 phút gồm shop, lịch boss/wave và variety rõ; Endless nối sau điểm kết thúc bằng lựa chọn của người chơi.
- Hoàn thiện nguyên tố/status/reaction, altar/ghép đặc biệt, item build-defining và các event phù hợp vòng chơi.
- Meta unlock, codex, achievement, best run và recap nguyên nhân chết; chưa tăng stat vĩnh viễn làm điều kiện vượt game.
- Mở rộng roster/arena theo khoảng trống build; 18 Champion trong design là roadmap nội dung, không làm 18 bản gần giống nhau.
- Save/resume qua pause/suspend, lỗi file và catalog migration; ghi rõ resume ở ranh giới wave, không hứa khôi phục mọi projectile giữa trận.

**Checkpoint:** run có build-up và cao trào, dài hơn không đồng nghĩa lặp lại; Endless không overflow stat/timer/gold/proc và không bị lệch vì resume.

### P7 — Production polish và release đa nền tảng

**Trạng thái:** chờ P6; compatibility được kiểm tra từ các checkpoint trước, không dồn hết tới cuối.

- Production asset/animation, SFX/music/mix, accessibility, localization/UI và quality tiers có thứ tự ưu tiên cue rõ.
- Windows standalone và Steam adapter; Android build/lifecycle; iOS qua macOS/Xcode hoặc build service được cung cấp.
- Chọn thiết bị mục tiêu và cấu hình tối thiểu trước khi tuyên bố đạt FPS; đo run dài, nhiệt/pin/memory/CPU/GPU/GC/overdraw.
- Kịch bản tải 25/50/100/200 đồng minh cộng địch/projectile/status: mốc đo, không cap roster. Giảm VFX trước, không bỏ damage/unit.
- Co-op/crossplay và online services là giai đoạn sau, không tự thêm backend/voice/chat vào scope solo.

**Checkpoint:** có build và báo cáo riêng cho Android/iOS/Windows; hình nhìn đẹp trên PC không chứng minh mobile đã đạt; không gọi Steam release-ready khi chưa có quyền/tài khoản/AppID.

## 6. Chuẩn nghiệm thu xuyên suốt

| Nhóm | Kiểm tra bắt buộc |
|---|---|
| Visual | Không arm/forearm, hand khóa grip; sprite/weapon đủ alpha/pivot; flip/sort đúng |
| Input | Keyboard/touch/controller dùng cùng Move intent; shop không kích hoạt movement xuyên UI |
| Combat | Contact theo nguồn, projectile hit một lần, simultaneous damage, timing độc lập render FPS |
| Ally | Bất tử, default không chặn đạn, không chạy khỏi leash; không tự thêm slot cap |
| Build | Stack đúng, stat UI khớp combat; bond/tier không đếm sai; không proc recursion vô hạn |
| Economy | Mua/reroll/ghép atomic, không âm vàng/dup; interest/discount theo cùng dữ liệu |
| Save | Ghi an toàn/backup/version; resume giữ seed và kết quả roll; migration không mất unlock |
| Lifecycle | Pause/focus/suspend không chạy simulation; restart không còn event/đạn/target cũ |
| Quality | Nguy hiểm đọc được trong đám đông; giảm flash/rung/VFX không giảm thông tin quan trọng |
| Platform | Cùng vùng nhìn, safe area, chữ/nút dùng được; đo máy thật trước công bố hiệu năng |

Khi implement, bắt đầu bằng compile/import và smoke check đúng phần vừa đổi, rồi regression liên quan và run thực tế. Setup kiểm thử tự động là việc nền tảng riêng cần ghi trong milestone; hiện repo chưa có test suite gameplay. Không ghi test PASS nếu chưa chạy. Mọi báo cáo có version dữ liệu/build/seed/setup/expected/observed.

## 7. Việc chưa xác nhận và cách xử lý

- Ghép/bản sao/vũ khí trước–sau mua, sau ghép, targeting và altar: giữ ledger `NEEDS_REFERENCE`; lấy video/ảnh có phiên bản và thao tác đủ chứng minh. Không hỏi lại những lựa chọn người dùng đã giao đối chiếu bản gốc, không bịa luật để nhanh code.
- Người dùng đã duyệt style nền và luật không cánh tay; còn cần duyệt motion/chi tiết độc lạ trên prefab tách lớp đầu tiên.
- Giá, interest cap/rate, stat baseline, timer từng wave và số wave là tunable khởi đầu, không coi số gợi ý trước đó là cân bằng cuối.
- URP, package input, module Android/iOS, thiết bị/tài khoản store/build service và SDK Steam chưa được xác nhận sẵn sàng. Mỗi phụ thuộc phải kiểm tra riêng; không tự trả phí hay đăng ký dịch vụ.

## 8. Bàn giao mỗi checkpoint

1. Scene chạy được, prefab và dữ liệu cần thiết; nguồn art/animation và metadata đi cùng, không chỉ ảnh render.
2. Capture/video từ Unity đúng build; ghi rõ prototype vs production, phần chưa làm và known issues.
3. Báo cáo validation thực tế và quyết định/tune vừa thay đổi; không che warning/error.
4. Cập nhật plan và trạng thái checkpoint. Không commit/push hoặc tạo nhánh nếu chưa được yêu cầu.

**Bước implement đầu tiên:** P0 + phần nền tảng độc lập của P1, ưu tiên prefab không cánh tay với bàn tay gắn vũ khí, movement và một combat testbed. Chưa bắt đầu code trong lượt lập plan này.
