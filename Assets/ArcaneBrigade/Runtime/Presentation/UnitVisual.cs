using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class UnitVisual : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer body;
        [SerializeField] private SpriteRenderer weapon;
        [SerializeField] private SpriteRenderer hand;
        [SerializeField] private SpriteRenderer shadow;
        [SerializeField] private Transform grip;
        [SerializeField] private Transform muzzle;
        private UnitState state;
        private MaterialPropertyBlock flashProperties;
        private LineRenderer bowString;
        private Material bowMaterial;
        private SpriteRenderer drawHand;
        private Color shadowColor;
        private readonly SpriteRenderer[] stampPips = new SpriteRenderer[3];
        private SpriteRenderer tierBell;
        private readonly LineRenderer[] exposedCorners = new LineRenderer[4];
        private Material exposedMaterial;
        private Vector2 arrivalOrigin;
        private float arrivalAge = 10;
        private int arrivalFrame;
        private float facingAngle;
        private BodyPuppet puppet;
        private GroundShadow grounded;
        private GroundShadow groundedWeapon;
        private Material entranceMaterial;
        private float lastDisplayTime;
        private readonly SpriteRenderer[] tierStars = new SpriteRenderer[3];
        private SpriteRenderer tierCrest;
        private float holdUntil;
        private float holdPose;
        private float nextHold;
        private float celebrateAt = -100;
        private Material paintedMaterial;
        private SpriteRenderer championBeacon;
        private float gaitBlend;
        public float GaitWeight => gaitBlend;
        private float previousGait;
        private float transitionSquash;
        private EvolutionAdornment adornment;
        private BossAnatomyVisual anatomyVisual;
        public static float BlendGait(float current, float target, float delta)
        {
            return Mathf.Lerp(current, target, 1 - Mathf.Exp(-Mathf.Clamp(delta, 0, 0.05f) * (target > current ? 13 : 9)));
        }
        public void HoldImpact(float time, float duration)
        {
            if (ReducedMotion || state == null || !state.Alive || time < nextHold) return;
            holdPose = time; holdUntil = time + Mathf.Clamp(duration, 0, 0.045f); nextHold = time + 0.12f;
        }
        public void Celebrate(float time) { celebrateAt = time + (state.Id % 4) * 0.035f; }
        public void BeginArrival(Vector2 origin)
        {
            arrivalOrigin = origin; arrivalAge = 0; arrivalFrame = Time.frameCount;
            CheckpointAudio.PlayWorldId("up.ally.land", origin);
        }
        public bool ReducedMotion { get; set; }
        public UnitState State => state;
        public Sprite DisplayedBody => body == null ? null : body.sprite;
        public SpriteRenderer BodyRenderer => body;
        public SpriteRenderer WeaponRenderer => weapon;
        public static Vector3 FitCompanionWeapon(Vector3 scale, Rect ink, float bodyHeight, bool melee)
        {
            float longest = Mathf.Max(ink.width * scale.x, ink.height * scale.y);
            float minimum = bodyHeight * (melee ? 1.05f : 0.65f);
            float maximum = bodyHeight * (melee ? 1.75f : 0.95f);
            return scale * (Mathf.Clamp(longest, minimum, maximum) / Mathf.Max(0.01f, longest));
        }
        public bool WeakspotVisible => exposedCorners[0] != null && exposedCorners[0].enabled;
        public static bool IsWeakspotVisible(UnitState unit, float time) => unit != null && unit.Alive && unit.Faction == Faction.Enemy && unit.ExposedUntil > time && unit.TelegraphUntil <= time;

        public static Vector2 TierWeaponShape(WeaponDefinition definition, int tier)
        {
            float step = Mathf.Clamp(tier - 1, 0, 2);
            if (definition.Signature == SignatureWeapon.Stamp) return new Vector2(1 + step * 0.12f, 1 + step * 0.28f);
            if (definition.Signature == SignatureWeapon.Umbrella) return new Vector2(1 + step * 0.24f, 1 + step * 0.08f);
            if (definition.Motion == WeaponMotion.Bow) return new Vector2(1 + step * 0.08f, 1 + step * 0.24f);
            if (definition.Family == ExpandedWeapon.Scissors) return new Vector2(1 + step * 0.22f, 1 + step * 0.08f);
            if (definition.Family == ExpandedWeapon.Camera || definition.Family == ExpandedWeapon.MusicBox) return new Vector2(1 + step * 0.08f, 1 + step * 0.2f);
            if (definition.Family == ExpandedWeapon.Iron || definition.Family == ExpandedWeapon.Hammer) return new Vector2(1 + step * 0.2f, 1 + step * 0.18f);
            return new Vector2(1 + step * 0.16f, 1 + step * 0.12f);
        }

        public static float IdleBeat(UnitState unit, float time, bool attacking, bool reducedMotion)
        {
            if (reducedMotion || unit.Faction != Faction.Companion || !unit.Alive || attacking || unit.PendingAttack || time - unit.LastHit < 0.4f || unit.TelegraphUntil > time) return 0;
            float phase = Mathf.Repeat(time + unit.Id * 0.73f, 6.4f);
            return phase < 0.65f ? Mathf.Sin(phase / 0.65f * Mathf.PI) : 0;
        }

        public static int AnimationFrame(float time, int identity, bool moving, bool reducedMotion)
        {
            if (reducedMotion) return 0;
            if (moving) return 2 + (Mathf.FloorToInt(time * 7 + identity * 0.37f) & 1);
            return Mathf.Repeat(time + identity * 0.73f, 4.7f) < 0.13f ? 1 : 0;
        }

        public void CopyDeathPose(SpriteRenderer targetBody, SpriteRenderer targetShadow, SpriteRenderer targetWeapon, SpriteRenderer targetHand, SpriteRenderer targetDrawHand, Transform targetGrip)
        {
            if (anatomyVisual != null) anatomyVisual.CopyDeathParts(targetBody.transform);
            CopyPart(body, targetBody);
            targetBody.enabled = body.sprite != null && state.Faction != Faction.Companion;
            CopyPart(shadow, targetShadow);
            targetGrip.localPosition = grip.localPosition;
            targetGrip.localRotation = grip.localRotation;
            CopyPart(weapon, targetWeapon);
            CopyPart(hand, targetHand);
            CopyPart(drawHand, targetDrawHand);
            targetWeapon.enabled = weapon.enabled && grip.gameObject.activeSelf && state.Faction != Faction.Companion;
            targetHand.enabled = hand.enabled && targetWeapon.enabled;
            targetDrawHand.enabled = drawHand != null && drawHand.enabled && drawHand.gameObject.activeSelf && targetWeapon.enabled;
            K8MotionDeath freeze = targetBody.GetComponent<K8MotionDeath>();
            if (freeze == null) freeze = targetBody.gameObject.AddComponent<K8MotionDeath>();
            freeze.Capture(targetShadow.transform, targetGrip, state.Position);
        }

        private static void CopyPart(SpriteRenderer source, SpriteRenderer destination)
        {
            destination.enabled = source != null && source.enabled && source.gameObject.activeSelf;
            if (source == null) return;
            destination.sprite = source.sprite;
            destination.sharedMaterial = source.sharedMaterial;
            destination.color = source.color;
            destination.flipX = source.flipX;
            destination.sortingOrder = source.sortingOrder;
            destination.transform.localPosition = source.transform.localPosition;
            destination.transform.localRotation = source.transform.localRotation;
            destination.transform.localScale = source.transform.localScale;
            destination.SetPropertyBlock(null);
        }

        public void CreateHierarchy(Sprite shadowSprite)
        {
            body = CreateSprite("BodyVisual", transform);
            grip = new GameObject("GripRoot").transform;
            grip.SetParent(transform, false);
            weapon = CreateSprite("WeaponVisual", grip);
            hand = CreateSprite("HandVisual", grip);
            muzzle = new GameObject("MuzzleMarker").transform;
            muzzle.SetParent(grip, false);
            shadow = CreateSprite("ShadowVisual", transform);
            shadow.sprite = shadowSprite;
            shadow.color = new Color(0f, 0f, 0f, 0.25f);
            shadow.transform.localScale = new Vector3(0.6f, 0.17f, 1f);
        }

        private static SpriteRenderer CreateSprite(string name, Transform parent)
        {
            var child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child.AddComponent<SpriteRenderer>();
        }

        public void Bind(UnitState unit, CheckpointCatalog catalog)
        {
            if (anatomyVisual != null) anatomyVisual.Clear();
            if (adornment != null) { adornment.Clear(); if (Application.isPlaying) Destroy(adornment); else DestroyImmediate(adornment); adornment = null; }
            state = unit;
            holdUntil = nextHold = 0; celebrateAt = -100;
            lastDisplayTime = 0;
            gaitBlend = previousGait = transitionSquash = 0;
            if (puppet == null) puppet = body.gameObject.AddComponent<BodyPuppet>();
            arrivalAge = 10;
            facingAngle = Mathf.Atan2(unit.Facing.y, unit.Facing.x) * Mathf.Rad2Deg;
            if (flashProperties == null) flashProperties = new MaterialPropertyBlock();
            body.sprite = unit.Definition.Body;
            Material flash = Resources.Load<Material>("UnitFlash");
            paintedMaterial = flash;
            if (flash != null) weapon.sharedMaterial = hand.sharedMaterial = flash;
            if (entranceMaterial != null) { Destroy(entranceMaterial); entranceMaterial = null; }
            if (unit.Definition.IsBoss && unit.Definition.BossPattern % 2 != 0)
            {
                entranceMaterial = new Material(Resources.Load<Shader>("MarketEmergence"));
                body.sharedMaterial = entranceMaterial;
            }
            if (flash != null && entranceMaterial == null) body.sharedMaterial = flash;
            shadow.color = new Color(0.015f, 0.025f, 0.045f, 0.38f);
            shadowColor = shadow.color;
            hand.sprite = catalog.Hand;
            weapon.sprite = unit.Definition.Weapon.Sprite;
            if (unit.Definition.IsBoss)
            {
                if (anatomyVisual == null) anatomyVisual = gameObject.AddComponent<BossAnatomyVisual>();
                anatomyVisual.Build(unit, body);
            }
            if (adornment == null && unit.Faction == Faction.Companion && unit.Definition.CombatTier > 1)
            {
                adornment = gameObject.AddComponent<EvolutionAdornment>(); adornment.Build(unit, body, weapon);
            }
            weapon.color = hand.color = Color.white;
            if (unit.Definition.Weapon.Mechanic == CarnivalWeapon.BloomOrb) weapon.color = new Color(0.65f, 1, 0.85f);
            else if (unit.Definition.Weapon.Mechanic == CarnivalWeapon.EchoBlade) weapon.color = new Color(0.65f, 0.8f, 1);
            else if (unit.Definition.Weapon.Mechanic == CarnivalWeapon.SatelliteDisc) weapon.color = new Color(1, 0.85f, 0.55f);
            if (drawHand != null) drawHand.color = Color.white;
            bool armed = unit.Faction != Faction.Enemy && unit.Definition.Weapon.Kind != WeaponKind.Contact && weapon.sprite != null;
            grip.gameObject.SetActive(armed);
            if (armed)
            {
                float weaponScale = K8Motion.WeaponWidth(unit) / Mathf.Max(0.01f, weapon.sprite.bounds.size.x);
                Vector2 shape = TierWeaponShape(unit.Definition.Weapon, unit.Definition.CombatTier);
                if (unit.Faction == Faction.Companion) weaponScale /= Mathf.Max(1, Mathf.Max(shape.x, shape.y) / 1.18f);
                weapon.transform.localScale = new Vector3(weaponScale * shape.x, weaponScale * shape.y, 1);
                if (unit.Faction == Faction.Companion)
                {
                    Rect ink = SpriteInkMetrics.BoundsOf(weapon.sprite);
                    weapon.transform.localScale = FitCompanionWeapon(weapon.transform.localScale, ink, K8Motion.BodyHeight(unit), unit.Definition.Weapon.Kind == WeaponKind.Melee);
                }
                if (hand.sprite != null) hand.transform.localScale = Vector3.one * ((unit.Faction == Faction.Companion ? 0.095f : 0.13f) / Mathf.Max(0.01f, hand.sprite.bounds.size.x));
                muzzle.localPosition = unit.Definition.Weapon.HasMuzzleOffset ? (Vector3)unit.Definition.Weapon.ProjectileMuzzleOffset : Vector3.right * unit.Definition.Weapon.MuzzleDistance;
            }
            if (tierBell == null) tierBell = CreateSprite("TierBellSatellite", grip);
            tierBell.sprite = weapon.sprite;
            tierBell.enabled = armed && unit.Definition.CombatTier >= 2 && unit.Definition.Weapon.Signature == SignatureWeapon.Bell;
            tierBell.transform.localPosition = unit.Faction == Faction.Companion ? new Vector3(0.07f, -0.16f, 0) : new Vector3(0.1f, -0.26f, 0);
            tierBell.transform.localScale = weapon.transform.localScale * (unit.Definition.CombatTier >= 3 ? 0.48f : 0.36f);
            name = unit.Definition.DisplayName + "_" + unit.Id;
            if (unit.Definition.Weapon.Motion == WeaponMotion.Bow && bowString == null)
            {
                var child = new GameObject("DrawString");
                child.transform.SetParent(grip, false);
                bowString = child.AddComponent<LineRenderer>();
                bowString.useWorldSpace = false;
                bowString.positionCount = 3;
                bowString.widthMultiplier = 0.018f;
                bowMaterial = new Material(Shader.Find("Sprites/Default"));
                bowString.sharedMaterial = bowMaterial;
                bowString.startColor = bowString.endColor = new Color(1f, 0.92f, 0.68f);
                drawHand = CreateSprite("DrawHandVisual", grip);
                drawHand.sprite = catalog.Hand;
                drawHand.transform.localScale = hand.transform.localScale;
            }
            if (bowString != null) bowString.enabled = unit.Definition.Weapon.Motion == WeaponMotion.Bow;
            if (drawHand != null) drawHand.gameObject.SetActive(unit.Definition.Weapon.Motion == WeaponMotion.Bow);
            Display(0f, 1f);
        }

        public void SetChampionCosmetic(int choice)
        {
            if (state != null && state.Faction == Faction.Champion && choice == 1)
                shadowColor = shadow.color = new Color(0.015f, 0.025f, 0.045f, 0.38f);
        }
        public void TransportShadow(Vector2 offset, float lift)
        {
            if (state == null || grounded == null) return;
            float height = K8Motion.BodyHeight(state);
            Vector2 ground = state.Position + offset - Vector2.up * (height * 0.5f + lift);
            grounded.Display(body.sprite, ground, height * 0.85f, lift, 0.3f);
            if (groundedWeapon != null && weapon.enabled && grip.gameObject.activeSelf)
                groundedWeapon.Display(weapon.sprite, new Vector2(weapon.transform.position.x, ground.y), K8Motion.WeaponWidth(state), lift, 0.1f);
        }

        public void Display(float simulationTime, float interpolation)
        {
            if (state == null) return;
            float motionDelta = Mathf.Clamp(simulationTime - lastDisplayTime, 0, 0.05f);
            lastDisplayTime = simulationTime;
            if (!ReducedMotion && simulationTime < holdUntil) simulationTime = holdPose;
            if (state.Definition.AuthoredMotion != null)
                body.sprite = state.Definition.AuthoredMotion.Select(state, simulationTime, ReducedMotion);
            else if (state.Definition.AnimationFrames.Length >= 4)
                body.sprite = state.Definition.AnimationFrames[K8Motion.BodyFrame(state, simulationTime, ReducedMotion)];
            weapon.color = hand.color = Color.white;
            shadow.color = state.Faction == Faction.Enemy ? new Color(0.02f, 0.02f, 0.04f, 0.3f) : shadowColor;
            if (tierBell != null) tierBell.color = Color.white;
            if (drawHand != null) drawHand.color = Color.white;
            if (bowString != null) bowString.startColor = bowString.endColor = new Color(1, 0.92f, 0.68f);
            transform.position = Vector2.Lerp(state.PreviousPosition, state.Position, interpolation);
            float entrance = state.Definition.IsBoss ? Mathf.Clamp01((state.EntranceUntil - simulationTime) / 1.2f) : 0;
            if (!ReducedMotion && entrance > 0)
                transform.position += Vector3.up * (state.Definition.BossPattern % 2 == 0 ? 6 * entrance * entrance : -state.Definition.BodyHeight * entrance);
            if (entranceMaterial != null) entranceMaterial.SetFloat("_FloorY", !ReducedMotion && entrance > 0 ? state.Position.y - state.Definition.BodyHeight * 0.5f : -10000);
            if (entrance <= 0 && paintedMaterial != null) body.sharedMaterial = paintedMaterial;
            if (arrivalAge < 0.9f && arrivalFrame != Time.frameCount)
            {
                arrivalAge += motionDelta; arrivalFrame = Time.frameCount;
            }
            float arrival = arrivalAge / 0.9f;
            if (arrival >= 0 && arrival < 1)
            {
                Vector2 landing = arrivalOrigin + new Vector2(0.5f, -0.6f);
                Vector2 destination = arrival < 0.45f ? Vector2.Lerp(arrivalOrigin, landing, arrival / 0.45f) : Vector2.Lerp(landing, (Vector2)transform.position, Mathf.SmoothStep(0, 1, (arrival - 0.45f) / 0.55f));
                if (!ReducedMotion && arrival < 0.45f) destination.y += Mathf.Sin(arrival / 0.45f * Mathf.PI) * 0.85f;
                transform.position = destination;
            }
            WeaponDefinition definition = state.Definition.Weapon;
            float age = simulationTime - state.AttackStarted;
            float poseWindup = state.AttackWindup > 0 ? state.AttackWindup : definition.Windup;
            bool attacking = K8Motion.Attacking(state, simulationTime);
            K8Motion.Pose pose = K8Motion.Attack(definition, age, poseWindup, ReducedMotion);
            float idle = IdleBeat(state, simulationTime, attacking, ReducedMotion);
            Vector2 facing = attacking ? state.AttackDirection : state.Facing;
            if (facing.sqrMagnitude < 0.01f) facing = Vector2.right;
            int order = 1000 - Mathf.RoundToInt(state.Position.y * 12f);
            body.sortingOrder = order;
            shadow.sortingOrder = order - 10;
            bool behind = facing.y > 0.45f;
            weapon.sortingOrder = order + (behind ? -2 : 2);
            hand.sortingOrder = order + (behind ? -1 : 3);
            if (tierBell != null) tierBell.sortingOrder = weapon.sortingOrder;
            body.flipX = facing.x < -0.1f;
            float desiredGait = Mathf.Clamp01((state.Position - state.PreviousPosition).magnitude * 20f);
            float blendDelta = Mathf.Min(0.05f, motionDelta);
            gaitBlend = BlendGait(gaitBlend, desiredGait, blendDelta);
            transitionSquash = Mathf.Lerp(transitionSquash, (gaitBlend - previousGait) * 0.65f, 1 - Mathf.Exp(-blendDelta * 18));
            previousGait = gaitBlend;
            float movement = ReducedMotion ? desiredGait : gaitBlend;
            bool pursuit = state.TravelMode == CompanionTravelMode.Pursuit;
            var profile = new SpeciesMotionProfile(state.Definition);
            float bob = ReducedMotion ? 0 : Mathf.Sin(simulationTime * profile.GaitCadence * (pursuit ? 1.45f : 1) + state.Id) * profile.GaitAmplitude * (pursuit ? 1.3f : 1) * movement;
            float attackLean = attacking ? pose.Lean : 0;
            if (pursuit && !attacking && !ReducedMotion) attackLean += Mathf.Clamp(state.Velocity.x, -1, 1) * (facing.x < 0 ? -1 : 1) * -7;
            float impactSquash = attacking ? pose.Squash : 0;
            if (!ReducedMotion) impactSquash += transitionSquash;
            float landingAge = arrivalAge - 0.3825f;
            if (!ReducedMotion && landingAge > 0 && landingAge < 0.18f) impactSquash += Mathf.Sin(landingAge / 0.18f * Mathf.PI) * 0.1f;
            if (state.Faction == Faction.Enemy && attacking && !ReducedMotion)
            {
                float charge = Mathf.Clamp01(age / Mathf.Max(0.01f, poseWindup));
                float release = Mathf.Clamp01((age - poseWindup) / Mathf.Max(0.01f, definition.Recovery));
                impactSquash += age < poseWindup ? charge * 0.14f : -Mathf.Sin(release * Mathf.PI) * 0.18f;
                attackLean += age < poseWindup ? -charge * 8 : Mathf.Sin(release * Mathf.PI) * 12;
            }
            float height = K8Motion.BodyHeight(state);
            float referenceHeight = state.Definition.AuthoredMotion != null ? state.Definition.AuthoredMotion.ReferenceHeight : body.sprite.bounds.size.y;
            float scale = height / Mathf.Max(0.01f, referenceHeight);
            float hitAge = simulationTime - state.LastHit;
            float hurt = ReducedMotion ? 0 : Mathf.Clamp01(1f - hitAge / 0.14f);
            impactSquash += hurt * 0.09f;
            body.transform.localScale = new Vector3(scale * (1f + bob * 0.8f + impactSquash), scale * (1f - bob * 0.8f - impactSquash), 1f);
            float pivotHeight = body.sprite == null ? 0 : body.sprite.pivot.y / body.sprite.rect.height;
            body.transform.localPosition = new Vector3(state.HitDirection.x * hurt * 0.09f, height * (pivotHeight - 0.5f) + Mathf.Abs(bob) + state.HitDirection.y * hurt * 0.09f, 0f);
            float celebrationAge = simulationTime - celebrateAt;
            float celebrationLift = ReducedMotion || celebrationAge < 0 || celebrationAge > 0.5f ? 0 : Mathf.Sin(celebrationAge / 0.5f * Mathf.PI) * 0.12f;
            body.transform.localPosition += Vector3.up * (celebrationLift + (ReducedMotion ? 0 : state.BodyLift));
            float personality = state.Id % 2 == 0 ? -1 : 1;
            body.transform.localRotation = Quaternion.Euler(0f, 0f, bob * 90f + state.HitDirection.x * hurt * 6 + (attackLean + idle * personality * 4f) * (facing.x < 0f ? -1f : 1f));
            shadow.transform.localPosition = new Vector3(0f, -height * 0.5f, 0f);
            shadow.transform.localScale = new Vector3(height * 0.65f, height * 0.16f, 1);
            if (grounded == null) { grounded = gameObject.AddComponent<GroundShadow>(); grounded.Configure(body.sprite); }
            float lift = entrance > 0 ? entrance * 6 : arrivalAge < 0.405f ? Mathf.Sin(Mathf.Clamp01(arrivalAge / 0.405f) * Mathf.PI) * 0.85f : 0;
            Vector2 ground = new Vector2(transform.position.x, transform.position.y - height * 0.5f - (entrance > 0 ? transform.position.y - state.Position.y : lift));
            grounded.Display(body.sprite, ground, height * 0.85f, lift + celebrationLift + Mathf.Abs(bob) + Mathf.Max(0, state.BodyLift), state.Definition.IsBoss ? 0.42f : 0.36f);
            if (groundedWeapon == null) { groundedWeapon = grip.gameObject.AddComponent<GroundShadow>(); groundedWeapon.Configure(weapon.sprite); }
            if (weapon.enabled && grip.gameObject.activeSelf) groundedWeapon.Display(weapon.sprite, new Vector2(weapon.transform.position.x, ground.y - 0.02f), K8Motion.WeaponWidth(state), 0.3f, 0.12f);
            else groundedWeapon.Hide();
            shadow.enabled = false;
            body.color = ReducedMotion || PresentationSettings.ReducedFlashes ? state.Definition.Tint : Color.Lerp(Color.white, state.Definition.Tint, Mathf.Clamp01((simulationTime - state.LastHit) / 0.12f));
            if (state.ChillUntil > simulationTime) body.color = Color.Lerp(body.color, new Color(0.55f, 0.85f, 1f), 0.35f);
            else if (state.BurnUntil > simulationTime) body.color = Color.Lerp(body.color, new Color(1f, 0.6f, 0.3f), 0.2f + Mathf.PingPong(simulationTime * 2f, 0.15f));
            body.GetPropertyBlock(flashProperties);
            // Capped, flickering flash: a target under constant fire never turns into a solid white silhouette.
            float flash = Mathf.Clamp01(1f - hitAge / 0.07f) * (state.Definition.IsBoss ? 0.35f : 0.55f) * (Mathf.Repeat(simulationTime * 14f, 1f) < 0.5f ? 1f : 0.3f);
            flashProperties.SetFloat("_Flash", ReducedMotion || PresentationSettings.ReducedFlashes ? 0 : flash);
            SpriteLighting.Apply(body, flashProperties, state.Faction == Faction.Enemy);
            if (state.Faction == Faction.Champion)
            {
                flashProperties.SetColor("_RimColor", new Color(0.45f, 1, 0.85f));
                flashProperties.SetFloat("_RimAmount", 0.4f);
            }
            body.SetPropertyBlock(flashProperties);
            if (state.TelegraphUntil > simulationTime) body.color = Color.Lerp(state.Definition.Tint, new Color(1f, 0.4f, 0.3f), ReducedMotion || PresentationSettings.ReducedFlashes ? 0.65f : 0.5f + Mathf.Sin(simulationTime * 12f) * 0.25f);
            if (puppet != null) puppet.Display(body, state, simulationTime, ReducedMotion);
            flashProperties.Clear(); SpriteLighting.Apply(weapon, flashProperties, false, true); weapon.SetPropertyBlock(flashProperties);
            DisplayWeakspot(simulationTime, order);
            DisplayTier(order, height);
            if (adornment != null) adornment.Display(state, simulationTime, order, ReducedMotion);
            if (anatomyVisual != null) anatomyVisual.Display(state, simulationTime, order, ReducedMotion);
            if (state.Faction == Faction.Champion && championBeacon == null)
            {
                championBeacon = CreateSprite("ChampionBeacon", transform); championBeacon.sprite = K8Motion.TierStar();
            }
            if (championBeacon != null)
            {
                championBeacon.enabled = state.Alive && state.Faction == Faction.Champion;
                championBeacon.sortingOrder = order - 1;
                championBeacon.transform.localPosition = new Vector3(0, -height * 0.5f - 0.06f, 0);
                championBeacon.transform.localScale = new Vector3(0.8f, 0.26f, 1) * (height / Mathf.Max(0.01f, championBeacon.sprite.bounds.size.x));
                championBeacon.color = new Color(0.5f, 1, 0.86f, 0.95f);
            }

            float angleOffset = !ReducedMotion && definition.Kind == WeaponKind.Melee ? Mathf.Sin(simulationTime * 1.8f + state.Id * 0.4f) * 9f : 0f;
            float recoil = 0f;
            if (idle > 0)
            {
                angleOffset = idle * (definition.Motion == WeaponMotion.Bell ? Mathf.Sin(simulationTime * 30f) * 9f : personality * 7f);
                recoil = definition.Motion == WeaponMotion.Stamp ? idle * 0.04f : 0;
                if (definition.Motion == WeaponMotion.Bow || definition.Motion == WeaponMotion.Camera) angleOffset = -idle * 5;
                if (definition.Motion == WeaponMotion.Hammer || definition.Motion == WeaponMotion.Slam) angleOffset = idle * personality * 3;
                if (definition.Motion == WeaponMotion.Cast || definition.Motion == WeaponMotion.Music) angleOffset = idle * personality * 10;
            }
            for (int index = 0; index < stampPips.Length; index++)
            {
                bool visible = hand.sprite != null && state.Alive && state.MarkVisibleUntil > simulationTime && state.VisibleMarks > index;
                if (visible && stampPips[index] == null)
                {
                    var pip = new GameObject("StampSeal" + index); pip.transform.SetParent(transform, false);
                    stampPips[index] = pip.AddComponent<SpriteRenderer>(); stampPips[index].sprite = hand.sprite;
                    stampPips[index].color = new Color(1, 0.27f, 0.38f);
                    pip.transform.localScale = Vector3.one * (0.1f / Mathf.Max(0.01f, hand.sprite.bounds.size.x));
                    pip.transform.localRotation = Quaternion.Euler(0, 0, 45);
                }
                if (stampPips[index] == null) continue;
                stampPips[index].enabled = visible; stampPips[index].sortingOrder = body.sortingOrder + 8;
                stampPips[index].transform.localPosition = new Vector3((index - 1) * 0.16f, height * 0.65f, 0);
            }
            if (attacking) { angleOffset = pose.Angle; recoil = pose.Recoil; }
            if (bowString != null && definition.Motion == WeaponMotion.Bow)
            {
                float pull = !ReducedMotion && attacking ? age < poseWindup ? Mathf.Clamp01(age / Mathf.Max(0.001f, poseWindup)) : 1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01((age - poseWindup) / Mathf.Max(0.001f, definition.Recovery * 0.18f))) : 0f;
                float bowHeight = weapon.sprite.bounds.size.y * weapon.transform.localScale.y * 0.4f;
                float anchor = -weapon.sprite.bounds.size.x * weapon.transform.localScale.x * 0.35f;
                float draw = pull * (state.Faction == Faction.Companion ? 0.12f : 0.17f);
                bowString.sortingOrder = weapon.sortingOrder + 1;
                bowString.SetPosition(0, new Vector3(anchor, bowHeight, 0));
                bowString.SetPosition(1, new Vector3(anchor - draw, 0, 0));
                bowString.SetPosition(2, new Vector3(anchor, -bowHeight, 0));
                drawHand.sortingOrder = hand.sortingOrder + 1;
                drawHand.transform.localPosition = new Vector3(anchor - draw, 0, 0);
            }
            float attachmentScale = state.Faction == Faction.Companion ? 0.72f : 1;
            grip.localPosition = facing.normalized * ((definition.MountDistance - recoil) * attachmentScale) + Vector2.up * (pose.Lift * attachmentScale);
            float targetAngle = Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg;
            facingAngle = ReducedMotion ? targetAngle : attacking ? Mathf.LerpAngle(facingAngle, targetAngle, 1 - Mathf.Exp(-motionDelta * 35)) : Mathf.MoveTowardsAngle(facingAngle, targetAngle, motionDelta * 240f);
            grip.localRotation = Quaternion.Euler(0f, 0f, facingAngle + angleOffset);
        }

        private void DisplayTier(int order, float height)
        {
            int tier = Mathf.Clamp(state.Definition.CombatTier, 1, 3);
            bool showCrest = state.Alive && state.Faction == Faction.Companion && tier > 1 && (adornment == null || adornment.PieceCount == 0);
            if (showCrest && tierCrest == null) { tierCrest = CreateSprite("TierChestCrest", transform); tierCrest.sprite = K8Motion.TierStar(); }
            if (tierCrest != null)
            {
                tierCrest.enabled = showCrest; tierCrest.sortingOrder = order + 4;
                tierCrest.color = tier == 3 ? new Color(1, 0.77f, 0.25f) : new Color(0.7f, 0.93f, 1);
                tierCrest.transform.localScale = Vector3.one * height * (tier == 3 ? 0.16f : 0.12f);
                tierCrest.transform.localPosition = body.transform.localPosition + Vector3.down * height * 0.17f;
                tierCrest.transform.localRotation = Quaternion.Euler(0, 0, tier == 2 ? 45 : 0);
            }
            float top = body.transform.localPosition.y + body.sprite.bounds.max.y * body.transform.localScale.y;
            for (int index = 0; index < tierStars.Length; index++)
            {
                bool visible = state.Alive && state.Faction == Faction.Companion && tier > 1 && index < tier;
                if (visible && tierStars[index] == null)
                {
                    tierStars[index] = CreateSprite("K8MotionTierStar" + index, transform);
                    tierStars[index].sprite = K8Motion.TierStar();
                    tierStars[index].transform.localScale = Vector3.one * 0.085f;
                }
                if (tierStars[index] == null) continue;
                tierStars[index].enabled = visible;
                tierStars[index].sortingOrder = order + 8;
                tierStars[index].color = tier == 3 ? new Color(1, 0.77f, 0.25f) : new Color(0.65f, 0.94f, 1);
                tierStars[index].transform.localPosition = new Vector3((index - (tier - 1) * 0.5f) * 0.095f, top + 0.065f, 0);
            }
        }

        private void DisplayWeakspot(float time, int order)
        {
            bool visible = IsWeakspotVisible(state, time);
            if (!visible && exposedCorners[0] == null) return;
            if (exposedMaterial == null) exposedMaterial = new Material(Shader.Find("Sprites/Default"));
            Vector3 center = body.transform.localPosition + Vector3.Scale(body.sprite.bounds.center, body.transform.localScale);
            Vector3 extent = Vector3.Scale(body.sprite.bounds.extents, body.transform.localScale) + new Vector3(0.045f, 0.045f, 0);
            float length = Mathf.Min(extent.x, extent.y) * 0.38f;
            for (int index = 0; index < exposedCorners.Length; index++)
            {
                if (exposedCorners[index] == null)
                {
                    var corner = new GameObject("ExposedMintCorner" + index);
                    corner.transform.SetParent(transform, false);
                    LineRenderer frame = corner.AddComponent<LineRenderer>();
                    frame.sharedMaterial = exposedMaterial; frame.useWorldSpace = false; frame.positionCount = 3;
                    frame.startColor = frame.endColor = new Color(0.55f, 1f, 0.82f, 0.65f);
                    exposedCorners[index] = frame;
                }
                LineRenderer renderer = exposedCorners[index];
                renderer.enabled = visible;
                if (!visible) continue;
                float horizontal = index < 2 ? -1 : 1;
                float vertical = index % 2 == 0 ? -1 : 1;
                Vector3 tip = center + new Vector3(horizontal * extent.x, vertical * extent.y, 0);
                renderer.SetPosition(0, tip - Vector3.right * horizontal * length);
                renderer.SetPosition(1, tip);
                renderer.SetPosition(2, tip - Vector3.up * vertical * length);
                renderer.widthMultiplier = Mathf.Clamp(state.Definition.BodyHeight * 0.02f, 0.012f, 0.026f);
                renderer.sortingOrder = order + 7;
                renderer.startColor = renderer.endColor = new Color(0.55f, 1f, 0.82f, 0.65f);
            }
        }

        private void OnDestroy()
        {
            if (entranceMaterial != null) Destroy(entranceMaterial);
            if (bowMaterial != null) Destroy(bowMaterial);
            if (exposedMaterial != null) Destroy(exposedMaterial);
        }

        public void SetTerminalFade(float alpha)
        {
            if (anatomyVisual != null) anatomyVisual.Fade(alpha);
            if (adornment != null) adornment.Fade(alpha);
            Fade(body, alpha); Fade(weapon, alpha); Fade(hand, alpha); Fade(shadow, alpha);
            if (grounded != null) grounded.Fade(alpha);
            if (groundedWeapon != null) groundedWeapon.Fade(alpha);
            if (puppet != null) puppet.Fade(alpha);
            if (tierBell != null) Fade(tierBell, alpha);
            if (tierCrest != null) Fade(tierCrest, alpha);
            foreach (SpriteRenderer star in tierStars) if (star != null) Fade(star, alpha);
            foreach (SpriteRenderer pip in stampPips) if (pip != null) Fade(pip, alpha);
            foreach (LineRenderer corner in exposedCorners)
            {
                if (corner == null) continue;
                Color tint = corner.startColor; tint.a *= alpha; corner.startColor = corner.endColor = tint;
            }
            if (drawHand != null) Fade(drawHand, alpha);
            if (bowString != null)
            {
                Color start = bowString.startColor; start.a = alpha; bowString.startColor = start;
                Color end = bowString.endColor; end.a = alpha; bowString.endColor = end;
            }
        }
        private static void Fade(SpriteRenderer renderer, float alpha) { Color color = renderer.color; color.a *= alpha; renderer.color = color; }
    }
}
