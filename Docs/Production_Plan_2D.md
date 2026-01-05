# Arcane Brigade — Gameplay và kế hoạch sản xuất 2D

**Ngày:** 2026-10-13  
**Trạng thái:** Đã có Full Run functional candidate; chưa nghiệm thu production/reference parity. Audit mới theo bản 1.5+ ở `Bizarre_Brigade_15_Reference_Audit.md`, backlog triển khai ở `Reference_Parity_Backlog.md`.
**Nền tảng:** Android, iOS, Windows PC qua Steam.  
**Mục tiêu:** Giữ sát trải nghiệm Bizarre Brigade trước, rồi mở rộng độ dài, độ thỏa mãn và sự đa dạng; chất lượng cao về cảm giác, animation, chiều sâu build và UI, không bỏ qua âm thanh hay hiệu năng.

**Kế hoạch thực thi:** [Implementation_Plan.md](Implementation_Plan.md) chốt checkpoint P0–P7, hợp đồng prefab và tiêu chí bắt đầu implement. Hướng Minimal Arcade đã được chủ dự án duyệt; cần thêm cá tính có chọn lọc và bàn tay gắn vũ khí, không có cánh tay.

**Bản sắc mở rộng đã duyệt:** [Signature_Gameplay_Expansion.md](Signature_Gameplay_Expansion.md) đặc tả vũ khí đồ vật kỳ quặc, build dây chuyền, biến hình khi ghép, map có đạo cụ tương tác và cảm giác hit/death/loot nhiều lớp. Giữ movement-only, không formation command; đây là AUTHORED design chưa implement, không ghi thành luật đã xác minh của Bizarre Brigade.

**Nhận diện giấc mơ duyệt toàn bộ:** [Nightmare_Identity_UI_Gameplay.md](Nightmare_Identity_UI_Gameplay.md) mở rộng UI hồ sơ/biên lai/hóa đơn kết quả, typography, enemy/boss, cá tính animation/audio và museum/nickname/cosmetic. K1 đã có Details và font nền hỗ trợ tiếng Việt; K2 có death/coin/pose. Phần còn lại chưa triển khai; không đổi power progression, controls hoặc scope solo. Xem `K0_K2_Implementation.md`.

## 1. Thứ tự ưu tiên tài liệu

**Thực thi hiện hành:** `Implementation_Roadmap.md` (K0–K7) sắp dependency/quality gate cho toàn scope đã duyệt, gồm ba bổ sung cuối N16–N18 trong `Nightmare_Identity_UI_Gameplay.md`. Không bắt đầu bằng thêm content hàng loạt; làm camera/Details/menu rồi death/coin/pose trước.

1. Các quyết định gameplay của chủ dự án trong cuộc trao đổi ngày 2026-10-13, tổng hợp bên dưới.
2. Tài liệu này: định hướng triển khai, đề xuất mặc định và các cổng nghiệm thu.
3. `Arcane_Brigade_Design_Document.md`: nguồn ý tưởng, tên nhân vật, thế giới và nội dung; không còn là đặc tả luật chuẩn khi mâu thuẫn với quyết định mới.
4. `MVP_Implementation_Plan.md`: kế hoạch lịch sử, không dùng trực tiếp để triển khai.

Tank-arena chỉ là tham khảo cách tổ chức đặc tả, dữ liệu, QA và pipeline sản xuất. Không sao chép luật RTS, quy trình commit/push hoặc quyền chạy test của dự án đó sang Arcane Brigade.

## 2. Gameplay đã chốt với chủ dự án

| Hệ thống | Quyết định |
|---|---|
| Thể loại | Roguelike đội quân tự chiến đấu, giữ sát Bizarre Brigade trước |
| Hiển thị | 2D vẽ tay nét cao có animation; không phải 3D, không phải pixel art |
| Cấu trúc nhân vật | Không cánh tay/cẳng tay; bàn tay gắn và chuyển động cùng vũ khí; thêm chi tiết độc lạ có chọn lọc |
| Điều khiển trong trận | Chỉ di chuyển; không nút đánh, ngắm, né/lướt, ultimate hay đổi vũ khí |
| Champion | Passive và tự đánh; không kỹ năng chủ động |
| Đấu trường | Cập nhật theo chủ dự án: một arena giới hạn rộng hơn viewport; camera theo người chơi và clamp ở rìa, không fit toàn sân |
| Đồng minh | Bất tử, không có máu để hồi; mặc định không chặn đạn |
| Chặn đạn | Chỉ khi có vật phẩm/cơ chế cho phép |
| Di chuyển đồng minh | Ở gần người chơi; vật phẩm có thể mở rộng phạm vi hoạt động |
| Vũ khí đồng minh | Ngẫu nhiên khi tuyển dụng, theo cơ chế tham chiếu; bỏ vũ khí cố định theo loại |
| Số đồng minh | Không giới hạn đội hình bằng số slot; không phí nuôi |
| Bản sao và ghép | Theo Bizarre Brigade; không tự thay bằng mô hình tích mảnh |
| Bond | Theo số đơn vị hiện hữu, không nhân theo cấp sao; có nhiều tag/bond |
| Va chạm địch | Có sát thương chạm người; độ khó tăng dần |
| Trúng nhiều đòn | Cho phép nhiều nguồn sát thương cùng lúc; không thêm miễn nhiễm toàn cục để né luật này |
| Một đòn | Không được có sức mạnh quá cao, tránh một đòn xóa nhân vật đầy máu |
| Nhặt đồ | Vàng/vật phẩm trên sân, hướng trải nghiệm như bản tham chiếu |
| Shop | Giữa wave, mua đồng minh và nâng cấp bằng cùng ngân sách vàng |
| Nâng cấp | Cộng dồn, phải mua sau wave; không hệ XP lên cấp chọn thưởng miễn phí trong trận |
| Kinh tế | Có reroll và lãi; không bán lại, không khóa món, không phí nuôi |
| Nguyên tố | Giữ bốn damage type, status và phản ứng; mọi tương tác tự động |
| Thời lượng | Run thường mục tiêu 30–40 phút gồm shop, không cần kéo dài hơn; có lựa chọn tiếp tục Endless |
| Nội dung mở rộng | Có cơ chế như altar và ghép đặc biệt, không chỉ bản chơi cơ bản |
| Phát hành | Hiện tại miễn phí; không tự thêm quảng cáo, IAP hay gacha trả tiền |

### Quyết định mặc định được giao cho người triển khai

- Ưu tiên solo thường và Endless; co-op online là giai đoạn sau, không làm giảm chất lượng solo để chạy theo tính năng.
- Lưu run ở ranh giới wave/shop; lưu mở khóa, codex, thành tích, cosmetic và kỷ lục sau khi chết.
- Không dùng nâng chỉ số vĩnh viễn làm điều kiện vượt độ khó; tăng khả năng chọn build qua mở khóa nội dung.
- Roster fantasy trong design là nguồn nội dung dài hạn, không cam kết sản xuất toàn bộ 18 Champion trước khi core loop được duyệt.
- Art lấy cảm giác dễ đọc, đội quân đông và sự kỳ quái của Bizarre Brigade làm tham chiếu; nhân vật, tranh, âm thanh, tên và UI của Arcane Brigade phải có bản sắc riêng.

## 3. Cổng đối chiếu Bizarre Brigade — không đoán luật

Đã tra cứu nguồn công khai, chưa trực tiếp thao tác game hoặc xác nhận từng frame qua video. “Làm y chang” về cơ chế không đồng nghĩa mọi chi tiết đã được kiểm chứng.

### Điều đã có nguồn hỗ trợ

- Vòng chơi sinh tồn đến khi hết timer được mô tả trong [Icy Veins](https://www.icy-veins.com/zenless-zone-zero/bizarre-brigade).
- Shop giữa wave, vũ khí ngẫu nhiên có icon nhận diện, ba đồng minh cấp thấp ghép thành cấp cao hơn và altar dùng tài nguyên đặc biệt được mô tả trong [hướng dẫn GamerSky ngày 2024-09-30](https://www.gamersky.com/handbook/202409/1824286.shtml). Đây là hướng dẫn bản cũ, không đủ xác nhận luật hiện tại.
- [Danh mục đồng minh cộng đồng](https://zenless-zone-zero.fandom.com/wiki/Bizarre_Brigade/Companions) có công thức ba bản sao và các công thức altar đặc biệt. Không suy ra từ đó rằng mọi cấp đều ghép theo cùng một luật.

### Bắt buộc kiểm chứng trước khi code hệ tương ứng

| Mục | Bằng chứng cần lấy | Không được tự giả định |
|---|---|---|
| Bản sao trên sân | Shop mua liên tiếp cùng loại/cùng vũ khí và cảnh đội hình sau mua | Không tự chọn tích mảnh hoặc cho bản sao tồn tại |
| Ghép thường | Trước/sau ghép, vật liệu, loại, cấp, vũ khí kết quả | Không mặc định cần cùng vũ khí, auto-merge, giữ hoặc reroll vũ khí |
| Cấp và độ hiếm | Công thức từng cấp, ngoại lệ và loại không thể ghép | Không đánh đồng độ hiếm với cấp nâng cấp |
| Vũ khí trong shop | Thẻ trước mua, đơn vị sau mua, reroll và mua lại | Không khẳng định thấy trước hay chỉ biết sau mua nếu chưa đối chiếu |
| Phân phối vũ khí | Ví dụ nhiều loại/cấp đồng minh và nhóm vũ khí | Không tự giới hạn thành nhóm vũ khí riêng cho từng loại |
| Targeting | Nhiều mục tiêu, thay đổi khoảng cách, địch chết, hướng di chuyển | Không lấy nearest-target trong MVP làm luật đã xác nhận |
| Theo người chơi | Người chơi đổi hướng, đứng yên, chạm/đẩy đồng minh, item tầm hoạt động | Không biến thành AI tự đi săn khắp sân |
| Altar | Thời điểm xuất hiện, tiền tệ, công thức, kết quả và giới hạn | Không chép nguyên giá trị của bản 2024 thành bản hiện tại |

Mỗi kết luận ghi phiên bản tham chiếu, URL/video, timestamp nếu có, thao tác, kết quả và mức tin cậy. Nếu chưa tìm được bằng chứng, giữ `NEEDS_REFERENCE`, không quảng cáo là đã làm đúng bản gốc. Có thể tiếp tục làm các phần độc lập trong lúc hoàn tất đối chiếu.

## 4. Vòng chơi và nhịp độ

`Chọn Champion → vào wave → di chuyển/né bằng vị trí/nhặt đồ → wave kết thúc → nhận thu nhập và lãi → shop/altar/ghép → mua build → wave tiếp theo → boss → thắng hoặc chết → kết quả/lưu tiến trình`.

Đề xuất nhịp run, chưa phải bảng cân bằng đã kiểm thử:

- Đầu run: đủ thời gian hiểu đòn tự đánh và thực hiện giao dịch đầu tiên có ý nghĩa.
- Giữa run: build bắt đầu vận hành; bond, vũ khí và vật phẩm thay đổi cách di chuyển/nhặt đồ.
- Cuối run: đội quân và phản ứng tạo cảm giác snowball, vẫn thấy người chơi, đạn địch và khoảng thoát.
- Wave thường theo timer; boss theo hạ mục tiêu. Cổng đối chiếu ở phần 3 sẽ xác nhận phần giữ sát bản gốc trước khi khóa lịch wave.
- 30–40 phút là mục tiêu trải nghiệm, không đạt bằng cách nhân máu hoặc kéo dài wave không có quyết định mới.
- Chưa khóa số wave, timer từng wave và lịch boss cho tới khi đo nhịp chơi.

Shop dừng mô phỏng trận. Nâng cấp và đồng minh cùng tranh ngân sách; lãi phải tạo lựa chọn “mua mạnh ngay hay tiết kiệm”, không biến nhịn mua thành đáp án đúng duy nhất.

## 5. Chuyển đổi design cũ

| Nội dung cũ | Xử lý mới |
|---|---|
| Stylized 3D | Thay bằng art bible 2D vẽ tay, animation và sprite atlas |
| Đánh thủ công, aim, đổi vũ khí | Loại khỏi điều khiển trong trận |
| 18 ultimate | Bỏ khỏi kit; chỉ tái sử dụng ý tưởng thành passive/item khi phù hợp, không tự bật ultimate theo cooldown |
| Ally có vũ khí cố định | Tách loại đồng minh, passive, bond và vũ khí ngẫu nhiên |
| HP, chết, hồi sinh đồng minh | Bỏ; healer hồi người chơi, hỗ trợ buff đội, tank kiểm soát/bảo vệ theo luật item |
| Blessing slot, equip/unequip/sell | Thay bằng nâng cấp mua và cộng dồn trong run, không menu trang bị giới hạn ô |
| Ghép 3/5 bản sao | Không dùng; đối chiếu luật ba thành một và ngoại lệ của tham chiếu |
| 4-wave MVP, ghép và audio bị hoãn | Chỉ phù hợp thử kỹ thuật ban đầu, không đủ chứng minh chất lượng game |

Không lấy nguyên số HP/damage trong design để triển khai: HP Champion 38–55 trong khi nhiều đòn địch có damage 60–200 gây nguy cơ one-hit; cần xây lại baseline đồng bộ.

## 6. Combat và chiều sâu build

- Tách passive Champion, passive đồng minh, hành vi vũ khí, item và bond thành các lớp dữ liệu độc lập.
- Giữ nhóm build: Champion tự gây damage, đội quân, va chạm, kinh tế/nhặt đồ, nguyên tố/kiểm soát và hybrid.
- Item cộng dồn phải hiển thị số lượng, hiệu quả thực tế và phần vượt trần nếu có; không ngầm giới hạn mà UI không nói.
- Mọi trạng thái có stack rule, thời hạn, refresh rule, boss resistance và nguồn áp dụng rõ ràng.
- Mọi phản ứng có quy tắc thứ tự, attribution và giới hạn tái kích hoạt; damage phản ứng không vô hạn gọi lại chính nó.
- Projectile chạm đồng minh mặc định đi xuyên; chỉ interceptor được cấp bởi item mới được chặn/phản xạ đúng quyền.
- Không có giới hạn slot đồng minh. Giới hạn VFX/render chỉ giảm trang trí, không xóa đơn vị hoặc bỏ damage.

### Sát thương nhận vào

Không có bất tử sau trúng đòn dùng chung cho mọi nguồn. Nhiều projectile/địch khác nhau có thể đánh cùng tick.

Một projectile hoặc một lần vung chỉ áp damage một lần trên mỗi mục tiêu theo `AttackInstanceId`. Chạm cùng một địch liên tục dùng nhịp sát thương contact riêng, không áp lại mỗi frame. Đây là chống lỗi tính damage trùng, không phải bảo vệ khỏi nhiều đòn hợp lệ.

Thiết lập ngưỡng từng đòn dựa trên HP baseline theo giai đoạn/độ khó, cân bằng damage và HP cùng nhau. Đề xuất baseline: đòn thường không quá khoảng 15% HP tham chiếu, đòn boss mạnh không quá khoảng 30%; các số này là điểm khởi đầu để thử, không phải số đã chốt. Không tăng damage địch theo HP thực tế của người chơi để vô hiệu hóa build tăng máu. Không che vấn đề bằng giảm tổng damage của nhiều nguồn cùng lúc.

## 7. Kiến trúc Unity dự kiến

Một codebase Unity 2D cho ba nền tảng. Project preview hiện có manifest và Editor 6000.4.8f1, dùng built-in renderer; chưa có URP hoặc Input System mới. Giữ nền đang chạy cho checkpoint đầu, đánh giá nhu cầu URP 2D trước sản xuất art/VFX hàng loạt; không tự đổi pipeline hoặc nâng Editor giữa milestone. Xem phần nền tảng trong `Implementation_Plan.md`.

```text
Assets/ArcaneBrigade/
  Runtime/
    Core/             trạng thái run, timer, ID, seed, gameplay events
    Combat/           damage, contact, projectile, status, reaction
    Champions/        movement, passive, auto-attack
    Brigade/          following, targeting, roster, merge, bonds
    Economy/          gold, income, interest, shop, altar
    Progression/      unlock, codex, achievements, run summary
    Persistence/      versioned saves, recovery, migration
    Presentation/     sprites, animation, VFX, audio, camera
    UI/               HUD, shop, collection, settings, result
    Platform/         input adapters, lifecycle, Steam adapter
    Data/             definitions và catalog loading
  Editor/             catalog validators, import, preview, build tooling
  Art/                sprites, atlas, rigs, animations, materials
  Audio/              SFX, music, mixer
  Data/               catalog và balance assets
  Scenes/             Boot, Frontend, Arena, Preview
```

- Luật game không phụ thuộc animation frame, draw call hoặc UI. Hit/shot event từ mô phỏng dẫn animation, không damage do VFX tự tạo.
- Fixed-step combat độc lập render; chốt tick rate bằng đo đáp ứng và tải, không bê 20 Hz của RTS sang game này.
- Pool projectile/VFX, truy vấn không cấp phát theo frame, spatial index để tránh tìm mục tiêu theo toàn bộ cặp đơn vị.
- Stable ID và generation của run để callback/đạn cũ không tác động run mới.
- RNG có seed, tách luồng shop/vũ khí/wave/gameplay với RNG trình bày. Không hứa deterministic cross-platform từ physics float của Unity.
- Định nghĩa dữ liệu có ID, version, đơn vị, nguồn và provenance; có một nguồn chuẩn, không chỉnh JSON và ScriptableObject riêng rẽ.
- Catalog validation phát hiện thiếu ID, công thức ghép vòng, giá sai, tag không tồn tại và asset thiếu.
- Cập nhật save khi mua/ghép trong shop để resume không nhân vàng hay nhân vật; ghi an toàn, có backup, version và migration.

## 8. Art, animation, UI và âm thanh

**Đính chính sau đối chiếu ảnh Bizarre Brigade:** theo [Minimal_Arcade_Art_Direction.md](Minimal_Arcade_Art_Direction.md), ưu tiên khối 2D phẳng cực đơn giản, đầu lớn/thân nhỏ, mặt vài nét và khả năng đọc ở 32–48 px. Không dùng các mẫu anime-chibi fantasy nhiều giáp, gouache hoặc dark-ink chi tiết trước đó làm chuẩn. Nét cao không có nghĩa nhiều chi tiết.

Không coi các hình tròn placeholder là chuẩn chất lượng cuối. Trước sản xuất hàng loạt cần một Champion, một đồng minh, một địch và một cụm HUD/shop đạt chuẩn được chủ dự án duyệt.

- Art bible: nét, bảng màu, silhouette, tỷ lệ, hướng nhìn, kích thước thực trên điện thoại, cách gắn vũ khí, tầng nền và tương phản.
- Style nền đã được duyệt. Thêm một vài chi tiết lớn và cá tính độc lạ, không tăng mật độ giáp/hoa văn. Không cánh tay/cẳng tay; cụm bàn tay–vũ khí chuyển động cùng nhau, body và vũ khí tách asset.
- Animation: locomotion, idle, anticipation, strike/fire, recovery, phản ứng và ghép; động tác vũ khí phải khớp impact.
- Xét rig 2D kết hợp animation vẽ tay cho key action; chỉ chọn pipeline sau khi kiểm chứng chất lượng và chi phí trên mẫu duyệt.
- Giữ nhân vật chính nổi bật; màu, icon và shape cùng báo phe/đòn, không chỉ dựa màu sắc.
- VFX đồng minh bớt ưu tiên hơn đạn/telegraph địch. Có tùy chọn giảm flash, rung màn, số damage và độ đậm hiệu ứng.
- HUD: HP, timer/wave, vàng, bond quan trọng. Chi tiết roster/build nằm trong bảng riêng, không phủ sân.
- Shop: đọc được loại đồng minh, vũ khí, cấp, tag, giá, số lượng item đã có, tác động trước/sau, reroll và lãi dự kiến.
- Âm thanh: melee/ranged/support có cue riêng; mix giữ cảnh báo, trúng đòn và pickup nổi bật trong đám đông. Có nhạc và SFX ngay ở vertical slice.
- Asset nguồn và giấy phép được lưu; không dùng art/sound/logo ZZZ làm asset phát hành.

## 9. Đa nền tảng và chất lượng có thể đo

- Mobile ngang, một tay với joystick ảo linh hoạt; chỉnh vị trí/kích thước, safe area và xử lý mất focus.
- PC có WASD/phím mũi tên; đề xuất hỗ trợ tay cầm và điều hướng menu bằng tay cầm. Không cần aim chuột.
- Cùng arena và vùng nhìn gameplay trên tỷ lệ màn hình khác nhau; phần dư bố trí UI/letterbox, không cho máy rộng lợi thế nhìn thêm.
- Android/iOS có suspend/resume, cuộc gọi, đổi audio device và save khi lifecycle thay đổi; không để thời gian nền làm mất run.
- Windows build chạy độc lập trước; Steam adapter bổ sung achievement/cloud save theo nhu cầu, không làm core phụ thuộc đăng nhập.
- iOS cần môi trường macOS/Xcode hoặc build service thích hợp để tạo và ký app; không hứa xuất IPA hoàn chỉnh chỉ từ Windows. Xem [Unity iOS build process](https://docs.unity.com/en-us/engine/6000.3/manual/platform-specific/iphone/ios-building-and-delivering/build-process).

Mục tiêu ban đầu, chưa có số đo: 60 FPS trên thiết bị mục tiêu; 30 FPS ổn định cho tier thấp; PC 60 FPS tối thiểu trên cấu hình tối thiểu được chọn. Chưa tuyên bố đạt hay gán cấu hình tối thiểu khi chưa thử máy thật.

Lập tải chuẩn ở các mốc 25/50/100/200 đồng minh cùng địch, đạn, status và phản ứng cuối run; các mốc này là kịch bản đo, không phải giới hạn roster. Theo dõi frame-time P95/P99, CPU/GPU, memory, GC, overdraw, thời gian tải, nhiệt và pin qua run dài. Tìm nguyên nhân khi không đạt; không bí mật giảm số đồng minh/damage để che tải.

## 10. Các giai đoạn triển khai

### G0 — Khóa đặc tả tham chiếu và baseline

Hoàn tất bảng chứng cứ phần 3; tách luật đã xác nhận với ngoại lệ chủ dự án (không phí nuôi, no one-hit, dài hơn). Thiết kế catalog, bảng HP/damage, economy/interest và một run skeleton. Tạo moodboard/art bible sơ bộ.

**Đạt khi:** mỗi luật tuyển dụng/ghép/vũ khí/targeting có bằng chứng hoặc được ghi rõ chưa xác nhận; không code cứng quyết định chưa biết. Các phần độc lập có thể bắt đầu trước, nhưng hệ cần reference chưa được coi là nghiệm thu.

### G1 — Nền tảng Unity và combat feel

Khởi tạo project 2D, input PC/touch, Boot/Frontend/Arena, camera theo người chơi trong arena giới hạn, movement và auto-attack; một địch melee, một ranged, contact damage, pickup và health. Tạo hệ damage/events/pool và preview asset mẫu.

**Đạt khi:** chỉ di chuyển là chơi được; không aim/attack/skill ẩn; sát thương và animation khớp, projectile không xuyên sai/đánh trùng, địch không spawn đè người chơi.

### G2 — Vertical slice chứng minh build

Đề xuất 3 Champion khác playstyle, 6 loại đồng minh, 6–8 vũ khí, 6 loại địch, 1 boss nhiều pattern, 1 arena; khoảng 20 item mua được với build tương tác rõ. Có ghép thường, bond, reroll, lãi và một nhóm công thức altar đặc biệt. Một run ngắn đủ trải nghiệm vòng chơi, không dùng run dài để che core chưa vui.

**Đạt khi:** chơi được ít nhất ba hướng build; quyết định mua có đánh đổi; healer hoạt động đúng người chơi; đồng minh bất tử và default không chặn đạn; có HUD/shop/audio/animation chuẩn mẫu, không chỉ số lượng content.

### G3 — Run đầy đủ và chiều sâu

Đạt nhịp 30–40 phút, thêm Endless, variety theo wave, status/reaction và altar mở rộng. Thêm meta unlock, codex, kết quả run, save/resume và recovery. Mở rộng Champion/ally/item theo build còn thiếu, không làm đủ 18 nhân vật bằng biến thể chỉ đổi màu.

**Đạt khi:** càng lâu càng khó nhưng không one-hit; bộ build không chỉ có một lựa chọn áp đảo; nguyên nhân chết đọc được; mua/ghép/resume không dup; run có cao trào chứ không chỉ tăng HP.

### G4 — Production art, animation và polish

Hoàn thiện asset qua pipeline mẫu được duyệt, âm thanh/nhạc, boss presentation, UI transitions và accessibility; bổ sung arena/theme với viewport/camera nhất quán trên thiết bị. Ưu tiên chất lượng đồng nhất trước mở rộng content.

**Đạt khi:** cảnh đông vẫn đọc được nguy hiểm; item/vũ khí/ghép có hình và phản hồi riêng; menu và shop thao tác tốt trên điện thoại lẫn PC.

### G5 — Hiệu năng và phát hành ba nền tảng

Chọn thiết bị/cấu hình mục tiêu, đo và tối ưu, regression combat/economy/save/UI, run dài/Endless, Android/iOS lifecycle, Windows/tay cầm; build/release pipeline và store readiness. Tài khoản, chứng chỉ, thiết bị iOS và quyền Steam là phụ thuộc ngoài mã nguồn, không tự coi đã có.

**Đạt khi:** có bản build thực tế được kiểm tra cho cả ba nền tảng, báo cáo kết quả và lỗi còn mở; không coi Windows chạy được là bằng chứng Android/iOS đạt.

## 11. QA và cách tổ chức kế thừa Tank-arena

Tham khảo `HANDOFF_machine_brigade.md`, `RTS_GAME_PLAN_V2.md`, `Machine_Brigade_PROJECTILE_FLIGHT_FEEL_MASTER_SPEC.md`, `Machine_Brigade_MAP_VISUAL_AUDIO_MASTER_SPEC.md`, `export_current/README.md` và `test/Machine_Brigade_FULL_GAME_QA_TEST_GUIDE.md`.

- Ghi build/version dữ liệu/seed/setup trong báo cáo; expected và observed tách riêng, không ghi PASS khi chưa chạy.
- Kiểm tra vai trò mỗi build: mạnh ở đâu, yếu ở đâu, scaling và chi phí; không kết luận cân bằng từ một trận.
- Test ghép với khác loại/vũ khí/cấp, item cộng dồn, bond trước/sau ghép, RNG sau resume, transaction mua thiếu vàng và click kép.
- Test đồng minh bất tử, chặn đạn có/không item, contact cooldown theo nguồn và nhiều đòn cùng tick.
- Test reaction recursion, proc làm giàu vô hạn, lifesteal từ overkill, hồi máu nhân đôi và save migration.
- Có asset contract, preview nhân vật/vũ khí/hiệu ứng, bảng tune có nguồn chuẩn và báo cáo bàn giao theo milestone.
- Nghiệm thu cảm giác/animation/UI cần chơi và duyệt hình thực tế; compile và unit test không thay thế được.

Không chạy Unity, test, simulation hoặc benchmark trong lượt lập tài liệu này. Khi bước sang triển khai, thống nhất lịch chạy validation với chủ dự án.

## 12. Việc tiếp theo

 Hoàn tất đối chiếu reference trọng yếu và bắt đầu P0/P1 theo `Implementation_Plan.md`: prefab body + cụm bàn tay/vũ khí không cánh tay, nền tảng movement và combat testbed. Giữ preview đã duyệt để so sánh. Không triển khai hệ ghép bằng giả định tích mảnh, không tạo prototype 3D, không giữ ultimate hay blessing slot của tài liệu cũ.
