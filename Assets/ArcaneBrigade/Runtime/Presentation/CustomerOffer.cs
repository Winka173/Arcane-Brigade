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
        public int InputConsumedFrame { get; private set; } = -1;
        public void Configure(ArenaController value) => owner = value;
        /// <summary>While the offer stands, the shop does not also act on the traveller's presses.</summary>
        public bool Open => owner?.Session != null && owner.Session.CustomerOfferAvailable && !owner.StoryConsumesInput && !owner.TransitionPending && !owner.Session.SecretRoomOpen;

        private void Gather()
        {
            species.Clear();
            foreach (RosterEntry entry in owner.Session.State.Roster) if (!species.Contains(entry.UnitId) && System.Array.IndexOf(ActOneCanon.StoryIds, entry.UnitId) < 0) species.Add(entry.UnitId);
        }
        private void Update()
        {
            if (!Open || owner.PresentationPaused) return;
            if (!Application.isFocused && !RunPaths.Diagnostic) return;
            Gather();
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1)) { InputConsumedFrame = Time.frameCount; owner.RefuseCustomer(); return; }
            for (int i = 0; i < Mathf.Min(9, species.Count); i++)
                if (Input.GetKeyDown(KeyCode.Alpha1 + i) || Input.GetKeyDown(KeyCode.Keypad1 + i)) { InputConsumedFrame = Time.frameCount; owner.SellMemory(species[i]); return; }
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
            int shown = Mathf.Min(9, species.Count);
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
                UnitDefinition unit = owner.Content.Unit(species[i]);
                Rect slot = new Rect(card.x + (card.width - shown * 150) * 0.5f + i * 150 + 10, card.y + 245, 130, 150);
                ArcadeSkin.Plate(slot, ArcadeSkin.Card);
                if (unit != null) RunHud.Sprite(new Rect(slot.x + 15, slot.y + 8, 100, 90), unit.Body);
                name.normal.textColor = ArcadeSkin.Cream;
                GUI.Label(new Rect(slot.x + 4, slot.y + 100, slot.width - 8, 44), (i + 1) + " · " + (unit == null ? species[i] : RunUICopy.UnitName(unit)), name);
                if (GUI.Button(slot, GUIContent.none, GUIStyle.none)) { InputConsumedFrame = Time.frameCount; owner.SellMemory(species[i]); }
            }
            Rect refuse = new Rect(card.x + card.width * 0.5f - 210, card.yMax - 82, 420, 60);
            ArcadeSkin.Plate(refuse, ArcadeSkin.Teal);
            name.normal.textColor = ArcadeSkin.Cream;
            if (GUI.Button(refuse, "SAY NOTHING AND WALK ON  [Esc]", name)) { InputConsumedFrame = Time.frameCount; owner.RefuseCustomer(); }
            GUI.matrix = previous;
        }
    }

    public sealed partial class ArenaController
    {
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
