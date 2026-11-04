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
        private float treePickAt;
        private bool respecArmed;
        /// <summary>The branch opened across the whole panel (UI standard: lanterns a thumb can tap on a phone), or -1 for the whole tree.</summary>
        private static int focusBranch = -1;
        /// <summary>Diagnostics: opens a lantern's card.</summary>
        public void PickLantern(string id) { treePick = id; treePickAt = Time.unscaledTime; }
        /// <summary>Diagnostics: opens one branch across the panel (-1: the whole tree).</summary>
        public void FocusLampBranch(int branch) { focusBranch = branch; treePick = null; phoneFocused = true; }
        private static readonly Color TreeInk = new Color(.16f, .1f, .06f, .95f), TreeBark = new Color(.42f, .28f, .15f, 1), TreeLit = new Color(1f, .82f, .45f, 1);

        private static Rect TreeArea => new Rect(PanelRect.x + 16, PanelRect.y + 74, PanelRect.width - 32, PanelRect.height - 84);
        private static Vector2 NodeAt(LampNode n, Rect tree)
        {
            if (focusBranch >= 0 && n.Branch == focusBranch)
            {
                // one branch, tier by tier from left to right; a fork's two lanterns one above the other
                float step = (tree.width - 180) / (MetaProgression.TreeTiers - 1), y = tree.center.y + 14;
                if (MetaProgression.Fork(n) is LampNode pair) y += Array.IndexOf(MetaProgression.Tree, n) < Array.IndexOf(MetaProgression.Tree, pair) ? -78 : 78;
                return new Vector2(tree.x + 100 + n.Tier * step, y);
            }
            if (n.Branch < 0) return new Vector2(tree.center.x + (n.Tier < 2 ? -1 : 1) * (n.Tier % 3 == 0 ? 270 : 110), tree.yMax - 22); // the roots: two each side
            float col = tree.width / MetaProgression.BranchNames.Length;
            float x = tree.x + col * (n.Branch + .5f);
            if (MetaProgression.Fork(n) is LampNode other) x += Array.IndexOf(MetaProgression.Tree, n) < Array.IndexOf(MetaProgression.Tree, other) ? -col * .24f : col * .24f;
            float bottom = tree.yMax - 112, top = tree.y + 44;
            return new Vector2(x, Mathf.Lerp(bottom, top, n.Tier / (float)(MetaProgression.TreeTiers - 1)));
        }
        private static float NodeSize(LampNode n) => focusBranch >= 0 ? (n.Capstone ? 76 : MetaProgression.Fork(n) != null ? 62 : 68)
            : n.Capstone ? 46 : n.Branch < 0 ? 36 : MetaProgression.Fork(n) != null ? 34 : 40;

        /// <summary>A bent branch from a to b (a quadratic bend): the painted brass-and-vine strip laid along it, glowing when lit
        /// (review 4/11/2026: flat cream strokes read as a placeholder over the painted wood); the ink strokes without the art.</summary>
        private static void Bough(Vector2 a, Vector2 mid, Vector2 b, float width, bool lit)
        {
            Sprite vine = ArcadeSkin.Crafted ? null : ArcadeSkin.KitSprite(lit ? "Tree/VineLit" : "Tree/VineUnlit");
            if (vine != null)
            {
                const int pieces = 12; float run = 0;
                for (int i = 0; i < pieces; i++)
                {
                    float u = i / (float)pieces;
                    Vector2 p = Q(a, mid, b, u), q = Q(a, mid, b, (i + 1) / (float)pieces);
                    float length = (q - p).magnitude, thick = Mathf.Lerp(width, width * .75f, u) * 2.6f; // the vine's leaves reach past the line
                    if (length < .5f) continue;
                    float repeat = thick * vine.texture.width / vine.texture.height; // one tile's length at this thickness
                    Matrix4x4 keep = GUI.matrix;
                    ArcadeSkin.TurnAround(Mathf.Atan2(q.y - p.y, q.x - p.x) * Mathf.Rad2Deg, p);
                    GUI.DrawTextureWithTexCoords(new Rect(p.x, p.y - thick * .5f, length + 1, thick), vine.texture, new Rect(run / repeat, 0, (length + 1) / repeat, 1));
                    GUI.matrix = keep; run += length;
                }
                return;
            }
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
        /// <summary>The brass knuckle where branches meet.</summary>
        private static void Knot(Vector2 at, float size)
        {
            if (Event.current.type != EventType.Repaint || ArcadeSkin.Crafted) return;
            if (ArcadeSkin.KitSprite("Tree/VineJunction") is Sprite knot) GUI.DrawTexture(new Rect(at.x - size * .5f, at.y - size * .5f, size, size), knot.texture, ScaleMode.ScaleToFit);
        }
        private static void Bough(Vector2 a, Vector2 b, float width, bool lit) => Bough(a, (a + b) * .5f, b, width, lit);
        private static Vector2 Q(Vector2 a, Vector2 m, Vector2 b, float t) => (1 - t) * (1 - t) * a + 2 * (1 - t) * t * m + t * t * b;

        private static bool phoneFocused;
        private void DrawTree(RunCatalog catalog, PlayerProfile profile, Func<bool> save)
        {
            navCols = 1;
            MetaProgression.Ensure(profile);
            // review 31/10/2026 (nine columns of small lanterns with no room between them): the tree opens one branch at a time,
            // on the first with something to buy, its tabs above; tapping the open tab again shows the whole tree and its roots
            if (!phoneFocused && focusBranch < 0)
            {
                phoneFocused = true; focusBranch = 0;
                foreach (LampNode n in MetaProgression.Tree) if (n.Branch >= 0 && MetaProgression.NodeBlocked(profile, n) == null) { focusBranch = n.Branch; break; }
            }
            Rect tree = TreeArea;
            float t = Time.unscaledTime;
            LampNode pick = MetaProgression.Node(treePick);
            Rect card = pick != null ? CardRect(NodeAt(pick, tree)) : Rect.zero;

            bool any = profile.Meta.Lamps.Count > 0;
            // on a phone the whole tree shows its trunk and roots only; each branch's lanterns open from its tab across the panel
            // (review 3/11/2026: nine branches side by side put the lanterns about 2 mm apart)
            bool trunkOnly = RunUIHints.Phone && focusBranch < 0;
            if (focusBranch < 0)
            {
                // the trunk, the boughs to each branch's first lantern, the roots
                Vector2 fork = new Vector2(tree.center.x, tree.yMax - 70), foot = new Vector2(tree.center.x, tree.yMax - 34);
                foreach (LampNode n in MetaProgression.Roots) { Vector2 at = NodeAt(n, tree); Bough(foot, new Vector2(Mathf.Lerp(foot.x, at.x, .5f), foot.y + 4), at, 7, MetaProgression.Lit(profile, n.Id)); }
                Bough(foot, fork, 24, any);
                Knot(fork, 64); // the trunk's fork
            }
            foreach (LampNode n in MetaProgression.Tree)
            {
                if (trunkOnly || focusBranch >= 0 && n.Branch != focusBranch) continue;
                Vector2 start = focusBranch >= 0 ? new Vector2(tree.x + 14, tree.center.y + 14) : new Vector2(tree.center.x, tree.yMax - 70);
                if (n.Tier == 0)
                {
                    Bough(start, new Vector2(Mathf.Lerp(start.x, NodeAt(n, tree).x, .85f), start.y), NodeAt(n, tree), focusBranch >= 0 ? 16 : 11, MetaProgression.Lit(profile, n.Id));
                    Knot(start, focusBranch >= 0 ? 54 : 40); // the branch grows out of a brass knuckle, never out of nothing
                }
                foreach (LampNode up in MetaProgression.Tree)
                    if (up.Branch == n.Branch && up.Tier == n.Tier + 1)
                        Bough(NodeAt(n, tree), NodeAt(up, tree), focusBranch >= 0 ? (up.Capstone ? 12 : 10) : up.Capstone ? 8 : 6, MetaProgression.Lit(profile, n.Id) && MetaProgression.Lit(profile, up.Id));
            }
            // the branch names are tabs: tap one to open it across the panel, tap it again for the whole tree
            // thumb-tall tabs, each as wide as its name needs out of the row (equal widths shrank "Merchant" to a size a phone could
            // not read), names in title case at one size
            int names = MetaProgression.BranchNames.Length;
            float[] need = new float[names]; float total = 0;
            for (int b = 0; b < names; b++) { need[b] = chip.CalcSize(new GUIContent(BranchLabel(MetaProgression.BranchNames[b]))).x + 34; total += need[b]; }
            float room = tree.width - 6 * names, spare = Mathf.Max(0, room - total) / names, squeeze = Mathf.Min(1, room / total), tx = tree.x;
            for (int b = 0; b < names; b++)
            {
                float tw = need[b] * squeeze + spare;
                Rect tab = new Rect(tx + 3, tree.y - 14, tw, 44); tx += tw + 6;
                if (ArcadeSkin.Tab(tab, BranchLabel(MetaProgression.BranchNames[b]), focusBranch == b, true, chip) | Nav(tab))
                {
                    focusBranch = focusBranch == b ? -1 : b; treePick = null; CheckpointAudio.PlayInterfaceId("ui.select");
                }
            }

            // the lanterns and the roots (the roots belong to the whole tree)
            if (!trunkOnly) foreach (LampNode n in MetaProgression.Tree) if (focusBranch < 0 || n.Branch == focusBranch) Lantern(n, tree, card, profile, t);
            if (focusBranch < 0) foreach (LampNode n in MetaProgression.Roots) Lantern(n, tree, card, profile, t);

            // take the whole tree back (free; a second tap confirms): the warning plate, never the tabs' walnut
            Rect respec = new Rect(PanelRect.x + 22, PanelRect.y + 12, 214, 46);
            if ((ArcadeSkin.Button(respec, respecArmed ? "TAP AGAIN · FREE" : "TAKE BACK ALL", KitButton.Danger, any, chip) | Nav(respec, any)) && any)
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
            if (pick != null)
            {
                Matrix4x4 keepMatrix = GUI.matrix; Color keepColour = GUI.color;
                LivingUI.Pop(Time.unscaledTime - treePickAt, card.center);
                TreeCard(card, pick, profile, save);
                GUI.matrix = keepMatrix; GUI.color = keepColour;
            }
        }

        private static string BranchLabel(string name) => string.IsNullOrEmpty(name) ? name : char.ToUpperInvariant(name[0]) + name.Substring(1).ToLowerInvariant();
        private static Rect CardRect(Vector2 at)
        {
            const float w = 290, h = 300;
            float x = at.x < PanelRect.center.x ? at.x + 40 : at.x - 40 - w;
            // never over the branch tabs (they end 30 px under the tree area's top): the card starts below them
            return new Rect(x, Mathf.Clamp(at.y - h * .5f, TreeArea.y + 38, PanelRect.yMax - h - 8), w, h);
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
            if (treePick == n.Id) ArcadeSkin.Glow(new Rect(r.x - 14, r.y - 14, r.width + 28, r.height + 28), new Color(ArcadeSkin.Gold.r, ArcadeSkin.Gold.g, ArcadeSkin.Gold.b, .6f)); // the picked lantern glows gold (its card is open)
            KitRound face = lit ? KitRound.Brass : !open || taken ? KitRound.Disabled : n.Currency == MetaCurrency.Keepsakes ? KitRound.Oxblood : KitRound.Walnut;
            bool tapped = ArcadeSkin.IconButton(r, face) | Nav(r);
            // the picture centred on the medallion's disc (it sat high and to one side of it)
            Rect disc = ArcadeSkin.RoundFace(r, face), icon = new Rect(disc.center.x - disc.width * .45f, disc.center.y - disc.width * .45f, disc.width * .9f, disc.width * .9f);
            Color keep = GUI.color;
            if (!open) ArcadeSkin.Lock(disc.center, size * .3f, new Color(.86f, .78f, .62f));
            else { if (taken || !lit && !ready) GUI.color = new Color(keep.r, keep.g, keep.b, keep.a * .55f); NodeIcon(icon, n); GUI.color = keep; }
            // a branch's crown node wears the crowned laurel frame, larger than the lanterns under it
            if (focusBranch >= 0 && n.Capstone && Event.current.type == EventType.Repaint && !ArcadeSkin.Crafted && ArcadeSkin.KitSprite("Tree/CapstoneFrame") is Sprite crown) // one branch open: room for it
            {
                float fw = r.width * 1.5f, fh = fw * crown.texture.height / crown.texture.width;
                GUI.DrawTexture(new Rect(disc.center.x - fw * .5f, disc.center.y - fh * .56f, fw, fh), crown.texture, ScaleMode.ScaleToFit);
            }
            // its ranks: a count on a small plate under it (10 px lozenges could not be read on a phone; the roots: a count beside them)
            if (n.Branch < 0 && open) { small.normal.textColor = ArcadeSkin.KitCream; small.alignment = TextAnchor.MiddleLeft; GUI.Label(new Rect(r.xMax + 4, r.y, 56, r.height), rank + "/" + n.Ranks, small); small.alignment = TextAnchor.UpperLeft; }
            else if (n.Ranks > 1 && open && focusBranch < 0)
                for (int k = 0; k < n.Ranks; k++) // the whole tree is dense: small lozenges; its card says the count
                    ArcadeSkin.Diamond(new Vector2(at.x + (k - (n.Ranks - 1) * .5f) * 9, r.yMax - 1), 8, k < rank ? ArcadeSkin.Gold : new Color(.22f, .15f, .1f));
            else if (n.Ranks > 1 && open)
            {
                small.normal.textColor = rank >= n.Ranks ? ArcadeSkin.Gold : ArcadeSkin.KitCream;
                ArcadeSkin.Tag(new Rect(at.x - 28, r.yMax - 8, 56, 26), rank + "/" + n.Ranks, CodeTone.Walnut, small, small.normal.textColor);
            }
            if (tapped) { treePick = treePick == n.Id ? null : n.Id; treePickAt = Time.unscaledTime; respecArmed = false; CheckpointAudio.PlayInterfaceId("ui.select"); }
        }

        private void TreeCard(Rect c, LampNode n, PlayerProfile profile, Func<bool> save)
        {
            bool flash = bought == n.Id && Time.unscaledTime - boughtAt < .6f;
            if (flash) ArcadeSkin.Glow(new Rect(c.x - 12, c.y - 12, c.width + 24, c.height + 24), new Color(1f, .85f, .5f, .5f * (1 - (Time.unscaledTime - boughtAt) / .6f)));
            // a parchment card with a tail pointing at its lantern (review 4/11/2026: a bare rectangle with a big empty middle)
            Sprite paper = ArcadeSkin.Crafted ? null : ArcadeSkin.KitSprite("Tree/ParchmentCard"), tail = ArcadeSkin.Crafted ? null : ArcadeSkin.KitSprite("Tree/ParchmentTail");
            Color ink; Rect inner;
            if (paper != null)
            {
                if (tail != null && Event.current.type == EventType.Repaint)
                {
                    Vector2 node = NodeAt(n, TreeArea); bool left = node.x < c.x; float tw = 46, th = 36;
                    Vector2 root = new Vector2(left ? c.x + 6 : c.xMax - 6, Mathf.Clamp(node.y, c.y + 40, c.yMax - 40));
                    Matrix4x4 keepTail = GUI.matrix; ArcadeSkin.TurnAround(left ? 90 : -90, root);
                    GUI.DrawTexture(new Rect(root.x - tw * .5f, root.y - 2, tw, th), tail.texture, ScaleMode.StretchToFill);
                    GUI.matrix = keepTail;
                }
                ArcadeSkin.Sliced(c, paper, .4f);
                ink = ArcadeSkin.KitInk; inner = new Rect(c.x + 26, c.y + 24, c.width - 52, c.height - 48);
            }
            else
            {
                ArcadeSkin.Frame(c, KitFrame.Card, .3f);
                ink = ArcadeSkin.FrameText(KitFrame.Card);
                inner = ArcadeSkin.Content(c, KitFrame.Card, true, .3f);
            }
            Rect close = new Rect(c.xMax - 56, c.y + 10, 44, 44); // inside the parchment's corner, not over its edge
            NodeIcon(new Rect(inner.center.x - 28, inner.y, 56, 56), n);
            body.normal.textColor = ink; small.normal.textColor = new Color(ink.r, ink.g, ink.b, .9f);
            body.alignment = TextAnchor.MiddleCenter; small.alignment = TextAnchor.UpperCenter;
            GUI.Label(new Rect(inner.x, inner.y + 58, inner.width, 26), n.Name, body);
            // the lines flow (a three-line effect pushed the tag over its last line)
            float lineHeight = Mathf.Max(26, small.CalcHeight(new GUIContent(n.Line), inner.width));
            GUI.Label(new Rect(inner.x, inner.y + 84, inner.width, lineHeight), n.Line, small);
            int rank = MetaProgression.LampRank(profile, n.Id);
            string tag = n.Capstone ? "THE BRANCH'S CROWN" : MetaProgression.Fork(n) != null ? "PICK 1 OF 2" : n.Ranks > 1 ? "RANK " + rank + "/" + n.Ranks : null;
            if (tag != null) { chip.normal.textColor = new Color(.55f, .18f, .12f); GUI.Label(new Rect(inner.x, inner.y + 88 + lineHeight, inner.width, 22), tag, chip); }
            body.alignment = TextAnchor.UpperLeft; small.alignment = TextAnchor.UpperLeft;

            string blocked = MetaProgression.NodeBlocked(profile, n);
            Rect button = new Rect(inner.x, inner.yMax - 42, inner.width, 42);
            if (blocked == "MAX")
            {
                // finished: the brass laurel stamp and the word, never a dead grey button (it read as "can't")
                Sprite done = ArcadeSkin.Crafted ? null : ArcadeSkin.KitSprite("Info/MasteredStamp");
                if (done != null) RunHud.Sprite(new Rect(button.x, button.center.y - 24, 48, 48), done);
                chip.normal.textColor = new Color(.45f, .3f, .08f);
                GUI.Label(new Rect(button.x + (done != null ? 56 : 0), button.y, button.width - (done != null ? 56 : 0), button.height), n.Ranks > 1 ? "MASTERED" : "LIT", chip);
            }
            else if (blocked == "Locked")
            {
                small.normal.textColor = ink; small.alignment = TextAnchor.UpperCenter;
                // plain words (review 31/10/2026: "Light 3 in FORGE or win 7 difficulties" read as nothing, and there are only five
                // difficulties): which branch to buy in first, and the difficulty path only when it can happen
                string before = RunUICopy.Title(MetaProgression.BranchNames[Mathf.Max(0, n.Branch - 1)]);
                string why = n.Branch < 0 ? "Opens when every branch is fully lit"
                    : "Opens after " + MetaProgression.BranchOpensAt + " upgrades on the " + before + " branch"
                      + (n.Branch <= RunMeta.DifficultyNames.Length ? "\nor after winning on " + n.Branch + (n.Branch == 1 ? " difficulty" : " different difficulties") : "");
                // the reason sits on the card's foot, the padlock just above it (it ran off the card's bottom)
                float whyHeight = small.CalcHeight(new GUIContent(why), inner.width);
                ArcadeSkin.Lock(new Vector2(button.center.x, inner.yMax - whyHeight - 16), 20, ink);
                GUI.Label(new Rect(inner.x, inner.yMax - whyHeight, inner.width, whyHeight), why, small);
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
                    GUI.Label(new Rect(inner.x, button.y - 44, inner.width, 26), blocked, small); // clear of the button's crest
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
