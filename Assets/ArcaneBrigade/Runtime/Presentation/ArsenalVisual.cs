using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Draws what the Bizarre Arsenal leaves in the room (Docs/Bizarre_Arsenal.md): lobbed bombs and sticks on an arc with
    /// a floor shadow, fizzing fuses that blink faster, bamboo springing up and sinking, petals drifting, a clinging octopus over
    /// its ink, planted signs with road stripes, toy bombs joined by wire, spinning pinwheels, a drill boring a spiral, the claw
    /// and plunger cords; and each weapon's own burst on its detonations. Every piece is painted art from ArsenalArtwork; a
    /// piece whose art is missing is simply not drawn (the weapon itself only exists once its art does).</summary>
    public sealed class ArsenalVisual : MonoBehaviour
    {
        private readonly List<SpriteRenderer> pool = new List<SpriteRenderer>();
        private int used;
        private sealed class Flash { public string Art; public Vector2 At; public float Born, Life, Size; }
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
        /// <summary>A tile stretched from a to b (wire, cord, spring, road stripes).</summary>
        private void Span(Sprite sprite, Vector2 a, Vector2 b, float thickness, int order, Color? tint = null)
        {
            if (sprite == null || (b - a).sqrMagnitude < 0.0004f) return;
            Rect ink = SpriteInkMetrics.BoundsOf(sprite);
            SpriteRenderer view = Next(order); view.sprite = sprite; view.color = tint ?? Color.white;
            view.transform.position = (a + b) * 0.5f; view.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(b.y - a.y, b.x - a.x) * Mathf.Rad2Deg);
            view.transform.localScale = new Vector3((b - a).magnitude / Mathf.Max(0.01f, ink.width), thickness / Mathf.Max(0.01f, ink.height), 1);
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

        /// <summary>A detonation from an Arsenal weapon gets that weapon's own painted burst.</summary>
        public void Play(CombatEvent value, float time)
        {
            if (value.Kind != CombatEventKind.Detonation || value.Weapon == null) return;
            string art = value.Weapon.Arsenal switch
            {
                ArsenalWeapon.CherryFuseBomb => "CottonPop", ArsenalWeapon.PopperSticks => "PopperBurst", ArsenalWeapon.DetonatorBox => "CandyFlossCloud",
                ArsenalWeapon.FirecrackerPinwheel => "PaperScraps", ArsenalWeapon.BalloonBuddy => "BalloonPop", ArsenalWeapon.ConfettiBlunderbuss => "ConfettiBurst", _ => null
            };
            if (art == null || ArsenalArtwork.Frames(art) == null || flashes.Count > 48) return;
            flashes.Add(new Flash { Art = art, At = value.Position, Born = time, Life = 0.5f, Size = Mathf.Max(0.8f, value.Value * 2.1f) });
            if (value.Weapon.Arsenal == ArsenalWeapon.FirecrackerPinwheel && ArsenalArtwork.Frames("BangText") != null)
                flashes.Add(new Flash { Art = "BangText", At = value.Position + Vector2.up * 0.5f, Born = time, Life = 0.6f, Size = 0.9f });
        }

        public void Display(CombatSimulation sim, float time)
        {
            used = 0;
            bool still = PresentationSettings.ReducedMotion;
            if (sim != null)
                foreach (ArsenalProp p in sim.ArsenalProps) DrawProp(sim, p, still);
            for (int i = flashes.Count - 1; i >= 0; i--)
            {
                Flash f = flashes[i]; float t = (time - f.Born) / f.Life;
                if (t >= 1 || t < 0) { flashes.RemoveAt(i); continue; }
                Draw(ArsenalArtwork.Frame(f.Art, t), f.At + Vector2.up * t * 0.2f, f.Size * (0.8f + 0.3f * t), 0, 2150, new Color(1, 1, 1, Mathf.Clamp01((1 - t) * 2.5f)));
            }
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
                    if (t < 0.3f) Draw(ArsenalArtwork.Frame("GlassShards", 0), at, 0.45f * (1 + t), 0, Order(at) + 1, new Color(1, 1, 1, 1 - t / 0.3f));
                    break;
                }
                case ArsenalPropKind.Petals:
                {
                    float t = Mathf.InverseLerp(p.Born, p.Until, now);
                    Draw(ArsenalArtwork.Frame("PaperPetal", 0, 1, p.Born.GetHashCode() & 7), at + (still ? Vector2.zero : new Vector2(Mathf.Sin(now * 2 + p.Born * 7) * 0.08f, -t * 0.12f)), 0.24f, still ? 0 : now * 60 + p.Born * 100, 70, new Color(1, 1, 1, Mathf.Clamp01((1 - t) * 3)));
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
                        Draw(ArsenalArtwork.Frame("InkPool", 0), at + Vector2.down * 0.15f, 1.4f, 0, 64, new Color(1, 1, 1, 0.85f), 0.45f);
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
                    Span(ArsenalArtwork.Frame("RoadStripes", 0), at - along * 0.8f, at + along * 0.8f, 0.5f, 65, new Color(1, 1, 1, 0.8f));
                    Draw(ArsenalArtwork.Frame("PlantedSign", still ? 1 : age < 0.35f ? 1 : 0), at + Vector2.up * 0.35f, 0.6f, still ? 0 : Mathf.Sin(age * 30) * 8 * Mathf.Clamp01(1 - age * 3), Order(at));
                    break;
                }
                case ArsenalPropKind.ToyBomb:
                {
                    if (p.Source != null) Span(ArsenalArtwork.Frame("DetonatorWire", 0), p.Source.Position, at, 0.06f, 62, new Color(1, 1, 1, 0.75f));
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
                    Draw(ArsenalArtwork.Frame("SpiralCrack", 0), at, 0.9f * Mathf.Clamp01(t * 3), still ? 0 : -now * 90, 63, new Color(1, 1, 1, 1 - t * 0.5f), 0.5f);
                    Draw(ArsenalArtwork.Frame("CandyDrillShot", 0), at + Vector2.up * (0.25f - 0.2f * t), 0.5f, -90 + (still ? 0 : Mathf.Sin(now * 50) * 6), Order(at));
                    break;
                }
                case ArsenalPropKind.Bump:
                {
                    if (now < p.At) break;
                    float t = (now - p.At) / Mathf.Max(0.05f, p.Until - p.At);
                    Draw(ArsenalArtwork.Frame("SteamPuff", t), at + Vector2.up * t * 0.2f, 0.6f * (0.7f + 0.5f * t), 0, 2100, new Color(1, 1, 1, 1 - t));
                    break;
                }
                case ArsenalPropKind.Batted:
                    if (age < 0.4f) Draw(ArsenalArtwork.Frame("HomeRunText", 0), p.Start + Vector2.up * (0.7f + age * 0.6f), 1.1f, 0, 2200, new Color(1, 1, 1, 1 - age / 0.4f));
                    break;
                case ArsenalPropKind.Grab:
                    if (p.Source != null) Span(ArsenalArtwork.Frame("ClawSpring", 0), p.Source.Position, at, 0.12f, Order(at) + 1);
                    Draw(ArsenalArtwork.Frame("ClawHead", 1), at, 0.4f, 0, Order(at) + 2);
                    break;
                case ArsenalPropKind.Plunge:
                    if (p.Source != null) Span(ArsenalArtwork.Frame("ElasticBand", 0), p.Source.Position, at, 0.07f, Order(at) + 1);
                    Draw(ArsenalArtwork.Frame("PlungerCup", 0), at, 0.32f, 0, Order(at) + 2);
                    break;
            }
        }
        private void OnDestroy() { foreach (SpriteRenderer view in pool) if (view != null) Destroy(view.gameObject); }
    }
}
