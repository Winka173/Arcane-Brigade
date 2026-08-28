using System;
using System.Collections.Generic;
using UnityEngine;
namespace ArcaneBrigade
{
    public static class EnemyRulesV3Catalog
    {
        public static readonly EnemyRuleDefinition[] Definitions={
            new EnemyRuleDefinition { Kind=EnemyKindV3.CrumbSwarm, Id="CrumbSwarm", Name="Crumb Swarm", Night=EnemyRulesV3Tuning.Nights[0], Role="Fodder", Body="comes 30 to 60 at once; each dies in one hit", Skill=null, Telegraph="a crowd of tiny sprites; the giant has a crown of crumbs" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Marcher, Id="Marcher", Name="Marcher", Night=EnemyRulesV3Tuning.Nights[1], Role="Fodder", Body="walks in tight ranks", Skill=null, Telegraph="ranks with matching stripes" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Pillbug, Id="Pillbug", Name="Pillbug", Night=EnemyRulesV3Tuning.Nights[2], Role="Fodder", Body="rolls fast in straight lines", Skill=null, Telegraph="a rolling stripe on the ground, solid red" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Flanker, Id="Flanker", Name="Flanker", Night=EnemyRulesV3Tuning.Nights[3], Role="Fodder", Body="enters behind the hero", Skill=null, Telegraph="a blue arrow on the ground" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Smasher, Id="Smasher", Name="Smasher", Night=EnemyRulesV3Tuning.Nights[4], Role="Brute", Skill="a telegraphed slam: area damage and a short stun on friends", Telegraph="a filled red circle for 1 s" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Stitcher, Id="Stitcher", Name="Stitcher", Night=EnemyRulesV3Tuning.Nights[5], Role="Support", Skill="when another of its kind falls near it, it heals to full; kill the group together", Telegraph="stitches glow between members" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Grower, Id="Grower", Name="Grower", Night=EnemyRulesV3Tuning.Nights[6], Role="Brute", Body="larger and tougher each wave stage", Skill=null, Telegraph="grows visibly larger" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Decoy, Id="Decoy", Name="Decoy", Night=EnemyRulesV3Tuning.Nights[7], Role="Trick", Body="looks like a rare enemy; dies in one hit", Skill=null, Telegraph="a faint wobble" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Shielder, Id="Shielder", Name="Shielder", Night=EnemyRulesV3Tuning.Nights[8], Role="Support", Skill="shields the group behind it (already in the game)", Telegraph="a dome over the group" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Splitter, Id="Splitter", Name="Splitter", Night=EnemyRulesV3Tuning.Nights[9], Role="Brute", Body="splits in two when it dies", Skill=null, Telegraph="a seam on its body" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Blinker, Id="Blinker", Name="Blinker", Night=EnemyRulesV3Tuning.Nights[10], Role="Trick", Body="moves in short hops", Skill=null, Telegraph="a flash on the ground" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.SniperDoll, Id="SniperDoll", Name="Sniper Doll", Night=EnemyRulesV3Tuning.Nights[11], Role="Ranged", Skill="stands outside the arena and shoots the lowest-health friend; a thin line shows its target for 1 s first", Telegraph="a red laser line" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Leech, Id="Leech", Name="Leech", Night=EnemyRulesV3Tuning.Nights[12], Role="Hunter", Skill="latches onto a friend and drains it, healing itself; hits on the friend also hit the leech", Telegraph="a thread between the leech and the friend" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Bomber, Id="Bomber", Name="Bomber", Night=EnemyRulesV3Tuning.Nights[13], Role="Brute", Skill="swells, then bursts and leaves a poison pool on friends' ground", Telegraph="a swelling red body" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Healer, Id="Healer", Name="Healer", Night=EnemyRulesV3Tuning.Nights[14], Role="Support", Skill="heals the weakest enemies nearby for 5% a second", Telegraph="green motes" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Buffer, Id="Buffer", Name="Buffer", Night=EnemyRulesV3Tuning.Nights[15], Role="Support", Skill="cheers: every enemy within 8 units gains +30% speed for 4 s", Telegraph="a horn note ring" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.BannerBearer, Id="BannerBearer", Name="Banner Bearer", Night=EnemyRulesV3Tuning.Nights[16], Role="Support", Skill="while alive every enemy within 6 units deals +20% damage; the banner drops on death and can be picked up by friends", Telegraph="a waving banner" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Charger, Id="Charger", Name="Charger", Night=EnemyRulesV3Tuning.Nights[17], Role="Hunter", Skill="charges the farthest friend in a line that passes through everything; hitting a wall stuns it", Telegraph="a long red strip on the ground" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Thief, Id="Thief", Name="Thief", Night=EnemyRulesV3Tuning.Nights[18], Role="Trick", Body="grabs dropped coins and runs; killing it returns 110%", Skill=null, Telegraph="a sack on its back" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Trapper, Id="Trapper", Name="Trapper", Night=EnemyRulesV3Tuning.Nights[19], Role="Trick", Skill="lays spikes, cocoons or mines under friends' feet", Telegraph="small marked tiles" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Marker, Id="Marker", Name="Marker", Night=EnemyRulesV3Tuning.Nights[20], Role="Support", Skill="marks the hero: every enemy turns toward the hero", Telegraph="a target over the hero" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.ConeBrute, Id="ConeBrute", Name="Cone-brute", Night=EnemyRulesV3Tuning.Nights[21], Role="Brute", Skill="its hits splash in a cone behind the target", Telegraph="a cone shape on the ground" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.EggLayer, Id="EggLayer", Name="Egg Layer", Night=EnemyRulesV3Tuning.Nights[22], Role="Support", Skill="lays an egg that hatches into 3 small enemies after 5 s unless it is destroyed", Telegraph="a shaking egg" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.ShieldWall, Id="ShieldWall", Name="Shield Wall", Night=EnemyRulesV3Tuning.Nights[23], Role="Brute", Body="immune to hits from the front", Skill=null, Telegraph="a big tower shield" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.MagnetBrute, Id="MagnetBrute", Name="Magnet Brute", Night=EnemyRulesV3Tuning.Nights[24], Role="Brute", Skill="pulls friendly projectiles toward itself and absorbs them (a bullet sponge for projectile builds)", Telegraph="a horseshoe magnet and field lines" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Mourner, Id="Mourner", Name="Mourner", Night=EnemyRulesV3Tuning.Nights[25], Role="Support", Skill="when it dies every enemy within 8 units gains +40% speed for 5 s", Telegraph="a black veil" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Silencer, Id="Silencer", Name="Silencer", Night=EnemyRulesV3Tuning.Nights[26], Role="Control", Skill="a ring around it pauses the skill bars of friends inside for 3 s", Telegraph="a hushed ring on the ground" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.TimeEater, Id="TimeEater", Name="Time Eater", Night=EnemyRulesV3Tuning.Nights[27], Role="Control", Skill="an aura halves the skill-bar fill speed of friends within 6 units", Telegraph="a pale clock face" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Cleanser, Id="Cleanser", Name="Cleanser", Night=EnemyRulesV3Tuning.Nights[28], Role="Control", Skill="removes buffs and shields from friends in an area", Telegraph="white sparks" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Ambusher, Id="Ambusher", Name="Ambusher", Night=EnemyRulesV3Tuning.Nights[29], Role="Trick", Body="hard to see until 3 units away", Skill=null, Telegraph="a ripple in the ground" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.PhaseWalker, Id="PhaseWalker", Name="Phase Walker", Night=EnemyRulesV3Tuning.Nights[30], Role="Hunter", Body="walks through walls and friends", Skill=null, Telegraph="a wavering outline" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Burrower, Id="Burrower", Name="Burrower", Night=EnemyRulesV3Tuning.Nights[31], Role="Hunter", Skill="tunnels under the arena and bursts up beneath a friend cluster", Telegraph="a moving mound, then a filled circle" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.KamikazeCandle, Id="KamikazeCandle", Name="Kamikaze Candle", Night=EnemyRulesV3Tuning.Nights[32], Role="Hunter", Skill="runs at the largest friend cluster and explodes in a wide area", Telegraph="a lit fuse" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.GravityImp, Id="GravityImp", Name="Gravity Imp", Night=EnemyRulesV3Tuning.Nights[33], Role="Control", Skill="pulls friends together into a clump (bad against area attacks, good for the enemy)", Telegraph="a spiral on the ground" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Shrieker, Id="Shrieker", Name="Shrieker", Night=EnemyRulesV3Tuning.Nights[34], Role="Control", Skill="a scream makes friends flee 1 s and breaks stillness effects", Telegraph="sound rings" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.GluttonousMouth, Id="GluttonousMouth", Name="Gluttonous Mouth", Night=EnemyRulesV3Tuning.Nights[35], Role="Trick", Body="eats dropped coins and pickups; kill it to get them back", Skill=null, Telegraph="a wide mouth" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Rallier, Id="Rallier", Name="Rallier", Night=EnemyRulesV3Tuning.Nights[36], Role="Support", Skill="when it falls below 50% health every enemy within 8 units gains a 15% shield", Telegraph="a raised fist" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.MirrorImp, Id="MirrorImp", Name="Mirror Imp", Night=EnemyRulesV3Tuning.Nights[37], Role="Trick", Skill="reflects 30% of ranged damage while facing the hero", Telegraph="a mirror shield" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Reviver, Id="Reviver", Name="Reviver", Night=EnemyRulesV3Tuning.Nights[38], Role="Support", Skill="revives one fallen enemy within 8 units after 5 s; kill it first", Telegraph="a green cross over bodies" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Summoner, Id="Summoner", Name="Summoner", Night=EnemyRulesV3Tuning.Nights[39], Role="Support", Skill="calls up to 6 small helpers every 10 s", Telegraph="a glowing circle" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Darkener, Id="Darkener", Name="Darkener", Night=EnemyRulesV3Tuning.Nights[40], Role="Control", Skill="makes a dark zone: friends inside miss 30% of shots", Telegraph="a black cloud" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Binder, Id="Binder", Name="Binder", Night=EnemyRulesV3Tuning.Nights[41], Role="Control", Skill="chains two enemies: if one falls the other takes 40% of its health (Tethered in the game)", Telegraph="a chain between enemies" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Drainer, Id="Drainer", Name="Drainer", Night=EnemyRulesV3Tuning.Nights[42], Role="Control", Skill="drains coins or health of friends inside a zone", Telegraph="a pink spiral" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Charmer, Id="Charmer", Name="Charmer", Night=EnemyRulesV3Tuning.Nights[43], Role="Control", Skill="charms one friend for 3 s: it attacks other friends", Telegraph="hearts over the friend" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.Immune, Id="Immune", Name="Immune", Night=EnemyRulesV3Tuning.Nights[44], Role="Brute", Body="cannot be stunned or slowed", Skill=null, Telegraph="a metallic sheen" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.StormEye, Id="StormEye", Name="Storm-eye", Night=EnemyRulesV3Tuning.Nights[45], Role="Ranged", Skill="every 3rd attack chains lightning to 4 more targets", Telegraph="a crackling eye" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.ArmourGrower, Id="ArmourGrower", Name="Armour-grower", Night=EnemyRulesV3Tuning.Nights[46], Role="Brute", Skill="grows and slams, then has armour for 4 s (Armored in the game)", Telegraph="plates growing on its body" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.MeteorCaller, Id="MeteorCaller", Name="Meteor Caller", Night=EnemyRulesV3Tuning.Nights[47], Role="Ranged", Skill="calls meteors onto the largest friend cluster (solid filled circles, 1.5 s)", Telegraph="filled red circles" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.TideBrute, Id="TideBrute", Name="Tide Brute", Night=EnemyRulesV3Tuning.Nights[48], Role="Control", Skill="floods the area under it: friends inside move 30% slower", Telegraph="a spreading blue puddle" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.FireBrute, Id="FireBrute", Name="Fire Brute", Night=EnemyRulesV3Tuning.Nights[49], Role="Elemental", Skill="leaves flames on the ground that burn friends who cross them", Telegraph="embers on the ground" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.IceWisp, Id="IceWisp", Name="Ice Wisp", Night=EnemyRulesV3Tuning.Nights[50], Role="Elemental", Skill="chills friends in a ring (-20% speed, stacking)", Telegraph="frosty ring" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.StormWisp, Id="StormWisp", Name="Storm Wisp", Night=EnemyRulesV3Tuning.Nights[51], Role="Elemental", Skill="chains lightning between friends in contact with each other", Telegraph="blue arcs" },
            new EnemyRuleDefinition { Kind=EnemyKindV3.VenomWisp, Id="VenomWisp", Name="Venom Wisp", Night=EnemyRulesV3Tuning.Nights[52], Role="Elemental", Skill="poisons friends in a ring; the poison spreads between friends that touch", Telegraph="a green mist" },
        };
        public static readonly EnemyAffixDefinition[] Affixes={
            new EnemyAffixDefinition(EnemyAffixV3.Swift,"Swift","Swift","Increased movement speed"),
            new EnemyAffixDefinition(EnemyAffixV3.Armored,"Armored","Armored","Armor and knockback immunity"),
            new EnemyAffixDefinition(EnemyAffixV3.Vampiric,"Vampiric","Vampiric","Heals from damage dealt by every nearby enemy"),
            new EnemyAffixDefinition(EnemyAffixV3.Explosive,"Explosive","Explosive","A warned filled circle bursts after death"),
            new EnemyAffixDefinition(EnemyAffixV3.Shielded,"Shielded","Shielded","Starts the wave shielded"),
            new EnemyAffixDefinition(EnemyAffixV3.Splitting,"Splitting","Splitting","Splits into two on death"),
            new EnemyAffixDefinition(EnemyAffixV3.Burning,"Burning","Burning","Leaves burning ground"),
            new EnemyAffixDefinition(EnemyAffixV3.Frozen,"Frozen","Frozen","Chills friends on contact"),
            new EnemyAffixDefinition(EnemyAffixV3.Teleporting,"Teleporting","Teleporting","Blinks to the weakest friend"),
            new EnemyAffixDefinition(EnemyAffixV3.Regenerating,"Regenerating","Regenerating","Regenerates outside combat"),
            new EnemyAffixDefinition(EnemyAffixV3.Commanding,"Commanding","Commanding","Nearby enemy damage aura"),
            new EnemyAffixDefinition(EnemyAffixV3.Reflective,"Reflective","Reflective","Reflects damage once per wave")
        };
        public static readonly EnemyBossDefinition[] Bosses={
            new EnemyBossDefinition { Kind=EnemyBossV3.PastryTyrant,Id="PastryTyrant",Name="Pastry Tyrant",FirstNight=EnemyRulesV3Tuning.BossFirstNights[0],RepeatNight=EnemyRulesV3Tuning.BossRepeatNights[0],SecondPhase=EnemyRulesV3Tuning.PastrySecond,FinalPhase=EnemyRulesV3Tuning.PastryFinal },
            new EnemyBossDefinition { Kind=EnemyBossV3.ClockworkWarden,Id="ClockworkWarden",Name="Clockwork Warden",FirstNight=EnemyRulesV3Tuning.BossFirstNights[1],RepeatNight=EnemyRulesV3Tuning.BossRepeatNights[1],SecondPhase=EnemyRulesV3Tuning.WardenSecond,FinalPhase=EnemyRulesV3Tuning.WardenFinal },
            new EnemyBossDefinition { Kind=EnemyBossV3.StormHerald,Id="StormHerald",Name="Storm Herald",FirstNight=EnemyRulesV3Tuning.BossFirstNights[2],RepeatNight=EnemyRulesV3Tuning.BossRepeatNights[2],SecondPhase=EnemyRulesV3Tuning.StormSecond,FinalPhase=EnemyRulesV3Tuning.StormFinal },
            new EnemyBossDefinition { Kind=EnemyBossV3.HollowChoir,Id="HollowChoir",Name="Hollow Choir",FirstNight=EnemyRulesV3Tuning.BossFirstNights[3],RepeatNight=EnemyRulesV3Tuning.BossRepeatNights[3],SecondPhase=EnemyRulesV3Tuning.OtherSecond,FinalPhase=EnemyRulesV3Tuning.OtherFinal },
            new EnemyBossDefinition { Kind=EnemyBossV3.GravityMaw,Id="GravityMaw",Name="Gravity Maw",FirstNight=EnemyRulesV3Tuning.BossFirstNights[4],RepeatNight=EnemyRulesV3Tuning.BossRepeatNights[4],SecondPhase=EnemyRulesV3Tuning.OtherSecond,FinalPhase=EnemyRulesV3Tuning.OtherFinal },
            new EnemyBossDefinition { Kind=EnemyBossV3.LanternQueen,Id="LanternQueen",Name="Lantern Queen",FirstNight=EnemyRulesV3Tuning.BossFirstNights[5],RepeatNight=EnemyRulesV3Tuning.BossRepeatNights[5],SecondPhase=EnemyRulesV3Tuning.OtherSecond,FinalPhase=EnemyRulesV3Tuning.OtherFinal }
        };
        private static readonly Dictionary<string,EnemyRuleDefinition> lookup=BuildLookup();
        private static Dictionary<string,EnemyRuleDefinition> BuildLookup()
        {
            var result=new Dictionary<string,EnemyRuleDefinition>(StringComparer.OrdinalIgnoreCase);
            foreach(var row in Definitions) { result[row.Id]=row; result[row.Name]=row; }
            return result;
        }
        public static EnemyRuleDefinition Find(string id) => id!=null && lookup.TryGetValue(id,out EnemyRuleDefinition value)?value:null;
        public static EnemyBossDefinition FindBoss(string id)
        { foreach(var boss in Bosses) if(boss.Id==id || boss.Name==id)return boss; return null; }
        public static bool Recognizes(UnitDefinition unit) => unit!=null && (Find(unit.Id)!=null || FindBoss(unit.Id)!=null);
        public static EnemyBossDefinition BossForNight(int night)
        { foreach(var boss in Bosses) if(boss.FirstNight==night || boss.RepeatNight==night)return boss; return null; }
        public static EnemyBodyTier Tier(EnemyKindV3 kind)
        {
            switch(kind)
            {
                case EnemyKindV3.CrumbSwarm: case EnemyKindV3.Marcher: case EnemyKindV3.Pillbug: case EnemyKindV3.Flanker:
                case EnemyKindV3.Decoy: case EnemyKindV3.Blinker: case EnemyKindV3.Ambusher: case EnemyKindV3.PhaseWalker: return EnemyBodyTier.Small;
                case EnemyKindV3.Grower: case EnemyKindV3.Splitter: case EnemyKindV3.ShieldWall: case EnemyKindV3.Immune:
                case EnemyKindV3.Thief: case EnemyKindV3.GluttonousMouth: return EnemyBodyTier.MediumPlain;
                case EnemyKindV3.Smasher: case EnemyKindV3.SniperDoll: case EnemyKindV3.Bomber: case EnemyKindV3.Healer:
                case EnemyKindV3.BannerBearer: case EnemyKindV3.EggLayer: case EnemyKindV3.KamikazeCandle: case EnemyKindV3.Charger: return EnemyBodyTier.MediumSkilled;
                default: return EnemyBodyTier.LargeSpecial;
            }
        }
        public static bool HasSkill(EnemyKindV3 kind) => Tier(kind)==EnemyBodyTier.MediumSkilled || Tier(kind)==EnemyBodyTier.LargeSpecial;
        public static bool IsSmall(UnitDefinition unit) => Find(unit?.Id)?.Tier==EnemyBodyTier.Small;
        public static bool IsLargeOrSpecial(UnitDefinition unit) => Find(unit?.Id)?.Tier==EnemyBodyTier.LargeSpecial;
        public static UnitDefinition Create(EnemyRuleDefinition row)
        {
            UnitDefinition unit=ScriptableObject.CreateInstance<UnitDefinition>();
            unit.Id=row.Id; unit.DisplayName=row.Name; unit.UnlockWave=row.Night;
            unit.Health=row.Kind==EnemyKindV3.CrumbSwarm || row.Kind==EnemyKindV3.Decoy?EnemyRulesV3Tuning.OneHitHealth:EnemyRulesV3Tuning.DefaultHealth;
            unit.Speed=EnemyRulesV3Tuning.DefaultSpeed*(row.Kind==EnemyKindV3.Pillbug?EnemyRulesV3Tuning.ChargedScale:1); unit.Radius=EnemyRulesV3Tuning.DefaultRadiusBody;
            unit.ContactDamage=EnemyRulesV3Tuning.DefaultDamage; unit.ContactInterval=EnemyRulesV3Tuning.DefaultCooldown;
            unit.Description=row.HasSkill?row.Skill:row.Body; unit.Weapon=CreateWeapon(row.Id,row.Role=="Ranged");
            return unit;
        }
        public static UnitDefinition CreateBoss(EnemyBossDefinition row)
        {
            UnitDefinition unit=ScriptableObject.CreateInstance<UnitDefinition>();
            unit.Id=row.Id; unit.DisplayName=row.Name; unit.IsBoss=true; unit.UnlockWave=row.FirstNight;
            unit.Health=EnemyRulesV3Tuning.BossHealth; unit.Speed=EnemyRulesV3Tuning.DefaultSpeed;
            unit.ContactDamage=EnemyRulesV3Tuning.DefaultDamage; unit.Weapon=CreateWeapon(row.Id,false); return unit;
        }
        private static WeaponDefinition CreateWeapon(string id,bool ranged)
        {
            WeaponDefinition weapon=ScriptableObject.CreateInstance<WeaponDefinition>();
            weapon.Id=id+"RulesV3Weapon"; weapon.Kind=ranged?WeaponKind.Projectile:WeaponKind.Contact;
            weapon.Damage=EnemyRulesV3Tuning.DefaultDamage; weapon.Range=EnemyRulesV3Tuning.DefaultRange;
            weapon.Cooldown=EnemyRulesV3Tuning.DefaultCooldown; return weapon;
        }
        /// <summary>Existing bodies with exact identities keep their assets. Every enemy on screen must be drawn, so a kind
        /// with no painting yet wears a painted body from the existing enemies (small kinds the smallest, large kinds the
        /// largest, a stable pick inside that band) until its own art exists.</summary>
        public static void Configure(RunCatalog catalog)
        {
            if(catalog==null)throw new ArgumentNullException(nameof(catalog));
            List<UnitDefinition> painted=PaintedSorted(catalog.Enemies), paintedBosses=PaintedSorted(catalog.Bosses);
            var enemies=new List<UnitDefinition>();
            foreach(var row in Definitions)
            { UnitDefinition found=null; if(catalog.Enemies!=null)foreach(var unit in catalog.Enemies)if(unit!=null && unit.Id==row.Id){found=unit;break;} enemies.Add(found??Dress(Create(row),painted,row.Tier)); }
            var bosses=new List<UnitDefinition>();
            foreach(var row in Bosses)
            { UnitDefinition found=null; if(catalog.Bosses!=null)foreach(var unit in catalog.Bosses)if(unit!=null && unit.Id==row.Id){found=unit;break;} bosses.Add(found??Dress(CreateBoss(row),paintedBosses,EnemyBodyTier.LargeSpecial)); }
            catalog.Enemies=enemies.ToArray(); catalog.Bosses=bosses.ToArray();
        }
        public static bool Painted(UnitDefinition unit) => unit!=null && (unit.Body!=null || unit.AuthoredMotion!=null || unit.AnimationFrames!=null && unit.AnimationFrames.Length>0);
        private static List<UnitDefinition> PaintedSorted(UnitDefinition[] units)
        {
            var list=new List<UnitDefinition>();
            if(units!=null)foreach(UnitDefinition unit in units)if(Painted(unit))list.Add(unit);
            list.Sort((a,b)=>a.Radius.CompareTo(b.Radius)!=0?a.Radius.CompareTo(b.Radius):string.CompareOrdinal(a.Id,b.Id));
            return list;
        }
        private static UnitDefinition Dress(UnitDefinition unit,List<UnitDefinition> painted,EnemyBodyTier tier)
        {
            if(Painted(unit))return unit;
            // its own painting when it has been drawn (Resources/EnemiesV3/<Id>), otherwise a borrowed painted body
            Sprite own=Resources.Load<Sprite>("EnemiesV3/"+unit.Id);
            if(own!=null){unit.Body=own;unit.BodyScale=1;return unit;}
            if(painted.Count==0)return unit;
            int third=Mathf.Max(1,painted.Count/3), start=tier==EnemyBodyTier.Small?0:tier==EnemyBodyTier.LargeSpecial?painted.Count-third:third;
            int span=Mathf.Min(third,painted.Count-start); uint hash=17; foreach(char c in unit.Id)hash=hash*31+c;
            UnitDefinition donor=painted[start+(int)(hash%(uint)span)];
            unit.Body=donor.Body; unit.AnimationFrames=donor.AnimationFrames; unit.AuthoredMotion=donor.AuthoredMotion;
            unit.BodyScale=donor.BodyScale; unit.Tint=donor.Tint; unit.Radius=donor.Radius;
            return unit;
        }
    }
}
