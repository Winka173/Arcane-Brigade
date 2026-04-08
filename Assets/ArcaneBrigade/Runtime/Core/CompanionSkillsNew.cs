using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The new friends' own skills and talents (Docs/Content_Expansion_Proposal.md §4.3-4.4 with Docs/Content_Expansion_v5.md
    /// §4-6 and §8), plus Iron Locomotive's frost whistle and Paper Inventor's tower (v5). Every skill acts from the friend or on
    /// its enemies (nothing radiates from the hero); placed pieces are ArsenalProps so ArsenalVisual paints them.</summary>
    public sealed partial class CombatSimulation
    {
        /// <summary>Player-facing copy of each friend's own skill or talent (the OWN SKILL section of its description).</summary>
        public static readonly Dictionary<string, string> SkillCopy = new Dictionary<string, string>
        {
            ["PillowOracle"] = "Every 8s lulls the strongest enemy near it to sleep for 1.5s.",
            ["ReelOwl"] = "Every 4th attack replays the strongest blow of a friend nearby on its own target.",
            ["ClockworkPiper"] = "Every 6s a tune: friends within 3 units attack 30% faster for 2s.",
            ["CoilKettle"] = "Its first blow on each new enemy springs it up, stunning it for 0.4s.",
            ["IronLocomotive"] = "Every 7s a frost whistle: a cold blast ahead freezes the first 3 enemies in it for 1s.",
            ["PaperInventor"] = "Once a fight builds a paper tower ahead of it that fires paper darts at the enemies near it.",
            ["ReturnCaptain"] = "Every 5s drops an anchor on the strongest enemy: friends deal it +20% damage for 5s.",
            ["MirrorNewt"] = "A quarter of the hostile shots that reach it are turned back at an enemy.",
            ["TeaDragon"] = "Every 7s sets a teapot ahead; it whistles, then bursts in scalding steam that spreads to the enemies beside it.",
            ["LighthouseWhale"] = "Every 6s sprays a column of water on the enemy nearest the hero, soaking the ones around and pushing them back.",
            ["JackInTheBox"] = "Every 6s spins a ball on its nose: enemies within 3.5 units turn on it for 1.5s.",
            ["KiteDragon"] = "Flies over the fight; every 5s sweeps a cutting band of wind across the field ahead.",
            ["SundaeGolem"] = "Every blow leaves a puddle of cream; an enemy that stands in it for 2s freezes for 1s.",
            ["LostCarousel"] = "Every 10s raises a carousel round the thickest crowd: its horses gallop round for 4s, striking and shoving.",
            ["EndlessIndex"] = "Every 3rd attack copies that blow onto 2 nearby enemies. Every enemy species defeated in the run is a page: at 10, 20 and 30 pages Runic School friends split their shots once more.",
            ["FirstSeed"] = "Every 8s grows a thorn hedge ahead that stops enemies for 3s; each fight a sapling sprouts to fight beside it.",
            ["EclipseEgg"] = "Each fight it hatches into one of three forms and keeps it: a saucer gunner, a brawler, or a herald that hastens the friends near it.",
            ["StaticFerret"] = "Its hits leave static that arcs once to the nearest other enemy (50%).",
            ["StormKitten"] = "Every 6s a little storm cloud over an enemy drops a bolt of lightning.",
            ["VoltEel"] = "Its hits arc on twice: to the nearest other enemy, then the next (50%, then 35%).",
            ["ShantySeaLion"] = "Every 3rd hit lands with a note that pushes the enemy back.",
            ["BottleGull"] = "Recruiting it counts as buying a drink.",
            ["PinnedBeetle"] = "Its hits pin the enemy in place for 0.3s (once every 2s per enemy).",
            ["FossilArmadillo"] = "Rolled up: an enemy that bumps into it is stunned for 0.3s (once every 2s per enemy).",
            ["SpecimenSlime"] = "Defeating a recorded species splits off a little slime that fights beside it (at most 2 a fight).",
            ["CourtHerald"] = "Fanfare: the first attack of every friend each wave fires one extra shot.",
        };

        private bool newSkillsStarted;
        private readonly HashSet<int> towerBuilt = new HashSet<int>();
        private readonly Dictionary<int, int> hitCount = new Dictionary<int, int>();
        private int miniSlimes;

        private ArsenalProp SkillProp(ArsenalPropKind kind, UnitState u, Vector2 at, float damage, float radius, float life, string cause)
        {
            ArsenalProp p = PlaceProp(kind, u, at, damage, radius, false, NewCause(u.Id).Child(cause));
            p.Until = Elapsed + life; return p;
        }
        private Vector2 Toward(UnitState u, Vector2 point, float distance)
        {
            Vector2 d = point - u.Position; return ClampPosition(u.Position + (d.sqrMagnitude > .0001f ? d.normalized : u.Facing) * Mathf.Min(distance, d.magnitude), .3f);
        }

        /// <summary>The first step of a fight: Eclipse Egg hatches, Court Herald sounds the fanfare, The First Seed's sapling sprouts,
        /// The Endless Index's pages split the Runic School's shots.</summary>
        private void StartNewCompanionSkills()
        {
            newSkillsStarted = true; towerBuilt.Clear(); hitCount.Clear(); miniSlimes = 0;
            ExpansionArtwork art = ExpansionArtwork.Current;
            bool herald = false; int pages = 0;
            foreach (UnitState u in units.ToArray())
            {
                if (!u.Alive || u.Faction != Faction.Companion) continue;
                switch (u.Definition.Id)
                {
                    case "EclipseEgg":
                        if (art != null && art.EclipseForms.Length == 3) Hatch(u, art.EclipseForms[combatRandom.Next(3)]);
                        break;
                    case "CourtHerald": herald = true; Skill(u, MorphGeometry.Disc, u.Position, u.Position, .3f); break;
                    case "FirstSeed":
                        if (art != null && art.Sapling != null) { UnitState sapling = Spawn(art.Sapling, Faction.Companion, ClampPosition(u.Position + Vector2.left * .6f, .3f)); sapling.SummonedBy = u.Id; }
                        break;
                    case "EndlessIndex": pages = Mathf.Max(pages, Mathf.Min(3, catalog.IndexPages / 10)); break;
                }
            }
            foreach (UnitState u in units)
            {
                if (u.Faction != Faction.Companion) continue;
                if (herald) u.HeraldShot = true;
                if (pages > 0 && System.Array.IndexOf(u.Definition.Tags, "rune") >= 0 && u.Definition.Weapon.Kind == WeaponKind.Projectile) u.Definition.Weapon.ProjectileSplit += pages;
            }
        }
        /// <summary>Eclipse Egg: the egg takes one form for the fight (its look and weapon; the weapon keeps the egg's star power).</summary>
        private void Hatch(UnitState egg, UnitDefinition form)
        {
            UnitDefinition hatched = Object.Instantiate(egg.Definition);
            hatched.Body = form.Body; hatched.AnimationFrames = form.AnimationFrames; hatched.AuthoredMotion = form.AuthoredMotion;
            WeaponDefinition weapon = Object.Instantiate(form.Weapon);
            weapon.Damage *= Mathf.Pow(2.7f, Mathf.Clamp(egg.Definition.CombatTier, 1, 3) - 1);
            hatched.Weapon = weapon; hatched.Description = form.Id; egg.Definition = hatched;
            Skill(egg, MorphGeometry.Disc, egg.Position, egg.Position, .4f);
        }

        /// <summary>Iron Locomotive (v5): a cold blast ahead freezes the first three enemies in it for 1s.</summary>
        private void FrostWhistle(UnitState u, UnitState ahead)
        {
            Vector2 dir = (ahead.Position - u.Position).normalized;
            skillTargets.Clear();
            foreach (UnitState t in units)
            {
                Vector2 off = t.Position - u.Position;
                if (CanHit(Faction.Champion, t) && off.sqrMagnitude <= 3.5f * 3.5f && Vector2.Dot(off.normalized, dir) > .6f) skillTargets.Add(t);
            }
            skillTargets.Sort((a, b) => (a.Position - u.Position).sqrMagnitude.CompareTo((b.Position - u.Position).sqrMagnitude));
            for (int k = 0; k < Mathf.Min(3, skillTargets.Count); k++)
            {
                UnitState t = skillTargets[k];
                ApplyWeaponDamage(t, u.Definition.Weapon.Damage * 1.2f, Faction.Companion, DamageElement.Frost, "Iron Locomotive", dir, cause: NewCause(u.Id).Child("frost-whistle"));
                if (t.Alive) Freeze(t, 1);
            }
            ArsenalProp gust = SkillProp(ArsenalPropKind.Gust, u, u.Position + dir * .3f, 0, 3.5f, .45f, "frost-whistle"); gust.Direction = dir;
            CompanionSkillCasts++;
        }
        /// <summary>Paper Inventor (v5): once a fight a paper tower stands ahead and fires darts at the enemies near it.</summary>
        private void PaperTower(UnitState u, UnitState first)
        {
            towerBuilt.Add(u.Id);
            ArsenalProp tower = SkillProp(ArsenalPropKind.Tower, u, Toward(u, first.Position, 1.2f), u.Definition.Weapon.Damage * .8f, 5f, float.PositiveInfinity, "paper-tower");
            tower.NextTick = Elapsed + .4f;
            Skill(u, MorphGeometry.Disc, tower.Position, tower.Position, .3f);
        }

        private void Chain(UnitState actor, UnitState from, float damage, int hops, string id)
        {
            var struck = new HashSet<int> { from.Id };
            for (int h = 0; h < hops; h++)
            {
                UnitState next = null; float best = 2.5f * 2.5f;
                foreach (UnitState u in units) if (!struck.Contains(u.Id) && CanHit(Faction.Champion, u)) { float d = (u.Position - from.Position).sqrMagnitude; if (d < best) { best = d; next = u; } }
                if (next == null) return;
                struck.Add(next.Id);
                ApplyWeaponDamage(next, damage * (h == 0 ? .5f : .35f), Faction.Companion, DamageElement.Arcane, id, (next.Position - from.Position).normalized, cause: NewCause(actor.Id).Child(id));
                IdentityGlyph(MorphGeometry.Line, from.Position, next.Position, 0, .25f, 0, HeroGlyphRole.Strike);
                CompanionSkillCasts++;
                from = next;
            }
        }

        private void ProcessNewCompanionSkills()
        {
            if (!newSkillsStarted) StartNewCompanionSkills();
            int count = events.Count;
            for (int i = 0; i < count; i++)
            {
                CombatEvent e = events[i];
                if (e.Cause.Depth != 0) continue;
                UnitState actor = UnitWithId(e.Cause.SourceUnitId);
                if (actor == null || actor.Faction != Faction.Companion || !actor.Alive) continue;
                string id = actor.Definition.Id;
                if (e.Kind == CombatEventKind.Fire)
                {
                    if (actor.HeraldShot && actor.Definition.Weapon.Kind != WeaponKind.Projectile) actor.HeraldShot = false;
                    skillCount.TryGetValue(actor.Id, out int n);
                    UnitState target = UnitWithId(actor.AttackTargetId);
                    if (id == "EndlessIndex" && n % 3 == 0 && target != null)
                    {
                        // the index copies the blow onto two more enemies near its target
                        int copied = 0;
                        foreach (UnitState u in units.ToArray())
                            if (copied < 2 && u != target && CanHit(Faction.Champion, u) && (u.Position - target.Position).sqrMagnitude <= 2.25f)
                            {
                                copied++;
                                ApplyWeaponDamage(u, actor.Definition.Weapon.Damage, Faction.Companion, DamageElement.Arcane, "The Endless Index", (u.Position - target.Position).normalized, cause: NewCause(actor.Id).Child("endless-index"));
                                IdentityGlyph(MorphGeometry.Line, target.Position, u.Position, 0, .3f, 0, HeroGlyphRole.Strike);
                            }
                        CompanionSkillCasts++;
                    }
                    if (id == "SundaeGolem" && target != null)
                    {
                        // every blow leaves a puddle of cream under its enemy (three at most)
                        SkillProp(ArsenalPropKind.Cream, actor, target.Position, 0, .6f, 3f, "sundae-cream");
                        DropOldest(actor, ArsenalPropKind.Cream, 3);
                        CompanionSkillCasts++;
                    }
                }
                if (e.Kind == CombatEventKind.Hit && e.Faction == Faction.Enemy && e.Subject != null)
                {
                    UnitState target = e.Subject;
                    hitCount.TryGetValue(actor.Id, out int h); hitCount[actor.Id] = ++h;
                    switch (id)
                    {
                        case "StaticFerret": Chain(actor, target, actor.Definition.Weapon.Damage, 1, "static-ferret"); break;
                        case "VoltEel": Chain(actor, target, actor.Definition.Weapon.Damage, 2, "volt-eel"); break;
                        case "ShantySeaLion":
                            if (h % 3 == 0 && target.Alive && !target.Definition.IsBoss)
                            {
                                Vector2 away = (target.Position - actor.Position).sqrMagnitude > .0001f ? (target.Position - actor.Position).normalized : Vector2.right;
                                target.Position = ClampPosition(target.Position + away * OctoberBalance.Push(target.Definition, .6f), target.Definition.Radius); spatial.Move(target);
                                events.Add(new CombatEvent(CombatEventKind.Knockback, target.Position, Faction.Companion, direction: away, cause: NewCause(actor.Id).Child("shanty-note")));
                                Skill(actor, MorphGeometry.Line, actor.Position, target.Position, 0);
                            }
                            break;
                        case "PinnedBeetle":
                            if (target.Alive && target.PinReadyAt <= Elapsed) { target.PinReadyAt = Elapsed + 2; Sleep(target, .3f); Skill(actor, MorphGeometry.Disc, target.Position, target.Position, .25f); }
                            break;
                    }
                }
                if (e.Kind == CombatEventKind.Kill && e.Faction == Faction.Enemy && e.Subject != null && id == "SpecimenSlime" && miniSlimes < 2 && Recorded(e.Subject)
                    && ExpansionArtwork.Current?.MiniSlime != null)
                {
                    // a recorded species splits off a little slime that fights beside it
                    miniSlimes++;
                    UnitState slime = Spawn(ExpansionArtwork.Current.MiniSlime, Faction.Companion, ClampPosition(e.Subject.Position, .3f)); slime.SummonedBy = actor.Id;
                    Skill(actor, MorphGeometry.Disc, e.Subject.Position, e.Subject.Position, .3f);
                }
            }
            foreach (UnitState u in units.ToArray())
            {
                if (!u.Alive || u.Faction != Faction.Companion) continue;
                switch (u.Definition.Id)
                {
                    case "TeaDragon":
                        if (Nearest(u.Position, 5f) is UnitState brew && SkillReady(u, 7))
                        {
                            ArsenalProp pot = SkillProp(ArsenalPropKind.Teapot, u, Toward(u, brew.Position, 1.6f), u.Definition.Weapon.Damage * 2.2f, 1f, 1.9f, "tea-dragon");
                            pot.Start = u.Position; pot.LandAt = Elapsed + .35f; pot.FuseAt = Elapsed + 1.3f;
                            CompanionSkillCasts++;
                        }
                        break;
                    case "LighthouseWhale":
                        if (Nearest(Champion.Position, 6f) is UnitState spray && SkillReady(u, 6))
                        {
                            // a column of water on the enemy nearest the hero: it soaks and pushes the ones around away from the hero
                            foreach (UnitState t in units.ToArray())
                            {
                                Vector2 off = t.Position - spray.Position;
                                if (!CanHit(Faction.Champion, t) || off.sqrMagnitude > .9f * .9f) continue;
                                Vector2 away = (t.Position - Champion.Position).sqrMagnitude > .0001f ? (t.Position - Champion.Position).normalized : Vector2.right;
                                ApplyWeaponDamage(t, u.Definition.Weapon.Damage * 1.6f, Faction.Companion, DamageElement.Physical, "Harbor Whale", away, cause: NewCause(u.Id).Child("harbor-spout"));
                                if (!t.Alive) continue;
                                t.KitWetUntil = Elapsed + 3;
                                if (!t.Definition.IsBoss) { t.Position = ClampPosition(t.Position + away * OctoberBalance.Push(t.Definition, .6f), t.Definition.Radius); spatial.Move(t); }
                            }
                            SkillProp(ArsenalPropKind.Spout, u, spray.Position, 0, .9f, .6f, "harbor-spout").Start = u.Position;
                            CompanionSkillCasts++;
                        }
                        break;
                    case "JackInTheBox":
                        if (Nearest(u.Position, 3.5f) != null && SkillReady(u, 6))
                        {
                            foreach (UnitState t in units)
                                if (CanHit(Faction.Champion, t) && !t.Definition.IsBoss && (t.Position - u.Position).sqrMagnitude <= 3.5f * 3.5f) { t.TauntBy = u.Id; t.TauntUntil = Elapsed + 1.5f; }
                            SkillProp(ArsenalPropKind.Taunt, u, u.Position, 0, 3.5f, 1.5f, "ringmaster-ball");
                            CompanionSkillCasts++;
                        }
                        break;
                    case "KiteDragon":
                        if (Nearest(u.Position, 6f) is UnitState gale && SkillReady(u, 5))
                        {
                            // a band of wind cut across the field ahead: struck along its whole length and shoved aside
                            Vector2 dir = (gale.Position - u.Position).normalized, from = u.Position, to = ClampPosition(from + dir * 6.5f, .3f), side = new Vector2(-dir.y, dir.x);
                            foreach (UnitState t in units.ToArray())
                                if (CanHit(Faction.Champion, t) && BodySegment(t, from, to, .45f) >= 0)
                                {
                                    ApplyWeaponDamage(t, u.Definition.Weapon.Damage * 2f, Faction.Companion, DamageElement.Arcane, "Kite Dragon", dir, cause: NewCause(u.Id).Child("kite-gale"));
                                    if (t.Alive && !t.Definition.IsBoss) { t.Position = ClampPosition(t.Position + side * OctoberBalance.Push(t.Definition, .4f), t.Definition.Radius); spatial.Move(t); }
                                }
                            ArsenalProp wind = SkillProp(ArsenalPropKind.Wind, u, to, 0, .45f, .55f, "kite-gale"); wind.Start = from; wind.Direction = dir;
                            CompanionSkillCasts++;
                        }
                        break;
                    case "LostCarousel":
                        if (SkillReady(u, 10) && ArsenalCrowd(u, 6f) is Vector2 crowd)
                        {
                            ArsenalProp ride = SkillProp(ArsenalPropKind.Carousel, u, crowd, u.Definition.Weapon.Damage * .6f, 1.2f, 4f, "lost-carousel");
                            ride.Start = crowd; CompanionSkillCasts++;
                        }
                        else if (skillAt.TryGetValue(u.Id, out float at) && at > Elapsed + 9.9f && ArsenalCrowd(u, 6f) == null) skillAt[u.Id] = Elapsed; // nobody to ride round yet
                        break;
                    case "FirstSeed":
                        if (Nearest(u.Position, 5f) is UnitState thorns && SkillReady(u, 8))
                        {
                            Vector2 dir = (thorns.Position - u.Position).normalized;
                            ArsenalProp hedge = SkillProp(ArsenalPropKind.Hedge, u, Toward(u, thorns.Position, 1.5f), u.Definition.Weapon.Damage * .25f, 1f, 3f, "first-seed-hedge");
                            hedge.Direction = new Vector2(-dir.y, dir.x);
                            CompanionSkillCasts++;
                        }
                        break;
                    case "StormKitten":
                        if (Nearest(u.Position, 5f) is UnitState bolt && SkillReady(u, 6))
                        {
                            ApplyWeaponDamage(bolt, u.Definition.Weapon.Damage * 1.6f, Faction.Companion, DamageElement.Arcane, "Storm Kitten", Vector2.down, cause: NewCause(u.Id).Child("storm-kitten"));
                            IdentityGlyph(MorphGeometry.Line, bolt.Position + new Vector2(.3f, 2.2f), bolt.Position, 0, .3f, 0, HeroGlyphRole.Strike);
                            CompanionSkillCasts++;
                        }
                        break;
                    case "FossilArmadillo":
                        foreach (UnitState t in units)
                            if (CanHit(Faction.Champion, t) && t.PinReadyAt <= Elapsed && (t.Position - u.Position).sqrMagnitude <= Mathf.Pow(u.Definition.Radius + t.Definition.Radius + .15f, 2))
                            { t.PinReadyAt = Elapsed + 2; Sleep(t, .3f); Skill(u, MorphGeometry.Disc, t.Position, t.Position, .25f); }
                        break;
                }
                if (u.Definition.Description == "EclipseEgg_Support" && SkillReady(u, 4))
                {
                    // the herald form hastens the friends near it
                    foreach (UnitState f in units) if (f.Alive && f.Faction == Faction.Companion && (f.Position - u.Position).sqrMagnitude <= 9) f.HasteUntil = Elapsed + 2;
                    Skill(u, MorphGeometry.Disc, u.Position, u.Position, .35f);
                }
            }
            ProcessSkillProps();
        }

        private void ProcessSkillProps()
        {
            foreach (UnitState u in units) if (u.Faction == Faction.Enemy && Elapsed - u.CreamSeen > .2f) u.CreamTime = 0;
            for (int i = arsenalProps.Count - 1; i >= 0; i--)
            {
                if (i >= arsenalProps.Count) continue;
                ArsenalProp p = arsenalProps[i]; UnitState source = p.Source;
                if (p.Kind < ArsenalPropKind.Teapot) continue;
                if (source == null || !source.Alive) { arsenalProps.RemoveAt(i); continue; }
                switch (p.Kind)
                {
                    case ArsenalPropKind.Teapot:
                        if (p.Stage == 0 && Elapsed >= p.FuseAt)
                        {
                            // the teapot whistles and bursts; the steam spreads once to the enemies beside the scalded ones
                            p.Stage = 1; p.Until = Elapsed + .6f;
                            var scalded = new List<UnitState>();
                            foreach (UnitState t in units.ToArray())
                                if (CanHit(Faction.Champion, t) && InBody(t, p.Position, p.Radius))
                                { scalded.Add(t); ApplyWeaponDamage(t, p.Damage, Faction.Companion, DamageElement.Ember, "Tea Dragon", t.Position - p.Position, cause: p.Cause); }
                            var spread = new HashSet<int>();
                            foreach (UnitState s in scalded)
                                foreach (UnitState t in units.ToArray())
                                    if (!scalded.Contains(t) && !spread.Contains(t.Id) && CanHit(Faction.Champion, t) && (t.Position - s.Position).sqrMagnitude <= 1)
                                    { spread.Add(t.Id); ApplyWeaponDamage(t, p.Damage * .5f, Faction.Companion, DamageElement.Ember, "Tea Dragon", t.Position - s.Position, cause: p.Cause.Child("tea-steam")); }
                        }
                        break;
                    case ArsenalPropKind.Cream:
                        foreach (UnitState t in units)
                        {
                            if (!CanHit(Faction.Champion, t) || (t.Position - p.Position).sqrMagnitude > Mathf.Pow(p.Radius + t.Definition.Radius * .5f, 2)) continue;
                            if (Elapsed - t.CreamSeen > .2f) t.CreamTime = 0;
                            t.CreamTime += Elapsed - Mathf.Max(t.CreamSeen, Elapsed - .1f); t.CreamSeen = Elapsed;
                            if (t.CreamTime >= 2) { t.CreamTime = 0; Freeze(t, 1); }
                        }
                        break;
                    case ArsenalPropKind.Carousel:
                        if (Elapsed >= p.NextTick)
                        {
                            p.NextTick = Elapsed + .3f;
                            float spin = (Elapsed - p.Born) * 2f;
                            for (int k = 0; k < 4; k++)
                            {
                                Vector2 horse = p.Start + new Vector2(Mathf.Cos(spin + k * Mathf.PI / 2), Mathf.Sin(spin + k * Mathf.PI / 2) * .7f) * p.Radius;
                                Vector2 tangent = new Vector2(-Mathf.Sin(spin + k * Mathf.PI / 2), Mathf.Cos(spin + k * Mathf.PI / 2));
                                foreach (UnitState t in units.ToArray())
                                {
                                    if (!CanHit(Faction.Champion, t) || !InBody(t, horse, .45f)) continue;
                                    ApplyWeaponDamage(t, p.Damage, Faction.Companion, DamageElement.Physical, "The Lost Carousel", tangent, cause: p.Cause);
                                    if (t.Alive && !t.Definition.IsBoss) { t.Position = ClampPosition(t.Position + tangent * OctoberBalance.Push(t.Definition, .3f), t.Definition.Radius); spatial.Move(t); }
                                }
                            }
                        }
                        break;
                    case ArsenalPropKind.Hedge:
                        if (Elapsed >= p.NextTick)
                        {
                            // the thorn hedge stops whatever walks into it and scratches it
                            p.NextTick = Elapsed + .25f; p.Order++;
                            Vector2 along = p.Direction.sqrMagnitude > .001f ? p.Direction.normalized : Vector2.right, across = new Vector2(-along.y, along.x);
                            foreach (UnitState t in units.ToArray())
                            {
                                Vector2 off = t.Position - p.Position;
                                if (!CanHit(Faction.Champion, t) || Mathf.Abs(Vector2.Dot(off, along)) > 1f + t.Definition.Radius || Mathf.Abs(Vector2.Dot(off, across)) > .3f + t.Definition.Radius) continue;
                                Sleep(t, .3f);
                                if (p.Order % 2 == 0) ApplyWeaponDamage(t, p.Damage, Faction.Companion, DamageElement.Physical, "The First Seed", across, cause: p.Cause);
                            }
                        }
                        break;
                    case ArsenalPropKind.Tower:
                        if (Elapsed >= p.NextTick)
                        {
                            p.NextTick = Elapsed + 1f;
                            UnitState mark = Nearest(p.Position, p.Radius);
                            if (mark != null)
                            {
                                ProjectileState dart = ArsenalShot(source, p.Position + Vector2.up * .4f, mark.Position - p.Position, 8f, p.Radius + 1, p.Damage, false, p.Cause, .1f);
                                dart.Arsenal = ArsenalWeapon.None;
                            }
                        }
                        break;
                }
                if (Elapsed > p.Until) arsenalProps.Remove(p);
            }
        }
    }
}
