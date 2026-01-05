# Arcane Brigade — cơ chế độc lạ và cảm giác thỏa mãn

**Ngày:** 2026-10-13. **Trạng thái:** chủ dự án đã duyệt hướng ý tưởng 2, 3, 4, 5, 6; đặc tả mở rộng bên dưới là **AUTHORED / DESIGN**, chưa implement runtime hoặc nghiệm thu balance/art/audio.

Đây là phần khác biệt nguyên bản của Arcane Brigade, không phải tính năng được xác nhận của Bizarre Brigade. Research 1.5+ vẫn là nền cho vòng chơi/shop/Details. Luật chủ dự án ưu tiên hơn mọi ví dụ trong tài liệu.

**Brainstorm bổ sung đã duyệt toàn bộ:** S6–S10 và N01–N15 trong `Nightmare_Identity_UI_Gameplay.md` mở rộng UI hồ sơ/build links/receipt/results, font ba vai trò, weapon quirks, connector relics, enemy/boss arena, idle/audio/material death và museum/nickname/cosmetic. Giữ S1–S5, không coi đây là runtime đã hoàn thành.

**Không đưa ý tưởng 1 vào scope:** chưa duyệt cơ chế đi thẳng/đổi hướng/đứng yên để điều khiển formation hoặc kích hoạt đợt đánh. Đồng minh vẫn tự đi quanh Champion trong bán kính; người chơi chỉ chọn vị trí, không nhập combo movement.

## 1. Bản sắc và các rào chắn

Thế giới giấc mơ biến những đồ vật đời thường thành đạo cụ ma thuật sống: nhìn đơn giản nhưng có tính cách, silhouette rõ, chuyển động hơi ngớ ngẩn và cơ chế chiến đấu dễ hiểu. Giữ thế giới/tên Arcane Brigade; không đổi project thành một game khác.

- Combat chỉ movement; không thêm nút đánh, aim, skill, dash, đổi vũ khí hoặc điều khiển formation.
- Đồng minh bất tử, không cánh tay/cẳng tay; bàn tay gắn vào vũ khí. Default không chặn đạn; chỉ item đã mô tả mới cho phép.
- Vũ khí roll ngẫu nhiên khi tuyển; không đổi sang shop chọn vũ khí. Không trùng unit + weapon; không cap đồng minh/upkeep/sell/lock.
- Mua và lựa chọn build ở rest bằng ba quầy vật lý. Không dừng combat để chọn thưởng hoặc bắt người chơi giải công thức giữa đàn quái.
- Không permanent stat grind, gacha trả tiền hay nâng cấp ngoài run bắt buộc.
- Map giới hạn rộng hơn viewport, camera theo người chơi; vật thể tương tác phải tạo cơ hội, không bắt đi giải puzzle mới được qua wave.
- Cảm giác mạnh đến từ phối hợp cơ chế và phản hồi chính xác, không chỉ tăng flash, rung màn hình hoặc tốc độ hiệu ứng.

Các con số nếu xuất hiện là tuning khởi điểm, không phải balance đã chốt. Không tăng số content để che core còn khó đọc hoặc chưa vui.

## 2. S1 — Vũ khí đồ vật với hành vi riêng

### 2.1. Catalog ý tưởng mở rộng

| Vũ khí | Cơ chế auto-combat | Cá tính animation / hình | Hướng synergy |
| --- | --- | --- | --- |
| Ô thủng | Đâm, bung tán tạo cú đẩy nhỏ; không mặc định chặn đạn | Tán lệch, bàn tay nằm ở cán; nén rồi bật | Melee, knockback lên quái, collision |
| Chuông mất tiếng | Tích nhịp khi đánh; đủ nhịp phát sóng | Chuông méo lắc mạnh nhưng sóng đến trễ một nhịp | Mark, AoE, echo |
| Máy đóng dấu | Đánh dấu mục tiêu; đủ dấu kích hoạt một burst | Dấu đỏ in trực tiếp, cán nảy sau cú đóng | Mark, crit, execute có điều kiện |
| Ấm trà sôi | Phun giọt; điểm đáp để lại vũng nóng tồn tại ngắn | Nắp rung, hơi thoát trước khi phun | Ember, zone, damage-over-time |
| Cung dây kéo | Tên đánh dấu đường nối giữa vài mục tiêu đã trúng | Dây kéo giãn rồi hồi, tay kéo dây tách khỏi thân | Pierce, tether, chain |
| Kéo cắt mây | Hai nhát khép/mở, cắt vùng hẹp theo hướng tự ngắm | Hai lưỡi đọc rõ, không xoay thành vòng sáng kín | Melee, marked targets |
| Máy ảnh ngái ngủ | Chụp một nhóm nhỏ, làm lộ điểm yếu trong thời gian ngắn | Ống kính thò ra, flash cục bộ, ảnh giấy bật xuống | Crit, debuff, support |
| Cá hộp lên dây cót | Thả cá lao vào quái rồi quay về chủ | Cá giãy, nắp hộp bật; đường hồi không gây hit ngoài luật | Returning projectile, bounce |
| Bàn ủi mộng du | Trượt ngắn, tạo vệt nóng; hồi về vị trí cầm | Trọng lượng nặng, tay giữ nguyên trên quai | Zone, knockback, Ember |
| Búa cao su có nấc | Gõ mục tiêu gần, nhịp cuối dội một vòng va chạm | Đầu búa kéo dài rồi co lại, impact lệch hài hước | Impact, shockwave |
| Đồng hồ báo thức lạc giờ | Đạn hẹn giờ phát nổ sau một fuse rõ | Kim quay và rung cục bộ trước khi nổ | Mortar/trap, delayed burst |
| Hộp nhạc thiếu răng | Phát nốt tự tìm mục tiêu; nốt cuối khuếch đại dấu | Tay quay trượt một nhịp, nốt có silhouette riêng | Echo, support, mark |

Đây là 12 concept, **không phải 12 asset/model đã tồn tại**. Có thể phát triển từ các weapon family đang có; không xóa tùy tiện bộ asset/vũ khí cũ hoặc đổi ID làm hỏng save.

### 2.2. Hợp đồng cho mỗi vũ khí

Mỗi weapon cần: nhịp anticipation → impact → recovery; target policy; phạm vi va chạm; flat/percent damage; element; projectile/fuse/zone rules; muzzle/hand anchors; projectile/impact/death interaction; SFX; mô tả shop/Details; hành vi theo tier.

- Một vũ khí có **một hành vi chính và tối đa một twist** trước khi thêm tier. Không nhét mark + chain + burn + hút tiền vào mọi món.
- Silhouette vẫn hiểu được ở 32–48 px. Khác biệt không dựa riêng vào màu, particle hoặc đọc một trang tooltip.
- Những vũ khí hữu ích với cùng một ally khác nhau phải có ưu/nhược điểm thật: burst/cadence/range/area/utility, không chỉ chênh DPS.
- UI hiển thị rõ vũ khí thật sau khi tuyển và nguồn RNG. Không quảng cáo một món cố định khi shop thực tế roll ngẫu nhiên.
- Các cơ chế đẩy/zone/tether chỉ tác động lên mục tiêu theo data; không thay đổi leash, miễn nhiễm hay blocking của đồng minh ngoài item cho phép.

### 2.3. Gói đầu tiên

Hoàn thiện **Ô thủng, Chuông mất tiếng, Máy đóng dấu** trước: một melee, một AoE nhịp và một single-target/mark. Chỉ mở rộng khi ba món này nhìn, nghe và chơi khác nhau thật. Cung/ma thuật hiện có vẫn nằm trong regression, không bị bỏ quên.

## 3. S2 — Build dây chuyền có chủ đích

Mục tiêu: người chơi mua một món và nhận ra nó đã nối hai phần của đội lại với nhau. Chuỗi phải hiểu được bằng hình/âm thanh/Details, không phải hiệu ứng ngẫu nhiên không rõ nguồn.

### 3.1. Sáu hướng build nguyên bản

| Build | Chuỗi mẫu | Điểm yếu / trade-off cần giữ |
| --- | --- | --- |
| Máy thu hoạch | Kill rơi coin → nhặt coin tích nhịp → chuông phát sóng → dấu trên quái tăng hiệu quả cú đánh kế tiếp | Cần ra lấy coin; ít hiệu quả khi boss chưa có add/loot; không sinh tiền từ tiền |
| Văn phòng hoàn trả | Đóng dấu → đủ dấu bật burst → máy ảnh làm lộ điểm yếu → đòn crit ăn lợi ích | Cần ramp trên mục tiêu; quái yếu chết sớm không cho cùng giá trị; chống proc crit tự nhân vô hạn |
| Tiệc trà ác mộng | Giọt trà đặt zone → quái đi qua bị burn → đòn búa lên mục tiêu burn tạo steam burst | Phụ thuộc vị trí, khó bắt mục tiêu di chuyển nhanh; zone không che đạn |
| Dàn hợp xướng lệch nhịp | Attack tích nốt → hộp nhạc tạo echo → chuông cộng hưởng với mục tiêu đã có dấu | Có khoảng nghỉ giữa burst; echo có damage scale và không tự tạo echo khác |
| Thợ săn kho báu | Mở rương → hồi máu/nhận buff trong run → cung hưởng lợi từ buff → thêm khả năng đi lấy rương kế tiếp | Cần Treasure Detection và thật sự đi lấy rương; không hồi vượt max hoặc xuất hiện lựa chọn thủ công trong combat |
| Xưởng đồ chơi | Auto-attack kích hoạt vật thể map → xe/búa quét nhóm quái → kill kích hoạt passive của đội | Mạnh quanh vật thể; yếu ở khoảng trống; không bắt map luôn có đúng một đạo cụ mới chơi được |

Mỗi build cần chạy ở mức cơ bản với **hai thành phần**, mạnh hơn khi đủ ba/bốn; không yêu cầu roll đủ bộ hiếm mới có tác dụng. Một item có thể nối hai build, nhưng mọi build không được kết thúc bằng cùng một loại nổ.

### 3.2. Item là lựa chọn, không chỉ buff dương

Các ví dụ AUTHORED cho hệ modifier/trigger, chưa chốt số:

- **Biên lai kéo dài:** pickup coin tích nhịp, giảm tốc hút coin. Đổi tiện lợi lấy burst.
- **Mực phản quang:** đánh dấu nhanh hơn nhưng dấu tồn tại ngắn; phù hợp đội cadence cao.
- **Khăn lót ấm:** mở rộng vũng trà, giảm damage mỗi tick; ưu tiên kiểm soát vùng.
- **Đĩa nhạc xước:** echo mạnh hơn nhưng tăng khoảng nghỉ giữa nhịp chính.
- **Túi có đáy giả:** tăng Treasure Detection, giảm bonus income ở rest; không giảm upkeep vì game không có upkeep.
- **Lò xo chắp vá:** đạo cụ map tích charge nhanh hơn, giảm bonus damage trực tiếp của brigade.
- **Giấy than:** một phần dấu truyền sang mục tiêu gần; không sao chép kill, coin hay toàn bộ projectile.
- **Phong bì rỗng:** collector pulse hút loot đủ điều kiện trong phạm vi, đổi lại giảm crit; không hút cả map mặc định.

Không làm một item bắt buộc cho tất cả nhân vật. Debuff phải ghi đúng scope và số trong tooltip; không gây surprise permanent stat loss ngoài run.

### 3.3. Quy tắc chống chuỗi vô hạn và lag

- Event mang root/cause, nguồn đơn vị, effect và target. Proc thứ cấp mặc định không kích hoạt lại chính nó.
- Mỗi effect công bố cooldown, stack/unique rule, target count và nhánh nào được phép kích hoạt tiếp. Đồ additive tăng giá trị không tự nhân số event vô hạn.
- Các chuỗi được duyệt có graph hữu hạn. Giới hạn độ sâu/nhánh là **luật gameplay rõ**, giống nhau trên mọi nền tảng, không phải âm thầm cắt damage khi máy yếu.
- Chỉ logical enemy death tạo loot một lần. Echo/mark burst và visuals không tạo death event thứ hai; coin pickup không sinh thêm coin từ chính coin đó.
- Không kéo proc từ phantom/projectile visual/death fragment. Overkill không nhân contribution hay lifesteal.
- Chỉ presentation được giảm số hạt/trail; không cap roster hoặc silently bỏ hit/projectile đã được luật gameplay cho phép.
- RNG drop/proc/shop/weapon tách nhau; save giữ counter/cooldown cần thiết ở boundary, không tự roll lại món vì mở Details.

### 3.4. Làm người chơi hiểu build đang hoạt động

Details hiển thị item theo role: nguồn → bộ nối → payoff; stat và tooltip vẫn dùng số thật. Bên cạnh từng item có giải thích trigger và cộng dồn.

Khi payoff xảy ra, một cue nhỏ đặc trưng xuất hiện gần nguồn. Không spam popup chữ mỗi proc. Cuối run có build recap: trigger thực tế, damage/heal/loot contribution và món nối quan trọng; không invent số đóng góp chưa đo.

## 4. S3 — Ghép vừa mạnh vừa có màn trình diễn

### 4.1. Gameplay và transaction

- Giữ ba copy cùng loại/cấp thành một cấp cao hơn, bond đếm body. Roster/weapon uniqueness vẫn phải hợp lệ sau merge.
- Weapon inheritance/randomness theo spec được chốt cho hệ hiện có; phần này **không tự quyết giữ vũ khí, chọn vũ khí hoặc đổi miễn phí**. Twist tier áp dụng lên weapon family thực của kết quả sau giao dịch.
- Tier mở **một biến thể hành vi có giới hạn** hoặc cải thiện utility, kèm tuning damage. Không nhân damage, số đạn, proc và AoE đồng thời ở mọi tier.
- Kết quả gameplay/save được commit đúng một lần trước reveal; animation không tiêu lại material hoặc roll lại weapon.
- Nếu save fail, rollback cả tiền/material/roster/weapon/RNG; không chạy reveal thành công rồi để build cũ tồn tại.
- Queue các merge liên tiếp tại rest; không biến một lần mua thành nhiều màn hình confirm. Skip/rút gọn presentation không đổi kết quả.

### 4.2. Biến hình có cá tính

| Family | Đổi silhouette | Twist tier mẫu |
| --- | --- | --- |
| Chuông | Vết nứt thành miệng cười, thêm chuông con | Một nhịp vọng yếu có delay rõ, không tự vọng tiếp |
| Đóng dấu | Cán lớn hơn, mặt dấu đổi hình | Dấu cuối lan sang một mục tiêu gần đủ điều kiện |
| Ấm trà | Nắp lệch, vòi dài, hơi thoát thành hình mặt | Giọt cuối đặt zone lớn hơn thay vì mọi giọt nhân kích thước |
| Ô | Tán thêm mảnh vá lớn, silhouette vẫn gọn | Cú bung cuối rộng hơn; không tự cấp interceptor |
| Cung | Đầu dây thành nút thắt nhận diện | Thêm một đoạn nối với danh sách target hữu hạn |

Không thêm chi tiết nhỏ dày đặc, arm bone hoặc hiệu ứng che hết nhân vật. Tier phải đọc được khi tắt particle, không chỉ bằng số sao.

### 4.3. Sequence presentation

Ba shadow/material dấu hiệu tụ lại → ngắt nhịp ngắn → silhouette kết quả mở ra → weapon/hand vào đúng pose → vòng merge bật ra → icon tier/bond cập nhật → âm thanh kết thúc riêng.

Thời lượng khởi điểm đề xuất: khoảng 0.6–1.0 giây tại rest; điều chỉnh bằng playtest, không phải timing reference. Giảm motion vẫn có silhouette/icon/audio confirmation. Đồng minh không bị kéo/teleport trong gameplay để phục vụ VFX; dùng visual ghosts nếu cần tụ hình.

Recipe đặc biệt ở altar có reveal riêng, preview material/tier/currency tại quầy. Không phải popup shop toàn màn hình và không tráo luật merge thường bằng crafting.

## 5. S4 — Arena có đồ chơi tương tác

Đạo cụ tạo lý do chọn vị trí trong sân, không thành skill thủ công hay puzzle chặn tiến trình. Đồng minh không bị đổi formation theo đường chạy của người chơi.

### 5.1. Sáu loại đạo cụ

| Đạo cụ | Kích hoạt / payoff | Giới hạn và đọc hình |
| --- | --- | --- |
| Chuông sân khấu | Nhận allied hit đủ charge rồi phát một sóng lên quái | Vòng charge và nhịp cảnh báo; cooldown trước lần tiếp theo |
| Bình mực | Vỡ vì allied attack spillover, đặt vùng chậm ngắn | Viền zone rõ, không hồi vô hạn để farm; không damage Champion |
| Xe đồ chơi | Allied impact đẩy xe theo hướng hit; quét quái dọc đường | Lane ngắn trong bound, không tự theo đuổi ngoài screen; kết thúc bằng slowdown |
| Búa treo lỏng | Nhận đủ charge, hạ xuống vùng báo trước | Warning hình mặt sàn; không nhắm Champion hoặc gây contact damage lên ally |
| Gương mất ngủ | Sao chép một projectile hợp lệ với hệ số giảm | Không sao chép bản sao/proc thứ cấp; mirror cooldown và charge hữu hạn |
| Hộp nhạc sàn | Cộng hưởng với attack gần đó, phát một pulse hỗ trợ ngắn | Pulse không hồi ally bất tử; scope/stat phải rõ, không tự tạo item miễn phí |

Wave đầu giới thiệu một loại dễ hiểu; số loại cùng lúc tăng có kiểm soát. Đây là catalog đề xuất; không mặc định spawn cả sáu loại trong mọi wave.

### 5.2. Tương tác và placement

- Dùng swept projectile/melee area hit và eligibility rõ. Ally không bỏ quái để tự chạy xa đánh prop; nhận charge chủ yếu qua hit spillover hoặc nearby auto-attack được data cho phép.
- Prop không là vũ khí cố định có thể dùng bằng Space trong combat. Space chỉ giữ chức năng interaction ở rest.
- Các đạo cụ của gói đầu chỉ hại quái/hỗ trợ player. Hazard hại player là feature khác, không tự thêm theo tài liệu này.
- Không spawn trên Champion, lối shop, sát wall gây kẹt, hoặc che loot/telegraph. Có vùng trống để né và build không dùng prop vẫn chơi được.
- Map seed + wave quyết định layout; screenshot/diagnostic ghi seed để reproduce. Camera chỉ thay vùng nhìn, không thay simulation bound.
- Prop ngoài camera không tự render full VFX; gameplay/timer vẫn theo luật. Không reset prop khi camera ra/vào hoặc mở Details.
- Enemy death/proc/prop có cause chain; một hit không charge một prop hai lần do collider trùng. Explosion không tạo vòng prop-trigger-prop vô hạn.
- Boss có resistance/tuning để một xe đồ chơi không xóa cả encounter. Giá trị prop không phụ thuộc có bao nhiêu pixel đang hiển thị.

### 5.3. Mở rộng arena có bản sắc

Ba motif tương lai dùng chung luật: **sân khấu thất lạc** (chuông/búa), **văn phòng chìm trong giấc ngủ** (mực/gương), **phòng đồ chơi bỏ quên** (xe/hộp nhạc). Mỗi motif đổi art/placement/presentation, không thêm biome mechanics trước khi vòng chơi hiện tại đạt chất lượng.

Rest giữ ba quầy và lối ra dễ nhận. Không đặt puzzle/prop combat lên quầy mua; không bắt người chơi đi hết map rộng để tiếp tục wave.

## 6. S5 — Cảm giác thỏa mãn theo từng nhịp

### 6.1. Chuỗi phản hồi chuẩn

**Attack preparation → impact → hit reaction → death → loot release → landing → attraction → collect → wallet/build response.** Gameplay và presentation có timeline liên quan nhưng không đồng nhất: hình chết vẫn diễn tiếp dù enemy đã mất hurtbox; animation collect không credit loot hai lần.

| Mốc | Yêu cầu presentation | Bảo vệ gameplay |
| --- | --- | --- |
| Đòn thường | Anticipation ngắn, impact rõ, trail gọn theo weapon | Không kéo cooldown vì render chậm |
| Crit | Số nổi bật, accent impact/audio riêng | Không đẩy toàn cảnh sáng hoặc camera shake mỗi crit |
| Enemy hit | White flash ngắn, squash/visual recoil có hướng | Không thêm hurt immunity hay thay vị trí collider ngoài rule |
| Enemy death | Collapse/silhouette break/fragments theo family; weapon/hand tan hợp lý | Hurtbox tắt ngay logical death; loot duy nhất |
| Elite death | Accent to hơn và reward readable | Không biến mọi quái thành elite explosion |
| Boss death | Khoảng nhấn rõ, body sequence và loot reveal trước results | Không thêm animation gây damage sau death trừ rule riêng đã mô tả |
| Coin drop | Scatter nảy có shadow, landing/spin/glint nhẹ | Điểm collect/logical loot tồn tại đúng, không phải fake coin |
| Coin pickup | Attraction/trail, sparkle ở người chơi, wallet pulse gộp nhóm | Credit tại mốc logical collect một lần; pause không collect |
| Chest/heart/egg | Cue riêng, icon/shape phân biệt với đạn | Không che source damage hoặc bật menu chọn giữa combat |
| Purchase/merge | Chime, arrival/reveal, update Details/bond | Transaction đúng trước confirmation |

### 6.2. Độ mượt là continuity, không chỉ FPS

- Pose theo phase có đường liên tục qua impact/recovery; không nhảy góc weapon từ âm sang dương trong một frame.
- Render-time interpolation cho unit/projectile/loot và pose; không thay simulation tick để che stutter.
- Particle dùng easing, cadence và lifetime có chủ đích. Slash, magic, arrow và heavy impact không dùng cùng một ring phóng to.
- VFX hit bám đúng vị trí contact, muzzle đúng bàn tay/weapon; không dùng pivot body cho mọi đòn.
- Death/loot effects không bị Clear ngay ở tick wave kết thúc; transition giữ cue quan trọng rồi mới dọn an toàn.
- Camera follow mềm nhưng không kéo lê khiến né đạn khó; shake nhỏ tùy chọn, không mặc định shake theo mọi hit.
- Không global slow-motion/hitstop làm wave timer, spawn, proc hoặc save khác nhau. Ưu tiên local presentation accent; mọi time-scaling thật phải có spec riêng.
- Capture realtime 60fps và đo frame-time; clip 12fps hiện tại không là bằng chứng nghiệm thu độ mượt.

### 6.3. Readability và audio

Ưu tiên: Champion và đạn nguy hiểm → boss/elite telegraph → loot quan trọng → impact payoff → trails/sparks trang trí. Layer/budget được test khi nhiều đơn vị cùng đánh.

Cho phép giảm numbers/flashes/shake/trails mà giữ thông tin và gameplay. Group wallet pulses và coin sounds theo cụm nhỏ; không phát hàng trăm note cùng lúc. Mỗi family có texture SFX riêng, crit/death/merge có accent nhưng không tranh transient với dấu hiệu nguy hiểm. Music ducking khi có boss/transition không mute toàn bộ âm cảnh báo.

Nhạc và audio audition vẫn chưa hoàn tất. Thiết kế cue không có nghĩa đã compose/licence/mix soundtrack production.

## 7. Hệ UI nối năm cơ chế

- Quầy shop: icon, rarity, giá/currency, modifier lợi/hại, scope, role synergy. Companion không show một vũ khí chắc chắn trước khi roll.
- Details: Stats tổng hợp đúng, Companions có weapon/tier/bond, Relics có stack/trigger/unique rules. Không thêm nút bán/đổi đồ.
- Merge/altar: preview vật liệu và kết quả hợp lệ; notification gọn ở rest, không inventory minigame thủ công.
- HUD: tín hiệu proc quan trọng chọn lọc, không dán toàn bộ trigger graph lên arena.
- Results: recap đội/vũ khí/đồ, contribution và chuỗi payoff; records không tự tăng permanent stat.
- Help/codex: ví dụ hai thành phần đầu của mỗi build, mở dần thông tin theo discovery; player vẫn có quyền thử ngoài build mẫu.

## 8. Phạm vi gói signature đầu tiên

Sau nền R1/R2, làm một lát cắt hoàn chỉnh trước khi nhân rộng:

1. Ba weapon families: Ô / Chuông / Đóng dấu, có target/pose/VFX/SFX/UI và tier behavior riêng.
2. Hai chuỗi: Máy thu hoạch và Văn phòng hoàn trả; các modifier/proc nguyên bản tạo đủ source → connector → payoff.
3. Merge thường có visual reveal, weapon/tier thật và transaction an toàn.
4. Hai prop: Chuông sân khấu và Xe đồ chơi, bố trí theo seed, không cần manual skill.
5. Enemy/player/boss death và coin drop/collect đủ pipeline, không chỉ puff/số vàng.
6. Details/recap giúp người chơi nhận ra vì sao chuỗi hoạt động.

Đây là **lát cắt khởi đầu, không phải giảm scope năm hướng đã duyệt**. Các weapon/build/prop còn lại mở rộng sau khi lát cắt đạt tiêu chí, không gắn deadline hoặc tuyên bố production-ready từ lượng content.

## 9. Nghiệm thu bắt buộc

| Nhóm | Kiểm tra |
| --- | --- |
| Controls | Trong combat Space/click/gesture không kích skill/formation; UI không rò input; chỉ movement |
| Weapon identity | Ba weapon khác nhau khi tắt VFX và khi ở cỡ nhỏ; regression melee/bow/magic đủ bốn hướng |
| Proc correctness | Một root không recurse vô hạn; event attribution đúng, secondary rules rõ, pause không chạy cooldown/proc |
| Economy | Một death/collect chỉ credit một lần; chain không farm coin từ coin, props không in loot; transactions rollback đúng |
| Merge | Ba copy đúng luật, uniqueness/bond đúng, skip/reload không đổi kết quả; animation không kéo logical ally khỏi leash |
| Props | Không charge kép vì collider; gương không copy bản sao; xe/búa không gây damage Champion; camera re-entry không reset |
| Combat clarity | Champion/projectile danger vẫn đọc được giữa burst; options giảm VFX không đổi damage, roster hoặc RNG |
| Death/loot | Wave-clear không cắt cue; hurtbox chết tắt ngay; coin logical và animation không lệch credit; boss results đúng thứ tự |
| Build variety | Mỗi chuỗi có tác dụng với hai thành phần, không cần full rare set; boss/empty-map weakness đúng, không một build luôn thắng |
| Performance | Đội đông/Endless, allocation/frame-time thực, clip realtime và thiết bị mobile; không lấy số test simulation thay art/audio approval |

Playtest phải ghi build/seed/setup, cảm nhận và observed behavior. Người chơi cần kể được “món nào nối build” và nhìn ra tier/vũ khí nào đang tạo payoff; nếu không, tăng UI/readability trước khi tăng damage hay số particle.
