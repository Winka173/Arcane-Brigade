using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>How to Play as a little picture book (requirement 24/10/2026: "a short video and concrete descriptions; don't use one
    /// style for everything"). Seven pages, each with a live demo drawn in the game (the hero walking and fighting by itself,
    /// a gauge filling into a transformation, a stall sale, two friends merging, a bond waking, a relic landing) beside two
    /// or three concrete lines. Each page has its own colour and layout; arrows and dots turn the pages (tap or keys). Shared
    /// by the main menu and the pause menu.</summary>
    public static class HowToPlayBook
    {
        private static GUIStyle head, body, pop;
        private static readonly string[] Titles = { "Survive the Night", "Just Move", "Fill Your Gauge", "The Shop", "Merge Friends", "Wake a Bond", "Keep Relics" };
        private static readonly string[][] Lines =
        {
            new[] { "Survive 20 nights.", "Every 4th night a boss comes.", "Clearing a night heals you fully." },
            new[] { "Just move: your hero and your friends attack by themselves.", "Swords want you close; bows like distance." },
            new[] { "Fighting fills your hero's gauge.", "Full, the hero transforms for a few seconds: a new power and stronger stats." },
            new[] { "Between nights, walk up to a stall to see what it sells.", "Gold buys friends and relics. Dreamsand pays in the dream shop." },
            new[] { "Three of the same friend merge into one with a new star.", "Each star grows its body, damage and reach." },
            new[] { "Every friend belongs to families.", "Two or more of one family wake a bond for the whole brigade." },
            new[] { "Relics are bonuses that last the whole run.", "A rarer frame holds a stronger relic." },
        };
        private static readonly Color[] Accent = { new Color(.95f, .72f, .32f), new Color(.45f, .85f, .78f), new Color(1f, .6f, .35f), new Color(.98f, .82f, .45f), new Color(.7f, .8f, 1f), new Color(.75f, .95f, .6f), new Color(.9f, .6f, .95f) };
        public static int PageCount => Titles.Length;

        public static void Draw(Rect area, RunCatalog c, string heroId, ref int page)
        {
            if (head == null)
            {
                head = ArcadeTypography.Style(34, true); head.wordWrap = true;
                body = ArcadeTypography.Style(22); body.wordWrap = true;
                pop = ArcadeTypography.Style(26, true); pop.alignment = TextAnchor.MiddleCenter;
            }
            page = (page % PageCount + PageCount) % PageCount;
            if (Event.current.type == EventType.KeyDown && (Event.current.keyCode == KeyCode.RightArrow || Event.current.keyCode == KeyCode.LeftArrow)) { page = (page + (Event.current.keyCode == KeyCode.RightArrow ? 1 : PageCount - 1)) % PageCount; Event.current.Use(); }
            float scale = area.height / 680f, t = PresentationSettings.ReducedMotion ? 1.5f : Time.unscaledTime;
            Color accent = Accent[page];
            // each page its own composition: the demo on the left, on the right, or wide on top
            int layout = page % 3;
            Rect card = new Rect(area.x, area.y, area.width, area.height - 80 * scale);
            Rect demo, text;
            // review 31/10/2026: How to Play is a real picture book - the painted open book, the play picture glued on one page
            // and the words on the facing page, turning sides from page to page
            Sprite book = ArcadeSkin.Crafted ? null : ArcadeSkin.KitSprite("Panels/PictureBook");
            if (book != null)
            {
                float k = Mathf.Min(card.width / 1600f, card.height / 660f);
                Rect spread = new Rect(card.center.x - 800 * k, card.center.y - 330 * k, 1600 * k, 660 * k);
                GUI.DrawTexture(spread, book.texture, ScaleMode.ScaleToFit);
                Rect left = new Rect(spread.x + 112 * k, spread.y + 64 * k, 618 * k, 494 * k), right = new Rect(spread.x + 852 * k, spread.y + 64 * k, 600 * k, 494 * k);
                demo = page % 2 == 0 ? left : right; text = page % 2 == 0 ? right : left;
                text = new Rect(text.x + 14 * k, text.y, text.width - 28 * k, text.height);
                if (!Clip(page, demo, t)) DemoPage(page, demo, t, c, heroId, accent, scale);
                ArcadeSkin.Glow(new Rect(demo.x - 6, demo.yMax - 4, demo.width + 12, 14), new Color(0, 0, 0, .25f)); // the picture lies on the page
                BookWords(page, text, scale, accent, true);
                PageTurns(area, card, scale, ref page);
                return;
            }
            ArcadeSkin.Ticket(card, page % 2 == 0 ? ArcadeSkin.Ink : ArcadeSkin.Paper, accent);
            Rect inner = ArcadeSkin.Content(card, KitFrame.Card);
            // review 31/10/2026: the picture takes most of the page and the words are set large and centred in their column (the
            // right half used to stand nearly empty under three small lines)
            if (layout == 0) { demo = new Rect(inner.x, inner.y, inner.width * .6f, inner.height); text = new Rect(demo.xMax + 36 * scale, inner.y, inner.width * .4f - 36 * scale, inner.height); }
            else if (layout == 1) { text = new Rect(inner.x + 8 * scale, inner.y, inner.width * .4f - 8 * scale, inner.height); demo = new Rect(text.xMax + 36 * scale, inner.y, inner.width * .6f - 36 * scale, inner.height); }
            else { demo = new Rect(inner.x, inner.y, inner.width, inner.height * .6f); text = new Rect(inner.x + 24 * scale, demo.yMax + 16 * scale, inner.width - 48 * scale, inner.height * .4f - 16 * scale); }
            RunHud.Fill(demo, new Color(.05f, .06f, .09f, .55f));
            // requirement 24/10/2026: the pages show pictures taken from real play (HowToRecorder -> Resources/HowTo); the code-drawn
            // demo stands in where a picture is missing
            if (!Clip(page, demo, t)) DemoPage(page, demo, t, c, heroId, accent, scale);
            BookWords(page, text, scale, accent, false);
            PageTurns(area, card, scale, ref page);
        }

        private static void BookWords(int page, Rect text, float scale, Color accent, bool onPaper)
        {
            int layout = onPaper ? 0 : page % 3;
            Color ink = onPaper || page % 2 != 0 ? ArcadeSkin.KitInk : ArcadeSkin.Paper;
            head.fontSize = Mathf.RoundToInt(48 * scale); body.fontSize = Mathf.RoundToInt(31 * scale); pop.fontSize = Mathf.RoundToInt(30 * scale);
            head.normal.textColor = Color.Lerp(accent, ink, ink == ArcadeSkin.Paper ? .1f : .55f); body.normal.textColor = ink;
            float block = 70 * scale; // the title and the lines as one block, centred in the column (the wide layout keeps its top)
            foreach (string line in Lines[page]) block += body.CalcHeight(new GUIContent(line), text.width - 36 * scale) + 16 * scale;
            float top = layout == 2 ? text.y : text.y + Mathf.Max(0, (text.height - block) * .5f);
            GUI.Label(new Rect(text.x, top, text.width, 62 * scale), Titles[page], head);
            float y = top + 70 * scale;
            foreach (string line in Lines[page])
            {
                ArcadeSkin.Diamond(new Vector2(text.x + 10 * scale, y + 19 * scale), 16 * scale, accent);
                float h = body.CalcHeight(new GUIContent(line), text.width - 36 * scale);
                GUI.Label(new Rect(text.x + 34 * scale, y, text.width - 36 * scale, h), line, body);
                y += h + 16 * scale;
            }
        }
        private static void PageTurns(Rect area, Rect card, float scale, ref int page)
        {
            // page turning: arrows on painted round buttons and a dot per page
            float by = card.yMax + 10 * scale, bs = 64 * scale; // a thumb-sized page turn (96 units on the 1080 canvas)
            if (ArcadeSkin.IconButton(new Rect(area.x, by, bs, bs), KitRound.Walnut, "◀")) page = (page + PageCount - 1) % PageCount;
            if (ArcadeSkin.IconButton(new Rect(area.xMax - bs, by, bs, bs), KitRound.Walnut, "▶")) page = (page + 1) % PageCount;
            for (int i = 0; i < PageCount; i++)
            {
                Vector2 dot = new Vector2(area.center.x + (i - (PageCount - 1) * .5f) * 44 * scale, by + bs * .5f);
                ArcadeSkin.Diamond(dot, (i == page ? 20 : 13) * scale, i == page ? Accent[i] : new Color(.5f, .46f, .4f, .8f));
                if (TouchTargets.Button(new Rect(dot.x - 22 * scale, dot.y - 22 * scale, 44 * scale, 44 * scale), GUIContent.none, GUIStyle.none)) page = i;
            }
        }

        private static readonly Texture2D[] clips = new Texture2D[7]; private static readonly bool[] clipTried = new bool[7];
        /// <summary>Shows the page's picture of real play, fitted inside the demo area. False if none.</summary>
        private static bool Clip(int page, Rect area, float t)
        {
            if (page < 0 || page >= clips.Length) return false;
            if (!clipTried[page]) { clipTried[page] = true; clips[page] = Resources.Load<Texture2D>("HowTo/Clip" + page); }
            Texture2D atlas = clips[page]; if (atlas == null) return false;
            GUI.DrawTexture(area, atlas, ScaleMode.ScaleAndCrop); // fills its frame (no black bars round the picture)
            return true;
        }
        private static Sprite Hero(RunCatalog c, string heroId) => (heroId != null ? c.Unit(heroId)?.Body : null) ?? (c.Champions != null && c.Champions.Length > 0 ? c.Champions[0].Body : null);
        private static Sprite Enemy(RunCatalog c, int i) => c.Enemies != null && c.Enemies.Length > 0 ? c.Enemies[i % c.Enemies.Length].Body : null;
        private static Sprite Friend(RunCatalog c, int i) => c.Companions != null && c.Companions.Length > 0 ? c.Companions[(i * 7) % c.Companions.Length].Body : null;
        private static void Sprite(Vector2 centre, float size, Sprite s, float alpha = 1, bool flip = false)
        {
            if (s == null) return;
            Color keep = GUI.color; GUI.color = new Color(keep.r, keep.g, keep.b, keep.a * alpha);
            Rect r = new Rect(centre.x - size * .5f, centre.y - size * .5f, size, size);
            // (facing is not mirrored: a negative rect draws nothing on some paths)
            RunHud.Sprite(r, s);
            GUI.color = keep;
        }

        private static void DemoPage(int page, Rect d, float t, RunCatalog c, string heroId, Color accent, float scale)
        {
            Vector2 mid = d.center; float u = d.height / 400f;
            switch (page)
            {
                case 0: // twenty lanterns light one by one, the fifth of each five a boss's red
                {
                    float k = Mathf.Repeat(t / 6f, 1); int lit = Mathf.FloorToInt(k * 21);
                    for (int i = 0; i < 20; i++)
                    {
                        Vector2 p = new Vector2(d.x + 40 * u + i % 10 * (d.width - 80 * u) / 9, d.y + (i < 10 ? 120 : 210) * u);
                        bool boss = (i + 1) % 4 == 0;
                        ArcadeSkin.Diamond(p, (boss ? 30 : 20) * u, i < lit ? (boss ? new Color(.95f, .3f, .3f) : accent) : new Color(.25f, .24f, .3f));
                    }
                    Sprite(new Vector2(d.x + 40 * u + Mathf.Min(lit, 19) % 10 * (d.width - 80 * u) / 9, d.y + (lit < 10 ? 60 : 300) * u + Mathf.Abs(Mathf.Sin(t * 8)) * -6 * u), 90 * u, Hero(c, heroId));
                    break;
                }
                case 1: // the hero strolls; enemies come in; shots fly by themselves
                {
                    Vector2 hero = mid + new Vector2(Mathf.Sin(t * .8f) * d.width * .22f, Mathf.Sin(t * 1.6f) * d.height * .12f);
                    Sprite(hero, 110 * u, Hero(c, heroId), 1, Mathf.Cos(t * .8f) < 0);
                    for (int i = 0; i < 3; i++)
                    {
                        float k = Mathf.Repeat(t * .35f + i / 3f, 1);
                        Vector2 foe = new Vector2(d.xMax - 50 * u - k * d.width * .45f, d.y + (90 + i * 110) * u);
                        Sprite(foe, 80 * u, Enemy(c, i + 2), k > .85f ? (1 - k) / .15f : 1);
                        float shot = Mathf.Repeat(t * 1.5f + i * .33f, 1);
                        ArcadeSkin.Diamond(Vector2.Lerp(hero, foe, shot), 14 * u, accent);
                    }
                    break;
                }
                case 2: // the gauge fills in three steps, then the hero transforms
                {
                    float k = Mathf.Repeat(t / 5f, 1), fill = Mathf.Clamp01(k / .6f);
                    Rect gauge = new Rect(d.x + 50 * u, d.y + 70 * u, 40 * u, 260 * u);
                    RunHud.Fill(gauge, new Color(.1f, .1f, .14f, .9f));
                    for (int i = 0; i < 3; i++) { float part = Mathf.Clamp01(fill * 3 - i); RunHud.Fill(new Rect(gauge.x + 5 * u, gauge.yMax - (i + part) * 84 * u - 4 * u, gauge.width - 10 * u, part * 80 * u), accent); }
                    float trans = Mathf.Clamp01((k - .6f) / .4f);
                    Vector2 hero = mid + new Vector2(40 * u, 20 * u);
                    if (trans > 0) ArcadeSkin.Glow(new Rect(hero.x - 120 * u, hero.y - 130 * u, 240 * u, 260 * u), new Color(accent.r, accent.g, accent.b, .5f * Mathf.Sin(trans * Mathf.PI) + .2f));
                    Sprite(hero, (120 + 30 * Mathf.Sin(Mathf.Clamp01(trans * 3) * Mathf.PI * .5f)) * u, Hero(c, heroId));
                    if (trans > .05f) { pop.normal.textColor = accent; GUI.Label(new Rect(hero.x - 150 * u, d.y + 20 * u, 300 * u, 50 * u), "TRANSFORM!", pop); }
                    break;
                }
                case 3: // a stall shows a friend; coins fly to it; SOLD
                {
                    float k = Mathf.Repeat(t / 4f, 1);
                    Rect stall = new Rect(mid.x - 90 * u, d.y + 50 * u, 180 * u, 220 * u);
                    ArcadeSkin.Ticket(stall, ArcadeSkin.Paper, accent);
                    Sprite(new Vector2(stall.center.x, stall.y + 90 * u), 110 * u, Friend(c, 3), k > .75f ? 1 - (k - .75f) * 4 : 1);
                    for (int i = 0; i < 4; i++) { float f = Mathf.Clamp01((k - .2f - i * .08f) / .3f); if (f > 0 && f < 1) Sprite(Vector2.Lerp(new Vector2(d.x + 60 * u, d.yMax - 60 * u), new Vector2(stall.center.x, stall.yMax - 30 * u), f), 34 * u, c.Arena != null ? c.Arena.Coin : null); }
                    if (k > .6f) { pop.normal.textColor = new Color(.9f, .3f, .25f); GUI.Label(new Rect(stall.x, stall.yMax - 70 * u, stall.width, 50 * u), "SOLD!", pop); }
                    break;
                }
                case 4: // two of the same friend slide together into one with two stars
                {
                    float k = Mathf.Repeat(t / 3.5f, 1), meet = Mathf.SmoothStep(0, 1, Mathf.Clamp01(k / .45f));
                    Sprite friend = Friend(c, 5);
                    if (meet < 1) { Sprite(mid + new Vector2(-150 * u * (1 - meet), 0), 100 * u, friend); Sprite(mid + new Vector2(150 * u * (1 - meet), 0), 100 * u, friend, 1, true); }
                    else
                    {
                        float grow = Mathf.Clamp01((k - .45f) / .2f);
                        ArcadeSkin.Glow(new Rect(mid.x - 110 * u, mid.y - 110 * u, 220 * u, 220 * u), new Color(1f, .82f, .4f, .5f * (1 - grow) + .15f));
                        Sprite(mid, (100 + 25 * grow) * u, friend);
                        for (int s = 0; s < 2; s++) ArcadeSkin.Diamond(new Vector2(mid.x + (s - .5f) * 34 * u, mid.y - 90 * u), 26 * u * grow, new Color(1f, .8f, .3f));
                    }
                    break;
                }
                case 5: // three friends of one family; their family icon lights over each; the bond wakes
                {
                    float k = Mathf.Repeat(t / 4f, 1);
                    Sprite icon = c.Bonds != null && c.Bonds.Length > 0 ? c.Bonds[0].Icon : null;
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 p = new Vector2(d.x + d.width * (.25f + i * .25f), mid.y + 40 * u);
                        Sprite(p, 90 * u, Friend(c, 11 + i));
                        bool on = k > .15f + i * .2f;
                        Sprite(p + Vector2.up * -85 * u, 46 * u, icon, on ? 1 : .25f);
                    }
                    if (k > .45f) { pop.normal.textColor = accent; GUI.Label(new Rect(d.x, d.y + 20 * u, d.width, 50 * u), "BOND AWAKE: +DAMAGE", pop); }
                    break;
                }
                default: // a relic drops into its frame, glowing by rarity
                {
                    float k = Mathf.Repeat(t / 3.5f, 1), drop = Mathf.Clamp01(k / .35f);
                    ItemDefinition relic = c.Items != null && c.Items.Length > 0 ? c.Items[(int)(t / 3.5f) % c.Items.Length] : null;
                    Rect slot = new Rect(mid.x - 80 * u, mid.y - 60 * u, 160 * u, 160 * u);
                    RarityDescriptionFrame.Edge(slot, relic != null ? relic.Rarity : ItemRarity.Rare, t);
                    if (relic != null) Sprite(new Vector2(slot.center.x, Mathf.Lerp(d.y - 40 * u, slot.center.y, 1 - (1 - drop) * (1 - drop))), 110 * u, relic.Icon);
                    if (k > .4f && relic != null) { pop.normal.textColor = ArcadeSkin.Better; GUI.Label(new Rect(d.x, slot.yMax + 14 * u, d.width, 46 * u), relic.Name, pop); }
                    break;
                }
            }
        }
    }
}
