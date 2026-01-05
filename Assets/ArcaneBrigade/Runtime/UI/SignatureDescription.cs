namespace ArcaneBrigade
{
    public static class SignatureDescription
    {
        public static string Describe(WeaponDefinition weapon, int tier, bool reflectiveInk = false)
        {
            if (weapon.Family != ExpandedWeapon.None) return ExpandedDescription.Describe(weapon, tier);
            if (weapon.Signature == SignatureWeapon.None) return "";
            string details = weapon.Signature == SignatureWeapon.Stamp
                ? "Một mục tiêu: " + weapon.SignatureBeats + " dấu cùng nguồn → burst " + (weapon.SignatureScale * 100).ToString("0") + "% damage. Mỗi hit " + (reflectiveInk ? 2 : 1) + " dấu; hết sau " + UnityEngine.Mathf.Max(0.1f, weapon.MarkLifetime * (reflectiveInk ? 0.65f : 1)).ToString("0.##") + "s không đánh."
                : "Mỗi " + weapon.SignatureBeats + " đòn → " + (weapon.Signature == SignatureWeapon.Bell ? "sóng tròn" : "bung ô phía trước") + " sau " + weapon.SignatureDelay.ToString("0.##") + "s, " + (weapon.SignatureScale * 100).ToString("0") + "% damage; radius " + weapon.SignatureRadius.ToString("0.##") + ".";
            if (tier >= 2) details += weapon.Signature == SignatureWeapon.Stamp ? "\nTier: truyền tối đa một dấu sang một mục tiêu gần; không tự burst tiếp." : weapon.Signature == SignatureWeapon.Bell ? "\nTier: một vọng yếu 35% sau 0.3s; không tạo vọng từ vọng." : "\nTier: radius bung ô ×1.25, không cấp chặn đạn.";
            return details + "\nPayoff không kích hoạt proc; nhịp/dấu reset ở ranh giới hall.";
        }
    }
}
