# Arcane Brigade — danh sách art tổng (cho agent vẽ)

**Bổ sung UI và thoại 17/10:** đã nối 49 icon stat phối từ art vẽ tay hiện có và 198 lời bình riêng cho 66 companion khi mua đồ, mở bond, ghép sao. Atlas 49 icon vẽ mới **còn chờ hạn mức công cụ ảnh**, không tính vào 261 PNG đã giao. Chi tiết và nghiệm thu: [Painted_Stats_And_Companion_Reactions.md](Painted_Stats_And_Companion_Reactions.md).

**Hoàn tất art 17/10:** đã giao đủ 261/261 PNG, gồm 24 frame HD Customer bổ sung và hai atlas Hob/Tally đã sửa mép. Cả năm bộ boss dùng 16 frame độ phân giải riêng mỗi bộ. Chi tiết, nguồn ảnh và kiểm tra: [Art_Master_Runtime_Pass.md](Art_Master_Runtime_Pass.md).

Cập nhật 17/10/2026. Đây là **danh sách duy nhất cần làm**: gom yêu cầu cốt truyện Hồi 1, các art hiện có bị đánh giá **chưa khớp cốt truyện** (vẽ lại), token cơ chế hero và hiệu ứng chiến đấu. Canon: [Mien_Cho_Act_One_Story.md](Mien_Cho_Act_One_Story.md). Bản cũ [Act_One_Story_Art_Requests.md](Act_One_Story_Art_Requests.md) đã được gộp vào đây.

Ký hiệu: **MỚI** = chưa có; **VẼ LẠI** = có nhưng không khớp truyện/style, thay hẳn; **SỬA** = edit từ sprite hiện có (giữ camera, tỉ lệ, chân đứng); **GIỮ** = đã đánh giá, không cần làm.

## Quy tắc chung

1. Mọi prompt vật thể bắt đầu bằng **prompt chung** dưới đây, chỉ thay dòng `Subject:`.
2. Tham chiếu style: `Assets/ArcaneBrigade/Art/FinalQuality/furniture-00_v1.png`, `Assets/ArcaneBrigade/Art/AnimationHD/NightMarket_v1.png`.
3. Không vẽ chữ vào ảnh (biên nhận, bảng, thư để trống — UI viết chữ). Không máu me, không cảnh làm hại trẻ em; nỗi sợ đến từ ẩn ý: bụi, sự bất động, chiếc ghế, cánh cửa, đôi giày nhỏ.
4. Đồng hành là sinh vật sống, **không phải đồ chơi**. Những Người Đã Ngồi Xuống là lữ khách cũ, **không phải quái**.
5. Màu: đỏ ember chỉ dành cho thứ thù địch; phe ta dùng đồng thau, hổ phách, xanh teal, kem.
6. Giao file: sprite đơn vào `Assets/ArcaneBrigade/Resources/MienCho/ActOne/` (importer có sẵn biến mọi PNG ở đó thành sprite PPU 256); atlas boss vào `Assets/ArcaneBrigade/Art/ActOneBosses/`; token hero và VFX theo đường dẫn ghi ở mục đó. Ghi mỗi ảnh vào `Docs/Act_One_Story_Image_Prompts.json` cùng định dạng `Docs/Act_One_Image_Prompts.json` (`name, path, source, prompt, transparent, sha256, size`).
7. Khung hình animation: vẽ một sprite gốc được duyệt trước, rồi edit ra từng frame từ sprite đó.

### Prompt chung

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: [MÔ TẢ VẬT CẦN VẼ]

Style: dark fairytale storybook illustration; confident dark ink
contours; rounded, slightly crooked and asymmetric forms; ornate
antique decoration; rich painterly shading; warm brass gold,
burgundy velvet, deep teal and midnight blue; cream highlights.
Readable silhouette and simplified detail at gameplay size.
Elevated front three-quarter view matching the reference sprites.

Output: one complete isolated object, genuine transparent RGBA
background, centered with 10–12% transparent padding.
No cropping, text, UI, background, ground plane, baked floor shadow,
external halo, or wind/smoke beneath the object.
Avoid photorealism, Blender/3D-render appearance, PBR and shiny plastic.

For damaged variants or animation frames, edit the approved sprite.
Preserve its design, camera, scale, silhouette alignment and foot
anchor; change only the requested damage or pose.
```

---

## Ưu tiên 0 — nhân vật và đồ vật cốt truyện (game đang dùng hình tạm)

### 0.1 Chiếc Ghế Ấm — MỚI (đang tạm dùng ghế bành đỏ `EmptyChair.png`, sai truyện: canon là ghế gỗ nhỏ cạnh giường trẻ con)

| File | Subject |
| --- | --- |
| `WarmChair.png` | A small, plain child's wooden bedside chair, slightly crooked, worn honey-brown wood with one repaired leg, a folded burgundy-and-cream quilted blanket on the seat, turned a little toward the viewer. Humble and homely, not ornate — clearly different from the ornate shop furniture. A faint warm tone painted into the seat wood, no glow halo. |
| `WarmChair_Coins.png` | SỬA từ `WarmChair.png`: three odd antique coins of different sizes resting on the folded blanket. Nothing else changes. |
| `WarmChair_Jar.png` | SỬA: a small stoppered glass jar on the blanket with a faint pale ripple inside like a trapped whisper. |
| `WarmChair_Phone.png` | SỬA: an old telephone receiver lying on the blanket, its curled cord trailing off the seat edge and ending in nothing. |

### 0.2 Những Người Đã Ngồi Xuống — MỚI

| File | Subject |
| --- | --- |
| `SeatedOne_A.png` | A former traveller sitting perfectly still on their own tall dining chair, seen from behind and slightly to the side, facing away. A gentle rounded storybook dream-form (proportions like the playable heroes), no face visible, coat and shoulders covered in thick grey dust, hands in the lap. Melancholy, eerie stillness. |
| `SeatedOne_B.png` | Same family: a small rounded traveller on a station bench seat, a suitcase at their feet, dusty, facing away. |
| `SeatedOne_C.png` | Same family: a tall thin traveller on a rocking chair that does not rock, a shawl gone grey with dust, facing away. |
| `SeatedOne_D.png` | Same family: a traveller on a low footstool, knees drawn up, a hat in their hands with dust on the brim, facing away. |
| `SeatedOne_E.png` | Same family: a traveller in a worn office chair, one shoe missing, dust, facing away. |
| `SeatedOne_A_Turned.png` | SỬA từ `SeatedOne_A.png`: only the head turned over the shoulder toward the viewer; the face is a soft featureless dusty oval with two faint dark hollows. Used once in the whole act. |

### 0.3 Last Customer — SỬA (art hiện có: áo choàng dài, mặt trống — hợp; thiếu mũ và “khuôn mặt không chịu đứng yên”)

Sprite gốc: `Assets/ArcaneBrigade/Art/Nightmare/LastCustomerIdle_v4.png` và mọi frame trong `NightmareAssets.CustomerFrames`. Mặt trống hiện tại **giữ nguyên** làm trạng thái sau khi hắn thua.

| File | Subject |
| --- | --- |
| `LastCustomer_Pleasant.png` (+ cùng bộ frame) | Edit: add a tall antique hat with a brass band; replace the blank face with a pleasant, polite, slightly too-smooth face that looks like a mask, its features subtly doubled as if two faces were overlapping. Coat, scroll, bell and pose unchanged. |
| `LastCustomer_BorrowedFace.png` (+ cùng bộ frame) | Edit of the Pleasant version: the face replaced by a kind, gentle, rain-damp face that clearly does not belong to him — slightly too small for his head, edges not quite meeting. |

### 0.4 Người bán hàng — VẼ LẠI (art hiện có: quái khổng lồ nhiều mắt, cười toe, lưỡi là cuộn hoá đơn — đọc như kẻ dụ khách hội chợ; canon là người tử tế, mệt mỏi, sợ bị bỏ lại, không bao giờ nhìn về phía chiếc ghế)

Thay `Assets/ArcaneBrigade/Art/K11/GiantGoldMerchant_v1.png` (atlas 4 frame giống bản cũ, cùng kích thước khung, pivot đáy). Giữ quy mô khổng lồ và chất “cửa tiệm biết sống”.

| Frame | Subject |
| --- | --- |
| 1 (idle) | A huge, gentle shopkeeper creature built like an old shop counter come to life: a rounded patched velvet body in burgundy and teal, brass drawers and a cash-till belly, several small tired kind eyes behind round spectacles, a soft closed-mouth smile, one big hand holding a steaming teacup, the other resting on the counter. Its gaze is lowered and turned slightly away to its left (never toward the viewer's right corner where the chair is). Warm, weary, protective. |
| 2 | Same, pouring tea from a small brass kettle. |
| 3 | Same, wiping a glass with a cloth, eyes still turned away. |
| 4 | Same, a worried look, one hand raised a little as if to say “not that one”. |

### 0.5 Bốn đồng hành cốt truyện — sprite riêng cho slot cốt truyện (đồng hành mua ở shop cùng loài giữ hình cũ)

| File | Loại | Subject |
| --- | --- | --- |
| `Hob.png` (+ frame như `TeapotPilgrim`) | SỬA từ TeapotPilgrim | Hob the hearth spirit: the teapot companion with a soft ember glow inside its spout and lid, a small tea towel over one arm, a tiny cup in its hand. Same body, pose and anchor. |
| `Latch.png` (+ frame như `HoodWisp`) | SỬA từ HoodWisp | Latch, the Keeper's guard: the hooded wisp now wearing a small brass gorget with a keyhole emblem (matching the Keeper's keyhole), a round dented shield strapped to its left side. |
| `Postscript.png` (+ 4 frame) | **VẼ LẠI** (snail hiện tại sai truyện) | Postscript the courier: a small creature folded out of an old cream envelope, its flap like a hood, a red wax seal for a heart, a tiny postman's cap, a satchel of unopened letters, little paper feet. Friendly, earnest. |
| `Tally.png` (+ frame như `PaperDevil`) | SỬA từ PaperDevil | Tally the former hunter: the origami paper devil with neat carved tally marks in groups of five across its body and wings like cuts in old paper, a small hunter's net folded at its hip. |

### 0.6 Điện thoại — GIỮ (`Resources/MienCho/Encounters/BreathingPhone.png` hợp style; code sẽ đặt nó trên quầy ở shop 8)

---

## Ưu tiên 1 — boss cốt truyện (wave 10, 15 và trận ẩn đang dùng thân tạm)

Mỗi boss: một sprite gốc được duyệt, rồi các frame edit từ đó theo đúng số lượng và bố cục frame của Keeper (`OutsideStageAssets.BossFrames`, 16 frame). Keeper (bàn tay khổng lồ có mắt ổ khoá) **GIỮ** — rất hợp với “từng kéo lữ khách đứng dậy”.

| File gốc | Subject |
| --- | --- |
| `ChairPorter.png` | THE CHAIR PORTER (wave 10): a tall, stooped, spindly porter creature in a faded railway porter's coat and cap, long arms, carrying on its back a teetering tower of mismatched chairs strapped with luggage straps; small dusty travellers sit on several chairs facing away; the chair at the very top is empty. Weary, menacing, enormous. |
| `DoorThatStayedShut.png` | THE DOOR THAT STAYED SHUT (wave 15): a living front door of a family house standing upright: painted teal wood, brass letterbox slot forming a grim mouth, two heavy door chains swinging like arms, a brass knocker, a peephole like an eye, small pencil height marks on the lower frame, rooted on its threshold step. |
| `TheBedside.png` | THE BEDSIDE (trận ẩn): the warm chair grown huge and wrong: the small wooden bedside chair stretched tall like a figure, its folded blanket draped like a shawl over a back that curves forward as if leaning closer, chair legs bent like long knees, a soft dark hollow where a face would be. Tender and terrifying. |

## Ưu tiên 1 — bốn vùng map và đồ cảnh

Sàn là texture lặp liền (không phải vật thể): thay khối `Output` bằng
`Output: seamless tileable square floor texture, viewed from above at the same angle as the existing arena floors (Art/K13/CarnivalStone_v1.png, Art/October/NightmareFloor.png), 2048x2048, no objects, no shadows, no text.`

| File | Subject |
| --- | --- |
| `Floor_Crossing.png` | Worn antique shop-front flagstones with inlaid brass lines, teal and midnight tones (waves 1–5). |
| `Floor_Platform.png` | Old railway platform boards with a faded cream safety edge stripe and scattered dust (waves 6–10). |
| `Floor_House.png` | A childhood home's wooden floor with a worn burgundy runner rug, scuffs and small footprints in dust (waves 11–15). |
| `Floor_PerfectRoom.png` | An immaculate soft cream bedroom carpet, too clean, faint regular pattern (waves 16–20). |
| `Floor_NightYouHid.png` | A child's bedroom floor at night: wooden boards, a small rug, a sliver of light under where a door would be (trận ẩn). |

Đồ cảnh (prompt chung):

| File | Subject |
| --- | --- |
| `Prop_BoundaryGate.png` | Ornate brass boundary gate in a stone arch with a blank notice board bolted to it. |
| `Prop_PlatformBench.png` | A long empty station bench with iron legs. |
| `Prop_DepartureBoard.png` | A hanging antique departure board with blank flap rows. |
| `Prop_PhoneBooth.png` | An antique wooden platform telephone booth, door ajar. |
| `Prop_DiningTable.png` | A family dining table set for two: one bowl with steam, one empty place before an empty chair. |
| `Prop_NailedLetterbox.png` | A wall letterbox nailed shut with crooked nails. |
| `Prop_InwardWindow.png` | A house window frame whose glass shows the room's own interior instead of the outside. |
| `Prop_FrontDoorMarks.png` | A closed teal front door with pencil height marks on the frame and a chain lock (cảnh vật, không phải boss). |
| `Prop_SingleBed.png` | A perfect child's single bed, crisp sheets, one pillow. |
| `Prop_Nightstand.png` | A nightstand with one cup of water at exactly the right place. |
| `Prop_ReceiptChair.png` | The Customer's throne: a chair built from layered paper receipts and brass tacks. |
| `Prop_Ledger.png` | The Customer's huge open ledger on a lectern, blank pages. |

## Ưu tiên 2 — vật phẩm, biểu tượng ký ức, journal, menu

| File | Subject |
| --- | --- |
| `Item_SewnLetter.png` | An envelope opened and sewn shut again with coarse thread. |
| `Item_Umbrella.png` | A closed black umbrella, still wet, droplets on the canopy. |
| `Item_ThursdayShoes.png` | A small pair of child's shoes, laces already tied, side by side (relic thưởng trận ẩn). |
| `Item_Receipt.png` | A long curled blank paper receipt with a wax-red stamp mark. |
| `Item_ExitKey.png` | An old brass key with a blank tag. |

**Biểu tượng 7 chi tiết ký ức — MỚI** (để người chơi chọn chi tiết được trả lại khi thắng trận ẩn; ảnh vuông nhỏ 512×512, cùng bộ):

| File | Subject |
| --- | --- |
| `Memory_Voice.png` | An open antique music-box lid with a single curl of sound rising from it. |
| `Memory_Raincoat.png` | A folded mustard-yellow raincoat with a few raindrops on the collar. |
| `Memory_Height.png` | A tall figure's shadow bending to look through a letterbox slot. |
| `Memory_Rain.png` | A wool coat sleeve beaded with rain, a faint steam of warmth. |
| `Memory_Face.png` | A rain-fogged pane of glass with a soft blurred silhouette of a kind face behind it. |
| `Memory_Knock.png` | A gloved hand knuckling three gentle knocks on teal-painted wood. |
| `Memory_Handwriting.png` | An envelope with looping handwriting suggested by ink strokes (no readable letters). |

**Tranh journal — VẼ LẠI** (tranh hiện tại `WithheldDawn/WaitingRoom/OpenDawn.png` thuộc canon cũ). Cảnh 16:9, 1672×941, cùng chất liệu với `Resources/MienCho/WithheldDawn.png`; được vẽ nền đầy đủ, không chữ:

| File | Scene |
| --- | --- |
| `Panel_Crossing.png` | The shop interior at night; a small wooden chair with a blanket in a far corner, turned toward the viewer; eight lanterns beyond the window. |
| `Panel_Platform.png` | An endless station platform lined with dusty seated travellers facing the empty rails. |
| `Panel_House.png` | A childhood hallway, front door with height marks, rain light under the door, tiny shoes by the mat. |
| `Panel_PerfectRoom.png` | A perfect bedroom with one bed; dusty seated figures line the walls facing the bed; one empty chair. |
| `Panel_Dawn.png` | Real dawn over the shop roof; companions around an ordinary table; one plain chair pulled out. |
| `Panel_Thursday.png` | A morning doormat in a modern flat: an envelope with no stamp. |

**Menu — SỬA**: `Resources/MienCho/WaitingRoom_ChairTurned.png` — edit của `Resources/MienCho/WaitingRoom.png`: chỉ chiếc ghế bành đỏ bên trái xoay thẳng về phía người xem, đệm có vết lõm như vừa có người ngồi. Mọi thứ khác giữ nguyên từng điểm ảnh (hotspot menu ánh xạ theo bố cục này).

**Phòng Perfect Room (cảnh kéo Hob) — SỬA**: `Resources/MienCho/Journey/PerfectRoom_Hob_v1.png` — edit của `PerfectRoom_Painted_v1.png`: thêm Những Người Đã Ngồi Xuống dọc tường quay về giường, một ghế trống, và Hob (ấm trà đồng hành) ngồi trên một chiếc ghế của riêng nó, vai phủ bụi.

## Ưu tiên 3 — token cơ chế hero (8 ảnh)

Prompt đầy đủ đã viết sẵn trong [Hero_Mechanic_Art_Requests.md](Hero_Mechanic_Art_Requests.md); đường dẫn `Assets/ArcaneBrigade/Art/MechanicEffects/Tokens/{Name}_v1.png`, 1 frame mỗi ảnh; sau khi lưu chạy `ArcaneBrigade.Editor.HeroMechanicArtImporter.Import`.

| Token | Hero |
| --- | --- |
| Knot | Lantern Weaver |
| Puff | Copper Cauldron |
| Fang | Night Maw |
| Thorn | Thorn Treasurer |
| Patch | Patchwork Host |
| Dumpling | Supper Mortar |
| Seal | Postmaster / Ivory Sovereign / Treasure Acorn |
| Moon | Moon Prowler |

## Ưu tiên 3 — hiệu ứng chiến đấu: đạn quái, vũ khí phe ta, vụ nổ

Prompt đầy đủ (đúng prompt chung), kích thước, số frame, pivot và chỗ thay trong code: [Combat_VFX_Art_Requests.md](Combat_VFX_Art_Requests.md). Thư mục giao: `Assets/ArcaneBrigade/Art/CombatVFX/{Hostile,Allied,Juice}/`, mỗi frame một PNG. Đạn vẽ **hướng sang phải (+X)** vì code tự xoay theo hướng bay. Đạn quái phải đọc là thù địch ngay (đỏ ember), vẫn giữ chất vẽ tay.

Tổng 48 yêu cầu: **21 thù địch** (vẽ lại toàn bộ đạn quái), **22 phe ta**, **5 vụ nổ**. Rà soát 17 hình của quái (12 vẽ lại, gồm 2 hazard hiện **không hiện gì**), 44 hiệu ứng phe ta (19 GIỮ, 3 sửa bằng code, 21 VẼ LẠI).

Thứ tự nên vẽ:
1. **H1 đạn quái chung + H7 đạn boss** — hiện mọi phát bắn của quái và boss chỉ là chấm tròn trắng tô hồng.
2. **H16–H18 hazard Curtain / Office / Toy** — đang gây sát thương mà không vẽ gì.
3. **H8 đạn đột biến + H9 cầu lửa affix** — không có hướng bay (lộn vòng), lửa xanh trông như đạn băng phe ta.
4. **H11 chớp nòng của quái + H12 hiệu ứng hero trúng đòn** — quái đang bắn bằng chớp teal của phe ta.
5. **A1–A16 đạn phe ta** — 16/30 vũ khí bắn chỉ là chấm trắng tô màu; ưu tiên Ink Pistol, Snow Globe, Lantern Staff; mũi tên và cầu rune (`BowMagic_v2`) đang là vector phẳng lệch style.
6. **J1–J5 khung nổ** (chớp nổ phe ta/thù địch, vòng sóng, 6 mảnh văng, bụi mực) — thay bốn sprite sinh bằng code của `ExplosionFeedback`.

GIỮ: đạn giấc mơ, yo-yo, kim, các bộ impact Cut/Slam/Pierce/Orb, bụi, khói chết, chớp nòng phe ta, art trường build-morph (`Forms_v1`).

## Ưu tiên 2 — vật phẩm nhặt trong trận (nhịp chơi mới) — MỚI

Đang tạm dùng icon đèn lồng và icon relic Marching Bell. Giao vào `Assets/ArcaneBrigade/Resources/MienCho/ActOne/` (sprite đơn, importer có sẵn), 512×512, nhìn từ trên xuống 3/4 như đồng xu và rương trong trận; phải đọc rõ khi chỉ cao khoảng 60 px trên màn hình.

| File | Subject |
| --- | --- |
| `Pickup_LanternFlare.png` | A small brass hand-lantern with its shutter thrown open, a bright warm-white flame flaring inside and a few cream sparks escaping the top. Friendly warm light, not fire damage; no ember red. |
| `Pickup_ShopkeeperBell.png` | The shopkeeper's brass counter bell (round dome bell on a dark wooden base), polished and slightly dented, a tiny burgundy ribbon tied to the base. |

| `Form_TeaSpill.png` | VẼ LẠI ô "Porcelain" của `Art/MechanicEffects/Forms_v1.png`, vì chủ dự án thấy nó như bát đĩa xanh vỡ mỗi khi quái chết. Vẽ thành: a warm amber splash of spilt tea seen from above, a soft round stain with a few droplets and two thin curls of steam; cream highlights; at most two tiny cream china chips. No blue-and-white crockery, no plates or bowls. Vùng trong suốt ở giữa để đọc như vùng ảnh hưởng tròn. |

Ghi chú gameplay: Lantern Flare = nhặt vào thì đốt sạch quái nhỏ trên màn hình; chuông = mọi đồng xu trong sảnh chạy về người chơi.

---

## Không cần vẽ (đã bỏ khỏi Hồi 1 hoặc giữ nguyên)

- `Resources/MienCho/Encounters/ReservedTable.png`, `LateReflection.png` — cảnh bàn/gương canon cũ đã gỡ.
- `Resources/MienCho/EmptyChair.png` — chỉ còn là hình dự phòng khi chưa có `WarmChair.png`.
- `Resources/MienCho/Journey/Station_Painted_v1.png`, `FootstepHall_Painted_v1.png` — phòng đi lại cũ, hiện không dùng.
- Keeper (`Art/October/Keeper.png`), đèn lồng (`Resources/MienCho/Lanterns/*`), điện thoại, menu Phòng Chờ gốc, quầy dream shop và các phòng bí mật (nội dung phụ) — giữ.

---

## Ưu tiên 3b — token cơ chế hero sau đợt rework 17/10 (3 ảnh, MỚI)

Từ [Hero_Mechanics_Review.md](Hero_Mechanics_Review.md). Ba cơ chế được làm lại và giờ kích hoạt thường xuyên (Drowsy Seer ~5 lần/phút, Mirror Keeper ~11, Hourglass Stag ~10), nên token procedural nhỏ của chúng hiện rất nhiều trên màn hình. Dùng **prompt chung** ở trên, chỉ thay dòng `Subject:`. Lưu vào `Assets/ArcaneBrigade/Art/MechanicEffects/Tokens/{Token}_v1.png` (1 frame, vuông 512 hoặc 1024), rồi chạy `ArcaneBrigade.Editor.HeroMechanicArtImporter.Import`. Token chỉ là vật thể nhỏ (hiện ở 0.15–0.4 đơn vị), không vẽ vùng đánh — vùng đánh vẫn do mực procedural vẽ đúng kích thước gameplay.

| Token (file) | Hero | Subject |
| --- | --- | --- |
| `Star_v1.png` | Drowsy Seer (giấc mơ ở lại chờ), Patchwork Host | One small sleepy five-pointed star made of folded cream paper with soft midnight-blue shading, its points slightly crooked and drooping as if dozing, a tiny brass stitch at its centre, warm candle-cream light along the top edges |
| `Shard_v1.png` | Mirror Keeper (phát đạn mượn), China Marionette | One single slim shard of old silvered mirror glass with a chipped brass frame corner still attached, pale teal-blue reflections and one cream glint, edges hand-inked and slightly uneven, no reflection of any face |
| `Sand_v1.png` | Hourglass Stag (khoản nợ thời gian) | One small closed pouch of fine golden sand tied with a burgundy thread and a tiny blank brass tag, a thin trickle of sand curling from its neck, painted midnight-blue shadow underneath the pouch only (no floor) |
