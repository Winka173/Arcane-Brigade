using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class BuildMorphVisual : MonoBehaviour
    {
        private sealed class View { public LineRenderer Ink, Line; public SpriteRenderer Marker; }
        private readonly List<View> pool = new List<View>();
        private Material material;
        private BuildMorphArtwork art;
        public int VisibleCount { get; private set; }
        public void Display(CombatSimulation simulation, bool reduced)
        {
            VisibleCount = 0;
            if (simulation != null && (simulation.Phase == RunPhase.Combat || simulation.Phase == RunPhase.Paused))
                foreach (MorphStrike strike in simulation.MorphStrikes)
                {
                    if (VisibleCount >= EffectDensity.Limit(72)) break;
                    View view = Acquire(VisibleCount++);
                    float time = simulation.Elapsed;
                    bool preparing = time < strike.At;
                    float fade = preparing ? .32f : Mathf.Clamp01((strike.Until - time) / .34f);
                    Color color = BuildMorphs.Get(strike.Kind).Color;
                    color.a = fade * EffectDensity.AlliedAlpha * (PresentationSettings.HighReadability ? .7f : 1);
                    view.Line.startColor = view.Line.endColor = color;
                    view.Ink.startColor = view.Ink.endColor = new Color(.045f, .075f, .12f, color.a * .85f);
                    float width = preparing ? .025f : .055f;
                    view.Line.widthMultiplier = width; view.Ink.widthMultiplier = width + .025f;
                    bool line = strike.Geometry == MorphGeometry.Line, seal = strike.Geometry == MorphGeometry.Seal;
                    bool arc = strike.Geometry == MorphGeometry.Crescent;
                    int count = line ? 14 : seal ? 4 : 48;
                    view.Line.positionCount = view.Ink.positionCount = count;
                    view.Line.loop = view.Ink.loop = !line && !arc;
                    Vector2 axis = strike.End - strike.Position;
                    for (int p = 0; p < count; p++)
                    {
                        Vector2 point;
                        if (line)
                        {
                            float f = p / (count - 1f);
                            Vector2 normal = new Vector2(-axis.y, axis.x).normalized;
                            float thread = strike.Kind == BuildMorphKind.Thread && !reduced ? Mathf.Sin(f * Mathf.PI * 8) * .035f : 0;
                            point = Vector2.Lerp(strike.Position, strike.End, f) + normal * thread;
                        }
                        else if (seal)
                        {
                            Vector2 side = new Vector2(-strike.Direction.y, strike.Direction.x);
                            point = strike.Position + strike.Direction * (p < 2 ? -1 : 1) * strike.Radius + side * (p == 0 || p == 3 ? -1 : 1) * strike.Radius;
                        }
                        else
                        {
                            float span = arc ? Mathf.Acos(strike.Cone) : Mathf.PI;
                            float a = arc ? Mathf.Atan2(strike.Direction.y, strike.Direction.x) - span + p / (count - 1f) * span * 2 : p * Mathf.PI * 2 / count;
                            point = strike.Position + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * strike.Radius;
                        }
                        view.Ink.SetPosition(p, point); view.Line.SetPosition(p, point);
                    }
                    view.Line.enabled = view.Ink.enabled = true;
                    Sprite sprite = strike.Kind == BuildMorphKind.Mirror ? strike.Source.Definition.Body : art == null ? null : art.Charm(strike.Kind);
                    view.Marker.enabled = sprite != null;
                    if (sprite != null)
                    {
                        view.Marker.sprite = sprite;
                        Vector2 at = line ? strike.Kind == BuildMorphKind.Mirror ? strike.Position : strike.End : strike.Position;
                        Rect ink = SpriteInkMetrics.BoundsOf(sprite);
                        float size = strike.Kind == BuildMorphKind.Mirror ? strike.Source.Definition.BodyHeight * .8f : .24f + strike.Stage * .025f;
                        float scale = size / Mathf.Max(.01f, ink.height);
                        view.Marker.transform.localScale = Vector3.one * scale;
                        view.Marker.transform.position = (Vector3)at - (Vector3)(ink.center * scale);
                        view.Marker.color = strike.Kind == BuildMorphKind.Mirror ? new Color(color.r, color.g, color.b, fade * .27f) : new Color(1, 1, 1, fade * EffectDensity.AlliedAlpha);
                    }
                }
            for (int i = VisibleCount; i < pool.Count; i++) pool[i].Ink.enabled = pool[i].Line.enabled = pool[i].Marker.enabled = false;
        }
        private View Acquire(int index)
        {
            if (index < pool.Count) return pool[index];
            if (material == null) { material = new Material(Shader.Find("Sprites/Default")); art = Resources.Load<BuildMorphArtwork>("BuildMorphArtwork"); }
            LineRenderer Line(string name, int order)
            {
                var go = new GameObject(name); go.transform.SetParent(transform, false);
                var line = go.AddComponent<LineRenderer>(); line.sharedMaterial = material; line.useWorldSpace = true;
                line.numCapVertices = 4; line.numCornerVertices = 3; line.sortingOrder = order; return line;
            }
            var marker = new GameObject("FormMotif"); marker.transform.SetParent(transform, false);
            var renderer = marker.AddComponent<SpriteRenderer>(); renderer.sortingOrder = 1102;
            var view = new View { Ink = Line("FormInk", 1100), Line = Line("FormColour", 1101), Marker = renderer }; pool.Add(view); return view;
        }
        private void OnDestroy() { if (material != null) Destroy(material); }
    }
}
