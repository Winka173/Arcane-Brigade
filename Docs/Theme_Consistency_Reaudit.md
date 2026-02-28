# Rà soát theme lần hai — 17/10/2026

Snapshot: `c08e0b2c`, sau [đợt sửa theme](Theme_Consistency_Fixes.md). Rà source vừa sửa và các đường gọi liên quan, đối chiếu canon Hồi 1; xem trực tiếp 7 PNG mới của Keeper/icon. **Không chạy build, test, player hoặc đo hiệu năng** theo yêu cầu trước. Đây là phát hiện bằng đọc code và xem asset, không phải lỗi đã tái hiện trên Windows. Không sửa vũ khí/mechanic hoặc gameplay trong lượt rà này.

Kết quả: **còn 10 điểm cần xử lý**, có cả lỗi ở phần trình bày vừa thêm. Chưa thể coi đợt trước là đã hoàn thiện toàn bộ.

## 1. P1 — Cảnh ghế có thể giữ nhầm nhân vật sau mua/reroll

`ActOneRestingScenes` chỉ tìm lại Hob/Tally khi biến tham chiếu bằng null, và chỉ xóa cache khi đổi wave. Trong khi mỗi giao dịch shop gọi `Begin(true)`, đẩy các `UnitVisual` vào pool rồi lấy ra `Bind` lại cho các đơn vị. Một renderer còn tồn tại không bảo đảm vẫn là Hob/Tally.

Hệ quả suy ra từ đường gọi: ở shop 17, sau mua/reroll, cảnh có thể đặt hero hoặc bạn khác lên ghế, đổi hình thân thành Hob, hoặc gắn tờ giấy vào nhầm người. Đây không phải thay đổi simulation nhưng có thể làm sai cả hiển thị người chơi.

Bằng chứng: [ActOneRestingScenes.cs:76](../Assets/ArcaneBrigade/Runtime/Presentation/ActOneRestingScenes.cs#L76), [giao dịch và Begin](../Assets/ArcaneBrigade/Runtime/Core/ArenaController.cs#L442), [pool](../Assets/ArcaneBrigade/Runtime/Core/ArenaController.cs#L583), [Bind lại](../Assets/ArcaneBrigade/Runtime/Core/ArenaController.cs#L630).

Hướng sửa: xác nhận lại danh tính/đơn vị và thế hệ simulation, bỏ tham chiếu đã bị rebind; cảnh chỉ được sở hữu pose của đúng diễn viên truyện.

## 2. P1 — Resume ngay sau cứu Hob có thể mở lại phòng đời cũ

Kéo Hob thành công lưu `HobRescued = 1`, nhưng `WaitingCampaign.Room` vẫn là 2 cho tới khi bấm trở về shop. Nếu thoát/crash trong khoảng đó, `ArenaController` chuyển vào WaitingRoom khi resume; `WaitingRoomController` tính `rescue = room == 2 && HobRescueNeeded`, lúc này thành false. Người chơi đi vào nhánh Perfect Room cũ có ba record/lựa chọn, thay vì trạng thái sau cứu Hob. Nhánh đó vừa được ghi là retired trong báo cáo.

Bằng chứng: [chọn chế độ phòng](../Assets/ArcaneBrigade/Runtime/Presentation/WaitingRoomController.cs#L63), [lưu sau kéo](../Assets/ArcaneBrigade/Runtime/Presentation/WaitingRoomController.cs#L158), [chỉ xóa Room khi rời phòng](../Assets/ArcaneBrigade/Runtime/Presentation/WaitingRoomController.cs#L239), [resume](../Assets/ArcaneBrigade/Runtime/Core/ArenaController.cs#L123).

Hướng sửa: phân biệt loại phòng với tiến độ cứu hộ; khôi phục cảnh hậu cứu hộ hoặc về shop an toàn, không rơi sang nội dung retired. Giữ nguyên kết quả cứu đã lưu.

## 3. P1 — Spoiler vẫn lọt qua tiểu sử và bảng đội hình

Whisper/merchant đã có kiểm tra bằng chứng, nhưng `CreatureLives.Arc` vẫn chỉ nhận `id, wave`, không nhận phase, trang đã đọc hoặc cờ cứu Hob. Cả journal Lives và bảng đội hình dùng đường này.

- Ngay khi wave 16 bắt đầu, tiểu sử Hob đã nói cả đoàn kéo Hob khỏi ghế rồi Hob chọn ghế đẩu, trước phòng cứu ở shop 16 và cảnh shop 17.
- Ở wave 12, tiểu sử Postscript đã chép chi tiết thư thứ Năm trước khi đọc cảnh mở thư.
- Ở wave 17, tiểu sử Tally đã nói từng biến mất một đêm, dù việc biến mất ở shop 18.

Bằng chứng: [nội dung](../Assets/ArcaneBrigade/Runtime/Data/CreatureLives.cs#L78), [gate chỉ theo wave](../Assets/ArcaneBrigade/Runtime/Data/CreatureLives.cs#L108), [journal](../Assets/ArcaneBrigade/Runtime/Presentation/MienChoPresentation.cs#L340), [bảng đội](../Assets/ArcaneBrigade/Runtime/UI/RunBuildReadModel.cs#L70).

Hướng sửa: một nguồn điều kiện kiến thức dùng chung cho tiểu sử, banter, merchant và journal; riêng hành động cứu Hob phải kiểm tra kết quả đã xảy ra.

## 4. P2 — Vài PNG Keeper mới còn dính mảnh của ô bên cạnh

**Đã thấy trực tiếp trên asset**, không chỉ suy đoán từ code:

- [Keeper_Kneel.png](../Assets/ArcaneBrigade/Resources/MienCho/ActOne/Keeper_Kneel.png): có mảnh hình sát cạnh phải.
- [Keeper_Whole_Body.png](../Assets/ArcaneBrigade/Resources/MienCho/ActOne/Keeper_Whole_Body.png): có mảnh rời ở dưới cạnh phải.
- [Keeper_Carpet_Intact.png](../Assets/ArcaneBrigade/Resources/MienCho/ActOne/Keeper_Carpet_Intact.png): có phần ngón tay ở cạnh trái.
- [Keeper_Hand_Broken.png](../Assets/ArcaneBrigade/Resources/MienCho/ActOne/Keeper_Hand_Broken.png): có mẩu viền thảm ở góc phải dưới.

Thân modular mới không có những phần in sẵn cần tháo, nhưng điều đó không làm sạch các PNG khác. Mảnh thừa còn làm sai vùng mực/kích thước, bóng và framing. Script tách atlas đang gán cả các cụm mực rời cho nhóm gần nhất; cần sửa cách phân nhóm hoặc ranh giới trước khi xuất lại, không chỉ vá PNG rồi để script tái tạo lỗi. Xem [slicer](../Tools/Art/slice_theme_consistency.py).

## 5. P2 — Keeper có art quỳ nhưng chưa dùng đúng trạng thái; pose bộ phận còn thiếu

`CustomerFace` chọn `Keeper_Kneel` khi **không còn sống**, còn lúc sống luôn chọn BodyShell. Trong khi thử thách có trạng thái `KneelUntil` kéo dài 7 giây sau khi vỡ đủ ba bộ phận. Ảnh quỳ chưa biểu đạt đúng cửa sổ này.

Ngoài ra, nhánh thay art bộ phận ghi đè ảnh charged/release bằng cùng một ảnh intact; bàn tay và thảm khi vận chuyển cũng dùng ba tham chiếu cùng một sprite. Có chuyển vị trí/nghiêng/co giãn, nhưng chưa có chuỗi pose painted tương ứng. Ảnh broken còn được dùng làm scar, nên lúc mảnh bay ra có thể đồng thời thấy một bản broken khác tại gốc.

Bằng chứng: [chọn thân](../Assets/ArcaneBrigade/Runtime/Data/ActOneArtwork.cs#L50), [trạng thái quỳ sẵn có](../Assets/ArcaneBrigade/Runtime/Core/BossPartRules.cs#L147), [ghi đè pose](../Assets/ArcaneBrigade/Runtime/Presentation/BossAnatomyVisual.cs#L62), [vận chuyển](../Assets/ArcaneBrigade/Runtime/Presentation/StageTransfer.cs#L48).

Hướng sửa nằm ở trình bày: đọc trạng thái hiện có, bổ sung pose/socket phù hợp. Không đổi moveset, vùng đánh hoặc thời gian quỳ.

## 6. P2 — Vật chứng hết wave vẫn có thể chiếm chỗ vô hình trong layout

Các ID `story:rest:12/15/17/18` được đăng ký một lần qua `ShopLayout.Place`. Khi hết wave, cảnh ẩn sprite nhưng không hủy đăng ký. Dictionary/order của layout không xóa những prop này khi đổi shop; `Plan` vẫn duyệt chúng và thêm footprint vào danh sách đã chiếm.

Hệ quả: vật chứng không còn nhìn thấy vẫn có thể đẩy đồ khác đi hoặc làm đạo cụ sau không tìm được chỗ. Khi đi tiếp nhiều shop, số footprint thừa tăng.

Bằng chứng: [ID theo wave](../Assets/ArcaneBrigade/Runtime/Presentation/ActOneRestingScenes.cs#L63), [đăng ký](../Assets/ArcaneBrigade/Runtime/Presentation/ShopLayout.cs#L60), [đổi room](../Assets/ArcaneBrigade/Runtime/Presentation/ShopLayout.cs#L85), [duyệt tất cả prop](../Assets/ArcaneBrigade/Runtime/Presentation/ShopLayout.cs#L166).

Hướng sửa: có vòng đời đăng ký theo shop/cảnh và loại footprint đã hết hiệu lực, giữ các vị trí hiện hành ổn định khi mua/reroll.

## 7. P2 — Điện thoại mới chưa tham gia tránh chồng đồ cảnh

Điện thoại cố định tại `(-3.15, -4.65)` và có ShopFocus để chọn tương tác, nhưng không gọi `ShopLayout.Place` và không nằm trong nhóm `Foreign` được reserve. ShopFocus chỉ giải quyết ai nhận phím, không giải quyết sprite nằm đè nhau. Các hệ bố trí khác chưa biết phải chừa vùng điện thoại. Chưa có ảnh player chứng minh nó đang chồng trong mọi trường hợp.

Bằng chứng: [vị trí/hiển thị điện thoại](../Assets/ArcaneBrigade/Runtime/Presentation/WaitingShopEncounter.cs#L11), [nhóm được reserve](../Assets/ArcaneBrigade/Runtime/Presentation/ShopLayout.cs#L120).

## 8. P2 — Vật chứng mới chưa có click trực tiếp lên hình; điện thoại lệch nút tay cầm

`ActOneRestingScenes.OnGUI` chỉ bắt click trong panel ở đáy màn hình; điện thoại cũng chỉ có nút LIFT THE RECEIVER trong panel. Chưa có vùng click gắn hình vật thể như tủ journal. Điện thoại nhận Space/R/X, còn các vật chứng mới nhận Space/E/A. Nút A thông thường không mở điện thoại khi đứng tại nguồn focus của nó.

Bằng chứng: [panel vật chứng](../Assets/ArcaneBrigade/Runtime/Presentation/ActOneRestingScenes.cs#L123), [phím điện thoại](../Assets/ArcaneBrigade/Runtime/Presentation/WaitingShopEncounter.cs#L127), [nút panel](../Assets/ArcaneBrigade/Runtime/Presentation/WaitingShopEncounter.cs#L174).

Hướng sửa: vùng click đi theo bounds hình đang lơ lửng; cùng hành động xác nhận cho Space/E/A, giữ một chủ sở hữu input.

## 9. P2 — Chuỗi ghé thăm Customer vẫn chủ yếu được kể bằng journal

Hiện scene đặt Customer trên map chỉ có shop 8/12/13. Canon còn các lần trả tiền ở shop 2/4/7/9/16/19; cảnh đứng ngoài cửa sổ ở 7 và giao dịch thất bại trước ngọn đèn cuối ở 19 có ý nghĩa chính. Vật trên ghế đã đổi theo lịch, nhưng không đồng nghĩa tất cả các hành động đó đã được diễn. Tiết lộ vẫn có trong văn bản, không phải nội dung bị mất hoàn toàn.

Bằng chứng: [điều kiện dựng diễn viên](../Assets/ArcaneBrigade/Runtime/Presentation/ActOneWorldArt.cs#L60), [canon và lịch](Mien_Cho_Act_One_Story.md#L162).

## 10. P2 — Ending có thể nói ta đi một mình dù bốn bạn truyện đang hiện diện

`NightmareDialogue.Ending` chọn câu “Even alone, you carried enough light…” chỉ vì `State.Roster.Count == 0`. Bốn diễn viên truyện không nằm trong roster thường, và đợt vừa rồi đã thêm họ vào đoàn bình minh. Lượt không mua bạn thường vẫn có thể hiện cả bốn người rồi được kể là một mình.

Bằng chứng: [lời ending](../Assets/ArcaneBrigade/Runtime/Presentation/NightmareDialogue.cs#L171), [cast truyện](../Assets/ArcaneBrigade/Runtime/Presentation/EndingTravellers.cs#L11).

## Phần đã có và giới hạn còn lại

- Nguồn sáng không còn bỏ mọi người sau nguồn thứ 16; batching đã nối vào shader. Không kết luận hiệu năng tốt ở đội rất đông khi chưa đo.
- Ba icon mới đã xem (`Projectiles`, `MoveSpeed`, `EnemyCount`) có ngôn ngữ hình nhất quán; chưa xem đủ 49 icon ở 24 px trong player. Không cần vẽ lại chúng chỉ vì bộ Keeper có lỗi cắt.
- Thân hero/companion mới chỉ được hạ bão hòa chung, chưa vẽ lại nếp mực và vật liệu cho mọi sprite cũ. Đây vẫn là giới hạn đồng nhất mỹ thuật, không phải 97 bộ art đã được thay.
- Chưa có nghiệm thu Windows mới cho bộ 70 PNG, đội đông, light batching, pause/resume hoặc bản phối nhạc Bedside. Các số gate cũ không thay thế việc này.
- Vũ khí/mechanic và Hồi 2–3 vẫn ngoài phạm vi. Không tính chúng là phần thiếu của lượt rà này.

Ưu tiên: **cache diễn viên + resume phòng + gate kiến thức → làm sạch atlas → pose Keeper + vòng đời layout → thao tác vật thể + cảnh Customer + câu ending**.
