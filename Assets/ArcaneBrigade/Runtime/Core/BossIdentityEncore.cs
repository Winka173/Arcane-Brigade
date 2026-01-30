using System;
using UnityEngine;

namespace ArcaneBrigade
{
    public static class BossIdentityEncore
    {
        public static string Name(int identity) => identity switch
        {
            0 => "Unclaimed Parade", 1 => "Forbidden Margins", 2 => "Dream Stepping Stones",
            3 => "Anchor Tramway", 4 => "Closing Shutters", 5 => "Unstitched Curtain",
            6 => "Cradle Without a Floor", 7 => "The Missing Coin", 8 => "Broken Measure",
            9 => "Baggage Conveyor", 11 => "The Hand Leaves One Finger", 12 => "Sign Here, Not There", _ => null
        };
    }

    public sealed partial class CombatSimulation
    {
        public void ReviewIdentityEncore(int cast)
        {
            if (!RunPaths.K7Capture || Boss == null) throw new InvalidOperationException("Encore review requires an isolated diagnostic boss");
            int identity = Boss.Definition.Id == "OutsideStageKeeper" ? 11 : Boss.Definition.Id == NightmareRules.CustomerId ? 12 : Boss.Definition.BossPattern;
            ResolveIdentityEncore(Boss, cast, identity, (position, angle, delay, speed, motion) => { });
            Boss.EncoreWindupUntil = Elapsed + 1.05f; Boss.EncoreRecoveryUntil = Elapsed + 1.5f;
        }
        private bool ResolveIdentityEncore(UnitState boss, int cast, int identity, Action<Vector2, float, float, float, HostileShot> shot)
        {
            if (BossIdentityEncore.Name(identity) == null) return false;
            Vector2 target = ClampPosition(Champion.Position, 1.2f);
            Vector2 heading = (target - boss.Position).normalized;
            if (heading.sqrMagnitude < .01f) heading = Vector2.down;
            Vector2 across = new Vector2(-heading.y, heading.x);
            float side = cast % 2 == 0 ? 1 : -1;
            void Prop(Vector2 at, float delay, float radius = .65f) => Danger(ExpandedFieldKind.FallingProp, boss, at, radius, delay, .38f);
            void Ink(Vector2 at, float delay, float radius = .75f) => Danger(ExpandedFieldKind.ShadowPool, boss, at, radius, delay, 1.15f);
            void Lash(Vector2 from, Vector2 to, float delay, float width = .25f)
            {
                int before = fields.Count;
                Danger(ExpandedFieldKind.Tentacle, boss, to, width, delay, .36f);
                if (fields.Count > before) { fields[fields.Count - 1].Start = ClampPosition(from, .1f); fields[fields.Count - 1].Direction = (to - from).normalized; }
            }
            switch (identity)
            {
                case 0:
                    for (int row = 0; row < 3; row++) for (int column = -2; column <= 2; column++)
                        if (column != (row + cast) % 3 - 1) Prop(target + across * column * 1.65f + heading * (row - 1) * 1.8f, 1.05f + row * .38f, .56f);
                    break;
                case 1:
                    for (int stroke = 0; stroke < 3; stroke++)
                    {
                        float offset = (stroke - 1) * 2.1f;
                        Lash(target + across * offset - heading * 3.5f, target + across * offset + heading * 3.5f, 1.15f + stroke * .42f);
                    }
                    Ink(target - across * side * 2.1f, 2.8f);
                    break;
                case 2:
                    for (int step = 0; step < 4; step++)
                    {
                        Vector2 landing = target + heading * (step - 1.5f) * 1.5f + across * (step % 2 == 0 ? -1 : 1) * 1.2f;
                        Prop(landing, 1.05f + step * .4f, .8f);
                    }
                    break;
                case 3:
                    for (int rail = -1; rail <= 1; rail += 2)
                        Lash(boss.Position + across * rail * 2.3f, boss.Position + heading * 9 + across * rail * 2.3f, 1.2f, .35f);
                    for (int anchor = 0; anchor < 3; anchor++) Prop(target + heading * (anchor - 1) * 2, 1.85f + anchor * .25f);
                    break;
                case 4:
                    for (int shutter = 0; shutter < 3; shutter++)
                    {
                        float offset = (3.6f - shutter * .8f) * side;
                        Lash(target + across * offset - heading * 3, target + across * offset + heading * 3, 1.15f + shutter * .45f, .35f);
                        Lash(target - across * offset - heading * 3, target - across * offset + heading * 3, 1.15f + shutter * .45f, .35f);
                    }
                    break;
                case 5:
                    for (int fold = 0; fold < 5; fold++)
                    {
                        Vector2 seam = target + across * (fold - 2) * 1.3f;
                        Lash(seam + heading * (fold % 2 == 0 ? -4 : 1), seam + heading * (fold % 2 == 0 ? -1 : 4), 1.1f + fold * .26f, .3f);
                    }
                    break;
                case 6:
                    for (int cradle = 0; cradle < 3; cradle++)
                    {
                        Vector2 island = target + across * (cradle - 1) * 2.2f + heading * (cradle % 2 == 0 ? -1 : 1);
                        Ink(island, 1.05f + cradle * .65f, .95f);
                        Prop(island + heading * 1.65f, 1.5f + cradle * .65f, .55f);
                    }
                    break;
                case 7:
                    for (int coin = 0; coin < 8; coin++)
                    {
                        if (coin == cast % 8 || coin == (cast + 1) % 8) continue;
                        Vector2 offset = Quaternion.Euler(0, 0, coin * 45) * Vector2.right * 2.7f;
                        Prop(target + offset, 1.1f + coin * .14f, .55f);
                    }
                    Lash(boss.Position, target, 2.8f, .3f);
                    break;
                case 8:
                    for (int beat = 0; beat < 4; beat++)
                    {
                        float offset = (beat - 1.5f) * 1.7f;
                        Lash(target + across * offset - heading * 4, target + across * offset + heading * 4, 1.1f + beat * .5f, .2f);
                        if (beat % 2 == 0) Prop(target + across * offset + heading * 2, 1.45f + beat * .5f, .45f);
                    }
                    break;
                case 9:
                    for (int luggage = 0; luggage < 6; luggage++)
                        Prop(target + across * (luggage - 2.5f) * 1.3f + heading * (luggage % 2 == 0 ? 1.1f : -1.1f), 1.05f + luggage * .25f, .72f);
                    Ink(target - heading * 2.8f, 2.9f, .9f);
                    break;
                case 11:
                    for (int finger = -2; finger <= 2; finger++)
                        if (finger != cast % 3 - 1) Lash(target + across * finger * 1.65f - heading * 4, target + across * finger * 1.65f + heading * 3, 1.35f + Mathf.Abs(finger) * .12f, .34f);
                    Prop(target - heading * 3, 2.7f, 1.15f);
                    break;
                case 12:
                    Vector2 corner = target + across * side * 2.4f;
                    Lash(corner - heading * 2.5f, corner + heading * 2.5f, 1.25f, .28f);
                    Lash(corner + heading * 2.5f, corner + heading * 2.5f - across * side * 4.8f, 1.8f, .28f);
                    Lash(corner - heading * 2.5f, corner - heading * 2.5f - across * side * 3, 2.35f, .28f);
                    Prop(corner - across * side * 2, 2.9f, .7f);
                    break;
            }
            float aim = Mathf.Atan2(heading.y, heading.x) * Mathf.Rad2Deg;
            if (identity == 0 || identity == 3 || identity == 7 || identity == 8)
                for (int bullet = -2; bullet <= 2; bullet++) if (bullet != 0) shot(boss.Position, aim + bullet * 18, .45f + Mathf.Abs(bullet) * .1f, 1.85f, identity == 7 ? HostileShot.Hook : HostileShot.Straight);
            events.Add(new CombatEvent(CombatEventKind.BossPhase, boss.Position, Faction.Enemy, value: identity, subject: boss, cause: NewCause(boss.Id).Child("boss-identity-encore")));
            return true;
        }
    }
}
