# Bộ tranh map và relic — 23/10/2026

Vẽ bằng `painted`, sau đó căn ô và dọn alpha theo chấp thuận của chủ dự án. Không dùng hình học code thay tranh. Prompt gốc map: [Map_Identity_Art_Prompt.txt](Map_Identity_Art_Prompt.txt); prompt thực tế và các lần sửa: [Map_Identity_Art_Generation.json](Map_Identity_Art_Generation.json), [Lone_Relics_Art_Generation.json](Lone_Relics_Art_Generation.json).

## Cửa và quái: 13 PNG

Thư mục: [MapEnemies](../Assets/ArcaneBrigade/Art/Playtest/MapEnemies/).

| Nhóm | File |
| --- | --- |
| Cửa hai trạng thái | Shop_ExitDoor.png |
| Phòng Chờ | Enemy_TicketMoth.png, Enemy_UmbrellaStalker.png, Enemy_SofaGlutton.png |
| Bưu Điện | Enemy_StampTick.png, Enemy_SealHound.png, Enemy_SackGolem.png |
| Hành Lang Đèn | Enemy_WickImp.png, Enemy_LampshadeJelly.png, Enemy_ChandelierCrawler.png |
| Kho Lưu Trữ | Enemy_ClipMite.png, Enemy_InkToad.png, Enemy_CabinetMimic.png |

Mỗi quái có 16 ô, 4 cột × 4 hàng; hai hàng thường, hai hàng đột biến. Đã sửa các sheet thiếu ô và các tư thế chớp mắt chưa rõ. Căn toàn bộ sprite bằng một hệ số chung trong từng sheet, giữ tỷ lệ hình, chung đáy ô và lề ít nhất 10%. Cửa có hai ô đóng/mở, cùng hệ số thu và cùng đáy. Không kéo giãn sprite.

Theo lựa chọn giữ canvas gốc của công cụ, atlas là **1254×1254**, cửa **1774×887**, không phải canvas 1536/2048 trong prompt ban đầu. Rect import chia theo kích thước thực tế, làm tròn ranh pixel. Metadata Unity đi kèm giữ GUID ổn định và chia ô; không có thay đổi cơ chế hoặc catalog trong task vẽ này.

Kiểm tra ảnh: [Map_Identity_Art_Verification.json](Map_Identity_Art_Verification.json). 13/13 RGBA, đủ ô có hình, bốn góc alpha 0 và lề ô đạt yêu cầu. Có xem hình để kiểm tra không có chữ và không bị cắt.

## Lone Line: 8 PNG

Thư mục: [LoneRelics](../Assets/ArcaneBrigade/Art/Playtest/LoneRelics/).

LoneLantern.png, EmptyChairs.png, SingleTicket.png, NightAlone.png, SelfPortrait.png, LastLight.png, IronResolve.png, SoloOverture.png.

Xuất **512×512 RGBA** theo yêu cầu bộ relic mới, một icon hoặc cụm vật dụng đúng mô tả trên mỗi file. Giữ màu tranh gốc, căn giữa, lề khoảng 10%; không thêm chữ hoặc nền. Khung chân dung được vẽ sửa để có đủ vòng treo. SoloOverture có ánh cầu vồng trên đũa và giấy. Kiểm tra: [Lone_Relics_Art_Verification.json](Lone_Relics_Art_Verification.json).

Các kiểm tra trên là kiểm tra file ảnh và bố cục, chưa phải nghiệm thu animation, giao diện hoặc gameplay chạy trong Unity. Task này không chạy build hoặc test gameplay.
