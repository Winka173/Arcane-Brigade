using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>World-space lettering with a dark ink outline and a soft drop shadow (18/10/2026), so captions in the
    /// scene (STAR UP, MASTERED, signs) read as thick painted lettering instead of thin bare text. Lilita One by default.</summary>
    public sealed class InkedText : MonoBehaviour
    {
        private static readonly Vector2[] Around = { new Vector2(1, 0), new Vector2(-1, 0), new Vector2(0, 1), new Vector2(0, -1), new Vector2(.7f, .7f), new Vector2(-.7f, .7f), new Vector2(.7f, -.7f), new Vector2(-.7f, -.7f) };
        public static readonly Color Ink = new Color(0.13f, 0.08f, 0.06f);
        private TextMesh face;
        private Font authoredFont;
        private TextMesh[] rim;
        private string text = "";
        private int languageRevision = -1;
        private Color colour = Color.white;
        public TextMesh Face => face;

        /// <summary>thickness: the outline's width in world units.</summary>
        public static InkedText Create(Transform parent, string name, Font font, int fontSize, float characterSize, int order, float thickness = 0.03f)
        {
            var root = new GameObject(name); root.transform.SetParent(parent, false);
            InkedText label = root.AddComponent<InkedText>();
            font = font != null ? font : ArcadeTypography.Punch;
            label.authoredFont = font;
            label.face = Make(root.transform, "Face", font, fontSize, characterSize, order, Vector2.zero);
            label.rim = new TextMesh[Around.Length + 1];
            for (int i = 0; i < Around.Length; i++) label.rim[i] = Make(root.transform, "InkRim", font, fontSize, characterSize, order - 1, Around[i] * thickness);
            label.rim[Around.Length] = Make(root.transform, "InkShadow", font, fontSize, characterSize, order - 2, new Vector2(thickness * 0.8f, -thickness * 1.6f));
            label.Apply();
            return label;
        }
        private static TextMesh Make(Transform parent, string name, Font font, int fontSize, float characterSize, int order, Vector2 offset)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.localPosition = offset;
            TextMesh mesh = go.AddComponent<TextMesh>();
            mesh.font = font; mesh.fontSize = fontSize; mesh.characterSize = characterSize; mesh.anchor = TextAnchor.MiddleCenter; mesh.alignment = TextAlignment.Center;
            MeshRenderer renderer = go.GetComponent<MeshRenderer>(); renderer.sharedMaterial = font.material; renderer.sortingOrder = order;
            return mesh;
        }

        public string Text { get => text; set { if (text == value) return; text = value ?? ""; Apply(); } }
        public Color Color { get => colour; set { if (colour == value) return; colour = value; Apply(); } }

        private void Apply()
        {
            if (face == null) return;
            languageRevision = GameLocalization.Revision;
            string shown = GameLocalization.Text(text);
            // Lilita One has no Vietnamese glyphs; use the complete storybook heading face for translated words.
            Font selectedFont = GameLocalization.IsVietnamese ? ArcadeTypography.Heading : authoredFont;
            if (face.font != selectedFont)
            {
                face.font = selectedFont;
                face.GetComponent<MeshRenderer>().sharedMaterial = face.font.material;
                foreach (TextMesh mesh in rim)
                {
                    mesh.font = face.font;
                    mesh.GetComponent<MeshRenderer>().sharedMaterial = face.font.material;
                }
            }
            face.text = shown; face.color = colour;
            for (int i = 0; i < rim.Length; i++)
            {
                rim[i].text = shown;
                rim[i].color = i < Around.Length ? new Color(Ink.r, Ink.g, Ink.b, colour.a) : new Color(0, 0, 0, colour.a * 0.45f);
            }
        }
        private void LateUpdate() { if (languageRevision != GameLocalization.Revision) Apply(); }
    }
}
