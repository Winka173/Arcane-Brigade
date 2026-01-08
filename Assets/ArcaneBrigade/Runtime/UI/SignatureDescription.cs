namespace ArcaneBrigade
{
    public static class SignatureDescription
    {
        public static string Describe(WeaponDefinition weapon, int tier, bool reflectiveInk = false)
        {
            if (weapon.Family != ExpandedWeapon.None) return ExpandedDescription.Describe(weapon, tier);
            if (weapon.Signature == SignatureWeapon.None) return weapon.Kind == WeaponKind.Projectile ? weapon.ProjectileCount + " đạn mỗi đòn · xuyên thêm " + weapon.ProjectilePierce + " mục tiêu khác nhau.\nMột viên đạn không đánh lặp cùng mục tiêu." : "";
            string details = weapon.Signature == SignatureWeapon.Stamp
                ? "Một mục tiêu: " + weapon.SignatureBeats + " dấu cùng nguồn → bùng nổ " + (weapon.SignatureScale * 100).ToString("0") + "% sát thương. Mỗi đòn " + (reflectiveInk ? 2 : 1) + " dấu; hết sau " + UnityEngine.Mathf.Max(0.1f, weapon.MarkLifetime * (reflectiveInk ? 0.65f : 1)).ToString("0.##") + " giây không đánh."
                : "Mỗi " + weapon.SignatureBeats + " đòn → " + (weapon.Signature == SignatureWeapon.Bell ? "sóng tròn" : "bung ô phía trước") + " sau " + weapon.SignatureDelay.ToString("0.##") + " giây, " + (weapon.SignatureScale * 100).ToString("0") + "% sát thương; bán kính " + weapon.SignatureRadius.ToString("0.##") + ".";
            if (tier >= 2) details += weapon.Signature == SignatureWeapon.Stamp ? "\nTừ cấp 2: truyền tối đa một dấu sang mục tiêu gần; không tự bùng nổ tiếp." : weapon.Signature == SignatureWeapon.Bell ? "\nTừ cấp 2: một tiếng vọng yếu 35% sau 0,3 giây; không tạo vọng từ vọng." : "\nTừ cấp 2: bán kính bung ô ×1,25, không thêm khả năng chặn đạn.";
            return details + "\nĐòn đặc biệt không kích hoạt hiệu ứng vật phẩm; nhịp và dấu bắt đầu lại mỗi màn.";
        }
    }
}
