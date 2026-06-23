using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    // The Post Office (waves 5-8; Docs/Map_Mechanics_2026-10-25.md section 2). Requirement 25/10/2026: each map's state, tunings
    // and rules live in its own file; MapEnvironmentCombat and MapBoosts only dispatch to the hooks below.

    public sealed partial class MapEnvironmentState
    {
        // Post Office: the mail chute, the paper gust, the giant parcel; the letter's boost and the stamped enemies
        public sealed class Letter { public Vector2 Position; public float Born; public bool Gold, Gone; public int Style; }
        public float ChuteAt = -100, NextChute, ChuteX, ChuteTop, ChuteBottom;
        public int LettersToDrop; public float NextLetter;
        public readonly List<Letter> Letters = new List<Letter>();
        public float GustWarnAt = -100, NextGust, GustY; public int GustDirection = 1;
        public float GustAt => GustWarnAt + MapEnvironmentRules.GustWarning;
        public float GustUntil => GustAt + MapEnvironmentRules.GustSeconds;
        public int ParcelState; // 0 not yet, 1 falling, 2 sitting, 3 broken open (gold), 4 opened into an elite
        public Vector2 Parcel; public float ParcelDue, ParcelAt, HatchAt, ParcelBreak;
        public int BoostKind = -1; public float BoostUntil;
        /// <summary>Letters caught from the chute's current drop, and every catch so far (the presentation counts them over the hero).</summary>
        public int LetterStreak, LettersCaught;
        public readonly Dictionary<int, float> Stamped = new Dictionary<int, float>();
    }

    public static partial class MapEnvironmentRules
    {
        public const int FullDelivery = 3; public const float FullDeliveryBonus = 4;
        public const float ChuteEvery = 13, ChuteWarn = 1.2f, LetterSpeed = 3.1f, BoostSeconds = 8, GoldBoostSeconds = 11, StampSeconds = 6, StampDamage = 1.2f;
        // the Post Office is the open floor with the strong draught (requirement 23/10/2026): wider, stronger gusts (were 1.3 / 1.4 / 1.1)
        public const float GustEvery = 11, GustWarning = 1, GustSeconds = 4, GustHalf = 1.7f, GustShotPush = 1.8f, GustBodyPush = 1.4f, LightBody = .38f;
        public const float ParcelShare = .3f, ParcelWarn = 1.3f, ParcelHatch = 11, ParcelBreakSeconds = 1.8f, ParcelReach = 1.5f;
        public const float BoostSpeed = 1.15f, BoostDamage = 1.15f;
        // requirement 23/10/2026: a broken parcel bursts
        public const float ParcelBurstRadius = 2.8f, ParcelBurstHits = 4f;
    }

    public sealed partial class CombatSimulation
    {
        private void InitPostOffice(MapEnvironmentState s)
        {
            s.NextChute = 5; s.NextGust = 8;
            s.ParcelDue = Mathf.Max(6, catalog.WaveDuration * MapEnvironmentRules.ParcelShare);
        }

        // requirement 23/10/2026: nothing on a map blocks or pushes anyone - the gusts are gone
        private void StepPostOffice(float deltaTime) { StepChute(deltaTime); StepParcel(deltaTime); }

        /// <summary>A stat the hero gets while standing somewhere on this map (MapBoosts.MapStandingBoost).</summary>
        private float PostOfficeStandingBoost(MapStat stat) => 0;
        /// <summary>How this map changes one enemy along one axis (1 = unchanged): a stamped enemy, or any enemy while the hero
        /// holds the letter's sharp boost, takes more damage.</summary>
        private float PostOfficeEnemyFactor(UnitState unit, MapEnemyAxis axis)
        {
            MapEnvironmentState s = mapEnv;
            if (axis != MapEnemyAxis.DamageTaken) return 1;
            float factor = 1;
            if (s.Stamped.Count > 0 && s.Stamped.TryGetValue(unit.Id, out float stamped) && stamped > Elapsed) factor *= MapEnvironmentRules.StampDamage;
            if (s.BoostKind == 1 && s.BoostUntil > Elapsed) factor *= MapEnvironmentRules.BoostDamage;
            return factor;
        }
        /// <summary>The attack cooldown factor of the hero or a friend on this map (below 1 = faster).</summary>
        private float PostOfficeAllyCycle(UnitState unit) => 1;
        /// <summary>The hero's pace on this map beyond the move-speed boosts: the letter's quick boost.</summary>
        private float PostOfficeHeroPace() => mapEnv.BoostKind == 0 && mapEnv.BoostUntil > Elapsed ? MapEnvironmentRules.BoostSpeed : 1;
        /// <summary>A fallen enemy's coins on this map: one more with the letter's rich boost.</summary>
        private int PostOfficeCoins(UnitState target, int coins) => mapEnv.BoostKind == 2 && mapEnv.BoostUntil > Elapsed && !target.SwarmMinion ? coins + 1 : coins;

        private void StepChute(float deltaTime)
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            if (t >= s.NextChute)
            {
                s.NextChute = t + MapEnvironmentRules.ChuteEvery; s.ChuteAt = t;
                float side = MapNext(1.5f, 4) * (mapRandom.Next(2) == 0 ? -1 : 1);
                Vector2 half = catalog.ArenaSize * .5f - Vector2.one;
                s.ChuteX = Mathf.Clamp(Champion.Position.x + side, -half.x, half.x);
                s.ChuteTop = Mathf.Min(half.y, Champion.Position.y + 4.6f); s.ChuteBottom = Mathf.Max(-half.y, Champion.Position.y - 5.2f);
                s.LettersToDrop = 5; s.NextLetter = t + MapEnvironmentRules.ChuteWarn; s.LetterStreak = 0;
                s.Shown.Add(MapIdentity.Element.Chute);
            }
            if (s.LettersToDrop > 0 && t >= s.NextLetter)
            {
                s.Letters.Add(new MapEnvironmentState.Letter { Position = new Vector2(s.ChuteX + MapNext(-.25f, .25f), s.ChuteTop), Born = t, Gold = s.LettersToDrop == 3, Style = mapRandom.Next(3) });
                s.LettersToDrop--; s.NextLetter = t + .45f;
            }
            for (int i = s.Letters.Count - 1; i >= 0; i--)
            {
                MapEnvironmentState.Letter letter = s.Letters[i];
                letter.Position += new Vector2(Mathf.Sin((t - letter.Born) * 3 + i) * .35f * deltaTime, -MapEnvironmentRules.LetterSpeed * deltaTime);
                if (letter.Position.y < s.ChuteBottom) { s.Letters.RemoveAt(i); continue; }
                if ((letter.Position - Champion.Position).sqrMagnitude < Mathf.Pow(Champion.Definition.Radius + .45f, 2))
                {
                    s.BoostKind = mapRandom.Next(3); s.BoostUntil = t + (letter.Gold ? MapEnvironmentRules.GoldBoostSeconds : MapEnvironmentRules.BoostSeconds);
                    // requirement 25/10/2026: catches count up (x1, x2...); three from one drop is a full delivery, a longer boost
                    s.LetterStreak++; s.LettersCaught++;
                    if (s.LetterStreak == MapEnvironmentRules.FullDelivery) s.BoostUntil += MapEnvironmentRules.FullDeliveryBonus;
                    s.Letters.RemoveAt(i); continue;
                }
                foreach (UnitState u in units)
                    if (u.Alive && u.Faction == Faction.Enemy && (u.Position - letter.Position).sqrMagnitude < Mathf.Pow(u.Definition.Radius + .35f, 2))
                        s.Stamped[u.Id] = t + MapEnvironmentRules.StampSeconds;
            }
        }

        private void StepGust(float deltaTime)
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            if (t >= s.NextGust)
            {
                s.NextGust = t + MapEnvironmentRules.GustEvery; s.GustWarnAt = t;
                s.GustY = Mathf.Clamp(Champion.Position.y + MapNext(-2, 2), -catalog.ArenaSize.y * .5f + 1.5f, catalog.ArenaSize.y * .5f - 1.5f);
                s.GustDirection = mapRandom.Next(2) == 0 ? -1 : 1;
                s.Shown.Add(MapIdentity.Element.Gust);
            }
            if (t < s.GustAt || t >= s.GustUntil) return;
            float push = s.GustDirection * deltaTime;
            foreach (ProjectileState p in projectiles)
                if (Mathf.Abs(p.Position.y - s.GustY) < MapEnvironmentRules.GustHalf) p.Position += new Vector2(push * MapEnvironmentRules.GustShotPush, 0);
            foreach (UnitState u in units)
                if (u.Alive && u.Faction == Faction.Enemy && !u.Definition.IsBoss && !u.Elite && u.Definition.Radius <= MapEnvironmentRules.LightBody && Mathf.Abs(u.Position.y - s.GustY) < MapEnvironmentRules.GustHalf)
                    u.Position = ClampPosition(u.Position + new Vector2(push * MapEnvironmentRules.GustBodyPush, 0), u.Definition.Radius);
        }

        private void StepParcel(float deltaTime)
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            switch (s.ParcelState)
            {
                case 0:
                    if (t < s.ParcelDue) return;
                    float angle = MapNext(0, Mathf.PI * 2);
                    s.Parcel = ClampPosition(Champion.Position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * .7f) * 4.2f, 1.5f);
                    s.ParcelAt = t; s.ParcelState = 1; s.Shown.Add(MapIdentity.Element.Parcel);
                    break;
                case 1:
                    if (t < s.ParcelAt + MapEnvironmentRules.ParcelWarn) return;
                    s.ParcelState = 2; s.ParcelAt = t; s.HatchAt = t + MapEnvironmentRules.ParcelHatch; // it lands; nothing is pushed
                    break;
                case 2:
                    if ((Champion.Position - s.Parcel).sqrMagnitude < MapEnvironmentRules.ParcelReach * MapEnvironmentRules.ParcelReach)
                        s.ParcelBreak = Mathf.Min(1, s.ParcelBreak + deltaTime / MapEnvironmentRules.ParcelBreakSeconds);
                    if (s.ParcelBreak >= 1)
                    {
                        s.ParcelState = 3; s.ParcelAt = t;
                        float perCoin = Mathf.Max(.01f, KitCoinScale * Mathf.Max(.01f, catalog.CoinValueScale));
                        int coins = Mathf.Max(4, Mathf.RoundToInt(Mathf.Max(8, catalog.ChestGold) * .5f / perCoin));
                        for (int i = 0; i < 4; i++) DropLoot(LootKind.Coin, s.Parcel + new Vector2((i % 2) * .5f - .25f, (i / 2) * .4f - .2f), coins / 4 + (i == 0 ? coins % 4 : 0));
                        // it bursts: splinters of the crate hit every enemy close by (four of the hero's own hits)
                        CombatCause burst = NewCause(Champion.Id).Child("map-parcel-burst");
                        float splinters = Champion.Definition.Weapon.Damage * MapEnvironmentRules.ParcelBurstHits;
                        foreach (UnitState u in units.ToArray())
                            if (u.Alive && u.Faction == Faction.Enemy && (u.Position - s.Parcel).sqrMagnitude < MapEnvironmentRules.ParcelBurstRadius * MapEnvironmentRules.ParcelBurstRadius)
                                ApplyDamage(u, u.Definition.IsBoss ? splinters * .5f : splinters, Faction.Champion, "Parcel", u.Position - s.Parcel, cause: burst);
                        events.Add(new CombatEvent(CombatEventKind.SignaturePulse, s.Parcel, Faction.Champion, MapEnvironmentRules.ParcelBurstRadius, subject: Champion, element: DamageElement.Physical, cause: burst));
                    }
                    else if (t >= s.HatchAt)
                    {
                        s.ParcelState = 4; s.ParcelAt = t;
                        UnitDefinition kind = null;
                        if (catalog.EnemyPool != null) foreach (UnitDefinition d in catalog.EnemyPool) if (d != null && !d.IsBoss && (kind == null || d.Health > kind.Health)) kind = d;
                        kind ??= catalog.MeleeEnemy;
                        if (kind == null) return;
                        UnitDefinition elite = Object.Instantiate(kind);
                        elite.Health = kind.Health * 4.5f; elite.Radius = kind.Radius * 1.2f; elite.BodyScale = kind.BodyScale * 1.3f;
                        elite.Tint = Color.Lerp(kind.Tint, new Color(1f, .82f, .45f), .45f); elite.KnockbackResistance = Mathf.Max(kind.KnockbackResistance, .6f);
                        elite.DisplayName = "Indebted " + RunUICopy.UnitName(kind);
                        UnitState unit = Spawn(elite, Faction.Enemy, s.Parcel + Vector2.down * .3f); unit.Elite = true;
                        if (unit.Variant != null && catalog.ExpandedContent) unit.Variant.Affix = MapIdentity.Affix(s.Theme);
                    }
                    break;
            }
        }

        /// <summary>For captures: the chute, its letters, a gust, a parcel being broken and stamped enemies on one frame.</summary>
        private void PostOfficeCaptureFixture()
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            s.ChuteAt = t - 1.6f; s.ChuteX = Champion.Position.x + 2.2f; s.ChuteTop = Champion.Position.y + 4.4f; s.ChuteBottom = Champion.Position.y - 5; s.NextChute = t + 30;
            s.Letters.Clear(); for (int i = 0; i < 4; i++) s.Letters.Add(new MapEnvironmentState.Letter { Position = new Vector2(s.ChuteX + (i % 2) * .3f - .15f, s.ChuteTop - .4f - i * 1.6f), Born = t - i, Gold = i == 2, Style = i % 3 });
            s.LettersToDrop = 0;
            s.GustWarnAt = t - MapEnvironmentRules.GustWarning - .5f; s.GustY = Champion.Position.y - 1.6f; s.GustDirection = 1; s.NextGust = t + 30;
            s.Parcel = ClampPosition(Champion.Position + new Vector2(-3.2f, .6f), 1.5f); s.ParcelState = 2; s.ParcelAt = t - 2; s.HatchAt = t + 6; s.ParcelBreak = .45f;
            foreach (UnitState u in units) if (u.Alive && u.Faction == Faction.Enemy && (u.Position - Champion.Position).sqrMagnitude < 36) s.Stamped[u.Id] = t + 6;
            s.BoostKind = 1; s.BoostUntil = t + 6;
            s.Shown.Add(MapIdentity.Element.Chute); s.Shown.Add(MapIdentity.Element.Gust); s.Shown.Add(MapIdentity.Element.Parcel);
        }
    }
}
