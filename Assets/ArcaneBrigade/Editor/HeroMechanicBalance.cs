using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>Measures what each hero identity mechanic actually adds in production waves. For every hero x approach
    /// it plays waves 5/10/15 (boss removed, 60s cap, two seeds) with a standard themed brigade and relic set, using
    /// weapon-range steering with a short rest every few seconds (so stand-still mechanics can trigger),
    /// then repeats the same fights with the mechanic switched off. Value is expressed in W-per-second (W = the hero's
    /// prepared weapon damage): net mechanic damage (minus damage it merely moved or deferred, plus bonus it added to
    /// basics), plus healing per max HP and half of prevented shot damage valued at 20W per max HP, and 0.6W per gold.
    /// Run: -executeMethod ArcaneBrigade.Editor.HeroMechanicBalance.Run [-hmb-label NAME]
    /// Output: PreviewExports/HeroMechanicBalance/NAME/{Runs.tsv,Summary.tsv,Summary.md}</summary>
    public static class HeroMechanicBalance
    {
        private static readonly int[] Waves = { 5, 10, 15 };
        private static readonly int[] Seeds = { 171017, 90210 };
        // 10% of max HP restored is worth 2W; a removed hostile shot counts half its damage (it might have missed); 1 gold = 0.6W
        private const float HpWorth = 20f, PreventHit = .5f, GoldValue = .6f;

        private sealed class Sample
        {
            public string Hero; public int Approach, Wave, Seed; public float W, MaxHp, Elapsed, Hp, HpOff, Value, Share, Damage, Net, Heal, Prevented, Gold, Basic, Casts, Kills, KillsOff, Total, TotalOff; public bool Dead, DeadOff;
        }

        public static void Run()
        {
            string[] args = Environment.GetCommandLineArgs(); int at = Array.IndexOf(args, "-hmb-label");
            string label = at >= 0 && at + 1 < args.Length ? args[at + 1] : "Current";
            string only = null; int ho = Array.IndexOf(args, "-hmb-heroes"); if (ho >= 0 && ho + 1 < args.Length) only = args[ho + 1];
            string root = "PreviewExports/HeroMechanicBalance/" + label; Directory.CreateDirectory(root);
            var catalog = Resources.Load<RunCatalog>("RunCatalog");
            var rows = new List<string> { "hero\tapproach\twave\tseed\tW\tmax_hp\tseconds\tcasts\tmech_damage\tnet_damage\tmech_heal\tprevented_hp\tgold\tbasic_damage\tvalue_W\tvalue_Wps\tbasic_Wps\tshare_of_team\thp_on\thp_off\tdead_on\tdead_off\tkills_on\tkills_off\tteam_on\tteam_off" };
            var samples = new List<Sample>();
            var offCache = new Dictionary<string, Sample>();
            foreach (UnitDefinition hero in catalog.Champions)
            {
                if (only != null && Array.IndexOf(only.Split(','), hero.Id) < 0) continue;
                for (int approach = 0; approach < 3; approach++)
                    foreach (int wave in Waves)
                        foreach (int seed in Seeds)
                        {
                            string key = hero.Id + "/" + wave + "/" + seed;
                            if (!offCache.TryGetValue(key, out Sample off)) { off = Play(catalog, hero, 0, wave, seed, false); offCache[key] = off; }
                            Sample on = Play(catalog, hero, approach, wave, seed, true);
                            on.HpOff = off.Hp; on.DeadOff = off.Dead; on.KillsOff = off.Kills; on.TotalOff = off.Total;
                            samples.Add(on);
                            rows.Add(string.Join("\t", hero.Id, approach, wave, seed, F(on.W), F(on.MaxHp), F(on.Elapsed), on.Casts, F(on.Damage), F(on.Net), F(on.Heal), F(on.Prevented), F(on.Gold), F(on.Basic), F(on.Value), F(on.Value / Mathf.Max(1, on.Elapsed)), F(on.Basic / on.W / Mathf.Max(1, on.Elapsed)), F(on.Share), F(on.Hp), F(on.HpOff), on.Dead ? 1 : 0, on.DeadOff ? 1 : 0, on.Kills, on.KillsOff, F(on.Total), F(on.TotalOff)));
                        }
                File.WriteAllLines(root + "/Runs.tsv", rows);
                Debug.Log("HERO_MECHANIC_BALANCE " + hero.Id);
            }
            // per hero x approach: mean W-per-second value over waves and seeds; median across all 93
            var groups = samples.GroupBy(s => s.Hero + "\t" + s.Approach).Select(g => new
            {
                Key = g.Key, Wps = g.Average(s => s.Value / Mathf.Max(1, s.Elapsed)), Share = g.Average(s => s.Share),
                Casts = g.Average(s => s.Casts / Mathf.Max(1, s.Elapsed) * 60), Basic = g.Average(s => s.Basic / s.W / Mathf.Max(1, s.Elapsed)), Hp = g.Average(s => s.Hp), HpOff = g.Average(s => s.HpOff),
                Dead = g.Count(s => s.Dead), DeadOff = g.Count(s => s.DeadOff), Kills = g.Average(s => s.Kills), KillsOff = g.Average(s => s.KillsOff), N = g.Count()
            }).ToList();
            float median = Median(groups.Select(g => g.Wps).ToList());
            var summary = new List<string> { "hero\tapproach\tvalue_Wps\tratio_to_median\tvs_basic\tshare_of_team\tcasts_per_min\thp_on\thp_off\tdeaths_on\tdeaths_off\tkills_on\tkills_off\tflag" };
            var md = new List<string> { "# Hero mechanic balance — " + label, "", "Median mechanic value: **" + F(median) + " W/s** (W = hero weapon damage). Target band 0.6x–1.6x median.", "", "| Hero | Approach | W/s | x median | vs own basics | team share | casts/min | HP on/off | deaths on/off | flag |", "|---|---|---|---|---|---|---|---|---|---|" };
            int outside = 0;
            foreach (var g in groups)
            {
                float ratio = median <= 0 ? 0 : g.Wps / median;
                string flag = ratio > 1.6f ? "HIGH" : ratio < .6f ? "LOW" : "ok"; if (flag != "ok") outside++;
                summary.Add(string.Join("\t", g.Key, F(g.Wps), F(ratio), F(g.Basic > 0 ? g.Wps / g.Basic : 0), F(g.Share), F(g.Casts), F(g.Hp), F(g.HpOff), g.Dead, g.DeadOff, F(g.Kills), F(g.KillsOff), flag));
                string[] k = g.Key.Split('\t');
                md.Add("| " + k[0] + " | " + k[1] + " | " + F(g.Wps) + " | " + F(ratio) + " | " + (g.Basic > 0 ? (g.Wps / g.Basic * 100).ToString("0", CultureInfo.InvariantCulture) + "%" : "-") + " | " + (g.Share * 100).ToString("0.0", CultureInfo.InvariantCulture) + "% | " + F(g.Casts) + " | " + F(g.Hp) + " / " + F(g.HpOff) + " | " + g.Dead + " / " + g.DeadOff + " | " + flag + " |");
            }
            md.Insert(3, "Outside band: **" + outside + " / " + groups.Count + "**. Spread (max/min of non-zero): " + F(groups.Where(g => g.Wps > 0).Select(g => g.Wps).DefaultIfEmpty(0).Max() / Mathf.Max(.0001f, groups.Where(g => g.Wps > 0).Select(g => g.Wps).DefaultIfEmpty(1).Min())) + ".");
            File.WriteAllLines(root + "/Summary.tsv", summary);
            File.WriteAllLines(root + "/Summary.md", md);
            File.WriteAllText(root + "/Method.txt", "Waves " + string.Join("/", Waves) + ", seeds " + string.Join("/", Seeds) + ", boss removed, 60s cap, difficulty 0. Standard brigade: first 4 themed, non-story, non-gift companions unlocked by the wave (tier 1 at wave 5, tier 2 later); SoloMaw has no roster and three eligible absorbed memories instead. Relics: wave 5 two, wave 10 four, wave 15 six from a fixed list. Weapon-range steering with a 1.8s rest in every 6s when nothing is within 1.6 units. Value = net mechanic damage (minus damage it only moved or deferred, plus bonus it added to basics) + healing and half of removed hostile-shot damage valued at 20W per max-HP + 0.6W per gold, divided by W and seconds. vs_basic compares with the hero's own primary damage per W per second. These are controlled fixtures, not natural campaign clears; LOW/HIGH flags are review prompts, not proof of imbalance for conditional utility.\n");
            Debug.Log("HERO_MECHANIC_BALANCE_COMPLETE median=" + F(median) + " outside=" + outside + "/" + groups.Count);
        }

        private static float Median(List<float> values) { values.Sort(); if (values.Count == 0) return 0; int n = values.Count; return n % 2 == 1 ? values[n / 2] : (values[n / 2 - 1] + values[n / 2]) * .5f; }
        private static string F(float v) => v.ToString("0.###", CultureInfo.InvariantCulture);
        private static readonly string[] Relics = { "key-oil", "warm-tea", "pillow-mail", "brigade-banner", "twin-seal", "royal-key" };

        private static Sample Play(RunCatalog catalog, UnitDefinition hero, int approach, int wave, int seed, bool identity)
        {
            using var session = new RunSession(catalog);
            session.Start(hero.Id, seed); session.State.Wave = wave; session.State.Gold = 60;
            session.State.HeroIdentity.Approach = approach;
            session.State.Roster.Clear();
            var allies = catalog.Companions.Where(u => !u.GiftOnly && !u.SpecialRecruit && u.UnlockWave <= wave && Array.IndexOf(ActOneCanon.StoryIds, u.Id) < 0 && K11HeroRules.Eligible(hero, u)).Take(4).ToList();
            if (hero.HeroMechanic != HeroMechanic.SoloMaw)
                foreach (var ally in allies) session.State.Roster.Add(new RosterEntry { UnitId = ally.Id, WeaponId = ally.Weapon.Id, Tier = wave >= 10 ? 2 : 1 });
            if (hero.Id == "LoneJelly")
                foreach (var memory in catalog.Companions.Where(u => !u.GiftOnly && !u.SpecialRecruit && u.UnlockWave <= wave && K11HeroRules.Eligible(hero, u) && Array.IndexOf(ActOneCanon.StoryIds, u.Id) < 0).Take(3)) session.State.AbsorbedSkills.Add(memory.Id);
            int relics = wave >= 15 ? 6 : wave >= 10 ? 4 : 2;
            foreach (string id in Relics.Take(relics)) if (catalog.Item(id) != null) session.State.Items.Add(new ItemStack { Id = id, Count = 1 });
            CheckpointCatalog combat = session.PrepareCombat();
            combat.Boss = null;
            if (!identity) { combat.EnableHeroIdentities = false; combat.EnableHeroMechanics = false; }
            var sim = new CombatSimulation(combat); sim.Start(unchecked(seed + wave * 397));
            float w = Mathf.Max(1, combat.Champion.Weapon.Damage);
            for (int tick = 0; tick < 1200 && sim.Phase == RunPhase.Combat; tick++) sim.Step(.05f, Steer(sim));
            var sample = new Sample { Hero = hero.Id, Approach = approach, Wave = wave, Seed = seed, W = w, MaxHp = Mathf.Max(1, sim.Champion.Definition.Health), Elapsed = Mathf.Max(1, sim.Elapsed) };
            sample.Hp = Mathf.Clamp01(sim.Champion.Health / Mathf.Max(1, sim.Champion.Definition.Health));
            sample.Dead = !sim.Champion.Alive || sim.Phase == RunPhase.Defeat; sample.Kills = sim.Kills;
            foreach (var entry in sim.Contributions)
            {
                sample.Total += entry.Damage;
                if (entry.EffectId != null && entry.EffectId.StartsWith("hero-identity-", StringComparison.Ordinal)) { sample.Damage += entry.Damage; sample.Heal += entry.Healing; }
                else if (entry.UnitId == hero.Id && entry.EffectId == "primary") sample.Basic += entry.Damage;
            }
            if (identity)
            {
                float net = sample.Damage - sim.HeroIdentityReplacedDamage - sim.HeroIdentityDeferredDamage + sim.HeroIdentityBonusDamage;
                sample.Net = net; sample.Prevented = sim.HeroIdentityPrevented; sample.Gold = sim.HeroIdentityGold;
                sample.Value = net / w + HpWorth * (sample.Heal + PreventHit * sample.Prevented) / sample.MaxHp + GoldValue * sample.Gold;
                sample.Share = sample.Total > 0 ? Mathf.Max(0, net) / sample.Total : 0;
                sample.Casts = sim.HeroIdentityActivations;
            }
            return sample;
        }

        /// <summary>Weapon-range steering, plus a short rest every 6s while nothing is in reach.</summary>
        private static Vector2 Steer(CombatSimulation sim)
        {
            Vector2 move = BuildBalanceDiagnostics.Steer(sim);
            if (Mathf.Repeat(sim.Elapsed, 6) < 4.2f) return move;
            foreach (UnitState u in sim.Units) if (u.Faction == Faction.Enemy && u.Alive && (u.Position - sim.Champion.Position).sqrMagnitude < 2.56f) return move;
            return Vector2.zero;
        }
    }
}
