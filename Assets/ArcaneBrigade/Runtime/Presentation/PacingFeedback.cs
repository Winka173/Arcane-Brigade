using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The pacing moments on screen: a surge's warning at the edge it comes from, the Lantern Flare's light, the bell's
    /// ripple, the end-of-wave sweep tally under the wallet, and the chest receipt that prints what a chest gave.</summary>
    public sealed class PacingFeedback : MonoBehaviour
    {
        private sealed class Receipt { public Vector2 World; public float Born; public int Gold, Healing; public bool Egg, Heart; public int Seed; }
        private ArenaController owner;
        private Camera view;
        private readonly List<Receipt> receipts = new List<Receipt>();
        private float flareAt = -100, bellAt = -100;
        private Vector2 flareFrom, bellFrom;
        private int tallyShown, sweepGeneration = -1, sweepGold, arrived;
        private float tallyPulse = -100, sweepStart = -100;
        private readonly List<Vector3> pieces = new List<Vector3>();
        private GUIStyle hand, numbers, small;
        private int reelTicks;
        private static Texture2D ring, edge;
        public void Configure(ArenaController controller, Camera camera) { owner = controller; view = camera; }

        public void Observe(IReadOnlyList<CombatEvent> events, float now)
        {
            foreach (CombatEvent value in events)
            {
                if (value.Kind == CombatEventKind.Flare) { flareAt = now; flareFrom = value.Position; }
                else if (value.Kind == CombatEventKind.Bell) { bellAt = now; bellFrom = value.Position; }
                else if (value.Kind == CombatEventKind.ChestOpened && receipts.Count < 4)
                    receipts.Add(new Receipt { World = value.Position, Born = Time.unscaledTime, Gold = Mathf.RoundToInt(value.Value), Egg = value.Direction.x > 0.5f, Heart = value.Direction.y > 0.5f, Healing = Mathf.RoundToInt(value.Direction.y), Seed = value.SubjectId });
            }
        }

        public void Clear() { receipts.Clear(); flareAt = bellAt = -100; }

        private void Styles()
        {
            if (hand != null) return;
            hand = ArcadeTypography.Style(30); hand.font = ArcadeTypography.Hand; hand.alignment = TextAnchor.MiddleCenter; hand.wordWrap = false;
            hand.normal.textColor = ArcadeSkin.Ink;
            numbers = ArcadeTypography.Style(26, numeric: true); numbers.alignment = TextAnchor.MiddleCenter; numbers.wordWrap = false;
            small = ArcadeTypography.Style(15, true); small.alignment = TextAnchor.MiddleCenter; small.wordWrap = false; small.normal.textColor = ArcadeSkin.Ink;
        }

        private void OnGUI()
        {
            if (owner == null || owner.Simulation == null || owner.Session == null || view == null) return;
            CombatSimulation simulation = owner.Simulation;
            bool combat = owner.Session.State.Phase == SessionPhase.Combat;
            if (simulation.SweptGold > 0 && simulation.Generation != sweepGeneration) BeginSweep(simulation);
            bool sweeping = Time.unscaledTime - sweepStart < 2.8f;
            if (!combat) receipts.Clear();
            if (!combat && !sweeping) return;
            Styles();
            Matrix4x4 previous = ArcadeTypography.BeginViewport(view.pixelRect, 1920f, 1080f);
            int depth = GUI.depth; GUI.depth = -5;
            float now = simulation.VisualElapsed;
            if (combat)
            {
                DrawSurgeWarning(simulation, now);
                DrawLight(flareFrom, now - flareAt, 0.55f, new Color(1f, 0.86f, 0.52f), 1500f, true);
                DrawLight(bellFrom, now - bellAt, 0.7f, new Color(0.95f, 0.78f, 0.42f), 900f, false);
                DrawReceipts();
            }
            if (sweeping) DrawSweep();
            GUI.depth = depth;
            GUI.matrix = previous;
        }

        private Vector2 ToDesign(Vector2 world)
        {
            Vector3 screen = view.WorldToScreenPoint(world);
            Vector3 point = GUI.matrix.inverse.MultiplyPoint3x4(new Vector3(screen.x, Screen.height - screen.y, 0));
            return new Vector2(point.x, point.y);
        }

        // ---- the surge comes from that edge: warm ember light and three ink chevrons, no drum ---------------------------
        private void DrawSurgeWarning(CombatSimulation simulation, float now)
        {
            if (simulation.SurgeAt < 0 || Event.current.type != EventType.Repaint) return;
            float lead = simulation.SurgeAt - simulation.Elapsed;
            if (lead > SwarmPacingRules.SurgeWarning || lead < -0.2f) return;
            float t = 1 - Mathf.Clamp01(lead / SwarmPacingRules.SurgeWarning);
            float pulse = PresentationSettings.ReducedFlashes ? 0.7f : 0.55f + 0.45f * Mathf.Abs(Mathf.Sin(now * 6f));
            Vector2 from = -simulation.SurgeDirection;  // the side the wall stands on
            if (edge == null) edge = Gradient();
            Color previous = GUI.color;
            GUI.color = new Color(ArcadeSkin.Danger.r, ArcadeSkin.Danger.g, ArcadeSkin.Danger.b, 0.55f * t * pulse);
            Rect band = from.x < -0.5f ? new Rect(0, 0, 150, 1080) : from.x > 0.5f ? new Rect(1770, 0, 150, 1080) : from.y > 0.5f ? new Rect(0, 0, 1920, 120) : new Rect(0, 960, 1920, 120);
            Matrix4x4 matrix = GUI.matrix;
            // the gradient texture fades left to right; rotate it so it always fades away from the edge
            float angle = from.x < -0.5f ? 0 : from.x > 0.5f ? 180 : from.y > 0.5f ? 90 : -90;
            Vector2 centre = band.center;
            Rect drawn = Mathf.Abs(from.x) > 0.5f ? band : new Rect(centre.x - band.height * 0.5f, centre.y - band.width * 0.5f, band.height, band.width);
            ArcadeSkin.TurnAround(angle, centre);
            GUI.DrawTexture(drawn, edge);
            GUI.matrix = matrix;
            GUI.color = previous;
            // chevrons point inward
            Vector2 inward = simulation.SurgeDirection; inward.y = -inward.y; // screen space
            Vector2 anchor = from.x < -0.5f ? new Vector2(70, 540) : from.x > 0.5f ? new Vector2(1850, 540) : from.y > 0.5f ? new Vector2(960, 60) : new Vector2(960, 1020);
            Vector2 across = new Vector2(-inward.y, inward.x);
            for (int index = -1; index <= 1; index++)
            {
                Vector2 point = anchor + across * (index * (Mathf.Abs(inward.x) > 0.5f ? 260 : 420)) + inward * (PresentationSettings.ReducedMotion ? 0 : t * 26);
                Chevron(point, inward, 34, new Color(1f, 0.55f, 0.36f, 0.85f * t));
            }
        }

        private static void Chevron(Vector2 point, Vector2 direction, float size, Color color)
        {
            Vector2 side = new Vector2(-direction.y, direction.x);
            for (int step = 0; step < 7; step++)
            {
                float f = step / 6f;
                Vector2 a = point - direction * size * 0.5f + side * size * (0.5f - f * 0.5f) + direction * size * f * 0.5f;
                Vector2 b = point - direction * size * 0.5f - side * size * (0.5f - f * 0.5f) + direction * size * f * 0.5f;
                ArcadeSkin.Line(new Rect(a.x - 4, a.y - 4, 8, 8), color);
                ArcadeSkin.Line(new Rect(b.x - 4, b.y - 4, 8, 8), color);
            }
        }

        // ---- the flare's light and the bell's ripple ----------------------------------------------------------------
        private void DrawLight(Vector2 world, float age, float duration, Color color, float reach, bool wash)
        {
            if (age < 0 || age > duration || Event.current.type != EventType.Repaint) return;
            float t = age / duration;
            if (ring == null) ring = Ring();
            Vector2 centre = ToDesign(world);
            float radius = Mathf.Lerp(40, reach, 1 - (1 - t) * (1 - t));
            Color previous = GUI.color;
            if (wash && !PresentationSettings.ReducedFlashes)
            {
                GUI.color = new Color(color.r, color.g, color.b, (1 - t) * (1 - t) * 0.12f);
                GUI.DrawTexture(new Rect(0, 0, 1920, 1080), Texture2D.whiteTexture);
            }
            GUI.color = new Color(color.r, color.g, color.b, (1 - t) * (PresentationSettings.ReducedEffects ? 0.45f : 0.85f));
            GUI.DrawTexture(new Rect(centre.x - radius, centre.y - radius, radius * 2, radius * 2), ring);
            if (!PresentationSettings.ReducedEffects)
            {
                float inner = radius * 0.62f;
                GUI.color = new Color(color.r, color.g, color.b, (1 - t) * 0.4f);
                GUI.DrawTexture(new Rect(centre.x - inner, centre.y - inner, inner * 2, inner * 2), ring);
            }
            GUI.color = previous;
        }

        // ---- chest receipts: one reel per reward, the amount printed on its tile (play-test 20/10/2026: sized to 1-3 rewards)
        private void DrawReceipts()
        {
            for (int index = receipts.Count - 1; index >= 0; index--)
            {
                Receipt receipt = receipts[index];
                float age = Time.unscaledTime - receipt.Born;
                if (age > 2.1f) { receipts.RemoveAt(index); continue; }
                bool reduced = PresentationSettings.ReducedMotion;
                float unroll = reduced ? 1 : Mathf.SmoothStep(0, 1, Mathf.Clamp01(age / 0.22f));
                float alpha = Mathf.Clamp01((2.1f - age) / 0.3f);
                Vector2 anchor = ToDesign(receipt.World + Vector2.up * 0.9f) + Vector2.up * (reduced ? 0 : -Mathf.Min(age, 0.5f) * 30);
                int count = 1 + (receipt.Egg ? 1 : 0) + (receipt.Heart ? 1 : 0);
                const float Tile = 84, Gap = 10;
                float width = Mathf.Max(220, 28 + count * Tile + (count - 1) * Gap);
                Rect paper = new Rect(anchor.x - width * 0.5f, anchor.y - 132 * unroll, width, 132 * unroll);
                Color previous = GUI.color; GUI.color = new Color(1, 1, 1, alpha);
                ArcadeSkin.Ticket(paper, ArcadeSkin.Cream, ArcadeSkin.Gold);
                if (unroll > 0.95f)
                {
                    GUI.Label(new Rect(paper.x, paper.y + 4, paper.width, 22), "THE CHEST GIVES", small);
                    RunCatalog content = owner.Content;
                    Sprite coin = content.Arena.Coin, egg = content.EggSprite, heart = content.HeartSprite, lamp = PacingArtwork.Flare(content);
                    Sprite[] faces = { coin, egg, heart, lamp };
                    for (int reel = 0; reel < count; reel++)
                    {
                        // the reels in order: gold, then the egg, then the heart (only the rewards this chest gave)
                        bool isEgg = reel == 1 && receipt.Egg, isHeart = reel > 0 && !isEgg;
                        Sprite result = isEgg ? egg : isHeart ? heart : coin;
                        string amount = isEgg ? "+1" : isHeart ? "+" + Mathf.Max(1, receipt.Healing) : "+" + receipt.Gold.ToString("N0");
                        float stop = 0.32f + reel * 0.2f;
                        bool landed = reduced || age >= stop;
                        Rect slot = new Rect(paper.x + 14 + reel * (Tile + Gap), paper.y + 32, Tile, Tile + 8);
                        bool tile = ArcadeSkin.RewardTile(slot, landed); // the painted reward tile (cream card in brass)
                        if (!tile) ArcadeSkin.Line(slot, new Color(0.13f, 0.1f, 0.12f, 0.9f));
                        Sprite face = landed ? result : faces[(int)(age * 22 + reel * 3 + receipt.Seed) % faces.Length];
                        float drop = landed && !reduced ? Mathf.Max(0, 1 - (age - stop) / 0.08f) * -10 : 0;
                        Rect icon = new Rect(slot.x + (Tile - 50) * 0.5f, slot.y + 6 + drop, 50, 50);
                        if (face != null) { Color tint = GUI.color; if (!landed) GUI.color = new Color(tint.r, tint.g, tint.b, tint.a * 0.55f); RunHud.Sprite(icon, face); GUI.color = tint; }
                        if (landed && (age >= stop + 0.12f || reduced))
                        {
                            int size = numbers.fontSize; numbers.normal.textColor = tile ? (isHeart ? new Color(.62f, .08f, .1f) : isEgg ? new Color(.08f, .36f, .42f) : new Color(.52f, .33f, .04f)) : isHeart ? new Color(1f, 0.7f, 0.66f) : isEgg ? ArcadeSkin.Teal : ArcadeSkin.Gold;
                            while (numbers.fontSize > 12 && numbers.CalcSize(new GUIContent(amount)).x > Tile - 6) numbers.fontSize--;
                            GUI.Label(tile ? ArcadeSkin.RewardNumberRect(slot) : new Rect(slot.x, slot.y + 58, Tile, 30), amount, numbers); // inside the painted tile's rim
                            numbers.fontSize = size;
                        }
                        if (landed && Event.current.type == EventType.Repaint && age - stop < 0.02f && reelTicks < 64) { reelTicks++; CheckpointAudio.PlayInterfaceId(reel == count - 1 ? "ui.receipt" : "shop.ticket.sway", 0.05f); }
                    }
                }
                GUI.color = previous;
            }
            if (Event.current.type == EventType.Repaint) reelTicks = Mathf.Max(0, reelTicks - 1);
        }

        // ---- the sweep: what flew home when the wave was won ------------------------------------------------------------
        private void BeginSweep(CombatSimulation simulation)
        {
            sweepGeneration = simulation.Generation; sweepGold = simulation.SweptGold; sweepStart = Time.unscaledTime;
            pieces.Clear(); foreach (Vector3 piece in simulation.SweptPieces) pieces.Add(piece);
            arrived = 0; tallyShown = 0; tallyPulse = -100;
        }

        private void DrawSweep()
        {
            float age = Time.unscaledTime - sweepStart;
            bool reduced = PresentationSettings.ReducedMotion;
            Vector2 wallet = new Vector2(56, 116), eggs = new Vector2(56, 162);
            RunCatalog content = owner.Content;
            int landed = 0, shownGold = 0;
            for (int index = 0; index < pieces.Count; index++)
            {
                Vector3 piece = pieces[index];
                float t = reduced ? 1 : Mathf.Clamp01((age - 0.12f - index * 0.014f) / 0.42f);
                bool egg = piece.z < 0;
                if (t >= 1) { landed++; if (!egg) shownGold += Mathf.RoundToInt(piece.z); continue; }
                if (Event.current.type != EventType.Repaint) continue;
                Vector2 from = ToDesign(new Vector2(piece.x, piece.y)), to = egg ? eggs : wallet;
                float eased = t * t * (3 - 2 * t);
                Vector2 point = Vector2.Lerp(from, to, eased) + Vector2.up * (-Mathf.Sin(t * Mathf.PI) * 120);
                float size = Mathf.Lerp(26, 18, t);
                RunHud.Sprite(new Rect(point.x - size * 0.5f, point.y - size * 0.5f, size, size), egg ? content.EggSprite : content.Arena.Coin);
            }
            if (landed > arrived && Event.current.type == EventType.Repaint)
            {
                // coins landing in the counter are silent (requirement 22/10/2026)
                arrived = landed; tallyPulse = Time.unscaledTime;
            }
            // pieces beyond the drawn ones (and merged coins) are counted once the last one lands
            tallyShown = landed >= pieces.Count ? sweepGold : Mathf.Min(sweepGold, shownGold);
            float fade = Mathf.Clamp01((2.8f - age) / 0.35f);
            float pop = reduced ? 0 : Mathf.Clamp01(1 - (Time.unscaledTime - tallyPulse) / 0.12f) * 4;
            Color previous = GUI.color; GUI.color = new Color(1, 1, 1, fade);
            Rect slip = new Rect(222, 98, 176, 36);
            ArcadeSkin.Ticket(new Rect(slip.x - pop, slip.y - pop, slip.width + pop * 2, slip.height + pop * 2), ArcadeSkin.Cream, ArcadeSkin.Gold);
            hand.normal.textColor = ArcadeSkin.Ink; hand.fontSize = 26;
            GUI.Label(new Rect(slip.x + 8, slip.y - 2, 70, slip.height), "swept", hand);
            numbers.normal.textColor = new Color(0.55f, 0.36f, 0.08f);
            GUI.Label(new Rect(slip.x + 72, slip.y, slip.width - 78, slip.height), "+" + tallyShown.ToString("N0"), numbers);
            hand.fontSize = 30;
            GUI.color = previous;
        }

        private static Texture2D Ring()
        {
            const int n = 128;
            var texture = new Texture2D(n, n, TextureFormat.RGBA32, false) { name = "PacingRing", wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
            var pixels = new Color[n * n];
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float d = new Vector2(x + 0.5f - n / 2f, y + 0.5f - n / 2f).magnitude / (n / 2f);
                    float band = Mathf.Clamp01(1 - Mathf.Abs(d - 0.9f) / 0.1f);
                    float glow = Mathf.Clamp01(1 - d) * 0.18f;
                    pixels[y * n + x] = new Color(1, 1, 1, Mathf.Clamp01(band * band + glow));
                }
            texture.SetPixels(pixels); texture.Apply(false, true); return texture;
        }

        private static Texture2D Gradient()
        {
            const int n = 64;
            var texture = new Texture2D(n, 1, TextureFormat.RGBA32, false) { name = "SurgeEdge", wrapMode = TextureWrapMode.Clamp, hideFlags = HideFlags.HideAndDontSave };
            for (int x = 0; x < n; x++) { float t = x / (n - 1f); texture.SetPixel(x, 0, new Color(1, 1, 1, (1 - t) * (1 - t))); }
            texture.Apply(false, true); return texture;
        }
    }
}
