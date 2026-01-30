using System;

namespace ArcaneBrigade
{
    public sealed class WaitingCompanionRoute
    {
        public readonly string Id,Title;
        public readonly string[] Scenes,First,Second,Outcomes;
        public WaitingCompanionRoute(string id,string title,string[] scenes,string[] first,string[] second,string[] outcomes)
        {Id=id;Title=title;Scenes=scenes;First=first;Second=second;Outcomes=outcomes;}
    }
    public static class WaitingCompanionQuests
    {
        public static readonly int[] Waves={4,10,16};
        public static readonly WaitingCompanionRoute[] Routes={
            new WaitingCompanionRoute("TeapotPilgrim","A CUP WITHOUT A TASK",
                new[]{"They have served everybody. Their own cup is dry. 'I can wash up before I sit.'", "The station's serving bell rings. They stand even though it is somebody else's shift.","A resident asks who will serve tomorrow. The pilgrim looks at you before answering."},
                new[]{"Set a place; offer to sit together.","Take this shift while they rest.","Help organise a shared rota."},
                new[]{"Ask what they would like to drink.","Ask whether they want company saying no.","Help them open a kitchen with days off."},
                new[]{"They sit without receiving a task. Later they pull out a second chair for someone equally tired.","They choose their own tea. Their cup stays beside yours in the room; nobody measures how much they give.","They rest through a bell. At the station they teach another resident how to cover a shift without making a debt.","They say no while you stand beside them. The room's bell falls silent; they choose when to pour again.","A shared rota appears on the station wall. Their last line is a rest day with no replacement task.","The kitchen's sign reads CLOSED ON MORNINGS WE NEED. In the epilogue they invite you as a guest, never a customer."}),
            new WaitingCompanionRoute("HoodWisp","THE HAND THAT OPENED",
                new[]{"The old seal identifies them as a boundary guard. They cover your left side before remembering why.","The gate's record shows their signature on a refused passage. They ask to see the page.","A former prisoner approaches. They do not want an apology today. The wisp waits for their instruction."},
                new[]{"Ask to hear their own account.","Read the record beside them.","Help open the route the prisoner chooses."},
                new[]{"Agree to travel without demanding a confession now.","Let them take a copy and return when ready.","Stand witness while they give up the guard's key."},
                new[]{"They name the door they guarded. At the next crossing their warning uses that name instead of calling it protection.","They keep the seal visible. Their banter asks permission before taking the watch.","The signature stays in the public register. In the final room they open their left hand rather than hiding it.","They bring back an annotated copy. They correct it in their own words without asking you to erase the original.","A former prisoner selects the first open route. The wisp walks behind, following rather than directing.","The master key hangs outside the gate. Anybody can reach it; forgiveness is never the price of passage."}),
            new WaitingCompanionRoute("ShellPostman","THE ANSWER WAS ALREADY HERE",
                new[]{"The courier carries an opened envelope. The addressee has not consented to a public reading.","A register lists a reply as undeliverable. The paper bears a cut seam, not an absent address.","The courier can leave the complete correspondence in an archive or deliver it privately."},
                new[]{"Offer a private place to read.","Keep the torn seam with the register.","Help establish a public archive with permission."},
                new[]{"Ask the courier to hold it until they are ready.","Ask the courier to trace the visitor's route.","Carry the reply to its living addressee."},
                new[]{"The envelope remains closed until its reader chooses. A warm table is left available in the station.","The courier returns the envelope when asked. Keeping it safe does not grant the right to read it.","Paper and dated record are displayed together. The archivist cannot rename an intercepted answer a missing one.","A route is drawn from the outside of the old lock. The station board gains its first verifiable arrival.","The archive asks permission before copying a letter. The courier trains a successor who can refuse a delivery.","The reply reaches its addressee offstage after dawn. The courier writes a first letter on their own behalf."}),
            new WaitingCompanionRoute("PaperDevil","A ROAD WITHOUT ABSOLUTION",
                new[]{"They recognise a collector's route. They once led hunters down it. 'I cannot promise they will forgive me.'", "A witness needs the old route map. Giving it away will identify the tracker who drew it.","The survivors choose to open an exit. They have not invited the former tracker to their gathering."},
                new[]{"Ask which route they can make safe today.","Help deliver the unedited map.","Help repair the exit and respect the boundary."},
                new[]{"Ask to hear the survivor's account first.","Offer to accompany their public testimony.","Help leave tools for residents to finish themselves."},
                new[]{"They mark a safe route and accept that another guide will lead the survivors. Repair does not purchase trust.","They listen without interrupting. Their later account includes the parts that make them look worse.","The complete map reaches its owners. A sealed gate becomes a resident-controlled route in the station.","Their testimony names their own actions. You witness it; you cannot grant forgiveness for someone else.","They mend the outside hinge and leave before the gathering. A later invitation, if any, belongs to the survivors.","The tool case stays outside without a return address. At dawn they seek honest work rather than a clean history."}),
            new WaitingCompanionRoute("CarouselPony","A DESTINATION THEY CHOOSE",
                new[]{"An old ticket assigns them a rider and a circular route. They have never seen its destination field.","The timetable now offers two real places: the coast and the reed settlement. They want time to choose.","Passengers gather at the open road. The pony reads the first request and then looks beyond it."},
                new[]{"Ask them where they want to go.","Help test a short road to the coast.","Help them become a carrier with a right to refuse."},
                new[]{"Offer to walk beside them without riding.","Help test a short road to the reeds.","Walk with them on a journey carrying nobody."},
                new[]{"Their first destination is written in their own hand. The ticket names a traveller rather than a vehicle.","You walk. The empty saddle remains visible in the station; it no longer signals somebody missing.","Salt appears on the destination board. They return from the coast because they choose to bring its map.","A reed tune plays at the platform. They found a place they may visit without owing anybody a return.","The carriage notice says ASK FIRST. Their final passenger waits for a nod before mounting.","At dawn the saddle is empty on purpose. They take the first road simply because they want to see it."}),
            new WaitingCompanionRoute("SecretPatchBear","A NAME WITHOUT A CLAIM",
                new[]{"They find their old claim ticket. Its number is legible; the space for a name was never filled.","A resident offers to repair a seam. The bear has not asked for it to disappear.","The table's register asks how they wish to be introduced. Nobody reaches for the claim ticket."},
                new[]{"Ask which name they want you to use.","Ask which repair they would choose.","Write the name they have chosen."},
                new[]{"Leave the line blank until they choose.","Stand beside them while they decline repair.","Leave space for a name they may change."},
                new[]{"They choose a name for today. Your account records who said it, never who owns it.","An unnamed place remains at the table. Silence has stopped being treated as permission.","They choose contrasting thread. The mirror keeps the chosen seam rather than smoothing it away.","The needle is put down immediately. Their reflection remains whole with the old seam visible.","The register records a living guest. At dawn they answer to a name that was not awarded for being rescued.","The table has an erasable name card. They carry the pencil into morning; tomorrow is still theirs."})
        };
        public static string Futures(RunSnapshot s)
        {
            string text="";var c=s.WaitingCampaign?.CompanionChoices;
            if(c!=null && c.Length==18)for(int i=0;i<6;i++){int choice=c[i*3+2];if(choice>0)text+=Routes[i].Outcomes[4+choice-1]+"\n";}
            return text;
        }
        public static int Next(RunSnapshot s,int route)
        {var choices=s.WaitingCampaign?.CompanionChoices;for(int i=0;i<3;i++)if(choices==null || choices.Length==0 || choices[route*3+i]==0)return i;return -1;}
        public static string Consequences(RunSnapshot s,string id)
        {
            int r=Array.FindIndex(Routes,x=>x.Id==id); if(r<0)return "";
            var c=s.WaitingCampaign?.CompanionChoices; if(c==null || c.Length==0)return "";
            string text="";for(int i=0;i<3;i++)if(c[r*3+i]>0)text+="\n\n"+Routes[r].Outcomes[i*2+c[r*3+i]-1];return text;
        }
    }
}
