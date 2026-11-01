using UnityEngine;

namespace ArcaneBrigade
{
    // The Forgotten Theatre's look (requirement 25/10/2026: each map draws in its own file; MapEnvironmentVisual and MapAtmosphere
    // dispatch here). Props come from Resources/Environment/Theatre (MapIdentity.Folder): Firework_Machine, Curtain_Velvet,
    // Seats_Row; until they are painted each has a code stand-in. The smoke and the spotlight's pool are a shader
    // (Resources/Shaders/MapTheatreSmoke) with an ink fallback; every other effect is ink.

    public sealed partial class MapEnvironmentVisual
    {
        private static readonly InkPalette TheatreTomato = new InkPalette(new Color(.28f, .05f, .05f), new Color(.86f, .22f, .18f), new Color(1f, .72f, .62f), new Color(1f, .42f, .32f), new Color(.45f, .7f, .3f));
        private static readonly InkPalette TheatreVelvet = new InkPalette(new Color(.12f, .02f, .06f), new Color(.5f, .08f, .16f), new Color(.92f, .5f, .55f), new Color(.85f, .3f, .38f), new Color(.95f, .76f, .32f));
        private static readonly InkPalette TheatreGhostInk = new InkPalette(new Color(.1f, .06f, .2f), new Color(.62f, .56f, .9f), new Color(.95f, .94f, 1f), new Color(.75f, .7f, 1f), new Color(.9f, .6f, .85f));
        private static readonly Color[] TheatreSpotColours = { new Color(1f, .97f, .88f), new Color(1f, .84f, .36f), new Color(1f, .6f, .8f) };
        private static readonly InkToken[] TheatreSpotTokens = { InkToken.Star, InkToken.Spark, InkToken.Seal };
        private static readonly int[] TheatreBankIds = { Shader.PropertyToID("_Bank0"), Shader.PropertyToID("_Bank1"), Shader.PropertyToID("_Bank2"), Shader.PropertyToID("_Bank3") };
        private static Mesh theatreQuad;
        private Material theatreSmoke;
        private bool theatreSmokeTried;
        private float theatreDrumSeen = -100, theatreCurtainSeen = -100, theatreFiredSeen = -100, theatreReadySeen = -100, theatreEncoreSeen = -100;
        private int theatreCrowd;

        /// <summary>Draws the map's living elements from its MapEnvironmentState fields (Put, Ring, Band, Lane, ink).</summary>
        private void DrawTheatre(MapEnvironmentState s, float t)
        {
            Camera cam = Camera.main;
            bool ortho = cam != null && cam.orthographic;
            Vector2 view = cam != null ? (Vector2)cam.transform.position : sim.Champion != null ? sim.Champion.Position : s.Anchor;
            float halfH = ortho ? cam.orthographicSize : 6, halfW = halfH * (cam != null ? cam.aspect : 1.78f);
            float now = still ? 0 : t;
            Color spot = TheatreSpotColour(s, t);
            // a second before it changes colour the light flickers (not with Reduced Flashes or Motion)
            float warn = Mathf.Clamp01(t - (s.TheatreNextColour - 1));
            float on = calm || still ? 1 : 1 - .22f * warn * (Mathf.Sin(t * 31) > 0 ? 1 : 0);
            TheatreFloor(s, t, now, spot, on, ortho ? cam : null, view, halfW, halfH);
            TheatreBeam(s, t, now, spot, on, view, halfH);
            TheatreDrum(s, t);
            TheatreAudience(s, t, now);
            TheatreCurtain(s, t, now, view, halfW);
            TheatreMachine(s, t, now, view, halfW, halfH);
            TheatreEncore(s, t, now);
        }
        /// <summary>The spotlight lifts a standing prop inside its pool back to full light (0 none, 1 fully lit).</summary>
        private float TheatrePropLight(MapEnvironmentState s, Vector2 at)
        {
            float d = (at - s.TheatreSpot).magnitude / MapEnvironmentRules.TheatreSpotRadius;
            return 1 - Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.7f, 1.2f, d));
        }
        /// <summary>The spotlight's colour now (it crossfades for 0.6 s when it changes).</summary>
        internal static Color TheatreSpotColour(MapEnvironmentState s, float t)
        {
            Color c = TheatreSpotColours[s.TheatreSpotColour % 3];
            float blend = Mathf.Clamp01((t - s.TheatreColourAt) / .6f);
            return blend >= 1 ? c : Color.Lerp(TheatreSpotColours[(s.TheatreSpotColour + 2) % 3], c, Mathf.SmoothStep(0, 1, blend));
        }
        private static float TheatreHash(int i, int k) => Mathf.Repeat(Mathf.Sin(i * 12.9898f + k * 78.233f) * 43758.5453f, 1);
        private static float TheatreOut(float x) { x = Mathf.Clamp01(x); return 1 - (1 - x) * (1 - x) * (1 - x); }
        private static Vector2 TheatrePit(MapEnvironmentState s) => s.TheatreSeats + Vector2.up * 1.8f;
        /// <summary>A painted prop standing on the floor, its shadow cast from its foot.</summary>
        private SpriteRenderer TheatreStand(string path, Vector2 at, float height, float alpha = 1, float angle = 0)
        {
            int slot = used; bool known = slot < shadows.Count;
            if (known) shadows[slot].PinCastToFoot = true;
            SpriteRenderer r = Put(path, at, height, alpha: alpha, angle: angle);
            if (known) shadows[slot].PinCastToFoot = false;
            return r;
        }
        private static Mesh TheatreQuad()
        {
            if (theatreQuad != null) return theatreQuad;
            theatreQuad = new Mesh { name = "Theatre smoke quad", hideFlags = HideFlags.DontSave };
            theatreQuad.vertices = new[] { new Vector3(-.5f, -.5f, 0), new Vector3(.5f, -.5f, 0), new Vector3(.5f, .5f, 0), new Vector3(-.5f, .5f, 0) };
            theatreQuad.triangles = new[] { 0, 2, 1, 0, 3, 2 }; theatreQuad.RecalculateBounds();
            return theatreQuad;
        }

        // ------------------------------------------------------------------ the smoke, the light's pool and its beam
        private void TheatreFloor(MapEnvironmentState s, float t, float now, Color spot, float on, Camera cam, Vector2 view, float halfW, float halfH)
        {
            // the drum parts the banks: they thin out, roll across and thicken again where they settle
            float part = Mathf.Clamp01((t - s.TheatreDrumAt) / MapEnvironmentRules.TheatreSmokePart);
            float density = part < 1 ? 1 - .85f * Mathf.Sin(part * Mathf.PI) : 1;
            float amount = (PresentationSettings.ReducedEffects ? .5f : .8f) * (PresentationSettings.HighReadability ? .6f : 1);
            float rx = MapEnvironmentRules.TheatreSmokeRx, ry = MapEnvironmentRules.TheatreSmokeRy, r = MapEnvironmentRules.TheatreSpotRadius;
            if (!theatreSmokeTried)
            {
                theatreSmokeTried = true;
                Shader shader = Resources.Load<Shader>("Shaders/MapTheatreSmoke");
                if (shader != null && shader.isSupported) theatreSmoke = new Material(shader) { name = "Theatre stage smoke", hideFlags = HideFlags.DontSave };
            }
            if (theatreSmoke != null && cam != null)
            {
                Material m = theatreSmoke;
                for (int i = 0; i < 4; i++) m.SetVector(TheatreBankIds[i], new Vector4(s.TheatreSmoke[i].x, s.TheatreSmoke[i].y, rx, ry));
                m.SetVector("_BankDensity", Vector4.one * density);
                float drumAge = t - s.TheatreDrumAt; Vector2 pit = TheatrePit(s);
                m.SetVector("_Drum", new Vector4(pit.x, pit.y, drumAge * 8.5f, drumAge < 2.2f && !still ? 1 - drumAge / 2.2f : 0));
                m.SetVector("_Spot", new Vector4(s.TheatreSpot.x, s.TheatreSpot.y, r, on));
                m.SetColor("_SpotColour", spot);
                m.SetColor("_SmokeLight", new Color(.8f, .74f, .88f)); m.SetColor("_SmokeShade", new Color(.33f, .27f, .42f));
                m.SetFloat("_Clock", now); m.SetFloat("_Amount", amount); m.SetFloat("_Dust", PresentationSettings.ReducedEffects || calm ? 0 : 1);
                // drawn this frame only, under the floor marks' ink and every body (sorting order 0)
                Graphics.DrawMesh(TheatreQuad(), Matrix4x4.TRS(new Vector3(view.x, view.y, 0), Quaternion.identity, new Vector3(halfW * 2 + 2, halfH * 2 + 2, 1)), m, 0, cam);
                Marks++;
                return;
            }
            // without the shader: each bank as soft overlapping washes, the light as one solid pool
            for (int i = 0; i < 4; i++)
                for (int k = -1; k <= 1; k++)
                    ink.Wash(s.TheatreSmoke[i] + new Vector2(k * rx * .45f + Mathf.Sin(now * .4f + i + k) * .2f, k * .12f), ry * 1.15f, new Color(.62f, .56f, .72f), .26f * density * amount);
            Color fill = spot; fill.a = .3f * on;
            ink.FloorZone(s.TheatreSpot, r, r, fill, fill);
            Marks++;
        }
        private void TheatreBeam(MapEnvironmentState s, float t, float now, Color spot, float on, Vector2 view, float halfH)
        {
            Vector2 foot = s.TheatreSpot; float r = MapEnvironmentRules.TheatreSpotRadius;
            float top = view.y + halfH + 1.5f, height = Mathf.Max(2, top - foot.y);
            // the beam falls from the rig at the upper left (the map's one light direction) and widens onto the pool
            Vector2 source = new Vector2(foot.x - height * .22f, top), axis = foot - source;
            float length = axis.magnitude, angle = Mathf.Atan2(axis.y, axis.x) * Mathf.Rad2Deg - 90;
            Vector2 mid = Vector2.Lerp(source, foot, .5f);
            float wide = r * 2.3f, narrow = r * 1.15f;
            ink.Place(MechanicInk.Glow, mid, Mathf.Sqrt(wide * length), angle, new Color(spot.r, spot.g, spot.b, .055f * on), true, Mathf.Sqrt(length / wide));
            ink.Place(MechanicInk.Glow, Vector2.Lerp(source, foot, .62f), Mathf.Sqrt(narrow * length), angle, new Color(spot.r, spot.g, spot.b, .075f * on), true, Mathf.Sqrt(length / narrow));
            ink.Light(foot, r * 2.3f, spot, .2f * on); // what stands in the pool is lit
            // dust turning in the beam
            int motes = PresentationSettings.ReducedEffects ? 0 : 10;
            Vector2 across = new Vector2(-axis.y, axis.x).normalized;
            for (int i = 0; i < motes; i++)
            {
                float u = Mathf.Repeat(TheatreHash(i, 1) + now * (.04f + .03f * TheatreHash(i, 2)), 1);
                Vector2 at = Vector2.Lerp(foot, source, u * .55f) + across * (TheatreHash(i, 3) - .5f) * narrow * (1 - u * .4f) + Vector2.right * Mathf.Sin(now * .9f + i) * .08f;
                ink.TokenAt(InkToken.Glint, ArsenalInk.Cream, at, .09f + .05f * TheatreHash(i, 4), now * 60 + i * 37, .7f * Mathf.Sin(u * Mathf.PI) * on);
            }
            // the colour's boost floats over its pool (a star for damage, a spark for attack speed, a seal for armour)
            InkPalette pal = s.TheatreSpotColour == 0 ? ArsenalInk.Cream : s.TheatreSpotColour == 1 ? ArsenalInk.Gold : ArsenalInk.Candy;
            ink.TokenAt(TheatreSpotTokens[s.TheatreSpotColour % 3], pal, foot + new Vector2(0, r * .75f + Mathf.Sin(now * 2.2f) * .06f), .34f, Mathf.Sin(now * 1.4f) * 10, .85f * on);
            // a new colour sweeps in with a spray of glints
            float change = t - s.TheatreColourAt;
            if (change >= 0 && change < .6f && !PresentationSettings.ReducedEffects)
                for (int k = 0; k < 8; k++)
                {
                    float a = (k * 45 + 20) * Mathf.Deg2Rad, d = r * (.3f + .7f * TheatreOut(change / .6f));
                    ink.TokenAt(InkToken.Glint, pal, foot + new Vector2(Mathf.Cos(a), Mathf.Sin(a) * .6f) * d, .2f, k * 40, 1 - change / .6f);
                }
        }
        private void TheatreDrum(MapEnvironmentState s, float t)
        {
            float age = t - s.TheatreDrumAt; Vector2 pit = TheatrePit(s);
            if (s.TheatreDrumAt > theatreDrumSeen + .01f)
            {
                theatreDrumSeen = s.TheatreDrumAt;
                if (age < .5f) ImpactWords.Pop("BOOM!", pit + Vector2.up * .5f, ImpactWords.Cream, 2, 1.05f);
            }
            if (age < 0 || age >= 1.1f) return;
            // the beat's sound rolls up out of the pit over the stage
            for (int k = 0; k < 3; k++)
            {
                float q = age * 1.5f - k * .2f; if (q <= 0 || q >= 1) continue;
                float rr = .7f + q * 3.4f;
                ink.Ribbon(InkCanvas.Arc(pit, rr, rr * .5f, 35, 145), ArsenalInk.Cream, .06f, 1, 0, (1 - q) * .75f);
            }
        }

        // ------------------------------------------------------------------ the ghost audience
        private void TheatreAudience(MapEnvironmentState s, float t, float now)
        {
            bool lit = s.TheatreHeroLit;
            for (int i = 0; i < 3; i++)
            {
                Vector2 at = s.TheatreSeats + new Vector2((i - 1) * 4.2f, i == 1 ? -.25f : 0);
                if (TheatreStand("Theatre/Seats_Row", at, 1.5f) == null) TheatreSeatsInk(at);
            }
            // the first throw after the hero leaves the light is booed; the first after stepping in is cheered
            int crowd = lit ? 1 : -1;
            if (crowd != theatreCrowd)
            {
                theatreCrowd = crowd;
                if (t > 2) ImpactWords.Pop(lit ? "BRAVO!" : "BOO!", s.TheatreSeats + Vector2.up * 2.4f, lit ? ImpactWords.Gold : new Color(1f, .42f, .36f), 1, .9f);
            }
            for (int k = 0; k < 9; k++)
            {
                // the painted ghosts (requirement 1/11/2026: the ink wisps read as blobs): sitting on their row's cushions in front of the backs, cheering
                // (clapping between two poses) while the hero is in the light, cross while not, the arm up as one throws a gift
                Vector2 seat = CombatSimulation.TheatreGhost(s, k);
                string pose = !lit ? "Theatre/GhostBoo" : still || Mathf.Sin(now * 9 + k * 1.7f) > 0 ? "Theatre/GhostCheer" : "Theatre/GhostWatch";
                foreach (MapEnvironmentState.TheatreThrow th in s.TheatreThrows)
                    if (th.Ghost == k && th.Coin && t - th.Born < .35f) pose = "Theatre/GhostThrow";
                int onSeats = 1000 - Mathf.RoundToInt((seat.y - .75f + .35f) * 12) + 1;
                if (Put(pose, seat + new Vector2(0, .25f + (still ? 0 : Mathf.Sin(now * 1.7f + k * 1.3f) * .06f)), 1f, onSeats, 1, k % 2 == 1) != null) continue;
                Vector2 g = seat + new Vector2(0, .55f + Mathf.Sin(now * 1.7f + k * 1.3f) * .07f);
                float look = sim.Champion != null ? Mathf.Clamp((sim.Champion.Position.x - g.x) * .02f, -.05f, .05f) : 0;
                // a wisp of a tail, a glowing body and head (ghosts are of the enemies' violet), two ink eyes on the hero
                ink.Stroke(g + new Vector2(0, -.1f), g + new Vector2(Mathf.Sin(now * 3 + k) * .14f, -.6f), TheatreGhostInk, .22f, 1, 0, .45f, .04f, k);
                ink.Place(MechanicInk.Glow, g, .8f, 0, new Color(.68f, .62f, .96f, .55f), true, 1.25f);
                ink.Place(MechanicInk.Glow, g + new Vector2(0, .33f), .5f, 0, new Color(.93f, .91f, 1f, .85f), true);
                ink.Stroke(g + new Vector2(-.09f + look, .39f), g + new Vector2(-.09f + look, .31f), ArsenalInk.Midnight, .07f, 1, 0, .9f);
                ink.Stroke(g + new Vector2(.09f + look, .39f), g + new Vector2(.09f + look, .31f), ArsenalInk.Midnight, .07f, 1, 0, .9f);
                if (lit)
                {
                    // applause: little hands meeting and parting, a glint on each clap
                    float clap = still ? .5f : Mathf.Abs(Mathf.Sin(now * 11 + k * 1.7f));
                    ink.Place(MechanicInk.Glow, g + new Vector2(-.07f - .15f * clap, .06f), .17f, 0, new Color(1, .97f, .9f, .9f), true);
                    ink.Place(MechanicInk.Glow, g + new Vector2(.07f + .15f * clap, .06f), .17f, 0, new Color(1, .97f, .9f, .9f), true);
                    if (clap < .2f && !calm) ink.TokenAt(InkToken.Glint, ArsenalInk.Gold, g + new Vector2(0, .14f), .2f, k * 30, .9f);
                }
                else
                {
                    // scowling brows
                    ink.Stroke(g + new Vector2(-.17f + look, .47f), g + new Vector2(-.04f + look, .42f), ArsenalInk.Midnight, .045f, 1, 0, .85f);
                    ink.Stroke(g + new Vector2(.17f + look, .47f), g + new Vector2(.04f + look, .42f), ArsenalInk.Midnight, .045f, 1, 0, .85f);
                }
            }
            float flight = MapEnvironmentRules.TheatreThrowFlight;
            float reach = MapEnvironmentRules.TheatreThrowReach + (sim.Champion != null ? sim.Champion.Definition.Radius * .5f : .15f);
            foreach (MapEnvironmentState.TheatreThrow th in s.TheatreThrows)
            {
                float age = t - th.Born, f = Mathf.Clamp01(age / flight);
                if (!th.Landed && age < flight)
                {
                    // the throwing ghost's arm swings up as it lets go
                    if (age < .3f && !th.Coin && MapIdentity.Sprite("Theatre/GhostBoo") == null)
                    {
                        Vector2 g = CombatSimulation.TheatreGhost(s, th.Ghost) + new Vector2(0, .55f);
                        ink.Stroke(g + new Vector2(.12f, .05f), g + new Vector2(.32f, .5f - age), TheatreGhostInk, .1f, 1, 0, .8f);
                    }
                    float height = 1.6f + Vector2.Distance(th.From, th.To) * .12f;
                    Vector2 at = Vector2.Lerp(th.From, th.To, f) + Vector2.up * (height * 4 * f * (1 - f));
                    if (th.Coin)
                        for (int c = 0; c < th.Gold; c++)
                        {
                            Vector2 o = new Vector2((c - (th.Gold - 1) * .5f) * .2f, Mathf.Sin(c * 2.1f) * .1f);
                            ink.TokenAt(InkToken.Coin, ArsenalInk.Gold, at + o, .3f, now * 400 + c * 90, 1);
                            ink.TokenAt(InkToken.Glint, ArsenalInk.Cream, at + o - (th.To - th.From).normalized * .3f, .14f, c * 50, .7f);
                        }
                    else
                    {
                        ink.TokenAt(InkToken.Citrus, TheatreTomato, at, .36f, now * 520 + th.Ghost * 40, 1);
                        // where it will land: a solid red area that grows as it comes down (danger to the hero is red)
                        Color red = new Color(.92f, .2f, .16f, .18f + .27f * f);
                        float rr = reach * (.55f + .45f * f);
                        ink.FloorZone(th.To, rr, rr, red, red); Marks++;
                    }
                }
                else if (!th.Coin) TheatreSplat(th.To, Mathf.Clamp01((age - flight) / .8f), th.Ghost);
            }
        }
        private void TheatreSeatsInk(Vector2 at)
        {
            // a row of three velvet seats with brass arms (until Seats_Row is painted)
            Props++;
            for (int k = -1; k <= 1; k++)
            {
                Vector2 c = at + new Vector2(k * .95f, 0);
                ink.Stroke(c + new Vector2(0, .1f), c + new Vector2(0, 1.05f), TheatreVelvet, .62f, 1, 0, 1);
                ink.Stroke(c + new Vector2(-.42f, .08f), c + new Vector2(-.42f, .62f), ArsenalInk.Gold, .07f, 1, 0, 1);
            }
            ink.Stroke(at + new Vector2(1.37f, .08f), at + new Vector2(1.37f, .62f), ArsenalInk.Gold, .07f, 1, 0, 1);
        }
        private void TheatreSplat(Vector2 at, float p, int seed)
        {
            float fade = 1 - p;
            // a stain of uneven red lobes, fading
            Color stain = new Color(.6f, .12f, .1f, .45f * fade);
            for (int i = 0; i < 4; i++)
            {
                float a = (seed * 47 + i * 90 + (i * 37) % 25) * Mathf.Deg2Rad;
                ink.FloorZone(at + new Vector2(Mathf.Cos(a) * .18f, Mathf.Sin(a) * .09f), .22f + .05f * (i % 2), .13f + .03f * (i % 2), stain, stain);
            }
            // pulp and seeds thrown out
            int n = PresentationSettings.ReducedEffects ? 4 : 7;
            for (int i = 0; i < n; i++)
            {
                float a = (seed * 31 + i * 51.4f) * Mathf.Deg2Rad, d = (still ? .5f : TheatreOut(p * 1.6f)) * (.45f + .12f * (i % 3));
                Vector2 q = at + new Vector2(Mathf.Cos(a), Mathf.Sin(a) * .6f) * d + Vector2.up * (still ? 0 : .5f * Mathf.Sin(Mathf.Clamp01(p * 1.6f) * Mathf.PI) * .4f);
                ink.TokenAt(i % 3 == 0 ? InkToken.Petal : InkToken.Drop, TheatreTomato, q, .16f * (1 - .4f * p), a * 57 + p * 200, fade);
            }
        }

        // ------------------------------------------------------------------ the velvet curtain
        private void TheatreCurtain(MapEnvironmentState s, float t, float now, Vector2 view, float halfW)
        {
            float warnAt = s.TheatreCurtainWarnAt, drop = s.TheatreCurtainDropAt, up = s.TheatreCurtainUpAt;
            float y = s.TheatreCurtainY, half = MapEnvironmentRules.TheatreCurtainHalf;
            float left = view.x - halfW - 1, right = view.x + halfW + 1;
            foreach (Vector3 sw in s.TheatreSwallowed)
            {
                // a shot swallowed by the velvet: a soft wine puff
                float age = t - sw.z; if (age < 0 || age > .45f) continue;
                float p = age / .45f;
                ink.TokenAt(InkToken.Puff, TheatreVelvet, new Vector2(sw.x, sw.y) + Vector2.up * .25f * p, .3f + .25f * p, sw.x * 50, 1 - p);
            }
            if (t < warnAt || t >= up + .7f) return;
            // the warning: a solid band across the map where it will fall, filling as it comes
            if (t < drop) Band(new Vector2(left, y), new Vector2(right, y), half, (t - warnAt) / MapEnvironmentRules.TheatreCurtainWarn, ArsenalInk.Amber, .9f);
            float fall = t < drop ? Mathf.Clamp01((t - (drop - .5f)) / .5f) : 1, lift = t >= up ? Mathf.Clamp01((t - up) / .7f) : 0;
            if (fall <= 0) return;
            bool hanging = t >= drop && t < up;
            float offset = (1 - fall * fall) * 7.5f + lift * lift * 7.5f, alpha = Mathf.Clamp01(fall * 3) * (1 - lift);
            float bounce = t >= drop && t < drop + .35f && !still ? Mathf.Sin((t - drop) / .35f * Mathf.PI) * .12f : 0;
            Sprite velvet = MapIdentity.Sprite("Theatre/Curtain_Velvet");
            const float tall = 3.1f;
            float width = velvet != null ? velvet.bounds.size.x / Mathf.Max(.01f, velvet.bounds.size.y) * tall : 1.6f;
            width = Mathf.Max(.6f, width * .92f); // panels overlap a little so no gap shows
            int n = 0;
            for (float x = Mathf.Floor(left / width) * width; x <= right && n < 32; x += width, n++)
            {
                Vector2 foot = new Vector2(x + width * .5f, y - half * .45f + offset + bounce);
                float sway = still ? 0 : Mathf.Sin(now * 1.2f + x * .7f) * 1.2f;
                if (velvet == null) TheatreVelvetInk(foot, width, tall, alpha, now, n);
                else if (hanging) TheatreStand("Theatre/Curtain_Velvet", foot, tall, alpha, sway);
                else Put("Theatre/Curtain_Velvet", foot, tall, Air - 5, alpha, angle: sway); // falling or rising: over everything
            }
            // it lands with a thud and a roll of dust along its hem
            if (drop > theatreCurtainSeen + .01f && t >= drop)
            {
                theatreCurtainSeen = drop;
                if (t - drop < .5f) ImpactWords.Pop("THUD!", new Vector2(sim.Champion != null ? sim.Champion.Position.x : view.x, y + 1.2f), new Color(1f, .78f, .5f), 2, 1f);
            }
            float settle = (t - drop) / .7f;
            if (settle >= 0 && settle < 1)
            {
                int puffs = PresentationSettings.ReducedEffects ? 3 : 6;
                for (int k = 0; k < puffs; k++)
                    ink.Draw(ArsenalInk.Moment.BrickDust, new Vector2(Mathf.Lerp(left + 1.5f, right - 1.5f, (k + .5f) / puffs), y - half * .45f), Vector2.up, settle, .8f, k * 53);
            }
        }
        private void TheatreVelvetInk(Vector2 foot, float width, float tall, float alpha, float now, int n)
        {
            // velvet folds hanging from a swag, a gold fringe at the hem (until Curtain_Velvet is painted)
            Props++;
            for (int k = 0; k < 4; k++)
            {
                float x = foot.x - width * .5f + width * (k + .5f) / 4, sway = Mathf.Sin(now * 1.3f + n + k * .8f) * .06f;
                ink.Stroke(new Vector2(x, foot.y + tall), new Vector2(x + sway, foot.y), TheatreVelvet, width * .3f, 1, 0, alpha, .025f, n * 4 + k);
            }
            ink.Stroke(new Vector2(foot.x - width * .5f, foot.y + .08f), new Vector2(foot.x + width * .5f, foot.y + .08f), ArsenalInk.Gold, .09f, 1, 0, alpha);
        }

        // ------------------------------------------------------------------ the firework machine
        private void TheatreMachine(MapEnvironmentState s, float t, float now, Vector2 view, float halfW, float halfH)
        {
            if (!sim.MapSideShown(MapSide.TheatreFirework)) return; // the machine stands only on its own waves (MapPacing)
            Vector2 m = s.TheatreMachine; int full = MapEnvironmentRules.TheatreFireworkLoad;
            bool ready = s.TheatreLoad >= full;
            float reach = MapEnvironmentRules.TheatreFireworkReach;
            // ready: stand here - one brass area brightening as the fuse burns (no rim)
            if (ready) Ring(m, reach, reach, Mathf.Max(.35f, s.TheatreFuse), ArsenalInk.Gold, .6f + (calm || still ? 0 : .12f * Mathf.Sin(now * 5)));
            if (TheatreStand("Theatre/Firework_Machine", m, 1.9f, angle: ready && !still ? Mathf.Sin(now * 22) * 1.2f : 0) == null) TheatreMachineInk(m);
            Vector2 muzzle = m + new Vector2(.2f, 1.75f);
            // the load gauge: a brass tube at its side that fills with each kill in the light
            Vector2 g0 = m + new Vector2(-1.05f, .15f), g1 = g0 + Vector2.up * 1.5f;
            ink.Stroke(g0, g1, ArsenalInk.Midnight, .2f, 1, 0, .55f);
            float fill = s.TheatreLoad / (float)full;
            if (fill > 0) ink.Stroke(g0, Vector2.Lerp(g0, g1, Mathf.Max(.04f, fill)), ready ? ArsenalInk.Amber : ArsenalInk.Gold, .13f, 1, 0, .95f);
            // each kill in the light sends its spark arcing into the machine
            foreach (Vector3 load in s.TheatreLoads)
            {
                float f = (t - load.z) / .8f; if (f < 0 || f >= 1) continue;
                var path = InkCanvas.Lob(new Vector2(load.x, load.y), muzzle, 1.4f);
                float head = Mathf.Max(.03f, TheatreOut(f));
                ink.Ribbon(path, ArsenalInk.Gold, .05f, head, Mathf.Max(0, head - .3f), 1 - f * .3f);
                ink.TokenAt(InkToken.Spark, ArsenalInk.Gold, InkCanvas.Sample(path, head), .22f, now * 300, 1);
            }
            if (ready)
            {
                if (s.TheatreReadyAt > theatreReadySeen + .01f)
                {
                    theatreReadySeen = s.TheatreReadyAt;
                    if (t - s.TheatreReadyAt < 1) ImpactWords.Pop("FIREWORKS READY!", m + Vector2.up * 2.4f, ImpactWords.Gold, 2, .9f);
                }
                // its fuse fizzes and sparks
                ink.Light(muzzle, 1.3f, ArsenalInk.Amber.Glow, .3f + (calm || still ? 0 : .12f * Mathf.Sin(now * 9)));
                int sparks = PresentationSettings.ReducedEffects ? 2 : 5;
                for (int k = 0; k < sparks; k++)
                {
                    float u = still ? .4f : Mathf.Repeat(now * 2.2f + k * .21f, 1), a = (90 + (k - 2) * 24) * Mathf.Deg2Rad;
                    ink.TokenAt(InkToken.Spark, ArsenalInk.Gold, muzzle + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * u * .6f + Vector2.down * u * u * .4f, .17f * (1 - u), k * 70 + now * 200, 1 - u);
                }
                // the fuse's progress: a brass bar under it
                if (s.TheatreFuse > 0)
                {
                    Vector2 b0 = m + new Vector2(-.8f, -.45f), b1 = m + new Vector2(.8f, -.45f);
                    ink.Stroke(b0, b1, ArsenalInk.Midnight, .16f, 1, 0, .6f);
                    ink.Stroke(b0, Vector2.Lerp(b0, b1, Mathf.Max(.05f, s.TheatreFuse)), ArsenalInk.Gold, .11f, 1, 0, 1);
                }
            }
            // the grand finale: rockets climb from the machine and burst over the whole stage; every enemy is hit and frozen
            float age = t - s.TheatreFiredAt;
            if (s.TheatreFiredAt > theatreFiredSeen + .01f)
            {
                theatreFiredSeen = s.TheatreFiredAt;
                if (age < 1 && sim.Champion != null) ImpactWords.Pop("GRAND FINALE!", Head(sim.Champion) + Vector2.up * .9f, ImpactWords.Gold, 3, 1.2f);
            }
            if (age < 0 || age >= 2.6f) return;
            int shells = PresentationSettings.ReducedEffects ? 3 : 6;
            for (int k = 0; k < shells; k++)
            {
                float launch = k * .09f, burstAt = launch + .45f;
                Vector2 sky = new Vector2(view.x + (TheatreHash(k, 3) - .5f) * halfW * 1.5f, view.y + halfH * (.2f + .45f * TheatreHash(k, 4)));
                if (age >= launch && age < burstAt)
                {
                    float f = TheatreOut((age - launch) / .45f);
                    ink.Ribbon(InkCanvas.Lob(muzzle, sky, 1.2f), ArsenalInk.Gold, .045f, Mathf.Max(.03f, f), Mathf.Max(0, f - .25f), .9f);
                }
                float b = (age - burstAt) / 1.6f;
                if (b >= 0 && b < 1) TheatreShell(sky, b, k);
            }
            int shown = 0;
            foreach (UnitState u in sim.Units)
            {
                if (shown >= 24) break;
                if (!u.Alive || u.Faction != Faction.Enemy || Mathf.Abs(u.Position.x - view.x) > halfW + 1 || Mathf.Abs(u.Position.y - view.y) > halfH + 1) continue;
                shown++;
                Vector2 head = Head(u);
                if (age >= .45f && age < .9f) ink.Draw(ArsenalInk.Moment.Spark, head, Vector2.up, (age - .45f) / .45f, .5f, u.Id * 31);
                if (u.FrozenUntil > sim.Elapsed) ink.TokenAt(InkToken.Shard, ArsenalInk.Glass, head + Vector2.up * .18f, .24f, u.Id * 40, .9f);
            }
        }
        private void TheatreShell(Vector2 c, float p, int k)
        {
            // a chrysanthemum shell: painted streaks thrown out, sagging as they burn, glints at their tips
            InkPalette pal = k % 3 == 0 ? ArsenalInk.Gold : k % 3 == 1 ? ArsenalInk.Candy : ArsenalInk.Cyan;
            float r = (still ? .8f : TheatreOut(p)) * (1.6f + .4f * (k % 2)), fade = 1 - p * p;
            Vector2 sag = Vector2.down * (still ? 0 : .7f * p * p);
            ink.Light(c, r * 2.6f, pal.Glow, .45f * (1 - p));
            int rays = PresentationSettings.ReducedEffects ? 8 : 14;
            for (int i = 0; i < rays; i++)
            {
                float a = (i * 360f / rays + k * 17) * Mathf.Deg2Rad; Vector2 d = new Vector2(Mathf.Cos(a), Mathf.Sin(a)), tip = c + d * r + sag;
                ink.Stroke(c + d * r * .45f + sag * .6f, tip, pal, .05f, 1, 0, .9f * fade);
                ink.TokenAt(i % 2 == 0 ? InkToken.Glint : InkToken.Star, pal, tip, .16f, a * 57 + p * 200, fade);
            }
        }
        private void TheatreMachineInk(Vector2 m)
        {
            // a crate with a brass mortar on it (until Firework_Machine is painted)
            Props++;
            ink.Stroke(m + new Vector2(-.5f, .3f), m + new Vector2(.5f, .3f), ArsenalInk.Leather, .55f, 1, 0, 1);
            ink.Stroke(m + new Vector2(-.1f, .55f), m + new Vector2(.2f, 1.7f), ArsenalInk.Gold, .3f, 1, 0, 1);
            ink.Stroke(m + new Vector2(-.45f, .62f), m + new Vector2(.45f, .62f), ArsenalInk.Wine, .08f, 1, 0, 1);
        }

        // ------------------------------------------------------------------ the encore
        private void TheatreEncore(MapEnvironmentState s, float t, float now)
        {
            UnitState hero = sim.Champion; if (hero == null) return;
            Vector2 head = Head(hero) + Vector2.up * .6f;
            // three stars over the hero fill, one a second, while it stays in the light (not during the encore's cooldown)
            if (s.TheatreHeroLit && t >= s.TheatreEncoreReady && s.TheatreLitSince > -50)
            {
                float held = (t - s.TheatreLitSince) / MapEnvironmentRules.TheatreEncoreHold;
                for (int k = 0; k < 3; k++)
                {
                    float fill = Mathf.Clamp01(held * 3 - k);
                    ink.TokenAt(InkToken.Star, fill >= 1 ? ArsenalInk.Gold : ArsenalInk.Midnight, head + new Vector2((k - 1) * .3f, k == 1 ? .1f : 0), .2f + .08f * fill, 0, .45f + .55f * fill);
                }
            }
            float age = t - s.TheatreEncoreAt;
            if (s.TheatreEncoreAt > theatreEncoreSeen + .01f)
            {
                theatreEncoreSeen = s.TheatreEncoreAt;
                if (age < 1) ImpactWords.Pop("ENCORE!", head + Vector2.up * .3f, ImpactWords.Candy, 3, 1.1f);
            }
            if (age < 0 || age >= 1.4f) return;
            // roses rain on the hero from the house
            int n = PresentationSettings.ReducedEffects ? 4 : 9;
            for (int k = 0; k < n; k++)
            {
                float f = Mathf.Clamp01((age - k * .06f) / .9f); if (f <= 0 || f >= 1) continue;
                Vector2 at = hero.Position + new Vector2((TheatreHash(k, 8) - .5f) * 1.6f + Mathf.Sin(now * 3 + k) * .1f, .3f + 2.6f * (1 - f));
                ink.TokenAt(k % 3 == 0 ? InkToken.Star : InkToken.Petal, k % 3 == 0 ? ArsenalInk.Gold : ArsenalInk.Candy, at, .22f, now * 160 + k * 50, 1 - f * f);
            }
        }
    }

    public sealed partial class MapAtmosphere
    {
        /// <summary>The Theatre's air: a dark velvet house, the spotlight's spill on the haze, breathing footlights along the seats,
        /// dust turning in the beam.</summary>
        private void AirTheatre(MapEnvironmentState s, int motes, float clock)
        {
            Grade(new Color(.16f, .05f, .13f, .3f), new Color(.03f, .01f, .05f, .74f));
            Color spot = MapEnvironmentVisual.TheatreSpotColour(s, clock);
            Put(MechanicInk.Glow, s.TheatreSpot, Vector2.one * MapEnvironmentRules.TheatreSpotRadius * 4.2f, 0, new Color(spot.r, spot.g, spot.b, .1f), HazeOrder);
            for (int i = -4; i <= 4; i++)
            {
                float breathe = calm || still ? 1 : .8f + .2f * Mathf.Sin(t * 1.3f + i * 1.9f);
                Put(MechanicInk.Glow, s.TheatreSeats + new Vector2(i * 1.5f, 1.45f), new Vector2(1.6f, .55f), 0, new Color(1f, .7f, .38f, .16f * breathe), HazeOrder);
            }
            for (int i = 0; i < motes; i++)
            {
                bool beam = i % 2 == 0;
                Vector2 at;
                if (beam)
                {
                    // the beam leans in from the upper left: its dust drifts up along it
                    float u = Mathf.Repeat(H(i, 1) + t * (.03f + .02f * H(i, 2)), 1);
                    at = s.TheatreSpot + new Vector2((H(i, 3) - .5f) * 2.6f - u * 1f, u * 4.5f);
                    at.x += still ? 0 : Mathf.Sin(t * .8f + i) * .15f;
                }
                else at = Wrap(new Vector2(H(i, 4) * 40, H(i, 5) * 40), new Vector2(.05f, .04f + .04f * H(i, 6)));
                Put(MechanicInk.Glow, at, Vector2.one * (.07f + .05f * H(i, 7)), 0, beam ? new Color(spot.r, spot.g, spot.b, .55f) : new Color(.85f, .7f, .8f, .25f), AirOrder);
            }
        }
    }
}
