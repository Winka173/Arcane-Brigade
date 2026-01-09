using System;
using UnityEngine;

namespace ArcaneBrigade
{
    public static class K11HeroRules
    {
        public static bool Eligible(UnitDefinition champion, UnitDefinition companion)
        {
            foreach (string theme in champion.RecruitThemes)
                if (Array.IndexOf(companion.Tags, theme) >= 0) return true;
            return false;
        }
        public static RosterEntry Starter(RunCatalog catalog, UnitDefinition champion)
        {
            string unit = champion.Id == "LanternKnight" ? "TeapotPilgrim" : champion.Id == "Postmaster" ? "BrassAcorn" : "HoodWisp";
            string weapon = champion.Id == "LanternKnight" ? "SpoonSabre" : champion.Id == "Postmaster" ? "InkPistol" : "SnowGlobeOrb";
            if (catalog.Unit(unit) == null || catalog.Weapon(weapon) == null || !Eligible(champion, catalog.Unit(unit))) throw new InvalidOperationException("Invalid themed starting brigade: " + champion.Id);
            return new RosterEntry { UnitId = unit, WeaponId = weapon };
        }
        public static string Describe(UnitDefinition champion) => Describe(champion.HeroMechanic);
        public static string Describe(HeroMechanic mechanic)
        {
            return mechanic switch
            {
                HeroMechanic.LanternStride => "BƯỚC ĐÈN: đi 12 đơn vị tích một xung cận chiến bán kính 2,4; gây 65% damage vũ khí, hồi tối thiểu 4 giây. Giữ một xung đến khi có quái gần; không tích khi shop/pause.",
                HeroMechanic.ParcelExpress => "CHUYỂN PHÁT: nhặt 6 vàng tích một loạt bưu kiện tự bắn tới quái; gây 70% damage vũ khí, hồi tối thiểu 4 giây. Không tính vàng từ nội tại/thu nhập.",
                HeroMechanic.DreamBreath => "NHỊP MƠ: đứng yên 2 giây làm chậm quái trong bán kính 3 và xóa tối đa 3 đạn địch gần; hồi 7 giây. Chờ nguy hiểm gần mới kích hoạt; di chuyển ngắt tích nhịp, không bất tử.",
                _ => ""
            };
        }
    }
}
