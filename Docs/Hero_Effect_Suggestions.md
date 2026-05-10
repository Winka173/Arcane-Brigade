# Đề xuất hiệu ứng riêng cho 36 hero — 24/10/2026

Đây là **đề xuất mỹ thuật**, chưa phải thay đổi đã triển khai. Đã đối chiếu `HeroKits.cs`, `HeroFlairVisual.cs`, `HeroSignatureVisual.cs` và `HeroAwakeningVisual.cs`, gồm đợt hiệu ứng ở commit `55c5a4cc` và phần kết chiêu mới ở `c08d2cee`. Runtime đã có token chờ, hiệu ứng va chạm theo hero, viền biến hình, kết chiêu và phản ứng của companion; đề xuất dưới đây tập trung vào chất liệu, trọng lượng và nhịp diễn xuất của những hiệu ứng đó.

## Hướng chung

- Mỗi hero cần một chuyển động nhận diện riêng. Độ đã đến từ **chuẩn bị → tác động rõ → dư âm ngắn**, thay vì thêm thật nhiều biểu tượng bay quanh thân.
- Điểm bắt đầu để thử: tăng bề dày lõi hiệu ứng chính khoảng **20–35%**, glow tại điểm chạm **15–25%**, giữ bụi/mảnh thứ cấp yếu hơn lõi. Đây là thông số đề xuất tương đối, chưa đo trên ảnh runtime. Không tăng đồng loạt opacity và số hạt.
- Tăng độ dày hình hiển thị bên trong vùng chiêu; không kéo dài tầm hoặc làm phần sáng trông như vùng gây damage lớn hơn thực tế. Thân, mặt, vũ khí, đạn địch và windup vẫn phải đọc được.
- Phe ta dùng hổ phách, kem, đồng, teal và sắc trăng; đỏ rượu chỉ là vật liệu. Ánh sáng phát đỏ nguy hiểm dành cho địch. Không khôi phục luồng gió dưới đồ lơ lửng, vòng tròn chung cho mọi hero hay lớp phủ toàn màn hình.
- Vật thể vật lý vẫn dùng tranh hợp theme. Ánh sáng, hơi nước, điện, mưa, phản chiếu và độ trong cần shader/mesh/hạt có diễn tiến, không thay bằng một icon phóng lớn. Tận dụng light field và shader đang dùng trong dự án; kiểm tra pipeline trước khi thêm một hệ lighting khác.
- Chiêu/buff chỉ sáng trên người thực sự được hưởng. Đồ trang trí do hiệu ứng tạo ra không được nhầm với pickup, vật cản, companion thật hay đạn địch.

## Từng hero

| Hero | Hiệu ứng nên thêm hoặc làm rõ | Hiệu ứng cũ nên tăng cường |
| --- | --- | --- |
| **Lantern Knight** | Hạt bụi chỉ hiện trong chùm sáng; vệt sáng lướt qua mặt sàn và viền đồng minh đúng lúc họ được buff. Khi hải đăng đổi hướng, lõi sáng đi trước, quầng mềm theo sau. | Tăng độ đầy của chùm sáng, lõi kem và mép hổ phách; phần sáng chạm mục tiêu dày hơn khoảng 25%. Đèn thật chiếu sáng, không thay bằng hình ngọn lửa khổng lồ. |
| **Postmaster** | Nhịp máy phân thư: các nòng giật lệch nhau, hơi mực thoát ngắn, giấy bị hút vào trước khi bắn; dấu đóng gắn với địa chỉ/mục tiêu thật. | Tăng lực nén khi đóng dấu, mảnh sáp và độ dày vệt bưu kiện. Giữ giấy nhỏ; không phủ cả đội bằng thư bay. |
| **Drowsy Seer** | Ba giấc mơ có mép bạc và độ trễ theo đường đi thật; lúc đổi chỗ, vị trí cũ khép thành nếp sáng rồi vị trí mới mở ra. | Làm trăng có khối, quầng mềm sau thân; vệt phép trăng dày hơn 20%, bản sao dịu và ít đặc hơn hero thật. |
| **Root Oracle** | Rễ đội mặt sàn lên, nứt theo nhánh, đâm rồi rút vào; cây bước bằng từng bó rễ thay vì trượt. | Tăng độ nặng của nhịp rễ trồi và mép vỏ cây; bụi đất thấp, tan nhanh, không che chân địch. |
| **Lone Jelly** | Chất gel kéo cổ chai rồi tách thành hai bản thể; các cơ quan được nhớ lại sáng nhẹ từ bên trong, phần gel hồi hình sau đòn. | Tăng squash/stretch và viền khúc xạ, không tăng kích thước thân. Bản sao dùng độ sáng thấp hơn hero để đọc được nhân vật chính. |
| **Glass Dice** | Xúc xắc có đổ mặt, nảy và dừng rõ; ba kết quả khớp nhau mới bung màn giấy bạc/đồng xu ngắn. | Jackpot mạnh hơn đòn thường bằng nhịp chờ và cú nảy, không bằng flash toàn màn; tăng độ dày dấu va chạm khoảng 25%. |
| **Tide Singer** | Sóng có đầu sóng cuộn, thân nước trong và bọt kéo sau; nước bắt ánh đèn khi đi ngang vùng sáng. | Tăng thể tích đầu sóng, giảm độ đặc phía sau; nốt nhạc hiện theo nhịp chạm/thưởng, không lơ lửng dày liên tục. |
| **Comet Runner** | Đường chạy để lại các điểm rơi thật; thiên thạch có vệt đến, chạm đất nén xuống rồi bật bụi sáng. | Vệt sao chổi dày ở đầu, tắt nhanh ở đuôi; điểm rơi rõ hơn khoảng 25%, không thêm vết nổ ngoài vùng đánh. |
| **Mirror Keeper** | Mặt gương phản môi trường thật ở mức đơn giản; khi bắt đạn, mặt gương lõm/nhăn một nhịp rồi phản hồi bằng vệt sáng bạc. | Tăng phản quang theo góc và cú giật lúc trả đạn; giảm mảnh kính bay dài vì dễ nhầm projectile. |
| **Treasure Acorn** | Mục tiêu truy nã có dấu đồng thau riêng; tay quay và buồng nạp chuyển động rõ, vỏ hạt nhỏ rơi ra khi bắn. | Đồng xu thưởng bật lên thành cụm ngắn rồi mờ, khác xu loot thật; tăng độ chắc của điểm trúng, tránh mưa xu kín màn. |
| **Verdant Matron** | Nụ hoa lớn dần theo trạng thái rồi nở nối tiếp từ các Sprout thực; nhựa cây chạy trong gân lá khi hồi phục. | Tăng độ bung cánh hoa và sắc sáng tại gân lá; hạt phấn thưa, vùng hồi phục không biến thành lớp xanh đặc. |
| **Copper Cauldron** | Hơi nước có khối, che khuất mềm và thoát đúng miệng van; nồi rung/giật theo nhịp piston, luồng xả cuối có đầu đặc và đuôi tan. | Tăng độ dày hơi xả và lực nén khi húc; ánh áp suất dùng kem/hổ phách, không đỏ ember của địch. |
| **China Marionette** | Các vết rạn chạy theo bề mặt sứ; vàng lỏng lấp dần từ vết thương, mối nối bật sáng đúng lúc dư chấn gây damage. | Làm đường vàng có bề dày và ánh kim; thân sứ giữ màu gốc, không trắng xóa trong ba giây bảo vệ. |
| **Soap Skater** | Màng xà phòng óng nhẹ theo góc; cua gấp để lại dải trượt cong, bọt căng trước khi vỡ, vùng băng có vệt xước. | Tăng chất màng và độ rõ cú vỡ thay vì tăng số bong bóng; băng chỉ bắt sáng trên các mảng nhỏ. |
| **Velvet Regent** | Lệnh phong tước truyền thành một nét vàng từ hero đến đồng minh thực sự được buff; áo choàng/viền của họ nhận một nhịp ánh sáng. | Nâng uy lực dấu phong tước và nhịp đội quân cùng đáp lại. Hero không đánh thường nên không gắn thêm đòn chém giả. |
| **Button Cavalier** | Đối thủ được chọn có dấu đối kiếm nhỏ; đầu kiếm thu sáng trong một nhịp rồi sáu đường đâm xuất hiện có trật tự, mỗi lần chạm có tia đồng. | Tăng độ sắc và độ dày lõi đường đâm; để khoảng nghỉ rất ngắn giữa các nhát, không vẽ sáu đường chồng thành đám rối. |
| **Night Maw** | Bóng tối bị kéo về miệng theo chiều nuốt, hàm có lực đóng và bụi mực bị nén; boss chỉ chịu dấu cắn đúng cơ chế, không biến mất như bị nuốt. | Làm rõ hình hàm bằng viền trăng/teal và cú khép; giảm màn khói phủ sau lưng để vẫn thấy địch. |
| **Thorn Treasurer** | Khi bị đánh, điểm va chạm truyền xung qua gai trên thân rồi bật ngược về phía kẻ tấn công; ngai mọc từng nhánh có sức căng. | Tăng lực bật của phản kích và độ nặng gai; tránh aura phóng liên tục lúc không có phản công thật. |
| **Citrus Marshal** | Đoàn tàu có móc nối co giãn, chao theo cua, tia đồng nhỏ xuất hiện đúng va chạm của các toa; dấu cam chỉ ở cú húc. | Tăng squash của đầu tàu, độ chắc của móc nối và âm va chạm. Không thêm đòn thường giả cho hero không dùng đánh thường. |
| **Ivory Sovereign** | Sắc lệnh mở ra thành nếp giấy, đường phán quyết đi như mép giấy dát vàng; hành quyết có một nét cắt dứt khoát ở mục tiêu đủ điều kiện. | Tăng độ sắc và tương phản ivory/đồng, làm ấn triện nặng hơn; không dùng cột sáng lớn che boss. |
| **Petal Citadel** | Các lớp cánh hoa mở lần lượt thành tường có độ cao; khi bắn, một khe cánh mở rồi khép, bóng cánh đổ cục bộ. | Tăng cảm giác tường dày và độ hồi khi nhận đòn; giữ khoảng trống để vẫn thấy thân và hướng hero. |
| **Patchwork Host** | Spotlight thật mềm hướng vào bạn diễn chính; đường chỉ nối ký hiệu đạo diễn với người diễn, ba bản sao bước ra theo các nếp màn. | Tăng độ tương phản của spotlight tại bạn diễn, không tăng sáng cả map; bản sao có viền chỉ riêng để phân biệt với companion thật. |
| **Archive Curator** | Trang sách gập mở thành sinh vật giấy; mực chạy qua nét vẽ trước khi bản ghi bước ra; bản sao được nhận diện bằng mép giấy. | Tăng chuyển động gập, bóng lớp giấy và hơi mực cục bộ. Không phủ toàn bộ sinh vật bằng màu tím phát sáng. |
| **Penny Broker** | Biên lai kéo căng rồi xé thành projectile; con dấu nện xuống, tiền lướt thành nét đồng về phía sổ khi thu nhập thật kích hoạt. | Tăng độ đàn hồi của giấy và cú đóng dấu; chỉ một nhịp tài chính rõ mỗi sự kiện, không bắn số/giấy vụn liên tục. |
| **Storm Fencer** | Điện chạy dọc vật liệu vũ khí, dồn ở mũi trước khi phóng; từng nhánh chain đi qua đúng thứ tự mục tiêu có thật. | Sét lõi kem, áo teal, nhánh phụ ngắn; tăng bề dày lõi khoảng 30%, thời gian sáng cực ngắn. Ngoài biến hình, nhấn trạng thái tích điện/va chạm thay vì đòn đánh chưa tồn tại. |
| **Tin Virtuoso** | Mỗi crit của hero là một nhịp chỉ huy nhìn thấy được; đồng minh đáp lại bằng vệt âm gắn đúng các phát bắn/đòn phản hồi. | Tăng độ đồng bộ nhịp, độ nảy của nhạc cụ và cụm âm tại impact; không thêm khuông nhạc che màn hình. |
| **Moon Prowler** | Viền trăng quét theo cú lách ra sau mục tiêu, một bóng ngắn ở vị trí cũ; nhật thực khép/mở cục bộ quanh cú đánh. | Tăng vệt cắt bạc và khoảnh khắc tái xuất, không tăng darkness toàn màn vì đang có hệ bóng tối của map. |
| **Supper Mortar** | Buồng nấu sôi, hơi bếp thoát đúng ống; món ăn có đà rơi, nén khi chạm và tách vụn theo loại món; món thứ sáu có nhịp phục vụ riêng. | Tăng độ nặng cú rơi và hơi nóng, giảm icon món ăn trôi lặp. Quầng hổ phách không được nhầm lửa địch. |
| **Lantern Weaver** | Sợi chỉ có lực căng/võng; nút thắt siết từ hai phía, ánh sáng chạy dọc đúng đường nối, sợi đứt bung thành vài xơ nhỏ. | Tăng độ dày đường chỉ khoảng 25% và điểm sáng tại nút; giới hạn số liên kết sáng cùng lúc để không thành mạng phủ map. |
| **Hourglass Stag** | Cát mắc giữa không trung tại vùng ngưng thời gian, kim/răng đồng khựng một nhịp; lúc thu nợ, các hạt quay ngược rồi rơi dứt khoát. | Tăng tương phản giữa đứng im và bật lại, độ dày nét thu nợ; không dùng motion blur hoặc khựng gameplay toàn cục giả. |
| **Rain Archivist** | Mưa được soi sáng khi đi qua đèn và đội hình; giọt gõ trên giấy/mặt sàn, mực lan theo vết trang; sét chỉ xuất hiện tại điểm đánh thật. | Tăng chiều sâu bằng lớp giọt trước/sau thưa và vệt nước tại impact. Không tăng mật độ mưa toàn camera khiến đạn địch biến mất. |
| **Kite Admiral** | Diều nghiêng theo hướng bay và tốc độ bạn được hộ tống; dây có độ chùng/căng, đầu dây phát một nhịp khi shot thật tách đôi. | Tăng diễn xuất thân diều và độ rõ khoảnh khắc tách; giữ diều nhỏ, không thêm năm cột sáng trên đầu đồng minh. |
| **Sundae Duchess** | Viên kem rơi có quán tính, bẹt ra khi chạm; phần sốt và tinh thể đóng băng tách chất liệu, tan từ mép vào trong. | Tăng squash và cảm giác lạnh tại điểm chạm; giảm ánh trắng của toàn vũng để vẫn thấy quái bên dưới. |
| **Bonsai Clocksmith** | Bánh răng làm cành cây khẽ xoay; kim quét có bóng/ánh đồng, lá mở theo hướng kim vừa đi qua; đồng minh trong vườn sáng đúng buff. | Tăng trọng lượng nhịp kim và ánh kim trên cành; vùng vườn dùng chi tiết đất/lá thưa, không là đĩa xanh đặc. |
| **Juggling Jester** | Bóng/chùy có trọng lượng và nhịp tung-bắt; năm cú rơi lệch nhịp có chủ ý, mỗi cú có nén đất và vài mảnh vải. | Tăng độ nảy sau impact và trail nhung/đồng. Giữ là cư dân Miền Chờ biểu diễn trong đêm, không biến cả map thành sân khấu neon. |
| **Bell Diver** | Thành chuông có phản quang đồng, không khí bị đẩy ra lúc húc; probe chạy thành nét sonar ngắn từ vị trí thật của phao/chuông. | Tăng khối nước/không khí của cú húc và cú nổ sâu; sóng âm là cung ngắn có hướng, tránh các vòng đồng tâm liên tục. |

## Thứ tự đáng làm trước

1. **Lantern Knight, Copper Cauldron, Tide Singer, Storm Fencer, Rain Archivist, Bell Diver:** chất liệu ánh sáng, hơi, nước và điện sẽ tạo khác biệt lớn nhất so với token/hình phẳng.
2. **China Marionette, Mirror Keeper, Lone Jelly, Lantern Weaver, Bonsai Clocksmith:** cải thiện phản chiếu, khúc xạ, bề mặt và độ căng để tranh có sức sống.
3. **Các hero còn lại:** chuẩn hóa anticipation, nhịp impact và kết chiêu theo từng cơ chế, rồi tăng lực có chọn lọc.

Nghiệm thu bằng clip cùng góc camera cho solo và đội đông, một mục tiêu và đàn quái, nền sáng và tối, chế độ thường và giảm hiệu ứng. Phải xem cả lúc nhiều hero/đồng minh đánh chồng lên nhau; một ảnh đẹp đứng riêng chưa chứng minh hiệu ứng đọc được khi chơi.
