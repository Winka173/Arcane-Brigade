# Audit release: năm độ khó — 27/10/2026

Yêu cầu đã chốt: giữ vai trò hero, đánh giá chênh lệch damage bằng tổng đội. Sau khi cấu hình độ khó được rút từ tám xuống năm ở commit `241deb8f`, chạy lại bằng source đó cộng bản sửa traversal và công cụ audit của lượt này. Báo cáo tám mức trước đó ở [Release_Balance_2026-10-27.md](Release_Balance_2026-10-27.md) chỉ dùng làm lịch sử đối chiếu.

**Chưa nghiệm thu release.** Gate đo lường và gate gameplay được giữ riêng: runner chạy xong không đồng nghĩa mọi kiểm tra đã qua.

Phạm vi đóng băng là source năm độ khó với trần ba sao. Trong khi lượt đối chứng đang chạy, workspace bắt đầu có thay đổi 4–5 sao/Rainbow Thread; phần đó được giữ riêng, không đưa vào số đo hoặc commit audit này. Cần đo lại khi tích hợp thay đổi sao vào vòng cân bằng sau.

## Ma trận

- Peaceful (0), Wandering (1), Nightmare (2), Sleepless (3), The Last Night (4).
- Compile: toàn bộ 36 hero × 5 độ khó × 15 mốc wave 1–50 = **2.700 cấu hình**; xác minh số hữu hạn, giá trị hợp lệ, lịch boss và thứ tự HP/damage.
- Campaign: toàn bộ 36 hero × 5 độ khó × seed 271026 × Hybrid = **180 lượt dự kiến**. Danh sách key nằm trong `CampaignPlan.tsv`, kết quả phải khớp đầy đủ trước khi gọi là ma trận hoàn tất.
- Bốn mức đầu dùng stat profile không meta. The Last Night dùng `MetaProgression.BotProfile(1)`/`BotLoadout(1)`: full Lamp Tree và mastery theo một nhánh fork cố định, kiểm tra profile thực sự mở được mức này; loadout từng hero lưu trong `Meta/`. Đây là fixture profile sở hữu nâng cấp, không chỉnh save người chơi. Không so trực tiếp win rate mức cuối có meta với bốn mức không meta như một thang độ khó được giữ cùng điều kiện.
- Toàn bộ catalog vẫn được thử, kể cả hero đang khóa/COMING SOON. Đây là probe hệ thống, không chứng nhận mọi hero đều được chọn trong menu hiện tại.
- Thu nhập, giá, reroll, quota, merge ba bản sao, nửa sao, quà prism và ngừng tuyển species mới trước wave 20 đều đi qua code thật. Không thêm HP/vàng/item giữa trận. Optional visitor/secret deals bị bỏ qua; không suy ra cân bằng event từ matrix này.
- Mỗi bước 0,05 giây; cutoff 90 giây với wave thường, 240 giây với boss. Timeout vẫn chưa phân giải. Lưu `BossFellAt` riêng với thời gian encounter để không nhầm boss chết sớm với thời điểm hết wave.

## Gate migration

Đã tái hiện **6/72 kiểm tra five-difficulty FAIL** trên source năm mức ban đầu. Profile chuyển đúng bit chiến thắng cũ và không remap lần hai, nhưng run chưa kết thúc schema 5 không được remap: index cũ 2/3/4 vẫn giữ số cũ (đổi ý nghĩa), index 5/6/7 bị `Invalid run meta identity`. Đây là lỗi tương thích save thật, không phải một thông số balance hoặc assertion lịch sử cần nới.

Đã bổ sung migration: run schema **6**, profile version **5**, cùng marker riêng để không remap hai lần khi Validate → Restore. Snapshot legacy được clone trước khi đổi index, giữ nguyên schema cũ cho các bước tương thích shop/roster còn lại. Profile chuyển cả lựa chọn cuối, lịch sử, achievement/evidence và key thưởng boss/Keeper; record bị gộp giữ điểm cao nhất, không tính lại điểm lịch sử, không cộng lại tiền thưởng. Không sửa hoặc xóa save thật trong quá trình kiểm tra.

`ReleaseFiveDifficultyChecks` còn kiểm tra tên/thứ tự năm mức, toàn bộ bảng hệ số, thứ tự HP/damage/density ở tám mốc wave, unlock full tree, round-trip save mới, chặn index không hợp lệ và chặn tuyển species mới ở regular/dream shop trước trận 20. Kiểm chứng migration sau sửa dùng file checksummed trong thư mục tạm riêng, gồm lặp save/load và kiểm tra tiền/progress không thay đổi. Schema ≤5 được xác định là layout tám mức cũ; schema 6 đánh dấu layout mới để các save tiếp theo không còn nhập nhằng.

Các gate lịch sử còn cần triage riêng. Ví dụ RecruitmentChecks cũ vẫn đòi mua species mới ở wave 20; đó không còn là kỳ vọng hợp lệ sau `241deb8f`. Giữ nguyên FAIL để không tự công bố toàn bộ regression xanh. Kết quả từng gate nằm trong `Checks.tsv`, chi tiết migration ở `FiveDifficultyChecks/Checks.txt`. Giữ file kiểm tra trước sửa trong folder lượt chính; không thay FAIL lịch sử bằng kết quả sau sửa.

## Bằng chứng và tái tạo

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/Balance/Run-ReleaseBalance.ps1 -Label MyFiveLevelPass -Difficulties 0,1,2,3,4 -Meta required
```

Runner kiểm tra lỗi chết-tách đôi trước, chạy DPS, rồi campaign; `-SkipDps` chỉ bỏ bước DPS nếu đã có bộ dữ liệu riêng được ghi rõ. Mỗi label có hash assembly để ngăn nối hai phiên bản khác nhau. `Method.txt` ghi tham số thực, `Difficulties.tsv` ghi tên theo index. `summarize_release.py --require-complete` từ chối ma trận campaign chưa đủ key.

Giữ phân biệt: compile pass, structural difficulty ladder pass, campaign outcomes, migration gate, damage gate 30%, event balance và cảm giác trên Windows player. Không dùng một nhóm để thay thế nhóm khác.

## Kết quả hoàn tất

Lượt chính `FiveLevels20261027` đã hoàn tất **180/180 campaign, 2.297 wave**, không exception hoặc `BLOCKED_SHOP`.

| Độ khó | Meta | Thắng / 36 | Thua | Timeout |
| --- | --- | ---: | ---: | ---: |
| Peaceful | Không | 20 | 15 | 1 |
| Wandering | Không | 15 | 20 | 1 |
| Nightmare | Không | 1 | 33 | 2 |
| Sleepless | Không | 4 | 31 | 1 |
| The Last Night | Full tree/mastery | 16 | 17 | 3 |

Đây là kết quả bot trên toàn catalog, không phải tỷ lệ thắng người chơi. The Last Night có loadout khác, nên 16/36 không chứng minh nó dễ hơn Sleepless. Nightmare còn có ba trường hợp không thắng nhưng cùng hero/seed thắng Sleepless: Lantern Knight timeout wave 20, Drowsy Seer thua wave 16, Velvet Regent thua wave 8. Shop/loot rẽ nhánh theo combat; cần đọc cùng seed đối chứng, không dùng một seed để sửa hệ số chỉ nhằm làm đẹp thứ tự tỷ lệ thắng.

Kinh tế không meta ở shop 16–19: trung vị ví/giá offer trung bình **3,12**, trung bình **16,92**. Có meta: trung vị **5,59**, trung bình **85,48**. Khoảng cách rất lớn cho thấy outlier kéo trung bình; chưa có cơ sở nerf thu nhập toàn game. Mẫu cuối trận chỉ gồm những lượt sống được tới đó.

Sau sửa migration: **106/106 kiểm tra qua**, gồm đọc file checksummed của cả tám index cũ, giữ migration shop schema 3/4, không thay snapshot đầu vào, gộp record đúng, giữ tiền và lịch sử thưởng, save/load lặp lại ổn định. Bằng chứng trước/sau được giữ riêng ở `FiveLevels20261027/FiveDifficultyChecks.Before.txt` và `FiveHeldout20261027/FiveDifficultyChecks.After.txt`.

Lượt chính chạy trước bản sửa save. Lượt đối chứng sau sửa dùng seed **814907**, sáu hero × năm mức = **30/30 campaign, 399 wave**, cùng quy tắc meta `required`. Thắng theo thứ tự năm mức: **3/6, 2/6, 1/6, 1/6, 3/6**; một timeout ở Peaceful, các mức khác không timeout. Sáu hero là Lantern Knight, Lone Jelly, Postmaster, Root Oracle, Glass Dice và Kite Admiral. Không gộp sáu hero này với toàn bộ 36 hero để so tỷ lệ thắng như cùng quần thể.

Tổng vòng năm mức: **210 campaign, 2.696 wave**. Cả 2.700 dòng compile trước/sau sửa migration **giống từng byte**. Toàn bộ **5.184 fixture DPS** sau sửa cũng giống baseline, gồm cả bảng range; 1.080 kiểm tra reach/shot pattern/crit encore qua. Năm đường chết-tách đôi qua hồi quy. Không có thay đổi số học damage/giá/độ khó của audit này.

**Gate 30% chưa đạt:** max/min tổng đội vẫn **3,67–9,71×** ở cấu hình cơ bản và **2,45–3,83×** ở cấu hình đầu tư. Trong fixture cơ bản Glass Dice cao nhất, Tin Virtuoso thấp nhất; cấu hình đầu tư đổi outlier theo đơn/đám đông. Không sửa bằng cách thêm đánh thường cho hero hỗ trợ hoặc gán đội giả cho hero solo.

Kết quả tổng hợp và hash input: [`FiveSummary.json`](../PreviewExports/ReleaseBalance/FiveSummary.json). Dữ liệu gốc nằm trong `FiveLevels20261027`, `FiveHeldout20261027` và `HeroDpsRange/FiveHeldout20261027[Invested]`. Script từ chối dữ liệu trùng key, thiếu mẫu, giá trị không hữu hạn hoặc nối hai assembly khác nhau. `FiveSourceInventory.json` chốt source/doc trong index của commit audit, tách khỏi WIP 5 sao.

## Các điểm còn mở

- Bảy nhóm regression lịch sử vẫn FAIL, được lưu nguyên trạng và cần triage theo contract mới; không phải tất cả đều là lỗi gameplay. Gate năm mức/migration mới đã PASS.
- `EnemyVariantCombat` vẫn có ba bảng elite **tám phần tử** và lấy trực tiếp index độ khó. Ở mức 4 mới, elite dùng phần tử 4 thay vì phần tử 7 của The Last Night cũ. Đây là chỗ cần remap hoặc quyết định tuning riêng; thứ tự HP/damage trong `JourneyBalance` không tự chứng minh elite đã được chuyển đúng.
- Cần tune outlier DPS, điều tra tăng trưởng tiền/item quá mạnh ở một số build, đo thêm chiến lược Buff/Recruit, thăm dò nhiều seed hơn và full-meta control nếu muốn so thang độ khó với cùng profile.
- Cần đo lợi ích/rủi ro từng lựa chọn event, boss tùy chọn, Endless và Windows player feel/performance. Ma trận này bỏ qua deal tùy chọn; transaction checks không thay thế cân bằng event.
