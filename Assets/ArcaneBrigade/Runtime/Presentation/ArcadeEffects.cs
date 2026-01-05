using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class ArcadeEffects : MonoBehaviour
    {
        private enum Shape { Ring, Slash, Star, Puff, Glyph }
        private sealed class Burst
        {
            public Mesh Mesh;
            public MeshRenderer Renderer;
            public Transform Transform;
            public Shape Shape;
            public float Age = 10f;
            public float Duration;
            public float Radius;
            public Color Color;
        }
        private readonly List<Burst> bursts = new List<Burst>();
        private readonly List<Vector3> vertices = new List<Vector3>();
        private readonly List<Color> colors = new List<Color>();
        private readonly List<int> triangles = new List<int>();
        private Material material;
        public int ActiveCount { get; private set; }

        private void Awake() { material = new Material(Shader.Find("Sprites/Default")); }

        public void Clear()
        {
            foreach (Burst burst in bursts) { burst.Age = 10f; burst.Renderer.enabled = false; }
            ActiveCount = 0;
        }

        public static Color ElementColor(DamageElement element)
        {
            return element == DamageElement.Ember ? new Color(1f, 0.34f, 0.12f) : element == DamageElement.Frost ? new Color(0.15f, 0.92f, 1f) : element == DamageElement.Arcane ? new Color(0.85f, 0.25f, 1f) : new Color(1f, 0.82f, 0.28f);
        }

        public void Play(CombatEvent value)
        {
            WeaponDefinition weapon = value.Weapon;
            Color color = ElementColor(value.Element);
            float angle = Mathf.Atan2(value.Direction.y, value.Direction.x) * Mathf.Rad2Deg;
            if (value.Kind == CombatEventKind.SignatureMark)
            {
                Spawn(Shape.Star, value.Position, 45, 0.12f + value.Value * 0.055f, 0.24f, new Color(1, 0.28f, 0.38f));
            }
            else if (value.Kind == CombatEventKind.SignatureBurst)
            {
                Spawn(Shape.Star, value.Position, 45, 0.72f, 0.28f, new Color(1, 0.25f, 0.34f));
                Spawn(Shape.Glyph, value.Position, 0, 0.55f, 0.35f, new Color(1, 0.86f, 0.56f));
            }
            else if (value.Kind == CombatEventKind.SignaturePulse)
            {
                bool umbrella = weapon != null && weapon.Signature == SignatureWeapon.Umbrella;
                Spawn(umbrella ? Shape.Slash : Shape.Ring, value.Position, angle, value.Value, 0.32f, umbrella ? new Color(0.35f, 0.95f, 0.88f) : new Color(1, 0.82f, 0.35f));
            }
            else if (value.Kind == CombatEventKind.Fire && weapon != null)
            {
                if (weapon.Kind == WeaponKind.Melee)
                {
                    float radius = Mathf.Min(weapon.Range, 2.5f);
                    if (weapon.Signature == SignatureWeapon.Stamp)
                        Spawn(Shape.Star, value.Position + value.Direction * weapon.Range * 0.65f, 45, 0.22f, 0.13f, new Color(1, 0.35f, 0.38f));
                    else
                    {
                        Spawn(Shape.Slash, value.Position, angle, radius, 0.18f, color);
                        Spawn(Shape.Slash, value.Position, angle + 8f, radius * 0.9f, 0.1f, new Color(1f, 1f, 0.88f));
                    }
                }
                else
                {
                    Vector2 offset = weapon.HasMuzzleOffset ? weapon.ProjectileMuzzleOffset : Vector2.right * weapon.MuzzleDistance;
                    Vector2 muzzle = value.Position + value.Direction * (weapon.MountDistance - weapon.RecoilDistance) + (Vector2)(Quaternion.Euler(0, 0, angle) * offset);
                    Spawn(weapon.Motion == WeaponMotion.Bow ? Shape.Star : Shape.Glyph, muzzle, angle, weapon.Motion == WeaponMotion.Bow ? 0.25f : 0.42f, 0.22f, color);
                }
            }
            else if (value.Kind == CombatEventKind.Windup && weapon != null && (weapon.Motion == WeaponMotion.Cast || weapon.Motion == WeaponMotion.Bell))
                Spawn(Shape.Glyph, value.Position + value.Direction * 0.7f, angle, 0.26f, weapon.Windup, color);
            else if (value.Kind == CombatEventKind.Hit)
            {
                Spawn(Shape.Star, value.Position, angle + 15f, value.Critical ? 0.58f : 0.32f, 0.12f, new Color(1f, 1f, 0.9f));
                Spawn(Shape.Ring, value.Position, 0f, value.Faction == Faction.Champion ? 0.75f : 0.45f, 0.22f, value.Faction == Faction.Champion ? new Color(1f, 0.35f, 0.45f) : color);
            }
            else if (value.Kind == CombatEventKind.Kill)
            {
                Spawn(Shape.Puff, value.Position, 0, 0.6f, 0.32f, new Color(0.17f, 0.13f, 0.26f));
                Spawn(Shape.Ring, value.Position, 0, 0.5f, 0.2f, new Color(0.75f, 0.45f, 0.95f));
            }
            else if (value.Kind == CombatEventKind.Pickup)
                Spawn(Shape.Star, value.Position, 0, 0.23f, 0.25f, value.Loot == LootKind.Egg ? new Color(0.45f, 1f, 0.86f) : value.Loot == LootKind.Heart ? new Color(1f, 0.4f, 0.58f) : new Color(1f, 0.88f, 0.45f));
            else if (value.Kind == CombatEventKind.ChestOpened)
            {
                Spawn(Shape.Ring, value.Position, 0, 0.85f, 0.4f, new Color(1f, 0.79f, 0.3f));
                Spawn(Shape.Star, value.Position, 0, 0.55f, 0.22f, new Color(1f, 0.96f, 0.75f));
            }
            else if (value.Kind == CombatEventKind.Proc)
                Spawn(Shape.Ring, value.Position, 0, value.Cause.ItemId == "chest-thunder" ? 3f : 0.4f, 0.35f, new Color(0.55f, 0.9f, 1f));
            else if (value.Kind == CombatEventKind.Heal)
                Spawn(Shape.Ring, value.Position, 0, 0.35f, 0.28f, new Color(0.35f, 0.92f, 0.75f));
        }

        private void Spawn(Shape shape, Vector2 position, float angle, float radius, float duration, Color color)
        {
            Burst burst = bursts.Find(candidate => candidate.Age >= candidate.Duration);
            if (burst == null)
            {
                if (bursts.Count >= 96) return;
                var child = new GameObject("Arcade" + shape);
                child.transform.SetParent(transform, false);
                var mesh = new Mesh { name = "PooledArcadeCue" };
                mesh.MarkDynamic();
                child.AddComponent<MeshFilter>().sharedMesh = mesh;
                MeshRenderer renderer = child.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = material;
                burst = new Burst { Mesh = mesh, Renderer = renderer, Transform = child.transform };
                bursts.Add(burst);
            }
            burst.Shape = shape;
            burst.Age = 0;
            burst.Duration = Mathf.Max(0.06f, duration);
            burst.Radius = radius;
            burst.Color = color;
            burst.Transform.position = position;
            burst.Transform.rotation = Quaternion.Euler(0, 0, angle);
            burst.Renderer.sortingOrder = shape == Shape.Puff ? 700 : 2100;
            burst.Renderer.enabled = true;
            Draw(burst);
        }

        public void Advance(float delta)
        {
            ActiveCount = 0;
            foreach (Burst burst in bursts)
            {
                if (burst.Age >= burst.Duration) continue;
                burst.Age += delta;
                if (burst.Age >= burst.Duration) { burst.Renderer.enabled = false; continue; }
                ActiveCount++;
                Draw(burst);
            }
        }

        private void Draw(Burst burst)
        {
            vertices.Clear(); colors.Clear(); triangles.Clear();
            float progress = Mathf.Clamp01(burst.Age / burst.Duration);
            Color color = burst.Color;
            color.a = Mathf.Pow(1f - progress, 1.4f) * (burst.Shape == Shape.Puff ? 0.22f : 0.95f);
            float eased = 1f - Mathf.Pow(1f - progress, 3);
            float radius = burst.Radius * (burst.Shape == Shape.Star ? 1f - eased * 0.3f : 0.75f + eased * 0.4f);
            const int segments = 72;
            if (burst.Shape == Shape.Ring || burst.Shape == Shape.Slash || burst.Shape == Shape.Glyph)
            {
                float first = burst.Shape == Shape.Slash ? -1.15f + progress * 0.35f : 0f;
                float length = burst.Shape == Shape.Slash ? 2.3f : Mathf.PI * 2f;
                for (int index = 0; index <= segments; index++)
                {
                    float fraction = index / (float)segments;
                    float angle = first + fraction * length;
                    Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    float width = burst.Shape == Shape.Slash ? 0.22f * Mathf.Sin(fraction * Mathf.PI) : 0.045f;
                    Color visible = color;
                    if (burst.Shape == Shape.Slash) visible.a *= Mathf.Clamp01(Mathf.Sin(fraction * Mathf.PI) * 4);
                    for (int edge = 0; edge < 4; edge++)
                    {
                        float edgeRadius = edge == 0 ? radius + 0.014f : edge == 1 ? radius : edge == 2 ? radius - width : radius - width - 0.014f;
                        vertices.Add(direction * Mathf.Max(0, edgeRadius));
                        colors.Add(edge == 0 || edge == 3 ? new Color(visible.r, visible.g, visible.b, 0) : visible);
                    }
                    if (index > 0)
                        for (int edge = 0; edge < 3; edge++)
                        {
                            int previous = (index - 1) * 4 + edge; int current = index * 4 + edge;
                            triangles.Add(previous); triangles.Add(current); triangles.Add(previous + 1);
                            triangles.Add(previous + 1); triangles.Add(current); triangles.Add(current + 1);
                        }
                }
                if (burst.Shape == Shape.Glyph)
                {
                    for (int rune = 0; rune < 4; rune++)
                    {
                        float angle = rune * Mathf.PI * 0.5f + progress * 0.5f;
                        Vector2 center = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius * 0.8f;
                        int firstVertex = vertices.Count;
                        vertices.Add(center + Vector2.up * 0.045f); vertices.Add(center + Vector2.right * 0.035f); vertices.Add(center + Vector2.down * 0.045f); vertices.Add(center + Vector2.left * 0.035f);
                        for (int corner = 0; corner < 4; corner++) colors.Add(color);
                        triangles.Add(firstVertex); triangles.Add(firstVertex + 1); triangles.Add(firstVertex + 2); triangles.Add(firstVertex); triangles.Add(firstVertex + 2); triangles.Add(firstVertex + 3);
                    }
                }
            }
            else
            {
                vertices.Add(Vector3.zero); colors.Add(color);
                int points = burst.Shape == Shape.Star ? 12 : 20;
                for (int index = 0; index <= points; index++)
                {
                    float angle = index * Mathf.PI * 2f / points;
                    float size = burst.Shape == Shape.Star ? (index % 2 == 0 ? 1f : 0.3f) : 0.9f + Mathf.Sin(index * 2.4f) * 0.12f;
                    vertices.Add(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius * size);
                    colors.Add(color);
                    if (index > 0) { triangles.Add(0); triangles.Add(index); triangles.Add(index + 1); }
                }
            }
            burst.Mesh.Clear();
            burst.Mesh.SetVertices(vertices); burst.Mesh.SetColors(colors); burst.Mesh.SetTriangles(triangles, 0);
            burst.Mesh.RecalculateBounds();
        }

        private void Quad(int index)
        {
            triangles.Add(index); triangles.Add(index + 2); triangles.Add(index + 1);
            triangles.Add(index + 1); triangles.Add(index + 2); triangles.Add(index + 3);
        }

        private void OnDestroy()
        {
            foreach (Burst burst in bursts) Destroy(burst.Mesh);
            if (material != null) Destroy(material);
        }
    }
}
