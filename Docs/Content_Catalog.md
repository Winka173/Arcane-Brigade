# Danh mục nội dung — Arcane Brigade

Cập nhật 18/10/2026 · theo thiết kế **mở rộng nội dung bản 5** đã duyệt ([Content_Expansion_v5.md](Content_Expansion_v5.md)). Tệp này chỉ để tra cứu: liệt kê **mọi** hero, bond, companion, vũ khí Arsenal, item và họ item, gồm cả nội dung mới chưa vào game.

**Cách đọc**

- Cột / nhãn **Mới** = nội dung mới của đợt mở rộng, **chưa có trong asset Unity** (vào game khi có art). Không có nhãn = đang có trong `RunCatalog.asset`.
- Tên trong game giữ nguyên tiếng Anh; Id (tên trong code) ghi bằng `code` khi khác tên hiển thị.
- Bậc độ hiếm: Common · Uncommon · Rare · Mythic · **Prismatic** (trong code là `Rainbow`). Bậc Crimson đã bỏ.
- Mô tả hiệu ứng: nội dung đã có lấy từ dữ liệu / code trong game (dịch sang tiếng Việt); nội dung mới lấy từ tài liệu thiết kế, ghi nguồn (v3 = [Content_Expansion_Proposal.md](Content_Expansion_Proposal.md), v5 = Content_Expansion_v5.md). Thiếu mô tả ghi *(chưa có mô tả)*.
- Nguồn dữ liệu: `Tools/Content/expansion_design.py` (bond, bậc, bộ cố định), `Tools/Content/expansion.json`, `Assets/ArcaneBrigade/Resources/RunCatalog.asset`, `Runtime/Data/HeroKits.cs`, `Runtime/Core/HeroGauge.cs`, `Runtime/Data/HeroLoadout.cs`, `Editor/ArsenalSpecs.cs`, `Runtime/Core/CompanionSkills.cs`, `Runtime/Core/OctoberCompanions.cs`, `Runtime/UI/ContentDescriptions.cs`.

## Mục lục

1. Tóm tắt
2. Hero (36)
3. Bond (33)
4. Companion (94)
5. Vũ khí Arsenal (60)
6. Item (200)
7. Họ item (18)
8. Phụ lục: bộ companion cố định của từng hero
9. Phụ lục: vũ khí ngoài Arsenal
10. Phụ lục: liên kết build
11. Ghi chú / chênh lệch

## 1. Tóm tắt

### 1.1 Số lượng

| Loại | Tổng (mục tiêu v5) | Đang trong game | Mới, chưa vào game |
|---|---:|---:|---:|
| Hero | 36 (36) | 31 | 5 |
| Companion | 94 (94) | 66 | 28 |
| Bond | 33 (33) | 33 (đã có đủ trong RunCatalog) | 0 (6 bond mới đã có dữ liệu, chờ icon art) |
| Vũ khí Arsenal | 60 (60) | 47 | 13 |
| Item | 200 (200) | 170 | 30 |
| Họ item | 18 (18) | — | — |

### 1.2 Theo độ hiếm

| Bậc | Companion | trong đó mới | Item | trong đó mới |
|---|---:|---:|---:|---:|
| Common | 31 | 6 | 43 | 3 |
| Uncommon | 27 | 7 | 66 | 5 |
| Rare | 17 | 7 | 44 | 6 |
| Mythic | 11 | 3 | 38 | 11 |
| Prismatic | 8 | 5 | 9 | 5 |
| **Tổng** | **94** | **28** | **200** | **30** |

Companion theo bond chủ đề: Hearth 12 · Archive 13 · Clock 12 · Frost 11 · Orchard 11 · Astral 11 · Tidal 13 · Oddity 11.

### 1.3 Luật bộ cố định (fixed pool)

- Mỗi hero có **2 bond chủ đề** (Patchwork Host có 3) + **1 bond phụ đặc trưng** (signature; mỗi bond phụ đặc trưng tối đa 2 hero) + **1 họ item** (tối đa 3 hero mỗi họ).
- Bộ companion của hero = mọi companion thuộc 2 bond chủ đề + toàn bộ thành viên bond đặc trưng (trừ companion riêng của hero khác) + companion riêng của chính hero (Grand Sprout, Little Sprout → Verdant Matron; Pinned Beetle, Fossil Armadillo, Specimen Slime → Archive Curator). Cỡ bộ 18–28 (Patchwork Host đến 36).
- Bộ item của hero = mọi item Common (dùng chung) + item thuộc 2 chủ đề của hero + item thuộc họ item của hero.
- **Luật đối thủ (rival):** hai hero cùng cặp chủ đề thì mỗi bên **không thấy companion Common–Rare thuộc bond đặc trưng của bên kia**; hai bộ phải cách nhau ≥6 companion và khác họ item.
- Mỗi companion có đúng 1 bond chủ đề + 1 bond phụ. Companion **Rare** tính **+1 thành viên** cho bond phụ của nó **khi bond đó đã kích hoạt** (thêm cấp, không tự bật bond).
- Companion **Mythic / Prismatic** luôn cầm vũ khí riêng (vũ khí khóa) và có kỹ năng riêng; vũ khí khóa bị loại khỏi vòng quay của companion khác.
- Prismatic không bán trong shop: đến từ quà mảnh lăng kính (3 mảnh → chọn 1 trong 3), chọn từ các Prismatic trong bộ của hero.
- Hero sống bằng bond (Lantern Knight, Velvet Regent, Verdant Matron và 5 hero mới) có nội tại "bond đặc trưng tính thêm 1 cấp"; các hero khác có nội tại riêng.
- Thanh hero (gauge): mọi thanh có dòng nạp nền (~35s đầy nếu không làm gì) và trần nạp (~18s nhanh nhất); 3 nấc đầy thì hero tự biến hình ~6s rồi nghỉ 3s. Cột "Nạp từ" ghi điểm mỗi sự kiện theo `HeroGauge.cs` (10 điểm = 1 nấc).

## 2. Hero (36)

### 2.1 Bảng tổng

| # | Hero | Mới | Bond chủ đề | Bond đặc trưng | Họ item | Vũ khí của hero | Companion khởi đầu | Bộ companion (đang có) | Bộ item | Bond phụ kích hoạt được trong bộ | Rival (bị ẩn) |
|---:|---|:-:|---|---|---|---|---|---:|---:|---|---|
| 1 | Lantern Knight |  | Hearth (Bếp Ấm) · Clock (Bộ Máy) | Lantern Guild | Đội | KeyCleaver | Kettle Bat | 23 (15) | 99 | Clockwork Union, Lantern Guild, Morning Bells, Prizefighters, Static Club, Sweet Tooth, Velvet Court | — |
| 2 | Postmaster |  | Archive (Thư Viện) · Tidal (Thủy Triều) | Paper Wings | Đánh dấu | MailboxLauncher | Bookmark Golem | 26 (17) | 83 | Crossing Knights, Hat Society, Lucky Leaves, Paper Wings, Rain Script, Runic School, Sea Shanty, Velvet Court, Wild Stitch | — |
| 3 | Drowsy Seer |  | Astral (Tinh Tú) · Hearth (Bếp Ấm) | Hat Society | Shop | MoonseedOrb | Candle Snail | 24 (17) | 87 | Clockwork Union, Hat Society, Lantern Guild, Longbow, Morning Bells, Prizefighters, Quicksoles, Sweet Tooth | — |
| 4 | The Root Oracle `RootOracle` |  | Orchard (Vườn Cây) · Frost (Sương Giá) | Rootkin | Sống sót | BambooVase | Mushroom Librarian | 21 (15) | 94 | Little Horns, Masked Carnival, Rootkin, Springstep, Sweet Tooth | — |
| 5 | The Lone Jelly `LoneJelly` |  | Oddity (Kỳ Lạ) · Tidal (Thủy Triều) | Crossing Knights | Merge | InkyOctopus | Jelly Swordsman | 24 (17) | 85 | Big Top, Crossing Knights, Lucky Leaves, Masked Carnival, Rain Script, Sea Shanty, Unbroken | — |
| 6 | Lady Loaded Dice `GlassDice` |  | Oddity (Kỳ Lạ) · Orchard (Vườn Cây) | Lucky Leaves | May rủi | GlitchToybox | Dice Crab | 24 (19) | 86 | Big Top, Little Horns, Lucky Leaves, Masked Carnival, Rootkin, Springstep, Unbroken | — |
| 7 | The Tide Singer `TideSinger` |  | Tidal (Thủy Triều) · Astral (Tinh Tú) | Sea Shanty | Đồ uống | RibbonHarp | Anchor Snail | 23 (16) | 90 | Crossing Knights, Hat Society, Longbow, Lucky Leaves, Quicksoles, Rain Script, Sea Shanty, Static Club | Paper Wings |
| 8 | The Comet Courier `CometRunner` |  | Astral (Tinh Tú) · Clock (Bộ Máy) | Quicksoles | Tốc độ | ClockCrescent | Comet Kite | 19 (16) | 84 | Clockwork Union, Hat Society, Longbow, Morning Bells, Quicksoles | Static Club |
| 9 | The Mirror Keeper `MirrorKeeper` |  | Frost (Sương Giá) · Oddity (Kỳ Lạ) | Unbroken | Sống sót | SnowGlobeOrb | Violet Remnant | 21 (16) | 101 | Big Top, Masked Carnival, Quicksoles, Sweet Tooth, Unbroken | Wild Stitch |
| 10 | The Treasure Clerk `TreasureAcorn` |  | Orchard (Vườn Cây) · Archive (Thư Viện) | Lucky Leaves | Rương | AcornSling | Brass Acorn | 24 (18) | 88 | Hat Society, Lucky Leaves, Paper Wings, Rootkin, Runic School, Springstep, Velvet Court, Wild Stitch | Little Horns |
| 11 | Verdant Matron |  | Orchard (Vườn Cây) · Hearth (Bếp Ấm) | Jade Nursery | Merge | LeafCrossbow | Little Sprout | 23 (17) | 83 | Clockwork Union, Jade Nursery, Lantern Guild, Little Horns, Morning Bells, Prizefighters, Rootkin, Sweet Tooth | Springstep |
| 12 | Copper Cauldron |  | Hearth (Bếp Ấm) · Oddity (Kỳ Lạ) | Unbroken | Va chạm | FireExtinguisher | Teacup Cricket | 25 (19) | 87 | Big Top, Clockwork Union, Lantern Guild, Masked Carnival, Prizefighters, Sweet Tooth, Unbroken | — |
| 13 | China Marionette |  | Frost (Sương Giá) · Oddity (Kỳ Lạ) | Wild Stitch | Vàng | NeedleSinger | Ribbon Spider | 23 (18) | 84 | Big Top, Masked Carnival, Quicksoles, Sweet Tooth, Unbroken, Wild Stitch | Unbroken |
| 14 | Soap Skater |  | Tidal (Thủy Triều) · Frost (Sương Giá) | Quicksoles | Đóng băng | BubbleWand | Sail Squid | 23 (17) | 85 | Crossing Knights, Lucky Leaves, Quicksoles, Sea Shanty, Sweet Tooth, Unbroken | Rain Script |
| 15 | Velvet Regent |  | Archive (Thư Viện) · Clock (Bộ Máy) | Velvet Court | Đội | StarforgeClaymore | Button Beetle | 23 (17) | 97 | Clockwork Union, Hat Society, Morning Bells, Paper Wings, Runic School, Static Club, Velvet Court, Wild Stitch | — |
| 16 | Button Cavalier |  | Clock (Bộ Máy) · Oddity (Kỳ Lạ) | Crossing Knights | Cận chiến | CorkscrewRapier | Tin Tick | 23 (18) | 88 | Big Top, Clockwork Union, Crossing Knights, Masked Carnival, Morning Bells, Static Club, Unbroken | Morning Bells |
| 17 | Night Maw |  | Oddity (Kỳ Lạ) · Astral (Tinh Tú) | Prizefighters | Đồ ăn | DrumstickClub | Mask Pepper | 24 (19) | 93 | Big Top, Hat Society, Longbow, Masked Carnival, Prizefighters, Quicksoles, Unbroken, Wild Stitch | — |
| 18 | Thorn Treasurer |  | Orchard (Vườn Cây) · Archive (Thư Viện) | Little Horns | Vàng | GumballGatling | Seed Hedgehog | 23 (16) | 83 | Hat Society, Little Horns, Paper Wings, Rootkin, Runic School, Springstep, Velvet Court, Wild Stitch | Lucky Leaves |
| 19 | Citrus Marshal |  | Orchard (Vườn Cây) · Hearth (Bếp Ấm) | Springstep | Va chạm | SignpostSlammer | Citrus Toad | 23 (17) | 86 | Clockwork Union, Lantern Guild, Little Horns, Morning Bells, Prizefighters, Rootkin, Springstep, Sweet Tooth | Jade Nursery |
| 20 | Ivory Sovereign |  | Archive (Thư Viện) · Frost (Sương Giá) | Hat Society | Shop | DreamStamp | Ice Cream Owl | 24 (18) | 85 | Hat Society, Paper Wings, Runic School, Sweet Tooth, Velvet Court, Wild Stitch | — |
| 21 | Petal Citadel |  | Orchard (Vườn Cây) · Astral (Tinh Tú) | Longbow | Tầm xa | RoseBouquet | Pear Moth | 22 (17) | 94 | Hat Society, Little Horns, Longbow, Morning Bells, Quicksoles, Rootkin, Springstep | — |
| 22 | Patchwork Host |  | Hearth (Bếp Ấm) · Oddity (Kỳ Lạ) · Orchard (Vườn Cây) | Big Top | Đội | ConfettiBlunderbuss | Sock Cyclops | 35 (26) | 115 | Big Top, Clockwork Union, Lantern Guild, Little Horns, Lucky Leaves, Masked Carnival, Morning Bells, Prizefighters, Rootkin, Springstep, Sweet Tooth, Unbroken | — |
| 23 | Archive Curator |  | Clock (Bộ Máy) · Frost (Sương Giá) | Runic School | Theo loài đã hạ | SleepCamera | Fridge Wisp | 27 (16) | 82 | Clockwork Union, Exhibit Hall, Morning Bells, Runic School, Static Club, Sweet Tooth | — |
| 24 | Penny Broker |  | Archive (Thư Viện) · Hearth (Bếp Ấm) | Clockwork Union | Vàng | DetonatorBox | Mailbox Imp | 27 (19) | 86 | Clockwork Union, Hat Society, Lantern Guild, Paper Wings, Prizefighters, Runic School, Sweet Tooth, Velvet Court, Wild Stitch | — |
| 25 | Storm Fencer |  | Clock (Bộ Máy) · Astral (Tinh Tú) | Static Club | Chí mạng | PlasmaTrident | Spring Duck | 19 (13) | 90 | Clockwork Union, Hat Society, Longbow, Morning Bells, Static Club | Quicksoles |
| 26 | Tin Virtuoso |  | Clock (Bộ Máy) · Oddity (Kỳ Lạ) | Morning Bells | Chí mạng | MusicBox | Staple Mole | 23 (17) | 96 | Big Top, Clockwork Union, Masked Carnival, Morning Bells, Static Club, Unbroken | Crossing Knights |
| 27 | Moon Prowler |  | Astral (Tinh Tú) · Frost (Sương Giá) | Masked Carnival | Cận chiến | MoonScythe | Star Sock | 23 (18) | 89 | Hat Society, Longbow, Masked Carnival, Quicksoles, Sweet Tooth, Wild Stitch | — |
| 28 | Supper Mortar |  | Hearth (Bếp Ấm) · Tidal (Thủy Triều) | Sweet Tooth | Đồ ăn | TeapotPopgun | Bread Crab | 28 (18) | 88 | Clockwork Union, Crossing Knights, Lantern Guild, Lucky Leaves, Prizefighters, Rain Script, Sea Shanty, Sweet Tooth | — |
| 29 | Lantern Weaver |  | Astral (Tinh Tú) · Archive (Thư Viện) | Lantern Guild | Tầm xa | LanternStaff | Thimble Moth | 26 (20) | 92 | Hat Society, Lantern Guild, Longbow, Paper Wings, Quicksoles, Runic School, Velvet Court, Wild Stitch | — |
| 30 | Hourglass Stag |  | Clock (Bộ Máy) · Tidal (Thủy Triều) | Morning Bells | Trì hoãn | AlarmMortar | Dream Compass | 26 (17) | 83 | Clockwork Union, Crossing Knights, Lucky Leaves, Morning Bells, Rain Script, Sea Shanty, Static Club | — |
| 31 | Rain Archivist |  | Tidal (Thủy Triều) · Orchard (Vườn Cây) | Rain Script | Rương | SquirtTrio | Ribbon Otter | 25 (16) | 85 | Crossing Knights, Little Horns, Lucky Leaves, Rain Script, Rootkin, Sea Shanty, Springstep | — |
| 32 | Kite Admiral | **Mới** | Astral (Tinh Tú) · Tidal (Thủy Triều) | Paper Wings | Tầm xa | KiteSpool | Paper Plane Pigeon **Mới** | 23 (17) | 94 | Crossing Knights, Hat Society, Longbow, Lucky Leaves, Paper Wings, Quicksoles, Rain Script, Static Club | Sea Shanty |
| 33 | Sundae Duchess | **Mới** | Frost (Sương Giá) · Hearth (Bếp Ấm) | Sweet Tooth | Đóng băng | ScoopLauncher | Baker Mouse **Mới** | 22 (15) | 83 | Clockwork Union, Lantern Guild, Prizefighters, Sweet Tooth | — |
| 34 | Bonsai Clocksmith | **Mới** | Orchard (Vườn Cây) · Clock (Bộ Máy) | Clockwork Union | Merge | CuckooCannon | Top Beetle **Mới** | 23 (17) | 83 | Clockwork Union, Little Horns, Morning Bells, Rootkin, Springstep, Static Club | — |
| 35 | Juggling Jester | **Mới** | Oddity (Kỳ Lạ) · Archive (Thư Viện) | Big Top | Chí mạng | JugglingPins | Lens Owl **Mới** | 24 (19) | 94 | Big Top, Hat Society, Masked Carnival, Paper Wings, Runic School, Unbroken, Velvet Court, Wild Stitch | — |
| 36 | Bell Diver | **Mới** | Tidal (Thủy Triều) · Frost (Sương Giá) | Rain Script | Đánh dấu | SonarHarpoon | Raincoat Frog **Mới** | 22 (14) | 85 | Crossing Knights, Lucky Leaves, Rain Script, Sea Shanty, Sweet Tooth, Unbroken | Quicksoles |

"Bộ companion (đang có)": tổng theo thiết kế, trong ngoặc là số companion đã có trong game hôm nay. "Bộ item": số item trong bộ (Common dùng chung + 2 chủ đề + họ item), tính trên 200 item.

### 2.2 Thanh, biến hình và nội tại

#### 2.2.1 Lantern Knight

- **Thanh:** Đèn lồng. Nạp từ: đồng đội đánh gần hero ×0.5 + thời gian ×0.4. Nhanh hơn khi: —.
- **Biến hình — Lighthouse:** Đèn thành hải đăng: chùm sáng quét sân đánh thứ nó đi qua; đồng đội trong vùng sáng +50% tốc đánh; hero giảm 20% sát thương nhận.
- **Nội tại hero:** Có ít nhất 2 đồng đội ở gần: hero nhận ít hơn 10% sát thương.
- **Nội tại đội / shop / run:**
  - Lantern Guild tính thêm 1 cấp.
  - Đồng đội trong tầm 4 của hero +8% tốc đánh.
  - Tầm đánh đồng đội +5%.
  - Địch trong vùng đèn (tầm 4) nhận +5% sát thương.

#### 2.2.2 Postmaster

- **Thanh:** Giá thư 4 ô. Nạp từ: đòn trúng ×1. Nhanh hơn khi: Lá thư thứ 4 mỗi lượt luôn chí mạng.
- **Biến hình — Sorting Gatling:** Máy phân loại sáu nòng thay hộp thư: mỗi 0.2s bắn một bưu kiện tự dẫn, ưu tiên địch đã đóng tem; +40% tốc đánh.
- **Nội tại hero:** Đòn thường đóng tem tối đa 3 địch trong 6s; địch có tem nhận +10% sát thương từ hero.
- **Nội tại đội / shop / run:**
  - Thư bảo đảm: món đầu tiên mua ở mỗi shop rẻ hơn 20%.
  - Đồng đội +8% sát thương lên địch có tem.
  - Đạn đồng đội bay nhanh hơn 10%.
  - Địch có tem bị hạ: 15% rơi thêm 1 xu.

#### 2.2.3 Drowsy Seer

- **Thanh:** Gối mộng hai con mắt. Nạp từ: quãng chạy ×0.4 + thời gian ×0.4. Nhanh hơn khi: —.
- **Biến hình — Full Moon:** Ba giấc mơ của hero đi lại đường hero vừa đi (trễ 1, 2, 3 giây) và phép vào địch gần chúng; +30% tốc chạy, +40% tốc đánh.
- **Nội tại hero:** Đứng yên 0.5s thì đòn kế +20%.
- **Nội tại đội / shop / run:**
  - Trứng mộng: cứ 3 wave nhận thêm 1 trứng.
  - Thức giấc: đồng đội +15% tốc đánh trong 5s đầu mỗi wave.
  - Tiên tri: mỗi shop hiện thêm 1 món.
  - Đòn của hero làm địch buồn ngủ: chậm 10% trong 1s.

#### 2.2.4 The Root Oracle (`RootOracle`)

- **Thanh:** Chậu cây trong chuông kính. Nạp từ: thời gian ×0.4 + đòn trúng ×1 + ở gần địch ×0.4. Nhanh hơn khi: Ở gần địch ×1.5.
- **Biến hình — Living Tree:** Hero nhổ rễ đi bằng rễ: mỗi bước mọc gai, rễ đánh nhanh gấp đôi, +50% máu tối đa trong lúc biến hình.
- **Nội tại hero:** Rễ đỡ một viên đạn địch gần hero mỗi 4s. Đứng giữa rễ hồi 1 máu mỗi 3s.
- **Nội tại đội / shop / run:**
  - Rễ sâu: trần lãi +2.
  - Địch gần rễ bị chậm 15%.
  - Đồng đội +8% sát thương lên địch gần rễ.
  - Mỗi 5s, địch gần hero nhất bị rễ giữ 1s.

#### 2.2.5 The Lone Jelly (`LoneJelly`)

- **Thanh:** Khuôn thạch vạch chia đôi. Nạp từ: hạ địch ×1.5. Nhanh hơn khi: —.
- **Biến hình — Mitosis:** Hero tách làm ba: hai con thạch mang các cơ quan đã nhớ chiến đấu cạnh hero; +30% tốc đánh.
- **Nội tại hero:** Cơ quan đã nhớ: mỗi 8 đòn hồi 1 máu; đòn của hero làm chậm địch 10%. Vỏ đã nhớ: hero nhận ít hơn 5% sát thương.
- **Nội tại đội / shop / run:**
  - Mỗi lần merge trong run: hero +2% sát thương vĩnh viễn (tối đa 25%).
  - Hấp thụ một ký ức hoàn 2 vàng.
  - Đòn của hero làm chậm địch 10% trong 1s.

#### 2.2.6 Lady Loaded Dice (`GlassDice`)

- **Thanh:** 3 ô xúc xắc. Nạp từ: nhặt xu ×1 + đòn trúng ×1. Nhanh hơn khi: Mỗi nấc tung một xúc xắc; ba mặt giống nhau thì biến hình mạnh hơn.
- **Biến hình — Casino Table:** Ba xúc xắc tung mỗi giây: đôi thì giáng một đòn, ba mặt giống nhau thì trúng độc đắc; +25% chí mạng.
- **Nội tại hero:** Mặt xúc xắc kế tiếp là một buff nhỏ: sát thương, tốc đánh, chí mạng, tốc chạy, vàng hoặc hồi máu.
- **Nội tại đội / shop / run:**
  - Mỗi lượt đổi có 1/6 cơ hội miễn phí và 1/6 cơ hội ra hàng hiếm hơn.
  - Đồng đội +5% chí mạng.
  - Cuối mỗi wave tung một xúc xắc: ra 6 được 6 vàng.

#### 2.2.7 The Tide Singer (`TideSinger`)

- **Thanh:** Ống nước biển. Nạp từ: thời gian ×0.4 + nhặt xu ×1. Nhanh hơn khi: —.
- **Biến hình — High Tide:** Hero cưỡi sóng: +40% tốc chạy, sóng đẩy địch gặp phải, hồi 1 máu mỗi giây.
- **Nội tại hero:** Mỗi 2 đồ uống mua được: thủy triều kéo dài thêm 1s (tối đa +3s).
- **Nội tại đội / shop / run:**
  - Đồ uống rẻ hơn 25%.
  - Mỗi đồ uống mua được: +1 vàng thu nhập mỗi wave về sau (tối đa +5).
  - Đồng đội +5% sát thương.

#### 2.2.8 The Comet Courier (`CometRunner`)

- **Thanh:** Đuôi sao chổi. Nạp từ: quãng chạy ×0.6. Nhanh hơn khi: —.
- **Biến hình — Meteor Shower:** Thiên thạch rơi dọc đường hero chạy; +50% tốc chạy, +25% tốc đánh.
- **Nội tại hero:** Chạy liên tục 2s: +15% tốc chạy.
- **Nội tại đội / shop / run:**
  - Xu bay về hero từ xa hơn 30%.
  - Đồng đội +8% tốc chạy.
  - Địch hero lao qua bị chậm 10%.

#### 2.2.9 The Mirror Keeper (`MirrorKeeper`)

- **Thanh:** Khung gương 5 mảnh. Nạp từ: đòn trúng ×1 + chặn đạn ×2. Nhanh hơn khi: Chặn được đạn ×2.
- **Biến hình — Hall of Mirrors:** Bốn tấm gương xoay quanh hero, dội mọi viên đạn địch chạm vào ngược lại; +25% tốc đánh.
- **Nội tại hero:** Gương đỡ một viên đạn địch mỗi 3s và trả nó cùng đòn kế của hero.
- **Nội tại đội / shop / run:**
  - Phản chiếu: mua một đồng đội đã có thì 10% được thêm một bản.
  - Đạn đồng đội xuyên thêm 1 địch.
  - Đòn đầu tiên của đồng đội lên mỗi địch +15%.
  - Đạn địch bay chậm hơn 10%.

#### 2.2.10 The Treasure Clerk (`TreasureAcorn`)

- **Thanh:** 3 lọ tinh chất. Nạp từ: hạ địch ×1.5. Nhanh hơn khi: Màu lọ theo loại địch, mỗi màu một buff nhỏ.
- **Biến hình — Acorn Machine Gun:** Súng hạt dẻ quay tay chỉ bắn vào mục tiêu truy nã; mỗi hạt trúng làm rơi xu cho đội.
- **Nội tại hero:** Luôn có một mục tiêu truy nã; hạ nó được 1 vàng (tối đa 8 mỗi wave).
- **Nội tại đội / shop / run:**
  - Rương đầu tiên mỗi wave có thêm 3 vàng.
  - Đồng đội +10% sát thương lên mục tiêu truy nã.
  - Giữ ít nhất 20 vàng: món rẻ nhất rẻ hơn 1.
  - Thu nhập +1 vàng mỗi wave.

#### 2.2.11 Verdant Matron

- **Thanh:** Giàn hoa. Nạp từ: thời gian ×0.4 + đòn trúng ×1 + mầm trên sân ×0.4. Nhanh hơn khi: Có mầm trên sân ×1.5.
- **Biến hình — Spring Festival:** Mọi mầm nở cùng lúc: gai bung quanh các mầm và quanh hero; hero hồi máu trong lúc biến hình.
- **Nội tại hero:** Vườn ươm: mầm vẫn lớn thành một loại đồng đội được chọn.
- **Nội tại đội / shop / run:**
  - Nhà kính: đồng đội 1★ mua vào có 15% thành 2★.
  - Mầm +10% sát thương.
  - Đồng đội +5% tốc đánh.
  - Jade Nursery tính thêm 1 cấp.

#### 2.2.12 Copper Cauldron

- **Thanh:** Ống áp suất, vạch đỏ nửa trên. Nạp từ: đòn trúng ×1 + bị đánh ×2.5. Nhanh hơn khi: Trên 50%: +15% sát thương.
- **Biến hình — Steam Engine:** Hero chạy như đầu máy: +60% tốc chạy, +30% tốc đánh, nhận ít hơn 25% sát thương; húc và đẩy thứ phía trước; kết thúc bằng một luồng hơi dài.
- **Nội tại hero:** Nắp nồi: đứng yên 0.5s thì nắp đỡ một viên đạn địch (mỗi 4s). Bị đánh: +10% tốc đánh trong 2s.
- **Nội tại đội / shop / run:**
  - Lượt đổi đầu tiên mỗi shop miễn phí.
  - Mỗi đòn hero nhận trong wave: đồng đội +2 sát thương va chạm (tối đa +10).
  - Khi hero bị đánh, đồng đội gần hero +15% tốc đánh trong 2s.

#### 2.2.13 China Marionette

- **Thanh:** Bình sứ nứt vàng. Nạp từ: đòn trúng ×1 + bị đánh ×2.5. Nhanh hơn khi: Bị đánh ×2.
- **Biến hình — Kintsugi:** Vết nứt lấp vàng: hero không nhận sát thương trong 3s, +40% tốc đánh, đòn để lại đường vàng rồi nổ.
- **Nội tại hero:** Mỗi vết nứt (tối đa 3, do bị thương) cho hero +5% sát thương.
- **Nội tại đội / shop / run:**
  - Tiền vá: mỗi lần bị thương được 1 vàng (tối đa 5 mỗi wave).
  - Khi hero bị thương, đồng đội gần +10% sát thương trong 3s.
  - Mỗi lần merge được 2 vàng.
  - Địch đánh hero bị nứt: nhận +5% sát thương trong 4s.

#### 2.2.14 Soap Skater

- **Thanh:** Chai bọt. Nạp từ: quãng chạy ×0.5 + rẽ gắt ×1.5. Nhanh hơn khi: Rẽ gắt ×1.5.
- **Biến hình — Ice Rink:** Sàn thành băng: địch trượt và chậm lại, hero lướt; +40% tốc chạy, +25% tốc đánh.
- **Nội tại hero:** Rẽ gắt để lại bong bóng làm chậm và đẩy nhẹ địch.
- **Nội tại đội / shop / run:**
  - Đồng đội +8% sát thương lên địch bị chậm.
  - Đồng đội đánh trúng địch đang bị chậm thì đóng băng nó 0.3s (mỗi địch một lần mỗi 4s).
  - Địch bị hạ gần hero có 10% rơi thêm 1 xu.

#### 2.2.15 Velvet Regent

- **Thanh:** Quyền trượng 5 ngọc. Nạp từ: đồng đội đánh trúng ×0.3 + đồng đội có bond kích hoạt ×0.15. Nhanh hơn khi: Có bond kích hoạt ×1.5.
- **Biến hình — Royal Parade:** Phong tước: trong lúc diễu hành mọi đồng đội đánh như được thêm 1 sao (+50% sát thương, +20% tốc đánh).
- **Nội tại hero:** Có ít nhất 3 đồng đội: hero +10% sát thương.
- **Nội tại đội / shop / run:**
  - Triều đình mở rộng: mỗi wave được tuyển thêm 1 loại đồng đội mới.
  - Đồng đội +6% sát thương.
  - Velvet Court tính thêm 1 cấp.

#### 2.2.16 Button Cavalier

- **Thanh:** Cờ đấu kiếm. Nạp từ: đòn trúng ×1 + né / suýt trúng ×2. Nhanh hơn khi: Né / suýt trúng ×2.
- **Biến hình — The Duel:** Thời gian chậm lại quanh một địch: hero đâm sáu nhát liên tiếp; +50% tốc đánh.
- **Nội tại hero:** Mỗi điểm thăng bằng (tối đa 3) cho +4% né.
- **Nội tại đội / shop / run:**
  - Lịch thiệp: từ món thứ hai mua ở mỗi shop rẻ hơn 10%.
  - Khi hero né, đồng đội +10% chí mạng trong 2s.
  - Đồng đội cận chiến +6% tốc đánh.

#### 2.2.17 Night Maw

- **Thanh:** Cái bụng. Nạp từ: hạ địch ×1.5. Nhanh hơn khi: —.
- **Biến hình — The Feast:** Một cái miệng đêm khổng lồ nuốt quái thường phía trước hero (không nuốt boss); mỗi lần nuốt hồi máu cho hero.
- **Nội tại hero:** Đói: hạ địch hồi máu cho hero.
- **Nội tại đội / shop / run:**
  - Đồ ăn mua được mạnh hơn 50% với hero.
  - Đồng đội +8% sát thương lên địch dưới 30% máu.
  - Địch bị hạ gần hero có 15% rơi thêm 1 xu.
  - Mỗi lần hạ địch: đồng đội +1% tốc đánh trong wave (tối đa 10%).

#### 2.2.18 Thorn Treasurer

- **Thanh:** Cột 6 gai. Nạp từ: đòn trúng ×1 + bị đánh ×2.5. Nhanh hơn khi: Bị đánh ×2.
- **Biến hình — Thorn Throne:** Hero ngồi trên ngai gai: mọi đòn đánh vào hero bị gai đáp trả; giảm 30% sát thương nhận.
- **Nội tại hero:** Gai đáp trả đòn hero nhận (mỗi giây một lần).
- **Nội tại đội / shop / run:**
  - Lãi kép: lãi tính trên 150% số vàng đang giữ.
  - Đồng đội +5% sát thương mỗi 50 vàng đang giữ (tối đa 10%).
  - Giữ ít nhất 50 vàng: lượt đổi rẻ hơn 1.
  - Gai trả tiền: mỗi lần gai đáp trả có 10% rơi 1 xu.

#### 2.2.19 Citrus Marshal

- **Thanh:** Ống nước cam sôi. Nạp từ: quãng chạy ×0.4 + đòn trúng ×1. Nhanh hơn khi: Trên 50%: +10% tốc chạy.
- **Biến hình — Citrus Train:** Đoàn xe nối thành một đoàn tàu cán qua địch; cả đội +30% tốc chạy.
- **Nội tại hero:** Đoàn xe: đồng đội đi theo thành hàng sau hero.
- **Nội tại đội / shop / run:**
  - Tiếp tế: +2 vàng cuối mỗi wave.
  - Đồng đội ở phía sau hero +5% sát thương.
  - Đồng đội +8% tốc chạy.
  - Đoàn xe quét xu dọc đường (tầm nhặt +0.6).

#### 2.2.20 Ivory Sovereign

- **Thanh:** Cuộn chiếu chỉ. Nạp từ: đòn trúng ×1 + bị đánh ×1.5. Nhanh hơn khi: —.
- **Biến hình — Coronation:** Chiếu chỉ cuối cùng vắt ngang sân thành một đường phán quyết; quái thường dưới 15% máu trên đường đó bị xử tử.
- **Nội tại hero:** Máu hoàng gia: 12 máu và mỗi đòn chỉ mất 1.
- **Nội tại đội / shop / run:**
  - Chọn của vua: món hiếm nhất mỗi shop rẻ hơn 30%.
  - Khi hero còn ít nhất 6 máu, đồng đội +6% sát thương.
  - Mỗi Mythic hoặc Prismatic mua được: đội +5% sát thương cả run (tối đa +25%).
  - Mỗi boss bị hạ trả 5 vàng.

#### 2.2.21 Petal Citadel

- **Thanh:** Tháp 4 cửa sổ cánh hoa. Nạp từ: thời gian ×0.4 + đòn trúng ×1. Nhanh hơn khi: —.
- **Biến hình — Petal Citadel:** Thành lũy cánh hoa mọc quanh hero: hero bắn từ trên tường, giảm 20% sát thương nhận.
- **Nội tại hero:** Bốn cánh hoa mọc từ các relic đang có.
- **Nội tại đội / shop / run:**
  - Bản thứ hai của một relic cho thêm 50% tác dụng.
  - Mỗi cánh hoa đã mọc: đồng đội +3% sát thương.
  - Relic nhân đôi: mua một relic đã có hoàn 3 vàng.
  - Đủ bốn cánh hoa: đồng đội +10% tốc đánh.

#### 2.2.22 Patchwork Host

- **Thanh:** Rạp hát và khán giả. Nạp từ: đồng đội đánh trúng ×0.3 + nhiều loại đồng đội cùng đánh ×0.15. Nhanh hơn khi: Nhiều loại đồng đội cùng đánh ×1.5.
- **Biến hình — Showtime:** Màn nhung kéo lên: đồng đội chính tách làm ba và cùng diễn.
- **Nội tại hero:** Mỗi lần đồng đội chính diễn, hero hồi 1 máu (tối đa mỗi 4s).
- **Nội tại đội / shop / run:**
  - Đồng đội chính được tính hai lần trong bond của nó.
  - Đồng đội chính +10% sát thương.
  - Đồng đội chính từ 2★: đồng đội +5% tốc đánh.

#### 2.2.23 Archive Curator

- **Thanh:** 3 ngăn mẫu vật. Nạp từ: hạ địch ×1.5 + loài mới ×3. Nhanh hơn khi: Loài mới ×3.
- **Biến hình — Living Exhibit:** Các loài đã ghi bước ra khỏi sổ thành rối giấy và chiến đấu cho hero.
- **Nội tại hero:** Hero +5% sát thương lên loài đã ghi.
- **Nội tại đội / shop / run:**
  - Mỗi 5 loài mới được ghi trả 2 vàng.
  - Đồng đội +5% sát thương lên loài đã ghi.
  - Đồng đội +5% chí mạng lên loài đã ghi.

#### 2.2.24 Penny Broker

- **Thanh:** Ống xu. Nạp từ: nhặt xu ×1. Nhanh hơn khi: —.
- **Biến hình — Bull Market:** Xu nhặt được tính gấp đôi và mỗi lần nhặt bắn ra một tia hóa đơn.
- **Nội tại hero:** Đặt hàng trước: giữ chỗ một relic cho shop sau.
- **Nội tại đội / shop / run:**
  - Cổ tức: +1 vàng mỗi relic đang có cuối mỗi wave (tối đa 5).
  - Đồng đội +2% tốc đánh mỗi relic đang có (tối đa 20%).
  - Đặt hàng trước rẻ hơn 10%.
  - Trần lãi +1.

#### 2.2.25 Storm Fencer

- **Thanh:** Cuộn sét. Nạp từ: đòn trúng ×1 + né / suýt trúng ×2. Nhanh hơn khi: Né / suýt trúng ×2.
- **Biến hình — Thunder God:** Lưỡi kiếm hóa sét: mỗi nhát lan sang bốn địch; +40% tốc đánh.
- **Nội tại hero:** Đòn thường có 10% lan sang một địch khác.
- **Nội tại đội / shop / run:**
  - Quái lớn và quái đầu đàn rơi trứng.
  - Đồng đội +8% sát thương lên địch bị sốc trong 3s gần nhất.
  - Đồng đội +5% tốc đánh.
  - Đòn đồng đội có 5% lan.

#### 2.2.26 Tin Virtuoso

- **Thanh:** Máy đếm nhịp. Nạp từ: đòn trúng ×1 + chí mạng ×1. Nhanh hơn khi: Chí mạng ×2.
- **Biến hình — Symphony:** Cả đội chơi cùng nhau: mỗi nhịp mọi đồng đội cùng đánh.
- **Nội tại hero:** Chí mạng của hero tính là hai nhịp.
- **Nội tại đội / shop / run:**
  - Hòa âm: mỗi cấp bond đang kích hoạt cho đồng đội +3% tốc đánh (tối đa 15%).
  - Đồng đội +3% chí mạng.
  - Đồng đội +5% tốc đánh.

#### 2.2.27 Moon Prowler

- **Thanh:** 4 pha trăng. Nạp từ: quãng chạy ×0.5 + đánh từ phía sau ×0.5. Nhanh hơn khi: Đánh từ phía sau ×1.5.
- **Biến hình — Eclipse:** Sân tối lại: hero lẻn ra sau hết địch này đến địch khác; mọi đòn đều tính là từ phía sau.
- **Nội tại hero:** Đánh từ phía sau địch +45%.
- **Nội tại đội / shop / run:**
  - Móc túi: đòn từ phía sau có 10% làm rơi 1 xu.
  - Đồng đội +8% sát thương lên địch đang quay lưng.
  - Địch bị đánh từ phía sau chậm 10%.
  - Đồng đội cận chiến đánh từ phía sau luôn chí mạng.

#### 2.2.28 Supper Mortar

- **Thanh:** Tủ 6 đĩa. Nạp từ: đòn trúng ×1. Nhanh hơn khi: —.
- **Biến hình — Six-Barrel Kitchen:** Cối sáu nòng thay ấm trà, phục vụ cả thực đơn.
- **Nội tại hero:** Hai món: mỗi đòn thường thành hai món bắn nhanh.
- **Nội tại đội / shop / run:**
  - Thực đơn: mỗi shop hiện thêm 1 món.
  - Mỗi đồ ăn mua được: đồng đội +3% sát thương (tối đa 15%).
  - Đồ ăn mạnh hơn 20% với hero.
  - Đồ ăn rẻ hơn 20%.

#### 2.2.29 Lantern Weaver

- **Thanh:** Khung thêu 5 kim. Nạp từ: đòn trúng ×1 + địch bị ghim ×0.5. Nhanh hơn khi: Địch đã bị ghim ×1.5.
- **Biến hình — Grand Tapestry:** Mọi nút thắt nối thành một tấm lưới siết lại: địch trong lưới chậm 40% và bị lưới cắt.
- **Nội tại hero:** Đòn của hero ghim tối đa 3 địch; địch bị ghim chậm 10%.
- **Nội tại đội / shop / run:**
  - Đạn đồng đội xuyên qua địch bị ghim được +1 xuyên.
  - Đồng đội +8% sát thương lên địch bị ghim.
  - Đạn đồng đội xuyên thêm 1 địch.

#### 2.2.30 Hourglass Stag

- **Thanh:** Đồng hồ cát sổ nợ. Nạp từ: đòn trúng ×1 + đòn ghi nợ ×0.5. Nhanh hơn khi: Đòn được ghi nợ ×1.5.
- **Biến hình — Foreclosure:** Thời gian dừng 1.5s và mọi khoản nợ bị thu cùng lúc, kèm lãi.
- **Nội tại hero:** Nợ thời gian: một phần sát thương của hero bị hoãn, thu sau kèm lãi.
- **Nội tại đội / shop / run:**
  - Khoản nợ hạ được con nợ trả 1 vàng.
  - Khi còn nợ chưa thu, đồng đội +5% tốc đánh.
  - Con nợ bị chậm 10%.

#### 2.2.31 Rain Archivist

- **Thanh:** 5 trang sách. Nạp từ: thời gian ×0.4 + đòn trúng ×1 + mở rương ×4. Nhanh hơn khi: Mở rương ×2.
- **Biến hình — Monsoon Chapter:** Bão quét qua sân: sét đánh địch và mưa hồi máu cho hero.
- **Nội tại hero:** Rương hồi máu cho hero.
- **Nội tại đội / shop / run:**
  - Rương ướt: rương đầu tiên mỗi wave có thêm 2 vàng.
  - Đòn của hero làm ướt địch: chậm 10% trong 1s.
  - Đồng đội +5% sát thương lên địch bị ướt.
  - Đồng đội +5% tốc đánh.

#### 2.2.32 Kite Admiral — **Mới**

- **Thanh:** Diều có cửa sổ gió (art). Nạp từ: đạn trúng xa hơn 4 ô ×1.5 + đòn trúng ×0.5. Nhanh hơn khi: Đạn của chính hero trúng xa hơn 4 ô (v5 §5).
- **Biến hình — Hạm đội diều (Fleet Kite):** 4 diều bay trên 4 đồng đội tầm xa gần nhất; đạn của họ tách đôi khi diều còn bay (v5 §4).
- **Nội tại hero:** Xuôi gió +15% tốc chạy (v3).
- **Nội tại đội / shop / run:**
  - Paper Wings tính thêm 1 cấp (v5 §3).
  - Đồng đội tầm xa +10% tầm (v3).
  - Đồng đội tầm xa +1 xuyên khi thanh đầy (v5 §5, thay "lượt đổi đầu mỗi wave giảm 50%").

#### 2.2.33 Sundae Duchess — **Mới**

- **Thanh:** Ly kem tan dần. Nạp từ: **chưa có công thức trong `HeroGauge.cs`**. Nhanh hơn khi: Bị đánh, hạ địch đang đóng băng (v3; tự tan theo thời gian).
- **Biến hình — Mưa Kem (Giant Scoop):** Kem khổng lồ rơi lên địch 6s, đóng băng chỗ rơi (v3).
- **Nội tại hero:** Mỗi đồ ăn mua được: +5 máu tối đa vĩnh viễn (v3).
- **Nội tại đội / shop / run:**
  - Sweet Tooth tính thêm 1 cấp (v5 §3).
  - Địch đóng băng chịu +8% sát thương từ đồng đội (v3).
  - Đồ ăn rẻ hơn 15% (v3).
  - Mỗi địch đóng băng bị hạ: +1 vàng, tối đa 5 mỗi wave (v5 §5).
  - Địch đóng băng trúng chí mạng mất 5% máu tối đa (v5 §8).

#### 2.2.34 Bonsai Clocksmith — **Mới**

- **Thanh:** Mặt đồng hồ. Nạp từ: đòn trúng ×1 + thời gian ×0.4 + hạ địch ×0.5. Nhanh hơn khi: Hạ địch ×1.5.
- **Biến hình — Vườn Thời Gian (Time Garden):** Vườn mọc phía trước; địch trong vườn chậm 50%, đồng đội trong vườn +40% tốc đánh (v3).
- **Nội tại hero:** Biến hình xong giữ lại nửa nấc (v3).
- **Nội tại đội / shop / run:**
  - Clockwork Union tính thêm 1 cấp (v5 §3).
  - Mỗi lần merge +2 vàng (v3).
  - Đồng đội +4% tốc đánh mỗi bond Bộ Máy / Vườn Cây đang kích hoạt (v3).

#### 2.2.35 Juggling Jester — **Mới**

- **Thanh:** 5 quả bóng trên đầu. Nạp từ: đồng đội chí mạng ×1 + đòn trúng ×0.5. Nhanh hơn khi: Chí mạng của đồng đội (v5 §5).
- **Biến hình — Đại Xiếc (Grand Circus):** 5 bóng nện xuống 5 địch, rồi 6s mọi đòn ném hai chùy (v3).
- **Nội tại hero:** Chí mạng của hero tung một quả bóng rơi xuống một địch ngẫu nhiên, 50% sát thương (v5 §5).
- **Nội tại đội / shop / run:**
  - Big Top tính thêm 1 cấp (v5 §3).
  - Thành viên Big Top tuyển vào có 10% là 2★ (v5 §8).
  - Đồng đội +6% chí mạng (v3).

#### 2.2.36 Bell Diver — **Mới**

- **Thanh:** Đồng hồ độ sâu. Nạp từ: ở gần địch ×0.5 + đòn trúng ×1. Nhanh hơn khi: Đánh trúng địch đã dò ×1.5.
- **Biến hình — Tàu Ngầm Chuông (Diving Bell):** Chui vào chuông đồng, húc theo hướng chạy, thả mìn phía sau, nhận ít hơn 50% sát thương (v3).
- **Nội tại hero:** Đánh trúng địch đã dò: 15% rơi 1 xu (v3).
- **Nội tại đội / shop / run:**
  - Rain Script tính thêm 1 cấp (v5 §3).
  - Địch đã dò chịu +8% sát thương từ đồng đội (v3).
  - Rương thêm 1 lựa chọn (v3).
  - Địch đã dò nhận thêm 1 lần nảy từ đạn đồng đội (v5 §5/§8).

## 3. Bond (33)

Cấp bond = số thành viên khác loài ÷ ngưỡng (làm tròn xuống); mỗi cấp cộng "Mỗi cấp", và khi đạt mốc thưởng thêm thì cộng thêm giá trị mốc. Trong game thưởng dương nhân ×0.9 (`BondDefinition.PositiveRewardScale`). Companion Rare cộng +1 thành viên cho bond phụ của nó khi bond đã kích hoạt.

### 3.1 Tám bond chủ đề

| Bond | Id | Loại | Ngưỡng | Hiệu ứng | Mỗi cấp | Mốc thưởng thêm | Thành viên | Hero dùng |
|---|---|---|---:|---|---|---|---|---|
| Hearth (Bếp Ấm) | `hearth` | chủ đề | 2 | Hồi máu hero mỗi giây | +0.15 | 4 → +0.15, 6 → +0.3 | 12: Bread Crab (C), Kettle Bat (C), Teapot Pilgrim (C), Baker Mouse (U)*, Bottle Gull (U)*, Candle Snail (U), Teacup Cricket (U), Velvet Enchantress (U), Saffron Mechanist (R), Coil Kettle (M), Tea Dragon (M)*, Sundae Golem (P)* | 9 hero (chủ đề) |
| Archive (Thư Viện) | `archive` | chủ đề | 2 | Sát thương | +8% | 4 → +8%, 6 → +16% | 13: Brass Acorn (C), Court Herald (C)*, Mailbox Imp (C), Pinned Beetle (C)*, Spindle Wren (U), Bookmark Golem (R), Lens Owl (R)*, The Folded Courier (R), The Forgotten Patchwork (R), Thimble Moth (R), Velvet Champion (R), Reel Owl (M), The Endless Index (P)* | 8 hero (chủ đề) |
| Clock (Bộ Máy) | `clock` | chủ đề | 2 | Tốc đánh | +7% | 4 → +7%, 6 → +14% | 12: Button Beetle (C), Dream Compass (C), Staple Mole (C), Static Ferret (C)*, Tin Tick (C), Grand Sprout (U), Lamp Goat (U)*, Specimen Slime (R)*, Spring Duck (R), Top Beetle (R)*, Clockwork Piper (M), The Last Bell (P) | 9 hero (chủ đề) |
| Frost (Sương Giá) | `frost` | chủ đề | 2 | Tốc độ quái | -5% | 4 → -5%, 6 → -10% | 11: Boba Tadpole (C)*, Fridge Wisp (C), Hood Wisp (C), Snow Spoon (C), Fossil Armadillo (U)*, Ice Cream Owl (U), Monster Marble (U), Rain Heron (R)*, Shovel Penguin (R)*, Iron Locomotive (M), The Impossible Tailor (P) | 9 hero (chủ đề) |
| Orchard (Vườn Cây) | `orchard` | chủ đề | 2 | May mắn | +0.04 | 4 → +0.04, 6 → +0.08 | 11: Mushroom Librarian (C), Radish Jester (C), Seed Hedgehog (C), Bristle Ball (U), Citrus Toad (U), Paper Devil (U), Pear Moth (U), Watering Snail (U)*, Yo-yo Squirrel (R)*, Paper Inventor (M), The First Seed (P)* | 10 hero (chủ đề) |
| Astral (Tinh Tú) | `astral` | chủ đề | 2 | Cỡ đạn | +8% | 4 → +8%, 6 → +16% | 11: Comet Kite (C), Orbit Moth (C), Paper Plane Pigeon (C)*, Little Sprout (U), Star Sock (U), Storm Kitten (U)*, Dough Foreman (R), Moon Jelly (R), The Sleeping Friend (R), Pillow Oracle (M), Eclipse Egg (P) | 9 hero (chủ đề) |
| Tidal (Thủy Triều) | `tidal` | chủ đề | 2 | Tỉ lệ nảy | +8% | 4 → +8%, 6 → +16% | 13: Bubble Koi (C), Shanty Sea Lion (C)*, Shell Postman (C), Anchor Snail (U), Balloon Mallet (U), Coral Teacup (U), Raincoat Frog (U)*, Ribbon Otter (U), Sail Squid (U), Volt Eel (R)*, Harbor Whale (M)*, Return Captain (M), Kite Dragon (P)* | 9 hero (chủ đề) |
| Oddity (Kỳ Lạ) | `oddity` | chủ đề | 2 | Chí mạng | +5% | 4 → +5%, 6 → +10% | 11: Crayon Gremlin (C), Dice Crab (C), Jelly Swordsman (C), Mask Pepper (C), Ribbon Spider (C), Sock Cyclops (C), Carousel Pony (U), Violet Remnant (U), Mirror Newt (M), Ringmaster Jack (M)*, The Lost Carousel (P)* | 10 hero (chủ đề) |

### 3.2 Hai mươi lăm bond phụ

| Bond | Id | Loại | Ngưỡng | Hiệu ứng | Mỗi cấp | Mốc thưởng thêm | Thành viên | Hero dùng |
|---|---|---|---:|---|---|---|---|---|
| Big Top (bond mới v3) | `big-top` | phụ | 2 | Giảm giá tuyển | +8% | — | 4: Balloon Mallet (U), Carousel Pony (U), Ringmaster Jack (M)*, The Lost Carousel (P)* | Đặc trưng: Patchwork Host, Juggling Jester; kích hoạt được trong 10 bộ |
| Clockwork Union | `gear` | phụ | 2 | Thu nhập mỗi wave | +4 | — | 5: Staple Mole (C), Saffron Mechanist (R), Top Beetle (R)*, Coil Kettle (M), Paper Inventor (M) | Đặc trưng: Penny Broker, Bonsai Clocksmith; kích hoạt được trong 17 bộ |
| Crossing Knights | `knight` | phụ | 2 | Sát thương va chạm | +5 | — | 3: Jelly Swordsman (C), Anchor Snail (U), Return Captain (M) | Đặc trưng: The Lone Jelly, Button Cavalier; kích hoạt được trong 10 bộ |
| Exhibit Hall (bond mới v5) | `exhibit-hall` | phụ | 2 | Sát thương đồng đội lên loài đã ghi | +10% | — | 3: Pinned Beetle (C)*, Fossil Armadillo (U)*, Specimen Slime (R)* | Bond riêng của Archive Curator; kích hoạt được trong 1 bộ |
| Hat Society | `hat` | phụ | 2 | Trần lãi | +2 | — | 4: Court Herald (C)*, Mailbox Imp (C), Dough Foreman (R), Pillow Oracle (M) | Đặc trưng: Drowsy Seer, Ivory Sovereign; kích hoạt được trong 16 bộ |
| Jade Nursery | `sprout` | phụ | 3 | Giảm giá shop vàng | +6% | — | 3: Grand Sprout (U), Little Sprout (U), Watering Snail (U)* | Đặc trưng: Verdant Matron; kích hoạt được trong 1 bộ |
| Lantern Guild | `lantern-guild` | phụ | 2 | Tầm đánh | +10% | — | 4: Teapot Pilgrim (C), Candle Snail (U), Teacup Cricket (U), Harbor Whale (M)* | Đặc trưng: Lantern Knight, Lantern Weaver; kích hoạt được trong 10 bộ |
| Little Horns | `horn` | phụ | 2 | Xuyên | +1 | — | 3: Bristle Ball (U), Lamp Goat (U)*, Paper Devil (U) | Đặc trưng: Thorn Treasurer; kích hoạt được trong 9 bộ |
| Longbow | `archer` | phụ | 2 | Sát thương tầm xa (cộng thẳng) | +3 | — | 3: Comet Kite (C), Dream Compass (C), Moon Jelly (R) | Đặc trưng: Petal Citadel; kích hoạt được trong 9 bộ |
| Lucky Leaves | `clover` | phụ | 2 | Độ hiếm shop | +8% | — | 4: Bubble Koi (C), Dice Crab (C), Mushroom Librarian (C), Coral Teacup (U) | Đặc trưng: Lady Loaded Dice, The Treasure Clerk; kích hoạt được trong 12 bộ |
| Masked Carnival | `mask` | phụ | 2 | Chất lượng rương | +10% | — | 4: Crayon Gremlin (C), Mask Pepper (C), Radish Jester (C), Monster Marble (U) | Đặc trưng: Moon Prowler; kích hoạt được trong 12 bộ |
| Morning Bells | `morning` | phụ | 2 | Sát thương hero | +10% | — | 5: Pear Moth (U), Clockwork Piper (M), Tea Dragon (M)*, Eclipse Egg (P), The Last Bell (P) | Đặc trưng: Tin Virtuoso, Hourglass Stag; kích hoạt được trong 14 bộ |
| Paper Wings (bond mới v3) | `paper-wings` | phụ | 2 | Tầm bay của đạn | +12% | — | 4: Paper Plane Pigeon (C)*, Spindle Wren (U), The Folded Courier (R), Kite Dragon (P)* | Đặc trưng: Postmaster, Kite Admiral; kích hoạt được trong 9 bộ |
| Prizefighters | `gauntlet` | phụ | 2 | Sát thương cận chiến (cộng thẳng) | +4 | — | 3: Bread Crab (C), Kettle Bat (C), Shovel Penguin (R)* | Đặc trưng: Night Maw; kích hoạt được trong 10 bộ |
| Quicksoles | `shoe` | phụ | 2 | Tốc chạy | +8% | — | 4: Fridge Wisp (C), Orbit Moth (C), Sock Cyclops (C), Star Sock (U) | Đặc trưng: The Comet Courier, Soap Skater; kích hoạt được trong 11 bộ |
| Rain Script | `rain-script` | phụ | 2 | Tốc độ đạn | +10% | — | 3: Raincoat Frog (U)*, Ribbon Otter (U), Rain Heron (R)* | Đặc trưng: Rain Archivist, Bell Diver; kích hoạt được trong 8 bộ |
| Rootkin | `root` | phụ | 2 | Máu tối đa hero | +8 | — | 3: Hood Wisp (C), Seed Hedgehog (C), The First Seed (P)* | Đặc trưng: The Root Oracle; kích hoạt được trong 10 bộ |
| Runic School | `rune` | phụ | 2 | Tách đạn | +1 | — | 4: Bookmark Golem (R), Lens Owl (R)*, Reel Owl (M), The Endless Index (P)* | Đặc trưng: Archive Curator; kích hoạt được trong 9 bộ |
| Sea Shanty (bond mới v5) | `sea-shanty` | phụ | 2 | Giảm giá relic | +6% | — | 3: Shanty Sea Lion (C)*, Shell Postman (C), Bottle Gull (U)* | Đặc trưng: The Tide Singer; kích hoạt được trong 8 bộ |
| Springstep | `hare` | phụ | 2 | Đẩy lùi | +15% | — | 3: Citrus Toad (U), Spring Duck (R), Yo-yo Squirrel (R)* | Đặc trưng: Citrus Marshal; kích hoạt được trong 9 bộ |
| Static Club (bond mới v5) | `static-club` | phụ | 2 | Tỉ lệ đòn đồng đội lan sang một địch gần (50% sát thương) | +4% | — | 4: Static Ferret (C)*, Tin Tick (C), Storm Kitten (U)*, Volt Eel (R)* | Đặc trưng: Storm Fencer; kích hoạt được trong 10 bộ |
| Sweet Tooth (bond mới v3) | `sweet-tooth` | phụ | 2 | Tốc đánh đồng đội | +7% | — | 5: Boba Tadpole (C)*, Snow Spoon (C), Baker Mouse (U)*, Ice Cream Owl (U), Sundae Golem (P)* | Đặc trưng: Supper Mortar, Sundae Duchess; kích hoạt được trong 17 bộ |
| Unbroken | `resolve` | phụ | 2 | Hệ số sát thương | +6% | — | 4: Sail Squid (U), Violet Remnant (U), Iron Locomotive (M), Mirror Newt (M) | Đặc trưng: The Mirror Keeper, Copper Cauldron; kích hoạt được trong 12 bộ |
| Velvet Court | `regal` | phụ | 2 | Sát thương đồng đội | +8% | — | 4: Brass Acorn (C), Button Beetle (C), Velvet Enchantress (U), Velvet Champion (R) | Đặc trưng: Velvet Regent; kích hoạt được trong 9 bộ |
| Wild Stitch | `hide` | phụ | 2 | Tầm nhặt | +0.4 | — | 5: Ribbon Spider (C), The Forgotten Patchwork (R), The Sleeping Friend (R), Thimble Moth (R), The Impossible Tailor (P) | Đặc trưng: China Marionette; kích hoạt được trong 11 bộ |

Ký hiệu thành viên: (C/U/R/M/P) = Common / Uncommon / Rare / Mythic / Prismatic; `*` = companion mới chưa vào game. Bond mới của đợt mở rộng: Paper Wings, Sweet Tooth, Big Top (bản 3) và Static Club, Sea Shanty, Exhibit Hall (bản 5). Bond đã bỏ: Fruit Basket, Lantern Revel (gộp vào Lantern Guild).

Tác dụng ghi trong thiết kế bản 5 §6 cho 3 bond mới nhất: **Static Club** — đòn của đồng đội có 4%/cấp lan sang một địch gần (50% sát thương); **Sea Shanty** — relic rẻ hơn 6%/cấp; **Exhibit Hall** — đồng đội +10%/cấp sát thương lên loài đã ghi (bond riêng của Archive Curator, như Jade Nursery của Verdant Matron).

## 4. Companion (94)

Vũ khí: companion Common–Rare nhận vũ khí ngẫu nhiên khi tuyển; Mythic / Prismatic luôn cầm **vũ khí khóa**. Companion mới C/U/R mang vũ khí mới của mình theo bản 3 §4.3. "Talent" là tác dụng trong game hiện tại (`OctoberCompanions.Copy`); "Kỹ năng riêng" là kỹ năng của Mythic / Prismatic và các companion bản 5. "Bộ hero" = số hero có companion này trong bộ cố định (danh sách đầy đủ ở Phụ lục 8).

### 4.1 Hearth — Bếp Ấm (12)

| Companion | Mới | Bậc | Bond chủ đề | Bond phụ | Vũ khí | Talent / kỹ năng | Bộ hero | Ghi chú |
|---|:-:|---|---|---|---|---|---:|---|
| Sundae Golem | **Mới** | Prismatic | Hearth (Bếp Ấm) | Sweet Tooth | **StarKnuckle** (khóa) | **Kỹ năng riêng:** Mỗi đòn để lại vũng kem; địch đứng trong vũng 2s bị đóng băng. | 9 | chỉ từ quà lăng kính |
| Coil Kettle |  | Mythic | Hearth (Bếp Ấm) | Clockwork Union | **CatcherMitt** (khóa) | **Kỹ năng riêng:** Đòn đầu tiên lên mỗi địch mới hất nó lên (choáng 0.4s). **Talent:** Lượt đổi trả tiền rẻ hơn 2 vàng (tối thiểu 1). Nội tại: hồi máu hero mỗi 4s. | 10 |  |
| Tea Dragon | **Mới** | Mythic | Hearth (Bếp Ấm) | Morning Bells | **HearthBrick** (khóa) | **Kỹ năng riêng:** Đặt một ấm trà phía trước; ấm reo rồi nổ hơi bỏng, lan sang địch kế bên (v5 §4). | 11 |  |
| Saffron Mechanist |  | Rare | Hearth (Bếp Ấm) | Clockwork Union | ngẫu nhiên | **Talent:** Khi mua: tuyển tối đa 2 Balloon Mallet. | 10 |  |
| Baker Mouse | **Mới** | Uncommon | Hearth (Bếp Ấm) | Sweet Tooth | RollingPin (bản 3) | (chưa có mô tả) | 9 | khởi đầu của Sundae Duchess |
| Bottle Gull | **Mới** | Uncommon | Hearth (Bếp Ấm) | Sea Shanty | ngẫu nhiên | **Kỹ năng riêng:** Mua vào tính như 1 đồ uống. | 10 |  |
| Candle Snail |  | Uncommon | Hearth (Bếp Ấm) | Lantern Guild | ngẫu nhiên | **Talent:** Đồng đội Fruit Basket +8% tốc đánh (Fruit Basket đã bị cắt, xem ghi chú). Nội tại: mỗi 4s làm lạnh địch xung quanh. | 10 | khởi đầu của Drowsy Seer |
| Teacup Cricket |  | Uncommon | Hearth (Bếp Ấm) | Lantern Guild | ngẫu nhiên | **Talent:** Mỗi đòn thường thứ 5 phát một tiếng vọng 40% sát thương vũ khí. | 10 | khởi đầu của Copper Cauldron |
| Velvet Enchantress |  | Uncommon | Hearth (Bếp Ấm) | Velvet Court | ngẫu nhiên | **Talent:** Đòn thường kéo nhẹ quái thường vào trong (không kéo boss). | 10 |  |
| Bread Crab |  | Common | Hearth (Bếp Ấm) | Prizefighters | ngẫu nhiên | **Talent:** Mỗi đòn con này +1% tốc đánh (tối đa 20% trong wave); mỗi đòn thứ 3 gây một vụ nổ nhỏ. Nội tại: hồi máu hero mỗi 4s. | 10 | khởi đầu của Supper Mortar |
| Kettle Bat |  | Common | Hearth (Bếp Ấm) | Prizefighters | ngẫu nhiên | **Talent:** Hero +5 máu tối đa, +0.08 giáp. Nội tại: mỗi 4s nhận vàng. | 10 | khởi đầu của Lantern Knight |
| Teapot Pilgrim |  | Common | Hearth (Bếp Ấm) | Lantern Guild | ngẫu nhiên | **Talent:** Mỗi đồ uống mua được: con này +4% sát thương (tối đa +40%). Nội tại: hồi máu hero mỗi 4s. | 10 | nhân vật cốt truyện (chỉ xuất hiện theo luật cốt truyện) |

### 4.2 Archive — Thư Viện (13)

| Companion | Mới | Bậc | Bond chủ đề | Bond phụ | Vũ khí | Talent / kỹ năng | Bộ hero | Ghi chú |
|---|:-:|---|---|---|---|---|---:|---|
| The Endless Index `EndlessIndex` | **Mới** | Prismatic | Archive (Thư Viện) | Runic School | **PaperShuriken** (khóa) | **Kỹ năng riêng:** Mỗi loài mới bị hạ thêm một trang; ở 10 / 20 / 30 trang, đồng đội Runic School +1 tách đạn (v5 §8). | 9 | chỉ từ quà lăng kính |
| Reel Owl |  | Mythic | Archive (Thư Viện) | Runic School | **DreamRacket** (khóa) | **Kỹ năng riêng:** Mỗi đòn thứ 4 phát lại đòn mạnh nhất của một đồng đội gần đó. **Talent:** Mỗi đòn thứ 3 bắn một tiếng vọng màu 25% sát thương vũ khí. | 9 |  |
| Bookmark Golem |  | Rare | Archive (Thư Viện) | Runic School | ngẫu nhiên | **Talent:** Khi hero đánh, con này bắn thêm một tia (hồi 0.7s, không kích hoạt dây chuyền). Nội tại: +sát thương mỗi đồng đội trong đội. | 9 | khởi đầu của Postmaster |
| Lens Owl | **Mới** | Rare | Archive (Thư Viện) | Runic School | SunLens (bản 3) | (chưa có mô tả) | 9 | khởi đầu của Juggling Jester |
| The Folded Courier `SecretPaperBird` |  | Rare | Archive (Thư Viện) | Paper Wings | ngẫu nhiên | **Talent:** Người đưa thư gấp giấy được cứu khỏi Kho Đồ Thất Lạc; bạn vĩnh viễn, nhớ lá thư của bạn và hồi máu định kỳ. Nội tại: hồi máu hero mỗi 4s. | 9 | cư dân sự kiện bí mật (Kho Đồ Thất Lạc) |
| The Forgotten Patchwork `SecretPatchBear` |  | Rare | Archive (Thư Viện) | Wild Stitch | ngẫu nhiên | **Talent:** Cư dân được cứu khỏi Kho Đồ Thất Lạc; bạn vĩnh viễn. | 9 | cư dân sự kiện bí mật (Kho Đồ Thất Lạc) |
| Thimble Moth |  | Rare | Archive (Thư Viện) | Wild Stitch | ngẫu nhiên | **Talent:** Hero bị đánh được 3 vàng (hồi chung 2s). | 9 | khởi đầu của Lantern Weaver |
| Velvet Champion |  | Rare | Archive (Thư Viện) | Velvet Court | ngẫu nhiên | **Talent:** Mọi bond +1 thành viên hỗ trợ (không cộng dồn với Walter khác). | 8 |  |
| Spindle Wren |  | Uncommon | Archive (Thư Viện) | Paper Wings | ngẫu nhiên | **Talent:** Đồng đội Lantern Guild +8% tầm. | 9 |  |
| Brass Acorn |  | Common | Archive (Thư Viện) | Velvet Court | ngẫu nhiên | **Talent:** Sau khi hero bị đánh, hồi 1 máu (hồi chung 1s). | 8 | khởi đầu của The Treasure Clerk |
| Court Herald | **Mới** | Common | Archive (Thư Viện) | Hat Society | ngẫu nhiên | **Kỹ năng riêng:** Kèn: đầu mỗi wave, đòn đầu của mọi đồng đội bắn thêm 1 viên. | 9 |  |
| Mailbox Imp |  | Common | Archive (Thư Viện) | Hat Society | ngẫu nhiên | **Talent:** 4s sau khi hero bị đánh, cả đội +10% tốc đánh. Nội tại: mỗi 4s nhận vàng. | 9 | khởi đầu của Penny Broker |
| Pinned Beetle | **Mới** | Common | Archive (Thư Viện) | Exhibit Hall | ngẫu nhiên | **Kỹ năng riêng:** Mẫu vật bảo tàng: đòn ghim địch 0.3s. | 1 | riêng của Archive Curator |

### 4.3 Clock — Bộ Máy (12)

| Companion | Mới | Bậc | Bond chủ đề | Bond phụ | Vũ khí | Talent / kỹ năng | Bộ hero | Ghi chú |
|---|:-:|---|---|---|---|---|---:|---|
| The Last Bell `RainbowBell` |  | Prismatic | Clock (Bộ Máy) | Morning Bells | **BuzzerBlock** (khóa) | **Kỹ năng riêng:** Giữ kỹ năng hiện có (talent "Pet Master": khi mua tuyển tối đa 4 viên bi bạn). | 9 | chỉ từ quà lăng kính |
| Clockwork Piper |  | Mythic | Clock (Bộ Máy) | Morning Bells | **WindupWrench** (khóa) | **Kỹ năng riêng:** Mỗi 6s tiếng sáo: đồng đội trong tầm 3 +30% tốc đánh 2s. **Talent:** Khi mua: tuyển hai phụ tá nhỏ. | 9 |  |
| Specimen Slime | **Mới** | Rare | Clock (Bộ Máy) | Exhibit Hall | ngẫu nhiên | **Kỹ năng riêng:** Mẫu vật bảo tàng: hạ một loài đã ghi thì tách ra một slime nhỏ (tối đa 2). | 1 | riêng của Archive Curator |
| Spring Duck |  | Rare | Clock (Bộ Máy) | Springstep | ngẫu nhiên | **Talent:** Đồng đội Clockwork Union +4% tốc đánh. | 10 | khởi đầu của Storm Fencer |
| Top Beetle | **Mới** | Rare | Clock (Bộ Máy) | Clockwork Union | SpinningTop (bản 3) | (chưa có mô tả) | 10 | khởi đầu của Bonsai Clocksmith |
| Grand Sprout |  | Uncommon | Clock (Bộ Máy) | Jade Nursery | ngẫu nhiên | **Talent:** 24 sát thương va chạm cơ bản; mang được thưởng vườn ươm. | 1 | riêng của Verdant Matron |
| Lamp Goat | **Mới** | Uncommon | Clock (Bộ Máy) | Little Horns | DragonLanternGlaive (bản 3) | (chưa có mô tả) | 10 |  |
| Button Beetle |  | Common | Clock (Bộ Máy) | Velvet Court | ngẫu nhiên | **Talent:** Mỗi con cùng loài cho nó +10% tốc đánh (tối đa 30%). Nội tại: hồi máu hero mỗi 4s. | 9 | khởi đầu của Velvet Regent |
| Dream Compass |  | Common | Clock (Bộ Máy) | Longbow | ngẫu nhiên | **Talent:** Mỗi lượt đổi trả tiền: +2 vàng và con này +3% sát thương (tối đa 30%). | 10 | khởi đầu của Hourglass Stag |
| Staple Mole |  | Common | Clock (Bộ Máy) | Clockwork Union | ngẫu nhiên | **Talent:** Mỗi cấp Lucky Leaves đang kích hoạt: +4 thu nhập wave. Nội tại: mỗi 4s nhận vàng. | 10 | khởi đầu của Tin Virtuoso |
| Static Ferret | **Mới** | Common | Clock (Bộ Máy) | Static Club | ngẫu nhiên | **Kỹ năng riêng:** Đòn để lại tĩnh điện lan một lần. | 8 |  |
| Tin Tick |  | Common | Clock (Bộ Máy) | Static Club | ngẫu nhiên | **Talent:** Hero +4 máu tối đa; mỗi 8 lần hạ địch con này +1% tốc đánh (tối đa 15% mỗi wave). Nội tại: mỗi 4s làm lạnh địch xung quanh. | 8 | khởi đầu của Button Cavalier; chuyển từ Clockwork Union sang Static Club (v5) |

### 4.4 Frost — Sương Giá (11)

| Companion | Mới | Bậc | Bond chủ đề | Bond phụ | Vũ khí | Talent / kỹ năng | Bộ hero | Ghi chú |
|---|:-:|---|---|---|---|---|---:|---|
| The Impossible Tailor `RainbowTailor` |  | Prismatic | Frost (Sương Giá) | Wild Stitch | **TopiaryShears** (khóa) | **Kỹ năng riêng:** Giữ kỹ năng hiện có (talent "Overtime King": thu nhập wave +15, hero mất 0.5 máu mỗi 5s). | 9 | chỉ từ quà lăng kính |
| Iron Locomotive |  | Mythic | Frost (Sương Giá) | Unbroken | **SnowGlobeGrenade** (khóa) | **Kỹ năng riêng:** Còi băng: một luồng hơi lạnh phía trước đóng băng 3 địch đầu tiên 1s (v5 §4). **Talent:** +40 sát thương va chạm; mỗi 6s dụ quái thường gần đó 2s (boss không bị dụ). | 10 |  |
| Rain Heron | **Mới** | Rare | Frost (Sương Giá) | Rain Script | không cố định (v5 §9) | (chưa có mô tả) | 9 |  |
| Shovel Penguin | **Mới** | Rare | Frost (Sương Giá) | Prizefighters | SnowShovel (bản 3) | (chưa có mô tả) | 10 |  |
| Fossil Armadillo | **Mới** | Uncommon | Frost (Sương Giá) | Exhibit Hall | ngẫu nhiên | **Kỹ năng riêng:** Mẫu vật bảo tàng: cuộn tròn, địch đánh vào nó bị choáng 0.3s. | 1 | riêng của Archive Curator |
| Ice Cream Owl |  | Uncommon | Frost (Sương Giá) | Sweet Tooth | ngẫu nhiên | **Talent:** Đồng đội Runic School bắn đạn to hơn 30%, +2 xuyên, nhưng đánh chậm hơn 15%. Nội tại: mỗi 4s làm lạnh địch xung quanh. | 10 | khởi đầu của Ivory Sovereign |
| Monster Marble |  | Uncommon | Frost (Sương Giá) | Masked Carnival | ngẫu nhiên | **Talent:** Viên bi bạn bất tử, dùng vũ khí được gán ngẫu nhiên. | 9 | chỉ đến từ quà / vườn ươm |
| Boba Tadpole | **Mới** | Common | Frost (Sương Giá) | Sweet Tooth | BobaStraw (bản 3) | (chưa có mô tả) | 10 |  |
| Fridge Wisp |  | Common | Frost (Sương Giá) | Quicksoles | ngẫu nhiên | **Talent:** Mỗi lần hero bị thương trong wave: đồng đội Prizefighters +1% tốc đánh (tối đa 15%). | 9 | khởi đầu của Archive Curator |
| Hood Wisp |  | Common | Frost (Sương Giá) | Rootkin | ngẫu nhiên | **Talent:** Đội +5% tốc chạy. | 9 | nhân vật cốt truyện (chỉ xuất hiện theo luật cốt truyện) |
| Snow Spoon |  | Common | Frost (Sương Giá) | Sweet Tooth | ngẫu nhiên | **Talent:** Mỗi cấp Wild Stitch hoặc Springstep: con này +10% sát thương, +5% tầm. Nội tại: +sát thương mỗi đồng đội trong đội. | 10 |  |

### 4.5 Orchard — Vườn Cây (11)

| Companion | Mới | Bậc | Bond chủ đề | Bond phụ | Vũ khí | Talent / kỹ năng | Bộ hero | Ghi chú |
|---|:-:|---|---|---|---|---|---:|---|
| The First Seed `FirstSeed` | **Mới** | Prismatic | Orchard (Vườn Cây) | Rootkin | **AcornCluster** (khóa) | **Kỹ năng riêng:** Mọc một bờ rào gai phía trước chặn đường quái 3s (v5 §5). | 10 | chỉ từ quà lăng kính |
| Paper Inventor |  | Mythic | Orchard (Vườn Cây) | Clockwork Union | **GrabberClaw** (khóa) | **Kỹ năng riêng:** Mỗi trận dựng 1 tháp giấy tự bắn phía trước nó (v3 §4.4). **Talent:** Khi mua: nhận 100 vàng một lần. | 11 |  |
| Yo-yo Squirrel | **Mới** | Rare | Orchard (Vườn Cây) | Springstep | YoYo (bản 3) | (chưa có mô tả) | 9 |  |
| Bristle Ball |  | Uncommon | Orchard (Vườn Cây) | Little Horns | ngẫu nhiên | **Talent:** 18 sát thương va chạm cơ bản; +1 mỗi lần hero bị thương trong wave (tối đa 20). | 9 | chỉ đến từ quà / vườn ươm |
| Citrus Toad |  | Uncommon | Orchard (Vườn Cây) | Springstep | ngẫu nhiên | **Talent:** Đồng đội Quicksoles +10% tốc chạy và tốc đánh. Nội tại: mỗi 4s nhận vàng. | 9 | khởi đầu của Citrus Marshal |
| Paper Devil |  | Uncommon | Orchard (Vườn Cây) | Little Horns | ngẫu nhiên | **Talent:** Không merge được. Mỗi 8 lần con này hạ địch: hero +2% tốc đánh (tối đa 20% trong wave). | 9 | nhân vật cốt truyện (chỉ xuất hiện theo luật cốt truyện) |
| Pear Moth |  | Uncommon | Orchard (Vườn Cây) | Morning Bells | ngẫu nhiên | **Talent:** Mỗi cấp Morning Bells: hero +1 viên đạn (tối đa +3). Nội tại: hồi máu hero mỗi 4s. | 12 | khởi đầu của Petal Citadel |
| Watering Snail | **Mới** | Uncommon | Orchard (Vườn Cây) | Jade Nursery | WateringCan (bản 3) | (chưa có mô tả) | 9 |  |
| Mushroom Librarian |  | Common | Orchard (Vườn Cây) | Lucky Leaves | ngẫu nhiên | **Talent:** Đòn thường làm chậm quái thường 0.4s. Nội tại: hồi máu hero mỗi 4s. | 9 | khởi đầu của The Root Oracle |
| Radish Jester |  | Common | Orchard (Vườn Cây) | Masked Carnival | ngẫu nhiên | **Talent:** Đòn thường tẩm độc: 1s sau giáng thêm 25% sát thương vũ khí. Nội tại: +sát thương mỗi đồng đội trong đội. | 11 |  |
| Seed Hedgehog |  | Common | Orchard (Vườn Cây) | Rootkin | ngẫu nhiên | **Talent:** Hạ địch bằng đòn thường có 30% hồi 1 máu cho hero. Nội tại: mỗi 4s làm lạnh địch xung quanh. | 10 | khởi đầu của Thorn Treasurer |

### 4.6 Astral — Tinh Tú (11)

| Companion | Mới | Bậc | Bond chủ đề | Bond phụ | Vũ khí | Talent / kỹ năng | Bộ hero | Ghi chú |
|---|:-:|---|---|---|---|---|---:|---|
| Eclipse Egg |  | Prismatic | Astral (Tinh Tú) | Morning Bells | **SaucerBlaster** (khóa) | **Kỹ năng riêng:** Mỗi trận nở thành 1 trong 3 dạng (tầm xa / cận chiến / hỗ trợ), giữ dạng đó cả trận (v3 §4.4). **Talent:** Mỗi Princess đi cùng cho con này +1 viên đạn (tối đa 3). Nội tại: +sát thương mỗi đồng đội trong đội. | 11 | chỉ từ quà lăng kính |
| Pillow Oracle |  | Mythic | Astral (Tinh Tú) | Hat Society | **FortuneGlobe** (khóa) | **Kỹ năng riêng:** Mỗi 8s ru ngủ địch mạnh nhất gần nó 1.5s. **Talent:** Đồng đội Clockwork Union +8% tốc chạy và tốc đánh. Nội tại: +sát thương mỗi đồng đội trong đội. | 10 |  |
| Dough Foreman |  | Rare | Astral (Tinh Tú) | Hat Society | ngẫu nhiên | **Talent:** Đội +20% sát thương, +10% tốc độ; địch nhanh hơn 10%. | 10 |  |
| Moon Jelly |  | Rare | Astral (Tinh Tú) | Longbow | ngẫu nhiên | **Talent:** Hero −0.08 giáp; mỗi tim nhặt được con này +2% tầm (tối đa 20%). | 9 |  |
| The Sleeping Friend `SecretDreamLamb` |  | Rare | Astral (Tinh Tú) | Wild Stitch | ngẫu nhiên | **Talent:** Khách vườn trong hai wave thường thắng; sau đó rời đi, hỗ trợ kết thúc. | 10 | cư dân sự kiện bí mật (Kho Đồ Thất Lạc) |
| Little Sprout |  | Uncommon | Astral (Tinh Tú) | Jade Nursery | ngẫu nhiên | **Talent:** Chỉ có ở vườn ươm; 12 sát thương va chạm cơ bản. Ba mầm lớn thành một đồng đội 2★ ngẫu nhiên và một Grand Sprout. | 1 | riêng của Verdant Matron; khởi đầu của Verdant Matron |
| Star Sock |  | Uncommon | Astral (Tinh Tú) | Quicksoles | ngẫu nhiên | **Talent:** Mỗi rương mở: con này +2% tốc đánh (tối đa 20%). Nội tại: hồi máu hero mỗi 4s. | 9 | khởi đầu của Moon Prowler |
| Storm Kitten | **Mới** | Uncommon | Astral (Tinh Tú) | Static Club | ngẫu nhiên | **Kỹ năng riêng:** Mỗi 6s một đám mây phía trước giáng một tia sét. | 8 |  |
| Comet Kite |  | Common | Astral (Tinh Tú) | Longbow | ngẫu nhiên | **Talent:** Mỗi rương mở: con này +2% sát thương (tối đa 30%). Nội tại: mỗi 4s nhận vàng. | 9 | khởi đầu của The Comet Courier |
| Orbit Moth |  | Common | Astral (Tinh Tú) | Quicksoles | ngẫu nhiên | **Talent:** Mỗi Prince đi cùng cho con này +8% tầm (tối đa 24%). Nội tại: mỗi 4s làm lạnh địch xung quanh. | 9 |  |
| Paper Plane Pigeon | **Mới** | Common | Astral (Tinh Tú) | Paper Wings | PaperPlaneSquadron (bản 3) | (chưa có mô tả) | 9 | khởi đầu của Kite Admiral |

### 4.7 Tidal — Thủy Triều (13)

| Companion | Mới | Bậc | Bond chủ đề | Bond phụ | Vũ khí | Talent / kỹ năng | Bộ hero | Ghi chú |
|---|:-:|---|---|---|---|---|---:|---|
| Kite Dragon | **Mới** | Prismatic | Tidal (Thủy Triều) | Paper Wings | **TwinMoonStaff** (khóa) | **Kỹ năng riêng:** Bay trên cao không bị chặn; mỗi 5s quét một dải gió cắt dọc sân phía trước. | 9 | chỉ từ quà lăng kính |
| Harbor Whale `LighthouseWhale` | **Mới** | Mythic | Tidal (Thủy Triều) | Lantern Guild | **PaperLanternBomb** (khóa) | **Kỹ năng riêng:** Mỗi 6s phun cột nước vào địch gần hero nhất, làm ướt và hất lùi (v5 §5). | 11 | tên cũ Lighthouse Whale, art giữ nguyên |
| Return Captain |  | Mythic | Tidal (Thủy Triều) | Crossing Knights | **PlushShark** (khóa) | **Kỹ năng riêng:** Thả neo vào một địch: đồng đội đánh địch đó +20%. **Talent:** Sau 30s trong wave, gây gấp ba sát thương vũ khí đến hết wave. | 10 |  |
| Volt Eel | **Mới** | Rare | Tidal (Thủy Triều) | Static Club | ngẫu nhiên | **Kỹ năng riêng:** Địch bị sốc lan thêm 1 lần. | 10 |  |
| Anchor Snail |  | Uncommon | Tidal (Thủy Triều) | Crossing Knights | ngẫu nhiên | **Talent:** Mỗi đồ ăn mua được: đội +1 sát thương cộng thẳng; kèm một đánh đổi máu nhỏ ở wave sau. Nội tại: mỗi 4s làm lạnh địch xung quanh. | 10 | khởi đầu của The Tide Singer |
| Balloon Mallet |  | Uncommon | Tidal (Thủy Triều) | Big Top | ngẫu nhiên | **Talent:** Đẩy quái thường; sát thương cơ bản giảm 70%. | 11 | chỉ đến từ quà / vườn ươm |
| Coral Teacup |  | Uncommon | Tidal (Thủy Triều) | Lucky Leaves | ngẫu nhiên | **Talent:** Đòn thường chí mạng có 25% cho 1 vàng (hồi chung 0.8s). Nội tại: mỗi 4s nhận vàng. | 11 |  |
| Raincoat Frog | **Mới** | Uncommon | Tidal (Thủy Triều) | Rain Script | PlungerPopper (bản 3) | (chưa có mô tả) | 8 | khởi đầu của Bell Diver |
| Ribbon Otter |  | Uncommon | Tidal (Thủy Triều) | Rain Script | ngẫu nhiên | **Talent:** Nhặt tim hồi thêm 1 máu. | 8 | khởi đầu của Rain Archivist |
| Sail Squid |  | Uncommon | Tidal (Thủy Triều) | Unbroken | ngẫu nhiên | **Talent:** Mỗi đòn con này +1% tốc đánh (tối đa 20% trong wave). | 11 | khởi đầu của Soap Skater |
| Bubble Koi |  | Common | Tidal (Thủy Triều) | Lucky Leaves | ngẫu nhiên | **Talent:** Mỗi 5s trong trận rơi một tim 1 máu. Nội tại: +sát thương mỗi đồng đội trong đội. | 11 |  |
| Shanty Sea Lion | **Mới** | Common | Tidal (Thủy Triều) | Sea Shanty | ngẫu nhiên | **Kỹ năng riêng:** Mỗi đòn thứ 3 một nốt nhạc đẩy địch lùi. | 8 |  |
| Shell Postman |  | Common | Tidal (Thủy Triều) | Sea Shanty | ngẫu nhiên | **Talent:** Bắn thêm 2 viên; mỗi viên yếu hơn 35%. Nội tại: hồi máu hero mỗi 4s. | 8 | nhân vật cốt truyện (chỉ xuất hiện theo luật cốt truyện); chuyển từ Lantern Guild sang Sea Shanty (v5) |

### 4.8 Oddity — Kỳ Lạ (11)

| Companion | Mới | Bậc | Bond chủ đề | Bond phụ | Vũ khí | Talent / kỹ năng | Bộ hero | Ghi chú |
|---|:-:|---|---|---|---|---|---:|---|
| The Lost Carousel `LostCarousel` | **Mới** | Prismatic | Oddity (Kỳ Lạ) | Big Top | **CherryFuseBomb** (khóa) | **Kỹ năng riêng:** Mỗi 10s dựng vòng ngựa gỗ quanh chỗ địch đông nhất: ngựa chạy vòng đẩy và gây sát thương. | 10 | chỉ từ quà lăng kính |
| Mirror Newt |  | Mythic | Oddity (Kỳ Lạ) | Unbroken | **KunaiFan** (khóa) | **Kỹ năng riêng:** 25% đạn địch chạm vào nó bị dội ngược. **Talent:** Đồng đội nhỏ hơn 12% (hitbox nhỏ theo). | 10 |  |
| Ringmaster Jack `JackInTheBox` | **Mới** | Mythic | Oddity (Kỳ Lạ) | Big Top | **PinataBomb** (khóa) | **Kỹ năng riêng:** Gây chú ý: mỗi 6s xoay bóng trên mũi, quái gần quay sang đánh nó 1.5s (v5 §5). | 10 | tên cũ Jack-in-the-Box, art giữ nguyên |
| Carousel Pony |  | Uncommon | Oddity (Kỳ Lạ) | Big Top | ngẫu nhiên | **Talent:** Chở một đồng đội gần đó, cả hai vẫn đánh (tự ghép cặp). | 10 |  |
| Violet Remnant |  | Uncommon | Oddity (Kỳ Lạ) | Unbroken | ngẫu nhiên | **Talent:** Con này +15% chí mạng. | 9 | khởi đầu của The Mirror Keeper |
| Crayon Gremlin |  | Common | Oddity (Kỳ Lạ) | Masked Carnival | ngẫu nhiên | **Talent:** Khi mua: nhận 6 trứng một lần. Nội tại: mỗi 4s làm lạnh địch xung quanh. | 11 |  |
| Dice Crab |  | Common | Oddity (Kỳ Lạ) | Lucky Leaves | ngẫu nhiên | **Talent:** Giảm 25% đòn đầu tiên hero nhận mỗi 6s. Nội tại: mỗi 4s nhận vàng. | 11 | khởi đầu của Lady Loaded Dice |
| Jelly Swordsman |  | Common | Oddity (Kỳ Lạ) | Crossing Knights | ngẫu nhiên | **Talent:** Đội +5% sát thương. | 9 | khởi đầu của The Lone Jelly |
| Mask Pepper |  | Common | Oddity (Kỳ Lạ) | Masked Carnival | ngẫu nhiên | **Talent:** Mỗi wave một lần: bị thương thì máu tối đa −2 và hồi đầy (không hồi máu hoàng gia). Nội tại: +sát thương mỗi đồng đội trong đội. | 11 | khởi đầu của Night Maw |
| Ribbon Spider |  | Common | Oddity (Kỳ Lạ) | Wild Stitch | ngẫu nhiên | **Talent:** Khi mua: nhận một thưởng run ngẫu nhiên (sát thương, tốc độ, may mắn hoặc giáp). Nội tại: hồi máu hero mỗi 4s. | 9 | khởi đầu của China Marionette |
| Sock Cyclops |  | Common | Oddity (Kỳ Lạ) | Quicksoles | ngẫu nhiên | **Talent:** Mỗi Carousel Pony cho con này +30% sát thương và +5 sát thương va chạm (tối đa 3 ngựa). Nội tại: hồi máu hero mỗi 4s. | 12 | khởi đầu của Patchwork Host |

## 5. Vũ khí Arsenal (60)

47 vũ khí "Bizarre Arsenal" đang trong game (số liệu và mô tả từ `Editor/ArsenalSpecs.cs`) + 13 vũ khí mới của đợt mở rộng. Sát thương / tầm / hồi chiêu là số bậc 1 trong asset. "Người cầm cố định": vũ khí của hero (`HeroLoadout.cs`) hoặc vũ khí khóa của companion Mythic / Prismatic (v5 §9); vũ khí không có người cầm cố định nằm trong vòng quay vũ khí của companion.

| # | Vũ khí | Mới | Kiểu | Hành vi | Sát thương · tầm · hồi chiêu | Người cầm cố định |
|---:|---|:-:|---|---|---|---|
| 1 | CherryFuseBomb |  | Tầm xa (ném vòng (lob)) | Ném bom ngòi: chạm đất, xì khói rồi nổ. | 14.8 · 5 · 1.35s | khóa: The Lost Carousel (Prismatic) |
| 2 | PopperSticks |  | Tầm xa (ném vòng, mìn) | Ném chậm một bó pháo; chạm đất rồi nổ rộng. | 9 · 3.2 · 1.25s | — |
| 3 | DreamRacket |  | Tầm xa (đạn) | Giao một quả bóng nảy qua 4 địch. | 25.6 · 5 · 1s | khóa: Reel Owl (Mythic) |
| 4 | BambooVase |  | Tầm xa (ném, rồi thành hàng) | Bình vỡ; một hàng tre mọc lên. | 44.1 · 4.6 · 1.3s | hero The Root Oracle |
| 5 | RoseBouquet |  | Tầm xa (ném, để vệt) | Bó hoa ném ra để lại dải cánh hoa làm chậm. | 14.6 · 4.5 · 1.15s | hero Petal Citadel |
| 6 | HomeRunBat |  | Cận chiến | Đánh bay một địch vào những địch khác. | 30.8 · 1.5 · 1.2s | — |
| 7 | CandyDrill |  | Tầm xa (đạn) | Xuyên 4 địch rồi khoan xuống sàn. | 13.5 · 5 · 1.2s | — |
| 8 | InkyOctopus |  | Tầm xa (triệu hồi) | Bạch tuộc bông bám vào một địch và phun mực ra sàn. | 35 · 4.5 · 0.9s | hero The Lone Jelly |
| 9 | GrabberClaw |  | Tầm xa (đạn) | Gắp một địch, kéo về rồi quăng đi. | 25.7 · 4 · 1.3s | khóa: Paper Inventor (Mythic) |
| 10 | StarforgeClaymore |  | Cận chiến | Nhát quét rất chậm và rộng phía trước; lưỡi kiếm chặn đạn. | 10.1 · 2.2 · 1.6s | hero Velvet Regent |
| 11 | DragonLanternGlaive |  | Cận chiến | Quét, quét lại, rồi một nhát đâm dài. | 18.3 · 2 · 0.95s | Lamp Goat (bản 3) |
| 12 | TwinMoonStaff |  | Tầm xa (đạn) | Gậy hai lưỡi ném ra lơ lửng, nghiến rồi bay về. | 10 · 3.6 · 1.25s | khóa: Kite Dragon (Prismatic) |
| 13 | PlushShark |  | Tầm xa (đạn) | Ném vào địch xa nhất; đớp 3 địch trên đường. | 29.5 · 5.5 · 1.45s | khóa: Return Captain (Mythic) |
| 14 | DrumstickClub |  | Cận chiến | Nện sàn phía trước; một sóng chấn chạy về trước. | 20 · 1.6 · 1.15s | hero Night Maw |
| 15 | PaperShuriken |  | Tầm xa (đạn) | 3 sao giấy, mỗi cái nảy thêm 2 địch. | 14.5 · 5 · 0.95s | khóa: The Endless Index (Prismatic) |
| 16 | SquirtTrio |  | Tầm xa (đạn) | 3 tia nước nảy tường và làm chậm. | 16.2 · 6 · 0.85s | hero Rain Archivist |
| 17 | ConfettiBlunderbuss |  | Tầm xa (đạn) | 7 viên kẹo bắn hình nón ngắn. | 32.2 · 3.2 · 1.25s | hero Patchwork Host |
| 18 | PlungerPopper |  | Tầm xa (đạn) | Dính vào địch, kéo lại rồi bật ra. | 42 · 4.2 · 1.3s | Raincoat Frog (bản 3) |
| 19 | SignpostSlammer |  | Cận chiến | Mỗi nhát thứ 3 cắm một biển báo làm chậm. | 12.5 · 1.6 · 0.95s | hero Citrus Marshal |
| 20 | DetonatorBox |  | Tầm xa (đặt bẫy) | Đặt 3 quả bom đồ chơi rồi kích nổ cùng lúc. | 17.6 · 5 · 1.05s | hero Penny Broker |
| 21 | FirecrackerPinwheel |  | Tầm xa (đặt bẫy) | Đặt bẫy chong chóng phun tia lửa rồi nổ. | 23.8 · 4 · 1.2s | — |
| 22 | GumballGatling |  | Tầm xa (đạn) | Bắn kẹo cao su liên thanh; nòng càng nóng càng nhanh. | 8.44 · 5 · 0.32s | hero Thorn Treasurer |
| 23 | BalloonBuddy |  | Tầm xa (đạn) | Bóng bay trôi vào đám đông rồi nổ. | 11.8 · 5 · 1.4s | — |
| 24 | GlitchToybox |  | Tầm xa (đặc biệt) | Mỗi đòn mượn một vũ khí ngẫu nhiên. | 39 · 4.5 · 1s | hero Lady Loaded Dice |
| 25 | SaucerBlaster |  | Tầm xa (đạn) | Bắn đĩa bay đồ chơi xuyên 4 địch. | 17 · 5.5 · 1.05s | khóa: Eclipse Egg (Prismatic) |
| 26 | FanfareHorn |  | Tầm xa (đạn hình quạt) | Thổi ba vây âm thanh vàng về phía trước. | 9.48 · 3.5 · 1.15s | — |
| 27 | WindupWrench |  | Cận chiến | Mỗi đòn thứ 3 lên dây cót một địch; nó xoay chóng mặt. | 18.1 · 1.5 · 0.95s | khóa: Clockwork Piper (Mythic) |
| 28 | CorkRifle |  | Tầm xa (đạn) | Một viên nút chai dài xuyên cả hàng. | 8.52 · 8 · 1.55s | — |
| 29 | CatcherMitt |  | Tầm xa (đạn) | Bắt đạn địch phía trước và ném trả. | 31.2 · 5 · 1.1s | khóa: Coil Kettle (Mythic) |
| 30 | FizzBottle |  | Tầm xa (ném vòng (lob)) | Chai vỡ thành vũng sủi bọt làm chậm. | 28.6 · 4.5 · 1.25s | — |
| 31 | LeekBaton |  | Cận chiến | Nhát quét rộng, rát, đẩy và làm chậm. | 15.3 · 1.7 · 0.85s | — |
| 32 | DowsingRod |  | Tầm xa (đạn) | Tìm địch yếu nhất trong tầm và làm lộ nó. | 18.7 · 5.5 · 1.05s | — |
| 33 | StarKnuckle |  | Cận chiến | Đấm nhanh; mỗi cú thứ 4 là cú móc sao. | 18.2 · 1.2 · 0.55s | khóa: Sundae Golem (Prismatic) |
| 34 | BuzzerBlock |  | Tầm xa (đặt bẫy) | Nút chuông chọc khăm trên sàn giật điện một chuỗi địch. | 22.8 · 4 · 1.15s | khóa: The Last Bell (Prismatic) |
| 35 | FlySwatter |  | Cận chiến | Đập dẹt và làm choáng; mỗi nhát thứ 3 nghiền địch yếu. | 16 · 1.4 · 0.9s | — |
| 36 | PawMitten |  | Cận chiến | Cào rất nhanh, rất ngắn; mỗi cú thứ 5 cào ba vệt. | 16 · 1 · 0.42s | — |
| 37 | HearthBrick |  | Tầm xa (ném vòng (lob)) | Ném gạch: rơi xuống, làm choáng và đẩy thứ nó rơi trúng. | 26.6 · 4.2 · 1.35s | khóa: Tea Dragon (Mythic) |
| 38 | VerdictGavel |  | Cận chiến | Nện phán quyết phía trước; may mắn và xu đã nhặt làm nó nặng hơn. | 31.5 · 1.3 · 1.05s | — |
| 39 | TalismanIngot |  | Tầm xa (đạn) | Ném thỏi vàng dán bùa từng loạt 4; mỗi thỏi nổ nhỏ. | 3.96 · 4.5 · 0.5s | — |
| 40 | KunaiFan |  | Tầm xa (đạn) | Ném ba kunai nối nhau trên một đường. | 13.2 · 6 · 1.1s | khóa: Mirror Newt (Mythic) |
| 41 | PaperLanternBomb |  | Tầm xa (ném vòng (bom)) | Bom đèn lồng giấy trôi chậm tới rồi nổ rộng. | 16 · 4.8 · 1.6s | khóa: Harbor Whale (Mythic) |
| 42 | SnowGlobeGrenade |  | Tầm xa (ném vòng (bom)) | Quả cầu tuyết vỡ nơi rơi, làm chậm một vùng nhỏ. | 12 · 4.4 · 1.3s | khóa: Iron Locomotive (Mythic) |
| 43 | PinataBomb |  | Tầm xa (ném vòng (bom)) | Piñata nổ, rải bom kẹo nhỏ xung quanh. | 11 · 4.5 · 1.5s | khóa: Ringmaster Jack (Mythic) |
| 44 | AcornCluster |  | Tầm xa (ném vòng (bom)) | Ba hạt dẻ buộc chùm tách ra giữa không trung và nổ quanh mục tiêu. | 10 · 4.6 · 1.25s | khóa: The First Seed (Prismatic) |
| 45 | FortuneGlobe |  | Tầm xa (đạn) | Ba hạt pha lê lang thang bay ra, càng bay càng to và mạnh. | 10 · 5 · 1.2s | khóa: Pillow Oracle (Mythic) |
| 46 | TopiaryShears |  | Cận chiến | Hai nhát cắt nhanh theo một đường phía trước; mỗi lần thứ 3 tỉa địch cứng nhất. | 10 · 1.4 · 0.9s | khóa: The Impossible Tailor (Prismatic) |
| 47 | BareKnuckles |  | Cận chiến | Không vũ khí: đấm rất nhanh; cú chí mạng nổ không khí phía trước. | 14.2 · 0.95 · 0.38s | — |
| 48 | KiteSpool | **Mới** | Tầm xa (vật thể tự bay) | Diều bay vòng quanh một điểm phía trước 2.5s; sợi chỉ cắt địch băng qua; đâm xuống cuối đòn. Tối đa cùng lúc: 1 diều. *(nguồn v3 §5; art đã vẽ)* | — | hero Kite Admiral |
| 49 | ScoopLauncher | **Mới** | Tầm xa (ném vòng (lob)) | Ném kem vào chỗ địch sẽ tới; vũng kem chậm 30% 2s; 3 vị luân phiên (lửa, băng, vật lý). Tối đa cùng lúc: 3 vũng. *(nguồn v3 §5; art đã vẽ)* | — | hero Sundae Duchess |
| 50 | CuckooCannon | **Mới** | Tầm xa (đạn) | Bắn quả lắc; phát thứ 3 chim cúc cu lao xuống địch xa nhất. Tối đa cùng lúc: 1 chim. *(nguồn v3 §5; art đã vẽ)* | — | hero Bonsai Clocksmith |
| 51 | JugglingPins | **Mới** | Tầm xa (ném) | Chùy trúng khi bay lên và khi rơi xuống; chùy kế chọn mục tiêu khác. Tối đa cùng lúc: 2 chùy. *(nguồn v5 §5; art đã vẽ)* | — | hero Juggling Jester |
| 52 | SonarHarpoon | **Mới** | Tầm xa (đạn) | Lao để lại một phao dò; địch đi qua phao bị dò (không còn đánh dấu trực tiếp). Tối đa cùng lúc: 2 lao. *(nguồn v5 §4/§8; art đã vẽ)* | — | hero Bell Diver |
| 53 | YoYo | **Mới** | Tầm xa (đạn, quay về) | Quăng tới địch rồi giật về, trúng 2 lần. Tối đa cùng lúc: 1. *(nguồn v3 §5; art đã vẽ)* | — | Yo-yo Squirrel (bản 3) |
| 54 | RollingPin | **Mới** | Chưa xếp loại: lăn phía trước | Để lại một dải bột dính giữ chân địch 0.5s. Tối đa cùng lúc: 1. *(nguồn v5 §4; art đã vẽ)* | — | Baker Mouse (bản 3) |
| 55 | BobaStraw | **Mới** | Tầm xa (đạn liên tục) | Dòng trân châu lượn hình sóng, trúng mọi thứ trên đường lượn. Tối đa cùng lúc: 8 viên. *(nguồn v5 §5; art đã vẽ)* | — | Boba Tadpole (bản 3) |
| 56 | PaperPlaneSquadron | **Mới** | Tầm xa (vật thể tự bay) | 3 máy bay giấy đuổi 3 địch khác nhau. Tối đa cùng lúc: 3. *(nguồn v3 §5; art đã vẽ)* | — | Paper Plane Pigeon (bản 3) |
| 57 | WateringCan | **Mới** | Chưa xếp loại: tưới phía trước | Làm ướt; địch ướt trúng đòn băng thì đóng băng (không còn mọc mầm). Tối đa cùng lúc: —. *(nguồn v5 §5; art đã vẽ)* | — | Watering Snail (bản 3) |
| 58 | SnowShovel | **Mới** | Tầm xa (hất tảng tuyết) | Hất tảng tuyết phía trước, chậm 40% 1.5s. Tối đa cùng lúc: 1. *(nguồn v3 §5; art đã vẽ)* | — | Shovel Penguin (bản 3) |
| 59 | SunLens | **Mới** | Tầm xa (vật thể tự đi) | Chấm nắng bò trên địch đốt liên tục; giữ 1.5s trên một con thì bốc cháy. Tối đa cùng lúc: 1 chấm. *(nguồn v3 §5; art đã vẽ)* | — | Lens Owl (bản 3) |
| 60 | SpinningTop | **Mới** | Tầm xa (vật thể tự đi) | Con quay xoáy tại chỗ, hất quái nhỏ văng ra ngoài. Tối đa cùng lúc: 1. *(nguồn v5 §8; art đã vẽ)* | — | Top Beetle (bản 3) |

Vũ khí khóa (v5 §9): Pillow Oracle → FortuneGlobe · Reel Owl → DreamRacket · Clockwork Piper → WindupWrench · Coil Kettle → CatcherMitt · Iron Locomotive → SnowGlobeGrenade · Paper Inventor → GrabberClaw · Return Captain → PlushShark · Mirror Newt → KunaiFan · The Last Bell → BuzzerBlock · The Impossible Tailor → TopiaryShears · Eclipse Egg → SaucerBlaster · Tea Dragon → HearthBrick · Harbor Whale → PaperLanternBomb · Ringmaster Jack → PinataBomb · Kite Dragon → TwinMoonStaff · Sundae Golem → StarKnuckle · The Lost Carousel → CherryFuseBomb · The Endless Index → PaperShuriken · The First Seed → AcornCluster. Không vũ khí khóa nào là vũ khí của hero hay của companion khởi đầu.

## 6. Item (200)

Cột "Tác dụng": chỉ số khi sở hữu (mỗi bản), rồi cơ chế đặc biệt. Phạm vi: Đội / Hero / Đồng đội / Run. Item Common không gắn chủ đề (mọi hero đều thấy). Giá theo `expansion.json` (item mới chưa định giá: khung giá theo bậc là Common 15–32, Uncommon 24–46, Rare 30–60, Mythic 48–75, Prismatic 200 — Prismatic chỉ từ quà lăng kính). Loại: Curio (đồ vật), Drink (đồ uống), Food (đồ ăn).

### 6.1 Common (43, mới 3)

| Item | Mới | Loại | Tác dụng | Chủ đề | Họ | Giá |
|---|:-:|---|---|---|---|---:|
| Bent Tape `CurioBentTape` |  | Curio | Đội: Tốc đánh +18% · Wave sau: +10% số quái mỗi bản mua. | chung (Common) | — | 32 |
| Breath Jar `CurioBreathJar` |  | Drink | Đội: Sát thương +20%; Đội: Tốc đánh -4% | chung (Common) | — | 32 |
| Clock Spring `clock-spring` |  | Curio | Đội: Tốc đánh +20% | chung (Common) | — | 20 |
| Clockseed Brooch `OctoberRelic2` |  | Curio | Đội: Sát thương va chạm +5; Đội: Tốc chạy -4% | chung (Common) | — | 32 |
| Coin Compass `coin-compass` |  | Curio | Run: Tầm nhặt +1.25 | chung (Common) | — | 15 |
| Contract · Attack range `k10-attackrange` |  | Curio | Đội: Tầm đánh +25%; Hero: Máu tối đa -1 | chung (Common) | — | 24 |
| Contract · Bullet pierce `k10-bulletpierce` |  | Curio | Đội: Xuyên +2; Hero: Máu tối đa -1 | chung (Common) | — | 21 |
| Contract · Bullet size `k10-bulletsize` |  | Curio | Đội: Cỡ đạn +25%; Hero: Máu tối đa -1 | chung (Common) | — | 18 |
| Contract · Collision damage `k10-collisiondamage` |  | Curio | Đội: Sát thương va chạm +10; Hero: Máu tối đa -1 | chung (Common) | — | 27 |
| Contract · Damage correction `k10-damagecorrection` |  | Curio | Đội: Hệ số sát thương +20%; Hero: Máu tối đa -1 | chung (Common) | — | 27 |
| Contract · Enemy count `k10-enemycount` |  | Curio | Run: Số quái +10%; Run: Hệ số điểm +20%; Hero: Máu tối đa -1 | chung (Common) | — | 24 |
| Contract · Enemy speed `k10-enemyspeed` |  | Curio | Run: Tốc độ quái +10%; Run: Hệ số điểm +20%; Hero: Máu tối đa -1 | chung (Common) | — | 18 |
| Contract · Healing correction `k10-healcorrection` |  | Curio | Hero: Hệ số hồi máu +8%; Hero: Máu tối đa -1 | chung (Common) | — | 24 |
| Contract · Heart healing `k10-hearthealing` |  | Curio | Hero: Hồi máu từ tim +2; Hero: Máu tối đa -1 | chung (Common) | — | 21 |
| Contract · Income `k10-income` |  | Curio | Run: Thu nhập +4; Hero: Máu tối đa -1 | chung (Common) | — | 18 |
| Contract · Knockback `k10-knockback` |  | Curio | Đội: Đẩy lùi +25%; Hero: Máu tối đa -1 | chung (Common) | — | 27 |
| Contract · Relic discount `k10-relicdiscount` |  | Curio | Run: Giảm giá relic +8%; Hero: Máu tối đa -1 | chung (Common) | — | 18 |
| Coral Broth `CurioCoralBroth` |  | Drink | Hero: Hồi máu mỗi giây +1; Hero: Máu tối đa -4 | chung (Common) | — | 32 |
| Coupon Book `coupon-book` |  | Curio | Run: Giảm giá shop vàng +5% | chung (Common) | — | 24 |
| Fang Pin `CurioFangPin` |  | Curio | Hero: Đẩy lùi +100%; Hero: Tốc đánh +20% | chung (Common) | — | 32 |
| Felt Boots `felt-boots` |  | Curio | Đội: Tốc chạy +10% | chung (Common) | — | 18 |
| Fortune Egg `fortune-egg` |  | Curio | Run: May mắn +0.2 | chung (Common) | — | 24 |
| Grave Chime `CurioGraveChime` |  | Drink | Hero: Sát thương va chạm +10; Hero: Tốc chạy -5% | chung (Common) | — | 32 |
| Honey Pot `honey-pot` |  | Curio | Hero: Máu tối đa +40 | chung (Common) | — | 22 |
| Key Oil `key-oil` |  | Curio | Đội: Sát thương +24% | chung (Common) | — | 18 |
| Kindling Gloves `CurioKindlingGloves` |  | Curio | Hero: Hồi máu mỗi giây -0.4 · Mỗi lần nhặt xu: 15% hồi 2 máu mỗi bản. | chung (Common) | — | 32 |
| Lantern Stitch `OctoberRelic0` |  | Curio | Đội: Tầm đánh +10%; Đội: Tốc đánh -5% | chung (Common) | — | 26 |
| Marrow Tea `K26Defense0` |  | Curio | Hero: Máu tối đa +50; Hero: Hồi máu mỗi giây +0.8 | chung (Common) | — | 32 |
| Meadow Tonic `CurioMeadowTonic` |  | Drink | Hero: Né +8%; Run: May mắn +0.06 | chung (Common) | — | 32 |
| Noon Spark `CurioNoonSpark` |  | Drink | Đội: Sát thương cận chiến (cộng thẳng) +5; Đội: Chí mạng +6% | chung (Common) | — | 32 |
| Oracle Lens `oracle-lens` |  | Curio | Đội: Chí mạng +10% | chung (Common) | — | 22 |
| Orchard Fizz `CurioOrchardFizz` |  | Drink | Hero: Máu tối đa +20; Run: Thu nhập -2 | chung (Common) | — | 32 |
| Piggy Mail `piggy-mail` |  | Curio | Run: Trần lãi +2 | chung (Common) | — | 18 |
| Pillow Mail `pillow-mail` |  | Curio | Hero: Giáp +0.22 | chung (Common) | — | 20 |
| Porcelain Splint `K26Defense1` |  | Curio | Hero: Giáp +0.35; Đội: Tốc chạy -4% | chung (Common) | — | 32 |
| Quiet Hour `CurioQuietHour` |  | Drink | Hero: Giáp +0.12; Run: Dò kho báu +0.15 | chung (Common) | — | 32 |
| Ricochet Seltzer `CurioRicochetSeltzer` |  | Drink | Đội: Sát thương tầm xa (cộng thẳng) +6; Đội: Tỉ lệ nảy +25%; Đội: Sát thương -5% | chung (Common) | — | 32 |
| Velvet First Aid `velvet-first-aid` |  | Curio | Run: Dò kho báu +0.1; Hero: Tốc chạy -2% · Mở rương: hồi 4 máu (hồi 2s) | chung (Common) | — | 24 |
| Wandering Map `wandering-map` |  | Curio | Run: Bán kính tuần tra +0.4 | chung (Common) | — | 18 |
| Warm Tea `warm-tea` |  | Curio | Hero: Hồi máu mỗi giây +1 | chung (Common) | — | 24 |
| Coin Sash `CoinSash` | **Mới** | — | +0.5 tầm nhặt khi đang chạy. *(v5 §6b)* *(art đã vẽ)* | chung (Common) | — | chưa định giá |
| Fish Cracker `FishCracker` | **Mới** | Food | Đồ ăn: +6% chí mạng. *(v5 §6b)* *(art đã vẽ)* | chung (Common) | — | chưa định giá |
| Tally Chalk `TallyChalk` | **Mới** | — | +1% sát thương mỗi 25 quái hạ trong wave, tối đa 8%. *(v5 §6b)* *(art đã vẽ)* | chung (Common) | — | chưa định giá |

### 6.2 Uncommon (66, mới 5)

| Item | Mới | Loại | Tác dụng | Chủ đề | Họ | Giá |
|---|:-:|---|---|---|---|---:|
| Brigade Banner `brigade-banner` |  | Curio | Đồng đội: Sát thương +30% | Bếp Ấm | Đội | 30 |
| Citrus Storm `CurioCitrusStorm` |  | Drink | Đội: Sát thương tầm xa (cộng thẳng) +6; Run: Số quái +2% | Thủy Triều | Đồ uống, Tầm xa | 46 |
| Clear Sip `CurioClearSip` |  | Drink | Hero: Sát thương va chạm -1 · Mỗi đồ uống mua được: +4 máu tối đa vĩnh viễn mỗi bản. | Thủy Triều | Đồ uống | 46 |
| Contract · Armor correction `k10-armorcorrection` |  | Curio | Hero: Hệ số giáp +8%; Hero: Máu tối đa -1 | Bộ Máy | Sống sót | 24 |
| Contract · Bullet bounce chance `k10-bulletbounce` |  | Curio | Đội: Tỉ lệ nảy +8%; Hero: Máu tối đa -1 | Sương Giá | Tầm xa | 27 |
| Contract · Bullet speed `k10-bulletspeed` |  | Curio | Đội: Tốc độ đạn +8%; Hero: Máu tối đa -1 | Sương Giá | Tầm xa | 24 |
| Contract · Bullet split `k10-bulletsplit` |  | Curio | Đội: Tách đạn +2; Hero: Máu tối đa -1 | Tinh Tú | Tầm xa | 24 |
| Contract · Bullet travel distance `k10-bulletdistance` |  | Curio | Đội: Tầm bay của đạn +8%; Hero: Máu tối đa -1 | Tinh Tú | Tầm xa | 24 |
| Contract · Champion size `k10-bodysize` |  | Curio | Hero: Cỡ thân hero +8%; Hero: Máu tối đa -1 | Bếp Ấm | Va chạm | 24 |
| Contract · Chest quality `k10-chestquality` |  | Curio | Run: Chất lượng rương +8%; Hero: Máu tối đa -1 | Thủy Triều | Rương | 24 |
| Contract · Collision frequency `k10-collisionfrequency` |  | Curio | Đội: Tần suất va chạm +8%; Hero: Máu tối đa -1 | Bộ Máy | Va chạm | 27 |
| Contract · Companion size `k10-companionsize` |  | Curio | Đồng đội: Cỡ thân đồng đội +8%; Hero: Máu tối đa -1 | Bếp Ấm | Va chạm | 24 |
| Contract · Dodge `k10-dodge` |  | Curio | Hero: Né +12%; Hero: Máu tối đa -1 | Thủy Triều | Sống sót | 27 |
| Contract · Enemy health `k10-enemyhealth` |  | Curio | Run: Máu quái +10%; Run: Hệ số điểm +20%; Hero: Máu tối đa -1 | Bộ Máy | Đội | 27 |
| Contract · Flat melee damage `k10-meleedamage` |  | Curio | Đội: Sát thương cận chiến (cộng thẳng) +4; Hero: Máu tối đa -1 | Bộ Máy | Cận chiến | 24 |
| Contract · Flat ranged damage `k10-rangeddamage` |  | Curio | Đội: Sát thương tầm xa (cộng thẳng) +4; Hero: Máu tối đa -1 | Sương Giá | Tầm xa | 24 |
| Contract · Health correction `k10-healthcorrection` |  | Curio | Hero: Hệ số máu +8%; Hero: Máu tối đa -1 | Bếp Ấm | Sống sót | 24 |
| Contract · Recruit discount `k10-recruitdiscount` |  | Curio | Run: Giảm giá tuyển +8%; Hero: Máu tối đa -1 | Vườn Cây | Shop | 27 |
| Contract · Score multiplier `k10-scoremultiplier` |  | Curio | Run: Hệ số điểm +8%; Hero: Máu tối đa -1 | Bộ Máy | Đội | 24 |
| Contract · Shop rarity `k10-shoprarity` |  | Curio | Run: Độ hiếm shop +8%; Hero: Máu tối đa -1 | Tinh Tú | Shop | 24 |
| Contract · Speed correction `k10-speedcorrection` |  | Curio | Đội: Hệ số tốc độ +8%; Hero: Máu tối đa -1 | Thủy Triều | Tốc độ | 24 |
| Courage Juice `CurioCourageJuice` |  | Drink | Đội: Tốc chạy +20%; Đội: Sát thương cận chiến (cộng thẳng) +5; Hero: Máu tối đa -10 | Thủy Triều | Đồ uống, Tốc độ, Cận chiến | 46 |
| Crooked Eight `CurioCrookedEight` |  | Curio | Run: May mắn +0.4; Đội: Tỉ lệ nảy +20%; Đội: Sát thương -6% | Vườn Cây | May rủi | 46 |
| Crown of the Empty Theatre `SecretCrown` |  | Curio | Đội: Sát thương +15%; Đội: Chí mạng +8% | Bếp Ấm | Đội, Chí mạng | 46 |
| Daylight Bonnet `CurioDaylightBonnet` |  | Curio | Hero: Máu tối đa +40 · Khi bị đánh: 5% hút mọi vật phẩm trên sàn. | Sương Giá | Vàng, Sống sót | 46 |
| Dead-Letter Key `SecretLetter` |  | Curio | Run: Tầm nhặt +0.8; Run: Dò kho báu +0.1 | Vườn Cây | Vàng, Rương | 46 |
| Dream Blindfold `CurioDreamBlindfold` |  | Curio | Run: May mắn +0.1; Run: Dò kho báu +0.25 · Mở rương: sét đánh 1–3 địch. | Vườn Cây | Rương, May rủi | 46 |
| Ember Espresso `CurioEmberEspresso` |  | Drink | Hero: Tốc đánh +24%; Hero: Sát thương va chạm -1 | Thư Viện | Đồ uống, Đội | 46 |
| Escape Thimble `K26Defense2` |  | Curio | Hero: Né +12%; Đội: Tốc chạy +12% | Bộ Máy | Tốc độ | 45 |
| Gilt Orbit `CurioGiltOrbit` |  | Curio | Đội: Sát thương tầm xa (cộng thẳng) +5 · Wave sau: +1 dò kho báu mỗi bản mua. | Tinh Tú | Rương, Tầm xa | 46 |
| Harvest Receipt `harvest-receipt` |  | Curio | Run: Tầm nhặt -0.15 · Ăn ý vũ khí: Cần vũ khí chuông đặc trưng trong tầm 6: nhặt xu đẩy nhịp chuông (tối đa 2 bước). | Thư Viện | Đội | 28 |
| Hourglass Advance `SecretHour` |  | Curio | Đội: Tốc đánh +18%; Đội: Sát thương +10% | Tinh Tú | Đội | 46 |
| Ink Feather `CurioInkFeather` |  | Curio | Lượt đổi rẻ hơn 2 vàng mỗi bản (tối thiểu 1). | Thư Viện | Shop | 46 |
| Interest Tin `CurioInterestTin` |  | Food | Run: Tầm nhặt +0.6 · Đầu mỗi wave: vàng đang giữ ×1.2 mỗi bản (lãi kép), tách riêng với lãi thường. | Vườn Cây | Đồ ăn, Vàng | 46 |
| Long Pocket `long-pocket` |  | Curio | Run: Bán kính mở rương +35%; Hero: Tốc chạy -2% | Bếp Ấm | Rương | 24 |
| Many Door Key `CurioManyDoorKey` |  | Curio | Run: Dò kho báu +0.25; Đội: Sát thương -5% · Mở rương: +0.06 may mắn vĩnh viễn mỗi bản. | Thủy Triều | May rủi, Rương | 46 |
| Marching Bell `marching-bell` |  | Curio | Đồng đội: Tốc đánh +24% | Bộ Máy | Đội | 26 |
| Moon Milk `CurioMoonMilk` |  | Drink | Hero: Hồi máu từ tim +4; Hero: Tốc chạy -5% | Vườn Cây | Đồ uống, Sống sót | 46 |
| Mulberry Riot `CurioMulberryRiot` |  | Drink | Run: Thu nhập +30; Run: Dò kho báu +0.6; Hero: Giáp -0.3 | Vườn Cây | Đồ uống, Vàng, Rương | 46 |
| Needle Target `CurioNeedleTarget` |  | Curio | Đội: Xuyên +1; Đội: Chí mạng +15%; Đội: Sát thương cận chiến (cộng thẳng) -3 | Sương Giá | Tầm xa, Chí mạng | 46 |
| Odd Elixir `CurioOddElixir` |  | Curio | Mỗi đồ uống mua được: đội +6% tốc đánh vĩnh viễn mỗi bản; đồ uống ra nhiều hơn 5%. | Kỳ Lạ | Đồ uống | 46 |
| Patch Spring `patch-spring` |  | Curio | Đồng đội: Tốc đánh -6% · Ăn ý vũ khí: Mỗi 3s tự nạp 1 lượt cho đồ chơi trong sân. | Kỳ Lạ | Đội | 32 |
| Pocket Sun `CurioPocketSun` |  | Drink | Hero: Cỡ thân hero -10%; Run: May mắn +0.3 | Thủy Triều | Đồ uống, May rủi | 46 |
| Prism Record `CurioPrismRecord` |  | Curio | Đội: Tầm đánh -10% · Phát bắn thường của hero có thể đánh thêm một địch ngẫu nhiên (hồi chung 0.15s). | Kỳ Lạ | Tầm xa | 46 |
| Quicksole `K26Defense3` |  | Curio | Đội: Tốc chạy +18%; Hero: Máu tối đa +20 | Kỳ Lạ | Tốc độ | 46 |
| Reflective Ink `reflective-ink` |  | Curio | Ăn ý vũ khí: Đòn tem thêm 2 dấu thay vì 1, nhưng dấu chỉ kéo dài 65%. | Thư Viện | Đánh dấu | 32 |
| Repair Coupon `repair-coupon` |  | Curio | Run: Tầm nhặt -0.1 · Ăn ý vũ khí: Đồ chơi trong sân hạ địch thì hồi 1 máu mỗi bản (hồi chung 0.6s). | Kỳ Lạ | Đội | 32 |
| Retort Cup `CurioRetortCup` |  | Curio | Hero: Giáp +0.4; Run: May mắn -0.08 · Khi bị đánh: 30% bắn 8 viên tỏa tròn, mỗi viên 12 sát thương mỗi bản. | Bếp Ấm | Tầm xa, Sống sót | 46 |
| Royal Key `royal-key` |  | Curio | Hero: Sát thương +44% | Tinh Tú | Chí mạng | 28 |
| Scratch Record `scratch-record` |  | Curio | Đội: Tốc đánh -4% · Ăn ý vũ khí: Đòn hộp nhạc thêm tiếng vọng sau 0.3s; nhịp chuông +tối đa 50% sát thương lên địch bị đánh dấu. | Thư Viện | Đội | 32 |
| Shattered Pager `CurioShatteredPager` |  | Curio | Run: Thu nhập +25; Hero: Tách đạn +2 | Sương Giá | Vàng, Tầm xa | 46 |
| Sleeping-Lamb Brooch `SecretDream` |  | Curio | Hero: Máu tối đa +12; Hero: Hồi máu từ tim +3 | Sương Giá | Sống sót | 46 |
| Sleepy Mist `CurioSleepyMist` |  | Drink | Đội: Sát thương +35%; Đội: Tốc chạy -20%; Run: Tốc độ quái -20% | Bộ Máy | Đồ uống, Đóng băng | 46 |
| Storm Dryer `CurioStormDryer` |  | Curio | Đội: Sát thương +24%; Đội: Đẩy lùi +20% | Tinh Tú | Đội, Cận chiến | 46 |
| Sunfire Flask `CurioSunfireFlask` |  | Drink | Đội: Tầm đánh +40%; Hero: Số viên đạn +1; Hero: Tốc đánh -25% | Thư Viện | Đồ uống, Tầm xa | 46 |
| Tea Coaster `tea-coaster` |  | Curio | Đội: Tốc chạy -2% · Ăn ý vũ khí: Vệt trà to hơn 40% nhưng sát thương vệt giảm 20%. | Kỳ Lạ | Đội | 32 |
| The Abandoned Wooden Heart `SecretWoodHeart` |  | Curio | Hero: Máu tối đa +18; Hero: Hồi máu mỗi giây +0.3 | Vườn Cây | Sống sót | 46 |
| The Glass Understudy `SecretMirror` |  | Curio | Đội: Chí mạng +5%; Đội: Tầm đánh +8% | Tinh Tú | Chí mạng, Tầm xa | 46 |
| Thermal Ink `thermal-ink` |  | Curio | Đội: Chí mạng -3% · Ăn ý vũ khí: Vệt nóng chạm địch bị đánh dấu thì giáng thêm đòn 40% sát thương vệt (hồi 0.9s mỗi địch). | Thư Viện | Đánh dấu | 32 |
| Treasure Ribbon `treasure-ribbon` |  | Curio | Đội: Sát thương -3% · Ăn ý vũ khí: Mở rương hồi 3 máu mỗi bản và tăng sát thương đòn kiểu cung trong 4s (tối đa +45%). | Kỳ Lạ | Đội | 32 |
| Watchful Tea `CurioWatchfulTea` |  | Curio | Hero: Hồi máu mỗi giây +2 · Wave sau: hero −0.8 giáp mỗi bản mua. | Bếp Ấm | Sống sót | 46 |
| Frost Salt `FrostSalt` | **Mới** | — | Đóng băng lâu hơn 0.3s. *(v5 §6b)* *(art đã vẽ)* | — (chỉ gắn họ; thiết kế: Sương Giá) | Đóng băng | chưa định giá |
| Late Fee `LateFee` | **Mới** | — | Sát thương theo thời gian và trì hoãn +25%. *(v5 §6b)* *(art đã vẽ)* | — (chỉ gắn họ; thiết kế: Bộ Máy) | Trì hoãn | chưa định giá |
| Postage Stamp `PostageStamp` | **Mới** | — | +10% sát thương lên địch bị đánh dấu. *(v5 §6b)* *(art đã vẽ)* | — (chỉ gắn họ; thiết kế: Thư Viện) | Đánh dấu | chưa định giá |
| Refund Slip `RefundSlip` | **Mới** | — | Mỗi shop một lần bán lại một món đủ giá. *(v5 §6b)* *(art đã vẽ)* | — (chỉ gắn họ; thiết kế: Bếp Ấm) | Shop | chưa định giá |
| Whetstone Wafer `WhetstoneWafer` | **Mới** | Food | Đồ ăn: đòn cận chiến chém thêm 1 địch (40%). *(v5 §6b)* *(art đã vẽ)* | — (chỉ gắn họ; thiết kế: Kỳ Lạ) | Đồ ăn, Cận chiến | chưa định giá |

### 6.3 Rare (44, mới 6)

| Item | Mới | Loại | Tác dụng | Chủ đề | Họ | Giá |
|---|:-:|---|---|---|---|---:|
| Altar Ember `altar-ember` |  | Curio | Hero: Sát thương +36%; Đội: Tốc chạy -3% | Tinh Tú | Chí mạng | 36 |
| Angry Reliquary `CurioAngryReliquary` |  | Curio | Tắt hồi máu thụ động và hồi từ tim; mở rương hồi 30 máu mỗi bản. | Kỳ Lạ | Rương | 60 |
| Bitter Honey `CurioBitterHoney` |  | Drink | Hero: Hồi máu mỗi giây +2.5; Run: Dò kho báu +0.6; Run: Tốc độ quái +7% | Sương Giá | Đồ uống, Sống sót, Rương | 60 |
| Brass Avalanche `brass-avalanche` |  | Curio | Đội: Đẩy lùi +25%; Hero: Tốc chạy -2% · Hạ địch: sóng sát thương 3, bán kính 1.8 (hồi 2s) | Bộ Máy | Theo loài đã hạ | 30 |
| Burden Collar `CurioBurdenCollar` |  | Curio | Hero: Sát thương va chạm +20; Đội: Tốc chạy -20% · Giáp hero cộng thêm đúng phần tốc chạy bị trừ của hero. | Bộ Máy | Va chạm | 60 |
| Carbon Ribbon `carbon-ribbon` |  | Curio | Đội: Chí mạng -4% · Ăn ý vũ khí: Tem nổ hút xu trong tầm 2.5; nhịp chuông làm lộ địch còn sống 1.5s. | Thư Viện | Đánh dấu | 40 |
| Chest Thunder `chest-thunder` |  | Curio | · Mở rương: sóng sát thương 14, bán kính 3 (hồi 1s) | Kỳ Lạ | Rương | 42 |
| Coin Salve `coin-salve` |  | Curio | · Nhặt xu: hồi 0.5 máu (hồi 0.2s) | Thư Viện | Đội | 30 |
| Glass Lotus `CurioGlassLotus` |  | Curio | Đầu mỗi wave: +3 xuyên và +30% chí mạng mỗi bản, mất khi hero bị đánh lần đầu. | Thư Viện | Chí mạng | 60 |
| Guardian Canopy `guardian-canopy` |  | Curio | Run: Đồng đội chặn đạn +1 | Tinh Tú | Sống sót | 45 |
| Hoarders Locket `CurioHoardersLocket` |  | Curio | Đội +1% sát thương mỗi 5 vàng đang giữ, mỗi bản. | Kỳ Lạ | Vàng | 60 |
| Hush Earmuffs `CurioHushEarmuffs` |  | Curio | Hero: Né +10%; Hero: Chí mạng +10% · Né thành công: 30% hồi 10 máu mỗi bản. | Thủy Triều | Sống sót, Chí mạng | 60 |
| Living Contraband Thread `SecretThread` |  | Curio | Đội: Tầm đánh +18%; Đội: Đẩy lùi +20% | Tinh Tú | Tầm xa, Cận chiến | 60 |
| Lodestone Label `CurioLodestoneLabel` |  | Curio | Đội: Sát thương tầm xa (cộng thẳng) +8; Run: Tầm nhặt +0.9 · Đồng đội đi sát hero hơn (bán kính ×0.65). | Sương Giá | Đội, Tầm xa, Vàng | 60 |
| Long Atlas `long-atlas` |  | Curio | Run: Bán kính tuần tra +1; Đội: Tốc chạy -4% | Bộ Máy | Đội | 36 |
| Mercy Spindle `K26Defense5` |  | Curio | Hero: Hồi máu mỗi giây +2.8; Run: Thu nhập -2 | Sương Giá | Sống sót | 60 |
| Nest Dumpling `CurioNestDumpling` |  | Food | Hero: Giáp -0.2 · Khi mua: nhận 3 trứng. | Vườn Cây | Đồ ăn, Shop | 60 |
| Office Lens `office-lens` |  | Curio | Đồng đội: Chí mạng +24%; Đồng đội: Tốc đánh -5% · Ăn ý vũ khí: Chí mạng lên địch bị lộ nhận thêm hệ số sát thương 1.25 (+0.05 mỗi bản, tối đa +0.25). | Sương Giá | Đánh dấu, Chí mạng | 38 |
| Oily Lozenge `CurioOilyLozenge` |  | Food | Hero: Máu tối đa +20 · Khi mua: các relic còn lại trong shop rẻ hơn 30%. | Sương Giá | Đồ ăn, Shop, Sống sót | 60 |
| Panic Bell `CurioPanicBell` |  | Curio | Mỗi lần nhặt xu: sét đánh một địch. | Thư Viện | May rủi | 60 |
| Petal Tonic `CurioPetalTonic` |  | Drink | Hero: Máu tối đa +90; Hero: Hồi máu mỗi giây -2 | Bếp Ấm | Đồ uống, Sống sót | 60 |
| Porcelain Promise `OctoberRelic3` |  | Curio | Hero: Hồi máu từ tim +2; Hero: Máu tối đa +10 | Bếp Ấm | Sống sót | 38 |
| Profit Slide `CurioProfitSlide` |  | Curio | Run: Thu nhập -10 · Thu nhập wave +1 cho mỗi +4% sát thương hero cộng thêm. | Thư Viện | Vàng | 60 |
| Pulse Bell `K26Defense4` |  | Curio | Hero: Hệ số máu +25%; Hero: Hồi máu mỗi giây +1.4 | Vườn Cây | Sống sót | 55 |
| Rain Receipt `OctoberRelic1` |  | Curio | Hero: Hồi máu mỗi giây +0.5; Run: Thu nhập -2 | Vườn Cây | Sống sót | 30 |
| Smuggler's Ticket `SecretTicket` |  | Curio | Run: Giảm giá shop vàng +8%; Run: May mắn +0.08 | Thủy Triều | Shop, May rủi | 60 |
| Sorrow Lantern `CurioSorrowLantern` |  | Curio | Hero: Né -5% · Mỗi 3s, mọi địch nhận sát thương bằng 20% máu tối đa của hero mỗi bản. | Kỳ Lạ | Trì hoãn | 60 |
| Sour Mandarin `CurioSourMandarin` |  | Food | Chỉ wave sau: đồng đội ngừng đánh thường, hồi chiêu của hero chia 6; mỗi 10 lần hạ địch hồi 3 máu. | Bếp Ấm | Đồ ăn, Cận chiến | 60 |
| Split Kernel `CurioSplitKernel` |  | Food | Hero: Hồi máu mỗi giây +3 · Đầu mỗi trận, máu hiện tại của hero giảm còn tối đa 50%. | Bếp Ấm | Đồ ăn, Sống sót | 60 |
| Stillwater Urn `CurioStillwaterUrn` |  | Curio | Hero: Tốc chạy -5% · Đứng yên hồi 3 máu/giây mỗi bản. | Thư Viện | Sống sót | 60 |
| Stone Crust `CurioStoneCrust` |  | Food | Hero: Tốc đánh -10% · Sát thương va chạm của hero +50% phần sát thương cận chiến cộng thêm của hero. | Vườn Cây | Đồ ăn, Cận chiến | 60 |
| Torn Almanac `CurioTornAlmanac` |  | Curio | Đội: Sát thương -5% · Đầu mỗi wave: nhận ngẫu nhiên 25–75 vàng mỗi bản. | Kỳ Lạ | May rủi | 60 |
| Treasure Whisper `treasure-whisper` |  | Curio | Run: Dò kho báu +0.35 | Thủy Triều | Rương | 30 |
| Twin Seal `twin-seal` |  | Curio | Đội: Số viên đạn +2 | Tinh Tú | Tầm xa | 35 |
| Urchin Ball `CurioUrchinBall` |  | Curio | Hero: Giáp +0.6; Hero: Tốc chạy +10% · Hero ngừng đánh thường; sát thương va chạm +10× giáp dương. | Thủy Triều | Va chạm, Sống sót, Tốc độ | 60 |
| Victor Draught `CurioVictorDraught` |  | Drink | Đội: Chí mạng +20%; Đội: Xuyên +1; Đội: Sát thương -20% | Thủy Triều | Đồ uống, Chí mạng, Tầm xa | 60 |
| Wakeful Cordial `CurioWakefulCordial` |  | Drink | Hero: Sát thương va chạm +25; Hero: Đẩy lùi +30%; Hero: Chí mạng -10% | Bộ Máy | Đồ uống, Va chạm, Cận chiến | 60 |
| Wild Pigment `CurioWildPigment` |  | Drink | Đội: Tốc chạy +15%; Đội: Sát thương cận chiến (cộng thẳng) +7; Đội: Sát thương tầm xa (cộng thẳng) -3 | Bộ Máy | Đồ uống, Tốc độ, Cận chiến | 60 |
| Brain Freeze `BrainFreeze` | **Mới** | — | Chí mạng lên địch đóng băng vỡ thành 3 mảnh băng. *(v5 §6b)* *(art đã vẽ)* | — (chỉ gắn họ; thiết kế: Sương Giá) | Đóng băng | chưa định giá |
| Haggler's Abacus `HagglersAbacus` | **Mới** | — | Mỗi lượt đổi: mọi món rẻ hơn 1, tối đa −30%. *(v5 §6b)* *(art đã vẽ)* | — (chỉ gắn họ; thiết kế: Thư Viện) | Shop | chưa định giá |
| Slipstream Scarf `SlipstreamScarf` | **Mới** | — | Đồng đội +1% tốc đánh mỗi 5% tốc chạy cộng thêm của hero, tối đa 15%. *(v5 §6b)* *(art đã vẽ)* | — (chỉ gắn họ; thiết kế: Tinh Tú) | Tốc độ | chưa định giá |
| Specimen Pin `SpecimenPin` | **Mới** | — | +1% chí mạng mỗi loài mới hạ trong run, tối đa 20%. *(v5 §6b)* *(chưa có art)* | — (chỉ gắn họ; thiết kế: Thư Viện) | Theo loài đã hạ | chưa định giá |
| Star Polish `StarPolish` | **Mới** | — | Đồng đội 3★ +1 viên đạn. *(v5 §6b)* *(chưa có art)* | — (chỉ gắn họ; thiết kế: Vườn Cây) | Merge | chưa định giá |
| Wind Chime `WindChime` | **Mới** | — | Mỗi 10 lần chí mạng của đội: 3 phát kế của mọi đồng đội +1 xuyên. *(v5 §5/§6b)* *(chưa có art)* | — (chỉ gắn họ; thiết kế: Bộ Máy) | Chí mạng | chưa định giá |

### 6.4 Mythic (38, mới 11)

| Item | Mới | Loại | Tác dụng | Chủ đề | Họ | Giá |
|---|:-:|---|---|---|---|---:|
| Anchor Tack `CurioAnchorTack` |  | Curio | Đồng đội đứng yên; tầm và thời gian bay của đạn ×2, +3 xuyên. | Thư Viện | Đội | 74 |
| Battering Helm `CurioBatteringHelm` |  | Curio | Hero: Sát thương va chạm +40; Hero: Tần suất va chạm +70%; Đội: Sát thương tầm xa (cộng thẳng) -10 | Bộ Máy | Va chạm | 74 |
| Crimson Seal `CrimsonSeal` |  | Curio | Hero: Sát thương +90%; Đội: Sát thương va chạm +24 · *Thiết kế:* Bản 3 §6: thêm cơ chế vết ấn nổ sau va chạm (từ Crimson xuống Mythic). | Kỳ Lạ | Chí mạng, Va chạm | 75 |
| Crimson Spring `CrimsonSpring` |  | Curio | Đội: Tốc đánh +50%; Đội: Tầm đánh +45% · *Thiết kế:* Bản 3 §6: thêm cơ chế mỗi 4s đòn kế bắn hai lần (từ Crimson xuống Mythic). | Thư Viện | Đội, Tầm xa | 75 |
| Echo Postage `echo-postage` |  | Curio | Đội: Số viên đạn +3; Đội: Tốc đánh -8% · *Thiết kế:* Bản 3 §7.2: ở lại Mythic, thêm một cơ chế nhỏ (chưa có mô tả). | Sương Giá | Tầm xa | 60 |
| Emerald Invoice `CurioEmeraldInvoice` |  | Curio | Run: Giảm giá relic +25%; Đội: Sát thương -20% | Thủy Triều | Shop | 74 |
| Feast Bucket `CurioFeastBucket` |  | Food | Hero: Máu tối đa +40; Hero: Hồi máu mỗi giây +1; Hero: Cỡ thân hero +10%; Hero: Né -30% · Mỗi đồ ăn mua được: +10 máu tối đa, +0.5 máu/giây, +5% cỡ hero vĩnh viễn mỗi bản. | Vườn Cây | Đồ ăn, Sống sót, Va chạm | 74 |
| Fortune Pear `CurioFortunePear` |  | Food | Run: May mắn +0.16; Run: Thu nhập -15 · Máu tối đa +100× may mắn dương. | Thủy Triều | Đồ ăn, May rủi | 74 |
| Gamble Sandwich `CurioGambleSandwich` |  | Food | Hero: Giáp -0.2 · Mỗi 3s tung hai lần: 50% hồi 20 máu, 50% mất 8 máu (mỗi bản). | Bếp Ấm | Đồ ăn, May rủi | 74 |
| Hungry Ledger `CurioHungryLedger` |  | Curio | Đầu mỗi wave: +4 sát thương cận chiến, +4 sát thương tầm xa và +10% sát thương đội vĩnh viễn mỗi bản. | Kỳ Lạ | Theo loài đã hạ | 60 |
| Iron Apron `K26Defense6` |  | Curio | Hero: Giáp +0.65; Đội: Tốc chạy -8% · *Thiết kế:* Bản 3 §7.2: ở lại Mythic, thêm một cơ chế nhỏ (chưa có mô tả). | Bộ Máy | Sống sót | 65 |
| Last Hope Motor `CurioLastHopeMotor` |  | Curio | Mỗi trận: +16 sát thương mỗi bản cho tới khi hero di chuyển lần đầu. | Thư Viện | Chí mạng | 74 |
| Liar Mask `CurioLiarMask` |  | Curio | Đội: Sát thương +100% · Mỗi lần hero bị đánh: đội −5% sát thương mỗi bản. | Tinh Tú | Chí mạng, Đội | 74 |
| Mending Moth `K26Defense7` |  | Curio | Hero: Hồi máu mỗi giây +2.5; Hero: Giáp +0.12 · *Thiết kế:* Bản 3 §7.2: ở lại Mythic, thêm một cơ chế nhỏ (chưa có mô tả). | Kỳ Lạ | Sống sót | 70 |
| Mute Clock `CurioMuteClock` |  | Curio | Đội: Tốc đánh +45%; Đội: Tốc chạy +40% · Trong trận hero mất 1 máu mỗi giây mỗi bản. | Thủy Triều | Tốc độ | 60 |
| Oath Parcel `CurioOathParcel` |  | Food | Khi mua: ghi chỉ số hiện tại làm sàn; về sau chỉ số không tụt dưới sàn. | Vườn Cây | Đồ ăn, Đội | 74 |
| Orbit Pastry `CurioOrbitPastry` |  | Food | Hero: Cỡ thân hero +25%; Hero: Cỡ đạn +75%; Hero: Tốc độ đạn -40%; Hero: Sát thương +100% | Vườn Cây | Đồ ăn, Va chạm, Tầm xa | 74 |
| Shield Blower `CurioShieldBlower` |  | Curio | Đội: Sát thương va chạm +25; Run: Đồng đội chặn đạn +1 | Tinh Tú | Va chạm, Sống sót | 74 |
| Sixfold Root `CurioSixfoldRoot` |  | Food | Lần mua đầu: nhớ 6 vũ khí ngẫu nhiên; đòn thường của đội thay bằng sáu vũ khí ma bắn từ hero. | Bếp Ấm | Đồ ăn, Cận chiến | 74 |
| Starlight Quilt `starlight-quilt` |  | Curio | Hero: Máu tối đa +90 · *Thiết kế:* Bản 3 §7.2: ở lại Mythic, thêm một cơ chế nhỏ (chưa có mô tả). | Vườn Cây | Sống sót | 48 |
| Tarnished Crown `tarnished-crown` |  | Curio | Hero: Sát thương +80% · *Thiết kế:* Bản 3 §7.2: ở lại Mythic, thêm một cơ chế nhỏ (chưa có mô tả). | Bếp Ấm | Chí mạng | 48 |
| Thorn Perfume `CurioThornPerfume` |  | Curio | Run: Tốc độ quái +30% · Mỗi lần hero bị đánh: thêm ngẫu nhiên một thưởng run nhỏ (máu, hồi máu, giáp, sát thương, may mắn hoặc chí mạng). | Tinh Tú | Sống sót | 74 |
| Truce Bottle `CurioTruceBottle` |  | Drink | Đội: Sát thương +40%; Đội: Chí mạng +20%; Đội: Tầm đánh +40%; Hero: Giáp +0.6 | Tinh Tú | Đồ uống, Đội, Chí mạng | 74 |
| Venom Pass `CurioVenomPass` |  | Curio | Đòn thường của hero tẩm độc 4s; mỗi giây gây sát thương bằng thu nhập wave. | Kỳ Lạ | Trì hoãn | 74 |
| Void Doubloon `CurioVoidDoubloon` |  | Curio | Đội: Sát thương -30% · Mỗi 3s mọi địch mất 15% máu hiện tại mỗi bản. | Bộ Máy | Trì hoãn | 74 |
| Wander Letter `CurioWanderLetter` |  | Curio | Đội: Chí mạng +40%; Hero: Giáp -0.7 · Đồng đội tuần tra khắp sân (bán kính = nửa đường chéo sân). | Sương Giá | Chí mạng | 74 |
| Wish Wick `CurioWishWick` |  | Curio | Hero: Máu tối đa -5; Run: Thu nhập -5 · Khi mua: thử tuyển tối đa 2 đồng đội thường từ bộ của hero. | Sương Giá | Merge | 74 |
| Cuckoo Pendulum `CuckooPendulum` | **Mới** | — | Mỗi 12s đòn kế tiếp của mọi đồng đội chí mạng. *(v5 §5)* *(chưa có art)* | — (chỉ gắn họ; thiết kế: Bộ Máy) | Chí mạng | chưa định giá |
| Diving Bell Lantern `DivingBellLantern` | **Mới** | — | Địch bị đánh dấu chịu +15% sát thương. *(v3 §7.3)* *(chưa có art)* | — (chỉ gắn họ; thiết kế: Thủy Triều) | Đánh dấu | chưa định giá |
| Duelist's Ribbon `DuelistsRibbon` | **Mới** | — | (chưa có mô tả) *(chưa có art)* | — (chỉ gắn họ; thiết kế: Kỳ Lạ) | Cận chiến | chưa định giá |
| Frost Ledger `FrostLedger` | **Mới** | — | (chưa có mô tả) *(chưa có art)* | — (chỉ gắn họ; thiết kế: Sương Giá) | Đóng băng, Theo loài đã hạ | chưa định giá |
| Funhouse Mirror `FunhouseMirror` | **Mới** | — | Mỗi 10s vũ khí một đồng đội bị bóp méo: to gấp đôi, chậm một nửa trong 3s. *(v5 §6b)* *(chưa có art)* | Kỳ Lạ | Đội | chưa định giá |
| Kite Tail Ribbon `KiteTailRibbon` | **Mới** | — | Đạn chí mạng của đồng đội tầm xa nảy thêm 1 lần. *(v5 §5)* *(chưa có art)* | — (chỉ gắn họ; thiết kế: Tinh Tú) | Tầm xa | chưa định giá |
| Melting Sundae `MeltingSundae` | **Mới** | — | Hero bị đánh thì địch gần đóng băng 0.5s (hồi 4s). *(v3 §7.3)* *(chưa có art)* | — (chỉ gắn họ; thiết kế: Sương Giá) | Đóng băng | chưa định giá |
| Splice Kit `SpliceKit` | **Mới** | — | Mỗi wave một lần, ba đồng đội 1★ khác loài gộp thành một con 2★ ngẫu nhiên. *(v5 §6b)* *(chưa có art)* | — (chỉ gắn họ; thiết kế: Bếp Ấm) | Merge | chưa định giá |
| Treasure Mimic `TreasureMimic` | **Mới** | — | (chưa có mô tả) *(chưa có art)* | — (chỉ gắn họ; thiết kế: Vườn Cây) | Rương | chưa định giá |
| Twin Ticket `TwinTicket` | **Mới** | — | Merge lên 3★ tặng thêm một bản 1★ cùng loại. *(v3 §7.3)* *(chưa có art)* | — (chỉ gắn họ; thiết kế: Thư Viện) | Merge | chưa định giá |
| Undertow Anchor `UndertowAnchor` | **Mới** | — | Mỗi 8s một con sóng từ tuyến đầu đẩy 5 địch gần nhất lùi 2 ô và làm ướt. *(v5 §6b)* *(chưa có art)* | Thủy Triều | Đội | chưa định giá |

### 6.5 Prismatic (9, mới 5)

| Item | Mới | Loại | Tác dụng | Chủ đề | Họ | Giá |
|---|:-:|---|---|---|---|---:|
| Borrowed Crown `RainbowCrown` |  | Curio | Đội: Sát thương +70%; Đội: Số viên đạn +2 · *Thiết kế:* Bản 5 §5: đồng đội nhiều sao nhất bắn thêm vũ khí của hero. | Bếp Ấm · Sương Giá | Đội, Tầm xa | 200 |
| Clockwork Heart `RainbowHeart` |  | Curio | Hero: Máu tối đa +120; Hero: Hồi máu mỗi giây +1.8 · *Thiết kế:* Bản 5 §5: mỗi lần merge lên dây cót: đội +3% tốc đánh cả run (tối đa +30%). | Sương Giá · Vườn Cây | Merge | 200 |
| Impossible Prism `RainbowPrism` |  | Curio | Đội: Tách đạn +3; Đội: Sát thương tầm xa (cộng thẳng) +4 | Tinh Tú · Thư Viện | Tầm xa | 200 |
| Living Thread `RainbowThread` |  | Curio | Đội: Sát thương cận chiến (cộng thẳng) +4; Đội: Đẩy lùi +63% | Bộ Máy · Bếp Ấm | Cận chiến | 200 |
| Dream Loom `DreamLoom` | **Mới** | — | Mọi item Common mạnh gấp đôi. *(v3 §7.3)* *(chưa có art)* | Sương Giá · Thư Viện | Đội | chưa định giá |
| Mirror of Twins `MirrorOfTwins` | **Mới** | — | Companion mạnh nhất +60% sát thương và mỗi đòn bắn thêm 1 viên. *(v3 §7.3 (chủ đề theo v5 §5))* *(chưa có art)* | Bếp Ấm · Kỳ Lạ | Đội | chưa định giá |
| Moon Clock Key `MoonClockKey` | **Mới** | — | Mọi bond đang kích hoạt tính thêm 1 thành viên. *(v3 §7.3)* *(chưa có art)* | Tinh Tú · Bộ Máy | Đội | chưa định giá |
| Storybook Ending `StorybookEnding` | **Mới** | — | Quái cuối cùng mỗi wave rơi một trang: +1 chỉ số đội ngẫu nhiên vĩnh viễn. *(v5 §5)* *(chưa có art)* | Vườn Cây · Thủy Triều | Đội | chưa định giá |
| The Endless Encore `TheEndlessEncore` | **Mới** | — | Bỏ 3s nghỉ sau biến hình. *(v3 §7.3)* *(chưa có art)* | Kỳ Lạ · Thủy Triều | Đội | chưa định giá |

## 7. Họ item (18)

Mỗi hero có 1 họ item; item gắn nhiều họ được (loại món + cơ chế + chỉ số). Item Common không gắn họ. Cột "Item" liệt kê mọi item không phải Common thuộc họ.

| Họ | Id | Hero | Số item | Item |
|---|---|---|---:|---|
| Chí mạng | `fam-crit` | Storm Fencer, Tin Virtuoso, Juggling Jester | 17 | Crown of the Empty Theatre (U), Needle Target (U), Royal Key (U), The Glass Understudy (U), Altar Ember (R), Glass Lotus (R), Hush Earmuffs (R), Office Lens (R), Victor Draught (R), Wind Chime (R)*, Crimson Seal (M), Cuckoo Pendulum (M)*, Last Hope Motor (M), Liar Mask (M), Tarnished Crown (M), Truce Bottle (M), Wander Letter (M) |
| Merge | `fam-merge` | The Lone Jelly, Bonsai Clocksmith, Verdant Matron | 5 | Star Polish (R)*, Splice Kit (M)*, Twin Ticket (M)*, Wish Wick (M), Clockwork Heart (P) |
| Tầm xa | `fam-ranged` | Kite Admiral, Petal Citadel, Lantern Weaver | 23 | Citrus Storm (U), Contract · Bullet bounce chance (U), Contract · Bullet speed (U), Contract · Bullet split (U), Contract · Bullet travel distance (U), Contract · Flat ranged damage (U), Gilt Orbit (U), Needle Target (U), Prism Record (U), Retort Cup (U), Shattered Pager (U), Sunfire Flask (U), The Glass Understudy (U), Living Contraband Thread (R), Lodestone Label (R), Twin Seal (R), Victor Draught (R), Crimson Spring (M), Echo Postage (M), Kite Tail Ribbon (M)*, Orbit Pastry (M), Borrowed Crown (P), Impossible Prism (P) |
| Vàng | `fam-gold` | Thorn Treasurer, Penny Broker, China Marionette | 8 | Daylight Bonnet (U), Dead-Letter Key (U), Interest Tin (U), Mulberry Riot (U), Shattered Pager (U), Hoarders Locket (R), Lodestone Label (R), Profit Slide (R) |
| Đội | `fam-team` | Lantern Knight, Velvet Regent, Patchwork Host | 30 | Brigade Banner (U), Contract · Enemy health (U), Contract · Score multiplier (U), Crown of the Empty Theatre (U), Ember Espresso (U), Harvest Receipt (U), Hourglass Advance (U), Marching Bell (U), Patch Spring (U), Repair Coupon (U), Scratch Record (U), Storm Dryer (U), Tea Coaster (U), Treasure Ribbon (U), Coin Salve (R), Lodestone Label (R), Long Atlas (R), Anchor Tack (M), Crimson Spring (M), Funhouse Mirror (M)*, Liar Mask (M), Oath Parcel (M), Truce Bottle (M), Undertow Anchor (M)*, Borrowed Crown (P), Dream Loom (P)*, Mirror of Twins (P)*, Moon Clock Key (P)*, Storybook Ending (P)*, The Endless Encore (P)* |
| Cận chiến | `fam-melee` | Button Cavalier, Moon Prowler | 12 | Contract · Flat melee damage (U), Courage Juice (U), Storm Dryer (U), Whetstone Wafer (U)*, Living Contraband Thread (R), Sour Mandarin (R), Stone Crust (R), Wakeful Cordial (R), Wild Pigment (R), Duelist's Ribbon (M)*, Sixfold Root (M), Living Thread (P) |
| Rương | `fam-chest` | The Treasure Clerk, Rain Archivist | 12 | Contract · Chest quality (U), Dead-Letter Key (U), Dream Blindfold (U), Gilt Orbit (U), Long Pocket (U), Many Door Key (U), Mulberry Riot (U), Angry Reliquary (R), Bitter Honey (R), Chest Thunder (R), Treasure Whisper (R), Treasure Mimic (M)* |
| Shop | `fam-shop` | Drowsy Seer, Ivory Sovereign | 9 | Contract · Recruit discount (U), Contract · Shop rarity (U), Ink Feather (U), Refund Slip (U)*, Haggler's Abacus (R)*, Nest Dumpling (R), Oily Lozenge (R), Smuggler's Ticket (R), Emerald Invoice (M) |
| Sống sót | `fam-survival` | The Root Oracle, The Mirror Keeper | 27 | Contract · Armor correction (U), Contract · Dodge (U), Contract · Health correction (U), Daylight Bonnet (U), Moon Milk (U), Retort Cup (U), Sleeping-Lamb Brooch (U), The Abandoned Wooden Heart (U), Watchful Tea (U), Bitter Honey (R), Guardian Canopy (R), Hush Earmuffs (R), Mercy Spindle (R), Oily Lozenge (R), Petal Tonic (R), Porcelain Promise (R), Pulse Bell (R), Rain Receipt (R), Split Kernel (R), Stillwater Urn (R), Urchin Ball (R), Feast Bucket (M), Iron Apron (M), Mending Moth (M), Shield Blower (M), Starlight Quilt (M), Thorn Perfume (M) |
| Va chạm | `fam-collision` | Citrus Marshal, Copper Cauldron | 11 | Contract · Champion size (U), Contract · Collision frequency (U), Contract · Companion size (U), Burden Collar (R), Urchin Ball (R), Wakeful Cordial (R), Battering Helm (M), Crimson Seal (M), Feast Bucket (M), Orbit Pastry (M), Shield Blower (M) |
| Đánh dấu | `fam-marks` | Postmaster, Bell Diver | 6 | Postage Stamp (U)*, Reflective Ink (U), Thermal Ink (U), Carbon Ribbon (R), Office Lens (R), Diving Bell Lantern (M)* |
| Đóng băng | `fam-freeze` | Soap Skater, Sundae Duchess | 5 | Frost Salt (U)*, Sleepy Mist (U), Brain Freeze (R)*, Frost Ledger (M)*, Melting Sundae (M)* |
| Đồ ăn | `fam-food` | Night Maw, Supper Mortar | 13 | Interest Tin (U), Whetstone Wafer (U)*, Nest Dumpling (R), Oily Lozenge (R), Sour Mandarin (R), Split Kernel (R), Stone Crust (R), Feast Bucket (M), Fortune Pear (M), Gamble Sandwich (M), Oath Parcel (M), Orbit Pastry (M), Sixfold Root (M) |
| May rủi | `fam-luck` | Lady Loaded Dice | 9 | Crooked Eight (U), Dream Blindfold (U), Many Door Key (U), Pocket Sun (U), Panic Bell (R), Smuggler's Ticket (R), Torn Almanac (R), Fortune Pear (M), Gamble Sandwich (M) |
| Theo loài đã hạ | `fam-kills` | Archive Curator | 4 | Brass Avalanche (R), Specimen Pin (R)*, Frost Ledger (M)*, Hungry Ledger (M) |
| Trì hoãn | `fam-delay` | Hourglass Stag | 4 | Late Fee (U)*, Sorrow Lantern (R), Venom Pass (M), Void Doubloon (M) |
| Tốc độ | `fam-speed` | The Comet Courier | 8 | Contract · Speed correction (U), Courage Juice (U), Escape Thimble (U), Quicksole (U), Slipstream Scarf (R)*, Urchin Ball (R), Wild Pigment (R), Mute Clock (M) |
| Đồ uống | `fam-drink` | The Tide Singer | 16 | Citrus Storm (U), Clear Sip (U), Courage Juice (U), Ember Espresso (U), Moon Milk (U), Mulberry Riot (U), Odd Elixir (U), Pocket Sun (U), Sleepy Mist (U), Sunfire Flask (U), Bitter Honey (R), Petal Tonic (R), Victor Draught (R), Wakeful Cordial (R), Wild Pigment (R), Truce Bottle (M) |

Ký hiệu: (U/R/M/P) = bậc; `*` = item mới chưa vào game. Item mới thuộc một họ (không phải Common, không phải họ Đội) chỉ gắn họ, không gắn chủ đề (v5 §9).

## 8. Phụ lục: bộ companion cố định của từng hero

`*` = companion mới chưa vào game; **đậm** = Mythic / Prismatic. Đã áp dụng luật đối thủ và companion riêng.

| Hero | Số | Companion |
|---|---:|---|
| Lantern Knight | 23 | **Sundae Golem***, **The Last Bell**, **Clockwork Piper**, **Coil Kettle**, **Harbor Whale***, **Tea Dragon***, Saffron Mechanist, Spring Duck, Top Beetle*, Baker Mouse*, Bottle Gull*, Candle Snail, Lamp Goat*, Teacup Cricket, Velvet Enchantress, Bread Crab, Button Beetle, Dream Compass, Kettle Bat, Staple Mole, Static Ferret*, Teapot Pilgrim, Tin Tick |
| Postmaster | 26 | **Kite Dragon***, **The Endless Index***, **Harbor Whale***, **Reel Owl**, **Return Captain**, Bookmark Golem, Lens Owl*, The Folded Courier, The Forgotten Patchwork, Thimble Moth, Velvet Champion, Volt Eel*, Anchor Snail, Balloon Mallet, Coral Teacup, Raincoat Frog*, Ribbon Otter, Sail Squid, Spindle Wren, Brass Acorn, Bubble Koi, Court Herald*, Mailbox Imp, Paper Plane Pigeon*, Shanty Sea Lion*, Shell Postman |
| Drowsy Seer | 24 | **Eclipse Egg**, **Sundae Golem***, **Coil Kettle**, **Pillow Oracle**, **Tea Dragon***, Dough Foreman, Moon Jelly, Saffron Mechanist, The Sleeping Friend, Baker Mouse*, Bottle Gull*, Candle Snail, Star Sock, Storm Kitten*, Teacup Cricket, Velvet Enchantress, Bread Crab, Comet Kite, Court Herald*, Kettle Bat, Mailbox Imp, Orbit Moth, Paper Plane Pigeon*, Teapot Pilgrim |
| The Root Oracle | 21 | **The First Seed***, **The Impossible Tailor**, **Iron Locomotive**, **Paper Inventor**, Rain Heron*, Shovel Penguin*, Yo-yo Squirrel*, Bristle Ball, Citrus Toad, Ice Cream Owl, Monster Marble, Paper Devil, Pear Moth, Watering Snail*, Boba Tadpole*, Fridge Wisp, Hood Wisp, Mushroom Librarian, Radish Jester, Seed Hedgehog, Snow Spoon |
| The Lone Jelly | 24 | **Kite Dragon***, **The Lost Carousel***, **Harbor Whale***, **Mirror Newt**, **Return Captain**, **Ringmaster Jack***, Volt Eel*, Anchor Snail, Balloon Mallet, Carousel Pony, Coral Teacup, Raincoat Frog*, Ribbon Otter, Sail Squid, Violet Remnant, Bubble Koi, Crayon Gremlin, Dice Crab, Jelly Swordsman, Mask Pepper, Ribbon Spider, Shanty Sea Lion*, Shell Postman, Sock Cyclops |
| Lady Loaded Dice | 24 | **The First Seed***, **The Lost Carousel***, **Mirror Newt**, **Paper Inventor**, **Ringmaster Jack***, Yo-yo Squirrel*, Bristle Ball, Carousel Pony, Citrus Toad, Coral Teacup, Paper Devil, Pear Moth, Violet Remnant, Watering Snail*, Bubble Koi, Crayon Gremlin, Dice Crab, Jelly Swordsman, Mask Pepper, Mushroom Librarian, Radish Jester, Ribbon Spider, Seed Hedgehog, Sock Cyclops |
| The Tide Singer | 23 | **Eclipse Egg**, **Kite Dragon***, **Harbor Whale***, **Pillow Oracle**, **Return Captain**, Dough Foreman, Moon Jelly, The Sleeping Friend, Volt Eel*, Anchor Snail, Balloon Mallet, Bottle Gull*, Coral Teacup, Raincoat Frog*, Ribbon Otter, Sail Squid, Star Sock, Storm Kitten*, Bubble Koi, Comet Kite, Orbit Moth, Shanty Sea Lion*, Shell Postman |
| The Comet Courier | 19 | **Eclipse Egg**, **The Last Bell**, **Clockwork Piper**, **Pillow Oracle**, Dough Foreman, Moon Jelly, Spring Duck, The Sleeping Friend, Top Beetle*, Lamp Goat*, Star Sock, Button Beetle, Comet Kite, Dream Compass, Fridge Wisp, Orbit Moth, Paper Plane Pigeon*, Sock Cyclops, Staple Mole |
| The Mirror Keeper | 21 | **The Impossible Tailor**, **The Lost Carousel***, **Iron Locomotive**, **Mirror Newt**, **Ringmaster Jack***, Rain Heron*, Shovel Penguin*, Carousel Pony, Ice Cream Owl, Monster Marble, Sail Squid, Violet Remnant, Boba Tadpole*, Crayon Gremlin, Dice Crab, Fridge Wisp, Hood Wisp, Jelly Swordsman, Mask Pepper, Snow Spoon, Sock Cyclops |
| The Treasure Clerk | 24 | **The Endless Index***, **The First Seed***, **Paper Inventor**, **Reel Owl**, Bookmark Golem, Lens Owl*, The Folded Courier, The Forgotten Patchwork, Thimble Moth, Velvet Champion, Yo-yo Squirrel*, Citrus Toad, Coral Teacup, Pear Moth, Spindle Wren, Watering Snail*, Brass Acorn, Bubble Koi, Court Herald*, Dice Crab, Mailbox Imp, Mushroom Librarian, Radish Jester, Seed Hedgehog |
| Verdant Matron | 23 | **Sundae Golem***, **The First Seed***, **Coil Kettle**, **Paper Inventor**, **Tea Dragon***, Saffron Mechanist, Baker Mouse*, Bottle Gull*, Bristle Ball, Candle Snail, Grand Sprout, Little Sprout, Paper Devil, Pear Moth, Teacup Cricket, Velvet Enchantress, Watering Snail*, Bread Crab, Kettle Bat, Mushroom Librarian, Radish Jester, Seed Hedgehog, Teapot Pilgrim |
| Copper Cauldron | 25 | **Sundae Golem***, **The Lost Carousel***, **Coil Kettle**, **Iron Locomotive**, **Mirror Newt**, **Ringmaster Jack***, **Tea Dragon***, Saffron Mechanist, Baker Mouse*, Bottle Gull*, Candle Snail, Carousel Pony, Sail Squid, Teacup Cricket, Velvet Enchantress, Violet Remnant, Bread Crab, Crayon Gremlin, Dice Crab, Jelly Swordsman, Kettle Bat, Mask Pepper, Ribbon Spider, Sock Cyclops, Teapot Pilgrim |
| China Marionette | 23 | **The Impossible Tailor**, **The Lost Carousel***, **Iron Locomotive**, **Mirror Newt**, **Ringmaster Jack***, Rain Heron*, Shovel Penguin*, The Forgotten Patchwork, The Sleeping Friend, Thimble Moth, Carousel Pony, Ice Cream Owl, Monster Marble, Boba Tadpole*, Crayon Gremlin, Dice Crab, Fridge Wisp, Hood Wisp, Jelly Swordsman, Mask Pepper, Ribbon Spider, Snow Spoon, Sock Cyclops |
| Soap Skater | 23 | **Kite Dragon***, **The Impossible Tailor**, **Harbor Whale***, **Iron Locomotive**, **Return Captain**, Shovel Penguin*, Volt Eel*, Anchor Snail, Balloon Mallet, Coral Teacup, Ice Cream Owl, Monster Marble, Sail Squid, Star Sock, Boba Tadpole*, Bubble Koi, Fridge Wisp, Hood Wisp, Orbit Moth, Shanty Sea Lion*, Shell Postman, Snow Spoon, Sock Cyclops |
| Velvet Regent | 23 | **The Endless Index***, **The Last Bell**, **Clockwork Piper**, **Reel Owl**, Bookmark Golem, Lens Owl*, Spring Duck, The Folded Courier, The Forgotten Patchwork, Thimble Moth, Top Beetle*, Velvet Champion, Lamp Goat*, Spindle Wren, Velvet Enchantress, Brass Acorn, Button Beetle, Court Herald*, Dream Compass, Mailbox Imp, Staple Mole, Static Ferret*, Tin Tick |
| Button Cavalier | 23 | **The Last Bell**, **The Lost Carousel***, **Clockwork Piper**, **Mirror Newt**, **Return Captain**, **Ringmaster Jack***, Spring Duck, Top Beetle*, Anchor Snail, Carousel Pony, Lamp Goat*, Violet Remnant, Button Beetle, Crayon Gremlin, Dice Crab, Dream Compass, Jelly Swordsman, Mask Pepper, Ribbon Spider, Sock Cyclops, Staple Mole, Static Ferret*, Tin Tick |
| Night Maw | 24 | **Eclipse Egg**, **The Lost Carousel***, **Mirror Newt**, **Pillow Oracle**, **Ringmaster Jack***, Dough Foreman, Moon Jelly, Shovel Penguin*, The Sleeping Friend, Carousel Pony, Star Sock, Storm Kitten*, Violet Remnant, Bread Crab, Comet Kite, Crayon Gremlin, Dice Crab, Jelly Swordsman, Kettle Bat, Mask Pepper, Orbit Moth, Paper Plane Pigeon*, Ribbon Spider, Sock Cyclops |
| Thorn Treasurer | 23 | **The Endless Index***, **The First Seed***, **Paper Inventor**, **Reel Owl**, Bookmark Golem, Lens Owl*, The Folded Courier, The Forgotten Patchwork, Thimble Moth, Velvet Champion, Yo-yo Squirrel*, Bristle Ball, Citrus Toad, Lamp Goat*, Paper Devil, Pear Moth, Spindle Wren, Watering Snail*, Brass Acorn, Court Herald*, Mailbox Imp, Radish Jester, Seed Hedgehog |
| Citrus Marshal | 23 | **Sundae Golem***, **The First Seed***, **Coil Kettle**, **Paper Inventor**, **Tea Dragon***, Saffron Mechanist, Spring Duck, Yo-yo Squirrel*, Baker Mouse*, Bottle Gull*, Bristle Ball, Candle Snail, Citrus Toad, Paper Devil, Pear Moth, Teacup Cricket, Velvet Enchantress, Bread Crab, Kettle Bat, Mushroom Librarian, Radish Jester, Seed Hedgehog, Teapot Pilgrim |
| Ivory Sovereign | 24 | **The Endless Index***, **The Impossible Tailor**, **Iron Locomotive**, **Pillow Oracle**, **Reel Owl**, Bookmark Golem, Dough Foreman, Lens Owl*, Rain Heron*, Shovel Penguin*, The Folded Courier, The Forgotten Patchwork, Thimble Moth, Velvet Champion, Ice Cream Owl, Monster Marble, Spindle Wren, Boba Tadpole*, Brass Acorn, Court Herald*, Fridge Wisp, Hood Wisp, Mailbox Imp, Snow Spoon |
| Petal Citadel | 22 | **Eclipse Egg**, **The First Seed***, **Paper Inventor**, **Pillow Oracle**, Dough Foreman, Moon Jelly, The Sleeping Friend, Yo-yo Squirrel*, Bristle Ball, Citrus Toad, Paper Devil, Pear Moth, Star Sock, Storm Kitten*, Watering Snail*, Comet Kite, Dream Compass, Mushroom Librarian, Orbit Moth, Paper Plane Pigeon*, Radish Jester, Seed Hedgehog |
| Patchwork Host | 35 | **Sundae Golem***, **The First Seed***, **The Lost Carousel***, **Coil Kettle**, **Mirror Newt**, **Paper Inventor**, **Ringmaster Jack***, **Tea Dragon***, Saffron Mechanist, Yo-yo Squirrel*, Baker Mouse*, Balloon Mallet, Bottle Gull*, Bristle Ball, Candle Snail, Carousel Pony, Citrus Toad, Paper Devil, Pear Moth, Teacup Cricket, Velvet Enchantress, Violet Remnant, Watering Snail*, Bread Crab, Crayon Gremlin, Dice Crab, Jelly Swordsman, Kettle Bat, Mask Pepper, Mushroom Librarian, Radish Jester, Ribbon Spider, Seed Hedgehog, Sock Cyclops, Teapot Pilgrim |
| Archive Curator | 27 | **The Endless Index***, **The Impossible Tailor**, **The Last Bell**, **Clockwork Piper**, **Iron Locomotive**, **Reel Owl**, Bookmark Golem, Lens Owl*, Rain Heron*, Shovel Penguin*, Specimen Slime*, Spring Duck, Top Beetle*, Fossil Armadillo*, Ice Cream Owl, Lamp Goat*, Monster Marble, Boba Tadpole*, Button Beetle, Dream Compass, Fridge Wisp, Hood Wisp, Pinned Beetle*, Snow Spoon, Staple Mole, Static Ferret*, Tin Tick |
| Penny Broker | 27 | **Sundae Golem***, **The Endless Index***, **Coil Kettle**, **Paper Inventor**, **Reel Owl**, **Tea Dragon***, Bookmark Golem, Lens Owl*, Saffron Mechanist, The Folded Courier, The Forgotten Patchwork, Thimble Moth, Top Beetle*, Velvet Champion, Baker Mouse*, Bottle Gull*, Candle Snail, Spindle Wren, Teacup Cricket, Velvet Enchantress, Brass Acorn, Bread Crab, Court Herald*, Kettle Bat, Mailbox Imp, Staple Mole, Teapot Pilgrim |
| Storm Fencer | 19 | **Eclipse Egg**, **The Last Bell**, **Clockwork Piper**, **Pillow Oracle**, Dough Foreman, Moon Jelly, Spring Duck, The Sleeping Friend, Top Beetle*, Volt Eel*, Lamp Goat*, Storm Kitten*, Button Beetle, Comet Kite, Dream Compass, Paper Plane Pigeon*, Staple Mole, Static Ferret*, Tin Tick |
| Tin Virtuoso | 23 | **Eclipse Egg**, **The Last Bell**, **The Lost Carousel***, **Clockwork Piper**, **Mirror Newt**, **Ringmaster Jack***, **Tea Dragon***, Spring Duck, Top Beetle*, Carousel Pony, Lamp Goat*, Pear Moth, Violet Remnant, Button Beetle, Crayon Gremlin, Dice Crab, Dream Compass, Mask Pepper, Ribbon Spider, Sock Cyclops, Staple Mole, Static Ferret*, Tin Tick |
| Moon Prowler | 23 | **Eclipse Egg**, **The Impossible Tailor**, **Iron Locomotive**, **Pillow Oracle**, Dough Foreman, Moon Jelly, Rain Heron*, Shovel Penguin*, The Sleeping Friend, Ice Cream Owl, Monster Marble, Star Sock, Storm Kitten*, Boba Tadpole*, Comet Kite, Crayon Gremlin, Fridge Wisp, Hood Wisp, Mask Pepper, Orbit Moth, Paper Plane Pigeon*, Radish Jester, Snow Spoon |
| Supper Mortar | 28 | **Kite Dragon***, **Sundae Golem***, **Coil Kettle**, **Harbor Whale***, **Return Captain**, **Tea Dragon***, Saffron Mechanist, Volt Eel*, Anchor Snail, Baker Mouse*, Balloon Mallet, Bottle Gull*, Candle Snail, Coral Teacup, Ice Cream Owl, Raincoat Frog*, Ribbon Otter, Sail Squid, Teacup Cricket, Velvet Enchantress, Boba Tadpole*, Bread Crab, Bubble Koi, Kettle Bat, Shanty Sea Lion*, Shell Postman, Snow Spoon, Teapot Pilgrim |
| Lantern Weaver | 26 | **Eclipse Egg**, **The Endless Index***, **Harbor Whale***, **Pillow Oracle**, **Reel Owl**, Bookmark Golem, Dough Foreman, Lens Owl*, Moon Jelly, The Folded Courier, The Forgotten Patchwork, The Sleeping Friend, Thimble Moth, Velvet Champion, Candle Snail, Spindle Wren, Star Sock, Storm Kitten*, Teacup Cricket, Brass Acorn, Comet Kite, Court Herald*, Mailbox Imp, Orbit Moth, Paper Plane Pigeon*, Teapot Pilgrim |
| Hourglass Stag | 26 | **Eclipse Egg**, **Kite Dragon***, **The Last Bell**, **Clockwork Piper**, **Harbor Whale***, **Return Captain**, **Tea Dragon***, Spring Duck, Top Beetle*, Volt Eel*, Anchor Snail, Balloon Mallet, Coral Teacup, Lamp Goat*, Pear Moth, Raincoat Frog*, Ribbon Otter, Sail Squid, Bubble Koi, Button Beetle, Dream Compass, Shanty Sea Lion*, Shell Postman, Staple Mole, Static Ferret*, Tin Tick |
| Rain Archivist | 25 | **Kite Dragon***, **The First Seed***, **Harbor Whale***, **Paper Inventor**, **Return Captain**, Rain Heron*, Volt Eel*, Yo-yo Squirrel*, Anchor Snail, Balloon Mallet, Bristle Ball, Citrus Toad, Coral Teacup, Paper Devil, Pear Moth, Raincoat Frog*, Ribbon Otter, Sail Squid, Watering Snail*, Bubble Koi, Mushroom Librarian, Radish Jester, Seed Hedgehog, Shanty Sea Lion*, Shell Postman |
| Kite Admiral **Mới** | 23 | **Eclipse Egg**, **Kite Dragon***, **Harbor Whale***, **Pillow Oracle**, **Return Captain**, Dough Foreman, Moon Jelly, The Folded Courier, The Sleeping Friend, Volt Eel*, Anchor Snail, Balloon Mallet, Coral Teacup, Raincoat Frog*, Ribbon Otter, Sail Squid, Spindle Wren, Star Sock, Storm Kitten*, Bubble Koi, Comet Kite, Orbit Moth, Paper Plane Pigeon* |
| Sundae Duchess **Mới** | 22 | **Sundae Golem***, **The Impossible Tailor**, **Coil Kettle**, **Iron Locomotive**, **Tea Dragon***, Rain Heron*, Saffron Mechanist, Shovel Penguin*, Baker Mouse*, Bottle Gull*, Candle Snail, Ice Cream Owl, Monster Marble, Teacup Cricket, Velvet Enchantress, Boba Tadpole*, Bread Crab, Fridge Wisp, Hood Wisp, Kettle Bat, Snow Spoon, Teapot Pilgrim |
| Bonsai Clocksmith **Mới** | 23 | **The First Seed***, **The Last Bell**, **Clockwork Piper**, **Coil Kettle**, **Paper Inventor**, Saffron Mechanist, Spring Duck, Top Beetle*, Yo-yo Squirrel*, Bristle Ball, Citrus Toad, Lamp Goat*, Paper Devil, Pear Moth, Watering Snail*, Button Beetle, Dream Compass, Mushroom Librarian, Radish Jester, Seed Hedgehog, Staple Mole, Static Ferret*, Tin Tick |
| Juggling Jester **Mới** | 24 | **The Endless Index***, **The Lost Carousel***, **Mirror Newt**, **Reel Owl**, **Ringmaster Jack***, Bookmark Golem, Lens Owl*, The Folded Courier, The Forgotten Patchwork, Thimble Moth, Velvet Champion, Balloon Mallet, Carousel Pony, Spindle Wren, Violet Remnant, Brass Acorn, Court Herald*, Crayon Gremlin, Dice Crab, Jelly Swordsman, Mailbox Imp, Mask Pepper, Ribbon Spider, Sock Cyclops |
| Bell Diver **Mới** | 22 | **Kite Dragon***, **The Impossible Tailor**, **Harbor Whale***, **Iron Locomotive**, **Return Captain**, Rain Heron*, Shovel Penguin*, Volt Eel*, Anchor Snail, Balloon Mallet, Coral Teacup, Ice Cream Owl, Monster Marble, Raincoat Frog*, Ribbon Otter, Sail Squid, Boba Tadpole*, Bubble Koi, Hood Wisp, Shanty Sea Lion*, Shell Postman, Snow Spoon |

## 9. Phụ lục: vũ khí ngoài Arsenal

Các vũ khí cũ (không thuộc 60 vũ khí Arsenal) vẫn có trong `RunCatalog.asset`: vũ khí của hero cũ, của companion khởi đầu và vòng quay companion. Mô tả lấy từ `GameplaySummary` khi có.

| Vũ khí | Kiểu | Người cầm cố định | Mô tả trong game |
|---|---|---|---|
| KeyCleaver | Cận chiến | hero Lantern Knight | (chưa có mô tả) |
| TeapotPopgun | Tầm xa | hero Supper Mortar | (chưa có mô tả) |
| UmbrellaLance | Cận chiến | bạn khởi đầu của Soap Skater | (chưa có mô tả) |
| MailboxLauncher | Tầm xa | hero Postmaster | (chưa có mô tả) |
| BellFork | Tầm xa | tạm của hero Velvet Regent | (chưa có mô tả) |
| BookPaddle | Cận chiến | bạn khởi đầu của Postmaster | (chưa có mô tả) |
| CrescentBow | Tầm xa | bạn khởi đầu của Lantern Knight | (chưa có mô tả) |
| RuneStaff | Tầm xa | bạn khởi đầu của Ivory Sovereign, tạm của hero The Root Oracle | (chưa có mô tả) |
| DreamStamp | Cận chiến | hero Ivory Sovereign | (chưa có mô tả) |
| CloudScissors | Cận chiến | bạn khởi đầu của Patchwork Host | (chưa có mô tả) |
| SleepCamera | Tầm xa | hero Archive Curator | (chưa có mô tả) |
| TinFish | Tầm xa | bạn khởi đầu của Rain Archivist | (chưa có mô tả) |
| DreamIron | Cận chiến | bạn khởi đầu của The Root Oracle | (chưa có mô tả) |
| RubberHammer | Cận chiến | bạn khởi đầu của The Tide Singer, tạm của hero Citrus Marshal | (chưa có mô tả) |
| AlarmMortar | Tầm xa | hero Hourglass Stag | (chưa có mô tả) |
| MusicBox | Tầm xa | hero Tin Virtuoso | (chưa có mô tả) |
| SpoonSabre | Cận chiến | bạn khởi đầu của The Treasure Clerk, tạm của hero Night Maw | (chưa có mô tả) |
| ToastCleaver | Cận chiến | bạn khởi đầu của Supper Mortar | (chưa có mô tả) |
| LollipopLance | Cận chiến | bạn khởi đầu của Thorn Treasurer | (chưa có mô tả) |
| PaperFan | Cận chiến | bạn khởi đầu của Lady Loaded Dice, tạm của hero Petal Citadel | (chưa có mô tả) |
| CorkscrewRapier | Cận chiến | hero Button Cavalier | (chưa có mô tả) |
| TeabagFlail | Cận chiến | bạn khởi đầu của Lantern Weaver | (chưa có mô tả) |
| UmbrellaHook | Cận chiến | bạn khởi đầu của The Mirror Keeper | (chưa có mô tả) |
| PizzaWheel | Cận chiến | bạn khởi đầu của Petal Citadel | (chưa có mô tả) |
| MoonScythe | Cận chiến | hero Moon Prowler | (chưa có mô tả) |
| ForkTrident | Cận chiến | bạn khởi đầu của Drowsy Seer | (chưa có mô tả) |
| MaracaHammer | Cận chiến | bạn khởi đầu của Tin Virtuoso | (chưa có mô tả) |
| TapeWhip | Cận chiến | bạn khởi đầu của Verdant Matron | (chưa có mô tả) |
| ButtonBow | Tầm xa | bạn khởi đầu của Velvet Regent | (chưa có mô tả) |
| LeafCrossbow | Tầm xa | hero Verdant Matron | (chưa có mô tả) |
| InkPistol | Tầm xa | bạn khởi đầu của Button Cavalier, tạm của hero Patchwork Host | (chưa có mô tả) |
| BubbleWand | Tầm xa | hero Soap Skater | (chưa có mô tả) |
| StarKazoo | Tầm xa | bạn khởi đầu của Citrus Marshal | (chưa có mô tả) |
| ShellCannon | Tầm xa | bạn khởi đầu của The Comet Courier | (chưa có mô tả) |
| AcornSling | Tầm xa | hero The Treasure Clerk | (chưa có mô tả) |
| CrescentBoomerang | Tầm xa | tạm của hero Penny Broker | (chưa có mô tả) |
| DartCompass | Tầm xa | bạn khởi đầu của Night Maw, tạm của hero Lady Loaded Dice | (chưa có mô tả) |
| LanternStaff | Tầm xa | hero Lantern Weaver | (chưa có mô tả) |
| SnowGlobeOrb | Tầm xa | hero The Mirror Keeper | (chưa có mô tả) |
| RibbonHarp | Tầm xa | hero The Tide Singer | (chưa có mô tả) |
| DreamOrbStaff | Tầm xa | tạm của hero Rain Archivist | (chưa có mô tả) |
| FireExtinguisher | Tầm xa | hero Copper Cauldron | (chưa có mô tả) |
| PlasmaTrident | Tầm xa | hero Storm Fencer | (chưa có mô tả) |
| MoonFeatherFan | Tầm xa | bạn khởi đầu của Moon Prowler | (chưa có mô tả) |
| TwinSabres | Cận chiến | bạn khởi đầu của The Lone Jelly | (chưa có mô tả) |
| ClockCrescent | Cận chiến | hero The Comet Courier | (chưa có mô tả) |
| ParasolNeedle | Cận chiến | bạn khởi đầu của Hourglass Stag, tạm của hero Thorn Treasurer | (chưa có mô tả) |
| StampHammer | Cận chiến | bạn khởi đầu của Penny Broker | (chưa có mô tả) |
| ClockworkYoYo | Cận chiến | bạn khởi đầu của Archive Curator | Homing throw. Returns on its tether. |
| ScrapMagnet | Tầm xa | — | Magnet bolt yanks its target and 3 neighbours in. |
| NeedleSinger | Tầm xa | hero China Marionette | 3 piercing needles. Briefly slows on hit. |
| SpringKnocker | Cận chiến | bạn khởi đầu của Copper Cauldron | Slow charge. Heavy splash and knockback. |
| DreamProjector | Tầm xa | tạm của hero The Lone Jelly | Projects a lingering damage pool. Dream only. |
| RibbonWheel | Cận chiến | bạn khởi đầu của China Marionette | Two delayed ribbon cuts in a wide arc. |
| MoonseedOrb | Tầm xa | hero Drowsy Seer | One slow orb blooms after impact. |
| AfterimageSabre | Cận chiến | bạn khởi đầu của Storm Fencer | Slash now. A ghost slash follows. |
| SatelliteDisc | Tầm xa | — | Circles one target, then returns. |

## 10. Phụ lục: liên kết build

`RunCatalog.BuildLinks` — các cặp relic được nối với nhau trong UI (không có thưởng bộ ẩn).

| Liên kết | Item |
|---|---|
| Engine Clockwork | Key Oil, Clock Spring |
| Engine Economy | Piggy Mail, Coupon Book |
| Engine Patrol | Coin Compass, Wandering Map |
| Engine Loot | Treasure Whisper, Chest Thunder |
| Signature Harvest | Harvest Receipt, Carbon Ribbon |
| Signature Office | Reflective Ink, Office Lens, Carbon Ribbon |
| Signature Tea | Tea Coaster, Thermal Ink |
| Signature Chorus | Scratch Record, Carbon Ribbon |
| Signature Treasure | Treasure Ribbon, Harvest Receipt |
| Signature Toys | Patch Spring, Repair Coupon |

## 11. Ghi chú / chênh lệch

### 11.1 Đối chiếu số lượng với mục tiêu v5

| Loại | Mục tiêu | Đếm được | Kết quả |
|---|---:|---:|---|
| Hero | 36 | 36 (31 trong game + 5 mới) | khớp |
| Companion | 94 | 94 (66 + 28 mới) | khớp |
| Bond | 33 | 33 (8 chủ đề + 25 phụ), cả 33 đã có trong `RunCatalog.asset` | khớp |
| Vũ khí Arsenal | 60 | 60 (47 + 13 mới) | khớp |
| Item | 200 | 200 (170 + 30 mới) | khớp |
| Họ item | 18 | 18 | khớp |
| Bậc Crimson | 0 | 0 item, 0 companion mang Crimson trong dữ liệu | khớp (enum `Crimson` vẫn còn trong code để save cũ không lệch) |

Dữ liệu Unity hiện tại khớp hoàn toàn với `expansion_design.py` / `expansion.json` cho 66 companion cũ (bond, bậc), 31 hero cũ (chủ đề, bond đặc trưng, họ, rival), 170 item cũ (bậc, giá, chủ đề, họ) và 33 bond (hiệu ứng, giá trị, ngưỡng).

### 11.2 Chênh lệch và chỗ còn thiếu

1. **Sundae Duchess chưa có công thức thanh** trong `HeroGauge.cs` (4 hero mới kia đã có). Thiết kế v3: thanh tự tan theo thời gian, nạp nhanh hơn khi bị đánh và khi hạ địch đang đóng băng.
2. **5 hero mới chưa có kit trong `HeroKits.cs`** (biến hình, nội tại). Mục 2.2 ghép từ v3 §1.2 + các sửa ở v5 §3/§4/§5/§8; nội tại v4 (đã bỏ) không còn tài liệu nên không ghi.
3. **Kỹ năng Mythic trong code khác thiết kế:**
   - Iron Locomotive: code (`CompanionSkills.cs`) vẫn là "mỗi 7s lao 4 ô, đóng băng địch trúng 1s"; v5 §4 đổi thành **còi băng** đóng băng 3 địch đầu tiên 1s.
   - Paper Inventor: code là "mỗi đòn thứ 5 bắn phi tiêu giấy vào 2 địch gần nhất"; v3 §4.4 là **dựng tháp giấy** (art `PaperInventor_Tower` đã vẽ).
   - Eclipse Egg (Prismatic, đang trong game): kỹ năng nở 3 dạng (v3) chưa có trong code combat; art 3 dạng đã vẽ.
   - The Last Bell, The Impossible Tailor: v3 ghi "giữ kỹ năng hiện có" — kỹ năng hiện có chính là talent ("Pet Master", "Overtime King"), chưa có kỹ năng Prismatic riêng.
4. **Kỹ năng ghi ở nhiều bản:** The Endless Index có ba phiên bản (v3: +2% sát thương mỗi loài; v5 §4: đòn thứ 3 chép sang 2 địch; v5 §8: trang sách → +1 tách đạn Runic School) — tệp này dùng v5 §8. The First Seed: v3 "mọc cây con" (art `FirstSeed_Sapling` đã vẽ) nhưng v5 §5 đổi thành bờ rào gai (art `FirstSeed_Hedge`). SpinningTop: v5 §5 "hút quái vào giữa" rồi v5 §8 "hất ra ngoài" — dùng §8. SonarHarpoon: v5 §4 "tiếng vọng" rồi §8 "phao dò" — dùng §8.
5. **Vũ khí của companion mới C/U/R:** v5 §9 nói Rain Heron "không còn vũ khí cố định", nhưng không nói rõ 10 companion mới C/U/R khác (Yo-yo Squirrel → YoYo, Baker Mouse → RollingPin…) có cầm cố định vũ khí bản 3 hay vào vòng quay chung. Tệp này ghi "(bản 3)". Raincoat Frog (PlungerPopper) và Lamp Goat (DragonLanternGlaive) được gán vũ khí Arsenal đã có.
6. **Talent tham chiếu bond đã bỏ:** Candle Snail (talent "Applehead") buff "Fruit Basket allies" — bond Fruit Basket đã bị cắt nên talent này không còn tác dụng. Một số talent khác buff bond mà companion đó không còn thuộc (vd. Pillow Oracle — Clockwork Union; Ice Cream Owl — Runic School; Staple Mole — Lucky Leaves); vẫn chạy, chỉ là không cùng bond.
7. **Item mới thiếu mô tả:** Treasure Mimic, Duelist's Ribbon, Frost Ledger (Mythic) chỉ có tên, chủ đề và họ trong v5 §6b; không tài liệu nào ghi tác dụng.
8. **Item mới chưa có giá:** `expansion.json` để `price: 0` cho cả 30 item mới.
9. **Item mới chỉ gắn họ:** v5 §6b ghi chủ đề cho từng món (vd. Kite Tail Ribbon — Tinh Tú) nhưng theo luật v5 §9, 20 món không phải Common / không thuộc họ Đội chỉ gắn họ, nên `expansion.json` không gắn chủ đề cho chúng. Bảng item ghi "— (chỉ gắn họ; thiết kế: …)".
10. **Tác dụng item theo thiết kế chưa vào dữ liệu Unity:** Clockwork Heart và Borrowed Crown (v5 §5) trong `RunCatalog` vẫn chỉ là chỉ số; Crimson Seal / Crimson Spring chưa có cơ chế thêm (v3 §6); Iron Apron, Mending Moth, Echo Postage, Starlight Quilt, Tarnished Crown được hứa "thêm cơ chế nhỏ" (v3 §7.2) nhưng chưa có mô tả cụ thể.
11. **Phân bậc item khác bảng mục tiêu cũ của v3 §7.4** (Common 50 · Uncommon 55 · Rare 50 · Mythic 35 · Prismatic 10): hiện là 43 · 66 · 44 · 38 · 9. Bảng v3 đã bị v5 §6b thay (bỏ các món Common yếu, bỏ Last Wish Coin); tổng 200 vẫn đúng.
12. **Bond đặc trưng chỉ kích hoạt ở một bộ:** Jade Nursery (Verdant Matron) và Exhibit Hall (Archive Curator) là bond riêng, đúng thiết kế. Exhibit Hall không là "bond đặc trưng" chính thức của Archive Curator trong `SIGNATURE` (bond đặc trưng là Runic School); nó vào bộ qua luật companion riêng.
13. **Art:** 10 sheet nhân vật (5 hero, 28 companion, 4 dạng phụ), 13 vũ khí mới, 5 món biến hình đã vẽ; còn thiếu 62 thanh Gauge2 của 31 hero cũ, 24 icon item mới (6 đã vẽ: Tally Chalk, Coin Sash, Fish Cracker, Postage Stamp, Frost Salt, Refund Slip) và 6 icon bond mới (theo `Docs/Expansion_Art_Progress.txt` và file trong `Assets/ArcaneBrigade/Art`). Chưa nội dung mới nào được nối vào `RunCatalog.asset`.
14. **Tên hiển thị khác Id:** một số hero hiển thị trong game bằng tên khác Id mà tài liệu thiết kế dùng: `RootOracle` = The Root Oracle, `LoneJelly` = The Lone Jelly, `GlassDice` = Lady Loaded Dice, `TideSinger` = The Tide Singer, `CometRunner` = The Comet Courier, `TreasureAcorn` = The Treasure Clerk, `MirrorKeeper` = The Mirror Keeper. Companion: `RainbowBell` = The Last Bell, `RainbowTailor` = The Impossible Tailor, `SecretPaperBird` = The Folded Courier, `SecretPatchBear` = The Forgotten Patchwork, `SecretDreamLamb` = The Sleeping Friend, `LighthouseWhale` = Harbor Whale, `JackInTheBox` = Ringmaster Jack.
15. **Ngoài phạm vi:** quái thường, boss, sự kiện, đồ cốt truyện (`ActOneCanon.StoryItem`) không liệt kê ở đây.
