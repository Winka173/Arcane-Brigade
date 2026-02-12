using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    [Serializable] public sealed class CombatVfxEntry { public string Name; public Sprite[] Frames = Array.Empty<Sprite>(); }
    public sealed class CombatVfxArtwork : ScriptableObject
    {
        public CombatVfxEntry[] Entries = Array.Empty<CombatVfxEntry>();
        private static CombatVfxArtwork current;
        private static Dictionary<string, Sprite[]> lookup;
        public static CombatVfxArtwork Current => current != null ? current : current = Resources.Load<CombatVfxArtwork>("CombatVfxArtwork");
        public static Sprite[] Frames(string name)
        {
            if (name == null || Current == null) return null;
            if (lookup == null) { lookup = new Dictionary<string, Sprite[]>(StringComparer.Ordinal); foreach (var e in Current.Entries) lookup[e.Name] = e.Frames; }
            return lookup.TryGetValue(name, out var frames) ? frames : null;
        }
        public static Sprite Frame(string name, float time, int offset = 0, float fps = 12)
        {
            var frames = Frames(name);
            if (frames == null || frames.Length == 0) return null;
            int index = PresentationSettings.ReducedMotion ? 0 : Mathf.Abs(Mathf.FloorToInt(time * fps) + offset) % frames.Length;
            return frames[index];
        }
        public static bool IsAuthored(Sprite image) => image != null && Current != null && Array.Exists(Current.Entries, e => Array.IndexOf(e.Frames, image) >= 0);
        public static Sprite Projectile(ProjectileState shot, CombatSimulation sim, float time)
            => Frame(ProjectileKey(shot, sim), time, shot?.Id ?? 0);
        public static string ProjectileKey(ProjectileState shot, CombatSimulation sim)
        {
            if (shot == null) return null;
            if (shot.Faction == Faction.Enemy)
            {
                UnitState source = sim?.UnitForPresentation(shot.SourceUnitId);
                if (shot.Sprite?.texture.name == "EnemyAffixEffects_v1")
                    return shot.HostileMotion == HostileShot.Hook ? "BlueFlameBolt" : "RedFlameBolt";
                if (shot.Sprite?.texture.name == "BodyFragments_v1" && source != null)
                {
                    int family = source.Definition.Mutation switch
                    {
                        EnemyMutation.SootPounce or EnemyMutation.RuneSpiral => 0,
                        EnemyMutation.FurnaceFan or EnemyMutation.KegRebound => 1,
                        EnemyMutation.MothMolting or EnemyMutation.CurtainCollapse or EnemyMutation.HydraWeave => 2,
                        EnemyMutation.SporeGarden or EnemyMutation.PufferEcho => 3,
                        EnemyMutation.DoorSlam or EnemyMutation.CabinetScissor or EnemyMutation.CombRake => 4,
                        EnemyMutation.MagnetSalvo or EnemyMutation.CoinBarrage => 5,
                        EnemyMutation.BellEruption or EnemyMutation.SirenEcho => 6, _ => 7
                    };
                    if (source.Definition.Mutation == EnemyMutation.None) family = source.Definition.Behavior switch { EnemyBehavior.Sweeper => 4, EnemyBehavior.Spore => 3, EnemyBehavior.Hopper => 1, EnemyBehavior.Burrower => 6, _ => 0 };
                    return "Mutation_" + family;
                }
                string key = source?.Definition.Id switch
                {
                    "ClockBat" => "ShotH2_ClockBat", "FurnaceToad" => "ShotH3_FurnaceToad", "RuneWisp" => "ShotH4_RuneWisp",
                    "SirenSeal" => "ShotH5_SirenSeal", "RibbonHydra" => "ShotH6_RibbonHydra",
                    "SecretDreamLamb" => "Secret_Dream", "SecretPaperBird" => "Secret_Paper", "SecretPatchBear" => "Secret_Patch", _ => null
                };
                return key ?? (source?.Definition.IsBoss == true ? "BossOrb" : "HostileBolt");
            }
            // Hero identity missiles have their own tokens; leave their exact presentation in charge.
            if (shot.Cause.ItemId?.StartsWith("hero-identity-", StringComparison.Ordinal) == true) return null;
            string friendly = shot.Weapon?.Id switch
            {
                "CrescentBow" => "ArrowBolt", "RuneStaff" => "RuneOrb", "BellFork" => "BellChime", "AcornSling" => "AcornPellet",
                "BubbleWand" => "Bubble", "ButtonBow" => "ButtonDart", "InkPistol" => "InkBullet", "LanternStaff" => "LanternEmber",
                "LeafCrossbow" => "LeafBolt", "MailboxLauncher" => "Letter", "MusicBox" or "RibbonHarp" => "MusicNote",
                "ShellCannon" => "ShellShot", "SnowGlobeOrb" => "GlobeShard", "StarKazoo" => "Star", "TeapotPopgun" => "TeaDrop",
                "TinFish" => "TinFish", "DartCompass" => "CompassDart", "ScrapMagnet" => "MagnetBolt", "DreamProjector" => "FilmFrame", _ => null
            };
            if (friendly != null) return friendly;
            return shot.Sprite == null || shot.Sprite.name == "Projectile" || shot.Sprite.texture.name == "Disc" ? "AllySpark" : null;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void Reset() { current = null; lookup = null; }
    }
}
