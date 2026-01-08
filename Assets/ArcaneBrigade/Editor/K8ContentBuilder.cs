using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class K8ContentBuilder
    {
        public static void Apply()
        {
            RunCatalog catalog = AssetDatabase.LoadAssetAtPath<RunCatalog>("Assets/ArcaneBrigade/Resources/RunCatalog.asset");
            if (catalog == null) throw new InvalidOperationException("K8: Run catalog missing");
            if (catalog.ContentRevision > 0)
            {
                LocalizeItems(catalog); EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssets(); return;
            }
            SetChampion(catalog, "LanternKnight", "Hiệp Sĩ Đèn", new[]
            {
                Modifier(ItemEffect.CompanionDamage, 0.25f), Modifier(ItemEffect.CompanionAttackSpeed, 0.15f), Modifier(ItemEffect.Roam, 0.6f)
            }, "Dẫn đoàn: đồng minh +25% sát thương, +15% tốc độ đánh, tuần tra xa thêm 0,6. Sát thương riêng tăng theo số đồng minh nhờ nội tại tập hợp.");
            SetChampion(catalog, "Postmaster", "Trưởng Bưu Cục", new[]
            {
                Modifier(ItemEffect.Discount, 0.15f), Modifier(ItemEffect.Pickup, 0.5f), Modifier(ItemEffect.Projectiles, 1f, ModifierScope.Champion)
            }, "Giao hàng kép: thủ lĩnh +1 đạn mỗi đòn bắn; nhặt đồ xa thêm 0,5. Hàng bằng vàng ở lần tạo cửa hàng tiếp theo rẻ hơn 15%; không giảm phí đổi hàng. Nhặt tiền tự động mỗi 4 giây theo nội tại.");
            SetChampion(catalog, "DrowsySeer", "Tiên Tri Ngái Ngủ", new[]
            {
                Modifier(ItemEffect.Armor, 0.35f), Modifier(ItemEffect.Heal, 0.6f), Modifier(ItemEffect.Crit, 0.15f, ModifierScope.Champion)
            }, "Giấc ngủ an lành: thủ lĩnh +0,35 giáp, hồi thêm 0,6 máu/giây, +15 điểm phần trăm chí mạng. Nội tại hồi máu mỗi 4 giây vẫn hoạt động; không có kỹ năng bấm tay.");
            var companionNames = new Dictionary<string, string>
            {
                { "TeapotPilgrim", "Lữ Khách Ấm Trà" }, { "CandleSnail", "Ốc Sên Nến" }, { "MailboxImp", "Tiểu Quỷ Hòm Thư" },
                { "PillowOracle", "Nhà Tiên Tri Gối" }, { "MushroomLibrarian", "Thủ Thư Nấm" }, { "HoodWisp", "Linh Hồn Trùm Mũ" }, { "JellySwordsman", "Kiếm Sĩ Thạch" }
            };
            foreach (UnitDefinition unit in catalog.Companions)
            {
                if (companionNames.TryGetValue(unit.Id, out string name)) unit.DisplayName = name;
                unit.Description = CompanionDescription(unit);
                EditorUtility.SetDirty(unit);
            }
            foreach (BondDefinition bond in catalog.Bonds)
            {
                switch (bond.Id)
                {
                    case "hearth": bond.Name = "Bếp Ấm"; break;
                    case "archive": bond.Name = "Thư Viện"; break;
                    case "clock": bond.Name = "Bộ Máy"; break;
                    case "frost": bond.Name = "Sương Giá"; break;
                }
                bond.ExtraBonuses = new[] { new BondThresholdBonus { Threshold = 4, Value = bond.Value }, new BondThresholdBonus { Threshold = 6, Value = bond.Value * 2f } };
            }
            var recipes = new List<AltarRecipe>(catalog.Recipes);
            AddRecipe(recipes, "frost-post", "MailboxImp", "HoodWisp", "CandleSnail", 4);
            AddRecipe(recipes, "clock-tea", "MushroomLibrarian", "JellySwordsman", "TeapotPilgrim", 4);
            catalog.Recipes = recipes.ToArray();
            LocalizeItems(catalog);
            foreach (WaveModifier wave in catalog.WaveModifiers)
                wave.Name = wave.Every == 7 ? "Bão đồng hồ: địch đông hơn" : wave.Every == 5 ? "Kho bạc thất lạc: nhiều vàng hơn" : "Kệ sách yên tĩnh: hồi máu nhẹ";
            catalog.ContentRevision = 1;
            RunModifierRules.Validate(catalog);
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log("ARCANE_K8_CONTENT_APPLIED");
        }

        private static StatModifier Modifier(ItemEffect effect, float value, ModifierScope? scope = null)
            => new StatModifier { Effect = effect, Value = value, Scope = scope ?? RunModifierRules.DefaultScope(effect) };

        private static void SetChampion(RunCatalog catalog, string id, string name, StatModifier[] modifiers, string description)
        {
            UnitDefinition champion = Array.Find(catalog.Champions, unit => unit.Id == id);
            if (champion == null) throw new InvalidOperationException("K8: Missing champion " + id);
            champion.DisplayName = name;
            champion.ChampionModifiers = modifiers;
            champion.Description = description;
            EditorUtility.SetDirty(champion);
        }

        private static string CompanionDescription(UnitDefinition unit)
        {
            string passive;
            switch (unit.Passive)
            {
                case PassiveKind.HealChampion: passive = "Hồi máu thủ lĩnh mỗi 4 giây theo sức mạnh nội tại."; break;
                case PassiveKind.Scavenge: passive = "Tìm vàng mỗi 4 giây theo sức mạnh nội tại."; break;
                case PassiveKind.ChillAura: passive = "Làm chậm địch gần mình mỗi 4 giây; thời gian chậm ngắn hơn với trùm."; break;
                case PassiveKind.Rally: passive = "Sát thương riêng tăng theo số đồng minh và sức mạnh nội tại."; break;
                default: passive = "Tự động tìm và đánh địch bằng vũ khí được nhận ngẫu nhiên."; break;
            }
            return passive + " Cấp 2/3: đòn bắn thường thêm 1/2 đạn và 1/2 lượt xuyên; vũ khí cận chiến tăng tầm 10/20% và chia chu kỳ cho 1,12/1,24, đòn quét thường mở rộng cung đánh. Đòn riêng như sóng chuông và vùng báo thức giữ quy tắc gốc, không tự nhân số hiệu ứng; sát thương và nội tại vẫn nhân theo cấp.";
        }

        private static void AddRecipe(List<AltarRecipe> recipes, string id, string first, string second, string result, int cost)
        {
            if (!recipes.Exists(recipe => recipe.Id == id)) recipes.Add(new AltarRecipe { Id = id, FirstUnit = first, SecondUnit = second, ResultUnit = result, EggCost = cost, ResultTier = 2 });
        }

        private static void LocalizeItems(RunCatalog catalog)
        {
            string[] ids = { "key-oil", "clock-spring", "honey-pot", "felt-boots", "coin-compass", "wandering-map", "guardian-canopy", "warm-tea", "oracle-lens", "twin-seal", "piggy-mail", "pillow-mail", "royal-key", "brigade-banner", "marching-bell", "coupon-book", "long-atlas", "starlight-quilt", "echo-postage", "tarnished-crown", "fortune-egg", "treasure-whisper", "coin-salve", "chest-thunder", "altar-ember", "harvest-receipt", "reflective-ink", "carbon-ribbon", "office-lens", "tea-coaster", "scratch-record", "treasure-ribbon", "patch-spring", "thermal-ink", "repair-coupon" };
            string[] names = { "Dầu Chìa Khóa", "Lò Xo Đồng Hồ", "Hũ Mật Ong", "Ủng Nỉ", "La Bàn Tiền", "Bản Đồ Lang Thang", "Tán Ô Hộ Vệ", "Trà Ấm", "Kính Tiên Tri", "Ấn Song Sinh", "Heo Thư Tiết Kiệm", "Thư Gối", "Chìa Khóa Hoàng Gia", "Cờ Lữ Đoàn", "Chuông Hành Quân", "Sổ Phiếu Giảm Giá", "Địa Đồ Dài", "Chăn Ánh Sao", "Tem Vọng Âm", "Vương Miện Xỉn", "Trứng May Mắn", "Lời Thì Thầm Kho Báu", "Thuốc Mỡ Đồng Xu", "Sấm Rương", "Than Hồng Tế Đàn", "Biên Lai Mùa Gặt", "Mực Phản Chiếu", "Ruy Băng Than", "Kính Văn Phòng", "Đế Lót Trà", "Đĩa Nhạc Xước", "Ruy Băng Kho Báu", "Lò Xo Vá", "Mực Nhiệt", "Phiếu Sửa Chữa" };
            var special = new Dictionary<string, string>
            {
                { "guardian-canopy", "Đồng minh chặn đạn địch. Cộng dồn không thêm lượt chặn." },
                { "coupon-book", "Giảm giá hàng bằng vàng ở lần tạo cửa hàng tiếp theo, tổng giảm tối đa 50%; không giảm phí đổi hàng." },
                { "piggy-mail", "Tăng giới hạn tiền lãi; lãi thực nhận còn phụ thuộc số vàng đang giữ." },
                { "pillow-mail", "Sát thương nhận chia cho 1 + giáp." },
                { "fortune-egg", "May mắn tăng cơ hội rơi trứng và tim; không tăng sát thương." },
                { "treasure-whisper", "Rương xuất hiện sớm hơn; vẫn phải đến gần để mở." },
                { "coin-salve", "Nhặt tiền: hồi 0,5 máu mỗi cộng dồn, một lần mỗi sự kiện gốc, hồi chiêu 0,2 giây." },
                { "chest-thunder", "Mở rương: xung sát thương 14 mỗi cộng dồn, bán kính 3, hồi chiêu 1 giây. Không kích hoạt hiệu ứng vật phẩm khác." },
                { "harvest-receipt", "Nhặt tiền tiến một nhịp cho chuông trong bán kính 6; cộng dồn thứ hai cho hai nhịp. Hồi chiêu 0,35 giây; không sinh tiền mới, không kích hoạt lặp từ hiệu ứng phụ." },
                { "reflective-ink", "Con dấu thêm một dấu mỗi đòn; thời hạn dấu nhân 0,65. Bùng dấu mở điểm yếu 1,8 giây; không trực tiếp tăng sát thương bùng dấu." },
                { "carbon-ribbon", "Sóng chuông mở điểm yếu 1,5 giây; bùng dấu kéo tiền trong bán kính 2,5. Không sao chép sát thương, hạ địch hoặc vật phẩm rơi; không tự lan truyền." },
                { "office-lens", "Bùng dấu mở điểm yếu 1,8 giây: chí mạng trên địch lộ điểm yếu thêm 30% sát thương, tăng theo cộng dồn tới 50%. Chí mạng không tạo bùng dấu mới." },
                { "tea-coaster", "Vùng trà rộng thêm 40%, sát thương vùng giảm 20%. Địch cháy cho phép đòn búa thứ ba tạo hơi nước; không cần đủ bộ vật phẩm." },
                { "scratch-record", "Mỗi nốt hộp nhạc thứ ba vọng lại một lần sau 0,3 giây, sát thương 35%/70%. Sóng chuông chính gây thêm 25%/50% sát thương lên địch còn dấu; không lặp vô hạn." },
                { "treasure-ribbon", "Mở rương hồi 3 máu và tăng sát thương cung 15% mỗi cộng dồn trong 4 giây, tối đa 45%." },
                { "patch-spring", "Tự nạp một nhịp cho vật dụng đấu trường mỗi 3 giây; hiệu ứng phụ không tự nạp lặp." },
                { "thermal-ink", "Nhiệt trà/bàn là trên địch có dấu tăng sát thương vùng 40%; hồi chiêu trên từng mục tiêu 0,9 giây. Không thêm dấu." },
                { "repair-coupon", "Vật dụng đấu trường hạ địch hồi 1/2/3 máu, hồi chiêu chung 0,6 giây. Không tạo vòng lặp tiền hoặc hiệu ứng vật phẩm." }
            };
            foreach (ItemDefinition item in catalog.Items)
            {
                int index = Array.IndexOf(ids, item.Id);
                if (index < 0) continue;
                item.Name = names[index];
                item.Description = RunModifierRules.Describe(item, 1).TrimEnd();
                if (special.TryGetValue(item.Id, out string detail)) item.Description += "\n" + detail;
                if (Array.Exists(item.EffectiveModifiers, modifier => modifier.Effect == ItemEffect.Health)) item.Description += "\nKhi mua, máu hiện tại tăng theo phần thay đổi máu tối đa.";
            }
        }
    }
}
