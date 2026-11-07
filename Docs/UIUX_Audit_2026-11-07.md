# Rà soát UI, UX và luồng thao tác — 07/11/2026

> Cập nhật: F01–F08 đã được xử lý trong lượt tiếp theo. Xem [báo cáo sửa lỗi và hiệu năng](UIUX_Performance_2026-11-07.md) cùng bằng chứng mới. Phần bên dưới giữ nguyên kết quả tại thời điểm audit trước khi sửa.

Kết quả: **chưa đạt để coi là UI/UX hoàn thiện**. Có lỗi dịch làm sai tên chức năng, số đếm chưa phản ánh dữ liệu, nhóm điều khiển thiếu hover và vấn đề với focus/tooltip. Bản sửa trong lượt này **chỉ vẽ lại nút sách** theo yêu cầu; các lỗi dưới đây được báo để xử lý tiếp, không tự thay đổi gameplay hay các luồng khác.

## Cách kiểm tra

- Áp dụng hai skill đã cài: [frontend-ui-engineering của Addy Osmani](../.claude/skills/frontend-ui-engineering/SKILL.md) và [design-taste-frontend](../.claude/skills/design-taste-frontend/SKILL.md), cùng tiêu chí app-screen và critique của skill thứ hai.
- Giữ ngôn ngữ hình ảnh fantasy vẽ tay của dự án. Các luật riêng cho website/dashboard như bỏ khung trang trí, bỏ serif, giới hạn màu chrome hay dùng sidebar không áp dụng máy móc vào game này.
- Game dùng Unity IMGUI, không có HTML/DOM. Không chạy và không tuyên bố đã qua Chrome inspector, axe, CSS lint hay kiểm tra screen reader của web.
- Kết quả hover được báo trong tài liệu này lấy từ **con trỏ GUI mô phỏng bên trong bản test**, không di chuyển chuột, gửi phím, click hệ thống hay giành focus. Lượt thử điều khiển bên ngoài trước yêu cầu dừng không được dùng làm bằng chứng hover.
- Ghi lại vùng tương tác thật từ `TouchTargets`, label cuối cùng từ `LocalizedGUI`, render bình thường và render khi hover từng vùng. So sánh pixel trong vùng, đối chiếu trạng thái selected/disabled, mã nguồn và ảnh. Pixel không đổi là dấu hiệu để xem xét, không tự động được tính là một lỗi.
- Bộ test dùng profile tách biệt trong `ArcaneBrigadeDiagnostics`. Các ảnh menu/Phòng Chờ dùng dữ liệu fixture được ghi rõ; không coi việc gọi trực tiếp một trang là bằng chứng mọi đường click tới trang đó đều đúng.
- Hook ghi nhận chỉ tồn tại khi build bản audit và được phục hồi byte-for-byte. Mã production thay đổi trong lượt này chỉ có phần vẽ nút sách ở `RunHud` và asset mới.

## Phạm vi đã xem

| Nhóm | Trạng thái/điều khiển đã rà |
| --- | --- |
| Menu | English / Tiếng Việt; hành trình mới, Phòng Chờ, bộ sưu tập, kỷ lục, cài đặt, hướng dẫn, thoát |
| Settings | Đầu trang và ba vị trí cuộn; ngôn ngữ, lựa chọn màn hình, âm thanh, slider, nút −/+, lựa chọn trải nghiệm/lối chơi; kích hoạt đổi sang tiếng Việt bằng callback GUI mô phỏng |
| Chọn nhân vật | Thẻ mở/khóa, đổi nhân vật, độ khó, giao ước, nút bắt đầu |
| Trang thông tin | Hướng dẫn, bộ sưu tập, thành tựu, kỷ lục, thử vũ khí; trạng thái rỗng và đã khám phá |
| Phòng Chờ | Cây Đèn và node, bưu điện, tủ đồ, bảo tàng. Ledger/Clock đã gộp vào nhánh Cây Đèn theo mã hiện tại; không tính chúng là hai màn độc lập đã hoàn thành |
| Trong lượt | Cửa hàng có hàng thật trong fixture, mô tả rút gọn/chi tiết, trường hợp không đủ vàng, pause, các trang con của pause, bốn tab Lữ đoàn |
| Luồng thoát | Hộp xác nhận giữ chơi / rời không lưu / lưu và trở lại; xem thêm render 960×540 và 1920×1080 |
| HUD điện thoại | Render bằng `-arcane-phone`, kiểm tra hình nút sách và gọi action mở Lữ đoàn. Chưa thay thế việc test trên Android/iOS thật |

Đã ghi **49 trạng thái**, **629 mẫu hover**; **596 mẫu khớp vùng đích**. 33 mẫu còn lại không được dùng làm bằng chứng một control nhận đúng hover. Có 300 vùng enabled không đổi pixel trong các mẫu khớp, nhưng con số này bao gồm trạng thái đã chọn và được xem xét theo từng nhóm, không phải 300 lỗi. Chi tiết trong [UIUXAuditMetrics.json](Verification/UIUXAuditMetrics.json). Không có lỗi runtime được ghi trong lượt audit cuối. Số frame chậm trong log khi chụp ảnh không dùng làm benchmark hiệu năng game.

Ảnh và JSON gốc nằm trong `PreviewExports/UIUXAuditSynthetic/`; ảnh nút sách trong `PreviewExports/BrigadeBook/`. PNG preview được giữ local theo `.gitignore`.

## Các vấn đề

### F01 — P1: bản dịch làm sai nghĩa tên chức năng

**Đã thấy trên bản render.** Tab `RELICS` thành **TIN TỨC**, `BONDS` thành **TRÁI PHIẾU**. Đây là tên chức năng chính, dễ khiến người chơi chọn sai trang hoặc hiểu sai hệ thống. Nên dùng **DI VẬT** và **LIÊN KẾT**, đồng bộ cả các biến thể chữ hoa/thường và tên gọi trong mô tả.

Bằng chứng: [bảng Lữ đoàn lúc mở](../PreviewExports/BrigadeBook/02-book-opens-brigade.png), [tab di vật](../PreviewExports/BrigadeBook/03-relics-tab.png); bảng dịch `Resources/Localization/vi.json` tại các khóa `RELICS`, `BONDS`; `RunDetails.cs:266`.

### F02 — P2: số đếm các tab bằng 0 khi mới mở dù đang có đồ

**Đã tái hiện bằng render và đối chiếu dữ liệu fixture.** Fixture có 1 đồng hành và 8 loại di vật. Lúc mở tab Thống kê, badge các tab khác hiện 0; khi chuyển tab, chúng thành 1 và 8. Người chơi có thể tưởng đội hình hoặc hiệu ứng đồ đã bị mất.

Nguyên nhân: lần mở đầu chỉ gọi `Capture(..., statsOnly: true)`, còn badge đọc `model.Companions.Count`, `model.Relics.Count`, `model.Links.Count` từ model chưa nạp. Nên tính badge nhẹ từ dữ liệu run hoặc hiện trạng thái chưa nạp, không dùng 0 giả.

Bằng chứng: hai ảnh ở F01; `RunDetails.cs:34`, `RunDetails.cs:267`, `RunBuildReadModel.cs:49`.

### F03 — P2: hover không nhất quán trên các phần tử đang bấm được

**Đã xác nhận bằng mô phỏng GUI và ảnh pixel.** Ở Settings đầu trang, 14/14 vùng nút nhận đúng vị trí hover mô phỏng; 13 vùng giữ nguyên pixel, trong khi Back thay đổi rõ. Trong 13 vùng đó có lựa chọn đã chọn/không đổi giá trị, nhưng các lựa chọn chưa chọn và nút âm lượng còn tác dụng cũng không có hover. `Choice` luôn vẽ `Normal`/`Chosen`; `Step` luôn vẽ một sprite cố định.

Thẻ nhân vật và một số vùng icon/chọn trang cũng dùng nút rỗng với `GUIStyle.none`; hover của nền nút không tự xuất hiện. Trong 19 vùng roster English, 12 vùng đang enabled không đổi pixel khi hover; các trường hợp có animation được đối chiếu thêm với mã, không suy diễn từ delta.

Đề xuất: dùng cùng một hợp đồng normal/hover/pressed/selected/focus/disabled cho cả nút có hình riêng; vùng icon có tác dụng cần feedback rõ. Không yêu cầu hiệu ứng hover trên điện thoại không có con trỏ.

Bằng chứng: `en-settings-fresh-baseline/hover-001/hover-003`, `vi-settings-fresh-*`, `en-roster-fresh-*`; [metrics](Verification/UIUXAuditMetrics.json); `SettingsSheet.cs:172`, `SettingsSheet.cs:249`, `ArcadeNavigation.cs:63`.

### F04 — P2: focus điều hướng có thể đi ra ngoài vùng Settings đang thấy

**Đã xác nhận qua trạng thái navigation mô phỏng và render.** Settings đăng ký cả 49 điều khiển, kể cả phần đang nằm ngoài vùng cuộn. Sau khi gọi đúng phương thức `Move` dùng cho điều hướng, selected ở index 33 nhưng viewport vẫn ở đầu trang, không có focus đang nhìn thấy.

Nên đưa vùng cuộn theo control được focus hoặc chỉ điều hướng theo các control mà hệ thống có thể đưa vào view. Cần áp dụng cả bàn phím và tay cầm; test này gọi trực tiếp phương thức điều hướng, không phát phím thật.

Bằng chứng: `vi-settings-navigation-offscreen.json/.png`; `ArcadeNavigation.cs:33`, `ArcadeNavigation.cs:61`, `SettingsSheet.cs:33`.

### F05 — P2: chữ rút gọn có tooltip trong dữ liệu nhưng không có renderer tooltip

**Đã thấy label rút gọn trong render; xác nhận thiếu phần hiển thị bằng mã.** Ví dụ bộ lọc thành tựu bị rút thành “Chỉ hiển thị phần chưa hoàn thà…”, tiêu đề thư cũng có dấu “…”. `FitContent` lưu tên đầy đủ trong `GUIContent.tooltip`, nhưng toàn bộ runtime không có chỗ đọc/vẽ `GUI.tooltip`. IMGUI không tự vẽ hộp tooltip chỉ vì gán trường đó.

Đề xuất: vẽ tooltip thật khi có con trỏ, và có cách đọc tên đầy đủ bằng chạm trên điện thoại; với nhãn chức năng ngắn, ưu tiên biên tập lại câu hoặc tăng khoảng trống.

Bằng chứng: `vi-achievements-full-name-hover.png`, `vi-hub-postbox-baseline.*`, `LocalizedGUI.cs:40`; tìm `GUI.tooltip` trong runtime không có kết quả.

### F06 — P2: thuật ngữ trong đoạn văn tiếng Việt mất liên kết giải thích

**Đã đối chiếu contract lookup bằng render và mã.** `GlossaryUI` có khóa English như `Damage`; `Find("Sát thương")` không có entry. Trong layout, đoạn văn được dịch trước khi tách token và gọi `Find(word)`, nên từ đã dịch thường không còn nối với entry gốc. Các chip `{bonus:...}` dùng ID riêng vẫn có thể hoạt động; lỗi không đồng nghĩa mọi tooltip đều hỏng.

Đề xuất: giữ ID thuật ngữ ổn định khi dịch nội dung, hoặc tạo alias ngôn ngữ có kiểm soát; không dùng nhãn hiển thị làm khóa logic.

Bằng chứng: `en-glossary-known-key.*` so với `vi-glossary-translated-key.*`; `GlossaryUI.cs:35`, `GlossaryUI.cs:197`, `GlossaryUI.cs:260`.

### F07 — P2: chưa bảo vệ đầy đủ token trình bày trước khi dịch

**Xác nhận ở mã và bảng dịch; chưa render mọi loại thẻ trao đổi/khách ghé.** Bộ bảo vệ hiện chỉ có `{mark}`, `{stat:...}`, `{bonus:...}`. Bảng dịch lại chứa `{gives}` → `{cho}`, `{costs}` → `{chi phí}`, trong khi parser chỉ nhận token English gốc. Các đoạn dùng những token này có thể mất nhãn lợi ích/chi phí hoặc hiện token như chữ thường.

Đề xuất: định nghĩa và bảo vệ toàn bộ grammar token của glossary, dịch riêng payload có thể hiển thị. Thêm kiểm tra đủ các loại token, không chỉ mark/bonus.

Bằng chứng: `GameLocalization.cs:26`; regex token trong `GlossaryUI.cs:13`; các khóa `{gives}`, `{costs}` trong `vi.json`. Không dùng ảnh shop thường để tuyên bố đã tái hiện mọi biến thể thẻ này.

### F08 — P2: nội dung tiếng Việt vẫn lẫn English và câu dịch khó hiểu

**Đã thấy trên render.** Chi tiết cửa hàng còn `While you have it (×2)`; lựa chọn cách đánh còn `Advance`, `Fan out`, `Hold ground`; một số mô tả dùng “mỗi lần truy cập” để nói về đòn đánh. Phần thuộc tính được dùng để ra quyết định mua đồ cần rõ và nhất quán hơn lời thoại văn học.

Đề xuất: ưu tiên biên tập nhãn chức năng, chỉ số, câu so sánh trước/sau và cách đánh; chuẩn hóa chuỗi động thành mẫu dịch thay vì ghép nhiều fragment rồi dịch ở biên render.

Bằng chứng: `vi-shop-item-detail-final-baseline.png`, `02-book-opens-brigade.png`; `HeroIdentityOptions.cs:32`, `HeroIdentityOptions.cs:44` và dữ liệu tên approach.

## Điểm đã đạt và giới hạn

- Header Settings hiện gọn, không đè Back ở các render đã xem. Các hộp xác nhận ở 960×540 và 1920×1080 nằm trong khung; câu giải thích nêu rõ tác dụng lưu/rời.
- Menu chính có phản hồi hover trong các mẫu hợp lệ. Không kết luận các nút đó hỏng chỉ từ việc chúng có cùng texture trong một số style.
- Đổi ngôn ngữ qua activation GUI mô phỏng áp dụng ngay. Bộ kiểm tra hiện có 35 check đạt, build Windows thành công, 0 lỗi biên dịch. Các warning hiện có được ghi trong báo cáo build.
- Không có kết luận toàn bộ game đã qua test: chưa test trên thiết bị cảm ứng/tay cầm thật, không phát chuột/phím OS, chưa chạy mọi nhân vật/vật phẩm/biến thể khách ghé và chưa xác minh mọi tình huống lỗi lưu/mua hàng.
- Scrollbar hiện chủ yếu là chỉ báo, còn kéo nội dung dùng logic drag của panel. Hành vi click track/kéo thumb theo chuẩn desktop nên được xem thêm, nhưng không được ghi là lỗi runtime đã tái hiện trong lượt này.

## Thay đổi đã làm

Đã thay riêng hình quyển sách thô bằng journal vẽ tay, giữ khung đồng và hành động mở bảng Lữ đoàn. Chi tiết nguồn ảnh và cách nhập asset: [BrigadeBookArtwork.md](BrigadeBookArtwork.md). Bản chạy mới: `Builds/Localization/ArcaneBrigade.exe`.

Thứ tự sửa đề xuất: F01 → F02/F07 → F03/F04/F05/F06 → F08. Không tự sửa các mục audit trong lượt này.
