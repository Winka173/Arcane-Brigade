# Sửa UI/UX và tối ưu hiệu năng — 07/11/2026

Đã xử lý 8 phát hiện F01–F08 trong [bản audit trước](UIUX_Audit_2026-11-07.md), giữ tiếng Anh mặc định và lựa chọn tiếng Việt trong Settings. Bản Windows nằm tại `Builds/Localization/ArcaneBrigade.exe`. Các thay đổi tập trung vào nút sách, đường tính chỉ số dùng chung, tra cứu dữ liệu và chi phí vẽ UI; không thay đổi cân bằng chiến đấu.

| Phát hiện | Cách xử lý |
| --- | --- |
| F01 — tên tab sai nghĩa | Dùng DI VẬT, LIÊN KẾT, ĐỒNG HÀNH, CHỈ SỐ; kiểm tra cả biến thể viết hoa. |
| F02 — số lượng bằng 0 khi mới mở | Đếm trực tiếp dữ liệu sở hữu và các nhóm liên kết, kể cả dạng biến đổi; không phụ thuộc tab đã tải hay chưa. |
| F03 — thiếu hover | Thêm viền focus cho vùng bấm custom, trạng thái hover/pressed/disabled cho nút tăng giảm, phản hồi cho slider. |
| F04 — focus ra ngoài vùng nhìn | Gắn điều khiển với scroll panel chứa nó; khi đổi focus, cuộn để điều khiển hiện trong viewport. |
| F05 — mất tên đầy đủ sau dấu … | Vẽ tooltip riêng: hover trên desktop, giữ ngắn sau khi nhả chạm trên giao diện điện thoại; giới hạn trong màn hình và xóa khi đổi trang. |
| F06 — glossary không nhận tiếng Việt | Tạo alias tiếng Việt và bộ tách từ theo ngôn ngữ, giữ cùng định danh chỉ số. |
| F07 — dịch hỏng token | Bảo vệ cả mark, gives, costs, tag, stat và bonus; chỉ dịch phần chữ xung quanh. |
| F08 — nhãn còn English hoặc sai nghĩa | Biên tập nhãn sở hữu, chỉ số va chạm/đòn đánh, hướng dẫn nâng cấp; bổ sung toàn bộ tên cách đánh của 36 anh hùng và biến thể viết hoa. |

Sách dùng bản chỉ số đã biên dịch của trận đang chạy, thay vì tạo một RunSession, phục hồi save và biên dịch toàn bộ đội hình thêm lần nữa. Trang đầu vẫn tải riêng; các tab còn lại chỉ dựng khi cần. Trang chỉ số đầu tiên cũng áp dụng phần hiệu chỉnh chỉ số và thông tin anh hùng như bản đầy đủ. Mua bán vẫn dùng bản đọc độc lập để bảo toàn kiểm tra và rollback; đường này bỏ việc dựng mô tả di vật/liên kết không dùng trong biên nhận. Các nơi chỉ cần so sánh chỉ số cửa hàng lấy đúng snapshot đó.

Các tối ưu dùng chung: lập chỉ mục ID cho catalog, tái sử dụng danh sách đồng hành trong một lần compile, tái sử dụng mảng modifier cũ, bỏ tính bonus phụ thuộc khi không sở hữu di vật tương ứng, cache nội dung GUI và kết quả đo/cắt chữ có giới hạn. Bộ vẽ khung dùng vùng nhớ stack thay cho hai List và hai mảng mới cho mỗi khung; sách bỏ vẽ các thẻ/hàng ngoài vùng nhìn. Bảng dịch dùng bucket tiền tố thay cho cây dictionary theo từng ký tự; preload khi áp dụng tiếng Việt để tránh dồn tải vào lần vẽ đầu. Cache được làm mới khi đổi ngôn ngữ.

## Số đo trước/sau

Unity 6000.4.8f1, Windows, cùng catalog 223 di vật / 96 đồng hành / 117 vũ khí. Fixture lớn: wave 15, thêm 9 đồng hành vào đội hình khởi đầu, 24 loại di vật ×2. Đây là số đo CPU trong Editor batch; median của 3 lần cho các dòng tạo dữ liệu sách, các dòng khác là một lần đo. Không suy rộng thành mức tăng FPS trên mọi thiết bị.

| Công việc | Trước | Sau |
| --- | ---: | ---: |
| Tạo dữ liệu trang đầu sách, đội hình lớn | 19,65 ms | 1,03 ms qua đường trình bày trận đang chạy |
| Tạo đầy đủ read model độc lập | 26,71 ms | 21,20 ms |
| Compile đội hình lớn | 16,62 ms | 14,69 ms |
| 10.000 cặp tra ID di vật/đồng hành | 6,54 ms | 0,82 ms |
| 600 bước mô phỏng, đội hình lớn | 73,45 ms | 71,66 ms |
| Khởi tạo bảng dịch lạnh | 118,71 ms | 48,41 ms |
| Managed memory giữ lại sau tải bảng dịch | 78,86 MiB | 2,61 MiB |
| 10.000 nhãn động chưa cache | 13,22 ms | 20,85 ms |

Trang đầu sách giảm khoảng 95% chi phí dựng dữ liệu trong fixture lớn. Bộ tra dịch đổi một ít CPU khi gặp nhãn động mới để giảm khoảng 97% bộ nhớ giữ lại; các nhãn đã gặp đi qua cache. Chưa có bằng chứng tăng đáng kể tốc độ mô phỏng chiến đấu: chênh lệch 73,45/71,66 ms nhỏ và có thể chịu nhiễu. Đường đọc độc lập vẫn compile lại để kiểm tra giao dịch. Lần cold đầu tiên trong Editor còn chi phí JIT/tải tài nguyên, không được tính như lần mở sách đã có trận đang chạy.

Dữ liệu gốc: [baseline](Verification/Performance/baseline.json), [after](Verification/Performance/after.json), [dịch trước](Verification/Performance/localization-baseline.txt), [dịch sau](Verification/Performance/localization-after.txt). Counter `GC.GetAllocatedBytesForCurrentThread` trả 0 không đáng tin trong Editor Mono này; trường `allocatedBytes: 0` trong JSON **không có nghĩa là không cấp phát**. Chỉ dùng số retained memory sau GC ở phép đo lạnh riêng biệt.

Đo native Windows riêng, bỏ cap/vsync chỉ trong fixture đo rồi khôi phục; mỗi cảnh lấy 600 frame sau 60 frame làm nóng, không ghi screenshot/stacktrace trong thời gian lấy mẫu. Máy dùng RTX 5070 Ti; một bản game khác của chủ dự án đang mở được giữ nguyên, và không chạy Unity build trong khi lấy các mẫu cuối.

| Cảnh native | FPS trung bình | Frame p95 | CPU main p95 | GPU p95 |
| --- | ---: | ---: | ---: | ---: |
| Trận wave 15, sau tối ưu | 196,4 | 6,79 ms | 6,83 ms | 0,21 ms |
| Sách, bản trung gian còn vẽ các hàng ngoài vùng nhìn | 44,8 | 25,43 ms | 25,42 ms | 0,45 ms |
| Sách, bản cuối chỉ vẽ hàng đang nhìn | 87,0 | 13,46 ms | 13,45 ms | 0,24 ms |

Đây là so sánh native giữa hai bản trong lượt tối ưu, **không phải baseline FPS của commit gốc**. Lần mở sách lạnh cuối cùng: 8,34 ms cho callback đồng bộ; không đo thời gian input đến pixel đầu tiên. Các lần fixture khác khoảng 1–14 ms tùy mức làm nóng. Trận đo có tối đa 46 unit / 78 projectile, chưa đại diện cho mọi tình huống endless. Native GC recorder không hỗ trợ trong build này nên báo NA. Số liệu và từng frame: [combat](Verification/Performance/final-expanded-combat-summary.txt), [book trước cull](Verification/Performance/book-before-cull-summary.txt), [book cuối](Verification/Performance/final-expanded-book-summary.txt), [combat CSV](Verification/Performance/final-expanded-combat-frames.csv), [book CSV](Verification/Performance/final-expanded-book-frames.csv), [callback sách](Verification/Performance/book-open-ms.txt).

## Kiểm tra và giới hạn

[Kiểm tra chức năng mới](Verification/Performance/uiux-functional.txt) đạt 116 assertion: tên chức năng/cách đánh, alias glossary, tất cả token, cache khi đổi ngôn ngữ, công thức đưa focus vào vùng nhìn, số lượng tab, mở sách không đổi trạng thái, chỉ số trình bày so với compiler độc lập, snapshot giao dịch và tính đúng của catalog khi thay/đổi ID. [LocalizationChecks](Verification/LocalizationChecks.txt) đạt; catalog offline có 8.017 mục. Đã biên tập thêm mô tả chỉ số glossary, đồng bộ vị trí bảng với vùng chặn bấm, dùng hình nút đóng thay cho ký tự bị co thành dấu chấm, đóng glossary khi mở sách và xóa bảng chọn độ khó khi đổi trang.

[Kết quả regression](Verification/Performance/regressions.txt) giữ rõ 5 nhóm thất bại: FullRun (duplicate identity save), CompanionDeployment/K6 (HP kỳ vọng 68 so với 56), HeroIdentity (định dạng hàng identity), K26 (giá tiền thay thế), K7 (mặc định nâng cấp profile). Đã chạy lại cùng các suite trên runtime nguyên gốc tại commit `c8a341b66`; chúng thất bại tại cùng assertion, xem [baseline suite cũ](Verification/Performance/legacy-baseline.txt). Không đổi balance hoặc sửa test để che các thất bại này. Localization, K10 stat compilation và bộ test mới đạt; đây không phải tuyên bố toàn bộ suite cũ đều xanh.

Render/hover: [48 trạng thái chính và smoke cuối](Verification/Performance/UIUXAuditAfter.json) cùng [6 trạng thái kiểm tra luồng](Verification/Performance/UIUXAuditTargetAfter.json), tổng 813 mẫu hover. Có 698 mẫu khớp vùng trỏ mục tiêu; 115 mẫu không khớp không được tính là pass. Còn 14 vùng không đổi pixel trong các ảnh cũ có bảng chọn phủ phía trên, không tự động coi là lỗi hover. Roster cuối có 20/20 vùng khớp, tất cả có phản hồi; các vùng Settings có phản hồi ở các mức cuộn đã chụp. Bộ sweep không chứng nhận mọi điều khiển/tổ hợp trạng thái trong game.

[Smoke native cuối](Verification/Performance/native-flow.txt) xác nhận bản dịch glossary, nút đóng hoạt động qua activation nội bộ, Settings đưa focus 31 vào vùng nhìn, 4 tab sách render/sweep và không ghi nhận runtime error. Ảnh kiểm chứng: [focus cuộn](Verification/Performance/vi-settings-navigation-offscreen.png), [tooltip đầy đủ](Verification/Performance/vi-achievements-full-name-hover.png), [glossary và nút đóng](Verification/Performance/final-reviewed-glossary.png), [sách mở với số lượng đúng](Verification/Performance/final-expanded-book.png). Windows release build thành công, 0 error / 49 warning; xem [build report](Verification/LocalizationBuild.txt).

Toàn bộ kiểm tra UI dùng trạng thái con trỏ/activation/navigation bên trong bản chẩn đoán, save tách biệt. Không điều khiển chuột, bàn phím hoặc lấy focus của hệ điều hành. Render Windows không thay thế kiểm thử chạm thực tế, gamepad thật hay đo trên điện thoại. Bảng dịch vẫn có văn bản máy dịch và các chuỗi mới chưa có bản dịch dùng English fallback; không tuyên bố đã biên tập thủ công toàn bộ lời thoại. Thanh cuộn vẫn hỗ trợ wheel/drag nội dung; chưa thêm thao tác kéo trực tiếp thumb/nhấn track từ ghi chú phụ của audit.
