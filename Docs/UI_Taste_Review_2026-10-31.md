# Rà soát UI theo taste-skill và frontend-ui-engineering — 31/10/2026

Tài liệu rà soát 66 màn hình. Ảnh lấy từ bản build mới nhất, gồm lượt chụp playtest và lượt chụp menu.

Màn hình được chấm theo hai bộ tiêu chí:

- **taste-skill (design-taste-frontend):**
  - 14 bước nhìn:
    - một giây rồi năm giây;
    - nheo mắt;
    - che logo;
    - mép chữ;
    - khoảng trống;
    - cỡ chữ;
    - màu;
    - hình;
    - độ lặp;
    - điện thoại;
    - độ đầy;
    - nội dung;
    - va chạm;
    - đối thủ và "thứ sống".
  - Điểm theo trọng số Awwwards: thiết kế 40, dễ dùng 30, sáng tạo 20, nội dung 10.
- **frontend-ui-engineering (addyosmani):**
  - trạng thái đang tải, trống, lỗi;
  - vùng bấm;
  - màu không phải tín hiệu duy nhất;
  - luồng đi lại giữa các màn.

Một số quy tắc của taste-skill được điều chỉnh cho game. Skill này viết cho trang web và cấm trang trí, như viền, gradient, khung và bóng đổ. Arcane Brigade lại lấy art vẽ tay làm phong cách, nên khung và viền vẽ tay được giữ. Các bước nhìn và cách chấm điểm vẫn áp dụng nguyên vẹn.

## Kết luận nhanh

| Màn | Kết luận | Điểm (TK/DD/ST/ND → tổng) |
| --- | --- | --- |
| Loading | **Làm lại** | 6 / 6 / 4 / 5 → 5.6 |
| Main menu | **Làm lại** | 6.5 / 7 / 5 / 6 → 6.3 |
| Hero select | Giữ, sửa nhỏ | 7.5 / 7.5 / 6.5 / 7 → 7.3 |
| How to Play | Sửa | 6 / 7 / 6 / 6 → 6.3 |
| Settings | Giữ | 7 / 8 / 6 / 7 → 7.1 |
| Collection | Giữ, sửa nhỏ | 7.5 / 7.5 / 7 / 7 → 7.4 |
| Waiting Room (hub) | Giữ | 8 / 7.5 / 8 / 7 → 7.8 |
| Lamp Tree | **Sửa** | 6.5 / 6 / 7 / 6 → 6.4 |
| Postbox, Bell, Window, Night Pact, Wardrobe, Museum | Giữ | khoảng 7.5 |
| Shop | **Sửa** | 7 / 6.5 / 8 / 7 → 7.1 |
| Thẻ mô tả món đồ và chi tiết | Sửa nhỏ | 7 / 7 / 6 / 6.5 → 6.8 |
| HUD trận đấu, boss, bản đồ | Giữ | 8 / 7.5 / 8 / 7 → 7.7 |
| Thẻ khách ghé, phòng bí mật, quà Keeper | Giữ | khoảng 7.5 |
| Combos | Sửa nhỏ | 6.5 / 7 / 6 / 5 → 6.4 |
| Tổng kết run | **Sửa** | 6.5 / 6.5 / 6 / 7.5 → 6.5 |
| Ending | Giữ | 8 / 8 / 8 / 8 → 8.0 |
| Bến đò, Khách Cuối | Giữ | khoảng 7.3 |

## Từng màn

### Loading — làm lại

- **Nhìn một giây:** chỉ thấy "một sảnh". Bức tranh sảnh đèn lồng đặt vào game nào cũng được, nên trượt bước "che logo".
- **Tiến độ:** chín hình thoi vẽ bằng code, không gắn với thế giới trong game.
- **Bố cục:** tên game nằm giữa, khác vị trí tên ở menu chính, nên chuyển từ loading sang menu bị giật.
- **Ý tưởng:** tám đèn lồng của mạch truyện (đèn trên HUD) biến thành thanh tiến độ. Màn hình là con hẻm đêm dẫn tới cửa Waiting Room. Tám đèn thật trong game sáng lần lượt dọc lối đi. Tên game nằm cùng vị trí với menu chính.

### Main menu — làm lại

- **Nhìn một giây:** "một nhân vật xanh đứng một mình". Lantern Knight đứng yên giữa khoảng trống lớn, đơn điệu.
- **Thiếu "thứ sống":** lần nào mở game cũng thấy y hệt.
- **Hai nền khác nhau:** sau Act I là phòng có chiếc ghế quay. Việc này có chủ ý, nên giữ.
- **Ý tưởng:** đội của đêm gần nhất đứng chờ ở cửa:
  - anh hùng vừa chơi đứng trước;
  - các bạn trong đội của lần chơi trước đứng quanh, mỗi bạn có hoạt ảnh riêng;
  - lần chơi đầu tiên dùng một đội mặc định.
  - Mỗi lần mở game thấy đúng đội của mình. Đây là "thứ sống" của màn.
- **Nền:** vẽ lại cho bố cục này. Một phần ba bên trái tối và yên để đặt nút. Bên phải là cửa và một khoảng sàn trống đủ cho sáu, bảy nhân vật.
- **Nút:** giữ một nút vàng duy nhất. Các nút phụ đã đạt chuẩn.

### Hero select — giữ, sửa nhỏ

- Dòng "Starts with" ở cuối ô mô tả bị cắt (Item01).
- Nhãn "Recruit themes" và ba icon nhỏ trông như một nhãn nhỏ nằm trên tiêu đề, loại mà taste-skill cấm. Nên gộp vào ô mô tả.

### How to Play — sửa

- Nửa phải của khung gần như trống: ba gạch đầu dòng chữ nhỏ, ảnh minh họa chỉ chiếm 40%. Màn này trượt bước "độ đầy".
- **Sửa:** ảnh minh họa chiếm khoảng 60% chiều rộng, chữ lớn hơn, mỗi trang một ý.

### Collection — giữ, sửa nhỏ

- Ô chi tiết bên phải trống phần lớn diện tích. Nên cho hình món đồ to gấp đôi và trình bày chỉ số thành từng dòng có icon.

### Lamp Tree — sửa

- **Lỗi chồng chữ:** thẻ "Forge fire" có chữ chồng lên chữ CROWN, và câu điều kiện bị cắt (10-lamp-card-right).
- Chín tab chữ nhỏ ("MERCHANT").
- Cả cây là một bức tường nút giống hệt nhau, trượt bước "độ lặp". Trên điện thoại nên mở thẳng một nhánh.

### Shop — sửa

- Có năm khối giành sự chú ý cùng lúc: băng gợi ý, lời thoại chủ shop, bảng Bonds, thẻ mô tả món đồ và các quầy hàng.
- Băng gợi ý nằm ngay trên đầu chủ shop, sát bong bóng lời thoại (Item04, Item06).
- **Sửa:** băng gợi ý nhường chỗ khi người trong phòng đang nói, và chỉ hiện khi không có thẻ mô tả nào đang mở.

### Thẻ chi tiết món đồ — sửa nhỏ

- Dòng "Curio · rare · stackable" bị lặp lại ngay bên dưới thành "Curio · rare · stacks" (Item10, Item11). Nên bỏ dòng thứ hai.

### Combos — sửa nhỏ

- Sáu dòng "Unknown combo" giống hệt nhau, trượt bước "độ lặp". Dòng đầu trang nên cho biết số combo đã tìm.

### Tổng kết run — sửa

- Tờ giấy chỉ chiếm một phần ba giữa màn hình, chữ nhỏ, hai bên là nền tối trống, trượt bước "độ đầy".
- **Sửa:** tờ giấy rộng khoảng 1000 trên khung 1280. Phần thưởng và đội hình chia hai cột, nút Waiting Room hiện ngay trên màn đầu.

### Các màn giữ nguyên

- **Hub:** biển gỗ treo xích và các trạm vẽ tay có ý tưởng rõ.
- **Postbox, Bell, Window, Night Pact, Wardrobe, Museum:** dùng chung một khung, mỗi màn có một tranh riêng bên trái. Ổn.
- **HUD trận đấu, boss, bản đồ, Ending:** đạt.

## Luồng chơi (frontend-ui-engineering)

1. **Mở game:** đi từ loading qua menu chính, Hero select rồi vào trận, mất hai lần bấm. Đạt.
2. **Loading:** hiện chạy cố định 1,8 giây chứ không theo việc tải thật. Nên chờ tải xong rồi mới chuyển sang menu, và vẫn giữ tối thiểu 1,2 giây để đèn kịp sáng.
3. **Waiting Room:**
   - Cửa đi thẳng tới Hero select. Đạt.
   - Trang của từng trạm có hai tấm tiêu đề chồng nhau ("THE WAITING ROOM" và tên trạm). Nên giữ tên trạm và thay tấm "THE WAITING ROOM" bằng nút quay lại có nhãn.
4. **Hết run:** có nút đi Lamp Tree. Đạt.
5. **Màn trống:**
   - Combos trung thực nhưng đơn điệu.
   - Collection, Records và Museum khi chưa có gì vẫn chưa được chụp (mục B7).
6. **Trạng thái lỗi:** lỗi lưu đã dùng câu dễ hiểu. Đạt.
7. **Vùng bấm:** tab Lamp Tree và nhãn MERCHANT nhỏ hơn mức tối thiểu của ngón tay.
8. **Chưa rà:** Records và trang tạm dừng chưa có trong lượt chụp (mục B6).

## Thứ tự làm

1. Loading và main menu: vẽ nền mới, thêm đội chờ ở cửa và tám đèn lồng làm thanh tiến độ.
2. Lỗi chữ chồng trong thẻ Lamp Tree, dòng lặp ở thẻ chi tiết, dòng "Starts with" bị cắt.
3. Băng gợi ý trong shop nhường chỗ cho lời thoại.
4. Tổng kết run và How to Play: làm đầy màn hình.
5. Bỏ tấm tiêu đề "THE WAITING ROOM" bị chồng trên các trạm; tab Lamp Tree to hơn.

## Đã làm (31/10/2026)

- **Loading:**
  - Bức nền mới là con hẻm đêm dẫn tới cửa Waiting Room.
  - Tám đèn lồng của mạch truyện đứng trên bậc đá và sáng dần theo tiến độ.
  - Tên game đặt cùng vị trí với menu chính.
- **Main menu:**
  - Bức nền mới vẽ cánh cửa hé sáng và thảm trống phía trước.
  - Đội của lần chơi gần nhất đứng chờ: anh hùng ở giữa, sáu bạn quanh, mỗi bạn có hoạt ảnh riêng và quay mặt về phía anh hùng, thỉnh thoảng một bạn nhảy lên.
  - Lần chơi đầu tiên dùng một đội mặc định.
  - Sau Act I, phòng có chiếc ghế quay vẫn giữ nguyên và chỉ có anh hùng.
- **Ảnh nền cũ:** đã xóa `Menu/Backdrop.png` và `LoadingPainting.jpg` để gọn build.
- **Lamp Tree:** dòng mô tả và nhãn trong thẻ xếp theo chiều cao thật của chữ, câu điều kiện mở khóa nằm ở đáy thẻ. Hết chồng chữ.
- **Thẻ chi tiết món đồ:** bỏ dòng loại món bị lặp.
- **Shop:** băng gợi ý chờ tới khi người trong phòng nói xong.
- **Chứng cứ:** `Docs/Evidence/UIAudit/menu_loading_redo.jpg`. Lượt chụp playtest đạt 85/85.

**Điểm chấm lại:**

| Màn | Điểm (TK/DD/ST/ND → tổng) | Ghi chú |
| --- | --- | --- |
| Loading | 7.5 / 7 / 8 / 7 → 7.4 | Có ý tưởng riêng: đèn lồng của mạch truyện làm thanh tiến độ. |
| Main menu | 7.5 / 7.5 / 8 / 7 → 7.5 | Có "thứ sống": đội của chính người chơi. |

**Còn lại:**

- How to Play và tổng kết run chưa đủ đầy.
- Tấm tiêu đề "THE WAITING ROOM" vẫn chồng trên trang các trạm.
- Tab Lamp Tree còn nhỏ.
- Loading vẫn chạy theo thời gian cố định, chưa theo việc tải thật.

## Vòng 2 (31/10/2026)

**Đợt art 4 (13 món):**
- sách tranh mở;
- thẻ ghi nhớ thường và thẻ đã ghi;
- dấu sáp đã đóng và chỗ trống chờ đóng dấu;
- kệ thư trống;
- ba icon Settings;
- huy hiệu bình minh và nến tắt;
- ngọn đèn thu nhỏ lửa;
- tấm hàng combo bằng gỗ.

**Cây đèn đường cho loading:** vẽ hai bản, tắt và sáng, khớp nhau từng điểm ảnh. Đèn đứng đúng phối cảnh: đèn xa nhỏ và sát nhau, đèn gần cửa to và thưa. Khi sáng có quầng sáng ở đầu đèn và vũng sáng trên nền đá ướt.

**Khung mô tả theo độ hiếm:** giữ bộ khung cũ có art riêng từng độ hiếm và trang trí hai bên. Khung 9 mảnh của đợt 2 đã bỏ.

**Đã sửa:**
- How to Play thành cuốn sách tranh: ảnh dán một trang, chữ lớn ở trang đối diện, hai bên đổi chỗ qua mỗi trang.
- Dream Journal: thẻ giấy da có chân dung anh hùng, dấu sáp và thanh tiến độ vẽ tay.
- Records khi chưa có lượt chơi nào có tranh kệ thư trống và câu nói vì sao trống.
- Records có lượt chơi: bảng tổng kết vẽ ở 2/3 cỡ, hết bị cắt chữ.
- Settings: mỗi mục mở đầu bằng icon vẽ tay, bỏ đường kẻ mảnh.
- Combos:
  - dòng đầu trang cho biết đã tìm bao nhiêu combo;
  - combo chưa tìm ra ghi tên bạn đã gặp ("Candle Snail + ?");
  - mỗi combo nằm trên tấm hàng gỗ.
- Tạm dừng: ngọn đèn thu nhỏ lửa trên thẻ.
- Kết thúc run: anh hùng đứng trên huy hiệu bình minh hoặc nến tắt, cột trái là khung vẽ tay.
- Collection: ô chi tiết là tủ trưng bày lớn, hình món đồ to.
- Waiting Room: khi mở một trạm, tấm tiêu đề phòng nhường chỗ cho tiêu đề trạm.
- Lamp Tree: tab cao 44 và viết hoa chữ đầu.
- **Lượt chụp mới:** Records, Dream Journal, ba trang tạm dừng, bốn màn trống và hai màn kết thúc run thật. Playtest đạt 90/90.

**Điểm vòng 2:**

| Màn | Trước | Sau |
| --- | --- | --- |
| Loading | 7.4 | 7.9 |
| How to Play | 6.3 | 7.8 |
| Dream Journal | 6.5 | 7.6 |
| Records khi trống | 5.8 | 7.3 |
| Kết thúc run | 6.5 | 7.6 |
| Combos | 6.4 | 7.2 |
| Settings | 7.1 | 7.4 |
| Collection | 7.4 | 7.6 |
| Lamp Tree | 6.4 | 7.0 |

**Chứng cứ:** `Docs/Evidence/UIAudit/review_round2.jpg`.

## Vòng 3 (31/10/2026)

- **Logo vẽ tay "Arcane Brigade":** chữ gỗ thếp vàng, có ngọn đèn trên chữ A và hoa văn đồng bên dưới. Thay cho chữ font thường ở menu chính và màn loading, đặt cùng một vị trí ở cả hai màn.
- **Lamp Tree:**
  - Nền là tranh cây gỗ và đồng trong phòng đêm, phủ 70% và giữ tối để đèn nổi lên.
  - Khi mở một nhánh, đèn chạy ngang qua thân cây vẽ tay.
- **Hero select:** anh hùng được chọn đứng trên một sân khấu tròn có đèn rọi.
- **Lượt chụp:** playtest đạt 90/90. Chứng cứ: `Docs/Evidence/UIAudit/review_round3.jpg`.

**Điểm vòng 3:**

| Màn | Trước | Sau |
| --- | --- | --- |
| Loading | 7.9 | 8.2 |
| Main menu | 7.5 | 8.0 |
| Lamp Tree | 7.0 | 7.6 |
| Hero select | 7.3 | 7.6 |

## Loading, lần cuối (31/10/2026)

- **Bỏ cột đèn ghép chồng lên tranh:** dù vẽ riêng, cột đèn không ăn nhập với ánh sáng và phối cảnh của bức tranh.
- **Tranh mới:**
  - Vẽ con hẻm có tám cột đèn nằm sẵn trong tranh, đèn tắt.
  - Từ chính bức đó, sửa thành bản đèn sáng. Hai bức khớp nhau gần như từng điểm ảnh.
- **Tách lớp sáng:** tách phần khác nhau quanh từng đèn (đầu đèn, quầng sáng, vũng sáng trên đá) thành tám lớp có viền mờ.
- **Trong game:**
  - Các lớp hiện dần lên bức đèn tắt.
  - Đèn đã sáng có lửa chập chờn, lõi sáng nhấp nháy và đốm lửa bay lên.
  - Tám đèn sáng hết ở 75% thời gian tải, rồi giữ một nhịp trước khi sang menu.
- **Chứng cứ:** `Docs/Evidence/UIAudit/loading_lamps_painted.jpg`. Ảnh trái chụp giữa lúc tải, ảnh phải khi đã sáng hết.

## Rà khoảng đệm (31/10/2026)

Cắt từng danh sách, hàng và thẻ ở cỡ thật để soi khoảng giữa các phần tử và khoảng giữa chữ với viền vẽ tay.

**Đã sửa:**
- Night Pact: các hàng cách nhau, phần trang trí của hàng dưới không còn chạm hàng trên, chữ cách viền.
- Combos: biểu tượng tránh nắp đồng ở đầu hàng, chữ có lề phải, dòng đếm combo xuống dòng thay vì tràn.
- Dream Journal: tiêu đề có lề trên, chân dung tránh dải ruy băng, tiêu đề dài tự thu nhỏ cho vừa một dòng.
- Main menu: có khoảng thở giữa dòng chữ phụ và nút đầu tiên, icon trên nút nhỏ tránh nắp trái.
- Hero select: tên và tag vũ khí nằm bên phải sân khấu, dòng loadout cách viền đáy.
- Postbox: chữ dừng trước nếp gấp của phong bì.
- Phòng bí mật: phần thưởng hai dòng có đủ chỗ.
- Waiting Room: biển treo cách tấm tiêu đề.
- Wardrobe: các hàng cao hơn, có thanh tiến độ tới mốc READY.
- Lamp Tree trên điện thoại: mở thẳng nhánh đầu tiên có thứ mua được.

**Chứng cứ:** `Docs/Evidence/UIAudit/padding_pass_1.jpg` và `padding_pass_2.jpg`. Playtest đạt 90/90.

## Rà khoảng đệm, vòng 2 (31/10/2026)

Cắt 28 khối UI ở cỡ thật, ở hai độ phân giải là 1920×1080 và 2560×1369, rồi sửa mọi chỗ chữ hoặc icon sát viền.

**Đã sửa:**
- **Bonds:** hàng cao 58, chữ nằm giữa hàng có khoảng trống trên dưới, hàng đầu cách dải tiêu đề, giấy da bỏ đường kẻ.
- **Menu chính:** icon trên nút nhỏ hơn và có lề quanh.
- **Settings:** núm slider tròn lại (phần tràn trước chỉ cộng theo chiều dọc).
- **Thẻ shop:**
  - thẻ cao 260;
  - tên, chip và chữ nằm trong viền khung độ hiếm;
  - chữ BUY tự thu nhỏ cho vừa nửa trên của nút;
  - giữa chữ BUY và giá có khoảng trống;
  - chữ trên chip có lề.
- **Thẻ quà của Keeper:** chỉ hiện số dòng vừa với chiều cao thẻ.
- **Bến đò và Khách Cuối:** các khung chân dung có khoảng cách với nhau.
- **Bảng tổng kết:** tiêu đề nằm giữa dải tiêu đề vẽ tay, các dòng bên dưới cách ra.
- **Tạm dừng:** ngọn đèn nằm trong khung, không đè lên lời thoại.
- **Collection:**
  - tab có trạng thái được chọn thật, bỏ dấu chấm;
  - icon tránh nắp tab;
  - tên trong ô nằm trong viền tủ.
- **Records:** hàng cao hơn và lùi vào trong để nắp hai đầu không bị cắt.
- **Postbox:** dòng "Bring it from hero select" có đủ hai dòng.

**Chứng cứ:** `padding_pass_3.jpg`, `padding_pass_4.jpg`. Playtest đạt 90/90.

## Rà toàn bộ, vòng 3 (31/10/2026)

**Waiting Room:**
- Thêm năm món đồ vẽ tay: ghế chờ, chồng vali, máy hát, giá treo áo, ống cắm ô.
- Các bạn đi lại trong phòng to gấp rưỡi, dùng khung đi bộ của chính mình, và ưu tiên đội của đêm gần nhất.
- Giữ phòng này vì đây là nơi tiêu Lamp Oil và chứa bảy trạm. Bỏ đi thì mất cả vòng tiến bộ.

**Trang các trạm:**
- Tranh và dòng chữ của trạm xếp thành một cụm ở giữa cột trái.
- Nội dung cách tấm tiêu đề.
- The Bell và The Window được hạ vào giữa khung.

**Tiêu đề:**
- Mọi tấm tiêu đề trang đều căn giữa: menu, trong trận, Brigade.
- Tiêu đề trên thẻ cũng căn giữa: thư, giấc mơ, Collection, bảng tổng kết.

**Bonds:** dải tiêu đề giữ cố định nhờ viền 9 mảnh phía trên 76 px. Chữ "Bonds" nằm giữa dải, các hàng bắt đầu dưới dải.

**Băng gợi ý:** chữ tự thu nhỏ cho vừa băng.

**Bến đò và Khách Cuối:**
- Thẻ cao hơn.
- Tên dưới chân dung dùng cỡ 22, xuống dòng đủ chỗ.
- Các khối cách nhau.

**Cỡ chữ:** thẻ khách ghé, phòng bí mật, bàn thờ, trang chi tiết đội hình và thẻ Khách Cuối được nâng lên cỡ chuẩn 28.

**Hero select:** icon chủ đề tuyển quân nằm trong bảng anh hùng, dưới tag vũ khí.

**Ảnh chụp toàn bộ:** 78 màn trong `Docs/Evidence/UIAudit/full_2026-10-31/` (20 tờ, mỗi tờ 4 màn). Playtest đạt 90/90.
