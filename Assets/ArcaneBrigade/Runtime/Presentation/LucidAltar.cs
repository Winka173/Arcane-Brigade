using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The lucid altar in the dream shop (requirement 22/10/2026; rules in LucidRite): its card picks which 3-star friend
    /// wakes, shows what it gains and the price in eggs and gold. Waking plays the rite: a column of light rises over the friend,
    /// motes spiral in, rays burst, and an ink crown draws itself on while LUCID and the friend's name show. Every lucid friend
    /// then wears that crown and slow orbiting sparks, in the shop and in battle. All drawn in code (ink strokes and light).
    /// Reduced Motion: no spiral or orbit, the crown appears whole.</summary>
    public sealed class LucidAltar : MonoBehaviour
    {
        private ArenaController owner;
        private bool shown;
        private int pick;
        private float openedAt = -10, riteAt = -100;
        private string riteUnit;
        private Vector2 riteFallback;
        private bool chimed;
        private InkCanvas air;
        private GUIStyle title, hand, line, small, button, banner;
        private readonly HashSet<string> lucidIds = new HashSet<string>();
        private const float RiteTime = 2.8f;
        private static readonly Color Gold = new Color(1f, .84f, .42f), Cream = new Color(1f, .96f, .84f), Violet = new Color(.72f, .52f, 1f);

        public int InputConsumedFrame { get; private set; } = -1;
        public bool Open => shown && owner?.Session != null && !owner.StoryConsumesInput && !owner.TransitionPending;
        public bool Performing => Time.unscaledTime - riteAt < RiteTime;
        public void Configure(ArenaController value) => owner = value;
        public void Show(Vector2 altar)
        {
            shown = true; pick = 0; riteFallback = altar; openedAt = Time.unscaledTime; InputConsumedFrame = Time.frameCount;
            CheckpointAudio.PlayInterfaceId("ui.open");
        }
        private void Close() { shown = false; InputConsumedFrame = Time.frameCount; }

        private void Update()
        {
            if (Open && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1))) Close();
            if (Performing && !chimed && Time.unscaledTime - riteAt > 1.2f) { chimed = true; CheckpointAudio.PlayInterfaceId("ui.unlock"); }
        }

        // ------------------------------------------------------------------ world: the crowns and the rite
        private void LateUpdate()
        {
            if (owner == null || owner.Session == null) return;
            air ??= new InkCanvas(transform, "Lucid light", 2058);
            air.Clear();
            lucidIds.Clear();
            foreach (RosterEntry entry in owner.Session.State.Roster) if (entry.Lucid) lucidIds.Add(entry.UnitId);
            float now = Time.unscaledTime; bool still = PresentationSettings.ReducedMotion;
            Bounds? target = null;
            foreach (UnitVisual view in owner.Views)
            {
                if (view == null || view.State == null || !view.State.Alive || view.State.Faction != Faction.Companion || view.BodyRenderer == null || !view.BodyRenderer.enabled) continue;
                string id = view.State.Definition != null ? view.State.Definition.Id : null;
                if (id == null) continue;
                if (Performing && id == riteUnit) target = view.BodyRenderer.bounds;
                else if (lucidIds.Contains(id)) Crown(view.BodyRenderer.bounds, now, 1, still);
            }
            if (Performing) Rite(target ?? new Bounds(riteFallback + Vector2.up * .6f, new Vector3(.9f, 1.2f, 1)), now - riteAt, still);
            air.Commit();
        }

        private static InkStroke Brush(float width, float alpha) => new InkStroke { Width = width, Head = 1, Tail = 1, Taper = .6f, Alpha = alpha, Ink = new Color(.42f, .26f, .08f), Body = Gold, Light = Cream };

        /// <summary>A lucid friend's mark: a brushed ink crown over its head, a soft light along it, and sparks slowly orbiting.</summary>
        private void Crown(Bounds body, float now, float reveal, bool still)
        {
            float w = Mathf.Clamp(body.size.x, .45f, 1.6f);
            Vector2 head = new Vector2(body.center.x, body.max.y + .08f + (still ? 0 : Mathf.Sin(now * 2.2f) * .025f));
            List<Vector2> crown = InkCanvas.Line(head + Vector2.left * w * .2f, head + Vector2.right * w * .2f, w * .07f, 3, true, 1);
            if (reveal < 1) { int keep = Mathf.Max(2, Mathf.RoundToInt(crown.Count * Mathf.Clamp01(reveal))); crown.RemoveRange(keep, crown.Count - keep); }
            air.GlowRibbon(crown, 1, 1, w * .16f, new Color(Gold.r, Gold.g, Gold.b, .32f));
            air.Stroke(crown, Brush(.03f, .95f));
            if (still || reveal < 1) return;
            air.Ticks(body.center, body.extents.magnitude * .95f, 5, .07f, .018f, now * 40, new Color(Violet.r, Violet.g, Violet.b, .7f));
        }

        /// <summary>The rite: a column of light rises, motes spiral in, rays burst at 1.2s, the crown draws itself on.</summary>
        private void Rite(Bounds body, float t, bool still)
        {
            Vector2 feet = new Vector2(body.center.x, body.min.y), top = feet + Vector2.up * (body.size.y + 2.2f);
            float rise = Mathf.Clamp01(t / .6f), fade = 1 - Mathf.Clamp01((t - 2.2f) / .6f);
            air.Band(feet, Vector2.Lerp(feet, top, rise), .42f + .1f * Mathf.Sin(t * 9), new Color(Cream.r, Cream.g, Cream.b, .5f * fade), new Color(Gold.r, Gold.g, Gold.b, 0));
            air.Band(feet, Vector2.Lerp(feet, top, rise), .16f, new Color(1, 1, 1, .7f * fade), new Color(Cream.r, Cream.g, Cream.b, 0));
            if (!still && t > .2f && t < 1.3f)
            {
                float k = (t - .2f) / 1.1f;
                air.Ticks(body.center, Mathf.Lerp(2.2f, .4f, k), 10, .14f, .03f, t * 260, new Color(Violet.r, Violet.g, Violet.b, .85f));
                air.Ticks(body.center, Mathf.Lerp(1.6f, .3f, k), 7, .1f, .025f, -t * 200, new Color(Gold.r, Gold.g, Gold.b, .85f));
            }
            if (t > 1.2f && t < 1.9f)
            {
                float k = (t - 1.2f) / .7f;
                air.Ticks(body.center, .5f + k * 2.4f, 14, .5f * (1 - k) + .1f, .05f * (1 - k) + .01f, 0, new Color(Cream.r, Cream.g, Cream.b, 1 - k));
            }
            if (t > 1.3f) Crown(body, Time.unscaledTime, still ? 1 : Mathf.Clamp01((t - 1.3f) / .6f), still);
        }

        private void StartRite(string unitId)
        {
            riteUnit = unitId; riteAt = Time.unscaledTime; chimed = false;
            CheckpointAudio.PlayInterfaceId("shop.fusion");
        }

        // ------------------------------------------------------------------ screen: the card and the rite's title
        private void Styles()
        {
            if (title != null) return;
            title = ArcadeTypography.Style(36, true); title.alignment = TextAnchor.MiddleLeft; title.wordWrap = false;
            hand = ArcadeTypography.Style(26); hand.font = ArcadeTypography.Hand; hand.alignment = TextAnchor.MiddleLeft; hand.wordWrap = true;
            line = ArcadeTypography.Style(22, true); line.alignment = TextAnchor.MiddleCenter; line.wordWrap = true;
            small = ArcadeTypography.Style(18, true); small.alignment = TextAnchor.MiddleCenter; small.wordWrap = false; small.clipping = TextClipping.Clip;
            button = ArcadeTypography.Style(28, true); button.alignment = TextAnchor.MiddleCenter;
            banner = ArcadeTypography.Style(30, true); banner.alignment = TextAnchor.MiddleCenter;
        }

        private void OnGUI()
        {
            if (owner == null || owner.Session == null || Camera.main == null || owner.PresentationPaused) return;
            if (!Open && !Performing) return;
            GUI.depth = -60;
            Matrix4x4 previous = ArcadeTypography.BeginViewport(Camera.main.pixelRect, 1920, 1080);
            Styles();
            if (Performing) DrawRiteTitle(Time.unscaledTime - riteAt);
            else DrawCard();
            GUI.matrix = previous;
        }

        private void DrawRiteTitle(float t)
        {
            float dim = Mathf.Clamp01(t / .3f) * (1 - Mathf.Clamp01((t - 2.3f) / .5f));
            RunHud.Fill(new Rect(0, 0, 1920, 1080), new Color(.04f, .02f, .08f, .35f * dim));
            if (t < 1.3f) return;
            Color keep = GUI.color;
            GUI.color = new Color(keep.r, keep.g, keep.b, keep.a * Mathf.Clamp01((t - 1.3f) / .25f) * (1 - Mathf.Clamp01((t - 2.4f) / .4f)));
            ArcadeSkin.TitleBanner(new Rect(660, 130, 600, 70), "LUCID", banner);
            UnitDefinition unit = owner.Content.Unit(riteUnit);
            line.normal.textColor = Cream;
            GUI.Label(new Rect(560, 212, 800, 44), (unit != null ? RunUICopy.UnitName(unit) : "") + " wakes past its dream", line);
            GUI.color = keep;
        }

        private void DrawCard()
        {
            RunSession session = owner.Session; RunSnapshot state = session.State;
            List<int> candidates = session.LucidCandidates();
            pick = Mathf.Clamp(pick, 0, Mathf.Max(0, candidates.Count - 1));
            float fade = Mathf.Clamp01((Time.unscaledTime - openedAt) / .2f);
            RunHud.Fill(new Rect(0, 0, 1920, 1080), new Color(0, 0, 0, .5f * fade));
            Color tint = GUI.color; GUI.color = new Color(tint.r, tint.g, tint.b, tint.a * fade);
            Rect card = new Rect(410, 230, 1100, 620);
            CardPop.Apply("lucid-altar", card); // the colour is restored with tint below
            ArcadeSkin.Ticket(card, ArcadeSkin.Paper, ArcadeSkin.Wax);
            Rect inner = ArcadeSkin.Content(card, KitFrame.Card);
            Color brown = new Color(.25f, .16f, .1f);
            title.normal.textColor = brown;
            GUI.Label(new Rect(inner.x, inner.y, inner.width, 48), "LUCID RITE", title);
            hand.normal.textColor = new Color(.30f, .22f, .16f, .8f);
            GUI.Label(new Rect(inner.x, inner.y + 50, inner.width, 40), "A 3-star friend wakes past its dream. " + (LucidLeft(state) == 1 ? "One rite left." : "Two rites a run."), hand);

            // the friends who can wake: their pictures, one picked
            const float tileW = 150, tileH = 196, gap = 14;
            int shownCount = Mathf.Min(6, candidates.Count);
            float rowW = shownCount * tileW + Mathf.Max(0, shownCount - 1) * gap, rowX = inner.center.x - rowW * .5f, rowY = inner.y + 104;
            for (int i = 0; i < shownCount; i++)
            {
                RosterEntry entry = state.Roster[candidates[i]];
                UnitDefinition unit = owner.Content.Unit(entry.UnitId);
                Rect tile = new Rect(rowX + i * (tileW + gap), rowY, tileW, tileH);
                if (i == pick) ArcadeSkin.Glow(new Rect(tile.x - 12, tile.y - 10, tile.width + 24, tile.height + 20), new Color(Gold.r, Gold.g, Gold.b, .55f));
                RunHud.Fill(tile, i == pick ? ArcadeSkin.CardLit : ArcadeSkin.Card);
                ShopDescriptionCard.DrawSprite(new Rect(tile.x + 16, tile.y + 12, tileW - 32, 120), unit != null ? unit.Body : null);
                small.normal.textColor = ArcadeSkin.Cream;
                GUI.Label(new Rect(tile.x + 8, tile.y + 136, tileW - 16, 26), unit != null ? RunUICopy.UnitName(unit) : entry.UnitId, small);
                small.normal.textColor = Gold;
                GUI.Label(new Rect(tile.x + 8, tile.y + 162, tileW - 16, 24), "★★★", small);
                if (GUI.Button(tile, GUIContent.none, GUIStyle.none)) { pick = i; CheckpointAudio.PlayInterfaceId("ui.select"); }
            }
            if (shownCount == 0) { line.normal.textColor = brown; GUI.Label(new Rect(inner.x, rowY + 60, inner.width, 60), "No 3-star friend is ready to wake.", line); }

            // what it gains, then the price with the HUD's own egg and coin
            line.normal.textColor = new Color(.14f, .40f, .25f);
            GUI.Label(new Rect(inner.x, rowY + tileH + 14, inner.width, 34), "+30% damage  ·  bigger  ·  weapon grows  ·  +1 in its bond", line);
            float priceY = rowY + tileH + 58;
            bool eggsOk = state.Eggs >= session.LucidEggCost, goldOk = state.Gold >= session.LucidGoldCost;
            Rect price = new Rect(inner.center.x - 170, priceY, 340, 40);
            RunHud.Sprite(new Rect(price.x, price.y, 40, 40), CurrencyIcons.Egg);
            line.normal.textColor = eggsOk ? brown : new Color(.7f, .15f, .12f); line.alignment = TextAnchor.MiddleLeft;
            GUI.Label(new Rect(price.x + 48, price.y, 110, 40), session.LucidEggCost.ToString(), line);
            RunHud.Sprite(new Rect(price.x + 170, price.y, 40, 40), CurrencyIcons.Coin);
            line.normal.textColor = goldOk ? brown : new Color(.7f, .15f, .12f);
            GUI.Label(new Rect(price.x + 218, price.y, 120, 40), session.LucidGoldCost.ToString(), line);
            line.alignment = TextAnchor.MiddleCenter;

            // AWAKEN and LATER, one size, apart
            float buttonsY = inner.yMax - 92;
            Rect awaken = new Rect(inner.center.x - 340, buttonsY, 320, 88), later = new Rect(inner.center.x + 40, buttonsY, 300, 88);
            bool can = shownCount > 0 && session.CanLucid(candidates[pick]);
            if (ArcadeSkin.Button(awaken, "AWAKEN", KitButton.Primary, can, button))
            {
                string unitId = state.Roster[candidates[pick]].UnitId;
                if (session.Lucid(candidates[pick])) { Close(); StartRite(unitId); }
            }
            if (ArcadeSkin.Button(later, "LATER", KitButton.Secondary, true, button)) Close();
            if (Event.current.type == EventType.MouseDown && !card.Contains(Event.current.mousePosition)) { Close(); Event.current.Use(); }
            GUI.color = tint;
        }
        private static int LucidLeft(RunSnapshot state) => RunSession.LucidMax - state.LucidRites;
    }
}
