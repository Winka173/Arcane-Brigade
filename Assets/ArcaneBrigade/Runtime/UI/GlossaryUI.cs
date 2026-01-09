using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace ArcaneBrigade
{
    public static class GlossaryUI
    {
        public sealed class Entry { public string Name; public string Description; public ItemEffect Effect; }
        private static readonly Dictionary<string, Entry> entries = Build();
        private static readonly Regex tokens = new Regex("((?<!\\p{L})(?:" + string.Join("|", entries.Keys.OrderByDescending(key => key.Length).Select(Regex.Escape)) + ")(?!\\p{L})|\\r?\\n|[^\\S\\r\\n]+|[^\\s]+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static Entry selected;
        private static float openedAt;
        private static readonly Dictionary<GUIStyle, GUIStyle> wordStyles = new Dictionary<GUIStyle, GUIStyle>();
        public static bool Open => selected != null;
        public static void Close() { selected = null; }
        public static Entry Find(string term) => entries.TryGetValue(term.Trim().TrimEnd('.', ',', ':', ';'), out Entry entry) ? entry : null;
        public static bool Show(string term) { selected = Find(term); openedAt = Time.unscaledTime; return selected != null; }
        public static void BlockUnderlying(Vector2 viewport)
        {
            if (selected == null) return;
            float width = Mathf.Min(620, viewport.x - 60);
            Rect panel = new Rect(viewport.x - width - 35, 150, width, 285);
            Event current = Event.current;
            if (!panel.Contains(current.mousePosition) || current.type != EventType.MouseDown && current.type != EventType.MouseUp && current.type != EventType.ScrollWheel && current.type != EventType.MouseDrag) return;
            if (current.type == EventType.MouseUp)
            {
                if (new Rect(panel.xMax - 55, panel.y + 15, 40, 40).Contains(current.mousePosition)) Close();
                GUIUtility.hotControl = 0;
            }
            current.Use();
        }

        private static Dictionary<string, Entry> Build()
        {
            var result = new Dictionary<string, Entry>(StringComparer.OrdinalIgnoreCase);
            foreach (ItemEffect effect in Enum.GetValues(typeof(ItemEffect)))
            {
                string name = RunModifierRules.ModifierName(effect);
                if (!result.ContainsKey(name)) result[name] = new Entry { Name = name, Effect = effect, Description = Definition(effect) + "\nPhạm vi: " + RunModifierRules.ScopeName(RunModifierRules.DefaultScope(effect)) + ". Giữ trong lượt, lượt mới reset." };
            }
            Add(result, "xuyên", "Đạn xuyên", ItemEffect.BulletPierce, "Số mục tiêu bổ sung một đạn có thể xuyên qua. Đạn thường không đánh lại cùng mục tiêu; cộng từ weapon, cấp và item, tối đa 64 lượt xuyên.", "pierce");
            Add(result, "split", "Chia nhánh đạn", ItemEffect.BulletSplit, "Tạo đạn con sau va chạm đầu, tối đa 6 nhánh. Đạn con gây 35% damage, nhỏ hơn và không split tiếp; không hồi lại thời gian sống.", "chia nhánh đạn");
            Add(result, "nảy", "Nảy đạn", ItemEffect.BulletBounce, "Xác suất đổi hướng tới mục tiêu chưa bị đạn này đánh. Tối đa 8 lần nảy xác suất. Nảy cố định của vũ khí là thuộc tính riêng; lifetime không được gia hạn.");
            Add(result, "chí mạng", "Chí mạng", ItemEffect.Crit, "Xác suất đòn đánh chí mạng; cộng trước rồi giới hạn 0–100%. Đây không phải né tránh.", "crit");
            Add(result, "né tránh", "Né tránh", ItemEffect.Dodge, "Roll riêng từng đòn nhận, tối đa 60%. Thành công không mất máu từ đòn đó; không có thời gian miễn damage chung sau mỗi đòn.", "dodge");
            Add(result, "giáp", "Giáp", ItemEffect.Armor, "Giáp dương giảm damage nhận theo 1/(1+giáp). Giáp âm làm nhận nhiều damage hơn, tối đa ×2 trước giới hạn mỗi đòn.", "armor");
            Add(result, "damage", "Sát thương", ItemEffect.Damage, "Damage cận/xa cộng thẳng được cộng vào gốc trước hệ số %. Chí mạng, phản ứng và hiệu ứng phụ tính theo luật riêng; bảng đóng góp chỉ ghi lượng máu thực mất.");
            Add(result, "hệ", "Liên kết hệ", ItemEffect.CompanionDamage, "Đếm số người trong roster có tag đó, không đếm sao. Mốc 2/4/6 có thưởng; một hệ có 4–6 loài để tuyển, không phải giới hạn số quân sở hữu.");
            Add(result, "merge", "Ghép cấp", ItemEffect.CompanionDamage, "Ba người cùng loại/cấp ghép thành một người cấp cao. Support cấp 1/2/3 ×1/2/4, không giữ ba nguồn cũ. Vũ khí recruit ngẫu nhiên, không trùng cùng loại + cùng weapon.", "tier");
            Add(result, "altar", "Quầy giấc mơ", ItemEffect.Luck, "Quầy đặc biệt xuất hiện theo luật lượt, dùng trứng hoặc máu hiện tại. Mua bằng máu phải còn ít nhất 1 HP; bonus của món mới không trả hộ giá mua.");
            Add(result, "reroll", "Đổi hàng", ItemEffect.Discount, "Trả vàng để đổi các món đang bày. Không bán hoặc khóa hàng; giảm giá món không tự giảm phí reroll.");
            Add(result, "stack", "Cộng dồn", ItemEffect.Damage, "Nhiều bản item cộng bonus theo stack hiệu lực. Item duy nhất không cộng thêm bản thứ hai. Cap kỹ thuật không xóa stack sở hữu.", "cộng dồn");
            Add(result, "scope", "Phạm vi tác động", ItemEffect.CompanionDamage, "Thủ lĩnh, đồng minh, toàn đội hoặc lượt chơi. Bonus chỉ áp dụng đúng phạm vi; bonus thủ lĩnh không tự chuyển sang đồng minh.", "toàn đội", "lượt chơi");
            Add(result, "hiệu chỉnh", "Hiệu chỉnh chỉ số", ItemEffect.DamageCorrection, "Hệ số bổ sung nhân sau các bonus cơ bản của chỉ số tương ứng, có sàn/cap riêng. Không phải chỉ số vĩnh viễn của hồ sơ.");
            Add(result, "vũ khí", "Vũ khí", ItemEffect.MeleeDamage, "Quy định tầm, damage gốc, chu kỳ, loại đòn, nguyên tố và hành vi đạn. Chỉ tự đánh; bàn tay và vũ khí tách khỏi thân, không có cánh tay.");
            Add(result, "thủ lĩnh", "Thủ lĩnh", ItemEffect.Health, "Nhân vật bạn điều khiển bằng di chuyển; tự đánh. Chết kết thúc lượt; các đồng minh bất tử không chặn đạn mặc định.");
            Add(result, "đồng minh", "Đồng minh", ItemEffect.CompanionDamage, "Quân tự tuần tra/nghỉ/đánh quanh thủ lĩnh. Có đặc tính lợi/hại riêng, không phí nuôi, không giới hạn roster.");
            Add(result, "nội tại", "Nội tại tự động", ItemEffect.Heal, "Hiệu ứng tự kích hoạt theo chu kỳ/điều kiện, không có nút kỹ năng. Hồi máu, tìm vàng và chill của đơn vị được đo riêng, không cộng vào một con số damage giả.", "tự đánh");
            Add(result, "boomerang", "Vũ khí quay về", ItemEffect.BulletDistance, "Bay ra rồi quay về chủ. Không hồi lại lifetime; split/pierce tuân theo luật đạn của vũ khí, không biến mọi vũ khí thành boomerang.");
            Add(result, "máu", "Máu hiện tại / tối đa", ItemEffect.Health, "Máu hiện tại là lượng còn sống; máu tối đa là trần sau build. Mua bằng máu trả lượng hiện tại, phải còn 1 HP. Mua/ghép đổi maximum giữ lượng máu đã mất.", "HP");
            Add(result, "vàng", "Vàng", ItemEffect.Income, "Nhặt trong trận, nhận thu nhập/lãi sau màn và dùng tại quầy thường. Vàng còn trong trận chỉ chốt vào save khi kết thúc màn, không sinh ra từ animation.");
            Add(result, "trứng", "Trứng", ItemEffect.Luck, "Tiền riêng cho quầy giấc mơ/altar. Một số món ở quầy đó trả bằng máu thay vì trứng; hai chi phí không cộng làm một.");
            Add(result, "bất tử", "Đồng minh bất tử", ItemEffect.Interceptor, "Đồng minh không mất máu hoặc chết. Không chặn đạn địch mặc định; chỉ item chặn đạn mới bật cơ chế đó. Thủ lĩnh không bất tử.");
            Add(result, "vô tận", "Chặng vô tận", ItemEffect.EnemyCount, "Chơi tiếp sau thắng, giữ roster/item/stat của lượt. Quái tiếp tục mạnh lên theo wave; không cộng stat vào hồ sơ lượt mới.");
            Add(result, "BƯỚC ĐÈN", "Bước Đèn", ItemEffect.CollisionDamage, K11HeroRules.Describe(HeroMechanic.LanternStride), "Lantern stride");
            Add(result, "CHUYỂN PHÁT", "Chuyển Phát", ItemEffect.Projectiles, K11HeroRules.Describe(HeroMechanic.ParcelExpress), "Parcel express");
            Add(result, "NHỊP MƠ", "Nhịp Mơ", ItemEffect.Dodge, K11HeroRules.Describe(HeroMechanic.DreamBreath), "Dream breath");
            Add(result, "THEME TUYỂN QUÂN", "Theme tuyển quân", ItemEffect.CompanionDamage, "Chỉ đồng minh thuộc các hệ đã chọn của thủ lĩnh được roll trong shop thường. Vũ khí vẫn ngẫu nhiên. Quân có sẵn trong save và hợp thể không bị xóa chỉ vì khác theme.");
            return result;
        }
        private static void Add(Dictionary<string, Entry> result, string key, string name, ItemEffect effect, string description, params string[] aliases)
        {
            var entry = new Entry { Name = name, Effect = effect, Description = description }; result[key] = entry;
            foreach (string alias in aliases) result[alias] = entry;
        }

        private static string Definition(ItemEffect effect) => effect switch
        {
            ItemEffect.Damage or ItemEffect.ChampionDamage or ItemEffect.CompanionDamage => "Tăng damage theo %, khác damage cận/xa cộng thẳng. Scope quyết định thủ lĩnh, đồng minh hay toàn đội; không tự chuyển buff giữa các scope.",
            ItemEffect.AttackSpeed or ItemEffect.CompanionAttackSpeed => "Chia chu kỳ đánh cho 1 + bonus. Tốc độ đánh cao giảm thời gian giữa các đòn, không tăng tốc di chuyển hoặc tốc độ bay đạn.",
            ItemEffect.Health => "Tăng trần máu. Maximum = (gốc + bonus HP) × hiệu chỉnh máu, tối thiểu 1. Thay maximum khi mua/ghép giữ lượng máu đã mất.",
            ItemEffect.MoveSpeed => "Nhân tốc độ di chuyển gốc theo bonus. Không tăng tốc độ đánh; bonus âm làm chậm nhưng hệ số có sàn để vẫn di chuyển được.",
            ItemEffect.Pickup => "Khoảng cách kích hoạt hút loot tự bay tới người. Tính bằng đơn vị map; không làm đạn hoặc hitbox người chơi lớn hơn.",
            ItemEffect.Roam => "Bán kính đồng minh được tuần tra quanh thủ lĩnh. Không phải tầm đánh; đồng minh vẫn có thời gian nghỉ riêng.",
            ItemEffect.Interceptor => "Bật chặn đạn địch bằng đồng minh bất tử. Không bật mặc định; không bảo vệ khỏi mọi dạng va chạm hoặc hazard.",
            ItemEffect.Heal => "HP hồi mỗi giây từ stat liên tục. Nội tại từng đơn vị, tim và proc hồi máu là nguồn khác, không gộp sai vào số này.",
            ItemEffect.Crit => "Tỉ lệ đòn chí mạng, cộng trước rồi giới hạn 0–100%. Không phải né tránh hoặc hệ số damage cận cộng thẳng.",
            ItemEffect.Projectiles => "Số đạn trong một lần bắn trước split. Một đòn cận không tự thành đạn; có giới hạn kỹ thuật 96 đạn mỗi lần bắn.",
            ItemEffect.Interest => "Tăng giới hạn tiền lãi tại kỳ nghỉ. Lãi thực còn phụ thuộc lượng vàng giữ và divisor của luật lượt; không phải thu nhập nền.",
            ItemEffect.Armor => "Giáp dương giảm damage nhận theo 1/(1+giáp). Giáp âm làm nhận nhiều damage, tối đa ×2 trước giới hạn mỗi đòn.",
            ItemEffect.Discount => "Giảm giá món bày bằng vàng, tối đa 50% khi cộng các discount. Không giảm reroll hoặc tự giảm giá trả bằng máu/trứng.",
            ItemEffect.Luck => "Tăng cơ hội loot và góp 25% bonus vào trọng số độ hiếm hàng khi roll shop. Không sửa chí mạng hoặc damage RNG.",
            ItemEffect.TreasureDetection => "Rương xuất hiện sớm hơn, tối thiểu cách nhau 4 giây. Góp 25% bonus vào chất lượng vàng của rương.",
            ItemEffect.ScoreMultiplier => "Nhân điểm build sau điểm nền/độ khó. Giá trị được chốt vào snapshot khi mua/kết thúc màn; không bịa điểm từ animation.",
            ItemEffect.CollisionDamage => "Damage tự gây khi người/đồng minh chạm địch, có cooldown riêng. Không phải lượng damage địch gây ngược vào bạn.",
            ItemEffect.MeleeDamage => "Bonus cố định cộng vào damage gốc của đòn cận trước các hệ số %. Không tăng damage đạn; có thể bị giảm bởi tradeoff.",
            ItemEffect.RangedDamage => "Bonus cố định cộng vào damage gốc đạn trước các hệ số %. Khác số đạn, split và damage toàn đội theo %.",
            ItemEffect.AttackRange => "Tăng tầm chọn/đánh của weapon và khoảng bay đạn. Không tăng bán kính tuần tra; tốc độ bay được tính riêng.",
            ItemEffect.Dodge => "Roll riêng mỗi đòn nhận, tối đa 60%. Thành công tránh damage của đòn đó, không tạo thời gian bất tử chung.",
            ItemEffect.Income => "Cộng vàng nền nhận sau wave và lúc vào chặng vô tận. Khác tiền lãi và vàng nhặt; có thể âm để giảm khoản nhận.",
            ItemEffect.BulletPierce => "Số mục tiêu thêm đạn xuyên được. Victim tracking tránh đánh lại cùng người; cộng weapon/cấp/stat, tối đa 64.",
            ItemEffect.BulletSplit => "Tạo đạn con ở impact đầu, tối đa 6 nhánh, mỗi nhánh 35% damage và nhỏ hơn. Không split đệ quy hay hồi lifetime.",
            ItemEffect.BulletBounce => "Xác suất đổi hướng sang mục tiêu chưa bị đạn đánh, 0–100%, tối đa 8 lần. Nảy cố định của weapon là nguồn riêng.",
            ItemEffect.BulletSize => "Nhân đồng thời chiều rộng render và radius collision đạn, ×0,25–4. Không chỉ phóng sprite để tạo cảm giác damage giả.",
            ItemEffect.BulletSpeed => "Tăng tốc bay. Lifetime giảm tương ứng để không tự nhận thêm tầm; tầm/khoảng bay có stat riêng.",
            ItemEffect.BulletDistance => "Tăng thời gian/khoảng bay còn lại. Không gia hạn khi split/nảy; boomerang vẫn có luật quay về chủ riêng.",
            ItemEffect.Knockback => "Tăng khoảng đẩy lùi tự động của weapon khi hit. Không thêm nút đẩy tay; trùm/hành vi đặc biệt có luật riêng.",
            ItemEffect.BodySize => "Tăng/giảm sprite và hitbox thủ lĩnh cùng nhau. Người to dễ bị chạm hơn; đây không phải bonus HP.",
            ItemEffect.CompanionSize => "Tăng/giảm sprite và radius đồng minh. Không đổi số người/hệ, không làm đồng minh mất trạng thái bất tử.",
            ItemEffect.EnemyCount => "Thay ngân sách quái và nhịp spawn. Có thể đổi áp lực lấy cơ hội loot/điểm; không tăng số loài quân bạn sở hữu.",
            ItemEffect.EnemyHealth => "Nhân HP quái bên cạnh curve wave/độ khó. Quái late-game còn nhận hệ số tăng riêng, không chỉ lớn lên bằng hình vẽ.",
            ItemEffect.EnemySpeed => "Nhân tốc di chuyển quái. Không tăng tốc đạn địch; hostile bullet vẫn giữ trần tốc độ để đọc được.",
            ItemEffect.HeartHealing => "Cộng HP hồi từ tim nhặt được. Không cộng vào regen liên tục hoặc mọi nội tại hồi máu.",
            ItemEffect.ChestQuality => "Nhân lượng vàng khi tạo rương, hệ số ×0,2–5. Không tự tạo item rarity mới hoặc đổi một rương thành tất cả loot.",
            ItemEffect.RecruitDiscount => "Giảm riêng giá tuyển đồng minh bằng vàng; cộng với discount chung, tổng tối đa 50%. Không giảm giá mọi loại món.",
            ItemEffect.RelicDiscount => "Giảm riêng giá item bằng vàng; cộng với discount chung, tổng tối đa 50%. Không tác động reroll hoặc blood shop.",
            ItemEffect.ShopRarity => "Tăng trọng số của rarity cao khi tạo hàng mới. Không nâng cấp rarity của món đang sở hữu/đang bày, không bảo đảm roll hiếm.",
            ItemEffect.HealthCorrection => "Nhân maximum HP sau cộng bonus HP gốc; khác HP hiện tại và giá máu trả tại quầy.",
            ItemEffect.DamageCorrection => "Nhân damage sau gốc + bonus cận/xa và các hệ số damage thường, đúng scope của nguồn.",
            ItemEffect.SpeedCorrection => "Hệ số thêm nhân tốc di chuyển sau MoveSpeed; không sửa attack cooldown hoặc tốc bay đạn.",
            ItemEffect.ArmorCorrection => "Nhân lượng giáp bonus trước công thức giảm/tăng damage nhận. Không phải cộng trực tiếp phần trăm miễn damage.",
            ItemEffect.HealCorrection => "Nhân regen liên tục khi compile combat. Không tự nhân tim, mọi proc hoặc healing contribution đã ghi.",
            ItemEffect.CollisionFrequency => "Giảm cooldown damage va chạm gây ra bằng hệ số 1+bonus. Khoảng hiệu lực 0,15–4 giây; không giảm cooldown địch chạm bạn.",
            _ => "Xem scope và nguồn của modifier trong sổ; không có bonus ẩn từ một dòng chú thích."
        };

        public static float Height(string text, GUIStyle style, float width) => Layout(new Rect(0, 0, width, 10000), text, style, false);
        public static void Text(Rect bounds, string text, GUIStyle style) { Layout(bounds, text, style, true); }
        private static float Layout(Rect bounds, string text, GUIStyle style, bool draw)
        {
            if (!wordStyles.TryGetValue(style, out GUIStyle words))
            {
                if (wordStyles.Count > 32) wordStyles.Clear();
                words = new GUIStyle(style) { wordWrap = false, clipping = TextClipping.Overflow, padding = new RectOffset(), margin = new RectOffset() }; wordStyles[style] = words;
            }
            if (draw && words.font != null) words.font.RequestCharactersInTexture(text ?? "", words.fontSize, words.fontStyle);
            float line = Mathf.Max(style.lineHeight, style.fontSize * 1.25f);
            float horizontal = 0, vertical = 0;
            foreach (Match token in tokens.Matches(text ?? ""))
            {
                string word = token.Value;
                if (word.Contains("\n")) { horizontal = 0; vertical += line; continue; }
                Entry entry = Find(word);
                float icon = entry == null ? 0 : line * 0.85f;
                float width = words.CalcSize(new GUIContent(word)).x + icon + (entry == null ? 0 : 2);
                if (horizontal > 0 && horizontal + width > bounds.width) { horizontal = 0; vertical += line; }
                if (draw)
                {
                    Rect target = new Rect(bounds.x + horizontal, bounds.y + vertical, width, line);
                    if (entry != null)
                    {
                        StatGlyph.Draw(new Rect(target.x, target.y, icon, line), entry.Effect);
                        Color previous = GUI.color; GUI.color *= ArcadeSkin.Teal;
                        Rect caption = new Rect(target.x + icon, target.y, width - icon, line);
                        GUI.Label(caption, word, words);
                        if (GUI.Button(caption, GUIContent.none, GUIStyle.none)) Show(word);
                        GUI.color = previous;
                        ArcadeSkin.Line(new Rect(target.x + icon, target.yMax - 2, width - icon, 1), new Color(0.3f, 0.85f, 0.75f, 0.35f));
                    }
                    else GUI.Label(target, word, words);
                }
                horizontal += width;
            }
            return vertical + line;
        }
        public static void Draw(Vector2 viewport)
        {
            if (selected == null) return;
            if (Time.unscaledTime - openedAt > 30) { Close(); return; }
            float width = Mathf.Min(620, viewport.x - 60);
            Rect panel = new Rect(viewport.x - width - 35, 150, width, 285);
            ArcadeSkin.Ticket(panel, ArcadeSkin.Ink, ArcadeSkin.Teal);
            StatGlyph.Draw(new Rect(panel.x + 20, panel.y + 18, 36, 36), selected.Effect);
            GUIStyle heading = ArcadeTypography.Style(24, true); heading.normal.textColor = ArcadeSkin.Cream;
            GUIStyle body = ArcadeTypography.Style(20); body.normal.textColor = ArcadeSkin.Cream;
            GUI.Label(new Rect(panel.x + 68, panel.y + 20, width - 125, 40), selected.Name, heading);
            if (GUI.Button(new Rect(panel.xMax - 55, panel.y + 15, 40, 40), "×")) Close();
            if (selected != null) GUI.Label(new Rect(panel.x + 25, panel.y + 75, width - 50, 180), selected.Description, body);
            if (Time.unscaledTime - openedAt > 30) Close();
        }
    }
}
