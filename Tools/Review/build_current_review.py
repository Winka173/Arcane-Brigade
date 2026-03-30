"""Produce the current, self-contained Vietnamese review dossier from a fresh Unity snapshot.

Run ArcaneBrigade.Editor.GameReviewSnapshot.Export first. No gameplay assets are changed.
"""
from __future__ import annotations

import hashlib
import json
import re
import subprocess
from collections import Counter
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
SNAPSHOT = ROOT / "Library/GameReview/Snapshot.json"
OUT = ROOT / "Docs/Arcane_Brigade_Complete_Review.md"
D = json.loads(SNAPSHOT.read_text(encoding="utf-8-sig"))
assert "HeroKits" in D and "WavePreviews" in D, "Export the updated Unity snapshot first"
HEAD = subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=ROOT, text=True).strip()
L: list[str] = []
anchors: set[str] = set()
coverage: dict[str, list[str]] = {}


def add(s=""):
    L.append(str(s))


def txt(v):
    if v is None or v == "": return "—"
    if isinstance(v, bool): return "có" if v else "không"
    if isinstance(v, float): return f"{v:.7g}"
    if isinstance(v, list): return "; ".join(txt(x) for x in v) or "—"
    if isinstance(v, dict): return txt(v.get("id", v.get("name", json.dumps(v, ensure_ascii=False))))
    return str(v)


def cell(v):
    return txt(v).replace("|", "&#124;").replace("\r", "").replace("\n", "<br>")


def table(headers, rows):
    add("| " + " | ".join(map(cell, headers)) + " |")
    add("| " + " | ".join("---" for _ in headers) + " |")
    for row in rows: add("| " + " | ".join(map(cell, row)) + " |")
    add()


def heading(title, anchor, level=2):
    assert anchor not in anchors, anchor
    anchors.add(anchor)
    add(f'<a id="{anchor}"></a>\n\n{"#" * level} {title}\n')


def details(title, body, language="text"):
    add(f"<details>\n<summary>{title}</summary>\n\n````{language}\n{body.rstrip()}\n````\n\n</details>\n")


def js(v):
    return json.dumps(v, ensure_ascii=False, indent=2, allow_nan=False)


def prose(s):
    add(str(s).replace("\\n", "\n") + "\n")


def fields(o, names):
    table(["Trường", "Giá trị"], ((k, o[k]) for k in names if k in o))


def track(group, ident):
    coverage.setdefault(group, []).append(ident)


def ref(ident, group):
    return f"[{ident}](#{group}-{ident.lower()})"


def embed_doc(path, title, warning=""):
    p = ROOT / path
    if not p.exists(): return
    add(f"### {title}\n")
    if warning: prose(warning)
    body = p.read_text(encoding="utf-8-sig")
    # The exported artifact must remain useful without the rest of the repository.
    body = re.sub(r"\[([^\]]+)\]\((?!https?://)[^)]+\)", r"\1", body)
    body = re.sub(r"(?m)^(#{1,6}) ", lambda m: "#" * min(6, len(m[1]) + 2) + " ", body)
    prose(body)


STATS = {x["Key"]: x for x in D["StatGlossary"]}
SCOPE = {"Team": "cả đội", "Champion": "hero", "Companions": "đồng minh", "Run": "lượt chơi"}


def mods(values):
    out = []
    for m in values or []:
        val = m["Value"]
        v = f"{val * 100:+.7g}%" if STATS[m["Effect"]]["Percent"] else f"{val:+.7g}"
        out.append(f"{SCOPE.get(m['Scope'], m['Scope'])}: {m['Effect']} {v}")
    return "; ".join(out) or "không có modifier trực tiếp"


def unit_table(u):
    table(["HP asset", "Tốc độ", "Bán kính", "Cao", "Vũ khí asset", "Va chạm / nhịp", "Crit"], [[
        u["Health"], u["Speed"], u["Radius"], u["BodyHeight"], u["Weapon"],
        f"{u['ContactDamage']} / {u['ContactInterval']} s", u["CriticalChance"]]])


kits = {x["Id"]: x for x in D["HeroKits"]}
baselines = {(x["HeroId"], x["Wave"]): x for x in D["HeroBaselines"]}
poolmap = {x["HeroId"]: x for x in D["HeroShopPools"]}
units = {u["Id"]: u for g in ["Champions", "Companions", "Enemies", "Bosses", "OtherUnitAssets"] for u in D[g]}
waiting = D.get("WaitingEnemies", [])
artmap = {a["Id"]: a for a in (D.get("WaitingEnemyRoster") or {}).get("Art", [])}
counts = {g: len(D[g]) for g in ["Champions", "Companions", "Items", "Bonds", "Recipes", "Weapons", "Enemies", "Bosses", "Achievements"]}

heading("ARCANE BRIGADE — Hồ sơ tổng hợp toàn game để review", "top", 1)
prose(f"**Cập nhật:** {D['ExportedAtVietnam']} · **Unity:** {D['UnityVersion']} · **Mã nguồn nền:** `{HEAD}` · **Catalog:** `{D['Catalog']['Version']}`.")
prose("**SPOILER TOÀN BỘ CỐT TRUYỆN VÀ KẾT THÚC.** Đây là một file độc lập: có phần giải thích bằng tiếng Việt, danh mục đầy đủ, thoại tiếng Anh nguyên bản, dữ liệu Unity và phụ lục công thức. Gửi riêng file này là đủ để review nội dung và hệ thống; nó không chứa binary ảnh/âm thanh hay thay thế bản game chơi thử.")
prose("Nguồn ưu tiên: **asset đang được Unity nạp + code runtime hiện tại**, rồi đến canon và tài liệu thiết kế. Nhãn **dữ liệu gốc** chưa gồm hero, sao, relic, bond, độ khó hoặc scaling; **preview biên dịch** là tình huống tổng hợp, không phải lượt chơi từ đầu đến cuối. Tài liệu cũ có thể ghi 31 hero/66 companion/170 item, đội tối đa 15–20, wave 45 giây hoặc boss mỗi 4 wave: không dùng các số đó làm luật Hồi 1 hiện hành.")
TOC = [
    ("overview", "01. Tổng quan và cách chơi"), ("story", "02. Thế giới, nhân vật, cốt truyện và kết thúc"),
    ("journey", "03. Chiến dịch, wave, mode và độ khó"), ("economy", "04. Kinh tế, shop, tuyển quân, sao và quà"),
    ("heroes", "05. Toàn bộ hero và Awakening"), ("companions", "06. Toàn bộ companion"),
    ("weapons", "07. Toàn bộ vũ khí"), ("items", "08. Toàn bộ item, food, drink và curio"),
    ("bonds", "09. Toàn bộ bond"), ("recipes", "10. Toàn bộ công thức altar"),
    ("builds", "11. Build, morph và liên kết item"), ("enemies", "12. Toàn bộ kẻ địch và tiến hóa"),
    ("bosses", "13. Boss thường, boss cốt truyện và boss ẩn"), ("secrets", "14. Phòng bí mật, hợp đồng và lựa chọn"),
    ("stats", "15. Stat, damage, giới hạn và cân bằng"), ("controls", "16. Điều khiển, UI và tương tác"),
    ("progression", "17. Lưu game, mở khóa và achievement"), ("visuals", "18. Art, animation, bóng tối và ánh sáng"),
    ("audio", "19. Âm nhạc, SFX và thoại"), ("review", "20. Phạm vi đã có, chênh lệch và đề bài review"),
    ("raw", "21. Phụ lục dữ liệu đầy đủ"), ("source", "22. Phụ lục mã thực thi và xuất xứ")]
for key, label in TOC: add(f"- [{label}](#{key})")
add()

heading(TOC[0][1], "overview")
prose("Arcane Brigade là game sinh tồn theo wave nhìn từ trên xuống, đánh tự động, xây build bằng mua đồ và tuyển đoàn đồng hành. Người chơi điều khiển vị trí, tránh đạn/đòn báo trước, nhặt tài nguyên và ra quyết định trong shop. Tông chính là sách truyện kỳ bí pha kinh dị: những sinh vật có cuộc đời riêng cùng đi qua Miền Chờ, giữa các ngọn đèn đang tắt và Sương Đen.")
table(["Nội dung trong dữ liệu hiện tại", "Số lượng / phạm vi"], [
    ("Hero", counts["Champions"]), ("Companion thường / sưu tập", counts["Companions"]),
    ("Đồng hành cốt truyện", "4 vị trí riêng: Hob, Latch, Postscript, Tally; tái sử dụng 4 loài có sẵn, không cộng thành 98 loài"),
    ("Item trong RunCatalog", counts["Items"]), ("Item thưởng ngoài catalog", "Thursday Shoes, xem mục truyện và quà"),
    ("Bond", counts["Bonds"]), ("Công thức altar", counts["Recipes"]),
    ("Vũ khí trong catalog / ngoài catalog", f"{counts['Weapons']} / {len(D['OtherWeaponAssets'])}; Arsenal là một tập con, không đồng nghĩa toàn bộ catalog"),
    ("Địch pool gốc / roster Miền Chờ bổ sung", f"{counts['Enemies']} / {len(waiting)}; Waking Sitter chỉ dùng ở SIT CLOSER"),
    ("Boss trong catalog gốc", counts["Bosses"]), ("Achievement", counts["Achievements"]),
    ("Hồi đang làm", "Hồi 1, 20 wave, 4 vùng; Hồi 2–3 để sau"),
    ("Mốc truyện có dữ liệu", f"{len(D['WaitingChapters'])} mốc / {sum(len(x['Pages']) for x in D['WaitingChapters'])} trang"),
    ("Độ khó / challenge có tên", f"{len(D['Modes']['Difficulty'])} / {len(D['Modes']['Challenges']) - 1}"),
    ("Morph / build link / phòng bí mật", f"{len(D['Morphs'])} / {len(D['BuildLinks'])} / {len(D['SecretRooms'])}")])
prose("**Vòng chơi:** chọn hero, difficulty và mode → sống sót trong wave, đánh tự động và né nguy hiểm → vào shop để mua stat, kỹ năng hoặc companion → ghép sao, đọc truyện, tương tác vật thể hoặc nhận lời mời bí mật → ra cửa sang wave tiếp → đánh Last Customer và chọn lời kết → có thể chơi Endless với build đã có. Hero hết HP là thua; companion hiện bất tử. Ánh sáng giúp đọc trận, không phải nhiên liệu sinh tồn và không tự cấp miễn sát thương.")
prose("**Hướng chơi cơ bản:** đội đông tận dụng nhiều nguồn damage, kỹ năng riêng và bond; đội nhỏ đầu tư relic tăng sức mạnh cá nhân, vũ khí và Awakening. Không thể chỉ so số người hoặc damage asset: tốc độ đánh, thời gian trúng đích, xuyên/nảy/tách, support, giá và quota đều quyết định sức mạnh. Hero solo/hấp thụ có luật riêng ghi ở từng mục.")

heading(TOC[1][1], "story")
prose("**Tiền đề:** nhân vật hư cấu lớn lên cùng cảm giác phải ngoan hoặc hữu ích mới đáng được yêu. Chiếc ghế cạnh giường từng trả lời khi không ai lắng nghe. Thực thể Người Luôn Ở Đó quan tâm nhưng muốn độc chiếm, giấu thư hồi âm và ngăn người đến đón. Lời hứa ‘I will wait if you will stay’ trở thành ràng buộc ở Miền Chờ. Đây là quá khứ nhân vật, không phải chẩn đoán người đang chơi. Miền Chờ tồn tại trước họ; companion không phải đồ chơi hay toàn bộ là những mảnh tâm lý của họ.")
table(["Vai trò", "Danh tính / xung đột"], [
    ("Người Luôn Ở Đó / The One Who Stays", "Thực thể gắn với Chiếc Ghế Ấm; muốn giữ sự phụ thuộc, thì thầm từ bên trái. Có tình cảm không xóa trách nhiệm giam giữ."),
    ("Last Customer", "Người đầu tiên đứng dậy nhưng bán tên; mua tên và ký ức của người khác để tồn tại. Giữ tên trong thỏa thuận với chiếc ghế."),
    ("Người bán hàng", "Từng được Keeper kéo dậy; giúp nhưng trì hoãn vì sợ bị bỏ lại; trao chìa khóa dù không đòi được tha thứ."),
    ("Keeper", "Cơ chế giữ ranh giới và cứu lữ khách; lệnh OPEN bị Customer viết thành CLOSED. Runtime hiện chuyển vai boss wave 5 sang Coin Widow; Keeper vẫn có vai trò chuyển vùng và lời mời shop 17."),
    ("Hob", "Linh hồn bếp lửa biết chăm người khác, học cách được chăm và nghỉ mà không cần hữu ích."),
    ("Latch", "Lính gác từng chặn ta vì tuân lệnh; che bên trái, đối chiếu với Tally."),
    ("Postscript", "Người đưa thư từ phong bì gấp, mang thư và bằng chứng đã có người trả lời."),
    ("Tally", "Cựu thợ săn khắc vạch những người từng bắt; từng thả ta; quay lại đòi trả những người ấy mà không được bảo đảm tha thứ.")])
prose("**Cảnh báo đối chiếu canon:** tài liệu truyện dưới đây là bản chốt có lịch sử. Runtime mới dùng THE COIN WIDOW ở wave 5, thay THE KEEPER trong timeline viết ngày 16/10. Danh tính nguồn của Chair Porter và Door That Stayed Shut vẫn là boss tái sử dụng. Bảng preview và thoại runtime phía sau ghi trạng thái đang chạy, không tự coi các hình thể placeholder đã có art độc lập.")
embed_doc("Docs/Mien_Cho_Act_One_Story.md", "Toàn văn canon Hồi 1 và các lựa chọn — có spoiler")
add("### Mốc truyện và lời thoại thực được xuất từ Unity\n")
for i, ch in enumerate(D["WaitingChapters"]):
    heading(f"{i + 1}. Wave {ch['Wave']} · {'shop' if ch.get('Shop') else 'combat'} · {ch['Title']}", f"chapter-{i}", 4)
    prose("**Bằng chứng:** " + ch["Evidence"])
    for n, page in enumerate(ch["Pages"], 1):
        add(f"**Trang {n}**\n")
        prose(page)
add("### Các bộ lời kết xuất từ runtime\n")
for e in D["Endings"]:
    add(f"#### {e['Choice']} · ký ức tùy chọn: {txt(e['OptionalMemories'])}\n")
    for n, page in enumerate(e["Pages"], 1): prose(f"**Trang {n}:** {page}")
prose("Các biến thể lựa chọn, trang bổ sung và hậu truyện còn đọc trong snapshot/mã ActOneCanon, NightmareDialogue, WaitingCampaign ở cuối file. Hai nhánh kết chính đều là tự do và bình minh thật; credit gợi mở Hồi 2 không hủy kết quả Hồi 1.")

heading(TOC[2][1], "journey")
table(["Vùng", "Wave", "Chủ đề / không gian"], zip(D["ActOne"]["Zones"], ["1–5", "6–10", "11–15", "16–20"], ["Cổng và tiệm, cảm giác phải có ích", "Sân ga và những người ngồi chờ", "Căn nhà có cửa sổ nhìn vào trong", "Căn phòng hoàn hảo và quầy Customer"]))
prose("Wave thường dùng `min(60, 30 + (wave−1)×2.5)` giây: bắt đầu 30 giây, đạt 60 từ wave 13. Boss Hồi 1 ở wave 5/10/15/20; boss thường có overtime, finale có thời lượng 0 (không giới hạn theo đồng hồ wave). Con số FirstWaveSeconds/WaveSeconds=45 trong RunCatalog là cấu hình nền, đã bị logic WaveCadence ghi đè khi chuẩn bị trận.")
table(["Wave", "Thời lượng preview (s)", "Boss runtime", "HP boss Peaceful, trước giao chiến", "Story party khi chuẩn bị combat"], ((x["Wave"], x["Seconds"], x["Boss"]["Name"] if x["Boss"] else "—", x["Boss"]["HP"] if x["Boss"] else "—", x["StoryCompanions"]) for x in D["WavePreviews"] if x["Difficulty"] == 0))
table(["Độ khó", "Hệ số HP", "Hệ số damage trước ×0.62", "Mật độ"], ((n, D["DifficultyFactors"]["Health"][i], D["DifficultyFactors"]["Damage"][i], D["DifficultyFactors"]["Density"][i]) for i, n in enumerate(D["Modes"]["Difficulty"])))
prose("Các hệ số bảng được đưa dần vào bằng ramp smoothstep từ 0.4 đến 1 trong wave 1–8. Damage còn nhân 0.62 và các bước nguồn/wave/relief; bảng không phải sát thương cuối. Mở độ khó tiếp theo bằng thắng độ khó trước. Endless tăng HP/damage theo hàm mũ riêng, số mũ chặn 200 để tránh tràn; scaling nền còn chạy.")
table(["Mode", "ID", "Seed"], zip(D["Modes"]["Challenges"], D["Modes"]["ChallengeIds"], D["Modes"]["Seeds"]))
prose("Free journey cho run tự do; các challenge có seed cố định, điều kiện thắng riêng (đến bình minh, hết hall 10, hoặc tối đa 6 companion) và nhánh ép difficulty ở RunMeta/RunSession. Không đồng nhất challenge với sáu difficulty.")

heading(TOC[3][1], "economy")
fields(D["Catalog"], ["StartingGold", "Income", "InterestDivisor", "InterestCap", "RerollPrice", "RerollPriceStep", "ShopSlots", "SpecialShopEvery", "SpecialShopPrice", "AltarBaseChance", "AltarChancePerEgg", "MaximumTier", "TierDamageMultiplier", "TierSupportMultiplier"])
prose("Giá trị trên là **gốc**. Vàng từ hạ quái/nhặt xu, thu nhập wave và lợi tức; trứng dùng ở dream shop/altar; một số món đổi bằng HP nhưng không được trả đến chết. CoinGoldValue hiện 0.75; giá vàng chịu CarnivalBalance.ShopPrice × WaveCadence.PriceRamp, rarity, tier, veteran, chiết khấu và tài năng. Dream shop có đường giá riêng (SpecialStagePriceFactor=1), không lấy công thức tăng giá vàng áp bừa cho trứng. Reroll đổi hàng, không reset quota hay phát lại animation đồ cảnh rơi.")
table(["Quy tắc tuyển đang có trong code", "Chi tiết"], [
    ("Trần cơ thể", "Không cap cơ thể; CompanionLimit=int.MaxValue. Quota mua vẫn áp dụng."),
    ("Shop thường: loài mới", "1 + KitNewRecruits mỗi wave đủ điều kiện; sau một wave mua loài mới, wave kế nghỉ, sau 2 wave mới mở. Luật này áp dụng từ đầu run, không còn mốc 10 mới bắt đầu."),
    ("Shop thường: bản sao", "Tối đa 1 bản sao của loài đang sở hữu mỗi wave; trần cơ sở 2 lần mua (1 mới + 1 bản sao), có ngoại lệ kit."),
    ("Dream shop", "Tối đa 1 loài mới/wave. Từ wave 10 dream shop kiểm tra 2 wave qua LastNewShopSpeciesWave, được ghi khi mua mới ở bất kỳ shop nào; bản sao dream shop không dùng quota bản sao thường."),
    ("Loài đã có", "Có trong roster hoặc AbsorbedSkills; khác sao/vũ khí không biến thành loài mới."),
    ("Story party", "Không mua/ghép/bán; không tính quota hoặc bond. 4 loài dùng làm story party bị loại khỏi hàng bán theo StorySpecies."),
    ("Prismatic", "Không bán như hàng shop thường; nhận qua quà/shard/lựa chọn có điều kiện, theo pool hero. Enum Rainbow hiển thị là Prismatic."),
    ("Ghép sao", "Ba đơn vị hợp lệ cùng loài/cấp → một cấp cao hơn, tối đa 3 sao; giữ ngẫu nhiên một vũ khí thật từ nguyên liệu, không roll ngoài nhóm."),
    ("Vũ khí khóa", "Mythic/Prismatic mang vũ khí riêng; vũ khí đó không được roll sang loài khác. Các mức thấp dùng bộ vũ khí đủ điều kiện."),
    ("Bán ký ức", "Shop 8 chọn một loài thường hoặc từ chối. Companion còn đi cùng nhưng thành người lạ và không tính bond; không chọn bốn bạn cốt truyện.")])
prose("Gate tìm loài mới giảm theo quân số từ 6, trước Luck, trọng số và ưu tiên ghép; đây không phải cap. Cơ hội bản nâng sao/ghép không bị coi là loài mới. Rare có đóng góp bổ sung cho bond phụ khi bond đó đã kích hoạt. Sau giao dịch, preview phải tính roster kết quả, bao gồm ghép/nâng sao; bond sắp bật được highlight/animation, không chỉ in số hiện tại.")
prose("**Shard/quà:** ba mảnh lăng kính mở một quà chọn trong pool; code giới hạn nguồn rơi theo run và có thưởng boss 10/15 nếu giữ LowestChampionShare ≥0.5. SIT CLOSER tặng Thursday Shoes (+12% MoveSpeed và +6% Dodge theo scope mặc định) và lựa chọn khôi phục chi tiết đèn hoặc quà màu khi có lựa chọn khả dụng. Điều kiện chính xác, xử lý pool cạn và save nằm trong phụ lục runtime.")

heading(TOC[4][1], "heroes")
prose("Mỗi hero có kit thụ động riêng và một Awakening tự kích hoạt khi đầy ba bậc. Mỗi bậc 10 điểm; có nạp nền 30/35 điểm/giây và trần 30/18 điểm/giây, để lối chơi tự nhiên đạt nhịp khoảng 18–35 giây ngoài thời gian biến hình/nghỉ. Nguồn riêng làm đầy nhanh hơn, không bắt giữ nút hoặc thực hiện nghi thức. HeroIdentity/approach còn tồn tại như lớp tương thích và vài lựa chọn; không mô tả bộ 108 approach cũ như thể thay thế kit Awakening hiện hành.")
table(["Hero", "ID", "Awakening", "Nạp nhanh khi", "Mở ở BestWave"], ((ref(u["Id"], "hero"), u["Id"], kits[u["Id"]]["Transformation"], kits[u["Id"]]["Fill"], u["UnlockWave"]) for u in D["Champions"]))
for n, u in enumerate(D["Champions"], 1):
    ident = u["Id"]; k = kits[ident]; track("Champions", ident)
    heading(f"{n:02d}. {u['DisplayName']} — `{ident}`", "hero-" + ident.lower(), 3)
    prose(u["Description"]); unit_table(u)
    prose(f"**Theme tuyển:** {txt(u['RecruitThemes'])}. **Modifier hero gốc:** {mods(u['ChampionModifiers'])}.")
    prose(f"**{k['Transformation']}** · {k['Seconds']} s · nạp từ {k['Fill']}.\n\n{k['Effect']}")
    table(["Phạm vi", "Thụ động hiện hành", "ID / giá trị thô"], (("Cá nhân" if t["Hero"] else "Đội / kinh tế / hệ thống", t["Text"], f"{t['Kind']} = {txt(t['Value'])}") for t in k["Traits"]))
    prose("**Nguồn nạp:** " + "; ".join(f"{p['Item1']}: {txt(p['Item2'])} điểm/đơn vị" for p in D["GaugeRecipes"].get(ident, [])))
    b = baselines[ident, 1]
    prose("**Starter thực khi Start:** " + ("; ".join(f"{r['UnitId']} / {r['WeaponId']} / {r['Tier']} sao" for r in b["Roster"]) or "không giữ cơ thể starter theo luật hero"))
    table(["Wave tổng hợp", "HP", "Damage cơ bản", "CD s", "Range", "Speed", "Giáp", "Né", "Regen", "Đạn / xuyên"], ((w, *[baselines[ident, w][f] for f in ["HP", "Damage", "Cooldown", "Range", "Speed", "Armor", "Dodge", "Regen"]], f"{baselines[ident,w]['Projectiles']} / {baselines[ident,w]['Pierce']}") for w in [1, 8, 12, 20, 30]))
    if ident in D["WaitingArrivals"]: prose("**Nhập cảnh:** " + txt(D["WaitingArrivals"][ident]))
    details("Pool companion qua lọc runtime, trước unlock / quota / loại shop / trọng số", txt(poolmap[ident].get("ThemeEligibleCompanions", [])))
    details("Pool item qua theme, trước unlock / quota / loại tiền / trọng số", txt(poolmap[ident]["ThemeEligibleItems"]))
    legacy = next((x for x in D["HeroIdentities"] if x["Id"] == ident), None)
    if legacy: details("HeroIdentity / approach còn trong dữ liệu, đọc cùng kit mới", js(legacy), "json")

heading(TOC[5][1], "companions")
prose("94 mục dưới đây là 94 loài trong catalog, không đếm lại theo sao/vũ khí. HP asset không có nghĩa companion chịu chết trong combat. Support theo scope và cấp sao, skill riêng theo trigger/cooldown; Mythic/Prismatic có vũ khí và kỹ năng đặc trưng. Các loài làm Hob/Latch/Postscript/Tally vẫn có entry catalog nhưng phiên bản story party được bố trí riêng và không đem bán.")
table(["Rarity", "Số loài"], Counter(x["Rarity"] for x in D["Companions"]).items())
for n, u in enumerate(D["Companions"], 1):
    ident = u["Id"]; track("Companions", ident)
    heading(f"{n:02d}. {u['DisplayName']} — `{ident}`", "ally-" + ident.lower(), 3)
    table(["Rarity", "Tags / bond", "Unlock wave", "Giá gốc", "GiftOnly", "SpecialRecruit", "ExclusiveHero"], [[u[k] for k in ["Rarity", "Tags", "UnlockWave", "Price", "GiftOnly", "SpecialRecruit", "ExclusiveHero"]]])
    prose(u["Description"])
    prose("**Mô tả talent/support/skill từ UI:**\n\n" + u.get("FullCompanionDescription", "—"))
    unit_table(u)
    prose("**Modifier support gốc:** " + mods(u["CompanionModifiers"]))
    life = D["WaitingLives"].get(ident)
    if life:
        table(["Đời sống / tuyến hiện có", "Nội dung"], life.items())
    if u.get("Dialogue"): prose("**Mẫu thoại được xuất:** " + " / ".join(u["Dialogue"]))

heading(TOC[6][1], "weapons")
prose("Liệt kê cả vũ khí catalog và asset ngoài catalog. Kind=Melee/Projectile/Contact, Family/Signature/Mechanic/Arsenal là các lớp hành vi khác nhau. Số damage, range, cooldown ở đây là gốc; đòn theo nhịp, homing, hồi đạn, nổ, nảy hoặc nhiều mục tiêu khiến Damage/CD không phải DPS thực. Vũ khí cầm là lớp sprite riêng; awakening có thể thay đòn, thay vật cầm hoặc triệu hồi một hình thể.")
for group, label in [("Weapons", "Catalog người chơi"), ("OtherWeaponAssets", "Asset ngoài catalog — địch, boss, checkpoint hoặc dạng đặc biệt")]:
    add(f"### {label}\n")
    for n, w in enumerate(D[group], 1):
        track(group, w["Id"])
        heading(f"{n:03d}. {w['DisplayName']} — `{w['Id']}`", "weapon-" + w["Id"].lower(), 4)
        prose(w.get("GameplaySummary") or w.get("Description") or "Không có copy tóm tắt; xem thông số và snapshot.")
        table(["Kind", "Damage", "CD / windup / recovery s", "Range", "Element", "Đạn / xuyên / tách / nảy"], [[w["Kind"], w["Damage"], f"{txt(w['Cooldown'])} / {txt(w['Windup'])} / {txt(w['Recovery'])}", w["Range"], w["Element"], f"{w['ProjectileCount']} / {w['ProjectilePierce']} / {w['ProjectileSplit']} / {w['ProjectileBounceChance']}"]])
        fields(w, ["Arsenal", "Family", "Mechanic", "Signature", "SignatureBeats", "SignatureRadius", "SignatureScale", "SignatureDelay", "MarkLifetime", "PushDistance", "ProjectileSpeed", "ProjectileLifetime", "ProjectileSpread", "HomingOutbound", "ProjectileRadius", "SwingAngle", "ConeDot", "SpecialOnly", "SecretRecall", "Motion"])

heading(TOC[7][1], "items")
prose("Đủ 200 item trong catalog, gồm FOOD / DRINK / CURIO. Food và drink tính giao dịch ngay lúc mua, không có bước dùng riêng; hiệu ứng ‘while owned’ tồn tại theo stack. Có thể có modifier có lợi, đổi chác, proc, buff một wave, tăng trưởng lưu run, hay tương tác vũ khí. Thứ tự nguồn số: EffectiveModifiers và luật runtime, sau đó mô tả; cặp Effect/Value legacy không ghi đè mảng Modifiers. Enum Rainbow hiển thị Prismatic; Crimson còn tên enum vì tương thích nhưng không tự tạo thành rarity đang bán.")
table(["Rarity", "Số item"], Counter(i["Rarity"] for i in D["Items"]).items())
families = sorted({t for item in D["Items"] for t in item["ShopThemes"] if t.startswith("fam-")})
add("### Các họ item hiện có theo tag runtime\n")
table(["Họ", "Hero có tag này", "Item thành viên"], ((f, [u["Id"] for u in D["Champions"] if f in u["RecruitThemes"]], [i["Id"] for i in D["Items"] if f in i["ShopThemes"]]) for f in families))
for n, i in enumerate(D["Items"], 1):
    track("Items", i["Id"])
    heading(f"{n:03d}. {i['Name']} — `{i['Id']}`", "item-" + i["Id"].lower(), 3)
    prose(i["Description"])
    table(["Rarity / loại", "Giá vàng gốc", "Altar gốc", "Unlock", "Pool", "Trọng số", "Stack"], [[f"{i['Rarity']} / {i['Category']}", i["Price"], f"{i['AltarPrice']} {i['AltarCurrency']}", i["UnlockWave"], i["Pools"], i["ShopWeight"], f"{i['Stacking']} / {i['EffectiveStackLimit']}"]])
    prose("**Modifier mỗi bản có hiệu lực:** " + mods(i["EffectiveModifiers"]))
    prose("**Scope, điều kiện, con số và proc trong UI hiện tại:**\n\n" + i["FullDescription"])
    fields(i, ["Mechanic", "SignatureRole", "ShopThemes"])
add("### Thursday Shoes — item thưởng ngoài 200 mục catalog\n")
details("Dữ liệu đầy đủ của Thursday Shoes", js(D["ActOne"]["ThursdayShoes"]), "json")

heading(TOC[8][1], "bonds")
prose("Bond được tính từ roster thực và các luật hero/rarity, không tính bốn story slot. `ActiveLevel = floor(count / Threshold)`, cộng ExtraBonuses đã đạt. Thưởng dương phần lớn nhân 0.9; BulletPierce/BulletSplit/Projectiles là số nguyên nên miễn hệ số 0.9 để tránh +1 bị làm tròn thành 0. EnemyCount/EnemyHealth/EnemySpeed dùng luật riêng. Rare chỉ cộng thêm đóng góp bond phụ sau khi bond phụ đã bật; skill/hero có thể nâng cấp hoặc nhân đếm. Không coi sao là ba loài riêng.")
for b in D["Bonds"]:
    track("Bonds", b["Id"])
    heading(f"{b['Name']} — `{b['Id']}`", "bond-" + b["Id"].lower(), 3)
    fields(b, ["Threshold", "Effect", "Value", "ExtraBonuses"])
    members = [u["Id"] for u in D["Companions"] if b["Id"] in u["Tags"]]
    prose("**Thành viên theo tag:** " + txt(members))
    def reward(c):
        v = (c // max(1, b["Threshold"])) * b["Value"] + sum(x["Value"] for x in b["ExtraBonuses"] if c >= x["Threshold"])
        return v * (0.9 if v > 0 and b["Effect"] not in {"EnemyCount", "EnemyHealth", "EnemySpeed", "BulletPierce", "BulletSplit", "Projectiles"} else 1)
    table(["Count đã tính", "Cấp thường", "Thưởng tổng sau quy tắc bond, trước compile"], ((c, c // max(1, b["Threshold"]), reward(c)) for c in [0, 1, 2, 3, 4, 6, 8]))

heading(TOC[9][1], "recipes")
prose("25 công thức là dữ liệu catalog, không phải 29. Recipe có hai hoặc ba nguyên liệu, tier tối thiểu, chi phí trứng và tier kết quả. Kết quả giữ vũ khí theo quy tắc nguyên liệu; kiểm tra khả dụng và không trùng biến thể áp dụng tại giao dịch.")
table(["ID", "Nguyên liệu 1", "Nguyên liệu 2", "Nguyên liệu 3", "Kết quả", "Trứng", "Tier tối thiểu → kết quả"], ((r["Id"], r["FirstUnit"], r["SecondUnit"], r["ThirdUnit"], r["ResultUnit"], r["EggCost"], f"{r['MinimumMaterialTier']} → {r['ResultTier']}") for r in D["Recipes"]))
coverage["Recipes"] = [r["Id"] for r in D["Recipes"]]

heading(TOC[10][1], "builds")
prose("Build gồm hero/kit, vũ khí, đội hình/sao, bond, relic, proc và các trạng thái riêng. Morph là hệ nhận diện/nhịp đòn gắn với nhóm stat, không phải một hero mới. Các chuỗi liên kết chỉ phát khi đúng trigger và vũ khí, có cause/cooldown/trần để hạn chế chuỗi vô hạn. Thiết kế đồ cầm tay/sprite không đại diện một cơ chế nếu nhánh runtime không dùng nó.")
add("### Các tương tác và biến cố trong arena\n")
prose("Ngoài đòn vũ khí, mã còn có vật tương tác arena, các đợt spectacle và giai đoạn arena boss. Chúng được bật/tắt theo CheckpointCatalog; một số trận truyện/bí mật cố ý tắt, nên không phải danh sách dưới đây đều xuất hiện trong mọi map. Tên enum là tên kỹ thuật, không phải nhãn theme cuối cùng hiển thị cho người chơi.")
table(["Hệ", "Các loại có trong runtime"], [(k, D["Enums"].get(k, [])) for k in ["ArenaToyKind", "ArenaToyMode", "SpectacleKind", "ExpandedFieldKind"]])
prose("Arena toy có nhịp nạp → cảnh báo → hoạt động/di chuyển → cooldown. Ví dụ chuông cần 3 charge, các loại còn lại 2; cảnh báo gốc 1.2 s, cooldown 5 s. Có chuông, xe, bình mực, búa treo, gương ngủ và hộp nhạc; các signature relic tương tác với charge/kill của chúng. Mặt sàn có thể mang tea/iron/ink, projector/link, fuse, shockwave, shadow pool hoặc vật rơi; phải phân biệt field có lợi với hazard gây sát thương. Vật cảnh bình thường là trang trí, không mặc định có collider gây damage.")
for m in D["Morphs"]:
    add(f"### {m['Name']} · {m['Kind']}\n")
    prose(m["Pattern"]); prose("**Stat liên quan:** " + txt(m["Affinities"]))
    prose("**Mô tả silhouette trong dữ liệu:** " + m["Silhouette"] + " (cần đối chiếu presentation hiện hành; một số phụ kiện hình thể đã bị bỏ).")
coverage["Morphs"] = [m["Kind"] for m in D["Morphs"]]
table(["Build link", "ID", "Item yêu cầu", "Hiệu ứng / điều kiện"], ((b["Name"], b["Id"], b["ItemIds"], b["Description"]) for b in D["BuildLinks"]))
coverage["BuildLinks"] = [b["Id"] for b in D["BuildLinks"]]
prose("Các hướng review build: hero nhiều buff/ít đồng minh; army dùng bond; melee ép sát; projectile xuyên/nảy/tách; collision; kinh tế chuyển sức mạnh; item proc/weapon signature; solo/hấp thụ. Không có kết luận mọi hướng mạnh ngang nhau chỉ từ bảng stat. Các báo cáo balance lịch sử được nhúng ở phần review để thấy cách đo và giới hạn bot.")

heading(TOC[11][1], "enemies")
prose("Pool gốc có 28 loài; roster Miền Chờ thêm 8 loại có art, trong đó Waking Sitter chỉ ở trận ẩn. Tên save/ID cũ được giữ khi đổi art: CoinLeech → Toll Tick, SporeBishop → Coat Rack, SmokingCoffin → Unclaimed Trunk, HopperKeg → Hatbox Hopper, BristleImp → Splinter Imp. Hai nửa thư và tem là đơn vị sinh từ Sewn Letter, không cộng thành loài bán trong catalog.")
for n, u in enumerate(D["Enemies"] + waiting, 1):
    ident = u["Id"]; track("EnemiesAll", ident)
    name = artmap.get(ident, {}).get("DisplayName") or u["DisplayName"]
    heading(f"{n:02d}. {name} — `{ident}`", "enemy-" + ident.lower(), 3)
    prose(u["Description"]); unit_table(u)
    fields(u, ["Behavior", "Shot", "WaitingKind", "Material", "Mutation", "SpecialEnemy", "EvolutionStage", "UnlockWave", "KnockbackResistance"])
    evolution = next((e for e in D["EnemyEvolution"] if e["Id"] == ident), None)
    if evolution: details("Tiến hóa / mutation", js(evolution), "json")
embed_doc("Docs/Enemy_Roster_v2.md", "Phạm vi xuất hiện và cơ chế riêng của roster Miền Chờ", "Tài liệu thiết kế cơ chế để đối chiếu; các con số runtime chi tiết nằm trong WaitingEnemies/WaitingEnemiesRun. Tất cả 15 file art Enemy_* đã tồn tại ở thời điểm xuất, không đồng nghĩa đã chơi thử mọi chuyển trạng thái.")
details("Enum affix, mutation, hành vi và kiểu đạn", js({k: v for k, v in D["Enums"].items() if any(s in k.lower() for s in ["enemy", "affix", "mutation", "hostile", "behavior"])}), "json")

heading(TOC[12][1], "bosses")
prose("Phân biệt 10 boss asset nguồn với lịch boss Hồi 1. Hồi 1 hiện đóng vùng ở 5/10/15 và finale 20. Chair Porter dùng nguồn SuitcaseLeviathan; Door That Stayed Shut dùng UndeliveredKing. Keeper là nhân vật riêng, có trận thử tùy chọn shop 17; THE BEDSIDE dùng UmbraCradle trong SIT CLOSER. Boss miễn knockback, có telegraph, bộ phận và nhịp đạn; một số mẫu encore dùng chung có biến thể.")
for u in D["Bosses"] + [units[i] for i in ["OutsideStageKeeper", "TheLastCustomer"]]:
    ident = u["Id"]; track("BossesAll", ident)
    heading(u["DisplayName"] + f" — `{ident}`", "boss-" + ident.lower(), 3)
    prose(u["Description"]); unit_table(u)
    fields(u, ["BossPattern", "Motif", "Behavior", "Weapon", "KnockbackResistance"])
    anatomy = next((p for p in D["BossAnatomy"] if p["BossId"] == ident), None)
    if anatomy: details("Bộ phận và vai trò", js(anatomy), "json")
prose("HP boss có nhiều lần ghi đè rồi nhân relief; không lấy source×20 cũ làm HP Keeper hiện tại. Bảng wave ở mục 03 đã gọi PrepareCombat thực và giữ HP cuối của preview. Có toàn bộ 120 preview (20 wave × 6 difficulty) trong phụ lục; đây là kiểm tra biên dịch, không phải kết quả thắng/thua.")

heading(TOC[13][1], "secrets")
prose("Hồi 1 có lời mời bí mật ở các shop thuộc nhịp `wave % 5 == 3` (3, 8, 13, 18), cùng các lựa chọn cốt truyện riêng. Tiến trình, lựa chọn, đồ tạm gửi và hợp đồng được lưu trong run. Phòng bí mật vừa có lựa chọn thương lượng vừa có chiến đấu/phần thưởng; không phải tất cả đều là một arena phụ.")
for r in D["SecretRooms"]:
    heading(r["Title"] + f" — {r['Kind']}", "secret-" + str(r["Kind"]).lower(), 3)
    prose(r["Hint"]); prose("**Giới thiệu sớm:** " + r["EarlyIntroduction"]); prose("**Giới thiệu muộn:** " + r["LateIntroduction"])
    details("Lựa chọn preview ở wave 11, ví fixture 200 vàng / 20 trứng", js(r["Choices"]), "json")
coverage["SecretRooms"] = [r["Kind"] for r in D["SecretRooms"]]
prose("**SIT CLOSER:** mở từ shop 8 đến 19, một lần/run, ngồi ở Chiếc Ghế Ấm. THE BEDSIDE có độ mạnh wave hiện tại +8 (chặn 40), hạn tiếng gõ 80 s. Thua kết thúc run. Thắng về cùng lượt shop, nhận Thursday Shoes và chọn hồi lại ký ức đèn hoặc quà màu nếu khả dụng. Waking Sitter chỉ đứng dậy giữ chân ở trận này. **Hob shop 16:** cả đoàn kéo dậy; runtime có trạng thái HobRescueNeeded và có thể tạm chưa đưa Hob vào story party đến khi tương tác xong, khác mô tả cũ ‘không rời đội’.")
details("Ký ức theo mọi lựa chọn phòng", js(D["EventMemories"]), "json")

heading(TOC[14][1], "stats")
prose("Scope Team=cả đội; Champion=hero; Companions=đồng minh; Run=kinh tế/hệ thống. HP/damage có thể là số thực; cooldown tính giây, speed/range theo world unit. Support companion tăng theo ModifierScale 1/2/4 ở sao 1/2/3; tier damage/support còn có hệ số riêng khi compile. Đừng nhân một tài năng phần trăm theo sao nếu copy không nói vậy.")
table(["ID stat", "Tên UI", "Scope mặc định", "UI dùng %"], ((s["Key"], s["Name"], s["DefaultScope"], s["Percent"]) for s in D["StatGlossary"]))
prose("**Giới hạn và phép tính quan trọng:** damage có soft knee; tốc đánh hero và companion dùng slope/cap khác nhau trong PlayerBuildBalance; compile companion có cooldown sàn 0.28 s, tầm melee tối đa 3.4 / loại còn lại 8, đạn tối đa 10, tốc độ unit tối đa 10. HP hero qua soft cap 250 và giới hạn theo wave, có ngoại lệ như Ivory Sovereign mất từng đơn vị HP. Crit, dodge, giáp, collision, nguyên tố và secondary proc có nhánh riêng. Boss không bị đẩy, địch thường bị đẩy tối đa 0.45 trước kháng. Những giới hạn này không có nghĩa mọi proc hoặc đòn biến hình dùng cùng phạm vi.")
prose("**Trạng thái và tương tác:** đòn vật lý/Ember/Frost/Arcane; slow/chill/freeze/sleep, đánh dấu/exposure, nợ damage, poison, wet/shock, pin/root và knockback được tạo bởi vũ khí, item, hero hoặc skill tương ứng. Không có một thời lượng chung cho mọi slow hay freeze: đọc mô tả nguồn gây hiệu ứng. Các đòn phân biệt windup, release, recovery, range, góc đánh, projectile lifetime/pierce/bounce/split và damage contact; số damage trên thẻ không chứng minh đòn đã trúng. Companion bất tử không làm hero miễn collision hoặc hazard.")
details("Toàn bộ constant runtime xuất từ assembly hiện tại", js(D["RuntimeConstants"]), "json")
prose("Người review nên so kết quả ở cùng hero/seed/độ khó, cùng ngân sách vàng và quota, cả sát thương lên đơn mục tiêu lẫn nhiều mục tiêu, thời gian di chuyển, khả năng đọc đòn và mất HP. Bot balance chỉ chứng minh tình huống của bot, không chứng minh trải nghiệm con người hoặc mọi build ngang sức.")

heading(TOC[15][1], "controls")
table(["Tác vụ", "Bàn phím / chuột", "Thiết bị khác / ghi chú"], [
    ("Di chuyển", "WASD / phím mũi tên", "Cần trái; touch kéo vùng điều khiển"),
    ("Đánh / Awakening", "Tự động", "Không phải giữ Space để nạp Awakening"),
    ("Shop / cửa / vật thể", "Đến gần + Space hoặc click hình vật thể", "A hoặc nút touch tương ứng; khoảng tương tác neo sàn, click theo bounds đang nổi"),
    ("Pause / quay lại", "Esc", "Start / B theo màn"),
    ("Chi tiết Brigade", "I; Tab/điều hướng trong bảng", "Y và LB/RB ở bảng Brigade; không nhầm với shortcut K/Y/J cũ của journal"),
    ("Journal và Paths & People", "Dùng tủ lưu trữ / ghế hoặc điểm tương tác trong world", "Không dùng các nút HUD/phím tắt K/J cũ"),
    ("Truyện / lời kết / quà", "Space/Enter/click theo lựa chọn đang mở", "Có chặn input xuyên sang shop; một số lựa chọn cần chọn mục rồi tương tác")])
prose("HUD gồm HP, wave, thời gian, vàng, trứng, bond, gauge riêng hero, shard/quà và thông tin mục tiêu. Bảng chi tiết cho phép xem roster, relic, vũ khí, stat và đóng góp. Shop description có icon stat, số có màu tăng/giảm, viền theo rarity và preview bond sau mua. Tooltip phải cho biết phạm vi áp dụng, điều kiện và cooldown thay vì chỉ lore.")
prose("Popup stat sau giao dịch không có box: icon + số nguyên/chữ đậm; thay đổi nhỏ hơn 1 ghi +<1/−<1, xuất hiện rồi trôi/fade; giảm chuyển động giữ fade. Hiển thị thay đổi thực của hero và bạn bị ảnh hưởng, xóa khi vào combat. Speech bubble giữ vị trí sau khi xuất hiện để người chơi đọc. Đồ cảnh rơi khi vừa vào shop mới, không lặp khi reroll/mua; đèn được miễn animation rơi.")
embed_doc("Docs/Painted_Stats_And_Companion_Reactions.md", "Icon stat và lời bình theo tình huống", "Bản ghi triển khai theo thời điểm của tài liệu; lần xuất hồ sơ này không chạy lại các capture/test được kể bên trong.")
prose("Tùy chọn có âm lượng tổng/nhạc/SFX, giảm chuyển động/flash/effect/rung/audio, High Readability và các dấu/số phản hồi. UI có scale/safe area, keyboard/controller/touch. Việc có handler touch không chứng nhận game đã đạt QA mobile. Input/modal/pause và lưu tiến độ journal có mã nhúng ở cuối file.")

heading(TOC[16][1], "progression")
prose("Profile giữ hero/độ khó mở khóa, achievement, museum/sưu tập, records và lịch sử run (tối đa 50). Hero unlock theo BestWave và UnlockWave; difficulty tiếp theo theo thắng bậc trước. Run lưu seed/RNG, phase/wave, gold/eggs, roster/weapon/tier, item stacks, quota/cooldown, tăng trưởng, lựa chọn truyện, hợp đồng và boss ẩn. Storage có kiểm tra dữ liệu và đường dự phòng; hiện là lưu cục bộ, không suy ra có cloud save hoặc bảng xếp hạng trực tuyến.")
prose("Điểm gồm hall clear×1000, kills×10, boss kills×500, +5000 khi thắng, nhân độ khó và ScoreMultiplier của build, chặn số nguyên. Endless có ParentRunId và bộ đếm chặng riêng; profile tránh ghi nhận lặp cùng RunId. Museum ghi các thứ đã gặp, không đồng nghĩa mọi thứ đều mở bán.")
table(["Achievement", "ID", "Điều kiện", "Metric", "Mục tiêu"], ((a["Name"], a["Id"], a["Description"], a["Metric"], a["Goal"]) for a in D["Achievements"]))
coverage["Achievements"] = [a["Id"] for a in D["Achievements"]]

heading(TOC[17][1], "visuals")
prose("Hướng art hiện hành: vẽ tay nhiều lớp kiểu sách truyện, viền mực có chủ ý, vật liệu thủ công hơi lệch, đồng thau cũ / hổ phách / kem / teal đậm / xanh nửa đêm. Phe ta không dùng glow đỏ nguy hiểm; đỏ rượu chỉ là vật liệu. Quái có ember/đỏ và vật liệu tối. Miền Chờ là đồ thất lạc, nhà ga đêm, thư, ghế, đèn và Sương Đen; không đổi toàn bộ thành đồ chơi hay rạp xiếc.")
prose("Đồ cảnh, quầy, vật tương tác, cửa và đèn lơ lửng: trôi ngang, lên xuống, nghiêng lệch pha; bóng neo sàn/điểm tương tác ổn định. Đã bỏ gió/khói dưới chân vật theo yêu cầu; Sương Đen môi trường là lớp riêng. Props được rải theo world map và hư dần theo chặng, không bám camera hoặc đột ngột sinh cả cụm từ wave 11. Đèn có biến thể vị trí/góc/kích thước và trạng thái nguyên/chập chờn/vỡ.")
prose("Càng sâu ánh sáng môi trường rút đi, đoàn tự tạo vùng sáng quanh hero và từng companion. SharedLightField dùng texture world-space 256×192, batch 16 nguồn và gộp tối đa; Capacity=16 là số nguồn mỗi batch, không phải cap đội. HeroRadius=6.4. Đèn trong vùng Sương Đen cũng bị lu mờ. Mắt/ember xa gắn vào địch thật; không dùng tám mắt trang trí như bằng chứng đã có địch thật. Mắt đỏ là hiệu ứng đọc quái, không là lời giải cốt truyện. Telegraph/đạn/contact và shop vẫn phải đọc được, High Readability/Reduced Effects giữ thông tin thiết yếu.")
prose("Animation gồm sheet idle/walk/anticipation/release, nhịp diễn xuất, chuyển sao giữ thân lớn hơn, attack choreography, hiệu ứng mực/vật liệu, rarity cover và Awakening. Gauge2 là hình thanh riêng theo hero (mặt trăng, tem, tàu, đồng hồ…), có Empty/Full khớp nhau; Gauge_* cũ vẫn tồn tại trong asset, không tự là hệ UI đang được dùng. Các asset Blender lịch sử đã được thay bằng art vẽ tay theo yêu cầu; sự tồn tại nguồn Blender không là style hiện tại.")
table(["Nhóm", "Có AuthoredMotion", "Có AnimationFrames"], ((g, sum(bool(u.get("Motion")) for u in D[g]), sum(bool(u.get("AnimationFrames")) for u in D[g])) for g in ["Champions", "Companions", "Enemies", "Bosses"]))
prose("Đếm trường animation chỉ xác nhận liên kết dữ liệu, không chứng minh tất cả frame đẹp, đúng pivot hoặc không lỗi UV trong mọi trận. Worklist/progress art và manifest theo file được nhúng ở phụ lục để review coverage mà không phải mở repo.")

heading(TOC[18][1], "audio")
music_path = ROOT / "Assets/ArcaneBrigade/Resources/Audio/Arcane/Music/music.json"
cues_path = ROOT / "Assets/ArcaneBrigade/Resources/Audio/Arcane/cues.json"
music = json.loads(music_path.read_text(encoding="utf-8-sig"))
cues = json.loads(cues_path.read_text(encoding="utf-8-sig"))
prose(f"Manifest hiện tại có **{len(music['pieces'])} bản nhạc / {sum(len(p['stems']) for p in music['pieces'])} stem** và **{len(cues['cues'])} cue âm thanh**. Có phân lớp nhạc, combat SFX, môi trường, UI, merchant voice/gibberish và motif truyện. Có clip không đồng nghĩa đã có hook runtime; contract phân biệt W (đã nối), P (chưa nối) và R (dự trữ).")
table(["Bản nhạc", "Thông tin manifest"], ((p.get("id", p.get("name", "—")), {k: v for k, v in p.items() if k != "stems"}) for p in music["pieces"]))
prose("Combat audio có ưu tiên danger/UI/cosmetic, cooldown/giới hạn voice, gom batch, culling và giảm âm companion để không thành tiếng ồn theo từng viên đạn. Pause/focus và reduced audio có đường xử lý riêng. Giai điệu ru có phần bị giấu và đoạn bình minh, nhưng không tự coi mọi thoại đã có voice acting thu âm. Cần nghe trong trận thật để đánh giá tiếng boss, âm báo đèn, chuyển vùng và độ rõ của lời thoại.")
details("Toàn bộ cue: ID, clip, gain, cooldown, pitch, giới hạn voice", js(cues), "json")
details("Toàn bộ score và stem", js(music), "json")
embed_doc("Docs/Audio/Audio_Integration_Contract.md", "Hợp đồng âm thanh và trạng thái nối — giữ nguyên ghi chú phiên bản lịch sử")

heading(TOC[19][1], "review")
table(["Điểm đối chiếu", "Trạng thái / giới hạn"], [
    ("Catalog cũ", f"Thay số cũ bằng {counts['Champions']} hero, {counts['Companions']} companion, {counts['Items']} item, {counts['Bonds']} bond, {counts['Achievements']} achievement đã được Unity đọc."),
    ("Boss wave 5", "Canon 16/10 ghi Keeper; ActOneCanon.BossName và StoryBoss hiện chạy Coin Widow. Cần thống nhất truyện/art/thoại khi review."),
    ("Thời lượng wave", "Cấu hình nền 45 s đã bị WaveCadence override; xem 120 preview. Không áp báo cáo 45 s mọi màn."),
    ("Quota", "Tài liệu shop 16/10 ghi 3 lượt mua; RecruitmentRules 19/10 hiện 1 bản sao/wave và nhịp loài mới thường mỗi 2 wave. Ưu tiên code."),
    ("Quota qua hai shop", "Code hiện bất đối xứng: SpecialNewRecruitsRemaining kiểm tra LastNewShopSpeciesWave, còn RegularRecruitOpen chỉ kiểm tra LastRegularNewSpeciesWave. Vì vậy không khẳng định chiều dream → thường đã khóa chung như thiết kế cũ; cần review tình huống này."),
    ("Story party", "4 loài có data chung nhưng không là hàng bán; Hob có trạng thái cần cứu ở shop 16."),
    ("Awakening và approach", "Có kit mới tự biến hình; HeroIdentities/approach cũ vẫn trong nguồn/UI. Đọc cùng nhánh Kit, không cộng cả hai bộ tác dụng một cách máy móc."),
    ("Stand-in boss", "Chair Porter / Door / Bedside dùng thân boss nguồn; chưa phải mọi boss truyện có bộ art/anatomy riêng."),
    ("Art đã có", "File tồn tại và liên kết motion không chứng minh mọi pose, gauge, tint, silhouette đã duyệt trực quan."),
    ("Hồi 2 và 3", "Để sau theo yêu cầu; hồ sơ không coi chúng là chiến dịch đã chơi được."),
    ("Bằng chứng lần xuất này", "Unity chạy exporter thành công, tạo snapshot mới và các preview tạm; không chơi full campaign, không đo FPS hay nghe âm thanh mới trong lượt làm tài liệu này.")])
prose("**Đề bài cho người review:** đánh giá từng mục 1–5, ghi ví dụ cụ thể và mức ưu tiên. (1) Cốt truyện: có hiểu vì sao bị giữ, ai giữ tên, ai đã đến đón, tám đèn và kết thúc? (2) Gameplay: vòng đánh/shop có nhịp, hero khác nhau, Awakening đọc được? (3) Build: đầu tư buff có cạnh tranh với mua companion, bond có làm build bắt buộc? (4) Độ khó: sáu bậc có dốc rõ, boss mất bao lâu, chết có hiểu lý do? (5) Theme: vật nào lạc tông, hiệu ứng nào che đòn, quái nào khó nhận? (6) UX: preview mua đúng kết quả, thao tác cửa/vật nổi có dễ, text có đọc được khi di chuyển? (7) Audio: âm báo quan trọng có nổi trên đám đông và nhạc? (8) Tiến trình: quà hiếm/ghép/mở khóa có đáng chờ, save/continue có giữ đúng lựa chọn?")
prose("Đầu ra review nên có: ba điểm mạnh; năm vấn đề lớn có tình huống tái hiện; các nội dung trùng vai trò hoặc thiếu payoff; đề xuất cắt/sửa/thêm theo mức ảnh hưởng và công sức. Các số bot bên dưới là lịch sử kiểm tra, không phải bảo đảm cân bằng hiện tại hoặc tỷ lệ thắng của người chơi.")
embed_doc("Docs/Balance_Pass_2026-10-19.md", "Báo cáo cân bằng lịch sử và cách đo")
embed_doc("Docs/Hero_Mechanics_Review.md", "Review cơ chế hero lịch sử", "Báo cáo này có trước/đan xen đợt Awakening, dùng để hiểu quá trình và hạn chế đo, không ghi đè HeroKits hiện tại.")

heading(TOC[20][1], "raw")
prose("Dữ liệu bên dưới giữ mọi trường được exporter đọc: số gốc, mô tả, ref sprite/motion, pool item, tài nguyên/enum, preview, thoại, lựa chọn và cấu hình. Đường dẫn asset là xuất xứ; không cần mở file phụ để đọc nội dung đã nhúng. Sprite/audio binary không được nhúng vào Markdown.")
def snapshot_text():
    # Keep every field, but do not repeat millions of indentation spaces.
    fields_out = []
    compact = lambda x: json.dumps(x, ensure_ascii=False, separators=(",", ":"), allow_nan=False)
    for key, value in D.items():
        encoded = "[\n" + ",\n".join(compact(x) for x in value) + "\n]" if isinstance(value, list) else compact(value)
        fields_out.append(compact(key) + ":" + encoded)
    result = "{\n" + ",\n".join(fields_out) + "\n}"
    assert json.loads(result) == D
    return result
details("Snapshot Unity toàn phần, giữ mọi trường dữ liệu", snapshot_text(), "json")
for path in ["Docs/Content_Expansion_v5.md"]:
    embed_doc(path, "Tài liệu tham chiếu: " + path, "Giữ nguyên nhãn/timestamp gốc; một số câu ‘chưa vào game’ đã lỗi thời. Số liệu runtime ở đầu hồ sơ là nguồn ưu tiên.")
for path in ["Docs/Expansion_Art_Worklist.json", "Docs/Bizarre_Arsenal_Art_Worklist.json", "Docs/Art_Master_Worklist.json"]:
    p = ROOT / path
    if p.exists():
        data = json.loads(p.read_text(encoding="utf-8-sig"))
        jobs = data if isinstance(data, list) else data.get("jobs", [])
        table(["Job art · " + path, "Đường dẫn", "File hiện có", "Phụ thuộc"], ((j.get("name"), j.get("path"), bool(j.get("path")) and (ROOT / j["path"]).exists(), j.get("depends")) for j in jobs))

heading(TOC[21][1], "source")
prose("Đây là phần cho reviewer kỹ thuật tra cứu công thức chính xác, không cần đọc hết khi review nội dung. Nhúng các lớp chứa công thức chính về kinh tế, wave, quota, stat, kit, relic, đồng minh, truyện và ánh sáng; các file runtime còn lại được định danh bằng SHA256. Capture/fixture/test không được coi là nội dung người chơi. Mã chỉ được sao chép vào hồ sơ, không sửa gameplay.")
sources = []
for folder in ["Core", "Data", "Persistence", "Input"]:
    sources += sorted((ROOT / f"Assets/ArcaneBrigade/Runtime/{folder}").glob("*.cs"))
for folder in ["Presentation", "UI"]:
    for p in sorted((ROOT / f"Assets/ArcaneBrigade/Runtime/{folder}").glob("*.cs")):
        if any(s.lower() in p.stem.lower() for s in ["ActOne", "Waiting", "Nightmare", "Light", "Lantern", "Fog", "Dialogue", "Speech", "Levitation", "Story", "HeroAwak", "HeroIdentity", "Shop", "Description", "ReadModel", "Settings", "Input", "Audio", "Music", "Prism", "Farewell", "Outside", "BuildMorph", "Arsenal", "Merge"]): sources.append(p)
exclude = ["capture", "fixture", "checks", "smoke", "framebuffer", "measurement", "recorder", "nativemetrics"]
manifest = []
embedded_sources = {
    "RunSession", "RecruitmentRules", "CarnivalBalance", "JourneyBalance", "PlayerBuildBalance", "OctoberBalance", "WaveCadence",
    "HeroKits", "HeroGauge", "HeroKitCombat", "HeroKitRun", "CompanionSkills", "NewCompanionSkills", "OctoberCompanions",
    "RunRelics", "RelicCombat", "RunCatalog", "ActOneRun", "ActOneCanon", "NightmareRun", "SecretEventRun", "WaitingEnemies",
    "WaitingEnemiesRun", "WaitingEnemyRoster", "NightmareLighting", "SharedLightField", "K11HeroRules", "HeroLoadout"
    , "ArenaToySimulation", "MidwaveSpectacle", "ExpandedCombat", "SecretEventCombat", "NightmareCombat"
}
embedded_count = 0
for p in dict.fromkeys(sources):
    if any(s in p.stem.lower() for s in exclude): continue
    raw = p.read_bytes(); rel = p.relative_to(ROOT).as_posix(); sha = hashlib.sha256(raw).hexdigest()
    manifest.append({"path": rel, "sha256": sha})
    if p.stem in embedded_sources:
        details(rel + " · SHA256 " + sha, raw.decode("utf-8-sig"), "csharp")
        embedded_count += 1
details("Manifest xuất xứ các tệp runtime đã đối chiếu", js(manifest), "json")
heading("Biên bản xuất và kiểm tra độ đầy đủ", "validation", 3)
table(["Danh mục", "Số entry trong phần đọc được"], ((k, len(v)) for k, v in coverage.items()))
prose(f"Snapshot: {D['ExportedAtVietnam']}. SHA256 `{hashlib.sha256(SNAPSHOT.read_bytes()).hexdigest()}`. Nền git `{HEAD}`. Có {embedded_count} tệp mã công thức được nhúng / {len(manifest)} tệp mã được định danh SHA256, {len(D['HeroBaselines'])} baseline hero và {len(D['WavePreviews'])} preview wave/độ khó. Exporter chỉ tạo session tạm để đọc dữ liệu; không rebuild catalog, không lưu profile hoặc thay gameplay. Lần xuất này không xác nhận một bản Windows mới đã được build.")
report = "\n".join(L) + "\n"
expected = {g: [x["Id"] for x in D[g]] for g in ["Champions", "Companions", "Items", "Weapons", "OtherWeaponAssets", "Bonds", "Recipes", "BuildLinks", "Achievements"]}
expected["EnemiesAll"] = [u["Id"] for u in D["Enemies"] + waiting]
expected["BossesAll"] = [u["Id"] for u in D["Bosses"]] + ["OutsideStageKeeper", "TheLastCustomer"]
for group, ids in expected.items(): assert coverage[group] == ids, group
missing_links = set(re.findall(r"\]\(#([^\s)]+)\)", report)) - anchors
assert not missing_links, missing_links
assert "\ufffd" not in report
assert sum(x.startswith("````") for x in report.splitlines()) % 2 == 0
OUT.write_text(report, encoding="utf-8", newline="\n")
validation = {"artifact": OUT.relative_to(ROOT).as_posix(), "snapshot": D["ExportedAtVietnam"], "gitBase": HEAD, "bytes": OUT.stat().st_size, "sha256": hashlib.sha256(OUT.read_bytes()).hexdigest(), "counts": {k: len(v) for k, v in coverage.items()}, "unresolvedLinks": sorted(missing_links), "sourceManifest": manifest, "status": "PASS"}
(ROOT / "Library/GameReview/Validation_Current.json").write_text(js(validation), encoding="utf-8")
print(js({k: v for k, v in validation.items() if k != "sourceManifest"}))
