# Balance pass — 19/10/2026

Yêu cầu (play-test): cân lại item, companion, hero, bond, địch theo mọi độ khó và kinh tế. Một item cho +20–30 máu tối đa mà món tốn máu lại rẻ; item damage cho quá nhiều so với giá; độ hiếm không khớp chỉ số; popup thay đổi chỉ số cảm giác như hiện **tổng tích lũy** chứ không phải chênh lệch của **lần mua đó** như Bizarre Brigade.

Công cụ: `Assets/ArcaneBrigade/Editor/BalanceAudit.cs` (`BalanceAudit.Run` = bảng tĩnh, `BalanceAudit.Sim` = 36 campaign bot), `Assets/ArcaneBrigade/Editor/BalancePass.cs` (bảng chỉnh, gọi ở cuối `ContentExpansionBuilder.Apply`). Dữ liệu: `PreviewExports/Balance/Before` và `PreviewExports/Balance/After`.

## 1. Đơn vị giá trị (VU)

1 VU = +1% damage toàn đội. Trọng số là ước lượng theo công thức của chính Arcane (crit ×1.5, giáp `1/(1+a)`, hero ~110 HP, hero power ×3, wave ~45 s, vàng thưởng giết quái), **không** phải số của Bizarre Brigade (các tài liệu BB trong `Docs/` không có bảng số item; không chép số).

| Chỉ số | VU / 1.0 | Ghi chú |
|---|---:|---|
| Damage %, Attack speed %, Damage correction | 100 | toàn đội |
| Champion/Companion damage %, Companion attack speed % | 55 | chỉ một phía |
| Crit % | 50 | đòn chí mạng ×1.5 |
| Flat melee/ranged damage | 3 / điểm | nửa đội, gốc vũ khí ~12 |
| Move speed %, Speed correction | 40 | |
| Attack range % | 50 | |
| Max HP | 0.75 / HP | hero 110 HP: +1 HP ≈ +0.9% máu hiệu dụng |
| Regen | 15 / HP/s | |
| Armor, Dodge, Health correction | 80 | +0.1 giáp ≈ +10% máu hiệu dụng |
| Projectiles / Pierce / Split | 40 / 10 / 12 mỗi điểm | |
| Bounce %, Bullet size %, Contact frequency % | 15 / 10 / 15 | |
| Contact damage | 1.2 / điểm | đòn chạm mỗi 0.8 s |
| Income | 2.5 / vàng mỗi wave | |
| Discount / Relic / Recruit discount | 120 / 80 / 60 | |
| Luck, Shop rarity, Treasure, Pickup, Roam, Interest, Heart healing | 25, 30, 8, 4, 3, 2/điểm, 1/điểm | tiện ích |
| Enemy count / health / speed (+) | −30 / −50 / −50 | buff địch = chi phí |
| Score multiplier, Body/Companion size | 0 | không có sức chiến đấu |

Chỉ số chỉ áp cho hero hoặc chỉ cho đồng minh (scope Champion/Companions) của nhóm tấn công tính ×0.55. Máu trả ở altar là **máu hiện tại** (mất trong một wave): 0.3 VU/HP.

Mục tiêu theo độ hiếm (chỉ phần chỉ số): Common 12, Uncommon 19, Rare 28, Mythic 40, Prismatic 55 VU, dải ±30%. Item tiện ích (kinh tế/loot/chỉ số lạ chiếm >50% giá trị) có sàn 50%. Item có cơ chế (mechanic/signature/proc) chỉ bị cắt khi riêng phần chỉ số đã vượt trần độ hiếm. Item điểm (score multiplier + buff địch) nằm ngoài ngân sách.

## 2. Kiểm kê (Before) — các lệch chính

**Item chỉ số theo độ hiếm** (95 item có ngân sách chỉ số: chỉ số + tiện ích; 105 item còn lại là cơ chế/điểm, liệt kê trong Items.csv):

| Độ hiếm (mục tiêu VU) | Before: min / trung vị / max | Trong dải ±30% | After: min / trung vị / max | Trong dải |
|---|---|---:|---|---:|
| Common (12) | 0 / 10 / 49.5 | 9/36 | 6 / 10.5 / 14 | 28/36 |
| Uncommon (19) | −0.75 / 11.3 / 75.7 | 9/40 | 9.6 / 15.8 / 24.1 | 30/40 |
| Rare (28) | 0 / 15 / 80 | 0/16 | 14.3 / 23.8 / 31 | 14/16 |
| Mythic (40) | 2.2 / 58 / 111 | 0/3 | 34 / 41.5 / 46.9 | 3/3 |

Item ngoài dải ±30% sau khi sửa đều là item tiện ích (sàn 50%, đều đạt) hoặc 3 khế ước k10 chỉ số khó đo (armor correction, bounce, travel distance — 50–51% mục tiêu), giữ để không thổi phồng chỉ số lạ.

- **Máu tối đa cho không**: Honey Pot (Common, 22 vàng) +40 HP; Marrow Tea (Common) +50 HP +0.8 HP/s = 49.5 VU (4× Common); Sun Hat (Uncommon) +40; Rose Water (Rare) +90 HP chỉ đổi −2 HP/s; Starlight Quilt (Mythic) +90 HP **bán bằng 23 HP hiện tại** — mua xong còn lời 67 HP vì max HP tăng được cộng thẳng vào máu hiện tại.
- **Chi phí máu tượng trưng**: 33 khế ước k10 chỉ trừ −1 max HP (0.75 VU); giá altar bằng máu của item Common/Uncommon chỉ 3–4 HP (hero 85–225 HP, mỗi wave hồi đầy).
- **Damage quá rẻ**: Clock Spring (Common) +20% tốc đánh = 20 VU; Breath Jar, Noon Spark, Ricochet Seltzer 16–18 VU ở Common; Twin Seal (Rare, 35 vàng hoặc 17 HP) +2 đạn toàn đội = 80 VU; Echo Postage +3 đạn; Truce Bottle 111 VU; Cracked Phone (Uncommon) +25 vàng/wave + chia đạn 2 = 76 VU (thu nhập gốc chỉ 15 + wave).
- **Item yếu so với độ hiếm**: Victor Draught (Rare) net 0; Battering Helm (Mythic) 2 VU; 13 khế ước k10 Uncommon chỉ +8% một chỉ số lạ (< 2 VU).
- **Bond**: VU mỗi thành viên ở ngưỡng đầu từ 0.23 (Rain Script) tới 5.4 (Runic School/Prizefighters) — lệch 23×. Theme bond ở 6 người: Archive 43 VU, Clock 38 nhưng Astral 4.3, Orchard 5.4, Tidal 6.5. **Lỗi**: hệ số 0.9 cho thưởng bond có lợi bị áp vào chỉ số số nguyên (pierce/split được làm tròn xuống khi compile) nên Runic School và Little Horns ở 2 người cho +0.9 → **không có tác dụng** tới khi đủ 4 người.
- **Companion**: modifier hỗ trợ rất nhỏ (tier 1: −5.8 … +11 VU, trung vị 0); sức mạnh nằm ở vũ khí/kỹ năng. Đã có heal/armor/dodge cho **hero** (Teapot Pilgrim, Candle Snail, Spring Duck, Pillow Oracle, hai Prismatic) — không thêm, không tăng (companion không buff hồi máu/giáp/né).
- **Hero**: HP gốc 60–225 (trung vị 105); DPS gốc (damage × hero power × số đạn / hồi chiêu) từ 14 (Archive Curator) tới 253 (Lone Jelly, solo). Lệch lớn nhưng kit/cơ chế bù (HeroMechanicBalance đo riêng) — không chỉnh trong lượt này.
- **Địch**: đường cong mượt — HP ×1.08 (w1) → ×31 (w20) ở Peaceful, ×1.94 → ×92 ở Beyond the Lamp; mỗi bậc độ khó ×1.22–1.26 ở w20; bước wave lớn nhất ×1.26–1.31. Không sửa. Nhưng bot **không thấy độ dốc độ khó** (Peaceful 3 thắng, Beyond the Lamp 4 thắng) vì cuối trận dư vàng.
- **Kinh tế**: vàng từ quái swarm tăng ~40× (w1 → w19) còn giá tăng ~20×. Ví / giá offer trung bình: 5.6 (w1–5) → 12.5 (w6–10) → 16 (w11–19); bot mua ~18 item mỗi shop cuối trận; reroll cố định 2 + 2n vàng (≈ 0.5% giá một item cuối trận).

## 3. Thay đổi đã áp dụng

**Item** (`BalancePass.Items`: 100 item đổi chỉ số; Truce Bottle và Fish Cracker sửa tại nguồn trong `ContentExpansionBuilder`). Ví dụ:

| Item | Độ hiếm | Before (VU) | After (VU) | Giá gốc vàng / altar |
|---|---|---|---|---|
| honey-pot | Common | Health/C 40 (30) | Health/C 15 (11.25) | 22→22 |
| K26Defense0 | Common | Health/C 50; Heal/C 0.8 (49.5) | Health/C 12; Heal/C 0.3 (13.5) | 32→26 |
| CurioOrchardFizz | Common | Health/C 20; Income/R -2 (10) | Health/C 18; Income/R -2 (8.5) | 32→17 |
| CurioDaylightBonnet | Uncommon | Health/C 40 (30) | Health/C 24 (18) | 46→46 |
| CurioPetalTonic | Rare | Health/C 90; Heal/C -2 (37.5) | Health/C 55; Heal/C -1.2 (23.25) | 60→39 |
| starlight-quilt | Mythic | Health/C 90 (67.5) | Health/C 50 (37.5) | 48→48 · altar HP 23→trứng 7 |
| k10-bulletpierce | Common | BulletPierce/T 2; Health/C -1 (19.25) | BulletPierce/T 2; Health/C -8 (14) | 21→27 |
| k10-income | Common | Income/R 4; Health/C -1 (9.25) | Income/R 5; Health/C -5 (8.75) | 18→18 · altar HP 3→HP 8 |
| CurioCoralBroth | Common | Heal/C 1; Health/C -4 (12) | Heal/C 1; Health/C -6 (10.5) | 32→21 |
| clock-spring | Common | AttackSpeed/T 0.2 (20) | AttackSpeed/T 0.12 (12) | 20→24 |
| CurioBreathJar | Common | Damage/T 0.2; AttackSpeed/T -0.04 (16) | Damage/T 0.16; AttackSpeed/T -0.04 (12) | 32→24 |
| CurioNoonSpark | Common | MeleeDamage/T 5; Crit/T 0.06 (18) | MeleeDamage/T 3; Crit/T 0.08 (13) | 32→25 |
| CurioShatteredPager | Uncommon | Income/R 25; BulletSplit/C 2 (75.7) | Income/R 7; BulletSplit/C 1 (24.1) | 46→42 |
| CurioTruceBottle | Mythic | Damage/T 0.4; Crit/T 0.2; AttackRange/T 0.4; Armor/C 0.6; MoveSpeed/C -0.3 (111.4) | Damage/T 0.2; Crit/T 0.15; AttackRange/T 0.2; Armor/C 0.2; MoveSpeed/C -0.3 (46.9) | 74→67 |
| twin-seal | Rare | Projectiles/T 2 (80) | Projectiles/T 1; AttackSpeed/T -0.1 (30) | 35→48 · altar HP 17→HP 18 |
| echo-postage | Mythic | Projectiles/T 3; AttackSpeed/T -0.08 (112) | Projectiles/T 2; AttackSpeed/T -0.3 (50) | 60→60 |
| tarnished-crown | Mythic | ChampionDamage/C 0.8 (44) | ChampionDamage/C 0.6 (33) | 48→48 |
| CurioVictorDraught | Rare | Crit/T 0.2; BulletPierce/T 1; Damage/T -0.2 (0) | Crit/T 0.3; BulletPierce/T 1; Damage/T -0.05 (20) | 60→34 |
| CurioBatteringHelm | Mythic | CollisionDamage/C 40; CollisionFrequency/C 0.7; RangedDamage/T -10 (2.18) | CollisionDamage/C 70; CollisionFrequency/C 0.7; RangedDamage/T -6 (33.98) | 74→56 |

Nguyên tắc: máu tặng không điều kiện ≤ +15 (Common), ≤ +25 (Uncommon); máu lớn hơn chỉ ở Rare+ hoặc kèm chi phí thật. Chi phí máu của khế ước k10: Common −5, Uncommon −6, món mạnh −8 (trước: −1). Item cơ chế chỉ bị cắt phần chỉ số khi riêng phần đó đã vượt trần (Sun Hat, Retort Cup, Watchful Tea, Bent Tape, Starlight Quilt, Tarnished Crown, Echo Postage, Iron Apron); Liar Mask, Mute Clock, Split Kernel, Urchin Ball giữ nguyên vì cơ chế là chi phí lớn (mất damage vĩnh viễn, mất 1 HP/s, vào trận nửa máu, hero ngừng đánh).

**Giá**: item chỉ số đặt giá trong dải cũ của độ hiếm (C 15–32, U 24–46, R 30–60, M 48–75) theo ngân sách: tỷ lệ 0.6 → đáy, 1.4 → trần (83 giá đổi). **Altar bằng máu**: giá sàn 8 / 13 / 18 / 27 HP hiện tại cho C/U/R/M sau hệ số độ hiếm (trước: 3 / 4 cho C/U). Item tăng max HP (Starlight Quilt, k10 Health correction) không còn bán bằng máu, chuyển sang trứng.

**Bond** (`Tools/Content/expansion_lists.json` + `expansion_design.py` `BOND_EFFECTS`; mục tiêu ~2.5 VU/thành viên, theme ~30 VU ở 6 người):

| Bond | Hiệu ứng | Giá trị/cấp | VU ở 2 người | VU ở 6 người |
|---|---|---|---|---|
| archive | Damage | 0.08 → 0.06 | 7.2 → 5.4 | 43.2 → 32.4 |
| clock | AttackSpeed | 0.07 → 0.06 | 6.3 → 5.4 | 37.8 → 32.4 |
| frost | EnemySpeed | -0.05 → -0.07 | 2.5 → 3.5 | 15 → 21 |
| orchard | Luck | 0.04 → 0.15 | 0.9 → 3.38 | 5.4 → 20.25 |
| astral | BulletSize | 0.08 → 0.2 | 0.72 → 1.8 | 4.32 → 10.8 |
| tidal | BulletBounce | 0.08 → 0.15 | 1.08 → 2.03 | 6.48 → 12.15 |
| oddity | Crit | 0.05 → 0.08 | 2.25 → 3.6 | 13.5 → 21.6 |
| hare | Knockback | 0.15 → 0.3 | 0.68 → 1.35 | 2.03 → 4.05 |
| archer | RangedDamage | 3 → 2 | 8.1 → 5.4 | 24.3 → 16.2 |
| gauntlet | MeleeDamage | 4 → 2 | 10.8 → 5.4 | 32.4 → 16.2 |
| regal | CompanionDamage | 0.08 → 0.1 | 3.96 → 4.95 | 11.88 → 14.85 |
| clover | ShopRarity | 0.08 → 0.15 | 2.16 → 4.05 | 6.48 → 12.15 |
| gear | Income | 4 → 3 | 9 → 6.75 | 27 → 20.25 |
| hat | Interest | 2 → 3 | 3.6 → 5.4 | 10.8 → 16.2 |
| horn | BulletPierce | 1 → 1 | 9 → 10 | 27 → 30 |
| rune | BulletSplit | 1 → 1 | 10.8 → 12 | 32.4 → 36 |
| mask | ChestQuality | 0.1 → 0.3 | 0.9 → 2.7 | 2.7 → 8.1 |
| hide | Pickup | 0.4 → 0.6 | 1.44 → 2.16 | 4.32 → 6.48 |
| shoe | MoveSpeed | 0.08 → 0.1 | 2.88 → 3.6 | 8.64 → 10.8 |
| rain-script | BulletSpeed | 0.1 → 0.25 | 0.45 → 1.13 | 1.35 → 3.38 |
| paper-wings | BulletDistance | 0.12 → 0.25 | 0.54 → 1.13 | 1.62 → 3.38 |
| sweet-tooth | CompanionAttackSpeed | 0.07 → 0.09 | 3.47 → 4.46 | 10.4 → 13.37 |
| static-club | ChainChance | 0.04 → 0.06 | 1.8 → 2.7 | 5.4 → 8.1 |
| exhibit-hall | RecordedDamage | 0.1 → 0.12 | 3.6 → 4.32 | 10.8 → 12.96 |

Hearth (Heal) giữ nguyên để không tăng hồi máu qua companion. `BondDefinition.BalanceReward` không còn nhân 0.9 cho pierce/split/projectiles (Runic School, Little Horns có +1 ngay ở 2 người; Before thực tế là 0).

**Kinh tế** (`CarnivalBalance.SwarmCoinPressure`, `RunSession.RerollBaseCost`): giá shop × (1 + min(1.2, 0.15 × (wave − 5))) — không đổi tới wave 5, ×1.75 ở w10, ×2.2 từ w13. Phí reroll (2 + 2n) × max(1, hệ số giá / 2): không đổi tới ~wave 6; w10: 9 / 26 vàng (lần 1 / lần 3), w19: 45 / 133. Thu nhập, lãi, vàng quái giữ nguyên.

**Địch, hero, companion**: không đổi số.

## 4. Popup thay đổi chỉ số

Trước: `ShopStatSnapshot` chụp chỉ số **đã compile** trước/sau **mỗi giao dịch** (không cộng dồn theo shop), nhưng `ShopStatFeedback` hiện `After − Before` của giá trị compile, làm tròn số nguyên. Giá trị compile đã nhân mọi bonus, hero power ×3, sao và soft cap, nên cùng một món +12% damage hiện "Damage +4" lúc đầu và "Damage +40" khi build đã lớn — con số lớn dần theo **tổng build**, cảm giác như tổng tích lũy. Chỉ số nhỏ (giáp +0.15, hồi +0.8/s, may mắn +20%) hiện "+<1".

Bizarre Brigade: các tài liệu `Bizarre_Brigade_15_*` không ghi định dạng popup; giả định theo footage và phản hồi người chơi: popup hiện thay đổi do **chính lần mua**, theo đơn vị của chỉ số, ví dụ "+5% damage", "+20 máu tối đa".

Sau: `ShopStatSnapshot` chụp thêm bonus nguồn (`session.Bonus` theo scope hero/companion). Khi giá trị compile đổi vì bonus đổi, popup hiện **delta bonus của giao dịch đó** theo đơn vị chỉ số: "Damage +12%", "Damage +4 flat", "Speed +22%", "Attacks/s +12%". Chỉ số chạm trần (compile không đổi) vẫn im lặng. Thay đổi không đến từ bonus (ghép sao, nội tại) hiện % tương đối ("Damage +170%"). Giáp/hồi/may mắn hiện số thập phân ("+0.15", "+0.8", "+20%"). Hóa đơn ghi tổng bonus trước → sau. Không sửa `RunHud.cs`: nó vẫn gọi `statFeedback.Observe/Draw`; `ShopStatFeedback` đọc trường mới `ShopStatDelta.Display`.

## 5. Kiểm chứng

Bot (`BalanceAudit.Sim`): 6 hero (Lantern Knight, Postmaster, Drowsy Seer, Comet Runner, Glass Dice, Tide Singer) × 6 độ khó, seed 161026, người mua chấm điểm theo VU; cắt wave thường 90 s, boss 300 s. Đây là bằng chứng bot, không phải tỷ lệ thắng người chơi; một seed mỗi ô.

| Độ khó | Before thắng / thua / hết giờ | After thắng / thua / hết giờ |
|---|---|---|
| 0 Peaceful | 3 / 2 / 1 | 4 / 2 / 0 |
| 1 Wandering | 4 / 0 / 2 | 3 / 1 / 2 |
| 2 Restless | 3 / 1 / 2 | 4 / 2 / 0 |
| 3 Nightmare | 3 / 2 / 1 | 4 / 1 / 1 |
| 4 Sleepless | 4 / 1 / 1 | 4 / 2 / 0 |
| 5 Beyond the Lamp | 4 / 1 / 1 | 2 / 4 / 0 (trung vị wave 15) |
| Tổng 36 | 21 / 7 / 8 | 21 / 12 / 3 |

| Wave | Ví / giá offer: Before → After | Item mỗi shop | Reroll mỗi shop |
|---|---|---|---|
| 1–5 | 5.6 → 6.2 | 3.7 → 4.2 | 1.0 → 1.2 |
| 6–10 | 12.5 → 8.9 | 10.9 → 8.5 | 3.6 → 2.5 |
| 11–15 | 16.0 → 8.6 | 18.1 → 10.0 | 6.5 → 3.2 |
| 16–19 | 15.7 → 8.2 | 18.4 → 9.6 | 7.6 → 3.3 |

Sim After chạy trước lần tinh chỉnh cuối 7 khế ước k10 tiện ích (+1–4 VU mỗi món, phần lớn bán ở altar); bảng tĩnh After là số cuối.

Bộ kiểm tra hiện có (Unity batch, sau khi áp dụng):

- `ContentExpansionChecks.Run`: CONTENT_EXPANSION_CHECKS_PASSED 388
- `ShopDescriptionChecks.Run`: SHOP_DESCRIPTION_PASS 2583
- `RosterCapAndBondChecks.Run`: ROSTER15_PASS 1149
- `HeroIdentityChecks.Run` (gồm popup shop: damage của hero và companion tăng, delta âm giữ lại): HERO_IDENTITY_PASS 2982
- `ShopFlowChecks.Run`: SHOP_FLOW_CHECKS 17 PASS · `PacingChecks.RunBatch`: PACING_CHECKS PASS · `ModeBalanceChecks.Run`: MODE_BALANCE_CHECKS 4229 PASS · `K10Checks.Run`: K10_FOCUSED_CHECKS_PASSED 176
- `K32Checks.Run`: ARCANE_K32_CHECKS_PASSED 29 (kèm checkpoint 17, squad 12, K0/K1 27, K2 131, K3 transaction 36)
- `K32AltarChecks.Run`: ARCANE_K32_ALTAR_CHECKS_PASSED 14
- `ActOneStoryChecks.RunWide` (lần chạy cuối, sau khi nhóm khác đưa vào vật lý va chạm): đạt ContentExpansion 388, K32 29, K32 altar 14, K3 final 31, K2 131, K10 176, K15 206, K16 205, HeroAwakening 288, VisualQuality 31, FeelTelegraph 21, K8 audio 114, K7 73 và các suite khác; **6 FAIL không thuộc lượt này**: October "K24 balanced melee weapon retains increased source reach" (MeleeBalance.Range 1.1), K14 "Spring hammer impact applies weapon knockback", K4Arena "fast sweep truncates at blocking hit", K4Signature/K4Final "Bell chimes strike the foe ahead…" (vật lý va chạm/cận chiến mới), HeroIdentity "Win32 IO 1224" (file `PreviewExports/HeroIdentity/Checks.txt` bị khóa; chạy riêng `HeroIdentityChecks.Run` đạt 2982). Lần chạy trước vật lý mới, K4Final chỉ hỏng ở khẳng định Guardian Canopy (đã sửa).
- `BuildBalanceChecks.Run`: dừng ở `BossReliefChecks` "Last Customer override gets 40 percent HP reduction 0" — **lỗi có sẵn**, không do lượt này: check còn ghi hệ số 0.75 trong khi `OctoberBalance.EnemyHealthRelief` = 0.95 từ commit d723b4a8 (18/10). Không sửa.

Check đã cập nhật vì khẳng định số tuning cũ: `RosterCapAndBondChecks` (không nhân 0.9 cho pierce/split/projectiles), `FullRunChecks` (bán kính tuần tra của 2 Wandering Map đọc từ dữ liệu thay vì hằng 0.8). Guardian Canopy dùng tốc đánh đồng minh thay vì damage đồng minh để giữ khẳng định "build damage chỉ cho hero không đổi đồng minh".

Lưu ý về thay đổi của nhóm khác: cả ba sim đã gồm thay đổi cận chiến của chủ dự án (`MeleeBalance.Range` 1.1, `Area` 0.8). Vật lý va chạm mới (`ImpactPhysics.cs`: trọng lượng vũ khí, choáng, đập tường, xác văng) vào sau sim After, nên đã chạy thêm `PreviewExports/Balance/AfterPhysics` (cũng gồm 7 khế ước k10 đã tinh chỉnh và Guardian Canopy cuối): **20 thắng / 11 thua / 5 hết giờ** (độ khó 0–5: 3/4/4/3/2/4 thắng). Tốc độ giết gần như không đổi (wave 16–19: 99.9 → 97.5 quái/giây; wave boss 33.1 → 30.2 s; wave 1–10 giống hệt), kinh tế giữ nguyên (ví / giá offer 6.2 / 8.9 / 8.4 / 7.8). Không cần chỉnh thêm vì vật lý va chạm. Wave boss chậm hơn Before (21.3 → 30–33 s) vì bot mua ít item hơn.

## 6. Việc còn mở

- Bot vẫn chưa thấy độ dốc độ khó rõ ràng (After 4/3/4/4/4/2 thắng, AfterPhysics 3/4/4/3/2/4) và chỉ có một seed mỗi ô. Cần chơi thử hoặc chạy nhiều seed hơn trước khi chỉnh thêm HP/damage địch.
- Giá altar bằng máu là số cố định: hero 60 HP (Glass Dice) trả 27 HP cho Mythic = 45% máu, hero 225 HP chỉ 12%. Có thể đổi sang % máu tối đa nếu muốn.
- Kinh tế cuối trận nay gần mức đầu trận (ví ≈ 8 lần giá một offer). Muốn shop cuối "giàu" hay "nghèo" hơn chỉ cần chỉnh 0.15 / trần 1.2 trong `SwarmCoinPressure`.
- Chạy lại builder cũ (October/K10/K26/SecretEvent) sẽ ghi đè item; như trước, luôn chạy `ContentExpansionBuilder.Apply` sau đó (hàm này gọi `BalancePass.Apply`).

## 7. Round 2 — quyết định của chủ dự án (19/10/2026)

Ba câu hỏi mở được quyết định: (1) đo bằng 3 seed mỗi ô (6 hero × 6 độ khó = 108 campaign) và chỉnh **hệ số theo độ khó** (không đổi đường cong theo wave) cho tỷ lệ thắng bot ~90 / 80 / 65 / 50 / 40 / 25–35%; (2) giá altar bằng máu theo % máu tối đa; (3) kinh tế cuối trận: ví ÷ giá offer ≈ 5–6 ở wave 11–19, ~6–7 lần mua mỗi shop, wave 1–5 giữ nguyên.

### Thay đổi

- **Altar bằng máu** (`RunSession.SpecialItemPrice`): giá = max(giá authored, % × máu tối đa) với Common 8%, Uncommon 12%, Rare 18%, Mythic 25%, Prismatic 32%; giá authored đã có sàn 8 / 13 / 18 / 27 HP. Ví dụ Mythic: hero 60 HP trả 27 (sàn), hero 110 HP trả 28, hero 225 HP trả 57. Quầy, mô tả và hóa đơn đều đọc `offer.Price` nên số hiển thị là giá thật. `MarketPolishChecks` đổi sang so với `SpecialItemPrice`.
- **Kinh tế** (`CarnivalBalance.SwarmCoinPressure`): 1 + min(2.4, 0.3 × (wave − 5)) — wave 1–5 không đổi, ×1.9 ở w8, ×2.5 ở w10, ×3.4 từ w13 (round 1: trần ×2.2). Phí reroll đi theo nửa hệ số giá như round 1.
- **Độ khó** (`JourneyBalance`: bảng hệ số cho 6 độ khó, đạt đủ từ wave 8, ramp từ wave 1 như cũ):

| | Peaceful | Wandering | Restless | Nightmare | Sleepless | Beyond the Lamp |
|---|---:|---:|---:|---:|---:|---:|
| HP trước | 1 | 1.215 | 1.52 | 1.915 | 2.4 | 2.975 |
| HP sau | 0.7 | 1.0 | 1.3 | 1.75 | 2.3 | 2.95 |
| Damage trước (× .62) | 1 | 1.158 | 1.332 | 1.522 | 1.728 | 1.95 |
| Damage sau (× .62) | 0.45 | 0.8 | 1.15 | 1.6 | 2.2 | 3.0 |
| Mật độ trước | 1 | 1.12 | 1.24 | 1.36 | 1.48 | 1.6 |
| Mật độ sau | 1 | 1.05 | 1.1 | 1.15 | 1.2 | 1.25 |

  Mật độ tăng ít hơn vì nhiều quái hơn = nhiều vàng hơn (bot giàu lên ở độ khó cao). `ModeBalanceChecks` cập nhật khẳng định "Peaceful base unchanged" thành "Peaceful là lớp nhẹ nhất" (HP ≤ 1, damage ≤ .62, mật độ = 1); thang tăng dần và trần HP ≤ 3 ở Beyond the Lamp giữ nguyên.
- **Silent Alarm** (Mute Clock): bot cũ chất 3–8 bản (mất 1 HP/s mỗi bản) và chết ở wave dài ở mọi độ khó. Nay Unique (1 bản), phần chỉ số về mục tiêu Mythic: +30% tốc đánh, +25% tốc chạy (trước +45% / +40%).
- **Bot đo** (`BalanceAudit`): tham số `-balance-seeds=N`, `BalanceAudit.Sim3` = 3 seed; trừ giá trị cho cơ chế tự gây hại (Silent Alarm, Split Kernel, Urchin Ball, Potato, Liar Mask, Rage); không trả altar xuống dưới 70% máu tối đa trong một shop. Đây là sửa cách đo, không phải luật game.

### Kết quả từng vòng (108 campaign, % thắng bot theo độ khó 0–5)

| Vòng | Thay đổi | Peaceful | Wandering | Restless | Nightmare | Sleepless | Beyond | Tổng |
|---|---|---:|---:|---:|---:|---:|---:|---:|
| Mục tiêu | | ~90% | ~80% | ~65% | ~50% | ~40% | 25–35% | |
| 1 (`Round2_1`) | kinh tế mới + altar %, độ khó cũ | 44% | 61% | 56% | 39% | 56% | 39% | 49% |
| 2 (`Round2_2`) | bảng độ khó v1 | 44% | 50% | 44% | 44% | 28% | 39% | 42% |
| 3 (`Round2_3`) | + Silent Alarm Unique, bot tính chi phí cơ chế | 50% | 56% | 50% | 44% | 33% | 44% | 46% |
| 4 (`Round2_4`, cuối) | bảng độ khó v2 (trên) + bot giữ ≥70% máu ở altar | 44% | 61% | 50% | 61% | 39% | 39% | 49% |

Mỗi ô 18 campaign (6 hero × 3 seed: 161026, 77138, 90210). Bảng số chi tiết: `PreviewExports/Balance/Round2_*/Sim_Summary.md`.

**Theo hero, vòng cuối (thắng / 18, gộp mọi độ khó):** Postmaster 18, Glass Dice 15, Lantern Knight 12, Tide Singer 3, Drowsy Seer 3, Comet Runner 2. Ở mọi vòng, Postmaster và Glass Dice thắng ≥ 14/18 ở **mọi** độ khó; Tide Singer, Drowsy Seer, Comet Runner thua phần lớn **ngay cả Peaceful** (damage địch ×0.45, HP ×0.7 ở vòng cuối) — chết ở wave 13–19. Vì vậy tỷ lệ thắng theo độ khó gần như bằng tỷ lệ hero mạnh trong mẫu; hệ số độ khó tạo được độ dốc ở phía trên (Sleepless/Beyond 39%, Comet Runner chết ở boss wave 5 từ Sleepless) nhưng **không thể** đưa Peaceful lên ~90% khi một nửa số hero thua ở mọi mức. Đã dừng ở giới hạn 2–3 vòng chỉnh độ khó.

**Kinh tế, vòng cuối** (mục tiêu ví ÷ giá offer 5–6 ở wave 11–19, ~6–7 lần mua mỗi shop cuối trận):

| Wave | Ví ÷ giá offer (round 1 → round 2) | Item / shop | Mua (item + companion) / shop | Reroll / shop |
|---|---|---:|---:|---:|
| 1–5 | 6.2 → 5.7 (không đổi luật) | 3.9 | 5.8 | 1.1 |
| 6–10 | 8.9 → 7.4 | 6.8 | 9.1 | 2.0 |
| 11–15 | 8.4 → 5.5 | 6.6 | 8.0 | 1.9 |
| 16–19 | 7.8 → 5.2 | 6.6 | 7.3 | 2.0 |

Đạt mục tiêu ví ÷ giá (5.5 / 5.2) và ~6.6 item mỗi shop cuối trận; tính cả companion là 7–8 lần mua.

**HP địch (compile / gốc loài) sau round 2:** Peaceful ×0.95 (w1) → ×21.7 (w20); Beyond the Lamp ×1.93 → ×91.5; mỗi bậc ×1.28–1.43 ở w20; bước wave lớn nhất ×1.26–1.31 (đường cong wave không đổi).

### Kiểm tra round 2

- Đạt: `ContentExpansionChecks.Run` CONTENT_EXPANSION_CHECKS_PASSED 388 · `ShopDescriptionChecks.Run` SHOP_DESCRIPTION_PASS 2583 · `ModeBalanceChecks.Run` MODE_BALANCE_CHECKS 4229 PASS · `PacingChecks.RunBatch` PACING_CHECKS PASS · `RosterCapAndBondChecks.Run` ROSTER15_PASS 1149 · `HeroIdentityChecks.Run` HERO_IDENTITY_PASS 2982 · `K26RelicChecks.Run` K26 relic checks PASS 156 · `K32Checks.Run` ARCANE_K32_CHECKS_PASSED 29 · `K32AltarChecks.Run` ARCANE_K32_ALTAR_CHECKS_PASSED 14 · `MarketPolishChecks.Run` MARKET_POLISH_CHECKS_PASSED 496 · `BalanceAudit.Run` BALANCE_AUDIT_PASSED (7 cờ, Silent Alarm không còn vượt) · `BalanceAudit.Sim3` BALANCE_SIM_PASSED runs=108 (4 lần).
- Reroll: hệ số giá shop nay nhân **sau** các giảm trừ (Ink Feather −2, talent Mary −2, Nicole/Belle), nên món giảm reroll vẫn chạm sàn 1 vàng ở mọi wave (K26 "Reroll reduction respects one-gold minimum" hỏng ở bản đầu round 2, nay đạt).
- Check cập nhật: `ModeBalanceChecks` ("Peaceful là lớp nhẹ nhất" thay cho "Peaceful base unchanged"); `K26RelicChecks` (máu Orchard Fizz đọc từ dữ liệu thay vì +20); `MarketPolishChecks` (giá altar so với `RunSession.SpecialItemPrice`). `MarketPolishChecks` còn hai chỗ đã lỗi thời **trước** lượt này và được sửa cho khớp runtime: giá item vàng thiếu hệ số ×0.85 (thêm vào RunSession ở e19d1cbc, 18/10) và giá companion thiếu hệ số veteran/sao.
- Không chạy lại `ActOneStoryChecks.RunWide` ở round 2 (lần chạy round 1 sau vật lý va chạm: các suite của lượt này đạt; 6 FAIL thuộc thay đổi cận chiến/vật lý của chủ dự án và khóa file).

### Việc còn mở sau round 2

- Cần cân **hero** trước khi chỉnh độ khó tiếp: Tide Singer, Drowsy Seer, Comet Runner thua 15–16/18 kể cả Peaceful; Postmaster 18/18, Glass Dice 15/18 thắng cả Beyond the Lamp. Có thể một phần do bot (K13 steering) chơi kém với các hero này — nên kiểm bằng `HeroMechanicBalance` hoặc chơi thử.
- Peaceful hiện dễ hơn nhiều so với trước (damage địch ×0.45 lúc đủ ramp). Nếu chơi thử thấy quá dễ với hero mạnh, nâng lại `DamageFactor[0]` (0.6 ở vòng 2 cho cùng kết quả bot).

## 8. Round 3 — hero (19/10/2026)

Mục tiêu (chủ dự án): mỗi hero thắng bot ~55–80% ở Wandering và 30–60% ở Nightmare, rồi đo lại độ dốc 108 campaign. Đo: `BalanceAudit.SweepHeroes` — cả **31 hero có trong catalog** (5 hero mới chưa vào catalog) × Wandering/Nightmare × 3 seed = 186 campaign mỗi lượt; `BalanceAudit.SweepDiagnose` ghi theo wave: tỷ lệ damage của hero, số lần kit/biến hình/thức tỉnh, damage nhận, HP thấp nhất, số bạn, số kỹ năng hấp thụ, nguồn damage cuối. Dữ liệu: `PreviewExports/Balance/Round3_*`.

### Chẩn đoán

- **Bot**: Penny Broker không bao giờ đặt đơn hàng tương lai → bot nay gọi `PlaceHeroOrder` mỗi shop. Hero hấp thụ (Archive Curator = "Wise", Lone Jelly = "Capped Gallant") biến bạn mua thành kỹ năng; chỉ ba kỹ năng gần nhất được dùng làm "memory organ", và **mỗi lần Wise hấp thụ tăng 4% máu địch** (`OctoberRun`). Bot cũ mua 15–20 bạn cho Archive Curator → địch +60–80% máu → chết ở boss wave 5. Bot nay chỉ mua đến khi có ba kỹ năng khác nhau. Riêng sửa này: Archive Curator Wandering 0/3 → 3/3 (lượt C).
- **Kit có chạy trong sim**: mọi hero yếu đều có kit cast, biến hình và thức tỉnh mỗi wave (Diagnose_Waves.csv); không thấy kit "chết".
- **Cách thua**: mọi lượt thua là hero chết vì bị bầy quái chạm (4–20 damage/đòn) khi đội không dọn kịp — boss wave 5/10 hoặc wave 14–18.
- **Hero mạnh (6/6)**: Postmaster, Root Oracle, Glass Dice, Mirror Keeper, China Marionette, Soap Skater, Button Cavalier, Hourglass Stag. Giảm máu ×0.7 và power 3 → 2.3 **không đổi kết quả nào** (lượt B): sức mạnh nằm ở đội hình/pool bạn, không ở chỉ số riêng của hero. Theo chỉ đạo, không đuổi theo nhóm này; các mức giảm đã hoàn lại.
- Tăng thô HP/power (lượt A/B: power tới 9, máu ×2) gần như không cứu hero yếu và sẽ vô lý với người chơi → đã thu về trong giới hạn power ≤ ~5 (Lone Jelly giữ 6.5 vốn có), sức sống ≤ 1.5.

Chẩn đoán từng hero yếu nằm ở cột cuối bảng kết quả bên dưới (Diagnose_Waves.csv: Wandering, 10 hero × 2 seed).

### Thay đổi hero (số cuối)

Đòn bẩy: `PlayerBuildBalance` (hero power, hệ số máu gốc "vitality" — áp vào máu gốc trước item/bond, không builder nào ghi đè), **cơ chế riêng** của hero và **nội tại cá nhân** (H) trong `HeroKits`. **Không đổi nội tại đội (T)** — buff cả đội/bạn, bond — giữ nguyên.

| Hero | Power | Vitality (máu gốc) | Cơ chế / nội tại cá nhân (trước → sau) | Lý do |
|---|---|---|---|---|
| Archive Curator | 3 → 4 | 1 → 1.3 (85 → 110) | Con rối Living Exhibit .34 → .5 W/đòn; "+5% → +15% damage lên loài đã ghi" | Sau ba lần hấp thụ gần như đánh một mình |
| Lone Jelly | 6.5 (giữ) | 1 → 1.5 (225 → 338) | Vỏ ký ức giảm damage 5% → 15%; hồi 1 HP mỗi 8 → 5 đòn | Hero solo, chết ở wave 8–9 vì bầy |
| Tide Singer | 3 → 3.8 | 1 → 1.5 | High Tide hồi 1 → 2 HP/giây | Chết wave 15–19 dù đủ đội |
| Drowsy Seer | 3 → 4 | 1 → 1.5 | Đứng yên 0.5s: đòn kế +20% → +30% | Chết wave 15 dù đủ đội |
| Ivory Sovereign | 3 → 4 | (máu cố định) | Máu hoàng gia 12 → 16 (mỗi đòn vẫn trừ 1) | 0/6, chết vì bẫy/bầy |
| Comet Runner | 3 → 3.6 | 1 → 1.45 | Chạy 2s liên tục: +15% → +25% tốc chạy | Chết ở boss wave 5 từ Sleepless |
| Patchwork Host | 4.5 → 5 | 1 → 1.45 | — | 1/6 |
| Night Maw, Petal Citadel, Storm Fencer, Supper Mortar | 3 → 3.3 | 1 → 1.3 | — | 2/6 |
| Verdant Matron, Copper Cauldron, Moon Prowler | 3 | 1 → 1.15 | — | 3/6 |

Mô tả kit/hero cập nhật theo số mới (HeroKits, HeroIdentity, OctoberContent, asset Ivory Sovereign).

### Kết quả theo hero (thắng / 3 mỗi ô)

"Trước" = `Round3_Before` (trước mọi thay đổi round 3); "sau" = `Round3_HeroesD` (số hero cuối, bot sửa, cùng luật tuyển quân mới của chủ dự án: tối đa 2 lần mua bạn thường mỗi wave, loài mới mỗi hai wave — số bạn mua mỗi shop 1.57 → 1.32). Ba seed mỗi ô: nhiễu ±1 là bình thường.

| Hero | Wandering trước | Nightmare trước | Wandering sau | Nightmare sau | Chẩn đoán |
|---|---:|---:|---:|---:|---|
| LanternKnight | 1/3 | 3/3 | 2/3 | 1/3 |  |
| Postmaster | 3/3 | 3/3 | 3/3 | 3/3 |  |
| DrowsySeer | 1/3 | 0/3 | 0/3 | 1/3 | Đủ 16 bạn, kit chạy; chết wave 15 (ShadowPool / bầy chạm). |
| RootOracle | 3/3 | 3/3 | 3/3 | 3/3 |  |
| LoneJelly | 0/3 | 0/3 | 2/3 | 1/3 | Solo (Capped Gallant), damage hero ~70% đội; chết wave 8–9 vì bầy chạm; kit chạy. |
| GlassDice | 3/3 | 3/3 | 3/3 | 2/3 |  |
| TideSinger | 1/3 | 1/3 | 1/3 | 0/3 | Đủ 15 bạn, kit chạy 16–22 lần/run; chết wave 15–19 vì bầy chạm khi đội không dọn kịp. |
| CometRunner | 0/3 | 1/3 | 3/3 | 1/3 | Chết ở boss wave 5 từ Sleepless; ở độ khó thấp thắng. |
| MirrorKeeper | 3/3 | 3/3 | 3/3 | 2/3 |  |
| TreasureAcorn | 3/3 | 2/3 | 3/3 | 1/3 |  |
| VerdantMatron | 1/3 | 2/3 | 3/3 | 0/3 |  |
| CopperCauldron | 1/3 | 2/3 | 2/3 | 1/3 |  |
| ChinaMarionette | 3/3 | 3/3 | 3/3 | 2/3 |  |
| SoapSkater | 3/3 | 3/3 | 3/3 | 2/3 |  |
| VelvetRegent | 3/3 | 2/3 | 2/3 | 2/3 |  |
| ButtonCavalier | 3/3 | 3/3 | 3/3 | 3/3 |  |
| NightMaw | 1/3 | 1/3 | 2/3 | 0/3 | Kit (nuốt quái, hồi máu) chạy; thua rải rác. |
| ThornTreasurer | 3/3 | 1/3 | 3/3 | 3/3 |  |
| CitrusMarshal | 3/3 | 1/3 | 2/3 | 3/3 |  |
| IvorySovereign | 0/3 | 0/3 | 1/3 | 0/3 | Máu hoàng gia 12, mỗi đòn −1: chết vì bẫy (FallingProp, Curtain) và đòn lặt vặt, không phải đòn lớn. |
| PetalCitadel | 2/3 | 0/3 | 1/3 | 0/3 |  |
| PatchworkHost | 0/3 | 1/3 | 1/3 | 1/3 | Bạn diễn tự chọn (Automatic) hoạt động; thua rải rác wave 13–18. |
| ArchiveCurator | 0/3 | 0/3 | 1/3 | 0/3 | Hấp thụ (Wise): bot cũ mua 15–20 bạn → địch +60–80% máu; sau 3 kỹ năng gần như đánh một mình, chết ở boss wave 5 vì bầy; kit/biến hình/thức tỉnh có chạy mỗi wave. |
| PennyBroker | 2/3 | 2/3 | 3/3 | 2/3 | Bot cũ không bao giờ đặt đơn hàng tương lai → nay đặt mỗi shop. |
| StormFencer | 2/3 | 0/3 | 2/3 | 2/3 |  |
| TinVirtuoso | 3/3 | 2/3 | 2/3 | 3/3 |  |
| MoonProwler | 2/3 | 1/3 | 1/3 | 2/3 |  |
| SupperMortar | 1/3 | 1/3 | 3/3 | 2/3 |  |
| LanternWeaver | 3/3 | 1/3 | 2/3 | 2/3 |  |
| HourglassStag | 3/3 | 3/3 | 3/3 | 2/3 |  |
| RainArchivist | 2/3 | 2/3 | 3/3 | 2/3 |  |
| **Tổng (93 mỗi cột)** | **59** (63%) | **50** (53%) | **69** (74%) | **49** (52%) | |

Tổng thể đạt dải mục tiêu (Wandering 74%, Nightmare 52%). Còn ngoài dải (1/6): Drowsy Seer, Tide Singer, Ivory Sovereign, Archive Curator, Petal Citadel — đều chết vì bị bầy chạm muộn trong trận, không còn lỗi bot/kit nào tìm thấy; tăng tiếp cần thay đổi cơ chế lớn hơn (ví dụ đẩy lùi/khiên khi bị vây) và nên chơi thử trước. Nhóm 6/6 (Postmaster, Root Oracle, Button Cavalier…) để nguyên theo chỉ đạo.

### Độ dốc độ khó sau round 3 (108 campaign, 6 hero đại diện)

Cùng 6 hero, 3 seed, 108 campaign. "Round 3" gồm: hero cuối, luật tuyển quân mới, xu ×0.75, boss −22% damage, quái thường +10% máu (`Round3_Econ1`).

| Độ khó | Mục tiêu | Round 2 cuối (`Round2_4`) | Round 3 (`Round3_Econ1`) |
|---|---:|---:|---:|
| Peaceful | ~90% | 44% | 50% |
| Wandering | ~80% | 61% | 67% |
| Restless | ~65% | 50% | 50% |
| Nightmare | ~50% | 61% | 33% |
| Sleepless | ~40% | 39% | 33% |
| Beyond the Lamp | 25–35% | 39% | 22% |

Độ dốc nay giảm dần từ Wandering tới Beyond the Lamp (67 → 50 → 33 → 33 → 22). Peaceful vẫn ~50% vì Drowsy Seer và Tide Singer (2/6 hero trong mẫu đo độ khó) thua cả ở Peaceful; không chỉnh thêm bảng độ khó ở round này (thay đổi kinh tế/boss/quái thường cùng lúc đã dịch cả đường cong).

### Kinh tế: xu ×0.75 (yêu cầu chơi thử)

`CarnivalBalance.CoinGoldValue = 0.75` → `CheckpointCatalog.CoinValueScale`: mỗi xu nhặt được cộng 0.75 vàng, phần lẻ cộng dồn giữa các lần nhặt (`CombatSimulation`). Số xu rơi, hình ảnh và cách nhặt không đổi; không tăng giá shop; thu nhập/lãi giữ nguyên.

| Wave | Ví ÷ giá offer: Round 2 → Round 3 | Mua / shop (item + bạn) | Reroll / shop |
|---|---|---|---|
| 1–5 | 5.7 → 4.6 | 5.8 → 4.8 | 1.1 → 0.7 |
| 6–10 | 7.4 → 5.7 | 9.1 → 7.6 | 2.0 → 1.5 |
| 11–15 | 5.5 → 4.6 | 8.0 → 7.1 | 1.9 → 1.3 |
| 16–19 | 5.2 → 3.9 | 7.3 → 6.1 | 2.0 → 1.2 |

Mục tiêu 4–4.5 ở wave 11–19: 4.6 / 3.9, quanh mục tiêu. Shop đầu vẫn mua được ~4–5 món (gồm cả bạn).

### Boss vs wave thường (yêu cầu chơi thử)

- Boss: `OctoberBalance.BossDamageRelief` 0.9 → 0.7 (−22% mọi damage boss gây ra: contact, đạn, signature, encore, organ, bẫy carnival, trademark — áp ở một điểm chung trong `CombatSimulation`).
- Quái thường: `OctoberBalance.NormalEnemyHealth` = 1.1 (+10% máu, chỉ quái không phải boss), áp cùng chỗ relief máu chung (`RunSession.ApplyEnemyDifficultyRelief`). `BossReliefChecks` cập nhật cho hệ số này.

| Mẫu | Máu mất / max HP mỗi wave (chết = 1): boss / thường / tỷ lệ | Tỷ lệ chết mỗi wave: boss / thường / tỷ lệ |
|---|---|---|
| Trước (Diagnose, Wandering, 10 hero × 2 seed) | 0.22 / 0.23 / 0.96× | 6.8% / 2.3% / 3.0× |
| Sau (`Round3_Econ1`, 108 campaign) | 0.44 / 0.27 / 1.60× | 9.1% / 2.0% / 4.5× |

Hai mẫu khác nhau (trước chỉ Wandering; sau gồm cả 6 độ khó và kinh tế xu ×0.75 nghèo hơn), nên không so trực tiếp được từng số. Theo máu mất trung bình, wave boss nay ~1.6× wave thường (trong mục tiêu 1.5–2×); nhưng **tỷ lệ chết** ở wave boss vẫn ~4.5× — chủ yếu hero yếu chết ở boss wave 5 khi đội chưa đủ. Nếu chơi thử vẫn thấy boss quá gắt, bước tiếp theo là giảm máu boss wave 5 (không phải damage).

### Kiểm tra round 3

- Đạt (sau mọi thay đổi round 3): `HeroIdentityChecks.Run` HERO_IDENTITY_PASS 2982 · `HeroAwakeningChecks.Run` HERO_AWAKENING_CHECKS_PASSED 288 · `ContentExpansionChecks.Run` CONTENT_EXPANSION_CHECKS_PASSED 388 · `ModeBalanceChecks.Run` MODE_BALANCE_CHECKS 4229 PASS · `PacingChecks.RunBatch` PACING_CHECKS PASS · `ShopDescriptionChecks.Run` SHOP_DESCRIPTION_PASS 2583 · `K32Checks.Run` ARCANE_K32_CHECKS_PASSED 29 (kèm K2 131, K0/K1 27, checkpoint 17, squad 12, K3 transaction 36).
- `BossReliefChecks.Run`: BOSS_RELIEF_CHECKS 125 PASS (kèm SHOP_FLOW_CHECKS 17 PASS)
- Check cập nhật: `K2Checks` (vàng nhặt được = giá trị xu × `CoinValueScale`); `BossReliefChecks` (quái thường × `NormalEnemyHealth`; damage boss × `BossDamageRelief` thay cho hằng .9; ba khẳng định máu boss Last Customer / Keeper / Bedside còn ghi hằng .75 × .6 từ trước 18/10 — relief đã là .95 — nay đọc hằng số thật).
- Sửa lỗi phát hiện khi kiểm: phần lẻ vàng của xu không được xóa khi `CombatSimulation.Start()` bắt đầu trận mới → nay reset cùng `Gold`.
- `HeroMechanicBalance` là công cụ đo, không phải bộ kiểm; không chạy lại (dài, ~560 trận) — số liệu round 3 lấy từ `SweepHeroes`/`SweepDiagnose`.
