using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The 36-hero review (owner 24/10/2026): what each transformation was missing, painted in ink. The second
    /// mechanics' strikes (root spikes, pollen, a mirror beam, sleep clouds, kite dives - HeroKitExtras), a spreading root
    /// network round Root Oracle, Glass Dice's three dice over its head with PAIR!/JACKPOT!, Penny Broker's rising market,
    /// Copper Cauldron's pressure and thicker steam, Tide Singer's longer wake, petals round Verdant Matron's feet, crowns on
    /// Velvet Regent's knights and sleep tokens over Drowsy Seer's dreams. No discs or rings; strokes, tokens and puffs.</summary>
    public sealed partial class HeroIdentityVisual
    {
        private float diceSeen = -10, marketStart = -10; private int marketGold;

        private void KitBoost()
        {
            HeroKit kit = sim.HeroKitInfo; if (kit == null || !sim.HeroAwakening) return;
            UnitState hero = sim.Champion;
            Vector2 at = DrawnAt(hero), feet = at - Vector2.up * hero.Definition.BodyHeight * .5f, head = at + Vector2.up * (hero.Definition.BodyHeight * .5f + .1f);
            float since = t - sim.HeroAwakenStarted, body = hero.Definition.BodyHeight;
            bool moving = (hero.Position - hero.PreviousPosition).sqrMagnitude > 1e-5f;
            Vector2 facing = hero.Facing.sqrMagnitude > .01f ? hero.Facing.normalized : Vector2.right;

            foreach (var d in sim.HeroAwakenDrops)
            {
                if (d.Kind < CombatSimulation.DropRootSpike) continue;
                float fall = Mathf.InverseLerp(d.Born, d.LandAt, t), after = Mathf.Clamp01((t - d.LandAt) / .8f);
                if (after >= 1) continue;
                switch (d.Kind)
                {
                    case CombatSimulation.DropRootSpike: RootSpike(d.At, fall, after, d.Born); break;
                    case CombatSimulation.DropPollen:
                        for (int i = 0; i < 6; i++) { float a = i * 60 + d.Born * 97, r = .15f + .55f * OutCubic(after); Token(i % 2 == 0 ? InkToken.Petal : InkToken.Glint, d.At + Dir(a) * r + Vector2.up * .35f * after, .16f * (1 - .4f * after), a + after * 120, (fall < 1 ? fall : 1 - after), true); }
                        break;
                    case CombatSimulation.DropMirrorBeam:
                        for (int i = 0; i < 4; i++) { float a = i * 90 + t * 200; Token(InkToken.Shard, d.At + Dir(a) * (.5f * (1 - fall)), .18f, a, fall < 1 ? .9f : 1 - after, true); }
                        break;
                    case CombatSimulation.DropSleep:
                        for (int i = 0; i < 3; i++) { float r = Mathf.Repeat(after * 1.2f + i / 3f, 1); Token(InkToken.Star, d.At + new Vector2((i - 1) * .3f + Mathf.Sin(r * 6 + i) * .08f, .1f + r * .8f), .15f + .08f * i, r * 90, Mathf.Sin(r * Mathf.PI) * (1 - after * .5f), true); }
                        Soft(air, InkCanvas.Line(d.At + new Vector2(-.5f, .05f), d.At + new Vector2(.5f, .05f), .08f, 2, false, d.Born), 1, 0, .05f, .6f * (1 - after));
                        break;
                    case CombatSimulation.DropKiteDive:
                    {
                        Vector2 from = d.At + new Vector2(-1.4f, 3f);
                        if (fall < 1) { var path = InkCanvas.Lob(from, d.At, .6f, 14); Soft(air, path, OutCubic(fall), Mathf.Max(0, fall - .35f), .05f, .8f); Token(InkToken.Spark, InkCanvas.Sample(InkCanvas.Lob(from, d.At, .6f, 14), fall), .3f, -60, .95f, true); }
                        else for (int i = 0; i < 5; i++) { float a = i * 72 + d.Born * 50; Token(InkToken.Spark, d.At + Dir(a) * (.2f + .5f * OutCubic(after)), .14f, a, 1 - after, true); }
                        break;
                    }
                }
            }

            switch (kit.Action)
            {
                case KitAction.Treant: // the roots spread out from its feet as the transformation goes on
                {
                    float reach = Mathf.Min(2.8f, .4f + since * .9f);
                    for (int i = 0; i < 7; i++)
                    {
                        float a = i * 51.4f + 20, wob = Mathf.Sin(t * .7f + i) * 6;
                        Vector2 tip = feet + new Vector2(Mathf.Cos((a + wob) * Mathf.Deg2Rad), Mathf.Sin((a + wob) * Mathf.Deg2Rad) * .45f) * reach;
                        groundInk.Stroke(InkCanvas.Line(feet, tip, .09f, 3.5f, false, i * 13), new InkStroke { Width = .07f, Head = 1, Tail = 0, Taper = 1.6f, Alpha = .85f * strength, Ink = new Color(.16f, .1f, .06f), Body = new Color(.38f, .24f, .13f), Light = new Color(.55f, .4f, .24f), LightOffset = .1f });
                        if (!sparse && i % 2 == 0) Token(InkToken.Thorn, tip, .14f, a, .8f, false);
                    }
                    break;
                }
                case KitAction.Casino: // the three dice over its head, tumbling as they are thrown, then showing their faces
                {
                    float rolled = t - sim.HeroAwakenDiceAt; float tumble = Mathf.Clamp01(rolled / .35f);
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 p = head + new Vector2((i - 1) * .34f, .55f + Mathf.Sin(tumble * Mathf.PI) * .25f);
                        Token(InkToken.Die, p, .3f, still ? 0 : (1 - tumble) * (360 + i * 90), .97f, true, tumble < 1 ? 1 + (int)(t * 20 + i) % 6 : sim.HeroAwakenDice[i]);
                    }
                    if (sim.HeroAwakenDiceAt > diceSeen && rolled > .35f)
                    {
                        diceSeen = sim.HeroAwakenDiceAt; int a = sim.HeroAwakenDice[0], b = sim.HeroAwakenDice[1], c = sim.HeroAwakenDice[2];
                        if (a == b && b == c) ImpactWords.Pop("JACKPOT!", head + Vector2.up * 1.1f, new Color(1f, .84f, .3f), 3, 1.3f, -4);
                        else if (a == b || b == c || a == c) ImpactWords.Pop("PAIR!", head + Vector2.up * 1f, new Color(1f, .93f, .7f), 2, 1f, 5);
                    }
                    break;
                }
                case KitAction.BullMarket: // a little market chart over its head climbs with every coin it collects
                {
                    if (marketStart != sim.HeroAwakenStarted) { marketStart = sim.HeroAwakenStarted; marketGold = sim.Gold; }
                    float gained = Mathf.Max(0, sim.Gold - marketGold), lift = Mathf.Min(1, gained / 40f);
                    Vector2 o = head + new Vector2(-.45f, .55f);
                    var pts = new System.Collections.Generic.List<Vector2>();
                    for (int i = 0; i <= 5; i++) pts.Add(o + new Vector2(i * .18f, (i / 5f) * .5f * lift + ((i * 37) % 5 - 2) * .03f));
                    Soft(air, pts, 1, 0, .045f, .9f);
                    Token(InkToken.Coin, pts[pts.Count - 1] + Vector2.up * .08f, .2f, still ? 0 : t * 120, .95f, true);
                    break;
                }
                case KitAction.Engine: // the boiler's pressure shows in thicker steam from its stack, puffing faster as it rises
                {
                    float pressure = sim.HeroAwakenPressure;
                    int n = sparse ? 3 : 6;
                    for (int i = 0; i < n; i++) { float r = Mathf.Repeat(t * (1.6f + pressure * 2f) + i / (float)n, 1); SteamPuff(head + new Vector2(-facing.x * (body * .3f + r * .6f), .05f + r * (.8f + pressure * .5f)), (.26f + r * .38f) * (1 + pressure * .5f), (1 - r) * .9f); }
                    if (moving && !sparse) for (int i = 0; i < 3; i++) { float r = Mathf.Repeat(t * 2.4f + i / 3f, 1); SteamPuff(feet - facing * (.3f + r * .9f) + Vector2.up * .1f, .2f + r * .3f, (1 - r) * .7f); }
                    break;
                }
                case KitAction.HighTide: // a longer, foaming wake behind the wave
                    if (moving && !sparse) for (int i = 0; i < 5; i++) { float r = Mathf.Repeat(t * 1.4f + i / 5f, 1); Token(InkToken.Bubble, feet - facing * (.4f + r * 1.6f) + new Vector2(0, Mathf.Sin(r * 9 + i) * .2f), .14f + .1f * (1 - r), 0, (1 - r) * .85f, false, 0, ArsenalInk.Water); }
                    break;
                case KitAction.Spring: // petals turn round its feet and rise
                    for (int i = 0; i < 6; i++) { float a = (still ? 0 : t * 50) + i * 60, r = Mathf.Repeat(t * .5f + i / 6f, 1); Token(InkToken.Petal, feet + new Vector2(Mathf.Cos(a * Mathf.Deg2Rad) * .6f, Mathf.Sin(a * Mathf.Deg2Rad) * .2f + r * .5f), .15f, a, Mathf.Sin(r * Mathf.PI) * .9f, true); }
                    break;
                case KitAction.Parade: // each knighted friend wears a little crown
                    foreach (UnitState f in sim.Units)
                    {
                        if (!f.Alive || f.Faction != Faction.Companion) continue;
                        Token(InkToken.Seal, Head(f) + Vector2.up * (.05f + Bob(f.Id) * 2), .2f, still ? 0 : Mathf.Sin(t * 2 + f.Id) * 8, .9f, true);
                    }
                    break;
                case KitAction.Feast: // what it swallows is drawn into the mouth in a spiral of ink and fangs
                {
                    Vector2 mouth = at + facing * body * .75f;
                    for (int i = 0; i < 6; i++) { float r = Mathf.Repeat(t * 1.6f + i / 6f, 1), a = i * 60 + r * 300; Token(i % 3 == 0 ? InkToken.Fang : InkToken.Puff, mouth + Dir(a) * (1.3f * (1 - r)), .16f * (1 - .5f * r), a, Mathf.Sin(r * Mathf.PI) * .85f, true); }
                    break;
                }
                case KitAction.Duel: // a stitched thread binds the hero to its chosen foe
                    if (sim.HeroAwakenFoe != null && sim.HeroAwakenFoe.Alive)
                    {
                        Vector2 foe = DrawnAt(sim.HeroAwakenFoe) + Vector2.up * .2f;
                        Soft(air, InkCanvas.Lob(at + Vector2.up * .2f, foe, .35f + Mathf.Sin(t * 3) * .05f, 16), 1, 0, .035f, .85f);
                        Token(InkToken.Button, foe + Vector2.up * (sim.HeroAwakenFoe.Definition.BodyHeight * .5f + .2f), .24f, still ? 0 : t * 90, .95f, true);
                    }
                    break;
                case KitAction.DivingBell: // a stream of bubbles round the bell
                    for (int i = 0; i < (sparse ? 3 : 6); i++) { float r = Mathf.Repeat(t * .9f + i / 6f, 1); Token(InkToken.Bubble, feet + new Vector2(((i * 37) % 7 - 3) * .14f + Mathf.Sin(r * 7 + i) * .06f, .2f + r * (body + .6f)), .1f + .08f * (i % 3), 0, Mathf.Sin(r * Mathf.PI) * .85f, true, 0, ArsenalInk.Water); }
                    break;
                case KitAction.TimeGarden: // a little gear turns over every foe the clock hand has slowed
                    foreach (UnitState u in sim.Units)
                        if (u.Alive && u.Faction == Faction.Enemy && u.KitSlowUntil > sim.Elapsed) Token(InkToken.Glint, Head(u) + Vector2.up * .05f, .18f, still ? 0 : t * 160 + u.Id * 30, .85f, true);
                    break;
                case KitAction.Dreams: // a thread of stars ties each dream to the hero
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 e = sim.HeroAwakenEchoes[i]; if ((e - hero.Position).sqrMagnitude < .1f) continue;
                        Vector2 a = at + Vector2.up * .2f, b = e + Vector2.up * .2f;
                        for (int s = 1; s < 5; s++) { float f = s / 5f; Token(InkToken.Star, Vector2.Lerp(a, b, f) + Vector2.up * Mathf.Sin(f * Mathf.PI) * .25f, .1f, t * 90 + s * 40, .6f, true); }
                    }
                    break;
            }
        }

        /// <summary>A root breaking through the floor under a foe: three thorned roots rising, curling, then sinking back.</summary>
        private void RootSpike(Vector2 at, float fall, float after, float seed)
        {
            float rise = fall < 1 ? OutBack(fall) * .6f : 1 - after * after;
            for (int i = 0; i < 3; i++)
            {
                float lean = (i - 1) * 22 + Mathf.Sin(seed * 7 + i) * 8, height = (.55f + .25f * (i % 2)) * rise;
                Vector2 b = at + new Vector2((i - 1) * .16f, -.1f), tip = b + Dir(90 + lean) * height;
                vial.Stroke(InkCanvas.Line(b, tip, .05f, 2.5f, false, seed + i), new InkStroke { Width = .09f, Head = 1, Tail = 0, Taper = 2.2f, Alpha = .95f * strength * Mathf.Clamp01(rise * 3), Ink = new Color(.14f, .09f, .05f), Body = new Color(.36f, .22f, .12f), Light = new Color(.6f, .44f, .25f), LightOffset = .12f });
                if (rise > .3f) Token(InkToken.Thorn, tip, .15f, 90 + lean, Mathf.Clamp01(rise), true);
            }
            if (fall >= 1 && after < .5f) Token(InkToken.Leaf, at + Vector2.up * .3f, .2f * (1 - after), seed * 40, 1 - after * 2, true);
        }
    }
}
