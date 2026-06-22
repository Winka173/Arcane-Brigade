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
            new Tip { Id = "merge", Line = "Two of the same friend became one, with a new star.", Icon = o => o.Session.State.Roster.Count > 0 ? o.Content.Unit(o.Session.State.Roster[0].UnitId)?.Body : null },
            new Tip { Id = "gauge", Line = "Your gauge is full: your hero's power is ready.", Icon = o => o.Content.Unit(o.Session.State.ChampionId)?.Body },
            new Tip { Id = "dream", Line = "The dream shop takes eggs or HP. An HP price lowers your max HP for the rest of the run.", Icon = o => o.Content.HeartSprite },
            new Tip { Id = "boss", Line = "A boss! Red ink marks where its blows land: step out before the rim closes.", Icon = o => o.Simulation?.Boss?.Definition.Body },
            new Tip { Id = "visitor", Line = "A visitor waits at the table. Walk up to see the deal.", Icon = o => ExpansionArtwork.Current?.Visitor(o.Session.State.Visitor) },
            // the maps' living elements (play-test item 24): one line each, the first time it appears
            MapTip(MapIdentity.Element.Board), MapTip(MapIdentity.Element.Chairs), MapTip(MapIdentity.Element.Chute), MapTip(MapIdentity.Element.Gust),
            MapTip(MapIdentity.Element.Parcel), MapTip(MapIdentity.Element.Light), MapTip(MapIdentity.Element.Snuffer), MapTip(MapIdentity.Element.Moths),
            MapTip(MapIdentity.Element.Clock), MapTip(MapIdentity.Element.Shelves), MapTip(MapIdentity.Element.Ledger),
        };
        private static Tip MapTip(MapIdentity.Element element) => new Tip { Id = MapIdentity.TipId(element), Line = MapIdentity.TipLine(element), Icon = o => MapIdentity.Sprite(MapIdentity.Prop(element)) };
        private static bool MapShown(CombatSimulation sim, string id)
        {
            if (sim?.MapEnvironment == null) return false;
            foreach (MapIdentity.Element element in sim.MapEnvironment.Shown) if (MapIdentity.TipId(element) == id) return true;
            return false;
        }
        private Tip showing; private float shownAt; private float goalAt = -1; private string goalRun;
        private GUIStyle line, title, small, hintLine;
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
                default: return id.StartsWith("env.", StringComparison.Ordinal) && s.Phase == SessionPhase.Combat && MapShown(sim, id);
            }
        }

        public void Draw(ArenaController owner)
        {
            ShownTip = Rect.zero;
            if (owner?.Session == null || owner.Profile == null || RunPaths.Diagnostic && !Forced) return;
            Styles();
            RunSnapshot state = owner.Session.State;
            // the goal of a journey, once, when its first wave starts
            if (goalRun != state.RunId) { goalRun = state.RunId; goalAt = -1; }
            if (goalAt < 0 && state.Phase == SessionPhase.Combat && state.Wave == 1 && !state.Endless && !Seen(owner.Profile, "goal")) { goalAt = Time.unscaledTime; Mark(owner, "goal"); }
            if (GoalShowing) { DrawGoal(owner); return; }
            // the first two shops: what to do now
            // one card at a time: the strip steps aside for a tip, an open card or the pause menu
            if (state.Phase == SessionPhase.Shop && state.Wave <= 2 && !state.Endless && owner.Shop != null && !owner.Shop.Special && !owner.HallExitPending
                && showing == null && !ModalOpen(owner) && RunHud.ShownShopCard.height < 300 && !owner.PresentationPaused) DrawShopHint(owner);
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

        /// <summary>Any modal card on screen (one shared check): the shop's inspect card, a visitor card, the Last Customer or the
        /// relight choice, the Keeper's gifts, a secret-event room, the brigade details. Tips wait (timer paused, not drawn).</summary>
        public static bool ModalOpen(ArenaController owner)
        {
            if (owner == null) return false;
            if (owner.GetComponent<ShopInspect>()?.Open != null || owner.GetComponent<VisitorCard>()?.Open == true || owner.GetComponent<LucidAltar>()?.Open == true || owner.GetComponent<WaitingRealmStage>()?.Asking == true || owner.Details.Open) return true;
            foreach (CustomerOffer offer in owner.GetComponents<CustomerOffer>()) if (offer.Open) return true;
            foreach (MemoryReturn memory in owner.GetComponents<MemoryReturn>()) if (memory.Open) return true;
            OutsideStageChoice gifts = owner.GetComponent<OutsideStageChoice>(); if (gifts != null && gifts.GiftsOpen) return true;
            SecretEventRoom room = owner.GetComponent<SecretEventRoom>(); return room != null && room.MenuOpen;
        }

        /// <summary>The tip card on screen this frame (zero when none), so speech bubbles keep off it.</summary>
        public static Rect ShownTip { get; private set; }
        private void DrawTip(ArenaController owner, Tip tip, float alpha)
        {
            Rect r = new Rect(560, 150, 800, 104);
            // never over an open card (the shop's details, an inspect card): the tip waits until it is closed
            Rect shown = RunHud.ShownShopCard;
            if (shown.height > 0 && shown.Overlaps(r) || ModalOpen(owner) || owner.PresentationPaused)
            {
                if (Event.current.type == EventType.Repaint) shownAt += Time.unscaledDeltaTime;
                return;
            }
            ShownTip = r;
            Color keep = GUI.color; Matrix4x4 keepMatrix = GUI.matrix; GUI.color = new Color(keep.r, keep.g, keep.b, keep.a * alpha);
            LivingUI.Auto("guide-tip", r.center, false); // a tap closes it with motion (requirement 25/10/2026)
            ArcadeSkin.Ticket(r, ArcadeSkin.Ink, ArcadeSkin.Gold);
            Highlight(r, Time.unscaledTime - shownAt, alpha);
            Sprite icon = null; try { icon = tip.Icon(owner); } catch (Exception) { }
            Rect inner = ArcadeSkin.Content(r, KitFrame.Dialogue); // inside the painted rim and roses
            if (icon != null) RunHud.Sprite(new Rect(inner.x, r.center.y - 34, 68, 68), icon);
            GUI.Label(new Rect(inner.x + 84, r.y, inner.xMax - inner.x - 84, r.height), tip.Line, line);
            GUI.color = keep; GUI.matrix = keepMatrix;
            if (GUI.Button(r, GUIContent.none, GUIStyle.none) && !LivingUI.Closing("guide-tip")) LivingUI.CloseThen("guide-tip", () => showing = null); // a tap closes it
        }

        /// <summary>Guide text must catch the eye (requirement 20/10/2026): a gold rim with two sparks running round it, a warm glow that
        /// swells for the first two seconds, and a soft inner rim. Reduced Motion keeps a still gold rim.</summary>
        private static void Highlight(Rect r, float age, float alpha)
        {
            Color gold = ArcadeSkin.Gold; gold.a = .9f * alpha;
            float t = 2; // rim thickness
            ArcadeSkin.Line(new Rect(r.x - t, r.y - t, r.width + 2 * t, t), gold); ArcadeSkin.Line(new Rect(r.x - t, r.yMax, r.width + 2 * t, t), gold);
            ArcadeSkin.Line(new Rect(r.x - t, r.y, t, r.height), gold); ArcadeSkin.Line(new Rect(r.xMax, r.y, t, r.height), gold);
            if (PresentationSettings.ReducedMotion) return;
            // the glow: a few widening rims fading out, strongest when the card appears
            float swell = Mathf.Clamp01(1 - age / 2f) * .55f + .12f + .06f * Mathf.Sin(age * 3f);
            for (int i = 1; i <= 4; i++)
            {
                Color glow = ArcadeSkin.Gold; glow.a = swell * alpha * (1 - i / 5f) * .5f; float o = t + i * 3;
                ArcadeSkin.Line(new Rect(r.x - o, r.y - o, r.width + 2 * o, 2), glow); ArcadeSkin.Line(new Rect(r.x - o, r.yMax + o - 2, r.width + 2 * o, 2), glow);
                ArcadeSkin.Line(new Rect(r.x - o, r.y - o, 2, r.height + 2 * o), glow); ArcadeSkin.Line(new Rect(r.xMax + o - 2, r.y - o, 2, r.height + 2 * o), glow);
            }
            // two sparks chasing each other round the rim
            float perimeter = 2 * (r.width + r.height);
            for (int k = 0; k < 2; k++)
            {
                float head = Mathf.Repeat(age * 420f + k * perimeter * .5f, perimeter);
                for (int s = 0; s < 14; s++)
                {
                    Vector2 p = OnRim(r, Mathf.Repeat(head - s * 7f, perimeter));
                    Color spark = Color.Lerp(Color.white, ArcadeSkin.Gold, s / 14f); spark.a = alpha * (1 - s / 14f);
                    float size = Mathf.Lerp(18, 6, s / 14f); // soft round light, never a hard square
                    Color keep = GUI.color; GUI.color = spark;
                    GUI.DrawTexture(new Rect(p.x - size * .5f, p.y - size * .5f, size, size), SoftDot);
                    GUI.color = keep;
                }
            }
        }
        private static Texture2D softDot;
        private static Texture2D SoftDot
        {
            get
            {
                if (softDot != null) return softDot;
                softDot = new Texture2D(32, 32, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
                for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++)
                {
                    float d = Vector2.Distance(new Vector2(x + .5f, y + .5f), new Vector2(16, 16)) / 16f;
                    softDot.SetPixel(x, y, new Color(1, 1, 1, Mathf.Pow(Mathf.Clamp01(1 - d), 2.2f)));
                }
                softDot.Apply(); return softDot;
            }
        }
        private static Vector2 OnRim(Rect r, float d)
        {
            if (d < r.width) return new Vector2(r.x + d, r.y - 1);
            d -= r.width; if (d < r.height) return new Vector2(r.xMax + 1, r.y + d);
            d -= r.height; if (d < r.width) return new Vector2(r.xMax - d, r.yMax + 1);
            d -= r.width; return new Vector2(r.x - 1, r.yMax - d);
        }

        private void DrawGoal(ArenaController owner)
        {
            Rect r = new Rect(510, 170, 900, 300);
            float age = Time.unscaledTime - goalAt, alpha = Mathf.Clamp01(age / .3f) * Mathf.Clamp01((GoalSeconds - age) / .5f);
            Color keep = GUI.color; GUI.color = new Color(keep.r, keep.g, keep.b, keep.a * alpha);
            ArcadeSkin.Ticket(r, ArcadeSkin.Ink, ArcadeSkin.Gold);
            Highlight(r, age, alpha);
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
            ArcadeSkin.Plate(r, ArcadeSkin.Accent); // a walnut plate (UI kit)
            Highlight(r, Time.unscaledTime, 1);
            RunCatalog c = owner.Content;
            Sprite friend = c.Companions.Length > 0 ? c.Companions[0].Body : null;
            (Sprite icon, string word)[] steps = { (friend, "Friends"), (c.Items.Length > 0 ? c.Items[0].Icon : null, "Relics"), (null, "Stairs ↓") };
            GUIStyle s = hintLine ??= new GUIStyle(line) { fontSize = 22, alignment = TextAnchor.MiddleLeft, wordWrap = false }; // made once, not every GUI pass
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
                (c.Companions.Length > 0 ? c.Companions[0].Body : null, "Merge", "Two of the same friend become one, with a new star."),
                (c.Items.Length > 0 ? c.Items[0].Icon : null, "Relics", "Relics give bonuses for the rest of the run."),
            };
            float w = (area.width - 40) / 3, h = (area.height - 20) / 2;
            GUIStyle head = new GUIStyle(title) { fontSize = 28 };
            for (int i = 0; i < cards.Length; i++)
            {
                Rect card = new Rect(area.x + i % 3 * (w + 20), area.y + i / 3 * (h + 20), w, h);
                ArcadeSkin.Ticket(card, ArcadeSkin.Ink, ArcadeSkin.Accent);
                if (cards[i].icon != null) RunHud.Sprite(new Rect(card.center.x - 55, card.y + 18, 110, 110), cards[i].icon);
                Rect inner = ArcadeSkin.Content(card, KitFrame.Dialogue); // inside the frame's rim with the standard padding
                GUI.Label(new Rect(inner.x, card.y + 132, inner.width, 40), cards[i].head, head);
                GUI.Label(new Rect(inner.x, card.y + 176, inner.width, inner.yMax - card.y - 176), cards[i].body, small);
            }
        }
    }
}
