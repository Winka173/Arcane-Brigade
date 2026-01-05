# Checkpoint 01 — combat và prefab modular

Ngày: 2026-10-13. Editor: Unity 6000.4.8f1. Dữ liệu: `checkpoint-0.1`, seed mặc định `131026`.

## Phạm vi đã làm

- Boot → Frontend → Arena; một wave 60 giây có thắng/thua, pause, mất focus tự pause, chơi lại và về menu.
- Chỉ di chuyển: WASD/mũi tên, touch kéo góc dưới trái. Input chuyển thành một vector Move; simulation không đọc phím.
- Fixed-step 60 Hz, render nội suy. RNG spawn/combat tách nhau; đây không phải cam kết deterministic bit-identical giữa nền tảng.
- Champion auto kiếm, một đồng minh auto trượng; jelly áp sát và jelly bắn nỏ. Thông số tune trong ScriptableObject, không phụ thuộc animation.
- Attack có windup → impact → recovery. Đạn kiểm tra swept segment, tránh xuyên mục tiêu do bước thời gian lớn.
- Đồng minh bất tử, đi gần champion, không chặn đạn. Nhiều nguồn damage có thể cùng trúng; cooldown contact theo từng nguồn, không có bất tử toàn cục.
- Địch chết rơi vàng, nhặt khi đến gần. Chưa có hệ kinh tế/shop dùng vàng.
- Prefab body riêng, `GripRoot/WeaponVisual`, `HandVisual`, `MuzzleMarker`; không có arm bone. Gallery camera kiểm tra 3 thân × 3 vũ khí.
- Bob/squash/swing/recoil procedural, sorting theo vị trí/hướng, bóng chân, spark impact/kill/pickup có pool giới hạn riêng và âm thanh tone thử. Giới hạn VFX không ảnh hưởng damage.
- Pool hiển thị unit/projectile/pickup; simulation vẫn dùng list và allocation thông thường, chưa spatial index hoặc benchmark đội quân lớn.

## Chạy và dựng lại

Mở `Assets/ArcaneBrigade/Scenes/Boot.unity` rồi Play, hoặc chạy `Builds/Checkpoint01/ArcaneBrigade.exe` cùng toàn bộ thư mục đi kèm. ESC/Pause tạm dừng.

Editor menu:

- `Arcane Brigade > Checkpoint > Build assets, check and capture`: tái import atlas, dựng scene/prefab/catalog baseline, smoke check và capture. **Lệnh này ghi lại các thông số baseline và scene checkpoint; không dùng sau khi đã tune/đổi scene nếu muốn giữ thay đổi đó.**
- `Arcane Brigade > Checkpoint > Build Windows playable`: build các scene đã lưu, không reset tuning.

Art nằm trong `Assets/ArcaneBrigade/Art/Checkpoint/`; dữ liệu trong `Assets/ArcaneBrigade/Data/Checkpoint/`; code gameplay ngoài `StylePreview/`.

Atlas modular là artwork gốc sinh bằng **painting pass**, dựa trên mẫu style đã duyệt, không trích sprite của Bizarre Brigade. Prompt đầy đủ: `Docs/Checkpoint_Asset_Prompt.txt`. PNG nguồn giữ nguyên, importer tách cell và trim alpha; disc/square là primitive do C# tạo.

## Bằng chứng kiểm thử

`PreviewExports/Checkpoint01_Checks.txt` ghi kết quả thực tế của smoke checks Editor: khởi tạo, diagonal speed, bounds, pause, bất tử đồng minh, damage cộng dồn, đạn xuyên qua đồng minh, contact cooldown, kill/pickup, một impact mỗi attack, leash, defeat/restart/victory và hierarchy/import sprite.

`PreviewExports/Checkpoint01_Build.txt` ghi kết quả build Windows. Log compile/build chi tiết trong `Logs/` (git-ignored).

Kết quả đã chạy: **17/17 smoke checks Editor PASS**, capture Editor thoát code 0, Windows x64 build thành công và executable chạy headless smoke **PASS, exit 0**. Báo cáo player được lưu trong `PreviewExports/Checkpoint01_PlayerSmoke.txt`; kiểm tra này dùng NullGfxDevice, không chứng minh chất lượng render/performance của bản PC.

`CheckpointPlayerSmoke` chỉ hoạt động khi có cờ `-arcane-smoke`: vào Arena, chờ 3 giây, kiểm tra simulation tiến và mọi SpriteRenderer active có sprite, rồi thoát với code 0/1. Đây là smoke test bản executable, không phải test chơi tay hoặc test FPS.

Ảnh `PreviewExports/Unity_Checkpoint01_Arena.png` render bằng **Unity Camera.Render**, simulation được bước thực trong Editor. Chữ HUD trên ảnh là label capture, không phải ảnh chụp UI OnGUI của bản chạy. `Unity_Checkpoint01_Modular.png` là gallery ghép asset trong Unity.

## Quy tắc tạm, chưa được coi là reference chính thức

| Hạng mục | Trạng thái |
|---|---|
| Auto-target gần nhất, cone melee, crit 3% ×1.5 | `CHECKPOINT_TUNING`, không tuyên bố giống Bizarre Brigade |
| Leash 2.2, đội hình một ally, mua/roll vũ khí | Chỉ fixture đầu trận; chưa có recruitment |
| 60 giây, spawn tăng dần, 18 địch sống tối đa | Load budget cho checkpoint, **không phải giới hạn đồng minh** hay độ dài run chính thức |
| Trùng unit/weapon, ghép 3 bản, xử lý weapon khi ghép, sao tối đa | `NEEDS_REFERENCE`, chưa implement |
| Giá/shop pool, reroll, công thức lãi, item/block đạn/roam | `NEEDS_REFERENCE`, chưa implement |
| Bond, 4 nguyên tố/status/reaction, altar/merge mở rộng | Chưa implement |

Không thêm kỹ năng chủ động, phí nuôi, bán/khóa shop hoặc giới hạn slot đồng minh.

## Chưa nghiệm thu production

Đây là nền combat để kiểm tra hướng chuyển động/art, **không phải chất lượng launch đã đạt**. Chưa có shop/ghép/bond/save/progression/boss/Endless/run 30–40 phút, animation vẽ tay, telegraph đầy đủ, UI production, localization, accessibility, nhạc hoặc Steam integration.

Hiện dùng built-in renderer, legacy input và OnGUI thử. Android/iOS chưa build/chạy thiết bị; máy hiện thiếu module Android/iOS. Chưa kiểm notch/safe-area HUD, thermal, bộ nhớ/FPS mobile; iOS còn cần macOS/Xcode/signing. Windows standalone không đồng nghĩa Steam release-ready.

Checkpoint tiếp: xác minh reference cho recruitment/shop/ghép, thêm đội quân và stat/bond pipeline, nâng input/UI/telegraph; song song kiểm chứng performance trước khi tăng content. Không bắt đầu sản xuất art hàng loạt trước khi motion/readability trong trận được duyệt.
