# K11 — seamless living arena, themed recruitment and English

User requirements: no scene/loading transition between waves; animated merchant/stock arrivals and departures; special merchant first then regular shop; evolving animated arena colors and richer scenery; distinct hostile weapons; larger allies; concise synergy previews; more challenging fair combat and viable varied melee; English default with Vietnamese option; protagonist-specific recruit pools and automatic mechanics.

1. Audit wave/shop lifecycle and reference; preserve movement-only, blood/egg prices, run-only stat persistence and transactional saves.
2. Implement in-place wave lifecycle, sequential merchant stage persistence, staged rug/merchant/stock motion and dream atmosphere. No black loading curtain or inter-wave SceneManager load.
3. Enemies attack with their own bodies (mouth spits, rams, body sweeps), no held hostile weapons per updated decision. Add richer nonblocking scenery and animated palette progression; enlarge allied body/weapon readability. An exploratory hostile-weapon atlas was generated before the decision changed and is not shipped or referenced.
4. Implement theme pools, original protagonist mechanics and relic tradeoffs; rebalance melee/ranged and difficulty without first-wave shooters or lethal one-hit damage.
5. Add English-default persisted language setting, translate UI/content/tooltips/dialogue and concise highlighted synergy previews.
6. Run focused/legacy tests, Windows build, native transition/shop/language captures and balance measurements; document actual results and limitations, push source/assets/evidence. Human feel and mobile gates remain open.

Reference audit records verified/inferred/inaccessible material separately. No claim of exact reference formulas, copied art or every reference item. Technical performance measurements do not replace human quality approval.

## Checkpoint status

K11.1 implemented and Windows technical gate passed: same-scene wave continuation, sequential special-to-regular merchant stage with save/restore, animated market arrival/departure/dream atmosphere, smooth palette transitions, denser scenery, larger allies, enemy body attacks with no held weapons. See `K11_Transition_Checkpoint.md` for actual evidence and limitations.

Latest merchant requirement added: two new original four-frame giant merchant strips, four merchant silhouettes in rotation, six large/colossal height presets by hall/shop identity. Merchant staging uses camera bounds so oversized crowns stay in view; ground shadows derive from sprite pivot and avoidance follows the actual NPC center. Native review caught a clipped crown and prompted this framing fix before finalizing.

Remaining K11 work: complete English-default localization; themed recruit pools and unique automatic protagonist mechanics; artifact variety and highlighted concise synergy previews; melee distribution/reach and harder measured balance; dedicated new enemy attack artwork and richer bespoke environmental art. These are pending, not implied finished by the transition gate.
