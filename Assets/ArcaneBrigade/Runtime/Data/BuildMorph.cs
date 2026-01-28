using System;
using UnityEngine;

namespace ArcaneBrigade
{
    public enum BuildMorphKind { None, Thread, Cinder, Clockwork, Porcelain, Constellation, Paper, Mirror, Chorus, Magnet, Tide, Frost, Seal }

    public sealed class BuildMorphDefinition
    {
        public readonly BuildMorphKind Kind;
        public readonly string Name, Silhouette, Pattern;
        public readonly Color Color;
        public readonly ItemEffect[] Affinities;
        public BuildMorphDefinition(BuildMorphKind kind, string name, string silhouette, string pattern, Color color, params ItemEffect[] affinities)
        { Kind = kind; Name = name; Silhouette = silhouette; Pattern = pattern; Color = color; Affinities = affinities; }
    }

    public static class BuildMorphs
    {
        public static readonly BuildMorphDefinition[] All = {
            new BuildMorphDefinition(BuildMorphKind.Thread, "Living Stitchwork", "Spool harness / needle antennae / thread satellites", "Pins a target, then stitches nearby enemies together. Higher forms weave a closed web.", new Color(.35f,.91f,.79f), ItemEffect.AttackRange, ItemEffect.BulletPierce, ItemEffect.BulletSpeed, ItemEffect.CompanionAttackSpeed),
            new BuildMorphDefinition(BuildMorphKind.Cinder, "Paper Furnace", "Paper ember charm / lantern furnace / folded flame fins", "A travelling row of ember vents erupts toward the target. Higher forms branch into a fork.", new Color(1,.72f,.32f), ItemEffect.Damage, ItemEffect.DamageCorrection, ItemEffect.BulletSize, ItemEffect.Knockback),
            new BuildMorphDefinition(BuildMorphKind.Clockwork, "Clockwork Foundry", "Gear key / winding engine / counter-rotating cogs", "Alternating hammer impacts flank the target. The final form adds a delayed crossbeam.", new Color(.91f,.76f,.38f), ItemEffect.MeleeDamage, ItemEffect.CollisionDamage, ItemEffect.Knockback, ItemEffect.Armor),
            new BuildMorphDefinition(BuildMorphKind.Porcelain, "Midnight Tea Service", "Cup charm / saucer shoulders / orbiting porcelain", "Three measured pours make a slowing tea garden. Higher forms add satellite cups.", new Color(.62f,.86f,.95f), ItemEffect.Heal, ItemEffect.HealCorrection, ItemEffect.HeartHealing, ItemEffect.BulletSplit, ItemEffect.BulletSize, ItemEffect.BulletDistance),
            new BuildMorphDefinition(BuildMorphKind.Constellation, "Pocket Constellation", "Star charm / orrery / moving star crown", "Stars close inward from several directions, joining into a constellation around the target.", new Color(.76f,.72f,1), ItemEffect.Projectiles, ItemEffect.RangedDamage, ItemEffect.BulletDistance, ItemEffect.Crit),
            new BuildMorphDefinition(BuildMorphKind.Paper, "Origami Tempest", "Crane charm / folded wings / floating paper petals", "Successive crescent cuts travel outward and push enemies. Higher forms unfold a wider fan.", new Color(.72f,.94f,.69f), ItemEffect.AttackRange, ItemEffect.MoveSpeed, ItemEffect.Dodge, ItemEffect.Knockback),
            new BuildMorphDefinition(BuildMorphKind.Mirror, "Silver Understudy", "Mirror charm / three-panel mirror fan / spectral fragments", "Offset reflections repeat a narrow strike after the original. Higher forms cross their sightlines.", new Color(.76f,.85f,1), ItemEffect.Crit, ItemEffect.Dodge, ItemEffect.AttackSpeed, ItemEffect.BulletBounce),
            new BuildMorphDefinition(BuildMorphKind.Chorus, "Toybox Orchestra", "Bell charm / pipe-organ backpack / note satellites", "Concentric sound waves sweep distinct distance bands. Higher forms add an answering wave.", new Color(.96f,.88f,.59f), ItemEffect.AttackSpeed, ItemEffect.CompanionAttackSpeed, ItemEffect.Roam, ItemEffect.Pickup),
            new BuildMorphDefinition(BuildMorphKind.Magnet, "Polarity Workshop", "Magnet charm / coil rig / orbiting magnetic poles", "A resisted magnetic draw gathers enemies before a delayed discharge. Higher forms add a cross current.", new Color(.45f,.88f,.91f), ItemEffect.Pickup, ItemEffect.Roam, ItemEffect.BulletBounce, ItemEffect.Knockback),
            new BuildMorphDefinition(BuildMorphKind.Tide, "Moonlit Regatta", "Fish charm / fishtail pennants / swimming satellites", "An outward wake returns along a different lane. Higher forms add a broad crossing wake.", new Color(.43f,.91f,.76f), ItemEffect.BulletSpeed, ItemEffect.BulletDistance, ItemEffect.MoveSpeed, ItemEffect.BulletBounce),
            new BuildMorphDefinition(BuildMorphKind.Frost, "Crystal Music Garden", "Crystal charm / crystal shoulders / six-point crown", "Ice spokes unfold around the target and briefly slow ordinary enemies. Higher forms fracture into an outer ring.", new Color(.65f,.93f,1), ItemEffect.BulletPierce, ItemEffect.Armor, ItemEffect.AttackRange, ItemEffect.Health),
            new BuildMorphDefinition(BuildMorphKind.Seal, "Impossible Ledger", "Wax charm / receipt mantle / revolving seals", "A rectangular seal closes over a target, followed by ink strokes across its edges and centre.", new Color(.86f,.72f,.96f), ItemEffect.Income, ItemEffect.Interest, ItemEffect.TreasureDetection, ItemEffect.Luck)
        };
        public static BuildMorphDefinition Get(BuildMorphKind kind) => (int)kind < 1 || (int)kind > All.Length ? null : All[(int)kind - 1];
        public static int Stage(int points, int relics) => relics == 0 ? 0 : points >= 6 ? 3 : points >= 4 ? 2 : points >= 2 ? 1 : 0;
        public static int NextThreshold(int stage) => stage >= 3 ? 6 : (stage + 1) * 2;
        public static string StageName(int stage) => stage == 0 ? "Dormant" : stage == 1 ? "Awakened" : stage == 2 ? "Transformed" : "Masterwork";
        public static BuildMorphKind For(WeaponDefinition w)
        {
            if (w == null || w.Kind == WeaponKind.Contact) return BuildMorphKind.None;
            if (w.Mechanic == CarnivalWeapon.Sewing || w.Family == ExpandedWeapon.Scissors) return BuildMorphKind.Thread;
            if (w.Signature == SignatureWeapon.Stamp) return BuildMorphKind.Seal;
            if (w.Mechanic == CarnivalWeapon.Magnet || w.Mechanic == CarnivalWeapon.YoYo) return BuildMorphKind.Magnet;
            if (w.Family == ExpandedWeapon.Tea) return BuildMorphKind.Porcelain;
            if (w.Family == ExpandedWeapon.Fish || w.Mechanic == CarnivalWeapon.SatelliteDisc) return BuildMorphKind.Tide;
            if (w.Family == ExpandedWeapon.MusicBox || w.Signature == SignatureWeapon.Bell) return BuildMorphKind.Chorus;
            if (w.Family == ExpandedWeapon.Camera || w.Mechanic == CarnivalWeapon.Projector || w.Mechanic == CarnivalWeapon.EchoBlade) return BuildMorphKind.Mirror;
            if (w.Family == ExpandedWeapon.Hammer || w.Family == ExpandedWeapon.Alarm || w.Mechanic == CarnivalWeapon.SpringHammer) return BuildMorphKind.Clockwork;
            if (w.Signature == SignatureWeapon.Umbrella || w.Mechanic == CarnivalWeapon.RibbonWheel || w.Id == "PaperFan" || w.Id == "MoonFeatherFan") return BuildMorphKind.Paper;
            if (w.Element == DamageElement.Ember || w.Family == ExpandedWeapon.Iron) return BuildMorphKind.Cinder;
            if (w.Element == DamageElement.Frost) return BuildMorphKind.Frost;
            return w.Kind == WeaponKind.Projectile ? BuildMorphKind.Constellation : BuildMorphKind.Mirror;
        }
        public static bool Supports(BuildMorphKind kind, WeaponDefinition w)
        {
            BuildMorphKind other = For(w);
            if (other == kind) return true;
            switch (kind)
            {
                case BuildMorphKind.Thread: return other == BuildMorphKind.Magnet || other == BuildMorphKind.Clockwork;
                case BuildMorphKind.Cinder: return other == BuildMorphKind.Paper || other == BuildMorphKind.Clockwork;
                case BuildMorphKind.Clockwork: return other == BuildMorphKind.Magnet || other == BuildMorphKind.Chorus;
                case BuildMorphKind.Porcelain: return other == BuildMorphKind.Frost || other == BuildMorphKind.Clockwork;
                case BuildMorphKind.Constellation: return other == BuildMorphKind.Mirror || other == BuildMorphKind.Chorus;
                case BuildMorphKind.Paper: return other == BuildMorphKind.Cinder || other == BuildMorphKind.Thread;
                case BuildMorphKind.Mirror: return other == BuildMorphKind.Seal || other == BuildMorphKind.Constellation;
                case BuildMorphKind.Chorus: return other == BuildMorphKind.Clockwork || other == BuildMorphKind.Seal;
                case BuildMorphKind.Magnet: return other == BuildMorphKind.Thread || other == BuildMorphKind.Tide;
                case BuildMorphKind.Tide: return other == BuildMorphKind.Magnet || other == BuildMorphKind.Frost;
                case BuildMorphKind.Frost: return other == BuildMorphKind.Porcelain || other == BuildMorphKind.Constellation;
                case BuildMorphKind.Seal: return other == BuildMorphKind.Chorus || other == BuildMorphKind.Mirror;
                default: return false;
            }
        }
        public static bool Affinity(BuildMorphKind kind, ItemDefinition item, bool companion)
        {
            BuildMorphDefinition definition = Get(kind);
            if (definition == null || item == null) return false;
            foreach (StatModifier m in item.EffectiveModifiers)
            {
                if (m.Value <= 0 || m.Scope == (companion ? ModifierScope.Champion : ModifierScope.Companions)) continue;
                if (Array.IndexOf(definition.Affinities, m.Effect) >= 0) return true;
            }
            // Authored connectors have identity as well as stats; one relic still grants one point.
            switch (item.SignatureRole)
            {
                case SignatureRelicKind.ThermalInk: return kind == BuildMorphKind.Cinder;
                case SignatureRelicKind.TeaCoaster: return kind == BuildMorphKind.Porcelain;
                case SignatureRelicKind.ScratchRecord: return kind == BuildMorphKind.Chorus;
                case SignatureRelicKind.ReflectiveInk: case SignatureRelicKind.OfficeLens: return kind == BuildMorphKind.Seal || kind == BuildMorphKind.Mirror;
                case SignatureRelicKind.PatchSpring: return kind == BuildMorphKind.Clockwork;
                default: return false;
            }
        }
    }

}
