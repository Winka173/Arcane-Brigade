using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>One layout pass for everything decorative in the shop. The shop's working parts are reserved first —
    /// the HUD panels, the merchant and its speech bubble, every offer with its price ticket, the reroll machine, the
    /// fusion altar, the exit and its CONTINUE plate, every one of the warm chair's 19 positions, and story props owned
    /// by other systems (secret invitations, Act I seated travellers) — then the decorations ask for room in priority
    /// order (journal cabinet, lamp, lanterns, the reserved place, keepsakes, architecture, distant watchers). Each one
    /// gets the free spot nearest to where it would like to stand, tested against real footprints (width, height and
    /// float clearance, not centres), or is left out of this shop. The plan is made once per shop visit and only redone
    /// when the room itself changes, so nothing jumps around while the traveller shops. Combat is untouched.</summary>
    [DefaultExecutionOrder(-90)]
    public sealed class ShopLayout : MonoBehaviour
    {
        public const float HalfWidth = 8.53f, HalfHeight = 4.8f; // RestViewHeight 4.8 at 16:9, camera fixed on the room
        public const float Hover = 0.7f;   // the most a floating prop rises above its floor contact
        public const float Pad = 0.06f;    // breathing room between footprints
        private sealed class Prop { public string Id; public Vector2 Preferred; public Vector2 Size; public int Priority; public Vector2 Foot; public bool Visible; public bool Planned; public bool Shrink, Rim, Transient; public float Fit = 1; }
        private readonly Dictionary<string, Prop> props = new Dictionary<string, Prop>();
        private readonly List<Prop> order = new List<Prop>();
        private readonly List<Rect> reserved = new List<Rect>();
        private readonly List<string> reservedNames = new List<string>();
        private readonly List<Rect> placed = new List<Rect>();
        private ArenaController owner;
        private int roomKey = int.MinValue;
        private int foreignKey;
        private bool dirty = true;
        private float nextScan;
        public static ShopLayout Active { get; private set; }
        public int Plans { get; private set; }
        public int Hidden { get; private set; }
        public IReadOnlyList<Rect> Reserved => reserved;
        public IReadOnlyList<string> ReservedNames => reservedNames;
        /// <summary>One line per decoration: where it wished to stand, where it went (or that it sat this shop out).</summary>
        public IEnumerable<string> Describe()
        {
            foreach (Prop prop in order)
                yield return prop.Id + " p" + prop.Priority + " wish " + prop.Preferred.ToString("0.00") + " size " + prop.Size.ToString("0.00") + (!prop.Planned ? " (unplanned)" : prop.Visible ? " -> " + prop.Foot.ToString("0.00") + (prop.Fit < 1 ? " at " + prop.Fit.ToString("0.00") + "x" : "") : " -> left out");
            for (int i = 0; i < reserved.Count; i++) yield return "reserved " + reservedNames[i] + " " + reserved[i].ToString("0.00");
        }
        public void Configure(ArenaController value) { owner = value; Active = this; }
        private void OnDestroy() { if (Active == this) Active = null; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] private static void ResetActive() => Active = null;
        public bool InShop => owner?.Session != null && owner.Simulation != null && owner.Session.State.Phase == SessionPhase.Shop && !owner.Session.SecretRoomOpen;
        /// <summary>Remove a scene-owned footprint as soon as that scene ends. Stable props keep their registration.</summary>
        public void Release(string id)
        {
            if (id == null || !props.TryGetValue(id,out Prop prop)) return;
            props.Remove(id); order.Remove(prop); dirty = true;
        }

        /// <summary>The footprint a prop of this size covers when its floor contact is at foot (float clearance included).</summary>
        /// <summary>The ferry stands by the right wall like a visitor (it may use another night's chair spot, rather a little
        /// smaller and close than far away) but, unlike one, it is never left out: it then takes any free spot.</summary>
        public const string FerryId = "ferry";
        public static Rect Footprint(Vector2 foot, Vector2 size) => new Rect(foot.x - size.x * 0.5f - 0.16f, foot.y + 0.05f, size.x + 0.32f, size.y + Hover);

        /// <summary>Asks for room in the shop. Returns false when this shop has no free spot for the prop (hide it);
        /// otherwise foot is where its floor contact goes. Outside the shop it returns preferred unchanged.</summary>
        public bool Place(string id, Vector2 preferred, Vector2 size, int priority, out Vector2 foot) => Place(id, preferred, size, priority, false, false, out foot, out _);
        /// <summary>As the short form; a story prop that must appear (shrink) may come in smaller when the room is
        /// crowded: draw it at fit times its size. A rim prop (distant watchers) only takes spots at the edges of the room.</summary>
        public bool Place(string id, Vector2 preferred, Vector2 size, int priority, bool shrink, bool rim, out Vector2 foot, out float fit)
        {
            foot = preferred; fit = 1;
            if (!InShop) return true;
            if (!props.TryGetValue(id, out Prop prop))
            {
                prop = new Prop { Id = id }; props[id] = prop; order.Add(prop); dirty = true;
            }
            // small sprite changes (animation, damage frames) do not reshuffle the room
            if (prop.Priority != priority || (prop.Preferred - preferred).sqrMagnitude > 0.04f || Mathf.Abs(prop.Size.x - size.x) > prop.Size.x * 0.15f + 0.02f || Mathf.Abs(prop.Size.y - size.y) > prop.Size.y * 0.15f + 0.02f)
            {
                prop.Priority = priority; prop.Preferred = preferred; prop.Size = size; dirty = true;
            }
            if (prop.Shrink != shrink || prop.Rim != rim) { prop.Shrink = shrink; prop.Rim = rim; dirty = true; }
            bool transient = id == FerryId || id.StartsWith("story:") && id.Contains("LastCustomer");
            if (prop.Transient != transient) { prop.Transient = transient; dirty = true; }
            if (dirty && plannedFrame != Time.frameCount) Plan(); // new arrivals in the same frame join next frame's plan
            foot = prop.Planned ? prop.Foot : preferred;
            fit = prop.Planned ? prop.Fit : 1;
            return !prop.Planned || prop.Visible;
        }
        /// <summary>The planned floor contact of a prop that has already asked for room (lights and tags follow it).</summary>
        public bool TryFoot(string id, out Vector2 foot)
        {
            foot = Vector2.zero;
            if (!InShop || !props.TryGetValue(id, out Prop prop) || !prop.Planned || !prop.Visible) return false;
            foot = prop.Foot; return true;
        }

        private void Update()
        {
            if (!InShop) { if (roomKey != int.MinValue) { roomKey = int.MinValue; dirty = true; foreach (Prop prop in order) prop.Planned = false; } return; }
            int key = RoomKey();
            if (key != roomKey) { roomKey = key; dirty = true; }
            if (Time.unscaledTime >= nextScan)
            {
                nextScan = Time.unscaledTime + 0.25f;
                int foreign = ForeignKey();
                if (foreign != foreignKey) { foreignKey = foreign; dirty = true; }
            }
        }
        private int RoomKey()
        {
            var state = owner.Session.State;
            unchecked
            {
                int key = state.Wave * 7919 + (owner.Shop != null && owner.Shop.Special ? 13 : 0) + (state.Endless ? 29 : 0);
                if (owner.Shop != null) foreach (WorldShop.Station station in owner.Shop.Stations) key = key * 31 + Mathf.RoundToInt(station.Position.x * 10) * 7 + Mathf.RoundToInt(station.Position.y * 10);
                var chair = owner.GetComponent<WaitingRealmStage>();
                if (chair != null && chair.ChairVisible) key = key * 17 + 5;
                return key;
            }
        }
        private int ForeignKey()
        {
            unchecked
            {
                int key = 1;
                // secret invitations and the Act I seated travellers are parented straight to the arena
                foreach (Transform child in owner.transform) if (child.gameObject.activeInHierarchy && Foreign(child.name)) key = key * 31 + child.name.GetHashCode() + Mathf.RoundToInt(child.position.x * 10) * 3 + Mathf.RoundToInt(child.position.y * 10);
                return key;
            }
        }
        // interactive story props owned elsewhere: reserved where they are (their systems test the player against those spots)
        private static bool Foreign(string name) => name.StartsWith("SecretInvitation_") || name.StartsWith("OutsideStage") || name == "TheHandWaits" || name.StartsWith("PrismaticReward"); // the Keeper's hand and gifts too
        private sealed class Adopted { public Transform Root; public SpriteRenderer View; public GroundShadow Shadow; public Vector2 Wish, Size, Applied; public string Id; public int Priority; public float Scale; }
        private readonly List<Adopted> adopted = new List<Adopted>();
        /// <summary>Act I's standing story figures (the seated travellers, the Last Customer beside the warm chair) are
        /// placed once by ActOneWorldArt; they join the same layout here, so they never stand on the HUD, a stall, the
        /// chair's path or another prop. Their system keeps drawing them; only their spot is moved.</summary>
        private void LateUpdate()
        {
            if (!InShop) { foreach (var entry in adopted) Release(entry.Id); adopted.Clear(); return; }
            adopted.RemoveAll(entry => { if (entry.Root != null) return false; Release(entry.Id); return true; });
            foreach (Transform child in owner.transform)
            {
                if (!child.name.StartsWith("ActOne-") || !child.gameObject.activeSelf || child.GetComponent<FloatingStoryProp>() != null) continue;
                if (adopted.Exists(entry => entry.Root == child)) continue;
                var view = child.GetComponent<SpriteRenderer>(); if (view == null || view.sprite == null) continue;
                Rect ink = Ink(view);
                adopted.Add(new Adopted { Root = child, View = view, Shadow = child.GetComponent<GroundShadow>(), Wish = new Vector2(ink.center.x, ink.yMin), Size = ink.size, Id = "story:" + child.name, Priority = child.name.Contains("LastCustomer") ? -1 : 6, Scale = child.localScale.x });
            }
            foreach (Adopted entry in adopted)
            {
                // the Last Customer must be there (smaller if the room is full); the seated travellers keep to the edges
                bool customer = entry.Priority < 0;
                bool shown = Place(entry.Id, entry.Wish, entry.Size, entry.Priority, customer, !customer, out Vector2 foot, out float fit);
                Vector2 delta = shown ? foot - entry.Wish : Vector2.zero;
                float scale = entry.Scale * (shown ? fit : 1);
                if (delta != entry.Applied || Mathf.Abs(entry.Root.localScale.x - scale) > 0.001f)
                {
                    // keep the painted feet on the planned spot whatever the scale
                    entry.Root.localScale = Vector3.one * scale;
                    entry.Root.position = (Vector3)(entry.Wish + delta) - entry.Root.TransformVector(WorldSpriteGrounding.Foot(entry.View.sprite));
                    entry.Shadow?.TranslateGround(delta - entry.Applied); entry.Applied = delta;
                }
                if (entry.View.enabled != shown) { entry.View.enabled = shown; if (!shown) entry.Shadow?.Hide(); }
            }
        }

        private string todaysChair = "";
        private void Reserve(string name, Rect rect) { reserved.Add(rect); reservedNames.Add(name); }
        private int plannedFrame = -1;
        private void Plan()
        {
            dirty = false; Plans++; plannedFrame = Time.frameCount;
            reserved.Clear(); reservedNames.Clear(); placed.Clear();
            ReserveRoom();
            order.Sort((a, b) => a.Priority != b.Priority ? a.Priority.CompareTo(b.Priority) : string.CompareOrdinal(a.Id, b.Id));
            Hidden = 0;
            Rect view = Rect.MinMaxRect(-HalfWidth + 0.05f, -HalfHeight, HalfWidth - 0.05f, HalfHeight - 0.05f);
            foreach (Prop prop in order)
            {
                prop.Planned = true; prop.Fit = 1; prop.Visible = false;
                Rect wanted = Footprint(prop.Preferred, prop.Size);
                // far outside the room (architecture standing out in the dark): nothing to arrange
                if (!wanted.Overlaps(view)) { prop.Foot = prop.Preferred; prop.Visible = true; continue; }
                // a one-visit figure belongs beside what it came for: rather smaller and close than full size across the room
                if (prop.Transient)
                    for (float fit = 1f; !prop.Visible && fit > 0.55f; fit -= 0.13f)
                        if (prop.Visible = Find(prop, prop.Size * fit, view, out prop.Foot, 3.5f)) prop.Fit = fit;
                if (!prop.Visible && (!prop.Transient || prop.Id == FerryId)) prop.Visible = Find(prop, prop.Size, view, out prop.Foot);
                // the journal cabinet and the lamp are part of the story: rather smaller than missing
                for (float fit = 0.85f; !prop.Visible && prop.Shrink && !prop.Transient && fit > 0.5f; fit -= 0.13f)
                    if (prop.Visible = Find(prop, prop.Size * fit, view, out prop.Foot)) prop.Fit = fit;
                if (prop.Visible) placed.Add(Footprint(prop.Foot, prop.Size * prop.Fit)); else Hidden++;
            }
        }
        private bool Find(Prop prop, Vector2 size, Rect view, out Vector2 foot, float within = float.MaxValue)
        {
            foot = prop.Preferred;
            if (Fits(Footprint(foot, size), view, prop.Rim, prop.Transient)) return true;
            // every floor point of the room on a fine grid, tried nearest-first: the first that fits wins
            if (grid == null)
            {
                var points = new List<Vector2>();
                for (float x = -HalfWidth + 0.2f; x <= HalfWidth - 0.2f; x += 0.125f)
                    for (float y = -HalfHeight + 0.05f; y <= HalfHeight - 0.4f; y += 0.125f) points.Add(new Vector2(x, y));
                grid = points.ToArray(); distances = new float[grid.Length]; byDistance = new int[grid.Length]; sortedValid = false;
            }
            // requirement 25/10/2026: a prop retried smaller (shrink, a one-visit figure) asks from the same spot; the ~10,000
            // floor points are sorted once per spot, not once per try
            if (!sortedValid || sortedFor.x != prop.Preferred.x || sortedFor.y != prop.Preferred.y)
            {
                for (int i = 0; i < grid.Length; i++) { distances[i] = (grid[i] - prop.Preferred).sqrMagnitude; byDistance[i] = i; }
                System.Array.Sort(distances, byDistance);
                sortedFor = prop.Preferred; sortedValid = true;
            }
            for (int i = 0; i < byDistance.Length; i++)
            {
                if (distances[i] > within * within) break;
                Vector2 candidate = grid[byDistance[i]];
                if (Fits(Footprint(candidate, size), view, prop.Rim, prop.Transient)) { foot = candidate; return true; }
            }
            return false;
        }
        private static Vector2[] grid;
        private static float[] distances;
        private static int[] byDistance;
        private static Vector2 sortedFor;
        private static bool sortedValid;
        private static readonly Rect Floor = Rect.MinMaxRect(-5.4f, -3.7f, 5.4f, 2.9f); // where the traveller shops
        /// <param name="transient">a figure that is only here on this one visit (the Last Customer beside the chair) may
        /// stand where the chair will be on other visits, never on today's chair</param>
        private bool Fits(Rect rect, Rect view, bool rim = false, bool transient = false)
        {
            // things at the rim of the room (watchers, seated travellers) may lean out past the left and right edges
            // (a visiting figure such as the Last Customer stays whole on screen; rim watchers lean out only a little)
            float lean = rim ? rect.width * 0.25f : 0;
            if (rect.xMin < view.xMin - lean || rect.xMax > view.xMax + lean || rect.yMin < view.yMin || rect.yMax > view.yMax) return false;
            if (rim && rect.Overlaps(Floor)) return false;
            Rect padded = new Rect(rect.x - Pad, rect.y - Pad, rect.width + Pad * 2, rect.height + Pad * 2);
            for (int i = 0; i < reserved.Count; i++)
            {
                if (transient && reservedNames[i].StartsWith("chair:") && reservedNames[i] != todaysChair) continue;
                if (padded.Overlaps(reserved[i])) return false;
            }
            foreach (Rect other in placed) if (padded.Overlaps(other)) return false;
            return true;
        }

        /// <summary>Everything the traveller uses in the shop, in world units (the shop camera never moves).</summary>
        private void ReserveRoom()
        {
            // screen furniture (1920x1080 HUD rects converted at the fixed shop framing)
            Reserve("hud:wallet+note", Gui(0, 0, 385, 300));
            Reserve("hud:wave", Gui(380, 18, 290, 64));
            Reserve("hud:bonds", Gui(1545, 10, 365, 260));
            Reserve("hud:continue", Gui(720, 925, 480, 110));
            Reserve("hud:card", Gui(400, 795, 1120, 285)); // the stall's description card: nothing stands behind it
            if (owner.Shop != null)
            {
                Vector2 merchant = owner.Shop.MerchantCenter; float height = owner.Shop.MerchantHeight;
                Bounds drawn = owner.Shop.MerchantBounds; // the painted merchant's own proportions (it may still be arriving)
                float half = drawn.size.y > 0.1f ? Mathf.Clamp(height * drawn.size.x / drawn.size.y * 0.5f, height * 0.4f, height * 0.62f) : height * 0.46f;
                half += 0.35f; // its painted ink (hands, cup, ledger) reaches past the estimate; the lamps kept brushing it
                Reserve("merchant", Rect.MinMaxRect(merchant.x - half, merchant.y - height * 0.5f - 0.05f, merchant.x + half, merchant.y + height * 0.5f + 0.1f));
                Reserve("merchant:speech", Rect.MinMaxRect(1.85f, 1.15f, 5.7f, 2.75f));
                foreach (WorldShop.Station station in owner.Shop.Stations)
                {
                    Vector2 at = station.Position;
                    if (station.Action == "NEXT HALL") Reserve("exit", Rect.MinMaxRect(at.x - 1.05f, -HalfHeight, at.x + 1.05f, at.y + 1.6f));
                    else if (station.Action == "REROLL") Reserve("reroll", Rect.MinMaxRect(at.x - 0.62f, at.y - 1.05f, at.x + 0.62f, at.y + 0.72f));
                    else Reserve("station:" + station.Action, Rect.MinMaxRect(at.x - 1.05f, at.y - 0.9f, at.x + 1.05f, at.y + 2.25f)); // pedestal, floating stock, its tag and price ticket with room to breathe
                }
            }
            // every place the warm chair will stand on this journey, not only today's
            var chair = owner.GetComponent<WaitingRealmStage>();
            var state = owner.Session.State;
            if (chair != null && chair.ChairVisible && !state.Endless && state.Wave <= ActOneCanon.Waves)
            {
                Vector2 size = chair.ChairSize;
                for (int wave = 1; wave <= 19; wave++) Reserve("chair:" + wave, Footprint(ActOneCanon.ChairPosition(wave), size));
                todaysChair = "chair:" + Mathf.Clamp(state.Wave, 1, 19);
            }
            // story props drawn by other systems
            foreach (SpriteRenderer view in owner.GetComponentsInChildren<SpriteRenderer>())
            {
                if (!view.enabled || view.sprite == null) continue;
                Transform t = view.transform;
                while (t.parent != null && t.parent != owner.transform && !Foreign(t.name)) t = t.parent;
                if (!Foreign(t.name)) continue;
                Reserve("story:" + t.name, Ink(view));
            }
        }
        private Rect Gui(float x, float y, float width, float height)
            => Rect.MinMaxRect((x / 1920f * 2 - 1) * HalfWidth, (1 - (y + height) / 1080f * 2) * HalfHeight, ((x + width) / 1920f * 2 - 1) * HalfWidth, (1 - y / 1080f * 2) * HalfHeight);
        /// <summary>World rectangle of a sprite's painted ink (not its transparent margins).</summary>
        public static Rect Ink(SpriteRenderer view)
        {
            Rect ink = SpriteInkMetrics.BoundsOf(view.sprite);
            if (view.flipX) ink.x = -ink.xMax;
            Vector3 a = view.transform.TransformPoint(new Vector3(ink.xMin, ink.yMin)), b = view.transform.TransformPoint(new Vector3(ink.xMax, ink.yMax));
            Vector3 c = view.transform.TransformPoint(new Vector3(ink.xMin, ink.yMax)), d = view.transform.TransformPoint(new Vector3(ink.xMax, ink.yMin));
            return Rect.MinMaxRect(Mathf.Min(a.x, b.x, c.x, d.x), Mathf.Min(a.y, b.y, c.y, d.y), Mathf.Max(a.x, b.x, c.x, d.x), Mathf.Max(a.y, b.y, c.y, d.y));
        }
        /// <summary>Width and painted height of a sprite drawn at the given world width.</summary>
        public static Vector2 SizeAtWidth(Sprite sprite, float width)
        {
            if (sprite == null) return Vector2.zero;
            Rect ink = SpriteInkMetrics.BoundsOf(sprite);
            return new Vector2(width, width * ink.height / Mathf.Max(0.01f, ink.width));
        }
        public static Vector2 SizeAtHeight(Sprite sprite, float height)
        {
            if (sprite == null) return Vector2.zero;
            Rect ink = SpriteInkMetrics.BoundsOf(sprite);
            return new Vector2(height * ink.width / Mathf.Max(0.01f, ink.height), height);
        }
    }
}
