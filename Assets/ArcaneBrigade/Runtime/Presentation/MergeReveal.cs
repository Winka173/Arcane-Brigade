using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class MergeReveal : MonoBehaviour
    {
        public sealed class Beat
        {
            public RosterEntry Result;
            public List<RosterEntry> Materials;
            public Vector2 Position;
            public bool Altar;
        }

        private ArenaController owner;
        private readonly Queue<Beat> queue = new Queue<Beat>();
        private readonly List<UnitVisual> ghosts = new List<UnitVisual>();
        private readonly List<UnitDefinition> definitions = new List<UnitDefinition>();
        private GameObject stage;
        private Beat active;
        private float age;
        private string runId;
        private int lastReceipt;
        private LineRenderer ring;
        private Material ringMaterial;
        private AudioSource confirmation;
        private AudioClip confirmationClip;
        private bool sounded;
        private SpriteRenderer purchaseShadow;
        private SpriteRenderer confirmationSpark;
        private Sprite[] confirmationFrames;
        public bool ReadOnly { get; set; }
        public bool ReducedMotion { get; set; }
        public int PendingCount => queue.Count + (active == null ? 0 : 1);
        public int CompletedCount { get; private set; }
        public void Configure(ArenaController controller) { owner = controller; }
        public static RunSnapshot Capture(RunSnapshot state) => JsonUtility.FromJson<RunSnapshot>(JsonUtility.ToJson(state));

        private static bool Same(RosterEntry first, RosterEntry second) => first.UnitId == second.UnitId && first.WeaponId == second.WeaponId && first.Tier == second.Tier;
        private static RosterEntry Copy(RosterEntry entry) => new RosterEntry { UnitId = entry.UnitId, WeaponId = entry.WeaponId, Tier = entry.Tier };

        public static List<Beat> CommittedBeats(RunSnapshot before, RunSnapshot after, bool committed, Vector2 position, bool altar)
        {
            var beats = new List<Beat>();
            if (!committed || before == null || after == null || before.Phase != SessionPhase.Shop || after.Phase != SessionPhase.Shop || before.RunId != after.RunId || after.NextReceipt != before.NextReceipt + 1 || !after.Receipts.Exists(receipt => receipt.Id == after.NextReceipt)) return beats;
            var removed = before.Roster.FindAll(entry => !after.Roster.Exists(candidate => Same(entry, candidate)));
            foreach (RosterEntry result in after.Roster)
            {
                if (before.Roster.Exists(entry => Same(entry, result))) continue;
                List<RosterEntry> materials = altar ? new List<RosterEntry>(removed) : removed.FindAll(entry => entry.UnitId == result.UnitId && entry.Tier < result.Tier);
                if (!altar)
                {
                    materials.Sort((first, second) => second.Tier.CompareTo(first.Tier));
                    float capacity = Mathf.Pow(3, result.Tier - 1);
                    var allocated = new List<RosterEntry>();
                    foreach (RosterEntry material in materials)
                    {
                        float weight = Mathf.Pow(3, material.Tier - 1);
                        if (weight > capacity) continue;
                        allocated.Add(material); capacity -= weight;
                    }
                    materials = allocated;
                }
                if (materials.Count < 2 || !altar && result.Tier <= 1) continue;
                var copies = new List<RosterEntry>();
                foreach (RosterEntry material in materials) copies.Add(Copy(material));
                beats.Add(new Beat { Result = Copy(result), Materials = copies, Position = position, Altar = altar });
                foreach (RosterEntry material in materials) removed.Remove(material);
                if (altar) break;
            }
            return beats;
        }

        public void EnqueueCommitted(RunSnapshot before, RunSnapshot after, bool committed, Vector2 position, bool altar)
        {
            if (after == null) return;
            if (runId != after.RunId) { Skip(); runId = after.RunId; lastReceipt = 0; CompletedCount = 0; }
            if (after.NextReceipt <= lastReceipt) return;
            List<Beat> beats = CommittedBeats(before, after, committed, position, altar);
            if (!committed) return;
            lastReceipt = after.NextReceipt;
            if (ReadOnly) { Skip(); return; }
            foreach (Beat beat in beats) queue.Enqueue(beat);
        }

        public void Skip() { queue.Clear(); ClearStage(); if (confirmation != null) confirmation.Stop(); }

        private void Update()
        {
            if (owner == null || owner.Session == null || owner.Simulation == null) return;
            if (ReadOnly) { Skip(); return; }
            if (owner.Session.State.RunId != runId || owner.Session.State.Phase != SessionPhase.Shop) { Skip(); return; }
            if (owner.Simulation.Phase == RunPhase.Paused || owner.Details.Open || owner.MenuConfirmation) { Skip(); return; }
            if (owner.Simulation.Phase == RunPhase.Rest && Input.GetKeyDown(KeyCode.Backspace)) { Skip(); return; }
            if (active == null)
            {
                if (queue.Count == 0) return;
                active = queue.Dequeue(); age = 0;
                CheckpointAudio.PlayInterfaceId("up.merge.gather");
                if (active.Altar) CheckpointAudio.PlayInterfaceId("shop.fusion", 0.09f);
                sounded = false;
                stage = new GameObject(active.Altar ? "AltarRevealGhosts" : "MergeRevealGhosts");
                stage.transform.SetParent(transform, false);
                stage.transform.position = active.Position + Vector2.up * 1.3f;
                foreach (RosterEntry material in active.Materials) ghosts.Add(CreateGhost(material));
                ghosts.Add(CreateGhost(active.Result));
                if (!active.Altar)
                {
                    var material = new GameObject("PurchasedBodyShadow_NoUnobservedWeapon");
                    material.transform.SetParent(stage.transform, false);
                    purchaseShadow = material.AddComponent<SpriteRenderer>();
                    purchaseShadow.sprite = owner.Content.Unit(active.Result.UnitId).Body;
                    material.transform.localScale = Vector3.one * (0.45f / Mathf.Max(0.01f, purchaseShadow.sprite.bounds.size.y));
                    purchaseShadow.sortingOrder = 2290;
                }
                CreateConfirmation();
            }
            age += Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            float gather = Mathf.SmoothStep(0, 1, Mathf.Clamp01(age / 0.28f));
            if (purchaseShadow != null)
            {
                purchaseShadow.enabled = !ReducedMotion && age < 0.32f;
                purchaseShadow.transform.localPosition = Vector3.up * (1 - gather) * 0.8f;
                purchaseShadow.color = new Color(0.25f, 0.2f, 0.35f, 1 - gather * 0.7f);
            }
            for (int index = 0; index < ghosts.Count; index++)
            {
                UnitVisual ghost = ghosts[index];
                bool result = index == ghosts.Count - 1;
                ghost.gameObject.SetActive(result ? age >= 0.36f || ReducedMotion : age < 0.32f && !ReducedMotion);
                if (!ghost.gameObject.activeSelf) continue;
                float angle = index * Mathf.PI * 2 / Mathf.Max(1, ghosts.Count - 1) + (active.Altar ? Mathf.PI * 0.5f : 0);
                Vector2 offset = result || ReducedMotion ? Vector2.zero : new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (1 - gather) * 0.8f;
                ghost.Display(0, 1);
                ghost.transform.position = stage.transform.position + (Vector3)offset;
                ghost.transform.localScale = Vector3.one * (result && !ReducedMotion ? Mathf.Lerp(0.8f, 1f, Mathf.Clamp01((age - 0.36f) / 0.18f)) : 0.7f);
                ghost.SetTerminalFade(result ? Mathf.Clamp01((0.9f - age) / 0.18f) : 1 - gather * 0.7f);
            }
            if (age >= 0.54f)
            {
                ring.enabled = confirmationSpark == null;
                float pulse = Mathf.Clamp01((age - 0.54f) / 0.36f);
                ring.transform.localScale = Vector3.one * (ReducedMotion ? 0.62f : Mathf.Lerp(0.4f, 1.1f, pulse));
                Color tint = active.Altar ? new Color(0.45f, 0.95f, 0.88f, 1 - pulse) : new Color(1f, 0.78f, 0.4f, 1 - pulse);
                ring.startColor = ring.endColor = tint;
                if (confirmationSpark != null)
                {
                    confirmationSpark.enabled = true; confirmationSpark.sprite = confirmationFrames[Mathf.Min(3, Mathf.FloorToInt(pulse * 4))];
                    confirmationSpark.transform.localScale = Vector3.one * (1.1f / confirmationSpark.sprite.bounds.size.x);
                    confirmationSpark.color = new Color(1, 1, 1, 1 - pulse);
                }
                if (!sounded)
                {
                    CheckpointAudio.PlayInterfaceId("up.merge");
                    if (active.Result.Tier >= 2) CheckpointAudio.PlayInterfaceId(active.Result.Tier >= 3 ? "up.star3" : "up.star2", 0.07f);
                    sounded = true;
                }
            }
            if (age >= 0.9f) { CompletedCount++; ClearStage(); }
        }

        private void CreateConfirmation()
        {
            PaintedAnimationLibrary library = Resources.Load<PaintedAnimationLibrary>("PaintedAnimationLibrary");
            if (library != null)
            {
                var sparkle = new GameObject("PaintedMergePayoff"); sparkle.transform.SetParent(stage.transform, false);
                confirmationSpark = sparkle.AddComponent<SpriteRenderer>(); confirmationSpark.sortingOrder = 2300; confirmationSpark.enabled = false;
                confirmationFrames = library.Magic;
            }
            if (ringMaterial == null) ringMaterial = new Material(Shader.Find("Sprites/Default"));
            var outline = new GameObject("MergeConfirmationRing");
            outline.transform.SetParent(stage.transform, false);
            ring = outline.AddComponent<LineRenderer>();
            ring.sharedMaterial = ringMaterial; ring.useWorldSpace = false; ring.loop = true;
            ring.positionCount = 24; ring.widthMultiplier = 0.025f; ring.sortingOrder = 2300; ring.enabled = false;
            for (int index = 0; index < 24; index++)
            {
                float angle = index * Mathf.PI * 2 / 24;
                ring.SetPosition(index, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0));
            }
            var label = new GameObject("CommittedTierLabel");
            label.transform.SetParent(stage.transform, false); label.transform.localPosition = Vector3.down * 0.8f;
            TextMesh text = label.AddComponent<TextMesh>();
            text.font = ArcadeTypography.Body; text.fontSize = 64; text.characterSize = 0.035f; text.anchor = TextAnchor.MiddleCenter;
            text.text = new string('★', Mathf.Clamp(active.Result.Tier, 1, 3));
            text.GetComponent<MeshRenderer>().sharedMaterial = text.font.material; text.GetComponent<MeshRenderer>().sortingOrder = 2300;
            if (confirmation == null)
            {
                confirmation = gameObject.AddComponent<AudioSource>(); confirmation.playOnAwake = false; confirmation.spatialBlend = 0;
                const int samples = 8820;
                var waveform = new float[samples];
                for (int index = 0; index < samples; index++)
                {
                    float time = index / 44100f;
                    waveform[index] = Mathf.Sin(time * Mathf.PI * 2 * (time < 0.09f ? 660 : 880)) * Mathf.Sin(index / (float)samples * Mathf.PI) * 0.35f;
                }
                confirmationClip = AudioClip.Create("MergeConfirmationStudy", samples, 1, 44100, false); confirmationClip.SetData(waveform, 0);
            }
        }

        private UnitVisual CreateGhost(RosterEntry entry)
        {
            UnitDefinition definition = Instantiate(owner.Content.Unit(entry.UnitId));
            definitions.Add(definition);
            definition.Weapon = owner.Content.Weapon(entry.WeaponId);
            definition.CombatTier = entry.Tier;
            var child = new GameObject("PresentationGhost");
            child.transform.SetParent(stage.transform, false);
            UnitVisual visual = child.AddComponent<UnitVisual>();
            visual.CreateHierarchy(owner.Content.Arena.Hand);
            visual.ReducedMotion = true;
            visual.Bind(new UnitState { Id = ghosts.Count + 1, Definition = definition, Faction = Faction.Companion, Position = stage.transform.position, PreviousPosition = stage.transform.position }, owner.Content.Arena);
            return visual;
        }

        private void ClearStage()
        {
            if (stage != null) { stage.SetActive(false); Destroy(stage); }
            foreach (UnitDefinition definition in definitions) Destroy(definition);
            definitions.Clear(); ghosts.Clear(); stage = null; active = null; purchaseShadow = null; confirmationSpark = null;
        }
        private void OnDisable() { Skip(); }
        private void OnDestroy()
        {
            if (ringMaterial != null) Destroy(ringMaterial);
            if (confirmationClip != null) Destroy(confirmationClip);
        }
    }
}
