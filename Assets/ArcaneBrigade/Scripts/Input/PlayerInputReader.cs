using UnityEngine;
using UnityEngine.InputSystem;

namespace ArcaneBrigade.Input
{
    /// <summary>
    /// Thin wrapper around the New Input System exposing the gameplay actions used
    /// by the Champion. MVP uses raw Keyboard/Mouse devices for simplicity; this can
    /// be migrated to an .inputactions asset later without changing call sites.
    /// </summary>
    public class PlayerInputReader : MonoBehaviour
    {
        public Vector2 MoveAxis { get; private set; }
        public bool InteractPressed { get; private set; }
        public bool UltimatePressed { get; private set; }
        public bool RefreshShopPressed { get; private set; }

        private void Update()
        {
            var kb = Keyboard.current;
            if (kb == null)
            {
                MoveAxis = Vector2.zero;
                InteractPressed = false;
                UltimatePressed = false;
                RefreshShopPressed = false;
                return;
            }

            float x = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
            float y = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);
            // Arrow-key fallback.
            if (Mathf.Approximately(x, 0f))
                x = (kb.rightArrowKey.isPressed ? 1f : 0f) - (kb.leftArrowKey.isPressed ? 1f : 0f);
            if (Mathf.Approximately(y, 0f))
                y = (kb.upArrowKey.isPressed ? 1f : 0f) - (kb.downArrowKey.isPressed ? 1f : 0f);

            var raw = new Vector2(x, y);
            MoveAxis = raw.sqrMagnitude > 1f ? raw.normalized : raw;

            InteractPressed   = kb.eKey.wasPressedThisFrame;
            UltimatePressed   = kb.spaceKey.wasPressedThisFrame;
            RefreshShopPressed = kb.rKey.wasPressedThisFrame;
        }
    }
}
