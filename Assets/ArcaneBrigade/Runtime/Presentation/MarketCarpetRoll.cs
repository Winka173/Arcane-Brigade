using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class MarketCarpetRoll : MonoBehaviour
    {
        private SpriteRenderer source;
        private Mesh mesh;
        private Material material;
        private MeshRenderer rolled;
        private Vector3[] vertices;
        private Color[] colors;
        private Mesh rollMesh;
        private MeshRenderer rollRenderer;
        private Vector3[] rollVertices;
        private Color[] rollColors;
        private Vector2[] rollCoordinates;
        private const int Columns = 128;
        private const int Rows = 24;
        private const int RollSegments = 48;
        public float RollFront { get; private set; }
        public float RollHeight { get; private set; }

        public void Configure(SpriteRenderer renderer)
        {
            source = renderer;
            var child = new GameObject("CurledCarpetSurface"); child.transform.SetParent(transform, false);
            rolled = child.AddComponent<MeshRenderer>(); rolled.sortingOrder = source.sortingOrder;
            rolled.sortingLayerID = source.sortingLayerID;
            material = new Material(Shader.Find("Sprites/Default")); material.mainTexture = source.sprite.texture;
            rolled.sharedMaterial = material;
            mesh = new Mesh { name = "MarketCarpetCurl" };
            child.AddComponent<MeshFilter>().sharedMesh = mesh;
            vertices = new Vector3[(Columns + 1) * (Rows + 1)]; colors = new Color[vertices.Length];
            var coordinates = new Vector2[vertices.Length];
            var triangles = new int[Columns * Rows * 6];
            Rect texture = source.sprite.textureRect;
            for (int row = 0; row <= Rows; row++)
                for (int column = 0; column <= Columns; column++)
                {
                    int index = row * (Columns + 1) + column;
                    coordinates[index] = new Vector2((texture.x + texture.width * column / Columns) / source.sprite.texture.width, (texture.y + texture.height * row / Rows) / source.sprite.texture.height);
                    if (row == Rows || column == Columns) continue;
                    int offset = (row * Columns + column) * 6;
                    triangles[offset] = index; triangles[offset + 1] = index + Columns + 1; triangles[offset + 2] = index + 1;
                    triangles[offset + 3] = index + 1; triangles[offset + 4] = index + Columns + 1; triangles[offset + 5] = index + Columns + 2;
                }
            mesh.vertices = vertices; mesh.uv = coordinates; mesh.triangles = triangles;
            var cylinder = new GameObject("MovingCarpetRoll"); cylinder.transform.SetParent(transform, false);
            rollRenderer = cylinder.AddComponent<MeshRenderer>(); rollRenderer.sharedMaterial = material;
            rollRenderer.sortingOrder = source.sortingOrder + 1; rollRenderer.sortingLayerID = source.sortingLayerID;
            rollMesh = new Mesh { name = "CarpetRollCylinder" }; cylinder.AddComponent<MeshFilter>().sharedMesh = rollMesh;
            rollVertices = new Vector3[(RollSegments + 1) * (Rows + 1)];
            rollColors = new Color[rollVertices.Length]; rollCoordinates = new Vector2[rollVertices.Length];
            var rollTriangles = new int[RollSegments * Rows * 6];
            for (int row = 0; row < Rows; row++)
                for (int segment = 0; segment < RollSegments; segment++)
                {
                    int index = row * (RollSegments + 1) + segment;
                    int offset = (row * RollSegments + segment) * 6;
                    rollTriangles[offset] = index; rollTriangles[offset + 1] = index + RollSegments + 1; rollTriangles[offset + 2] = index + 1;
                    rollTriangles[offset + 3] = index + 1; rollTriangles[offset + 4] = index + RollSegments + 1; rollTriangles[offset + 5] = index + RollSegments + 2;
                }
            rollMesh.vertices = rollVertices; rollMesh.triangles = rollTriangles;
        }

        public void Display(float progress, float opacity)
        {
            if (source == null || mesh == null) return;
            source.enabled = progress >= 0.999f;
            rolled.enabled = !source.enabled && opacity > 0;
            rollRenderer.enabled = rolled.enabled;
            Color tint = source.color; tint.a = opacity; source.color = tint;
            if (!rolled.enabled) return;
            Bounds bounds = source.sprite.bounds;
            float front = bounds.min.x + bounds.size.x * progress;
            RollFront = front;
            float radius = bounds.size.x * (0.012f + 0.022f * Mathf.Sqrt(1 - Mathf.Clamp01(progress))) * Mathf.SmoothStep(0, 1, (1 - progress) / 0.035f);
            RollHeight = radius * 2;
            for (int row = 0; row <= Rows; row++)
                for (int column = 0; column <= Columns; column++)
                {
                    int index = row * (Columns + 1) + column;
                    float original = bounds.min.x + bounds.size.x * column / Columns;
                    vertices[index] = new Vector3(Mathf.Min(original, front), bounds.min.y + bounds.size.y * row / Rows, 0);
                    colors[index] = new Color(tint.r, tint.g, tint.b, original > front ? 0 : opacity);
                }
            mesh.vertices = vertices; mesh.colors = colors; mesh.RecalculateBounds();
            Rect texture = source.sprite.textureRect;
            for (int row = 0; row <= Rows; row++)
                for (int segment = 0; segment <= RollSegments; segment++)
                {
                    int index = row * (RollSegments + 1) + segment;
                    float angle = segment * Mathf.PI * 2 / RollSegments;
                    float rise = radius * (1 - Mathf.Cos(angle));
                    rollVertices[index] = new Vector3(front + Mathf.Sin(angle) * radius, bounds.min.y + bounds.size.y * row / Rows + rise * 0.55f, -rise);
                    float shade = 0.46f + 0.54f * Mathf.Clamp01(Mathf.Sin(angle));
                    rollColors[index] = new Color(tint.r * shade, tint.g * shade, tint.b * shade, opacity);
                    float sample = Mathf.Clamp01(progress + angle * radius / bounds.size.x);
                    rollCoordinates[index] = new Vector2((texture.x + texture.width * sample) / source.sprite.texture.width, (texture.y + texture.height * row / Rows) / source.sprite.texture.height);
                }
            rollMesh.vertices = rollVertices; rollMesh.colors = rollColors; rollMesh.uv = rollCoordinates; rollMesh.RecalculateBounds();
        }

        private void OnDestroy()
        {
            if (mesh != null) Destroy(mesh);
            if (rollMesh != null) Destroy(rollMesh);
            if (material != null) Destroy(material);
        }
    }
}
