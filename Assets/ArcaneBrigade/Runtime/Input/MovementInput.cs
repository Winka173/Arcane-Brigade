using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class MovementInput
    {
        private int fingerId = -1;
        private Vector2 origin;
        private Vector2 offset;
        public bool TouchActive => fingerId >= 0;
        public Vector2 TouchOrigin => origin;
        public Vector2 TouchOffset => offset;
        public float TouchRadius { get; private set; }

        public Vector2 Read(Rect viewport)
        {
            float horizontal = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1f : 0f) - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1f : 0f);
            float vertical = (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1f : 0f) - (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1f : 0f);
            Vector2 keyboard = Vector2.ClampMagnitude(new Vector2(horizontal, vertical), 1f);
            Vector2 pad = FilterPad(ArcadeNavigation.Pad);
            if (pad.sqrMagnitude > keyboard.sqrMagnitude) keyboard = pad;
            TouchRadius = Mathf.Max(42f, viewport.height * 0.075f);
            bool tracked = false;
            Rect safe = Screen.safeArea;
            for (int index = 0; index < Input.touchCount; index++)
            {
                Touch touch = Input.GetTouch(index);
                if (fingerId < 0 && touch.phase == TouchPhase.Began && viewport.Contains(touch.position) && safe.Contains(touch.position) && touch.position.x < viewport.center.x && touch.position.y < viewport.y + viewport.height * 0.65f)
                {
                    fingerId = touch.fingerId;
                    origin = touch.position;
                }
                if (touch.fingerId != fingerId) continue;
                tracked = true;
                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) Reset();
                else offset = Vector2.ClampMagnitude(touch.position - origin, TouchRadius);
            }
            if (!tracked && fingerId >= 0) Reset();
            return TouchActive ? offset / TouchRadius : keyboard;
        }

        public void Reset() { fingerId = -1; offset = Vector2.zero; }
        public static Vector2 FilterPad(Vector2 value)
        {
            if (!float.IsFinite(value.x) || !float.IsFinite(value.y) || value.magnitude <= 0.18f) return Vector2.zero;
            return value.normalized * Mathf.Clamp01((value.magnitude - 0.18f) / 0.82f);
        }
    }
}
