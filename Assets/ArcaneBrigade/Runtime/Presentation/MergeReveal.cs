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
        private Material payoffMaterial;
        private Mesh starMesh;
        private MaterialPropertyBlock starTint;
        private readonly List<Transform> stars = new List<Transform>();
        private readonly List<LineRenderer> rays = new List<LineRenderer>();
        private Transform compression;
        private TextMesh tierCaption;
        private const float RevealAt = 0.66f;
        private const float LandAt = 1.12f;
        private const float FinishAt = 1.9f;
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
            PresentStage();
            if (age >= FinishAt) { CompletedCount++; ClearStage(); }
        }

        private void PresentStage()
        {
            float gather = Mathf.SmoothStep(0, 1, Mathf.Clamp01(age / 0.46f));
            float fade = Mathf.Clamp01((FinishAt - age) / 0.24f);
            if (purchaseShadow != null)
            {
                purchaseShadow.enabled = !ReducedMotion && age < 0.46f;
                purchaseShadow.transform.localPosition = Vector3.up * (1 - gather) * 0.8f;
                purchaseShadow.color = new Color(0.25f, 0.2f, 0.35f, 1 - gather * 0.7f);
            }
            for (int index = 0; index < ghosts.Count; index++)
            {
                UnitVisual ghost = ghosts[index];
                bool result = index == ghosts.Count - 1;
                ghost.gameObject.SetActive(result ? age >= RevealAt || ReducedMotion : age < 0.46f && !ReducedMotion);
                if (!ghost.gameObject.activeSelf) continue;
                float angle = index * Mathf.PI * 2 / Mathf.Max(1, ghosts.Count - 1) + (active.Altar ? Mathf.PI * 0.5f : 0) + gather * 0.45f;
                Vector2 offset = result || ReducedMotion ? Vector2.zero : new Vector2(Mathf.Cos(angle), Mathf.Sin(angle) * 0.65f) * (1 - gather) * 1.65f;
                float emergence = Mathf.Clamp01((age - RevealAt) / (LandAt - RevealAt));
                if (result && !ReducedMotion) offset.y = Mathf.Sin(emergence * Mathf.PI) * 0.38f;
                ghost.Display(0, 1);
                ghost.transform.position = stage.transform.position + (Vector3)offset;
                float size = result ? (ReducedMotion ? 1.35f : Mathf.Lerp(0.48f, 1.35f, Mathf.SmoothStep(0, 1, emergence)) + Mathf.Sin(emergence * Mathf.PI) * 0.65f) : Mathf.Lerp(0.95f, 0.3f, gather);
                float landing = !ReducedMotion && result ? Mathf.Sin(Mathf.Clamp01((age - LandAt) / 0.18f) * Mathf.PI) * 0.1f : 0;
                ghost.transform.localScale = new Vector3(size * (1 + landing), size * (1 - landing), 1);
                ghost.SetTerminalFade(result ? fade : 1 - gather * 0.75f);
            }
            Color gold = active.Altar ? new Color(0.55f, 1f, 0.9f) : new Color(1f, 0.79f, 0.32f);
            compression.gameObject.SetActive(!ReducedMotion && age >= 0.28f && age < RevealAt);
            float charge = Mathf.Clamp01((age - 0.28f) / (RevealAt - 0.28f));
            compression.localScale = Vector3.one * Mathf.Lerp(0.48f, 0.12f, charge);
            compression.localRotation = Quaternion.Euler(0, 0, charge * 100);
            float burst = Mathf.Clamp01((age - RevealAt) / 0.48f);
            for (int index = 0; index < rays.Count; index++)
            {
                LineRenderer ray = rays[index];
                ray.enabled = !ReducedMotion && age >= RevealAt && burst < 1;
                float angle = index * Mathf.PI * 2 / rays.Count + 0.2f;
                Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
                ray.SetPosition(0, direction * Mathf.Lerp(0.35f, 1.5f, burst));
                ray.SetPosition(1, direction * Mathf.Lerp(0.75f, 2.2f, burst));
                ray.startColor = ray.endColor = new Color(gold.r, gold.g, gold.b, (1 - burst) * 0.65f);
            }
            for (int index = 0; index < stars.Count; index++)
            {
                Transform star = stars[index];
                float appearAt = RevealAt + index * 0.13f;
                star.gameObject.SetActive(ReducedMotion || age >= appearAt);
                float arrival = Mathf.SmoothStep(0, 1, Mathf.Clamp01((age - appearAt) / 0.28f));
                float pop = ReducedMotion ? 0 : Mathf.Sin(arrival * Mathf.PI);
                star.localPosition = new Vector3((index - (stars.Count - 1) * 0.5f) * 0.52f, 1.95f + (ReducedMotion ? 0 : (1 - arrival) * 0.55f), 0);
                star.localScale = Vector3.one * (ReducedMotion ? 0.24f : Mathf.Lerp(0.06f, 0.24f, arrival) + pop * 0.12f);
                star.localRotation = Quaternion.Euler(0, 0, ReducedMotion ? 0 : (1 - arrival) * -32);
                if (starTint == null) starTint = new MaterialPropertyBlock();
                starTint.SetColor("_Color", new Color(gold.r, gold.g, gold.b, fade));
                star.GetComponent<MeshRenderer>().SetPropertyBlock(starTint);
            }
            tierCaption.gameObject.SetActive(ReducedMotion || age >= LandAt);
            tierCaption.color = new Color(gold.r, gold.g, gold.b, fade);
            if (age >= RevealAt)
            {
                if (!ReducedMotion && ghosts.Count > 0)
                {
                    float flourish = AnimationTiming.Pulse(age - LandAt, .5f);
                    UnitDefinition result = owner.Content.Unit(active.Result.UnitId);
                    float direction = result.Material == BodyMaterial.Metal || result.Material == BodyMaterial.Wood ? -1 : 1;
                    UnitVisual revealed = ghosts[ghosts.Count - 1];
                    revealed.transform.localRotation = Quaternion.Euler(0, 0, direction * flourish * 5);
                }
                if (confirmationSpark != null)
                {
                    confirmationSpark.enabled = !ReducedMotion && burst < 1;
                    confirmationSpark.sprite = confirmationFrames[Mathf.Min(confirmationFrames.Length - 1, Mathf.FloorToInt(burst * confirmationFrames.Length))];
                    if (confirmationSpark.sprite != null) confirmationSpark.transform.localScale = Vector3.one * (2.4f / Mathf.Max(0.01f, confirmationSpark.sprite.bounds.size.x));
                    confirmationSpark.color = new Color(1, 1, 1, (1 - burst) * 0.65f);
                }
                if (!sounded)
                {
                    CheckpointAudio.PlayInterfaceId("up.merge");
                    if (active.Result.Tier >= 2) CheckpointAudio.PlayInterfaceId(active.Result.Tier >= 3 ? "up.star3" : "up.star2", 0.07f);
                    sounded = true;
                }
            }
        }

        private void CreateConfirmation()
        {
            PaintedAnimationLibrary library = Resources.Load<PaintedAnimationLibrary>("PaintedAnimationLibrary");
            if (library != null && library.Magic != null && library.Magic.Length > 0)
            {
                var sparkle = new GameObject("PaintedMergePayoff"); sparkle.transform.SetParent(stage.transform, false);
                confirmationSpark = sparkle.AddComponent<SpriteRenderer>(); confirmationSpark.sortingOrder = 2290; confirmationSpark.enabled = false;
                confirmationFrames = library.Magic;
            }
            if (payoffMaterial == null) payoffMaterial = new Material(Shader.Find("Sprites/Default"));
            if (starMesh == null)
            {
                var vertices = new Vector3[11];
                var triangles = new int[30];
                var colors = new Color[11];
                colors[0] = Color.white;
                for (int index = 0; index < 10; index++)
                {
                    float angle = Mathf.PI * 0.5f + index * Mathf.PI / 5;
                    vertices[index + 1] = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * (index % 2 == 0 ? 1 : 0.45f);
                    colors[index + 1] = Color.white;
                    triangles[index * 3] = 0; triangles[index * 3 + 1] = index + 1; triangles[index * 3 + 2] = (index + 1) % 10 + 1;
                }
                starMesh = new Mesh { name = "MergePresentationStar", vertices = vertices, triangles = triangles, colors = colors };
                starMesh.RecalculateBounds();
            }
            compression = CreateStar("CompressionGlint");
            compression.gameObject.SetActive(false);
            for (int index = 0; index < Mathf.Clamp(active.Result.Tier, 1, 3); index++)
            {
                Transform star = CreateStar("CommittedTierStar");
                star.gameObject.SetActive(false); stars.Add(star);
            }
            for (int index = 0; index < 8; index++)
            {
                var child = new GameObject("RevealRay"); child.transform.SetParent(stage.transform, false);
                LineRenderer ray = child.AddComponent<LineRenderer>();
                ray.sharedMaterial = payoffMaterial; ray.useWorldSpace = false; ray.positionCount = 2;
                ray.widthMultiplier = 0.045f; ray.sortingOrder = 2291; ray.enabled = false;
                rays.Add(ray);
            }
            var label = new GameObject("CommittedTierLabel");
            label.transform.SetParent(stage.transform, false); label.transform.localPosition = Vector3.down * 0.8f;
            TextMesh text = label.AddComponent<TextMesh>();
            text.font = ArcadeTypography.Body; text.fontSize = 64; text.characterSize = 0.035f; text.anchor = TextAnchor.MiddleCenter;
            text.text = active.Altar ? "FUSION COMPLETE" : "STAR UP";
            text.color = new Color(1, 0.79f, 0.32f, 0); tierCaption = text;
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

        private Transform CreateStar(string objectName)
        {
            var child = new GameObject(objectName);
            child.transform.SetParent(stage.transform, false);
            child.AddComponent<MeshFilter>().sharedMesh = starMesh;
            MeshRenderer renderer = child.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = payoffMaterial; renderer.sortingOrder = 2301;
            return child.transform;
        }

        private UnitVisual CreateGhost(RosterEntry entry)
        {
            UnitDefinition definition = Instantiate(owner.Content.Unit(entry.UnitId));
            definitions.Add(definition);
            definition.Weapon = owner.Content.Weapon(entry.WeaponId);
            definition.CombatTier = entry.Tier;
            var child = new GameObject("PresentationGhost");
            child.transform.SetParent(stage.transform, false);
            child.AddComponent<UnityEngine.Rendering.SortingGroup>().sortingOrder = 2295;
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
            stars.Clear(); rays.Clear(); compression = null; tierCaption = null; confirmationFrames = null;
        }
        private void OnDisable() { Skip(); }
        private void OnDestroy()
        {
            if (payoffMaterial != null) Destroy(payoffMaterial);
            if (starMesh != null) Destroy(starMesh);
            if (confirmationClip != null) Destroy(confirmationClip);
        }
    }
}
