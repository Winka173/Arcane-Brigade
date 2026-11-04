using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    public static class ToyboxAudioChecks
    {
        [MenuItem("Arcane Brigade/Audio/Toybox replacement checks")]
        public static void Run()
        {
            ArcaneAudioChecks.Run();
            var notes = new List<string>();
            void Check(bool ok, string caption)
            {
                if (!ok) throw new InvalidOperationException("Toybox audio: " + caption);
                notes.Add("PASS " + caption);
            }
            // the library only grows (real recordings, a sound per hero weapon on 4/11/2026): at least the first set, and no cue
            // without a clip (was an exact count of 206 cues and 582 clips)
            Check(ArcaneCueLibrary.CueCount >= 206, "at least 206 cue definitions (" + ArcaneCueLibrary.CueCount + ")");
            int clips = 0;
            foreach (ArcaneCueDef cue in ArcaneCueLibrary.All) { Check(cue.clips != null && cue.clips.Length > 0, "a clip for " + cue.id); clips += cue.clips.Length; }
            Check(clips >= 582, "at least 582 clips (" + clips + ")");
            Check(ArcaneCueLibrary.Pieces.Count > 0, "adaptive compositions (" + ArcaneCueLibrary.Pieces.Count + ")"); // the music set changes as tracks are replaced (was exactly 15)
            var definition = ScriptableObject.CreateInstance<UnitDefinition>();
            var weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            var subject = new UnitState { Definition = definition, Faction = Faction.Champion };
            try
            {
                foreach (HeroMechanic hero in Enum.GetValues(typeof(HeroMechanic)))
                {
                    if (hero == HeroMechanic.None) continue;
                    definition.HeroMechanic = hero;
                    string id = ArcaneCueLibrary.HeroCue(hero);
                    Check(id != null && ArcaneCueLibrary.Get(id) != null, "hero library: " + hero);
                    if (hero != HeroMechanic.ParcelExpress)
                        Check(ArcaneCueLibrary.EventId(new CombatEvent(CombatEventKind.SignaturePulse, Vector2.zero, Faction.Champion, subject: subject)) == id, "hero pulse route: " + hero);
                }
                Check(ArcaneCueLibrary.EventId(new CombatEvent(CombatEventKind.Fire, Vector2.zero, Faction.Champion,
                    weapon: weapon, subject: subject, cause: new CombatCause(1, 0, "hero-ParcelExpress"))) == "hero.parcel.express", "parcel activation uses its own cue");
                var mechanics = new Dictionary<CarnivalWeapon, string>
                {
                    { CarnivalWeapon.YoYo, "wpn.yoyo.throw" }, { CarnivalWeapon.Magnet, "wpn.magnet.pull" },
                    { CarnivalWeapon.Sewing, "wpn.needle.volley" }, { CarnivalWeapon.SpringHammer, "wpn.spring.slam" },
                    { CarnivalWeapon.Projector, "wpn.projector.cast" }, { CarnivalWeapon.RibbonWheel, "wpn.ribbon.cut" }
                };
                foreach (var pair in mechanics)
                {
                    weapon.Mechanic = pair.Key;
                    Check(ArcaneCueLibrary.EventId(new CombatEvent(CombatEventKind.Fire, Vector2.zero, Faction.Companion, weapon: weapon)) == pair.Value, "weapon route: " + pair.Key);
                }
                weapon.Mechanic = CarnivalWeapon.YoYo;
                Check(ArcaneCueLibrary.EventId(new CombatEvent(CombatEventKind.ProjectileReturn, Vector2.zero, Faction.Champion, weapon: weapon)) == "wpn.yoyo.return", "yo-yo return differs from ordinary boomerang");
                weapon.Mechanic = CarnivalWeapon.None;
                Check(ArcaneCueLibrary.EventId(new CombatEvent(CombatEventKind.ProjectileReturn, Vector2.zero, Faction.Champion, weapon: weapon)) == "wpn.boomerang.return", "ordinary boomerang route retained");
                weapon.Kind = WeaponKind.Projectile;
                subject.Faction = Faction.Enemy;
                definition.Shot = HostileShot.Hook;
                Check(ArcaneCueLibrary.EventId(new CombatEvent(CombatEventKind.Fire, Vector2.zero, Faction.Enemy, weapon: weapon, subject: subject)) == "enemy.shot.arc", "curved hostile shot route");
                definition.Shot = HostileShot.Weave;
                Check(ArcaneCueLibrary.EventId(new CombatEvent(CombatEventKind.Fire, Vector2.zero, Faction.Enemy, weapon: weapon, subject: subject)) == "enemy.shot.spiral", "weaving hostile shot route");
                weapon.ProjectileCount = 3;
                Check(ArcaneCueLibrary.EventId(new CombatEvent(CombatEventKind.Fire, Vector2.zero, Faction.Enemy, weapon: weapon, subject: subject)) == "enemy.shot.burst", "volley route takes priority");
                foreach (string boss in new[] { "ChoirEngine", "CoinWidow", "LastLantern", "MarqueeMaw", "ScarletIndex", "SleepCollector", "SuitcaseLeviathan", "UmbraCradle", "UndeliveredKing", "WaxAdmiral" })
                {
                    definition.Id = boss;
                    Check(ArcaneCueLibrary.EventId(new CombatEvent(CombatEventKind.BossLanded, Vector2.zero, Faction.Enemy, subject: subject)) == "boss.voice." + boss.ToLowerInvariant(), "boss landing voice: " + boss);
                }
                Check(float.IsPositiveInfinity(ToyboxAudioObserver.ChargeRequired(HeroMechanic.None)), "heroes without a charge never announce ready");
                Check(float.IsPositiveInfinity(ToyboxAudioObserver.ChargeRequired(HeroMechanic.RootedOracle)), "automatic ward has no charge-ready sound");
                Check(ToyboxAudioObserver.ChargeRequired(HeroMechanic.TideCollector) == 8, "Tide charge threshold matches gameplay");
            }
            finally { UnityEngine.Object.DestroyImmediate(definition); UnityEngine.Object.DestroyImmediate(weapon); }
            File.WriteAllLines("Library/ToyboxAudioChecks.txt", notes);
            Debug.Log("TOYBOX_AUDIO_CHECKS_PASSED " + notes.Count + " routing checks; listening review remains separate.");
        }
    }
}
