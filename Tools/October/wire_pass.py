exec(Path(__file__).with_name('finish_pass.py').read_text().split('for i,name')[0]) if False else None
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
def edit(rel,old,new):
 p=ROOT/rel;s=p.read_text(encoding='utf-8-sig')
 if old not in s: raise RuntimeError(f'Cannot find {old[:90]} in {rel}')
 p.write_text(s.replace(old,new),encoding='utf-8')
C='Assets/ArcaneBrigade/Runtime/'
edit(C+'Core/RunSession.cs','difficulty > 4','difficulty >= RunMeta.DifficultyNames.Length')
edit(C+'Core/RunSession.cs','State.RelicGains ??=','State.OctoberGains ??= new List<StatModifier>();\n            State.AbsorbedSkills ??= new List<string>();\n            State.AchievementEvidence ??= new List<AchievementMetric>();\n            State.RelicGains ??=')
edit(C+'Core/RunSession.cs','ValidateRelicState(snapshot);','ValidateRelicState(snapshot);\n            ValidateOctoberState(snapshot);')
edit(C+'Core/RunSession.cs','CompileOctoberRoster();','CompileOctoberRoster();\n            RunAchievements.Observe(this, catalog, CombatCatalog);')
edit(C+'Core/RunSession.cs','State.OctoberHearts =','RunAchievements.Mark(State.AchievementEvidence, "battle-gold", simulation.Gold);\n            State.OctoberHearts =')
edit(C+'Core/RunAchievements.cs','OutsideStageStatus.Claimed','OutsideStageStatus.Completed')
edit(C+'Persistence/RunStorage.cs','public int Cosmetic;','public int Cosmetic;\n        public List<AchievementMetric> AchievementEvidence = new List<AchievementMetric>();')
edit(C+'Persistence/RunStorage.cs','RunMeta.Credit(profile, run);','RunMeta.Credit(profile, run);\n                RunAchievements.Credit(profile, run, catalog);')
edit(C+'UI/RunMenu.cs','private int historySelection;','private int historySelection;\n        private readonly AchievementJournal achievementBook = new AchievementJournal();')
edit(C+'UI/RunMenu.cs','+ " / 5"','+ " / " + RunMeta.DifficultyNames.Length')
edit(C+'UI/ShortOfferCopy.cs','public static string Companion(UnitDefinition unit) =>','public static string Companion(UnitDefinition unit) => !string.IsNullOrEmpty(unit.ReferenceCompanion) ? unit.Description :')
edit(C+'UI/RunUICopy.cs','return unit.Description + "\\n\\n" + OctoberContent.HeroCopy(unit.ReferenceHero);','return unit.Description == OctoberContent.HeroCopy(unit.ReferenceHero) ? unit.Description : unit.Description + "\\n\\n" + OctoberContent.HeroCopy(unit.ReferenceHero);')
# Absorption heroes can shop for companion memories while retaining a solo body.
edit(C+'Core/K11HeroRules.cs','if (champion.HeroMechanic == HeroMechanic.SoloMaw) return false;','if (champion.HeroMechanic == HeroMechanic.SoloMaw && champion.ReferenceHero != "Capped Gallant") return false;')
edit(C+'Core/RunSession.cs','if (OctoberHero == "Wise") {','if (OctoberAbsorbs) {')
edit(C+'Core/RunSession.cs','&& OctoberHero != "Wise") return false;','&& !OctoberAbsorbs) return false;')
edit(C+'Core/RunSession.cs','if (offer.Kind == OfferKind.Companion && catalog.Unit(State.ChampionId).HeroMechanic == HeroMechanic.SoloMaw) return false;','if (offer.Kind == OfferKind.Companion && catalog.Unit(State.ChampionId).HeroMechanic == HeroMechanic.SoloMaw && !OctoberAbsorbs) return false;')
edit(C+'Core/RunSession.cs','if (catalog.Unit(State.ChampionId).HeroMechanic != HeroMechanic.SoloMaw && unit.SpecialRecruit','if ((catalog.Unit(State.ChampionId).HeroMechanic != HeroMechanic.SoloMaw || OctoberAbsorbs) && unit.SpecialRecruit')
# Preserve ordinary boss organ size independently of the new smaller body.
edit(C+'Presentation/BossAnatomyVisual.cs','float scale = sizes[part] /','float scale = (unit.Definition.Id == "OutsideStageKeeper" || unit.Definition.Id == NightmareRules.CustomerId ? 1 : 1.25f) * sizes[part] /')
# Rarity uses delicate elliptical filigree with soft backlight, no tall opaque light bars.
edit(C+'Presentation/ShopRarityGlow.cs','ray.positionCount = 2;','ray.positionCount = 13;')
edit(C+'Presentation/ShopRarityGlow.cs','ray.startWidth = 0.055f; ray.endWidth = 0.005f;','ray.startWidth = 0.012f; ray.endWidth = 0.012f;')
edit(C+'Presentation/ShopRarityGlow.cs','(index == 0 ? 0.85f : 0.5f)','(index == 0 ? 0.25f : 0.08f)')
edit(C+'Presentation/ShopRarityGlow.cs','new Vector3(0.42f, 2.2f, 1)','new Vector3(.65f, .65f, 1)')
edit(C+'Presentation/ShopRarityGlow.cs','rays[index].SetPosition(0, direction * 0.28f);','for (int p = 0; p < 13; p++)\n                {\n                    float a = angle + (p / 12f - .5f) * .55f;\n                    float radius = .65f + .06f * Mathf.Sin(p / 12f * Mathf.PI);\n                    rays[index].SetPosition(p, new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius * .72f, 0));\n                }')
edit(C+'Presentation/ShopRarityGlow.cs','rays[index].SetPosition(1, direction * (0.62f + strength * 0.26f) * pulse * flare);','// Short separated arcs leave the merchandise silhouette unobscured.')
edit(C+'Presentation/ShopRarityGlow.cs','0.72f * visibility','0.45f * visibility')
# Clearer relic ownership cards: rarity stripe and an isolated large selected icon.
edit(C+'UI/RunBuildReadModel.cs','public Sprite WeaponIcon;','public Sprite WeaponIcon; public Color Accent;')
edit(C+'UI/RunBuildReadModel.cs','Name = item.Name, Icon = item.Icon, Summary','Name = item.Name, Icon = item.Icon, Accent = RunModifierRules.RarityColor(item.Rarity), Summary')
edit(C+'UI/RunDetails.cs','float contentLeft = row.Icon != null ? 120 : 28;','if (tab == 2)\n                {\n                    ArcadeSkin.Line(new Rect(card.x + 2, card.y + 4, 5, card.height - 8), row.Accent);\n                    RunHud.Fill(new Rect(18, card.y + 12, 90, 100), new Color(.09f, .075f, .14f));\n                }\n                float contentLeft = row.Icon != null ? 130 : 28;')
edit(C+'UI/RunDetails.cs','string explanation = rows.Count == 0 ?','if (tab == 2 && rows.Count > 0)\n            {\n                var chosen = rows[Mathf.Clamp(selection, 0, rows.Count - 1)];\n                RunHud.Sprite(new Rect(1185, 325, 100, 100), chosen.Icon);\n                GUI.Label(new Rect(1310, 340, 480, 70), chosen.Name, text);\n            }\n            string explanation = rows.Count == 0 ?')
edit(C+'UI/RunDetails.cs','Mathf.Max(620, GlossaryUI.Height','Mathf.Max(620, GlossaryUI.Height')
edit(C+'UI/RunDetails.cs','new Rect(1180, 325, 625, 610)','new Rect(1180, tab == 2 ? 445 : 325, 625, tab == 2 ? 490 : 610)')
print('October UI and persistence wiring applied')
