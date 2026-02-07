using System;
using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>What a glyph means to the presentation; never read by gameplay.</summary>
    public enum HeroGlyphRole { Emblem, Strike, Link, Lob, Mark, Ambient, Catch, Poise }
    public sealed class HeroIdentityGlyph
    {
        public HeroGlyphRole Role;
        public int Seed;
        public MorphGeometry Shape;
        public Vector2 Position, End, Direction;
        public float Radius, Width, Created, At, Until;
        public Sprite Portrait;
    }
    public sealed partial class CombatSimulation
    {
        private sealed class IdentityStrike
        {
            public MorphGeometry Shape; public Vector2 Position, End, Direction;
            public float Radius, Width, At, Budget, Slow, Push; public int Targets;
            public DamageElement Element; public CombatCause Cause;
        }
        private sealed class IdentityMark { public UnitState Target; public float Until; }
        private sealed class IdentityReflection { public Vector2 Direction; public DamageElement Element; }
        private readonly List<IdentityStrike> identityStrikes = new List<IdentityStrike>();
        private readonly List<HeroIdentityGlyph> identityGlyphs = new List<HeroIdentityGlyph>();
        private readonly List<IdentityMark> identityMarks = new List<IdentityMark>();
        private readonly List<IdentityReflection> identityReflections = new List<IdentityReflection>();
        private readonly List<UnitState> identityTargets = new List<UnitState>();
        private readonly List<UnitState> identityPerformers = new List<UnitState>();
        private readonly List<UnitDefinition> identitySpecimens = new List<UnitDefinition>();
        private readonly List<Vector2> identityTrail = new List<Vector2>();
        private readonly HashSet<int> identityNearMisses = new HashSet<int>();
        private readonly List<int> identityPages = new List<int>();
        private float identityCharge, identityAt, identitySecondAt, identityNearAt, identityStill, identityTickAt, identityStepAt, identityGhostUntil, identityGhostAt, identityBank, identityBankAt, identityRecovered;
        private int identityAttacks, identityCycle, identityCounter, identityPayouts, identityFace, identityThorns;
        private Vector2 identityHeading, identityGhost;
        private UnitState identityBounty, identityLead;
        private bool identityRearSpent;
        public int HeroIdentityActivations { get; private set; }
        public float HeroIdentityLastCast { get; private set; } = -100;
        public bool HeroIdentityEnabled => catalog.EnableHeroIdentities && Champion != null && HeroIdentities.For(Champion.Definition.Id) != null;
        private string IdentityId => Champion.Definition.Id;
        private int IdentityApproach => catalog.HeroApproach;
        private float IdentityPower => Mathf.Max(1, Champion.Definition.Weapon.Damage);
        public IReadOnlyList<HeroIdentityGlyph> HeroIdentityGlyphs => identityGlyphs;
        // Read-only views of the mechanic's live state for the presentation layer.
        public int HeroIdentityMarkCount => identityMarks.Count;
        public UnitState HeroIdentityMarked(int index) => identityMarks[index].Target;
        public float HeroIdentityMarkUntil(int index) => identityMarks[index].Until;
        public UnitState HeroIdentityBounty => identityBounty != null && identityBounty.Alive ? identityBounty : null;
        public Vector2 HeroIdentityRearDirection => identityGhost;
        public bool HeroIdentityDreaming => IdentityId == "DrowsySeer" && identityGhostUntil > Elapsed;
        public Vector2 HeroIdentityDreamPosition => identityGhost;
        public float HeroIdentityDreamUntil => identityGhostUntil;
        public int HeroIdentityStored => identityReflections.Count;
        public IReadOnlyList<Vector2> HeroIdentityRoute => identityTrail;
        public float HeroIdentityReadyAt => identityAt;
        public UnitState HeroIdentityLead => identityLead;
        public int HeroIdentityNextFace => identityFace;
        public int HeroIdentityRootTurn => identityCycle % 3;
        public float HeroIdentityStillTime => identityStill;
        public float HeroIdentityBankAge => identityBankAt < 0 ? -1 : Elapsed - identityBankAt;
        public int HeroIdentityPetal(int index) => index >= 0 && index < catalog.HeroPetals.Length ? catalog.HeroPetals[index] : 0;
        public int HeroIdentityApproach => IdentityApproach;
        public int HeroIdentitySpecimens => identitySpecimens.Count;
        public float HeroIdentityCharge => identityCharge;
        public float HeroIdentityRequired => IdentityId switch {
            "LanternKnight" => 8, "Postmaster" => 3, "DrowsySeer" => 6, "RootOracle" => 3,
            "LoneJelly" => 3, "GlassDice" => 6, "TideSinger" => 3, "CometRunner" => 10,
            "MirrorKeeper" => 3, "TreasureAcorn" => 3, "VerdantMatron" => 6, "CopperCauldron" => 6,
            "ChinaMarionette" => 3, "SoapSkater" => 3, "VelvetRegent" => 6, "ButtonCavalier" => 3,
            "NightMaw" => 3, "ThornTreasurer" => 4, "CitrusMarshal" => 8, "IvorySovereign" => 3,
            "PetalCitadel" => 4, "PatchworkHost" => 4, "ArchiveCurator" => 3, "PennyBroker" => 10,
            "StormFencer" => 3, "TinVirtuoso" => 2 + IdentityApproach * 2, "MoonProwler" => 5,
            "SupperMortar" => 2, "LanternWeaver" => 3, "HourglassStag" => IdentityPower * 2,
            "RainArchivist" => 3, _ => 1 };
        public string HeroIdentityStatus
        {
            get
            {
                if (!HeroIdentityEnabled) return "";
                if (IdentityId == "GlassDice") return "NEXT FACE " + identityFace + " / " + Mathf.FloorToInt(identityCharge) + "/6 GOLD";
                if (IdentityId == "MirrorKeeper") return identityReflections.Count + "/3 REFLECTIONS";
                if (IdentityId == "DrowsySeer" && identityGhostUntil > Elapsed) return "DREAM " + (identityGhostUntil - Elapsed).ToString("0.0") + "s";
                if (IdentityId == "PatchworkHost") return identityLead == null ? "CHOOSE A FEATURED FRIEND IN SHOP" : "LEAD: " + identityLead.Definition.DisplayName;
                if (IdentityId == "PetalCitadel") return "PETALS " + string.Join(" / ", catalog.HeroPetals);
                if (IdentityId == "SupperMortar") return HeroIdentities.For(IdentityId).Approaches[IdentityApproach] + " / TWO SHELLS";
                if (IdentityId == "HourglassStag") return "DEBT " + identityBank.ToString("0.0");
                if (IdentityId == "MoonProwler" && identityCharge >= 5) return "REAR STRIKE READY";
                return Mathf.FloorToInt(identityCharge) + " / " + HeroIdentityRequired.ToString("0") + "  " + HeroIdentities.For(IdentityId).Resource.ToUpperInvariant();
            }
        }

        private void ResetHeroIdentity()
        {
            ClearHeroIdentity();
            identityCharge = identityStill = identityBank = identityRecovered = 0;
            identityAt = identitySecondAt = identityNearAt = identityTickAt = identityStepAt = identityGhostAt = 0;
            identityGhostUntil = 0; identityBankAt = -1; identityHeading = Vector2.right;
            identityAttacks = identityCycle = identityCounter = identityPayouts = 0;
            identityFace = Mathf.Clamp(catalog.HeroDiceFace, 1, 6); identityThorns = Mathf.Clamp(catalog.HeroWallet / 30, 0, 4);
            identityBounty = identityLead = null; identityRearSpent = false;
            HeroIdentityActivations = 0; HeroIdentityLastCast = -100;
            identityTrail.Add(Champion.Position);
            if (!HeroIdentityEnabled) return;
            foreach (UnitState u in units)
            {
                if (u.Faction != Faction.Companion) continue;
                if (u.Definition.Id == catalog.HeroLeadUnit && u.Definition.Weapon.Id == catalog.HeroLeadWeapon) { identityLead = u; break; }
                if (identityLead == null || u.Definition.CombatTier > identityLead.Definition.CombatTier) identityLead = u;
            }
            if (IdentityId == "ThornTreasurer") identityCharge = identityThorns;
        }
        private void ClearHeroIdentity()
        {
            identityStrikes.Clear(); identityGlyphs.Clear(); identityMarks.Clear(); identityReflections.Clear(); identitySpecimens.Clear();
            identityTrail.Clear(); identityNearMisses.Clear(); identityPages.Clear(); identityTargets.Clear(); identityPerformers.Clear();
        }
        private CombatCause IdentityCause() => NewCause(Champion.Id).Child("hero-identity-" + IdentityId);
        private bool IdentityOwn(CombatCause cause) => cause.ItemId != null && cause.ItemId.StartsWith("hero-identity-", StringComparison.Ordinal);
        private void IdentityCast()
        {
            HeroIdentityActivations++; HeroIdentityLastCast = Elapsed; Champion.LastSupport = Elapsed;
            var cause = IdentityCause(); RecordActivation(cause);
            events.Add(new CombatEvent(CombatEventKind.HeroIdentityCast, Champion.Position, Faction.Champion, subject: Champion, cause: cause));
        }
        private int identityGlyphSeed;
        private HeroIdentityGlyph IdentityGlyph(MorphGeometry shape, Vector2 at, Vector2 end, float radius, float duration = .6f, float delay = 0, HeroGlyphRole role = HeroGlyphRole.Emblem)
        {
            if (identityGlyphs.Count >= 40) identityGlyphs.RemoveAt(0);
            var g = new HeroIdentityGlyph { Role = role, Seed = ++identityGlyphSeed, Shape = shape, Position = at, End = end, Radius = radius, Width = .2f,
                Direction = (end - at).sqrMagnitude > .001f ? (end - at).normalized : Champion.Facing,
                Created = Elapsed, At = Elapsed + delay, Until = Elapsed + delay + duration };
            identityGlyphs.Add(g); return g;
        }
        private void IdentityHit(MorphGeometry shape, Vector2 at, Vector2 end, float radius, float budget, float delay = .2f, int maxTargets = 3, DamageElement element = DamageElement.Physical, float slow = 0, float push = 0)
        {
            if (identityStrikes.Count >= 32 || !float.IsFinite(budget) || budget <= 0) return;
            identityStrikes.Add(new IdentityStrike { Shape = shape, Position = at, End = end, Radius = radius, Width = .25f, At = Elapsed + delay,
                Direction = (end - at).sqrMagnitude > .001f ? (end - at).normalized : Champion.Facing,
                Budget = budget, Targets = Mathf.Clamp(maxTargets, 1, 5), Element = element, Slow = slow, Push = push, Cause = IdentityCause() });
            IdentityGlyph(shape, at, end, radius, .35f, delay, HeroGlyphRole.Strike).Width = .25f;
        }
        private void ResolveIdentityStrikes()
        {
            for (int i = identityStrikes.Count - 1; i >= 0 && Phase == RunPhase.Combat; i--)
            {
                var s = identityStrikes[i]; if (Elapsed < s.At) continue;
                identityStrikes.RemoveAt(i); identityTargets.Clear();
                foreach (UnitState u in units)
                {
                    if (!CanHit(Faction.Champion, u)) continue;
                    float distance = Vector2.Distance(u.Position, s.Position), r = u.Definition.Radius;
                    bool inside = s.Shape == MorphGeometry.Line ? SegmentCircle(s.Position, s.End, u.Position, s.Width + r) >= 0 :
                        s.Shape == MorphGeometry.Ring ? Mathf.Abs(distance - s.Radius) <= s.Width + r :
                        s.Shape == MorphGeometry.Crescent ? distance <= s.Radius + r && Vector2.Dot((u.Position - s.Position).normalized, s.Direction) >= .1f : distance <= s.Radius + r;
                    if (inside) identityTargets.Add(u);
                }
                identityTargets.Sort((a,b) => { int c = (a.Position - s.Position).sqrMagnitude.CompareTo((b.Position - s.Position).sqrMagnitude); return c != 0 ? c : a.Id.CompareTo(b.Id); });
                int count = Mathf.Min(s.Targets, identityTargets.Count);
                for (int n = 0; n < count; n++)
                {
                    UnitState u = identityTargets[n]; Vector2 dir = (u.Position - s.Position).normalized;
                    ApplyWeaponDamage(u, s.Budget / count, Faction.Champion, s.Element, HeroIdentities.For(IdentityId).Name, dir, cause: s.Cause);
                    if (!u.Alive) continue;
                    if (s.Slow > 0) u.ChillUntil = Mathf.Max(u.ChillUntil, Elapsed + (u.Definition.IsBoss ? Mathf.Min(.3f, s.Slow) : s.Slow));
                    float push = OctoberBalance.Push(u.Definition, Mathf.Abs(s.Push));
                    if (push > 0) { u.Position = ClampPosition(u.Position + dir * push * Mathf.Sign(s.Push), u.Definition.Radius); spatial.Move(u); }
                }
            }
        }
        private UnitState IdentityTarget(Vector2 origin, float range = 8, int preference = 0)
        {
            UnitState best = null; float score = float.PositiveInfinity;
            foreach (UnitState u in units)
            {
                if (!CanHit(Faction.Champion, u) || (u.Position - origin).sqrMagnitude > range * range) continue;
                float next = (u.Position - origin).sqrMagnitude;
                if (preference == 1 && u.Definition.Weapon.Kind == WeaponKind.Projectile) next -= 80;
                if (preference == 2) next -= Mathf.Min(90, u.Health / Mathf.Max(1, IdentityPower));
                if (next < score) { best = u; score = next; }
            }
            return best;
        }
        private void IdentityShot(Vector2 from, Vector2 direction, float totalDamage, int count = 1, float spread = 16, DamageElement element = DamageElement.Physical, int homingTarget = 0, Sprite sprite = null)
        {
            count = Mathf.Clamp(count, 1, 6); direction = direction.sqrMagnitude < .001f ? Vector2.right : direction.normalized;
            for (int i = 0; i < count && projectiles.Count < 1000; i++)
            {
                Vector2 d = Quaternion.Euler(0,0,(i - (count - 1) * .5f) * spread) * direction;
                projectiles.Add(new ProjectileState { Id = ++nextId, Faction = Faction.Champion, Position = from, PreviousPosition = from,
                    Velocity = d * 5.5f, Remaining = 1.65f, Damage = totalDamage / count, Color = HeroIdentities.For(IdentityId).Color,
                    Element = element, SourceName = HeroIdentities.For(IdentityId).Name, SourceUnitId = Champion.Id,
                    Cause = IdentityCause(), Sprite = sprite != null ? sprite : catalog.Projectile, VisualWidth = .21f, Radius = .07f, IsFragment = true, IdentityTargetId = homingTarget });
            }
        }
        private void IdentityPattern(UnitState target, float budget, int approach = -1, Vector2? origin = null, DamageElement element = DamageElement.Physical)
        {
            if (target == null) return;
            Vector2 at = origin ?? Champion.Position, dir = (target.Position - at).normalized;
            int mode = approach < 0 ? IdentityApproach : approach;
            if (mode == 0) IdentityHit(MorphGeometry.Line, at, at + dir * Mathf.Min(6, Vector2.Distance(at, target.Position) + 1), 0, budget, element: element);
            else if (mode == 1) IdentityHit(MorphGeometry.Crescent, at, at + dir, Mathf.Min(4, Vector2.Distance(at, target.Position) + .5f), budget, element: element);
            else { IdentityHit(MorphGeometry.Line, target.Position - Vector2.one, target.Position + Vector2.one, 0, budget * .5f, element: element); IdentityHit(MorphGeometry.Line, target.Position + new Vector2(-1,1), target.Position + new Vector2(1,-1), 0, budget * .5f, .35f, element: element); }
        }
        private void IdentityPerform(float budget, int wanted)
        {
            identityPerformers.Clear();
            foreach (UnitState u in units) if (u.Alive && u.Faction == Faction.Companion) identityPerformers.Add(u);
            if (identityPerformers.Count == 0) identityPerformers.Add(Champion);
            int count = Mathf.Min(wanted, identityPerformers.Count);
            bool ownWeapons = IdentityId == "TinVirtuoso" || IdentityId == "VelvetRegent";
            if (ownWeapons) { budget = 0; for (int i=0;i<count;i++) budget += Mathf.Min(IdentityPower*2, identityPerformers[(identityCycle+i)%identityPerformers.Count].Definition.Weapon.Damage*.7f); }
            float weights = 0; for (int i=0;i<count;i++) weights += Mathf.Max(1,identityPerformers[(identityCycle+i)%identityPerformers.Count].Definition.Weapon.Damage);
            for (int i = 0; i < count; i++)
            {
                UnitState u = identityPerformers[(identityCycle + i) % identityPerformers.Count];
                UnitState target = IdentityTarget(u.Position, 7, IdentityId == "VelvetRegent" ? IdentityApproach : 0); if (target == null) continue;
                IdentityEcho(u, target, ownWeapons ? budget*Mathf.Max(1,u.Definition.Weapon.Damage)/Mathf.Max(1,weights) : budget / count); IdentityGlyph(MorphGeometry.Line, Champion.Position, u.Position, 0, .35f, role: HeroGlyphRole.Link);
            }
            identityCycle++;
        }
        private void IdentityEcho(UnitState performer, UnitState target, float budget, bool fromHero = false)
        {
            Vector2 from = fromHero ? Champion.Position : performer.Position;
            var w = performer.Definition.Weapon;
            if (w.Kind == WeaponKind.Projectile) IdentityShot(from, target.Position - from, budget, Mathf.Min(3, w.ProjectileCount), element: w.Element, sprite: w.ProjectileSprite);
            else IdentityHit(MorphGeometry.Crescent, from, target.Position, Mathf.Clamp(Vector2.Distance(from, target.Position) + .3f, .8f, 3.4f), budget, element: w.Element);
        }
        private void IdentityMarkTarget(UnitState target)
        {
            if (target == null || !target.Alive) return;
            var mark = identityMarks.Find(m => m.Target == target);
            if (mark != null) { mark.Until = Elapsed + 6; return; }
            if (identityMarks.Count >= 3) identityMarks.RemoveAt(0);
            identityMarks.Add(new IdentityMark { Target = target, Until = Elapsed + 6 });
        }
        private void IdentityAddPage(int kind)
        {
            if (Elapsed < identityAt || identityPages.Count >= 3) return;
            identityPages.Add(kind); identityCharge = identityPages.Count;
        }
        private void IdentityNearMiss()
        {
            if (Elapsed < identityNearAt) return;
            foreach (var p in projectiles)
            {
                if (p.Faction != Faction.Enemy || identityNearMisses.Contains(p.Id)) continue;
                float d = Vector2.Distance(p.Position, Champion.Position), collision = Champion.Definition.Radius + p.Radius;
                if (d <= collision + .12f || d > collision + .65f || Vector2.Dot(p.Velocity, p.Position - Champion.Position) <= 0) continue;
                identityNearMisses.Add(p.Id); identityCharge = Mathf.Min(3, identityCharge + 1); identityNearAt = Elapsed + .8f;
                IdentityGlyph(MorphGeometry.Ring, Champion.Position, Champion.Position, .5f, .25f, role: HeroGlyphRole.Poise); break;
            }
            if (identityNearMisses.Count > 64) identityNearMisses.RemoveWhere(id => !projectiles.Exists(p => p.Id == id));
        }
        private float IdentityDamage(UnitState target, float damage, Faction source, CombatCause cause)
        {
            if (!HeroIdentityEnabled || target.Faction != Faction.Enemy || source != Faction.Champion || cause.SourceUnitId != Champion.Id || cause.Depth != 0 || IdentityId != "HourglassStag") return damage;
            float banked = Mathf.Min(damage * .25f, Mathf.Max(0, IdentityPower * 2 - identityBank));
            if (banked <= 0) return damage;
            identityBank += banked; identityCharge = identityBank;
            if (identityBankAt < 0) identityBankAt = Elapsed;
            return damage - banked;
        }
        private float IdentityAttackDamage(UnitState source, float damage)
        {
            if (!HeroIdentityEnabled || source != Champion || IdentityId != "MoonProwler" || identityCharge < 5) return damage;
            var target = units.Find(u => u.Id == source.AttackTargetId);
            if (target == null || target != identityBounty || Vector2.Dot((Champion.Position - target.Position).normalized, identityGhost) > -.25f) return damage;
            identityCharge = 0; identityRearSpent = true; identityBounty = target; IdentityCast();
            IdentityPattern(target, damage * .2f); return damage * 1.45f;
        }
        private bool IdentityMortar(UnitState source, float damage)
        {
            if (!HeroIdentityEnabled || source != Champion || IdentityId != "SupperMortar") return false;
            UnitState target = units.Find(u => u.Id == source.AttackTargetId && CanHit(Faction.Champion,u));
            if (target == null) return true;
            float food = Mathf.Min(.35f, catalog.HeroFood * .025f);
            for (int i = 0; i < 2; i++)
            {
                Vector2 at = target.Position + Vector2.right * (i == 0 ? -.55f : .55f);
                if (IdentityApproach == 1) IdentityHit(MorphGeometry.Line, at - Vector2.up, at + Vector2.up, 0, damage * .5f, .85f + i * .15f, element: DamageElement.Ember);
                else IdentityHit(MorphGeometry.Disc, at, at, 1.05f + food, damage * .5f, .85f + i * .15f, element: IdentityApproach == 2 ? DamageElement.Frost : DamageElement.Physical, slow: IdentityApproach == 2 ? 1 : 0);
                IdentityGlyph(MorphGeometry.Line, source.Position, at, 0, .85f + i * .15f, role: HeroGlyphRole.Lob);
            }
            identityCharge = 2; IdentityCast(); return true;
        }
        private float IdentityTargetScore(UnitState enemy)
        {
            if (!HeroIdentityEnabled || IdentityId != "VelvetRegent") return 0;
            return IdentityApproach == 1 && enemy.Definition.Weapon.Kind == WeaponKind.Projectile ? -2 : IdentityApproach == 2 ? -Mathf.Min(2, enemy.Health / Mathf.Max(1, IdentityPower) * .1f) : 0;
        }
        private bool FollowIdentityConvoy(UnitState unit, float delta)
        {
            if (!HeroIdentityEnabled || IdentityId != "CitrusMarshal" || catalog.StaticFollowers || Phase != RunPhase.Combat) return false;
            int lane = unit.Id % 2 == 0 ? 1 : -1, row = unit.Id % 5;
            Vector2 forward = identityHeading.sqrMagnitude > .01f ? identityHeading : Champion.Facing, side = new Vector2(-forward.y, forward.x);
            Vector2 offset = IdentityApproach == 2 ? (Vector2)(Quaternion.Euler(0,0,unit.Id * 137.5f) * Vector2.right) * (1 + row * .18f) : -forward * (1 + row * .42f) + side * lane * (IdentityApproach == 1 ? 1.5f : .65f);
            Vector2 desired = ClampPosition(Champion.Position + offset, unit.Definition.Radius), previous = unit.Position;
            unit.Position = Vector2.MoveTowards(unit.Position, desired, Mathf.Min(10, unit.Definition.Speed) * delta);
            unit.Velocity = (unit.Position - previous) / delta; return true;
        }
    }
}
