using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class MidwaveSpectacleVisual : MonoBehaviour
    {
        private Material material;
        private Mesh mesh;
        private MeshRenderer floor;
        private LineRenderer edge;
        private LineRenderer embellishment;
        private SpriteRenderer bait;
        private readonly SpriteRenderer[] props = new SpriteRenderer[3];
        private readonly GroundShadow[] shadows = new GroundShadow[3];
        private CarnivalStageAssets art;
        private bool rectangular;
        private Vector3[] footprint;
        private readonly MaterialPropertyBlock tint = new MaterialPropertyBlock();
        public bool Visible => floor != null && floor.enabled;
        private void Prepare()
        {
            if (material != null) return;
            art = Resources.Load<CarnivalStageAssets>("CarnivalStageAssets");
            material = new Material(Shader.Find("Sprites/Default"));
            var ground = new GameObject("SpectacleFloorInk"); ground.transform.SetParent(transform, false);
            floor = ground.AddComponent<MeshRenderer>(); floor.sharedMaterial = material; floor.sortingOrder = 750;
            ground.AddComponent<MeshFilter>();
            var rim = new GameObject("SpectacleDangerEdge"); rim.transform.SetParent(transform, false);
            edge = rim.AddComponent<LineRenderer>(); edge.sharedMaterial = material; edge.useWorldSpace = true; edge.loop = true; edge.numCornerVertices = 5; edge.numCapVertices = 4; edge.sortingOrder = 2350;
            var flourish = new GameObject("SpectacleFlourish"); flourish.transform.SetParent(transform, false);
            embellishment = flourish.AddComponent<LineRenderer>(); embellishment.sharedMaterial = material; embellishment.useWorldSpace = true; embellishment.numCornerVertices = embellishment.numCapVertices = 4; embellishment.sortingOrder = 751; embellishment.widthMultiplier = 0.035f;
            var lure = new GameObject("GildedBait"); lure.transform.SetParent(transform, false); bait = lure.AddComponent<SpriteRenderer>(); bait.sprite = K8Motion.TierStar(); bait.sortingOrder = 1200;
            for (int index = 0; index < props.Length; index++)
            {
                var child = new GameObject("SpectaclePerformer" + index); child.transform.SetParent(transform, false);
                props[index] = child.AddComponent<SpriteRenderer>(); shadows[index] = child.AddComponent<GroundShadow>();
            }
        }
        private void MakeFloor(bool rectangle)
        {
            if (mesh != null && rectangular == rectangle) return;
            if (mesh != null) Destroy(mesh); rectangular = rectangle;
            int segments = rectangle ? 4 : 48;
            var vertices = new Vector3[segments + 1]; var colors = new Color[segments + 1]; var triangles = new int[segments * 3];
            colors[0] = Color.white;
            for (int index = 0; index < segments; index++)
            {
                float angle = index * Mathf.PI * 2 / segments + (rectangle ? Mathf.PI * 0.25f : 0);
                vertices[index + 1] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * (rectangle ? 1.414214f : 1);
                colors[index + 1] = new Color(1, 1, 1, rectangle ? 0.6f : 0.04f);
                triangles[index * 3] = 0; triangles[index * 3 + 1] = index + 1; triangles[index * 3 + 2] = (index + 1) % segments + 1;
            }
            mesh = new Mesh { name = "SpectacleInkFootprint", vertices = vertices, colors = colors, triangles = triangles };
            footprint = vertices;
            floor.GetComponent<MeshFilter>().sharedMesh = mesh;
        }
        public void Display(CombatSimulation simulation)
        {
            Prepare();
            MidwaveSpectacle show = simulation.Spectacle;
            bool active = (simulation.Phase == RunPhase.Combat || simulation.Phase == RunPhase.Paused) && show != null && show.Active(simulation.Elapsed);
            floor.enabled = edge.enabled = active;
            embellishment.enabled = bait.enabled = active;
            if (!active) { for (int index = 0; index < props.Length; index++) { props[index].enabled = false; shadows[index].Hide(); } return; }
            float age = show.Age(simulation.Elapsed);
            bool parade = show.Kind == SpectacleKind.PaperParade;
            bool treasure = show.Kind == SpectacleKind.GildedTrap;
            bool reduced = PresentationSettings.ReducedMotion;
            MakeFloor(parade || treasure);
            float fade = Mathf.Clamp01(age / 0.3f) * Mathf.Clamp01((MidwaveSpectacle.Duration - age) / 0.6f);
            Color color = age < 2 ? new Color(1, 0.75f, 0.3f) : show.Dangerous ? new Color(1, 0.22f, 0.43f) : new Color(0.4f, 1, 0.75f);
            floor.transform.position = parade ? show.Origin : show.Position;
            floor.transform.localScale = parade ? new Vector3(6, 0.75f, 1) : treasure ? new Vector3(1.15f, 0.8f, 1) : Vector3.one * 1.15f;
            tint.SetColor("_Color", new Color(color.r, color.g, color.b, fade * (treasure ? 0.2f : 0.16f))); floor.SetPropertyBlock(tint);
            edge.startColor = edge.endColor = new Color(color.r, color.g, color.b, fade * (show.Dangerous ? 0.8f : 0.55f)); edge.widthMultiplier = 0.045f;
            int count = rectangular ? 4 : 48; edge.positionCount = count;
            for (int index = 0; index < count; index++) edge.SetPosition(index, floor.transform.TransformPoint(footprint[index + 1]));
            bait.enabled = treasure && !show.Claimed;
            if (bait.enabled)
            {
                bait.transform.position = show.Position + Vector2.up * (0.55f + (reduced ? 0 : Mathf.Sin(age * 3) * 0.08f));
                bait.transform.localScale = Vector3.one * (0.3f / bait.sprite.bounds.size.x);
                bait.color = new Color(1, 0.85f, 0.3f, fade);
                bait.transform.rotation = Quaternion.Euler(0, 0, reduced ? 0 : age * 35);
            }
            embellishment.positionCount = parade ? 24 : 48;
            embellishment.startColor = embellishment.endColor = new Color(treasure ? 1 : 0.7f, treasure ? 0.78f : 0.55f, treasure ? 0.3f : 1, fade * 0.55f);
            embellishment.loop = treasure;
            for (int point = 0; point < embellishment.positionCount; point++)
            {
                float fraction = point / (float)(embellishment.positionCount - 1);
                if (parade)
                    embellishment.SetPosition(point, show.Origin + new Vector2(-6 + fraction * 12, Mathf.Sin(fraction * Mathf.PI * 12 + (reduced ? 0 : age * 3)) * 0.06f));
                else
                {
                    float angle = fraction * Mathf.PI * (treasure ? 2 : 5) + (reduced ? 0 : age * (treasure ? 1 : -1.2f));
                    float radius = treasure ? 0.55f : Mathf.Lerp(0.08f, 1.05f, fraction);
                    embellishment.SetPosition(point, show.Position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
                }
            }
            for (int index = 0; index < props.Length; index++)
            {
                SpriteRenderer prop = props[index]; prop.enabled = art != null && (!treasure || index == 0);
                if (!prop.enabled) { shadows[index].Hide(); continue; }
                int frame = PresentationSettings.ReducedMotion ? 0 : Mathf.FloorToInt(age * 7 + index * 0.7f) % (parade ? 12 : 4);
                prop.sprite = parade ? art.Courier[frame] : treasure ? art.Stands[4 + frame] : K8Motion.TierStar();
                Vector2 position = show.Position + (parade ? Vector2.left * index * 1.2f : Vector2.zero);
                if (!parade && !treasure)
                {
                    float angle = index * Mathf.PI * 2 / 3 + (reduced ? 0 : age * 1.1f);
                    position += new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 0.65f;
                }
                float hop = PresentationSettings.ReducedMotion ? 0 : Mathf.Abs(Mathf.Sin(age * (parade ? 6 : 2) + index)) * (parade ? 0.16f : 0.08f);
                prop.transform.position = position + Vector2.up * hop;
                prop.transform.localScale = Vector3.one * ((parade ? 1 : treasure ? 0.9f : 0.16f) / prop.sprite.bounds.size.x);
                prop.transform.rotation = Quaternion.Euler(0, 0, PresentationSettings.ReducedMotion ? 0 : Mathf.Sin(age * 3 + index) * (parade ? 7 : 3));
                prop.sortingOrder = 1000 - Mathf.RoundToInt(position.y * 12);
                prop.color = treasure ? new Color(1, 0.87f, 0.5f, fade) : parade ? new Color(1, 1, 1, fade) : new Color(0.6f, 0.35f, 0.9f, fade * 0.8f);
                shadows[index].Display(prop.sprite, position, 0.8f, hop, fade * 0.3f);
            }
        }
        private void OnDestroy() { if (mesh != null) Destroy(mesh); if (material != null) Destroy(material); }
    }
}
