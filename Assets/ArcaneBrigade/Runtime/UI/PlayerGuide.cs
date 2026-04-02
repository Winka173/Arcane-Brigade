using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>What to do, said when it matters (play-test 20/10/2026, item 4): the goal card when a journey starts, one short
    /// tip the first time each thing appears (first shop, first bond, first merge, full gauge, dream shop, first boss, first
    /// visitor), a "what now" line in the first two shops, and the "How to play" cards in the pause menu. Tips never block:
    /// each shows once (remembered in the profile), fades after a few seconds and closes on a tap. Settings can show them again.</summary>
    public sealed class PlayerGuide
    {
        public sealed class Tip { public string Id; public string Line; public Func<ArenaController, Sprite> Icon; }
        public static readonly Tip[] Tips =
        {
            new Tip { Id = "shop", Line = "Buy friends to build bonds, relics for power, then take the stairs.", Icon = o => o.Content.Arena.Coin },
            new Tip { Id = "bond", Line = "A bond is active: friends of one family give a bonus together.", Icon = o => o.Content.Bonds.Length > 0 ? o.Content.Bonds[0].Icon : null },
            new Tip { Id = "merge", Line = "Three of the same friend became one, with a new star.", Icon = o => o.Session.State.Roster.Count > 0 ? o.Content.Unit(o.Session.State.Roster[0].UnitId)?.Body : null },
            new Tip { Id = "gauge", Line = "Your gauge is full: your hero's power is ready.", Icon = o => o.Content.Unit(o.Session.State.ChampionId)?.Body },
            new Tip { Id = "dream", Line = "The dream shop takes eggs or HP. An HP price lowers your max HP for the rest of the run.", Icon = o => o.Content.HeartSprite },
            new Tip { Id = "boss", Line = "A boss! It hits hard. Keep moving and let your friends work.", Icon = o => o.Simulation?.Boss?.Definition.Body },
            new Tip { Id = "visitor", Line = "A visitor waits at the table. Walk up to see the deal.", Icon = o => ExpansionArtwork.Current?.Visitor(o.Session.State.Visitor) },
        };
        private Tip showing; private float shownAt; private float goalAt = -1; private string goalRun;
        private GUIStyle line, title, small;
        public string Showing => showing?.Id;
        public bool GoalShowing => goalAt >= 0 && Time.unscaledTime - goalAt < GoalSeconds;
        private const float TipSeconds = 6.5f, GoalSeconds = 7f;

        public static bool Seen(PlayerProfile profile, string id) => profile != null && profile.TipsSeen.Contains(id);
        private static void Mark(ArenaController owner, string id)
        {
            PlayerProfile profile = owner.Profile; if (profile == null || profile.TipsSeen.Contains(id)) return;
            profile.TipsSeen.Add(id);
            if (RunPaths.Diagnostic) return;
            try { new RunStorage(RunPaths.Root).SaveProfile(profile); } catch (Exception exception) { Debug.LogWarning("Tip not saved: " + exception.Message); }
        }

        private static bool Due(ArenaController o, string id)
        {
            RunSnapshot s = o.Session.State; CombatSimulation sim = o.Simulation;
            switch (id)
            {
                case "shop": return s.Phase == SessionPhase.Shop && o.Shop != null && !o.Shop.Special;
                case "bond": foreach (BondDefinition b in o.Content.Bonds) if (o.Session.BondCounts.TryGetValue(b.Id, out int n) && b.ActiveLevel(n) > 0) return true; return false;
                case "merge": return s.Roster.Exists(r => r.Tier >= 2);
                case "gauge": return s.Phase == SessionPhase.Combat && sim != null && sim.HeroIdentityEnabled && sim.HeroIdentityRequired > 0 && sim.HeroIdentityCharge >= sim.HeroIdentityRequired;
                case "dream": return s.Phase == SessionPhase.Shop && o.Shop != null && o.Shop.Special;
                case "boss": return s.Phase == SessionPhase.Combat && sim?.Boss != null && sim.Boss.Alive;
                case "visitor": return o.Session.Visitor != null;
                default: return false;
            }
        }

        public void Draw(ArenaController owner)
        {
            if (owner?.Session == null || owner.Profile == null || RunPaths.Diagnostic && !Forced) return;
            Styles();
            RunSnapshot state = owner.Session.State;
            // the goal of a journey, once, when its first wave starts
            if (goalRun != state.RunId) { goalRun = state.RunId; goalAt = -1; }
            if (goalAt < 0 && state.Phase == SessionPhase.Combat && state.Wave == 1 && !state.Endless && !Seen(owner.Profile, "goal")) { goalAt = Time.unscaledTime; Mark(owner, "goal"); }
            if (GoalShowing) { DrawGoal(owner); return; }
            // the first two shops: what to do now
            if (state.Phase == SessionPhase.Shop && state.Wave <= 2 && !state.Endless && owner.Shop != null && !owner.Shop.Special && !owner.HallExitPending) DrawShopHint(owner);
            if (showing == null)
                foreach (Tip tip in Tips)
                    if (!Seen(owner.Profile, tip.Id) && Due(owner, tip.Id)) { showing = tip; shownAt = Time.unscaledTime; Mark(owner, tip.Id); CheckpointAudio.PlayInterfaceId("ui.coach.pop"); break; }
            if (showing == null) return;
            float age = Time.unscaledTime - shownAt;
            if (age > TipSeconds) { showing = null; return; }
            DrawTip(owner, showing, Mathf.Clamp01(age / .25f) * Mathf.Clamp01((TipSeconds - age) / .4f));
        }
        private static bool Forced => Array.IndexOf(Environment.GetCommandLineArgs(), "-arcane-guide") >= 0 || PlaytestFixCapture.Requested;
        /// <summary>For captures: show a tip now.</summary>
        public void ShowForCapture(string id) { showing = Array.Find(Tips, t => t.Id == id); shownAt = Time.unscaledTime; }
        public void ShowGoalForCapture() { goalAt = Time.unscaledTime; }

        private void Styles()
        {
            if (line != null) return;
            line = ArcadeTypography.Style(26); line.wordWrap = true; line.alignment = TextAnchor.MiddleLeft; line.normal.textColor = ArcadeSkin.Cream;
            title = ArcadeTypography.Style(36, true); title.alignment = TextAnchor.MiddleCenter; title.normal.textColor = ArcadeSkin.Gold;
            small = ArcadeTypography.Style(24); small.wordWrap = true; small.alignment = TextAnchor.UpperCenter; small.normal.textColor = ArcadeSkin.Cream;
        }

        private void DrawTip(ArenaController owner, Tip tip, float alpha)
        {
            Rect r = new Rect(560, 150, 800, 104);
            Color keep = GUI.color; GUI.color = new Color(keep.r, keep.g, keep.b, keep.a * alpha);
            ArcadeSkin.Ticket(r, ArcadeSkin.Ink, ArcadeSkin.Gold);
            Sprite icon = null; try { icon = tip.Icon(owner); } catch (Exception) { }
            if (icon != null) RunHud.Sprite(new Rect(r.x + 18, r.y + 14, 76, 76), icon);
            GUI.Label(new Rect(r.x + 110, r.y + 8, r.width - 130, r.height - 16), tip.Line, line);
            GUI.color = keep;
            if (GUI.Button(r, GUIContent.none, GUIStyle.none)) showing = null; // a tap closes it
        }

        private void DrawGoal(ArenaController owner)
        {
            Rect r = new Rect(510, 170, 900, 300);
            float age = Time.unscaledTime - goalAt, alpha = Mathf.Clamp01(age / .3f) * Mathf.Clamp01((GoalSeconds - age) / .5f);
            Color keep = GUI.color; GUI.color = new Color(keep.r, keep.g, keep.b, keep.a * alpha);
            ArcadeSkin.Ticket(r, ArcadeSkin.Ink, ArcadeSkin.Gold);
            GUI.Label(new Rect(r.x, r.y + 18, r.width, 50), "Reach the dawn", title);
            RunCatalog c = owner.Content;
            Sprite[] icons = { c.Unit(owner.Session.State.ChampionId)?.Body, c.Bosses.Length > 0 ? c.Bosses[0].Body : null, c.Arena.Coin };
            string[] words = { "Survive 20 nights", "Every 5th night, a boss", "Between nights, spend in the shop" };
            for (int i = 0; i < 3; i++)
            {
                Rect cell = new Rect(r.x + 30 + i * 285, r.y + 80, 270, 200);
                if (icons[i] != null) RunHud.Sprite(new Rect(cell.center.x - 50, cell.y, 100, 100), icons[i]);
                GUI.Label(new Rect(cell.x, cell.y + 110, cell.width, 80), words[i], small);
            }
            GUI.color = keep;
            if (GUI.Button(r, GUIContent.none, GUIStyle.none)) goalAt = -100;
        }

        private void DrawShopHint(ArenaController owner)
        {
            Rect r = new Rect(660, 96, 600, 52);
            ArcadeSkin.Line(r, new Color(.05f, .06f, .1f, .82f));
            RunCatalog c = owner.Content;
            Sprite friend = c.Companions.Length > 0 ? c.Companions[0].Body : null;
            (Sprite icon, string word)[] steps = { (friend, "Friends"), (c.Items.Length > 0 ? c.Items[0].Icon : null, "Relics"), (null, "Stairs ↓") };
            GUIStyle s = new GUIStyle(line) { fontSize = 22, alignment = TextAnchor.MiddleLeft, wordWrap = false };
            for (int i = 0; i < steps.Length; i++)
            {
                Rect cell = new Rect(r.x + 14 + i * 196, r.y + 4, 190, 44);
                if (steps[i].icon != null) RunHud.Sprite(new Rect(cell.x, cell.y + 2, 40, 40), steps[i].icon);
                GUI.Label(new Rect(cell.x + (steps[i].icon != null ? 46 : 6), cell.y, 140, 44), (i > 0 ? "" : "") + steps[i].word + (i < steps.Length - 1 ? "  →" : ""), s);
            }
        }

        /// <summary>The "How to play" cards (pause menu): six pictures, one line each, re-readable any time.</summary>
        public void DrawHowToPlay(Rect area, ArenaController owner)
        {
            Styles();
            RunCatalog c = owner.Content;
            (Sprite icon, string head, string body)[] cards =
            {
                (c.Unit(owner.Session.State.ChampionId)?.Body, "The goal", "Survive 20 nights. Every 5th night a boss comes."),
                (c.Unit(owner.Session.State.ChampionId)?.Weapon?.Sprite, "Move", "Just move. Your hero and friends attack by themselves."),
                (c.Arena.Coin, "Gold and eggs", "Pick them up. Gold buys in the shop, eggs in the dream shop."),
                (c.Bonds.Length > 0 ? c.Bonds[0].Icon : null, "Friends and bonds", "Friends of one family together unlock a bond."),
                (c.Companions.Length > 0 ? c.Companions[0].Body : null, "Merge", "Three of the same friend become one, with a new star."),
                (c.Items.Length > 0 ? c.Items[0].Icon : null, "Relics", "Relics give bonuses for the rest of the run. Tap a line to learn more."),
            };
            float w = (area.width - 40) / 3, h = (area.height - 20) / 2;
            GUIStyle head = new GUIStyle(title) { fontSize = 28 };
            for (int i = 0; i < cards.Length; i++)
            {
                Rect card = new Rect(area.x + i % 3 * (w + 20), area.y + i / 3 * (h + 20), w, h);
                ArcadeSkin.Ticket(card, ArcadeSkin.Ink, ArcadeSkin.Accent);
                if (cards[i].icon != null) RunHud.Sprite(new Rect(card.center.x - 55, card.y + 18, 110, 110), cards[i].icon);
                GUI.Label(new Rect(card.x + 10, card.y + 132, card.width - 20, 40), cards[i].head, head);
                GUI.Label(new Rect(card.x + 18, card.y + 176, card.width - 36, card.height - 186), cards[i].body, small);
            }
        }
    }
}
