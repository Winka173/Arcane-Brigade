namespace ArcaneBrigade
{
    public static class WaitingRooms
    {
        public static string Art(int room)=>room==1?"Station":room==2?"PerfectRoom":room==3?"FootstepHall":"PerfectRoom";
        public static string Label(int room,int point)=>new[]{
            "PASSENGER REGISTER","DESTINATION BOARD","INSIDE RELEASE",
            "A MOVABLE CHAIR","THE SEALED WINDOW","THE THRESHOLD",
            "THE HEIGHT MARKS","THE EXTRA FOOTPRINTS","THE NOTE UNDER THE CHAIR",
            "THE UNOccupied CHAIR","THE WITHHELD REPLY","THE FIRST RELEASE"
        }[(room-1)*3+point].ToUpperInvariant();
        public static string Question(int room,int point)=>new[]{
            "A name was entered as cargo. The register will accept a traveller's own correction. Who should hold the amended record?",
            "The board promises arrival WHEN YOU ARE GOOD ENOUGH. Two real destinations can replace the condition. Which road will you mark first?",
            "The release can be held by a new keeper or made reachable from both sides. You will not appoint another owner. How will travellers find it?",
            "The room puts the chair back whenever somebody moves it. There is space for a second person only after the chair leaves its assigned place.",
            "The window shows a painted morning. Behind it is an actual night and somebody else's light. You can choose an honest view without pretending it is already dawn.",
            "The door narrows when your companions approach. The inside release works, but the room asks whether you will keep it for yourself.",
            "Several children left marks here. The extra steps use your old rhythm, but these heights do not all belong to you. What will the record preserve?",
            "You stop. The steps continue toward the door. Nothing here can damage you; this is the sound of waiting repeating itself. What response will you leave?",
            "The note reads I WILL BE QUIET UNTIL THEY COME. You may leave a place to rest or an address where a living person can answer.",
            "The chair held a voice when nobody answered. Its help happened. Its later claim happened too. You do not need to reduce it to one memory.",
            "The reply says somebody came on Thursday. The lock was forced from outside. The entity still calls the letter an attempt to abandon it.",
            "The release clause has no field for forgiveness. The entity asks you to choose how to explain your refusal. Both answers leave the handle in your hands."
        }[(room-1)*3+point];
        public static string Option(int room,int point,int option)=>new[]{
            "Let the passenger keep the original.|Deposit a consented copy in the public archive.",
            "Mark the coast and its verified address.|Mark the reed settlement and its verified address.",
            "Hang a key on both sides.|Write the release instruction above the lever.",
            "Move the chair beside the door.|Make a shared space at the table.",
            "Open the window to the real night.|Ask a companion which view they want.",
            "Leave the release visible to every guest.|Remove the latch that needs a keeper.",
            "Record every distinct mark without one explanation.|Leave space for the residents to annotate it.",
            "Listen, then state that the waiting may end.|Leave a light and walk to the open door.",
            "Leave a chair where rest is not a promise to stay.|Leave the public station's actual address.",
            "Keep the record of its care beside the record of harm.|Ask the witnesses to keep both accounts independently.",
            "Keep the complete reply in your own hand.|Place a copy where the visitor may find it.",
            "Name the harm. Refuse the claim.|Acknowledge the care. Refuse the claim."
        }[(room-1)*3+point].Split('|')[option-1];
        public static string Result(int room,int point,int option)
        {
            string[] first={"The passenger takes their own page. The cargo number is crossed out, still readable beneath the correction.","The board reads COAST / VERIFIED ROUTE. An arrival time appears.","Two reachable keys hang beside the open gate.","The chair remains where you moved it. The room has lost its assigned centre.","Real night replaces the painted morning. A light across the street answers.","The release stays visible. A guest can use it without an explanation.","The marks remain different. The hall can no longer call every child by your name.","The extra steps reach the door and stop. The silence is allowed to last.","A chair faces the open threshold. Its occupant will not owe it a lifetime.","The two records lie beside each other. Neither is allowed to erase the other.","You keep the complete reply. The entity cannot edit the paper in your hand.","You will name the withheld letter and the locked door. The refusal is enough."};
            string[] second={"The archive keeps a permitted copy. The passenger's name remains theirs to change.","The board reads REED SETTLEMENT / VERIFIED ROUTE. A driver signs the timetable.","The instruction is legible from either side. No keeper can hide the handle behind a title.","The chair turns toward a second place. Another cup appears without a price.","Your companion chooses the real night. The window stays open after they step away.","The latch comes away. The doorway no longer asks who is in charge.","Blank space stays beside the marks. A living witness can disagree with this account.","The trail now ends at your light rather than beneath a closed door.","The note names a place and a person who may answer. It does not promise they will always be there.","Two witnesses take separate copies. No sole custodian owns the story.","A complete copy waits outside the old lock. There is room for a reply to the reply.","You will acknowledge the care and refuse the captivity. Acknowledgement is not an agreement to remain."};
            return(option==1?first:second)[(room-1)*3+point];
        }
    }
}
