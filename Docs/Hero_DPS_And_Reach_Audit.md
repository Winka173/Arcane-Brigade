# Tầm vũ khí, DPS hero và che khuất shopkeeper — 24/10/2026

Đã rà **117 vũ khí và 36 hero**, chỉnh phần tầm quá dài, cân bằng sức đánh theo vai trò và sửa shopkeeper che hero. Đây là cân bằng dựa trên mô phỏng chiến đấu thật của game trong Unity Editor Windows; chưa phải chứng nhận win rate chiến dịch hoặc nghiệm thu hình ảnh trên bản Windows player.

## Tầm đánh

- Tầm cận chiến vượt 2.2 được nén còn `2.2 + 25% phần vượt`, tối đa **2.6** đơn vị. Tầm xa vượt 5 được nén còn `5 + 35% phần vượt`, tối đa **6.5**. Vũ khí vốn ngắn vẫn giữ tầm; vũ khí dài vẫn có lợi thế. Sao, item và bonus hero đi qua bước biên dịch này; bonus companion áp dụng sau đó cũng bị chặn ở mức cuối.
- Đạn thường tiêu thụ **quãng đường di chuyển thực** qua `TravelLeft`; không chỉ dùng thời gian tồn tại. Đạn tăng tốc, nảy và các mảnh tách không tự nhận thêm tầm vô hạn. Quãng đường tính cả khoảng cách từ người bắn đến nòng; Glitch Toybox mượn vũ khí cũng phải tôn trọng tầm của người dùng.
- Đạn Arsenal trước đây dùng nhiều khoảng bay cố định hoặc `Range + ...`; nay chặng bắn đầu phải theo tầm đã biên dịch. Đường đâm/chém cận chiến không dùng chiều dài riêng vượt vũ khí.
- Các cơ chế quay về, dừng để nghiền, khoan/phao khi hết chuyến bay và nhảy sang mục tiêu kế tiếp vẫn có bước riêng. Vụ nổ, chuỗi lan và kỹ năng hero có vùng riêng; không coi toàn bộ chúng là một đòn đánh thường có cùng tầm. Đạn kỹ năng identity thông thường giới hạn quãng bay 6.5 từ nơi phát chiêu; rễ, hải đăng, bản sao và các vùng phép vẫn giữ hình học cơ chế riêng.
- Không đổi tầm đòn của phe địch trong hệ giới hạn này.

## Cân bằng hero

Hệ số power chỉ thay đổi ở những hero cần chỉnh sau đo; không sửa damage asset gốc hoặc quy tắc sao/rarity của companion.

| Hero | Power trước → sau | Lý do |
| --- | --- | --- |
| Root Oracle | 3.6 → 2.7 | Damage vùng và damage đơn đều cao; hạ đầu ra nhưng giữ bộ rễ/cây. |
| Patchwork Host | 5 → 4 | Giảm sức đánh riêng quá lớn ở hero đồng thời có bạn diễn. |
| Drowsy Seer | 4 → 3.6 | Hạ nhẹ sức mạnh khi di chuyển và các giấc mơ cùng đánh. |
| Treasure Acorn | 3 → 2.5 | Hạ phần damage vũ khí, giữ tiền thưởng và tương tác mục tiêu truy nã. |
| Rain Archivist | 3 → 2.7 | Hạ nhẹ damage, giữ giá trị hồi phục/thu thập trang. |
| Soap Skater | 3 → 3.6 | Tăng sức đánh riêng; đồng thời nới phạt damage cả đội từ **−80% còn −55%**. Mô tả đã cập nhật. |
| Storm Fencer | 3.3 → 3.6 | Tăng nhẹ lúc biến hình; không cho đánh thường ngoài trạng thái được phép. |
| Supper Mortar | 3.3 → 3.8 | Nâng sức đánh còn thấp sau đợt giảm Teapot Popgun. |
| Button Cavalier | 3 → 3.3 | Tăng nhẹ đòn riêng; giữ vai trò né/đấu tay đôi. |
| Lantern Weaver | 3 → 3.3 | Tăng nhẹ phần đánh trực tiếp, giữ cơ chế đường chỉ/khống chế. |
| Kite Admiral | 3.3 → 4.5 | Tăng phần damage riêng vốn thấp; giữ buff hộ tống và điều kiện đánh xa. |
| Bell Diver | 3.3 → 3.6 | Tăng nhẹ damage riêng, giữ probe và chống chịu. |

**Lone Jelly giữ power 6.5**: không có đội quân, nên phải đối chiếu với tổng sức đánh của đội khác. Velvet Regent/Citrus Marshal không được thêm đánh thường để làm đẹp bảng DPS. Archive Curator còn có giá trị từ mua hero và sưu tập; các lợi thế kinh tế không quy thành DPS giả.

**Tin Virtuoso/Billy:** trước đây companion trả lời crit bằng một đạn echo chung, mất kiểu đánh và lợi thế đánh vùng của vũ khí thật. Nay mỗi đồng minh sẵn sàng vẫn có **50% cơ hội** đáp lại một đòn crit của hero, nhưng thực hiện **đòn của vũ khí đang cầm, 180% damage**, có windup/cooldown riêng, hiệu ứng sao và projectile count của chính nó. Không có crit thì vẫn không tự đánh thường. Lệnh chờ bị xóa khi vào shop. Mô tả đã ghi rõ điều kiện và con số.

Phản kích Sassy Senior và sắc lệnh Card Queen cũng được sửa nguồn ghi nhận damage về đúng hero, thay vì mang nguồn đòn của kẻ vừa đánh họ. Đây là sửa attribution; không tự tăng damage hai cơ chế đó.

## Cách đo và bằng chứng

- **2,592 mẫu trước chỉnh power** và **2,592 mẫu sau**, cùng seed `77138`, 60 giây mỗi mẫu: 36 hero × 3 approach × wave 10/20 × 1/8 mục tiêu × solo/đội × đứng/di chuyển/áp sát có áp lực đạn.
- Thêm **2,592 mẫu sau với đầu tư chung**: bonus team damage +50%, crit +35 điểm phần trăm, attack speed +25%. Đây là bonus cô lập của fixture, không thêm vào save hay game thật. Giúp quan sát các kit phụ thuộc crit và kit có phạt damage.
- Đội chuẩn: Jelly Swordsman, Teapot Pilgrim, Brass Acorn, Candle Snail, đều 2 sao. Lone Jelly giữ solo và ba ký ức mẫu. Không thay đội theo kết quả để tăng thành tích một hero.
- Mục tiêu có 1,200 HP, kháng knockback, đứng tại vị trí xác định và được thay lại sau khi chết; không có đàn spawn ngẫu nhiên làm sai số giữa các lượt. Damage lấy từ ledger thực tế, không chỉ `Damage / Cooldown`. Bài áp sát có projectile thật để các cơ chế bắt đạn/phản ứng có cơ hội hoạt động; giữ hero sống phục vụ đo đầu ra, không dùng bài này kết luận khả năng sống sót.
- Tin Virtuoso được đo lại đủ 72 trường hợp ở mỗi cấu hình sau sửa đòn đáp trả; các dòng này thay đúng phần tương ứng trong hai bảng `After`. Không cộng thành số mẫu độc lập của toàn bộ 36 hero.
- [RangeOnly/Heroes.tsv](../PreviewExports/HeroDpsRange/RangeOnly/Heroes.tsv) là baseline đối chiếu. [After/Heroes.tsv](../PreviewExports/HeroDpsRange/After/Heroes.tsv) và [AfterInvested/Heroes.tsv](../PreviewExports/HeroDpsRange/AfterInvested/Heroes.tsv) là kết quả cuối. [Summary.tsv](../PreviewExports/HeroDpsRange/Summary.tsv) có đủ 36 hero; [Summary.json](../PreviewExports/HeroDpsRange/Summary.json) ghi phương pháp/phạm vi.
- [Before/Ranges.tsv](../PreviewExports/HeroDpsRange/Before/Ranges.tsv) và [After/Ranges.tsv](../PreviewExports/HeroDpsRange/After/Ranges.tsv): **1,161 cấu hình tầm** = 117 vũ khí × 3 sao × wave 1/10/20 + 36 hero × 3 wave. `flightDistance` trong bảng là speed × lifetime danh nghĩa; quãng bay thực của đạn thường còn bị giới hạn bởi `TravelLeft`.
- [Checks.txt](../PreviewExports/HeroDpsRange/Checks.txt): giới hạn tầm, quãng bay thực của đủ shot pattern, các bước quay về/khoan/phao, che khuất shopkeeper và điều kiện đáp trả crit. Lượt `Before/Heroes.tsv` là bài thăm dò cũ, **không dùng làm baseline so sánh** vì cấu hình đội và tình huống khác.

Giới hạn: một seed cố định; chưa bao trùm mọi item, mọi độ khó, cách chơi người thật hay mọi cơ chế mua hàng. Mục tiêu 1,200 HP làm các chiêu theo phần trăm HP có giá trị khác quái nhỏ thực tế. Không đồng nhất DPS giữa hero hỗ trợ, kinh tế, solo và đánh vùng; bảng giúp phát hiện độ lệch, không chứng minh mọi hero có cùng khả năng thắng.

Chạy lại bằng Unity batch mode: `ArcaneBrigade.Editor.HeroDpsRangeAudit.Final -audit-label After`. Có thể dùng `-dps-hero TinVirtuoso` để đo riêng hero khi chỉ đổi cơ chế đó. Kiểm tra hồi quy: `ArcaneBrigade.Editor.FriendlyReachChecks.Run`.

## Shopkeeper

`HeroFocus` trước đây bỏ qua toàn bộ prop trong shop. Nay cho phép **riêng thân shopkeeper** làm mờ khi che hero; quầy và hàng vẫn đọc rõ. Khi portrait chia thân thành nhiều mảnh diễn xuất, tất cả mảnh cùng giảm alpha, tránh đường ráp hai nửa. Hiệu ứng áp dụng sau khi actor cập nhật tint/sort, rồi khôi phục độ rõ khi không còn che khuất. Có kiểm tra cả portrait nguyên tấm và chia mảnh, hàng không bị fade và alpha phục hồi.

Sửa thêm cách tạo `MaterialPropertyBlock` của actor/portrait: khởi tạo lúc sử dụng, tránh lỗi constructor trong Unity Editor; sprite cắt được hủy đúng theo play/edit mode.

## Đề xuất hình ảnh tiếp theo

Đề xuất riêng cho cả 36 hero, mức tăng cường hiệu ứng cũ và thứ tự ưu tiên: [Hero_Effect_Suggestions.md](Hero_Effect_Suggestions.md). Phần đó là đề xuất, chưa tự động triển khai hiệu ứng mới.

## K?t qu? ?? 36 hero

DPS ri?ng l? trung b?nh c? ba approach, hai m?c wave, ba t?nh hu?ng v? hai c?u h?nh ??i. C?t ??i c? ??u t? l? t?ng damage c?a hero + b?n companion 2 sao; Lone Jelly v?n solo theo c? ch?. Kh?ng c?ng c?t bonus v?o t?ng damage l?n n?a.

| Hero | ??n tr??c ? sau | Nh?m tr??c ? sau | ??i c? ??u t?: ??n / nh?m |
| --- | ---: | ---: | ---: |
| LanternKnight | 47.2 ? 47.2 | 231.7 ? 231.7 | 998.4 / 4295.0 |
| Postmaster | 63.1 ? 63.1 | 113.9 ? 113.9 | 807.9 / 3101.9 |
| DrowsySeer | 69.1 ? 62.6 | 208.3 ? 189.1 | 701.7 / 2813.8 |
| RootOracle | 145.4 ? 111.5 | 636.2 ? 491.6 | 817.0 / 3434.4 |
| LoneJelly | 542.7 ? 542.7 | 815.7 ? 815.7 | 858.3 / 1503.3 |
| GlassDice | 46.1 ? 46.1 | 263.3 ? 263.3 | 726.1 / 3363.6 |
| TideSinger | 82.5 ? 82.5 | 142.0 ? 142.0 | 753.1 / 2874.5 |
| CometRunner | 48.0 ? 48.0 | 92.8 ? 92.8 | 661.2 / 2686.9 |
| MirrorKeeper | 44.6 ? 44.6 | 86.4 ? 86.4 | 678.3 / 3009.1 |
| TreasureAcorn | 128.0 ? 125.1 | 369.5 ? 367.3 | 781.1 / 3305.2 |
| VerdantMatron | 36.7 ? 36.7 | 95.9 ? 95.9 | 650.2 / 2750.9 |
| CopperCauldron | 68.9 ? 68.9 | 126.9 ? 126.9 | 719.7 / 2735.5 |
| ChinaMarionette | 47.2 ? 47.2 | 142.6 ? 142.6 | 687.0 / 2873.8 |
| SoapSkater | 9.5 ? 22.7 | 28.2 ? 55.7 | 441.3 / 1843.4 |
| VelvetRegent | 3.7 ? 3.7 | 23.3 ? 23.3 | 909.7 / 3902.5 |
| ButtonCavalier | 31.4 ? 33.7 | 100.1 ? 109.8 | 653.9 / 2780.5 |
| NightMaw | 87.5 ? 87.5 | 255.2 ? 255.2 | 806.3 / 3261.0 |
| ThornTreasurer | 50.2 ? 50.2 | 94.3 ? 94.3 | 523.1 / 2036.8 |
| CitrusMarshal | 12.0 ? 12.0 | 73.5 ? 73.5 | 393.6 / 1636.2 |
| IvorySovereign | 55.4 ? 55.4 | 127.2 ? 127.2 | 686.9 / 2848.0 |
| PetalCitadel | 54.1 ? 54.1 | 191.7 ? 191.7 | 815.4 / 3444.4 |
| PatchworkHost | 150.7 ? 122.1 | 211.3 ? 175.5 | 851.8 / 2946.5 |
| ArchiveCurator | 25.5 ? 25.5 | 87.0 ? 87.0 | 687.7 / 2891.7 |
| PennyBroker | 54.1 ? 54.1 | 233.5 ? 233.5 | 654.4 / 2872.6 |
| StormFencer | 29.6 ? 30.9 | 176.6 ? 180.9 | 655.0 / 3018.2 |
| TinVirtuoso | 84.5 ? 84.5 | 118.3 ? 115.9 | 439.9 / 1328.0 |
| MoonProwler | 40.5 ? 40.5 | 198.4 ? 198.4 | 672.7 / 3070.2 |
| SupperMortar | 37.9 ? 42.2 | 67.1 ? 71.1 | 649.4 / 2628.1 |
| LanternWeaver | 36.9 ? 39.4 | 73.4 ? 74.8 | 664.3 / 3188.6 |
| HourglassStag | 47.2 ? 47.2 | 210.2 ? 210.2 | 671.6 / 2919.5 |
| RainArchivist | 87.1 ? 79.3 | 120.3 ? 111.1 | 790.1 / 2862.1 |
| KiteAdmiral | 15.5 ? 20.5 | 70.9 ? 91.0 | 602.0 / 2831.8 |
| SundaeDuchess | 44.8 ? 44.8 | 187.5 ? 187.5 | 694.3 / 2902.0 |
| BonsaiClocksmith | 48.4 ? 48.4 | 111.4 ? 111.4 | 682.6 / 2843.8 |
| JugglingJester | 53.4 ? 53.4 | 169.4 ? 169.4 | 702.8 / 2954.4 |
| BellDiver | 35.9 ? 38.8 | 71.4 ? 75.8 | 689.0 / 2802.7 |
