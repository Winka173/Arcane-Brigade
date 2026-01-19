using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public enum AudioBus { Music, Combat, UI, Foley, Voice, Ambience }

    /// <summary>One data-driven cue (see Tools/Audio/build_sfx.py -> Resources/Audio/Arcane/cues.json).</summary>
    [Serializable]
    public sealed class ArcaneCueDef
    {
        public string id;
        public string bus;
        public int priority;
        public float gainDb;
        public float cooldown;
        public int maxVoices;
        public float pitchJitter;
        public bool loop;
        public bool danger;
        public string[] clips;
        [NonSerialized] public AudioClip[] loaded;
        [NonSerialized] public int last = -1;
        [NonSerialized] public float gain;
        [NonSerialized] public AudioBus busId;
    }

    [Serializable]
    public sealed class ArcaneCueFile
    {
        public int version;
        public ArcaneCueDef[] cues;
    }

    [Serializable]
    public sealed class ArcanePieceDef
    {
        public string id;
        public string title;
        public float bpm;
        public int beatsPerBar;
        public int bars;
        public double loopSeconds;
        public int loopSamples;
        public string[] scenes;
        public string family;
        public string[] stems;
        [NonSerialized] public AudioClip[] loaded;
        public double BarSeconds => loopSeconds / bars;
    }

    [Serializable]
    public sealed class ArcanePieceFile
    {
        public int version;
        public ArcanePieceDef[] pieces;
    }

    /// <summary>
    /// Cue registry + music registry. Adding a character/weapon/map means adding a row to the Python cue table
    /// (or an alias in <see cref="EventId"/>); no code change is needed to add variations to an existing cue.
    /// </summary>
    public static class ArcaneCueLibrary
    {
        public const string SceneMenu = "menu";
        public const string SceneSelect = "select";
        public const string SceneCombatEarly = "combat_early";
        public const string SceneCombatMid = "combat_mid";
        public const string SceneCombatLate = "combat_late";
        public const string SceneBoss = "boss";
        public const string SceneEndless = "endless";
        public const string SceneShopGold = "shop_gold";
        public const string SceneShopDream = "shop_dream";

        private static Dictionary<string, ArcaneCueDef> cues;
        private static ArcanePieceDef[] pieces;
        private static bool tried;
        private static bool musicBroken;

        /// <summary>Switch for regression harnesses that assert the legacy audio path (K8AudioChecks playback).</summary>
        public static bool Enabled = true;
        public static bool Available { get { EnsureLoaded(); return Enabled && cues != null && cues.Count > 0; } }
        public static bool MusicAvailable { get { EnsureLoaded(); return Enabled && !musicBroken && pieces != null && pieces.Length > 0; } }
        public static int CueCount { get { EnsureLoaded(); return cues == null ? 0 : cues.Count; } }
        public static IEnumerable<ArcaneCueDef> All { get { EnsureLoaded(); return cues == null ? (IEnumerable<ArcaneCueDef>)new ArcaneCueDef[0] : cues.Values; } }
        public static IReadOnlyList<ArcanePieceDef> Pieces { get { EnsureLoaded(); return pieces ?? new ArcanePieceDef[0]; } }

        public static void EnsureLoaded()
        {
            if (tried) return;
            tried = true;
            try
            {
                TextAsset cueText = Resources.Load<TextAsset>("Audio/Arcane/cues");
                if (cueText != null)
                {
                    ArcaneCueFile file = JsonUtility.FromJson<ArcaneCueFile>(cueText.text);
                    cues = new Dictionary<string, ArcaneCueDef>(file.cues.Length);
                    foreach (ArcaneCueDef def in file.cues)
                    {
                        def.gain = Mathf.Pow(10f, def.gainDb / 20f);
                        def.busId = ParseBus(def.bus);
                        cues[def.id] = def;
                    }
                }
                TextAsset musicText = Resources.Load<TextAsset>("Audio/Arcane/Music/music");
                if (musicText != null) pieces = JsonUtility.FromJson<ArcanePieceFile>(musicText.text).pieces;
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Arcane audio manifest failed to load; legacy audio stays active. " + exception.Message);
                cues = null; pieces = null;
            }
        }

        public static AudioBus ParseBus(string bus) => Enum.TryParse(bus, out AudioBus parsed) ? parsed : AudioBus.Foley;

        public static ArcaneCueDef Get(string id)
        {
            EnsureLoaded();
            return id != null && cues != null && cues.TryGetValue(id, out ArcaneCueDef def) ? def : null;
        }

        /// <summary>Returns a variation that is never the one played last for this cue (when more than one exists).</summary>
        public static AudioClip Pick(ArcaneCueDef def)
        {
            if (def == null || def.clips == null || def.clips.Length == 0) return null;
            if (def.loaded == null) def.loaded = new AudioClip[def.clips.Length];
            int count = def.clips.Length;
            int index = count == 1 ? 0 : UnityEngine.Random.Range(0, count - 1);
            if (count > 1 && index >= def.last && def.last >= 0) index++;
            index = Mathf.Clamp(index, 0, count - 1);
            if (def.loaded[index] == null) def.loaded[index] = Resources.Load<AudioClip>("Audio/Arcane/" + def.clips[index]);
            if (def.loaded[index] == null) return null;
            def.last = index;
            return def.loaded[index];
        }

        public static AudioClip[] LoadStems(ArcanePieceDef piece)
        {
            if (piece.loaded != null) return piece.loaded;
            var stems = new AudioClip[piece.stems.Length];
            for (int index = 0; index < stems.Length; index++)
            {
                stems[index] = Resources.Load<AudioClip>("Audio/Arcane/" + piece.stems[index]);
                if (stems[index] == null) { musicBroken = true; return null; }
            }
            piece.loaded = stems;
            return stems;
        }

        /// <summary>Pieces tagged with a scene; <paramref name="variant"/> rotates when a scene has several compositions.</summary>
        public static ArcanePieceDef PieceForScene(string scene, int variant)
        {
            EnsureLoaded();
            if (pieces == null) return null;
            ArcanePieceDef first = null;
            int matches = 0;
            foreach (ArcanePieceDef piece in pieces) if (Array.IndexOf(piece.scenes, scene) >= 0) matches++;
            if (matches == 0) return null;
            int want = Mathf.Abs(variant) % matches;
            foreach (ArcanePieceDef piece in pieces)
            {
                if (Array.IndexOf(piece.scenes, scene) < 0) continue;
                if (first == null) first = piece;
                if (want-- == 0) return piece;
            }
            return first;
        }

        public static string SceneFor(SessionPhase phase, bool boss, int wave, int normalWaves, bool endless, bool dreamShop)
        {
            if (phase == SessionPhase.Shop) return dreamShop ? SceneShopDream : SceneShopGold;
            if (phase != SessionPhase.Combat) return SceneMenu;
            if (boss) return SceneBoss;
            if (endless) return SceneEndless;
            float progress = normalWaves <= 0 ? 0f : (wave - 1f) / normalWaves;
            return progress < 0.34f ? SceneCombatEarly : progress < 0.67f ? SceneCombatMid : SceneCombatLate;
        }

        // ------------------------------------------------------------ event -> cue id
        private static readonly string[] MaterialNames = { "paper", "metal", "wax", "fabric", "glass", "wood" };

        /// <summary>
        /// Gameplay event -> stable cue id. Returns null when the event has no sound (or the cue is missing), in which
        /// case the caller falls back to the legacy cue table. Full contract: Docs/Audio/Audio_Integration_Contract.md.
        /// Facts this relies on (verified against CombatSimulation): Fire/Windup carry the ATTACKER's faction, Hit/Kill carry the
        /// TARGET's faction; boss pattern events have no Weapon and no Subject; Hit.Cause.ItemId names secondary damage
        /// ("ember-status", "element-reaction", "arcane-chain", "projectile-split", "expanded-fuse", ...); Cause.RootId is shared by one attack.
        /// </summary>
        public static string EventId(CombatEvent value) => EventId(value, out _);

        public static string EventId(CombatEvent value, out float gainScale)
        {
            gainScale = 1f;
            bool boss = value.Subject != null && value.Subject.Definition != null && value.Subject.Definition.IsBoss;
            bool enemy = value.Faction == Faction.Enemy;
            bool bossPattern = enemy && value.Weapon == null && value.Subject == null;   // ProcessBoss emits bare Windup/Fire
            string cause = value.Cause.ItemId ?? "";
            string id = null;
            switch (value.Kind)
            {
                case CombatEventKind.Windup:
                    if (boss || bossPattern) id = "boss.telegraph";
                    else if (enemy && value.Weapon != null) id = value.Weapon.Kind == WeaponKind.Projectile ? "enemy.spit.windup" : "cmb.windup";
                    else if (value.Weapon != null && value.Weapon.Motion == WeaponMotion.Bow) id = "wpn.bow.draw";
                    else if (value.Faction == Faction.Champion || value.Weapon != null && (value.Weapon.Motion == WeaponMotion.Slam || value.Weapon.Motion == WeaponMotion.Hammer)) id = "cmb.windup";
                    break;
                case CombatEventKind.Fire:
                    if (boss || bossPattern) id = "boss.attack";
                    else id = FireId(value, enemy);
                    if (value.Faction == Faction.Companion) gainScale = 0.7f;   // crowd: allies sit under the hero
                    break;
                case CombatEventKind.Hit:
                    if (value.Faction == Faction.Champion) { id = "plr.hurt"; break; }
                    if (cause == "ember-status") { id = "fx.burn"; break; }
                    if (cause == "element-reaction") { id = "fx.reaction.steam"; break; }
                    if (cause == "arcane-chain") { id = "fx.reaction.arcane"; break; }
                    if (cause == "expanded-fuse") { id = "sig.alarm.boom"; break; }
                    if (cause == "zipper-link" || cause == "expanded-link") { id = "fx.field.zipper"; break; }
                    if (cause == "expanded-officehazard" || cause == "expanded-toyhazard" || cause == "expanded-curtain") { id = "fx.hazard.hit"; break; }
                    if (cause == "projectile-split") gainScale = 0.55f;
                    else if (cause == "projectile-ricochet" || cause == "projectile-bounce") gainScale = 0.8f;
                    if (value.Critical) id = "cmb.crit";
                    else if (value.Subject != null && value.Subject.Definition != null) id = "cmb.hit." + MaterialNames[(int)value.Subject.Definition.Material];
                    else id = "cmb.hit.wood";
                    break;
                case CombatEventKind.Kill:
                    if (value.Faction == Faction.Champion) id = "plr.death";
                    else if (boss) id = "boss.defeat";
                    else if (value.Subject != null && value.Subject.Definition != null)
                        id = IsLargeEnemy(value.Subject.Definition) ? "enemy.die.large" : "enemy.die." + MaterialNames[(int)value.Subject.Definition.Material];
                    else id = "enemy.die.wood";
                    break;
                case CombatEventKind.Pickup:
                    id = value.Loot == LootKind.Egg ? "loot.egg" : value.Loot == LootKind.Heart ? "loot.heart" : "loot.coin.pickup";
                    break;
                case CombatEventKind.Heal: id = "plr.heal"; break;
                case CombatEventKind.ChestOpened: id = "loot.chest.open"; break;
                case CombatEventKind.Dodge: id = "plr.dodge"; break;
                case CombatEventKind.SignaturePulse:
                case CombatEventKind.SignatureBurst:
                    if (value.Weapon == null && value.Subject != null && value.Faction == Faction.Champion)   // K11 hero mechanic pulse
                        id = value.Element == DamageElement.Ember ? "hero.pulse.ember" : value.Element == DamageElement.Frost ? "hero.pulse.frost" : null;
                    else if (value.Weapon != null)
                        id = value.Weapon.Signature == SignatureWeapon.Bell ? "sig.bell" : value.Weapon.Signature == SignatureWeapon.Stamp ? "sig.stamp" : value.Weapon.Signature == SignatureWeapon.Umbrella ? "sig.umbrella" : null;
                    break;
                case CombatEventKind.Proc:
                    id = "up.chain";
                    break;
                case CombatEventKind.ProjectileReturn: id = "wpn.boomerang.return"; break;
                case CombatEventKind.ProjectilePierce: id = "wpn.pierce"; break;
                case CombatEventKind.ProjectileSplit: id = "wpn.split"; break;
                case CombatEventKind.ProjectileBounce: id = "wpn.bounce"; break;
                case CombatEventKind.Knockback: id = "fx.knockback"; break;
                case CombatEventKind.BossPhase: id = "boss.phase"; break;
                case CombatEventKind.ContactWindup: id = "enemy.lunge"; break;
                case CombatEventKind.StatusApplied: id = value.Element == DamageElement.Frost ? "fx.chill" : "fx.burn"; break;
                case CombatEventKind.LootAttract: id = "loot.coin.vacuum"; break;
                case CombatEventKind.MimicWake: id = "enemy.mimic.wake"; break;
                case CombatEventKind.LootDrop:
                    id = value.Loot == LootKind.Coin ? "loot.coin.drop" : value.Loot == LootKind.Chest ? "loot.chest.appear" : null;
                    break;
            }
            return id != null && Get(id) != null ? id : null;
        }

        /// <summary>Large non-boss enemies (WardrobeCrab 0.55, FurnaceToad 0.48 radius). BodyScale is a player stat, not an enemy size.</summary>
        public static bool IsLargeEnemy(UnitDefinition definition) => definition != null && !definition.IsBoss && definition.Radius >= 0.45f;

        private static string FireId(CombatEvent value, bool enemy)
        {
            WeaponDefinition weapon = value.Weapon;
            if (weapon == null) return null;
            if (enemy) return weapon.Kind == WeaponKind.Projectile ? "enemy.spit.fire" : "enemy.lunge";
            if (weapon.Family == ExpandedWeapon.Fish) return "wpn.boomerang.fly";   // TinFish, CrescentBoomerang, ClockCrescent all return to the owner
            switch (weapon.Family)
            {
                case ExpandedWeapon.Tea: return "sig.tea";
                case ExpandedWeapon.Alarm: return "sig.alarm";
                case ExpandedWeapon.TetherBow: return "wpn.bow.release";
            }
            switch (weapon.Motion)
            {
                case WeaponMotion.Slash: return "cmb.swing.blade";
                case WeaponMotion.Thrust: return "cmb.swing.spear";
                case WeaponMotion.Slam: return "cmb.swing.hammer";
                case WeaponMotion.Recoil: return weapon.ProjectileCount >= 3 || weapon.ProjectileRadius >= 0.08f ? "wpn.launcher" : "wpn.gun.pop";
                case WeaponMotion.Bell: return "sig.bell";
                case WeaponMotion.Bow: return "wpn.bow.release";
                case WeaponMotion.Cast: return weapon.Element == DamageElement.Ember ? "wpn.cast.fire" : weapon.Element == DamageElement.Frost ? "wpn.cast.ice" : "wpn.cast.arcane";
                case WeaponMotion.Stamp: return "sig.stamp";
                case WeaponMotion.Scissors: return "sig.scissors";
                case WeaponMotion.Camera: return "sig.camera";
                case WeaponMotion.Iron: return "sig.iron";
                case WeaponMotion.Hammer: return "sig.hammer";
                case WeaponMotion.Music: return "sig.musicbox";
            }
            return weapon.Kind == WeaponKind.Melee ? "cmb.swing.blade" : "wpn.gun.pop";
        }

        /// <summary>Legacy 0..38 cue index -> new cue id (UI / world calls that still pass integers).</summary>
        public static string LegacyId(int cue)
        {
            switch (cue)
            {
                case 0: return "cmb.swing.blade";
                case 1: return "wpn.bow.release";
                case 2: return "wpn.gun.pop";
                case 3: return "cmb.hit.wood";
                case 4: return "loot.coin.pickup";
                case 5: return "enemy.die.wood";
                case 6: return "boss.defeat";
                case 7: return "sig.bell";
                case 8: return "sig.stamp";
                case 9: return "sig.umbrella";
                case 10: return "sig.tea";
                case 11: return "wpn.bow.release";
                case 12: return "sig.scissors";
                case 13: return "sig.camera";
                case 14: return "sig.fish";
                case 15: return "sig.iron";
                case 16: return "sig.hammer";
                case 17: return "sig.alarm";
                case 18: return "sig.musicbox";
                case 19: return "ui.select";
                case 20: return "shop.buy.gold";
                case 21: return "shop.reroll";
                case 22: return "shop.invalid";
                case 23: return "up.merge";
                case 24: return "boss.telegraph";
                case 25: return "plr.lowhp";
                case 26: return "plr.hurt";
            }
            if (cue >= 27 && cue <= 32) return "enemy.die." + MaterialNames[cue - 27];
            if (cue >= 33 && cue <= 38) return "cmb.hit." + MaterialNames[cue - 33];
            return null;
        }
    }
}
