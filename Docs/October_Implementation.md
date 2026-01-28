# Đợt mở rộng theo prompt/prompt.txt

## Nội dung đã nối vào game

Catalog thực tế hiện có **31 playable, 66 loài đồng minh, 170 relic, 29 bond và 104 achievement nhật ký mới**. Trong đó có đối ứng cho 28 Dream Seekers, 60 loài companion, 76 relic và 19 bond tham khảo; các biến thể sao dùng chung loài. Giữ nội dung riêng đã có, thêm 3 playable, 3 companion, 4 relic và 2 bond nguyên bản. Có 21 công thức altar mới, tổng cộng 25 công thức (21 mới + 4 cũ; đính chính số 29 trong báo cáo trước).

Đây là bản chuyển thể cơ chế sang chiến đấu, vũ khí, giá và nhịp wave của Arcane Brigade. Chỉ số và một số nguyên liệu công thức được thiết kế lại; không phải bản sao từng giá trị của game tham khảo. Mỗi đối ứng có tên, hình và mô tả riêng. Các tên tham khảo nằm trong metadata và bảng đối chiếu để kiểm tra độ phủ.

| Yêu cầu | Kết quả trong game |
| --- | --- |
| Bổ sung playable, companion, relic, bond | Đối chiếu từng loài/từng vật phẩm; bảng xuất trực tiếp từ catalog bên dưới. 21 playable mới có các pose vẽ riêng và animation chuyển động/tấn công, cùng 4 trạng thái biểu cảm. 19 companion mới có pose di chuyển và tấn công riêng. |
| Nội dung độc quyền | Lantern Weaver dệt đòn thứ tư; Hourglass Stag tạo vòng thời gian; Rain Archivist hồi máu/đáp trả khi mở rương. Teacup Cricket, Ribbon Otter, Spindle Wren; Lantern Guild và Rain Script; 4 relic mới. |
| Ghép sao | Mỗi lần ghép chọn một vũ khí thực tế của 3 nguyên liệu, giữ nguyên vũ khí đó. Hai đồng minh 2 sao có thể mua mảnh thứ ba và ghép ngay cả khi đội đã đủ 20. |
| Companion đặc biệt | Nursery Sprout, Pony có người cưỡi thật trong simulation, quà chiêu mộ từ Lisa/Bella/Jake, thưởng Roger/Daisy/Nia, đồng minh chỉ có từ quà, altar và thưởng ẩn. Wise/Capped hấp thụ ký ức thay vì thêm cơ thể. |
| Nursery | Ba Little Sprout đổi thành một bạn 2 sao ngẫu nhiên hợp theme và một Grand Sprout 2 sao; mỗi phần thưởng giữ một vũ khí nguyên liệu, tuân giới hạn 20 và không trùng biến thể. |
| Shop theo playable | Lọc cả shop thường và đặc biệt theo theme của hero; relic dựa trên loại và hiệu ứng có ích. Một số vật phẩm cơ bản dùng chung. Belle/Patchwork Host là ngoại lệ chủ ý với danh mục rộng. |
| Đồng minh cấp cao | Đội dưới 7 người từ wave 4 có thể gặp cấp cao ở cả hai shop. Đội tối đa 3 người từ wave 10 có cơ hội 3 sao. Ưu tiên mảnh đúng cấp để ghép. |
| Achievement | 104 mục với điều kiện rõ ràng, tiến độ, lọc chưa hoàn thành; lưu cùng profile. Giữ hệ thống kỷ lục cũ. |
| Description và thoại | Mô tả cơ chế, số cụ thể, đổi chác; tooltips hiện số sau compile. Thoại riêng cho 31 hero, thoại theo chất liệu/tính cách đồng minh, thêm lời merchant. Có khoảng nghỉ để tránh nói liên tục. |
| Đồ vật lơ lửng | Neo chân theo vùng mực thật của sprite, bóng tiếp xúc theo nền; đồ trên quầy bám mặt đỡ. Props sàn không bob/rock liên tục. |
| UI | Chọn mode thành card ngắn có mô tả riêng; mô tả hero cuộn; pause có padding; owned relic có dải rarity, icon và khung mô tả; bảng thưởng rainbow có mô tả đầy đủ và nút nhận. |
| Độ khó | Thêm Beyond the Lamp sau 5 mức cũ; mở bằng chiến thắng mức trước. |
| Cửa và rương | Cửa vẽ mới 4 pose mở khi đến gần, chân cửa luôn bám ngưỡng; rương có quầng vàng và glint, giảm hiệu ứng theo settings. |
| Chớp người chơi/đồng minh | Không flash khi bị đánh hay tấn công. Companion bất tử, không mất HP. Vẫn có động tác phản ứng. |
| Vàng, giá, quân số | Giảm phần vàng theo wave, giá tăng rõ hơn từ wave 8; tỉ lệ chiêu mộ giảm dần từ 10 tới 20, vẫn ưu tiên mảnh ghép; giới hạn cơ thể mới 20. |
| Cân bằng late | Giảm damage, reach, attack speed; lợi ích chỉ số lớn giảm dần. Melee rộng chịu giảm damage, tầm tối đa 3.4. Ranged tối đa 8; đạn giới hạn khoảng đi. |
| Boss thường | Thân nhỏ hơn 20%, bộ phận bù scale để giữ kích thước. Không bị knockback. Thêm lao có báo trước và 6 mẫu đạn. |
| Outside Keeper | Máu nguồn x20, damage vũ khí x2.8, contact x2.5, thân x1.75; art lớn 4 pose mới. Giữ cơ chế chữ ký hiện có. |
| Tất cả boss | Cả 10 boss thường, Keeper và Last Customer có spiral, fan, vòng có khe, cross, hook và rain có khoảng thoát/báo trước. Last Customer giữ 6 chuỗi đòn riêng và phần kết. |
| Ác mộng wave 11+ | Nền thảm ác mộng vẽ mới và ba loài mắt ở rìa; props giữ độ đục, khu vực trung tâm được để trống. |
| Rarity | Viền vòng phân đoạn, ánh nền nhẹ, hạt chuyển động; giảm chuyển động/flash theo cài đặt. |
| Wave và endless | Wave thường 45 giây. Từ wave 20 endless có thêm hệ số HP 1.16 và damage 1.075 lũy tiến mỗi wave, ngoài scaling sẵn có. Boss/sự kiện vẫn có điều kiện hoàn thành riêng. |
| Ngoại hình lên sao | Cấp 2/3 chỉ tăng thân; bỏ phụ kiện tier và phụ kiện hình thái trên người. Các chuỗi đòn build vẫn hoạt động. |

## Các giá trị cân bằng chính

- Late chuyển dần từ wave 8 đến wave 20: damage x1 → x0.83, reach x1 → x0.86, cooldown cơ bản x1.12 → x1.35; cooldown cuối tối thiểu 0.28 giây.
- Bonus attack speed trên 60% chỉ nhận thêm 12% phần vượt; hệ số cuối tối đa 2.2. Range bonus trên 50% chỉ nhận thêm 15% phần vượt, hệ số tối đa 1.8.
- Tầm tăng từ sao còn 9% mỗi bậc. Thân sao 2/3 vẫn lớn hơn để nhận biết.
- Máu tối đa áp dụng lợi ích giảm dần trên 250; trần chuyển từ 2000 xuống 600 về late. Armor compile tối đa 2.5, tốc di chuyển 10, projectiles 10. Các hiệu ứng đặc trưng hero vẫn có ngoại lệ rõ ràng, như Queen 12 HP.
- Quân thường HP x1.1, damage x1.08; push mỗi lần tối đa 0.45, nhân kháng. Boss push bằng 0 để không thể khóa boss bằng đội đông.
- Xác suất giữ ứng viên loài mới giảm 75% → 2% khi đội 10 → 20; xác suất giữ ô recruit giảm 100% → 32%. Đây là trọng số lọc ứng viên, không phải xác suất cuối của mọi ô shop.
- Vàng thu nhập wave: phần tăng theo wave còn `floor(wave * 1.5)` thay cho `wave * 2`. Các nguồn vàng riêng vẫn có hiệu lực.
- Endless có giới hạn số mũ kỹ thuật 200 để tránh tràn số ở lượt chạy rất dài.

## Bảng kiểm tra nội dung

Các bảng được `OctoberAudit.Export()` xuất từ dữ liệu Unity thực, không nhập tay:

- [Playable](../PreviewExports/October/Heroes.tsv)
- [Companion, cơ chế và nguồn nhận](../PreviewExports/October/Companions.tsv)
- [Relic và hiệu ứng](../PreviewExports/October/Relics.tsv)
- [Bond](../PreviewExports/October/Bonds.tsv)
- [Công thức altar](../PreviewExports/October/Recipes.tsv)
- [104 achievement](../PreviewExports/October/Achievements.tsv)

Nguồn đối chiếu: [Dream Seekers](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Dream_Seekers), [Companions](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Companions), [Relics](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Relics), [Achievements](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade_%28Achievement%29). Dữ liệu trang đã lưu trong Library để nghiên cứu; các bảng trên chỉ giữ tên đối chiếu và thiết kế/mô tả tự viết. Achievement được thiết kế lại thành mục tiêu phù hợp game, không chép nguyên văn danh sách nguồn.

## Art và kiểm tra

25 atlas/texture mới nằm trong `Assets/ArcaneBrigade/Art/October`. Tạo bằng công cụ image generation, import/slice bằng Unity, giữ nền trong suốt cho nhân vật. [Prompt và nguồn từng asset](October_Art_Provenance.json). Animation kết hợp pose vẽ riêng với hệ thống chuyển động, diễn xuất và vũ khí của game; không dùng asset studio.

Lệnh Unity: `ArcaneBrigade.Editor.OctoberContentBuilder.BuildWindows` xây catalog, chạy các bộ kiểm tra, xuất bảng nội dung và build Windows. `OctoberContentBuilder.Regressions` chạy riêng các kiểm tra. Các kết quả nằm trong `PreviewExports/October` và log `Library/October-FinalBuild.log`.

Kiểm tra bao gồm 31 hero chạy combat và save round trip, shop nhiều seed/theming hai loại shop, quà và ghép sao, mọi công thức altar mới, party cap, HP cost, companion bất tử, 12 boss thực hiện đủ 6 mẫu đạn; hồi quy secret events, nightmare, farewell. Bản Windows được chụp trực tiếp ở 1920×1080 và 1280×720, cả chế độ thường và giảm hiệu ứng. Capture dùng profile tạm riêng.

Kết quả cuối: **2.478 kiểm tra logic/hồi quy đạt; 46 kiểm tra Windows thường và 46 kiểm tra Windows giảm hiệu ứng đạt. Build Windows thành công, 0 lỗi, 0 cảnh báo.** [Báo cáo máy đọc được](../PreviewExports/October/Verification.json), [ảnh và hướng dẫn kiểm tra](../PreviewExports/October/README.md).

Build: `Builds/FullRun/ArcaneBrigade.exe`. Ảnh kiểm tra: `PreviewExports/October/Native` và `NativeReduced`. Các kiểm tra tự động xác nhận cơ chế và khả năng chạy; độ khó và cảm giác chơi dài hạn vẫn cần phản hồi từ chơi thực tế.
