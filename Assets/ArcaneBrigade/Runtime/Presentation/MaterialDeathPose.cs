using UnityEngine;

namespace ArcaneBrigade
{
    public static class MaterialDeathPose
    {
        public static Vector2 Shape(BodyMaterial material, float collapse)
        {
            collapse = Mathf.Clamp01(collapse);
            if (material == BodyMaterial.Wax) return new Vector2(1 + collapse * 0.7f, 1 - collapse * 0.82f);
            if (material == BodyMaterial.Fabric) return new Vector2(1 - collapse * 0.48f, 1 - collapse * 0.65f);
            if (material == BodyMaterial.Glass) return Vector2.one * (1 - collapse * 0.65f);
            if (material == BodyMaterial.Metal || material == BodyMaterial.Wood) return new Vector2(1 + collapse * 0.05f, 1 - collapse * 0.1f);
            return new Vector2(1 + collapse * 0.3f, 1 - collapse * 0.75f);
        }
    }
}
