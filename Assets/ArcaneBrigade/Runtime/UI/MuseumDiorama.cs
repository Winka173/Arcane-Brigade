using System;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class MuseumDiorama : IDisposable
    {
        private GameObject root;
        private Camera camera;
        private RenderTexture texture;
        private UnitDefinition definition;
        private WeaponDefinition weapon;
        private UnitVisual visual;
        private UnitState state;
        private UnitDefinition source;
        private int tier;
        private float triggered = -100;
        public int Pose { get; set; }
        public int BrokenMask { get; set; }
        public void Draw(Rect rect, UnitDefinition unit, CheckpointCatalog catalog, int stars)
        {
            if (unit != source || stars != tier) Create(unit, catalog, stars);
            if (Event.current.type == EventType.Repaint)
            {
                float time = Time.unscaledTime;
                state.PreviousPosition = state.Position;
                state.Position = Pose == 1 && !PresentationSettings.ReducedMotion ? new Vector2(Mathf.Sin(time * 1.5f) * 0.1f, 0) : Vector2.zero;
                state.Velocity = Pose == 1 ? Vector2.right : Vector2.zero;
                state.Facing = Vector2.right;
                if (definition.IsBoss)
                {
                    if ((BrokenMask & 1) != 0) { if (!state.LeftPartBroken) state.LeftPartBrokenAt = time; } else state.LeftPartBrokenAt = -100;
                    if ((BrokenMask & 2) != 0) { if (!state.RightPartBroken) state.RightPartBrokenAt = time; } else state.RightPartBrokenAt = -100;
                    if ((BrokenMask & 4) != 0) { if (state.CenterPartBrokenAt < 0) state.CenterPartBrokenAt = time; } else state.CenterPartBrokenAt = -100;
                    state.ActiveBossOrgan = Mathf.FloorToInt(time / 2.4f) % 3;
                }
                if (Pose == 2)
                {
                    if (time - triggered > 2.4f) triggered = time;
                    state.AttackStarted = triggered;
                    state.PendingAttack = time - triggered < definition.Weapon.Windup;
                    state.AttackDirection = Vector2.right;
                    if (definition.IsBoss) state.TelegraphUntil = time - triggered < 0.85f ? triggered + 0.85f : 0;
                    if (definition.IsBoss) state.BossOrganReleaseAt = triggered + 0.85f;
                }
                else { state.PendingAttack = false; state.AttackStarted = -100; state.TelegraphUntil = 0; }
                if (Pose == 3)
                {
                    if (time - triggered > 2.2f) triggered = time;
                    state.LastHit = triggered; state.HitDirection = Vector2.left;
                }
                else state.LastHit = -100;
                visual.ReducedMotion = PresentationSettings.ReducedMotion;
                visual.Display(time, 1);
                Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
                try
                {
                    foreach (Renderer renderer in renderers) { renderer.gameObject.layer = 31; renderer.forceRenderingOff = false; }
                    camera.Render();
                }
                finally { foreach (Renderer renderer in renderers) renderer.forceRenderingOff = true; }
            }
            GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit, true);
        }
        private void Create(UnitDefinition unit, CheckpointCatalog catalog, int stars)
        {
            Dispose(); source = unit; tier = stars; BrokenMask = 0;
            definition = UnityEngine.Object.Instantiate(unit);
            weapon = UnityEngine.Object.Instantiate(unit.Weapon); definition.Weapon = weapon;
            definition.CombatTier = stars;
            if (!unit.IsBoss) { definition.BodyHeight *= CompanionTierScaling.Body(stars); CompanionTierScaling.Apply(weapon, stars); }
            state = new UnitState { Id = 19, Definition = definition, Health = definition.Health, Faction = unit.IsBoss ? Faction.Enemy : Faction.Companion };
            root = new GameObject("CollectionDiorama");
            var cameraObject = new GameObject("CollectionCamera"); cameraObject.transform.SetParent(root.transform, false);
            cameraObject.transform.localPosition = new Vector3(0.18f, 0, -10);
            camera = cameraObject.AddComponent<Camera>(); camera.enabled = false; camera.orthographic = true;
            camera.orthographicSize = Mathf.Max(0.65f, definition.BodyHeight * 0.7f);
            camera.cullingMask = 1 << 31; camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.clear;
            texture = new RenderTexture(768, 384, 16, RenderTextureFormat.ARGB32) { antiAliasing = 4, name = "CollectionPreview" };
            texture.Create(); camera.targetTexture = texture;
            var actor = new GameObject("ExhibitActor"); actor.transform.SetParent(root.transform, false);
            visual = actor.AddComponent<UnitVisual>(); visual.CreateHierarchy(null); visual.Bind(state, catalog);
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true)) { renderer.gameObject.layer = 31; renderer.forceRenderingOff = true; }
        }
        public void Dispose()
        {
            if (camera != null) camera.targetTexture = null;
            if (root != null) { root.SetActive(false); UnityEngine.Object.Destroy(root); }
            if (texture != null) { texture.Release(); UnityEngine.Object.Destroy(texture); }
            if (definition != null) UnityEngine.Object.Destroy(definition);
            if (weapon != null) UnityEngine.Object.Destroy(weapon);
            root = null; camera = null; texture = null; source = null; visual = null; definition = null; weapon = null;
        }
    }
}
