# Rà độ thống nhất theme toàn app — 17/10/2026

Chuẩn đối chiếu: `Mien_Cho_Act_One_Story.md` là canon Hồi 1; `Art_Master_List.md` là hướng art hiện hành. Miền Chờ có cư dân sống, sự chờ đợi và tình thương có điều kiện; kinh dị qua dấu vết, khoảng trống và lựa chọn. Art raster cổ tích tối, nét mực, đồng thau/hổ phách/teal/kem; không dựng lại bằng Blender. Đỏ ember là tín hiệu thù địch; ánh sáng đoàn là sự đồng hành. Giữ các khoảng ấm, hình dạng kỳ quặc và sự dễ đọc của combat.

Đây là **audit, chưa sửa gameplay/art/truyện**. Không build/chạy test theo yêu cầu chốt nhanh trước đó. Quét danh mục 326 file C# trong UI/Presentation/Core/Data, đọc các đường thực thi liên quan, đối chiếu canon và xem trực tiếp 11 ảnh/contact sheet đã có. Không đọc sát từng dòng của cả 326 file, không duyệt từng frame của toàn catalog, không nghe toàn soundtrack. Ảnh lưu chỉ chứng minh bản tại thời điểm chụp; các thay đổi code mới hơn được ghi riêng. Snapshot bắt đầu tại `fe67d31d`; workspace còn thay đổi VFX/description của công việc khác, được giữ nguyên.

## Những điểm cần ưu tiên

### 01 · P1 · Nhiều thế hệ art đang đứng cùng một cảnh

**Đã thấy trong ảnh, code vẫn có hai nguồn art.** Hero và một số companion/vũ khí cũ dùng mảng màu tươi, biên dày, ít sắc độ; người bán hàng, Postscript, boss cửa và đồ cảnh mới dùng nhiều lớp vẽ, hoa văn và ánh kim. Đặt cạnh nhau khiến một số nhân vật giống sticker dán lên cảnh. Đây là nhận xét mỹ thuật, không phải lỗi tải asset.

Bằng chứng: [combat 17](../PreviewExports/ArtMaster/Native/Combat-17.png), [shop](../PreviewExports/ArtMaster/Native/Shop-kind-eyes.png), [volley mới](../PreviewExports/WeaponVfx/Volley_Bell_Umbrella_Hammer_Magnet.jpg). `Runtime/Data/ActOneArtwork.cs:39` thay art cho các diễn viên canon riêng, phần còn lại tiếp tục lấy catalog.

**Hướng sửa:** chuẩn hóa nét, độ bão hòa và chất liệu cho hero/vũ khí xuất hiện nhiều nhất trước. Giữ silhouette đơn giản ở kích thước chơi; không tăng hoa văn toàn bộ 66 loài chỉ để giống cảnh.

### 02 · P1 · Vũ khí lớn làm cảnh thành một cuộc diễu hành đồ vật

**Đã thấy trong ảnh mới và có công thức hiện hành.** `Runtime/Presentation/UnitVisual.cs:91` cho vũ khí cận chiến companion dài tối thiểu 1.55 và tối đa 2.5 lần chiều cao cơ thể. Chìa khóa đỏ, búa và bàn chải che người cầm, che mặt boss và lấn quan hệ giữa các nhân vật. Đội đông làm vấn đề nặng hơn.

Bằng chứng: [volley mới](../PreviewExports/WeaponVfx/Volley_Bell_Umbrella_Hammer_Magnet.jpg), [boss cửa](../PreviewExports/ArtMaster/Native/Boss-15.png). Ảnh boss cũ chỉ minh họa tỷ lệ, không nghiệm thu VFX mới.

**Hướng sửa:** tách kích thước nghỉ khỏi nhấn mạnh lúc ra đòn; giới hạn phần che mặt/người; dùng chuyển động và vệt tác động để đọc đòn. Phải giữ khớp hình và tầm đánh, không thu nhỏ art một cách tùy ý rồi để damage xảy ra ngoài hình.

### 03 · P1 · Phe ta còn dùng đỏ gần với tín hiệu nguy hiểm

**Đã thấy.** Chìa khóa, búa, Tally và một số palette hero có đỏ tươi chiếm mảng lớn. `Runtime/Presentation/HeroMechanicStyle.cs` còn các palette đỏ/hồng mạnh, ví dụ GlassDice và NightMaw. `SharedLightField.IdentityColor` chọn hue trên toàn vòng HSV, không giới hạn bảng màu đồng hành. Ánh sáng hồng nhạt không tự động là lỗi, nhưng hệ hiện tại chưa bảo đảm ngôn ngữ màu như master list.

**Hướng sửa:** đỏ rượu vang có thể là màu vật liệu; tránh lõi/viền phát sáng đỏ ember cho đòn phe ta. Chốt bảng màu nguồn sáng đoàn và màu báo sát thương; không làm mọi hiệu ứng cùng vàng đến mức mất nhận diện hero.

### 04 · P1 · Ánh sáng đoàn còn giới hạn cứng 16 nguồn

**Xác nhận trong code.** `Runtime/Presentation/SharedLightField.cs:9` đặt `Capacity = 16`; `Add` trả về ngay khi đầy. Có hero và hơn 15 companion, các sinh vật sau đó không đóng góp nguồn vào field hoặc `Strength`. Đội hiện đã bỏ trần cơ thể; bốn companion truyện cũng dùng ngân sách này. Chưa thể khẳng định mọi người đều đang soi sáng cho nhau khi đội đông.

**Hướng sửa:** mỗi người đóng góp vào ánh sáng, sau đó gộp vùng gần nhau vào ngân sách GPU. Giữ trần cường độ để đội đông không biến đêm thành ban ngày. Không cần tăng số đèn shader theo số cơ thể.

### 05 · P1 · Keeper nguyên hình vẫn chưa có bộ art được yêu cầu

**Xác nhận asset còn thiếu tại snapshot.** Không tìm thấy `Keeper_Silhouette`, `Pickup_PrismShard`, `Keeper_Whole_Body`, `Keeper_Kneel` trong thư mục giao art Hồi 1. `Runtime/Presentation/StageTransfer.cs:59` còn fallback từ silhouette sang một frame boss cũ. Master list đã có yêu cầu mới cho thân/hand/carpet/eye/kneel nhưng đầu tài liệu vẫn dễ tạo cảm giác toàn bộ art đã xong.

**Hướng sửa:** vẽ Keeper với một thiết kế thống nhất giữa ba lần vận chuyển và trận shop 17; trạng thái quỳ, bộ phận vỡ và quà cầu vồng phải cùng hình thể. Đây là công việc bổ sung sau đợt 261 PNG, không phủ nhận đợt giao trước.

### 06 · P1 · Đoàn bình minh thiếu bốn người quan trọng nhất

**Xác nhận đường dựng danh sách.** `Runtime/Presentation/MienChoPresentation.cs:299` — `DrawDawn` lấy hero và `State.Roster`. `Runtime/Presentation/NightmareDeparture.cs:18` lấy cứu hộ, roster và hero. Cả hai chưa thêm `CombatCatalog.StoryCompanions` hoặc bốn diễn viên canon. Trong khi `Core/ActOneRun.cs:32` giữ Hob/Latch/Postscript/Tally riêng ngoài roster.

Hob có thể có mặt trong tranh minh họa nền, nhưng điều đó không thay việc thiếu diễn viên sống trong hai đoàn dựng bằng code.

**Hướng sửa:** dựng một danh sách người thực sự đi cùng cho ending, giữ art/tên riêng của bốn bạn; xử lý cứu hộ và ký ức đã bán, tránh nhân đôi. Cảnh bình minh cần cho thấy họ cùng ra ngoài.

### 07 · P1 · Màn chọn ending còn dấu vết lời giải cũ

**Xác nhận đường render.** `Runtime/Presentation/NightmareFarewellRoom.cs:23` còn chọn `NightmareAssets.Current.ManFrames[0]`; dòng 77 ghi “THE MAN BEHIND THE RECEIPT / I remembered the price. I forgot the name.” Canon mới nhấn mặt thật đã bán, mặt cuối trống và hợp đồng có chữ ký ghế. Dùng một người đàn ông hiện hình lại ở màn này có nguy cơ làm người chơi hiểu rằng boss được trả mặt hoặc được cứu thành người bình thường.

**Hướng sửa:** thống nhất trạng thái hậu chiến với `LastCustomer_Blank`, hợp đồng cháy và ngọn đèn cuối. Nếu cố ý cho một hình người xuất hiện, cần một diễn biến giải thích rõ thay vì kế thừa art cũ.

### 08 · P1 · Lời thì thầm có thể làm lộ bí mật trước cảnh tiết lộ

**Xác nhận điều kiện chọn thoại.** `Runtime/Presentation/NightmareDialogue.cs:95` có “His ledger's first entry is his own name.” `Whisper` lấy bảng theo zone 16–20, không chờ mốc wave 19 hoặc trang bằng chứng đã đọc. Các bảng khác cũng có chi tiết về thư, khóa và đêm thứ Năm được lựa theo zone thay vì tiến độ phát hiện.

**Hướng sửa:** gắn câu chứa thông tin mới với wave chính xác và cờ chứng cứ; trước đó chỉ gieo dấu hiệu. Sau khi đã biết thì lời nhắc được phép nhắc lại. Không chỉ thay RNG hoặc bỏ ngẫu nhiên một câu.

## Art, animation và cách kể cần hoàn thiện

### 09 · P2 · Mặt Customer đổi theo đồng hồ, chưa phải diễn xuất gắn ý nghĩa

**Xác nhận.** `Runtime/Data/ActOneArtwork.cs:45` đổi Pleasant/BorrowedFace bằng `FloorToInt(time * .45f) % 3`. Mặt đổi theo chu kỳ ngay cả khi không có lời dụ dỗ, mốc mất đèn hoặc phản ứng của nhân vật. Có pose mới không đồng nghĩa đã có một chuỗi diễn phù hợp câu chuyện.

**Hướng sửa:** giữ mặt đủ lâu để nhận ra; đổi mặt khi dụ nhắc lời hứa, bị phá phòng thủ hoặc mốc trận. Có thể giữ một số bất thường nhỏ không đều để tạo cảm giác mặt vay mượn mất kiểm soát.

### 10 · P2 · Nhân vật trong phòng khám phá còn giống quân cờ trượt

**Xác nhận code; ảnh cũ hỗ trợ bố cục.** `Runtime/Presentation/WaitingRoomController.cs:67` tạo SpriteRenderer từ một `Body`; đoạn Update chỉ đổi vị trí/flip của hero và Lerp vị trí các bạn. Không đi qua đầy đủ `UnitVisual`/AuthoredMotionSet như combat. Cảnh kéo Hob đã có kéo, thêm người và đứng dậy; không coi nó hoàn toàn chưa làm. Tuy nhiên, bước chân, quay đầu, bàn tay kéo và tư thế nghỉ còn thiếu ở đường này.

**Hướng sửa:** dùng diễn viên nhẹ có idle/walk/turn/pull/release, nhịp ngừng và khoảng nhìn nhau. Kéo Hob nên kết bằng một nhịp đứng vững/đỡ vai thay vì chỉ nâng sprite lên.

### 11 · P2 · Đạo cụ tương tác trong phòng còn dùng hình đại diện

**Xác nhận.** `WaitingRoomController.cs:77` lấy ledger/clock/key cho cả ba điểm bằng chứng của các phòng. Dòng 84 dùng `MienCho/EmptyChair`; dòng 86 dùng body hero làm hình nền cho một vùng `WaitingGlow`. Shader glow là hiệu ứng trừu tượng, không phải một cửa sổ được vẽ đúng thiết kế.

**Hướng sửa:** mỗi bằng chứng có hình đúng vật đang được đọc: vạch chiều cao, dấu chân, mẩu giấy, bảng đích đến. Ghế bình thường có thể khác Ghế Ấm, nhưng phải chủ động chọn mẫu thay vì dùng hình cũ mặc định. Phòng 8/14 có đường vào cũ đang bị ẩn; phòng cứu Hob vẫn là đường hiện hành cần ưu tiên.

### 12 · P2 · Nhiều cảnh quan trọng đã được viết, nhưng chưa được diễn tương xứng

**Xác nhận phần viết và một số đường vô hiệu hóa; chất lượng nhịp cần xem player.** `WaitingShopEncounter.ForWave` luôn trả `None`: bàn/điện thoại/gương cũ đã được chuyển vào journal. Canon có tiếng gõ bên kia cửa, ô ướt, Hob ngồi ghế đẩu, Tally đọc rồi giấu hóa đơn; có trang văn và panel, nhưng không nên gọi tất cả là cảnh tương tác đã diễn trên map.

**Hướng sửa:** chọn một vài cảnh trọng tâm để diễn trực tiếp — điện thoại shop 8, cửa mở và ô ướt, Hob tự ngồi ghế thường, Tally vắng mặt. Không phục hồi toàn bộ encounter cũ nếu sai canon, và không biến mỗi wave thành cutscene dài.

### 13 · P2 · Từ vựng đồ chơi vẫn lẫn với cư dân có đời sống

**Xác nhận dữ liệu có thể hiện ra.** `Resources/RunCatalog.asset` còn bond “Toy Knights” và “Playtime”; `Runtime/Data/BuildMorph.cs:28` là “Toybox Orchestra”. `Core/OctoberCombat.cs:209` ghi nguồn damage “Living toy poison”; fallback `OctoberCompanions.Copy` còn “An original toy companion”. Fallback chỉ đáng báo là câu có thể quay lại nếu một loài không có mapping; không khẳng định mọi người chơi đang thấy nó.

**Hướng sửa:** đổi lớp tên/mô tả theo các hội cư dân, đoàn diễn hoặc nhạc cụ; giữ ID save nếu cần. Không cấm đồ chơi tồn tại trong thế giới và không coi tên class `ArenaToy` là lỗi lore tự thân. Quan trọng là không gọi sinh vật có ý thức là tài sản/đồ chơi trong giọng kể trung lập.

### 14 · P2 · UI và hướng dẫn chưa thống nhất với quyết định tương tác mới

**Xác nhận.** `MienChoPresentation.cs:107` vẫn nhận J. `RunMenu.cs:271` vẫn hướng dẫn J và ghế mở “paths and companion stories”. Trong khi `WaitingArchiveInteractions.cs:16,29` chỉ có một spot cho journal và ẩn đường ghế cũ. `SecretEventRoom.cs:65,73` còn giữ phím .85s; cứu Hob giữ 2.4s. Đây là lệch quyết định UX “bỏ phím K/Y/J, bấm Space cơ bản”, không phải lỗi chất lượng tranh.

UI chính đã đổi màu và `ArcadeSkin.Slant` nay vẽ bo mềm; ảnh UIPoster cũ không đủ để kết luận UI hiện tại còn toàn viền neon/hình xiên. `WaitingJourney` còn nhãn kỹ thuật “ENTER WALKABLE ROOM”, nhưng đường vào cũ đang bị ẩn nên không xếp nó như lỗi màn chính đang lộ.

**Hướng sửa:** một bảng hướng dẫn duy nhất từ input hiện hành; thao tác tại vật thể thống nhất; cảnh kéo có thể phát thành animation sau một lần bấm, vẫn giữ ý nghĩa cả đoàn cùng kéo.

### 15 · P2 · Icon stat chưa phải bộ thiết kế chuyên dụng

**Đã biết và thấy trong ảnh.** 49 mapping đang phối relic/vũ khí có sẵn, một số chi tiết dài/mảnh hoặc badge nhỏ khó đọc ở 24 px. Có cùng nguồn art nhưng chưa cùng trọng lượng thị giác hoặc nghĩa biểu tượng. [Ảnh kiểm tra](../PreviewExports/CompanionReactions/Native/Painted-icons-24-and-40px.png).

**Hướng sửa:** atlas riêng khi công cụ ảnh dùng được; silhouette và độ đậm đồng nhất, tách nghĩa damage/melee/ranged/hero/companion ngay ở kích thước chơi. Công cụ vẽ đã báo hạn mức; không được ghi 49 tranh mới đã hoàn tất.

## Âm thanh và tài liệu cần đối chiếu tiếp

### 16 · P2 · Trận The Bedside chưa chọn bài ru làm nhạc trận riêng

**Xác nhận nhánh chọn nhạc; chưa đánh giá bằng nghe.** `Core/ArenaController.cs:248` chỉ force menu ở bình minh; `ArcaneCueLibrary.SceneFor` đưa mọi boss vào scene `boss`. `WaitingRealmStage.cs:108` dừng motif khi vào combat. Vì vậy phần canon “bài ru thay nhạc chiến đấu” chưa được biểu đạt bằng đường chọn nhạc riêng cho SIT CLOSER.

Manifest có 15 piece thuộc family `toybox_individual_v4`, gồm các bài late combat 166/172 BPM. Điều đó không tự chứng minh nhạc sai hoặc cần thay toàn bộ. Cần nghe trong ngữ cảnh cùng tiếng gõ, gió, thoại và những đoạn im lặng. Fallback jazz chỉ dùng khi manifest/clip lỗi, không coi đó là nhạc chính đang chạy.

**Hướng sửa:** bài ru biến thể riêng cho trận ẩn, tiếng gõ rõ mà không tăng volume gây giật mình; nhịp trống thấp xuống khi cần nghe câu chuyện. Sau đó mới quyết định piece nào giữ/viết lại.

### 17 · P2 · Tài liệu hoàn thành chưa phân biệt bộ đã giao với phần đang thay mới

**Xác nhận.** Master list đầu trang ghi 261/261 nhưng cuối có bộ Keeper mới chưa giao; bản thiết kế truyện cũ còn mô tả sáu tuyến và các phòng cũ. Đường ghế tới `WaitingJourney` đã ẩn, bốn route cũ lại kiểm roster thường thay vì slot truyện; `WaitingCompanionQuests.Futures` không có caller ending hiện hành. Đây là nội dung cũ/không tới được ở luồng chuẩn, không nên báo sáu tuyến đã hoàn thiện trong bản canon hiện tại.

**Hướng sửa:** ghi rõ phần đã giao theo đợt, phần còn thiếu, phần retired. Nếu khôi phục sáu tuyến thì nối qua diễn viên canon và màn tương tác phù hợp; nếu bỏ thì loại khỏi tuyên bố nghiệm thu. Hồi 2/3 đã để sau theo quyết định của chủ dự án, không tính là việc thiếu của Hồi 1.

## Những phần nên giữ và giới hạn kết luận

- Ghế gỗ ấm, người bán hàng hiền mệt mỏi, Postscript phong thư, art boss cửa và các panel mới đã tạo ngôn ngữ chung tốt. Không cần vẽ lại toàn bộ chỉ vì còn điểm lệch.
- Cảnh chính và đồ cảnh mới dùng tọa độ thế giới; không lặp lại kết luận cũ rằng mọi prop còn đi theo camera. Lơ lửng có chủ ý và bỏ gió dưới vật đã là hướng được chốt.
- Không phát hiện lý do để đưa Blender trở lại. Tên file/builder lịch sử không chứng minh một model Blender đang xuất hiện trong player.
- Mắt thật gắn quái và mắt trang trí xanh đang được phân lớp; không viết chúng thành một thực thể lore mới trái quyết định “mắt đỏ chỉ là thể hiện khi tối”.
- VFX đang có đợt thay mới ở working tree; ảnh mới cho thấy vẫn cần chỉnh tỷ lệ/màu nhưng không đủ để khẳng định toàn bộ animation mới xấu hoặc hỏng. Hình học nhẹ báo vùng nguy hiểm có ích và một phần đã được duyệt; không thay mọi dấu báo bằng tranh đặc che sàn.
- Các bản sửa bóng tối, preview bond, nerf địch và khóa chat vừa commit chưa có nghiệm thu runtime mới. Báo cáo không coi chúng đã đạt thẩm mỹ chỉ vì code tồn tại, cũng không tự động gọi chúng là lỗi.

Thứ tự đề xuất: **đoàn ending + tránh spoiler → art Keeper → tỷ lệ và màu vũ khí → ánh sáng đội đông → diễn xuất phòng/cảnh trọng tâm → icon, nhạc và tài liệu/hướng dẫn**. Cần chốt một bộ mẫu cùng camera/kích thước chơi trước khi mở đợt vẽ diện rộng.
