"""Build one self-contained Vietnamese review dossier from a read-only Unity export.

Run Unity GameReviewSnapshot.Export first, then: python Tools/Review/export_game_review.py
Only the Markdown dossier and Library/GameReview/Validation.json are written here.
"""
from __future__ import annotations

import hashlib
import json
import re
import subprocess
from collections import Counter
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "Docs/Arcane_Brigade_Complete_Review.md"
SNAPSHOT = ROOT / "Library/GameReview/Snapshot.json"
D = json.loads(SNAPSHOT.read_text(encoding="utf-8-sig"))
lines: list[str] = []
anchors: set[str] = set()
coverage: dict[str, list[str]] = {}


def add(s=""):
    lines.append(str(s))


def jdump(x, pretty=False):
    return json.dumps(x, ensure_ascii=False, indent=2 if pretty else None,
                      separators=None if pretty else (",", ":"), allow_nan=False)


def snapshot_records(x):
    """Preserve exact JSON without a megabyte-long editor/rendering line."""
    fields = []
    for key, value in x.items():
        if isinstance(value, list):
            encoded = "[\n" + ",\n".join(jdump(row) for row in value) + "\n]"
        else:
            encoded = jdump(value)
        fields.append(jdump(key) + ":" + encoded)
    result = "{\n" + ",\n".join(fields) + "\n}"
    assert json.loads(result) == x
    return result


def text(x):
    if x is None:
        return "—"
    if isinstance(x, bool):
        return "Có" if x else "Không"
    if isinstance(x, float):
        return f"{x:.7g}"
    if isinstance(x, dict):
        if "id" in x:
            return f"`{x['id']}`"
        if "asset" in x:
            return x.get("name", x["asset"])
        if set(x) <= {"x", "y", "z", "w"}:
            return "(" + ", ".join(text(v) for v in x.values()) + ")"
        return jdump(x)
    if isinstance(x, list):
        return "; ".join(text(v) for v in x) or "—"
    return str(x).replace("\\n", "\n")


def cell(x):
    return text(x).replace("|", "&#124;").replace("\r", "").replace("\n", "<br>")


def table(headers, rows):
    add("| " + " | ".join(cell(h) for h in headers) + " |")
    add("| " + " | ".join("---" for _ in headers) + " |")
    for row in rows:
        add("| " + " | ".join(cell(v) for v in row) + " |")
    add()


def heading(label, anchor, level=2):
    if anchor in anchors:
        raise ValueError("Duplicate anchor: " + anchor)
    anchors.add(anchor)
    add(f'<a id="{anchor}"></a>\n\n{"#" * level} {label}\n')


def details(label, language, body):
    add(f"<details>\n<summary>{label}</summary>\n\n````{language}\n{body.rstrip()}\n````\n\n</details>\n")


def fields_table(o, fields):
    table(["Trường", "Giá trị"], ((k, o.get(k)) for k in fields if k in o))


def track(group, ident):
    coverage.setdefault(group, []).append(ident)


STATS = {s["Key"]: s for s in D["StatGlossary"]}
SCOPE = {"Team": "Cả đội", "Champion": "Nhân vật", "Companions": "Đồng minh", "Run": "Run / hệ thống"}


def modifier(m):
    e, value = m["Effect"], m["Value"]
    v = f"{value * 100:+.7g}%" if STATS[e]["Percent"] else f"{value:+.7g}"
    return f"{SCOPE.get(m.get('Scope'), m.get('Scope', ''))}: {e} {v}"


def modifiers(ms):
    return "; ".join(modifier(m) for m in (ms or [])) or "Không có modifier cộng trực tiếp"


def reference(ident, group):
    return f"[{ident}](#{group}-{ident.lower()})"


units = {u["Id"]: u for group in ["Champions", "Companions", "Enemies", "Bosses", "OtherUnitAssets"] for u in D[group]}
weapons = {w["Id"]: w for group in ["Weapons", "OtherWeaponAssets"] for w in D[group]}
items = {i["Id"]: i for i in D["Items"]}
baselines = {(b["HeroId"], b["Wave"]): b for b in D["HeroBaselines"]}
poolmap = {p["HeroId"]: p for p in D["HeroShopPools"]}
evolutions = {e["Id"]: e for e in D["EnemyEvolution"]}
parts = {a["BossId"]: a["Organs"] for a in D["BossAnatomy"]}

heading("ARCANE BRIGADE — Hồ sơ toàn diện để review", "top", 1)
add(f"**Ảnh chụp dữ liệu:** {D['ExportedAtVietnam']}. **Unity:** {D['UnityVersion']}. **Catalog:** `{D['Catalog']['Version']}`. **Ngôn ngữ hồ sơ:** tiếng Việt; tên, lời thoại và ID giữ nguyên để đối chiếu game.")
add()
add("**Có spoiler toàn bộ cốt truyện, phòng bí mật và kết thúc.** Đây là hồ sơ của bản mã nguồn và asset đang có trong workspace, gồm cả thay đổi chưa commit. Nó mô tả bản triển khai hiện tại, không phải lời hứa thiết kế của các tài liệu cũ. Chỉ cần gửi **file Markdown này** cho người review: các bảng, dữ liệu thô, lời kết, manifest âm thanh và công thức thực thi cần tra cứu đều nằm bên trong.")
add()
add("Cách đọc chỉ số: **gốc** là giá trị trong asset; **có hiệu lực** là modifier/cơ chế được code sử dụng; **biên dịch trận** là số sau khi ghép hero, sao, relic, bond, wave và cân bằng; **trong trận** còn chịu trạng thái, proc và mục tiêu. Không thể gán một DPS duy nhất cho một build có đạn xuyên, nhiều mục tiêu, hồi đòn và phản ứng nguyên tố. Các ví dụ biên dịch ở đây không phải số đo chơi thử.")
add()
toc = [
    ("overview", "01. Tổng quan, vòng chơi và hướng dẫn"), ("controls", "02. Điều khiển, giao diện và tùy chọn"),
    ("modes", "03. Mode, độ khó, mở khóa và lưu tiến trình"), ("stats", "04. Toàn bộ chỉ số, phép tính và giới hạn"),
    ("economy", "05. Vàng, loot, shop, altar và nâng sao"), ("heroes", "06. Toàn bộ nhân vật điều khiển"),
    ("companions", "07. Toàn bộ đồng minh"), ("weapons", "08. Toàn bộ vũ khí và thông số"),
    ("relics", "09. Toàn bộ relic, đồ ăn, đồ uống"), ("bonds", "10. Toàn bộ bond"),
    ("recipes", "11. Toàn bộ công thức altar"), ("builds", "12. Build, hình thái và chuỗi liên kết"),
    ("enemies", "13. Toàn bộ địch, tiến hóa, affix và nhịp wave"), ("bosses", "14. Toàn bộ boss và bộ phận"),
    ("secrets", "15. Cổng bí mật, hợp đồng và phần thưởng"), ("story", "16. Cốt truyện và toàn bộ kết thúc"),
    ("achievements", "17. Toàn bộ achievement"), ("presentation", "18. Mỹ thuật, animation và khả năng đọc trận"),
    ("audio", "19. Toàn bộ nhạc và âm thanh"), ("review-notes", "20. Giới hạn hiện tại và gợi ý kiểm tra review"),
    ("raw", "21. Phụ lục dữ liệu đầy đủ, cấu hình và lời thoại"), ("source", "22. Phụ lục code công thức thực thi và nguồn gốc")]
for anchor, label in toc:
    add(f"- [{label}](#{anchor})")
add()

heading("01. Tổng quan, vòng chơi và hướng dẫn", "overview")
add("Arcane Brigade là game sinh tồn theo wave nhìn từ trên xuống, nhân vật và đồ chơi tự chiến đấu. Người chơi tập trung vào vị trí, đường né đạn, chọn đội hình, quản lý tiền và quyết định rủi ro trong các cửa hàng. Một cửa tiệm đồ chơi ấm áp dần lộ ra hình hài một giấc mơ có cơ thể, ký ức và nỗi sợ phải chia tay.")
add()
table(["Nội dung đang có", "Số lượng / quy tắc"], [
    ("Nhân vật điều khiển", len(D["Champions"])), ("Loài đồng minh", len(D["Companions"])),
    ("Vũ khí trong catalog người chơi", len(D["Weapons"])),
    ("Asset vũ khí khác, chủ yếu của địch/boss/checkpoint", len(D["OtherWeaponAssets"])),
    ("Relic / đồ ăn / đồ uống", len(D["Items"])), ("Bond", len(D["Bonds"])), ("Công thức altar", len(D["Recipes"])),
    ("Hình thái build", len(D["Morphs"])), ("Liên kết item có tên", len(D["BuildLinks"])),
    ("Địch trong pool", len(D["Enemies"])), ("Loài có tiến hóa riêng", len(D["EnemyEvolution"])),
    ("Boss thường trong catalog", len(D["Bosses"])), ("Boss đặc biệt", "Outside Stage Keeper; The Last Customer"),
    ("Loại phòng bí mật", len(D["SecretRooms"])), ("Achievement nhật ký", len(D["Achievements"])),
    ("Chiến dịch", "20 wave; wave thường 45 giây; boss mỗi 4 wave"),
    ("Đội hình", "Tối đa 20 cơ thể đồng minh; tối đa 3 sao; một số hero chơi solo hoặc hấp thụ kỹ năng")])
add("Một run diễn ra theo chuỗi: chọn hero và mode → chiến đấu tự động trong khi điều khiển vị trí → nhận vàng/loot và nghỉ ở shop → mua relic, tuyển quân, ghép sao hoặc dùng altar → cân nhắc cổng bí mật → đi qua cửa sang wave tiếp theo. Wave 20 dẫn đến The Last Customer và lựa chọn lời chào tạm biệt. Hoàn thành lời kết rồi có thể chọn chơi Endless với build hiện có.")
add()
add("Cách bắt đầu: giữ đủ khoảng trống để né các vùng báo trước; nhặt vàng, trứng, tim và mở rương; đọc tooltip về hiệu ứng và đổi chác trước khi mua. Ba đồng minh cùng loài và cùng sao tự ghép. Giữ tiền có lợi tức, nhưng đội hình yếu sẽ làm việc né boss khó hơn. Khi đội đông, shop giảm việc đưa thêm loài mới và ưu tiên mảnh ghép. Hiệu ứng di chuyển và tự động truy đuổi không thay thế việc người chơi né vùng nguy hiểm.")
add()
add("Nhịp chiến dịch tối thiểu theo thời gian wave là khoảng **15 phút chiến đấu** (20 × 45 giây); thời gian thực dài hơn do cửa hàng, sự kiện, boss overtime và các trận không giới hạn giờ. Thua khi HP nhân vật về 0; đồng minh hiện bất tử, không phải đội quân có hệ tử trận riêng.")
add()

heading("02. Điều khiển, giao diện và tùy chọn", "controls")
table(["Tình huống", "Bàn phím / chuột", "Tay cầm / cảm ứng"], [
    ("Di chuyển", "WASD hoặc phím mũi tên", "Cần trái; kéo vùng thấp bên trái màn hình khi dùng touch"),
    ("Tấn công", "Tự động khi có mục tiêu hợp lệ", "Tự động"),
    ("Mua / dùng trạm / cửa trong shop", "Đến gần rồi Space; có nút bấm trong UI tương ứng", "A; nút tương tác cảm ứng"),
    ("Bảng chi tiết", "I", "Y"), ("Chuyển tab chi tiết", "Tab / phím điều hướng", "LB / RB"),
    ("Tạm dừng / quay lại", "Esc", "B / Start"),
    ("Cổng bí mật", "Giữ E / Space / Enter khoảng 0.85 giây khi ở vùng tương tác", "Giữ A; điều khiển xác nhận tương ứng"),
    ("Chọn phương án sự kiện", "1–3 hoặc trái/phải; xác nhận lựa chọn", "LB/RB hoặc điều hướng, rồi A"),
    ("Lời kết", "Di chuyển tới lựa chọn hoặc bấm; Space / Enter để lật trang", "Di chuyển hoặc A; hero không đi được vẫn chọn trái/phải")])
add("Deadzone tay cầm 0.18, sau đó chuẩn hóa biên độ còn lại. Vùng kéo touch ở phía trái, phần dưới 65% màn hình; bán kính điều khiển theo chiều cao, tối thiểu 42 px. UI có xử lý safe area; phần báo cáo cũ đã kiểm tra bố cục 1080p và 720p, không tương đương chứng nhận trên mọi thiết bị cảm ứng.")
add()
add("HUD hiển thị wave, thời gian/HP, vàng, trứng và bond. Tóm tắt bond góc trên phải tối đa **5 dòng**, có **View all** để xem đủ. Bảng đội hình/relic/vũ khí/đóng góp cho phép chọn để đọc chi tiết. Owned relic có icon lớn, dải rarity và khung mô tả cuộn; lựa chọn Rainbow sau Keeper có preview, mô tả cuộn và nút Claim. Shop có hóa đơn giao dịch gần nhất, thông tin ghép và giá thực tế.")
add("**Phản hồi shop:** giao dịch thành công so sánh chỉ số đã compile trước/sau, hiện số tăng/giảm trên hero và từng đồng minh chịu ảnh hưởng; gồm cả buff gián tiếp từ talent, bond, ghép sao và đổi roster. Mỗi đợt hiển thị tối đa 8 đơn vị, hai dòng mỗi đơn vị trong 1.5 giây; các dòng/đơn vị còn lại được xếp hàng. Hóa đơn giữ chi tiết trước → sau. Giao dịch thất bại không phát popup. Chỉ hiển thị ở shop khi simulation nghỉ; đóng khi vào combat và không phát lại tồn đọng trong trận.")
add("**Chọn cách chơi hero:** BRIGADE > STATS > dòng đầu. Phím 1–3 hoặc X trên tay cầm đổi approach; F / L3 thực hiện thao tác đặc biệt nếu hero hỗ trợ. Có nút chạm cho cả ba lựa chọn và thao tác riêng. Chỉ thay đổi khi nghỉ ở shop; áp dụng ở trận kế tiếp.")
add()
add("Thiết lập gồm âm lượng tổng/nhạc/SFX, giảm chuyển động, giảm flash, giảm hiệu ứng, giảm rung, chế độ dễ đọc, số sát thương và dấu loot. Mặc định profile: Volume 0.7, MusicVolume 0.8, SfxVolume 0.85. Giảm hiệu ứng vẫn giữ báo trước và thông tin nguy hiểm cần né. Có menu Help, Records, Museum và Achievement Journal; đây là các trang runtime, không phải nội dung suy đoán từ concept.")
add()

heading("03. Mode, độ khó, mở khóa và lưu tiến trình", "modes")
table(["Index", "Độ khó", "Mở khóa / ảnh hưởng"], ((i, n, "Mặc định mở" if i == 0 else "Mở sau chiến thắng độ khó ngay trước; tăng hệ số địch và điểm") for i, n in enumerate(D["Modes"]["Difficulty"])))
table(["Challenge ID", "Tên", "Seed quy định", "Điều kiện"], ((cid or "(trống)", name, seed if cid else "Người chơi / run chọn", ["Hành trình tự do", "Night shift: đến bình minh", "First light: hoàn thành hall 10", "Small brigade: thắng với tối đa 6 đồng minh"][i]) for i, (cid, name, seed) in enumerate(zip(D["Modes"]["ChallengeIds"], D["Modes"]["Challenges"], D["Modes"]["Seeds"]))))
add("Challenge có seed cố định và khóa difficulty 0. Hero mở theo BestWave so với UnlockWave trong bảng hero. Các tên tham khảo từ Bizarre Brigade được lưu dưới dạng metadata đối chiếu; nhân vật trong game có tên, art và cơ chế chuyển thể, không có nghĩa các chỉ số trùng bản tham khảo.")
add()
add("Điểm cơ sở = số hall đã clear × 1000 + kills × 10 + boss kills × 500 + 5000 nếu thắng; nhân hệ số độ khó `(100 + 20 × difficulty) / 100`, sau đó nhân ScoreMultiplier của build và chặn về giới hạn số nguyên. Endless ghi bản run mới có ParentRunId: giữ build/tài sản nhưng reset các bộ đếm kills, đóng góp và thời gian của chặng để ghi thành tích riêng. Một lần hoàn tất không được cộng profile lặp nhờ completed RunId.")
add()
add("Profile giữ mở khóa, achievement/progress, sưu tập và kỷ lục. Lịch sử tối đa 50 run; dữ liệu phân biệt hero, difficulty, normal/endless và challenge. Museum ghi unit, item, boss, vũ khí, cấp sao và chain đã gặp. Skin bối cảnh: mặc định; NightOffice khi BestWave ≥ 5; PaperStage khi đã thấy ít nhất 3 boss. Chưa có nền tảng meta-stat mua vĩnh viễn được xác nhận trong bản triển khai này.")
add()
add("Profile version 3 và snapshot schema revision 3. Run lưu danh tính, seed/RNG, đội hình, item, shop, hợp đồng, boss ẩn, cốt truyện, thành tích và các khoản tăng trưởng cần phục hồi. RunStorage kiểm tra cấu trúc/giá trị, envelope hash và đường khôi phục bản dự phòng. Có một nhánh Continue lưu cục bộ; không có dịch vụ leaderboard trực tuyến hay cloud save được nối trong nguồn hiện tại. Toàn bộ mã lưu/di trú nằm trong phụ lục để reviewer kỹ thuật kiểm tra.")
add()

heading("04. Toàn bộ chỉ số, phép tính và giới hạn", "stats")
add("Đơn vị: HP và damage là số thực; cooldown/windup/recovery/lifetime là giây; speed là đơn vị world/giây; range/radius là đơn vị world; xác suất 0.1 = 10%; phần trăm cộng modifier thường dùng số thập phân. Scope Team áp dụng hero và allies; Champion/Companions chỉ áp dụng nhóm tương ứng; Run tác động kinh tế, loot hoặc hệ thống. Luck và TreasureDetection được UI lưu như số hệ số, không tự suy ra một tỉ lệ rơi cuối cùng.")
add()
notes = {
    "Damage": "Cộng hệ số sát thương đội; phần bonus vượt 1.2 chỉ còn 30% hiệu lực.",
    "AttackSpeed": "Giảm cooldown qua hệ số; bonus vượt 0.6 chỉ còn 12%; factor 0.25–2.2 trước các nhánh khác.",
    "Health": "HP cộng trước HealthCorrection; có soft cap và trần theo wave, kèm ngoại lệ hero.",
    "MoveSpeed": "Bonus tốc độ đi; tốc độ unit sau CompileAlly tối đa 10.",
    "Pickup": "Bán kính hút/nhặt loot; không phải ChestReach.", "Roam": "Vùng tuần tra/đi tìm mục tiêu của đồng minh.",
    "Interceptor": "Khả năng ally chặn đạn theo quy tắc chặn; không phải cộng HP ally.",
    "Heal": "Hồi HP mỗi giây, chịu hệ số và ngoại lệ hero.", "Crit": "Xác suất chí mạng; basic crit thường ×1.5 damage.",
    "Projectiles": "Số đạn cộng; CompileAlly chặn 1–10, một số đòn proc có mẫu riêng.",
    "Interest": "Cộng trần tiền lãi, không đổi InterestDivisor.", "Armor": "Giáp dương chia damage cho (1+armor); giáp âm tăng damage.",
    "ChampionDamage": "Bonus sát thương riêng hero.", "CompanionDamage": "Bonus sát thương riêng ally.",
    "CompanionAttackSpeed": "Bonus tốc đánh riêng ally.", "Discount": "Giảm giá vàng cơ bản, tổng discount có giới hạn.",
    "Luck": "Tham gia trọng số shop/loot và một số cơ chế hero; không phải đảm bảo đồ hiếm.",
    "TreasureDetection": "Tác động phát hiện/kỳ loot và rương theo code.", "ScoreMultiplier": "Nhân điểm run; factor 0.1–100.",
    "CollisionDamage": "Damage khi thân va chạm; khác vũ khí Kind Contact.",
    "MeleeDamage": "Damage phẳng cộng vào vũ khí cận chiến/contact trước multiplier.",
    "RangedDamage": "Damage phẳng cộng vào vũ khí Projectile trước multiplier.",
    "AttackRange": "Bonus range, soft knee 0.5, slope 0.15, factor 0.4–1.8.",
    "Dodge": "Né đòn đủ điều kiện; cap thường 60%, Sir Purrcival 75%.", "Income": "Cộng vàng sau clear wave bình thường.",
    "BulletPierce": "Xuyên cộng, tổng compile 0–64.", "BulletSplit": "Tách cộng, compile 0–6; có chặn đệ quy.",
    "BulletBounce": "Xác suất nảy, clamp 0–1.", "BulletSize": "Kích thước hình và hit radius, factor 0.25–4.",
    "BulletSpeed": "Đổi vận tốc, lifetime bù ngược để giữ quãng đi; không tự làm DPS cao hơn.",
    "BulletDistance": "Đổi quãng đi qua lifetime.", "Knockback": "Khoảng đẩy theo factor; địch thường tối đa 0.45/lần trước kháng; boss miễn.",
    "BodySize": "Scale thân và hit radius hero; factor 0.4–2.5.", "CompanionSize": "Scale thân/hit radius allies; riêng với scale sao.",
    "EnemyCount": "Đổi mật độ/budget địch.", "EnemyHealth": "Hệ số HP địch, factor 0.2–10 ở compile cuối.",
    "EnemySpeed": "Hệ số tốc độ địch, factor 0.25–2.", "HeartHealing": "Cộng HP khi nhặt tim.",
    "ChestQuality": "Tác động chất lượng/thưởng rương.", "RecruitDiscount": "Giảm riêng giá tuyển quân.",
    "RelicDiscount": "Giảm riêng giá item.", "ShopRarity": "Đổi trọng số rarity trong shop, không mở mọi item ngoài pool.",
    "HealthCorrection": "Hệ số hiệu chỉnh HP sau phần cộng phẳng.", "DamageCorrection": "Hệ số hiệu chỉnh damage sau các phần cộng.",
    "SpeedCorrection": "Hệ số hiệu chỉnh tốc độ.", "ArmorCorrection": "Hệ số hiệu chỉnh giáp.",
    "HealCorrection": "Hệ số hiệu chỉnh hồi máu; một số hero chặn hồi.",
    "CollisionFrequency": "Tăng tần suất bằng chia interval cho factor; interval 0.15–4 giây.",
    "ChestReach": "Bán kính tương tác/mở rương, tách khỏi nhặt xu/trứng/tim.",
}
table(["Key", "Tên UI", "Đơn vị UI", "Scope mặc định", "Diễn giải"], ((s["Key"], s["Name"], "% bonus" if s["Percent"] else "Số / hệ số", SCOPE[s["DefaultScope"]], notes[s["Key"]]) for s in D["StatGlossary"]))
add("Thứ tự tổng quát: clone unit/vũ khí gốc → áp dụng sao và modifier thuộc scope → passive/hero/relic/bond → cân bằng theo wave → các hiệu ứng chuẩn bị trận → trạng thái và proc trong simulation. Vì có các nhánh chạy sau CompileAlly, số cap dưới đây phải được hiểu tại đúng bước thực thi; không dùng chúng để khẳng định mọi đòn đặc biệt đều có cùng một trần.")
add()
add("Đặt `L = clamp01((wave−8)/12)` và `soft(x,k,s) = x nếu x≤k, ngược lại k+(x−k)×s`. HP thường = `clamp(soft(rawHP,250,lerp(1,0.25,L)),1,lerp(2000,600,L))`. Card Queen dùng 12 HP; Periwinky lấy vàng và clamp 20–400. Flat damage bonus vượt 10 giảm hiệu lực từ 100% về 35% theo L.")
add()
table(["Bước CompileAlly", "Quy tắc hiện hành"], [
    ("Cooldown", "max(0.28, cooldown × lerp(1.12,1.35,L)); windup ≤ 35% cooldown; recovery ≤ 40% cooldown"),
    ("Range", "clamp(range × lerp(1,0.86,L),0.4, melee?3.4:8)"),
    ("Damage", "×lerp(1,0.83,L); melee thêm clamp(sqrt(2.2/max(2.2,range trước cap)),0.68,1)"),
    ("Projectile", "Count 1–10; lifetime ≤ 9/max(1,ProjectileSpeed); pierce 0–64; split 0–6"),
    ("Signature", "Radius ≤ 3.2; PushDistance ≤ 0.45"), ("Tốc độ", "Unit.Speed ≤ 10"),
    ("CollisionDamage", "≤ lerp(200,80,L) tại bước compile"),
    ("Armor", "Giảm hiệu lực phần vượt 0.8 theo late; trần compile 2.5"),
    ("Knockback", "Boss = 0; địch khác clamp(requested,0,0.45) × (1−clamp01(resistance))")])
add("Đồng minh không mất HP khi nhận damage. Hero không flash trắng khi trúng đòn; vẫn có phản ứng animation và âm thanh. Với đòn hại thông thường: kiểm tra nhánh hiệu ứng → dodge nếu được phép → chặn raw hit tối đa 22% max HP trước armor → giáp dương chia `(1+armor)`, giáp âm nhân `1+min(1,−armor)` → ward/hero/nhánh giảm riêng. Đây không phải invulnerability frame tổng quát. Health cost chủ động bỏ qua armor/dodge/cap; ví dụ Omnom mất 1 HP/giây và Philip mất 0.5 HP mỗi 5 giây. Ngoại lệ Card Queen và các cơ chế hồi ghi ở mục hero/code.")
add()
add("DamageElement có Physical, Ember, Frost, Arcane. Hiệu ứng cháy, chậm, đánh dấu, các phản ứng trộn và đánh trễ có bộ đếm/cause riêng để hạn chế chain; basic hit, signature, proc và secondary strike không phải lúc nào cũng kích hoạt lại cùng một trigger. Phụ lục code giữ toàn bộ `ApplyDamage`, phản ứng nguyên tố, projectile/pierce/split/bounce, collision và quy tắc proc để không làm mất các hệ số nhỏ trong mô tả tổng hợp.")
add()
table(["Nguyên tố / tương tác", "Thông số thực thi"], [
    ("Physical", "Không tự đặt burn/chill/arcane-chain từ bước ApplyWeaponDamage"),
    ("Ember", "Đặt BurnUntil = thời gian hiện tại +2 giây; tick burn và cause được xử lý riêng trong simulation"),
    ("Frost", "Chill kéo dài 1.5 giây trên địch thường, 0.6 giây trên boss"),
    ("Ember gặp Chill / Frost gặp Burn", "Xóa cả hai trạng thái và đánh thêm 50% damage của cú hit tạo phản ứng"),
    ("Arcane", "Đánh lan một địch khác còn sống cách mục tiêu <1.5 world unit, damage 35%; không gọi đệ quy chuỗi nguyên tố")])

heading("05. Vàng, loot, shop, altar và nâng sao", "economy")
scalar_catalog = [(k, v) for k, v in D["Catalog"].items() if isinstance(v, (str, int, float, bool))]
table(["Cấu hình catalog", "Giá trị gốc"], scalar_catalog)
add("Các trường catalog có nhánh legacy nên cần đối chiếu runtime: dù RerollPrice asset là 4, công thức hiện hành khởi điểm 2 vàng; TierSupportMultiplier asset là 3, support sao hiện dùng ×1/2/4; SpecialShopEvery không thay thế xác suất altar. Bảng giữ nguyên dữ liệu để reviewer thấy cả cấu hình và override.")
add()
add("Bắt đầu 35 vàng. Clear wave thông thường hồi đầy HP và trả `20 + floor(wave×1.5) + floor(IncomeBonus)`; lãi `min(15+floor(InterestBonus), floor(gold/10))` trước khi cộng thu nhập. Chiến thắng cuối chiến dịch không tự trả thêm một vòng thu nhập bình thường. Khi bước sang Endless có khoản mở chặng riêng. GoldPerKill cơ sở là 2, có thể được WaveModifier thay thế; sau đó nhân `1+min(2,max(0,wave−2)/8)` và làm tròn Unity, tối thiểu 1. Nhóm swarm nhận một payout GoldPerKill mỗi 3 kill (không có nghĩa chỉ 1 vàng). Các đồ đặc biệt, hero, loot và hợp đồng có thể thay đổi thu nhập thực.")
add()
add("Loot có Coin, Egg, Heart, Chest. Coin và Egg là tiền khác nhau; tim hồi HP theo rule hero và HeartHealing/HealCorrection; rương có nhịp spawn, khoảng mở riêng và quality. Các proc CoinCollected/ChestOpened/EnemyKilled từ cấu hình có chance, cooldown, value, radius. Bảng cấu hình Arena và ProcRules đầy đủ được nhúng ở phụ lục; giá trị checkpoint gốc có thể được PrepareCombat ghi đè.")
add()
table(["Proc rule", "Item", "Trigger", "Action", "Power", "Radius", "Cooldown s", "Chance"],
      ((r["Id"], r["ItemId"], r["Trigger"], r["Action"], r["Power"], r["Radius"], r["Cooldown"], r["Chance"]) for r in D["Catalog"]["ProcRules"]))
table(["Wave modifier (ưu tiên theo thứ tự)", "Mỗi N wave", "Spawn multiplier nguồn", "Budget multiplier nguồn", "GoldPerKill nguồn", "Healing thêm"],
      ((r["Name"], r["Every"], r["SpawnMultiplier"], r["BudgetMultiplier"], r["GoldPerKill"], r["Healing"]) for r in D["Catalog"]["WaveModifiers"]))
add("Nếu nhiều Every cùng khớp, ActiveModifier lấy mục đầu tiên trong bảng, không cộng cả ba. Spawn/Budget modifier được lerp từ 1 đến giá trị nguồn ở mức 0.5 trước khi áp; GoldPerKill đi qua scaling vàng theo wave nêu trên. Rương thường spawn cách nhau `max(4,ChestInterval/max(0.25,1+TreasureDetection))` giây, tối đa 4 rương đang chờ. Value rương = round((8+random nguyên 0–8)×clamp(1+ChestQuality,0.2,5)), tối thiểu 1. Tim rơi từ kill thường có value 8; chance trứng/tim = clamp01(chance nguồn×(1+Luck)), trừ các nhánh loot bị chặn.")
add()
add("Shop vàng có 3 ô. Nhánh tuyển quân tập trung ở ô 0 theo CompanionOfferChance; các ô còn lại lấy item trừ khi pool tương ứng rỗng. Pool lọc unlock wave, theme hero, loại shop, trọng số >0 và stack còn mua được. Giá item không phải Price trong asset: đó chỉ là đầu vào.")
add()
add("Với `s=max(0,wave−1)`, hệ số shop thường = `min(10000,(1+0.12s+0.009s²)×(1+min(0.2,max(0,wave−3)×0.01))×(1+max(0,wave−7)×0.065))`. Giá recruit cơ sở = ceil(Price × veteran × tierFactor × shopFactor), tierFactor 1/1.8/3.5. Veteran dựa cấp cao nhất đã sở hữu cùng loài: 1 nếu chưa 2 sao, 2 nếu có 2 sao, 3 nếu có 3 sao. Giá item vàng cơ sở = ceil(Price × 0.85 × (1+0.4×rarityIndex) × shopFactor). Sau đó áp chiết khấu liên quan, giới hạn discount −100% đến +50%, làm tròn lên và tối thiểu 1.")
add()
add("RerollCost = max(1, 2 + rerollsThisShop×max(1,2) − 2×RerollFeather − 2×MaryTalent + ngoại lệ hero); Nicole +2, Belle −1. Refresh thường và altar dùng chung bộ đếm, reset khi sang wave mới. Smiley Fox/Nicole và relic có thể nhận tăng trưởng khi refresh; không xem việc refresh là hoàn toàn miễn phí dù có refund.")
add()
add("Giữ tối đa **3 lượt mua companion thường mỗi round**, gồm bản trùng nâng sao; sau lượt thứ 2, nhánh companion còn 15%, sau lượt thứ 3 bằng 0. Trong trần đó, **loài mới tối đa 2 ở round 1–3, rồi 1**. Companion từ shop đặc biệt miễn cả hai quota và không bị số lượt mua thường làm giảm slot. Loài mới là ID chưa có trong roster hoặc memory hấp thụ. Round 4+: gate loài mới 85% dưới 6 cơ thể, từ 6 dùng `t=clamp01((count−6)/14)`, `lerp(0.55,0.02,1−(1−t)²)`. Round 1–3 giữ gate ít nhất 70%. Luck vẫn điều chỉnh gate; đây không phải tỉ lệ cuối mỗi ô. Không giảm slot nâng sao theo quân số. Quà companion thường từ relic dùng hai quota tương ứng; quà talent/altar có quy tắc riêng. Trần cơ thể vẫn 20, bond và công thức nâng sao giữ nguyên.")
add()
add("Mảnh ghép đang thiếu được ưu tiên; khi có hai 1 sao mà chưa có 2 sao cùng loài, ưu tiên bắt đầu 85% rồi tăng 3 điểm % cho mỗi lần khô, đến ngưỡng refresh đủ điều kiện thì bảo đảm một ứng viên mảnh ghép. Đội dưới 7 người từ wave 4 có thể gặp cấp cao; đội ≤3 từ wave 10 có nhánh roll <18 cho 3 sao, <55 cho 2 sao; các đội nhỏ khác có nhánh <30 cho 2 sao. Đây là roll trong nhóm đủ điều kiện, không phải tỉ lệ tổng mọi offer. Card King có ưu tiên loài mới 2 sao riêng.")
add()
add("Altar kích hoạt với `min(1,0.10 + Eggs×0.05)`, Periwinky luôn 100%. Có shop đặc biệt 3 ô, giá recruit trứng cơ sở 4 × veteran × tierFactor × `(1+max(0,wave−7)×0.045)`; item lấy AltarPrice × `(1+0.25×rarityIndex)` × hệ số trên. Giá dùng trứng hoặc HP theo item; mua bằng HP bắt buộc để lại ít nhất 1 HP. Item Pools=Both được ReviewBalance phân về Gold hoặc Eggs, không nhất thiết xuất hiện ở cả hai shop.")
add()
add("Ba đơn vị cùng loài/cùng sao tự ghép lên một bậc, tối đa 3 sao. Vũ khí kết quả được chọn từ **ba vũ khí nguyên liệu thật**, đồng thời không tạo trùng biến thể loài+vũ khí trong roster. Evil không ghép. Ba Little Sprout sinh một bạn 2 sao ngẫu nhiên hợp theme và một Grand Sprout 2 sao khi còn chỗ; các nhánh kiểm tra trùng vũ khí và giới hạn quân số vẫn áp dụng.")
add()
table(["Cấp", "Scale thân", "Damage sao", "Support", "Range sao", "Nhánh ranged"], [
    ("1 sao", "×1", "×1", "×1", "×1", "Gốc"),
    ("2 sao", "×1.35", "×2.7", "×2", "+9%", "+1 projectile và +1 pierce"),
    ("3 sao", "×1.8", "×7.29", "×4", "+18%", "+2 projectile và +2 pierce")])
add("Đây là hệ số bước nâng sao, không phải toàn bộ damage cuối; melee còn bonus tốc đánh 4% mỗi bậc rồi chịu cân bằng cooldown. Bản hiện tại bỏ phụ kiện đeo thêm do lên sao/hình thái; sao thể hiện bằng thân lớn hơn. Ở giới hạn 20 cơ thể vẫn có thể mua mảnh làm hoàn tất ghép ngay; không cho tích vô hạn quân vượt trần.")
add()

def unit_block(u, group, index):
    ident = u["Id"]
    track(group, ident)
    heading(f"{index:02d}. {u['DisplayName']} — `{ident}`", f"{group}-{ident.lower()}", 3)
    add(text(u.get("ReadableDescription") or u.get("Description")) + "\n")
    if u.get("ReferenceCompanion"):
        add(f"**Metadata tham khảo:** {text(u['ReferenceCompanion'])}.\n")
    if u.get("ReferenceHero"):
        add(f"**Metadata tham khảo:** {text(u['ReferenceHero'])}.\n")
    table(["HP gốc", "Speed", "Radius", "BodyHeight", "Vũ khí asset", "Crit", "Contact / interval", "Collision / frequency"], [[
        u["Health"], u["Speed"], u["Radius"], u["BodyHeight"], reference(u["Weapon"]["id"], "weapon"), u["CriticalChance"],
        f"{text(u['ContactDamage'])} / {text(u['ContactInterval'])}s", f"{text(u['CollisionDamage'])} / {text(u['CollisionFrequency'])}s"]])
    table(["Thuộc tính", "Dữ liệu"], [
        ("Tags / bond", u["Tags"]), ("Passive", f"{u['Passive']} / strength={text(u['PassiveStrength'])}: {u['PassiveDescription']}"),
        ("Modifier hero", modifiers(u["ChampionModifiers"])), ("Modifier companion", modifiers(u["CompanionModifiers"])),
        ("HeroMechanic", u["HeroMechanicDescription"] or u["HeroMechanic"]),
        ("UnlockWave", u["UnlockWave"]), ("Price gốc", u["Price"]), ("Rarity", u["Rarity"]),
        ("SpecialRecruit / GiftOnly / ExclusiveHero", f"{text(u['SpecialRecruit'])} / {text(u['GiftOnly'])} / {u['ExclusiveHero'] or '—'}"),
        ("RecruitThemes / locked", f"{text(u['RecruitThemes'])} / {text(u['RecruitThemeLocked'])}"),
        ("Material / Motif", f"{u['Material']} / {u['Motif']}"),
        ("Behavior / Shot / kháng đẩy", f"{u['Behavior']} / {u['Shot']} / {text(u['KnockbackResistance'])}"),
        ("Asset nguồn", u["AssetPath"])])
    if u.get("Dialogue"):
        add("**Thoại đã xuất:** " + " / ".join('“' + str(x) + '”' for x in u["Dialogue"]) + "\n")


heading("06. Toàn bộ 31 nhân vật điều khiển", "heroes")
add("Mỗi hero có một mechanic riêng theo ID, tài nguyên/điều kiện riêng và ba approach, tổng 31 mechanic / 93 lựa chọn. Các tên HeroMechanic trong asset bên dưới là enum nền được giữ cho innate cũ và tương thích save. Hệ mới thay phần kích hoạt chung bằng HeroIdentities; xem bảng này, mô tả hero và phụ lục HeroIdentityActions/Combat để đánh giá gameplay thực. Đòn phụ dùng ngân sách damage hữu hạn và giới hạn mục tiêu; không kích hoạt vòng proc vô hạn.")
table(["Hero", "Mechanic hiện hành", "Tài nguyên", "Ba approach"], ((reference(h["Id"], "hero"), h["Name"], h["Resource"], " / ".join(h["Approaches"])) for h in D["HeroIdentities"]))
details("Toàn bộ 31 mechanic và 93 approach đang chạy", "json", jdump(D["HeroIdentities"], True))
add("Bảng gốc và baseline đặt cạnh nhau để thấy tác động của passive và compile. Baseline dùng seed 610026, difficulty Peaceful, 35 vàng, starter mặc định, chưa mua item và chưa tích tăng trưởng; wave 8/12/20/30 được đặt trực tiếp để đo quy tắc compile. Đây là **155 tình huống tổng hợp độc lập**, không phải chơi từ đầu đến cuối. Wave 30 bật Endless. Starter vẫn giữ nguyên cấp khởi đầu trong tất cả ví dụ.")
add()
table(["Hero", "ID", "Mở ở BestWave", "HP gốc", "Speed gốc", "HeroMechanic"], ((reference(u["Id"], "hero"), u["Id"], u["UnlockWave"], u["Health"], u["Speed"], u["HeroMechanic"]) for u in D["Champions"]))
for i, u in enumerate(D["Champions"], 1):
    unit_block(u, "hero", i)
    b = baselines[(u["Id"], 1)]
    add("**Starter mặc định:** " + ("; ".join(f"{r['UnitId']} / {r['WeaponId']} / {r['Tier']} sao" for r in b["Roster"]) or "Không có; nhánh solo") + ".\n")
    table(["Wave tổng hợp", "HP", "Damage cơ bản", "Cooldown s", "Range", "Speed", "Crit", "Armor", "Dodge", "Regen HP/s", "Đạn / xuyên"],
          ((w, *[baselines[(u["Id"], w)][k] for k in ["HP", "Damage", "Cooldown", "Range", "Speed", "Crit", "Armor", "Dodge", "Regen"]], f"{baselines[(u['Id'], w)]['Projectiles']} / {baselines[(u['Id'], w)]['Pierce']}") for w in [1, 8, 12, 20, 30]))
    hero_ref = u.get("ReferenceHero")
    def ally_ok(a):
        if a["GiftOnly"]:
            return False
        if u["HeroMechanic"] == "SoloMaw" and hero_ref != "Capped Gallant":
            return False
        if a["ExclusiveHero"]:
            return a["ExclusiveHero"] == hero_ref
        return hero_ref == "Belle" or bool(set(u["RecruitThemes"] or []) & set(a["Tags"] or []))
    ordinary = [a["Id"] for a in D["Companions"] if ally_ok(a) and not a["SpecialRecruit"]]
    special = [a["Id"] for a in D["Companions"] if ally_ok(a) and a["SpecialRecruit"]]
    add("**Pool loài qua lọc theme (trước unlock wave, cap và trọng số):** thường: " + text(ordinary) + "; special: " + text(special) + ".\n")
    add("**Pool item qua lọc theme (trước loại shop, unlock, weight và stack):** " + text(poolmap[u["Id"]]["ThemeEligibleItems"]) + ".\n")

heading("07. Toàn bộ 66 loài đồng minh", "companions")
add("Mỗi mục là một loài, không phải một biến thể sao/vũ khí. Vũ khí asset là mặc định dữ liệu; khi tuyển thực tế, game roll từ các vũ khí catalog đủ điều kiện và không trùng biến thể đã có. Rule hiện tại cho phép vũ khí `!SpecialOnly` hoặc ally SpecialRecruit; không khóa vũ khí theo hình vẽ loài. Các tài năng, support và aura vẫn khác nhau theo loài. Các dòng HP chỉ là dữ liệu cấu hình; allies đang bất tử trong combat.")
add()
table(["Loài", "ID", "Mở wave", "Giá gốc", "Special", "GiftOnly", "Tags"], ((reference(u["Id"], "ally"), u["Id"], u["UnlockWave"], u["Price"], u["SpecialRecruit"], u["GiftOnly"], u["Tags"]) for u in D["Companions"]))
for i, u in enumerate(D["Companions"], 1):
    unit_block(u, "ally", i)
add("Carousel Pony chọn một đồng minh thật gần nó làm rider, cả hai tiếp tục đánh và đều bất tử; không tạo một đơn vị chỉ có hình trang trí. Grand Sprout, Little Sprout và Bristle Ball có nhánh không basic attack, dùng collision/talent riêng. Wise/Capped Gallant hấp thụ kỹ năng qua tuyển thay vì nhất thiết giữ cơ thể trong roster. Các quà Lisa/Bella/Jake, nhận tiền Roger, trứng Daisy và bonus Nia chỉ xảy ra theo trigger mua, không lặp vô hạn mỗi khung hình. Phụ lục OctoberCompanions/OctoberCombat/OctoberRun giữ từng tài năng và số cooldown/giới hạn.")
add()

heading("08. Toàn bộ vũ khí và thông số", "weapons")
add("Có 57 vũ khí trong RunCatalog và thêm các asset tấn công của địch/boss/checkpoint. Toàn bộ đều được liệt kê dưới đây; phần ngoài catalog được ghi nhãn riêng, không tính vào lựa chọn shop người chơi. Damage/cooldown là gốc, trước hero, sao, relic và cân bằng. `Damage/Cooldown` không bao quát AoE, signature và thời gian đạn quay về nên không được xuất như DPS thực.")
add()
table(["Tên", "ID", "Kind", "Damage", "Range", "Cooldown", "Signature / Family / Mechanic", "Morph"],
      ((reference(w["Id"], "weapon"), w["Id"], w["Kind"], w["Damage"], w["Range"], w["Cooldown"], f"{w['Signature']} / {w['Family']} / {w['Mechanic']}", w["Morph"]) for w in D["Weapons"]))
weapon_fields = ["Kind", "Element", "Motion", "Damage", "Range", "Cooldown", "Windup", "Recovery", "ProjectileSpeed", "ProjectileSpread", "HomingOutbound", "SpecialOnly", "ProjectileLifetime", "VisualWidth", "MountDistance", "MuzzleDistance", "ConeDot", "ProjectileCount", "ProjectilePierce", "ProjectileSplit", "ProjectileBounceChance", "ProjectileRadius", "SwingAngle", "RecoilDistance", "BodyAttackTilt", "HasMuzzleOffset", "ProjectileMuzzleOffset", "ProjectileVisualWidth", "Signature", "SignatureBeats", "SignatureRadius", "SignatureScale", "SignatureDelay", "MarkLifetime", "PushDistance", "Family", "Mechanic", "SecretRecall", "Morph", "ProjectileColor", "AssetPath"]
for group, title in [("Weapons", "Trong catalog người chơi"), ("OtherWeaponAssets", "Asset ngoài catalog người chơi")]:
    add(f"### {title}\n")
    for i, w in enumerate(D[group], 1):
        track(group, w["Id"])
        heading(f"{i:02d}. {w['DisplayName']} — `{w['Id']}`", f"weapon-{w['Id'].lower()}", 4)
        desc = text(w.get("GameplaySummary") or w.get("Description"))
        if desc != "—" and desc:
            add(desc + "\n")
        fields_table(w, weapon_fields)

heading("09. Toàn bộ 170 relic, đồ ăn và đồ uống", "relics")
add("**Nguồn số liệu hiệu lực:** EffectiveModifiers và EffectiveStackLimit, không suy từ dòng Description hoặc cặp Effect/Value legacy khi đã có Modifiers. Mỗi entry giữ cả mô tả hiện có để review copy. Giá vàng cơ sở/AltarPrice đều phải đi qua công thức shop. ShopWeight=0 hoặc loại pool đặc biệt có thể là phần thưởng riêng, không phải mua tự do. Hệ số chủng loại rarity: Common=0, Uncommon=1, Rare=2, Mythic=3, Crimson=4, Rainbow=5.")
add()
table(["Rarity", "Số item"], sorted(Counter(x["Rarity"] for x in D["Items"]).items(), key=lambda p: D["Enums"]["ItemRarity"].index(p[0])))
table(["Item", "ID", "Rarity", "Wave", "Price gốc", "Altar", "Proc"], ((reference(x["Id"], "item"), x["Id"], x["Rarity"], x["UnlockWave"], x["Price"], f"{x['AltarPrice']} {x['AltarCurrency']}", x["Mechanic"] if x["Mechanic"] != "None" else x["SignatureRole"]) for x in D["Items"]))
for i, x in enumerate(D["Items"], 1):
    track("Items", x["Id"])
    heading(f"{i:03d}. {x['Name']} — `{x['Id']}`", f"item-{x['Id'].lower()}", 3)
    add("**Hiệu ứng cộng có hiệu lực mỗi stack:** " + modifiers(x["EffectiveModifiers"]) + ".\n")
    add("**Copy hiện có trong dữ liệu:** " + text(x["Description"]) + "\n")
    fields_table(x, ["Rarity", "Category", "Price", "UnlockWave", "Pools", "ShopWeight", "AltarCurrency", "AltarPrice", "Stacking", "MaximumStacks", "EffectiveStackLimit", "Mechanic", "SignatureRole", "ShopThemes", "ReferenceName", "Effect", "Value"])
    if x["Mechanic"] != "None" or x["SignatureRole"] != "None":
        add("Proc trên item này được thực thi riêng ngoài modifier cộng. Mô tả chi tiết hiện hành trong UI:\n\n" + text(x["ReadableEffects"]) + "\n")
add("Nhóm RelicMechanic gồm cơ chế phản đòn, coin/chest healing, compound interest, thuê buff, reroll, tăng trưởng đồ ăn/đồ uống, đổi stat, solo, collision-only, follower miễn phí/đứng yên, mua quà và bộ sáu vũ khí. Toàn bộ `RunRelics.cs` và `RelicCombat.cs` được nhúng cuối file để tra threshold, trigger, cooldown và giới hạn. Description hiện dùng EffectiveModifiers: Key Oil ghi +24% mỗi bản. Tất cả 170 item có nhãn DRINK / FOOD / CURIO; drink và food tính một lần ngay khi mua, không có nút dùng riêng. Preview mua đồ còn liệt kê phản ứng từ talent/hero/relic đang sở hữu.")
add()
add("### Các proc và chuyển đổi stat quan trọng — bổ sung ngoài modifier tĩnh\n")
add("Trong bảng này n là tổng EffectiveStacks của cùng RelicMechanic đang hoạt động, không tính item bị suspend. Các stat được GainRelic có thể được ghi lại trong run; số combat vẫn chịu thứ tự compile/ngoại lệ hero. Damage secondary có cause riêng, không giả định tạo lại toàn bộ chuỗi trigger.\n")
table(["RelicMechanic", "Công thức / thời điểm thực thi"], [
    ("HoardPower", "+1% damage mỗi 5 vàng ×n; trong trận cập nhật tương đối theo ví gốc + vàng đã nhặt"),
    ("LuckToHealth", "+max(0,Luck)×100×n max HP"),
    ("MeleeToCollision", "+50% bonus flat melee dương ×n vào collision của hero"),
    ("SlowToArmor", "+max(0,−MoveSpeedBonus)×n armor"),
    ("DamageToIncome", "+floor(max(0,DamageBonusHero)/0.04)×n income"),
    ("WaveGrowth", "Mỗi đầu wave: +4n flat melee, +4n flat ranged, +10%n team damage"),
    ("CompoundInterest", "Mỗi đầu wave: vàng ×1.2^n, chặn bộ đếm; không thay lãi clear wave"),
    ("ChaosIncome", "Mỗi đầu wave +random nguyên 25–75 ×n vàng, seed theo seed run và wave"),
    ("DrinkVitality / DrinkTempo", "Mỗi drink mua: +4n HP / +6%n attack speed; DrinkTempo còn tăng trọng số drink"),
    ("FoodGrowth", "Mỗi food mua: +10n HP, +0.5n HP/s, +5%n body size"),
    ("CrowdedTape / ExposedRecovery / TreasureLease", "Lease ở wave kế: +10%n EnemyCount / −0.8n armor / +n TreasureDetection"),
    ("TurnDiscount", "Trong shop hiện tại: item vàng chưa mua còn 70% giá; đặt bonus discount 30% của kỳ"),
    ("EggGift", "+3 trứng ngay khi mua item có mechanic này"),
    ("CompanionGift", "Thử tuyển tối đa 2 bạn thường hợp theme, vẫn kiểm tra quota 3 recruit/wave và cap 20"),
    ("CloseFollowers", "Leash và roam outer radius ×0.65 khi có ít nhất một stack"),
    ("FreeFollowers", "Roam outer bằng nửa độ dài đường chéo WorldSize"),
    ("StaticFollowers", "Allies speed=0, range/lifetime ×2, pierce +3; basic/source khác vẫn theo nhánh riêng"),
    ("HalfHealthSeed", "Bắt đầu combat: HP hiện tại tối đa còn 50% max HP"),
    ("ChestOnlyRecovery", "Tắt passive heal, HeartHealing −100000; mở rương hồi 30n HP theo rule Heal"),
    ("CoinMending", "Nhặt coin: 15% hồi 2n HP"),
    ("CoinLightning", "Nhặt coin: đánh một mục tiêu ngẫu nhiên, damage max(4,Luck×50)×n"),
    ("ChestLuck", "Mở rương: +0.06n Luck"),
    ("ChestLightning", "Mở rương: đánh 1–3 mục tiêu khác nhau, damage max(8,TreasureDetection×100)×n"),
    ("RetortVolley", "Hero bị hit: 30% bắn 8 đạn tròn, damage 12n, speed 5, life 2s"),
    ("PanicMagnet", "Hero bị hit: 5% cho toàn bộ pickup hiện có bắt đầu hút"),
    ("FragilePower", "Mỗi hit lên hero: tích −5%n team damage; áp cả bộ nhớ run và tổn hao trong trận"),
    ("HurtGrowth", "Mỗi hit chọn một: +4 HP / +0.08 heal / +0.015 armor / +0.02 damage / +0.02 Luck / +0.01 crit, nhân n"),
    ("FreshLotus", "Đầu trận +3n pierce, +30%n crit; bỏ phần này khi hero nhận hit đầu tiên"),
    ("StillRecovery", "Trong khi input đứng yên: hồi 3n HP/s"),
    ("StillPower", "+16n flat damage trong combat cho đến lần di chuyển đầu tiên; không tự hồi buff chỉ vì dừng lại"),
    ("IncomePoison", "Basic hit hero đặt poison 4s, tick mỗi 1s damage max(1,RelicIncome); RelicIncome lấy catalog income + bonus"),
    ("BleedingAlarm", "Mỗi 1s chịu n damage được đánh dấu injected; không đồng nhất với mọi healthCost"),
    ("GambleRecovery", "Mỗi 3s có hai roll độc lập: 50% hồi 20n, 50% nhận 8n damage; có thể cả hai hoặc không cái nào"),
    ("SorrowPulse / VoidPulse", "Mỗi 3s đánh mọi địch hợp lệ: 20% max HP hero ×n Sorrow +15% HP hiện tại địch ×n Void"),
    ("PrismEcho", "Basic Fire hero depth=0: strike một địch, damage max(2,RelicRanged×2)×n, cooldown 0.15s"),
    ("SoloOverdrive", "Lease wave kế: chặn attack allies, hero cooldown chia 6 (reset min 0.1s, attack loop còn có cap riêng); mỗi 10 kills hồi 3 HP"),
    ("CollisionOnly", "Chặn basic hero; thêm 10×armor dương ×n collision; các nguồn proc riêng vẫn kiểm tra rule riêng"),
    ("StatFloor", "Khi mua, chụp sàn Bonus theo từng effect/scope; các lần sau không xuống dưới sàn; giữ floor cho RelicAttackDamage"),
    ("SixWeapons", "Khóa 6 weapon ID khi mua đầu tiên; 6 phantom dùng stats compile tương ứng, tắt basic thường của phe người chơi; cooldown phantom tối thiểu 0.2s")])

heading("10. Toàn bộ 29 bond", "bonds")
add("Bond hiện đếm **từng entry đồng minh trong roster** theo tag, không cộng tag hero trực tiếp. Nhiều đồng minh cùng loài nhưng khác vũ khí đều đóng góp count; lên sao gộp ba cơ thể thành một nên có thể làm giảm count. Walter thêm một count cho mọi bond nếu có ít nhất một tài năng này; bản Walter thứ hai không cộng thêm count hỗ trợ. Không có blacklist bond riêng cho hero, nhưng theme tuyển quân gián tiếp giới hạn khả năng tạo bond.")
add()
add("Công thức: `level=floor(max(0,count)/max(1,Threshold))`; `Reward=level×Value + tổng ExtraBonuses.Value có threshold≤count`. Nghĩa là Value cơ sở lặp lại ở mỗi bội Threshold, **không chỉ nhận một lần tại mốc đầu**. Bảng dưới thể hiện mọi mốc thay đổi có thể đạt tới count 21 (20 cơ thể + Walter), kèm bonus bổ sung. Raw Value theo đơn vị của effect trong glossary; không phải mọi raw value đều là phần trăm.")
add()
for i, b in enumerate(D["Bonds"], 1):
    track("Bonds", b["Id"])
    heading(f"{i:02d}. {b['Name']} — `{b['Id']}`", f"bond-{b['Id'].lower()}", 3)
    milestones = sorted(set(range(b["Threshold"], 17, b["Threshold"])) | {x["Threshold"] for x in b["ExtraBonuses"] if x["Threshold"] <= 16})
    rows = []
    for count in milestones:
        extra = sum(x["Value"] for x in b["ExtraBonuses"] if count >= x["Threshold"])
        level = count // b["Threshold"]
        raw_reward = level * b["Value"] + extra
        reward = raw_reward * D["RuntimeConstants"]["BondDefinition"]["PositiveRewardScale"] if raw_reward > 0 and b["Effect"] not in ("EnemyCount", "EnemyHealth", "EnemySpeed") else raw_reward
        rows.append((count, level, b["Effect"], b["Value"], extra, reward))
    table(["Count cần", "Level", "Effect", "Value mỗi level", "Extra đã đạt", "Reward raw tổng"], rows)
    add("**Đồng minh đóng góp tag:** " + "; ".join(f"{u['DisplayName']} (`{u['Id']}`)" for u in D["Companions"] if b["Id"] in (u["Tags"] or [])) + ".\n")

heading("11. Toàn bộ 25 công thức altar", "recipes")
add("25 công thức là số thực trong catalog: 4 cũ + 21 mới, không phải 29. Cần đủ các loài nguyên liệu, mỗi entry tối thiểu MinimumMaterialTier và đủ trứng; tiêu thụ nguyên liệu để nhận ResultTier. Vũ khí kết quả kế thừa một vũ khí nguyên liệu, đồng thời giữ quy tắc không trùng biến thể; công thức không roll một vũ khí ngoài nguyên liệu. Tooltip/validation của altar vẫn quyết định điều kiện dùng tại thời điểm đó.")
add()
table(["Recipe ID", "Nguyên liệu", "Trứng", "Sao tối thiểu nguyên liệu", "Kết quả", "Sao kết quả"],
      ((r["Id"], " + ".join(r[k] for k in ["FirstUnit", "SecondUnit", "ThirdUnit"] if r[k]), r["EggCost"], r["MinimumMaterialTier"], r["ResultUnit"], r["ResultTier"]) for r in D["Recipes"]))
for r in D["Recipes"]:
    track("Recipes", r["Id"])

heading("12. Build, hình thái và chuỗi liên kết", "builds")
add("Mỗi vũ khí thuộc một trong 12 hướng hình thái nếu đủ điều kiện. Craft = số relic hợp affinity khác ID + số vũ khí hỗ trợ khác ID do thành viên cố định khác cầm (tối đa 2) + (sao−1). Cần ít nhất một relic tương thích; mở cấp hình thái ở 2 / 4 / 6 craft. Nhiều bản sao cùng relic vẫn cộng stat nhưng không cộng craft; item đang bị mượn/ký gửi hoặc vô hiệu hóa không được tính.")
add()
add("Mỗi đòn basic thứ ba có thể phát một chuỗi hình thái, tối đa một lần mỗi 2.4 giây. Basic hit trong lần đó giữ 72% damage; phần choreography dùng ngân sách damage hữu hạn `(0.6+0.2×stage)×rawAttackDamage`, chia cho các segment/mục tiêu; giới hạn mục tiêu tăng 3/4/5 theo stage. Không có nút đánh thêm, cũng không có nhân damage vô hạn theo số đường vẽ. Nhân vật vẫn dùng hình thể gốc, thân to theo sao; chuỗi đòn và projectile/VFX thay đổi rõ theo nhánh.")
add()
for i, m in enumerate(D["Morphs"], 1):
    track("Morphs", m["Kind"])
    heading(f"{i:02d}. {m['Name']} — {m['Kind']}", "morph-" + m["Kind"].lower(), 3)
    add(text(m["Pattern"]) + "\n")
    add("**Affinity:** " + text(m["Affinities"]) + ". **Vũ khí thuộc nhánh:** " + "; ".join(w["DisplayName"] + " (`" + w["Id"] + "`)" for w in D["Weapons"] if w["Morph"] == m["Kind"]) + ".\n")
    add("**Trường Silhouette còn lưu trong dữ liệu:** " + m["Silhouette"] + ". Đây là mô tả ý tưởng phụ kiện cũ; phần phụ kiện gắn lên thân hiện đã bị bỏ. Không dùng dòng này làm bằng chứng phụ kiện còn xuất hiện trong build hiện tại.\n")
table(["Build link", "ID", "Item IDs", "Quy tắc"], ((b["Name"], b["Id"], b["ItemIds"], b["Description"]) for b in D["BuildLinks"]))
for b in D["BuildLinks"]:
    track("BuildLinks", b["Id"])
add("Các BuildLinks trên là nhóm kết nối để đọc build; khi mô tả nói No hidden set bonus thì không có một buff set bí mật ngoài hiệu ứng từng item. Signature/Mechanic vũ khí vẫn hoạt động riêng; morph không thay thế tất cả cơ chế gốc.")
add()

heading("13. Toàn bộ địch, tiến hóa, affix và nhịp wave", "enemies")
add("Pool hiện có 28 enemy: 20 loài nền có evolution và 8 specialist khói/lửa. Các giá trị sau là nguồn; HP, damage, tốc độ, số lượng còn được nhân theo wave, difficulty, Carnival và Endless. Mỗi enemy có Behavior, HostileShot, material và mutation riêng.")
add()
for i, u in enumerate(D["Enemies"], 1):
    unit_block(u, "enemy", i)
    e = evolutions.get(u["Id"])
    if e:
        add(f"**Tiến hóa từ wave {e['MutationWave']}: {e['MutationName']} / `{e['Ability']}`.** {e['Summary']}\n")
        add("Mutation này đổi moveset/motion, thân ×1.12 và radius ×1.08; bản thân bước Apply evolution không tự cộng HP. Các hệ scaling khác vẫn chạy.\n")
    elif u["SpecialEnemy"] != "None":
        add(f"**Specialist:** `{u['SpecialEnemy']}`; bản tiến hóa từ wave {max(12, u['UnlockWave'] + 5)}. Các nhánh summon/affix có giới hạn riêng.\n")
add("### Công thức địch theo wave\n")
add("Đặt w=wave, d=difficulty. Scaling nền = `(1+0.1×(w−1))×(1+0.18d)×(1+0.07d²)`. HP từ source × scaling; boss thêm BossHealthMultiplier asset 0.55 × BossBulk(w) × 1.6; địch không boss thuộc nhánh behavior cao có bước ×0.9. Tất cả thêm CarnivalEnemyHealth ×1.1 × EndlessHealth, rồi late HP factor `(1+max(0,w−10)×0.055)` và EnemyHealth modifier. Specialist thêm HP ×1.6, thân ×1.2, radius ×1.15.")
add()
add("CarnivalEnemyHealth = `1.2×(1+(max(0,w−1)/12)^1.45)×(1+1.4×(max(0,w−4)/16)^1.4)`. BossBulk tăng tuyến tính 4.6→7.5 từ wave 4→20. CarnivalEnemySpeed = `1.08×(1+1.8×(1−exp(−max(0,w−1)×0.05)))`; boss thêm ×1.08. CarnivalEnemyDamage = `1.15×(1+min(3,max(0,w−3)×0.09))×(1+min(1.4,max(0,w−5)×0.07))`.")
add()
add("Nguồn contact/weapon damage cộng `w×0.35` rồi đi qua giới hạn theo loại boss/thường và late; nhân pressure `(0.62+0.095d)×CarnivalEnemyDamage×1.08×EndlessDamage`; boss thêm ×1.18. Có cap tốc độ đạn địch 2.4 tại nhánh compile chuẩn. Keeper/Last Customer/sự kiện có override riêng; không lấy một con số nguồn làm HP thực của mọi lần gặp.")
add()
add("Endless dùng HP thêm `1.16^min(200,max(0,w−19))`, damage thêm `1.075^min(200,max(0,w−19))`. Nhánh này chỉ chạy khi Endless=true, nên wave 20 normal không tự nhận hệ số Endless. Sau 200 bước số mũ hai hệ số này ngừng tăng, scaling nền vẫn tiếp tục.")
add()
add("### Affix, summon và áp lực phụ\n")
table(["Affix", "Tỉ trọng trong lần affix thành công", "Nhánh"], [
    ("BlackSmoke", "35%", "Khói / đường che và strike"), ("BlueFlame", "25%", "Lửa xanh / volley"),
    ("RedFlame", "25%", "Lửa đỏ / áp sát"), ("Blackfire", "10%", "Biến thể hỏa lực đen"), ("TwinFlame", "5%", "Hai nhịp lửa")])
add("Từ wave 5, chance affix `min(24,8+wave−5)%`, tối đa 4 affixed enemies đồng thời. **Mã hiện hành có buff stat affix**: HP ×2.4, body ×1.3, radius ×1.25, speed ×1.08, contact/weapon damage ×1.45. Đây là điểm lệch với tài liệu K25 cũ từng nói không có hidden stat boost. Nội dung moveset và thời gian của từng biến thể nằm trong EnemyVariantCombat và EnemyInteractionRules ở phụ lục.")
add()
add("Specialist tối đa 3; concurrent windup đặc biệt tối đa 4. Summon tối đa 12, sống 12 giây, HP ×0.35, entrance 0.55 giây, không summon đệ quy/không affix/không rơi loot thường; vẫn có nhánh on-kill riêng. Ash remnants tối đa 24, sống 7 giây. Swarm tăng áp lực ×1→3 từ wave 3→12, batch crest tối đa 36, chu kỳ 12 giây có nhịp nghỉ. Mỗi 3 swarm kills trả một payout bằng GoldPerKill của trận, đã qua scaling vàng theo wave.")
add()
add("### Sự kiện giữa wave và đồ chơi đấu trường\n")
add("Wave lẻ không boss từ wave 5 có thể có spectacle, khởi động khoảng 40% thời gian wave, tổng cửa sổ 10 giây và báo trước 2 giây: PaperParade (tối đa 12 xu 1 vàng, hazard di động damage 5 với giới hạn HP theo rule); GildedTrap (rương giá trị 10, vùng nguy hiểm ngắt nhịp damage 4); WanderingEclipse (tối đa 8 thưởng xu 2, damage 2). Các trường này vẫn có kiểm tra mục tiêu, cooldown và trần sát thương, không phải ép người chơi làm thử thách parkour.")
add()
add("Đồ chơi ArenaToyKind: StageBell, ToyCart, InkBottle, HangingHammer, SleepMirror, FloorMusicBox. Trạng thái Charging→Warning→Rolling→Cooldown, báo trước cơ sở 1.2 giây, cooldown 5 giây, roll 0.8 giây. Chúng tác động combat bằng vùng/va chạm và phản ứng lên hero/địch theo simulation. Props nền có neo chân và collision theo loại, không phải toàn bộ vật vẽ đều là vật tương tác.")
add()

heading("14. Toàn bộ boss và bộ phận", "bosses")
add("Boss thường vào wave chia hết cho 4; catalog có 10 loài, nhóm/rotation chọn theo seed và stage; wave 20 normal được thay bằng The Last Customer. Boss đã xuất hiện chưa chết thì có overtime, không kết thúc cứng ở giây 45. Thân boss thường hiện ×0.8 so với scale trước lượt cân bằng, các bộ phận có bù kích thước; boss miễn knockback kể cả kéo/đẩy của nhánh mới.")
add()
signature_names = [
    ("RoyalStampede", "DeadLetterCross"), ("QuillGuillotine", "SpilledArchive"), ("PillowDrop", "NightmareHop"),
    ("AnchorRake", "MeltingBroadside"), ("ShutterCage", "FuelRain"), ("CurtainCall", "StageDive"),
    ("CradleCrash", "ShadowLullaby"), ("VaultScissor", "BankRun"), ("BassProcession", "FallingRequiem"),
    ("LostLuggageDrop", "LeviathanLanding")]
for i, u in enumerate(D["Bosses"] + [units["OutsideStageKeeper"], units["TheLastCustomer"]], 1):
    unit_block(u, "boss", i)
    p = u["BossPattern"]
    if u["Id"] in {x["Id"] for x in D["Bosses"]}:
        add(f"**Signature gốc theo pattern {p}:** " + " / ".join(signature_names[p % 10]) + ". Một signature sau mỗi 4 volley boss; windup cơ sở 1.15 giây, hai biến thể luân phiên. Nhánh leap có travel 1.1 giây và tránh contact giữa không trung.\n")
    if u["Id"] in parts:
        table(["Bộ phận", "ID", "Role", "Durability / HP boss", "Offset", "Height", "Khi phá"],
              ((a["Name"], a["Id"], a["Role"], a["Durability"], a["Offset"], a["Height"], a["BreakSummary"]) for a in parts[u["Id"]]))
add("Damage lên bộ phận chọn theo vị trí hit: trục dọc nổi trội đi vào core, còn lại chọn trái/phải. Durability là tỷ lệ HP boss cho bộ phận, không phải một boss HP thứ hai độc lập cộng vào bảng. Phá Lash/Volley/Props/Pool/Pulse/Vacuum/Locomotion/Tempo/Rupture làm yếu đúng nhánh tương ứng; không xóa mọi đòn của boss cùng lúc.")
add()
add("Cả 12 boss thường/đặc biệt thêm bộ encore sáu hình đạn: spiral/weave 72, fan 24, vòng có khe 36, cross 28, hook 24, rain 11. Đây là tổng viên trong mẫu phát, có chia đợt và cap lượng đạn, không phải mọi mẫu bắn tất cả trong một frame. Windup 0.9 giây; cooldown cơ sở Keeper 4.2 giây, boss khác 5.2 giây; **mọi boss trong nhánh này** thêm 1 giây khi còn trên nửa máu. Đạn thường speed 1.65–2.2, life 7 giây, damage ×0.7 của nguồn mẫu. Boss thường lao ở mỗi encore thứ ba: báo trước 1.1 giây, travel 0.55 giây. Mẫu này có biến thể theo boss nhưng vẫn là bộ mẫu dùng chung; Keeper và Customer giữ các đòn chữ ký riêng.")
add()
add("**Outside Stage Keeper:** sau các lần transfer bằng Hand / CarpetTongue / KeyholeEye ở các mốc sau wave 4/8/12 (đi vào wave 5/9/13), nhánh boss ẩn có thể mở sau đủ 3 transfer. Chọn chiến đấu là rủi ro thật: thua kết thúc run, không có hard timer. HP nguồn ×20 trước các hệ số difficulty/wave/Endless của nhánh; thêm `(1+0.25d+0.08×max(0,w−13))`; damage vũ khí ×2.8, contact ×2.5; thân ×1.75. Điều kiện/ràng buộc chính xác nằm trong OutsideStageRun. Thắng được chọn một trong ba offer Rainbow từ pool 4 relic và 2 allies; tránh thưởng trùng đã sở hữu, bảo đảm có nhánh ally khi hero không solo. Có preview và mô tả trước Claim.")
add()
add("**The Last Customer:** boss cốt truyện wave 20, không cần thắng Keeper hay thấy hết event. Arena riêng 28×20, EnemyBudget=0, WaveDuration=0; HP có override source ×(1+0.2d) trong nhánh finale. Có sáu đòn chữ ký của LastCustomerCombat và encore mới. Đánh bại hắn đưa game tới Dawn/lựa chọn goodbye; Endless không bỏ qua lời kết đang chờ.")
add()
table(["Boss đặc biệt", "HP source", "HP nhánh cuối (trước damage trong trận)", "Ví dụ Peaceful"], [
    ("Outside Stage Keeper", units["OutsideStageKeeper"]["Health"], "source×20×(1+0.25d+max(0,w−13)×0.08)×EndlessHealth", "Wave 13 normal: 320000 HP; phép gán này ghi đè HP BuildEnemy"),
    ("The Last Customer", units["TheLastCustomer"]["Health"], "source×(1+0.2d)", "Wave 20 normal: 180000 HP; phép gán finale ghi đè HP BuildEnemy")])

heading("15. Cổng bí mật, hợp đồng và phần thưởng", "secrets")
add("Hệ thống gồm 8 phòng, mỗi phòng 3 lựa chọn. Cửa discovery có cơ hội bố trí ở các kỳ shop sau wave 3/7/11/15/19 theo seed riêng; hai cổng ứng viên, tối đa hai lượt thăm thông thường mỗi run, một lượt mỗi kỳ nghỉ, không lặp phòng đã thăm. Vào rồi rời mà không nhận deal vẫn tính lượt. Dead Letter có thể xếp Empty Theatre chưa thăm vào lượt thêm về sau. Các phòng tập trung giao dịch, lựa chọn và combat, không có thử thách di chuyển riêng.")
add()
add("Wave và stock shop cũ được giữ khi trở về. Trận event không trả thu nhập/loot thường; HP đã chi vẫn bị trừ, chết kết thúc run. Một hợp đồng đang hoạt động tại một thời điểm; đếm tiến độ bằng chiến thắng wave thường, không farm bằng event. Chặng cuối vẫn hoàn tất hợp đồng theo rule mà không tạo một khoản thu nhập của wave không tồn tại. Journal lưu tối đa 24 dòng; đã chọn rồi không lấy phần thưởng khác của cùng lượt.")
add()
for i, r in enumerate(D["SecretRooms"], 1):
    track("SecretRooms", r["Kind"])
    heading(f"{i:02d}. {r['Title']} — `{r['Kind']}`", "secret-" + r["Kind"].lower(), 3)
    add("**Dấu gợi ý:** " + r["Hint"] + "\n")
    add("**Lời dẫn sớm:** “" + r["EarlyIntroduction"] + "”\n\n**Lời dẫn sâu:** “" + r["LateIntroduction"] + "”\n")
    for ci, choice in enumerate(r["Choices"], 1):
        add(f"#### Lựa chọn {ci}: {choice['Title']}\n")
        add(text(choice["Copy"]) + "\n")
        add(f"Reward ID: `{choice['RewardId']}`.\n")
    add("Các lựa chọn trên là copy preview tại wave 11, 200 vàng, 20 trứng và một item hiếm ví dụ (`guardian-canopy`); khả năng chọn thực tế phụ thuộc tiền, HP, roster, item được chọn và hợp đồng hiện hành. Không lấy Enabled của tình huống preview làm lời bảo đảm mọi lần ghé đều mua được.\n")
table(["Phòng / cơ chế", "Chi tiết hiệu lực ngoài copy tổng quát"], [
    ("Tailor: Recall", "+15% ranged damage; đạn quay về; không pierce theo nhánh may"),
    ("Tailor: Aftershock", "Echo 30% damage; melee cooldown +20%"),
    ("Tailor: Shelter", "+18 max HP; incoming ×0.85; team damage −8%"),
    ("Understudy duel", "Khóa bản sao basic attack của hero ×0.65, không sao chép mọi passive/proc"),
    ("Borrowed Shadow", "Cho mượn item 2 chiến thắng normal; nhận ghost; solo nhận +20% hero damage theo nhánh"),
    ("Rời gương bình yên", "Heal 20% theo nhánh"),
    ("Train ticket", "Giá 25+3×min(wave,30) vàng; trade item lấy thread hoặc đấu collector lấy crown"),
    ("Borrowed hour", "Hợp đồng 3 chiến thắng"),
    ("Income debt", "Vay 30+3×min(wave,30) vàng; trả 25% (LastIncome+LastInterest) trong 2 chiến thắng; không đánh vào loot kill"),
    ("Deposit / Insured Parcel", "Gửi item 2 chiến thắng; tăng 25% phần modifier dương, không nhân mọi proc"),
    ("Sleeping Garden", "Hồi đầy + brooch / tất toán hợp đồng / DreamLamb 2 chiến thắng; solo nhánh +20% hero damage"),
    ("Empty Theatre", "Trận giấy 28 giây không loot thường; leading actor / duet có điều kiện riêng"),
    ("Dead Letter trade", "+20% cho đồng minh được chọn bằng cặp UnitId+WeaponId; không tự theo một danh tính merge khác"),
    ("Send letter", "Nhận key, xếp Theatre về sau; nếu đã rescue có +15 vàng một lần")])
add("Actor event dùng HP `(650+140×wave)×(1+0.2d)×(leading?1.3:1)`; Understudy body 1.5, actor khác 2.3, radius 0.65, speed 1.15 hoặc 0.6, contact `5+0.12×wave`, interval 1.3; miễn knockback trong nhánh event. Dueling partner HP ×0.5, cooldown 2.8, 2 đạn, damage 7. Theatre swarm budget 32, spawn interval 0.7, time 28, HP ×0.65. Các số đã qua scene-specific override không được trộn vào scaling địch thường.")
add()

heading("16. Cốt truyện và toàn bộ kết thúc — SPOILER", "story")
add("### Tiền đề và ý nghĩa\n")
add("Cửa tiệm không muốn đóng cửa vì sợ bị quên. Đồ chơi, người chủ tiệm và đoàn bạn nhỏ nằm trong một giấc mơ đang tìm cách giữ lại sự ấm áp của mình. Càng tiến xa, môi trường càng biểu lộ một cơ thể: vân gỗ như nhịp thở, mặt kính như mí mắt, thảm như lưỡi, cánh tay nâng lối đi. Keeper thuộc về nơi đang mơ này; The Last Customer là kẻ muốn sở hữu toàn bộ giấc mơ bằng hợp đồng và hóa đơn.")
add()
add("Chủ đề chính là khác biệt giữa giữ gìn và chiếm hữu, giữa thuộc về nhau và bị mua, giữa tạm biệt và quên lãng. Các căn phòng ẩn thêm bằng chứng cho việc bên ngoài vẫn tồn tại: một người bạn được cứu, một lá thư gửi đi, một lời hứa trả xong. Chúng thay đổi ký ức/lời nói và lời kết, không phải điều kiện bắt buộc để người chơi được kết thúc chiến dịch.")
add()
table(["Depth", "Wave", "Diễn biến hiện hành"], [
    (0, "1–4", "Tiệm ấm, trà và chuông; ghế trống và hóa đơn chưa có giá là dấu gợi đầu"),
    (1, "5–8", "Đường ngoài im, bóng phản chiếu sai; đồ vật bắt đầu nhớ người chơi"),
    (2, "9–12", "Kệ thở, giấy nhớ tên, kính lộ mắt; từ wave 11 có art sàn/mắt ác mộng"),
    (3, "13–16", "Tiệm nhận ra cơ thể của mình; tay/lưỡi/mắt và ký ức sở hữu lộ rõ"),
    (4, "17–20", "Ghế cuối đã có người ngồi; Customer định giá mọi thứ; hướng tới bình minh")])
add("Depth = clamp((wave−1)/4,0,4) theo chia nguyên; tăng theo wave, không theo bộ đếm thời gian chơi riêng và không có một thanh Sanity tiêu hao. Wave 19 có khoảng lặng mở đầu shop khoảng 4 giây trong presentation, vẫn cho điều khiển. Vật mốc từ sự kiện giữ hình dạng quen thuộc giữa biến dạng; props không bị làm trong suốt hàng loạt. Nhạc đổi lớp theo depth nhưng không biến thành cơ chế đánh đúng beat.")
add()
add("### Cao trào và lựa chọn\n")
add("Sau Last Customer, hợp đồng vỡ; dưới mặt nạ là một người giữ hóa đơn không có tên. Đoàn bạn không phải thứ hắn có quyền mua. Chủ tiệm nhận ra rằng mở cửa mãi không phải cách duy nhất để được nhớ. Người chơi chọn **Close the shop** hoặc **Leave a light**. Cả hai là kết thúc bình minh thực, không chia thành good/bad dựa việc ép hoàn thành bí mật. Leave a light thêm lời hẹn/afterword; không đồng nghĩa có Chapter 2 chơi được trong bản hiện tại.")
add()
add("### Toàn văn các trang kết thúc đã xuất từ runtime\n")
for e in D["Endings"]:
    label = e["Choice"] + (" — đã gửi thư, rescue và hoàn tất Keeper" if e["OptionalMemories"] else " — không các ký ức tùy chọn trên")
    add("#### " + label + "\n")
    for i, page in enumerate(e["Pages"], 1):
        add(f"**Trang {i}**\n")
        add("\n".join("> " + s for s in page.splitlines()) + "\n")
add("Bốn bản trên ghép hai lựa chọn farewell với hai bộ ký ức đại diện. Các điều kiện LetterSent, RescuedToy và KeeperCompleted là độc lập; trong run thật chúng có thể trộn từng câu. Nếu không có companion, câu mở cửa đổi sang nhân vật tự đẩy cửa. Hàm Ending đầy đủ ở phụ lục giữ mọi nhánh ghép để reviewer không hiểu rằng chỉ tồn tại bốn trạng thái cứng.")
add()
table(["Phòng đã nhớ", "Lựa chọn index", "Câu ký ức"], ((m["Kind"], m["Choice"], m["Text"]) for m in D["EventMemories"]))
add("Toàn bộ bảng thoại từng depth, Dream shop, Whisper và Evidence nằm ở phụ lục dữ liệu, cùng thoại hero/ally. Lời thoại gốc chủ yếu tiếng Anh; lời kể tiếng Việt trong hồ sơ là phần giải thích cho reviewer, không phải xác nhận game đã có bản địa hóa tiếng Việt.")
add()

heading("17. Toàn bộ 104 achievement nhật ký", "achievements")
add("Achievement theo dõi metric và goal, có tiến độ, lưu profile và bộ lọc chưa hoàn thành. Các nhóm gồm thắng theo hero/difficulty, điểm, gold, stat, kills, quân số, sao, bond, Keeper, secret và farewell. Bảng là định nghĩa thực tế từ RunAchievements.Definitions; không cộng lẫn kỷ lục lịch sử thành achievement mới. Điều kiện đánh dấu metric được nhúng nguyên văn trong RunAchievements ở phụ lục.")
add()
table(["ID", "Tên", "Điều kiện mô tả", "Metric", "Goal"], ((a["Id"], a["Name"], a["Description"], a["Metric"], a["Goal"]) for a in D["Achievements"]))
for a in D["Achievements"]:
    track("Achievements", a["Id"])

heading("18. Mỹ thuật, animation và khả năng đọc trận", "presentation")
add("Định hướng hiện tại là đồ chơi/cửa tiệm đêm có chất liệu Paper, Metal, Wax, Fabric, Glass, Wood; đi dần từ ấm áp sang cơ thể ác mộng. Hero/ally/enemy có pose, weapon motion, phản ứng và body puppet theo nguồn. 21 hero mới có pose idle/walk/windup/release và trạng thái biểu cảm riêng; 19 companion bổ sung có pose di chuyển/tấn công. Điều này không đồng nghĩa toàn bộ 10 hero cũ đã được vẽ lại.")
add()
add("Animation dùng authored frames cộng diễn xuất bằng code, pose windup/release, recoil/weapon tilt, idle/walk và thay biểu cảm. Foot grounding lấy vùng mực sprite để neo chân và đặt bóng tiếp xúc; hàng hóa bám mặt đỡ, vật sàn bỏ bob/rock toàn thân. Merchant vẫn có diễn xuất nhưng chân bám nền. Door có 4 pose, mở theo khoảng gần và không còn bị lifecycle quầy ghi đè sprite/transform.")
add()
add("Rarity shop hiện dùng 16 frame ánh sáng vẽ riêng xanh/tím/vàng/cầu vồng, ánh nền rộng và motes. Aura đi theo món hàng khi focus/reroll/rời quầy; companion mua xong không mang aura shop vào trận. Rương có quầng vàng và glint; hỗ trợ reduced flashes/effects. Sao 2/3 chỉ tăng thân ×1.35/×1.8, bỏ phụ kiện đính lên người. Hình thái build vẫn có choreography/đạn/VFX riêng; mô tả silhouette trong data còn là dấu vết thiết kế cũ.")
add()
add("Lỗi chớp/mất bộ phận khi ally đánh đã được sửa ở BodyPuppet: rect UV của frame được cập nhật đúng cho các mảng thân thay vì sao chép nhầm vùng vật liệu. Các báo cáo đã lưu bao phủ 66 loài ×3 sao và 31 hero qua 12 trạng thái, cả thường/giảm hiệu ứng. Đây là bằng chứng của lượt kiểm tra đó; hồ sơ xuất lần này không chạy lại toàn bộ QA đồ họa và không chứng minh mọi map/góc nhìn đã được duyệt tay.")
add()
add("Môi trường nightmare thêm sàn, mắt và art rìa từ wave 11; giữ alpha của props và khoảng chiến đấu ở giữa. Finale có dàn cảnh riêng, phần hậu cảnh ở rìa tránh che vùng né. VFX, projectile và audio có budget/cooldown để trận đông không phát vô hạn sự kiện; hiệu năng thực tế vẫn cần đo trên cấu hình reviewer mục tiêu.")
add()

heading("19. Toàn bộ nhạc và âm thanh", "audio")
music = json.loads((ROOT / "Assets/ArcaneBrigade/Resources/Audio/Arcane/Music/music.json").read_text(encoding="utf-8"))
cues = json.loads((ROOT / "Assets/ArcaneBrigade/Resources/Audio/Arcane/cues.json").read_text(encoding="utf-8"))
manifest = json.loads((ROOT / "Docs/Audio/Toybox_Asset_Manifest.json").read_text(encoding="utf-8"))
clip_count = sum(len(c["clips"]) for c in cues["cues"])
add(f"Runtime manifest có **{len(music['pieces'])} bài**, mỗi bài **4 stem** (tổng {sum(len(p['stems']) for p in music['pieces'])}); **{len(cues['cues'])} cue** ngoài nhạc với **{clip_count} đường clip**. Bộ nhạc hiện là Toybox revision 4: mỗi bài có score, hòa âm, bass, trống, phrase và form riêng. Foley revision 5 tổng hợp mới toàn bộ clip ngoài nhạc; không dùng các file studio khác theme làm nguồn.")
add()
table(["Piece ID", "Tên", "BPM", "Nhịp", "Số ô", "Loop giây", "Loop samples", "Scene"], ((p["id"], p["title"], p["bpm"], f"{p['beatsPerBar']}/4", p["bars"], p["loopSeconds"], p["loopSamples"], p["scenes"]) for p in music["pieces"]))
composition_doc = (ROOT / "Docs/Audio/Toybox_Individual_Compositions.md").read_text(encoding="utf-8-sig")
add("### Bản sắc từng bài\n")
for ln in composition_doc.splitlines():
    if ln.startswith("|"):
        add(ln)
add()
add("### Nhạc thích ứng và mix\n")
add("Bốn lớp base/rhythm/melody/intensity chạy đồng bộ DSP; đổi scene theo bar và crossfade. Độ đông combat và low-HP danger thay đổi trọng lượng stem. Depth nightmare giảm melody dần 1→0.5 và rhythm 1→0.85; dawn trả về trạng thái sáng. Pause/mất focus có xử lý dừng/khôi phục. Nhịp nhạc không khóa input và không tự thưởng crit do đánh đúng beat; cue `ui.encore.ready` là cue dự trữ, không chứng minh đã có một thanh Encore gameplay.")
add()
add("Sample rate 44.1 kHz; nhạc render master PCM24 và encode Vorbis chất lượng 8; SFX nguồn mono PCM16, peak thiết kế −3 dBFS, ambience khoảng −7 dBFS. Cue có gain riêng nên mức nghe thực còn qua bus/mix/ducking. Combat, UI, Foley, Voice, Ambience và Music được phân bus; danger ưu tiên sự kiện né đòn, cooldown/maxVoices hạn chế spam. Voice hiện là âm vô ngôn tổng hợp, không phải diễn viên đọc toàn bộ thoại.")
add()
add("### Toàn bộ cue runtime\n")
table(["Cue ID", "Bus", "Priority", "Gain dB", "Cooldown s", "Max voices", "Pitch jitter", "Loop", "Danger", "Clip paths"], ((c["id"], c["bus"], c["priority"], c["gainDb"], c["cooldown"], c["maxVoices"], c["pitchJitter"], c["loop"], c["danger"], c["clips"]) for c in cues["cues"]))
add("### Nguồn và khả năng tái lập\n")
add("Music soạn/render bằng code, nhạc cụ sampled dùng **GeneralUser GS 2.0.3** và percussion tổng hợp gốc. Foley dùng engine tổng hợp gốc, không sample/soundfont/studio đầu vào; voice là tổng hợp không lời. Audio fallback cũ còn credit riêng (các bộ Kenney CC0/K8) nếu thư viện mới thiếu. Các tuyên bố provenance cũ về bộ v2 hoàn toàn không sample không được áp sang nhạc Toybox dùng GeneralUser GS. Hồ sơ này giữ nguyên license và manifest hash ở phụ lục, không tự xác nhận quyền phát hành mọi asset khác của toàn game.")
add()
details("Manifest đầy đủ âm thanh: đường dẫn, nguồn, frame và SHA-256 từng asset", "json", jdump(manifest, True))
details("Manifest nhạc runtime đầy đủ, bao gồm đường dẫn 60 stem", "json", jdump(music, True))
license_path = ROOT / "Docs/Audio/ThirdParty/GeneralUser-GS-LICENSE.txt"
if license_path.exists():
    details("Toàn văn GeneralUser GS License đi kèm dự án", "text", license_path.read_text(encoding="utf-8-sig"))

heading("20. Giới hạn hiện tại và gợi ý kiểm tra review", "review-notes")
table(["Điểm cần biết", "Tình trạng thực tế / tác động khi review"], [
    ("Số nội dung", "31 hero, 66 ally, 170 item, 29 bond, 25 recipe; một số doc cũ ghi số khác"),
    ("Description và số hiệu lực", "Đã chuẩn hóa 66 companion và 170 item theo cơ chế hiện có; giữ metadata legacy để đối chiếu. UI dùng EffectiveModifiers, tier và số bản sở hữu. Giới hạn chiến đấu vẫn có thể giảm lợi ích stat"),
    ("Silhouette build", "Data còn mô tả phụ kiện nhưng code presentation hiện bỏ phụ kiện; đánh giá choreography và VFX thực"),
    ("Affix stat", "Có buff HP/damage/speed hiện hành; tài liệu cũ nói không buff là lỗi thời"),
    ("45 giây", "Nhịp wave thường; boss overtime, Keeper/finale và sự kiện có ngoại lệ"),
    ("Chất lượng/khác biệt nhạc", "Có 15 score riêng và audit kỹ thuật; cần reviewer nghe thực, không coi độ khác MIDI là hài lòng người chơi"),
    ("Boss moveset", "12 boss có encore chung có biến thể; không phải 12 bộ encore hoàn toàn độc lập"),
    ("Bất tử ally", "Đội hình xoay quanh nguồn damage/support/position, không có chiến thuật cứu ally khỏi chết trong bản hiện tại"),
    ("Rarity và số lượng", "Item tồn tại không bảo đảm xuất hiện trong mọi hero pool/shop; reward-only và cap vẫn áp dụng"),
    ("Ngôn ngữ", "Copy trong game chủ yếu tiếng Anh; hồ sơ tiếng Việt không phải gói localization"),
    ("Thoại độc nhất", "Có thoại hero và pool theo chất liệu/loài; chưa có bộ thoại dài riêng cho đủ 66 loài"),
    ("Art và QA", "Có frame/vẽ mới cho phần nội dung mở rộng; không đồng nghĩa vẽ lại toàn bộ art cũ hoặc duyệt thủ công mọi góc map"),
    ("Balance dài hạn", "Các test dữ liệu và render không chứng minh economy/Endless luôn cân bằng hoặc mọi build đều thú vị"),
    ("Phạm vi hồ sơ", "Xuất bản working tree hiện tại; không thay gameplay, không tự coi tất cả check cũ là check mới của snapshot này")])
add("Một lượt review hữu ích nên đi qua: một hero có đoàn quân và một hero solo/hấp thụ; build melee/ranged/collision và ít nhất hai morph khác hướng; mua/ghép ở 10–20 ally; một altar recipe; một secret combat và một hợp đồng; Keeper; Last Customer, hai farewell và Endless. Ghi cảm nhận về nhận diện đòn, sự chồng lấp VFX/audio, giá sau wave 8, cơ hội hoàn thành ghép và thời gian hạ boss. Đây là phạm vi trải nghiệm đề xuất cho review, không phải báo cáo đã chơi qua toàn bộ ở lần xuất này.")
add()
evidence_paths = ["PreviewExports/October/Verification.json", "PreviewExports/CompanionVisibility/Verification.json", "PreviewExports/Secrets/Verification.json", "PreviewExports/BuildMorphs/Verification.json", "PreviewExports/Nightmare/Verification.json"]
for rel in evidence_paths:
    p = ROOT / rel
    if p.exists():
        details("Bằng chứng kiểm tra đã lưu: " + rel + " (không chạy lại khi xuất hồ sơ)", "json", p.read_text(encoding="utf-8-sig"))

heading("21. Phụ lục dữ liệu đầy đủ, cấu hình và lời thoại", "raw")
add("Phần tra cứu phía trên dùng số làm tròn ngắn để dễ đọc. Phần dưới giữ giá trị float xuất round-trip từ Unity, đầy đủ các trường public của asset, cả partial class, tham chiếu sprite/motion/acting, cấu hình chưa override và enum/constant. Tham chiếu asset ở đây là định danh xuất xứ, không phải tệp bắt buộc phải mở thêm để đọc hồ sơ. Chuỗi thoại có escape `\\n` thuộc dữ liệu gốc, không phải lỗi mất dòng của hồ sơ.")
add()
details("Cấu hình RunCatalog đầy đủ (wave modifiers, proc rules, chỉ số và danh sách catalog)", "json", jdump(D["Catalog"], True))
details("Cấu hình Checkpoint/Arena gốc (trước PrepareCombat)", "json", jdump(D["Arena"], True))
details("Toàn bộ lời thoại theo depth, Dream, Whisper và Evidence", "json", jdump(D["NarrativeTables"], True))
details("Tất cả enum và constant runtime được xuất", "json", jdump({"Enums": D["Enums"], "RuntimeConstants": D["RuntimeConstants"]}, True))
extra_live = {"OutsideStageKeeper", "TheLastCustomer", "SecretCabinetGuardian"}
table(["Unit asset ngoài 4 catalog chính", "Vai trò khi review", "Asset"], ((u["Id"], "Đang dùng ở nhánh đặc biệt" if u["Id"] in extra_live else "Asset checkpoint/legacy; không cộng vào số nội dung active khi chưa chứng minh đường gọi", u["AssetPath"]) for u in D["OtherUnitAssets"]))
details("Snapshot Unity toàn phần — mọi trường dữ liệu của toàn bộ danh mục", "json", snapshot_records(D))

heading("22. Phụ lục code công thức thực thi và nguồn gốc", "source")
add("Các công thức chuyên biệt về tài năng, proc, item tradeoff và thời gian tấn công có nhiều nhánh hơn một bảng stat. Dưới đây là **bản sao nội dung mã tại thời điểm xuất**, giữ nguyên để reviewer kỹ thuật tra cứu mọi hệ số và thứ tự áp dụng trong chính file này. Tên file và SHA-256 dùng định vị/kiểm tra xuất xứ. Không cần checkout repo để đọc. Các tên Kxx/October/Carnival là tên lớp tích hợp, không phải mode sản phẩm hiển thị cho người chơi.")
add()
exclude_fragments = ("Capture", "Fixture", "Measurement", "Checks", "Smoke", "Framebuffer", "Recorder", "NativeMetrics")
if "WaitingChapters" in D:
    heading("Miền Chờ: cốt truyện và ánh sáng đồng hành", "mien-cho-shared-light", 2)
    add("10 mốc cố định / 27 trang trong chiến dịch 20 wave; 31 đoạn nhập cảnh, 66 tiểu sử và 6 tuyến companion theo chặng. Journal ở shop bằng J, có pause, scroll và lưu acknowledgement. Các vignette là nội dung viết sẵn; chưa có ba chiến dịch riêng hoặc hệ quest phân nhánh cho mọi loài.")
    add("Ánh sáng world-space 256×192 với tối đa 16 nguồn: hero bán kính 6.4 và mỗi companion 3.4. Gộp nguồn theo giá trị lớn nhất, không cộng vô hạn. Địch xa có silhouette/mắt hoặc ember; trong vùng contact an toàn hoặc windup thì thân hiện rõ, boss giữ ít nhất 80%. High Readability và giảm hiệu ứng giữ ánh sáng thiết yếu. Hai ending đều là bình minh thật.")
    add("Act I now uses thirteen newly illustrated assets in the existing game's style: drawn lanterns in three damage states, chair, dining group, telephone, mirror, three story panels and three room backgrounds. Blender lamp meshes and render assets have been retired outside Assets. Unity light fields and black fog remain; levitation preserves floating, lean and fixed floor shadows without under-object smoke/wind. Three 2D rooms can be explored on foot; six optional companion routes store three binary responses each and contribute authored epilogues. Later acts remain deferred. Details: Docs/Mien_Cho_Painted_Art_Pass.md.")
    add("Journal now saves a separate page for every chapter and distinguishes read-later acknowledgement from last-page completion. Three optional in-world shop encounters appear after waves 4-5 (reserved table), 6-7 (disconnected phone) and 10-11 (delayed reflection of an actual party member). They pause with the world, never appear during combat and have no stat or currency reward. The current transparent sprites are newly illustrated; retired Blender sources are historical only.")
    for chapter in D["WaitingChapters"]:
        add("### Wave " + str(chapter["Wave"]) + " / " + chapter["Title"])
        add("Evidence: " + chapter["Evidence"])
        for page_index, page_text in enumerate(chapter["Pages"], 1):
            add("**Page " + str(page_index) + "**\n\n" + page_text)
    details("31 dream arrivals", "json", jdump(D["WaitingArrivals"], True))
    details("66 lives and companion arc excerpts by progression", "json", jdump(D["WaitingLives"], True))
source_paths = sorted(p for p in (ROOT / "Assets/ArcaneBrigade/Runtime/Core").glob("*.cs") if not any(x in p.stem for x in exclude_fragments))
source_paths += sorted((ROOT / "Assets/ArcaneBrigade/Runtime/Data").glob("*.cs"))
source_paths += sorted((ROOT / "Assets/ArcaneBrigade/Runtime/Persistence").glob("*.cs"))
for name in ["NightmareDialogue", "OctoberDialogue", "MerchantDialogue", "ArcaneAudio", "ArcadeMusic", "SecretEventRoom", "NightmareFarewellRoom", "NightmareDeparture", "OutsideStageChoice", "StageTransfer", "BuildMorphAppearance", "CharacterActingLibrary", "HeroIdentityVisual", "MienChoPresentation", "SharedLightField", "NightmareLighting", "NightmarePresence", "WaitingRealmStage", "WaitingShopEncounter", "WaitingJourney", "WaitingRoomController", "NightLanternField", "DreamLevitation", "FloatingStoryProp", "WaitingCampaign", "WaitingCompanionQuests", "WaitingRooms"]:
    source_paths.append(ROOT / f"Assets/ArcaneBrigade/Runtime/Presentation/{name}.cs")
for name in ["RunUICopy", "RunBuildReadModel", "BuildMorphReadModel", "BondHudSummary", "ShortOfferCopy", "ContentDescriptions", "HeroIdentityOptions", "ShopStatFeedback"]:
    source_paths.append(ROOT / f"Assets/ArcaneBrigade/Runtime/UI/{name}.cs")
source_paths = list(dict.fromkeys(p for p in source_paths if p.exists()))
source_manifest = []
for p in source_paths:
    rel = p.relative_to(ROOT).as_posix()
    raw = p.read_bytes()
    sha = hashlib.sha256(raw).hexdigest()
    source_manifest.append({"path": rel, "sha256": sha})
    details(rel + " | SHA-256 " + sha, "csharp", raw.decode("utf-8-sig"))
for rel in ["Docs/Audio/Toybox_Delivery.md", "Docs/Audio/Toybox_Foley_Designs.md", "Docs/October_Art_Provenance.json", "Docs/Secret_Event_Art_Prompts.json"]:
    p = ROOT / rel
    if p.exists():
        # Keep supporting provenance inside the same artifact, but neutralize links
        # that would imply a second required document.
        body = re.sub(r"\[([^\]]+)\]\((?!https?://)[^)]+\)", r"\1", p.read_text(encoding="utf-8-sig"))
        details("Nguồn hỗ trợ được nhúng: " + rel, "json" if p.suffix == ".json" else "text", body)

head = subprocess.run(["git", "rev-parse", "HEAD"], cwd=ROOT, capture_output=True, text=True, check=True).stdout.strip()
snapshot_sha = hashlib.sha256(SNAPSHOT.read_bytes()).hexdigest()
heading("Biên bản xuất và tự kiểm tra hồ sơ", "export-validation", 3)
add(f"Snapshot nguồn SHA-256: `{snapshot_sha}`. Git HEAD để định danh nền: `{head}`; có thay đổi working tree chưa commit. Unity export hoàn tất batchmode thành công; exporter chỉ đọc catalog và tạo RunSession tạm để xem compile/preview, không lưu profile hay thay asset gameplay. Script tạo hồ sơ: `Tools/Review/export_game_review.py`.")
add()
counts = {g: len(ids) for g, ids in coverage.items()}
table(["Danh mục kiểm đếm trong phần đọc được", "Đã xuất đủ"], counts.items())
add(f"Có {len(source_paths)} tệp mã runtime được nhúng cho công thức/nhánh đặc biệt; {len(D['HeroBaselines'])} baseline tổng hợp; {len(D['EventMemories'])} ký ức lựa chọn; {len(D['Endings'])} bộ trang kết đại diện. Audio cue/stem và snapshot toàn trường cũng đã nhúng. Các kiểm tra này xác nhận tính đầy đủ của hồ sơ xuất, không thay thế QA chơi game.")
add()

report = "\n".join(lines).replace("\r\n", "\n") + "\n"
# Completeness assertions use generated entries, not incidental appearances in raw JSON.
expected = {"hero": D["Champions"], "ally": D["Companions"], "enemy": D["Enemies"],
            "boss": D["Bosses"] + [units["OutsideStageKeeper"], units["TheLastCustomer"]],
            "Weapons": D["Weapons"], "OtherWeaponAssets": D["OtherWeaponAssets"], "Items": D["Items"],
            "Bonds": D["Bonds"], "Recipes": D["Recipes"], "BuildLinks": D["BuildLinks"], "Achievements": D["Achievements"]}
for group, entries in expected.items():
    assert coverage[group] == [x["Id"] for x in entries], group
assert coverage["Morphs"] == [x["Kind"] for x in D["Morphs"]]
assert coverage["SecretRooms"] == [x["Kind"] for x in D["SecretRooms"]]
assert len(anchors) == len(re.findall(r'<a id="([^"]+)"></a>', report))
missing = sorted(set(re.findall(r"\]\(#([^\s)]+)\)", report)) - anchors)
assert not missing, missing
assert "\ufffd" not in report, "Unicode replacement character"
assert sum(ln.startswith("````") for ln in report.splitlines()) % 2 == 0
OUT.parent.mkdir(parents=True, exist_ok=True)
OUT.write_text(report, encoding="utf-8", newline="\n")
validation = {"artifact": OUT.relative_to(ROOT).as_posix(), "bytes": OUT.stat().st_size,
              "sha256": hashlib.sha256(OUT.read_bytes()).hexdigest(), "snapshotSha256": snapshot_sha,
              "exportedAtVietnam": D["ExportedAtVietnam"], "coverage": counts,
              "audio": {"pieces": len(music["pieces"]), "stems": sum(len(p["stems"]) for p in music["pieces"]),
                        "cues": len(cues["cues"]), "clipPaths": clip_count},
              "anchors": len(anchors), "unresolvedInternalLinks": missing,
              "sourceFiles": source_manifest, "status": "PASS"}
(ROOT / "Library/GameReview/Validation.json").write_text(jdump(validation, True), encoding="utf-8")
print(jdump({k: v for k, v in validation.items() if k != "sourceFiles"}, True))
