# Mechanic nhân vật, mô tả nội dung và phản hồi shop

Triển khai ngày 16/10/2026. Cả **31 nhân vật điều khiển** có mechanic riêng theo ID và **ba approach mỗi người**, tổng **93 lựa chọn**. Các innate về kinh tế, starter, giới hạn hồi máu và pool shop vẫn tham gia vào build. Những kích hoạt cũ trùng chức năng đã được nhường cho mechanic mới để tránh nổ hai lần.

Rà soát description trước khi giao: cả **93 approach có đoạn giải thích riêng** trong mô tả hero và khung lựa chọn có cuộn. Ghi rõ hình đánh, trigger, hồi chiêu, damage budget, hồi máu và ngoại lệ theo từng lựa chọn; có hướng dẫn W là damage vũ khí đã chuẩn bị (tối thiểu 1 cho mechanic), AoE chia tổng damage theo mục tiêu. Xem toàn bộ nội dung xuất trực tiếp từ runtime ở [Hero_Description_Audit.md](../PreviewExports/HeroIdentity/Hero_Description_Audit.md).

## Chọn cách chơi

Trong shop, mở **BRIGADE → STATS → dòng Hero đầu tiên**. Bấm ba nút approach, phím **1–3**, hoặc **X trên tay cầm** để chọn. Thay đổi áp dụng vào trận kế tiếp; không đổi approach giữa trận.

**F / L3** gọi thao tác riêng nếu hero hỗ trợ. Glass Dice đổi mặt xúc xắc kế tiếp với giá 6 vàng, một lần mỗi wave; Penny Broker đặt đơn; Patchwork Host đổi bạn diễn chính; Lone Jelly đổi memory ở ô tương ứng approach đang chọn. Các thao tác này đều có nút trên giao diện.

## 31 cách chơi

| Hero | Mechanic thực tế | Ba approach |
| --- | --- | --- |
| Lantern Knight | Thu ánh sáng từ đòn cơ bản của bạn trong bán kính 4; đủ 8 thì tối đa bốn bạn cùng tiến công với tổng damage hữu hạn. Khi solo tự thắp đèn. | Advance / Fan out / Hold ground |
| Postmaster | Đóng dấu tối đa ba địch trong 6 giây; mỗi ba đòn gửi bưu kiện tới các địa chỉ đã đánh dấu. | Express / Fragile / Relay |
| Drowsy Seer | Di chuyển và đánh tích buồn ngủ; đủ 6 rồi đứng yên 1,5 giây để để lại giấc mơ đánh tại vị trí đó trong 4 giây. | Lucid / Wandering / Winter |
| Root Oracle | Ba rễ quanh thân, mỗi 3 giây công kích hoặc che chở. Có thể chặn đạn, cào thành đường hoặc đổi một phần sức đánh lấy hồi phục. | Shelter / Thorns / Harvest |
| Lone Jelly | Chọn tối đa ba memory làm cơ quan; mỗi ba đòn biểu hiện cơ quan tiếp theo theo đặc tính memory. Các nội tại đã hấp thụ vẫn tồn tại. | Bite / Glands / Threadwork |
| Glass Dice | Nhặt đủ 6 vàng để tung mặt đã báo trước, hồi 4 giây; mặt 1–6 có hiệu ứng khác nhau và chuyển theo chuỗi. Shop cho đổi mặt mở đầu. | Straight bet / Split bet / Around the table |
| Tide Singer | Ba lần nhặt xu pha một đợt thủy triều, hồi 4 giây; mỗi hai drink đã mua thêm một nốt, tối đa ba. Shore có hồi 3 HP. | Current / Shore / Breaker |
| Comet Runner | Đi 10 đơn vị thì đốt ba điểm gần nhất của lộ trình thành vệt đánh trễ. Hình của đường đi quyết định vùng đánh. | Trail / Twin tails / Closed orbit |
| Mirror Keeper | Đứng yên 1,5 giây để bắt tối đa ba đạn địch gần mình; đòn cơ bản kế tiếp hoặc hết 3 giây chờ sẽ trả đạn với damage có trần. | Aimed glass / Return to sender / Prism ring |
| Treasure Acorn | Chọn một mục tiêu truy thu; ba dấu đánh cơ bản hoặc hạ mục tiêu sẽ tất toán, trả 1 vàng tối đa tám lần/wave và một đợt đánh. | Local accounts / Collectors / Large accounts |
| Verdant Matron | Chọn nhóm vai trò ưu tiên cho bạn tiến hóa từ Sprout; mỗi 6 giây Sprout nở hiệu ứng bảo vệ, đánh hoặc dưỡng đội. Ghép vẫn giữ vũ khí nguyên liệu. | Guard nursery / Striker nursery / Guide nursery |
| Copper Cauldron | Địch ở gần và đòn đánh thông thường nhận vào tạo hơi; đủ 6 thì xả theo hướng. Chi phí HP chủ động không tạo hơi. | Lid / Boil / Whistle |
| China Marionette | Mỗi 6 HP thật sự được hồi tạo một mảnh sứ, tối đa ba; đánh tiêu một mảnh để tung đòn trễ. Hồi từ chính mảnh sứ không tạo vòng lặp. | Needlework / Broken mirror / Sheltering stitch |
| Soap Skater | Đổi hướng ít nhất 55° khi đang đi để để lại bong bóng, tối đa một/1,2 giây; ba lần đổi hướng gần nhau tạo mẫu lớn hơn. | Ribbon / Fountain / Bubble |
| Velvet Regent | Không tự đánh; sáu đòn cơ bản của đồng minh chuẩn bị mệnh lệnh. Tối đa ba bạn trả đòn theo vũ khí thật; lựa chọn đổi cả ưu tiên mục tiêu. | Spear / Volley / Court |
| Button Cavalier | Né hoặc để đạn sượt tạo poise. Mỗi viên chỉ tính một lần, sượt có hồi chung 0,8 giây; đủ ba thì đòn kế tiếp phản kích. | Thrust / Guard sweep / Salute |
| Night Maw | Ba mạng chuẩn bị cú cắn, hồi 2,5 giây. Hunger và hồi khi hạ địch vẫn chạy; không nuốt/execute boss tức thì. | Bite / Wide jaw / Second mouth |
| Thorn Treasurer | Đầu wave có một gai thế chấp mỗi 30 vàng giữ lại, tối đa bốn. Bị thương tiêu gai phản kích; mỗi sáu đòn mọc lại một gai. Không bí mật tiêu vàng. | Lances / Crown / Crossed receipt |
| Citrus Marshal | Dẫn đội hình đi theo hướng di chuyển; đi 8 đơn vị để lại đường nổ tiếp xúc. Ba lựa chọn đổi bố trí đoàn; tôn trọng thú cưỡi và follower đứng yên. | Escort / Procession / Close circle |
| Ivory Sovereign | Giữ 12 HP hoàng gia; bị thương hoặc mỗi ba đòn viết một sắc lệnh đánh trễ, hồi chung 1 giây. Chi phí HP không viết sắc lệnh. | Verdict / Court seal / Sentence |
| Petal Citadel | Relic khác nhau nuôi bốn cánh: công, thủ, hồi, đạn. Mỗi bốn đòn nở mẫu dựa cánh hiện có; đủ bốn nhóm thêm vòng ngoài. Bản trùng không thêm cấp cánh. | Bloom / Thorn lattice / Shelter |
| Patchwork Host | Chọn bạn diễn chính; bốn đòn cơ bản/tiếp xúc của bạn ấy cho hero diễn lại hình đánh theo vũ khí thật, hồi 3 giây. Bạn hỗ trợ thêm hồi nhẹ. | Featured act / Duet / Wide stage |
| Archive Curator | Ghi tối đa ba loài địch và phong cách đánh; mỗi 6 giây tái hiện đường lao, loạt đạn hoặc đòn gần. Chọn giữ mẫu hay tìm loài mới. | Current edition / Preserve / New specimens |
| Penny Broker | Đặt một món xác định cho shop kế tiếp, cọc 8 + wave vàng tối đa 40; giá được báo trước, đơn giữ qua reroll đến khi mua. Nhặt 10 vàng tạo dấu biên nhận khi đánh. | Curios / Drinks / Food |
| Storm Fencer | Nguy hiểm gần đó tích điện chậm; đạn sượt +1, dodge +2. Đủ ba tự rút kiếm có báo trước, hồi 3 giây. Không cần đánh cơ bản và không stun boss. | Quickdraw / Forked lightning / Crosscut |
| Tin Virtuoso | Đòn cơ bản tạo nhịp, crit tính hai. Câu nhạc đủ 2/4/6 nhịp bảo đảm 1/2/tối đa 4 bạn đáp bằng hình đánh và sức mạnh vũ khí riêng, chia tổng ngân sách damage. | Solo / Duet / Tutti |
| Moon Prowler | Đi 5 đơn vị nạp bóng; đánh từ phía sau dấu hướng cố định của mục tiêu tiêu bóng để thêm 45% damage. Đánh trước mặt giữ bóng; hạ bằng flank trả tối đa 6 vàng/wave. | Rear cut / Side crescent / Crossing shadow |
| Supper Mortar | Đòn cơ bản thành hai quả đạn rơi trễ thật; mỗi vụ nổ chia damage cho tối đa ba địch. Food tăng bán kính 0,025 mỗi món, tối đa 0,35. | Dumplings / Pepper / Sorbet |
| Lantern Weaver | Ghim tối đa ba địch sống; mỗi bốn đòn đóng mạng lưới theo liên kết. Ghim tồn tại 6 giây; địch chết bị bỏ khỏi mạng, không có chuỗi hit vô tận. | Closed weave / Pull stitch / Broad seam |
| Hourglass Stag | Hoãn 25% damage cơ bản cá nhân vào kho tối đa hai lần damage vũ khí; trả sau 4/3/5 giây với thưởng 10%. Damage trả không tự gửi lại vào kho. | Full payment / Early settlement / Winter instalments |
| Rain Archivist | Mở rương, hồi HP, dodge hoặc năm đòn viết trang sách. Ba trang gọi mưa băng; trang hồi/rương đổi hồi phục và độ rộng. Hồi của sách không viết trang mới. | Rain / Storm / Mist |

Các vệt mạng, dấu mục tiêu, quỹ đạo, vòng chắn và bóng diễn sử dụng hình học runtime cùng sprite hiện có. Đợt này không tạo thêm bộ sheet vẽ tay. Có thanh tài nguyên/trạng thái trên HUD và âm báo phù hợp từ thư viện hiện tại; hỗ trợ reduced motion/effects, không thêm flash trắng lên thân.

## Companion và item: quét đủ danh sách

Đã chuẩn hóa mô tả **66 loài companion** và **170 item** vào asset và các đường UI: shop, xem đội hình, owned relic, museum và chọn thưởng. Không bổ sung mechanic companion mới trong đợt này.

- Companion tách **talent**, **support khi sở hữu**, **passive định kỳ**, nguồn nhận và bond. Ghi đối tượng hưởng, điều kiện, cooldown, cap và cách cộng theo body/memory/sao. Support modifier dùng hệ số sao 1/2/4; passive định kỳ dùng hệ số riêng của catalog 1/3/9.
- Drink-reactive companion ghi rõ **mua DRINK** tăng damage của chính nó, +4%/drink tối đa +40%, áp dụng khi chuẩn bị trận tiếp theo. Roshi ghi food cho flat damage toàn đội kèm giảm max HP. Các buff Fruit/Clockwork/Runic/Quicksole/Lantern, giảm giáp và tăng tốc địch đều được nêu rõ.
- Item có nhãn **DRINK / FOOD / CURIO**, rarity và quy tắc stack. Drink/food tính ngay khi mua một lần, không cần bấm dùng; item vẫn nằm trong danh sách đồ đã sở hữu của run.
- Mô tả lấy **EffectiveModifiers**, ghi đúng scope, số mỗi bản và tổng số bản đang có. Proc/mechanic ghi trigger, con số, điều kiện và giới hạn. Preview mua item liệt kê phản ứng từ những hero/talent/relic đang sở hữu, không chỉ stat trực tiếp của món đó.
- Sửa các copy sai/thiếu: Key Oil +24% mỗi bản; Smiley Fox hoàn vàng tối đa 6/lượt; Judge/Matchstick tính cấp bond; Carbon Ribbon hút xu ở stamp burst và expose bằng bell; Patch Spring tự thêm một charge mỗi 3 giây cho arena toy đang nạp.

Toàn bộ từng entry được lưu trong [Description_Audit.md](../PreviewExports/HeroIdentity/Description_Audit.md). Bản một file phục vụ review cũng được cập nhật tại [Arcane_Brigade_Complete_Review.md](Arcane_Brigade_Complete_Review.md).

## Phản hồi thay đổi chỉ số — chỉ trong shop

Sau hành động thành công, so sánh chỉ số đã compile **trước và sau** giao dịch. Mua item/recruit, ghép tự động, altar, reroll, đổi approach, thưởng Rainbow và lựa chọn sự kiện trong shop đều đi qua snapshot thay đổi. Hero và từng companion bị tác động có panel gần vị trí của mình, hiện dấu **+ / −** cùng lượng thay đổi thực. Chỉ số không đổi vì chạm cap sẽ không báo tăng giả.

Theo dõi damage, tốc đánh, reach, speed, crit, số đạn/pierce/split/bounce, kích thước/tốc độ/quãng đường đạn, push, tiếp xúc, kích thước thân, sao, passive; thêm HP/giáp/dodge/hồi/pickup, chỉ số kinh tế và các thay đổi trạng thái build. Có thông báo gia nhập/ghép/rời roster và đổi approach/memory/bạn diễn. Hóa đơn giữ giá trị trước → sau để đọc lại.

Mỗi lượt hiện tối đa **8 đơn vị**, **hai dòng/đơn vị** trong **1,5 giây**, rồi chuyển sang các dòng/đơn vị còn lại. Cách xếp hàng này giữ toàn bộ chênh lệch mà không phủ kín màn hình khi đội đông. Panel dừng khi đang đọc bảng chi tiết hoặc đang chuyển cảnh để người chơi không bỏ lỡ.

**Chỉ vẽ khi session ở Shop và simulation ở Rest. Vào combat thì xóa hàng đợi ngay; trong trận không hiện popup thay đổi stat và không phát lại tồn đọng.** Đây là phản hồi stat của shop; HUD tài nguyên hero và các hiệu ứng chiến đấu vẫn phục vụ mechanic riêng trong trận. Giao dịch thất bại hoặc lưu thất bại không tạo phản hồi thành công giả.

## Kiểm tra và bản chạy

- [Verification.json](../PreviewExports/HeroIdentity/Verification.json): **2.330 kiểm tra đạt**, gồm 31 × 3 approach, mô tả đầy đủ cho cả 93 lựa chọn trong UI, kích hoạt thực trong simulation, save/restore, giới hạn queue, pause không farm, 66 companion × ba cấp, 170 giao dịch item, đúng counter drink/food, cả delta âm và tắt toàn bộ feedback trước combat.
- [Native_Checks.txt](../PreviewExports/HeroIdentity/Native/Native_Checks.txt) và [Reduced/Native_Checks.txt](../PreviewExports/HeroIdentity/Reduced/Native_Checks.txt): chạy Windows/D3D11, 1080p và 720p, thường và reduced motion/effects. Xem [popup shop](../PreviewExports/HeroIdentity/Native/04-shop-stat-gains-720.png), [chọn approach](../PreviewExports/HeroIdentity/Native/02-hero-approach-720.png), [mạng Weaver trong combat](../PreviewExports/HeroIdentity/Native/06-live-network-combat-720.png).
- [Build.txt](../PreviewExports/HeroIdentity/Build.txt): Windows build thành công, **0 lỗi / 0 cảnh báo**. Bản chạy: `Builds/HeroIdentity/ArcaneBrigade.exe`.
- [Delivery_Validation.json](../PreviewExports/HeroIdentity/Delivery_Validation.json): 12 khối JSON trong hồ sơ review hợp lệ, các liên kết tài liệu đầy đủ; đối chiếu source asset 66 companion và 170 item không đổi số liệu/quy tắc gameplay ngoài mô tả. Bỏ qua trường mô tả, export diễn xuất và chuẩn hóa metadata null/rỗng khi so sánh.

Các kiểm tra xác nhận cơ chế, dữ liệu và những trạng thái đã chụp. Cân bằng dài hạn giữa 93 approach vẫn cần các lượt chơi thực tế; không suy ra mọi lựa chọn mạnh ngang nhau từ kiểm tra tự động.
