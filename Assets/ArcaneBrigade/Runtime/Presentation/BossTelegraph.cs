using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class BossTelegraph : MonoBehaviour
    {
        private LineRenderer[] lines;
        private Material material;

        private void Awake()
        {
            material = new Material(Shader.Find("Sprites/Default"));
            lines = new LineRenderer[20];
            for (int index = 0; index < lines.Length; index++)
            {
                var child = new GameObject("DangerLane");
                child.transform.SetParent(transform, false);
                LineRenderer line = child.AddComponent<LineRenderer>();
                line.sharedMaterial = material;
                line.positionCount = 2;
                line.widthMultiplier = 0.035f;
                line.sortingOrder = 550;
                line.enabled = false;
                lines[index] = line;
            }
        }

        public void Display(CombatSimulation simulation)
        {
            UnitState boss = simulation.Boss;
            bool active = boss != null && boss.Alive && boss.TelegraphUntil > simulation.Elapsed;
            int count = active ? CombatSimulation.BossBulletCount(boss.Definition.BossPattern) : 0;
            for (int index = 0; index < lines.Length; index++)
            {
                LineRenderer line = lines[index];
                line.enabled = index < count;
                if (!line.enabled) continue;
                float angle = CombatSimulation.BossBulletAngle(boss, index, boss.TelegraphUntil);
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Color color = new Color(1f, 0.48f, 0.32f, PresentationSettings.ReducedMotion ? 0.5f : 0.25f + 0.3f * Mathf.PingPong(simulation.Elapsed * 4f, 1f));
                line.startColor = line.endColor = color;
                line.SetPosition(0, boss.Position);
                line.SetPosition(1, boss.Position + direction * 12f);
            }
        }

        private void OnDestroy() { if (material != null) Destroy(material); }
    }
}
