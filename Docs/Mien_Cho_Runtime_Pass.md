# Miền Chờ — bản triển khai cốt truyện và ánh sáng đồng hành

**Đính chính theo canon hiện tại 17/10:** đây là nghiệm thu lịch sử trước khi viết lại Hồi 1. Ba phòng/sáu tuyến ở các mục dưới không đồng nghĩa đang truy cập được trong game canon: Station/FootstepHall và sáu route lựa chọn đã retired; phòng cứu Hob và journal Hồi 1 vẫn dùng. Các sửa theme mới, 70 PNG bổ sung và giới hạn nghiệm thu được ghi tại [Theme_Consistency_Fixes.md](Theme_Consistency_Fixes.md).

**Art hiện hành:** bộ Blender đã được thay bằng [13 asset vẽ mới theo theme game](Mien_Cho_Painted_Art_Pass.md). Các phần mô tả tạo asset Blender dưới đây là lịch sử; logic truyện và tương tác vẫn được giữ.

Ngày triển khai: 16/10/2026. Bản này đưa hướng đã chốt trong [tài liệu thiết kế](Mien_Cho_Story_And_Shared_Light.md) vào chiến dịch 20 wave hiện tại.

**Bản mở rộng trước đợt viết lại canon:** [đèn cổ và sương đen](Act_One_Lanterns_And_Black_Fog.md) từng bổ sung ba phòng khám phá 2D, sáu tuyến có lựa chọn, đèn mesh/Point Light và sương nhiều lớp. Các gate/số đo ở phần dưới và [ActOne](../PreviewExports/ActOne/) là lịch sử, không nghiệm thu source mới nhất. Hồi 2–3 chưa triển khai.

## Cốt truyện đang có trong game

- **10 mốc cố định, 27 trang:** wave 1, 4, 6, 8, 10, 12, 14, 16, 19 và 20. Mốc đầu và cuối mở trước combat; các mốc còn lại mở ở shop sau wave tương ứng. Tiến trình không cần RNG phòng bí mật. Người chơi có thể chọn đọc sau; chương đã mở luôn có trong journal của shop bằng nút **WAITING JOURNAL / J**.
- Nội dung giải thích Miền Chờ có trước nhân vật, lời hứa thời thơ ấu, người đến đón bị ngăn ở cửa, các chứng cứ độc lập và cách thực thể kiểm soát ký ức. Người Luôn Ở Đó, Customer, Keeper và merchant có vai trò riêng. Các cảnh điện thoại, phản chiếu, bước chân, nhà ga và căn phòng một người tạo nhịp kinh dị xen kẽ.
- **31 đoạn nhập cảnh riêng** theo hero. Đây là góc nhìn đầu hành trình; không phải 31 chiến dịch độc lập.
- **66 tiểu sử riêng** nối trực tiếp vào description companion. Mỗi loài có nguồn gốc và điều mình muốn. Ngoại hình hiện tại giữ nguyên; không coi chúng là vật sở hữu hay gọi chung là đồ chơi. Tên ID/save cũ như `RescuedToy` giữ để tương thích.
- **6 tuyến companion, mỗi tuyến 3 đoạn**, mở theo tiến độ 1–8 / 9–16 / 17+: Teapot Pilgrim, Hood Wisp, Shell Postman, Paper Devil, Carousel Pony và Secret Patch Bear. Có tiểu sử và đoạn hành trình trong mục **THE PEOPLE TRAVELLING WITH YOU**; banter ngoài shop đổi theo chặng. Đây là các vignette viết sẵn và thoại theo tiến độ, chưa phải hệ quest phân nhánh hoặc tương tác vật lý riêng cho từng cảnh.
- Hai farewell dẫn tới bình minh thật. Nhánh để đèn là lời mời; không ép quay lại hoặc tha thứ. Câu hỏi về giá trị bản thân được trả lời bằng một chỗ ngồi ở bàn. Kết quả/win chỉ mở sau khi đi hết trang ending như hệ thống hiện tại.

Journal dừng simulation và clock trình bày trong lúc đọc; nội dung dài có scroll. Acknowledgement lưu trong `MienChoMemory` riêng, không sửa revision ending cũ. Save thiếu trường mới vẫn đọc được. Save ở kết quả/kết thúc không bị tự mở lại toàn bộ truyện. Đọc sau được ghi là đã ghi nhận chương để tránh tự bật lại, không phải bằng chứng người chơi đã đọc hết chữ.

Đợt bổ sung đã có **lưu trang riêng cho từng chương**, khôi phục đúng trang khi mở lại hoặc tải save. Cờ hoàn thành chỉ bật khi xác nhận trang cuối; nút đọc sau vẫn chỉ ghi acknowledgement. Bản save cũ có mảng trang thiếu/null/rỗng được nâng cấp khi mở journal. Chặn input trong frame mở/đóng truyện để Space/A không mua đồ và Esc/B không bật pause ngoài ý muốn. Có cuộn bằng stick/mũi tên, LB/RB chuyển trang, X chuyển mục companion, B quay lại; mở journal trong shop bằng J hoặc nút nhấn stick phải. Chưa nghiệm thu trực tiếp trên tay cầm vật lý.

## Ba cảnh khám phá trong shop

Đã thêm các vật thể đặt trong world, xuất từ scene Blender riêng và được render bởi Unity. Chúng ở góc shop, không có collider hoặc phần thưởng stat, không chiếm quota shop và không xuất hiện trong combat/phòng bí mật/kết quả.

| Shop sau wave | Vật thể | Tương tác |
| --- | --- | --- |
| 4–5 | Bàn ăn, ba ghế, hai cốc, bảng tên MARA | Tới gần, R/X hoặc bấm Examine để xem chứng cứ về một chỗ đã được đặt trước |
| 6–7 | Điện thoại quay số không nối dây | Khi tới gần có tối đa hai lượt chuông; không phát lớp mới khi Reduced Audio. Examine mở chương điện thoại |
| 10–11 | Gương đứng với hình phản chiếu chậm và giọt nước mắt | Hiện hình một companion thật trong đội; solo dùng hero. Hình phản chiếu không chạy theo pose của actor; Examine mở chương gương |

Ánh sáng và bóng được bake trong sprite từ Blender; Unity bổ sung bóng tiếp đất, chuyển động lơ lửng và hạt sáng nhẹ. Theo phản hồi mới nhất đã bỏ khói/vệt gió dưới đồ vật ở cả hệ cũ và mới; xem [Dream_Levitation_02.md](Dream_Levitation_02.md). Mirror dùng sprite của nhân vật thật và lớp màu kính riêng; đây là vignette bằng sprite, không phải phản chiếu ray tracing. Pause dừng chuyển động/giọt lệ/lịch chuông và chặn Examine; rời shop xóa phần trình bày ngay. Chứng cứ đã xem có cờ lưu riêng, không khóa tuyến chính nếu bỏ qua. Mục thoại thở/tiếng nói của điện thoại vẫn là nội dung đọc, chưa có voice acting.

## Ánh sáng trong cảnh thật

- Năm depth theo wave 1–4 / 5–8 / 9–12 / 13–16 / 17+: ambient giảm từ **0.92 đến 0.09**; dawn lên 1.08. Độ tối không tăng theo thời gian đứng yên và chặn ở depth 4 trong endless.
- Hero tự có vùng sáng mềm bán kính **6.4**; mỗi companion thêm nguồn bán kính **3.4**. Màu ổn định theo ID, không làm sprite mất màu gốc. Hào quang nhẹ dưới từng thành viên giúp thấy nguồn sáng.
- `SharedLightField` dựng một texture ánh sáng world-space **256×192**, tối đa **16 nguồn** gồm hero và 15 companion. Nguồn chồng dùng giá trị lớn nhất theo kênh, có trần spill; không cộng 15 lần thành ban ngày. Save đội cũ quá 15 vẫn giữ đội, nhưng ngân sách ánh sáng là 16 nguồn.
- Floor nhận ánh sáng theo world position; landmark và kiến trúc ngoại vi tối theo vùng sáng bằng màu RGB, không giảm opacity đồ cảnh. Shop giữ nền sáng tối thiểu 0.5 để đọc hàng và giá. Đây là giải pháp ánh sáng cho pipeline sprite 2D, không dùng 16 realtime shadow-casting lights.
- Địch xa có silhouette và dấu hiện diện đỏ/cam ở đúng actor đang di chuyển. Có danh sách loài dùng hai mắt; các loài trừu tượng dùng một đốm ember. Watcher trang trí ngoại vi chuyển xanh nhạt để phân biệt với địch. Đây là điểm sáng biểu tượng, chưa có mask emissive vẽ riêng theo từng pose của 28 loài.
- Thân địch hiện đầy đủ trong vùng an toàn **4.3 + bán kính địch**, trong lúc pending attack/telegraph hoặc vừa tung đòn. Boss luôn ít nhất 80% độ sáng. Đạn, telegraph, loot, HUD không đi qua mặt nạ làm tối thân địch. Không có darkness rút HP, nhiên liệu hay aura miễn sát thương mới.
- High Readability giữ ambient combat ít nhất 0.24 và silhouette địch thường ít nhất 62%. Giảm hiệu ứng/chuyển động không xóa ánh sáng cần để chơi. Pause giữ clock/field ổn định.

## Blender, Unity và âm thanh

Đã gọi **Blender MCP** để tạo một scene riêng `MienCho_SharedDawn`, dựng ghế, khung cửa, thư, vật liệu, area lights và camera; render bằng Cycles. Không xóa scene ban đầu của Blender. Bốn ảnh xuất được nhập vào Unity:

- `Resources/MienCho/WaitingRoom.png`, `WithheldDawn.png`, `OpenDawn.png`: ba panel có ánh sáng khác nhau, dùng cho journal và ending. Trang bình minh ghép sprite hero/đội hình thật lên panel mới, thay ảnh tĩnh dễ khiến hiểu rằng bạn đồng hành trở lại thành đồ vật. Không coi ba ảnh là ba căn phòng 3D chơi được.
- `Resources/MienCho/EmptyChair.png`: sprite alpha dùng trong world làm ghế trống ngoại vi.
- Source chỉnh sửa: [MienCho_SharedDawn.blend](../Tools/Blender/MienCho_SharedDawn.blend).
- Ba sprite khám phá: `Resources/MienCho/Encounters/ReservedTable.png`, `BreathingPhone.png`, `LateReflection.png`. Source [MienCho_EncounterProps.blend](../Tools/Blender/MienCho_EncounterProps.blend), script tái tạo [create_waiting_encounters.py](../Tools/Blender/create_waiting_encounters.py); Cycles 32 samples với vật liệu/đèn tự dựng.

Unity MCP không có trong bộ công cụ của phiên này; tích hợp, import, biên dịch và chạy Windows thực hiện qua Unity Editor/Player cài tại máy. Ánh sáng runtime là shader/RenderTexture trong Unity; Blender đảm nhiệm scene và ánh sáng bake cho ảnh, không phải bằng chứng runtime dùng đèn Cycles.

Thêm hai motif nhạc tổng hợp bằng code: giai điệu bỏ lửng ở các shop 1/8/16 và câu kết ở bình minh. Volume theo cài đặt Music; Reduced Audio tắt lớp motif mới. Không có voice acting, soundtrack thu âm mới hoặc âm thanh bước chân giả gây nhầm với địch.

## Nghiệm thu và phạm vi

Kết quả máy tại [Build.txt](../PreviewExports/MienCho/Build.txt), [kiểm tra Editor](../PreviewExports/MienCho/Checks.txt). Ảnh và kiểm tra Windows thường/giảm hiệu ứng ở [Native](../PreviewExports/MienCho/Native/) và [Reduced](../PreviewExports/MienCho/Reduced/). Báo cáo gate cuối: [Gate_Summary.md](../PreviewExports/MienCho/Gate_Summary.md).

Gate truyện/shop qua **4683 kiểm tra** gồm phần mới và hồi quy; Windows có **25 ảnh mỗi chế độ** (21 ảnh truyện/ánh sáng và 4 ảnh cảnh shop), kiểm tra hai farewell, intro tự mở, tương tác cảnh shop và tải lại trang nhật ký từ đĩa. Bản chạy hồi quy hiện tại **0 lỗi / 0 cảnh báo**; gate tổng của đợt hồi 1 mới nằm ở phần cập nhật đầu tài liệu. Cả bốn lượt Player kết thúc mã 0, lưu tại [Capture_Runs.json](../PreviewExports/MienCho/Capture_Runs.json). Đây là kiểm tra theo fixture có giới hạn số quái; số đo frame time không thay cho benchmark mọi cấu hình hoặc playtest cân bằng. Toàn bộ chữ truyện để rà soát: [Story_Text.md](../PreviewExports/MienCho/Story_Text.md); nguồn art và hash: [Mien_Cho_Art_Provenance.json](Mien_Cho_Art_Provenance.json).

Bản Windows cập nhật nằm tại `Builds/HeroIdentity/ArcaneBrigade.exe`, dùng lại thư mục build hiện có để tránh thêm một bản sao hơn 1.5 GB trên ổ C. Tên thư mục không quyết định phiên bản nội dung; `PreviewExports/MienCho/Build.txt` xác định gate của bản này.

Ba hồi dài hạn đã được thu vào hành trình 20 wave này. Chưa có ba chiến dịch 20 wave riêng, 66 quest phân nhánh, những phòng 3D mới để tự đi lại, hoặc boss chiến đấu riêng cho Người Luôn Ở Đó. Các cảnh kinh dị chính được kể trong journal; scene runtime có ghế, ánh sáng, mắt thật, âm nhạc và ba vật thể khám phá trong shop. Không lấy số ảnh/gate animation của agent khác làm nghiệm thu cho các tính năng này. Kiểm tra hình ảnh không thay thế playtest dài hạn về nhịp đọc, cảm xúc hay độ dễ nhìn trên mọi màn hình.
