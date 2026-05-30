using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>The transformation's name as hand-lettered word art (owner 24/10/2026: "SYMPHONY!" read as plain text). Each
    /// letter is its own inked piece on a gentle arch, in a run of the hero's colours, and arrives in its hero's manner:
    /// stamped (brush, ink), bobbing on a wave, licking up like flame, flickering in like a bolt, swinging down on a thread
    /// or drifting in on silk. A brush swash draws in under the word with the hero's token at each end; then the letters
    /// lift away one by one. Regal heroes letter in the serif display face, the rest in the round punch face.</summary>
    public sealed partial class HeroIdentityVisual
    {
        private const float TitleLife = 2.1f, TitleSize = .068f; // 2.1: the stat lines stay a moment after the name lifts away
        private const int TitleFontSize = 52;
        private readonly List<InkedText> titleLetters = new List<InkedText>();
        private readonly List<float> titleAdvance = new List<float>();
        private string titleShown;
        private Font titleFont;
        private bool titleDrawn; // set while the title shows this frame; Display hides the letters otherwise
        private readonly List<InkedText> statLines = new List<InkedText>();
        private readonly List<string> statText = new List<string>();

        private static bool Regal(string id) => id == "VelvetRegent" || id == "IvorySovereign" || id == "ArchiveCurator" || id == "PennyBroker" || id == "ThornTreasurer" || id == "PetalCitadel" || id == "HourglassStag" || id == "MirrorKeeper";

        private void HideTitle()
        {
            foreach (InkedText l in titleLetters) if (l != null && l.gameObject.activeSelf) l.gameObject.SetActive(false);
            foreach (InkedText l in statLines) if (l != null && l.gameObject.activeSelf) l.gameObject.SetActive(false);
        }
        /// <summary>What the transformation gives the hero itself, in the shop's own words ("Attack speed +40%").</summary>
        private void KitStats(HeroKit kit)
        {
            statText.Clear();
            if (kit.HeroAttackSpeed > 0) statText.Add("Attack speed +" + Mathf.RoundToInt(kit.HeroAttackSpeed * 100) + "%");
            if (kit.HeroMoveSpeed > 0) statText.Add("Move speed +" + Mathf.RoundToInt(kit.HeroMoveSpeed * 100) + "%");
            if (kit.HeroDamageTaken < 1) statText.Add("Damage taken -" + Mathf.RoundToInt((1 - kit.HeroDamageTaken) * 100) + "%");
            if (kit.HeroCrit > 0) statText.Add("Crit chance +" + Mathf.RoundToInt(kit.HeroCrit * 100) + "%");
            if (kit.TeamAttackSpeed > 0) statText.Add("Friends' attack speed +" + Mathf.RoundToInt(kit.TeamAttackSpeed * 100) + "%");
            if (kit.Action == KitAction.Treant) statText.Add("Max HP +50%");
            if (kit.Action == KitAction.Kintsugi) statText.Add("Untouchable 3s");
        }

        /// <summary>The transformation's name lettered over the hero for a moment as it begins.</summary>
        private void Banner(bool awake, HeroKit kit)
        {
            if (awake && sim.HeroAwakenStarted > bannerFor) bannerFor = sim.HeroAwakenStarted;
            float age = t - bannerFor;
            if (!awake || kit == null || age >= TitleLife) return;
            titleDrawn = true;
            string word = kit.Transformation + "!";
            Font font = Regal(style.Id) ? ArcadeTypography.Display : ArcadeTypography.Punch;
            if (word != titleShown || font != titleFont) Letter(word, font);

            // the word's width from each glyph's advance (TextMesh draws ten font pixels to a unit at character size 1)
            float width = 0; foreach (float a in titleAdvance) width += a;
            float scale = Mathf.Min(1, 3.2f / Mathf.Max(.01f, width)); // a long name is lettered smaller, never wider than 3.2 units
            Vector2 centre = Head(sim.Champion) + Vector2.up * .75f;
            float x = -width * .5f;
            int n = titleLetters.Count;
            Color from = Color.Lerp(ink.Light, Color.white, .3f), to = Color.Lerp(ink.Body, ink.Light, .25f);
            for (int i = 0; i < n; i++)
            {
                InkedText letter = titleLetters[i];
                float half = titleAdvance[i] * .5f, u = n > 1 ? i / (n - 1f) * 2 - 1 : 0; x += half;
                float hand = ((i * 37 + word.Length * 11) % 9 - 4) * 1f; // each letter's own small lean, as if drawn by hand
                Vector2 rest = centre + new Vector2(x * scale, -.16f * u * u + .08f);    // a gentle arch
                float local = age - i * .035f, arrive = Mathf.Clamp01(local / .28f), leave = Mathf.Clamp01((age - 1.12f - i * .025f) / .3f);
                Vector2 at = rest; float angle = hand, size = 1, alpha = 1;
                if (!still)
                {
                    switch (style.Brush)
                    {
                        case InkBrush.Wave:
                            at += Vector2.down * .3f * (1 - OutCubic(arrive)) + Vector2.up * Mathf.Sin(t * 6 + i * .7f) * .035f;
                            size = OutBack(arrive); break;
                        case InkBrush.Flame:
                            at += Vector2.down * .25f * (1 - OutCubic(arrive)) + Vector2.up * Mathf.Abs(Mathf.Sin(t * 13 + i * 1.9f)) * .025f;
                            size = OutCubic(arrive) * (1 + .06f * Mathf.Sin(t * 21 + i)); break;
                        case InkBrush.Bolt:
                            alpha = arrive >= 1 ? 1 : Mathf.Repeat(t * 24 + i * .37f, 1) < .55f ? 1 : .15f;
                            at += new Vector2(Mathf.Sin(t * 47 + i * 3), Mathf.Cos(t * 53 + i)) * .012f * (1 - leave);
                            size = arrive > 0 ? 1 : 0; break;
                        case InkBrush.Thread:
                            at += Vector2.up * .45f * (1 - OutCubic(arrive));
                            angle += Mathf.Sin(local * 11) * 24 * Mathf.Exp(-local * 4) * (arrive > 0 ? 1 : 0);
                            size = arrive > 0 ? 1 : 0; break;
                        case InkBrush.Silk:
                            at += new Vector2(-.5f * (1 - OutCubic(arrive)), Mathf.Sin(t * 3 + i * .5f) * .03f);
                            angle += Mathf.Sin(t * 2.5f + i * .6f) * 4;
                            size = OutCubic(arrive); break;
                        default: // stamped down from above, overshooting, a little off its lean before it settles
                            at += Vector2.up * .3f * (1 - OutCubic(arrive));
                            size = arrive <= 0 ? 0 : 1 + .55f * (1 - OutBack(arrive));
                            angle += (1 - arrive) * (i % 2 == 0 ? 14 : -14); break;
                    }
                    at += Vector2.up * .35f * leave * leave; size *= 1 - .4f * leave; alpha *= 1 - leave;
                }
                if (local < 0 && !still) alpha = 0;
                Color c = Color.Lerp(from, to, n > 1 ? i / (n - 1f) : 0);
                float flash = still ? 0 : Mathf.Clamp01(1 - (local - .2f) / .12f) * (arrive >= .7f ? 1 : 0); // a letter flashes white as it lands
                c = Color.Lerp(c, Color.white, .7f * flash); c.a = alpha;
                letter.gameObject.SetActive(alpha > .01f);
                letter.transform.position = new Vector3(at.x, at.y, 0);
                letter.transform.localRotation = Quaternion.Euler(0, 0, angle);
                letter.transform.localScale = Vector3.one * Mathf.Max(.01f, size * scale);
                letter.Color = c;
                x += half;
            }
            // owner 24/10/2026: under the name, what the transformation gives the hero, line by line like the shop's purchase
            // lines (green, with a white flash as each lands)
            KitStats(kit);
            while (statLines.Count < statText.Count) statLines.Add(InkedText.Create(transform, "Skill stat line", ArcadeTypography.Punch, 44, .042f, 2399, .026f));
            for (int i = 0; i < statLines.Count; i++)
            {
                InkedText line = statLines[i];
                if (i >= statText.Count) { if (line.gameObject.activeSelf) line.gameObject.SetActive(false); continue; }
                float local = age - .45f - i * .18f, show = still ? 1 : Mathf.Clamp01(local / .2f), leave = still ? 0 : Mathf.Clamp01((age - 1.6f - i * .05f) / .3f);
                bool on = (still || local > 0) && leave < 1;
                if (line.gameObject.activeSelf != on) line.gameObject.SetActive(on);
                if (!on) continue;
                line.Text = statText[i];
                line.transform.position = (Vector3)(centre + Vector2.down * (.46f + i * .3f) + Vector2.up * (.12f * leave - .08f * (1 - show)));
                line.transform.localScale = Vector3.one * Mathf.Max(.01f, OutBack(show));
                Color g = Color.Lerp(ArcadeSkin.Better, Color.white, .6f * Mathf.Clamp01(1 - local / .15f)); g.a = 1 - leave;
                line.Color = g;
            }
            // the swash: one tapered brush stroke drawn in under the word, the hero's token popping at each end
            float sw = still ? 1 : OutCubic(Mathf.Clamp01((age - .18f) / .3f)), gone = still ? 0 : Mathf.Clamp01((age - 1.15f) / .35f);
            if (sw > 0 && !sparse)
            {
                Vector2 left = centre + new Vector2(-width * .5f * scale - .08f, -.32f), right = centre + new Vector2(width * .5f * scale + .08f, -.27f);
                var swash = InkCanvas.Lob(left, right, -.1f, 18);
                Soft(air, swash, sw, gone, .065f, .9f * (1 - gone));
                Soft(air, InkCanvas.Lob(Vector2.Lerp(left, right, .25f), Vector2.Lerp(left, right, .75f), -.07f, 12), Mathf.Clamp01((sw - .25f) / .5f), gone, .085f, .8f * (1 - gone));
                float pop = Mathf.Clamp01((age - .45f) / .25f);
                if (pop > 0)
                    for (int e = 0; e < 2; e++)
                        Token(style.Token, (e == 0 ? left : right) + Vector2.up * .04f, .2f * (still ? 1 : OutBack(pop)), still ? 0 : (e == 0 ? -1 : 1) * (20 + age * 40), 1 - gone, true);
            }
        }

        private void Letter(string word, Font font)
        {
            titleShown = word; titleFont = font;
            font.RequestCharactersInTexture(word, TitleFontSize);
            while (titleLetters.Count < word.Length) titleLetters.Add(InkedText.Create(transform, "Skill title letter", font, TitleFontSize, TitleSize, 2400, .03f));
            titleAdvance.Clear();
            for (int i = 0; i < titleLetters.Count; i++)
            {
                InkedText letter = titleLetters[i];
                if (i >= word.Length) { letter.gameObject.SetActive(false); continue; }
                SetFont(letter, font);
                letter.Text = word[i].ToString();
                float advance = font.GetCharacterInfo(word[i], out CharacterInfo info, TitleFontSize) ? info.advance : TitleFontSize * .55f;
                titleAdvance.Add(advance * TitleSize * .1f);
            }
            while (titleLetters.Count > word.Length) { Destroy(titleLetters[titleLetters.Count - 1].gameObject); titleLetters.RemoveAt(titleLetters.Count - 1); }
        }
        private static void SetFont(InkedText letter, Font font)
        {
            foreach (TextMesh mesh in letter.GetComponentsInChildren<TextMesh>(true))
            {
                if (mesh.font == font) continue;
                mesh.font = font; mesh.GetComponent<MeshRenderer>().sharedMaterial = font.material;
            }
        }
    }
}
