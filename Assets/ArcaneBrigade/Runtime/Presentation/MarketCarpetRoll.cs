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
        private const int Columns = 32;
        private const int Rows = 32;

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
        }

        public void Display(float progress, float opacity)
        {
            if (source == null || mesh == null) return;
            source.enabled = progress >= 0.999f;
            rolled.enabled = !source.enabled && opacity > 0;
            Color tint = source.color; tint.a = opacity; source.color = tint;
            if (!rolled.enabled) return;
            Bounds bounds = source.sprite.bounds;
            float front = bounds.max.y - bounds.size.y * progress;
            float radius = bounds.size.y * 0.065f;
            for (int row = 0; row <= Rows; row++)
                for (int column = 0; column <= Columns; column++)
                {
                    int index = row * (Columns + 1) + column;
                    float original = bounds.min.y + bounds.size.y * row / Rows;
                    float unrolledDistance = Mathf.Max(0, front - original);
                    float angle = Mathf.Min(Mathf.PI, unrolledDistance / radius);
                    float rise = angle > 0 ? radius * (1 - Mathf.Cos(angle)) : 0;
                    vertices[index] = new Vector3(bounds.min.x + bounds.size.x * column / Columns, angle > 0 ? front - Mathf.Sin(angle) * radius + rise * 0.65f : original, -rise);
                    float shade = angle > 0 ? 0.7f + Mathf.Cos(angle) * 0.25f : 1;
                    colors[index] = new Color(tint.r * shade, tint.g * shade, tint.b * shade, unrolledDistance > radius * Mathf.PI ? 0 : opacity);
                }
            mesh.vertices = vertices; mesh.colors = colors; mesh.RecalculateBounds();
        }

        private void OnDestroy()
        {
            if (mesh != null) Destroy(mesh);
            if (material != null) Destroy(material);
        }
    }
}
