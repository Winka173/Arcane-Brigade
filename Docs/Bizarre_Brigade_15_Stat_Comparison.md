# Bizarre Brigade 1.5+ — kiểm kê chỉ số và đối chiếu K9

Ngày đối chiếu: 2026-10-14. Đây là kiểm kê các nhóm chỉ số công khai tìm được, **không phải tuyên bố đã trích xuất toàn bộ enum/công thức nội bộ**. Không dùng số của Arcane để giả định công thức Bizarre Brigade.

## Nguồn và độ chắc chắn

- **V — footage 1.5:** [Belle, 28-01-2025](https://www.youtube.com/watch?v=70OufN07abE), [Boo Fortress, 30-01-2025](https://www.youtube.com/watch?v=tqvQPCo51xc). Panel của footage Fortress khoảng 492 giây là bằng chứng chính về tên/nhóm chỉ số. Frame kiểm tra cục bộ: `Logs/K9-reference-stats.png` và `Logs/K9-reference-scroll.png`; không đưa tài sản tham chiếu vào game.
- **S — nguồn cộng đồng, có thể trộn phiên bản:** chỉ đối chiếu các trích đoạn được lập chỉ mục của [Companions](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Companions), [Relics](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Relics), [Dream Seekers](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Dream_Seekers), [Achievements](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade_(Achievement)). Trang đầy đủ bị chặn khi truy cập; không nhận là đã đọc hết. Ba tên dưới panel và nhóm thuộc tính phụ chỉ có mức S trong lượt audit này.
- [Game8, hướng dẫn cập nhật 28-01-2025](https://game8.co/games/Zenless-Zone-Zero/archives/475381) dùng để đối chiếu cấu trúc gameplay, không làm bằng chứng công thức.
- **A — source Arcane hiện tại:** `RunModifierRules`, `RunSession`, `CombatSimulation`, `RunBuildReadModel`, `RunMeta`, weapon definitions. Các trạng thái sau được đối chiếu code, không suy ra chỉ từ tên UI.

## 17 nhóm chỉ số cốt lõi tìm được

Tên Việt là diễn giải để dễ so sánh, không phải bản chép UI. V/S chỉ mức xác nhận phía Bizarre Brigade. “Có” không có nghĩa cùng đơn vị, cap hoặc công thức.

| Nhóm | BB | Arcane K9 | Khác biệt cần biết |
| --- | --- | --- | --- |
| Máu tối đa | V | Có: Health | Mức máu tối thiểu 1; buff/debuff đồng minh đã tính thật. |
| Hồi máu | V | Có: Heal + nội tại riêng | Heal của ta là máu/giây; nội tại từng đơn vị kích hoạt mỗi 4 giây. Không khẳng định BB dùng cùng chu kỳ. |
| Số đạn | V | Có: Projectiles | Có scope thủ lĩnh/đồng minh, cấp sao thêm đạn; một số signature có luật riêng. |
| Hệ số điểm | V | Chưa có modifier build | Có điểm thật và hệ số độ khó/challenge, không có stat mua được để nhân điểm. |
| Sát thương va chạm gây ra | V | Chưa có | Địch chạm gây damage vào người chơi **không tương đương** build va chạm gây damage ra ngoài. |
| Tầm tấn công | V | Có thuộc tính vũ khí, chưa có stat tổng | Cận chiến tăng tầm theo cấp; item/đồng minh chưa tăng tầm đánh tổng quát. Roam là tầm đi lại, không phải tầm đánh. |
| Tỉ lệ chí mạng | V | Có: Crit | Xác suất của ta clamp 0–100%; chưa xác minh quy tắc vượt 100% của BB 1.5. |
| Tốc độ di chuyển | V | Có: MoveSpeed | Ta cộng hệ số lên tốc độ gốc, có sàn; không cùng đơn vị số với panel BB. |
| Giáp | V | Có: Armor | Ta dùng giảm damage `1/(1+max(0,armor))`; giáp âm chưa làm damage nhận tăng. Không suy ra công thức BB từ một screenshot. |
| Sát thương cận chiến cộng thẳng | V | Chưa có stat riêng | Có damage vũ khí và buff %, chưa có bonus cố định chỉ dành cho cận chiến. |
| Sát thương tầm xa cộng thẳng | V | Chưa có stat riêng | Tương tự; không gọi CompanionDamage là ranged damage. |
| Sát thương bổ sung theo % | V | Có tương đương: Damage | Có thêm ChampionDamage/CompanionDamage và scope; các hệ số không bảo đảm đúng công thức BB. |
| Tìm kho báu | V | Có một phần: TreasureDetection | Ta rút ngắn khoảng xuất hiện rương, chưa tăng chất lượng rương theo stat này. |
| May mắn | V/S | Có một phần: Luck | Ta tác động loot/trứng/tim/rương, **không** tác động roll shop hoặc RNG damage. Không nhận là trùng toàn bộ tác dụng BB. |
| Né tránh xác suất | S | Chưa có | Né bằng di chuyển có; chưa có roll miễn damage. Không tự thêm i-frame sau mọi đòn. |
| Tốc độ đánh | S | Có: AttackSpeed | Ta chia cooldown theo modifier, có thêm scope và bonus đồng minh; không khẳng định cap hay thang số BB. |
| Thu nhập | S | Có nền kinh tế, chưa có stat tổng | Income cố định trong catalog + nội tại tìm vàng + lãi; chưa có modifier Income theo build. |

**Kết quả:** 8 nhóm có cơ chế tương đương nền tảng (máu, hồi máu, số đạn, chí mạng, tốc độ, giáp, damage %, tốc độ đánh); 4 nhóm một phần (tầm đánh, tìm kho báu, may mắn, thu nhập); 5 nhóm chưa có (hệ số điểm theo build, va chạm gây ra, cận chiến cộng thẳng, tầm xa cộng thẳng, né tránh xác suất). Không cộng các trạng thái “một phần” thành parity đầy đủ.

## Thuộc tính phụ: không gộp nhầm vào 17 dòng trên

Các nhóm sau được tìm từ hiệu ứng công khai (S); công thức/cap đúng riêng bản 1.5 chưa đủ bằng chứng để khẳng định.

| Nhóm phụ | Arcane hiện tại |
| --- | --- |
| Kích thước đạn | Sprite/width có, bán kính collision đạn chưa là stat build tổng quát. |
| Xuyên mục tiêu | Có theo weapon/cấp; chưa có modifier xuyên chung. |
| Nảy đạn | Có ở họ vũ khí cụ thể; số lượt nảy không tương đương xác suất nảy. |
| Chia nhánh đạn | Có ở signature cụ thể, chưa có stat chia nhánh áp dụng mọi đạn. |
| Khoảng bay / tốc độ đạn | Weapon có; chưa có modifier build tổng quát. |
| Đẩy lùi / damage đẩy lùi | Có push tự động từ weapon; không có nút đẩy tay, theo luật user chỉ di chuyển. |
| Bán kính hút/nhặt | Có Pickup, coin flight/loot visuals; không thay hitbox người chơi. |
| Kích thước thủ lĩnh/đồng minh | Có kích thước render và radius definition, chưa có modifier build. Không xem phóng hình là tăng hitbox. |
| Các hệ số hiệu chỉnh/cap riêng nhân vật | Chưa có hệ thống correction tổng quát tương đương; có đặc tính thủ lĩnh và sàn riêng từng công thức. |
| Giá shop / phân phối hàng | Discount có; weight và pool có trong data. Chưa có đủ các kiểu thay trọng số theo build. |
| Số lượng/sức mạnh/tốc độ quái | Wave modifier và difficulty có; chưa cho mọi item/đồng minh sửa trực tiếp các thuộc tính địch. |
| Lượng hồi từ tim | Loot hồi máu có; chưa là modifier build riêng. |
| Tăng stat theo điều kiện | Có proc và signature với cause chain giới hạn; chưa đủ biến thể theo mua đồ, mỗi wave, mỗi kill, từng tag như reference. |
| Phí nuôi / tăng stat vĩnh viễn | **Chủ ý không làm**, theo luật đã chốt; hồ sơ chỉ mở khóa/trang trí/kỷ lục. |

Vàng, trứng, máu hiện tại, giá món, độ hiếm, số stack, cấp sao, số người/hệ, timer và điểm tích lũy là tài nguyên/trạng thái/metadata, không phải tất cả đều cần một stat cộng dồn riêng.

## Đồng minh: thiếu ở K8, đã nối ở K9

K8 chỉ có nội tại theo đơn vị và hệ theo số người. K9 thêm `CompanionModifiers` thật: người trong roster đang tồn tại đóng góp đúng scope; cả số dương và âm; cấp 1/2/3 nhân 1/2/4. Đây là thiết kế Arcane, **không chép hệ số BB**. Ghép 3 người thành 1 người cấp cao làm giảm tổng nguồn support 3→2, đổi lấy sức mạnh vũ khí/cấp; hệ vẫn đếm số người, không đếm sao.

| Đồng minh | Lợi cấp 1 | Đổi lại cấp 1 |
| --- | --- | --- |
| Lữ Khách Ấm Trà | Thủ lĩnh hồi +0,04 HP/s | Thủ lĩnh tốc độ −0,3% |
| Ốc Sên Nến | Thủ lĩnh giáp +0,025 | Thủ lĩnh tốc độ −0,5% |
| Tiểu Quỷ Hòm Thư | Bán kính nhặt +0,08 | May mắn −0,01 |
| Nhà Tiên Tri Gối | Thủ lĩnh máu tối đa +3 | Thủ lĩnh tốc độ đánh −0,5% |
| Thủ Thư Nấm | May mắn +0,025 | Đồng minh tốc độ đánh −0,5% |
| Linh Hồn Trùm Mũ | Đội chí mạng +1 điểm % | Thủ lĩnh máu tối đa −1 |
| Kiếm Sĩ Thạch | Đồng minh damage +2,5% | Đồng minh tốc độ −0,5% |

Thông tin xuất hiện trước mua và trong sổ đội quân, kèm nguồn modifier. Mua/ghép/altar tính lại từ roster, không cộng bonus vào save nhiều lần. Đổi HP tối đa giữ lượng máu đã mất; shop không được làm HP xuống dưới 1. Blood shop kiểm tra sức mua **trước** bonus của món mới.

## Ưu tiên bổ sung sau đối chiếu

1. Nếu mở rộng hệ chỉ số: né tránh, damage cận/xa cộng thẳng, tầm đánh, thu nhập là nhóm dễ đọc và có giá trị build trước.
2. Va chạm gây damage và hệ số điểm cần thiết kế riêng: không khuyến khích lao vào chết vì đổi damage, không làm leaderboard mất khả năng so sánh phiên bản.
3. Đạn lớn/xuyên/nảy/chia nhánh cần test collision và ngân sách projectile, không chỉ tăng sprite cho đẹp.
4. Chất lượng rương và tác dụng Luck cần UI ghi đúng phạm vi. Không sửa Luck để âm thầm đổi shop RNG.

Chưa triển khai các stat còn thiếu chỉ vì cùng tên với reference. Tài liệu này là danh sách chênh lệch để tránh báo đã có khi thực tế chỉ là data vũ khí hoặc một signature.
