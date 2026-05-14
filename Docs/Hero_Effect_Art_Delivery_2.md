# Hero effect artwork ? part 2 delivery

24 October 2026. Built-in `painted`; identity refs and prompt set in [manifest](Hero_Effect_Art_Manifest_2.json). All files saved under `Assets/ArcaneBrigade/Art/Playtest/HeroEffects/`.

24 sheets / **78 frames**, not the 76 originally totaled in the request. Each cell 256?256, genuine RGBA alpha, four corner pixels alpha 0, at least 30px empty border in every exported cell. Total PNG size **3,332,719 bytes** (3.18 MiB). No code-painted replacements; processing only rescales, aligns cells and removes faint alpha debris as previously authorized. Physical material colours only; no backdrop or ground shadows.

| Sheet | Frames |
| --- | ---: |
| LanternKnight_LighthouseLens.png | 4 |
| DrowsySeer_MoonPhase.png | 4 |
| RootOracle_RootTip.png | 3 |
| LoneJelly_Organs.png | 3 |
| GlassDice_Die.png | 6 |
| TideSinger_WaveCrest.png | 3 |
| CometRunner_Comet.png | 3 |
| VerdantMatron_Bud.png | 3 |
| ChinaMarionette_Shard.png | 3 |
| SoapSkater_IceChips.png | 3 |
| VelvetRegent_Banner.png | 3 |
| ButtonCavalier_Buttons.png | 3 |
| NightMaw_Jaw.png | 3 |
| ThornTreasurer_ThornCrown.png | 4 |
| IvorySovereign_Decree.png | 3 |
| PatchworkHost_Curtain.png | 3 |
| ArchiveCurator_Book.png | 4 |
| PennyBroker_Receipt.png | 3 |
| StormFencer_Sheath.png | 3 |
| TinVirtuoso_MusicStand.png | 3 |
| MoonProwler_ShadowCat.png | 3 |
| LanternWeaver_Lantern.png | 3 |
| RainArchivist_Cloud.png | 3 |
| SundaeDuchess_Cherry.png | 2 |

Unity batch import completed successfully: **38 sheets / 118 sprites** across both manifests; all keys verified in `Resources/HeroEffectArtLibrary.asset`. The existing runtime already requests these keys. This verifies import and library references, not a new Windows build or visual playthrough of every hero. The 1536px dice strip uses a 2048 texture cap so its cells remain 256px.

[Review board](../PreviewExports/HeroEffects/ContactSheet2.jpg) ? [image QA](Hero_Effect_Art_Verification_2.json) ? [Unity evidence](Hero_Effect_Art_Unity_Verification_2.txt).
