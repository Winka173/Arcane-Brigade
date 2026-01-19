"""Generates Docs/Audio/Audio_Integration_Contract.md from cues.json, music.json, the weapon assets and the annotations below.
Run: python3 Tools/Audio/gen_contract.py   (re-run after changing cues.json or annotations; the table columns that come from the
manifest - bus, priority, gain, cooldown, voices, pitch, clip paths - can never drift from what ships)."""
import glob
import json
import os
import re

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
AUD = os.path.join(ROOT, 'Assets', 'ArcaneBrigade', 'Resources', 'Audio', 'Arcane')
cues = json.load(open(os.path.join(AUD, 'cues.json')))['cues']
pieces = json.load(open(os.path.join(AUD, 'Music', 'music.json')))['pieces']
BY = {c['id']: c for c in cues}

E = 'W-E'    # wired through CombatEvent: ArenaController.cs:292 -> CheckpointAudio.Play(CombatEvent) -> ArcaneCueLibrary.EventId
EV = 'ArenaController.FixedUpdate -> CheckpointAudio.PlayBatch -> ArcaneCueLibrary.EventId'

# id -> (sound, when, emitter, granularity, status)   status starts with W-E / W-I / W-M / W-S (wired) or P (not wired) / R (reserved)
A = {}


def a(cid, sound, when, who, gran, status):
    A[cid] = (sound, when, who, gran, status)


# ------------------------------------------------------------------------------------------------ combat: swings / hits
SW = 'Fire event (tick = Windup + weapon.Windup 0.15-0.24 s). Cú vung chạy NGAY sau tick này (K8Motion.Attack release), nên transient đặt tại +0 ms.'
for k, snd, motion in (('blade', 'xẹt gió băng 0.9-4.2 kHz, 0.17-0.24 s, hơi giấy', 'Slash'), ('spear', 'xẹt hẹp, sáng, ngắn 0.16 s', 'Thrust'),
                       ('hammer', 'thổi trầm 0.26-0.34 s + thân 90-150 Hz', 'Slam'), ('scissors', 'xẹt rất ngắn sáng 0.14 s', 'Scissors (khi không đi nhánh Family Scissors) - hiện chưa dùng, xem sig.scissors'),
                       ('umbrella', 'vải phần phật + xẹt 0.26 s', 'chưa gán: UmbrellaHook/UmbrellaLance dùng Slash/Thrust'), ('whip', 'roi gió cao 0.16 s', 'chưa gán')):
    used = k in ('blade', 'spear', 'hammer')
    a('cmb.swing.' + k, snd, SW, 'Champion / đồng minh, Motion ' + motion, 'một lần mỗi lượt đánh (một Fire cho cả vòng cung, không theo nạn nhân)',
      ('W-E: ' + EV) if used else 'R: có clip, EventId chưa trả id này (không có vũ khí cần)')
a('cmb.windup', 'tiếng lấy đà: xẹt ngược nhẹ + nốt trượt lên 0.18 s', 'Windup event = bắt đầu lấy đà; kéo dài đúng weapon.Windup (0.15-0.24 s)',
  'Champion mọi vũ khí; đồng minh chỉ Slam/Hammer; quái cận chiến phi-Contact', 'một lần mỗi lượt đánh', 'W-E: ' + EV + ' (đồng minh nhẹ hơn để không dày)')
a('cmb.miss', 'hơi gió rất nhẹ 0.1 s, thua swing -8 dB', 'cùng tick với Fire của Champion khi không có Hit nào cùng Cause.RootId trong bước mô phỏng',
  'chỉ Champion melee', 'một lần mỗi lượt đánh hụt', 'W-E (một phần): PlayBatch(events) tự phát hụt; ArenaController.cs:292 hiện gọi Play từng event nên CHƯA có hụt -> đổi vòng lặp sang PlayBatch')
MAT_DESC = {'paper': 'giấy khô, bẹp, sáng', 'metal': 'kim loại: thân 330 Hz + chuông 1.5 kHz', 'wax': 'sáp: đục, mềm', 'fabric': 'vải: bịch, ấm', 'glass': 'thủy tinh: tinh 700 Hz + chuông 2.6 kHz', 'wood': 'gỗ: cộc, 240 Hz + 620 Hz'}
for m in MAT_DESC:
    a('cmb.hit.' + m, MAT_DESC[m] + ', 0.2-0.3 s',
      'Hit event (ApplyDamage cùng tick Fire với melee; với đạn là tick đạn chạm). Hit.Faction = phe NẠN NHÂN',
      'quái có BodyMaterial=' + m + '; nghe theo nạn nhân', 'mỗi Hit nhưng 1 âm/Cause.RootId trong 90 ms (một vòng cung trúng 10 quái = 1 âm); đạn xuyên: mỗi nạn nhân cách nhau >=45 ms nhóm cmb.hit',
      'W-E: ' + EV + ' + IsDuplicateHit')
    a('enemy.die.' + m, 'tan vỡ theo chất liệu ' + m + ' + hơi bụi + nốt marimba thấp, 0.3-0.4 s', 'Kill event (Hit và Kill cùng tick; Kill phát sau Hit)',
      'quái thường chất liệu ' + m, 'một lần mỗi quái chết; nhóm enemy.die tối thiểu 70 ms giữa hai lần', 'W-E: ' + EV)
a('cmb.crit', 'cú trúng + chớp celesta 2.4-3.1 kHz, 0.45 s, thay (không cộng) âm hit', 'Hit event có Critical=true', 'mọi nạn nhân quái', 'mỗi Hit crit (không bị dedupe Cause.RootId)', 'W-E: ' + EV)
a('wpn.bow.draw', 'dây cung căng, 0.2-0.23 s', 'Windup event của vũ khí Motion Bow (đúng độ dài Windup)', 'Champion/đồng minh cung (ButtonBow, LeafCrossbow, AcornSling, CrescentBow)', 'một lần mỗi phát bắn',
  'W-E: ' + EV)
a('wpn.bow.release', 'dây bật pizzicato + xẹt tên, 0.35 s', 'Fire event (tên rời dây)', 'cung + TetherBow (CrescentBow)', 'một lần mỗi loạt tên (ProjectileCount>1 vẫn 1 âm)', 'W-E: ' + EV)
a('wpn.gun.pop', 'nút chai bật + gỗ cộc, 0.18 s', 'Fire event (viên đạn ra nòng)', 'Motion Recoil, ProjectileCount<3 và bán kính đạn <0.08 (InkPistol, MailboxLauncher, DartCompass, PlasmaTrident? xem bảng vũ khí)', 'một lần mỗi loạt', 'W-E: ' + EV)
a('wpn.launcher', 'bịch trầm 80 Hz + nút bật 0.28 s', 'Fire event', 'Motion Recoil, ProjectileCount>=3 hoặc ProjectileRadius>=0.08 (ShellCannon, FireExtinguisher, PlasmaTrident)', 'một lần mỗi loạt (3-6 viên vẫn 1 âm)', 'W-E: ' + EV)
a('wpn.cast.fire', 'phập phù lửa: nhiễu lọc + lách tách, 0.45 s', 'Fire event của Motion Cast, Element Ember', 'LanternStaff, TeapotPopgun (Family Tea dùng sig.tea) ...', 'một lần mỗi lần niệm', 'W-E: ' + EV)
a('wpn.cast.ice', 'lấp lánh celesta cao + rây băng, 0.45 s', 'Fire event của Motion Cast, Element Frost', 'SnowGlobeOrb', 'một lần mỗi lần niệm', 'W-E: ' + EV)
a('wpn.cast.arcane', 'arpeggio vibes + pad detune, 0.55-0.7 s', 'Fire event của Motion Cast, Element Arcane/Physical', 'BubbleWand, StarKazoo, DreamOrbStaff, MoonFeatherFan, RuneStaff, RibbonHarp(Music)', 'một lần mỗi lần niệm', 'W-E: ' + EV)
a('wpn.boomerang.fly', 'rít quay AM 18 Hz + gió, 0.6 s', 'Fire event của Family Fish (ném đi)', 'TinFish, CrescentBoomerang, ClockCrescent', 'một lần mỗi lần ném; KHÔNG lặp theo thời gian bay', 'W-E: ' + EV)
a('wpn.boomerang.return', 'rít tăng cao dần + bắt gỗ "cộp" 0.5 s', 'Projectile.Returning chuyển false->true (ExpandedCombat.cs:107 hết 0.7 s, K8CombatProjectiles.cs:56 sau khi chạm); cú bắt lúc đạn tới chủ',
  'chủ của boomerang (Champion/đồng minh)', 'một lần mỗi chiều về (mỗi projectile)', 'P: sim không phát event. Thêm CombatEventKind.ProjectileReturn tại ExpandedCombat.cs:107 và K8CombatProjectiles.cs:56; EventId: -> "wpn.boomerang.return"')
a('wpn.pierce', 'xẹt + pizz sáng 0.3 s', 'đạn xuyên qua nạn nhân thứ 2+ (K8CombatProjectiles.cs: previousVictims < ProjectilePierce, tick "return projectile continues")', 'LeafCrossbow, DartCompass, DreamOrbStaff, PlasmaTrident, MoonFeatherFan, RuneStaff', 'tối đa 1 lần mỗi projectile mỗi nạn nhân thứ 2+; nhóm wpn 60 ms',
  'P: thêm CombatEventKind.ProjectilePierce khi đạn tiếp tục sau Victims.Add (K8CombatProjectiles.cs, nhánh `previousVictims < Pierce`); hiện Hit của nạn nhân thứ 2 vẫn có tiếng hit')
a('wpn.split', '3 tiếng gỗ tách nhanh tăng cao 0.25 s', 'tick SplitCombatProjectile (K8CombatProjectiles.cs:53) - đạn vỡ thành mảnh', 'Tea (TeapotPopgun), SnowGlobeOrb (+bonus BulletSplit)', 'một lần mỗi lần vỡ (SplitOnImpact chỉ 1 lần mỗi đạn)',
  'P: thêm CombatEventKind.ProjectileSplit trong SplitCombatProjectile; Hit của mảnh đã được hạ -5 dB tự động (Cause "projectile-split")')
a('wpn.bounce', 'marimba nảy 0.3 s', 'đạn nảy tường (K8CombatProjectiles.cs:41) hoặc ricochet sang mục tiêu (:86)', 'TetherBow, Fish, BubbleWand(0.35 bounce), mọi vũ khí có BulletBounce', 'một lần mỗi lần nảy; tối đa 4 lần/giây toàn cục (cooldown 0.06 s, 2 voice)',
  'P: thêm CombatEventKind.ProjectileBounce tại 2 điểm trên; Hit sau nảy tự hạ -2 dB (Cause "projectile-bounce/ricochet")')
SIGW = 'Fire event (impact tick của lượt đánh)'
a('sig.bell', 'chuông 440 Hz nhiều hài + đuôi 1.4 s', 'SignaturePulse event (mỗi nhịp chuông; Weapon.Signature=Bell) hoặc Fire của Motion Bell', 'BellFork (Champion DrowsySeer) và đồng minh Bell', 'một lần mỗi nhịp sóng (không theo nạn nhân); cooldown 0.25 s', 'W-E: ' + EV)
a('sig.umbrella', 'ô bật vải + pop, 0.3 s', 'SignaturePulse/SignatureBurst của Signature Umbrella', 'UmbrellaLance', 'một lần mỗi xung', 'W-E: ' + EV)
a('sig.stamp', 'đập dấu: thụp 95 Hz + giấy + cọ gỗ, 0.3 s', 'SignatureBurst (con dấu nổ, SignatureCombat.cs:87) hoặc Fire Motion Stamp (DreamStamp)', 'DreamStamp, StampHammer (Slam không dùng)', 'một lần mỗi lần đóng dấu', 'W-E: ' + EV)
a('sig.scissors', 'hai tiếng "cắt" kim loại 0.25 s', 'Fire của Motion Scissors (CloudScissors, TwinSabres); hai Strike field tại +0 và +0.12 s nên âm nằm trong 0.25 s', 'CloudScissors, TwinSabres', 'một lần mỗi lượt', 'W-E: ' + EV)
a('sig.camera', 'tua nạp + màn trập 2 lần, 0.32 s', 'Fire của Motion Camera (tia chụp tại Fire; Photo field kéo dài 0.22 s)', 'SleepCamera', 'một lần mỗi lượt chụp (tối đa 3-4 mục tiêu vẫn 1 âm)', 'W-E: ' + EV)
a('sig.musicbox', '3 nốt hộp nhạc đi lên 0.8 s', 'Fire của Motion Music (MusicBox)', 'MusicBox', 'một lần mỗi viên; nốt cuối (FinalNote) không có cue riêng', 'W-E: ' + EV)
a('sig.tea', 'bắn nước: nhiễu + giọt 0.3 s', 'Fire của Family Tea', 'TeapotPopgun', 'một lần mỗi viên; vũng trà tick = Hit "expanded-tea" (nghe như hit thường)', 'W-E: ' + EV)
a('sig.fish', 'vỗ cá: vải + ướt 0.3 s', '(dành cho cú vỗ cá khi Hit của đạn Fish)', 'TinFish', '-', 'R: có clip nhưng KHÔNG được dùng (Family Fish nay đều là boomerang -> wpn.boomerang.fly)')
a('sig.alarm', 'chuông báo 6 tiếng gỗ-chuông 0.4 s', 'Fire của Family Alarm = thả đạn cối (fuse 0.9 s)', 'AlarmMortar', 'một lần mỗi quả', 'W-E: ' + EV)
a('sig.iron', 'ép kim loại + hơi 0.4 s', 'Fire của Motion Iron', 'DreamIron', 'một lần mỗi lượt (vệt Iron 1.4 s không phát tiếp)', 'W-E: ' + EV)
a('sig.hammer', 'bịch gỗ nặng 0.55 s', 'Fire của Motion Hammer (RubberHammer); Echo field 0.1 s sau không có cue riêng', 'RubberHammer', 'một lần mỗi lượt', 'W-E: ' + EV)
a('sig.alarm.boom', 'nổ cối: thụp 70 Hz + bụi + chuông, 0.5 s', 'Hit event có Cause.ItemId="expanded-fuse" = fuse nổ (0.9 s SAU Fire)', 'AlarmMortar', 'một lần mỗi quả (dedupe Cause.RootId 90 ms)', 'W-E: ' + EV)
a('fx.field.zipper', 'zip tăng cao + pizz 0.3 s', 'Hit event Cause.ItemId="zipper-link"/"expanded-link" (dây TetherBow nối nạn nhân kế)', 'CrescentBow', 'mỗi mắt xích (tối đa 2-3/lần bắn), cooldown 0.1 s', 'W-E: ' + EV)
a('fx.burn', 'lách tách lửa 0.12 s', 'Hit event Cause.ItemId="ember-status" (Burn tick mỗi 0.5 s)', 'quái đang cháy', 'mỗi tick nhưng cooldown 0.12 s/2 voice, bị bỏ đầu tiên khi đông (priority 200, Reduced Audio bỏ hẳn)', 'W-E: ' + EV)
a('fx.chill', 'chớp celesta băng 0.25 s', '(áp chill) không có event: ChillUntil gán im lặng trong ApplyWeaponDamage', 'quái bị lạnh', '-', 'P: cần CombatEventKind.StatusApplied (Ember/Frost) tại CombatSimulation.cs ApplyWeaponDamage nhánh Frost; clip có sẵn')
a('fx.knockback', 'xẹt lùi + bịch 0.22 s', 'đẩy lùi trong ApplyDamage (CombatSimulation.cs:720, PushDistance*(1-KnockbackResistance))', 'quái bị đẩy bởi vũ khí Slam/Hammer (PushDistance 0.5-0.6)', 'chỉ khi PushDistance>=0.5 và nạn nhân sống; 1 âm/Cause.RootId',
  'P: thêm CombatEventKind.Knockback ngay sau dòng 720 (Position, Direction, Weapon); âm hit đã đủ cho vũ khí nhẹ')
a('fx.reaction.steam', 'hơi + celesta 0.5 s', 'Hit event Cause.ItemId="element-reaction" (Ember<->Frost, damage x0.5)', 'phản ứng Ember+Chill / Frost+Burn', 'một lần mỗi phản ứng; cooldown 0.2 s', 'W-E: ' + EV + ' (chưa phân biệt shatter vs steam vì sim không ghi hướng phản ứng)')
a('fx.reaction.shatter', 'tan băng: chùm celesta nhỏ 0.5 s', '(phản ứng Frost trên quái đang Burn)', 'như trên', '-', 'P: cần event ghi hướng phản ứng (element-reaction-frost) ở CombatSimulation.cs ApplyWeaponDamage; hiện luôn phát steam')
a('fx.reaction.arcane', 'flare vibes 4 nốt 0.5 s', 'Hit event Cause.ItemId="arcane-chain" (nổ lan 1.5 u, damage x0.35)', 'vũ khí Arcane', 'một lần mỗi quái lan (dedupe Cause.RootId 90 ms -> ~1 âm/lượt)', 'W-E: ' + EV)
# ------------------------------------------------------------------------------------------------ characters
a('chr.step', 'bước dạ/nỉ 0.09 s', 'cùng nhịp FootstepDust.Display (FootstepDust.cs:50): Champion 0.22 s/bước', 'CHỈ Champion', 'mỗi bụi bước của Champion; cooldown 0.16 s, priority 220 (bị bỏ đầu tiên)', 'P: gọi PlayWorldId("chr.step", unit.Position) khi unit.Faction==Champion tại FootstepDust.cs:50. Đồng minh KHÔNG có bước riêng.')
a('chr.dash', 'xẹt gió + bụi 0.3 s', 'đồng minh chuyển CompanionTravelMode Patrol->Pursuit (vệt tốc độ + bụi)', 'đồng minh truy đuổi, tối đa 2 con gần Champion nhất cùng lúc', 'một lần mỗi lần chuyển sang Pursuit; cooldown 0.2 s',
  'P: edge-detect unit.TravelMode trong FootstepDust.Display (đã có biến pursuit) -> PlayWorldId("chr.dash", pos)')
a('chr.jump', 'nhảy marimba + lên tone 0.22 s', 'UnitVisual.BeginArrival (UnitVisual.cs:34): cú bật lúc t=0 của vòng cung 0.405 s', 'đồng minh mới đáp xuống', 'một lần mỗi đồng minh mới', 'P: PlayCommittedArrival (ArenaController) -> visual.BeginArrival: gọi PlayWorldId("chr.jump", origin)')
a('chr.land', 'bịch + vải 0.2 s', '(Champion chưa có nhảy/đáp)', '-', '-', 'R: chưa có chuyển động đáp của Champion; dành cho sau. Đồng minh dùng up.ally.land')
a('plr.dodge', 'xẹt gió gấp 0.3 s', 'Dodge event (CombatSimulation.cs:694) tại thời điểm né', 'Champion (stat Dodge)', 'mỗi lần né, cooldown 0.2 s', 'W-E: ' + EV)
a('plr.hurt', 'cú bịch vải + clarinet rơi 0.35 s', 'Hit event có Faction=Champion (tick sát thương)', 'Champion bị trúng (chạm, đạn, hazard)', 'mỗi Hit, cooldown 0.15 s, KHÔNG dedupe, lane danger riêng', 'W-E: ' + EV + ' (danger: không bị gate cosmetic, duck nhạc 0.48 x 0.5 s)')
a('plr.lowhp', 'tim đập: kick + upright 0.5 s', 'cạnh lên HP<25% (ArenaController.cs:224-226)', 'Champion', 'một lần khi vào ngưỡng; nếu muốn nhịp tim lặp cần loop riêng (chưa có)', 'W-I: ArenaController.cs:226 PlayCue(25,true) -> LegacyId 25')
a('plr.heal', 'celesta đi lên 4 nốt 0.8 s', 'Heal event (CombatSimulation.cs:455/842, ArenaController hồi cuối wave)', 'Champion', 'một lần mỗi Heal (passive-regen không phát event); cooldown 0.25 s', 'W-E: ' + EV)
a('plr.death', 'clarinet tụt + gong + bịch 1.4 s', 'Kill event Faction=Champion; cùng lúc DeathFeedback.Play', 'Champion', 'một lần', 'W-E: ' + EV)
# ------------------------------------------------------------------------------------------------ enemies / bosses
a('enemy.spit.windup', 'ọc ọc tăng cao 0.3 s', 'Windup event của quái có vũ khí Projectile (bắt đầu khạc)', 'quái bắn: ClockBat, RuneWisp, SirenSeal (các quái còn lại dùng Contact)', 'một lần mỗi lượt; cooldown 0.1 s/2 voice', 'W-E: ' + EV + ' (danger lane)')
a('enemy.spit.fire', 'tách + gỗ 0.22 s', 'Fire event của quái Projectile (viên rời thân)', 'quái bắn', 'một lần mỗi loạt (không theo viên)', 'W-E: ' + EV + ' (danger lane)')
a('enemy.lunge', 'xẹt thấp + thân 110 Hz 0.3 s', 'quái Contact bắt đầu lao/đập: ProcessContact đặt ContactImpactAt=Elapsed+0.22 s (CombatSimulation.cs:645), đòn rơi sau 0.22 s', 'quái Contact (đa số quái thường)', 'một lần mỗi cú đập; cooldown 0.12 s/2 voice; lane danger',
  'P: quái Contact KHÔNG phát event nào (Fire/Windup chỉ có cho quái có Weapon.Kind != Contact). Thêm CombatEventKind.ContactWindup tại CombatSimulation.cs:645; EventId -> enemy.lunge. (EventId đã trả enemy.lunge cho Fire quái melee có vũ khí, hiện không có quái nào như vậy)')
a('enemy.hurt', 'hit theo chất liệu (bản rút gọn)', '(dự phòng)', '-', '-', 'R: có clip nhưng KHÔNG dùng - cmb.hit.<chất liệu> đã đảm nhiệm. Có thể xóa.')
a('enemy.big.step', 'bước nặng 60 Hz 0.35 s', 'bước của quái lớn (WardrobeCrab r=0.55, FurnaceToad r=0.48): chưa có nhịp bước cho quái', 'quái Radius>=0.45 không phải boss', 'tối đa 1 bước/0.3 s mỗi quái lớn gần nhất', 'P: cần nhịp bước riêng cho quái lớn (vị trí thay đổi trong MoveEnemy) - gameplay quyết định')
a('enemy.big.roar', 'gầm bass clarinet + saw 0.9 s', 'quái lớn bắt đầu truy đuổi/xuất hiện lần đầu trong wave', 'quái lớn', 'một lần mỗi quái lớn mỗi wave', 'P: spawn quái lớn trong SpawnWaveEnemies (CombatSimulation.cs ~353) - cần event Spawn')
a('enemy.die.large', 'sập nặng + khói + marimba 0.5 s', 'Kill event của quái Radius>=0.45', 'WardrobeCrab, FurnaceToad', 'một lần mỗi quái', 'W-E: ' + EV + ' (ngưỡng dựa Radius vì BodyScale của quái luôn =1)')
a('boss.enter', 'gong + 3 kick + saw + gầm; điểm rơi 1.0-1.2 s', 'khi boss spawn (UnitState.EntranceUntil=1.2 s; BossLanded event tại cuối); ArcadeMusic gọi tại cạnh lên boss', 'boss', 'một lần mỗi wave boss', 'W-M: ArcadeMusic.SetPhase cạnh lên `boss` -> PlayInterfaceId; điểm rơi khớp BossLanded (CombatSimulation.cs:312)')
a('boss.telegraph', 'tích tắc gỗ dâng + gió 0.5 s', 'Windup bare event (CombatSimulation.cs:504, TelegraphUntil=Elapsed+0.85 s) = bắt đầu các làn cảnh báo', 'boss', 'một lần mỗi đợt đạn (3.5 s/lần)', 'W-E: ' + EV + ' (bossPattern); TRÙNG với ArenaController.cs:223 PlayCue(24) - cooldown 0.3 s chặn đôi; có thể xóa dòng 223')
a('boss.attack', 'thịch kim loại + bước + vung 0.6 s', 'Fire bare event (CombatSimulation.cs:499) = đạn mẫu rời boss (0.85 s sau telegraph)', 'boss', 'một lần mỗi đợt (5-20 đạn vẫn 1 âm)', 'W-E: ' + EV)
a('boss.phase', 'gong + chuông + sweep 2.2 s', 'ProcessBossMotif (ExpandedEnemies.cs:39) khi boss <=50% HP, một lần mỗi boss', 'boss', 'một lần', 'P: sim không phát event. Thêm CombatEventKind.BossPhase tại ExpandedEnemies.cs:39; EventId -> "boss.phase"; hazard xuất hiện 1.5-1.8 s sau -> fx.hazard.warn')
a('boss.defeat', 'gong + marimba rơi + celesta 3.0 s', 'Kill event của boss', 'boss', 'một lần', 'W-E: ' + EV)
a('fx.hazard.warn', 'rung trầm + tích tắc tăng 0.6 s', 'ExpandedField Office/Toy/CurtainHazard được tạo (ProcessBossMotif, delay 1.5-1.8 s)', 'boss', 'một lần mỗi cụm hazard (3-4 vùng vẫn 1 âm)', 'P: cùng hook BossPhase/Field tạo hazard; danger lane')
a('fx.hazard.hit', 'đập kim loại + chuông 0.7 s', 'Hit event Cause.ItemId="expanded-officehazard|toyhazard|curtain" (hazard nổ)', 'Champion/quái trong vùng', 'một lần mỗi vùng nổ (dedupe 90 ms)', 'W-E: ' + EV)
a('enemy.mimic.wake', 'cọt kẹt + thở gấp 0.55 s', 'SleepMimic tỉnh dậy (MoveExpandedEnemy: Awakened=true, TelegraphUntil=+0.8 s; ExpandedEnemies.cs:12)', 'SleepMimic', 'một lần mỗi mimic', 'P: thêm event/hook tại ExpandedEnemies.cs:12-15')
a('hero.pulse.ember', 'phù lửa + gong thấp 1.0 s', 'SignaturePulse có Weapon==null, Faction Champion, Element Ember (K11HeroCombat: LanternStride, cooldown 4 s)', 'LanternKnight', 'một lần mỗi lần kích hoạt', 'W-E: ' + EV)
a('hero.pulse.frost', 'băng lấp lánh + pad 1.0 s', 'như trên, Element Frost (DreamBreath, cooldown 7 s)', 'DrowsySeer (HeroMechanic.DreamBreath)', 'một lần mỗi lần', 'W-E: ' + EV)
# ------------------------------------------------------------------------------------------------ loot
a('loot.coin.drop', 'tiếng xu "tinh" + hạt kim loại 0.4 s', 'DropLoot(Coin) khi quái chết (CombatSimulation.cs:806)', 'xu từ quái', 'tối đa 1 âm mỗi tick Kill (không mỗi xu); cooldown 0.04 s',
  'P: thêm CombatEventKind.LootDrop trong DropLoot (có Loot kind, Position); với rương 3 vật phẩm vẫn 1 âm')
a('loot.coin.bounce', 'nảy ngắn 0.2 s', 'xu nảy trong 0.26 s đầu (CollectPickups age<0.26)', 'xu', 'KHÔNG phát theo từng xu: chỉ 1 nảy đại diện cho mỗi đợt rơi; cooldown 0.05 s', 'P: cùng hook LootDrop (+0.1 s)')
a('loot.coin.vacuum', 'xoáy tăng cao nhỏ 0.14 s', 'pickup.Attracting chuyển true (CombatSimulation.cs:786)', 'xu bị hút', 'tối đa 1 lần mỗi 50 ms toàn cục, 2 voice', 'P: event PickupAttract hoặc phát khi Attracting false->true')
a('loot.coin.pickup', 'chuông xu 2 nốt 0.5 s', 'Pickup event Loot=Coin (nhặt xong, CombatSimulation.cs:799)', 'Champion', 'mỗi xu nhưng cooldown 0.035 s, 3 voice; coin dồn -> gate cosmetic + Reduced Audio bỏ bớt', 'W-E: ' + EV)
a('loot.egg', 'pop gỗ + marimba hai nốt 0.4 s', 'Pickup Loot=Egg', 'Champion', 'mỗi trứng', 'W-E: ' + EV)
a('loot.heart', 'vibes hai nốt + tim 0.9 s', 'Pickup Loot=Heart (kèm Heal event -> plr.heal cùng lúc, ưu tiên heart)', 'Champion', 'mỗi tim', 'W-E: ' + EV)
a('loot.chest.appear', 'nhảy + bịch gỗ + chuông 0.8 s', 'SpawnTreasure -> DropLoot(Chest) (CombatSimulation.cs:814)', 'rương', 'một lần mỗi rương (tối đa 4 rương)', 'P: event LootDrop với Loot==Chest')
a('loot.chest.open', 'cọt kẹt + chốt + celesta 5 nốt 1.2 s', 'ChestOpened event (CombatSimulation.cs:830)', 'Champion', 'một lần mỗi rương', 'W-E: ' + EV)
# ------------------------------------------------------------------------------------------------ upgrades
a('up.merge.gather', 'xoáy hội tụ tăng cao + tích tắc 0.54 s', 'MergeReveal Beat bắt đầu (MergeReveal.cs:99, age=0); kết thúc đúng lúc flash age=0.54', 'đồng minh bị ghép', 'một lần mỗi Beat', 'P: gọi PlayInterfaceId("up.merge.gather") ngay sau `active = queue.Dequeue()`')
a('up.merge', 'cú bịch gỗ + chuông + lấp lánh 0.8 s (CHỈ điểm va chạm)', 'MergeReveal age>=0.54 (MergeReveal.cs:151, hiện PlayInterface(23))', 'đồng minh/ghép altar', 'một lần mỗi Beat', 'W-I: MergeReveal.cs:151 -> LegacyId 23. Giữ; thêm up.star2/3 cùng tick theo Result.Tier')
a('up.star2', 'arpeggio celesta+vibes 4 nốt + chuông 1.8 s', 'cùng tick up.merge khi Beat.Result.Tier==2', 'đồng minh sao 2', 'một lần mỗi Beat', 'P: MergeReveal.cs:151, nếu active.Result.Tier==2')
a('up.star3', 'arpeggio 8 nốt + chuông cao 1.8 s', 'như trên khi Tier==3', 'đồng minh sao 3', 'một lần mỗi Beat', 'P: như trên')
a('up.ally.land', 'cung bay + chạm đất vải + marimba trầm 1.0 s (chạm đất tại 0.405 s)', 'UnitVisual.BeginArrival (UnitVisual.cs:34): gọi tại t=0, clip tự rơi đúng 0.405 s', 'đồng minh mới', 'một lần mỗi đồng minh mới; cooldown 0.12 s', 'P: PlayCommittedArrival -> visual.BeginArrival(origin)')
a('up.bond.t1', 'vibes+pizz hợp âm 3 nốt 1.4 s', 'sau Buy thành công: ActiveLevel của một BondDefinition tăng lên 1 (RecountBonds, RunSession.cs:220)', 'bond (Hearth/Archive/Clock/Frost/Orchard/Astral/Tidal/Oddity)', 'một lần mỗi lần tăng bậc', 'P: WorldShop.Interact sau BuyOffer: so BondCounts/ActiveLevel trước-sau (RunBondFeedback đã có Flashing)')
a('up.bond.t2', 'như t1 + nốt thứ 4', 'như trên khi bậc >=2', 'bond', 'như trên', 'P: như trên')
a('up.chain', 'chuỗi woodblock 4 tiếng + celesta 0.8 s', 'Proc event (item/relic proc: CombatSimulation.cs:862, SignatureRelics.cs:31)', 'item có ProcRule hoặc relic', 'mỗi Proc nhưng cooldown 0.2 s (Proc đã có cooldown riêng)', 'W-E: ' + EV)
for tier, who in (('blue', 'Uncommon (0.25,0.7,1.0)'), ('purple', 'Rare (0.75,0.6,1.0)'), ('gold', 'Mythic (1.0,0.76,0.35)')):
    a('up.rarity.' + tier, {'blue': 'hai nốt vibes sạch 1.0 s', 'purple': 'ba nốt vibes + trượt bí ẩn 1.4 s', 'gold': '6 nốt celesta + chuông + gong + lấp lánh 2.0 s'}[tier],
      'khi hàng độ hiếm này đáp xuống bệ (ShopRarityGlow tạo cùng lúc: WorldShop.cs:254) và khi mua', 'hàng ' + who, 'một lần mỗi hàng khi LANDS (stock đáp tại StockArrivalDelay 0.7 + index*0.12 + 0.55 s)', 'P: PlayInterfaceId(id, delay=0.7+0.12*index+0.55) tại WorldShop.cs:254')
# ------------------------------------------------------------------------------------------------ shop
a('shop.keeper.enter.gold', 'rơi từ trên (tone rơi) -> đáp 0.98 s -> bịch + chuông', 'WorldShop.Build merchantEntrance (WorldShop.cs:234-236); đáp tại MerchantArrivalDelay 0.18 + 0.8 s = 0.98 s', 'shop vàng (Special=false)', 'một lần mỗi lần Build có merchantEntrance', 'W-S: WorldShop.cs:236 PlayInterfaceId')
a('shop.keeper.enter.dream', 'trồi lên từ đất (sweep nở) -> 0.98 s + vibes', 'như trên, Special=true (người bán trồi, không rơi)', 'shop giấc mơ', 'như trên', 'W-S: WorldShop.cs:236')
a('shop.keeper.exit.gold', 'quét xuống + tone tụt 0.8 s', 'WorldShop.Depart (WorldShop.cs:275): người bán đi sau MerchantDepartureDelay 0.38 s, dài 0.8 s', 'shop vàng', 'một lần', 'P: PlayInterfaceId("shop.keeper.exit.gold", 0.38f) tại Depart')
a('shop.keeper.exit.dream', 'như trên, chìm xuống', 'như trên', 'shop giấc mơ', 'một lần', 'P: như trên (.dream)')
a('shop.carpet.open', 'vải trải 0.9 s + phịch cuối tại 0.84 s', 'MarketLifecycle.Configure entrance (carpet progress 0->1 trong 0.9 s)', 'cả hai shop', 'một lần mỗi lần Build có entrance', 'P: gọi cùng lúc keeper.enter (WorldShop.cs:236), delay 0')
a('shop.carpet.roll', 'vải cuộn 0.75 s + cạch gỗ tại 0.7 s', 'MarketLifecycle.Depart (MarketLifecycle.cs:38): CarpetDepartureDelay 0.55 s, dài 0.75 s', 'cả hai shop', 'một lần', 'P: PlayInterfaceId("shop.carpet.roll", 0.55f) tại Depart')
a('shop.stand.appear', 'bật lên + bịch gỗ + marimba 0.5 s', 'bệ/hàng đáp xuống: ShopDisplayActor.Arrive (delay StockArrivalDelay 0.7 + index*0.12) + 0.55 s', 'hàng ở 3 bệ', 'một lần mỗi hàng (tối đa 3) tại lúc đáp', 'P: PlayInterfaceId(id, 0.7+0.12*index+0.55) tại WorldShop.cs:249')
a('shop.item.pop', 'pop + marimba 0.35 s', 'vật phẩm bay từ người bán tới bệ (cùng lịch với stand.appear; dùng cho Item thay Companion)', 'hàng loại Item', 'một lần mỗi hàng', 'P: như stand.appear (chọn một trong hai theo Offer.Kind: Item->item.pop, Companion->stand.appear)')
a('shop.approach', 'vibes cao nhẹ 0.3 s', 'Nearby đổi sang một station mới (WorldShop.cs:111, Display)', 'Champion tới gần món', 'một lần mỗi lần Nearby đổi giá trị (không phát khi giữ nguyên)', 'P: WorldShop.cs:122 nhánh `station.Focus < 0.02f` (EnteredAt) đã là cạnh lên')
a('shop.focus', 'tick gỗ + đồng hồ 0.06 s', 'chuyển focus trực tiếp giữa hai station liền nhau', 'Champion', 'tối đa 1/0.06 s', 'P: cùng cạnh trên khi station trước không null')
a('shop.buy.gold', 'chuông quầy + xu + gỗ 0.9 s', 'khi RunTransaction.TryCommit thành công (ArenaController.cs:364) = cùng tick Reveal/ItemPickup bắt đầu', 'mua bằng vàng (shop vàng)', 'một lần mỗi giao dịch', 'W-I: ArenaController.Transaction cue=20 -> LegacyId 20. CẢNH BÁO: cue 20 cũng bị dùng ở BeginHallExit (ArenaController.cs:398) và FinishSpecialShop (:400) -> sai ngữ nghĩa, xem mục 8')
a('shop.buy.dream', 'vibes trượt lung linh 1.6 s', 'như trên khi offer.Currency là Eggs/Health (shop giấc mơ)', 'mua bằng trứng/HP', 'một lần mỗi giao dịch', 'P: Transaction cần biết Currency/Special; hoặc WorldShop.Interact sau BuyOffer thành công (WorldShop.cs:150)')
for cur, wh in (('gold', 'vàng'), ('egg', 'trứng'), ('hp', 'HP (phải còn >=1 HP)')):
    a('shop.deny.' + cur, {'gold': 'clarinet thấp + hai tiếng gỗ 0.35 s', 'egg': 'marimba trầm + hai tiếng gỗ 0.35 s', 'hp': 'kick mềm + hai tiếng gỗ 0.35 s'}[cur],
      'nhánh denied của WorldShop.Interact (WorldShop.cs:203, DeniedCount++), cùng lúc station.DeniedUntil rung đỏ 0.65 s', 'khi không đủ ' + wh, 'một lần mỗi lần bấm hỏng; cooldown 0.25 s',
      'P: WorldShop.cs:203: chọn id theo station.Currency (Gold/Eggs/Health); hiện chỉ có cue 22 chung cho mọi lỗi')
a('shop.invalid', 'tiếng gỗ đục 0.08 s', 'giao dịch lỗi chung (hết hàng, trần stack, save lỗi, thiếu nguyên liệu fusion)', 'mọi shop', 'một lần mỗi lần', 'W-I: ArenaController.cs:361 PlayInterface(22) cho MỌI thất bại (kể cả thiếu tiền) - nên thay bằng shop.deny.* khi biết nguyên nhân')
a('shop.reroll', '8 tiếng gỗ xáo + chuông 0.9 s', 'Transaction("Reroll...") thành công (ArenaController.cs:386-387)', 'shop vàng và altar', 'một lần mỗi lần reroll', 'W-I: cue 21 -> LegacyId 21 -> shop.reroll (trước đó cue 21 chưa ánh xạ và vẫn phát Kenney)')
a('shop.fusion', 'sweep hợp nhất + chuông + celesta 2.0 s', 'FuseAltar thành công (ArenaController.cs:388 cue 23)', 'altar (shop vàng wave>=4)', 'một lần mỗi fusion', 'W-I (nửa đúng): cue 23 hiện -> up.merge. Nên gọi PlayInterfaceId("shop.fusion") tại WorldShop.cs:183 và để up.merge/star lo phần Reveal')
a('shop.descend', 'quét xuống + gõ gỗ 5 nhịp 1.0 s', 'NEXT HALL: BeginHallExit (ArenaController.cs:392) + HallDepartureEffect', 'Champion rời shop', 'một lần mỗi lần', 'W-I (sai): hiện PlayInterface(20) = shop.buy.gold. Đổi thành PlayInterfaceId("shop.descend")')
# ------------------------------------------------------------------------------------------------ ui
UIW = {'hover': ('tick gỗ 0.04 s', 'focus/hover đổi sang một nút ENABLED khác (ArcadeNavigation: Move() cạnh selected đổi; chuột: lastHoverIdentity đổi, Repaint)', 'P: ArcadeNavigation.cs:33 Move() và :50 (hover) - KHÔNG phát từ OnGUI mỗi pass'),
       'select': ('tick gỗ + marimba 0.25 s', 'click/accept một nút (ArcadeNavigation.cs:70)', 'W-I: ArcadeNavigation.cs:70 PlayInterface(19) -> ui.select (đã phát cả khi bấm +/- ở biên không đổi giá trị)'),
       'confirm': ('hai nốt marimba đi lên 0.5 s', 'xác nhận hành động quan trọng (START, SAVE AND RETURN, PLAY AGAIN)', 'P: tại RunMenu.Launch / RunHud nút quan trọng'),
       'back': ('hai nốt marimba đi xuống 0.4 s', 'quay lại/hủy: BACK, CLOSE, STAY, Esc đóng', 'P: nút BACK (RunMenu.cs:235), CancelMenuReturn (ArenaController.cs:332)'),
       'open': ('sweep mở + celesta 0.4 s', 'mở menu/notebook: ToggleDetails open (ArenaController.cs:323), vào Page mới (RunMenu.ShowPage:76)', 'P'),
       'close': ('sweep đóng + gỗ 0.3 s', 'đóng notebook/menu', 'P'),
       'page': ('giấy lật 0.28 s', 'đổi trang/cuộn có đổi giá trị (Records/Museum selection đổi)', 'P'),
       'tooltip': ('celesta rất ngắn 0.12 s', 'tooltip bond/offer HIỆN mới (RunHud.cs:259 selectedBond đổi; :193 tooltipStation đổi)', 'P'),
       'tab': ('tick gỗ 0.05 s', 'đổi tab Details/Museum khi index MỚI khác tab cũ (RunDetails.SelectTab, RunMuseum.SelectTab)', 'P'),
       'pause': ('vibes 2 nốt đi xuống 0.7 s', 'SetPaused(true) do người chơi (ArenaController.cs:309; KHÔNG phát cho FocusLost/Backgrounded)', 'P'),
       'resume': ('vibes 3 nốt đi lên 0.7 s', 'SetPaused(false) do người chơi', 'P'),
       'setting': ('tick gỗ 0.04 s', 'slider âm lượng đổi >=0.005 (RunMenu.DrawVolume:407) - throttle 80 ms khi kéo; toggle (REDUCED..., HIGH-CONTRAST...)', 'P: phát SAU khi giá trị mới đã Apply để nghe ngay mức mới'),
       'unlock': ('gỗ + chuông 2 nốt 1.2 s', 'mở khóa (champion/độ khó/cosmetic) khi lần đầu hiện', 'P'),
       'achievement': ('celesta 5 nốt + chuông 1.8 s', 'PersistBoundary delta>0 (ArenaController.cs:348) - cạnh, không phải mỗi frame', 'P: phát một lần ở ResultsReady (MetaNotice chỉ hiển thị tại màn kết quả)'),
       'receipt': ('10 tick máy in + chuông 0.9 s', 'RunInvoice hiện mới (identity != run.RunId, RunInvoice.cs:53)', 'P'),
       'locked': ('hai tiếng gỗ trầm 0.2 s', 'chọn mục khóa/chưa khám phá (START khi BestWave<UnlockWave; thẻ "? UNDISCOVERED" trong Museum)', 'P (clip mới)'),
       'error': ('hai nốt marimba rơi + bịch 0.5 s', 'lỗi lưu/launch hiện ra (RunMenu.error được set; ArenaController.SaveError)', 'P (clip mới)')}
for k, (snd, when, st) in UIW.items():
    a('ui.' + k, snd, when, 'người chơi', 'một lần mỗi thay đổi TRẠNG THÁI thật; không phát khi không đổi', st)
a('sting.waveclear', 'marimba 4 nốt + vibes + pizz 1.6 s', 'phase Combat->Shop (ArcadeMusic.SetPhase)', '-', 'một lần mỗi wave', 'W-M: ArcadeMusic.cs SetPhase')
a('sting.victory', 'celesta/vibes + chuông + gong 3.6 s; nhạc im 3.6 s', 'phase đổi sang Victory', '-', 'một lần', 'W-M')
a('sting.defeat', 'clarinet tụt 4 nốt + gong 3.8 s; nhạc im 3.8 s', 'phase đổi sang Defeat', '-', 'một lần', 'W-M')
for k, snd in (('stage', 'sân khấu đêm: không khí trầm'), ('office', 'văn phòng ma thuật: đèn huỳnh quang + quạt'), ('lostshop', 'cửa hàng thất lạc: phòng tối + tích tắc'), ('goldshop', 'shop vàng: phòng ấm + chuông quầy thưa'), ('dreamshop', 'shop giấc mơ: drone detune + lấp lánh'), ('toys', 'gác đồ chơi: lên dây cót')):
    used = k in ('stage', 'goldshop', 'dreamshop')
    a('amb.' + k, snd + ' (loop 24 s liền mạch)', 'ArcadeMusic.UpdateAmbience: theo scene', '-', 'một bed, crossfade 4-8 s', ('W-M: ArcadeMusic.AmbienceFor' if used else 'R: chưa có API theme map để chọn (office/lostshop/toys); cần Theme từ gameplay'))
a('amb.transition', 'quét gió 1.4 s', '(chuyển cảnh)', '-', '-', 'R: chưa gọi; combat -> shop đã có sting.waveclear')
VN = 'MerchantVoice.Advance (WorldShop.cs:117, Display); nhịp theo TỪ'
a('voice.gold.syl', 'âm tiết gibberish ấm 118 Hz 0.11-0.15 s', 'đầu mỗi từ + từ >5 chữ có âm tiết thứ hai; sau , ; - nghỉ 0.16 s, sau . ! ? : nghỉ 0.3 s; tối đa 14 âm tiết/câu; không bao giờ mỗi ký tự', 'người bán shop vàng', 'tối đa 1 âm tiết/0.14 s', 'W-S: ' + VN)
a('voice.dream.syl', 'âm tiết hơi thở, cao 190 Hz, rung 6 Hz 0.16-0.2 s', 'như trên', 'người bán shop giấc mơ', 'như trên', 'W-S: ' + VN)
for kd in ('gold', 'dream'):
    for k, snd, when in (('laugh', 'cười 4 âm tiết', 'MerchantMoment.Buy/BloodBuy'), ('hmm', 'ừm kéo dài', 'MerchantMoment.Inspect (lúc người chơi nhìn món lâu)'), ('gasp', 'hít hơi', 'MerchantMoment.Denied/Fuse'), ('farewell', '3 âm tiết chào', 'MerchantMoment.Leave')):
        a('voice.%s.%s' % (kd, k), snd, when + ' (WorldShop.cs:195 dialogue.Speak)', 'người bán ' + ('vàng' if kd == 'gold' else 'giấc mơ'), 'một lần mỗi câu, KHÔNG chồng lên chuỗi âm tiết', 'P: MerchantVoice.Gesture("%s", special) có sẵn, chưa ai gọi' % k)

WIRED = {
    'Assets/ArcaneBrigade/Runtime/Presentation/ArcaneAudio.cs': ['wpn.boomerang.return', 'wpn.pierce', 'wpn.split', 'wpn.bounce', 'fx.chill', 'fx.knockback', 'boss.phase', 'enemy.lunge', 'enemy.mimic.wake', 'loot.coin.drop', 'loot.coin.vacuum', 'loot.chest.appear'],
    'Assets/ArcaneBrigade/Runtime/Presentation/CheckpointAudio.cs': ['cmb.miss', 'loot.coin.bounce', 'fx.hazard.warn'],
    'Assets/ArcaneBrigade/Runtime/Presentation/FootstepDust.cs': ['chr.step', 'chr.dash', 'enemy.big.step', 'enemy.big.roar'],
    'Assets/ArcaneBrigade/Runtime/Presentation/UnitVisual.cs': ['up.ally.land'],
    'Assets/ArcaneBrigade/Runtime/Presentation/MergeReveal.cs': ['up.merge.gather', 'up.merge', 'up.star2', 'up.star3', 'shop.fusion'],
    'Assets/ArcaneBrigade/Runtime/Presentation/MarketLifecycle.cs': ['shop.carpet.open', 'shop.carpet.roll', 'shop.keeper.exit.gold', 'shop.keeper.exit.dream'],
    'Assets/ArcaneBrigade/Runtime/Presentation/WorldShop.cs': ['up.bond.t1', 'up.bond.t2', 'up.rarity.blue', 'up.rarity.purple', 'up.rarity.gold', 'shop.stand.appear', 'shop.item.pop', 'shop.approach', 'shop.focus', 'shop.buy.gold', 'shop.buy.dream', 'shop.deny.gold', 'shop.deny.egg', 'shop.deny.hp', 'shop.keeper.enter.gold', 'shop.keeper.enter.dream'],
    'Assets/ArcaneBrigade/Runtime/Input/ArcadeNavigation.cs': ['ui.hover'],
    'Assets/ArcaneBrigade/Runtime/UI/RunMenu.cs': ['ui.back', 'ui.open', 'ui.setting', 'ui.confirm'],
    'Assets/ArcaneBrigade/Runtime/UI/RunDetails.cs': ['ui.close', 'ui.tab', 'ui.page'],
    'Assets/ArcaneBrigade/Runtime/UI/GlossaryUI.cs': ['ui.tooltip'],
    'Assets/ArcaneBrigade/Runtime/UI/RunMuseum.cs': ['ui.locked'],
    'Assets/ArcaneBrigade/Runtime/Core/ArenaController.cs': ['ui.pause', 'ui.resume', 'ui.unlock', 'ui.achievement', 'ui.error', 'shop.descend'],
    'Assets/ArcaneBrigade/Runtime/UI/RunInvoice.cs': ['ui.receipt'],
}
for path, ids in WIRED.items():
    source = open(os.path.join(ROOT, path), encoding='utf-8').read().splitlines()
    for cid in ids:
        line = next((number for number, text in enumerate(source, 1) if '"' + cid + '"' in text), None)
        assert line is not None, (cid, path)
        sound, when, who, gran, _ = A[cid]
        A[cid] = (sound, when, who, gran, 'W-I: `%s:%d` (event routes are driven by PlayBatch)' % (path, line))
for kind in ('gold', 'dream'):
    for gesture in ('laugh', 'hmm', 'gasp', 'farewell'):
        cid = 'voice.%s.%s' % (kind, gesture)
        path = 'Assets/ArcaneBrigade/Runtime/Presentation/WorldShop.cs'
        source = open(os.path.join(ROOT, path), encoding='utf-8').read().splitlines()
        line = next(number for number, text in enumerate(source, 1) if 'Gesture(' in text and '"' + gesture + '"' in text)
        sound, when, who, gran, _ = A[cid]
        A[cid] = (sound, when, who, gran, 'W-S: `%s:%d` -> MerchantVoice.Gesture' % (path, line))
for cid, reason in {'chr.jump': 'Arrival already contains its lift and landing arc in up.ally.land; separate jump would double the transient.', 'fx.reaction.shatter': 'No separate shatter mechanic; existing Ember/Frost reaction routes to fx.reaction.steam.'}.items():
    sound, when, who, gran, _ = A[cid]
    A[cid] = (sound, when, who, gran, 'R: ' + reason)
sound, _, who, gran, status = A['chr.dash']
A['chr.dash'] = (sound, 'Patrol -> Pursuit edge, not every pursuit footstep; off-screen culled and globally cooldown-limited.', who, gran, status)
sound, _, who, gran, status = A['cmb.miss']
A['cmb.miss'] = (sound, 'Hero melee Fire with no matching Hit schedules a quiet follow-up after 0.07 s.', who, gran, status)
for cid in ('up.star2', 'up.star3'):
    sound, _, who, gran, status = A[cid]
    A[cid] = (sound, 'Merge flash at age 0.54 s, star arpeggio after 0.07 s to avoid simultaneous cue masking.', who, gran, status)

missing = [c['id'] for c in cues if c['id'] not in A]
extra = [k for k in A if k not in BY]
assert not missing and not extra, (missing, extra)

BUSDESC = {'Combat': 'Combat', 'UI': 'UI', 'Foley': 'Foley', 'Voice': 'Voice', 'Ambience': 'Ambience'}
FOLDER = {'Voice': 'Voice', 'Ambience': 'Amb'}


def clip_path(c):
    f = FOLDER.get(c['bus'], 'Sfx')
    base = c['id'].replace('.', '_')
    n = len(c['clips'])
    return '%s/%s_00..%02d.wav' % (f, base, n - 1) if n > 1 else '%s/%s_00.wav' % (f, base)


def exist(c):
    ok = True
    for rel in c['clips']:
        p = os.path.join(AUD, rel + '.wav')
        ok &= os.path.exists(p) and os.path.getsize(p) > 1000
    return ok


def policy(c):
    parts = []
    parts.append('%s · prio %d · %+g dB (x%.2f)' % (c['bus'], c['priority'], c['gainDb'], 10 ** (c['gainDb'] / 20)))
    parts.append('cd %.2f s · %d voice' % (c['cooldown'], c['maxVoices']))
    parts.append('pitch ±%d%%' % round(c['pitchJitter'] * 100) if c['pitchJitter'] else 'pitch cố định')
    if c['danger']:
        parts.append('DANGER lane')
    if c['loop']:
        parts.append('loop')
    return '<br>'.join(parts)


def behaviour(c):
    if c['bus'] == 'UI':
        pz = 'phát khi pause'
    elif c['bus'] == 'Ambience':
        pz = 'bed tiếp tục, x0.3 khi pause'
    elif c['danger']:
        pz = 'im khi pause'
    else:
        pz = 'im khi pause (SimulationPaused)'
    if c['danger']:
        ra = 'Reduced: giữ nguyên gain'
    elif c['priority'] >= 180:
        ra = 'Reduced: BỎ'
    else:
        ra = 'Reduced: x0.65, cd x2'
    return pz + '<br>' + ra


SECTIONS = [('Combat: lấy đà, vung, đâm, đập, trúng, hụt, critical', ['cmb.']), ('Vũ khí: cung, súng, phép, boomerang, đạn xuyên/tách/nảy', ['wpn.']),
            ('Signature và payoff vũ khí', ['sig.']), ('Hệ, trạng thái, phản ứng, knockback, hazard', ['fx.']), ('Hero mechanic', ['hero.']),
            ('Nhân vật chính và di chuyển', ['chr.', 'plr.']), ('Quái, quái lớn, boss', ['enemy.', 'boss.']), ('Loot', ['loot.']), ('Nâng cấp: ghép sao, đồng minh, bond, chuỗi, độ hiếm', ['up.']),
            ('Shop', ['shop.']), ('UI', ['ui.']), ('Sting và ambience', ['sting.', 'amb.']), ('Giọng người bán', ['voice.'])]

out = []
w = out.append
w(open(os.path.join(os.path.dirname(__file__), 'contract_head.md'), encoding='utf-8').read())
w('\n## 5. Bảng hợp đồng từng cue (%d cue, %d clip)\n' % (len(cues), sum(len(c['clips']) for c in cues)))
w('Cột **Mức** và **Hành vi** sinh từ `cues.json`, nên không thể lệch với bản build. Đường dẫn clip tính từ `Assets/ArcaneBrigade/Resources/Audio/Arcane/`. `NN` = biến thể liên tiếp, không bao giờ lặp biến thể vừa phát.\n')
w('Mã trạng thái: **W-E** đã nối qua `CombatEvent`; **W-I** đã nối qua số nguyên cũ; **W-M/W-S** nối trong ArcadeMusic/WorldShop/MerchantVoice; **P** chưa nối (nêu điểm hook đề xuất); **R** có asset nhưng chưa/không dùng.\n')
used = set()
for title, prefixes in SECTIONS:
    rows = [c for c in cues if any(c['id'].startswith(p) for p in prefixes) and c['id'] not in used]
    if not rows:
        continue
    w('\n### 5.%d %s\n' % (SECTIONS.index((title, prefixes)) + 1, title))
    w('| Cue ID · clip | Âm thanh | Khi nào phát | Đối tượng | Một lần / mỗi viên / mỗi lượt | Mức · giới hạn | Pause · Reduced | Điểm gọi · trạng thái |')
    w('| --- | --- | --- | --- | --- | --- | --- | --- |')
    for c in rows:
        used.add(c['id'])
        s, wh, who, g, st = A[c['id']]
        flag = '' if exist(c) else ' ⚠ THIẾU FILE'
        w('| `%s`<br>`%s`%s | %s | %s | %s | %s | %s | %s | %s |' % (c['id'], clip_path(c), flag, s, wh, who, g, policy(c), behaviour(c), st.replace('|', '/')))

# ------------------------------------------------------------------------------------------- weapons table
M = ['Slash', 'Thrust', 'Slam', 'Recoil', 'Bell', 'Bow', 'Cast', 'Stamp', 'Scissors', 'Camera', 'Iron', 'Hammer', 'Music']
F = ['None', 'Tea', 'TetherBow', 'Scissors', 'Camera', 'Fish', 'Iron', 'Hammer', 'Alarm', 'MusicBox']
EL = ['Physical', 'Ember', 'Frost', 'Arcane']
KD = ['Contact', 'Melee', 'Projectile']
SG = ['', 'Umbrella', 'Bell', 'Stamp']


def fire_cue(motion, fam, kind, element, count, radius):
    if fam == 'Fish':
        return 'wpn.boomerang.fly'
    if fam == 'Tea':
        return 'sig.tea'
    if fam == 'Alarm':
        return 'sig.alarm'
    if fam == 'TetherBow':
        return 'wpn.bow.release'
    return {'Slash': 'cmb.swing.blade', 'Thrust': 'cmb.swing.spear', 'Slam': 'cmb.swing.hammer', 'Recoil': 'wpn.launcher' if count >= 3 or radius >= 0.08 else 'wpn.gun.pop',
            'Bell': 'sig.bell', 'Bow': 'wpn.bow.release', 'Cast': {'Ember': 'wpn.cast.fire', 'Frost': 'wpn.cast.ice'}.get(element, 'wpn.cast.arcane'), 'Stamp': 'sig.stamp',
            'Scissors': 'sig.scissors', 'Camera': 'sig.camera', 'Iron': 'sig.iron', 'Hammer': 'sig.hammer', 'Music': 'sig.musicbox'}[motion]


rows = []
for p in sorted(glob.glob(os.path.join(ROOT, 'Assets', 'ArcaneBrigade', 'Data', 'Run', '*.asset'))):
    t = open(p, encoding='utf-8', errors='ignore').read()
    if 'ProjectileCount' not in t or 'Motion:' not in t:
        continue
    g = lambda k, d='0': (re.search(r'^\s*' + k + r': (\S+)', t, re.M) or [None, d])[1]
    wid = g('Id')
    kind = KD[int(g('Kind'))]
    if kind == 'Contact' or wid.endswith('Attack'):
        continue   # enemy contact/attack stubs
    motion, fam, el = M[int(g('Motion'))], F[int(g('Family'))], EL[int(g('Element'))]
    count, pierce, split, bounce = int(g('ProjectileCount')), int(g('ProjectilePierce')), int(g('ProjectileSplit')), float(g('ProjectileBounceChance'))
    radius = float(g('ProjectileRadius', '0.055'))
    sig = SG[int(g('Signature'))]
    push = float(g('PushDistance'))
    traits = []
    if count > 1:
        traits.append('%d đạn/loạt' % count)
    if pierce:
        traits.append('xuyên %d' % pierce)
    if split:
        traits.append('tách %d' % split)
    if bounce:
        traits.append('nảy %.0f%%' % (bounce * 100))
    if g('HomingOutbound') == '1':
        traits.append('homing ra')
    if g('SpecialOnly') == '1':
        traits.append('chỉ shop đặc biệt')
    if push >= 0.5:
        traits.append('knockback %.1f' % push)
    fire = 'cmb.swing.blade' if sig == 'Umbrella' and False else fire_cue(motion, fam, kind, el, count, radius)
    if sig == 'Umbrella':
        fire = fire_cue(motion, fam, kind, el, count, radius)
    windup = 'wpn.bow.draw' if motion == 'Bow' else ('cmb.windup' if motion in ('Slam', 'Hammer') else 'cmb.windup chỉ khi là vũ khí của Champion; đồng minh: không')
    if kind == 'Projectile':
        impact = 'Hit của đạn: cmb.hit.<chất liệu quái>' + (' (nạn nhân thứ 2+: wpn.pierce chưa nối)' if pierce else '') + (' (mảnh tách -5 dB, wpn.split chưa nối)' if split else '') + (' (nảy: wpn.bounce chưa nối)' if bounce or fam in ('TetherBow',) else '')
    else:
        impact = 'Hit của vòng cung: 1 âm/lượt (dedupe RootId)'
    extra = {'Fish': 'đạn quay về: wpn.boomerang.return (chưa nối)', 'Tea': 'vũng trà: Hit "expanded-tea" (hit thường); tách: wpn.split (chưa nối)', 'TetherBow': 'dây nối: fx.field.zipper (Hit "zipper-link"); ricochet: wpn.bounce (chưa nối)',
             'Alarm': 'sig.alarm.boom khi fuse nổ 0.9 s sau (Hit "expanded-fuse")', 'Camera': 'Photo 0.22 s: không có cue riêng', 'Scissors': 'Strike thứ 2 tại +0.12 s nằm trong clip',
             'Hammer': 'Echo field: Hit thường', 'Iron': 'vệt Iron 1.4 s: Hit thường', 'MusicBox': 'nốt cuối FinalNote: không riêng'}.get(fam, '')
    if sig:
        extra = ('SignaturePulse/Burst -> sig.' + sig.lower() + '; ' + extra).strip('; ')
    rows.append((wid, '%s/%s/%s/%s' % (kind, motion, fam, el), ', '.join(traits) or '-', windup, fire, impact, extra.replace(' (chưa nối)', '') or '-'))
w('\n## 6. Từng loại vũ khí (%d vũ khí, đọc từ `Data/Run/*.asset`)\n' % len(rows))
w('Âm **phát đòn** = Fire (một lần mỗi lượt/loạt). Âm **projectile va chạm** = Hit riêng (mỗi nạn nhân), phân biệt bằng `Hit.Weapon.Kind==Projectile` và `Cause.ItemId`. Hai âm KHÔNG bao giờ dùng chung cue.\n')
w('| Vũ khí | Kind/Motion/Family/Element | Đặc tính | Lấy đà (Windup) | Phát đòn (Fire) | Va chạm (Hit) | Payoff/ghi chú |')
w('| --- | --- | --- | --- | --- | --- | --- |')
for r in rows:
    w('| `%s` | %s | %s | `%s` | `%s` | %s | %s |' % r)

# ------------------------------------------------------------------------------------------- old -> new
w(open(os.path.join(os.path.dirname(__file__), 'contract_tail.md'), encoding='utf-8').read())

# computed lists
w('\n## 12. Danh sách trạng thái (sinh tự động)\n')
def lst(pred):
    return ', '.join('`%s`' % c['id'] for c in cues if pred(A[c['id']][4], c))
w('**Đã nối (W-*)**: ' + lst(lambda st, c: st.startswith('W')) + '\n')
w('**Chưa nối (P)**: ' + (lst(lambda st, c: st.startswith('P')) or 'không') + '\n')
w('**Có asset nhưng không dùng (R)**: ' + lst(lambda st, c: st.startswith('R')) + '\n')
w('**Thiếu file asset**: ' + (', '.join('`%s`' % c['id'] for c in cues if not exist(c)) or 'không') + '\n')
w('\nTổng: %d cue; đã nối %d; chưa nối %d; có asset không dùng %d.\n' % (len(cues), sum(A[c['id']][4].startswith('W') for c in cues), sum(A[c['id']][4].startswith('P') for c in cues), sum(A[c['id']][4].startswith('R') for c in cues)))

# music table
w('\n## 13. Nhạc (đã nối qua ArcadeMusic)\n')
w('| Bản | Scene | BPM | Nhịp | Loop | Stem |')
w('| --- | --- | --- | --- | --- | --- |')
for pc in pieces:
    w('| `%s` | %s | %g | %d/4 | %.1f s (%d bars) | `%s_{base,rhythm,melody,intensity}.ogg` |' % (pc['id'], ', '.join(pc['scenes']), pc['bpm'], pc['beatsPerBar'], pc['loopSeconds'], pc['bars'], pc['stems'][0].rsplit('_', 1)[0]))
open(os.path.join(ROOT, 'Docs', 'Audio', 'Audio_Integration_Contract.md'), 'w', encoding='utf-8').write('\n'.join(out) + '\n')
print('written', len(cues), 'cues', len(rows), 'weapons')
