using System;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The Lamp Tree panel (requirement 24/10/2026): an ink tree growing from one trunk into nine branches over four roots,
    /// each lantern a node (a fork shows its two side by side; small lozenges under a lantern are its ranks). Tapping a lantern
    /// opens its card beside it (icon, one line, rank, price); a locked branch shows padlocks and its card says what opens it.
    /// "Take back all" refunds every rank (free; a second tap confirms).</summary>
    public sealed partial class MetaHub
    {
        private string treePick;
        private bool respecArmed;
        /// <summary>Diagnostics: opens a lantern's card.</summary>
        public void PickLantern(string id) => treePick = id;
        private static readonly Color TreeInk = new Color(.16f, .1f, .06f, .95f), TreeBark = new Color(.42f, .28f, .15f, 1), TreeLit = new Color(1f, .82f, .45f, 1);

        private static Rect TreeArea => new Rect(PanelRect.x + 16, PanelRect.y + 74, PanelRect.width - 32, PanelRect.height - 84);
        private static Vector2 NodeAt(LampNode n, Rect tree)
        {
            if (n.Branch < 0) return new Vector2(tree.center.x + (n.Tier < 2 ? -1 : 1) * (n.Tier % 3 == 0 ? 270 : 110), tree.yMax - 22); // the roots: two each side
            float col = tree.width / MetaProgression.BranchNames.Length;
            float x = tree.x + col * (n.Branch + .5f);
            if (MetaProgression.Fork(n) is LampNode other) x += Array.IndexOf(MetaProgression.Tree, n) < Array.IndexOf(MetaProgression.Tree, other) ? -col * .24f : col * .24f;
            float bottom = tree.yMax - 112, top = tree.y + 44;
            return new Vector2(x, Mathf.Lerp(bottom, top, n.Tier / (float)(MetaProgression.TreeTiers - 1)));
        }
        private static float NodeSize(LampNode n) => n.Capstone ? 46 : n.Branch < 0 ? 36 : MetaProgression.Fork(n) != null ? 34 : 40;

        /// <summary>A bent ink branch from a to b (a quadratic bend, dark ink under bark; gold when lit).</summary>
        private static void Bough(Vector2 a, Vector2 mid, Vector2 b, float width, bool lit)
        {
            const int steps = 10;
            for (int pass = 0; pass < 2; pass++)
                for (int i = 0; i < steps; i++)
                {
                    float u = i / (float)steps, v = (i + 1) / (float)steps;
                    Vector2 p = Q(a, mid, b, u), q = Q(a, mid, b, v);
                    float w = Mathf.Lerp(width, width * .7f, u);
                    if (pass == 0) ArcadeSkin.Stroke(p, q, w + 4, TreeInk);
                    else ArcadeSkin.Stroke(p, q, w, lit ? TreeLit : TreeBark);
                }
        }
        private static void Bough(Vector2 a, Vector2 b, float width, bool lit) => Bough(a, (a + b) * .5f, b, width, lit);
        private static Vector2 Q(Vector2 a, Vector2 m, Vector2 b, float t) => (1 - t) * (1 - t) * a + 2 * (1 - t) * t * m + t * t * b;

        private void DrawTree(RunCatalog catalog, PlayerProfile profile, Func<bool> save)
        {
            navCols = 1;
            MetaProgression.Ensure(profile);
            Rect tree = TreeArea;
            float t = Time.unscaledTime;
            LampNode pick = MetaProgression.Node(treePick);
            Rect card = pick != null ? CardRect(NodeAt(pick, tree)) : Rect.zero;

            // the trunk, the boughs to each branch's first lantern, the roots
            Vector2 fork = new Vector2(tree.center.x, tree.yMax - 70), foot = new Vector2(tree.center.x, tree.yMax - 34);
            bool any = profile.Meta.Lamps.Count > 0;
            foreach (LampNode n in MetaProgression.Roots) { Vector2 at = NodeAt(n, tree); Bough(foot, new Vector2(Mathf.Lerp(foot.x, at.x, .5f), foot.y + 4), at, 7, MetaProgression.Lit(profile, n.Id)); }
            Bough(foot, fork, 24, any);
            foreach (LampNode n in MetaProgression.Tree)
            {
                if (n.Tier == 0) Bough(fork, new Vector2(Mathf.Lerp(fork.x, NodeAt(n, tree).x, .85f), fork.y), NodeAt(n, tree), 11, MetaProgression.Lit(profile, n.Id));
                foreach (LampNode up in MetaProgression.Tree)
                    if (up.Branch == n.Branch && up.Tier == n.Tier + 1)
                        Bough(NodeAt(n, tree), NodeAt(up, tree), up.Capstone ? 8 : 6, MetaProgression.Lit(profile, n.Id) && MetaProgression.Lit(profile, up.Id));
            }
            // branch names over their crowns
            chip.normal.textColor = ArcadeSkin.KitCream;
            float col = tree.width / MetaProgression.BranchNames.Length;
            for (int b = 0; b < MetaProgression.BranchNames.Length; b++)
                GUI.Label(new Rect(tree.x + col * b, tree.y - 2, col, 22), MetaProgression.BranchNames[b], chip);

            // the lanterns and the roots
            foreach (LampNode n in MetaProgression.Tree) Lantern(n, tree, card, profile, t);
            foreach (LampNode n in MetaProgression.Roots) Lantern(n, tree, card, profile, t);

            // take the whole tree back (free; a second tap confirms)
            Rect respec = new Rect(PanelRect.x + 22, PanelRect.y + 12, 214, 46);
            if ((ArcadeSkin.Button(respec, respecArmed ? "TAP AGAIN · FREE" : "TAKE BACK ALL", KitButton.Secondary, any, chip) | Nav(respec, any)) && any)
            {
                if (!respecArmed) { respecArmed = true; CheckpointAudio.PlayInterfaceId("ui.select"); }
                else
                {
                    string before = JsonUtility.ToJson(profile.Meta);
                    if (MetaProgression.Respec(profile) && !save()) { JsonUtility.FromJsonOverwrite(before, profile.Meta); CheckpointAudio.PlayInterfaceId("ui.error"); }
                    else CheckpointAudio.PlayInterfaceId("ui.back");
                    respecArmed = false;
                }
            }

            // the chosen lantern's card, beside it
            if (pick != null) TreeCard(card, pick, profile, save);
        }

        private static Rect CardRect(Vector2 at)
        {
            const float w = 290, h = 262;
            float x = at.x < PanelRect.center.x ? at.x + 40 : at.x - 40 - w;
            return new Rect(x, Mathf.Clamp(at.y - h * .5f, PanelRect.y + 64, PanelRect.yMax - h - 8), w, h);
        }

        private void Lantern(LampNode n, Rect tree, Rect card, PlayerProfile profile, float t)
        {
            Vector2 at = NodeAt(n, tree); float size = NodeSize(n);
            Rect r = new Rect(at.x - size * .5f, at.y - size * .5f, size, size);
            // under the open card a lantern is only drawn (the card takes the taps)
            if (card.width > 0 && r.Overlaps(card) && Event.current.type != EventType.Repaint) return;
            int rank = MetaProgression.LampRank(profile, n.Id);
            bool lit = rank > 0, open = MetaProgression.BranchOpen(profile, n.Branch);
            string blocked = MetaProgression.NodeBlocked(profile, n);
            bool ready = blocked == null, taken = blocked == "Other path taken";
            if (lit) ArcadeSkin.Glow(new Rect(r.x - 20, r.y - 20, r.width + 40, r.height + 40), new Color(1f, .78f, .4f, .4f + .08f * Mathf.Sin(t * 2 + n.Tier)));
            else if (ready) ArcadeSkin.Glow(new Rect(r.x - 12, r.y - 12, r.width + 24, r.height + 24), new Color(1f, .9f, .6f, .16f + .12f * Mathf.Sin(t * 3)));
            if (treePick == n.Id) ArcadeSkin.Glow(new Rect(r.x - 14, r.y - 14, r.width + 28, r.height + 28), new Color(ArcadeSkin.Gold.r, ArcadeSkin.Gold.g, ArcadeSkin.Gold.b, .6f));
            KitRound face = lit ? KitRound.Brass : !open || taken ? KitRound.Disabled : n.Currency == MetaCurrency.Keepsakes ? KitRound.Oxblood : KitRound.Walnut;
            bool tapped = ArcadeSkin.IconButton(r, face) | Nav(r);
            Rect icon = new Rect(r.x + size * .2f, r.y + size * .2f, size * .6f, size * .6f);
            Color keep = GUI.color;
            if (!open) ArcadeSkin.Lock(r.center + Vector2.up * size * .04f, size * .3f, new Color(.86f, .78f, .62f));
            else { if (taken || !lit && !ready) GUI.color = new Color(keep.r, keep.g, keep.b, keep.a * .55f); NodeIcon(icon, n); GUI.color = keep; }
            // its ranks: lozenges along the rim (the roots: a count beside them)
            if (n.Branch < 0 && open) { small.normal.textColor = ArcadeSkin.KitCream; small.alignment = TextAnchor.MiddleLeft; GUI.Label(new Rect(r.xMax + 4, r.y, 56, r.height), rank + "/" + n.Ranks, small); small.alignment = TextAnchor.UpperLeft; }
            else if (n.Ranks > 1 && open)
                for (int k = 0; k < n.Ranks; k++)
                    ArcadeSkin.Diamond(new Vector2(at.x + (k - (n.Ranks - 1) * .5f) * 9, r.yMax - 1), 8, k < rank ? ArcadeSkin.Gold : new Color(.22f, .15f, .1f));
            if (tapped) { treePick = treePick == n.Id ? null : n.Id; respecArmed = false; CheckpointAudio.PlayInterfaceId("ui.select"); }
        }

        private void TreeCard(Rect c, LampNode n, PlayerProfile profile, Func<bool> save)
        {
            bool flash = bought == n.Id && Time.unscaledTime - boughtAt < .6f;
            if (flash) ArcadeSkin.Glow(new Rect(c.x - 12, c.y - 12, c.width + 24, c.height + 24), new Color(1f, .85f, .5f, .5f * (1 - (Time.unscaledTime - boughtAt) / .6f)));
            ArcadeSkin.Frame(c, KitFrame.Card, .3f);
            Color ink = ArcadeSkin.FrameText(KitFrame.Card);
            Rect inner = ArcadeSkin.Content(c, KitFrame.Card, true, .3f);
            Rect close = new Rect(c.xMax - 44, c.y - 6, 44, 44);
            NodeIcon(new Rect(inner.center.x - 28, inner.y, 56, 56), n);
            body.normal.textColor = ink; small.normal.textColor = new Color(ink.r, ink.g, ink.b, .9f);
            body.alignment = TextAnchor.MiddleCenter; small.alignment = TextAnchor.UpperCenter;
            GUI.Label(new Rect(inner.x, inner.y + 58, inner.width, 26), n.Name, body);
            GUI.Label(new Rect(inner.x, inner.y + 84, inner.width, 44), n.Line, small);
            int rank = MetaProgression.LampRank(profile, n.Id);
            string tag = n.Capstone ? "CROWN" : MetaProgression.Fork(n) != null ? "PICK 1 OF 2" : n.Ranks > 1 ? "RANK " + rank + "/" + n.Ranks : null;
            if (tag != null) { chip.normal.textColor = new Color(.55f, .18f, .12f); GUI.Label(new Rect(inner.x, inner.y + 124, inner.width, 22), tag, chip); }
            body.alignment = TextAnchor.UpperLeft; small.alignment = TextAnchor.UpperLeft;

            string blocked = MetaProgression.NodeBlocked(profile, n);
            Rect button = new Rect(inner.x, inner.yMax - 42, inner.width, 42);
            if (blocked == "MAX") ArcadeSkin.Button(button, n.Ranks > 1 ? "MAX" : "LIT", KitButton.Secondary, false, chip);
            else if (blocked == "Locked")
            {
                ArcadeSkin.Lock(new Vector2(button.center.x, button.y - 14), 20, ink);
                small.normal.textColor = ink; small.alignment = TextAnchor.UpperCenter;
                string why = n.Branch < 0 ? "Light every branch first" : "Light " + MetaProgression.BranchOpensAt + " in " + MetaProgression.BranchNames[n.Branch - 1] + "\nor win " + n.Branch + " difficult" + (n.Branch == 1 ? "y" : "ies");
                GUI.Label(new Rect(inner.x, button.y, inner.width, 44), why, small);
                small.alignment = TextAnchor.UpperLeft;
            }
            else
            {
                bool can = blocked == null;
                string caption = n.CostAt(rank).ToString("N0");
                if ((ArcadeSkin.Button(button, "      " + caption, can ? KitButton.Primary : KitButton.Secondary, can, body) | Nav(button, can)) && can)
                {
                    string before = JsonUtility.ToJson(profile.Meta);
                    if (MetaProgression.Light(profile, n.Id))
                    {
                        if (!save()) { JsonUtility.FromJsonOverwrite(before, profile.Meta); CheckpointAudio.PlayInterfaceId("ui.error"); }
                        else { bought = n.Id; boughtAt = Time.unscaledTime; CheckpointAudio.PlayInterfaceId(n.Capstone || rank + 1 >= n.Ranks && n.Ranks > 1 ? "up.star3" : "shop.buy.gold"); }
                    }
                }
                float cw = body.CalcSize(new GUIContent(caption)).x;
                RunHud.Sprite(new Rect(button.center.x - cw * .5f - 30, button.center.y - 15, 30, 30), MetaIcons.Currency(n.Currency));
                if (!can && blocked != "Not enough oil" && blocked != "Not enough keepsakes")
                {
                    small.normal.textColor = new Color(.55f, .18f, .12f); small.alignment = TextAnchor.UpperCenter;
                    GUI.Label(new Rect(inner.x, button.y - 22, inner.width, 22), blocked, small);
                    small.alignment = TextAnchor.UpperLeft;
                }
            }
            if (ArcadeSkin.Medallion(close, KitMedallion.Close) | Nav(close)) { treePick = null; CheckpointAudio.PlayInterfaceId("ui.back"); }
        }

        private static void NodeIcon(Rect r, LampNode n)
        {
            if (n.Stat.HasValue && PaintedStatIcons.TryDraw(r, n.Stat.Value)) return;
            Sprite s;
            switch (n.Icon)
            {
                case "egg": case "star": s = CurrencyIcons.Egg; break;
                case "coin": s = CurrencyIcons.Coin; break;
                case "clock": s = MetaIcons.Station(MetaStation.Clock, true); break;
                case "nursery": case "family": s = MetaIcons.Station(MetaStation.Nursery, true); break;
                case "ledger": s = MetaIcons.Station(MetaStation.Ledger, true); break;
                case "keep": s = MetaIcons.Keepsake; break;
                case "rare": s = MetaIcons.Station(MetaStation.Museum, true); break;
                case "gauge": s = MetaIcons.Station(MetaStation.Lantern, true); break;
                case "dream": s = MetaIcons.Station(MetaStation.Window, true); break;
                case "hunt": s = MetaIcons.Station(MetaStation.NightPact, true); break;
                case "shell": s = MetaIcons.Station(MetaStation.Bell, true); break;
                default: s = MetaIcons.LampOil; break;
            }
            if (n.Icon == "shell" && PaintedStatIcons.TryDraw(r, ItemEffect.Armor)) s = null;
            if (s != null) RunHud.Sprite(r, s);
            if (n.Icon == "star") ArcadeSkin.Diamond(new Vector2(r.xMax - r.width * .12f, r.y + r.height * .14f), r.width * .34f, ArcadeSkin.Gold); // one star more
        }
    }
}
