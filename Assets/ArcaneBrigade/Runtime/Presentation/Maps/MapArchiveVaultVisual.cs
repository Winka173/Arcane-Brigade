using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    // The Archive Vault's look (requirement 25/10/2026: each map draws in its own file; MapEnvironmentVisual and MapAtmosphere
    // dispatch here). Props come from Resources/Environment/ArchiveVault: Page_A, Page_B, Inkpot_Up, Inkpot_Tipped, Stamp_Giant
    // (and the old Clock_Idle, now scenery); until they are painted each has a code stand-in. The ink fog and the ink pool are
    // a shader (Resources/Shaders/MapArchiveVaultInk) with an ink fallback; every other effect is ink.

    public sealed partial class MapEnvironmentVisual
    {
        private static readonly InkPalette VaultInkPal = new InkPalette(new Color(.04f, .02f, .08f), new Color(.22f, .13f, .34f), new Color(.72f, .62f, .95f), new Color(.55f, .4f, .85f), new Color(.9f, .6f, .85f));
        private static readonly InkPalette VaultWormPal = new InkPalette(new Color(.12f, .05f, .18f), new Color(.6f, .42f, .82f), new Color(.95f, .88f, 1f), new Color(.75f, .6f, 1f), new Color(.98f, .9f, .7f));
        private static readonly InkToken[] VaultKindTokens = { InkToken.Star, InkToken.Spark, InkToken.Seal, InkToken.Coin, InkToken.Bolt };
        private static readonly int[] VaultLaneIds = { Shader.PropertyToID("_Lane0"), Shader.PropertyToID("_Lane1"), Shader.PropertyToID("_Lane2") };
        private static readonly Color VaultViolet = new Color(.8f, .64f, 1f);
        private static Mesh vaultQuad;
        private Material vaultInk;
        private bool vaultInkTried;
        private float vaultChapterSeen = -100, vaultStampSeen = -100, vaultSpillSeen = -100;
        private int vaultChainShown;
        private readonly Dictionary<int, int> vaultPagesSeen = new Dictionary<int, int>();

        private void DrawArchiveVault(MapEnvironmentState s, float t)
        {
            Camera cam = Camera.main;
            bool ortho = cam != null && cam.orthographic;
            Vector2 view = cam != null ? (Vector2)cam.transform.position : sim.Champion != null ? sim.Champion.Position : s.Anchor;
            float halfH = ortho ? cam.orthographicSize : 6, halfW = halfH * (cam != null ? cam.aspect : 1.78f);
            float now = still ? 0 : t;
            // scenery: the map's fixed rows of shelves (if any) and the old clock, its chime gone
            foreach (Vector2 piece in s.WallProps) Put("ArchiveVault/Shelf_Straight", piece, 2.4f);
            VaultStand("ArchiveVault/Clock_Idle", s.VaultClock, 2.6f);
            VaultFloor(s, t, now, ortho ? cam : null, view, halfW, halfH);
            VaultPotDraw(s, t, now);
            VaultPagesDraw(s, t, now);
            VaultWormsDraw(s, t, now);
            VaultBurstsDraw(s, t);
            VaultStampDraw(s, t, now);
            VaultChapterDraw(s, t, now);
        }
        /// <summary>How much the map's own light lifts a standing prop at this spot (0 none, 1 fully lit).</summary>
        private float ArchiveVaultPropLight(MapEnvironmentState s, Vector2 at) => 0;

        private static float VaultOut(float x) { x = Mathf.Clamp01(x); return 1 - (1 - x) * (1 - x) * (1 - x); }
        private static InkPalette VaultKindPalette(int kind) => kind switch { 0 => ArsenalInk.Amber, 1 => ArsenalInk.Gold, 2 => ArsenalInk.Silver, 3 => ArsenalInk.Gold, _ => ArsenalInk.Cyan };
        private static Vector2 VaultTurn(Vector2 v, float degrees)
        {
            float a = degrees * Mathf.Deg2Rad, c = Mathf.Cos(a), s = Mathf.Sin(a);
            return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
        }
        /// <summary>A painted prop standing on the floor, its shadow cast from its foot.</summary>
        private SpriteRenderer VaultStand(string path, Vector2 at, float height, float alpha = 1, float angle = 0, bool flip = false)
        {
            int slot = used; bool known = slot < shadows.Count;
            if (known) shadows[slot].PinCastToFoot = true;
            SpriteRenderer r = Put(path, at, height, alpha: alpha, flip: flip, angle: angle);
            if (known) shadows[slot].PinCastToFoot = false;
            return r;
        }
        private static Mesh VaultQuad()
        {
            if (vaultQuad != null) return vaultQuad;
            vaultQuad = new Mesh { name = "Vault ink quad", hideFlags = HideFlags.DontSave };
            vaultQuad.vertices = new[] { new Vector3(-.5f, -.5f, 0), new Vector3(.5f, -.5f, 0), new Vector3(.5f, .5f, 0), new Vector3(-.5f, .5f, 0) };
            vaultQuad.triangles = new[] { 0, 2, 1, 0, 3, 2 }; vaultQuad.RecalculateBounds();
            return vaultQuad;
        }

        // ------------------------------------------------------------------ ink fog and the ink pool on the floor
        private void VaultFloor(MapEnvironmentState s, float t, float now, Camera cam, Vector2 view, float halfW, float halfH)
        {
            float amount = (PresentationSettings.ReducedEffects ? .5f : .75f) * (PresentationSettings.HighReadability ? .65f : 1);
            bool pool = t >= s.VaultSpillAt && t < s.VaultPoolUntil;
            float R = MapEnvironmentRules.VaultPoolRadius;
            float radius = pool ? R * (still ? 1 : VaultOut((t - s.VaultSpillAt) / .45f)) : 0; // it runs out of the pot in a moment
            float strength = pool ? Mathf.Clamp01((s.VaultPoolUntil - t) / .6f) : 0;
            float half = MapEnvironmentRules.VaultFogHalf;
            if (!vaultInkTried)
            {
                vaultInkTried = true;
                Shader shader = Resources.Load<Shader>("Shaders/MapArchiveVaultInk");
                if (shader != null && shader.isSupported) vaultInk = new Material(shader) { name = "Vault ink fog", hideFlags = HideFlags.DontSave };
            }
            if (vaultInk != null && cam != null)
            {
                Material m = vaultInk; Vector4 halves = Vector4.zero;
                for (int i = 0; i < 3; i++)
                {
                    if (i < s.VaultFog.Count) { MapEnvironmentState.VaultLane lane = s.VaultFog[i]; m.SetVector(VaultLaneIds[i], new Vector4(lane.A.x, lane.A.y, lane.B.x, lane.B.y)); halves[i] = half; }
                    else m.SetVector(VaultLaneIds[i], Vector4.zero);
                }
                m.SetVector("_LaneHalf", halves);
                m.SetVector("_Pool", new Vector4(s.VaultPool.x, s.VaultPool.y, radius, strength));
                m.SetColor("_Ink", new Color(.1f, .07f, .2f)); m.SetColor("_Sheen", new Color(.44f, .34f, .64f));
                m.SetColor("_PoolInk", new Color(.11f, .05f, .19f)); m.SetColor("_PoolGloss", new Color(.55f, .45f, .85f));
                m.SetFloat("_Clock", now); m.SetFloat("_Amount", amount);
                // drawn this frame only, under the floor marks' ink and every body (sorting order 0)
                Graphics.DrawMesh(VaultQuad(), Matrix4x4.TRS(new Vector3(view.x, view.y, 0), Quaternion.identity, new Vector3(halfW * 2 + 2, halfH * 2 + 2, 1)), m, 0, cam);
                Marks++;
            }
            else
            {
                // without the shader: each lane one solid wash of ink mist with a few wisps drifting along it, the pool one solid fill
                Color mist = new Color(.18f, .12f, .3f, .42f * amount);
                for (int i = 0; i < s.VaultFog.Count; i++)
                {
                    MapEnvironmentState.VaultLane lane = s.VaultFog[i];
                    ink.FloorLane(lane.A, lane.B, half, mist, mist);
                    Vector2 along = lane.B - lane.A;
                    for (int k = 0; k < 4; k++)
                    {
                        float u = Mathf.Repeat(k * .25f + now * .04f, 1);
                        Vector2 at = lane.A + along * u + new Vector2(-along.y, along.x).normalized * Mathf.Sin(k * 2.3f + now) * half * .4f;
                        ink.FloorStroke(at, at + along.normalized * 1.4f, VaultInkPal, .08f, 1, 0, .5f * Mathf.Sin(u * Mathf.PI), .04f, i * 4 + k);
                    }
                    Marks++;
                }
                if (pool)
                {
                    Color fill = new Color(.11f, .05f, .19f, .78f * strength);
                    ink.FloorZone(s.VaultPool, radius, radius, fill, fill);
                    ink.FloorStroke(s.VaultPool + new Vector2(-radius * .5f, radius * .3f), s.VaultPool + new Vector2(-radius * .1f, radius * .45f), VaultInkPal, .07f, 1, 0, .6f * strength);
                    Marks++;
                }
            }
            if (!pool) return;
            // bubbles rising from the ink, and every enemy standing in it shines with it (it runs faster and hits harder)
            ink.Bubbles(s.VaultPool + new Vector2(-R * .3f, 0), R * .6f, now, VaultInkPal, 3, 1, .85f * strength);
            ink.Bubbles(s.VaultPool + new Vector2(R * .35f, -R * .2f), R * .5f, now, VaultInkPal, 2, 3, .85f * strength);
            int shown = 0;
            foreach (UnitState u in sim.Units)
            {
                if (shown >= 20) break;
                if (!u.Alive || u.Faction != Faction.Enemy || !sim.InVaultPool(u.Position)) continue;
                ink.TokenAt(InkToken.Drop, VaultInkPal, Head(u) + Vector2.up * .14f, .22f, 0, .9f);
                shown++;
            }
        }

        // ------------------------------------------------------------------ the giant inkpot
        private void VaultPotDraw(MapEnvironmentState s, float t, float now)
        {
            float warn = t - s.VaultPotAt;
            if (warn < 0 || t >= s.VaultPoolUntil + .6f) return;
            float R = MapEnvironmentRules.VaultPoolRadius;
            bool right = s.VaultPool.x > s.VaultPot.x;
            float tipSign = right ? -1 : 1; // it tips over toward the pool
            Vector2 mouth = s.VaultPot + new Vector2(right ? .75f : -.75f, .35f);
            if (t < s.VaultSpillAt)
            {
                float p = warn / MapEnvironmentRules.VaultPotWarn;
                // where the ink will run: one solid violet area filling as the pot rocks harder (violet: it helps the enemies)
                Ring(s.VaultPool, R, R, p, ArsenalInk.Violet, .85f);
                float rock = still ? 0 : Mathf.Sin(now * (9 + 14 * p)) * (2 + 9 * p * p);
                if (VaultStand("ArchiveVault/Inkpot_Up", s.VaultPot, 1.9f, angle: rock) == null) VaultPotInk(s.VaultPot, rock, 1);
                // a drip creeping over its lip
                Vector2 lip = s.VaultPot + VaultTurn(new Vector2(right ? .3f : -.3f, 1.55f), rock);
                ink.Stroke(lip, lip + Vector2.down * (.2f + .5f * p), VaultInkPal, .08f, 1, 0, .9f);
                return;
            }
            float tip = Mathf.Clamp01((t - s.VaultSpillAt) / .3f), fade = Mathf.Clamp01((s.VaultPoolUntil + .6f - t) / .6f);
            if (tip < 1)
            {
                float angle = tipSign * 90 * tip * tip;
                if (Put("ArchiveVault/Inkpot_Up", s.VaultPot, 1.9f, Air - 8, 1, angle: angle) == null) VaultPotInk(s.VaultPot, angle, 1);
            }
            else if (VaultStand("ArchiveVault/Inkpot_Tipped", s.VaultPot, 1.3f, fade, flip: !right) == null) VaultPotInk(s.VaultPot, tipSign * 90, fade);
            if (s.VaultSpillAt > vaultSpillSeen + .01f)
            {
                vaultSpillSeen = s.VaultSpillAt;
                if (t - s.VaultSpillAt < .5f) ImpactWords.Pop("INK SPILL!", s.VaultPool + Vector2.up * 1.4f, VaultViolet, 2, 1f);
            }
            // the ink gushing out of its mouth into the pool
            float gush = (t - s.VaultSpillAt) / .7f;
            if (gush >= 0 && gush < 1)
            {
                var path = InkCanvas.Lob(mouth, s.VaultPool, .6f);
                float head = Mathf.Max(.03f, VaultOut(gush * 1.6f));
                ink.Ribbon(path, VaultInkPal, .22f, head, Mathf.Max(0, (gush - .45f) / .55f * head), 1, false);
            }
        }
        private void VaultPotInk(Vector2 at, float angle, float alpha)
        {
            // a squat glass inkpot with a cream label (until Inkpot_Up / Inkpot_Tipped are painted)
            Props++;
            ink.Stroke(at + VaultTurn(new Vector2(-.45f, .45f), angle), at + VaultTurn(new Vector2(.45f, .45f), angle), VaultInkPal, .8f, 1, 0, alpha);
            ink.Stroke(at + VaultTurn(new Vector2(0, .85f), angle), at + VaultTurn(new Vector2(0, 1.2f), angle), VaultInkPal, .36f, 1, 0, alpha);
            ink.Stroke(at + VaultTurn(new Vector2(-.3f, .45f), angle), at + VaultTurn(new Vector2(.3f, .45f), angle), ArsenalInk.Cream, .18f, 1, 0, alpha);
        }

        // ------------------------------------------------------------------ flying pages
        private void VaultPagesDraw(MapEnvironmentState s, float t, float now)
        {
            foreach (MapEnvironmentState.VaultPage page in s.VaultPages)
            {
                Vector2 at = page.Position; int kind = page.Kind;
                if (t < page.LandAt)
                {
                    // in the air: fluttering above its own shadow, a glint trailing it
                    float f = Mathf.Clamp01((t - page.Born) / Mathf.Max(.01f, page.LandAt - page.Born));
                    float lift = page.Spat ? .6f * Mathf.Sin(f * Mathf.PI) : .65f * (1 - f * f) + .08f;
                    ink.Wash(at, .3f, new Color(.03f, .01f, .06f), .28f);
                    float flutter = still ? 0 : Mathf.Sin(now * 7 + kind * 1.3f + page.Born) * 28;
                    VaultPageSprite(kind, at + Vector2.up * lift, Air, flutter, 1);
                    ink.TokenAt(VaultKindTokens[kind % 5], VaultKindPalette(kind), at + Vector2.up * lift + new Vector2(.34f, .22f), .24f, 0, .95f);
                    Vector2 back = (page.From - page.To).normalized;
                    if (!page.Spat) ink.TokenAt(InkToken.Glint, ArsenalInk.Cream, at + Vector2.up * lift + back * .45f, .14f, now * 90, .7f);
                    continue;
                }
                // on the floor: it glows to be picked up and blinks in its last two seconds
                float left = page.LandAt + MapEnvironmentRules.VaultPageFloor - t;
                float blink = left < 2 && !calm && !still ? (Mathf.Sin(t * 14) > 0 ? 1 : .45f) : 1;
                ink.Light(at, .95f, new Color(1, .93f, .75f), .22f * blink);
                VaultPageSprite(kind, at, 60, kind * 17 - 20, blink);
                ink.TokenAt(VaultKindTokens[kind % 5], VaultKindPalette(kind), at + new Vector2(.32f, .2f + (still ? 0 : Mathf.Sin(now * 2.5f + kind) * .04f)), .24f, 0, .95f * blink);
            }
        }
        private void VaultPageSprite(int kind, Vector2 at, int order, float angle, float alpha)
        {
            if (Put(kind % 2 == 0 ? "ArchiveVault/Page_A" : "ArchiveVault/Page_B", at, .55f, order, alpha, angle: angle) == null)
            { ink.TokenAt(InkToken.Page, ArsenalInk.Cream, at, .55f, angle, alpha); Props++; }
        }
        private void VaultChapterDraw(MapEnvironmentState s, float t, float now)
        {
            UnitState hero = sim.Champion; if (hero == null) return;
            Vector2 head = Head(hero) + Vector2.up * .6f;
            // the pages toward a CHAPTER over the hero: one more filled with each catch within 30 s
            int caught = 0;
            foreach (Vector2 c in s.VaultCatches) if (c.x > t - MapEnvironmentRules.VaultChapterWindow) caught++;
            if (caught > 0 && caught < MapEnvironmentRules.VaultChapterPages)
                for (int k = 0; k < MapEnvironmentRules.VaultChapterPages; k++)
                    ink.TokenAt(InkToken.Page, k < caught ? ArsenalInk.Cream : ArsenalInk.Midnight, head + new Vector2((k - 1) * .3f, k == 1 ? .1f : 0), .26f, -8 + 8 * k, k < caught ? 1 : .4f);
            // a catch flutters
            float caughtAge = t - s.VaultCaughtAt;
            if (caughtAge >= 0 && caughtAge < .5f) ink.Draw(ArsenalInk.Moment.PaperScraps, s.VaultCaughtWhere, Vector2.up, caughtAge / .5f, .55f, s.VaultCaught * 37);
            float age = t - s.VaultChapterAt;
            if (s.VaultChapterAt > vaultChapterSeen + .01f)
            {
                vaultChapterSeen = s.VaultChapterAt;
                if (age < 1) ImpactWords.Pop("CHAPTER!", head + Vector2.up * .35f, ImpactWords.Gold, 3, 1.15f);
            }
            if (age < 0 || age >= 1.3f) return;
            // the chapter's pages whirl round the hero and fly off
            int n = PresentationSettings.ReducedEffects ? 3 : 6;
            for (int k = 0; k < n; k++)
            {
                float a = (now * 240 + k * 360f / n) * Mathf.Deg2Rad, r = .9f + .8f * age;
                ink.TokenAt(InkToken.Page, ArsenalInk.Cream, hero.Position + Vector2.up * .6f + new Vector2(Mathf.Cos(a), Mathf.Sin(a) * .5f) * r, .32f, a * 57, 1 - (age / 1.3f) * (age / 1.3f));
            }
        }

        // ------------------------------------------------------------------ bookworms
        private void VaultWormsDraw(MapEnvironmentState s, float t, float now)
        {
            if (vaultPagesSeen.Count > 64) vaultPagesSeen.Clear();
            int shown = 0;
            foreach (KeyValuePair<int, MapEnvironmentState.VaultWorm> pair in s.VaultWorms)
            {
                if (shown >= 12) break;
                UnitState u = sim.UnitForPresentation(pair.Key); if (u == null || !u.Alive) continue;
                MapEnvironmentState.VaultWorm worm = pair.Value; shown++;
                // a little violet bookworm inching over its head, the pages it ate stacked beside it
                Vector2 h = Head(u) + Vector2.up * .14f;
                float inch = still ? .1f : .07f + .08f * Mathf.Abs(Mathf.Sin(now * 5 + pair.Key));
                Vector2 tail = h + new Vector2(-.2f, 0), nose = h + new Vector2(.2f, 0);
                ink.Ribbon(InkCanvas.Lob(tail, nose, inch), VaultWormPal, .1f, 1, 0, .95f, false);
                ink.Place(MechanicInk.Glow, nose + Vector2.up * .03f, .2f, 0, new Color(.88f, .8f, 1f, .95f), true);
                ink.Stroke(nose + new Vector2(.03f, .07f), nose + new Vector2(.03f, .04f), ArsenalInk.Midnight, .045f, 1, 0, .9f);
                for (int k = 0; k < worm.Pages; k++)
                    ink.TokenAt(InkToken.Page, ArsenalInk.Cream, h + new Vector2(.42f + .06f * k, -.06f + .1f * k), .26f, -12 + 9 * k, .95f);
                if (vaultPagesSeen.TryGetValue(pair.Key, out int seen) && worm.Pages > seen)
                    ImpactWords.Pop("GULP!", Head(u) + Vector2.up * .55f, VaultViolet, 1, .8f);
                vaultPagesSeen[pair.Key] = worm.Pages;
                // the page it just ate is torn to scraps at its mouth
                float ate = t - worm.AteAt;
                if (ate >= 0 && ate < .6f) ink.Draw(ArsenalInk.Moment.PaperScraps, nose + Vector2.up * .1f, Vector2.up, ate / .6f, .8f, pair.Key * 13);
                // a hungry one shows the way to the page it crawls to
                if (worm.Pages >= MapEnvironmentRules.VaultWormPages) continue;
                MapEnvironmentState.VaultPage target = null; float near = float.MaxValue;
                foreach (MapEnvironmentState.VaultPage page in s.VaultPages)
                {
                    if (t < page.LandAt) continue;
                    float d = (page.To - u.Position).sqrMagnitude;
                    if (d < near) { near = d; target = page; }
                }
                if (target != null && near < 225) Dash(u.Position, target.To, .55f, ArsenalInk.Violet, .55f);
            }
            // a fallen bookworm's pages burst back out
            foreach (Vector3 spit in s.VaultSpits)
            {
                float age = t - spit.z; if (age < 0 || age >= .7f) continue;
                ink.Draw(ArsenalInk.Moment.PaperScraps, new Vector2(spit.x, spit.y), Vector2.up, age / .7f, .9f, spit.x * 31);
            }
        }

        // ------------------------------------------------------------------ the ink bursts
        private void VaultBurstsDraw(MapEnvironmentState s, float t)
        {
            int recent = 0; Vector2 last = Vector2.zero;
            foreach (Vector3 b in s.VaultBursts)
            {
                float age = t - b.z; if (age < 0 || age >= .55f) continue;
                Vector2 at = new Vector2(b.x, b.y); float p = age / .55f;
                if (age < .8f) { recent++; last = at; }
                ink.Light(at, 2.4f, new Color(.55f, .4f, .85f), .4f * (1 - p));
                int n = PresentationSettings.ReducedEffects ? 5 : 9; float reach = MapEnvironmentRules.VaultBurstRadius, seed = b.x * 37 + b.y * 11;
                for (int i = 0; i < n; i++)
                {
                    float a = (seed + i * 360f / n) * Mathf.Deg2Rad, d = reach * (still ? .7f : VaultOut(p)) * (.6f + .2f * (i % 3));
                    Vector2 dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a) * .6f);
                    ink.TokenAt(i % 2 == 0 ? InkToken.Drop : InkToken.Bubble, VaultInkPal, at + dir * d + Vector2.down * (still ? 0 : .3f * p * p), .2f * (1 - .4f * p), a * 57 + p * 180, 1 - p * p);
                    if (i % 2 == 0) ink.Stroke(at + dir * (.2f + .3f * p), at + dir * (.45f + .7f * p), VaultInkPal, .06f, 1, 0, 1 - p);
                }
            }
            // two or more in a row: the chain is named
            if (recent >= 2 && recent > vaultChainShown) { ImpactWords.Pop("INK CHAIN ×" + recent, last + Vector2.up * 1f, VaultViolet, 2, .95f); vaultChainShown = recent; }
            else if (recent == 0) vaultChainShown = 0;
        }

        // ------------------------------------------------------------------ the stamp from the sky
        private void VaultStampDraw(MapEnvironmentState s, float t, float now)
        {
            // every enemy it marked PROCESSED wears a wine seal over its head
            int marks = 0;
            foreach (KeyValuePair<int, float> mark in s.VaultProcessed)
            {
                if (mark.Value <= t || marks >= 30) continue;
                UnitState u = sim.UnitForPresentation(mark.Key); if (u == null || !u.Alive) continue;
                ink.TokenAt(InkToken.Seal, ArsenalInk.Wine, Head(u) + Vector2.up * .18f, .3f, -12, .95f * Mathf.Clamp01((mark.Value - t) / .5f));
                marks++;
            }
            float warn = t - s.VaultStampAt, W = MapEnvironmentRules.VaultStampWarn, R = MapEnvironmentRules.VaultStampRadius;
            if (s.VaultStampAt < 0 || warn < 0 || warn > W + 2.6f) return;
            Vector2 c = s.VaultStamp;
            if (warn < W)
            {
                float p = warn / W;
                // where it falls: one solid amber area filling (it never hurts the hero), its shadow gathering in it
                Ring(c, R, R, p, ArsenalInk.Amber, .9f);
                ink.Wash(c, R * (.3f + .45f * p), new Color(.05f, .02f, .08f), .3f * p);
                float fall = p * p;
                VaultStampSprite(c + Vector2.up * (7.5f * (1 - fall) + .15f), Mathf.Clamp01(p * 3), 0);
                return;
            }
            float after = warn - W;
            if (s.VaultStampAt > vaultStampSeen + .01f)
            {
                vaultStampSeen = s.VaultStampAt;
                if (after < .5f) ImpactWords.Pop("PROCESSED!", c + Vector2.up * 1.3f, new Color(1f, .45f, .42f), 3, 1.15f);
            }
            // the imprint it leaves: a solid wine stamp face with cream lines of type, fading
            float fade = 1 - Mathf.Clamp01(after / 2.6f);
            Color face = new Color(.6f, .12f, .2f, .55f * fade);
            ink.FloorLane(c + new Vector2(-R * .55f, 0), c + new Vector2(R * .55f, 0), R * .38f, face, face); Marks++;
            for (int k = -1; k <= 1; k++) ink.FloorStroke(c + new Vector2(-R * .4f, k * .3f), c + new Vector2(R * (k == 0 ? .4f : .25f), k * .3f), ArsenalInk.Cream, .07f, 1, 0, .8f * fade);
            // it presses, then lifts away; dust kicks up round it
            float lift = Mathf.Clamp01((after - .25f) / .6f);
            if (lift < 1) VaultStampSprite(c + Vector2.up * (lift * lift * 7.5f), 1 - lift, after < .12f && !still ? .9f : 1);
            if (after < .7f)
                for (int k = 0; k < 4; k++)
                {
                    float a = (k * 90 + 45) * Mathf.Deg2Rad;
                    ink.Draw(ArsenalInk.Moment.BrickDust, c + new Vector2(Mathf.Cos(a) * R * .85f, Mathf.Sin(a) * R * .5f), Vector2.up, after / .7f, .8f, k * 41);
                }
        }
        private void VaultStampSprite(Vector2 at, float alpha, float squash)
        {
            SpriteRenderer r = Put("ArchiveVault/Stamp_Giant", at, 2.8f, Air - 6, alpha);
            if (r != null)
            {
                if (squash > 0 && squash < 1) r.transform.localScale = new Vector3(r.transform.localScale.x / squash, r.transform.localScale.y * squash, 1);
                return;
            }
            // a giant rubber stamp: a turned wooden handle on a wine block (until Stamp_Giant is painted)
            Props++;
            ink.Stroke(at + new Vector2(0, 2.4f), at + new Vector2(0, 1.1f), ArsenalInk.Leather, .42f, 1, 0, alpha);
            ink.Stroke(at + new Vector2(-1.1f, .55f), at + new Vector2(1.1f, .55f), ArsenalInk.Wine, .8f, 1, 0, alpha);
            ink.Stroke(at + new Vector2(-1.15f, .12f), at + new Vector2(1.15f, .12f), ArsenalInk.Gold, .14f, 1, 0, alpha);
        }
    }

    public sealed partial class MapAtmosphere
    {
        /// <summary>Heavy dusk haze, a warm lamp glow by the old clock, ink motes drifting up and paper flecks turning.</summary>
        private void AirArchiveVault(MapEnvironmentState s, int motes, float clock)
        {
            Grade(new Color(.86f, .58f, .48f, .1f), new Color(.13f, .05f, .15f, .42f));
            Put(MechanicInk.Glow, s.VaultClock + Vector2.up * 1.4f, Vector2.one * 3.2f, 0, new Color(1f, .8f, .55f, .08f), HazeOrder);
            // heavy haze banks
            for (int i = 0; i < 4; i++)
            {
                Vector2 at = Wrap(new Vector2(H(i, 1) * 30, 0), new Vector2(-.08f, 0)); at.y = centre.y + Mathf.Lerp(-.7f, .7f, i / 3f) * halfH;
                Put(MechanicInk.Glow, at, new Vector2(halfW * 1.3f, 2.2f), 0, new Color(.74f, .6f, .7f, .12f), HazeOrder);
            }
            // ink motes drifting up, and every fourth a paper fleck turning as it rises
            for (int i = 0; i < motes; i++)
            {
                Vector2 at = Wrap(new Vector2(H(i, 2) * 40, H(i, 3) * 40), new Vector2(.04f * (H(i, 4) - .5f), .16f + .1f * H(i, 5)));
                at.x += still ? 0 : Mathf.Sin(t * .9f + i * 1.7f) * .12f;
                if (i % 4 == 0) Put(flat, at, new Vector2(.13f, .09f * (still ? 1 : .35f + .65f * Mathf.Abs(Mathf.Cos(t * 1.8f + i)))), still ? 10 : t * 50 + i * 40, new Color(.95f, .9f, .8f, .45f), AirOrder);
                else Put(MechanicInk.Glow, at, Vector2.one * (.08f + .06f * H(i, 6)), 0, new Color(.16f, .09f, .26f, .5f), AirOrder);
            }
        }
    }
}
