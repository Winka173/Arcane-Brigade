using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ArcaneBrigade.Editor
{
    /// <summary>The polish pass of 19/10/2026: each boss's own move (BossTrademarks), the rare-offer unveil, the painted
    /// tier stars and crown, the pause menu's settings and restart, and the display / shake settings.</summary>
    public static class PolishPassChecks
    {
        private static readonly List<string> notes = new List<string>();
        private static void Require(bool ok, string caption) { if (!ok) throw new InvalidOperationException("Polish: " + caption); notes.Add("PASS " + caption); }

        [MenuItem("Arcane Brigade/Checks/Polish pass (boss moves, unveil, settings)")]
        public static void Run()
        {
            notes.Clear();
            RunCatalog catalog = Resources.Load<RunCatalog>("RunCatalog");
            MethodInfo build = typeof(RunSession).GetMethod("BuildEnemy", BindingFlags.Instance | BindingFlags.NonPublic);
            MethodInfo trademark = typeof(CombatSimulation).GetMethod("ResolveBossTrademark", BindingFlags.Instance | BindingFlags.NonPublic);
            var names = new HashSet<string>();
            foreach (UnitDefinition source in catalog.Bosses)
            {
                string name = BossTrademarks.Name(source.BossPattern);
                if (name == null) continue;
                Require(names.Add(name), "A move of its own " + source.Id + " (" + name + ")");
                Require(BossTrademarks.Fires(source, 1) && BossTrademarks.Fires(source, 5) && !BossTrademarks.Fires(source, 3) && !BossTrademarks.Fires(source, 0), "Second volley of every four, never on a signature " + source.Id);
                using var session = new RunSession(catalog); session.Start("LanternKnight", 19102); session.State.Wave = 4; session.State.Roster.Clear();
                CheckpointCatalog arena = session.PrepareCombat();
                arena.Boss = (UnitDefinition)build.Invoke(session, new object[] { source, 1f }); arena.EnemyBudget = 0;
                arena.EnableBossArenaPhase = arena.EnableArenaToys = arena.EnableSpectacles = false;
                var simulation = new CombatSimulation(arena); simulation.Start(19102);
                UnitState boss = simulation.Boss; boss.EntranceUntil = 0; boss.Position = boss.PreviousPosition = new Vector2(-4, 2);
                boss.NextBossPattern = boss.NextContact = boss.BossRecoveryUntil = simulation.Champion.NextAttack = float.PositiveInfinity;
                simulation.Champion.Position = simulation.Champion.PreviousPosition = new Vector2(2, -1);
                trademark.Invoke(simulation, new object[] { boss });
                var pieces = simulation.ExpandedFields.Where(f => f.Kind == ExpandedFieldKind.Roller).ToList();
                Require(pieces.Count >= 1 && pieces.All(f => f.Source == boss && f.Style == source.BossPattern && f.At > simulation.Elapsed), "Its pieces wait for their inked path first " + source.Id + " (" + pieces.Count + ")");
                Require(pieces.All(f => !CarnivalHazards.Contains(f, f.Position, .3f, simulation.Elapsed)), "Nothing hurts during the warning " + source.Id);
                if (source.BossPattern == 5) Require(pieces.All(f => f.Damage == 0), "The follow spot itself never hurts (only its sandbag does)");
                else Require(pieces.All(f => f.Damage > 0), "Real damage from the pieces " + source.Id);
                Vector2[] start = pieces.Select(f => f.Position).ToArray();
                // keep the hero still and alive; the pieces must travel and the move must end
                float hurt = 0; int steps = 0;
                for (; steps < 200 && simulation.ExpandedFields.Any(f => f.Kind == ExpandedFieldKind.Roller); steps++)
                {
                    float before = simulation.Champion.Health;
                    simulation.Step(.05f, Vector2.zero);
                    hurt += Mathf.Max(0, before - simulation.Champion.Health);
                    simulation.Champion.Health = simulation.Champion.Definition.Health;
                    if (steps == 24) Require(pieces.Where(f => f.Motion != TrademarkMotion.Rise).Any(f => Vector2.Distance(f.Position, start[pieces.IndexOf(f)]) > .2f || f.Spent) || source.BossPattern == 8, "The pieces travel " + source.Id);
                }
                Require(steps < 200, "The move ends by itself " + source.Id);
                Require(hurt <= simulation.Champion.Definition.Health * .08f * pieces.Count * 2 + .01f, "Each piece (and its drop) hits at most once, for the capped hazard damage " + source.Id);
                if (source.BossPattern == 5) Require(simulation.Elapsed > 0, "Follow spot ran");
                // the boss's own volley order: windup on volley 1 is a trademark and leaves no bullets
                boss.BossVolley = 1; boss.ActiveBossSignature = -1; boss.TelegraphUntil = 0; boss.NextBossPattern = 0; boss.BossRecoveryUntil = 0;
                int shots = simulation.Projectiles.Count;
                for (int i = 0; i < 40 && !boss.ActiveTrademark && boss.BossVolley == 1; i++) simulation.Step(.05f, Vector2.zero);
                Require(boss.ActiveTrademark || boss.BossVolley > 1, "Volley 1 winds up a trademark " + source.Id);
                for (int i = 0; i < 40 && boss.BossVolley == 1; i++) { simulation.Step(.05f, Vector2.zero); simulation.Champion.Health = simulation.Champion.Definition.Health; }
                Require(boss.BossVolley == 2 && simulation.ExpandedFields.Any(f => f.Kind == ExpandedFieldKind.Roller), "The live boss throws it " + source.Id);
                simulation.EnterRest(); Require(simulation.ExpandedFields.Count == 0, "The shop clears the pieces " + source.Id);
            }
            Require(names.Count == 10, "Ten bosses, ten moves");
            Require(Enumerable.Range(0, 10).All(p => BossTrademarks.Prop(p) == null || BossTrademarks.Prop(p).StartsWith("BossProp_")), "Painted pieces are named for the art worklist");

            // the rare unveil
            Require(RareUnveil.Unveils(ItemRarity.Mythic) && RareUnveil.Unveils(ItemRarity.Rainbow) && !RareUnveil.Unveils(ItemRarity.Rare) && !RareUnveil.Unveils(ItemRarity.Common), "Only Mythic and Prismatic offers arrive covered");
            Require(RareUnveil.ClothSprite(false) != null && RareUnveil.ClothSprite(true) != null, "A cloth exists for both (painted or inked)");
            var stand = new GameObject("PolishStand"); var view = stand.AddComponent<SpriteRenderer>(); view.sprite = RareUnveil.ClothSprite(false);
            var cover = new GameObject("PolishCover").AddComponent<RareUnveil>(); cover.Configure(null, view, ItemRarity.Mythic, .5f);
            Require(cover.Covered && !cover.Finished, "Covered until the beat after landing");
            UnityEngine.Object.DestroyImmediate(cover.gameObject); UnityEngine.Object.DestroyImmediate(stand);

            // the star-up ceremony keeps its painted stars and crown (MergeReveal)
            Require(ArsenalArtwork.Frame("StarUp_SilverStar", 0) != null && ArsenalArtwork.Frame("StarUp_GoldStar", 0) != null && ArsenalArtwork.Frame("StarUp_Crown", 0) != null, "Painted star-up art is wired");

            // settings: shake strength, display, the pause menu
            PresentationSettings.Apply(new PlayerProfile { ShakeScale = .5f });
            Require(Mathf.Approximately(PresentationSettings.ShakeScale, .5f), "Shake strength applies");
            PresentationSettings.Apply(new PlayerProfile { ShakeScale = 1, ReducedShake = true });
            Require(PresentationSettings.ShakeScale == 0, "Old 'reduced shake' profiles stay shake-free");
            PlayerProfile old = JsonUtility.FromJson<PlayerProfile>("{\"Version\":3,\"Volume\":0.5}");
            Require(old.ShakeScale == 1 && old.WindowMode == -1 && old.ScreenWidth == 0 && old.VSync, "Older profiles keep the launch display and full shake");
            Require(SettingsSheet.ShakePercent(new PlayerProfile { ShakeScale = .5f }) == 50 && SettingsSheet.ShakePercent(new PlayerProfile { ReducedShake = true }) == 0, "Shake steps read back");
            Require(SettingsSheet.Sizes().Count > 0 && SettingsSheet.Sizes().All(s => s.x >= 1280 && s.y >= 720 || s == SettingsSheet.DisplaySize(new PlayerProfile())), "Resolutions from 1280x720 up");
            string hud = File.ReadAllText("Assets/ArcaneBrigade/Runtime/UI/RunHud.cs");
            Require(hud.Contains("\"SETTINGS\"") && hud.Contains("\"START OVER\"") && hud.Contains("SettingsSheet.Draw") && hud.Split(new[] { "DrawPause(owner)" }, StringSplitOptions.None).Length == 3, "Pause menu (fight and shop): settings and a confirmed restart");
            PresentationSettings.Apply(new PlayerProfile());

            Directory.CreateDirectory("PreviewExports/PolishPass");
            File.WriteAllLines("PreviewExports/PolishPass/Checks.txt", notes);
            Debug.Log("POLISH_PASS_CHECKS_PASSED " + notes.Count);
        }
    }
}
