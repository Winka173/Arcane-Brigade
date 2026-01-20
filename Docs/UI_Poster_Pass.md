# UI "Midnight Carnival Poster" pass (2026-10-14)

Restyle toàn bộ UI IMGUI theo hướng poster lễ hội đêm (cảm hứng ngôn ngữ đồ họa ZZZ: panel cắt xéo, sticker, halftone), đồng bộ với soundtrack "Neon Carnival Funk". Không đổi gameplay, save hay layout contract đã có test.

## Thay đổi
- **Skin chung** (`Runtime/UI/ArcadeSkin.cs`): nút vé nghiêng có bóng đổ cứng nướng sẵn trong texture; `Primary()` đỏ cho hành động chính (NEW RUN, START, BUY, RESUME, PLAY AGAIN, CONTINUE); panel mực có viền kem, tab xéo và halftone (chỉ panel lớn); `Slant/Plate/Glow/Focus`; màu độ hiếm `RarityColor`; slider theo skin. Focus bàn phím/tay cầm = chevron + gạch teal.
- **Trong trận**: nền sàn tối/dịu hơn (`FloorHD.mat` _Color 0.78/0.82/0.88, _Saturation 1.02, đồng bộ `VisualQualityBuilder`); đạn địch một màu nguy hiểm đỏ hồng riêng; hit flash giới hạn + nhấp nháy (không còn bóng trắng đặc); số damage: đòn thường nhỏ/ngắn, gộp 0.3 s, tối đa 12 cùng lúc, chí mạng to và lâu, sát thương nhận vào luôn hiện.
- **HUD**: cụm máu/tiền/trứng có plate, wave + đồng hồ có nhãn TIME (vàng khi ≤5 s, đỏ khi boss overtime), thanh máu boss có tên + vạch phase 50%, plate cho số bond, băng rôn WAVE CLEARED/kết quả.
- **Menu**: logo display có bóng đỏ, tag "NIGHT CARNIVAL", NEW RUN nổi bật, RESUME ghi rõ wave hoặc "NO SAVED RUN", nhân vật lớn dưới spotlight; trang con dùng nền minh họa làm tối thay cho khối phẳng, tiêu đề theo trang.
- **Chọn nhân vật**: cột chi tiết có panel/spotlight, tên font display, vũ khí dạng tag, mô tả cao hơn (ít cuộn), START đỏ/LOCKED, nhãn "RECRUIT THEMES", thẻ khóa ghi "LOCKED · WAVE n".
- **Shop**: tooltip có dải + tag độ hiếm (COMMON/UNCOMMON/RARE/MYTHIC) hoặc COMPANION.
- **Bảng Brigade**: tab xéo có số đếm, chữ giải thích 23px, dòng chọn có vạch teal; gạch chân glossary chỉ hiện khi rê chuột.

## Kiểm tra
- `K13Builder.VerifyAndBuild` (K11/K12/K12Polish/MarketPolish/K13 checks + Windows build): PASS, 0 lỗi/cảnh báo, trước hai chỉnh literal cuối (ngưỡng trang trí panel, trả độ đậm bóng về giá trị của `ShadowChecks`).
- Native capture `-arcane-k7-capture -arcane-k13`: 58 PASS; FAIL duy nhất là kích thước cửa sổ 4K của màn hình máy chụp (3840x3240), không liên quan UI. Ảnh: `PreviewExports/UIPoster/`.
- Chưa playtest bằng tay; chưa kiểm thử touch/mobile.
