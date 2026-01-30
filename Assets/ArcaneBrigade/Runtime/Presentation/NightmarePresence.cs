using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class NightmarePresence : MonoBehaviour
    {
        private SpriteRenderer halo, left, right;
        private static Sprite disc;
        private static Material glow;
        private string lightIdentity;
        private Color lightColor;
        public int VisibleCues { get; private set; }
        public static bool HasEyes(string id) => id switch
        {
            "HushMite" or "ClockBat" or "BristleImp" or "InkMoth" or "SirenSeal" or "DoorBearer" or "FurnaceToad" or "WardrobeCrab" or "ChargerRam" or "CoinLeech" or "BlastPuffer" or "BroomSkater" => true,
            _ => false
        };
        private SpriteRenderer Make(string label)
        {
            if (disc == null) disc = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height), Vector2.one * .5f, Texture2D.whiteTexture.width);
            if (glow == null) glow = Resources.Load<Material>("MienCho/WaitingGlow");
            var child = new GameObject(label); child.transform.SetParent(transform, false);
            var view = child.AddComponent<SpriteRenderer>(); view.sprite = disc; view.sharedMaterial = glow;
            return view;
        }
        public void Display(UnitState unit, SpriteRenderer body, float clock, float visibility)
        {
            if (halo != null) halo.enabled = false;
            if (left != null) left.enabled = false;
            if (right != null) right.enabled = false;
            VisibleCues = 0;
            if (glow == null) glow = Resources.Load<Material>("MienCho/WaitingGlow");
            if (glow == null || glow.shader == null || !glow.shader.isSupported) return;
            var field = SharedLightField.Active; var lighting = NightmareLighting.Active;
            if (field == null || lighting == null || !unit.Alive) return;
            float depth = lighting.Depth / 4 * (1 - lighting.Dawn);
            if (unit.Faction != Faction.Enemy)
            {
                halo ??= Make("CompanionOwnLight");
                halo.enabled = true;
                halo.transform.position = (Vector3)unit.Position + Vector3.up * .16f;
                halo.transform.localScale = new Vector3(2.4f, 1.1f, 1);
                // Set world size independently of animation/star parent scale.
                halo.transform.localScale = Vector3.Scale(halo.transform.localScale, new Vector3(1 / transform.lossyScale.x, 1 / transform.lossyScale.y, 1));
                if (lightIdentity != unit.Definition.Id) { lightIdentity = unit.Definition.Id; lightColor = SharedLightField.IdentityColor(lightIdentity); }
                Color tint = lightColor; tint.a = Mathf.Lerp(.035f, .15f, depth);
                halo.color = tint; halo.sortingOrder = body.sortingOrder - 3;
                return;
            }
            if (unit.Definition.IsBoss || depth < .35f || visibility > .96f) return;
            left ??= Make("RealEnemyEyeOrEmber"); right ??= Make("RealEnemySecondEye");
            bool pair = HasEyes(unit.Definition.Id);
            Bounds ink = body.bounds;
            Vector3 face = new Vector3(ink.center.x, ink.min.y + ink.size.y * .66f, 0);
            float separation = Mathf.Clamp(ink.size.x * .13f, .06f, .18f);
            float alpha = Mathf.Lerp(.32f, .85f, depth) * Mathf.Clamp01((1 - visibility) * 2);
            left.enabled = true; right.enabled = pair; VisibleCues = pair ? 2 : 1;
            left.transform.position = face + Vector3.left * (pair ? separation : 0);
            right.transform.position = face + Vector3.right * separation;
            Vector3 worldSize = pair ? new Vector3(.16f, .085f, 1) : new Vector3(.16f, .24f, 1);
            var size = Vector3.Scale(worldSize, new Vector3(1 / transform.lossyScale.x, 1 / transform.lossyScale.y, 1));
            left.transform.localScale = right.transform.localScale = size;
            left.color = right.color = pair ? new Color(1, .035f, .045f, alpha) : new Color(1, .26f, .055f, alpha);
            left.sortingOrder = right.sortingOrder = body.sortingOrder + 5;
        }
    }
}
