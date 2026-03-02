using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The Bizarre Arsenal (Docs/Bizarre_Arsenal.md §1): 24 weapons found in Bizarre Brigade research. Every attack is
    /// thrown, fired, planted ahead or swung in front; nothing radiates from the wielder's body. Delayed and planted pieces are
    /// ArsenalProps (capped, deterministic, cleared at rest); special flights are ProjectileStates tagged with Arsenal.</summary>
    public sealed partial class CombatSimulation
    {
        private readonly List<ArsenalProp> arsenalProps = new List<ArsenalProp>();
        public IReadOnlyList<ArsenalProp> ArsenalProps => arsenalProps;
        public int ArsenalBursts { get; private set; }
        private const int MaxArsenalProps = 256;

        /// <summary>The simple pool weapons a Glitch Toybox borrows from, in a fixed order (seeded picks stay deterministic).</summary>
        public static readonly string[] GlitchPool = { "InkPistol", "SpoonSabre", "AcornSling", "CrescentBow", "BubbleWand", "TeapotPopgun", "ToastCleaver", "PaperFan", "StarKazoo", "LeafCrossbow", "ButtonBow", "RubberHammer" };

        private static WeaponDefinition[] glitchWeapons;
        private static WeaponDefinition[] GlitchWeapons
        {
            get
            {
                if (glitchWeapons != null) return glitchWeapons;
                RunCatalog pool = Resources.Load<RunCatalog>("RunCatalog");
                glitchWeapons = new WeaponDefinition[GlitchPool.Length];
                for (int i = 0; i < GlitchPool.Length; i++) glitchWeapons[i] = pool != null ? pool.Weapon(GlitchPool[i]) : null;
                return glitchWeapons;
            }
        }

        private void ClearArsenal()
        {
            arsenalProps.Clear(); ArsenalBursts = 0;
            foreach (UnitState unit in units) { unit.ArsenalBeat = 0; unit.ArsenalHeat = 0; unit.Borrowed = unit.BorrowNext = null; }
        }

        private int ArsenalTier(UnitState unit) => Mathf.Clamp(unit.Definition.CombatTier, 1, 3);
        private UnitState ArsenalLocked(UnitState source) { UnitState t = UnitWithId(source.AttackTargetId); return t != null && CanHit(source.Faction, t) ? t : null; }
        private Vector2 ArsenalAim(UnitState source) => source.AttackDirection.sqrMagnitude > 0.001f ? source.AttackDirection.normalized : source.Facing;
        private static Vector2 ArsenalTurn(Vector2 v, float degrees) => Quaternion.Euler(0, 0, degrees) * v;

        private ArsenalProp PlaceProp(ArsenalPropKind kind, UnitState source, Vector2 at, float damage, float radius, bool critical, CombatCause cause)
        {
            if (arsenalProps.Count >= MaxArsenalProps) arsenalProps.RemoveAt(0);
            var prop = new ArsenalProp { Kind = kind, Source = source, Weapon = source.Definition.Weapon, Start = source.Position, Position = at, Direction = ArsenalAim(source),
                Born = Elapsed, LandAt = Elapsed, FuseAt = float.PositiveInfinity, At = Elapsed, Until = Elapsed + 6, Damage = damage, Radius = radius, Critical = critical, Cause = cause };
            arsenalProps.Add(prop); return prop;
        }
        private int OwnedProps(UnitState source, ArsenalPropKind kind) { int n = 0; foreach (ArsenalProp p in arsenalProps) if (p.Source == source && p.Kind == kind && !p.Resolved) n++; return n; }
        private void DropOldest(UnitState source, ArsenalPropKind kind, int keep)
        {
            while (OwnedProps(source, kind) > keep)
                for (int i = 0; i < arsenalProps.Count; i++)
                    if (arsenalProps[i].Source == source && arsenalProps[i].Kind == kind && !arsenalProps[i].Resolved) { arsenalProps.RemoveAt(i); break; }
        }

        /// <summary>Damage everything hostile in a circle; returns how many were struck.</summary>
        private int ArsenalBurst(UnitState source, WeaponDefinition weapon, Vector2 at, float radius, float damage, bool critical, CombatCause cause, float push = 0, float chill = 0, bool loud = true)
        {
            int count = 0;
            for (int i = 0; i < units.Count; i++)
            {
                UnitState target = units[i];
                if (!CanHit(source.Faction, target) || (target.Position - at).sqrMagnitude > (radius + target.Definition.Radius) * (radius + target.Definition.Radius)) continue;
                Vector2 away = (target.Position - at).sqrMagnitude > 0.0001f ? (target.Position - at).normalized : ArsenalAim(source);
                ApplyWeaponDamage(target, damage, source.Faction, weapon.Element, weapon.Id, away, weapon, critical, cause);
                count++;
                if (!target.Alive || target.Definition.IsBoss) continue;
                if (chill > 0) target.ChillUntil = Mathf.Max(target.ChillUntil, Elapsed + chill);
                if (push > 0) { target.Position = ClampPosition(target.Position + away * OctoberBalance.Push(target.Definition, push), target.Definition.Radius); spatial.Move(target); }
            }
            if (loud && radius >= 0.6f) { ArsenalBursts++; events.Add(new CombatEvent(CombatEventKind.Detonation, at, source.Faction, radius, ArsenalAim(source), weapon, critical, cause: cause)); }
            return count;
        }

        /// <summary>A shot of an Arsenal weapon (its own sprite and width), travelling `range` before it ends.</summary>
        private ProjectileState ArsenalShot(UnitState source, Vector2 from, Vector2 direction, float speed, float range, float damage, bool critical, CombatCause cause, float radius = 0.12f, int pierce = 0, WeaponDefinition weapon = null)
        {
            weapon ??= source.Definition.Weapon;
            direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.right;
            var shot = new ProjectileState
            {
                Id = ++nextId, Faction = source.Faction, Position = from, PreviousPosition = source.Position, LaunchSegment = true,
                Velocity = direction * speed, LaunchVelocity = direction * speed, Damage = damage, Remaining = range / Mathf.Max(0.1f, speed),
                Color = weapon.ProjectileColor, Element = weapon.Element, SourceName = source.Definition.DisplayName + " " + weapon.Id,
                Sprite = weapon.ProjectileSprite, VisualWidth = weapon.ProjectileVisualWidth, Radius = radius, Critical = critical,
                SourceUnitId = source.Id, Cause = cause, Weapon = weapon, Arsenal = weapon.Arsenal, ExtraPierce = pierce, ArsenalHome = source.Position
            };
            if (projectiles.Count < 4096) projectiles.Add(shot);
            return shot;
        }
        private Vector2 ArsenalMuzzle(UnitState source) => source.Position + ArsenalAim(source) * Mathf.Max(0.3f, source.Definition.Weapon.MuzzleDistance);

        /// <summary>The wielder's attack lands: returns true when an Arsenal weapon handled it.</summary>
        private bool ResolveArsenalAttack(UnitState source, float damage, bool critical, CombatCause cause)
        {
            WeaponDefinition weapon = source.Definition.Weapon;
            UnitState target = ArsenalLocked(source);
            Vector2 aim = ArsenalAim(source), point = target != null ? target.Position : source.Position + aim * weapon.Range * 0.8f;
            int tier = ArsenalTier(source);
            source.ArsenalBeat++;
            switch (weapon.Arsenal)
            {
                case ArsenalWeapon.CherryFuseBomb:
                    for (int i = 0; i < (tier >= 2 ? 2 : 1); i++)
                    {
                        Vector2 at = ClampPosition(point + new Vector2(-aim.y, aim.x) * (i == 0 ? 0 : 0.6f), 0.3f);
                        ArsenalProp bomb = PlaceProp(ArsenalPropKind.Bomb, source, at, damage * 1.6f, 1.1f, critical, cause);
                        bomb.LandAt = Elapsed + 0.55f; bomb.FuseAt = bomb.LandAt + 0.8f; bomb.Until = bomb.FuseAt + 0.1f; bomb.Stage = tier;
                    }
                    return true;
                case ArsenalWeapon.PopperSticks:
                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 at = ClampPosition(source.Position + ArsenalTurn(aim, i * 18) * (2.2f + 0.4f * (1 - Mathf.Abs(i)) + 0.4f), 0.3f);
                        ArsenalProp stick = PlaceProp(ArsenalPropKind.Stick, source, at, damage * 0.7f, 0.75f, critical, cause);
                        stick.LandAt = Elapsed + 0.4f + 0.05f * (i + 1); stick.FuseAt = stick.LandAt + 2f; stick.Until = stick.FuseAt + 0.1f;
                    }
                    DropOldest(source, ArsenalPropKind.Stick, 9);
                    return true;
                case ArsenalWeapon.DreamRacket:
                {
                    ProjectileState ball = ArsenalShot(source, ArsenalMuzzle(source), point - ArsenalMuzzle(source), 8f, 6f, damage * 0.85f, critical, cause, 0.14f, 3);
                    ball.ArsenalHops = 3; return true;
                }
                case ArsenalWeapon.BambooVase:
                {
                    ArsenalProp vase = PlaceProp(ArsenalPropKind.Vase, source, ClampPosition(point, 0.3f), damage, 0.45f, critical, cause);
                    vase.LandAt = Elapsed + 0.45f; vase.Until = vase.LandAt + 0.05f; return true;
                }
                case ArsenalWeapon.RoseBouquet:
                    ArsenalShot(source, ArsenalMuzzle(source), point - ArsenalMuzzle(source), 5.5f, 4.5f, damage * 0.6f, critical, cause, 0.16f);
                    return true;
                case ArsenalWeapon.HomeRunBat:
                {
                    if (target == null || (target.Position - source.Position).magnitude > weapon.Range + target.Definition.Radius) return true;
                    ApplyWeaponDamage(target, damage * 1.2f, source.Faction, weapon.Element, weapon.Id, aim, weapon, critical, cause);
                    if (target.Alive && !target.Definition.IsBoss)
                    {
                        ArsenalProp flight = PlaceProp(ArsenalPropKind.Batted, source, target.Position, damage * 0.8f, target.Definition.Radius + 0.3f, critical, cause.Child("home-run"));
                        flight.TargetId = target.Id; flight.Direction = aim; flight.Until = Elapsed + 0.4f; flight.Victims.Add(target.Id);
                    }
                    return true;
                }
                case ArsenalWeapon.CandyDrill:
                    ArsenalShot(source, ArsenalMuzzle(source), point - ArsenalMuzzle(source), 7.5f, 5f, damage, critical, cause, 0.13f, 3);
                    return true;
                case ArsenalWeapon.InkyOctopus:
                {
                    ArsenalProp octopus = null;
                    foreach (ArsenalProp p in arsenalProps) if (p.Source == source && p.Kind == ArsenalPropKind.Octopus) octopus = p;
                    if (octopus != null)
                    {
                        // already clinging: a squeeze on whatever it holds
                        UnitState held = UnitWithId(octopus.TargetId);
                        if (octopus.Stage == 1 && held != null && CanHit(source.Faction, held)) ApplyWeaponDamage(held, damage * 0.4f, source.Faction, weapon.Element, weapon.Id, aim, weapon, critical, cause);
                        return true;
                    }
                    if (target == null) return true;
                    octopus = PlaceProp(ArsenalPropKind.Octopus, source, target.Position, damage * 0.25f, 0.7f, critical, cause);
                    octopus.TargetId = target.Id; octopus.LandAt = Elapsed + 0.45f; octopus.FuseAt = octopus.LandAt + 3f; octopus.Until = octopus.FuseAt + 0.45f;
                    return true;
                }
                case ArsenalWeapon.GrabberClaw:
                    ArsenalShot(source, ArsenalMuzzle(source), point - ArsenalMuzzle(source), 9f, 4f, damage, critical, cause, 0.14f);
                    return true;
                case ArsenalWeapon.StarforgeClaymore:
                    ArsenalSweep(source, weapon, aim, weapon.Range, -0.17f, damage * 1.9f, critical, cause);
                    return true;
                case ArsenalWeapon.DragonLanternGlaive:
                    if (source.ArsenalBeat % 3 != 0) ArsenalSweep(source, weapon, ArsenalTurn(aim, source.ArsenalBeat % 3 == 1 ? 12 : -12), weapon.Range, 0.5f, damage, critical, cause);
                    else ArsenalLine(source, weapon, source.Position, source.Position + aim * 3f, 0.35f, damage * 1.4f, critical, cause.Child("glaive-thrust"));
                    return true;
                case ArsenalWeapon.TwinMoonStaff:
                    ArsenalShot(source, ArsenalMuzzle(source), aim, 6.5f, 3.2f, damage, critical, cause, 0.32f, 8);
                    return true;
                case ArsenalWeapon.PlushShark:
                {
                    UnitState far = null; float best = -1;
                    foreach (UnitState u in units)
                    {
                        float d = (u.Position - source.Position).sqrMagnitude;
                        if (CanHit(source.Faction, u) && d <= 5.5f * 5.5f && d > best) { best = d; far = u; }
                    }
                    Vector2 to = (far != null ? far.Position : point) - ArsenalMuzzle(source);
                    ArsenalShot(source, ArsenalMuzzle(source), to, 7f, Mathf.Min(5.5f, to.magnitude + 0.8f), damage * 1.1f, critical, cause, 0.26f, 99);
                    return true;
                }
                case ArsenalWeapon.DrumstickClub:
                {
                    Vector2 at = ClampPosition(source.Position + aim * 1.3f, 0.3f);
                    ArsenalBurst(source, weapon, at, 0.9f, damage * 1.3f, critical, cause, 0.25f);
                    for (int i = 1; i <= 3; i++)
                    {
                        ArsenalProp bump = PlaceProp(ArsenalPropKind.Bump, source, ClampPosition(at + aim * (0.75f * i), 0.3f), damage * 0.5f, 0.45f, critical, cause.Child("drumstick-shock"));
                        bump.At = Elapsed + 0.08f * i; bump.Until = bump.At + 0.3f;
                    }
                    if (source.ArsenalBeat % 3 == 0)
                        for (int i = 0; i < 4; i++) ArsenalShot(source, at, ArsenalTurn(aim, -40 + i * 26.7f), 6f, 2f, damage * 0.25f, critical, cause.Child("batter-crumb"), 0.08f).IsFragment = true;
                    return true;
                }
                case ArsenalWeapon.PaperShuriken:
                    for (int i = -1; i <= 1; i++) ArsenalShot(source, ArsenalMuzzle(source), ArsenalTurn(point - ArsenalMuzzle(source), i * 12), 8.5f, 5f, damage * 0.55f, critical, cause, 0.1f, 2).ArsenalHops = 2;
                    return true;
                case ArsenalWeapon.SquirtTrio:
                    for (int i = -1; i <= 1; i++) { ProjectileState jet = ArsenalShot(source, ArsenalMuzzle(source), ArsenalTurn(point - ArsenalMuzzle(source), i * 10), 8f, 6.5f, damage * 0.5f, critical, cause, 0.1f); jet.RicochetsRemaining = 2; }
                    return true;
                case ArsenalWeapon.ConfettiBlunderbuss:
                    for (int i = 0; i < 7; i++) ArsenalShot(source, ArsenalMuzzle(source), ArsenalTurn(point - ArsenalMuzzle(source), -15 + i * 5), 9f, 3.2f, damage * 0.26f, critical, cause, 0.08f);
                    if (source.Faction == Faction.Companion) { source.Position = ClampPosition(source.Position - aim * 0.2f, source.Definition.Radius); spatial.Move(source); }
                    return true;
                case ArsenalWeapon.PlungerPopper:
                    ArsenalShot(source, ArsenalMuzzle(source), point - ArsenalMuzzle(source), 8.5f, 4.2f, damage * 0.25f, critical, cause, 0.14f);
                    return true;
                case ArsenalWeapon.SignpostSlammer:
                    ArsenalSweep(source, weapon, aim, weapon.Range, weapon.ConeDot, damage, critical, cause);
                    if (source.ArsenalBeat % 3 == 0)
                    {
                        ArsenalProp sign = PlaceProp(ArsenalPropKind.Sign, source, ClampPosition(point, 0.3f), 0, 0.45f, critical, cause.Child("road-sign"));
                        sign.Until = Elapsed + 3f; sign.Direction = new Vector2(-aim.y, aim.x);
                    }
                    return true;
                case ArsenalWeapon.DetonatorBox:
                {
                    if (target != null) { ArsenalProp bomb = PlaceProp(ArsenalPropKind.ToyBomb, source, ClampPosition(target.Position, 0.3f), damage * 1.2f, 0.9f, critical, cause); bomb.Until = float.PositiveInfinity; bomb.Order = source.ArsenalBeat; }
                    DropOldest(source, ArsenalPropKind.ToyBomb, 3);
                    if (OwnedProps(source, ArsenalPropKind.ToyBomb) >= 3)
                    {
                        // the plunger: every bomb goes in the order they were planted, along the wire
                        int step = 0;
                        foreach (ArsenalProp p in arsenalProps)
                            if (p.Source == source && p.Kind == ArsenalPropKind.ToyBomb && !p.Resolved && p.Stage == 0) { p.Stage = 1; p.FuseAt = Elapsed + 0.25f + 0.14f * step++; p.Until = p.FuseAt + 0.1f; }
                    }
                    return true;
                }
                case ArsenalWeapon.FirecrackerPinwheel:
                {
                    ArsenalProp wheel = PlaceProp(ArsenalPropKind.Pinwheel, source, ClampPosition(source.Position + (point - source.Position).normalized * Mathf.Min(1.6f, (point - source.Position).magnitude), 0.3f), damage * 0.9f, 0.8f, critical, cause);
                    wheel.LandAt = Elapsed + 0.3f; wheel.Until = Elapsed + 12f;
                    DropOldest(source, ArsenalPropKind.Pinwheel, 2);
                    return true;
                }
                case ArsenalWeapon.GumballGatling:
                {
                    if (Elapsed - source.ArsenalLastShot > 1.2f) source.ArsenalHeat = 0;
                    source.ArsenalLastShot = Elapsed;
                    ArsenalShot(source, ArsenalMuzzle(source), ArsenalTurn(point - ArsenalMuzzle(source), (float)(combatRandom.NextDouble() * 8 - 4)), 9f, weapon.Range + 0.5f, damage, critical, cause, 0.09f);
                    source.ArsenalHeat = Mathf.Min(1, source.ArsenalHeat + 0.1f);
                    // a hot barrel fires sooner: up to 40% off the cooldown, never faster than 0.16 s
                    source.NextAttack = Mathf.Min(source.NextAttack, source.AttackStarted + Mathf.Max(0.16f, weapon.Cooldown * (1 - 0.4f * source.ArsenalHeat)));
                    return true;
                }
                case ArsenalWeapon.BalloonBuddy:
                {
                    Vector2 crowd = ArsenalCrowd(source, 5f) ?? point;
                    ProjectileState balloon = ArsenalShot(source, ArsenalMuzzle(source), crowd - ArsenalMuzzle(source), 2.2f, 2.2f * 3f, damage * 0.6f, critical, cause, 0.3f);
                    balloon.ArsenalTimer = Elapsed + 3f; return true;
                }
                case ArsenalWeapon.GlitchToybox:
                    return Glitch(source, damage, critical, cause);
            }
            return false;
        }

        // ---- shapes ----
        private void ArsenalSweep(UnitState source, WeaponDefinition weapon, Vector2 aim, float range, float coneDot, float damage, bool critical, CombatCause cause)
        {
            for (int i = 0; i < units.Count; i++)
            {
                UnitState target = units[i];
                Vector2 delta = target.Position - source.Position;
                if (CanHit(source.Faction, target) && delta.magnitude <= range + target.Definition.Radius && Vector2.Dot(delta.normalized, aim) >= coneDot)
                    ApplyWeaponDamage(target, damage, source.Faction, weapon.Element, weapon.Id, aim, weapon, critical, cause);
            }
        }
        private void ArsenalLine(UnitState source, WeaponDefinition weapon, Vector2 from, Vector2 to, float width, float damage, bool critical, CombatCause cause)
        {
            for (int i = 0; i < units.Count; i++)
            {
                UnitState target = units[i];
                if (CanHit(source.Faction, target) && SegmentCircle(from, to, target.Position, target.Definition.Radius + width) >= 0)
                    ApplyWeaponDamage(target, damage, source.Faction, weapon.Element, weapon.Id, (to - from).normalized, weapon, critical, cause);
            }
        }
        /// <summary>The centre of the densest group of enemies within reach, if any.</summary>
        private Vector2? ArsenalCrowd(UnitState source, float reach)
        {
            UnitState best = null; int most = 0;
            foreach (UnitState u in units)
            {
                if (!CanHit(source.Faction, u) || (u.Position - source.Position).sqrMagnitude > reach * reach) continue;
                int near = 0;
                foreach (UnitState v in units) if (CanHit(source.Faction, v) && (v.Position - u.Position).sqrMagnitude <= 1.44f) near++;
                if (near > most) { most = near; best = u; }
            }
            return best?.Position;
        }

        /// <summary>Glitch Toybox: borrow the shown weapon from the pool, perform its own attack at 90%, and roll the next.</summary>
        private bool Glitch(UnitState source, float damage, bool critical, CombatCause cause)
        {
            WeaponDefinition borrowed = source.BorrowNext ?? Pick();
            source.Borrowed = borrowed; source.BorrowNext = Pick();
            if (borrowed == null) return true;
            if (borrowed.Kind == WeaponKind.Projectile) EmitProjectile(source, damage * 0.9f, critical, cause, borrowed);
            else ArsenalSweep(source, borrowed, ArsenalAim(source), borrowed.Range, borrowed.ConeDot, damage * 0.9f, critical, cause);
            return true;

            WeaponDefinition Pick()
            {
                for (int tries = 0; tries < 4; tries++)
                {
                    WeaponDefinition w = GlitchWeapons[combatRandom.Next(GlitchWeapons.Length)];
                    if (w != null && w.Arsenal == ArsenalWeapon.None) return w;
                }
                return null;
            }
        }

        // ---- shots in flight ----
        /// <summary>Per-step behaviour of Arsenal shots: 0 = let the normal flight run, -1 = remove, 1 = moved here.</summary>
        private int MoveArsenalProjectile(ProjectileState shot, float delta)
        {
            if (shot.Arsenal == ArsenalWeapon.None || shot.Faction == Faction.Enemy) return 0;
            UnitState source = UnitWithId(shot.SourceUnitId);
            switch (shot.Arsenal)
            {
                case ArsenalWeapon.RoseBouquet:
                    // a ribbon of petals left along the way
                    if (source != null && Elapsed >= shot.ArsenalTimer)
                    {
                        shot.ArsenalTimer = Elapsed + 0.15f;
                        ArsenalProp petals = PlaceProp(ArsenalPropKind.Petals, source, shot.Position, shot.Damage * 0.3f, 0.45f, shot.Critical, shot.Cause.Child("rose-petals"));
                        petals.Until = Elapsed + 2f; petals.Direction = shot.Velocity.normalized;
                    }
                    return 0;
                case ArsenalWeapon.CandyDrill:
                    if (shot.Remaining - delta <= 0 && source != null) { PlantDrill(source, shot); return -1; }
                    return 0;
                case ArsenalWeapon.BalloonBuddy:
                {
                    // drifts toward the crowd; pops on its own after 3 s
                    if (source != null && ArsenalCrowd(source, 5f) is Vector2 crowd) shot.Velocity = Vector2.Lerp(shot.Velocity, (crowd - shot.Position).normalized * 2.2f, Mathf.Clamp01(delta * 2));
                    if (Elapsed >= shot.ArsenalTimer && source != null) { PopBalloon(source, shot); return -1; }
                    return 0;
                }
                case ArsenalWeapon.TwinMoonStaff:
                {
                    if (source == null) return -1;
                    if (shot.ArsenalStage == 0 && shot.Remaining - delta <= 0) { shot.ArsenalStage = 1; shot.ArsenalTimer = Elapsed + 0.5f; shot.Remaining = 5; }
                    if (shot.ArsenalStage == 1)
                    {
                        // hovering at the end of its flight, grinding what is under it
                        if (Elapsed >= shot.ArsenalTick) { shot.ArsenalTick = Elapsed + 0.15f; ArsenalBurst(source, shot.Weapon, shot.Position, 0.6f, shot.Damage * 0.3f, shot.Critical, shot.Cause.Child("twin-moon-grind"), loud: false); }
                        if (Elapsed >= shot.ArsenalTimer) { shot.ArsenalStage = 2; shot.Victims.Clear(); }
                        return 1;
                    }
                    if (shot.ArsenalStage == 2)
                    {
                        Vector2 home = source.Position - shot.Position;
                        if (home.magnitude < 0.35f) return -1;
                        shot.Velocity = home.normalized * 7.5f; shot.Remaining = 5;
                    }
                    return 0;
                }
                case ArsenalWeapon.PlushShark:
                    if (shot.ArsenalStage == 0 && shot.Remaining - delta <= 0 && source != null) { shot.ArsenalStage = 1; shot.Remaining = 5; shot.Damage = 0; }
                    if (shot.ArsenalStage == 1 && source != null)
                    {
                        // swims home along the floor without biting
                        Vector2 home = source.Position - shot.Position;
                        if (home.magnitude < 0.4f) return -1;
                        shot.Position += home.normalized * Mathf.Min(home.magnitude, 6.5f * delta); shot.Velocity = home.normalized * 6.5f;
                        return 1;
                    }
                    return 0;
            }
            return 0;
        }

        /// <summary>An Arsenal shot struck an enemy (its own damage is already applied).</summary>
        private void OnArsenalHit(ProjectileState shot, UnitState target)
        {
            UnitState source = UnitWithId(shot.SourceUnitId);
            if (source == null || shot.Faction == Faction.Enemy) return;
            switch (shot.Arsenal)
            {
                case ArsenalWeapon.DreamRacket:
                case ArsenalWeapon.PaperShuriken:
                {
                    if (shot.ArsenalHops <= 0) { shot.Remaining = 0; return; }
                    float reach = shot.Arsenal == ArsenalWeapon.DreamRacket ? 2.6f : 2.4f;
                    UnitState next = null; float best = reach * reach;
                    foreach (UnitState u in units)
                    {
                        float d = (u.Position - target.Position).sqrMagnitude;
                        if (CanHit(source.Faction, u) && !shot.Victims.Contains(u.Id) && d < best) { best = d; next = u; }
                    }
                    if (next == null) { shot.Remaining = 0; return; }
                    shot.ArsenalHops--; shot.Velocity = (next.Position - target.Position).normalized * shot.Velocity.magnitude; shot.Remaining = 0.6f;
                    if (shot.Arsenal == ArsenalWeapon.DreamRacket) shot.Damage *= 0.9f;
                    return;
                }
                case ArsenalWeapon.CandyDrill:
                    if (shot.Victims.Count >= 4) { PlantDrill(source, shot); shot.Remaining = 0; }
                    return;
                case ArsenalWeapon.SquirtTrio:
                    if (target.Alive) target.ChillUntil = Mathf.Max(target.ChillUntil, Elapsed + (target.Definition.IsBoss ? 0.3f : 1.5f));
                    return;
                case ArsenalWeapon.BalloonBuddy:
                    PopBalloon(source, shot); shot.Remaining = 0; return;
                case ArsenalWeapon.PlushShark:
                    if (shot.Victims.Count >= 3) { shot.ArsenalStage = 1; shot.Damage = 0; shot.Remaining = 5; }
                    return;
                case ArsenalWeapon.GrabberClaw:
                    if (target.Alive && !target.Definition.IsBoss)
                    {
                        ArsenalProp grab = PlaceProp(ArsenalPropKind.Grab, source, target.Position, shot.Damage * 0.6f, target.Definition.Radius + 0.3f, shot.Critical, shot.Cause.Child("grabber-fling"));
                        grab.TargetId = target.Id; grab.Start = target.Position; grab.LandAt = Elapsed + 0.3f; grab.Until = grab.LandAt + 0.3f; grab.Victims.Add(target.Id);
                        grab.Direction = (combatRandom.Next(2) == 0 ? 1 : -1) * new Vector2(-ArsenalAim(source).y, ArsenalAim(source).x);
                    }
                    shot.Remaining = 0; return;
                case ArsenalWeapon.PlungerPopper:
                {
                    ArsenalProp plunge = PlaceProp(ArsenalPropKind.Plunge, source, target.Position, shot.Damage / 0.25f * 1.1f, 0, shot.Critical, shot.Cause.Child("plunger-pop"));
                    plunge.TargetId = target.Id; plunge.Start = target.Position; plunge.LandAt = Elapsed + 0.4f; plunge.Until = plunge.LandAt + 0.05f;
                    shot.Remaining = 0; return;
                }
            }
        }
        private void PlantDrill(UnitState source, ProjectileState shot)
        {
            ArsenalProp drill = PlaceProp(ArsenalPropKind.Drill, source, shot.Position, shot.Damage * 0.2f, 0.6f, shot.Critical, shot.Cause.Child("candy-drill"));
            drill.Until = Elapsed + 1f; drill.Direction = shot.Velocity.normalized;
        }
        private void PopBalloon(UnitState source, ProjectileState shot)
        {
            ArsenalBurst(source, shot.Weapon, shot.Position, 1f, shot.Damage, shot.Critical, shot.Cause.Child("balloon-pop"), push: 0.6f, chill: 1.5f);
        }

        // ---- props in the room ----
        private void ProcessArsenal(float delta)
        {
            for (int index = arsenalProps.Count - 1; index >= 0 && Phase == RunPhase.Combat; index--)
            {
                if (index >= arsenalProps.Count) continue;
                ArsenalProp p = arsenalProps[index];
                UnitState source = p.Source;
                if (source == null || !source.Alive && p.Kind != ArsenalPropKind.Bomb) { arsenalProps.RemoveAt(index); continue; }
                WeaponDefinition weapon = p.Weapon;
                switch (p.Kind)
                {
                    case ArsenalPropKind.Bomb:
                    case ArsenalPropKind.Bomblet:
                        if (Elapsed < p.FuseAt) break;
                        ArsenalBurst(source, weapon, p.Position, p.Radius, p.Damage, p.Critical, p.Cause);
                        if (p.Kind == ArsenalPropKind.Bomb && p.Stage >= 3)
                            for (int i = 0; i < 3; i++)
                            {
                                ArsenalProp bit = PlaceProp(ArsenalPropKind.Bomblet, source, ClampPosition(p.Position + ArsenalTurn(Vector2.right, i * 120 + 30) * 0.9f, 0.3f), p.Damage * 0.25f, 0.6f, p.Critical, p.Cause.Child("cherry-bomblet"));
                                bit.Start = p.Position; bit.LandAt = Elapsed + 0.25f; bit.FuseAt = bit.LandAt + 0.1f; bit.Until = bit.FuseAt + 0.1f;
                            }
                        arsenalProps.Remove(p); break;
                    case ArsenalPropKind.Stick:
                    {
                        if (Elapsed < p.LandAt) break;
                        bool touched = false;
                        foreach (UnitState u in units) if (CanHit(source.Faction, u) && (u.Position - p.Position).sqrMagnitude <= Mathf.Pow(0.5f + u.Definition.Radius, 2)) { touched = true; break; }
                        if (!touched && Elapsed < p.FuseAt) break;
                        ArsenalBurst(source, weapon, p.Position, p.Radius, p.Damage, p.Critical, p.Cause);
                        // the chain: sticks nearby go off a beat later
                        foreach (ArsenalProp q in arsenalProps)
                            if (q != p && q.Kind == ArsenalPropKind.Stick && q.Source == source && Elapsed >= q.LandAt && (q.Position - p.Position).sqrMagnitude <= 1.44f) q.FuseAt = Mathf.Min(q.FuseAt, Elapsed + 0.12f);
                        arsenalProps.Remove(p); break;
                    }
                    case ArsenalPropKind.Vase:
                        if (Elapsed < p.LandAt) break;
                        // the vase shatters and a line of bamboo springs up, continuing the throw
                        for (int i = 0; i < 4; i++)
                        {
                            ArsenalProp shoot = PlaceProp(ArsenalPropKind.Shoot, source, ClampPosition(p.Position + p.Direction * (0.55f * i), 0.3f), p.Damage * 0.55f, 0.45f, p.Critical, p.Cause.Child("bamboo-shoot"));
                            shoot.At = Elapsed + 0.12f * i; shoot.Until = shoot.At + 0.6f; shoot.Direction = p.Direction;
                        }
                        arsenalProps.Remove(p); break;
                    case ArsenalPropKind.Shoot:
                    case ArsenalPropKind.Bump:
                        if (!p.Resolved && Elapsed >= p.At)
                        {
                            p.Resolved = true;
                            foreach (UnitState u in units)
                            {
                                if (!CanHit(source.Faction, u) || (u.Position - p.Position).sqrMagnitude > Mathf.Pow(p.Radius + u.Definition.Radius, 2)) continue;
                                ApplyWeaponDamage(u, p.Damage, source.Faction, weapon.Element, weapon.Id, p.Direction, weapon, p.Critical, p.Cause);
                                if (p.Kind == ArsenalPropKind.Shoot && u.Alive && !u.Definition.IsBoss) u.ChillUntil = Mathf.Max(u.ChillUntil, Elapsed + 0.25f);
                            }
                        }
                        if (Elapsed > p.Until) arsenalProps.Remove(p);
                        break;
                    case ArsenalPropKind.Petals:
                    case ArsenalPropKind.Drill:
                        if (Elapsed >= p.NextTick)
                        {
                            p.NextTick = Elapsed + (p.Kind == ArsenalPropKind.Drill ? 0.2f : 0.3f);
                            foreach (UnitState u in units)
                            {
                                if (!CanHit(source.Faction, u) || (u.Position - p.Position).sqrMagnitude > Mathf.Pow(p.Radius + u.Definition.Radius, 2)) continue;
                                ApplyDamage(u, p.Damage, source.Faction, weapon.Id, p.Direction, weapon, p.Critical, cause: p.Cause);
                                if (p.Kind == ArsenalPropKind.Petals && u.Alive) u.ChillUntil = Mathf.Max(u.ChillUntil, Elapsed + 0.4f);
                            }
                        }
                        if (Elapsed > p.Until) arsenalProps.Remove(p);
                        break;
                    case ArsenalPropKind.Octopus:
                    {
                        UnitState held = UnitWithId(p.TargetId);
                        if (p.Stage == 0)
                        {
                            if (held != null && held.Alive) p.Position = held.Position;
                            if (Elapsed >= p.LandAt) p.Stage = held != null && CanHit(source.Faction, held) ? 1 : 2;
                            if (p.Stage == 2) { p.Start = p.Position; p.FuseAt = Elapsed; p.Until = Elapsed + 0.45f; }
                            break;
                        }
                        if (p.Stage == 1)
                        {
                            if (held == null || !CanHit(source.Faction, held))
                            {
                                // its friend is gone: hop to the nearest enemy close by, or come home
                                UnitState next = null; float best = 2.5f * 2.5f;
                                foreach (UnitState u in units) { float d = (u.Position - p.Position).sqrMagnitude; if (CanHit(source.Faction, u) && d < best) { best = d; next = u; } }
                                if (next != null) { p.TargetId = next.Id; held = next; }
                                else { p.Stage = 2; p.Start = p.Position; p.FuseAt = Elapsed; p.Until = Elapsed + 0.45f; break; }
                            }
                            p.Position = held.Position;
                            if (Elapsed >= p.NextTick)
                            {
                                p.NextTick = Elapsed + 0.4f;
                                ApplyWeaponDamage(held, p.Damage, source.Faction, weapon.Element, weapon.Id, Vector2.zero, weapon, p.Critical, p.Cause);
                                foreach (UnitState u in units) if (u != held && CanHit(source.Faction, u) && (u.Position - p.Position).sqrMagnitude <= Mathf.Pow(p.Radius + u.Definition.Radius, 2)) u.ChillUntil = Mathf.Max(u.ChillUntil, Elapsed + 0.5f);
                            }
                            if (Elapsed >= p.FuseAt) { p.Stage = 2; p.Start = p.Position; p.FuseAt = Elapsed; p.Until = Elapsed + 0.45f; }
                            break;
                        }
                        if (Elapsed > p.Until) arsenalProps.Remove(p);
                        break;
                    }
                    case ArsenalPropKind.Sign:
                        if (Elapsed >= p.NextTick)
                        {
                            p.NextTick = Elapsed + 0.3f;
                            foreach (UnitState u in units)
                            {
                                if (!CanHit(source.Faction, u)) continue;
                                Vector2 off = u.Position - p.Position;
                                // a 1.6 x 0.9 "no entry" strip across the way the enemies came
                                if (Mathf.Abs(Vector2.Dot(off, p.Direction)) <= 0.8f + u.Definition.Radius && Mathf.Abs(Vector2.Dot(off, new Vector2(-p.Direction.y, p.Direction.x))) <= 0.45f + u.Definition.Radius)
                                    u.ChillUntil = Mathf.Max(u.ChillUntil, Elapsed + 0.5f);
                            }
                        }
                        if (Elapsed > p.Until) arsenalProps.Remove(p);
                        break;
                    case ArsenalPropKind.ToyBomb:
                        if (p.Stage == 1 && Elapsed >= p.FuseAt) { ArsenalBurst(source, weapon, p.Position, p.Radius, p.Damage, p.Critical, p.Cause.Child("detonator")); arsenalProps.Remove(p); }
                        break;
                    case ArsenalPropKind.Pinwheel:
                    {
                        if (Elapsed < p.LandAt) break;
                        if (p.Stage == 0)
                        {
                            foreach (UnitState u in units) if (CanHit(source.Faction, u) && (u.Position - p.Position).sqrMagnitude <= Mathf.Pow(1f + u.Definition.Radius, 2)) { p.Stage = 1; p.FuseAt = Elapsed + 0.6f; p.NextTick = Elapsed; break; }
                            if (Elapsed > p.Until) arsenalProps.Remove(p);
                            break;
                        }
                        if (Elapsed >= p.NextTick && p.Order < 6)
                        {
                            // the spinning pinwheel throws a spark every 0.1 s
                            p.NextTick = Elapsed + 0.1f;
                            ProjectileState spark = ArsenalShot(source, p.Position, ArsenalTurn(Vector2.right, p.Order * 60 + 15), 6f, 1.6f, p.Damage * 0.28f, p.Critical, p.Cause.Child("pinwheel-spark"), 0.07f, 0, weapon);
                            spark.IsFragment = true; spark.Arsenal = ArsenalWeapon.None; p.Order++;
                        }
                        if (Elapsed >= p.FuseAt) { ArsenalBurst(source, weapon, p.Position, p.Radius, p.Damage, p.Critical, p.Cause); arsenalProps.Remove(p); }
                        break;
                    }
                    case ArsenalPropKind.Batted:
                    case ArsenalPropKind.Grab:
                    {
                        UnitState carried = UnitWithId(p.TargetId);
                        if (carried == null || !carried.Alive) { arsenalProps.Remove(p); break; }
                        Vector2 move;
                        if (p.Kind == ArsenalPropKind.Grab && Elapsed < p.LandAt)
                        {
                            // reeled in to just in front of the wielder
                            Vector2 dock = source.Position + ArsenalAim(source) * 1f;
                            move = (dock - carried.Position) * Mathf.Clamp01(delta / Mathf.Max(0.01f, p.LandAt - Elapsed + delta));
                        }
                        else move = p.Direction * (p.Kind == ArsenalPropKind.Batted ? 8f : 5.3f) * delta;
                        carried.Position = ClampPosition(carried.Position + move, carried.Definition.Radius); spatial.Move(carried);
                        carried.ChillUntil = Mathf.Max(carried.ChillUntil, Elapsed + 0.2f);
                        p.Position = carried.Position;
                        if (p.Kind == ArsenalPropKind.Batted || Elapsed >= p.LandAt)
                            foreach (UnitState u in units)
                            {
                                if (!CanHit(source.Faction, u) || p.Victims.Contains(u.Id) || (u.Position - carried.Position).sqrMagnitude > Mathf.Pow(p.Radius + u.Definition.Radius, 2)) continue;
                                p.Victims.Add(u.Id);
                                ApplyWeaponDamage(u, p.Damage, source.Faction, weapon.Element, weapon.Id, p.Direction, weapon, p.Critical, p.Cause);
                            }
                        if (Elapsed > p.Until) arsenalProps.Remove(p);
                        break;
                    }
                    case ArsenalPropKind.Plunge:
                    {
                        UnitState stuck = UnitWithId(p.TargetId);
                        if (stuck == null || !stuck.Alive) { arsenalProps.Remove(p); break; }
                        if (Elapsed < p.LandAt)
                        {
                            if (!stuck.Definition.IsBoss)
                            {
                                Vector2 toward = (source.Position - stuck.Position);
                                stuck.Position = ClampPosition(stuck.Position + toward.normalized * Mathf.Min(toward.magnitude * 0.5f, 2.5f * delta), stuck.Definition.Radius); spatial.Move(stuck);
                            }
                            p.Position = stuck.Position; break;
                        }
                        Vector2 away = (stuck.Position - source.Position).sqrMagnitude > 0.0001f ? (stuck.Position - source.Position).normalized : ArsenalAim(source);
                        ApplyWeaponDamage(stuck, p.Damage, source.Faction, weapon.Element, weapon.Id, away, weapon, p.Critical, p.Cause);
                        if (stuck.Alive && !stuck.Definition.IsBoss) { stuck.Position = ClampPosition(stuck.Position + away * OctoberBalance.Push(stuck.Definition, 0.8f), stuck.Definition.Radius); spatial.Move(stuck); }
                        arsenalProps.Remove(p); break;
                    }
                }
            }
            ArsenalGuards();
        }

        /// <summary>Starforge Claymore: from its wind-up to the end of the swing, the blade's arc in front destroys hostile shots.</summary>
        private void ArsenalGuards()
        {
            foreach (UnitState unit in units)
            {
                if (!unit.Alive || unit.Faction == Faction.Enemy || unit.Definition.Weapon.Arsenal != ArsenalWeapon.StarforgeClaymore) continue;
                bool guarding = unit.PendingAttack || Elapsed <= unit.ImpactTime + 0.25f;
                if (!guarding) continue;
                Vector2 aim = ArsenalAim(unit); float reach = unit.Definition.Weapon.Range;
                for (int i = projectiles.Count - 1; i >= 0; i--)
                {
                    ProjectileState shot = projectiles[i];
                    Vector2 off = shot.Position - unit.Position;
                    if (shot.Faction != Faction.Enemy || off.sqrMagnitude > reach * reach || off.sqrMagnitude < 0.04f || Vector2.Dot(off.normalized, aim) < -0.17f) continue;
                    projectiles.RemoveAt(i);
                    events.Add(new CombatEvent(CombatEventKind.Proc, shot.Position, unit.Faction, 0.3f, aim, unit.Definition.Weapon, cause: NewCause(unit.Id).Child("claymore-block")));
                }
            }
        }

#if UNITY_EDITOR
        public bool ArsenalAttackFixture(UnitState source, UnitState target)
        {
            source.AttackTargetId = target != null ? target.Id : 0;
            if (target != null) source.AttackDirection = (target.Position - source.Position).normalized;
            return ResolveArsenalAttack(source, source.Definition.Weapon.Damage, false, NewCause(source.Id));
        }
        public ProjectileState ProjectileFixtureShot(Vector2 position, Vector2 velocity)
        {
            var shot = new ProjectileState { Id = ++nextId, Faction = Faction.Enemy, Position = position, PreviousPosition = position, Velocity = velocity, Damage = 1, Remaining = 5, Color = Color.white };
            projectiles.Add(shot); return shot;
        }
#endif
    }
}
