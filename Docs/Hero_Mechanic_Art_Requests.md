# Hero mechanic art requests — 17/10/2026

Cơ chế của 31 hero hiện được vẽ bằng mực procedural (`MechanicInk`, `InkCanvas`, `HeroIdentityVisual`): vùng nước màu và viền mực đúng bằng vùng sát thương thật, nét cọ ba lớp, cùng một "token" vẽ nhỏ riêng cho từng hero. Token procedural đọc được ở kích thước gameplay nhưng còn đơn giản. Các yêu cầu dưới đây thay **token** bằng tranh vẽ tay; không thay hình học vùng đánh (vẫn do simulation quyết định).

Cách gắn vào game: lưu đúng đường dẫn, rồi chạy `-executeMethod ArcaneBrigade.Editor.HeroMechanicArtImporter.Import`. Mỗi file là **một sprite tĩnh, 1 frame**; chuyển động (bay, xoay, nảy, co giãn) do code làm. Ảnh nên vuông, khoảng 512×512 hoặc 1024×1024. Thiếu file nào thì token procedural tương ứng vẫn được dùng. Token mặt xúc xắc (Glass Dice) giữ procedural vì cần hiện đúng mặt 1–6.

Ưu tiên: các token mà bản procedural yếu nhất ở kích thước nhỏ (nút thắt, hơi nước, răng nanh, gai, miếng vá, bánh bao, dấu sáp, trăng).

---

## 1. Lantern Weaver — living-network knot

Target: `Assets/ArcaneBrigade/Art/MechanicEffects/Tokens/Knot_v1.png` (1 frame)

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a single glowing thread knot pinned in the air — a small round tangle of mint-teal silk thread tied into a loose bow-knot around a tiny brass lantern bead, two short frayed thread ends trailing left and right, warm candle glow inside the bead

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

## 2. Copper Cauldron — pressure-cooker steam puff

Target: `Assets/ArcaneBrigade/Art/MechanicEffects/Tokens/Puff_v1.png` (1 frame)

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: one compact billow of cream kitchen steam shaped like three or four rounded cloud lobes, painted as a solid illustrated object (not a transparent smoke effect), warm copper-orange shading on its underside and cream highlights on top, one tiny curl of steam at its crest

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

## 3. Night Maw — hungry-mouth fang

Target: `Assets/ArcaneBrigade/Art/MechanicEffects/Tokens/Fang_v1.png` (1 frame, point facing down)

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: one single curved storybook fang, ivory-cream with a slightly crooked tip pointing straight down, a burgundy-crimson gum root at the top, soft painterly shading, friendly-spooky rather than gory, no blood

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

## 4. Thorn Treasurer — collateral thorn

Target: `Assets/ArcaneBrigade/Art/MechanicEffects/Tokens/Thorn_v1.png` (1 frame, point facing up)

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: a single emerald-green rose thorn standing point-up, gently curved like a hook, with a tiny brass coin-ring collar wrapped around its base, cream highlight along its spine

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

## 5. Patchwork Host — stage patch

Target: `Assets/ArcaneBrigade/Art/MechanicEffects/Tokens/Patch_v1.png` (1 frame)

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: one small square fabric patch of rose-burgundy velvet, slightly tilted, with cream running-stitch thread around its border, one corner lifting, a tiny brass button sewn at its centre

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

## 6. Supper Mortar — dumpling shell

Target: `Assets/ArcaneBrigade/Art/MechanicEffects/Tokens/Dumpling_v1.png` (1 frame)

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: one plump steamed dumpling used as a cannon shell, cream dough with pleated top folds, warm orange shading underneath, a tiny brass fuse cap twisted into the topknot with a single ember spark

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

## 7. Postmaster / Ivory Sovereign / Treasure Acorn — wax seal stamp

Target: `Assets/ArcaneBrigade/Art/MechanicEffects/Tokens/Seal_v1.png` (1 frame). Dùng chung cho dấu địa chỉ (Postmaster), sắc lệnh (Ivory Sovereign) và dấu truy thu (Treasure Acorn), nên chọn sáp đỏ burgundy trung tính.

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: one round burgundy sealing-wax stamp seen from the front, with a wavy melted rim and a raised brass-gold star crest pressed into its centre, small cream highlight on the upper left of the rim

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

## 8. Moon Prowler — shadow moon

Target: `Assets/ArcaneBrigade/Art/MechanicEffects/Tokens/Moon_v1.png` (1 frame, horns facing right)

```
Create a 2D raster game sprite for Arcane Brigade using painted.
Attach the existing furniture-00_v1.png and NightMarket_v1.png as
STYLE REFERENCES. Match their visual language closely.

Subject: one small crescent moon with its horns facing right, pale periwinkle-blue with midnight-blue shading in its hollow, a sleepy closed eye and a tiny brass star earring hanging from the lower horn

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

Các token khác (tia đèn, phong thư, sao mơ, lá, giọt, nốt nhạc, mảnh gương, xu, cánh hoa, bong bóng, giáo, nút áo, lát cam, trang sách, tia sét, đồng hồ cát) có thể yêu cầu cùng cách với tên file `{Tên InkToken}_v1.png` nếu cần sau khi duyệt ảnh trong `PreviewExports/HeroMechanics/`. Lưu ý: token dùng chung (xu, lá, nốt nhạc, mảnh, giọt) sẽ mất màu riêng theo hero khi thay bằng tranh vẽ.
