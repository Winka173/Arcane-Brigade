# Visual quality — hướng 1

Ngày: 2026-10-13. Chọn chất lượng game/render 4K, không ép mỗi texture thành 4096px và không đổi gameplay K3.

## Thay đổi

- Sáu body và tám vũ khí được vẽ lại, silhouette đơn giản, màu mạnh, không cánh tay; bàn tay/vũ khí vẫn theo presentation hiện có. Nguồn thật 1672×941 RGBA, tách sprite theo vùng alpha; không upscale để giả native 4K. Một số unit vẫn dùng chung body, chưa phải toàn bộ roster có art độc lập.
- Asset mới: `Assets/ArcaneBrigade/Art/QualityHD/Characters_v2.png`, `Weapons_v2.png`, `FloorTile_v2.png`. Giữ nguyên asset cũ để đối chiếu. Import không nén mất dữ liệu, không giữ CPU copy cho sprite, bilinear; tile dùng mip/trilinear/aniso và mirrored wrap.
- Map 34×22 giữ biên gameplay, tile rộng 6 world units (~279 texels/world); không kéo một bitmap 1672px phủ toàn map. Viền đá nhiều lớp dùng mesh native, không dựa vào PNG nhỏ. Shader giảm độ chi tiết tương phản nền để nhân vật, đạn và loot nổi bật; tăng saturation vừa phải.
- Combat view height 5.4 → 3.9: kích thước hiển thị +38.46%. Rest 6 → 4.8: +25%. Camera vẫn follow/clamp; phải di chuyển mới tới rìa map.
- Vòng hiệu ứng 72 đoạn thay 24, biên alpha mềm, easing bán kính/opacity, glyph phép, màu nguyên tố sáng hơn; projectile có đầu/viền mượt hơn. Không đổi damage hoặc cooldown. Không tuyên bố mọi animation/effect đã đạt production.
- UI teal/purple đậm hơn; giá quầy có bảng nền tối bo góc, reroll đặt chữ bên cạnh biểu tượng. Vẫn là ba quầy vật lý và Space/USE, không chuyển thành modal shop.
- Feedback font/UI chưa đẹp đã được nhận; chủ dự án đồng ý để redesign ở phase sau. Font hiện tại giữ nguyên. K6 dựng visual system đồng nhất, K7 nghiệm thu glyph/kerning/transitions; pass này không tự nhận font/UI đã đạt chất lượng cuối.
- Desktop MSAA 4×, mobile 2×; cửa sổ PC đổi kích thước được. Không ép Android/iOS chạy 4K. Device/performance gate vẫn ở K7.

## Kiểm chứng

`PreviewExports/VisualQuality/Checks.txt`: 31 checks PASS. Regression chạy qua K3 final: Checkpoint 17, Squad 12, FullRun 96, K0/K1 27, K2 22, transaction 36, modifier 29, altar 14, K3 final 31. Build Windows thành công, 0 errors / 0 warnings.

`Gameplay_Contract.txt` xác nhận IDs, health, damage, speed, cooldown, range, contact, tier, economy, modifiers/procs giữ nguyên. Camera, sprite size/pivot và visual muzzle được thay; không khẳng định đường bay pixel-identical. Save schema và luật mua/merge không đổi.

Native Windows D3D11 capture: `PreviewExports/VisualQuality/Player4K/`. Mỗi PNG được assert **3840×2160** ngay trong Unity. Máy hiện chạy cửa sổ **1920×1080**, Unity `CaptureScreenshotAsTexture(2)` render supersize 2; không resize ảnh ngoài Unity. Đây là bằng chứng render/capture 4K, **không phải bằng chứng chạy gameplay realtime 4K 60 FPS**. Số timing trong `Capture_Notes.txt` chỉ áp dụng live resolution và fixture nhẹ.

Roster/HP/enemy/loot/shop được seed để kiểm tra hình ảnh; enemy AI đóng băng, autocombat/effects chạy bằng simulation thật. Save diagnostic tách riêng. Tám ảnh effects là các frame lấy mẫu có timestamp, không phải video mượt 60 FPS. Gallery HTML không nội suy frame.

## Research và giới hạn

Đối chiếu silhouette, nền lạnh yên, accent nóng và readability từ [Bizarre Brigade screenshot/event guide](https://www.sportskeeda.com/esports/zenless-zone-zero-zzz-don-t-walk-alone-event-guide) và [arcade guide](https://www.icy-veins.com/zenless-zone-zero/bizarre-brigade). Screenshot combat cũ không chứng minh chính xác thông số bản 1.5+. Audit gameplay 1.5+ vẫn theo `Bizarre_Brigade_15_Reference_Audit.md`.

Asset mới là nhân vật/vũ khí riêng của dự án, không đưa screenshot/nhân vật/logo reference vào bản game. Prompt brief được lưu ở `Visual_Quality_Generation_Prompts.md`. Chưa hoàn tất art riêng mọi enemy/boss/item, soundtrack, mobile hoặc K4; đây là pass nâng chất lượng, không chứng nhận production.

## Chạy lại

Unity 6000.4.8f1: `Arcane Brigade > Visual Quality > Apply HD assets map and check build`. Builder remap art trên dữ liệu đang có, không dựng baseline. Chỉ kiểm tra/build: `Check existing visual pass and build`.

Player: `Builds/FullRun/ArcaneBrigade.exe`. Capture diagnostic: `-arcane-quality-capture -capturePath "<absolute output path>" -force-d3d11 -screen-width 3840 -screen-height 2160 -screen-fullscreen 0`. Build/cache không commit; source, import settings, scenes và bằng chứng capture có trong Git/LFS.
