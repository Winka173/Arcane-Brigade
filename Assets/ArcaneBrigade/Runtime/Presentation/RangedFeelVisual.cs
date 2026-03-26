using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Our ranged weapons' feel on screen (19/10/2026, owner's V1 and R2-R7), all painted ink (ArsenalInk / MechanicInk
    /// tokens and brush strokes): each weapon's own small flourish at the muzzle, a gun's casing dropping to the floor, a slow
    /// weapon gathering light at its muzzle through its wind-up (and a soft thup as it lets go), arrows and darts staying
    /// stuck in the enemy they hit until it falls, an inked line joining the points a piercing shot passed through, a
    /// chevron where a shot bounces, and a star glint at the screen's edge where a flung body sails out of view (with a
    /// bright ting). Events arrive through Observe (ArsenalVisual.Play forwards each simulation step's events).
    /// Reduced Motion drops casings and keeps the ink still; Reduced Effects thins everything; Reduced Flashes softens light.</summary>
    public sealed class RangedFeelVisual : MonoBehaviour
    {
        private sealed class Puff { public RangedFeel.Flourish Kind; public Vector2 At, Dir; public float Born, Seed; public InkPalette Palette; }
        private sealed class Shell { public Vector2 At, Velocity; public float Born, Floor, Angle, Spin; public InkToken Token; public InkPalette Palette; public float Squash; public bool Landed; }
        private sealed class Stuck { public UnitState Unit; public Sprite Sprite; public Vector2 Offset, Dir; public float Angle, Size; public SpriteRenderer View; }
        private sealed class Link { public Vector2 A, B; public float Born; public int Step; }
        private sealed class Mark { public Vector2 At, Dir; public float Born; }
        private struct Chain { public Vector2 Last; public float At; public int Step; }

        public const float FlourishLife = .3f, CasingLife = .6f, LinkLife = .4f, ChevronLife = .35f, GlintLife = .5f;
        public const int CasingsPerFrame = 3, MaxCasings = 24, MaxStuck = 120, MaxFlourishes = 32, MaxLinks = 24, MaxChevrons = 16, MaxCharges = 16;
        /// <summary>The library cues these moments borrow (no dedicated cue exists yet).</summary>
        public const string TingCue = "sig.bell", ThupCue = "chr.land";

        private ArsenalInk ink;
        private bool drawn;
        private CombatSimulation bound;
        private Camera view;
        private float lastNow = -1, lastThup = -10, lastTing = -10;
        private int casingFrame = -1, casingsThisFrame;
        private readonly List<Puff> puffs = new List<Puff>();
        private readonly List<Shell> shells = new List<Shell>();
        private readonly List<Stuck> stuck = new List<Stuck>();
        private readonly List<SpriteRenderer> stuckPool = new List<SpriteRenderer>();
        private readonly List<Link> links = new List<Link>();
        private readonly List<Mark> chevrons = new List<Mark>();
        private readonly List<Mark> glints = new List<Mark>();
        private readonly Dictionary<int, Chain> chains = new Dictionary<int, Chain>();
        private readonly HashSet<int> homeRuns = new HashSet<int>();

        /// <summary>A camera to judge "left the view" against (checks); otherwise the arena camera.</summary>
        public Camera ViewOverride;
        // what is live (read by checks)
        public int StuckCount => stuck.Count;
        public int Flourishes => puffs.Count;
        public int Casings => shells.Count;
        public int Links => links.Count;
        public int Chevrons => chevrons.Count;
        public int Glints => glints.Count;
        public int Charges { get; private set; }
        public int Thups { get; private set; }
        public int Tings { get; private set; }
        public int StuckOn(int unitId) { int n = 0; foreach (Stuck s in stuck) if (s.Unit.Id == unitId) n++; return n; }

        private static bool Live(CombatSimulation sim) => sim != null && (sim.Phase == RunPhase.Combat || sim.Phase == RunPhase.Paused);
        private static Vector2 Perp(Vector2 d) => new Vector2(-d.y, d.x);
        private static float OutCubic(float x) { x = Mathf.Clamp01(x); return 1 - (1 - x) * (1 - x) * (1 - x); }
        private static float AngleOf(Vector2 d) => Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;

        /// <summary>Everything ends at once: the shop, a new fight, the run's end.</summary>
        public void Clear()
        {
            puffs.Clear(); shells.Clear(); links.Clear(); chevrons.Clear(); glints.Clear(); chains.Clear(); homeRuns.Clear();
            foreach (Stuck s in stuck) if (s.View != null) s.View.enabled = false;
            stuck.Clear();
        }

        // ------------------------------------------------------------------ events
        /// <summary>One simulation event (every event of every step; ArsenalVisual.Play forwards them).</summary>
        public void Observe(CombatEvent value, float now)
        {
            if (bound != null && !Live(bound)) return;
            ImpactWords.Active?.Observe(value);
            switch (value.Kind)
            {
                case CombatEventKind.Fire:
                    if (value.Faction != Faction.Enemy && value.Weapon != null && value.Weapon.Kind == WeaponKind.Projectile) Fired(value, now);
                    break;
                case CombatEventKind.Hit:
                    // a Hit carries the target's faction: an enemy struck by one of our pointy shots keeps it
                    if (value.Faction == Faction.Enemy && value.Value > 0 && value.Subject != null && value.Subject.Alive && !value.Subject.Flung && RangedFeel.IsPointy(value.Weapon)) Stick(value);
                    break;
                case CombatEventKind.ProjectilePierce:
                    if (value.Faction != Faction.Enemy) Pierced(value, now);
                    break;
                case CombatEventKind.ProjectileBounce:
                    if (value.Faction != Faction.Enemy) Bounced(value, now);
                    break;
            }
        }

        private void Fired(CombatEvent value, float now)
        {
            WeaponDefinition w = value.Weapon;
            Vector2 dir = value.Direction.sqrMagnitude > .0001f ? value.Direction.normalized : Vector2.right;
            int shooter = value.Subject != null ? value.Subject.Id : value.Cause.SourceUnitId;
            Vector2 muzzle = value.Position + dir * Mathf.Max(.3f, w.MountDistance + w.MuzzleDistance * .6f);
            if (UnitVisual.TryMuzzle(shooter, out Vector2 tip)) muzzle = tip;
            RangedFeel.Flourish kind = RangedFeel.FlourishFor(w);
            bool hero = value.Faction == Faction.Champion;
            if (kind != RangedFeel.Flourish.None && puffs.Count < EffectDensity.Limit(MaxFlourishes) && (hero || EffectDensity.CosmeticProjectile(value.Cause.RootId + shooter)))
                puffs.Add(new Puff { Kind = kind, At = muzzle, Dir = dir, Born = now, Seed = (muzzle.x * 37 + muzzle.y * 91 + now * 13) % 360, Palette = RangedFeel.ElementPalette(w.Element) });
            if (RangedFeel.IsGunLike(w) && !PresentationSettings.ReducedMotion)
            {
                if (casingFrame != Time.frameCount) { casingFrame = Time.frameCount; casingsThisFrame = 0; }
                if (casingsThisFrame < CasingsPerFrame && shells.Count < EffectDensity.Limit(MaxCasings))
                {
                    casingsThisFrame++;
                    var look = RangedFeel.Casing(w);
                    float side = dir.x < 0 ? 1 : -1, jitter = ((shooter * 37 + shells.Count * 11) % 7) / 6f;
                    Vector2 breech = muzzle - dir * Mathf.Clamp(w.MuzzleDistance * .5f + .08f, .12f, .4f);
                    shells.Add(new Shell
                    {
                        At = breech, Velocity = new Vector2(side * (.7f + .5f * jitter), 1.7f + .6f * jitter), Born = now, Floor = Mathf.Min(breech.y - .12f, value.Position.y - .3f),
                        Angle = jitter * 180, Spin = (side > 0 ? -1 : 1) * (720 + 360 * jitter), Token = look.token, Palette = look.palette, Squash = look.squash
                    });
                }
            }
            if (RangedFeel.IsSlow(w) && !PresentationSettings.ReducedAudio && now - lastThup >= .12f)
            {
                lastThup = now; Thups++;
                CheckpointAudio.PlayWorldId(ThupCue, muzzle, .5f, 1.25f); // a soft thup as the charged shot lets go
            }
        }

        private void Stick(CombatEvent value)
        {
            UnitState unit = value.Subject;
            int cap = PresentationSettings.ReducedEffects ? RangedFeel.ReducedStuckPerEnemy : RangedFeel.StuckPerEnemy;
            int on = 0, oldest = -1;
            for (int i = 0; i < stuck.Count; i++) if (stuck[i].Unit == unit) { on++; if (oldest < 0) oldest = i; }
            if (on >= cap) Release(oldest);                             // the newest shots stay, the oldest fall away
            if (stuck.Count >= EffectDensity.Limit(MaxStuck)) Release(0);
            Sprite sprite = RangedFeel.ShotSprite(value.Weapon);
            if (sprite == null) return;
            Vector2 dir = value.Direction.sqrMagnitude > .0001f ? value.Direction.normalized : Vector2.right;
            float radius = Mathf.Max(.18f, unit.Definition.Radius), height = Mathf.Max(.3f, unit.Definition.BodyHeight);
            int n = on + unit.Id * 3;
            float across = ((n * 37) % 7) / 6f - .5f, rise = ((n * 53 + 3) % 5) / 4f - .5f;
            stuck.Add(new Stuck
            {
                Unit = unit, Sprite = sprite, Dir = dir,
                Offset = -dir * radius * .45f + Perp(dir) * across * radius * 1.1f + Vector2.up * rise * height * .35f,
                Angle = AngleOf(dir) + across * 18, Size = ProjectileVisual.Allied(value.Weapon.ProjectileVisualWidth) * .8f
            });
        }
        private void Release(int index)
        {
            if (index < 0 || index >= stuck.Count) return;
            if (stuck[index].View != null) stuck[index].View.enabled = false;
            stuck.RemoveAt(index);
        }

        private void Pierced(CombatEvent value, float now)
        {
            int root = value.Cause.RootId;
            if (root == 0) return;
            if (chains.TryGetValue(root, out Chain chain) && now - chain.At < 1.2f)
            {
                chain.Step++;
                ImpactWords.Pierced(chain.Step + 1, value.Position);
                if (links.Count >= EffectDensity.Limit(MaxLinks)) links.RemoveAt(0);
                links.Add(new Link { A = chain.Last, B = value.Position, Born = now, Step = chain.Step });
            }
            else chain = new Chain();
            chain.Last = value.Position; chain.At = now;
            if (chains.Count > 256) chains.Clear();
            chains[root] = chain;
        }

        private void Bounced(CombatEvent value, float now)
        {
            Vector2 dir = value.Direction;
            CombatSimulation sim = bound ?? RangedFeel.Simulation;
            if (dir.sqrMagnitude < .0001f && sim != null)
            {
                // the shot's new heading: the live shot of this attack nearest the bounce
                float best = .36f;
                foreach (ProjectileState shot in sim.Projectiles)
                {
                    if (shot.Cause.RootId != value.Cause.RootId || shot.Velocity.sqrMagnitude < .0001f) continue;
                    float d = (shot.Position - value.Position).sqrMagnitude;
                    if (d < best) { best = d; dir = shot.Velocity; }
                }
            }
            if (dir.sqrMagnitude < .0001f) return;
            if (chevrons.Count >= EffectDensity.Limit(MaxChevrons)) chevrons.RemoveAt(0);
            chevrons.Add(new Mark { At = value.Position, Dir = dir.normalized, Born = now });
        }

        // ------------------------------------------------------------------ frame
        public void Display(CombatSimulation simulation) => Display(simulation, Time.time);

        public void Display(CombatSimulation simulation, float now)
        {
            Charges = 0;
            if (simulation != bound) { Clear(); bound = simulation; }
            RangedFeel.Simulation = simulation;
            bool live = Live(simulation);
            if (!live) Clear();
            float delta = lastNow < 0 ? 0 : Mathf.Clamp(now - lastNow, 0, .05f);
            lastNow = now;
            if (live) FindHomeRuns(simulation, now);
            puffs.RemoveAll(p => now - p.Born > FlourishLife || now < p.Born);
            shells.RemoveAll(s => now - s.Born > CasingLife || now < s.Born);
            links.RemoveAll(l => now - l.Born > LinkLife || now < l.Born);
            chevrons.RemoveAll(c => now - c.Born > ChevronLife || now < c.Born);
            glints.RemoveAll(g => now - g.Born > GlintLife || now < g.Born);
            for (int i = stuck.Count - 1; i >= 0; i--) if (stuck[i].Unit == null || !stuck[i].Unit.Alive || stuck[i].Unit.Flung) Release(i); // falls with its enemy
            bool anything = live && (puffs.Count + shells.Count + links.Count + chevrons.Count + glints.Count + stuck.Count > 0 || AnyCharging(simulation));
            if (!anything) { if (drawn) { ink.Begin(); ink.End(); drawn = false; } HideStuckViews(0); return; }
            if (ink == null) ink = new ArsenalInk(transform);
            ink.Begin(); drawn = true;
            bool still = PresentationSettings.ReducedMotion;
            DrawCharges(simulation, still);
            foreach (Puff p in puffs) DrawFlourish(p, Mathf.Clamp01((now - p.Born) / FlourishLife), still);
            foreach (Shell s in shells) DrawShell(s, now, delta);
            foreach (Link l in links) DrawLink(l, Mathf.Clamp01((now - l.Born) / LinkLife), still);
            foreach (Mark c in chevrons) DrawChevron(c, Mathf.Clamp01((now - c.Born) / ChevronLife), still);
            foreach (Mark g in glints) DrawGlint(g, Mathf.Clamp01((now - g.Born) / GlintLife), now, still);
            ink.End();
            DrawStuck();
        }

        private static bool AnyCharging(CombatSimulation sim)
        {
            foreach (UnitState u in sim.Units) if (Charging(u, sim.Elapsed, out _)) return true;
            return false;
        }
        private static bool Charging(UnitState u, float now, out float charge)
        {
            charge = 0;
            if (u == null || !u.Alive || u.Faction == Faction.Enemy || !u.PendingAttack || u.Definition == null) return false;
            WeaponDefinition w = u.Definition.Weapon;
            if (!RangedFeel.IsSlow(w)) return false;
            float windup = u.AttackWindup > 0 ? u.AttackWindup : w.Windup, age = now - u.AttackStarted;
            if (age < 0 || age >= windup) return false;
            charge = Mathf.Clamp01(age / Mathf.Max(.01f, windup));
            return true;
        }

        // ------------------------------------------------------------------ V1 home run
        private void FindHomeRuns(CombatSimulation sim, float now)
        {
            if (sim.Flung.Count == 0) return;
            if (view == null) view = ViewOverride;
            if (view == null) { ArenaCamera arena = FindAnyObjectByType<ArenaCamera>(); view = arena != null ? arena.GetComponent<Camera>() : Camera.main; }
            if (view == null) return;
            bool still = PresentationSettings.ReducedMotion;
            foreach (FlungBody body in sim.Flung)
            {
                if (body.Definition == null || homeRuns.Contains(body.UnitId)) continue;
                // where ImpactVisual draws it: lifted on its arc
                Vector2 drawnAt = body.Position + Vector2.up * (ImpactVisual.BodyLift(body, sim.Elapsed, still) + ImpactVisual.BodySize(body) * .35f);
                Vector3 port = view.WorldToViewportPoint(new Vector3(drawnAt.x, drawnAt.y, 0));
                if (port.x >= 0 && port.x <= 1 && port.y >= 0 && port.y <= 1) continue;
                homeRuns.Add(body.UnitId);
                Vector3 edge = new Vector3(Mathf.Clamp(port.x, .04f, .96f), Mathf.Clamp(port.y, .05f, .95f), port.z);
                Vector2 at = view.ViewportToWorldPoint(edge);
                glints.Add(new Mark { At = at, Dir = body.Velocity.sqrMagnitude > .01f ? body.Velocity.normalized : Vector2.up, Born = now });
                Vector3 inside = new Vector3(Mathf.Clamp(port.x, .14f, .86f), Mathf.Clamp(port.y, .16f, .84f), port.z);
                ImpactWords.Pop("HOME RUN!", view.ViewportToWorldPoint(inside), ImpactWords.Gold, 3, 1.1f, port.x < .5f ? 8 : -8);
                if (now - lastTing >= .15f) { lastTing = now; Tings++; CheckpointAudio.PlayWorldId(TingCue, at, .45f, 1.7f); }
            }
        }

        // ------------------------------------------------------------------ drawing
        private void DrawCharges(CombatSimulation sim, bool still)
        {
            int limit = EffectDensity.Limit(MaxCharges);
            foreach (UnitState u in sim.Units)
            {
                if (Charges >= limit) break;
                if (!Charging(u, sim.Elapsed, out float charge)) continue;
                Charges++;
                WeaponDefinition w = u.Definition.Weapon;
                Vector2 dir = u.AttackDirection.sqrMagnitude > .0001f ? u.AttackDirection.normalized : Vector2.right;
                Vector2 muzzle = UnitVisual.TryMuzzle(u.Id, out Vector2 tip) ? tip : u.Position + dir * Mathf.Max(.3f, w.MountDistance + w.MuzzleDistance);
                InkPalette pal = RangedFeel.ElementPalette(w.Element);
                float grow = Mathf.SmoothStep(0, 1, charge);
                // light gathering at the muzzle, and two motes drawn in toward it (never thrown out from the wielder)
                ink.Light(muzzle, Mathf.Lerp(.15f, .7f, grow), pal.Glow, (.2f + .35f * grow) * EffectDensity.AlliedAlpha);
                if (PresentationSettings.ReducedEffects) continue;
                for (int i = 0; i < 2; i++)
                {
                    float a = (still ? 0 : charge * 140) + i * 180 + u.Id * 23;
                    Vector2 off = new Vector2(Mathf.Cos(a * Mathf.Deg2Rad), Mathf.Sin(a * Mathf.Deg2Rad)) * (.38f * (1 - grow) + .04f);
                    ink.TokenAt(InkToken.Spark, pal, muzzle + off, .1f + .08f * grow, still ? 0 : a * 2, .4f + .6f * grow);
                }
                if (w.Motion == WeaponMotion.Bow) ink.Stroke(muzzle - dir * .05f, muzzle - dir * (.05f + .25f * grow), ArsenalInk.Silver, .02f, 1, 0, .7f * grow); // the pull, inked behind the arrow's head
            }
        }

        private void DrawFlourish(Puff f, float p, bool still)
        {
            Vector2 m = f.At, d = f.Dir, n = Perp(d), up = Vector2.up;
            float fade = 1 - p * p, go = still ? .6f : OutCubic(p);
            switch (f.Kind)
            {
                case RangedFeel.Flourish.Twang:
                {
                    // the string snapping forward and shivering, a thin release stroke and a glint at the nock
                    float shiver = still ? 0 : Mathf.Sin(p * 38) * .07f * (1 - p);
                    Vector2 a = m - d * .3f + n * .22f, b = m - d * .3f - n * .22f, c = m - d * (.3f - .1f * go) + n * shiver;
                    ink.Stroke(a, c, ArsenalInk.Silver, .022f, 1, 0, .9f * fade);
                    ink.Stroke(c, b, ArsenalInk.Silver, .022f, 1, 0, .9f * fade);
                    ink.Stroke(m, m + d * (.12f + .38f * go), ArsenalInk.Cream, .03f, 1, still ? 0 : p * .8f, .85f * fade);
                    ink.TokenAt(InkToken.Glint, ArsenalInk.Cream, m, .2f * (1 - .5f * p), 0, fade);
                    break;
                }
                case RangedFeel.Flourish.Pop:
                    ink.Muzzle(ArsenalInk.Cotton, InkToken.Patch, m, d, p, f.Seed);
                    for (int s = -1; s <= 1; s += 2)
                    {
                        Vector2 ray = (d + n * s * .75f).normalized;
                        ink.Stroke(m + ray * (.12f + .1f * go), m + ray * (.24f + .14f * go), ArsenalInk.Leather, .028f, 1, 0, fade);
                    }
                    break;
                case RangedFeel.Flourish.Steam:
                    for (int i = 0; i < 3; i++)
                    {
                        float q = Mathf.Clamp01(p * 1.2f - i * .12f), sway = still ? 0 : Mathf.Sin(q * 6 + i * 2) * .06f;
                        ink.TokenAt(InkToken.Puff, ArsenalInk.Cotton, m + d * (.06f + .08f * i) + up * (.08f + .42f * q) + n * sway, .13f + .1f * q, still ? 0 : q * 40 + i * 70, (1 - q * q) * .9f);
                    }
                    ink.Stroke(m + up * .05f, m + up * (.12f + .3f * go) + d * .06f, ArsenalInk.Cotton, .03f, 1, still ? 0 : p, .6f * fade, still ? 0 : .05f, f.Seed);
                    break;
                case RangedFeel.Flourish.Spark:
                    ink.Light(m, .45f * (1 - p), f.Palette.Glow, .3f * fade);
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 flick = (d + n * ((i - 1) * .45f)).normalized;
                        ink.TokenAt(InkToken.Spark, f.Palette, m + flick * (.08f + .32f * go) + (still ? Vector2.zero : Vector2.down * .1f * p * p), .14f * (1 - .4f * p), still ? 0 : f.Seed + p * 300 * (i - 1), fade);
                    }
                    break;
                case RangedFeel.Flourish.Glint:
                    ink.Stroke(m - d * .25f + n * .1f, m + d * (.1f + .15f * go), ArsenalInk.Silver, .025f, 1, still ? 0 : p, .8f * fade, still ? 0 : .03f, f.Seed);
                    ink.TokenAt(InkToken.Glint, ArsenalInk.Cream, m + d * .05f, .28f * (1 - .6f * p), 0, fade);
                    break;
                case RangedFeel.Flourish.Bubbles:
                    for (int i = 0; i < 3; i++)
                    {
                        float q = Mathf.Clamp01(p * 1.15f - i * .1f);
                        ink.TokenAt(InkToken.Bubble, ArsenalInk.Water, m + d * (.1f + .28f * q + .05f * i) + up * (.12f * q + .05f * (i - 1)) + n * ((i - 1) * .08f), .09f + .04f * i, 0, 1 - q * q);
                    }
                    break;
                case RangedFeel.Flourish.Notes:
                    for (int i = 0; i < 2; i++)
                    {
                        float q = Mathf.Clamp01(p * 1.1f - i * .15f), bob = still ? 0 : Mathf.Sin(q * 9 + i * 3) * .05f;
                        ink.TokenAt(InkToken.Note, i == 0 ? ArsenalInk.Gold : ArsenalInk.Violet, m + d * (.12f + .2f * q) + up * (.1f + .3f * q + bob), .17f, still ? 0 : bob * 120, 1 - q * q);
                    }
                    break;
                case RangedFeel.Flourish.Paper:
                    for (int i = 0; i < 2; i++)
                    {
                        float q = Mathf.Clamp01(p - i * .1f);
                        ink.TokenAt(InkToken.Page, ArsenalInk.Cream, m + d * (.1f + .25f * q) + n * ((i * 2 - 1) * (.08f + .1f * q)) + (still ? Vector2.zero : Vector2.down * .12f * q * q), .15f, still ? 0 : f.Seed + q * 260 * (i * 2 - 1), 1 - q * q);
                    }
                    break;
                case RangedFeel.Flourish.Smoke:
                    ink.Muzzle(ArsenalInk.Dust, InkToken.Spark, m, d, p, f.Seed);
                    ink.Stroke(m, m + d * (.1f + .3f * go), ArsenalInk.Gold, .03f, 1, still ? 0 : p, .8f * fade);
                    break;
                case RangedFeel.Flourish.Snow:
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 flick = (d + n * ((i - 1) * .5f)).normalized;
                        ink.TokenAt(i == 1 ? InkToken.Shard : InkToken.Sand, ArsenalInk.Glass, m + flick * (.08f + .3f * go), .12f, still ? 0 : f.Seed + p * 200 * (i - 1), fade);
                    }
                    break;
                case RangedFeel.Flourish.Petal:
                    for (int i = 0; i < 2; i++)
                    {
                        float q = Mathf.Clamp01(p - i * .12f);
                        ink.TokenAt(i == 0 ? InkToken.Petal : InkToken.Leaf, i == 0 ? ArsenalInk.Candy : ArsenalInk.Green, m + d * (.1f + .22f * q) + n * ((i * 2 - 1) * .08f) + (still ? Vector2.zero : Vector2.down * .1f * q * q), .15f, still ? 0 : f.Seed + q * 220 * (i * 2 - 1), 1 - q * q);
                    }
                    break;
                case RangedFeel.Flourish.Swish:
                {
                    // the throwing hand's swish: a curved stroke over the shoulder to the release point
                    Vector2 from = m - d * .35f + up * .25f, mid = m - d * .08f + up * .12f + n * .05f, to = m + d * .12f;
                    float head = still ? 1 : OutCubic(p / .5f), tail = still ? 0 : Mathf.Clamp01((p - .25f) / .75f);
                    ink.Stroke(from, mid, ArsenalInk.Cream, .03f, head, tail, .75f * fade);
                    ink.Stroke(mid, to, ArsenalInk.Cream, .026f, head, tail, .75f * fade);
                    break;
                }
            }
        }

        private void DrawShell(Shell s, float now, float delta)
        {
            if (!s.Landed && delta > 0)
            {
                s.Velocity += Vector2.down * 9f * delta;
                s.At += s.Velocity * delta;
                s.Angle += s.Spin * delta;
                if (s.At.y <= s.Floor && s.Velocity.y < 0)
                {
                    s.At.y = s.Floor;
                    if (s.Velocity.y < -1.2f) { s.Velocity = new Vector2(s.Velocity.x * .45f, -s.Velocity.y * .3f); s.Spin *= .4f; } // one small bounce
                    else { s.Landed = true; s.Velocity = Vector2.zero; s.Angle = Mathf.Round(s.Angle / 180) * 180 + 90; }
                }
            }
            float t = Mathf.Clamp01((now - s.Born) / CasingLife), alpha = 1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01((t - .55f) / .45f));
            ink.Place(MechanicInk.Token(s.Token, s.Palette), s.At, .11f, s.Angle, new Color(1, 1, 1, alpha), !s.Landed, s.Squash);
        }

        private void DrawLink(Link l, float p, bool still)
        {
            // the inked thread a piercing shot left between the bodies it went through; each step a touch brighter
            float head = still ? 1 : OutCubic(p / .35f), tail = still ? 0 : Mathf.Clamp01((p - .3f) / .7f);
            InkPalette pal = l.Step >= 2 ? ArsenalInk.Gold : ArsenalInk.Silver;
            ink.Stroke(l.A, l.B, pal, .045f + .01f * Mathf.Min(3, l.Step), head, tail, .85f * (1 - p * p), still ? 0 : .02f, l.A.x * 7);
            ink.TokenAt(InkToken.Glint, ArsenalInk.Cream, l.B, .2f * (1 - p), 0, 1 - p);
        }

        private void DrawChevron(Mark c, float p, bool still)
        {
            // a ">" pointing the new way, with a short stroke from the bounce point toward it
            Vector2 d = c.Dir, n = Perp(d);
            float reach = still ? .3f : .2f + .15f * OutCubic(p), fade = 1 - p * p;
            Vector2 apex = c.At + d * (reach + .1f);
            ink.Stroke(c.At, c.At + d * reach, ArsenalInk.Cream, .03f, 1, still ? 0 : p * .6f, .8f * fade);
            ink.Stroke(apex - d * .16f + n * .12f, apex, ArsenalInk.Gold, .035f, 1, 0, fade);
            ink.Stroke(apex - d * .16f - n * .12f, apex, ArsenalInk.Gold, .035f, 1, 0, fade);
        }

        private void DrawGlint(Mark g, float p, float now, bool still)
        {
            // a star twinkling at the edge of the screen where the body sailed out
            float pop = still ? 1 : Mathf.Sin(Mathf.Clamp01(p / .35f) * Mathf.PI * .5f) * (1 - .3f * p), fade = 1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01((p - .5f) / .5f));
            ink.Light(g.At, 1.1f * pop, ArsenalInk.Gold.Glow, .45f * fade);
            ink.TokenAt(InkToken.Star, ArsenalInk.Gold, g.At, .42f * pop, still ? 0 : now * 260, fade);
            ink.TokenAt(InkToken.Glint, ArsenalInk.Cream, g.At + new Vector2(.06f, .06f), .6f * pop, 0, fade);
        }

        private void DrawStuck()
        {
            int used = 0;
            foreach (Stuck s in stuck)
            {
                if (s.Sprite == null || s.Unit == null) continue;
                Vector2 at = UnitVisual.TryDrawn(s.Unit.Id, out Vector2 drawnAt) ? drawnAt : s.Unit.Position;
                if (used == stuckPool.Count)
                {
                    var go = new GameObject("StuckShot"); go.transform.SetParent(transform, false);
                    stuckPool.Add(go.AddComponent<SpriteRenderer>());
                }
                SpriteRenderer shown = stuckPool[used++]; s.View = shown;
                shown.enabled = true; shown.sprite = s.Sprite; shown.color = Color.white;
                shown.sortingOrder = 1004 - Mathf.RoundToInt(s.Unit.Position.y * 12f); // just in front of its enemy's body
                float inkWidth = Mathf.Max(.01f, SpriteInkMetrics.BoundsOf(s.Sprite).width);
                shown.transform.position = at + s.Offset - s.Dir * s.Size * .3f; // the head buried, the shaft standing out
                shown.transform.rotation = Quaternion.Euler(0, 0, s.Angle);
                shown.transform.localScale = Vector3.one * (s.Size / inkWidth);
            }
            HideStuckViews(used);
        }
        private void HideStuckViews(int from) { for (int i = from; i < stuckPool.Count; i++) if (stuckPool[i] != null) stuckPool[i].enabled = false; }

        private void OnDestroy()
        {
            if (RangedFeel.Simulation == bound) RangedFeel.Simulation = null;
            if (!Application.isPlaying) return; // edit-time fixtures: the children go with their host
            ink?.Destroy();
            foreach (SpriteRenderer r in stuckPool) if (r != null) Destroy(r.gameObject);
        }
    }
}
