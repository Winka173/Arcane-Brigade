using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Shop 8: the Last Customer waits at the end of the platform and offers a lantern for a memory of one of the
    /// traveller's companions (Docs/Mien_Cho_Act_One_Story.md). Selling makes that companion a stranger; refusing costs nothing.
    /// From shop 9 he now and then comes back to buy a friend outright (RunSession.MemorySaleOpen): all it cost, in gold.</summary>
    public sealed class CustomerOffer : MonoBehaviour
    {
        private ArenaController owner;
        private GUIStyle hand, plain, name;
        private readonly List<string> species = new List<string>();
        private int page; // nine companions per page; the arrows turn it
        // the friend is picked first and sold only with the confirm button (requirement 22/10/2026: one tap sold a memory)
        private string picked;
        public int InputConsumedFrame { get; private set; } = -1;
        public void Configure(ArenaController value) => owner = value;
        /// <summary>While the offer stands, the shop does not also act on the traveller's presses.</summary>
        public bool Open => Standing && !folded;
        /// <summary>The offer stands (open, or folded away as a letter waiting on the HUD).</summary>
        private bool Standing => owner?.Session != null && (owner.Session.CustomerOfferAvailable || owner.Session.MemorySaleOpen) && !owner.StoryConsumesInput && !owner.TransitionPending && !owner.Session.SecretRoomOpen;
        // requirement 24/10/2026: Esc used to refuse for good before the letter was even read; now Esc folds it away into a letter that
        // waits on the screen (tap to reopen) - only WALK ON refuses. It unfolds from its seal each time it opens.
        private bool folded, wasOpen; private float openedAt = -10;
        private static readonly Rect LetterRect = new Rect(1640, 232, 250, 64);

        private bool Sale => owner.Session.MemorySaleOpen;
        private Sprite saleIcon;
        private void Gather()
        {
            species.Clear();
            if (Sale) { species.AddRange(owner.Session.PartingSpecies()); return; }
            foreach (RosterEntry entry in owner.Session.State.Roster) if (!species.Contains(entry.UnitId) && !RunSession.StorySpecies(entry.UnitId)) species.Add(entry.UnitId);
        }
        private void Update()
        {
            if (!Open || owner.PresentationPaused) return;
            if (!Application.isFocused && !RunPaths.Diagnostic) return;
            Gather();
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1)) { InputConsumedFrame = Time.frameCount; folded = true; CheckpointAudio.PlayInterfaceId("ui.back"); return; }
            int pages = Mathf.Max(1, (species.Count + 8) / 9); page = Mathf.Clamp(page, 0, pages - 1);
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.JoystickButton5)) page = (page + 1) % pages;
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.JoystickButton4)) page = (page + pages - 1) % pages;
            for (int i = 0; i < Mathf.Min(9, species.Count - page * 9); i++)
                if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i)) { picked = species[page * 9 + i]; return; }
            if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && picked != null && species.Contains(picked)) Sell();
        }
        private void Sell() { InputConsumedFrame = Time.frameCount; if (Sale ? owner.SellToCustomer(picked) : owner.SellMemory(picked)) picked = null; }
        private void Seal(Vector2 at, float size)
        {
            if (Sale && (saleIcon ??= Resources.Load<Sprite>("UI/Items/MemorySale")) != null) RunHud.Sprite(new Rect(at.x - size * .7f, at.y - size * .7f, size * 1.4f, size * 1.4f), saleIcon);
            else ArcadeSkin.Seal(at, size, "$");
        }
        private void OnGUI()
        {
            if (!Standing) { folded = false; wasOpen = false; return; }
            if (owner.PresentationPaused || Camera.main == null) return;
            if (folded)
            {
                // the folded letter waits under the wallet: a sealed ticket; a tap opens it again
                GUI.depth = -55; Matrix4x4 keepMatrix = ArcadeTypography.BeginViewport(Camera.main.pixelRect, 1920, 1080);
                if (name == null) { name = ArcadeTypography.Style(ArcadeTypography.SizeNote, true); name.alignment = TextAnchor.MiddleCenter; name.wordWrap = true; }
                float bob = PresentationSettings.ReducedMotion ? 0 : Mathf.Sin(Time.unscaledTime * 2.4f) * 3;
                Rect letter = new Rect(LetterRect.x, LetterRect.y + bob, LetterRect.width, LetterRect.height);
                ArcadeSkin.Ticket(letter, ArcadeSkin.Paper, ArcadeSkin.Wax); Seal(new Vector2(letter.x + 34, letter.center.y), 34);
                name.normal.textColor = new Color(.25f, .19f, .14f);
                GUI.Label(new Rect(letter.x + 60, letter.y + 6, letter.width - 76, letter.height - 12), "A letter from the Last Customer", name);
                if (TouchTargets.Button(letter, GUIContent.none, GUIStyle.none)) { folded = false; InputConsumedFrame = Time.frameCount; CheckpointAudio.PlayInterfaceId("ui.select"); }
                GUI.matrix = keepMatrix; wasOpen = false; return;
            }
            if (!wasOpen) { wasOpen = true; openedAt = Time.unscaledTime; }
            Gather();
            GUI.depth = -60; // above the HUD and any speech
            Matrix4x4 previous = ArcadeTypography.BeginViewport(Camera.main.pixelRect, 1920, 1080);
            if (hand == null)
            {
                hand = ArcadeTypography.Style(30); hand.font = ArcadeTypography.Hand; hand.alignment = TextAnchor.MiddleCenter; hand.wordWrap = true;
                plain = ArcadeTypography.Style(ArcadeTypography.SizeNote); plain.alignment = TextAnchor.UpperCenter; plain.wordWrap = true;
                name = ArcadeTypography.Style(22, true); name.alignment = TextAnchor.MiddleCenter; name.wordWrap = true; // portrait captions
            }
            RunHud.Fill(new Rect(-1920, -1080, 5760, 3240), new Color(0, 0, 0, 0.45f));
            int pages = Mathf.Max(1, (species.Count + 8) / 9); page = Mathf.Clamp(page, 0, pages - 1);
            int first = page * 9, shown = Mathf.Min(9, species.Count - first);
            float width = Mathf.Max(820, 140 + shown * 166); // a clear gap between the round frames
            Rect card = new Rect((1920 - width) * 0.5f, 300, width, 640);
            Color popKeep = CardPop.Apply("last-customer", card);
            // the letter unfolds from its seal: from a thin fold to full height with a small overshoot, scaled in the HUD's own
            // design space (no pivot helpers: they misplace under the scaled HUD matrix)
            float unfold = PresentationSettings.ReducedMotion ? 1 : Mathf.Clamp01((Time.unscaledTime - openedAt) / .42f);
            if (unfold < 1) { float k = unfold - 1, sy = Mathf.Max(.04f, 1 + 2.4f * k * k * k + 1.4f * k * k); Vector3 pv = new Vector3(card.center.x, card.y + 40, 0); GUI.matrix = GUI.matrix * Matrix4x4.TRS(pv, Quaternion.identity, Vector3.one) * Matrix4x4.Scale(new Vector3(1, sy, 1)) * Matrix4x4.TRS(-pv, Quaternion.identity, Vector3.one); }
            ArcadeSkin.Ticket(card, ArcadeSkin.Paper, ArcadeSkin.Wax);
            Seal(new Vector2(card.xMax - 42, card.y + 40), 46);
            hand.normal.textColor = new Color(.25f, .19f, .14f);
            GUI.Label(new Rect(card.x + 50, card.y + 26, card.width - 100, 120), Sale ? "THE LAST CUSTOMER: 'I'll take one of them off your hands. Every coin they cost you, back in your pocket.'" : "THE LAST CUSTOMER: 'Sell me your memory of one of them, and I'll light a lamp for you. You'd hardly miss it.'", hand);
            plain.normal.textColor = new Color(.18f, .14f, .1f);
            GUI.Label(new Rect(card.x + 50, card.y + 148, card.width - 100, 84), Sale ? "The one you choose leaves for good. No letter, no new place: only gold." : "The one you choose stays with you, but becomes a stranger: you forget who they are, and they no longer count toward bonds. One broken lantern burns again.", plain);
            for (int i = 0; i < shown; i++)
            {
                UnitDefinition unit = owner.Content.Unit(species[first + i]);
                // the portrait in its round frame, the name in ink on the paper below it (no key numbers: phones have no keys)
                Rect slot = new Rect(card.x + (card.width - shown * 166) * 0.5f + i * 166 + 20, card.y + 236, 126, 184), frame = new Rect(slot.x + 4, slot.y, 118, 118);
                bool isPicked = picked == species[first + i];
                Matrix4x4 unscaled = GUI.matrix;
                // the picked one keeps its size and place (the chosen seal marks it)
                ArcadeSkin.Plate(frame, ArcadeSkin.Card);
                if (unit != null) RunHud.Sprite(new Rect(frame.x + 12, frame.y + 10, 94, 94), unit.Body); // the friend fills its medallion
                GUI.matrix = unscaled;
                if (isPicked) ArcadeSkin.PickedSeal(frame); // the picked friend's medallion is stamped
                name.normal.textColor = isPicked ? new Color(.62f, .27f, .05f) : new Color(.18f, .14f, .1f); // and its name in warm brown
                TextAnchor keepName = name.alignment; name.alignment = TextAnchor.UpperCenter;
                GUI.Label(new Rect(slot.x - 10, frame.yMax + 4, slot.width + 20, 66), unit == null ? species[first + i] : RunUICopy.UnitName(unit), name); // two full lines, never clipped
                name.alignment = keepName;
                if (TouchTargets.Button(slot, GUIContent.none, GUIStyle.none)) { picked = species[first + i]; CheckpointAudio.PlayInterfaceId("ui.select"); }
            }
            if (pages > 1)
            {
                // more companions than fit: page through them
                Rect previousPage = new Rect(card.x + 20, card.y + 290, 44, 60), nextPage = new Rect(card.xMax - 64, card.y + 290, 44, 60);
                ArcadeSkin.Plate(previousPage, ArcadeSkin.Card); ArcadeSkin.Plate(nextPage, ArcadeSkin.Card);
                name.normal.textColor = ArcadeSkin.Cream;
                if (TouchTargets.Button(previousPage, "◀", name)) page = (page + pages - 1) % pages;
                if (TouchTargets.Button(nextPage, "▶", name)) page = (page + 1) % pages;
                plain.normal.textColor = new Color(.18f, .14f, .1f);
                GUI.Label(new Rect(card.x, card.y + 395, card.width, 22), (page + 1) + " / " + pages, plain);
            }
            // what selling the picked one means, then SELL and WALK ON side by side (painted buttons, one size)
            bool ready = picked != null && species.Contains(picked);
            UnitDefinition chosen = ready ? owner.Content.Unit(picked) : null;
            plain.normal.textColor = ready ? new Color(.55f, .12f, .1f) : new Color(.18f, .14f, .1f);
            GUI.Label(new Rect(card.x + 50, card.y + 436, card.width - 100, 60), ready
                ? Sale ? (chosen != null ? RunUICopy.UnitName(chosen) : picked) + " leaves for good. You get " + owner.Session.MemorySalePrice(picked) + " gold."
                : (chosen != null ? RunUICopy.UnitName(chosen) : picked) + " stays and fights, but no longer counts toward bonds. A broken lantern burns again."
                : "Pick a friend to see what you would give up.", plain);
            float buttonsY = card.yMax - 120;
            Rect sell = new Rect(card.center.x - 380, buttonsY, 340, 88), refuse = new Rect(card.center.x + 40, buttonsY, 340, 88);
            // both answers close the letter with its motion first (requirement 25/10/2026)
            if (ArcadeSkin.Button(sell, ready ? (Sale ? "SELL" : "SELL THE MEMORY") : "PICK ONE", KitButton.Danger, ready, name) && !CardPop.Closing("last-customer")) CardPop.CloseThen("last-customer", Sell);
            if (ArcadeSkin.Button(refuse, "WALK ON", KitButton.Secondary, true, name) && !CardPop.Closing("last-customer")) { InputConsumedFrame = Time.frameCount; CardPop.CloseThen("last-customer", () => { if (Sale) owner.DeclineMemorySale(); else owner.RefuseCustomer(); }); }
            if (unfold < .5f && !PresentationSettings.ReducedMotion) Seal(new Vector2(card.center.x, card.y + 40), 60 * (1 - unfold * 2) + 10); // the seal breaking open
            GUI.color = popKeep;
            GUI.matrix = previous;
        }
    }

    /// <summary>After SIT CLOSER is won: the lanterns that went dark are listed with what each one held; one comes back.</summary>
    public sealed class MemoryReturn : MonoBehaviour
    {
        private ArenaController owner;
        private GUIStyle hand, plain, label;
        private readonly Sprite[] icons = new Sprite[7];
        private static readonly string[] IconNames = { "Memory_Voice", "Memory_Raincoat", "Memory_Height", "Memory_Rain", "Memory_Face", "Memory_Knock", "Memory_Handwriting" };
        public void Configure(ArenaController value)
        {
            owner = value;
            for (int i = 0; i < icons.Length; i++) icons[i] = Resources.Load<Sprite>("MienCho/ActOne/" + IconNames[i]);
        }
        public bool Open => owner?.Session != null && owner.Session.RelightChoiceOpen && !owner.StoryConsumesInput && !owner.TransitionPending;
        private readonly List<int> dark = new List<int>();
        // picked first, then confirmed (requirement 22/10/2026: one tap chose before the player knew what it meant); -1 none, dark.Count the colours
        private int picked = -1;
        private GUIStyle info;
        private void Gather()
        {
            dark.Clear();
            for (int lamp = 0; lamp < 7; lamp++) if (ActOneCanon.LampState(lamp, owner.Session.State.Wave, owner.Session.State.ActOne.RelitMask) == 2) dark.Add(lamp);
        }
        private void Update()
        {
            if (!Open || owner.PresentationPaused || !Application.isFocused && !RunPaths.Diagnostic) return;
            Gather();
            for (int i = 0; i < dark.Count; i++)
                if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i)) { picked = i; return; }
            if (owner.Session.ColoursAvailable && (Input.GetKeyDown(KeyCode.Alpha1 + dark.Count) || Input.GetKeyDown(KeyCode.Keypad1 + dark.Count))) picked = dark.Count;
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) Confirm();
        }
        private void Confirm()
        {
            if (picked >= 0 && picked < dark.Count) { if (owner.ChooseRelight(dark[picked])) picked = -1; }
            else if (picked == dark.Count && owner.Session.ColoursAvailable) { if (owner.ChooseColours()) picked = -1; }
        }
        private void OnGUI()
        {
            if (!Open || owner.PresentationPaused || Camera.main == null) return;
            Gather();
            GUI.depth = -60; // above the HUD and any speech
            Matrix4x4 previous = ArcadeTypography.BeginViewport(Camera.main.pixelRect, 1920, 1080);
            if (hand == null)
            {
                hand = ArcadeTypography.Style(32); hand.font = ArcadeTypography.Hand; hand.alignment = TextAnchor.MiddleCenter; hand.wordWrap = true;
                plain = ArcadeTypography.Style(ArcadeTypography.SizeNote); plain.alignment = TextAnchor.UpperCenter; plain.wordWrap = true;
                label = ArcadeTypography.Style(ArcadeTypography.SizeNote, true); label.alignment = TextAnchor.MiddleCenter; label.wordWrap = true;
            }
            RunHud.Fill(new Rect(-1920, -1080, 5760, 3240), new Color(0, 0, 0, 0.45f));
            bool colours = owner.Session.ColoursAvailable; int slots = dark.Count + (colours ? 1 : 0);
            float width = Mathf.Max(820, 120 + slots * 165);
            Rect card = new Rect((1920 - width) * 0.5f, 300, width, 600);
            Color popKeep = CardPop.Apply("memory-return", card); // every card opens the same way
            ArcadeSkin.Ticket(card, ArcadeSkin.Paper, ArcadeSkin.Accent);
            hand.normal.textColor = new Color(.25f, .19f, .14f);
            GUI.Label(new Rect(card.x + 50, card.y + 24, card.width - 100, 90), "You stood up. Nobody paid. Outside, one of the dark lanterns is warm again.", hand);
            plain.normal.textColor = new Color(.18f, .14f, .1f);
            GUI.Label(new Rect(card.x + 50, card.y + 118, card.width - 100, 30), colours ? "Choose what comes back to you, or take the colours instead." : "Choose what comes back to you.", plain);
            for (int i = 0; i < dark.Count; i++)
            {
                int lamp = dark[i];
                Rect slot = new Rect(card.x + (card.width - slots * 165) * 0.5f + i * 165 + 8, card.y + 165, 150, 190);
                ArcadeSkin.Plate(slot, picked == i ? ArcadeSkin.Gold : ArcadeSkin.Card); // the picked lamp's plate turns gold
                if (icons[lamp] != null) RunHud.Sprite(new Rect(slot.x + 25, slot.y + 10, 100, 100), icons[lamp]);
                else ArcadeSkin.Glow(new Rect(slot.x + 35, slot.y + 20, 80, 80), new Color(ArcadeSkin.Gold.r, ArcadeSkin.Gold.g, ArcadeSkin.Gold.b, 0.5f));
                label.normal.textColor = ArcadeSkin.Cream;
                GUI.Label(new Rect(slot.x + 6, slot.y + 115, slot.width - 12, 70), (i + 1) + " · " + ActOneCanon.LampHeld[lamp], label);
                if (TouchTargets.Button(slot, GUIContent.none, GUIStyle.none)) { picked = i; CheckpointAudio.PlayInterfaceId("ui.select"); }
            }
            if (colours)
            {
                Rect slot = new Rect(card.x + (card.width - slots * 165) * 0.5f + dark.Count * 165 + 8, card.y + 165, 150, 190);
                ArcadeSkin.Plate(slot, picked == dark.Count ? ArcadeSkin.Gold : ArcadeSkin.Card);
                Sprite prism = PacingArtwork.Prism(owner.Content);
                if (prism != null) RunHud.Sprite(new Rect(slot.x + 25, slot.y + 10, 100, 100), prism);
                label.normal.textColor = new Color(0.68f, 1, 0.92f);
                GUI.Label(new Rect(slot.x + 6, slot.y + 115, slot.width - 12, 70), (dark.Count + 1) + " · the colours: a rainbow gift", label);
                if (TouchTargets.Button(slot, GUIContent.none, GUIStyle.none)) { picked = dark.Count; CheckpointAudio.PlayInterfaceId("ui.select"); }
            }
            // what the pick means, in plain words, then the confirm button
            if (info == null) { info = ArcadeTypography.Style(ArcadeTypography.SizeNote); info.alignment = TextAnchor.MiddleCenter; info.wordWrap = true; }
            info.normal.textColor = new Color(.18f, .14f, .1f);
            string meaning = picked >= 0 && picked < dark.Count
                ? "Relight this lantern: it burns again in every fight from now on, lighting its corner, and you remember " + ActOneCanon.LampHeld[dark[picked]] + "."
                : picked == dark.Count && colours ? "Leave the lanterns dark and choose one of three rainbow gifts now." : "Pick a lantern's memory or the colours to see what it gives.";
            GUI.Label(new Rect(card.x + 60, card.y + 372, card.width - 120, 70), meaning, info);
            bool ready = picked >= 0 && (picked < dark.Count || colours && picked == dark.Count);
            if (ArcadeSkin.Button(new Rect(card.center.x - 200, card.yMax - 128, 400, 88), ready ? (picked == dark.Count ? "TAKE THE COLOURS" : "RELIGHT") : "PICK ONE", KitButton.Primary, ready, info) && !CardPop.Closing("memory-return")) CardPop.CloseThen("memory-return", Confirm);
            GUI.color = popKeep;
            GUI.matrix = previous;
        }
    }

    public sealed partial class ArenaController
    {
        public bool ChooseRelight(int lamp) => CommitStory(() => session.ChooseRelight(lamp), "up.star2");
        public bool ChooseColours() => CommitStory(() => session.ChooseColours(), "up.rarity.gold");
        public bool SellMemory(string unitId) => CommitStory(() => session.SellMemory(unitId), "sig.stamp");
        public bool RefuseCustomer() => CommitStory(() => session.RefuseCustomer(), "ui.close");
        public bool SellToCustomer(string unitId) => CommitStory(() => session.SellToCustomer(unitId), "sig.stamp");
        public bool DeclineMemorySale() => CommitStory(() => session.DeclineMemorySale(), "ui.close");
        public bool SeeHome(string unitId) => CommitStory(() => session.SeeHome(unitId), "up.star2");
        public bool Ferry(string unitId, string newId) => CommitStory(() => session.Ferry(unitId, newId), "up.rarity.gold");
        /// <summary>Applies a story decision, saves it, and rebuilds the shop so bonds and lanterns reflect it.</summary>
        private bool CommitStory(System.Func<bool> decision, string cue)
        {
            if (PresentationPaused || TransitionPending) return false;
            RunSnapshot before = MergeReveal.Capture(session.State);
            if (!decision()) return false;
            if (!PersistBoundary()) { session.Restore(before); return false; }
            CheckpointAudio.PlayInterfaceId(cue, .15f);
            Vector2 position = simulation.Champion.Position;
            Begin(true);
            simulation.Champion.Position = simulation.Champion.PreviousPosition = position;
            return true;
        }
    }
}
