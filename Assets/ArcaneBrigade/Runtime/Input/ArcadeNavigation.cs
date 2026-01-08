using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    public sealed class ArcadeNavigation
    {
        private struct Control { public bool Enabled; public Rect Bounds; }
        private readonly List<Control> controls = new List<Control>();
        private int selected;
        private bool submit;
        private float nextMove;
        private bool keyboardFocus;
        private readonly Dictionary<string, float> hover = new Dictionary<string, float>();
        private readonly Dictionary<string, float> pressed = new Dictionary<string, float>();
        public int Selected => selected;
        public static Vector2 Pad => new Vector2(Input.GetAxisRaw("BrigadePadHorizontal"), Input.GetAxisRaw("BrigadePadVertical"));
        public void ReadInput()
        {
            Vector2 pad = Pad;
            bool next = Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Tab) && !Input.GetKey(KeyCode.LeftShift);
            bool previous = Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.Tab) && Input.GetKey(KeyCode.LeftShift);
            if (pad.sqrMagnitude < 0.16f) nextMove = 0;
            if (Time.unscaledTime >= nextMove && pad.sqrMagnitude > 0.36f)
            {
                if (Mathf.Abs(pad.y) >= Mathf.Abs(pad.x)) { next |= pad.y < -0.6f; previous |= pad.y > 0.6f; }
                else { next |= pad.x > 0.6f; previous |= pad.x < -0.6f; }
                nextMove = Time.unscaledTime + 0.18f;
            }
            if (next || previous) Move(next ? 1 : -1);
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0)) { submit = true; keyboardFocus = true; }
        }
        public void Move(int direction)
        {
            if (controls.Count == 0) return;
            keyboardFocus = true;
            for (int attempt = 0; attempt < controls.Count; attempt++)
            {
                selected = (selected + (direction < 0 ? -1 : 1) + controls.Count) % controls.Count;
                if (controls[selected].Enabled) break;
            }
        }
        public void Reset() { selected = 0; submit = false; controls.Clear(); keyboardFocus = false; }
        public void QueueSubmit() { submit = true; keyboardFocus = true; }
        public void Begin() { controls.Clear(); }
        public bool Button(Rect bounds, string caption, GUIStyle style)
        {
            int index = controls.Count;
            controls.Add(new Control { Enabled = GUI.enabled, Bounds = bounds });
            bool focused = GUI.enabled && (bounds.Contains(Event.current.mousePosition) || keyboardFocus && selected == index);
            string identity = index + ":" + caption;
            float amount = hover.TryGetValue(identity, out float value) ? value : 0;
            if (Event.current.type == EventType.Repaint)
            {
                amount = Mathf.MoveTowards(amount, focused ? 1 : 0, Mathf.Min(Time.unscaledDeltaTime, 0.05f) * 7);
                if (hover.Count > 256) hover.Clear();
                hover[identity] = amount;
            }
            float shift = PresentationSettings.ReducedMotion ? 0 : amount * 4;
            Rect animatedBounds = new Rect(bounds.x + shift, bounds.y - shift * 0.5f, bounds.width, bounds.height);
            float compression = pressed.TryGetValue(identity, out float began) ? LivingUI.Press(Time.unscaledTime - began, PresentationSettings.ReducedMotion) * 0.025f : 0;
            animatedBounds = new Rect(animatedBounds.x + bounds.width * compression * 0.5f, animatedBounds.y + bounds.height * compression * 0.5f, bounds.width * (1 - compression), bounds.height * (1 - compression));
            bool clicked = GUI.Button(animatedBounds, caption, style);
            bool accepted = submit && keyboardFocus && index == selected && GUI.enabled && Event.current.type == EventType.Repaint;
            if (accepted) submit = false;
            if (keyboardFocus && selected == index && GUI.enabled)
            {
                ArcadeSkin.Line(new Rect(bounds.x + 8, bounds.y + 8, 5, bounds.height - 16), ArcadeSkin.Accent);
            }
            if (clicked || accepted) { submit = false; selected = index; if (pressed.Count > 256) pressed.Clear(); pressed[identity] = Time.unscaledTime; CheckpointAudio.PlayInterface(19); }
            return clicked || accepted;
        }
        public void End()
        {
            if (selected >= controls.Count) selected = Mathf.Max(0, controls.Count - 1);
            if (controls.Count > 0 && !controls[selected].Enabled) Move(1);
            if (Event.current.type == EventType.Repaint) submit = false;
        }
    }
}
