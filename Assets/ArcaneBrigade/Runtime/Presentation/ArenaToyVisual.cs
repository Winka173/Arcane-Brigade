using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class ArenaToyVisual : MonoBehaviour
    {
        private sealed class ToyView
        {
            public Transform Root;
            public Transform Body;
            public LineRenderer Warning;
            public LineRenderer Destination;
            public LineRenderer Lane;
            public readonly List<GameObject> Charges = new List<GameObject>();
        }

        private readonly List<ToyView> views = new List<ToyView>();
        private readonly List<Mesh> meshes = new List<Mesh>();
        private Material material;
        private int generation = -1;
        private CombatSimulation displayedSimulation;
        private static readonly Color Ink = new Color(0.16f, 0.13f, 0.25f);
        private static readonly Color Brass = new Color(0.94f, 0.70f, 0.32f);
        private static readonly Color Cream = new Color(1f, 0.91f, 0.72f);
        private static readonly Color Mint = new Color(0.38f, 0.85f, 0.76f);

        public void Display(CombatSimulation simulation, float interpolation = 1f)
        {
            if (simulation == null) { ClearViews(); return; }
            if (material == null) material = new Material(Shader.Find("Sprites/Default"));
            if (displayedSimulation != simulation || generation != simulation.Generation || views.Count != simulation.ArenaToys.Count)
            {
                ClearViews();
                displayedSimulation = simulation;
                generation = simulation.Generation;
                foreach (ArenaToyState toy in simulation.ArenaToys) views.Add(Build(toy));
            }
            bool visible = simulation.Phase == RunPhase.Combat || simulation.Phase == RunPhase.Paused;
            float now = simulation.Elapsed;
            for (int index = 0; index < views.Count; index++)
            {
                ToyView view = views[index];
                ArenaToyState toy = simulation.ArenaToys[index];
                view.Root.gameObject.SetActive(visible);
                if (!visible) continue;
                Vector2 position = Vector2.Lerp(toy.PreviousPosition, toy.Position, Mathf.Clamp01(interpolation));
                view.Root.position = new Vector3(position.x, position.y, 0f);
                bool warning = toy.Mode == ArenaToyMode.Warning;
                float shake = warning ? Mathf.Sin(now * 24f) * 5f : 0f;
                view.Body.localRotation = Quaternion.Euler(0f, 0f, shake);
                view.Body.localScale = Vector3.one * (warning ? 1f + 0.035f * Mathf.Sin(now * 18f) : 1f);
                Color cue = Color.Lerp(Brass, Cream, 0.5f + 0.5f * Mathf.Sin(now * 8f));
                view.Warning.enabled = warning && toy.Kind != ArenaToyKind.ToyCart;
                Circle(view.Warning, toy.Position, toy.Radius, cue);
                view.Lane.enabled = (warning || toy.Mode == ArenaToyMode.Rolling) && toy.Kind == ArenaToyKind.ToyCart;
                Lane(view.Lane, toy.LaneStart, toy.LaneEnd, toy.Radius, cue);
                view.Destination.enabled = simulation.ArenaShiftWarning;
                Circle(view.Destination, toy.ShiftDestination, 0.75f, Mint);
                for (int charge = 0; charge < view.Charges.Count; charge++)
                    view.Charges[charge].SetActive(charge < toy.Charge || warning);
            }
        }

        private ToyView Build(ArenaToyState toy)
        {
            var root = new GameObject(toy.Kind.ToString());
            root.transform.SetParent(transform, false);
            var body = new GameObject("PaintedWoodAndBrass");
            body.transform.SetParent(root.transform, false);
            var view = new ToyView { Root = root.transform, Body = body.transform };
            Disc(root.transform, "SoftShadow", new Vector2(0f, -0.14f), new Vector2(0.65f, 0.22f), new Color(0.1f, 0.08f, 0.18f, 0.22f), 45);
            RunCatalog content = Resources.Load<RunCatalog>("RunCatalog");
            if (content != null && content.ArenaPropSprites.Length > (int)toy.Kind && content.ArenaPropSprites[(int)toy.Kind] != null)
            {
                Sprite sprite = content.ArenaPropSprites[(int)toy.Kind];
                var painted = new GameObject("HDProp"); painted.transform.SetParent(body.transform, false);
                SpriteRenderer renderer = painted.AddComponent<SpriteRenderer>(); renderer.sprite = sprite; renderer.sortingOrder = 84;
                painted.transform.localScale = Vector3.one * (1.15f / sprite.bounds.size.x);
            }
            else if (toy.Kind == ArenaToyKind.StageBell)
            {
                Quad(body.transform, "PlinthOutline", new Vector2(0f, -0.3f), new Vector2(1.12f, 0.22f), Ink);
                Quad(body.transform, "Plinth", new Vector2(0f, -0.28f), new Vector2(1f, 0.12f), Mint);
                Polygon(body.transform, "BellOutline", new[] { new Vector2(-0.51f, -0.18f), new Vector2(0.51f, -0.18f), new Vector2(0.32f, 0.12f), new Vector2(0.24f, 0.50f), new Vector2(-0.24f, 0.50f), new Vector2(-0.32f, 0.12f) }, Ink, 82);
                Polygon(body.transform, "BellBrass", new[] { new Vector2(-0.43f, -0.12f), new Vector2(0.43f, -0.12f), new Vector2(0.26f, 0.14f), new Vector2(0.18f, 0.43f), new Vector2(-0.18f, 0.43f), new Vector2(-0.26f, 0.14f) }, Brass, 83);
                Quad(body.transform, "BrushedHighlight", new Vector2(-0.1f, 0.23f), new Vector2(0.055f, 0.27f), Cream, 84);
                Disc(body.transform, "Clapper", new Vector2(0f, -0.19f), Vector2.one * 0.11f, Ink, 84);
                Disc(body.transform, "Crown", new Vector2(0f, 0.52f), Vector2.one * 0.08f, Brass, 84);
            }
            else
            {
                Quad(body.transform, "ChassisOutline", new Vector2(0f, 0.02f), new Vector2(1.15f, 0.54f), Ink);
                Quad(body.transform, "LacquerChassis", new Vector2(0f, 0.04f), new Vector2(1.03f, 0.4f), Mint, 83);
                Quad(body.transform, "CabOutline", new Vector2(0.18f, 0.39f), new Vector2(0.46f, 0.4f), Ink, 84);
                Quad(body.transform, "CabWindow", new Vector2(0.18f, 0.4f), new Vector2(0.32f, 0.24f), Cream, 85);
                Quad(body.transform, "BrassRail", new Vector2(-0.30f, 0.25f), new Vector2(0.30f, 0.07f), Brass, 85);
                for (int wheel = 0; wheel < 2; wheel++)
                {
                    Vector2 centre = new Vector2(wheel == 0 ? -0.34f : 0.34f, -0.25f);
                    Disc(body.transform, "RubberWheel", centre, Vector2.one * 0.20f, Ink, 86);
                    Disc(body.transform, "Hub", centre, Vector2.one * 0.10f, Brass, 87);
                    Disc(body.transform, "HubPin", centre, Vector2.one * 0.035f, Cream, 88);
                }
            }
            for (int charge = 0; charge < toy.RequiredCharge; charge++)
                view.Charges.Add(Disc(root.transform, "ChargePip", new Vector2((charge - (toy.RequiredCharge - 1) * 0.5f) * 0.22f, -0.60f), Vector2.one * 0.065f, Brass, 90));
            view.Warning = MakeLine(root.transform, "BellWarning", 49);
            view.Destination = MakeLine(root.transform, "ShiftPreview", 48);
            view.Lane = MakeLine(root.transform, "CartLane", 49);
            return view;
        }

        private LineRenderer MakeLine(Transform parent, string name, int order)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            var line = child.AddComponent<LineRenderer>();
            line.sharedMaterial = material;
            line.useWorldSpace = true;
            line.loop = true;
            line.widthMultiplier = 0.035f;
            line.sortingOrder = order;
            line.enabled = false;
            return line;
        }

        private static void Circle(LineRenderer line, Vector2 centre, float radius, Color color)
        {
            if (!line.enabled) return;
            line.positionCount = 64;
            line.startColor = line.endColor = color;
            for (int point = 0; point < 64; point++)
            {
                float angle = point * Mathf.PI * 2f / 64f;
                line.SetPosition(point, centre + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
            }
        }

        private static void Lane(LineRenderer line, Vector2 start, Vector2 end, float radius, Color color)
        {
            if (!line.enabled) return;
            Vector2 offset = end - start;
            Vector2 normal = offset.sqrMagnitude > 0.0001f ? new Vector2(-offset.y, offset.x).normalized * radius : Vector2.up * radius;
            line.positionCount = 4;
            line.startColor = line.endColor = color;
            line.SetPosition(0, start + normal);
            line.SetPosition(1, end + normal);
            line.SetPosition(2, end - normal);
            line.SetPosition(3, start - normal);
        }

        private GameObject Quad(Transform parent, string name, Vector2 centre, Vector2 size, Color color, int order = 82)
        {
            Vector2 half = size * 0.5f;
            return Polygon(parent, name, new[] { centre + new Vector2(-half.x, -half.y), centre + new Vector2(half.x, -half.y), centre + half, centre + new Vector2(-half.x, half.y) }, color, order);
        }

        private GameObject Disc(Transform parent, string name, Vector2 centre, Vector2 radius, Color color, int order)
        {
            var points = new Vector2[32];
            for (int point = 0; point < points.Length; point++)
            {
                float angle = point * Mathf.PI * 2f / points.Length;
                points[point] = centre + new Vector2(Mathf.Cos(angle) * radius.x, Mathf.Sin(angle) * radius.y);
            }
            return Polygon(parent, name, points, color, order);
        }

        private GameObject Polygon(Transform parent, string name, Vector2[] points, Color color, int order)
        {
            var vertices = new Vector3[points.Length];
            var colors = new Color[points.Length];
            var triangles = new int[(points.Length - 2) * 3];
            for (int point = 0; point < points.Length; point++) { vertices[point] = points[point]; colors[point] = color; }
            for (int triangle = 0; triangle < points.Length - 2; triangle++)
            {
                triangles[triangle * 3] = 0;
                triangles[triangle * 3 + 1] = triangle + 1;
                triangles[triangle * 3 + 2] = triangle + 2;
            }
            var mesh = new Mesh { name = name, vertices = vertices, colors = colors, triangles = triangles };
            mesh.RecalculateBounds();
            meshes.Add(mesh);
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            child.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = child.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.sortingOrder = order;
            return child;
        }

        private void ClearViews()
        {
            foreach (ToyView view in views) { view.Root.gameObject.SetActive(false); Destroy(view.Root.gameObject); }
            views.Clear();
            foreach (Mesh mesh in meshes) Destroy(mesh);
            meshes.Clear();
        }

        private void OnDestroy()
        {
            ClearViews();
            if (material != null) Destroy(material);
        }
    }
}
