# Bổ sung bản dịch chat — 07/11/2026

Câu trong ảnh, `Not those? Fair.`, hiện hiển thị **“Chưa ưng à? Được thôi.”** khi chọn tiếng Việt. Tiếng Anh vẫn là mặc định.

Nguyên nhân: `VoiceLines` lưu dữ liệu dạng `spoken text|delivery note`, nhưng runtime tách lấy trường đầu tiên làm phụ đề. Bộ trích xuất cũ thu thập cả chuỗi và đôi khi chia pipe bằng quy tắc dành cho tên cách đánh, khiến 273 khóa thoại riêng chưa có trong bảng dịch. Bong bóng thoại đã gọi bộ dịch đúng; khóa tra mới là phần bị thiếu.

Đã sửa bộ trích xuất để chỉ lấy câu được nói, bỏ ghi chú diễn xuất; chuyển các bản dịch cũ sang khóa câu thoại riêng khi hai phía giữ đúng dấu phân cách, rồi áp dụng nội dung biên tập cuối cùng. Đã biên tập lại toàn bộ lời của chủ tiệm và Mẹ Giấc Mơ, gồm chào hỏi, mua hàng, đổi hàng, thiếu tiền và lời thoại cốt truyện. Những nhân vật khác dùng bản dịch đã có, nay tra được bằng đúng khóa phụ đề. Không đổi ID nhân vật, trigger, tên clip hoặc bản thu âm.

Hiện đủ khóa dịch cho **282 lượt thoại có chữ, tương ứng 281 câu khác nhau** trong `VoiceLines`; 12 âm thanh không lời không cần phụ đề. Bổ sung thêm các lựa chọn hội thoại và nhãn thống kê còn thiếu được bộ trích xuất phát hiện. Catalog hiện có **8.308 mục**. Đây là kiểm tra độ phủ khóa, không phải tuyên bố đã biên tập thủ công toàn bộ lời thoại của mọi nhân vật.

[LocalizationChecks](Verification/LocalizationChecks.txt) đạt **601 assertion**, gồm kiểm tra từng câu thoại có khóa riêng, runtime trả đúng bản dịch không lẫn delivery note, câu báo lỗi được dịch đúng và English giữ nguyên. [Smoke native](Verification/ChatLocalization/NativeChecks.txt) dùng đường `SpeechBubbles` thật trong cửa hàng, xác nhận câu này ở tiếng Việt, chuyển sang tiếng Anh rồi về tiếng Việt, không ghi nhận runtime error. Không điều khiển chuột, bàn phím hoặc focus hệ điều hành. Để lặp lại, dùng audit player và `Tools/Review/verify_chat_localization.py`, đặt `BRIGADE_AUDIT_BRIDGE` trỏ tới bridge riêng.

Ảnh native: [câu thoại tiếng Việt trong cửa hàng](Verification/ChatLocalization/vi-merchant-reroll.png). Windows build mới: `Builds/Localization/ArcaneBrigade.exe`; [build report](Verification/LocalizationBuild.txt).
