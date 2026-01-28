using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class ExpandedVisual : MonoBehaviour
    {
        private readonly List<LineRenderer> lines = new List<LineRenderer>();
        private Material material;
        private Mesh disc;
        private Mesh beam;
        private readonly List<MeshRenderer> fills = new List<MeshRenderer>();
        private readonly List<SpriteRenderer> props = new List<SpriteRenderer>();
        private readonly List<GroundShadow> propShadows = new List<GroundShadow>();
        private readonly List<EnemyVariantFieldVisual> variantFields = new List<EnemyVariantFieldVisual>();
        private CarnivalStageAssets stageArt;
        private MaterialPropertyBlock tint;
        public bool ReducedMotion;
        public int VisibleCount { get; private set; }
        public void Display(CombatSimulation simulation)
        {
            if (tint == null) tint = new MaterialPropertyBlock();
            VisibleCount = 0;
            if (simulation != null && (simulation.Phase == RunPhase.Combat || simulation.Phase == RunPhase.Paused))
                for (int priority = 0; priority < 2; priority++)
                foreach (ExpandedField field in simulation.ExpandedFields)
                {
                    bool hazard = CarnivalHazards.IsHostile(field.Kind);
                    bool carnival = field.Kind == ExpandedFieldKind.Projector || field.Kind == ExpandedFieldKind.MagnetCone || field.Kind == ExpandedFieldKind.YoYoTether || field.Kind == ExpandedFieldKind.SewingLink;
                    if (field.Kind == ExpandedFieldKind.YoYoTether && (field.Projectile == null || !ContainsProjectile(simulation, field.Projectile))) continue;
                    if (hazard != (priority == 0)) continue;
                    if (!hazard && !carnival && (field.Kind != ExpandedFieldKind.Link || field.Radius != 0) && field.Kind != ExpandedFieldKind.Strike && field.Kind != ExpandedFieldKind.Echo) continue;
                    if (VisibleCount >= (hazard ? 96 : EffectDensity.Limit(96))) break;
                    int slot = VisibleCount++;
                    LineRenderer line = Acquire(slot);
                    SpriteRenderer prop = props[slot];
                    prop.enabled = field.Kind == ExpandedFieldKind.FallingProp && field.Source != null;
                    if (prop.enabled)
                    {
                        BossAnatomyEntry anatomy = BossAnatomy.For(field.Source.Definition);
                        if (!field.Source.Definition.IsBoss && EnemyEvolutionLibrary.Current != null && EnemyEvolutionLibrary.Current.BodyFragments.Length == 16)
                            prop.sprite = EnemyEvolutionLibrary.Fragment(field.Source.Definition, Mathf.FloorToInt(simulation.Elapsed * 7));
                        else if (anatomy != null)
                        {
                            int organ = field.SourceOrgan >= 0 ? field.SourceOrgan : 2;
                            for (int part = 0; part < anatomy.Organs.Length && field.SourceOrgan < 0; part++)
                                if ((anatomy.Organs[part].Role & BossOrganRole.Props) != 0) { organ = part; break; }
                            prop.sprite = anatomy.Organs[organ].Charged;
                        }
                        else if (stageArt != null && stageArt.Stands.Length == 12)
                        {
                            int variant = field.Source.Definition.BossPattern % 5 == 0 ? 8 : 0;
                            prop.sprite = stageArt.Stands[variant + (ReducedMotion ? 0 : Mathf.FloorToInt(simulation.Elapsed * 8) % 4)];
                        }
                        float flight = Mathf.Clamp01((simulation.Elapsed - field.Created) / Mathf.Max(0.05f, field.At - field.Created));
                        prop.transform.position = ReducedMotion ? field.Position : Vector2.Lerp(field.Start, field.Position, flight) + Vector2.up * Mathf.Sin(flight * Mathf.PI) * 2.5f;
                        prop.transform.rotation = Quaternion.Euler(0, 0, ReducedMotion ? 0 : flight * 210);
                        prop.transform.localScale = Vector3.one * (0.7f / Mathf.Max(0.01f, prop.sprite.bounds.size.x));
                        prop.color = new Color(1, 0.7f, 0.6f, simulation.Elapsed < field.At ? 1 : Mathf.Clamp01((field.Until - simulation.Elapsed) / 0.4f));
                        propShadows[slot].Display(prop.sprite, Vector2.Lerp(field.Start, field.Position, flight), 0.7f, ReducedMotion ? 0 : Mathf.Sin(flight * Mathf.PI) * 2.5f, 0.3f * prop.color.a);
                    }
                    else propShadows[slot].Hide();
                    bool warning = simulation.Elapsed < field.At;
                    float progress = Mathf.Clamp01((simulation.Elapsed - field.At) / Mathf.Max(0.05f, field.Until - field.At));
                    Color color = hazard ? warning ? new Color(1f, 0.7f, 0.24f) : new Color(1f, 0.25f, 0.37f) : field.Kind == ExpandedFieldKind.Tea || field.Kind == ExpandedFieldKind.Iron ? new Color(1f, 0.45f, 0.24f) : field.Kind == ExpandedFieldKind.Photo ? new Color(0.4f, 1f, 0.83f) : new Color(0.83f, 0.64f, 1f);
                    if (!hazard && field.ImpactWeapon != null && field.ImpactWeapon.Mechanic == CarnivalWeapon.BloomOrb) color = new Color(0.55f, 1, 0.8f);
                    else if (!hazard && field.ImpactWeapon != null && field.ImpactWeapon.Mechanic == CarnivalWeapon.EchoBlade) color = new Color(0.65f, 0.8f, 1);
                    color.a = warning ? 0.65f + (ReducedMotion || PresentationSettings.ReducedFlashes ? 0 : Mathf.Sin(simulation.Elapsed * 8f) * 0.2f) : 0.8f * (1 - progress * 0.75f);
                    if (!hazard) color.a *= EffectDensity.AlliedAlpha;
                    line.startColor = line.endColor = color; line.widthMultiplier = hazard ? 0.06f : 0.04f; line.loop = true;
                    if (field.Kind == ExpandedFieldKind.Shockwave && !warning) line.widthMultiplier = 0.44f;
                    if (field.Kind == ExpandedFieldKind.YoYoTether || field.Kind == ExpandedFieldKind.SewingLink || field.Kind == ExpandedFieldKind.Link && field.Radius == 0)
                    {
                        Vector2 start = field.Anchor != null ? field.Anchor.Position : field.Source == null ? field.Start : field.Source.Position;
                        Vector2 end = field.Projectile != null ? field.Projectile.Position : field.Target != null ? field.Target.Position : field.Position;
                        line.loop = false; line.positionCount = 12;
                        for (int point = 0; point < 12; point++)
                        {
                            float fraction = point / 11f;
                            Vector2 normal = new Vector2(-(end - start).y, (end - start).x).normalized;
                            line.SetPosition(point, Vector2.Lerp(start, end, fraction) + normal * Mathf.Sin(fraction * Mathf.PI * 8f) * (ReducedMotion ? 0 : 0.055f) * (1 - progress));
                        }
                    }
                    else if (field.Kind == ExpandedFieldKind.Tentacle)
                    {
                        line.loop = false; line.positionCount = 16;
                        line.widthMultiplier = warning ? 0.06f : field.Radius * 2;
                        Vector2 end = field.Position;
                        Vector2 normal = new Vector2(-(end - field.Start).y, (end - field.Start).x).normalized;
                        for (int point = 0; point < 16; point++)
                        {
                            float fraction = point / 15f;
                            line.SetPosition(point, Vector2.Lerp(field.Start, end, fraction) + normal * Mathf.Sin(fraction * Mathf.PI * 3) * (warning || ReducedMotion ? 0 : 0.12f));
                        }
                    }
                    else if (field.Kind == ExpandedFieldKind.Iron)
                    {
                        Vector2 normal = new Vector2(-field.Direction.y, field.Direction.x) * field.Radius;
                        line.positionCount = 4;
                        line.SetPosition(0, field.Position - field.Direction + normal); line.SetPosition(1, field.Position + field.Direction + normal);
                        line.SetPosition(2, field.Position + field.Direction - normal); line.SetPosition(3, field.Position - field.Direction - normal);
                    }
                    else
                    {
                        line.positionCount = 64;
                        float radius = field.Radius * (ReducedMotion || hazard || warning ? 1 : Mathf.Lerp(0.75f, 1.08f, progress));
                        if (field.Kind == ExpandedFieldKind.Shockwave && !warning) radius = CarnivalHazards.ShockRadius(field, simulation.Elapsed);
                        for (int point = 0; point < 64; point++)
                        {
                            float angle = point * Mathf.PI * 2 / 64;
                            if (field.Kind == ExpandedFieldKind.Strike || field.Kind == ExpandedFieldKind.MagnetCone) angle = Mathf.Atan2(field.Direction.y, field.Direction.x) - 0.65f + point / 63f * 1.3f;
                            line.SetPosition(point, field.Position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
                        }
                        if (field.Kind == ExpandedFieldKind.Strike) line.loop = false;
                    }
                    line.enabled = hazard || carnival || field.Kind == ExpandedFieldKind.Link && field.Radius == 0 || field.Kind == ExpandedFieldKind.Strike || field.Kind == ExpandedFieldKind.Echo;
                    if (hazard) line.enabled = !warning && (field.Kind == ExpandedFieldKind.Tentacle || field.Kind == ExpandedFieldKind.Shockwave);
                    if (field.Kind == ExpandedFieldKind.Projector) line.enabled = false;
                    MeshRenderer fill = fills[slot];
                    fill.enabled = (hazard || field.Kind == ExpandedFieldKind.Projector) && field.Radius > 0;
                    if (hazard) fill.enabled = !warning && field.Kind == ExpandedFieldKind.ShadowPool;
                    if (field.Kind == ExpandedFieldKind.Tentacle || field.Kind == ExpandedFieldKind.Shockwave) fill.enabled = false;
                    if (fill.enabled)
                    {
                        fill.transform.position = field.Position; fill.transform.localScale = Vector3.one * field.Radius;
                        fill.GetComponent<MeshFilter>().sharedMesh = disc;
                        fill.transform.rotation = Quaternion.identity;
                        if (field.Kind == ExpandedFieldKind.Projector && field.Source != null)
                        {
                            if (beam == null)
                            {
                                beam = new Mesh { name = "CarnivalProjectorBeam", vertices = new[] { Vector3.zero, new Vector3(1, 1, 0), new Vector3(1, -1, 0) }, triangles = new[] { 0, 1, 2 }, colors = new[] { new Color(1, 1, 1, 0.65f), new Color(1, 1, 1, 0), new Color(1, 1, 1, 0) } };
                                beam.RecalculateBounds();
                            }
                            Vector2 direction = field.Position - field.Source.Position;
                            fill.GetComponent<MeshFilter>().sharedMesh = beam;
                            fill.transform.position = field.Source.Position;
                            fill.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
                            fill.transform.localScale = new Vector3(direction.magnitude + field.Radius, field.Radius, 1);
                        }
                        Color wash = color; wash.a *= hazard ? 0.14f : 0.65f;
                        tint.SetColor("_Color", wash); fill.SetPropertyBlock(tint);
                    }
                    bool painted = hazard && field.Source != null && field.Source.Variant != null && (field.Source.Variant.Affix != EnemyAffix.None || field.Source.Definition.SpecialEnemy != SpecialEnemyKind.None) && field.Kind != ExpandedFieldKind.FallingProp;
                    if (painted)
                    {
                        line.enabled = fill.enabled = false;
                        variantFields[slot].Display(field, simulation.Elapsed, ReducedMotion);
                    }
                    else variantFields[slot].Hide();
                }
            for (int index = VisibleCount; index < lines.Count; index++) { lines[index].enabled = false; fills[index].enabled = false; props[index].enabled = false; propShadows[index].Hide(); variantFields[index].Hide(); }
        }
        private static bool ContainsProjectile(CombatSimulation simulation, ProjectileState projectile)
        {
            foreach (ProjectileState active in simulation.Projectiles) if (active == projectile) return true;
            return false;
        }

        private LineRenderer Acquire(int index)
        {
            if (index < lines.Count) return lines[index];
            if (material == null) material = new Material(Shader.Find("Sprites/Default"));
            var child = new GameObject("ExpandedFieldCue"); child.transform.SetParent(transform, false);
            LineRenderer line = child.AddComponent<LineRenderer>(); line.sharedMaterial = material; line.useWorldSpace = true;
            line.sortingOrder = 65; line.numCapVertices = 5; line.numCornerVertices = 5; lines.Add(line);
            var falling = new GameObject("ThrownLuggage"); falling.transform.SetParent(transform, false);
            if (stageArt == null) stageArt = Resources.Load<CarnivalStageAssets>("CarnivalStageAssets");
            SpriteRenderer prop = falling.AddComponent<SpriteRenderer>(); prop.sprite = Resources.Load<RunCatalog>("RunCatalog").ChestSprite;
            prop.sortingOrder = 2450; props.Add(prop);
            GroundShadow shadow = falling.AddComponent<GroundShadow>(); shadow.Configure(prop.sprite); propShadows.Add(shadow);
            var painted = new GameObject("EnemyVariantFieldArt"); painted.transform.SetParent(transform, false);
            variantFields.Add(painted.AddComponent<EnemyVariantFieldVisual>());
            if (disc == null)
            {
                var vertices = new Vector3[65]; var colors = new Color[65]; var triangles = new int[192]; colors[0] = Color.white;
                for (int point = 0; point < 64; point++)
                {
                    float angle = point * Mathf.PI * 2 / 64;
                    vertices[point + 1] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0); colors[point + 1] = new Color(1, 1, 1, 0.4f);
                    triangles[point * 3] = 0; triangles[point * 3 + 1] = point + 1; triangles[point * 3 + 2] = (point + 1) % 64 + 1;
                }
                disc = new Mesh { name = "ExpandedSoftField", vertices = vertices, colors = colors, triangles = triangles }; disc.RecalculateBounds();
            }
            var wash = new GameObject("SoftFieldWash"); wash.transform.SetParent(transform, false);
            wash.AddComponent<MeshFilter>().sharedMesh = disc; MeshRenderer renderer = wash.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material; renderer.sortingOrder = 61; fills.Add(renderer); return line;
        }
        private void OnDestroy() { if (material != null) Destroy(material); if (disc != null) Destroy(disc); if (beam != null) Destroy(beam); }
    }
}
