# Mở rộng nội dung — bản 5 (18/10/2026): sửa 10 vấn đề của lần rà soát thứ hai

Thay bản 4 (`Content_Expansion_v4.md`, đã bỏ). Vẫn dựa trên bản 3 ([Content_Expansion_Proposal.md](Content_Expansion_Proposal.md))
cho những gì không nhắc lại ở đây. Số lượng: **36 hero, 94 companion (+9 theo đề xuất agent), 60 vũ khí Arsenal, 33 bond (+3), 200 item**
(30 item mới gồm đủ 12 món agent đề xuất; bỏ các món đồ ăn/đồ uống Common yếu).
Nguồn dữ liệu: `Tools/Content/expansion_design.py` (kiểm tự động, hiện **0 lỗi**).

---

## 1. Luật bộ cố định

Bộ của hero = companion thuộc **2 bond chủ đề** + toàn bộ thành viên **bond phụ đặc trưng** + item **Common** + item thuộc 2 chủ đề +
item thuộc **họ item** của hero.

| Luật | Giá trị |
|---|---|
| Bond chủ đề mỗi hero | 2 (Patchwork Host 3) |
| Bond phụ đặc trưng | 1 mỗi hero, tối đa 2 hero mỗi bond, chọn theo chất của hero |
| Họ item | **18 họ hẹp**, tối đa 3 hero mỗi họ |
| Hai hero cùng cặp chủ đề | phải khác nhau **≥3 companion** và **khác họ item** (kiểm tự động) |
| Cỡ bộ companion | 18–28 (Patchwork 36) — nới thêm 2 vì 9 companion mới |
| Họ của item | theo loại (món ăn / đồ uống) trước, rồi theo cơ chế, rồi theo chỉ số dương đầu tiên; bỏ qua chỉ số phạt (địch nhanh hơn, thêm quái, thêm máu quái, điểm số) |
| Mythic và Prismatic theo chủ đề | chia đều (Mythic hiện 3–4 món mỗi chủ đề; 6 Prismatic mới lấp Kỳ Lạ và Thủy Triều) |
| Companion Rare | **+1 thành viên cho bond phụ của nó khi bond đó đã kích hoạt** (thêm cấp, không tự bật bond) |
| Vũ khí đã khóa | mỗi companion Mythic/Prismatic luôn cầm vũ khí riêng (bảng §9), vũ khí đó bị loại khỏi vòng quay của companion khác; Rain Heron (Rare) không còn vũ khí cố định |

## 2. Ba mươi sáu hero

| Hero | Bond chủ đề | Bond đặc trưng | Họ item |
|---|---|---|---|
| LanternKnight | Bếp Ấm · Bộ Máy | Lantern Guild | Đội |
| LanternWeaver | Tinh Tú · Thư Viện | **Lantern Guild** | Tầm xa |
| Postmaster | Thư Viện · Thủy Triều | Paper Wings | **Đánh dấu** |
| KiteAdmiral | Tinh Tú · Thủy Triều | Paper Wings | Tầm xa |
| DrowsySeer | Tinh Tú · Bếp Ấm | Hat Society | Shop |
| IvorySovereign | Thư Viện · Sương Giá | Hat Society | Shop |
| RootOracle | Vườn Cây · Sương Giá | Rootkin | Sống sót |
| LoneJelly | Kỳ Lạ · Thủy Triều | Crossing Knights | **Merge** |
| ButtonCavalier | Bộ Máy · Kỳ Lạ | Crossing Knights | Cận chiến |
| GlassDice | Kỳ Lạ · Vườn Cây | Lucky Leaves | **May rủi** |
| TreasureAcorn | Vườn Cây · Thư Viện | Lucky Leaves | Rương |
| TinVirtuoso | Bộ Máy · Kỳ Lạ | Morning Bells | Chí mạng |
| HourglassStag | Bộ Máy · Thủy Triều | **Morning Bells** (đồng hồ báo thức) | **Trì hoãn** |
| CitrusMarshal | Vườn Cây · Bếp Ấm | Springstep | Va chạm |
| TideSinger | Thủy Triều · Tinh Tú | **Sea Shanty** (mới) | Đồ uống |
| CometRunner | Tinh Tú · Bộ Máy | Quicksoles | Tốc độ |
| SoapSkater | Thủy Triều · Sương Giá | Quicksoles | **Đóng băng** |
| MirrorKeeper | Sương Giá · Kỳ Lạ | Unbroken | Sống sót |
| CopperCauldron | Bếp Ấm · Kỳ Lạ | Unbroken | **Va chạm** |
| VerdantMatron | Vườn Cây · Bếp Ấm | Jade Nursery | **Merge** |
| ChinaMarionette | Sương Giá · Kỳ Lạ | Wild Stitch | Vàng |
| VelvetRegent | Thư Viện · Bộ Máy | Velvet Court | Đội |
| NightMaw | Kỳ Lạ · Tinh Tú | Prizefighters | Đồ ăn |
| ThornTreasurer | Vườn Cây · Thư Viện | Little Horns | Vàng |
| PetalCitadel | Vườn Cây · Tinh Tú | Longbow | Tầm xa |
| PatchworkHost | Bếp Ấm · Kỳ Lạ · Vườn Cây | Big Top | Đội |
| JugglingJester | Kỳ Lạ · Thư Viện | Big Top | Chí mạng |
| ArchiveCurator | **Bộ Máy · Sương Giá** (cặp chưa ai dùng) | Runic School + **Exhibit Hall** (bond riêng, mới) | **Theo loài đã hạ** |
| StormFencer | Bộ Máy · Tinh Tú | **Static Club** (mới) | Chí mạng |
| PennyBroker | Thư Viện · Bếp Ấm | Clockwork Union | Vàng |
| BonsaiClocksmith | Vườn Cây · Bộ Máy | Clockwork Union | **Merge** |
| MoonProwler | Tinh Tú · Sương Giá | Masked Carnival | Cận chiến |
| SupperMortar | Bếp Ấm · Thủy Triều | Sweet Tooth | Đồ ăn |
| SundaeDuchess | Sương Giá · Bếp Ấm | Sweet Tooth | **Đóng băng** |
| RainArchivist | Thủy Triều · Vườn Cây | Rain Script | Rương |
| BellDiver | Thủy Triều · Sương Giá | Rain Script | **Đánh dấu** |

18 họ: Tầm xa (3) · Đánh dấu (2) · Shop (2) · Merge (3) · Cận chiến (2) · May rủi (1) · Rương (2) · Đồ uống (1) · Đóng băng (2) ·
Tốc độ (1) · Va chạm (2) · Sống sót (2) · Vàng (3) · Đồ ăn (2) · Đội (3) · Chí mạng (3) · Trì hoãn (1) · Theo loài (1).

Starter mới (đều trong bộ, không phải nhân vật cốt truyện, companion đặc biệt hay quà; không hero nào bắt đầu với Mythic): Ivory → Ice Cream Owl ·
Moon Prowler → Star Sock · Lantern Weaver → Thimble Moth · Hourglass Stag → Dream Compass · Root Oracle → Mushroom Librarian ·
Drowsy Seer → Candle Snail · Archive Curator → Fridge Wisp · Mirror Keeper → Violet Remnant · Copper Cauldron → Teacup Cricket.

## 3. Nội tại "+1 cấp bond đặc trưng" chỉ còn cho hero sống bằng bond

Giữ cho LanternKnight, VelvetRegent, VerdantMatron và 5 hero mới. Chín hero khác nhận lại nội tại riêng, theo họ item của mình:

| Hero | Nội tại đội mới |
|---|---|
| RootOracle | Rễ che chở: đồng đội đứng trong tầm 3 của hero đỡ một viên đạn bắn vào hero, mỗi 5s một lần |
| PetalCitadel | Relic nhân đôi: mua một relic đã có thì được hoàn 3 vàng |
| ThornTreasurer | Gai trả tiền: mỗi lần gai phản đòn có 10% rơi 1 xu |
| SoapSkater | Đồng đội đánh trúng địch đang bị làm chậm thì đóng băng nó 0.3s (mỗi địch một lần mỗi 4s) |
| MoonProwler | Đồng đội cận chiến đánh từ phía sau thì luôn chí mạng |
| TideSinger | Mỗi đồ uống mua được: +1 vàng thu nhập mỗi wave về sau (tối đa +5) |
| CopperCauldron | Mỗi đòn hero nhận trong wave: đồng đội +2 sát thương va chạm (tối đa +10) |
| LanternWeaver | Địch bị ghim chậm 15% |
| IvorySovereign | Mỗi Mythic hoặc Prismatic mua được: đội +5% sát thương cả run (tối đa +25%) |

## 4. Bỏ kiểu "đánh dấu rồi +X%" và "quét một làn" ở chỗ không phải của chúng

| Ở đâu | Bản 4 | Bản 5 |
|---|---|---|
| Ivory — Coronation | đường phán quyết đánh dấu +15% | **đường phán quyết xử tử quái thường dưới 15% máu** |
| Tea Dragon | hơi trà đánh dấu +15% | **đặt một ấm trà phía trước; ấm reo rồi nổ hơi bỏng, lan sang địch kế bên** |
| The Endless Index | ký tự +20% cho Runic School | **mỗi đòn thứ 3, ký tự chép đòn đó sang 2 địch gần** |
| SonarHarpoon | đánh dấu và vọng âm | **không đánh dấu: tiếng vọng đánh địch gần nhất phía sau mục tiêu** |
| Velvet — Royal Parade | xung phong theo đường | **Phong tước: mọi đồng đội đánh như được thêm 1 sao trong 6s** |
| Iron Locomotive | xúc tuyết dọn làn | **còi băng: một luồng hơi lạnh phía trước đóng băng 3 địch đầu tiên 1s** |
| RollingPin | cán phẳng đẩy lùi theo làn | **để lại một dải bột dính giữ chân địch 0.5s** |
| Kite Admiral — biến hình | hạm đội diều quét làn | **Hạm đội diều: 4 diều bay trên 4 đồng đội tầm xa gần nhất; đạn của họ tách đôi khi diều còn bay** |

Đánh dấu chỉ còn ở: Postmaster (tem), Bell Diver (dò), Return Captain (neo) và họ item Đánh dấu.

## 5. Prismatic, Rare và các trùng lặp khác

| Món | Bản 5 |
|---|---|
| Clockwork Heart | mỗi lần merge lên dây cót: đội +3% tốc đánh cả run (tối đa +30%) |
| Borrowed Crown | đồng đội nhiều sao nhất bắn thêm vũ khí của hero |
| Storybook Ending | quái cuối cùng mỗi wave rơi một trang: +1 chỉ số đội ngẫu nhiên vĩnh viễn |
| Mirror of Twins | chỉ thuộc chủ đề của Patchwork Host (Bếp Ấm · Kỳ Lạ) |
| Wind Chime (Rare) | mỗi 10 lần chí mạng của đội: 3 phát kế của mọi đồng đội +1 xuyên |
| Cuckoo Pendulum (Mythic) | mỗi 12s đòn kế tiếp của mọi đồng đội chí mạng |
| Kite Tail Ribbon (Mythic) | đạn chí mạng của đồng đội tầm xa nảy thêm 1 lần |
| Sundae Duchess | bỏ "+2% tốc đánh mỗi đồ uống"; thay bằng **mỗi địch đóng băng bị hạ: +1 vàng (tối đa 5 mỗi wave)** |
| Kite Admiral | thanh nạp từ **đạn của chính hero trúng xa hơn 4 ô**; bỏ "giảm 50% lượt đổi đầu" (trùng Copper); thay bằng **đồng đội tầm xa +1 xuyên khi thanh đầy** |
| Juggling Jester | thanh nạp từ **chí mạng của đồng đội**; nội tại hero thành **chí mạng của hero tung một quả bóng rơi xuống một địch ngẫu nhiên (50% sát thương)** |
| Bell Diver | bỏ "đạn của địch đã dò chậm 30%" (trùng Mirror); thay bằng **địch đã dò nhận thêm 1 lần nảy từ đạn đồng đội** |
| Ringmaster Jack (Jack-in-the-Box, giữ art đã vẽ) | **gây chú ý: mỗi 6s xoay bóng trên mũi, quái gần quay sang đánh nó 1.5s** |
| Harbor Whale (Lighthouse Whale, giữ art) | phun nước vào **địch gần hero nhất** (không còn "chỗ đông nhất", trùng The Lost Carousel) |
| The First Seed | **mọc một bờ rào gai phía trước chặn đường quái 3s** |
| JugglingPins | chùy trúng khi bay lên và khi rơi xuống; chùy kế chọn mục tiêu khác |
| BobaStraw | dòng trân châu lượn hình sóng, trúng mọi thứ trên đường lượn |
| WateringCan | làm ướt; địch ướt trúng đòn băng thì đóng băng (không còn mầm) |
| SpinningTop | con quay đứng xoáy phía trước 2s, hút quái nhỏ vào giữa |

## 6. Nội dung mới theo đề xuất agent

**3 bond mới** (ngưỡng 2):

| Bond | Tác dụng | Cho |
|---|---|---|
| **Static Club** | đòn của đồng đội có 4%/cấp lan sang một địch gần (50% sát thương) | Storm Fencer (bond đặc trưng) |
| **Sea Shanty** | relic rẻ hơn 6%/cấp | Tide Singer (bond đặc trưng) |
| **Exhibit Hall** | đồng đội +10%/cấp sát thương lên loài đã ghi | riêng Archive Curator (như Jade Nursery của Verdant) |

**9 companion mới** (94 tổng):

| Companion | Bậc | Bond | Ghi chú |
|---|---|---|---|
| Static Ferret | Common | Bộ Máy · Static Club | đòn để lại tĩnh điện lan một lần |
| Storm Kitten | Uncommon | Tinh Tú · Static Club | mỗi 6s một đám mây phía trước giáng một tia sét |
| Volt Eel | Rare | Thủy Triều · Static Club | địch bị sốc lan thêm 1 lần |
| Shanty Sea Lion | Common | Thủy Triều · Sea Shanty | mỗi đòn thứ 3 một nốt nhạc đẩy địch lùi |
| Bottle Gull | Uncommon | Bếp Ấm · Sea Shanty | mua vào tính như 1 đồ uống |
| Pinned Beetle | Common | Thư Viện · Exhibit Hall | mẫu vật bảo tàng, chỉ ở bộ Archive Curator; đòn ghim địch 0.3s |
| Fossil Armadillo | Uncommon | Sương Giá · Exhibit Hall | mẫu vật bảo tàng, chỉ ở bộ Archive Curator; địch đánh vào nó bị choáng 0.3s |
| Specimen Slime | Rare | Bộ Máy · Exhibit Hall | mẫu vật bảo tàng, chỉ ở bộ Archive Curator; hạ loài đã ghi thì tách slime nhỏ (tối đa 2) |
| Court Herald | Common | Thư Viện · Hat Society | kèn: đầu mỗi wave, đòn đầu của mọi đồng đội bắn thêm 1 viên |

Shell Postman chuyển từ Lantern Guild sang Sea Shanty (bưu tá ven biển) để Sea Shanty có đủ người ở các bộ Thủy Triều; Tin Tick chuyển từ
Clockwork Union sang Static Club. Shell Postman là nhân vật cốt truyện: nó vẫn chỉ xuất hiện theo luật cốt truyện, chỉ bond của nó đổi. Starter của
Ivory giữ là Ice Cream Owl (Court Herald chỉ vào game khi có art).

## 6b. Ba mươi item mới (chốt sau lần rà thứ ba): giữ 200 item

| Bậc | Món (chủ đề · họ) |
|---|---|
| Common (3, dùng chung) | Tally Chalk — +1% sát thương mỗi 25 quái hạ trong wave, tối đa 8% · Coin Sash — +0.5 tầm nhặt khi đang chạy · Fish Cracker — món ăn, +6% chí mạng |
| Uncommon (5) | Postage Stamp — +10% sát thương lên địch bị đánh dấu (Thư Viện · Đánh dấu) · Frost Salt — đóng băng lâu hơn 0.3s (Sương Giá · Đóng băng) · **Refund Slip** — mỗi shop một lần bán lại một món đủ giá (Bếp Ấm · Shop) · Whetstone Wafer — món ăn: đòn cận chiến chém thêm 1 địch 40% (Kỳ Lạ · Đồ ăn + Cận chiến) · Late Fee — sát thương theo thời gian và trì hoãn +25% (Bộ Máy · Trì hoãn) |
| Rare (6) | Haggler's Abacus — mỗi lượt đổi: mọi món rẻ hơn 1, tối đa −30% (Thư Viện · Shop) · Slipstream Scarf — đồng đội +1% tốc đánh mỗi 5% tốc chạy cộng thêm của hero, tối đa 15% (Tinh Tú · Tốc độ) · Brain Freeze — chí mạng lên địch đóng băng vỡ thành 3 mảnh băng (Sương Giá · Đóng băng) · Wind Chime — mỗi 10 lần chí mạng của đội: 3 phát kế +1 xuyên (Bộ Máy · Chí mạng) · **Specimen Pin** — +1% chí mạng mỗi loài mới hạ trong run, tối đa 20% (Thư Viện · Theo loài) · **Star Polish** — đồng đội 3★ +1 viên đạn (Vườn Cây · Merge) |
| Mythic (11) | Kite Tail Ribbon (Tinh Tú · Tầm xa) · Melting Sundae (Sương Giá · Đóng băng) · Cuckoo Pendulum (Bộ Máy · Chí mạng) · Diving Bell Lantern (Thủy Triều · Đánh dấu) · Twin Ticket (Thư Viện · Merge) · Treasure Mimic (Vườn Cây · Rương) · Duelist's Ribbon (Kỳ Lạ · Cận chiến) · Frost Ledger (Sương Giá · Đóng băng + Theo loài) · **Undertow Anchor — mỗi 8s một con sóng từ tuyến đầu đẩy 5 địch gần nhất lùi 2 ô và làm ướt** (Thủy Triều) · **Funhouse Mirror — mỗi 10s vũ khí một đồng đội bị bóp méo: to gấp đôi, chậm một nửa trong 3s** (Kỳ Lạ) · **Splice Kit** — mỗi wave một lần, ba đồng đội 1★ khác loài gộp thành một con 2★ ngẫu nhiên (Bếp Ấm · Merge) |
| Prismatic (5) | The Endless Encore (Kỳ Lạ · Thủy Triều) · Moon Clock Key (Tinh Tú · Bộ Máy) · Mirror of Twins (Bếp Ấm · Kỳ Lạ) · Storybook Ending (Vườn Cây · Thủy Triều) · Dream Loom (Sương Giá · Thư Viện) |

Bỏ: Last Wish Coin (chỉ là kinh tế, không đủ Prismatic), Lucky Penny, Grafting Tape (mạnh hơn kit Lone Jelly), cùng các món đã bỏ ở
lần trước. Đổi: Window Seat Ticket → Refund Slip (trùng nội tại Penny), Tailwind Scarf → Coin Sash (trùng Slipstream Scarf).

## 8. Quyết định sau lần rà thứ ba

| Chủ đề | Quyết định |
|---|---|
| Họ item | item **gắn nhiều họ** (loại món + cơ chế + chỉ số), sửa họ của các món bị xếp sai; mọi họ có ít nhất 4 món không phải Common |
| Luật bond đối thủ | hai hero cùng cặp chủ đề: mỗi bên không thấy companion Common–Rare thuộc bond đặc trưng của bên kia; cặp đó phải cách nhau ≥6 companion |
| Static Club | thêm Tin Tick (bọ thiếc) làm thành viên thứ 4 |
| Rare | chỉ tính thêm 1 thành viên **khi bond đã kích hoạt** (thêm cấp, không tự bật bond) |
| Unbroken | đổi tác dụng thành **+hệ số sát thương** (không còn là bond sống sót thứ 3) |
| Starter | Root Oracle → Mushroom Librarian (Hood Wisp là nhân vật cốt truyện); Archive Curator → Fridge Wisp (Spindle Wren nằm ngoài bộ mới) |
| Court Herald | đầu mỗi wave, đòn đầu của mọi đồng đội bắn thêm 1 viên (không còn "đánh dấu +10%") |
| The Endless Index | mỗi loài mới bị hạ thêm một trang; ở 10 / 20 / 30 trang, đồng đội Runic School +1 tách đạn |
| SonarHarpoon | lao để lại một phao dò; địch đi qua phao bị dò |
| SpinningTop | xoáy tại chỗ, hất quái nhỏ văng **ra ngoài** |
| Bell Diver | địch đã dò nhận thêm 1 lần nảy từ đạn đồng đội (thay "đòn đầu chí mạng") |
| Root Oracle | mỗi 5s, địch gần hero nhất bị giữ chân 1s (thay "đồng đội đỡ đạn") |
| Lantern Weaver | đạn đồng đội xuyên qua địch bị ghim +1 xuyên |
| Sundae Duchess | địch đóng băng trúng chí mạng mất 5% máu tối đa |
| Lone Jelly | +2% sát thương mỗi lần merge (tối đa 25%) |
| Juggling Jester | thành viên Big Top tuyển vào có 10% là 2★ (thay "từ lượt đổi thứ 3 rẻ hơn 1") |
| Exhibit Hall | Pinned Beetle: đòn ghim địch 0.3s · Fossil Armadillo: cuộn tròn, địch đánh vào nó bị choáng 0.3s · Specimen Slime (Rare): hạ một loài đã ghi thì tách ra một slime nhỏ (tối đa 2) |
| Đổi hình (chưa vẽ) | Thunder Lamb → Storm Kitten, Conch Crooner → Shanty Sea Lion, Pressed Moth → Pinned Beetle, Fossil Tortoise → Fossil Armadillo |
| Giữ (đã vẽ) | Sundae Duchess dáng búp bê sứ, Ringmaster Jack, Harbor Whale (chỉ đổi tên) |

## 7. Art

Danh sách việc vẽ `Docs/Expansion_Art_Worklist.json` đã cập nhật theo bản 5: icon 30 item theo danh sách mới; thêm ấm trà của Tea
Dragon và bờ rào gai của The First Seed; bỏ mầm của WateringCan; đổi tên Lighthouse Whale → Harbor Whale, Jack-in-the-Box →
Ringmaster Jack (art đã vẽ giữ nguyên); thêm 9 companion mới (3 sheet nhân vật) và 6 icon bond mới (Paper Wings, Sweet Tooth, Big
Top, Static Club, Sea Shanty, Exhibit Hall). Những ảnh đã vẽ xong không phải vẽ lại.

## 9. Vũ khí khóa của companion Mythic và Prismatic (lần rà cuối)

| Companion | Bậc | Vũ khí | | Companion | Bậc | Vũ khí |
|---|---|---|---|---|---|---|
| Pillow Oracle | Mythic | FortuneGlobe | | The Last Bell | Prismatic | BuzzerBlock |
| Reel Owl | Mythic | DreamRacket | | The Impossible Tailor | Prismatic | TopiaryShears |
| Clockwork Piper | Mythic | WindupWrench | | Eclipse Egg | Prismatic | SaucerBlaster |
| Coil Kettle | Mythic | CatcherMitt | | Kite Dragon | Prismatic | TwinMoonStaff |
| Iron Locomotive | Mythic | SnowGlobeGrenade | | Sundae Golem | Prismatic | StarKnuckle |
| Paper Inventor | Mythic | GrabberClaw | | The Lost Carousel | Prismatic | CherryFuseBomb |
| Return Captain | Mythic | PlushShark | | The Endless Index | Prismatic | PaperShuriken |
| Mirror Newt | Mythic | KunaiFan | | The First Seed | Prismatic | AcornCluster |
| Tea Dragon | Mythic | HearthBrick | | | | |
| Harbor Whale | Mythic | PaperLanternBomb | | | | |
| Ringmaster Jack | Mythic | PinataBomb | | | | |

Không vũ khí nào trong bảng là vũ khí của hero hay starter. 20 item mới thuộc một họ (không phải Common, không phải họ Đội) chỉ
gắn họ, không gắn chủ đề, để hai hero cùng cặp chủ đề có shop khác nhau hơn. Quà lăng kính bù món ngoài bộ cho đủ 3 lựa chọn khi
bộ còn thiếu Prismatic (trong lúc chờ art).

## 10. Tác dụng chốt của 30 item mới và các món cũ được thêm cơ chế (bước 3)

Gom từ các bản đã duyệt (bản 4 §7.3, các lần rà, §5 và §6b của bản 5; bản sau thắng bản trước).

| Món | Bậc | Tác dụng |
|---|---|---|
| Tally Chalk | Common | +1% sát thương đội mỗi 25 quái hạ trong wave (tối đa 8%) |
| Coin Sash | Common | +0.5 tầm nhặt khi hero đang chạy |
| Fish Cracker | Common (đồ ăn) | +6% chí mạng |
| Postage Stamp | Uncommon | +10% sát thương lên địch bị đánh dấu |
| Frost Salt | Uncommon | đóng băng lâu hơn 0.3s |
| Refund Slip | Uncommon | mỗi shop một lần bán lại một món đủ giá |
| Whetstone Wafer | Uncommon (đồ ăn) | đòn cận chiến chém thêm 1 địch 40% |
| Late Fee | Uncommon | sát thương theo thời gian và sát thương trì hoãn +25% |
| Haggler's Abacus | Rare | mỗi lượt đổi trong shop: mọi món rẻ hơn 1 (tối đa −30%) |
| Slipstream Scarf | Rare | đồng đội +1% tốc đánh mỗi 5% tốc chạy cộng thêm của hero (tối đa 15%) |
| Brain Freeze | Rare | chí mạng lên địch đóng băng vỡ thành 3 mảnh băng |
| Wind Chime | Rare | mỗi 10 lần chí mạng của đội: 3 phát kế của mọi đồng đội +1 xuyên |
| Specimen Pin | Rare | +1% chí mạng mỗi loài mới hạ trong run (tối đa 20%) |
| Star Polish | Rare | đồng đội 3★ +1 viên đạn |
| Kite Tail Ribbon | Mythic | đạn chí mạng của đồng đội tầm xa nảy thêm 1 lần |
| Melting Sundae | Mythic | hero bị đánh thì địch gần đóng băng 0.5s (hồi 4s) |
| Cuckoo Pendulum | Mythic | mỗi 12s đòn kế tiếp của mọi đồng đội chí mạng |
| Diving Bell Lantern | Mythic | địch bị đánh dấu chịu +15% sát thương |
| Twin Ticket | Mythic | merge lên 3★ tặng thêm một bản 1★ cùng loại |
| Treasure Mimic | Mythic | rương đầu mỗi wave đánh giúp 10s |
| Duelist's Ribbon | Mythic | đòn thứ 4 của đồng đội cận chiến đánh hai lần |
| Frost Ledger | Mythic | mỗi địch đóng băng bị hạ: đội +2% sát thương trong wave (tối đa 20%) |
| Undertow Anchor | Mythic | mỗi 8s một con sóng từ tuyến đầu đẩy 5 địch gần nhất lùi 2 ô và làm ướt |
| Funhouse Mirror | Mythic | mỗi 10s vũ khí một đồng đội bị bóp méo: to gấp đôi, chậm một nửa trong 3s |
| Splice Kit | Mythic | mỗi wave một lần, ba đồng đội 1★ khác loài gộp thành một con 2★ ngẫu nhiên |
| The Endless Encore | Prismatic | bỏ 3s nghỉ sau biến hình (không giữ nấc) |
| Moon Clock Key | Prismatic | mọi bond đang kích hoạt tính thêm 1 thành viên |
| Mirror of Twins | Prismatic | companion mạnh nhất +60% sát thương và mỗi đòn bắn thêm 1 viên |
| Storybook Ending | Prismatic | quái cuối cùng mỗi wave rơi một trang: +1 chỉ số đội ngẫu nhiên vĩnh viễn |
| Dream Loom | Prismatic | mọi item Common mạnh gấp đôi |

**Cơ chế nhỏ cho các Mythic chỉ cộng chỉ số** (giữ chỉ số cũ; Crimson Seal/Spring giảm chỉ số vì xuống Mythic):

| Món | Cơ chế |
|---|---|
| Tarnished Crown | mỗi đòn thứ 8 của hero là đòn vương giả: gấp 3 sát thương |
| Starlight Quilt | mỗi wave một lần, đòn hạ gục hero chỉ để lại 1 máu và 1.5s an toàn |
| Iron Apron | địch đánh hero ở gần bị hất lùi 1.5 ô (mỗi giây một lần) |
| Mending Moth | 3s không bị đánh: hồi máu mỗi giây của hero gấp 3 |
| Echo Postage | đòn thường của đồng đội hạ địch thì vọng sang địch gần nhất (50% sát thương vũ khí) |
| Shield Blower | đạn địch bị đồng đội chặn được thổi ngược lại thành đạn ta, gấp đôi sát thương |
| Emerald Invoice | mỗi relic đang có trả bớt hóa đơn: điểm trừ sát thương giảm 2% mỗi món (hết ở 10 món) |
| Crimson Seal | va chạm đóng một ấn đỏ; 1s sau ấn nổ gấp 3 sát thương va chạm quanh địch (hero +50%, va chạm +16) |
| Crimson Spring | mỗi 4s đòn kế của mỗi đồng đội bật hai lần (tốc đánh +25%, tầm +25%) |
| Key Oil | +12% sát thương đội (từ +24%) |
| Truce Bottle | thêm −30% tốc chạy của hero, chỉ giữ 1 cái |

**Bốn Prismatic cũ:** Borrowed Crown — đồng đội nhiều sao nhất bắn thêm vũ khí của hero (+15% sát thương) · Clockwork Heart — mỗi
lần merge lên dây cót: đội +3% tốc đánh cả run, tối đa +30% (+10% tốc đánh) · Impossible Prism — đạn tách của đồng đội thành ba màu
lửa / băng / huyền bí (tách +3) · Living Thread — đồng đội cận chiến được nối chỉ: đòn của một con kéo theo một nhát 50% của con cận
chiến gần nhất (+4 sát thương cận chiến, +30% đẩy lùi).
