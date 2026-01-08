using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public static class RunUICopy
    {
        public static string UnitName(UnitDefinition unit) => unit == null ? "Không rõ nhân vật" : UnitName(unit.Id, unit.DisplayName);
        public static string UnitName(string id, string fallback = null)
        {
            if (!string.IsNullOrEmpty(fallback) && fallback != id) return fallback;
            switch (id)
            {
                case "LanternKnight": return "Hiệp sĩ đèn lồng";
                case "Postmaster": return "Trưởng trạm bưu điện";
                case "DrowsySeer": return "Nhà tiên tri ngái ngủ";
                case "TeapotPilgrim": return "Lữ khách ấm trà";
                case "CandleSnail": return "Ốc sên nến";
                case "MailboxImp": return "Tiểu quỷ hòm thư";
                case "PillowOracle": return "Nhà tiên tri gối";
                case "MushroomLibrarian": return "Thủ thư nấm";
                case "HoodWisp": return "Đốm sáng trùm mũ";
                case "JellySwordsman": return "Kiếm sĩ thạch";
                default: return fallback ?? id;
            }
        }
        public static string WeaponName(string id)
        {
            switch (id)
            {
                case "KeyCleaver": return "Kiếm chìa khóa";
                case "RuneStaff": return "Trượng cổ ngữ";
                case "UmbrellaLance": return "Thương ô";
                case "MailboxLauncher": return "Ống phóng bưu kiện";
                case "BellFork": return "Chuông âm thoa";
                case "DreamStamp": return "Con dấu giấc mơ";
                case "TeapotPopgun": return "Súng ấm trà";
                case "CrescentBow": return "Cung trăng khuyết";
                case "CloudScissors": return "Kéo mây";
                case "SleepCamera": return "Máy ảnh ru ngủ";
                case "TinFish": return "Cá thiếc";
                case "DreamIron": return "Bàn ủi giấc mơ";
                case "RubberHammer": return "Búa cao su";
                case "AlarmMortar": return "Pháo báo thức";
                case "MusicBox": return "Hộp nhạc";
                default: return string.IsNullOrEmpty(id) ? "Không rõ vũ khí" : id;
            }
        }
        public static string WeaponKindName(WeaponKind kind) => kind == WeaponKind.Projectile ? "Đánh xa" : kind == WeaponKind.Melee ? "Cận chiến" : "Va chạm";
        public static string ElementName(DamageElement element) => element == DamageElement.Ember ? "Lửa" : element == DamageElement.Frost ? "Băng" : element == DamageElement.Arcane ? "Phép thuật" : "Vật lý";
        public static string UnlockCondition(UnitDefinition unit, int bestWave) => bestWave >= unit.UnlockWave ? "Đã mở khóa" : "Đạt màn " + unit.UnlockWave + " để mở · hiện tại " + bestWave;
        public static string ChampionDescription(UnitDefinition unit)
        {
            if (unit.ChampionModifiers != null && unit.ChampionModifiers.Length > 0) return unit.Description;
            switch (unit.Id)
            {
                case "LanternKnight": return "Tiên phong cận chiến bằng kiếm chìa khóa. Sát thương tăng theo số đồng minh trong đoàn; hợp với đội quân đông.";
                case "Postmaster": return "Bắn bưu kiện từ xa và tự kiếm thêm vàng khi chiến đấu. Hợp với người thích tích tiền để mua sắm.";
                case "DrowsySeer": return "Đánh bằng chuông âm thoa và tự hồi máu định kỳ. Di chuyển chậm hơn, nhưng có nhiều máu để trụ lâu.";
                default: return WeaponKindName(unit.Weapon.Kind) + " · " + PassiveDescription(unit.Passive) + "\n" + unit.Description;
            }
        }
        public static string PassiveDescription(PassiveKind passive)
        {
            switch (passive)
            {
                case PassiveKind.Rally: return "Sát thương tăng theo số đồng minh";
                case PassiveKind.HealChampion: return "Tự hồi máu cho người dẫn đoàn mỗi 4 giây";
                case PassiveKind.Scavenge: return "Tự kiếm thêm vàng mỗi 4 giây khi chiến đấu";
                case PassiveKind.ChillAura: return "Làm chậm kẻ địch ở gần";
                default: return "Không có nội tại riêng";
            }
        }
        public static string BondDescription(BondDefinition bond, int count)
        {
            int threshold = Mathf.Max(1, bond.Threshold);
            int level = bond.ActiveLevel(count);
            string result = bond.Name + " · " + (level > 0 ? "Đã kích hoạt ×" + level : "Chưa kích hoạt")
                + "\n" + count + "/" + bond.NextThreshold(count) + " đồng minh · mỗi " + threshold + " người thêm một bậc"
                + "\n" + RunBuildReadModel.EffectName(bond.Effect) + ": " + bond.Reward(count).ToString("+0.###;-0.###;0");
            foreach (BondThresholdBonus bonus in bond.ExtraBonuses ?? new BondThresholdBonus[0])
                result += "\nMốc " + bonus.Threshold + " người: thêm " + bonus.Value.ToString("+0.###;-0.###;0") + (count >= bonus.Threshold ? " · đã đạt" : " · chưa đạt");
            return result + "\nMỗi đồng minh tính một người, không nhân theo cấp.";
        }
        public static string PhaseName(SessionPhase phase) => phase == SessionPhase.Combat ? "Đang chiến đấu" : phase == SessionPhase.Shop ? "Đang nghỉ và mua sắm" : phase == SessionPhase.Victory ? "Đã chiến thắng" : "Đã kết thúc";
        public static string TriggerName(ProcTrigger trigger) => trigger == ProcTrigger.CoinCollected ? "nhặt vàng" : trigger == ProcTrigger.ChestOpened ? "mở rương" : "hạ gục kẻ địch";
        public static string ActionName(ProcAction action) => action == ProcAction.HealChampion ? "hồi máu người dẫn đoàn" : "gây sát thương vùng";
        public static string RecordName(string key, RunCatalog catalog)
        {
            string[] parts = (key ?? "").Split('/');
            if (parts.Length < 3 || !int.TryParse(parts[1], out int difficulty)) return key;
            return UnitName(parts[0], catalog.Unit(parts[0])?.DisplayName) + " · " + RunMeta.DifficultyNames[Mathf.Clamp(difficulty, 0, 4)] + " · " + (parts[2] == "endless" ? "Vô tận" : "Chuyến đi") + (parts.Length > 3 && !string.IsNullOrEmpty(parts[3]) ? " · " + AchievementName("challenge-" + parts[3]) : "");
        }
        public static string AchievementName(string id)
        {
            if (id == "Brigade dawn") return "Đưa đoàn tới bình minh";
            if (id == "challenge-night-shift") return "Hoàn thành Ca đêm";
            if (id == "challenge-first-light") return "Hoàn thành Ánh sáng đầu";
            if (id == "challenge-small-brigade") return "Hoàn thành Đội nhỏ";
            const string prefix = "clear-difficulty-";
            if (id != null && id.StartsWith(prefix) && int.TryParse(id.Substring(prefix.Length), out int difficulty)) return "Thắng bậc " + RunMeta.DifficultyNames[Mathf.Clamp(difficulty, 0, 4)];
            return id;
        }
        public static string ContributionName(string id, RunCatalog catalog)
        {
            if (string.IsNullOrEmpty(id)) return "Đòn đánh chính";
            ItemDefinition item = catalog.Item(id);
            if (item != null) return item.Name;
            switch (id)
            {
                case "primary": return "Đòn đánh chính";
                case "unit-passive": return "Nội tại hồi máu";
                case "passive-regen": return "Hồi máu thụ động";
                case "scavenge": return "Nhặt vàng tự động";
                case "element-reaction": return "Phản ứng nguyên tố";
                case "stamp-burst": return "Bùng nổ con dấu";
                case "stamp-carbon": return "Truyền dấu";
                case "carbon-exposure": return "Làm suy yếu";
                case "projectile-bounce": return "Đạn bật tường";
                case "projectile-ricochet": return "Đạn nảy mục tiêu";
                case "projectile-split": return "Đạn phân tách";
                case "zipper-link": return "Đường tên nối";
                case "heat-status": return "Thiêu đốt";
                default: return "Hiệu ứng " + id;
            }
        }
        public static string DamageKind(string kind) => (kind ?? "").Replace("Physical", "Vật lý").Replace("Ember", "Lửa").Replace("Frost", "Băng").Replace("Arcane", "Phép thuật").Replace("Projectile", "Đạn").Replace("Melee", "Cận chiến").Replace("Contact", "Va chạm").Replace("arena", "đấu trường");
        public static string DamageSource(string source, RunCatalog catalog)
        {
            foreach (UnitDefinition enemy in catalog.Enemies)
                if (source == enemy.DisplayName || source == enemy.DisplayName + " contact") return UnitName(enemy) + (source.EndsWith(" contact") ? " · va chạm" : "");
            foreach (UnitDefinition boss in catalog.Bosses)
                if (source == boss.DisplayName || source == boss.DisplayName + " contact") return UnitName(boss) + (source.EndsWith(" contact") ? " · va chạm" : "");
            return source == "Enemy damage" ? "Đòn đánh kẻ địch" : source;
        }
    }

    public enum RunUIDevice { KeyboardMouse, Controller, Touch }
    public static class RunUIHints
    {
        private static int frame = -1;
        private static Vector2 previousPad;
        private static Vector3 previousMouse;
        public static RunUIDevice Device { get; private set; }
        public static void Refresh()
        {
            if (frame == Time.frameCount) return;
            frame = Time.frameCount;
            Vector2 pad = ArcadeNavigation.Pad;
            if (Input.anyKeyDown || (Input.mousePosition - previousMouse).sqrMagnitude > 4) Device = RunUIDevice.KeyboardMouse;
            bool padPressed = false;
            for (int index = 0; index < 20; index++) padPressed |= Input.GetKeyDown((KeyCode)((int)KeyCode.JoystickButton0 + index));
            if (padPressed || pad.sqrMagnitude > 0.16f && (pad - previousPad).sqrMagnitude > 0.01f) Device = RunUIDevice.Controller;
            if (Input.touchCount > 0 || Application.isMobilePlatform) Device = RunUIDevice.Touch;
            previousPad = pad; previousMouse = Input.mousePosition;
        }
        public static string Details(RunUIDevice device) => device == RunUIDevice.Controller ? "Đội quân · nút Y / nút 3" : device == RunUIDevice.Touch ? "Chạm để xem đội quân" : "Đội quân · phím I";
        public static string Pause(RunUIDevice device) => device == RunUIDevice.Controller ? "Tạm dừng / tiếp tục · Start hoặc B" : device == RunUIDevice.Touch ? "Chạm để tạm dừng / tiếp tục" : "Tạm dừng / tiếp tục · Esc";
        public static string Interact(RunUIDevice device) => device == RunUIDevice.Controller ? "DÙNG · A" : device == RunUIDevice.Touch ? "DÙNG" : "DÙNG · Space";
        public static string Menu => Device == RunUIDevice.Controller ? "Cần điều khiển: chọn · A: xác nhận · B: quay lại" : Device == RunUIDevice.Touch ? "Chạm để chọn · kéo để cuộn" : "Mũi tên / Tab: chọn · Enter: xác nhận · Esc: quay lại";
        public static string DetailsNavigation => Device == RunUIDevice.Controller ? "LB/RB: mục · A/X: dòng tiếp/trước · Y/B: đóng" : Device == RunUIDevice.Touch ? "Chạm mục hoặc dòng · kéo để cuộn" : "Tab: đổi mục · ↑/↓: chọn dòng · I/Esc: đóng";
    }

    public sealed class RunBondFeedback
    {
        private string runId;
        private readonly Dictionary<string, float> rewards = new Dictionary<string, float>();
        private readonly Dictionary<string, float> flashes = new Dictionary<string, float>();
        public void Observe(string identity, BondDefinition[] bonds, Dictionary<string, int> counts, float now)
        {
            bool initial = runId != identity || rewards.Count == 0;
            if (runId != identity) { rewards.Clear(); flashes.Clear(); runId = identity; }
            foreach (BondDefinition bond in bonds)
            {
                float reward = bond.Reward(counts.TryGetValue(bond.Id, out int count) ? count : 0);
                if (!initial && rewards.TryGetValue(bond.Id, out float previous) && reward > previous) flashes[bond.Id] = now + 2;
                if (rewards.TryGetValue(bond.Id, out float before) && reward < before) flashes.Remove(bond.Id);
                rewards[bond.Id] = reward;
            }
        }
        public bool Flashing(string id, float now) => flashes.TryGetValue(id, out float until) && now < until;
    }
}
