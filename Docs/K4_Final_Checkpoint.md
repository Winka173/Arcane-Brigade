# K4 — functional Windows gate DONE

Ngày 2026-10-13, Unity6000.4.8f1. K4.1–K4.5 đã implement, nối vào game thật, chạy regression và native Windows vertical slice. **DONE là gate runtime Windows**, theo quyết định device gate sang K7; không phải final art/audio/balance/mobile/Steam release approval. Font/UI redesign giữ K6–K7 theo feedback chủ dự án.

## Phạm vi đạt

| Bước | Runtime/presentation | Bằng chứng |
| --- | --- | --- |
| K4.1 | Ô/Bell/Stamp target/cadence/tier, pose và VFX/SFX khác nhau | 40 signature checks, native slice đã có |
| K4.2 | Hai build, bốn relic HD, Carbon connector, graph/HP ledger thật | 36 final integration checks; native graph, Harvest cadence và Office crit bonus |
| K4.3 | Merge/altar ghost reveal sau save, queue/dedup/skip, tier silhouette và idle | Merge checks PASS; native quầy→random roll→tier2→reveal finish/skip |
| K4.4 | Stage bell/cart, seed+wave placement, warning/cooldown, boss shift | 49 arena checks, native automatic hit→charge→warning→payoff, boss phase→rest |
| K4.5 | Discovered-only sandbox, cloned data/RNG/scene, tier/preset/reset/exit | 174 sandbox checks; native moving tier3 stamp and actual save bytes unchanged |

Gate tổng: một lát cắt combat → logical kill/loot → shop thật → merge → build payoff → boss phase/defeat → rest save → menu/sandbox chơi được. Native là **fixture có seed và tuning riêng**, không gọi là run tự nhiên hoặc playtest balance. Native notes liệt kê đầy đủ preset.

## Build và ledger

- **Máy thu hoạch:** kill→coin→pickup→Harvest Receipt→Bell beat→sóng trễ. Bell+Receipt đã active, không yêu cầu đủ set. Một pickup cấp 1 nhịp (stack2 cấp2), cooldown0.35s, chọn Bell gần trong6units; không sinh coin từ coin. Receipt đổi tiện lợi lấy payoff qua pickup radius−0.15/stack.
- **Văn phòng hoàn trả:** Stamp→Reflective Ink→burst→weakness→Office Lens crit bonus. Ink thêm một dấu/hit nhưng expiry×0.65, nên Stamp+Ink đã đổi cadence. Lens +15% companion crit và−5% attack speed/stack; burst mở1.8s weakspot, crit tăng thêm30–50% theo stack. Không crit→stamp recursion. Details hiển thị expiry/count hiệu lực khi có Ink.
- **Carbon Ribbon:** Bell wave mở weakness1.5s cho Lens; Stamp burst hút coin đủ điều kiện trong2.5units. Stamp+Carbon đã có utility, không cần đủ hai bộ; không copy damage/death/currency, crit team−4%, unique. Base expiry/cooldown/radius là tuning riêng, không thông số Bizarre Brigade đã xác minh.
- Ledger đo HP thực mất/hồi và coin thực credit, loại overkill/overheal/visual damage; bonus Lens chia từ cùng HP loss, không cộng toàn damage lần hai. Source unit/weapon/tier/effect riêng, activation là số thực thi effect, không claim DPS hoặc dự đoán contribution. Snapshot chưa settle hiện riêng trong Details/Links; settled rows save và cộng đúng một lần theo phase guard. Ledger float tổng cap1e12, coin/activation capint.MaxValue như counter policy; không cắt gameplay damage khi chạm cap báo cáo.
- Rà soát độc lập phát hiện và đã sửa attribution Ink khi chỉ sở hữu Lens, và depth Bell payoff khi Harvest hoàn thành nhịp; có regression mới. Wave/echo luôn depth1 cùng root/source thật; secondary không proc hoặc charge prop.

## Merge, tier và personality

Chỉ quầy giao dịch save thành công mới enqueue. Payload từ tuple roster trước/sau và receipt ID commit, không đoán vũ khí copy thứ ba hoặc roll reveal. Ghosts không teleport/collider gameplay, queue/skip/reload không tiêu hoặc mutate RNG. Reveal final committed result của cascade, không dựng trung gian chưa quan sát. Altar có cue riêng, không modal shop.

Tier weapon proportions/attachments và bell satellites đọc được không cần particle; hand vẫn trên grip, không thêm arm. Idle beat ngắn/deterministic, hit/attack/telegraph ưu tiên. Settings reduced motion lưu trong profile, giữ tier/icon/audio nhưng bỏ gathering/idle. Backspace tại rest skip visual-only. Mesh weakspot bracket mỏng trên enemy không che danger; expiry/death/rebind xử lý riêng. Art silhouette refinement và readability dense-combat cuối vẫn K5/K7.

## Arena

Wave1 giới thiệu StageBell; từ wave2 có cả bell/cart nếu đặt được vị trí an toàn. Dedicated seed/wave RNG, không đụng shop/combat/loot RNG; biên34×22/camera giữ nguyên. Root primary allied impact mới charge, stable projectile cause theo swept segment cắt tại first blocker; melee spillover theo actual hit contact. Không gọi từ Fire/windup/Space, không duplicate root hoặc prop→prop recursion.

Bell cần3roots, cart2; warning1.2s, cooldown5s. Bell14damage/radius2.4; cart lane4units, roll0.8s có slowdown, swept hit mỗi enemy một lần. Boss ×0.35 damage và cap8% maxHP/payoff. Prop chỉ hại enemy, không block/hại ally/player hoặc bắt giải puzzle.

Boss <=50%HP: destination rings1.2s trước chuyển props, revalidate Champion/loot/telegraph/vùng biên lúc hoàn tất. Nếu điểm không còn an toàn, prop ở lại; không teleport brigade hoặc đổi bullet damage. Chỉ một phase, death/rest/reset dọn warning/toy timers. Native brass/mint outlined meshes scale ở4K; warning lane dưới danger layers. Tiny/crowded arena có thể bỏ prop thay vì spawn gây kẹt.

## Sandbox và persistence

Menu **THỬ VŨ KHÍ** chỉ dùng `SeenWeapons` hoặc weapon thực của saved roster/champion. SeenUnits không suy ra mọi weapon có thể roll. Preview tier1–3 ghi rõ NOT OWNED; stationary/moving target, keyboard/touch buttons/controller bindings. Dùng CombatSimulation/UnitVisual/projectile/effects/audio cùng runtime, no build bonuses/body passive, dummy bất tử, không thưởng/farm kills. Templates/runtime assets/camera scene/RNG riêng, reset/dispose có checks. Desktop RT2560×1280 MSAA4; mobile880×440 MSAA2 chưa nghiệm thu device. Các input binding có source, không coi đó là test tay cầm/touch thật.

Run schema vẫn2, ledger thiếu trong save cũ migrate thành empty; profileVersion1 thêm SeenWeapons/ReducedMotion, thiếu discovery thành empty. Progress ghi roster weapon thật tại boundary, champion default weapon cũng ghi khi đã vào run. Sandbox không gọi storage hoặc mutate profile/currency/score/discovery/Unity RNG. Native chứng minh selection/reset/exit giữ byte-identical `run.json` và `profile.json`; release/dispose render target sau preview.

## Verification và giới hạn

- Build Windows: Succeeded, 0 errors / 0 warnings (`PreviewExports/FullRun_Build.txt`). Binary local ignored: `Builds/FullRun/ArcaneBrigade.exe`.
- Regression: Checkpoint17, Squad12, FullRun97, K0/K1 27, K2 23, transaction36, modifier29, altar14, K3Final31, HD31; K4Signature40, Merge PASS, Arena49, Sandbox174, Final36. Actor smoke checks không thay device tests.
- Native seven PNGs3840×2160, WindowsD3D11 live1920×1080 + Unity supersize2, không resize ngoài Unity. Không realtime4K60FPS benchmark, không video/motion audition. Actor fixture startsHall3 with seeded roster/relics/gold/loot, duration18s, enemyAI/attacks/ambient spawns/allypatrol frozen, squad relocated near seededprops; Stampcrit1, bossHP3000 và bullets frozen. Actual impacts/root/cadence/loot/transactions/merge/phase/settlement/save/sandbox chạy production code, không bơm fake proc/toy hits. Shop preset chọn unit offer, weapon vẫn roll thật.
- Native assertions/capture notes: `PreviewExports/K4/Final/Player/Capture_Notes.txt`; gallery `index.html`. Editor reports `Final/Checks.txt`, `Sandbox_Checks.txt`, signature checks; full log mirror `Final/Editor_Checks.txt`. Hash version snapshot: `Final/Artifact_SHA256.txt`.
- Nguồn relic PNG1269×1240 RGBA thật, alpha-bounded slices giữ gutter lệch, lossless/noCPUcopy; không native4K. Bộ weapon/body/map HD cũ giữ nguyên. Prompt built-in: `K4_Relic_Generation_Prompt.md`. Prop mesh không phụ thuộc raster upscale.
- SFX/cue là synthesis nguyên bản/study, chưa compose/audition/mix soundtrack production. Chưa human playtest natural full run/Endless hoặc mobile/controller hardware. Font/UI chưa redesign. Các gate này thuộc K5–K7, không ghi đã đạt chỉ vì compile/check PASS.

## Chạy lại và bước tiếp

Unity menu `Arcane Brigade > K4 > Apply full signature slice check and build`; migration idempotent, không dựng baseline. Check-only/build: `Check full signature slice and build`. Native: `-arcane-k4-final-capture -capturePath "<absolute folder>" -force-d3d11 -screen-width 1920 -screen-height 1080 -screen-fullscreen 0`.

K5 tiếp tục mở rộng family/build/props/enemy/boss qua quality gate, không tăng số content để che core. K6 museum/results/meta và redesign font/UI đã nhận feedback; K7 production feel/audio/perf/devices/Steam requirements. **Không còn hạng mục K4 functional gate Windows chưa implement**; không nâng các nhãn production/mobile cùng lúc.
