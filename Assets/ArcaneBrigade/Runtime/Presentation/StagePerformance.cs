using UnityEngine;

namespace ArcaneBrigade
{
    [DefaultExecutionOrder(-40)]
    public sealed class StagePerformance : MonoBehaviour
    {
        private ArenaController owner;
        private float impactAt = -100;
        private float lightAt = -100;
        private float strength;
        private Vector2 position;
        private Vector2 lightPosition;
        private Color lightColor;
        private int bossId = -1;
        private int act;
        private float actAt = -100;
        private float nextDisturbance;
        private TactileAnimationLibrary art;
        private TactileFeedback feedback;
        public static StagePerformance Active { get; private set; }
        public int BossAct => act;
        public float Impact => owner == null || PresentationSettings.ReducedMotion ? 0 : AnimationTiming.Settle(owner.PresentationClock - impactAt, .8f, 3) * strength;
        public float ActGesture => owner == null || PresentationSettings.ReducedMotion ? 0 : AnimationTiming.Pulse(owner.PresentationClock - actAt, 1.6f);
        public Vector2 ImpactPosition => position;
        public Vector4 Light
        {
            get
            {
                float age = owner == null ? 1 : owner.PresentationClock - lightAt;
                float glow = PresentationSettings.ReducedFlashes || PresentationSettings.ReducedEffects ? 0 : AnimationTiming.Pulse(age, .38f) * .22f;
                return new Vector4(lightPosition.x, lightPosition.y, 3.5f, glow);
            }
        }
        public Color LightColor => lightColor;
        public void Configure(ArenaController controller) { owner = controller; Active = this; art = TactileAnimationLibrary.Current; }
        public void Observe(CombatEvent value)
        {
            if (owner == null || owner.PresentationPaused) return;
            bool heavy = value.Weapon != null && (value.Weapon.Motion == WeaponMotion.Hammer || value.Weapon.Motion == WeaponMotion.Slam || value.Weapon.Motion == WeaponMotion.Stamp);
            if (value.Kind == CombatEventKind.BossLanded || value.Kind == CombatEventKind.BossPhase || value.Kind == CombatEventKind.Kill && value.Subject?.Definition.IsBoss == true || value.Kind == CombatEventKind.Hit && heavy && value.Value > 0)
            {
                float now = owner.PresentationClock;
                if (now - impactAt < .16f) return;
                impactAt = now; position = value.Position; strength = value.Kind == CombatEventKind.Hit ? .3f : .9f;
                if (now >= nextDisturbance && art != null)
                {
                    nextDisturbance = now + .65f;
                    if (feedback == null) feedback = owner.GetComponent<TactileFeedback>();
                    for (int index = 0; index < 2; index++)
                    {
                        Vector2 shelf = owner.Session == null ? Vector2.zero : NightmareLighting.Landmark(owner, index == 0 ? 3 : 4, owner.Session.State.Phase == SessionPhase.Shop);
                        if (Vector2.Distance(shelf, position) < 8) feedback?.Emit(art.Dust, shelf, 1, .85f, new Color(.6f, .5f, .48f, .28f), Vector2.down * .8f, 0, -29);
                    }
                }
            }
            if (value.Kind == CombatEventKind.Fire && value.Element != DamageElement.Physical && owner.PresentationClock - lightAt > .16f)
            {
                lightAt = owner.PresentationClock; lightPosition = value.Position;
                lightColor = ArcadeEffects.ElementColor(value.Element);
            }
        }
        private void Update()
        {
            if (owner == null || owner.Simulation == null || owner.PresentationPaused) return;
            UnitState boss = owner.Simulation.Boss;
            if (boss == null) { bossId = -1; act = 0; }
            else
            {
                int current = AnimationTiming.BossAct(boss.Health, boss.Definition.Health);
                if (bossId != boss.Id) { bossId = boss.Id; act = current; actAt = -100; }
                else if (boss.Alive && current > act) { act = current; actAt = owner.PresentationClock; }
            }
            Shader.SetGlobalVector("_StageImpulse", new Vector4(position.x, position.y, Mathf.Abs(Impact), act));
        }
        private void OnDestroy() { if (Active == this) { Active = null; Shader.SetGlobalVector("_StageImpulse", Vector4.zero); } }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetState() { Active = null; Shader.SetGlobalVector("_StageImpulse", Vector4.zero); }
    }
}
