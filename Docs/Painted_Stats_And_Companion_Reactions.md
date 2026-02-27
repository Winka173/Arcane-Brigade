# Icon stat và lời bình theo tình huống

**Cập nhật sau đợt fallback:** công cụ ảnh đã dùng lại được; đã vẽ và tích hợp **49 icon stat chuyên dụng** trong `Resources/MienCho/ActOne/Stat_*.png`. Runtime ưu tiên chúng, bộ phối cũ chỉ dự phòng. Đợt mới không chạy test/build; prompt/provenance và phạm vi xem [Theme_Consistency_Fixes.md](Theme_Consistency_Fixes.md). Những ảnh và số kiểm tra dưới đây thuộc bộ fallback cũ.

Cập nhật 17/10/2026. Phạm vi: thay phần hiển thị icon stat và thêm phản ứng companion khi mua món phù hợp, mở mốc bond và ghép sao. Không đổi bonus, quota mua, RNG chiến đấu hay quy tắc ghép.

## Art đang dùng và phần còn chờ

Runtime đã chuyển 49 ItemEffect sang bộ `PaintedStatIcons.asset`, dùng sprite vẽ tay hiện có của game. Nguồn chính là relic K19, vũ khí, bond và loot; các nhóm hero/companion, địch và correction có phối badge để phân biệt. Cắt theo vùng mực thực, chuẩn hóa kích thước và nghiêng các hình dài để rõ ở kích thước nhỏ. Tooltip, glossary và thông báo stat dùng chung qua StatGlyph. Không thêm texture theo từng frame.

Đây là **bộ phối từ art có sẵn**, không phải 49 tranh mới đã được tạo. Đã gọi công cụ `painted` tích hợp để vẽ atlas mới nhưng nhận lỗi `usage_limit_reached`. Không dùng API/CLI có phí làm phương án thay thế. [Prompt và tham chiếu đã lưu](Painted_Stat_Atlas_Request.json) để tiếp tục khi công cụ vẽ dùng được. Nguồn của từng stat nằm ở [Icon_Sources.tsv](../PreviewExports/CompanionReactions/Icon_Sources.tsv).

Asset runtime: `Assets/ArcaneBrigade/Resources/PaintedStatIcons.asset`. Builder: `PaintedStatIconBuilder.Import`.

## Lời bình đã triển khai

- 66 loài × 3 tình huống = **198 câu riêng**, không dùng chung một câu giữa các loài. [Danh sách câu](../PreviewExports/CompanionReactions/Reaction_Lines.tsv).
- Mua đồ: chọn người thật đang đi cùng phù hợp với modifier/scope và loại vũ khí đang cầm; ưu tiên cơ chế drink/food tương ứng. Không gán lợi ích ranged cho người cận chiến hoặc chuyển modifier hero-only sang companion.
- Bond: chỉ phản ứng khi giao dịch tăng count qua mốc thưởng thực trong catalog. Chọn một người thuộc bond; một giao dịch mở nhiều bond vẫn chỉ thêm một lời bình.
- Ghép: người lên sao dùng câu riêng của loài. Chỉ nhận là ghép khi các nguyên liệu sao thấp thực sự bị tiêu thụ; mua thẳng sao cao vẫn là gia nhập dù đang có hai bản sao thấp hơn trong đội. Evil vẫn không thể ghép.
- Hob, Latch, Postscript và Tally có thể trả lời khi đang hiện diện theo canon. Họ giữ tên/vị trí riêng; không biến thành cơ thể mua trong roster và không cộng count bond.
- Chỉ phát sau giao dịch đã lưu thành công. Xem mô tả, mua thất bại, save rollback và đọc lại receipt không sinh lời bình thành công.
- Một giao dịch thêm tối đa một câu theo ngữ cảnh, ngoài lời gia nhập/ghép hiện có. Chờ merchant, reveal và câu companion trước; không lặp cùng tình huống của cùng người trong một wave. Có giới hạn hàng đợi và thời gian chờ; câu cũ có thể được bỏ nếu người chơi đi nhanh. Pause giữ câu đang chờ; lời mua cũ không mang sang combat.
- Đây là **lời bình bằng chữ trong speech bubble**. Không có bộ thu âm mới cho 198 câu; lời ghép mới không phát một câu thu âm chung có nội dung khác đè lên nó.
- Thông báo tăng/giảm stat tránh vùng khung thoại và đuôi thoại để không đè lên câu companion đang nói.

## Kiểm tra và player

[Kiểm tra dữ liệu](../PreviewExports/CompanionReactions/Checks.txt) bao phủ 49 mapping/icon/badge, đủ 66 loài và ba tình huống, độ dài/câu riêng, scope/vũ khí, giao dịch đã lưu, rollback, RNG và mốc bond. Kết quả build và nghiệm thu player được lưu cùng thư mục [CompanionReactions](../PreviewExports/CompanionReactions/Build.txt).

Kết quả: **380/380 kiểm tra dữ liệu**, build Windows thành công (0 lỗi, 20 cảnh báo). Hai lượt player cuối cùng đều **36/36 kiểm tra qua, 6 ảnh mỗi lượt**: [1080p thường](../PreviewExports/CompanionReactions/Native/Run.json) và [720p giảm hiệu ứng/chuyển động/chớp](../PreviewExports/CompanionReactions/Reduced720/Run.json), cùng hash runtime `CC46FE7368C14006EB5A2A3DE88B804ED1DB921B70A1E27D1D8310BE42BEB2F2`. Đã xem ảnh icon, Hob nhận drink và ghép Button Beetle; ảnh ghép cuối đã tách số stat khỏi khung thoại. [Ảnh ghép](../PreviewExports/CompanionReactions/Native/Beetle-merge-response.png), [ảnh 720p](../PreviewExports/CompanionReactions/Reduced720/Hob-drink-response.png). Đây là kiểm tra các tình huống nêu trên, không phải chứng minh mọi đội hình đều bố trí hoàn hảo.

Player kiểm tra ở `D:/ArcaneBrigadeDiagnostics/CompanionReactions/ArcaneBrigade.exe`; workspace build riêng ở D để dữ liệu tạm không làm đầy C. Source/asset vẫn ở repository chính. Lệnh build `ArcaneBrigade.Editor.CompanionReactionChecks.VerifyAndBuild` nhận thêm `-reactionBuildPath` khi cần chọn ổ đĩa. Chạy capture bằng `Tools/Art/Capture_Reactions.ps1 -Player <đường dẫn player>`, thêm `-Mode Reduced -Width 1280 -Height 720` cho lượt 720p giảm hiệu ứng. Diagnostic dùng save/profile riêng.
