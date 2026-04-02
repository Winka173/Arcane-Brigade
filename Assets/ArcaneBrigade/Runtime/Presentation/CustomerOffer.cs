using System.Collections.Generic;
using UnityEngine;

namespace ArcaneBrigade
{
    /// <summary>Shop 8: the Last Customer waits at the end of the platform and offers a lantern for a memory of one of the
    /// traveller's companions (Docs/Mien_Cho_Act_One_Story.md). Selling makes that companion a stranger; refusing costs nothing.</summary>
    public sealed class CustomerOffer : MonoBehaviour
    {
        private ArenaController owner;
        private GUIStyle hand, plain, name;
        private readonly List<string> species = new List<string>();
        private int page; // nine companions per page; the arrows turn it
        public int InputConsumedFrame { get; private set; } = -1;
        public void Configure(ArenaController value) => owner = value;
        /// <summary>While the offer stands, the shop does not also act on the traveller's presses.</summary>
        public bool Open => owner?.Session != null && owner.Session.CustomerOfferAvailable && !owner.StoryConsumesInput && !owner.TransitionPending && !owner.Session.SecretRoomOpen;

        private void Gather()
        {
            species.Clear();
            foreach (RosterEntry entry in owner.Session.State.Roster) if (!species.Contains(entry.UnitId) && !RunSession.StorySpecies(entry.UnitId)) species.Add(entry.UnitId);
        }
        private void Update()
        {
            if (!Open || owner.PresentationPaused) return;
            if (!Application.isFocused && !RunPaths.Diagnostic) return;
            Gather();
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1)) { InputConsumedFrame = Time.frameCount; owner.RefuseCustomer(); return; }
            int pages = Mathf.Max(1, (species.Count + 8) / 9); page = Mathf.Clamp(page, 0, pages - 1);
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.JoystickButton5)) page = (page + 1) % pages;
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.JoystickButton4)) page = (page + pages - 1) % pages;
            for (int i = 0; i < Mathf.Min(9, species.Count - page * 9); i++)
                if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i)) { InputConsumedFrame = Time.frameCount; owner.SellMemory(species[page * 9 + i]); return; }
        }
        private void OnGUI()
        {
            if (!Open || owner.PresentationPaused || Camera.main == null) return;
            Gather();
            Matrix4x4 previous = ArcadeTypography.BeginViewport(Camera.main.pixelRect, 1920, 1080);
            if (hand == null)
            {
                hand = ArcadeTypography.Style(30); hand.font = ArcadeTypography.Hand; hand.alignment = TextAnchor.MiddleCenter; hand.wordWrap = true;
                plain = ArcadeTypography.Style(20); plain.alignment = TextAnchor.UpperCenter; plain.wordWrap = true;
                name = ArcadeTypography.Style(16, true); name.alignment = TextAnchor.MiddleCenter; name.wordWrap = true;
            }
            RunHud.Fill(new Rect(0, 0, 1920, 1080), new Color(0, 0, 0, 0.45f));
            int pages = Mathf.Max(1, (species.Count + 8) / 9); page = Mathf.Clamp(page, 0, pages - 1);
            int first = page * 9, shown = Mathf.Min(9, species.Count - first);
            float width = Mathf.Max(820, 120 + shown * 150);
            Rect card = new Rect((1920 - width) * 0.5f, 405, width, 500); // below the shopkeeper's speech bubble
            ArcadeSkin.Ticket(card, ArcadeSkin.Paper, ArcadeSkin.Wax);
            ArcadeSkin.Seal(new Vector2(card.xMax - 42, card.y + 40), 46, "$");
            hand.normal.textColor = new Color(.25f, .19f, .14f);
            GUI.Label(new Rect(card.x + 50, card.y + 26, card.width - 100, 120), "THE LAST CUSTOMER: 'Sell me your memory of one of them, and I'll light a lamp for you. You'd hardly miss it.'", hand);
            plain.normal.textColor = new Color(.18f, .14f, .1f);
            GUI.Label(new Rect(card.x + 50, card.y + 148, card.width - 100, 84), "The one you choose stays with you, but becomes a stranger: you forget who they are, and they no longer count toward bonds. One broken lantern burns again.", plain);
            for (int i = 0; i < shown; i++)
            {
                UnitDefinition unit = owner.Content.Unit(species[first + i]);
                Rect slot = new Rect(card.x + (card.width - shown * 150) * 0.5f + i * 150 + 10, card.y + 245, 130, 150);
                ArcadeSkin.Plate(slot, ArcadeSkin.Card);
                if (unit != null) RunHud.Sprite(new Rect(slot.x + 15, slot.y + 8, 100, 90), unit.Body);
                name.normal.textColor = ArcadeSkin.Cream;
                GUI.Label(new Rect(slot.x + 4, slot.y + 100, slot.width - 8, 44), (i + 1) + " · " + (unit == null ? species[first + i] : RunUICopy.UnitName(unit)), name);
                if (GUI.Button(slot, GUIContent.none, GUIStyle.none)) { InputConsumedFrame = Time.frameCount; owner.SellMemory(species[first + i]); }
            }
            if (pages > 1)
            {
                // more companions than fit: page through them
                Rect previousPage = new Rect(card.x + 20, card.y + 290, 44, 60), nextPage = new Rect(card.xMax - 64, card.y + 290, 44, 60);
                ArcadeSkin.Plate(previousPage, ArcadeSkin.Card); ArcadeSkin.Plate(nextPage, ArcadeSkin.Card);
                name.normal.textColor = ArcadeSkin.Cream;
                if (GUI.Button(previousPage, "◀", name)) page = (page + pages - 1) % pages;
                if (GUI.Button(nextPage, "▶", name)) page = (page + 1) % pages;
                plain.normal.textColor = new Color(.18f, .14f, .1f);
                GUI.Label(new Rect(card.x, card.y + 395, card.width, 22), (page + 1) + " / " + pages + "   ← →", plain);
            }
            Rect refuse = new Rect(card.x + card.width * 0.5f - 210, card.yMax - 82, 420, 60);
            ArcadeSkin.Plate(refuse, ArcadeSkin.Teal);
            name.normal.textColor = ArcadeSkin.Cream;
            if (GUI.Button(refuse, "SAY NOTHING AND WALK ON", name)) { InputConsumedFrame = Time.frameCount; owner.RefuseCustomer(); }
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
                if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i)) { owner.ChooseRelight(dark[i]); return; }
            if (owner.Session.ColoursAvailable && (Input.GetKeyDown(KeyCode.Alpha1 + dark.Count) || Input.GetKeyDown(KeyCode.Keypad1 + dark.Count))) owner.ChooseColours();
        }
        private void OnGUI()
        {
            if (!Open || owner.PresentationPaused || Camera.main == null) return;
            Gather();
            Matrix4x4 previous = ArcadeTypography.BeginViewport(Camera.main.pixelRect, 1920, 1080);
            if (hand == null)
            {
                hand = ArcadeTypography.Style(32); hand.font = ArcadeTypography.Hand; hand.alignment = TextAnchor.MiddleCenter; hand.wordWrap = true;
                plain = ArcadeTypography.Style(20); plain.alignment = TextAnchor.UpperCenter; plain.wordWrap = true;
                label = ArcadeTypography.Style(17, true); label.alignment = TextAnchor.MiddleCenter; label.wordWrap = true;
            }
            RunHud.Fill(new Rect(0, 0, 1920, 1080), new Color(0, 0, 0, 0.45f));
            bool colours = owner.Session.ColoursAvailable; int slots = dark.Count + (colours ? 1 : 0);
            float width = Mathf.Max(820, 120 + slots * 165);
            Rect card = new Rect((1920 - width) * 0.5f, 405, width, 440);
            ArcadeSkin.Ticket(card, ArcadeSkin.Paper, ArcadeSkin.Accent);
            hand.normal.textColor = new Color(.25f, .19f, .14f);
            GUI.Label(new Rect(card.x + 50, card.y + 24, card.width - 100, 90), "You stood up. Nobody paid. Outside, one of the dark lanterns is warm again.", hand);
            plain.normal.textColor = new Color(.18f, .14f, .1f);
            GUI.Label(new Rect(card.x + 50, card.y + 118, card.width - 100, 30), colours ? "Choose what comes back to you, or take the colours instead." : "Choose what comes back to you.", plain);
            for (int i = 0; i < dark.Count; i++)
            {
                int lamp = dark[i];
                Rect slot = new Rect(card.x + (card.width - slots * 165) * 0.5f + i * 165 + 8, card.y + 165, 150, 190);
                ArcadeSkin.Plate(slot, ArcadeSkin.Card);
                if (icons[lamp] != null) RunHud.Sprite(new Rect(slot.x + 25, slot.y + 10, 100, 100), icons[lamp]);
                else ArcadeSkin.Glow(new Rect(slot.x + 35, slot.y + 20, 80, 80), new Color(ArcadeSkin.Gold.r, ArcadeSkin.Gold.g, ArcadeSkin.Gold.b, 0.5f));
                label.normal.textColor = ArcadeSkin.Cream;
                GUI.Label(new Rect(slot.x + 6, slot.y + 115, slot.width - 12, 70), (i + 1) + " · " + ActOneCanon.LampHeld[lamp], label);
                if (GUI.Button(slot, GUIContent.none, GUIStyle.none)) owner.ChooseRelight(lamp);
            }
            if (colours)
            {
                Rect slot = new Rect(card.x + (card.width - slots * 165) * 0.5f + dark.Count * 165 + 8, card.y + 165, 150, 190);
                ArcadeSkin.Plate(slot, ArcadeSkin.Card);
                Sprite prism = PacingArtwork.Prism(owner.Content);
                if (prism != null) RunHud.Sprite(new Rect(slot.x + 25, slot.y + 10, 100, 100), prism);
                label.normal.textColor = new Color(0.68f, 1, 0.92f);
                GUI.Label(new Rect(slot.x + 6, slot.y + 115, slot.width - 12, 70), (dark.Count + 1) + " · the colours: a rainbow gift", label);
                if (GUI.Button(slot, GUIContent.none, GUIStyle.none)) owner.ChooseColours();
            }
            GUI.matrix = previous;
        }
    }

    public sealed partial class ArenaController
    {
        public bool ChooseRelight(int lamp) => CommitStory(() => session.ChooseRelight(lamp), "up.star2");
        public bool ChooseColours() => CommitStory(() => session.ChooseColours(), "up.rarity.gold");
        public bool SellMemory(string unitId) => CommitStory(() => session.SellMemory(unitId), "sig.stamp");
        public bool RefuseCustomer() => CommitStory(() => session.RefuseCustomer(), "ui.close");
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
