using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Draws what the Bizarre Arsenal leaves in the room (Docs/Bizarre_Arsenal.md): lobbed bombs and sticks on an arc with
    /// a floor shadow, fizzing fuses that blink faster, bamboo springing up and sinking, petals drifting, a clinging octopus over
    /// its ink, planted signs with road stripes, toy bombs joined by wire, spinning pinwheels, a drill boring a spiral, the claw
    /// and plunger cords; and each weapon's own moment as it attacks and bursts. Objects (bombs, balls, signs, held weapons)
    /// are painted art from ArsenalArtwork; every effect around them is painted ink (ArsenalInk), like the hero mechanics.</summary>
    public sealed class ArsenalVisual : MonoBehaviour
    {
        private readonly List<SpriteRenderer> pool = new List<SpriteRenderer>();
        private int used;
        private sealed class Flash { public ArsenalInk.Moment Moment; public Vector2 At, Dir; public float Born, Life, Size, Seed; public Sprite Held; }
        private ArsenalInk ink;
        private readonly List<Flash> flashes = new List<Flash>();
        private static readonly Color Shadow = new Color(0.05f, 0.03f, 0.08f, 0.35f);
        private static Sprite soft;

        private SpriteRenderer Next(int order)
        {
            if (used == pool.Count)
            {
                var go = new GameObject("ArsenalPiece"); go.transform.SetParent(transform, false);
                pool.Add(go.AddComponent<SpriteRenderer>());
            }
            SpriteRenderer view = pool[used++]; view.enabled = true; view.sortingOrder = order; view.color = Color.white; view.flipX = false;
            return view;
        }
        private void Draw(Sprite sprite, Vector2 at, float width, float angle, int order, Color? tint = null, float squash = 1)
        {
            if (sprite == null) return;
            SpriteRenderer view = Next(order); view.sprite = sprite; view.color = tint ?? Color.white;
            float ink = Mathf.Max(0.01f, SpriteInkMetrics.BoundsOf(sprite).width);
            view.transform.position = at; view.transform.rotation = Quaternion.Euler(0, 0, angle);
            view.transform.localScale = new Vector3(width / ink, width / ink * squash, 1);
        }
        private void ShadowAt(Vector2 at, float width)
        {
            soft ??= MakeSoft();
            Draw(soft, at, width, 0, 60, Shadow, 0.4f);
        }
        private static Sprite MakeSoft()
        {
            var t = new Texture2D(64, 64, TextureFormat.RGBA32, false) { hideFlags = HideFlags.DontSave, wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[64 * 64];
            for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++) { float d = Mathf.Sqrt((x - 31.5f) * (x - 31.5f) + (y - 31.5f) * (y - 31.5f)) / 31.5f; px[y * 64 + x] = new Color32(255, 255, 255, (byte)(Mathf.Clamp01(1 - d) * Mathf.Clamp01(1 - d) * 255)); }
            t.SetPixels32(px); t.Apply(false, true);
            return Sprite.Create(t, new Rect(0, 0, 64, 64), new Vector2(.5f, .5f), 64);
        }
        private static int Order(Vector2 at) => 1000 - Mathf.RoundToInt(at.y * 12);
        private static Vector2 Arc(Vector2 from, Vector2 to, float t, float height) => Vector2.Lerp(from, to, t) + Vector2.up * height * Mathf.Sin(Mathf.PI * Mathf.Clamp01(t));

        /// <summary>Each Arsenal weapon's own painted-ink moment: as it attacks (a swipe, a muzzle puff), when it bursts (a cotton pop,
        /// a gavel's verdict) and when it catches or shocks.</summary>
        public void Play(CombatEvent value, float time)
        {
            if (value.Weapon == null || value.Weapon.Arsenal == ArsenalWeapon.None || flashes.Count > 64) return;
            ArsenalWeapon w = value.Weapon.Arsenal;
            Vector2 dir = value.Direction.sqrMagnitude > 0.0001f ? value.Direction.normalized : Vector2.right;
            if (value.Kind == CombatEventKind.Fire)
            {
                (ArsenalInk.Moment m, float ahead, float size, float life) cue = w switch
                {
                    ArsenalWeapon.CorkRifle => (ArsenalInk.Moment.CorkPop, 0.6f, 0.5f, 0.32f),
                    ArsenalWeapon.ConfettiBlunderbuss => (ArsenalInk.Moment.MuzzleCloud, 0.55f, 0.8f, 0.4f),
                    ArsenalWeapon.FanfareHorn => (ArsenalInk.Moment.MuzzleCloud, 0.6f, 0.5f, 0.32f),
                    ArsenalWeapon.SquirtTrio => (ArsenalInk.Moment.Droplets, 0.55f, 0.7f, 0.4f),
                    ArsenalWeapon.LeekBaton => (ArsenalInk.Moment.LeekSwipe, 0, 1.6f, 0.34f),
                    ArsenalWeapon.PawMitten => (ArsenalInk.Moment.PawSwipe, 0, 0.95f, 0.26f),
                    ArsenalWeapon.StarforgeClaymore => (ArsenalInk.Moment.GoldLeaf, 0, 2.3f, 0.45f),
                    ArsenalWeapon.DragonLanternGlaive => (ArsenalInk.Moment.Dragon, 0, 2f, 0.36f),
                    ArsenalWeapon.WindupWrench or ArsenalWeapon.SignpostSlammer or ArsenalWeapon.HomeRunBat => (ArsenalInk.Moment.MetalSwipe, 0, 1.55f, 0.3f),
                    ArsenalWeapon.DrumstickClub => (ArsenalInk.Moment.Crumbs, 0, 1.4f, 0.45f),
                    ArsenalWeapon.FlySwatter => (ArsenalInk.Moment.SwatPrint, 1.1f, 0.9f, 0.5f),
                    ArsenalWeapon.BareKnuckles or ArsenalWeapon.StarKnuckle => (ArsenalInk.Moment.FistJab, 0, 0.75f, 0.22f),
                    ArsenalWeapon.GlitchToybox => (ArsenalInk.Moment.Glitch, 0.5f, 0.5f, 0.3f),
                    _ => ((ArsenalInk.Moment)(-1), 0, 0, 0)
                };
                if ((int)cue.m >= 0) Add(cue.m, value.Position + dir * cue.ahead, dir, time, cue.life, cue.size);
                return;
            }
            if (value.Kind == CombatEventKind.Proc)
            {
                if (w == ArsenalWeapon.CatcherMitt) Add(ArsenalInk.Moment.MittCatch, value.Position, dir, time, 0.3f, 0.5f);
                else if (w == ArsenalWeapon.BuzzerBlock) Add(ArsenalInk.Moment.Zap, value.Position, dir, time, 0.4f, 0.9f);
                else if (w == ArsenalWeapon.StarforgeClaymore) Add(ArsenalInk.Moment.Spark, value.Position, dir, time, 0.25f, 0.4f);
                return;
            }
            if (value.Kind != CombatEventKind.Detonation) return;
            ArsenalInk.Moment burst = w switch
            {
                ArsenalWeapon.CherryFuseBomb => ArsenalInk.Moment.CottonPop, ArsenalWeapon.PopperSticks => ArsenalInk.Moment.PopperBurst,
                ArsenalWeapon.DetonatorBox => ArsenalInk.Moment.CandyFloss, ArsenalWeapon.FirecrackerPinwheel => ArsenalInk.Moment.PaperScraps,
                ArsenalWeapon.BalloonBuddy => ArsenalInk.Moment.BalloonPop, ArsenalWeapon.ConfettiBlunderbuss => ArsenalInk.Moment.Confetti,
                ArsenalWeapon.HearthBrick => ArsenalInk.Moment.BrickDust, ArsenalWeapon.VerdictGavel => ArsenalInk.Moment.GavelStamp,
                ArsenalWeapon.TalismanIngot => ArsenalInk.Moment.IngotBlast, ArsenalWeapon.BareKnuckles => ArsenalInk.Moment.KnucklePop,
                _ => ArsenalInk.Moment.Burst
            };
            Flash f = Add(burst, value.Position, dir, time, burst == ArsenalInk.Moment.GavelStamp ? 0.7f : 0.55f, Mathf.Max(0.6f, value.Value));
            if (burst == ArsenalInk.Moment.GavelStamp) f.Held = Held(value.Weapon.Id);
        }
        private Flash Add(ArsenalInk.Moment m, Vector2 at, Vector2 dir, float time, float life, float size)
        {
            var f = new Flash { Moment = m, At = at, Dir = dir, Born = time, Life = life, Size = size, Seed = (at.x * 37 + at.y * 91 + time * 13) % 360 };
            flashes.Add(f); return f;
        }
        /// <summary>A thrown copy of the weapon itself uses its own held painting.</summary>
        private static Sprite Held(string id) => ArsenalArtwork.Frame(id + "_Held", 0);

        public void Display(CombatSimulation sim, float time)
        {
            used = 0;
            ink ??= new ArsenalInk(transform);
            ink.Begin();
            bool still = PresentationSettings.ReducedMotion;
            if (sim != null)
            {
                foreach (ArsenalProp p in sim.ArsenalProps) DrawProp(sim, p, still);
                foreach (ProjectileState shot in sim.Projectiles)
                {
                    // shots that leave a wake: the saucer's rings in the air, the shark's ripple on the floor
                    if (shot.Arsenal == ArsenalWeapon.SaucerBlaster) ink.Wake(shot.Position, shot.Velocity, ArsenalInk.Cyan, sim.Elapsed, shot.Id, false);
                    else if (shot.Arsenal == ArsenalWeapon.PlushShark) ink.Wake(shot.Position + Vector2.down * 0.15f, shot.Velocity, ArsenalInk.Water, sim.Elapsed, shot.Id, true);
                }
            }
            for (int i = flashes.Count - 1; i >= 0; i--)
            {
                Flash f = flashes[i]; float t = (time - f.Born) / f.Life;
                if (t >= 1 || t < 0) { flashes.RemoveAt(i); continue; }
                ink.Draw(f.Moment, f.At, f.Dir, t, f.Size, f.Seed, f.Held);
            }
            ink.End();
            for (int i = used; i < pool.Count; i++) pool[i].enabled = false;
        }

        private void DrawProp(CombatSimulation sim, ArsenalProp p, bool still)
        {
            float now = sim.Elapsed, age = now - p.Born;
            Vector2 at = p.Position;
            switch (p.Kind)
            {
                case ArsenalPropKind.Bomb:
                case ArsenalPropKind.Bomblet:
                case ArsenalPropKind.Stick:
                case ArsenalPropKind.Vase:
                {
                    float size = p.Kind == ArsenalPropKind.Bomblet ? 0.26f : p.Kind == ArsenalPropKind.Stick ? 0.38f : 0.42f;
                    string art = p.Kind == ArsenalPropKind.Stick ? "PopperStick" : p.Kind == ArsenalPropKind.Vase ? "BambooVaseShot" : "CherryBomb";
                    if (p.Kind == ArsenalPropKind.Vase && ArsenalArtwork.Frame(art, 0) == null) art = "BambooVase_Held";
                    if (now < p.LandAt)
                    {
                        float t = Mathf.InverseLerp(p.Born, p.LandAt, now);
                        Vector2 air = Arc(p.Start, p.Position, t, 0.9f);
                        ShadowAt(Vector2.Lerp(p.Start, p.Position, t) + Vector2.down * 0.1f, size * (0.6f + 0.4f * t));
                        Draw(ArsenalArtwork.Frame(art, 0), air, size, still ? 0 : t * 540, Order(air));
                        break;
                    }
                    // landed: one small bounce, then the fuse fizzes and the rim blinks faster as it shortens
                    float since = now - p.LandAt, fuse = float.IsInfinity(p.FuseAt) ? 0 : Mathf.InverseLerp(p.LandAt, p.FuseAt, now);
                    float bounce = still ? 0 : Mathf.Max(0, Mathf.Sin(since * 18) * 0.08f * Mathf.Clamp01(1 - since * 4));
                    float blink = still ? 1 : 1 + 0.12f * Mathf.Max(0, Mathf.Sin(now * (8 + 30 * fuse)));
                    ShadowAt(at + Vector2.down * 0.1f, size);
                    Draw(ArsenalArtwork.Frame(art, fuse) ?? ArsenalArtwork.Frame(art, 0), at + Vector2.up * bounce, size * blink, 0, Order(at));
                    break;
                }
                case ArsenalPropKind.Shoot:
                {
                    if (now < p.At) break;
                    float t = (now - p.At) / Mathf.Max(0.05f, p.Until - p.At);
                    float rise = Mathf.Clamp01(t * 4), sink = Mathf.Clamp01((t - 0.75f) * 4);
                    Draw(ArsenalArtwork.Frame("BambooShoot", rise), at, 0.5f, 0, Order(at), new Color(1, 1, 1, 1 - sink), still ? 1 : 0.2f + 0.8f * rise - 0.5f * sink);
                    if (t < 0.3f) ink.Shards(at, t / 0.3f, p.At * 97);
                    break;
                }
                case ArsenalPropKind.Petals:
                {
                    float t = Mathf.InverseLerp(p.Born, p.Until, now);
                    ink.Token(InkToken.Petal, (p.Born.GetHashCode() & 1) == 0 ? ArsenalInk.Wine : ArsenalInk.Candy, at + (still ? Vector2.zero : new Vector2(Mathf.Sin(now * 2 + p.Born * 7) * 0.08f, -t * 0.12f)), 0.22f, now * 60 + p.Born * 100, Mathf.Clamp01((1 - t) * 3), false);
                    break;
                }
                case ArsenalPropKind.Octopus:
                {
                    Vector2 home = p.Source != null ? p.Source.Position : at;
                    if (p.Stage == 0)
                    {
                        float t = Mathf.InverseLerp(p.Born, p.LandAt, now);
                        Draw(ArsenalArtwork.Frame("PlushOctopus", 0), Arc(p.Start, at, t, 0.7f), 0.5f, still ? 0 : t * 360, Order(at) + 2);
                    }
                    else if (p.Stage == 1)
                    {
                        ink.Pool(at + Vector2.down * 0.15f, 0.7f, Mathf.Clamp01((p.FuseAt - now) * 3), ArsenalInk.Violet, p.Born * 50, 0.9f, 0.45f);
                        Sprite cling = ArsenalArtwork.Frames("PlushOctopus") is Sprite[] f && f.Length >= 3 ? f[still ? 1 : 1 + (Mathf.FloorToInt(now * 5) & 1)] : null;
                        Draw(cling, at + Vector2.up * 0.25f, 0.55f, 0, Order(at) + 3);
                    }
                    else
                    {
                        float t = Mathf.InverseLerp(p.FuseAt, p.Until, now);
                        Sprite[] f = ArsenalArtwork.Frames("PlushOctopus");
                        Draw(f != null ? f[f.Length - 1] : null, Arc(p.Start, home, t, 0.6f), 0.45f, 0, Order(at) + 2);
                    }
                    break;
                }
                case ArsenalPropKind.Sign:
                {
                    Vector2 along = p.Direction.normalized;
                    ink.Stripes(at - along * 0.8f, at + along * 0.8f, 0.25f, now, Mathf.Clamp01((p.Until - now) * 2));
                    // the wielder's own road sign, stood upright in the floor (its painting points right: turned up, the face is on top)
                    float wobble = still ? 0 : Mathf.Sin(age * 30) * 8 * Mathf.Clamp01(1 - age * 3);
                    ShadowAt(at + Vector2.down * 0.05f, 0.3f);
                    Draw(Held(p.Weapon != null ? p.Weapon.Id : "SignpostSlammer"), at + Vector2.up * 0.42f, 0.84f, 90 + wobble, Order(at));
                    break;
                }
                case ArsenalPropKind.ToyBomb:
                {
                    if (p.Source != null) ink.Cord(p.Source.Position + Vector2.down * 0.2f, at, ArsenalInk.Midnight, 0.04f, true, 0.85f);
                    float shake = p.Stage == 1 && !still ? Mathf.Sin(now * 60) * 0.03f : 0;
                    ShadowAt(at + Vector2.down * 0.1f, 0.4f);
                    Draw(ArsenalArtwork.Frame("ToyBomb", 0), at + Vector2.right * shake, 0.4f, 0, Order(at));
                    break;
                }
                case ArsenalPropKind.Pinwheel:
                {
                    if (now < p.LandAt) { Draw(ArsenalArtwork.Frame("Pinwheel", 0), Arc(p.Start, at, Mathf.InverseLerp(p.Born, p.LandAt, now), 0.5f), 0.5f, 0, Order(at)); break; }
                    string spin = "Pinwheel";
                    Sprite[] f = ArsenalArtwork.Frames(spin);
                    Sprite frame = f == null ? null : still ? f[0] : p.Stage == 0 ? f[Mathf.FloorToInt(now * 3) % Mathf.Min(2, f.Length)] : f[Mathf.Min(f.Length - 1, 2 + (Mathf.FloorToInt(now * 20) & 1))];
                    Draw(frame, at + Vector2.up * 0.3f, 0.55f, 0, Order(at));
                    break;
                }
                case ArsenalPropKind.Drill:
                {
                    float t = Mathf.InverseLerp(p.Born, p.Until, now);
                    ink.Spiral(at, 0.45f, t, now, p.Born * 40);
                    Draw(ArsenalArtwork.Frame("CandyDrillShot", 0) ?? Held("CandyDrill"), at + Vector2.up * (0.25f - 0.2f * t), 0.5f, -90 + (still ? 0 : Mathf.Sin(now * 50) * 6), Order(at));
                    break;
                }
                case ArsenalPropKind.Bump:
                {
                    if (now < p.At) break;
                    float t = (now - p.At) / Mathf.Max(0.05f, p.Until - p.At);
                    ink.Draw(ArsenalInk.Moment.Steam, at, Vector2.right, t, 0.7f, p.At * 31);
                    break;
                }
                case ArsenalPropKind.Batted:
                    if (age < 0.45f) ink.Draw(ArsenalInk.Moment.HomeRun, p.Start + Vector2.up * 0.3f, p.Direction, age / 0.45f, 0.9f, p.Born * 13);
                    // the hand-lettered HOME RUN! rides over the ink burst when its painting exists
                    if (age < 0.45f) Draw(ArsenalArtwork.Frame("HomeRunText", 0), p.Start + Vector2.up * (0.75f + age * 0.5f), 1.1f, 0, 2200, new Color(1, 1, 1, Mathf.Clamp01((0.45f - age) * 5)), still ? 1 : 0.85f + 0.15f * Mathf.Clamp01(age * 8));
                    ink.Streaks(at, p.Direction, Mathf.Clamp01((p.Until - now) * 3));
                    break;
                case ArsenalPropKind.Grab:
                    if (p.Source != null) ink.Coil(p.Source.Position, at);
                    Draw(ArsenalArtwork.Frame("ClawHead", 1), at, 0.4f, 0, Order(at) + 2);
                    break;
                case ArsenalPropKind.Wound:
                {
                    // the wind-up key turns in the enemy's back, then it spins dizzy under a ring of stars
                    Sprite key = ArsenalArtwork.Frame("WindupKey", now, 6);
                    if (p.Stage == 0) Draw(key, at + new Vector2(-0.2f, 0.35f), 0.26f, still ? 0 : now * 220, Order(at) + 4);
                    else ink.Orbit(at + Vector2.up * 0.6f, 0.26f, now, InkToken.Star, ArsenalInk.Gold, 3);
                    break;
                }
                case ArsenalPropKind.Fizz:
                {
                    if (now < p.LandAt)
                    {
                        float t = Mathf.InverseLerp(p.Born, p.LandAt, now);
                        Draw(ArsenalArtwork.Frame("SodaBottleShot", 0) ?? Held("FizzBottle"), Arc(p.Start, at, t, 0.8f), 0.36f, still ? 0 : t * 480, Order(at));
                        break;
                    }
                    float life = Mathf.InverseLerp(p.LandAt, p.Until, now);
                    float spread = still ? 1 : 0.8f + 0.2f * Mathf.Clamp01(life * 6);
                    ink.Pool(at, 0.8f * spread, Mathf.Clamp01((1 - life) * 3), ArsenalInk.Green, p.Born * 50, 0.85f);
                    ink.Bubbles(at, 0.8f, now, ArsenalInk.Green, 5, p.Born, Mathf.Clamp01((1 - life) * 3));
                    break;
                }
                case ArsenalPropKind.Catch:
                    if (p.Source != null) Draw(ArsenalArtwork.Frame("Fastball", 0), p.Source.Position + new Vector2(0.28f, 0.1f + 0.08f * (p.Born.GetHashCode() & 3)), 0.14f, 0, Order(p.Source.Position) + 5);
                    break;
                case ArsenalPropKind.Brick:
                {
                    // the wielder's own brick tumbling on a low arc over its floor shadow
                    float t = Mathf.InverseLerp(p.Born, p.LandAt, now);
                    Vector2 air = Arc(p.Start, p.Position, t, 0.7f);
                    ShadowAt(Vector2.Lerp(p.Start, p.Position, t) + Vector2.down * 0.1f, 0.36f * (0.6f + 0.4f * t));
                    Draw(Held("HearthBrick"), air, 0.42f, still ? 0 : t * 400, Order(air));
                    break;
                }
                case ArsenalPropKind.Buzzer:
                {
                    if (now < p.LandAt) { Draw(ArsenalArtwork.Frame("BuzzerFloor", 0), Arc(p.Start, at, Mathf.InverseLerp(p.Born, p.LandAt, now), 0.45f), 0.4f, 0, Order(at)); break; }
                    ShadowAt(at + Vector2.down * 0.08f, 0.36f);
                    Draw(ArsenalArtwork.Frame("BuzzerFloor", 0), at, 0.4f, 0, Order(at));
                    break;
                }
                case ArsenalPropKind.Plunge:
                    if (p.Source != null) ink.Cord(p.Source.Position, at, ArsenalInk.Wine, 0.06f, false);
                    Draw(ArsenalArtwork.Frame("PlungerCup", 0), at, 0.32f, 0, Order(at) + 2);
                    break;
            }
        }
        private void OnDestroy() { foreach (SpriteRenderer view in pool) if (view != null) Destroy(view.gameObject); ink?.Destroy(); }
    }
}
