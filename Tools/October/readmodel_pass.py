from pathlib import Path
p=Path('Assets/ArcaneBrigade/Runtime/UI/RunBuildReadModel.cs'); s=p.read_text(encoding='utf-8-sig')
replacements={
'Mathf.Max(1f, champion.Health + session.Bonus(ItemEffect.Health)).ToString("0.0")':'compiled.Champion.Health.ToString("0.0")',
'(weapon.Cooldown / Mathf.Max(0.1f, 1f + session.Bonus(ItemEffect.AttackSpeed, ModifierScope.Champion))).ToString("0.00")':'compiled.Champion.Weapon.Cooldown.ToString("0.00")',
'(champion.Speed * Mathf.Max(0.1f, 1f + session.Bonus(ItemEffect.MoveSpeed, ModifierScope.Champion))).ToString("0.00")':'compiled.Champion.Speed.ToString("0.00")',
'Percent(Mathf.Clamp01(champion.CriticalChance + session.Bonus(ItemEffect.Crit, ModifierScope.Champion)))':'Percent(compiled.Champion.CriticalChance)',
'(weapon.ProjectileCount + Mathf.FloorToInt(session.Bonus(ItemEffect.Projectiles, ModifierScope.Champion))).ToString()':'compiled.Champion.Weapon.ProjectileCount.ToString()',
'session.Bonus(ItemEffect.Armor).ToString("0.00")':'compiled.Armor.ToString("0.00")',
'(session.Bonus(ItemEffect.Heal) + (session.ActiveModifier == null ? 0f : session.ActiveModifier.Healing)).ToString("0.00")':'compiled.PassiveHeal.ToString("0.00")',
'(catalog.Arena.PickupRadius + session.Bonus(ItemEffect.Pickup)).ToString("0.00")':'compiled.PickupRadius.ToString("0.00")',
'profile.Achievements.Count * 40':'profile.Achievements.FindAll(a => !a.StartsWith("journal-")).Count * 40'
}
for a,b in replacements.items():
 if a in s:s=s.replace(a,b)
p.write_text(s,encoding='utf-8')
p=Path('Assets/ArcaneBrigade/Runtime/UI/RunMenu.cs');s=p.read_text(encoding='utf-8-sig').replace('profile.Achievements.Count * 40','profile.Achievements.FindAll(a => !a.StartsWith("journal-")).Count * 40');p.write_text(s,encoding='utf-8')
