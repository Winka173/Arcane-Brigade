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
        public UnitState State => state;

        public void CopyDeathPose(SpriteRenderer targetBody, SpriteRenderer targetShadow, SpriteRenderer targetWeapon, SpriteRenderer targetHand, SpriteRenderer targetDrawHand, Transform targetGrip)
        {
            CopyPart(body, targetBody);
            CopyPart(shadow, targetShadow);
            targetGrip.localPosition = grip.localPosition;
            targetGrip.localRotation = grip.localRotation;
            CopyPart(weapon, targetWeapon);
            CopyPart(hand, targetHand);
            CopyPart(drawHand, targetDrawHand);
            targetWeapon.enabled = targetHand.enabled = grip.gameObject.activeSelf;
            targetDrawHand.enabled = drawHand != null && drawHand.gameObject.activeSelf && grip.gameObject.activeSelf;
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
            state = unit;
            if (flashProperties == null) flashProperties = new MaterialPropertyBlock();
            body.sprite = unit.Definition.Body;
            Material flash = Resources.Load<Material>("UnitFlash");
            if (flash != null) body.sharedMaterial = flash;
            shadow.color = unit.Faction == Faction.Champion ? new Color(0.95f, 0.76f, 0.32f, 0.6f)
                : unit.Faction == Faction.Companion ? new Color(0.3f, 0.75f, 0.72f, 0.4f) : new Color(0f, 0f, 0f, 0.25f);
            shadowColor = shadow.color;
            hand.sprite = catalog.Hand;
            weapon.sprite = unit.Definition.Weapon.Sprite;
            weapon.color = hand.color = Color.white;
            if (drawHand != null) drawHand.color = Color.white;
            bool armed = unit.Definition.Weapon.Kind != WeaponKind.Contact && weapon.sprite != null;
            grip.gameObject.SetActive(armed);
            if (armed)
            {
                float weaponScale = unit.Definition.Weapon.VisualWidth / weapon.sprite.bounds.size.x;
                weapon.transform.localScale = Vector3.one * weaponScale;
                hand.transform.localScale = Vector3.one * (0.13f / hand.sprite.bounds.size.x);
                muzzle.localPosition = unit.Definition.Weapon.HasMuzzleOffset ? (Vector3)unit.Definition.Weapon.ProjectileMuzzleOffset : Vector3.right * unit.Definition.Weapon.MuzzleDistance;
            }
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
                shadowColor = shadow.color = new Color(0.3f, 0.85f, 0.8f, 0.65f);
        }

        public void Display(float simulationTime, float interpolation)
        {
            if (state == null) return;
            weapon.color = hand.color = Color.white;
            shadow.color = shadowColor;
            if (drawHand != null) drawHand.color = Color.white;
            if (bowString != null) bowString.startColor = bowString.endColor = new Color(1, 0.92f, 0.68f);
            transform.position = Vector2.Lerp(state.PreviousPosition, state.Position, interpolation);
            WeaponDefinition definition = state.Definition.Weapon;
            float age = simulationTime - state.AttackStarted;
            bool attacking = age >= 0f && age < definition.Windup + definition.Recovery;
            Vector2 facing = attacking ? state.AttackDirection : state.Facing;
            if (facing.sqrMagnitude < 0.01f) facing = Vector2.right;
            int order = 1000 - Mathf.RoundToInt(state.Position.y * 12f);
            body.sortingOrder = order;
            shadow.sortingOrder = order - 10;
            bool behind = facing.y > 0.45f;
            weapon.sortingOrder = order + (behind ? -2 : 2);
            hand.sortingOrder = order + (behind ? -1 : 3);
            body.flipX = facing.x < -0.1f;
            float movement = Mathf.Clamp01((state.Position - state.PreviousPosition).magnitude * 20f);
            float bob = Mathf.Sin(simulationTime * 13f + state.Id) * 0.025f * movement;
            float attackLean = 0f;
            float impactSquash = 0f;
            if (attacking)
            {
                float anticipation = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(age / Mathf.Max(0.001f, definition.Windup)));
                float followThrough = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((age - definition.Windup) / Mathf.Max(0.001f, definition.Recovery)));
                float strike = Mathf.SmoothStep(0, 1, Mathf.Clamp01((anticipation - 0.65f) / 0.35f));
                attackLean = definition.BodyAttackTilt * (age < definition.Windup ? Mathf.Lerp(-anticipation * 0.4f, 1, strike) : followThrough);
                if (definition.Kind == WeaponKind.Melee && age >= definition.Windup) impactSquash = 0.08f * followThrough;
            }
            float scale = state.Definition.BodyHeight / body.sprite.bounds.size.y;
            float hitAge = simulationTime - state.LastHit;
            float hurt = Mathf.Clamp01(1f - hitAge / 0.14f);
            impactSquash += hurt * 0.09f;
            body.transform.localScale = new Vector3(scale * (1f + bob * 0.8f + impactSquash), scale * (1f - bob * 0.8f - impactSquash), 1f);
            body.transform.localPosition = new Vector3(state.HitDirection.x * hurt * 0.09f, -state.Definition.BodyHeight * 0.5f + Mathf.Abs(bob) + state.HitDirection.y * hurt * 0.09f, 0f);
            body.transform.localRotation = Quaternion.Euler(0f, 0f, bob * 90f + attackLean * (facing.x < 0f ? -1f : 1f));
            shadow.transform.localPosition = new Vector3(0f, -state.Definition.BodyHeight * 0.5f, 0f);
            body.color = Color.Lerp(Color.white, state.Definition.Tint, Mathf.Clamp01((simulationTime - state.LastHit) / 0.12f));
            if (state.ChillUntil > simulationTime) body.color = Color.Lerp(body.color, new Color(0.55f, 0.85f, 1f), 0.35f);
            else if (state.BurnUntil > simulationTime) body.color = Color.Lerp(body.color, new Color(1f, 0.6f, 0.3f), 0.2f + Mathf.PingPong(simulationTime * 2f, 0.15f));
            body.GetPropertyBlock(flashProperties);
            flashProperties.SetFloat("_Flash", Mathf.Clamp01(1f - hitAge / 0.075f));
            body.SetPropertyBlock(flashProperties);
            if (state.TelegraphUntil > simulationTime) body.color = Color.Lerp(state.Definition.Tint, new Color(1f, 0.4f, 0.3f), 0.5f + Mathf.Sin(simulationTime * 22f) * 0.4f);

            float angleOffset = 0f;
            float recoil = 0f;
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
                stampPips[index].transform.localPosition = new Vector3((index - 1) * 0.16f, state.Definition.BodyHeight * 0.65f, 0);
            }
            if (attacking)
            {
                if (definition.Kind == WeaponKind.Melee)
                {
                    float windup = Mathf.Clamp01(age / Mathf.Max(0.001f, definition.Windup));
                    float recovery = Mathf.Clamp01((age - definition.Windup) / Mathf.Max(0.001f, definition.Recovery));
                    float strike = Mathf.SmoothStep(0, 1, Mathf.Clamp01((windup - 0.7f) / 0.3f));
                    if (definition.Motion == WeaponMotion.Thrust)
                        recoil = age < definition.Windup ? Mathf.Lerp(0.12f * windup, -0.28f, strike) : -0.28f * (1f - Mathf.SmoothStep(0, 1, recovery));
                    else
                        angleOffset = age < definition.Windup ? Mathf.Lerp(-definition.SwingAngle * Mathf.SmoothStep(0, 1, windup), definition.SwingAngle, strike) : definition.SwingAngle * (1f - Mathf.SmoothStep(0, 1, recovery));
                    if (definition.Motion == WeaponMotion.Slam) recoil = age < definition.Windup ? -0.12f * strike : -0.12f * (1f - recovery);
                    if (definition.Motion == WeaponMotion.Stamp)
                    {
                        angleOffset = 0;
                        recoil = age < definition.Windup ? Mathf.Lerp(0.12f * windup, -0.32f, strike) : -0.32f * (1 - Mathf.SmoothStep(0, 1, recovery));
                    }
                }
                else if (age >= definition.Windup)
                {
                    recoil = definition.RecoilDistance * (1f - Mathf.Clamp01((age - definition.Windup) / Mathf.Max(0.001f, definition.Recovery)));
                    if (definition.Motion == WeaponMotion.Bell) angleOffset = Mathf.Sin((age - definition.Windup) * 45f) * 12f;
                }
                else if (definition.Motion == WeaponMotion.Cast)
                {
                    float windup = Mathf.Clamp01(age / Mathf.Max(0.001f, definition.Windup));
                    angleOffset = -22f * windup * (1 - Mathf.SmoothStep(0, 1, Mathf.Clamp01((windup - 0.7f) / 0.3f)));
                }
            }
            if (bowString != null && definition.Motion == WeaponMotion.Bow)
            {
                float pull = attacking && age < definition.Windup ? Mathf.Clamp01(age / Mathf.Max(0.001f, definition.Windup)) : 0f;
                float height = weapon.sprite.bounds.size.y * weapon.transform.localScale.y * 0.4f;
                bowString.sortingOrder = weapon.sortingOrder + 1;
                bowString.SetPosition(0, new Vector3(-0.18f, height, 0));
                bowString.SetPosition(1, new Vector3(-0.18f - pull * 0.17f, 0, 0));
                bowString.SetPosition(2, new Vector3(-0.18f, -height, 0));
                drawHand.sortingOrder = hand.sortingOrder + 1;
                drawHand.transform.localPosition = new Vector3(-0.18f - pull * 0.17f, 0, 0);
                recoil += pull * 0.06f;
            }
            grip.localPosition = facing * (definition.MountDistance - recoil);
            grip.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(facing.y, facing.x) * Mathf.Rad2Deg + angleOffset);
        }

        private void OnDestroy() { if (bowMaterial != null) Destroy(bowMaterial); }

        public void SetTerminalFade(float alpha)
        {
            Fade(body, alpha); Fade(weapon, alpha); Fade(hand, alpha); Fade(shadow, alpha);
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
