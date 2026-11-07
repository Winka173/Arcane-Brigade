using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>HUD pieces that only make sense in Miền Chờ: the eight lanterns of the Thursday visitor, red eyes at the
    /// screen edge for what is still coming out of the dark, and the ring of small flames around the hero — one for every
    /// companion sharing their light.</summary>
    public static class NightHud
    {
        private static GUIStyle count;
        private static GUIStyle loss;
        private static readonly Sprite[] lanterns = new Sprite[3];

        /// <summary>SIT CLOSER: someone is knocking at the door of the night you hid; the fight is lost when it stops.</summary>
        public static void Knocking(Rect area, float remaining)
        {
            if (remaining < 0) return;
            if (loss == null) { loss = ArcadeTypography.Style(34); loss.font = ArcadeTypography.Hand; loss.alignment = TextAnchor.MiddleCenter; loss.wordWrap = true; }
            float beat = PresentationSettings.ReducedMotion ? 1 : 0.75f + 0.25f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * Mathf.PI * 0.8f));
            loss.normal.textColor = remaining < 15 ? new Color(ArcadeSkin.Danger.r, ArcadeSkin.Danger.g, ArcadeSkin.Danger.b, beat) : new Color(ArcadeSkin.Paper.r, ArcadeSkin.Paper.g, ArcadeSkin.Paper.b, beat);
            LocalizedGUI.Label(area, "someone is knocking · answer before it stops · " + Mathf.CeilToInt(remaining) + "s", loss);
        }

        /// <summary>When a lantern goes out, the thing it held is written across the night for a few seconds.</summary>
        public static void LampLoss(Rect area)
        {
            int lamp = NightLanternField.LastLoss;
            float age = Time.unscaledTime - NightLanternField.LastLossAt;
            if (lamp < 0 || age > 7f) return;
            if (loss == null) { loss = ArcadeTypography.Style(34); loss.font = ArcadeTypography.Hand; loss.alignment = TextAnchor.MiddleCenter; loss.wordWrap = true; }
            float alpha = Mathf.Clamp01(age / 0.8f) * Mathf.Clamp01((7f - age) / 1.6f);
            Color was = GUI.color;
            GUI.color = new Color(was.r, was.g, was.b, was.a * alpha);
            loss.normal.textColor = new Color(0, 0, 0, 0.7f);
            LocalizedGUI.Label(new Rect(area.x + 2, area.y + 2, area.width, area.height), ActOneCanon.LampLoss(lamp), loss);
            loss.normal.textColor = ArcadeSkin.Paper;
            LocalizedGUI.Label(area, ActOneCanon.LampLoss(lamp), loss);
            GUI.color = was;
        }

        /// <summary>The eight lanterns of the Thursday visitor as a small row under the wallet: lit, flickering or dark.
        /// No text and no wave road, so it never competes with the fight.</summary>
        public static void Lanterns(Rect area, RunSnapshot state)
        {
            if (lanterns[0] == null) for (int i = 0; i < 3; i++) lanterns[i] = Resources.Load<Sprite>("MienCho/Lanterns/" + new[] { "Intact", "Cracked", "Broken" }[i]);
            bool endless = state.Endless || state.Wave > ActOneCanon.Waves;
            int relit = state.ActOne?.RelitMask ?? 0;
            ArcadeSkin.Plate(area, new Color(ArcadeSkin.Card.r, ArcadeSkin.Card.g, ArcadeSkin.Card.b, 0.85f));
            float step = (area.width - 16) / ActOneCanon.BreakWave.Length;
            for (int lamp = 0; lamp < ActOneCanon.BreakWave.Length; lamp++)
            {
                int stateOf = endless ? ((relit & (1 << lamp)) != 0 || lamp == 7 ? 0 : 2) : ActOneCanon.LampState(lamp, Mathf.Max(1, state.Wave), relit);
                float x = area.x + 8 + step * (lamp + 0.5f);
                Rect icon = new Rect(x - 8, area.y + 4, 16, area.height - 8);
                if (stateOf == 0) ArcadeSkin.Glow(new Rect(x - 13, area.y + 2, 26, area.height - 4), new Color(ArcadeSkin.Gold.r, ArcadeSkin.Gold.g, ArcadeSkin.Gold.b, 0.45f));
                if (stateOf == 1 && !PresentationSettings.ReducedFlashes) ArcadeSkin.Glow(new Rect(x - 11, area.y + 4, 22, area.height - 8), new Color(ArcadeSkin.Gold.r, ArcadeSkin.Gold.g, ArcadeSkin.Gold.b, 0.18f + 0.14f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 3f + lamp))));
                Color was = GUI.color;
                if (stateOf == 2) GUI.color = new Color(was.r * 0.55f, was.g * 0.55f, was.b * 0.65f, was.a * 0.45f);
                Sprite picture = lanterns[stateOf];
                if (picture != null) RunHud.Sprite(icon, picture);
                else ArcadeSkin.Dot(new Rect(x - 3, area.y + 8, 6, 10), stateOf == 2 ? ArcadeSkin.Card : ArcadeSkin.Gold);
                GUI.color = was;
            }
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
            if (count == null) { count = ArcadeTypography.Style(18, false, true); count.alignment = TextAnchor.MiddleCenter; count.wordWrap = false; }
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
                LocalizedGUI.Label(new Rect(centre.x + radius - 4, centre.y + radius * 0.35f, 40, 16), "+" + (companions - shown), count);
            }
        }
    }
}
