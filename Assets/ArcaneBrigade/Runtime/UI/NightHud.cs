using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>HUD pieces that only make sense in Miền Chờ: the road down through the night to dawn (with the journal's
    /// letters along it), red eyes at the screen edge for what is still coming out of the dark, and the ring of small
    /// flames around the hero — one for every companion sharing their light.</summary>
    public static class NightHud
    {
        private static GUIStyle caption;
        private static GUIStyle count;

        /// <summary>A path of one notch per wave from dusk to dawn. Lit notches are behind you; letters mark chapter waves.</summary>
        public static void DawnPath(Rect area, RunSnapshot state, int normalWaves, bool combat)
        {
            if (caption == null)
            {
                caption = ArcadeTypography.Style(12, false, true); caption.alignment = TextAnchor.MiddleCenter; caption.wordWrap = false;
            }
            normalWaves = Mathf.Max(1, normalWaves);
            float y = area.center.y;
            Color amber = ArcadeSkin.Accent;
            bool endless = state.Endless || state.Wave > normalWaves;
            float left = area.x, right = area.xMax - (endless ? 0 : 26);
            ArcadeSkin.Line(new Rect(left, y - 1, right - left, 2), new Color(ArcadeSkin.Cream.r, ArcadeSkin.Cream.g, ArcadeSkin.Cream.b, 0.16f));
            int current = Mathf.Clamp(state.Wave, 1, normalWaves);
            float step = (right - left) / Mathf.Max(1, normalWaves - 1);
            float done = (current - 1 + (combat ? 0f : 1f)) / Mathf.Max(1, normalWaves - 1);
            ArcadeSkin.Line(new Rect(left, y - 1, (right - left) * Mathf.Clamp01(endless ? 1 : done), 2), new Color(amber.r, amber.g, amber.b, 0.75f));
            int opened = 0;
            try { opened = MienChoStory.Available(state); } catch (System.Exception) { opened = 0; }
            for (int wave = 1; wave <= normalWaves; wave++)
            {
                float x = left + (wave - 1) * step;
                bool passed = wave < current || wave == current && !combat || endless;
                bool here = wave == current && !endless;
                float size = here ? 7 : 4;
                ArcadeSkin.Slant(new Rect(x - size * 0.5f, y - size * 0.5f, size, size), passed || here ? amber : new Color(ArcadeSkin.Cream.r, ArcadeSkin.Cream.g, ArcadeSkin.Cream.b, 0.28f));
                if (here)
                {
                    float breathe = PresentationSettings.ReducedMotion ? 0.7f : 0.6f + 0.4f * Mathf.Sin(Time.unscaledTime * 2.4f);
                    ArcadeSkin.Glow(new Rect(x - 16, y - 16, 32, 32), new Color(ArcadeSkin.Gold.r, ArcadeSkin.Gold.g, ArcadeSkin.Gold.b, 0.6f * breathe));
                }
            }
            // the journal's chapters wait along the road as small letters; opened ones are lit
            int index = 0;
            foreach (WaitingChapter chapter in MienChoStory.Chapters)
            {
                if (chapter.Wave < 1 || chapter.Wave > normalWaves) { index++; continue; }
                float x = left + (chapter.Wave - 1) * step;
                bool open = index < opened;
                Rect letter = new Rect(x - 5, y - 15, 10, 7);
                ArcadeSkin.Line(letter, open ? ArcadeSkin.Paper : new Color(ArcadeSkin.Paper.r, ArcadeSkin.Paper.g, ArcadeSkin.Paper.b, 0.25f));
                ArcadeSkin.Line(new Rect(letter.x, letter.y, letter.width, 1), open ? amber : new Color(0, 0, 0, 0.3f));
                index++;
            }
            if (!endless)
            {
                // dawn waits at the end of the road
                ArcadeSkin.Glow(new Rect(area.xMax - 30, y - 22, 44, 44), new Color(ArcadeSkin.Dawn.r, ArcadeSkin.Dawn.g, ArcadeSkin.Dawn.b, 0.55f));
                ArcadeSkin.Slant(new Rect(area.xMax - 14, y - 6, 12, 12), ArcadeSkin.Dawn);
            }
            int remaining = normalWaves - current + (combat ? 1 : 0);
            string text = endless ? "THE NIGHT DOES NOT END" : remaining <= 1 ? (combat ? "LAST WAVE BEFORE DAWN" : "DAWN IS NEXT") : "DAWN IN " + remaining + " WAVES";
            caption.normal.textColor = new Color(ArcadeSkin.Cream.r, ArcadeSkin.Cream.g, ArcadeSkin.Cream.b, 0.75f);
            GUI.Label(new Rect(area.x, y + 8, area.width, 16), text, caption);
        }

        private const int Buckets = 28;
        private static readonly float[] nearest = new float[Buckets];
        private static readonly UnitState[] nearestUnit = new UnitState[Buckets];

        /// <summary>Hostile things outside the view show as a pair of red eyes on the screen edge, nearest per direction.</summary>
        public static void EdgeEyes(CombatSimulation simulation, Camera camera, Vector2 design)
        {
            if (Event.current.type != EventType.Repaint || simulation == null || camera == null || !simulation.Champion.Alive) return;
            for (int k = 0; k < Buckets; k++) { nearest[k] = float.MaxValue; nearestUnit[k] = null; }
            Vector2 hero = simulation.Champion.Position;
            const float range = 24f;
            foreach (UnitState unit in simulation.Units)
            {
                if (unit == null || !unit.Alive || unit.Faction != Faction.Enemy) continue;
                Vector3 viewport = camera.WorldToViewportPoint(unit.Position);
                if (viewport.x > -0.02f && viewport.x < 1.02f && viewport.y > -0.02f && viewport.y < 1.02f) continue; // visible: the world shows it
                float distance = Vector2.Distance(hero, unit.Position);
                if (distance > range) continue;
                Vector2 direction = new Vector2(viewport.x - 0.5f, viewport.y - 0.5f);
                int bucket = Mathf.FloorToInt(Mathf.Repeat(Mathf.Atan2(direction.y, direction.x) / (Mathf.PI * 2f), 1f) * Buckets) % Buckets;
                if (distance < nearest[bucket]) { nearest[bucket] = distance; nearestUnit[bucket] = unit; }
            }
            Vector2 centre = design * 0.5f;
            float inset = 34;
            for (int bucket = 0; bucket < Buckets; bucket++)
            {
                UnitState unit = nearestUnit[bucket];
                if (unit == null) continue;
                Vector3 viewport = camera.WorldToViewportPoint(unit.Position);
                Vector2 direction = new Vector2(viewport.x - 0.5f, 0.5f - viewport.y); // GUI y grows downward
                if (direction.sqrMagnitude < 1e-6f) continue;
                Vector2 scaled = new Vector2(direction.x * design.x, direction.y * design.y);
                float t = Mathf.Min((centre.x - inset) / Mathf.Max(1e-4f, Mathf.Abs(scaled.x)), (centre.y - inset) / Mathf.Max(1e-4f, Mathf.Abs(scaled.y)));
                Vector2 point = centre + scaled * t;
                float closeness = 1f - Mathf.Clamp01(nearest[bucket] / range);
                bool boss = unit.Definition != null && unit.Definition.IsBoss;
                float alpha = Mathf.Lerp(0.25f, 1f, closeness) * (PresentationSettings.HighReadability ? 1f : 0.9f);
                float size = boss ? 9 : 5 + closeness * 2;
                float gap = boss ? 13 : 8;
                // eyes blink now and then, each direction on its own rhythm
                bool blink = !PresentationSettings.ReducedMotion && Mathf.Repeat(Time.unscaledTime * 0.27f + bucket * 0.37f, 1f) > 0.965f;
                Vector2 across = new Vector2(-scaled.normalized.y, scaled.normalized.x);
                if (Mathf.Abs(across.x) < 0.5f) across = new Vector2(1, 0); // keep eyes side by side horizontally on left/right edges
                ArcadeSkin.Glow(new Rect(point.x - size * 3, point.y - size * 2.2f, size * 6, size * 4.4f), new Color(ArcadeSkin.Danger.r, ArcadeSkin.Danger.g, ArcadeSkin.Danger.b, 0.35f * alpha));
                for (int eye = -1; eye <= 1; eye += 2)
                {
                    Vector2 at = point + new Vector2(gap * 0.5f * eye, 0);
                    if (blink) ArcadeSkin.Line(new Rect(at.x - size * 0.6f, at.y, size * 1.2f, 1.5f), new Color(1f, 0.4f, 0.3f, alpha));
                    else
                    {
                        ArcadeSkin.Slant(new Rect(at.x - size * 0.5f, at.y - size * 0.5f, size, size), new Color(ArcadeSkin.Danger.r, ArcadeSkin.Danger.g, ArcadeSkin.Danger.b, alpha));
                        ArcadeSkin.Slant(new Rect(at.x - size * 0.18f, at.y - size * 0.18f, size * 0.36f, size * 0.36f), new Color(1f, 0.85f, 0.7f, alpha));
                    }
                }
            }
        }

        /// <summary>Small flames around the hero portrait, one per companion sharing their light (15 shown, the rest counted).</summary>
        public static void LightRing(Vector2 centre, float radius, int companions)
        {
            if (count == null) { count = ArcadeTypography.Style(12, false, true); count.alignment = TextAnchor.MiddleCenter; count.wordWrap = false; }
            int shown = Mathf.Min(companions, 15);
            float time = Time.unscaledTime;
            for (int index = 0; index < shown; index++)
            {
                float angle = Mathf.Lerp(200f, 340f, shown == 1 ? 0.5f : index / (float)(shown - 1)) * Mathf.Deg2Rad; // an arc under the portrait
                Vector2 at = centre + new Vector2(Mathf.Cos(angle), -Mathf.Sin(angle)) * radius;
                float flicker = PresentationSettings.ReducedMotion ? 0.8f : 0.7f + 0.3f * Mathf.Sin(time * 5.3f + index * 1.9f) * Mathf.Sin(time * 2.1f + index);
                ArcadeSkin.Glow(new Rect(at.x - 7, at.y - 7, 14, 14), new Color(ArcadeSkin.Gold.r, ArcadeSkin.Gold.g, ArcadeSkin.Gold.b, 0.55f * flicker));
                ArcadeSkin.Slant(new Rect(at.x - 1.5f, at.y - 2.5f, 3, 5), new Color(1f, 0.92f, 0.75f, 0.9f));
            }
            if (companions > shown)
            {
                count.normal.textColor = ArcadeSkin.Gold;
                GUI.Label(new Rect(centre.x + radius - 4, centre.y + radius * 0.35f, 40, 16), "+" + (companions - shown), count);
            }
        }
    }
}
