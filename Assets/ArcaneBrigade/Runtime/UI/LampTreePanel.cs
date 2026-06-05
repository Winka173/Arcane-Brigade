using System;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The Lamp Tree panel (owner 24/10/2026): an ink tree growing from one trunk into five branches, each lit lantern a
    /// node; a fork shows its two lanterns side by side. Tapping a lantern opens its card on the right (icon, one line, price);
    /// a locked branch shows padlocks and says what opens it. "Take back all" refunds every lantern (free).</summary>
    public sealed partial class MetaHub
    {
        private string treePick;
        private bool respecArmed;
        private static readonly Color TreeInk = new Color(.16f, .1f, .06f, .95f), TreeBark = new Color(.42f, .28f, .15f, 1), TreeLit = new Color(1f, .82f, .45f, 1);

        private static Vector2 NodeAt(LampNode n, Rect tree)
        {
            float col = tree.width / MetaProgression.BranchNames.Length;
            float x = tree.x + col * (n.Branch + .5f);
            if (MetaProgression.Fork(n) is LampNode other) x += Array.IndexOf(MetaProgression.Tree, n) < Array.IndexOf(MetaProgression.Tree, other) ? -col * .24f : col * .24f;
            float bottom = tree.yMax - 92, top = tree.y + 56;
            return new Vector2(x, Mathf.Lerp(bottom, top, n.Tier / (float)(MetaProgression.TreeTiers - 1)));
        }
        private static float NodeSize(LampNode n) => n.Capstone ? 52 : MetaProgression.Fork(n) != null ? 38 : 42;

        /// <summary>A bent ink branch from a to b (a quadratic bend, dark ink under bark; gold when lit).</summary>
        private static void Bough(Vector2 a, Vector2 b, float bend, float width, bool lit) => Bough(a, (a + b) * .5f + new Vector2(bend, 0), b, width, lit);
        /// <summary>The same, bent through a control point.</summary>
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
        private static Vector2 Q(Vector2 a, Vector2 m, Vector2 b, float t) => (1 - t) * (1 - t) * a + 2 * (1 - t) * t * m + t * t * b;

        private void DrawTree(RunCatalog catalog, PlayerProfile profile, Func<bool> save)
        {
            navCols = 1;
            Rect tree = new Rect(PanelRect.x + 26, PanelRect.y + 40, 620, PanelRect.height - 60);
            Rect card = new Rect(tree.xMax + 18, PanelRect.y + 56, PanelRect.xMax - tree.xMax - 44, PanelRect.height - 150);
            float t = Time.unscaledTime;

            // the trunk and the boughs to each branch's first lantern
            Vector2 root = new Vector2(tree.center.x, tree.yMax - 2), fork = new Vector2(tree.center.x, tree.yMax - 40);
            Bough(root, fork, 0, 24, MetaProgression.Ensure(profile).Tree.Count > 0);
            foreach (LampNode n in MetaProgression.Tree)
            {
                if (n.Tier == 0) Bough(fork, new Vector2(Mathf.Lerp(fork.x, NodeAt(n, tree).x, .85f), fork.y), NodeAt(n, tree), 12, MetaProgression.Lit(profile, n.Id));
                foreach (LampNode up in MetaProgression.Tree)
                    if (up.Branch == n.Branch && up.Tier == n.Tier + 1)
                        Bough(NodeAt(n, tree), NodeAt(up, tree), 0, up.Capstone ? 9 : 7, MetaProgression.Lit(profile, n.Id) && MetaProgression.Lit(profile, up.Id));
            }
            // branch names over their crowns
            chip.normal.textColor = ArcadeSkin.KitCream;
            float col = tree.width / MetaProgression.BranchNames.Length;
            for (int b = 0; b < MetaProgression.BranchNames.Length; b++)
            {
                Rect label = new Rect(tree.x + col * b + 4, tree.y, col - 8, 26);
                GUI.Label(label, MetaProgression.BranchNames[b], chip);
            }

            // the lanterns
            foreach (LampNode n in MetaProgression.Tree)
            {
                Vector2 at = NodeAt(n, tree); float size = NodeSize(n);
                Rect r = new Rect(at.x - size * .5f, at.y - size * .5f, size, size);
                bool lit = MetaProgression.Lit(profile, n.Id), open = MetaProgression.BranchOpen(profile, n.Branch);
                string blocked = MetaProgression.NodeBlocked(profile, n);
                bool ready = blocked == null, taken = blocked == "Other path taken";
                if (lit) ArcadeSkin.Glow(new Rect(r.x - 22, r.y - 22, r.width + 44, r.height + 44), new Color(1f, .78f, .4f, .42f + .08f * Mathf.Sin(t * 2 + n.Tier)));
                else if (ready) ArcadeSkin.Glow(new Rect(r.x - 14, r.y - 14, r.width + 28, r.height + 28), new Color(1f, .9f, .6f, .16f + .12f * Mathf.Sin(t * 3)));
                if (treePick == n.Id) ArcadeSkin.Glow(new Rect(r.x - 16, r.y - 16, r.width + 32, r.height + 32), new Color(ArcadeSkin.Gold.r, ArcadeSkin.Gold.g, ArcadeSkin.Gold.b, .6f));
                KitRound face = lit ? KitRound.Brass : !open || taken ? KitRound.Disabled : n.Currency == MetaCurrency.Keepsakes ? KitRound.Oxblood : KitRound.Walnut;
                bool tapped = ArcadeSkin.IconButton(r, face) | Nav(r);
                Rect icon = new Rect(r.x + size * .2f, r.y + size * .2f, size * .6f, size * .6f);
                Color keep = GUI.color;
                if (!open) ArcadeSkin.Lock(r.center + Vector2.up * size * .04f, size * .3f, new Color(.86f, .78f, .62f));
                else { if (taken || !lit && !ready) GUI.color = new Color(keep.r, keep.g, keep.b, keep.a * .55f); NodeIcon(icon, n); GUI.color = keep; }
                if (tapped) { treePick = n.Id; respecArmed = false; CheckpointAudio.PlayInterfaceId("ui.select"); }
            }

            // the chosen lantern's card
            LampNode pick = MetaProgression.Node(treePick) ?? Array.Find(MetaProgression.Tree, n => MetaProgression.NodeBlocked(profile, n) == null) ?? MetaProgression.Tree[0];
            TreeCard(card, pick, profile, save);

            // take the whole tree back (free; a second tap confirms)
            Rect respec = new Rect(card.x, card.yMax + 18, card.width, 46);
            bool any = profile.Meta.Tree.Count > 0;
            if ((ArcadeSkin.Button(respec, respecArmed ? "TAP AGAIN" : "TAKE BACK ALL", KitButton.Secondary, any, body) | Nav(respec, any)) && any)
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
            small.normal.textColor = new Color(ArcadeSkin.KitCream.r, ArcadeSkin.KitCream.g, ArcadeSkin.KitCream.b, .8f); small.alignment = TextAnchor.UpperCenter;
            GUI.Label(new Rect(respec.x, respec.yMax + 2, respec.width, 22), "Free · all refunded", small);
            small.alignment = TextAnchor.UpperLeft;
        }

        private void TreeCard(Rect c, LampNode n, PlayerProfile profile, Func<bool> save)
        {
            bool flash = bought == n.Id && Time.unscaledTime - boughtAt < .6f;
            if (flash) ArcadeSkin.Glow(new Rect(c.x - 12, c.y - 12, c.width + 24, c.height + 24), new Color(1f, .85f, .5f, .5f * (1 - (Time.unscaledTime - boughtAt) / .6f)));
            ArcadeSkin.Frame(c, KitFrame.Card, .3f);
            Color ink = ArcadeSkin.FrameText(KitFrame.Card);
            Rect inner = ArcadeSkin.Content(c, KitFrame.Card, true, .3f);
            NodeIcon(new Rect(inner.center.x - 36, inner.y + 4, 72, 72), n);
            body.normal.textColor = ink; small.normal.textColor = new Color(ink.r, ink.g, ink.b, .9f);
            body.alignment = TextAnchor.MiddleCenter; small.alignment = TextAnchor.UpperCenter;
            GUI.Label(new Rect(inner.x, inner.y + 84, inner.width, 28), n.Name, body);
            GUI.Label(new Rect(inner.x, inner.y + 114, inner.width, 64), n.Line, small);
            if (n.Capstone || MetaProgression.Fork(n) != null)
            {
                chip.normal.textColor = new Color(.55f, .18f, .12f);
                GUI.Label(new Rect(inner.x, inner.y + 168, inner.width, 24), n.Capstone ? "CROWN" : "PICK 1 OF 2", chip);
            }
            body.alignment = TextAnchor.UpperLeft; small.alignment = TextAnchor.UpperLeft;

            string blocked = MetaProgression.NodeBlocked(profile, n);
            Rect button = new Rect(inner.x, inner.yMax - 44, inner.width, 44);
            if (blocked == "LIT") { ArcadeSkin.Button(button, "LIT", KitButton.Secondary, false, chip); return; }
            if (blocked == "Locked")
            {
                ArcadeSkin.Lock(new Vector2(button.center.x, button.y - 34), 26, ink);
                small.normal.textColor = ink; small.alignment = TextAnchor.UpperCenter;
                GUI.Label(new Rect(inner.x, button.y - 4, inner.width, 48), "Light " + MetaProgression.BranchOpensAt + " in " + MetaProgression.BranchNames[n.Branch - 1] + "\nor win " + n.Branch + " difficult" + (n.Branch == 1 ? "y" : "ies"), small);
                small.alignment = TextAnchor.UpperLeft;
                return;
            }
            bool can = blocked == null;
            string caption = n.Cost.ToString("N0");
            if ((ArcadeSkin.Button(button, "      " + caption, can ? KitButton.Primary : KitButton.Secondary, can, body) | Nav(button, can)) && can)
            {
                string before = JsonUtility.ToJson(profile.Meta);
                if (MetaProgression.Light(profile, n.Id))
                {
                    if (!save()) { JsonUtility.FromJsonOverwrite(before, profile.Meta); CheckpointAudio.PlayInterfaceId("ui.error"); }
                    else { bought = n.Id; boughtAt = Time.unscaledTime; CheckpointAudio.PlayInterfaceId(n.Capstone ? "up.star3" : "shop.buy.gold"); }
                }
            }
            float cw = body.CalcSize(new GUIContent(caption)).x;
            RunHud.Sprite(new Rect(button.center.x - cw * .5f - 30, button.center.y - 15, 30, 30), MetaIcons.Currency(n.Currency));
            if (!can && blocked != "Not enough oil" && blocked != "Not enough keepsakes")
            {
                small.normal.textColor = new Color(.55f, .18f, .12f); small.alignment = TextAnchor.UpperCenter;
                GUI.Label(new Rect(inner.x, button.y - 26, inner.width, 22), blocked, small);
                small.alignment = TextAnchor.UpperLeft;
            }
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
                default: s = MetaIcons.LampOil; break;
            }
            RunHud.Sprite(r, s);
            if (n.Icon == "star") ArcadeSkin.Diamond(new Vector2(r.xMax - r.width * .12f, r.y + r.height * .14f), r.width * .34f, ArcadeSkin.Gold); // one star more
        }
    }
}
