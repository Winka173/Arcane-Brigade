using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    // The Forgotten Theatre (waves 15-17; Docs/Map_Mechanics_2026-10-25.md section 5): the three-colour spotlight, stage smoke,
    // the ghost audience, the velvet curtain, the firework machine. Requirement 25/10/2026: the map's state, tunings and rules
    // live in this file only; MapEnvironmentCombat and MapBoosts dispatch to the hooks below.

    public sealed partial class MapEnvironmentState
    {
        // Theatre: the drifting spotlight (its colour is its boost) and the encore
        public Vector2 TheatreSpot, TheatreSpotGoal;
        public int TheatreSpotColour; // 0 white (damage), 1 yellow (attack speed), 2 pink (armour)
        public float TheatreColourAt = -100, TheatreNextColour;
        public bool TheatreHeroLit;
        public float TheatreLitSince = -100, TheatreEncoreAt = -100, TheatreEncoreReady;
        // Theatre: the stage smoke banks (xy centre; they part on the drum and roll back in elsewhere)
        public readonly Vector2[] TheatreSmoke = new Vector2[4], TheatreSmokeFrom = new Vector2[4], TheatreSmokeTo = new Vector2[4];
        public float TheatreDrumAt = -100, TheatreNextDrum;
        // Theatre: the ghost audience in its row of seats at the map's edge, and what it throws
        public sealed class TheatreThrow { public Vector2 From, To; public float Born; public bool Coin, Landed, Struck; public int Gold, Ghost; }
        public Vector2 TheatreSeats;
        public float TheatreNextThrow;
        public readonly List<TheatreThrow> TheatreThrows = new List<TheatreThrow>();
        // Theatre: the velvet curtain (a band across the map; our shots it swallows are kept a moment for the presentation: x, y, time)
        public float TheatreCurtainWarnAt = -100, TheatreNextCurtain, TheatreCurtainY;
        public bool TheatreCurtainCrushed;
        public float TheatreCurtainDropAt => TheatreCurtainWarnAt + MapEnvironmentRules.TheatreCurtainWarn;
        public float TheatreCurtainUpAt => TheatreCurtainDropAt + MapEnvironmentRules.TheatreCurtainDown;
        public readonly List<Vector3> TheatreSwallowed = new List<Vector3>();
        // Theatre: the firework machine, loaded by kills inside the spotlight (each load's spark: x, y, time)
        public Vector2 TheatreMachine;
        public int TheatreLoad;
        public float TheatreFuse, TheatreFiredAt = -100, TheatreReadyAt = -100;
        public readonly List<Vector3> TheatreLoads = new List<Vector3>();
        public readonly Dictionary<int, Vector2> TheatreInLight = new Dictionary<int, Vector2>();
        public readonly HashSet<int> TheatreCounted = new HashSet<int>();
    }

    public static partial class MapEnvironmentRules
    {
        // the three-colour spotlight: 1.8 units, drifting about 1 unit/s, a new colour every 10 s; ENCORE after 3 s straight in it
        public const float TheatreSpotRadius = 1.8f, TheatreSpotDrift = 1, TheatreColourEvery = 10;
        public const float TheatreSpotDamage = .2f, TheatreSpotHaste = .2f, TheatreSpotArmor = 3;
        public const float TheatreEncoreHold = 3, TheatreEncoreSeconds = 8, TheatreEncoreCooldown = 10, TheatreEncoreGold = 1, TheatreEncoreSpeed = .15f;
        // stage smoke: a drum every 15 s parts it; enemies inside are 10% slower and ranged ones reach 25% less
        public const float TheatreDrumEvery = 15, TheatreSmokePart = 2.5f, TheatreSmokeRx = 3.3f, TheatreSmokeRy = 1.9f, TheatreSmokeSlow = .9f, TheatreSmokeRange = .75f;
        // the ghost audience: every 3 s a throw at the hero outside the light (50% of an ordinary enemy's hit), 1-3 gold inside it
        public const float TheatreThrowEvery = 3, TheatreThrowFlight = 1.05f, TheatreThrowHit = .5f, TheatreThrowReach = .75f, TheatreThrowTrack = 6.5f;
        public const int TheatreCoinsMin = 1, TheatreCoinsMax = 3;
        // the velvet curtain: every 25 s, warned 2 s by a solid area, down for 5 s; enemies under it lose 25% of max health (bosses 5%)
        public const float TheatreCurtainEvery = 25, TheatreCurtainWarn = 2, TheatreCurtainDown = 5, TheatreCurtainHalf = .8f, TheatreCurtainCrush = .25f, TheatreCurtainBossCrush = .05f;
        // the firework machine: 25 kills inside the spotlight load it; 1 s by it fires 3x the hero hit on every enemy and a 1 s freeze
        public const int TheatreFireworkLoad = 25;
        public const float TheatreFireworkHold = 1, TheatreFireworkReach = 1.5f, TheatreFireworkHits = 3, TheatreFireworkFreeze = 1;
    }

    public sealed partial class CombatSimulation
    {
        /// <summary>Sets the map up at the start of an ordinary wave (s.Anchor is the room's centre near the hero).</summary>
        private void InitTheatre(MapEnvironmentState s)
        {
            Vector2 a = s.Anchor, half = catalog.ArenaSize * .5f;
            s.TheatreSpot = s.TheatreSpotGoal = ClampPosition(a + new Vector2(-2.4f, .8f), MapEnvironmentRules.TheatreSpotRadius);
            s.TheatreNextColour = MapEnvironmentRules.TheatreColourEvery;
            // the ghost audience's row of seats sits at the bottom edge of the room, facing the stage
            s.TheatreSeats = new Vector2(a.x, Mathf.Max(a.y - 5.4f, -half.y + 1.3f));
            s.TheatreMachine = ClampPosition(a + new Vector2(7.2f, 2.6f), 1.2f);
            Vector2[] banks = { new Vector2(-4.6f, 1.6f), new Vector2(4.4f, -1.2f), new Vector2(-1.2f, -2.8f), new Vector2(1.8f, 3.4f) };
            for (int i = 0; i < banks.Length; i++) s.TheatreSmoke[i] = s.TheatreSmokeFrom[i] = s.TheatreSmokeTo[i] = a + banks[i];
            s.TheatreNextDrum = MapEnvironmentRules.TheatreDrumEvery; s.TheatreNextThrow = 4; s.TheatreNextCurtain = 11;
        }

        /// <summary>Runs the map each simulation step of the wave.</summary>
        private void StepTheatre(float deltaTime)
        {
            mapEnv.Shown.Add(MapIdentity.Element.Spotlight);
            StepTheatreSpot(deltaTime);
            StepTheatreSmoke();
            StepTheatreAudience(deltaTime);
            StepTheatreCurtain();
            StepTheatreFirework(deltaTime);
        }

        /// <summary>A stat the hero gets while standing in the spotlight: white +20% damage, yellow +20% attack speed, pink +3 armour.</summary>
        private float TheatreStandingBoost(MapStat stat)
        {
            MapEnvironmentState s = mapEnv;
            if (!InTheatreSpot(Champion.Position)) return 0;
            if (TheatreSpotStat(s.TheatreSpotColour) != stat) return 0;
            return TheatreSpotValue(s.TheatreSpotColour);
        }
        private static MapStat TheatreSpotStat(int colour) => colour == 0 ? MapStat.Damage : colour == 1 ? MapStat.AttackSpeed : MapStat.Armor;
        private static float TheatreSpotValue(int colour) => colour == 0 ? MapEnvironmentRules.TheatreSpotDamage : colour == 1 ? MapEnvironmentRules.TheatreSpotHaste : MapEnvironmentRules.TheatreSpotArmor;
        /// <summary>How this map changes one enemy along one axis (1 = unchanged): in the stage smoke it walks 10% slower and, if it
        /// shoots, reaches 25% less far.</summary>
        private float TheatreEnemyFactor(UnitState unit, MapEnemyAxis axis)
        {
            switch (axis)
            {
                case MapEnemyAxis.Speed: return InTheatreSmoke(unit.Position) ? MapEnvironmentRules.TheatreSmokeSlow : 1;
                case MapEnemyAxis.Range:
                    return unit.Definition.Weapon != null && unit.Definition.Weapon.Kind == WeaponKind.Projectile && InTheatreSmoke(unit.Position) ? MapEnvironmentRules.TheatreSmokeRange : 1;
                default: return 1;
            }
        }
        /// <summary>The attack cooldown factor of the hero or a friend on this map (below 1 = faster).</summary>
        private float TheatreAllyCycle(UnitState unit) => 1;
        /// <summary>The hero's pace on this map beyond the move-speed boosts.</summary>
        private float TheatreHeroPace() => 1;
        /// <summary>A fallen enemy's coins on this map (unchanged); a kill inside the spotlight loads the firework machine.</summary>
        private int TheatreCoins(UnitState target, int coins)
        {
            if (InTheatreSpot(target.Position)) TheatreLoadKill(target.Id, target.Position);
            return coins;
        }

        // ------------------------------------------------------------------ the spotlight and the encore
        public bool InTheatreSpot(Vector2 position)
        {
            if (mapEnv == null || mapEnv.Theme != MapTheme.Theatre) return false;
            float r = MapEnvironmentRules.TheatreSpotRadius;
            return (position - mapEnv.TheatreSpot).sqrMagnitude < r * r;
        }
        private void StepTheatreSpot(float deltaTime)
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            // it drifts about 1 unit/s from one mark to the next, always somewhere near the hero
            if ((s.TheatreSpot - s.TheatreSpotGoal).sqrMagnitude < .04f)
            {
                float angle = MapNext(0, Mathf.PI * 2), reach = MapNext(2, 5);
                s.TheatreSpotGoal = ClampPosition(Champion.Position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * .7f) * reach, MapEnvironmentRules.TheatreSpotRadius + .4f);
            }
            s.TheatreSpot = Vector2.MoveTowards(s.TheatreSpot, s.TheatreSpotGoal, MapEnvironmentRules.TheatreSpotDrift * deltaTime);
            bool lit = InTheatreSpot(Champion.Position);
            if (t >= s.TheatreNextColour)
            {
                s.TheatreSpotColour = (s.TheatreSpotColour + 1) % 3; s.TheatreColourAt = t; s.TheatreNextColour = t + MapEnvironmentRules.TheatreColourEvery;
                if (lit) TheatreSaySpot();
            }
            if (lit && !s.TheatreHeroLit) { s.TheatreLitSince = t; TheatreSaySpot(); }
            if (!lit) s.TheatreLitSince = -100;
            s.TheatreHeroLit = lit;
            // ENCORE: 3 s straight in the light lends +1 gold a kill and +15% speed for 8 s, kept after leaving it (10 s cooldown)
            if (lit && t - s.TheatreLitSince >= MapEnvironmentRules.TheatreEncoreHold && t >= s.TheatreEncoreReady)
            {
                GrantMapBoost(MapStat.Gold, MapEnvironmentRules.TheatreEncoreGold, MapEnvironmentRules.TheatreEncoreSeconds, "Encore", "ENCORE");
                GrantMapBoost(MapStat.MoveSpeed, MapEnvironmentRules.TheatreEncoreSpeed, MapEnvironmentRules.TheatreEncoreSeconds, "Encore", "ENCORE");
                s.TheatreEncoreAt = t; s.TheatreEncoreReady = t + MapEnvironmentRules.TheatreEncoreCooldown; s.TheatreLitSince = t;
            }
        }
        /// <summary>Over the hero's head: the spotlight's stat and its amount (requirement 25/10/2026: every boost says so).</summary>
        private void TheatreSaySpot()
        {
            int colour = mapEnv.TheatreSpotColour; MapStat stat = TheatreSpotStat(colour);
            MapSay(MapBoostText.Label(stat, TheatreSpotValue(colour)), MapBoostText.Colour(stat));
        }

        // ------------------------------------------------------------------ stage smoke and the drum
        public bool InTheatreSmoke(Vector2 position)
        {
            MapEnvironmentState s = mapEnv;
            if (s == null || s.Theme != MapTheme.Theatre || Elapsed < s.TheatreDrumAt + MapEnvironmentRules.TheatreSmokePart) return false; // parted by the drum
            float rx = MapEnvironmentRules.TheatreSmokeRx, ry = MapEnvironmentRules.TheatreSmokeRy;
            foreach (Vector2 bank in s.TheatreSmoke)
            {
                Vector2 d = position - bank;
                if (d.x * d.x / (rx * rx) + d.y * d.y / (ry * ry) < 1) return true;
            }
            return false;
        }
        private void StepTheatreSmoke()
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            if (t >= s.TheatreNextDrum)
            {
                // the drum booms from the pit by the seats: the smoke parts and rolls back in somewhere else round the hero
                s.TheatreDrumAt = t; s.TheatreNextDrum = t + MapEnvironmentRules.TheatreDrumEvery;
                for (int i = 0; i < s.TheatreSmoke.Length; i++)
                {
                    s.TheatreSmokeFrom[i] = s.TheatreSmoke[i];
                    Vector2 quarter = new Vector2(i % 2 == 0 ? -1 : 1, i < 2 ? 1 : -1);
                    s.TheatreSmokeTo[i] = ClampPosition(Champion.Position + Vector2.Scale(quarter, new Vector2(MapNext(1.8f, 5.6f), MapNext(1, 3.4f))), 1.5f);
                }
            }
            float p = Mathf.Clamp01((t - s.TheatreDrumAt) / MapEnvironmentRules.TheatreSmokePart);
            for (int i = 0; i < s.TheatreSmoke.Length; i++) s.TheatreSmoke[i] = Vector2.Lerp(s.TheatreSmokeFrom[i], s.TheatreSmokeTo[i], Mathf.SmoothStep(0, 1, p));
        }

        // ------------------------------------------------------------------ the ghost audience
        /// <summary>Where a ghost sits: three seat rows of three ghosts along the room's bottom edge.</summary>
        public static Vector2 TheatreGhost(MapEnvironmentState s, int ghost) => s.TheatreSeats + new Vector2((ghost / 3 - 1) * 4.2f + (ghost % 3 - 1) * .9f, .75f + (ghost / 3 == 1 ? -.25f : 0));
        private float TheatreOrdinaryHit()
        {
            UnitDefinition plain = catalog.MeleeEnemy;
            if (plain == null) return 4;
            return plain.Weapon != null && plain.Weapon.Damage > 0 ? plain.Weapon.Damage : plain.ContactDamage;
        }
        private void StepTheatreAudience(float deltaTime)
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            if (t >= s.TheatreNextThrow)
            {
                s.TheatreNextThrow = t + MapEnvironmentRules.TheatreThrowEvery;
                bool applause = InTheatreSpot(Champion.Position);
                int ghost = mapRandom.Next(9);
                var thrown = new MapEnvironmentState.TheatreThrow { From = TheatreGhost(s, ghost) + Vector2.up * .35f, Born = t, Coin = applause, Ghost = ghost };
                if (applause)
                {
                    float angle = MapNext(0, Mathf.PI * 2);
                    thrown.Gold = mapRandom.Next(MapEnvironmentRules.TheatreCoinsMin, MapEnvironmentRules.TheatreCoinsMax + 1);
                    thrown.To = ClampPosition(Champion.Position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * MapNext(.5f, 1.1f), .3f);
                }
                else thrown.To = Champion.Position;
                s.TheatreThrows.Add(thrown);
            }
            for (int i = s.TheatreThrows.Count - 1; i >= 0; i--)
            {
                MapEnvironmentState.TheatreThrow thrown = s.TheatreThrows[i];
                float age = t - thrown.Born;
                if (age > MapEnvironmentRules.TheatreThrowFlight + .9f) { s.TheatreThrows.RemoveAt(i); continue; }
                if (thrown.Landed) continue;
                // a thrown thing follows the hero for most of its flight, then falls where it was aimed: a quick step aside dodges it
                if (!thrown.Coin && age < MapEnvironmentRules.TheatreThrowFlight * .6f) thrown.To = Vector2.MoveTowards(thrown.To, Champion.Position, MapEnvironmentRules.TheatreThrowTrack * deltaTime);
                if (age < MapEnvironmentRules.TheatreThrowFlight) continue;
                thrown.Landed = true;
                if (thrown.Coin)
                {
                    float perCoin = Mathf.Max(.01f, KitCoinScale * Mathf.Max(.01f, catalog.CoinValueScale));
                    DropLoot(LootKind.Coin, thrown.To, Mathf.Max(1, Mathf.RoundToInt(thrown.Gold / perCoin)));
                }
                else if ((Champion.Position - thrown.To).sqrMagnitude < Mathf.Pow(MapEnvironmentRules.TheatreThrowReach + Champion.Definition.Radius * .5f, 2))
                {
                    thrown.Struck = true;
                    ApplyDamage(Champion, TheatreOrdinaryHit() * MapEnvironmentRules.TheatreThrowHit, Faction.Enemy, "Ghost audience", (thrown.To - thrown.From).normalized, cause: NewCause(0).Child("map-audience"));
                    if (Phase != RunPhase.Combat) return;
                }
            }
        }

        // ------------------------------------------------------------------ the velvet curtain
        private void StepTheatreCurtain()
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            if (t >= s.TheatreNextCurtain)
            {
                s.TheatreNextCurtain = t + MapEnvironmentRules.TheatreCurtainEvery; s.TheatreCurtainWarnAt = t; s.TheatreCurtainCrushed = false;
                s.TheatreCurtainY = TheatreCurtainLine();
            }
            if (t < s.TheatreCurtainDropAt || t >= s.TheatreCurtainUpAt) return;
            float half = MapEnvironmentRules.TheatreCurtainHalf, y = s.TheatreCurtainY;
            if (!s.TheatreCurtainCrushed)
            {
                // it lands: every enemy under it loses a quarter of its max health (a boss 5%)
                s.TheatreCurtainCrushed = true;
                CombatCause crush = NewCause(Champion.Id).Child("map-curtain");
                foreach (UnitState u in units.ToArray())
                    if (u.Alive && u.Faction == Faction.Enemy && Mathf.Abs(u.Position.y - y) < half + u.Definition.Radius * .5f)
                        ApplyDamage(u, u.Definition.Health * (u.Definition.IsBoss ? MapEnvironmentRules.TheatreCurtainBossCrush : MapEnvironmentRules.TheatreCurtainCrush), Faction.Champion, "Velvet curtain", Vector2.down, cause: crush);
            }
        }

        /// <summary>While the curtain hangs, our shots (the hero's and the friends') stop at it before they can hit anything beyond;
        /// enemy shots pass. Asked by MoveProjectiles before a shot moves.</summary>
        private bool TheatreStopsShot(ProjectileState p, float deltaTime)
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            if (s == null || s.Theme != MapTheme.Theatre || p.Faction == Faction.Enemy || t < s.TheatreCurtainDropAt || t >= s.TheatreCurtainUpAt) return false;
            float half = MapEnvironmentRules.TheatreCurtainHalf, y = s.TheatreCurtainY;
            Vector2 next = p.Position + p.Velocity * deltaTime;
            bool inside = Mathf.Abs(p.Position.y - y) < half;
            bool crossing = (p.Position.y - y) * (next.y - y) <= 0 || Mathf.Abs(next.y - y) < half;
            if (!inside && !crossing) return false;
            if (s.TheatreSwallowed.Count >= 16) s.TheatreSwallowed.RemoveAt(0);
            s.TheatreSwallowed.Add(new Vector3(p.Position.x, inside ? p.Position.y : y, t));
            return true;
        }
        /// <summary>Where the curtain falls: across the row near the hero with the most enemies in it (never quite on the hero).</summary>
        private float TheatreCurtainLine()
        {
            float half = MapEnvironmentRules.TheatreCurtainHalf, best = Champion.Position.y + 2.2f; int most = 0;
            for (int k = -8; k <= 8; k++)
            {
                if (Mathf.Abs(k) < 3) continue;
                float y = Champion.Position.y + k * .4f; int count = 0;
                foreach (UnitState u in StableUnits)
                    if (u.Alive && u.Faction == Faction.Enemy && Mathf.Abs(u.Position.x - Champion.Position.x) < 10 && Mathf.Abs(u.Position.y - y) < half) count++;
                if (count > most || count == most && count > 0 && Mathf.Abs(Mathf.Abs(k * .4f) - 2) < Mathf.Abs(Mathf.Abs(best - Champion.Position.y) - 2)) { most = count; best = y; }
            }
            float edge = catalog.ArenaSize.y * .5f - 1;
            return Mathf.Clamp(best, -edge, edge);
        }

        // ------------------------------------------------------------------ the firework machine
        private void TheatreLoadKill(int id, Vector2 at)
        {
            MapEnvironmentState s = mapEnv;
            if (!s.TheatreCounted.Add(id) || s.TheatreLoad >= MapEnvironmentRules.TheatreFireworkLoad) return;
            s.TheatreLoad++;
            if (s.TheatreLoads.Count >= 12) s.TheatreLoads.RemoveAt(0);
            s.TheatreLoads.Add(new Vector3(at.x, at.y, Elapsed));
            if (s.TheatreLoad >= MapEnvironmentRules.TheatreFireworkLoad) s.TheatreReadyAt = Elapsed;
        }
        private void StepTheatreFirework(float deltaTime)
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed;
            // kills inside the spotlight load it: an enemy seen in the light last step and fallen since counts (coinless ones too)
            foreach (KeyValuePair<int, Vector2> seen in s.TheatreInLight)
            {
                UnitState u = UnitWithId(seen.Key);
                if (u == null || !u.Alive) TheatreLoadKill(seen.Key, u != null ? u.Position : seen.Value);
            }
            s.TheatreInLight.Clear();
            foreach (UnitState u in StableUnits)
                if (u.Alive && u.Faction == Faction.Enemy && InTheatreSpot(u.Position)) s.TheatreInLight[u.Id] = u.Position;
            bool ready = s.TheatreLoad >= MapEnvironmentRules.TheatreFireworkLoad;
            bool near = (Champion.Position - s.TheatreMachine).sqrMagnitude < MapEnvironmentRules.TheatreFireworkReach * MapEnvironmentRules.TheatreFireworkReach;
            if (!ready || !near) { s.TheatreFuse = Mathf.Max(0, s.TheatreFuse - deltaTime * 2); return; }
            s.TheatreFuse = Mathf.Min(1, s.TheatreFuse + deltaTime / MapEnvironmentRules.TheatreFireworkHold);
            if (s.TheatreFuse < 1) return;
            // the grand finale: every enemy takes three of the hero's hits and freezes for 1 s
            s.TheatreFiredAt = t; s.TheatreLoad = 0; s.TheatreFuse = 0;
            CombatCause finale = NewCause(Champion.Id).Child("map-fireworks");
            float blast = Champion.Definition.Weapon.Damage * MapEnvironmentRules.TheatreFireworkHits;
            foreach (UnitState u in units.ToArray())
            {
                if (!u.Alive || u.Faction != Faction.Enemy) continue;
                ApplyDamage(u, blast, Faction.Champion, "Fireworks", Vector2.up, cause: finale);
                if (u.Alive) Freeze(u, MapEnvironmentRules.TheatreFireworkFreeze);
            }
        }

        /// <summary>For captures: every element of the map on screen at once (the hero in the light, smoke round it, a throw in the
        /// air, the curtain warning just above, the machine loaded and fizzing).</summary>
        private void TheatreCaptureFixture()
        {
            MapEnvironmentState s = mapEnv; float t = Elapsed; Vector2 hero = Champion.Position;
            s.TheatreSpot = s.TheatreSpotGoal = hero + new Vector2(.4f, .2f); s.TheatreSpotColour = 1; s.TheatreNextColour = t + 30;
            s.TheatreSeats = hero + new Vector2(0, -4.2f);
            Vector2[] banks = { new Vector2(-4, .8f), new Vector2(3.8f, -1.2f), new Vector2(-1.6f, -2.4f), new Vector2(2.6f, 2.6f) };
            for (int i = 0; i < banks.Length; i++) s.TheatreSmoke[i] = s.TheatreSmokeFrom[i] = s.TheatreSmokeTo[i] = hero + banks[i];
            s.TheatreDrumAt = t - .5f; s.TheatreNextDrum = t + 30;
            s.TheatreThrows.Clear();
            s.TheatreThrows.Add(new MapEnvironmentState.TheatreThrow { From = TheatreGhost(s, 1) + Vector2.up * .35f, To = hero + new Vector2(.5f, .3f), Born = t - .2f, Coin = true, Gold = 2, Ghost = 1 });
            s.TheatreThrows.Add(new MapEnvironmentState.TheatreThrow { From = TheatreGhost(s, 7) + Vector2.up * .35f, To = hero + new Vector2(-1.4f, .4f), Born = t - .6f, Ghost = 7 });
            s.TheatreNextThrow = t + 30;
            s.TheatreCurtainWarnAt = t - 1.2f; s.TheatreCurtainY = hero.y + 2.6f; s.TheatreCurtainCrushed = false; s.TheatreNextCurtain = t + 30;
            s.TheatreMachine = hero + new Vector2(4.2f, .6f); s.TheatreLoad = MapEnvironmentRules.TheatreFireworkLoad; s.TheatreReadyAt = t - 1; s.TheatreFuse = .4f;
        }
    }
}
