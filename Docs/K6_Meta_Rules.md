# K6 — quy tắc meta và giao diện

Ngày thiết kế: 2026-10-14. Tuning nguyên bản của Arcane Brigade; **không tuyên bố là công thức chính xác của Bizarre Brigade 1.5+**. Vẫn movement-only/autocombat; không đổi mua tại quầy, currency, ally cap hoặc permanent stats.

## Mode, score và unlock

- Normal: 20 hall như baseline. Năm bậc: Êm đềm / Lạc lối / Chập chờn / Ác mộng / Không ngủ. Thắng Normal không challenge ở bậc trước mở bậc tiếp. Champion vẫn mở theo BestWave như baseline.
- Bậc đánh số nội bộ 0–4; HP enemy nhân `1 + 0.18 × bậc`, budget nhân `1 + 0.12 × bậc`. Damage từng nguồn giữ caps hiện có 14 thường / 26 boss; không tăng stat Champion vĩnh viễn. Chưa coi tuning đã qua full-run balance K7.
- Ba challenge local: Ca đêm (thắng), Ánh sáng đầu (clear hall 10 và kết thúc run), Đội nhỏ (thắng với tối đa sáu body cuối run). Không limit roster cứng, không thêm kỹ năng/input. Challenge dùng difficulty 0 và seed `610026 + index × 7919`, validate cả khi restore. Chọn Champion trước, bảng điểm theo Champion; không online/daily clock/server/leaderboard.
- Score = `(1.000 × hall clear + 10 × kills + 500 × bosses + 5.000 nếu thắng) × (1 + 0.2 × bậc)`, clamp counter. Combat hiển thị thêm kill/boss đã xảy ra; hall đang đánh không tính clear. Damage/heal/loot/VFX không sinh điểm. Hall defeat không nhận điểm clear; rest ngay sau thắng được tính đúng một hall.
- Best records theo Champion / difficulty / Normal hoặc Endless / challenge ID. BestWave vẫn là compatibility counter cho unlock cũ, không phải score theo mode.

## Kết quả và persistence

- Schema run 3; profile 2. Envelope SHA256, `.tmp` + flush + atomic replace + `.bak` giữ nguyên. Profile disk write thất bại rollback in-memory counters/discovery/history; có thể retry. Menu phục hồi credit terminal run đã lưu mà profile chưa nhận. Retry/reopen dedupe theo RunId, không theo nickname.
- Detailed history giữ 50 hóa đơn gần nhất; CompletedRuns giữ identity lifetime để eviction không recredit. Best records không phụ thuộc detailed history còn tồn tại.
- Frozen deep-copy build, actual weapon/tier/stack, ledger hiệu lực, seed/mode, score và nickname được lưu. Nickname chọn nguồn damage lớn nhất, tie theo stable key; không RNG/network. History dùng nickname/score đã lưu, không reroll khi đổi trang.
- Contribution: damage/heal theo HP thực, không overkill. Secondary chỉ ghi phần HP nó thực gây mất; kill ghi một lần cho nguồn đòn kết thúc. Không cộng kill lần nữa cho nguồn root/connector. Ledger gồm unit/weapon/tier/effect; không chia damage đều theo rarity.
- Invoice có đội dạng grid + danh sách, cổ vật và ledger scroll, quick summary, death source/kind/damage thật. Export tùy chọn lưu ảnh frame local trong `RunPaths.Root/Receipts`, không upload; PNG là frame UI hiện tại, không hứa export toàn bộ phần scroll.
- Endless: từ Victory đã credit, tạo RunId mới + ParentRunId; StartHall = hall kế. Giữ HP/roster/relic/currency/journal discovery; reset kills/bosses/time/ledger/death cause riêng segment. Rest mở Endless không nhận điểm clear hall chưa đánh; thắng thường và chết Endless không trộn records. Scaling wave baseline tiếp tục cùng difficulty multiplier, không reset sức mạnh quái về hall 1.
- Migration schema 0–2 giữ normal ledger/RNG/IDs/roster; missing history/discovery không bịa lại. Ledger Endless cũ không tách được normal/segment nên xóa chỉ ledger đã trộn, giữ build/counters; không gọi số 0 là đo lại lịch sử. Kills theo nguồn cũ không đo thì có nhãn legacy, không phân bổ giả.

## Museum và cosmetic

- Năm ngăn: vũ khí/tier, cổ vật, đồng minh, chuỗi, boss. Locked hiển thị yêu cầu, không reveal mô tả chưa khám phá.
- Journal AcquiredVariants ghi weapon/tier thật lúc nhận và mỗi kết quả merge/craft, gồm cả weapon vừa roll rồi bị tiêu bởi triple merge. Journal nằm trong transaction snapshot nên rollback không mở discovery. Profile không suy weapon từ body.
- Boss identity chỉ từ boss thật chết lúc settle. Chain chỉ từ effect ledger có damage/heal/coin/activation thực; sở hữu set hay graph ACTIVE không tự nhận chain. Map effect → graph ID cụ thể trong RunMeta, không đoán keyword.
- Sandbox giữ pipeline K4/K5 cô lập, private RNG/data, không save/score/discovery; tier preview ghi rõ NOT OWNED. Museum dùng Tab/LB/RB đổi ngăn, mũi tên/X/Y chọn mẫu, Enter/A thử, Escape/B quay lại; touch qua các nút/card. Controller toàn menu và thiết bị thật vẫn K7.
- Ba bundle: Cửa hàng thất lạc (mặc định), Văn phòng đêm (BestWave 5), Sân khấu giấy (ba boss khác nhau). Đổi accent/receipt paper/frame/stamp và marker cosmetic cũ; không đổi threat semantic, collider, damage, timing, RNG hay hồi HP.

## UI và chữ

- Be Vietnam Pro Black: headings/buttons; Be Vietnam Pro Medium: body Việt; Noto Sans Mono Bold: số/giá/timer. Không thêm custom proprietary font hay migrate render pipeline.
- Shared ticket edge, divider, small accession/header, sold stamp; menu có backdrop original, Details có body + weapon thực, museum có kệ/card/inspector, results có giấy hóa đơn. HUD tách HP/currency/score và tránh chuỗi dài một dòng; quầy vẫn world-space, không modal shop.
- Nguồn font chính thức: [Google Fonts / Be Vietnam Pro](https://github.com/google/fonts/tree/c13390277dfb1fcc7415d84ef7ef9cfc1e52f8c1/ofl/bevietnampro), SIL OFL 1.1. Black/Medium/OFL tải nguyên bản; Noto provenance giữ `K1_Font_Provenance.md`. Build chép cả hai license vào ThirdPartyNotices.
- SHA256 Black `DD0EC461782DA7F4A58698B431A67C455CED7C09DEB9C40D7B47E61975D3FFDB`; Medium `B60832BFA0FCD015158112C64D7E3FDAD3B0C6287D1823F85A3103636E845268`; OFL `6B7F8F73609A25EA78C891E34CF37B06F8A676B7EA986E941E43B009110F2A85`.

K7: mọi aspect/glyph combining/kerning/device safe area, controller flow toàn game, UI transitions, natural full-run balance, GPU/GC/frame-time, audio audition và Steam prerequisites. Windows functional milestone không thay nghiệm thu production.
