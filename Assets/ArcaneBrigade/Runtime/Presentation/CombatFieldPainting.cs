using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Draws authored ink at the simulation's real lane/radius, including its warning phase.</summary>
    public sealed class CombatFieldPainting
    {
        private readonly SpriteRenderer view, outline;
        public CombatFieldPainting(Transform parent, int index)
        {
            var go = new GameObject("Illustrated field " + index); go.transform.SetParent(parent, false);
            view = go.AddComponent<SpriteRenderer>(); view.sortingOrder = 62; // on the floor with every other area, under the bodies
            var danger = new GameObject("Illustrated warning edge " + index); danger.transform.SetParent(parent, false);
            outline = danger.AddComponent<SpriteRenderer>(); outline.sortingOrder = 63;
        }
        public void Hide() { view.enabled = outline.enabled = false; }
        public bool Display(ExpandedField field, float time)
        {
            Hide();
            if (time > field.Until) return false;
            bool warning = time < field.At;
            string key = field.Kind switch
            {
                ExpandedFieldKind.Tentacle => "Tentacle", ExpandedFieldKind.Shockwave => "ShockRing",
                ExpandedFieldKind.ShadowPool => "ShadowPool", ExpandedFieldKind.Curtain => "Curtain",
                ExpandedFieldKind.OfficeHazard => "OfficeHazard", ExpandedFieldKind.ToyHazard => "ToyHazard",
                ExpandedFieldKind.SewingLink => "Thread", _ => null
            };
            if (key == null) return false;
            bool lane = field.Kind == ExpandedFieldKind.Tentacle || key == "Thread";
            bool staged = key == "Tentacle" || key == "Curtain" || key == "OfficeHazard" || key == "ToyHazard";
            Sprite[] frames = CombatVfxArtwork.Frames(warning && !staged ? lane ? "TelegraphLane" : "TelegraphCircle" : key);
            if (frames == null || frames.Length == 0) return false;
            int frame = warning || PresentationSettings.ReducedMotion ? 0 : Mathf.FloorToInt((time - field.At) * 10) % frames.Length;
            // These two-frame hazards explicitly author anticipation then impact, rather than looping the warning.
            if (!warning && staged) frame = frames.Length - 1;
            Sprite image = frames[frame]; if (image == null) return false;
            Vector2 position = field.Position;
            float radius = field.Kind == ExpandedFieldKind.Shockwave && !warning ? CarnivalHazards.ShockRadius(field, time) : field.Radius;
            Vector2 size = Vector2.one * Mathf.Max(.05f, radius * 2);
            float angle = 0;
            if (lane)
            {
                Vector2 start = key == "Thread" ? field.Anchor != null ? field.Anchor.Position : field.Source?.Position ?? field.Start : field.Start;
                Vector2 end = key == "Thread" ? field.Target?.Position ?? field.Position : field.Position;
                position = (start + end) * .5f;
                size = new Vector2(Mathf.Max(.1f, Vector2.Distance(start, end) + (key == "Thread" ? 0 : radius * 2)), key == "Thread" ? .12f : radius * 2);
                angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;
            }
            Rect ink = SpriteInkMetrics.BoundsOf(image);
            // a staged hazard gathers during its warning (75% -> full) while its true edge is shown at full size
            float gather = warning && staged && !PresentationSettings.ReducedMotion ? Mathf.InverseLerp(field.Created, field.At, time) : 1;
            Vector2 shown = size * (warning && staged ? .75f + .25f * gather : 1);
            view.sprite = image; view.enabled = true;
            view.transform.rotation = Quaternion.Euler(0, 0, angle);
            view.transform.localScale = new Vector3(shown.x / Mathf.Max(.01f, ink.width), shown.y / Mathf.Max(.01f, ink.height), 1);
            view.transform.position = (Vector3)position - view.transform.TransformVector(ink.center);
            float fade = warning ? (staged ? .55f + .3f * gather : .85f) : Mathf.Lerp(staged ? .95f : 1, staged ? .2f : .5f, Mathf.Clamp01((time - field.At) / Mathf.Max(.05f, field.Until - field.At)));
            view.color = new Color(1, 1, 1, fade * (key == "Thread" ? EffectDensity.AlliedAlpha : 1));
            if (warning && staged)
            {
                Sprite edge = CombatVfxArtwork.Frame(lane ? "TelegraphLane" : "TelegraphCircle", 0);
                if (edge != null)
                {
                    Rect border = SpriteInkMetrics.BoundsOf(edge); outline.sprite = edge; outline.enabled = true;
                    outline.transform.rotation = view.transform.rotation;
                    outline.transform.localScale = new Vector3(size.x / Mathf.Max(.01f, border.width), size.y / Mathf.Max(.01f, border.height), 1);
                    outline.transform.position = (Vector3)position - outline.transform.TransformVector(border.center);
                    // the edge brightens and beats faster as the blow comes
                    float beat = PresentationSettings.ReducedFlashes || PresentationSettings.ReducedMotion ? 0 : .12f * Mathf.Sin(time * (8 + 22 * gather));
                    outline.color = new Color(1, 1, 1, Mathf.Clamp01(.45f + .4f * gather + beat));
                }
            }
            return true;
        }
    }
}
