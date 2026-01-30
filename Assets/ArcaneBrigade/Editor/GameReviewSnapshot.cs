using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace ArcaneBrigade.Editor
{
    // Read-only export: never rebuilds content, saves assets, launches a run or edits player profiles.
    public static class GameReviewSnapshot
    {
        private static Dictionary<string, object> Fields(object o) => o.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public).OrderBy(f => f.MetadataToken).ToDictionary(f => f.Name, f => f.GetValue(o));
        private static object Ref(UnityEngine.Object o)
        {
            if (o == null) return null;
            var d = new Dictionary<string, object> { { "type", o.GetType().Name }, { "name", o.name }, { "asset", AssetDatabase.GetAssetPath(o) } };
            if (o is UnitDefinition u) d["id"] = u.Id;
            if (o is WeaponDefinition w) d["id"] = w.Id;
            if (o is Sprite s) { d["rect"] = new[] { s.rect.x, s.rect.y, s.rect.width, s.rect.height }; d["pixelsPerUnit"] = s.pixelsPerUnit; }
            if (o is AudioClip a) { d["seconds"] = a.length; d["frequency"] = a.frequency; d["channels"] = a.channels; }
            return d;
        }
        private static string Json(object value)
        {
            if (value == null) return "null";
            if (value is string s) return "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n").Replace("\t", "\\t").Replace("\b", "\\b").Replace("\f", "\\f") + "\"";
            if (value is bool b) return b ? "true" : "false";
            if (value is Enum) return Json(value.ToString());
            if (value is float f) return float.IsFinite(f) ? f.ToString("R", CultureInfo.InvariantCulture) : Json(f.ToString(CultureInfo.InvariantCulture));
            if (value is double n) return double.IsFinite(n) ? n.ToString("R", CultureInfo.InvariantCulture) : Json(n.ToString(CultureInfo.InvariantCulture));
            if (value is decimal || value is byte || value is sbyte || value is short || value is ushort || value is int || value is uint || value is long || value is ulong) return Convert.ToString(value, CultureInfo.InvariantCulture);
            if (value is UnityEngine.Object unity) return Json(Ref(unity));
            if (value is IDictionary map) { var pairs = new List<string>(); foreach (DictionaryEntry e in map) pairs.Add(Json(e.Key.ToString()) + ":" + Json(e.Value)); return "{" + string.Join(",", pairs) + "}"; }
            if (value is IEnumerable list) { var rows = new List<string>(); foreach (object item in list) rows.Add(Json(item)); return "[" + string.Join(",", rows) + "]"; }
            return Json(Fields(value));
        }
        private static object Unit(UnitDefinition u)
        {
            var d = Fields(u); d["AssetPath"] = AssetDatabase.GetAssetPath(u);
            d["ReadableDescription"] = string.IsNullOrEmpty(u.ReferenceHero) ? u.Description : RunUICopy.ChampionDescription(u);
            d["PassiveDescription"] = RunUICopy.PassiveDescription(u.Passive);
            d["HeroMechanicDescription"] = HeroIdentities.For(u.Id) == null ? K11HeroRules.Describe(u) : HeroIdentities.Describe(u.Id);
            d["Motion"] = u.AuthoredMotion == null ? null : Fields(u.AuthoredMotion);
            d["Acting"] = CharacterActingLibrary.Current?.Find(u.Id);
            d["Dialogue"] = string.IsNullOrEmpty(u.ReferenceHero) ? Enumerable.Range(0, 3).Select(i => OctoberDialogue.AllyLine(u, i)).Distinct().ToArray() : Enumerable.Range(0, 2).Select(i => OctoberDialogue.HeroLine(u.ReferenceHero, i)).ToArray();
            return d;
        }
        private static object Weapon(WeaponDefinition w)
        {
            var d = Fields(w); d["AssetPath"] = AssetDatabase.GetAssetPath(w); d["DisplayName"] = RunUICopy.WeaponName(w.Id);
            d["Description"] = SignatureDescription.Describe(w, 1, false); d["Morph"] = BuildMorphs.For(w); return d;
        }
        public static void Export()
        {
            RunCatalog c = Resources.Load<RunCatalog>("RunCatalog"); var root = new Dictionary<string, object>();
            root["ExportedAtVietnam"] = DateTimeOffset.UtcNow.ToOffset(TimeSpan.FromHours(7)).ToString("yyyy-MM-dd HH:mm:ss zzz");
            root["UnityVersion"] = Application.unityVersion;
            root["Catalog"] = Fields(c); root["Arena"] = Fields(c.Arena);
            root["Champions"] = c.Champions.Select(Unit).ToArray(); root["Companions"] = c.Companions.Select(Unit).ToArray();
            root["Enemies"] = c.Enemies.Select(Unit).ToArray(); root["Bosses"] = c.Bosses.Select(Unit).ToArray();
            root["Weapons"] = c.Weapons.Select(Weapon).ToArray();
            root["OtherWeaponAssets"] = AssetDatabase.FindAssets("t:WeaponDefinition", new[] { "Assets/ArcaneBrigade" }).Select(g => AssetDatabase.LoadAssetAtPath<WeaponDefinition>(AssetDatabase.GUIDToAssetPath(g))).Where(w => w != null && !c.Weapons.Contains(w)).Select(Weapon).ToArray();
            root["EnemyEvolution"] = EnemyEvolutionLibrary.Current.Species.Select(e => { var d = Fields(e); d["MutationWave"] = EnemyEvolutionLibrary.MutationWave(e.Id); return d; }).ToArray();
            root["Items"] = c.Items.Select(i => { var d = Fields(i); d["EffectiveModifiers"] = i.EffectiveModifiers; d["EffectiveStackLimit"] = i.StackLimit; d["ReadableEffects"] = RunModifierRules.Describe(i, 1); return d; }).ToArray();
            root["Bonds"] = c.Bonds; root["Recipes"] = c.Recipes; root["BuildLinks"] = c.BuildLinks; root["Morphs"] = BuildMorphs.All;
            root["HeroIdentities"] = HeroIdentities.All;
            root["WaitingChapters"] = MienChoStory.Chapters;
            root["WaitingArrivals"] = c.Champions.ToDictionary(u => u.Id, u => WaitingArrivals.For(u.Id));
            root["WaitingLives"] = c.Companions.ToDictionary(u => u.Id, u => new Dictionary<string, object> { { "Origin", CreatureLives.Origin(u.Id) }, { "Early", CreatureLives.Arc(u.Id, 1) }, { "Middle", CreatureLives.Arc(u.Id, 9) }, { "Late", CreatureLives.Arc(u.Id, 17) } });
            root["Achievements"] = RunAchievements.Definitions(c);
            root["StatGlossary"] = Enum.GetValues(typeof(ItemEffect)).Cast<ItemEffect>().Select(e => new { Key = e.ToString(), Name = RunModifierRules.ModifierName(e), DefaultScope = RunModifierRules.DefaultScope(e).ToString(), Percent = RunModifierRules.ValueText(e, 1).EndsWith("%") }).Select(o => new Dictionary<string, object> { { "Key", o.Key }, { "Name", o.Name }, { "DefaultScope", o.DefaultScope }, { "Percent", o.Percent } }).ToArray();
            root["Modes"] = new Dictionary<string, object> { { "Difficulty", RunMeta.DifficultyNames }, { "ChallengeIds", RunMeta.ChallengeIds }, { "Challenges", RunMeta.ChallengeNames }, { "Seeds", Enumerable.Range(0, RunMeta.ChallengeIds.Length).Select(RunMeta.ChallengeSeed).ToArray() } };
            var baselines = new List<object>();
            foreach (UnitDefinition hero in c.Champions)
            foreach (int wave in new[] { 1, 8, 12, 20, 30 })
            {
                using var s = new RunSession(c); s.Start(hero.Id, 610026); s.State.Wave = wave; s.State.Endless = wave > c.NormalWaves;
                CheckpointCatalog a = s.PrepareCombat(); UnitDefinition u = a.Champion; WeaponDefinition w = u.Weapon;
                baselines.Add(new Dictionary<string, object> { { "HeroId", hero.Id }, { "Wave", wave }, { "Endless", s.State.Endless }, { "Gold", s.State.Gold }, { "Roster", s.State.Roster },
                    { "HP", u.Health }, { "Damage", w.Damage }, { "Cooldown", w.Cooldown }, { "Range", w.Range }, { "Speed", u.Speed }, { "Crit", u.CriticalChance }, { "Armor", a.Armor }, { "Dodge", a.Dodge }, { "Regen", a.PassiveHeal },
                    { "Projectiles", w.ProjectileCount }, { "Pierce", w.ProjectilePierce }, { "Split", w.ProjectileSplit }, { "ContactDamage", u.CollisionDamage }, { "BodyHeight", u.BodyHeight }, { "PickupRadius", a.PickupRadius }, { "CompiledCompanions", a.StartingCompanions.Select(x => new Dictionary<string, object> { { "Id", x.Id }, { "Weapon", x.Weapon.Id }, { "Health", x.Health }, { "Damage", x.Weapon.Damage }, { "Cooldown", x.Weapon.Cooldown }, { "Range", x.Weapon.Range } }).ToArray() } });
            }
            root["HeroBaselines"] = baselines;
            root["HeroShopPools"] = c.Champions.Select(hero => {
                using var s = new RunSession(c); s.Start(hero.Id, 610026);
                return new Dictionary<string, object> { { "HeroId", hero.Id }, { "ThemeEligibleItems", c.Items.Where(s.OctoberItemEligible).Select(i => i.Id).ToArray() } };
            }).ToArray();
            var extra = new List<object>();
            foreach (string guid in AssetDatabase.FindAssets("t:UnitDefinition", new[] { "Assets/ArcaneBrigade" }))
            {
                UnitDefinition u = AssetDatabase.LoadAssetAtPath<UnitDefinition>(AssetDatabase.GUIDToAssetPath(guid));
                if (u != null && !c.Champions.Contains(u) && !c.Companions.Contains(u) && !c.Enemies.Contains(u) && !c.Bosses.Contains(u)) extra.Add(Unit(u));
            }
            root["OtherUnitAssets"] = extra;
            root["BossAnatomy"] = c.Bosses.Concat(new[] { OutsideStageAssets.Current.Boss, NightmareAssets.Current.Customer }).Select(u => BossAnatomy.For(u)).ToArray();
            root["Outside"] = Fields(OutsideStageAssets.Current); root["Nightmare"] = Fields(NightmareAssets.Current); root["SecretAssets"] = Fields(SecretEventAssets.Current);
            var rooms = new List<object>();
            foreach (SecretEventKind kind in Enum.GetValues(typeof(SecretEventKind)))
            {
                using var s = new RunSession(c); s.Start(c.Champions[0].Id, 610026); s.State.Wave = 11; s.State.Phase = SessionPhase.Shop; s.State.Gold = 200; s.State.Eggs = 20;
                ItemDefinition example = c.Items.First(i => i.Rarity == ItemRarity.Rare && i.Mechanic == RelicMechanic.None); s.State.Items.Add(new ItemStack { Id = example.Id, Count = 1 });
                s.State.Secrets.Kind = kind; s.State.Secrets.Stage = SecretEventStage.Choosing; s.State.Secrets.TargetItem = example.Id;
                if (s.State.Roster.Count > 0) { s.State.Secrets.TargetUnit = s.State.Roster[0].UnitId; s.State.Secrets.TargetWeapon = s.State.Roster[0].WeaponId; }
                string early = NightmareDialogue.Room(s.State, kind); s.State.Wave = 17; string late = NightmareDialogue.Room(s.State, kind); s.State.Wave = 11;
                rooms.Add(new Dictionary<string, object> { { "Kind", kind }, { "Title", SecretEventAssets.Titles[(int)kind] }, { "Hint", SecretEventAssets.Hints[(int)kind] }, { "EarlyIntroduction", early }, { "LateIntroduction", late }, { "PreviewWave", 11 }, { "PreviewTargetItem", example.Id }, { "Choices", s.SecretChoices() } });
            }
            root["SecretRooms"] = rooms;
            var endings = new List<object>();
            foreach (NightmareFarewell farewell in new[] { NightmareFarewell.CloseShop, NightmareFarewell.LeaveLight }) foreach (bool optional in new[] { false, true })
            {
                using var s = new RunSession(c); s.Start(c.Champions[0].Id, 610026); s.State.Nightmare.Farewell = farewell;
                s.State.Secrets.LetterSent = s.State.Secrets.RescuedToy = optional; if (optional) s.State.OutsideStage = OutsideStageStatus.Completed;
                endings.Add(new Dictionary<string, object> { { "Choice", farewell }, { "OptionalMemories", optional }, { "Pages", Enumerable.Range(0, farewell == NightmareFarewell.LeaveLight ? 5 : 4).Select(i => NightmareDialogue.Ending(s.State, i)).ToArray() } });
            }
            root["Endings"] = endings;
            var memories = new List<object>();
            foreach (SecretEventKind kind in Enum.GetValues(typeof(SecretEventKind))) foreach (int choice in Enumerable.Range(0, 3))
            {
                using var s = new RunSession(c); s.Start(c.Champions[0].Id, 610026); s.State.Nightmare.ResolvedMask = 1 << (int)kind; s.State.Nightmare.Choices[(int)kind] = choice;
                memories.Add(new Dictionary<string, object> { { "Kind", kind }, { "Choice", choice }, { "Text", NightmareDialogue.Memory(s.State, (int)kind) } });
            }
            root["EventMemories"] = memories;
            root["NarrativeTables"] = typeof(NightmareDialogue).GetFields(BindingFlags.NonPublic | BindingFlags.Static).Where(f => f.FieldType == typeof(string[][])).ToDictionary(f => f.Name, f => f.GetValue(null));
            var constants = new Dictionary<string, object>();
            foreach (Type t in typeof(RunCatalog).Assembly.GetTypes().Where(t => t.Namespace == "ArcaneBrigade" && !t.Name.Contains("Capture") && !t.Name.Contains("Fixture") && !t.Name.Contains("Checks")))
            {
                var fields = t.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).Where(f => f.IsLiteral && !f.IsInitOnly).ToArray();
                if (fields.Length > 0 && !t.IsEnum) constants[t.Name] = fields.ToDictionary(f => f.Name, f => f.GetRawConstantValue());
            }
            root["RuntimeConstants"] = constants;
            root["Enums"] = typeof(RunCatalog).Assembly.GetTypes().Where(t => t.IsEnum && t.Namespace == "ArcaneBrigade").ToDictionary(t => t.Name, t => (object)Enum.GetNames(t));
            Directory.CreateDirectory("Library/GameReview"); File.WriteAllText("Library/GameReview/Snapshot.json", Json(root), new UTF8Encoding(false));
            Debug.Log("GAME_REVIEW_SNAPSHOT " + c.Champions.Length + "/" + c.Companions.Length + "/" + c.Weapons.Length + "/" + c.Enemies.Length + "/" + c.Bosses.Length + "/" + c.Items.Length + "/" + c.Bonds.Length + "/" + c.Recipes.Length);
        }
    }
}
