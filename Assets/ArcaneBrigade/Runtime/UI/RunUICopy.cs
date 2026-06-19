using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public static class RunUICopy
    {
        public static string UnitName(UnitDefinition unit) => unit == null ? "Unknown character" : UnitName(unit.Id, unit.DisplayName);
        public static string UnitName(string id, string fallback = null)
        {
            if (!string.IsNullOrEmpty(fallback) && fallback != id) return fallback;
            switch (id)
            {
                // enemies whose display name is still their id (Docs/Enemy_Roster_v2.md §2.5)
                case "HushMite": return "Hush Mite";
                case "ClockBat": return "Clock Bat";
                case "BristleImp": return "Bristle Imp";
                case "RuneWisp": return "Wick Wisp";
                case "InkMoth": return "Ink Moth";
                case "SirenSeal": return "Lullaby Seal";
                case "DoorBearer": return "Door Bearer";
                case "MoonMagnet": return "Moon Magnet";
                case "SleepMimic": return "Sleep Mimic";
                case "CurtainWisp": return "Curtain Wisp";
                case "LanternKnight": return "Lantern Knight";
                case "Postmaster": return "Postmaster";
                case "DrowsySeer": return "Drowsy Seer";
                case "TeapotPilgrim": return "Teapot Pilgrim";
                case "CandleSnail": return "Candle Snail";
                case "MailboxImp": return "Mailbox Imp";
                case "PillowOracle": return "Pillow Oracle";
                case "MushroomLibrarian": return "Mushroom Librarian";
                case "HoodWisp": return "Hood Wisp";
                case "JellySwordsman": return "Jelly Swordsman";
                default: return fallback ?? id;
            }
        }
        public static string WeaponName(string id)
        {
            switch (id)
            {
                case "SpoonSabre": return "Spoon Sabre";
                case "ToastCleaver": return "Toast Cleaver";
                case "LollipopLance": return "Lollipop Lance";
                case "PaperFan": return "Paper Fan";
                case "CorkscrewRapier": return "Corkscrew Rapier";
                case "TeabagFlail": return "Teabag Flail";
                case "UmbrellaHook": return "Umbrella Hook";
                case "PizzaWheel": return "Pizza Wheel";
                case "MoonScythe": return "Moon Scythe";
                case "ForkTrident": return "Fork Trident";
                case "MaracaHammer": return "Maraca Hammer";
                case "TapeWhip": return "Tape Whip";
                case "ButtonBow": return "Button Bow";
                case "LeafCrossbow": return "Leaf Crossbow";
                case "InkPistol": return "Ink Pistol";
                case "BubbleWand": return "Bubble Wand";
                case "StarKazoo": return "Star Kazoo";
                case "ShellCannon": return "Shell Cannon";
                case "AcornSling": return "Acorn Sling";
                case "CrescentBoomerang": return "Crescent Boomerang";
                case "DartCompass": return "Dart Compass";
                case "LanternStaff": return "Lantern Staff";
                case "SnowGlobeOrb": return "Snow Globe Orb";
                case "RibbonHarp": return "Ribbon Harp";
            }
            switch (id)
            {
                case "KeyCleaver": return "Keyblade";
                case "RuneStaff": return "Rune Staff";
                case "ParasolNeedle": return "Parasol Needle";
                case "MailboxLauncher": return "Parcel Launcher";
                case "BellFork": return "Tuning Bell";
                case "DreamStamp": return "Dream Stamp";
                case "TeapotPopgun": return "Teapot Popgun";
                case "CrescentBow": return "Crescent Bow";
                case "CloudScissors": return "Cloud Scissors";
                case "SleepCamera": return "Sleep Camera";
                case "TinFish": return "Tin Fish";
                case "DreamIron": return "Dream Iron";
                case "RubberHammer": return "Rubber Hammer";
                case "AlarmMortar": return "Alarm Mortar";
                case "MusicBox": return "Music Box";
                case "DreamOrbStaff": return "Dream Orb Staff";
                case "FireExtinguisher": return "Fire Extinguisher";
                case "PlasmaTrident": return "Plasma Trident";
                case "KiteSpool": return "Kite Spool";
                case "ScoopLauncher": return "Scoop Launcher";
                case "CuckooCannon": return "Cuckoo Cannon";
                case "JugglingPins": return "Juggling Pins";
                case "SonarHarpoon": return "Sonar Harpoon";
                case "MoonFeatherFan": return "Moon Feather Fan";
                case "TwinSabres": return "Twin Sabres";
                case "ClockCrescent": return "Clock Crescent";
                case "UmbrellaLance": return "Umbrella Lance";
                case "StampHammer": return "Stamp Hammer";
                default: return string.IsNullOrEmpty(id) ? "Unknown weapon" : System.Text.RegularExpressions.Regex.Replace(id, "(?<=[a-z])(?=[A-Z])", " "); // SpinningTop reads Spinning Top
            }
        }
        public static string WeaponKindName(WeaponKind kind) => kind == WeaponKind.Projectile ? "Ranged" : kind == WeaponKind.Melee ? "Melee" : "Contact";
        public static string ElementName(DamageElement element) => element == DamageElement.Ember ? "Ember" : element == DamageElement.Frost ? "Frost" : element == DamageElement.Arcane ? "Arcane" : "Physical";
        public static string UnlockCondition(UnitDefinition unit, int bestWave) => bestWave >= unit.UnlockWave ? "" : "Unlock: reach wave " + unit.UnlockWave;
        public static string ChampionDescription(UnitDefinition unit)
        {
            var identity = HeroIdentities.For(unit.Id);
            if (identity == null) return LegacyChampionDescription(unit);
            string innate = unit.Id switch {
                "LanternKnight" => "Start with +5 HP, +5% movement speed and 0.2 HP/s. Rally strengthens personal attacks with your brigade.",
                "Postmaster" => "Team flat ranged damage +3 and melee damage -2. After 25s, team melee attacks deal 40% more and ranged attacks 25% less.",
                "DrowsySeer" => "Basic attacks work while moving or resting; rest to recover 5% max HP each second. Friends strengthen your personal shots and body while their own damage is reduced slightly.",
                "RootOracle" => "Stationary in battle; walks in shops. Whole-arena pickup, 30% shop discount, no dodge and -100% treasure detection.",
                "LoneJelly" => "Fight without friend bodies. Buying friends absorbs their talents. Solo weapon damage grows with wave, within its cap.",
                "GlassDice" => "Uses the equipped weapon normally. Half of positive Luck also grants team damage, up to 50%. Collected hearts add run Luck.",
                "CometRunner" => "Team movement speed +20%; start with +10% dodge and crit, but armor -0.15.",
                "MirrorKeeper" => "Each Prizefighter bond level adds 3 hero collision damage. Standing still prepares the borrowed volley.",
                "StormFencer" => "Equipped weapon attacks normally. Hero collision damage doubles. Start with +20% dodge.",
                "TinVirtuoso" => "Start with +10% crit and 2 projectiles. Hero shots cannot split. Friends use their weapons and also answer conducted phrases.",
                "LanternWeaver" or "HourglassStag" or "RainArchivist" or "SupperMortar" => "Base weapon and all listed run modifiers still apply.",
                "KiteAdmiral" => "Kite Spool: a kite loops ahead, its string cuts, then it dives. Starts with Paper Plane Pigeon.",
                "SundaeDuchess" => "Scoop Launcher: scoops splat and slow; the frost scoop freezes. Starts with Baker Mouse.",
                "BonsaiClocksmith" => "Cuckoo Cannon: pendulum shots; every third, a cuckoo dives on the farthest enemy. Starts with Top Beetle.",
                "JugglingJester" => "Juggling Pins: each pin strikes going up and coming down. Starts with Lens Owl.",
                "BellDiver" => "Sonar Harpoon: pierces two and probes them. Starts with Raincoat Frog.",
                _ => OctoberContent.HeroCopy(unit.ReferenceHero)
            };
            return HeroIdentities.Describe(unit.Id) + "\n\nINNATE TRAITS\n" + innate;
        }
        private static string LegacyChampionDescription(UnitDefinition unit)
        {
            if (!string.IsNullOrEmpty(unit.ReferenceHero)) return unit.Description == OctoberContent.HeroCopy(unit.ReferenceHero) ? unit.Description : unit.Description + "\n\n" + OctoberContent.HeroCopy(unit.ReferenceHero);
            if (unit.HeroMechanic >= HeroMechanic.RootedOracle) return K11HeroRules.Describe(unit);
            if (unit.ChampionModifiers != null && unit.ChampionModifiers.Length > 0) return unit.Description + (unit.HeroMechanic == HeroMechanic.None ? "" : "\n\n" + K11HeroRules.Describe(unit));
            switch (unit.Id)
            {
                case "LanternKnight": return "A melee pioneer with a keyblade. Damage increases with the brigade's size; suited to large rosters.";
                case "Postmaster": return "Launches parcels from range and scavenges extra gold in combat. Suited to saving up for shops.";
                case "DrowsySeer": return "Attacks with a tuning bell and heals periodically. Moves slower, but has more health to endure.";
                default: return WeaponKindName(unit.Weapon.Kind) + " · " + PassiveDescription(unit.Passive) + "\n" + unit.Description;
            }
        }
        public static string PassiveDescription(PassiveKind passive)
        {
            switch (passive)
            {
                case PassiveKind.Rally: return "Damage scales with friend count";
                case PassiveKind.HealChampion: return "Automatically heals the hero every 4 seconds";
                case PassiveKind.Scavenge: return "Scavenges extra gold every 4 seconds in combat";
                case PassiveKind.ChillAura: return "Chills nearby enemies";
                default: return "No personal passive";
            }
        }
        public static string BondDescription(BondDefinition bond, int count)
        {
            int threshold = Mathf.Max(1, bond.Threshold);
            int level = bond.ActiveLevel(count);
            string result = bond.Name + " · " + (level > 0 ? "Activated ×" + level : "Not active yet")
                + "\n" + count + "/" + bond.NextThreshold(count) + " friends · every " + threshold + " friends adds a level"
                + "\n" + RunModifierRules.Line(bond.Effect, RunModifierRules.DefaultScope(bond.Effect), bond.Reward(count));
            foreach (BondThresholdBonus bonus in bond.ExtraBonuses ?? new BondThresholdBonus[0])
                result += "\nAt " + bonus.Threshold + " friends: " + RunModifierRules.ValueText(bond.Effect, bond.BalanceReward(bonus.Value)) + " more" + (count >= bonus.Threshold ? " ✓" : "");
            return result + "\nEach friend counts once, whatever its stars.";
        }
        public static string PhaseName(SessionPhase phase) => phase == SessionPhase.Combat ? "In combat" : phase == SessionPhase.Shop ? "Resting and shopping" : phase == SessionPhase.Victory ? "Victorious" : "Finished";
        public static string TriggerName(ProcTrigger trigger) => trigger == ProcTrigger.CoinCollected ? "collect gold" : trigger == ProcTrigger.ChestOpened ? "open chests" : "defeat enemies";
        public static string ActionName(ProcAction action) => action == ProcAction.HealChampion ? "heal the hero" : "deal area damage";
        public static string RecordName(string key, RunCatalog catalog)
        {
            string[] parts = (key ?? "").Split('/');
            if (parts.Length < 3 || !int.TryParse(parts[1], out int difficulty)) return key;
            return UnitName(parts[0], catalog.Unit(parts[0])?.DisplayName) + " · " + RunMeta.DifficultyNames[Mathf.Clamp(difficulty, 0, RunMeta.DifficultyNames.Length - 1)] + " · " + (parts[2] == "endless" ? "Endless" : "Journey") + (parts.Length > 3 && !string.IsNullOrEmpty(parts[3]) ? " · " + AchievementName("challenge-" + parts[3]) : "");
        }
        public static string AchievementName(string id)
        {
            if (id == "Brigade dawn") return "Lead the brigade to dawn";
            if (id == "challenge-night-shift") return "Complete Night Shift";
            if (id == "challenge-first-light") return "Complete First Light";
            if (id == "challenge-small-brigade") return "Complete Small Brigade";
            const string prefix = "clear-difficulty-";
            if (id != null && id.StartsWith(prefix) && int.TryParse(id.Substring(prefix.Length), out int difficulty)) return "Win difficulty " + RunMeta.DifficultyNames[Mathf.Clamp(difficulty, 0, RunMeta.DifficultyNames.Length - 1)];
            return id;
        }
        public static string ContributionName(string id, RunCatalog catalog)
        {
            if (string.IsNullOrEmpty(id)) return "Primary attack";
            ItemDefinition item = catalog.Item(id);
            if (item != null) return item.Name;
            switch (id)
            {
                case "primary": return "Primary attack";
                  case "hero-LanternStride": return "LANTERN STRIDE";
                  case "hero-ParcelExpress": return "PARCEL EXPRESS";
                  case "hero-DreamBreath": return "DREAM BREATH";
                case "unit-passive": return "Healing passive";
                case "passive-regen": return "Passive regeneration";
                case "scavenge": return "Automatic scavenging";
                case "element-reaction": return "Elemental reaction";
                case "stamp-burst": return "Stamp burst";
                case "stamp-carbon": return "Mark transfer";
                case "carbon-exposure": return "Weakening";
                case "projectile-bounce": return "Wall bounce";
                case "projectile-ricochet": return "Target bounce";
                case "projectile-split": return "Projectile split";
                case "zipper-link": return "Arrow tether";
                case "heat-status": return "Burn";
                default: return "Effect " + id;
            }
        }
        public static string DamageKind(string kind) => (kind ?? "").Replace("Physical", "Physical").Replace("Ember", "Ember").Replace("Frost", "Frost").Replace("Arcane", "Arcane").Replace("Projectile", "Projectile").Replace("Melee", "Melee").Replace("Contact", "Contact").Replace("arena", "arena");
        public static string DamageSource(string source, RunCatalog catalog)
        {
            foreach (UnitDefinition enemy in catalog.Enemies)
                if (source == enemy.DisplayName || source == enemy.DisplayName + " contact") return UnitName(enemy) + (source.EndsWith(" contact") ? " · contact" : "");
            foreach (UnitDefinition boss in catalog.Bosses)
                if (source == boss.DisplayName || source == boss.DisplayName + " contact") return UnitName(boss) + (source.EndsWith(" contact") ? " · contact" : "");
            return source == "Enemy damage" ? "Enemy attack" : source;
        }
    }

    public enum RunUIDevice { KeyboardMouse, Controller, Touch }
    public static class RunUIHints
    {
        private static int frame = -1;
        private static Vector2 previousPad;
        private static Vector3 previousMouse;
        public static RunUIDevice Device { get; private set; }
        public static void Refresh()
        {
            if (frame == Time.frameCount) return;
            frame = Time.frameCount;
            Vector2 pad = ArcadeNavigation.Pad;
            if (Input.anyKeyDown || (Input.mousePosition - previousMouse).sqrMagnitude > 4) Device = RunUIDevice.KeyboardMouse;
            bool padPressed = false;
            for (int index = 0; index < 20; index++) padPressed |= Input.GetKeyDown((KeyCode)((int)KeyCode.JoystickButton0 + index));
            if (padPressed || pad.sqrMagnitude > 0.16f && (pad - previousPad).sqrMagnitude > 0.01f) Device = RunUIDevice.Controller;
            if (Input.touchCount > 0 || Application.isMobilePlatform) Device = RunUIDevice.Touch;
            previousPad = pad; previousMouse = Input.mousePosition;
        }
        // requirement 22/10/2026: no key or button hints anywhere; a hover label only names what the icon opens
        public static string Details(RunUIDevice device) => "Your brigade";
        public static string Pause(RunUIDevice device) => "Pause";
        public static string Interact(RunUIDevice device) => "USE";
        public static string Menu => Device == RunUIDevice.Controller ? "Stick: select · A: confirm · B: back" : Device == RunUIDevice.Touch ? "Tap to select · drag to scroll" : "Arrows / Tab: select · Enter: confirm · Esc: back";
        public static string DetailsNavigation => Device == RunUIDevice.Controller ? "LB/RB: tab · A/X: next/previous row · Y/B: close" : Device == RunUIDevice.Touch ? "Tap a tab or row · drag to scroll" : "Tab: change tab · ↑/↓: select row · I/Esc: close";
    }

    public sealed class RunBondFeedback
    {
        private string runId;
        private float lastObserved;
        private readonly Dictionary<string, float> rewards = new Dictionary<string, float>();
        private readonly Dictionary<string, float> flashes = new Dictionary<string, float>();
        public void Observe(string identity, BondDefinition[] bonds, Dictionary<string, int> counts, float now)
        {
            if (now < lastObserved) flashes.Clear();
            lastObserved = now;
            bool initial = runId != identity || rewards.Count == 0;
            if (runId != identity) { rewards.Clear(); flashes.Clear(); runId = identity; }
            foreach (BondDefinition bond in bonds)
            {
                float reward = bond.Reward(counts.TryGetValue(bond.Id, out int count) ? count : 0);
                if (!initial && rewards.TryGetValue(bond.Id, out float previous) && reward > previous) flashes[bond.Id] = now + 2;
                if (rewards.TryGetValue(bond.Id, out float before) && reward < before) flashes.Remove(bond.Id);
                rewards[bond.Id] = reward;
            }
        }
        public bool Flashing(string id, float now) => flashes.TryGetValue(id, out float until) && now < until;
        public float Burst(string id, float now)
        {
            if (!flashes.TryGetValue(id, out float until)) return 0;
            float age = now - (until - 2);
            return age >= 0 && age < 0.65f ? Mathf.Sin(age * 18) * Mathf.Exp(-age * 5) : 0;
        }
    }
}
