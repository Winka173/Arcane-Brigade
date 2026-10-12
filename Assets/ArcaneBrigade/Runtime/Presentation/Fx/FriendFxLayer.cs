using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>One FxBatch for our side's small, constant effects: element trails on friends' ordinary shots (ShotTrails) and the
    /// cast tell on a friend's weapon just before its skill (CastTells). Skill casts arrive with no lead time, so the tell reads the
    /// skill bar instead: it measures how fast each friend's bar fills and lights the weapon when the bar is ~0.3 s from full.
    /// Presentation only: it reads the bars, it never delays or changes a cast. A bar that empties without a cast (a once-per-wave
    /// skill) silences that friend's tell until it really casts again.</summary>
    public sealed class FriendFxLayer : MonoBehaviour
    {
        private FxBatch batch;
        private int drawnFrame = -1;
        private struct Bar { public float Frac, Time, Rate, FiredAt, FiredBar; public bool Live, Silent; public Vector2 Aim; }
        private readonly Dictionary<int, Bar> bars = new Dictionary<int, Bar>();
        public int Tells { get; private set; }

        public static FriendFxLayer Create(Transform parent)
        {
            var go = new GameObject("Friend shot trails and cast tells"); go.transform.SetParent(parent, false);
            var layer = go.AddComponent<FriendFxLayer>(); layer.batch = new FxBatch(go.transform, 2141, 2140); return layer;
        }

        public FxBatch Begin() { batch.Begin(); drawnFrame = Time.frameCount; return batch; }
        public void End() => batch.End();
        // nothing drawn this frame (out of combat): clear what the last frame left
        private void LateUpdate() { if (drawnFrame < Time.frameCount - 1 && batch != null && batch.Quads > 0) { batch.Begin(); batch.End(); } }

        public void Shot(ElementLook look, Vector2 at, Vector2 velocity, int id, float now) => ShotTrails.Draw(batch, look, at, velocity, id, now);

        /// <summary>A friend's skill went off (CompanionSkillRuntime.CastRequested).</summary>
        public void Fired(int sourceId, Vector2 target, float now, float barTime)
        {
            bars.TryGetValue(sourceId, out Bar s);
            s.FiredAt = now; s.FiredBar = barTime; s.Silent = false; s.Aim = target; bars[sourceId] = s;
        }

        /// <summary>Draws each friend's cast tell (or the release just after its cast). <paramref name="lookOf"/>: the friend's bond element.</summary>
        public void DrawTells(CombatSimulation sim, CompanionSkillRuntime skills, float now, Func<int, ElementLook> lookOf)
        {
            Tells = 0;
            if (sim == null || skills == null) return;
            float clock = skills.Time;
            IReadOnlyList<UnitState> units = sim.Units;
            for (int i = 0; i < units.Count; i++)
            {
                UnitState u = units[i];
                if (u == null || u.Faction != Faction.Companion || !u.Alive) continue;
                float frac = skills.BarFraction(u.Id);
                bars.TryGetValue(u.Id, out Bar s);
                if (s.Live && clock < s.Time - .001f) { s = default; s.FiredAt = -10; } // a new wave restarted the bars
                float dt = clock - s.Time;
                if (s.Live && dt > 0)
                {
                    if (frac < s.Frac - .25f)
                    {   // the bar emptied: a cast should have come with it
                        if (s.Frac > .8f && clock - s.FiredBar > .15f) s.Silent = true;
                    }
                    else { float r = (frac - s.Frac) / dt; s.Rate = s.Rate <= 0 ? r : Mathf.Lerp(s.Rate, r, .35f); }
                }
                if (!s.Live || dt > 0) { s.Frac = frac; s.Time = clock; s.Live = true; if (s.FiredAt == 0) s.FiredAt = -10; }
                bars[u.Id] = s;

                float since = now - s.FiredAt;
                bool releasing = since >= 0 && since < CastTells.ReleaseTime;
                float left = s.Rate > .0001f ? (1 - frac) / s.Rate : 99, charge = s.Silent || frac <= 0 || since < .2f ? 0 : Mathf.Clamp01(1 - left / CastTells.Lead);
                if (charge <= 0 && !releasing) continue;
                if (!Tip(u, out Vector2 tip)) continue;
                ElementLook look = lookOf(u.Id); if (look == ElementLook.None) look = ElementLook.Gold;
                if (releasing) CastTells.Release(batch, look, tip, s.Aim - tip, since, u.Id);
                else { CastTells.Charge(batch, look, tip, charge, now, u.Id); Tells++; }
            }
        }

        private static bool Tip(UnitState u, out Vector2 tip)
        {
            if (UnitVisual.TryMuzzle(u.Id, out tip)) return true;
            if (!UnitVisual.TryDrawn(u.Id, out Vector2 at)) return false;
            Vector2 facing = u.Facing.sqrMagnitude > .01f ? u.Facing.normalized : Vector2.right;
            tip = at + Vector2.up * .45f + facing * .28f; return true;
        }

        // ---- effects sandbox demo entries ----------------------------------------------------------------------------------------
        private static readonly ElementLook[] Looks = { ElementLook.Fire, ElementLook.Ice, ElementLook.Water, ElementLook.Storm, ElementLook.Void, ElementLook.Light, ElementLook.Wind, ElementLook.Poison,
            ElementLook.Earth, ElementLook.Bloom, ElementLook.Crystal, ElementLook.Gold, ElementLook.Paper, ElementLook.Thread, ElementLook.Sound, ElementLook.Time, ElementLook.Dream };
        private static readonly Vector2[] Spots = { new Vector2(-6.4f, -.8f), new Vector2(-3.5f, -.9f), new Vector2(-6.3f, -3.5f), new Vector2(-3.6f, -3.6f) };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Sandbox()
        {
            FxSandbox.Register("Friend shots", "All elements", at => { for (int i = 0; i < Spots.Length; i++) Volley(Looks[(i * 5 + UnityEngine.Random.Range(0, 17)) % Looks.Length], Spots[i], at, i); });
            FxSandbox.Register("Cast tells", "All friends", at => { for (int i = 0; i < Spots.Length; i++) Tell(Looks[(i * 4 + UnityEngine.Random.Range(0, 17)) % Looks.Length], Spots[i], at, i * .35f); });
            foreach (ElementLook look in Looks)
            {
                ElementLook l = look;
                FxSandbox.Register("Friend shots", l.ToString(), at => { for (int i = 0; i < Spots.Length; i++) Volley(l, Spots[i], at, i); });
                FxSandbox.Register("Cast tells", l.ToString(), at => Tell(l, FxSandbox.Friend, at, 0));
            }
        }

        /// <summary>A friend fires a stream of ordinary shots into the crowd.</summary>
        private static void Volley(ElementLook look, Vector2 from, Vector2 crowd, int lane)
        {
            Vector2 muzzle = from + new Vector2(.3f, .35f), target = crowd + new Vector2(-.6f + lane * .4f, (lane - 1.5f) * .5f);
            Vector2 d = target - muzzle; float len = d.magnitude, speed = 9; Vector2 v = d / Mathf.Max(.01f, len) * speed;
            for (int k = 0; k < 6; k++)
            {
                int id = lane * 31 + k * 7 + UnityEngine.Random.Range(0, 999);
                FxSandbox.Later(k * .22f, () => FxSandbox.Draw(len / speed, (b, t, now) =>
                {
                    Vector2 at = muzzle + v * t;
                    ShotTrails.Draw(b, look, at, v, id, now);
                    b.Quad(FxShape.Glow, at, .1f, FxKit.A(Color.Lerp(RulesV3Ink.LookColour(look), Color.white, .5f), .9f), 0, id); // stands in for the painted token
                }));
            }
        }

        /// <summary>A friend's weapon gathers its element for 0.3 s, then the skill goes off toward the crowd.</summary>
        private static void Tell(ElementLook look, Vector2 friend, Vector2 crowd, float delay)
        {
            Vector2 tip = friend + new Vector2(.3f, .35f); int seed = UnityEngine.Random.Range(1, 999);
            FxSandbox.Later(delay, () => FxSandbox.Draw(CastTells.Lead + CastTells.ReleaseTime, (b, t, now) =>
            {
                if (t < CastTells.Lead) CastTells.Charge(b, look, tip, t / CastTells.Lead, now, seed);
                else CastTells.Release(b, look, tip, crowd - tip, t - CastTells.Lead, seed);
            }));
        }
    }
}
