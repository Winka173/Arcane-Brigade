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

## Pass 2 — sân khấu sống (2026-10-14)
- **Menu chính diegetic** (`RunMenu.DrawLanding`, `SpotAreas`): các mục là đồ vật trong tranh LostShop_v1 — nhân vật trên bàn = NEW RUN, ô cửa vòm = RESUME, lọ kính = COLLECTION, máy in biên lai = RECORDS, đèn trăng = SETTINGS, chuông = HOW TO PLAY. Hover/focus: spotlight + vé nhãn + gợi ý, nhân vật quay về phía đồ vật, đồ vật lấp lánh lần lượt theo nhịp. Hàng nút dưới đáy ánh xạ 1-1 cho bàn phím/tay cầm (thứ tự giữ nguyên, test điều hướng K7 PASS).
- **Vé giá treo** (`WorldShop.DrawPriceTags`): dây treo, đung đưa, nảy theo nhịp, rung mạnh khi bị từ chối, giá đỏ khi không đủ tiền, tag theo màu độ hiếm; mua xong dập dấu SOLD rồi rơi.
- **So sánh trước khi mua**: cột "IF YOU BUY" trong tooltip dùng `RunOfferPreview.Capture` (dữ liệu mô phỏng thật, cache theo trạng thái build), ▲ xanh/▼ đỏ; companion hiện kích thước đội.
- **Bond = "TONIGHT'S PROGRAM"**: thẻ giấy góc phải, mỗi bond một dòng với icon, tên, pip tới mốc kế tiếp, con dấu LVn; hover/click mở mô tả.
- **Máu thấp**: viền đỏ thở theo nhịp (`LowHealthVignette`) + `ArcadeMusic.Danger` hạ lớp melody/intensity để còn nhịp và tiếng tim (không dùng filter vì SFX dùng chung GameObject).
- **Hướng dẫn trong trận** (`FirstRunCoach`): 6 bong bóng từ nhân vật (di chuyển, tự đánh, nhặt xu, mua, ghép 3, xuống cầu thang), theo thiết bị; xong (hoặc tới wave 3) ghi `PlayerProfile.TutorialSeen` nên các lần sau không hiện. Tắt trong chế độ capture trừ khi có `-arcane-coach`.
- **Cut-in sân khấu** (`StageCutIn`): boss vào sàn, hạ boss ("SHOWSTOPPER!"), đồng minh lên 3★, bond lên cấp — dải poster quét ngang 1,35 s, không dừng game; Reduced Motion bỏ chuyển động trượt.
- **UI theo nhịp** (`BeatPulse` từ `ArcadeMusic.BeatOriginDsp/BeatSeconds`): khung wave, logo, vé giá, thẻ program, đồ vật menu nảy theo phách; số wave đóng dấu khi wave bắt đầu (`WaveStamp`).

Kiểm tra pass 2: Runtime biên dịch sạch; build Windows 0 lỗi/cảnh báo; K11/K12Polish/MarketPolish/K13/ArcaneAudio checks PASS; capture 58 PASS (FAIL duy nhất vẫn là cửa sổ 4K của máy chụp). `K12Checks` đang dừng ở dữ liệu K14 đang làm dở ("Two exclusive dream recruits"); `K8UIChecks` cũ còn đòi chuỗi tiếng Việt từ trước K11. Cut-in, viền máu thấp, dấu số wave và bong bóng hướng dẫn chưa có ảnh chụp tự động — cần xem khi chơi.
