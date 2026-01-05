# K0 — hợp đồng dữ liệu và quyết định

Ngày 2026-10-13. Baseline: `PreviewExports/K0/Source_Before_K1.zip`, manifest SHA-256 và regression 96 PASS trong cùng thư mục. Archive không chứa Library hoặc save player. Không gọi lại FullRunBuilder.BuildAndCheck để nâng cấp: builder đó reset catalog cũ.

## Hợp đồng

- Unit/weapon/item/recipe/bond IDs hiện có bất biến. Thêm content bằng ID mới; không đổi ID để thay art. UnitDefinition/WeaponDefinition là asset authoring; PrepareCombat clone, không ghi stat vào asset gốc.
- Baseline K0: RunSnapshot.SchemaRevision=1 độc lập catalog.Version. Save legacy revision 0/missing vẫn nhận, giữ ID/seed/two RNG/counters. K3.1 nâng runtime lên revision 2, thêm receipts/reroll counter và nhận revision 0/1; không sửa archive baseline. Từ chối future revision; không silently reset. SpecialOffers missing thành list rỗng; legacy excess shop offers giữ migration cũ.
- RunStorage giữ envelope checksum, atomic replace và backup. Diagnostic dùng một root riêng chung frontend/arena trong process. Overwrite từ menu phải xác nhận, copy cả primary/backup sang ArchivedRuns trước khi launch; lỗi backup chặn launch.
- Stats hiện tại: additive item stacks và bond threshold activations; tier và Rally multiplicative theo runtime. Scope hiển thị từ ItemEffect thực, không thêm proc/rarity giả. Details là read model không tiêu thụ RNG, không clone/modify combat.
- Loot K2 có stable runtime ID, giá trị, spawn time, previous/current position, scatter velocity và trạng thái attraction. Một ID credit một lần khi logical collect; animation không gọi Save/Gold. K3 mới mở rộng chest/egg/luck.
- Event K2 bổ sung subject ID, phân biệt nạn nhân với faction nguồn Kill; boss/player death cũng có Kill event. Đây là foundation attribution, không tự nhận đã có contribution registry/proc system K3.
- Gameplay dùng fixed simulation time. Presentation interpolates và có clock riêng không đổi combat/timer/RNG. Pause ownership: manual, Details, navigation, focus, background; đóng một lớp không nhả pause của lớp khác.
- Art: 2D silhouette gọn, không cánh tay, grip chứa hand + weapon. Floor/boundary tách riêng; font body/heading/numbers, OFL nguyên bản kèm build. K2 chết không để grip sống vô hạn hoặc body còn hurtbox.

## Decision register

| Rule | Hiện trạng | Trước khi đổi |
|---|---|---|
| Altar HP current/max, giá/chance theo eggs | K3.2b dùng current HP và chance theo eggs, AUTHORED tuning; chưa xác minh công thức reference | Chi tiết K3_Altar_Economy.md; không coi tuning là parity |
| Merge weapon inheritance | Giữ code hiện có | Xác minh reference trước K3/K4; không cho chọn weapon |
| Rarity, weighted pool, reroll curve, pricing | Prototype catalog giá cố định | K3 AUTHORED tuning nếu thiếu evidence |
| Interest divisor/cap | Prototype 10 / 15 | Hiển thị đúng số runtime, không gọi là công thức reference |
| Normal hall duration / Endless / boss scaling | 20×84s, curve cũ | K5/K6 playtest và quyết định riêng |
| Typography identity | Noto nền, có license và tiếng Việt | K7 lettering độc quyền; không gọi font nền là final art |
| Death/coin timings | K2 AUTHORED, không đo từ video reference | Không sao chép asset/audio độc quyền; nghiệm thu feel riêng |
| N02/N17/N18 | Chưa có contribution/proc/receipt/loot đủ | K3 dùng event/state thật; không vẽ graph hoặc marker giả |

## Fixtures / rollback

`K1Builder.CheckExisting` chạy regression và fixtures schema legacy/future, corrupt primary → backup, write failure, read-only Details, large roster và pause ownership. Fixture roots ở temporaryCachePath, không đụng persistentDataPath player. Baseline manifest không overwrite được bằng Capture lần hai. Archive là backup có thể phục hồi thủ công; không auto rollback vào source đang làm.
